using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class F16SaftModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodNc",
                table: "Produse",
                type: "character varying(8)",
                maxLength: 8,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UnitateMasuraId",
                table: "Produse",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RolTert",
                table: "Conturi",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Societati",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Denumire = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true),
                    CodFiscal = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    InregistratTva = table.Column<bool>(type: "boolean", nullable: false),
                    RegistruComert = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Tara = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    Strada = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true),
                    Numar = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    DetaliiAdresa = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true),
                    Localitate = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    CodPostal = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    JudetId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContactNume = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    ContactPrenume = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    Telefon = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    Email = table.Column<string>(type: "character varying(70)", maxLength: 70, nullable: true),
                    ContBancarId = table.Column<Guid>(type: "uuid", nullable: true),
                    BazaContabila = table.Column<string>(type: "character varying(18)", maxLength: 18, nullable: true),
                    RaporteazaCnp = table.Column<bool>(type: "boolean", nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Societati", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Societati_ConturiProprii_ContBancarId",
                        column: x => x.ContBancarId,
                        principalTable: "ConturiProprii",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Societati_Judete_JudetId",
                        column: x => x.JudetId,
                        principalTable: "Judete",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UnitatiMasura",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Cod = table.Column<string>(type: "character varying(9)", maxLength: 9, nullable: true),
                    Denumire = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnitatiMasura", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Produse_UnitateMasuraId",
                table: "Produse",
                column: "UnitateMasuraId");

            migrationBuilder.CreateIndex(
                name: "IX_Societati_ContBancarId",
                table: "Societati",
                column: "ContBancarId");

            migrationBuilder.CreateIndex(
                name: "IX_Societati_JudetId",
                table: "Societati",
                column: "JudetId");

            migrationBuilder.CreateIndex(
                name: "IX_UnitatiMasura_Cod",
                table: "UnitatiMasura",
                column: "Cod",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Produse_UnitatiMasura_UnitateMasuraId",
                table: "Produse",
                column: "UnitateMasuraId",
                principalTable: "UnitatiMasura",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Produse_UnitatiMasura_UnitateMasuraId",
                table: "Produse");

            migrationBuilder.DropTable(
                name: "Societati");

            migrationBuilder.DropTable(
                name: "UnitatiMasura");

            migrationBuilder.DropIndex(
                name: "IX_Produse_UnitateMasuraId",
                table: "Produse");

            migrationBuilder.DropColumn(
                name: "CodNc",
                table: "Produse");

            migrationBuilder.DropColumn(
                name: "UnitateMasuraId",
                table: "Produse");

            migrationBuilder.DropColumn(
                name: "RolTert",
                table: "Conturi");
        }
    }
}
