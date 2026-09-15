using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class F26CodEconomicImobilizare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CodEconomicId",
                table: "Imobilizari",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CodEconomicId",
                table: "IesiriImobilizariDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CodEconomicId",
                table: "AmortizariLunareDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Imobilizari_CodEconomicId",
                table: "Imobilizari",
                column: "CodEconomicId");

            migrationBuilder.CreateIndex(
                name: "IX_IesiriImobilizariDetalii_CodEconomicId",
                table: "IesiriImobilizariDetalii",
                column: "CodEconomicId");

            migrationBuilder.CreateIndex(
                name: "IX_AmortizariLunareDetalii_CodEconomicId",
                table: "AmortizariLunareDetalii",
                column: "CodEconomicId");

            migrationBuilder.AddForeignKey(
                name: "FK_AmortizariLunareDetalii_CoduriEconomice_CodEconomicId",
                table: "AmortizariLunareDetalii",
                column: "CodEconomicId",
                principalTable: "CoduriEconomice",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_IesiriImobilizariDetalii_CoduriEconomice_CodEconomicId",
                table: "IesiriImobilizariDetalii",
                column: "CodEconomicId",
                principalTable: "CoduriEconomice",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Imobilizari_CoduriEconomice_CodEconomicId",
                table: "Imobilizari",
                column: "CodEconomicId",
                principalTable: "CoduriEconomice",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AmortizariLunareDetalii_CoduriEconomice_CodEconomicId",
                table: "AmortizariLunareDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_IesiriImobilizariDetalii_CoduriEconomice_CodEconomicId",
                table: "IesiriImobilizariDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_Imobilizari_CoduriEconomice_CodEconomicId",
                table: "Imobilizari");

            migrationBuilder.DropIndex(
                name: "IX_Imobilizari_CodEconomicId",
                table: "Imobilizari");

            migrationBuilder.DropIndex(
                name: "IX_IesiriImobilizariDetalii_CodEconomicId",
                table: "IesiriImobilizariDetalii");

            migrationBuilder.DropIndex(
                name: "IX_AmortizariLunareDetalii_CodEconomicId",
                table: "AmortizariLunareDetalii");

            migrationBuilder.DropColumn(
                name: "CodEconomicId",
                table: "Imobilizari");

            migrationBuilder.DropColumn(
                name: "CodEconomicId",
                table: "IesiriImobilizariDetalii");

            migrationBuilder.DropColumn(
                name: "CodEconomicId",
                table: "AmortizariLunareDetalii");
        }
    }
}
