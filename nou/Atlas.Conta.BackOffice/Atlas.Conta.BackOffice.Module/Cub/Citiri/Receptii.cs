using Microsoft.EntityFrameworkCore;

namespace Atlas.Conta.BackOffice.Module.Cub.Citiri;

/// <summary>Proveniența istorică comună diagnosticelor, fără dependență de politicile de azi.</summary>
public static class Receptii {
    public const string LegaturiSql = """
        select n."ID" as id, s."ID" as sursa
        from "Documente" n
        join "Documente" s on s."ID" = n."SursaReceptieiId"
        where n."ClrType" = 'NIR' and s."ClrType" = 'FacturaIntrare'
          and exists (select 1 from "Postare" p join "Tranzactie" t on t."ID" = p."TranzactieId"
              where p."DocumentId" = s."ID" and t."Fel" = 1 and p."Carte" = 1
                and p."FelUnitate" = 1 and p."Unitate" is not null
                and (n."TranzactieReceptieSursaId" is null or n."TranzactieReceptieSursaId" = t."ID"))
        """;

    public class Legatura {
        public virtual Guid Id { get; set; }
        public virtual Guid Sursa { get; set; }
    }

    public static Dictionary<Guid, Guid> Legaturi(DbContext ctx, IReadOnlyCollection<Guid> documente) =>
        Toate(ctx).Where(r => documente.Contains(r.Id)).ToDictionary(r => r.Id, r => r.Sursa);

    public static Dictionary<Guid, Guid> Legaturi(DbContext ctx) => Toate(ctx).ToDictionary(r => r.Id, r => r.Sursa);

    static IQueryable<Legatura> Toate(DbContext ctx) =>
        ctx.Database.SqlQueryRaw<Legatura>("select r.id as \"Id\", r.sursa as \"Sursa\" from (" + LegaturiSql + ") r");
}
