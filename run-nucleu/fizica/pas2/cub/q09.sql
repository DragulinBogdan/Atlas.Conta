-- Q09 (cub) FIFO: loturile cu sold > 0 in gestiunea TRANZIT la 2025-06-30,
-- IN ORDINEA FIFO, in SQL (azi ordinea e in C#, dupa un GetObjectByKey<Lot> per lot).
-- Lipseste TipStoc: nu e coordonata a cubului in modelul de masurare (se declara).
SELECT p."Unitate" AS "LotId",
       COALESCE(sum(p."Cantitate"), 0.0) AS "Sold",
       COALESCE(sum(p."Valoare"), 0.0) AS "Valoare",
       min(p."Data") AS "PrimaMiscare"
FROM {T} p
WHERE p."Spatiu" = 2 AND p."Gestiune" = '01a0b48c-d7ca-73c5-88b4-c0b508a78342' AND p."Data" <= DATE '2025-06-30'
GROUP BY p."Unitate"
HAVING COALESCE(sum(p."Cantitate"), 0.0) > 0.0
ORDER BY min(p."Data"), p."Unitate"
