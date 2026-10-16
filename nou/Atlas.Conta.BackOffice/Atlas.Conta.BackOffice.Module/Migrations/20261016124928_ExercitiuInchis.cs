using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class ExercitiuInchis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "ExercitiuInchis",
                table: "PerioadeFiscale",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ExercitiuInchisLa",
                table: "PerioadeFiscale",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExercitiuInchis",
                table: "PerioadeFiscale");

            migrationBuilder.DropColumn(
                name: "ExercitiuInchisLa",
                table: "PerioadeFiscale");
        }
    }
}
