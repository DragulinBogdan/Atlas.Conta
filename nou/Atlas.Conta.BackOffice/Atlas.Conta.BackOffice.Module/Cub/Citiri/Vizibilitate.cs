using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

public static class Vizibilitate {
    /// <summary>Contextul (securizat sau nu) vede cel puțin o postare, pe orice domeniu.</summary>
    public static bool ArePostari(IObjectSpace os) => os.GetObjectsQuery<Postare>().Any();
}
