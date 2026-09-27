using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class SnapshotStocCub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SolduriPerioadaStoc_Repartitori_RepartitorId",
                table: "SolduriPerioadaStoc");

            migrationBuilder.DropIndex(
                name: "IX_SolduriPerioadaStoc_An_Luna_LotId_RepartitorId_TipStoc",
                table: "SolduriPerioadaStoc");

            migrationBuilder.DropIndex(
                name: "IX_SolduriPerioadaStoc_RepartitorId",
                table: "SolduriPerioadaStoc");

            migrationBuilder.DropColumn(
                name: "TipStoc",
                table: "SolduriPerioadaStoc");

            migrationBuilder.DropColumn(
                name: "RepartitorId",
                table: "SolduriPerioadaStoc");

            migrationBuilder.AddColumn<Guid>(
                name: "ProdusId",
                table: "SolduriPerioadaStoc",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ContId",
                table: "SolduriPerioadaStoc",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Deschisa",
                table: "SolduriPerioadaStoc",
                type: "date",
                nullable: false);

            migrationBuilder.AddColumn<Guid>(
                name: "GestiuneId",
                table: "SolduriPerioadaStoc",
                type: "uuid",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaStoc_An_Luna_LotId_ContId_ProdusId_GestiuneId",
                table: "SolduriPerioadaStoc",
                columns: new[] { "An", "Luna", "LotId", "ContId", "ProdusId", "GestiuneId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SolduriPerioadaStoc_An_Luna_LotId_ContId_ProdusId_GestiuneId",
                table: "SolduriPerioadaStoc");

            migrationBuilder.DropColumn(
                name: "ContId",
                table: "SolduriPerioadaStoc");

            migrationBuilder.DropColumn(
                name: "Deschisa",
                table: "SolduriPerioadaStoc");

            migrationBuilder.DropColumn(
                name: "GestiuneId",
                table: "SolduriPerioadaStoc");

            migrationBuilder.DropColumn(
                name: "ProdusId",
                table: "SolduriPerioadaStoc");

            migrationBuilder.AddColumn<Guid>(
                name: "RepartitorId",
                table: "SolduriPerioadaStoc",
                type: "uuid",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "TipStoc",
                table: "SolduriPerioadaStoc",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaStoc_An_Luna_LotId_RepartitorId_TipStoc",
                table: "SolduriPerioadaStoc",
                columns: new[] { "An", "Luna", "LotId", "RepartitorId", "TipStoc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaStoc_RepartitorId",
                table: "SolduriPerioadaStoc",
                column: "RepartitorId");

            migrationBuilder.AddForeignKey(
                name: "FK_SolduriPerioadaStoc_Repartitori_RepartitorId",
                table: "SolduriPerioadaStoc",
                column: "RepartitorId",
                principalTable: "Repartitori",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
