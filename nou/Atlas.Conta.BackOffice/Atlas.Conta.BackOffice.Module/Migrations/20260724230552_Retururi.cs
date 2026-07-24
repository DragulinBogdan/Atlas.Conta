using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class Retururi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PastreazaSemn",
                table: "ReguliContare",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RetururiClient",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetururiClient", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RetururiClient_Documente_ID",
                        column: x => x.ID,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RetururiFurnizor",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetururiFurnizor", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RetururiFurnizor_Documente_ID",
                        column: x => x.ID,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RetururiClient");

            migrationBuilder.DropTable(
                name: "RetururiFurnizor");

            migrationBuilder.DropColumn(
                name: "PastreazaSemn",
                table: "ReguliContare");
        }
    }
}
