using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class DescarcareGestiune : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TipTvaImplicitId",
                table: "TipuriDocument",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProdusId",
                table: "FacturiIesireDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "GestiuneDescarcareId",
                table: "FacturiIesire",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DescarcariGestiune",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DescarcariGestiune", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DescarcariGestiune_Documente_ID",
                        column: x => x.ID,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DescarcariGestiuneDetalii",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    LinieSursaId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DescarcariGestiuneDetalii", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DescarcariGestiuneDetalii_DocumentDetalii_ID",
                        column: x => x.ID,
                        principalTable: "DocumentDetalii",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DescarcariGestiuneDetalii_DocumentDetalii_LinieSursaId",
                        column: x => x.LinieSursaId,
                        principalTable: "DocumentDetalii",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TipuriDocument_TipTvaImplicitId",
                table: "TipuriDocument",
                column: "TipTvaImplicitId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturiIesireDetalii_ProdusId",
                table: "FacturiIesireDetalii",
                column: "ProdusId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturiIesire_GestiuneDescarcareId",
                table: "FacturiIesire",
                column: "GestiuneDescarcareId");

            migrationBuilder.CreateIndex(
                name: "IX_DescarcariGestiuneDetalii_LinieSursaId",
                table: "DescarcariGestiuneDetalii",
                column: "LinieSursaId");

            migrationBuilder.AddForeignKey(
                name: "FK_FacturiIesire_Gestiuni_GestiuneDescarcareId",
                table: "FacturiIesire",
                column: "GestiuneDescarcareId",
                principalTable: "Gestiuni",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_FacturiIesireDetalii_Produse_ProdusId",
                table: "FacturiIesireDetalii",
                column: "ProdusId",
                principalTable: "Produse",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_TipuriDocument_TipuriTva_TipTvaImplicitId",
                table: "TipuriDocument",
                column: "TipTvaImplicitId",
                principalTable: "TipuriTva",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FacturiIesire_Gestiuni_GestiuneDescarcareId",
                table: "FacturiIesire");

            migrationBuilder.DropForeignKey(
                name: "FK_FacturiIesireDetalii_Produse_ProdusId",
                table: "FacturiIesireDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_TipuriDocument_TipuriTva_TipTvaImplicitId",
                table: "TipuriDocument");

            migrationBuilder.DropTable(
                name: "DescarcariGestiune");

            migrationBuilder.DropTable(
                name: "DescarcariGestiuneDetalii");

            migrationBuilder.DropIndex(
                name: "IX_TipuriDocument_TipTvaImplicitId",
                table: "TipuriDocument");

            migrationBuilder.DropIndex(
                name: "IX_FacturiIesireDetalii_ProdusId",
                table: "FacturiIesireDetalii");

            migrationBuilder.DropIndex(
                name: "IX_FacturiIesire_GestiuneDescarcareId",
                table: "FacturiIesire");

            migrationBuilder.DropColumn(
                name: "TipTvaImplicitId",
                table: "TipuriDocument");

            migrationBuilder.DropColumn(
                name: "ProdusId",
                table: "FacturiIesireDetalii");

            migrationBuilder.DropColumn(
                name: "GestiuneDescarcareId",
                table: "FacturiIesire");
        }
    }
}
