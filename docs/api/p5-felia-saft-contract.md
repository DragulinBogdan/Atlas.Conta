# Pasul 5, felia 16 — SAF-T (D406 L) ca proiecție peste registre — CONTRACT

- **Data**: 2026-08-26
- **Tipar**: D300 (69) / D394 (71) — proiecție peste registre, formularul e al
  legii, maparea e politică, ce cere formularul și modelul nu are se RAPORTEAZĂ.
- **Referința structurii oficiale**: `docs/api/d406-structura-2026.md`
  (schema 4.1.16 / istoric 133 din 16.02.2026; namespace producție
  `mfp:anaf:dgti:d406:declaratie:v1`; kit DUK local J2.2.8, publicat J2.2.15).
- **Restanțe pe care le închide**: D4-r5 (cod NC pe produs), D4-r10 (antetul
  societății), D4-r14 (`FaraOp11`), 69-r6/D4-r13 parțial (prima infrastructură
  de fișier XML din repo).

## Ce a arătat explorarea (fapte, nu design)

1. **Fundația se transferă aproape întreagă.** `RegistruTva` e deja per LINIE
   „fiindcă asta cere SAF-T" (`Registre.cs:198-200`), cu `DetaliuId` nenul;
   `RegistruContabil` poartă partenerul PER LATURĂ (dimensiunea `Repartitor`);
   `AtomContabil` (66) dă unpivot-ul; `Balanta(analitic: true)` dă soldurile
   per cont × repartitor; `CoduriTip` rezolvă tipul polimorf într-un query
   (60b); `Judet.Cod` e deja ISO 3166-2 (72b); adresa partenerului e deja
   tăiată la lungimile `AddressStructure` (72a); `TipTva.CodSafTLivrare /
   CodSafTAchizitie` poartă codurile reale ale nomenclatorului (36a, seed
   privat); storno = valori negative pe corespondența originală (46a) e exact
   convenția „storno în negru" a lui GL.31/32.
2. **Patru goluri blochează secțiuni întregi, nu câmpuri**: (a) entitatea
   **societate proprie** — `CustomerID` ȘI `SupplierID` sunt obligatorii AMBELE
   pe `TransactionLine`/`PaymentLine`, latura liberă = identificatorul
   raportorului; `Header` cere nume/CUI/adresă/contact/IBAN/`AuditFileRegion`/
   `TaxAccountingBasis`; (b) **cod NC** (`ProductCommodityCode` M pe `Product`);
   (c) **UM cu cod UN/ECE** (`UOMBase`/`UOMStandard` M; `Produs.UM` e string
   liber); (d) tipul fiscal al facturii (`InvoiceType` din 6 coduri admise).
   Identificatorul ≤18 (`SystemID`) e OPȚIONAL peste tot — golul dispare prin
   omisiune; `TransactionID` (70) și `PaymentRefNo` (35) încap un Guid.
3. **Validatorul e un oracol offline** (`anaf/duk_SAFT_an_luna/dist`,
   `java -jar DUKIntegrator.jar -d -v D406 f.xml erori.txt`, fișierul de erori
   conține literal `ok`). Regulile lui REALE, extrase din bytecode: `Name` în
   `CustomerInfo/SupplierInfo` respins („folosiți CustomerID"); tuplele
   `PaymentMethod ↔ PaymentMechanism`; `Amount == round(CurrencyAmount ×
   ExchangeRate)`; `Region` NUMAI pe RO; `HeaderComment ↔ perioada`;
   `AccountID` contra planului ales de `TaxAccountingBasis` (potrivire pe
   sintetic prin prefix); RegistrationNumber-ul societății = `RO`+CUI sau CUI
   (ALTĂ regulă decât prefixele `00`–`11` ale partenerilor). **Nicio regulă
   `Total == Σ linii` nu există în validator** — totalurile sunt ale noastre.
4. **Modul de raportare L** (lunar/trimestrial) = Header, GeneralLedgerAccounts,
   Customers, Suppliers, TaxTable, UOMTable, AnalysisTypeTable, Products,
   GeneralLedgerEntries, SalesInvoices, PurchaseInvoices, Payments. S (la
   cerere: MovementTypeTable, PhysicalStock, Owners, MovementOfGoods) și A
   (anual: Assets, AssetTransactions) sunt alte declarații (`HeaderComment`
   C/A), cu alte reguli — nu „secțiuni opționale ale lunarului".
5. **1C exporta L cu TVA fictiv** (cota 19% hardcodată, `TaxPercentage=1.00`
   pe note, `PaymentMethod='03'` fix, un depozit fictiv, fără încasări) și
   trecea DUK — deci validatorul nu verifică semantica fiscală; verificarea
   asta rămâne a noastră (cusăturile cu registrele, ModelCheck).
6. Planul de conturi al instituțiilor publice NU e printre cele 12 baze
   (`TaxAccountingBasis`); profilul bugetar n-are unde să se valideze.

## De ce societatea e nomenclator, identitatea SAF-T e cod, iar jurnalul e tipul

- **Societatea** e o entitate a bazei (35d: bază-per-client), editabilă de
  utilizator, nu o setare de seed înghețată: `SetareProfil` rămâne profil +
  rotunjire (52a); antetul societății e `Societate`, un singur rând, nucleu.
- **Identificatorul de partener** (`00`+CUI / `01`+ISO+cod / `03`+CNP / `04`+cod
  intern / `05`/`06` neînregistrați / `02` non-UE) e o funcție a legii peste
  cele 4 câmpuri fiscale ale lui `Partener` (71b) — cod, exact ca
  `TipPartener` din D394. La fel `InvoiceType` (380/381) și `PaymentMethod`
  (funcție a lui `TipInstrumentPlata`). Nu variază per client ⇒ nu sunt
  politică.
- **Rolul de client/furnizor al unui cont** (411/401…) e al planului ⇒ seed
  per profil, DATE (`Cont.RolTert`), nu simbol hardcodat (29).
- **Jurnalul** = `TipDocument` (o linie per tip): singura grupare care există
  în model și pe care `CoduriTip` o dă într-un query.
- **Fișierul se generează din proiecție, nu invers**: `SaftDto` (arbore de
  DTO-uri plate, testabil în ModelCheck, servit ca JSON) → `SaftXml`
  (scriitor streaming). Cusăturile se verifică pe DTO; validitatea sintactică
  pe XML, cu DUK.

## Deciziile de fixat

### D16-D1 — `Societate`: un rând, nucleu, editabil

- Clasă nouă `Societate : BaseObject` în `Nomenclatoare/`, un singur rând per
  bază (gardian: al doilea rând ⇒ refuz; seed-ul creează rândul GOL dacă
  lipsește, nu-l rescrie niciodată). Câmpuri: `Denumire` (70), `CodFiscal`
  (CUI normalizat ca la partener, fără `RO`), `InregistratTva`, `RegistruComert`,
  adresa PLATĂ cu ACELEAȘI 6 câmpuri și lungimi ca `Partener` (72a):
  `Strada/Numar/DetaliiAdresa/Localitate/CodPostal/JudetId` — `JudetId` cere
  `Tara == RO`; `Tara` ISO-2 default RO; contact: `ContactNume`, `ContactPrenume`
  (35 fiecare), `Telefon` (35), `Email` (70); `ContBancarId?` → `ContPropriu`
  (cu `EsteBanca` și IBAN — lookup filtrat); `BazaContabila` string (18) din
  setul `TaxAccountingBasis` (`A`/`IFRS`/`ONG`/`ONGE`/…; default `A`, validare
  pe listă în gardian); `RaporteazaCnp` bool default **false** (PF cu CNP ⇒
  `04`+cod intern, nu `03`+CNP — decizia de confidențialitate din legacy, ca
  politică a bazei).
- `Lungimi` (72a) devine sursă COMUNĂ: `AdresaSaft.Lungimi` (reflecție pe
  `Partener`) + probă ModelCheck că `Societate` are aceleași `MaxLength` pe
  cele 6 câmpuri.
- XAF: `NavigationItem("Configurare")`, detail view; OData: `Societate` CRUD
  (rândul unic — POST refuzat de gardian când există). Migrație
  `F16_Societate...` aplicată pe toate bazele de dev (63).
- Import1C (D6): `flax.Organizatii` + `InfoRg_*` + `ConturiBancare` umplu
  rândul DOAR pe câmp gol (regula 72d), raportat.

### D16-D2 — Produsul: `CodNc` + nomenclatorul `UnitateMasura`

- `UnitateMasura : BaseObject` nucleu, `[ForbidCRUD]`, seed autoritar din
  foaia `Unitati_masura` (UN/ECE Rec 20/21 — **2.162 coduri**, tuplul
  `(Cod, Denumire)` generat în `Comun/UnitatiMasuraUnEce.cs` dintr-un script
  peste xlsx, comis; `UnitatiMasuraAsteptate` verificat la seed ca `Judet`).
  Index unic filtrat pe `Cod` (`GCRecord = 0`).
- `Produs.UnitateMasuraId?` FK nou; `Produs.UM` (string) RĂMÂNE ca text de
  afișare/legacy (tech-debt marcat: se elimină când toate produsele au FK —
  restanță cu nume). Migrația umple FK-ul unde `UM` se potrivește exact pe
  `Cod` sau pe un dicționar RO→UN/ECE (`buc`/`bucata`/`bucată` → `H87`,
  `kg` → `KGM`, `l`/`litru` → `LTR`, `m` → `MTR`, `mp` → `MTK`, `mc` → `MTQ`,
  `ora`/`h` → `HUR`, `set` → `SET`, `to`/`tona` → `TNE`, `cm` → `CMT`;
  dicționarul e în `Comun/UnitatiMasuraRo.cs`, refolosit de Import1C).
- `Produs.CodNc` string(8) nullable, `[RuleRegularExpression] ^\d{8}$`; fără
  nomenclator NC8 seed-uit (9.984 coduri care se schimbă anual — validarea e a
  DUK-ului; restanță: nomenclator + lookup).
- În fișier: `ProductCommodityCode` = `CodNc` sau **`0`** când lipsește
  (valoarea pe care exportul legacy a depus-o în producție) + avertisment
  agregat `FaraCodNc` (număr + exemple); `GoodsServicesID` = `01` pentru
  `Natura == Stoc`, `02` altfel; `UOMBase = UOMStandard` = codul UM, factor
  `1`; produs fără FK de UM ⇒ `H87` + avertisment `FaraUnitateMasura`.
  `ValuationMethod` = `FIFO` (51e: modelul e apărat ca FIFO sub OMFP 96(3)).
- D4-r14: avertismentul `FaraOp11` din D394 se scoate; `op11` = tipul cu
  taxare inversă × `CodNc` (D4-r5 închis în D394 doar cât cere formularul —
  se implementează în pasul proiecției D394 dacă e ≤ 1 h, altfel rămâne
  restanță separată, cu nume).

### D16-D3 — Politica: `Cont.RolTert` + funcțiile pure ale legii

- `Cont.RolTert` enum `{Niciunul, Client, Furnizor}`, seed per profil (privat:
  `411*`, `413`, `418`, `4111`… ⇒ Client; `401`, `403`, `404`, `408`, `462` ⇒
  Furnizor; `419`/`409` avansuri: Client/Furnizor după clasă; lista exactă în
  `ProfilPrivat`, cu `VerificaProfil`-ul neatins). Bugetar: nimic (SAF-T
  neaplicabil, D5).
- Funcții pure, publice, în `Module/Saft/SaftReguli.cs`, testate în ModelCheck:
  - `IdPartener(Partener, Societate)`: `InregistratTva || (Tara==RO && CUI
    valid)` ⇒ `00`+CUI; `Fizica && CNP && RaporteazaCnp` ⇒ `03`+CNP; UE≠RO ⇒
    (`01` dacă înregistrat, altfel `05`)+ISO+cod; non-UE ⇒ `02`/`06`+ISO+cod;
    altfel `04`+`Partener.Cod` filtrat `[A-Za-z0-9]`. Lista UE = set static
    de 27 în cod. `IdSocietate(Societate)` = `00`+CUI.
  - `RegistrationNumberSocietate` = (`RO` dacă `InregistratTva`)+CUI.
  - `InvoiceType(document)`: `Storno` sau `ReturClient/ReturFurnizor` ⇒ `381`,
    altfel `380`; `SelfBillingIndicator` = `0`.
  - `MetodaPlata(TipInstrumentPlata)` → `(PaymentMethod, PaymentMechanism)`:
    numerar (`DispozitieCasa`, `Chitanta`) ⇒ `01`/`10`; `OrdinPlata` ⇒
    `03`/`42`; `Cec` ⇒ `03`/`20`. Tuplele = cele din nomenclator.
  - `SimbolSaft(Cont)` = `Simbol` fără puncte, segmentele terminale `00`
    tăiate? **NU** — se emit cifrele ca atare, fără puncte (`302.02.00` ⇒
    `3020200`); DUK validează pe sintetic prin prefix; lungimea (J2.2.5
    „6 cifre") se MĂSOARĂ în V3 și se pin-uiește în §Închidere.
  - `TipCont(Functie)`: `D` ⇒ `Activ`, `C` ⇒ `Pasiv`, `B` ⇒ `Bifunctional`;
    necunoscut ⇒ `Bifunctional` + avertisment.

### D16-D4 — Proiecția `SaftProiectii.Saft(os, an, luna)` → `SaftDto`

Un DTO întreg, plat (sealed classes, enum-uri ca string, `decimal?` = absență),
cu secțiunile L și **avertismente agregate per cauză** (`CodAvertismentSaft`).
Toate query-urile pe uși EF, fără SQL brut; două-trei query-uri grupate pe
registru + nomenclatoarele în dicționare (LEFT JOIN în memorie: „un rând nu
iese din raport fiindcă i-a murit eticheta"; partenerul șters logic se
declară, `IgnoreQueryFilters`).

- **Header**: din `Societate` + constante de cod (`SoftwareCompanyName`
  „Atlas", `SoftwareID` „Atlas.Conta", `SoftwareVersion` = versiunea
  assembly-ului); `HeaderComment` = `L`; `SelectionCriteria` pe
  `PeriodStart/End` (luna) — `T` (trimestrial pentru neînregistrați) =
  restanță; `AuditFileRegion` = `Judet.Cod` al societății; `TaxAccountingBasis`
  = `BazaContabila`; `DefaultCurrencyCode` `RON`; `Segment 1/1`.
- **GeneralLedgerAccounts**: `Balanta(dataStart, dataEnd, analitic:false)`
  — conturile cu sold sau rulaj; `Opening/Closing` prin `xs:choice` (debit sau
  credit, semnat, netat la cont); `AccountType` din `Functie`; `StandardAccountID`
  omis (planul e deja cel standard la privat).
- **Customers/Suppliers**: `Balanta(analitic:true)` pe conturile cu
  `RolTert`; un partener apare în `Customers` dacă are sold sau rulaj pe un
  cont Client, în `Suppliers` idem Furnizor (poate fi în ambele — decizia 16);
  `AccountID` = contul cu cel mai mare |rulaj| dintre cele ale rolului;
  soldurile = Σ pe conturile rolului; `CompanyStructure` din `Partener` (adresa;
  `City` gol ⇒ `Nespecificat` + avertisment `AdresaIncompleta`; `Country` =
  `Tara`; `Region` doar RO); `TaxRegistration` doar la prefix `00`
  (`TaxType` din `Nomenclator_Regim_fiscal`: `100040` dacă `TvaLaIncasare`,
  altfel `100010`; neînregistrat ⇒ fără `TaxRegistration`). Repartitorul care
  nu e `Partener` pe un cont cu `RolTert` (angajat pe 461) ⇒ `Neincluse` cu
  cauza + avertisment.
- **TaxTable**: un `TaxTableEntry` (`TaxType` `300`) cu `TaxCodeDetails` pentru
  fiecare cod SAF-T distinct FOLOSIT în perioadă (din `RegistruTva`, ambele
  direcții) + codul `000000` nu se declară. `TipTva` cu cod null folosit în
  perioadă ⇒ liniile lui ies cu `000/000000` + avertisment `TipTvaFaraCodSaft`
  (număr, sumă, exemple) — NU refuz (N19/TI19 pe perioade istorice).
- **UOMTable**: UM distincte ale produselor declarate. **Products**: produsele
  care apar pe liniile facturilor din perioadă (FCT/FCL/RLF/RDC via `ProdusId`
  sau `Lot.ProdusId`) — nu tot nomenclatorul.
- **AnalysisTypeTable**: câte o intrare per dimensiune FOLOSITĂ pe rândurile
  de registru ale perioadei (`CC` CentruCost, `P` Proiect, `U` Unitate, `SF`
  SursaFinantare, `CF`, `CE`), `AnalysisID` = codul nomenclatorului; pe
  `TransactionLine.Analysis` se emit dimensiunile laturii (fără `Repartitor` și
  `Material`, care au alt loc). Nicio dimensiune folosită ⇒ tabelul gol.
- **GeneralLedgerEntries**: `Journal` per `TipDocument` (din `CoduriTip` pe
  documentele perioadei; `JournalID` = `Cod`, `Type` = `Cod`); `Transaction` =
  un `Document` (`TransactionID` = Guid; `Period` = luna; `TransactionDate` =
  `Data`; `SystemEntryDate`/`GLPostingDate` = `DataOperare` (data, nu ora);
  `Description` = `Denumire tip + Numar`; `CustomerID/SupplierID` la nivel de
  tranzacție = ale partenerului dacă documentul are UN partener pe laturi,
  altfel societatea); rândul de `RegistruContabil` ⇒ **două** `TransactionLine`
  (debit, credit), `RecordID` = poziție `1..n` în ordinea `(Data, NumarNota,
  Id)`; `AccountID` = `SimbolSaft`; `CustomerID/SupplierID` ale liniei = din
  dimensiunea `Repartitor` a LATURII dacă e `Partener` și contul laturii are
  `RolTert` (Client ⇒ `CustomerID`, Furnizor ⇒ `SupplierID`; cealaltă =
  societatea), altfel ambele = societatea; `Description` = a liniei sursă
  (`Descriere` unde există) sau a tranzacției; `Debit/CreditAmount` cu semn
  (storno negativ, 46a), `CurrencyCode` `RON`, `CurrencyAmount = Amount`,
  fără `ExchangeRate`; `TaxInformation`: rândul al cărui cont e contul de TVA al
  unui `TipTva` (`ContTvaDeductibil/Colectat/Neexigibil`) ȘI cu `DetaliuId`
  prezent în `RegistruTva` ⇒ `300` + codul direcțional (`Sens` din registru) +
  `TaxPercentage` (cota snapshot) + `TaxAmount` = `Tva` al rândului; orice alt
  rând ⇒ `000/000000`, `TaxAmount 0`. Rândurile de deschidere (`DocumentId`
  null) NU intră în GL (sunt în solduri). `NumberOfEntries` = nr. tranzacții,
  `TotalDebit/TotalCredit` = Σ semnate.
- **SalesInvoices / PurchaseInvoices**: FCL + RDC ⇒ vânzări; FCT + RLF ⇒
  cumpărări; un `Invoice` per document OPERAT/STORNAT (stornoul = factură
  proprie cu valori negative, `381`, ca la D394 — `nrFact` pe (Document ×
  Storno)); `InvoiceNo` = `Numar`; `AccountID` = contul cu `RolTert` de pe
  rândurile contabile ale documentului (lipsă ⇒ avertisment + `Neincluse`);
  `CustomerInfo/SupplierInfo` = `IdPartener` + `BillingAddress`; liniile =
  `DocumentDetaliu` ale frunzei, `LineNumber` = poziție în `Id` crescător
  (fără număr persistat — stabil între două generări, asumat); `AccountID`
  al liniei = contrapartida din rândul contabil cu `DetaliuId` = linia (lipsă
  ⇒ contul TVA al tipului? NU: `Neincluse`); `Quantity` = |`Cantitate`| (linia
  de venit a RDC: cantitatea ei); `UnitPrice` = `Valoare/Cantitate` rotunjit la
  bani sau `PretUnitar` unde frunza îl are; `InvoiceLineAmount` = `Valoare`
  (net); `DebitCreditIndicator` = `C` vânzare / `D` cumpărare, semnul poartă
  stornoul; `TaxPointDate` = `Data`; `TaxInformation` din `RegistruTva` pe
  `DetaliuId` (fără rând ⇒ `000/000000`); `Description` = `Descriere` sau
  `Produs.Denumire` sau `TipMaterial.Denumire`; `InvoiceDocumentTotals`:
  `NetTotal` = Σ `Valoare`, `GrossTotal` = Σ (`Valoare`+`ValoareTva`),
  `TaxInformationTotals` per cod. Linia de COST a RDC (fără `TipTva`, 68) nu
  e linie de factură — rămâne în GL. `Neincluse` = documentele care nu pot
  fi emise, cu cauza.
- **Payments**: PLT + INC OPERATE cu contrapartida `Partener` sau `Angajat`
  (viramentele 581 și laturile `ContPropriu↔ContPropriu` rămân doar în GL);
  `PaymentRefNo` = `Numar`; `TransactionDate` = `Data`; `PaymentMethod` +
  `PaymentMechanism` din `TipInstrumentPlata`; `Description` = tip + număr;
  linia = `DocumentTrezorerieDetaliu`: `AccountID` = contul contrapartidei
  (rândul contabil cu `DetaliuId`), `CustomerID/SupplierID` = partenerul pe
  rolul contului (angajatul ⇒ ambele = societatea + avertisment
  `PlataCatreAngajat`), `DebitCreditIndicator` = `D` plată / `C` încasare,
  `PaymentLineAmount` = `Valoare`, `TaxInformation 000/000000`,
  `SourceDocumentID` = `Numar`-ul documentului stins DACĂ imperecherea e unică
  pe document (altfel omis); `GrossTotal` = `Total`.
- **Cusăturile (contract)**: Σ `DebitAmount` == Σ `CreditAmount` == Σ
  `RegistruContabil.Valoare` pe perioadă (semnat); Σ `TaxAmount` pe
  `TransactionLine` == Σ `RegistruTva.Tva`; Σ `NetTotal` pe facturi + Σ
  `Neincluse` (facturi) == Σ `RegistruTva.Baza` per sens pe tipurile FCT/FCL/
  RLF/RDC; fiecare `CustomerID`/`SupplierID` de pe facturi/plăți există în
  `Customers`/`Suppliers`; `Closing` din GLA == `Balanta` la cent. Nimic nu se
  pierde: orice document/linie neemis(ă) e în `Neincluse` cu cauză.
- Ce NU face: nu rotunjește altfel decât la 2 zecimale (bani exacți, 71g), nu
  persistă declarația (35c), nu paginează, nu filtrează `Storno`.

### D16-D5 — Fișierul: `SaftXml` streaming + oracolul DUK

- `Module/Saft/SaftXml.cs`: `XmlWriter` peste `Stream`, namespace producție,
  numele elementelor = **ale validatorului** (`TotalSegmentsInSequence`,
  `UOMToUOMPhysicalStockConversionFactor` etc. — lista din structura §C.1),
  ordinea = a schemei, zecimale cu punct/2 zecimale (`InvariantCulture`),
  fără diacritice? **NU se strică textul**: XML UTF-8 acceptă diacritice;
  legacy le elimina fără motiv documentat — se măsoară în V3 (DUK cu
  diacritice); dacă DUK le refuză, se normalizează cu avertisment.
  `xs:choice`-urile din §C.4 respectate (debit XOR credit; opening debit XOR
  credit; `TaxPercentage`, nu `FlatTaxRate`).
- **Bugetar** ⇒ proiecția întoarce `Neaplicabil` cu motiv (planul instituțiilor
  publice nu e printre bazele D406), 422 pe REST, fără fișier.
- `tools/ModelCheck/Duk.cs`: rulează kitul local (`-d`, offline, `an=/luna=`)
  DACĂ `java` din kit există; citește `.err.txt` (`ok` ⇔ valid) și `.wrn.txt`
  (prefix `!`); avertismentele DUK se listează, nu blochează (C.3). Ancora de
  versiune (J2.2.8) se scrie în §Închidere; actualizarea kitului = acțiunea
  utilizatorului, nu a suitei.
- REST (`SaftController`, `api/proiectii/saft`): `GET ?an&luna` ⇒ `SaftDto`
  (JSON — rezumat + avertismente + `Neincluse`, secțiunile întregi; formularul
  nu se paginează); `GET xml?an&luna` ⇒ `application/xml`, `Content-Disposition`
  `SAF-T_{CUI}_{an}-{luna}.xml`, generat streaming pe ușa SECURED — `User` fără
  drept pe `RegistruContabil` ⇒ JSON gol (200, ca D394) dar **XML ⇒ 403**
  (un fișier gol semnat cu CUI-ul e o declarație falsă, nu o listă goală).
  Perioada obligatorie, `an ≥ 2020` (`PeriodYear` minInclusive) ⇒ altfel 400
  `EroriDto`. Codegen + `verifica:drift`.

### D16-D6 — Client `/saft` și Import1C

- React `src/felii/tva/Saft.tsx`, rută `/saft`, perioada în URL; rezumat per
  secțiune (număr de intrări, totaluri), avertismentele pliate, `Neincluse`,
  buton „Descarcă XML" = link `<a href>` la endpoint-ul de fișier (fără
  buffer în TS; token-ul JWT: dacă descărcarea prin link nu poartă header-ul
  de autorizare, se folosește `fetch` + `blob` — se decide la implementare și
  se scrie în §Închidere). Ecranul `Societate` = XAF (React = restanță cu
  nume, ca 72-r9). Produs: `CodNc` + `UnitateMasura` pe ecranul OData existent.
- Import1C: `Societate` din `flax.Organizatii` (+ adresă `InfoRg_InformatiaDeContact`
  cu `Object_Organizatii_ID`, IBAN din `ConturiBancare` proprii) pe câmp gol;
  `Produs.UnitateMasuraId` din `Nomenclator.UM` prin dicționarul RO + codurile
  UN/ECE exacte (contor per nerezolvat); `CodNc` din `Nomenclator.NIC` dacă e
  8 cifre. Flag nou `--saft an luna` = generează pe clona Flax fișierul lunii
  și rulează DUK, cu raport per cauză (V5).

### D16-D4 — amendamente din pasul 2 (măsurate pe scenă, 2026-08-26)

- **Partenerul se citește de pe RÂND, nu de pe latură** (constatarea 64h,
  confirmată): convenția `RepartitorImplicitDebit/Credit` pune pe fiecare
  latură CONTRAPARTIDA (pe `628 = 401` furnizorul e pe DEBIT). Regula legii
  rămâne (rolul e al CONTULUI): pentru rândul contabil, `Customer/SupplierID`
  se decid după `RolTert` al contului cu rol, iar partenerul = cel de pe rând
  (latura lui dacă e `Partener`, altfel cealaltă) — un singur loc,
  `PartenerulRandului`. Consecință: `Customers/Suppliers` NU ies din
  `Balanta(analitic)` (cheia ei e cont × dimensiunea ACELEIAȘI laturi), ci
  dintr-un agregat propriu pe (pereche de conturi × pereche de repartitori)
  filtrat pe conturile cu rol. Decizia de model 64h rămâne deschisă.
- **Liniile de STOC ale FCT** n-au rând contabil propriu (recepția contează pe
  NIR, 26a) ⇒ `InvoiceLine.AccountID` nu are sursă pe factură. NU se
  inventează un cont (nici `TipMaterial.ContImplicit`): contrapartida se ia
  din realitatea MATERIALIZATĂ a conexului — linia FCT naște lotul
  (`Lot.LinieIntrareId`), NIR-ul conex îl recepționează (`RegistruStoc.LotId`
  → `DetaliuId` al NIR-ului → `RegistruContabil.DetaliuId` → contul de DEBIT).
  Fără rând de NIR pe lot ⇒ `Neincluse/FaraContrapartida` (pasul 3).
- **Cusătura TVA are trei termeni**: Σ `TaxAmount` GL + TVA capitalizat (fără
  rând contabil de TVA) + taxa tipurilor FĂRĂ cod SAF-T (iese `000000`) ==
  Σ `RegistruTva.Tva`.
- **Riscul 3 decis**: `ContTvaNeexigibil` (4428) ⇒ `000/000000` — nu e taxă
  exigibilă. **Riscul 2 decis**: două nomenclatoare cu același identificator ⇒
  o intrare cu soldurile CUMULATE + `PartenerDublat`. Partenerul cu factură
  operată ȘI stornată în lună (rulaj net 0) se declară cu solduri zero —
  fișierul îl referă. Prefixul `00` cere CUI VALID (altfel `04` + avertisment
  `PartenerFaraCuiValid`), ca `01` să rămână accesibil străinilor
  înregistrați. `AdresaIncompleta` se numără per partener.
- **`Cont.Functie` e GOL pe planul privat** ⇒ `AccountType` iese
  `Bifunctional` peste tot: gaură de DATE, se închide în pasul 3 cu coloana
  `Functie` (A/P/B) în resursa `plan-conturi-omfp.csv` a seed-ului privat, din
  anexa OMFP 1802 (funcția e a legii, nu a clientului). Bugetar: neatins.
- D4-r14 făcut: `FaraOp11` doar pe liniile fără produs cu `CodNc`; categoria
  de bunuri/structura `op11` rămân restanța D4-r5.

### D16-D5/D6 — amendamente din pasul 4 (măsurate pe clona Flax.Api, 09/2025)

- **JSON-ul întreg NU e un formular**: 38,6 MiB pe lună (3.950 clienți, 13.966
  tranzacții × ~4 linii, 3.000 plăți) — „formularul nu se paginează" (69/71) e
  contrazis de date. Decizie: `GET api/proiectii/saft` servește un **SUMAR**
  (`SaftSumarDto`: antet, contoare + totaluri per secțiune, cusăturile,
  `Neincluse`, avertismentele) — nu listele; listele trăiesc DOAR în fișier
  (`saft/xml`) și în `SaftProiectii.Saft()` pentru ModelCheck. Ecranul `/saft`
  consumă sumarul (pasul 4b).
- XML-ul se scrie streaming pe `Response.Body` cu `AllowSynchronousIO` per
  cerere (Kestrel refuză scrierile sincrone; bufferarea ar ține luna de două
  ori în RAM; `SaftXml` rămâne sincron — Import1C scrie același fișier pe disc).
  Răspuns chunked, fără `Content-Length`. 403-ul pe fișier = `CanRead` pe TIPUL
  `RegistruContabil`, aditiv în `ContaApiController` (`PoateCiti`), gate-ul
  comenzilor neatins.
- **Normalizarea CUI e repetată și insensibilă la caz** (`RORo1853162` ⇒
  `1853162`): singura cauză de respingere DUK pe fișierul real (2 parteneri din
  5.536). Pasul 4b în `D394Proiectii.NormalizeazaCui` (o singură sursă) +
  contor în Import1C.
- **Serializarea JSON a `SaftDto` se probează în ModelCheck** (coliziunea
  `PartenerID`/`PartenerId` a dat 500 până la V4; nimic din suită nu
  serializa).
- `[Produces("application/xml")]` NU se folosește (impune content-type-ul și pe
  `EroriDto` ⇒ 406); content-type-ul de succes se pune manual.

## Ce NU intră, cu motiv

- **Declarațiile S și A** (MovementOfGoods/PhysicalStock/Owners — cere
  `MovementType` per tip × `TipStoc` ca politică și `OwnerID`; Assets = modul
  separat, decizia 9). Restanță cu nume: „SAF-T S peste `RegistruStoc`".
- **Trimestrial (`T`)**, **rectificativă (`R`)**, **nerezidenți**,
  **segmentarea** (pragul nu e în nicio sursă; un singur segment; fișierul mare
  se măsoară în V5).
- **Multi-valută**: totul RON; FCT cu `Valuta ≠ RON` ⇒ avertisment
  `FacturaInValuta` (34g deschis).
- **WHT**, `Taxonomies` (niciodată raportată), `TaxRegistration` cu
  `100030` (radierea, 72-r4/D4-r1 — modelul are `InactivFiscal`, care NU e
  „anulat ex officio").
- **Contactul partenerilor / IBAN-ul partenerilor** (34g) — `Contact` și
  `BankAccount` sunt opționale pe `CompanyStructure`.
- **Nomenclatorul NC8 seed-uit** (9.984 coduri anuale) — validarea e a DUK.
- **`LineNumber` persistat** pe `DocumentDetaliu` — ordinea pe `Id` e stabilă
  între generări; se persistă doar dacă V5 arată nevoia.
- **Actul normativ** (OpANAF 1783/2021: termene, praguri, grație) — nu e în
  repo; nu afectează structura; se notează ca restanță documentară.

## Riscurile pin-uite (ținta review-ului advers)

1. `CustomerID/SupplierID` pe linia de GL: partenerul pe latura de DEBIT a unui
   cont Furnizor (plata unei datorii) trebuie să iasă `SupplierID`, nu
   `CustomerID` — rolul e al CONTULUI, nu al laturii.
2. Un partener pe un cont Client ȘI pe unul Furnizor ⇒ două intrări (Customer +
   Supplier) cu același `RegistrationNumber` — legal; dar același partener de
   DOUĂ ori în `Customers` (două conturi Client) ⇒ cheie duplicată ⇒ una singură.
3. `TaxInformation` pe rândul TVA: `ContTvaNeexigibil` (4428) există pe
   `TipTva` — rândul 4428 primește cod, deși nu e taxă exigibilă; se decide în
   V2 (probabil `000000` pe neexigibil, cu avertisment).
4. Rândul contabil fără `DetaliuId` (TVA-ul pe document? ITV — `380200`
   închiderea de TVA e cod DOAR pentru GL): ITV ⇒ `TaxCode 380200`
   (`TVA_NoteContabile`) pe rândurile 4426/4427 = 4423/4424 — se pin-uiește
   prin `PoliticaInchidereTva` (FK-uri, nu simbol).
5. `AccountID` cu simbolul fără puncte poate depăși „6 cifre" (J2.2.5) — se
   măsoară cu DUK pe planul privat (`4111` vs `411.01`?).
6. `ExchangeRate` absent cu `CurrencyCode=RON`: dacă DUK cere `Amount ==
   CurrencyAmount × ExchangeRate` cu `ExchangeRate` tratat ca 0 ⇒ fișierul
   pică; se măsoară în V3, fallback `ExchangeRate 1`.
7. `TotalSegmentsInSequence > SegmentIndex` (mesajul spune „mai mare") pe
   `1/1` — se măsoară.
8. `ProductCommodityCode = 0` acceptat de DUK (legacy în producție) — se
   măsoară; dacă nu, `00000000`? NU: se raportează și se cere decizia.
9. Diacriticele în XML — se măsoară (risc: DUK le acceptă, dar validatorul
   ANAF online nu; legacy le scotea).
10. `Storno` negativ pe `InvoiceLineAmount` cu `DebitCreditIndicator`
    neschimbat — conform S.I.47 („signing relative to indicator"); dar
    `Quantity` negativ? — se emite |cantitate| și semnul pe sume.
11. Perf: luna pe baza Flax (≈ zeci de mii de rânduri de registru) — proiecția
    în memorie trebuie să rămână sub câteva secunde; streaming-ul XML nu ține
    string-ul întreg.
12. Gardianul `Societate` unic + seed care nu rescrie: `--forceUpdate` pe o
    bază cu societate completată NU o golește.

## Verificări (ModelCheck, ambele profiluri; DUK offline; HTTP pe clona Flax.Api)

- **V1 modelul**: `Societate` unic (al doilea ⇒ refuz), lungimile de adresă ==
  ale lui `Partener`, `JudetId` cu `Tara ≠ RO` ⇒ refuz, `BazaContabila` în
  afara listei ⇒ refuz; `UnitateMasura` seed = 2.162, `H87`/`KGM`/`LTR`
  prezente; `Produs.CodNc` `1234567` ⇒ refuz; `Cont.RolTert` seed privat pe
  `411`/`401` (prin simbol în PROBĂ, nu în cod); bugetar: zero `RolTert`.
- **V2 regulile + proiecția** (privat, scenă cu FCT+NIR, FCL+DSC, RDC, RLF,
  PLT cu imperechere, INC, NTC, un storno, un partener UE neînregistrat, unul
  PF fără CNP, un angajat pe 461): `IdPartener` pe cele 6 cazuri; `InvoiceType`
  380/381; `MetodaPlata` tuplele; cusăturile din D4 la cent; un partener în
  ambele liste; `Neincluse` cu cauze; avertismentele agregate (număr + exemple).
  Bugetar: `Neaplicabil`.
- **V3 fișierul**: XML-ul scenei trece DUK (`ok`) — cu diacritice, fără
  `ExchangeRate`, cu `CodNc=0`, cu `1/1`; fiecare din riscurile 5–9 are
  rezultatul scris în §Închidere. Dacă `java` lipsește, proba se sare
  ZGOMOTOS (linie în rezumat), nu tăcut.
- **V4 HTTP** (clona Flax.Api): `User` ⇒ JSON 200 gol + XML 403; `Admin` ⇒ 200
  + fișier; `an=2019` ⇒ 400; bugetar (`BackOffice` implicit) ⇒ 422; timp de
  răspuns JSON și XML pe 09/2025 (ms).
- **V5 Import1C pe clona Flax**: `Societate` din `Organizatii`; contoarele UM
  (rezolvate/nerezolvate) și `CodNc`; `--saft 2025 9` ⇒ DUK: `ok` sau erorile
  grupate per cauză; cusăturile D4 pe luna reală; dimensiunea fișierului și
  timpul; `reconciliere-*.txt` neatins (nu se re-importă documente — dacă
  migrația/seed-ul obligă `--recreeaza`, raportul trebuie IDENTIC cu
  baseline-ul).
- `verifica:drift` idempotent; `--dump-metadata` comis; ModelCheck verde pe
  ambele profiluri.

## Regula de oprire

Agentul se oprește și raportează (nu normalizează tăcut) dacă:
- DUK refuză fișierul scenei pe o regulă care cere schimbare de MODEL (nu de
  scriere) — se aduce mesajul brut;
- o cusătură din D4 nu bate la cent pe scenă;
- `Societate` sau `UnitateMasura` cer atingerea `Partener`/`Produs` dincolo de
  cele două coloane noi;
- migrația atinge altceva decât `Societati`, `UnitatiMasura`, `Produse`
  (2 coloane), `Conturi` (`RolTert`);
- pe Flax, `--saft` cere re-import și raportul de reconciliere diferă de
  baseline cu o linie;
- ModelCheck pică pe un profil; `verifica:drift` nu e idempotent.

**Explicit NU în regula de oprire**: avertismente DUK, `Neincluse` pe date
reale, `FaraCodNc`/`FaraUnitateMasura` în masă pe Flax — se raportează cu
cifre.

## Închidere (2026-08-26, după fix-urile review-ului advers)

- [x] Pașii 1–5 + 4b comise, fiecare verificat independent; pasul 6a =
  fix-urile review-ului (14, toate aplicate). ModelCheck final: **bugetar 752,
  privat 530**, verde; migrația `20260826083001_F16SaftModel` pe 6 baze.
- [x] **DUK J2.2.8 (kit `1.4.18.3.3`)** — ancora de versiune a feliei;
  publicat J2.2.15 (73-r8). Fișierul scenei ⇒ `ok`, 0 atenționări.
  Fișierele REALE Flax: **09/2025 `ok`** (71,5 MiB după fix-uri), **12/2025
  `ok`** (71,6 MiB), 0 atenționări.
- Riscurile, MĂSURATE cu oracolul: (5) `AccountID` de 7 cifre TRECE; (6)
  `ExchangeRate` absent pe RON TRECE (nu se emite); (7) `1/1` TRECE; (8)
  `ProductCommodityCode = 0` TRECE (`0`, `00000000`, `99999999` sunt literal în
  NC8 al jar-ului); (9) diacriticele TREC (nu se despiacritizează); `ONGE`
  TRECE; `04` cu cratimă RESPINS (corect); codul NC e validat contra NC8 al
  anului (`12345678` respins, `01012100` acceptat); Grecia: `01EL`/`01GR`,
  `Country EL`/`GR` TREC toate — alegerea ISO `GR` e a noastră
  (`SaftReguli.CodTaraSaft`, o singură grafie per țară, identificator + adresă);
  `TotalSegmentsInsequence` cu `s` mic (grafia XSD; cu `S` ar fi picat). (10)
  cantitatea negativă nemăsurată (73-r11). (11) perf pe Flax 09/2025:
  proiecție 4,3–4,5 s, XML 0,4 s, DUK 3,5 s; REST JSON (sumar) — întregul era
  2,4–3,4 s / 38,6 MiB, XML 3,4–6,4 s; 12 luni în 28,3 s. (12) seed-ul nu
  golește `Societate` completată — probat.
- **V4 (clona Flax.Api, HTTP real)**: `User` JSON 200 gol / XML **403**;
  `Admin` 200 + fișier `SAF-T_14639030_2025-09.xml`; `an=2019`, `luna=13`,
  lipsă, `an=abc` ⇒ 400 `EroriDto`; bugetar ⇒ 422 (JSON și XML); smoke
  vizual `/saft`. Nemăsurat pe HTTP: 422 pe CUI gol/invalid (F7, probat la
  nivelul scriitorului) — 73-r19.
- **V5 (baza Flax de reconciliere)**: registrele identice la rând,
  `reconciliere-*.txt` SHA neschimbat; `--societate` 10 câmpuri; `--um-nc`
  18.642 `CodNc` / 5 UM nerezolvate (`ml`×4, `pac`×1) / 1 TARIC de 10 cifre;
  **09/2025 după fix-uri**: 13.966 tranzacții / 52.932 linii GL, 3.950
  clienți / 228 furnizori, 3.700 facturi emise (9.459.762,04 / 10.987.556,17),
  **1.772 primite (8.191.889,46 / 9.869.562,07 — era 1.688 înainte de L1)**,
  3.000 plăți (17.401.707,14), 2.554 produse; `Neincluse` **100 ⇒ 16**
  (`ContFaraRol` 84 ⇒ 0); cusăturile 1–6 la cent: partidă dublă
  80.527.820,95; TVA 3.238.255,54 + 0 − 32.788,80 = 3.205.466,74; achiziții
  7.981.303,47; livrări 7.266.639,45; **per cont 99 verificate / 0 diferite,
  Σ|closing| 41.384.123,58**; terți clienți 79.887,56 + 5.296.545,71 =
  5.376.433,27; furnizori −1.905.584,14 − 7.119.441,10 = −9.025.025,24;
  master files 0 orfani. Avertismente: `TertFaraPartener` 473 (1.676.955,21),
  `PartenerFaraCuiValid` 412, `FaraCodNc` 195, `AdresaIncompleta` 49,
  `PartenerDublat` 16, `TipTvaFaraCodSaft` 2 (−32.788,80: N19/TI19).
- `verifica:drift` idempotent; `--dump-metadata` comis.

**Ce a scos review-ul advers — 0 blocante, 3 `[lege]`, 12 `[fond]`, 4
`[cosmetic]`** (decizia 73 §j): L1 FCT all-stock (TI) lipsea din
`PurchaseInvoices` — 84 facturi/lună, 3,37 M bază, recuperate; L2 Capitalizat
brut; L3 stornoul fără data lui; F1 plățile stornate pozitive de două ori; F3
cusăturile 1/4 vide; F4 cusătura terților lipsea; F5 DEC/NTC fără urmă; F6
`AccountID` gol pe terț; F7 fișier fără CUI = 200; F8 `SeedRolTert` peste
conturile manuale; F2 pin-uit (latura proprie); F10 măsurat. Toate fixate în
pasul 6a, cu +9 probe.

**Rămase, ne-blocante**: 73-r1…r19 (decizia 73 §k; r19 = 422 pe CUI
gol/invalid nemăsurat pe HTTP, `--cititori` C4 nemăsurat pe Flax).

## Pașii (un agent per pas, verificare independentă + commit după fiecare)

1. **Model + seed**: D1, D2, `Cont.RolTert` (D3, partea de date), migrație pe
   toate bazele, gardieni, layout XAF, OData, `--dump-metadata`, V1.
2. **Reguli + proiecție**: `SaftReguli`, `SaftProiectii` → `SaftDto`, `Neincluse`,
   avertismente, D4-r14; V2 (scena + cusăturile), bugetar `Neaplicabil`.
3. **XML + DUK**: `SaftXml`, `Duk.cs` în ModelCheck, V3 cu rezultatele
   riscurilor 5–9 scrise.
4. **REST + client**: `SaftController` (JSON + XML), codegen, `/saft`, V4.
5. **Import1C**: `Societate`/UM/NC din 1C, `--saft`, V5 pe clona Flax (rulare
   detașată + monitor).
6. **Închidere**: review advers, fix-uri de main, decizia 73, CLAUDE.md §73 +
   restanțele cu nume (SAF-T S, `T`, segmentare, NC8 nomenclator, `Produs.UM`
   string, ecranul React `Societate`, OpANAF 1783/2021), README jurnal,
   istoric, `lista-react.md`, memoria de handoff.
