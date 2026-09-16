using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class F27Pas4PerioadaDeclarare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PerioadaAn",
                table: "RegistruTva",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PerioadaLuna",
                table: "RegistruTva",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "ScrisLa",
                table: "RegistruTva",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "DeclarareIntarziata",
                table: "PoliticiTva",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // F27-D5: totul ce există a fost declarat în perioada FAPTULUI (nu
            // existau perioade închise cu documente întârziate), iar momentul
            // scrierii rândului e cel al operării documentului lui.
            migrationBuilder.Sql("""
                UPDATE "RegistruTva" r SET
                    "PerioadaAn" = EXTRACT(YEAR FROM r."Data"),
                    "PerioadaLuna" = EXTRACT(MONTH FROM r."Data"),
                    "ScrisLa" = COALESCE(
                        (SELECT d."DataOperare" FROM "Documente" d WHERE d."ID" = r."DocumentId"),
                        r."Data"::timestamptz);
                """);

            migrationBuilder.CreateIndex(
                name: "IX_RegistruTva_PerioadaAn_PerioadaLuna",
                table: "RegistruTva",
                columns: new[] { "PerioadaAn", "PerioadaLuna" },
                filter: "\"GCRecord\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RegistruTva_PerioadaAn_PerioadaLuna",
                table: "RegistruTva");

            migrationBuilder.DropColumn(
                name: "PerioadaAn",
                table: "RegistruTva");

            migrationBuilder.DropColumn(
                name: "PerioadaLuna",
                table: "RegistruTva");

            migrationBuilder.DropColumn(
                name: "ScrisLa",
                table: "RegistruTva");

            migrationBuilder.DropColumn(
                name: "DeclarareIntarziata",
                table: "PoliticiTva");
        }
    }
}
