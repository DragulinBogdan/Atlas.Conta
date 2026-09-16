using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Trz;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using Microsoft.Extensions.DependencyInjection;

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
    readonly PopupWindowShowAction desface;

    public ImperechereController() {
        TargetObjectType = typeof(Imperechere);
        TargetViewType = ViewType.Any;

        // F27-D8: o imperechere dintr-o perioadă închisă nu se șterge — se
        // desface prin rând invers, datat în fereastra deschisă. Un parametru
        // (data) cu precompletare ⇒ dialog pe obiect non-persistent (79-r1).
        desface = new PopupWindowShowAction(this, "Imperechere.Desfa", PredefinedCategory.RecordEdit) {
            Caption = "Desfă împerecherea",
            ToolTip = "Scrie rândul invers (sumă negativă) la data indicată; imperecherea rămâne în istoric.",
            AcceptButtonCaption = "Desfă",
            CancelButtonCaption = "Renunță",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject,
        };
        desface.CustomizePopupWindowParams += Desfa_CustomizePopupWindowParams;
        desface.Execute += Desfa_Execute;
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

    void Desfa_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e) {
        var os = Application.CreateObjectSpace(typeof(DesfacereParametri));
        var parametri = os.CreateObject<DesfacereParametri>();
        parametri.Data = DateTime.Today;
        e.View = Application.CreateDetailView(os, parametri);
        e.View.Caption = "Desfă împerecherea";
    }

    void Desfa_Execute(object sender, PopupWindowShowActionExecuteEventArgs e) {
        var parametri = (DesfacereParametri)e.PopupWindowViewCurrentObject;
        var data = DateOnly.FromDateTime(parametri.Data == default ? DateTime.Today : parametri.Data);
        var imperechere = (Imperechere)View.CurrentObject;
        var id = imperechere.ID;

        // Gate-ul comenzii, în forma de pe `POST api/imperecheri/{id}/desfa`:
        // desfacerea SCRIE un rând nou, deci Write pe instanță (nu Delete).
        if (Application.Security is not IRequestSecurityStrategy cerinte
                || !IsGrantedExtensions.CanWrite(cerinte, ObjectSpace, (object)imperechere))
            throw new UserFriendlyException(
                Refuzuri.FaraDrept(OperatieAcces.Modificare, typeof(Imperechere)));

        // Rândul invers e al MOTORULUI: gardianul de Committing îl refuză pe ușa
        // securizată, deci comanda rulează în ObjectSpace-ul non-secured propriu
        // (aceeași secvență ca operarea — `DocumentOperareController`).
        var fabrica = Application.ServiceProvider.GetRequiredService<INonSecuredObjectSpaceFactory>();
        using (var osMotor = fabrica.CreateNonSecuredObjectSpace(typeof(Imperechere))) {
            try {
                ImperechereService.Desfa(osMotor, id, data);
            }
            catch (OperareException ex) {
                throw new UserFriendlyException(ex.Message);
            }
        }
        ObjectSpace.Refresh();
        AplicaCapabilitati();
    }

    // ListView: editarea inline a link-urilor e interzisă (invarianții nu se pot
    // reverifica pe editare parțială) — New/Delete rămân. DetailView: editabil
    // DOAR pe obiect nou (trebuie cules), read-only după persistare.
    void AplicaCapabilitati() {
        var nou = View.CurrentObject != null && View.ObjectSpace.IsNewObject(View.CurrentObject);
        // F27-D8: faptul de stingere e datat, iar `0001-01-01` e refuzat la
        // commit — culegerea pornește de la azi, ca `DocumentDefaultsController`.
        if (nou && View.CurrentObject is Imperechere proaspata && proaspata.Data == default)
            proaspata.Data = DateOnly.FromDateTime(DateTime.Today);
        View.AllowEdit["Invarianti"] = View is DetailView && nou;
        // Desfacerea are sens doar pe o imperechere persistată care nu e ea
        // însăși un rând invers; restul refuzurilor sunt ale motorului.
        desface.Enabled["Persistata"] = !nou
            && View.CurrentObject is Imperechere imperechere
            && imperechere.InverseazaId == null;
    }
}

// Parametrul dialogului de desfacere (F27-D8), tiparul `CorectieParametri`:
// non-persistent, `SetPropertyValue` ca precompletarea să se vadă.
[DomainComponent]
[XafDisplayName("Desfă împerecherea")]
public class DesfacereParametri : NonPersistentBaseObject {
    DateTime data;

    [XafDisplayName("Data desfacerii")]
    [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy}")]
    [ModelDefault("EditMask", "d")]
    public DateTime Data {
        get => data;
        set => SetPropertyValue(ref data, value);
    }
}
