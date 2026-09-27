BEGIN READ ONLY;

SELECT "Stare", count(*) AS documente,
       count(*) FILTER (WHERE "CorecteazaId" IS NOT NULL) AS corectii,
       count(*) FILTER (WHERE "Data" <> "DataInregistrare") AS intarziate
FROM "Documente"
WHERE "ClrType" = 'BonConsum' AND "GCRecord" = 0
GROUP BY "Stare" ORDER BY "Stare";

WITH linii AS (
    SELECT d."ID", count(l."ID") AS n, count(DISTINCT l."LotId") AS loturi
    FROM "Documente" d LEFT JOIN "DocumentDetalii" l
      ON l."DocumentId" = d."ID" AND l."GCRecord" = 0
    WHERE d."ClrType" = 'BonConsum' AND d."GCRecord" = 0
    GROUP BY d."ID"
)
SELECT count(*) FILTER (WHERE n = 1) AS o_linie,
       count(*) FILTER (WHERE n > 1) AS mai_multe,
       max(n) AS maxim_linii,
       count(*) FILTER (WHERE n > loturi) AS lot_repetat
FROM linii;

SELECT count(*) AS linii,
       count(*) FILTER (WHERE l."Cantitate" = 0) AS zero,
       count(*) FILTER (WHERE l."Cantitate" < 0) AS negative,
       count(*) FILTER (WHERE l."LotId" IS NULL) AS fara_lot,
       count(*) FILTER (WHERE l."Cantitate" <> trunc(l."Cantitate")) AS fractionare,
       count(DISTINCT l."TipMaterialId") AS tipuri_material,
       count(DISTINCT d."PredatorId") AS gestiuni,
       count(DISTINCT d."PrimitorId") AS locuri_consum
FROM "Documente" d JOIN "DocumentDetalii" l ON l."DocumentId" = d."ID"
WHERE d."ClrType" = 'BonConsum' AND d."GCRecord" = 0 AND l."GCRecord" = 0;

ROLLBACK;
