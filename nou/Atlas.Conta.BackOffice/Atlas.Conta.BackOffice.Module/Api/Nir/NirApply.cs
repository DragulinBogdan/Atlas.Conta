using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Api.Nir;

// Felia NIR (F2-D3): DOAR proiecțiile de citire. Scrierea nu există în felia
// asta — NIR-ul de producție e clona conexă generată de motor la operarea
// facturii, iar comenzile (opereaza/anuleaza/storneaza/valideaza) merg prin
// `OperareApi`, care e agnostic de tip. Numele `NirApply` se păstrează pentru
// simetria feliilor: când NIR-ul cules manual intră în scop, `Aplica`/`Sterge`
// se adaugă AICI, fără să mute nimic.
//
// Proiecții PLATE (42c): `Select` înainte de materializare, niciun membru
// [NotMapped] și nicio navigație enumerată în afara query-ului (25b).
public static class NirApply {

    // `null` dacă documentul nu există (sau nu e un NIR).
    public static NirReadDto Citeste(IObjectSpace os, Guid id) {
        var h = os.GetObjectsQuery<NIR>()
            .Where(d => d.ID == id)
            .Select(d => new {
                d.ID, d.Numar, d.Data, d.Stare, d.DataOperare,
                d.PredatorId, PredatorDenumire = d.Predator.Denumire,
                d.PrimitorId, PrimitorDenumire = d.Primitor.Denumire,
                d.Autogenerat, d.DocumentSursaId,
                // LEFT JOIN pe documentul-sursă: null pe un NIR cules manual.
                DocumentSursaNumar = d.DocumentSursa.Numar
            })
            .FirstOrDefault();
        if (h == null)
            return null;

        // Citirea liniilor merge pe BAZA detaliului, cu frunza NIR (DIM-2) adusă
        // prin `as` (TPT ⇒ LEFT JOIN în SQL): clona conexă generată azi se naște
        // pe `NirDetaliu` ([TipDetaliu]), dar NIR-urile ISTORICE (importul/clonele
        // pre-DIM-2) poartă linii de tip BAZĂ — pe frunză singură ar fi ieșit
        // `Linii: []` cu `Total` nenul (constatarea pasului 3 al feliei).
        // Dimensiunile sunt null pe liniile de bază — exact ce poartă.
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
                l.TipTvaId, TipTvaCod = l.TipTva.Cod,
                CodEconomicId = (l as NirDetaliu).CodEconomicId,
                CodEconomicCod = (l as NirDetaliu).CodEconomic.Cod,
                SursaFinantareId = (l as NirDetaliu).SursaFinantareId,
                SursaFinantareCod = (l as NirDetaliu).SursaFinantare.Cod,
                CodFunctionalId = (l as NirDetaliu).CodFunctionalId,
                CodFunctionalCod = (l as NirDetaliu).CodFunctional.Cod,
                ProiectId = (l as NirDetaliu).ProiectId,
                ProiectCod = (l as NirDetaliu).Proiect.Cod
            })
            .ToList();

        // `Total` se agregă pe BAZA detaliului (definiția `Document.Total`), ca
        // să dea EXACT ce dă `Lista` chiar dacă documentul ar purta o linie de
        // tip bază (NIR cules manual înainte de frunza DIM-2).
        var total = os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => l.DocumentId == id)
            .Sum(l => (decimal?)(l.Valoare + l.ValoareTva)) ?? 0m;

        // Affordance ONESTĂ pe stingeri (F3-D2): NIR-ul nu e creanță și n-ar
        // trebui să poarte imperecheri, dar gardianul motorului
        // (`VerificaFaraImperecheri`) e generic pe `Document` — dacă totuși
        // există un link (import, compensare pe notă), refuzul se ARATĂ, nu se
        // descoperă la apăsarea butonului. Un `Any` mărginit per citire.
        var faraImperecheri = !ApiProiectii.AreImperecheri(os, id);

        return new NirReadDto {
            Id = h.ID, Numar = h.Numar, Data = h.Data,
            Stare = h.Stare.ToString(), DataOperare = h.DataOperare,
            PredatorId = h.PredatorId, PredatorDenumire = h.PredatorDenumire,
            PrimitorId = h.PrimitorId, PrimitorDenumire = h.PrimitorDenumire,
            Total = total,
            Autogenerat = h.Autogenerat, DocumentSursaId = h.DocumentSursaId,
            DocumentSursaNumar = h.DocumentSursaNumar,
            DocumentSursaTip = ApiProiectii.CodTip(os, h.DocumentSursaId),
            // Review advers F2-D5: pe acest tier NIR-ul nu are NICIO cale de
            // scriere (F2-D3) — o affordance de editare ar minți contractul.
            PoateEdita = false,
            PoateOpera = h.Stare == StareDocument.Draft,
            PoateAnula = h.Stare == StareDocument.Operat && faraImperecheri,
            PoateStorna = h.Stare == StareDocument.Operat && faraImperecheri,
            Linii = linii.Select(l => new NirLinieReadDto {
                Id = l.ID, TipMaterialId = l.TipMaterialId,
                TipMaterialCod = l.TipMaterialCod, TipMaterialDenumire = l.TipMaterialDenumire,
                LotId = l.LotId,
                LotEticheta = ApiProiectii.EtichetaLot(l.LotProdus, l.LotData, l.LotPret),
                Cantitate = l.Cantitate, Valoare = l.Valoare, ValoareTva = l.ValoareTva,
                TipTvaId = l.TipTvaId, TipTvaCod = l.TipTvaCod,
                CodEconomicId = l.CodEconomicId, CodEconomicCod = l.CodEconomicCod,
                SursaFinantareId = l.SursaFinantareId, SursaFinantareCod = l.SursaFinantareCod,
                CodFunctionalId = l.CodFunctionalId, CodFunctionalCod = l.CodFunctionalCod,
                ProiectId = l.ProiectId, ProiectCod = l.ProiectCod
            }).ToList()
        };
    }

    // `IQueryable` — DataSourceLoader îi pune deasupra filtrarea/sortarea/
    // paginarea clientului (43c). `Total` prin JOIN PE AGREGAT, nu subquery
    // corelat (42c), pe BAZA detaliului.
    public static IQueryable<NirListDto> Lista(IObjectSpace os) {
        var totaluri = os.GetObjectsQuery<DocumentDetaliu>()
            .GroupBy(l => l.DocumentId)
            .Select(g => new { DocumentId = g.Key, Total = g.Sum(x => x.Valoare + x.ValoareTva) });

        return from d in os.GetObjectsQuery<NIR>()
               join t in totaluri on d.ID equals t.DocumentId into agregat
               from t in agregat.DefaultIfEmpty()
               select new NirListDto {
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
