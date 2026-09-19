-- B3b. Reconcilierea cu proba M5 a review-ului (1.200 contra 1.179) si
-- cazurile in care AMBELE documente stau pe aceeasi latura.
\set ON_ERROR_STOP on
\pset footer off

\echo ===== B3b.1 proba M5 refacuta pe cub."_TertDoc" (per PICIOR) =====
SELECT count(*) AS total,
       count(*) FILTER (WHERE s."Cont" IS DISTINCT FROM f."Cont")   AS cont_diferit,
       count(*) FILTER (WHERE f."DocumentId" IS NULL)               AS fara_tert_pe_stins,
       count(*) FILTER (WHERE s."Cont" IS DISTINCT FROM f."Cont"
                          AND f."DocumentId" IS NOT NULL)           AS cont_diferit_real
FROM "Imperecheri" i
LEFT JOIN cub."_TertDoc" s ON s."DocumentId" = i."DocumentStingatorId"
LEFT JOIN cub."_TertDoc" f ON f."DocumentId" = i."DocumentId";

\echo
\echo ===== B3b.2 cele care ies diferite pe PICIOR dar nu pe CONT-agregat =====
SELECT ds."ClrType" AS tip_stins, dg."ClrType" AS tip_stingator,
       cf."Cont" <> cs."Cont" AS dif_pe_picior,
       tf."Cont" <> ts."Cont" AS dif_pe_cont_agregat,
       count(*) AS numar, sum(i."Suma") AS suma
FROM "Imperecheri" i
JOIN cub."_TertDoc" cs ON cs."DocumentId" = i."DocumentStingatorId"
JOIN cub."_TertDoc" cf ON cf."DocumentId" = i."DocumentId"
JOIN tr."TertRef"   ts ON ts."DocumentId" = i."DocumentStingatorId"
JOIN tr."TertRef"   tf ON tf."DocumentId" = i."DocumentId"
JOIN tr."Doc" ds ON ds."ID" = i."DocumentId"
JOIN tr."Doc" dg ON dg."ID" = i."DocumentStingatorId"
WHERE cf."Cont" <> cs."Cont"
GROUP BY 1,2,3,4 ORDER BY numar DESC;

\echo
\echo ===== B3b.3 imperecherile cu AMBELE parti pe aceeasi latura =====
SELECT ds."ClrType" AS tip_stins, dg."ClrType" AS tip_stingator,
       f."ContSimbol" AS cont_stins, f."Latura" AS lat_stins,
       s."ContSimbol" AS cont_stingator, s."Latura" AS lat_sting,
       count(*) AS numar, sum(i."Suma") AS suma
FROM "Imperecheri" i
JOIN tr."TertRef" s ON s."DocumentId" = i."DocumentStingatorId"
JOIN tr."TertRef" f ON f."DocumentId" = i."DocumentId"
JOIN tr."Doc" ds ON ds."ID" = i."DocumentId"
JOIN tr."Doc" dg ON dg."ID" = i."DocumentStingatorId"
WHERE s."Latura" = f."Latura"
GROUP BY 1,2,3,4,5,6 ORDER BY numar DESC;

\echo
\echo ===== B3b.4 partener diferit intre stins si stingator: pe tip =====
SELECT ds."ClrType" AS tip_stins, dg."ClrType" AS tip_stingator,
       (f."DocumentId" IS NULL) AS stins_fara_tert,
       count(*) AS numar, sum(i."Suma") AS suma
FROM "Imperecheri" i
JOIN tr."TertRef" s ON s."DocumentId" = i."DocumentStingatorId"
LEFT JOIN tr."TertRef" f ON f."DocumentId" = i."DocumentId"
JOIN tr."Doc" ds ON ds."ID" = i."DocumentId"
JOIN tr."Doc" dg ON dg."ID" = i."DocumentStingatorId"
WHERE s."Partener" IS DISTINCT FROM f."Partener"
GROUP BY 1,2,3 ORDER BY numar DESC;

\echo
\echo ===== B3b.5 cele 1.085 de FCT stinse fara postare de tert: au NIR? =====
WITH fara AS (
  SELECT DISTINCT i."DocumentId" AS id FROM "Imperecheri" i
  LEFT JOIN tr."TertRef" f ON f."DocumentId" = i."DocumentId" WHERE f."DocumentId" IS NULL)
SELECT count(*) AS documente,
       count(*) FILTER (WHERE EXISTS (SELECT 1 FROM tr."FctNir" x WHERE x."FctId" = fara.id)) AS cu_nir,
       sum(c."TotalStingere") AS total_stingere,
       sum(COALESCE((SELECT sum(-n."NetDebitor") FROM tr."TertDocContNet" n
                     JOIN tr."FctNir" x ON x."NirId" = n."DocumentId"
                     WHERE x."FctId" = fara.id AND n."ContSimbol" = '401'), 0)) AS c401_pe_nir
FROM fara JOIN tr."Doc" c ON c."ID" = fara.id;
