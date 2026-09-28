using DevExpress.ExpressApp;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

public static class Produse {
    /// <summary>Produsul are cel puțin o postare în cub, pe orice fel de tranzacție.</summary>
    public static bool AreMiscari(IObjectSpace os, Guid produs) => os.GetObjectsQuery<Postare>().Any(p => p.Produs == produs);
}
