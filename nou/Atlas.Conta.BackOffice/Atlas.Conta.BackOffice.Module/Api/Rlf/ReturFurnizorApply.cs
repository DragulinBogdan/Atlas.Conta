using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Rlf;

// Nucleul feliei RLF: reconcilierea agregatului (scriere) + proiecțiile plate
// (citire). ZERO ASP.NET aici — controllerul din host e transport, iar
// ModelCheck exersează exact același cod pe `EFCoreObjectSpaceProvider`
// standalone (precedentul: motorul — docs 113709).
//
// CONTRACT DE APELANT: `Aplica`/`Sterge` rulează în ObjectSpace-ul SECURED al
// apelantului (endpoint-ul de scriere) și COMIT. Gardianul de Committing e
// ultima autoritate — pre-check-ul de Draft există ca mesajul să fie al
// DOMENIULUI și ca refuzul să vină înaintea oricărei modificări de stare.
//
// ═══ Ce face Apply pe RLF în plus/în minus față de BCS ═══
// În PLUS: lanțul de TVA la culegere (F19-D7), cele două seam-uri ale FCT-ului
// apelate EXPLICIT (pe tierul API nu rulează niciun ViewController):
//   1. `TvaService.AplicaTipTvaImplicit` — doar pe liniile NOI fără TipTva în
//      payload (culegerea explicită, inclusiv golirea deliberată, bate default-ul);
//   2. `TvaService.CalculeazaValori` — scrie `Valoare`/`ValoareTva` din
//      `|cantitate| × prețul lotului`, apoi override-ul manual de `ValoareTva`.
// În MINUS: fără seam de culegere a loturilor — liniile de RLF nu declară
// `ILinieCareNasteLot`, deci nu nasc nimic; ele DESCARCĂ lotul original.
//
// ═══ Forma de CULEGERE e pozitivă — și Apply o impune ═══
// `PregatesteOperare` semnează la operare (46e) și e idempotent prin `Abs`.
// Consecința pentru calea de API (riscul 6 al contractului): după
// operare → ANULARE documentul redevine Draft cu liniile SEMNATE, iar ReadDto
// le întoarce așa (fapta operării). Un PUT de round-trip le-ar retrimite
// negative. Apply le NORMALIZEAZĂ înapoi la forma de culegere — magnitudini —
// exact cu aceleași `Abs`-uri pe care le aplică hook-ul; altfel draftul ar arăta
// o cantitate negativă lângă o valoare pozitivă, iar operatorul n-ar avea din ce
// citi cifra de pe nota de credit. Cine bate pe cine: pe DRAFT bate Apply (forma
// de culegere), la OPERARE bate hook-ul (forma de postare) — și cele două nu se
// contrazic, fiindcă amândouă pleacă din magnitudine.
public static class ReturFurnizorApply {

    // ═══════════════════════ Scriere ═══════════════════════

    // `id` null = creare; altfel actualizare. Întoarce ID-ul documentului
    // (puntea 42b: entitățile nu traversează granița, cheile da).
    public static Guid Aplica(IObjectSpace os, Guid? id, RlfWriteDto dto) {
        if (dto == null)
            throw new OperareException("Lipsește corpul cererii.");

        ReturFurnizor doc;
        if (id is Guid existentId) {
            doc = Rezolva.Cere<ReturFurnizor>(os, existentId, "Returul la furnizor");
            // Pre-check de DOMENIU: gardianul (`GardianEditare`) ar prinde oricum
            // la commit, dar abia după ce am rescris header-ul și liniile în
            // ObjectSpace-ul viu, cu mesajul lui generic. Aici oprim din prima.
            if (doc.Stare != StareDocument.Draft)
                throw new OperareException(
                    $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se mai modifică. "
                    + "Anulați operarea sau stornați-l.");
        }
        else {
            doc = os.CreateObject<ReturFurnizor>();
        }

        // `Numar` NU se atinge (F19-D6): seria „RLF-" e server-owned, asignată la
        // MATERIALIZARE, în propria operare (53b) — gardianul de Committing o și
        // păzește pe tipurile cu politică de numerotare.
        doc.Data = dto.Data;
        // NAVIGAȚIA, nu FK-ul scalar (ca la BTR/BCS/NIR): rezolvarea validează
        // existența cu mesaj de domeniu, iar pe o entitate urmărită navigația
        // încărcată ar rescrie la fixup un FK setat direct. TIPUL laturilor
        // (Gestiune → Partener) rămâne invariant al OPERĂRII.
        doc.Predator = GasesteRepartitor(os, dto.PredatorId, "Predatorul (gestiunea)");
        doc.Primitor = GasesteRepartitor(os, dto.PrimitorId, "Primitorul (furnizorul)");

        ReconciliazaLinii(os, doc, dto.Linii ?? new List<RlfLinieWriteDto>());

        os.CommitChanges();
        return doc.ID;
    }

    // Ștergerea agregatului. Pre-check de DOMENIU pe Draft (gardianul de
    // Committing rămâne plasa).
    //
    // FĂRĂ `LoturiCulegereService.CurataOrfane` (ca BCS, spre deosebire de
    // NIR/LDI): liniile de RLF nu declară `ILinieCareNasteLot`, deci nu nasc
    // niciodată loturi — lotul unei linii de retur e al altcuiva (l-a născut
    // recepția), iar curățenia n-ar avea ce căuta. Un apel ar fi inofensiv, dar
    // mincinos.
    public static void Sterge(IObjectSpace os, Guid id) {
        var doc = Rezolva.Cere<ReturFurnizor>(os, id, "Returul la furnizor");
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
    static void ReconciliazaLinii(IObjectSpace os, ReturFurnizor doc, List<RlfLinieWriteDto> linii) {
        var existente = doc.Detalii.ToDictionary(d => d.ID);
        var pastrate = new HashSet<Guid>();
        // Latura fiscală a tipului (F13-D1), rezolvată O SINGURĂ DATĂ pentru tot
        // agregatul: e proprietatea documentului, nu a liniei. RLF stornează o
        // ACHIZIȚIE, deci `Deductibil` — taxarea inversă rămâne autolichidare.
        var directieTva = TvaService.DirectiePentru(os, doc);

        foreach (var l in linii) {
            DocumentDetaliu detaliu;
            var noua = false;
            if (l.Id is Guid linieId) {
                if (!existente.TryGetValue(linieId, out detaliu))
                    throw new OperareException(
                        $"Linia {linieId} nu aparține documentului {Eticheta(doc)}.");
                // Un Id repetat în payload ar suprascrie tăcut prima apariție.
                if (!pastrate.Add(linieId))
                    throw new OperareException($"Linia {linieId} apare de două ori în cerere.");
            }
            else {
                detaliu = os.CreateObject<DocumentDetaliu>();
                detaliu.Document = doc;
                noua = true;
            }

            // Baza DE DINAINTEA mapării, pentru semantica recalculului de mai jos:
            // în UI recalculul TVA se declanșează DOAR la schimbarea bazei
            // (cantitatea sau LOTUL, fiindcă prețul e al lotului) ori a TipTva —
            // un Save care nu le atinge NU pierde override-ul de ValoareTva.
            var bazaVeche = noua ? 0m : Baza(os, detaliu.LotId, detaliu.Cantitate);
            Guid? tipTvaVechi = noua ? null : detaliu.TipTvaId;

            detaliu.TipMaterial = Rezolva.Cere<TipMaterial>(os, l.TipMaterialId, "Tipul (contul/clasa)");
            if (l.LotId is Guid lotId) {
                detaliu.Lot = Rezolva.Cere<Lot>(os, lotId, "Lotul");
            }
            else {
                detaliu.Lot = null;
                detaliu.LotId = null;
            }

            // Scara numerică (49e) e gard la construirea MODELULUI, nu a valorii:
            // o valoare în afara coloanei ar ieși ca DbUpdateException brută din
            // Postgres. Refuzăm cu mesaj de domeniu (ca BCS/NIR/FCT).
            VerificaScara(l.Cantitate, Scara.Cantitate, "Cantitatea");
            // MAGNITUDINE: semnul e al operării (28a/46e). Vezi nota de clasă —
            // un ReadDto de document ANULAT poartă cantitatea semnată, iar
            // round-trip-ul lui trebuie să se întoarcă la forma de culegere, nu
            // să fie refuzat.
            detaliu.Cantitate = Math.Abs(l.Cantitate);

            if (l.TipTvaId is Guid tipTvaId) {
                detaliu.TipTva = Rezolva.Cere<TipTva>(os, tipTvaId, "Tipul de TVA");
            }
            else {
                detaliu.TipTva = null;
                detaliu.TipTvaId = null;
            }

            // Default-ul de TipTva al tipului de document (RLF are
            // `TipTvaImplicit = N21`) — DOAR pe liniile noi al căror payload n-a
            // dat un TipTva: pe o linie existentă, golirea lui e decizie explicită
            // a operatorului, iar re-aplicarea default-ului ar face-o imposibilă.
            if (noua && l.TipTvaId == null)
                TvaService.AplicaTipTvaImplicit(os, doc, detaliu);

            // Normalizarea la pozitiv ÎNAINTE de calcul — oglinda exactă a
            // primului rând din `ReturFurnizor.PregatesteOperare`. Fără ea, un
            // ValoareTva rămas semnat de o operare anulată ar fi „păstrat" ca
            // override negativ.
            detaliu.ValoareTva = Math.Abs(detaliu.ValoareTva);
            // Lanțul de valori, materializat LA CULEGERE (GATE 53c): operatorul
            // confruntă nota de credit înainte de operare — de aceea `Valoare` nu
            // e în WriteDto. Formula e GEAMĂNA hook-ului (F19-D8), fără pasul de
            // SEMNARE: `Valoare` se recalculează ÎNTOTDEAUNA (nu se culege
            // niciodată — e a lotului), iar `ValoareTva` se PĂSTREAZĂ exact cât
            // timp declanșatorii din UI n-au bătut (`pastreazaTvaCules`, 36a).
            // Nuanța lui `pastreazaTvaCules`: păstrează doar un TVA NENUL — un
            // zero „cules" se recalculează, aici ca și la operare (36a e regula
            // OPERĂRII, iar acolo pragul e tot `!= 0`). Culegerea arată deci exact
            // ce va posta motorul, nu o cifră care s-ar schimba sub operator.
            //
            // F18/F5, `IDocumentCuIesireFiscala`: baza e `|q| × preț` și ATÂT.
            // Culegerea nu consultă soldul valoric al cheii și nu prezice golirea
            // — restul de cenți rămâne pe lot, exact ca la operare. Un al doilea
            // adevăr aici ar arăta operatorului o cifră pe care motorul n-o scrie.
            var bazaNoua = Baza(os, detaliu.LotId, detaliu.Cantitate);
            var pastreaza = !noua && bazaNoua == bazaVeche && detaliu.TipTvaId == tipTvaVechi;
            TvaService.CalculeazaValori(detaliu, bazaNoua,
                TvaService.IncarcaTipuri(os, new[] { detaliu }), directieTva, pastreaza);

            // Override-ul operatorului, DUPĂ calcul (oglinda fluxului UI): nota de
            // credit a furnizorului bate rotunjirea noastră (regula 36a). La
            // operare îl păstrează `pastreazaTvaCules: true`.
            if (l.ValoareTva is decimal valoareTva) {
                VerificaScara(valoareTva, Scara.Bani, "Valoarea TVA");
                // Ca pe FCT: override-ul are sens DOAR pe regimurile care postează
                // TVA separat. Pe Capitalizat TVA-ul e deja în `Valoare` (regimul
                // e oricum refuzat de tip la operare); pe Scutit/Neimpozabil/fără
                // TipTva, `PregatesteOperare` l-ar șterge — acceptarea lui ar minți.
                // Prin FK, nu prin navigație (25b): `AplicaTipTvaImplicit` setează
                // doar `TipTvaId`.
                var regim = detaliu.TipTvaId is Guid tipTvaLinieId
                    ? os.GetObjectByKey<TipTva>(tipTvaLinieId)?.Regim
                    : null;
                if (valoareTva != 0 && regim is not (RegimTva.Normal or RegimTva.TaxareInversa))
                    throw new OperareException(
                        "Valoarea TVA se completează manual doar pe un tip de TVA cu regim "
                        + "Normal sau Taxare inversă — regimul liniei nu poartă TVA separat.");
                // MAGNITUDINE, ca `Cantitate`: pe RLF negativul nu e un TVA de
                // semn contrar, e semnul pe care l-a pus operarea și pe care
                // round-trip-ul îl aduce înapoi.
                detaliu.ValoareTva = Math.Abs(valoareTva);
            }
        }

        var sterse = existente.Values.Where(d => !pastrate.Contains(d.ID)).ToList();
        if (sterse.Count > 0)
            os.Delete(sterse);
    }

    // Baza liniei = `|cantitate| × prețul lotului`. Fără lot (draft legitim
    // incomplet) e 0: valoarea veche, a lotului scos de pe linie, ar minți pe
    // ecran (precedentul BCS). NEROTUNJITĂ — rotunjirea se face o singură dată,
    // în `CalculeazaValori`, exact ca la operare.
    static decimal Baza(IObjectSpace os, Guid? lotId, decimal cantitate) =>
        lotId is Guid id && os.GetObjectByKey<Lot>(id) is Lot lot
            ? Math.Abs(cantitate) * lot.PretUnitar
            : 0m;

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
    // Direct pe `DocumentDetaliu`, fără `as`-cast: RLF n-are frunză (F19-D9),
    // deci toate liniile lui sunt de bază — șablonul BTR/BCS.

    // `null` dacă documentul nu există, nu e vizibil (pe ușa securizată cele
    // două nu se disting — F22-D1, apelantul le traduce în același 404)
    // sau nu e un retur la furnizor.
    public static RlfReadDto Citeste(IObjectSpace os, Guid id) {
        var h = os.GetObjectsQuery<ReturFurnizor>()
            .Where(d => d.ID == id)
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.Stare, d.DataOperare,
                d.PredatorId, PredatorDenumire = d.Predator.Denumire,
                d.PrimitorId, PrimitorDenumire = d.Primitor.Denumire
            })
            .FirstOrDefault();
        if (h == null)
            return null;

        var linii = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => l.DocumentId == id)
            .OrderBy(l => l.ID)
            .Select(l => new {
                l.ID, l.TipMaterialId,
                TipMaterialCod = l.TipMaterial.Cod,
                TipMaterialDenumire = l.TipMaterial.Denumire,
                l.LotId,
                LotProdus = l.Lot.Produs.Denumire,
                LotData = (DateOnly?)l.Lot.Data,
                LotPret = (decimal?)l.Lot.PretUnitar,
                l.Cantitate, l.Valoare, l.ValoareTva,
                l.TipTvaId, TipTvaCod = l.TipTva.Cod, TipTvaDenumire = l.TipTva.Denumire,
                TipTvaCota = (decimal?)l.TipTva.Cota
            })
            .ToList();

        // Affordance ONESTĂ pe stingeri (F3-D2): RLF nu e în `DocumenteCuRest`
        // (F19-D11), dar gardianul motorului (`VerificaFaraImperecheri`) e generic
        // pe `Document` — dacă totuși există un link (compensare pe notă, import),
        // refuzul se ARATĂ, nu se descoperă la apăsarea butonului.
        var faraImperecheri = !ApiProiectii.AreImperecheri(os, id);

        return new RlfReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
            Total = linii.Sum(l => l.Valoare + l.ValoareTva),
            PoateEdita = h.Stare == StareDocument.Draft,
            PoateOpera = h.Stare == StareDocument.Draft,
            PoateAnula = h.Stare == StareDocument.Operat && faraImperecheri,
            PoateStorna = h.Stare == StareDocument.Operat && faraImperecheri,
            Linii = linii.Select(l => new RlfLinieReadDto {
                Id = l.ID, TipMaterialId = l.TipMaterialId,
                TipMaterialCod = l.TipMaterialCod, TipMaterialDenumire = l.TipMaterialDenumire,
                LotId = l.LotId,
                LotEticheta = ApiProiectii.EtichetaLot(l.LotProdus, l.LotData, l.LotPret),
                Cantitate = l.Cantitate, Valoare = l.Valoare, ValoareTva = l.ValoareTva,
                TipTvaId = l.TipTvaId, TipTvaCod = l.TipTvaCod,
                TipTvaDenumire = l.TipTvaDenumire, TipTvaCota = l.TipTvaCota
            }).ToList()
        };
    }

    // `IQueryable` — DataSourceLoader îi pune deasupra filtrarea/sortarea/
    // paginarea clientului (43c). `Total` prin JOIN PE AGREGAT, nu subquery
    // corelat (42c).
    public static IQueryable<RlfListDto> Lista(IObjectSpace os) {
        var totaluri = os.GetObjectsQuery<DocumentDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new { DocumentId = g.Key, Total = g.Sum(x => x.Valoare + x.ValoareTva) });

        return from d in os.GetObjectsQuery<ReturFurnizor>()
               join t in totaluri on d.ID equals t.DocumentId into agregat
               from t in agregat.DefaultIfEmpty()
               select new RlfListDto {
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
