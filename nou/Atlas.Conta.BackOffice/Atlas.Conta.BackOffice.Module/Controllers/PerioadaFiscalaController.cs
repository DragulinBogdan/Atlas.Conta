using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Api.Perioade;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using System.ComponentModel;
using DevExpress.ExpressApp;
using Atlas.DXF.Core.Appearance.Attributes;
using DevExpress.ExpressApp.ConditionalAppearance;
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

    readonly PopupWindowShowAction inchide;
    readonly PopupWindowShowAction redeschide;
    readonly SimpleAction reconstruieste;

    public PerioadaFiscalaController() {
        // Dialog, nu confirmare (F27-D2): închiderea se acceptă pe constatări
        // CONCRETE, nu pe un „da". Se deschide și când nu e nimic de acceptat —
        // lista goală e tot un verdict.
        inchide = new PopupWindowShowAction(this, "PerioadaFiscala.Inchide", PredefinedCategory.RecordEdit) {
            Caption = "Închide perioada",
            ToolTip = "Verifică luna selectată și o închide, cu acceptarea conștientă a avertismentelor.",
            AcceptButtonCaption = "Închide",
            CancelButtonCaption = "Renunță",
        };
        inchide.CustomizePopupWindowParams += Inchide_CustomizePopupWindowParams;
        inchide.Execute += Inchide_Execute;

        redeschide = new PopupWindowShowAction(this, "PerioadaFiscala.Redeschide", PredefinedCategory.RecordEdit) {
            Caption = "Redeschide perioada",
            ToolTip = "Redeschide ultima perioadă închisă, cu motiv scris.",
            AcceptButtonCaption = "Redeschide",
            CancelButtonCaption = "Renunță",
        };
        redeschide.CustomizePopupWindowParams += Redeschide_CustomizePopupWindowParams;
        redeschide.Execute += Redeschide_Execute;

        // Fără subiect: recalculează TOATE perioadele de referință, deci nu
        // depinde de rândul selectat (F27-D3).
        reconstruieste = new SimpleAction(this, "PerioadaFiscala.Reconstruieste", PredefinedCategory.RecordEdit) {
            Caption = "Reconstruiește soldurile",
            ToolTip = "Recalculează integral soldurile perioadelor de referință și raportează diferențele.",
            SelectionDependencyType = SelectionDependencyType.Independent,
            TargetViewType = ViewType.ListView,
            ConfirmationMessage = "Reconstruiți soldurile perioadelor de referință? Recalculul citește tot "
                + "istoricul registrelor și rescrie snapshot-urile; diferențele se raportează întâi.",
        };
        reconstruieste.Execute += Reconstruieste_Execute;
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

    void Reconstruieste_Execute(object sender, SimpleActionExecuteEventArgs e) {
        if (Application.Security is not IRequestSecurityStrategy cerinte
                || !cerinte.CanWrite(typeof(PerioadaFiscala), ObjectSpace))
            throw new UserFriendlyException(Refuzuri.FaraDrept(OperatieAcces.Modificare, typeof(PerioadaFiscala)));
        ReconstructieRezultatDto rezultat;
        var fabrica = Application.ServiceProvider.GetRequiredService<INonSecuredObjectSpaceFactory>();
        using (var osMotor = fabrica.CreateNonSecuredObjectSpace(typeof(PerioadaFiscala))) {
            try {
                rezultat = PerioadeApply.Reconstruieste(osMotor);
            }
            catch (OperareException ex) {
                throw new UserFriendlyException(ex.Message);
            }
        }
        ObjectSpace.Refresh();
        Informeaza(Rezuma(rezultat));
    }

    // Raportul iese ȘI fără diferențe, cu zerouri explicite (35b).
    static string Rezuma(ReconstructieRezultatDto rezultat) {
        if (rezultat.Referinte.Length == 0)
            return "Nicio perioadă de referință: nu există perioade închise, deci nu s-a reconstruit nimic.";
        var linii = rezultat.Referinte.Select(r =>
            $"{r.Luna:00}/{r.An}: contabil {r.ContabilExistente} → {r.ContabilRecalculate} "
            + $"({r.ContabilDiferite} diferite, Δdebit {r.DiferentaDebit:0.00}, Δcredit {r.DiferentaCredit:0.00}); "
            + $"stoc {r.StocExistente} → {r.StocRecalculate} ({r.StocDiferite} diferite, "
            + $"Δcantitate {r.DiferentaCantitate:0.000}, Δvaloare {r.DiferentaValoare:0.00})");
        return string.Join(Environment.NewLine, linii);
    }

    // Verificarea rulează pe ușa NON-SECURED, ca pe REST: verdictul însumează
    // documente, închideri de TVA și amortizări, iar un lanț filtrat ar da un
    // răspuns FALS, nu unul gol (73g/80e).
    void Inchide_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e) {
        var perioada = (PerioadaFiscala)View.CurrentObject;
        var os = Application.CreateObjectSpace(typeof(InchiderePerioadaParametri));
        var parametri = os.CreateObject<InchiderePerioadaParametri>();
        parametri.An = perioada.An;
        parametri.Luna = perioada.Luna;
        var fabrica = Application.ServiceProvider.GetRequiredService<INonSecuredObjectSpaceFactory>();
        using (var osMotor = fabrica.CreateNonSecuredObjectSpace(typeof(PerioadaFiscala)))
            foreach (var dto in PerioadeApply.Verifica(osMotor, perioada.An, perioada.Luna)) {
                var rand = os.CreateObject<ConstatareAcceptabila>();
                rand.Cheie = dto.Cheie;
                rand.Severitate = dto.Severitate;
                rand.Fel = dto.Fel;
                rand.Text = dto.Text;
                rand.Obiect = dto.ObiectEticheta;
                parametri.Constatari.Add(rand);
            }
        e.View = Application.CreateDetailView(os, parametri);
        e.View.Caption = $"Închide perioada {perioada.Luna:00}/{perioada.An}";
    }

    void Inchide_Execute(object sender, PopupWindowShowActionExecuteEventArgs e) {
        var parametri = (InchiderePerioadaParametri)e.PopupWindowViewCurrentObject;
        var perioada = (PerioadaFiscala)View.CurrentObject;
        var an = perioada.An;
        var luna = perioada.Luna;
        var cerere = new InchidePerioadaRequestDto {
            Acceptate = parametri.Constatari.Where(c => c.Acceptata).Select(c => c.Cheie).ToArray(),
        };
        var rezultat = Comanda(perioada,
            os => PerioadeApply.Inchide(os, an, luna, cerere, UserId(), Application.Security?.UserName));
        Informeaza($"Perioada {luna:00}/{an} a fost închisă la {rezultat.La.ToLocalTime():dd.MM.yyyy HH:mm}"
            + (rezultat.Acceptari.Length == 0
                ? ", fără constatări de acceptat."
                : $", cu {rezultat.Acceptari.Length} constatări acceptate."));
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

// Parametrii dialogului de închidere (F27-D2): lista constatărilor, cu bifa de
// acceptare pe avertismente. Non-persistent, ca `RedeschiderePerioadaParametri`;
// colecția e `[Aggregated]` fiindcă rândurile trăiesc cât dialogul.
[DomainComponent]
[XafDisplayName("Închide perioada")]
public class InchiderePerioadaParametri : NonPersistentBaseObject {
    int an; int luna;

    [XafDisplayName("An")]
    [DevExpress.ExpressApp.Model.ModelDefault("EditMask", "d")]
    [DevExpress.ExpressApp.Model.ModelDefault("DisplayFormat", "{0:0}")]
    [DevExpress.ExpressApp.Model.ModelDefault("AllowEdit", "False")]
    public int An { get => an; set => SetPropertyValue(ref an, value); }

    [XafDisplayName("Luna")]
    [DevExpress.ExpressApp.Model.ModelDefault("EditMask", "d")]
    [DevExpress.ExpressApp.Model.ModelDefault("DisplayFormat", "{0:0}")]
    [DevExpress.ExpressApp.Model.ModelDefault("AllowEdit", "False")]
    public int Luna { get => luna; set => SetPropertyValue(ref luna, value); }

    [Aggregated]
    [XafDisplayName("Constatări")]
    public BindingList<ConstatareAcceptabila> Constatari { get; } = [];
}

// O constatare, cu bifa ei. `Acceptata` e editabilă doar pe avertisment;
// blocantul rămâne refuzat de serviciu oricum — bifa ar fi o promisiune falsă.
[DomainComponent]
[XafDisplayName("Constatare")]
// Lista e a VERIFICĂRII: rândurile vin de la server, nu se adaugă și nu se
// șterg din dialog.
[ForbidCRUD("ListView")]
// Bifa se dă în grilă, nu într-un formular per rând: acceptarea e un gest pe
// listă („astea le știu”), iar `PopupEditForm` (implicitul Blazor) l-ar fi rupt
// în câte un dialog per constatare.
[DevExpress.ExpressApp.Model.ModelDefault("InlineEditMode", "Batch")]
[Appearance("ConstatareBlocanta", AppearanceItemType.ViewItem,
    "Severitate != 'Avertisment'", TargetItems = nameof(Acceptata), Enabled = false)]
[DefaultProperty(nameof(Text))]
public class ConstatareAcceptabila : NonPersistentBaseObject {
    bool acceptata;

    [Browsable(false)]
    public string Cheie { get; set; }

    [XafDisplayName("Severitate")]
    [DevExpress.ExpressApp.Model.ModelDefault("AllowEdit", "False")]
    public string Severitate { get; set; }

    [XafDisplayName("Fel")]
    [DevExpress.ExpressApp.Model.ModelDefault("AllowEdit", "False")]
    public string Fel { get; set; }

    [XafDisplayName("Constatare")]
    [DevExpress.ExpressApp.Model.ModelDefault("AllowEdit", "False")]
    public string Text { get; set; }

    [XafDisplayName("Obiect")]
    [DevExpress.ExpressApp.Model.ModelDefault("AllowEdit", "False")]
    public string Obiect { get; set; }

    [XafDisplayName("Acceptată")]
    public bool Acceptata { get => acceptata; set => SetPropertyValue(ref acceptata, value); }
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
