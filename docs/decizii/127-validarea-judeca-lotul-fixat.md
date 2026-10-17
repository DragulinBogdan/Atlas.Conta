# 127 — Validarea judecă lotul fixat, anularea PIF judecă AMO pe lună, produsul cu stoc nu-și schimbă clasa, lotul nu se naște negativ

- Data: 2026-10-17
- Stare: propusă
- Execuție: firul „rotund”, felia R1 (2026-10-17), branch `rotund-r1`; ModelCheck integral verde pe ambele profiluri
- Arii: motor, stoc, imobilizari, nomenclatoare
- Amendează: —
- Închide: S-r7, S-r8, F26-r18, NG-r2
- Docs: SC-FCT-17, SC-FCT-18 din `docs/nucleu/scenarii/FCT.md`; SC-CIT-116 din `docs/nucleu/scenarii/CITIRI.md`; proba IMO-R9 din `VerificaReviewF26`; `docs/stare-curenta/motor.md`, `nomenclatoare.md`, `imobilizari.md`

## Regula durabilă

**Dry-run-ul și operarea judecă același contract; dependențele unui eveniment de imobilizare se judecă la fel la operare și la retragere; contul lotului nu se mută sub stocul existent.**

(a) **Lotul născut se fixează înaintea refuzurilor.** Prețul de intrare și data lotului născut de document se pun în pregătirea comună a validării și a operării (`MotorOperare.PregatesteSiValideaza`), înaintea contractului. Validarea lucrează pe ObjectSpace-ul apelantului, pe care acesta îl aruncă; nu persistă nimic. Cantitatea nepozitivă (`CANTITATE_NEPOZITIVA`) și valoarea negativă (`LOT_VALOARE_NEGATIVA`, 126 f) se refuză la amândouă.

(b) **Rândul lunar AMO depinde numai de evenimentele lunilor anterioare.** Anularea și stornarea unui PIF (și dependenții AMO) consideră un rând de amortizare drept fapt ulterior numai dacă e dintr-o lună ulterioară evenimentului; celelalte fapte ale fișei rămân pe dată (`Data >=`).

(c) **Produsul cu stoc nu-și schimbă clasa sau contul.** Gardianul de editare refuză schimbarea `TipMaterialId` a unui produs cu cantitate nenulă pe vreun lot dintr-o gestiune, când tipul nou are altă clasă sau alt cont implicit. Între tipuri cu aceeași clasă și același cont schimbarea e liberă; produsul fără stoc își schimbă tipul liber.

## Context

Restanțele S-r7, S-r8, F26-r18 și NG-r2 erau `activă` (091) la inventarul din 126. S-r8: dry-run-ul nu fixa prețul și data lotului, deci contractul lui diferea de cel real pe `Unitate.Deschisa`; chiar operarea judeca refuzurile pe lotul nefixat. F26-r18: retragerea folosea `Data >=`, iar operarea „luna >”, deci refuza fals anularea unei modernizări din luna unei AMO operate. NG-r2 (consultarea „nod și găleată”): contul lotului vine din tipul curent al produsului; o schimbare de tip după recepție ar fi căutat soldul pe contul nou.

## Tranșări

- (a) în pregătirea comună, nu doar în `Valideaza`: altfel operarea însăși judeca refuzurile pe lot nefixat. Precedentul: `PregatesteOperare` modifică deja obiectele înaintea refuzurilor; 33d privește scrierile de registru, nu fixarea lotului.
- (c) gardian la editare, nu contul lotului citit din poziția lui în cub (precedentul 098 c la fișe): poziția lotului e deja în cub, dar cititorii operandului și ai stocului pornesc din tipul produsului; gardianul păstrează un singur loc al regulii. Stocul în gestiune, nu orice sold pe lot: poziția de consum pe cheltuială (126 d) rămâne pe lot și nu blochează.
- (b) pe felul rândului, nu pe tipul documentului: numai amortizarea are granularitate lunară.

## Ce rămâne deschis

Nimic nou. Liniile negative din Flax (325) sunt întrebări de migrare (091-r4).

## Istoric

- 2026-10-17: scrisă la felia R1 a firului „rotund”; propusă owner-ului.
