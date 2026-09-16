using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Controllers;

// GATE XAF (D9): `Data` = AZI la creare. Modelul nu avea NICIUN hook de
// inițializare, deci fiecare document nou pornea pe `0001-01-01` — dată pe care
// gardianul de perioadă refuză la operare, după ce operatorul a cules tot.
// Generic pe `Document` (toate tipurile), nu doar pe cele două ecrane ale gate-ului.
//
// De ce NU `NewObjectViewController.ObjectCreated` (precedentul
// DefaultTipTvaController): verificat pe surse (26.1.3,
// `NewObjectViewController.CreateObject`) — `ObjectCreated` se ridică pe frame-ul
// UNDE s-a apăsat New (ListView-ul rădăcină / lista nested) și abia DUPĂ aceea se
// creează DetailView-ul; un controller pe DetailView-ul obiectului nou nu îl vede
// niciodată. La linia de detaliu asta funcționa fiindcă acolo controllerul stă pe
// LISTA nested, adică pe frame-ul sursă. Pentru documentul rădăcină echivalentul
// corect e activarea DetailView-ului cu un obiect NOU.
//
// Data deja setată nu se atinge: documentele conexe/secundare (NIR din FCT, plata
// automată, DSC) își iau data din sursă, iar clonele o poartă cu ele.
public class DocumentDefaultsController : ObjectViewController<DetailView, Document> {
    protected override void OnActivated() {
        base.OnActivated();
        Aplica();
        View.CurrentObjectChanged += OnCurrentObjectChanged;
        ObjectSpace.ObjectChanged += OnObjectChanged;
    }

    protected override void OnDeactivated() {
        ObjectSpace.ObjectChanged -= OnObjectChanged;
        View.CurrentObjectChanged -= OnCurrentObjectChanged;
        base.OnDeactivated();
    }

    void OnCurrentObjectChanged(object sender, EventArgs e) => Aplica();

    // F27-D4: data înregistrării URMEAZĂ data documentului cât timp cele două
    // erau egale; culeasă distinct, rămâne. `OldValue` e populat pe EF Core
    // (surse 26.1.3, `EFCoreObjectSpace.EFCoreObject_PropertyChanged` :199-201,
    // prin `propertyChangeTracker.GetPreviousValue`), spre deosebire de
    // `BaseObjectSpace.Object_PropertyChanged`, care trimite null.
    void OnObjectChanged(object sender, ObjectChangedEventArgs e) {
        if (e.Object is not Document doc || e.PropertyName != nameof(Document.Data))
            return;
        if (e.OldValue is DateOnly veche && veche == doc.DataInregistrare)
            doc.DataInregistrare = doc.Data;
    }

    void Aplica() {
        var doc = ViewCurrentObject;
        // DOAR obiecte noi: un document persistat cu `Data` goală (reziduu de date)
        // nu se modifică pe simpla deschidere a ecranului.
        if (doc == null || doc.Data != default || !ObjectSpace.IsNewObject(doc))
            return;
        doc.Data = DateOnly.FromDateTime(DateTime.Today);
        doc.DataInregistrare = doc.Data;
    }
}
