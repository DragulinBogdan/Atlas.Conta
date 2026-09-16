using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class F27Pas1PerioadaLant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "InchisaLa",
                table: "PerioadeFiscale",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "InchisaPrimaOara",
                table: "PerioadeFiscale",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InchideriPerioade",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    PerioadaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fel = table.Column<int>(type: "integer", nullable: false),
                    La = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeId = table.Column<Guid>(type: "uuid", nullable: true),
                    De = table.Column<string>(type: "text", nullable: true),
                    Motiv = table.Column<string>(type: "text", nullable: true),
                    Acceptari = table.Column<string>(type: "text", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InchideriPerioade", x => x.ID);
                    table.ForeignKey(
                        name: "FK_InchideriPerioade_PerioadeFiscale_PerioadaId",
                        column: x => x.PerioadaId,
                        principalTable: "PerioadeFiscale",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PerioadeFiscale_An_Luna",
                table: "PerioadeFiscale",
                columns: new[] { "An", "Luna" },
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_InchideriPerioade_PerioadaId",
                table: "InchideriPerioade",
                column: "PerioadaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InchideriPerioade");

            migrationBuilder.DropIndex(
                name: "IX_PerioadeFiscale_An_Luna",
                table: "PerioadeFiscale");

            migrationBuilder.DropColumn(
                name: "InchisaLa",
                table: "PerioadeFiscale");

            migrationBuilder.DropColumn(
                name: "InchisaPrimaOara",
                table: "PerioadeFiscale");
        }
    }
}
