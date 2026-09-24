using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class FisaSiProvenientaPostarii : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE "Postare"
                    ADD COLUMN "FelUnitate" smallint NULL,
                    ADD COLUMN "InversaDinId" uuid NULL,
                    ADD COLUMN "InversaDinSpatiu" smallint NULL,
                    ADD COLUMN "SuportId" uuid NULL,
                    ADD COLUMN "SuportSpatiu" smallint NULL;
                UPDATE "Postare" SET "FelUnitate" = CASE WHEN "Spatiu" = 2 THEN 1 ELSE 2 END
                    WHERE "Unitate" IS NOT NULL;
                ALTER TABLE "Postare"
                    ADD CONSTRAINT "CK_Postare_FelUnitate" CHECK (
                        ("Unitate" IS NULL AND "FelUnitate" IS NULL AND "UnitateDeschisa" IS NULL)
                        OR ("Unitate" IS NOT NULL AND "UnitateDeschisa" IS NOT NULL AND "FelUnitate" IS NOT NULL
                            AND (("Spatiu" = 2 AND "FelUnitate" = 1)
                                OR ("Spatiu" = 1 AND "FelUnitate" IN (2, 3))))),
                    ADD CONSTRAINT "CK_Postare_Referinte" CHECK (
                        (("InversaDinId" IS NULL AND "InversaDinSpatiu" IS NULL)
                            OR ("InversaDinId" IS NOT NULL AND "InversaDinSpatiu" IS NOT NULL AND "InversaDinSpatiu" IN (1, 2)))
                        AND (("SuportId" IS NULL AND "SuportSpatiu" IS NULL)
                            OR ("SuportId" IS NOT NULL AND "SuportSpatiu" IS NOT NULL AND "SuportSpatiu" IN (1, 2))));
                CREATE INDEX "IX_Postare_Suport" ON "Postare" ("SuportSpatiu", "SuportId") WHERE "SuportId" IS NOT NULL;
                CREATE INDEX "IX_Postare_InversaDin" ON "Postare" ("InversaDinSpatiu", "InversaDinId") WHERE "InversaDinId" IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX "IX_Postare_Suport";
                DROP INDEX "IX_Postare_InversaDin";
                ALTER TABLE "Postare"
                    DROP CONSTRAINT "CK_Postare_FelUnitate",
                    DROP CONSTRAINT "CK_Postare_Referinte",
                    DROP COLUMN "FelUnitate",
                    DROP COLUMN "InversaDinId",
                    DROP COLUMN "InversaDinSpatiu",
                    DROP COLUMN "SuportId",
                    DROP COLUMN "SuportSpatiu";
                """);
        }
    }
}
