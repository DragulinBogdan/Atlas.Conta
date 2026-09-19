\pset footer off
\timing off
CREATE TEMP TABLE tc AS SELECT "DocumentId" d, "Cont" c, "ContSimbol" s, "Natural" n FROM tr."TertLa1231";
CREATE INDEX ON tc(d); ANALYZE tc;
CREATE TEMP TABLE imp AS SELECT "ID", "DocumentId" stins, "DocumentStingatorId" sting, "Suma" FROM "Imperecheri" WHERE "GCRecord"=0;
CREATE INDEX ON imp(stins); CREATE INDEX ON imp(sting); ANALYZE imp;

\echo == R7.1 imperecherile pe conturi ==
WITH cs AS (SELECT i."ID", bool_or(true) AS are FROM imp i JOIN tc a ON a.d=i.stins JOIN tc b ON b.d=i.sting AND b.c=a.c GROUP BY 1),
     st AS (SELECT i."ID", count(DISTINCT a.c) AS nc FROM imp i JOIN tc a ON a.d=i.stins GROUP BY 1)
SELECT (SELECT count(*) FROM imp) AS imperecheri,
       (SELECT count(*) FROM cs) AS au_cont_comun,
       (SELECT count(*) FROM st WHERE nc>0) - (SELECT count(*) FROM cs) AS stins_doar_pe_alte_conturi,
       (SELECT count(*) FROM st WHERE nc>1) AS stins_cu_2plus_conturi,
       (SELECT count(*) FROM imp WHERE stins NOT IN (SELECT d FROM tc)) AS stins_fara_nicio_postare_tert;

\echo
\echo == R7.2 FCL cu doua partide (4111 + 419) ==
WITH f AS (SELECT t.d, sum(t.n) FILTER (WHERE t.s='4111') p4111, sum(t.n) FILTER (WHERE t.s='419') p419
           FROM tc t JOIN tr."Doc" x ON x."ID"=t.d WHERE x."ClrType"='FacturaIesire' GROUP BY 1 HAVING count(*)>1)
SELECT count(*) fcl_doua_partide, round(sum(p4111),2) sigma_4111, round(sum(p419),2) sigma_419,
       round(sum(COALESCE(a."Asignat",0)),2) sigma_stins,
       count(*) FILTER (WHERE COALESCE(a."Asignat",0) > p4111+0.005) stingere_peste_partida_4111
FROM f LEFT JOIN tr."Asignat" a ON a."DocumentId"=f.d;
