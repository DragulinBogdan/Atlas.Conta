# Stoc

**Actualizat: 2026-10-15.** [Index](README.md)

Loturile și evaluarea FIFO, recepția și diferențele ei, transformarea, inventarul și corecția valorii lotului.

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

## Transformarea n→m (ASM-B2…B7, 2026-09-23)

`Declaratie.Transformari` conține linii cu rol consum/produs, capăt real,
cantitate, valoare și cauză. Primitiva pură adaugă o postare marcată
`Contrapondere` = transformare: același cont/produs/analiză/cauză, cantitate
opusă, valoare zero, fără repartitor, unitate sau fapt fiscal
(`CONTRAPONDERE_TRANSFORMARE_INVALIDA`). Conservările sunt neschimbate.
Stornoul selectează tranzacția Transfer de stoc întreagă, inclusiv aceste
postări din partiția Contabil, și verifică conservarea înaintea scrierii.

Declarantul citește rolul și prețul prin `ILinieCuTransformare`. P = R se
verifică și pe operandul închis, fără pregătirea valorilor entității.
Grupurile sunt clasificate o singură dată după P/R; contribuțiile Δ se
acumulează pe produsul țintă înaintea gardului de pozitivitate. Prețul cules,
valoarea entității și prețul lotului rămân ale registrelor. Stornoul inversează
ajustarea istorică exact; corecția calculează din nou. Contractul detaliat:
[transformarea ASM](../nucleu/tr-d7b-asm-transformare-contract.md).

## FCL și DSC pe cub (T-D4, felia 32, pasul 2)

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
  clasei (T-D13), fără linii, cantitate ≤ 0, `PRODUS_ALT_TIP`, regulă lipsă, TVA.
- **DSC** (`DeclarantDescarcareGestiune`): o mișcare per linie — lotul iese
  din gestiunea predatoare pe contul de stoc al regulii (`6xx = 3xx` per
  Tip), evaluat cu `Evaluare.Iesire` pe raportul curent în secvența liniilor
  (ca BCS), iar costul intră cu `+q` pe postarea marcată cu contrapondere,
  fără unitate (090 (g)). Cu primitor extern marcajul e „terț”, iar
  `Repartitor` e primitorul (T-D13 (g); fără partidă — 607 n-are rol de terț).
  Cu primitor intern marcajul e „consum”, iar postarea n-are repartitor
  (C111 S8). Aceeași formă cu sau fără factura-sursă. Lotul e ales de
  `DescarcareService` (pin-uri, apoi FIFO), declarantul nu realocă.
- **Transferul INC → FCL** e activ prin `Materializare.Imperecheaza` fără
  nicio schimbare: referința e a stingătorului (4111), plafonul e restul FCL
  pe 4111; partida 419 nu primește transfer.
- **Diferența declarată T-D4.2** (aceeași clasă ca T-D2.2, N-r3): pe Flax 842
  din 36.696 DSC (2,29 %), 919 linii din 62.063, toate ieșiri PARȚIALE (cele
  47.505 goliri sunt egale), Σ semnată +31,01 lei, |Δ| max 16,50 lei (lot cu
  preț înghețat 0), 909 din 919 linii ≤ 6 bani, pe 79.199.898,27 lei mutați.
  Nu se normalizează (T-r7).

## Corecția valorii lotului (TR-D9b-CV, 119)

Factura de intrare poate purta linii de corecție a valorii unui lot existent
(`FacturaIntrareDetaliu.CorectieValoareLot`): cantitate zero, valoare
semnată culeasă, lotul corectat pe linie. Linia corectată este linia de
intrare a lotului. Un document cu o linie de corecție are numai linii de
corecție, fără TVA; nu naște lot și nu generează NIR. (CV-D1)

- Valoarea liniei se împarte pe stocul rămas și pe consumurile active ale
  lotului, după cantitate: Hamilton pe poziții, apoi pe postările fiecărei
  poziții. Fiecare cotă nenulă e o pereche `Operare` cu debitul pe poziția
  beneficiarului, cantitate zero, atribuită postării beneficiare, și creditul
  pe partida proprie a corecției la furnizor. Bonul operat și prețul de
  intrare al lotului rămân neschimbate; ieșirile următoare se evaluează din
  soldul ajustat. (CV-D2, CV-D3)
- Lotul corectabil se judecă pe forma faptelor din cub: o recepție activă de
  la furnizorul corecției, consumuri din poziția de stoc și inversele lor.
  Orice altă mișcare pe lot refuză corecția. (CV-D10)
- Stornarea bonului scrie, în aceeași comandă, inversa lui și câte o
  tranzacție `Operare` pe fiecare corecție cu alocare activă pe el: perechea
  de compensare mută alocarea de pe consum pe stoc. Bonul stornat se închide
  la zero pe fiecare cont; corecția rămâne operată și poartă integral valoarea
  ei. Stornarea corecției inversează toate postările ei `Operare`, alocări și
  compensări. (119)
- La stornare, saltul pe `Atribuit` nu mai urmează postările pe lot; pe
  partide rămâne cel din 090 (i). (119 c)
- Fiecare `Operare` a corecției are explicație proprie: `AlocareValorica` pe
  fiecare beneficiar cu pondere, inclusiv cel cu cota zero, respectiv
  `CompensareValorica` pe fiecare pereche de compensare. Invariantul de audit
  (`CorectieValorica.Auditeaza`, în `INV-CUB`) reface repartizarea din
  ponderi și verifică postările și compensările față de decizii. (CV-D8)
- Refuzuri: `CV_STRUCTURA`, `CV_LOT_NEELIGIBIL`, `CV_VALOARE_INSUFICIENTA`,
  `CV_CRONOLOGIE`, `CV_DEPENDENTI`, `CV_AN_INCHIS`, `CV_PROVENIENTA`, toate în
  `CoduriRefuz`. Nucleul emite motivul (`MotivCorectieValorica`), iar
  declarantul și materializarea îl traduc prin `CoduriRefuz.Refuz`. Regimul
  pe stare citește aceleași garduri fără a executa comanda. (CV-D7, CV-D10)

Regula stă în `Nucleu/Motor/CorectieValorica.cs`; declarația în
`DeclarantFacturaIntrare.Corectie.cs`; faptele lotului în
`Cub/Citiri/CorectiiValorice.cs`; compensarea în
`Cub/Materializare.CorectiiValorice.cs`. Proba: `--scenarii CVL` și
`CorectieValoricaTeste`.

## NIR delta și citirile comune (098/099, 2026-09-24)

[Contractul NIR](../nucleu/tr-d8-nir-delta-contract.md) fixează sursa ca
tranzacție și linii istorice, un singur cumul activ sub blocare comună și
politica `tip × cauză × clasă`. `PoliticaDiferenta` este configurabilă,
cu proveniență de seed; nomenclatorul privat existent furnizează conturile 32x.
Bugetarul folosește maparea aprobată 473.01.09/408.00.00/461.01.09,
428.01.02 pentru angajați și 35x pentru PeDrum. Lipsa politicii sau a
analizelor obligatorii este refuz atomic. Stocul istoric, datoria și TVA-ul
facturii nu se rescriu; inversarea utilizează delta existentă.

`Citiri/Receptii` este intrarea comună pentru proveniența recepțiilor conexe.
Pentru NIR acoperit, cubul păstrează recepția pe factură și gardul ei de stoc:
după stornarea sau anularea NIR-ului acoperit cu consum, operațiile următoare
pe lot se judecă numai pe soldul cubului (098-r3, SC-X-27).
Refuzul retragerii este verificat înaintea modificării tracker-ului, iar
cel al operării înaintea numerotării.

Scriitorul stornoului garantează proveniența inversei. `Citiri/Invarianti`
verifică în ModelCheck (`INV-CUB`), pe faptele fiecărei scene a catalogului,
înaintea purjei ei, invarianții pe care scrierea nu-i garantează prin
construcție (schema garantează deja, prin `CK_Postare_FelUnitate`, unitatea și
nașterea partidei): proveniența
(`CITIRE_PROVENIENTA_LIPSA`), echilibrul pe tranzacție și carte, conservarea
transferului, taxa postată = taxa liniei, unitățile de partidă, proveniența
fișelor și explicația, inclusiv valoarea liniei = decizia ieșirii, cu semn
(`CITIRE_EXPLICATIE_LINIE`, D9-D3), și forma coordonatelor
(`VerificaCoordonate`, `CITIRE_COORDONATE_INVALIDE`, C111-D2): partenerul
fiscal numai cu fapt fiscal; locul numai cu fapt fiscal sau pe partidă; faptul
fiscal cu loc; cantitatea nemarcată numai pe lot; lotul pe repartitor intern;
partida pe repartitor extern; locul, repartitor intern; contraponderea
transformării fără repartitor, unitate, valoare, valută sau fapt fiscal, pe
orice postare marcată, cu sau fără pereche. Inversa fiscală se
compară cu originalul și pe `PartenerFiscal` și `Loc`
(`CITIRE_FISCAL_INVERSA`). Hosturile nu
scanează istoria la pornire; o bază care nu corespunde codului se recreează
(102b, 102d).
`Postari` compune interogarea fără diagnostic global per apel;
inversele Operare/Deschidere se clasifică prin ID și spațiul originii.
Balanța, fișa, jurnalul și soldul pe partener citesc cubul, împreună cu
snapshot-ul contabil. Axa analitică e una: repartitorul de sold al postării
(S1, C111-Q6). Capătul intern apare pe repartitorul lui intern, cu sau fără
fapt fiscal; contul propriu apare ca repartitor; partenerul numai fiscal și
locul nu sunt pe axă. `repartitorId` filtrează repartitorul de sold, de orice
fel; `repartitorNul`, pe fișă, înseamnă fără niciun repartitor. Capătul
intern cu partidă (419, 408, 4091) apare pe titular, nu pe locul intern (S2).
Snapshot-ul contabil (`SoldPerioadaContabil`) are aceeași axă, `RepartitorId`,
fără coloană de gestiune; citirea dinamică și cea din snapshot dau aceleași
cifre. Snapshot-ul
se scrie numai din cub și se reconstruiește din postări. Citirile securizate folosesc
postările autorizate, nu snapshot-ul global, pentru a păstra permisiunile
pe rând și membru. Fișa afișează toate conturile corespondente, fără să
inventeze o pereche; jurnalul are identitatea postare × spațiu.

Cititorul de lot folosește cheia lot/cont/produs/gestiune, cu Transfer și
inversele lui; gestiunea lotului e repartitorul postării cu lot. BCS/BTR/DSC/LDI/ASM evaluează ieșirile din soldul cubului.
FIFO și pinurile DSC
folosesc data înregistrării și coordonatele complete. Raportul de stoc
arată contul, gestiunea și costul unitar din sold; etichetele lipsă nu
elimină sumele. Snapshot-ul de stoc (`SoldPerioadaStoc`) se scrie numai din
cub pe aceeași cheie, cu gestiunea lotului (`GestiuneId`) și data deschiderii; citirile nesecurizate folosesc snapshot + fereastră,
inclusiv evaluarea ieșirii cu graniță strict anterioară documentului. Reconstrucția detectează și data alterată, pe lângă
diferențele de chei și măsuri. Soldurile 0/0 se omit, cele 0/valoare nenulă
rămân. Postările marcate cu contrapondere n-au lot, deci nu intră în
disponibilul real. TR-D8 rămâne în
lucru pentru fiscal/SAF-T și verificările transversale.
`Citiri/Transformare` oferă un singur predicat
pentru contraponderea transformării (`Contrapondere` = transformare),
utilizat și de probe/diagnostic.
DEC are probă numerică independentă pentru inversare și corecție peste
închidere: partidă −100 în ianuarie, zero după inversă, noua partidă −80.

Factura de avans 4091 privat / 409.01.01 bugetar nu produce recepție.
Seed-ul bugetar clasifică 409.01.01 fără stoc (clasa S), conform 099(d);
postările istorice nu se reclasifică prin această corecție de seed.

UrmarestePartide este activ și pe 461 privat, respectiv
408.00.00/461.01.09/428.01.02 bugetar. Efectul aparține contului, nu NIR-ului:
și FCT bugetar pe 408 deschide partida furnizorului (SC-NIR-36/FCT), iar
INC/PLT pe 461 folosesc urmărirea partidelor prin mecanismul comun.
SursaReceptieiId este proveniența unică, scrisă la generarea conexului;
corecția o păstrează independent de Autogenerat. Recepția-sursă se citește
o singură dată pe comandă, sub blocarea sursei la operare. Validarea
analizelor curente privește capătul diferenței; capătul stocului păstrează
analiza istorică. Imputatul fără cauză Imputabila sau fără deltă se golește.

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (15; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): 2, 13, 23, 25, 27, 37, 38 (amendată de 81, 84), 53, 56 (amendată de 77, 81), 62, 75, 98, 99, 102, 119.
<!-- /gen:decizii -->
