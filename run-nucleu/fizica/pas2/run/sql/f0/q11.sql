EXPLAIN (ANALYZE, BUFFERS)
SELECT r."ContDebitId", r."ContCreditId",
       r."DimensiuniDebit_RepartitorId" AS "DebitRepartitorId",
       r."DimensiuniCredit_RepartitorId" AS "CreditRepartitorId",
       COALESCE(sum(CASE WHEN r."Data" < '2025-06-01' THEN r."Valoare" ELSE 0.0 END), 0.0) AS "Initial",
       COALESCE(sum(CASE WHEN r."Data" >= '2025-06-01' THEN r."Valoare" ELSE 0.0 END), 0.0) AS "Rulaj"
FROM "RegistruContabil" AS r
WHERE r."GCRecord" = 0 AND r."Data" <= '2025-06-30'
  AND (r."ContDebitId" = ANY ('{01a0b48c-a8aa-7e36-8f14-4fa2fa4ea009,01a0b48c-a8ab-7b50-b3b7-071a2b7a349b,01a0b48c-a8ac-7e4b-a6f0-9387b76e96eb,01a0b48c-a8ac-7ab5-a52e-152a7dc2545f,01a0b48c-a8ad-7886-bf9d-1ef6ec9b9d85,01a0b48c-a8ad-738e-9300-b9812b654f5d,01a0b48c-a8ae-7e28-ab0c-d459207dc577,01a0b48c-a8af-76f6-8adc-75a198ef6a63,01a0b48c-a8af-70d2-ad7b-1d1f3709b4fe,01a0b48c-a8b0-734d-a49d-507ee62971ef,01a0b48c-a8b1-7d53-a2d6-b11eb0cc58ed,01a0b48c-a8b2-73ce-a6cd-a045b80364a8,01a0b48c-a8b2-7841-9d84-1c268bdb59e3,01a0b48c-a8b3-7212-9f12-a75dc52b91c2,01a0b48c-a8b3-70d2-85c6-14793081762f,01a0b48c-a8b4-75ad-a6f5-0403d7678e18}')
    OR r."ContCreditId" = ANY ('{01a0b48c-a8aa-7e36-8f14-4fa2fa4ea009,01a0b48c-a8ab-7b50-b3b7-071a2b7a349b,01a0b48c-a8ac-7e4b-a6f0-9387b76e96eb,01a0b48c-a8ac-7ab5-a52e-152a7dc2545f,01a0b48c-a8ad-7886-bf9d-1ef6ec9b9d85,01a0b48c-a8ad-738e-9300-b9812b654f5d,01a0b48c-a8ae-7e28-ab0c-d459207dc577,01a0b48c-a8af-76f6-8adc-75a198ef6a63,01a0b48c-a8af-70d2-ad7b-1d1f3709b4fe,01a0b48c-a8b0-734d-a49d-507ee62971ef,01a0b48c-a8b1-7d53-a2d6-b11eb0cc58ed,01a0b48c-a8b2-73ce-a6cd-a045b80364a8,01a0b48c-a8b2-7841-9d84-1c268bdb59e3,01a0b48c-a8b3-7212-9f12-a75dc52b91c2,01a0b48c-a8b3-70d2-85c6-14793081762f,01a0b48c-a8b4-75ad-a6f5-0403d7678e18}'))
GROUP BY r."ContDebitId", r."ContCreditId", r."DimensiuniDebit_RepartitorId", r."DimensiuniCredit_RepartitorId";
