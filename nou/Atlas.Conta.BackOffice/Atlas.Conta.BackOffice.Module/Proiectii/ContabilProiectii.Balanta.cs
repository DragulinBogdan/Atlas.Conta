using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExtreme.AspNet.Data;

namespace Atlas.Conta.BackOffice.Module.Proiectii;

/// <summary>Rândul balanței: <c>Initial*</c> sunt sumele precedente (soldul inițial al anului plus rulajele de la 1 ianuarie până la start).</summary>
public sealed class BalantaRand {
    public Guid ContId { get; set; }
    public string ContSimbol { get; set; }
    public string ContDenumire { get; set; }
    public Guid? RepartitorId { get; set; }
    public string RepartitorDenumire { get; set; }

    public decimal SoldInceputAnDebit { get; set; }
    public decimal SoldInceputAnCredit { get; set; }
    public decimal InitialDebit { get; set; }
    public decimal InitialCredit { get; set; }
    public decimal SoldInitialDebit { get; set; }
    public decimal SoldInitialCredit { get; set; }

    public decimal RulajDebit { get; set; }
    public decimal RulajCredit { get; set; }

    public decimal SoldFinalDebit { get; set; }
    public decimal SoldFinalCredit { get; set; }
}

public sealed class BalantaPlanRand {
    public Guid ContId { get; set; }
    public Guid? ParinteId { get; set; }
    public string ContSimbol { get; set; }
    public string ContDenumire { get; set; }
    public int Nivel { get; set; }
    public bool AreCopii { get; set; }
    public bool AreMiscareProprie { get; set; }

    public decimal SoldInceputAnDebit { get; set; }
    public decimal SoldInceputAnCredit { get; set; }
    public decimal InitialDebit { get; set; }
    public decimal InitialCredit { get; set; }
    public decimal SoldInitialDebit { get; set; }
    public decimal SoldInitialCredit { get; set; }

    public decimal RulajDebit { get; set; }
    public decimal RulajCredit { get; set; }

    public decimal SoldFinalDebit { get; set; }
    public decimal SoldFinalCredit { get; set; }
}

sealed class AgregatBalanta {
    public Guid ContId { get; set; }
    public string ContSimbol { get; set; }
    public string ContDenumire { get; set; }
    public Guid? RepartitorId { get; set; }
    public string RepartitorDenumire { get; set; }

    public decimal AnDebit { get; set; }
    public decimal AnCredit { get; set; }
    public decimal InainteDebit { get; set; }
    public decimal InainteCredit { get; set; }
    public decimal RulajDebit { get; set; }
    public decimal RulajCredit { get; set; }
}

// Felia atomului în balanță: 0 = cumulul până la 31.12 al anului precedent, 1 = înaintea startului, 2 = rulajul.
struct AtomBalanta {
    public Guid ContId { get; set; }
    public Guid? RepartitorId { get; set; }
    public int Felie { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
}

public static partial class ContabilProiectii {
    public static IQueryable<BalantaRand> Balanta(
        IObjectSpace os, DateOnly dataStart, DateOnly dataEnd, bool analitic = false,
        Guid? repartitorId = null, Guid? materialId = null, Guid? codFunctionalId = null,
        Guid? codEconomicId = null, Guid? sursaFinantareId = null, Guid? unitateId = null,
        Guid? proiectId = null, Guid? centruCostId = null,
        CitireCumul citire = CitireCumul.Vizibila) {

        IQueryable<AtomContabil> Filtrate(IQueryable<AtomContabil> atomi) => PeDimensiuni(atomi, repartitorId, materialId,
            codFunctionalId, codEconomicId, sursaFinantareId, unitateId, proiectId, centruCostId);

        // E3-D8: granița de an e calendaristică; startul pe 1 ianuarie (sau fără start) citește o singură dată.
        var inceputAn = dataStart == DateOnly.MinValue ? DateOnly.MinValue : new DateOnly(dataStart.Year, 1, 1);
        var anLaStart = dataStart == inceputAn;
        var atomi = Filtrate(AtomiPerioada(os, dataStart, dataEnd, citire))
            .Select(a => new AtomBalanta {
                ContId = a.ContId, RepartitorId = a.RepartitorId,
                Felie = a.Data < dataStart ? 1 : 2, Debit = a.Debit, Credit = a.Credit,
            });
        if (!anLaStart)
            atomi = atomi.Concat(Filtrate(SolduriService.AtomiCumulati(os, citire, inceputAn.AddDays(-1)))
                .Select(a => new AtomBalanta {
                    ContId = a.ContId, RepartitorId = a.RepartitorId,
                    Felie = 0, Debit = a.Debit, Credit = a.Credit,
                }));
        IQueryable<AgregatBalanta> etichetate;
        if (analitic) {
            var agregate = atomi
                .GroupBy(a => new { a.ContId, a.RepartitorId })
                .Select(g => new {
                    g.Key.ContId,
                    g.Key.RepartitorId,
                    AnDebit = g.Sum(a => a.Felie == 0 ? a.Debit : 0m),
                    AnCredit = g.Sum(a => a.Felie == 0 ? a.Credit : 0m),
                    InainteDebit = g.Sum(a => a.Felie == 1 ? a.Debit : 0m),
                    InainteCredit = g.Sum(a => a.Felie == 1 ? a.Credit : 0m),
                    RulajDebit = g.Sum(a => a.Felie == 2 ? a.Debit : 0m),
                    RulajCredit = g.Sum(a => a.Felie == 2 ? a.Credit : 0m)
                });
            etichetate =
                from a in agregate
                join c in os.GetObjectsQuery<Cont>() on a.ContId equals c.ID into grupCont
                from c in grupCont.DefaultIfEmpty()
                join r in os.GetObjectsQuery<Repartitor>()
                    on a.RepartitorId equals (Guid?)r.ID into grupRep
                from r in grupRep.DefaultIfEmpty()
                select new AgregatBalanta {
                    ContId = a.ContId,
                    ContSimbol = c == null ? null : c.Simbol,
                    ContDenumire = c == null ? null : c.Denumire,
                    RepartitorId = a.RepartitorId,
                    RepartitorDenumire = r == null ? null : r.Denumire,
                    AnDebit = anLaStart ? a.InainteDebit : a.AnDebit,
                    AnCredit = anLaStart ? a.InainteCredit : a.AnCredit,
                    InainteDebit = a.InainteDebit, InainteCredit = a.InainteCredit,
                    RulajDebit = a.RulajDebit, RulajCredit = a.RulajCredit
                };
        }
        else {
            var agregate = atomi
                .GroupBy(a => a.ContId)
                .Select(g => new {
                    ContId = g.Key,
                    AnDebit = g.Sum(a => a.Felie == 0 ? a.Debit : 0m),
                    AnCredit = g.Sum(a => a.Felie == 0 ? a.Credit : 0m),
                    InainteDebit = g.Sum(a => a.Felie == 1 ? a.Debit : 0m),
                    InainteCredit = g.Sum(a => a.Felie == 1 ? a.Credit : 0m),
                    RulajDebit = g.Sum(a => a.Felie == 2 ? a.Debit : 0m),
                    RulajCredit = g.Sum(a => a.Felie == 2 ? a.Credit : 0m)
                });
            etichetate =
                from a in agregate
                join c in os.GetObjectsQuery<Cont>() on a.ContId equals c.ID into grupCont
                from c in grupCont.DefaultIfEmpty()
                select new AgregatBalanta {
                    ContId = a.ContId,
                    ContSimbol = c == null ? null : c.Simbol,
                    ContDenumire = c == null ? null : c.Denumire,
                    RepartitorId = null, RepartitorDenumire = null,
                    AnDebit = anLaStart ? a.InainteDebit : a.AnDebit,
                    AnCredit = anLaStart ? a.InainteCredit : a.AnCredit,
                    InainteDebit = a.InainteDebit, InainteCredit = a.InainteCredit,
                    RulajDebit = a.RulajDebit, RulajCredit = a.RulajCredit
                };
        }

        return from a in etichetate
               let netAn = a.AnDebit - a.AnCredit
               let netInitial = a.InainteDebit - a.InainteCredit
               let netFinal = a.InainteDebit - a.InainteCredit + a.RulajDebit - a.RulajCredit
               select new BalantaRand {
                   ContId = a.ContId,
                   ContSimbol = a.ContSimbol,
                   ContDenumire = a.ContDenumire,
                   RepartitorId = a.RepartitorId,
                   RepartitorDenumire = a.RepartitorDenumire,
                   SoldInceputAnDebit = netAn > 0m ? netAn : 0m,
                   SoldInceputAnCredit = netAn < 0m ? -netAn : 0m,
                   InitialDebit = (netAn > 0m ? netAn : 0m) + a.InainteDebit - a.AnDebit,
                   InitialCredit = (netAn < 0m ? -netAn : 0m) + a.InainteCredit - a.AnCredit,
                   SoldInitialDebit = netInitial > 0m ? netInitial : 0m,
                   SoldInitialCredit = netInitial < 0m ? -netInitial : 0m,
                   RulajDebit = a.RulajDebit,
                   RulajCredit = a.RulajCredit,
                   SoldFinalDebit = netFinal > 0m ? netFinal : 0m,
                   SoldFinalCredit = netFinal < 0m ? -netFinal : 0m
               };
    }

    public static SortingInfo[] OrdineBalanta(bool analitic) => analitic
        ? new[] {
            OrdineLista.Crescator(nameof(BalantaRand.ContSimbol)),
            OrdineLista.Crescator(nameof(BalantaRand.ContId)),
            OrdineLista.Crescator(nameof(BalantaRand.RepartitorId))
        }
        : new[] {
            OrdineLista.Crescator(nameof(BalantaRand.ContSimbol)),
            OrdineLista.Crescator(nameof(BalantaRand.ContId))
        };

    public static List<BalantaPlanRand> BalantaPlan(
        IObjectSpace os, DateOnly dataStart, DateOnly dataEnd, int? nivelMaxim = null,
        Guid? repartitorId = null, Guid? materialId = null, Guid? codFunctionalId = null,
        Guid? codEconomicId = null, Guid? sursaFinantareId = null, Guid? unitateId = null,
        Guid? proiectId = null, Guid? centruCostId = null,
        CitireCumul citire = CitireCumul.Vizibila) {

        var frunze = Balanta(os, dataStart, dataEnd, analitic: false,
            repartitorId, materialId, codFunctionalId, codEconomicId,
            sursaFinantareId, unitateId, proiectId, centruCostId, citire).ToList();

        var plan = os.GetObjectsQuery<Cont>()
            .Select(c => new { c.ID, c.Simbol, c.Denumire, c.ParinteId })
            .ToDictionary(c => c.ID, c => (c.Simbol, c.Denumire, c.ParinteId));

        var noduri = new Dictionary<Guid, BalantaPlanRand>();

        BalantaPlanRand Nod(Guid contId) {
            if (noduri.TryGetValue(contId, out var existent))
                return existent;
            var gasit = plan.TryGetValue(contId, out var info);
            var nod = new BalantaPlanRand {
                ContId = contId,
                ContSimbol = gasit ? info.Simbol : null,
                ContDenumire = gasit ? info.Denumire : null,
                ParinteId = gasit && info.ParinteId is Guid parinte && plan.ContainsKey(parinte)
                    ? parinte : null
            };
            noduri.Add(contId, nod);
            return nod;
        }

        foreach (var frunza in frunze) {
            Nod(frunza.ContId).AreMiscareProprie = true;
            var vizitate = new HashSet<Guid>();
            for (Guid? id = frunza.ContId; id is Guid contId && vizitate.Add(contId); id = Nod(contId).ParinteId) {
                var nod = Nod(contId);
                nod.SoldInceputAnDebit += frunza.SoldInceputAnDebit;
                nod.SoldInceputAnCredit += frunza.SoldInceputAnCredit;
                nod.InitialDebit += frunza.InitialDebit;
                nod.InitialCredit += frunza.InitialCredit;
                nod.RulajDebit += frunza.RulajDebit;
                nod.RulajCredit += frunza.RulajCredit;
            }
        }

        foreach (var nod in noduri.Values) {
            var nivel = 0;
            var vizitate = new HashSet<Guid> { nod.ContId };
            var parinte = nod.ParinteId;
            while (parinte is Guid pid && noduri.TryGetValue(pid, out var sus) && vizitate.Add(pid)) {
                nivel++;
                parinte = sus.ParinteId;
            }
            nod.Nivel = nivel;
        }

        foreach (var nod in noduri.Values) {
            var netAn = nod.SoldInceputAnDebit - nod.SoldInceputAnCredit;
            nod.SoldInceputAnDebit = netAn > 0m ? netAn : 0m;
            nod.SoldInceputAnCredit = netAn < 0m ? -netAn : 0m;
            var netInitial = nod.InitialDebit - nod.InitialCredit;
            var netFinal = netInitial + nod.RulajDebit - nod.RulajCredit;
            nod.SoldInitialDebit = netInitial > 0m ? netInitial : 0m;
            nod.SoldInitialCredit = netInitial < 0m ? -netInitial : 0m;
            nod.SoldFinalDebit = netFinal > 0m ? netFinal : 0m;
            nod.SoldFinalCredit = netFinal < 0m ? -netFinal : 0m;
        }

        var pastrate = nivelMaxim is int max
            ? noduri.Values.Where(n => n.Nivel < max).ToList()
            : noduri.Values.ToList();
        var ramase = pastrate.Select(n => n.ContId).ToHashSet();

        foreach (var nod in pastrate)
            nod.AreCopii = false;
        foreach (var nod in pastrate)
            if (nod.ParinteId is Guid pid && ramase.Contains(pid))
                noduri[pid].AreCopii = true;

        return pastrate
            .OrderBy(n => n.ContSimbol ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(n => n.ContId)
            .ToList();
    }
}
