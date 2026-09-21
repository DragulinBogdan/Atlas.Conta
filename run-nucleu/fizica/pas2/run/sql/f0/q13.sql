EXPLAIN (ANALYZE, BUFFERS)
SELECT r."ID", r."Data", r."TipStoc", r."LotId", r."RepartitorId", r."Cantitate", r."Valoare",
       r."Storno", r."DocumentId", r."DetaliuId", r."GCRecord", r."OptimisticLockField"
FROM "RegistruStoc" AS r
WHERE r."GCRecord" = 0 AND r."DocumentId" = '01a0b494-a2da-7484-94ce-1019f56c4fef';
EXPLAIN (ANALYZE, BUFFERS)
SELECT r.*
FROM "RegistruContabil" AS r
WHERE r."GCRecord" = 0 AND r."DocumentId" = '01a0b494-a2da-7484-94ce-1019f56c4fef';
EXPLAIN (ANALYZE, BUFFERS)
SELECT r.*
FROM "RegistruTva" AS r
WHERE r."GCRecord" = 0 AND r."DocumentId" = '01a0b494-a2da-7484-94ce-1019f56c4fef';
