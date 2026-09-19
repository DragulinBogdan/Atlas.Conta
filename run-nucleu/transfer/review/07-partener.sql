\pset footer off
\echo == R8.1 postarile de tert FARA coordonata Partener, pe tip de document ==
SELECT COALESCE(d."ClrType",'(deschidere)') AS tip, p."ContSimbol" AS cont,
       count(*) AS picioare, count(*) FILTER (WHERE p."Partener" IS NULL) AS fara_partener,
       round(sum(p."Valoare") FILTER (WHERE p."Partener" IS NULL),2) AS suma_fara_partener
FROM tr."Picior" p LEFT JOIN tr."Doc" d ON d."ID"=p."DocumentId"
WHERE p."RolTert" <> 0
GROUP BY 1,2 HAVING count(*) FILTER (WHERE p."Partener" IS NULL) > 0
ORDER BY 4 DESC LIMIT 12;
\echo
\echo == R8.2 total ==
SELECT count(*) AS picioare_tert, count(*) FILTER (WHERE "Partener" IS NULL) AS fara_partener,
       round(100.0*count(*) FILTER (WHERE "Partener" IS NULL)/count(*),2) AS pct
FROM tr."Picior" WHERE "RolTert" <> 0;
