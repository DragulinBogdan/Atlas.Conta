-- B6. Cele trei definitii candidate de partida, la 31.12.2025.
--  A = documentul (Unitate = DocumentId, ca in cubul masurat)
--  B = documentul x contul de tert
--  C = lantul conex (FCT + NIR-urile ei = o partida; altfel ca A)
--
-- CAPCANA: toate `Imperecheri.Data` sunt 2026-09-18 (artefact de import), deci
-- un filtru pe data lor ar scoate TOATE stingerile din fereastra. Aici
-- imperecherile se aplica INDIFERENT de data lor (ca in F0, unde
-- `AsignariPanaLa` le-ar taia la fel de gresit) — vezi B6.0.
\set ON_ERROR_STOP on
\pset footer off

\echo ===== B6.0 control: datele imperecherilor =====
SELECT min("Data") AS data_min, max("Data") AS data_max, count(*) AS randuri,
       count(*) FILTER (WHERE "Data" <= DATE '2025-12-31') AS in_2025,
       count(*) FILTER (WHERE "InverseazaId" IS NOT NULL)  AS randuri_inverse,
       count(*) FILTER (WHERE "Suma" < 0)                  AS sume_negative
FROM "Imperecheri" WHERE "GCRecord" = 0;

\echo
\echo ===== B6.0b control: documentele la 31.12.2025 =====
SELECT count(*) AS documente, count(*) FILTER (WHERE "DataInregistrare" <= DATE '2025-12-31') AS pana_la_3112,
       count(*) FILTER (WHERE "TotalStingere" IS NULL) AS fara_total
FROM tr."Doc" WHERE "Stare" = 1;

-- Postarile de tert la 31.12.2025, in SENSUL NATURAL al contului
-- (RolTert=1 client -> debitor; RolTert=2 furnizor -> creditor).
DROP TABLE IF EXISTS tr."TertLa1231";
CREATE UNLOGGED TABLE tr."TertLa1231" AS
SELECT p."DocumentId", p."Cont", max(p."ContSimbol") AS "ContSimbol", max(p."RolTert") AS "RolTert",
       sum(CASE WHEN p."RolTert" = 1
                THEN (CASE WHEN p."Latura" = 1 THEN p."Valoare" ELSE -p."Valoare" END)
                ELSE (CASE WHEN p."Latura" = 2 THEN p."Valoare" ELSE -p."Valoare" END) END) AS "Natural"
FROM tr."Picior" p
WHERE p."RolTert" <> 0 AND p."DocumentId" IS NOT NULL AND p."Data" <= DATE '2025-12-31'
GROUP BY 1,2;
ALTER TABLE tr."TertLa1231" ADD PRIMARY KEY ("DocumentId","Cont");
ANALYZE tr."TertLa1231";

-- Efectul imperecherii pe partida, in acelasi sens natural:
--   stins      -> −Suma ;  stingator -> +Suma
DROP TABLE IF EXISTS tr."ImpEfect";
CREATE UNLOGGED TABLE tr."ImpEfect" AS
SELECT "DocumentId" AS "Doc", -sum("Suma") AS "Efect" FROM "Imperecheri" WHERE "GCRecord"=0 GROUP BY 1
UNION ALL
SELECT "DocumentStingatorId", sum("Suma") FROM "Imperecheri" WHERE "GCRecord"=0 GROUP BY 1;
DROP TABLE IF EXISTS tr."ImpEfectDoc";
CREATE UNLOGGED TABLE tr."ImpEfectDoc" AS
SELECT "Doc" AS "DocumentId", sum("Efect") AS "Efect" FROM tr."ImpEfect" GROUP BY 1;
ALTER TABLE tr."ImpEfectDoc" ADD PRIMARY KEY ("DocumentId");
ANALYZE tr."ImpEfectDoc";

-- ---------- DEFINITIA A: partida = documentul ----------
DROP TABLE IF EXISTS tr."PartidaA";
CREATE UNLOGGED TABLE tr."PartidaA" AS
SELECT t."DocumentId", sum(t."Natural") + COALESCE(max(e."Efect"), 0) AS "Rest"
FROM tr."TertLa1231" t LEFT JOIN tr."ImpEfectDoc" e ON e."DocumentId" = t."DocumentId"
GROUP BY 1;
ALTER TABLE tr."PartidaA" ADD PRIMARY KEY ("DocumentId");
ANALYZE tr."PartidaA";

-- ---------- DEFINITIA B: partida = (document, cont) ----------
-- Imperecherea se atribuie contului de referinta al STINGATORULUI pe AMBELE
-- partide (propunerea §4.4). Contul de referinta = tr."TertRef".
DROP TABLE IF EXISTS tr."ImpEfectCont";
CREATE UNLOGGED TABLE tr."ImpEfectCont" AS
SELECT i."DocumentId" AS "Doc", s."Cont", -sum(i."Suma") AS "Efect"
FROM "Imperecheri" i JOIN tr."TertRef" s ON s."DocumentId" = i."DocumentStingatorId"
WHERE i."GCRecord" = 0 GROUP BY 1,2
UNION ALL
SELECT i."DocumentStingatorId", s."Cont", sum(i."Suma")
FROM "Imperecheri" i JOIN tr."TertRef" s ON s."DocumentId" = i."DocumentStingatorId"
WHERE i."GCRecord" = 0 GROUP BY 1,2;
DROP TABLE IF EXISTS tr."PartidaB";
CREATE UNLOGGED TABLE tr."PartidaB" AS
SELECT COALESCE(t."DocumentId", e."Doc") AS "DocumentId",
       COALESCE(t."Cont", e."Cont")      AS "Cont",
       COALESCE(sum(t."Natural"), 0) + COALESCE(max(e2."Efect"), 0) AS "Rest",
       (t."DocumentId" IS NULL)          AS "ContInventat"
FROM tr."TertLa1231" t
FULL OUTER JOIN (SELECT "Doc", "Cont", sum("Efect") AS "Efect" FROM tr."ImpEfectCont" GROUP BY 1,2) e
  ON e."Doc" = t."DocumentId" AND e."Cont" = t."Cont"
LEFT JOIN LATERAL (SELECT e."Efect") e2 ON TRUE
GROUP BY 1,2,4;
CREATE INDEX ON tr."PartidaB"("DocumentId");
ANALYZE tr."PartidaB";

-- ---------- DEFINITIA C: lantul conex (FCT + NIR-urile ei) ----------
DROP TABLE IF EXISTS tr."CheieC";
CREATE UNLOGGED TABLE tr."CheieC" AS
SELECT d."ID" AS "DocumentId", COALESCE(x."FctId", d."ID") AS "Cheie"
FROM tr."Doc" d LEFT JOIN tr."FctNir" x ON x."NirId" = d."ID";
ALTER TABLE tr."CheieC" ADD PRIMARY KEY ("DocumentId");
CREATE INDEX ON tr."CheieC"("Cheie");
ANALYZE tr."CheieC";

DROP TABLE IF EXISTS tr."PartidaC";
CREATE UNLOGGED TABLE tr."PartidaC" AS
SELECT c."Cheie", sum(a."Rest") AS "Rest"
FROM tr."PartidaA" a JOIN tr."CheieC" c ON c."DocumentId" = a."DocumentId"
GROUP BY 1;
ALTER TABLE tr."PartidaC" ADD PRIMARY KEY ("Cheie");
ANALYZE tr."PartidaC";

\echo
\echo ===== B6.1 A / B / C: partide cu rest <> 0, suma pe debitor si pe creditor =====
SELECT 'A (document)' AS definitie, count(*) FILTER (WHERE "Rest" <> 0) AS partide_cu_rest,
       sum("Rest") FILTER (WHERE "Rest" > 0) AS suma_debitor,
       sum("Rest") FILTER (WHERE "Rest" < 0) AS suma_creditor,
       count(*) AS partide_total
FROM tr."PartidaA"
UNION ALL
SELECT 'B (document x cont)', count(*) FILTER (WHERE "Rest" <> 0),
       sum("Rest") FILTER (WHERE "Rest" > 0), sum("Rest") FILTER (WHERE "Rest" < 0), count(*)
FROM tr."PartidaB"
UNION ALL
SELECT 'C (lant conex)', count(*) FILTER (WHERE "Rest" <> 0),
       sum("Rest") FILTER (WHERE "Rest" > 0), sum("Rest") FILTER (WHERE "Rest" < 0), count(*)
FROM tr."PartidaC";

\echo
\echo ===== B6.2 coincidenta cu PartideDeschise 12/2025, pe DocumentId =====
-- F0: restul e POZITIV prin constructie (TotalStingere − Asignat); pentru
-- comparatie se ia |Rest| al definitiei.
WITH f0 AS (SELECT "DocumentId", "Rest" FROM "PartideDeschise" WHERE "An"=2025 AND "Luna"=12),
     a  AS (SELECT "DocumentId", "Rest" FROM tr."PartidaA" WHERE "Rest" <> 0),
     b  AS (SELECT "DocumentId", sum("Rest") AS "Rest" FROM tr."PartidaB" GROUP BY 1 HAVING sum("Rest") <> 0),
     c  AS (SELECT k."DocumentId", p."Rest" FROM tr."PartidaC" p JOIN tr."CheieC" k ON k."Cheie" = p."Cheie"
            WHERE p."Rest" <> 0)
SELECT 'A' AS definitie,
       (SELECT count(*) FROM f0) AS f0_randuri, (SELECT count(*) FROM a) AS def_randuri,
       count(*) FILTER (WHERE f0."DocumentId" IS NOT NULL AND a."DocumentId" IS NOT NULL) AS comune,
       count(*) FILTER (WHERE f0."DocumentId" IS NOT NULL AND a."DocumentId" IS NOT NULL
                          AND abs(a."Rest") = f0."Rest")                                   AS rest_identic,
       count(*) FILTER (WHERE f0."DocumentId" IS NULL)  AS doar_in_definitie,
       count(*) FILTER (WHERE a."DocumentId" IS NULL)   AS doar_in_f0,
       sum(abs(abs(COALESCE(a."Rest",0)) - COALESCE(f0."Rest",0))) AS abs_diferenta
FROM f0 FULL OUTER JOIN a ON a."DocumentId" = f0."DocumentId"
UNION ALL
SELECT 'B', (SELECT count(*) FROM f0), (SELECT count(*) FROM tr."PartidaB" WHERE "Rest" <> 0),
       count(*) FILTER (WHERE f0."DocumentId" IS NOT NULL AND b."DocumentId" IS NOT NULL),
       count(*) FILTER (WHERE f0."DocumentId" IS NOT NULL AND b."DocumentId" IS NOT NULL
                          AND abs(b."Rest") = f0."Rest"),
       count(*) FILTER (WHERE f0."DocumentId" IS NULL),
       count(*) FILTER (WHERE b."DocumentId" IS NULL),
       sum(abs(abs(COALESCE(b."Rest",0)) - COALESCE(f0."Rest",0)))
FROM f0 FULL OUTER JOIN b ON b."DocumentId" = f0."DocumentId"
UNION ALL
SELECT 'C', (SELECT count(*) FROM f0), (SELECT count(*) FROM tr."PartidaC" WHERE "Rest" <> 0),
       count(*) FILTER (WHERE f0."DocumentId" IS NOT NULL AND c."DocumentId" IS NOT NULL),
       count(*) FILTER (WHERE f0."DocumentId" IS NOT NULL AND c."DocumentId" IS NOT NULL
                          AND abs(c."Rest") = f0."Rest"),
       count(*) FILTER (WHERE f0."DocumentId" IS NULL),
       count(*) FILTER (WHERE c."DocumentId" IS NULL),
       sum(abs(abs(COALESCE(c."Rest",0)) - COALESCE(f0."Rest",0)))
FROM f0 FULL OUTER JOIN c ON c."DocumentId" = f0."DocumentId";

\echo
\echo ===== B6.3 unde difera A de F0: pe tip de document =====
WITH f0 AS (SELECT "DocumentId", "Rest" FROM "PartideDeschise" WHERE "An"=2025 AND "Luna"=12),
     a  AS (SELECT "DocumentId", "Rest" FROM tr."PartidaA" WHERE "Rest" <> 0)
SELECT COALESCE(d."ClrType",'(?)') AS tip,
       count(*) FILTER (WHERE f0."DocumentId" IS NOT NULL AND a."DocumentId" IS NOT NULL
                          AND abs(a."Rest") = f0."Rest")                        AS identic,
       count(*) FILTER (WHERE f0."DocumentId" IS NOT NULL AND a."DocumentId" IS NOT NULL
                          AND abs(a."Rest") <> f0."Rest")                       AS comun_dar_alt_rest,
       count(*) FILTER (WHERE f0."DocumentId" IS NULL)                          AS doar_cub,
       count(*) FILTER (WHERE a."DocumentId" IS NULL)                           AS doar_f0
FROM f0 FULL OUTER JOIN a ON a."DocumentId" = f0."DocumentId"
LEFT JOIN tr."Doc" d ON d."ID" = COALESCE(f0."DocumentId", a."DocumentId")
GROUP BY 1 ORDER BY 2 DESC;

\echo
\echo ===== B6.4 B: cate partide per document, si cate conturi "inventate" de imperechere =====
SELECT nr_conturi, count(*) AS documente FROM (
  SELECT "DocumentId", count(*) AS nr_conturi FROM tr."PartidaB" WHERE "Rest" <> 0 GROUP BY 1) q
GROUP BY 1 ORDER BY 1;
SELECT count(*) AS partide_pe_cont_inventat, sum("Rest") AS suma
FROM tr."PartidaB" WHERE "ContInventat";
