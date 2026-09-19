\pset footer off
\echo ===== R4.1 picioarele pe 3xx FARA rand de stoc (TR-r2): corespondenta si suma =====
WITH p3 AS (
  SELECT r."ID", r."DetaliuId", r."DocumentId", r."Valoare",
         cd."Simbol" AS d, cc."Simbol" AS c,
         CASE WHEN cd."Simbol" ~ '^3' THEN 1 ELSE 2 END AS latura_stoc
  FROM "RegistruContabil" r
  JOIN "Conturi" cd ON cd."ID"=r."ContDebitId" JOIN "Conturi" cc ON cc."ID"=r."ContCreditId"
  WHERE (cd."Simbol" ~ '^3' OR cc."Simbol" ~ '^3')
), fara AS (
  SELECT p.*, COALESCE(doc."ClrType",'(deschidere)') AS tip FROM p3 p
  LEFT JOIN "Documente" doc ON doc."ID"=p."DocumentId"
  WHERE NOT EXISTS (SELECT 1 FROM "RegistruStoc" s WHERE s."DetaliuId" IS NOT NULL AND s."DetaliuId"=p."DetaliuId")
)
SELECT tip, d||' = '||c AS corespondenta, count(*) AS randuri, round(sum("Valoare"),2) AS suma
FROM fara GROUP BY 1,2 HAVING count(*) >= 10 ORDER BY 3 DESC;

\echo
\echo ===== R4.2 total pe tip =====
WITH p3 AS (
  SELECT r."ID", r."DetaliuId", r."DocumentId", r."Valoare" FROM "RegistruContabil" r
  JOIN "Conturi" cd ON cd."ID"=r."ContDebitId" JOIN "Conturi" cc ON cc."ID"=r."ContCreditId"
  WHERE (cd."Simbol" ~ '^3' OR cc."Simbol" ~ '^3'))
SELECT COALESCE(doc."ClrType",'(deschidere)') AS tip, count(*) AS randuri, round(sum(p."Valoare"),2) AS suma
FROM p3 p LEFT JOIN "Documente" doc ON doc."ID"=p."DocumentId"
WHERE NOT EXISTS (SELECT 1 FROM "RegistruStoc" s WHERE s."DetaliuId" IS NOT NULL AND s."DetaliuId"=p."DetaliuId")
GROUP BY 1 ORDER BY 2 DESC;

\echo
\echo ===== R4.3 FCT si NIR-ul ei: aceeasi luna de DataInregistrare? (gate-ul TR-D7) =====
SELECT count(*) AS perechi,
       count(*) FILTER (WHERE date_trunc('month', f."DataInregistrare") = date_trunc('month', n."DataInregistrare")) AS aceeasi_luna,
       count(*) FILTER (WHERE f."DataInregistrare" <> n."DataInregistrare") AS zile_diferite,
       count(*) FILTER (WHERE date_trunc('month', f."Data") <> date_trunc('month', n."Data")) AS luna_fizica_diferita
FROM "Documente" n JOIN "Documente" f ON f."ID"=n."DocumentSursaId"
WHERE n."ClrType"='NIR' AND n."GCRecord"=0;

\echo
\echo ===== R4.4 perechea 1:1 stoc<->contabil: contul piciorului 3xx == ContImplicit al produsului? =====
WITH per AS (
  SELECT s."ID" AS sid, s."DetaliuId", s."Valoare" AS vstoc, t."ContImplicitId" AS cont_produs
  FROM "RegistruStoc" s JOIN "Loturi" l ON l."ID"=s."LotId" JOIN "Produse" pr ON pr."ID"=l."ProdusId"
       JOIN "TipuriMaterial" t ON t."ID"=pr."TipMaterialId"
  WHERE s."DetaliuId" IS NOT NULL),
nota AS (
  SELECT r."DetaliuId", count(*) AS n, min(r."ID"::text) AS rid FROM "RegistruContabil" r GROUP BY 1)
SELECT count(*) AS perechi_1_1,
       count(*) FILTER (WHERE abs(abs(p.vstoc) - abs(r."Valoare")) > 0.004) AS valoare_diferita,
       count(*) FILTER (WHERE p.cont_produs NOT IN (r."ContDebitId", r."ContCreditId")) AS cont_produs_neatins
FROM per p JOIN nota nn ON nn."DetaliuId"=p."DetaliuId" AND nn.n=1
     JOIN "RegistruContabil" r ON r."ID"::text=nn.rid
WHERE (SELECT count(*) FROM "RegistruStoc" s2 WHERE s2."DetaliuId"=p."DetaliuId") = 1;
