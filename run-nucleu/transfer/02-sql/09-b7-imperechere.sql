-- B7. Imperecherea ca tranzactie (§4.4 al fizicii), sub fiecare definitie.
-- "Reprezentabila fara rupere" = (1) partida stinsa are postare pe contul de
-- referinta al stingatorului, (2) latura e opusa (perechea inchide, nu adauga),
-- (3) suma <= |restul propriu| al AMBELOR partide (inainte de imperecheri).
\set ON_ERROR_STOP on
\pset footer off

-- Restul PROPRIU (fara imperecheri) per definitie.
DROP TABLE IF EXISTS tr."ProprA";
CREATE UNLOGGED TABLE tr."ProprA" AS
SELECT "DocumentId", sum("Natural") AS "Propriu" FROM tr."TertLa1231" GROUP BY 1;
ALTER TABLE tr."ProprA" ADD PRIMARY KEY ("DocumentId");
DROP TABLE IF EXISTS tr."ProprC";
CREATE UNLOGGED TABLE tr."ProprC" AS
SELECT c."Cheie", sum(p."Propriu") AS "Propriu"
FROM tr."ProprA" p JOIN tr."CheieC" c ON c."DocumentId" = p."DocumentId" GROUP BY 1;
ALTER TABLE tr."ProprC" ADD PRIMARY KEY ("Cheie");
ANALYZE tr."ProprA"; ANALYZE tr."ProprC";

-- Consumul cumulat per document/lant, in valoare absoluta.
DROP TABLE IF EXISTS tr."ConsumA";
CREATE UNLOGGED TABLE tr."ConsumA" AS
SELECT "Doc" AS "DocumentId", sum("S") AS "Consum" FROM (
  SELECT "DocumentId" AS "Doc", sum("Suma") AS "S" FROM "Imperecheri" WHERE "GCRecord"=0 GROUP BY 1
  UNION ALL
  SELECT "DocumentStingatorId", sum("Suma") FROM "Imperecheri" WHERE "GCRecord"=0 GROUP BY 1) q
GROUP BY 1;
ALTER TABLE tr."ConsumA" ADD PRIMARY KEY ("DocumentId");
ANALYZE tr."ConsumA";

\echo ===== B7.1 latura: cate imperecheri au partile pe laturi OPUSE =====
SELECT count(*) AS total,
       count(*) FILTER (WHERE f."Latura" IS NOT NULL AND s."Latura" <> f."Latura") AS laturi_opuse,
       count(*) FILTER (WHERE f."Latura" IS NOT NULL AND s."Latura"  = f."Latura") AS aceeasi_latura,
       count(*) FILTER (WHERE f."Latura" IS NULL)                                  AS stins_fara_tert
FROM "Imperecheri" i
JOIN tr."TertRef" s ON s."DocumentId" = i."DocumentStingatorId"
LEFT JOIN tr."TertRef" f ON f."DocumentId" = i."DocumentId";

\echo
\echo ===== B7.2 reprezentabilitatea, pe definitii =====
WITH baza AS (
  SELECT i."ID", i."Suma", i."DocumentId" AS stins, i."DocumentStingatorId" AS sting,
         s."Cont" AS cont_sting,
         -- (1a) A/C: partida stinsa are VREO postare de tert
         EXISTS (SELECT 1 FROM tr."TertLa1231" t WHERE t."DocumentId" = i."DocumentId") AS a_cont_ok,
         -- (1b) B: partida stinsa are EXACT contul stingatorului
         EXISTS (SELECT 1 FROM tr."TertLa1231" t
                 WHERE t."DocumentId" = i."DocumentId" AND t."Cont" = s."Cont")         AS b_cont_ok,
         -- (1c) C: lantul conex al stinsului are contul stingatorului
         EXISTS (SELECT 1 FROM tr."TertLa1231" t JOIN tr."CheieC" k ON k."DocumentId" = t."DocumentId"
                 WHERE k."Cheie" = (SELECT k2."Cheie" FROM tr."CheieC" k2 WHERE k2."DocumentId" = i."DocumentId")
                   AND t."Cont" = s."Cont")                                             AS c_cont_ok
  FROM "Imperecheri" i JOIN tr."TertRef" s ON s."DocumentId" = i."DocumentStingatorId"
  WHERE i."GCRecord" = 0
), depasire AS (
  -- documente / lanturi al caror consum cumulat depaseste |restul propriu|
  SELECT "DocumentId" FROM tr."ConsumA" c JOIN tr."ProprA" p USING ("DocumentId")
  WHERE round(abs(c."Consum"), 2) > round(abs(p."Propriu"), 2) + 0.005
), depasireC AS (
  SELECT k."Cheie" FROM tr."ConsumA" c JOIN tr."CheieC" k ON k."DocumentId" = c."DocumentId"
  GROUP BY 1 HAVING round(abs(sum(c."Consum")), 2)
       > round(abs((SELECT p."Propriu" FROM tr."ProprC" p WHERE p."Cheie" = k."Cheie")), 2) + 0.005
)
SELECT 'A (document)' AS definitie, count(*) AS total,
       count(*) FILTER (WHERE a_cont_ok) AS cont_ok,
       count(*) FILTER (WHERE a_cont_ok
         AND stins NOT IN (SELECT * FROM depasire) AND sting NOT IN (SELECT * FROM depasire)) AS fara_rupere,
       count(*) FILTER (WHERE NOT a_cont_ok) AS rupt_cont,
       count(*) FILTER (WHERE a_cont_ok
         AND (stins IN (SELECT * FROM depasire) OR sting IN (SELECT * FROM depasire))) AS rupt_suma
FROM baza
UNION ALL
SELECT 'B (document x cont)', count(*), count(*) FILTER (WHERE b_cont_ok),
       count(*) FILTER (WHERE b_cont_ok
         AND stins NOT IN (SELECT * FROM depasire) AND sting NOT IN (SELECT * FROM depasire)),
       count(*) FILTER (WHERE NOT b_cont_ok),
       count(*) FILTER (WHERE b_cont_ok
         AND (stins IN (SELECT * FROM depasire) OR sting IN (SELECT * FROM depasire)))
FROM baza
UNION ALL
SELECT 'C (lant conex)', count(*), count(*) FILTER (WHERE c_cont_ok),
       count(*) FILTER (WHERE c_cont_ok
         AND (SELECT k."Cheie" FROM tr."CheieC" k WHERE k."DocumentId" = stins) NOT IN (SELECT * FROM depasireC)
         AND (SELECT k."Cheie" FROM tr."CheieC" k WHERE k."DocumentId" = sting) NOT IN (SELECT * FROM depasireC)),
       count(*) FILTER (WHERE NOT c_cont_ok),
       count(*) FILTER (WHERE c_cont_ok
         AND ((SELECT k."Cheie" FROM tr."CheieC" k WHERE k."DocumentId" = stins) IN (SELECT * FROM depasireC)
           OR (SELECT k."Cheie" FROM tr."CheieC" k WHERE k."DocumentId" = sting) IN (SELECT * FROM depasireC)))
FROM baza;

\echo
\echo ===== B7.3 depasirea de suma: pe tip de document (definitia A) =====
SELECT d."ClrType", count(*) AS documente,
       sum(abs(c."Consum") - abs(p."Propriu")) AS depasire_totala
FROM tr."ConsumA" c JOIN tr."ProprA" p USING ("DocumentId") JOIN tr."Doc" d ON d."ID" = c."DocumentId"
WHERE round(abs(c."Consum"), 2) > round(abs(p."Propriu"), 2) + 0.005
GROUP BY 1 ORDER BY 2 DESC;

\echo
\echo ===== B7.4 acelasi, sub definitia C (lantul FCT+NIR) =====
SELECT count(*) AS lanturi_care_depasesc,
       sum(abs(s."Consum") - abs(p."Propriu")) AS depasire_totala
FROM (SELECT k."Cheie", sum(c."Consum") AS "Consum" FROM tr."ConsumA" c
      JOIN tr."CheieC" k ON k."DocumentId" = c."DocumentId" GROUP BY 1) s
JOIN tr."ProprC" p ON p."Cheie" = s."Cheie"
WHERE round(abs(s."Consum"), 2) > round(abs(p."Propriu"), 2) + 0.005;

\echo
\echo ===== B7.5 definitia C: coincidenta cu F0 pe lanturile FCT+NIR =====
WITH lant AS (
  SELECT x."FctId" AS "Cheie", p."Rest" AS rest_c,
         COALESCE((SELECT sum(f."Rest") FROM "PartideDeschise" f
                   WHERE f."An"=2025 AND f."Luna"=12
                     AND (f."DocumentId" = x."FctId" OR f."DocumentId" = x."NirId")), 0) AS rest_f0_lant,
         COALESCE((SELECT f."Rest" FROM "PartideDeschise" f
                   WHERE f."An"=2025 AND f."Luna"=12 AND f."DocumentId" = x."FctId"), 0)  AS rest_f0_fct
  FROM tr."FctNir" x JOIN tr."PartidaC" p ON p."Cheie" = x."FctId")
SELECT count(*) AS lanturi,
       count(*) FILTER (WHERE abs(rest_c) = rest_f0_fct)   AS c_egal_cu_f0_al_fct,
       count(*) FILTER (WHERE abs(rest_c) = rest_f0_lant)  AS c_egal_cu_f0_fct_plus_nir,
       count(*) FILTER (WHERE rest_c = 0)                  AS lanturi_stinse,
       sum(abs(rest_c)) AS suma_c, sum(rest_f0_lant) AS suma_f0_lant, sum(rest_f0_fct) AS suma_f0_fct
FROM lant;
