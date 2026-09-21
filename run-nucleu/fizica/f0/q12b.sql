-- Q12b (F0) A doua interogare a sectiunii SAF-T GLE: faptele fiscale ale perioadei de
-- declarare (TaxInformation), materializate integral -- SaftProiectii.cs:353-358.
-- Textul e cel citat in comentariul lui f0/q12.sql, scos in fisier propriu ca sa aiba
-- cifra proprie la pasul 2 (perechea lui cub/q12b.sql).
-- Filtre copiate din cod: GCRecord = 0; PerioadaAn*100 + PerioadaLuna intre 202506 si 202506
--   (predicat ARITMETIC -- de aceea IX_RegistruTva_PerioadaAn_PerioadaLuna ramane nefolosit,
--   surpriza 4 din 01-interogari-azi.md).
SELECT r."ID", r."DocumentId", r."DetaliuId", r."Sens", r."TipTvaId", r."Regim", r."Cota",
       r."Baza", r."Tva", r."Storno"
FROM "RegistruTva" AS r
WHERE r."GCRecord" = 0
  AND r."PerioadaAn" * 100 + r."PerioadaLuna" >= 202506
  AND r."PerioadaAn" * 100 + r."PerioadaLuna" <= 202506
