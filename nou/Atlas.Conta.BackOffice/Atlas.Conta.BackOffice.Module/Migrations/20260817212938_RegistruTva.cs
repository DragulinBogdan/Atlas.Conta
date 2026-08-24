using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class RegistruTva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegistruTva",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    Sens = table.Column<int>(type: "integer", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DetaliuId = table.Column<Guid>(type: "uuid", nullable: false),
                    PartenerId = table.Column<Guid>(type: "uuid", nullable: true),
                    TipTvaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Regim = table.Column<int>(type: "integer", nullable: false),
                    Cota = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    Baza = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Tva = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Storno = table.Column<bool>(type: "boolean", nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistruTva", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RegistruTva_DocumentDetalii_DetaliuId",
                        column: x => x.DetaliuId,
                        principalTable: "DocumentDetalii",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistruTva_Documente_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RegistruTva_Repartitori_PartenerId",
                        column: x => x.PartenerId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_RegistruTva_TipuriTva_TipTvaId",
                        column: x => x.TipTvaId,
                        principalTable: "TipuriTva",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistruTva_DetaliuId",
                table: "RegistruTva",
                column: "DetaliuId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruTva_DocumentId",
                table: "RegistruTva",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruTva_PartenerId",
                table: "RegistruTva",
                column: "PartenerId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruTva_TipTvaId",
                table: "RegistruTva",
                column: "TipTvaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistruTva");
        }
    }
}
