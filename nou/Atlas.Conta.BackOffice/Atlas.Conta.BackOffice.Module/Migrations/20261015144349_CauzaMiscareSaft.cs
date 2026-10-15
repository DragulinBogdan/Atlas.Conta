using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class CauzaMiscareSaft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PoliticiMiscareSaft_TipDocumentId_TipStoc",
                table: "PoliticiMiscareSaft");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiMiscareSaft_TipDocumentId_TipStoc_Semn",
                table: "PoliticiMiscareSaft");

            migrationBuilder.AddColumn<int>(
                name: "Cauza",
                table: "PoliticiMiscareSaft",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiMiscareSaft_TipDocumentId_TipStoc_Cauza",
                table: "PoliticiMiscareSaft",
                columns: new[] { "TipDocumentId", "TipStoc", "Cauza" },
                unique: true,
                filter: "\"Semn\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiMiscareSaft_TipDocumentId_TipStoc_Semn_Cauza",
                table: "PoliticiMiscareSaft",
                columns: new[] { "TipDocumentId", "TipStoc", "Semn", "Cauza" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PoliticiMiscareSaft_TipDocumentId_TipStoc_Cauza",
                table: "PoliticiMiscareSaft");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiMiscareSaft_TipDocumentId_TipStoc_Semn_Cauza",
                table: "PoliticiMiscareSaft");

            migrationBuilder.DropColumn(
                name: "Cauza",
                table: "PoliticiMiscareSaft");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiMiscareSaft_TipDocumentId_TipStoc",
                table: "PoliticiMiscareSaft",
                columns: new[] { "TipDocumentId", "TipStoc" },
                unique: true,
                filter: "\"Semn\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiMiscareSaft_TipDocumentId_TipStoc_Semn",
                table: "PoliticiMiscareSaft",
                columns: new[] { "TipDocumentId", "TipStoc", "Semn" },
                unique: true);
        }
    }
}
