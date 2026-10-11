# C111, pasul 6 — uneltele de volum, costul scrierii și scara pe forma nouă

**Data:** 2026-10-11. **Stare:** făcut; pasul 6 e închis împreună cu
[importul](c111-pas6-import.md). Contractul:
[C111](c111-coordonate-contract.md), D9 și C111-Q17.

Urmele, neversionate: `run-verificari/c111-pas6/` (`perf-head/`, `perf-jrn*/`,
`perf-head2/`, `scara/`, `fk-comenzi/`, `perf-vechi/`, `fk-rand-5m.json`, cu
rețetele `detasat.ps1`, `fk-rand.py`, `fk-comenzi.ps1`, `perf-vechi.ps1`,
`comenzi-compara.py`, `scara-compara.py`, `jurnal-ab.py`).

## Rezumat

| Ce | Rezultatul |
|---|---|
| `ScaraVolum.sql` pe forma nouă | rulează; probele de corectitudine verzi la ×1, ×10 și ×629 |
| Scara transversală (PerfCub) după C111 | prima rulare: 2 FAIL pe profil, toate pe registrul jurnal; după corectură 1.580 privat / 1.046 bugetar OK, zero FAIL |
| Integrala după corectură | 3.667 bugetar / 5.069 privat OK, zero FAIL (`run-verificari/20261011-010523-964/`) |
| Cele două chei străine noi, pe rând | 2,3 µs pe postare la inserare, împreună |
| Cele două chei străine noi, pe comandă | sub zgomot: raportul timpilor SQL cu / fără e 0,996, între rulări identice 0,97–1,05 |
| Forma nouă față de `main`, pe comandă | timpii în zgomot; același număr de comenzi SQL, în afară de AMO (+2) și NIR (+1) |
| Scara de volum, 5.001.179 de postări | 24 de minute; toți cititorii cu cifrele de control ale scenei |
| Registrul jurnal al lunii la volum, cu și fără contrapartidă | 4.772 ms și 1.287 ms, pe aceeași bază, pentru 888.148 de rânduri |

Totul a rulat pe instanța nativă (5446), cu binarul Debug al ModelCheck, câte
o rulare grea o dată.

## Uneltele

`ScaraVolum.sql` numește explicit coloanele fiecărei tabele clonate și
putrezise de două ori: la `Documente.TotalStingere` și acum, de trei coloane
deodată (`Partener`, `Gestiune`, plus `Ordinal`, adăugat de decizia 117 și
niciodată scris de script). E adus la forma nouă:

- partenerii de clonat sunt cei purtați de `Repartitor` sau de
  `PartenerFiscal`; gestiunile, conturile proprii și `Loc` rămân neclonate;
- originea partidei se recunoaște pe `Repartitor`;
- postările de deschidere clonate rămân pe tranzacția unică de deschidere și
  își continuă ordinalele, altfel baza de volum ar încălca invariantul din 117;
- probele de copie compară și partenerul fiscal clonat, iar o probă nouă
  numără tranzacțiile cu ordinale nenumărate de la 1.

Proba `SCARA-COLOANE` rulează în orice ModelCheck și compară listele de
coloane din script cu coloanele scrise ale bazei. O coloană nouă pe una dintre
cele cinci tabele pică acolo.

Rețetele rulează nativ și și-au pierdut „container” din nume:
`scripts/perf-cub.ps1` și `scripts/scara-volum.ps1`, cu `-PregatesteBaze` și
`-Nume`. `perf-saft-container.ps1` a rămas cum era.

## Ce a prins scara transversală

Prima rulare completă de după C111 a trecut scena pe ambele profiluri (S6–S8
nu-i schimbă cifrele de control) și a picat numai pe registrul jurnal:

- privat: accesul la `Postare` depindea de istoric (3.958 de rânduri atinse la
  m = 0, 7.062 la m = 6 și m = 12);
- bugetar: durata creștea mai mult decât liniar în k (12 ms la k = 16,
  83 ms la k = 64).

Cauza e în pasul 5. Registrul jurnal căuta cealaltă postare a perechii în
`Contabil.Postari`, adică în toate postările vizibile, cu cele patru îmbinări
ale filtrului de vizibilitate. Planul citea toată tabela ca să găsească
perechile unei luni.

Corectura are două părți:

1. Perechea se caută în `Contabil.Perechi`: domeniul contabil, fără filtrul pe
   felul tranzacției. Cele două postări ale unei perechi stau în aceeași
   tranzacție, iar la storno originalele lor stau în aceeași tranzacție
   originală, deci vizibilitatea uneia e a celeilalte.
2. Sursa perechii e mărginită de intervalul cititorului. Asta cere ca cele
   două postări să aibă aceeași dată. Nucleul le scrie așa (`Miscare.Postari`,
   `Mutare.Postari`, `Storno.Inverseaza`); `INV-CUB` o cere acum
   (`VerificaPerechi`), cu mutantul `PERECHE-DATA`. Pe baza de import: 35.038
   de perechi, niciuna cu date diferite.

Fișa contului folosește aceeași sursă. Pe import, fișa lui 401 pe ianuarie dă
2.205 ms pe contul întreg și 185 ms pe cont × repartitor (mediana a cinci
rulări prin produs), față de 1.955 și 191 ms înaintea corecturii; diferența e
în împrăștierea rulărilor.

## Costul celor două chei străine noi

Pe `main` postarea are o cheie străină spre `Repartitori` pe fiecare partiție
(`Partener`); pe forma nouă are trei (`Repartitor`, `PartenerFiscal`, `Loc`).
Măsurarea e pe aceeași bază, prin schimbarea stării (C111-Q17), nu între baze
diferite, cum scria contractul.

**Pe rând.** Pe o clonă a bazei de volum, inserarea a zece copii ale scenei
(79.510 postări) rulează sub `EXPLAIN (ANALYZE)` într-o tranzacție anulată,
deci fiecare măsurare pleacă din aceeași stare. Planul dă timpul fiecărui
declanșator de cheie străină. Mediana a cinci rulări:

| Cheia | `Postare_Contabil` (57.290 de rânduri) | `Postare_Stoc` (22.220) |
|---|---|---|
| `PartenerFiscal` | 1,31 µs pe rând | 0,88 µs |
| `Loc` | 1,20 µs | 0,78 µs |
| `Repartitor` | 3,92 µs | 4,35 µs |
| `TranzactieId`, pentru scară | 8,41 µs | 7,97 µs |

Cele două chei noi costă împreună 179 ms la 79.510 postări, adică 2,3 µs pe
postare. În scenă, 22% din postările contabile și 12% din cele de stoc au
partener fiscal și loc; restul sunt nule, iar verificarea unei valori nule e
mai ieftină. Toate cheile străine ale postării costă împreună 31 µs pe
postare, iar inserarea întreagă 140–190 µs. Timpul total al celor trei
variante (toate cheile, fără cele două noi, fără nicio cheie spre
`Repartitori`) se suprapune de la o rulare la alta: 10,9–15,3 s, 11,2–14,3 s
și 10,7–13,1 s.

**Pe comandă.** Pe baza scării transversale private, scena cu m = 12 și
k = 64 s-a construit de patru ori, alternat, cu și fără cele două chei:
512 comenzi de fiecare tip și stare. Raportul medianelor de timp SQL, cu
împărțit la fără, pe cele 12 tipuri de comandă: median 0,996, între 0,884
(închiderea, 24 de comenzi) și 1,007. Două rulări în aceeași stare diferă
între ele cu 0,97–1,05. La nivel de comandă cele două chei nu se văd.

## Forma nouă față de `main`

Binarul de pe `main` (`1cc9d93`, într-un worktree) a construit aceeași scenă
pe clone ale bazelor `…C111P0`, din același seed, pe aceeași instanță.
Compararea e între baze diferite și e orientativă pentru timp.

| Profil | Raportul medianelor de timp SQL, nou / vechi | Comenzi SQL pe comandă |
|---|---|---|
| privat | median 1,008 (0,989–1,037) | identice pe 11 tipuri; AMO 81 față de 79, NIR 58 față de 57 |
| bugetar | median 0,972 (0,929–1,003) | identice pe 10 tipuri; AMO 83 față de 81, NIR 56 față de 55 |

Comenzile în plus sunt citirea felului repartitorilor din nomenclator, o
interogare pe lot, fiindcă felul nu e coloană pe postare (111 (a)):
`Fapte.Repartitori` de două ori la imobilizări și `Randuri.Repartitori` la
diferența NIR-ului.

## Scara de volum

`scara-volum.ps1 -Factori 1,10,F` pe `Atlas.Conta.ModelCheck.Privat.C111Vol`:
scena de 7.951 de postări, multiplicată de 629 de ori, 5.001.179 de postări.
Baza rămâne, ca bază de citire pe forma nouă.

| Mărime | Forma nouă, nativ | 2026-10-08, container, forma veche |
|---|---|---|
| durata rulării | 24,1 min (trei trepte) | — (numai treapta finală) |
| multiplicarea până la ×629 | 597 s | 300 s |
| prima reconstrucție a snapshot-urilor | 73,2 s | 44,2 s |
| `Postare_Contabil`, tabela | 720 MiB | 696 MiB |
| `Postare_Contabil`, indecșii | 505 MiB | 542 MiB |
| rânduri în `SolduriPerioadaContabil` | 248.428 | 616.195 |
| rânduri de balanță analitică | 3.164 | 7.561 |
| baza întreagă | 2.907 MiB | 3.026 MiB |

Balanța analitică și snapshot-ul contabil au mai puține chei fiindcă pe
conturile cu fapt fiscal ale scenei (302, 4426, 4427, 628 și 704) partenerul
e pe forma nouă partener fiscal, nu coordonată de sold (111): șapte chei cont
× partener mai puțin, de 629 de ori. Partenerii clonați sunt aceiași cinci.

Cititorii, la cald, pe ruta nesecurizată:

| Cititor | Rânduri livrate | Forma nouă, nativ | 2026-10-08, container |
|---|---|---|---|
| balanța analitică (`BAL-NV`) | 3.164 | 1.586 ms | 1.128 ms |
| balanța din snapshot (`BAL-NI`) | 3.164 | 1.007 ms | 622 ms |
| fișa contului pe partener (`FISA-NV`) | 320 | 136 ms | 128 ms |
| registrul jurnal (`JRN-N`) | 888.148 | 6.914 ms | 1.887 ms |
| soldul partenerilor (`SPART-NV`) | 3.160 | 1.534 ms | 1.041 ms |
| partidele cu rest (`PREST-NV`) | 488.733 | 13.431 ms | 6.367 ms |
| stocul (`STOC-NV`) | 651.644 | 2.703 ms | 1.324 ms |
| dry-run-ul notei stingătoare (`PDISP-N`) | 1 | 20 ms | 147 ms |
| reconstrucția (`RECON-N`) | 1 | 88,8 s | 57,3 s |

Cele două coloane nu se compară ca efect al formei: sunt instanțe diferite.
Balanța, stocul și partidele cu rest au aceleași planuri, nod cu nod, și
aceleași rânduri atinse în ambele rulări, iar timpul e de 1,4–2,1 ori mai mare
pe nativ. Diferența e a instanței. `PDISP-N` e mai rapid din deciziile
114–117 (1.794 de rânduri atinse, față de 5 milioane), nu din C111.

Registrul jurnal e singurul la care forma contează: are acum contrapartida.
Pe aceeași bază, cu SQL-ul produsului și cu forma lui fără îmbinarea perechii
(`jurnal-ab.py`, cinci rulări alternate, timpul din `EXPLAIN ANALYZE`): 1.287
ms fără contrapartidă și 4.772 ms cu ea, pentru 888.148 de rânduri. Planul
caută perechea fiecărui rând prin indexul pe tranzacție, pe ambele partiții:
888.148 de căutări, câte patru pagini fiecare.

## Ce rămâne

- Bazele de import și clonele de măsurare de pe 5444 sunt pe schema veche,
  inclusiv `.D9bVol`. Baza de volum la zi e `.C111Vol`, pe 5446.
- Contrapartida costă circa 4 µs pe rând de jurnal la volum. Planul ales e
  bucla cu index; o îmbinare prin hash pe postările intervalului n-a fost
  încercată.
- Instanța nativă e mai lentă decât containerul pe citirile mari. Cauza nu e
  căutată; ambele sunt Postgres neconfigurat.
