using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class LdiCulegereLot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ProdusId",
                table: "ListeDiferenteInventarDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ListeDiferenteInventarDetalii_ProdusId",
                table: "ListeDiferenteInventarDetalii",
                column: "ProdusId");

            migrationBuilder.AddForeignKey(
                name: "FK_ListeDiferenteInventarDetalii_Produse_ProdusId",
                table: "ListeDiferenteInventarDetalii",
                column: "ProdusId",
                principalTable: "Produse",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ListeDiferenteInventarDetalii_Produse_ProdusId",
                table: "ListeDiferenteInventarDetalii");

            migrationBuilder.DropIndex(
                name: "IX_ListeDiferenteInventarDetalii_ProdusId",
                table: "ListeDiferenteInventarDetalii");

            migrationBuilder.DropColumn(
                name: "ProdusId",
                table: "ListeDiferenteInventarDetalii");
        }
    }
}
