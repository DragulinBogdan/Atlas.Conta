\pset footer off
\echo ===== R5.1 notele 891 = 3xx: ce sunt =====
SELECT to_char(r."Data",'YYYY-MM') AS luna, count(*) AS randuri,
       count(*) FILTER (WHERE r."Valoare"<0) AS negative,
       round(sum(abs(r."Valoare")),2) AS suma_abs, round(sum(r."Valoare"),2) AS suma
FROM "RegistruContabil" r JOIN "Conturi" cd ON cd."ID"=r."ContDebitId" JOIN "Conturi" cc ON cc."ID"=r."ContCreditId"
WHERE cd."Simbol"='891' AND cc."Simbol" ~ '^3' GROUP BY 1 ORDER BY 1;
\echo
\echo ===== R5.2 toate corespondentele pe 891 (orice cont), pe luna =====
SELECT cd."Simbol"||' = '||cc."Simbol" AS corespondenta, count(*) AS randuri, round(sum(r."Valoare"),2) AS suma
FROM "RegistruContabil" r JOIN "Conturi" cd ON cd."ID"=r."ContDebitId" JOIN "Conturi" cc ON cc."ID"=r."ContCreditId"
WHERE cd."Simbol" LIKE '89%' OR cc."Simbol" LIKE '89%' GROUP BY 1 ORDER BY 2 DESC LIMIT 15;
\echo
\echo ===== R5.3 documentele care le poarta =====
SELECT d."ClrType", d."Numar", count(*) AS randuri FROM "RegistruContabil" r
JOIN "Conturi" cd ON cd."ID"=r."ContDebitId" JOIN "Documente" d ON d."ID"=r."DocumentId"
WHERE cd."Simbol"='891' GROUP BY 1,2 ORDER BY 3 DESC LIMIT 8;
