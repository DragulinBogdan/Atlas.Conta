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
        Grup("delta zero", zero.Id, nzero, zero.Linii[0].Lot!.Value, 4, 0);
        var (minus, nminus) = Constatat(3); Opereaza(nminus);
        Grup("minus", minus.Id, nminus, minus.Linii[0].Lot!.Value, 4, -1);
        var (plus, nplus) = Constatat(4); var lotNou = AdaugaPlus(nplus, 1, 10); Opereaza(nplus);
        Grup("plus, lotul sursei", plus.Id, nplus, plus.Linii[0].Lot!.Value, 4, 0);
        Grup("plus, lotul adăugat", plus.Id, nplus, lotNou.Lot!.Value, 0, 1);

        Storneaza(nminus, new(An, 1, 20));
        Grup("storno, fără cumul activ", minus.Id, nminus, minus.Linii[0].Lot!.Value, 4, 0);
        var corectie = CuSpatiu(os => ComenziDocument.Sistem(os)
            .Corecteaza(nzero, new(An, 1, 20), MotivCorectie.EroareMateriala).CorectieId);
        Schimba(corectie, 2); Opereaza(corectie);
        Verifica("SC-CIT-103", "corecție: cubul 4 pe factură și −2 pe corecție", CuSpatiu(os => {
            var lot = zero.Linii[0].Lot!.Value;
            return Cub(os, lot, zero.Id) == 4
                && Cub(os, lot, nzero) == 0 && Cub(os, lot, corectie) == -2;
        }));
    }

    static decimal Cub(IObjectSpace os, Guid lot, Guid document) => os.GetObjectsQuery<C.Postare>()
        .Where(p => p.DocumentId == document && p.Unitate == lot && p.FelUnitate == N.FelUnitate.Lot)
        .Sum(p => (decimal?)p.Cantitate) ?? 0m;

    void Grup(string caz, Guid factura, Guid nir, Guid lot, decimal peFactura, decimal peNir) =>
        Verifica("SC-CIT-103", $"{caz}: cubul {peFactura} pe factură și {peNir} pe recepție",
            CuSpatiu(os => Cub(os, lot, factura) == peFactura && Cub(os, lot, nir) == peNir));
}
