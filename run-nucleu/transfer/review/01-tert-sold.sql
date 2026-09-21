-- R1: soldul real al conturilor de terț la 31.12.2025 (din registrul contabil, nu din cub)
WITH picior AS (
  SELECT "ContDebitId" AS cont,  "Valoare" AS v FROM "RegistruContabil" WHERE "Data" <= DATE '2025-12-31'
  UNION ALL
  SELECT "ContCreditId",        -"Valoare"     FROM "RegistruContabil" WHERE "Data" <= DATE '2025-12-31'
)
SELECT c."Simbol", c."RolTert", round(sum(p.v),2) AS sold_debitor_pozitiv
FROM picior p JOIN "Conturi" c ON c."ID" = p.cont
WHERE c."RolTert" <> 0
GROUP BY 1,2
ORDER BY 1;
