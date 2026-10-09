using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class OrdinalPostare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP VIEW "PostareVizual";""");

            migrationBuilder.AddColumn<int>(
                name: "Ordinal",
                table: "Postare",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // 117: rândurile existente se numără în ordinea ID-ului; scrierea dă mereu ordinalul, deci fără implicit.
            migrationBuilder.Sql("""
                UPDATE "Postare" p SET "Ordinal" = x.n
                FROM (SELECT "ID", "Spatiu",
                        (row_number() OVER (PARTITION BY "TranzactieId" ORDER BY "ID"))::integer AS n
                    FROM "Postare") x
                WHERE p."ID" = x."ID" AND p."Spatiu" = x."Spatiu";
                ALTER TABLE "Postare" ALTER COLUMN "Ordinal" DROP DEFAULT;
                """);

            migrationBuilder.Sql(PostareVizual.View());
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP VIEW "PostareVizual";""");

            migrationBuilder.DropColumn(
                name: "Ordinal",
                table: "Postare");

            migrationBuilder.Sql(PostareVizual.View());
        }
    }
}
