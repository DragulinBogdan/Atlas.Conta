using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class SensPePostare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Latura",
                table: "Postare",
                newName: "Sens");

            // View-ul își îngheață lista de coloane la creare.
            migrationBuilder.Sql("""ALTER VIEW "PostareVizual" RENAME COLUMN "Latura" TO "Sens";""");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""ALTER VIEW "PostareVizual" RENAME COLUMN "Sens" TO "Latura";""");

            migrationBuilder.RenameColumn(
                name: "Sens",
                table: "Postare",
                newName: "Latura");
        }
    }
}
