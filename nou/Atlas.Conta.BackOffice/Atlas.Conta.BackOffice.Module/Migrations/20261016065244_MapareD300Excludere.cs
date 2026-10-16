using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class MapareD300Excludere : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MapariD300_TipTvaId_SensTva_RandId",
                table: "MapariD300");

            migrationBuilder.AlterColumn<Guid>(
                name: "RandId",
                table: "MapariD300",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "Motiv",
                table: "MapariD300",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MapariD300_TipTvaId_SensTva_RandId",
                table: "MapariD300",
                columns: new[] { "TipTvaId", "SensTva", "RandId" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MapariD300_TipTvaId_SensTva_RandId",
                table: "MapariD300");

            migrationBuilder.DropColumn(
                name: "Motiv",
                table: "MapariD300");

            migrationBuilder.AlterColumn<Guid>(
                name: "RandId",
                table: "MapariD300",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MapariD300_TipTvaId_SensTva_RandId",
                table: "MapariD300",
                columns: new[] { "TipTvaId", "SensTva", "RandId" },
                unique: true);
        }
    }
}
