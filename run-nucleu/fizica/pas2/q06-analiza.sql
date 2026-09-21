\set ON_ERROR_STOP on
WITH f0 AS (
  SELECT p."DocumentId" AS doc, round(p."Rest",2) AS rest
  FROM "PartideDeschise" p JOIN "Documente" d ON d."ID"=p."DocumentId"
  WHERE p."GCRecord"=0 AND p."An"=2025 AND p."Luna"=12 AND p."Rest" <> 0
    AND d."ClrType" IN ('FacturaIntrare','FacturaIesire','Plata','Incasare','Decont','ReturClient')),
f2 AS (
  SELECT g."Unitate" AS doc, round(abs(g."Sold"),2) AS rest FROM (
    SELECT p."Unitate", sum(CASE WHEN p."Latura"=1 THEN p."Valoare" ELSE -p."Valoare" END) AS "Sold"
    FROM f2."Postare" p JOIN "Conturi" c ON c."ID"=p."Cont" AND c."RolTert" <> 0
    JOIN "Documente" d ON d."ID"=p."Unitate"
    WHERE p."Spatiu"=1 AND p."Data" <= DATE '2025-12-31' AND p."Unitate" IS NOT NULL
      AND d."ClrType" IN ('FacturaIntrare','FacturaIesire','Plata','Incasare','Decont','ReturClient')
    GROUP BY 1) g WHERE g."Sold" <> 0)
SELECT d."ClrType",
       count(*) FILTER (WHERE f0.rest IS NOT NULL AND f2.rest IS NOT NULL AND f0.rest = f2.rest) AS coincid,
       count(*) FILTER (WHERE f0.rest IS NOT NULL AND f2.rest IS NOT NULL AND f0.rest <> f2.rest) AS difera,
       count(*) FILTER (WHERE f2.rest IS NULL) AS doar_f0,
       count(*) FILTER (WHERE f0.rest IS NULL) AS doar_f2
FROM f0 FULL JOIN f2 ON f2.doc = f0.doc
JOIN "Documente" d ON d."ID" = COALESCE(f0.doc, f2.doc)
GROUP BY 1 ORDER BY 1;

WITH f0 AS (
  SELECT p."DocumentId" AS doc, round(p."Rest",2) AS rest
  FROM "PartideDeschise" p JOIN "Documente" d ON d."ID"=p."DocumentId"
  WHERE p."GCRecord"=0 AND p."An"=2025 AND p."Luna"=12 AND p."Rest" <> 0
    AND d."ClrType" IN ('FacturaIntrare','FacturaIesire','Plata','Incasare','Decont','ReturClient')),
f2 AS (
  SELECT g."Unitate" AS doc, round(abs(g."Sold"),2) AS rest FROM (
    SELECT p."Unitate", sum(CASE WHEN p."Latura"=1 THEN p."Valoare" ELSE -p."Valoare" END) AS "Sold"
    FROM f2."Postare" p JOIN "Conturi" c ON c."ID"=p."Cont" AND c."RolTert" <> 0
    JOIN "Documente" d ON d."ID"=p."Unitate"
    WHERE p."Spatiu"=1 AND p."Data" <= DATE '2025-12-31' AND p."Unitate" IS NOT NULL
      AND d."ClrType" IN ('FacturaIntrare','FacturaIesire','Plata','Incasare','Decont','ReturClient')
    GROUP BY 1) g WHERE g."Sold" <> 0)
SELECT d."ClrType", COALESCE(f0.doc,f2.doc)::text AS doc, d."Numar",
       f0.rest AS rest_f0, f2.rest AS rest_f2, d."TotalStingere"
FROM f0 FULL JOIN f2 ON f2.doc = f0.doc
JOIN "Documente" d ON d."ID" = COALESCE(f0.doc, f2.doc)
WHERE f0.rest IS DISTINCT FROM f2.rest
ORDER BY d."ClrType", 2 LIMIT 5;
