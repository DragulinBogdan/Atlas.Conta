-- Q05 Sold cont x repartitor la 2025-12-31, pe contul 4111 (toti "partenerii")
-- Oglindeste: ContabilProiectii.SoldParteneri, Proiectii/ContabilProiectii.cs:490-538
--   sursa: SolduriService.AtomiCumulati(os, laData) FARA granita -- ContabilProiectii.cs:496
--   cheia: (ContId, RepartitorId) -- :508; LEFT JOIN pe ambele etichete -- :520-524
--   randurile cu sold net zero se omit -- :537
-- oglindit de mana (EF n-a fost capturat pe ruta asta); tiparul e cel din Q01/Q02.
-- Filtre: GCRecord=0; contId=4111; reperul e RegistruContabil.Data; Storno neatins.
-- ales: contul 4111 = 01a0b48c-a8b2-73ce-a6cd-a045b80364a8 (cel mai traficat cont de tert).
-- DE CE aproape totul vine din SNAPSHOT, spre deosebire de Q01/Q03:
--   Referinta(os, panaLa = 2025-12-31) = (2025, 12, sfarsit 2025-12-31) -- SolduriService.cs:207-215
--   => ramura de registru e `Data > 2025-12-31 AND Data <= 2025-12-31`, adica VIDA prin
--   constructie, si totusi ramane in SQL (SolduriService.cs:247). Citirea e pura pe snapshot.
SELECT s."ContId", c0."Simbol" AS "ContSimbol", c0."Denumire" AS "ContDenumire",
       s."RepartitorId", r0."Denumire" AS "RepartitorDenumire",
       s."Debit", s."Credit",
       CASE WHEN s."Debit" - s."Credit" > 0.0 THEN s."Debit" - s."Credit" ELSE 0.0 END AS "SoldDebitor",
       CASE WHEN s."Debit" - s."Credit" < 0.0 THEN -(s."Debit" - s."Credit") ELSE 0.0 END AS "SoldCreditor"
FROM (
    SELECT u."ContId", u."RepartitorId",
           COALESCE(sum(u."Debit"), 0.0) AS "Debit",
           COALESCE(sum(u."Credit"), 0.0) AS "Credit"
    FROM (
        SELECT sp."ContId", sp."RepartitorId", sp."Debit", sp."Credit"
        FROM "SolduriPerioadaContabil" AS sp
        WHERE sp."GCRecord" = 0 AND sp."An" = 2025 AND sp."Luna" = 12
        UNION ALL
        SELECT r."ContDebitId" AS "ContId", r."DimensiuniDebit_RepartitorId" AS "RepartitorId", r."Valoare" AS "Debit", 0.0 AS "Credit"
        FROM "RegistruContabil" AS r
        WHERE r."GCRecord" = 0 AND r."Data" > '2025-12-31' AND r."Data" <= '2025-12-31'
        UNION ALL
        SELECT r0."ContCreditId" AS "ContId", r0."DimensiuniCredit_RepartitorId" AS "RepartitorId", 0.0 AS "Debit", r0."Valoare" AS "Credit"
        FROM "RegistruContabil" AS r0
        WHERE r0."GCRecord" = 0 AND r0."Data" > '2025-12-31' AND r0."Data" <= '2025-12-31'
    ) AS u
    WHERE u."ContId" = '01a0b48c-a8b2-73ce-a6cd-a045b80364a8'
    GROUP BY u."ContId", u."RepartitorId"
) AS s
LEFT JOIN (SELECT c."ID", c."Denumire", c."Simbol" FROM "Conturi" AS c WHERE c."GCRecord" = 0) AS c0 ON s."ContId" = c0."ID"
LEFT JOIN (SELECT rp."ID", rp."Denumire" FROM "Repartitori" AS rp WHERE rp."GCRecord" = 0) AS r0 ON s."RepartitorId" = r0."ID"
WHERE s."Debit" - s."Credit" <> 0.0
