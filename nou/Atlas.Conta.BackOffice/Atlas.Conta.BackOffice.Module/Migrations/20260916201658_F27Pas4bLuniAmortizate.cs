using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class F27Pas4bLuniAmortizate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Luni",
                table: "AmortizariLunareDetalii",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // F27-D4: fiecare linie existentă acoperă exact o lună.
            migrationBuilder.Sql("""
                UPDATE "AmortizariLunareDetalii" SET "Luni" = 1;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Luni",
                table: "AmortizariLunareDetalii");
        }
    }
}
