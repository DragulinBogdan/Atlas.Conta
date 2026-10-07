#nullable enable
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

/// <summary>Contorul curent al unui rând de politică reținut în explicație (D9-A8).</summary>
public static class VersiuniPolitica {
    static readonly IReadOnlyDictionary<string, Type> tipuri = typeof(Politica).Assembly.GetTypes()
        .Where(t => !t.IsAbstract && typeof(Politica).IsAssignableFrom(t))
        .ToDictionary(t => t.Name);

    /// <summary>`OptimisticLockField` al rândului azi; null când rândul a dispărut sau felul e necunoscut.</summary>
    public static int? Contor(IObjectSpace os, N.VersiunePolitica versiune) =>
        tipuri.TryGetValue(versiune.Fel, out var tip) && os.GetObjectByKey(tip, versiune.Rand) is Editabila rand
            ? rand.OptimisticLockField
            : null;
}
