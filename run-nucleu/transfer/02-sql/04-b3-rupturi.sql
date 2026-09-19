-- B3. Cele doua rupturi ale amendamentului 4 (imperecherea ca tranzactie),
-- re-derivate din `tr` (nu din cub."_TertDoc", care alege UNA din postari).
\set ON_ERROR_STOP on
\pset footer off

-- Partida de referinta a unui document, exact ca in cub (DISTINCT ON,
-- |Valoare| maxima) — ca cifrele sa fie comparabile cu review-ul.
DROP TABLE IF EXISTS tr."TertRef";
CREATE UNLOGGED TABLE tr."TertRef" AS
SELECT DISTINCT ON (n."DocumentId")
       n."DocumentId", n."Cont", n."ContSimbol", n."RolTert", n."Partener",
       CASE WHEN n."Debit" >= n."Credit" THEN 1 ELSE 2 END AS "Latura",
       n."NetDebitor",
       (SELECT count(*) FROM tr."TertDocContNet" m WHERE m."DocumentId" = n."DocumentId") AS nr_conturi
FROM tr."TertDocContNet" n
ORDER BY n."DocumentId", abs(n."NetDebitor") DESC, n."Cont";
ALTER TABLE tr."TertRef" ADD PRIMARY KEY ("DocumentId");
ANALYZE tr."TertRef";

\echo ===== B3.1 rezumat (oglinda probei M5) =====
SELECT count(*) AS total,
       count(*) FILTER (WHERE s."Cont" IS DISTINCT FROM f."Cont")         AS cont_diferit,
       count(*) FILTER (WHERE s."Latura" IS DISTINCT FROM f."Latura")     AS latura_diferita,
       count(*) FILTER (WHERE f."DocumentId" IS NULL)                     AS fara_tert_pe_stins,
       count(*) FILTER (WHERE s."DocumentId" IS NULL)                     AS fara_tert_pe_stingator,
       count(*) FILTER (WHERE s."Partener" IS DISTINCT FROM f."Partener") AS partener_diferit
FROM "Imperecheri" i
LEFT JOIN tr."TertRef" s ON s."DocumentId" = i."DocumentStingatorId"
LEFT JOIN tr."TertRef" f ON f."DocumentId" = i."DocumentId";

\echo
\echo ===== B3.2 contul stins <> contul stingatorului: (tip stins, tip stingator, cont, cont) =====
SELECT ds."ClrType" AS tip_stins, dg."ClrType" AS tip_stingator,
       f."ContSimbol" AS cont_stins, s."ContSimbol" AS cont_stingator,
       count(*) AS numar, sum(i."Suma") AS suma
FROM "Imperecheri" i
JOIN tr."TertRef" s ON s."DocumentId" = i."DocumentStingatorId"
JOIN tr."TertRef" f ON f."DocumentId" = i."DocumentId"
JOIN tr."Doc" ds ON ds."ID" = i."DocumentId"
JOIN tr."Doc" dg ON dg."ID" = i."DocumentStingatorId"
WHERE s."Cont" IS DISTINCT FROM f."Cont"
GROUP BY 1,2,3,4 ORDER BY numar DESC;

\echo
\echo ===== B3.3 documente STINSE fara nicio postare de tert: pe tip =====
SELECT ds."ClrType" AS tip_stins, count(*) AS imperecheri,
       count(DISTINCT i."DocumentId") AS documente, sum(i."Suma") AS suma
FROM "Imperecheri" i
JOIN tr."Doc" ds ON ds."ID" = i."DocumentId"
LEFT JOIN tr."TertRef" f ON f."DocumentId" = i."DocumentId"
WHERE f."DocumentId" IS NULL
GROUP BY 1 ORDER BY 2 DESC;

\echo
\echo ===== B3.4 trei exemple de document stins fara postare de tert + ce posteaza =====
WITH ex AS (
  SELECT DISTINCT i."DocumentId" FROM "Imperecheri" i
  LEFT JOIN tr."TertRef" f ON f."DocumentId" = i."DocumentId"
  WHERE f."DocumentId" IS NULL
  ORDER BY 1 LIMIT 3)
SELECT d."ClrType", d."Numar", d."Data", d."TotalStingere",
       p."ContSimbol", p."Latura", sum(p."Valoare") AS valoare
FROM ex JOIN tr."Doc" d ON d."ID" = ex."DocumentId"
LEFT JOIN tr."Picior" p ON p."DocumentId" = d."ID"
GROUP BY 1,2,3,4,5,6 ORDER BY d."Numar", p."ContSimbol", p."Latura";

\echo
\echo ===== B3.5 latura: cum se distribuie (stins x stingator) =====
SELECT f."ContSimbol" AS cont_stins, f."Latura" AS latura_stins,
       s."ContSimbol" AS cont_stingator, s."Latura" AS latura_stingator,
       count(*) AS numar
FROM "Imperecheri" i
JOIN tr."TertRef" s ON s."DocumentId" = i."DocumentStingatorId"
JOIN tr."TertRef" f ON f."DocumentId" = i."DocumentId"
GROUP BY 1,2,3,4 ORDER BY numar DESC LIMIT 20;
