using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using Microsoft.EntityFrameworkCore;
using N = Atlas.Conta.Nucleu;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

public struct SoldPartida {
    public Guid UnitateId { get; set; }
    public Guid ContId { get; set; }
    public Guid PartenerId { get; set; }
    public DateOnly Deschisa { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public struct OriginePartida {
    public Guid UnitateId { get; set; }
    public Guid ContId { get; set; }
    public Guid PartenerId { get; set; }
    public Guid? DocumentId { get; set; }
}

public struct PartidaProprie {
    public Guid DocumentId { get; set; }
    public Guid UnitateId { get; set; }
    public Guid ContId { get; set; }
    public Guid PartenerId { get; set; }
    public decimal Net { get; set; }
}

public struct PerecheDisponibila {
    public Guid StingatorId { get; set; }
    public Guid StinsId { get; set; }
    public decimal Disponibil { get; set; }
}

public static class Partide {
    public static IQueryable<PerecheDisponibila> Perechi(IObjectSpace os, Guid document, bool stinge,
            Guid? partener = null) {
        var proprii = Proprii(os, DateOnly.MaxValue);
        var surse = stinge ? proprii.Where(p => p.DocumentId == document) : proprii;
        var tinte = stinge ? proprii : proprii.Where(p => p.DocumentId == document);
        if (partener is { } cp) surse = surse.Where(p => p.PartenerId == cp);
        return (from s in surse
                join t in tinte on new { s.ContId, s.PartenerId } equals new { t.ContId, t.PartenerId }
                where s.DocumentId != t.DocumentId && s.Net * t.Net < 0m
                select new { StingatorId = s.DocumentId, StinsId = t.DocumentId,
                    Disponibil = Math.Min(Math.Abs(s.Net), Math.Abs(t.Net)) })
            .GroupBy(p => new { p.StingatorId, p.StinsId })
            .Where(g => g.Count() == 1)
            .Select(g => new PerecheDisponibila { StingatorId = g.Key.StingatorId, StinsId = g.Key.StinsId,
                Disponibil = g.Sum(p => p.Disponibil) });
    }
    public static IQueryable<PartidaProprie> Proprii(IObjectSpace os, DateOnly panaLa) =>
        from s in Solduri(os, panaLa)
        join o in Origini(os) on new { s.UnitateId, s.ContId, s.PartenerId }
            equals new { o.UnitateId, o.ContId, o.PartenerId }
        where o.DocumentId != null
        select new PartidaProprie { DocumentId = o.DocumentId.Value, UnitateId = s.UnitateId,
            ContId = s.ContId, PartenerId = s.PartenerId, Net = s.Debit - s.Credit };

    /// <summary>Restul documentului pe partidele proprii, cu aceeași selecție pe sens ca <see cref="Total"/>.</summary>
    public static decimal Ramas(IObjectSpace os, Guid document, SensStingere? sens) {
        var proprii = Proprii(os, DateOnly.MaxValue).Where(p => p.DocumentId == document);
        return sens switch {
            SensStingere.Datorie => proprii.Where(p => p.Net < 0m).Select(p => (decimal?)-p.Net).Sum(),
            SensStingere.Creanta => proprii.Where(p => p.Net > 0m).Select(p => (decimal?)p.Net).Sum(),
            _ => proprii.Select(p => (decimal?)Math.Abs(p.Net)).Sum(),
        } ?? 0m;
    }

    public static decimal Disponibil(IObjectSpace os, Guid document, Guid partener, SensStingere sens) {
        var proprii = Proprii(os, DateOnly.MaxValue)
            .Where(p => p.DocumentId == document && p.PartenerId == partener);
        return (sens == SensStingere.Datorie ? proprii.Where(p => p.Net > 0m) : proprii.Where(p => p.Net < 0m))
            .Select(p => (decimal?)Math.Abs(p.Net)).Sum() ?? 0m;
    }

    /// <summary>Totalul documentului pe partidele proprii, în sensul de stins; fără sens declarat, Σ |net| pe unitate.</summary>
    public static decimal Total(IObjectSpace os, Guid document, SensStingere? sens) {
        var noi = Noi(os, document)
            .Where(p => p.Tranzactie.Fel == N.FelTranzactie.Operare && p.FelUnitate == N.FelUnitate.Partida).ToArray();
        var nete = noi.Length != 0
            ? noi.GroupBy(p => new { p.Cont, p.Partener, p.Unitate })
                .Select(g => g.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare)).ToList()
            : Contabil.Postari(os)
                .Where(p => p.DocumentId == document && p.FelUnitate == N.FelUnitate.Partida)
                .GroupBy(p => new { p.Cont, p.Partener, p.Unitate })
                .Select(g => g.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare)).ToList();
        return sens switch {
            SensStingere.Datorie => nete.Where(n => n < 0m).Sum(n => -n),
            SensStingere.Creanta => nete.Where(n => n > 0m).Sum(),
            _ => nete.Sum(Math.Abs),
        };
    }

    /// <summary>Postările contabile ale documentului materializate în comanda curentă, încă nescrise în bază.</summary>
    public static IEnumerable<Postare> Noi(IObjectSpace os, Guid document) =>
        os.ModifiedObjects.OfType<Postare>().Where(p => os.IsNewObject(p)
            && p.DocumentId == document && p.Carte == N.Carte.Contabil);

    public static decimal Capacitate(IObjectSpace os, Guid document, Guid partener, SensStingere sens) {
        var nete = Contabil.Postari(os).Where(p => p.DocumentId == document && p.Partener == partener
                && p.FelUnitate == N.FelUnitate.Partida)
            .GroupBy(p => new { p.Cont, p.Unitate })
            .Select(g => new { Net = g.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare) });
        return (sens == SensStingere.Datorie ? nete.Where(p => p.Net > 0m) : nete.Where(p => p.Net < 0m))
            .Select(p => (decimal?)Math.Abs(p.Net)).Sum() ?? 0m;
    }

    public static decimal NominalizataLibera(IObjectSpace os, Guid stingator, Guid stins, DateOnly? laData = null, Guid? partener = null) {
        var zi = laData ?? DateOnly.MaxValue;
        var unitati = Origini(os).Where(o => o.DocumentId == stins).Select(o => o.UnitateId);
        var postari = Postari(os).Where(p => p.DocumentId == stingator && unitati.Contains(p.Unitate.Value)
                && (partener == null || p.Partener == partener))
            .GroupBy(p => new { p.Unitate, p.Cont, p.Partener, p.Data })
            .Select(g => new { g.Key.Unitate, g.Key.Cont, g.Key.Partener, g.Key.Data,
                Net = g.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : -p.Valoare) }).ToArray();
        var legaturi = os.GetObjectsQuery<Imperechere>()
            .Where(i => i.DocumentStingatorId == stingator && i.DocumentId == stins)
            .Select(i => new { i.Data, i.Suma }).ToArray();
        var efecte = postari.GroupBy(p => new { p.Unitate, p.Cont, p.Partener })
            .Select(g => Evolutie(g.Select(p => (p.Data, p.Net)), zi).ToArray()).ToArray();
        var legata = Evolutie(legaturi.Select(i => (i.Data, i.Suma)), zi).ToArray();
        static decimal La((DateOnly Zi, decimal Sold)[] evolutie, DateOnly zi) =>
            evolutie.LastOrDefault(p => p.Zi <= zi).Sold;
        return Math.Max(0m, efecte.SelectMany(e => e).Concat(legata).Select(p => p.Zi).Distinct()
            .Min(t => efecte.Sum(e => Math.Abs(La(e, t))) - La(legata, t)));
    }

    /// <summary>
    /// Soldul la data cerută, apoi la fiecare zi deja scrisă după ea: zilele anterioare se lipesc
    /// de dată, mișcările aceleiași zile se compensează împreună (C-D5).
    /// </summary>
    public static IEnumerable<(DateOnly Zi, decimal Sold)> Evolutie(IEnumerable<(DateOnly Data, decimal Net)> miscari, DateOnly data) {
        var peZile = miscari.GroupBy(m => m.Data < data ? data : m.Data).ToDictionary(g => g.Key, g => g.Sum(m => m.Net));
        var sold = 0m;
        foreach (var zi in peZile.Keys.Append(data).Distinct().Order()) {
            sold += peZile.GetValueOrDefault(zi);
            yield return (zi, sold);
        }
    }

    /// <summary>Cât din sold rămâne în sensul <paramref name="semn"/> la data cerută și în fiecare zi deja scrisă după ea.</summary>
    public static decimal DisponibilTemporal(IEnumerable<(DateOnly Data, decimal Net)> miscari, DateOnly data, decimal semn) =>
        Math.Max(0m, Evolutie(miscari, data).Min(p => semn * p.Sold));

    public static Guid Identitate(Guid document, Guid cont, Guid partener) =>
        N.Unitate.DeschidePartida(cont, partener, document, DateOnly.MinValue).Id;

    public static IQueryable<OriginePartida> Origini(IObjectSpace os) => Postari(os)
        .Where(p => p.DocumentId == null && p.Tranzactie.Fel == N.FelTranzactie.Deschidere
            || p.DocumentId != null && p.Unitate == Identitate(p.DocumentId.Value, p.Cont, p.Partener.Value))
        .Select(p => new OriginePartida { UnitateId = p.Unitate.Value, ContId = p.Cont,
            PartenerId = p.Partener.Value, DocumentId = p.DocumentId }).Distinct();

    public static IQueryable<SoldPartida> Cumulate(IObjectSpace os, CitireCumul citire, DateOnly panaLa) {
        var snapshot = os.GetObjectsQuery<PartidaDeschisa>().Select(s => new SoldLunar<SoldPartida> {
            An = s.An, Luna = s.Luna,
            Rand = new SoldPartida { UnitateId = s.UnitateId, ContId = s.ContId,
                PartenerId = s.PartenerId, Deschisa = s.Deschisa, Debit = s.Debit, Credit = s.Credit }
        });
        var miscari = Postari(os).Select(p => new RandDatat<SoldPartida> {
            Data = p.Data,
            Rand = new SoldPartida { UnitateId = p.Unitate.Value, ContId = p.Cont,
                PartenerId = p.Partener.Value, Deschisa = p.UnitateDeschisa.Value,
                Debit = p.Latura == N.Latura.Debit ? p.Valoare : 0m,
                Credit = p.Latura == N.Latura.Credit ? p.Valoare : 0m }
        });
        return CumulPerioade.Citeste(os, citire, miscari, snapshot, panaLa)
            .GroupBy(p => new { p.UnitateId, p.ContId, p.PartenerId })
            .Select(g => new SoldPartida { UnitateId = g.Key.UnitateId, ContId = g.Key.ContId,
                PartenerId = g.Key.PartenerId, Deschisa = g.Min(p => p.Deschisa),
                Debit = g.Sum(p => p.Debit), Credit = g.Sum(p => p.Credit) });
    }
    public static IQueryable<Postare> Postari(IObjectSpace os) => os.GetObjectsQuery<Postare>().Where(Transformare.FaraContrapondere)
        .Where(p => p.Carte == N.Carte.Contabil
            && p.FelUnitate == N.FelUnitate.Partida && p.Unitate != null
            && p.Partener != null && p.UnitateDeschisa != null);

    public static IQueryable<SoldPartida> Solduri(IObjectSpace os, DateOnly panaLa, Guid? faraDocument = null) =>
        Postari(os).Where(p => p.Data <= panaLa && (faraDocument == null || p.DocumentId != faraDocument))
            .GroupBy(p => new { p.Unitate, p.Cont, p.Partener })
            .Select(g => new SoldPartida {
                UnitateId = g.Key.Unitate.Value, ContId = g.Key.Cont, PartenerId = g.Key.Partener.Value,
                Deschisa = g.Min(p => p.UnitateDeschisa.Value),
                Debit = g.Sum(p => p.Latura == N.Latura.Debit ? p.Valoare : 0m),
                Credit = g.Sum(p => p.Latura == N.Latura.Credit ? p.Valoare : 0m)
            });

    public static void VerificaAcoperire(IObjectSpace os) {
        var externi = os.GetObjectsQuery<Partener>().Select(p => p.ID)
            .Concat(os.GetObjectsQuery<Angajat>().Select(p => p.ID));
        var documente = os.GetObjectsQuery<Document>();
        var conturi = os.GetObjectsQuery<Cont>().Where(c => c.UrmarestePartide);
        var lipsuri = os.GetObjectsQuery<Postare>().Where(Transformare.FaraContrapondere)
            .Where(p => p.Carte == N.Carte.Contabil
                && (p.FelUnitate == N.FelUnitate.Partida
                    && (p.Unitate == null || p.Partener == null || p.UnitateDeschisa == null)
                || p.Unitate == null && conturi.Any(c => c.ID == p.Cont)
                    && (p.Partener != null || documente.Any(d => d.ID == p.DocumentId
                        && (externi.Contains(d.PredatorId) || externi.Contains(d.PrimitorId))))))
            .Select(p => new { p.ID, p.DocumentId, p.Cont, p.LinieId });
        var numar = lipsuri.LongCount();
        if (numar != 0)
            throw new OperareException($"CITIRE_PARTIDE_INCOMPLETE: {numar} postări fără identitate completă de partidă; exemple: "
                + string.Join("; ", lipsuri.OrderBy(p => p.ID).Take(10).ToList()
                    .Select(p => $"postare {p.ID}, document {p.DocumentId}, cont {p.Cont}, linie {p.LinieId}")));
    }
}
