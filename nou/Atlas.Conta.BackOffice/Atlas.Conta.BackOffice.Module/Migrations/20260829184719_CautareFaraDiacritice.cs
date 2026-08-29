using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class CautareFaraDiacritice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Cautare",
                table: "UnitatiMasura",
                type: "text",
                nullable: true,
                computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "Cautare",
                table: "TipuriTva",
                type: "text",
                nullable: true,
                computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "Cautare",
                table: "TipuriMaterial",
                type: "text",
                nullable: true,
                computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "Cautare",
                table: "TipuriDocument",
                type: "text",
                nullable: true,
                computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "Cautare",
                table: "SurseFinantare",
                type: "text",
                nullable: true,
                computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "Cautare",
                table: "Repartitori",
                type: "text",
                nullable: true,
                computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "Cautare",
                table: "Proiecte",
                type: "text",
                nullable: true,
                computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "Cautare",
                table: "Produse",
                type: "text",
                nullable: true,
                computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "Cautare",
                table: "Judete",
                type: "text",
                nullable: true,
                computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "Cautare",
                table: "Conturi",
                type: "text",
                nullable: true,
                computedColumnSql: "translate(lower(coalesce(\"Simbol\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "Cautare",
                table: "CoduriFunctionale",
                type: "text",
                nullable: true,
                computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "Cautare",
                table: "CoduriEconomice",
                type: "text",
                nullable: true,
                computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "Cautare",
                table: "ClaseProduse",
                type: "text",
                nullable: true,
                computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')",
                stored: true);

            migrationBuilder.AddColumn<string>(
                name: "Cautare",
                table: "Angajamente",
                type: "text",
                nullable: true,
                computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')",
                stored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cautare",
                table: "UnitatiMasura");

            migrationBuilder.DropColumn(
                name: "Cautare",
                table: "TipuriTva");

            migrationBuilder.DropColumn(
                name: "Cautare",
                table: "TipuriMaterial");

            migrationBuilder.DropColumn(
                name: "Cautare",
                table: "TipuriDocument");

            migrationBuilder.DropColumn(
                name: "Cautare",
                table: "SurseFinantare");

            migrationBuilder.DropColumn(
                name: "Cautare",
                table: "Repartitori");

            migrationBuilder.DropColumn(
                name: "Cautare",
                table: "Proiecte");

            migrationBuilder.DropColumn(
                name: "Cautare",
                table: "Produse");

            migrationBuilder.DropColumn(
                name: "Cautare",
                table: "Judete");

            migrationBuilder.DropColumn(
                name: "Cautare",
                table: "Conturi");

            migrationBuilder.DropColumn(
                name: "Cautare",
                table: "CoduriFunctionale");

            migrationBuilder.DropColumn(
                name: "Cautare",
                table: "CoduriEconomice");

            migrationBuilder.DropColumn(
                name: "Cautare",
                table: "ClaseProduse");

            migrationBuilder.DropColumn(
                name: "Cautare",
                table: "Angajamente");
        }
    }
}
