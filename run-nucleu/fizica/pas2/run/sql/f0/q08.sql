EXPLAIN (ANALYZE, BUFFERS)
SELECT r."DocumentId", r."Storno", r."PartenerId", r."Sens", r."TipTvaId", r."Cota",
       COUNT(*) AS "Randuri",
       COALESCE(SUM(r."Baza"), 0.0) AS "Baza",
       COALESCE(SUM(r."Tva"), 0.0) AS "Tva"
FROM "RegistruTva" r
WHERE r."GCRecord" = 0
  AND r."PerioadaAn" * 100 + r."PerioadaLuna" >= 202506
  AND r."PerioadaAn" * 100 + r."PerioadaLuna" <= 202506
GROUP BY r."DocumentId", r."Storno", r."PartenerId", r."Sens", r."TipTvaId", r."Cota";
