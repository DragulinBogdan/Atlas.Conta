-- Q03c (cub) Fisa 4111 CU contrapartida = celelalte conturi ale tranzactiei
-- (string_agg pe TranzactieId). Pe tranzactia cu mai multe picioare contrapartida nu e
-- unica -- de aceea e lista, nu un cont (docs/nucleu/nucleu-coordonate-rapoarte.md §3).
SELECT f."ID", f."Data", f."TranzactieId", f."Sens", f."Debit", f."Credit", f."SoldCurent",
       f."DocumentId", doc."Numar" AS "DocumentNumar", doc."ClrType" AS "DocumentTip", cp."Contrapartida"
FROM (
  SELECT p."ID", p."Data", p."TranzactieId", p."DocumentId",
         CASE WHEN p."Latura" = 1 THEN 'D' ELSE 'C' END AS "Sens",
         CASE WHEN p."Latura" = 1 THEN p."Valoare" ELSE 0.0 END AS "Debit",
         CASE WHEN p."Latura" = 2 THEN p."Valoare" ELSE 0.0 END AS "Credit",
         SUM(CASE WHEN p."Latura" = 1 THEN p."Valoare" ELSE -p."Valoare" END)
           OVER (ORDER BY p."Data", p."TranzactieId", p."ID"
                 ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS "SoldCurent"
  FROM {T} p
  WHERE p."Spatiu" = 1 AND p."Cont" = '01a0b48c-a8b2-73ce-a6cd-a045b80364a8' AND p."Data" <= DATE '2025-12-31'
) f
LEFT JOIN "Documente" doc ON doc."ID" = f."DocumentId" AND doc."GCRecord" = 0
LEFT JOIN (
  SELECT q."TranzactieId", string_agg(DISTINCT c2."Simbol", '+') AS "Contrapartida"
  FROM {T} q JOIN "Conturi" c2 ON c2."ID" = q."Cont"
  WHERE q."Spatiu" = 1 AND q."Cont" <> '01a0b48c-a8b2-73ce-a6cd-a045b80364a8'
    AND q."TranzactieId" IN (SELECT p2."TranzactieId" FROM {T} p2
                             WHERE p2."Spatiu" = 1 AND p2."Cont" = '01a0b48c-a8b2-73ce-a6cd-a045b80364a8'
                               AND p2."Data" <= DATE '2025-12-31' AND p2."Data" >= DATE '2025-01-01')
  GROUP BY q."TranzactieId") cp ON cp."TranzactieId" = f."TranzactieId"
WHERE f."Data" >= DATE '2025-01-01'
