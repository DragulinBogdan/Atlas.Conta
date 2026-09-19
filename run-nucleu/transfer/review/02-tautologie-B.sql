\pset footer off
\echo ===== R2.1 soldul contabil REAL al conturilor de tert la 31.12.2025, cu si fara deschidere =====
WITH picior AS (
  SELECT r."ContDebitId" AS cont, r."Valoare" AS v, r."DocumentId" FROM "RegistruContabil" r WHERE r."Data" <= DATE '2025-12-31'
  UNION ALL
  SELECT r."ContCreditId", -r."Valoare", r."DocumentId"          FROM "RegistruContabil" r WHERE r."Data" <= DATE '2025-12-31'
)
SELECT c."Simbol", c."RolTert",
       round(sum(CASE WHEN c."RolTert"=1 THEN p.v ELSE -p.v END),2) AS natural_tot,
       round(sum(CASE WHEN c."RolTert"=1 THEN p.v ELSE -p.v END) FILTER (WHERE p."DocumentId" IS NOT NULL),2) AS natural_cu_document,
       round(sum(CASE WHEN c."RolTert"=1 THEN p.v ELSE -p.v END) FILTER (WHERE p."DocumentId" IS NULL),2) AS natural_deschidere
FROM picior p JOIN "Conturi" c ON c."ID"=p.cont WHERE c."RolTert" <> 0
GROUP BY ROLLUP (1,2) ORDER BY 1 NULLS LAST;

\echo
\echo ===== R2.2 tautologia: A cu acelasi FULL OUTER JOIN ca B (partida inventata pe DOCUMENT) =====
WITH efect AS (
  SELECT "DocumentId" AS doc, -sum("Suma") AS ef FROM "Imperecheri" WHERE "GCRecord"=0 GROUP BY 1
  UNION ALL
  SELECT "DocumentStingatorId", sum("Suma")     FROM "Imperecheri" WHERE "GCRecord"=0 GROUP BY 1
), efdoc AS (SELECT doc, sum(ef) AS ef FROM efect GROUP BY 1),
prop AS (SELECT "DocumentId" AS doc, sum("Natural") AS nat FROM tr."TertLa1231" GROUP BY 1),
a_full AS (
  SELECT COALESCE(p.doc, e.doc) AS doc, COALESCE(p.nat,0)+COALESCE(e.ef,0) AS rest,
         (p.doc IS NULL) AS inventata
  FROM prop p FULL OUTER JOIN efdoc e ON e.doc = p.doc
)
SELECT 'A cu LEFT JOIN (cum e in raport)'  AS varianta, round(sum(rest),2) AS suma, count(*) FILTER (WHERE rest<>0) AS partide_nenule
FROM (SELECT p.doc, p.nat + COALESCE(e.ef,0) AS rest FROM prop p LEFT JOIN efdoc e ON e.doc=p.doc) x
UNION ALL
SELECT 'A cu FULL OUTER JOIN (ca B)', round(sum(rest),2), count(*) FILTER (WHERE rest<>0) FROM a_full
UNION ALL
SELECT '  din care partide A inventate', round(sum(rest),2), count(*) FROM a_full WHERE inventata
UNION ALL
SELECT 'B: Sigma rest (din raport)', round(sum("Rest"),2), count(*) FILTER (WHERE "Rest"<>0) FROM tr."PartidaB"
UNION ALL
SELECT '  din care partide B inventate', round(sum("Rest"),2), count(*) FROM tr."PartidaB" WHERE "ContInventat"
UNION ALL
SELECT 'Sigma postari de tert cu document (= "soldul real")', round(sum("Natural"),2), count(*) FROM tr."TertLa1231";
