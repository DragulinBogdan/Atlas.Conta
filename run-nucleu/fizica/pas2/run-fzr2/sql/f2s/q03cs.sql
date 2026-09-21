EXPLAIN (ANALYZE, BUFFERS)
SELECT f."ID", f."Data", f."TranzactieId", f."Sens", f."Debit", f."Credit",
       si."Sold" + f."Rulaj" AS "SoldCurent",
       f."DocumentId", doc."Numar" AS "DocumentNumar", doc."ClrType" AS "DocumentTip", cp."Contrapartida"
FROM (
  SELECT p."ID", p."Data", p."TranzactieId", p."DocumentId",
         CASE WHEN p."Latura" = 1 THEN 'D' ELSE 'C' END AS "Sens",
         CASE WHEN p."Latura" = 1 THEN p."Valoare" ELSE 0.0 END AS "Debit",
         CASE WHEN p."Latura" = 2 THEN p."Valoare" ELSE 0.0 END AS "Credit",
         SUM(CASE WHEN p."Latura" = 1 THEN p."Valoare" ELSE -p."Valoare" END)
           OVER (ORDER BY p."Data", p."TranzactieId", p."ID"
                 ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS "Rulaj"
  FROM f2."Postare" p
  WHERE p."Spatiu" = 1 AND p."Cont" = '01a0b48c-a8b2-73ce-a6cd-a045b80364a8'
    AND p."Data" >= DATE '2025-01-01' AND p."Data" <= DATE '2025-12-31'
) f
CROSS JOIN (
  SELECT COALESCE(SUM(CASE WHEN s."Latura" = 1 THEN s."Valoare" ELSE -s."Valoare" END), 0.0) AS "Sold"
  FROM cub."Sold" s
  WHERE s."Spatiu" = 1 AND s."Granita" = DATE '2024-12-31' AND s."Cont" = '01a0b48c-a8b2-73ce-a6cd-a045b80364a8'
) si
LEFT JOIN "Documente" doc ON doc."ID" = f."DocumentId" AND doc."GCRecord" = 0
LEFT JOIN (
  SELECT q."TranzactieId", string_agg(DISTINCT c2."Simbol", '+') AS "Contrapartida"
  FROM f2."Postare" q JOIN "Conturi" c2 ON c2."ID" = q."Cont"
  WHERE q."Spatiu" = 1 AND q."Cont" <> '01a0b48c-a8b2-73ce-a6cd-a045b80364a8'
    AND q."TranzactieId" IN (SELECT p2."TranzactieId" FROM f2."Postare" p2
                             WHERE p2."Spatiu" = 1 AND p2."Cont" = '01a0b48c-a8b2-73ce-a6cd-a045b80364a8'
                               AND p2."Data" <= DATE '2025-12-31' AND p2."Data" >= DATE '2025-01-01')
  GROUP BY q."TranzactieId") cp ON cp."TranzactieId" = f."TranzactieId";
