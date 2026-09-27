using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Culegere;
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
// Culegerea (precompletări, valori, forma pozitivă a draftului — 46e) e a
// `CulegereDocument` (104c): un draft anulat, retrimis cu liniile semnate, revine
// la magnitudini.
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
        DocumentApply.AplicaDate(doc, dto.Data, dto.DataInregistrare);
        doc.DataExigibilitate = dto.DataExigibilitate;
        doc.DataPrimire = dto.DataPrimire;
        // NAVIGAȚIA, nu FK-ul scalar (ca la BTR/BCS/NIR): rezolvarea validează
        // existența cu mesaj de domeniu, iar pe o entitate urmărită navigația
        // încărcată ar rescrie la fixup un FK setat direct. TIPUL laturilor
        // (Gestiune → Partener) rămâne invariant al OPERĂRII.
        doc.Predator = GasesteRepartitor(os, dto.PredatorId, "Predatorul (gestiunea)");
        doc.Primitor = GasesteRepartitor(os, dto.PrimitorId, "Primitorul (furnizorul)");

        ReconciliazaLinii(os, doc, dto.Linii ?? new List<RlfLinieWriteDto>());
        CulegereDocument.InainteDeSalvare(os);
        os.CommitChanges();
        return doc.ID;
    }

    // Pre-check de DOMENIU pe Draft (gardianul de Committing rămâne plasa).
    public static void Sterge(IObjectSpace os, Guid id) {
        var doc = Rezolva.Cere<ReturFurnizor>(os, id, "Returul la furnizor");
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
    static void ReconciliazaLinii(IObjectSpace os, ReturFurnizor doc, List<RlfLinieWriteDto> linii) {
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
                d.ID, d.Numar, d.Data, d.DataInregistrare, d.DataExigibilitate, d.DataPrimire, d.Stare, d.DataOperare,
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
                l.Cantitate, l.Valoare, l.ValoareTva, l.TvaCules,
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
            DataInregistrare = h.DataInregistrare,
            DataExigibilitate = h.DataExigibilitate,
            DataPrimire = h.DataPrimire ?? h.DataInregistrare,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
            Total = linii.Sum(l => l.Valoare + l.ValoareTva),
            PoateEdita = h.Stare == StareDocument.Draft,
            PoateOpera = h.Stare == StareDocument.Draft,
            Corectie = ApiProiectii.Corectie(os, id),
            PoateAnula = h.Stare == StareDocument.Operat && faraImperecheri,
            PoateStorna = h.Stare == StareDocument.Operat && faraImperecheri,
            Linii = linii.Select(l => new RlfLinieReadDto {
                Id = l.ID, TipMaterialId = l.TipMaterialId,
                TipMaterialCod = l.TipMaterialCod, TipMaterialDenumire = l.TipMaterialDenumire,
                LotId = l.LotId,
                LotEticheta = ApiProiectii.EtichetaLot(l.LotProdus, l.LotData, l.LotPret),
                Cantitate = l.Cantitate, Valoare = l.Valoare, ValoareTva = l.ValoareTva, TvaCules = l.TvaCules,
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
