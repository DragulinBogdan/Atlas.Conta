BEGIN READ ONLY;
WITH conexe AS (
    SELECT n."ID", n."DocumentSursaId" AS sursa, n."Autogenerat", n."Stare",
           n."PredatorId" IS DISTINCT FROM f."PredatorId"
               OR n."PrimitorId" IS DISTINCT FROM f."PrimitorId" AS laturi_diferite
    FROM "Documente" n
    JOIN "Documente" f ON f."ID" = n."DocumentSursaId" AND f."GCRecord" = 0
    WHERE n."GCRecord" = 0 AND n."ClrType" = 'NIR' AND f."ClrType" = 'FacturaIntrare'
), forme AS (
    SELECT n."ID", 'sursa' AS rol, l."LotId", l."TipMaterialId", l."Cantitate", l."Valoare", count(*) AS cate
    FROM conexe n JOIN "DocumentDetalii" l ON l."DocumentId" = n.sursa
    WHERE l."GCRecord" = 0 AND l."LotId" IS NOT NULL
    GROUP BY 1,2,3,4,5,6
    UNION ALL
    SELECT n."ID", 'conex', l."LotId", l."TipMaterialId", l."Cantitate", l."Valoare", count(*)
    FROM conexe n JOIN "DocumentDetalii" l ON l."DocumentId" = n."ID"
    WHERE l."GCRecord" = 0
    GROUP BY 1,2,3,4,5,6
), diferente AS (
    SELECT "ID", "LotId", "TipMaterialId", "Cantitate", "Valoare",
           sum(CASE WHEN rol = 'conex' THEN cate ELSE -cate END) AS delta_linii
    FROM forme GROUP BY 1,2,3,4,5 HAVING sum(CASE WHEN rol = 'conex' THEN cate ELSE -cate END) <> 0
), totaluri AS (
    SELECT "ID", sum(CASE WHEN rol = 'conex' THEN "Cantitate" * cate ELSE -"Cantitate" * cate END) AS dq,
           sum(CASE WHEN rol = 'conex' THEN "Valoare" * cate ELSE -"Valoare" * cate END) AS dv
    FROM forme GROUP BY 1
)
SELECT count(*) AS conexe_fct,
       count(*) FILTER (WHERE n."Autogenerat") AS autogenerate,
       count(*) FILTER (WHERE n.laturi_diferite) AS laturi_diferite,
       count(*) FILTER (WHERE EXISTS (SELECT 1 FROM diferente d WHERE d."ID" = n."ID")) AS multiset_diferit,
       count(*) FILTER (WHERE t.dq <> 0) AS total_cantitate_diferit,
       count(*) FILTER (WHERE t.dv <> 0) AS total_valoare_diferit,
       count(*) FILTER (WHERE t.dq = 0 AND t.dv = 0
           AND EXISTS (SELECT 1 FROM diferente d WHERE d."ID" = n."ID")) AS diferente_ascunse_de_total,
       coalesce(sum(t.dv), 0) AS delta_valoare_totala
FROM conexe n LEFT JOIN totaluri t ON t."ID" = n."ID";

SELECT n."Stare", count(*) AS documente,
       count(*) FILTER (WHERE n."DocumentSursaId" IS NULL) AS fara_sursa,
       count(*) FILTER (WHERE n."CorecteazaId" IS NOT NULL) AS corectii,
       count(*) FILTER (WHERE f."ID" IS NULL AND n."DocumentSursaId" IS NOT NULL) AS sursa_absenta,
       count(*) FILTER (WHERE f."ID" IS NOT NULL AND f."ClrType" <> 'FacturaIntrare') AS alta_sursa
FROM "Documente" n LEFT JOIN "Documente" f ON f."ID" = n."DocumentSursaId" AND f."GCRecord" = 0
WHERE n."GCRecord" = 0 AND n."ClrType" = 'NIR'
GROUP BY 1 ORDER BY 1;
ROLLBACK;
