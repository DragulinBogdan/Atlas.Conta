using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Btr;

// Nucleul feliei BTR: reconcilierea agregatului (scriere) și proiecțiile plate
// (citire). ZERO ASP.NET aici — controllerul din host e transport, iar
// ModelCheck exersează exact același cod pe `EFCoreObjectSpaceProvider`
// standalone (precedentul: motorul, validat tot așa — docs 113709).
//
// CONTRACT DE APELANT: `Aplica` rulează în ObjectSpace-ul SECURED al apelantului
// (endpoint-ul de scriere) și COMITE. Gardianul de Committing e ultima autoritate
// — pre-check-ul de mai jos există ca mesajul să fie al DOMENIULUI („nu mai e
// Draft"), nu al gardianului generic, și ca refuzul să vină înaintea oricărei
// modificări de stare în ObjectSpace-ul viu.
public static class NotaTransferApply {

    // ═══════════════════════ Scriere ═══════════════════════

    // `id` null = creare; altfel actualizare. Întoarce ID-ul documentului
    // (puntea 42b: entitățile nu traversează granița, cheile da).
    public static Guid Aplica(IObjectSpace os, Guid? id, NotaTransferWriteDto dto) {
        if (dto == null)
            throw new OperareException("Lipsește corpul cererii.");

        NotaTransfer doc;
        if (id is Guid existentId) {
            doc = os.GetObjectByKey<NotaTransfer>(existentId)
                ?? throw new OperareException($"Nota de transfer {existentId} nu există.");
            // Pre-check de DOMENIU: gardianul (`GardianEditare`) ar prinde oricum
            // la commit, dar abia după ce am rescris header-ul și liniile în
            // ObjectSpace-ul viu, cu mesajul lui generic. Aici oprim din prima.
            if (doc.Stare != StareDocument.Draft)
                throw new OperareException(
                    $"Documentul {Eticheta(doc)} nu mai e Draft (starea „{doc.Stare}”) — nu se mai modifică. "
                    + "Anulați operarea sau stornați-l.");
        }
        else {
            doc = os.CreateObject<NotaTransfer>();
        }

        doc.Data = dto.Data;
        // NAVIGAȚIA, nu FK-ul scalar: (1) rezolvarea validează existența cu mesaj
        // de domeniu (altfel ar ieși o violare de FK din Postgres), (2) regulile
        // XAF de culegere (`RuleRequiredField`) stau pe navigație, (3) pe o
        // entitate deja urmărită, navigația încărcată ar rescrie la fixup un FK
        // setat direct.
        doc.Predator = GasesteRepartitor(os, dto.PredatorId, "Predatorul");
        doc.Primitor = GasesteRepartitor(os, dto.PrimitorId, "Primitorul");
        doc.NumarPV = dto.NumarPV;
        doc.DataPV = dto.DataPV;

        ReconciliazaLinii(os, doc, dto.Linii ?? new List<NotaTransferLinieWriteDto>());

        os.CommitChanges();
        return doc.ID;
    }

    // Reconcilierea server-side a colecției (D8): upsert pe `Id`, delete pe
    // liniile dispărute din payload. Clientul trimite agregatul ÎNTREG — nu
    // există CRUD per linie (43c).
    static void ReconciliazaLinii(IObjectSpace os, NotaTransfer doc, List<NotaTransferLinieWriteDto> linii) {
        var existente = doc.Detalii.ToDictionary(d => d.ID);
        var pastrate = new HashSet<Guid>();

        foreach (var l in linii) {
            DocumentDetaliu detaliu;
            if (l.Id is Guid linieId) {
                if (!existente.TryGetValue(linieId, out detaliu))
                    throw new OperareException(
                        $"Linia {linieId} nu aparține documentului {Eticheta(doc)}.");
                // Un Id repetat în payload ar suprascrie tăcut prima apariție (și
                // ar face ca „numărul de linii trimis" să nu mai fie cel salvat).
                if (!pastrate.Add(linieId))
                    throw new OperareException($"Linia {linieId} apare de două ori în cerere.");
            }
            else {
                detaliu = os.CreateObject<DocumentDetaliu>();
                detaliu.Document = doc;
            }

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
            detaliu.Cantitate = l.Cantitate;
            // `Valoare` NU se atinge: o materializează `PregatesteOperare`
            // (preț lot × cantitate) la operare — server-owned (D8).
        }

        var sterse = existente.Values.Where(d => !pastrate.Contains(d.ID)).ToList();
        if (sterse.Count > 0)
            os.Delete(sterse);
    }

    static Repartitor GasesteRepartitor(IObjectSpace os, Guid id, string rol) =>
        os.GetObjectByKey<Repartitor>(id)
            ?? throw new OperareException($"{rol} ({id}) nu există în nomenclatorul de repartitori.");

    static string Eticheta(Document doc) =>
        string.IsNullOrWhiteSpace(doc.Numar) ? $"({doc.Data:dd.MM.yyyy})" : doc.Numar;

    // ═══════════════════════ Citire ═══════════════════════
    //
    // Proiecții PLATE (decizia 42c): `Select` înainte de materializare, niciun
    // membru [NotMapped] și nicio navigație enumerată în afara query-ului
    // (reconfirmarea 25b). Owned-ul nu mai există (DIM-3), dar principiul e
    // același: pe sârmă trec câmpuri, nu grafuri.

    // `null` dacă documentul nu există (sau nu e o notă de transfer).
    public static NotaTransferReadDto Citeste(IObjectSpace os, Guid id) {
        var h = os.GetObjectsQuery<NotaTransfer>()
            .Where(d => d.ID == id)
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.Stare, d.DataOperare,
                d.PredatorId, PredatorDenumire = d.Predator.Denumire,
                d.PrimitorId, PrimitorDenumire = d.Primitor.Denumire,
                d.NumarPV, d.DataPV, d.Autogenerat, d.DocumentSursaId
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
                l.Cantitate, l.Valoare, l.ValoareTva
            })
            .ToList();

        return new NotaTransferReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
            NumarPV = h.NumarPV, DataPV = h.DataPV,
            Total = linii.Sum(l => l.Valoare + l.ValoareTva),
            Autogenerat = h.Autogenerat, DocumentSursaId = h.DocumentSursaId,
            // Affordances din stare — aceleași surse ca acțiunile XAF.
            PoateEdita = h.Stare == StareDocument.Draft,
            PoateOpera = h.Stare == StareDocument.Draft,
            PoateAnula = h.Stare == StareDocument.Operat,
            PoateStorna = h.Stare == StareDocument.Operat,
            Linii = linii.Select(l => new NotaTransferLinieReadDto {
                Id = l.ID, TipMaterialId = l.TipMaterialId,
                TipMaterialCod = l.TipMaterialCod, TipMaterialDenumire = l.TipMaterialDenumire,
                LotId = l.LotId,
                LotEticheta = EtichetaLot(l.LotProdus, l.LotData, l.LotPret),
                Cantitate = l.Cantitate, Valoare = l.Valoare
            }).ToList()
        };
    }

    // `IQueryable` — DataSourceLoader îi pune deasupra filtrarea/sortarea/paginarea
    // clientului și abia apoi materializează (grila e remote, 43c).
    //
    // `Total` prin JOIN PE AGREGAT, nu subquery corelat (42c): agregarea liniilor
    // se face o dată, iar LEFT JOIN-ul păstrează drafturile fără linii.
    public static IQueryable<NotaTransferListDto> Lista(IObjectSpace os) {
        var totaluri = os.GetObjectsQuery<DocumentDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new { DocumentId = g.Key, Total = g.Sum(x => x.Valoare + x.ValoareTva) });

        return from d in os.GetObjectsQuery<NotaTransfer>()
               join t in totaluri on d.ID equals t.DocumentId into agregat
               from t in agregat.DefaultIfEmpty()
               select new NotaTransferListDto {
                   Id = d.ID,
                   Numar = d.Numar,
                   Data = d.Data,
                   // Enum → string ÎN SQL (`CASE`): starea e string pe sârmă, dar
                   // filtrarea și sortarea rămân server-side (grila remote le
                   // trimite ca text). `ToString()` pe enum nu e o traducere
                   // garantată la toți providerii — expresia explicită e.
                   Stare = d.Stare == StareDocument.Draft ? "Draft"
                       : d.Stare == StareDocument.Operat ? "Operat"
                       : "Stornat",
                   PredatorDenumire = d.Predator.Denumire,
                   PrimitorDenumire = d.Primitor.Denumire,
                   Total = (decimal?)t.Total ?? 0m
               };
    }

    // Oglinda lui `Lot.Eticheta` (care e [NotMapped], deci inaccesibil în SQL):
    // aceleași reguli, compuse în memorie după materializarea câmpurilor plate.
    // Orice schimbare acolo se reflectă aici — cusătura e documentată în ambele.
    static string EtichetaLot(string produs, DateOnly? data, decimal? pretUnitar) {
        if (data == null)
            return null;
        var denumire = produs ?? "(produs nedefinit)";
        var pret = pretUnitar ?? 0m;
        return data == default(DateOnly) && pret == 0m
            ? $"{denumire} (în culegere)"
            : $"{denumire} · {data:dd.MM.yyyy} · {pret:0.####}";
    }
}
