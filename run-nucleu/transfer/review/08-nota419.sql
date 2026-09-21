-- R7.3: notele care debiteaza 419 (regularizarea avansului) — pot numi o partida?
-- Rulare lenta (~3 min, EXISTS corelat); rezultatul citat in raport §M6.
\pset footer off
WITH nota419 AS (
  SELECT p."DocumentId", p."Partener", sum(p."Valoare") AS suma
  FROM tr."Picior" p
  WHERE p."ContSimbol"='419' AND p."Latura"=1 AND p."DocumentId" IS NOT NULL
    AND EXISTS (SELECT 1 FROM tr."Doc" d WHERE d."ID"=p."DocumentId" AND d."ClrType"='NotaContabila')
  GROUP BY 1,2)
SELECT count(*) AS note,
       count(*) FILTER (WHERE "Partener" IS NULL) AS fara_partener,
       round(sum(suma),2) AS total,
       count(*) FILTER (WHERE EXISTS (
          SELECT 1 FROM tr."TertLa1231" t
          JOIN tr."Picior" q ON q."DocumentId"=t."DocumentId" AND q."ContSimbol"='419'
          WHERE t."ContSimbol"='419' AND q."Partener" = nota419."Partener"
            AND abs(abs(t."Natural") - nota419.suma) < 0.005)) AS potrivire_exacta_pe_partener_si_suma
FROM nota419;
-- rezultat: 2068 | 1862 | 8252374.26 | 125
