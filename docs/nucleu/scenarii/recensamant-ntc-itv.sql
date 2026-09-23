BEGIN READ ONLY;
WITH documente AS (
    SELECT d."ID", d."ClrType", d."Data" <> d."DataInregistrare" AS intarziat,
           count(l."ID") AS linii,
           count(*) FILTER (WHERE l."Valoare" < 0) AS negative,
           count(*) FILTER (WHERE l."ContDebitId" = l."ContCreditId") AS acelasi_cont,
           count(*) FILTER (WHERE l."RepartitorDebitId" IS NOT NULL OR l."RepartitorCreditId" IS NOT NULL) AS repartitor_explicit
    FROM "Documente" d LEFT JOIN "DocumentDetalii" l
      ON l."DocumentId" = d."ID" AND l."GCRecord" = 0
    WHERE d."GCRecord" = 0 AND d."ClrType" IN ('NotaContabila','InchidereTva')
    GROUP BY d."ID"
)
SELECT "ClrType", count(*) AS documente, sum(linii) AS linii,
       count(*) FILTER (WHERE linii > 1) AS multiple, max(linii) AS maxim_linii,
       sum(negative) AS linii_negative, sum(acelasi_cont) AS acelasi_cont,
       sum(repartitor_explicit) AS repartitor_explicit,
       count(*) FILTER (WHERE intarziat) AS intarziate
FROM documente GROUP BY "ClrType" ORDER BY "ClrType";

WITH picioare AS (
    SELECT d."ID" AS document, d."ClrType", c."Simbol", c."RolTert", r."ClrType" AS fel_repartitor,
           r."ID" AS partener, l."LotId", l."Valoare", c."ID" AS cont
    FROM "Documente" d JOIN "DocumentDetalii" l ON l."DocumentId" = d."ID" AND l."GCRecord" = 0
    CROSS JOIN LATERAL (VALUES (l."ContDebitId", l."RepartitorDebitId"),
                              (l."ContCreditId", l."RepartitorCreditId")) AS x(cont, repartitor)
    JOIN "Conturi" c ON c."ID" = x.cont
    LEFT JOIN "Repartitori" r ON r."ID" = x.repartitor
    WHERE d."GCRecord" = 0 AND d."ClrType" IN ('NotaContabila','InchidereTva')
)
SELECT "ClrType", "Simbol", "RolTert", fel_repartitor, count(*) AS picioare, sum("Valoare") AS valoare
FROM picioare
WHERE "RolTert" <> 0 OR ("Simbol" LIKE '3%' AND "LotId" IS NULL)
GROUP BY "ClrType", "Simbol", "RolTert", fel_repartitor ORDER BY 1, 2, 4;

WITH picioare AS (
    SELECT d."ID" AS document, x.cont, x.repartitor
    FROM "Documente" d JOIN "DocumentDetalii" l ON l."DocumentId" = d."ID" AND l."GCRecord" = 0
    CROSS JOIN LATERAL (VALUES (l."ContDebitId", l."RepartitorDebitId"),
                              (l."ContCreditId", l."RepartitorCreditId")) AS x(cont, repartitor)
    JOIN "Repartitori" r ON r."ID" = x.repartitor AND r."ClrType" IN ('Partener','Angajat')
    JOIN "Conturi" c ON c."ID" = x.cont AND c."RolTert" <> 0
    WHERE d."GCRecord" = 0 AND d."ClrType" = 'NotaContabila'
)
SELECT count(*) AS document_cont_cu_parteneri_multipli, max(parteneri) AS maxim_parteneri
FROM (SELECT document, cont, count(DISTINCT repartitor) AS parteneri FROM picioare
      GROUP BY document, cont HAVING count(DISTINCT repartitor) > 1) x;
ROLLBACK;
