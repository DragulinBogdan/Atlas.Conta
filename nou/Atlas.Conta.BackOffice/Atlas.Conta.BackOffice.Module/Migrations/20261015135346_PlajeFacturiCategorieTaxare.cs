using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class PlajeFacturiCategorieTaxare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CategorieTaxareInversa",
                table: "Produse",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlajeFacturi",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Serie = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    NumarInitial = table.Column<long>(type: "bigint", nullable: false),
                    NumarFinal = table.Column<long>(type: "bigint", nullable: false),
                    DataDecizie = table.Column<DateOnly>(type: "date", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Activ = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlajeFacturi", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlajeFacturi");

            migrationBuilder.DropColumn(
                name: "CategorieTaxareInversa",
                table: "Produse");
        }
    }
}
