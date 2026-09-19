-- Q03 Fisa contului 4111 pe 2025, cu sold cumulativ
-- Oglindeste: ContabilProiectii.FisaCont, Proiectii/ContabilProiectii.cs:764-960.
-- SQL REAL: singurul loc din repo cu SQL BRUT (ContabilProiectii.cs:688); textul de mai jos
--   e chiar cel construit la :812-921, cu placeholder-ele {n} inlocuite cu valorile literale.
-- Filtre, copiate din cod:
--   (a) r."GCRecord" = 0 SCRIS DE MANA pe registru si pe fiecare tabela alaturata
--       (:825,859,917,918,919) -- SqlQuery nu trece prin HasQueryFilter;
--   (a) r."ContDebitId"/"ContCreditId" = contId, r."Data" <= dataEnd (:825);
--   (c) f."Data" >= dataStart in exterior (:920) -- randurile anterioare contribuie la sold
--       fara sa se afiseze;
--   Storno NU filtreaza (:823 il proiecteaza, nimic nu-l taie);
--   fereastra: SUM(Debit-Credit) OVER (ORDER BY Data, Id, Sens DESC ROWS UNBOUNDED PRECEDING) (:908-911);
--   DocumentTip iese constant NULL si se completeaza in memorie peste pagina (:902, :1094-1102).
-- ales: contul 4111 = 01a0b48c-a8b2-73ce-a6cd-a045b80364a8 -- 108.906 randuri pe debit +
--   35.335 pe credit = 144.241 atomi, de departe primul (locul 2, 371, are 67.330+...).
-- FARA ramura de snapshot: referinta = Referinta(os, dataStart-1 = 2024-12-31) = null
--   (singura referinta a bazei e 2025-12, care se incheie la 2025-12-31) -- ContabilProiectii.cs:801.
-- Ordinea si LIMIT-ul NU sunt in acest text: le pune DataSourceLoader peste proiectie
--   prin OrdineFisa() (:1025-1029). Costul e independent de LIMIT (WindowAgg integral).
SELECT
    f."Id" AS "Id",
    f."Data" AS "Data",
    f."NumarNota" AS "NumarNota",
    f."Sens" AS "Sens",
    f."Debit" AS "Debit",
    f."Credit" AS "Credit",
    f."SoldCurent" AS "SoldCurent",
    f."ContrapartidaId" AS "ContrapartidaId",
    cp."Simbol" AS "ContrapartidaSimbol",
    rep."Denumire" AS "RepartitorDenumire",
    f."DocumentId" AS "DocumentId",
    CAST(NULL AS text) AS "DocumentTip",
    doc."Numar" AS "DocumentNumar",
    f."Storno" AS "Storno"
FROM (
    SELECT
        a.*,
        SUM(a."Debit" - a."Credit") OVER (
            ORDER BY a."Data", a."Id", a."Sens" DESC
            ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW
        ) AS "SoldCurent"
    FROM (
            SELECT
                r."ID" AS "Id",
                r."Data" AS "Data",
                r."NumarNota" AS "NumarNota",
                CAST('D' AS text) AS "Sens",
                r."Valoare" AS "Debit",
                CAST(0 AS numeric(18,2)) AS "Credit",
                r."ContCreditId" AS "ContrapartidaId",
                r."DocumentId" AS "DocumentId",
                r."Storno" AS "Storno",
                r."DimensiuniDebit_RepartitorId" AS "RepartitorId",
                r."DimensiuniDebit_MaterialId" AS "MaterialId",
                r."DimensiuniDebit_CodFunctionalId" AS "CodFunctionalId",
                r."DimensiuniDebit_CodEconomicId" AS "CodEconomicId",
                r."DimensiuniDebit_SursaFinantareId" AS "SursaFinantareId",
                r."DimensiuniDebit_UnitateId" AS "UnitateId",
                r."DimensiuniDebit_ProiectId" AS "ProiectId",
                r."DimensiuniDebit_CentruCostId" AS "CentruCostId"
            FROM "RegistruContabil" r
            WHERE r."GCRecord" = 0 AND r."ContDebitId" = '01a0b48c-a8b2-73ce-a6cd-a045b80364a8' AND r."Data" <= '2025-12-31'
        UNION ALL
            SELECT
                r."ID" AS "Id",
                r."Data" AS "Data",
                r."NumarNota" AS "NumarNota",
                CAST('C' AS text) AS "Sens",
                CAST(0 AS numeric(18,2)) AS "Debit",
                r."Valoare" AS "Credit",
                r."ContDebitId" AS "ContrapartidaId",
                r."DocumentId" AS "DocumentId",
                r."Storno" AS "Storno",
                r."DimensiuniCredit_RepartitorId" AS "RepartitorId",
                r."DimensiuniCredit_MaterialId" AS "MaterialId",
                r."DimensiuniCredit_CodFunctionalId" AS "CodFunctionalId",
                r."DimensiuniCredit_CodEconomicId" AS "CodEconomicId",
                r."DimensiuniCredit_SursaFinantareId" AS "SursaFinantareId",
                r."DimensiuniCredit_UnitateId" AS "UnitateId",
                r."DimensiuniCredit_ProiectId" AS "ProiectId",
                r."DimensiuniCredit_CentruCostId" AS "CentruCostId"
            FROM "RegistruContabil" r
            WHERE r."GCRecord" = 0 AND r."ContCreditId" = '01a0b48c-a8b2-73ce-a6cd-a045b80364a8' AND r."Data" <= '2025-12-31'
    ) a
    WHERE 1 = 1
) f
LEFT JOIN "Conturi" cp ON cp."ID" = f."ContrapartidaId" AND cp."GCRecord" = 0
LEFT JOIN "Repartitori" rep ON rep."ID" = f."RepartitorId" AND rep."GCRecord" = 0
LEFT JOIN "Documente" doc ON doc."ID" = f."DocumentId" AND doc."GCRecord" = 0
WHERE f."Data" >= '2025-01-01'
