EXPLAIN (ANALYZE, BUFFERS)
SELECT p."Gestiune", p."Unitate" AS "LotId",
       COALESCE(sum(p."Cantitate"), 0.0) AS "Cantitate",
       COALESCE(sum(p."Valoare"), 0.0) AS "Valoare"
FROM f1."Postare" p
WHERE p."Spatiu" = 2 AND p."Data" <= DATE '2025-12-31'
GROUP BY p."Gestiune", p."Unitate"
HAVING COALESCE(sum(p."Cantitate"), 0.0) <> 0.0;
