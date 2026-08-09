using Atlas.Conta.BackOffice.Module.Api.Fcl;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Actions;
using DevExpress.Persistent.Base;
using Microsoft.Extensions.DependencyInjection;

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
        var fclId = ViewCurrentObject.ID;

        // Gate de autorizare, ca la DocumentOperareController (spike D-F1): ușa
        // non-secured de mai jos e a MOTORULUI — cine nu are Write pe document
        // prin securitatea XAF nu comandă generarea.
        if (Application.Security is not DevExpress.ExpressApp.Security.IRequestSecurityStrategy cerinte
                || !DevExpress.ExpressApp.Security.IsGrantedExtensions.CanWrite(
                        cerinte, ObjectSpace, (object)ViewCurrentObject))
            throw new UserFriendlyException("Nu aveți dreptul de scriere necesar pentru generarea descărcării.");

        // Comanda rulează pe ObjectSpace NON-SECURED, ca pe tierul Web API
        // (F4-D3, review advers D1): `DescarcareService.Genereaza` scrie
        // `Autogenerat` + `DocumentSursa` — câmpuri server-owned pe care
        // `GardianEditare` le refuză pe orice cale secured, inclusiv OS-ul creat
        // de controller prin `Application.CreateObjectSpace` (familia View-urilor
        // Blazor). Aceeași intrare de Module ca endpoint-ul: Apply comite și
        // întoarce restul recalculat DUPĂ commit (draftul contează la acoperire).
        GenerareDescarcareRezultatDto rezultat;
        var fabrica = Application.ServiceProvider.GetRequiredService<INonSecuredObjectSpaceFactory>();
        using (var osMotor = fabrica.CreateNonSecuredObjectSpace(typeof(DescarcareGestiune))) {
            try {
                rezultat = FacturaIesireApply.GenereazaDescarcare(osMotor, fclId, data);
            }
            catch (OperareException ex) {
                throw new UserFriendlyException(ex.Message);
            }
        }

        if (rezultat.DscId is not Guid dscId) {
            // Nimic de alocat (fără linii de stoc, fără sold, sau tot acoperit).
            // Raportăm restul interogabil per linie (design §5: rămâne cusătura
            // pe care se așază fluxul de comenzi la pasul 5).
            Informeaza("Nimic de descărcat.", RezumaResturi(rezultat.Resturi));
            return;
        }

        // Deschidem DSC-ul în editare pe un ObjectSpace de VIEW propriu
        // (pattern-ul conexului din DocumentOperareController: puntea între
        // ObjectSpace-uri e ID-ul, nu instanța).
        var osView = Application.CreateObjectSpace(typeof(DescarcareGestiune));
        e.ShowViewParameters.CreatedView = Application.CreateDetailView(
            osView, osView.GetObjectByKey<DescarcareGestiune>(dscId));
        e.ShowViewParameters.TargetWindow = TargetWindow.Default;

        if (rezultat.Resturi.Count > 0)
            Informeaza("Descărcare generată. Rest nedescărcat (backorder), interogabil per linie:",
                RezumaResturi(rezultat.Resturi));
    }

    // Rezumat produs × rest din DTO-ul comenzii (denumirile vin deja proiectate).
    static string RezumaResturi(IReadOnlyList<RestNedescarcatRandDto> resturi) =>
        string.Join("; ", resturi.Select(x => $"{x.ProdusDenumire ?? "?"} × {x.Rest:0.###}"));

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
