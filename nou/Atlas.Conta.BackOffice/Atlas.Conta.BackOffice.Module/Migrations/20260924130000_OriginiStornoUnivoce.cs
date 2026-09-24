using Atlas.Conta.BackOffice.Module.BusinessObjects;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Atlas.Conta.BackOffice.Module.Migrations;

[DbContext(typeof(BackOfficeEFCoreDbContext))]
[Migration("20260924130000_OriginiStornoUnivoce")]
public class OriginiStornoUnivoce : Migration {
    public const string Completeaza = """
        WITH candidati AS (
            SELECT i."ID" AS inversa, i."Spatiu" AS spatiu_inversa,
                   o."ID" AS original, o."Spatiu" AS spatiu_original,
                   count(*) OVER (PARTITION BY i."ID", i."Spatiu") AS originale,
                   count(*) OVER (PARTITION BY o."ID", o."Spatiu") AS inverse
            FROM "Postare" i
            JOIN "Tranzactie" ti ON ti."ID" = i."TranzactieId"
            JOIN "Postare" o ON o."DocumentId" = i."DocumentId"
                AND o."Spatiu" = i."Spatiu"
            JOIN "Tranzactie" tor ON tor."ID" = o."TranzactieId"
            WHERE ti."Fel" = 2 AND tor."Fel" <> 2
              AND i."InversaDinId" IS NULL AND i."InversaDinSpatiu" IS NULL
              AND o."Data" <= i."Data" AND tor."ScrisLa" <= ti."ScrisLa"
              AND o."Cantitate" = -i."Cantitate"
              AND o."Valoare" = -i."Valoare" AND o."ValoareValuta" = -i."ValoareValuta"
              AND (o."PerioadaDeclarare" IS NULL) = (i."PerioadaDeclarare" IS NULL)
              AND to_jsonb(o) - ARRAY['ID','TranzactieId','Data','InversaDinId','InversaDinSpatiu',
                      'Cantitate','Valoare','ValoareValuta','PerioadaDeclarare']
                  = to_jsonb(i) - ARRAY['ID','TranzactieId','Data','InversaDinId','InversaDinSpatiu',
                      'Cantitate','Valoare','ValoareValuta','PerioadaDeclarare']
              AND NOT EXISTS (SELECT 1 FROM "Postare" deja
                  WHERE deja."InversaDinId" = o."ID" AND deja."InversaDinSpatiu" = o."Spatiu")
        )
        UPDATE "Postare" p SET "InversaDinId" = c.original, "InversaDinSpatiu" = c.spatiu_original
        FROM candidati c
        WHERE c.originale = 1 AND c.inverse = 1 AND p."ID" = c.inversa AND p."Spatiu" = c.spatiu_inversa;
        """;

    protected override void Up(MigrationBuilder migrationBuilder) => migrationBuilder.Sql(Completeaza);

    protected override void Down(MigrationBuilder migrationBuilder) { }
}
