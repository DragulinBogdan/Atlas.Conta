using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class ListaDiferenteInventar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SemnFiltru",
                table: "ReguliContare",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "DataExpirare",
                table: "ListeDiferenteInventarDetalii",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LotFabricatie",
                table: "ListeDiferenteInventarDetalii",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SemnFiltru",
                table: "ReguliContare");

            migrationBuilder.DropColumn(
                name: "DataExpirare",
                table: "ListeDiferenteInventarDetalii");

            migrationBuilder.DropColumn(
                name: "LotFabricatie",
                table: "ListeDiferenteInventarDetalii");
        }
    }
}
