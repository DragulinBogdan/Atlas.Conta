# SAF-T

**Actualizat: 2026-10-15.** [Index](README.md)

Fișierul SAF-T (D406): profilul și forma exportului, taxele, terțul comercial, registrul contabil, facturile și plățile (L), stocurile (S), reconcilierile și validarea.

## Profilul și forma exportului

SAF-T L și S folosesc profilul privat; profilul bugetar este neaplicabil și
este refuzat cu 422. Exportul este lunar. Sumarul JSON expune agregate,
avertismente și reconcilieri; XML-ul este scris prin streaming. (73c, 73g)

Exportul L și S citește numai cubul; ruta pe registre nu există
(`SaftProiectii.Saft` și `SaftStocuri`). Diferențele ei față de cub sunt
clasificate o singură dată, cu martor numeric, în
[raportul A/B final](../nucleu/tr-d8-saft-ab.md); probele D16/D17 rulează pe
cub. (SAF-B8)

Identificatorii emiși trebuie să fie unici și să respecte lungimile
acceptate. Data postării nu se emite când este în afara perioadei acceptate.
Profilul XML de stoc păstrează secțiunile L goale și cere secțiunea de stoc
fizic; o reprezentare invalidă este refuzată înainte de scriere. (74b, 74c, 74e)

O secțiune `GeneralLedgerEntries`, `SalesInvoices`, `PurchaseInvoices` sau
`Payments` fără intrări se scrie goală, fără `NumberOfEntries` și totaluri,
pe ambele module: validatorul respinge totalurile zero, iar schema cere
secțiunea prezentă. (74e, S0-R5)

## Taxele

`TaxInformation` citește `Cub.Citiri.Fiscale`. `MapareTvaSaft` selectează
codurile după versiunea exportului, secțiune, tip, regim, cotă istorică,
import, sens și rol. Lipsa mapării produce diagnostic; autocolectarea are
cod distinct numai în GeneralLedger. Politica are editor React și proveniență.
Restul `SourceDocuments` și sursele contabile/stoc ale exportului complet
sunt inventariate separat în TR-D8; validarea taxelor nu certifică migrarea
sau securitatea întregului SAF-T. (103, D8-B8)

TVA este asociată pe linie și storno. Retururile/stornările folosesc tipul și
semnul fiscal corespunzător. Pentru facturile de stoc, legătura contabilă
poate proveni din NIR-ul materializat. Societatea apare în rolurile de
client/furnizor cerute de reprezentarea celeilalte laturi. (73e)

## Terțul comercial

În L, terțul comercial al unei postări pe un cont cu `RolTert` e
repartitorul ei de sold, când e extern (`PostareJurnal.TertComercial`), fără
rezervă pe partenerul fiscal (S11, C111-Q9). Registrul, facturile și
soldurile Customers și Suppliers citesc aceeași axă:

- postarea cu partidă are ca repartitor titularul, deci iese pe el;
- capătul de decont cu terț pe linie iese pe terțul liniei, în registru și
  în sold;
- postarea fără repartitor extern de pe un cont cu rol comercial — capătul
  intern cu fapt fiscal, nota pe repartitor intern — iese în registru cu
  codul societății și avertismentul `TertFaraPartener`, iar soldul ei intră
  în `Neincluse`: cu `RepartitorNePartener` și repartitorul numit când are
  unul, cu `FaraPartener` când n-are;
- angajatul e repartitor extern, dar nu partener: postarea lui are aceeași
  soartă, avertisment în registru și `RepartitorNePartener` la sold.

Probe: SC-C111-26, SC-C111-27. (S11, C111-Q9)

## Registrul contabil și facturile (L)

`SaftProiectii.SaftPeCub` produce nomenclatoarele, GL-ul, facturile și
plățile din `Citiri.Contabil.Jurnal`, `Citiri.Fiscale` și `Citiri.Plati`, fără
registre. Ruta publică L îl folosește (R1). (TR-D8 S1, S2)

Tranzacția GL este tranzacția cubului, inclusiv deschiderea din lună.
Linia GL este postarea, iar totalul D/C este rulajul balanței.
`TransactionDate` și `GLPostingDate` sunt data contabilă; `SystemEntryDate`
este data UTC a `ScrisLa`. Factura este evenimentul cubului
(`Operare`/`Storno`) și poartă `TransactionID`-ul tranzacției GL. Tipul:
storno 381, reemisă după corecție 384, total comercial negativ 381, altfel
380. Luna închisă se reexportă identic după o corecție ulterioară (B').
Brutul vine din postările de terț, iar netul, taxa și autocolectarea din
faptele fiscale ale liniei. Brutul trebuie să fie Σ(net + taxă −
autocolectare); cantitatea comercială vine din linia operată.

Linia de corecție a valorii lotului (FCT, `CorectieValoareLot`) iese cu
cantitatea 0, prețul 0 și informația fiscală nefiscală (`000`/`000000`),
câte o linie pe fiecare cont de contrapartidă: stocul rămas și consumul
compensat. Factura urmează regula comună a tipului (380; 381 pe storno sau pe
total negativ, deci și pe diminuare). Pe o linie fiscală a unui lot care are o
corecție de valoare (returul la furnizor pe lot corectat), cota corecției din
linie, fără fapt fiscal, iese ca a doua linie nefiscală cu cantitatea 0,
lângă baza fiscală; pe orice alt lot, diferența rămâne refuz. (E2-D10, 122 g)
Ambiguitatea, sursa lipsă, maparea lipsă și corecția cu înlocuitor Draft
sunt refuzuri (`SaftDto.Refuzuri`), iar XML-ul nu se scrie. Citirea cere
RepeatableRead și refuză o tranzacție ambiantă mai slabă. Regulile complete:
[S1-R](../nucleu/tr-d8-saft-contract.md#s1-r--review-și-tranșări-2026-09-28). (TR-D8 S1)

Unitatea de măsură a unui produs cu mișcări operate nu se mai schimbă: modificarea
e refuzată, deci cantitatea istorică se exportă în unitatea definită. Stabilitatea
octet cu octet a reexportului e probată cu metadatele și data generării
fixate; redenumirile din nomenclator (descrieri) rămân etichete curente.
[Review S1](../nucleu/tr-d8-saft-s1-review-codex.md). (TR-D8 S1, R1)

Factura în valută se declară în RON, cu avertismentul `FacturaInValuta`
(cubul nu poartă valuta, B-r6). `ConturiDiferite` numără conturile al căror
Closing − Opening din GeneralLedgerAccounts diferă de rulajul net al liniilor
GL emise; `Neincluse` pe terți poartă cauza și numele repartitorului, după
regula terțului comercial de mai sus. (B-r6, S11)

## Plățile (L)

Plata este evenimentul cubului (`Operare`/`Storno`) al unui document de
trezorerie, cu `TransactionID`-ul tranzacției GL. Liniile ei sunt postările
de pe latura contrapartidei (opusă laturii pe care `Document.Laturi()` admite
numai contul propriu), grupate
pe cont, terț și țintă; terțul e repartitorul extern al postării
(`Citiri/Plati`). Ținta se află la capătul lunii operării: partida
străină nominalizată la operare, perechile `Transfer` de pe partida proprie
datate până atunci (legătură, desfacere, notă care stinge avansul) și restul.
`SourceDocumentID` este numărul documentului-origine al țintei. Restul și
partida inițială nu au referință. Stornoul neagă liniile operării. Legătura
de după luna plății nu apare în nicio declarație. Viramentul intern rămâne
numai în GL. Plata către partener fără cont cu `RolTert` intră în
`Neincluse`. Plata către angajat poartă codul societății. Valuta și faptele
fiscale pe plată refuză exportul. Compensarea prin notă nu este plată
(SAFT-r1). Regulile:
[S2](../nucleu/tr-d8-saft-contract.md#s2--payments-și-comutarea-l-contract-pentru-aprobare). (TR-D8 S2)

## Stocurile (S)

`SaftProiectii.SaftStocuriPeCub` produce PhysicalStock și MovementOfGoods din
postările pe lot (`Citiri.Loturi`), fără registre. Categoria stocului vine din
contul istoric al postării (`Cont.CategorieStoc`, moștenită de la părinte;
privat: 30/34/38 Magazie, 37 Mărfuri, 6 și 711 Consum). Magazie/Mărfuri au
poziții. Consumul nu e stoc, iar liniile lui cer politică, de regulă
excludere. Folosință, Custodie, Gratuit și producția în curs sunt
neacoperite. Contul fără categorie pe tot lanțul refuză fișierul. (TR-D8 S3)

Mișcarea este evenimentul cubului (`Operare`/`Transfer`/`Storno`) × cod.
Linia agregă postările pe (linie, lot, cont, gestiune). Codul vine din
politica pe tip × categorie × semn × cauză, cu semnul originii pe storno;
pe linia fără cantitate, semnul e al valorii. NIR delta
este 10 cu cantitatea semnată (S3-R1). ASM raportează producția la valoarea
finală a cubului (ΣC, S3-R2). Linia numai valorică e acceptată doar pentru
corecția valorii lotului: cauza `CorectieValorica` din cheia politicii dă 90
pe plus și 60 pe minus, iar partea pe consum e exclusă cu motiv. Orice altă
linie numai valorică rămâne refuz (SAFT-r2). (E2-D10)
Poziția este lot × cont × produs × gestiune; `Opening` include deschiderea
din lună. Cheile: WarehouseID = codul gestiunii, `StockAccountNo` = lotul,
`MovementReference` lizibilă sau rezerva `{TranzactieId:N}{cod}`. Pe o
categorie raportabilă, politica nu poate exclude. Regulile:
[S3](../nucleu/tr-d8-saft-contract.md#s3--movementofgoods-physicalstock-și-comutarea-c-contract-pentru-aprobare). (TR-D8 S3)

Stocul fizic se grupează pe repartitor și lot, pentru tipurile eligibile;
mișcările pe document × storno × cod, cu cantități și valori semnate. (74a, 74c)

## Reconcilieri și validare

Datele neincluse rămân explicite și participă la reconcilierea notelor,
facturilor, TVA, soldurilor și nomenclatoarelor. Lipsa unui câmp necesar nu
este mascată printr-o valoare inventată. (73e)

Reconcilierile S urmăresc registrul față de ceea ce se emite, completitudinea,
soldurile pe cont și integritatea referințelor. Diferențele sunt raportate.
Validarea DUK verifică fișierul în profilurile testate; nu extinde acoperirea
funcțională declarată în [limitele curente](limite-curente.md). (74d, 74e)

## Limitele implementării

- Inițialul de stoc și de cont, ca și soldurile terților, pornesc din
  snapshot, sub accesul complet verificat înaintea proiecției. (105, SAFT-r4,
  X-D5)
- `TaxInformation` citește faptele pe luna exportului contabil. Ruta publică
  L citește cubul; validarea XSD/DUK certifică registrul general, facturile și
  plățile lunilor scenei. Citirea alocărilor filtrează postările contabile pe
  `Unitate`, prin indexul `(Unitate, Data)`. Numărul de comenzi SQL ale
  exportului e constant, iar istoricul nu se citește din `Postare`; planul pe
  volum real nu e măsurat (FZ-r3). (103, D8-B8, S1-R8, S2)
- După o inserare masivă, până la autoanalyze, citirea faptelor fiscale poate
  degenera în Nested Loop. (SAFT-r5)
- Lotul pe două conturi de stoc nu are azi producător; dacă apare, poziția îl
  separă prin `ProductType`, iar garda de injectivitate refuză cheile
  duplicate. (SC-SAFT-12 parțial)
- Acoperirea politicii de mișcări SAF-T se probează într-o singură direcție:
  fiecare pereche tip × categorie pe care cubul o produce în scena SAF-T are
  politică, iar scena atinge cele opt tipuri care mișcă stocul (FCT în locul
  NIR: pe cub recepția mișcă pe factură). Direcția inversă nu se măsoară:
  perechile pe Magazie și NIR/Marfuri rămân în politică fără scenă care să le
  producă. (D9-D8)

<!-- gen:decizii -->
## Decizii

Deciziile care numesc pagina în `Arii:` (5; titlurile în [index](../decizii/README.md); generat de `docskit gen`, nu se editează): 34, 73 (amendată de 74, 105), 74 (amendată de 105), 103, 122.
<!-- /gen:decizii -->
