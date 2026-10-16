using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class InchidereRezultat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CoordonatePastrate",
                table: "DocumentDetalii",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "SursaPeDebit",
                table: "DocumentDetalii",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UnitateId",
                table: "DocumentDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PoliticiInchidereConturi",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    Ordine = table.Column<int>(type: "integer", nullable: false),
                    Moment = table.Column<int>(type: "integer", nullable: false),
                    ContSursaId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContDestinatieId = table.Column<Guid>(type: "uuid", nullable: false),
                    PastreazaRepartitor = table.Column<bool>(type: "boolean", nullable: false),
                    CoordonatePastrate = table.Column<int>(type: "integer", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiInchidereConturi", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PoliticiInchidereConturi_Conturi_ContDestinatieId",
                        column: x => x.ContDestinatieId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_PoliticiInchidereConturi_Conturi_ContSursaId",
                        column: x => x.ContSursaId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_UnitateId",
                table: "DocumentDetalii",
                column: "UnitateId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiInchidereConturi_ContDestinatieId",
                table: "PoliticiInchidereConturi",
                column: "ContDestinatieId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiInchidereConturi_ContSursaId_Moment",
                table: "PoliticiInchidereConturi",
                columns: new[] { "ContSursaId", "Moment" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentDetalii_Unitati_UnitateId",
                table: "DocumentDetalii",
                column: "UnitateId",
                principalTable: "Unitati",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentDetalii_Unitati_UnitateId",
                table: "DocumentDetalii");

            migrationBuilder.DropTable(
                name: "PoliticiInchidereConturi");

            migrationBuilder.DropIndex(
                name: "IX_DocumentDetalii_UnitateId",
                table: "DocumentDetalii");

            migrationBuilder.DropColumn(
                name: "CoordonatePastrate",
                table: "DocumentDetalii");

            migrationBuilder.DropColumn(
                name: "SursaPeDebit",
                table: "DocumentDetalii");

            migrationBuilder.DropColumn(
                name: "UnitateId",
                table: "DocumentDetalii");
        }
    }
}
