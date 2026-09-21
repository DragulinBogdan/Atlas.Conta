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

// Fața UI a motorului (decizia 14): acțiunile doar deleagă către motor; toată
// logica (gardieni, registre, tranzacție) stă acolo.
//
// Spike pasul 5 (D5, decizia 42b): calea e ACUM identică cu cea a tierului Web
// API — „secvență, nu cuib". Faza 1 = culegerea, comisă în ObjectSpace-ul
// SECURED al View-ului (cu validarea de Save și cu seam-urile de Committing ale
// culegerii). Faza 2 = comanda, prin `OperareApi`, într-un ObjectSpace
// NON-SECURED propriu, aruncat la final: acolo motorul își ține tranzacția
// integral, iar gardianul generic (`GardianEditare`) nu e activ — el refuză
// exact ce face motorul (registre, tranziții de `Stare`). Puntea între cele două
// faze e ID-ul documentului, în ambele sensuri; rezultatul se întoarce ca DATE
// (`OperareRezultat`), nu ca entitate din OS-ul comenzii.
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
            var rezultat = Executa(OperareApi.Opereaza);
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
            Executa(OperareApi.AnuleazaOperarea);
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
            Executa((os, id) => OperareApi.Storneaza(os, id, data));
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
        var documentId = ViewCurrentObject.ID;

        // Gate-ul comenzii, în forma de pe `POST api/documente/{id}/corecteaza`:
        // Write pe INSTANȚĂ (ca la operare/storno) plus Create ȘI Write pe TIPUL
        // CONCRET — comanda produce un document nou.
        var tip = MotorOperare.ClasaReala(ViewCurrentObject);
        if (Application.Security is not IRequestSecurityStrategy cerinte
                || !IsGrantedExtensions.CanWrite(cerinte, ObjectSpace, (object)ViewCurrentObject))
            throw new UserFriendlyException(
                "Nu aveți dreptul de scriere necesar pentru comenzile de operare pe acest document.");
        if (!cerinte.CanCreate(tip, ObjectSpace) || !cerinte.CanWrite(tip, ObjectSpace))
            throw new UserFriendlyException(Refuzuri.FaraDrept(OperatieAcces.Creare, tip));

        // Culegerea se comite ÎNAINTE de comandă (aceeași secvență ca `Executa`).
        ObjectSpace.CommitChanges();

        CorectieRezultat rezultat;
        var fabrica = Application.ServiceProvider.GetRequiredService<INonSecuredObjectSpaceFactory>();
        using (var osMotor = fabrica.CreateNonSecuredObjectSpace(typeof(Document))) {
            try {
                rezultat = OperareApi.Corecteaza(osMotor, documentId, data, parametri.Motiv);
            }
            catch (OperareException ex) {
                throw new UserFriendlyException(ex.Message);
            }
        }

        // Draftul s-a născut în ALT DbContext. `TargetWindow.NewWindow` — tab nou
        // cu controllerele lui, ca la generarea închiderii de TVA (79-r1): pe MDI,
        // `Default` cât timp dialogul e deschis devine `NewModalWindow`.
        ObjectSpace.Refresh();
        ActualizeazaDisponibilitatea();
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

    OperareRezultat Executa(Func<IObjectSpace, Guid, OperareRezultat> comanda) {
        // Culegerea se comite (și se VALIDEAZĂ — contextul Save) ÎNAINTE de motor.
        // Validarea Save rulează în Committing, adică DUPĂ ce motorul ar fi
        // materializat registrele/numărul/Stare=Operat în același ObjectSpace —
        // o regulă picată atunci ar lăsa o „operare-fantomă" în OS-ul viu, pe
        // care un Save ulterior ar comite-o fără re-rularea motorului.
        //
        // Commit NECONDIȚIONAT (GATE XAF D2): pe un draft deschis și neatins
        // `IsModified` e false, iar seam-ul de culegere din `Committing`
        // (DocumenteLoturiCulegereController — nașterea lotului) n-ar mai rula
        // niciodată pe calea „butonul direct", pe care contractul D2 o declară
        // acoperită. Un commit fără modificări e inofensiv (SaveChanges pe zero
        // entries), dar dă seam-urilor de Committing ultima șansă.
        ObjectSpace.CommitChanges();
        var documentId = ViewCurrentObject.ID;

        // Gate de autorizare (review advers F1): ușa non-secured de mai jos e a
        // MOTORULUI, nu a utilizatorului — cine nu are Write pe document prin
        // securitatea XAF nu comandă operarea/anularea/stornarea. Pre-migrare
        // refuzul venea implicit din commit-ul motorului în OS-ul secured al
        // View-ului; acum se decide explicit, înaintea ușii.
        if (Application.Security is not DevExpress.ExpressApp.Security.IRequestSecurityStrategy cerinte
                || !DevExpress.ExpressApp.Security.IsGrantedExtensions.CanWrite(
                        cerinte, ObjectSpace, (object)ViewCurrentObject))
            throw new UserFriendlyException("Nu aveți dreptul de scriere necesar pentru comenzile de operare pe acest document.");

        // Faza 2 (D5/42b): comanda rulează în ObjectSpace-ul NON-SECURED al
        // motorului, nu în cel al View-ului. `INonSecuredObjectSpaceFactory` e
        // scoped în DI-ul oricărui host AddXaf (DevExpress.ExpressApp\Services\
        // Core\StartupExtensions.cs:70-83), iar OS-urile lui NU trec prin
        // `IObjectSpaceCustomizer` (NonSecuredObjectSpaceFactory.cs:51-55) —
        // deci gardianul generic nu-l blochează. Motorul comite singur.
        OperareRezultat rezultat;
        var fabrica = Application.ServiceProvider.GetRequiredService<INonSecuredObjectSpaceFactory>();
        using (var osMotor = fabrica.CreateNonSecuredObjectSpace(typeof(Document))) {
            try {
                rezultat = comanda(osMotor, documentId);
            }
            catch (OperareException ex) {
                // Motorul cumulează erorile cu „\n" (o singură sursă de reguli — și
                // pentru consolă/API). În Blazor mesajul ajunge într-un
                // `<span class="xaf-alert-message">` (verificat pe surse:
                // AlertsHandlerServiceExceptionsExtensions → AlertTemplate), unde
                // `white-space` implicit ar colapsa liniile într-un paragraf continuu.
                // `site.css` cere acum `pre-line` pe clasa aceea; bulinele rămân ca
                // plasă (dacă CSS-ul nu se aplică, liniile rămân totuși distinguibile).
                var linii = ex.Message.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                if (linii.Length <= 1)
                    throw;
                throw new UserFriendlyException(
                    string.Join("\n", linii.Select(l => "• " + l.Trim())), ex);
            }
        }

        // Comanda a comis în ALT DbContext — OS-ul View-ului e stale (starea,
        // numărul, valorile rescrise de PregatesteOperare). `Refresh()` e exact
        // ce face acțiunea standard Refresh a XAF (SystemModule\
        // RefreshController.cs:63-67): `EFCoreObjectSpace.ReloadCore`
        // RECREEAZĂ DbContext-ul (EFCoreObjectSpace.cs:855-875), iar
        // `DetailView.OnObjectSpaceReloaded` re-obține CurrentObject din
        // contextul proaspăt (DetailView.cs:141-143) — deci ViewCurrentObject de
        // mai jos e instanța nouă, cu starea comisă de motor.
        ObjectSpace.Refresh();
        ActualizeazaDisponibilitatea();
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
        ActualizeazaDisponibilitatea();
        View.CurrentObjectChanged += OnCurrentObjectChanged;
    }

    protected override void OnDeactivated() {
        View.CurrentObjectChanged -= OnCurrentObjectChanged;
        base.OnDeactivated();
    }

    void OnCurrentObjectChanged(object sender, EventArgs e) => ActualizeazaDisponibilitatea();

    void ActualizeazaDisponibilitatea() {
        var stare = ViewCurrentObject?.Stare;
        opereaza.Enabled["Stare"] = stare == StareDocument.Draft;
        anuleaza.Enabled["Stare"] = stare == StareDocument.Operat;
        storneaza.Enabled["Stare"] = stare == StareDocument.Operat;
        corecteaza.Enabled["Stare"] = stare == StareDocument.Operat;
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
