-- B7 (v2): economia unificarii, cu regula RELAXATA de potrivire.
-- In cub, "Latura" e coordonata derivata; ce conteaza e ca exista un PICIOR de
-- postare pe un cont de stoc (3xx), pe acelasi DetaliuId, cu |valoare| egala.
-- Cub azi: 2 postari per rand contabil (D+C) + 2 per imperechere + 1 per rand
-- de stoc + 1 per rand fiscal (pas1/04-reconciliere.sql:55).
\echo == B7.1 clasificarea randurilor de stoc ==
WITH cls AS (
  SELECT st."ID", st.tip, st."TipStoc",
    CASE
      WHEN st."DetaliuId" IS NULL THEN 'A. deschidere (fara DetaliuId)'
      WHEN NOT EXISTS (SELECT 1 FROM tr3.cont rc WHERE rc."DetaliuId"=st."DetaliuId") THEN 'B. nicio nota pe linie'
      WHEN EXISTS (SELECT 1 FROM tr3.cont rc WHERE rc."DetaliuId"=st."DetaliuId"
                   AND abs(rc."Valoare")=abs(st."Valoare")
                   AND (rc.simbol_d LIKE '3%' OR rc.simbol_c LIKE '3%'))
        THEN 'C. picior 3xx cu |valoare| egala (unificabil)'
      WHEN EXISTS (SELECT 1 FROM tr3.cont rc WHERE rc."DetaliuId"=st."DetaliuId"
                   AND abs(rc."Valoare")=abs(st."Valoare"))
        THEN 'D. nota cu |valoare| egala dar fara picior 3xx'
      ELSE 'E. nota exista, nicio valoare egala'
    END AS clasa
  FROM tr3.stoc st)
SELECT clasa, count(*) randuri, round(100.0*count(*)/283498,2) pct_stoc,
       round(100.0*count(*)/1162622,2) pct_cub
FROM cls GROUP BY 1 ORDER BY 1;

\echo == B7.2 pe tip de document ==
WITH cls AS (
  SELECT st.tip, st."TipStoc",
    CASE
      WHEN st."DetaliuId" IS NULL THEN 'A deschidere'
      WHEN NOT EXISTS (SELECT 1 FROM tr3.cont rc WHERE rc."DetaliuId"=st."DetaliuId") THEN 'B fara nota'
      WHEN EXISTS (SELECT 1 FROM tr3.cont rc WHERE rc."DetaliuId"=st."DetaliuId"
                   AND abs(rc."Valoare")=abs(st."Valoare")
                   AND (rc.simbol_d LIKE '3%' OR rc.simbol_c LIKE '3%')) THEN 'C unificabil 3xx'
      WHEN EXISTS (SELECT 1 FROM tr3.cont rc WHERE rc."DetaliuId"=st."DetaliuId"
                   AND abs(rc."Valoare")=abs(st."Valoare")) THEN 'D valoare egala, fara 3xx'
      ELSE 'E separat' END AS clasa
  FROM tr3.stoc st)
SELECT tip, "TipStoc", clasa, count(*) randuri FROM cls GROUP BY 1,2,3 ORDER BY 1,2,4 DESC;

\echo == B7.3 contentie: per DetaliuId, cate randuri de stoc contra cate PICIOARE 3xx disponibile ==
WITH s AS (SELECT "DetaliuId" d, count(*) n FROM tr3.stoc WHERE "DetaliuId" IS NOT NULL GROUP BY 1),
     p AS (SELECT "DetaliuId" d,
             count(*) FILTER (WHERE simbol_d LIKE '3%') + count(*) FILTER (WHERE simbol_c LIKE '3%') AS picioare3
           FROM tr3.cont WHERE "DetaliuId" IS NOT NULL GROUP BY 1)
SELECT s.n AS randuri_stoc, COALESCE(p.picioare3,0) AS picioare_3xx, count(*) linii,
       sum(LEAST(s.n, COALESCE(p.picioare3,0))) AS potriviri_posibile
FROM s LEFT JOIN p ON p.d=s.d GROUP BY 1,2 ORDER BY 1,2;

\echo == B7.4 total: cate postari dispar din cub ==
WITH s AS (SELECT "DetaliuId" d, count(*) n FROM tr3.stoc WHERE "DetaliuId" IS NOT NULL GROUP BY 1),
     p AS (SELECT "DetaliuId" d,
             count(*) FILTER (WHERE simbol_d LIKE '3%') + count(*) FILTER (WHERE simbol_c LIKE '3%') AS picioare3
           FROM tr3.cont WHERE "DetaliuId" IS NOT NULL GROUP BY 1)
SELECT sum(LEAST(s.n, COALESCE(p.picioare3,0))) AS postari_care_dispar,
       283498 - sum(LEAST(s.n, COALESCE(p.picioare3,0))) AS postari_de_stoc_ramase,
       round(100.0*sum(LEAST(s.n, COALESCE(p.picioare3,0)))/1162622,2) AS pct_din_cub
FROM s LEFT JOIN p ON p.d=s.d;
