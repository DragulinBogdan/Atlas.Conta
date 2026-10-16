# Raportare

**Actualizat: 2026-10-16.** [Index](README.md)

Explicația deciziei persistată în cub, invarianții care o verifică, citirile contabile ale cubului și identitățile cititorilor lui.

## Explicația deciziei

Contractul acceptat își persistă deciziile și ipotezele: de ce a costat
ieșirea atât, din ce sold, ce partide a stins. (X-D4)

- **Unde**: `Tranzactie.Explicatie` (`jsonb`), pe prima tranzacție scrisă a
  contractului — `Operare` când există, altfel `Transfer` (BTR, ASM fără
  schimb de cont). A doua tranzacție a aceluiași contract o referă prin
  `ExplicatieDinId`. Se scrie în același `INSERT` cu rândul și nu se mai
  modifică. `Storno`, `Deschidere`, transferul împerecherii, desfacerea și
  stingerea de deschidere nu au explicație; baza refuză prin
  `CK_Tranzactie_Explicatie` și `CK_Tranzactie_ExplicatieDin`. (X-D4)
- **Ce**: `Cub.Explicatie` — versiunea schemei (`v` = 2), numele
  declarantului, `JumatatiDeBan`, deciziile și ipotezele contractului în
  ordinea declarației, cu linia și unitatea lor. Un cititor refuză o versiune,
  o decizie sau o ipoteză pe care nu o cunoaște; versiunea 1 nu are cititor. (X-D4)
- **Regula care a decis** (D9-A8): pentru fiecare rând de politică din care
  declarantul a luat un fapt folosit la o postare, ipoteza
  `VersiunePolitica(Fel, Rand, Versiune)` — clasa rândului, identificatorul
  și contorul lui la operare, una pe rând, fără dubluri
  (`Declaratii/PoliticiConsumate`). Se rețin: regula de contare câștigătoare
  a fiecărei linii (numită și pe `ContRezolvat.Regula`), regula de stoc a
  LDI, politica de TVA când declarația are fapt fiscal, politica de
  diferență a NIR-ului conex și politica
  de amortizare a fișelor atinse. Nu se rețin nomenclatoarele care decid
  (tipul de material, tipul de TVA, steagurile contului) și nici conținutul
  regulii la operare (D9-r3). Un declarant care nu consumă reguli (ASM, BTR,
  NTC) nu scrie ipoteza. Lista nominală, declarant cu declarant:
  `docs/nucleu/tr-d9-pas7c-versiunea-politicii.md`. (D9-A8, D9-r3)
- **Ieșirea pe lot** are exact o decizie de valoare: `ValoareIesire` când
  valoarea vine din soldul citit (BCS, DSC, BTR, LDI minus, consumul ASM),
  `ValoareDeclarata` când o dă altă sursă — linia documentului (RLF și RDC, la
  valoarea culeasă) sau recepția facturii (NIR-minus). Sursele sunt
  `Declaratii.SurseValoare`. Decizia poartă cantitatea și valoarea pozitive;
  postarea-sursă a unui transfer le are negative. Mecanismul e al
  declarantului, nu al explicației: `IDeclarant.SursaValoareDeclarata` e
  `null` pentru declaranții care evaluează din sold și numește sursa pentru
  cei trei care declară valoarea (NIR, RLF, RDC). Niciun declarant nu le
  amestecă. (X-D4)
- **Soldul citit** (`SoldUnitateCitit`) e soldul net al unității la data
  documentului, fără documentul curent, o dată per unitate. Soldul dinaintea
  fiecărei ieșiri următoare de pe aceeași unitate se derivă din deciziile
  anterioare (`Explicatie.IesiriEvaluate`). (X-D4)
- **Stingerea FIFO** are câte o `AlocareFifo` per linie și partidă stinsă,
  iar partida stinsă are soldul ei citit între ipoteze; partida proprie a
  documentului are `PartidaDeschisa`. (X-D4)
- **Stornoul** se explică prin original: `Cub.Citiri.Explicatii.PeTranzactie`
  urmează `InversaDin` și întoarce explicațiile purtătorilor de origine.
  **Anularea operării** șterge explicația odată cu tranzacția. (X-D4)
Dry-run-ul nu persistă nimic. O declarație pe care nucleul nu o poate
construi (`ArgumentException`) sau un declarant care nu întoarce nici
declarație, nici refuz dau refuzul `DECLARATIE_INVALIDA`, pe dry-run și pe
operare, în aceeași formă ca orice refuz al declarației. (S-r11)

### Invarianții explicației și ai tranzacției

- **Acoperirea explicației** (`Explicatii.VerificaAcoperire`, în `INV-CUB`):
  `CITIRE_EXPLICATIE_LIPSA` (o tranzacție `Operare` sau o ieșire pe lot fără
  explicație), `…_REFERINTA` (referință spre o tranzacție fără explicație sau
  a altui document), `…_STORNO` (inversa pe lot nu oglindește originalul),
  `…_IESIRE` (ieșirile postate și deciziile de valoare nu corespund una la
  una pe linie, unitate, cantitate și valoare), `…_EVALUARE` (valoarea unei
  `ValoareIesire` nu rezultă din soldul persistat, cu rotunjirea bazei),
  `…_STINGERE` (alocare fără postare, postare pe partida altui document
  fără alocare, partidă stinsă fără sold citit sau cu alocări peste soldul
  citit), `…_MECANISM` (explicația numește alt declarant decât cel al
  documentului, poartă o valoare declarată la un declarant care evaluează
  din sold, sau o valoare evaluată ori altă sursă la unul care declară).
  Declarantul se ia din document, nu din explicație. (X-D4)
- **Transferul persistat conservă pe cont** (`Invarianti.VerificaTransferuri`,
  `CITIRE_TRANSFER_NECONSERVAT`, în `INV-CUB`): în orice tranzacție `Transfer`
  valoarea se anulează pe (cont, latură) și cantitatea pe (cont, produs). E
  regula de la contractare (090f), reverificată pe ce s-a scris. Acoperă
  contul capătului de destinație, pe care nici acoperirea cantitativă a
  stocului, nici reconcilierea (a) nu îl văd. (090f)
- **Perechea persistată e cea de la contractare** (`Invarianti.VerificaPerechi`,
  în `INV-CUB`): `CITIRE_PERECHE_INVALIDA` — pe (tranzacție, ordinal) nu stau
  exact două postări cu același document, aceeași linie, aceeași dată,
  cantități opuse și valorile potrivite laturilor felului; `CITIRE_PERECHE_LIPSA` — postare fără
  ordinal în afara deschiderii și a liniilor transformării. (D9-A2)
- **Ordinalul numără postările tranzacției** (`Invarianti.VerificaOrdinale`,
  în `INV-CUB`): `CITIRE_ORDINAL_INVALID` — postările unei tranzacții nu sunt
  numărate de la 1, fără goluri și fără dubluri. (117)

## Identitățile cititorilor

Cititorii din `Cub/Citiri` expun fiecare identitate separat; nimeni nu
alege între două coloane. (C111-D10)

| Identitatea | Domeniul | Coloana |
|---|---|---|
| titularul partidei | postarea cu `FelUnitate.Partida` (`Citiri/Partide`) | `Repartitor` |
| terțul capătului | postarea contabilă a unui document (`Citiri/Plati`) | `Repartitor`, când e extern |
| partenerul fiscal | postarea cu fapt fiscal (`Citiri/Fiscale`) | `PartenerFiscal` |
| locul | postarea cu fapt fiscal sau capătul intern cu partidă; îl citesc lista postărilor și `INV-CUB`, nicio citire de sold | `Loc` |
| terțul comercial | postarea pe un cont cu `RolTert` (`Contabil.Jurnal`, `PostareJurnal.TertComercial`) | `Repartitor`, când e extern |
| locul lotului | postarea cu lot (`Citiri/Loturi`) | `Repartitor` |
| repartitorul intern fără lot | locul fișei, poziția fără fișă (`Citiri/Imobilizari`) | `Repartitor` |
| contraponderea | postarea marcată | `Contrapondere` |
| axa analitică | atomul contabil (`Contabil.Analitice`) | `Repartitor` |

„Extern” se judecă într-un singur loc, `Contabil.Externi`: repartitorii de
fel partener sau angajat. (C111-D10)

Regula terțului comercial în SAF-T (S11) și cea a partenerului fiscal în
D394 (S13) sunt în [saft § Terțul comercial](saft.md#terțul-comercial) și
[tva § D300 și D394](tva.md#d300-și-d394). (S11, S13)

## Citirile contabile ale cubului

`Postari` compune interogarea fără diagnostic global per apel;
inversele Operare/Deschidere se clasifică prin ID și spațiul originii.
Balanța, fișa, jurnalul și soldul pe partener citesc cubul, împreună cu
snapshot-ul contabil. Fișa afișează toate conturile corespondente, fără să
inventeze o pereche; jurnalul are identitatea postare × spațiu. (098, 099)

Axa analitică e una: repartitorul de sold al postării. Capătul intern apare
pe repartitorul lui intern, cu sau fără fapt fiscal; contul propriu apare ca
repartitor; partenerul numai fiscal și locul nu sunt pe axă. `repartitorId`
filtrează repartitorul de sold, de orice fel; `repartitorNul`, pe fișă,
înseamnă fără niciun repartitor. Capătul intern cu partidă (419, 408, 4091)
apare pe titular, nu pe locul intern. (S1, S2, C111-Q6)

Snapshot-ul contabil (`SoldPerioadaContabil`) are aceeași axă, `RepartitorId`,
fără coloană de gestiune; citirea dinamică și cea din snapshot dau aceleași
cifre. Snapshot-ul se scrie numai din cub și se reconstruiește din postări.
Citirile securizate folosesc postările autorizate, nu snapshot-ul global,
pentru a păstra permisiunile pe rând și membru. (S1, C111-Q6, 098, 099)

### Balanța pe an

- Balanța are graniță de an calendaristic, independentă de închiderea
  exercițiului. `SoldInceputAn` (debit și credit) e soldul net al rândului la
  1 ianuarie al anului `dataStart`, deci soldul final de la 31.12 pe aceeași
  cheie. `Initial*` sunt sumele precedente: `SoldInceputAn` plus rulajele de
  la 1 ianuarie până înaintea startului. Soldul inițial și cel final rămân
  nete și neschimbate. Nu se scrie nicio tranzacție de deschidere. (E3-D8)
- Netarea se face pe cheia rândului: contul pe balanța sintetică, contul și
  repartitorul pe cea analitică. Pe planul de conturi, `SoldInceputAn` se
  netează la nod, ca soldurile, iar sumele precedente se adună din frunze.
  (E3-D8)
- Cu startul pe 1 ianuarie, balanța citește cumulul o singură dată. Altfel
  adaugă cumulul până la 31.12 al anului precedent, care pornește din
  snapshot-ul lui decembrie închis, dacă există. Rulajele dintre 1 ianuarie
  și start sunt diferența brută dintre cele două cumuluri, fiindcă debitul și
  creditul se cumulează separat. (E3-D8, F27-D3)
- SAF-T citește din balanță numai soldurile nete; filtrul conturilor de stoc
  raportabile din rezumatul S3 cere sold inițial net sau rulaj, nu sume
  brute. (E3-D8)

## Limitele implementării

- Rapoartele cumulate ale API-ului (balanță, fișă, sold parteneri, partide cu
  rest, stoc) citesc `Vizibila` și recitesc istoricul la fiecare cerere; nu
  există un al doilea model de citire. (104b, FZ-r1, X-D5)
- `PartideCuRest` caută documentul deschizător al fiecărei partide
  parcurgând toate postările de partidă (`Partide.Origini`), și când
  soldurile vin din snapshot: costul crește liniar cu istoricul. (F27-r16,
  X-D5, D9-D10 b)
- `documente-cu-rest` e proiecția scumpă: uniunea documentelor operate ale
  celor opt tipuri, cu filtrul abia în `WHERE`-ul exterior, plus legăturile
  ca tabele derivate. Corelarea legăturii scoate scanările secvențiale pe
  panoul filtrat, dar mută costul pe calea nefiltrată, consumată de
  constatarea de rest scadent la închidere; măsurătorile sunt în
  [p5-perf-masuratori](../api/p5-perf-masuratori.md). (F27-r16)
- Fișa de cont și balanța analitică depășesc ținta de timp a grilei: la fișă,
  costul e cadrul cererii (securitatea pe ObjectSpace, hidratarea,
  serializarea), nu proiecția; la balanța analitică, cardinalitatea cheii
  `Cont × Repartitor`, pe care `work_mem` n-o ajută. (F27-r14, F27-r15)
- Contrapartida domină costul registrului jurnal la volum: perechea fiecărui
  rând se caută prin indexul `(TranzactieId, Pereche)`. (C111-Q20)
- Fișa contului și registrul jurnal compun SQL-ul de mână în jurul surselor
  din LINQ, fiindcă egalitatea pe `Pereche`, nulabilă, nu iese din LINQ fără
  ramură de nul. `Pereche` nulă înseamnă „fără pereche”; valoarea 0 nu e
  folosită. (C111-Q20)
- Explicația repetă câte două `ContRezolvat` și unitatea întreagă pe fiecare
  linie. Contorul rândului de politică e `OptimisticLockField`: nu-l mișcă o
  scriere SQL directă sau restaurarea bazei, iar un rând șters și recreat
  apare ca dispărut. (X-D5, D9-A8)

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (7; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): 42 (amendată de 104, 106), 66, 67, 90 (amendată de 91, 92, 93, 95, 96, 110, 119), 104 (amendată de 106), 117, 124.
<!-- /gen:decizii -->
