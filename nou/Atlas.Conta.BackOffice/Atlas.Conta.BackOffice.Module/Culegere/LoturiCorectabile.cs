using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Culegere;

/// <summary>
/// Loturile pe care linia de corecție le poate alege: recepționate în gestiunea facturii, pe o factură
/// operată a aceluiași furnizor. Eligibilitatea pe faptele lotului rămâne a declarantului (CV-D10).
/// </summary>
public static class LoturiCorectabile {
    public static IEnumerable<Lot> Candidati(IObjectSpace os, Document doc) {
        if (os == null || doc == null || doc.PredatorId == Guid.Empty || doc.PrimitorId == Guid.Empty)
            return [];
        var receptii = os.GetObjectsQuery<FacturaIntrareDetaliu>()
            .Where(l => !l.CorectieValoareLot && l.DocumentId != doc.ID
                && l.Document.Stare == StareDocument.Operat && l.Document.PredatorId == doc.PredatorId)
            .Select(l => (Guid?)l.ID);
        return os.GetObjectsQuery<Lot>()
            .Where(l => l.GestiuneId == doc.PrimitorId && receptii.Contains(l.LinieIntrareId))
            .OrderBy(l => l.Data).ThenBy(l => l.Produs.Denumire).ToArray();
    }
}
