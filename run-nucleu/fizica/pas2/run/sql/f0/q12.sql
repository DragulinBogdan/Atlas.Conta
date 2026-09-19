EXPLAIN (ANALYZE, BUFFERS)
SELECT r."ID" AS "Id", r."Data", r."NumarNota", r."DocumentId", r."DetaliuId",
       r."ContDebitId", r."ContCreditId", r."Valoare", r."Storno",
       r."DimensiuniDebit_RepartitorId", r."DimensiuniDebit_CodFunctionalId",
       r."DimensiuniDebit_CodEconomicId", r."DimensiuniDebit_SursaFinantareId",
       r."DimensiuniDebit_UnitateId", r."DimensiuniDebit_ProiectId", r."DimensiuniDebit_CentruCostId",
       r."DimensiuniCredit_RepartitorId", r."DimensiuniCredit_CodFunctionalId",
       r."DimensiuniCredit_CodEconomicId", r."DimensiuniCredit_SursaFinantareId",
       r."DimensiuniCredit_UnitateId", r."DimensiuniCredit_ProiectId", r."DimensiuniCredit_CentruCostId"
FROM "RegistruContabil" AS r
WHERE r."GCRecord" = 0 AND r."Data" >= '2025-06-01' AND r."Data" <= '2025-06-30'
  AND r."DocumentId" IS NOT NULL;
