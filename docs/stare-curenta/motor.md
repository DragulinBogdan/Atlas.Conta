# Motorul de postare

**Actualizat: 2026-10-17.** [Index](README.md)

Contarea și dimensiunile, precizia numerică, nucleul pur, declarația fluxului per tip, laturile documentului, cubul persistat și scrierea serială.

## Contare și dimensiuni

Planul de conturi este sintetic. Analiticele se obțin din dimensiuni în
raportare, fără conturi analitice generate și persistate. (10)

Nomenclatoarele-dimensiune (cod funcțional, cod economic, sursă de
finanțare, proiect, angajament, clasificarea imobilizărilor) derivă din baza abstractă `Dimensiune`:
cod, denumire și căutare fără diacritice. Baza e contractul pentru codul
generic (`T : Dimensiune`) și rămâne în afara modelului EF: fiecare
derivată are tabelul și coloana de căutare proprie. `Unitate` nu derivă din
`Dimensiune`.

`SursaCont` alege cont explicit, contul implicit al tipului material sau
contul implicit al repartitorului de pe una dintre laturi. Motorul nu
conține simboluri de cont specifice profilului. (26b, 29)

Potrivirea politicilor pe o linie este un set de funcții pure pe fapte plate
(`Motor/Potrivire.cs`), consumat de motor, de gardurile documentelor și de
explicație; entitățile se mapează la fapte o singură dată (`Fapte`). La
contare, tipul material exact are prioritate față de filtrul de natură, iar
acesta față de regula generică; `SemnFiltru` elimină regulile incompatibile
înaintea alegerii; câștigătorul e primul de la nivelul cel mai înalt, în
ordinea în care baza întoarce regulile. La stoc, per latură, regulile
specifice pe clasă bat regula generică, iar generica se aplică doar liniilor
cu natura de stoc. Lipsa unei reguli nu produce o notă; tipurile pentru care
tăcerea ar pierde postarea declară pe clasă `[GardContare(natură, nivel
minim, mesaj)]` — aplicat o singură dată în validarea de bază a documentului,
pe toate axele potrivirii (FCL, DSC, RDC cer regulă exactă pe liniile de
stoc; trezoreria cere cel puțin natura pe liniile de virament). Postarea
explicită este permisă numai prin contractele declarate de tip și de linie. (26c, 28a, 32a, 64, 84e–f)

Dimensiunile disponibile sunt repartitor, material, cod funcțional, cod
economic, sursă de finanțare, unitate, proiect și centru de cost. FK-urile
culese sunt pe frunzele liniilor; `DimensiuniCulese()` furnizează motorului
un value object nepersistat. Regulile și registrele păstrează câmpurile plat. (15, 54c)

Pentru fiecare latură, rezolvarea urmează: valoarea liniei → override-ul
laturii din regulă → valorile comune ale regulii → default-ul polimorf al
antetului → materialul din lot. Postarea explicită a liniei are prioritate.
În validarea clasificației bugetare, angajamentul poate satisface cerința de
cod economic. (25f, 32c, 33a)

`Cont.DimensiuniObligatorii` și `Cont.FeluriRepartitor` se verifică pe
mișcările contabile ale contractului (`Cub/GardAnaliza`): gardul judecă numai
capătul, iar cele două familii sunt independente. Repartitorul judecat e
repartitorul de sold al capătului (`Capat.Repartitor`); latura documentului
nu se citește. Contul declară felurile admise (C111-D4):

| `FeluriRepartitor` | Gardul |
|---|---|
| mulțime vidă (`Nedeclarat`) | nu judecă repartitorul |
| feluri, fără `Niciunul` | cere repartitor, de unul din feluri |
| feluri, cu `Niciunul` | admite lipsa; un repartitor prezent e de unul din feluri |

Un repartitor de alt fel e refuzat cu „Repartitor de felul … (primit: …)”
(S6). Capătul marcat cu contrapondere e scutit numai de prezența
repartitorului: felul unuia prezent și celelalte dimensiuni i se judecă
integral. Aceeași judecată se aplică postărilor de deschidere. Seed-ul
bugetar declară 20 de conturi, cele cu defalcarea R din CPLAN: partener pe
furnizori, clienți, debitori și conturile de finanțare; partener și angajat
pe 462.01.01 și 462.01.09; cont propriu pe 552.00.00; partener sau niciunul
pe 437.01.00, 437.02.00 și 803.00.01 (111-r2). Seed-ul privat nu declară
nimic. Urmarea pe bugetar: nota contabilă pe un cont de terți fără partener
pe linie e refuzată, fiindcă latura fără repartitor explicit ia locul notei,
care e intern.
Materialul e produsul; restul e analiza postării; angajamentul liniei ține
loc de cod economic. Mutările (BTR, PIF), transformările (ASM) și cartea
fiscală nu intră. Recepția liniei de stoc a facturii e păzită la operarea
facturii, nu a NIR-ului conex; diferența NIR-ului acoperit rămâne la gardul
recepției conexe, care cere terțul real pe contraponderea de inventar când
contul cere repartitor. Gardul refuză declarantul care uită coordonata
(proba directă `GARD-ANALIZA`). (TR-D9a: D9-D4, D9-A3; C111-D4)

## Precizie numerică

Cantitățile folosesc `numeric(18,3)`, sumele `numeric(18,2)`, iar prețurile
`numeric(18,6)`. Convențiile sunt centralizate în `Scara`; o proprietate
decimală fără mapare explicită este refuzată la verificarea modelului. O
valoare care depășește scara coloanei e refuzată de gardianul de commit, pe
orice ușă securizată, cu numele câmpului. Nu ajunge excepție de bază. (49e, 104c)

Prețul se rotunjește cu `AwayFromZero`. Rotunjirea sumelor respectă profilul
fixat al bazei. Culegerea, motorul și raportarea folosesc aceeași convenție;
clientul nu introduce o rotunjire contabilă independentă. (42c, 51c, 52a)

## Nucleul pur

`nou/Atlas.Conta.Nucleu` ține tipurile cubului și regulile pure ale deciziei
90, fără nicio referință la EF/XAF/HTTP; singurul lui consumator e
`Atlas.Conta.BackOffice.Module` (folderul `Declaratii/`). (TR-D6b) Ce ține
nucleul (contractul `docs/nucleu/tr-d6a-nucleu-pur-contract.md`):

- **Cubul**: `Coordonate` (cont, latură OBLIGATORIE, dată, repartitor,
  contrapondere, produs, unitate, cod TVA compus, perioadă de declarare,
  partener fiscal, loc, valută, carte, analiză ×6), `Postare` cu trei măsuri
  la scară apărată în constructor și în `with`, `Tranzactie` cu felul
  `Operare | Storno | Transfer | Deschidere`; spațiul (`Stoc` = unitate de
  tip lot, restul `Contabil`) e funcție, nu câmp. Jurnalul TVA e proiecție pe
  `CodTva`, nu spațiu separat. (N-D2)
- **Coordonatele de repartitor** (111a, 111b, C111-D2), aceleași pe `Capat`
  și pe `Coordonate`:
  - `Repartitor` = `(Id, Fel)`: repartitorul de sold al postării, unul
    singur; nul = niciunul. `FelRepartitor` e al nucleului: partenerul și
    angajatul sunt externi, gestiunea și unitatea internă sunt interne,
    contul propriu nu e nici una, nici alta.
  - `Contrapondere` (terț, consum, inventar, transformare) marchează postarea
    care ține contracantitatea în afara evidenței; nul = postare în evidență.
  - `PartenerFiscal` stă numai pe postarea cu fapt fiscal.
  - `Loc` e locul intern al postării când nu e repartitorul ei de sold: pe
    orice postare cu fapt fiscal și pe capătul intern căruia titularul unei
    partide i-a luat locul (C111-Q14). Nu intră în nicio cheie de sold.
- **Conservarea**, structurală: valoarea per `Carte`, cantitatea per produs
  peste TOATE postările, transferul cu Σ = 0 per (cont, latură), valoarea
  negativă doar în `Storno`/`Transfer`, postările datate ca tranzacția,
  deschiderea fără document, scutită de Σ. (N-D3, N-D4)
- **Forma coordonatelor** (C111-D2, invarianții 1–4), verificată tot de
  `Conservare`:
  1. cantitatea cere produs; nemarcată cu contrapondere, cere lot
     (`UNITATE_LIPSA`) și repartitor intern (`GESTIUNE_LIPSA`);
  2. lotul cere produsul lui și repartitor intern; partida cere repartitor
     extern (`PARTENER_LIPSA`), egal cu titularul ei, și contul ei
     (`UNITATE_NEPOTRIVITA`);
  3. `PartenerFiscal` apare numai cu fapt fiscal
     (`PARTENER_FISCAL_FARA_FAPT`); `Loc`, numai cu fapt fiscal sau pe o
     postare cu partidă (`LOC_FARA_TEMEI`); faptul fiscal cere `Loc`
     (`LOC_FISCAL_LIPSA`), iar `PartenerFiscal` poate lipsi;
  4. contraponderea transformării n-are repartitor, unitate, valoare, valută
     sau fapt fiscal (`CONTRAPONDERE_TRANSFORMARE_INVALIDA`).

  Inversa fiscală poartă același `PartenerFiscal` și același `Loc` ca
  originalul (invariantul 5): stornoul le copiază de pe postarea inversată,
  iar `INV-CUB` le compară pe identitatea originalului
  (`CITIRE_FISCAL_INVERSA`), nu pe ordinalul perechii.
- **Perechea** (`Postare.Pereche`, D9-A2): ordinalul mișcării sau mutării în
  tranzacția ei, dat de `Motor` (mișcările 1…m în `Operare`, mutările 1…k în
  `Transfer`); transformările și deschiderea n-au ordinal. `Conservare`
  (`PERECHE_INVALIDA`) cere pe fiecare ordinal nenul exact două postări cu
  aceeași cauză și cantități opuse: pe laturi opuse cu aceeași valoare
  (`Operare`), pe aceeași latură cu valori opuse (`Transfer`); stornoul
  acceptă ambele forme. Nu e coordonată: nu intră în chei de sold și
  snapshot-uri. O citesc fișa contului și registrul jurnal: contrapartida
  unui rând e contul celeilalte postări a perechii, iar rândul fără pereche
  (deschiderea) n-are contrapartidă (111 (d), C111 S9). Cealaltă postare se
  caută pe (tranzacție, ordinal) prin `Contabil.Perechi`, mărginită de
  intervalul cititorului: vizibilitatea și data sunt ale perechii, deci
  filtrul pe felul tranzacției nu se repetă. (C111)
- **Unitatea** (lot = partidă = fișă): raportul = cost / curs / valoare
  rămasă ca citire; partida deschisă de un document are id determinist din
  (document, cont, partener), conform 092; identitățile istorice sunt păstrate
  la citire și stingere, fără rescrierea postărilor. **FIFO**: unitatea numită
  pe linie se consumă întâi,
  fără cădere pe FIFO, apoi (data deschiderii, documentul deschizător, id;
  114), tolerant cu rest
  întors. **Evaluarea ieșirii** pe raportul CURENT al unității, ultima
  ieșire ia restul ⇒ cantitate zero ⇒ valoare zero; diferența față de prețul
  înghețat pe lot, substituit doar la golire, e declarată; documentele nu
  pot produce două prețuri pe același lot, deci apare doar din deschideri și
  import. (N-D6, N-D7, N-r3)
- **Repartizarea** = Hamilton ierarhic, Σ = total exact, singura primitivă
  de distribuție. **TVA**: taxa se decide și se rotunjește pe document ×
  cotă, se postează per linie prin Hamilton peste |net|, pe fiecare semn
  separat; taxa dată pe facturile primite se validează cu toleranță, nu se
  recalculează. Aceeași taxă stă pe linia documentului și pe postare: 3 × 0,01
  la 21 % ⇒ 0,01 pe o singură linie, peste tot (109). Toleranța e rândul de
  politică `PoliticaTva.TolerantaTaxa` (vezi
  [gardurile declaranților](#gardurile-declaranților-ca-dată-sau-ca-regulă)).
  (N-D8, 090j, N-r4)
- **Sold** = Σ pe orice cheie, pe tranzacții, cu `Transfer` exclus ca
  parametru al citirii; snapshot-ul e lema `Sold(≤t) = Sold(≤t0) +
  Sold(t0<d≤t)`. **Stornoul** = inversul cauzat ∪ atribuit, fără schimb de
  latură, tranzacție distinctă; atribuirile de la nominalizarea partidelor
  intră în selecție (090 (i), 119 g). Alocările valorice atribuite pe lot nu
  intră în stornoul beneficiarului: efectul lor încă activ îl mută corecția,
  în aceeași comandă, într-o `Operare` proprie scrisă la data stornării, câte
  una pe fiecare corecție cu compensare nenulă
  (`Materializare.ScrieCompensari`). (N-D9, N-D10, N-r5, 119 b–d)
- **Motorul**: `Declaratie` (mișcări pe coordonate rezolvate + decizii +
  ipoteze) → exact o tranzacție `Operare` (sau `Transfer` din mutări pe
  același cont) → `Contract` acceptat/refuzat, determinist, cu contorul de
  jumătăți de ban al instanței `Rotunjire` primite. `Decizie`/`Ipoteza` sunt
  ierarhii închise cu cazurile pilotului. (N-D11)

## Declarația fluxului per tip

Forma care înlocuiește hook-urile de motor ale frunzelor (contractul
`docs/nucleu/tr-d6b-declaratia-fluxului-contract.md`, B-D1…B-D10).

### Operandul și declarantul

- **Operandul închis** (`Declaratii/Operand.cs`, DTO): documentul (laturile
  ca `RepartitorFapt` cu felul din discriminatorul `ClrType`), liniile cu
  lotul, prețul, produsul și dimensiunile culese, politica (regulile de
  contare/stoc, politica de TVA, tipurile de TVA cu conturile lor, conturile
  atinse cu `UrmarestePartide`), starea citită (soldurile loturilor la data
  înregistrării fără documentul curent, restul partidei sursei, perioada
  de declarare, perioada deschisă, toleranța taxei). Faptele de politică
  poartă contorul rândului (`OptimisticLockField`, D9-A8).
  Îl construiește `Motor/Fapte.Operand(os, doc)` PE SETURI (o interogare
  per tabelă, probat `≤ 16` cu `NumaratorSql`). Câmpurile de frunză fără
  interfață declarată (`Valuta`/`Curs` pe FCT) NU intră. Politicile pe tip
  (validare, numerotare, scadență, conex, TVA) se citesc din bază, fără
  căutarea XAF în obiectele necomise: motorul operează în ObjectSpace
  propriu (58c), iar căutarea compila criteriul la fiecare politică lipsă. (125c)
  Politica de diferențe a tipului intră în operand numai pentru declaranții
  care o cer (`IDeclarantCuDiferente`, azi RLF), ca restul tipurilor să nu
  plătească interogarea. (128)
- **Declarantul** (`IDeclarant.Declara(Operand, Rotunjire, refuzuri) →
  Declaratie?`): pur, fără `IObjectSpace`, fără entități; frunza îl numește
  printr-o singură METODĂ polimorfă `Document.Declarant()` (`null` = tipul nu
  declară încă; o proprietate ar intra în metadata clientului). Driverul
  `Declaratii/Contractare.Contracteaza(os, doc)` = operand → declarant →
  `Motor.Opereaza`; `RefuzException` a primitivelor devine refuz.
  Rezolvările comune (`Contari`): contul fiecărei laturi prin
  `Potrivire.Contare/Cont` și coalesce-ul dimensiunilor prin
  `DimensiuniResolver` — aceleași funcții pure ca motorul vechi.

### Regula coordonatelor

Capătul poartă repartitorul al cărui sold îl mișcă. Repartitorul de sold îl
dă declarantul (latura, linia, regula de contare); partenerul fiscal îl dă
politica de TVA. Niciunul nu se deduce din clasa contului. (C111-D5; B-D8
pct. 9, 10)

- **Capătul intern** (stoc, cheltuială, venit, fișă) poartă repartitorul
  intern al documentului, cu sau fără fapt fiscal.
- **Capătul de terț** îl pune un singur helper, `Declaratii/Terti.Capat`,
  pe capătul pe care i-l dă apelantul: titularul și partida pe un cont cu
  `UrmarestePartide`; terțul singur pe capătul fără repartitor al unui cont
  care declară feluri de repartitor (`ContFapt.DeclaraRepartitor`), și când
  contul admite lipsa; prezența și felul le judecă gardul, deci terțul de
  alt fel decât cele declarate e refuzat, nu omis (C111-F-R1). Capătul din cartea
  fiscală nu primește terț. Terțul e latura externă a documentului; la DEC
  și NTC e repartitorul liniei, iar DEC, NTC, DSC și diferența NIR îl pun
  pe capătul extern necondiționat (T-D13 g).
- **Capătul intern cu partidă** (419, 408, 4091): titularul partidei ia
  locul repartitorului intern, iar acesta rămâne pe postare ca `Loc`, cu
  sau fără fapt fiscal. Îl scrie `Terti.Capat`, din repartitorul intern pe
  care capătul îl avea (C111-Q14).
- **Faptul fiscal** are un singur scriitor, `Fiscal.CuFapt`: codul,
  reperul, perioada, partenerul fiscal și locul. `PartenerFiscal` e latura
  numită de `PoliticaTva.SursaContrapartida` (predatorul sau primitorul); o
  sursă care nu numește o latură îl lasă nul (C111-R16). `Loc` e singura
  latură internă a documentului (`Fiscal.Locul`, din `RepartitorFapt.Parte`,
  nu din politică; C111-Q10): fără exact una, faptul rămâne fără loc și e
  refuzat (`LOC_FISCAL_LIPSA`). Locul e al documentului: baza și taxa
  poartă același loc, și când repartitorul de sold al bazei e altul. `Loc`
  se scrie și când e egal cu `Repartitor`.
- **Capătul de taxă** al direcției are ca repartitor locul. Autocolectarea
  de la taxarea inversă n-are repartitor: poartă numai partenerul fiscal și
  locul.
- **Contracantitatea** stă pe postarea marcată cu `Contrapondere`: terț la
  recepție (FCT, NIR), la retururi (RLF, RDC) și la descărcarea de gestiune
  (DSC); inventar la plusul LDI și la diferența NIR; consum la minusul LDI;
  transformare la ASM, fără repartitor. Pe postarea marcată terțul stă în
  `Repartitor` când declarantul îl numește: diferența NIR, DSC cu primitor
  extern, returul de la client.
- **Trezoreria** alege capătul de terț structural (predatorul dă pe
  credit, primitorul primește pe debit). Capătul propriu numit de regulă
  prin latură poartă contul propriu ca repartitor; cu ambele conturi numite
  explicit rămâne fără repartitor, vizibil gardului (SC-PLT-09, SC-INC-09).

Coordonata nu deschide partidă și nu schimbă unitatea. Probe: SC-PLT-08 pe
ambele profiluri și catalogul formelor, `docs/nucleu/scenarii/C111.md`
(`--scenarii C111`); pe seed, 462.01.09 (bugetar) e singurul cont fără
partide care cere repartitor și e atins de catalog.

### Declaranții pe tip

- **BCS** (`DeclarantBonConsum`): o mișcare per linie — lotul iese de pe
  contul creditor al regulii din gestiunea predatoare și intră pe contul de
  cheltuială al locului de consum (repartitorul real, cu lotul ca unitate
  re-cheiată pe contul postării), evaluat pe raportul curent în secvența
  liniilor.
- **PLT/INC** (`DeclarantTrezorerie`, O clasă pentru ambele: diferența e în
  regula de contare și în contul cu `UrmarestePartide`): o mișcare per linie de
  defalcare; plata născută din factură numește partida sursei prin
  `Fifo.Nominalizeaza` cât ține restul ei, excedentul pe partida proprie
  ca a doua mișcare (linia se sparge, inclusiv piciorul de bani); fără sursă,
  partida proprie (avans); la virament contul propriu stă numai pe capătul
  al cărui cont vine de la latură, iar capătul de tranzit n-are repartitor
  (C111 S7). Laturile sunt ale contractului clasei (T-D13: PLT
  `Propriu → Extern | Propriu`, INC `Extern | Propriu → Propriu`), verificat
  înaintea declarantului; declarantul refuză doar laturile identice.
- **FCT** (`DeclarantFacturaIntrare`): fiecare linie postează netul pe
  regula ei de contare (imobilizări → 404). Linia de stoc e RECEPȚIA facturii
  (TR-D3), pe regula naturii `Stoc` (112): gestiunea primitoare, lotul născut
  de linie, `+q`; contracantitatea `−q` stă pe postarea de terț, marcată
  `Contrapondere` = terț (N-D4). Fără regulă linia e refuzată cu `REGULA_CONTARE_LIPSA`; debitul
  regulii pe alt cont decât al lotului, cu `CONT_STOC_LIPSA`. Dimensiunile
  regulii se aplică peste analiza liniei, ca la celelalte naturi;
  taxa se DECIDE per document × cotă — culeasă = autoritară cu toleranță,
  nedată = Hamilton per linie, scrisă pe linie la pregătire și postată de
  acolo (109); faptul fiscal e atribut al postării interne
  (`CodTva` tip × sens × rol, perioada declarării, `PartenerFiscal`, `Loc`);
  `Normal` 4426 = 401, `TaxareInversa` 4426 = 4427 (4427 fără `CodTva`: azi
  nu există fapt fiscal colectat pe TI), `Capitalizat` = bază + taxă pe
  ACELAȘI cont de cost (jurnalul rămâne proiecție), `Scutit`/`Neimpozabil`
  doar bază; o singură partidă per cont de terț. NIR-ul conex postează numai
  diferența față de recepția deja reprezentată în cubul FCT (098, 099). Plata
  autogenerată are document și postări proprii.
- **FCL** (`DeclarantFacturaIesire`): FCT în oglindă — per linie venitul pe
  regula de vânzare a Tipului (creditul = internul, cu emitentul ca
  repartitor; debitul 4111 = terțul, cu partidă), taxa colectată per linie
  prin `Fiscal.Impozitul` pe direcția `Colectat` (creditul pe 4427, cu locul
  documentului ca repartitor, `Fiscal.Locul`; debitul pe contrapartidă),
  taxa culeasă autoritară, taxarea inversă pe
  livrare fără postare de taxă. Zero postări de stoc: linia de natură `Stoc` e
  linie de venit, fără lot și fără produs. Partidă pe fiecare cont cu `UrmarestePartide`
  al liniilor (S-D16): FCL cu regularizare de avans (`4111 = 419`) deschide
  DOUĂ partide, iar soldul partidei de creanță e netul ei (debitul de −100 al
  liniei de avans intră pe 4111). Valorile negative (prețuri negative) se
  declară semnate, pe aceeași latură. Refuzuri: laturile prin contractul
  clasei (T-D13), fără linii, cantitate ≤ 0, `PRODUS_ALT_TIP`, regulă lipsă, TVA. (T-D4)
- **RDC** (`DeclarantReturClient`), activat numai pe profilul privat. Linia
  fără lot inversează venitul și TVA-ul, deschizând partida proprie negativă;
  linia cu lot readuce cantitatea și valoarea pe lotul original, fără fapt
  fiscal. NTC poate compensa partida returului cu cea a facturii;
  nominalizarea directă pe factura originală nu există (TR-D9b). Catalogul
  `docs/nucleu/scenarii/RDC.md` acoperă și stocul returnat deja consumat,
  anularea/reoperarea și corecția mixtă peste luna închisă. (T-D8)
- **RLF** (`DeclarantReturFurnizor`), tot numai pe privat. Pe lotul original
  scade cantitatea și valoarea notei fiscale, fără preluarea soldului valoric
  la golire. Partida proprie 401 are rest pozitiv; compensarea cu factura
  folosește NTC. Catalogul `scenarii/RLF.md` probează și reziduul −0,01 pe
  lot gol, reintrarea prin RDC și descărcarea ulterioară la soldul cubului.
  Reziduul nu se elimină prin reevaluare (T-r2, TR-D9b). (T-D6, T-D8)
- **DVI** (`DeclarantDvi`), activat numai pe privat. Taxa este contabilă;
  baza vamală este o pereche debit/credit pe contul deductibil în
  `Carte=Fiscal`, fără unități, cu locul documentului ca repartitor pe ambele
  capete. Numai debitul fiscal poartă cod, rol Bază, `PartenerFiscal`, `Loc`
  și perioadă; creditul echilibrează fără fapt fiscal. Baza 0,01
  rămâne în cub și când taxa este zero. Anularea, stornoul și corecția folosesc
  mecanismele comune, inclusiv refuzul inversării cu o partidă consumată prin
  NTC. Contract: `docs/nucleu/tr-d7b-dvi-baza-fiscala-contract.md`; probe:
  `SC-DVI-01…20`, `SC-X-14`. Conturile se rezolvă din politică, inclusiv ancora
  fiscală; modelul pur și schema nu se schimbă.
- **NTC și ITV** folosesc același `DeclarantNotaContabila` (T-D3).
  Nota păstrează conturile explicite și valoarea semnată; cantitatea este 0.
  Pe un cont cu `UrmarestePartide`, partenerul explicit al liniei nominalizează FIFO
  partidele aceluiași cont și partener, în sensul stingerii și până la rest;
  excedentul deschide partida proprie. Linia poate numi, pe fiecare latură,
  partida pe care o stinge (`PartidaDebitId`, `PartidaCreditId`): latura
  stinge atunci numai acea partidă, până la restul ei, iar ce nu acoperă merge
  pe partida proprie, nu pe FIFO. O partidă numită care nu există la data notei
  pe contul și partenerul laturii e refuzată (`PARTIDA_NUMITA_INVALIDA`).
  Câmpurile sunt ascunse în XAF și lipsesc din `WriteDto`; le scrie conectorul
  de import (115). Soldurile pentru această nominalizare
  se citesc din cub la data înregistrării, ca fapte în operand. Fără partener,
  nu se inventează partidă; pe 3xx fără lot postarea rămâne doar contabilă.
  Împerecherea ulterioară fără partidă proprie nu mai transferă încă o dată
  valoarea deja nominalizată. Sursele cu nominalizări active sunt protejate
  la anulare/storno, inclusiv în intervalul dintre nominalizare și inversarea
  ei ulterioară: un dependent stornat pe 20 nu permite stornarea sursei pe 10.
- **ITV** postează explicit liniile generate din `SolduriService`, fără fapte
  fiscale noi. Corecția legată verifică soldurile la `DataInregistrare`, unde
  stornoul a redeschis sumele; închiderea obișnuită folosește `Data`.
  Validarea și citirea `Stale` folosesc aceeași regulă. Cataloagele și
  validarea independentă: `docs/nucleu/scenarii/NTC.md`, `ITV.md`.
- **IRZ** are declarant propriu (`DeclarantInchidereRezultat`): fiecare linie
  e o mișcare între cheia-sursă și destinație. Latura sursei, marcată pe linie,
  primește repartitorul, materialul și analiza complete; latura destinației
  primește repartitorul scris pe linie și numai coordonatele păstrate
  (`InchidereFapt.Pastrate`). Repartitorul lipsă rămâne lipsă, fără implicitul
  antetului; partidele nu se nominalizează. Catalogul: `docs/nucleu/scenarii/EX.md`.
  (E3-D3)

### Tipul care nu declară

Orice tip operabil declară. Tipul fără declarant (BPR) sau fără politica de
profil pe care declarantul lui o cere (`IDeclarant.PoliticaCeruta`: regulile
de contare pentru DSC, RDC și RLF, politica de TVA pentru DVI, politica
închiderii de TVA pentru ITV — absente pe bugetar, politica închiderii
conturilor pentru IRZ) e refuzat la dry-run și la
operare cu `TIP_FARA_DECLARATIE`, înaintea validării frunzei, cu sau fără
număr cules. Tipul de document nu poartă un comutator de postare în cub:
orice tip operabil postează, prin declarant. (D9-D5, D9-D6)

O regulă de contare pe un tip al cărui declarant nu contează prin reguli
(`IDeclarant.ConteazaPrinReguli` fals: ASM, BTR, PIF, AMO, CAS, NTC, ITV, IRZ, DVI)
sau care nu declară e refuzată la editare, pe ușa gardianului, cu
`REGULA_CONTARE_FARA_CONSUMATOR`: politica nu descrie comportament pe care
motorul nu-l are. (4, D9-A4)

Pe tipurile care contează prin reguli, fiecare linie cere regulă: linia fără
regulă potrivită e refuzată la operare cu `REGULA_CONTARE_LIPSA`; numai
liniile NIR-ului conex preluate din recepția facturii moștenesc capătul ei de
stoc și nu cer regulă. Recepția facturii nu face excepție: regula ei e `FCT/Stoc`, în seed-ul ambelor
profiluri (debit din contul Tipului, credit din contul furnizorului, cu
rezerva 401 / 401.01.00). (112)

### Gardurile declaranților, ca dată sau ca regulă

- Valoarea negativă e admisă în `Operare`: e reprezentarea „în roșu” a liniei
  culese, retur sau discount pe același document. `SEMN_NEGATIV` rămâne doar pe
  `Deschidere`. Lotul nu se naște însă negativ: linia care creează lot cu
  valoare negativă e refuzată cu `LOT_VALOARE_NEGATIVA`; returul sau reducerea
  se fac prin RLF sau prin corecția valorică. (S-D14, 126f)
- Lotul născut de document își primește prețul și data în pregătirea comună
  a validării și a operării, înaintea refuzurilor: dry-run-ul și operarea
  judecă același contract, iar cantitatea nepozitivă e refuzată la amândouă
  (`CANTITATE_NEPOZITIVA`). Validarea nu persistă nimic. (127a)
- `PoliticaTva.TolerantaTaxa` e opțională: `null` înseamnă că taxa culeasă
  rămâne autoritară, fără validare — valoarea de seed a profilului privat. O
  valoare dată refuză `TVA_IN_AFARA_TOLERANTEI` peste `toleranță × liniile
  cotei`. (S-D15)
- O linie cu două conturi cu `UrmarestePartide` numește partidă pe AMBELE capete,
  fiecare pe contul lui. (S-D16)
- Tipul de document nu declară latura contului propriu: declarantul de
  trezorerie o alege structural, piciorul propriu e cel al cărui cont vine de
  la repartitorul intern, iar la virament capătul de tranzit (581) rămâne fără
  repartitor, deci soldul lui se închide pe o singură cheie.
  (S-D7, D9-A10, D9-D6; C111 S7)
- Linia fără regulă de contare e refuzată (`REGULA_CONTARE_LIPSA`), fiindcă
  valoarea ei ar dispărea din contare. (B-r10)

## Laturile documentului ca structură

Modelul e cel din legacy (`GEST_DEFA_DOCUM`: predator → primitor pe
intern/extern per tip), pus în cod, nu în tabelă:

- **`Parte`** (`Declaratii/Laturi.cs`) e partea repartitorului față de
  patrimoniu, derivată o singură dată din `FelRepartitor` (= `ClrType`, 89b)
  prin `Laturi.ParteA`: `Extern` (Partener, Angajat), `Intern` (Gestiune,
  UnitateInterna), `Propriu` (ContPropriu). `RepartitorFapt.Parte` o expune.
  Partea nu are coloană: pe `Capat` și în nucleu ajunge felul
  (`RepartitorFapt.Coordonata` = `Repartitor(Id, Fel)`), din care nucleul
  citește `EsteIntern` și `EsteExtern`. (T-D13)
- **Contractul** e METODA polimorfă `Document.Laturi()` (abstractă: fiecare
  tip o declară, compilatorul o cere) → `ContractLaturi(Latura Predator,
  Latura Primitor)`, unde `Latura` = părțile permise + calitatea cerută
  (`LocConsum` pe primitorul BCS, `Comisie` pe al LDI) + felul exact DOAR
  unde structura îl cere: laturile care poartă stoc sunt `Gestiune`, fiindcă
  `Lot.Gestiune` e tipat `Gestiune`. Fără tabelă de politică, fără `switch`.
  (T-D13)
- **Un singur loc de verificare**, `Laturi.Verifica`, pe ambele uși:
  `Contractare` o cheamă ÎNAINTEA declarantului (refuzul laturii e singurul
  refuz al declarației), iar `Document.ValideazaOperare` o cheamă pe faptele
  citite din bază (`Fapte.Laturile`), cu aceeași linie de mesaj
  (`Contractare.Mesaj`: `COD: text`). Codurile sunt `PREDATOR_NEPOTRIVIT` /
  `PRIMITOR_NEPOTRIVIT`; declaranții și clasele nu au verificări proprii de
  latură. (T-D13)
- **Contractele declarate** (predator → primitor): NIR, FCT, RDC
  `Extern → Gestiune`; FCL, RLF `Gestiune/Intern → Extern` (FCL emitentul e
  `Intern`, RLF `Gestiune`); BTR, ASM `Gestiune → Gestiune`; BCS `Gestiune →
  Intern + LocConsum`; LDI `Gestiune → Intern + Comisie`; DSC `Gestiune →
  Extern | Intern`; NTC, ITV `Intern | Propriu` pe ambele; DEC, DVI
  `Extern → Intern`; PLT `Propriu → Extern | Propriu`; INC `Extern | Propriu
  → Propriu`; PIF, CAS, AMO, BPR `Intern → Intern` (AMO cere în plus
  identitatea, în clasa ei). Granularitatea e a părții: pe laturile fără stoc
  `UnitateInterna` și `Gestiune` sunt interschimbabile (DVI, PIF, CAS, DEC),
  `Partener` și `Angajat` la fel pe cele externe (FCT, NIR, FCL, DEC), iar
  `Angajat` nu e admis pe laturile interne (BCS/LDI cu calitate, NTC). (T-D13)
- **Terțul pe capătul extern**: DSC pune `Repartitor` = primitorul extern pe
  capătul 607 (T-D13 (g)).
- **UI înghețat** (090 (m)): filtrarea lookup-urilor Predator/Primitor pe
  partea permisă e a dezghețului (`lista-react.md`).

## Cubul persistat

Motorul scrie `Tranzactie`/`Postare` în tranzacția comenzii, pentru orice tip
care declară; e singurul scriitor al faptelor. Contractul formei:
`docs/nucleu/tr-d7a-strangler-contract.md` (S-D1…S-D16); tăierea registrelor:
`docs/nucleu/tr-d9-taierea-contract.md` (D9-D2).

Citirile — sold, proiecții, fișe, SAF-T, D394, D406 — sunt pe cub. Notele pe
conturi de stoc fără lot (TR-r2) rămân delimitate prin contractele tipurilor.
Citirile contabile cer `Carte=Contabil`, jurnalul TVA citește ambele cărți
și numai faptele cu cod, adunând valorile semnate. SC-DVI-16 detectează lipsa
filtrului de carte inclusiv pe soldul cont × partener fiscal.

### Entitățile și forma lor fizică

`Module/Cub/` ține cele două entități, POCO EF în afara `EntitateConta`:
cubul e append-only, deci fără `OptimisticLockField` și fără filtru global de
interogare. Proprietățile sunt `virtual` și colecția e
`ObservableCollection`, cât timp hosturile folosesc proxy-uri de change
tracking. Niciuna nu intră în metadata clientului. (S-D1) În XAF apar numai
ca evidență read-only (lista `Postare` de sub „Registre”). Pe orice ușă
securizată gardianul le refuză crearea, modificarea și ștergerea, inclusiv
administratorului: le scrie numai motorul, pe ușa de sistem. Refuzurile de
ștergere citesc cubul: lotul propriu al unei linii șterse rămâne cât e
unitatea unei postări, tipul de TVA purtat de o postare nu se șterge, fișa
care e unitatea unei postări nu se șterge. (D9-D9, D9-A6)

| Entitate | Coloane |
|---|---|
| `Tranzactie` | `ID`, `DocumentId`, `Fel` (`Operare`, `Storno`, `Transfer`, `Deschidere`), `Data`, `ScrisLa` (UTC) |
| `Postare` | `Spatiu`, `TranzactieId`, `DocumentId`, `LinieId`, `Data`, `Cont`, `Sens`, `Repartitor`, `Contrapondere`, `Produs`, `Unitate`, `UnitateDeschisa`, `FelUnitate`, codul de TVA în trei coloane (`TipTvaId`, `SensTva`, `RolTva`), `PartenerFiscal`, `Loc`, `PerioadaDeclarare`, `Valuta`, `Carte`, cele șase dimensiuni, `Atribuit`, `Pereche`, `Cantitate`, `ValoareValuta`, `Valoare` |

Enumurile nucleului se mapează pe `smallint` cu valorile lor numerice; scara
măsurilor rămâne a gardianului `Scara` (bani 18,2, cantități 18,3).

Tabela `Postare` e partiționată LIST pe `Spatiu`, cu exact două partiții —
`Postare_Contabil` (1) și `Postare_Stoc` (2) — și cheie primară `(Spatiu, ID)`.
Cheia compusă și partiționarea trăiesc DOAR în bază: modelul EF declară cheia
`ID`, fiindcă XAF EF Core nu suportă chei compuse. FK-urile stau pe partiții,
nu pe părinte: `Tranzactie`, `Documente`, `Conturi`, `Produse` și
`Repartitori` — prin `Repartitor`, `PartenerFiscal` și `Loc`, fiecare cu
cheia ei — pe amândouă, `Loturi` (unitate) doar pe Stoc. Nu există FK pe
`Unitate` pe partiția Contabil, fiindcă id-ul partidei e hash determinist, nu
id de rând. (S-D2, C111-D2)

`Repartitor`, `PartenerFiscal` și `Loc` sunt `uuid` nulabile; `Contrapondere`
e `smallint` nulabil, cu valorile enumului (1 terț, 2 consum, 3 inventar,
4 transformare). Felul repartitorului nu e coloană (111a): e discriminatorul
rândului din `Repartitori`. Schema păzește forma prin constrângeri:

| Constrângerea | Ce cere |
|---|---|
| `CK_Postare_FelUnitate` | unitatea, felul și data deschiderii, toate sau niciuna; lotul numai pe Stoc, partida și fișa numai pe Contabil |
| `CK_Postare_Referinte` | suportul și originea inversei, fiecare cu spațiul ei |
| `CK_Postare_Contrapondere` | `Contrapondere` nul sau între 1 și 4 |
| `CK_Postare_BlocFiscal` | fără fapt fiscal: `PartenerFiscal` nul, iar `Loc` nul sau pe o postare cu partidă; cu fapt fiscal: `Loc` nenul |
| `CK_Postare_FiscalComplet` | calificarea și reperul fiscal, complete când există tip de TVA |

Cei 14 indecși ai tabelei sunt scriși în SQL. Pe axa analitică stau
`IX_Postare_Contabil_Repartitor_Cont_Data` (parțial, pe `Repartitor` nenul) și
`IX_Postare_Stoc_Produs_Data`, care include `Repartitor` și `Unitate`;
`IX_Postare_Fisa_Cont` e parțial pe postările cu fișă. `PartenerFiscal` și
`Loc` n-au index.

`UnitateDeschisa` e data deschiderii unității, decisă de declarant la postare:
cu ea, cititorul rândurilor reconstruiește unitatea fără niciun lookup.
Reconstrucția merge pe FEL — `Partida` ia repartitorul rândului ca titular și
n-are produs, `Lot` ia produsul și n-are titular — iar scrierea refuză un rând
din care unitatea nu s-ar reconstrui exact. (`Module/Cub/Randuri.cs`)

Felul repartitorului se recitește, nu se deduce (C111-D3). `Randuri.Citeste`
primește repartitorii rândurilor citiți o singură dată pe lot
(`Randuri.Repartitori`, prin `Motor/Fapte.Repartitori`), nu câte o interogare
pe postare. Un repartitor fără rând sau cu fel necunoscut e refuz explicit,
`REPARTITOR_FARA_FEL`. Pe aceeași cale recitesc stornoul, împerecherea și
desfacerea transferului, stingerea deschiderii, poziția fără fișă, retragerea
de lot și recepția istorică a NIR-ului; capetele imobilizărilor construite
din proiecția cubului (`Motor/ImobilizariFapte`) își iau felul din același
cititor.

Orice migrație care atinge `Postare` sau `Tranzactie` se scrie în SQL, nu se
lasă generată: snapshot-ul EF poartă cheia simplă și FK-ul pe părinte, baza are
cheia compusă și FK-urile per partiție. (S-r4)

### Materializarea, stornoul, anularea

`Module/Cub/Materializare.cs` rulează din `MotorOperare`, deci pe toate ușile
(UI, WebApi, Import1C):

- **Operarea** — după scrierile pe document, înainte de commit: contractul declarantului devine cel mult o tranzacție `Operare` și
  cel mult una `Transfer` (cel puțin una din ele; felul mixt de mai jos, T-D2),
  datate cu `DataInregistrare`, o postare per postare a contractului. Refuzul
  declarației E refuzul operației: o singură eroare cu toate refuzurile, iar
  tranzacția de comandă se anulează integral — nimic în cub, nimic pe document.
  Dry-run-ul arată aceleași refuzuri, în forma `EroriDto` de azi. (S-D4)
- **Stornoul** — o singură tranzacție de fel `Storno`: inversul exact al
  postărilor `Operare` ale documentului, al transferului lui de STOC (unități
  de fel `Lot`; transferurile pe partidă sunt ale împerecherii și le inversează
  rândul ei invers, S-D13) și al celor `Atribuit` spre ele, datat
  la data stornării, cu `PerioadaDeclarare` re-ștampilată la perioada
  stornării. La corecția cu motivul `EroareMateriala`, postările fiscale ale
  inversei se nasc cu atribuirea corecției — perioada originalului sau, dacă e
  depusă, regularizarea — și cu marcajul de inversă tehnică; nicio postare
  existentă nu se rescrie. (S-D5, D9-A5) Ordinalul perechii se păstrează pe
  prima sursă (după identificatorul tranzacției) și se decalează pe celelalte
  cu maximul surselor dinaintea lor, ca perechile să rămână distincte în
  unica tranzacție de storno (D9-A2, `Storno.Inverseaza` pe `(Sursa, Postare)`).
- **Anularea operării** șterge fizic tranzacțiile `Operare` și `Transfer` ale
  documentului și postările lor; e gardată
  de „fără împerecheri", deci orice `Transfer` al documentului e al lui. (S-D5, T-D2)

### Felul mixt: `Transfer` pe linia care nu schimbă contul

Declarația are `Miscari` (debit ≠ credit, devin `Operare`) și
`Mutari` (același cont, aceeași latură, −/+ între două capete, devin
`Transfer`). O linie de stoc al cărei cont nu se schimbă între laturi e o
mutare; una care schimbă contul e o mișcare. Un document are astfel cel mult o
`Operare` și cel mult un `Transfer`, cel puțin una — amendament de literă al
lui 090 (a), T-r1. Fiecare capăt real al unui `Transfer` de stoc poartă
gestiunea ca `Repartitor` și lotul ca `Unitate`; contraponderea ASM de mai jos
nu are nici repartitor, nici unitate.
Rapoartele pe cont exclud Transfer (Σ per (cont, latură) = 0),
cele pe gestiune și pe lot îl includ. (T-D2)

- **BTR** (`DeclarantNotaTransfer`): lotul își schimbă gestiunea și contul lui
  rămâne, deci BTR produce NUMAI `Transfer` (T-D2.1). Contul e al lotului
  (`TipMaterial.ContImplicitId`), fără regulă de contare; refuzuri:
  `GESTIUNI_IDENTICE`, `CONT_STOC_LIPSA`, plus cele de gestiune, lot și
  cantitate. Valoarea = `Evaluare.Iesire` pe raportul curent al lotului în
  gestiunea predatoare, în secvența liniilor.
- **Diferența declarată pe BTR**: cubul evaluează pe raportul curent, nu cu
  prețul înghețat la nașterea lotului (`round(cantitate × Lot.PretUnitar)`)
  (090 (j), N-r3). Diferența apare numai pe ieșirile PARȚIALE (golirile sunt
  egale) și e repartizare între gestiuni, nu conservare; se vede la citirile
  pe cub per gestiune × lot. Nu se normalizează. (T-D2.2)
- Ramura `Operare` a formei mixte e probată prin proprietăți în nucleu; pe
  scenă o probează ASM.

### Ordinea liniilor

`DocumentDetaliu.Pozitie` e ordinea de culegere a liniei. Se atribuie o
singură dată, la salvarea unei linii noi, în `SaveChanges`-ul contextului — un
singur loc pentru UI, WebApi, Import1C și conexul clonat — ca max-ul liniilor
documentului plus unu. Operandul declarantului (`Fapte.Operand`) și coaja
comenzii (`MotorOperare.Liniile`: TVA culeasă, clonarea conexului) citesc
liniile `OrderBy(Pozitie)`, apoi `ThenBy(ID)`, deci conexul clonat primește
liniile sursei în aceeași ordine. (S-D6)

### Scrierea serială per bază

- **O singură comandă scrie la un moment dat.** Tranzacția unei comenzi se
  deschide prin `TranzactieComanda.Incepe`, care ia ca primă instrucțiune
  blocajul scrierii — un `pg_advisory_xact_lock` cu cheie constantă
  (`TranzactieComanda.BlocajScriere`), ținut până la commit sau rollback.
  Blocajul protejează citirea care decide valoarea, nu doar scrierea: soldul
  lotului, restul partidei și starea perioadei se citesc
  după el, pe starea lăsată de comanda precedentă. (X-D6)
- **Cine îl ia**: operarea, anularea, stornoul și corecția; împerecherea,
  desfacerea și ștergerea ei; închiderea, redeschiderea și reconstrucția
  perioadei; generarea și regenerarea AMO și ITV; confirmarea depunerii;
  deschiderea și stingerea ei, care rulează în tranzacția apelantului și iau
  blocajul la intrare (`Materializare.CereScriere`); salvarea care adaugă
  detalii fără poziție. Lista e nominală și probată pe sursă. (X-D6)
- **Cine nu îl ia**: citirile, dry-run-ul (`Valideaza`, `Refuzuri`) și
  salvarea unui draft fără detalii noi. (X-D6)
- **Ordinea blocajelor** e „scrierea, apoi restul”: perioada (F27-D1),
  documentele stingerii, suportul și fișele IMO (97001), sursa recepției
  (97002) și depunerile rămân și se iau după blocajul scrierii, deci nu pot
  forma cicluri între comenzi. (X-D6)
- **Așteptarea** durează cât comanda din față. Dacă depășește timpul de
  comandă al conexiunii, a doua comandă primește refuzul `SCRIERE_OCUPATA`
  (422), fără nimic scris, și se poate relua. (X-D6)
- **`Pozitie` pe detalii** (S-r9): salvarea care adaugă detalii fără poziție
  deschide tranzacția ei, ia blocajul scrierii și abia apoi citește maximul
  pe document; două sesiuni care adaugă pe același document primesc poziții
  distincte, în ordinea intrării.
- **Rezultatele seriale probate pe două conexiuni** (SC-X-15…SC-X-23, ambele
  profiluri): două consumuri peste lot — un succes și `STOC_INSUFICIENT`;
  două consumuri care încap pe 3/1,00 — 0,33 și 0,34, cu soldurile citite
  3/1,00 și 2/0,67 în explicațiile persistate; consum contra stornoul
  recepției — un succes și un refuz, în ambele ordini; două stingeri peste
  restul aceleiași partide — un succes și un refuz; aceeași pereche — după
  sume; operare, apoi închidere — ambele trec și snapshot-ul poartă
  operarea; închidere, apoi operare — operarea e refuzată. (X-D6)

### Deschiderea generică

Deschiderea generică este disponibilă prin `Cub.Materializare.Deschide`
în tranzacția explicită a apelantului, fără commit propriu. Soldurile de
control includ ancora aleasă de apelant; loturile și partidele înlocuiesc
rândul bloc pe Carte/Cont/Sens. Totalurile diferite sunt refuzate înaintea
creării entităților de cub. Partidele cer cont cu UrmarestePartide, partener și
referință stabilă; nu există document fictiv. Indexul unic filtrat permite
o singură Deschidere în bază. Stingerea unei partide inițiale verifică
restul întregii unități, contul, partenerul și perioada sub blocare comună
cu storno/anulare. Transferul contribuie la restul de domeniu al plății,
la lista de documente cu rest și la snapshot-ul partidelor, cu aceeași tăiere pe dată;
anularea este refuzată, iar stornoul nu poate preceda stingerea și reface
partida fără a rescrie deschiderea. Detalierea stocului derivă din politică;
loturile deja folosite și deschiderea datată după istorie sunt refuzate.
Intrarea păstrează analiza/valuta și verifică dimensiunile obligatorii;
limitele intrării și ale stingerii sunt în DES-B4. Deschiderea generică
scrie cubul, iar loturile ei se consumă ca oricare altele. (094, DES-B1…B4)

## Limitele implementării

- Felurile de repartitor le declară numai seed-ul bugetar, pe conturile cu
  defalcarea R; pentru o parte din ele felul vine din natura contului, nu din
  date. Pe 461.01.09 angajatul nu e admis (C111-Q21). Planul privat nu declară
  nimic, iar contul nedeclarat nu judecă repartitorul: lumea e deschisă
  (C111-r2). Terțul stă pe capătul de terț numai când contul declară feluri de
  repartitor sau urmărește partide; pe un cont nedeclarat și fără partide (446
  privat) lipsește. Pe contul care admite lipsa (437.01.00, 437.02.00,
  803.00.01), terțul cunoscut de document stă pe capăt, iar lipsa e admisă
  numai când documentul n-are terț pentru el (C111-F-R1). Pe conturile de
  cheltuieli și venituri terțul nu e axă de sold: apare numai ca
  `PartenerFiscal`, pe postarea cu fapt fiscal. Pe trezorerie, când regula de
  contare numește ambele conturi, capătul propriu n-are repartitor; dacă
  contul lui cere repartitor, documentul e refuzat (SC-PLT-09), nu reparat cu
  terțul. (C111-D4)
- Cerința de material citește numai produsul lotului, nu și materialul fix al
  unei reguli de contare. BTR, ASM și PIF nu sunt păzite de ea. (D9-r1)
- Blocajul scrierii îl ia `TranzactieComanda`. O unealtă care își deschide
  singură tranzacția (`Database.BeginTransaction`) și cheamă motorul nu îl
  are; proba pe sursă refuză asta în `Module`, WebApi și Blazor, nu și în
  uneltele standalone. Deschiderea și stingerea ei îl iau oricum, la intrare.
  (X-D6)
- O postare existentă nu se rescrie: garda e la salvare
  (`POSTARE_MODIFICATA`), pe starea `Modified` a entității, deci nu vede o
  actualizare făcută direct în SQL. Ștergerea postărilor la anulare rămâne
  permisă motorului. (D9-A5)
- Stornoul cu mai multe surse decalează ordinalele de pereche ale surselor
  după prima, deci ordinalul stornoului nu e literal al originalului. (D9-A2)
- Regula listării postărilor pe lot și pe partidă (N-r8) e fixată pe
  intrările comune și probată pe ele. Singura listare pe lot din producție e
  mișcarea de stoc SAF-T, care poartă eticheta felului; o fișă a lotului sau
  a partidei se construiește peste `Loturi.Postari`, respectiv
  `Partide.Postari`, fără filtru pe fel.
- Cubul primește numai ce se adună. Faptele neaditive („valoarea valabilă la
  data d”) nu intră în cub nici ca postare cu măsuri zero, nici ca coordonată,
  nici ca stare scrisă de motor pe nomenclator; destinația lor e faptul
  datat, supapă cu nume construită la primul consumator. Cele trei forme
  existente (postările zero ale revizuirii PIF, locul și starea fișei) sunt
  tolerate ca moștenire și se mută atunci (131-r2). Registre de acumulare
  separate nu se adaugă. (131)

## Locurile regulilor în cod

- [Document și contracte](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/BusinessObjects/Documente/Document.cs)
- [Motorul operării](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Motor/MotorOperare.cs)
- [Culegerea draftului (L3)](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Culegere/)
- [Nucleul pur: conservarea](../../nou/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu/Conservare.cs)
- [Nucleul pur: motorul pe declarație](../../nou/Atlas.Conta.Nucleu/Atlas.Conta.Nucleu/Motor/Motor.cs)
- [Declarația fluxului: operandul, driverul, declaranții BCS/PLT/FCT](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Declaratii/)
- [Adaptorul operandului închis (`Fapte.Operand`)](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Motor/Fapte.cs)
- [Probele declaranților în ModelCheck](../../nou/tools/ModelCheck/Nucleu/)
- [Cubul persistat: entitățile, materializarea, cititorul rândurilor, transferurile](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Cub/)
- [Serviciul de împerechere](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Motor/ImperechereService.cs)
- [Documentele de trezorerie](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/BusinessObjects/Documente/Trezorerie.cs)
- [Documentele imobilizărilor](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/BusinessObjects/Documente/Imobilizari.cs)
- [Serviciul de amortizare](../../nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Motor/AmortizareService.cs)

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (36; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): 1, 2, 3 (amendată de 89), 4, 6, 10, 15, 18, 25, 26 (amendată de 112), 28, 29, 32, 33, 42 (amendată de 104, 106), 49, 51, 52, 54, 64 (amendată de 84), 89, 90 (amendată de 91, 92, 93, 95, 96, 110, 119), 94, 98, 99, 104 (amendată de 106), 109, 111, 112, 114 (amendată de 116), 115, 124, 126, 127, 128, 131.
<!-- /gen:decizii -->
