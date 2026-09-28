using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class IntervaleTvaSiAvans : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "ValabilDeLa",
                table: "TipuriTva",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ValabilPanaLa",
                table: "TipuriTva",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RegularizareAvans",
                table: "TipuriMaterial",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "LinieAvansId",
                table: "DocumentDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "TvaCules",
                table: "DocumentDetalii",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("""
                UPDATE "DocumentDetalii" l SET "TvaCules" = (l."ValoareTva" <> 0)
                FROM "Documente" d WHERE d."ID" = l."DocumentId" AND d."Stare" = 0;
                """);

            migrationBuilder.AddCheckConstraint(
                name: "CK_TipTva_Interval",
                table: "TipuriTva",
                sql: "\"ValabilDeLa\" IS NULL OR \"ValabilPanaLa\" IS NULL OR \"ValabilDeLa\" <= \"ValabilPanaLa\"");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_LinieAvansId",
                table: "DocumentDetalii",
                column: "LinieAvansId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_DocumentDetalii_TvaCules",
                table: "DocumentDetalii",
                sql: "NOT \"TvaCules\" OR \"ValoareTva\" <> 0");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentDetalii_DocumentDetalii_LinieAvansId",
                table: "DocumentDetalii",
                column: "LinieAvansId",
                principalTable: "DocumentDetalii",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentDetalii_DocumentDetalii_LinieAvansId",
                table: "DocumentDetalii");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TipTva_Interval",
                table: "TipuriTva");

            migrationBuilder.DropIndex(
                name: "IX_DocumentDetalii_LinieAvansId",
                table: "DocumentDetalii");

            migrationBuilder.DropCheckConstraint(
                name: "CK_DocumentDetalii_TvaCules",
                table: "DocumentDetalii");

            migrationBuilder.DropColumn(
                name: "ValabilDeLa",
                table: "TipuriTva");

            migrationBuilder.DropColumn(
                name: "ValabilPanaLa",
                table: "TipuriTva");

            migrationBuilder.DropColumn(
                name: "RegularizareAvans",
                table: "TipuriMaterial");

            migrationBuilder.DropColumn(
                name: "LinieAvansId",
                table: "DocumentDetalii");

            migrationBuilder.DropColumn(
                name: "TvaCules",
                table: "DocumentDetalii");
        }
    }
}
