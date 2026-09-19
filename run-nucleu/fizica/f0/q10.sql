-- Q10 SAF-T PhysicalStock la 2025-12-31 (toate loturile cu Suma cant > 0, cu valoare)
-- Oglindeste: SaftProiectii.AgregatStoc, Saft/SaftProiectii.cs:2592-2608 -- O SINGURA interogare
--   grupata pe (gestiune x lot x TipStoc), cu sume conditionate; NU filtreaza pe TipStoc.
--   Reducerea pe (gestiune x lot) peste TipStoc-urile RAPORTATE se face in memorie:
--   SaftProiectii.SoldPeCheie, :2613-2625.
-- SQL REAL emis de EF, capturat in run-f28/pas3/saft-B12-0.sql (rularea D406 S pe 2025-12
--   din pasul 3 al feliei 28) -- copiat aici cuvant cu cuvant.
-- Filtre copiate din cod:
--   GCRecord = 0; Data <= dataEnd (2025-12-31) -- :2600
--   dataStart (2025-12-01) NU e filtru: e granita din SUM(CASE) initial/rulaj -- :2604-2607
--   REGISTRU BRUT, FARA snapshot -- spre deosebire de GeneralLedgerAccounts (nota :2595-2597);
--   PhysicalStock nu trece prin SolduriService, deci nu vede SolduriPerioadaStoc.
--   Storno nu se filtreaza.
-- ales: perioada = 2025-12 (modulul C al D406 cere o LUNA, SaftController.cs:242).
-- CE RAMANE IN MEMORIE: TipStoc-urile raportate (cele care apar intr-o PoliticaMiscareSaft
--   CU cod, :1687), multimea de chei (deschideri U inchideri U chei cu miscare, :1832-1834),
--   StockAccountNo (LotId doar daca produsul are >1 lot in aceeasi gestiune, :1896-1903),
--   UnitPrice = Lot.PretUnitar rotunjit (:1810,1884) si avertismentele (SoldNegativ,
--   ReziduValoricFaraCantitate, ProdusFaraContStoc).
SELECT r."RepartitorId", r."LotId", r."TipStoc", COALESCE(sum(CASE
	    WHEN r."Data" < '2025-12-01' THEN r."Cantitate"
	    ELSE 0.0
	END), 0.0) AS "CantitateInitiala", COALESCE(sum(CASE
	    WHEN r."Data" < '2025-12-01' THEN r."Valoare"
	    ELSE 0.0
	END), 0.0) AS "ValoareInitiala", COALESCE(sum(CASE
	    WHEN r."Data" >= '2025-12-01' THEN r."Cantitate"
	    ELSE 0.0
	END), 0.0) AS "CantitateRulaj", COALESCE(sum(CASE
	    WHEN r."Data" >= '2025-12-01' THEN r."Valoare"
	    ELSE 0.0
	END), 0.0) AS "ValoareRulaj", COALESCE(sum(CASE
	    WHEN r."Data" < '2025-12-01' THEN 1
	    ELSE 0
	END), 0)::int AS "RanduriInitiale", count(*)::int AS "Randuri"
	FROM "RegistruStoc" AS r
	WHERE r."GCRecord" = 0 AND r."Data" <= '2025-12-31'
	GROUP BY r."RepartitorId", r."LotId", r."TipStoc"