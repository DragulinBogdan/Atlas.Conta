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
    readonly SimpleAction storneaza;

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
        };

        anuleaza = new SimpleAction(this, "Document.AnuleazaOperarea", PredefinedCategory.RecordEdit) {
            Caption = "Anulează operarea",
            ConfirmationMessage = "Anulați operarea? Rândurile de registru ale documentului se șterg (corecție directă).",
        };
        anuleaza.Execute += (s, e) => Executa(MotorOperare.AnuleazaOperarea);

        storneaza = new SimpleAction(this, "Document.Storneaza", PredefinedCategory.RecordEdit) {
            Caption = "Stornează", ConfirmationMessage = "Stornați documentul la data de azi?",
        };
        storneaza.Execute += (s, e) => Executa((os, doc)
            => MotorOperare.Storneaza(os, doc, DateOnly.FromDateTime(DateTime.Today)));
    }

    void Executa(Action<IObjectSpace, Document> operatie) {
        // Modificările din editor intră în aceeași tranzacție cu registrele.
        operatie(ObjectSpace, ViewCurrentObject);
        ActualizeazaDisponibilitatea();
    }

    protected override void OnActivated() {
        base.OnActivated();
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
