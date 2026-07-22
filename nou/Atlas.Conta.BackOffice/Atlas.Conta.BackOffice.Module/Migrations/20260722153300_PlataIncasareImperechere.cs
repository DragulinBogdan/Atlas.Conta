using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class PlataIncasareImperechere : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ContImplicitId",
                table: "Repartitori",
                type: "uuid",
                nullable: true);

            // Decizia 31: ContImplicit urcă de pe Partener pe baza Repartitor —
            // valorile existente se mută, nu se pierd.
            migrationBuilder.Sql("""
                UPDATE "Repartitori" r
                SET "ContImplicitId" = p."ContImplicitId"
                FROM "Parteneri" p
                WHERE p."ID" = r."ID" AND p."ContImplicitId" IS NOT NULL;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Parteneri_Conturi_ContImplicitId",
                table: "Parteneri");

            migrationBuilder.DropIndex(
                name: "IX_Parteneri_ContImplicitId",
                table: "Parteneri");

            migrationBuilder.DropColumn(
                name: "ContImplicitId",
                table: "Parteneri");

            migrationBuilder.AddColumn<bool>(
                name: "Autogenerat",
                table: "Imperecheri",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Repartitori_ContImplicitId",
                table: "Repartitori",
                column: "ContImplicitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Repartitori_Conturi_ContImplicitId",
                table: "Repartitori",
                column: "ContImplicitId",
                principalTable: "Conturi",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ContImplicitId",
                table: "Parteneri",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE "Parteneri" p
                SET "ContImplicitId" = r."ContImplicitId"
                FROM "Repartitori" r
                WHERE r."ID" = p."ID" AND r."ContImplicitId" IS NOT NULL;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_Repartitori_Conturi_ContImplicitId",
                table: "Repartitori");

            migrationBuilder.DropIndex(
                name: "IX_Repartitori_ContImplicitId",
                table: "Repartitori");

            migrationBuilder.DropColumn(
                name: "ContImplicitId",
                table: "Repartitori");

            migrationBuilder.DropColumn(
                name: "Autogenerat",
                table: "Imperecheri");

            migrationBuilder.CreateIndex(
                name: "IX_Parteneri_ContImplicitId",
                table: "Parteneri",
                column: "ContImplicitId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parteneri_Conturi_ContImplicitId",
                table: "Parteneri",
                column: "ContImplicitId",
                principalTable: "Conturi",
                principalColumn: "ID");
        }
    }
}
