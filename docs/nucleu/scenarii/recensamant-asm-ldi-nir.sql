BEGIN READ ONLY;
WITH linii AS (
    SELECT d."ID" AS document, d."ClrType", d."Autogenerat",
           d."Data" <> d."DataInregistrare" AS intarziat,
           l."ID" AS linie, l."Cantitate", l."Valoare", lot."ProdusId",
           t."ContImplicitId" AS cont
    FROM "Documente" d
    LEFT JOIN "DocumentDetalii" l ON l."DocumentId" = d."ID" AND l."GCRecord" = 0
    LEFT JOIN "Loturi" lot ON lot."ID" = l."LotId"
    LEFT JOIN "Produse" p ON p."ID" = lot."ProdusId"
    LEFT JOIN "TipuriMaterial" t ON t."ID" = p."TipMaterialId"
    WHERE d."GCRecord" = 0 AND d."ClrType" IN ('Asamblare','ListaDiferenteInventar','NIR')
), documente AS (
    SELECT document, "ClrType", "Autogenerat", intarziat, count(linie) AS linii,
           count(*) FILTER (WHERE "Cantitate" < 0) AS consumuri,
           count(*) FILTER (WHERE "Cantitate" > 0) AS produse,
           count(DISTINCT cont) AS conturi, count(DISTINCT "ProdusId") AS produse_distincte,
           sum("Cantitate") AS q, sum("Valoare") AS v
    FROM linii GROUP BY 1,2,3,4
)
SELECT "ClrType", count(*) AS documente, sum(linii) AS linii,
       count(*) FILTER (WHERE "Autogenerat") AS autogenerate,
       count(*) FILTER (WHERE intarziat) AS intarziate,
       count(*) FILTER (WHERE consumuri > 0 AND produse > 0) AS bidirectionale,
       count(*) FILTER (WHERE consumuri > 1) AS consumuri_multiple,
       count(*) FILTER (WHERE produse > 1) AS produse_multiple,
       count(*) FILTER (WHERE conturi > 1) AS conturi_multiple,
       count(*) FILTER (WHERE produse_distincte > 1) AS produse_distincte_multiple,
       count(*) FILTER (WHERE q <> 0) AS suma_cantitate_nenula,
       count(*) FILTER (WHERE v <> 0) AS suma_valoare_nenula
FROM documente GROUP BY 1 ORDER BY 1;

WITH produse AS (
    SELECT d."ID", lot."ProdusId", sum(l."Cantitate") AS q
    FROM "Documente" d JOIN "DocumentDetalii" l ON l."DocumentId" = d."ID" AND l."GCRecord" = 0
    JOIN "Loturi" lot ON lot."ID" = l."LotId"
    WHERE d."GCRecord" = 0 AND d."ClrType" = 'Asamblare'
    GROUP BY 1,2
)
SELECT count(DISTINCT "ID") AS asm_cu_suma_nenula_pe_produs,
       count(*) FILTER (WHERE q <> 0) AS pozitii_produs_neconservate
FROM produse WHERE q <> 0;

SELECT d."ClrType", l."Directie", sign(l."Cantitate") AS semn,
       c."Simbol", count(*) AS linii
FROM "Documente" d JOIN "DocumentDetalii" l ON l."DocumentId" = d."ID" AND l."GCRecord" = 0
LEFT JOIN "Loturi" lot ON lot."ID" = l."LotId"
LEFT JOIN "Produse" p ON p."ID" = lot."ProdusId"
LEFT JOIN "TipuriMaterial" t ON t."ID" = p."TipMaterialId"
LEFT JOIN "Conturi" c ON c."ID" = t."ContImplicitId"
WHERE d."GCRecord" = 0 AND d."ClrType" IN ('Asamblare','ListaDiferenteInventar','NIR')
GROUP BY 1,2,3,4 ORDER BY 1,2,3,4;
ROLLBACK;

