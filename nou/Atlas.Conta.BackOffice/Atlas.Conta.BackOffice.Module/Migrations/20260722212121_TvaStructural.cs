using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class TvaStructural : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CotaTva",
                table: "FacturiIntrareDetalii");

            migrationBuilder.DropColumn(
                name: "CotaTva",
                table: "FacturiIesireDetalii");

            migrationBuilder.DropColumn(
                name: "CotaTva",
                table: "DecontDetalii");

            migrationBuilder.AddColumn<Guid>(
                name: "TipTvaId",
                table: "DocumentDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ValoareTva",
                table: "DocumentDetalii",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "PoliticiTva",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    TipDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Directie = table.Column<int>(type: "integer", nullable: false),
                    SursaContrapartida = table.Column<int>(type: "integer", nullable: false),
                    ContrapartidaFallbackId = table.Column<Guid>(type: "uuid", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiTva", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PoliticiTva_Conturi_ContrapartidaFallbackId",
                        column: x => x.ContrapartidaFallbackId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_PoliticiTva_TipuriDocument_TipDocumentId",
                        column: x => x.TipDocumentId,
                        principalTable: "TipuriDocument",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TipuriTva",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Cod = table.Column<string>(type: "text", nullable: true),
                    Denumire = table.Column<string>(type: "text", nullable: true),
                    Cota = table.Column<decimal>(type: "numeric", nullable: false),
                    Regim = table.Column<int>(type: "integer", nullable: false),
                    ContTvaDeductibilId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContTvaColectatId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContTvaNeexigibilId = table.Column<Guid>(type: "uuid", nullable: true),
                    CodSafTLivrare = table.Column<string>(type: "text", nullable: true),
                    CodSafTAchizitie = table.Column<string>(type: "text", nullable: true),
                    CategorieD394 = table.Column<string>(type: "text", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipuriTva", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TipuriTva_Conturi_ContTvaColectatId",
                        column: x => x.ContTvaColectatId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TipuriTva_Conturi_ContTvaDeductibilId",
                        column: x => x.ContTvaDeductibilId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TipuriTva_Conturi_ContTvaNeexigibilId",
                        column: x => x.ContTvaNeexigibilId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_TipTvaId",
                table: "DocumentDetalii",
                column: "TipTvaId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiTva_ContrapartidaFallbackId",
                table: "PoliticiTva",
                column: "ContrapartidaFallbackId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiTva_TipDocumentId",
                table: "PoliticiTva",
                column: "TipDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_TipuriTva_ContTvaColectatId",
                table: "TipuriTva",
                column: "ContTvaColectatId");

            migrationBuilder.CreateIndex(
                name: "IX_TipuriTva_ContTvaDeductibilId",
                table: "TipuriTva",
                column: "ContTvaDeductibilId");

            migrationBuilder.CreateIndex(
                name: "IX_TipuriTva_ContTvaNeexigibilId",
                table: "TipuriTva",
                column: "ContTvaNeexigibilId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentDetalii_TipuriTva_TipTvaId",
                table: "DocumentDetalii",
                column: "TipTvaId",
                principalTable: "TipuriTva",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentDetalii_TipuriTva_TipTvaId",
                table: "DocumentDetalii");

            migrationBuilder.DropTable(
                name: "PoliticiTva");

            migrationBuilder.DropTable(
                name: "TipuriTva");

            migrationBuilder.DropIndex(
                name: "IX_DocumentDetalii_TipTvaId",
                table: "DocumentDetalii");

            migrationBuilder.DropColumn(
                name: "TipTvaId",
                table: "DocumentDetalii");

            migrationBuilder.DropColumn(
                name: "ValoareTva",
                table: "DocumentDetalii");

            migrationBuilder.AddColumn<decimal>(
                name: "CotaTva",
                table: "FacturiIntrareDetalii",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CotaTva",
                table: "FacturiIesireDetalii",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CotaTva",
                table: "DecontDetalii",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
