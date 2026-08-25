using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class AddD394 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategorieD394",
                table: "TipuriTva");

            migrationBuilder.AddColumn<bool>(
                name: "InregistratTva",
                table: "Parteneri",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Tara",
                table: "Parteneri",
                type: "text",
                nullable: true);

            // Partenerii existenți sunt persoane juridice (valoarea 1 a enum-ului),
            // nu 0 — un membru inexistent; obiectele noi primesc default-ul din C#.
            migrationBuilder.AddColumn<int>(
                name: "TipPersoana",
                table: "Parteneri",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // Țara partenerilor existenți = RO (default-ul de model, D4-D1): un
            // null aici ar clasifica rândul D394 ca „în afara UE".
            migrationBuilder.Sql("UPDATE \"Parteneri\" SET \"Tara\" = 'RO' WHERE \"Tara\" IS NULL");

            migrationBuilder.AddColumn<bool>(
                name: "TvaLaIncasare",
                table: "Parteneri",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "MapariD394",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    TipTvaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Sens = table.Column<int>(type: "integer", nullable: false),
                    Tip = table.Column<int>(type: "integer", nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MapariD394", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MapariD394_TipuriTva_TipTvaId",
                        column: x => x.TipTvaId,
                        principalTable: "TipuriTva",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MapariD394_TipTvaId_Sens",
                table: "MapariD394",
                columns: new[] { "TipTvaId", "Sens" },
                unique: true,
                filter: "\"GCRecord\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MapariD394");

            migrationBuilder.DropColumn(
                name: "InregistratTva",
                table: "Parteneri");

            migrationBuilder.DropColumn(
                name: "Tara",
                table: "Parteneri");

            migrationBuilder.DropColumn(
                name: "TipPersoana",
                table: "Parteneri");

            migrationBuilder.DropColumn(
                name: "TvaLaIncasare",
                table: "Parteneri");

            migrationBuilder.AddColumn<string>(
                name: "CategorieD394",
                table: "TipuriTva",
                type: "text",
                nullable: true);
        }
    }
}
