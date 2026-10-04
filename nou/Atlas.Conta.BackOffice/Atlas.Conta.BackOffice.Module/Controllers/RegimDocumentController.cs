using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.ExpressApp.SystemModule;

namespace Atlas.Conta.BackOffice.Module.Controllers;

// Adaptorul XAF al regimului pe stare (106d): editabilitatea view-ului și comenzile frame-ului,
// potrivite după sufixul ID-ului acțiunii („Document.Opereaza” → „Opereaza”).
public class RegimDocumentController : ObjectViewController<DetailView, Document> {
    const string Cheie = "Regim";
    readonly Dictionary<string, string> tooltipuri = [];

    protected override void OnActivated() {
        base.OnActivated();
        Aplica();
        View.CurrentObjectChanged += OnSchimbare;
        ObjectSpace.Committed += OnSchimbare;
        ObjectSpace.Reloaded += OnSchimbare;
    }

    protected override void OnDeactivated() {
        View.CurrentObjectChanged -= OnSchimbare;
        ObjectSpace.Committed -= OnSchimbare;
        ObjectSpace.Reloaded -= OnSchimbare;
        base.OnDeactivated();
    }

    void OnSchimbare(object sender, EventArgs e) => Aplica();

    void Aplica() {
        var doc = ViewCurrentObject;
        var regim = doc == null ? null : RegimDocument.Calculeaza(ObjectSpace, doc);
        View.AllowEdit[Cheie] = regim?.Editabil ?? true;
        foreach (var actiune in Frame.Controllers.Cast<Controller>().SelectMany(c => c.Actions)) {
            var comanda = Comanda(actiune);
            if (comanda == null)
                continue;
            tooltipuri.TryAdd(actiune.Id, actiune.ToolTip);
            var motiv = regim?.Motiv(comanda);
            actiune.Enabled[Cheie] = regim == null || motiv == null;
            actiune.ToolTip = motiv ?? tooltipuri[actiune.Id];
        }
    }

    public static string Comanda(ActionBase actiune) {
        if (actiune.Controller is DeleteObjectsViewController sters && actiune == sters.DeleteAction)
            return RegimDocument.Sterge;
        var sufix = actiune.Id[(actiune.Id.LastIndexOf('.') + 1)..];
        return RegimDocument.ComenziCunoscute.Contains(sufix) ? sufix : null;
    }
}

// Lista de documente aplică pe selecție componenta ieftină a ștergerii (106j).
public class RegimListaController : ObjectViewController<ListView, Document> {
    const string Cheie = "Regim";
    SimpleAction stergere;
    string tooltip;

    protected override void OnActivated() {
        base.OnActivated();
        stergere = Frame.GetController<DeleteObjectsViewController>()?.DeleteAction;
        tooltip = stergere?.ToolTip;
        View.SelectionChanged += OnSelectie;
        Aplica();
    }

    protected override void OnDeactivated() {
        View.SelectionChanged -= OnSelectie;
        if (stergere != null) {
            stergere.Enabled.RemoveItem(Cheie);
            stergere.ToolTip = tooltip;
        }
        base.OnDeactivated();
    }

    void OnSelectie(object sender, EventArgs e) => Aplica();

    void Aplica() {
        if (stergere == null)
            return;
        var selectate = View.SelectedObjects.OfType<Document>().ToList();
        var motive = selectate.Select(RegimDocument.MotivStergere).Where(m => m != null).ToList();
        stergere.Enabled[Cheie] = motive.Count == 0;
        stergere.ToolTip = motive.Count == 0 ? tooltip
            : selectate.Count == 1 ? motive[0]
            : $"{motive.Count} din {selectate.Count} documente selectate nu se pot șterge. {motive[0]}";
    }
}

// Liniile nested și dialogul liniei urmează componenta ieftină a regimului, pe masterul lor.
public class RegimLiniiController : ObjectViewController<ListView, DocumentDetaliu> {
    const string Cheie = "Regim";

    public RegimLiniiController() {
        TargetViewNesting = Nesting.Nested;
    }

    protected override void OnActivated() {
        base.OnActivated();
        Aplica();
        ObjectSpace.Committed += OnSchimbare;
        ObjectSpace.Reloaded += OnSchimbare;
    }

    protected override void OnDeactivated() {
        ObjectSpace.Committed -= OnSchimbare;
        ObjectSpace.Reloaded -= OnSchimbare;
        base.OnDeactivated();
    }

    void OnSchimbare(object sender, EventArgs e) => Aplica();

    void Aplica() {
        // Masterul se citește din frame-ul nested (docs DevExpress 112912); back-reference-ul liniei noi lipsește pre-commit (402990).
        var editabil = RegimDocument.EsteEditabil((Frame as NestedFrame)?.ViewItem?.CurrentObject as Document);
        View.AllowEdit[Cheie] = editabil;
        View.AllowNew[Cheie] = editabil;
        View.AllowDelete[Cheie] = editabil;
    }
}

public class RegimLinieController : ObjectViewController<DetailView, DocumentDetaliu> {
    const string Cheie = "Regim";

    protected override void OnActivated() {
        base.OnActivated();
        Aplica();
        ObjectSpace.Committed += OnSchimbare;
        ObjectSpace.Reloaded += OnSchimbare;
    }

    protected override void OnDeactivated() {
        ObjectSpace.Committed -= OnSchimbare;
        ObjectSpace.Reloaded -= OnSchimbare;
        base.OnDeactivated();
    }

    void OnSchimbare(object sender, EventArgs e) => Aplica();

    void Aplica() {
        var editabil = RegimDocument.EsteEditabil(ViewCurrentObject?.Document);
        View.AllowEdit[Cheie] = editabil;
        View.AllowDelete[Cheie] = editabil;
    }
}
