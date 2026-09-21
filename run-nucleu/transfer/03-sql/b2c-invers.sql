-- B2 (invers): randuri CONTABILE pe conturi de stoc fara rand de stoc pereche.
\echo == B2.6 conturile de stoc (TipMaterial.ContImplicit) ==
SELECT DISTINCT c."Simbol", c."Denumire" FROM "TipuriMaterial" tm JOIN "Conturi" c ON c."ID"=tm."ContImplicitId" ORDER BY 1;

\echo == B2.7 randuri contabile care ating un cont 3xx, pe tip x cont x latura x are rand de stoc pe acelasi DetaliuId ==
WITH l AS (
  SELECT k."ID", k.tip, k."DetaliuId", k.simbol, k.latura, k."Valoare"
  FROM (
    SELECT "ID", tip, "DetaliuId", simbol_d AS simbol, 'D' AS latura, "Valoare" FROM tr3.cont
    UNION ALL
    SELECT "ID", tip, "DetaliuId", simbol_c AS simbol, 'C' AS latura, "Valoare" FROM tr3.cont
  ) k WHERE k.simbol LIKE '3%')
SELECT l.tip, l.simbol, l.latura,
       count(*) randuri,
       count(*) FILTER (WHERE l."DetaliuId" IS NOT NULL AND EXISTS (SELECT 1 FROM tr3.stoc s WHERE s."DetaliuId"=l."DetaliuId")) AS cu_rand_stoc,
       count(*) FILTER (WHERE l."DetaliuId" IS NULL OR NOT EXISTS (SELECT 1 FROM tr3.stoc s WHERE s."DetaliuId"=l."DetaliuId")) AS fara_rand_stoc,
       sum(l."Valoare") suma
FROM l GROUP BY 1,2,3 ORDER BY 4 DESC;

\echo == B2.8 perechea 1:2 detaliata (tip, conturile celor doua note) ==
WITH s AS (SELECT "DetaliuId" d, count(*) n, min("ID"::text) id FROM tr3.stoc WHERE "DetaliuId" IS NOT NULL GROUP BY 1),
     c AS (SELECT "DetaliuId" d, count(*) n FROM tr3.cont WHERE "DetaliuId" IS NOT NULL GROUP BY 1)
SELECT st.tip, st."TipStoc", count(*) linii,
       count(*) FILTER (WHERE EXISTS (SELECT 1 FROM tr3.cont rc WHERE rc."DetaliuId"=s.d AND abs(rc."Valoare")=abs(st."Valoare"))) AS are_nota_egala,
       string_agg(DISTINCT (SELECT string_agg(rc.simbol_d||'/'||rc.simbol_c,' + ' ORDER BY rc.simbol_d) FROM tr3.cont rc WHERE rc."DetaliuId"=s.d), ' | ') conturi
FROM s JOIN c ON c.d=s.d AND s.n=1 AND c.n=2
JOIN tr3.stoc st ON st."ID"::text=s.id
GROUP BY 1,2;
