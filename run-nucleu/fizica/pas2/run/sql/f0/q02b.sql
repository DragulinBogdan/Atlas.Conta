EXPLAIN (ANALYZE, BUFFERS)
SELECT u0."ContId", c0."Simbol" AS "ContSimbol", c0."Denumire" AS "ContDenumire",
       u0."PartenerId" AS "RepartitorId", r0."Denumire" AS "RepartitorDenumire",
       u0."InitialDebit", u0."InitialCredit",
       CASE WHEN u0."InitialDebit" - u0."InitialCredit" > 0.0 THEN u0."InitialDebit" - u0."InitialCredit" ELSE 0.0 END AS "SoldInitialDebit",
       CASE WHEN u0."InitialDebit" - u0."InitialCredit" < 0.0 THEN -(u0."InitialDebit" - u0."InitialCredit") ELSE 0.0 END AS "SoldInitialCredit",
       u0."RulajDebit", u0."RulajCredit",
       CASE WHEN ((u0."InitialDebit" - u0."InitialCredit") + u0."RulajDebit") - u0."RulajCredit" > 0.0 THEN ((u0."InitialDebit" - u0."InitialCredit") + u0."RulajDebit") - u0."RulajCredit" ELSE 0.0 END AS "SoldFinalDebit",
       CASE WHEN ((u0."InitialDebit" - u0."InitialCredit") + u0."RulajDebit") - u0."RulajCredit" < 0.0 THEN -(((u0."InitialDebit" - u0."InitialCredit") + u0."RulajDebit") - u0."RulajCredit") ELSE 0.0 END AS "SoldFinalCredit"
FROM (
    SELECT u."ContId", u."PartenerId",
        COALESCE(sum(CASE WHEN u."Data" <  DATE '2025-01-01' THEN u."Debit"  ELSE 0.0 END), 0.0) AS "InitialDebit",
        COALESCE(sum(CASE WHEN u."Data" <  DATE '2025-01-01' THEN u."Credit" ELSE 0.0 END), 0.0) AS "InitialCredit",
        COALESCE(sum(CASE WHEN u."Data" >= DATE '2025-01-01' THEN u."Debit"  ELSE 0.0 END), 0.0) AS "RulajDebit",
        COALESCE(sum(CASE WHEN u."Data" >= DATE '2025-01-01' THEN u."Credit" ELSE 0.0 END), 0.0) AS "RulajCredit"
    FROM (
        SELECT r."Data", r."ContDebitId" AS "ContId",
               COALESCE(CASE WHEN rd."ClrType" IN ('Partener','Angajat') THEN r."DimensiuniDebit_RepartitorId" END,
                        CASE WHEN rc."ClrType" IN ('Partener','Angajat') THEN r."DimensiuniCredit_RepartitorId" END) AS "PartenerId",
               r."Valoare" AS "Debit", 0.0 AS "Credit"
        FROM "RegistruContabil" AS r
        LEFT JOIN "Repartitori" rd ON rd."ID" = r."DimensiuniDebit_RepartitorId" AND rd."GCRecord" = 0
        LEFT JOIN "Repartitori" rc ON rc."ID" = r."DimensiuniCredit_RepartitorId" AND rc."GCRecord" = 0
        WHERE r."GCRecord" = 0
        UNION ALL
        SELECT r."Data", r."ContCreditId" AS "ContId",
               COALESCE(CASE WHEN rc."ClrType" IN ('Partener','Angajat') THEN r."DimensiuniCredit_RepartitorId" END,
                        CASE WHEN rd."ClrType" IN ('Partener','Angajat') THEN r."DimensiuniDebit_RepartitorId" END) AS "PartenerId",
               0.0 AS "Debit", r."Valoare" AS "Credit"
        FROM "RegistruContabil" AS r
        LEFT JOIN "Repartitori" rd ON rd."ID" = r."DimensiuniDebit_RepartitorId" AND rd."GCRecord" = 0
        LEFT JOIN "Repartitori" rc ON rc."ID" = r."DimensiuniCredit_RepartitorId" AND rc."GCRecord" = 0
        WHERE r."GCRecord" = 0
    ) AS u
    WHERE u."Data" <= DATE '2025-12-31'
    GROUP BY u."ContId", u."PartenerId"
) AS u0
LEFT JOIN (SELECT c."ID", c."Denumire", c."Simbol" FROM "Conturi" AS c WHERE c."GCRecord" = 0) AS c0 ON u0."ContId" = c0."ID"
LEFT JOIN (SELECT rp."ID", rp."Denumire" FROM "Repartitori" AS rp WHERE rp."GCRecord" = 0) AS r0 ON u0."PartenerId" = r0."ID"
WHERE c0."Simbol" IN ('4111', '401');
