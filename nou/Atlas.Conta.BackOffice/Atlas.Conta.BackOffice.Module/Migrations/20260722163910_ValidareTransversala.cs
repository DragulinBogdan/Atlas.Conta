using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class ValidareTransversala : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PoliticiValidare",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    TipDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CereClasificatieBugetara = table.Column<bool>(type: "boolean", nullable: false),
                    NaturaInterzisa = table.Column<int>(type: "integer", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiValidare", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PoliticiValidare_TipuriDocument_TipDocumentId",
                        column: x => x.TipDocumentId,
                        principalTable: "TipuriDocument",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiValidare_TipDocumentId",
                table: "PoliticiValidare",
                column: "TipDocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PoliticiValidare");
        }
    }
}
