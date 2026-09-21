-- Q04 (cub) Fisa 4111 x partenerul cel mai activ pe 4111 IN CUB.
-- ales: CONSUMATOR FINAL = 01a0b48c-a9fe-73e1-89f8-2908bb52cd65, 3.034 postari din 144.241.
-- Referinta de egalitate: f0/q04b.sql (acelasi partener, luat de pe oricare latura).
-- f0/q04.sql masoara alt caz (Sediul central, o UnitateInterna, 107.033 randuri) --
-- cifrele nu se compara intre ele, de aceea q04b se masoara separat.
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
  FROM f2."Postare" p
  WHERE p."Spatiu" = 1 AND p."Cont" = '01a0b48c-a8b2-73ce-a6cd-a045b80364a8' AND p."Data" <= DATE '2025-12-31'
    AND p."Partener" = '01a0b48c-a9fe-73e1-89f8-2908bb52cd65'
) f
LEFT JOIN "Documente" doc ON doc."ID" = f."DocumentId" AND doc."GCRecord" = 0
WHERE f."Data" >= DATE '2025-01-01'
