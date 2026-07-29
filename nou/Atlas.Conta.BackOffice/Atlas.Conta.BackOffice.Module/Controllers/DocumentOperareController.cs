using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;

namespace Atlas.Conta.BackOffice.Module.Controllers;

// Fața UI a motorului (decizia 14): acțiunile doar deleagă către MotorOperare;
// toată logica (gardieni, registre, tranzacție) stă în motor — aceeași cale o
// va folosi și tierul Web API pentru React (pasul 5).
public class DocumentOperareController : ObjectViewController<DetailView, Document> {
    readonly SimpleAction opereaza;
    readonly SimpleAction anuleaza;
    readonly ParametrizedAction storneaza;

    public DocumentOperareController() {
        opereaza = new SimpleAction(this, "Document.Opereaza", PredefinedCategory.RecordEdit) {
            Caption = "Operează", ConfirmationMessage = "Operați documentul? Se vor scrie registrele.",
        };
        opereaza.Execute += (s, e) => {
            Document conex = null;
            Executa((os, doc) => conex = MotorOperare.Opereaza(os, doc));
            // Fluxul legacy (00 §6): documentul conex generat se deschide imediat
            // în editare — utilizatorul îl verifică și îl operează separat.
            if (conex != null) {
                var os = Application.CreateObjectSpace(conex.GetType());
                e.ShowViewParameters.CreatedView = Application.CreateDetailView(os, os.GetObject(conex));
                e.ShowViewParameters.TargetWindow = TargetWindow.Default;
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
            Informeaza(parti);
        };

        anuleaza = new SimpleAction(this, "Document.AnuleazaOperarea", PredefinedCategory.RecordEdit) {
            Caption = "Anulează operarea",
            ConfirmationMessage = "Anulați operarea? Rândurile de registru ale documentului se șterg (corecție directă).",
        };
        anuleaza.Execute += (s, e) => {
            Executa(MotorOperare.AnuleazaOperarea);
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
            Executa((os, doc) => MotorOperare.Storneaza(os, doc, data));
            Informeaza(new List<string> { $"Stornat la {data:dd.MM.yyyy}." });
        };
    }

    void Executa(Action<IObjectSpace, Document> operatie) {
        // Culegerea se comite (și se VALIDEAZĂ — contextul Save) ÎNAINTE de motor.
        // Validarea Save rulează în Committing, adică DUPĂ ce motorul ar fi
        // materializat registrele/numărul/Stare=Operat în același ObjectSpace —
        // o regulă picată atunci ar lăsa o „operare-fantomă" în OS-ul viu, pe
        // care un Save ulterior ar comite-o fără re-rularea motorului.
        //
        // Commit NECONDIȚIONAT (GATE XAF D2): pe un draft deschis și neatins
        // `IsModified` e false, iar seam-ul de culegere din `Committing`
        // (FacturaIntrareLoturiController — nașterea lotului) n-ar mai rula
        // niciodată pe calea „butonul direct", pe care contractul D2 o declară
        // acoperită. Un commit fără modificări e inofensiv (SaveChanges pe zero
        // entries), dar dă seam-urilor de Committing ultima șansă.
        ObjectSpace.CommitChanges();
        try {
            operatie(ObjectSpace, ViewCurrentObject);
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
        ActualizeazaDisponibilitatea();
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
    }
}
