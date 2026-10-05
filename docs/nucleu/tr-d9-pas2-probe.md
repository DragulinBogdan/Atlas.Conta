# TR-D9a — pasul 2: gardul analizei pe mișcări și probele dinaintea tăierii

- Data: 2026-10-05
- Bază: `tr-d9-taierea` la `409054d`; contractul
  [`tr-d9-taierea-contract.md`](tr-d9-taierea-contract.md), D9-D15, rândul
  pasului 2, cu amendamentul 1.
- Ce conține: ce a intrat în cod, ce s-a măsurat, ce corectează inventarul
  pasului 1 și ce rămâne de hotărât. Așteptările numerice stau în catalogul
  de scenarii; aici sunt numai trimiterile.
- Regimul rămâne dual. Singura schimbare de comportament a pasului e
  schimbarea 5 din D9-D1.

## 1. Gardul analizei obligatorii (D9-D4, D9-A3)

**Forma.** `Module/Cub/GardAnaliza`: funcția pură `Lipsuri` și adaptorul
`Verifica`, chemat din `Materializare.Opereaza` și `Materializare.Refuzuri`
după acceptarea contractului. Textul refuzului e cel de azi
(`MotorOperare.VerificaLatura`), deci aserțiile existente nu se schimbă.

**Domeniul e structural, nu o listă de tipuri.** Gardul judecă mișcările
declarației (`Declaratie.Miscari`) din `Carte = Contabil`. `Contractare`
le întoarce lângă contract. Urmează de aici, fără nicio ramură pe tip:

| Ce produce declarantul | În domeniu | Tipuri azi |
|---|---|---|
| mișcări în cartea contabilă | da | BCS, DSC, LDI, NIR manual, FCT (contarea, taxa și recepția), FCL, DEC, PLT, INC, RDC, RLF, NTC, ITV, AMO, CAS, taxa DVI |
| mișcări în cartea fiscală | nu | baza DVI, amortizarea fiscală |
| mutări | nu | BTR, PIF |
| transformări, în amândouă felurile | nu | ASM |
| diferența NIR-ului conex acoperit | nu aici | rămâne la `ReceptiiConexe.VerificaAnaliza` |

Tabelul coincide cu §4 din inventar, cu recepția facturii inclusă (D9-A3).
BTR, ASM și PIF rămân nepăzite: D9-r1.

**Maparea dimensiunilor** e extrasă într-un singur loc
(`GardAnaliza.Dimensiuni`) și folosită de gard, de `ReceptiiConexe` și de
deschidere.

**Probele.**

| Proba | Ce dovedește | Rezultat |
|---|---|---|
| `GARD-ANALIZA`, 9 aserții pe funcția pură (`ProbeGardAnaliza`) | cele trei forme din D9-D4 (trece cu angajament și fără cod economic; refuză când lipsesc amândouă; trece cu cod economic explicit), cartea fiscală în afara domeniului, fiecare axă pe numele ei, un singur rând per linie și cont | verde, ambele profiluri |
| SC-FCT-12, 13, 14 (`ScenariiFct`, bugetar) | recepția facturii prin comandă, în aceleași trei forme; refuzul e atomic și nu naște NIR conex | verde |
| integrala bugetară cu amândouă gardurile | niciun document acceptat azi nu e refuzat de gardul nou | 3.459 OK, zero FAIL |
| aceeași integrală cu gardul vechi dezactivat local | gardul nou refuză singur tot ce refuza cel vechi, cu același text | 3.459 OK, zero FAIL |
| aceeași integrală cu amândouă dezactivate | scenele depind efectiv de gard | pică la prima scenă dependentă (FCT pe 404, defalcarea BFEPR) |

Ultimele două sunt rulări-diagnostic, cu modificările aruncate; logurile sunt
în `run-nucleu/tr-d9a/pas2/` (`g1`, `g2`, `g3`).

### Constatări care cer hotărârea owner-ului

| Id | Constatarea | Ce am făcut | Ce rămâne de hotărât |
|---|---|---|---|
| G1 | Flag-ul `Repartitor` nu refuză azi nimic pe documente. Nota planului vechi își ia repartitorul, în ultimă instanță, de pe latura antetului (`RepartitorImplicitDebit` / `Credit`), care nu e niciodată nulă. Maparea din D9-D4, „partenerul sau gestiunea postării", aplicată strict, ar refuza documente acceptate azi: cubul scoate partenerul de pe conturile fără rol de terț (B-D8 pct. 4), iar piciorul de terț nu poartă gestiune (B-D8 pct. 9). Pe seed-ul bugetar 16 din cele 20 de conturi care cer repartitor nu urmăresc partide (401.02.00, 462.01.09, 552.00.00, 437.x, 458.x, 774, 805.00.00 și altele) | gardul portat citește capătul, apoi latura documentului, ca azi. Pasul admite numai refuzuri echivalente. Funcția pură rămâne strictă când nu primește repartitorul documentului, și e probată așa | dacă `Repartitor` trebuie să devină strict pe postare. Ar fi comportament nou, în afara listei din D9-D1, și ar cere ca declaranții să păstreze partenerul pe aceste conturi |
| G2 | Flag-ul `Material`: nota veche ia materialul și din regula de contare (override sau comun); postarea poartă numai produsul lotului. Niciun cont din seed nu cere `Material`, pe niciun profil | maparea din D9-D4, neschimbată | nimic acum. Limită: o regulă a clientului cu material fix, pe un cont editat să ceară `Material`, ar fi refuzată de gardul nou și acceptată de cel vechi |

## 2. Măsurători care corectează inventarul pasului 1

Inventarul a dedus aceste rânduri din cod și le-a marcat *din cod*.

| Loc în inventar | Ce spunea | Ce s-a măsurat | Proba |
|---|---|---|---|
| §6, DSC pe bugetar | dry-run-ul trece; cu număr cules documentul rămâne Operat fără efect | dry-run și operare refuzate de frunză („nu are regulă de contare de cost"), cu sau fără număr; zero efecte | SC-DSC-09 |
| §6, DVI pe bugetar | de fixat la pasul 2 dacă profilul are tip de TVA de import | nu are (numai CAP0/11/19/21); operarea e refuzată întotdeauna de frunză. Calea „Operat fără efect" nu există | SC-DVI-12 |
| §6, RLF, RDC, BPR | cu număr cules: Operat fără efect | confirmat: Operat, zero postări, zero rânduri de registru | SC-RLF-14, SC-RDC-19, SC-X-25 |
| I5 | regula pe un tip fără contare prin reguli produce notă numai în registru | confirmat pe BTR; în plus, acoperirea registru → cub o semnalează deja azi (`CITIRE_ISTORIC_INCOMPLET`) | SC-X-26 |
| I7, §5 | returul pe lot inițial fără preț de intrare iese cu valoare 0 | confirmat: linia 0, postarea −1 cu valoare 0, lot 3/60 → 2/60 | SC-DES-22 |
| D9-D13, 098-r3 | după stornarea NIR-ului acoperit cu consum, o operație pe lot e refuzată azi de garda registrului | nimic nu e refuzat. Registrul lotului ajunge −4, apoi −10; consumurile următoare trec. Garda de sold a registrului rulează numai pe tipurile din afara cubului, care nu au reguli de stoc | SC-X-27 |

Urmarea pentru 098-r3: proba cerută de D9-D13 („refuzată azi, acceptată
după") nu are jumătatea de azi. Restanța se închide la tăiere cu proba
SC-X-27, care arată operația acceptată în ambele regimuri.

## 3. Valoarea liniilor (D9-D3)

Scrise înaintea codului, cu cifra de azi măsurată și cu cea de după tăiere.

| Sursa | Rânduri | Azi | După tăiere |
|---|---|---|---|
| (a) evaluată din sold | SC-BCS-15, SC-BCS-16 | linia din soldul registrului (3,33; 3,33; 3,34), postarea din soldul cubului (3,33; 3,34; 3,33) | linia = postarea |
| (a), ASM | SC-ASM-17 | consum pe linie −3,33; −3,33; −3,34; pe postări 3,33; 3,34; 3,33 | refuz până la redistribuire; rândurile 17–26 rescrise în [ASM](scenarii/ASM.md) |
| (b) prețul de intrare | SC-RLF-05, SC-RLF-08, SC-RLF-13, SC-RDC-07 | −0,33; −10,01; −10,00; −10,01; −20,00, aceleași după anulare și reoperare și prin corecție | neschimbat |

## 4. Aserțiile din `Program.cs` ale căror cifre se schimbă la pasul 6

Numite după check, fiindcă numerele de linie din inventar sunt de la `88b45a7`.

| Check | Azi | După tăiere |
|---|---|---|
| D18-V2 (a), dry-run = operare | dry-run-ul lasă pe linia din memorie 10,00, golirea planului vechi | dry-run-ul nu întoarce valori; pe linie rămâne estimarea 10,01. Aserția se mută pe refuzuri și pe valoarea de după operare |
| D18-V2 (h), consumul ASM după o operare refuzată | linia poartă −10,00, scrisă de planul vechi înaintea validării | linia poartă estimarea; valoarea finală se citește din evaluarea declarantului |
| NUC-BCS-N-R3-2 | motorul vechi scrie pe linie și pe notă 50 (5 × prețul înghețat 10) | linia ia decizia contractului, 75 |
| NUC-BCS-N-R3-3 | diferența 75 − 50 = 25 între contract și motorul vechi | rămâne numai 75 |

## 5. Contabilitatea probelor (D9-D7 e)

| Profil | Înainte (`409054d`) | După pasul 2 | Diferența |
|---|---|---|---|
| bugetar | 3.459 | 3.527 | +68 |
| privat | 4.771 | 4.835 | +64 |

Nicio aserție nu a fost ștearsă sau slăbită. Nucleul: 180 din 180.
Soluția, Import1C, Migrare și BackfillTva compilează.

## 6. Ce duce pasul 6 din probele de aici

- refuzul tipului care nu declară sau n-are politică pe profil, cu un singur
  cod (propus `TIP_FARA_DECLARATIE`): SC-RLF-12/14, SC-RDC-16/19, SC-DSC-09,
  SC-DVI-12, SC-X-25 își schimbă coloana de azi cu cea de după;
- refuzul regulii de contare fără consumator la editare (propus
  `REGULA_CONTARE_FARA_CONSUMATOR`), printr-un membru al `IDeclarant`:
  SC-X-26;
- valoarea liniei din decizia contractului: SC-BCS-15/16 și cele patru
  aserții din §4;
- ASM pe gardul P = C: tabelul din [ASM](scenarii/ASM.md).

Numele codurilor sunt propuneri; se fixează la pasul 6.
