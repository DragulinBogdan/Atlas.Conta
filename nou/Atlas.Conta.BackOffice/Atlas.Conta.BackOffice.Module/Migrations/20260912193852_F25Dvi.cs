using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class F25Dvi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "DeImport",
                table: "TipuriTva",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Dvi",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dvi", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Dvi_Documente_ID",
                        column: x => x.ID,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DviFacturi",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DviId = table.Column<Guid>(type: "uuid", nullable: false),
                    FacturaId = table.Column<Guid>(type: "uuid", nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DviFacturi", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DviFacturi_Dvi_DviId",
                        column: x => x.DviId,
                        principalTable: "Dvi",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DviFacturi_FacturiIntrare_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "FacturiIntrare",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DviFacturi_DviId_FacturaId",
                table: "DviFacturi",
                columns: new[] { "DviId", "FacturaId" },
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_DviFacturi_FacturaId",
                table: "DviFacturi",
                column: "FacturaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DviFacturi");

            migrationBuilder.DropTable(
                name: "Dvi");

            migrationBuilder.DropColumn(
                name: "DeImport",
                table: "TipuriTva");
        }
    }
}
