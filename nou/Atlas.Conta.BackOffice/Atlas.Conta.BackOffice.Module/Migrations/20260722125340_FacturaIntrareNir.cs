using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class FacturaIntrareNir : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Loturi_DocumentDetalii_LinieIntrareId",
                table: "Loturi");

            migrationBuilder.DropForeignKey(
                name: "FK_TipuriMaterial_PoliticiConex_PoliticaConexID",
                table: "TipuriMaterial");

            migrationBuilder.DropIndex(
                name: "IX_Loturi_LinieIntrareId",
                table: "Loturi");

            // Fostul FK de colecție PoliticaConex.TipuriMaterialPermise (înlocuit
            // de NaturaFiltru) și noul ContImplicit sunt coloane semantic diferite
            // — drop + add, nu rename (scaffold-ul le-ar fi „redenumit").
            migrationBuilder.DropIndex(
                name: "IX_TipuriMaterial_PoliticaConexID",
                table: "TipuriMaterial");

            migrationBuilder.DropColumn(
                name: "PoliticaConexID",
                table: "TipuriMaterial");

            migrationBuilder.AddColumn<Guid>(
                name: "ContImplicitId",
                table: "TipuriMaterial",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipuriMaterial_ContImplicitId",
                table: "TipuriMaterial",
                column: "ContImplicitId");

            migrationBuilder.AddColumn<int>(
                name: "NaturaFiltru",
                table: "ReguliContare",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SursaContCredit",
                table: "ReguliContare",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "SursaContDebit",
                table: "ReguliContare",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NaturaFiltru",
                table: "PoliticiConex",
                type: "integer",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_TipuriMaterial_Conturi_ContImplicitId",
                table: "TipuriMaterial",
                column: "ContImplicitId",
                principalTable: "Conturi",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TipuriMaterial_Conturi_ContImplicitId",
                table: "TipuriMaterial");

            migrationBuilder.DropColumn(
                name: "NaturaFiltru",
                table: "ReguliContare");

            migrationBuilder.DropColumn(
                name: "SursaContCredit",
                table: "ReguliContare");

            migrationBuilder.DropColumn(
                name: "SursaContDebit",
                table: "ReguliContare");

            migrationBuilder.DropColumn(
                name: "NaturaFiltru",
                table: "PoliticiConex");

            migrationBuilder.DropIndex(
                name: "IX_TipuriMaterial_ContImplicitId",
                table: "TipuriMaterial");

            migrationBuilder.DropColumn(
                name: "ContImplicitId",
                table: "TipuriMaterial");

            migrationBuilder.AddColumn<Guid>(
                name: "PoliticaConexID",
                table: "TipuriMaterial",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipuriMaterial_PoliticaConexID",
                table: "TipuriMaterial",
                column: "PoliticaConexID");

            migrationBuilder.CreateIndex(
                name: "IX_Loturi_LinieIntrareId",
                table: "Loturi",
                column: "LinieIntrareId");

            migrationBuilder.AddForeignKey(
                name: "FK_Loturi_DocumentDetalii_LinieIntrareId",
                table: "Loturi",
                column: "LinieIntrareId",
                principalTable: "DocumentDetalii",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_TipuriMaterial_PoliticiConex_PoliticaConexID",
                table: "TipuriMaterial",
                column: "PoliticaConexID",
                principalTable: "PoliticiConex",
                principalColumn: "ID");
        }
    }
}
