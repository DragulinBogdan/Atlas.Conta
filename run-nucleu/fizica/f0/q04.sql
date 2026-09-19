-- Q04 Fisa contului 4111 x UN repartitor (cel mai activ), 2025
-- Oglindeste: ContabilProiectii.FisaCont cu repartitorId dat -- filtrul de nivel (b),
--   aplicat INAINTEA ferestrei (Proiectii/ContabilProiectii.cs:864-882): "fisa pe proiectul X"
--   are si soldul initial al lui X. Textul e cel de la :812-921 cu un `Filtru()` in plus (:870).
-- ales: 01a0b48c-a9e0-7622-a340-b4c93649c78b = "Sediul central" -- 107.033 din cele 144.241
--   de randuri ale contului 4111. SURPRIZA: e o UnitateInterna, nu un Partener. Pe 4111
--   dimensiunea Repartitor poarta UnitateInterna (107.033) si ContPropriu (31.381); doar
--   5.825 de randuri poarta un Partener, cel mai activ fiind FURTUNA MARIUS cu 114 randuri.
--   (Confirma nota din run-nucleu/coordonate/01-contabil.md §5.6: RepartitorDenumire NU e
--   partenerul contului de tert.)
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
          AND a."RepartitorId" = '01a0b48c-a9e0-7622-a340-b4c93649c78b'
) f
LEFT JOIN "Conturi" cp ON cp."ID" = f."ContrapartidaId" AND cp."GCRecord" = 0
LEFT JOIN "Repartitori" rep ON rep."ID" = f."RepartitorId" AND rep."GCRecord" = 0
LEFT JOIN "Documente" doc ON doc."ID" = f."DocumentId" AND doc."GCRecord" = 0
WHERE f."Data" >= '2025-01-01'
