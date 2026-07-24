using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class InchidereTva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InchideriTva",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InchideriTva", x => x.ID);
                    table.ForeignKey(
                        name: "FK_InchideriTva_NoteContabile_ID",
                        column: x => x.ID,
                        principalTable: "NoteContabile",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PoliticiInchidereTva",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    TipDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContDeductibilaId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContColectataId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContDePlataId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContDeRecuperatId = table.Column<Guid>(type: "uuid", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiInchidereTva", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PoliticiInchidereTva_Conturi_ContColectataId",
                        column: x => x.ContColectataId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_PoliticiInchidereTva_Conturi_ContDePlataId",
                        column: x => x.ContDePlataId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_PoliticiInchidereTva_Conturi_ContDeRecuperatId",
                        column: x => x.ContDeRecuperatId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_PoliticiInchidereTva_Conturi_ContDeductibilaId",
                        column: x => x.ContDeductibilaId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_PoliticiInchidereTva_TipuriDocument_TipDocumentId",
                        column: x => x.TipDocumentId,
                        principalTable: "TipuriDocument",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiInchidereTva_ContColectataId",
                table: "PoliticiInchidereTva",
                column: "ContColectataId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiInchidereTva_ContDeductibilaId",
                table: "PoliticiInchidereTva",
                column: "ContDeductibilaId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiInchidereTva_ContDePlataId",
                table: "PoliticiInchidereTva",
                column: "ContDePlataId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiInchidereTva_ContDeRecuperatId",
                table: "PoliticiInchidereTva",
                column: "ContDeRecuperatId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiInchidereTva_TipDocumentId",
                table: "PoliticiInchidereTva",
                column: "TipDocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InchideriTva");

            migrationBuilder.DropTable(
                name: "PoliticiInchidereTva");
        }
    }
}
