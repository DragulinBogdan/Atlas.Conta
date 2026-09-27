using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using DevExpress.ExpressApp;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Saft;

public sealed class MapariFiscale(IObjectSpace os) {
    public const string Versiune = "RO-16.02.2026";
    readonly Dictionary<(SectiuneTvaSaft, Guid, RegimTva, decimal, bool, SensTva, N.RolTva), MapareTvaSaft> mapari =
        os.GetObjectsQuery<MapareTvaSaft>().Where(m => m.Versiune == Versiune).ToList()
            .ToDictionary(m => (m.Sectiune, m.TipTvaId, m.Regim, m.Cota, m.DeImport, m.Sens, m.Rol));

    public MapareTvaSaft Pentru(FaptFiscal fapt, SectiuneTvaSaft sectiune,
            N.RolTva rol = N.RolTva.Taxa) =>
        mapari.GetValueOrDefault((sectiune, fapt.TipTvaId, fapt.Regim, fapt.Cota, fapt.DeImport, fapt.Sens, rol));
}
