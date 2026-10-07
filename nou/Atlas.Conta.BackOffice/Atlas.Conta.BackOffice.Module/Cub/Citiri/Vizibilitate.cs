using Atlas.Conta.BackOffice.Module.Api;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Security;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

public static class Vizibilitate {
    /// <summary>Contextul (securizat sau nu) vede cel puțin o postare, pe orice domeniu.</summary>
    public static bool ArePostari(IObjectSpace os) => os.GetObjectsQuery<Postare>().Any();

    /// <summary>Lipsurile de citire completă pe `Postare` care golesc lista de evidență a cubului; lista goală = acces (D9-A12 c).</summary>
    public static List<string> AccesLista(IObjectSpace os, ISelectDataSecurityProvider securitate) =>
        AccesComplet.Lipsuri(os, securitate, [typeof(Postare)]);
}
