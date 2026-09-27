using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Trz;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Security;
using DevExpress.ExpressApp.SystemModule;
using DevExpress.Persistent.Base;
using DevExpress.Persistent.BaseImpl.EF;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Conta.BackOffice.Module.Controllers;

// Imperecherea (decizia 31d, extinsă de 48b la nota de compensare) = stingerea
// m2m stingător↔document. Invarianții trăiesc în `ImperechereService`, iar
// `GardianEditare` îi aplică pe ORICE ușă securizată (spike pasul 5, D4) —
// inclusiv pe OData, unde nu există controllere de view.
//
// Crearea, desfacerea și ștergerea sunt comenzi cu gate pe ObjectSpace-ul
// securizat și tranzacție proprie non-secured. Ștergerea desface și
// nominalizarea din cub; CRUD-ul direct nu poate menține această atomicitate.
public sealed class ImperechereController : ViewController {
    const string CheieNew = "F27.ImperechereaEComanda";

    readonly PopupWindowShowAction desface;
    readonly PopupWindowShowAction imperecheaza;
    readonly SimpleAction sterge;
    NewObjectViewController controllerNou;
    DeleteObjectsViewController controllerStergere;

    public ImperechereController() {
        TargetObjectType = typeof(Imperechere);
        TargetViewType = ViewType.Any;

        // Review advers F27 (1c): culegerea prin `New` + `Save` scria pe ușa
        // SECURIZATĂ, iar gardianul de Committing citește perioada în autocommit
        // — între validarea lui și `SaveChanges` o închidere se poate strecura,
        // iar rândul ajunge datat într-o perioadă închisă. Comanda ia rândul
        // perioadei sub `FOR UPDATE` în propria tranzacție, deci nu există
        // fereastră: `New` se retrage, culegerea trece prin dialog (79-r1).
        imperecheaza = new PopupWindowShowAction(this, "Imperechere.Creeaza", PredefinedCategory.RecordEdit) {
            Caption = "Împerechează",
            ToolTip = "Leagă documentul care stinge de documentul stins, cu suma și data cerute.",
            AcceptButtonCaption = "Împerechează",
            CancelButtonCaption = "Renunță",
            SelectionDependencyType = SelectionDependencyType.Independent,
        };
        imperecheaza.CustomizePopupWindowParams += Imperecheaza_CustomizePopupWindowParams;
        imperecheaza.Execute += Imperecheaza_Execute;

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
        sterge = new SimpleAction(this, "Imperechere.Sterge", PredefinedCategory.RecordEdit) {
            Caption = "Șterge împerecherea",
            SelectionDependencyType = SelectionDependencyType.RequireSingleObject,
            ConfirmationMessage = "Ștergeți împerecherea și eliberați suma stinsă?",
        };
        sterge.Execute += Sterge_Execute;
    }

    protected override void OnActivated() {
        base.OnActivated();
        AplicaCapabilitati();
        View.CurrentObjectChanged += OnCurrentObjectChanged;
        // După Save-ul unui obiect NOU, CurrentObjectChanged nu se declanșează —
        // fără re-evaluare pe Committed view-ul ar rămâne editabil (backstop-ul
        // din gardian refuză abia la commit); pattern-ul 40c.
        View.ObjectSpace.Committed += OnCommitted;
        imperecheaza.Active["ListView"] = View is ListView;
        controllerNou = Frame.GetController<NewObjectViewController>();
        if (controllerNou != null)
            controllerNou.NewObjectAction.Active[CheieNew] = false;
        controllerStergere = Frame.GetController<DeleteObjectsViewController>();
        if (controllerStergere != null)
            controllerStergere.DeleteAction.Active[CheieNew] = false;
    }

    protected override void OnDeactivated() {
        View.CurrentObjectChanged -= OnCurrentObjectChanged;
        View.ObjectSpace.Committed -= OnCommitted;
        if (controllerNou != null)
            controllerNou.NewObjectAction.Active.RemoveItem(CheieNew);
        controllerNou = null;
        if (controllerStergere != null)
            controllerStergere.DeleteAction.Active.RemoveItem(CheieNew);
        controllerStergere = null;
        base.OnDeactivated();
    }

    void Imperecheaza_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e) {
        // Lookup-urile de document cer un ObjectSpace PERSISTENT sub cel
        // non-persistent al parametrilor — precedentul `InchidereTvaGenerareController`.
        var os = Application.CreateObjectSpace(typeof(ImperechereParametri));
        if (os is NonPersistentObjectSpace npos && npos.Owner is not CompositeObjectSpace) {
            npos.PopulateAdditionalObjectSpaces(Application);
            npos.AutoDisposeAdditionalObjectSpaces = true;
        }
        var parametri = os.CreateObject<ImperechereParametri>();
        parametri.Data = DateTime.Today;
        e.View = Application.CreateDetailView(os, parametri);
        e.View.Caption = "Împerechează";
    }

    void Imperecheaza_Execute(object sender, PopupWindowShowActionExecuteEventArgs e) {
        var parametri = (ImperechereParametri)e.PopupWindowViewCurrentObject;
        var erori = new List<string>();
        if (parametri.DocumentStingator == null)
            erori.Add("Alegeți documentul care stinge.");
        if (parametri.Document == null)
            erori.Add("Alegeți documentul stins.");
        if (parametri.Suma <= 0)
            erori.Add("Suma imperecheată trebuie să fie pozitivă.");
        if (parametri.Data == default)
            erori.Add("Data imperecherii e obligatorie — stingerea e un fapt datat.");
        if (erori.Count > 0)
            throw new UserFriendlyException(string.Join("\n", erori));

        // Gate-ul de CREARE pe TIP, pe OS-ul securizat, înaintea ușii
        // non-secured (F21-D3 + 80b: Create ȘI Write).
        if (Application.Security is not IRequestSecurityStrategy cerinte
                || !cerinte.CanCreate(typeof(Imperechere), ObjectSpace)
                || !cerinte.CanWrite(typeof(Imperechere), ObjectSpace))
            throw new UserFriendlyException(Refuzuri.FaraDrept(OperatieAcces.Creare, typeof(Imperechere)));

        var idStingator = parametri.DocumentStingator.ID;
        var idStins = parametri.Document.ID;
        var suma = parametri.Suma;
        var data = DateOnly.FromDateTime(parametri.Data);
        var fabrica = Application.ServiceProvider.GetRequiredService<INonSecuredObjectSpaceFactory>();
        using (var osMotor = fabrica.CreateNonSecuredObjectSpace(typeof(Imperechere))) {
            try {
                ImperechereService.Imperecheaza(osMotor,
                    osMotor.GetObjectByKey<Document>(idStingator),
                    osMotor.GetObjectByKey<Document>(idStins), suma, null, data);
            }
            catch (OperareException ex) {
                throw new UserFriendlyException(ex.Message);
            }
        }
        ObjectSpace.Refresh();
        AplicaCapabilitati();
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

    void Sterge_Execute(object sender, SimpleActionExecuteEventArgs e) {
        if (View.CurrentObject is not Imperechere imperechere) return;
        if (Application.Security is not IRequestSecurityStrategy cerinte
                || !IsGrantedExtensions.CanDelete(cerinte, ObjectSpace, (object)imperechere))
            throw new UserFriendlyException(
                Refuzuri.FaraDrept(OperatieAcces.Stergere, typeof(Imperechere)));
        var fabrica = Application.ServiceProvider.GetRequiredService<INonSecuredObjectSpaceFactory>();
        using (var osMotor = fabrica.CreateNonSecuredObjectSpace(typeof(Imperechere))) {
            try { ImperechereService.Sterge(osMotor, imperechere.ID); }
            catch (OperareException ex) { throw new UserFriendlyException(ex.Message); }
        }
        if (View is DetailView) View.Close();
        else ObjectSpace.Refresh();
    }

    // Imperecherea nu se mai CULEGE nicăieri: e produsul unei comenzi, deci
    // ecranele ei sunt read-only (ListView și DetailView). Ștergerea rămâne a
    // ferestrei deschise, desfacerea a celei închise.
    void AplicaCapabilitati() {
        var nou = View.CurrentObject != null && View.ObjectSpace.IsNewObject(View.CurrentObject);
        View.AllowEdit["Invarianti"] = false;
        // Desfacerea are sens doar pe o imperechere persistată care nu e ea
        // însăși un rând invers; restul refuzurilor sunt ale motorului.
        desface.Enabled["Persistata"] = !nou
            && View.CurrentObject is Imperechere imperechere
            && imperechere.InverseazaId == null;
        sterge.Enabled["Persistata"] = desface.Enabled["Persistata"];
        sterge.Enabled["Acces"] = View.CurrentObject is Imperechere curenta
            && Application.Security is IRequestSecurityStrategy cerinte
            && IsGrantedExtensions.CanDelete(cerinte, ObjectSpace, (object)curenta);
    }
}

// Parametrii dialogului de împerechere (review F27, 1c), tiparul
// `GenerareInchidereTvaParametri`: non-persistent, `SetPropertyValue` ca
// precompletarea să se vadă; lookup-urile trăiesc pe OS-urile adiționale.
[DomainComponent]
[XafDisplayName("Împerechează")]
public class ImperechereParametri : NonPersistentBaseObject {
    Document documentStingator;
    Document document;
    decimal suma;
    DateTime data;

    [XafDisplayName("Document care stinge")]
    public Document DocumentStingator {
        get => documentStingator;
        set => SetPropertyValue(ref documentStingator, value);
    }

    [XafDisplayName("Document stins")]
    public Document Document {
        get => document;
        set => SetPropertyValue(ref document, value);
    }

    [XafDisplayName("Suma")]
    [ModelDefault("DisplayFormat", "{0:#,##0.00}")]
    public decimal Suma {
        get => suma;
        set => SetPropertyValue(ref suma, value);
    }

    [XafDisplayName("Data imperecherii")]
    [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy}")]
    [ModelDefault("EditMask", "d")]
    public DateTime Data {
        get => data;
        set => SetPropertyValue(ref data, value);
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
