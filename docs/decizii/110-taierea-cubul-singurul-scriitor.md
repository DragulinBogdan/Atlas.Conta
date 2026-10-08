# 110 — Tăierea: cubul e singurul scriitor și singura sursă; registrele, regimul dual și oracolul lor au ieșit (TR-D9a)

- Data: 2026-10-07
- Stare: **aprobată de owner, 2026-10-07**. Review-ul advers Codex al închiderii e închis (D9-F-R1…R4 corectate și reverificate; IZ-R1 și IZ-R2 închise). Proba `PerfCub` pe clone diferite e acceptată de owner ca abatere de la „aceeași bază” din regula de oprire 6. Amendează 108 (a), (g), (h), (j) și 090 (a), (l); precizează D9-A2 (ordinalul perechii la storno); închide S-r6, T-r7, T-r13, TR-r9, X-r2, 109-r1, T-r9, 63f, 64h, 86-r13, F27-r11, 098-r3; nu decide 111. (i) precizată de 113 (lanțul de migrații are și `RegulaContareFaraPastreazaSemn`); B-r3 închisă prin 112, 110-r1 prin 113
- Docs: contractul `docs/nucleu/tr-d9-taierea-contract.md` cu amendamentele 1–3; pașii `docs/nucleu/tr-d9-inventar.md`, `tr-d9-pas2-probe.md`, `tr-d9-pas3-retintire.md`, `tr-d9-pas5b-probe.md`, `tr-d9-pas5c-imbinare-partide.md`, `tr-d9-pas6-taierea.md`, `tr-d9-pas6b-repartitor.md`, `tr-d9-pas6c-spargere-modelcheck.md`, `tr-d9-pas7-declaratii.md`, `tr-d9-pas7b-pereche.md`, `tr-d9-pas7c-versiunea-politicii.md`, `tr-d9-pas8-inchiderea.md`; `docs/stare-curenta/` (toate paginile); dovezile în `run-nucleu/tr-d9a/` și `run-verificari/d9-pas8-*`

## Regula durabilă

**Operarea unui document are un singur efect contabil: tranzacțiile contractului, scrise în cub. Nu există al doilea scriitor, al doilea sold și nici un regim care să aleagă între ele.**

(a) **Un singur scriitor, o singură sursă.** `RegistruContabil`, `RegistruStoc`, `RegistruTva`, `RegistruImobilizari`, `PosteazaInCub`, `TotalStingere`, `LaturaContPropriu`, `StocService` și planul de operare al registrelor nu există. Numele lor sunt interzise ca referință de cod în `nou/`, C# și TypeScript (`--probe-sursa`, proba numelor interzise); excepțiile sunt o listă închisă: fișierul probei și codul generat. Lista nominală X-D2 păstrează numai legătura `Imperechere`. Amendează 108 (a).

(b) **Orice tip operabil declară.** Un tip fără declarant sau fără politica pe care declarantul o cere pe profil e refuzat cu `TIP_FARA_DECLARATIE`, pe toate ușile, înaintea oricărui efect. O regulă de contare pe un tip al cărui declarant nu contează prin reguli e refuzată la editare (`REGULA_CONTARE_FARA_CONSUMATOR`). 108 (h) rămâne fără obiect: nu mai există steag de stins.

(c) **Valoarea liniei de ieșire e decizia contractului.** Are două surse numite: evaluarea din soldul unității (`ValoareIesire`) și valoarea declarată de o sursă numită (`ValoareDeclarata`: linia documentului, recepția facturii). Linia, postarea și explicația poartă aceeași cifră, cu semnul cantității (`CITIRE_EXPLICATIE_LINIE`). `Lot.PretUnitar` e preț de intrare, dată a lotului scrisă o dată: nu evaluează nicio ieșire din sold. ASM cere P = C; altfel `ASAMBLARE_NEBALANSATA`, până la redistribuire. Amendează 090 (a) pe acest punct.

(d) **Corectitudinea o poartă catalogul de scenarii și invarianții interni ai cubului.** Nu mai există reconciliere registre ↔ cub și nici acoperire registru → cub. `INV-CUB` verifică, pe faptele fiecărei scene și cu un mutant pe fiecare ramură: echilibrul pe tranzacție și carte, conservarea transferului, proveniența inverselor, calificarea fiscală, taxa postată = taxa liniei, unitățile de partidă, proveniența fișelor, explicația (ieșirile = deciziile, soldul citit, FIFO, declarantul, valoarea liniei) și perechea. O aserție de regulă citește cititorii cubului și rezultatul persistat, nu declarația recalculată. Amendează 108 (g) și (j).

(e) **Cifrele citite din cub cer un singur drept: citirea completă pe `Postare`.** Subiectul e unic pe cele trei porți (proiecții, OData, XAF) și pe verificarea închiderii. Evidența XAF e o singură listă, „Registre → Postări", pe proiecția de citire `PostareVizual` (view SQL, `ServerView`, fără navigații), cu dreptul judecat pe `Postare` la activare. O postare existentă nu se rescrie (`POSTARE_MODIFICATA`); inversa fiscală a unei corecții se naște finală.

(f) **Analiza obligatorie se judecă pe postare, iar repartitorul e al capătului.** Pe piciorul de terț al unui cont care cere repartitor postarea poartă partenerul, și când contul nu urmărește partide; gardul nu citește latura documentului. Recepția liniei de stoc a facturii e în domeniul gardului.

(g) **Perechea e ordinalul mișcării.** `Postare.Pereche` numără mișcările în `Operare` și mutările în `Transfer`; cele două postări ale aceleiași mișcări îl poartă, și numai ele. Transformările și deschiderea n-au ordinal. La storno, ordinalul e al originalului pentru prima tranzacție sursă și se decalează cu maximul surselor dinainte pentru celelalte: stornoul e o singură tranzacție peste mai multe surse, iar ordinalele lor încep toate de la 1. Nu e coordonată: nu intră în chei de sold, în snapshot-uri sau în agregări. Precizează D9-A2.

(h) **Explicația reține regula care a decis.** Fiecare rând de politică din care declarantul a luat un fapt folosit la o postare intră ca `VersiunePolitica(Fel, Rand, Versiune)`, cu contorul rândului la operare; `ContRezolvat` numește regula de contare. Cititorul arată dacă rândul s-a schimbat de atunci. Contorul crește pe toate ușile de scriere ale produsului. Explicația e în formatul 2, fără cititor pentru formatul 1.

(i) **Schema e greenfield.** Lanțul de migrații e `InitialCreate` plus view-ul `PostareVizual`; partițiile, cheile compuse, indecșii cititorilor și funcția `cub_partida_id` sunt SQL scris de mână în el. O bază care nu corespunde codului se recreează (102 b).

(j) **Limitele consemnate** (secțiunea „Limite" de mai jos) nu sunt promisiuni ale tăierii și nu se închid prin tăcere.

(k) **Execuția după tăiere.** Regimul dual per `TipDocument`, maparea fizicii registre → cub ca oracol și cele trei oracole din 090 (l) au ieșit; proba supremă e catalogul (091). Urmează TR-D9b (unitățile), cu poarta de decizie la contractul ei (111). Amendează 090 (l).

## Context

090 a hotărât cubul de postări și a lăsat tăierea motorului vechi la TR-D9. TR-D7 a dus tipurile pe cub sub regim dual, iar TR-D8 a mutat toți cititorii pe intrările comune ale cubului (108). La pornirea feliei, registrele mai erau scrise de scriitorul dual și comparate cu cubul de un oracol; nimeni nu le mai citea ca sursă.

TR-D9 s-a împărțit în două (D9-Q1 = A): TR-D9a, tăierea, și TR-D9b, unitățile (reevaluarea, `Atribuit` ca mecanism de produs, valuta, `PoliticaEvaluare`, costul vamal pe lot). Decizia aceasta închide TR-D9a.

Regula de ordine a contractului a fost una singură: întâi se portează consumatorii, sub regimul dual; simbolurile dispar la sfârșit, într-un pas atomic.

## Ce s-a făcut, pe pași

| Pas | Ce | Commit |
|---|---|---|
| 0 | contractul, review advers Codex (D9-RV1…RV9), tranșările D9-Q1…Q5 | `88b45a7` |
| 1 | inventarul nominal, cu constatările I1…I8 | `a5df5fe` |
| — | amendamentul 1 (D9-A1…A9), după consultarea cub vs registre | `409054d` |
| 2 | gardul analizei pe mișcările contractului, probele dinaintea tăierii; G1 și G2 | `efd1c3c` |
| — | amendamentul 2 (D9-A10, D9-A11) | `717c3b5` |
| 3 | aserțiile de regulă re-țintite pe cititorii cubului, cu aceleași cifre | `a9516bd` |
| 4 | consumatorii din produs pe cub; dreptul unic; lista XAF `Postare`; gardul scrierii | `f136d08` |
| 5 | Import1C pe cititorii cubului; Migrare și BackfillTva șterse | `8be5b8b`, `c28251d` |
| 5b | probele dinaintea tăierii: reconcilierea arhivată, `PerfCub` termenul A, scara de volum | `1178b9d` |
| 5c | îmbinarea partidelor pe chei nulabile, în cinci locuri | `eafbc34` |
| 6 | tăierea: motorul nu mai scrie, nu mai citește și nu mai ramifică pe registre și pe regim | `9580b8b` |
| 6b | repartitorul pe capătul de terț, gardul strict | `535cdbd`, `ea5d0bc` |
| 6c | `Program.cs` din ModelCheck spart în `Suita` + `Suita/*.cs`, secvență identică | `3335847` |
| — | amendamentul 3 (D9-A12): lista de evidență pe view | `2b8de1b`, `1ff82df` |
| 7 | declarațiile dispar atomic; `InitialCreate`; lista pe `PostareVizual` | `bb2f05f`, `6855658` |
| 7b | cheia de pereche pe postare | `69fc028` |
| 7c | regula care a decis, în explicație | `85fcd5b`, `46916d9` |
| 8 | închiderea | commit-ul acestei decizii |

Review-urile adverse Codex: contractul (închis 2026-10-05), lotul 3–5 (D9-L35-R1, R2, închis 2026-10-06), lotul 6 + 6b (D9-6B-R1, închis 2026-10-07). Pașii 7, 7b, 7c și 8 sunt obiectul review-ului de închidere.

## Cele opt schimbări de comportament declarate (D9-D1)

Lista a fost închisă; orice altă schimbare ar fi fost defect. Toate au rânduri de catalog scrise înaintea codului.

1. Valoarea liniei de ieșire evaluate din sold vine din decizia contractului (D9-D3).
2. ASM cu P ≠ C e refuzat până la redistribuire (D9-D3).
3. Cifrele citite din cub cer citirea completă pe `Postare` (D9-D9).
4. Tipul inert pe profil e refuzat la operare, nu mai rămâne „operat fără efect" (D9-D5, I6).
5. Recepția liniei de stoc a facturii e păzită de gardul analizei la operarea facturii (D9-A3).
6. Regula de contare fără consumator e refuzată la editare (D9-A4).
7. Explicația reține regulile de politică consumate, cu contorul rândului; formatul 2 (D9-A8).
8. Partenerul pe piciorul de terț al conturilor care cer repartitor; gardul strict pe postare (D9-A10).

## Regula de oprire, punct cu punct

| # | Condiția | Starea | Proba |
|---|---|---|---|
| 1 | niciun nume interzis în `nou/` | îndeplinită | `--probe-sursa` 11/11; proba numelor interzise: zero referințe, zece mutanți la cifra așteptată |
| 2 | integrala verde pe ambele profiluri, pe baze din `InitialCreate`; numărul de `Check` reconciliat | îndeplinită | 4.803 privat / 3.524 bugetar, zero FAIL, pe baze create la zi; reconcilierea numărului în `tr-d9-pas8-inchiderea.md` §2 |
| 3 | catalogul verde; cifrele neschimbate în afara celor opt schimbări | îndeplinită | catalogul rulează în integrală; diferențele pe pași sunt numai adaos sau rânduri rescrise nominal |
| 4 | fiecare refuz al planului vechi are rând în inventar | îndeplinită | `tr-d9-inventar.md` §3–§6, măsurat la pasul 2 |
| 5 | nucleul; `refuzuri.ps1` de două ori; gardul pe `Postare`, `Tranzactie`, `PostareVizual`; drift; metadata; TPH | îndeplinită | nucleu 190/190; `refuzuri.ps1` 318/318 de două ori la rând; `STR-VIZUAL-*` și cazurile gardului în integrală; `verifica:drift` zero; metadata la zi; TPH 111 interogări, zero încălcări pe ambele baze |
| 6 | scara `PerfCub` își păstrează criteriile de formă; `PartideCuRest` închis sau re-amânat; costul comenzii A/B | îndeplinită, cu abaterea acceptată de owner (2026-10-07) | A/B este pe clone diferite, nu pe aceeași bază cerută de contract (D9-F-R4); owner-ul a acceptat proba alternativă, cu limita ei; termenul B: 1.540 privat / 1.010 bugetar OK, zero FAIL, `PREST-NI` amânat ca în A; nicio comandă de scriere nu face mai multe comenzi SQL, iar 69 din 73 de perechi fac mai puține; `PartideCuRest` re-amânat explicit, cu cifra |
| 7 | `stare-curenta/` și invarianții nu mai descriu regimul dual | îndeplinită | ultimele două mențiuni (PIF, AMO) scoase la pasul 8; invarianții I, III, VI în literă curentă de la pasul 6 |
| 8 | review-ul advers Codex e închis și decizia 110 e scrisă | îndeplinită | decizia e scrisă și aprobată; D9-F-R1…R4 corectate și reverificate; mutanții suplimentari confirmați de Codex; IZ-R1 și IZ-R2 închise (2026-10-07) |
| 9 | probele dinaintea tăierii arhivate; scara de volum raportată owner-ului înaintea pasului 6 | îndeplinită | `tr-d9-pas5b-probe.md`, raportată 2026-10-06 |
| 10 | invariantul perechii verde în nucleu și în `INV-CUB`; nicio postare existentă nu ajunge modificată la commit | îndeplinită | pasul 7b; `POSTARE_MODIFICATA` și inversa fiscală născută finală (pasul 6) |
| 11 | probele versiunii politicii verzi | îndeplinită | SC-CIT-111 pe ambele profiluri și pe HTTP (pasul 7c) |
| 12 | niciun document de catalog acceptat înaintea pasului 6b nu e refuzat de gardul strict; postările care au căpătat partenerul sunt numite | îndeplinită | pasul 6b: zero postări de catalog capătă coordonata; SC-PLT-08/09, SC-INC-09 |

Felia e închisă (2026-10-07): toate cele douăsprezece condiții sunt îndeplinite, rândul 6 cu abaterea acceptată de owner.

## Probele închiderii

Detaliul e în `docs/nucleu/tr-d9-pas8-inchiderea.md`. Pe scurt:

| Probă | Rezultat |
|---|---|
| Integrala ModelCheck, după corecturile D9-F-R1…R4 și review-ul lor | 4.828 privat / 3.549 bugetar OK, zero FAIL; `20261007-223433-979`, pe clone noi `.ClaudeD9Rv2` |
| Nucleu | 190/190 |
| `--probe-sursa` | 11/11 |
| `refuzuri.ps1` pe host viu, bază privată nouă din seed | 318/318 de două ori la rând; `neexpunere-cub.py` 0 FAIL; `explicatii.py` 9 PASS |
| `--dump-integritate-tph` | 111 interogări, zero încălcări pe ambele baze |
| `verifica:drift`, `--dump-metadata` | zero; la zi |
| `PerfCub`, termenul B | 1.540 privat / 1.010 bugetar OK, zero FAIL; comenzile de document fac cu 6 până la 16 comenzi SQL mai puțin și durează cu 11–24 % mai puțin, iar nicio comandă de scriere nu face mai multe; cifrele de control ale cititorilor sunt identice |
| Browser: operarea și lista „Registre → Postări" | factură operată din ecranul XAF; lista arată cele 4 postări ale ei, cu coduri, fără acțiuni de scriere |
| Import1C, rularea-diagnostic pe ianuarie | 15.232 de documente, zero eșecuri de import, `INV-CUB` verde pe 82.142 de postări; 10 rânduri FAIL de reconciliere față de 9: contractul 1 arată acum dublarea reclasificării (110-r2), contractul 5 are 5 partide neexplicate față de 8 |

## Scara de volum (D9-A1)

Raportată owner-ului înaintea tăierii, pe scena `PerfCub` privată multiplicată la 5.001.179 de postări (`tr-d9-pas5b-probe.md` §3, re-măsurată după corectură în `tr-d9-pas5c-imbinare-partide.md`):

- ruta securizată nu costă la volum: câteva milisecunde peste ruta de sistem, la orice treaptă;
- balanța, soldurile de parteneri, registrul jurnal și soldurile de loturi cresc liniar și rămân sub două secunde;
- snapshot-ul nu scurtează balanța la acest volum;
- partidele cu rest și snapshot-ul de partide creșteau pătratic, din îmbinarea soldurilor cu originile pe chei nulabile. Corectată în felie, la pasul 5c: partidele cu rest trec de la peste 10 minute la 6 s, reconstrucția snapshot-urilor de la „oprită la 30 de minute" la 48 s, la 651.644 de partide;
- criteriul de formă al `PartideCuRest` (documentul deschizător citit fără parcurgerea tuturor postărilor de partidă) rămâne picat și e **re-amânat explicit de owner, cu cifra**: 2,7 s de SQL pentru 488.733 de partide cu rest. Ține până la decizia 111 (F27-r16, F28-r1).

## Limite

Din D9-A9:

- registrele derivate nu se construiesc; se pot deriva din cub ulterior. Cheia de pereche face forma pe perechi derivabilă;
- cheia de pereche nu acoperă transformările (consumul și produsul sunt corespondență de grup) și nu e citită încă de nimeni (D9-r4);
- „numai cubul" nu e literal la imobilizări: fișa ia metoda, durata și categoria din liniile documentelor;
- corecția de preț fără diferență de cantitate nu postează nimic; corecția de preț după consum e a TR-D9b;
- Custodia și ALOP sunt extensii separate; tăierea nu le probează;
- explicația reține care regulă a decis și dacă s-a schimbat, nu și cum arăta la operare (D9-r3);
- poziția fără unitate rămâne cum era: nota contabilă poate lăsa un sold pe cont în afara unităților, iar diferența față de loturi o semnalează numai reconcilierea SAF-T (D9-A7, NG-r4).

Adunate în felie:

- materialul din regulă nu ajunge pe postare: o regulă a clientului cu material fix, pe un cont editat să ceară `Material`, e refuzată de gardul nou; niciun cont din seed nu cere `Material` (D9-A11);
- gardul analizei nu păzește mutările și transformările (BTR, ASM), nici înainte, nici după (D9-r1);
- trezoreria cu ambele conturi explicite pe regulă: piciorul de terț se alege structural, iar gestiunea proprie pe asemenea reguli rămâne limită (review-ul D9-6B-R1);
- nota contabilă operată fără nicio postare nu mai e văzută de niciun invariant, iar acoperirea politicii de mișcare SAF-T se probează într-o singură direcție (110-r5);
- lista „Registre → Postări": valuta fără etichetă, eticheta lotului ne-unică, fără navigație spre tranzacție; sortarea pe o coloană de cod scanează tot cubul (9,1 s la 5 milioane de postări), iar pe containerul Postgres de dezvoltare, cu `/dev/shm` de 64 MB, sortarea paralelă pică. Fidelitatea istorică a etichetelor nu e cerință (D9-A12);
- schema recomprimată a pierdut `DEFAULT false` pe 17 coloane booleene adăugate cândva prin `AddColumn`; EF scrie mereu valoarea;
- stornoul peste mai multe surse decalează ordinalele perechii, deci ordinalul stornoului nu e literal al originalului (litera g);
- contorul din explicație nu vede o scriere SQL directă pe rândul de politică și nici restaurarea bazei; politica de amortizare se reține pe orice fișă atinsă care are rând, fără să spună care cont a venit din ea;
- `POSTARE_MODIFICATA` judecă starea entității, nu un `UPDATE` direct în SQL; refuzul `TIP_FARA_DECLARATIE` nu intră în regimul pe stare, deci butonul de operare rămâne activ pe un tip inert;
- Import1C: conectorul rulează pe cititorii cubului, dar reconcilierea lui pe ianuarie nu e verde. Contractul 1 are trei conturi fără explicație (371, 3028, 303, cu exact sumele notei-punte a reclasificării, 110-r2), iar contractul 5 are 5 partide inițiale neexplicate, altele decât cele 8 dinaintea tăierii și neatribuite unei schimbări cu nume. Diagnosticul se urmărește în felia de migrare (091-r4); pentru contractul 5 cauza rămâne neatribuită, inclusiv față de schimbările motorului. Import1C nu este gate-ul tăierii.

Forma văzută de utilizator a fiecărei limite e în `docs/stare-curenta/limite-curente.md`.

## Amendamente la decizii anterioare

| Decizia | Punctul | Ce se schimbă |
|---|---|---|
| 108 | (a) | lista nominală nu mai are scriitori duali, martori, mapare sau evidență XAF a registrelor: rămâne numai legătura `Imperechere` |
| 108 | (g) | acoperirea cubului nu se mai compară cu registrele; verificările din `Invarianti.Verifica` sunt cele interne (litera d). `CITIRE_ISTORIC_STOC_INCOMPLET` și comparația pe grup și proveniență au ieșit |
| 108 | (h) | fără obiect: `PosteazaInCub` nu există |
| 108 | (j) | reconcilierea (a)–(g) și diagnosticul ASM-B7 au ieșit odată cu al doilea scriitor; ultima rulare e arhivată în `tr-d9-pas5b-probe.md` |
| 090 | (a) | `Lot.PretUnitar` rămâne, ca preț de intrare; nu evaluează ieșiri din sold (D9-Q5) |
| 090 | (l) | regimul dual ca dată, maparea fizicii ca oracol și cele trei oracole au ieșit; tăierea a fost greenfield, cu bazele recreate |
| D9-A2 | inversa | ordinalul originalului plus decalajul tranzacției sursă (litera g) |

108 (b)–(f), (i), (k) rămân neschimbate.

## Verdictul restanțelor (D9-D13)

Scris în `docs/decizii/restante.md`. Pe scurt:

- **închise prin tăiere**: S-r6, T-r7, T-r13, TR-r9, X-r2, 109-r1, T-r9, 63f (fără obiect), 64h, 86-r13, F27-r11 (cu proba pe `Partener` din pasul 6b), 098-r3 (cu SC-X-27; jumătatea „refuzată azi" nu exista), IM-r1, IM-r3;
- **rămân active, TR-D9b**: 75-r4, N-r5, T-r2, 86-r1, B-r6, 107-r4, 51e, 097-r2 și hook-urile de stingere 76-r1…r3, 86-r11, T-r5;
- **rămân active, până la 111**: F27-r16 (criteriul de formă, re-amânat cu cifra) și F28-r1;
- **după PoC**: 104-r2, 097-r3, F27-r12, F26-r5;
- **limită declarată**: T-r3;
- **noi**: D9-r1, D9-r3, D9-r4 (după PoC); 110-r1…110-r5. D9-r2 nu se deschide: regula de contare fără consumator e refuzată la editare. Adăugată 2026-10-08, după PoC: 110-r6, izolarea probelor de politica reală. `ScenariiTvaIntervale` și `ScenariiFiscale` își șterg politica de TVA pe FCT cu un filtru care nu o leagă de tipul de TVA al scenei, deci șterg orice politică de TVA pe FCT care nu e din seed; `PoliticaTva` are cheia tip de document × direcție, fără nimic propriu scenei, așa că rețeta `CurataPolitica` de la D9-F-R2/R3 nu se transferă și corectura cere ștergere pe identitate. `explicatii.py` editează pe HTTP regula de contare reală din seed, pe o bază de unică folosință.

Rândurile de catalog „amânat la TR-D9" care privesc registrele (limita duală din SC-X-01, garda din NIR, SC-ASM-19) s-au închis la pasul 6; reevaluările și compensările (SC-X-05/06/07, SC-X-09, SC-X-13) trec la TR-D9b.

## Ce rămâne de hotărât de owner

1. Hotărât (2026-10-07): decizia e aprobată, proba PerfCub pe clone diferite e acceptată ca abatere de la regula de oprire 6, felia e închisă.
2. **B-r3**: recepția facturii n-are regulă de contare proprie. Motivul amânării a dispărut, dar NIR-ul conex rămâne documentul diferențelor, iar o regulă `FCT/Stoc` e schimbare de politică. La tăiere a rămas activă, cu destinația la owner. **Închisă 2026-10-08**: regulă proprie `FCT/Stoc`, decizia 112, mersă în main prin PR #23.
3. **110-r1**: `RegulaContare.PastreazaSemn` nu mai e citit de niciun declarant. Se scoate sau se leagă. **Închisă 2026-10-08**: câmpul e scos, decizia 113, mersă în main prin PR #24.
4. **110-r3**: coordonatele ne-nule cu `Guid.Empty`. Dimensionată; recomandată ca subiect al contractului TR-D9b / deciziei 111, nu al tăierii. Hotărât 2026-10-08: perimetrul se restrânge la cele 11 coordonate de sold, referințele și blocul fiscal rămân nulabile; restul se hotărăște la poarta 111 (pct. 6 din „Înaintea deciziei").
5. **Bazele de probă** care se pot șterge: cele `.P7b` și `.D9P8` ale acestui pas, plus cele vechi de pe schema dinaintea tăierii (`.D9P5b`, `.D9Vol`, `.D9VolProba`, `.D9P7`, `.Privat.D9P4`, `.Flax.R3f`, `.Flax.M1s`, `.Flax.R3`). Lista și rostul fiecăreia: `tr-d9-pas8-inchiderea.md` §8.
6. **111** rămâne propusă; poarta ei e contractul TR-D9b.

## Review

Review Codex la `aecd189` (2026-10-07): D9-F-R1…R4, toate P2; regula de oprire 8 rămâne deschisă.
Corecturi cerute de owner și implementate de Codex, fără commit:

- R1: forma transformărilor se verifică pe mulțimea postărilor fără ordinal; mutanți pentru pierderea
  perechii lângă transformare și pentru predicatele perechii nenule (`tr-d9-pas7b-pereche.md`).
- R2/R3: SC-CIT-111 folosește tip și regulă proprii; curățenie pe identitate, inclusiv `RefuzSeed`,
  probe după editare/ștergere și martor BCS străin (`tr-d9-pas7c-versiunea-politicii.md`).
- R4: rândul 6 raportează proba alternativă și aprobarea lipsă; contractul nu a fost amendat tacit.
- Atribuirea diferențelor Import1C contractul 5 rămâne deschisă; apartenența diagnosticului la migrare
  nu demonstrează excluderea motorului drept cauză.

Rezultatele corecturilor se consemnează în documentul pasului 8. Predarea corecturilor nu închide
singură review-ul; owner-ul și Claude reverifică înainte de commit.

Review-ul corecturilor (Claude, 2026-10-07): R1…R4 țin, reverificate independent. Două constatări din aceeași
clasă cu R2/R3 sunt corectate în aceeași schimbare: SC-X-26 purja regulile BTR după `DinSeed` și lăsa un refuz
de seed; bazele de dezvoltare purtau refuzurile vechilor probe. Cinci mutanți noi acoperă ramurile
predicatului perechii rămase fără probă pe PostgreSQL, cu izolarea verificată prin scoaterea fiecărei
disjuncții. Detaliul și cifrele: pasul 8 §11.

Închiderea (2026-10-07): Codex a confirmat cei cinci mutanți suplimentari fără constatări blocante și a găsit
două defecte în rețeta de izolare, IZ-R1 (un `FAIL` urmat de `OK` trecea drept ucis) și IZ-R2 (sursa era
modificată înaintea luării blocajului). Ambele sunt corectate și reverificate de Codex
(`comunicari/2026-10-07-2327-codex-claude-tr-d9a-izolare-iz-r1-r2-review.md`). Owner-ul a acceptat proba
PerfCub alternativă și a aprobat decizia.
