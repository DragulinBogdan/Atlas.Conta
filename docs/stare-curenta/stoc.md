# Stoc

**Actualizat: 2026-10-16.** [Index](README.md)

Loturile și evaluarea FIFO, citirea stocului, recepția conexă și diferențele ei, transformarea, descărcarea de gestiune și corecția valorii lotului; plus invarianții cubului și citirile contabile ale lui.

## Stoc și evaluare

- Doar `ClasaProdus.Natura = Stoc` intră în regulile de stoc. Natura și tipul
  material sunt date distincte de identificarea produsului. (23b)
- Cheia soldului este `(Lot, Cont, Produs, Gestiune)`. (25d, 27b, TR-D8)
- Soldul cantitativ intermediar trebuie să fie nenegativ la orice dată
  afectată, inclusiv pentru documente introduse retroactiv. (25d)
- Lotul se naște la culegerea liniei de intrare și se finalizează la operare.
  `LoturiCulegereService` este calea comună XAF/Apply. Un lot finalizat nu
  este șters de curățenia culegerii. (25c, 56, 53f)
- `ILinieCareNasteLot` declară nașterea lotului; `ProdusId` pe o linie care
  doar selectează/pin-uiește stoc nu înseamnă naștere de lot. (62)
- Evaluarea este identificare specifică pe lot. Alegerea automată este FIFO
  în produs × gestiune; pin-ul manual are prioritate. Un pin nu completează
  automat lipsa sa cu alte loturi. (13, 37d, 38b)
- Ieșirea are una din două surse de valoare, numită de declarant. Evaluată
  din sold (BCS, BTR, DSC, minusul LDI, consumul ASM): contractul o evaluează
  pe soldul cubului, în secvența liniilor, iar ultima bucată ia restul; linia
  primește la operare valoarea decisă, cu semnul cantității ei, deci linia și
  postarea sunt egale. Pe draft și la dry-run linia poartă estimarea
  `cantitate × preț de intrare`. Declarată la prețul de intrare (RLF, costul
  RDC, linia NIR-ului conex pe lot străin): `cantitate × Lot.PretUnitar`; nu
  absoarbe reziduul lotului. `Lot.PretUnitar` e prețul de intrare, scris o
  dată, la operarea liniei care naște lotul. (75a, 090 j, D9-D3)
- O operație retroactivă nu recalculează valorile unor ieșiri deja operate. (75a)
- Loturile inițiale cu același produs, gestiune, cont și dată se departajează în FIFO
  pe identificatorul lotului, dat de cel care le creează. (116-r2)

### Citirea stocului

Cititorul de lot folosește cheia lot/cont/produs/gestiune, cu Transfer și
inversele lui; gestiunea lotului e repartitorul postării cu lot.
BCS/BTR/DSC/LDI/ASM evaluează ieșirile din soldul cubului. FIFO și pinurile
DSC folosesc data înregistrării și coordonatele complete. Raportul de stoc
arată contul, gestiunea și costul unitar din sold; etichetele lipsă nu
elimină sumele. Postările marcate cu contrapondere n-au lot, deci nu intră în
disponibilul real. (098, 099, TR-D8)

Snapshot-ul de stoc (`SoldPerioadaStoc`) se scrie numai din cub pe aceeași
cheie, cu gestiunea lotului (`GestiuneId`) și data deschiderii; citirile
nesecurizate folosesc snapshot + fereastră, inclusiv evaluarea ieșirii cu
graniță strict anterioară documentului. Reconstrucția detectează și data
alterată, pe lângă diferențele de chei și măsuri. Soldurile 0/0 se omit, cele
0/valoare nenulă rămân. (098, 099, TR-D8)

Soldul cerut pe o listă de loturi (`Loturi.Cumulate(…, loturi)`) restrânge
snapshot-ul și postările înaintea grupării. Pe ramura postărilor fereastra e
pe `"Data" + 0`, ca planul generic al unei comenzi pregătite să citească pe
lot și nu pe toată fereastra de dată. Operandul motorului și gardul de sold
intermediar citesc așa. (125)

## Recepția conexă și diferențele de recepție

[Contractul NIR](../nucleu/tr-d8-nir-delta-contract.md) fixează sursa ca
tranzacție și linii istorice, un singur cumul activ sub blocare comună și
politica `tip × cauză × clasă`. `PoliticaDiferenta` este configurabilă,
cu proveniență de seed; nomenclatorul privat furnizează conturile 32x.
Bugetarul folosește maparea aprobată 473.01.09/408.00.00/461.01.09,
428.01.02 pentru angajați și 35x pentru PeDrum. Lipsa politicii sau a
analizelor obligatorii este refuz atomic. Stocul istoric, datoria și TVA-ul
facturii nu se rescriu; inversarea utilizează delta existentă. (098, 099)

`Citiri/Receptii` este intrarea comună pentru proveniența recepțiilor conexe.
Pentru NIR acoperit, cubul păstrează recepția pe factură și gardul ei de stoc:
după stornarea sau anularea NIR-ului acoperit cu consum, operațiile următoare
pe lot se judecă numai pe soldul cubului (098-r3, SC-X-27). Refuzul retragerii
este verificat înaintea modificării tracker-ului, iar cel al operării
înaintea numerotării. (098, 099)

`SursaReceptieiId` este proveniența unică, scrisă la generarea conexului;
corecția o păstrează independent de `Autogenerat`. Recepția-sursă se citește
o singură dată pe comandă, sub blocarea sursei la operare. Validarea
analizelor curente privește capătul diferenței; capătul stocului păstrează
analiza istorică. Imputatul fără cauză Imputabila sau fără deltă se golește. (098, 099)

Factura de avans 4091 privat / 409.01.01 bugetar nu produce recepție.
Seed-ul bugetar clasifică 409.01.01 fără stoc (clasa S); postările istorice
nu se reclasifică prin această corecție de seed. (099d)


## Transformarea n→m

`Declaratie.Transformari` conține linii cu rol consum/produs, capăt real,
cantitate, valoare și cauză. Primitiva pură adaugă o postare marcată
`Contrapondere` = transformare: același cont/produs/analiză/cauză, cantitate
opusă, valoare zero, fără repartitor, unitate sau fapt fiscal
(`CONTRAPONDERE_TRANSFORMARE_INVALIDA`). Conservările sunt neschimbate.
Stornoul selectează tranzacția Transfer de stoc întreagă, inclusiv aceste
postări din partiția Contabil, și verifică conservarea înaintea scrierii.
(ASM-B2…B7)

Declarantul citește rolul și prețul prin `ILinieCuTransformare`. P = R se
verifică și pe operandul închis, fără pregătirea valorilor entității.
Grupurile sunt clasificate o singură dată după P/R; contribuțiile Δ se
acumulează pe produsul țintă înaintea gardului de pozitivitate. Prețul cules,
valoarea entității și prețul lotului rămân ale registrelor. Stornoul inversează
ajustarea istorică exact; corecția calculează din nou. Contractul detaliat:
[transformarea ASM](../nucleu/tr-d7b-asm-transformare-contract.md). (ASM-B2…B7)

`Citiri/Transformare` oferă un singur predicat pentru contraponderea
transformării (`Contrapondere` = transformare), utilizat și de
probe/diagnostic. (ASM-B2…B7)

## Factura de ieșire și descărcarea de gestiune

Declarantul FCL e în [motor § Declaranții pe tip](motor.md#declaranții-pe-tip).

- **DSC** (`DeclarantDescarcareGestiune`): o mișcare per linie — lotul iese
  din gestiunea predatoare pe contul de stoc al regulii (`6xx = 3xx` per
  Tip), evaluat cu `Evaluare.Iesire` pe raportul curent în secvența liniilor
  (ca BCS), iar costul intră cu `+q` pe postarea marcată cu contrapondere,
  fără unitate (090 (g)). Cu primitor extern marcajul e „terț”, iar
  `Repartitor` e primitorul (T-D13 (g); fără partidă — 607 n-are rol de terț).
  Cu primitor intern marcajul e „consum”, iar postarea n-are repartitor
  (C111 S8). Aceeași formă cu sau fără factura-sursă. Lotul e ales de
  `DescarcareService` (pin-uri, apoi FIFO), declarantul nu realocă. (T-D4)
- **Transferul INC → FCL** e activ prin `Materializare.Imperecheaza`:
  referința e a stingătorului (4111), plafonul e restul FCL pe 4111; partida
  419 nu primește transfer. (T-D4)
- **Diferența declarată pe DSC** (aceeași clasă ca T-D2.2, N-r3) privește
  numai ieșirile PARȚIALE; golirile sunt egale. Nu se normalizează. (T-D4.2, T-r7)

## Corecția valorii lotului

Factura de intrare poate purta linii de corecție a valorii unui lot existent
(`FacturaIntrareDetaliu.CorectieValoareLot`): cantitate zero, valoare
semnată culeasă, lotul corectat pe linie. Linia corectată este linia de
intrare a lotului. Un document cu o linie de corecție are numai linii de
corecție, fără TVA; nu naște lot și nu generează NIR. (119, CV-D1)

- Culegerea (L3, `NormalizariTip`) golește pe linia de corecție produsul,
  cantitatea, prețul, valoarea TVA, avansul și atributele de lot, nu aplică
  implicitul de TVA (linia nu e fiscală) și ia tipul din produsul lotului.
  Tipul de TVA ales rămâne pe draft, iar operarea refuză corecția taxabilă cu
  `CV_STRUCTURA`; taxa nu se omite tacit. Comutarea pe corecție pornește
  valoarea de la zero și golește tipul de TVA adus de produs; comutarea înapoi
  rupe lotul ales și recalculează valoarea. Lotul propriu nefinalizat al unei linii
  obișnuite comutate dispare la salvare. Ecranul XAF și `FacturaIntrareApply`
  dau aceeași linie (SC-CVL-25). (104c, CV-D1)
- Valoarea liniei se împarte pe stocul rămas și pe consumurile active ale
  lotului, după cantitate: Hamilton pe poziții, apoi pe postările fiecărei
  poziții. Fiecare cotă nenulă e o pereche `Operare` cu debitul pe poziția
  beneficiarului, cantitate zero, atribuită postării beneficiare, și creditul
  pe partida proprie a corecției la furnizor. Bonul operat și prețul de
  intrare al lotului rămân neschimbate; ieșirile următoare se evaluează din
  soldul ajustat. (CV-D2, CV-D3)
- Lotul corectabil se judecă pe forma faptelor din cub: o recepție activă de
  la furnizorul corecției, consumuri din poziția de stoc și inversele lor.
  Orice altă mișcare pe lot refuză corecția: transfer, descărcare, retur la
  furnizor sau de la client, asamblare, inventar, NIR cu diferențe. (CV-D10)
- Pe un lot cu alocare activă, returul ia prețul ajustat: q × prețul
  lotului plus cota q / cantitatea recepționată din corecțiile încă active.
  La returul la furnizor linia rămâne baza fiscală, cu TVA-ul ei. Cota iese
  ca a doua mișcare, fără cantitate și fără TVA, în `ValoareDeclarata.Corectie`.
  La returul de la client, linia de cost ia direct prețul ajustat. Cât un
  retur e activ, corecția nu se stornează și nu se retrage (`CV_DEPENDENTI`).
  NIR-ul conex re-operat cu diferență (`SursaValoareDeclarata` = `Receptie`)
  se refuză cu `CV_DEPENDENTI` până la stornarea corecției. Celelalte ieșiri
  se evaluează din soldul ajustat. Lotul cu stoc rămas zero și valoare pe
  poziția de stoc nu se corectează. (CV-D10)
- Linia de corecție are tipul produsului lotului (`CV_STRUCTURA`), ca regula
  de contare să fie a lotului. Retragerea unei corecții e refuzată
  (`CV_DEPENDENTI`) cât lotul are o corecție scrisă după ea, care i-a citit
  soldul ajustat; stornarea rămâne permisă. (CV-D10)
- Stornarea bonului scrie, în aceeași comandă, inversa lui și câte o
  tranzacție `Operare` pe fiecare corecție cu alocare activă pe el: perechea
  de compensare mută alocarea de pe consum pe stoc. Bonul stornat se închide
  la zero pe fiecare cont; corecția rămâne operată și poartă integral valoarea
  ei. Stornarea corecției inversează toate postările ei `Operare`, alocări și
  compensări. (119)
- La stornare, saltul pe `Atribuit` nu urmează postările pe lot; pe partide
  rămâne cel din 090 (i). (119 c)
- Fiecare `Operare` a corecției are explicație proprie: `AlocareValorica` pe
  fiecare beneficiar cu pondere, inclusiv cel cu cota zero, respectiv
  `CompensareValorica` pe fiecare pereche de compensare. Invariantul de audit
  (`CorectieValorica.Auditeaza`, în `INV-CUB`) reface repartizarea din
  ponderi și verifică postările și compensările față de decizii: ponderea
  unui consum e cantitatea postării lui, compensarea are cauza rădăcinii.
  (CV-D8)
- `GET api/proiectii/explicatii/{tranzactie}` le întoarce pe linia explicată:
  `Alocari` (unitatea, postarea beneficiară, ponderea, cota și, pe stoc,
  soldul citit la data corecției) și `Compensari` (rădăcina, beneficiarul
  stornat, poziția sursă și cea de destinație, valoarea mutată). Referințele
  spre postările altor documente nu lărgesc accesul: ușa cere deja citirea
  completă a postărilor și a documentelor (`EXPLICATIE_ACCES_INCOMPLET`).
  La storno, explicațiile ambelor `Operare` ale corecției vin ca origini.
  Ieșirea returului la furnizor pe un lot corectat poartă în `Iesiri` și
  `Corectie`, cota scoasă fără cantitate; auditul explicațiilor cere ca
  postările fără cantitate ale liniei pe lot să fie exact ea
  (`CITIRE_EXPLICATIE_IESIRE`). (CV-D8, CV-D10)
- Refuzuri: `CV_STRUCTURA`, `CV_LOT_NEELIGIBIL`, `CV_VALOARE_INSUFICIENTA`,
  `CV_CRONOLOGIE`, `CV_DEPENDENTI`, `CV_AN_INCHIS`, `CV_PROVENIENTA`, toate în
  `CoduriRefuz`. Nucleul emite motivul (`MotivCorectieValorica`), iar
  declarantul și materializarea îl traduc prin `CoduriRefuz.Refuz`. Regimul
  pe stare citește aceleași garduri fără a executa comanda. (CV-D7, CV-D10)
- `CV_AN_INCHIS` se dă pe exercițiul închis, nu pe anul calendaristic: un
  consum din anul anterior primește cota corecției cât timp exercițiul lui e
  deschis. (E3-D10)

Regula stă în `Nucleu/Motor/CorectieValorica.cs`; declarația în
`DeclarantFacturaIntrare.Corectie.cs`; faptele lotului în
`Cub/Citiri/CorectiiValorice.cs`; compensarea în
`Cub/Materializare.CorectiiValorice.cs`. Proba: `--scenarii CVL` și
`CorectieValoricaTeste`. (119)

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (18; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): 2, 13, 23, 25, 27, 37, 38 (amendată de 81, 84), 53, 56 (amendată de 77, 81), 62, 75, 98, 99, 102, 119, 124, 125 (propusă), 126 (propusă).
<!-- /gen:decizii -->
