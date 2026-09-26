using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class C104CheieDeductibilitate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReguliDeductibilitate_Categorie_DeLa",
                table: "ReguliDeductibilitate");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliDeductibilitate_Categorie_Fel_DeLa",
                table: "ReguliDeductibilitate",
                columns: new[] { "Categorie", "Fel", "DeLa" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReguliDeductibilitate_Categorie_Fel_DeLa",
                table: "ReguliDeductibilitate");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliDeductibilitate_Categorie_DeLa",
                table: "ReguliDeductibilitate",
                columns: new[] { "Categorie", "DeLa" },
                unique: true);
        }
    }
}
