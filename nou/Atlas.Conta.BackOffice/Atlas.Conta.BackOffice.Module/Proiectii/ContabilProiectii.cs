using System.Runtime.CompilerServices;
using System.Text;
using N = Atlas.Conta.Nucleu;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Motor;
using DevExpress.ExpressApp;
using DevExpress.ExpressApp.EFCore;
using DevExtreme.AspNet.Data;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.Module.Proiectii;

public struct AtomContabil {
    public DateOnly Data { get; set; }
    public Guid ContId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }

    public Guid? RepartitorId { get; set; }
    public Guid? GestiuneId { get; set; }
    public Guid? MaterialId { get; set; }
    public Guid? CodFunctionalId { get; set; }
    public Guid? CodEconomicId { get; set; }
    public Guid? SursaFinantareId { get; set; }
    public Guid? UnitateId { get; set; }
    public Guid? ProiectId { get; set; }
    public Guid? CentruCostId { get; set; }
}

sealed class AgregatBalanta {
    public Guid ContId { get; set; }
    public string ContSimbol { get; set; }
    public string ContDenumire { get; set; }
    public Guid? RepartitorId { get; set; }
    public string RepartitorDenumire { get; set; }

    public decimal InitialDebit { get; set; }
    public decimal InitialCredit { get; set; }
    public decimal RulajDebit { get; set; }
    public decimal RulajCredit { get; set; }
}

public sealed class BalantaRand {
    public Guid ContId { get; set; }
    public string ContSimbol { get; set; }
    public string ContDenumire { get; set; }
    public Guid? RepartitorId { get; set; }
    public string RepartitorDenumire { get; set; }

    public decimal InitialDebit { get; set; }
    public decimal InitialCredit { get; set; }
    public decimal SoldInitialDebit { get; set; }
    public decimal SoldInitialCredit { get; set; }

    public decimal RulajDebit { get; set; }
    public decimal RulajCredit { get; set; }

    public decimal SoldFinalDebit { get; set; }
    public decimal SoldFinalCredit { get; set; }
}

public sealed class SoldPartenerRand {
    public Guid ContId { get; set; }
    public string ContSimbol { get; set; }
    public string ContDenumire { get; set; }
    public Guid? RepartitorId { get; set; }
    public string RepartitorDenumire { get; set; }

    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal SoldDebitor { get; set; }
    public decimal SoldCreditor { get; set; }
}

public sealed class BalantaPlanRand {
    public Guid ContId { get; set; }
    public Guid? ParinteId { get; set; }
    public string ContSimbol { get; set; }
    public string ContDenumire { get; set; }
    public int Nivel { get; set; }
    public bool AreCopii { get; set; }
    public bool AreMiscareProprie { get; set; }

    public decimal InitialDebit { get; set; }
    public decimal InitialCredit { get; set; }
    public decimal SoldInitialDebit { get; set; }
    public decimal SoldInitialCredit { get; set; }

    public decimal RulajDebit { get; set; }
    public decimal RulajCredit { get; set; }

    public decimal SoldFinalDebit { get; set; }
    public decimal SoldFinalCredit { get; set; }
}

public interface IRandCuDocument {
    Guid? DocumentId { get; }
    string DocumentTip { get; set; }
}

public sealed class FisaContRand : IRandCuDocument {
    public Guid Id { get; set; }
    public int Spatiu { get; set; }
    public Guid TranzactieId { get; set; }
    public DateOnly Data { get; set; }
    public string NumarNota { get; set; }
    public string Sens { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public decimal SoldCurent { get; set; }
    public Guid? ContrapartidaId { get; set; }
    public string ContrapartidaSimbol { get; set; }
    public string RepartitorDenumire { get; set; }
    public Guid? DocumentId { get; set; }
    public string DocumentTip { get; set; }
    public string DocumentNumar { get; set; }
    public bool Storno { get; set; }
}

public class FisaContSql {
    public virtual Guid Id { get; set; }
    public virtual int Spatiu { get; set; }
    public virtual Guid TranzactieId { get; set; }
    public virtual DateOnly Data { get; set; }
    public virtual string NumarNota { get; set; }
    public virtual string Sens { get; set; }
    public virtual decimal Debit { get; set; }
    public virtual decimal Credit { get; set; }
    public virtual decimal SoldCurent { get; set; }
    public virtual Guid? ContrapartidaId { get; set; }
    public virtual string ContrapartidaSimbol { get; set; }
    public virtual string RepartitorDenumire { get; set; }
    public virtual Guid? DocumentId { get; set; }
    public virtual string DocumentTip { get; set; }
    public virtual string DocumentNumar { get; set; }
    public virtual bool Storno { get; set; }
}

public sealed class JurnalRand : IRandCuDocument {
    public Guid Id { get; set; }
    public int Spatiu { get; set; }
    public Guid TranzactieId { get; set; }
    public DateOnly Data { get; set; }
    public string NumarNota { get; set; }
    public Guid ContId { get; set; }
    public string ContSimbol { get; set; }
    public string Sens { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public Guid? DocumentId { get; set; }
    public string DocumentTip { get; set; }
    public string DocumentNumar { get; set; }
    public bool Storno { get; set; }
}

struct SursaFisa {
    public Guid Id { get; set; }
    public Guid TranzactieId { get; set; }
    public Guid ContId { get; set; }
    public int Spatiu { get; set; }
    public DateOnly Data { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public bool Storno { get; set; }
    public Guid? RepartitorId { get; set; }
    public Guid? GestiuneId { get; set; }
    public Guid? MaterialId { get; set; }
    public Guid? CodFunctionalId { get; set; }
    public Guid? CodEconomicId { get; set; }
    public Guid? SursaFinantareId { get; set; }
    public Guid? UnitateId { get; set; }
    public Guid? ProiectId { get; set; }
    public Guid? CentruCostId { get; set; }
    public Guid? DocumentId { get; set; }
    public string ContSimbol { get; set; }
    public string NumarNota { get; set; }
    public string Sens { get; set; }
    public string RepartitorDenumire { get; set; }
}

public static class ContabilProiectii {

    public static IQueryable<AtomContabil> Atomi(IObjectSpace os) =>
        Cub.Citiri.Contabil.Postari(os).Select(p => new AtomContabil {
            Data = p.Data, ContId = p.Cont,
            Debit = p.Latura == Atlas.Conta.Nucleu.Latura.Debit ? p.Valoare : 0m,
            Credit = p.Latura == Atlas.Conta.Nucleu.Latura.Credit ? p.Valoare : 0m,
            RepartitorId = p.Partener, GestiuneId = p.Gestiune, MaterialId = p.Produs,
            CodFunctionalId = p.CodFunctional, CodEconomicId = p.CodEconomic,
            SursaFinantareId = p.SursaFinantare, UnitateId = p.UnitateOrganizatorica,
            ProiectId = p.Proiect, CentruCostId = p.CentruCost
        });

    public static IQueryable<BalantaRand> Balanta(
        IObjectSpace os, DateOnly dataStart, DateOnly dataEnd, bool analitic = false,
        Guid? repartitorId = null, Guid? materialId = null, Guid? codFunctionalId = null,
        Guid? codEconomicId = null, Guid? sursaFinantareId = null, Guid? unitateId = null,
        Guid? proiectId = null, Guid? centruCostId = null, Guid? gestiuneId = null) {

        var atomi = dataStart == DateOnly.MinValue ? Atomi(os).Where(a => a.Data <= dataEnd)
            : SolduriService.AtomiCumulati(os, dataEnd, dataStart.AddDays(-1));

        if (gestiuneId is Guid vGest) atomi = atomi.Where(a => a.GestiuneId == vGest);
        if (repartitorId is Guid vRep) atomi = atomi.Where(a => a.RepartitorId == vRep);
        if (materialId is Guid vMat) atomi = atomi.Where(a => a.MaterialId == vMat);
        if (codFunctionalId is Guid vCf) atomi = atomi.Where(a => a.CodFunctionalId == vCf);
        if (codEconomicId is Guid vCe) atomi = atomi.Where(a => a.CodEconomicId == vCe);
        if (sursaFinantareId is Guid vSf) atomi = atomi.Where(a => a.SursaFinantareId == vSf);
        if (unitateId is Guid vUn) atomi = atomi.Where(a => a.UnitateId == vUn);
        if (proiectId is Guid vPr) atomi = atomi.Where(a => a.ProiectId == vPr);
        if (centruCostId is Guid vCc) atomi = atomi.Where(a => a.CentruCostId == vCc);

        var conturi = os.GetObjectsQuery<Cont>();
        IQueryable<AgregatBalanta> etichetate;

        if (analitic) {
            var agregate = atomi
                .GroupBy(a => new { a.ContId, a.RepartitorId })
                .Select(g => new {
                    g.Key.ContId,
                    g.Key.RepartitorId,
                    InitialDebit = g.Sum(a => a.Data < dataStart ? a.Debit : 0m),
                    InitialCredit = g.Sum(a => a.Data < dataStart ? a.Credit : 0m),
                    RulajDebit = g.Sum(a => a.Data >= dataStart ? a.Debit : 0m),
                    RulajCredit = g.Sum(a => a.Data >= dataStart ? a.Credit : 0m)
                });
            etichetate =
                from a in agregate
                join c in conturi on a.ContId equals c.ID into grupCont
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
                    InitialDebit = a.InitialDebit, InitialCredit = a.InitialCredit,
                    RulajDebit = a.RulajDebit, RulajCredit = a.RulajCredit
                };
        }
        else {
            var agregate = atomi
                .GroupBy(a => a.ContId)
                .Select(g => new {
                    ContId = g.Key,
                    InitialDebit = g.Sum(a => a.Data < dataStart ? a.Debit : 0m),
                    InitialCredit = g.Sum(a => a.Data < dataStart ? a.Credit : 0m),
                    RulajDebit = g.Sum(a => a.Data >= dataStart ? a.Debit : 0m),
                    RulajCredit = g.Sum(a => a.Data >= dataStart ? a.Credit : 0m)
                });
            etichetate =
                from a in agregate
                join c in conturi on a.ContId equals c.ID into grupCont
                from c in grupCont.DefaultIfEmpty()
                select new AgregatBalanta {
                    ContId = a.ContId,
                    ContSimbol = c == null ? null : c.Simbol,
                    ContDenumire = c == null ? null : c.Denumire,
                    RepartitorId = null, RepartitorDenumire = null,
                    InitialDebit = a.InitialDebit, InitialCredit = a.InitialCredit,
                    RulajDebit = a.RulajDebit, RulajCredit = a.RulajCredit
                };
        }

        return from a in etichetate
               let netInitial = a.InitialDebit - a.InitialCredit
               let netFinal = a.InitialDebit - a.InitialCredit + a.RulajDebit - a.RulajCredit
               select new BalantaRand {
                   ContId = a.ContId,
                   ContSimbol = a.ContSimbol,
                   ContDenumire = a.ContDenumire,
                   RepartitorId = a.RepartitorId,
                   RepartitorDenumire = a.RepartitorDenumire,
                   InitialDebit = a.InitialDebit,
                   InitialCredit = a.InitialCredit,
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

    public static IQueryable<SoldPartenerRand> SoldParteneri(
        IObjectSpace os, DateOnly laData, Guid? contId = null,
        Guid? repartitorId = null, Guid? materialId = null, Guid? codFunctionalId = null,
        Guid? codEconomicId = null, Guid? sursaFinantareId = null, Guid? unitateId = null,
        Guid? proiectId = null, Guid? centruCostId = null, Guid? gestiuneId = null) {

        var atomi = SolduriService.AtomiCumulati(os, laData);
        if (contId is Guid vCont) atomi = atomi.Where(a => a.ContId == vCont);
        if (gestiuneId is Guid vGest) atomi = atomi.Where(a => a.GestiuneId == vGest);
        if (repartitorId is Guid vRep) atomi = atomi.Where(a => a.RepartitorId == vRep);
        if (materialId is Guid vMat) atomi = atomi.Where(a => a.MaterialId == vMat);
        if (codFunctionalId is Guid vCf) atomi = atomi.Where(a => a.CodFunctionalId == vCf);
        if (codEconomicId is Guid vCe) atomi = atomi.Where(a => a.CodEconomicId == vCe);
        if (sursaFinantareId is Guid vSf) atomi = atomi.Where(a => a.SursaFinantareId == vSf);
        if (unitateId is Guid vUn) atomi = atomi.Where(a => a.UnitateId == vUn);
        if (proiectId is Guid vPr) atomi = atomi.Where(a => a.ProiectId == vPr);
        if (centruCostId is Guid vCc) atomi = atomi.Where(a => a.CentruCostId == vCc);

        var agregate = atomi
            .GroupBy(a => new { a.ContId, a.RepartitorId })
            .Select(g => new {
                g.Key.ContId,
                g.Key.RepartitorId,
                Debit = g.Sum(a => a.Debit),
                Credit = g.Sum(a => a.Credit)
            });

        var etichetate =
            from a in agregate
            join c in os.GetObjectsQuery<Cont>() on a.ContId equals c.ID into grupCont
            from c in grupCont.DefaultIfEmpty()
            join r in os.GetObjectsQuery<Repartitor>()
                on a.RepartitorId equals (Guid?)r.ID into grupRep
            from r in grupRep.DefaultIfEmpty()
            let net = a.Debit - a.Credit
            select new SoldPartenerRand {
                ContId = a.ContId,
                ContSimbol = c == null ? null : c.Simbol,
                ContDenumire = c == null ? null : c.Denumire,
                RepartitorId = a.RepartitorId,
                RepartitorDenumire = r == null ? null : r.Denumire,
                Debit = a.Debit,
                Credit = a.Credit,
                SoldDebitor = net > 0m ? net : 0m,
                SoldCreditor = net < 0m ? -net : 0m
            };
        return etichetate.Where(x => x.SoldDebitor != 0m || x.SoldCreditor != 0m);
    }

    public static SortingInfo[] OrdineSoldParteneri() => new[] {
        OrdineLista.Crescator(nameof(SoldPartenerRand.ContSimbol)),
        OrdineLista.Crescator(nameof(SoldPartenerRand.ContId)),
        OrdineLista.Crescator(nameof(SoldPartenerRand.RepartitorId))
    };

    public static List<BalantaPlanRand> BalantaPlan(
        IObjectSpace os, DateOnly dataStart, DateOnly dataEnd, int? nivelMaxim = null,
        Guid? repartitorId = null, Guid? materialId = null, Guid? codFunctionalId = null,
        Guid? codEconomicId = null, Guid? sursaFinantareId = null, Guid? unitateId = null,
        Guid? proiectId = null, Guid? centruCostId = null, Guid? gestiuneId = null) {

        var frunze = Balanta(os, dataStart, dataEnd, analitic: false,
            repartitorId, materialId, codFunctionalId, codEconomicId,
            sursaFinantareId, unitateId, proiectId, centruCostId, gestiuneId).ToList();

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

    public static IQueryable<FisaContRand> FisaCont(
        IObjectSpace os, Guid contId, DateOnly dataStart, DateOnly dataEnd,
        Guid? repartitorId = null, Guid? materialId = null, Guid? codFunctionalId = null,
        Guid? codEconomicId = null, Guid? sursaFinantareId = null, Guid? unitateId = null,
        Guid? proiectId = null, Guid? centruCostId = null, bool repartitorNul = false,
        Guid? gestiuneId = null) {
        var argumente = new List<object>();
        string P(object valoare) { argumente.Add(valoare); return "{" + (argumente.Count - 1) + "}"; }
        var postari = Cub.Citiri.Contabil.Postari(os).Where(p => p.Data <= dataEnd);
        var etichetate = from p in postari
            join c in os.GetObjectsQuery<Cont>() on p.Cont equals c.ID into conturi
            from c in conturi.DefaultIfEmpty()
            join d in os.GetObjectsQuery<Document>() on p.DocumentId equals d.ID into documente
            from d in documente.DefaultIfEmpty()
            join r in os.GetObjectsQuery<Repartitor>() on p.Partener equals r.ID into parteneri
            from r in parteneri.DefaultIfEmpty()
            select new SursaFisa {
                Id = p.ID, Spatiu = (int)p.Spatiu, TranzactieId = p.TranzactieId, Data = p.Data, ContId = p.Cont,
                ContSimbol = c.Simbol, NumarNota = d.Numar, Sens = p.Latura == N.Latura.Debit ? "D" : "C",
                Debit = p.Latura == N.Latura.Debit ? p.Valoare : 0m,
                Credit = p.Latura == N.Latura.Credit ? p.Valoare : 0m,
                RepartitorId = p.Partener, GestiuneId = p.Gestiune, MaterialId = p.Produs,
                CodFunctionalId = p.CodFunctional, CodEconomicId = p.CodEconomic,
                SursaFinantareId = p.SursaFinantare, UnitateId = p.UnitateOrganizatorica,
                ProiectId = p.Proiect, CentruCostId = p.CentruCost,
                RepartitorDenumire = r.Denumire, DocumentId = p.DocumentId,
                Storno = p.Tranzactie.Fel == N.FelTranzactie.Storno
            };
        var initial = (dataStart == DateOnly.MinValue ? Atomi(os).Where(a => false)
            : SolduriService.AtomiCumulati(os, dataStart.AddDays(-1))).Where(a => a.ContId == contId);
        var dimensiuni = new Dictionary<string, Guid?> {
            ["RepartitorId"] = repartitorId, ["GestiuneId"] = gestiuneId, ["MaterialId"] = materialId,
            ["CodFunctionalId"] = codFunctionalId, ["CodEconomicId"] = codEconomicId,
            ["SursaFinantareId"] = sursaFinantareId, ["UnitateId"] = unitateId,
            ["ProiectId"] = proiectId, ["CentruCostId"] = centruCostId
        };
        string Filtre(string alias) => string.Concat(dimensiuni.Where(d => d.Value.HasValue)
            .Select(d => $" AND {alias}.\"{d.Key}\" = {P(d.Value.Value)}"))
            + (repartitorNul ? $" AND {alias}.\"RepartitorId\" IS NULL" : "");
        var sursa = SqlInterogare.Compune(etichetate, P);
        var sold = SqlInterogare.Compune(initial, P);
        var sql = $"""
            WITH postari AS NOT MATERIALIZED ({sursa}),
            initial AS (SELECT COALESCE(SUM(i."Debit" - i."Credit"), 0) AS sold FROM ({sold}) i WHERE true{Filtre("i")})
            SELECT p."Id", p."Spatiu", p."TranzactieId", p."Data", p."NumarNota", p."Sens", p."Debit", p."Credit",
                (SELECT sold FROM initial) + SUM(p."Debit" - p."Credit") OVER (
                    ORDER BY p."Data", p."Id", p."Spatiu" ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS "SoldCurent",
                (SELECT CASE WHEN COUNT(DISTINCT c."ContId") = 1 THEN MIN(c."ContId"::text)::uuid END
                    FROM postari c WHERE c."TranzactieId" = p."TranzactieId" AND c."Sens" <> p."Sens") AS "ContrapartidaId",
                (SELECT string_agg(DISTINCT c."ContSimbol", ', ' ORDER BY c."ContSimbol")
                    FROM postari c WHERE c."TranzactieId" = p."TranzactieId" AND c."Sens" <> p."Sens") AS "ContrapartidaSimbol",
                p."RepartitorDenumire", p."DocumentId", NULL::text AS "DocumentTip", p."NumarNota" AS "DocumentNumar", p."Storno"
            FROM postari p WHERE p."ContId" = {P(contId)} AND p."Data" >= {P(dataStart)}{Filtre("p")}
            """;
        return ((EFCoreObjectSpace)os).DbContext.Database
            .SqlQuery<FisaContSql>(FormattableStringFactory.Create(sql, argumente.ToArray()))
            .Select(r => new FisaContRand {
                Id = r.Id, Spatiu = r.Spatiu, TranzactieId = r.TranzactieId, Data = r.Data,
                NumarNota = r.NumarNota, Sens = r.Sens, Debit = r.Debit, Credit = r.Credit,
                SoldCurent = r.SoldCurent, ContrapartidaId = r.ContrapartidaId,
                ContrapartidaSimbol = r.ContrapartidaSimbol, RepartitorDenumire = r.RepartitorDenumire,
                DocumentId = r.DocumentId, DocumentTip = r.DocumentTip, DocumentNumar = r.DocumentNumar, Storno = r.Storno
            }).OrderBy(r => r.Data).ThenBy(r => r.Id).ThenBy(r => r.Spatiu);
    }

    public static SortingInfo[] OrdineFisa() => new[] {
        OrdineLista.Crescator(nameof(FisaContRand.Data)),
        OrdineLista.Crescator(nameof(FisaContRand.Id)),
        OrdineLista.Crescator(nameof(FisaContRand.Spatiu))
    };

    public static IQueryable<JurnalRand> RegistruJurnal(
        IObjectSpace os, DateOnly? dataStart = null, DateOnly? dataEnd = null) {
        var postari = Cub.Citiri.Contabil.Postari(os);
        if (dataStart is { } ds) postari = postari.Where(p => p.Data >= ds);
        if (dataEnd is { } de) postari = postari.Where(p => p.Data <= de);
        return (from p in postari
            join c in os.GetObjectsQuery<Cont>() on p.Cont equals c.ID into conturi
            from c in conturi.DefaultIfEmpty()
            join d in os.GetObjectsQuery<Document>() on p.DocumentId equals d.ID into documente
            from d in documente.DefaultIfEmpty()
            select new JurnalRand {
                Id = p.ID, Spatiu = (int)p.Spatiu, TranzactieId = p.TranzactieId, Data = p.Data,
                NumarNota = d.Numar, ContId = p.Cont, ContSimbol = c.Simbol,
                Sens = p.Latura == N.Latura.Debit ? "D" : "C",
                Debit = p.Latura == N.Latura.Debit ? p.Valoare : 0m,
                Credit = p.Latura == N.Latura.Credit ? p.Valoare : 0m,
                DocumentId = p.DocumentId, DocumentNumar = d.Numar, DocumentTip = null,
                Storno = p.Tranzactie.Fel == N.FelTranzactie.Storno
            }).OrderBy(r => r.Data).ThenBy(r => r.TranzactieId).ThenBy(r => r.Id).ThenBy(r => r.Spatiu);
    }

    public static SortingInfo[] OrdineJurnal() => new[] {
        OrdineLista.Crescator(nameof(JurnalRand.Data)),
        OrdineLista.Crescator(nameof(JurnalRand.TranzactieId)),
        OrdineLista.Crescator(nameof(JurnalRand.Id)),
        OrdineLista.Crescator(nameof(JurnalRand.Spatiu))
    };

    public static void CompleteazaTipDocument(IObjectSpace os, IEnumerable<IRandCuDocument> randuri) {
        var cuDocument = randuri?.Where(r => r?.DocumentId != null).ToList();
        if (cuDocument == null || cuDocument.Count == 0)
            return;
        var tipuri = ApiProiectii.CoduriTip(os,
            cuDocument.Select(r => r.DocumentId.Value).Distinct().ToList());
        foreach (var rand in cuDocument)
            rand.DocumentTip = tipuri.GetValueOrDefault(rand.DocumentId.Value);
    }
}
