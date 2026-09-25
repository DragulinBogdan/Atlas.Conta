using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class CitiriContabileCub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SolduriPerioadaContabil_An_Luna_ContId_RepartitorId_Materia~",
                table: "SolduriPerioadaContabil");

            migrationBuilder.AddColumn<bool>(
                name: "DinCub",
                table: "SolduriPerioadaContabil",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "GestiuneId",
                table: "SolduriPerioadaContabil",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_An_Luna_ContId_RepartitorId_Gestiun~",
                table: "SolduriPerioadaContabil",
                columns: new[] { "An", "Luna", "ContId", "RepartitorId", "GestiuneId", "MaterialId", "CodFunctionalId", "CodEconomicId", "SursaFinantareId", "UnitateId", "ProiectId", "CentruCostId" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SolduriPerioadaContabil_An_Luna_ContId_RepartitorId_Gestiun~",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropColumn(
                name: "DinCub",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropColumn(
                name: "GestiuneId",
                table: "SolduriPerioadaContabil");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_An_Luna_ContId_RepartitorId_Materia~",
                table: "SolduriPerioadaContabil",
                columns: new[] { "An", "Luna", "ContId", "RepartitorId", "MaterialId", "CodFunctionalId", "CodEconomicId", "SursaFinantareId", "UnitateId", "ProiectId", "CentruCostId" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);
        }
    }
}
