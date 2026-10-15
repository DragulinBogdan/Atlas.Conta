using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class RetragereDepunere : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DepuneriDeclaratii_Formular_Perioada_VersiuneExportata",
                table: "DepuneriDeclaratii");

            migrationBuilder.AddColumn<Guid>(
                name: "Inlocuieste",
                table: "DepuneriDeclaratii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivRetragere",
                table: "DepuneriDeclaratii",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RetrasaDe",
                table: "DepuneriDeclaratii",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RetrasaLa",
                table: "DepuneriDeclaratii",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DepuneriDeclaratii_Formular_Perioada_VersiuneExportata",
                table: "DepuneriDeclaratii",
                columns: new[] { "Formular", "Perioada", "VersiuneExportata" },
                unique: true,
                filter: "\"RetrasaLa\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DepuneriDeclaratii_Formular_Perioada_VersiuneExportata",
                table: "DepuneriDeclaratii");

            migrationBuilder.DropColumn(
                name: "Inlocuieste",
                table: "DepuneriDeclaratii");

            migrationBuilder.DropColumn(
                name: "MotivRetragere",
                table: "DepuneriDeclaratii");

            migrationBuilder.DropColumn(
                name: "RetrasaDe",
                table: "DepuneriDeclaratii");

            migrationBuilder.DropColumn(
                name: "RetrasaLa",
                table: "DepuneriDeclaratii");

            migrationBuilder.CreateIndex(
                name: "IX_DepuneriDeclaratii_Formular_Perioada_VersiuneExportata",
                table: "DepuneriDeclaratii",
                columns: new[] { "Formular", "Perioada", "VersiuneExportata" },
                unique: true);
        }
    }
}
