-- B1. Per tip de document: operate, cate posteaza pe >=1 cont de tert,
-- distributia conturilor de tert DISTINCTE (1/2/3+) si a partenerilor
-- distincti pe postarile de tert (0/1/2+).
\set ON_ERROR_STOP on
\pset footer off

\echo ===== B1.1 per ClrType =====
WITH t AS (
  SELECT n."DocumentId",
         count(*)                                   AS conturi_tert,
         count(DISTINCT n."Partener")               AS parteneri
  FROM tr."TertDocContNet" n GROUP BY 1
)
SELECT d."ClrType",
       count(*)                                                  AS operate,
       count(t."DocumentId")                                     AS cu_tert,
       count(*) FILTER (WHERE t.conturi_tert = 1)                AS ct_1,
       count(*) FILTER (WHERE t.conturi_tert = 2)                AS ct_2,
       count(*) FILTER (WHERE t.conturi_tert >= 3)               AS ct_3plus,
       count(*) FILTER (WHERE COALESCE(t.parteneri,0) = 0)       AS part_0,
       count(*) FILTER (WHERE t.parteneri = 1)                   AS part_1,
       count(*) FILTER (WHERE t.parteneri >= 2)                  AS part_2plus
FROM tr."Doc" d
LEFT JOIN t ON t."DocumentId" = d."ID"
WHERE d."Stare" = 1
GROUP BY 1 ORDER BY 2 DESC;

\echo
\echo ===== B1.2 total =====
WITH t AS (
  SELECT n."DocumentId", count(*) AS conturi_tert, count(DISTINCT n."Partener") AS parteneri
  FROM tr."TertDocContNet" n GROUP BY 1
)
SELECT count(*) AS operate, count(t."DocumentId") AS cu_tert,
       count(*) FILTER (WHERE t.conturi_tert = 1) AS ct_1,
       count(*) FILTER (WHERE t.conturi_tert = 2) AS ct_2,
       count(*) FILTER (WHERE t.conturi_tert >= 3) AS ct_3plus,
       count(*) FILTER (WHERE COALESCE(t.parteneri,0) = 0) AS part_0,
       count(*) FILTER (WHERE t.parteneri = 1) AS part_1,
       count(*) FILTER (WHERE t.parteneri >= 2) AS part_2plus
FROM tr."Doc" d LEFT JOIN t ON t."DocumentId" = d."ID" WHERE d."Stare" = 1;

\echo
\echo ===== B1.3 ce conturi de tert, pe tip (simbol x latura) =====
SELECT d."ClrType", n."ContSimbol", n."RolTert",
       count(*) AS documente,
       count(*) FILTER (WHERE n."Laturi" = 2) AS si_debit_si_credit,
       sum(n."NetDebitor") AS net_debitor
FROM tr."TertDocContNet" n JOIN tr."Doc" d ON d."ID" = n."DocumentId"
WHERE d."Stare" = 1
GROUP BY 1,2,3 ORDER BY 1,2;

\echo
\echo ===== B1.4 stari (control) =====
SELECT "Stare", count(*) FROM tr."Doc" GROUP BY 1 ORDER BY 1;
