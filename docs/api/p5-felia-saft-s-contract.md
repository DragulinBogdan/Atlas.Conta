# Pasul 5, felia 17 — SAF-T D406 **S (stocuri)** peste `RegistruStoc` — CONTRACT

> Data: 2026-08-27. Restanța 73-r1. Precedentul direct: felia 16
> (`p5-felia-saft-contract.md`, decizia 73) — aceleași uși, același scriitor,
> același oracol (DUK). Invarianții testați: I (registrul e sursa), II
> (politica nu inventează comportament — codul de mișcare e DATE), IV, V
> (nimic nu se pierde: `Neincluse` + `Excluse` deliberate + avertismente).

## Ce a arătat explorarea (fapte, nu design)

Sursele: XSD v2.4.7/2.4.8/2.4.9 (`D:\Dev\Work\Anaf.XML\SafT\`, secțiunile S
identice între versiuni), `anaf/RO_SAFT_SchemaDefCod_16.02.2026.xlsx` (foile
`Nomenclator stocuri`, `SAFT_Nomenclator_StockChar`, matricea „Raportare la
cerere - Stocuri"), Ghidul ANAF D406 v2.0 (dec. 2021; copie în
`anaf/SAF_T_Ghidul_D406_v2.0_dec2021.pdf`, p. 6/8–10/35–36/40–42),
`D406Validator.jar` J2.2.8 dezasamblat (`javap`), codul feliei 16.

1. **S = altă declarație, nu secțiuni opționale ale lunarei.** Tipul vine
   EXCLUSIV din `HeaderComment = "C"` (validatorul: `AUDIT_FILE_TYPE.ON_DEMAND
   = "C"`); nu există `D406S`, apelul DUK e identic cu L. Secțiunile așteptate
   pentru `C` (validator + ghid p. 42): `GeneralLedgerAccounts, TaxTable,
   UOMTable, AnalysisTypeTable, MovementTypeTable, Products, PhysicalStock,
   Owners, MovementOfGoods`. `Customers/Suppliers/GLE/Sales/Purchase/Payments`
   = N/A (tag gol, schema le cere ca element). `PhysicalStock` e singura
   secțiune S opțională în schemă (`minOccurs=0`; prezentă ⇒ ≥1 intrare).
2. **Perioada**: pentru `C` validatorul NU corelează `HeaderComment` cu
   `SelectionCriteria`; ghidul cere „declarații separate pentru fiecare
   lună/trimestru din perioada solicitată" ⇒ **S = un fișier per lună**,
   aceeași parametrizare `(an, luna)` ca L. Termen: ≥ 30 zile de la solicitare
   (art. 59^1 CPF + OpANAF 1783/2021, 73-r7). Rectificativa = redepunere
   integrală, fără bifă.
3. **Nomenclatorul mișcărilor** (19 coduri, ACELAȘI pentru `MovementType` și
   `MovementSubType`; valoare din afara listei = eroare fatală): 10 Achiziție ·
   20 Producție · 30 Vânzare · 40 Retur produse vândute · 50 Retur produse
   achiziționate · 60 Reduceri comerciale primite · 70 Consum · 80 Transfer
   intern · 90 Cheltuieli ulterioare capitalizate · 100/101 Diferențe de preț
   ± · 110 Plus de inventar · 120 Minus de inventar · 130/140 Ajustări
   depreciere / reluări · 150 Gratuit · 160 Degradate · 170 Expirate · 180 Alte
   tranzacții (fără cantitate).
4. **Ce validează DUK pe S** (tot ce există): formatele `00`–`08` pe
   `CustomerID/SupplierID/OwnerID`, „`CustomerID` și `SupplierID` nu pot fi
   ambele 0", `OwnerID ≠ "0"`, `AccountID` în planul de conturi al
   `TaxAccountingBasis`, `UOM` în nomenclator, `ProductCommodityCode` contra
   NC8, `MovementType/SubType` contra nomenclatorului, `MovementPostingTime`
   `HH:mm`. **Nicio aritmetică** (Opening + intrări − ieșiri = Closing NU e
   verificată; totalurile `NumberOfMovementLines/TotalQuantityReceived/Issued`
   n-au regulă). Cusăturile sunt ale noastre.
5. **Granularitatea `PhysicalStock`** (ghid p. 36): „fiecare produs per
   gestiune și per preț unitar aplicabil" la FIFO/LIFO; `StockAccountNo` =
   identificatorul lotului când sunt mai multe intrări per produs; `OwnerID`
   obligatoriu (bunuri proprii ⇒ `00`+CUI raportor, `Owners` GOL — ghid p.
   36: „dacă proprietarul tuturor stocurilor e contribuabilul, nu se
   raportează nimic în `Owners`"; terții ⇒ `8038`). `ProductType` = TEXT liber
   ≤ 18, fără nomenclator. `StockCharacteristics` = `blue_35`/`yellow_124`
   doar la accizabile, altfel `0`.
6. **Terții pe liniile de mișcare** (xlsx SD.MG.21/22, verbatim): livrare
   către client ⇒ `CustomerID` = partenerul, `SupplierID` = `0`; achiziție ⇒
   simetric; mișcările interne ⇒ AMBELE = `00`+CUI raportor. Convenție
   DIFERITĂ de L (unde latura liberă = raportorul).
7. **Cantitatea**: „așa cum e exprimată în documente"; semnul pe ieșiri NU e
   specificat nicăieri (`TotalQuantityReceived/Issued` separate sugerează
   direcția din tip). Se măsoară cu DUK (riscul 1).
8. În repo: `SaftXml` emite azi `MovementTypeTable/Owners/Assets/
   MovementOfGoods` ca tag gol și NU emite `PhysicalStock`; `RegistruStoc` =
   `(Data, TipStoc, LotId, RepartitorId, Cantitate semnată, Valoare semnată,
   Storno, DocumentId?, DetaliuId?)`; `RegulaStoc` = `(TipDocument × Latura ×
   Clasa?) → TipStoc + Semn`; `StocProiectii.SoldStoc` = agregat pe
   `(Lot × Repartitor × TipStoc)`; nicio fișă de stoc cronologică.
   `Repartitor` are `Cod/Denumire/Calitati/CodFiscal/Tara…`; `Gestiune` n-are
   câmpuri proprii.

## De ce codul de mișcare e politică, iar restul e cod

Tipul de mișcare e o funcție a TIPULUI de document și a registrului pe care
îl atinge (BCS ⇒ 70, NIR ⇒ 10), NU a produsului sau a liniei — deci e o
politică per tip (decizia 4, 20), pe cheia `(TipDocument × TipStoc × Semn?)`,
seed-uită pe funcționalitate per profil (21). Ce e al legii — formatul
identității, convenția `0`/raportor pe laturi, granularitatea per preț —
rămâne în `SaftReguli` (cod). Identitatea raportorului, produsul, UM, NC:
deja fixate de 73a/b. Registrul de stoc e sursa unică (I): fișierul S e o
PROIECȚIE a lui; niciun calcul nou de stoc, doar agregare.

## Deciziile de fixat

### D17-D1 — `PoliticaMiscareSaft`: `(TipDocument × TipStoc × Semn?) → cod + rol`

- Clasă nouă în `Politici.cs`, `[NavigationItem("Politici")]`, editabilă (ca
  `RegulaStoc`; NU `ForbidCRUD`): `TipDocumentId` (FK), `TipStoc` (enum),
  `Semn` (`int?`; `null` = orice semn), `CodMiscare` (`string`, `MaxLength 9`,
  **nullable**), `RolTert` (enum nou `RolTertSaft { Niciunul, Client,
  Furnizor }`), `Motiv` (`string?`, cerut când `CodMiscare` e null).
  Unic filtrat pe `(TipDocumentId, TipStoc, Semn)` cu `GCRecord = 0` (ca
  `MapareD300`, 69b) **+ al doilea index unic pe `(TipDocumentId, TipStoc)`
  filtrat `Semn IS NULL AND GCRecord = 0`** (în Postgres `NULL <> NULL`, altfel
  dublura „orice semn" trece; amendat la pasul 1, probat pe calea reală);
  gardian: `CodMiscare` din nomenclatorul de cod
  (`SaftReguli.CoduriMiscare`, lista din 3), `Motiv` obligatoriu la cod null.
- Semantica: rând cu cod ⇒ rândurile de registru potrivite se emit cu acel
  cod; rând cu cod **null** ⇒ **`Excluse` DELIBERATE** (agregat per politică:
  număr, Σ cantitate, Σ valoare, motiv) — nu `Neincluse`; rând de registru
  FĂRĂ politică ⇒ `Neincluse` cu cauza `FaraCodMiscare` (tip × TipStoc ×
  semn ca exemplu). Potrivire: `Semn` exact bate `Semn null`.
- `TipStoc`-urile RAPORTATE = cele care apar în politici cu cod; `PhysicalStock`
  se calculează DOAR pe ele (soldurile `Consum`/`Folosinta` nu sunt patrimoniu
  în magazie). Un rând de registru pe un `TipStoc` fără nicio politică cu cod
  ⇒ `Neincluse`; soldul de deschidere pe un astfel de `TipStoc` ⇒ avertisment
  `SoldPeTipStocNeraportat` (număr, Σ valoare), nu `Neincluse` (n-are
  document).
- Seed privat (funcționalitate, nu transcriere): NIR `Magazie/Marfuri +` ⇒
  `10 / Furnizor`; BTR ± ⇒ `80 / Niciunul`; BCS `Magazie/Marfuri −` ⇒ `70 /
  Niciunul`, BCS `Consum +` ⇒ cod **null**, motiv „consumul pe responsabil nu
  e stoc în magazie"; LDI `+` ⇒ `110`, `−` ⇒ `120`; DSC `−` ⇒ `30 / Client`;
  ASM `+` ⇒ `20`, `−` ⇒ `70`; RLF `−` ⇒ `50 / Furnizor`; RDC `+` ⇒ `40 /
  Client`. Bugetar: NIMIC (S e `Neaplicabil` la bugetar, ca L — 73c).
  `Custodie`/`Gratuit`/`ProductieNeterminata` fără politică (nimic nu le
  produce azi) — dacă apar, ies în `Neincluse`.
- Migrația atinge DOAR tabela nouă. OData: politica = **ReadOnly** (regula 56:
  politicile sunt ReadOnly pe OData, ca `MapareD394`; `RegulaStoc` nu e expusă
  deloc — amendat de main la pasul 1). `--dump-metadata` pentru caption-uri.

### D17-D2 — `SaftReguli` (cod): funcțiile legii pentru S

- `CoduriMiscare`: dicționar cod → descriere RO (din 3), sursa UNICĂ pentru
  `MovementTypeTable.Description` și pentru gardianul politicii.
- `TertiLinieStoc(rol, idPartener?, idSocietate) → (CustomerID, SupplierID)`:
  `Client` ⇒ `(partener, "0")`, `Furnizor` ⇒ `("0", partener)`, `Niciunul` ⇒
  `(societate, societate)`; rol cerut dar partener lipsă/ne-`Partener` ⇒
  `(societate, societate)` + avertisment `TertLipsaPeMiscare` (6).
- `OwnerIdRaportor(societate)` = `"00" + CUI` (5). `ProductType(simbolCont)` =
  simbolul contului de stoc (≤ 18). `StockCharacteristic` = `("0","0")`
  (riscul 3).
- `MovementReference(codTip, numar, cod?)` ≤ 35 (D3).

### D17-D3 — Proiecția `SaftProiectii.SaftStocuri(os, an, luna)` → `SaftDto`

ACELAȘI `SaftDto` (un DTO, un scriitor): se adaugă `SaftTipMiscare`,
`SaftMiscareStoc`/`SaftLinieMiscareStoc`, `SaftStocFizic`, `SaftExclus`,
rezumatul primește cusăturile S; `Header.HeaderComment = "C"`. Listele L
(`Customers/Suppliers/Jurnale/Facturi/Plati`) rămân GOALE; `SaftXml` scrie ce
e nenul și omite `PhysicalStock` când lista e goală.

- **Header**: ca L, cu `HeaderComment = C`, aceeași lună `(PeriodStart/End)`.
- **GeneralLedgerAccounts / TaxTable / UOMTable / AnalysisTypeTable**: ca L
  (refolosire); `TaxTable` = codurile folosite în perioadă (poate fi gol);
  `UOMTable` = UM-urile produselor declarate; `AnalysisTypeTable` = ca L
  (dimensiunile perioadei) — S nu emite `Analysis` pe linii.
- **MovementTypeTable**: codurile DISTINCTE folosite pe mișcările perioadei,
  cu descrierea din `CoduriMiscare`.
- **Products**: produsele care apar în `MovementOfGoods` SAU în
  `PhysicalStock` (nu tot nomenclatorul); forma ca L (`CodNc` → `0` + avertisment
  `FaraCodNc`, UM → `H87` + `FaraUnitateMasura`, `ValuationMethod FIFO`).
- **PhysicalStock**: o intrare per `(Repartitor × Lot)` pe `TipStoc`-urile
  raportate (lotul = prețul unitar aplicabil, ghid p. 36; `TipStoc` diferite
  pe același lot × gestiune se ADUNĂ — patrimoniul e al gestiunii);
  `Opening` = Σ `(Cantitate, Valoare)` cu `Data < 1 al lunii` (inclusiv
  rândurile de deschidere `DocumentId null`), `Closing` = Σ cu `Data ≤
  ultima zi`; intrările cu `Opening = Closing = 0` ȘI fără mișcare în lună se
  OMIT; `WarehouseID` = `Repartitor.Cod` (gol ⇒ `Denumire` ≤ 35);
  `ProductCode` = `Produs.Cod`; `StockAccountNo` = `Lot.Id` (Guid, ≤ 70) doar
  când produsul are > 1 lot în aceeași gestiune, altfel omis; `ProductType` =
  simbolul contului de stoc al produsului (`TipMaterial.ContImplicit`,
  `SimbolSaft`; lipsă ⇒ `"0"` + avertisment `ProdusFaraContStoc`);
  `StockAccountCommodityCode` = `CodNc` sau `0`; `OwnerID` = raportorul;
  `UOMPhysicalStock` = UM-ul produsului; `UOMToUOMBaseConversionFactor 1`;
  `UnitPrice` = `Lot.PretUnitar` la 2 zecimale; `StockCharacteristics
  ("0","0")`. Soldurile negative (nu ar trebui — gardianul 25d) ⇒ se emit ca
  atare + avertisment `SoldNegativ`.
- **MovementOfGoods**: rândurile de registru cu `DocumentId ≠ null` și `Data`
  în lună, potrivite pe politică; un `StockMovement` per **`(Document × Storno
  × CodMiscare)`** (un document cu două coduri — ASM — se sparge; stornoul =
  mișcare proprie, ca `381`); `MovementReference` = `{CodTip}-{Numar}`, +
  `/{cod}` doar când documentul se sparge, + `/S` pe storno; `MovementDate` =
  `Data` rândului; `MovementPostingDate` = `DataOperare` (data);
  `MovementType` = codul; `DocumentReference` = `(CodTip, Numar)`; linia =
  rândul de registru: `LineNumber` = poziție `1..n` în `(Id)`; `AccountID` =
  contul de stoc al produsului (ca `ProductType`; lipsă ⇒ `Neincluse`,
  `FaraContStoc` — un cont inventat e interzis, 73e); `TransactionID` =
  Guid-ul documentului; `CustomerID/SupplierID` din `TertiLinieStoc` cu
  partenerul documentului — **partenerul = de pe laturile documentului, sau
  ale `DocumentSursa` când documentul e autogenerat** (NIR ← FCT, DSC ← FCL;
  `Autogenerat` + `DocumentSursaId`); `ProductCode`; `StockAccountNo` =
  `Lot.Id` (aceeași regulă ca la `PhysicalStock`, pe aceeași gestiune);
  `Quantity` = **`Cantitate` SEMNATĂ ca în registru** (intrare +, ieșire −,
  stornoul inversat; riscul 1 — fallback |q| cu semnul doar pe storno);
  `UnitOfMeasure` = UM-ul produsului; `UOMToUOMPhysicalStockConversionFactor
  1`; `BookValue` = `Valoare` semnată; `MovementSubType` = același cod;
  `MovementComments` = `Descriere`-a liniei dacă există; fără
  `TaxInformation`, fără `ShipTo/From` (gestiunea e în `PhysicalStock`;
  transferul BTR iese ca DOUĂ linii ±, gestiunile prin `WarehouseID` pe
  `ShipFrom/ShipTo` = restanță). `NumberOfMovementLines` = nr. linii;
  `TotalQuantityReceived` = Σ `Cantitate > 0`, `TotalQuantityIssued` = |Σ
  `Cantitate < 0`|.
- **Owners**: GOL (proprii). Terții cu `8038` = restanță cu nume.
- **Cusăturile (contract, la cent / la a 3-a zecimală pe cantități)**:
  (S1) per intrare `PhysicalStock`: `Opening + Σ Quantity(lot × gestiune, în
  lună, pe TipStoc raportate) == Closing`, idem pe valoare; global Σ.
  (S2) Σ `BookValue` (mișcări) + Σ `Excluse.Valoare` + Σ `Neincluse.Valoare`
  == Σ `RegistruStoc.Valoare` pe documentele lunii (TOATE `TipStoc`); idem
  cantitate. (S3) Σ `ClosingStockValue` per `ProductType` (cont de stoc) vs
  `Closing` din `Balanta` pe același cont — **măsurată și raportată**, NU
  blocantă (registrul contabil poate purta 3xx și din NTC/deschideri fără
  lot); diferența per cont în rezumat. (S4) fiecare `ProductCode` din mișcări
  și stoc fizic e în `Products`; fiecare cod din mișcări e în
  `MovementTypeTable`; fiecare `CustomerID/SupplierID ≠ 0` are format valid.
- `Sumar(dto)` = contoarele S (mișcări, linii, intrări de stoc fizic, tipuri
  de mișcare, produse) + rezumat + `Neincluse` + `Excluse` + avertismente.
- Bugetar ⇒ `Neaplicabil` (aceeași regulă ca L: fără `RolTert/Functie`
  ⇒ 422).

### D17-D4 — Fișierul, REST, client, Import1C

- `SaftXml.Scrie` = același scriitor: `MovementTypeTable`, `PhysicalStock`
  (omis când gol), `MovementOfGoods` scrise din DTO; ordinea/numele din XSD.
- `SaftController`: `GET api/proiectii/saft/stocuri` (sumar, `User` ⇒ 200
  gol) și `GET api/proiectii/saft/stocuri/xml` (streaming, `User` ⇒ 403,
  bugetar ⇒ 422, CUI invalid ⇒ 422, perioadă ⇒ 400) — aceleași gărzi,
  ordinea lor identică; fișier `SAF-T-S_{CUI}_{an}-{luna}.xml`.
- Client `/saft`: comutator **L / S** (URL = stare: `?fel=S`), secțiunile S
  (mișcări, stoc fizic, tipuri), cusăturile S1–S4 cu stare, `Excluse` +
  `Neincluse` (plafon 200), descărcare prin `fetch + blob`. Codegen.
- Import1C: `--saft-s <an> <luna>` peste `Saft1C` (parametru `fel`), raport
  per cauză + DUK; nicio schimbare în import/reconciliere.

## Ce NU intră, cu motiv

- `Owners` cu terți (`8038`), custodie/consignație — nimic nu le produce azi
  (`TipStoc.Custodie` fără regulă); restanță.
- `ShipTo/ShipFrom` pe BTR, `LocationID`, `MovementPostingTime` — opționale,
  fără sursă în model.
- Trimestrial `T`/`NT` pentru S, segmentarea (73-r2/r3) — aceleași restanțe.
- `ValuationMethod` ≠ FIFO (51e), UM cu conversie (`UOMStandard ≠ UOMBase`;
  `Produs.UM` string, 73-r5).
- Codurile 60/90/100/101/130/140/150–180: niciun tip de document le produce;
  politica le poate primi fără release când apar tipurile.
- SAF-T A (active) = modul separat (decizia 9).

## Riscurile pin-uite (ținta review-ului advers)

1. `Quantity` NEGATIV pe ieșiri/storno — DUK îl acceptă? (`SAFquantityType`
   n-are `minInclusive`); fallback: |q|, semnul doar pe storno, direcția din cod.
2. Tag-urile goale `GeneralLedgerEntries/SalesInvoices/PurchaseInvoices/
   Payments/Customers/Suppliers` într-un fișier `C` — DUK le tolerează
   (validatorul n-are mesaj de respingere găsit)? Se măsoară.
3. `StockCharacteristic = "0"` (cheia e text liber fără nomenclator) — se măsoară.
4. `StockAccountNo` = Guid (36 caractere, ≤ 70) și `MovementReference` ≤ 35
   cu sufixe — lungimile.
5. `ProductType` = simbol de cont (`301`, `371`) — validatorul n-are regulă; se
   măsoară că nu e confundat cu `AccountID`.
6. `AccountID` pe linia de mișcare = `TipMaterial.ContImplicit` — pe Flax,
   produsele importate au contul din identitatea `nomenclator × simbol` (50a);
   câte rânduri ies `FaraContStoc`? Cifra, nu presupunerea.
7. S1 pe Flax: soldurile de deschidere ale importului (`DocumentId null`, lot
   per codmat) + mișcările lunii == `SoldStoc` la fine de lună, la cent — dacă
   nu bate, e o gaură în proiecție, nu în registru (reconcilierea lunară a
   importului e verde).
8. Partenerul prin `DocumentSursa` (NIR ← FCT, DSC ← FCL): NIR manual (fără
   sursă) ⇒ fără partener ⇒ raportorul pe ambele + avertisment — nu refuz.
9. Perf: Flax are zeci de mii de rânduri de stoc pe lună; proiecția în
   memorie sub câteva secunde; două query-uri grupate (solduri cumulate până
   la început / până la sfârșit) + rândurile lunii, nu per lot.
10. Un rând de registru al unui document DRAFT nu există (registrul e al
    motorului) — dar `Stornat`: rândurile originale + cele de storno sunt
    ambele în lună sau în luni diferite; S1 trebuie să bată în ambele cazuri.

## Verificări (ModelCheck, ambele profiluri; DUK offline; HTTP pe clona Flax.Api)

- **V1 modelul**: politica — unicitate `(tip, TipStoc, Semn)` (al doilea ⇒
  refuz), cod din afara listei ⇒ refuz, cod null fără motiv ⇒ refuz; seed
  privat idempotent (numărul de rânduri stabil la re-seed); bugetar: zero
  rânduri.
- **V2 proiecția** (privat, scenă: deschidere pe 2 loturi × 2 gestiuni, NIR ←
  FCT, BTR, BCS, LDI ±, FCL + DSC, ASM, RLF, RDC, un storno în ACEEAȘI lună și
  unul peste lună, un NIR manual fără sursă, un produs fără cont de stoc):
  fiecare document ⇒ codul lui; ASM spart în două mișcări; `Excluse` (BCS
  Consum) cu Σ; `Neincluse` cu cauze; terții `(partener,0)` / `(0,partener)` /
  `(soc,soc)`; `PhysicalStock` per lot × gestiune cu `StockAccountNo` doar la
  > 1 lot; S1–S4 la cent; `Sumar` round-trip JSON; bugetar `Neaplicabil`.
- **V3 fișierul**: XML-ul scenei trece DUK (`ok`) cu `HeaderComment C`;
  riscurile 1–5 cu rezultatul scris în §Închidere; `PhysicalStock` omis când
  e gol trece; kit lipsă ⇒ SĂRIT zgomotos.
- **V4 HTTP** (clona Flax.Api): matricea 200/403/422/400 pe `stocuri` și
  `stocuri/xml`; timpii JSON/XML pe 09/2025; smoke vizual `/saft?fel=S`.
- **V5 Import1C** (clona Flax): `--saft-s 2025 9` și `2025 12` ⇒ DUK `ok` sau
  erorile per cauză; S1–S4 pe luna reală (S3 raportată per cont); `Neincluse`/
  `Excluse` per cauză cu cifre; dimensiunea fișierului și timpul;
  `reconciliere-*.txt` NEATINS.
- `verifica:drift` idempotent; `--dump-metadata` comis; ModelCheck verde pe
  ambele profiluri.

## Regula de oprire

Agentul se oprește și raportează (nu normalizează tăcut) dacă:
- DUK refuză fișierul scenei pe o regulă care cere schimbare de MODEL — cu
  mesajul brut;
- S1 sau S2 nu bat la cent/miime pe scenă;
- migrația atinge altceva decât tabela `PoliticiMiscareSaft`;
- proiecția are nevoie de un câmp pe `RegistruStoc`/`Lot`/`Produs` care nu
  există — se raportează, nu se adaugă;
- pe Flax, raportul de reconciliere diferă de baseline cu o linie;
- ModelCheck pică pe un profil; `verifica:drift` nu e idempotent.

**Explicit NU în regula de oprire**: avertismente DUK, `Neincluse`/`Excluse`
în masă pe Flax, S3 care nu bate (se raportează per cont), `FaraContStoc` pe
produsele importate — cifre, nu presupuneri.

## Pașii (un agent per pas, verificare independentă + commit după fiecare)

1. **Model + seed**: D1 (`PoliticaMiscareSaft`, `RolTertSaft`), migrație pe
   toate bazele, gardian, seed privat, OData, `--dump-metadata`, V1.
2. **Reguli + proiecție**: D2, D3 (`SaftStocuri`, DTO-urile S, `Excluse`,
   cusăturile S1–S4, `Sumar`), V2.
3. **XML + DUK**: D4 partea de fișier, V3 cu riscurile 1–5 scrise.
4. **REST + client + Import1C**: controller, codegen, `/saft?fel=S`,
   `--saft-s`, V4 + V5 (rulare detașată + monitor).
5. **Închidere**: review advers, fix-uri de main, decizia 74, CLAUDE.md §74 +
   restanțe cu nume, README jurnal, istoric, `lista-react.md`, memoria de
   handoff.
