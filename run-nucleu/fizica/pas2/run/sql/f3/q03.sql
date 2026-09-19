EXPLAIN (ANALYZE, BUFFERS)
SELECT f."ID", f."Data", f."TranzactieId", f."Sens", f."Debit", f."Credit", f."SoldCurent",
       f."DocumentId", doc."Numar" AS "DocumentNumar", doc."ClrType" AS "DocumentTip"
FROM (
  SELECT p."ID", p."Data", p."TranzactieId", p."DocumentId",
         CASE WHEN p."Latura" = 1 THEN 'D' ELSE 'C' END AS "Sens",
         CASE WHEN p."Latura" = 1 THEN p."Valoare" ELSE 0.0 END AS "Debit",
         CASE WHEN p."Latura" = 2 THEN p."Valoare" ELSE 0.0 END AS "Credit",
         SUM(CASE WHEN p."Latura" = 1 THEN p."Valoare" ELSE -p."Valoare" END)
           OVER (ORDER BY p."Data", p."TranzactieId", p."ID"
                 ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS "SoldCurent"
  FROM f3."Postare" p
  WHERE p."Spatiu" = 1 AND p."Cont" = '01a0b48c-a8b2-73ce-a6cd-a045b80364a8' AND p."Data" <= DATE '2025-12-31'
) f
LEFT JOIN "Documente" doc ON doc."ID" = f."DocumentId" AND doc."GCRecord" = 0
WHERE f."Data" >= DATE '2025-01-01';
