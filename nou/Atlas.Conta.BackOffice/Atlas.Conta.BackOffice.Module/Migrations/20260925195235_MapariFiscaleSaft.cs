using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class MapariFiscaleSaft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MapariTvaSaft",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    Versiune = table.Column<string>(type: "text", nullable: true),
                    Sectiune = table.Column<int>(type: "integer", nullable: false),
                    TipTvaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Regim = table.Column<int>(type: "integer", nullable: false),
                    Cota = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    DeImport = table.Column<bool>(type: "boolean", nullable: false),
                    Sens = table.Column<int>(type: "integer", nullable: false),
                    Rol = table.Column<int>(type: "integer", nullable: false),
                    TaxType = table.Column<string>(type: "text", nullable: true),
                    TaxCode = table.Column<string>(type: "text", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapariTvaSaft", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MapariTvaSaft_TipuriTva_TipTvaId",
                        column: x => x.TipTvaId,
                        principalTable: "TipuriTva",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MapariTvaSaft_TipTvaId",
                table: "MapariTvaSaft",
                column: "TipTvaId");

            migrationBuilder.CreateIndex(
                name: "IX_MapariTvaSaft_Versiune_Sectiune_TipTvaId_Regim_Cota_DeImpor~",
                table: "MapariTvaSaft",
                columns: new[] { "Versiune", "Sectiune", "TipTvaId", "Regim", "Cota", "DeImport", "Sens", "Rol" },
                unique: true,
                filter: "\"GCRecord\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MapariTvaSaft");
        }
    }
}
