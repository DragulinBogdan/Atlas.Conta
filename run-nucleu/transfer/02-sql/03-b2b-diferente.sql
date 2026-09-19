-- B2b. De ce difera `TotalStingere` de (401 FCT + 401 NIR) pe 1.797 de facturi.
\set ON_ERROR_STOP on
\pset footer off

DROP TABLE IF EXISTS tr."FctCifre";
CREATE UNLOGGED TABLE tr."FctCifre" AS
SELECT f."ID", f."Numar", f."Data", f."TotalStingere",
       COALESCE((SELECT sum(-n."NetDebitor") FROM tr."TertDocContNet" n
                 WHERE n."DocumentId" = f."ID" AND n."ContSimbol" = '401'), 0) AS c401_fct,
       COALESCE((SELECT sum(-n."NetDebitor") FROM tr."TertDocContNet" n
                 WHERE n."DocumentId" = f."ID" AND n."RolTert" <> 0), 0)       AS tert_fct,
       COALESCE((SELECT sum(-n."NetDebitor") FROM tr."TertDocContNet" n
                 JOIN tr."FctNir" x ON x."NirId" = n."DocumentId"
                 WHERE x."FctId" = f."ID" AND n."ContSimbol" = '401'), 0)      AS c401_nir,
       COALESCE((SELECT sum(v."Tva")  FROM "RegistruTva" v WHERE v."DocumentId" = f."ID"), 0) AS tva,
       COALESCE((SELECT sum(v."Baza") FROM "RegistruTva" v WHERE v."DocumentId" = f."ID"), 0) AS baza,
       (SELECT string_agg(DISTINCT v."Regim"::text, ',') FROM "RegistruTva" v WHERE v."DocumentId" = f."ID") AS regimuri,
       EXISTS (SELECT 1 FROM tr."FctNir" x WHERE x."FctId" = f."ID") AS are_nir
FROM tr."Doc" f WHERE f."ClrType" = 'FacturaIntrare' AND f."Stare" = 1;
ALTER TABLE tr."FctCifre" ADD PRIMARY KEY ("ID");
ANALYZE tr."FctCifre";

\echo ===== B2b.1 FCT cu NIR, pe regim TVA: coincide TotalStingere cu 401(FCT)+401(NIR)? =====
SELECT COALESCE(regimuri,'(fara rand TVA)') AS regimuri,
       count(*) AS facturi,
       count(*) FILTER (WHERE "TotalStingere" = c401_fct + c401_nir) AS exact,
       count(*) FILTER (WHERE "TotalStingere" <> c401_fct + c401_nir) AS diferit,
       sum("TotalStingere" - (c401_fct + c401_nir)) AS delta
FROM tr."FctCifre" WHERE are_nir GROUP BY 1 ORDER BY 2 DESC;

\echo
\echo ===== B2b.2 acelasi, dar cu baza+tva contra TotalStingere =====
SELECT count(*) AS fct_cu_nir,
       count(*) FILTER (WHERE "TotalStingere" = baza + tva) AS total_egal_baza_plus_tva,
       count(*) FILTER (WHERE c401_nir = baza)              AS nir401_egal_baza,
       count(*) FILTER (WHERE c401_nir <> baza)             AS nir401_diferit_de_baza,
       sum(c401_nir - baza)                                  AS delta_nir_baza
FROM tr."FctCifre" WHERE are_nir;

\echo
\echo ===== B2b.3 trei exemple de diferenta =====
SELECT "Numar", "Data", "TotalStingere", c401_fct, c401_nir,
       "TotalStingere" - (c401_fct + c401_nir) AS delta, tva, baza, regimuri
FROM tr."FctCifre" WHERE are_nir AND "TotalStingere" <> c401_fct + c401_nir
ORDER BY abs("TotalStingere" - (c401_fct + c401_nir)) DESC LIMIT 3;

\echo
\echo ===== B2b.4 FCT fara NIR: cine e diferit =====
SELECT COALESCE(regimuri,'(fara rand TVA)') AS regimuri, count(*) AS facturi,
       count(*) FILTER (WHERE "TotalStingere" = tert_fct) AS exact,
       sum("TotalStingere" - tert_fct) AS delta
FROM tr."FctCifre" WHERE NOT are_nir GROUP BY 1 ORDER BY 2 DESC;
