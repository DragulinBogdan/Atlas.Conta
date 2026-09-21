EXPLAIN (ANALYZE, BUFFERS)
SELECT r."ID", r."DocumentId", r."DetaliuId", r."Sens", r."TipTvaId", r."Regim", r."Cota",
       r."Baza", r."Tva", r."Storno"
FROM "RegistruTva" AS r
WHERE r."GCRecord" = 0
  AND r."PerioadaAn" * 100 + r."PerioadaLuna" >= 202506
  AND r."PerioadaAn" * 100 + r."PerioadaLuna" <= 202506;
