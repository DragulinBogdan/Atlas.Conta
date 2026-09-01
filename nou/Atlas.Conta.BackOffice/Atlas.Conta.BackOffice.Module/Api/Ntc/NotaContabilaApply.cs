using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using Atlas.Conta.BackOffice.Module.Proiectii;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Ntc;

// Felia NTC: reconcilierea agregatului (scriere) + proiecțiile plate (citire) +
// panoul de compensare. ZERO ASP.NET aici — controllerul din host e transport,
// iar ModelCheck exersează exact același cod pe `EFCoreObjectSpaceProvider`
// standalone (precedentul: motorul — docs 113709).
//
// CONTRACT DE APELANT: `Aplica`/`Sterge` rulează în ObjectSpace-ul SECURED al
// apelantului (endpoint-ul de scriere) și COMIT. Gardianul de Committing e
// ultima autoritate — pre-check-ul de Draft există ca mesajul să fie al
// DOMENIULUI și ca refuzul să vină înaintea oricărei modificări de stare.
//
// ═══ Ce face Apply pe NTC în MINUS față de toate celelalte felii ═══
// NIMIC nu se materializează. Nota n-are lanț de valori (F19-D8): `Valoare` e
// culeasă, nu calculată; n-are TVA de calculat (`PoliticaTva` lipsește în ambele
// profiluri, F19-D7); n-are loturi de născut sau de curățat (`ILinieCareNasteLot`
// nu e declarată); n-are cantitate pro-forma de normalizat. Rămâne maparea
// câmpurilor + reconcilierea colecției — și e corect că e atât: orice „ajutor"
// în plus ar fi un al doilea adevăr față de ce postează motorul.
//
// ═══ Ce NU atinge reconcilierea pe o linie EXISTENTĂ ═══
// `Cantitate`, `LotId`, `TipTvaId`, `ValoareTva`, `AngajamentId` — câmpuri de
// bază fără semantică pe notă, pe care payload-ul feliei nici nu le poartă
// (F19-D7). Notele ISTORICE de import le pot avea completate; un PUT care le-ar
// goli „ca să fie curat" ar RESCRIE date pe care operatorul nu le-a văzut și nu
// le-a cerut. Sunt oricum inerte: fără reguli de stoc nimic nu citește
// `Cantitate`/`Lot`, iar fără `PoliticaTva` `RegistruTvaService` nu scrie niciun
// rând pentru ele (riscul 9 al contractului).
public static class NotaContabilaApply {

    // ═══════════════════════ Scriere ═══════════════════════

    // `id` null = creare; altfel actualizare. Întoarce ID-ul documentului
    // (puntea 42b: entitățile nu traversează granița, cheile da).
    public static Guid Aplica(IObjectSpace os, Guid? id, NtcWriteDto dto) {
        if (dto == null)
            throw new OperareException("Lipsește corpul cererii.");

        NotaContabila doc;
        if (id is Guid existentId) {
            doc = os.GetObjectByKey<NotaContabila>(existentId)
                ?? throw new OperareException($"Nota contabilă {existentId} nu există.");
            RefuzaInchidereaTva(doc);
            // Pre-check de DOMENIU: gardianul (`GardianEditare`) ar prinde oricum
            // la commit, dar abia după ce am rescris header-ul și liniile în
            // ObjectSpace-ul viu, cu mesajul lui generic. Aici oprim din prima.
            if (doc.Stare != StareDocument.Draft)
                throw new OperareException(
                    $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se mai modifică. "
                    + "Anulați operarea sau stornați-l.");
        }
        else {
            doc = os.CreateObject<NotaContabila>();
        }

        // `Numar` NU se atinge (F19-D6): seria „NTC-" e server-owned, asignată la
        // MATERIALIZARE, în propria operare (GATE XAF D6) — gardianul de
        // Committing o și păzește pe tipurile cu politică de numerotare.
        doc.Data = dto.Data;
        // NAVIGAȚIA, nu FK-ul scalar (ca peste tot): rezolvarea validează
        // existența cu mesaj de domeniu, iar pe o entitate urmărită navigația
        // încărcată ar rescrie la fixup un FK setat direct. TIPUL laturilor
        // (repartitori INTERNI) rămâne invariant al OPERĂRII.
        doc.Predator = GasesteRepartitor(os, dto.PredatorId, "Predatorul (unitatea internă)");
        doc.Primitor = GasesteRepartitor(os, dto.PrimitorId, "Primitorul (unitatea internă)");

        ReconciliazaLinii(os, doc, dto.Linii ?? new List<NtcLinieWriteDto>());

        os.CommitChanges();
        return doc.ID;
    }

    // Ștergerea agregatului. Pre-check de DOMENIU pe Draft (gardianul de
    // Committing rămâne plasa).
    //
    // FĂRĂ `LoturiCulegereService.CurataOrfane` (ca BCS/DEC): liniile de notă nu
    // nasc loturi, deci curățenia n-ar avea ce căuta — un apel ar fi inofensiv,
    // dar mincinos. FĂRĂ refuz pe `Autogenerat`: nota nu e artefactul unei
    // operări (închiderea de TVA are tipul ei, ITV).
    public static void Sterge(IObjectSpace os, Guid id) {
        var doc = os.GetObjectByKey<NotaContabila>(id)
            ?? throw new OperareException($"Nota contabilă {id} nu există.");
        RefuzaInchidereaTva(doc);
        if (doc.Stare != StareDocument.Draft)
            throw new OperareException(
                $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se șterge. "
                + "Anulați operarea sau stornați-l.");

        os.Delete(doc.Detalii.ToList());
        os.Delete(doc);
        os.CommitChanges();
    }

    // Reconcilierea server-side a colecției (42d): upsert pe `Id`, delete pe
    // liniile dispărute din payload. Clientul trimite agregatul ÎNTREG.
    static void ReconciliazaLinii(IObjectSpace os, NotaContabila doc, List<NtcLinieWriteDto> linii) {
        // Mulțimea de referință e `Detalii` ÎNTREG, nu doar frunzele NTC: o notă
        // importată/istorică poate purta linii de tip BAZĂ (motorul le refuză la
        // operare — 38c), iar payload-ul e adevărul agregatului, deci
        // reconcilierea trebuie să le vadă ca să le poată ȘTERGE.
        var existente = doc.Detalii.ToDictionary(d => d.ID);
        var pastrate = new HashSet<Guid>();

        foreach (var l in linii) {
            NotaContabilaDetaliu detaliu;
            if (l.Id is Guid linieId) {
                if (!existente.TryGetValue(linieId, out var existenta))
                    throw new OperareException(
                        $"Linia {linieId} nu aparține documentului {Eticheta(doc)}.");
                // Un Id repetat în payload ar suprascrie tăcut prima apariție.
                if (!pastrate.Add(linieId))
                    throw new OperareException($"Linia {linieId} apare de două ori în cerere.");
                detaliu = existenta as NotaContabilaDetaliu
                    ?? throw new OperareException(
                        $"Linia {linieId} nu e o linie de notă contabilă (tip vechi) — ștergeți-o din document "
                        + "și culegeți-o din nou.");
            }
            else {
                detaliu = os.CreateObject<NotaContabilaDetaliu>();
                detaliu.Document = doc;
            }

            detaliu.TipMaterial = os.GetObjectByKey<TipMaterial>(l.TipMaterialId)
                ?? throw new OperareException($"Tipul (contul/clasa) {l.TipMaterialId} nu există.");
            detaliu.Descriere = l.Descriere;

            // Postarea explicită pe linie (32a) — trăsătura tipului. Toate patru
            // nullable pe sârmă; conturile sunt obligatorii, dar refuzul e al
            // OPERĂRII (`NotaContabila.ValideazaOperare`), nu al culegerii: un
            // draft incomplet e legitim, iar o a doua sursă a aceleiași reguli ar
            // diverge tăcut (42a).
            detaliu.ContDebit = Nomenclator<Cont>(os, l.ContDebitId, "Contul debitor al liniei");
            if (l.ContDebitId == null) detaliu.ContDebitId = null;
            detaliu.ContCredit = Nomenclator<Cont>(os, l.ContCreditId, "Contul creditor al liniei");
            if (l.ContCreditId == null) detaliu.ContCreditId = null;
            detaliu.RepartitorDebit = Nomenclator<Repartitor>(os, l.RepartitorDebitId,
                "Repartitorul debitor al liniei");
            if (l.RepartitorDebitId == null) detaliu.RepartitorDebitId = null;
            detaliu.RepartitorCredit = Nomenclator<Repartitor>(os, l.RepartitorCreditId,
                "Repartitorul creditor al liniei");
            if (l.RepartitorCreditId == null) detaliu.RepartitorCreditId = null;

            // Dimensiunea frunzei (DIM-2), pe NAVIGAȚIE ca restul FK-urilor.
            detaliu.CodEconomic = Nomenclator<CodEconomic>(os, l.CodEconomicId, "Codul economic");
            if (l.CodEconomicId == null) detaliu.CodEconomicId = null;

            // Scara numerică (49e) e gard la construirea MODELULUI, nu a valorii:
            // o valoare în afara coloanei ar ieși ca DbUpdateException brută din
            // Postgres. Refuzăm cu mesaj de domeniu (ca FCT/NIR/LDI/DEC).
            VerificaScara(l.Valoare, Scara.Bani, "Valoarea");
            // CULEASĂ ca atare — inclusiv negativă (F19-D8). Zero îl refuză tipul.
            detaliu.Valoare = l.Valoare;
        }

        var sterse = existente.Values.Where(d => !pastrate.Contains(d.ID)).ToList();
        if (sterse.Count > 0)
            os.Delete(sterse);
    }

    // ═══ ITV nu e o notă contabilă a acestei felii (F21-D5) ═══
    //
    // Sub TPT, `InchidereTva : NotaContabila` — deci `GetObjectByKey<NotaContabila>`
    // ÎNTOARCE și închiderile de TVA, iar până la felia 21 un draft ITV se putea
    // rescrie prin `PUT api/ntc/{id}` (reconcilierea acceptă orice linii; gardianul
    // anti-stale l-ar fi prins abia la OPERARE) sau șterge prin `DELETE`. Nu e o
    // notă pe care operatorul o culege: liniile ei sunt calculate din soldurile
    // registrului, iar „modificarea" ei e REGENERAREA, din ecranul propriu.
    //
    // `is` aici e la GRANIȚĂ, în Apply, nu în motor — regula „motorul nu cunoaște
    // frunzele" nu e atinsă. Comenzile (`opereaza`/`anuleaza`/`storneaza`) rămân
    // PERMISE pe ruta NTC: sunt `OperareApi` pe `Document`, agnostic la tip, și
    // produc exact același rezultat ca pe ruta ITV — un al doilea gard acolo ar fi
    // fost o regulă fără miză.
    static void RefuzaInchidereaTva(NotaContabila doc) {
        if (doc is InchidereTva)
            throw new OperareException(
                $"Documentul {Eticheta(doc)} e o închidere de TVA — se gestionează din ecranul ei (/itv).");
    }

    static T Nomenclator<T>(IObjectSpace os, Guid? id, string rol) where T : class {
        if (id == null)
            return null;
        return os.GetObjectByKey<T>(id.Value)
            ?? throw new OperareException($"{rol} ({id}) nu există în nomenclator.");
    }

    static Repartitor GasesteRepartitor(IObjectSpace os, Guid id, string rol) =>
        os.GetObjectByKey<Repartitor>(id)
            ?? throw new OperareException($"{rol} ({id}) nu există în nomenclatorul de repartitori.");

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

    // `null` dacă documentul nu există (sau nu e o notă contabilă).
    public static NtcReadDto Citeste(IObjectSpace os, Guid id) {
        var h = os.GetObjectsQuery<NotaContabila>()
            // F21-D5: închiderea de TVA are felia ei. EF traduce `is` pe TPT
            // printr-un test pe frunză, deci filtrul rămâne server-side, iar
            // `GET api/ntc/{id}` pe un ITV întoarce 404, nu o reprezentare
            // parțială a unui document pe care ecranul de notă nu-l poate scrie.
            .Where(d => !(d is InchidereTva))
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
        // (TPT ⇒ LEFT JOIN în SQL): notele ISTORICE/importate pot purta linii de
        // tip BAZĂ, iar pe frunză singură ar fi ieșit `Linii: []` cu `Total` nenul
        // (constatarea F5 pe NIR). NULLABLE EXPLICIT pe valorile frunzei — pe o
        // linie de bază cast-ul dă null.
        var linii = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => l.DocumentId == id)
            .OrderBy(l => l.ID)
            .Select(l => new {
                l.ID,
                l.TipMaterialId,
                TipMaterialCod = l.TipMaterial.Cod,
                TipMaterialDenumire = l.TipMaterial.Denumire,
                Descriere = (l as NotaContabilaDetaliu).Descriere,
                ContDebitId = (l as NotaContabilaDetaliu).ContDebitId,
                ContDebitSimbol = (l as NotaContabilaDetaliu).ContDebit.Simbol,
                ContDebitDenumire = (l as NotaContabilaDetaliu).ContDebit.Denumire,
                ContCreditId = (l as NotaContabilaDetaliu).ContCreditId,
                ContCreditSimbol = (l as NotaContabilaDetaliu).ContCredit.Simbol,
                ContCreditDenumire = (l as NotaContabilaDetaliu).ContCredit.Denumire,
                RepartitorDebitId = (l as NotaContabilaDetaliu).RepartitorDebitId,
                RepartitorDebitDenumire = (l as NotaContabilaDetaliu).RepartitorDebit.Denumire,
                RepartitorCreditId = (l as NotaContabilaDetaliu).RepartitorCreditId,
                RepartitorCreditDenumire = (l as NotaContabilaDetaliu).RepartitorCredit.Denumire,
                CodEconomicId = (l as NotaContabilaDetaliu).CodEconomicId,
                CodEconomicCod = (l as NotaContabilaDetaliu).CodEconomic.Cod,
                l.Valoare, l.ValoareTva
            })
            .ToList();

        // Affordance ONESTĂ pe stingeri (F3-D2/57d): pe NTC compensarea E cazul
        // normal al tipului (48b), deci refuzul gardianului de anulare/storno se
        // ARATĂ, nu se descoperă la apăsarea butonului.
        var faraImperecheri = !ApiProiectii.AreImperecheri(os, id);

        return new NtcReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
            // Definiția `Document.Total` (BRUT, pe BAZA detaliului) — aceeași
            // cifră în agregat și în listă, chiar dacă documentul poartă linii de
            // tip bază. Pe notă `ValoareTva` e 0 prin construcție.
            Total = linii.Sum(l => l.Valoare + l.ValoareTva),
            PoateEdita = h.Stare == StareDocument.Draft,
            PoateOpera = h.Stare == StareDocument.Draft,
            PoateAnula = h.Stare == StareDocument.Operat && faraImperecheri,
            PoateStorna = h.Stare == StareDocument.Operat && faraImperecheri,
            Linii = linii.Select(l => new NtcLinieReadDto {
                Id = l.ID,
                TipMaterialId = l.TipMaterialId,
                TipMaterialCod = l.TipMaterialCod, TipMaterialDenumire = l.TipMaterialDenumire,
                Descriere = l.Descriere,
                ContDebitId = l.ContDebitId, ContDebitSimbol = l.ContDebitSimbol,
                ContDebitDenumire = l.ContDebitDenumire,
                ContCreditId = l.ContCreditId, ContCreditSimbol = l.ContCreditSimbol,
                ContCreditDenumire = l.ContCreditDenumire,
                RepartitorDebitId = l.RepartitorDebitId,
                RepartitorDebitDenumire = l.RepartitorDebitDenumire,
                RepartitorCreditId = l.RepartitorCreditId,
                RepartitorCreditDenumire = l.RepartitorCreditDenumire,
                CodEconomicId = l.CodEconomicId, CodEconomicCod = l.CodEconomicCod,
                Valoare = l.Valoare
            }).ToList()
        };
    }

    // `IQueryable` — DataSourceLoader îi pune deasupra filtrarea/sortarea/
    // paginarea clientului (43c). `Total` prin JOIN PE AGREGAT, nu subquery
    // corelat (42c), pe BAZA detaliului.
    public static IQueryable<NtcListDto> Lista(IObjectSpace os) {
        var totaluri = os.GetObjectsQuery<DocumentDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new { DocumentId = g.Key, Total = g.Sum(x => x.Valoare + x.ValoareTva) });

        // F21-D5: fără filtru, lista notelor conținea și închiderile de TVA (TPT).
        return from d in os.GetObjectsQuery<NotaContabila>().Where(d => !(d is InchidereTva))
               join t in totaluri on d.ID equals t.DocumentId into agregat
               from t in agregat.DefaultIfEmpty()
               select new NtcListDto {
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

    // ═══ Panoul de compensare (F19-D10) ═══
    //
    // „Ce pot stinge cu nota asta" are n răspunsuri — unul per contrapartidă de
    // pe linii ȘI per SENS (F19-D16): jumătatea de debit stinge datorii,
    // jumătatea de credit stinge creanțe, iar plafonul se consumă separat pe
    // fiecare. Rândurile candidate sunt `DocumenteCuRest` FILTRATE pe acea
    // contrapartidă și pe acel sens: proiecția existentă, refolosită ca atare.
    // NTC nu se adaugă în ea (n-are semantică de „rest"), deci nota nu se poate
    // propune pe ea însăși.
    //
    // Cele două cifre ale plafonului vin din CELE DOUĂ funcții pe care le cheamă
    // și `ImperechereService.ValideazaCreare` (`CapacitateStingere` +
    // `AsignatFataDe`) — nu din formule refăcute aici. Consecință MĂSURATĂ în
    // ModelCheck: `Disponibil` e exact suma maximă pe care serviciul o acceptă
    // față de contrapartida respectivă (plafonată în plus de `Rest`-ul
    // documentului ales).
    //
    // `null` dacă documentul nu există (sau nu e o notă contabilă).
    public static NtcCandidatiDto Candidati(IObjectSpace os, Guid id) {
        // Plafon de pagină per contrapartidă, ca la orice listă (`Incarca`): pe
        // baza de import un partener poate avea sute de documente deschise.
        const int Plafon = 100;

        // F21-D5, a patra ușă a feliei: `GetObjectByKey<NotaContabila>` întoarce și
        // închiderile de TVA (TPT), deci panoul de compensare al notei răspundea 200
        // pe un id de ITV. Practic era inert (`CapacitateStingere` pe ITV iese
        // dicționar GOL — liniile n-au repartitori), dar un 200 pe o resursă care
        // nu e a feliei e o afirmație falsă: aceeași frunză, același null ca
        // `Citeste` ⇒ 404 pe `GET api/ntc/{id}/candidati`.
        var doc = os.GetObjectByKey<NotaContabila>(id);
        if (doc == null || doc is InchidereTva)
            return null;

        var rezultat = new NtcCandidatiDto {
            DocumentId = id,
            Stare = doc.Stare.ToString(),
            // Oglinda primului invariant al stingerii: ambele documente OPERATE.
            PoateStinge = doc.Stare == StareDocument.Operat
        };

        // Afordanța nu contrazice datele pe care le însoțește (review M3): pe un
        // draft nota NU stinge nimic (primul invariant al stingerii: ambele
        // documente operate), deci panoul nu întoarce plafoane și candidați
        // lângă un `PoateStinge: false`. Un panou complet cu buton „Stinge" pe
        // fiecare rând, urmat de refuzul serviciului la prima apăsare, e exact
        // „panoul promite mai mult decât acceptă serviciul" (riscul 2), doar pe
        // axa STĂRII în loc de a plafonului. `Stare`/`PoateStinge` rămân — ele
        // sunt răspunsul la „de ce e gol".
        if (!rezultat.PoateStinge)
            return rezultat;

        var capacitati = doc.CapacitateStingere(os);
        if (capacitati == null || capacitati.Count == 0)
            return rezultat;

        var ids = capacitati.Keys.ToList();
        var etichete = os.GetObjectsQuery<Repartitor>()
            .Where(r => ids.Contains(r.ID))
            .Select(r => new { r.ID, r.Cod, r.Denumire })
            .ToList()
            .ToDictionary(r => r.ID);

        foreach (var cheie in capacitati.Keys
                     .OrderBy(k => etichete.TryGetValue(k, out var e) ? e.Denumire : null)) {
            var plafon = capacitati[cheie];
            etichete.TryGetValue(cheie, out var eticheta);
            // Un rând per JUMĂTATE nenulă. `Datorie` întâi, ca în bilanț.
            foreach (var sens in new[] { SensStingere.Datorie, SensStingere.Creanta }) {
                var capacitate = plafon[sens];
                if (capacitate == 0m)
                    continue;
                var asignat = ImperechereService.AsignatFataDe(os, id, cheie, sens);
                // Ordinea de stingere: cele mai VECHI datorii/creanțe întâi (ordinea
                // în care le-ar lua un contabil), nu ordinea de inserare.
                var randuri = ImperecheriProiectii.DocumenteCuRest(os, cheie, sens)
                    .OrderBy(r => r.Data).ThenBy(r => r.Numar)
                    .Take(Plafon + 1)
                    .ToList();
                var maiSunt = randuri.Count > Plafon;
                if (maiSunt)
                    randuri.RemoveAt(randuri.Count - 1);
                rezultat.Contrapartide.Add(new NtcContrapartidaDto {
                    RepartitorId = cheie,
                    RepartitorCod = eticheta?.Cod,
                    RepartitorDenumire = eticheta?.Denumire,
                    Sens = sens.ToString(),
                    Capacitate = capacitate,
                    Asignat = asignat,
                    Disponibil = capacitate - asignat,
                    Candidati = randuri,
                    MaiSunt = maiSunt
                });
            }
        }
        return rezultat;
    }
}
