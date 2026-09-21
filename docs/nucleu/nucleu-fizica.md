# Fizica cubului, măsurată pe baza de import (pasul 2 din §11)

Stare: **PROBAT (2026-09-19), cu review advers aplicat** (agent separat, 5
MAJOR / 8 MEDIU / minore; toate acceptate — unde au schimbat o concluzie, e
spus; probele lui în `run-nucleu/fizica/review/`). Întrebarea din
`nucleu-cub-design.md` §11: un cub logic, N partiții fizice pe spațiu, indexi
parțiali per uz — cifră pe volumele de import, nu opinie. Contractul
măsurătorii (FZ-D1…D9) e `run-nucleu/fizica/CONTRACT.md`; cifrele integrale,
EXPLAIN-urile, SQL-ul și diff-urile sunt în `run-nucleu/fizica/`
(`01-interogari-azi.md`, `02-semantica-mapare.md`, `pas1/out/`,
`pas2/rezultate.md`, `pas2/out/tabel-x10c.md`, `pas2/egalitate.md`,
`pas2/indexi-f2.md`, `pas2/scriere.md`).

## 1. Ce s-a măsurat și cum

Baza: clona lui `Atlas.Conta.Import1C.Flax` (2025 integral, profil privat:
304.382 rânduri contabile, 283.498 de stoc, 90.732 fiscale, 205.186 documente
operate, 44.448 împerecheri). Registrele au fost transformate DETERMINIST în
cub (un rând contabil = două postări cu `Latura`; un rând de stoc = o postare
cu `Cantitate` și `Valoare`; un rând fiscal = două postări, Bază și Taxă;
împerecherea = tranzacție proprie datată, §4.4) și verificate cu un gate de
reconciliere înainte de orice cifră: Σ D = Σ C pe fiecare tranzacție, Σ per
cont / lot / (cod TVA × perioadă) egale cu registrele, numărul de postări
egal cu formula. Toate verzi (`pas1/out/reconciliere.txt`).

Cubul la ×1: **1.162.622 postări** (697.660 contabile, 283.498 stoc, 181.464
fiscale), 249.622 tranzacții, 8 versiuni de `CodTva`. La ×10 (același an
replicat 2016–2025 cu ACELEAȘI ID-uri de coordonate și de documente):
**11.562.545 postări** (`count(*)`; cifrele din fișierele de mărimi sunt
`reltuples`, estimări).

Forme comparate, pe aceleași date și aceleași setări Postgres (18.4,
`shared_buffers` 160 MB, `work_mem` 4 MB, neatinse): **F0** = registrele de
azi cu snapshot-urile și indexii EF; **F1** = cubul plat cu un index pe
fiecare coloană-cheie (portarea naivă); **F2** = cubul partiționat LIST pe
`Spatiu` (Contabil / Stoc / Fiscal), cu indexi per uz proiectați din lotul de
interogări înainte de măsurare; **F3** = F2 sub-partiționat RANGE pe an;
**+S** = F2/F3 cu tabela `Sold` (Σ pe toate coordonatele la 31 decembrie).
Lotul: 14 interogări = rapoartele reale (balanțe, fișă, sold pe partener,
partide cu rest, jurnal TVA, D394, FIFO, PhysicalStock, terți SAF-T, GLE,
citirea pentru storno, scrierea operării), pe F0 în forma proiecțiilor de
azi, pe cub în forma din `nucleu-coordonate-rapoarte.md` §3. Mediana a 5
rulări calde după una aruncată, `EXPLAIN (ANALYZE, BUFFERS)`.

**Limitele metodei, declarate înainte de cifre** (scoase de review):
- replicarea ×10 a lăsat F0 FĂRĂ referința 2024-12-31 pe care
  `SolduriService.Referinte` (`Motor/SolduriService.cs:75-89`, „ultima
  închisă plus fiecare decembrie închis") ar avea-o într-o bază reală cu ani
  închiși — deci coloana F0 de la ×10 pe balanțe e F0 dezavantajat;
- `Sold` are un număr CONSTANT de rânduri per graniță (389.656 pe Contabil la
  fiecare din cele 10), fiindcă coordonatele s-au refolosit — cifrele +S de
  la ×10 citesc o graniță + un an, adică date de ×1: sunt LIMITE INFERIOARE,
  nu cifre de scară;
- interogările pe cub sunt sume pe coordonate; unde F0 livrează raportul
  întreg (antete, denumiri), cubul livrează agregatul — comparația e notată
  la fiecare caz;
- transformarea nu filtrează `GCRecord` (0 rânduri șterse logic pe Flax în
  toate registrele), deci egalitatea nu acoperă o bază cu ștergeri logice.

## 2. Rezultatul, în cinci răspunsuri

### (1) Forma: F2 — pe structură, nu pe viteză de citire

Mediana la ×10 (ms), toată coloana din aceeași rulare, cu setul final de
indexi (`pas2/out/tabel-x10c.md`); îngroșat = cea mai rapidă formă de CUB:

| Q | F0 | F1 | F2 | F3 | F2+S | F3+S |
|---|---|---|---|---|---|---|
| Balanță sintetică (Q01) | 618° | 485 | 424 | 548 | 109 | **89** |
| Balanță cont × partener (Q02) | 1.068° | 1.190 | 866 | 1.119 | 300 | **273** |
| Fișa 4111, tot istoricul (Q03) | 1.017 | **1.090** | 1.757 | 1.248 | — | — |
| Fișa 4111 × partener (Q04) | 719 | 77 | 33 | **32** | — | — |
| Sold 4111 × partener (Q05) | 6°° | 388 | 323 | 383 | 55 | **52** |
| Partide cu rest (Q06) | 72°° | 691 | 567 | 652 | **63**‡‡ | 63 |
| Jurnal cumpărări, o lună (Q07) | 23 | **5,4** | 7,4 | 6,7 | — | — |
| D394, o lună (Q08) | 18 | **6,6** | 7,5 | 7,5 | — | — |
| FIFO produs × gestiune (Q09b) | 1,0 | 1,8 | **0,8** | 1,0 | — | — |
| PhysicalStock la 31.12 (Q10) | 698 | 1.436 | **524**† | 580 | — | — |
| Terți SAF-T, o lună (Q11) | 600 | **871** | 959 | 1.220 | 105 | **105** |
| GLE, rândurile lunii (Q12) | 3‡ | **266** | 275 | 273 | — | — |
| TaxInformation GLE (Q12b) | 17 | 540 | **137** | 180 | — | — |
| Citirea unui document (Q13) | 0,3 | **0,5** | 0,7 | 1,1 | — | — |
| Scrierea unui document, 49 linii (Q14) | 5,3 | 5,2 | **1,7** | 1,7 | — | — |

° F0 fără referința 2024-12-31 (limita metodei). Cu referință (proba
review-ului, `review/01-f0-cu-referinta.sql`, rulată pe ×10): **57 ms**, față
de 99 ms pe F2+S — F0 e mai RAPID, fiindcă `SolduriPerioadaContabil` e cheiat
mai grosier (184.780 rânduri) decât `Sold` la aceeași graniță (389.656).
°° F0 citește doar snapshot-uri nereplicate: cifra e de ×1; perechea de
comparat e +S, care e la rândul ei limită inferioară. ‡‡ Q06+S întoarce
`(Unitate, Sold, Sens)`; F0 livrează raportul cu antete și denumiri (șase
uniuni); cu antetul de document adăugat, Q06+S = 131–143 ms, ≈2× F0.
† cu setul de coloane al lui F0 (șase sume condiționate, două numărători,
cheia cu `TipStoc`), cubul dă 657 ms: −6 %, nu −25 %. ‡ F0 citește rânduri
plate și asamblează în C#; interogarea pe cub face în SQL joinul pe
`Tranzactie` (2,5 M rânduri la ×10) — evitabil, ca la F0; cei 275 ms măsoară
forma interogării și dublarea rândurilor, nu forma fizică.

Ce spun cifrele, fără narativ: la ×10, fără `Sold`, **F2 nu e mai rapid decât
F0 la citire**. Bate F0 pe interogările fiscale ale lunii (7 contra 23, 8 contra
18), pe intrarea prin partener (33 contra 719) și pe FIFO; e egal sau apropiat
pe balanțe și PhysicalStock; pierde pe terții SAF-T (959 contra 600), pe fișa
integrală (1.757 contra 1.017), pe GLE (forma interogării) și pe
TaxInformation. Cu `Sold`, câștigurile sunt CUB CONTRA CUB (§2.3); contra
snapshot-urilor de azi pe același gran nu s-a măsurat (FZ-r1).

Verdictul F2 stă pe structură: (a) **coloana polimorfă `Unitate` devine
FK-abilă prin partiție** — documentul-partidă pe Contabil, lotul pe Stoc; o
singură cheie străină pe tabela logică e imposibilă, două pe partiții sunt
verificate de bază (`pas2/fk-f2.sql`); (b) **scrierea**: 1,7 ms per document de
49 de linii scriind 198 de postări, contra 5,3 ms pe F0 pentru 99 de rânduri —
pe F0, 82 % din timp sunt cele 24 de triggere FK; cu FK echivalente pe cub
(9, fiindcă coordonatele nu se mai dublează debit/credit) F2 ajunge la 4,28
ms, în același interval cu F0 (4,14–4,98 contra 4,63–5,47) — cubul nu e mai
ieftin la scriere pentru că e cub, ci pentru că are jumătate din FK-uri; (c) **un singur set de indexi** (4 per uz + 3 de identitate)
în locul celor 24 de pe `RegistruContabil`; (d) un singur snapshot (§2.3).
**F1** e respinsă cu cifră: cea mai scumpă formă la scriere la ×1 (6,55 ms,
fără niciun FK — plătește 18 indexi), 1,75 GB de indexi la ×10, fără
integritate pe coloana polimorfă. **F3** nu se plătește acum: pe interogările
lunii e egală sau mai lentă (indexul pe `Data` al lui F2 taie deja luna;
`Append` peste 12 sub-partiții adaugă), câștigă pe fișa integrală (1.248 față
de 1.757) și costă 511 MB de index în plus la ×10; se redeschide când
citirile integrale de istoric trec pragul acceptat pe baza reală (FZ-r6).

### (2) Indexii: identitate + PATRU per uz, nu unul per coordonată

Setul final (`pas2/indexi-f2.md`): PK `(Spatiu, ID)`, identitatea
`TranzactieId`, `DocumentId`, `LinieId` (citirea pentru storno, `Cauza`), și
per uz — Contabil `(Partener, Cont, Data) WHERE Partener IS NOT NULL` și
`(Data)`; Stoc `(Produs, Data) INCLUDE (Cantitate, Valoare, Gestiune, Unitate)`
(calea FIFO reală intră pe produs); Fiscal `(PerioadaDeclarare, CodTvaId)
INCLUDE (RolTva, Valoare, Partener, DocumentId, TranzactieId, LinieId)`
(jurnale și D394 ca `Index Only Scan`).

Cei doi indexi cei mai largi din setul proiectat — `(Cont, Data) INCLUDE (…)`
și `(Gestiune, Unitate, Data) INCLUDE (…)`, 1.384 MB la ×10 — **nu au fost
atinși de planificator la nicio interogare la ×10**: `Cont` nu e coordonată
selectivă pe date reale (4111 singur ține 144.241 din 697.660 de postări
contabile), iar agregatele largi se fac prin `Parallel Seq Scan` pe partiție,
pe care un index de 76 B/intrare nu-l bate. Scoaterea lor (iterația unică,
declarată) **nu costă nimic** — deplasările de 5–12 % văzute după iterație
apar cu același semn și pe F1, neatinsă de iterație, iar `shared read` pe F2
e identic înainte și după (Q01 154.335 → 154.357); singurul candidat de câștig
real e PhysicalStock (−16 % contra −2 % pe F1). BRIN pe `Data` a fost respins
pe forma de azi a interogărilor (singura condusă de `Data` extrage rânduri);
sub `Sold`, fiecare balanță devine o fereastră de un an peste istoric și
respingerea se reevaluează (FZ-r9).

Contrastul cu azi: `RegistruContabil` are 24 de indexi (unul per FK, 70 MB la
55 MB de tabelă), iar `IX_RegistruTva_PerioadaAn_PerioadaLuna` există și nu e
folosit — predicatul e scris aritmetic (`TvaProiectii.cs:145-157`): `Seq Scan`
pe tot registrul pentru o lună, contra 1,6 ms `Index Only Scan` cu predicatul
pe coloane.

### (3) Ce cumpără snapshot-ul: cub contra cub, 4–9×; contra F0 nemăsurat pe același gran

`Sold` = Σ `Cantitate`/`Valoare` pe TOATE coordonatele, per spațiu, la 31
decembrie; construit din cub în 1,7 s (×1) și 50 s (×10); proba de egalitate
`Sold(31.12.2025) == Σ directă` = 0 abateri la ambele scări. O singură tabelă
ține ce azi stă în trei snapshot-uri ȘI în `PartideDeschise`, pentru că
păstrează `Unitate` și partenerul: la ×1, la 31.12.2025, are 321.199 rânduri
pe Contabil (184.780 în `SolduriPerioadaContabil`, cheiat pe repartitor) și
110.786 pe Stoc; la ×10 aceeași graniță are 389.713.

Cub contra cub, la ×10: balanță 424 → 109 ms, balanță × partener 866 → 300,
sold pe partener 323 → 55, partide 567 → 63, terți SAF-T 959 → 105.
Mecanismul e în pagini: nicio formă nu încape în `shared_buffers` la ×10;
balanța citește 154.000 de pagini pe F2 și 19.600 pe F2+S. Proprietatea care
contează e că **costul unei citiri „la dată" cu `Sold` depinde de mărimea
graniței + postările de după, nu de lungimea istoricului** — dar mărimea
graniței crește cu numărul de coordonate distincte, iar aici a fost ținută
artificial constantă (FZ-r4). Cubul cu toate coordonatele e și un COST:
`Sold` la aceeași graniță e de 1,7× snapshot-ul de azi la ×1 (2,1× la ×10,
unde snapshot-ul F0 e nereplicat), și pe balanța sintetică F0 cu referință
(57 ms) bate F2+S (99 ms). Dacă un `Sold` mai
grosier (fără `Unitate`, sau doar pe cont) merită ca al doilea read model e
întrebare de măsurat, nu de presupus (FZ-r1). Discul: 1.233 MB la ×10 pentru
10 granițe, limită inferioară.

### (4) Ce se rupe la ×10

- **Citirile integrale ale istoricului** scalează liniar pe toate formele:
  fișa contului cu sold cumulativ 1,0–1,8 s, PhysicalStock 0,5–0,7 s, balanța
  × partener fără `Sold` 0,9–1,2 s. Fișa e singura interogare pe care F2 e
  clar mai lentă decât F0 la ×10 (1.757 față de 1.017 ms): F0 intră pe doi
  indexi de cont, F2 pe `Data` și filtrează. Remediul e structural și
  MĂSURAT (FZ-r2, 2026-09-19): fișa pornește de la `Sold` la granița
  anterioară + postările de după — F2 1.726 → **348 ms** la ×10 (F3+S 328, cu
  contrapartidă 2.255 → 940), egală cu forma fără snapshot la ×1 (istoric de
  un an); soldul final identic pe F0/F2/F2+S. Costul nu mai depinde de
  lungimea istoricului. Dar F0 are același remediu deja în cod (ramura de
  referință a lui `FisaCont`, F27-D3) și rămâne mai rapid: **248 ms**; sursa
  diferenței e granul lui `Sold` (126.263 rânduri pe 4111 la o graniță, 23 ms,
  contra 1.591 în snapshot-ul de azi, 1,2 ms — cifră pentru FZ-r1) și cele
  38 % rânduri în plus ale cubului pe fereastră la ×10 (împerecherile
  replicate). `(Cont, Data)` nu e remediu nici pe fereastra de un an: ignorat
  de planificator, forțat aduce 16 % pentru 512 MB. Detaliu:
  `pas2/rezultate.md` §FZ-r2.
- **Discul**, pe aceeași bază de comparație (`sum(pg_relation_size)` pe ×10,
  proba review-ului): F2 = 2.008 MB tabelă + 1.441 MB indexi (setul final) +
  `Tranzactie` 283 MB = 3.732 MB; cu `Sold` pe 10 granițe, 4.965 MB. F0
  replicat = 2.105 MB cu o singură graniță de snapshot; cu cele 10 decembrii
  pe care `Referinte` le-ar materializa, ≈3,4 GB. Raport: **1,5–1,8× discul
  lui F0**, în funcție de câte granițe se socotesc de fiecare parte. Sursa:
  11,56 M postări contra 6,72 M rânduri de registru (contabilul și fiscalul
  se dublează prin `Latura` și rol); lățimea rândului e aceeași (182 B contra
  189 B).
- **Scrierea nu se rupe**: 1,68 ms per document de 49 de linii la ×10 pe F2,
  5,31 pe F0 (§2.1 b).

### (5) Ce nu s-a putut proba

`Valuta`/`Atribuit` (fără date), stornoul (0 rânduri pe Flax; forma e în
schemă), imobilizările (registrul e gol pe Flax), lookup-ul per partidă
(SAF-T Payments `SourceDocumentID` — nu e în lot, neindexat pe speculație),
`TipStoc` ca redundanță (pe sink-ul de consum e singurul care separă),
reperele `TaxInformation` din GLE (`LinieId` contra `PerioadaDeclarare`,
necomparabile prin construcție), costul FK la ×10 și încărcarea în bloc,
creșterea reală a coordonatelor pe zece ani, `Sold` contra snapshot-urile de
azi pe același gran, împerecherea ca tranzacție (§4.4 — nicio postare de
împerechere nu intră în vreo interogare a lotului, toate fiind datate 2026).
Rândul de stoc unificat cu postarea 3xx nu s-a măsurat; limita superioară a
economiei e derivabilă: din 283.498 de rânduri de stoc, 169.376 (NotaTransfer,
fără rând contabil) și 6.947 (deschideri) n-au pereche contabilă, deci
unificarea ar scoate cel mult ≈107.000 de postări (9 % din cub).

## 3. Reconstruibilitatea, probată pe cifre

La ×1, cu dump-uri sortate și diff-uite (`pas2/egalitate.md`):

- **8 din 13 reconstruiesc raportul de azi cu cifre identice**: balanța
  sintetică, fișa 4111 (potrivire de multiset pe `(Data, Sens, Debit, Credit,
  DocumentId)` — `SoldCurent` exclus fiindcă depinde de ordinea ferestrei,
  `Contrapartida` fiindcă pe cub e lista conturilor tranzacției, `Storno` și
  `NumarNota` fiindcă nu există în modelul de măsurare; soldul final
  4.951.749,02 identic), jurnalul de cumpărări, D394 (agregatul), FIFO
  produs × gestiune, PhysicalStock, GLE (rândurile lunii), citirea pentru
  storno. `Cota` normalizată la două zecimale pe ambele părți (scară, nu cifră).
- **4 probează fidelitatea transformării, nu reconstruirea raportului de azi**:
  balanța × partener, fișa × partener, soldul pe partener și terții SAF-T
  s-au comparat cu F0 RESCRIS cu regula cubului (partenerul de pe oricare
  latură), fiindcă azi sunt cheiate pe REPARTITORUL LATURII, care pe 4111 e
  `UnitateInterna`/`ContPropriu` în 138.414 din 144.239 de rânduri: 1.985 de
  chei cont × repartitor contra 20.190 cont × partener; `SolduriPerioadaContabil`
  nu s-a comparat niciodată direct. Ce probează: aceeași regulă aplicată pe
  registru și pe cub dă aceleași sume. Ce NU probează: că raportul de azi iese
  din cub — nu iese, fiindcă azi grupează de zece ori mai grosier decât
  partenerul (proba numerică a amendamentului 4 din pasul 1).
- **1 diferă** — partidele cu rest, din trei mecanisme verificate: (1) restul
  cubului e soldul TUTUROR conturilor de terț ale documentului, al lui F0 e
  `TotalStingere − Asignat` — pe 2.072 de facturi de ieșire cu avans pe 419
  cubul compensează în partidă (16,61), F0 raportează brutul (104,02); (2) pe
  Flax datoria față de furnizor e împărțită prin conexul FCT → NIR (NIR-ul
  creditează 401 cu netul, factura doar cu TVA-ul): în cub partida se
  deschide pe documentul care POSTEAZĂ, deci 16.942 din 17.793 de facturi de
  intrare au partide separate; (3) semn pe ReturClient (convenție de dump).
  1 și 2 sunt aceeași întrebare — **ce e „o partidă": documentul sau postarea
  pe contul de terț** — de tranșat la pasul 3.

## 4. Ce amendează designul (de scris în decizie)

1. **Forma fizică**: o tabelă logică `Postare`, partiționată LIST pe
   coordonata `Spatiu` (Contabil | Stoc | Fiscal), coloane tipate (nu EAV, nu
   JSONB), `Tranzactie` separată, FK-uri per partiție pe coloanele polimorfe.
   Partiționarea pe an rămâne opțiune (FZ-r6).
2. **Indexii**: identitate (`TranzactieId`, `DocumentId`, `LinieId`) + per uz,
   proiectați din interogări și ținuți doar dacă planul îi atinge; niciun
   index per coordonată; niciun index-atelier pe `Cont`.
3. **`Sold`** (§7 al designului) e o singură tabelă pe toate coordonatele, per
   spațiu, la granițele de perioadă, cu proba de egalitate ca gate; ține ce
   azi stă în trei snapshot-uri și `PartideDeschise`. Dacă granul lui e și cel
   al citirii sau se adaugă un read model mai grosier se decide pe FZ-r1.
4. **Împerecherea ca tranzacție proprie, datată** (`Fel` = Împerechere) e
   PROPUNERE NETESTATĂ de lot: două postări pe același cont și aceeași latură,
   −Suma pe partida stingătorului, +Suma pe partida stinsă, ca Σ per (`Cont`,
   `Latura`) să rămână neschimbată. Gate-ul trece (a)–(d) cu 44.448 de
   asemenea tranzacții, dar e gol pentru ele (0 = 0 prin construcție) și nicio
   interogare nu le vede (toate datate 2026-09-18). Review-ul a arătat ce nu
   vede gate-ul: pe 1.200 de împerecheri contul de terț al documentului stins
   e ALTUL decât al stingătorului (partida s-ar închide pe un cont pe care n-a
   postat); 1.179 documente stinse n-au nicio postare de terț (cubul ar inventa
   partida); `Valoare` semnată pe aceeași latură face rulajele brute per
   partidă ne-sume (legal pentru storno în roșu, de declarat pentru
   împerechere). Se tranșează la pasul 3, împreună cu §3 (ce e o partidă).
5. **`Partener` stă pe postarea de terț și pe perechea ei** (regula „de pe
   oricare latură" a rândului de azi) — necesară ca soldurile pe partener să
   fie sume (§3); cazurile partener–partener (compensări, note) și decontul cu
   angajat rămân de probat pe cifre la pasul 3.
6. **Lățimea și discul**: ≈182 B/postare, cub 1,5–1,8× discul registrelor de
   azi — acceptat și declarat; contrapartida e o singură formă de scriere, o
   singură tabelă de solduri și 9 FK-uri în loc de 24.

## 5. Constatări colaterale, cu cifre

- **Artefact de date pe Flax**: toate cele 44.448 de `Imperecheri` au `Data`
  = 2026-09-18 (data rulării importului), deci `PartideDeschise` la 12/2025
  arată 24.937 facturi de ieșire integral neîncasate (77 M) deși au
  împerechere. Corect față de „împerecherea e fapt datat" (88); cifra de
  raport e a importului, nu a realității — de semnalat conectorului 1C.
- **Capcană Postgres**: `pg_get_indexdef` pe un index al unei tabele
  partiționate întoarce `CREATE INDEX … ON ONLY`, deci recreează doar
  învelișul părintelui, fără indexii partițiilor. A lăsat f2/f3 fără
  identitate la ×10 într-o primă rulare (citirea unui document 196,6 ms în
  loc de 0,7); găsit, reparat, proba păstrată (`pas2/out/raw-x10-fara-identitate/`).
- **Costul de scriere al F0 e în FK-uri**: 16 coloane plate de dimensiune cu
  FK propriu = 24 de triggere = 82 % din timpul de inserție la cald (58 % în
  rularea rece a explorării 01).
- **Balanța „analitică" de azi nu e pe partener** (§3): cheia e repartitorul
  laturii; pe 4111 asta înseamnă sediul, nu clientul.

## 6. Restanțe (FZ-r)

- FZ-r1 `Sold` contra snapshot-urile de azi pe același gran (și dacă un read
  model mai grosier — fără `Unitate` — merită ca al doilea): nemăsurat; F0 cu
  referință bate F2+S pe balanța sintetică (57 contra 99 ms).
- FZ-r2 fișa contului de la `Sold` + postările de după granița anterioară —
  **MĂSURATĂ 2026-09-19** (§2 (4)): 1.726 → 348 ms la ×10, egalitate 0
  abateri; F0 cu referință 248 ms. Ce rămâne din ea trece în FZ-r1 (granul
  lui `Sold`).
- FZ-r3 lookup-ul per partidă (SAF-T Payments, fișa unei partide): index
  `(Unitate, Data)` pe Contabil de probat pe o interogare reală.
- FZ-r4 creșterea reală a coordonatelor pe un istoric lung: `Sold` la 1,2 GB
  și toate cifrele +S sunt limite inferioare; de re-măsurat pe ani reali.
- FZ-r5 rândul de stoc unificat cu postarea 3xx: economie ≤ 9 %, decizie de
  modelare la pasul 3; la fel `TipStoc` pe sink-ul de consum.
- FZ-r6 partiționarea pe an: redeschisă când citirile integrale de istoric
  trec pragul acceptat pe baza reală — fișa nu mai e motiv (F3+S 328 ↔ F2+S
  348 ms, FZ-r2); rămâne PhysicalStock.
- FZ-r7 reperul `TaxInformation` din GLE (`LinieId` contra `PerioadaDeclarare`)
  — întrebare de design SAF-T, nu de fizică.
- FZ-r8 costul FK la scară, încărcarea în bloc, `VACUUM`/bloat sub scrieri
  reale, interogări concurente cu operarea serializată.
- FZ-r9 BRIN pe `Data` reevaluat pe forma cu `Sold` (ferestre de un an).
- FZ-r10 împerecherea ca tranzacție: cele trei rupturi din §4.4, pe date cu
  împerecheri datate real.
