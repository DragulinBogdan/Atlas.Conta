using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;

namespace Atlas.Conta.BackOffice.Module.Controllers;

// P2 (design §5): calea BACKORDER a descărcării de gestiune. La operarea FCL,
// DSC-ul conex se generează automat (GenereazaSecundar → DescarcareService); dar
// pozițiile fără stoc la data facturării rămân nedescărcate (venitul se postează
// acum, costul când sosește marfa pe FCT→NIR). Această acțiune re-rulează
// generatorul pentru RESTUL nedescărcat, pe FCL deja operată.
//
// Doar deleagă la DescarcareService (aceeași cale o va folosi și tierul Web API):
// generarea trăiește în motor; controllerul culege data, comite draftul într-un
// ObjectSpace propriu și îl deschide în editare (utilizatorul verifică
// override-urile de lot și îl operează separat, ca la conexul FCT→NIR).
public class FacturaIesireDescarcareController : ObjectViewController<DetailView, FacturaIesire> {
    readonly ParametrizedAction genereaza;

    public FacturaIesireDescarcareController() {
        // Data se culege (design §5: descărcarea târzie nu poate precede stocul;
        // gardianul de sold acoperă natural). ParametrizedAction minimal — un
        // editor de dată în toolbar, fără dialog; default azi, gol → azi.
        genereaza = new ParametrizedAction(this, "FacturaIesire.GenereazaDescarcarea",
            PredefinedCategory.RecordEdit, typeof(DateTime)) {
            Caption = "Generează descărcarea",
            ToolTip = "Generează descărcarea de gestiune pentru restul nedescărcat (backorder), la data indicată.",
            NullValuePrompt = "Data descărcării",
        };
        genereaza.Execute += Genereaza_Execute;
    }

    void Genereaza_Execute(object sender, ParametrizedActionExecuteEventArgs e) {
        // Gol / neschimbat (DateTime.MinValue) → azi.
        var aleasa = e.ParameterCurrentValue is DateTime dt && dt != default ? dt : DateTime.Today;
        var data = DateOnly.FromDateTime(aleasa);

        // ObjectSpace propriu: FCL e deja operată/comisă, nu edităm nimic pe ea —
        // izolăm generarea de OS-ul editorului (spre deosebire de MotorOperare,
        // unde modificările editorului intră în aceeași tranzacție cu registrele).
        var os = Application.CreateObjectSpace(typeof(DescarcareGestiune));
        var fcl = (FacturaIesire)os.GetObject(ViewCurrentObject);

        var dsc = DescarcareService.Genereaza(os, fcl, data);
        if (dsc == null) {
            // Nimic de alocat (fără linii de stoc, fără sold, sau tot acoperit).
            // Raportăm restul interogabil per linie (design §5: rămâne cusătura
            // pe care se așază fluxul de comenzi la pasul 5).
            var resturi = DescarcareService.RestNedescarcat(os, fcl).Where(x => x.RestNeacoperit > 0).ToList();
            Informeaza("Nimic de descărcat.", RezumaResturi(os, resturi));
            os.Dispose();
            return;
        }

        os.CommitChanges();

        // Draftul contează la acoperire (design §5, anti-dublare): restul se
        // recalculează DUPĂ commit — arată ce a mai rămas backorder.
        var ramase = DescarcareService.RestNedescarcat(os, fcl).Where(x => x.RestNeacoperit > 0).ToList();

        // Deschidem DSC-ul în editare (pattern-ul conexului din DocumentOperareController).
        e.ShowViewParameters.CreatedView = Application.CreateDetailView(os, dsc);
        e.ShowViewParameters.TargetWindow = TargetWindow.Default;

        if (ramase.Count > 0)
            Informeaza("Descărcare generată. Rest nedescărcat (backorder), interogabil per linie:",
                RezumaResturi(os, ramase));
    }

    // Rezumat produs × cantitate al resturilor (proiecție server-side, 25b).
    // Reutilizat de DocumentOperareController la operarea FCL (design §5:
    // restul se raportează și pe „mesajul operării").
    internal static string RezumaResturi(IObjectSpace os,
        IReadOnlyList<(Guid LinieId, Guid? ProdusId, Guid? LotId, decimal Cantitate, decimal Acoperit, decimal RestNeacoperit)> resturi) {
        if (resturi.Count == 0)
            return string.Empty;
        var idsProdus = resturi.Where(x => x.ProdusId != null).Select(x => x.ProdusId.Value).Distinct().ToList();
        var denumiri = os.GetObjectsQuery<Produs>()
            .Where(p => idsProdus.Contains(p.ID))
            .Select(p => new { p.ID, p.Denumire })
            .ToDictionary(x => x.ID, x => x.Denumire);
        return string.Join("; ", resturi.Select(x =>
            $"{(x.ProdusId != null ? denumiri.GetValueOrDefault(x.ProdusId.Value) ?? "?" : "?")} × {x.RestNeacoperit:0.###}"));
    }

    void Informeaza(string prefix, string detaliu) {
        var mesaj = string.IsNullOrEmpty(detaliu) ? prefix : $"{prefix} {detaliu}";
        Application.ShowViewStrategy.ShowMessage(new MessageOptions {
            Message = mesaj,
            Type = InformationType.Info,
            Duration = 6000,
        });
    }

    protected override void OnActivated() {
        base.OnActivated();
        // Default azi în editorul din toolbar (cosmetic; coalesce-ul din Execute
        // rămâne autoritatea pe gol/MinValue).
        genereaza.Value = DateTime.Today;
        ActualizeazaDisponibilitatea();
        View.CurrentObjectChanged += OnCurrentObjectChanged;
    }

    protected override void OnDeactivated() {
        View.CurrentObjectChanged -= OnCurrentObjectChanged;
        base.OnDeactivated();
    }

    void OnCurrentObjectChanged(object sender, EventArgs e) => ActualizeazaDisponibilitatea();

    // Backorder-ul are sens doar pe FCL OPERATĂ (ca Anulează/Stornează în
    // DocumentOperareController): pe draft nu există încă acoperire de generat.
    void ActualizeazaDisponibilitatea() =>
        genereaza.Enabled["Stare"] = ViewCurrentObject?.Stare == StareDocument.Operat;
}
