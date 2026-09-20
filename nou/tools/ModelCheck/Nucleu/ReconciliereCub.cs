#nullable enable
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.ModelCheck;

/// <summary>
/// Gate-ul de reconciliere al fizicii pe tipurile migrate (S-D9.2): SQL brut pe
/// set, cubul persistat contra registrelor vechi, toleranță 0. Grupul unui tip
/// migrat e documentul lui ∪ conexele lui autogenerate (TR-D3).
/// </summary>
static class ReconciliereCub {
    /// <param name="Cheie">coordonata pe care se compară, gata de citit.</param>
    public sealed record Rand(string Litera, string Cheie, decimal Cub, decimal Registre) {
        public decimal Delta => Cub - Registre;

        public override string ToString() =>
            $"{Litera}  {Cheie}  cub={Numar(Cub)}  registre={Numar(Registre)}  Δ={Numar(Delta)}";

        static string Numar(decimal v) => v.ToString("0.####", CultureInfo.InvariantCulture);
    }

    // Grupul: capul de grup e documentul operat al unui tip migrat; conexul e
    // documentul autogenerat al tipului-țintă declarat de `PoliticiConex` (NIR-ul
    // facturii), ale cărui registre aparțin aceleiași fizici (S-D9.2 (a)).
    const string Grupul = """
        with migrat as (
            select t."ID" as tip, t."Cod" as cod, t."ClrType" as clr
            from "TipuriDocument" t
            where t."GCRecord" = 0 and t."PosteazaInCub"),
        cap as (
            select d."ID" as id, m.cod as grup
            from "Documente" d join migrat m on m.clr = d."ClrType"
            where d."GCRecord" = 0 and d."Stare" = 1 {0}),
        conex as (
            select c."ID" as id, cap.grup
            from "Documente" c
            join cap on cap.id = c."DocumentSursaId"
            join migrat m on m.cod = cap.grup
            join "PoliticiConex" p on p."TipDocumentSursaId" = m.tip and p."GCRecord" = 0
            join "TipuriDocument" tt on tt."ID" = p."TipDocumentTintaId"
            where c."GCRecord" = 0 and c."Autogenerat" and c."ClrType" = tt."ClrType" {1}),
        grup as (select id, grup from cap union select id, grup from conex)
        """;

    public static IReadOnlyList<string> TipuriMigrate(DbContext ctx) {
        ArgumentNullException.ThrowIfNull(ctx);
        return Citeste(ctx,
            "select t.\"Cod\" from \"TipuriDocument\" t where t.\"GCRecord\" = 0 and t.\"PosteazaInCub\" "
            + "order by t.\"Cod\"",
            null,
            cititor => cititor.GetString(0));
    }

    /// <param name="documente">
    /// setul pe care se reconciliază; <c>null</c> = toată baza. Scena ModelCheck îl
    /// dă ca să nu măsoare documentele operate de alte scene cu tipul nemigrat.
    /// </param>
    public static List<Rand> Ruleaza(DbContext ctx, IReadOnlyCollection<Guid>? documente = null) {
        ArgumentNullException.ThrowIfNull(ctx);
        var set = documente?.Distinct().ToArray();
        var randuri = new List<Rand>();
        randuri.AddRange(Contabile(ctx, set));
        randuri.AddRange(Stocuri(ctx, set));
        randuri.AddRange(Fiscale(ctx, set));
        randuri.AddRange(Balanta(ctx, set));
        randuri.AddRange(Numarul(ctx, set));
        return randuri;
    }

    // (a) Σ Valoare per (grup, Cont, Latura, lună). Piciorul de stoc al unei
    // recepții stă pe partiția Stoc (`Spatiu = Stoc ⇔ Lot`, N-D2), deci suma e
    // peste AMBELE partiții — altfel jumătate din fiecare notă ar lipsi.
    static List<Rand> Contabile(DbContext ctx, Guid[]? set) => Citeste(ctx, $$"""
        {{Grupul}},
        cub as (
            select g.grup, p."Cont" as cont, p."Latura" as latura,
                   date_trunc('month', p."Data")::date as luna, sum(p."Valoare") as v
            from "Postare" p
            join "Tranzactie" t on t."ID" = p."TranzactieId"
            join grup g on g.id = p."DocumentId"
            where t."Fel" = 1
            group by 1, 2, 3, 4),
        reg as (
            select grup, cont, latura, luna, sum(v) as v from (
                select g.grup, r."ContDebitId" as cont, 1 as latura,
                       date_trunc('month', r."Data")::date as luna, r."Valoare" as v
                from "RegistruContabil" r join grup g on g.id = r."DocumentId"
                where r."GCRecord" = 0 and not r."Storno"
                union all
                select g.grup, r."ContCreditId", 2,
                       date_trunc('month', r."Data")::date, r."Valoare"
                from "RegistruContabil" r join grup g on g.id = r."DocumentId"
                where r."GCRecord" = 0 and not r."Storno") x
            group by 1, 2, 3, 4)
        select coalesce(c.grup, r.grup),
               coalesce((select k."Simbol" from "Conturi" k where k."ID" = coalesce(c.cont, r.cont)),
                        coalesce(c.cont, r.cont)::text),
               case coalesce(c.latura, r.latura) when 1 then 'D' else 'C' end,
               to_char(coalesce(c.luna, r.luna), 'YYYY-MM'),
               coalesce(c.v, 0), coalesce(r.v, 0)
        from cub c full outer join reg r
          on r.grup = c.grup and r.cont = c.cont and r.latura = c.latura and r.luna = c.luna
        where coalesce(c.v, 0) <> coalesce(r.v, 0)
        order by 1, 4, 2, 3
        """, set, cititor => new Rand(
            "(a) contabil",
            $"{cititor.GetString(0)} {cititor.GetString(2)} {cititor.GetString(1)} {cititor.GetString(3)}",
            cititor.GetDecimal(4),
            cititor.GetDecimal(5)));

    // (b) Σ Cantitate per (grup, unitate = lot, lună) pe partiția Stoc. Capătul
    // virtual N-D4 poartă −q pe partiția Contabil și NU intră: în registre nu are
    // rând de stoc.
    static List<Rand> Stocuri(DbContext ctx, Guid[]? set) => Citeste(ctx, $$"""
        {{Grupul}},
        cub as (
            select g.grup, p."Unitate" as lot, date_trunc('month', p."Data")::date as luna,
                   case when p."Cantitate" >= 0 then '+' else '-' end as semn,
                   sum(p."Cantitate") as v
            from "Postare" p
            join "Tranzactie" t on t."ID" = p."TranzactieId"
            join grup g on g.id = p."DocumentId"
            where t."Fel" = 1 and p."Spatiu" = 2
            group by 1, 2, 3, 4),
        reg as (
            select g.grup, r."LotId" as lot, date_trunc('month', r."Data")::date as luna,
                   case when r."Cantitate" >= 0 then '+' else '-' end as semn,
                   sum(r."Cantitate") as v
            from "RegistruStoc" r join grup g on g.id = r."DocumentId"
            where r."GCRecord" = 0 and not r."Storno"
            group by 1, 2, 3, 4)
        select coalesce(c.grup, r.grup),
               coalesce(left(coalesce(c.lot, r.lot)::text, 8), '(fără unitate)'),
               to_char(coalesce(c.luna, r.luna), 'YYYY-MM'),
               coalesce(c.semn, r.semn),
               coalesce(c.v, 0), coalesce(r.v, 0)
        from cub c full outer join reg r
          on r.grup = c.grup and r.lot = c.lot and r.luna = c.luna and r.semn = c.semn
        where coalesce(c.v, 0) <> coalesce(r.v, 0)
        order by 1, 3, 2, 4
        """, set, cititor => new Rand(
            "(b) stoc",
            $"{cititor.GetString(0)} lot={cititor.GetString(1)} {cititor.GetString(2)} "
            + $"semn={cititor.GetString(3)}",
            cititor.GetDecimal(4),
            cititor.GetDecimal(5)));

    // (c) Σ per (grup, TipTva, Sens, Rol, PerioadaDeclarare): în cub faptul fiscal
    // e ATRIBUT al postării interne (B-D8 pct. 5), în registre e rândul `RegistruTva`
    // cu cele două cifre (Bază, Taxă).
    static List<Rand> Fiscale(DbContext ctx, Guid[]? set) => Citeste(ctx, $$"""
        {{Grupul}},
        cub as (
            select g.grup, p."TipTvaId" as tip, p."SensTva" as sens, p."RolTva" as rol,
                   p."PerioadaDeclarare" as per, sum(p."Valoare") as v
            from "Postare" p
            join "Tranzactie" t on t."ID" = p."TranzactieId"
            join grup g on g.id = p."DocumentId"
            where t."Fel" = 1 and p."TipTvaId" is not null
            group by 1, 2, 3, 4, 5),
        reg as (
            select grup, tip, sens, rol, per, sum(v) as v from (
                select g.grup, r."TipTvaId" as tip, r."Sens" as sens, 1 as rol,
                       r."PerioadaAn" * 100 + r."PerioadaLuna" as per, r."Baza" as v
                from "RegistruTva" r join grup g on g.id = r."DocumentId"
                where r."GCRecord" = 0 and not r."Storno"
                union all
                select g.grup, r."TipTvaId", r."Sens", 2,
                       r."PerioadaAn" * 100 + r."PerioadaLuna", r."Tva"
                from "RegistruTva" r join grup g on g.id = r."DocumentId"
                where r."GCRecord" = 0 and not r."Storno") x
            group by 1, 2, 3, 4, 5)
        select coalesce(c.grup, r.grup),
               coalesce((select k."Cod" from "TipuriTva" k where k."ID" = coalesce(c.tip, r.tip)),
                        coalesce(c.tip, r.tip)::text),
               case coalesce(c.sens, r.sens) when 1 then 'achizitie' else 'livrare' end,
               case coalesce(c.rol, r.rol) when 1 then 'baza' else 'taxa' end,
               coalesce(c.per, r.per),
               coalesce(c.v, 0), coalesce(r.v, 0)
        from cub c full outer join reg r
          on r.grup = c.grup and r.tip = c.tip and r.sens = c.sens and r.rol = c.rol and r.per = c.per
        where coalesce(c.v, 0) <> coalesce(r.v, 0)
        order by 1, 5, 2, 4
        """, set, cititor => new Rand(
            "(c) fiscal",
            $"{cititor.GetString(0)} {cititor.GetString(1)}/{cititor.GetString(2)}/{cititor.GetString(3)} "
            + $"per={cititor.GetInt32(4)}",
            cititor.GetDecimal(5),
            cititor.GetDecimal(6)));

    // (d) fiecare tranzacție e balansată pe fiecare `Carte` (conservarea, persistată).
    static List<Rand> Balanta(DbContext ctx, Guid[]? set) => Citeste(ctx, $$"""
        {{Grupul}}
        select t."ID"::text, t."Fel", p."Carte",
               sum(case when p."Latura" = 1 then p."Valoare" else -p."Valoare" end)
        from "Postare" p
        join "Tranzactie" t on t."ID" = p."TranzactieId"
        join grup g on g.id = t."DocumentId"
        group by 1, 2, 3
        having sum(case when p."Latura" = 1 then p."Valoare" else -p."Valoare" end) <> 0
        order by 1, 3
        """, set, cititor => new Rand(
            "(d) balanță",
            $"tranzacția {cititor.GetString(0)[..8]} fel={cititor.GetInt16(1)} carte={cititor.GetInt16(2)}: "
            + "Σ D − Σ C",
            cititor.GetDecimal(3),
            0m));

    // (e) un document operat al unui tip migrat ⇔ EXACT o tranzacție `Operare`;
    // și nicio tranzacție `Operare` pe un document care nu e al unui tip migrat.
    static List<Rand> Numarul(DbContext ctx, Guid[]? set) => Citeste(ctx, $$"""
        {{Grupul}},
        peDocument as (
            select cap.id, (select count(*) from "Tranzactie" t
                            where t."DocumentId" = cap.id and t."Fel" = 1) as cate
            from cap),
        straine as (
            select t."ID" as id from "Tranzactie" t
            where t."Fel" = 1 and (t."DocumentId" is null or t."DocumentId" not in (select id from cap)))
        select 'document ' || id::text || ': tranzacții `Operare`', cate::numeric, 1::numeric
        from peDocument where cate <> 1
        union all
        select 'tranzacții `Operare` pe documente în afara tipurilor migrate', count(*)::numeric, 0::numeric
        from straine having count(*) > 0
        """, set, cititor => new Rand(
            "(e) număr", cititor.GetString(0), cititor.GetDecimal(1), cititor.GetDecimal(2)));

    static List<T> Citeste<T>(
            DbContext ctx, string sql, Guid[]? set, Func<System.Data.Common.DbDataReader, T> proiectie) {
        var conn = ctx.Database.GetDbConnection();
        var deschisEu = conn.State != System.Data.ConnectionState.Open;
        if (deschisEu)
            conn.Open();
        try {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = string.Format(
                CultureInfo.InvariantCulture,
                sql,
                set is null ? "" : "and d.\"ID\" = any(@doc)",
                set is null ? "" : "and c.\"ID\" = any(@doc)");
            if (set is not null) {
                var p = cmd.CreateParameter();
                p.ParameterName = "doc";
                p.Value = set;
                cmd.Parameters.Add(p);
            }
            cmd.CommandTimeout = 0;
            using var cititor = cmd.ExecuteReader();
            var rezultat = new List<T>();
            while (cititor.Read())
                rezultat.Add(proiectie(cititor));
            return rezultat;
        }
        finally {
            if (deschisEu)
                conn.Close();
        }
    }

    public static string Raport(IReadOnlyList<Rand> randuri) {
        ArgumentNullException.ThrowIfNull(randuri);
        var text = new StringBuilder();
        foreach (var litera in new[] { "(a) contabil", "(b) stoc", "(c) fiscal", "(d) balanță", "(e) număr" }) {
            var aleLui = randuri.Where(r => r.Litera == litera).ToList();
            text.AppendLine($"{(aleLui.Count == 0 ? "OK  " : "FAIL")} {litera}: "
                + $"{aleLui.Count} rânduri cu Δ ≠ 0 (toleranță 0)");
            foreach (var rand in aleLui.Take(200))
                text.AppendLine($"       {rand}");
            if (aleLui.Count > 200)
                text.AppendLine($"       … încă {aleLui.Count - 200} rânduri");
        }
        return text.ToString().TrimEnd();
    }
}
