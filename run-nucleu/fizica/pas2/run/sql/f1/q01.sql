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
  SELECT p."Cont", 
    COALESCE(sum(CASE WHEN p."Data" <  DATE '2025-01-01' AND p."Latura" = 1 THEN p."Valoare" ELSE 0.0 END), 0.0) AS "InitialDebit",
    COALESCE(sum(CASE WHEN p."Data" <  DATE '2025-01-01' AND p."Latura" = 2 THEN p."Valoare" ELSE 0.0 END), 0.0) AS "InitialCredit",
    COALESCE(sum(CASE WHEN p."Data" >= DATE '2025-01-01' AND p."Latura" = 1 THEN p."Valoare" ELSE 0.0 END), 0.0) AS "RulajDebit",
    COALESCE(sum(CASE WHEN p."Data" >= DATE '2025-01-01' AND p."Latura" = 2 THEN p."Valoare" ELSE 0.0 END), 0.0) AS "RulajCredit"
  FROM f1."Postare" p
  WHERE p."Spatiu" = 1 AND p."Data" <= DATE '2025-12-31'
  GROUP BY p."Cont") g
LEFT JOIN (SELECT c."ID", c."Denumire", c."Simbol" FROM "Conturi" c WHERE c."GCRecord" = 0) c0 ON g."Cont" = c0."ID";
