using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class ImperechereStingator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Imperecheri_DocumentTrezorerie_DocumentTrezorerieId",
                table: "Imperecheri");

            migrationBuilder.RenameColumn(
                name: "DocumentTrezorerieId",
                table: "Imperecheri",
                newName: "DocumentStingatorId");

            migrationBuilder.RenameIndex(
                name: "IX_Imperecheri_DocumentTrezorerieId",
                table: "Imperecheri",
                newName: "IX_Imperecheri_DocumentStingatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Imperecheri_Documente_DocumentStingatorId",
                table: "Imperecheri",
                column: "DocumentStingatorId",
                principalTable: "Documente",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Imperecheri_Documente_DocumentStingatorId",
                table: "Imperecheri");

            migrationBuilder.RenameColumn(
                name: "DocumentStingatorId",
                table: "Imperecheri",
                newName: "DocumentTrezorerieId");

            migrationBuilder.RenameIndex(
                name: "IX_Imperecheri_DocumentStingatorId",
                table: "Imperecheri",
                newName: "IX_Imperecheri_DocumentTrezorerieId");

            migrationBuilder.AddForeignKey(
                name: "FK_Imperecheri_DocumentTrezorerie_DocumentTrezorerieId",
                table: "Imperecheri",
                column: "DocumentTrezorerieId",
                principalTable: "DocumentTrezorerie",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
