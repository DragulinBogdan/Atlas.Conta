using System.ComponentModel;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Controllers;

// Adaptor XAF peste `LoturiCulegereService.Sincronizeaza` (F2-D1): controllerul nu
// mai poartă logică — doar fereastra de execuție (`ObjectSpace.Committing`) și
// documentul curent. Motivația mecanismului (de ce Committing, self-healing-ul
// 53f, lotul nefinalizat pe draft) trăiește lângă cod, în serviciu.
public class FacturaIntrareLoturiController : ObjectViewController<DetailView, FacturaIntrare> {
    protected override void OnActivated() {
        base.OnActivated();
        ObjectSpace.Committing += OnCommitting;
    }

    protected override void OnDeactivated() {
        ObjectSpace.Committing -= OnCommitting;
        base.OnDeactivated();
    }

    void OnCommitting(object sender, CancelEventArgs e) =>
        LoturiCulegereService.Sincronizeaza(ObjectSpace, ViewCurrentObject);
}

// Geamănul de pe LISTĂ (același tipar ca `DocumentDetaliiEditareController` +
// `DocumentDetaliuDetailEditareController`): ștergerea unui document se face de
// obicei din ListView, unde controllerul de mai sus nici nu e activ, iar liniile
// cascadate ar lăsa loturile orfane. Aici NU se creează și nu se sincronizează
// nimic (într-o listă nu se culege nici produs, nici latură) — doar curățenia.
// Țintă `Document`, nu `FacturaIntrare`: acoperă și un eventual ListView pe baza
// ierarhiei; filtrul pe `FacturaIntrareDetaliu` din serviciu face selecția.
public class DocumenteLoturiCuratenieController : ObjectViewController<ListView, Document> {
    protected override void OnActivated() {
        base.OnActivated();
        ObjectSpace.Committing += OnCommitting;
    }

    protected override void OnDeactivated() {
        ObjectSpace.Committing -= OnCommitting;
        base.OnDeactivated();
    }

    void OnCommitting(object sender, CancelEventArgs e) =>
        LoturiCulegereService.CurataOrfane(ObjectSpace);
}
