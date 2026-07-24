using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class MigrareLegatura : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MigrareLegaturi",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Tabela = table.Column<string>(type: "text", nullable: true),
                    CheieLegacy = table.Column<string>(type: "text", nullable: true),
                    TintaId = table.Column<Guid>(type: "uuid", nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MigrareLegaturi", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MigrareLegaturi_Tabela_CheieLegacy",
                table: "MigrareLegaturi",
                columns: new[] { "Tabela", "CheieLegacy" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MigrareLegaturi");
        }
    }
}
