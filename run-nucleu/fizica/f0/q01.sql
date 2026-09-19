-- Q01 Balanta de verificare PLATA SINTETICA pe 2025 (sold initial + rulaje + sold final, per cont)
-- Oglindeste: ContabilProiectii.Balanta(analitic:false), Proiectii/ContabilProiectii.cs:320-458
--   atomii: ContabilProiectii.cs:266-312 (unpivot UNION ALL, dimensiunile laturii)
--   sursa:  SolduriService.AtomiCumulati(os, dataEnd, granita = dataStart-1), Motor/SolduriService.cs:226-248
--   netare: ContabilProiectii.cs:441-442,451-456
-- Filtre copiate din cod:
--   GCRecord = 0        -> HasQueryFilter XAF pe calea LINQ (ContabilProiectii.cs:1034-1035)
--   Data <= dataEnd     -> SolduriService.cs:229 (reperul e RegistruContabil.Data, NU Document.Data)
--   dataStart NU e filtru -> e granita din SUM(CASE), ContabilProiectii.cs:383-386
--   Storno NU se filtreaza -> ContabilProiectii.cs:307-309 (suma algebrica e adevarul)
--   DocumentId IS NULL (randuri de deschidere) intra -> ContabilProiectii.cs:310-311
-- SQL REAL emis de EF (forma capturata in run-f28/pas3/saft-B09-4.sql), cu datele lui 2025.
-- ales: an = 2025 intreg; fara filtru de dimensiune.
-- DE CE fara snapshot, desi 12/2025 e inchisa pe Flax:
--   Referinte(os) = {2025-12} (ultima inchisa + fiecare decembrie inchis, SolduriService.cs:75-89);
--   balanta cere Referinta(os, dataStart-1 = 2024-12-31) (ContabilProiectii.cs:331),
--   iar 2025-12 se incheie la 2025-12-31 > 2024-12-31 => Referinta = null (SolduriService.cs:207-215)
--   => citire INTEGRALA din registru. Snapshot-ul de la 2025-12 NU poate servi nicio balanta a lui 2025.
SELECT u0."ContId", c0."Simbol" AS "ContSimbol", c0."Denumire" AS "ContDenumire", NULL AS "RepartitorId", NULL AS "RepartitorDenumire", u0."InitialDebit", u0."InitialCredit", CASE
    WHEN u0."InitialDebit" - u0."InitialCredit" > 0.0 THEN u0."InitialDebit" - u0."InitialCredit"
    ELSE 0.0
END AS "SoldInitialDebit", CASE
    WHEN u0."InitialDebit" - u0."InitialCredit" < 0.0 THEN -(u0."InitialDebit" - u0."InitialCredit")
    ELSE 0.0
END AS "SoldInitialCredit", u0."RulajDebit", u0."RulajCredit", CASE
    WHEN ((u0."InitialDebit" - u0."InitialCredit") + u0."RulajDebit") - u0."RulajCredit" > 0.0 THEN ((u0."InitialDebit" - u0."InitialCredit") + u0."RulajDebit") - u0."RulajCredit"
    ELSE 0.0
END AS "SoldFinalDebit", CASE
    WHEN ((u0."InitialDebit" - u0."InitialCredit") + u0."RulajDebit") - u0."RulajCredit" < 0.0 THEN -(((u0."InitialDebit" - u0."InitialCredit") + u0."RulajDebit") - u0."RulajCredit")
    ELSE 0.0
END AS "SoldFinalCredit"
FROM (
    SELECT u."ContId", COALESCE(sum(CASE
        WHEN u."Data" < '2025-01-01' THEN u."Debit"
        ELSE 0.0
    END), 0.0) AS "InitialDebit", COALESCE(sum(CASE
        WHEN u."Data" < '2025-01-01' THEN u."Credit"
        ELSE 0.0
    END), 0.0) AS "InitialCredit", COALESCE(sum(CASE
        WHEN u."Data" >= '2025-01-01' THEN u."Debit"
        ELSE 0.0
    END), 0.0) AS "RulajDebit", COALESCE(sum(CASE
        WHEN u."Data" >= '2025-01-01' THEN u."Credit"
        ELSE 0.0
    END), 0.0) AS "RulajCredit"
    FROM (
        SELECT r."Data", r."ContDebitId" AS "ContId", r."Valoare" AS "Debit", 0.0 AS "Credit"
        FROM "RegistruContabil" AS r
        WHERE r."GCRecord" = 0
        UNION ALL
        SELECT r0."Data", r0."ContCreditId" AS "ContId", 0.0 AS "Debit", r0."Valoare" AS "Credit"
        FROM "RegistruContabil" AS r0
        WHERE r0."GCRecord" = 0
    ) AS u
    WHERE u."Data" <= '2025-12-31'
    GROUP BY u."ContId"
) AS u0
LEFT JOIN (
    SELECT c."ID", c."Denumire", c."Simbol"
    FROM "Conturi" AS c
    WHERE c."GCRecord" = 0
) AS c0 ON u0."ContId" = c0."ID"
