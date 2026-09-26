using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class C104EntitatiProprii : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentDetalii_DocumentDetalii_LinieSursaId",
                table: "DocumentDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentDetalii_Imobilizari_ImobilizareId",
                table: "DocumentDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentDetalii_TipuriMaterial_TipMaterialId",
                table: "DocumentDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_Documente_Documente_CorecteazaId",
                table: "Documente");

            migrationBuilder.DropForeignKey(
                name: "FK_Documente_Documente_LaturaPerecheId",
                table: "Documente");

            migrationBuilder.DropForeignKey(
                name: "FK_Documente_Repartitori_PredatorId",
                table: "Documente");

            migrationBuilder.DropForeignKey(
                name: "FK_Documente_Repartitori_PrimitorId",
                table: "Documente");

            migrationBuilder.DropForeignKey(
                name: "FK_DviFacturi_Documente_DviId",
                table: "DviFacturi");

            migrationBuilder.DropForeignKey(
                name: "FK_DviFacturi_Documente_FacturaId",
                table: "DviFacturi");

            migrationBuilder.DropForeignKey(
                name: "FK_Imobilizari_ClasificariImobilizari_ClasificareId",
                table: "Imobilizari");

            migrationBuilder.DropForeignKey(
                name: "FK_Imobilizari_CoduriEconomice_CodEconomicId",
                table: "Imobilizari");

            migrationBuilder.DropForeignKey(
                name: "FK_Imobilizari_Repartitori_CentruCostId",
                table: "Imobilizari");

            migrationBuilder.DropForeignKey(
                name: "FK_Imobilizari_Repartitori_LocId",
                table: "Imobilizari");

            migrationBuilder.DropForeignKey(
                name: "FK_Imobilizari_Repartitori_ResponsabilId",
                table: "Imobilizari");

            migrationBuilder.DropForeignKey(
                name: "FK_Imobilizari_TipuriMaterial_TipMaterialId",
                table: "Imobilizari");

            migrationBuilder.DropForeignKey(
                name: "FK_Imperecheri_Documente_DocumentId",
                table: "Imperecheri");

            migrationBuilder.DropForeignKey(
                name: "FK_Imperecheri_Documente_DocumentStingatorId",
                table: "Imperecheri");

            migrationBuilder.DropForeignKey(
                name: "FK_Imperecheri_Imperecheri_InverseazaId",
                table: "Imperecheri");

            migrationBuilder.DropForeignKey(
                name: "FK_InchideriPerioade_PerioadeFiscale_PerioadaId",
                table: "InchideriPerioade");

            migrationBuilder.DropForeignKey(
                name: "FK_Loturi_Produse_ProdusId",
                table: "Loturi");

            migrationBuilder.DropForeignKey(
                name: "FK_Loturi_Repartitori_GestiuneId",
                table: "Loturi");

            migrationBuilder.DropForeignKey(
                name: "FK_MapariD300_RanduriD300_RandId",
                table: "MapariD300");

            migrationBuilder.DropForeignKey(
                name: "FK_MapariD300_TipuriTva_TipTvaId",
                table: "MapariD300");

            migrationBuilder.DropForeignKey(
                name: "FK_MapariD394_TipuriTva_TipTvaId",
                table: "MapariD394");

            migrationBuilder.DropForeignKey(
                name: "FK_MapariTvaSaft_TipuriTva_TipTvaId",
                table: "MapariTvaSaft");

            migrationBuilder.DropForeignKey(
                name: "FK_PartideDeschise_Documente_DocumentId",
                table: "PartideDeschise");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiAmortizare_Conturi_ContAmortizareId",
                table: "PoliticiAmortizare");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiAmortizare_Conturi_ContCheltuialaAmortizareId",
                table: "PoliticiAmortizare");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiAmortizare_Conturi_ContCheltuialaCedareId",
                table: "PoliticiAmortizare");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiAmortizare_TipuriMaterial_TipMaterialId",
                table: "PoliticiAmortizare");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiConex_TipuriDocument_TipDocumentSursaId",
                table: "PoliticiConex");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiConex_TipuriDocument_TipDocumentTintaId",
                table: "PoliticiConex");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiDiferenta_ClaseProduse_ClasaId",
                table: "PoliticiDiferenta");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiDiferenta_Conturi_ContId",
                table: "PoliticiDiferenta");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiDiferenta_TipuriDocument_TipDocumentId",
                table: "PoliticiDiferenta");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiInchidereTva_TipuriDocument_TipDocumentId",
                table: "PoliticiInchidereTva");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiMiscareSaft_TipuriDocument_TipDocumentId",
                table: "PoliticiMiscareSaft");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiNumerotare_TipuriDocument_TipDocumentId",
                table: "PoliticiNumerotare");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiScadenta_TipuriDocument_TipDocumentId",
                table: "PoliticiScadenta");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiTva_TipuriDocument_TipDocumentId",
                table: "PoliticiTva");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiTvaImplicit_TipuriDocument_TipDocumentId",
                table: "PoliticiTvaImplicit");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiTvaImplicit_TipuriTva_TipTvaId",
                table: "PoliticiTvaImplicit");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiValidare_TipuriDocument_TipDocumentId",
                table: "PoliticiValidare");

            migrationBuilder.DropForeignKey(
                name: "FK_Produse_UnitatiMasura_UnitateMasuraId",
                table: "Produse");

            migrationBuilder.DropForeignKey(
                name: "FK_RanduriD300_RanduriD300_OglindaAId",
                table: "RanduriD300");

            migrationBuilder.DropForeignKey(
                name: "FK_RanduriD300_RanduriD300_ParinteId",
                table: "RanduriD300");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Conturi_ContCreditId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Conturi_ContDebitId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruImobilizari_DocumentDetalii_DetaliuId",
                table: "RegistruImobilizari");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruImobilizari_Documente_DocumentId",
                table: "RegistruImobilizari");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruImobilizari_Imobilizari_ImobilizareId",
                table: "RegistruImobilizari");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruImobilizari_Repartitori_RepartitorId",
                table: "RegistruImobilizari");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruStoc_Loturi_LotId",
                table: "RegistruStoc");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruStoc_Repartitori_RepartitorId",
                table: "RegistruStoc");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruTva_DocumentDetalii_DetaliuId",
                table: "RegistruTva");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruTva_Documente_DocumentId",
                table: "RegistruTva");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruTva_TipuriTva_TipTvaId",
                table: "RegistruTva");

            migrationBuilder.DropForeignKey(
                name: "FK_ReguliContare_TipuriDocument_TipDocumentId",
                table: "ReguliContare");

            migrationBuilder.DropForeignKey(
                name: "FK_ReguliStoc_TipuriDocument_TipDocumentId",
                table: "ReguliStoc");

            migrationBuilder.DropForeignKey(
                name: "FK_Repartitori_Judete_JudetId",
                table: "Repartitori");

            migrationBuilder.DropForeignKey(
                name: "FK_Societati_Judete_JudetId",
                table: "Societati");

            migrationBuilder.DropForeignKey(
                name: "FK_Societati_Repartitori_ContBancarId",
                table: "Societati");

            migrationBuilder.DropForeignKey(
                name: "FK_SolduriPerioadaContabil_CoduriEconomice_CodEconomicId",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_SolduriPerioadaContabil_CoduriFunctionale_CodFunctionalId",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_SolduriPerioadaContabil_Conturi_ContId",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_SolduriPerioadaContabil_Produse_MaterialId",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_SolduriPerioadaContabil_Proiecte_ProiectId",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_SolduriPerioadaContabil_Repartitori_CentruCostId",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_SolduriPerioadaContabil_Repartitori_RepartitorId",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_SolduriPerioadaContabil_SurseFinantare_SursaFinantareId",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_SolduriPerioadaContabil_Unitati_UnitateId",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_SolduriPerioadaStoc_Loturi_LotId",
                table: "SolduriPerioadaStoc");

            migrationBuilder.DropForeignKey(
                name: "FK_TipuriMaterial_ClaseProduse_ClasaId",
                table: "TipuriMaterial");

            migrationBuilder.DropIndex(
                name: "IX_UnitatiMasura_Cod",
                table: "UnitatiMasura");

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
                name: "IX_RegistruTva_PerioadaAn_PerioadaLuna",
                table: "RegistruTva");

            migrationBuilder.DropIndex(
                name: "IX_RegistruStoc_Data",
                table: "RegistruStoc");

            migrationBuilder.DropIndex(
                name: "IX_RegistruContabil_Data",
                table: "RegistruContabil");

            migrationBuilder.DropIndex(
                name: "IX_RanduriD300_Cod",
                table: "RanduriD300");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiValidare_TipDocumentId",
                table: "PoliticiValidare");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiTvaImplicit_TipDocumentId_ClasaFiscala_ValabilDeLa",
                table: "PoliticiTvaImplicit");

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
                name: "IX_PoliticiMiscareSaft_TipDocumentId_TipStoc",
                table: "PoliticiMiscareSaft");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiMiscareSaft_TipDocumentId_TipStoc_Semn",
                table: "PoliticiMiscareSaft");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiInchidereTva_TipDocumentId",
                table: "PoliticiInchidereTva");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiInchidere_Fel",
                table: "PoliticiInchidere");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiDiferenta_TipDocumentId_Cauza_ClasaId",
                table: "PoliticiDiferenta");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiConex_TipDocumentSursaId",
                table: "PoliticiConex");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiAmortizare_TipMaterialId",
                table: "PoliticiAmortizare");

            migrationBuilder.DropIndex(
                name: "IX_PerioadeFiscale_An_Luna",
                table: "PerioadeFiscale");

            migrationBuilder.DropIndex(
                name: "IX_MapariTvaSaft_Versiune_Sectiune_TipTvaId_Regim_Cota_DeImpor~",
                table: "MapariTvaSaft");

            migrationBuilder.DropIndex(
                name: "IX_MapariD394_TipTvaId_Sens",
                table: "MapariD394");

            migrationBuilder.DropIndex(
                name: "IX_MapariD300_TipTvaId_Sens_RandId",
                table: "MapariD300");

            migrationBuilder.DropIndex(
                name: "IX_Judete_Cod",
                table: "Judete");

            migrationBuilder.DropIndex(
                name: "IX_Imperecheri_Data",
                table: "Imperecheri");

            migrationBuilder.DropIndex(
                name: "IX_Imperecheri_InverseazaId",
                table: "Imperecheri");

            migrationBuilder.DropIndex(
                name: "IX_Imobilizari_NumarInventar",
                table: "Imobilizari");

            migrationBuilder.DropIndex(
                name: "IX_DviFacturi_DviId_FacturaId",
                table: "DviFacturi");

            migrationBuilder.DropIndex(
                name: "IX_Documente_CorecteazaId",
                table: "Documente");

            migrationBuilder.DropIndex(
                name: "IX_Documente_DataInregistrare",
                table: "Documente");

            migrationBuilder.DropIndex(
                name: "IX_Conturi_Simbol",
                table: "Conturi");

            migrationBuilder.DropIndex(
                name: "IX_ClasificariImobilizari_Cod",
                table: "ClasificariImobilizari");

            migrationBuilder.DropIndex(
                name: "IX_ClaseProduse_Cod",
                table: "ClaseProduse");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "UnitatiMasura");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "Unitati");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "TipuriTva");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "TipuriMaterial");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "TipuriDocument");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "SurseFinantare");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "StateMachineTransitions");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "StateMachineStates");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "StateMachineAppearances");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "SolduriPerioadaStoc");

            migrationBuilder.DropColumn(
                name: "OptimisticLockField",
                table: "SolduriPerioadaStoc");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropColumn(
                name: "OptimisticLockField",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "Societati");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "SetariProfil");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "ReportDataV2");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "Repartitori");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "ReguliStoc");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "ReguliDeductibilitate");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "ReguliContare");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "RegistruTva");

            migrationBuilder.DropColumn(
                name: "OptimisticLockField",
                table: "RegistruTva");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "RegistruStoc");

            migrationBuilder.DropColumn(
                name: "OptimisticLockField",
                table: "RegistruStoc");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "RegistruImobilizari");

            migrationBuilder.DropColumn(
                name: "OptimisticLockField",
                table: "RegistruImobilizari");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "RegistruContabil");

            migrationBuilder.DropColumn(
                name: "OptimisticLockField",
                table: "RegistruContabil");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "RanduriD300");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "Proiecte");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "Produse");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "PoliticiValidare");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "PoliticiTvaImplicit");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "PoliticiTva");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "PoliticiScadenta");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "PoliticiNumerotare");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "PoliticiMiscareSaft");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "PoliticiInchidereTva");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "PoliticiInchidere");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "PoliticiDiferenta");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "PoliticiConex");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "PoliticiAmortizare");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "PermissionPolicyUserLoginInfo");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "PermissionPolicyUser");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "PermissionPolicyTypePermissionObject");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "PermissionPolicyRoleBase");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "PermissionPolicyObjectPermissionsObject");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "PermissionPolicyNavigationPermissionObject");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "PermissionPolicyMemberPermissionsObject");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "PermissionPolicyActionPermissionObject");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "PerioadeFiscale");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "PartideDeschise");

            migrationBuilder.DropColumn(
                name: "OptimisticLockField",
                table: "PartideDeschise");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "ModelDifferences");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "ModelDifferenceAspects");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "MigrareLegaturi");

            migrationBuilder.DropColumn(
                name: "OptimisticLockField",
                table: "MigrareLegaturi");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "MapariTvaSaft");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "MapariD394");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "MapariD300");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "Loturi");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "Judete");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "InchideriPerioade");

            migrationBuilder.DropColumn(
                name: "OptimisticLockField",
                table: "InchideriPerioade");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "Imperecheri");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "Imobilizari");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "HCategories");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "FileData");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "DviFacturi");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "Documente");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "DocumentDetalii");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "DepuneriDeclaratii");

            migrationBuilder.DropColumn(
                name: "OptimisticLockField",
                table: "DepuneriDeclaratii");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "DashboardData");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "Conturi");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "CoduriFunctionale");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "CoduriEconomice");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "ClasificariImobilizari");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "ClaseProduse");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "AuditEFCoreWeakReferences");

            migrationBuilder.DropColumn(
                name: "GCRecord",
                table: "Angajamente");

            migrationBuilder.AddColumn<bool>(
                name: "Activ",
                table: "UnitatiMasura",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activ",
                table: "Unitati",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "Activ",
                table: "TipuriTva",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "Activ",
                table: "TipuriMaterial",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activ",
                table: "SurseFinantare",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activ",
                table: "RanduriD300",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activ",
                table: "Proiecte",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activ",
                table: "Produse",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activ",
                table: "Judete",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activ",
                table: "Imobilizari",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activ",
                table: "Conturi",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activ",
                table: "CoduriFunctionale",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activ",
                table: "CoduriEconomice",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activ",
                table: "ClasificariImobilizari",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activ",
                table: "ClaseProduse",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Activ",
                table: "Angajamente",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RefuzuriSeed",
                columns: table => new
                {
                    ID = table.Column<Guid>(type: "uuid", nullable: false),
                    Tip = table.Column<string>(type: "text", nullable: true),
                    Cheie = table.Column<string>(type: "text", nullable: true),
                    La = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OptimisticLockField = table.Column<int>(type: "integer", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefuzuriSeed", x => x.ID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UnitatiMasura_Cod",
                table: "UnitatiMasura",
                column: "Cod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipuriTva_Cod",
                table: "TipuriTva",
                column: "Cod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipuriMaterial_Cod",
                table: "TipuriMaterial",
                column: "Cod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipuriDocument_ClrType",
                table: "TipuriDocument",
                column: "ClrType",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TipuriDocument_Cod",
                table: "TipuriDocument",
                column: "Cod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReguliStoc_TipDocumentId_Latura_ClasaId",
                table: "ReguliStoc",
                columns: new[] { "TipDocumentId", "Latura", "ClasaId" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_ReguliDeductibilitate_Categorie_DeLa",
                table: "ReguliDeductibilitate",
                columns: new[] { "Categorie", "DeLa" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReguliContare_TipDocumentId_TipMaterialId_NaturaFiltru_Semn~",
                table: "ReguliContare",
                columns: new[] { "TipDocumentId", "TipMaterialId", "NaturaFiltru", "SemnFiltru" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_RegistruTva_PerioadaAn_PerioadaLuna",
                table: "RegistruTva",
                columns: new[] { "PerioadaAn", "PerioadaLuna" });

            migrationBuilder.CreateIndex(
                name: "IX_RegistruStoc_Data",
                table: "RegistruStoc",
                column: "Data");

            migrationBuilder.CreateIndex(
                name: "IX_RegistruContabil_Data",
                table: "RegistruContabil",
                column: "Data");

            migrationBuilder.CreateIndex(
                name: "IX_RanduriD300_Cod",
                table: "RanduriD300",
                column: "Cod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiValidare_TipDocumentId",
                table: "PoliticiValidare",
                column: "TipDocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiTvaImplicit_TipDocumentId_ClasaFiscala_ValabilDeLa",
                table: "PoliticiTvaImplicit",
                columns: new[] { "TipDocumentId", "ClasaFiscala", "ValabilDeLa" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiTva_TipDocumentId",
                table: "PoliticiTva",
                column: "TipDocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiScadenta_TipDocumentId",
                table: "PoliticiScadenta",
                column: "TipDocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiNumerotare_TipDocumentId",
                table: "PoliticiNumerotare",
                column: "TipDocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiMiscareSaft_TipDocumentId_TipStoc",
                table: "PoliticiMiscareSaft",
                columns: new[] { "TipDocumentId", "TipStoc" },
                unique: true,
                filter: "\"Semn\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiMiscareSaft_TipDocumentId_TipStoc_Semn",
                table: "PoliticiMiscareSaft",
                columns: new[] { "TipDocumentId", "TipStoc", "Semn" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiInchidereTva_TipDocumentId",
                table: "PoliticiInchidereTva",
                column: "TipDocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiInchidere_Fel",
                table: "PoliticiInchidere",
                column: "Fel",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiDiferenta_TipDocumentId_Cauza_ClasaId",
                table: "PoliticiDiferenta",
                columns: new[] { "TipDocumentId", "Cauza", "ClasaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiConex_TipDocumentSursaId",
                table: "PoliticiConex",
                column: "TipDocumentSursaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiAmortizare_TipMaterialId",
                table: "PoliticiAmortizare",
                column: "TipMaterialId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PerioadeFiscale_An_Luna",
                table: "PerioadeFiscale",
                columns: new[] { "An", "Luna" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MapariTvaSaft_Versiune_Sectiune_TipTvaId_Regim_Cota_DeImpor~",
                table: "MapariTvaSaft",
                columns: new[] { "Versiune", "Sectiune", "TipTvaId", "Regim", "Cota", "DeImport", "Sens", "Rol" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MapariD394_TipTvaId_Sens",
                table: "MapariD394",
                columns: new[] { "TipTvaId", "Sens" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MapariD300_TipTvaId_Sens_RandId",
                table: "MapariD300",
                columns: new[] { "TipTvaId", "Sens", "RandId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Judete_Cod",
                table: "Judete",
                column: "Cod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Imperecheri_Data",
                table: "Imperecheri",
                column: "Data");

            migrationBuilder.CreateIndex(
                name: "IX_Imperecheri_InverseazaId",
                table: "Imperecheri",
                column: "InverseazaId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Imobilizari_NumarInventar",
                table: "Imobilizari",
                column: "NumarInventar",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DviFacturi_DviId_FacturaId",
                table: "DviFacturi",
                columns: new[] { "DviId", "FacturaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Documente_CorecteazaId",
                table: "Documente",
                column: "CorecteazaId");

            migrationBuilder.CreateIndex(
                name: "IX_Documente_DataInregistrare",
                table: "Documente",
                column: "DataInregistrare");

            migrationBuilder.CreateIndex(
                name: "IX_Conturi_Simbol",
                table: "Conturi",
                column: "Simbol",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClasificariImobilizari_Cod",
                table: "ClasificariImobilizari",
                column: "Cod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClaseProduse_Cod",
                table: "ClaseProduse",
                column: "Cod",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RefuzuriSeed_Tip_Cheie",
                table: "RefuzuriSeed",
                columns: new[] { "Tip", "Cheie" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentDetalii_DocumentDetalii_LinieSursaId",
                table: "DocumentDetalii",
                column: "LinieSursaId",
                principalTable: "DocumentDetalii",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentDetalii_Imobilizari_ImobilizareId",
                table: "DocumentDetalii",
                column: "ImobilizareId",
                principalTable: "Imobilizari",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentDetalii_TipuriMaterial_TipMaterialId",
                table: "DocumentDetalii",
                column: "TipMaterialId",
                principalTable: "TipuriMaterial",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Documente_Documente_CorecteazaId",
                table: "Documente",
                column: "CorecteazaId",
                principalTable: "Documente",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Documente_Documente_LaturaPerecheId",
                table: "Documente",
                column: "LaturaPerecheId",
                principalTable: "Documente",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Documente_Repartitori_PredatorId",
                table: "Documente",
                column: "PredatorId",
                principalTable: "Repartitori",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Documente_Repartitori_PrimitorId",
                table: "Documente",
                column: "PrimitorId",
                principalTable: "Repartitori",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_DviFacturi_Documente_DviId",
                table: "DviFacturi",
                column: "DviId",
                principalTable: "Documente",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DviFacturi_Documente_FacturaId",
                table: "DviFacturi",
                column: "FacturaId",
                principalTable: "Documente",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Imobilizari_ClasificariImobilizari_ClasificareId",
                table: "Imobilizari",
                column: "ClasificareId",
                principalTable: "ClasificariImobilizari",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Imobilizari_CoduriEconomice_CodEconomicId",
                table: "Imobilizari",
                column: "CodEconomicId",
                principalTable: "CoduriEconomice",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Imobilizari_Repartitori_CentruCostId",
                table: "Imobilizari",
                column: "CentruCostId",
                principalTable: "Repartitori",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Imobilizari_Repartitori_LocId",
                table: "Imobilizari",
                column: "LocId",
                principalTable: "Repartitori",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Imobilizari_Repartitori_ResponsabilId",
                table: "Imobilizari",
                column: "ResponsabilId",
                principalTable: "Repartitori",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Imobilizari_TipuriMaterial_TipMaterialId",
                table: "Imobilizari",
                column: "TipMaterialId",
                principalTable: "TipuriMaterial",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Imperecheri_Documente_DocumentId",
                table: "Imperecheri",
                column: "DocumentId",
                principalTable: "Documente",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Imperecheri_Documente_DocumentStingatorId",
                table: "Imperecheri",
                column: "DocumentStingatorId",
                principalTable: "Documente",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Imperecheri_Imperecheri_InverseazaId",
                table: "Imperecheri",
                column: "InverseazaId",
                principalTable: "Imperecheri",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_InchideriPerioade_PerioadeFiscale_PerioadaId",
                table: "InchideriPerioade",
                column: "PerioadaId",
                principalTable: "PerioadeFiscale",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Loturi_Produse_ProdusId",
                table: "Loturi",
                column: "ProdusId",
                principalTable: "Produse",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Loturi_Repartitori_GestiuneId",
                table: "Loturi",
                column: "GestiuneId",
                principalTable: "Repartitori",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_MapariD300_RanduriD300_RandId",
                table: "MapariD300",
                column: "RandId",
                principalTable: "RanduriD300",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_MapariD300_TipuriTva_TipTvaId",
                table: "MapariD300",
                column: "TipTvaId",
                principalTable: "TipuriTva",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_MapariD394_TipuriTva_TipTvaId",
                table: "MapariD394",
                column: "TipTvaId",
                principalTable: "TipuriTva",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_MapariTvaSaft_TipuriTva_TipTvaId",
                table: "MapariTvaSaft",
                column: "TipTvaId",
                principalTable: "TipuriTva",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PartideDeschise_Documente_DocumentId",
                table: "PartideDeschise",
                column: "DocumentId",
                principalTable: "Documente",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiAmortizare_Conturi_ContAmortizareId",
                table: "PoliticiAmortizare",
                column: "ContAmortizareId",
                principalTable: "Conturi",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiAmortizare_Conturi_ContCheltuialaAmortizareId",
                table: "PoliticiAmortizare",
                column: "ContCheltuialaAmortizareId",
                principalTable: "Conturi",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiAmortizare_Conturi_ContCheltuialaCedareId",
                table: "PoliticiAmortizare",
                column: "ContCheltuialaCedareId",
                principalTable: "Conturi",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiAmortizare_TipuriMaterial_TipMaterialId",
                table: "PoliticiAmortizare",
                column: "TipMaterialId",
                principalTable: "TipuriMaterial",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiConex_TipuriDocument_TipDocumentSursaId",
                table: "PoliticiConex",
                column: "TipDocumentSursaId",
                principalTable: "TipuriDocument",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiConex_TipuriDocument_TipDocumentTintaId",
                table: "PoliticiConex",
                column: "TipDocumentTintaId",
                principalTable: "TipuriDocument",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiDiferenta_ClaseProduse_ClasaId",
                table: "PoliticiDiferenta",
                column: "ClasaId",
                principalTable: "ClaseProduse",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiDiferenta_Conturi_ContId",
                table: "PoliticiDiferenta",
                column: "ContId",
                principalTable: "Conturi",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiDiferenta_TipuriDocument_TipDocumentId",
                table: "PoliticiDiferenta",
                column: "TipDocumentId",
                principalTable: "TipuriDocument",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiInchidereTva_TipuriDocument_TipDocumentId",
                table: "PoliticiInchidereTva",
                column: "TipDocumentId",
                principalTable: "TipuriDocument",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiMiscareSaft_TipuriDocument_TipDocumentId",
                table: "PoliticiMiscareSaft",
                column: "TipDocumentId",
                principalTable: "TipuriDocument",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiNumerotare_TipuriDocument_TipDocumentId",
                table: "PoliticiNumerotare",
                column: "TipDocumentId",
                principalTable: "TipuriDocument",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiScadenta_TipuriDocument_TipDocumentId",
                table: "PoliticiScadenta",
                column: "TipDocumentId",
                principalTable: "TipuriDocument",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiTva_TipuriDocument_TipDocumentId",
                table: "PoliticiTva",
                column: "TipDocumentId",
                principalTable: "TipuriDocument",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiTvaImplicit_TipuriDocument_TipDocumentId",
                table: "PoliticiTvaImplicit",
                column: "TipDocumentId",
                principalTable: "TipuriDocument",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiTvaImplicit_TipuriTva_TipTvaId",
                table: "PoliticiTvaImplicit",
                column: "TipTvaId",
                principalTable: "TipuriTva",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiValidare_TipuriDocument_TipDocumentId",
                table: "PoliticiValidare",
                column: "TipDocumentId",
                principalTable: "TipuriDocument",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Produse_UnitatiMasura_UnitateMasuraId",
                table: "Produse",
                column: "UnitateMasuraId",
                principalTable: "UnitatiMasura",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RanduriD300_RanduriD300_OglindaAId",
                table: "RanduriD300",
                column: "OglindaAId",
                principalTable: "RanduriD300",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RanduriD300_RanduriD300_ParinteId",
                table: "RanduriD300",
                column: "ParinteId",
                principalTable: "RanduriD300",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Conturi_ContCreditId",
                table: "RegistruContabil",
                column: "ContCreditId",
                principalTable: "Conturi",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Conturi_ContDebitId",
                table: "RegistruContabil",
                column: "ContDebitId",
                principalTable: "Conturi",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruImobilizari_DocumentDetalii_DetaliuId",
                table: "RegistruImobilizari",
                column: "DetaliuId",
                principalTable: "DocumentDetalii",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruImobilizari_Documente_DocumentId",
                table: "RegistruImobilizari",
                column: "DocumentId",
                principalTable: "Documente",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruImobilizari_Imobilizari_ImobilizareId",
                table: "RegistruImobilizari",
                column: "ImobilizareId",
                principalTable: "Imobilizari",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruImobilizari_Repartitori_RepartitorId",
                table: "RegistruImobilizari",
                column: "RepartitorId",
                principalTable: "Repartitori",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruStoc_Loturi_LotId",
                table: "RegistruStoc",
                column: "LotId",
                principalTable: "Loturi",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruStoc_Repartitori_RepartitorId",
                table: "RegistruStoc",
                column: "RepartitorId",
                principalTable: "Repartitori",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruTva_DocumentDetalii_DetaliuId",
                table: "RegistruTva",
                column: "DetaliuId",
                principalTable: "DocumentDetalii",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruTva_Documente_DocumentId",
                table: "RegistruTva",
                column: "DocumentId",
                principalTable: "Documente",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruTva_TipuriTva_TipTvaId",
                table: "RegistruTva",
                column: "TipTvaId",
                principalTable: "TipuriTva",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_ReguliContare_TipuriDocument_TipDocumentId",
                table: "ReguliContare",
                column: "TipDocumentId",
                principalTable: "TipuriDocument",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_ReguliStoc_TipuriDocument_TipDocumentId",
                table: "ReguliStoc",
                column: "TipDocumentId",
                principalTable: "TipuriDocument",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Repartitori_Judete_JudetId",
                table: "Repartitori",
                column: "JudetId",
                principalTable: "Judete",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Societati_Judete_JudetId",
                table: "Societati",
                column: "JudetId",
                principalTable: "Judete",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Societati_Repartitori_ContBancarId",
                table: "Societati",
                column: "ContBancarId",
                principalTable: "Repartitori",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_SolduriPerioadaContabil_CoduriEconomice_CodEconomicId",
                table: "SolduriPerioadaContabil",
                column: "CodEconomicId",
                principalTable: "CoduriEconomice",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_SolduriPerioadaContabil_CoduriFunctionale_CodFunctionalId",
                table: "SolduriPerioadaContabil",
                column: "CodFunctionalId",
                principalTable: "CoduriFunctionale",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_SolduriPerioadaContabil_Conturi_ContId",
                table: "SolduriPerioadaContabil",
                column: "ContId",
                principalTable: "Conturi",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_SolduriPerioadaContabil_Produse_MaterialId",
                table: "SolduriPerioadaContabil",
                column: "MaterialId",
                principalTable: "Produse",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_SolduriPerioadaContabil_Proiecte_ProiectId",
                table: "SolduriPerioadaContabil",
                column: "ProiectId",
                principalTable: "Proiecte",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_SolduriPerioadaContabil_Repartitori_CentruCostId",
                table: "SolduriPerioadaContabil",
                column: "CentruCostId",
                principalTable: "Repartitori",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_SolduriPerioadaContabil_Repartitori_RepartitorId",
                table: "SolduriPerioadaContabil",
                column: "RepartitorId",
                principalTable: "Repartitori",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_SolduriPerioadaContabil_SurseFinantare_SursaFinantareId",
                table: "SolduriPerioadaContabil",
                column: "SursaFinantareId",
                principalTable: "SurseFinantare",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_SolduriPerioadaContabil_Unitati_UnitateId",
                table: "SolduriPerioadaContabil",
                column: "UnitateId",
                principalTable: "Unitati",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_SolduriPerioadaStoc_Loturi_LotId",
                table: "SolduriPerioadaStoc",
                column: "LotId",
                principalTable: "Loturi",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_TipuriMaterial_ClaseProduse_ClasaId",
                table: "TipuriMaterial",
                column: "ClasaId",
                principalTable: "ClaseProduse",
                principalColumn: "ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentDetalii_DocumentDetalii_LinieSursaId",
                table: "DocumentDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentDetalii_Imobilizari_ImobilizareId",
                table: "DocumentDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_DocumentDetalii_TipuriMaterial_TipMaterialId",
                table: "DocumentDetalii");

            migrationBuilder.DropForeignKey(
                name: "FK_Documente_Documente_CorecteazaId",
                table: "Documente");

            migrationBuilder.DropForeignKey(
                name: "FK_Documente_Documente_LaturaPerecheId",
                table: "Documente");

            migrationBuilder.DropForeignKey(
                name: "FK_Documente_Repartitori_PredatorId",
                table: "Documente");

            migrationBuilder.DropForeignKey(
                name: "FK_Documente_Repartitori_PrimitorId",
                table: "Documente");

            migrationBuilder.DropForeignKey(
                name: "FK_DviFacturi_Documente_DviId",
                table: "DviFacturi");

            migrationBuilder.DropForeignKey(
                name: "FK_DviFacturi_Documente_FacturaId",
                table: "DviFacturi");

            migrationBuilder.DropForeignKey(
                name: "FK_Imobilizari_ClasificariImobilizari_ClasificareId",
                table: "Imobilizari");

            migrationBuilder.DropForeignKey(
                name: "FK_Imobilizari_CoduriEconomice_CodEconomicId",
                table: "Imobilizari");

            migrationBuilder.DropForeignKey(
                name: "FK_Imobilizari_Repartitori_CentruCostId",
                table: "Imobilizari");

            migrationBuilder.DropForeignKey(
                name: "FK_Imobilizari_Repartitori_LocId",
                table: "Imobilizari");

            migrationBuilder.DropForeignKey(
                name: "FK_Imobilizari_Repartitori_ResponsabilId",
                table: "Imobilizari");

            migrationBuilder.DropForeignKey(
                name: "FK_Imobilizari_TipuriMaterial_TipMaterialId",
                table: "Imobilizari");

            migrationBuilder.DropForeignKey(
                name: "FK_Imperecheri_Documente_DocumentId",
                table: "Imperecheri");

            migrationBuilder.DropForeignKey(
                name: "FK_Imperecheri_Documente_DocumentStingatorId",
                table: "Imperecheri");

            migrationBuilder.DropForeignKey(
                name: "FK_Imperecheri_Imperecheri_InverseazaId",
                table: "Imperecheri");

            migrationBuilder.DropForeignKey(
                name: "FK_InchideriPerioade_PerioadeFiscale_PerioadaId",
                table: "InchideriPerioade");

            migrationBuilder.DropForeignKey(
                name: "FK_Loturi_Produse_ProdusId",
                table: "Loturi");

            migrationBuilder.DropForeignKey(
                name: "FK_Loturi_Repartitori_GestiuneId",
                table: "Loturi");

            migrationBuilder.DropForeignKey(
                name: "FK_MapariD300_RanduriD300_RandId",
                table: "MapariD300");

            migrationBuilder.DropForeignKey(
                name: "FK_MapariD300_TipuriTva_TipTvaId",
                table: "MapariD300");

            migrationBuilder.DropForeignKey(
                name: "FK_MapariD394_TipuriTva_TipTvaId",
                table: "MapariD394");

            migrationBuilder.DropForeignKey(
                name: "FK_MapariTvaSaft_TipuriTva_TipTvaId",
                table: "MapariTvaSaft");

            migrationBuilder.DropForeignKey(
                name: "FK_PartideDeschise_Documente_DocumentId",
                table: "PartideDeschise");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiAmortizare_Conturi_ContAmortizareId",
                table: "PoliticiAmortizare");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiAmortizare_Conturi_ContCheltuialaAmortizareId",
                table: "PoliticiAmortizare");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiAmortizare_Conturi_ContCheltuialaCedareId",
                table: "PoliticiAmortizare");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiAmortizare_TipuriMaterial_TipMaterialId",
                table: "PoliticiAmortizare");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiConex_TipuriDocument_TipDocumentSursaId",
                table: "PoliticiConex");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiConex_TipuriDocument_TipDocumentTintaId",
                table: "PoliticiConex");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiDiferenta_ClaseProduse_ClasaId",
                table: "PoliticiDiferenta");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiDiferenta_Conturi_ContId",
                table: "PoliticiDiferenta");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiDiferenta_TipuriDocument_TipDocumentId",
                table: "PoliticiDiferenta");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiInchidereTva_TipuriDocument_TipDocumentId",
                table: "PoliticiInchidereTva");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiMiscareSaft_TipuriDocument_TipDocumentId",
                table: "PoliticiMiscareSaft");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiNumerotare_TipuriDocument_TipDocumentId",
                table: "PoliticiNumerotare");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiScadenta_TipuriDocument_TipDocumentId",
                table: "PoliticiScadenta");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiTva_TipuriDocument_TipDocumentId",
                table: "PoliticiTva");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiTvaImplicit_TipuriDocument_TipDocumentId",
                table: "PoliticiTvaImplicit");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiTvaImplicit_TipuriTva_TipTvaId",
                table: "PoliticiTvaImplicit");

            migrationBuilder.DropForeignKey(
                name: "FK_PoliticiValidare_TipuriDocument_TipDocumentId",
                table: "PoliticiValidare");

            migrationBuilder.DropForeignKey(
                name: "FK_Produse_UnitatiMasura_UnitateMasuraId",
                table: "Produse");

            migrationBuilder.DropForeignKey(
                name: "FK_RanduriD300_RanduriD300_OglindaAId",
                table: "RanduriD300");

            migrationBuilder.DropForeignKey(
                name: "FK_RanduriD300_RanduriD300_ParinteId",
                table: "RanduriD300");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Conturi_ContCreditId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruContabil_Conturi_ContDebitId",
                table: "RegistruContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruImobilizari_DocumentDetalii_DetaliuId",
                table: "RegistruImobilizari");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruImobilizari_Documente_DocumentId",
                table: "RegistruImobilizari");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruImobilizari_Imobilizari_ImobilizareId",
                table: "RegistruImobilizari");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruImobilizari_Repartitori_RepartitorId",
                table: "RegistruImobilizari");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruStoc_Loturi_LotId",
                table: "RegistruStoc");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruStoc_Repartitori_RepartitorId",
                table: "RegistruStoc");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruTva_DocumentDetalii_DetaliuId",
                table: "RegistruTva");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruTva_Documente_DocumentId",
                table: "RegistruTva");

            migrationBuilder.DropForeignKey(
                name: "FK_RegistruTva_TipuriTva_TipTvaId",
                table: "RegistruTva");

            migrationBuilder.DropForeignKey(
                name: "FK_ReguliContare_TipuriDocument_TipDocumentId",
                table: "ReguliContare");

            migrationBuilder.DropForeignKey(
                name: "FK_ReguliStoc_TipuriDocument_TipDocumentId",
                table: "ReguliStoc");

            migrationBuilder.DropForeignKey(
                name: "FK_Repartitori_Judete_JudetId",
                table: "Repartitori");

            migrationBuilder.DropForeignKey(
                name: "FK_Societati_Judete_JudetId",
                table: "Societati");

            migrationBuilder.DropForeignKey(
                name: "FK_Societati_Repartitori_ContBancarId",
                table: "Societati");

            migrationBuilder.DropForeignKey(
                name: "FK_SolduriPerioadaContabil_CoduriEconomice_CodEconomicId",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_SolduriPerioadaContabil_CoduriFunctionale_CodFunctionalId",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_SolduriPerioadaContabil_Conturi_ContId",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_SolduriPerioadaContabil_Produse_MaterialId",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_SolduriPerioadaContabil_Proiecte_ProiectId",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_SolduriPerioadaContabil_Repartitori_CentruCostId",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_SolduriPerioadaContabil_Repartitori_RepartitorId",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_SolduriPerioadaContabil_SurseFinantare_SursaFinantareId",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_SolduriPerioadaContabil_Unitati_UnitateId",
                table: "SolduriPerioadaContabil");

            migrationBuilder.DropForeignKey(
                name: "FK_SolduriPerioadaStoc_Loturi_LotId",
                table: "SolduriPerioadaStoc");

            migrationBuilder.DropForeignKey(
                name: "FK_TipuriMaterial_ClaseProduse_ClasaId",
                table: "TipuriMaterial");

            migrationBuilder.DropTable(
                name: "RefuzuriSeed");

            migrationBuilder.DropIndex(
                name: "IX_UnitatiMasura_Cod",
                table: "UnitatiMasura");

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
                name: "IX_ReguliDeductibilitate_Categorie_DeLa",
                table: "ReguliDeductibilitate");

            migrationBuilder.DropIndex(
                name: "IX_ReguliContare_TipDocumentId_TipMaterialId_NaturaFiltru_Semn~",
                table: "ReguliContare");

            migrationBuilder.DropIndex(
                name: "IX_RegistruTva_PerioadaAn_PerioadaLuna",
                table: "RegistruTva");

            migrationBuilder.DropIndex(
                name: "IX_RegistruStoc_Data",
                table: "RegistruStoc");

            migrationBuilder.DropIndex(
                name: "IX_RegistruContabil_Data",
                table: "RegistruContabil");

            migrationBuilder.DropIndex(
                name: "IX_RanduriD300_Cod",
                table: "RanduriD300");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiValidare_TipDocumentId",
                table: "PoliticiValidare");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiTvaImplicit_TipDocumentId_ClasaFiscala_ValabilDeLa",
                table: "PoliticiTvaImplicit");

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
                name: "IX_PoliticiMiscareSaft_TipDocumentId_TipStoc",
                table: "PoliticiMiscareSaft");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiMiscareSaft_TipDocumentId_TipStoc_Semn",
                table: "PoliticiMiscareSaft");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiInchidereTva_TipDocumentId",
                table: "PoliticiInchidereTva");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiInchidere_Fel",
                table: "PoliticiInchidere");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiDiferenta_TipDocumentId_Cauza_ClasaId",
                table: "PoliticiDiferenta");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiConex_TipDocumentSursaId",
                table: "PoliticiConex");

            migrationBuilder.DropIndex(
                name: "IX_PoliticiAmortizare_TipMaterialId",
                table: "PoliticiAmortizare");

            migrationBuilder.DropIndex(
                name: "IX_PerioadeFiscale_An_Luna",
                table: "PerioadeFiscale");

            migrationBuilder.DropIndex(
                name: "IX_MapariTvaSaft_Versiune_Sectiune_TipTvaId_Regim_Cota_DeImpor~",
                table: "MapariTvaSaft");

            migrationBuilder.DropIndex(
                name: "IX_MapariD394_TipTvaId_Sens",
                table: "MapariD394");

            migrationBuilder.DropIndex(
                name: "IX_MapariD300_TipTvaId_Sens_RandId",
                table: "MapariD300");

            migrationBuilder.DropIndex(
                name: "IX_Judete_Cod",
                table: "Judete");

            migrationBuilder.DropIndex(
                name: "IX_Imperecheri_Data",
                table: "Imperecheri");

            migrationBuilder.DropIndex(
                name: "IX_Imperecheri_InverseazaId",
                table: "Imperecheri");

            migrationBuilder.DropIndex(
                name: "IX_Imobilizari_NumarInventar",
                table: "Imobilizari");

            migrationBuilder.DropIndex(
                name: "IX_DviFacturi_DviId_FacturaId",
                table: "DviFacturi");

            migrationBuilder.DropIndex(
                name: "IX_Documente_CorecteazaId",
                table: "Documente");

            migrationBuilder.DropIndex(
                name: "IX_Documente_DataInregistrare",
                table: "Documente");

            migrationBuilder.DropIndex(
                name: "IX_Conturi_Simbol",
                table: "Conturi");

            migrationBuilder.DropIndex(
                name: "IX_ClasificariImobilizari_Cod",
                table: "ClasificariImobilizari");

            migrationBuilder.DropIndex(
                name: "IX_ClaseProduse_Cod",
                table: "ClaseProduse");

            migrationBuilder.DropColumn(
                name: "Activ",
                table: "UnitatiMasura");

            migrationBuilder.DropColumn(
                name: "Activ",
                table: "Unitati");

            migrationBuilder.DropColumn(
                name: "Activ",
                table: "TipuriMaterial");

            migrationBuilder.DropColumn(
                name: "Activ",
                table: "SurseFinantare");

            migrationBuilder.DropColumn(
                name: "Activ",
                table: "RanduriD300");

            migrationBuilder.DropColumn(
                name: "Activ",
                table: "Proiecte");

            migrationBuilder.DropColumn(
                name: "Activ",
                table: "Produse");

            migrationBuilder.DropColumn(
                name: "Activ",
                table: "Judete");

            migrationBuilder.DropColumn(
                name: "Activ",
                table: "Imobilizari");

            migrationBuilder.DropColumn(
                name: "Activ",
                table: "Conturi");

            migrationBuilder.DropColumn(
                name: "Activ",
                table: "CoduriFunctionale");

            migrationBuilder.DropColumn(
                name: "Activ",
                table: "CoduriEconomice");

            migrationBuilder.DropColumn(
                name: "Activ",
                table: "ClasificariImobilizari");

            migrationBuilder.DropColumn(
                name: "Activ",
                table: "ClaseProduse");

            migrationBuilder.DropColumn(
                name: "Activ",
                table: "Angajamente");

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "UnitatiMasura",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "Unitati",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<bool>(
                name: "Activ",
                table: "TipuriTva",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "TipuriTva",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "TipuriMaterial",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "TipuriDocument",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "SurseFinantare",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "StateMachineTransitions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "StateMachineStates",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "StateMachineAppearances",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "SolduriPerioadaStoc",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OptimisticLockField",
                table: "SolduriPerioadaStoc",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "SolduriPerioadaContabil",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OptimisticLockField",
                table: "SolduriPerioadaContabil",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "Societati",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "SetariProfil",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "ReportDataV2",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "Repartitori",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "ReguliStoc",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "ReguliDeductibilitate",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "ReguliContare",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "RegistruTva",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OptimisticLockField",
                table: "RegistruTva",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "RegistruStoc",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OptimisticLockField",
                table: "RegistruStoc",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "RegistruImobilizari",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OptimisticLockField",
                table: "RegistruImobilizari",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "RegistruContabil",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OptimisticLockField",
                table: "RegistruContabil",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "RanduriD300",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "Proiecte",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "Produse",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "PoliticiValidare",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "PoliticiTvaImplicit",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "PoliticiTva",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "PoliticiScadenta",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "PoliticiNumerotare",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "PoliticiMiscareSaft",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "PoliticiInchidereTva",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "PoliticiInchidere",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "PoliticiDiferenta",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "PoliticiConex",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "PoliticiAmortizare",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "PermissionPolicyUserLoginInfo",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "PermissionPolicyUser",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "PermissionPolicyTypePermissionObject",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "PermissionPolicyRoleBase",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "PermissionPolicyObjectPermissionsObject",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "PermissionPolicyNavigationPermissionObject",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "PermissionPolicyMemberPermissionsObject",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "PermissionPolicyActionPermissionObject",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "PerioadeFiscale",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "PartideDeschise",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OptimisticLockField",
                table: "PartideDeschise",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "ModelDifferences",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "ModelDifferenceAspects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "MigrareLegaturi",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OptimisticLockField",
                table: "MigrareLegaturi",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "MapariTvaSaft",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "MapariD394",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "MapariD300",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "Loturi",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "Judete",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "InchideriPerioade",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OptimisticLockField",
                table: "InchideriPerioade",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "Imperecheri",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "Imobilizari",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "HCategories",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "FileData",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "Events",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "DviFacturi",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "Documente",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "DocumentDetalii",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "DepuneriDeclaratii",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "OptimisticLockField",
                table: "DepuneriDeclaratii",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "DashboardData",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "Conturi",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "CoduriFunctionale",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "CoduriEconomice",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "ClasificariImobilizari",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "ClaseProduse",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "AuditEFCoreWeakReferences",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GCRecord",
                table: "Angajamente",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UnitatiMasura_Cod",
                table: "UnitatiMasura",
                column: "Cod",
                unique: true,
                filter: "\"GCRecord\" = 0");

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
                name: "IX_RegistruTva_PerioadaAn_PerioadaLuna",
                table: "RegistruTva",
                columns: new[] { "PerioadaAn", "PerioadaLuna" },
                filter: "\"GCRecord\" = 0");

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
                name: "IX_RanduriD300_Cod",
                table: "RanduriD300",
                column: "Cod",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiValidare_TipDocumentId",
                table: "PoliticiValidare",
                column: "TipDocumentId",
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
                name: "IX_PoliticiMiscareSaft_TipDocumentId_TipStoc",
                table: "PoliticiMiscareSaft",
                columns: new[] { "TipDocumentId", "TipStoc" },
                unique: true,
                filter: "\"Semn\" IS NULL AND \"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiMiscareSaft_TipDocumentId_TipStoc_Semn",
                table: "PoliticiMiscareSaft",
                columns: new[] { "TipDocumentId", "TipStoc", "Semn" },
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiInchidereTva_TipDocumentId",
                table: "PoliticiInchidereTva",
                column: "TipDocumentId",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiInchidere_Fel",
                table: "PoliticiInchidere",
                column: "Fel",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiDiferenta_TipDocumentId_Cauza_ClasaId",
                table: "PoliticiDiferenta",
                columns: new[] { "TipDocumentId", "Cauza", "ClasaId" },
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiConex_TipDocumentSursaId",
                table: "PoliticiConex",
                column: "TipDocumentSursaId",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PoliticiAmortizare_TipMaterialId",
                table: "PoliticiAmortizare",
                column: "TipMaterialId",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_PerioadeFiscale_An_Luna",
                table: "PerioadeFiscale",
                columns: new[] { "An", "Luna" },
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MapariTvaSaft_Versiune_Sectiune_TipTvaId_Regim_Cota_DeImpor~",
                table: "MapariTvaSaft",
                columns: new[] { "Versiune", "Sectiune", "TipTvaId", "Regim", "Cota", "DeImport", "Sens", "Rol" },
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MapariD394_TipTvaId_Sens",
                table: "MapariD394",
                columns: new[] { "TipTvaId", "Sens" },
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_MapariD300_TipTvaId_Sens_RandId",
                table: "MapariD300",
                columns: new[] { "TipTvaId", "Sens", "RandId" },
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Judete_Cod",
                table: "Judete",
                column: "Cod",
                unique: true,
                filter: "\"GCRecord\" = 0");

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
                name: "IX_Imobilizari_NumarInventar",
                table: "Imobilizari",
                column: "NumarInventar",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_DviFacturi_DviId_FacturaId",
                table: "DviFacturi",
                columns: new[] { "DviId", "FacturaId" },
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Documente_CorecteazaId",
                table: "Documente",
                column: "CorecteazaId",
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Documente_DataInregistrare",
                table: "Documente",
                column: "DataInregistrare",
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_Conturi_Simbol",
                table: "Conturi",
                column: "Simbol",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ClasificariImobilizari_Cod",
                table: "ClasificariImobilizari",
                column: "Cod",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.CreateIndex(
                name: "IX_ClaseProduse_Cod",
                table: "ClaseProduse",
                column: "Cod",
                unique: true,
                filter: "\"GCRecord\" = 0");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentDetalii_DocumentDetalii_LinieSursaId",
                table: "DocumentDetalii",
                column: "LinieSursaId",
                principalTable: "DocumentDetalii",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentDetalii_Imobilizari_ImobilizareId",
                table: "DocumentDetalii",
                column: "ImobilizareId",
                principalTable: "Imobilizari",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentDetalii_TipuriMaterial_TipMaterialId",
                table: "DocumentDetalii",
                column: "TipMaterialId",
                principalTable: "TipuriMaterial",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Documente_Documente_CorecteazaId",
                table: "Documente",
                column: "CorecteazaId",
                principalTable: "Documente",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documente_Documente_LaturaPerecheId",
                table: "Documente",
                column: "LaturaPerecheId",
                principalTable: "Documente",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Documente_Repartitori_PredatorId",
                table: "Documente",
                column: "PredatorId",
                principalTable: "Repartitori",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Documente_Repartitori_PrimitorId",
                table: "Documente",
                column: "PrimitorId",
                principalTable: "Repartitori",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DviFacturi_Documente_DviId",
                table: "DviFacturi",
                column: "DviId",
                principalTable: "Documente",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DviFacturi_Documente_FacturaId",
                table: "DviFacturi",
                column: "FacturaId",
                principalTable: "Documente",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Imobilizari_ClasificariImobilizari_ClasificareId",
                table: "Imobilizari",
                column: "ClasificareId",
                principalTable: "ClasificariImobilizari",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Imobilizari_CoduriEconomice_CodEconomicId",
                table: "Imobilizari",
                column: "CodEconomicId",
                principalTable: "CoduriEconomice",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Imobilizari_Repartitori_CentruCostId",
                table: "Imobilizari",
                column: "CentruCostId",
                principalTable: "Repartitori",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Imobilizari_Repartitori_LocId",
                table: "Imobilizari",
                column: "LocId",
                principalTable: "Repartitori",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Imobilizari_Repartitori_ResponsabilId",
                table: "Imobilizari",
                column: "ResponsabilId",
                principalTable: "Repartitori",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Imobilizari_TipuriMaterial_TipMaterialId",
                table: "Imobilizari",
                column: "TipMaterialId",
                principalTable: "TipuriMaterial",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Imperecheri_Documente_DocumentId",
                table: "Imperecheri",
                column: "DocumentId",
                principalTable: "Documente",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Imperecheri_Documente_DocumentStingatorId",
                table: "Imperecheri",
                column: "DocumentStingatorId",
                principalTable: "Documente",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Imperecheri_Imperecheri_InverseazaId",
                table: "Imperecheri",
                column: "InverseazaId",
                principalTable: "Imperecheri",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InchideriPerioade_PerioadeFiscale_PerioadaId",
                table: "InchideriPerioade",
                column: "PerioadaId",
                principalTable: "PerioadeFiscale",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Loturi_Produse_ProdusId",
                table: "Loturi",
                column: "ProdusId",
                principalTable: "Produse",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Loturi_Repartitori_GestiuneId",
                table: "Loturi",
                column: "GestiuneId",
                principalTable: "Repartitori",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MapariD300_RanduriD300_RandId",
                table: "MapariD300",
                column: "RandId",
                principalTable: "RanduriD300",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MapariD300_TipuriTva_TipTvaId",
                table: "MapariD300",
                column: "TipTvaId",
                principalTable: "TipuriTva",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MapariD394_TipuriTva_TipTvaId",
                table: "MapariD394",
                column: "TipTvaId",
                principalTable: "TipuriTva",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MapariTvaSaft_TipuriTva_TipTvaId",
                table: "MapariTvaSaft",
                column: "TipTvaId",
                principalTable: "TipuriTva",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PartideDeschise_Documente_DocumentId",
                table: "PartideDeschise",
                column: "DocumentId",
                principalTable: "Documente",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiAmortizare_Conturi_ContAmortizareId",
                table: "PoliticiAmortizare",
                column: "ContAmortizareId",
                principalTable: "Conturi",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiAmortizare_Conturi_ContCheltuialaAmortizareId",
                table: "PoliticiAmortizare",
                column: "ContCheltuialaAmortizareId",
                principalTable: "Conturi",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiAmortizare_Conturi_ContCheltuialaCedareId",
                table: "PoliticiAmortizare",
                column: "ContCheltuialaCedareId",
                principalTable: "Conturi",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiAmortizare_TipuriMaterial_TipMaterialId",
                table: "PoliticiAmortizare",
                column: "TipMaterialId",
                principalTable: "TipuriMaterial",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiConex_TipuriDocument_TipDocumentSursaId",
                table: "PoliticiConex",
                column: "TipDocumentSursaId",
                principalTable: "TipuriDocument",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiConex_TipuriDocument_TipDocumentTintaId",
                table: "PoliticiConex",
                column: "TipDocumentTintaId",
                principalTable: "TipuriDocument",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiDiferenta_ClaseProduse_ClasaId",
                table: "PoliticiDiferenta",
                column: "ClasaId",
                principalTable: "ClaseProduse",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiDiferenta_Conturi_ContId",
                table: "PoliticiDiferenta",
                column: "ContId",
                principalTable: "Conturi",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiDiferenta_TipuriDocument_TipDocumentId",
                table: "PoliticiDiferenta",
                column: "TipDocumentId",
                principalTable: "TipuriDocument",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiInchidereTva_TipuriDocument_TipDocumentId",
                table: "PoliticiInchidereTva",
                column: "TipDocumentId",
                principalTable: "TipuriDocument",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiMiscareSaft_TipuriDocument_TipDocumentId",
                table: "PoliticiMiscareSaft",
                column: "TipDocumentId",
                principalTable: "TipuriDocument",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiNumerotare_TipuriDocument_TipDocumentId",
                table: "PoliticiNumerotare",
                column: "TipDocumentId",
                principalTable: "TipuriDocument",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiScadenta_TipuriDocument_TipDocumentId",
                table: "PoliticiScadenta",
                column: "TipDocumentId",
                principalTable: "TipuriDocument",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiTva_TipuriDocument_TipDocumentId",
                table: "PoliticiTva",
                column: "TipDocumentId",
                principalTable: "TipuriDocument",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiTvaImplicit_TipuriDocument_TipDocumentId",
                table: "PoliticiTvaImplicit",
                column: "TipDocumentId",
                principalTable: "TipuriDocument",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiTvaImplicit_TipuriTva_TipTvaId",
                table: "PoliticiTvaImplicit",
                column: "TipTvaId",
                principalTable: "TipuriTva",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PoliticiValidare_TipuriDocument_TipDocumentId",
                table: "PoliticiValidare",
                column: "TipDocumentId",
                principalTable: "TipuriDocument",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Produse_UnitatiMasura_UnitateMasuraId",
                table: "Produse",
                column: "UnitateMasuraId",
                principalTable: "UnitatiMasura",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RanduriD300_RanduriD300_OglindaAId",
                table: "RanduriD300",
                column: "OglindaAId",
                principalTable: "RanduriD300",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RanduriD300_RanduriD300_ParinteId",
                table: "RanduriD300",
                column: "ParinteId",
                principalTable: "RanduriD300",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Conturi_ContCreditId",
                table: "RegistruContabil",
                column: "ContCreditId",
                principalTable: "Conturi",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruContabil_Conturi_ContDebitId",
                table: "RegistruContabil",
                column: "ContDebitId",
                principalTable: "Conturi",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruImobilizari_DocumentDetalii_DetaliuId",
                table: "RegistruImobilizari",
                column: "DetaliuId",
                principalTable: "DocumentDetalii",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruImobilizari_Documente_DocumentId",
                table: "RegistruImobilizari",
                column: "DocumentId",
                principalTable: "Documente",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruImobilizari_Imobilizari_ImobilizareId",
                table: "RegistruImobilizari",
                column: "ImobilizareId",
                principalTable: "Imobilizari",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruImobilizari_Repartitori_RepartitorId",
                table: "RegistruImobilizari",
                column: "RepartitorId",
                principalTable: "Repartitori",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruStoc_Loturi_LotId",
                table: "RegistruStoc",
                column: "LotId",
                principalTable: "Loturi",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruStoc_Repartitori_RepartitorId",
                table: "RegistruStoc",
                column: "RepartitorId",
                principalTable: "Repartitori",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruTva_DocumentDetalii_DetaliuId",
                table: "RegistruTva",
                column: "DetaliuId",
                principalTable: "DocumentDetalii",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruTva_Documente_DocumentId",
                table: "RegistruTva",
                column: "DocumentId",
                principalTable: "Documente",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RegistruTva_TipuriTva_TipTvaId",
                table: "RegistruTva",
                column: "TipTvaId",
                principalTable: "TipuriTva",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReguliContare_TipuriDocument_TipDocumentId",
                table: "ReguliContare",
                column: "TipDocumentId",
                principalTable: "TipuriDocument",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ReguliStoc_TipuriDocument_TipDocumentId",
                table: "ReguliStoc",
                column: "TipDocumentId",
                principalTable: "TipuriDocument",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Repartitori_Judete_JudetId",
                table: "Repartitori",
                column: "JudetId",
                principalTable: "Judete",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Societati_Judete_JudetId",
                table: "Societati",
                column: "JudetId",
                principalTable: "Judete",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Societati_Repartitori_ContBancarId",
                table: "Societati",
                column: "ContBancarId",
                principalTable: "Repartitori",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SolduriPerioadaContabil_CoduriEconomice_CodEconomicId",
                table: "SolduriPerioadaContabil",
                column: "CodEconomicId",
                principalTable: "CoduriEconomice",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SolduriPerioadaContabil_CoduriFunctionale_CodFunctionalId",
                table: "SolduriPerioadaContabil",
                column: "CodFunctionalId",
                principalTable: "CoduriFunctionale",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SolduriPerioadaContabil_Conturi_ContId",
                table: "SolduriPerioadaContabil",
                column: "ContId",
                principalTable: "Conturi",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SolduriPerioadaContabil_Produse_MaterialId",
                table: "SolduriPerioadaContabil",
                column: "MaterialId",
                principalTable: "Produse",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SolduriPerioadaContabil_Proiecte_ProiectId",
                table: "SolduriPerioadaContabil",
                column: "ProiectId",
                principalTable: "Proiecte",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SolduriPerioadaContabil_Repartitori_CentruCostId",
                table: "SolduriPerioadaContabil",
                column: "CentruCostId",
                principalTable: "Repartitori",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SolduriPerioadaContabil_Repartitori_RepartitorId",
                table: "SolduriPerioadaContabil",
                column: "RepartitorId",
                principalTable: "Repartitori",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SolduriPerioadaContabil_SurseFinantare_SursaFinantareId",
                table: "SolduriPerioadaContabil",
                column: "SursaFinantareId",
                principalTable: "SurseFinantare",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SolduriPerioadaContabil_Unitati_UnitateId",
                table: "SolduriPerioadaContabil",
                column: "UnitateId",
                principalTable: "Unitati",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SolduriPerioadaStoc_Loturi_LotId",
                table: "SolduriPerioadaStoc",
                column: "LotId",
                principalTable: "Loturi",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TipuriMaterial_ClaseProduse_ClasaId",
                table: "TipuriMaterial",
                column: "ClasaId",
                principalTable: "ClaseProduse",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
