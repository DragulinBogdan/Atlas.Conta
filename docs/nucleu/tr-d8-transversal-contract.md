# TR-D8 — gate-ul transversal: contract pentru aprobare

- Data: 2026-10-01
- Stare: **aprobat de owner, 2026-10-03** (X-Q1…X-Q4 = A/A/A/A; nivelul 3 al
  blocajului ales 2026-10-01); review-ul advers Codex al contractului e
  închis (X-RV1…X-RV7, inclusiv 1.1/1.2/5.1,
  `comunicari/2026-10-01-1400-codex-claude-tr-d8-transversal-contract-inchis.md`).
  Branch `tr-d8-transversal`, tăiat din `main` = `02f7788` (după PR #14).
  Implementarea pornește cu pasul 1 din X-D8, un commit per pas.
  **Pasul 1 (X-D2) implementat și verificat, 2026-10-03** — vezi „Execuție".
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
conexiuni reale, costul cititorilor pe scară și probele de acoperire ale
regimului dual (în ModelCheck, pe fapte, înaintea purjei — nu la pornirea
hosturilor, 102d). Fără ele, TR-D8 rămâne „portat raport cu raport", nu
„închis".

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

### Modelul perioadei rămâne cel din 088 (confirmat de owner, 2026-10-01)

Discuția despre „trasul de linie prin materializare, cu redeschidere logată
și reevaluare" s-a tranșat pe ce există: închiderea e comandă care
materializează soldurile pe perioadele de referință (088 b/d), snapshot-ul
se reconstruiește exact din postări (090 i) și nu devine tranzacție de
`Deschidere` lunară (094 interzice dublarea faptelor); redeschiderea e
explicită, cu motiv, logată, numai pe ultima perioadă închisă, în lanț
(088 a/c), și readuce perioada la regulile uneia deschise: retroactivul e
permis dacă nu produce stoc negativ în nicio zi, iar **valoarea ieșirilor
deja operate nu se rescrie** — reevaluarea la redeschidere ar fi mecanismul
`Atribuit` (090 k, N-r5) și rămâne TR-D9; fiscalul declarat nu se
redeschide, se regularizează (103). Corecția în perioadă închisă rămâne
storno legat + document nou (088 h). Felia nu atinge acest model; X-D5
dovedește consecința lui pentru cititori: după cutoff, citirea pornește din
snapshot și nu atinge istoricul.

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
clase sau este defect corectat în felie. O intrare numește **utilizarea**
permisă (fișier + metodă + registrul + rolul), nu absolvă un serviciu întreg
prin apartenența la o clasă; `LoturiCulegereService` sau `CorectieService`
intră cu metoda care scrie/culege din registru, iar orice altă citire a lor
rămâne încălcare. Lista finală este „lista nominală a
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

- (a) **ce se persistă**: explicația unui contract se scrie **o singură
  dată**, pe **prima tranzacție efectiv produsă** de contract — `Operare`
  când există, altfel `Transfer` (BTR produce numai `Transfer`, X-RV5.1); a
  doua tranzacție a aceleiași declarații poartă numai referința la ea
  (`ExplicatieDin`), ca nicio decizie să nu fie numărată de două ori
  (X-RV5). Conținutul: lista ordonată a deciziilor și ipotezelor, cu
  `Linie` și `Unitate` acolo unde le au, `JumatatiDeBan`, numele
  declarantului, versiunea schemei; pentru `Storno` originea este
  `InversaDin` și explicația este a originalului, nu se duplică; pentru
  `Transfer` din împerechere (`Motor.Transfera`) și pentru `Deschidere`
  explicația este goală, declarat;
- (b) **forma** este alegerea owner-ului (X-Q1); recomandarea este o coloană
  `Explicatie jsonb` pe `Tranzactie`, scrisă în aceeași instrucțiune cu rândul,
  cu schemă versionată (`v`, `declarant`, `jumatati`, `decizii[]`,
  `ipoteze[]`), migrație scrisă în SQL (S-r4), append-only ca tot cubul;
- (c) **cititorul** `Cub.Citiri.Explicatii` întoarce înregistrări tipizate pe
  tranzacție și pe linie („de ce acest lot": alocările și valoarea ieșirii
  per unitate, soldul citit, versiunea politicii); intră în lista permisă
  091-r3; se expune read-only prin API (`GET` pe tranzacție), fără pagină React
  nouă (104d). **Autorizarea** (X-RV2): explicația dezvăluie solduri
  integrale ale unităților (`SoldUnitateCitit`) și valori ale altor linii,
  pe care vizibilitatea tranzacției nu le acoperă; de aceea citirea
  explicației cere **acces complet** la domeniul ei (tipurile, membrii
  valorici și istoricul unităților atinse), verificat înaintea proiecției
  după precedentul `SaftAcces`/SAF-D4; fără acces complet, refuz
  `EXPLICATIE_ACCES_INCOMPLET` (403 pe tranzacție vizibilă, 404 pe
  invizibilă), niciodată o proiecție parțială;
- (d) **invariantul de audit** (probă ModelCheck, ambele profiluri), pe
  mecanism, cu domeniu și semn explicite (X-RV5): (1) o postare de ieșire
  pe lot evaluată din sold (BCS, DSC, LDI minus, ASM consum, NTC pe lot în
  `Operare`; BTR și celelalte transferuri evaluate în `Transfer`, unde
  capătul-sursă −Q/−V se normalizează față de decizia +Q/+V, X-RV5.1) are
  exact o `ValoareIesire` a aceleiași linii și unități, cu valoarea
  postată, și un `SoldUnitateCitit` al unității; (2) o ieșire a cărei valoare nu vine din sold (RLF la valoare
  fiscală, NIR-delta) are exact o decizie nouă `ValoareDeclarata(Linie,
  Unitate, Cantitate, Valoare, Sursa)` în nucleu (ierarhie închisă N-D11,
  cu teste), fără schimbarea evaluării aprobate; (3) stornoul unei intrări
  (cantitate negativă pe lot în tranzacție `Storno`) nu are explicație
  proprie: verifică inversa și explicația originalului; (4) o stingere FIFO
  are `AlocareFifo` per unitate; (5) anularea fizică șterge explicația odată
  cu tranzacția (excepția declarată în 091, punctul 5, de tranșat la TR-D9);
  (6) proba cade dacă explicația e eliminată sau alterată (mutant);
- (e) **scenariile** SC-CIT-96…99 (catalogul [CITIRI](scenarii/CITIRI.md)):
  „de ce acest lot" pe BCS din două loturi cu prețuri diferite, pe NTC cu
  stingere FIFO pe două partide, pe ASM cu `AbsorbtieEvaluare` și
  `Operare` + `Transfer` (o singură explicație, referită), pe BTR (purtător
  `Transfer`, semnul sursei normalizat), pe RLF la golire cu reziduu și
  NIR-minus (`ValoareDeclarata`), plus **HTTP pe host viu**:
  tranzacție vizibilă cu (1) membru valoric refuzat, (2) altă linie refuzată,
  (3) istoric parțial al unității — răspunsul este 403 fără nicio valoare
  derivată; storno → explicația originii;
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
  măsurată, m ∈ {0, 6, 12} luni închise × 16 unități, lotul „lung". Ca
  ramurile reconcilierii și INV-CUB să nu fie verzi pe mulțime vidă (102d),
  scena mai conține, o dată per bază: `Deschidere` (bloc + lot + partidă)
  la începutul istoricului când m > 0, o NTC cu împerechere explicită
  (transfer pe partidă), un PIF cu AMO lunară, o LDI minus, și pe privat o
  DVI legată la FCT și un RLF; artefactul X-D3 listează per ramură faptele
  exercitate și contoarele/notele (grupuri incomplete, (f) vacuă), nu numai
  exit 0;
- (b) **ce se măsoară**, fiecare într-un proces nou, rece și cald, cu numărul
  comenzilor SQL, `ms`, `ms SQL`, rânduri citite, alocări, vârfuri: balanța
  lunii, fișa contului cu cele mai multe postări, jurnalul lunii, soldul
  partenerilor, `PartideCuRest`, raportul de stoc, disponibilul FIFO al unui
  BCS nou (costul operării), reconstrucția snapshot-urilor (contabil, stoc,
  partide), jurnalele TVA, D300, D394, SAF-T L și S (deja măsurate, se
  reiau pe aceeași bază), raportul de impact R6 (103h), `PartideDisponibile`
  (102-r5, numai măsurat), închiderea unei luni; fiecare pe ObjectSpace
  securizat (`Admin`) și nesecurizat acolo unde produsul are ambele căi;
- (c) **criteriile**, fără prag absolut, într-o matrice operație × rută ×
  criteriu (X-RV7): rutele sunt „citire din snapshot valid", „reconstrucție"
  (citește legitim istoricul pe care îl reconstruiește), „recitire fără
  graniță sigură" (acces parțial, excluderea unui document); comenzi SQL
  constante în k și m (fără N+1); pentru rutele cu snapshot, **proba din
  plan**: la k fix, `EXPLAIN (ANALYZE, BUFFERS)` pe m = 0/6/12 arată același
  număr de partiții/intervale atinse pe `Postare`, `actual rows × loops` și
  buffers pe nodurile de scanare independente de m — „rândurile livrate" nu
  sunt proba, un `SUM` pe tot istoricul întoarce un rând; liniaritate
  f(4k) ≤ 1,25 × 4 × f(k) pe durată și alocări la cald; cardinalitatea
  rezultatului se fixează sau creșterea ei justificată se raportează
  separat; XSD/DUK pe SAF-T la k = 64; **controlul numeric**: fiecare cititor
  măsurat se compară pe aceeași stare cu un oracol independent (așteptările
  scenei, nu alt cititor) înainte și după orice optimizare, ca un cititor
  „rapid" care omite soldul inițial să cadă; **un criteriu picat lasă X-D5
  și TR-D8 deschise** (X-RV6): o amânare este amendament explicit al
  owner-ului, cu cititorul, criteriul, cifra și restanța numite, nu o
  linie adăugată în `limite-curente`;
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
fără a ține rândul. Direcția fixată de 091: blocaj pesimist în tranzacția
de comandă, nu `Serializable` cu reluare.

**Alegerea owner-ului, 2026-10-01: scrierea în cub este serială per bază
(nivelul 3).** Dintre cele trei granularități discutate (chei fine pe
unitate, X-RV1.1/1.2; gestiune + partener; un singur blocaj per bază),
owner-ul a ales modelul natural: o comandă care scrie în cub (operare,
storno, anulare, corecție, împerechere, desfacere, stingere de deschidere,
reconstrucție) ia la intrare un `pg_advisory_xact_lock` constant și îl ține
până la commit; citirile nu-l ating niciodată. Limita se transferă către
modul de operare: doi operatori nu scriu simultan în cub, al doilea așteaptă
durata unei comenzi. Prețul primește cifră în X-D5 (durata fiecărei comenzi
la k = 64, m = 12). Rafinarea pe gestiune + partener intră ca restanță
`X-r1`, cu criteriul ei: așteptare pe blocaj măsurată peste un prag fixat de
owner atunci, nu acum. X-RV1.1 și X-RV1.2 se închid prin construcție: nu
există chei de derivat și nici ordine între ele.

Se pin-uiește:

- (a) **probele**, fiecare cu două `IObjectSpace`-uri pe conexiuni distincte,
  pe ambele profiluri, cu o **matrice a rezultatelor seriale permise** per
  scenariu și per ordine (X-RV4), nu regula globală „exact una trece":
  două consumuri pe același lot când încape unul singur → un succes și un
  refuz de domeniu; două consumuri care încap amândouă pe lot 3/1,00 →
  **ambele acceptate, cu valorile 0,33 și 0,34 în ordinea serializată și
  rest 1/0,33**, explicațiile persistate dovedind cele două solduri citite
  diferite (X-RV1); consum contra retragerea intrării (storno FCT/NIR) →
  un succes și un refuz, în ambele ordini; două stingeri pe aceeași partidă
  peste rest → un succes și un refuz; operare contra închiderea perioadei
  ei → operare → închidere ambele acceptate, închidere → operare refuzată
  (F27-r8); două detalii noi pe același document → poziții distincte,
  ambele acceptate (S-r9 e despre `DocumentDetaliu.Pozitie`); două
  împerecheri pe aceeași pereche → rezultatul din sumele fixture-ului. În
  toate: niciun 500, nicio excepție de unicitate, cubul conservat, `INV-CUB`
  verde. Sincronizarea testului lasă prima sesiune să facă commit când a
  doua a intrat în așteptarea blocajului (timeout și rollback controlate),
  nu cere ambelor să fie simultan în secțiunea protejată;
- (b) **mecanismul**: blocajul protejează **citirea care decide valoarea**,
  nu doar scrierea (X-RV1). Un singur `pg_advisory_xact_lock` cu cheie
  constantă, luat într-un singur loc (`TranzactieComanda.Incepe` sau
  echivalentul de la intrarea comenzii), **înaintea** planului registrelor
  (`MotorOperare.Opereaza`), a contractului (`Contractare`) și a oricărui
  alt blocaj existent; toate celelalte blocaje (perioada F27-D1, documentele
  stingerii, 97001, 97002, depunerile) rămân și se iau după el, deci
  ordinea comună e „global → restul", fără cicluri posibile. Dry-run-ul
  (`Valideaza`/`Refuzuri`) și citirile nu iau blocajul; `Pozitie` pe
  detalii se atribuie sub blocajul global, ca orice scriere;
- (c) **proba structurală**: fiecare comandă care scrie în cub ia blocajul
  global ca primă instrucțiune (probă ModelCheck pe captura SQL a fiecărui
  tip de comandă, inclusiv împerecherea, stingerea de deschidere,
  reconstrucția și închiderea), plus proba că nicio citire de producție nu
  îl ia; probele de interacțiune din (a) rămân toate și trec prin
  construcție, dar se rulează pe două conexiuni reale ca să dovedească
  serializarea, nu s-o presupună;
- (d) 097-r3 (blocajul comun IMO) și blocajele fine existente rămân așa cum
  sunt; blocajul global le acoperă, scoaterea lor e a lui TR-D9 dacă se
  dovedește redundantă.

## X-D7 — Activarea, regimul dual și restanțele TR-D8

- (a) **istoric de stoc incomplet** (probă ModelCheck pe fapte, înaintea
  purjei, nu la pornirea hosturilor, 102d): `Invarianti.Verifica` acoperă
  azi contabilul (din `RegistruContabil`, pe grupul recepției prin
  `Receptii.Legaturi`), deschiderea, partidele și fișele; se adaugă
  acoperirea cantitativă **pe eveniment/grup și proveniență**, nu pe rând
  (X-RV3): registrele recepției conexe aparțin NIR-ului, cubul are recepția
  pe FCT și numai delta pe NIR (NIR-D1/D2, 098/099), deci corespondența se
  face pe grupul-sursă (lot, gestiune, cont, sens, origine de storno), cu
  Σ cantitate egală per grup și fără ca o postare să acopere două rânduri;
  transferurile cantitative (BTR) au ambele capete; lipsa =
  `CITIRE_ISTORIC_STOC_INCOMPLET`. Probe: recepție conexă cu delta zero,
  minus, plus și storno trec nemodificate; ștergerea unui efect sau a
  provenienței și lipsa unui capăt BTR sunt detectate; așteptările
  cantitative sunt independente de interogarea invariantului;
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
- o probă de concurență al cărei rezultat diferă de matricea serială din
  X-D6 (a) este oprire până la corectarea mecanismului;
- un cititor de producție pe registre în afara listei nominale este defect;
- niciun prag absolut de perf nu se inventează; un criteriu de formă picat
  lasă gate-ul deschis; amânarea e amendament explicit al owner-ului, nu
  text în `limite-curente`;
- ModelCheck roșu pe oricare profil = oprire; o singură rulare grea o dată;
  baza de volum se măsoară în container, nu prin proxy-ul Docker Desktop.

## X-Q — întrebările pentru owner

**Pin-urile owner-ului, 2026-10-03: X-Q1 = A, X-Q2 = A, X-Q3 = A, X-Q4 = A.**
Recomandarea Claude și Codex a fost aceeași. Opțiunile B rămân mai jos ca
istoric al tranșării, nu se implementează.

- **X-Q1** forma explicației deciziei: **A** = `Explicatie jsonb` pe
  `Tranzactie`, schemă versionată, cititor + `GET` read-only (recomandat:
  compact, o singură scriere, append-only, interogabil); **B** = tabelă copil
  normalizată `Decizie` (rând per decizie/ipoteză). Ambele fără pagină React.
- **X-Q2** gate-ul de perf: **A** = scara sintetică `PerfCub` cu criteriile de
  formă din X-D5 (c), fără prag absolut, în container (recomandat; precedentul
  B8-Q2 = A); **B** = amendament de perimetru: X-D5 iese din gate și
  rămâne restanță până la o bază de volum reală (migrare); TR-D8 s-ar
  închide fără cifre de perf, declarat în decizia 107.
- **X-Q3** 102-r4: **A** = refuz `IMPERECHERE_FARA_EFECT` pe ramura negativă a
  desfacerii automate (recomandat: o singură sursă de reguli, fără urmă
  tăcută); **B** = invariant `INV-CUB` „legăturile vii ale perechii nu depășesc
  efectul pe partide", desfacerea rămâne permisă.
- **X-Q4** `PosteazaInCub` după prima tranzacție în cub: **A** = ireversibil,
  refuz în gardian și seed (recomandat); **B** = permis cu diagnostic la
  activare (`CITIRE_ISTORIC_INCOMPLET` ar prinde documentele ulterioare).

## Amendamente după review-ul Codex (2026-10-01)

[Review-ul](tr-d8-transversal-review-codex.md) a adus șapte observații;
toate sunt acceptate și încorporate mai sus, cu probele lor de închidere:

| Obs. | Ce schimbă în contract |
|---|---|
| X-RV1 (P1) | X-D6 (b): blocajul se ia la intrarea comenzii, pe chei grosiere derivate din linii, înaintea planului registrelor și a contractului; proba pozitivă 3/1,00 → 0,33 + 0,34; inventarul ordinii comune a blocajelor; X-D6 (c) căile mixte reale |
| X-RV2 (P1) | X-D4 (c): explicația cere acces complet pe domeniul ei (precedent SAF-D4), altfel refuz; X-D4 (e) probe HTTP cu membru/linie/istoric refuzate |
| X-RV3 (P2) | X-D7 (a): acoperirea cantitativă pe grup și proveniență, nu pe rând; probele NIR conex delta zero/minus/plus/storno |
| X-RV4 (P2) | X-D6 (a): matrice a rezultatelor seriale per scenariu și ordine; S-r9 = `DocumentDetaliu.Pozitie`; sincronizarea testului fără secțiune simultană |
| X-RV5 (P2) | X-D4 (a)/(d): explicația o singură dată per contract, referită de `Transfer`; invariant pe mecanism; `ValoareDeclarata` pentru RLF/NIR-delta; stornoul verifică originea |
| X-RV6 (P2) | X-D5 (c), regula de oprire, X-Q2 (B): criteriu picat = gate deschis; amânarea = amendament explicit al owner-ului |
| X-RV7 (P2) | X-D5 (c): matrice operație × rută × criteriu, proba din plan (partiții, `actual rows × loops`, buffers), reconstrucția separată, controlul numeric cu oracol independent |
| răspunsuri 1, 5 | X-D5 (a): fixture-ul exercită Deschidere, NTC + împerechere, PIF/AMO, LDI, DVI, RLF; artefactul X-D3 arată contoarele și notele per ramură; X-D2: intrarea permisă numește utilizarea; „la pornire" scos (102d) |
| X-RV1.1, X-RV1.2 (reverificare) | închise prin alegerea owner-ului: blocaj global per bază (X-D6), fără chei de derivat și fără ordine între ele; rafinarea = restanța X-r1 |
| X-RV5.1 (reverificare) | X-D4 (a)/(d)/(e): purtătorul explicației e prima tranzacție efectiv produsă (BTR = `Transfer`); transferurile evaluate intră în invariant cu semnul sursei normalizat; BTR în probele auditului |

Reverificarea Codex (`comunicari/2026-10-01-1122-…`) a închis X-RV2, X-RV3,
X-RV4, X-RV6, X-RV7. Recomandarea lui Codex pe X-Q1…X-Q4 este A/A/A/A, ca a
mea; pin-ul rămâne al owner-ului.

## Execuție

### Pasul 1 — X-D2: proba registrelor, lista nominală, N-r8 pe cititori (2026-10-03)

Livrat: `tools/ModelCheck/ProbeCititoriRegistre.cs` + `.Lista.cs` (proba
`X-D2`), `ProbeTransferCititori.cs` (proba `N-r8`), `SursaProductie.cs`
(arborii sintactici ai sursei de producție), SC-CIT-100…102 și modul
`ModelCheck --probe-sursa [--lista]`. Regula, lista nominală și limitele
probei stau în `stare-curenta/dezvoltare-si-validare.md` și
`limite-curente.md`.

Ce a arătat prima rulare (87 de utilizări fișier × membru × registru în 291
de fișiere) și cum amendează X-D2:

1. **Trei clase în plus față de cele trei din contract.** *Maparea EF*
   (`BackOfficeEFCoreDbContext`). *Autorizarea*: rutele care întorc cifre cer
   dreptul de citire pe tipul registrului (`RegistrulCitibil` în trei
   controllere, `PerioadeController.TipuriInsumate`), deși cifrele vin din cub
   (F22-D5, 80e). *Legătura*: `Imperechere` nu este registru, este legătura
   explicită dintre două documente și rămâne după TR-D9; proba îi fixează
   membrii, ca suma legăturilor să nu redevină sursă de rest. Singurul calcul
   care o citește este `Partide.NominalizataLibera` (101).
2. **Un defect, corectat.** `TvaProiectii.IntreLuni(IQueryable<RegistruTva>)`
   era un cititor de registru rămas în `Proiectii/`, fără apelant de producție;
   a ieșit, iar oracolul pe registrul fiscal stă în ModelCheck.
3. **Intrările „de verificat" din contract.** `LoturiCulegereService` nu culege
   din registru: `LoturiLiniiSterse.Curata` întreabă numai dacă lotul are urmă
   (martor). `CorectieService.Corecteaza` scrie perioada inversei (scriitor).
   `ImperecheriProiectii.Asignari` și martorul `RegistruTva` din `TvaProiectii`
   nu mai există.
4. **Perimetrul cititorilor.** În `Proiectii/`, `Api/`, `Culegere/`, `Saft/`,
   `Declaratii/` și WebApi sunt 12 intrări: 4 de autorizare, 7 ale legăturii
   și una singură care consumă un sold de registru, absorbția ASM-B6 din
   `DeclarantAsamblare.Declara` (clasa 1 a contractului, „`StocService.R`").
   Evaluarea cubului citește `Loturi.Cumulate`, nu registrul.
5. **Cod fără apelant de producție.** `StocService.Sold`, `AlocaFifoTolerant`
   și `AlocaFifo` sunt chemate numai de ModelCheck. Rămân declarate în clasa 1
   până la TR-D9; proba refuză orice apelant nou.
6. **N-r8.** Regimul fiecărei intrări publice pe rânduri de cub este declarat
   și probat numeric. `Fiscale.Postari` și `Imobilizari.PozitiiFaraFisa` nu
   filtrează felul; `PozitiiFaraFisa` întoarce contraponderile ASM, cu valoare
   zero, iar consumatorul ei filtrează pe conturile imobilizărilor. Regula
   listării rămâne la X-D7 (d).

Limita probei: scanarea e sintactică, iar purtătorii netipizați de date de
registru sunt declarați de mână (`Purtatori`).

De înregistrat ca restanță la decizia 107: **X-r2** — subiectul permisiunii
care păzește cifrele citite din cub, după tăierea tipurilor de registru
(TR-D9).

Validare: integrala **3.277 bugetar / 4.485 privat OK**, zero FAIL,
`run-verificari/20261003-173905-898/`; scenele ASM, BTR și PLT separat pe ambele
profiluri, `run-verificari/20261003-173755-531/`. Bazele: clonele `.ClaudeX1`.

## Ce NU intră (amânări cu nume)

- TR-D9: tăierea registrelor și a `DescarcareService`, N-r5/`Atribuit`,
  T-r13 evaluări noi, TR-r9, 097-r2, T-r2/T-r3/T-r5, B-r3/B-r6/B-r8/S-r3.
- Migrare (091-r4): contractul 1 al reconcilierii 1C, pragul real FZ-r3,
  bazele de volum reale.
- După PoC: 102-r5 (numai măsurat aici), ecranele explicației, F27-r8 în
  afara probei de închidere, B8-r*.
