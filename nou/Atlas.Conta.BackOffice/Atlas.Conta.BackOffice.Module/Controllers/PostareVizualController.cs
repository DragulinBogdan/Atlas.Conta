using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.Cub;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using DevExpress.Data.Filtering;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;

namespace Atlas.Conta.BackOffice.Module.Controllers;

/// <summary>Lista de evidență a cubului refuză la activare rolul fără citire completă pe `Postare` (D9-A12 c).</summary>
public class PostareVizualController : ObjectViewController<ListView, PostareVizual> {
    public const string CheieAcces = "Acces";

    protected override void OnActivated() {
        base.OnActivated();
        if (Application?.Security is not ISelectDataSecurityProvider securitate)
            return;
        var refuz = Aplica(View.CollectionSource, Vizibilitate.AccesLista(ObjectSpace, securitate));
        if (refuz != null)
            Application.ShowViewStrategy.ShowMessage(new MessageOptions {
                Message = refuz, Type = InformationType.Warning, Duration = 10000,
            });
    }

    /// <summary>Pune criteriul fals pe sursa listei când există lipsuri și întoarce textul refuzului; `null` = acces.</summary>
    public static string Aplica(CollectionSourceBase sursa, IReadOnlyList<string> lipsuri) {
        if (lipsuri.Count == 0) {
            sursa.Criteria.Remove(CheieAcces);
            return null;
        }
        sursa.Criteria[CheieAcces] = CriteriaOperator.Parse("1 = 0");
        return $"{Refuzuri.FaraDrept(OperatieAcces.Citire, typeof(Postare))} Restricționate: {string.Join(", ", lipsuri)}.";
    }
}
