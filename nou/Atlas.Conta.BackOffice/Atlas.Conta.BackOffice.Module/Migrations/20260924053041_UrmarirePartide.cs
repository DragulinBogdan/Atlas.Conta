using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class UrmarirePartide : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "UrmarestePartide",
                table: "Conturi",
                type: "boolean",
                nullable: false,
                defaultValue: false);
            migrationBuilder.Sql("""
                UPDATE "Conturi" SET "UrmarestePartide" = TRUE WHERE "RolTert" IN (1, 2);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UrmarestePartide",
                table: "Conturi");
        }
    }
}
