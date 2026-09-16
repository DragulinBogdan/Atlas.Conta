using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class F27Pas5Corectie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CorecteazaId",
                table: "Documente",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MotivCorectie",
                table: "Documente",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documente_CorecteazaId",
                table: "Documente",
                column: "CorecteazaId",
                filter: "\"GCRecord\" = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Documente_Documente_CorecteazaId",
                table: "Documente",
                column: "CorecteazaId",
                principalTable: "Documente",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documente_Documente_CorecteazaId",
                table: "Documente");

            migrationBuilder.DropIndex(
                name: "IX_Documente_CorecteazaId",
                table: "Documente");

            migrationBuilder.DropColumn(
                name: "CorecteazaId",
                table: "Documente");

            migrationBuilder.DropColumn(
                name: "MotivCorectie",
                table: "Documente");
        }
    }
}
