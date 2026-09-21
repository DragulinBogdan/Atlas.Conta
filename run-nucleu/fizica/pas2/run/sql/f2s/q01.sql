EXPLAIN (ANALYZE, BUFFERS)
SELECT g."Cont" AS "ContId", c0."Simbol" AS "ContSimbol", c0."Denumire" AS "ContDenumire",
       NULL AS "RepartitorId", NULL AS "RepartitorDenumire",
       g."InitialDebit", g."InitialCredit",
       CASE WHEN g."InitialDebit" - g."InitialCredit" > 0.0 THEN g."InitialDebit" - g."InitialCredit" ELSE 0.0 END AS "SoldInitialDebit",
       CASE WHEN g."InitialDebit" - g."InitialCredit" < 0.0 THEN -(g."InitialDebit" - g."InitialCredit") ELSE 0.0 END AS "SoldInitialCredit",
       g."RulajDebit", g."RulajCredit",
       CASE WHEN ((g."InitialDebit" - g."InitialCredit") + g."RulajDebit") - g."RulajCredit" > 0.0 THEN ((g."InitialDebit" - g."InitialCredit") + g."RulajDebit") - g."RulajCredit" ELSE 0.0 END AS "SoldFinalDebit",
       CASE WHEN ((g."InitialDebit" - g."InitialCredit") + g."RulajDebit") - g."RulajCredit" < 0.0 THEN -(((g."InitialDebit" - g."InitialCredit") + g."RulajDebit") - g."RulajCredit") ELSE 0.0 END AS "SoldFinalCredit"
FROM (
  SELECT u."Cont", 
    COALESCE(sum(u."ID_"), 0.0) AS "InitialDebit", COALESCE(sum(u."IC_"), 0.0) AS "InitialCredit",
    COALESCE(sum(u."RD_"), 0.0) AS "RulajDebit",   COALESCE(sum(u."RC_"), 0.0) AS "RulajCredit"
  FROM (
    SELECT s."Cont", 
           CASE WHEN s."Latura" = 1 THEN s."Valoare" ELSE 0.0 END AS "ID_",
           CASE WHEN s."Latura" = 2 THEN s."Valoare" ELSE 0.0 END AS "IC_",
           0.0 AS "RD_", 0.0 AS "RC_"
    FROM cub."Sold" s WHERE s."Spatiu" = 1 AND s."Granita" = DATE '2024-12-31'
    UNION ALL
    SELECT p."Cont", 0.0, 0.0,
           CASE WHEN p."Latura" = 1 THEN p."Valoare" ELSE 0.0 END,
           CASE WHEN p."Latura" = 2 THEN p."Valoare" ELSE 0.0 END
    FROM f2."Postare" p
    WHERE p."Spatiu" = 1 AND p."Data" >= DATE '2025-01-01' AND p."Data" <= DATE '2025-12-31'
  ) u GROUP BY u."Cont") g
LEFT JOIN (SELECT c."ID", c."Denumire", c."Simbol" FROM "Conturi" c WHERE c."GCRecord" = 0) c0 ON g."Cont" = c0."ID";
