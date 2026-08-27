using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class F17PoliticaMiscareSaft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PoliticiMiscareSaft",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    TipDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipStoc = table.Column<int>(type: "integer", nullable: false),
                    Semn = table.Column<int>(type: "integer", nullable: true),
                    CodMiscare = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    RolTert = table.Column<int>(type: "integer", nullable: false),
                    Motiv = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiMiscareSaft", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PoliticiMiscareSaft_TipuriDocument_TipDocumentId",
                        column: x => x.TipDocumentId,
                        principalTable: "TipuriDocument",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiMiscareSaft_TipDocumentId_TipStoc",
                table: "PoliticiMiscareSaft",
                columns: new[] { "TipDocumentId", "TipStoc" },
                unique: true,
                filter: "\"Semn\" IS NULL AND \"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiMiscareSaft_TipDocumentId_TipStoc_Semn",
                table: "PoliticiMiscareSaft",
                columns: new[] { "TipDocumentId", "TipStoc", "Semn" },
                unique: true,
                filter: "\"GCRecord\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PoliticiMiscareSaft");
        }
    }
}
