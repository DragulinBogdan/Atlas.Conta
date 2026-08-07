using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    // DIM-2 (decizia 54c, docs/dim/dim-2-inventar.md): dimensiunile culese coboară
    // de pe baza DocumentDetalii (owned Dimensiuni_*) pe frunzele derivate, ca
    // FK-uri explicite. Ordinea e a mutării de date: gard zgomotos → coloanele noi
    // → UPDATE de mutare → abia apoi DROP pe bază. Gardul refuză migrarea dacă
    // vreo valoare ar fi pierdută (rânduri de bază cu dimensiuni în afara
    // frunzelor mutate, sau componente pe care nicio frunză nu le poartă —
    // R/M/U/CC, fapt F7/F8 din inventar); în practică toate bazele sunt curate.
    /// <inheritdoc />
    public partial class DimensiuniPeFrunze : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Gardul anti-pierdere: orice dimensiune care nu are unde să se mute
            // oprește migrarea cu mesaj explicit (nu se aruncă date în tăcere).
            migrationBuilder.Sql("""
                DO $$
                DECLARE
                    fara_frunza integer;
                    componente_nepurtate integer;
                    sfp_in_afara_fct integer;
                BEGIN
                    SELECT count(*) INTO componente_nepurtate FROM "DocumentDetalii" d
                    WHERE d."Dimensiuni_RepartitorId" IS NOT NULL
                       OR d."Dimensiuni_MaterialId" IS NOT NULL
                       OR d."Dimensiuni_UnitateId" IS NOT NULL
                       OR d."Dimensiuni_CentruCostId" IS NOT NULL;
                    IF componente_nepurtate > 0 THEN
                        RAISE EXCEPTION 'DimensiuniPeFrunze: % linii poartă Repartitor/Material/Unitate/CentruCost pe owned — nicio frunză nu preia aceste componente (inventar F7/F8); tranșați manual înainte de migrare.', componente_nepurtate;
                    END IF;

                    SELECT count(*) INTO sfp_in_afara_fct FROM "DocumentDetalii" d
                    WHERE (d."Dimensiuni_SursaFinantareId" IS NOT NULL
                        OR d."Dimensiuni_CodFunctionalId" IS NOT NULL
                        OR d."Dimensiuni_ProiectId" IS NOT NULL)
                      AND NOT EXISTS (SELECT 1 FROM "FacturiIntrareDetalii" f WHERE f."ID" = d."ID");
                    IF sfp_in_afara_fct > 0 THEN
                        RAISE EXCEPTION 'DimensiuniPeFrunze: % linii ne-FCT poartă SursaFinantare/CodFunctional/Proiect — doar frunza FCT le preia; tranșați manual înainte de migrare.', sfp_in_afara_fct;
                    END IF;

                    SELECT count(*) INTO fara_frunza FROM "DocumentDetalii" d
                    WHERE d."Dimensiuni_CodEconomicId" IS NOT NULL
                      AND NOT EXISTS (SELECT 1 FROM "FacturiIntrareDetalii" f WHERE f."ID" = d."ID")
                      AND NOT EXISTS (SELECT 1 FROM "FacturiIesireDetalii" f WHERE f."ID" = d."ID")
                      AND NOT EXISTS (SELECT 1 FROM "DescarcariGestiuneDetalii" f WHERE f."ID" = d."ID")
                      AND NOT EXISTS (SELECT 1 FROM "ListeDiferenteInventarDetalii" f WHERE f."ID" = d."ID")
                      AND NOT EXISTS (SELECT 1 FROM "DecontDetalii" f WHERE f."ID" = d."ID")
                      AND NOT EXISTS (SELECT 1 FROM "NoteContabileDetalii" f WHERE f."ID" = d."ID");
                    IF fara_frunza > 0 THEN
                        RAISE EXCEPTION 'DimensiuniPeFrunze: % linii de bază (NIR/PLT/INC vechi) poartă CodEconomic care s-ar pierde — tranșați manual înainte de migrare.', fara_frunza;
                    END IF;
                END $$;
                """);

            migrationBuilder.AddColumn<Guid>(
                name: "CodEconomicId",
                table: "NoteContabileDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CodEconomicId",
                table: "ListeDiferenteInventarDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CodEconomicId",
                table: "FacturiIntrareDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CodFunctionalId",
                table: "FacturiIntrareDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProiectId",
                table: "FacturiIntrareDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SursaFinantareId",
                table: "FacturiIntrareDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CodEconomicId",
                table: "FacturiIesireDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CodEconomicId",
                table: "DescarcariGestiuneDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CodEconomicId",
                table: "DecontDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NIRDetalii",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CodEconomicId = table.Column<Guid>(type: "uuid", nullable: true),
                    SursaFinantareId = table.Column<Guid>(type: "uuid", nullable: true),
                    CodFunctionalId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProiectId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NIRDetalii", x => x.ID);
                    table.ForeignKey(
                        name: "FK_NIRDetalii_CoduriEconomice_CodEconomicId",
                        column: x => x.CodEconomicId,
                        principalTable: "CoduriEconomice",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_NIRDetalii_CoduriFunctionale_CodFunctionalId",
                        column: x => x.CodFunctionalId,
                        principalTable: "CoduriFunctionale",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_NIRDetalii_DocumentDetalii_ID",
                        column: x => x.ID,
                        principalTable: "DocumentDetalii",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_NIRDetalii_Proiecte_ProiectId",
                        column: x => x.ProiectId,
                        principalTable: "Proiecte",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_NIRDetalii_SurseFinantare_SursaFinantareId",
                        column: x => x.SursaFinantareId,
                        principalTable: "SurseFinantare",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "TrezorerieDetalii",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    CodEconomicId = table.Column<Guid>(type: "uuid", nullable: true),
                    SursaFinantareId = table.Column<Guid>(type: "uuid", nullable: true),
                    CodFunctionalId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProiectId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrezorerieDetalii", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TrezorerieDetalii_CoduriEconomice_CodEconomicId",
                        column: x => x.CodEconomicId,
                        principalTable: "CoduriEconomice",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TrezorerieDetalii_CoduriFunctionale_CodFunctionalId",
                        column: x => x.CodFunctionalId,
                        principalTable: "CoduriFunctionale",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TrezorerieDetalii_DocumentDetalii_ID",
                        column: x => x.ID,
                        principalTable: "DocumentDetalii",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrezorerieDetalii_Proiecte_ProiectId",
                        column: x => x.ProiectId,
                        principalTable: "Proiecte",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_TrezorerieDetalii_SurseFinantare_SursaFinantareId",
                        column: x => x.SursaFinantareId,
                        principalTable: "SurseFinantare",
                        principalColumn: "ID");
                });

            // Mutarea datelor bază → frunze, ÎNAINTE de DROP (NIRDetalii și
            // TrezorerieDetalii sunt tabele noi — n-au rânduri de mutat; liniile
            // istorice NIR/PLT/INC rămân rânduri de bază, garantat fără
            // dimensiuni de gardul de mai sus).
            migrationBuilder.Sql("""
                UPDATE "FacturiIntrareDetalii" f SET
                    "CodEconomicId" = d."Dimensiuni_CodEconomicId",
                    "SursaFinantareId" = d."Dimensiuni_SursaFinantareId",
                    "CodFunctionalId" = d."Dimensiuni_CodFunctionalId",
                    "ProiectId" = d."Dimensiuni_ProiectId"
                FROM "DocumentDetalii" d WHERE d."ID" = f."ID";

                UPDATE "FacturiIesireDetalii" f SET "CodEconomicId" = d."Dimensiuni_CodEconomicId"
                FROM "DocumentDetalii" d WHERE d."ID" = f."ID";

                UPDATE "DescarcariGestiuneDetalii" f SET "CodEconomicId" = d."Dimensiuni_CodEconomicId"
                FROM "DocumentDetalii" d WHERE d."ID" = f."ID";

                UPDATE "ListeDiferenteInventarDetalii" f SET "CodEconomicId" = d."Dimensiuni_CodEconomicId"
                FROM "DocumentDetalii" d WHERE d."ID" = f."ID";

                UPDATE "DecontDetalii" f SET "CodEconomicId" = d."Dimensiuni_CodEconomicId"
                FROM "DocumentDetalii" d WHERE d."ID" = f."ID";

                UPDATE "NoteContabileDetalii" f SET "CodEconomicId" = d."Dimensiuni_CodEconomicId"
                FROM "DocumentDetalii" d WHERE d."ID" = f."ID";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_NoteContabileDetalii_CodEconomicId",
                table: "NoteContabileDetalii",
                column: "CodEconomicId");

            migrationBuilder.CreateIndex(
                name: "IX_ListeDiferenteInventarDetalii_CodEconomicId",
                table: "ListeDiferenteInventarDetalii",
                column: "CodEconomicId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturiIntrareDetalii_CodEconomicId",
                table: "FacturiIntrareDetalii",
                column: "CodEconomicId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturiIntrareDetalii_CodFunctionalId",
                table: "FacturiIntrareDetalii",
                column: "CodFunctionalId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturiIntrareDetalii_ProiectId",
                table: "FacturiIntrareDetalii",
                column: "ProiectId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturiIntrareDetalii_SursaFinantareId",
                table: "FacturiIntrareDetalii",
                column: "SursaFinantareId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturiIesireDetalii_CodEconomicId",
                table: "FacturiIesireDetalii",
                column: "CodEconomicId");

            migrationBuilder.CreateIndex(
                name: "IX_DescarcariGestiuneDetalii_CodEconomicId",
                table: "DescarcariGestiuneDetalii",
                column: "CodEconomicId");

            migrationBuilder.CreateIndex(
                name: "IX_DecontDetalii_CodEconomicId",
                table: "DecontDetalii",
                column: "CodEconomicId");

            migrationBuilder.CreateIndex(
                name: "IX_NIRDetalii_CodEconomicId",
                table: "NIRDetalii",
                column: "CodEconomicId");

            migrationBuilder.CreateIndex(
                name: "IX_NIRDetalii_CodFunctionalId",
                table: "NIRDetalii",
                column: "CodFunctionalId");

            migrationBuilder.CreateIndex(
                name: "IX_NIRDetalii_ProiectId",
                table: "NIRDetalii",
                column: "ProiectId");

            migrationBuilder.CreateIndex(
                name: "IX_NIRDetalii_SursaFinantareId",
                table: "NIRDetalii",
                column: "SursaFinantareId");

            migrationBuilder.CreateIndex(
                name: "IX_TrezorerieDetalii_CodEconomicId",
                table: "TrezorerieDetalii",
                column: "CodEconomicId");

            migrationBuilder.CreateIndex(
                name: "IX_TrezorerieDetalii_CodFunctionalId",
                table: "TrezorerieDetalii",
                column: "CodFunctionalId");

            migrationBuilder.CreateIndex(
                name: "IX_TrezorerieDetalii_ProiectId",
                table: "TrezorerieDetalii",
                column: "ProiectId");

            migrationBuilder.CreateIndex(
                name: "IX_TrezorerieDetalii_SursaFinantareId",
                table: "TrezorerieDetalii",
                column: "SursaFinantareId");

            migrationBuilder.AddForeignKey(
                name: "FK_DecontDetalii_CoduriEconomice_CodEconomicId",
                table: "DecontDetalii",
                column: "CodEconomicId",
                principalTable: "CoduriEconomice",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_DescarcariGestiuneDetalii_CoduriEconomice_CodEconomicId",
                table: "DescarcariGestiuneDetalii",
                column: "CodEconomicId",
                principalTable: "CoduriEconomice",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_FacturiIesireDetalii_CoduriEconomice_CodEconomicId",
                table: "FacturiIesireDetalii",
                column: "CodEconomicId",
                principalTable: "CoduriEconomice",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_FacturiIntrareDetalii_CoduriEconomice_CodEconomicId",
                table: "FacturiIntrareDetalii",
                column: "CodEconomicId",
                principalTable: "CoduriEconomice",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_FacturiIntrareDetalii_CoduriFunctionale_CodFunctionalId",
                table: "FacturiIntrareDetalii",
                column: "CodFunctionalId",
                principalTable: "CoduriFunctionale",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_FacturiIntrareDetalii_Proiecte_ProiectId",
                table: "FacturiIntrareDetalii",
                column: "ProiectId",
                principalTable: "Proiecte",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_FacturiIntrareDetalii_SurseFinantare_SursaFinantareId",
                table: "FacturiIntrareDetalii",
                column: "SursaFinantareId",
                principalTable: "SurseFinantare",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_ListeDiferenteInventarDetalii_CoduriEconomice_CodEconomicId",
                table: "ListeDiferenteInventarDetalii",
                column: "CodEconomicId",
                principalTable: "CoduriEconomice",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_NoteContabileDetalii_CoduriEconomice_CodEconomicId",
                table: "NoteContabileDetalii",
                column: "CodEconomicId",
                principalTable: "CoduriEconomice",
                principalColumn: "ID");

            // Abia acum, cu datele mutate, owned-ul dispare de pe bază.
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentDetalii_CoduriEconomice_Dimensiuni_CodEconomicId",
                table: "DocumentDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentDetalii_CoduriFunctionale_Dimensiuni_CodFunctionalId",
                table: "DocumentDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentDetalii_Produse_Dimensiuni_MaterialId",
                table: "DocumentDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentDetalii_Proiecte_Dimensiuni_ProiectId",
                table: "DocumentDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentDetalii_Repartitori_Dimensiuni_CentruCostId",
                table: "DocumentDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentDetalii_Repartitori_Dimensiuni_RepartitorId",
                table: "DocumentDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentDetalii_SurseFinantare_Dimensiuni_SursaFinantareId",
                table: "DocumentDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentDetalii_Unitati_Dimensiuni_UnitateId",
                table: "DocumentDetalii");

            migrationBuilder.DropIndex(
                name: "IX_DocumentDetalii_Dimensiuni_CentruCostId",
                table: "DocumentDetalii");

            migrationBuilder.DropIndex(
                name: "IX_DocumentDetalii_Dimensiuni_CodEconomicId",
                table: "DocumentDetalii");

            migrationBuilder.DropIndex(
                name: "IX_DocumentDetalii_Dimensiuni_CodFunctionalId",
                table: "DocumentDetalii");

            migrationBuilder.DropIndex(
                name: "IX_DocumentDetalii_Dimensiuni_MaterialId",
                table: "DocumentDetalii");

            migrationBuilder.DropIndex(
                name: "IX_DocumentDetalii_Dimensiuni_ProiectId",
                table: "DocumentDetalii");

            migrationBuilder.DropIndex(
                name: "IX_DocumentDetalii_Dimensiuni_RepartitorId",
                table: "DocumentDetalii");

            migrationBuilder.DropIndex(
                name: "IX_DocumentDetalii_Dimensiuni_SursaFinantareId",
                table: "DocumentDetalii");

            migrationBuilder.DropIndex(
                name: "IX_DocumentDetalii_Dimensiuni_UnitateId",
                table: "DocumentDetalii");

            migrationBuilder.DropColumn(
                name: "Dimensiuni_CentruCostId",
                table: "DocumentDetalii");

            migrationBuilder.DropColumn(
                name: "Dimensiuni_CodEconomicId",
                table: "DocumentDetalii");

            migrationBuilder.DropColumn(
                name: "Dimensiuni_CodFunctionalId",
                table: "DocumentDetalii");

            migrationBuilder.DropColumn(
                name: "Dimensiuni_MaterialId",
                table: "DocumentDetalii");

            migrationBuilder.DropColumn(
                name: "Dimensiuni_ProiectId",
                table: "DocumentDetalii");

            migrationBuilder.DropColumn(
                name: "Dimensiuni_RepartitorId",
                table: "DocumentDetalii");

            migrationBuilder.DropColumn(
                name: "Dimensiuni_SursaFinantareId",
                table: "DocumentDetalii");

            migrationBuilder.DropColumn(
                name: "Dimensiuni_UnitateId",
                table: "DocumentDetalii");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "Dimensiuni_CentruCostId",
                table: "DocumentDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Dimensiuni_CodEconomicId",
                table: "DocumentDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Dimensiuni_CodFunctionalId",
                table: "DocumentDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Dimensiuni_MaterialId",
                table: "DocumentDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Dimensiuni_ProiectId",
                table: "DocumentDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Dimensiuni_RepartitorId",
                table: "DocumentDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Dimensiuni_SursaFinantareId",
                table: "DocumentDetalii",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Dimensiuni_UnitateId",
                table: "DocumentDetalii",
                type: "uuid",
                nullable: true);

            // Copierea inversă frunze → bază, înainte ca frunzele să-și piardă
            // coloanele (NIRDetalii/TrezorerieDetalii încă există aici).
            migrationBuilder.Sql("""
                UPDATE "DocumentDetalii" d SET
                    "Dimensiuni_CodEconomicId" = f."CodEconomicId",
                    "Dimensiuni_SursaFinantareId" = f."SursaFinantareId",
                    "Dimensiuni_CodFunctionalId" = f."CodFunctionalId",
                    "Dimensiuni_ProiectId" = f."ProiectId"
                FROM "FacturiIntrareDetalii" f WHERE f."ID" = d."ID";

                UPDATE "DocumentDetalii" d SET
                    "Dimensiuni_CodEconomicId" = f."CodEconomicId",
                    "Dimensiuni_SursaFinantareId" = f."SursaFinantareId",
                    "Dimensiuni_CodFunctionalId" = f."CodFunctionalId",
                    "Dimensiuni_ProiectId" = f."ProiectId"
                FROM "NIRDetalii" f WHERE f."ID" = d."ID";

                UPDATE "DocumentDetalii" d SET
                    "Dimensiuni_CodEconomicId" = f."CodEconomicId",
                    "Dimensiuni_SursaFinantareId" = f."SursaFinantareId",
                    "Dimensiuni_CodFunctionalId" = f."CodFunctionalId",
                    "Dimensiuni_ProiectId" = f."ProiectId"
                FROM "TrezorerieDetalii" f WHERE f."ID" = d."ID";

                UPDATE "DocumentDetalii" d SET "Dimensiuni_CodEconomicId" = f."CodEconomicId"
                FROM "FacturiIesireDetalii" f WHERE f."ID" = d."ID";

                UPDATE "DocumentDetalii" d SET "Dimensiuni_CodEconomicId" = f."CodEconomicId"
                FROM "DescarcariGestiuneDetalii" f WHERE f."ID" = d."ID";

                UPDATE "DocumentDetalii" d SET "Dimensiuni_CodEconomicId" = f."CodEconomicId"
                FROM "ListeDiferenteInventarDetalii" f WHERE f."ID" = d."ID";

                UPDATE "DocumentDetalii" d SET "Dimensiuni_CodEconomicId" = f."CodEconomicId"
                FROM "DecontDetalii" f WHERE f."ID" = d."ID";

                UPDATE "DocumentDetalii" d SET "Dimensiuni_CodEconomicId" = f."CodEconomicId"
                FROM "NoteContabileDetalii" f WHERE f."ID" = d."ID";
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_DecontDetalii_CoduriEconomice_CodEconomicId",
                table: "DecontDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_DescarcariGestiuneDetalii_CoduriEconomice_CodEconomicId",
                table: "DescarcariGestiuneDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_FacturiIesireDetalii_CoduriEconomice_CodEconomicId",
                table: "FacturiIesireDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_FacturiIntrareDetalii_CoduriEconomice_CodEconomicId",
                table: "FacturiIntrareDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_FacturiIntrareDetalii_CoduriFunctionale_CodFunctionalId",
                table: "FacturiIntrareDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_FacturiIntrareDetalii_Proiecte_ProiectId",
                table: "FacturiIntrareDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_FacturiIntrareDetalii_SurseFinantare_SursaFinantareId",
                table: "FacturiIntrareDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_ListeDiferenteInventarDetalii_CoduriEconomice_CodEconomicId",
                table: "ListeDiferenteInventarDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_NoteContabileDetalii_CoduriEconomice_CodEconomicId",
                table: "NoteContabileDetalii");

            migrationBuilder.DropTable(
                name: "NIRDetalii");

            migrationBuilder.DropTable(
                name: "TrezorerieDetalii");

            migrationBuilder.DropIndex(
                name: "IX_NoteContabileDetalii_CodEconomicId",
                table: "NoteContabileDetalii");

            migrationBuilder.DropIndex(
                name: "IX_ListeDiferenteInventarDetalii_CodEconomicId",
                table: "ListeDiferenteInventarDetalii");

            migrationBuilder.DropIndex(
                name: "IX_FacturiIntrareDetalii_CodEconomicId",
                table: "FacturiIntrareDetalii");

            migrationBuilder.DropIndex(
                name: "IX_FacturiIntrareDetalii_CodFunctionalId",
                table: "FacturiIntrareDetalii");

            migrationBuilder.DropIndex(
                name: "IX_FacturiIntrareDetalii_ProiectId",
                table: "FacturiIntrareDetalii");

            migrationBuilder.DropIndex(
                name: "IX_FacturiIntrareDetalii_SursaFinantareId",
                table: "FacturiIntrareDetalii");

            migrationBuilder.DropIndex(
                name: "IX_FacturiIesireDetalii_CodEconomicId",
                table: "FacturiIesireDetalii");

            migrationBuilder.DropIndex(
                name: "IX_DescarcariGestiuneDetalii_CodEconomicId",
                table: "DescarcariGestiuneDetalii");

            migrationBuilder.DropIndex(
                name: "IX_DecontDetalii_CodEconomicId",
                table: "DecontDetalii");

            migrationBuilder.DropColumn(
                name: "CodEconomicId",
                table: "NoteContabileDetalii");

            migrationBuilder.DropColumn(
                name: "CodEconomicId",
                table: "ListeDiferenteInventarDetalii");

            migrationBuilder.DropColumn(
                name: "CodEconomicId",
                table: "FacturiIntrareDetalii");

            migrationBuilder.DropColumn(
                name: "CodFunctionalId",
                table: "FacturiIntrareDetalii");

            migrationBuilder.DropColumn(
                name: "ProiectId",
                table: "FacturiIntrareDetalii");

            migrationBuilder.DropColumn(
                name: "SursaFinantareId",
                table: "FacturiIntrareDetalii");

            migrationBuilder.DropColumn(
                name: "CodEconomicId",
                table: "FacturiIesireDetalii");

            migrationBuilder.DropColumn(
                name: "CodEconomicId",
                table: "DescarcariGestiuneDetalii");

            migrationBuilder.DropColumn(
                name: "CodEconomicId",
                table: "DecontDetalii");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_Dimensiuni_CentruCostId",
                table: "DocumentDetalii",
                column: "Dimensiuni_CentruCostId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_Dimensiuni_CodEconomicId",
                table: "DocumentDetalii",
                column: "Dimensiuni_CodEconomicId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_Dimensiuni_CodFunctionalId",
                table: "DocumentDetalii",
                column: "Dimensiuni_CodFunctionalId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_Dimensiuni_MaterialId",
                table: "DocumentDetalii",
                column: "Dimensiuni_MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_Dimensiuni_ProiectId",
                table: "DocumentDetalii",
                column: "Dimensiuni_ProiectId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_Dimensiuni_RepartitorId",
                table: "DocumentDetalii",
                column: "Dimensiuni_RepartitorId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_Dimensiuni_SursaFinantareId",
                table: "DocumentDetalii",
                column: "Dimensiuni_SursaFinantareId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentDetalii_Dimensiuni_UnitateId",
                table: "DocumentDetalii",
                column: "Dimensiuni_UnitateId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentDetalii_CoduriEconomice_Dimensiuni_CodEconomicId",
                table: "DocumentDetalii",
                column: "Dimensiuni_CodEconomicId",
                principalTable: "CoduriEconomice",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentDetalii_CoduriFunctionale_Dimensiuni_CodFunctionalId",
                table: "DocumentDetalii",
                column: "Dimensiuni_CodFunctionalId",
                principalTable: "CoduriFunctionale",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentDetalii_Produse_Dimensiuni_MaterialId",
                table: "DocumentDetalii",
                column: "Dimensiuni_MaterialId",
                principalTable: "Produse",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentDetalii_Proiecte_Dimensiuni_ProiectId",
                table: "DocumentDetalii",
                column: "Dimensiuni_ProiectId",
                principalTable: "Proiecte",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentDetalii_Repartitori_Dimensiuni_CentruCostId",
                table: "DocumentDetalii",
                column: "Dimensiuni_CentruCostId",
                principalTable: "Repartitori",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentDetalii_Repartitori_Dimensiuni_RepartitorId",
                table: "DocumentDetalii",
                column: "Dimensiuni_RepartitorId",
                principalTable: "Repartitori",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentDetalii_SurseFinantare_Dimensiuni_SursaFinantareId",
                table: "DocumentDetalii",
                column: "Dimensiuni_SursaFinantareId",
                principalTable: "SurseFinantare",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentDetalii_Unitati_Dimensiuni_UnitateId",
                table: "DocumentDetalii",
                column: "Dimensiuni_UnitateId",
                principalTable: "Unitati",
                principalColumn: "ID");
        }
    }
}
