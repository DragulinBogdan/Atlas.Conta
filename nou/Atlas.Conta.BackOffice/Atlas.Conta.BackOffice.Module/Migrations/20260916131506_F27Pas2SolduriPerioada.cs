using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class F27Pas2SolduriPerioada : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SolduriPerioadaContabil",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    An = table.Column<int>(type: "integer", nullable: false),
                    Luna = table.Column<int>(type: "integer", nullable: false),
                    ContId = table.Column<Guid>(type: "uuid", nullable: false),
                    RepartitorId = table.Column<Guid>(type: "uuid", nullable: true),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: true),
                    CodFunctionalId = table.Column<Guid>(type: "uuid", nullable: true),
                    CodEconomicId = table.Column<Guid>(type: "uuid", nullable: true),
                    SursaFinantareId = table.Column<Guid>(type: "uuid", nullable: true),
                    UnitateId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProiectId = table.Column<Guid>(type: "uuid", nullable: true),
                    CentruCostId = table.Column<Guid>(type: "uuid", nullable: true),
                    Debit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Credit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolduriPerioadaContabil", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SolduriPerioadaContabil_CoduriEconomice_CodEconomicId",
                        column: x => x.CodEconomicId,
                        principalTable: "CoduriEconomice",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolduriPerioadaContabil_CoduriFunctionale_CodFunctionalId",
                        column: x => x.CodFunctionalId,
                        principalTable: "CoduriFunctionale",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolduriPerioadaContabil_Conturi_ContId",
                        column: x => x.ContId,
                        principalTable: "Conturi",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolduriPerioadaContabil_Produse_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Produse",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolduriPerioadaContabil_Proiecte_ProiectId",
                        column: x => x.ProiectId,
                        principalTable: "Proiecte",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolduriPerioadaContabil_Repartitori_CentruCostId",
                        column: x => x.CentruCostId,
                        principalTable: "Repartitori",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolduriPerioadaContabil_Repartitori_RepartitorId",
                        column: x => x.RepartitorId,
                        principalTable: "Repartitori",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolduriPerioadaContabil_SurseFinantare_SursaFinantareId",
                        column: x => x.SursaFinantareId,
                        principalTable: "SurseFinantare",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolduriPerioadaContabil_Unitati_UnitateId",
                        column: x => x.UnitateId,
                        principalTable: "Unitati",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SolduriPerioadaStoc",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    An = table.Column<int>(type: "integer", nullable: false),
                    Luna = table.Column<int>(type: "integer", nullable: false),
                    LotId = table.Column<Guid>(type: "uuid", nullable: false),
                    RepartitorId = table.Column<Guid>(type: "uuid", nullable: false),
                    TipStoc = table.Column<int>(type: "integer", nullable: false),
                    Cantitate = table.Column<decimal>(type: "numeric(18,3)", precision: 18, scale: 3, nullable: false),
                    Valoare = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    GCRecord = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolduriPerioadaStoc", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SolduriPerioadaStoc_Loturi_LotId",
                        column: x => x.LotId,
                        principalTable: "Loturi",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SolduriPerioadaStoc_Repartitori_RepartitorId",
                        column: x => x.RepartitorId,
                        principalTable: "Repartitori",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegistruStoc_Data",
                table: "RegistruStoc",
                column: "Data",
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_Data",
                table: "RegistruContabil",
                column: "Data",
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_An_Luna",
                table: "SolduriPerioadaContabil",
                columns: new[] { "An", "Luna" });

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_An_Luna_ContId_RepartitorId_Materia~",
                table: "SolduriPerioadaContabil",
                columns: new[] { "An", "Luna", "ContId", "RepartitorId", "MaterialId", "CodFunctionalId", "CodEconomicId", "SursaFinantareId", "UnitateId", "ProiectId", "CentruCostId" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_CentruCostId",
                table: "SolduriPerioadaContabil",
                column: "CentruCostId");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_CodEconomicId",
                table: "SolduriPerioadaContabil",
                column: "CodEconomicId");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_CodFunctionalId",
                table: "SolduriPerioadaContabil",
                column: "CodFunctionalId");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_ContId",
                table: "SolduriPerioadaContabil",
                column: "ContId");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_MaterialId",
                table: "SolduriPerioadaContabil",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_ProiectId",
                table: "SolduriPerioadaContabil",
                column: "ProiectId");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_RepartitorId",
                table: "SolduriPerioadaContabil",
                column: "RepartitorId");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_SursaFinantareId",
                table: "SolduriPerioadaContabil",
                column: "SursaFinantareId");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaContabil_UnitateId",
                table: "SolduriPerioadaContabil",
                column: "UnitateId");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaStoc_An_Luna",
                table: "SolduriPerioadaStoc",
                columns: new[] { "An", "Luna" });

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaStoc_An_Luna_LotId_RepartitorId_TipStoc",
                table: "SolduriPerioadaStoc",
                columns: new[] { "An", "Luna", "LotId", "RepartitorId", "TipStoc" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaStoc_LotId",
                table: "SolduriPerioadaStoc",
                column: "LotId");

            migrationBuilder.CreateIndex(
                name: "IX_SolduriPerioadaStoc_RepartitorId",
                table: "SolduriPerioadaStoc",
                column: "RepartitorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SolduriPerioadaContabil");

            migrationBuilder.DropTable(
                name: "SolduriPerioadaStoc");

            migrationBuilder.DropIndex(
                name: "IX_RegistruStoc_Data",
                table: "RegistruStoc");

            migrationBuilder.DropIndex(
                name: "IX_RegistruContabil_Data",
                table: "RegistruContabil");
        }
    }
}
