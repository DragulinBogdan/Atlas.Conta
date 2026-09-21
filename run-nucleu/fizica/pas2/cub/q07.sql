-- Q07 (cub) Jurnal cumparari 2025-06: Spatiu=3, PerioadaDeclarare=202506,
-- CodTva.Sens=1; per (DocumentId, TranzactieId, CodTvaId), Baza/Taxa = rolul.
-- Din document se citesc DOAR atribute (Numar, Data, ClrType), niciodata cifre.
SELECT g."DocumentId", d."Numar" AS "DocumentNumar", d."Data" AS "DocumentData",
       d."ClrType" AS "DocumentTip", g."TranzactieId", g."CodTvaId",
       ct."Regim", ct."Cota", t."Cod" AS "TipTvaCod", t."CodSafTAchizitie" AS "CodSafT",
       g."Baza", g."Taxa"
FROM (
  SELECT p."DocumentId", p."TranzactieId", p."CodTvaId",
         COALESCE(sum(CASE WHEN p."RolTva" = 1 THEN p."Valoare" ELSE 0.0 END), 0.0) AS "Baza",
         COALESCE(sum(CASE WHEN p."RolTva" = 2 THEN p."Valoare" ELSE 0.0 END), 0.0) AS "Taxa"
  FROM {T} p
  WHERE p."Spatiu" = 3 AND p."PerioadaDeclarare" = 202506
    AND p."CodTvaId" IN (SELECT k."ID" FROM cub."CodTva" k WHERE k."Sens" = 1)
  GROUP BY p."DocumentId", p."TranzactieId", p."CodTvaId") g
JOIN cub."CodTva" ct ON ct."ID" = g."CodTvaId"
LEFT JOIN (SELECT tt."ID", tt."Cod", tt."CodSafTAchizitie" FROM "TipuriTva" tt WHERE tt."GCRecord" = 0) t ON t."ID" = ct."TipTvaId"
LEFT JOIN (SELECT dd."ID", dd."Numar", dd."Data", dd."ClrType" FROM "Documente" dd WHERE dd."GCRecord" = 0) d ON d."ID" = g."DocumentId"
