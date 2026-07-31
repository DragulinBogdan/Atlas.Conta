using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;

namespace Atlas.Conta.BackOffice.Module.Controllers;

// GATE XAF (D7): `Numar` READ-ONLY data-driven. Tipul care ARE rând
// `PoliticaNumerotare` își primește numărul de la motor, la operare (seria fiscală
// a FCL — `FCL-…`); un câmp editabil acolo o ocolea. Tipurile FĂRĂ politică (FCT —
// numărul e al furnizorului) îl culeg obligatoriu, exact ca azi.
//
// Criteriul e DATA, nu tipul CLR: nimic hardcodat, aceeași ancoră pe care o folosește
// motorul (`MotorOperare.GasesteTipDocument` — de-proxificarea EF Core + căutarea
// pe ClrType). O interogare per view, la activare: tipul unui DetailView e fix,
// deci rezultatul se cache-uiește; re-evaluarea pe `CurrentObjectChanged` doar
// re-aplică flag-ul pe editor.
public class NumarPoliticaController : ObjectViewController<DetailView, Document> {
    const string CheieCapacitate = "GateXaf.PoliticaNumerotare";

    bool? areNumerotare;

    protected override void OnActivated() {
        base.OnActivated();
        Aplica();
        View.CurrentObjectChanged += OnCurrentObjectChanged;
    }

    protected override void OnDeactivated() {
        View.CurrentObjectChanged -= OnCurrentObjectChanged;
        // Review advers D2: instanțele de ViewController se REFOLOSESC între
        // view-uri (Frame.SetView doar dezactivează/activează aceleași obiecte, iar
        // în Blazor toate view-urile rădăcină trec prin MainWindow), deci un cache
        // nereseta se lipea de tipul următor: FCL→FCT lăsa `Numar` read-only pe o
        // factură care își CULEGE numărul (blocaj: operarea îl cere, UI-ul nu-l
        // dă), iar FCT→FCL îl lăsa editabil pe seria fiscală (`AsignaNumar` iese pe
        // număr nenul ⇒ seria ocolită, exact ce D7 trebuia să prevină).
        areNumerotare = null;
        base.OnDeactivated();
    }

    void OnCurrentObjectChanged(object sender, EventArgs e) => Aplica();

    void Aplica() {
        var doc = ViewCurrentObject;
        if (doc == null)
            return;
        areNumerotare ??= AreNumerotare(doc);
        // ViewItems există deja la activare (CompositeView.LoadModel le creează din
        // model; doar CONTROALELE sunt amânate), deci flag-ul se poate pune înainte
        // de randare — editorul îl citește la crearea controlului.
        if (View.FindItem(nameof(Document.Numar)) is PropertyEditor editor)
            editor.AllowEdit[CheieCapacitate] = !areNumerotare.Value;
    }

    bool AreNumerotare(Document doc) {
        try {
            var tip = MotorOperare.GasesteTipDocument(ObjectSpace, doc);
            return ObjectSpace.FirstOrDefault<PoliticaNumerotare>(p => p.TipDocumentId == tip.ID) != null;
        }
        catch (OperareException) {
            // Ancora TipDocument lipsește (bază neseed-uită): nu blocăm culegerea —
            // operarea va refuza oricum, cu mesajul motorului.
            return false;
        }
    }
}
