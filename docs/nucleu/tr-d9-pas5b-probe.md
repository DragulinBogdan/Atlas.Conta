# TR-D9a — pasul 5b: probele dinaintea tăierii (D9-A1)

- Data: 2026-10-06
- Bază: `tr-d9-taierea` la `417af66`; contractul
  [`tr-d9-taierea-contract.md`](tr-d9-taierea-contract.md), D9-D15, rândul
  pasului 5b; textul întreg în
  [`tr-d9-taierea-amendament-1.md`](tr-d9-taierea-amendament-1.md), D9-A1.
- Ce conține: ultima reconciliere registre ↔ cub cu fiecare diferență
  explicată, `PerfCub` în regim dual ca termen A al comparației de după
  tăiere, scara de volum și cifra pentru D9-D10 (b).
- Fără cod de produs. Singurul cod e al uneltei: modul `ModelCheck
  --scara-volum`.
- Dovezile brute stau în `run-verificari/` și `run-nucleu/tr-d9a/pas5b/`,
  care nu sunt versionate. Cifrele care trebuie să supraviețuiască tăierii
  sunt transcrise aici.

## 1. Ultima reconciliere registre ↔ cub

**Baza.** `Atlas.Conta.Import1C.Flax.R3f`: ianuarie 2025, importată la
2026-10-05 cu codul 109-R1. D9-A1 numea `.Flax.M1s`; baza aceea e dinaintea
deciziei 109 și nu corespunde codului, deci nu poate fi proba (precizarea
owner-ului din 2026-10-06, transcrisă în amendament). Măsurat pe amândouă:

| | `.Flax.R3f` | `.Flax.M1s` |
|---|---|---|
| linii cu taxa marcată (`TvaCules`) | 7.242 | 0 |
| linii cu taxa postată diferită de taxa liniei | 0 | 497 |

`ModelCheck --reconciliere-cub` pe `.Flax.R3f`: exit 1, șase rânduri Δ.
Baza e citită fără scriere; numărătorile dinainte și de după sunt identice
(82.144 de postări, 18.727 de tranzacții, 24.459 / 29.660 / 7.242 rânduri în
registrul contabil, de stoc și de TVA). Arhiva:
`run-verificari/d9-pas5b-reconciliere/`, cu `explicatii.md`. Prima rulare,
pe `.Flax.M1s`, e păstrată în subdirectorul `m1s/` ca martor.

| Clasă | Rânduri | Δ (cub − registre) | Explicată de |
|---|---|---|---|
| rolul `Autocolectare` fără rând în `RegistruTva`, litera (c) | 2 | FCT +386.843,70; RLF −9.134,98 | 103 (e): achiziția cu taxare inversă are în cub și faptul contrapărții; registrul are un singur rând pe linie |
| evaluarea ieșirii, litera (a) | 4 | BCS −0,01 de două ori; DSC −5,02 de două ori | T-D4.2, N-r3, ASM-B7: preț înghețat în registru, raport curent în cub |
| (h) ASM, în afara exit-ului | 3 | D 3028 9.316,01; D 303 358,36; C 371 9.674,37 | D8-B4 / T-r15 |
| (b), (d), (e), (g) | 0 | — | — |
| (f) partide | 0, vacuă | — | baza n-are nicio perioadă închisă |

Clasa taxei pe linie față de taxa pe document × cotă (11 rânduri pe
`.Flax.M1s`) a dispărut pe `.Flax.R3f`: taxa din cub e egală cu
`RegistruTva` pe toate cele 4.927 de perechi document × cotă. Ținea de bază,
nu de cod.

Diagnosticul valoric (ASM-B7) semnalează 6.961 de poziții din 15.888:
6.776 numai prin data deschiderii (2024-12-31 în registru, 2025-01-01 în
cub; 107), 130 cu valoare fără cantitate rămasă numai în registru
(−6.267,19; 107) și 55 din evaluarea ieșirii (+5,03).

**Nicio clasă nouă, neexplicată.** Verificat independent de main:
numărătorile, suma rolului `Autocolectare` pe cele două tipuri și soldurile
de cont de mai jos; cele trei cauze recitite în documentele citate.

### TR-r12

Cifra istorică, +585.404,66 pe 3xx, e a unei baze Flax de an întreg care nu
mai există și nu se poate reproduce. Pe ianuarie 2025:

| | Registrul contabil | Registrul de stoc | Cub |
|---|---|---|---|
| sold 3xx | 9.550.041,90 | 9.564.233,35 | 9.550.046,93 |

Registrul de stoc depășește registrul contabil cu 14.191,45, tot pe 371; din
ei 6.267,19 sunt valoarea fără cantitate de la deschidere. Soldul cubului e
al registrului contabil plus 5,03 (evaluarea ieșirii). Partea cu lot a
cubului e 9.557.971,19, iar postările 3xx fără unitate însumează −7.924,26
și sunt toate ale notelor contabile.

### Constatări ale reconcilierii, pentru owner

- **A-1. Reclasificarea 371 → 3028 / 303 e de două ori pe cont în cub.**
  ASM-ul `SED#-R` o postează pe loturi (9.316,01 + 358,36), iar nota-punte a
  conectorului, `SED#-P`, o postează fără lot (9.316,02 + 358,36). Soldul
  cubului pe 3028 e 11.648,86, față de 2.332,84 în registrul contabil;
  totalul 3xx nu se schimbă. Puntea e din vremea în care ASM-ul nu posta
  contabil (`docs/import/faza-1c-design.md`, „NTC-punte"). Ține de conector:
  la migrare, puntea reclasificării cade pentru ASM-urile care postează.
- **A-2. Valoarea fără cantitate are altă cifră decât în decizia 107.**
  Decizia și contractul M1 dau 47 de celule / 719,94 lei; baza are 130 de
  rânduri / 6.267,19 lei, iar logul importului care a scris-o dă 121 de chei
  și aceeași sumă. Cauza e aceeași.
- **A-3. Litera (c) nu cunoaște rolul `Autocolectare`.** Diferența nu era
  declarată ca diferență a reconcilierii și n-a fost văzută până acum: scena
  `PerfCub` n-are achiziții în taxare inversă. Unealta dispare la pasul 6.
- **A-4. Golirile nu sunt „egale prin construcție".** Pe toate cele 177 de
  ieșiri cu valoare diferită, valoarea cubului e evaluarea nucleului pe
  soldul cubului; 99 dintre ele sunt goliri, fiindcă fiecare parte își
  golește propriul rest. T-D2.2 și T-D4.2 spuneau că golirile sunt egale, iar
  ASM-B7 că formula primește soldul registrelor: pe bazele scrise la
  2026-10-04 și 2026-10-05 nu e așa. Clasa și cauza rămân aceleași.
- **A-5. Reconcilierea nu dă nicio probă a partidelor** pe această bază:
  litera (f) e vacuă fără perioadă închisă.

## 2. `PerfCub` în regim dual: termenul A

Scara completă, pe clonele `Atlas.Conta.BackOffice.D9P5b` și
`Atlas.Conta.ModelCheck.Privat.D9P5b`, în containerul din rețeaua Postgres:
`run-verificari/perf-cub-20261006-161201/`. Privat **1.541 OK**, bugetar
**1.011 OK**, zero FAIL, DUK exit 0; `PREST-NI` rămâne `AMÂNAT`. A doua
mostră, `-Operatii JRN`, dă zgomotul comenzilor de scriere:
`run-verificari/perf-cub-20261006-164756/`. Binarul: commit `417af66`,
arbore curat, `dllSha256` `B3233835…9FAA2A79`.

Comenzile de scriere, mediana în ms peste toate execuțiile la m = 0 / 6 / 12
luni închise:

**privat**

| comandă | execuții | mostra 1 | mostra 2 | comenzi SQL |
|---|---|---|---|---|
| AMO | 1 / 6 / 12 | 169,5 / 47,1 / 46,1 | 175,5 / 46,2 / 47,8 | 91 / 89 / 89 |
| ASM | 64 / 160 / 256 | 28,6 / 29,9 / 29,5 | 29,6 / 29,4 / 30,4 | 53 |
| BCS | 64 / 160 / 256 | 26,3 / 27,1 / 26,6 | 27,8 / 26,6 / 27,6 | 50 |
| BCS lung | 1 / 1 / 1 | 115,0 / 125,5 / 104,1 | 107,3 / 104,1 / 121,0 | 113 |
| BTR | 64 / 160 / 256 | 23,7 / 25,0 / 24,5 | 25,1 / 24,6 / 25,4 | 47 |
| DSC | 64 / 160 / 256 | 29,1 / 30,7 / 30,1 | 30,4 / 29,7 / 30,9 | 56 |
| FCL | 64 / 160 / 256 | 28,3 / 29,4 / 29,0 | 29,2 / 28,9 / 30,2 | 66 |
| FCT | 64 / 160 / 256 | 38,8 / 41,3 / 40,0 | 41,3 / 40,0 / 41,5 | 80 |
| împerechere | 128 / 320 / 512 | 18,8 / 19,6 / 19,7 | 19,8 / 19,7 / 20,4 | 29,5 |
| INC | 64 / 160 / 256 | 19,8 / 21,0 / 20,5 | 20,7 / 20,5 / 21,1 | 43 |
| închidere | 6 / 12 | 71,2 / 72,4 | 64,7 / 75,7 | 68 |
| NIR | 64 / 160 / 256 | 30,7 / 31,8 / 31,1 | 32,0 / 31,3 / 32,5 | 67 |
| PLT | 64 / 160 / 256 | 19,9 / 20,9 / 20,5 | 21,1 / 20,6 / 21,3 | 43 |

**bugetar**

| comandă | execuții | mostra 1 | mostra 2 | comenzi SQL |
|---|---|---|---|---|
| AMO | 1 / 6 / 12 | 197,1 / 49,4 / 49,0 | 171,9 / 48,0 / 46,7 | 93 / 91 / 91 |
| ASM | 64 / 160 / 256 | 29,0 / 29,2 / 30,8 | 29,8 / 29,9 / 30,2 | 53 |
| BCS | 64 / 160 / 256 | 26,9 / 26,8 / 27,9 | 27,1 / 27,3 / 27,5 | 50 |
| BCS lung | 1 / 1 / 1 | 121,4 / 122,5 / 119,6 | 109,9 / 185,3 / 171,8 | 113 |
| BTR | 64 / 160 / 256 | 24,8 / 24,5 / 26,1 | 25,0 / 25,1 / 25,2 | 47 |
| FCL | 64 / 160 / 256 | 23,8 / 23,3 / 24,5 | 23,9 / 24,1 / 24,1 | 52 |
| FCT | 64 / 160 / 256 | 32,9 / 33,2 / 34,9 | 33,8 / 34,1 / 34,3 | 63 |
| împerechere | 128 / 320 / 512 | 19,1 / 19,0 / 20,2 | 19,5 / 19,7 / 20,0 | 29,5 |
| INC | 64 / 160 / 256 | 21,0 / 20,8 / 21,9 | 21,5 / 21,5 / 21,8 | 45 |
| închidere | 6 / 12 | 47,6 / 50,3 | 47,0 / 51,9 | 50 |
| NIR | 64 / 160 / 256 | 30,5 / 30,5 / 32,0 | 31,1 / 31,2 / 31,3 | 65 |
| PLT | 64 / 160 / 256 | 20,8 / 20,9 / 21,6 | 21,3 / 21,4 / 21,6 | 45 |

**Cum se citește după tăiere.** Numărul de comenzi SQL e identic între
mostre pe toate perechile: e termenul sigur al comparației. Durata are
zgomot: pe comenzile cu cel puțin 64 de execuții, mediana diferă între
mostre cu cel mult 6,2 %; pe cele cu o singură execuție, cu până la 51 %.
O diferență de durată sub acest zgomot nu e efect. Termenul B se măsoară cu
aceeași rețetă, pe clone ale acelorași două baze (`.D9P5b` rămân până
atunci).

**Față de 2026-10-04** (alt commit, alte clone): comenzile din domeniul
gardului analizei au câte 2 comenzi SQL în plus pe privat și 2–4 pe bugetar
(FCT, FCL, PLT, INC, BCS, DSC, AMO), iar cele două predicții (`FIFO-N`,
`PDISP-N`) câte una. ASM, BTR, NIR-ul conex, împerecherea și închiderea,
care nu trec prin gard, au același număr. Atribuirea către pasul 2 e din
coincidența de domeniu și din citirea `GardAnaliza.Verifica`, nu dintr-o
măsurare pe commit. Rândurile livrate, numărul de scanări pe `Postare` și
cifrele de control sunt neschimbate pe toate punctele.

## 3. Scara de volum

**Metoda.** Scena `PerfCub` privată la m = 12, k = 64 (7.951 de postări) e
păstrată în bază și multiplicată „în lățime" direct în SQL: fiecare copie
are identități proprii pentru tranzacții, postări, documente, loturi și
partenerii externi ai scenei; conturile, produsele, gestiunile și perioadele
sunt comune. Partidele copiei își primesc identitatea prin funcția bazei
`cub_partida_id`, deci cititorul produsului le recunoaște originea.
Liniile de document, registrele, împerecherile și fișele nu se multiplică:
baza rezultată e de citire. După fiecare treaptă: `VACUUM (ANALYZE)`,
probele de corectitudine, reconstrucția snapshot-urilor pe calea produsului
și măsurarea cititorilor, rece și cald, fiecare într-un proces nou.
Utilizatorul e cel administrativ al scenei; Postgres e pe configurația
implicită (`shared_buffers` 128 MB, `work_mem` 4 MB). Excluși: cititorii
fiscali și SAF-T, care citesc linii de document, și închiderea, care scrie.

Rularea: `run-verificari/scara-volum-20261006-173637/`, 93,7 minute, 301 OK,
zero FAIL. Baza: `Atlas.Conta.ModelCheck.Privat.D9Vol`.

| | ×1 | ×10 | ×100 | ×629 |
|---|---|---|---|---|
| postări | 7.951 | 79.510 | 795.100 | 5.001.179 |
| documente | 2.358 | 23.580 | 235.800 | 1.483.182 |
| partide | 1.036 | 10.360 | 103.600 | 651.644 |
| baza, MiB | 41 | 81 | 528 | 2.665 |
| reconstrucția snapshot-urilor | 0,3 s | 2,5 s | 135,7 s | oprită la 30 de minute |

Cititorii, ms la cald:

| operație | ×1 | ×10 | ×100 | ×629 | rânduri livrate la ×629 |
|---|---|---|---|---|---|
| balanța, securizat / sistem / din snapshot | 13,0 / 8,6 / 13,3 | 29,4 / 24,4 / 29,3 | 140 / 135 / 169 | 1.148 / 1.128 / — | 7.561 |
| fișa contului, la fel | 25,3 / 15,9 / 23,1 | 30,2 / 20,2 / 25,9 | 39,2 / 30,7 / 39,1 | 130 / 122 / — | 320 |
| registrul jurnal, securizat / sistem | 13,8 / 10,8 | 78,1 / 56,5 | 357 / 302 | 1.930 / 1.861 | 888.148 |
| soldurile de parteneri | 13,1 / 8,0 / 8,8 | 28,4 / 22,9 / 20,6 | 126 / 123 / 118 | 1.022 / 1.047 / — | 7.560 |
| partidele cu rest | 39,8 / 35,7 / 40,5 | 1.166 / 1.183 / 1.140 | 83.607 / 79.079 / 169.462 | peste 10 minute | — |
| soldurile de loturi | 11,3 / 7,5 / 8,4 | 73,0 / 39,3 / 38,9 | 357 / 290 / 306 | 1.341 / 1.254 / — | 651.644 |
| predicția FIFO | 44,3 | 32,1 | 39,7 | 38,1 | 1 |
| disponibilul partidei | 23,8 | 34,5 | 46,8 | 152 | 1 |
| verificarea snapshot-urilor | 135 | 2.513 | 130.861 | — | — |

La ×629 rutele din snapshot și verificarea lui nu au cifră, fiindcă
reconstrucția n-a terminat.

Ce spun cifrele:

- **Ruta securizată nu costă la volum.** Face 6 comenzi SQL în loc de una,
  iar diferența e de câteva milisecunde la orice treaptă. Utilizatorul e
  administrativ: filtrele de rând ale unui rol restrâns nu sunt măsurate.
- **Balanța, soldurile de parteneri, jurnalul și loturile cresc liniar** și
  rămân sub două secunde la 5 milioane de postări. Jurnalul și loturile
  livrează tot ce citesc (888.148 și 651.644 de rânduri, circa 450 MiB
  alocați pe execuție): costul lor e al consumatorului care cere tot.
- **Snapshot-ul nu scurtează balanța** la acest volum (169 ms față de 135 la
  ×100).
- **Partidele cu rest și reconstrucția cresc pătratic**: vezi §4.

Probele de corectitudine, refăcute de main în SQL pe baza de la ×629:
postările pe `Spatiu × Carte`, `Σ Valoare` pe latură, documentele și loturile
sunt exact 629 × scena; tranzacțiile la fel, pe `Operare`, `Storno` și
`Transfer`; nicio tranzacție dezechilibrată în cartea contabilă; 651.015
partide cu origine pe document, recunoscute de `cub_partida_id`, plus 629 de
partide de deschidere. Cifrele de control ale cititorilor: 144 egale cu
așteptarea, niciuna diferită. Main a re-măsurat balanța la ×629 (1,5 s sub
`EXPLAIN ANALYZE`, 7.561 de chei).

Abaterea de la rețetă: tranzacția `Deschidere` nu se clonează, fiindcă
indexul `IX_Tranzactie_Fel` admite una singură pe bază. Postările ei clonate
rămân pe tranzacția originală.

## 4. D9-D10 (b): cifra și cauza

| factor | partide cu rest | securizat, ms | sistem, ms | din snapshot, ms |
|---|---|---|---|---|
| ×1 | 777 | 39,8 | 35,7 | 40,5 |
| ×10 | 7.770 | 1.166 | 1.183 | 1.140 |
| ×100 | 77.700 | 83.607 | 79.079 | 169.462 |
| ×629 | 488.733 | peste 10 minute | peste 10 minute | nemăsurat |

Rândurile atinse pe `Postare` cresc liniar cu volumul; durata nu.

**Cauza nu e criteriul amânat.** Criteriul de formă din X-D5 spune că
`PartideCuRest` parcurge toate postările de partidă ca să găsească
documentul deschizător. Parcurgerea aceea e liniară. Ce crește pătratic e
îmbinarea dintre solduri și origini: cheile ei, unitatea și partenerul, sunt
nulabile pe `Postare`, iar interogarea generată le compară cu
`a = b OR (a IS NULL AND b IS NULL)`. Postgres nu poate îmbina pe o
asemenea condiție, așa că îmbină numai pe cont și respinge restul perechilor
una câte una: 4,04 miliarde de perechi la 103.600 de partide.

**Proba, pe aceeași bază.** Aceeași interogare, cu egalitate simplă pe cele
trei chei, rulată de main pe baza de la ×629: **8,9 secunde**, 488.733 de
rânduri, adică 777 × 629. Forma generată de produs nu termină în 10 minute.

**Aceeași formă e în trei locuri din produs:**

| Loc | Ce face | Efect măsurat |
|---|---|---|
| `Proiectii/ImperecheriProiectii.cs`, `PartideCuRest` | raportul partidelor cu rest | tabelul de mai sus |
| `Motor/SolduriService.cs`, `SursaPartide` | snapshot-ul de partide, scris la închiderea lunii și la reconstrucție | 60,9 s din verificarea de la ×100; reconstrucția de la ×629 oprită la 30 de minute |
| `Cub/Citiri/Partide.cs`, `Proprii` | perechile, restul și disponibilul unui document | filtrat pe document: 152 ms la ×629 |

Al doilea rând e cel care apasă: **închiderea de lună scrie snapshot-ul de
partide prin aceeași îmbinare**. Scara n-a măsurat închiderea, fiindcă
scrie, dar instrucțiunea e aceeași.

Ce rămâne de hotărât de owner, cu cifra:

1. îmbinarea se corectează în felie sau după ea. E cod de produs, deci nu a
   intrat în 5b. Semantica nu se schimbă: cheile comparate sunt nenule prin
   filtrul cititorului. Proba ar fi catalogul neschimbat și scara re-rulată
   pe `.D9Vol`;
2. criteriul de formă propriu-zis (documentul deschizător citit fără
   parcurgerea tuturor postărilor de partidă) se închide sau se re-amână.
   Cu îmbinarea corectată, parcurgerea costă 8,9 s la 651.644 de partide.

## 5. Unealta și bazele

`ModelCheck --scara-volum privat`, rulat de
`nou/tools/ModelCheck/scripts/scara-volum-container.ps1`. Scena se păstrează
printr-un comutator al `PerfCub`; fără el, `PerfCub` și suita integrală
rulează ca înainte. SQL-ul multiplicării e resursa `ScaraVolum.sql`.

Baze create pe 5444, niciuna ștearsă:

| Bază | Mărime | Rost |
|---|---|---|
| `Atlas.Conta.BackOffice.D9P5b` | 30 MB | sursa termenului A, bugetar; rămâne până la termenul B |
| `Atlas.Conta.ModelCheck.Privat.D9P5b` | 32 MB | sursa termenului A, privat; la fel |
| `Atlas.Conta.ModelCheck.Privat.D9Vol` | 2.665 MB | baza scării, la ×629; snapshot-urile ei sunt cele de la ×100 |
| `Atlas.Conta.ModelCheck.Privat.D9VolProba` | 51 MB | proba mică a uneltei; se poate șterge |

`.D9Vol` are schema suplimentară `scara` (fotografia scenei) și e baza pe
care se re-măsoară corectura din §4.

## 6. Verificarea

ModelCheck integral înainte și după: **3.552 bugetar / 4.860 privat OK**,
zero FAIL. Comparația normalizată a celor două rulări: zero linii dispărute
sau apărute pe privat; pe bugetar două, numai prin numărul de rânduri din
nume (constatarea de mai jos). Soluția compilează fără erori; `--probe-sursa`
10 OK. Arborele e atins numai în `nou/tools/ModelCheck/`.

## 7. Constatări în afara pasului

- **Suita integrală lasă reziduu pe bugetar**: câte un rând
  `E2E-SC-PARTIDE-MIX` în `CoduriFunctionale` și în `SurseFinantare` la
  fiecare rulare. Baza `Atlas.Conta.BackOffice` are 22 de asemenea rânduri,
  iar numele aserției `Cautare == Normalizeaza pe … (N rânduri)` crește cu
  unu la fiecare rulare. Nu schimbă niciun verdict; o purjă lipsă a unei
  scene.
- **Limita probei.** `.Flax.R3f` e scrisă înaintea pasului 2. Gardul
  analizei refuză, nu rescrie, deci postările ei sunt cele pe care le-ar
  scrie codul de azi; că gardul n-ar refuza astăzi un document deja operat
  acolo nu se poate proba fără re-operare.
