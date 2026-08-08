using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Controllers;

// Imperecherea (decizia 31d, extinsă de 48b la nota de compensare) = stingerea
// m2m stingător↔document; invarianții trăiesc în ImperechereService, dar New
// generic prin UI îi ocolea. Decizia: New rămâne
// PERMIS, dar validat la commit; Edit blocat (șterge + recreează); Delete liber
// (link-ul se desface fără registre proprii — gardianul de anulare/storno există
// deja în motor).
//
// Spike pasul 5 (D4): partea de ENFORCEMENT a migrat în `GardianEditare` —
// gardian generic pe `Committing`-ul ORICĂRUI ObjectSpace secured, deci aceleași
// invariante și pe calea Web API, unde nu există controllere de view. Aici rămân
// doar CAPABILITĂȚILE de UX (ce e editabil în ecran).
public sealed class ImperechereController : ViewController {
    public ImperechereController() {
        TargetObjectType = typeof(Imperechere);
        TargetViewType = ViewType.Any;
    }

    protected override void OnActivated() {
        base.OnActivated();
        AplicaCapabilitati();
        View.CurrentObjectChanged += OnCurrentObjectChanged;
        // După Save-ul unui obiect NOU, CurrentObjectChanged nu se declanșează —
        // fără re-evaluare pe Committed view-ul ar rămâne editabil (backstop-ul
        // din gardian refuză abia la commit); pattern-ul 40c.
        View.ObjectSpace.Committed += OnCommitted;
    }

    protected override void OnDeactivated() {
        View.CurrentObjectChanged -= OnCurrentObjectChanged;
        View.ObjectSpace.Committed -= OnCommitted;
        base.OnDeactivated();
    }

    void OnCurrentObjectChanged(object sender, EventArgs e) => AplicaCapabilitati();

    void OnCommitted(object sender, EventArgs e) => AplicaCapabilitati();

    // ListView: editarea inline a link-urilor e interzisă (invarianții nu se pot
    // reverifica pe editare parțială) — New/Delete rămân. DetailView: editabil
    // DOAR pe obiect nou (trebuie cules), read-only după persistare.
    void AplicaCapabilitati() {
        View.AllowEdit["Invarianti"] = View is DetailView
            && View.ObjectSpace.IsNewObject(View.CurrentObject);
    }
}
