using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class AddD300 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RanduriD300",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: true),
                    Denumire = table.Column<string>(type: "text", nullable: true),
                    Sectiune = table.Column<int>(type: "integer", nullable: false),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    AreBaza = table.Column<bool>(type: "boolean", nullable: false),
                    AreTva = table.Column<bool>(type: "boolean", nullable: false),
                    Fel = table.Column<int>(type: "integer", nullable: false),
                    ParinteId = table.Column<Guid>(type: "uuid", nullable: true),
                    OglindaAId = table.Column<Guid>(type: "uuid", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RanduriD300", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RanduriD300_RanduriD300_OglindaAId",
                        column: x => x.OglindaAId,
                        principalTable: "RanduriD300",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RanduriD300_RanduriD300_ParinteId",
                        column: x => x.ParinteId,
                        principalTable: "RanduriD300",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MapariD300",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    TipTvaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sens = table.Column<int>(type: "integer", nullable: false),
                    RandId = table.Column<Guid>(type: "uuid", nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapariD300", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MapariD300_RanduriD300_RandId",
                        column: x => x.RandId,
                        principalTable: "RanduriD300",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MapariD300_TipuriTva_TipTvaId",
                        column: x => x.TipTvaId,
                        principalTable: "TipuriTva",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MapariD300_RandId",
                table: "MapariD300",
                column: "RandId");

            migrationBuilder.CreateIndex(
                name: "IX_MapariD300_TipTvaId_Sens_RandId",
                table: "MapariD300",
                columns: new[] { "TipTvaId", "Sens", "RandId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RanduriD300_Cod",
                table: "RanduriD300",
                column: "Cod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RanduriD300_OglindaAId",
                table: "RanduriD300",
                column: "OglindaAId");

            migrationBuilder.CreateIndex(
                name: "IX_RanduriD300_ParinteId",
                table: "RanduriD300",
                column: "ParinteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MapariD300");

            migrationBuilder.DropTable(
                name: "RanduriD300");
        }
    }
}
