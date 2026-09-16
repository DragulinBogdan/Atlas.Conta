# Felia 27 — perioada ca lanț, închiderea ca comandă, soldurile materializate la închidere, data înregistrării (contract)

Data: 2026-09-16. Stare: deschisă, **prioritate decisă**: cerință de produs
declarată (soldurile la zi și soldul partenerilor sunt cerințe normale de
exploatare; arieratele la nivel de document sunt cerute de închiderea de
an). Pleacă din discuția de arhitectură din 2026-09-15/16 (cache-ul de
solduri → lanțul perioadelor → data înregistrării → corecția → închiderea cu
acceptare conștientă). Decizia rezultată se scrie la închidere (088).

## Scop

Registrele sunt rulaje pure și orice sold se calculează la fiecare citire,
prin sumă peste TOT istoricul cheii. Nu există nicio agregare persistată.
Măsurat (59, 66): la un an de date (305 k rânduri `RegistruContabil`,
282 k `RegistruStoc`) nimic nu trece de 300 ms, dar singurul cost structural
e exact cel care crește cu istoricul, nu cu perioada cerută:

| Consumatorul | Ce face azi | Unde |
|---|---|---|
| Balanța | o agregare peste toți atomii `Data <= dataEnd`; inițialul = partea `< dataStart`, din tot istoricul | `ContabilProiectii.Balanta` |
| Fișa de cont | fereastră cumulată peste tot istoricul contului, independent de pagină (`WindowAgg`, 214–286 ms pe `4111`, ~1–1,2 s extrapolat la 5 ani) | `ContabilProiectii.FisaCont` |
| Soldul de stoc | `GROUP BY` peste tot registrul | `StocProiectii.SoldStoc` |
| Motorul, hot path | `SolduriLaData`, `Sold`, `AlocaFifoTolerant` însumează toate rândurile cheii; `VerificaSoldIntermediar` recitește tot istoricul cheii și îl cumulează zi cu zi la FIECARE operare | `StocService` |
| Închiderea TVA | `SUM` pe 4426/4427 `Data <= panaLa`, din tot istoricul | `InchidereTvaService.Solduri` |
| Documentele cu rest | `GROUP BY` peste TOATE liniile din `DocumentDetalii` pentru total + unpivot peste TOATE împerecherile (423 ms, singura proiecție cu creștere liniară, 59) | `ImperecheriProiectii.DocumenteCuRest` |

Baza e per client și nu se resetează niciodată (legacy avea bază nouă per an
și `solduri_repartitori` materializate la trecerea de an). Fără o graniță
persistată, fiecare din rândurile de mai sus crește liniar cu vechimea bazei.

Perioada fiscală e azi un nomenclator simplu (`PerioadaFiscala`: `An`, `Luna`,
`Inchisa`), fără nicio regulă în `GardianEditare`, fără contiguitate și fără
regulă de redeschidere. Consecința: se poate redeschide ianuarie cu martie
închis, motorul acceptă un document retroactiv în ianuarie, iar ITV-urile
operate din februarie și martie (calculate pe soldurile de la sfârșit de
lună) rămân tăcut greșite. Contiguitatea e codificată de două ori, local,
pe tipuri (`NeCronologica` la ITV, „lună precedentă lipsă" la AMO) — semnul
că regula aparține perioadei.

`Document` are doar `Data` (a documentului fizic) și `DataOperare` (timestamp
tehnic). Nu există o dată contabilă distinctă: un document din perioadă
închisă nu poate fi operat deloc, iar o eroare materială dintr-o perioadă
închisă nu are altă cale decât storno-ul la o dată nouă, fără legătură
declarată cu documentul corect și fără motiv.

`RegistruTva` are doar `Data`; jurnalele, D300, D394 și SAF-T filtrează pe ea.
Nu există perioada de declarare ca dată distinctă, deci nici noțiunea de
conținut de rectificativă.

`Imperechere` nu are dată proprie (o ia din documentul stingător prin join)
și nici gardian de perioadă (`VerificaDeschisa` are 5 apelanți: motorul,
AMO, ITV — nu și `ImperechereService`).

Ce cumpără felia, **declarat**:

1. **Solduri la zi și la orice dată cu cost mărginit de perioada deschisă**,
   pentru orice cheie (cont, cont × repartitor, cont × dimensiuni, lot ×
   gestiune × tip stoc): balanță, fișă, sold stoc, sold partener, liste cu
   sold. Gardianul de stoc din motor cumulează doar fereastra deschisă.
2. **Arieratele la nivel de document** (facturi neîncasate/neplătite),
   înghețate la fiecare închidere, deci și la 31.12, fără recalcul ulterior.
3. **Perioada ca lanț**, cu închiderea și redeschiderea ca operații ale
   motorului, verificate, acceptate conștient și auditate.
4. **Documentul întârziat și eroarea materială** ca fluxuri normale, nu ca
   excepții: data înregistrării separată de data documentului, perioada de
   declarare separată de data faptului fiscal, corecția legată și motivată.

Ce NU e motivul: performanța de azi. Cifrele curente sunt sub prag; felia
pornește pe cerința de produs, iar mărginirea costului e consecința.

## Testul contra invarianților

- **I** (orice urmă în registre are document): neatins. Snapshot-urile NU
  sunt registre și nu sunt urme: sunt proiecții persistate ale registrelor,
  derivabile integral, cu invariantul „există dacă și numai dacă perioada e
  închisă". Închiderea nu scrie registre.
- **III** (registrele = singurul adevăr al agregării; rândul scris nu se
  modifică; perioada închisă = graniță absolută): ÎNTĂRIT. Snapshot-ul e o
  sumă peste registre și e verificat contra lor în ModelCheck (ca proba D9
  `SoldStoc` ↔ `StocService.Sold`); nu e un al doilea adevăr fiindcă e
  reconstruibil și măsurat. Granița devine LANȚ (D1), deci „absolută" capătă
  și sensul „nimic de dinaintea ultimei închideri nu se mai poate schimba,
  prin nicio cale". `DataInregistrare` (D4) e exact ce face granița
  respectabilă fără a refuza documentele întârziate.
- **II** (motorul nu cunoaște frunzele): neatins. Data înregistrării intră
  pe bază fiindcă trece ambele teste (semantică identică pe orice tip;
  motorul o consumă direct la postare). Perioada de declarare e a
  registrului fiscal, decisă prin politică (IV), nu prin `is` pe tip.
- **IV** (politica = date): severitatea constatărilor de închidere și regula
  de declarare a faptului fiscal întârziat sunt DATE (`PoliticaInchidere`,
  câmp nou pe `PoliticaTva`); mecanismul (lanț, snapshot, acceptare) e cod.
- **V** (sursele externe = evidență): Import1C rulează neschimbat
  (`DataInregistrare = Data` implicit), baseline-ul rămâne identic.
- **VI** (evaluarea = motorul): FIFO-ul și golirea valorică rămân în motor;
  snapshot-ul de stoc le dă doar soldul de pornire. Lotul se naște la data
  înregistrării (D4), singura ordine compatibilă cu „sold ≥ 0 la orice dată".
- 14/25d: anularea rămâne doar în perioadă deschisă și fără dependenți;
  storno-ul la data stornării. Împerecherea din perioadă închisă devine fapt
  (D8) — o extindere a lui 14 la relația dintre documente, nu o excepție.
- 42c (TS nu calculează sold/rest/total): întărit — totalul documentului
  devine coloană scrisă de motor (D7), nu agregat la citire.
- 66d (netarea nu e aditivă ⇒ modul se cere): PĂSTRAT prin construcție —
  snapshot-ul contabil ține debitul și creditul cumulate SEPARAT pe cheia
  COMPLETĂ a atomului; orice cheie mai grosieră e rollup aditiv, iar netarea
  se face abia la nivelul cerut.
- IM-D4 (contractul de citire „Stoc": prefix-sum fără a încărca întregul
  registru): felia aceasta îl face posibil; contractul „Relații" primește
  perioada ca lanț. Felia 27 precede izolarea motorului.

## Amendamente după pasul 0 (2026-09-16, spike `p5-felia27-pas0-spike.md`)

- **F27-D1 (cursa), tranșată: F1 pe ambele capete.** Tranzacția e a
  `DbContext`-ului ObjectSpace-ului propriu al comenzii
  (`((EFCoreObjectSpace)os).DbContext.Database.BeginTransaction()`, cast-ul
  canonic din `ContabilProiectii.cs:790`), deci NU e „în afara
  ObjectSpace-ului"; F27-r3 nu se activează. Operarea: tranzacția în
  `OperareApi` (cele patru metode) și în `InchidereTvaApply`/`AmoApply.Genereaza`
  — `MotorOperare` neatins; `GardianPerioada.VerificaDeschisa` citește prin
  `SqlQuery … FOR SHARE` cu `"GCRecord" = 0` explicit, ÎN LOCUL
  `FirstOrDefault`-ului (zero statement-uri în plus; 0 rânduri ⇒ „nedefinită =
  închisă"). Închiderea: tranzacție proprie în `PerioadaService.Inchide` —
  `FOR UPDATE` pe rând → verificare → `SUM` → snapshot/partide → `Inchisa` →
  `CommitChanges` → `Commit`. F2 (interceptor) respins ca gardian de graniță
  (depinde tăcut de `AutoTransactionBehavior.Always`); F3 respins (conflict
  fals între două operări în aceeași lună); F5 respins. Intră în pasul 2.
- **F27-D3 (snapshot-urile), amendat: perioade DE REFERINȚĂ, nu toate cele
  închise.** Măsurat: cheia completă crește cu ~15 k/lună și nu scade; un
  snapshot per perioadă închisă ar ajunge la ~27 M rânduri la 5 ani (~19×
  registrul). Regula nouă: snapshot există ⇔ perioada e DE REFERINȚĂ = ultima
  perioadă închisă SAU un decembrie închis (capăt de an). Închiderea lui P scrie
  snapshot(P) = snapshot(P−1) + rulaje(P) (`UNION ALL` + `GROUP BY`, nu JOIN —
  dimensiunile nullable) și ȘTERGE snapshot(P−1) dacă P−1 nu e decembrie;
  redeschiderea lui P șterge snapshot(P) și RECONSTRUIEȘTE snapshot(P−1) prin
  `SUM` integral (≤ 1,5 s la 5 ani; redeschiderea e rară). Invariantul probat
  devine: pentru fiecare perioadă de referință snapshot(P) = `SUM(registru,
  DataInregistrare <= sfârșit P)` la cent, pe fiecare cheie, și nicio altă
  perioadă nu are snapshot. `SolduriService` pornește de la ULTIMA perioadă de
  referință ≤ d (o dată istorică între capete costă cel mult un an de rulaje).
  Cheile integral zero (debit ȘI credit cumulate 0; cantitate ȘI valoare 0) se
  OMIT (−92,9 % pe stoc); cheia absentă = zero pentru orice consumator.
  Aceeași regulă de referință pentru `PartidaDeschisa` (F27-D7): arieratele la
  31.12 rămân acoperite. Pragul de timp (30 s) e respectat cu marjă ~20×;
  cheia completă rămâne.
- **Index pe `Data`** (`WHERE "GCRecord" = 0`) pe `RegistruContabil` și
  `RegistruStoc`: intră în migrația pasului 2 (rulajele lunii 70,6 → 37,9 ms).
- **Unicitate `(An, Luna)`** pe `PerioadeFiscale`: index unic filtrat, în pasul 1.
- **Notă pentru ModelCheck:** `EFCoreOptimisticLockInterceptor` NU e înregistrat
  pe căile standalone; orice probă de concurență îl înregistrează explicit.

## Deciziile

### F27-D1 — Perioada fiscală e lanț; `Inchisa` se scrie doar prin comandă

- **Închiderea lui P cere P−1 închisă.** Regula „perioadă absentă = închisă"
  rămâne și dă capătul lanțului: baza are o primă perioadă (deschiderea),
  tot ce e înainte e închis prin absență; tot ce e după ultima definită e
  închis prin absență, deci perioadele se definesc înainte, ca azi (seed,
  `Import1C.Perioade.Asigura`).
- **Redeschiderea lui P cere P+1 deschisă** (sau absentă). Se redeschide
  doar ultima închisă; cascada e explicită, nu subînțeleasă. Un „stale" pe
  închiderile operate ale lunilor următoare devine imposibil prin
  construcție; staleness-ul rămâne confinat în fereastra deschisă, unde
  trăiește deja detecția per tip (ITV pe drafturi, AMO la operare).
- `PerioadaFiscala.Inchisa` e AL MOTORULUI: `GardianEditare` refuză orice
  scriere pe ea prin ușa securizată, cu aceeași regulă ca pe registre
  (14/42a); `An`/`Luna` se creează liber (nomenclator), dar unicitatea
  `(An, Luna)` e verificată. XAF: `AllowEdit=False` pe `Inchisa`, acțiunile
  „Închide"/„Redeschide" pe `PerioadaFiscala` (tiparul 79-r1).
- Istoricul e append-only: `InchiderePerioada` (`PerioadaId`, `Fel`
  Închidere|Redeschidere, `La` timestamp, `DeId` utilizator, `Acceptari`
  — D2). Pe `PerioadaFiscala` se materializează `Inchisa` și `InchisaLa`
  (null cât e deschisă) pentru hot path-ul gardianului, plus
  `InchisaPrimaOara` (nu se șterge la redeschidere — D5 are nevoie de ea).
- **Cursa închidere ↔ operare**: închiderea verifică și scrie snapshot-ul în
  tranzacția ei; operarea citește `Inchisa` în tranzacția ei. Fereastra
  dintre cele două se închide prin blocarea rândului perioadei:
  `VerificaDeschisa` ia `FOR SHARE`, comanda de închidere `FOR UPDATE`
  (Postgres; pe SQL Server `UPDLOCK`/`HOLDLOCK` — provider-ul e deja
  bifurcat prin `EFCoreProvider`). Forma exactă se decide la pasul 0.
  Concurența între operatori (25f) rămâne parcată; asta e doar cursa
  perioadei.

### F27-D2 — Închiderea = verificare → acceptare conștientă → închidere

- `GET api/perioade/{an}/{luna}/verificare` întoarce **constatări tipizate**:
  `Cheie` stabilă (ex. `ITV-LIPSA`, `AMO-LIPSA:{unitateId}`,
  `DRAFT-IN-PERIOADA:{documentId}`), `Fel`, `Severitate`
  (Blocant | Avertisment), `Text`, `Obiect` (id + etichetă). Forma e cea a
  lui `PoliticiController.Verificare` (`ConstatareProfilDto`).
- `POST api/perioade/{an}/{luna}/inchide` cu corpul `{ Acceptate: [chei] }`.
  Comanda **rulează din nou verificarea în aceeași tranzacție**: un blocant
  refuză mereu (422 cu lista); un avertisment a cărui cheie nu e în
  `Acceptate` refuză (422 cu lista întreagă, ca ecranul s-o arate și
  utilizatorul să accepte constatări concrete, nu un flag de forțare); toate
  acceptate ⇒ închidere + snapshot (D3) + partide deschise (D7) + rândul de
  istoric cu acceptările (cine, când, ce). Un mecanism „verificare →
  închidere → raportare" e respins explicit: raportul de după nu mai poate
  schimba nimic.
- `POST api/perioade/{an}/{luna}/redeschide` cu `{ Motiv }` obligatoriu;
  refuză dacă P+1 e închisă; șterge snapshot-urile și partidele lui P în
  aceeași tranzacție; scrie istoricul.
- **Severitatea e politică** (`PoliticaInchidere`: `FelConstatare` →
  Blocant | Avertisment | Ignorat, per profil, seed cu `DinSeed`). Blocantele
  STRUCTURALE stau în cod și nu sunt configurabile: P−1 neînchisă;
  redeschidere cu P+1 închisă; snapshot inconsistent la reconstrucție.
- Setul inițial de constatări (extensibil ca date, nu ca schemă): ITV lipsă
  sau neoperată; AMO lipsă sau neoperată per unitate internă cu fișe
  eligibile; drafturi cu `DataInregistrare` în P (avertisment: după
  închidere rămân operabile cu `DataInregistrare` ulterioară — consecința
  utilă a lui D4; nu mai sunt „documente moarte"); documente operate în P
  cu `Rest` scadent (informativ, la cerere). Reconcilierea 1C NU intră
  (conectorul, nu mecanismul).
- Ecrane: XAF acțiunea pe `PerioadaFiscala` cu dialogul constatărilor și
  bifele de acceptare (tiparul 79-r1: obiect non-persistent, `NewWindow`);
  React `/perioade` cu lista lanțului, verificarea și închiderea (închide
  53i/79-r3 în forma corectă). Refuzurile uniforme (80): 401 → 400 → 404 →
  403 → 422, `EroriDto`.

### F27-D3 — Soldurile materializate la închidere; toți consumatorii pornesc de la ultima închidere

- `SoldPerioadaContabil` (`An`, `Luna`, `ContId`, cele 8 dimensiuni ale
  laturii ca FK-uri nullable, `Debit`, `Credit` cumulate de la începutul
  bazei până la sfârșitul lui P) și `SoldPerioadaStoc` (`An`, `Luna`,
  `LotId`, `RepartitorId`, `TipStoc`, `Cantitate`, `Valoare`). Cheia e
  cheia COMPLETĂ a atomului, nu a raportului: orice raport e rollup aditiv
  pe snapshot + rulaje.
- Scriere: snapshot(P) = snapshot(P−1) + rulaje(P), în tranzacția
  închiderii; prima închidere a bazei = `SUM` pe tot istoricul ≤ P.
  Reconstrucția (`POST api/perioade/reconstruieste`, Administrator) face
  `SUM` integral pentru fiecare perioadă închisă și RAPORTEAZĂ diferențele
  înainte să rescrie (35b: diferența se raportează, nu se ascunde).
- Invariant, probat în ModelCheck pe ambele profiluri: rândurile snapshot
  există ⇔ perioada e închisă; pentru fiecare perioadă închisă snapshot(P)
  = `SUM(registru, DataInregistrare <= sfârșit P)` pe fiecare cheie, la cent.
- **Consumatorii**: un singur serviciu de citire (`SolduriService`) răspunde
  „soldul cheilor X la data d" ca `snapshot(ultima închisă ≤ d) + SUM(rulaje
  în (ultima închisă, d])`. Îl consumă: balanța (inițialul = snapshot +
  rulaje până la `dataStart−1`), fișa (fereastra pornește cu offset-ul
  soldului la `dataStart−1`, cumulează doar rândurile perioadei cerute),
  soldul de stoc, `SolduriLaData`/`Sold`/`AlocaFifoTolerant`,
  `VerificaSoldIntermediar` (pornește de la snapshot ≥ 0 și cumulează doar
  fereastra deschisă), `InchidereTvaService.Solduri`, SAF-T (inițialul de
  stoc și de cont). Niciun hot path nu mai citește rânduri de dinaintea
  ultimei închideri. Registrele NU se ating.
- Rândurile de deschidere ale migrării (`DocumentId = null`) sunt rânduri
  de registru și intră în sumă normal; nu se convertesc în snapshot.
- Proiecție nouă: `GET api/proiectii/sold-parteneri` (cont × repartitor la
  data cerută, cu filtrele balanței), pe același serviciu; sursa listei de
  parteneri cu sold și a coloanei de sold din lookup-uri, la cerere.

### F27-D4 — `DataInregistrare` pe `Document`: registrele se scriu la data înregistrării

- Câmp pe bază, `DateOnly`, editabil în Draft, implicit `= Data` (la creare
  și la schimbarea lui `Data` cât timp cele două erau egale); cerut
  `>= Data`. Gardianul de perioadă (`VerificaDeschisa`) se mută pe
  `DataInregistrare`. `Data` rămâne a documentului fizic: numerotarea,
  scadența, cronologia seriilor proprii (FCL) și identitatea fiscală rămân
  pe ea.
- Registrele contabil, stoc și imobilizări se scriu cu
  `Data = DataInregistrare`. Lotul se naște cu `Lot.Data = DataInregistrare`
  (ordinea FIFO = ordinea intrării în evidență; e singura ordine
  compatibilă cu „sold ≥ 0 la orice dată"). Amortizarea unei fișe puse în
  funcțiune întârziat pornește din luna înregistrării, cu recuperarea
  lunilor restante în prima AMO (regula de recuperare = tranșare în pasul
  4, cu proba pe scena 2028).
- Storno-ul: `dataStorno >= DataInregistrare` (azi `>= Data`).
- Fișa de cont și registrul-jurnal afișează `Data` = data înregistrării și
  numărul documentului; drill-down-ul deschide documentul, unde stau ambele
  date. Documentul întârziat apare deci în luna în care a intrat în
  evidență, cu numărul și data lui fizică vizibile — exact ce așteaptă
  contabilul.
- Import1C: nu setează câmpul (implicitul = `Data`) ⇒ registrele identice,
  baseline identic. `metadata.json`: caption „Data înregistrării".
- Documentul cu `Data` într-o perioadă închisă și `DataInregistrare` în cea
  deschisă e fluxul NORMAL al documentului întârziat: nu e corecție, nu are
  motiv, nu atinge snapshot-ul. Efectul lui fiscal e al lui D5.

### F27-D5 — `PerioadaDeclarare` pe `RegistruTva`; rectificativa e o consecință, nu un flag

- `RegistruTva` primește `PerioadaAn`/`PerioadaLuna` (perioada de
  declarare), distinctă de `Data` (data faptului fiscal, care rămâne data
  documentului: jurnalele sunt pe data facturii). Jurnalele de TVA, D300,
  D394 și SAF-T filtrează pe PERIOADA DE DECLARARE, nu pe `Data`.
- Regula de completare, la scrierea rândului: dacă perioada lui `Data` e
  deschisă ⇒ perioada lui `Data`. Dacă e închisă ⇒ decide politica, ca
  dată pe `PoliticaTva` (câmp nou `DeclarareIntarziata`):
  `PerioadaInregistrarii` (implicit pe deductibil — art. 301 Cod fiscal,
  dreptul de deducere se exercită în perioada primirii facturii, fără
  rectificativă) sau `PerioadaFaptului` (implicit pe colectat — factura
  noastră neînregistrată la timp aparține fiscal perioadei originale).
  Profilul poate alege altfel; motorul nu știe de ce.
- **Conținutul de rectificativă** pentru P = rândurile cu perioada de
  declarare P scrise după `InchisaPrimaOara` a lui P
  (`DataOperare > InchisaPrimaOara`).
  *Notă de execuție (pasul 4a)*: reperul per rând e `RegistruTva.ScrisLa`, nu
  `Document.DataOperare` — `DataOperare` e a documentului și nu descrie rândul
  de storno, scris mai târziu peste același document. Nu există flag: e derivat din două
  timestamp-uri. D300/D394/D406 pe o perioadă închisă cu astfel de rânduri
  se raportează ca RECTIFICATIVĂ, cu secțiunea „diferențe față de declarat"
  = exact acele rânduri. Redeschiderea NU șterge `InchisaPrimaOara`: ce a
  fost declarat o dată rămâne reperul.
- Storno-ul păstrează regula JT-D5 (rândul invers = fapt al perioadei
  stornării): `Data = dataStorno`, perioada de declarare = perioada lui
  `dataStorno`. Excepția e corecția cu motiv (D6).

### F27-D6 — Corecția în perioadă închisă: storno legat + document nou, cu motiv

- Nu există editare în loc a unui document operat, nicăieri, niciodată
  (55a rămâne). În perioadă deschisă: ce există azi (anulare + corecție
  directă fără dependenți; altfel storno).
- În perioadă închisă: **comanda `corecteaza`** = storno-ul originalului la
  o dată din perioada deschisă + document nou (Draft, culegerea copiată din
  original, cu `Numar` și `Data` ale documentului fizic păstrate,
  `DataInregistrare` = data corecției) care poartă `CorecteazaId` (originalul
  stornat) și `MotivCorectie` (`EroareMateriala` | `FaptNou`). Legătura e
  1:1 și verificabilă la commit (`IVerificabilLaCommit`): originalul trebuie
  să fie `Stornat`, un original nu poate fi corectat de două ori.
- Motivul decide EFECTUL FISCAL, nu contarea: `EroareMateriala` ⇒ rândurile
  de TVA ale stornoului ȘI ale documentului nou primesc perioada de
  declarare a ORIGINALULUI (⇒ rectificativă pe P, prin D5); `FaptNou` ⇒
  regula normală din D5. Contarea documentului nou e cea normală, din
  politică; reclasificarea pe rezultatul reportat (1174, OMFP 1802 pct.
  66–67) pentru erorile semnificative din exerciții anterioare rămâne notă
  contabilă manuală, decisă de contabil (F27-r1) — motorul nu judecă
  semnificația.
- Snapshot-ul lui P nu se atinge: stornoul și corecția trăiesc în fereastra
  deschisă. Situațiile lui P rămân cum au fost la închidere; adevărul
  corectat se vede din prima perioadă deschisă încolo, iar fiscal prin
  rectificativă.

### F27-D7 — Totalul documentului e fapt scris la operare; partidele deschise se materializează la închidere

- `Document.TotalStingere` (`decimal`, al motorului, `AllowEdit=False`):
  Σ `LiniiCreanta` la operare, în aceeași tranzacție cu registrele; null la
  anulare; neatins la storno (documentul stornat iese din candidați prin
  `Stare`). `ImperechereService.Total` îl citește de pe document (un
  `SELECT` pe cheie, nu un `SUM` pe linii). `DocumenteCuRest` renunță la
  `Brut` și la excluderea lui `ReturClient` (totalul lui e deja filtrat prin
  hook, deci proiecția nu mai diverge de serviciu — închide amânarea din
  antetul `ImperecheriProiectii.cs`).
- `PartidaDeschisa` (`An`, `Luna`, `DocumentId`, `Rest`): la închiderea lui
  P, toate documentele operate cu `Rest ≠ 0` la sfârșitul lui P
  (`TotalStingere − Σ Imperechere.Suma cu Data <= sfârșit P`). Ștearsă la
  redeschidere. Invariant probat: pentru fiecare P închisă, mulțimea și
  restul coincid cu recalculul din registre + împerecheri.
- `DocumenteCuRest(d)` = partidele lui P (ultima închisă ≤ d) cu restul
  ajustat cu împerecherile din `(P, d]` ∪ documentele operate cu
  `DataInregistrare` în `(P, d]` cu restul lor. Costul e mărginit de
  fereastra deschisă plus numărul partidelor deschise (care e mic prin
  natura lui: ce e neîncasat, nu ce a fost emis).
- La 31.12 lista partidelor deschise E arieratele la nivel de document.
  Scadențarul și aging-ul (F27-r2) sunt aceeași listă cu scadența alături.

### F27-D8 — Împerecherea e fapt datat

- `Imperechere.Data` (`DateOnly`, persistată la creare): automat =
  `DataInregistrare` a stingătorului; manual = data cerută, `>=`
  `DataInregistrare` a ambelor documente, în perioadă deschisă
  (`VerificaDeschisa` intră în `ImperechereService.Imperecheaza`).
- Ștergerea directă a unei împerecheri rămâne doar în perioadă deschisă (e
  „link simplu", ca azi). O împerechere din perioadă închisă se desface prin
  RÂND INVERS (`Suma` negativă, `Data` în perioada deschisă, `InverseazaId`)
  — scris de motor la stornarea unui document împerecheat
  (`VerificaFaraImperecheri` se relaxează: împerecherile din perioadă
  închisă se inversează, cele din perioada deschisă se cer șterse ca azi) și
  disponibil ca acțiune explicită („Desfă împerecherea"). `Asignari`
  (unpivot) și `PartidaDeschisa` însumează algebric, ca registrele.
- Import1C: împerecherile primesc `Data` din stingător (backfill în
  migrație pentru bazele existente: `Data = DocumentStingator.Data`).

### F27-D9 — Ce rămâne neschimbat, explicit

- Schema registrelor contabil/stoc/imobilizări: neatinsă (doar semantica
  lui `Data` = data înregistrării, identică cu azi pentru orice document cu
  `DataInregistrare = Data`).
- `MotorOperare.CalculeazaSiValideaza`: se schimbă DOAR data pe care o
  citește (D4) și sursa soldului de pornire (D3). Nicio regulă de potrivire,
  contare, evaluare sau stingere nu se atinge.
- Anularea, storno-ul, conexul, perechea 581, stingerea automată: aceleași
  reguli, pe `DataInregistrare`.
- Profilul bugetar: aceleași mecanisme; seed-ul politicilor noi diferă de
  conținut (severități, `DeclarareIntarziata`), nu de schemă.

### F27-D10 — Regula de oprire a feliei

Felia e închisă când, pe codul final:

- ModelCheck verde pe AMBELE profiluri cu toate probele existente (cifrele
  ≥ cele de la deschidere: privat 1184/0, bugetar 1068/0) plus probele noi:
  lanțul (închidere necontiguă refuzată, redeschidere cu P+1 închisă
  refuzată, CRUD pe `Inchisa` refuzat), acceptarea (blocant neacceptabil,
  avertisment neacceptat ⇒ 422 cu lista, acceptat ⇒ închis + istoric),
  snapshot ⇔ închisă și egalitatea la cent cu `SUM` pe fiecare cheie (scena
  2028 cu 3 luni închise, pe contabil și stoc, cu dimensiuni pe laturi),
  fișa/balanța/soldul de stoc identice cu și fără snapshot (aceeași scenă
  citită înainte și după închidere), `VerificaSoldIntermediar` cu snapshot
  (refuzul „sold negativ" identic ca text), documentul întârziat (registre
  la `DataInregistrare`, lot la `DataInregistrare`, perioada de declarare
  după politică, conținut de rectificativă detectat), corecția
  (`EroareMateriala` ⇒ perioada originalului pe rândurile fiscale; `FaptNou`
  ⇒ regula normală; original necorectabil de două ori), partidele deschise
  ⇔ recalcul, împerecherea în perioadă închisă (creare refuzată, desfacere
  prin rând invers), `TotalStingere` ⇔ `Σ LiniiCreanta` pe fiecare tip
  stins, `ReturClient` în `DocumenteCuRest` cu totalul lui filtrat.
- Import1C integral pe Flax cu raportul de reconciliere IDENTIC cu ultimul
  raport verde al feliei 26; apoi o a doua rulare cu închiderea lunilor pe
  parcurs (`inchide` după fiecare lună importată, acceptând avertismentele)
  cu ACELAȘI raport și cu snapshot-urile verificate contra `SUM` pe baza
  reală (12 perioade, 0 diferențe).
- Perf pe HTTP pe baza de import, după închiderea a 11 luni din 12: fișa
  contului `4111` pe decembrie sub 100 ms; balanța analitică pe decembrie
  sub 100 ms; `DocumenteCuRest` filtrat pe contrapartida-reper sub 150 ms
  (azi 423); operarea unei FCL cu 49 de linii nu mai lentă decât azi (59).
  Cifrele se scriu în `p5-perf-masuratori.md` (addendum).
- `refuzuri.ps1` ≥ 229/229 pe host viu Privat, cu scenele noi (perioade:
  `Cititor` 403 pe `inchide`, `Configurator` 403, inexistent 404, blocant
  422; corecție; sold-parteneri).
- `openapi.json`/`api-types.ts` fără drift; `metadata.json` la zi
  (`DataInregistrare`, `TotalStingere`, `Imperechere.Data`, perioade);
  `has-pending-model-changes`: niciuna; migrațiile canonice (23a).
- Smoke pe hostul Blazor viu și în browser pe React: închiderea unei luni cu
  acceptare, refuzul necontiguu, documentul întârziat cules cu
  `DataInregistrare`, corecția din ecran, `/perioade`, `/sold-parteneri`.
- `docs/stare-curenta` actualizată în același commit cu codul (domeniu:
  perioada ca lanț, data înregistrării, corecția; politici: închiderea,
  declararea întârziată, rectificativa; api: `api/perioade`,
  `sold-parteneri`, `corecteaza`; limite: ce rămâne).

## Pașii (un agent per pas, regulă de oprire per pas, verificare independentă)

0. **Spike cursa perioadei + costul snapshot-ului** (D1, D3): forma blocării
   rândului perioadei pe Postgres prin EF/ObjectSpace (SQL brut, ca fișa),
   dovada că `VerificaDeschisa` cu `FOR SHARE` nu schimbă planul operării;
   `SUM` integral pe baza de import pentru o lună (cost de referință al
   primei închideri și al reconstrucției). Oprire: dacă blocarea cere
   tranzacție explicită în afara ObjectSpace-ului, felia amână cursa cu nume
   (F27-r3) și merge pe verificarea în tranzacție singură; dacă `SUM`-ul
   integral pe cheia completă a atomului depășește 30 s, cheia se
   reproiectează înainte de pasul 2.
   *Executat (2026-09-16)*: `docs/api/p5-felia27-pas0-spike.md`. F1 probat
   pe calea reală XAF (`CommitChanges` se înrolează în `CurrentTransaction`;
   `FOR SHARE` ține rândul înainte și după commit; rollback-ul anulează și
   commit-ul); F2 condiționat (`AutoTransactionBehavior.Always`,
   `DbTransactionInterceptor` cu `override`); F3 cu conflict fals; F5 respins.
   `SUM` integral pe cheia completă: 300 ms azi (187.378 chei), ~1,5 s la
   5 ani; o lună 70,6 ms (37,9 cu index pe `Data`); incrementala identică la
   cent cu `SUM`-ul direct. Regula de oprire neatinsă pe ambele întrebări;
   amendamentele de mai sus.
1. **Lanțul și comanda de închidere** (D1, D2 fără constatări): schema
   (`InchiderePerioada`, `Inchisa`/`InchisaLa`/`InchisaPrimaOara`), gardianul
   pe `Inchisa`, `inchide`/`redeschide` pe REST și XAF, refuzurile uniforme,
   probele lanțului. Oprire: ModelCheck identic + probele noi;
   `refuzuri.ps1` verde cu scenele perioadelor.
   *Executat 2026-09-16, fără opriri; devierile raportate*: `PerioadaFiscala`
   în fișier propriu cu `InchiderePerioada` + `FelInchiderePerioada`,
   `SeveritateConstatare` în `Enums.cs`, `Motor/PerioadaService`
   (`Verifica`/`Inchide`/`Redeschide`/`Lant`), `Api/Perioade/*`,
   `PerioadeController`, `PerioadaFiscalaController` (XAF), `ContaUiBaseline`;
   migrația `20260916123442_F27Pas1PerioadaLant` (tabela `InchideriPerioade`,
   două coloane noi, index unic filtrat pe `(An, Luna)`, zero atingeri de
   date), aplicată pe cele trei baze fără duplicate preexistente. Gardianul a
   primit și un control POZITIV (perioada deschisă fără istoric se șterge).
   Cele două probe care scriau `Inchisa = true` trec pe comandă: F21-D9.5d
   închide lanțul 2026 întreg și îl redeschide invers (12/2026 nu e capăt de
   lanț, iar cifrele 52,5/42 sunt ale scenei 2026), gardianul de perioadă al
   motorului se mută pe o perioadă de scenă în 2029 (capăt de lanț prin
   absența precedentei). ModelCheck bugetar 1087/0, privat 1203/0 (+19
   `PER-V0…V10`); `refuzuri.ps1` 242/242 pe host viu Privat, cu lanțul
   neatins; `pnpm build` verde, codegen idempotent. Rămân deschise: corpul
   `Acceptate` primit și ignorat (pasul 7), cursa închidere ↔ operare (F1 din
   amendamente, pasul 2), ecranul React `/perioade` (pasul 7).
2. **Snapshot-urile și `SolduriService`** (D3): tabelele, scrierea la
   închidere, ștergerea la redeschidere, reconstrucția cu raport,
   consumatorii mutați unul câte unul (proiecții → motor → ITV → SAF-T),
   fiecare cu proba „identic cu și fără snapshot". Oprire: ModelCheck
   identic; proba de egalitate la cent pe scena 2028; `Motor/StocService`
   cu snapshot și `VerificaSoldIntermediar` cu același text de refuz.
   *Executat 2a (2026-09-16), fără opriri; devierile raportate*: tranzacția
   comenzii (`Motor/TranzactieComanda`) în `OperareApi` (operare/anulare/storno),
   în `InchidereTvaApply`/`AmoApply` (`Genereaza`/`Regenereaza`) și în comenzile
   perioadei; `GardianPerioada.VerificaDeschisa` citește prin
   `SqlQuery … FOR SHARE` cu textele NESCHIMBATE, iar `PerioadaService` ia
   `FOR UPDATE` pe rândul perioadei ca primă instrucțiune. `SoldPerioadaContabil`
   și `SoldPerioadaStoc` (`BusinessObjects/Registre/SolduriPerioada.cs`) cu
   unicitatea cheii complete `NULLS NOT DISTINCT`, `Motor/SolduriService`
   (`Referinte`/`AreSnapshot`/`Materializeaza`/`Elimina`/`Reconstruieste`),
   `POST api/perioade/reconstruieste` cu gate de SCRIERE pe tip
   (`ContaApiController.PoateScrie`) și acțiunea XAF „Reconstruiește soldurile";
   migrația `20260916131506_F27Pas2SolduriPerioada` (două tabele + indexurile
   filtrate pe `Data` ale registrelor contabil și stoc), aplicată pe cele trei
   baze. Consumatorii NU s-au mutat încă: rămân pentru 2b.
   *Executat 2b (2026-09-16), cu O OPRIRE raportată; devierile mai jos*:
   `Motor/SolduriService` a primit partea de CITIRE — `Referinta` (ultima
   perioadă de referință care se termină până la o dată), `AtomiCumulati`
   (snapshot contabil proiectat ca `AtomContabil`, datat la sfârșitul
   referinței, `Concat` cu rulajele de după ea) și `MiscariCumulate`
   (oglinda de stoc, cu filtrele de document și de produs aplicate per
   ramură). Mutați: `ContabilProiectii.Balanta` (deci și `BalantaPlan` și
   inițialul de CONT al SAF-T), `ContabilProiectii.FisaCont` (a treia ramură
   a uniunii = rândul sintetic din snapshot, cu filtrele de dimensiune
   aplicate înăuntru și coordonatele lor purtate pe rând),
   `StocProiectii.SoldStoc` (cu `laData` opțional, expus ca
   `GET api/proiectii/sold-stoc?laData=`), `StocService.SolduriLaData`,
   `Sold`, `AlocaFifoTolerant`, `VerificaSoldIntermediar` și
   `InchidereTvaService.Solduri`. **Oprirea**: `SaftProiectii.AgregatStoc`
   NU s-a mutat — `Randuri` din agregatul lui e RAPORTAT CA NUMĂR în
   avertismentul `SoldPeTipStocNeraportat`, iar dintr-un snapshot numărul de
   rânduri de registru nu se mai poate afla; `RanduriInitiale > 0` ar deveni
   fals pentru cheile cu sold cumulat zero. Devieri: (1) `SoldStoc` și
   `SolduriLaData` omit acum cheile cu cantitate ȘI valoare zero — fără asta
   „identic cu și fără snapshot" e fals prin construcție, fiindcă un lot
   golit înaintea referinței lipsește din snapshot; consecință vizibilă:
   lista de sold de stoc nu mai arată loturile consumate integral;
   (2) `CaleaBrutaEchivalenta` rămâne pe TOT istoricul contului (o fereastră
   goală după referință ar fi dat 0 = 0 unui utilizator fără drept pe
   registru și i-ar fi servit soldul din snapshot, necitit prin securitate);
   (3) `MiscareCumulata` e clasă cu setteri, nu record pozițional: peste o
   proiecție de constructor EF nu mai vede membrii și `Where`-ul de deasupra
   cade în evaluare pe client (probat: excepția de traducere); (4) controlul
   `AsteptatStoc` din `SOL-V2b` citește acum REGISTRUL direct, fiindcă
   `StocService.SolduriLaData` pornește el însuși din snapshot și proba ar fi
   devenit circulară. Probe: `SOL-C0` plus 32 de comparații `SOL-C` pe fiecare
   profil — fiecare consumator citit pe scena deschisă și apoi cu referința
   01/2031 și cu referința 03/2031, serializat și comparat la cent, la rând și
   la ordine (inclusiv fișa filtrată pe repartitor și pe „fără repartitor", și
   refuzul „Sold negativ" cu text identic). ModelCheck bugetar 1142/0, privat
   1258/0; `refuzuri.ps1` 246/246 pe host viu Privat (neschimbat ca număr);
   `has-pending-model-changes`: niciuna (2b n-are migrație); codegen
   idempotent, `pnpm build` verde. Măsurat informativ pe clona Flax (11 luni
   închise, referința 11/2025): fișa lui `4111` pe decembrie 193 → 79 ms,
   balanța analitică 318 → 265 ms, soldul de stoc 164 → 43 ms; închiderea
   costă 0,65 s pe prima lună și 3,8 s pe a unsprezecea, redeschiderea între
   0,01 și 4,3 s; snapshot-ul referinței = 171.396 + 7.790 rânduri;
   redeschiderea completă lasă zero perioade închise și zero rânduri. Tot de
   acolo, o consecință de RAPORTARE care nu se vede pe scena ModelCheck:
   balanța analitică a lunii decembrie scade de la 72.910 la 71.167 de
   rânduri, fiindcă 1.743 de chei integral nule nu mai au rând de snapshot —
   toate cu inițial, rulaj și sold zero. Scris în `limite-curente.md`.
3. **`DataInregistrare`** (D4): câmpul, implicitul, gardianul mutat,
   registrele și lotul la data înregistrării, storno-ul, fișa/jurnalul,
   XAF + React (câmp în antetul tuturor tipurilor culese, prin
   `ContaUiBaseline`), `metadata.json`. Oprire: ModelCheck identic;
   Import1C integral cu raport identic (prima probă supremă a feliei).
   *Proba supremă #1 (main, 2026-09-17)*: Import1C integral pe `Import1C.Flax`
   (16.09 22:21 → 17.09 01:04, `--recreeaza --cititori` + `--reclasifica`, exit 0)
   ⇒ `reconciliere-20260916-222253.txt` IDENTIC pe conținut sortat cu baseline-ul
   `reconciliere-20260914-164035.txt` (singura diferență: linia cu timestamp-ul).
   *Executat 2026-09-16, fără opriri; devierile raportate*: `Document.DataInregistrare`
   (`DateOnly`, „Data înregistrării”) cu index filtrat pe rândurile vii; migrația
   `20260916184148_F27Pas3DataInregistrare` (coloană + backfill `= "Data"` în
   aceeași migrație + index), aplicată pe cele patru baze. Implicitul stă la TREI
   seam-uri, niciunul în setter: `DocumentDefaultsController` (la creare, plus un
   abonament `ObjectSpace.ObjectChanged` pe `Data` care o trage după ea cât timp
   cele două erau egale — `OldValue` E populat pe `EFCoreObjectSpace`, prin
   `propertyChangeTracker.GetPreviousValue`, spre deosebire de
   `BaseObjectSpace.Object_PropertyChanged`), `Api/DocumentApply.AplicaDate` (un
   helper nou, chemat din cele 15 `*Apply` în locul lui `doc.Data = dto.Data`) și
   normalizarea din `MotorOperare.CalculeazaSiValideaza` („`default` ⇒ `Data`”),
   care ține Import1C/Migrare neatinse. Ordinea `DataInregistrare >= Data` e
   refuzată în toate trei (gardianul de Committing, adaptorul de scriere, motorul),
   cu text identic. `git diff Motor/MotorOperare.cs` = 8 înlocuiri de dată (gardianul
   de perioadă la operare și la anulare, lotul, `RegistruStoc`, `RegistruContabil`,
   data `MiscareStoc`-urilor gardianului de sold, `conex.DataInregistrare`, capătul
   stornoului) + normalizarea + gardul de ordine + textul stornoului; `RegistruTva.Data`
   NEATINS. `StocService`: o singură linie (`SolduriLaData` la `AplicaValoareIesire`);
   `VerificaGoliri` nu s-a atins — e funcție PURĂ peste rândurile de registru, deci
   urmează data lor fără să știe de document. Imobilizările: `RegistruImobilizari.Data`,
   `VerificaFaraFapteUlterioare`, `VerificaLunaStornarii`, `AmortizareOperataDinLuna`,
   `AmortizareService.Situatie`/`LiniiIesire` și cronologia AMO↔PIF trec pe data
   înregistrării; `Imobilizare.DataPunereInFunctiune`/`DataIesire` rămân pe data
   FIZICĂ (sunt ale fișei, nu ale registrului). Secundarele: DSC-ul din FCL și latura
   pereche a viramentului moștenesc data înregistrării sursei; plata autogenerată din
   FCT NU — `plata.Data` e câmp CULES (`PlataData ?? Data`), iar data sursei ar putea
   fi anterioară lui și ar cădea pe propriul gard de ordine, deci i se normalizează la
   data ei (scris în `limite-curente.md`). Probe: `DIR-V0…V13` pe ambele profiluri
   (documentul întârziat cu registrele, lotul și numărul de serie la înregistrare;
   implicitul care cade pe gardianul perioadei; ordinea refuzată pe ambele uși;
   `RegistruTva` pe data faptului fiscal și conexul cu ambele date; anularea permisă
   în perioada înregistrării; stornoul refuzat înainte de înregistrare și acceptat
   după; FIFO care consumă lotul cules la timp înaintea celui întârziat;
   „Sold negativ” cu text identic; PIF întârziat cu stornoul cerut în luna
   înregistrării; calea fără câmp cules, cu registrele la `Data`).
   Documentele GENERATE primesc data înregistrării la CREARE, prin
   `DocumentApply.Generat` chemat din `AmoApply`/`InchidereTvaApply`
   (`Genereaza`+`Regenereaza`) și din `FacturaIesireApply.GenereazaDescarcare` —
   serviciile rămân neatinse, iar toate ecranele (inclusiv cele XAF) trec prin
   aceste `*Apply`. Fără el, draftul generat ar fi purtat `default` până la
   operare și l-ar fi ARĂTAT așa.
   React: câmpul în cele 15 formulare de culegere, ca afișare pe cele 3 ecrane
   generate, ȘI în cele 15 `spreWrite` (fără asta, o re-salvare a documentului
   citit ar fi rescris tăcut data înregistrării peste data documentului — găsit
   la smoke, nu la citirea codului). Coloana din listele de documente NU s-a
   adăugat: ar fi dus atingerile React la 48 de fișiere, peste pragul din regula
   de oprire (scris în `limite-curente.md`). `refuzuri.ps1` primește două scene
   noi (POST și PUT de DVI cu data înregistrării înaintea datei ⇒ 422).
4. **`PerioadaDeclarare` și rectificativa** (D5) + recuperarea amortizării
   întârziate (D4): câmpurile pe `RegistruTva`, `DeclarareIntarziata` pe
   `PoliticaTva` (seed pe ambele profiluri), completarea în
   `RegistruTvaService`, jurnalele/D300/D394/SAF-T pe perioada de declarare,
   secțiunea de rectificativă. Oprire: ModelCheck identic; D300/D394 pe baza
   de import identice cu cele de dinainte pentru perioadele fără rânduri
   întârziate (nicio schimbare pe istoric).
   *Executat 4a (2026-09-16), cu O OPRIRE raportată; devierile mai jos*:
   `RegistruTva` a primit `PerioadaAn`/`PerioadaLuna` (perioada de DECLARARE) și
   `ScrisLa` (momentul scrierii RÂNDULUI, UTC); `PoliticaTva.DeclarareIntarziata`
   (`PerioadaInregistrarii` | `PerioadaFaptului`) cu seed privat pe DIRECȚIE
   (FCT/DEC/RLF/DVI deduc ⇒ perioada înregistrării, FCL/RDC colectează ⇒ perioada
   faptului); bugetarul rămâne fără rânduri `PoliticaTva`, deci inert. Regula de
   completare stă într-o singură funcție, `RegistruTvaService.PerioadaDeclarare`,
   cu trei apelanți: materializarea din `MotorOperare.Opereaza` (regula ajunge
   acolo prin `RandTva.Regula`, ca motorul să nu recitească politica), rândul
   invers din `Storneaza` (perioada stornării, JT-D5) și `BackfillTva.Scrie`.
   `git diff Motor/MotorOperare.cs` = exact cele două blocuri de materializare.
   Migrația `20260916193817_F27Pas4PerioadaDeclarare` (trei coloane pe registru,
   una pe politică, backfill `PerioadaAn/Luna` din `Data` și `ScrisLa` din
   `Document.DataOperare`, index filtrat `(PerioadaAn, PerioadaLuna)`), aplicată
   pe `Atlas.Conta.BackOffice`, `.Privat` și `Import1C.Flax.Api`; baza
   `Import1C.Flax` a rămas neatinsă (import în curs), iar `ModelCheck.Privat` se
   migrează singură. Consumatorii: toate cele șase citiri (jurnal, decont,
   D300:177, D394:287 și :635, SAF-T:350 și :2231) trec prin helperul unic
   `TvaProiectii.IntreLuni`; `PerioadaAn`/`PerioadaLuna` intră ȘI în cheia de
   grupare a jurnalului și ies pe `JurnalTvaRand`. `TvaProiectii.Rectificativa`
   (rândurile perioadei cu `ScrisLa > InchisaPrimaOara`, plus agregatul pe cheia
   decontului) + `GET api/proiectii/rectificativa-tva?an=&luna=` cu gate dublu
   (404 din perioadă, 403 din registru); `D300Dto`/`D394Dto` primesc
   `Rectificativa` și `DiferenteDeclarat`, completate doar pe o lună exactă.
   SAF-T: doar filtrul; marcajul în fișier rămâne F27-r5.

   **OPRIREA (regula de oprire, punctul „o probă existentă D4 își schimbă
   cifrele”)**: `D4-V3 storno` tăia luna august în două ferestre de ZILE
   (1–20.08 operarea, 21–31.08 stornarea) și cerea cifre diferite pe ele. Cu
   filtrul pe perioada de DECLARARE, care are granularitate de LUNĂ, o fereastră
   sub-lunară nu mai taie nimic: ambele jumătăți întorc luna întreagă. Nu e un
   defect de implementare, ci consecința directă a formulei pin-uite în D5 —
   perioada fiscală E luna, deci un interval de zile nu mai poate selecta o
   parte din ea. Probele care cer o fereastră de zile pe un raport fiscal nu mai
   sunt exprimabile. Am adaptat proba păstrând TOATE cifrele lunii (2500 / 525,
   nrFact 6, Documente 5) și mutând dovada „stornoul e un fapt distinct, în
   perioada stornării” pe registrul citit direct (un rând, −400 / −84, perioada
   de declarare 08); proba afirmă acum explicit și consecința nouă (ambele
   jumătăți = luna întreagă). Decizia de a păstra formula simplă a lui D5 în
   locul unui filtru compus (zile pentru rândurile nemutate, lună pentru cele
   mutate) e a implementării și cere ratificare.

   Alte devieri: (1) contractul spune `DataOperare > InchisaPrimaOara` pentru
   conținutul de rectificativă — `DataOperare` e a DOCUMENTULUI și nu descrie
   rândul de storno scris mai târziu, deci reperul per rând e `ScrisLa`;
   (2) DTO-urile se numesc `D300Dto`/`D394Dto`, nu `D300Rezultat`/`D394Rezultat`;
   (3) `PoliticaTva` are acum două enum-uri fără membru 0, deci scena `F23-V4`
   își culege explicit `DeclarareIntarziata` (tiparul feliei 24, F24-G1) — textul
   și verdictele ei rămân identice; (4) rândul injectat prin SQL brut în `D4-V7`
   a primit cele trei coloane noi, altfel cădea în afara filtrului;
   (5) `D16-V2` citește registrul prin același helper, ca proba independentă să
   măsoare aceeași fereastră ca proiecția.

   *Executat 4b (2026-09-16), fără opriri*: recuperarea amortizării întârziate
   stă în `Motor/AmortizareService.cs` — restul motorului neatins. Lunile lunii M
   se citesc ca DATORATE − ACOPERITE (`LuniDeRecuperat`): datorate = lunile
   întregi de la luna de după `DataPunereInFunctiune` până la M inclusiv,
   acoperite = suma coloanei `Luni` a rândurilor cu `Fel = Amortizare` până la
   sfârșitul lui M. Devierea de la textul pin-uit: plafonul nu se aplică pe
   `datorate`, ci pe `n`, ca DURATĂ RĂMASĂ (`max(durata contabilă, durata
   fiscală) − luni inițiale − acoperite`) cu podeaua 1 — altfel fișa cu luni
   inițiale de deschidere (`LuniAmortizateInitial`, care numără luni de
   DINAINTEA punerii în evidență) ar fi ieșit cu `n` negativ, iar restul de
   rotunjire de după ultima lună a duratei (Citan: 34 de luni pe o durată de 33)
   n-ar mai fi fost postat; cu podeaua, `n = 1` reproduce exact calculul de azi
   și toate probele `IMO-*` rămân neschimbate numeric și ca text. Aritmetica:
   `Cifra` primește `n` și iterează `CotaLunara`, cu `LuniDeLaEveniment`,
   `LuniDeLaPunere` și `RestCurent` avansate la fiecare pas — nu `n × cota`.
   Eligibilitatea: fișa fără eveniment la M−1 dar cu rânduri în M (PIF-ul
   înregistrat întârziat) își citește baza și parametrii la sfârșitul lui M;
   pentru toate celelalte, ca azi. Gardianul `LunaLipsa` e NEATINS.
   `LinieAmortizare.Luni`, `AmortizareLunaraDetaliu.Luni` (`AllowEdit=False`,
   caption „Luni acoperite”, migrația `20260916201658_F27Pas4bLuniAmortizate` cu
   backfill `1`), `rand.Luni = l.Luni` în `MaterializeazaRegistrul`, `Luni` în
   cheia anti-stale `Nepotriviri`; `Inverseaza` copia deja `-r.Luni`, deci
   stornoul n-a cerut nimic. Deductibilul se calculează pe SUMA lunii, cu regula
   de la sfârșitul ei: plafonul lunar se aplică O SINGURĂ dată pe suma
   recuperată (decizie scrisă în `domeniu-si-operare.md`). `LinieAmoDto.Luni` pe
   API, coloana „Luni acoperite” în grila AMO din React și în ListView-ul XAF al
   liniilor. Probe `AMO-V0…V8` în blocul review-ului F26 (scena 2028, cu
   ianuarie–februarie închise prin `PerioadaService.Inchide` și desfăcute la
   final): fișa întârziată recuperează două luni (200,00 = 2 × 100,00, rândul de
   registru cu `Luni` = 2) lângă martorul înregistrat la timp (o lună), degresivul
   (300,00) și acceleratul (500,00) egalează la ban aritmetica pură iterativă,
   plafonul de vehicul se aplică o dată (privat 8.000 fiscal ⇒ 1.500 deductibil;
   bugetarul, fără reguli, deductibil = fiscal), situația la 30.04 e IDENTICĂ cu
   a martorului (3 luni, 300,00), `LunaLipsa` refuză la fel ca înainte, `Luni`
   schimbat pe draft e refuzat anti-stale, stornoul scrie `-2`. ModelCheck
   bugetar 1174/0 (de la 1165), privat 1309/0 (de la 1300) — exact cele nouă
   probe noi, nicio cifră existentă schimbată; `has-pending-model-changes` curat,
   `--dump-metadata` și codegen-ul idempotente, `pnpm build` verde,
   `refuzuri.ps1` 254/254 pe host viu Privat (număr neschimbat).
5. **Corecția** (D6): `CorecteazaId`, `MotivCorectie`, comanda `corecteaza`
   pe REST și XAF (pe fiecare tip, prin controllerul de bază), verificarea la
   commit, efectul fiscal per motiv, ecranul React (buton pe documentul
   stornabil din perioadă închisă + banda „corectează pe …"). Oprire:
   ModelCheck identic + probele corecției; smoke în browser.
   *Executat 5 (2026-09-17), fără opriri*: `Document.CorecteazaId` (FK self
   `Restrict`, index filtrat) + `Document.MotivCorectie` (`EroareMateriala` |
   `FaptNou`), migrația `20260916210018_F27Pas5Corectie`; ambele ALE MOTORULUI
   — `GardianEditare.VerificaDocument` refuză scrierea lor pe ușa securizată,
   la creare și la editare, iar invariantul legăturii
   (`GardianEditare.VerificaLegaturaCorectiei`: motiv prezent, original
   existent și STORNAT, același tip concret, 1:1) rulează la FIECARE commit
   securizat ȘI în comandă, unde gardianul nu e activ. Comanda e
   `Motor/CorectieService.Corecteaza` (tranzacția comenzii, un singur
   `CommitChanges`): storno prin `MotorOperare.Storneaza` NEATINS, apoi
   documentul nou. Culegerea se copiază GENERIC prin metadata EF
   (`Entry(...).Metadata.GetProperties()` — tot lanțul TPT, scalare + FK-uri),
   cu excluderile `ID`/`Stare`/`DataOperare`/`DataInregistrare`/
   `DocumentSursaId`/`Autogenerat`/`CorecteazaId`/`MotivCorectie`/`GCRecord`/
   `OptimisticLockField` pe antet și `ID`/`DocumentId` pe linii; `Numar` și
   `Data` se păstrează (`AsignaNumar` onorează numărul, deci seria nu se
   consumă — NU există nicio unicitate de număr pe serie în model, verificat,
   deci excepția pin-uită n-a fost necesară). Lotul: linia care l-a NĂSCUT
   (`Lot.LinieIntrareId == linia`) primește pe copie un lot PROPRIU nou,
   nefinalizat (tot prin copiere generică, fără `Data`/`PretUnitar`), pe care
   motorul îl finalizează la operare; linia care doar CONSUMĂ păstrează
   `LotId`. Efectul fiscal: rândurile de storno primesc perioada originalului
   la `EroareMateriala` (scris în `CorectieService`, nu în `Storneaza`), iar
   documentul nou o primește prin `RegistruTvaService.PerioadaDeclarare`, care
   capătă `Document` ca prim argument (`MotorOperare` — o singură linie).
   Ușile: `OperareApi.Corecteaza` → `CorectieRezultat`, `POST
   api/documente/{id}/corecteaza` (controller nou, ordinea 400 → 404 → 403 cu
   Create+Write pe TIPUL CONCRET → 422), `PopupWindowShowAction` „Corectează"
   pe `DocumentOperareController` cu `CorectieParametri` non-persistent și
   draftul deschis în `TargetWindow.NewWindow`, grupul „Corecție" pe baza
   `Document` în `ContaUiBaseline` (ascuns prin `[Appearance]` când
   `CorecteazaId` e null). Deviere raportată: refuzul „deja corectat" se
   verifică ÎNAINTEA celui de stare — un original corectat e oricum `Stornat`,
   deci ordinea pin-uită ar fi înghițit mesajul specific. A doua deviere:
   `ReadDto`-urile primesc UN câmp `Corectie` (`CorectieDto` partajat:
   `OriginalId`, `Eticheta`, `Motiv`), nu trei câmpuri plate — o linie pe
   fiecare din cele 18 `*Apply`, prin `ApiProiectii.Corectie`. A treia:
   `[XafDefaultProperty(nameof(Numar))]` pe `Document`, altfel link-ul din
   grupul „Corecție" (și `Document sursă`) arătau GUID-ul rândului. React:
   `nucleu/CorectieDocument.tsx` (buton + dialog cu dată și motiv + banda
   „Corectează pe …"), slot `corectie` pe `DocumentShell`, o linie în fiecare
   din cele 18 ecrane de document. Probe `COR-V0…V19` pe ambele profiluri
   (scena 2034, ianuarie închis): storno-ul la data corecției, documentul nou
   cu `Numar`/`Data` păstrate și culegerea copiată, lotul renăscut vs. lotul
   păstrat, operarea corecției cu linia schimbată, refuzurile (draft, perioadă
   închisă, deja corectat, legătura scrisă de mână, original ne-stornat,
   1:1), snapshot-ul lui 01/2034 identic + reconstrucția cu zero diferențe,
   notele originalului însumând zero; pe privat, în plus, `EroareMateriala` ⇒
   perioada originalului pe storno ȘI pe documentul nou ⇒ rectificativa lui
   01/2034 cu exact cele două rânduri (−100 și +150), `FaptNou` ⇒ regula
   normală D5 pe ambele direcții. ModelCheck bugetar 1191/0 (de la 1174),
   privat 1332/0 (de la 1309); `refuzuri.ps1` 261/261 pe host viu Privat (de la
   254, cu scena `corecteaza`); `has-pending-model-changes` curat,
   `--dump-metadata` și codegen-ul idempotente, `pnpm build` verde; smoke React
   și XAF cu capturi în `run-f27/pas5/`.
6. **Totalul, partidele deschise, împerecherea datată** (D7, D8):
   `TotalStingere` scris la operare (backfill în migrație pentru documentele
   operate existente), `PartidaDeschisa` la închidere, `Imperechere.Data` +
   gardianul + rândul invers, `DocumenteCuRest` rescris, `ReturClient`
   inclus, `sold-parteneri`. Oprire: ModelCheck identic; Import1C integral
   identic; perf pe `DocumenteCuRest` sub 150 ms.
   *Executat 2026-09-17, fără opriri; devierile raportate*: `Document.TotalStingere`
   (`decimal?`, al motorului), `Imperechere.Data` + `InverseazaId`/`Inverseaza`
   (FK self, `Restrict`, unicitate filtrată — legătura e 1:1) și
   `PartidaDeschisa` (`BusinessObjects/Registre/SolduriPerioada.cs`, tabela
   `PartideDeschise`, unic pe `(An, Luna, DocumentId)`); migrația
   `20260916220304_F27Pas6PartideImperecheri` cu TREI backfill-uri
   (`TotalStingere` din liniile de creanță — filtrul `LotId IS NULL` al lui
   `ReturClient` E exprimabil în SQL, deci returul a intrat în backfill corect;
   zero pe documentele operate fără linii; `Imperechere.Data`), aplicată pe
   `Atlas.Conta.BackOffice`, `.Privat` și `ModelCheck.Privat`. Pe Privat
   backfill-ul e verificat cu SQL: 0 diferențe față de recalcul pe 205.168 de
   documente operate/stornate, 46.057 de împerecheri datate, iar 1.638 de RDC-uri
   au total DIFERIT de brutul liniilor (dovada că filtrul s-a aplicat).
   **Devieri**: (1) `MaterializeazaPartide` e chemată din
   `SolduriService.Materializeaza`, nu din `PerioadaService.Inchide` — cei trei
   apelanți (închidere, redeschidere pe P−1, reconstrucție) o vor toți, iar
   `Elimina`/`AreSnapshot` acoperă partidele uniform; `PerioadaService` rămâne
   NEATINS. (2) Backfill-ul lui `Imperechere.Data` ia
   `GREATEST(stingător, stins)`, nu data stingătorului singură, ca invariantul
   de ordine să fie adevărat și pe istoric (pe bazele de azi cele două coincid).
   (3) `InverseazaLaStorno` refuză pe perioada deschisă doar legăturile VII
   (nici inverse, nici deja inversate): altfel o desfacere din fereastra
   deschisă ar fi blocat stornarea documentului pe care tocmai îl elibera.
   (4) `DocumenteCuRest` are o a TREIA ramură de candidați față de contract —
   documentele atinse de o împerechere din fereastra deschisă: un document
   stins integral la închidere lipsește din partide, dar o desfacere ulterioară
   îi readuce restul, iar fără ramura asta proiecția ar fi tăcut incompletă
   (probat: `PAR-V11`). (5) `sold-parteneri` grupează pe dimensiunea
   **Repartitor**, care urmează laturile documentului (00 §5), nu contul de
   terț: pe factura de client atomul de debit al lui 4111 poartă EMITENTUL
   (103.301 din 108.912 rânduri pe baza Privat), deci ecranul nu e „creanța per
   partener" — aceea se citește din partidele deschise. Scris în
   `limite-curente.md` și spus pe ecran; dimensionarea conturilor de terț pe
   partener rămâne decizie separată. (6) `ReturClient` a intrat în uniune, dar
   rândurile lui tot nu apar: creanța unui retur e NEGATIVĂ după operare, iar
   `Rest > 0` o taie (`PAR-V23`) — excluderea prin ramură lipsă nu mai e însă
   necesară, iar antetul cu amânarea e șters.
   `MotorOperare` a primit EXACT trei instrucțiuni (`git diff --stat`: 5+/1−):
   `TotalStingere` la operare, `null` la anulare, apelul `InverseazaLaStorno`
   în locul gardianului de împerecheri la storno (gardianul rămâne al anulării).
   Probe `PAR-V0…V24` pe ambele profiluri (scena 2035; V22/V23 doar pe privat,
   unde există planul de stoc/venit al RDC-ului). ModelCheck bugetar 1215/0,
   privat 1358/0; `refuzuri.ps1` 272/272 pe host viu Privat (+11: `desfa`,
   `sold-parteneri`, `documente-cu-rest?laData=`); `has-pending-model-changes`
   curat, metadata regenerată, codegen idempotent, `pnpm build` verde.
   **Perf** (informativ, host viu Privat, ZERO perioade închise):
   `documente-cu-rest` filtrat pe contrapartida-reper (4.861 FCT) 423 → **147 ms**
   (min din 5; media 161) — câștigul vine din dispariția lui `Brut` (un
   `GROUP BY` peste tot `DocumentDetalii`), nu din partide, care încă nu există
   pe baza aceea; nefiltrat 304 ms / 25.081 de rânduri, cifră confirmată la rând
   cu o recalculare SQL independentă; `sold-parteneri` 337 ms / 70.732 de rânduri.
   Ținta oficială (< 150 ms după 11 luni închise) rămâne a pasului 8.
7. **Constatările și acceptarea** (D2 complet): `PoliticaInchidere` (seed),
   setul inițial de constatări, corpul `Acceptate`, dialogul XAF și ecranul
   React `/perioade`, istoricul cu acceptări. Oprire: ModelCheck identic +
   probele acceptării; `refuzuri.ps1` verde; smoke.
8. **Probele supreme + review advers + docs**: Import1C integral cu
   închiderea lunilor pe parcurs (12 perioade, snapshot verificat contra
   `SUM`), perf după 11 luni închise, `refuzuri.ps1` integral; review advers
   (agent separat, worktree + `MODELCHECK_BAZA_SUFIX`) pe scenariile: operare
   în P în cursa cu închiderea; redeschidere cu rectificativă deja emisă;
   corecție a unui document cu conex/pereche/împerecheri; document întârziat
   cu lot consumat între `Data` și `DataInregistrare`; `EroareMateriala` pe
   colectat cu partener schimbat (D394); partidă deschisă pe document stins
   parțial în P și inversat în P+1; anularea unui document operat în P după
   redeschiderea lui P (snapshot șters, apoi re-închis); decizia 088 (Regula
   durabilă a–k), README-ul jurnalului, `restante.md` (F27-r1…), istoricul,
   CLAUDE.md §Stare, stare-curenta, contractul §Închidere.

## Ce NU intră (amânări cu nume, textul aici)

- **F27-r1** reclasificarea pe 1174 a erorilor semnificative din exerciții
  anterioare: notă contabilă manuală; o politică de „prag de semnificație"
  se decide separat, dacă produsul o cere.
- **F27-r2** scadențar/aging pe partidele deschise (aceeași listă +
  scadența + bucket-uri): felie de raportare, după ce partidele există.
- **F27-r3** cursa închidere ↔ operare prin blocarea rândului perioadei —
  dacă pasul 0 o amână; până atunci verificarea în tranzacție singură,
  fereastra documentată în limite-curente.
- **F27-r4** închiderea de an ca operație distinctă (transferul rezultatului
  121 → 1174/117, soldurile de deschidere ale anului nou): NU e necesară
  pentru solduri (snapshot-ul lui decembrie le acoperă); rămâne cerință de
  produs separată, tot pe lanț.
- **F27-r5** D406/D300/D394 rectificative ca FIȘIER (marcajul de
  rectificativă în XML/PDF): proiecțiile expun conținutul; formatul de
  depunere e felie proprie.
- **F27-r6** constatări de închidere pe reconcilierea 1C (documente
  neimportate în P): conectorul, nu mecanismul.
- **F27-r7** soldul în lookup-urile de partener din culegere (coloană
  `Sold` pe `Partener` prin `sold-parteneri`): la cererea ecranelor.
- **F27-r8** concurența între operatori (25f) rămâne parcată; felia rezolvă
  doar cursa perioadei.
- **F27-r9** `DataInregistrare` pe documentele generate (AMO/ITV/DSC/NIR
  autogenerate): moștenesc data sursei sau ultima zi a lunii, ca azi;
  editabilitatea ei pe generate se decide la cerere.
- **F27-r10** (deschisă la pasul 2b) SAF-T: inițialul de stoc pe registru
  integral; mutarea pe referință cere schimbarea semanticii `exista`/`Randuri`
  din `SoldPeCheie`/`SoldPeTipStocNeraportat`, decizie de raportare, nu de
  motor. Inițialul de CONT al SAF-T e deja pe referință, prin `Balanta`.

## Relația cu izolarea motorului (contractul IM)

Felia 27 precede IM: contractul de citire „Stoc" (IM-D4) devine
implementabil fără a încărca istoricul (`SolduriService` e primul adaptor al
lui), iar „Relații" primește perioada ca lanț. Nimic din felia 27 nu
contrazice IM-D1…D10; `SolduriService` se scrie de la început ca serviciu cu
întrebări economice (chei + dată → solduri), nu ca `IQueryable` expus.
