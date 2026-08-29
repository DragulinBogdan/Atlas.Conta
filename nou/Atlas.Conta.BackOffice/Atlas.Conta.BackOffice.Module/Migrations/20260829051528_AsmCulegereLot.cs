using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class AsmCulegereLot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProdusId",
                table: "AsamblariDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AsamblariDetalii_ProdusId",
                table: "AsamblariDetalii",
                column: "ProdusId");

            migrationBuilder.AddForeignKey(
                name: "FK_AsamblariDetalii_Produse_ProdusId",
                table: "AsamblariDetalii",
                column: "ProdusId",
                principalTable: "Produse",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AsamblariDetalii_Produse_ProdusId",
                table: "AsamblariDetalii");

            migrationBuilder.DropIndex(
                name: "IX_AsamblariDetalii_ProdusId",
                table: "AsamblariDetalii");

            migrationBuilder.DropColumn(
                name: "ProdusId",
                table: "AsamblariDetalii");
        }
    }
}
