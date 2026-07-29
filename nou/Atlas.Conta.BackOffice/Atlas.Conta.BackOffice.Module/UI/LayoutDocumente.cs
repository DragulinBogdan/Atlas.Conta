using DevExpress.ExpressApp.Model;
using DevExpress.ExpressApp.Model.Core;
using DevExpress.ExpressApp.Model.NodeGenerators;
using DevExpress.ExpressApp.Utils;

namespace Atlas.Conta.BackOffice.Module.UI;

// Grupurile de layout ale DetailView-urilor de document (GATE XAF D12).
//
// Repartizarea membrilor pe grupuri se face DECLARATIV, cu `[DetailViewLayout]`
// pe proprietăți — mecanismul NATIV al generatorului de layout
// (ModelDetailViewLayoutNodesGenerator): un grup cu id necunoscut e creat sub
// `Main`, în poziția GroupIndex, iar membrii neanotați rămân în `SimpleEditors`.
// EntityFluent NU putea face asta: `.Section()/.Group()` doar ADAUGĂ item-urile
// care lipsesc din layout (EnsureItemIfMissing verifică tot arborele), iar la
// momentul customizer-ului layout-ul auto-generat conține deja toți membrii —
// grupurile ar fi ieșit goale. Vezi raportul feliei.
//
// Constrângere a generatorului (AddToCustomLayoutGroups): toate proprietățile
// care împart un GroupId TREBUIE să aibă aceleași GroupType/GroupIndex — altfel
// aruncă la generarea modelului. De aici perechile id+ordine ca CONSTANTE.
public static class GrupLayout {
    // Comune tuturor documentelor (declarate pe baza `Document`).
    public const string Document = "GrupDocument";
    public const int OrdineDocument = 0;
    public const string Stare = "GrupStare";
    // Ordine mare: grupurile proprii derivatei se așază între Document și Stare.
    public const int OrdineStare = 8;

    // Proprii FacturaIntrare.
    public const string Scadenta = "GrupScadenta";
    public const int OrdineScadenta = 1;
    public const string Plata = "GrupPlata";
    public const int OrdinePlata = 2;
    public const string Altele = "GrupAltele";
    public const int OrdineAltele = 3;

    // Proprii FacturaIesire.
    public const string Livrare = "GrupLivrare";
    public const int OrdineLivrare = 1;

    // Grupul colecției `Detalii`, generat automat de XAF pentru singura colecție
    // a documentului (id = `{item}_Group`, caption golit de generator).
    public const string Detalii = "Detalii_Group";
}

// Etichetele RO ale grupurilor — singurul lucru pe care `[DetailViewLayout]` nu
// îl poartă. Rulează după generarea nodului Layout al FIECĂRUI DetailView
// (ModelDetailViewLayoutNodesGenerator): node = nodul Layout, node.Parent =
// DetailView-ul. Se aplică doar documentelor; captions-urile câmpurilor stau pe
// proprietăți ([XafDisplayName]), ca să fie aceleași în ListView și DetailView.
public sealed class LayoutDocumenteUpdater : ModelNodesGeneratorUpdater<ModelDetailViewLayoutNodesGenerator> {
    static readonly Dictionary<string, string> Etichete = new(StringComparer.Ordinal) {
        [GrupLayout.Document] = "Document",
        [GrupLayout.Scadenta] = "Scadență & PV",
        [GrupLayout.Plata] = "Plată",
        [GrupLayout.Altele] = "Altele",
        [GrupLayout.Livrare] = "Livrare",
        [GrupLayout.Stare] = "Stare & totaluri",
        [GrupLayout.Detalii] = "Detalii",
    };

    public override void UpdateNode(ModelNode node) {
        if (node is not IModelViewLayout)
            return;
        if (node.Parent is not IModelObjectView view)
            return;
        var tip = view.ModelClass?.TypeInfo?.Type;
        if (tip == null || !typeof(BusinessObjects.Document).IsAssignableFrom(tip))
            return;

        foreach (var grup in Grupuri(node))
            if (Etichete.TryGetValue(grup.Id, out var eticheta))
                AplicaEticheta(grup, eticheta);
    }

    // Arborele de layout, în adâncime. `GetNodes()` (nu `Nodes`) — el garantează
    // generarea nodurilor; lista brută poate fi încă null.
    static IEnumerable<IModelLayoutGroup> Grupuri(ModelNode node) {
        foreach (var copil in node.GetNodes()) {
            if (copil is not IModelLayoutGroup grup)
                continue;
            yield return grup;
            foreach (var nested in Grupuri(copil))
                yield return nested;
        }
    }

    // Se suprascrie DOAR default-ul (disciplina baseline-ului: diff-ul
    // utilizatorului din Model Editor rămâne prioritar). Generatorul lasă
    // caption-ul GOL pe grupurile în flux (≤4 editoare) și îl pune derivat din id
    // pe cele împărțite în două coloane (>4) — ambele variante sunt „default".
    static void AplicaEticheta(IModelLayoutGroup grup, string eticheta) {
        var derivatDinId = CaptionHelper.ConvertCompoundName(grup.Id);
        if (string.IsNullOrEmpty(grup.Caption) || string.Equals(grup.Caption, derivatDinId, StringComparison.Ordinal))
            grup.Caption = eticheta;
        grup.ShowCaption ??= true;
    }
}
