using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class CodDenumireObligatorii : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "UnitatiMasura",
                type: "character varying(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "UnitatiMasura",
                type: "character varying(9)",
                maxLength: 9,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(9)",
                oldMaxLength: 9,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "TipuriTva",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "TipuriTva",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "TipuriMaterial",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "TipuriMaterial",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "TipuriDocument",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "TipuriDocument",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "SurseFinantare",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "SurseFinantare",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "Repartitori",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "Repartitori",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "Proiecte",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "Proiecte",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "Produse",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "Produse",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "Judete",
                type: "character varying(35)",
                maxLength: 35,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(35)",
                oldMaxLength: 35,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "Judete",
                type: "character varying(5)",
                maxLength: 5,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Simbol",
                table: "Conturi",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "Conturi",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "CoduriFunctionale",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "CoduriFunctionale",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "CoduriEconomice",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "CoduriEconomice",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "ClaseProduse",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "ClaseProduse",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "Angajamente",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "Angajamente",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_UnitatiMasura_Cod_negol",
                table: "UnitatiMasura",
                sql: "btrim(\"Cod\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_UnitatiMasura_Denumire_negol",
                table: "UnitatiMasura",
                sql: "btrim(\"Denumire\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TipuriTva_Cod_negol",
                table: "TipuriTva",
                sql: "btrim(\"Cod\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TipuriTva_Denumire_negol",
                table: "TipuriTva",
                sql: "btrim(\"Denumire\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TipuriMaterial_Cod_negol",
                table: "TipuriMaterial",
                sql: "btrim(\"Cod\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TipuriMaterial_Denumire_negol",
                table: "TipuriMaterial",
                sql: "btrim(\"Denumire\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TipuriDocument_Cod_negol",
                table: "TipuriDocument",
                sql: "btrim(\"Cod\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_TipuriDocument_Denumire_negol",
                table: "TipuriDocument",
                sql: "btrim(\"Denumire\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SurseFinantare_Cod_negol",
                table: "SurseFinantare",
                sql: "btrim(\"Cod\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_SurseFinantare_Denumire_negol",
                table: "SurseFinantare",
                sql: "btrim(\"Denumire\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Repartitori_Cod_negol",
                table: "Repartitori",
                sql: "btrim(\"Cod\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Repartitori_Denumire_negol",
                table: "Repartitori",
                sql: "btrim(\"Denumire\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Proiecte_Cod_negol",
                table: "Proiecte",
                sql: "btrim(\"Cod\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Proiecte_Denumire_negol",
                table: "Proiecte",
                sql: "btrim(\"Denumire\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Produse_Cod_negol",
                table: "Produse",
                sql: "btrim(\"Cod\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Produse_Denumire_negol",
                table: "Produse",
                sql: "btrim(\"Denumire\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Judete_Cod_negol",
                table: "Judete",
                sql: "btrim(\"Cod\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Judete_Denumire_negol",
                table: "Judete",
                sql: "btrim(\"Denumire\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Conturi_Denumire_negol",
                table: "Conturi",
                sql: "btrim(\"Denumire\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Conturi_Simbol_negol",
                table: "Conturi",
                sql: "btrim(\"Simbol\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CoduriFunctionale_Cod_negol",
                table: "CoduriFunctionale",
                sql: "btrim(\"Cod\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CoduriFunctionale_Denumire_negol",
                table: "CoduriFunctionale",
                sql: "btrim(\"Denumire\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CoduriEconomice_Cod_negol",
                table: "CoduriEconomice",
                sql: "btrim(\"Cod\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_CoduriEconomice_Denumire_negol",
                table: "CoduriEconomice",
                sql: "btrim(\"Denumire\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ClaseProduse_Cod_negol",
                table: "ClaseProduse",
                sql: "btrim(\"Cod\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_ClaseProduse_Denumire_negol",
                table: "ClaseProduse",
                sql: "btrim(\"Denumire\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Angajamente_Cod_negol",
                table: "Angajamente",
                sql: "btrim(\"Cod\") <> ''");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Angajamente_Denumire_negol",
                table: "Angajamente",
                sql: "btrim(\"Denumire\") <> ''");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_UnitatiMasura_Cod_negol",
                table: "UnitatiMasura");

            migrationBuilder.DropCheckConstraint(
                name: "CK_UnitatiMasura_Denumire_negol",
                table: "UnitatiMasura");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TipuriTva_Cod_negol",
                table: "TipuriTva");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TipuriTva_Denumire_negol",
                table: "TipuriTva");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TipuriMaterial_Cod_negol",
                table: "TipuriMaterial");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TipuriMaterial_Denumire_negol",
                table: "TipuriMaterial");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TipuriDocument_Cod_negol",
                table: "TipuriDocument");

            migrationBuilder.DropCheckConstraint(
                name: "CK_TipuriDocument_Denumire_negol",
                table: "TipuriDocument");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SurseFinantare_Cod_negol",
                table: "SurseFinantare");

            migrationBuilder.DropCheckConstraint(
                name: "CK_SurseFinantare_Denumire_negol",
                table: "SurseFinantare");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Repartitori_Cod_negol",
                table: "Repartitori");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Repartitori_Denumire_negol",
                table: "Repartitori");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Proiecte_Cod_negol",
                table: "Proiecte");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Proiecte_Denumire_negol",
                table: "Proiecte");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Produse_Cod_negol",
                table: "Produse");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Produse_Denumire_negol",
                table: "Produse");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Judete_Cod_negol",
                table: "Judete");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Judete_Denumire_negol",
                table: "Judete");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Conturi_Denumire_negol",
                table: "Conturi");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Conturi_Simbol_negol",
                table: "Conturi");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CoduriFunctionale_Cod_negol",
                table: "CoduriFunctionale");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CoduriFunctionale_Denumire_negol",
                table: "CoduriFunctionale");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CoduriEconomice_Cod_negol",
                table: "CoduriEconomice");

            migrationBuilder.DropCheckConstraint(
                name: "CK_CoduriEconomice_Denumire_negol",
                table: "CoduriEconomice");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ClaseProduse_Cod_negol",
                table: "ClaseProduse");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ClaseProduse_Denumire_negol",
                table: "ClaseProduse");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Angajamente_Cod_negol",
                table: "Angajamente");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Angajamente_Denumire_negol",
                table: "Angajamente");

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "UnitatiMasura",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "UnitatiMasura",
                type: "character varying(9)",
                maxLength: 9,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(9)",
                oldMaxLength: 9);

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "TipuriTva",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "TipuriTva",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "TipuriMaterial",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "TipuriMaterial",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "TipuriDocument",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "TipuriDocument",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "SurseFinantare",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "SurseFinantare",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "Repartitori",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "Repartitori",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "Proiecte",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "Proiecte",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "Produse",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "Produse",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "Judete",
                type: "character varying(35)",
                maxLength: 35,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(35)",
                oldMaxLength: 35);

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "Judete",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(5)",
                oldMaxLength: 5);

            migrationBuilder.AlterColumn<string>(
                name: "Simbol",
                table: "Conturi",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "Conturi",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "CoduriFunctionale",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "CoduriFunctionale",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "CoduriEconomice",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "CoduriEconomice",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "ClaseProduse",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "ClaseProduse",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Denumire",
                table: "Angajamente",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Cod",
                table: "Angajamente",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
