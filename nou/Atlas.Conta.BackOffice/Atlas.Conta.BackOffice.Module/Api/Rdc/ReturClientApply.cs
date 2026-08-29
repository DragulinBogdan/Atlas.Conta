using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Rdc;

// Nucleul feliei RDC: reconcilierea agregatului (scriere) + proiecțiile plate
// (citire). ZERO ASP.NET aici — controllerul din host e transport, iar
// ModelCheck exersează exact același cod pe `EFCoreObjectSpaceProvider`
// standalone (precedentul: motorul — docs 113709).
//
// CONTRACT DE APELANT: `Aplica`/`Sterge` rulează în ObjectSpace-ul SECURED al
// apelantului (endpoint-ul de scriere) și COMIT. Gardianul de Committing e
// ultima autoritate — pre-check-ul de Draft există ca mesajul să fie al
// DOMENIULUI și ca refuzul să vină înaintea oricărei modificări de stare.
//
// ═══ Ce e propriu RDC-ului ═══
//  1. **Rolul liniei = prezența lui `LotId`**, IMUABIL pe o linie existentă
//     (riscul 5, F19-D14): schimbarea de rol prin PUT e refuz explicit, nu
//     conversie tăcută. Vezi `ReturClientDtos` pentru motiv.
//  2. **Linia de COST își pierde identitatea fiscală**, nu doar valoarea:
//     `TipTvaId = null` + `ValoareTva = 0` PERSISTATE la culegere (F19-D7),
//     oglinda exactă a lui `ReturClient.PregatesteOperare`. „Inert devine
//     adevărat, nu doar afirmat" (F6-D3): `RegistruTva` scrie un rând pentru
//     ORICE linie cu `TipTvaId`, deci un implicit rămas pe linia de cost ar intra
//     în jurnal ca bază impozabilă (defectul închis în felia 11) — inclusiv la
//     backfill, care recitește din model, nu din registrul de la operare.
//  3. **Forma de culegere e pozitivă** (F19-D8): semnarea storno e a operării și
//     e idempotentă prin `Abs`. Ca pe RLF, Apply normalizează magnitudinile —
//     detaliile în `ReturFurnizorApply` (riscul 6).
public static class ReturClientApply {

    // ═══════════════════════ Scriere ═══════════════════════

    // `id` null = creare; altfel actualizare. Întoarce ID-ul documentului
    // (puntea 42b: entitățile nu traversează granița, cheile da).
    public static Guid Aplica(IObjectSpace os, Guid? id, RdcWriteDto dto) {
        if (dto == null)
            throw new OperareException("Lipsește corpul cererii.");

        ReturClient doc;
        if (id is Guid existentId) {
            doc = os.GetObjectByKey<ReturClient>(existentId)
                ?? throw new OperareException($"Returul de la client {existentId} nu există.");
            // Pre-check de DOMENIU: gardianul (`GardianEditare`) ar prinde oricum
            // la commit, dar abia după ce am rescris header-ul și liniile în
            // ObjectSpace-ul viu, cu mesajul lui generic. Aici oprim din prima.
            if (doc.Stare != StareDocument.Draft)
                throw new OperareException(
                    $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se mai modifică. "
                    + "Anulați operarea sau stornați-l.");
        }
        else {
            doc = os.CreateObject<ReturClient>();
        }

        // `Numar` NU se atinge (F19-D6): seria „RDC-" e server-owned, asignată la
        // MATERIALIZARE, în propria operare (53b).
        doc.Data = dto.Data;
        // NAVIGAȚIA, nu FK-ul scalar: rezolvarea validează existența cu mesaj de
        // domeniu. TIPUL laturilor (Partener → Gestiune) rămâne invariant al
        // OPERĂRII.
        doc.Predator = GasesteRepartitor(os, dto.PredatorId, "Predatorul (clientul)");
        doc.Primitor = GasesteRepartitor(os, dto.PrimitorId, "Primitorul (gestiunea)");

        ReconciliazaLinii(os, doc, dto.Linii ?? new List<RdcLinieWriteDto>());

        os.CommitChanges();
        return doc.ID;
    }

    // Ștergerea agregatului. Pre-check de DOMENIU pe Draft (gardianul de
    // Committing rămâne plasa). Fără curățenie de loturi: nicio linie de RDC nu
    // naște lot (marfa revine pe cel ORIGINAL).
    public static void Sterge(IObjectSpace os, Guid id) {
        var doc = os.GetObjectByKey<ReturClient>(id)
            ?? throw new OperareException($"Returul de la client {id} nu există.");
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
    static void ReconciliazaLinii(IObjectSpace os, ReturClient doc, List<RdcLinieWriteDto> linii) {
        var existente = doc.Detalii.ToDictionary(d => d.ID);
        var pastrate = new HashSet<Guid>();
        // Latura fiscală a tipului (F13-D1), rezolvată O SINGURĂ DATĂ pentru tot
        // agregatul. RDC stornează o LIVRARE, deci `Colectat` — pe taxarea inversă
        // nu există taxă de stornat.
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
                // RISCUL 5, pin-uit: rolul unei linii EXISTENTE e imuabil. O
                // conversie ar trebui să fie completă (TVA, natura Tipului,
                // valoarea, cantitatea pro-formă) — pe jumătate ar lăsa în
                // document o linie care nu e niciunul din cele două lucruri, iar
                // completă ar rescrie tăcut culegerea operatorului. Refuzul spune
                // și CE e de făcut: agregatul exprimă deja ștergerea (id-ul lipsă).
                if ((detaliu.LotId == null) != (l.LotId == null))
                    throw new OperareException(
                        $"Linia {linieId} este linie de "
                        + (detaliu.LotId == null ? "VENIT (fără lot)" : "MARFĂ RETURNATĂ (cu lot)")
                        + " și nu-și poate schimba rolul: rolul e dat de prezența lotului, iar cele două "
                        + "roluri au câmpuri diferite (venitul poartă valoarea și TVA-ul, marfa poartă lotul "
                        + "și cantitatea). Ștergeți linia din document și culegeți-o din nou pe rolul dorit.");
            }
            else {
                detaliu = os.CreateObject<DocumentDetaliu>();
                detaliu.Document = doc;
                noua = true;
            }

            // Baza DE DINAINTEA mapării, pentru semantica recalculului de TVA (ca
            // pe FCT/RLF): pe VENIT baza e valoarea culeasă, deci un Save care n-o
            // atinge nu pierde override-ul de ValoareTva.
            var bazaVeche = noua ? 0m : Math.Abs(detaliu.Valoare);
            Guid? tipTvaVechi = noua ? null : detaliu.TipTvaId;

            detaliu.TipMaterial = os.GetObjectByKey<TipMaterial>(l.TipMaterialId)
                ?? throw new OperareException($"Tipul (contul/clasa) {l.TipMaterialId} nu există.");
            if (l.LotId is Guid lotId) {
                detaliu.Lot = os.GetObjectByKey<Lot>(lotId)
                    ?? throw new OperareException($"Lotul {lotId} nu există.");
            }
            else {
                detaliu.Lot = null;
                detaliu.LotId = null;
            }

            // Scara numerică (49e) e gard la construirea MODELULUI, nu a valorii:
            // o valoare în afara coloanei ar ieși ca DbUpdateException brută din
            // Postgres. Refuzăm cu mesaj de domeniu.
            VerificaScara(l.Cantitate, Scara.Cantitate, "Cantitatea");
            VerificaScara(l.Valoare, Scara.Bani, "Valoarea");
            // MAGNITUDINE: semnul e al operării (28a/46e) — vezi nota din
            // `ReturFurnizorApply` despre round-trip-ul unui document anulat.
            detaliu.Cantitate = Math.Abs(l.Cantitate);

            if (detaliu.LotId != null) {
                // ── Linia de COST (marfa care revine pe lotul ORIGINAL) ──
                // Costul e al LOTULUI, nu se culege (pattern BTR/BCS/DSC), iar
                // `Valoare` din payload se ignoră deliberat: e câmp al celuilalt rol.
                var lot = os.GetObjectByKey<Lot>(detaliu.LotId.Value);
                detaliu.Valoare = Scara.RotunjesteBani(detaliu.Cantitate * lot.PretUnitar);
                // F19-D7: identitatea fiscală se ȘTERGE, nu se ignoră. Persistată,
                // altfel `RegistruTva` (și backfill-ul lui) scrie rândul.
                detaliu.TipTva = null;
                detaliu.TipTvaId = null;
                detaliu.ValoareTva = 0m;
                continue;
            }

            // ── Linia de VENIT (venitul stornat) ──
            if (l.TipTvaId is Guid tipTvaId) {
                detaliu.TipTva = os.GetObjectByKey<TipTva>(tipTvaId)
                    ?? throw new OperareException($"Tipul de TVA {tipTvaId} nu există.");
            }
            else {
                detaliu.TipTva = null;
                detaliu.TipTvaId = null;
            }

            // Default-ul de TipTva al tipului de document (RDC are
            // `TipTvaImplicit = N21`) — DOAR pe liniile NOI fără TipTva în payload.
            // Pe linia de COST nu ajunge niciodată: acolo bucla s-a întors deja.
            if (noua && l.TipTvaId == null)
                TvaService.AplicaTipTvaImplicit(os, doc, detaliu);

            // Normalizarea la pozitiv ÎNAINTE de calcul — oglinda primului rând al
            // ramurii de venit din `ReturClient.PregatesteOperare`.
            detaliu.ValoareTva = Math.Abs(detaliu.ValoareTva);
            // Lanțul de valori, materializat LA CULEGERE (GATE 53c). Baza e
            // valoarea CULEASĂ (venitul stornat de pe factura originală);
            // `CalculeazaValori` scrie `Valoare = round(baza)` și TVA-ul din cotă,
            // păstrând un override nenul cât timp declanșatorii n-au bătut.
            var bazaNoua = Math.Abs(l.Valoare);
            var pastreaza = !noua && bazaNoua == bazaVeche && detaliu.TipTvaId == tipTvaVechi;
            TvaService.CalculeazaValori(detaliu, bazaNoua,
                TvaService.IncarcaTipuri(os, new[] { detaliu }), directieTva, pastreaza);

            // Override-ul operatorului, DUPĂ calcul (oglinda fluxului UI, 36a).
            if (l.ValoareTva is decimal valoareTva) {
                VerificaScara(valoareTva, Scara.Bani, "Valoarea TVA");
                var regim = detaliu.TipTvaId is Guid tipTvaLinieId
                    ? os.GetObjectByKey<TipTva>(tipTvaLinieId)?.Regim
                    : null;
                if (valoareTva != 0 && regim is not (RegimTva.Normal or RegimTva.TaxareInversa))
                    throw new OperareException(
                        "Valoarea TVA se completează manual doar pe un tip de TVA cu regim "
                        + "Normal sau Taxare inversă — regimul liniei nu poartă TVA separat.");
                detaliu.ValoareTva = Math.Abs(valoareTva);
            }
        }

        var sterse = existente.Values.Where(d => !pastrate.Contains(d.ID)).ToList();
        if (sterse.Count > 0)
            os.Delete(sterse);
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
    // Proiecții PLATE (42c), direct pe `DocumentDetaliu`: RDC n-are frunză
    // (F19-D9), deci toate liniile lui sunt de bază.

    // `null` dacă documentul nu există (sau nu e un retur de la client).
    public static RdcReadDto Citeste(IObjectSpace os, Guid id) {
        var h = os.GetObjectsQuery<ReturClient>()
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

        var faraImperecheri = !ApiProiectii.AreImperecheri(os, id);

        return new RdcReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
            // CUSĂTURĂ cu modelul: aceeași definiție ca `ReturClient.Total`
            // (virtual) și ca `ReturClient.LiniiCreanta` — DOAR liniile fără lot.
            Total = linii.Where(l => l.LotId == null).Sum(l => l.Valoare + l.ValoareTva),
            TotalCost = linii.Where(l => l.LotId != null).Sum(l => l.Valoare),
            PoateEdita = h.Stare == StareDocument.Draft,
            PoateOpera = h.Stare == StareDocument.Draft,
            PoateAnula = h.Stare == StareDocument.Operat && faraImperecheri,
            PoateStorna = h.Stare == StareDocument.Operat && faraImperecheri,
            Linii = linii.Select(l => new RdcLinieReadDto {
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
    // paginarea clientului (43c). DOUĂ agregate condiționate într-o singură
    // grupare (nu două join-uri): `Total` = venitul, `TotalCost` = marfa.
    public static IQueryable<RdcListDto> Lista(IObjectSpace os) {
        var totaluri = os.GetObjectsQuery<DocumentDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new {
                DocumentId = g.Key,
                Total = g.Sum(x => x.LotId == null ? x.Valoare + x.ValoareTva : 0m),
                TotalCost = g.Sum(x => x.LotId != null ? x.Valoare : 0m)
            });

        return from d in os.GetObjectsQuery<ReturClient>()
               join t in totaluri on d.ID equals t.DocumentId into agregat
               from t in agregat.DefaultIfEmpty()
               select new RdcListDto {
                   Id = d.ID,
                   Numar = d.Numar,
                   Data = d.Data,
                   Stare = d.Stare == StareDocument.Draft ? "Draft"
                       : d.Stare == StareDocument.Operat ? "Operat"
                       : "Stornat",
                   PredatorDenumire = d.Predator.Denumire,
                   PrimitorDenumire = d.Primitor.Denumire,
                   Total = (decimal?)t.Total ?? 0m,
                   TotalCost = (decimal?)t.TotalCost ?? 0m
               };
    }
}
