using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class D300IndexFiltrat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RanduriD300_Cod",
                table: "RanduriD300");

            migrationBuilder.DropIndex(
                name: "IX_MapariD300_TipTvaId_Sens_RandId",
                table: "MapariD300");

            migrationBuilder.CreateIndex(
                name: "IX_RanduriD300_Cod",
                table: "RanduriD300",
                column: "Cod",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MapariD300_TipTvaId_Sens_RandId",
                table: "MapariD300",
                columns: new[] { "TipTvaId", "Sens", "RandId" },
                unique: true,
                filter: "\"GCRecord\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RanduriD300_Cod",
                table: "RanduriD300");

            migrationBuilder.DropIndex(
                name: "IX_MapariD300_TipTvaId_Sens_RandId",
                table: "MapariD300");

            migrationBuilder.CreateIndex(
                name: "IX_RanduriD300_Cod",
                table: "RanduriD300",
                column: "Cod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MapariD300_TipTvaId_Sens_RandId",
                table: "MapariD300",
                columns: new[] { "TipTvaId", "Sens", "RandId" },
                unique: true);
        }
    }
}
