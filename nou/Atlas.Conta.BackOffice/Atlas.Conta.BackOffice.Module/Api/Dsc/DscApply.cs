using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Dsc;

// Felia DSC (F4-D2): DOAR proiecțiile de citire. Scrierea nu există în felia
// asta — descărcarea e generată de `DescarcareService` (hook la operarea FCL +
// comanda de backorder `FacturaIesireApply.GenereazaDescarcare`), iar comenzile
// (opereaza/anuleaza/storneaza/valideaza) merg prin `OperareApi`, agnostic de
// tip. Numele `DscApply` se păstrează pentru simetria feliilor: când descărcarea
// culeasă manual intră în scop, `Aplica`/`Sterge` se adaugă AICI.
//
// Proiecții PLATE (42c): `Select` înainte de materializare, niciun membru
// [NotMapped] și nicio navigație enumerată în afara query-ului (25b).
public static class DscApply {

    // `null` dacă documentul nu există, nu e vizibil (pe ușa securizată cele
    // două nu se disting — F22-D1, apelantul le traduce în același 404)
    // sau nu e o descărcare de gestiune.
    public static DscReadDto Citeste(IObjectSpace os, Guid id) {
        var h = os.GetObjectsQuery<DescarcareGestiune>()
            .Where(d => d.ID == id)
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.Stare, d.DataOperare,
                d.PredatorId, PredatorDenumire = d.Predator.Denumire,
                d.PrimitorId, PrimitorDenumire = d.Primitor.Denumire,
                d.Autogenerat, d.DocumentSursaId,
                // LEFT JOIN pe documentul-sursă: null pe o descărcare culeasă manual.
                DocumentSursaNumar = d.DocumentSursa.Numar
            })
            .FirstOrDefault();
        if (h == null)
            return null;

        // Liniile se citesc pe tipul DERIVAT (ca la FCL): identitatea rândului de
        // descărcare e frunza (`LinieSursa`, `CodEconomic`), iar o linie de bază
        // pe un DSC e refuzată la operare („detaliu generic", review P2 defect 7)
        // — nu există cale prin care una să ajungă aici, fiindcă generatorul
        // creează exclusiv `DescarcareGestiuneDetaliu`.
        var linii = os.GetObjectsQuery<DescarcareGestiuneDetaliu>()
            .Where(l => l.DocumentId == id)
            .OrderBy(l => l.ID)
            .Select(l => new {
                l.ID, l.TipMaterialId,
                TipMaterialCod = l.TipMaterial.Cod,
                TipMaterialDenumire = l.TipMaterial.Denumire,
                l.LinieSursaId,
                l.LotId,
                // Produsul vine PRIN LOT (frunza DSC nu poartă ProdusId) — același
                // JOIN dă și cele trei câmpuri ale etichetei.
                LotProdusId = (Guid?)l.Lot.ProdusId,
                LotProdus = l.Lot.Produs.Denumire,
                LotData = (DateOnly?)l.Lot.Data,
                LotPret = (decimal?)l.Lot.PretUnitar,
                l.Cantitate, l.Valoare,
                l.CodEconomicId, CodEconomicCod = l.CodEconomic.Cod
            })
            .ToList();

        // `Total` se agregă pe BAZA detaliului (definiția `Document.Total`), ca să
        // dea EXACT ce dă `Lista`. Pe DSC `ValoareTva` e 0 prin construcție, deci
        // totalul e costul descărcării.
        var total = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => l.DocumentId == id)
            .Sum(l => (decimal?)(l.Valoare + l.ValoareTva)) ?? 0m;

        // Affordances ONESTE (F3-D2), oglinda celor două gardiene generice ale
        // motorului la anulare/stornare: `VerificaFaraImperecheri` (31d) și grupul
        // conex (un copil OPERAT refuză). Pe DSC copiii sunt în mod normal
        // inexistenți — dar predicatul e al BAZEI, iar affordance-ul care l-ar
        // ignora ar minți exact în cazul în care refuzul chiar vine. Un `Any`
        // mărginit per citire, nu proiecția `Copii` (DSC nu expune grupul).
        var faraCopiiOperati = !os.GetObjectsQuery<Document>()
            .Any(d => d.DocumentSursaId == id && d.Stare == StareDocument.Operat);
        var faraImperecheri = !ApiProiectii.AreImperecheri(os, id);

        return new DscReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
            Total = total,
            Autogenerat = h.Autogenerat, DocumentSursaId = h.DocumentSursaId,
            DocumentSursaNumar = h.DocumentSursaNumar,
            DocumentSursaTip = ApiProiectii.CodTip(os, h.DocumentSursaId),
            // Pe acest tier descărcarea n-are NICIO cale de scriere (F4-D2) — o
            // affordance de editare ar minți contractul (precedentul NIR, F2-D5).
            PoateEdita = false,
            PoateOpera = h.Stare == StareDocument.Draft,
            PoateAnula = h.Stare == StareDocument.Operat && faraCopiiOperati && faraImperecheri,
            PoateStorna = h.Stare == StareDocument.Operat && faraCopiiOperati && faraImperecheri,
            Linii = linii.Select(l => new DscLinieReadDto {
                Id = l.ID, TipMaterialId = l.TipMaterialId,
                TipMaterialCod = l.TipMaterialCod, TipMaterialDenumire = l.TipMaterialDenumire,
                LinieSursaId = l.LinieSursaId,
                LotId = l.LotId,
                LotEticheta = ApiProiectii.EtichetaLot(l.LotProdus, l.LotData, l.LotPret),
                ProdusId = l.LotProdusId, ProdusDenumire = l.LotProdus,
                Cantitate = l.Cantitate, Valoare = l.Valoare,
                CodEconomicId = l.CodEconomicId, CodEconomicCod = l.CodEconomicCod
            }).ToList()
        };
    }

    // `IQueryable` — DataSourceLoader îi pune deasupra filtrarea/sortarea/
    // paginarea clientului (43c). `Total` prin JOIN PE AGREGAT, nu subquery
    // corelat (42c), pe BAZA detaliului.
    public static IQueryable<DscListDto> Lista(IObjectSpace os) {
        var totaluri = os.GetObjectsQuery<DocumentDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new { DocumentId = g.Key, Total = g.Sum(x => x.Valoare + x.ValoareTva) });

        return from d in os.GetObjectsQuery<DescarcareGestiune>()
               join t in totaluri on d.ID equals t.DocumentId into agregat
               from t in agregat.DefaultIfEmpty()
               select new DscListDto {
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
                   Autogenerat = d.Autogenerat,
                   Total = (decimal?)t.Total ?? 0m
               };
    }
}
