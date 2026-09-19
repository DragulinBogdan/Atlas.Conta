-- B5 (continuare): FCT <-> NIR, ClrType corect = 'NIR'.
\echo == B5.2 legatura la nivel de DOCUMENT ==
SELECT 'FCT total' k, count(*)::text v FROM "Documente" WHERE "ClrType"='FacturaIntrare'
UNION ALL SELECT 'NIR total', count(*)::text FROM "Documente" WHERE "ClrType"='NIR'
UNION ALL SELECT 'NIR cu DocumentSursa = FCT', count(*)::text FROM "Documente" n JOIN "Documente" f ON f."ID"=n."DocumentSursaId"
   WHERE n."ClrType"='NIR' AND f."ClrType"='FacturaIntrare'
UNION ALL SELECT 'NIR cu DocumentSursa (orice)', count(*)::text FROM "Documente" WHERE "ClrType"='NIR' AND "DocumentSursaId" IS NOT NULL
UNION ALL SELECT 'NIR fara sursa (manual)', count(*)::text FROM "Documente" WHERE "ClrType"='NIR' AND "DocumentSursaId" IS NULL
UNION ALL SELECT 'NIR Autogenerat', count(*)::text FROM "Documente" WHERE "ClrType"='NIR' AND "Autogenerat"
UNION ALL SELECT 'FCT fara niciun NIR conex', count(*)::text FROM "Documente" f
   WHERE f."ClrType"='FacturaIntrare' AND NOT EXISTS (SELECT 1 FROM "Documente" n WHERE n."DocumentSursaId"=f."ID" AND n."ClrType"='NIR')
UNION ALL SELECT 'NIR pe FCT: max NIR per FCT', COALESCE(max(c)::text,'0') FROM (
   SELECT f."ID", count(n."ID") c FROM "Documente" f LEFT JOIN "Documente" n ON n."DocumentSursaId"=f."ID" AND n."ClrType"='NIR'
   WHERE f."ClrType"='FacturaIntrare' GROUP BY 1) x;

\echo == B5.3 distributia NIR per FCT ==
SELECT c AS nir_per_fct, count(*) AS facturi FROM (
  SELECT f."ID", count(n."ID") c FROM "Documente" f LEFT JOIN "Documente" n ON n."DocumentSursaId"=f."ID" AND n."ClrType"='NIR'
  WHERE f."ClrType"='FacturaIntrare' GROUP BY 1) x GROUP BY 1 ORDER BY 1;

\echo == B5.4 FCT fara NIR: au linii de stoc? (natura clasei liniei) ==
WITH fct_fara AS (SELECT f."ID" FROM "Documente" f
  WHERE f."ClrType"='FacturaIntrare' AND NOT EXISTS (SELECT 1 FROM "Documente" n WHERE n."DocumentSursaId"=f."ID" AND n."ClrType"='NIR'))
SELECT cp."Natura", count(*) linii, count(DISTINCT dd."DocumentId") facturi
FROM "DocumentDetalii" dd JOIN fct_fara ON fct_fara."ID"=dd."DocumentId"
LEFT JOIN "TipuriMaterial" tm ON tm."ID"=dd."TipMaterialId" LEFT JOIN "ClaseProduse" cp ON cp."ID"=tm."ClasaId"
GROUP BY 1 ORDER BY 2 DESC;

\echo == B5.5 legatura la nivel de LINIE prin lot: Lot.LinieIntrareId ==
SELECT src."ClrType" AS linie_care_naste_lotul, count(*) loturi
FROM "Loturi" l JOIN "DocumentDetalii" src ON src."ID"=l."LinieIntrareId" GROUP BY 1 ORDER BY 2 DESC;
SELECT 'Loturi total' k, count(*) v FROM "Loturi"
UNION ALL SELECT 'Loturi fara LinieIntrareId', count(*) FROM "Loturi" WHERE "LinieIntrareId" IS NULL;

\echo == B5.6 perechea linie FCT -> linie NIR prin lot: cantitate si pret ==
WITH fl AS (SELECT dd."ID", dd."DocumentId", dd."Cantitate", dd."PretUnitar", dd."Valoare", dd."LotId"
            FROM "DocumentDetalii" dd JOIN "Documente" f ON f."ID"=dd."DocumentId" WHERE f."ClrType"='FacturaIntrare'),
     nl AS (SELECT dd."ID", dd."DocumentId", dd."Cantitate", dd."PretUnitar", dd."Valoare", dd."LotId"
            FROM "DocumentDetalii" dd JOIN "Documente" n ON n."ID"=dd."DocumentId" WHERE n."ClrType"='NIR')
SELECT count(*) perechi,
       count(*) FILTER (WHERE fl."Cantitate"=nl."Cantitate") cantitate_egala,
       count(*) FILTER (WHERE fl."Valoare"=nl."Valoare") valoare_egala,
       count(*) FILTER (WHERE fl."PretUnitar" IS NOT DISTINCT FROM nl."PretUnitar") pret_egal,
       count(*) FILTER (WHERE nl."PretUnitar" IS NULL) nir_fara_pret
FROM fl JOIN nl ON nl."LotId"=fl."LotId";

\echo == B5.7 linii FCT de stoc fara linie NIR pe acelasi lot ==
WITH fl AS (SELECT dd."ID", dd."LotId" FROM "DocumentDetalii" dd JOIN "Documente" f ON f."ID"=dd."DocumentId"
            WHERE f."ClrType"='FacturaIntrare' AND dd."LotId" IS NOT NULL)
SELECT count(*) linii_fct_cu_lot,
       count(*) FILTER (WHERE EXISTS (SELECT 1 FROM "DocumentDetalii" dn JOIN "Documente" n ON n."ID"=dn."DocumentId"
                                      WHERE n."ClrType"='NIR' AND dn."LotId"=fl."LotId")) cu_linie_nir,
       count(*) FILTER (WHERE NOT EXISTS (SELECT 1 FROM "DocumentDetalii" dn JOIN "Documente" n ON n."ID"=dn."DocumentId"
                                      WHERE n."ClrType"='NIR' AND dn."LotId"=fl."LotId")) fara_linie_nir
FROM fl;

\echo == B5.8 DVI: legatura n->m cu facturile ==
SELECT 'DviFacturi randuri' k, count(*)::text v FROM "DviFacturi"
UNION ALL SELECT 'DVI distincte', count(DISTINCT "DviId")::text FROM "DviFacturi"
UNION ALL SELECT 'Facturi distincte', count(DISTINCT "FacturaId")::text FROM "DviFacturi";
