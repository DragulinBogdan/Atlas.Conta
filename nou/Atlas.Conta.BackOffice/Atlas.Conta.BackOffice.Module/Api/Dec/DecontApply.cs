using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Dec;

// Felia DEC: reconcilierea agregatului (scriere) + proiecțiile plate (citire).
// ZERO ASP.NET aici — controllerul din host e transport, iar ModelCheck
// exersează exact același cod pe `EFCoreObjectSpaceProvider` standalone
// (precedentul: motorul — docs 113709).
//
// CONTRACT DE APELANT: `Aplica`/`Sterge` rulează în ObjectSpace-ul SECURED al
// apelantului (endpoint-ul de scriere) și COMIT. Gardianul de Committing e
// ultima autoritate — pre-check-ul de Draft există ca mesajul să fie al
// DOMENIULUI și ca refuzul să vină înaintea oricărei modificări de stare.
//
// ═══ Ce face Apply pe DEC ═══
// Pe tierul API nu rulează NICIUN ViewController, deci seam-urile de culegere se
// apelează EXPLICIT, în ordinea din UI (șablonul FCT/FCL):
//   1. maparea câmpurilor culese (inclusiv postarea explicită pe linie);
//   2. normalizarea CANTITĂȚII pro-forma 0 → 1 (F8-D2) — la culegere, nu în
//      spate; `Decont.PregatesteOperare` o repetă idempotent la operare, pentru
//      calea XAF/import;
//   3. `TvaService.AplicaTipTvaImplicit` — doar pe liniile NOI fără TipTva în
//      payload (culegerea explicită, inclusiv golirea deliberată, bate default-ul);
//   4. `TvaService.CalculeazaLaCulegere` — `Valoare`/`ValoareTva` din
//      `PretUnitar × Cantitate`, CONDIȚIONAT de declanșatori, apoi override-ul
//      manual de `ValoareTva`, dacă vine.
// FĂRĂ seam de loturi: liniile de decont nu declară `ILinieCareNasteLot` și nu
// referă loturi — decontul justifică cheltuieli, nu mișcă stoc.
public static class DecontApply {

    // ═══════════════════════ Scriere ═══════════════════════

    // `id` null = creare; altfel actualizare. Întoarce ID-ul documentului
    // (puntea 42b: entitățile nu traversează granița, cheile da).
    public static Guid Aplica(IObjectSpace os, Guid? id, DecontWriteDto dto) {
        if (dto == null)
            throw new OperareException("Lipsește corpul cererii.");

        Decont doc;
        if (id is Guid existentId) {
            doc = Rezolva.Cere<Decont>(os, existentId, "Decontul");
            // Pre-check de DOMENIU: gardianul (`GardianEditare`) ar prinde oricum
            // la commit, dar abia după ce am rescris header-ul și liniile în
            // ObjectSpace-ul viu, cu mesajul lui generic. Aici oprim din prima.
            if (doc.Stare != StareDocument.Draft)
                throw new OperareException(
                    $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se mai modifică. "
                    + "Anulați operarea sau stornați-l.");
        }
        else {
            doc = os.CreateObject<Decont>();
        }

        // `Numar` NU se atinge (F8-D3): seria „DEC-" e server-owned, asignată la
        // MATERIALIZARE, în propria operare (GATE XAF D6) — gardianul de
        // Committing o și păzește pe tipurile cu politică de numerotare.
        doc.Data = dto.Data;
        // NAVIGAȚIA, nu FK-ul scalar (ca peste tot): rezolvarea validează
        // existența cu mesaj de domeniu, iar pe o entitate urmărită navigația
        // încărcată ar rescrie la fixup un FK setat direct. TIPUL laturilor
        // (Angajat → unitate internă) rămâne invariant al OPERĂRII.
        doc.Predator = GasesteRepartitor(os, dto.PredatorId, "Predatorul (titularul)");
        doc.Primitor = GasesteRepartitor(os, dto.PrimitorId,
            "Primitorul (unitatea internă care primește justificarea)");
        doc.NumarPV = dto.NumarPV;
        doc.DataPV = dto.DataPV;

        ReconciliazaLinii(os, doc, dto.Linii ?? new List<DecontLinieWriteDto>());

        os.CommitChanges();
        return doc.ID;
    }

    // Ștergerea agregatului. Pre-check de DOMENIU pe Draft (gardianul de
    // Committing rămâne plasa).
    //
    // FĂRĂ `LoturiCulegereService.CurataOrfane` (ca BCS, spre deosebire de
    // NIR/FCT/LDI): liniile de decont nu nasc loturi, deci curățenia n-ar avea ce
    // căuta — un apel ar fi inofensiv, dar mincinos.
    //
    // FĂRĂ refuzul pe `Autogenerat` (F5/F7): decontul nu e niciodată artefactul
    // unei operări — nu e țintă de `PoliticaConex` și niciun tip nu-l produce ca
    // secundar.
    public static void Sterge(IObjectSpace os, Guid id) {
        var doc = Rezolva.Cere<Decont>(os, id, "Decontul");
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
    static void ReconciliazaLinii(IObjectSpace os, Decont doc, List<DecontLinieWriteDto> linii) {
        // Mulțimea de referință e `Detalii` ÎNTREG, nu doar frunzele DEC: un
        // decont istoric/importat poate purta linii de tip BAZĂ, iar payload-ul e
        // adevărul agregatului — reconcilierea trebuie să le vadă, ca să le poată
        // șterge.
        var existente = doc.Detalii.ToDictionary(d => d.ID);
        var pastrate = new HashSet<Guid>();
        // Latura fiscală a tipului (F13-D1), rezolvată O SINGURĂ DATĂ pentru tot
        // agregatul: e proprietatea documentului, nu a liniei.
        var directieTva = TvaService.DirectiePentru(os, doc);

        foreach (var l in linii) {
            DecontDetaliu detaliu;
            var noua = false;
            if (l.Id is Guid linieId) {
                if (!existente.TryGetValue(linieId, out var existenta))
                    throw new OperareException(
                        $"Linia {linieId} nu aparține documentului {Eticheta(doc)}.");
                // Un Id repetat în payload ar suprascrie tăcut prima apariție.
                if (!pastrate.Add(linieId))
                    throw new OperareException($"Linia {linieId} apare de două ori în cerere.");
                detaliu = existenta as DecontDetaliu
                    ?? throw new OperareException(
                        $"Linia {linieId} nu e o linie de decont (tip vechi) — ștergeți-o din document "
                        + "și culegeți-o din nou.");
            }
            else {
                detaliu = os.CreateObject<DecontDetaliu>();
                detaliu.Document = doc;
                noua = true;
            }

            // Starea DE DINAINTEA mapării, pentru semantica recalculului de mai
            // jos: recalculul TVA se declanșează DOAR la schimbarea bazei
            // (Cantitate/PretUnitar) sau a TipTva — un PUT care nu le atinge NU
            // pierde override-ul de ValoareTva (regula F2, o singură sursă).
            var bazaVeche = noua ? 0m : detaliu.PretUnitar * detaliu.Cantitate;
            Guid? tipTvaVechi = noua ? null : detaliu.TipTvaId;

            detaliu.TipMaterial = Rezolva.Cere<TipMaterial>(os, l.TipMaterialId, "Tipul (contul/clasa)");
            detaliu.Descriere = l.Descriere;

            // Scara numerică (49e) e gard la construirea MODELULUI, nu a valorii:
            // o valoare în afara coloanei ar ieși ca DbUpdateException brută din
            // Postgres. Refuzăm cu mesaj de domeniu (ca FCT/NIR/LDI).
            VerificaScara(l.Cantitate, Scara.Cantitate, "Cantitatea");
            VerificaScara(l.PretUnitar, Scara.Pret, "Prețul unitar");
            // Cantitatea PRO-FORMA (32d, legacy BUC/1) — normalizată LA CULEGERE
            // (F8-D2): decontul se culege pe sumă, nu pe cantitate, iar un 0 lăsat
            // pe linie ar da `Valoare = 0` pe ecran și ar fi „reparat" abia în
            // spate, la operare. `PregatesteOperare` face același lucru,
            // idempotent, pentru calea XAF/import.
            detaliu.Cantitate = l.Cantitate == 0m ? 1m : l.Cantitate;
            detaliu.PretUnitar = l.PretUnitar;

            if (l.TipTvaId is Guid tipTvaId) {
                detaliu.TipTva = Rezolva.Cere<TipTva>(os, tipTvaId, "Tipul de TVA");
            }
            else {
                detaliu.TipTva = null;
                detaliu.TipTvaId = null;
            }

            // Dimensiunea frunzei (DIM-2) + angajamentul de pe BAZĂ — pe
            // NAVIGAȚIE, ca restul FK-urilor: existența se validează cu mesaj de
            // domeniu, nu cu violare de FK.
            detaliu.CodEconomic = Nomenclator<CodEconomic>(os, l.CodEconomicId, "Codul economic");
            if (l.CodEconomicId == null) detaliu.CodEconomicId = null;
            detaliu.Angajament = Nomenclator<Angajament>(os, l.AngajamentId, "Angajamentul");
            if (l.AngajamentId == null) detaliu.AngajamentId = null;

            // Postarea explicită pe linie (32a) — trăsătura PROPRIE a tipului.
            // Toate patru opționale: ce rămâne gol cade pe regula de contare.
            // Conturile SUMATOR nu se refuză aici (F8-D14): filtrul din lookup e
            // affordance, autoritatea rămâne motorul — nu se inventează o regulă
            // nouă de refuz în felia de transport.
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

            // Default-ul de TipTva al tipului de document (38d/37f) — DOAR pe
            // liniile noi al căror payload n-a dat un TipTva: pe o linie
            // existentă, golirea lui e decizie explicită a operatorului, iar
            // re-aplicarea default-ului ar face-o imposibilă.
            if (noua && l.TipTvaId == null)
                TvaService.AplicaTipTvaImplicit(os, doc, detaliu);

            // Lanțul de valori, materializat LA CULEGERE (GATE 53c, prin
            // aderarea F8-D2 la `ILinieCuPretUnitar`): operatorul confruntă bonul
            // înainte de operare. `PregatesteOperare` îl rescrie la operare din
            // aceeași formulă — de aceea `Valoare` nu e în WriteDto.
            var bazaNoua = detaliu.PretUnitar * detaliu.Cantitate;
            if (noua || bazaNoua != bazaVeche || detaliu.TipTvaId != tipTvaVechi)
                TvaService.CalculeazaLaCulegere(os, directieTva, detaliu, bazaNoua);
            // Override-ul operatorului, DUPĂ calcul (oglinda fluxului UI): bonul
            // justificat bate rotunjirea noastră (regula 36a, uniformizată prin
            // 48b pe FCT/FCL/DEC). La operare îl păstrează `pastreazaTvaCules`.
            if (l.ValoareTva is decimal valoareTva) {
                VerificaScara(valoareTva, Scara.Bani, "Valoarea TVA");
                // Aceleași două refuzuri ca pe FCT (F2-D1/D7), fără variantă a
                // treia: pe Capitalizat TVA-ul e deja în `Valoare` (l-ar număra de
                // două ori în Total); pe Scutit/Neimpozabil/fără TipTva,
                // `PregatesteOperare` l-ar șterge oricum la operare. Negativul nu
                // e TVA justificat (stornarea are documentele ei).
                if (valoareTva < 0)
                    throw new OperareException("Valoarea TVA nu poate fi negativă.");
                // Prin FK, nu prin navigație: `AplicaTipTvaImplicit` setează doar
                // `TipTvaId`, iar navigația lazy nu e garantată pe toate căile (25b).
                var regim = detaliu.TipTvaId is Guid tipTvaLinieId
                    ? os.GetObjectByKey<TipTva>(tipTvaLinieId)?.Regim
                    : null;
                if (valoareTva != 0 && regim is not (RegimTva.Normal or RegimTva.TaxareInversa))
                    throw new OperareException(
                        "Valoarea TVA se completează manual doar pe un tip de TVA cu regim "
                        + "Normal sau Taxare inversă — regimul liniei nu poartă TVA separat.");
                detaliu.ValoareTva = valoareTva;
            }
        }

        var sterse = existente.Values.Where(d => !pastrate.Contains(d.ID)).ToList();
        if (sterse.Count > 0)
            os.Delete(sterse);
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
    // sau nu e un decont.
    public static DecontReadDto Citeste(IObjectSpace os, Guid id) {
        var h = os.GetObjectsQuery<Decont>()
            .Where(d => d.ID == id)
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.Stare, d.DataOperare,
                d.PredatorId, PredatorDenumire = d.Predator.Denumire,
                d.PrimitorId, PrimitorDenumire = d.Primitor.Denumire,
                d.NumarPV, d.DataPV
            })
            .FirstOrDefault();
        if (h == null)
            return null;

        // Citirea liniilor merge pe BAZA detaliului, cu frunza adusă prin `as`
        // (TPT ⇒ LEFT JOIN în SQL): deconturile ISTORICE pot purta linii de tip
        // BAZĂ, iar pe frunză singură ar fi ieșit `Linii: []` cu `Total` nenul
        // (constatarea F5 pe NIR). NULLABLE EXPLICIT pe TOATE valorile frunzei —
        // pe o linie de bază cast-ul dă null, iar un `decimal` non-nullable ar
        // pica la materializare.
        var linii = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => l.DocumentId == id)
            .OrderBy(l => l.ID)
            .Select(l => new {
                l.ID,
                l.TipMaterialId,
                TipMaterialCod = l.TipMaterial.Cod,
                TipMaterialDenumire = l.TipMaterial.Denumire,
                Descriere = (l as DecontDetaliu).Descriere,
                l.Cantitate,
                PretUnitar = (decimal?)(l as DecontDetaliu).PretUnitar,
                l.Valoare, l.ValoareTva,
                l.TipTvaId, TipTvaCod = l.TipTva.Cod, TipTvaDenumire = l.TipTva.Denumire,
                TipTvaCota = (decimal?)l.TipTva.Cota,
                CodEconomicId = (l as DecontDetaliu).CodEconomicId,
                CodEconomicCod = (l as DecontDetaliu).CodEconomic.Cod,
                l.AngajamentId, AngajamentCod = l.Angajament.Cod,
                ContDebitId = (l as DecontDetaliu).ContDebitId,
                ContDebitSimbol = (l as DecontDetaliu).ContDebit.Simbol,
                ContCreditId = (l as DecontDetaliu).ContCreditId,
                ContCreditSimbol = (l as DecontDetaliu).ContCredit.Simbol,
                RepartitorDebitId = (l as DecontDetaliu).RepartitorDebitId,
                RepartitorDebitDenumire = (l as DecontDetaliu).RepartitorDebit.Denumire,
                RepartitorCreditId = (l as DecontDetaliu).RepartitorCreditId,
                RepartitorCreditDenumire = (l as DecontDetaliu).RepartitorCredit.Denumire
            })
            .ToList();

        // `Total` se agregă pe BAZA detaliului (definiția `Document.Total`), ca să
        // dea EXACT ce dă `Lista` chiar dacă documentul poartă linii de tip bază.
        var total = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => l.DocumentId == id)
            .Sum(l => (decimal?)(l.Valoare + l.ValoareTva)) ?? 0m;

        // Affordance ONESTĂ pe stingeri (F3-D2/57d): decontul e una dintre cele 5
        // ramuri ale proiecției `DocumenteCuRest` și stă pe lanțul
        // avans↔decont↔regularizare — imperecherea e cazul NORMAL al tipului,
        // deci refuzul gardianului se ARATĂ, nu se descoperă la apăsarea butonului.
        var faraImperecheri = !ApiProiectii.AreImperecheri(os, id);

        return new DecontReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
            NumarPV = h.NumarPV, DataPV = h.DataPV,
            Total = total,
            PoateEdita = h.Stare == StareDocument.Draft,
            PoateOpera = h.Stare == StareDocument.Draft,
            PoateAnula = h.Stare == StareDocument.Operat && faraImperecheri,
            PoateStorna = h.Stare == StareDocument.Operat && faraImperecheri,
            Linii = linii.Select(l => new DecontLinieReadDto {
                Id = l.ID,
                TipMaterialId = l.TipMaterialId,
                TipMaterialCod = l.TipMaterialCod, TipMaterialDenumire = l.TipMaterialDenumire,
                Descriere = l.Descriere,
                Cantitate = l.Cantitate, PretUnitar = l.PretUnitar,
                Valoare = l.Valoare, ValoareTva = l.ValoareTva,
                TipTvaId = l.TipTvaId, TipTvaCod = l.TipTvaCod,
                TipTvaDenumire = l.TipTvaDenumire, TipTvaCota = l.TipTvaCota,
                CodEconomicId = l.CodEconomicId, CodEconomicCod = l.CodEconomicCod,
                AngajamentId = l.AngajamentId, AngajamentCod = l.AngajamentCod,
                ContDebitId = l.ContDebitId, ContDebitSimbol = l.ContDebitSimbol,
                ContCreditId = l.ContCreditId, ContCreditSimbol = l.ContCreditSimbol,
                RepartitorDebitId = l.RepartitorDebitId,
                RepartitorDebitDenumire = l.RepartitorDebitDenumire,
                RepartitorCreditId = l.RepartitorCreditId,
                RepartitorCreditDenumire = l.RepartitorCreditDenumire
            }).ToList()
        };
    }

    // `IQueryable` — DataSourceLoader îi pune deasupra filtrarea/sortarea/
    // paginarea clientului (43c). `Total` prin JOIN PE AGREGAT, nu subquery
    // corelat (42c), pe BAZA detaliului și BRUT (ca `Document.Total`).
    public static IQueryable<DecontListDto> Lista(IObjectSpace os) {
        var totaluri = os.GetObjectsQuery<DocumentDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new { DocumentId = g.Key, Total = g.Sum(x => x.Valoare + x.ValoareTva) });

        return from d in os.GetObjectsQuery<Decont>()
               join t in totaluri on d.ID equals t.DocumentId into agregat
               from t in agregat.DefaultIfEmpty()
               select new DecontListDto {
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
