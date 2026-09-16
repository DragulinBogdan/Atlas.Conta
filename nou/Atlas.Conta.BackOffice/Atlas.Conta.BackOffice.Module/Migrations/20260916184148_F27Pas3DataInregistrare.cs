using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class F27Pas3DataInregistrare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DataInregistrare",
                table: "Documente",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            // F27-D4: documentele existente au intrat în evidență la data lor.
            migrationBuilder.Sql("""
                UPDATE "Documente" SET "DataInregistrare" = "Data";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Documente_DataInregistrare",
                table: "Documente",
                column: "DataInregistrare",
                filter: "\"GCRecord\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Documente_DataInregistrare",
                table: "Documente");

            migrationBuilder.DropColumn(
                name: "DataInregistrare",
                table: "Documente");
        }
    }
}
