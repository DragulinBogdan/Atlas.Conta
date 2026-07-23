using System.ComponentModel;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Controllers;

// Imperecherea (decizia 31d) = stingerea m2m plată↔document; invarianții trăiesc
// în ImperechereService, dar New generic prin UI îi ocolea. Decizia: New rămâne
// PERMIS, dar validat la commit; Edit blocat (șterge + recreează); Delete liber
// (link-ul se desface fără registre proprii — gardianul de anulare/storno există
// deja în motor). Calea nu se suprapune cu motorul: controller-ul se activează
// DOAR pe view-urile de Imperechere; imperecherea automată a plății se creează în
// OS-ul documentului (view de Document), unde acest controller nu e activ.
public sealed class ImperechereController : ViewController {
    public ImperechereController() {
        TargetObjectType = typeof(Imperechere);
        TargetViewType = ViewType.Any;
    }

    protected override void OnActivated() {
        base.OnActivated();
        AplicaCapabilitati();
        View.CurrentObjectChanged += OnCurrentObjectChanged;
        View.ObjectSpace.Committing += OnCommitting;
        // După Save-ul unui obiect NOU, CurrentObjectChanged nu se declanșează —
        // fără re-evaluare pe Committed view-ul ar rămâne editabil (backstop-ul
        // din OnCommitting refuza abia la commit); pattern-ul 40c.
        View.ObjectSpace.Committed += OnCommitted;
    }

    protected override void OnDeactivated() {
        View.CurrentObjectChanged -= OnCurrentObjectChanged;
        View.ObjectSpace.Committing -= OnCommitting;
        View.ObjectSpace.Committed -= OnCommitted;
        base.OnDeactivated();
    }

    void OnCurrentObjectChanged(object sender, EventArgs e) => AplicaCapabilitati();

    void OnCommitted(object sender, EventArgs e) => AplicaCapabilitati();

    // ListView: editarea inline a link-urilor e interzisă (invarianții nu se pot
    // reverifica pe editare parțială) — New/Delete rămân. DetailView: editabil
    // DOAR pe obiect nou (trebuie cules), read-only după persistare.
    void AplicaCapabilitati() {
        View.AllowEdit["Invarianti"] = View is DetailView
            && View.ObjectSpace.IsNewObject(View.CurrentObject);
    }

    void OnCommitting(object sender, CancelEventArgs e) {
        var os = View.ObjectSpace;
        foreach (var imp in os.ModifiedObjects.OfType<Imperechere>()) {
            if (os.IsDeletedObject(imp))
                continue; // ștergerea e liberă (decizia 31d)
            if (os.IsNewObject(imp))
                // Limitare asumată (ca gardianul de sold, decizia 25f): două
                // link-uri NOI în același commit nu se văd reciproc la Σ≤rest —
                // niciunul nu e încă persistat, deci fiecare se validează contra
                // restului din baza de date (back-office cu operator unic).
                ImperechereService.ValideazaCreare(os, imp.DocumentTrezorerie, imp.Document, imp.Suma);
            else
                throw new OperareException("Imperecherea nu se editează — șterge-o și creeaz-o din nou.");
        }
    }
}
