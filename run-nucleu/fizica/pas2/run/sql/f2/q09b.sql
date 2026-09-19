EXPLAIN (ANALYZE, BUFFERS)
SELECT p."Unitate" AS "LotId", COALESCE(sum(p."Cantitate"), 0.0) AS "Sold"
FROM f2."Postare" p
WHERE p."Spatiu" = 2 AND p."Produs" = '01a0b48d-78b6-754c-92e9-c3250a37995b' AND p."Gestiune" = '01a0b48c-d7ca-73c5-88b4-c0b508a78342'
  AND p."Data" <= DATE '2025-06-30'
GROUP BY p."Unitate"
HAVING COALESCE(sum(p."Cantitate"), 0.0) > 0.0;
