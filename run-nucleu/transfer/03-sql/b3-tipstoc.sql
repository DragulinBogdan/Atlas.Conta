-- B3: TipStoc contra (Cont al laturii de valoare, Repartitor).
-- Doua definitii ale contului, pentru ca niciuna nu acopera singura tot:
--   (a) contul de stoc al PRODUSULUI: Lot -> Produs -> TipMaterial.ContImplicit
--       (acoperire totala, calea folosita si de SAF-T S);
--   (b) contul POSTAT: rand contabil pe acelasi DetaliuId, latura aleasa dupa
--       semnul cantitatii (intrare => debit, iesire => credit).
\echo == B3.1 (a) TipStoc x cont produs x clasa repartitor x repartitor ==
SELECT "TipStoc", simbol_tip_material AS cont_produs, rep_clr, rep_cod, count(*) randuri
FROM tr3.stoc GROUP BY 1,2,3,4 ORDER BY 1,5 DESC;

\echo == B3.2 (a) exista doua TipStoc pe ACEEASI (cont produs, repartitor)? ==
SELECT simbol_tip_material AS cont_produs, rep_clr, rep_cod,
       count(DISTINCT "TipStoc") nr_tipstoc, string_agg(DISTINCT "TipStoc"::text,',') tipstoc, count(*) randuri
FROM tr3.stoc GROUP BY 1,2,3 HAVING count(DISTINCT "TipStoc")>1 ORDER BY 6 DESC;

\echo == B3.3 (b) contul POSTAT pe latura semnului, pe TipStoc ==
WITH pereche AS (
  SELECT s."ID", s."TipStoc", s."Cantitate", s.rep_clr, s.rep_cod, s.tip,
         (SELECT CASE WHEN s."Cantitate">=0 THEN c.simbol_d ELSE c.simbol_c END
          FROM tr3.cont c WHERE c."DetaliuId"=s."DetaliuId"
            AND (CASE WHEN s."Cantitate">=0 THEN c.simbol_d ELSE c.simbol_c END) LIKE '3%'
          ORDER BY c."ID" LIMIT 1) AS cont_postat
  FROM tr3.stoc s)
SELECT "TipStoc", COALESCE(cont_postat,'(fara rand contabil pe latura)') cont_postat, rep_clr, rep_cod, count(*) randuri
FROM pereche GROUP BY 1,2,3,4 ORDER BY 1,5 DESC;

\echo == B3.4 (b) coliziuni: doua TipStoc pe aceeasi (cont postat, repartitor) ==
WITH pereche AS (
  SELECT s."ID", s."TipStoc", s.rep_cod, s.rep_clr,
         (SELECT CASE WHEN s."Cantitate">=0 THEN c.simbol_d ELSE c.simbol_c END
          FROM tr3.cont c WHERE c."DetaliuId"=s."DetaliuId"
            AND (CASE WHEN s."Cantitate">=0 THEN c.simbol_d ELSE c.simbol_c END) LIKE '3%'
          ORDER BY c."ID" LIMIT 1) AS cont_postat
  FROM tr3.stoc s)
SELECT cont_postat, rep_clr, rep_cod, count(DISTINCT "TipStoc") nr, string_agg(DISTINCT "TipStoc"::text,',') tipstoc, count(*) randuri
FROM pereche WHERE cont_postat IS NOT NULL GROUP BY 1,2,3 HAVING count(DISTINCT "TipStoc")>1 ORDER BY 6 DESC;

\echo == B3.5 repartitorii care apar in RegistruStoc ==
SELECT rep_clr, rep_cod, rep_den, string_agg(DISTINCT "TipStoc"::text,',') tipstoc, count(*) randuri
FROM tr3.stoc GROUP BY 1,2,3 ORDER BY 5 DESC;
