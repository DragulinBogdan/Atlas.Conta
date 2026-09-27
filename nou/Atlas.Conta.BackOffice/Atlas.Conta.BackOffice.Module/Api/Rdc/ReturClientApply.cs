using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Culegere;
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
// Culegerea (precompletări, valori, forma pozitivă, golirea TVA-ului pe linia de
// cost — F19-D7) e a `CulegereDocument` (104c). Rolul liniei = prezența lui
// `LotId` (F19-D14).
public static class ReturClientApply {

    // ═══════════════════════ Scriere ═══════════════════════

    // `id` null = creare; altfel actualizare. Întoarce ID-ul documentului
    // (puntea 42b: entitățile nu traversează granița, cheile da).
    public static Guid Aplica(IObjectSpace os, Guid? id, RdcWriteDto dto) {
        if (dto == null)
            throw new OperareException("Lipsește corpul cererii.");

        ReturClient doc;
        if (id is Guid existentId) {
            doc = Rezolva.Cere<ReturClient>(os, existentId, "Returul de la client");
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
        DocumentApply.AplicaDate(doc, dto.Data, dto.DataInregistrare);
        doc.DataExigibilitate = dto.DataExigibilitate;
        // NAVIGAȚIA, nu FK-ul scalar: rezolvarea validează existența cu mesaj de
        // domeniu. TIPUL laturilor (Partener → Gestiune) rămâne invariant al
        // OPERĂRII.
        doc.Predator = GasesteRepartitor(os, dto.PredatorId, "Predatorul (clientul)");
        doc.Primitor = GasesteRepartitor(os, dto.PrimitorId, "Primitorul (gestiunea)");

        ReconciliazaLinii(os, doc, dto.Linii ?? new List<RdcLinieWriteDto>());
        CulegereDocument.InainteDeSalvare(os);
        os.CommitChanges();
        return doc.ID;
    }

    // Pre-check de DOMENIU pe Draft (gardianul de Committing rămâne plasa).
    public static void Sterge(IObjectSpace os, Guid id) {
        var doc = Rezolva.Cere<ReturClient>(os, id, "Returul de la client");
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
    static void ReconciliazaLinii(IObjectSpace os, ReturClient doc, List<RdcLinieWriteDto> linii) {
        var existente = doc.Detalii.ToDictionary(d => d.ID);
        var pastrate = new HashSet<Guid>();

        foreach (var l in linii) {
            DocumentDetaliu detaliu;
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
            }

            var inainte = CulegereDocument.Urmareste(os, doc, detaliu);
            ApiLinie.TipMaterial(os, detaliu, l.TipMaterialId);
            if (l.LotId is Guid lotId) {
                detaliu.Lot = Rezolva.Cere<Lot>(os, lotId, "Lotul");
            }
            else {
                detaliu.Lot = null;
                detaliu.LotId = null;
            }
            detaliu.Cantitate = l.Cantitate;
            detaliu.Valoare = l.Valoare;
            if (l.TipTvaId is Guid tipTvaId) {
                detaliu.TipTva = Rezolva.Cere<TipTva>(os, tipTvaId, "Tipul de TVA");
            }
            else {
                detaliu.TipTva = null;
                detaliu.TipTvaId = null;
            }

            CulegereDocument.Mapata(os, doc, detaliu, inainte, l.ValoareTva);
        }

        var sterse = existente.Values.Where(d => !pastrate.Contains(d.ID)).ToList();
        if (sterse.Count > 0)
            os.Delete(sterse);
    }

    static Repartitor GasesteRepartitor(IObjectSpace os, Guid id, string rol) =>
        Rezolva.Cere<Repartitor>(os, id, rol);

    static string Eticheta(Document doc) =>
        string.IsNullOrWhiteSpace(doc.Numar) ? $"({doc.Data:dd.MM.yyyy})" : doc.Numar;

    // ═══════════════════════ Citire ═══════════════════════
    //
    // Proiecții PLATE (42c), direct pe `DocumentDetaliu`: RDC n-are frunză
    // (F19-D9), deci toate liniile lui sunt de bază.

    // `null` dacă documentul nu există, nu e vizibil (pe ușa securizată cele
    // două nu se disting — F22-D1, apelantul le traduce în același 404)
    // sau nu e un retur de la client.
    public static RdcReadDto Citeste(IObjectSpace os, Guid id) {
        var h = os.GetObjectsQuery<ReturClient>()
            .Where(d => d.ID == id)
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.DataInregistrare, d.DataExigibilitate, d.Stare, d.DataOperare,
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
            DataInregistrare = h.DataInregistrare,
            DataExigibilitate = h.DataExigibilitate,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
            // CUSĂTURĂ cu modelul: aceeași definiție ca `ReturClient.Total`
            // (virtual) și ca `ReturClient.LiniiCreanta` — DOAR liniile fără lot.
            Total = linii.Where(l => l.LotId == null).Sum(l => l.Valoare + l.ValoareTva),
            TotalCost = linii.Where(l => l.LotId != null).Sum(l => l.Valoare),
            PoateEdita = h.Stare == StareDocument.Draft,
            PoateOpera = h.Stare == StareDocument.Draft,
            Corectie = ApiProiectii.Corectie(os, id),
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
