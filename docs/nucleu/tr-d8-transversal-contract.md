# TR-D8 — gate-ul transversal: contract pentru aprobare

- Data: 2026-10-01
- Stare: propus (branch `tr-d8-transversal`, tăiat din `main` = `02f7788`,
  după PR #14); aprobarea D-urilor și răspunsurile X-Q1…X-Q4 sunt ale
  owner-ului; review advers al contractului cerut lui Codex.
- Bază: [D8-B5](tr-d8-citiri-contract.md) pasul 5 și „Limite care împiedică
  închiderea TR-D8" din [review-ul propriu](tr-d8-review-codex.md); 090 (i)(j),
  091 (g)(4)(5), 091-r3; decizia 105 §„Ce rămâne deschis";
  restanțele cu starea `activă, TR-D8` din `docs/decizii/restante.md`.
- Închiderea: decizia **107** (106 este luată de `c106-regim-stare`, nemers;
  numerele nu se refolosesc).

## De ce o felie separată

Toate consumatoarele din [inventar](tr-d8-citiri-inventar.md) sunt portate
nominal (contabil, stoc, partide, fiscal, SAF-T, imobilizări, snapshot-uri).
Ce nu s-a făcut este ce niciun raport nu poartă singur: dovada că nimic din
producție nu mai citește registrele în domeniul portat, reconcilierea (a)–(h)
pe o bază întreagă, explicația deciziei persistată (S-r2), concurența pe două
conexiuni reale, costul cititorilor pe scară și diagnosticele regimului dual la
pornire. Fără ele, TR-D8 rămâne „portat raport cu raport", nu „închis".

## Ce spun sursele că înseamnă gate-ul transversal

| Sursa | Cerința | Litera de aici |
|---|---|---|
| D8-B5 (5) | auditul deciziei (S-r2), perf A/B pe aceeași bază, arhitectură, integral ambele profiluri, review advers și docs | X-D4, X-D5, X-D2, X-D8 |
| D8-B5 final | lista nominală a consumatorilor portați; niciun raport complet alimentat dintr-un cub incomplet | X-D2, X-D7 |
| D8-B4 / T-D10 | diagnosticul transversal (h) nu e finalizat; (a)–(g) toleranță 0 | X-D3 |
| review propriu §Limite | activarea stocului și politica modificată în mers; concurența pe lot (două conexiuni, consum–retragere) | X-D7, X-D6 |
| 091 (g)(4) | concurența probată cu două sesiuni reale pe același lot, pe aceeași partidă, operare simultană cu închiderea (F27-r8, S-r9) | X-D6 |
| 091 (g)(5), 091-r3 | accesul la `Postare` în afara cititorilor refuzat de test de arhitectură; `Transfer` exclus într-un singur loc (N-r8) | X-D2 |
| 090 (j), S-r2 | explicația deciziei persistată compact pe tranzacție; scenariile o asertează („de ce acest lot") | X-D4 |
| 090 (i), FZ-r1, SAFT-r4, FZ-r3, 103h | materializare doar pe cifră măsurată; granul `Sold`; balanța din snapshot; planul pe volum al lookup-ului de partidă; costurile R6 | X-D5 |
| T-r11, T-r13, T-r14, N-r8, 097-r1, 102-r4, S-r11 | restanțele marcate TR-D8 | X-D7 |

## X-D1 — Perimetrul și ce NU închide

TR-D8 se închide numai când sunt verzi împreună: X-D2 (arhitectura
registrelor cu lista nominală), X-D3 (reconcilierea pe baza de volum), X-D4
(explicația deciziei), X-D5 (scara transversală cu criteriile de formă), X-D6
(concurența), X-D7 (activarea și restanțele TR-D8), X-D8 (integrala, review-ul
advers, docs). Fiecare literă are probă în ModelCheck sau artefact în
`run-verificari/`; nicio literă nu se închide prin text.

Rămân explicit în afara feliei: tăierea scriitorilor vechi și a regimului
dual (TR-D9); stornoul după reevaluare N-r5 și `Atribuit` (091 (h), TR-D9);
evaluările noi ale reziduului dual T-r13 (TR-D9; aici numai diagnosticul
existent); probele de formă TR-r9 (rescrise când mor registrele); pragul
absolut de perf pe volum REAL (FZ-r3, migrare); 102-r5 (după PoC); B8-r*
și restanțele `după PoC`. Import1C rămâne înghețat (091-r4).

## X-D2 — Testul de arhitectură al registrelor și lista nominală (T-r11 pe întregul inventar)

`ProbeCititoriCub` (091-r3) apără azi numai rândurile cubului. Se adaugă
proba inversă: **niciun cititor de producție** (`Proiectii/`, `Api/`,
controllere WebApi/OData, `Culegere/`, `Saft/`, `Declaratii/`) nu citește
`RegistruContabil`, `RegistruStoc`, `RegistruTva`, `RegistruImobilizari` sau
`Imperechere` ca sursă de sold/rulaj/rest. Excepțiile permise sunt o listă
închisă, cu rolul fiecărei intrări, și se împart în trei clase:

1. **scriitori ai regimului dual** (`MotorOperare`, `StocService.R` pentru
   absorbția ASM, `CorectieService`, `LoturiCulegereService` cât timp culeg
   din registre — de verificat la prima rulare);
2. **martori și diagnostice** (`Invarianti`, `Imobilizari.VerificaAcoperire`,
   `RegistruTva` ca martor în `TvaProiectii` dacă mai există, `Asignari`);
3. **suprafețe de evidență XAF** ale registrelor (`ContaUiBaseline`, listele
   de registre): rămân până la TR-D9 și se declară ca atare.

Prima rulare a probei produce lista reală; fiecare intrare intră într-una din
clase sau este defect corectat în felie. Lista finală este „lista nominală a
consumatorilor portați" cerută de D8-B5 și se copiază în
`stare-curenta/dezvoltare-si-validare.md`. Regula N-r8 se probează pe fiecare
cititor comun: `Contabil` exclude `Transfer` și inversa lui, `Loturi` și
`Partide` le includ, niciun consumator nu refiltrează.

## X-D3 — Reconcilierea transversală (a)–(h) pe o bază întreagă

`--reconciliere-cub` rulează azi pe scenă (`STR-RECONCILIERE`, set restrâns)
și a rulat integral numai pe Flax, care nu mai există (102). Litera (h) e
livrată pentru delimitarea ASM, nu măsurată pe o bază întreagă. Se pin-uiește:

- (a) reconcilierea integrală (`documente = null`) rulează pe baza de volum
  sintetică din X-D5, la fiecare treaptă m (0, 6, 12 luni închise), pe ambele
  profiluri, **înaintea purjei**: (a)–(g) = 0 rânduri Δ, exit 0; (h)
  descompusă pe grupul-sursă și raportată în `run-verificari/`;
- (b) `Invarianti.Verifica` (INV-CUB) rulează pe aceeași bază, în același
  punct, nu doar pe scenele suitei;
- (c) diagnosticul ASM-B7 (lot × gestiune × cont, cu proveniență) rulează pe
  aceeași bază și își raportează pozițiile cu cantitate zero și valoare
  nenulă; nicio diferență nu se declară N-r3 automat;
- (d) întrebarea T-D10 „cine are dreptate, cubul sau registrul" pe datele 1C
  este a migrării (091-r4), nu a acestui gate; aici cubul și registrele sunt
  scrise de aceleași comenzi și orice Δ (a)–(g) este defect, nu consecință.

## X-D4 — Explicația deciziei persistată (S-r2, 090 j)

Fapt măsurat în cod: `Contractare.Contracteaza` întoarce `N.Contract` cu
`Decizii` (`AlocareFifo`, `ValoareIesire`, `PartidaDeschisa`, `ContRezolvat`,
`AbsorbtieEvaluare`), `Ipoteze` (`SoldUnitateCitit`, `PerioadaDeschisa`,
`VersiunePolitica`) și `JumatatiDeBan`; `Materializare.Opereaza` scrie numai
`contract.Tranzactii`. Explicația se pierde la fiecare operare.

Se pin-uiește:

- (a) **ce se persistă**: pentru fiecare tranzacție scrisă din contract
  (`Operare` și `Transfer` din aceeași declarație), lista ordonată a
  deciziilor și ipotezelor contractului, plus `JumatatiDeBan` și numele
  declarantului; pentru `Storno` originea este deja `InversaDin` și nu se
  duplică; pentru `Transfer` din împerechere (`Motor.Transfera`) și pentru
  `Deschidere` explicația este goală, declarat;
- (b) **forma** este alegerea owner-ului (X-Q1); recomandarea este o coloană
  `Explicatie jsonb` pe `Tranzactie`, scrisă în aceeași instrucțiune cu rândul,
  cu schemă versionată (`v`, `declarant`, `jumatati`, `decizii[]`,
  `ipoteze[]`), migrație scrisă în SQL (S-r4), append-only ca tot cubul;
- (c) **cititorul** `Cub.Citiri.Explicatii` întoarce înregistrări tipizate pe
  tranzacție și pe linie („de ce acest lot": alocările și valoarea ieșirii
  per unitate, soldul citit, versiunea politicii); intră în lista permisă
  091-r3; se expune read-only prin API (`GET` pe tranzacție), fără pagină React
  nouă (104d);
- (d) **invariantul de audit** (probă ModelCheck, ambele profiluri): orice
  postare de ieșire pe lot are în explicație o `ValoareIesire` a aceleiași
  linii și unități, cu Σ valoare egală cu valoarea postată; orice stingere
  FIFO are `AlocareFifo` per unitate; `SoldUnitateCitit` există pentru fiecare
  unitate consumată; storno-ul nu are explicație proprie; anularea fizică
  șterge explicația odată cu tranzacția (excepția declarată în 091, punctul
  5, de tranșat la TR-D9);
- (e) **scenariile** SC-CIT-96…99 (catalogul [CITIRI](scenarii/CITIRI.md)):
  „de ce acest lot" pe BCS din două loturi cu prețuri diferite, pe NTC cu
  stingere FIFO pe două partide, pe ASM cu `AbsorbtieEvaluare`, și citirea
  securizată (rândul refuzat nu lasă explicația să divulge valoarea);
- (f) dry-run-ul nu persistă nimic; S-r11 se închide aici: excepțiile de
  construcție (`InvalidOperationException` din `Contractare`,
  `ArgumentException` din nucleu) devin refuz cu cod stabil
  `DECLARATIE_INVALIDA`, 422 pe ușa API, nu 500.

## X-D5 — Scara transversală: costul cititorilor comuni, criterii de formă

Nu există bază de volum reală (102); o cifră se compară doar cu ea însăși pe
aceeași bază. Precedentul aprobat este B8-Q2 = A (scară sintetică) și
B8-RV3-P (rulare în container, `ANALYZE` înaintea măsurării, proces nou per
punct, rece/cald). Se pin-uiește, sub rezerva X-Q2:

- (a) **scena** `PerfCub` generalizează `PerfSaft`: aceeași unitate (FCT+NIR,
  PLT, BCS, BTR, DSC, ASM, FCL+INC) pe privat; pe bugetar unitatea fără DSC și
  cu ASM/LDI după disponibilitatea tipului; k ∈ {1, 4, 16, 64} unități în luna
  măsurată, m ∈ {0, 6, 12} luni închise × 16 unități, lotul „lung";
- (b) **ce se măsoară**, fiecare într-un proces nou, rece și cald, cu numărul
  comenzilor SQL, `ms`, `ms SQL`, rânduri citite, alocări, vârfuri: balanța
  lunii, fișa contului cu cele mai multe postări, jurnalul lunii, soldul
  partenerilor, `PartideCuRest`, raportul de stoc, disponibilul FIFO al unui
  BCS nou (costul operării), reconstrucția snapshot-urilor (contabil, stoc,
  partide), jurnalele TVA, D300, D394, SAF-T L și S (deja măsurate, se
  reiau pe aceeași bază), raportul de impact R6 (103h), `PartideDisponibile`
  (102-r5, numai măsurat), închiderea unei luni; fiecare pe ObjectSpace
  securizat (`Admin`) și nesecurizat acolo unde produsul are ambele căi;
- (c) **criteriile**, fără prag absolut: comenzi SQL constante în k și m
  (fără N+1); cititorii cu snapshot nu citesc istoricul (rândurile `Postare`
  la k fix nu cresc cu m); liniaritate f(4k) ≤ 1,25 × 4 × f(k) pe durată și
  alocări la cald; XSD/DUK pe SAF-T la k = 64; orice cititor care pică un
  criteriu se corectează în felie sau intră în `limite-curente` cu cifra;
- (d) **ce tranșează cifrele**: SAFT-r4 (balanța L/S pornește din snapshot
  când accesul e complet; se închide dacă rândurile citite devin independente
  de m), FZ-r1 (granul `Sold`: A/B snapshot contra recitire pe aceeași bază;
  se închide cu cifra, fără al doilea read model dacă nu o cere), FZ-r3
  (planul `(Unitate, Data)` la k = 64 și m = 12; volumul real rămâne
  migrare), 103h (costurile R6 primesc buget măsurat, nu estimat);
- (e) rularea: `ModelCheck --perf-cub` în containerul `dotnet/aspnet:10.0`
  din `perf-saft-container.ps1` generalizat, artefacte `run-verificari/perf-cub-*`
  (tabel, planuri `EXPLAIN (ANALYZE, BUFFERS)` la k = 64, `rulare.json` cu
  commit și hash DLL); raportul intră în `docs/api/p5-perf-masuratori.md`.

## X-D6 — Concurența pe două conexiuni reale (091 g4)

Blocajele existente: perioada este deja serializată (F27-D1: gardianul de
operare ia `FOR SHARE` pe rândul perioadei, închiderea ia `FOR UPDATE` ca
primă instrucțiune; două operări nu se blochează între ele); `FOR UPDATE` pe
tranzacția de deschidere și pe documentele stingerii (DES, în ordinea `ID`);
`pg_advisory_xact_lock` pe suport (97001), pe sursa recepției (97002) și pe
depuneri (fiscal). **Nu există blocaj pe lot și nici pe partidă**;
`VerificaSoldIntermediar` și `VerificaDisponibilTemporal` citesc și verifică
fără a ține rândul. Direcția fixată de 091: blocaj pesimist per unitate în
tranzacția de comandă, nu `Serializable` cu reluare.

Se pin-uiește:

- (a) **probele**, fiecare cu două `IObjectSpace`-uri pe conexiuni distincte,
  barieră între „a verificat" și „a scris", pe ambele profiluri: două consumuri
  pe același lot când încape unul singur; consum contra retragerea intrării
  (storno FCT/NIR); două stingeri pe aceeași partidă peste rest; operare
  contra închiderea perioadei ei (F27-r8); două documente noi pe același
  document-părinte pentru `Pozitie` (S-r9); două împerecheri pe aceeași
  pereche (101). Așteptarea: exact una trece, cealaltă primește refuzul de
  domeniu (nu excepție de unicitate, nu 500), iar cubul rămâne conservat și
  `INV-CUB` verde;
- (b) **mecanismul**: `Materializare` ia, înaintea verificărilor de sold,
  `pg_advisory_xact_lock` pe fiecare unitate atinsă (lot și partidă), în ordine
  deterministă (după `Guid`), ca să nu existe deadlock între două comenzi cu
  aceleași unități în altă ordine; blocajul perioadei rămâne cel din F27-D1
  și se probează, nu se dublează; `Pozitie` se atribuie sub blocaj pe
  document sau primește index unic cu refuz de domeniu; dry-run-ul nu ia
  blocaje;
- (c) proba de deadlock: două comenzi cu aceleași două unități în ordine
  inversă se serializează, nu pică;
- (d) 097-r3 (blocajul comun IMO) rămâne așa cum e; mecanismul general de
  aici nu-l înlocuiește în felie, se notează dacă îl poate absorbi la TR-D9.

## X-D7 — Activarea, regimul dual și restanțele TR-D8

- (a) **istoric de stoc incomplet**: `Invarianti.Verifica` acoperă azi
  contabilul (din `RegistruContabil`), deschiderea, partidele și fișele; se
  adaugă acoperirea cantitativă: fiecare rând `RegistruStoc` al unui tip cu
  `PosteazaInCub` are postare pe lot în cub cu aceeași cantitate și același
  storno, iar transferurile cantitative (BTR) au ambele capete; lipsa =
  `CITIRE_ISTORIC_STOC_INCOMPLET`;
- (b) **`PosteazaInCub` nu se stinge**: odată ce un tip are tranzacții în
  cub, trecerea pe `false` este refuzată de gardian (X-Q4), iar seed-ul nu o
  aliniază pe `false`; altfel un document nou ar produce fapte doar în
  registre și cititorii ar minți tăcut;
- (c) **T-r14**: `Transformare.FaraContrapondere` e aplicat în `Contabil`,
  `Partide` și `Invarianti`; se probează pe fiecare cititor comun (fișă,
  jurnal, GL, MovementOfGoods, `Loturi`) că contraponderile nu apar și că
  diagnosticul `Comparabil` le numără; restanța se închide cu probele;
- (d) **N-r8**: regula listării: jurnalul și fișa de cont nu arată `Transfer`
  și nici stornoul lui; fișa partidei și a lotului le arată ca rânduri cu
  eticheta felului; probă pe fiecare;
- (e) **097-r1**: `Imobilizari.VerificaAcoperire` există; se probează pe
  istoric fără fișă/origine/suport că refuză, și restanța se închide;
- (f) **102-r4**: alegerea owner-ului (X-Q3); recomandarea este refuz
  `IMPERECHERE_FARA_EFECT` pe ramura negativă a desfacerii automate, cu
  proba mutantului;
- (g) **T-r11**: se închide prin X-D2 (o singură intrare `Carte=Contabil`,
  probele cu DVI fiscală prezentă pe rulaje și solduri filtrate pe partener
  există în SC-DVI/SC-CIT; se listează în închidere);
- (h) **T-r13** rămâne activă (TR-D9): aici doar diagnosticul ASM-B7 pe baza
  de volum (X-D3 c).

## X-D8 — Integrala, review-ul advers și închiderea

Ordinea pașilor, un commit per pas pe `tr-d8-transversal`:

0. Contractul (acest fișier) → review Codex → aprobarea owner-ului pe
   X-D1…X-D8 și răspunsurile X-Q1…X-Q4.
1. X-D2: proba registrelor, lista nominală, N-r8 pe cititori.
2. X-D4: migrația SQL, materializarea explicației, cititorul, API,
   SC-CIT-96…99, S-r11.
3. X-D6: probele de concurență, blocajul per unitate, S-r9, deadlock.
4. X-D7: acoperirea cantitativă, gardianul `PosteazaInCub`, T-r14, N-r8,
   097-r1, 102-r4.
5. X-D5 + X-D3: `PerfCub`, reconcilierea integrală și INV-CUB pe baza de
   volum, măsurătorile în container, tranșarea SAFT-r4/FZ-r1/FZ-r3/103h.
6. Integrala ambele profiluri, `refuzuri.ps1` pe host viu, drift OpenAPI,
   review advers Codex prin `comunicari/`, decizia 107, `stare-curenta/`,
   `restante.md`, `istoric-plan-de-lucru.md`, CLAUDE.md §Stare.

Regula de oprire:

- un rând Δ pe (a)–(g) în X-D3 este defect, nu declarație; felia nu se
  închide cu Δ;
- o probă de concurență care nu are exact un câștigător și un refuz de
  domeniu este oprire până la corectarea mecanismului;
- un cititor de producție pe registre în afara listei nominale este defect;
- niciun prag absolut de perf nu se inventează; un criteriu de formă picat se
  corectează sau intră în `limite-curente` cu cifra și numele cititorului;
- ModelCheck roșu pe oricare profil = oprire; o singură rulare grea o dată;
  baza de volum se măsoară în container, nu prin proxy-ul Docker Desktop.

## X-Q — întrebările pentru owner

- **X-Q1** forma explicației deciziei: **A** = `Explicatie jsonb` pe
  `Tranzactie`, schemă versionată, cititor + `GET` read-only (recomandat:
  compact, o singură scriere, append-only, interogabil); **B** = tabelă copil
  normalizată `Decizie` (rând per decizie/ipoteză). Ambele fără pagină React.
- **X-Q2** gate-ul de perf: **A** = scara sintetică `PerfCub` cu criteriile de
  formă din X-D5 (c), fără prag absolut, în container (recomandat; precedentul
  B8-Q2 = A); **B** = gate-ul rămâne deschis până la o bază de volum reală
  (migrare), TR-D8 se închide fără cifre de perf.
- **X-Q3** 102-r4: **A** = refuz `IMPERECHERE_FARA_EFECT` pe ramura negativă a
  desfacerii automate (recomandat: o singură sursă de reguli, fără urmă
  tăcută); **B** = invariant `INV-CUB` „legăturile vii ale perechii nu depășesc
  efectul pe partide", desfacerea rămâne permisă.
- **X-Q4** `PosteazaInCub` după prima tranzacție în cub: **A** = ireversibil,
  refuz în gardian și seed (recomandat); **B** = permis cu diagnostic la
  activare (`CITIRE_ISTORIC_INCOMPLET` ar prinde documentele ulterioare).

## Ce NU intră (amânări cu nume)

- TR-D9: tăierea registrelor și a `DescarcareService`, N-r5/`Atribuit`,
  T-r13 evaluări noi, TR-r9, 097-r2, T-r2/T-r3/T-r5, B-r3/B-r6/B-r8/S-r3.
- Migrare (091-r4): contractul 1 al reconcilierii 1C, pragul real FZ-r3,
  bazele de volum reale.
- După PoC: 102-r5 (numai măsurat aici), ecranele explicației, F27-r8 în
  afara probei de închidere, B8-r*.
