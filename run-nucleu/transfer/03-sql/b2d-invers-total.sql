\echo == B2.9 total picioare 3xx fara rand de stoc, pe tip ==
WITH l AS (
  SELECT "ID", tip, "DetaliuId", simbol, latura, "Valoare" FROM (
    SELECT "ID", tip, "DetaliuId", simbol_d simbol, 'D' latura, "Valoare" FROM tr3.cont
    UNION ALL SELECT "ID", tip, "DetaliuId", simbol_c simbol, 'C' latura, "Valoare" FROM tr3.cont) k
  WHERE simbol LIKE '3%')
SELECT tip, count(*) picioare_3xx,
  count(*) FILTER (WHERE "DetaliuId" IS NULL OR NOT EXISTS (SELECT 1 FROM tr3.stoc s WHERE s."DetaliuId"=l."DetaliuId")) fara_stoc
FROM l GROUP BY 1 ORDER BY 3 DESC;

\echo == B2.10 anomalia: linia de FCT care posteaza direct 371 ==
SELECT d."Numar", d."Data", d."ClrType", c.simbol_d, c.simbol_c, c."Valoare", c."DetaliuId"
FROM tr3.cont c JOIN "Documente" d ON d."ID"=c."DocumentId"
WHERE c.tip='FacturaIntrare' AND (c.simbol_d='371' OR c.simbol_c='371');

\echo == B2.11 notele contabile pe 3xx: ce corespondente ==
SELECT simbol_d||' = '||simbol_c corespondenta, count(*) randuri, sum("Valoare") suma
FROM tr3.cont WHERE tip='NotaContabila' AND (simbol_d LIKE '3%' OR simbol_c LIKE '3%')
GROUP BY 1 ORDER BY 2 DESC LIMIT 20;

\echo == B5.12 FCT: cate linii pe natura, si cate au lot ==
SELECT cp."Natura", count(*) linii, count(*) FILTER (WHERE dd."LotId" IS NOT NULL) cu_lot
FROM "DocumentDetalii" dd JOIN "Documente" f ON f."ID"=dd."DocumentId"
LEFT JOIN "TipuriMaterial" tm ON tm."ID"=dd."TipMaterialId" LEFT JOIN "ClaseProduse" cp ON cp."ID"=tm."ClasaId"
WHERE f."ClrType"='FacturaIntrare' GROUP BY 1 ORDER BY 2 DESC;
\echo == B5.13 NIR: linii, si cate au lot ==
SELECT count(*) linii_nir, count(*) FILTER (WHERE dd."LotId" IS NOT NULL) cu_lot
FROM "DocumentDetalii" dd JOIN "Documente" n ON n."ID"=dd."DocumentId" WHERE n."ClrType"='NIR';
