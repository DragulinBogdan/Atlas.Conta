-- B6c. Testul care separa definitiile: suma partidelor = soldul conturilor de tert?
-- (o partida e o FRACTIE a soldului contului; daca sumele nu se inchid,
-- definitia nu e o partitie a soldului).
\set ON_ERROR_STOP on
\pset footer off

\echo ===== B6c.1 soldul REAL al conturilor de tert la 31.12.2025 =====
SELECT t."ContSimbol", max(t."RolTert") AS rol, sum(t."Natural") AS sold_natural
FROM tr."TertLa1231" t GROUP BY 1 ORDER BY 1;

\echo
\echo ===== B6c.2 total pe rol =====
SELECT max("RolTert") AS rol_grup, sum("Natural") AS sold FROM tr."TertLa1231"
GROUP BY "RolTert" ORDER BY 1;
SELECT sum("Natural") AS sold_total_tert FROM tr."TertLa1231";

\echo
\echo ===== B6c.3 Sigma partide contra sold, pe fiecare definitie =====
SELECT 'sold real (Sigma postari de tert)' AS ce, sum("Natural") AS suma FROM tr."TertLa1231"
UNION ALL SELECT 'A: Sigma rest', sum("Rest") FROM tr."PartidaA"
UNION ALL SELECT 'B: Sigma rest', sum("Rest") FROM tr."PartidaB"
UNION ALL SELECT 'C: Sigma rest', sum("Rest") FROM tr."PartidaC"
UNION ALL SELECT 'F0 (PartideDeschise 12/2025)', sum("Rest") FROM "PartideDeschise" WHERE "An"=2025 AND "Luna"=12
UNION ALL SELECT 'F0 onest (TotalStingere - Asignat)', sum("Rest") FROM tr."F0Rest"
UNION ALL SELECT 'F0 onest, doar documentele cu postare de tert',
       sum(f."Rest") FROM tr."F0Rest" f WHERE EXISTS (SELECT 1 FROM tr."TertLa1231" t WHERE t."DocumentId"=f."DocumentId");

\echo
\echo ===== B6c.4 F0 onest pe documentele FARA nicio postare de tert =====
SELECT d."ClrType", count(*) AS partide, sum(f."Rest") AS suma
FROM tr."F0Rest" f JOIN tr."Doc" d ON d."ID" = f."DocumentId"
WHERE NOT EXISTS (SELECT 1 FROM tr."TertLa1231" t WHERE t."DocumentId" = f."DocumentId")
GROUP BY 1 ORDER BY 3 DESC;

\echo
\echo ===== B6c.5 dubla partida a lantului: NIR + FCT in F0 onest =====
SELECT count(*) AS lanturi,
       count(*) FILTER (WHERE fn."Rest" IS NOT NULL AND ff."Rest" IS NOT NULL) AS ambele_cu_rest,
       sum(COALESCE(fn."Rest",0)) AS rest_nir, sum(COALESCE(ff."Rest",0)) AS rest_fct,
       sum(COALESCE(fn."Rest",0) + COALESCE(ff."Rest",0)) AS rest_lant_f0,
       sum(abs(pc."Rest")) AS rest_lant_cub
FROM tr."FctNir" x
LEFT JOIN tr."F0Rest" fn ON fn."DocumentId" = x."NirId"
LEFT JOIN tr."F0Rest" ff ON ff."DocumentId" = x."FctId"
LEFT JOIN tr."PartidaC" pc ON pc."Cheie" = x."FctId";
