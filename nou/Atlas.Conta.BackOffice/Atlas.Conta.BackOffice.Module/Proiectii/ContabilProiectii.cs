using System.Runtime.CompilerServices;
using System.Text;
using N = Atlas.Conta.Nucleu;
using Atlas.Conta.BackOffice.Module.Api;
using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Atlas.Conta.BackOffice.Module.Cub.Citiri;
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
    public Guid? MaterialId { get; set; }
    public Guid? CodFunctionalId { get; set; }
    public Guid? CodEconomicId { get; set; }
    public Guid? SursaFinantareId { get; set; }
    public Guid? UnitateId { get; set; }
    public Guid? ProiectId { get; set; }
    public Guid? CentruCostId { get; set; }
}

public sealed class FisaContSold {
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
    public Guid? ContrapartidaId { get; set; }
    public string ContrapartidaSimbol { get; set; }
    public Guid? DocumentId { get; set; }
    public string DocumentTip { get; set; }
    public string DocumentNumar { get; set; }
    public bool Storno { get; set; }
}

public class JurnalSql {
    public virtual Guid Id { get; set; }
    public virtual int Spatiu { get; set; }
    public virtual Guid TranzactieId { get; set; }
    public virtual DateOnly Data { get; set; }
    public virtual string NumarNota { get; set; }
    public virtual Guid ContId { get; set; }
    public virtual string ContSimbol { get; set; }
    public virtual string Sens { get; set; }
    public virtual decimal Debit { get; set; }
    public virtual decimal Credit { get; set; }
    public virtual Guid? ContrapartidaId { get; set; }
    public virtual string ContrapartidaSimbol { get; set; }
    public virtual Guid? DocumentId { get; set; }
    public virtual bool Storno { get; set; }
}

struct SursaJurnal {
    public Guid Id { get; set; }
    public int Spatiu { get; set; }
    public Guid TranzactieId { get; set; }
    public int? Pereche { get; set; }
    public DateOnly Data { get; set; }
    public string NumarNota { get; set; }
    public Guid ContId { get; set; }
    public string ContSimbol { get; set; }
    public string Sens { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public Guid? DocumentId { get; set; }
    public bool Storno { get; set; }
}

struct SursaPereche {
    public Guid Id { get; set; }
    public Guid TranzactieId { get; set; }
    public int? Pereche { get; set; }
    public Guid ContId { get; set; }
    public string ContSimbol { get; set; }
}

struct SursaTert {
    public Guid Id { get; set; }
}

struct SursaFisa {
    public Guid Id { get; set; }
    public Guid TranzactieId { get; set; }
    public int? Pereche { get; set; }
    public Guid ContId { get; set; }
    public int Spatiu { get; set; }
    public DateOnly Data { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public bool Storno { get; set; }
    public Guid? RepartitorId { get; set; }
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

public static partial class ContabilProiectii {

    /// <summary>Atomii balanței și ai snapshot-ului; transferurile conservă pe cont, deci mută valoarea numai între repartitori și materiale (126e).</summary>
    public static IQueryable<AtomContabil> Atomi(IObjectSpace os) =>
        Atomi(Cub.Citiri.Contabil.Analitice(os)).Concat(Atomi(Cub.Citiri.Contabil.AnaliticeTransfer(os)));

    static IQueryable<AtomContabil> Atomi(IQueryable<Cub.Citiri.PostareAnalitica> postari) =>
        postari.Select(p => new AtomContabil {
            Data = p.Data, ContId = p.Cont,
            Debit = p.Sens == Atlas.Conta.Nucleu.Sens.Debit ? p.Valoare : 0m,
            Credit = p.Sens == Atlas.Conta.Nucleu.Sens.Credit ? p.Valoare : 0m,
            RepartitorId = p.Repartitor, MaterialId = p.Produs,
            CodFunctionalId = p.CodFunctional, CodEconomicId = p.CodEconomic,
            SursaFinantareId = p.SursaFinantare, UnitateId = p.UnitateOrganizatorica,
            ProiectId = p.Proiect, CentruCostId = p.CentruCost
        });

    static IQueryable<AtomContabil> AtomiPerioada(IObjectSpace os, DateOnly dataStart, DateOnly dataEnd, CitireCumul citire) =>
        dataStart == DateOnly.MinValue ? Atomi(os).Where(a => a.Data <= dataEnd)
            : SolduriService.AtomiCumulati(os, citire, dataEnd, dataStart.AddDays(-1));

    static IQueryable<AtomContabil> PeDimensiuni(IQueryable<AtomContabil> atomi, Guid? repartitorId, Guid? materialId,
            Guid? codFunctionalId, Guid? codEconomicId, Guid? sursaFinantareId, Guid? unitateId, Guid? proiectId,
            Guid? centruCostId) {
        if (repartitorId is Guid vRep) atomi = atomi.Where(a => a.RepartitorId == vRep);
        if (materialId is Guid vMat) atomi = atomi.Where(a => a.MaterialId == vMat);
        if (codFunctionalId is Guid vCf) atomi = atomi.Where(a => a.CodFunctionalId == vCf);
        if (codEconomicId is Guid vCe) atomi = atomi.Where(a => a.CodEconomicId == vCe);
        if (sursaFinantareId is Guid vSf) atomi = atomi.Where(a => a.SursaFinantareId == vSf);
        if (unitateId is Guid vUn) atomi = atomi.Where(a => a.UnitateId == vUn);
        if (proiectId is Guid vPr) atomi = atomi.Where(a => a.ProiectId == vPr);
        if (centruCostId is Guid vCc) atomi = atomi.Where(a => a.CentruCostId == vCc);
        return atomi;
    }

    public static IQueryable<SoldPartenerRand> SoldParteneri(
        IObjectSpace os, DateOnly laData, Guid? contId = null,
        Guid? repartitorId = null, Guid? materialId = null, Guid? codFunctionalId = null,
        Guid? codEconomicId = null, Guid? sursaFinantareId = null, Guid? unitateId = null,
        Guid? proiectId = null, Guid? centruCostId = null,
        CitireCumul citire = CitireCumul.Vizibila) {

        var atomi = SolduriService.AtomiCumulati(os, citire, laData);
        if (contId is Guid vCont) atomi = atomi.Where(a => a.ContId == vCont);
        atomi = PeDimensiuni(atomi, repartitorId, materialId, codFunctionalId, codEconomicId,
            sursaFinantareId, unitateId, proiectId, centruCostId);

        // C111-Q13: terții pe rând propriu; restul contului într-un singur rând fără terț.
        var externi = Cub.Citiri.Contabil.Externi(os);
        var agregate = atomi
            .Select(a => new {
                a.ContId, a.Debit, a.Credit,
                RepartitorId = a.RepartitorId != null && externi.Contains(a.RepartitorId.Value) ? a.RepartitorId : null,
            })
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

    public static IQueryable<FisaContRand> FisaCont(
        IObjectSpace os, Guid contId, DateOnly dataStart, DateOnly dataEnd,
        Guid? repartitorId = null, Guid? materialId = null, Guid? codFunctionalId = null,
        Guid? codEconomicId = null, Guid? sursaFinantareId = null, Guid? unitateId = null,
        Guid? proiectId = null, Guid? centruCostId = null, bool repartitorNul = false,
        CitireCumul citire = CitireCumul.Vizibila, bool faraTert = false) {
        var argumente = new List<object>();
        string P(object valoare) { argumente.Add(valoare); return "{" + (argumente.Count - 1) + "}"; }
        var peUnitati = repartitorId != null || materialId != null;
        var postari = Cub.Citiri.Contabil.Analitice(os).Where(p => p.Data <= dataEnd);
        if (peUnitati) postari = postari.Concat(Cub.Citiri.Contabil.AnaliticeTransfer(os).Where(p => p.Data <= dataEnd));
        var etichetate = from p in postari
            join c in os.GetObjectsQuery<Cont>() on p.Cont equals c.ID into conturi
            from c in conturi.DefaultIfEmpty()
            join d in os.GetObjectsQuery<Document>() on p.DocumentId equals d.ID into documente
            from d in documente.DefaultIfEmpty()
            join r in os.GetObjectsQuery<Repartitor>() on p.Repartitor equals r.ID into parteneri
            from r in parteneri.DefaultIfEmpty()
            select new SursaFisa {
                Id = p.Id, Spatiu = (int)p.Spatiu, TranzactieId = p.TranzactieId, Pereche = p.Pereche, Data = p.Data,
                ContId = p.Cont, ContSimbol = c.Simbol, NumarNota = d.Numar, Sens = p.Sens == N.Sens.Debit ? "D" : "C",
                Debit = p.Sens == N.Sens.Debit ? p.Valoare : 0m,
                Credit = p.Sens == N.Sens.Credit ? p.Valoare : 0m,
                RepartitorId = p.Repartitor, MaterialId = p.Produs,
                CodFunctionalId = p.CodFunctional, CodEconomicId = p.CodEconomic,
                SursaFinantareId = p.SursaFinantare, UnitateId = p.UnitateOrganizatorica,
                ProiectId = p.Proiect, CentruCostId = p.CentruCost,
                RepartitorDenumire = r.Denumire, DocumentId = p.DocumentId,
                Storno = p.Fel == N.FelTranzactie.Storno
            };
        var initial = (dataStart == DateOnly.MinValue ? Atomi(os).Where(a => false)
            : SolduriService.AtomiCumulati(os, citire, dataStart.AddDays(-1))).Where(a => a.ContId == contId);
        var dimensiuni = new Dictionary<string, Guid?> {
            ["RepartitorId"] = repartitorId, ["MaterialId"] = materialId,
            ["CodFunctionalId"] = codFunctionalId, ["CodEconomicId"] = codEconomicId,
            ["SursaFinantareId"] = sursaFinantareId, ["UnitateId"] = unitateId,
            ["ProiectId"] = proiectId, ["CentruCostId"] = centruCostId
        };
        string Filtre(string alias) => string.Concat(dimensiuni.Where(d => d.Value.HasValue)
            .Select(d => $" AND {alias}.\"{d.Key}\" = {P(d.Value.Value)}"))
            + (repartitorNul ? $" AND {alias}.\"RepartitorId\" IS NULL" : "")
            + (faraTert ? $" AND NOT EXISTS (SELECT 1 FROM externi e WHERE e.\"Id\" = {alias}.\"RepartitorId\")" : "");
        // C111-Q18: aceeași mulțime ca rândul fără terț din SoldParteneri.
        var externi = faraTert
            ? $"externi AS NOT MATERIALIZED ({SqlInterogare.Compune(Cub.Citiri.Contabil.Externi(os).Select(id => new SursaTert { Id = id }), P)}),\n"
            : "";
        var sursa = SqlInterogare.Compune(etichetate, P);
        var sold = SqlInterogare.Compune(initial, P);
        var sql = $"""
            WITH {externi}postari AS NOT MATERIALIZED ({sursa}),
            perechi AS NOT MATERIALIZED ({SqlInterogare.Compune(Perechi(os, dataStart, dataEnd), P)}),
            initial AS (SELECT COALESCE(SUM(i."Debit" - i."Credit"), 0) AS sold FROM ({sold}) i WHERE true{Filtre("i")})
            SELECT p."Id", p."Spatiu", p."TranzactieId", p."Data", p."NumarNota", p."Sens", p."Debit", p."Credit",
                (SELECT sold FROM initial) + SUM(p."Debit" - p."Credit") OVER (
                    ORDER BY p."Data", p."Id", p."Spatiu" ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS "SoldCurent",
                c."ContId" AS "ContrapartidaId", c."ContSimbol" AS "ContrapartidaSimbol",
                p."RepartitorDenumire", p."DocumentId", NULL::text AS "DocumentTip", p."NumarNota" AS "DocumentNumar", p."Storno"
            FROM postari p
            LEFT JOIN perechi c ON c."TranzactieId" = p."TranzactieId" AND c."Pereche" = p."Pereche" AND c."Id" <> p."Id"
            WHERE p."ContId" = {P(contId)} AND p."Data" >= {P(dataStart)}{Filtre("p")}
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

    /// <summary>Soldul inițial, rulajele și soldul final ale fișei, din atomii balanței, pe filtrele fișei (C111-Q19).</summary>
    public static FisaContSold FisaSold(
        IObjectSpace os, Guid contId, DateOnly dataStart, DateOnly dataEnd,
        Guid? repartitorId = null, Guid? materialId = null, Guid? codFunctionalId = null,
        Guid? codEconomicId = null, Guid? sursaFinantareId = null, Guid? unitateId = null,
        Guid? proiectId = null, Guid? centruCostId = null, bool repartitorNul = false,
        CitireCumul citire = CitireCumul.Vizibila, bool faraTert = false) {
        var atomi = PeDimensiuni(AtomiPerioada(os, dataStart, dataEnd, citire).Where(a => a.ContId == contId),
            repartitorId, materialId, codFunctionalId, codEconomicId, sursaFinantareId, unitateId, proiectId, centruCostId);
        if (repartitorNul) atomi = atomi.Where(a => a.RepartitorId == null);
        if (faraTert) {
            var externi = Cub.Citiri.Contabil.Externi(os);
            atomi = atomi.Where(a => a.RepartitorId == null || !externi.Contains(a.RepartitorId.Value));
        }
        var suma = atomi.GroupBy(a => a.ContId).Select(g => new {
            InitialDebit = g.Sum(a => a.Data < dataStart ? a.Debit : 0m),
            InitialCredit = g.Sum(a => a.Data < dataStart ? a.Credit : 0m),
            RulajDebit = g.Sum(a => a.Data >= dataStart ? a.Debit : 0m),
            RulajCredit = g.Sum(a => a.Data >= dataStart ? a.Credit : 0m)
        }).FirstOrDefault();
        if (suma == null) return new FisaContSold();
        var initial = suma.InitialDebit - suma.InitialCredit;
        var final = initial + suma.RulajDebit - suma.RulajCredit;
        return new FisaContSold {
            SoldInitialDebit = initial > 0m ? initial : 0m, SoldInitialCredit = initial < 0m ? -initial : 0m,
            RulajDebit = suma.RulajDebit, RulajCredit = suma.RulajCredit,
            SoldFinalDebit = final > 0m ? final : 0m, SoldFinalCredit = final < 0m ? -final : 0m
        };
    }

    public static SortingInfo[] OrdineFisa() => new[] {
        OrdineLista.Crescator(nameof(FisaContRand.Data)),
        OrdineLista.Crescator(nameof(FisaContRand.Id)),
        OrdineLista.Crescator(nameof(FisaContRand.Spatiu))
    };

    public static IQueryable<JurnalRand> RegistruJurnal(
        IObjectSpace os, DateOnly? dataStart = null, DateOnly? dataEnd = null) {
        var argumente = new List<object>();
        string P(object valoare) { argumente.Add(valoare); return "{" + (argumente.Count - 1) + "}"; }
        var postari = Cub.Citiri.Contabil.Postari(os);
        if (dataStart is { } ds) postari = postari.Where(p => p.Data >= ds);
        if (dataEnd is { } de) postari = postari.Where(p => p.Data <= de);
        var randuri = from p in postari
            join c in os.GetObjectsQuery<Cont>() on p.Cont equals c.ID into conturi
            from c in conturi.DefaultIfEmpty()
            join d in os.GetObjectsQuery<Document>() on p.DocumentId equals d.ID into documente
            from d in documente.DefaultIfEmpty()
            select new SursaJurnal {
                Id = p.ID, Spatiu = (int)p.Spatiu, TranzactieId = p.TranzactieId, Pereche = p.Pereche, Data = p.Data,
                NumarNota = d.Numar, ContId = p.Cont, ContSimbol = c.Simbol,
                Sens = p.Sens == N.Sens.Debit ? "D" : "C",
                Debit = p.Sens == N.Sens.Debit ? p.Valoare : 0m,
                Credit = p.Sens == N.Sens.Credit ? p.Valoare : 0m,
                DocumentId = p.DocumentId, Storno = p.Tranzactie.Fel == N.FelTranzactie.Storno
            };
        // C111-Q20: egalitatea pe pereche se scrie în SQL; din LINQ, pe coloană nulabilă, iese cu ramură de nul.
        var sql = $"""
            WITH postari AS NOT MATERIALIZED ({SqlInterogare.Compune(randuri, P)}),
            perechi AS NOT MATERIALIZED ({SqlInterogare.Compune(Perechi(os, dataStart, dataEnd), P)})
            SELECT p."Id", p."Spatiu", p."TranzactieId", p."Data", p."NumarNota", p."ContId", p."ContSimbol",
                p."Sens", p."Debit", p."Credit", c."ContId" AS "ContrapartidaId", c."ContSimbol" AS "ContrapartidaSimbol",
                p."DocumentId", p."Storno"
            FROM postari p
            LEFT JOIN perechi c ON c."TranzactieId" = p."TranzactieId" AND c."Pereche" = p."Pereche" AND c."Id" <> p."Id"
            """;
        return ((EFCoreObjectSpace)os).DbContext.Database
            .SqlQuery<JurnalSql>(FormattableStringFactory.Create(sql, argumente.ToArray()))
            .Select(r => new JurnalRand {
                Id = r.Id, Spatiu = r.Spatiu, TranzactieId = r.TranzactieId, Data = r.Data,
                NumarNota = r.NumarNota, ContId = r.ContId, ContSimbol = r.ContSimbol,
                Sens = r.Sens, Debit = r.Debit, Credit = r.Credit,
                ContrapartidaId = r.ContrapartidaId, ContrapartidaSimbol = r.ContrapartidaSimbol,
                DocumentId = r.DocumentId, DocumentNumar = r.NumarNota, DocumentTip = null, Storno = r.Storno
            }).OrderBy(r => r.Data).ThenBy(r => r.TranzactieId).ThenBy(r => r.Id).ThenBy(r => r.Spatiu);
    }

    static IQueryable<SursaPereche> Perechi(IObjectSpace os, DateOnly? deLa, DateOnly? panaLa) =>
        from x in Cub.Citiri.Contabil.Perechi(os, deLa, panaLa)
        join c in os.GetObjectsQuery<Cont>() on x.Cont equals c.ID
        select new SursaPereche { Id = x.ID, TranzactieId = x.TranzactieId, Pereche = x.Pereche, ContId = x.Cont, ContSimbol = c.Simbol };

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
