-- B5: FCT contra NIR.
\echo == B5.0 politica de conex (seed) ==
SELECT s."Cod" sursa, t."Cod" tinta, pc."InverseazaLaturi", pc."NaturaFiltru", pc."DinSeed"
FROM "PoliticiConex" pc JOIN "TipuriDocument" s ON s."ID"=pc."TipDocumentSursaId"
JOIN "TipuriDocument" t ON t."ID"=pc."TipDocumentTintaId" ORDER BY 1;

\echo == B5.1 LinieSursaId: pe ce tipuri de linie exista si spre ce ==
SELECT dd."ClrType" linie, src."ClrType" linie_sursa, count(*) n
FROM "DocumentDetalii" dd LEFT JOIN "DocumentDetalii" src ON src."ID"=dd."LinieSursaId"
WHERE dd."LinieSursaId" IS NOT NULL GROUP BY 1,2 ORDER BY 3 DESC;

\echo == B5.2 legatura FCT <-> NIR la nivel de DOCUMENT (DocumentSursaId) ==
SELECT 'FCT total' k, count(*) v FROM "Documente" WHERE "ClrType"='FacturaIntrare'
UNION ALL SELECT 'FCT operate/stornate', count(*) FROM "Documente" WHERE "ClrType"='FacturaIntrare' AND "Stare"<>0
UNION ALL SELECT 'NIR total', count(*) FROM "Documente" WHERE "ClrType"='NotaIntrareReceptie'
UNION ALL SELECT 'NIR cu DocumentSursa FCT', count(*) FROM "Documente" n JOIN "Documente" f ON f."ID"=n."DocumentSursaId"
   WHERE n."ClrType"='NotaIntrareReceptie' AND f."ClrType"='FacturaIntrare'
UNION ALL SELECT 'NIR fara DocumentSursa (manual)', count(*) FROM "Documente" n
   WHERE n."ClrType"='NotaIntrareReceptie' AND n."DocumentSursaId" IS NULL
UNION ALL SELECT 'FCT fara niciun NIR conex', count(*) FROM "Documente" f
   WHERE f."ClrType"='FacturaIntrare' AND NOT EXISTS (SELECT 1 FROM "Documente" n WHERE n."DocumentSursaId"=f."ID" AND n."ClrType"='NotaIntrareReceptie');

\echo == B5.2b ClrType-urile reale ale documentelor (ancora numelor) ==
SELECT "ClrType", count(*) FROM "Documente" GROUP BY 1 ORDER BY 2 DESC;
