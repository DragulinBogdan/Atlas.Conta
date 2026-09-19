-- B4. Avansul: 419/409 pe tipuri; FCL stinse de un document cu 419;
-- "rest brut" (F0) contra "rest compensat" (cub, suma pe TOATE conturile de tert).
\set ON_ERROR_STOP on
\pset footer off

\echo ===== B4.1 documente cu postare pe 419/409x, pe tip =====
SELECT d."ClrType", n."ContSimbol",
       count(DISTINCT d."ID") AS documente,
       sum(n."Debit") AS debit, sum(n."Credit") AS credit
FROM tr."TertDocContNet" n JOIN tr."Doc" d ON d."ID" = n."DocumentId"
WHERE n."ContSimbol" LIKE '419%' OR n."ContSimbol" LIKE '409%'
GROUP BY 1,2 ORDER BY 1,2;

\echo
\echo ===== B4.2 un exemplu de FCL cu 419: toate picioarele =====
WITH ex AS (
  SELECT n."DocumentId" FROM tr."TertDocContNet" n JOIN tr."Doc" d ON d."ID"=n."DocumentId"
  WHERE n."ContSimbol"='419' AND d."ClrType"='FacturaIesire' ORDER BY 1 LIMIT 1)
SELECT d."Numar", d."TotalStingere", p."ContSimbol", p."Latura", sum(p."Valoare") AS valoare
FROM ex JOIN tr."Doc" d ON d."ID"=ex."DocumentId"
JOIN tr."Picior" p ON p."DocumentId"=d."ID"
GROUP BY 1,2,3,4 ORDER BY p."ContSimbol", p."Latura";

\echo
\echo ===== B4.3 FCL cu imperechere al carei STINGATOR posteaza pe 419 =====
SELECT count(DISTINCT i."DocumentId") AS fcl_stinse_de_avans,
       count(*) AS imperecheri, sum(i."Suma") AS suma,
       count(DISTINCT i."DocumentStingatorId") AS stingatori,
       string_agg(DISTINCT dg."ClrType", ',') AS tipuri_stingator
FROM "Imperecheri" i
JOIN tr."Doc" ds ON ds."ID" = i."DocumentId" AND ds."ClrType" = 'FacturaIesire'
JOIN tr."Doc" dg ON dg."ID" = i."DocumentStingatorId"
WHERE EXISTS (SELECT 1 FROM tr."TertDocContNet" n
              WHERE n."DocumentId" = i."DocumentStingatorId" AND n."ContSimbol" = '419');

\echo
\echo ===== B4.4 rest brut (F0) contra rest compensat (cub) pe FCL cu 419 propriu =====
-- Conventia de semn: pentru un cont cu RolTert=1 (client) restul "natural" e
-- soldul DEBITOR; pentru RolTert=2 (furnizor) e soldul CREDITOR.
WITH fcl AS (
  SELECT d."ID", d."Numar", d."TotalStingere",
         d."TotalStingere" - COALESCE(a."Asignat", 0) AS rest_f0,
         COALESCE((SELECT sum(CASE WHEN n."RolTert" = 1 THEN n."NetDebitor" ELSE -n."NetDebitor" END)
                   FROM tr."TertDocContNet" n WHERE n."DocumentId" = d."ID"), 0) AS rest_cub
  FROM tr."Doc" d LEFT JOIN tr."Asignat" a ON a."DocumentId" = d."ID"
  WHERE d."ClrType" = 'FacturaIesire' AND d."Stare" = 1
    AND EXISTS (SELECT 1 FROM tr."TertDocContNet" n
                WHERE n."DocumentId" = d."ID" AND n."ContSimbol" = '419')
)
SELECT count(*) AS facturi, sum("TotalStingere") AS total, sum(rest_f0) AS rest_brut_f0,
       sum(rest_cub) AS rest_compensat_cub,
       count(*) FILTER (WHERE rest_f0 <> 0) AS cu_rest_f0,
       count(*) FILTER (WHERE rest_cub <> 0) AS cu_rest_cub
FROM fcl;

\echo
\echo ===== B4.5 acelasi, la 31.12.2025 si numai pe cele cu rest =====
WITH fcl AS (
  SELECT d."ID", d."TotalStingere" - COALESCE(a."Asignat", 0) AS rest_f0,
         COALESCE((SELECT sum(CASE WHEN n."RolTert" = 1 THEN n."NetDebitor" ELSE -n."NetDebitor" END)
                   FROM tr."TertDocContNet" n WHERE n."DocumentId" = d."ID"), 0) AS rest_cub
  FROM tr."Doc" d LEFT JOIN tr."Asignat" a ON a."DocumentId" = d."ID"
  WHERE d."ClrType" = 'FacturaIesire' AND d."Stare" = 1 AND d."DataInregistrare" <= DATE '2025-12-31'
    AND EXISTS (SELECT 1 FROM tr."TertDocContNet" n
                WHERE n."DocumentId" = d."ID" AND n."ContSimbol" = '419')
)
SELECT count(*) FILTER (WHERE rest_f0 <> 0) AS facturi_cu_rest_f0,
       sum(rest_f0) FILTER (WHERE rest_f0 <> 0) AS suma_rest_f0,
       avg(rest_f0) FILTER (WHERE rest_f0 <> 0) AS medie_rest_f0,
       count(*) FILTER (WHERE rest_cub <> 0) AS facturi_cu_rest_cub,
       sum(rest_cub) FILTER (WHERE rest_cub <> 0) AS suma_rest_cub,
       avg(rest_cub) FILTER (WHERE rest_cub <> 0) AS medie_rest_cub
FROM fcl;
