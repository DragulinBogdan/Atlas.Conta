-- Q14 SCRIEREA: INSERT-urile pe care le face operarea unui document cu ~50 de linii
-- Oglindeste: MotorOperare.Opereaza, materializarea -- Motor/MotorOperare.cs:353-397:
--   RegistruStoc (Data = doc.DataInregistrare, :354)
--   RegistruContabil (Data = doc.DataInregistrare, :366; dimensiunile inghetate pe rand, :371-372)
--   RegistruTva (Data = doc.Data -- data FIZICA, :379; PerioadaDeclarare, :380-383; ScrisLa, :384)
-- In motor sunt `os.CreateObject<...>()` in bucla + un singur CommitChanges; EF le trimite ca
--   INSERT-uri parametrizate in loturi. Mai jos e ECHIVALENTUL `INSERT ... SELECT` din randurile
--   unui document deja operat, cu ID-uri noi -- rulat in TRANZACTIE cu ROLLBACK.
-- ales: 01a0b4d1-42ba-7622-a0c8-6a1e534425fb = FCT `FA/EU-2500084872`, 49 de linii,
--   50 de randuri contabile, 49 fiscale, 0 de stoc. E documentul-reper al masuratorilor de
--   operare din docs/api/p5-perf-masuratori.md (felia 27 si felia 28).
-- SURPRIZA de forma: factura de INTRARE nu posteaza nimic pe stoc (0 randuri) -- receptia o
--   posteaza NIR-ul conex; e limita din run-nucleu/coordonate/03-saft.md §9 / §5.6 al sintezei.
-- NU e inclus aici, dar face parte din aceeasi tranzactie: UPDATE "Documente" SET
--   "TotalStingere" = round2(SUM(LiniiCreanta.(Valoare+ValoareTva))) (MotorOperare.cs:396-397),
--   "Stare" = 1, "DataOperare", numerotarea, lotul creat de linia de intrare (:333-350)
--   si documentul conex (:404-406).
BEGIN;

INSERT INTO "RegistruStoc" ("ID","Data","TipStoc","LotId","RepartitorId","Cantitate","Valoare","Storno","DocumentId","DetaliuId","GCRecord","OptimisticLockField")
SELECT gen_random_uuid(), r."Data", r."TipStoc", r."LotId", r."RepartitorId", r."Cantitate", r."Valoare", r."Storno", r."DocumentId", r."DetaliuId", 0, 0
FROM "RegistruStoc" r
WHERE r."GCRecord" = 0 AND r."DocumentId" = '01a0b4d1-42ba-7622-a0c8-6a1e534425fb'
  AND r."Data" >= DATE '2025-01-01';

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

INSERT INTO "RegistruTva" ("ID","Data","PerioadaAn","PerioadaLuna","ScrisLa","Sens","DocumentId","DetaliuId","PartenerId","TipTvaId","Regim","Cota","Baza","Tva","Storno","GCRecord","OptimisticLockField")
SELECT gen_random_uuid(), r."Data", r."PerioadaAn", r."PerioadaLuna", now(), r."Sens", r."DocumentId", r."DetaliuId", r."PartenerId", r."TipTvaId", r."Regim", r."Cota", r."Baza", r."Tva", r."Storno", 0, 0
FROM "RegistruTva" r
WHERE r."GCRecord" = 0 AND r."DocumentId" = '01a0b4d1-42ba-7622-a0c8-6a1e534425fb'
  AND r."Data" >= DATE '2025-01-01';

ROLLBACK;
