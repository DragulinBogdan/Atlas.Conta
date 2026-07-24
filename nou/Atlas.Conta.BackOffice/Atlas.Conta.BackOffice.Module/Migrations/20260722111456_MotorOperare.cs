using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class MotorOperare : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_CoduriEconomice_Dimensiuni_CodEconomicId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_CoduriFunctionale_Dimensiuni_CodFunctional~",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Documente_DocumentId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Produse_Dimensiuni_MaterialId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Proiecte_Dimensiuni_ProiectId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Repartitori_Dimensiuni_CentruCostId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Repartitori_Dimensiuni_RepartitorId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Repartitori_RepartitorCreditId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Repartitori_RepartitorDebitId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_SurseFinantare_Dimensiuni_SursaFinantareId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Unitati_Dimensiuni_UnitateId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruStoc_Documente_DocumentId",
                table: "RegistruStoc");

            migrationBuilder.RenameColumn(
                name: "Dimensiuni_UnitateId",
                table: "RegistruContabil",
                newName: "DimensiuniDebit_UnitateId");

            migrationBuilder.RenameColumn(
                name: "Dimensiuni_SursaFinantareId",
                table: "RegistruContabil",
                newName: "DimensiuniDebit_SursaFinantareId");

            migrationBuilder.RenameColumn(
                name: "Dimensiuni_RepartitorId",
                table: "RegistruContabil",
                newName: "DimensiuniDebit_RepartitorId");

            migrationBuilder.RenameColumn(
                name: "Dimensiuni_ProiectId",
                table: "RegistruContabil",
                newName: "DimensiuniDebit_ProiectId");

            migrationBuilder.RenameColumn(
                name: "Dimensiuni_MaterialId",
                table: "RegistruContabil",
                newName: "DimensiuniDebit_MaterialId");

            migrationBuilder.RenameColumn(
                name: "Dimensiuni_CodFunctionalId",
                table: "RegistruContabil",
                newName: "DimensiuniDebit_CodFunctionalId");

            migrationBuilder.RenameColumn(
                name: "Dimensiuni_CodEconomicId",
                table: "RegistruContabil",
                newName: "DimensiuniDebit_CodEconomicId");

            migrationBuilder.RenameColumn(
                name: "Dimensiuni_CentruCostId",
                table: "RegistruContabil",
                newName: "DimensiuniDebit_CentruCostId");

            migrationBuilder.RenameColumn(
                name: "RepartitorDebitId",
                table: "RegistruContabil",
                newName: "DimensiuniCredit_UnitateId");

            migrationBuilder.RenameColumn(
                name: "RepartitorCreditId",
                table: "RegistruContabil",
                newName: "DimensiuniCredit_SursaFinantareId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistruContabil_RepartitorDebitId",
                table: "RegistruContabil",
                newName: "IX_RegistruContabil_DimensiuniCredit_UnitateId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistruContabil_RepartitorCreditId",
                table: "RegistruContabil",
                newName: "IX_RegistruContabil_DimensiuniCredit_SursaFinantareId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistruContabil_Dimensiuni_UnitateId",
                table: "RegistruContabil",
                newName: "IX_RegistruContabil_DimensiuniDebit_UnitateId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistruContabil_Dimensiuni_SursaFinantareId",
                table: "RegistruContabil",
                newName: "IX_RegistruContabil_DimensiuniDebit_SursaFinantareId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistruContabil_Dimensiuni_RepartitorId",
                table: "RegistruContabil",
                newName: "IX_RegistruContabil_DimensiuniDebit_RepartitorId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistruContabil_Dimensiuni_ProiectId",
                table: "RegistruContabil",
                newName: "IX_RegistruContabil_DimensiuniDebit_ProiectId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistruContabil_Dimensiuni_MaterialId",
                table: "RegistruContabil",
                newName: "IX_RegistruContabil_DimensiuniDebit_MaterialId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistruContabil_Dimensiuni_CodFunctionalId",
                table: "RegistruContabil",
                newName: "IX_RegistruContabil_DimensiuniDebit_CodFunctionalId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistruContabil_Dimensiuni_CodEconomicId",
                table: "RegistruContabil",
                newName: "IX_RegistruContabil_DimensiuniDebit_CodEconomicId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistruContabil_Dimensiuni_CentruCostId",
                table: "RegistruContabil",
                newName: "IX_RegistruContabil_DimensiuniDebit_CentruCostId");

            migrationBuilder.AlterColumn<Guid>(
                name: "DocumentId",
                table: "RegistruStoc",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<bool>(
                name: "Storno",
                table: "RegistruStoc",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "DocumentId",
                table: "RegistruContabil",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "DimensiuniCredit_CentruCostId",
                table: "RegistruContabil",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DimensiuniCredit_CodEconomicId",
                table: "RegistruContabil",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DimensiuniCredit_CodFunctionalId",
                table: "RegistruContabil",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DimensiuniCredit_MaterialId",
                table: "RegistruContabil",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DimensiuniCredit_ProiectId",
                table: "RegistruContabil",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DimensiuniCredit_RepartitorId",
                table: "RegistruContabil",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Storno",
                table: "RegistruContabil",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_DimensiuniCredit_CentruCostId",
                table: "RegistruContabil",
                column: "DimensiuniCredit_CentruCostId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_DimensiuniCredit_CodEconomicId",
                table: "RegistruContabil",
                column: "DimensiuniCredit_CodEconomicId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_DimensiuniCredit_CodFunctionalId",
                table: "RegistruContabil",
                column: "DimensiuniCredit_CodFunctionalId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_DimensiuniCredit_MaterialId",
                table: "RegistruContabil",
                column: "DimensiuniCredit_MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_DimensiuniCredit_ProiectId",
                table: "RegistruContabil",
                column: "DimensiuniCredit_ProiectId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_DimensiuniCredit_RepartitorId",
                table: "RegistruContabil",
                column: "DimensiuniCredit_RepartitorId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_CoduriEconomice_DimensiuniCredit_CodEconom~",
                table: "RegistruContabil",
                column: "DimensiuniCredit_CodEconomicId",
                principalTable: "CoduriEconomice",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_CoduriEconomice_DimensiuniDebit_CodEconomi~",
                table: "RegistruContabil",
                column: "DimensiuniDebit_CodEconomicId",
                principalTable: "CoduriEconomice",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_CoduriFunctionale_DimensiuniCredit_CodFunc~",
                table: "RegistruContabil",
                column: "DimensiuniCredit_CodFunctionalId",
                principalTable: "CoduriFunctionale",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_CoduriFunctionale_DimensiuniDebit_CodFunct~",
                table: "RegistruContabil",
                column: "DimensiuniDebit_CodFunctionalId",
                principalTable: "CoduriFunctionale",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Documente_DocumentId",
                table: "RegistruContabil",
                column: "DocumentId",
                principalTable: "Documente",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Produse_DimensiuniCredit_MaterialId",
                table: "RegistruContabil",
                column: "DimensiuniCredit_MaterialId",
                principalTable: "Produse",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Produse_DimensiuniDebit_MaterialId",
                table: "RegistruContabil",
                column: "DimensiuniDebit_MaterialId",
                principalTable: "Produse",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Proiecte_DimensiuniCredit_ProiectId",
                table: "RegistruContabil",
                column: "DimensiuniCredit_ProiectId",
                principalTable: "Proiecte",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Proiecte_DimensiuniDebit_ProiectId",
                table: "RegistruContabil",
                column: "DimensiuniDebit_ProiectId",
                principalTable: "Proiecte",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Repartitori_DimensiuniCredit_CentruCostId",
                table: "RegistruContabil",
                column: "DimensiuniCredit_CentruCostId",
                principalTable: "Repartitori",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Repartitori_DimensiuniCredit_RepartitorId",
                table: "RegistruContabil",
                column: "DimensiuniCredit_RepartitorId",
                principalTable: "Repartitori",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Repartitori_DimensiuniDebit_CentruCostId",
                table: "RegistruContabil",
                column: "DimensiuniDebit_CentruCostId",
                principalTable: "Repartitori",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Repartitori_DimensiuniDebit_RepartitorId",
                table: "RegistruContabil",
                column: "DimensiuniDebit_RepartitorId",
                principalTable: "Repartitori",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_SurseFinantare_DimensiuniCredit_SursaFinan~",
                table: "RegistruContabil",
                column: "DimensiuniCredit_SursaFinantareId",
                principalTable: "SurseFinantare",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_SurseFinantare_DimensiuniDebit_SursaFinant~",
                table: "RegistruContabil",
                column: "DimensiuniDebit_SursaFinantareId",
                principalTable: "SurseFinantare",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Unitati_DimensiuniCredit_UnitateId",
                table: "RegistruContabil",
                column: "DimensiuniCredit_UnitateId",
                principalTable: "Unitati",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Unitati_DimensiuniDebit_UnitateId",
                table: "RegistruContabil",
                column: "DimensiuniDebit_UnitateId",
                principalTable: "Unitati",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruStoc_Documente_DocumentId",
                table: "RegistruStoc",
                column: "DocumentId",
                principalTable: "Documente",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_CoduriEconomice_DimensiuniCredit_CodEconom~",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_CoduriEconomice_DimensiuniDebit_CodEconomi~",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_CoduriFunctionale_DimensiuniCredit_CodFunc~",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_CoduriFunctionale_DimensiuniDebit_CodFunct~",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Documente_DocumentId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Produse_DimensiuniCredit_MaterialId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Produse_DimensiuniDebit_MaterialId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Proiecte_DimensiuniCredit_ProiectId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Proiecte_DimensiuniDebit_ProiectId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Repartitori_DimensiuniCredit_CentruCostId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Repartitori_DimensiuniCredit_RepartitorId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Repartitori_DimensiuniDebit_CentruCostId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Repartitori_DimensiuniDebit_RepartitorId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_SurseFinantare_DimensiuniCredit_SursaFinan~",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_SurseFinantare_DimensiuniDebit_SursaFinant~",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Unitati_DimensiuniCredit_UnitateId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Unitati_DimensiuniDebit_UnitateId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruStoc_Documente_DocumentId",
                table: "RegistruStoc");

            migrationBuilder.DropIndex(
                name: "IX_RegistruContabil_DimensiuniCredit_CentruCostId",
                table: "RegistruContabil");

            migrationBuilder.DropIndex(
                name: "IX_RegistruContabil_DimensiuniCredit_CodEconomicId",
                table: "RegistruContabil");

            migrationBuilder.DropIndex(
                name: "IX_RegistruContabil_DimensiuniCredit_CodFunctionalId",
                table: "RegistruContabil");

            migrationBuilder.DropIndex(
                name: "IX_RegistruContabil_DimensiuniCredit_MaterialId",
                table: "RegistruContabil");

            migrationBuilder.DropIndex(
                name: "IX_RegistruContabil_DimensiuniCredit_ProiectId",
                table: "RegistruContabil");

            migrationBuilder.DropIndex(
                name: "IX_RegistruContabil_DimensiuniCredit_RepartitorId",
                table: "RegistruContabil");

            migrationBuilder.DropColumn(
                name: "Storno",
                table: "RegistruStoc");

            migrationBuilder.DropColumn(
                name: "DimensiuniCredit_CentruCostId",
                table: "RegistruContabil");

            migrationBuilder.DropColumn(
                name: "DimensiuniCredit_CodEconomicId",
                table: "RegistruContabil");

            migrationBuilder.DropColumn(
                name: "DimensiuniCredit_CodFunctionalId",
                table: "RegistruContabil");

            migrationBuilder.DropColumn(
                name: "DimensiuniCredit_MaterialId",
                table: "RegistruContabil");

            migrationBuilder.DropColumn(
                name: "DimensiuniCredit_ProiectId",
                table: "RegistruContabil");

            migrationBuilder.DropColumn(
                name: "DimensiuniCredit_RepartitorId",
                table: "RegistruContabil");

            migrationBuilder.DropColumn(
                name: "Storno",
                table: "RegistruContabil");

            migrationBuilder.RenameColumn(
                name: "DimensiuniDebit_UnitateId",
                table: "RegistruContabil",
                newName: "Dimensiuni_UnitateId");

            migrationBuilder.RenameColumn(
                name: "DimensiuniDebit_SursaFinantareId",
                table: "RegistruContabil",
                newName: "Dimensiuni_SursaFinantareId");

            migrationBuilder.RenameColumn(
                name: "DimensiuniDebit_RepartitorId",
                table: "RegistruContabil",
                newName: "Dimensiuni_RepartitorId");

            migrationBuilder.RenameColumn(
                name: "DimensiuniDebit_ProiectId",
                table: "RegistruContabil",
                newName: "Dimensiuni_ProiectId");

            migrationBuilder.RenameColumn(
                name: "DimensiuniDebit_MaterialId",
                table: "RegistruContabil",
                newName: "Dimensiuni_MaterialId");

            migrationBuilder.RenameColumn(
                name: "DimensiuniDebit_CodFunctionalId",
                table: "RegistruContabil",
                newName: "Dimensiuni_CodFunctionalId");

            migrationBuilder.RenameColumn(
                name: "DimensiuniDebit_CodEconomicId",
                table: "RegistruContabil",
                newName: "Dimensiuni_CodEconomicId");

            migrationBuilder.RenameColumn(
                name: "DimensiuniDebit_CentruCostId",
                table: "RegistruContabil",
                newName: "Dimensiuni_CentruCostId");

            migrationBuilder.RenameColumn(
                name: "DimensiuniCredit_UnitateId",
                table: "RegistruContabil",
                newName: "RepartitorDebitId");

            migrationBuilder.RenameColumn(
                name: "DimensiuniCredit_SursaFinantareId",
                table: "RegistruContabil",
                newName: "RepartitorCreditId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistruContabil_DimensiuniDebit_UnitateId",
                table: "RegistruContabil",
                newName: "IX_RegistruContabil_Dimensiuni_UnitateId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistruContabil_DimensiuniDebit_SursaFinantareId",
                table: "RegistruContabil",
                newName: "IX_RegistruContabil_Dimensiuni_SursaFinantareId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistruContabil_DimensiuniDebit_RepartitorId",
                table: "RegistruContabil",
                newName: "IX_RegistruContabil_Dimensiuni_RepartitorId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistruContabil_DimensiuniDebit_ProiectId",
                table: "RegistruContabil",
                newName: "IX_RegistruContabil_Dimensiuni_ProiectId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistruContabil_DimensiuniDebit_MaterialId",
                table: "RegistruContabil",
                newName: "IX_RegistruContabil_Dimensiuni_MaterialId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistruContabil_DimensiuniDebit_CodFunctionalId",
                table: "RegistruContabil",
                newName: "IX_RegistruContabil_Dimensiuni_CodFunctionalId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistruContabil_DimensiuniDebit_CodEconomicId",
                table: "RegistruContabil",
                newName: "IX_RegistruContabil_Dimensiuni_CodEconomicId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistruContabil_DimensiuniDebit_CentruCostId",
                table: "RegistruContabil",
                newName: "IX_RegistruContabil_Dimensiuni_CentruCostId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistruContabil_DimensiuniCredit_UnitateId",
                table: "RegistruContabil",
                newName: "IX_RegistruContabil_RepartitorDebitId");

            migrationBuilder.RenameIndex(
                name: "IX_RegistruContabil_DimensiuniCredit_SursaFinantareId",
                table: "RegistruContabil",
                newName: "IX_RegistruContabil_RepartitorCreditId");

            migrationBuilder.AlterColumn<Guid>(
                name: "DocumentId",
                table: "RegistruStoc",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "DocumentId",
                table: "RegistruContabil",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_CoduriEconomice_Dimensiuni_CodEconomicId",
                table: "RegistruContabil",
                column: "Dimensiuni_CodEconomicId",
                principalTable: "CoduriEconomice",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_CoduriFunctionale_Dimensiuni_CodFunctional~",
                table: "RegistruContabil",
                column: "Dimensiuni_CodFunctionalId",
                principalTable: "CoduriFunctionale",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Documente_DocumentId",
                table: "RegistruContabil",
                column: "DocumentId",
                principalTable: "Documente",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Produse_Dimensiuni_MaterialId",
                table: "RegistruContabil",
                column: "Dimensiuni_MaterialId",
                principalTable: "Produse",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Proiecte_Dimensiuni_ProiectId",
                table: "RegistruContabil",
                column: "Dimensiuni_ProiectId",
                principalTable: "Proiecte",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Repartitori_Dimensiuni_CentruCostId",
                table: "RegistruContabil",
                column: "Dimensiuni_CentruCostId",
                principalTable: "Repartitori",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Repartitori_Dimensiuni_RepartitorId",
                table: "RegistruContabil",
                column: "Dimensiuni_RepartitorId",
                principalTable: "Repartitori",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Repartitori_RepartitorCreditId",
                table: "RegistruContabil",
                column: "RepartitorCreditId",
                principalTable: "Repartitori",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Repartitori_RepartitorDebitId",
                table: "RegistruContabil",
                column: "RepartitorDebitId",
                principalTable: "Repartitori",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_SurseFinantare_Dimensiuni_SursaFinantareId",
                table: "RegistruContabil",
                column: "Dimensiuni_SursaFinantareId",
                principalTable: "SurseFinantare",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Unitati_Dimensiuni_UnitateId",
                table: "RegistruContabil",
                column: "Dimensiuni_UnitateId",
                principalTable: "Unitati",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruStoc_Documente_DocumentId",
                table: "RegistruStoc",
                column: "DocumentId",
                principalTable: "Documente",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
