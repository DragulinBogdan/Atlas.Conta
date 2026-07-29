using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class FacturaIntrareProdus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProdusId",
                table: "FacturiIntrareDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_FacturiIntrareDetalii_ProdusId",
                table: "FacturiIntrareDetalii",
                column: "ProdusId");

            migrationBuilder.AddForeignKey(
                name: "FK_FacturiIntrareDetalii_Produse_ProdusId",
                table: "FacturiIntrareDetalii",
                column: "ProdusId",
                principalTable: "Produse",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FacturiIntrareDetalii_Produse_ProdusId",
                table: "FacturiIntrareDetalii");

            migrationBuilder.DropIndex(
                name: "IX_FacturiIntrareDetalii_ProdusId",
                table: "FacturiIntrareDetalii");

            migrationBuilder.DropColumn(
                name: "ProdusId",
                table: "FacturiIntrareDetalii");
        }
    }
}
