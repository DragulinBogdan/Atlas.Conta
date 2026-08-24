using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class LaturaPereche : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LaturaPerecheId",
                table: "DocumentTrezorerie",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTrezorerie_LaturaPerecheId",
                table: "DocumentTrezorerie",
                column: "LaturaPerecheId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentTrezorerie_DocumentTrezorerie_LaturaPerecheId",
                table: "DocumentTrezorerie",
                column: "LaturaPerecheId",
                principalTable: "DocumentTrezorerie",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentTrezorerie_DocumentTrezorerie_LaturaPerecheId",
                table: "DocumentTrezorerie");

            migrationBuilder.DropIndex(
                name: "IX_DocumentTrezorerie_LaturaPerecheId",
                table: "DocumentTrezorerie");

            migrationBuilder.DropColumn(
                name: "LaturaPerecheId",
                table: "DocumentTrezorerie");
        }
    }
}
