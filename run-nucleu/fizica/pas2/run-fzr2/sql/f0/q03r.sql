EXPLAIN (ANALYZE, BUFFERS)
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
            WHERE r."GCRecord" = 0 AND r."ContDebitId" = '01a0b48c-a8b2-73ce-a6cd-a045b80364a8' AND r."Data" <= '2025-12-31' AND r."Data" > '2024-12-31'
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
            WHERE r."GCRecord" = 0 AND r."ContCreditId" = '01a0b48c-a8b2-73ce-a6cd-a045b80364a8' AND r."Data" <= '2025-12-31' AND r."Data" > '2024-12-31'
        UNION ALL
            SELECT
                '00000000-0000-0000-0000-000000000000'::uuid AS "Id",
                '2024-12-31'::date AS "Data",
                CAST(NULL AS text) AS "NumarNota",
                CAST('S' AS text) AS "Sens",
                COALESCE(SUM(s."Debit"), CAST(0 AS numeric(18,2))) AS "Debit",
                COALESCE(SUM(s."Credit"), CAST(0 AS numeric(18,2))) AS "Credit",
                CAST(NULL AS uuid) AS "ContrapartidaId",
                CAST(NULL AS uuid) AS "DocumentId",
                CAST(false AS boolean) AS "Storno",
                CAST(NULL AS uuid) AS "RepartitorId",
                CAST(NULL AS uuid) AS "MaterialId",
                CAST(NULL AS uuid) AS "CodFunctionalId",
                CAST(NULL AS uuid) AS "CodEconomicId",
                CAST(NULL AS uuid) AS "SursaFinantareId",
                CAST(NULL AS uuid) AS "UnitateId",
                CAST(NULL AS uuid) AS "ProiectId",
                CAST(NULL AS uuid) AS "CentruCostId"
            FROM "SolduriPerioadaContabil" s
            WHERE s."GCRecord" = 0 AND s."An" = 2025 AND s."Luna" = 12
              AND s."ContId" = '01a0b48c-a8b2-73ce-a6cd-a045b80364a8'
    ) a
    WHERE 1 = 1
) f
LEFT JOIN "Conturi" cp ON cp."ID" = f."ContrapartidaId" AND cp."GCRecord" = 0
LEFT JOIN "Repartitori" rep ON rep."ID" = f."RepartitorId" AND rep."GCRecord" = 0
LEFT JOIN "Documente" doc ON doc."ID" = f."DocumentId" AND doc."GCRecord" = 0
WHERE f."Data" >= '2025-01-01';
