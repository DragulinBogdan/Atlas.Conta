using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class CubDePostari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LaturaContPropriu",
                table: "TipuriDocument",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PosteazaInCub",
                table: "TipuriDocument",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "TolerantaTaxa",
                table: "PoliticiTva",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Pozitie",
                table: "DocumentDetalii",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Tranzactie",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: true),
                    Fel = table.Column<short>(type: "smallint", nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    ScrisLa = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tranzactie", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Tranzactie_Documente_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documente",
                        principalColumn: "ID");
                });

            // S-D2 — `Postare` NU se creează prin `CreateTable`: e PARTIȚIONATĂ LIST
            // pe `Spatiu`, cu cheia primară `(Spatiu, ID)` (Postgres cere cheia
            // partiției în orice constrângere unică) — formă pe care modelul EF n-o
            // poate declara (XAF EF Core nu suportă chei compuse). Coloanele sunt
            // exact cele pe care le-ar fi generat EF, ca snapshot-ul să rămână
            // coerent; divergența cheii e DECLARATĂ (S-r4) și probată de STR-SCHEMA.
            // FK-urile stau pe PARTIȚII (Postgres le cere pe partiție sau pe
            // părinte); `Gestiune` n-are FK cât timp gestiunile virtuale sunt
            // id-uri fără rând (S-r3), `Unitate` n-are FK pe Contabil (id de
            // partidă = hash determinist, nu rând), `Valuta` până la B-r6.
            migrationBuilder.Sql(@"
                CREATE TABLE ""Postare"" (
                    ""ID"" uuid NOT NULL,
                    ""Spatiu"" smallint NOT NULL,
                    ""TranzactieId"" uuid NOT NULL,
                    ""DocumentId"" uuid NULL,
                    ""LinieId"" uuid NULL,
                    ""Data"" date NOT NULL,
                    ""Cont"" uuid NOT NULL,
                    ""Latura"" smallint NOT NULL,
                    ""Partener"" uuid NULL,
                    ""Gestiune"" uuid NULL,
                    ""Produs"" uuid NULL,
                    ""Unitate"" uuid NULL,
                    ""UnitateDeschisa"" date NULL,
                    ""TipTvaId"" uuid NULL,
                    ""SensTva"" smallint NULL,
                    ""RolTva"" smallint NULL,
                    ""PerioadaDeclarare"" integer NULL,
                    ""Valuta"" uuid NULL,
                    ""Carte"" smallint NOT NULL,
                    ""CodFunctional"" uuid NULL,
                    ""CodEconomic"" uuid NULL,
                    ""SursaFinantare"" uuid NULL,
                    ""UnitateOrganizatorica"" uuid NULL,
                    ""Proiect"" uuid NULL,
                    ""CentruCost"" uuid NULL,
                    ""Atribuit"" uuid NULL,
                    ""Cantitate"" numeric(18,3) NOT NULL,
                    ""ValoareValuta"" numeric(18,2) NOT NULL,
                    ""Valoare"" numeric(18,2) NOT NULL,
                    CONSTRAINT ""PK_Postare"" PRIMARY KEY (""Spatiu"", ""ID"")
                ) PARTITION BY LIST (""Spatiu"");

                CREATE TABLE ""Postare_Contabil"" PARTITION OF ""Postare"" FOR VALUES IN (1);
                CREATE TABLE ""Postare_Stoc"" PARTITION OF ""Postare"" FOR VALUES IN (2);

                ALTER TABLE ""Postare_Contabil""
                    ADD CONSTRAINT ""FK_Postare_Contabil_Tranzactie_TranzactieId""
                        FOREIGN KEY (""TranzactieId"") REFERENCES ""Tranzactie"" (""ID""),
                    ADD CONSTRAINT ""FK_Postare_Contabil_Documente_DocumentId""
                        FOREIGN KEY (""DocumentId"") REFERENCES ""Documente"" (""ID""),
                    ADD CONSTRAINT ""FK_Postare_Contabil_Conturi_Cont""
                        FOREIGN KEY (""Cont"") REFERENCES ""Conturi"" (""ID""),
                    ADD CONSTRAINT ""FK_Postare_Contabil_Repartitori_Partener""
                        FOREIGN KEY (""Partener"") REFERENCES ""Repartitori"" (""ID""),
                    ADD CONSTRAINT ""FK_Postare_Contabil_Produse_Produs""
                        FOREIGN KEY (""Produs"") REFERENCES ""Produse"" (""ID"");

                ALTER TABLE ""Postare_Stoc""
                    ADD CONSTRAINT ""FK_Postare_Stoc_Tranzactie_TranzactieId""
                        FOREIGN KEY (""TranzactieId"") REFERENCES ""Tranzactie"" (""ID""),
                    ADD CONSTRAINT ""FK_Postare_Stoc_Documente_DocumentId""
                        FOREIGN KEY (""DocumentId"") REFERENCES ""Documente"" (""ID""),
                    ADD CONSTRAINT ""FK_Postare_Stoc_Conturi_Cont""
                        FOREIGN KEY (""Cont"") REFERENCES ""Conturi"" (""ID""),
                    ADD CONSTRAINT ""FK_Postare_Stoc_Repartitori_Partener""
                        FOREIGN KEY (""Partener"") REFERENCES ""Repartitori"" (""ID""),
                    ADD CONSTRAINT ""FK_Postare_Stoc_Produse_Produs""
                        FOREIGN KEY (""Produs"") REFERENCES ""Produse"" (""ID""),
                    ADD CONSTRAINT ""FK_Postare_Stoc_Loturi_Unitate""
                        FOREIGN KEY (""Unitate"") REFERENCES ""Loturi"" (""ID"");

                CREATE INDEX ""IX_Postare_Stoc_Produs_Data"" ON ""Postare_Stoc"" (""Produs"", ""Data"")
                    INCLUDE (""Cantitate"", ""Valoare"", ""Gestiune"", ""Unitate"");
                CREATE INDEX ""IX_Postare_Contabil_Partener_Cont_Data"" ON ""Postare_Contabil""
                    (""Partener"", ""Cont"", ""Data"") WHERE ""Partener"" IS NOT NULL;
                CREATE INDEX ""IX_Postare_Contabil_Data"" ON ""Postare_Contabil"" (""Data"");
                CREATE INDEX ""IX_Postare_Contabil_PerioadaDeclarare_TipTvaId"" ON ""Postare_Contabil""
                    (""PerioadaDeclarare"", ""TipTvaId"") WHERE ""PerioadaDeclarare"" IS NOT NULL;
            ");

            migrationBuilder.Sql(@"
                CREATE INDEX ""IX_Postare_DocumentId"" ON ""Postare"" (""DocumentId"");
                CREATE INDEX ""IX_Postare_TranzactieId"" ON ""Postare"" (""TranzactieId"");
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Tranzactie_DocumentId",
                table: "Tranzactie",
                column: "DocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Partițiile cad cu părintele.
            migrationBuilder.Sql(@"DROP TABLE ""Postare"";");

            migrationBuilder.DropTable(
                name: "Tranzactie");

            migrationBuilder.DropColumn(
                name: "LaturaContPropriu",
                table: "TipuriDocument");

            migrationBuilder.DropColumn(
                name: "PosteazaInCub",
                table: "TipuriDocument");

            migrationBuilder.DropColumn(
                name: "TolerantaTaxa",
                table: "PoliticiTva");

            migrationBuilder.DropColumn(
                name: "Pozitie",
                table: "DocumentDetalii");
        }
    }
}
