using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.SystemModule;

namespace Atlas.Conta.BackOffice.Module.Controllers;

// P1, datoria din design §8: default TipTva la CULEGERE (nu în motor). Când
// utilizatorul adaugă o linie nouă în colecția agregată Detalii a unui Document,
// precompletăm TipTva din ancora tipului (TipDocument.TipTvaImplicit) ÎNAINTE ca
// editorul liniei să fie afișat — astfel N21 (privat) / CAP21 (bugetar) apar
// gata alese pe FCT/FCL/DEC, iar pe tipurile fără default (NIR/DSC/…) nu se
// întâmplă nimic. Generic pe Document/DocumentDetaliu: ancora decide prin
// TipTvaImplicitId, fără hardcodarea tipurilor.
//
// Aplicat o singură dată per linie nouă, prin NewObjectViewController.ObjectCreated
// al frame-ului nested al colecției (calea „New" în lista nested). Culegerea
// explicită NU se suprascrie (helper-ul e deja no-op dacă TipTva e setat).
public class DefaultTipTvaController : ObjectViewController<ListView, DocumentDetaliu> {
    NewObjectViewController newController;

    public DefaultTipTvaController() {
        // Doar liste nested (colecția Detalii dintr-un DetailView de Document);
        // DocumentDetaliu n-are ListView rădăcină, dar scoping-ul e explicit.
        TargetViewNesting = Nesting.Nested;
    }

    protected override void OnActivated() {
        base.OnActivated();
        newController = Frame.GetController<NewObjectViewController>();
        if (newController != null)
            newController.ObjectCreated += OnObjectCreated;
    }

    void OnObjectCreated(object sender, ObjectCreatedEventArgs e) {
        if (e.CreatedObject is not DocumentDetaliu linie)
            return;
        // EF Core: back-reference-ul liniei (linie.Document) NU e inițializat
        // până la commit (docs DevExpress 402990) — masterul se ia din frame-ul
        // nested (docs 112912). ObjectSpace-ul listei nested = OS-ul master-ului
        // (colecție agregată), deci linia și documentul trăiesc în același OS.
        if (Frame is NestedFrame nested && nested.ViewItem?.CurrentObject is Document doc)
            TvaService.AplicaTipTvaImplicit(ObjectSpace, doc, linie);
    }

    protected override void OnDeactivated() {
        if (newController != null)
            newController.ObjectCreated -= OnObjectCreated;
        newController = null;
        base.OnDeactivated();
    }
}
