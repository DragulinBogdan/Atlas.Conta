-- Q12 SAF-T GeneralLedgerEntries pe 2025-06: randurile contabile ale lunii
-- Oglindeste: SaftProiectii, proiectia plata RandGl -- Saft/SaftProiectii.cs:331-347,
--   urmata de `.ToList()` (materializare INTEGRALA, fara paginare -- SaftController.cs:28).
-- oglindit de mana (rularea capturata din run-f28/pas3 e modulul C, care n-are GLE).
-- Filtre copiate din cod:
--   GCRecord = 0 (calea LINQ); Data >= dataStart AND Data <= dataEnd -- :332
--   DocumentId IS NOT NULL -- :332: randurile de DESCHIDERE sunt solduri, nu tranzactii
--   Storno NU se filtreaza; randurile inverse intra ca fapte proprii
--   reperul e RegistruContabil.Data; RegistruTva se filtreaza pe PerioadaDeclarare --
--     DOUA repere diferite in aceeasi proiectie (:355, nota din 03-saft.md §8)
--   IgnoreAutoIncludes(): cele 16 navigatii de dimensiuni puse de BackOfficeDbContext -- :331
-- ales: luna 2025-06 (ceruta de spec).
-- CELELALTE DOUA INTEROGARI ale aceleiasi sectiuni, tot materializate integral:
--   (a) faptele fiscale ale perioadei de declarare -- :353-358
--       SELECT "ID","DocumentId","DetaliuId","Sens","TipTvaId","Regim","Cota","Baza","Tva","Storno"
--       FROM "RegistruTva" WHERE "GCRecord"=0 AND "PerioadaAn"*100+"PerioadaLuna" BETWEEN 202506 AND 202506;
--   (b) antetele documentelor atinse (contabil U fiscal) -- :371-375
--       SELECT "ID","Numar","Data","DataOperare","PredatorId","PrimitorId" FROM "Documente"
--       WHERE "GCRecord"=0 AND "ID" = ANY (...ids...);
--   (c) codul de tip: CititorTipDocument, a treia interogare, pe ("ID","ClrType") -- Api/CititorTipDocument.cs:13-43.
-- CE RAMANE IN MEMORIE: gruparea Journal/Transaction/TransactionLine, TransactionDate =
--   MIN(Data) al documentului (:725), identitatile de latura (:678-696), TaxInformation pe
--   cheia (DetaliuId, Storno) (:359-362), ordonarile (:704-707, :750-753) si totalurile.
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
  AND r."DocumentId" IS NOT NULL
