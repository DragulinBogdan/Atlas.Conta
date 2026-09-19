EXPLAIN (ANALYZE, BUFFERS)
SELECT a."DocumentId", d."Numar" AS "DocumentNumar", NULL AS "DocumentTip",
       a."Data", a."PerioadaAn", a."PerioadaLuna",
       a."PartenerId", p."Denumire" AS "PartenerDenumire", p."CodFiscal" AS "PartenerCodFiscal",
       a."TipTvaId", t."Cod" AS "TipTvaCod", t."Denumire" AS "TipTvaDenumire",
       CASE WHEN a."Regim" = 1 THEN 'Normal'
            WHEN a."Regim" = 2 THEN 'Capitalizat'
            WHEN a."Regim" = 3 THEN 'TaxareInversa'
            WHEN a."Regim" = 4 THEN 'Scutit'
            ELSE 'Neimpozabil' END AS "Regim",
       a."Cota", t."CodSafTAchizitie" AS "CodSafT",
       a."Baza", a."Tva", a."Storno"
FROM (
    SELECT r."DocumentId", r."TipTvaId", r."PartenerId", r."Regim", r."Cota", r."Storno",
           r."PerioadaAn", r."PerioadaLuna",
           MIN(r."Data") AS "Data", COALESCE(SUM(r."Baza"), 0.0) AS "Baza", COALESCE(SUM(r."Tva"), 0.0) AS "Tva"
    FROM "RegistruTva" r
    WHERE r."GCRecord" = 0 AND r."Sens" = 1
      AND r."PerioadaAn" * 100 + r."PerioadaLuna" >= 202506
      AND r."PerioadaAn" * 100 + r."PerioadaLuna" <= 202506
    GROUP BY r."DocumentId", r."TipTvaId", r."PartenerId", r."Regim", r."Cota", r."Storno",
             r."PerioadaAn", r."PerioadaLuna"
) AS a
LEFT JOIN (SELECT dd."ID", dd."Numar" FROM "Documente" dd WHERE dd."GCRecord" = 0) AS d ON a."DocumentId" = d."ID"
LEFT JOIN (SELECT rp."ID", rp."Denumire", rp."CodFiscal" FROM "Repartitori" rp WHERE rp."GCRecord" = 0) AS p ON a."PartenerId" = p."ID"
LEFT JOIN (SELECT tt."ID", tt."Cod", tt."Denumire", tt."CodSafTAchizitie" FROM "TipuriTva" tt WHERE tt."GCRecord" = 0) AS t ON a."TipTvaId" = t."ID";
