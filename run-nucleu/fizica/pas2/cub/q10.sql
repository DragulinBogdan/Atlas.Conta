-- Q10 (cub) SAF-T PhysicalStock la 2025-12-31: per (Gestiune, Unitate=lot).
-- f0/q10.sql grupeaza pe (Repartitor, Lot, TipStoc) si reduce peste TipStoc in memorie;
-- pe cub reducerea e deja facuta de gran -- egalitatea se verifica pe acelasi gran.
SELECT p."Gestiune", p."Unitate" AS "LotId",
       COALESCE(sum(p."Cantitate"), 0.0) AS "Cantitate",
       COALESCE(sum(p."Valoare"), 0.0) AS "Valoare"
FROM {T} p
WHERE p."Spatiu" = 2 AND p."Data" <= DATE '2025-12-31'
GROUP BY p."Gestiune", p."Unitate"
HAVING COALESCE(sum(p."Cantitate"), 0.0) <> 0.0
