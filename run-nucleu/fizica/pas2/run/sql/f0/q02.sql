EXPLAIN (ANALYZE, BUFFERS)
SELECT u0."ContId", c0."Simbol" AS "ContSimbol", c0."Denumire" AS "ContDenumire",
       u0."RepartitorId", r0."Denumire" AS "RepartitorDenumire",
       u0."InitialDebit", u0."InitialCredit",
       CASE WHEN u0."InitialDebit" - u0."InitialCredit" > 0.0 THEN u0."InitialDebit" - u0."InitialCredit" ELSE 0.0 END AS "SoldInitialDebit",
       CASE WHEN u0."InitialDebit" - u0."InitialCredit" < 0.0 THEN -(u0."InitialDebit" - u0."InitialCredit") ELSE 0.0 END AS "SoldInitialCredit",
       u0."RulajDebit", u0."RulajCredit",
       CASE WHEN ((u0."InitialDebit" - u0."InitialCredit") + u0."RulajDebit") - u0."RulajCredit" > 0.0 THEN ((u0."InitialDebit" - u0."InitialCredit") + u0."RulajDebit") - u0."RulajCredit" ELSE 0.0 END AS "SoldFinalDebit",
       CASE WHEN ((u0."InitialDebit" - u0."InitialCredit") + u0."RulajDebit") - u0."RulajCredit" < 0.0 THEN -(((u0."InitialDebit" - u0."InitialCredit") + u0."RulajDebit") - u0."RulajCredit") ELSE 0.0 END AS "SoldFinalCredit"
FROM (
    SELECT u."ContId", u."RepartitorId",
        COALESCE(sum(CASE WHEN u."Data" < '2025-01-01' THEN u."Debit"  ELSE 0.0 END), 0.0) AS "InitialDebit",
        COALESCE(sum(CASE WHEN u."Data" < '2025-01-01' THEN u."Credit" ELSE 0.0 END), 0.0) AS "InitialCredit",
        COALESCE(sum(CASE WHEN u."Data" >= '2025-01-01' THEN u."Debit"  ELSE 0.0 END), 0.0) AS "RulajDebit",
        COALESCE(sum(CASE WHEN u."Data" >= '2025-01-01' THEN u."Credit" ELSE 0.0 END), 0.0) AS "RulajCredit"
    FROM (
        SELECT r."Data", r."ContDebitId" AS "ContId", r."DimensiuniDebit_RepartitorId" AS "RepartitorId", r."Valoare" AS "Debit", 0.0 AS "Credit"
        FROM "RegistruContabil" AS r
        WHERE r."GCRecord" = 0
        UNION ALL
        SELECT r0."Data", r0."ContCreditId" AS "ContId", r0."DimensiuniCredit_RepartitorId" AS "RepartitorId", 0.0 AS "Debit", r0."Valoare" AS "Credit"
        FROM "RegistruContabil" AS r0
        WHERE r0."GCRecord" = 0
    ) AS u
    WHERE u."Data" <= '2025-12-31'
    GROUP BY u."ContId", u."RepartitorId"
) AS u0
LEFT JOIN (SELECT c."ID", c."Denumire", c."Simbol" FROM "Conturi" AS c WHERE c."GCRecord" = 0) AS c0 ON u0."ContId" = c0."ID"
LEFT JOIN (SELECT rp."ID", rp."Denumire" FROM "Repartitori" AS rp WHERE rp."GCRecord" = 0) AS r0 ON u0."RepartitorId" = r0."ID"
WHERE c0."Simbol" IN ('4111', '401');
