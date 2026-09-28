using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Culegere;

public static class AvansuriCulegere {
    public static IEnumerable<DocumentDetaliu> Candidati(IObjectSpace os, Document doc) {
        if (os == null || doc == null) return [];
        var tip = MotorOperare.GasesteTipDocument(os, doc);
        var politica = os.GetObjectsQuery<PoliticaTva>().SingleOrDefault(p => p.TipDocumentId == tip.ID);
        if (politica == null) return [];
        var sens = politica.Directie == DirectieTva.Deductibil ? SensTva.Achizitie : SensTva.Livrare;
        Guid? partener = politica.SursaContrapartida switch {
            SursaCont.RepartitorPredator => doc.PredatorId,
            SursaCont.RepartitorPrimitor => doc.PrimitorId, _ => null };
        var postari = Fiscale.Postari(os);
        var compensate = from p in postari
                         join i in postari on new { Id = (Guid?)p.ID, Spatiu = (Atlas.Conta.Nucleu.Spatiu?)p.Spatiu }
                             equals new { Id = i.InversaDinId, Spatiu = i.InversaDinSpatiu }
                         select p.LinieId;
        var surse = Fiscale.Fapte(os).Where(f => !f.Storno && f.Baza > 0m && f.Sens == sens
            && f.PartenerId == partener && f.DocumentId != doc.ID && !compensate.Contains(f.DetaliuId))
            .Select(f => f.DetaliuId);
        return os.GetObjectsQuery<DocumentDetaliu>()
            .Where(l => (l is FacturaIntrareDetaliu || l is FacturaIesireDetaliu)
                && l.Document.Stare == StareDocument.Operat && l.TipMaterial.RegularizareAvans && surse.Contains(l.ID))
            .OrderBy(l => l.Document.Data).ThenBy(l => l.Document.Numar).ThenBy(l => l.Pozitie).ToArray();
    }
}
