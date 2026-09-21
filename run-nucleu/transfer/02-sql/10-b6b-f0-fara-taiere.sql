-- B6b. `PartideDeschise` 12/2025 e otravita de artefactul de data (toate
-- `Imperecheri.Data` = 2026-09-18, deci `AsignariPanaLa('2025-12-31')` nu
-- gaseste NICIO asignare): restul lui F0 la 12/2025 e chiar `TotalStingere`.
-- Aici se reface comparatia cu F0 calculat ALGEBRIC, fara taierea pe data.
\set ON_ERROR_STOP on
\pset footer off

\echo ===== B6b.0 proba artefactului: PartideDeschise = TotalStingere? =====
SELECT count(*) AS randuri,
       count(*) FILTER (WHERE p."Rest" = d."TotalStingere") AS rest_egal_totalstingere,
       sum(p."Rest") AS suma_rest,
       (SELECT sum("TotalStingere") FROM tr."Doc" WHERE "Stare"=1 AND "TotalStingere" <> 0) AS suma_totalstingere
FROM "PartideDeschise" p JOIN tr."Doc" d ON d."ID" = p."DocumentId"
WHERE p."An"=2025 AND p."Luna"=12;

-- F0 "onest": TotalStingere − Asignat (toate imperecherile, fara taiere).
DROP TABLE IF EXISTS tr."F0Rest";
CREATE UNLOGGED TABLE tr."F0Rest" AS
SELECT d."ID" AS "DocumentId", d."TotalStingere" - COALESCE(a."Asignat", 0) AS "Rest"
FROM tr."Doc" d LEFT JOIN tr."Asignat" a ON a."DocumentId" = d."ID"
WHERE d."Stare" = 1 AND d."TotalStingere" IS NOT NULL
  AND d."DataInregistrare" <= DATE '2025-12-31'
  AND d."TotalStingere" - COALESCE(a."Asignat", 0) <> 0;
ALTER TABLE tr."F0Rest" ADD PRIMARY KEY ("DocumentId");
ANALYZE tr."F0Rest";

\echo
\echo ===== B6b.1 F0 onest: cate partide, pe tip =====
SELECT d."ClrType", count(*) AS partide, sum(f."Rest") AS suma
FROM tr."F0Rest" f JOIN tr."Doc" d ON d."ID" = f."DocumentId" GROUP BY 1 ORDER BY 2 DESC;

\echo
\echo ===== B6b.2 A / B / C contra F0 onest, pe DocumentId =====
WITH f0 AS (SELECT * FROM tr."F0Rest"),
     a  AS (SELECT "DocumentId", "Rest" FROM tr."PartidaA" WHERE "Rest" <> 0),
     b  AS (SELECT "DocumentId", sum("Rest") AS "Rest" FROM tr."PartidaB" GROUP BY 1 HAVING sum("Rest") <> 0),
     c  AS (SELECT k."DocumentId", p."Rest" FROM tr."PartidaC" p JOIN tr."CheieC" k ON k."Cheie" = p."Cheie"
            WHERE p."Rest" <> 0)
SELECT 'A' AS definitie, (SELECT count(*) FROM f0) AS f0_randuri, (SELECT count(*) FROM a) AS def_randuri,
       count(*) FILTER (WHERE f0."DocumentId" IS NOT NULL AND a."DocumentId" IS NOT NULL) AS comune,
       count(*) FILTER (WHERE abs(a."Rest") = f0."Rest")  AS rest_identic,
       count(*) FILTER (WHERE f0."DocumentId" IS NULL)    AS doar_in_definitie,
       count(*) FILTER (WHERE a."DocumentId" IS NULL)     AS doar_in_f0,
       sum(abs(abs(COALESCE(a."Rest",0)) - COALESCE(f0."Rest",0))) AS abs_diferenta
FROM f0 FULL OUTER JOIN a ON a."DocumentId" = f0."DocumentId"
UNION ALL
SELECT 'B', (SELECT count(*) FROM f0), (SELECT count(*) FROM tr."PartidaB" WHERE "Rest" <> 0),
       count(*) FILTER (WHERE f0."DocumentId" IS NOT NULL AND b."DocumentId" IS NOT NULL),
       count(*) FILTER (WHERE abs(b."Rest") = f0."Rest"),
       count(*) FILTER (WHERE f0."DocumentId" IS NULL),
       count(*) FILTER (WHERE b."DocumentId" IS NULL),
       sum(abs(abs(COALESCE(b."Rest",0)) - COALESCE(f0."Rest",0)))
FROM f0 FULL OUTER JOIN b ON b."DocumentId" = f0."DocumentId"
UNION ALL
SELECT 'C', (SELECT count(*) FROM f0), (SELECT count(*) FROM tr."PartidaC" WHERE "Rest" <> 0),
       count(*) FILTER (WHERE f0."DocumentId" IS NOT NULL AND c."DocumentId" IS NOT NULL),
       count(*) FILTER (WHERE abs(c."Rest") = f0."Rest"),
       count(*) FILTER (WHERE f0."DocumentId" IS NULL),
       count(*) FILTER (WHERE c."DocumentId" IS NULL),
       sum(abs(abs(COALESCE(c."Rest",0)) - COALESCE(f0."Rest",0)))
FROM f0 FULL OUTER JOIN c ON c."DocumentId" = f0."DocumentId";

\echo
\echo ===== B6b.3 A contra F0 onest, pe tip =====
WITH f0 AS (SELECT * FROM tr."F0Rest"), a AS (SELECT "DocumentId","Rest" FROM tr."PartidaA" WHERE "Rest" <> 0)
SELECT COALESCE(d."ClrType",'(?)') AS tip,
       count(*) FILTER (WHERE abs(a."Rest") = f0."Rest") AS identic,
       count(*) FILTER (WHERE f0."DocumentId" IS NOT NULL AND a."DocumentId" IS NOT NULL
                          AND abs(a."Rest") <> f0."Rest") AS comun_dar_alt_rest,
       count(*) FILTER (WHERE f0."DocumentId" IS NULL)    AS doar_cub,
       count(*) FILTER (WHERE a."DocumentId" IS NULL)     AS doar_f0
FROM f0 FULL OUTER JOIN a ON a."DocumentId" = f0."DocumentId"
LEFT JOIN tr."Doc" d ON d."ID" = COALESCE(f0."DocumentId", a."DocumentId")
GROUP BY 1 ORDER BY 2 DESC;

\echo
\echo ===== B6b.4 C contra F0 onest, pe lantul FCT+NIR =====
WITH lant AS (
  SELECT x."FctId" AS "Cheie", p."Rest" AS rest_c,
         COALESCE((SELECT sum(f."Rest") FROM tr."F0Rest" f
                   WHERE f."DocumentId" = x."FctId" OR f."DocumentId" = x."NirId"), 0) AS rest_f0_lant
  FROM tr."FctNir" x JOIN tr."PartidaC" p ON p."Cheie" = x."FctId")
SELECT count(*) AS lanturi,
       count(*) FILTER (WHERE abs(rest_c) = rest_f0_lant) AS identic,
       count(*) FILTER (WHERE rest_c = 0 AND rest_f0_lant = 0) AS ambele_stinse,
       count(*) FILTER (WHERE rest_c = 0 AND rest_f0_lant <> 0) AS cub_stins_f0_nu,
       count(*) FILTER (WHERE rest_c <> 0 AND rest_f0_lant = 0) AS f0_stins_cub_nu,
       sum(abs(rest_c)) AS suma_c, sum(rest_f0_lant) AS suma_f0
FROM lant;

\echo
\echo ===== B6b.5 semnul: unde |rest| difera de rest (documente cu rest negativ in cub) =====
SELECT d."ClrType", count(*) AS partide, sum(a."Rest") AS suma
FROM tr."PartidaA" a JOIN tr."Doc" d ON d."ID" = a."DocumentId"
WHERE a."Rest" < 0 GROUP BY 1 ORDER BY 2 DESC;
