# Decizia 34 — Pasul 4 — migrarea datelor

- **Data**: 2026-07-22 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §34

---

**Pasul 4 — migrarea datelor — executat; unealta `nou/tools/Migrare`
(consolă SQL Server → Postgres prin `EFCoreObjectSpaceProvider`, ca
ModelCheck), validată pe Contabilitate_2026. Tranșările:**
(a) **Sursa = deschiderea materializată de trecerea de an legacy** în baza
anului nou (`solduri_repartitori` + LDI-ul administrativ de 31.12 scris de
`spPreiaStocuriInitiale` + `gest_gnmcl`), NU recalcularea închiderii:
legacy își închide anul cu procedurile proprii, migrarea preia rezultatul.
La go-live: întâi trecerea de an legacy, apoi unealta.
(b) **Idempotență prin `MigrareLegatura`** (tabelă nouă: Tabela +
CheieLegacy → TintaId, index unic) pentru sursele cheiate pe id
(REPARTITORI, CASIERIE, GEST_SUMATOR, GEST_GNMCL, OI_*); nomenclatoarele
cu cod natural (CodFunctional/CodEconomic, perioade) fac upsert pe cod.
Rândurile de deschidere (DocumentId=null — decizia 25e, exclusiv
proprietatea migrării) se rescriu integral la fiecare rulare.
(c) **Clasificarea repartitorilor** (inventar 10 §1 → decizia 16):
purtătorii de stoc de deschidere FORȚAȚI `Gestiune` (lotul cere gestiune —
semnalul datelor bate eticheta legacy), apoi Gestiuni(7/18)→Gestiune,
Salariați(3)→Angajat, Unități/Departament(11/19)→UnitateInterna, restul
clasificărilor de identitate→Partener; neclasificați: GESTINT=1→
UnitateInterna, altfel Partener. Gestionar/Comisie/Departament/CentruCost/
LocConsum → `Calitati` (cu `|=`). ContImplicit din REPARTITORI.CONT cu
tăierea segmentelor; `casierie` → ContPropriu de sine stătător (CRSP_LEI →
ContImplicit, denumirea-IBAN → Iban).
(d) **Solduri contabile = rânduri RegistruContabil contra 891.01.00**
(„Bilanț de deschidere", există în CPLAN) la 31.12.(an−1), dimensiunile
(R/CF/CE/U/P) pe latura contului, analiticele legacy tăiate la plan.
Clasa 8 (17,2M — evidența angajamentelor/creditelor bugetare) NU se
migrează — vine cu modulul angajamente (deciziile 9/22c). SursaFinantare
rămâne goală (mecanismul de defalcare — decizia 21). **Terții pornesc pe
sold per partener, FĂRĂ facturi/imperecheri istorice** — exact modelul
legacy (GEST_DECONTARI începe gol la an nou); plățile pe facturi vechi
sting soldul de deschidere fără imperechere.
(e) **Stoc = Lot per codmat + rând RegistruStoc per linie LDI** (tip stoc
din clasa Tipului — maparea NIR/28a); produsul din sumator (surogat per
codmat unde lipsește), migrate DOAR cele referite de deschidere (inventar
10 §3). PretUnitar din linia LDI, atributele din gnmcl; Lot.Data =
data_cod reală (ordinea FIFO istorică se păstrează).
(f) **Reconcilierea** (contractul pasului 4): soldurile citite ÎNAPOI din
Postgres = soldurile legacy per cont (0 diferențe; 891 se închide la 0;
stocul nou = LDI-ul legacy per cont); nepotrivirea stoc↔contabilitate a
LEGACY-ului (303.x fără detaliu de stoc în sursă ~2,06M; combustibil) se
RAPORTEAZĂ, nu se ascunde — diferențele sunt ale sursei și se tranșează la
go-live (inventariere sau acceptare explicită).
(g) Amânate, documentate: adrese/conturi bancare/delegați (satelit,
aditiv), JUDETE/LOCALITATI/valute, importul FCT (Tethys) și extrasele de
trezorerie (la pasul 5, ca API).
