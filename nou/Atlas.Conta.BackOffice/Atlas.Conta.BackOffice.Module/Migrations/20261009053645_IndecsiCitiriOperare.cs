using Microsoft.EntityFrameworkCore.Migrations;
using N = Atlas.Conta.Nucleu;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class IndecsiCitiriOperare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Loturi_LinieIntrareId",
                table: "Loturi",
                column: "LinieIntrareId");
            migrationBuilder.Sql($"""
                CREATE INDEX "IX_Postare_Fisa_Cont" ON "Postare" ("Cont") WHERE "FelUnitate" = {(short)N.FelUnitate.Fisa};
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""DROP INDEX "IX_Postare_Fisa_Cont";""");
            migrationBuilder.DropIndex(
                name: "IX_Loturi_LinieIntrareId",
                table: "Loturi");
        }
    }
}
