using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class C102Curatenie : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION cub_partida_anterioara(uuid, uuid)");

            migrationBuilder.DropIndex(
                name: "IX_PartideDeschise_An_Luna_UnitateId_ContId_PartenerId",
                table: "PartideDeschise");

            migrationBuilder.DropColumn(
                name: "DinCub",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropColumn(
                name: "DinCub",
                table: "PartideDeschise");

            migrationBuilder.DropColumn(
                name: "EfectCubVerificat",
                table: "Imperecheri");

            migrationBuilder.CreateIndex(
                name: "IX_PartideDeschise_An_Luna_UnitateId_ContId_PartenerId",
                table: "PartideDeschise",
                columns: new[] { "An", "Luna", "UnitateId", "ContId", "PartenerId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PartideDeschise_An_Luna_UnitateId_ContId_PartenerId",
                table: "PartideDeschise");

            migrationBuilder.AddColumn<bool>(
                name: "DinCub",
                table: "SolduriPerioadaContabil",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DinCub",
                table: "PartideDeschise",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EfectCubVerificat",
                table: "Imperecheri",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_PartideDeschise_An_Luna_UnitateId_ContId_PartenerId",
                table: "PartideDeschise",
                columns: new[] { "An", "Luna", "UnitateId", "ContId", "PartenerId" },
                unique: true,
                filter: "\"DinCub\"");
        }
    }
}
