using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Controllers;

// Gardianul de FOND al registrelor append-only (decizia 14): [ForbidCRUD] doar
// ASCUNDE acțiunile, iar regulile de appearance nu se evaluează pe un ListView
// gol (New rămânea funcțional pe bază proaspătă) și nu fac DetailView-ul
// read-only (Save rămânea posibil prin dialogul „modificări nesalvate") —
// review advers polish XAF. Aici se taie capabilitățile view-ului însuși;
// motorul nu e atins (scrie prin propriile commit-uri, nu prin view).
public class RegistruReadOnlyController : ViewController {
    public RegistruReadOnlyController() {
        TargetViewType = ViewType.Any;
    }

    protected override void OnActivated() {
        base.OnActivated();
        var tip = View.ObjectTypeInfo?.Type;
        if (tip != typeof(RegistruStoc) && tip != typeof(RegistruContabil))
            return;
        View.AllowEdit["AppendOnly"] = false;
        View.AllowNew["AppendOnly"] = false;
        View.AllowDelete["AppendOnly"] = false;
    }
}
