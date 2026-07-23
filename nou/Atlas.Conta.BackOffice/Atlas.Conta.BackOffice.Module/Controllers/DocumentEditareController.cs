using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Controllers;

// Gardian de editare (review P2 defect 3): un document care nu mai e Draft nu se
// editează — registrele s-au scris din valorile operării, iar la P2 câmpurile FCL
// devin load-bearing și DUPĂ operare (RestNedescarcat citește cantitățile liniilor
// la generarea backorder-ului). Corecția legitimă = Anulează operarea (→ Draft)
// sau Stornează — acțiunile rămân active. Gardianul e la nivel de DetailView;
// acoperirea completă a editoarelor nested = polish XAF (planificat post-P2).
public class DocumentEditareController : ObjectViewController<DetailView, Document> {
    protected override void OnActivated() {
        base.OnActivated();
        Aplica();
        View.CurrentObjectChanged += OnSchimbare;
        ObjectSpace.Committed += OnSchimbare;
    }

    protected override void OnDeactivated() {
        View.CurrentObjectChanged -= OnSchimbare;
        ObjectSpace.Committed -= OnSchimbare;
        base.OnDeactivated();
    }

    void OnSchimbare(object sender, EventArgs e) => Aplica();

    void Aplica() =>
        View.AllowEdit["Stare"] = ViewCurrentObject == null || ViewCurrentObject.Stare == StareDocument.Draft;
}
