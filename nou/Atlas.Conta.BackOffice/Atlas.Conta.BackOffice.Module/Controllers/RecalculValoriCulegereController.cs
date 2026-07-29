using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Controllers;

// GATE XAF (D5 + D3, GOL 3 din contract): operatorul culegea FINANCIAR ORB —
// `Valoare`/`ValoareTva`/`Total` rămâneau 0 până la operare, fiindcă formula TVA
// (`TvaService.CalculeazaValori`) era apelată doar din `PregatesteOperare`. Nu putea
// confrunta totalul cu hârtia ÎNAINTE de a scrie registrele.
//
// Aici recalculăm la culegere prin ACELAȘI helper (o singură sursă a formulei —
// seam-ul `TvaService.CalculeazaLaCulegere`, pasul 1): la schimbarea BAZEI
// (Cantitate / PretUnitar / TipTva) linia își recalculează `Valoare` + `ValoareTva`.
// Editarea DIRECTĂ a lui `ValoareTva` (sau `Valoare`) nu declanșează nimic, deci
// override-ul manual supraviețuiește până la operare, unde regula 36a
// (`pastreazaTvaCules`) îl păstrează — factura furnizorului/emisă bate rotunjirea
// noastră. Schimbarea bazei invalidează override-ul, deliberat (seam-ul e
// documentat în TvaService).
//
// D3: la alegerea produsului se precompletează `TipMaterial` din
// `Produs.TipMaterial` (doar dacă e gol) — fricțiunea principală a culegerii; baza
// de calcul nu se schimbă, deci nu recalculăm.
//
// Blazor: `ObjectChanged` vine la commit-ul CELULEI/editorului (blur, Enter,
// selecție în lookup), nu per tastă — valorile se actualizează la părăsirea
// câmpului, ceea ce e suficient pentru „un contabil tolerant" (pragul gate-ului).
public class RecalculValoriCulegereController : ObjectViewController<DetailView, Document> {
    bool inRecalcul;
    bool abonat;

    protected override void OnActivated() {
        base.OnActivated();
        // Doar tipurile cu lanț de valori cules (ILinieCuPretUnitar): FCT + FCL —
        // exact cele două ecrane ale gate-ului. Restul documentelor își
        // materializează `Valoare` din altă parte (preț de lot, valoare culeasă
        // direct), deci n-au ce recalcula. Abonarea condiționată în locul lui
        // `Active[...]`: nu forțăm dezactivarea controllerului, doar nu ascultăm.
        abonat = TipCuPretUnitarCules(View.ObjectTypeInfo?.Type);
        if (abonat)
            ObjectSpace.ObjectChanged += OnObjectChanged;
    }

    protected override void OnDeactivated() {
        if (abonat)
            ObjectSpace.ObjectChanged -= OnObjectChanged;
        abonat = false;
        base.OnDeactivated();
    }

    static bool TipCuPretUnitarCules(Type tip) =>
        tip != null && (typeof(FacturaIntrare).IsAssignableFrom(tip) || typeof(FacturaIesire).IsAssignableFrom(tip));

    void OnObjectChanged(object sender, ObjectChangedEventArgs e) {
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

        var doc = ViewCurrentObject;
        // Doar liniile documentului curent, și doar cât e Draft (după operare
        // valorile sunt cele din care s-au scris registrele).
        if (doc == null || doc.Stare != StareDocument.Draft || !doc.Detalii.Contains(linie))
            return;

        if (proprietate is nameof(FacturaIntrareDetaliu.Produs) or nameof(FacturaIntrareDetaliu.ProdusId)) {
            PrecompleteazaTip(linie);
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
            TvaService.CalculeazaLaCulegere(ObjectSpace, linie, cuPret.PretUnitar * linie.Cantitate);
        }
        finally {
            inRecalcul = false;
        }
    }

    // D3: Tipul (contul/clasa) se derivă din produsul ales, dacă operatorul nu l-a
    // cules deja. Citirea se face prin PROIECȚIE (25b), dar se scrie NAVIGAȚIA —
    // altfel editorul din UI ar rămâne pe valoarea veche (FK-ul singur nu-i spune
    // lookup-ului ce să afișeze).
    void PrecompleteazaTip(DocumentDetaliu linie) {
        if (linie.TipMaterialId != Guid.Empty)
            return;
        var produsId = linie switch {
            FacturaIntrareDetaliu d => d.ProdusId,
            FacturaIesireDetaliu d => d.ProdusId,
            _ => null,
        };
        if (produsId == null)
            return;
        var tipId = ObjectSpace.GetObjectsQuery<Produs>()
            .Where(p => p.ID == produsId.Value)
            .Select(p => p.TipMaterialId)
            .FirstOrDefault();
        if (tipId == null)
            return;
        var tip = ObjectSpace.GetObjectByKey<TipMaterial>(tipId.Value);
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
}
