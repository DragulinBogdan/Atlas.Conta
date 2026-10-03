using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class ExplicatieTranzactie : Migration
    {
        // S-r4: migrațiile cubului se scriu în SQL.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Tranzactie"
                    ADD COLUMN "Explicatie" jsonb NULL,
                    ADD COLUMN "ExplicatieDinId" uuid NULL,
                    ADD CONSTRAINT "CK_Tranzactie_Explicatie" CHECK (
                        "Explicatie" IS NULL OR ("ExplicatieDinId" IS NULL AND "DocumentId" IS NOT NULL AND "Fel" IN (1, 3))),
                    ADD CONSTRAINT "CK_Tranzactie_ExplicatieDin" CHECK (
                        "ExplicatieDinId" IS NULL OR ("DocumentId" IS NOT NULL AND "Fel" = 3)),
                    ADD CONSTRAINT "FK_Tranzactie_Tranzactie_ExplicatieDinId"
                        FOREIGN KEY ("ExplicatieDinId") REFERENCES "Tranzactie" ("ID");
                CREATE INDEX "IX_Tranzactie_ExplicatieDinId" ON "Tranzactie" ("ExplicatieDinId");
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX "IX_Tranzactie_ExplicatieDinId";
                ALTER TABLE "Tranzactie"
                    DROP CONSTRAINT "FK_Tranzactie_Tranzactie_ExplicatieDinId",
                    DROP CONSTRAINT "CK_Tranzactie_ExplicatieDin",
                    DROP CONSTRAINT "CK_Tranzactie_Explicatie",
                    DROP COLUMN "ExplicatieDinId",
                    DROP COLUMN "Explicatie";
                """);
        }
    }
}
