using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class F27Pas7PoliticaInchidere : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PoliticiInchidere",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    Fel = table.Column<int>(type: "integer", nullable: false),
                    Severitate = table.Column<int>(type: "integer", nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiInchidere", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiInchidere_Fel",
                table: "PoliticiInchidere",
                column: "Fel",
                unique: true,
                filter: "\"GCRecord\" = 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PoliticiInchidere");
        }
    }
}
