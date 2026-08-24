# Decizia 52 — Felia 1C-d-final

- **Data**: 2026-07-28 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §52
- **Docs**: docs/import/faza-1c-design.md

---

**Felia 1C-d-final — executată; anul 2025 integral prin motor cu
CONTRACT ÎNDEPLINIT pe toate cele 4 contracte × 12 luni** (187.317
documente + 17.814 copii, 0 eșecuri, 186 skip-uri 100% itemizate,
66.553 chei stoc la decembrie cu 0 nejustificate, TVA 6,81M la cent) —
mai strâns decât gate-ul 50 (care avea 1 cheie reziduală). Lotul 50e,
G3, 51c și validarea BTR-cost închise; faza 1C e ÎNCHISĂ. Tranșările:
(a) **51c implementată**: `SetareProfil` (primul rând de setare per
bază: Profil + `MidpointRounding RotunjireBani`), scris de seed și
ÎNGHEȚAT (profil sau convenție diferită = refuz; appsettings
`ConventieRotunjire` doar la prima seed-uire); `Scara.FixeazaConventia`
(o dată per proces, valoare diferită = throw) + bootstrap per host
(Updater/Blazor best-effort/ModelCheck/Import1C/Migrare).
`RotunjestePret` rămâne AwayFromZero FIX — convenția e a banilor
POSTAȚI, nu a evaluării (prețul de lot nu se schimbă cu profilul).
(b) **G3 executat**: cele 27 de Tipuri ad-hoc în seed explicit
ProfilPrivat — clasă nouă `TER` („Terți și regularizări",
Natura=Serviciu DELIBERAT: regulile de contare se potrivesc pe natură,
Tehnica le-ar scoate din joc), cheltuieli în C, utilități în S,
venituri în VEN, `S371` cu ContImplicit=371 explicit; upsert pe Cod
(bazele importate se corectează); Check-uri noi în ModelCheck; anul
rulează cu ZERO „gaură de profil completată ad-hoc".
(c) **D1 tranșat pe structura reală, nu pe schița review-ului**:
șablonul BCS „plan per grup" NU se poate replica pe FCL/RVA/AVE/RLF/ASM
(puntea și clasificarea sunt per DOCUMENT) — unitatea cu componente de
STOC deja operate se REFUZĂ zgomotos cu remediul `--deblocheaza`;
nuanța din review: cheile legate cu document DRAFT lângă frați operați
NU se refuză și NU se replanifică — se RE-operează prin fluxul normal
(plan nul); gard pe copii: DSC/#btr se importă doar cu sursa Operată.
(d) **D5**: purjarea divergențelor legată de DAREA ÎNAPOI — UitaSursa
în aceeași tranzacție cu ștergerea draftului (hook `inainteDeCommit`),
legătura orfană purjează și divergențele sursei; punctul „purjare la
ORICE replanificare" a fost implementat, MĂSURAT și RESPINS (documentele
blocate se replanifică fără să dea nimic înapoi — rândul vechi e faptul;
o a doua trecere pierdea o înregistrare și pica contractul pe o cheie
adevărată).
(e) **D2/D3/D4**: negativul sursei mărginit pe cheia EXACTĂ
produs×depozit, per AXĂ (fiecare axă doar celulele negative pe ea);
plafonul contractului (1) = suma SEMNATĂ a justificărilor contractului
(3) defalcate per cont pe ambele părți (produs→Tip→cont / celulele
sursei mapate), cu potrivire de semn cerută și oglinda pe CONTRAPARTIDĂ
(semn inversat) — circularitatea „delta brută își e propriul plafon" a
murit; axa de valoare (mulțimea marcată) raportată lună de lună; deriva
de rotunjire = CONTRACTUL 4 (blocant), prag = max(podea statistică,
convenția din Scara: AwayFromZero → N_cum×0,005, ToEven → 3σ√N_cum) pe
midpoint-urile CUMULATE, numărate DOAR la materializarea prin motor și
persistate per lună (`1C:Midpoint`, MAX — determinism la reluare).
(f) **D6**: sabotajul = două probe în `Sabotaj.cs` (partial class peste
ReconciliereLuna — proba nu se mai poate despărți de contract): proba
contabilă pe rând cu ambele conturi CURATE (Δ≈0 pre-probă) și în afara
mulțimii plafonabile DERIVATE din politici (fără prefixe); proba de stoc
pe cheie care se închide exact, în afara tuturor categoriilor; verdictul
cere AMBELE detectate (proba scăpată = exit propriu, numită); (2)/(4)
nesabotate cu motivul scris în cod.
(g) **Re-validarea a scos și închis două defecte de MĂSURĂTOARE** (trei
rulări integrale, diagnoze read-only pe date): ramura „preț de lot" din
`StocDinNota.Masoara` A MURIT — pe notele care mută stoc măsurătoarea e
uniform negarea mișcării sursei (−cantitate1C, −valoare1C): prețul
lotului e STARE, nu mișcare, iar 1C își reconciliază singur diferența
de evaluare prin rândul-pereche 607=371 fără cantitate (evaluarea
noastră o număra de DOUĂ ori; probă pe an: 326 chei exacte numai cu
negarea, 0 numai cu prețul de lot); hunk-ul BTR-cost era INSUFICIENT,
nu greșit — delta de cost per depozit e PER BUCATĂ și călătorește cu
marfa: `Evaluare.Masoara` emite acum contrapartida EfectStoc la
ieșirile cu perechea evaluată (DSC/BCS/LDI−/RLF/RDC, fără conturi —
axa contabilă e a punții „Evaluare"), iar cheia −0,69 din decizia 50 e
ÎNCHISĂ măsurat.
(h) Rămase semnalate, ne-blocante: consumul ASM rămâne pe poarta de
proveniență (mutarea pe măsurătoare-la-ieșire = rafinare viitoare a
axei de valoare D3); un reziduu TRANZIT (−106,24, fuziune de loturi)
trece prin poarta de axă-valoare, nu prin egalitate; observațiile
ne-blocante din 49f (predicatul #inc, casieriile pe simbol, O(n²) în
Persista — fixat doar HashSet-ul). Următorul pas al roadmap-ului:
GATE XAF (44.2).
