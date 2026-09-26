using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.EntityFrameworkCore.Security.Infrastructure;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

public struct RandDatat<T> {
    public DateOnly Data { get; set; }
    public T Rand { get; set; }
}

public struct SoldLunar<T> {
    public int An { get; set; }
    public int Luna { get; set; }
    public T Rand { get; set; }
}

public static class CumulPerioade {
    internal static bool EsteSecurizat(IObjectSpace os) =>
        os is ISecuredObjectSpace && (os is not EFCoreObjectSpace ef
            || ef.DbContext.GetService<ISecurityEnabledOption>().EnableSecurity);

    /// <summary>Referința și fereastra sunt citite în aceeași instrucțiune; citirea secured folosește doar mișcările.</summary>
    public static IQueryable<T> Citeste<T>(IObjectSpace os, IQueryable<RandDatat<T>> miscari,
            IQueryable<SoldLunar<T>> snapshot, DateOnly panaLa, DateOnly? granita = null) {
        var direct = miscari.Where(m => m.Data <= panaLa);
        if (EsteSecurizat(os)) return direct.Select(m => m.Rand);
        var limita = granita ?? panaLa;
        var ultimaLuna = limita.Year * 12 + limita.Month;
        if (limita.Day != DateTime.DaysInMonth(limita.Year, limita.Month)) ultimaLuna--;
        var inchise = os.GetObjectsQuery<PerioadaFiscala>().Where(p => p.Inchisa);
        var referinta = inchise
            .Where(p => p.Luna == 12 || !inchise.Any(q => q.An > p.An || q.An == p.An && q.Luna > p.Luna))
            .Where(p => p.An * 12 + p.Luna <= ultimaLuna)
            .OrderByDescending(p => p.An).ThenByDescending(p => p.Luna).Take(1);
        var sold = snapshot.Where(s => referinta.Any(p => p.An == s.An && p.Luna == s.Luna));
        return sold.Select(s => s.Rand).Concat(direct
            .Where(m => m.Data.Year * 12 + m.Data.Month
                > (sold.Select(s => (int?)(s.An * 12 + s.Luna)).Max() ?? 0))
            .Select(m => m.Rand));
    }
}
