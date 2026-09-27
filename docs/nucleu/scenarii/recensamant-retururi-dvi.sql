BEGIN READ ONLY;
WITH documente AS (
    SELECT d."ID", d."ClrType", d."Data" <> d."DataInregistrare" AS intarziat,
           d."CorecteazaId" IS NOT NULL AS corectie, count(l."ID") AS linii,
           count(l."ID") FILTER (WHERE l."LotId" IS NOT NULL) AS cost,
           count(l."ID") FILTER (WHERE l."LotId" IS NULL) AS fara_lot,
           count(DISTINCT l."TipTvaId") AS cote
    FROM "Documente" d LEFT JOIN "DocumentDetalii" l
      ON l."DocumentId" = d."ID" AND l."GCRecord" = 0
    WHERE d."GCRecord" = 0 AND d."ClrType" IN ('ReturClient','ReturFurnizor','Dvi')
    GROUP BY d."ID"
)
SELECT "ClrType", count(*) AS documente, sum(linii) AS linii,
       count(*) FILTER (WHERE linii > 1) AS multiple, max(linii) AS maxim_linii,
       sum(cost) AS cu_lot, sum(fara_lot) AS fara_lot,
       count(*) FILTER (WHERE cost > 0 AND fara_lot > 0) AS mixte,
       count(*) FILTER (WHERE cote > 1) AS multicota,
       count(*) FILTER (WHERE intarziat) AS intarziate,
       count(*) FILTER (WHERE corectie) AS corectii
FROM documente GROUP BY "ClrType" ORDER BY 1;

SELECT d."ClrType", l."LotId" IS NOT NULL AS cu_lot, t."Cod" AS tva,
       count(*) AS linii, count(*) FILTER (WHERE l."Cantitate" < 0) AS q_negativ,
       count(*) FILTER (WHERE l."Valoare" < 0) AS v_negativ,
       count(*) FILTER (WHERE l."Valoare" = 0) AS v_zero,
       count(*) FILTER (WHERE lot."LinieIntrareId" = l."ID") AS lot_propriu
FROM "Documente" d JOIN "DocumentDetalii" l ON l."DocumentId" = d."ID" AND l."GCRecord" = 0
LEFT JOIN "TipuriTva" t ON t."ID" = l."TipTvaId"
LEFT JOIN "Loturi" lot ON lot."ID" = l."LotId"
WHERE d."GCRecord" = 0 AND d."ClrType" IN ('ReturClient','ReturFurnizor','Dvi')
GROUP BY 1,2,3 ORDER BY 1,2,3;
ROLLBACK;
