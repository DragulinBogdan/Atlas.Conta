using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class F23ImpliciteSiPolitici : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ReguliStoc_TipDocumentId",
                table: "ReguliStoc");

            migrationBuilder.DropIndex(
                name: "IX_ReguliContare_TipDocumentId",
                table: "ReguliContare");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiValidare_TipDocumentId",
                table: "PoliticiValidare");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiTva_TipDocumentId",
                table: "PoliticiTva");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiScadenta_TipDocumentId",
                table: "PoliticiScadenta");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiNumerotare_TipDocumentId",
                table: "PoliticiNumerotare");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiInchidereTva_TipDocumentId",
                table: "PoliticiInchidereTva");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiConex_TipDocumentSursaId",
                table: "PoliticiConex");

            migrationBuilder.AddColumn<bool>(
                name: "Activ",
                table: "TipuriTva",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "DinSeed",
                table: "TipuriTva",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DinSeed",
                table: "TipuriMaterial",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DinSeed",
                table: "TipuriDocument",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DinSeed",
                table: "ReguliStoc",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DinSeed",
                table: "ReguliContare",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TipTvaImplicitId",
                table: "Produse",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DinSeed",
                table: "PoliticiValidare",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DinSeed",
                table: "PoliticiTva",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DinSeed",
                table: "PoliticiScadenta",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DinSeed",
                table: "PoliticiNumerotare",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DinSeed",
                table: "PoliticiMiscareSaft",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DinSeed",
                table: "PoliticiInchidereTva",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DinSeed",
                table: "PoliticiConex",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TipTvaImplicitId",
                table: "Parteneri",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "DinSeed",
                table: "MapariD394",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DinSeed",
                table: "MapariD300",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DinSeed",
                table: "Conturi",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "DinSeed",
                table: "ClaseProduse",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "PoliticiTvaImplicit",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    TipDocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClasaFiscala = table.Column<int>(type: "integer", nullable: true),
                    ValabilDeLa = table.Column<DateOnly>(type: "date", nullable: true),
                    TipTvaId = table.Column<Guid>(type: "uuid", nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiTvaImplicit", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PoliticiTvaImplicit_TipuriDocument_TipDocumentId",
                        column: x => x.TipDocumentId,
                        principalTable: "TipuriDocument",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PoliticiTvaImplicit_TipuriTva_TipTvaId",
                        column: x => x.TipTvaId,
                        principalTable: "TipuriTva",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TipuriTva_Cod",
                table: "TipuriTva",
                column: "Cod",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TipuriMaterial_Cod",
                table: "TipuriMaterial",
                column: "Cod",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TipuriDocument_ClrType",
                table: "TipuriDocument",
                column: "ClrType",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_TipuriDocument_Cod",
                table: "TipuriDocument",
                column: "Cod",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliStoc_TipDocumentId_Latura_ClasaId",
                table: "ReguliStoc",
                columns: new[] { "TipDocumentId", "Latura", "ClasaId" },
                unique: true,
                filter: "\"GCRecord\" = 0")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_TipDocumentId_TipMaterialId_NaturaFiltru_Semn~",
                table: "ReguliContare",
                columns: new[] { "TipDocumentId", "TipMaterialId", "NaturaFiltru", "SemnFiltru" },
                unique: true,
                filter: "\"GCRecord\" = 0")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_Produse_TipTvaImplicitId",
                table: "Produse",
                column: "TipTvaImplicitId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiValidare_TipDocumentId",
                table: "PoliticiValidare",
                column: "TipDocumentId",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiTva_TipDocumentId",
                table: "PoliticiTva",
                column: "TipDocumentId",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiScadenta_TipDocumentId",
                table: "PoliticiScadenta",
                column: "TipDocumentId",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiNumerotare_TipDocumentId",
                table: "PoliticiNumerotare",
                column: "TipDocumentId",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiInchidereTva_TipDocumentId",
                table: "PoliticiInchidereTva",
                column: "TipDocumentId",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiConex_TipDocumentSursaId",
                table: "PoliticiConex",
                column: "TipDocumentSursaId",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Parteneri_TipTvaImplicitId",
                table: "Parteneri",
                column: "TipTvaImplicitId");

            migrationBuilder.CreateIndex(
                name: "IX_Conturi_Simbol",
                table: "Conturi",
                column: "Simbol",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ClaseProduse_Cod",
                table: "ClaseProduse",
                column: "Cod",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiTvaImplicit_TipDocumentId_ClasaFiscala_ValabilDeLa",
                table: "PoliticiTvaImplicit",
                columns: new[] { "TipDocumentId", "ClasaFiscala", "ValabilDeLa" },
                unique: true,
                filter: "\"GCRecord\" = 0")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiTvaImplicit_TipTvaId",
                table: "PoliticiTvaImplicit",
                column: "TipTvaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Parteneri_TipuriTva_TipTvaImplicitId",
                table: "Parteneri",
                column: "TipTvaImplicitId",
                principalTable: "TipuriTva",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Produse_TipuriTva_TipTvaImplicitId",
                table: "Produse",
                column: "TipTvaImplicitId",
                principalTable: "TipuriTva",
                principalColumn: "ID");

            // F23-D4 (amendat) — „TOT CE EXISTĂ LA MIGRAȚIE E CONSIDERAT LIVRAT".
            //
            // Coloana se adaugă cu `DEFAULT FALSE`, deci fără backfill toate
            // rândurile bazelor existente ar apărea ca „manuale" în raportul de
            // profil (F23-D8) — pe bugetar, planul de conturi singur ar fi
            // însemnat 1.679 de constatări false. Timbrul se aprinde AICI, o
            // singură dată: ce era în bază înainte de felia 23 e conținut
            // livrat, fiindcă nu exista ușă prin care clientul să scrie politici
            // (OData era `ReadOnly` pe toate).
            //
            // De ce nu o face seed-ul, marcând și rândurile pe care le GĂSEȘTE
            // pe cheia lui: gardianul stinge timbrul la orice scriere
            // securizată, iar un seed care l-ar re-aprinde ar șterge exact
            // semnalul de divergență pe care flag-ul există să-l arate — la
            // primul `--forceUpdate`. Seed-ul timbrează DOAR ce creează
            // (`ContaSeeder.Seedat`); migrația face restul, o dată.
            //
            // `PoliticiTvaImplicit` NU e în listă: tabela e creată goală de
            // migrația de față, deci n-are rânduri de timbrat — ale ei vin de la
            // seed, care le marchează la creare.
            migrationBuilder.Sql("UPDATE \"TipuriDocument\" SET \"DinSeed\" = TRUE;");
            migrationBuilder.Sql("UPDATE \"TipuriTva\" SET \"DinSeed\" = TRUE;");
            migrationBuilder.Sql("UPDATE \"Conturi\" SET \"DinSeed\" = TRUE;");
            migrationBuilder.Sql("UPDATE \"ClaseProduse\" SET \"DinSeed\" = TRUE;");
            migrationBuilder.Sql("UPDATE \"TipuriMaterial\" SET \"DinSeed\" = TRUE;");
            migrationBuilder.Sql("UPDATE \"ReguliStoc\" SET \"DinSeed\" = TRUE;");
            migrationBuilder.Sql("UPDATE \"ReguliContare\" SET \"DinSeed\" = TRUE;");
            migrationBuilder.Sql("UPDATE \"PoliticiConex\" SET \"DinSeed\" = TRUE;");
            migrationBuilder.Sql("UPDATE \"PoliticiScadenta\" SET \"DinSeed\" = TRUE;");
            migrationBuilder.Sql("UPDATE \"PoliticiValidare\" SET \"DinSeed\" = TRUE;");
            migrationBuilder.Sql("UPDATE \"PoliticiTva\" SET \"DinSeed\" = TRUE;");
            migrationBuilder.Sql("UPDATE \"PoliticiInchidereTva\" SET \"DinSeed\" = TRUE;");
            migrationBuilder.Sql("UPDATE \"PoliticiNumerotare\" SET \"DinSeed\" = TRUE;");
            migrationBuilder.Sql("UPDATE \"PoliticiMiscareSaft\" SET \"DinSeed\" = TRUE;");
            migrationBuilder.Sql("UPDATE \"MapariD300\" SET \"DinSeed\" = TRUE;");
            migrationBuilder.Sql("UPDATE \"MapariD394\" SET \"DinSeed\" = TRUE;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Parteneri_TipuriTva_TipTvaImplicitId",
                table: "Parteneri");

            migrationBuilder.DropForeignKey(
                name: "FK_Produse_TipuriTva_TipTvaImplicitId",
                table: "Produse");

            migrationBuilder.DropTable(
                name: "PoliticiTvaImplicit");

            migrationBuilder.DropIndex(
                name: "IX_TipuriTva_Cod",
                table: "TipuriTva");

            migrationBuilder.DropIndex(
                name: "IX_TipuriMaterial_Cod",
                table: "TipuriMaterial");

            migrationBuilder.DropIndex(
                name: "IX_TipuriDocument_ClrType",
                table: "TipuriDocument");

            migrationBuilder.DropIndex(
                name: "IX_TipuriDocument_Cod",
                table: "TipuriDocument");

            migrationBuilder.DropIndex(
                name: "IX_ReguliStoc_TipDocumentId_Latura_ClasaId",
                table: "ReguliStoc");

            migrationBuilder.DropIndex(
                name: "IX_ReguliContare_TipDocumentId_TipMaterialId_NaturaFiltru_Semn~",
                table: "ReguliContare");

            migrationBuilder.DropIndex(
                name: "IX_Produse_TipTvaImplicitId",
                table: "Produse");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiValidare_TipDocumentId",
                table: "PoliticiValidare");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiTva_TipDocumentId",
                table: "PoliticiTva");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiScadenta_TipDocumentId",
                table: "PoliticiScadenta");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiNumerotare_TipDocumentId",
                table: "PoliticiNumerotare");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiInchidereTva_TipDocumentId",
                table: "PoliticiInchidereTva");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiConex_TipDocumentSursaId",
                table: "PoliticiConex");

            migrationBuilder.DropIndex(
                name: "IX_Parteneri_TipTvaImplicitId",
                table: "Parteneri");

            migrationBuilder.DropIndex(
                name: "IX_Conturi_Simbol",
                table: "Conturi");

            migrationBuilder.DropIndex(
                name: "IX_ClaseProduse_Cod",
                table: "ClaseProduse");

            migrationBuilder.DropColumn(
                name: "Activ",
                table: "TipuriTva");

            migrationBuilder.DropColumn(
                name: "DinSeed",
                table: "TipuriTva");

            migrationBuilder.DropColumn(
                name: "DinSeed",
                table: "TipuriMaterial");

            migrationBuilder.DropColumn(
                name: "DinSeed",
                table: "TipuriDocument");

            migrationBuilder.DropColumn(
                name: "DinSeed",
                table: "ReguliStoc");

            migrationBuilder.DropColumn(
                name: "DinSeed",
                table: "ReguliContare");

            migrationBuilder.DropColumn(
                name: "TipTvaImplicitId",
                table: "Produse");

            migrationBuilder.DropColumn(
                name: "DinSeed",
                table: "PoliticiValidare");

            migrationBuilder.DropColumn(
                name: "DinSeed",
                table: "PoliticiTva");

            migrationBuilder.DropColumn(
                name: "DinSeed",
                table: "PoliticiScadenta");

            migrationBuilder.DropColumn(
                name: "DinSeed",
                table: "PoliticiNumerotare");

            migrationBuilder.DropColumn(
                name: "DinSeed",
                table: "PoliticiMiscareSaft");

            migrationBuilder.DropColumn(
                name: "DinSeed",
                table: "PoliticiInchidereTva");

            migrationBuilder.DropColumn(
                name: "DinSeed",
                table: "PoliticiConex");

            migrationBuilder.DropColumn(
                name: "TipTvaImplicitId",
                table: "Parteneri");

            migrationBuilder.DropColumn(
                name: "DinSeed",
                table: "MapariD394");

            migrationBuilder.DropColumn(
                name: "DinSeed",
                table: "MapariD300");

            migrationBuilder.DropColumn(
                name: "DinSeed",
                table: "Conturi");

            migrationBuilder.DropColumn(
                name: "DinSeed",
                table: "ClaseProduse");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliStoc_TipDocumentId",
                table: "ReguliStoc",
                column: "TipDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_TipDocumentId",
                table: "ReguliContare",
                column: "TipDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiValidare_TipDocumentId",
                table: "PoliticiValidare",
                column: "TipDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiTva_TipDocumentId",
                table: "PoliticiTva",
                column: "TipDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiScadenta_TipDocumentId",
                table: "PoliticiScadenta",
                column: "TipDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiNumerotare_TipDocumentId",
                table: "PoliticiNumerotare",
                column: "TipDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiInchidereTva_TipDocumentId",
                table: "PoliticiInchidereTva",
                column: "TipDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiConex_TipDocumentSursaId",
                table: "PoliticiConex",
                column: "TipDocumentSursaId");
        }
    }
}
