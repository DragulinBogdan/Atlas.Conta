EXPLAIN (ANALYZE, BUFFERS)
SELECT p."ID", p."LinieId", p."DocumentId", p."CodTvaId", p."RolTva", p."Valoare",
       p."PerioadaDeclarare", ct."Regim", ct."Cota", ct."Sens"
FROM f2."Postare" p
JOIN cub."CodTva" ct ON ct."ID" = p."CodTvaId"
WHERE p."Spatiu" = 3
  AND p."LinieId" IN (
    SELECT c."LinieId" FROM f2."Postare" c
    WHERE c."Spatiu" = 1 AND c."Data" >= DATE '2025-06-01' AND c."Data" <= DATE '2025-06-30'
      AND c."LinieId" IS NOT NULL);
