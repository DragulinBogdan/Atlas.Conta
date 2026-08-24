# Decizia 48 — Analiza pre-1C-c — patru probleme tranșate ÎNAINTE de execuție

- **Data**: 2026-07-25 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §48
- **Docs**: docs/import/faza-1c-design.md

---

**Analiza pre-1C-c — patru probleme tranșate ÎNAINTE de execuție**
(design §12, toate confirmate 25.07.2026):
(a) **Pin vs. solduri netate = supapă de IMPORT, motorul neatins** (37d
intact): helper unic de realocare pentru TOATE ieșirile pe lot (BTR/BCS/
ASM/RLF/DSC) — pinul întâi, deficitul FIFO în produs×gestiune (grupa în
care netarea a conservat sumele), raportat. Contractul §8.3 amendat:
cantitate strictă peste tot; valoarea pe grupele netate = diferență
justificată („netare deschidere"), purtată înainte lună de lună.
(b) **Cele cinci mecanici §4 fixate**: extras per rând; imperecherile =
trecerea 2 per lună (skip+raport pe sold de deschidere/tipuri neimportate;
invariant picat pe date reale = raport, nu stop); Compensare = NTC +
**extensie de invariant ca decizie de produs** (NotaContabila operată
poate stinge; invariantul de trezorerie 31d reformulat: contrapartida
stinsă apare pe liniile ei explicite); imperecherea retururilor (total
negativ, 46f) = skip+raport; RVA = FCL „consumator final" + DSC pin, cu
**uniformizarea regulii 36a „ValoareTva culeasă nu se suprascrie" pe
FCT/FCL/DEC** (altfel rotunjirea pică contractul ITV); avizele = rețeta
generală de surogat (stoc prin DSC/LDI cu pin + contabil prin NTC
transcris exact).
(c) **Maparea conturilor: LISTĂ (CSV comentat), NU regulă mecanică** —
contra-exemplul 43111→4311 (corect: 4316/CASS) face tăierea de cifre
nesigură exact pe volum; descoperirea-în-mers se rezolvă cu **fază
pre-flight** care mătură toate codurile mișcărilor 2025 și emite triajul
unic (denumiri față-n față + sugestii neaplicate automat). Bonus scos de
amendament: `plan-conturi-omfp.csv` era TRUNCHIAT (921 rupt, fără
922–925/93/931/933) — reparat și verificat contra listei D406
(`anaf/plan_conturi_bal_soc_com.md`, DUKIntegrator); ModelCheck privat
verde.
(d) **Rularea**: idempotență per DOCUMENT (legătura în ACELAȘI commit cu
draftul; legat+Draft la resume → re-operare), buclă lunară documente →
imperecheri → **ITV în buclă** (relaxarea graniței 1C-c/1C-d: fără
închidere, contractul (1) pica lunar pe 4426/4427/4423; 1C-d rămâne
gate-ul — anul întreg + 4423/4424 pe 12 luni), stop dur implicit +
`--continua` cu diferențe purtate înainte, măsurătoare pe ianuarie
înainte de orice discuție de performanță.
