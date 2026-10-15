using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class FisierDepunere : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ParametriFisier",
                table: "DepuneriDeclaratii",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Sha256Fisier",
                table: "DepuneriDeclaratii",
                type: "character varying(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ParametriFisier",
                table: "DepuneriDeclaratii");

            migrationBuilder.DropColumn(
                name: "Sha256Fisier",
                table: "DepuneriDeclaratii");
        }
    }
}
