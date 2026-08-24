# Decizia 61 — Restanțele §Închidere F3/F4 golite

- **Data**: 2026-08-09 (primul commit în jurnal)
- **Stare**: activă
- **Rezumat durabil**: `CLAUDE.md` §61

---

**Restanțele §Închidere F3/F4 golite** (ModelCheck verde ambele profiluri;
probe live în browser pe baza de import). Tranșările:
(a) **D-6b închis integral: `DocumentSursaTip` pe ReadDto** (NIR/TRZ/DSC) —
rezolvat polimorf prin `ApiProiectii.CodTip` (mulțimea de un element a lui
`CoduriTip`, 60b; un query mărginit per citire, doar cu sursă nenulă);
clientul rutează TOATE link-urile „Generat din" prin `rutaTip` (NIR și
trezoreria nu mai hardcodează `/fct/`, DSC nu mai presupune 'FCL') — tip
fără felie de client = text, nu link mort; la transferul 581 nu mai rămâne
nimic de generalizat. Asserții ModelCheck pe toate cele 4 situri.
(b) **Etichetele liniilor NESALVATE (moștenit FCT)**: editorul de linie
CULEGE etichetele la selecție (`laSelectie` pe Produs/TipMaterial/Lot/
TipTva; eticheta Tipului precompletat vine din `$expand=TipMaterial` al
selecției de produs — zero fetch în plus, nimic inventat în TS) și le
întoarce prin `onSalveaza(linie, etichete)`; detaliul le ține per POZIȚIE
(paralel cu `agregat.Linii`), grila le folosește cu precedența
culese → server → gol (alegerea proaspătă a operatorului bate ReadDto-ul);
mor la orice re-seed dinspre server, inclusiv la salvarea documentului
existent (serverul poate reordona liniile — etichetele per poziție ar
minți). Valoare/ValoareTva/lotul FCT rămân exclusiv ale serverului.
FCT+FCL; TRZ neatins deliberat (etichetele lui = coduri de dimensiuni
opționale + precompletarea programatică TRZ, pe care `laSelectie` — doar
acțiuni ale omului, 56e — n-o vede).
(c) **Emitentul pe FCL existent**: sonda de existență promovată în nucleu
(`nucleu/sonda.ts: existaInSet`, extrasă din `LaturaContrapartida` F3 —
a doua utilizare a cerut extracția) decide: `PredatorId` în setul
`UnitateInterna` ⇒ lookup (editabil pe draft, rezolvat pe operat);
valoare istorică din afara setului SAU sondă nereușită ⇒ afișarea statică
din ReadDto — default-ul care nu minte.
