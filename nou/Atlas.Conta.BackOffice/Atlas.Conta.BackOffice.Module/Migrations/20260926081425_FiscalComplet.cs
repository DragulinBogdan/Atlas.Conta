using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class FiscalComplet : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeclarareIntarziata",
                table: "PoliticiTva");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Postare_FiscalComplet",
                table: "Postare",
                sql: "\"TipTvaId\" IS NULL OR (\n    \"DocumentId\" IS NOT NULL AND \"LinieId\" IS NOT NULL AND\n    \"SensTva\" IS NOT NULL AND \"RolTva\" IS NOT NULL AND\n    \"RegimTva\" IS NOT NULL AND \"CotaTva\" IS NOT NULL AND \"DeImport\" IS NOT NULL AND\n    \"DocumentFiscalId\" IS NOT NULL AND \"DataDocument\" IS NOT NULL AND\n    \"DataExigibilitate\" IS NOT NULL AND \"DataInregistrare\" IS NOT NULL AND\n    \"PerioadaDeclarare\" IS NOT NULL AND \"PerioadaD394\" IS NOT NULL AND\n    (\"SensTva\" <> 1 OR \"DataPrimire\" IS NOT NULL))");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Postare_FiscalComplet",
                table: "Postare");

            migrationBuilder.AddColumn<int>(
                name: "DeclarareIntarziata",
                table: "PoliticiTva",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
