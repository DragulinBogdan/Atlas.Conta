# Decizia 28 — Felia 3c-3 — ListaDiferenteInventar

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §28

---

**Felia 3c-3 — ListaDiferenteInventar — executată; validată e2e în
ModelCheck (sold deschidere → LDI cu minus + plus pe aceeași listă →
gardieni → anulare directă → storno). Tranșările:**
(a) **Direcția explicită se materializează în semn la operare**
(`Directie` pe detaliu — testul bazei §4/22): UI-ul culege cantitatea
pozitivă, `PregatesteOperare` semnează Cantitate ȘI Valoare (minus =
cantitate negativă × preț lot; plus = cantitate × `PretEvaluare` cules) —
idempotent (Abs înainte de semnare). Un SINGUR set de reguli de stoc:
+1 pe predator (gestiunea inventariată), cantitatea semnată dă direcția
(exact mecanismul legacy defa 270, cu direcția explicită în loc de
convenție); tip stoc per clasă ca la NIR (generic→Magazie, OF→Folosință,
MC→Custodie).
(b) **`RegulaContare.SemnFiltru` (±1/null)** — extensia aditivă care
desparte contarea pe direcție (inventar 05: LDI e singurul tip din set
unde semnul chiar diferențiază). Nepotrivirea de semn scoate regula din
joc la TOATE nivelurile de specificitate (plusul sare peste regula exactă
de minus și cade pe genericul de plus); valoarea postată se normalizează
cu semnul filtrului (`(SemnFiltru ?? +1) × Valoare`) — nota minusului e
pozitivă deși linia poartă valoare negativă.
(c) **Seed contare LDI**: minus = 6xx = 3xx per TipMaterial cu
SemnFiltru=-1 — aceeași derivare din simbol ca la BCS, extrasă în
helper-ul comun `SeedContare6xxDin3xx` (incremental la updater); plus =
un rând generic Natura=Stoc, SemnFiltru=+1, debit din contul Tipului,
credit explicit **791.00.00** (CPLAN nu are 758 — 791 „Venituri din
valorificarea unor bunuri ale statului" e echivalentul din planul
instituțiilor publice).
(d) **Plusul creează lot** la culegere pe propria linie (`CreeazaLot`,
gestiunea = predatorul), cu `ILinieCuAtributeLot` (LotFabricatie/
DataExpirare pe poziție — inventar 05); motorul îl finalizează la operare
(PretUnitar = PretEvaluare). Minusul descarcă un lot EXISTENT (validare:
lotul nu e creat de linia proprie; refolosirea lotului altcuiva pe plus =
refuz). **Primitorul = comisia de inventariere** — calitatea transversală
`Comisie` (decizia 16), seed repartitor `COMISIE` (UnitateInterna, `|=`).
(e) Validări proprii: predator Gestiune, primitor intern cu Comisie,
direcție setată explicit (enum-ul nu are default valid — protecție la
linii culese fără direcție), cantitate ≠ 0, lot per linie, plus cu preț
de evaluare pozitiv și lot în gestiunea inventariată.
