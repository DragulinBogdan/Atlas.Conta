# E4 — migrarea din 1C: deschiderea la o graniță de lună, lunile de după prin motor, diferențele cu nume

- Tip: contract
- Data: 2026-10-17
- Trigger: owner, deschiderea etapei migrării după „rotund” (2026-10-17; 091 f, 130 b)
- Livrează: F-IMP-01, F-IMP-02 (etapa E4 din [scop](../scop.md))
- Arii: import, partide, imobilizari, stoc, perioade, saft, verificare
- Decizii: 018, 021, 034, 091, 107, 115, 116, 124, 130, 131
- Stare: deschis

Conectorul 1C (`nou/tools/Import1C`) a stat înghețat de la 091 (f), cu
excepția feliei M1 (107). Pe modelul rotund el compilează și rulează: pe
ianuarie 2025, cu bază recreată, deschiderea e exactă, iar luna se importă
fără eșec, dar contractele 1 și 5 pică (măsurătoarea de plecare de mai jos).
Granița e fixată în cod la 01.01.2025, fișele de imobilizări nu se deschid,
iar anul nu s-a rulat integral pe cub.

## Scop

1. **Deschiderea unei baze din 1C la o graniță de lună aleasă**: conturile,
   stocul pe lot, terții pe partidă (107) și imobilizările cu valoarea și
   amortizarea cumulată (F26-r13), prin `Materializare.Deschide` și PIF-ul
   de deschidere, o dată per bază. Granița devine parametru.
2. **Lunile de după graniță, importate prin motor și reconciliate lunar**
   cu sursa în cele cinci contracte. O diferență fie are nume (categorie,
   sumă, restanță sau decizie), fie e eșec al lunii.
3. **Anul 2025 integral pe Flax** (12/12), cu închiderea exercițiului
   (124) pe baza de import și soldurile ei de la 01.01.2026 comparate cu
   1C.
4. **Proba de go-live**: o bază nouă, deschisă din 1C la 01.01.2026, cu
   ianuarie 2026 importat peste ea.
5. **Rejudecarea restanțelor `migrare`** (130 c): la închiderea etapei,
   niciuna nu mai are starea `migrare`.

## Ce NU intră

- Istoricul dinaintea graniței: rămâne în 1C (NT1, 18, 107).
- Un ecran de migrare în XAF: unealta rămâne consolă, operată de
  implementator (E4-D5).
- Corecțiile de motor descoperite pe date: devin scenariu nou plus decizie
  (091-r5), nu cârpeală în conector. Etapa nu atinge `Module` decât prin
  felia acelei decizii.
- Faptul datat (131): parametrii fișei se deschid în forma de azi, tolerată
  de 131 (b). Mutarea lor pe faptul datat e a primului consumator (131-r2).
- Valuta (0 pe Flax), alte surse decât 1C, alte societăți decât Flax.
- Valoarea de produs a `PoliticaTva.TolerantaTaxa` (S-r1): e politică a
  clientului, nu condiție a migrării.

## Măsurătoarea de plecare (2026-10-17, main `6f6adee4`)

`Import1C --recreeaza --pana-la 1` pe 5446, bază recreată. Ieșirea e în
`run-verificari/migrare-m0/` (neversionat).

| Ce | Rezultat |
|---|---|
| Durata | deschiderea 0:26, ianuarie 5:38 (9.225 de unități, 15.232 de documente, ~22 ms pe document), contractul lunar 0:07 |
| Deschiderea la 01.01.2025 | exactă: 55 de conturi, 62 de controale (cont, latură), 3.967 partide, 6.817 loturi, 0 diferențe; INV-CUB verde |
| Import | 0 eșecuri, 15 sărite, toate cu motiv; idempotența 15.232 = 15.232 |
| Contractul 1 | FAIL pe 371 (−9.674,37), 3028 (+9.316,01), 303 (+358,36): suma e 0,00, adică reclasificarea 371 → 3028/303 apare de două ori (110-r2) |
| Contractele 2, 3, 4 | verzi (ITV 2/2, 15.564 de chei de stoc, deriva −0,08) |
| Contractul 5 | FAIL: 5 partide fără explicație din 3.967 (4 pe 4111, 1 pe 401; 4 marcate „mișcată direct la operare”, 107-r9); 134 neatinse de trecerea 2, declarate (Σ −255.317,47) |
| INV-CUB | verde pe baza integrală |

Sursa (`EServicesFlx`, schema `flax`) are note contabile până la
31.07.2026 și `BalantaNivel3` lunar până la 2026-06. Pre-flight-ul
avertizează că două view-uri regenerate (`DiminuareStocDeMarfuri`,
`Stornare`) nu mai expun coloanele cerute. Pe 2025 nu apar, dar trebuie
reverificate pe 2026 (pasul 4).

## D-uri

### E4-D1 — migrarea înseamnă trecerea la o graniță, iar lunile de după sunt rulare paralelă (owner, de confirmat)

Produsul migrării e baza clientului deschisă la granița aleasă. Lunile
importate după graniță au două roluri. La un client real, ele sunt rularea
paralelă: clientul încă lucrează în 1C, iar baza se ține la zi până la
trecere. Pe Flax, ele sunt proba conectorului și a motorului pe date
reale. Un document importat e operat de motor, nu copiat (45a). Contractele
compară baza cu sursa, nu o corectează (21, 34f).

### E4-D2 — granița e parametru

`--deschidere-la AAAA-LL` (implicit 2025-01) fixează data deschiderii,
anul pre-flight-ului, perioadele create și fereastra lunilor
(`--pana-la` numără de la graniță). Constanta 2025 și literalele de an din
conector ies, cu excepția textelor de raport care citează o cifră
măsurată. Partida inițială și lotul inițial se citesc din
`BalantaNivel3` pe `Period` = granița, ca în M1.

### E4-D3 — imobilizările se deschid prin PIF-ul de deschidere (F26-r13)

Fiecare fișă activă în 1C la graniță devine o imobilizare cu PIF de
deschidere: valoarea de intrare, amortizarea cumulată contabilă și
fiscală, durata rămasă, metoda, contul și locul. Sursa e
`InfoRg_InformatiiInitialeMF`, `InfoRg_ParametriiDeAmortizareMF`,
`InfoRg_StareaMFOrganizatii`, `InfoRg_ConturiMF` și `Balanta` pe 21x/28x.
Recensământul se face la pasul 2. Controlul e Σ fișelor = soldul 1C pe
fiecare cont 2xx și 28xx de la graniță. Diferența se raportează, nu se
absoarbe. Canarii 131 (f) se aplică: fără postare de zero nouă, fără
coordonată neaditivă nouă și fără scriere nouă a motorului pe nomenclator.
Forma de azi a fișei se folosește așa cum e.

### E4-D4 — o diferență are nume sau e eșec

Contractele 1–5 rămân cele din `faza-1c-design.md` §8, §16 și
`m1-deschidere-terti-contract.md` M1-D9. Fiecare lună se încheie cu zero
diferențe fără explicație. O explicație e o categorie numită în raport,
cu număr și sumă, legată de o restanță, de o decizie sau de un fapt al
sursei raportat (34f). Categoriile noi trec prin pasul în care apar și se
numesc în tabelul „Categorii” de la închidere. Toleranța rămâne 0,005.
Conectorul nu normalizează ce face motorul (091-r5).

### E4-D5 — unealta rămâne consolă

Migrarea o face implementatorul, o dată per client, pe o bază dedicată
care devine baza clientului. Nu primește ecran XAF, ușă HTTP sau rol.
Ieșirea ei e raportul pe disc și baza.

### E4-D6 — partenerul generic la trecere (107-r5, owner, de confirmat)

Pozițiile fără partener pe conturi urmărite intră pe `MIGRARE-NEDEFINIT`
(107 d). Propunerea: la trecere, raportul deschiderii listează pozițiile
partenerului generic cu cont, document și sumă. Contabilul le rezolvă
după trecere, cu NTC de reclasificare pe partenerul real. Partenerul
generic are `NuIncludeInDec394` și nu intră în D394. În SAF-T apare ca
partener propriu, cu identificatorul lui. Pasul 4 probează lista și o
reclasificare pe baza de go-live.

### E4-D7 — reproductibilitatea

Două importuri ale aceluiași binar, pe aceeași fereastră, dau același
raport de reconciliere, aceleași legături și aceleași partide pe chei
naturale (116). Se probează pe ianuarie–martie 2025 la pasul 3, cu
ordinea stingerilor dintr-o compensare fixată cronologic (107-r7).

## Restanțe

Triajul pe modelul rotund (130 c). Starea din registru se schimbă în
același commit cu contractul.

| Id | Verdict | Pas |
|---|---|---|
| 091-r4 | activă: etapa însăși; se închide prin decizia etapei | 5 |
| 110-r2 | închisă prin 132 (a): puntea reclasificării cade pentru ASM-ul care postează | 1 |
| 107-r9 | închisă prin 132 (b), (e): slotul gol numește poziția fără document; restul se atribuie pe document | 1 |
| 109-r3 | închisă prin 132 (d): plafonare la rest, cu excedentul raportat | 1 |
| TR-r2 | închisă prin 132: pe ianuarie, notele pe 371 fără lot sunt toate în categoriile punții | 1 |
| 107-r7 | activă: ordinea cronologică a stingerilor (E4-D7) | 3 |
| 116-r2 | activă numai dacă proba E4-D7 arată o diferență; altfel trece în „după PoC” | 3 |
| F26-r13 | închisă prin 133 (a), (b), (e)–(g): fișele la deschidere, AMO de motor, CAS pe fișă; ITV-ul lui martie generat | 2 |
| 107-r8 | activă: rotunjirea la bani a notei-punte, cu divergența declarată; blochează iunie | 3 |
| 107-r10 | închisă prin 132 (c), mutată la pasul 1: o partidă a contractului 5 pe ianuarie | 1 |
| 107-r1 | activă: stingerile prin facturi intră în trecerea 2 sau rămân categorie declarată, cu sumă | 3 |
| S-r5 | activă: plafonările de plată devin categorie a contractului 5 | 3 |
| E3-r6 | activă: IRZ pe decembrie 2025 și exercițiul închis pe baza de import | 3 |
| FZ-r3 | activă: lookup-ul per partidă măsurat pe volumul anului | 3 |
| 107-r5 | activă: E4-D6 | 4 |
| 107-r2 | activă: rândurile de terț fără partener, judecate odată cu E4-D6 | 4 |
| 73-r9, 74-r3, 74-r15, 86-r9 | active: SAF-T L pe decembrie 2025, validat DUK pe baza de import; datele sursei se raportează ca atenționări | 4 |
| TR-r12 | activă: se închide la pasul 5; cubul e singurul registru (TR-D9, 131 d), iar contractele 1 și 3 judecă aceeași valoare | 5 |
| 091-r5 | activă: regula trece în decizia etapei | 5 |
| 75-r3 | după PoC: reluarea parțială rămâne limită scrisă; etapa rulează integral | — |
| F27-r6 | după PoC: constatările de închidere pe documentele neimportate nu blochează trecerea | — |
| S-r1 | după PoC: politica clientului (vezi „Ce NU intră”) | — |

Restanțele noi ale etapei se numesc `E4-rN`.

## Pași

| Pas | Conținut | Probă | Stare |
|---|---|---|---|
| 0 | contractul, triajul, scopul E4, rândurile din catalog; owner pe E4-D1, E4-D6 și pe triaj | docskit verde | propus 2026-10-17 |
| 1 | ianuarie 2025 verde: 110-r2, 107-r9, 107-r10, 109-r3, TR-r2 | `--recreeaza --pana-la 1`: contractele 1–5 fără diferență fără nume, INV-CUB | închis 2026-10-17 (132) |
| 2 | fișele la deschidere (E4-D3), recensământul imobilizărilor | controlul pe 2xx/28xx; ianuarie–martie 2025 verde, cu ITV-ul lui martie | închis 2026-10-17 (133) |
| 3 | anul 2025 12/12; IRZ pe decembrie, exercițiul închis, soldurile de la 01.01.2026 față de `BalantaNivel3`; E4-D7; FZ-r3 | rularea integrală detașată; raportul cu tabelul „Categorii”; două importuri ale lunilor 1–3 identice | — |
| 4 | granița parametru (E4-D2); bază nouă la 01.01.2026 + ianuarie 2026; E4-D6; SAF-T L pe decembrie 2025 | deschiderea exactă la 2026-01; ianuarie 2026 cu contractele 1–5; DUK `ok` | — |
| 5 | review advers, decizia etapei, triajul final, starea curentă, catalogul | docskit; ModelCheck pe ambele profiluri, dacă `Module` s-a schimbat | — |

## Tranșări la pasul 1 (2026-10-17)

Regulile sunt în decizia 132. Ianuarie 2025, bază recreată: „CONTRACT
ÎNDEPLINIT”, 0 eșecuri, 6:14 în total.

| Contract | După pasul 1 |
|---|---|
| 1 | verde: puntea reclasificării nu mai dublează ASM-ul (132 a) |
| 5 | 3.967 partide, 0 FAIL: 1.497 stinse integral, 14 explicate de refuzuri, 31 de plafonare, 128 neatinse de trecerea 2 (Σ cub −258.298,19), 2 explicate pe document (o notă pe FIFO, Σ −60,00, E4-r1; un rând de retur la furnizor ținut pe partida documentului) |
| Împerecheri | 2.590 create; 38 plafonate pe documente (excedent Σ 9.417,08, 132 d) |

E4-r1 nu cere schimbare de motor la pasul 1, fiindcă contractul 5 o numește
(regula de oprire 1 nu se declanșează). Rămâne „după PoC”: e o notă de 60
de lei care creditează facturi din 2024 cu sold zero la graniță.

## Tranșări la pasul 2 (2026-10-17)

Regulile sunt în decizia 133. Ianuarie–martie 2025, bază recreată:
„CONTRACT ÎNDEPLINIT”, 0 eșecuri, 20:26 în total (ianuarie 5:37,
februarie 6:33, martie 7:35).

| Ce | După pasul 2 |
|---|---|
| Recensământul la 01.01.2025 | 92 de fișe (Σ valoare 3.805.511,88, Σ amortizare 949.551,12, 25 amortizate integral); 26 de solduri fără fișă, cu motiv; 5 tipuri gemene pentru perechile 1C (activ, amortizare) |
| Controlul pe 2xx/28x | Balanța = fișe + fără fișă + fără subconto MF, pe fiecare cont; pe 2814, 2.179,98 sold debitor fără subconto MF, Δ declarat la deschidere (133 b) |
| Lunile | 4 PIF-uri în ianuarie, AMO în fiecare lună, CAS-ul din 10.03 cu divergența de ieșire 0,00; divergența de cotă Σ \|Δ\| 11,02 / 22,00 / 33,15, cumulată |
| Contractul 2 | ITV generat în fiecare lună, inclusiv martie |
| Contractul 5, februarie | 0 FAIL: 8 × 107-r1 (Σ −2.445,56), 2 × N-D7 (Σ 331,43), 3 × E4-r1 (Σ −5,68), 2 × 107h (Σ −667,13) |
| Contractul 5, martie | 0 FAIL: 3 × 107h (Σ 502,69), 6 × E4-r1 (Σ −62,44) |
| Canarii 131 (f) | 0 postări de zero în bază; fără coordonată nouă, fără scriere a motorului pe nomenclator |

Februarie și martie n-au fost măsurate la pasul 0. Primele lor rulări au
scos trei clase de diferențe, niciuna pe imobilizări: reziduul T-r2 pe
6588/7588 (133 d), netarea 1C a unei poziții de sens opus în plată
(133 c) și notele cu partida numită peste rest sau lăsată pe FIFO
(133 h). Toate trei sunt numiri în conector. Regula de oprire 1 nu se
declanșează.

## Regula de oprire

Lucrul se oprește și revine la owner când:

1. o diferență poate fi închisă numai printr-o schimbare de motor, adică
   printr-un scenariu nou care nu e o variantă a unuia din catalog (091-r5);
2. deschiderea fișelor ar cere o postare de zero, o coordonată neaditivă
   sau o scriere a motorului pe nomenclator (131 f);
3. o lună importată durează peste 10 minute sau anul peste 2 ore;
   atunci se profilează înainte de rularea următoare;
4. 1C nu poate da o cifră de control pentru o clasă de solduri de la
   graniță (de exemplu fișele fără sold pe 2xx);
5. un pas consumă de două ori estimarea lui fără probă verde.

## Gata când

- Deschiderea e exactă la 2025-01 și la 2026-01, inclusiv fișele, cu
  INV-CUB verde.
- Lunile 2025-01…12 și 2026-01 se importă fără eșec, iar contractele 1–5
  nu lasă nicio diferență fără nume.
- Exercițiul 2025 e închis pe baza de import, iar soldurile ei de la
  01.01.2026 se potrivesc pe cont cu `BalantaNivel3` 1C, cu diferențele
  numite.
- SAF-T L pe decembrie 2025 trece DUK.
- Niciun rând din registru nu mai are starea `migrare`.
- Review-ul advers e închis, iar decizia etapei e scrisă.
