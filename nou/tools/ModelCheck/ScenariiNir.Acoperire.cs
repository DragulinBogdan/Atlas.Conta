using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;
using N = Atlas.Conta.Nucleu;
using C = Atlas.Conta.BackOffice.Module.Cub;

namespace Atlas.Conta.BackOffice.ModelCheck;

sealed partial class ScenariiNir {
    void AcoperireStoc() {
        var (zero, nzero) = Constatat(4); Opereaza(nzero);
        Grup("delta zero", zero.Id, nzero, zero.Linii[0].Lot!.Value, 4, 4, 0);
        var (minus, nminus) = Constatat(3); Opereaza(nminus);
        Grup("minus", minus.Id, nminus, minus.Linii[0].Lot!.Value, 3, 4, -1);
        var (plus, nplus) = Constatat(4); var lotNou = AdaugaPlus(nplus, 1, 10); Opereaza(nplus);
        Grup("plus, lotul sursei", plus.Id, nplus, plus.Linii[0].Lot!.Value, 4, 4, 0);
        Grup("plus, lotul adăugat", plus.Id, nplus, lotNou.Lot!.Value, 1, 0, 1);

        Mutant("efectul recepției fără cantitate", db => db.Database.ExecuteSqlInterpolated(
            $"UPDATE \"Postare\" SET \"Cantitate\" = 0 WHERE \"DocumentId\" = {zero.Id} AND \"Unitate\" = {zero.Linii[0].Lot}"));
        Mutant("delta fără cantitate", db => db.Database.ExecuteSqlInterpolated(
            $"UPDATE \"Postare\" SET \"Cantitate\" = 0 WHERE \"DocumentId\" = {nminus} AND \"Unitate\" = {minus.Linii[0].Lot}"));
        Mutant("proveniența ștearsă", db => db.Database.ExecuteSqlInterpolated(
            $"UPDATE \"Documente\" SET \"SursaReceptieiId\" = NULL WHERE \"ID\" = {nzero}"));

        Storneaza(nminus, new(An, 1, 20));
        Grup("storno, fără cumul activ", minus.Id, nminus, minus.Linii[0].Lot!.Value, 0, 4, 0);
        var corectie = CuSpatiu(os => ComenziDocument.Sistem(os)
            .Corecteaza(nzero, new(An, 1, 20), MotivCorectie.EroareMateriala).CorectieId);
        Schimba(corectie, 2); Opereaza(corectie);
        Verifica("SC-CIT-103", "corecție: registrul grupului 4 − 4 + 2, cubul 4 pe factură și −2 pe corecție", CuSpatiu(os => {
            var lot = zero.Linii[0].Lot!.Value;
            C.Citiri.Loturi.VerificaAcoperire(os);
            return Registru(os, lot, zero.Id, nzero, corectie) == 2 && Cub(os, lot, zero.Id) == 4
                && Cub(os, lot, nzero) == 0 && Cub(os, lot, corectie) == -2;
        }));
    }

    static decimal Registru(IObjectSpace os, Guid lot, params Guid[] documente) => os.GetObjectsQuery<RegistruStoc>()
        .Where(r => r.LotId == lot && r.DocumentId != null && documente.Contains(r.DocumentId.Value))
        .Sum(r => (decimal?)r.Cantitate) ?? 0m;

    static decimal Cub(IObjectSpace os, Guid lot, Guid document) => os.GetObjectsQuery<C.Postare>()
        .Where(p => p.DocumentId == document && p.Unitate == lot && p.FelUnitate == N.FelUnitate.Lot)
        .Sum(p => (decimal?)p.Cantitate) ?? 0m;

    void Grup(string caz, Guid factura, Guid nir, Guid lot, decimal registru, decimal peFactura, decimal peNir) =>
        Verifica("SC-CIT-103", $"{caz}: registrul grupului {registru}, cubul {peFactura} pe factură și {peNir} pe recepție; acoperirea trece",
            CuSpatiu(os => {
                C.Citiri.Loturi.VerificaAcoperire(os);
                return Registru(os, lot, factura, nir) == registru && Cub(os, lot, factura) == peFactura && Cub(os, lot, nir) == peNir;
            }));

    void Mutant(string caz, Action<DbContext> strica) => Comanda(os => {
        var db = ((EFCoreObjectSpace)os).DbContext;
        using var tx = db.Database.BeginTransaction();
        strica(db);
        Refuza("SC-CIT-104/" + caz, () => C.Citiri.Invarianti.Verifica(os), C.Citiri.Loturi.IstoricIncomplet);
        tx.Rollback();
    });
}
