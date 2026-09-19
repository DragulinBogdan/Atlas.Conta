-- Q11 SAF-T Customers/Suppliers pe 2025-06 (sold initial + rulaje + final, partener x cont)
-- Oglindeste: SaftProiectii, agregatul de terti -- Saft/SaftProiectii.cs:388-397
--   cheia: (ContDebitId, ContCreditId, DebitRepartitorId, CreditRepartitorId) -- :391
--   apoi acumulare pe (PartenerId, Rol, ContId) si grupare pe (Partener, Rol) -- :503, :525
-- oglindit de mana (rularea capturata din run-f28/pas3 e modulul C, care n-are sectiunea asta).
-- Filtre copiate din cod:
--   GCRecord = 0; Data <= dataEnd -- :389
--   cont de debit SAU de credit cu RolTert != Niciunul -- :380-381, :390
--   dataStart NU e filtru: granita din SUM(CASE) -- :393-394
--   Storno nu se filtreaza -- nota din run-nucleu/coordonate/03-saft.md §3
--   AGREGAT PROPRIU, nu Balanta(analitic) si NU snapshot: scanare pe TOT istoricul
--     pana la dataEnd, spre deosebire de GeneralLedgerAccounts (:386-388).
-- ales: cele 16 conturi cu RolTert nenul din nomenclator (401,403,404,405,408,409,4091..4094
--   = Furnizor; 411,4111,4118,413,418,419 = Client); perioada 2025-06 (ceruta de spec).
-- CE RAMANE IN MEMORIE: PartenerulRandului (repartitorul laturii proprii daca e Partener,
--   altfel al celeilalte laturi, :469), identitatea SAF-T (prefixele 00..06, SaftReguli.cs:53-107),
--   AccountID = contul rolului cu cea mai mare miscare (argmax, :533-537), cumularea
--   partenerilor cu acelasi identificator (:2944) si tertiiReferiti cu sold zero (:1398-1415).
SELECT r."ContDebitId", r."ContCreditId",
       r."DimensiuniDebit_RepartitorId" AS "DebitRepartitorId",
       r."DimensiuniCredit_RepartitorId" AS "CreditRepartitorId",
       COALESCE(sum(CASE WHEN r."Data" < '2025-06-01' THEN r."Valoare" ELSE 0.0 END), 0.0) AS "Initial",
       COALESCE(sum(CASE WHEN r."Data" >= '2025-06-01' THEN r."Valoare" ELSE 0.0 END), 0.0) AS "Rulaj"
FROM "RegistruContabil" AS r
WHERE r."GCRecord" = 0 AND r."Data" <= '2025-06-30'
  AND (r."ContDebitId" = ANY ('{01a0b48c-a8aa-7e36-8f14-4fa2fa4ea009,01a0b48c-a8ab-7b50-b3b7-071a2b7a349b,01a0b48c-a8ac-7e4b-a6f0-9387b76e96eb,01a0b48c-a8ac-7ab5-a52e-152a7dc2545f,01a0b48c-a8ad-7886-bf9d-1ef6ec9b9d85,01a0b48c-a8ad-738e-9300-b9812b654f5d,01a0b48c-a8ae-7e28-ab0c-d459207dc577,01a0b48c-a8af-76f6-8adc-75a198ef6a63,01a0b48c-a8af-70d2-ad7b-1d1f3709b4fe,01a0b48c-a8b0-734d-a49d-507ee62971ef,01a0b48c-a8b1-7d53-a2d6-b11eb0cc58ed,01a0b48c-a8b2-73ce-a6cd-a045b80364a8,01a0b48c-a8b2-7841-9d84-1c268bdb59e3,01a0b48c-a8b3-7212-9f12-a75dc52b91c2,01a0b48c-a8b3-70d2-85c6-14793081762f,01a0b48c-a8b4-75ad-a6f5-0403d7678e18}')
    OR r."ContCreditId" = ANY ('{01a0b48c-a8aa-7e36-8f14-4fa2fa4ea009,01a0b48c-a8ab-7b50-b3b7-071a2b7a349b,01a0b48c-a8ac-7e4b-a6f0-9387b76e96eb,01a0b48c-a8ac-7ab5-a52e-152a7dc2545f,01a0b48c-a8ad-7886-bf9d-1ef6ec9b9d85,01a0b48c-a8ad-738e-9300-b9812b654f5d,01a0b48c-a8ae-7e28-ab0c-d459207dc577,01a0b48c-a8af-76f6-8adc-75a198ef6a63,01a0b48c-a8af-70d2-ad7b-1d1f3709b4fe,01a0b48c-a8b0-734d-a49d-507ee62971ef,01a0b48c-a8b1-7d53-a2d6-b11eb0cc58ed,01a0b48c-a8b2-73ce-a6cd-a045b80364a8,01a0b48c-a8b2-7841-9d84-1c268bdb59e3,01a0b48c-a8b3-7212-9f12-a75dc52b91c2,01a0b48c-a8b3-70d2-85c6-14793081762f,01a0b48c-a8b4-75ad-a6f5-0403d7678e18}'))
GROUP BY r."ContDebitId", r."ContCreditId", r."DimensiuniDebit_RepartitorId", r."DimensiuniCredit_RepartitorId"
