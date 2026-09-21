-- B2 (continuare): comparatia de VALOARE pe perechile 1:1, 1:2 si 2:1.
-- Fara min(ID): cardinalitatea o da `n`, deci join-ul direct pe DetaliuId
-- intoarce exact o pereche pe linie acolo unde n=1 de ambele parti.
\echo == B2.3 perechi 1:1 -- |stoc.Valoare| contra contabil.Valoare ==
WITH s AS (SELECT "DetaliuId" d, count(*) n FROM tr3.stoc WHERE "DetaliuId" IS NOT NULL GROUP BY 1),
     c AS (SELECT "DetaliuId" d, count(*) n FROM tr3.cont WHERE "DetaliuId" IS NOT NULL GROUP BY 1),
     p AS (SELECT rs.tip, rs."TipStoc", rs."Valoare" vs, rc."Valoare" vc
           FROM s JOIN c ON c.d=s.d AND s.n=1 AND c.n=1
           JOIN tr3.stoc rs ON rs."DetaliuId"=s.d
           JOIN tr3.cont rc ON rc."DetaliuId"=s.d)
SELECT tip, "TipStoc",
       count(*) FILTER (WHERE abs(vs)=abs(vc)) AS egale_abs,
       count(*) FILTER (WHERE abs(vs)<>abs(vc)) AS diferite,
       coalesce(sum(abs(vs)-abs(vc)) FILTER (WHERE abs(vs)<>abs(vc)),0) AS delta_total,
       count(*) AS total
FROM p GROUP BY 1,2 ORDER BY 6 DESC;

\echo == B2.3b exemple de perechi 1:1 cu valoare diferita (max 10) ==
WITH s AS (SELECT "DetaliuId" d, count(*) n FROM tr3.stoc WHERE "DetaliuId" IS NOT NULL GROUP BY 1),
     c AS (SELECT "DetaliuId" d, count(*) n FROM tr3.cont WHERE "DetaliuId" IS NOT NULL GROUP BY 1)
SELECT rs.tip, rs."Valoare" v_stoc, rc."Valoare" v_nota, abs(rs."Valoare")-abs(rc."Valoare") delta, s.d "DetaliuId"
FROM s JOIN c ON c.d=s.d AND s.n=1 AND c.n=1
JOIN tr3.stoc rs ON rs."DetaliuId"=s.d JOIN tr3.cont rc ON rc."DetaliuId"=s.d
WHERE abs(rs."Valoare")<>abs(rc."Valoare") LIMIT 10;

\echo == B2.4 perechi 1:2 -- linia are 2 note (net + TVA) ==
WITH s AS (SELECT "DetaliuId" d, count(*) n FROM tr3.stoc WHERE "DetaliuId" IS NOT NULL GROUP BY 1),
     c AS (SELECT "DetaliuId" d, count(*) n FROM tr3.cont WHERE "DetaliuId" IS NOT NULL GROUP BY 1)
SELECT rs.tip, rs."TipStoc", count(*) linii,
       count(*) FILTER (WHERE EXISTS (SELECT 1 FROM tr3.cont rc WHERE rc."DetaliuId"=s.d AND abs(rc."Valoare")=abs(rs."Valoare"))) AS are_nota_egala
FROM s JOIN c ON c.d=s.d AND s.n=1 AND c.n=2
JOIN tr3.stoc rs ON rs."DetaliuId"=s.d
GROUP BY 1,2;

\echo == B2.5 perechi 2:1 (BCS) -- suma algebrica a celor doua randuri de stoc contra notei ==
WITH s AS (SELECT "DetaliuId" d, count(*) n, sum("Valoare") vs, max(abs("Valoare")) vmax FROM tr3.stoc WHERE "DetaliuId" IS NOT NULL GROUP BY 1),
     c AS (SELECT "DetaliuId" d, count(*) n, sum("Valoare") vc FROM tr3.cont WHERE "DetaliuId" IS NOT NULL GROUP BY 1)
SELECT rc.tip, count(*) linii,
       count(*) FILTER (WHERE s.vs=0) suma_stoc_zero,
       count(*) FILTER (WHERE s.vmax=abs(c.vc)) nota_egala_cu_max,
       sum(s.vs) suma_stoc, sum(c.vc) suma_nota
FROM s JOIN c ON c.d=s.d AND s.n=2 AND c.n=1
JOIN tr3.cont rc ON rc."DetaliuId"=s.d
GROUP BY 1;
