using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

public static partial class Imobilizari {
    public sealed record ConturiFisa(Guid? Activ, Guid? Amortizare);

    internal static IQueryable<Postare> Nominalizari(IObjectSpace os) {
        var pif = os.GetObjectsQuery<PunereInFunctiuneDetaliu>().Select(l => l.ID);
        var amo = os.GetObjectsQuery<AmortizareLunaraDetaliu>().Select(l => l.ID);
        return os.GetObjectsQuery<Postare>().Where(p => p.FelUnitate == N.FelUnitate.Fisa
            && p.Unitate != null && p.InversaDinId == null && p.LinieId != null
            && (pif.Contains(p.LinieId.Value) || amo.Contains(p.LinieId.Value)));
    }

    public static IReadOnlyDictionary<Guid, ConturiFisa> Conturi(IObjectSpace os, List<Guid> fise,
            DateOnly laData, Guid? exceptie = null) {
        var inverse = os.GetObjectsQuery<Postare>().Where(p => p.InversaDinId != null && p.Data <= laData)
            .Select(p => p.InversaDinId);
        var pozitii = Nominalizari(os).Where(p => fise.Contains(p.Unitate.Value) && p.Data <= laData
                && p.DocumentId != exceptie && !inverse.Contains(p.ID))
            .Select(p => new { Fisa = p.Unitate.Value, p.Cont, p.Latura }).Distinct().ToList();
        return pozitii.GroupBy(p => p.Fisa).ToDictionary(g => g.Key, g => {
            Guid? Cont(N.Latura latura) {
                var conturi = g.Where(p => p.Latura == latura).Select(p => p.Cont).Distinct().ToArray();
                if (conturi.Length > 1) throw new OperareException(
                    $"Fișa {g.Key} are conturi istorice multiple pe {latura}; este necesară reconcilierea istoricului.");
                return conturi.Length == 0 ? null : conturi[0];
            }
            return new ConturiFisa(Cont(N.Latura.Debit), Cont(N.Latura.Credit));
        });
    }
}
