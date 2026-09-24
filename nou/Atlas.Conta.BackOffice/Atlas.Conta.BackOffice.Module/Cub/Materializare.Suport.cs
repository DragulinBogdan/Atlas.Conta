using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Declaratii;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.Module.Cub;

public static partial class Materializare {
    public static void BlocheazaFise(IObjectSpace os) {
        if (os is EFCoreObjectSpace ef && ef.DbContext.Database.CurrentTransaction != null)
            BlocheazaNominalizarea(os);
    }

    static void BlocheazaNominalizarea(IObjectSpace os) {
        if (os is not EFCoreObjectSpace ef || ef.DbContext.Database.CurrentTransaction == null)
            throw new OperareException("Nominalizarea și inversarea suportului cer tranzacția comenzii.");
        ef.DbContext.Database.ExecuteSqlRaw("SELECT pg_advisory_xact_lock(97001)");
    }

    static void VerificaSuportFaraDependenti(IObjectSpace os, Document doc, DateOnly deLa) {
        BlocheazaNominalizarea(os);
        var surse = os.GetObjectsQuery<Postare>().Where(p => p.DocumentId == doc.ID).Select(p => p.ID);
        var dependenti = os.GetObjectsQuery<Postare>()
            .Where(p => p.SuportId != null && surse.Contains(p.SuportId.Value) && p.Unitate == null)
            .GroupBy(p => new { p.SuportId, p.DocumentId, p.Data })
            .Select(g => new { g.Key.SuportId, g.Key.DocumentId, g.Key.Data, Valoare = g.Sum(p => p.Valoare) })
            .ToList();
        var activ = dependenti.GroupBy(p => new { p.SuportId, p.DocumentId }).Any(g => {
            decimal sold = 0m;
            foreach (var zi in g.GroupBy(p => p.Data <= deLa ? deLa : p.Data).OrderBy(p => p.Key)) {
                sold += zi.Sum(p => p.Valoare);
                if (sold != 0m) return true;
            }
            return false;
        });
        if (activ)
            throw new OperareException($"{CoduriRefuz.SuportCuDependenti}: Suportul contabil este nominalizat pe fișe active.");
    }
}
