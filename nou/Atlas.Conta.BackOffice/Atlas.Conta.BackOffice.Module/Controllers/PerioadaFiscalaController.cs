using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Perioade;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Conta.BackOffice.Module.Controllers;

// Închiderea și redeschiderea perioadei din XAF Blazor (F27-D1/D2), în forma
// comenzilor de pe `api/perioade` (44/53: Blazor e vehiculul de iterație, nu un
// al doilea produs):
//
//   * GATE-UL e `CanWrite` pe INSTANȚĂ, pe ObjectSpace-ul securizat al View-ului,
//     înaintea ușii non-secured — fraza refuzului e cea din Module.
//   * COMANDA e `PerioadeApply`, pe ușa NON-SECURED (58c): serviciul scrie
//     `Inchisa` și rândul de istoric, adică exact ce refuză `GardianEditare` pe
//     ușa securizată.
//   * MOTIVUL redeschiderii se culege într-un dialog pe obiect NON-PERSISTENT
//     (tiparul 79-r1); fără lookup, deci fără spațiu compus.
public class PerioadaFiscalaController : ObjectViewController<ObjectView, PerioadaFiscala> {
    const string CheieStare = "StarePerioada";

    readonly SimpleAction inchide;
    readonly PopupWindowShowAction redeschide;

    public PerioadaFiscalaController() {
        inchide = new SimpleAction(this, "PerioadaFiscala.Inchide", PredefinedCategory.RecordEdit) {
            Caption = "Închide perioada",
            ToolTip = "Închide luna selectată, după verificarea lanțului: perioada închisă e graniță absolută.",
            ConfirmationMessage = "Închideți perioada selectată? În perioada închisă nu se mai poate opera, "
                + "anula sau storna niciun document.",
        };
        inchide.Execute += Inchide_Execute;

        redeschide = new PopupWindowShowAction(this, "PerioadaFiscala.Redeschide", PredefinedCategory.RecordEdit) {
            Caption = "Redeschide perioada",
            ToolTip = "Redeschide ultima perioadă închisă, cu motiv scris.",
            AcceptButtonCaption = "Redeschide",
            CancelButtonCaption = "Renunță",
        };
        redeschide.CustomizePopupWindowParams += Redeschide_CustomizePopupWindowParams;
        redeschide.Execute += Redeschide_Execute;
    }

    protected override void OnActivated() {
        base.OnActivated();
        View.CurrentObjectChanged += Stare_Changed;
        ActualizeazaStarea();
    }

    protected override void OnDeactivated() {
        View.CurrentObjectChanged -= Stare_Changed;
        base.OnDeactivated();
    }

    void Stare_Changed(object sender, EventArgs e) => ActualizeazaStarea();

    void ActualizeazaStarea() {
        var perioada = View?.CurrentObject as PerioadaFiscala;
        inchide.Enabled[CheieStare] = perioada is { Inchisa: false };
        redeschide.Enabled[CheieStare] = perioada is { Inchisa: true };
    }

    void Inchide_Execute(object sender, SimpleActionExecuteEventArgs e) {
        var perioada = (PerioadaFiscala)View.CurrentObject;
        var an = perioada.An;
        var luna = perioada.Luna;
        var rezultat = Comanda(perioada,
            os => PerioadeApply.Inchide(os, an, luna, null, UserId(), Application.Security?.UserName));
        Informeaza($"Perioada {luna:00}/{an} a fost închisă la {rezultat.La.ToLocalTime():dd.MM.yyyy HH:mm}.");
    }

    void Redeschide_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e) {
        var os = Application.CreateObjectSpace(typeof(RedeschiderePerioadaParametri));
        var parametri = os.CreateObject<RedeschiderePerioadaParametri>();
        e.View = Application.CreateDetailView(os, parametri);
        e.View.Caption = "Redeschide perioada";
    }

    void Redeschide_Execute(object sender, PopupWindowShowActionExecuteEventArgs e) {
        var parametri = (RedeschiderePerioadaParametri)e.PopupWindowViewCurrentObject;
        var perioada = (PerioadaFiscala)View.CurrentObject;
        var an = perioada.An;
        var luna = perioada.Luna;
        var cerere = new RedeschidePerioadaRequestDto { Motiv = parametri.Motiv };
        var rezultat = Comanda(perioada,
            os => PerioadeApply.Redeschide(os, an, luna, cerere, UserId(), Application.Security?.UserName));
        Informeaza($"Perioada {luna:00}/{an} a fost redeschisă la {rezultat.La.ToLocalTime():dd.MM.yyyy HH:mm}.");
    }

    InchiderePerioadaRezultatDto Comanda(PerioadaFiscala perioada,
            Func<IObjectSpace, InchiderePerioadaRezultatDto> comanda) {
        if (Application.Security is not IRequestSecurityStrategy cerinte
                || !cerinte.CanWrite(ObjectSpace, perioada))
            throw new UserFriendlyException(Refuzuri.FaraDrept(OperatieAcces.Modificare, typeof(PerioadaFiscala)));

        InchiderePerioadaRezultatDto rezultat;
        var fabrica = Application.ServiceProvider.GetRequiredService<INonSecuredObjectSpaceFactory>();
        using (var osMotor = fabrica.CreateNonSecuredObjectSpace(typeof(PerioadaFiscala))) {
            try {
                rezultat = comanda(osMotor);
            }
            catch (OperareException ex) {
                throw new UserFriendlyException(ex.Message);
            }
        }
        // Starea s-a schimbat în ALT DbContext: lista/detaliul sunt stale.
        ObjectSpace.Refresh();
        ActualizeazaStarea();
        return rezultat;
    }

    Guid? UserId() => Application.Security?.UserId as Guid?;

    void Informeaza(string mesaj) =>
        Application.ShowViewStrategy.ShowMessage(new MessageOptions {
            Message = mesaj,
            Type = InformationType.Success,
            Duration = 8000,
        });
}

// Parametrul dialogului de redeschidere — cererea `RedeschidePerioadaRequestDto`.
// Non-persistent: trăiește cât dialogul, nu are tabel și nu intră în
// `metadata.json` (dump-ul ia doar spațiul `BusinessObjects`).
// `SetPropertyValue`, nu auto-proprietăți: altfel `INotifyPropertyChanged` tace.
[DomainComponent]
[XafDisplayName("Redeschide perioada")]
public class RedeschiderePerioadaParametri : NonPersistentBaseObject {
    string motiv;

    [XafDisplayName("Motiv")]
    [DevExpress.ExpressApp.Model.ModelDefault("RowCount", "3")]
    public string Motiv {
        get => motiv;
        set => SetPropertyValue(ref motiv, value);
    }
}
