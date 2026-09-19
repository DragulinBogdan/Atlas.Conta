BEGIN;

EXPLAIN (ANALYZE, BUFFERS)
INSERT INTO "RegistruStoc" ("ID","Data","TipStoc","LotId","RepartitorId","Cantitate","Valoare","Storno","DocumentId","DetaliuId","GCRecord","OptimisticLockField")
SELECT gen_random_uuid(), r."Data", r."TipStoc", r."LotId", r."RepartitorId", r."Cantitate", r."Valoare", r."Storno", r."DocumentId", r."DetaliuId", 0, 0
FROM "RegistruStoc" r
WHERE r."GCRecord" = 0 AND r."DocumentId" = '01a0b4d1-42ba-7622-a0c8-6a1e534425fb'
  AND r."Data" >= DATE '2025-01-01';

EXPLAIN (ANALYZE, BUFFERS)
INSERT INTO "RegistruContabil" ("ID","Data","NumarNota","ContDebitId","ContCreditId","Valoare",
  "DimensiuniDebit_RepartitorId","DimensiuniDebit_MaterialId","DimensiuniDebit_CodFunctionalId","DimensiuniDebit_CodEconomicId","DimensiuniDebit_SursaFinantareId","DimensiuniDebit_UnitateId","DimensiuniDebit_ProiectId","DimensiuniDebit_CentruCostId",
  "DimensiuniCredit_RepartitorId","DimensiuniCredit_MaterialId","DimensiuniCredit_CodFunctionalId","DimensiuniCredit_CodEconomicId","DimensiuniCredit_SursaFinantareId","DimensiuniCredit_UnitateId","DimensiuniCredit_ProiectId","DimensiuniCredit_CentruCostId",
  "Storno","DocumentId","DetaliuId","GCRecord","OptimisticLockField")
SELECT gen_random_uuid(), r."Data", r."NumarNota", r."ContDebitId", r."ContCreditId", r."Valoare",
  r."DimensiuniDebit_RepartitorId", r."DimensiuniDebit_MaterialId", r."DimensiuniDebit_CodFunctionalId", r."DimensiuniDebit_CodEconomicId", r."DimensiuniDebit_SursaFinantareId", r."DimensiuniDebit_UnitateId", r."DimensiuniDebit_ProiectId", r."DimensiuniDebit_CentruCostId",
  r."DimensiuniCredit_RepartitorId", r."DimensiuniCredit_MaterialId", r."DimensiuniCredit_CodFunctionalId", r."DimensiuniCredit_CodEconomicId", r."DimensiuniCredit_SursaFinantareId", r."DimensiuniCredit_UnitateId", r."DimensiuniCredit_ProiectId", r."DimensiuniCredit_CentruCostId",
  r."Storno", r."DocumentId", r."DetaliuId", 0, 0
FROM "RegistruContabil" r
WHERE r."GCRecord" = 0 AND r."DocumentId" = '01a0b4d1-42ba-7622-a0c8-6a1e534425fb'
  AND r."Data" >= DATE '2025-01-01';

EXPLAIN (ANALYZE, BUFFERS)
INSERT INTO "RegistruTva" ("ID","Data","PerioadaAn","PerioadaLuna","ScrisLa","Sens","DocumentId","DetaliuId","PartenerId","TipTvaId","Regim","Cota","Baza","Tva","Storno","GCRecord","OptimisticLockField")
SELECT gen_random_uuid(), r."Data", r."PerioadaAn", r."PerioadaLuna", now(), r."Sens", r."DocumentId", r."DetaliuId", r."PartenerId", r."TipTvaId", r."Regim", r."Cota", r."Baza", r."Tva", r."Storno", 0, 0
FROM "RegistruTva" r
WHERE r."GCRecord" = 0 AND r."DocumentId" = '01a0b4d1-42ba-7622-a0c8-6a1e534425fb'
  AND r."Data" >= DATE '2025-01-01';

ROLLBACK;
