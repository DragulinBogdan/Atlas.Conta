using System.Reflection;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.ExpressApp.Model.NodeGenerators;

namespace Atlas.Conta.BackOffice.Module.UI;

// Comută ListView-ul colecției nested `Detalii` de pe tipul de bază (DocumentDetaliu)
// pe cel al derivatei declarate cu [TipDetaliu]. Pattern XAF documentat: pentru un
// ListPropertyEditor, IModelMemberViewItem.View decide View-ul intern al colecției
// (docs DevExpress: IModelMemberViewItem.View). Cu ListView-ul tipizat,
// NewObjectViewController creează derivata, iar coloanele reflectă schema ei.
//
// Rulează la generarea nodului Items al FIECĂRUI DetailView (ModelDetailViewItemsNodesGenerator):
// node = nodul Items (IModelViewItems), node.Parent = DetailView-ul (IModelObjectView).
public sealed class TipDetaliuViewUpdater : ModelNodesGeneratorUpdater<ModelDetailViewItemsNodesGenerator> {
    public override void UpdateNode(ModelNode node) {
        if (node is not IModelViewItems items)
            return;
        if (node.Parent is not IModelObjectView objectView)
            return;
        var tipDocument = objectView.ModelClass?.TypeInfo?.Type;
        if (tipDocument == null)
            return;
        // Inherited = false pe atribut: se potrivește doar clasa care îl declară
        // direct (tipul concret al DetailView-ului), nu ierarhia.
        var attribut = tipDocument.GetCustomAttribute<TipDetaliuAttribute>(inherit: false);
        if (attribut == null)
            return;

        var listViewDetaliu = node.Application.BOModel.GetClass(attribut.TipDetaliu)?.DefaultListView;
        if (listViewDetaliu == null)
            return;

        var itemDetalii = items
            .OfType<IModelMemberViewItem>()
            .FirstOrDefault(i => i.PropertyName == nameof(Document.Detalii));
        // Set doar dacă nu e deja fixat (SetIfEmpty): la stadiul de generare View e
        // null, iar diff-urile utilizatorului din Model Editor se aplică oricum
        // PESTE stratul generatoarelor și rămân prioritare.
        if (itemDetalii != null && itemDetalii.View == null)
            itemDetalii.View = listViewDetaliu;
    }
}
