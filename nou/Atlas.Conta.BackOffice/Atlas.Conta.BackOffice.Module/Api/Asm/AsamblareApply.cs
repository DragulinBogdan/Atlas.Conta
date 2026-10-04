using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Culegere;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Asm;

// Felia ASM: reconcilierea agregatului (scriere) + proiecțiile plate (citire) +
// comanda `distribuie-valoarea` (F19-D4). ZERO ASP.NET aici — controllerul din
// host e transport, iar ModelCheck exersează exact același cod pe
// `EFCoreObjectSpaceProvider` standalone.
//
// CONTRACT DE APELANT: `Aplica`/`Sterge` rulează în ObjectSpace-ul SECURED al
// apelantului (endpoint-ul de scriere) și COMIT. Gardianul de Committing e
// ultima autoritate — pre-check-ul de Draft există ca mesajul să fie al
// DOMENIULUI și ca refuzul să vină înaintea oricărei modificări de stare.
// `DistribuieValoarea` e o COMANDĂ: rulează pe ușa NON-SECURED (58c — scrie
// `PretEvaluare`/`Valoare` printr-un serviciu, nu prin culegere) și își cere un
// AL DOILEA ObjectSpace, de unică folosință, pentru predicție.
//
// Culegerea (golirea câmpurilor celeilalte direcții, ruperea pinului străin,
// loturi, valori) e a `CulegereDocument` (104c). Consumul descarcă lotul pinuit;
// produsul își naște lotul din `ProdusId`, iar `LotId` din payload se ignoră.
public static class AsamblareApply {

    // ═══════════════════════ Scriere ═══════════════════════

    // `id` null = creare; altfel actualizare. Întoarce ID-ul documentului
    // (puntea 42b: entitățile nu traversează granița, cheile da).
    public static Guid Aplica(IObjectSpace os, Guid? id, AsmWriteDto dto) {
        if (dto == null)
            throw new OperareException("Lipsește corpul cererii.");

        Asamblare doc;
        if (id is Guid existentId) {
            doc = Rezolva.Cere<Asamblare>(os, existentId, "Asamblarea");
            if (doc.Stare != StareDocument.Draft)
                throw new OperareException(
                    $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se mai modifică. "
                    + "Anulați operarea sau stornați-l.");
        }
        else {
            doc = os.CreateObject<Asamblare>();
        }

        // `Numar` NU se atinge (F19-D6): seria „ASM-" e server-owned, asignată la
        // MATERIALIZARE, în propria operare (53b).
        DocumentApply.AplicaDate(doc, dto.Data, dto.DataInregistrare);
        // NAVIGAȚIA, nu FK-ul scalar (ca peste tot): rezolvarea validează
        // existența cu mesaj de domeniu, iar pe o entitate urmărită navigația
        // încărcată ar rescrie la fixup un FK setat direct. TIPUL laturilor
        // (ambele Gestiune) rămâne invariant al OPERĂRII.
        doc.Predator = GasesteRepartitor(os, dto.PredatorId, "Predatorul (gestiunea în care se asamblează)");
        doc.Primitor = GasesteRepartitor(os, dto.PrimitorId, "Primitorul (gestiunea care primește)");

        ReconciliazaLinii(os, doc, dto.Linii ?? new List<AsmLinieWriteDto>());
        CulegereDocument.InainteDeSalvare(os);
        os.CommitChanges();
        return doc.ID;
    }

    // Pre-check de DOMENIU pe Draft (gardianul de Committing rămâne plasa).
    // Fără refuzul pe `Autogenerat`: ASM nu e niciodată artefactul unei operări
    // (nu e țintă de `PoliticaConex` și niciun tip nu-l produce ca secundar).
    public static void Sterge(IObjectSpace os, Guid id) {
        var doc = Rezolva.Cere<Asamblare>(os, id, "Asamblarea");
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException(
                $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se șterge. "
                + "Anulați operarea sau stornați-l.");

        os.Delete(doc.Detalii.ToList());
        os.Delete(doc);
        CulegereDocument.InainteDeSalvare(os);
        os.CommitChanges();
    }

    // Reconcilierea server-side a colecției (42d): upsert pe `Id`, delete pe
    // liniile dispărute din payload. Clientul trimite agregatul ÎNTREG.
    static void ReconciliazaLinii(IObjectSpace os, Asamblare doc, List<AsmLinieWriteDto> linii) {
        // Mulțimea de referință e `Detalii` ÎNTREG, nu doar frunzele ASM: un ASM
        // istoric/importat poate purta linii de tip BAZĂ (importul 1C le-a scris
        // ca atare), iar payload-ul e adevărul agregatului — reconcilierea
        // trebuie să le vadă, ca să le poată șterge.
        var existente = doc.Detalii.ToDictionary(d => d.ID);
        var pastrate = new HashSet<Guid>();

        foreach (var l in linii) {
            // Pe liniile EXISTENTE tipul se judecă ÎNAINTEA parse-ului de
            // direcție (lecția F6-M4): o linie de tip BAZĂ iese din ReadDto cu
            // `Directie` null, iar parse-ul ar refuza-o cu mesajul de enum în
            // locul celui acționabil de mai jos.
            AsamblareDetaliu detaliu;
            CulegereDocument.Amprenta? inainte = null;
            if (l.Id is Guid linieId) {
                if (!existente.TryGetValue(linieId, out var existenta))
                    throw new OperareException(
                        $"Linia {linieId} nu aparține documentului {Eticheta(doc)}.");
                // Un Id repetat în payload ar suprascrie tăcut prima apariție.
                if (!pastrate.Add(linieId))
                    throw new OperareException($"Linia {linieId} apare de două ori în cerere.");
                detaliu = existenta as AsamblareDetaliu
                    ?? throw new OperareException(
                        $"Linia {linieId} nu e o linie de asamblare (tip vechi) — ștergeți-o din document "
                        + "și culegeți-o din nou.");
                inainte = CulegereDocument.Urmareste(os, doc, detaliu);
                detaliu.Directie = ApiEnum.DirectieAsm(l.Directie);
            }
            else {
                // Parse-ul enumerării ÎNAINTE de `CreateObject`: direcția decide
                // tot ce urmează, iar un refuz după creare ar lăsa o linie orfană
                // în ObjectSpace-ul viu.
                var directie = ApiEnum.DirectieAsm(l.Directie);
                detaliu = os.CreateObject<AsamblareDetaliu>();
                detaliu.Document = doc;
                detaliu.Directie = directie;
            }
            ApiLinie.TipMaterial(os, detaliu, l.TipMaterialId);
            detaliu.Cantitate = l.Cantitate;

            if (l.ProdusId is Guid produsId) {
                detaliu.Produs = Rezolva.Cere<Produs>(os, produsId, "Produsul");
            }
            else {
                detaliu.Produs = null;
                detaliu.ProdusId = null;
            }
            detaliu.PretEvaluare = l.PretEvaluare;
            detaliu.DataExpirare = l.DataExpirare;
            detaliu.LotFabricatie = l.LotFabricatie;

            // Pe produs lotul e server-owned; `LotId` din payload e doar ecoul citirii.
            if (detaliu.Directie == DirectieAsamblare.Consum) {
                if (l.LotId is Guid lotId) {
                    detaliu.Lot = Rezolva.Cere<Lot>(os, lotId, "Lotul");
                }
                else {
                    detaliu.Lot = null;
                    detaliu.LotId = null;
                }
            }

            // Angajamentul de pe BAZĂ (frunza ASM n-are dimensiuni proprii) — pe
            // NAVIGAȚIE, ca restul FK-urilor.
            detaliu.Angajament = Nomenclator<Angajament>(os, l.AngajamentId, "Angajamentul");
            if (l.AngajamentId == null) detaliu.AngajamentId = null;

            CulegereDocument.Mapata(os, doc, detaliu, inainte, null);
        }

        var sterse = existente.Values.Where(d => !pastrate.Contains(d.ID)).ToList();
        if (sterse.Count > 0)
            os.Delete(sterse);
    }

    // ═══════════════════════ F19-D4: distribuirea valorii consumului ═══════════════════════
    //
    // PROBLEMA (restanța 75-r1). Din D18-D2 consumul care GOLEȘTE cheia de stoc
    // preia tot soldul valoric rămas pe ea, nu `preț × cantitate`. Operatorul
    // care evaluează produsul la `preț lot × cantitate` primește refuz pe
    // invariantul 46d cu un rest de cenți și n-are NICIO cale să nimerească
    // cifra din ecran (prețul lotului are 6 zecimale, restul e al acumulării
    // rotunjirilor de pe ieșirile anterioare). Fără mecanismul de mai jos ecranul
    // ASM ar fi o capcană — de aia felia livrează comanda, nu doar formularul.
    //
    // CE FACE. Rescrie `PretEvaluare` pe liniile de PRODUS astfel încât
    // `Σ Valoare(produse) == Σ |Valoare(consumuri)|` EXACT (nu „în toleranță":
    // invariantul are 0,005, dar o comandă care lasă cenți pe masă ar fi tot o
    // capcană, cu un pas mai departe).
    //
    // PREDICȚIA. Valoarea consumurilor NU se recalculează aici: se cere
    // MOTORULUI, prin `MotorOperare.Valideaza` (dry-run) pe un ObjectSpace de
    // UNICĂ FOLOSINȚĂ. Dry-run-ul rulează exact fazele de calcul ale operării —
    // `PregatesteOperare` (semnare + `preț × cantitate`) urmat de
    // `StocService.AplicaValoareIesire` (regula golirii, pe cheia și semnul
    // REGULII de stoc) — și se oprește înainte de materializare. Deci cifra pe
    // care o citim de pe liniile lui e, la cent, cifra pe care operarea o va
    // scrie: NU există aici o a doua formulă a golirii, nici o a doua potrivire
    // de reguli de stoc. Erorile dry-run-ului (inclusiv chiar invariantul 46d, pe
    // care tocmai îl reparăm) se IGNORĂ deliberat — ne interesează valorile
    // calculate, nu verdictul; ce nu se poate ignora e ca faza de calcul să NU fi
    // rulat, și asta se vede structural (vezi `PrezicSumaConsum`).
    //
    // ObjectSpace-ul predicției e OBLIGATORIU altul decât cel al comenzii:
    // `PregatesteOperare` SEMNEAZĂ cantitățile pe linii (contractul lui
    // `MotorOperare.Valideaza`), iar un commit peste ele ar lăsa draftul cu
    // cantități negative culese.
    //
    // PREDICȚIA E A MOMENTULUI. Se citește registrul de stoc AȘA CUM E ACUM. Dacă
    // între distribuire și operare se schimbă ceva ce mișcă golirea (alt document
    // golește lotul primul, o anulare readuce cantitate, se schimbă data
    // documentului), invariantul 46d refuză la operare — cu AMBELE sume, ca azi.
    // NU încercăm să prevenim asta (ar cere blocarea lotului între două cereri
    // HTTP, adică exact concurența parcată în 25f): reparația e re-rularea
    // comenzii, iar refuzul motorului rămâne autoritatea.
    //
    // Idempotentă: a doua rulare pe același document, cu același registru, dă
    // aceleași cifre — cheia de repartizare devine chiar valorile scrise de prima
    // rulare, iar prețul e normalizat ca funcție a valorii finale (vezi mai jos).
    public static AsmDistribuireDto DistribuieValoarea(IObjectSpace os, Func<IObjectSpace> fabricaPredictie, Guid id) {
        var doc = Rezolva.Cere<Asamblare>(os, id, "Asamblarea");
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException(
                $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — valoarea nu se mai distribuie.");

        var vii = doc.Detalii.Where(d => !os.IsObjectToDelete(d)).ToList();
        var frunze = vii.OfType<AsamblareDetaliu>().ToList();
        if (frunze.Count != vii.Count)
            throw new OperareException(
                "Documentul poartă linii de tip vechi (fără rol de asamblare) — ștergeți-le și culegeți-le din nou "
                + "înainte de a distribui valoarea.");
        var consumuri = frunze.Where(d => d.Directie == DirectieAsamblare.Consum).ToList();
        var produse = frunze.Where(d => d.Directie == DirectieAsamblare.Produs).ToList();
        if (consumuri.Count == 0)
            throw new OperareException("Asamblarea n-are nicio linie de consum — nu există valoare de distribuit.");
        if (produse.Count == 0)
            throw new OperareException("Asamblarea n-are nicio linie de produs — n-are pe ce distribui valoarea.");

        // Pre-check-uri de DOMENIU, înaintea predicției: pe un draft incomplet
        // cifra prezisă s-ar schimba imediat ce linia se completează, iar
        // operatorul ar rămâne cu prețuri care par bune și nu sunt. Cantitatea 0
        // e refuzată oricum de tip (28e) și e împărțitorul de mai jos.
        foreach (var c in consumuri) {
            if (c.Cantitate == 0m)
                throw new OperareException("O linie de consum are cantitatea zero — completați-o înainte de distribuire.");
            if (c.LotId == null)
                throw new OperareException(
                    "O linie de consum n-are lot ales — valoarea consumului nu se poate prezice fără lot "
                    + "(alegeți lotul, apoi distribuiți).");
        }
        foreach (var p in produse)
            if (p.Cantitate == 0m)
                throw new OperareException("O linie de produs are cantitatea zero — completați-o înainte de distribuire.");

        decimal tinta;
        using (var osPredictie = fabricaPredictie())
            tinta = PrezicSumaConsum(osPredictie, id);
        if (tinta <= 0m)
            throw new OperareException(
                $"Valoarea prezisă a consumului e {tinta:N2} — nu se poate distribui pe produse "
                + "(prețul de evaluare al unei linii de produs trebuie să fie pozitiv).");

        // CHEIA DE REPARTIZARE (F19-D4): proporțional cu valoarea CULEASĂ a
        // fiecărei linii de produs; dacă NICIUNA n-are valoare culeasă,
        // proporțional cu cantitatea. O singură linie de produs = tot consumul.
        var cantitati = produse.Select(p => Math.Abs(p.Cantitate)).ToList();
        var greutati = produse.Select(p => Scara.RotunjesteBani(Math.Abs(p.Cantitate) * (p.PretEvaluare ?? 0m))).ToList();
        var totalGreutati = greutati.Sum();
        if (totalGreutati <= 0m) {
            greutati = cantitati;
            totalGreutati = greutati.Sum();
        }
        // Cazul MIXT (o linie evaluată, alta nu) nu are cheie de repartizare
        // onestă: ponderea 0 ar da preț 0, adică exact refuzul „preț de evaluare
        // pozitiv" al operării, mutat cu un pas mai încolo. Se REFUZĂ, cu ce are
        // operatorul de făcut — nu se inventează o a treia cheie.
        if (greutati.Any(g => g <= 0m))
            throw new OperareException(
                "Unele linii de produs au preț de evaluare cules și altele nu — cheia de repartizare ar da 0,00 pe "
                + "cele fără. Completați un preț orientativ pe fiecare linie de produs (sau ștergeți-le pe toate) "
                + "și distribuiți din nou.");

        // Repartizarea pe bani: ultima linie ia RESTUL, ca Σ părților să fie
        // exact ținta chiar dacă fiecare cotă s-a rotunjit în jos.
        var parti = new decimal[produse.Count];
        var atribuit = 0m;
        for (var i = 0; i < produse.Count - 1; i++) {
            parti[i] = Scara.RotunjesteBani(tinta * greutati[i] / totalGreutati);
            atribuit += parti[i];
        }
        parti[^1] = tinta - atribuit;
        if (parti.Any(p => p <= 0m))
            throw new OperareException(
                "Repartizarea ar lăsa o linie de produs cu 0,00 — cantitățile sau prețurile culese sunt prea "
                + "disproporționate pentru valoarea consumului.");

        // Prețul: `Round(parte / q, 6)` (scara prețurilor, 49e), apoi valoarea
        // REALIZATĂ — cea pe care o scrie `CalculeazaValori`, la culegere și la operare.
        var preturi = new decimal[produse.Count];
        var realizate = new decimal[produse.Count];
        for (var i = 0; i < produse.Count; i++) {
            preturi[i] = Scara.RotunjestePret(parti[i] / cantitati[i]);
            realizate[i] = Scara.RotunjesteBani(cantitati[i] * preturi[i]);
        }

        // REZIDUUL DE BAN se plimbă: diferența dintre țintă și Σ realizate se
        // pune pe linia care o poate ABSORBI, adică aceea pe care există un preț
        // de 6 zecimale care dă exact valoarea nouă. Se încearcă în ordinea
        // cantității CRESCĂTOARE: cu cât cantitatea e mai mică, cu atât un pas de
        // 1e-6 pe preț mișcă valoarea mai puțin, deci grila valorilor realizabile
        // e mai fină.
        var reziduu = tinta - realizate.Sum();
        var plimbat = reziduu;
        if (reziduu != 0m) {
            foreach (var i in Enumerable.Range(0, produse.Count)
                         .OrderBy(i => cantitati[i]).ThenBy(i => i)) {
                var nou = realizate[i] + reziduu;
                if (nou <= 0m)
                    continue;
                var pret = Scara.RotunjestePret(nou / cantitati[i]);
                if (Scara.RotunjesteBani(cantitati[i] * pret) != nou)
                    continue;
                preturi[i] = pret;
                realizate[i] = nou;
                reziduu = 0m;
                break;
            }
        }
        // LIMITA 75-r4, DECLARATĂ, nu ascunsă: pe cantități mari grila valorilor
        // realizabile devine mai groasă decât banul (q = 1.000.000 ⇒ un pas de
        // 1e-6 pe preț mișcă valoarea cu 1,00 leu), deci niciun preț reprezentabil
        // nu stinge un reziduu de 0,01. Refuzăm cu CIFRA, în loc să lăsăm un ASM
        // pe care operarea îl va refuza oricum, fără să spună de ce.
        if (reziduu != 0m)
            throw new OperareException(
                $"Valoarea consumului ({tinta:N2}) nu se poate distribui exact: rămâne un reziduu de {reziduu:N2} pe "
                + "care nicio linie de produs nu-l poate absorbi (la cantitățile astea un pas de preț de 0,000001 "
                + "mișcă valoarea cu mai mult de un ban). Ajustați cantitățile sau spargeți asamblarea.");

        // NORMALIZAREA prețului ca funcție a valorii FINALE — condiția
        // idempotenței: la a doua rulare cheia de repartizare e chiar valoarea
        // scrisă acum, deci prețul recalculat din ea trebuie să fie ACELAȘI.
        // (`Round(v/q, 6)` e cel mai apropiat preț de pe grilă, iar `v` e
        // realizabil prin construcție, deci verificarea trece; condiționarea e
        // plasa, nu o ramură așteptată.)
        for (var i = 0; i < produse.Count; i++) {
            var canonic = Scara.RotunjestePret(realizate[i] / cantitati[i]);
            if (Scara.RotunjesteBani(cantitati[i] * canonic) == realizate[i])
                preturi[i] = canonic;
        }

        for (var i = 0; i < produse.Count; i++)
            produse[i].PretEvaluare = preturi[i];
        // Valorile vin din culegere, nu din `realizate`; o divergență iese la verificarea de mai jos.
        CulegereDocument.Normalizeaza(os, doc);
        var sumaProdus = produse.Sum(p => p.Valoare);
        if (sumaProdus != tinta)
            throw new OperareException(
                $"Distribuirea n-a închis invariantul: produse {sumaProdus:N2} față de consum {tinta:N2}.");

        os.CommitChanges();
        return new AsmDistribuireDto {
            SumaConsum = tinta,
            SumaProdus = sumaProdus,
            ReziduuPlimbat = plimbat,
            Document = Citeste(os, id)
        };
    }

    // Cifra pe care o vor scrie consumurile la operare, cerută MOTORULUI.
    //
    // `MotorOperare.Valideaza` rulează, în ordine: gardul de stare, gardul de
    // perioadă, `PregatesteOperare` (semnează cantitățile și pune `preț ×
    // cantitate`), potrivirea regulilor de stoc + `StocService.AplicaValoareIesire`
    // (regula golirii D18-D2), abia apoi validările. Erorile lui nu ne
    // interesează — le va spune operarea; ne interesează VALORILE.
    //
    // Ce trebuie totuși deosebit: cazul în care faza de calcul NU s-a executat
    // (perioadă închisă, tip de document lipsă din seed — refuzuri care cad
    // ÎNAINTE de `PregatesteOperare`). Se vede STRUCTURAL, fără să ghicim din
    // textul erorilor: `PregatesteOperare` al ASM semnează consumurile la
    // `−Abs(Cantitate)`, iar apelantul a verificat deja că nicio cantitate nu e 0
    // ⇒ dacă vreun consum a rămas cu cantitate pozitivă, calculul n-a rulat.
    static decimal PrezicSumaConsum(IObjectSpace osPredictie, Guid id) {
        var doc = Rezolva.Cere<Asamblare>(osPredictie, id, "Asamblarea");
        var erori = MotorOperare.Valideaza(osPredictie, doc);
        var consumuri = doc.Detalii.OfType<AsamblareDetaliu>()
            .Where(d => d.Directie == DirectieAsamblare.Consum).ToList();
        if (consumuri.Count == 0 || consumuri.Any(d => d.Cantitate >= 0m))
            throw new OperareException(
                "Valoarea consumului nu se poate prezice — motorul se oprește înaintea calculului"
                + (erori.Count > 0 ? ":\n" + string.Join("\n", erori) : "."));
        // Convenția frunzei: consumurile poartă valori NEGATIVE (F19-D8), deci
        // magnitudinea e `−Σ`.
        return -consumuri.Sum(d => d.Valoare);
    }

    static T Nomenclator<T>(IObjectSpace os, Guid? id, string rol)
            where T : class => Rezolva.Optional<T>(os, id, rol);

    static Repartitor GasesteRepartitor(IObjectSpace os, Guid id, string rol) =>
        Rezolva.Cere<Repartitor>(os, id, rol);

    static string Eticheta(Document doc) =>
        string.IsNullOrWhiteSpace(doc.Numar) ? $"({doc.Data:dd.MM.yyyy})" : doc.Numar;

    // ═══════════════════════ Citire ═══════════════════════
    //
    // Proiecții PLATE (42c): `Select` înainte de materializare, niciun membru
    // [NotMapped] și nicio navigație enumerată în afara query-ului (25b).

    // `null` dacă documentul nu există, nu e vizibil (pe ușa securizată cele
    // două nu se disting — F22-D1, apelantul le traduce în același 404)
    // sau nu e o asamblare.
    public static AsmReadDto Citeste(IObjectSpace os, Guid id) {
        var h = os.GetObjectsQuery<Asamblare>()
            .Where(d => d.ID == id)
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.DataInregistrare, d.Stare, d.DataOperare,
                d.PredatorId, PredatorDenumire = d.Predator.Denumire,
                d.PrimitorId, PrimitorDenumire = d.Primitor.Denumire
            })
            .FirstOrDefault();
        if (h == null)
            return null;

        // Pe BAZA detaliului: liniile de tip bază (import, istoric) apar în `Linii`, cu valorile frunzei null.
        // `as` nu filtrează pe tip; sigur fiindcă liniile unui document sunt frunza lui sau baza (F28-H, 89).
        // Valorile frunzei sunt nullable explicit: pe o linie de bază vin null, iar un tip valoare ar pica la materializare.
        var linii = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => l.DocumentId == id)
            .OrderBy(l => l.ID)
            .Select(l => new {
                l.ID,
                Directie = (DirectieAsamblare?)(l as AsamblareDetaliu).Directie,
                l.TipMaterialId,
                TipMaterialCod = l.TipMaterial.Cod,
                TipMaterialDenumire = l.TipMaterial.Denumire,
                ProdusId = (l as AsamblareDetaliu).ProdusId,
                ProdusCod = (l as AsamblareDetaliu).Produs.Cod,
                ProdusDenumire = (l as AsamblareDetaliu).Produs.Denumire,
                l.LotId,
                LotProdus = l.Lot.Produs.Denumire,
                LotData = (DateOnly?)l.Lot.Data,
                LotPret = (decimal?)l.Lot.PretUnitar,
                l.Cantitate,
                PretEvaluare = (l as AsamblareDetaliu).PretEvaluare,
                l.Valoare,
                DataExpirare = (l as AsamblareDetaliu).DataExpirare,
                LotFabricatie = (l as AsamblareDetaliu).LotFabricatie,
                l.AngajamentId, AngajamentCod = l.Angajament.Cod
            })
            .ToList();

        // `Total` se agregă pe BAZA detaliului (definiția `Document.Total`), ca să
        // dea EXACT ce dă `Lista` chiar dacă documentul poartă linii de tip bază.
        var total = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => l.DocumentId == id)
            .Sum(l => (decimal?)(l.Valoare + l.ValoareTva)) ?? 0m;

        // Invariantul 46d, calculat SERVER-SIDE (F19-D9): magnitudini pozitive,
        // ca pe hârtie. Liniile de tip BAZĂ n-au direcție, deci nu intră în
        // niciuna dintre sume — pe un document curat `Total == Diferenta`.
        var sumaConsum = -linii.Where(l => l.Directie == DirectieAsamblare.Consum).Sum(l => l.Valoare);
        var sumaProdus = linii.Where(l => l.Directie == DirectieAsamblare.Produs).Sum(l => l.Valoare);

        var regim = RegimDocument.Calculeaza(os, id);

        return new AsmReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            DataInregistrare = h.DataInregistrare,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
            Total = total,
            SumaConsum = sumaConsum, SumaProdus = sumaProdus, Diferenta = sumaProdus - sumaConsum,
            PoateEdita = regim.Editabil,
            PoateOpera = regim.Poate(ComandaDocument.Opereaza),
            Corectie = ApiProiectii.Corectie(os, id),
            PoateAnula = regim.Poate(ComandaDocument.AnuleazaOperarea),
            PoateStorna = regim.Poate(ComandaDocument.Storneaza),
            PoateDistribui = regim.Poate(RegimDocument.Distribuie),
            Linii = linii.Select(l => new AsmLinieReadDto {
                Id = l.ID,
                Directie = l.Directie?.ToString(),
                TipMaterialId = l.TipMaterialId,
                TipMaterialCod = l.TipMaterialCod, TipMaterialDenumire = l.TipMaterialDenumire,
                ProdusId = l.ProdusId, ProdusCod = l.ProdusCod, ProdusDenumire = l.ProdusDenumire,
                LotId = l.LotId,
                LotEticheta = ApiProiectii.EtichetaLot(l.LotProdus, l.LotData, l.LotPret),
                Cantitate = l.Cantitate, PretEvaluare = l.PretEvaluare, Valoare = l.Valoare,
                DataExpirare = l.DataExpirare, LotFabricatie = l.LotFabricatie,
                AngajamentId = l.AngajamentId, AngajamentCod = l.AngajamentCod
            }).ToList()
        };
    }

    // `IQueryable` — DataSourceLoader îi pune deasupra filtrarea/sortarea/
    // paginarea clientului (43c). `Total` prin JOIN PE AGREGAT, nu subquery
    // corelat (42c), pe BAZA detaliului.
    public static IQueryable<AsmListDto> Lista(IObjectSpace os) {
        var totaluri = os.GetObjectsQuery<DocumentDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new { DocumentId = g.Key, Total = g.Sum(x => x.Valoare + x.ValoareTva) });

        return from d in os.GetObjectsQuery<Asamblare>()
               join t in totaluri on d.ID equals t.DocumentId into agregat
               from t in agregat.DefaultIfEmpty()
               select new AsmListDto {
                   Id = d.ID,
                   Numar = d.Numar,
                   Data = d.Data,
                   // Enum → string ÎN SQL (`CASE`): filtrarea și sortarea rămân
                   // server-side, deși pe sârmă starea e text.
                   Stare = d.Stare == StareDocument.Draft ? "Draft"
                       : d.Stare == StareDocument.Operat ? "Operat"
                       : "Stornat",
                   PredatorDenumire = d.Predator.Denumire,
                   PrimitorDenumire = d.Primitor.Denumire,
                   Total = (decimal?)t.Total ?? 0m
               };
    }
}
