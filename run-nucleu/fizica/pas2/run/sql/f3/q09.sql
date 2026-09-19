EXPLAIN (ANALYZE, BUFFERS)
SELECT p."Unitate" AS "LotId",
       COALESCE(sum(p."Cantitate"), 0.0) AS "Sold",
       COALESCE(sum(p."Valoare"), 0.0) AS "Valoare",
       min(p."Data") AS "PrimaMiscare"
FROM f3."Postare" p
WHERE p."Spatiu" = 2 AND p."Gestiune" = '01a0b48c-d7ca-73c5-88b4-c0b508a78342' AND p."Data" <= DATE '2025-06-30'
GROUP BY p."Unitate"
HAVING COALESCE(sum(p."Cantitate"), 0.0) > 0.0
ORDER BY min(p."Data"), p."Unitate";
