EXPLAIN (ANALYZE, BUFFERS)
SELECT m."LotId", COALESCE(SUM(m."Cantitate"), 0.0) AS "Sold"
FROM (
    SELECT r."LotId", r."RepartitorId", r."TipStoc", r."Cantitate"
    FROM "RegistruStoc" AS r
    INNER JOIN (SELECT l."ID", l."ProdusId" FROM "Loturi" AS l WHERE l."GCRecord" = 0) AS l0 ON r."LotId" = l0."ID"
    WHERE r."GCRecord" = 0 AND r."Data" <= '2025-06-30'
      AND l0."ProdusId" = '01a0b48d-78b6-754c-92e9-c3250a37995b'
) AS m
WHERE m."RepartitorId" = '01a0b48c-d7ca-73c5-88b4-c0b508a78342' AND m."TipStoc" = 1
GROUP BY m."LotId"
HAVING COALESCE(SUM(m."Cantitate"), 0.0) > 0.0;
