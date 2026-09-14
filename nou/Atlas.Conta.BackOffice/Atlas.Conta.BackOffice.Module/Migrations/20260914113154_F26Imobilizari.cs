using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class F26Imobilizari : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AmortizariLunare",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmortizariLunare", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AmortizariLunare_Documente_ID",
                        column: x => x.ID,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClasificariImobilizari",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DurataMinAni = table.Column<int>(type: "integer", nullable: true),
                    DurataMaxAni = table.Column<int>(type: "integer", nullable: true),
                    Grupa = table.Column<string>(type: "text", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Cod = table.Column<string>(type: "text", nullable: false),
                    Denumire = table.Column<string>(type: "text", nullable: false),
                    Cautare = table.Column<string>(type: "text", nullable: true, computedColumnSql: "translate(lower(coalesce(\"Cod\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClasificariImobilizari", x => x.ID);
                    table.CheckConstraint("CK_ClasificariImobilizari_Cod_negol", "btrim(\"Cod\") <> ''");
                    table.CheckConstraint("CK_ClasificariImobilizari_Denumire_negol", "btrim(\"Denumire\") <> ''");
                });

            migrationBuilder.CreateTable(
                name: "IesiriImobilizari",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Cauza = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IesiriImobilizari", x => x.ID);
                    table.ForeignKey(
                        name: "FK_IesiriImobilizari_Documente_ID",
                        column: x => x.ID,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PoliticiAmortizare",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    TipMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContAmortizareId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContCheltuialaAmortizareId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContCheltuialaCedareId = table.Column<Guid>(type: "uuid", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PoliticiAmortizare", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PoliticiAmortizare_Conturi_ContAmortizareId",
                        column: x => x.ContAmortizareId,
                        principalTable: "Conturi",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PoliticiAmortizare_Conturi_ContCheltuialaAmortizareId",
                        column: x => x.ContCheltuialaAmortizareId,
                        principalTable: "Conturi",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PoliticiAmortizare_Conturi_ContCheltuialaCedareId",
                        column: x => x.ContCheltuialaCedareId,
                        principalTable: "Conturi",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PoliticiAmortizare_TipuriMaterial_TipMaterialId",
                        column: x => x.TipMaterialId,
                        principalTable: "TipuriMaterial",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PuneriInFunctiune",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuneriInFunctiune", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PuneriInFunctiune_Documente_ID",
                        column: x => x.ID,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReguliDeductibilitate",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    DinSeed = table.Column<bool>(type: "boolean", nullable: false),
                    Categorie = table.Column<int>(type: "integer", nullable: false),
                    DoarNeexclusiv = table.Column<bool>(type: "boolean", nullable: false),
                    Fel = table.Column<int>(type: "integer", nullable: false),
                    Valoare = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DeLa = table.Column<DateOnly>(type: "date", nullable: false),
                    PanaLa = table.Column<DateOnly>(type: "date", nullable: true),
                    Temei = table.Column<string>(type: "text", nullable: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReguliDeductibilitate", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Imobilizari",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    NumarInventar = table.Column<string>(type: "text", nullable: false),
                    Denumire = table.Column<string>(type: "text", nullable: false),
                    TipMaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClasificareId = table.Column<Guid>(type: "uuid", nullable: true),
                    LocId = table.Column<Guid>(type: "uuid", nullable: false),
                    CentruCostId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResponsabilId = table.Column<Guid>(type: "uuid", nullable: true),
                    Stare = table.Column<int>(type: "integer", nullable: false),
                    DataPunereInFunctiune = table.Column<DateOnly>(type: "date", nullable: true),
                    DataIesire = table.Column<DateOnly>(type: "date", nullable: true),
                    Cautare = table.Column<string>(type: "text", nullable: true, computedColumnSql: "translate(lower(coalesce(\"NumarInventar\", '') || ' ' || coalesce(\"Denumire\", '')), 'ăâîșşțţéèêëáàäöüçñ', 'aaisstteeeeaaaoucn')", stored: true),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Imobilizari", x => x.ID);
                    table.CheckConstraint("CK_Imobilizari_Denumire_negol", "btrim(\"Denumire\") <> ''");
                    table.CheckConstraint("CK_Imobilizari_NumarInventar_negol", "btrim(\"NumarInventar\") <> ''");
                    table.ForeignKey(
                        name: "FK_Imobilizari_Angajati_ResponsabilId",
                        column: x => x.ResponsabilId,
                        principalTable: "Angajati",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Imobilizari_ClasificariImobilizari_ClasificareId",
                        column: x => x.ClasificareId,
                        principalTable: "ClasificariImobilizari",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Imobilizari_Repartitori_CentruCostId",
                        column: x => x.CentruCostId,
                        principalTable: "Repartitori",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Imobilizari_Repartitori_LocId",
                        column: x => x.LocId,
                        principalTable: "Repartitori",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Imobilizari_TipuriMaterial_TipMaterialId",
                        column: x => x.TipMaterialId,
                        principalTable: "TipuriMaterial",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AmortizariLunareDetalii",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    ImobilizareId = table.Column<Guid>(type: "uuid", nullable: false),
                    ValoareFiscala = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValoareDeductibila = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ContDebitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContCreditId = table.Column<Guid>(type: "uuid", nullable: true),
                    RepartitorDebitId = table.Column<Guid>(type: "uuid", nullable: true),
                    RepartitorCreditId = table.Column<Guid>(type: "uuid", nullable: true),
                    CentruCostId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AmortizariLunareDetalii", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AmortizariLunareDetalii_Conturi_ContCreditId",
                        column: x => x.ContCreditId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_AmortizariLunareDetalii_Conturi_ContDebitId",
                        column: x => x.ContDebitId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_AmortizariLunareDetalii_DocumentDetalii_ID",
                        column: x => x.ID,
                        principalTable: "DocumentDetalii",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AmortizariLunareDetalii_Imobilizari_ImobilizareId",
                        column: x => x.ImobilizareId,
                        principalTable: "Imobilizari",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AmortizariLunareDetalii_Repartitori_CentruCostId",
                        column: x => x.CentruCostId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_AmortizariLunareDetalii_Repartitori_RepartitorCreditId",
                        column: x => x.RepartitorCreditId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_AmortizariLunareDetalii_Repartitori_RepartitorDebitId",
                        column: x => x.RepartitorDebitId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "IesiriImobilizariDetalii",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    ImobilizareId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fel = table.Column<int>(type: "integer", nullable: false),
                    ContDebitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ContCreditId = table.Column<Guid>(type: "uuid", nullable: true),
                    RepartitorDebitId = table.Column<Guid>(type: "uuid", nullable: true),
                    RepartitorCreditId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IesiriImobilizariDetalii", x => x.ID);
                    table.ForeignKey(
                        name: "FK_IesiriImobilizariDetalii_Conturi_ContCreditId",
                        column: x => x.ContCreditId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_IesiriImobilizariDetalii_Conturi_ContDebitId",
                        column: x => x.ContDebitId,
                        principalTable: "Conturi",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_IesiriImobilizariDetalii_DocumentDetalii_ID",
                        column: x => x.ID,
                        principalTable: "DocumentDetalii",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IesiriImobilizariDetalii_Imobilizari_ImobilizareId",
                        column: x => x.ImobilizareId,
                        principalTable: "Imobilizari",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IesiriImobilizariDetalii_Repartitori_RepartitorCreditId",
                        column: x => x.RepartitorCreditId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_IesiriImobilizariDetalii_Repartitori_RepartitorDebitId",
                        column: x => x.RepartitorDebitId,
                        principalTable: "Repartitori",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "PuneriInFunctiuneDetalii",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    ImobilizareId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fel = table.Column<int>(type: "integer", nullable: false),
                    LinieSursaId = table.Column<Guid>(type: "uuid", nullable: true),
                    ValoareFiscala = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AmortizareInitiala = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AmortizareFiscalaInitiala = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    LuniAmortizateInitial = table.Column<int>(type: "integer", nullable: false),
                    Metoda = table.Column<int>(type: "integer", nullable: true),
                    DurataLuni = table.Column<int>(type: "integer", nullable: true),
                    ValoareReziduala = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MetodaFiscala = table.Column<int>(type: "integer", nullable: true),
                    DurataFiscalaLuni = table.Column<int>(type: "integer", nullable: true),
                    CategorieFiscala = table.Column<int>(type: "integer", nullable: true),
                    UtilizareExclusiva = table.Column<bool>(type: "boolean", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PuneriInFunctiuneDetalii", x => x.ID);
                    table.ForeignKey(
                        name: "FK_PuneriInFunctiuneDetalii_DocumentDetalii_ID",
                        column: x => x.ID,
                        principalTable: "DocumentDetalii",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PuneriInFunctiuneDetalii_DocumentDetalii_LinieSursaId",
                        column: x => x.LinieSursaId,
                        principalTable: "DocumentDetalii",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PuneriInFunctiuneDetalii_Imobilizari_ImobilizareId",
                        column: x => x.ImobilizareId,
                        principalTable: "Imobilizari",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegistruImobilizari",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Data = table.Column<DateOnly>(type: "date", nullable: false),
                    ImobilizareId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fel = table.Column<int>(type: "integer", nullable: false),
                    Valoare = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ValoareFiscala = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Amortizare = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AmortizareFiscala = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AmortizareDeductibila = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Luni = table.Column<int>(type: "integer", nullable: false),
                    Metoda = table.Column<int>(type: "integer", nullable: true),
                    DurataLuni = table.Column<int>(type: "integer", nullable: true),
                    ValoareReziduala = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    MetodaFiscala = table.Column<int>(type: "integer", nullable: true),
                    DurataFiscalaLuni = table.Column<int>(type: "integer", nullable: true),
                    CategorieFiscala = table.Column<int>(type: "integer", nullable: true),
                    UtilizareExclusiva = table.Column<bool>(type: "boolean", nullable: true),
                    RepartitorId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DetaliuId = table.Column<Guid>(type: "uuid", nullable: false),
                    Storno = table.Column<bool>(type: "boolean", nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistruImobilizari", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RegistruImobilizari_DocumentDetalii_DetaliuId",
                        column: x => x.DetaliuId,
                        principalTable: "DocumentDetalii",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistruImobilizari_Documente_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Documente",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistruImobilizari_Imobilizari_ImobilizareId",
                        column: x => x.ImobilizareId,
                        principalTable: "Imobilizari",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RegistruImobilizari_Repartitori_RepartitorId",
                        column: x => x.RepartitorId,
                        principalTable: "Repartitori",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AmortizariLunareDetalii_CentruCostId",
                table: "AmortizariLunareDetalii",
                column: "CentruCostId");

            migrationBuilder.CreateIndex(
                name: "IX_AmortizariLunareDetalii_ContCreditId",
                table: "AmortizariLunareDetalii",
                column: "ContCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_AmortizariLunareDetalii_ContDebitId",
                table: "AmortizariLunareDetalii",
                column: "ContDebitId");

            migrationBuilder.CreateIndex(
                name: "IX_AmortizariLunareDetalii_ImobilizareId",
                table: "AmortizariLunareDetalii",
                column: "ImobilizareId");

            migrationBuilder.CreateIndex(
                name: "IX_AmortizariLunareDetalii_RepartitorCreditId",
                table: "AmortizariLunareDetalii",
                column: "RepartitorCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_AmortizariLunareDetalii_RepartitorDebitId",
                table: "AmortizariLunareDetalii",
                column: "RepartitorDebitId");

            migrationBuilder.CreateIndex(
                name: "IX_ClasificariImobilizari_Cod",
                table: "ClasificariImobilizari",
                column: "Cod",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_IesiriImobilizariDetalii_ContCreditId",
                table: "IesiriImobilizariDetalii",
                column: "ContCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_IesiriImobilizariDetalii_ContDebitId",
                table: "IesiriImobilizariDetalii",
                column: "ContDebitId");

            migrationBuilder.CreateIndex(
                name: "IX_IesiriImobilizariDetalii_ImobilizareId",
                table: "IesiriImobilizariDetalii",
                column: "ImobilizareId");

            migrationBuilder.CreateIndex(
                name: "IX_IesiriImobilizariDetalii_RepartitorCreditId",
                table: "IesiriImobilizariDetalii",
                column: "RepartitorCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_IesiriImobilizariDetalii_RepartitorDebitId",
                table: "IesiriImobilizariDetalii",
                column: "RepartitorDebitId");

            migrationBuilder.CreateIndex(
                name: "IX_Imobilizari_CentruCostId",
                table: "Imobilizari",
                column: "CentruCostId");

            migrationBuilder.CreateIndex(
                name: "IX_Imobilizari_ClasificareId",
                table: "Imobilizari",
                column: "ClasificareId");

            migrationBuilder.CreateIndex(
                name: "IX_Imobilizari_LocId",
                table: "Imobilizari",
                column: "LocId");

            migrationBuilder.CreateIndex(
                name: "IX_Imobilizari_NumarInventar",
                table: "Imobilizari",
                column: "NumarInventar",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Imobilizari_ResponsabilId",
                table: "Imobilizari",
                column: "ResponsabilId");

            migrationBuilder.CreateIndex(
                name: "IX_Imobilizari_TipMaterialId",
                table: "Imobilizari",
                column: "TipMaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiAmortizare_ContAmortizareId",
                table: "PoliticiAmortizare",
                column: "ContAmortizareId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiAmortizare_ContCheltuialaAmortizareId",
                table: "PoliticiAmortizare",
                column: "ContCheltuialaAmortizareId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiAmortizare_ContCheltuialaCedareId",
                table: "PoliticiAmortizare",
                column: "ContCheltuialaCedareId");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiAmortizare_TipMaterialId",
                table: "PoliticiAmortizare",
                column: "TipMaterialId",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PuneriInFunctiuneDetalii_ImobilizareId",
                table: "PuneriInFunctiuneDetalii",
                column: "ImobilizareId");

            migrationBuilder.CreateIndex(
                name: "IX_PuneriInFunctiuneDetalii_LinieSursaId",
                table: "PuneriInFunctiuneDetalii",
                column: "LinieSursaId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruImobilizari_DetaliuId",
                table: "RegistruImobilizari",
                column: "DetaliuId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruImobilizari_DocumentId",
                table: "RegistruImobilizari",
                column: "DocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruImobilizari_ImobilizareId",
                table: "RegistruImobilizari",
                column: "ImobilizareId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruImobilizari_RepartitorId",
                table: "RegistruImobilizari",
                column: "RepartitorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AmortizariLunare");

            migrationBuilder.DropTable(
                name: "AmortizariLunareDetalii");

            migrationBuilder.DropTable(
                name: "IesiriImobilizari");

            migrationBuilder.DropTable(
                name: "IesiriImobilizariDetalii");

            migrationBuilder.DropTable(
                name: "PoliticiAmortizare");

            migrationBuilder.DropTable(
                name: "PuneriInFunctiune");

            migrationBuilder.DropTable(
                name: "PuneriInFunctiuneDetalii");

            migrationBuilder.DropTable(
                name: "RegistruImobilizari");

            migrationBuilder.DropTable(
                name: "ReguliDeductibilitate");

            migrationBuilder.DropTable(
                name: "Imobilizari");

            migrationBuilder.DropTable(
                name: "ClasificariImobilizari");
        }
    }
}
