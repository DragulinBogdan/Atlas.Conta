using Atlas.Conta.BackOffice.Module.BusinessObjects;
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
// ═══ Ce e propriu feliei: DIRECȚIA conduce culegerea (geamăn cu LDI) ═══
// Fiecare linie e un CONSUM sau un PRODUS, iar cele două n-au aceleași câmpuri.
// Reconcilierea aplică deci două contracte diferite pe aceeași frunză:
//   * CONSUM — descarcă un lot EXISTENT: se aplică pinul `LotId`, iar câmpurile
//     produsului (produs, preț de evaluare, atributele lotului) se GOLESC
//     (F6-D3, aplicat pe ASM). Golirea e PERSISTATĂ, nu doar ignorată („inert
//     devine adevărat, nu doar afirmat"): un produs rămas pe linie din starea de
//     produs l-ar citi validarea de coerență Tip↔lot și ar naște lot-artefact.
//   * PRODUS — NAȘTE lotul din `ProdusId`, prin `LoturiCulegereService`, în
//     gestiunea în care se ASAMBLEAZĂ (predatorul — hook-ul
//     `Asamblare.GestiuneLoturiCulese`, F19-D3). `LotId` din payload se IGNORĂ:
//     e server-owned.
// Gardul de direcție trăiește în SERVICIU (`ILinieCareNasteLot.NasteLot`), nu
// aici: culegerea golește câmpurile, serviciul curăță lotul.
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
        doc.Data = dto.Data;
        // NAVIGAȚIA, nu FK-ul scalar (ca peste tot): rezolvarea validează
        // existența cu mesaj de domeniu, iar pe o entitate urmărită navigația
        // încărcată ar rescrie la fixup un FK setat direct. TIPUL laturilor
        // (ambele Gestiune) rămâne invariant al OPERĂRII.
        doc.Predator = GasesteRepartitor(os, dto.PredatorId, "Predatorul (gestiunea în care se asamblează)");
        doc.Primitor = GasesteRepartitor(os, dto.PrimitorId, "Primitorul (gestiunea care primește)");

        ReconciliazaLinii(os, doc, dto.Linii ?? new List<AsmLinieWriteDto>());

        // Seam-ul de culegere al loturilor (F2-D1, generalizat la F5-D3): naște
        // lotul liniei de produs din `ProdusId`, în gestiunea PREDATORULUI, și
        // curăță lotul propriu al liniilor care nu mai nasc (consumul — gardul
        // `NasteLot`). Pinul liniei de consum rămâne NEATINS (gardul de lot
        // străin).
        LoturiCulegereService.Sincronizeaza(os, doc);

        // Valoarea liniei, materializată ABIA ACUM: pe consum formula are nevoie
        // de pinul rămas după gard (vezi `MaterializeazaValori`).
        MaterializeazaValori(os, doc);

        os.CommitChanges();
        return doc.ID;
    }

    // Ștergerea agregatului. Pre-check de DOMENIU pe Draft (gardianul de
    // Committing rămâne plasa), apoi curățenia loturilor NĂSCUTE LA CULEGERE ale
    // liniilor care dispar (loturile produselor). Ordinea contează: `CurataOrfane`
    // citește `GetObjectsToDelete`, deci trebuie să vadă ștergerile DEJA marcate,
    // dar înaintea commit-ului. Loturile FINALIZATE de motor nu se ating
    // niciodată — inclusiv lotul pinuit de o linie de consum, care nici măcar nu
    // e al liniilor de aici.
    //
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
        LoturiCulegereService.CurataOrfane(os);
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
            detaliu.TipMaterial = Rezolva.Cere<TipMaterial>(os, l.TipMaterialId, "Tipul (contul/clasa)");

            // Scara numerică (49e) e gard la construirea MODELULUI: o valoare în
            // afara coloanei ar ieși ca DbUpdateException brută din Postgres.
            // Semnul NU se verifică: culegerea e pozitivă prin contract, iar
            // cantitatea unei linii deja operate e semnată — `ValideazaOperare`
            // cere doar „≠ 0", și nu inventăm un refuz peste el.
            VerificaScara(l.Cantitate, Scara.Cantitate, "Cantitatea");
            detaliu.Cantitate = l.Cantitate;

            if (detaliu.Directie == DirectieAsamblare.Consum) {
                // Câmpurile PRODUSULUI se golesc — persistat, nu doar ignorat.
                // Navigația ȘI FK-ul scalar: fixup-ul EF nu are voie să reînvie
                // referința dintr-o navigație încă încărcată.
                detaliu.Produs = null;
                detaliu.ProdusId = null;
                detaliu.PretEvaluare = null;
                detaliu.DataExpirare = null;
                detaliu.LotFabricatie = null;
                // Pinul lotului consumat — singura direcție pe care `LotId` se
                // aplică.
                if (l.LotId is Guid lotId) {
                    detaliu.Lot = Rezolva.Cere<Lot>(os, lotId, "Lotul");
                }
                else {
                    detaliu.Lot = null;
                    detaliu.LotId = null;
                }
            }
            else {
                // Produsul e mecanismul lotului (F19-D3): îl consumă
                // `LoturiCulegereService` după reconciliere.
                if (l.ProdusId is Guid produsId) {
                    detaliu.Produs = Rezolva.Cere<Produs>(os, produsId, "Produsul");
                }
                else {
                    detaliu.Produs = null;
                    detaliu.ProdusId = null;
                }
                if (l.PretEvaluare is decimal pret)
                    VerificaScara(pret, Scara.Pret, "Prețul de evaluare");
                detaliu.PretEvaluare = l.PretEvaluare;
                detaliu.DataExpirare = l.DataExpirare;
                detaliu.LotFabricatie = l.LotFabricatie;
                // `LotId` din PAYLOAD nu se aplică: pe produs lotul e
                // server-owned, îl gestionează serviciul de culegere. Valoarea
                // trimisă e ecoul ReadDto-ului, nu o intenție a operatorului.
                //
                // Ce se atinge totuși: pinul rămas pe linie dintr-o stare
                // ANTERIOARĂ de CONSUM. Oglinda exactă a golirii de mai sus, și
                // tot din motivul ei — „inert devine adevărat, nu doar afirmat"
                // (F6-D3). Pe ASM linia de produs trebuie să-și DEȚINĂ lotul
                // (`ValideazaOperare`: „lotul unei linii de produs se naște pe
                // linia însăși, nu se refolosește"), iar gardul de lot STRĂIN din
                // `LoturiCulegereService` refuză să nască unul cât linia referă
                // lotul altcuiva. Fără ruptura de aici, comutarea
                // Produs → Consum → Produs lăsa documentul PERMANENT ne-operabil:
                // linia rămânea pinuită pe lotul consumului, nu năștea niciodată
                // lot propriu, iar refuzul venea abia la operare, cu un mesaj pe
                // care operatorul nu-l poate acționa din ecran (`Lot` e
                // read-only/server-owned pe direcția Produs). MĂSURAT în
                // ModelCheck (`E2E-API-ASM`, comutarea în ambele sensuri).
                //
                // Se rupe DOAR referința străină: lotul PROPRIU al liniei rămâne
                // (altfel fiecare PUT ar naște altul), inclusiv unul finalizat de
                // o operare anterioară.
                if (detaliu.LotId is Guid pin) {
                    var lotPin = os.GetObjectByKey<Lot>(pin);
                    if (lotPin == null || lotPin.LinieIntrareId != detaliu.ID) {
                        detaliu.Lot = null;
                        detaliu.LotId = null;
                    }
                }
            }

            // Angajamentul de pe BAZĂ (frunza ASM n-are dimensiuni proprii) — pe
            // NAVIGAȚIE, ca restul FK-urilor.
            detaliu.Angajament = Nomenclator<Angajament>(os, l.AngajamentId, "Angajamentul");
            if (l.AngajamentId == null) detaliu.AngajamentId = null;
        }

        var sterse = existente.Values.Where(d => !pastrate.Contains(d.ID)).ToList();
        if (sterse.Count > 0)
            os.Delete(sterse);
    }

    // Valoarea liniei, materializată LA CULEGERE (GATE 53c: operatorul vede
    // diferența invariantului înainte de operare) — de aceea `Valoare` nu e în
    // WriteDto.
    //
    // Formula e GEAMĂNA lui `Asamblare.PregatesteOperare` (F19-D8), care o
    // rescrie la operare, cu o singură deosebire deliberată: `Cantitate` NU se
    // atinge aici. Semnarea cantității e fapta OPERĂRII (28a) — culegerea o ține
    // pozitivă, ca UI-ul. Valoarea, în schimb, e SEMNATĂ de pe acum: `Total`-ul
    // draftului ASM trebuie să fie exact diferența invariantului 46d (0 ⇔
    // echilibrat — F19-D9). De aceea formula folosește `Math.Abs(Cantitate)`
    // explicit: e idempotentă și pe un document re-cules după operare + anulare,
    // unde linia poartă deja cantitatea semnată.
    //
    // Rulează DUPĂ `Sincronizeaza`: abia atunci linia de produs are lotul născut
    // (nu contează pentru formulă — produsul se evaluează la `PretEvaluare` —
    // dar consumul depinde de pinul rămas după gard).
    //
    // Doar FRUNZELE (`OfType`), ca în hook: liniile de tip BAZĂ ale ASM-urilor
    // istorice/importate n-au direcție și n-au de unde lua un preț, iar
    // rescrierea valorii lor ar fi pierdere tăcută de dată contabilă reală.
    //
    // Ce NU face: nu prezice golirea (D18-D2). Regula golirii e a MOTORULUI, pe
    // cheia și semnul REGULII de stoc (75a) — culegerea rămâne previzualizarea
    // `preț × cantitate`, iar diferența dintre ele e exact problema pe care o
    // rezolvă comanda `DistribuieValoarea` de mai jos.
    static void MaterializeazaValori(IObjectSpace os, Asamblare doc) {
        foreach (var d in doc.Detalii.OfType<AsamblareDetaliu>()) {
            // Liniile marcate spre ștergere în acest commit nu se mai ating.
            if (os.IsObjectToDelete(d))
                continue;
            if (d.Directie == DirectieAsamblare.Consum) {
                // Consumul se evaluează la prețul lotului DESCĂRCAT. Fără lot
                // (draft incomplet — operarea îl va refuza) valoarea se golește:
                // valoarea veche, a lotului scos de pe linie, ar minți pe ecran.
                var lot = d.LotId is Guid lotId ? os.GetObjectByKey<Lot>(lotId) : null;
                d.Valoare = lot != null
                    ? Scara.RotunjesteBani(-Math.Abs(d.Cantitate) * lot.PretUnitar)
                    : 0m;
            }
            else if (d.Directie == DirectieAsamblare.Produs) {
                // Produsul se evaluează la prețul CULES (lotul nou se naște cu
                // el; validarea de operare îl cere pozitiv).
                d.Valoare = Scara.RotunjesteBani(Math.Abs(d.Cantitate) * (d.PretEvaluare ?? 0m));
            }
        }
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
        // REALIZATĂ — cea pe care o va scrie `MaterializeazaValori` și, la
        // operare, `PregatesteOperare`.
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
        // Valorile se rescriu prin ACEEAȘI funcție ca la culegere (nu din
        // `realizate`): dacă cele două ar diverge vreodată, verificarea de mai jos
        // o prinde ÎNAINTE de commit.
        MaterializeazaValori(os, doc);
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

    // Gardul de scară: `numeric(18, s)` ⇒ cel mult `s` zecimale și `18 − s` cifre
    // întregi. Aceeași formă pentru toate cele trei scări ale modelului (49e).
    static void VerificaScara(decimal valoare, int scara, string rol) {
        if (decimal.Round(valoare, scara) != valoare)
            throw new OperareException($"{rol} acceptă cel mult {scara} zecimale.");
        var limita = 1m;
        for (var i = 0; i < Scara.Precizie - scara; i++)
            limita *= 10m;
        if (Math.Abs(valoare) >= limita)
            throw new OperareException(
                $"{rol} depășește intervalul suportat ({Scara.Precizie - scara} cifre întregi).");
    }

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
                d.ID, d.Numar, d.Data, d.Stare, d.DataOperare,
                d.PredatorId, PredatorDenumire = d.Predator.Denumire,
                d.PrimitorId, PrimitorDenumire = d.Primitor.Denumire
            })
            .FirstOrDefault();
        if (h == null)
            return null;

        // Citirea liniilor merge pe BAZA detaliului, cu frunza adusă prin `as`
        // (TPT ⇒ LEFT JOIN în SQL): ASM-urile ISTORICE (importul 1C) poartă linii
        // de tip BAZĂ, iar pe frunză singură ar fi ieșit `Linii: []` cu `Total`
        // nenul. NULLABLE EXPLICIT pe TOATE valorile frunzei — inclusiv pe
        // `Directie`: pe o linie de bază cast-ul dă null, iar un enum
        // non-nullable ar pica la materializare.
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

        // Affordance ONESTĂ pe stingeri: ASM nu e creanță și n-ar trebui să poarte
        // imperecheri, dar gardianul motorului (`VerificaFaraImperecheri`) e
        // generic pe `Document` — dacă totuși există un link, refuzul se ARATĂ.
        var faraImperecheri = !ApiProiectii.AreImperecheri(os, id);
        var draft = h.Stare == StareDocument.Draft;

        return new AsmReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
            Total = total,
            SumaConsum = sumaConsum, SumaProdus = sumaProdus, Diferenta = sumaProdus - sumaConsum,
            PoateEdita = draft,
            PoateOpera = draft,
            PoateAnula = h.Stare == StareDocument.Operat && faraImperecheri,
            PoateStorna = h.Stare == StareDocument.Operat && faraImperecheri,
            PoateDistribui = draft
                && linii.Any(l => l.Directie == DirectieAsamblare.Consum)
                && linii.Any(l => l.Directie == DirectieAsamblare.Produs),
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
