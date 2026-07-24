using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class NotaContabila : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NoteContabile",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteContabile", x => x.ID);
                    table.ForeignKey(
                        name: "FK_NoteContabile_Documente_ID",
                        column: x => x.ID,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NoteContabileDetalii",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Descriere = table.Column<string>(type: "text", nullable: true),
                    ContDebitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContCreditId = table.Column<Guid>(type: "uuid", nullable: true),
                    RepartitorDebitId = table.Column<Guid>(type: "uuid", nullable: true),
                    RepartitorCreditId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NoteContabileDetalii", x => x.ID);
                    table.ForeignKey(
                        name: "FK_NoteContabileDetalii_Conturi_ContCreditId",
                        column: x => x.ContCreditId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_NoteContabileDetalii_Conturi_ContDebitId",
                        column: x => x.ContDebitId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_NoteContabileDetalii_DocumentDetalii_ID",
                        column: x => x.ID,
                        principalTable: "DocumentDetalii",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NoteContabileDetalii_Repartitori_RepartitorCreditId",
                        column: x => x.RepartitorCreditId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_NoteContabileDetalii_Repartitori_RepartitorDebitId",
                        column: x => x.RepartitorDebitId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_NoteContabileDetalii_ContCreditId",
                table: "NoteContabileDetalii",
                column: "ContCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteContabileDetalii_ContDebitId",
                table: "NoteContabileDetalii",
                column: "ContDebitId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteContabileDetalii_RepartitorCreditId",
                table: "NoteContabileDetalii",
                column: "RepartitorCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_NoteContabileDetalii_RepartitorDebitId",
                table: "NoteContabileDetalii",
                column: "RepartitorDebitId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NoteContabile");

            migrationBuilder.DropTable(
                name: "NoteContabileDetalii");
        }
    }
}
