using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class NirCulegereLot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DataExpirare",
                table: "NIRDetalii",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LotFabricatie",
                table: "NIRDetalii",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PretUnitar",
                table: "NIRDetalii",
                type: "numeric(18,6)",
                precision: 18,
                scale: 6,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<Guid>(
                name: "ProdusId",
                table: "NIRDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_NIRDetalii_ProdusId",
                table: "NIRDetalii",
                column: "ProdusId");

            migrationBuilder.AddForeignKey(
                name: "FK_NIRDetalii_Produse_ProdusId",
                table: "NIRDetalii",
                column: "ProdusId",
                principalTable: "Produse",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_NIRDetalii_Produse_ProdusId",
                table: "NIRDetalii");

            migrationBuilder.DropIndex(
                name: "IX_NIRDetalii_ProdusId",
                table: "NIRDetalii");

            migrationBuilder.DropColumn(
                name: "DataExpirare",
                table: "NIRDetalii");

            migrationBuilder.DropColumn(
                name: "LotFabricatie",
                table: "NIRDetalii");

            migrationBuilder.DropColumn(
                name: "PretUnitar",
                table: "NIRDetalii");

            migrationBuilder.DropColumn(
                name: "ProdusId",
                table: "NIRDetalii");
        }
    }
}
