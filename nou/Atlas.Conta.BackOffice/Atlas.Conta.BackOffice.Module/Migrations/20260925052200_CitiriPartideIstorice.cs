using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class CitiriPartideIstorice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE FUNCTION cub_partida_anterioara(document uuid, cont uuid) RETURNS uuid
                LANGUAGE plpgsql IMMUTABLE STRICT PARALLEL SAFE AS $body$
                DECLARE element uuid; raw bytea; intrare bytea := ''::bytea;
                BEGIN
                    FOREACH element IN ARRAY ARRAY[document, cont] LOOP
                        raw := uuid_send(element);
                        intrare := intrare || substring(raw FROM 4 FOR 1) || substring(raw FROM 3 FOR 1) || substring(raw FROM 2 FOR 1) || substring(raw FROM 1 FOR 1) || substring(raw FROM 6 FOR 1) || substring(raw FROM 5 FOR 1) || substring(raw FROM 8 FOR 1) || substring(raw FROM 7 FOR 1) || substring(raw FROM 9 FOR 1) || substring(raw FROM 10 FOR 1) || substring(raw FROM 11 FOR 1) || substring(raw FROM 12 FOR 1) || substring(raw FROM 13 FOR 1) || substring(raw FROM 14 FOR 1) || substring(raw FROM 15 FOR 1) || substring(raw FROM 16 FOR 1);
                    END LOOP;
                    raw := substring(sha256(intrare) FROM 1 FOR 16);
                    raw := set_byte(raw, 7, (get_byte(raw, 7) & 15) | 128);
                    RETURN encode(substring(raw FROM 4 FOR 1) || substring(raw FROM 3 FOR 1) || substring(raw FROM 2 FOR 1) || substring(raw FROM 1 FOR 1) || substring(raw FROM 6 FOR 1) || substring(raw FROM 5 FOR 1) || substring(raw FROM 8 FOR 1) || substring(raw FROM 7 FOR 1) || substring(raw FROM 9 FOR 1) || substring(raw FROM 10 FOR 1) || substring(raw FROM 11 FOR 1) || substring(raw FROM 12 FOR 1) || substring(raw FROM 13 FOR 1) || substring(raw FROM 14 FOR 1) || substring(raw FROM 15 FOR 1) || substring(raw FROM 16 FOR 1), 'hex')::uuid;
                END
                $body$;
                """);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION cub_partida_anterioara(uuid, uuid)");


        }
    }
}
