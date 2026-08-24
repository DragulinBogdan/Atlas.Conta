# Decizia 47 — Felia 1C-b — scheletul Import1C + nomenclatoare + deschiderea

- **Data**: 2026-07-25 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §47
- **Docs**: docs/import/faza-1c-design.md

---

**Felia 1C-b — scheletul Import1C + nomenclatoare + deschiderea — executată;
validată pe date reale (contract verde, sabotaj-test exit 1) + review advers
cu cele 3 fix-uri de fond aplicate. Tranșările:**
(a) **`nou/tools/Import1C`** (consolă ca Migrare): baza DEDICATĂ
`Atlas.Conta.Import1C.Flax` migrată + seed-uită de unealtă pe profil Privat
(calea ModelCheck); `FlaxDb` pe view-urile `[flax]` cu SQL parametrizat
(agregare peste valute, chei hex `KeyField`); idempotență prin legături
`"1C:<view>"`; `MapeazaCont` = concatenarea segmentelor punctate
(`442.6`→`4426`) + tăierea ultimului segment (`121.25`→`121`) + dicționar
de override; `--sabotaj` = auto-testul permanent al contractului de
reconciliere (+1 leu pe rânduri scrise → FAIL + exit 1).
(b) **Nomenclatoare: mici integral, mari la cerere.** Depozite→Gestiune
(elemente; marcate-șterse = `Activ=false`), Casierii/ConturiBancare
proprii→ContPropriu (cont implicit 5311/5314, 5121/5124 sau ContDeEvidenta
mapat; `OwnerId_Organizatii` filtrează cele 46 proprii din 5.065),
PersoaneFizice→Angajat (CNP fără câmp — aditiv la cerință). Partenerii
(129k) și Nomenclator (312k, coduri NE-unice) NUMAI prin
`ImportLaCerere.AsiguraPartener/AsiguraProdus`: ObjectSpace propriu,
legătura scrisă DUPĂ commit, recuperare la rulare întreruptă = entitate cu
același cod NELEGATĂ de nicio legătură, cu re-aplicarea atributelor (fix
review: altfel omonimele se încrucișau); același mecanism de recuperare pe
cod și în `Upsert`-ul nomenclatoarelor mici (fix review).
(c) **Deschiderea 01.01.2025, scrisă direct (`DocumentId=null`, rescriere
integrală la fiecare rulare)**: solduri per cont contra ancorei 891 la
31.12.2024, FĂRĂ dimensiuni (fapt al sursei: BalantaNivel1 nu defalcă
terții la deschidere — stingerile 2025 pe sold global, modelul 34d);
reziduul de rotunjire al sursei pe propriul `891.` (−0,01) nu se scrie ca
rând 891/891 — ancora trebuie să-l REPRODUCĂ; extrabilanțierele se sar
raportat; overrides fixate: `132.`→4752, `4465`→446, `43111`→4316,
`43115`→4315, `117.2x`→1171, `211.3`→2111, `411.2`→4111; la deschidere
orice gaură = FAIL, nu avertisment. Profilul privat completat: Tipurile
3021/3028 (gradul II al lui 302 — derivările incremental le prind automat).
(d) **Stoc: Lot per (document creator × produs × SIMBOL cont)** — simbolul
în cheie e fix de review (46 perechi doc×produs pe >1 cont fuzionau marfă
371 cu materiale 3028); rânduri `RegistruStoc` per (lot × depozit) — soldul
Atlas trăiește per Lot×Repartitor, `Lot.Gestiune` = doar nașterea; Tipul
produsului multi-cont = contul DOMINANT pe valoare; TipStoc din simbolul
LOTULUI (MF→Marfuri, rest→Magazie — oglindește regulile private); data
lotului parsată din descrierea documentului creator (99,9%; fallback
31.12.2024). **Netarea retururilor-ca-lot** (2.606 celule negative, ~22%)
per produs×depozit: FIFO pro-rata cu rest pe ultimul lot + fixpoint
(backstop: citirea-înapoi refuză orice rând negativ); conservă EXACT
Σcantitate/Σvaloare per grupă; 124 grupe total-negative (−5.260,56 lei) +
4 poziții orfane (408,37 lei) = diferențe ale SURSEI, raportate nu ascunse
(34f). Loturile golite de netare SE CREEAZĂ cu prețul brut istoric
(pin-urile 1C-c le referă); doar cele integral în grupe sărite (260) nu.
(e) **Reconcilierea deschiderii (`Reconciliere.cs`)**: recitire INTEGRALĂ
din Postgres, independentă de structurile fazei de scriere; (1) sold per
cont OMFP agregat pe ambele părți (446.1/446.7/4465 colapsează legitim pe
446) — 55 simboluri, 0 diferențe; (2) ancora = reziduul sursei (−0,01);
(3) stoc per produs×gestiune pe REUNIUNEA cheilor, tradus la identitatea 1C
prin legăturile inversate — 8.283 chei, 0 nejustificate, 127 justificate cu
motiv. Per-LOT nu e țintă (netarea rearanjează deliberat în grupă — §8.3).
(f) **Datorie documentată pentru 1C-c** (review, defect 4): mecanica
MapeazaCont nu reduce analiticele FĂRĂ punct (43112 nu cade pe 4311) — pe
tot planul 1C: 36 coduri nerezolvabile (CAS/CASS angajator — salariile!,
6351, 4466, 2311, 4013/4043) + ~95 pe sumator (411., 602., 605., 623.11…);
azi pică zgomotos, la 1C-c `overrideCont` crește masiv — de bugetat ca
lucru la maparea §4, nu descoperit în mers. Verificat explicit: ZERO
colapsuri semantice tăcute pe cele 653 de coduri. Watch: perioada reziduală
3999-11 în Balanta (Σ=0, invizibilă la filtrarea pe lună); FCL pe 3021/3028
→ credit 708 prin fallback (mapa vânzării are doar 371/345/381 — de
confirmat la 1C-c); limitare asumată: unealta e single-operator pe bază
(două instanțe concurente = race pe legături, ca 25f).
