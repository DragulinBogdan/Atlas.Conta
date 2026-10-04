using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Atlas.Conta.BackOffice.Module.Migrations
{
    /// <inheritdoc />
    public partial class IndecsiCititoriCub : Migration
    {
        // S-r4: migrațiile cubului se scriu în SQL.
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                CREATE INDEX "IX_Postare_Stoc_Data" ON "Postare_Stoc" ("Data");
                CREATE INDEX "IX_Postare_Stoc_PerioadaDeclarare_TipTvaId" ON "Postare_Stoc"
                    ("PerioadaDeclarare", "TipTvaId") WHERE "PerioadaDeclarare" IS NOT NULL;
                CREATE INDEX "IX_Postare_PerioadaD394" ON "Postare" ("PerioadaD394") WHERE "PerioadaD394" IS NOT NULL;
                CREATE INDEX "IX_Postare_DataExigibilitate" ON "Postare" ("DataExigibilitate") WHERE "DataExigibilitate" IS NOT NULL;
                CREATE INDEX "IX_Postare_Unitate_Data" ON "Postare" ("Unitate", "Data") WHERE "Unitate" IS NOT NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX "IX_Postare_Unitate_Data";
                DROP INDEX "IX_Postare_DataExigibilitate";
                DROP INDEX "IX_Postare_PerioadaD394";
                DROP INDEX "IX_Postare_Stoc_PerioadaDeclarare_TipTvaId";
                DROP INDEX "IX_Postare_Stoc_Data";
                """);
        }
    }
}
