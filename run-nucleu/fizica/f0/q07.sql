-- Q07 Jurnalul de CUMPARARI 2025-06 (per document x cota)
-- Oglindeste: TvaProiectii.JurnalTva(sens: Achizitie), Proiectii/TvaProiectii.cs:172-283
--   cheia: (DocumentId, TipTvaId, PartenerId, Regim, Cota, Storno, PerioadaAn, PerioadaLuna) -- :211-215
--   Data = MIN(RegistruTva.Data) -- :225 (data FIZICA a documentului, nu DataInregistrare)
--   toate cele trei join-uri de eticheta sunt LEFT -- :235-250
--   CodSafT e DIRECTIONAL, ales pe `sens`, la CITIRE -- :230-233,278
-- oglindit de mana; tiparul GROUP BY + LEFT JOIN e confirmat de capturile EF din run-f28/pas3.
-- Filtre copiate din cod:
--   Sens = 1 (Achizitie), OBLIGATORIU (refuz 400 in TvaControllere.cs:41-45)
--   perioada: PerioadaAn*100 + PerioadaLuna intre 202506 si 202506 -- TvaProiectii.IntreLuni :145-157
--     NU pe Data si NU pe DataInregistrare; filtrul cade pe RANDURI, inaintea gruparii (:175-176)
--   Storno NU se filtreaza, dar E in cheie (operarea si stornarea = doua fapte fiscale)
--   Stare a documentului nu se filtreaza (registrul contine doar fapte operate)
-- ales: luna 2025-06 (ceruta de spec); sensul Achizitie = 1.
-- Nota 89: `(p as Partener).CodFiscal` NU filtreaza pe tip sub TPH -- coloana se citeste direct
--   de pe randul din "Repartitori", oricare ar fi ClrType-ul lui.
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
LEFT JOIN (SELECT tt."ID", tt."Cod", tt."Denumire", tt."CodSafTAchizitie" FROM "TipuriTva" tt WHERE tt."GCRecord" = 0) AS t ON a."TipTvaId" = t."ID"
