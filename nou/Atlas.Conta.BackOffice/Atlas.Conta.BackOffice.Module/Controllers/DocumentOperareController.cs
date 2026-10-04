using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.DC;
using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Security;
using DevExpress.Persistent.Base;
using Microsoft.Extensions.DependencyInjection;

namespace Atlas.Conta.BackOffice.Module.Controllers;

// Adaptorul XAF al cojii comenzii (104b): culegerea se comite în OS-ul View-ului, comanda pleacă prin ID (42b).
public class DocumentOperareController : ObjectViewController<DetailView, Document> {
    readonly SimpleAction opereaza;
    readonly SimpleAction anuleaza;
    readonly ParametrizedAction storneaza;
    readonly PopupWindowShowAction corecteaza;

    public DocumentOperareController() {
        opereaza = new SimpleAction(this, "Document.Opereaza", PredefinedCategory.RecordEdit) {
            Caption = "Operează", ConfirmationMessage = "Operați documentul? Se vor scrie registrele.",
        };
        opereaza.Execute += (s, e) => {
            var rezultat = Executa(c => c.Opereaza(ViewCurrentObject.ID));
            // Fluxul legacy (00 §6): documentul conex generat se deschide imediat
            // în editare — utilizatorul îl verifică și îl operează separat.
            // Prin ID (D5): entitatea trăia în OS-ul comenzii, care s-a închis.
            if (rezultat.ConexId is Guid conexId) {
                var os = Application.CreateObjectSpace(typeof(Document));
                var conex = os.GetObjectByKey<Document>(conexId);
                if (conex != null) {
                    e.ShowViewParameters.CreatedView = Application.CreateDetailView(os, conex);
                    e.ShowViewParameters.TargetWindow = TargetWindow.Default;
                }
            }
            // GATE XAF (D11): feedback la SUCCES — până acum operarea reușită nu
            // spunea nimic, iar numărul asignat de politică (seria fiscală) rămânea
            // invizibil până la un refresh. Mesajul e UNUL singur: restul
            // nedescărcat al FCL (P2, design §5 — pozițiile fără stoc rămân
            // backorder, interogabile per linie) se COMBINĂ în el, nu mai deschide
            // al doilea toast.
            var parti = new List<string>();
            var doc = ViewCurrentObject;
            if (doc?.Stare == StareDocument.Operat)
                parti.Add($"Operat. Nr. {doc.Numar}.");
            if (doc is FacturaIesire fcl) {
                var resturi = DescarcareService.RestNedescarcat(ObjectSpace, fcl)
                    .Where(x => x.RestNeacoperit > 0).ToList();
                if (resturi.Count > 0)
                    parti.Add("Rest nedescărcat (backorder): "
                        + FacturaIesireDescarcareController.RezumaResturi(ObjectSpace, resturi));
            }
            // Informările motorului (ex. conexul generat) vin ca date, nu ca
            // efect secundar — aceleași mesaje le va primi și clientul React.
            parti.AddRange(rezultat.Mesaje);
            Informeaza(parti);
        };

        anuleaza = new SimpleAction(this, "Document.AnuleazaOperarea", PredefinedCategory.RecordEdit) {
            Caption = "Anulează operarea",
            ConfirmationMessage = "Anulați operarea? Rândurile de registru ale documentului se șterg (corecție directă).",
        };
        anuleaza.Execute += (s, e) => {
            Executa(c => c.AnuleazaOperarea(ViewCurrentObject.ID));
            Informeaza(new List<string> { "Operarea anulată." });
        };

        // GATE XAF (D10): data stornării se CULEGE (motorul o primea deja ca
        // parametru, controllerul o hardcoda pe azi — corecția peste graniță de
        // perioadă era imposibilă din UI). Precedentul: „Generează descărcarea"
        // (FacturaIesireDescarcareController) — editor de dată în toolbar, default
        // azi, gol/neschimbat → azi. Confirmarea NU se pierde: verificat pe surse
        // (26.1.3, `ActionControlBase.InvokeActionExecuteAsync` → `Confirm()`),
        // ParametrizedAction trece prin exact același dialog ca SimpleAction.
        storneaza = new ParametrizedAction(this, "Document.Storneaza",
            PredefinedCategory.RecordEdit, typeof(DateTime)) {
            Caption = "Stornează",
            ToolTip = "Stornează documentul la data indicată (rânduri inverse de registru).",
            NullValuePrompt = "Data stornării",
            ConfirmationMessage = "Stornați documentul la data indicată?",
        };
        storneaza.Execute += (s, e) => {
            var aleasa = e.ParameterCurrentValue is DateTime dt && dt != default ? dt : DateTime.Today;
            var data = DateOnly.FromDateTime(aleasa);
            Executa(c => c.Storneaza(ViewCurrentObject.ID, data));
            Informeaza(new List<string> { $"Stornat la {data:dd.MM.yyyy}." });
        };

        // F27-D6: corecția peste graniță. Doi parametri (data + motivul) ⇒
        // dialog pe obiect non-persistent (tiparul 79-r1), nu editor în toolbar.
        corecteaza = new PopupWindowShowAction(this, "Document.Corecteaza", PredefinedCategory.RecordEdit) {
            Caption = "Corectează",
            ToolTip = "Stornează documentul la data indicată și deschide un draft nou cu aceeași culegere.",
            AcceptButtonCaption = "Corectează",
            CancelButtonCaption = "Renunță",
            ConfirmationMessage = "Corectați documentul? Se stornează la data indicată și se creează un "
                + "draft nou, legat de el, cu aceeași culegere.",
        };
        corecteaza.CustomizePopupWindowParams += Corecteaza_CustomizePopupWindowParams;
        corecteaza.Execute += Corecteaza_Execute;
    }

    void Corecteaza_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e) {
        // Fără lookup ⇒ fără spațiu compus (tiparul `RedeschiderePerioadaParametri`).
        var os = Application.CreateObjectSpace(typeof(CorectieParametri));
        var parametri = os.CreateObject<CorectieParametri>();
        parametri.Data = DateTime.Today;
        parametri.Motiv = MotivCorectie.EroareMateriala;
        e.View = Application.CreateDetailView(os, parametri);
        e.View.Caption = "Corectează documentul";
    }

    void Corecteaza_Execute(object sender, PopupWindowShowActionExecuteEventArgs e) {
        var parametri = (CorectieParametri)e.PopupWindowViewCurrentObject;
        var data = DateOnly.FromDateTime(parametri.Data == default ? DateTime.Today : parametri.Data);
        var rezultat = Executa(c => c.Corecteaza(ViewCurrentObject.ID, data, parametri.Motiv));

        // Draftul s-a născut în ALT DbContext. `TargetWindow.NewWindow` — tab nou
        // cu controllerele lui, ca la generarea închiderii de TVA (79-r1): pe MDI,
        // `Default` cât timp dialogul e deschis devine `NewModalWindow`.
        var osView = Application.CreateObjectSpace(typeof(Document));
        var draft = osView.GetObjectByKey<Document>(rezultat.CorectieId);
        if (draft != null) {
            e.ShowViewParameters.CreatedView = Application.CreateDetailView(osView, draft);
            e.ShowViewParameters.TargetWindow = TargetWindow.NewWindow;
        }
        Informeaza(new List<string> {
            $"Stornat la {data:dd.MM.yyyy}. Draftul de corecție e deschis alături."
        });
    }

    // Commit necondiționat: validarea Save și seam-urile de Committing ale culegerii rulează înaintea motorului (GATE XAF D2).
    T Executa<T>(Func<ComenziDocument, T> comanda) {
        ObjectSpace.CommitChanges();
        var servicii = Application.ServiceProvider;
        var comenzi = new ComenziDocument(servicii.GetRequiredService<INonSecuredObjectSpaceFactory>(),
            new DreptComandaXaf(servicii.GetRequiredService<IObjectSpaceFactory>(), Application.Security));
        T rezultat;
        try {
            rezultat = comanda(comenzi);
        }
        catch (Exception ex) when (ex is RefuzAcces or SubiectInvizibil) {
            throw new UserFriendlyException(ex.Message, ex);
        }
        catch (OperareException ex) {
            // Blazor colapsează liniile în `xaf-alert-message`; bulinele le păstrează distincte.
            var linii = ex.Message.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            if (linii.Length <= 1)
                throw;
            throw new UserFriendlyException(
                string.Join("\n", linii.Select(l => "• " + l.Trim())), ex);
        }
        // Comanda a comis în alt DbContext: `Refresh` recreează contextul și re-obține CurrentObject.
        ObjectSpace.Refresh();
        return rezultat;
    }

    void Informeaza(List<string> parti) {
        if (parti.Count == 0)
            return;
        Application.ShowViewStrategy.ShowMessage(new MessageOptions {
            Message = string.Join(" ", parti),
            Type = InformationType.Success,
            Duration = 6000,
        });
    }

    protected override void OnActivated() {
        base.OnActivated();
        // Default azi în editorul din toolbar (cosmetic; coalesce-ul din Execute
        // rămâne autoritatea pe gol/MinValue) — ca la „Generează descărcarea".
        storneaza.Value = DateTime.Today;
    }
}

// Parametrii dialogului de corecție — cererea `CorecteazaRequestDto`, cu motivul
// ca enum (editorul îi arată `[XafDisplayName]`-urile). Non-persistent: trăiește
// cât dialogul, n-are tabel și nu intră în `metadata.json`. `SetPropertyValue`,
// nu auto-proprietăți: altfel `INotifyPropertyChanged` tace și precompletarea
// nu se vede.
[DomainComponent]
[XafDisplayName("Corectează documentul")]
public class CorectieParametri : NonPersistentBaseObject {
    DateTime data;
    MotivCorectie motiv;

    [XafDisplayName("Data corecției")]
    [ModelDefault("DisplayFormat", "{0:dd.MM.yyyy}")]
    [ModelDefault("EditMask", "d")]
    public DateTime Data {
        get => data;
        set => SetPropertyValue(ref data, value);
    }

    [XafDisplayName("Motivul corecției")]
    public MotivCorectie Motiv {
        get => motiv;
        set => SetPropertyValue(ref motiv, value);
    }
}
