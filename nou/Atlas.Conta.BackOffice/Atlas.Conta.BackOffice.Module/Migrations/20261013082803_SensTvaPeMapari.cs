using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class SensTvaPeMapari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Sens",
                table: "MapariTvaSaft",
                newName: "SensTva");

            migrationBuilder.RenameColumn(
                name: "Sens",
                table: "MapariD394",
                newName: "SensTva");

            migrationBuilder.RenameIndex(
                name: "IX_MapariD394_TipTvaId_Sens",
                table: "MapariD394",
                newName: "IX_MapariD394_TipTvaId_SensTva");

            migrationBuilder.RenameColumn(
                name: "Sens",
                table: "MapariD300",
                newName: "SensTva");

            migrationBuilder.RenameIndex(
                name: "IX_MapariD300_TipTvaId_Sens_RandId",
                table: "MapariD300",
                newName: "IX_MapariD300_TipTvaId_SensTva_RandId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SensTva",
                table: "MapariTvaSaft",
                newName: "Sens");

            migrationBuilder.RenameColumn(
                name: "SensTva",
                table: "MapariD394",
                newName: "Sens");

            migrationBuilder.RenameIndex(
                name: "IX_MapariD394_TipTvaId_SensTva",
                table: "MapariD394",
                newName: "IX_MapariD394_TipTvaId_Sens");

            migrationBuilder.RenameColumn(
                name: "SensTva",
                table: "MapariD300",
                newName: "Sens");

            migrationBuilder.RenameIndex(
                name: "IX_MapariD300_TipTvaId_SensTva_RandId",
                table: "MapariD300",
                newName: "IX_MapariD300_TipTvaId_Sens_RandId");
        }
    }
}
