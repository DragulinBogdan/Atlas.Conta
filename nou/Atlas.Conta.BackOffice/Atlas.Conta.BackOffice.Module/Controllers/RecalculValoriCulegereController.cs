using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Controllers;

// GATE XAF (D5 + D3, GOL 3 din contract): operatorul culegea FINANCIAR ORB —
// `Valoare`/`ValoareTva`/`Total` rămâneau 0 până la operare, fiindcă formula TVA
// (`TvaService.CalculeazaValori`) era apelată doar din `PregatesteOperare`. Nu putea
// confrunta totalul cu hârtia ÎNAINTE de a scrie registrele.
//
// Recalculul la culegere folosește ACELAȘI helper (o singură sursă a formulei —
// seam-ul `TvaService.CalculeazaLaCulegere`, pasul 1): la schimbarea BAZEI
// (Cantitate / PretUnitar / TipTva) linia își recalculează `Valoare` + `ValoareTva`.
// Editarea DIRECTĂ a lui `ValoareTva` (sau `Valoare`) nu declanșează nimic, deci
// override-ul manual supraviețuiește până la operare, unde regula 36a
// (`pastreazaTvaCules`) îl păstrează — factura furnizorului/emisă bate rotunjirea
// noastră. Schimbarea bazei invalidează override-ul, deliberat.
//
// D3: la alegerea produsului se precompletează `TipMaterial` din
// `Produs.TipMaterial` (doar dacă e gol) — fricțiunea principală a culegerii; baza
// de calcul nu se schimbă, deci nu recalculăm.
//
// Blazor: `ObjectChanged` vine la commit-ul CELULEI/editorului (blur, Enter,
// selecție în lookup), nu per tastă — valorile se actualizează la părăsirea
// câmpului, ceea ce e suficient pentru „un contabil tolerant" (pragul gate-ului).
static class RecalculCulegere {
    // Declanșatorii: baza de calcul (recalcul) și produsul (precompletare de Tip).
    public static void Reactioneaza(IObjectSpace os, ObjectChangedEventArgs e, Func<DocumentDetaliu, Document> gazda, ref bool inRecalcul) {
        // Guard de reintrare: scrierea lui `Valoare`/`ValoareTva`/`TipMaterial`
        // ridică la rândul ei ObjectChanged.
        if (inRecalcul)
            return;
        if (e.Object is not DocumentDetaliu linie)
            return;
        // PropertyName e null pe căile fără membru identificat (reload / refresh
        // global al obiectului) — nu se știe ce s-a schimbat, deci nu atingem nimic.
        var proprietate = e.PropertyName;
        if (string.IsNullOrEmpty(proprietate))
            return;

        // Doar cât documentul-gazdă e Draft (după operare valorile sunt cele din
        // care s-au scris registrele).
        var doc = gazda(linie);
        if (doc == null || doc.Stare != StareDocument.Draft)
            return;

        if (proprietate is nameof(FacturaIntrareDetaliu.Produs) or nameof(FacturaIntrareDetaliu.ProdusId)) {
            PrecompleteazaTip(os, linie, ref inRecalcul);
            return;
        }
        if (linie is not ILinieCuPretUnitar cuPret)
            return;
        if (proprietate is not (nameof(DocumentDetaliu.Cantitate)
                or nameof(FacturaIntrareDetaliu.PretUnitar)
                or nameof(DocumentDetaliu.TipTva)
                or nameof(DocumentDetaliu.TipTvaId)))
            return;

        inRecalcul = true;
        try {
            TvaService.CalculeazaLaCulegere(os, linie, cuPret.PretUnitar * linie.Cantitate);
        }
        finally {
            inRecalcul = false;
        }
    }

    // D3: Tipul (contul/clasa) se derivă din produsul ales, dacă operatorul nu l-a
    // cules deja. Citirea se face prin PROIECȚIE (25b), dar se scrie NAVIGAȚIA —
    // altfel editorul din UI ar rămâne pe valoarea veche (FK-ul singur nu-i spune
    // lookup-ului ce să afișeze).
    static void PrecompleteazaTip(IObjectSpace os, DocumentDetaliu linie, ref bool inRecalcul) {
        if (linie.TipMaterialId != Guid.Empty)
            return;
        var produsId = linie switch {
            FacturaIntrareDetaliu d => d.ProdusId,
            FacturaIesireDetaliu d => d.ProdusId,
            _ => null,
        };
        if (produsId == null)
            return;
        var tipId = os.GetObjectsQuery<Produs>()
            .Where(p => p.ID == produsId.Value)
            .Select(p => p.TipMaterialId)
            .FirstOrDefault();
        if (tipId == null)
            return;
        var tip = os.GetObjectByKey<TipMaterial>(tipId.Value);
        if (tip == null)
            return;
        inRecalcul = true;
        try {
            linie.TipMaterial = tip;
        }
        finally {
            inRecalcul = false;
        }
    }

    // Tipurile cu lanț de valori cules (ILinieCuPretUnitar): FCT + FCL — exact cele
    // două ecrane ale gate-ului. Restul documentelor își materializează `Valoare`
    // din altă parte (preț de lot, valoare culeasă direct), deci n-au ce recalcula.
    public static bool TipCuPretUnitarCules(Type tip) =>
        tip != null && (typeof(FacturaIntrare).IsAssignableFrom(tip) || typeof(FacturaIesire).IsAssignableFrom(tip));
}

// Calea „editare în grila nested / master deschis".
public class RecalculValoriCulegereController : ObjectViewController<DetailView, Document> {
    bool inRecalcul;
    bool abonat;

    protected override void OnActivated() {
        base.OnActivated();
        // Abonare condiționată în locul lui `Active[...]`: nu forțăm dezactivarea
        // controllerului, doar nu ascultăm pe tipurile fără preț cules.
        abonat = RecalculCulegere.TipCuPretUnitarCules(View.ObjectTypeInfo?.Type);
        if (abonat)
            ObjectSpace.ObjectChanged += OnObjectChanged;
    }

    protected override void OnDeactivated() {
        if (abonat)
            ObjectSpace.ObjectChanged -= OnObjectChanged;
        abonat = false;
        base.OnDeactivated();
    }

    void OnObjectChanged(object sender, ObjectChangedEventArgs e) =>
        RecalculCulegere.Reactioneaza(ObjectSpace, e, Gazda, ref inRecalcul);

    Document Gazda(DocumentDetaliu linie) {
        var doc = ViewCurrentObject;
        return doc != null && doc.Detalii.Contains(linie) ? doc : null;
    }
}

// Geamănul de pe DetailView-ul LINIEI — găsit la smoke-ul UI real (GATE XAF, pasul
// 4): New pe colecția `Detalii` deschide DetailView-ul derivatei (mecanismul 40a),
// un view SEPARAT pe care controllerul de mai sus (țintit pe `Document`) nu se
// activează. Pe calea aceea — care e calea NORMALĂ de culegere a unei linii —
// lipseau ambele: D3 (Tipul nu se precompleta din produs) și D5 (`Valoare` /
// `ValoareTva` rămâneau 0). Același tipar de geamăn ca la read-only
// (`DocumentDetaliuDetailEditareController`) și la lotul read-only din baseline.
public class RecalculValoriLinieController : ObjectViewController<DetailView, DocumentDetaliu> {
    bool inRecalcul;

    protected override void OnActivated() {
        base.OnActivated();
        ObjectSpace.ObjectChanged += OnObjectChanged;
    }

    protected override void OnDeactivated() {
        ObjectSpace.ObjectChanged -= OnObjectChanged;
        base.OnDeactivated();
    }

    void OnObjectChanged(object sender, ObjectChangedEventArgs e) =>
        RecalculCulegere.Reactioneaza(ObjectSpace, e, Gazda, ref inRecalcul);

    // Documentul-gazdă vine din back-reference-ul liniei. Pe linia NOUĂ deschisă în
    // dialog el e deja materializat (`CommitMasterObject` a comis masterul înainte
    // de a deschide view-ul liniei — docs DevExpress 402990 se referă la momentul
    // ObjectCreated, dinaintea acestui punct); guard-ul pe null îl acoperă oricum,
    // iar filtrul de tip lasă în joc doar liniile de FCT/FCL.
    Document Gazda(DocumentDetaliu linie) {
        var doc = linie.Document;
        return doc != null && RecalculCulegere.TipCuPretUnitarCules(doc.GetType()) ? doc : null;
    }
}
