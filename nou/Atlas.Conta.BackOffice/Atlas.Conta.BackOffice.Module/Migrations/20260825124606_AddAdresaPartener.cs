using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class AddAdresaPartener : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodPostal",
                table: "Parteneri",
                type: "character varying(18)",
                maxLength: 18,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataSincronizareAnaf",
                table: "Parteneri",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DetaliiAdresa",
                table: "Parteneri",
                type: "character varying(70)",
                maxLength: 70,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "InactivFiscal",
                table: "Parteneri",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "JudetId",
                table: "Parteneri",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Localitate",
                table: "Parteneri",
                type: "character varying(35)",
                maxLength: 35,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Numar",
                table: "Parteneri",
                type: "character varying(18)",
                maxLength: 18,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Strada",
                table: "Parteneri",
                type: "character varying(70)",
                maxLength: 70,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Judete",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Cod = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    Denumire = table.Column<string>(type: "character varying(35)", maxLength: 35, nullable: true),
                    CodAuto = table.Column<string>(type: "character varying(2)", maxLength: 2, nullable: true),
                    CodCnp = table.Column<int>(type: "integer", nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Judete", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Parteneri_JudetId",
                table: "Parteneri",
                column: "JudetId");

            migrationBuilder.CreateIndex(
                name: "IX_Judete_Cod",
                table: "Judete",
                column: "Cod",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_Parteneri_Judete_JudetId",
                table: "Parteneri",
                column: "JudetId",
                principalTable: "Judete",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parteneri_Judete_JudetId",
                table: "Parteneri");

            migrationBuilder.DropTable(
                name: "Judete");

            migrationBuilder.DropIndex(
                name: "IX_Parteneri_JudetId",
                table: "Parteneri");

            migrationBuilder.DropColumn(
                name: "CodPostal",
                table: "Parteneri");

            migrationBuilder.DropColumn(
                name: "DataSincronizareAnaf",
                table: "Parteneri");

            migrationBuilder.DropColumn(
                name: "DetaliiAdresa",
                table: "Parteneri");

            migrationBuilder.DropColumn(
                name: "InactivFiscal",
                table: "Parteneri");

            migrationBuilder.DropColumn(
                name: "JudetId",
                table: "Parteneri");

            migrationBuilder.DropColumn(
                name: "Localitate",
                table: "Parteneri");

            migrationBuilder.DropColumn(
                name: "Numar",
                table: "Parteneri");

            migrationBuilder.DropColumn(
                name: "Strada",
                table: "Parteneri");
        }
    }
}
