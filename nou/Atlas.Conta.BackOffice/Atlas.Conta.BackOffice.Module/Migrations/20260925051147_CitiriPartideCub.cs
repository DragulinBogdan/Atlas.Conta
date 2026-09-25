using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class CitiriPartideCub : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE FUNCTION cub_partida_id(document uuid, cont uuid, partener uuid) RETURNS uuid
                LANGUAGE plpgsql IMMUTABLE STRICT PARALLEL SAFE AS $body$
                DECLARE element uuid; raw bytea; intrare bytea := ''::bytea;
                BEGIN
                    FOREACH element IN ARRAY ARRAY[document, cont, partener] LOOP
                        raw := uuid_send(element);
                        intrare := intrare || substring(raw FROM 4 FOR 1) || substring(raw FROM 3 FOR 1) || substring(raw FROM 2 FOR 1) || substring(raw FROM 1 FOR 1) || substring(raw FROM 6 FOR 1) || substring(raw FROM 5 FOR 1) || substring(raw FROM 8 FOR 1) || substring(raw FROM 7 FOR 1) || substring(raw FROM 9 FOR 1) || substring(raw FROM 10 FOR 1) || substring(raw FROM 11 FOR 1) || substring(raw FROM 12 FOR 1) || substring(raw FROM 13 FOR 1) || substring(raw FROM 14 FOR 1) || substring(raw FROM 15 FOR 1) || substring(raw FROM 16 FOR 1);
                    END LOOP;
                    raw := substring(sha256(intrare) FROM 1 FOR 16);
                    raw := set_byte(raw, 7, (get_byte(raw, 7) & 15) | 128);
                    RETURN encode(substring(raw FROM 4 FOR 1) || substring(raw FROM 3 FOR 1) || substring(raw FROM 2 FOR 1) || substring(raw FROM 1 FOR 1) || substring(raw FROM 6 FOR 1) || substring(raw FROM 5 FOR 1) || substring(raw FROM 8 FOR 1) || substring(raw FROM 7 FOR 1) || substring(raw FROM 9 FOR 1) || substring(raw FROM 10 FOR 1) || substring(raw FROM 11 FOR 1) || substring(raw FROM 12 FOR 1) || substring(raw FROM 13 FOR 1) || substring(raw FROM 14 FOR 1) || substring(raw FROM 15 FOR 1) || substring(raw FROM 16 FOR 1), 'hex')::uuid;
                END
                $body$;
                """);

            migrationBuilder.DropIndex(
                name: "IX_PartideDeschise_An_Luna_DocumentId",
                table: "PartideDeschise");

            migrationBuilder.AlterColumn<Guid>(
                name: "DocumentId",
                table: "PartideDeschise",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "ContId",
                table: "PartideDeschise",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<decimal>(
                name: "Credit",
                table: "PartideDeschise",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "Debit",
                table: "PartideDeschise",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateOnly>(
                name: "Deschisa",
                table: "PartideDeschise",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.AddColumn<bool>(
                name: "DinCub",
                table: "PartideDeschise",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "PartenerId",
                table: "PartideDeschise",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "UnitateId",
                table: "PartideDeschise",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_PartideDeschise_An_Luna_UnitateId_ContId_PartenerId",
                table: "PartideDeschise",
                columns: new[] { "An", "Luna", "UnitateId", "ContId", "PartenerId" },
                unique: true, filter: "\"DinCub\"");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"PartideDeschise\" WHERE \"DinCub\"");
            migrationBuilder.Sql("DROP FUNCTION cub_partida_id(uuid, uuid, uuid)");

            migrationBuilder.DropIndex(
                name: "IX_PartideDeschise_An_Luna_UnitateId_ContId_PartenerId",
                table: "PartideDeschise");

            migrationBuilder.DropColumn(
                name: "ContId",
                table: "PartideDeschise");

            migrationBuilder.DropColumn(
                name: "Credit",
                table: "PartideDeschise");

            migrationBuilder.DropColumn(
                name: "Debit",
                table: "PartideDeschise");

            migrationBuilder.DropColumn(
                name: "Deschisa",
                table: "PartideDeschise");

            migrationBuilder.DropColumn(
                name: "DinCub",
                table: "PartideDeschise");

            migrationBuilder.DropColumn(
                name: "PartenerId",
                table: "PartideDeschise");

            migrationBuilder.DropColumn(
                name: "UnitateId",
                table: "PartideDeschise");

            migrationBuilder.AlterColumn<Guid>(
                name: "DocumentId",
                table: "PartideDeschise",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartideDeschise_An_Luna_DocumentId",
                table: "PartideDeschise",
                columns: new[] { "An", "Luna", "DocumentId" },
                unique: true);
        }
    }
}
