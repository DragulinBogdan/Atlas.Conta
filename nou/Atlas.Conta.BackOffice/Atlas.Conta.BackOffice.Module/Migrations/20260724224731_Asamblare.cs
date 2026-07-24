using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class Asamblare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Asamblari",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Asamblari", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Asamblari_Documente_ID",
                        column: x => x.ID,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AsamblariDetalii",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Directie = table.Column<int>(type: "integer", nullable: false),
                    PretEvaluare = table.Column<decimal>(type: "numeric", nullable: true),
                    DataExpirare = table.Column<DateOnly>(type: "date", nullable: true),
                    LotFabricatie = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsamblariDetalii", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AsamblariDetalii_DocumentDetalii_ID",
                        column: x => x.ID,
                        principalTable: "DocumentDetalii",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Asamblari");

            migrationBuilder.DropTable(
                name: "AsamblariDetalii");
        }
    }
}
