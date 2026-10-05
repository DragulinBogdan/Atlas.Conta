# TR-D9a — amendamentul 2 la contract

- Data: 2026-10-06
- Bază: `tr-d9-taierea` la `efd1c3c`; contractul
  [`tr-d9-taierea-contract.md`](tr-d9-taierea-contract.md), constatările G1 și
  G2 din [`tr-d9-pas2-probe.md`](tr-d9-pas2-probe.md), §1.
- Stare: aprobat de owner (2026-10-06, în chat), în forma îngustă: partenerul
  numai pe piciorul de terț al conturilor care cer repartitor. Transcris în
  contract, secțiunea „Amendamentul 2".
- Respinsă: partenerul pe toate postările documentului cu terț. Ar schimba
  sensul coordonatei din „al cui e soldul" în „cu cine s-a întâmplat
  evenimentul" și ar strica soldul pe partener al conturilor de stoc și de
  trezorerie. Partenerul evenimentului rămâne citibil prin document.
  Partenerul și pe conturile de cheltuieli și venituri e candidat pentru
  TR-D9b, cu scenariile lui.

## D9-A10 — G1: repartitorul e pe postare, gardul e strict (pas nou, 2c)

**Ce e azi.** Flag-ul `Repartitor` al unui cont nu refuză nimic pe documente.
Nota planului vechi își ia repartitorul, în ultimă instanță, de pe latura
antetului, care nu e niciodată nulă. Cubul poartă partenerul numai pe
conturile care urmăresc partide (`Partide.CuPartida`) și pe postările
fiscale (B-D8 pct. 4), iar piciorul de terț nu poartă gestiune (pct. 9). Pe
seed-ul bugetar, 16 din cele 20 de conturi care cer repartitor nu urmăresc
partide. Gardul portat la pasul 2 trece de aceea prin latura documentului,
ca nota veche.

**Ce urmează din asta, netratat.** După tăiere, pe aceste conturi cubul nu
mai poartă deloc repartitorul pe care registrul îl purta. Analiza pe
repartitor a unui cont care o cere obligatoriu s-ar pierde odată cu
registrul.

**Regula.**

- (a) Pe o postare din cartea contabilă, pe un cont cu flag-ul `Repartitor`,
  declarantul pune repartitorul pe capăt după sensul piciorului, ca în restul
  cubului: piciorul intern poartă `Gestiune`, cum o poartă deja; piciorul de
  terț poartă `Partener` = latura externă a documentului, și când contul nu
  urmărește partide. Convenția pozițională a notei vechi (debit ← predator,
  credit ← primitor) NU se reproduce.
- (b) Coordonata nu deschide partidă și nu schimbă unitatea postării.
  Urmărirea partidelor rămâne pe `UrmarestePartide`.
- (c) Gardul analizei judecă numai capătul: `Partener` sau `Gestiune`. Citirea
  laturii documentului iese din `GardAnaliza.Verifica`.
- (d) B-D8 pct. 4 se amendează: `Partener` rămâne și pe conturile cu flag-ul
  `Repartitor`.
- (e) Domeniul e cel al gardului (D9-D4): mișcările din cartea contabilă.
  Mutările și transformările rămân în afară, sub D9-r1.

**Schimbarea de comportament** (a opta în D9-D1): postările de pe conturile
cu flag-ul `Repartitor` capătă coordonata. Pe seed, niciun document acceptat
azi nu trebuie să ajungă refuzat. Un document cu latura goală nu există azi,
deci gardul strict refuză efectiv numai un declarant care uită coordonata.

**De măsurat înaintea codului**, cu cifrele scrise în catalog:

1. rândurile de catalog ale căror postări capătă coordonata, pe ambele
   profiluri;
2. cititorii care grupează pe `Postare.Partener` sau pe `Gestiune`: soldul pe
   parteneri, balanța analitică, fișa contului, SAF-T. Pentru fiecare, cifra
   de azi și cea de după;
3. invarianții nucleului și `INV-CUB` care presupun „partener numai cu
   partidă" sau „gestiune numai cu stoc";
4. oracolul registre → cub, cât mai trăiește: nota veche poartă pe
   piciorul de terț latura pozițională, nu partenerul, deci coordonata nouă
   nu are geamăn în registru. Diferența se declară cu nume în normalizările
   oracolului și moare cu el la pasul 6.

**Unde intră.** Pas nou 2c, sub regimul dual, făcut de main, înaintea pasului
5b. Proba coordonatei e catalogul, nu oracolul. Cele 17 aserții din clasa
„repartitorul de pe latura notei" (inventar, §13) se rescriu în acest pas;
pasul 3 le lasă neatinse.

**Oprire înaintea termenului.** Un document de catalog acceptat azi ajunge
refuzat; o cifră a unui cititor se schimbă altfel decât prin apariția
coordonatei; un invariant al nucleului interzice coordonata fără unitate.

## D9-A11 — G2: materialul din regulă nu ajunge pe postare (limită acceptată)

Gardul rămâne cu maparea din D9-D4: `Material` = produsul postării. Limita se
consemnează în decizia 110: o regulă de contare a clientului cu material fix,
pe un cont editat să ceară `Material`, e refuzată de gardul nou și era
acceptată de cel vechi. Niciun cont din seed nu cere `Material`, pe niciun
profil.

## Ce s-a schimbat în textul contractului

| Loc | Schimbarea |
|---|---|
| D9-D1 | schimbarea 8: repartitorul pe postare, gardul strict |
| D9-D4 | maparea `Repartitor`, fără latura documentului; trimiterea la B-D8 pct. 4 amendat |
| D9-D15 | pasul 2c în tabel; regula de oprire primește probele lui |
| D9-D13 | 64h, 86-r13, F27-r11: proba pe `Partener` vine din pasul 2c |
| decizia 110 | limita din D9-A11 |
