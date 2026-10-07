using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Migrations;
using N = Atlas.Conta.Nucleu;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class PostareVizual : Migration
    {
        // D9-A12: proiecția de citire a listei XAF; gestiunile virtuale se etichetează din constantele nucleului.
        public static string GestiuniVirtuale() => string.Join("\n", typeof(N.GestiuniVirtuale)
            .GetProperties(BindingFlags.Public | BindingFlags.Static).Where(p => p.PropertyType == typeof(Guid))
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .Select(p => $"            WHEN '{(Guid)p.GetValue(null)}'::uuid THEN '{p.Name}'"));

        public static string View() => $"""
            CREATE VIEW "PostareVizual" AS
            SELECT p.*,
                t."Fel" AS "TranzactieFel",
                d."Numar" AS "DocumentNumar",
                c."Simbol" AS "ContSimbol",
                pa."Cod" AS "PartenerCod",
                CASE p."Gestiune"
            {GestiuniVirtuale()}
                    ELSE g."Cod"
                END AS "GestiuneCod",
                pr."Cod" AS "ProdusCod",
                CASE p."FelUnitate"
                    WHEN 1 THEN pr."Cod" || '/' || to_char(l."Data", 'YYYY-MM-DD')
                    WHEN 2 THEN pa."Cod" || '/' || to_char(p."UnitateDeschisa", 'YYYY-MM-DD')
                    WHEN 3 THEN f."NumarInventar"
                END AS "UnitateCod",
                tt."Cod" AS "TipTvaCod",
                cf."Cod" AS "CodFunctionalCod",
                ce."Cod" AS "CodEconomicCod",
                sf."Cod" AS "SursaFinantareCod",
                u."Cod" AS "UnitateOrganizatoricaCod",
                pj."Cod" AS "ProiectCod",
                cc."Cod" AS "CentruCostCod"
            FROM "Postare" p
            LEFT JOIN "Tranzactie" t ON t."ID" = p."TranzactieId"
            LEFT JOIN "Documente" d ON d."ID" = p."DocumentId"
            LEFT JOIN "Conturi" c ON c."ID" = p."Cont"
            LEFT JOIN "Repartitori" pa ON pa."ID" = p."Partener"
            LEFT JOIN "Repartitori" g ON g."ID" = p."Gestiune"
            LEFT JOIN "Produse" pr ON pr."ID" = p."Produs"
            LEFT JOIN "Loturi" l ON p."FelUnitate" = 1 AND l."ID" = p."Unitate"
            LEFT JOIN "Imobilizari" f ON p."FelUnitate" = 3 AND f."ID" = p."Unitate"
            LEFT JOIN "TipuriTva" tt ON tt."ID" = p."TipTvaId"
            LEFT JOIN "CoduriFunctionale" cf ON cf."ID" = p."CodFunctional"
            LEFT JOIN "CoduriEconomice" ce ON ce."ID" = p."CodEconomic"
            LEFT JOIN "SurseFinantare" sf ON sf."ID" = p."SursaFinantare"
            LEFT JOIN "Unitati" u ON u."ID" = p."UnitateOrganizatorica"
            LEFT JOIN "Proiecte" pj ON pj."ID" = p."Proiect"
            LEFT JOIN "Repartitori" cc ON cc."ID" = p."CentruCost";
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(View());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP VIEW "PostareVizual";""");
        }
    }
}
