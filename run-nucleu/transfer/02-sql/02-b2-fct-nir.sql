-- B2. Conexul FCT -> NIR: cine posteaza pe 401 si cu ce suma.
\set ON_ERROR_STOP on
\pset footer off

-- Perechile FCT-NIR: legatura e la nivel de DOCUMENT (`DocumentSursaId`).
DROP TABLE IF EXISTS tr."FctNir";
CREATE UNLOGGED TABLE tr."FctNir" AS
SELECT f."ID" AS "FctId", n."ID" AS "NirId"
FROM tr."Doc" f JOIN tr."Doc" n ON n."DocumentSursaId" = f."ID"
WHERE f."ClrType" = 'FacturaIntrare' AND n."ClrType" = 'NIR' AND n."Stare" = 1 AND f."Stare" = 1;
CREATE INDEX ON tr."FctNir"("FctId");
CREATE INDEX ON tr."FctNir"("NirId");
ANALYZE tr."FctNir";

\echo ===== B2.1 acoperirea conexului =====
SELECT
 (SELECT count(*) FROM tr."Doc" WHERE "ClrType"='FacturaIntrare' AND "Stare"=1)                AS fct_operate,
 (SELECT count(DISTINCT "FctId") FROM tr."FctNir")                                            AS fct_cu_nir,
 (SELECT count(*) FROM tr."Doc" WHERE "ClrType"='NIR' AND "Stare"=1)                          AS nir_operate,
 (SELECT count(DISTINCT "NirId") FROM tr."FctNir")                                            AS nir_cu_fct,
 (SELECT count(*) FROM tr."Doc" n WHERE n."ClrType"='NIR' AND n."Stare"=1
    AND NOT EXISTS (SELECT 1 FROM tr."FctNir" x WHERE x."NirId" = n."ID"))                    AS nir_fara_fct,
 (SELECT count(*) FROM (SELECT "FctId" FROM tr."FctNir" GROUP BY 1 HAVING count(*)>1) q)      AS fct_cu_mai_multe_nir,
 (SELECT count(*) FROM (SELECT "NirId" FROM tr."FctNir" GROUP BY 1 HAVING count(*)>1) q)      AS nir_cu_mai_multe_fct;

\echo
\echo ===== B2.2 cati NIR per FCT =====
SELECT nr_nir, count(*) AS facturi FROM (
  SELECT "FctId", count(*) AS nr_nir FROM tr."FctNir" GROUP BY 1) q
GROUP BY 1 ORDER BY 1;

\echo
\echo ===== B2.3 pe perechi: 401 al FCT, 401 al NIR, TotalStingere al FCT =====
WITH p AS (
  SELECT x."FctId",
         sum(CASE WHEN n."ContSimbol" = '401' THEN -n."NetDebitor" ELSE 0 END) AS c401_nir
  FROM tr."FctNir" x
  LEFT JOIN tr."TertDocContNet" n ON n."DocumentId" = x."NirId"
  GROUP BY 1
), f AS (
  SELECT d."ID" AS "FctId", d."TotalStingere",
         COALESCE((SELECT sum(-n."NetDebitor") FROM tr."TertDocContNet" n
                   WHERE n."DocumentId" = d."ID" AND n."ContSimbol" = '401'), 0) AS c401_fct,
         COALESCE((SELECT sum(-n."NetDebitor") FROM tr."TertDocContNet" n
                   WHERE n."DocumentId" = d."ID" AND n."RolTert" <> 0), 0)        AS tert_fct
  FROM tr."Doc" d WHERE d."ClrType" = 'FacturaIntrare' AND d."Stare" = 1
)
SELECT count(*)                                                        AS perechi_fct,
       sum(f.c401_fct)                                                 AS suma_401_fct,
       sum(p.c401_nir)                                                 AS suma_401_nir,
       sum(f."TotalStingere")                                          AS suma_totalstingere,
       sum(f.c401_fct + p.c401_nir)                                    AS suma_401_fct_plus_nir,
       count(*) FILTER (WHERE f."TotalStingere" = f.c401_fct + p.c401_nir) AS exact,
       count(*) FILTER (WHERE f."TotalStingere" <> f.c401_fct + p.c401_nir) AS diferit,
       sum(abs(f."TotalStingere" - (f.c401_fct + p.c401_nir)))         AS abs_diferenta
FROM f JOIN p ON p."FctId" = f."FctId";

\echo
\echo ===== B2.4 FCT fara NIR (servicii): ce posteaza pe 401 =====
WITH f AS (
  SELECT d."ID", d."TotalStingere",
         COALESCE((SELECT sum(-n."NetDebitor") FROM tr."TertDocContNet" n
                   WHERE n."DocumentId" = d."ID" AND n."RolTert" <> 0), 0) AS tert_fct,
         EXISTS (SELECT 1 FROM tr."TertDocContNet" n WHERE n."DocumentId" = d."ID") AS are_tert
  FROM tr."Doc" d
  WHERE d."ClrType" = 'FacturaIntrare' AND d."Stare" = 1
    AND NOT EXISTS (SELECT 1 FROM tr."FctNir" x WHERE x."FctId" = d."ID")
)
SELECT count(*) AS fct_fara_nir,
       count(*) FILTER (WHERE are_tert)      AS cu_postare_tert,
       count(*) FILTER (WHERE NOT are_tert)  AS fara_postare_tert,
       sum(tert_fct)                         AS suma_tert,
       sum("TotalStingere")                  AS suma_totalstingere,
       count(*) FILTER (WHERE "TotalStingere" = tert_fct) AS exact
FROM f;

\echo
\echo ===== B2.5 pe FCT cu NIR: raportul TVA / net (linia de TVA a facturii) =====
-- Ipoteza de verificat: FCT posteaza pe 401 DOAR TVA-ul, NIR-ul netul.
WITH x AS (
  SELECT f."ID" AS "FctId",
         COALESCE((SELECT sum(-n."NetDebitor") FROM tr."TertDocContNet" n
                   WHERE n."DocumentId" = f."ID" AND n."ContSimbol" = '401'), 0) AS c401_fct,
         COALESCE((SELECT sum(v."Tva") FROM "RegistruTva" v WHERE v."DocumentId" = f."ID"), 0) AS tva,
         COALESCE((SELECT sum(v."Baza") FROM "RegistruTva" v WHERE v."DocumentId" = f."ID"), 0) AS baza
  FROM tr."Doc" f
  WHERE f."ClrType" = 'FacturaIntrare' AND f."Stare" = 1
    AND EXISTS (SELECT 1 FROM tr."FctNir" y WHERE y."FctId" = f."ID")
)
SELECT count(*) AS fct_cu_nir,
       count(*) FILTER (WHERE c401_fct = tva)                AS c401_egal_tva,
       count(*) FILTER (WHERE c401_fct = tva + baza)         AS c401_egal_brut,
       count(*) FILTER (WHERE c401_fct = 0)                  AS c401_zero,
       sum(c401_fct) AS suma_c401, sum(tva) AS suma_tva, sum(baza) AS suma_baza
FROM x;

\echo
\echo ===== B2.6 3 exemple de FCT cu mai multe NIR =====
SELECT f."Numar" AS fct_numar, f."Data", count(*) AS nr_nir,
       string_agg(n."Numar", ' | ' ORDER BY n."Numar") AS nir_numere
FROM tr."FctNir" x JOIN tr."Doc" f ON f."ID" = x."FctId" JOIN tr."Doc" n ON n."ID" = x."NirId"
GROUP BY f."ID", f."Numar", f."Data" HAVING count(*) > 1
ORDER BY count(*) DESC LIMIT 3;
