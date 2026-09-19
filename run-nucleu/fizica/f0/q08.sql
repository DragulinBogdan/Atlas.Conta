-- Q08 D394 pe 2025-06 (partener x cota x op, cu NrFact) -- PARTEA CARE ARE SQL
-- Oglindeste: D394Proiectii.D394, Proiectii/D394Proiectii.cs:283-708
--   pasul 1 (singura interogare pe registru): agregatul de mai jos -- :293-306, urmat de `.ToList()`
--   cheia: (DocumentId, Storno, PartenerId, Sens, TipTvaId, Cota) -- :294
-- Filtre copiate din cod:
--   perioada de DECLARARE, prin TvaProiectii.IntreLuni -- :293; ambele capete obligatorii
--     (D394Controller.cs:35-43)
--   Storno NU se filtreaza dar INTRA in cheie (la ANAF factura si stornarea ei = doua facturi) -- :288-292
--   Sens NU se filtreaza (ambele sensuri intra in acelasi agregat)
-- ales: luna 2025-06 (ceruta de spec).
-- CE RAMANE IN MEMORIE (regula de oprire a spec-ului -- nu are echivalent SQL):
--   * clasificarea partenerului: a doua interogare, `Partener` cu IgnoreQueryFilters()
--     pe ids-urile distincte (:318-324) -- se poate scrie in SQL, dar `TipPartener` si
--     `NormalizeazaCui` (taierea repetata a prefixului RO, :205-225) sunt C#;
--   * re-decizia tipului PE CUI, peste nomenclatoare ("inregistrat bate tot") -- :332-344;
--   * NrFact: 1 pe cota cu |Sum(Tva)| maxim per (Document x Storno x CheieCui x Tip),
--     cu departajare pe cota mai mare -- :422-435,455-457 (ne-aditiv, argmax);
--   * Rezumat / RezumatCote / Neincluse / cele noua avertismente -- :371-374,490,527,582-691.
SELECT r."DocumentId", r."Storno", r."PartenerId", r."Sens", r."TipTvaId", r."Cota",
       COUNT(*) AS "Randuri",
       COALESCE(SUM(r."Baza"), 0.0) AS "Baza",
       COALESCE(SUM(r."Tva"), 0.0) AS "Tva"
FROM "RegistruTva" r
WHERE r."GCRecord" = 0
  AND r."PerioadaAn" * 100 + r."PerioadaLuna" >= 202506
  AND r."PerioadaAn" * 100 + r."PerioadaLuna" <= 202506
GROUP BY r."DocumentId", r."Storno", r."PartenerId", r."Sens", r."TipTvaId", r."Cota"
