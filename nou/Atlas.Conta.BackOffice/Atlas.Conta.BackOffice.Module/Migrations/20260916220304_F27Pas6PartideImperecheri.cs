using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class F27Pas6PartideImperecheri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "Data",
                table: "Imperecheri",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<Guid>(
                name: "InverseazaId",
                table: "Imperecheri",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalStingere",
                table: "Documente",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PartideDeschise",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    An = table.Column<int>(type: "integer", nullable: false),
                    Luna = table.Column<int>(type: "integer", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    Rest = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartideDeschise", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PartideDeschise_Documente_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            // F27-D7: totalul de stins al documentelor DEJA operate sau stornate —
            // Σ (Valoare + ValoareTva) pe liniile lor. Pentru `ReturClient` suma e
            // doar pe liniile de VENIT (`LiniiCreanta` al lui filtrează `LotId IS
            // NULL`; liniile de cost sunt mișcare internă venit↔stoc), iar filtrul
            // e exprimabil în SQL ca atare — de aceea returul poate intra de acum
            // și în proiecția de rest.
            migrationBuilder.Sql("""
                UPDATE "Documente" d
                   SET "TotalStingere" = COALESCE(t."Suma", 0)
                  FROM (
                        SELECT l."DocumentId", SUM(l."Valoare" + l."ValoareTva") AS "Suma"
                          FROM "DocumentDetalii" l
                         WHERE l."GCRecord" = 0
                           AND (l."LotId" IS NULL
                                OR NOT EXISTS (SELECT 1 FROM "RetururiClient" r
                                                WHERE r."ID" = l."DocumentId"))
                         GROUP BY l."DocumentId"
                       ) t
                 WHERE d."ID" = t."DocumentId" AND d."GCRecord" = 0 AND d."Stare" IN (1, 2)
                """);
            // Documentul operat fără nicio linie de creanță are totalul ZERO, nu
            // „necunoscut": `ImperechereService.Total` refuză `NULL` pe un
            // document ieșit din Draft.
            migrationBuilder.Sql("""
                UPDATE "Documente" SET "TotalStingere" = 0
                 WHERE "GCRecord" = 0 AND "Stare" IN (1, 2) AND "TotalStingere" IS NULL
                """);

            // F27-D8: data faptului de stingere pentru rândurile existente. Nu e
            // data stingătorului singură, ci cea mai TÂRZIE dintre cele două
            // înregistrări — invariantul cerut la scriere („nu preceda niciunul")
            // rămâne adevărat și pe istoric. Pe bazele de azi cele două coincid
            // (`DataInregistrare` a fost ea însăși backfilled din `Data`).
            migrationBuilder.Sql("""
                UPDATE "Imperecheri" i
                   SET "Data" = GREATEST(s."DataInregistrare", d."DataInregistrare")
                  FROM "Documente" s, "Documente" d
                 WHERE s."ID" = i."DocumentStingatorId" AND d."ID" = i."DocumentId"
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Imperecheri_Data",
                table: "Imperecheri",
                column: "Data",
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Imperecheri_InverseazaId",
                table: "Imperecheri",
                column: "InverseazaId",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PartideDeschise_An_Luna",
                table: "PartideDeschise",
                columns: new[] { "An", "Luna" });

            migrationBuilder.CreateIndex(
                name: "IX_PartideDeschise_An_Luna_DocumentId",
                table: "PartideDeschise",
                columns: new[] { "An", "Luna", "DocumentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartideDeschise_DocumentId",
                table: "PartideDeschise",
                column: "DocumentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Imperecheri_Imperecheri_InverseazaId",
                table: "Imperecheri",
                column: "InverseazaId",
                principalTable: "Imperecheri",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Imperecheri_Imperecheri_InverseazaId",
                table: "Imperecheri");

            migrationBuilder.DropTable(
                name: "PartideDeschise");

            migrationBuilder.DropIndex(
                name: "IX_Imperecheri_Data",
                table: "Imperecheri");

            migrationBuilder.DropIndex(
                name: "IX_Imperecheri_InverseazaId",
                table: "Imperecheri");

            migrationBuilder.DropColumn(
                name: "Data",
                table: "Imperecheri");

            migrationBuilder.DropColumn(
                name: "InverseazaId",
                table: "Imperecheri");

            migrationBuilder.DropColumn(
                name: "TotalStingere",
                table: "Documente");
        }
    }
}
