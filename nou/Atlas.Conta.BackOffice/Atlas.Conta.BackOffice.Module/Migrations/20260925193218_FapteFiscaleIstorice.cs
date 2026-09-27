using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class FapteFiscaleIstorice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CotaTva",
                table: "Postare",
                type: "numeric(18,4)",
                precision: 18,
                scale: 4,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataDocument",
                table: "Postare",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataExigibilitate",
                table: "Postare",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataInregistrare",
                table: "Postare",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataPrimire",
                table: "Postare",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DeImport",
                table: "Postare",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DocumentFiscalId",
                table: "Postare",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "InversaTehnica",
                table: "Postare",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PerioadaD394",
                table: "Postare",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<short>(
                name: "RegimTva",
                table: "Postare",
                type: "smallint",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "RegularizareD300",
                table: "Postare",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataExigibilitate",
                table: "Documente",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataPrimire",
                table: "Documente",
                type: "date",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "DepuneriDeclaratii",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Formular = table.Column<int>(type: "integer", nullable: false),
                    Perioada = table.Column<int>(type: "integer", nullable: false),
                    VersiuneExportata = table.Column<string>(type: "text", nullable: true),
                    ConfirmataLa = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ConfirmataDe = table.Column<string>(type: "text", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DepuneriDeclaratii", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DepuneriDeclaratii_Formular_Perioada_VersiuneExportata",
                table: "DepuneriDeclaratii",
                columns: new[] { "Formular", "Perioada", "VersiuneExportata" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DepuneriDeclaratii");

            migrationBuilder.DropColumn(
                name: "CotaTva",
                table: "Postare");

            migrationBuilder.DropColumn(
                name: "DataDocument",
                table: "Postare");

            migrationBuilder.DropColumn(
                name: "DataExigibilitate",
                table: "Postare");

            migrationBuilder.DropColumn(
                name: "DataInregistrare",
                table: "Postare");

            migrationBuilder.DropColumn(
                name: "DataPrimire",
                table: "Postare");

            migrationBuilder.DropColumn(
                name: "DeImport",
                table: "Postare");

            migrationBuilder.DropColumn(
                name: "DocumentFiscalId",
                table: "Postare");

            migrationBuilder.DropColumn(
                name: "InversaTehnica",
                table: "Postare");

            migrationBuilder.DropColumn(
                name: "PerioadaD394",
                table: "Postare");

            migrationBuilder.DropColumn(
                name: "RegimTva",
                table: "Postare");

            migrationBuilder.DropColumn(
                name: "RegularizareD300",
                table: "Postare");

            migrationBuilder.DropColumn(
                name: "DataExigibilitate",
                table: "Documente");

            migrationBuilder.DropColumn(
                name: "DataPrimire",
                table: "Documente");
        }
    }
}
