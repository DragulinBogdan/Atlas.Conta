using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Culegere;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;

namespace Atlas.Conta.BackOffice.Module.Controllers;

public class RecalculeazaTvaController : ObjectViewController<ListView, DocumentDetaliu> {
    public RecalculeazaTvaController() {
        TargetViewNesting = Nesting.Nested;
        var actiune = new SimpleAction(this, "Document.RecalculeazaTva", PredefinedCategory.Edit) {
            Caption = "Recalculează TVA la cotă",
            SelectionDependencyType = SelectionDependencyType.RequireMultipleObjects,
            TargetObjectsCriteria = "Document.Stare = 0",
            TargetObjectsCriteriaMode = TargetObjectsCriteriaMode.TrueForAll,
        };
        actiune.Execute += (_, e) => {
            var linii = e.SelectedObjects.Cast<DocumentDetaliu>().ToArray();
            if (linii.Select(l => l.DocumentId).Distinct().Count() != 1)
                throw new UserFriendlyException("Selectați liniile unui singur document.");
            var doc = linii[0].Document ?? ObjectSpace.GetObjectByKey<Document>(linii[0].DocumentId);
            CulegereDocument.RecalculeazaTva(ObjectSpace, doc, linii);
            View.Refresh();
        };
    }

    protected override void OnActivated() {
        base.OnActivated();
        Active["DocumentCuTva"] = View.CollectionSource is PropertyCollectionSource { MasterObject: Document doc }
            && doc.CuTva();
    }
}
