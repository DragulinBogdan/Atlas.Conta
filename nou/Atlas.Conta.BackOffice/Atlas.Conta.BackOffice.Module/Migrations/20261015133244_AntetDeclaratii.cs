using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class AntetDeclaratii : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Caen",
                table: "Societati",
                type: "character varying(4)",
                maxLength: 4,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeclarantAdresa",
                table: "Societati",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeclarantFunctie",
                table: "Societati",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeclarantNume",
                table: "Societati",
                type: "character varying(75)",
                maxLength: 75,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeclarantPrenume",
                table: "Societati",
                type: "character varying(75)",
                maxLength: 75,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IntocmitCalitate",
                table: "Societati",
                type: "character varying(75)",
                maxLength: 75,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IntocmitCif",
                table: "Societati",
                type: "character varying(13)",
                maxLength: 13,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "IntocmitDe",
                table: "Societati",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "IntocmitDenumire",
                table: "Societati",
                type: "character varying(75)",
                maxLength: 75,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OptiuneConsultareD394",
                table: "Societati",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PeriodicitateTva",
                table: "Societati",
                type: "integer",
                nullable: false,
                defaultValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Caen",
                table: "Societati");

            migrationBuilder.DropColumn(
                name: "DeclarantAdresa",
                table: "Societati");

            migrationBuilder.DropColumn(
                name: "DeclarantFunctie",
                table: "Societati");

            migrationBuilder.DropColumn(
                name: "DeclarantNume",
                table: "Societati");

            migrationBuilder.DropColumn(
                name: "DeclarantPrenume",
                table: "Societati");

            migrationBuilder.DropColumn(
                name: "IntocmitCalitate",
                table: "Societati");

            migrationBuilder.DropColumn(
                name: "IntocmitCif",
                table: "Societati");

            migrationBuilder.DropColumn(
                name: "IntocmitDe",
                table: "Societati");

            migrationBuilder.DropColumn(
                name: "IntocmitDenumire",
                table: "Societati");

            migrationBuilder.DropColumn(
                name: "OptiuneConsultareD394",
                table: "Societati");

            migrationBuilder.DropColumn(
                name: "PeriodicitateTva",
                table: "Societati");
        }
    }
}
