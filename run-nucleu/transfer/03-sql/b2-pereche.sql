-- B2: perechea rand de stoc <-> rand contabil, cheia comuna = DetaliuId.
-- (RegistruStoc n-are FK spre cont; DetaliuId e singura cheie comuna, plus
--  DocumentId. Randurile de deschidere n-au DetaliuId.)
\echo == B2.0 acoperirea cheii ==
SELECT 'stoc cu DetaliuId' k, count(*) v FROM "RegistruStoc" WHERE "DetaliuId" IS NOT NULL
UNION ALL SELECT 'stoc fara DetaliuId', count(*) FROM "RegistruStoc" WHERE "DetaliuId" IS NULL
UNION ALL SELECT 'contabil cu DetaliuId', count(*) FROM "RegistruContabil" WHERE "DetaliuId" IS NOT NULL
UNION ALL SELECT 'contabil fara DetaliuId', count(*) FROM "RegistruContabil" WHERE "DetaliuId" IS NULL;

\echo == B2.1 cardinalitatea pe DetaliuId (cate randuri de stoc x cate randuri contabile) ==
WITH s AS (SELECT "DetaliuId" d, count(*) n FROM "RegistruStoc" WHERE "DetaliuId" IS NOT NULL GROUP BY 1),
     c AS (SELECT "DetaliuId" d, count(*) n FROM "RegistruContabil" WHERE "DetaliuId" IS NOT NULL GROUP BY 1)
SELECT COALESCE(s.n,0) AS randuri_stoc, COALESCE(c.n,0) AS randuri_contabil, count(*) AS linii
FROM s FULL JOIN c ON c.d=s.d GROUP BY 1,2 ORDER BY 1,2;

\echo == B2.2 randuri de stoc fara NICIUN rand contabil pe acelasi DetaliuId, pe tip ==
SELECT COALESCE(d."ClrType",'(deschidere)') tip, rs."TipStoc", count(*) randuri, sum(rs."Valoare") suma_valoare
FROM "RegistruStoc" rs
LEFT JOIN "Documente" d ON d."ID"=rs."DocumentId"
WHERE rs."DetaliuId" IS NULL
   OR NOT EXISTS (SELECT 1 FROM "RegistruContabil" rc WHERE rc."DetaliuId"=rs."DetaliuId")
GROUP BY 1,2 ORDER BY 3 DESC;
