-- Q05+S (cub) idem, direct din cub."Sold" la granita 2025-12-31 (= data ceruta).
SELECT g."Cont" AS "ContId", c0."Simbol" AS "ContSimbol", c0."Denumire" AS "ContDenumire",
       g."Partener" AS "RepartitorId", r0."Denumire" AS "RepartitorDenumire",
       g."Debit", g."Credit",
       CASE WHEN g."Debit" - g."Credit" > 0.0 THEN g."Debit" - g."Credit" ELSE 0.0 END AS "SoldDebitor",
       CASE WHEN g."Debit" - g."Credit" < 0.0 THEN -(g."Debit" - g."Credit") ELSE 0.0 END AS "SoldCreditor"
FROM (
  SELECT s."Cont", s."Partener",
         COALESCE(sum(CASE WHEN s."Latura" = 1 THEN s."Valoare" ELSE 0.0 END), 0.0) AS "Debit",
         COALESCE(sum(CASE WHEN s."Latura" = 2 THEN s."Valoare" ELSE 0.0 END), 0.0) AS "Credit"
  FROM cub."Sold" s
  WHERE s."Spatiu" = 1 AND s."Granita" = DATE '2025-12-31' AND s."Cont" = '01a0b48c-a8b2-73ce-a6cd-a045b80364a8'
  GROUP BY s."Cont", s."Partener") g
LEFT JOIN (SELECT c."ID", c."Denumire", c."Simbol" FROM "Conturi" c WHERE c."GCRecord" = 0) c0 ON g."Cont" = c0."ID"
LEFT JOIN (SELECT rp."ID", rp."Denumire" FROM "Repartitori" rp WHERE rp."GCRecord" = 0) r0 ON g."Partener" = r0."ID"
WHERE g."Debit" - g."Credit" <> 0.0
