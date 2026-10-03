using Atlas.Conta.BackOffice.Module.BusinessObjects;
using DevExpress.ExpressApp;

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

/// <summary>Cine citește cumulul, declarat de apelant (104b).</summary>
public enum CitireCumul {
    /// <summary>Cititorul poate să nu vadă toate mișcările (ușa securizată): cumulul se citește numai din mișcări.</summary>
    Vizibila,
    /// <summary>Cititorul vede toate mișcările (motorul, ușa de sistem): cumulul pornește din snapshot.</summary>
    Integrala
}

public static class CumulPerioade {
    /// <summary>Referința și fereastra sunt citite în aceeași instrucțiune; citirea vizibilă folosește doar mișcările.</summary>
    public static IQueryable<T> Citeste<T>(IObjectSpace os, CitireCumul citire, IQueryable<RandDatat<T>> miscari,
            IQueryable<SoldLunar<T>> snapshot, DateOnly panaLa, DateOnly? granita = null) {
        var direct = miscari.Where(m => m.Data <= panaLa);
        if (citire == CitireCumul.Vizibila) return direct.Select(m => m.Rand);
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
            .Where(m => m.Data > (sold.Select(s => (DateOnly?)new DateOnly(s.An, s.Luna, 1).AddMonths(1).AddDays(-1)).Max()
                ?? DateOnly.MinValue))
            .Select(m => m.Rand));
    }
}
