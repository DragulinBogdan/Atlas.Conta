using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class DiferenteReceptie : Migration
    {
        public const string CompleteazaSursa = """
            UPDATE "Documente" n SET "SursaReceptieiId" = n."DocumentSursaId"
            FROM "Documente" s
            WHERE n."ClrType" = 'NIR' AND n."Autogenerat"
              AND n."SursaReceptieiId" IS NULL
              AND s."ID" = n."DocumentSursaId" AND s."ClrType" = 'FacturaIntrare'
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SursaReceptieiId",
                table: "Documente",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(CompleteazaSursa);

            migrationBuilder.AddColumn<Guid>(
                name: "TranzactieReceptieSursaId",
                table: "Documente",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CauzaDiferentei",
                table: "DocumentDetalii",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LinieSursaReceptieId",
                table: "DocumentDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PartenerDiferentaId",
                table: "DocumentDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PoliticiDiferenta",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    TipDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClasaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Cauza = table.Column<int>(type: "integer", nullable: false),
                    ContId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContPersonalId = table.Column<Guid>(type: "uuid", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiDiferenta", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PoliticiDiferenta_ClaseProduse_ClasaId",
                        column: x => x.ClasaId,
                        principalTable: "ClaseProduse",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PoliticiDiferenta_Conturi_ContId",
                        column: x => x.ContId,
                        principalTable: "Conturi",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PoliticiDiferenta_Conturi_ContPersonalId",
                        column: x => x.ContPersonalId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_PoliticiDiferenta_TipuriDocument_TipDocumentId",
                        column: x => x.TipDocumentId,
                        principalTable: "TipuriDocument",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_PartenerDiferentaId",
                table: "DocumentDetalii",
                column: "PartenerDiferentaId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiDiferenta_ClasaId",
                table: "PoliticiDiferenta",
                column: "ClasaId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiDiferenta_ContId",
                table: "PoliticiDiferenta",
                column: "ContId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiDiferenta_ContPersonalId",
                table: "PoliticiDiferenta",
                column: "ContPersonalId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiDiferenta_TipDocumentId_Cauza_ClasaId",
                table: "PoliticiDiferenta",
                columns: new[] { "TipDocumentId", "Cauza", "ClasaId" },
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentDetalii_Repartitori_PartenerDiferentaId",
                table: "DocumentDetalii",
                column: "PartenerDiferentaId",
                principalTable: "Repartitori",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentDetalii_Repartitori_PartenerDiferentaId",
                table: "DocumentDetalii");

            migrationBuilder.DropTable(
                name: "PoliticiDiferenta");

            migrationBuilder.DropIndex(
                name: "IX_DocumentDetalii_PartenerDiferentaId",
                table: "DocumentDetalii");

            migrationBuilder.DropColumn(
                name: "SursaReceptieiId",
                table: "Documente");

            migrationBuilder.DropColumn(
                name: "TranzactieReceptieSursaId",
                table: "Documente");

            migrationBuilder.DropColumn(
                name: "CauzaDiferentei",
                table: "DocumentDetalii");

            migrationBuilder.DropColumn(
                name: "LinieSursaReceptieId",
                table: "DocumentDetalii");

            migrationBuilder.DropColumn(
                name: "PartenerDiferentaId",
                table: "DocumentDetalii");
        }
    }
}
