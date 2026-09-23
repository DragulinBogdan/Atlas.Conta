BEGIN READ ONLY;
WITH documente AS (
    SELECT d."ID", d."ClrType", d."CorecteazaId", d."Data" <> d."DataInregistrare" AS intarziat,
           count(l."ID") AS linii, count(DISTINCT l."LotId") AS loturi,
           count(DISTINCT l."TipTvaId") AS cote,
           count(*) FILTER (WHERE l."Cantitate" < 0 OR l."Valoare" < 0) AS negative,
           count(*) FILTER (WHERE l."LotId" IS NOT NULL) AS cu_lot
    FROM "Documente" d LEFT JOIN "DocumentDetalii" l
      ON l."DocumentId" = d."ID" AND l."GCRecord" = 0
    WHERE d."GCRecord" = 0 AND d."ClrType" IN
      ('FacturaIntrare','Plata','Incasare','NotaTransfer','FacturaIesire','DescarcareGestiune')
    GROUP BY d."ID"
)
SELECT "ClrType", count(*) AS documente, sum(linii) AS linii,
       count(*) FILTER (WHERE linii > 1) AS multiple, max(linii) AS maxim_linii,
       count(*) FILTER (WHERE cote > 1) AS multicota,
       count(*) FILTER (WHERE cu_lot > loturi) AS lot_repetat,
       sum(negative) AS linii_negative,
       count(*) FILTER (WHERE "CorecteazaId" IS NOT NULL) AS corectii,
       count(*) FILTER (WHERE intarziat) AS intarziate
FROM documente GROUP BY "ClrType" ORDER BY "ClrType";
ROLLBACK;
