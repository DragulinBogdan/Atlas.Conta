-- Q06 Documentele cu rest la 2025-12-31 (partidele deschise), forma NEFILTRATA si FARA LIMIT
-- Oglindeste: ImperecheriProiectii.DocumenteCuRest, Proiectii/ImperecheriProiectii.cs:110-225
--   antete: uniune pe SASE tipuri concrete, contrapartida si sensul LITERALE per ramura (:118-168)
--   Asignari (unpivot pe ambele laturi, ALGEBRIC): :94-104
--   fereastra deschisa: Asignari(os, sfarsitReferinta, laData).GroupBy(DocumentId) (:195-197)
--   partide: PartidaDeschisa a referintei (:202-204)
--   restul in trei feluri + multimea de candidati: :215-223, :226-227
-- oglindit de mana; forma V0 masurata in docs/api/p5-perf-masuratori.md §Felia 27
--   ("nefiltrat, FARA LIMIT" = 220-232 ms acolo) -- exact forma consumata de
--   PerioadaService.RestScadent, nu pagina de grila.
-- Filtre copiate din cod:
--   Stare = 1 (Operat) pe FIECARE ramura -- :118,124,139,147,154,164
--   DataInregistrare <= laData -- :171-172 (COLOANA AFISATA e `Data`, data fizica: doua repere)
--   PLT/INC: se exclud picioarele de virament intern (ambele laturi ContPropriu) -- :140,148
--   Storno / registre: NICIUN registru nu intra in acest raport
--   filtrul final Rest > 0 -- :248
-- ales: laData = 2025-12-31, fara contrapartidaId si fara sens (cazul greu: toate documentele).
-- Referinta = (2025,12), deci fereastra de imperecheri e `Data > 2025-12-31 AND <= 2025-12-31`,
--   adica VIDA -- restul vine integral din PartideDeschise (201.046 randuri).
SELECT a."DocumentId", a."Tip", a."Numar", a."Data", a."ContrapartidaId", a."ContrapartidaDenumire",
       a."Sens", a."Total",
       a."Total" - (COALESCE(p."Suma", CASE WHEN a."DataInregistrare" > '2025-12-31' THEN a."Total" ELSE 0.0 END) - COALESCE(s."Suma", 0.0)) AS "Asignat",
       COALESCE(p."Suma", CASE WHEN a."DataInregistrare" > '2025-12-31' THEN a."Total" ELSE 0.0 END) - COALESCE(s."Suma", 0.0) AS "Rest"
FROM (
    SELECT d."ID" AS "DocumentId", 'FCT' AS "Tip", d."Numar", d."Data", d."DataInregistrare",
           d."PredatorId" AS "ContrapartidaId", rp."Denumire" AS "ContrapartidaDenumire",
           'Datorie' AS "Sens", COALESCE(d."TotalStingere", 0.0) AS "Total"
    FROM "Documente" d LEFT JOIN "Repartitori" rp ON rp."ID" = d."PredatorId" AND rp."GCRecord" = 0
    WHERE d."GCRecord" = 0 AND d."ClrType" = 'FacturaIntrare' AND d."Stare" = 1
  UNION ALL
    SELECT d."ID", 'FCL', d."Numar", d."Data", d."DataInregistrare",
           d."PrimitorId", rp."Denumire", 'Creanta', COALESCE(d."TotalStingere", 0.0)
    FROM "Documente" d LEFT JOIN "Repartitori" rp ON rp."ID" = d."PrimitorId" AND rp."GCRecord" = 0
    WHERE d."GCRecord" = 0 AND d."ClrType" = 'FacturaIesire' AND d."Stare" = 1
  UNION ALL
    SELECT d."ID", 'PLT', d."Numar", d."Data", d."DataInregistrare",
           d."PrimitorId", rp."Denumire", 'Creanta', COALESCE(d."TotalStingere", 0.0)
    FROM "Documente" d
    LEFT JOIN "Repartitori" rp ON rp."ID" = d."PrimitorId" AND rp."GCRecord" = 0
    LEFT JOIN "Repartitori" rpd ON rpd."ID" = d."PredatorId" AND rpd."GCRecord" = 0
    WHERE d."GCRecord" = 0 AND d."ClrType" = 'Plata' AND d."Stare" = 1
      AND NOT (rpd."ClrType" = 'ContPropriu' AND rp."ClrType" = 'ContPropriu')
  UNION ALL
    SELECT d."ID", 'INC', d."Numar", d."Data", d."DataInregistrare",
           d."PredatorId", rpd."Denumire", 'Datorie', COALESCE(d."TotalStingere", 0.0)
    FROM "Documente" d
    LEFT JOIN "Repartitori" rpd ON rpd."ID" = d."PredatorId" AND rpd."GCRecord" = 0
    LEFT JOIN "Repartitori" rp ON rp."ID" = d."PrimitorId" AND rp."GCRecord" = 0
    WHERE d."GCRecord" = 0 AND d."ClrType" = 'Incasare' AND d."Stare" = 1
      AND NOT (rpd."ClrType" = 'ContPropriu' AND rp."ClrType" = 'ContPropriu')
  UNION ALL
    SELECT d."ID", 'DEC', d."Numar", d."Data", d."DataInregistrare",
           d."PredatorId", rp."Denumire", 'Datorie', COALESCE(d."TotalStingere", 0.0)
    FROM "Documente" d LEFT JOIN "Repartitori" rp ON rp."ID" = d."PredatorId" AND rp."GCRecord" = 0
    WHERE d."GCRecord" = 0 AND d."ClrType" = 'Decont' AND d."Stare" = 1
  UNION ALL
    SELECT d."ID", 'RDC', d."Numar", d."Data", d."DataInregistrare",
           d."PredatorId", rp."Denumire", 'Datorie', COALESCE(d."TotalStingere", 0.0)
    FROM "Documente" d LEFT JOIN "Repartitori" rp ON rp."ID" = d."PredatorId" AND rp."GCRecord" = 0
    WHERE d."GCRecord" = 0 AND d."ClrType" = 'ReturClient' AND d."Stare" = 1
) AS a
LEFT JOIN (
    SELECT pd."DocumentId", pd."Rest" AS "Suma"
    FROM "PartideDeschise" pd
    WHERE pd."GCRecord" = 0 AND pd."An" = 2025 AND pd."Luna" = 12
) AS p ON p."DocumentId" = a."DocumentId"
LEFT JOIN (
    SELECT x."DocumentId", SUM(x."Suma") AS "Suma"
    FROM (
        SELECT i."DocumentStingatorId" AS "DocumentId", i."Suma"
        FROM "Imperecheri" i
        WHERE i."GCRecord" = 0 AND i."Data" > '2025-12-31' AND i."Data" <= '2025-12-31'
        UNION ALL
        SELECT i."DocumentId", i."Suma"
        FROM "Imperecheri" i
        WHERE i."GCRecord" = 0 AND i."Data" > '2025-12-31' AND i."Data" <= '2025-12-31'
    ) x
    GROUP BY x."DocumentId"
) AS s ON s."DocumentId" = a."DocumentId"
WHERE a."DataInregistrare" <= '2025-12-31'
  AND (p."Suma" IS NOT NULL OR s."Suma" IS NOT NULL OR a."DataInregistrare" > '2025-12-31')
  AND COALESCE(p."Suma", CASE WHEN a."DataInregistrare" > '2025-12-31' THEN a."Total" ELSE 0.0 END) - COALESCE(s."Suma", 0.0) > 0.0
