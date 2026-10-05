using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Culegere;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;

namespace Atlas.Conta.BackOffice.Module.Controllers;

/// <summary>Adaptorul XAF al culegerii interactive (104c): evenimentele ecranului devin apelurile L3.</summary>
public class CulegereDocumentController : ObjectViewController<DetailView, Document> {
    bool inCulegere;

    protected override void OnActivated() {
        base.OnActivated();
        Nou();
        View.CurrentObjectChanged += OnCurrentObjectChanged;
        ObjectSpace.ObjectChanged += OnObjectChanged;
        ObjectSpace.ObjectReloaded += OnObjectReloaded;
    }

    protected override void OnDeactivated() {
        ObjectSpace.ObjectReloaded -= OnObjectReloaded;
        ObjectSpace.ObjectChanged -= OnObjectChanged;
        View.CurrentObjectChanged -= OnCurrentObjectChanged;
        base.OnDeactivated();
    }

    void OnCurrentObjectChanged(object sender, EventArgs e) => Nou();

    void Nou() {
        if (ViewCurrentObject is Document doc && ObjectSpace.IsNewObject(doc))
            CulegereDocument.Nou(doc);
    }

    // `OldValue` e populat pe EF Core (`EFCoreObjectSpace.EFCoreObject_PropertyChanged`).
    void OnObjectChanged(object sender, ObjectChangedEventArgs e) {
        if (inCulegere || ObjectSpace.IsReloading || string.IsNullOrEmpty(e.PropertyName))
            return;
        inCulegere = true;
        try {
            if (e.Object is Document doc && e.PropertyName == nameof(Document.Data) && e.OldValue is DateOnly veche)
                CulegereDocument.DataMutata(doc, veche);
            else if (e.Object is DocumentDetaliu linie && Gazda(linie) is Document gazda) {
                CulegereDocument.LinieSchimbata(ObjectSpace, gazda, linie, e.PropertyName);
                ReimprospateazaTotal();
            }
        }
        finally {
            inCulegere = false;
        }
    }

    // 109f: linia reîncărcată a fost salvată în alt spațiu, care a renormalizat tot documentul.
    void OnObjectReloaded(object sender, ObjectManipulatingEventArgs e) {
        if (inCulegere || e.Object is not DocumentDetaliu linie || Gazda(linie) is not Document gazda)
            return;
        inCulegere = true;
        try {
            var modificate = ObjectSpace.ModifiedObjects;
            foreach (var sora in gazda.Detalii.Where(l => l != linie && !ObjectSpace.IsNewObject(l)
                    && !modificate.Contains(l)).ToList())
                ObjectSpace.ReloadObject(sora);
            ReimprospateazaTotal();
        }
        finally {
            inCulegere = false;
        }
    }

    Document Gazda(DocumentDetaliu linie) {
        var doc = ViewCurrentObject;
        return doc != null && doc.Detalii.Contains(linie) ? doc : null;
    }

    // `Total` e calculat, fără notificare: XAF nu-l reîmprospătează la schimbarea unei linii.
    void ReimprospateazaTotal() {
        if (View?.FindItem(nameof(Document.Total)) is PropertyEditor editor)
            editor.Refresh();
    }
}

/// <summary>Geamănul de pe DetailView-ul liniei: `New` pe `Detalii` deschide view-ul derivatei (40a).</summary>
public class CulegereLinieController : ObjectViewController<DetailView, DocumentDetaliu> {
    bool inCulegere;

    protected override void OnActivated() {
        base.OnActivated();
        ObjectSpace.ObjectChanged += OnObjectChanged;
    }

    protected override void OnDeactivated() {
        ObjectSpace.ObjectChanged -= OnObjectChanged;
        base.OnDeactivated();
    }

    void OnObjectChanged(object sender, ObjectChangedEventArgs e) {
        if (inCulegere || ObjectSpace.IsReloading || string.IsNullOrEmpty(e.PropertyName)
                || e.Object is not DocumentDetaliu linie || linie.Document is not Document gazda)
            return;
        inCulegere = true;
        try {
            CulegereDocument.LinieSchimbata(ObjectSpace, gazda, linie, e.PropertyName);
            ReimprospateazaDupaScriere(linie, e);
        }
        finally {
            inCulegere = false;
        }
    }

    // `PropertyEditor.ReadValue` e ignorat cât editorul își scrie valoarea (DevExpress PropertyEditor.cs:257):
    // când culegerea a schimbat chiar proprietatea editată, recitirea se amână după scriere.
    void ReimprospateazaDupaScriere(DocumentDetaliu linie, ObjectChangedEventArgs e) {
        if (View.FindItem(e.PropertyName) is not PropertyEditor editor
                || Equals(editor.MemberInfo.GetValue(linie), e.NewValue))
            return;
        SynchronizationContext.Current?.Post(_ => editor.ReadValue(), null);
    }
}
