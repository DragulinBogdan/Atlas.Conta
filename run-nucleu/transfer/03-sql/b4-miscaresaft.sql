-- B4: PoliticaMiscareSaft -- cheia de azi contra (tip document x Cont x semn x rol tert).
\echo == B4.1 randurile de politica (seed privat) ==
SELECT td."Cod" tip, p."TipStoc", p."Semn", p."CodMiscare", p."RolTert", p."Motiv", p."DinSeed"
FROM "PoliticiMiscareSaft" p JOIN "TipuriDocument" td ON td."ID"=p."TipDocumentId"
ORDER BY 1,2,3;

\echo == B4.2 randurile REALE de stoc pe cheia de azi -> cod, si pe cheia noua ==
-- semnRegula = (Storno ? -1 : +1) * sign(Cantitate)  (SaftProiectii.cs:1596-1603)
WITH r AS (
  SELECT s."ID", s.tip AS clr, s."TipStoc", s.simbol_tip_material AS cont,
         CASE WHEN s."Storno" THEN -1 ELSE 1 END * (CASE WHEN s."Cantitate">0 THEN 1 WHEN s."Cantitate"<0 THEN -1 ELSE 0 END) AS semn_regula,
         s."DocumentId"
  FROM tr3.stoc s WHERE s."DocumentId" IS NOT NULL),
pol AS (
  SELECT td."ClrType" clr, p."TipStoc", p."Semn", p."CodMiscare", p."RolTert"
  FROM "PoliticiMiscareSaft" p JOIN "TipuriDocument" td ON td."ID"=p."TipDocumentId"),
m AS (
  SELECT r.*, COALESCE(pe."CodMiscare", pg."CodMiscare") cod,
              COALESCE(pe."RolTert",   pg."RolTert")    rol,
              (pe."CodMiscare" IS NULL AND pg."CodMiscare" IS NULL
                AND pe."TipStoc" IS NULL AND pg."TipStoc" IS NULL) fara_politica
  FROM r
  LEFT JOIN pol pe ON pe.clr=r.clr AND pe."TipStoc"=r."TipStoc" AND pe."Semn"=r.semn_regula
  LEFT JOIN pol pg ON pg.clr=r.clr AND pg."TipStoc"=r."TipStoc" AND pg."Semn" IS NULL)
SELECT clr, "TipStoc", semn_regula, cont, COALESCE(cod,'(fara cod)') cod, COALESCE(rol::text,'-') rol, count(*) randuri
FROM m GROUP BY 1,2,3,4,5,6 ORDER BY 1,2,3,4;

\echo == B4.3 INJECTIVITATE: cheia noua (tip document x cont x semn) -> cate coduri distincte ==
WITH r AS (
  SELECT s."ID", s.tip AS clr, s."TipStoc", s.simbol_tip_material AS cont,
         CASE WHEN s."Storno" THEN -1 ELSE 1 END * (CASE WHEN s."Cantitate">0 THEN 1 WHEN s."Cantitate"<0 THEN -1 ELSE 0 END) AS semn_regula
  FROM tr3.stoc s WHERE s."DocumentId" IS NOT NULL),
pol AS (
  SELECT td."ClrType" clr, p."TipStoc", p."Semn", p."CodMiscare", p."RolTert"
  FROM "PoliticiMiscareSaft" p JOIN "TipuriDocument" td ON td."ID"=p."TipDocumentId"),
m AS (
  SELECT r.*, COALESCE(pe."CodMiscare", pg."CodMiscare") cod, COALESCE(pe."RolTert", pg."RolTert") rol
  FROM r LEFT JOIN pol pe ON pe.clr=r.clr AND pe."TipStoc"=r."TipStoc" AND pe."Semn"=r.semn_regula
         LEFT JOIN pol pg ON pg.clr=r.clr AND pg."TipStoc"=r."TipStoc" AND pg."Semn" IS NULL)
SELECT clr, cont, semn_regula,
       count(DISTINCT COALESCE(cod,'(null)')) coduri_distincte,
       string_agg(DISTINCT COALESCE(cod,'(null)'),',') coduri,
       count(DISTINCT COALESCE(rol::text,'-')) roluri_distincte,
       string_agg(DISTINCT COALESCE(rol::text,'-'),',') roluri,
       string_agg(DISTINCT "TipStoc"::text,',') tipstoc,
       count(*) randuri
FROM m GROUP BY 1,2,3
HAVING count(DISTINCT COALESCE(cod,'(null)'))>1 OR count(DISTINCT COALESCE(rol::text,'-'))>1
ORDER BY 9 DESC;

\echo == B4.4 acoperirea cheii noi: cate chei noi, cate chei vechi ==
WITH r AS (
  SELECT s.tip AS clr, s."TipStoc", s.simbol_tip_material AS cont,
         CASE WHEN s."Storno" THEN -1 ELSE 1 END * (CASE WHEN s."Cantitate">0 THEN 1 WHEN s."Cantitate"<0 THEN -1 ELSE 0 END) AS semn_regula
  FROM tr3.stoc s WHERE s."DocumentId" IS NOT NULL)
SELECT count(DISTINCT (clr,"TipStoc",semn_regula)) chei_azi, count(DISTINCT (clr,cont,semn_regula)) chei_noi FROM r;
