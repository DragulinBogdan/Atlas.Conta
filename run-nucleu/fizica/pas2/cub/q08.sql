-- Q08 (cub) D394 2025-06, doar agregatul (restul e randare): per
-- (DocumentId, TranzactieId, Partener, CodTvaId), Suma Baza si Suma Taxa.
SELECT p."DocumentId", p."TranzactieId", p."Partener", p."CodTvaId",
       count(*) AS "Randuri",
       COALESCE(sum(CASE WHEN p."RolTva" = 1 THEN p."Valoare" ELSE 0.0 END), 0.0) AS "Baza",
       COALESCE(sum(CASE WHEN p."RolTva" = 2 THEN p."Valoare" ELSE 0.0 END), 0.0) AS "Taxa"
FROM {T} p
WHERE p."Spatiu" = 3 AND p."PerioadaDeclarare" = 202506
GROUP BY p."DocumentId", p."TranzactieId", p."Partener", p."CodTvaId"
