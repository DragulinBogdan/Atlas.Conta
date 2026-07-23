using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Controllers;

// Completează gardianul de editare din DocumentEditareController (care oprea DOAR
// DetailView-ul master la nivel de View.AllowEdit): editoarele NESTED ale colecției
// Detalii rămâneau editabile pe un document deja operat, deși liniile sunt
// load-bearing după operare (registrele s-au scris din ele). Aici blocăm liniile
// nested când masterul nu mai e Draft — AllowEdit + AllowNew + AllowDelete.
//
// ObjectViewController pe DocumentDetaliu (baza) acoperă automat toate listele
// nested ale derivatelor (FacturaIesireDetaliu, NIR-ul etc.). TargetViewNesting
// = Nested restrânge la colecția dintr-un DetailView de Document (DocumentDetaliu
// n-are ListView rădăcină, dar scoping-ul rămâne explicit — ca DefaultTipTvaController).
//
// Corecția legitimă rămâne Anulează operarea (→ Draft, redevine editabil) sau
// Stornează — exact ca la gardianul de header.
public class DocumentDetaliiEditareController : ObjectViewController<ListView, DocumentDetaliu> {
    public DocumentDetaliiEditareController() {
        TargetViewNesting = Nesting.Nested;
    }

    protected override void OnActivated() {
        base.OnActivated();
        Aplica();
        // Post-operare: MotorOperare comite pe OS-ul master-ului (același OS ca
        // lista nested — colecție agregată), deci Committed re-evaluează starea
        // fără reconstrucția view-ului. Același mecanism ca DocumentEditareController.
        ObjectSpace.Committed += OnSchimbare;
    }

    protected override void OnDeactivated() {
        ObjectSpace.Committed -= OnSchimbare;
        base.OnDeactivated();
    }

    void OnSchimbare(object sender, EventArgs e) => Aplica();

    void Aplica() {
        // Masterul se citește din frame-ul nested (docs DevExpress 112912);
        // back-reference-ul liniei nu e inițializat pre-commit (402990), dar aici
        // ne interesează DOAR starea documentului-gazdă.
        var master = (Frame as NestedFrame)?.ViewItem?.CurrentObject as Document;
        // Master lipsă (list root teoretic) → nu impunem nimic; Draft → editabil.
        bool editabil = master == null || master.Stare == StareDocument.Draft;
        View.AllowEdit["Stare"] = editabil;
        View.AllowNew["Stare"] = editabil;
        View.AllowDelete["Stare"] = editabil;
    }
}

// Geamănul pe DetailView-ul LINIEI (review advers polish XAF): gardianul de mai
// sus oprește doar lista — click pe rând deschide DetailView-ul liniei în același
// ObjectSpace, iar un Save de acolo ar edita o linie a unui document Operat pe
// lângă registrele deja scrise. Masterul se citește din back-reference (pe linii
// persistate se încarcă lazy; liniile NOI n-au back-reference pre-commit — 402990 —
// dar ele există doar pe documente Draft, deci null → editabil e corect).
public class DocumentDetaliuDetailEditareController : ObjectViewController<DetailView, DocumentDetaliu> {
    protected override void OnActivated() {
        base.OnActivated();
        Aplica();
        ObjectSpace.Committed += OnSchimbare;
    }

    protected override void OnDeactivated() {
        ObjectSpace.Committed -= OnSchimbare;
        base.OnDeactivated();
    }

    void OnSchimbare(object sender, EventArgs e) => Aplica();

    void Aplica() {
        var master = ViewCurrentObject?.Document;
        bool editabil = master == null || master.Stare == StareDocument.Draft;
        View.AllowEdit["Stare"] = editabil;
        View.AllowDelete["Stare"] = editabil;
    }
}
