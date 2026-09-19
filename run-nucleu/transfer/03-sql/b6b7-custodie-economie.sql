\echo == B5.9 pretul liniei NIR (clona conexa nu copiaza PretUnitar) ==
SELECT 'NIR: linii cu PretUnitar=0' k, count(*)::text v FROM "DocumentDetalii" dd JOIN "Documente" n ON n."ID"=dd."DocumentId"
 WHERE n."ClrType"='NIR' AND dd."PretUnitar"=0
UNION ALL SELECT 'NIR: linii cu PretUnitar<>0', count(*)::text FROM "DocumentDetalii" dd JOIN "Documente" n ON n."ID"=dd."DocumentId"
 WHERE n."ClrType"='NIR' AND dd."PretUnitar"<>0
UNION ALL SELECT 'FCT: linii cu PretUnitar=0', count(*)::text FROM "DocumentDetalii" dd JOIN "Documente" f ON f."ID"=dd."DocumentId"
 WHERE f."ClrType"='FacturaIntrare' AND dd."PretUnitar"=0;

\echo == B5.10 ce posteaza FCT si ce posteaza NIR (conturi, pe rand contabil) ==
SELECT tip, simbol_d||' = '||simbol_c AS corespondenta, count(*) randuri, sum("Valoare") suma
FROM tr3.cont WHERE tip IN ('FacturaIntrare','NIR') GROUP BY 1,2 ORDER BY 1,3 DESC;

\echo == B5.11 regulile de contare pe FCT si NIR ==
SELECT td."Cod" tip, tm."Cod" tip_material, rc."NaturaFiltru", rc."SemnFiltru", rc."PastreazaSemn",
       rc."SursaContDebit", cd."Simbol" cont_d, rc."SursaContCredit", cc."Simbol" cont_c
FROM "ReguliContare" rc JOIN "TipuriDocument" td ON td."ID"=rc."TipDocumentId"
LEFT JOIN "TipuriMaterial" tm ON tm."ID"=rc."TipMaterialId"
LEFT JOIN "Conturi" cd ON cd."ID"=rc."ContDebitId" LEFT JOIN "Conturi" cc ON cc."ID"=rc."ContCreditId"
WHERE td."Cod" IN ('FCT','NIR','DSC','BCS','ASM','BTR','LDI','RLF','RDC') ORDER BY 1,2;

\echo == B6 custodie / 803x ==
SELECT 'RegistruStoc TipStoc=4 (Custodie)' k, count(*)::text v FROM "RegistruStoc" WHERE "TipStoc"=4
UNION ALL SELECT 'RegistruStoc TipStoc 3/6/7 (Folosinta/Gratuit/PN)', count(*)::text FROM "RegistruStoc" WHERE "TipStoc" IN (3,6,7)
UNION ALL SELECT 'Conturi 80xx in plan', count(*)::text FROM "Conturi" WHERE "Simbol" LIKE '80%'
UNION ALL SELECT 'RegistruContabil pe cont 80xx', count(*)::text FROM tr3.cont WHERE simbol_d LIKE '80%' OR simbol_c LIKE '80%';
SELECT "Simbol","Denumire" FROM "Conturi" WHERE "Simbol" LIKE '80%' ORDER BY 1;

\echo == B7 economia unificarii: randuri de stoc cu / fara postare 3xx pereche 1:1 ==
WITH s AS (SELECT "DetaliuId" d, count(*) n FROM tr3.stoc WHERE "DetaliuId" IS NOT NULL GROUP BY 1),
     c AS (SELECT "DetaliuId" d, count(*) n FROM tr3.cont WHERE "DetaliuId" IS NOT NULL GROUP BY 1),
     cls AS (
       SELECT st."ID", st.tip,
         CASE
           WHEN st."DetaliuId" IS NULL THEN 'deschidere (fara detaliu)'
           WHEN NOT EXISTS (SELECT 1 FROM tr3.cont rc WHERE rc."DetaliuId"=st."DetaliuId") THEN 'fara nicio nota'
           WHEN EXISTS (SELECT 1 FROM tr3.cont rc WHERE rc."DetaliuId"=st."DetaliuId"
                        AND abs(rc."Valoare")=abs(st."Valoare")
                        AND ((st."Cantitate">=0 AND rc.simbol_d LIKE '3%') OR (st."Cantitate"<0 AND rc.simbol_c LIKE '3%')))
             THEN 'nota 3xx pe latura semnului, aceeasi valoare (unificabila)'
           ELSE 'nota exista dar nu se potriveste (ramane separata)'
         END AS clasa
       FROM tr3.stoc st)
SELECT clasa, count(*) randuri, round(100.0*count(*)/(SELECT count(*) FROM tr3.stoc),2) pct_din_stoc
FROM cls GROUP BY 1 ORDER BY 2 DESC;

\echo == B7b detaliat pe tip ==
WITH cls AS (
  SELECT st."ID", st.tip,
    CASE
      WHEN st."DetaliuId" IS NULL THEN 'deschidere'
      WHEN NOT EXISTS (SELECT 1 FROM tr3.cont rc WHERE rc."DetaliuId"=st."DetaliuId") THEN 'fara nota'
      WHEN EXISTS (SELECT 1 FROM tr3.cont rc WHERE rc."DetaliuId"=st."DetaliuId"
                   AND abs(rc."Valoare")=abs(st."Valoare")
                   AND ((st."Cantitate">=0 AND rc.simbol_d LIKE '3%') OR (st."Cantitate"<0 AND rc.simbol_c LIKE '3%')))
        THEN 'unificabila'
      ELSE 'separata'
    END AS clasa
  FROM tr3.stoc st)
SELECT tip, clasa, count(*) randuri FROM cls GROUP BY 1,2 ORDER BY 1,3 DESC;

\echo == B7c cubul: cate postari ar disparea, procent din f2.Postare ==
SELECT (SELECT count(*) FROM f2."Postare") postari_cub,
       (SELECT count(*) FROM f2."Postare" WHERE "Spatiu"=2) postari_stoc,
       (SELECT count(*) FROM f2."Postare" WHERE "Spatiu"=1) postari_contabil,
       (SELECT count(*) FROM f2."Postare" WHERE "Spatiu"=3) postari_fiscal;
