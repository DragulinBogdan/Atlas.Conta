-- Q09 Intrarea FIFO: loturile cu sold > 0 dintr-o gestiune, la 2025-06-30
-- Oglindeste: StocService.AlocaFifoTolerant, Motor/StocService.cs:259-283
--   sursa: SolduriService.MiscariCumulate(os, data, produsId) -- Motor/SolduriService.cs:271-297
--   filtrele de cheie: RepartitorId == gestiune && TipStoc -- StocService.cs:263
--   gruparea pe LotId si `Sold > 0` -- :264-266
-- oglindit de mana. Filtre copiate din cod:
--   GCRecord = 0 (calea LINQ); Data <= data -- SolduriService.cs:275
--     reperul e RegistruStoc.Data = Document.DataInregistrare (MotorOperare.cs:354)
--   produsId traieste pe navigatia Lot -- SolduriService.cs:279 (INNER JOIN "Loturi")
--   Storno NU se filtreaza (randurile inverse se anuleaza algebric)
-- ales: gestiunea TRANZIT = 01a0b48c-d7ca-73c5-88b4-c0b508a78342 (58.703 randuri de registru,
--   cea mai activa);
--   produsul 01a0b48d-78b6-754c-92e9-c3250a37995b (SED000097087/381) cu TipStoc = 1 (Magazie):
--   perechea (produs x gestiune x TipStoc) cu CELE MAI MULTE loturi inca pozitive la data
--   ceruta pe toata baza -- 30 de loturi; adica cel mai lung lant FIFO existent la 2025-06-30.
-- FARA snapshot: Referinta(os, 2025-06-30) = null (singura referinta, 2025-12, se incheie
--   la 2025-12-31 > 2025-06-30) -- SolduriService.cs:293.
-- CE RAMANE IN MEMORIE: ordinea FIFO insasi. `ORDER BY Lot.Data, Lot.ID` se face in C#,
--   dupa un `os.GetObjectByKey<Lot>(x.LotId)` PER LOT (StocService.cs:272-273) -- N+1 pe
--   numarul de loturi cu sold; plus scaderea lui `dejaAlocat` (:276) si taierea la cantitatea
--   ceruta (:277-282). SQL-ul de mai jos produce doar multimea candidatilor, neordonata.
SELECT m."LotId", COALESCE(SUM(m."Cantitate"), 0.0) AS "Sold"
FROM (
    SELECT r."LotId", r."RepartitorId", r."TipStoc", r."Cantitate"
    FROM "RegistruStoc" AS r
    INNER JOIN (SELECT l."ID", l."ProdusId" FROM "Loturi" AS l WHERE l."GCRecord" = 0) AS l0 ON r."LotId" = l0."ID"
    WHERE r."GCRecord" = 0 AND r."Data" <= '2025-06-30'
      AND l0."ProdusId" = '01a0b48d-78b6-754c-92e9-c3250a37995b'
) AS m
WHERE m."RepartitorId" = '01a0b48c-d7ca-73c5-88b4-c0b508a78342' AND m."TipStoc" = 1
GROUP BY m."LotId"
HAVING COALESCE(SUM(m."Cantitate"), 0.0) > 0.0
