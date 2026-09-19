# Inventar D406 (SAF-T) — ce citesc secțiunile reale

Pas 1 al designului `docs/nucleu/nucleu-cub-design.md`. Inventar, fără propuneri.

Prescurtări de fișier (toate relative la rădăcina repo-ului):

| Prescurtare | Fișier |
|---|---|
| `SP` | `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Saft/SaftProiectii.cs` |
| `SR` | `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Saft/SaftReguli.cs` |
| `SD` | `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Saft/SaftDto.cs` |
| `SX` | `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Saft/SaftXml.cs` |
| `SC` | `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.WebApi/API/Conta/SaftController.cs` |
| `CP` | `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Proiectii/ContabilProiectii.cs` |
| `TP` | `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Proiectii/TvaProiectii.cs` |
| `SS` | `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Motor/SolduriService.cs` |
| `CT` | `nou/Atlas.Conta.BackOffice/Atlas.Conta.BackOffice.Module/Api/CititorTipDocument.cs` |

---

## 0. Identitate (comună tuturor secțiunilor)

- **Nume / cod**: D406 SAF-T, două module: **L** (lunar) și **C** („la cerere" =
  stocuri, numit în cod „S").
- **Proiecții**: `SaftProiectii.Saft` (L) `SP:225`; `SaftProiectii.SaftStocuri`
  (C) `SP:1610`. Ambele produc ACELAȘI `SaftDto` (`SD:684`), cu alt set de liste
  populate.
- **Reguli pure**: `SR` (identități de terț, coduri de taxă, tip de cont, metodă
  de plată, nomenclatorul codurilor de mișcare, `MovementReference`).
- **Scriitor**: `SX` (XML streaming, `SX:152` și urm.).
- **Uși**: DOAR REST, patru rute (`SC:63`, `SC:77`, `SC:113`, `SC:124`):
  `GET api/proiectii/saft` și `…/saft/stocuri` întorc `SaftSumarDto` (contoare +
  cusături + `Neincluse` agregat, funcție PURĂ pe DTO `SP:145`);
  `…/saft/xml` și `…/saft/stocuri/xml` întorc fișierul. Fără OData, fără ecran
  XAF, fără paginare (`SC:28`), fără cache (`SC:49`).
- **Perioadă**: obligatorie, o LUNĂ (`an`+`luna`, `SC:242`); `DataStart` =
  ziua 1, `DataEnd` = ultima zi (`SP:229`, `SP:1614`).
- **Refuz de domeniu**: profil bugetar ⇒ `Neaplicabil` ÎNAINTEA oricărei
  interogări pe registre (`SP:238`, `SP:1619`, `MotivNeaplicabil` `SP:2662`,
  citit din `SetareProfil.Profil`).

**Ce NU există în lot**: `Owners` și `Assets` se scriu ca tag-uri GOALE
(`SX:430`–`SX:432`, comentariul `SX:34`), iar `AssetTransactions` nu e emis
deloc. Grep pe `Saft/` nu găsește nicio referință la `RegistruImobilizari`,
`Imobilizare` sau `SoldPerioada*` — felia 26 (imobilizări) NU alimentează D406.

---

## 1. Header

**Granul rândului**: un rând per declarație (un singur `Societate`,
`FirstOrDefault` `SP:2692`).

| Câmp | Sursă | Fel | Fișier:linie |
|---|---|---|---|
| `AuditFileVersion` | constanta `"2.0"` | calcul | `SP:56`, `SP:2736` |
| `AuditFileCountry` | constanta `"RO"` | calcul | `SP:57` |
| `AuditFileRegion` | `Societate.Judet.Cod` | nomenclator | `SP:2738`, `SP:2687` |
| `AuditFileDateCreated` | `DateTime.Today` (parametru doar pentru probe) | calcul | `SP:2739` |
| `SoftwareCompanyName` / `SoftwareID` | constante `"Atlas"` / `"Atlas.Conta"` | calcul | `SP:50`–`SP:51` |
| `SoftwareVersion` | `AssemblyInformationalVersion`, tăiat la 18 | calcul | `SP:2742`, `SP:2980` |
| `DefaultCurrencyCode` | constanta `"RON"` | calcul | `SP:58` |
| `HeaderComment` | `"L"` (lunar) / `"C"` (stocuri) | calcul | `SP:52`, `SP:55` |
| `SegmentIndex` / `TotalSegmentsInSequence` | constante `"1"` | calcul | `SP:2745` |
| `TaxAccountingBasis` | `Societate.BazaContabila` (fallback `Societate.BazaContabilaImplicita`) | nomenclator | `SP:2747` |
| `RegistrationNumber` | `SR.RegistrationNumberSocietate(CodFiscal, Tara, InregistratTva)` | nomenclator + calcul | `SP:2750`, `SR:176` |
| `Name` | `Societate.Denumire` | nomenclator | `SP:2751` |
| `Address.*` | `Societate.Strada/Numar/DetaliiAdresa/Localitate/CodPostal/Judet.Cod/Tara` | nomenclator | `SP:2752`–`SP:2760` |
| `ContactFirstName` / `ContactLastName` | `Societate.ContactPrenume` / `ContactNume` | nomenclator | `SP:2761` |
| `Telephone` / `Email` | `Societate.Telefon` / `Email` | nomenclator | `SP:2763` |
| `IBANNumber` | `Societate.ContBancar.Iban` | nomenclator | `SP:2765`, `SP:2689` |
| `BankAccountNumber` | `Iban ?? ContBancar.Cod` | nomenclator + calcul | `SP:2766` |
| `PeriodStart` / `PeriodEnd` | parametrul `luna` (identic pe ambele) | calcul | `SP:2767`–`SP:2770` |
| `PeriodStartYear` / `PeriodEndYear` | parametrul `an` | calcul | `SP:2768` |

**Filtre / agregare**: niciuna — antetul nu citește registre.

**Ce citește din DOCUMENT**: nimic.

**Cazuri speciale**:
- `City` gol ⇒ literalul `"Nespecificat"` (`SP:2756`, `SP:66`).
- `Region` se emite DOAR pe `Country == "RO"` (`SP:2758`).
- `VerificaSocietate` (`SP:2706`) emite `SocietateIncompleta` pentru CodFiscal,
  Denumire, Localitate, Judet, ContactNume, Telefon, Iban lipsă; CUI care nu
  trece cifra de control (`SR.CuiValid`, `SR:261`) e avertisment, nu refuz.
- Ușa FIȘIER refuză cu 422 fără CUI sau cu CUI invalid (`SC:166`–`SC:174`);
  ușa JSON rămâne 200. Numele fișierului se construiește din CUI cu `RO` tăiat
  (`SC:229`).

---

## 2. MasterFiles → GeneralLedgerAccounts

**Granul rândului**: **cont sintetic × perioadă** (un rând per `Cont`, cheia
balanței sintetice; `analitic: false`, deci FĂRĂ repartitor). `SP:2790`.

**Sursa de bază**: `ContabilProiectii.Balanta(os, dataStart, dataEnd,
analitic: false)` `CP:320` → `SolduriService.AtomiCumulati` `CP:331` /
`SS:226`. Aceasta e SINGURA secțiune care trece prin SNAPSHOT: dacă există o
perioadă de referință, soldul pornește din `SoldPerioadaContabil` (`SS:231`) și
se concatenează doar atomii de după ea (`SS:247`); fără referință, e registrul
întreg.

| Câmp | Sursă | Fel | Fișier:linie |
|---|---|---|---|
| `AccountID` | `SR.SimbolSaft(Cont.Simbol)` (doar alfanumerice) | nomenclator | `SP:2805`, `SR:235` |
| `AccountDescription` | `Cont.Denumire` | nomenclator | `SP:2806` |
| `AccountType` | `SR.TipCont(Cont.Functie)`: D⇒`Activ`, C⇒`Pasiv`, rest⇒`Bifunctional` | nomenclator + calcul | `SP:2807`, `SR:243` |
| `OpeningDebitBalance` / `OpeningCreditBalance` | `InitialDebit − InitialCredit`, despicat pe semn | snapshot + registru | `SP:2794`, `SP:2809` |
| `ClosingDebitBalance` / `ClosingCreditBalance` | deschidere + `RulajDebit − RulajCredit`, despicat pe semn | snapshot + registru | `SP:2795`, `SP:2811` |

**Filtre și chei de agregare**:
- Granița de agregare e `dataStart`: `Initial*` = `Data < dataStart`,
  `Rulaj*` = `Data >= dataStart` (`CP:383`–`CP:386`). `Data` = `RegistruContabil.Data`.
- Rândurile de STORNO intră (registru append-only, suma algebrică e adevărul,
  `CP:307`).
- Rândurile de DESCHIDERE (`DocumentId == null`) intră (`CP:309`).
- Se sar conturile cu toate cele patru cifre zero (`SP:2796`).
- JOIN-ul pe `Cont` e LEFT — un atom nu se pierde dacă i-a murit eticheta
  (`CP:362`–`CP:373`).

**Ce citește din DOCUMENT**: nimic.

**Cazuri speciale**:
- `xs:choice` debit XOR credit; zero se declară pe DEBIT (`SP:2808`).
- `Functie` diferită de D/C/B ⇒ `Bifunctional` + avertisment
  `TipContNecunoscut` (`SP:2799`).
- Cusătura 4: `Closing` per cont din fișier se compară cu balanța recalculată,
  cont cu cont; diferențele se numără în `ConturiDiferite` (`SP:1488`–`SP:1500`).

---

## 3. MasterFiles → Customers / Suppliers

**Granul rândului**: **partener × rol** (Client sau Furnizor). Conturile
rolului se ÎNSUMEAZĂ într-un singur rând (`SP:522`, `SP:525`).

**Sursa de bază**: agregat PROPRIU peste `RegistruContabil`, nu balanța și NU
snapshotul (`SP:388`–`SP:397`): `Data <= dataEnd`, cont de debit SAU de credit
cu `RolTert != Niciunul`, grupat pe
`(ContDebitId, ContCreditId, DebitRepartitorId, CreditRepartitorId)`, cu
`Initial = Σ(Data < dataStart)` și `Rulaj = Σ(Data >= dataStart)`. Deci scanare
pe TOT istoricul, fără snapshot — spre deosebire de `GeneralLedgerAccounts`.

**Cum se alege partenerul** (`PartenerulRandului`, `SP:469`): repartitorul
laturii PROPRII dacă e un `Partener`; altfel repartitorul celeilalte laturi;
altfel `null`. Rolul vine de la CONTUL laturii (`Cont.RolTert`, `SP:293`),
partenerul de pe RÂND — motivul e scris în antetul clasei (`SP:29`–`SP:46`):
convenția 00 §5 pune pe fiecare latură contrapartida, nu titularul contului.

| Câmp | Sursă | Fel | Fișier:linie |
|---|---|---|---|
| `Id` / `RegistrationNumber` | `SR.IdPartener(TipPersoana, Tara, InregistratTva, CodFiscal, Cod, ID, RaporteazaCnp)` → prefix `00`/`01`/`02`/`03`/`04`/`05`/`06` | nomenclator + calcul | `SP:435`, `SR:53`–`SR:107` |
| `Name` | `Partener.Denumire` | nomenclator | `SP:543` |
| `Address.*` | `Partener.Strada/Numar/DetaliiAdresa/Localitate/CodPostal/Judet.Cod/Tara` | nomenclator | `SP:260`–`SP:279`, `SP:424` |
| `TaxRegistrationNumber` | `Id406[2..]`, DOAR pe `FelIdSaft.CuiRoman` | calcul | `SP:545` |
| `TaxType` | `SR.RegimFiscalPartener` → `100010` / `100040` (TVA la încasare) / `null` | nomenclator + politică | `SP:546`, `SR:192` |
| `AccountID` | contul rolului cu cea mai mare `Miscare` (tie-break: `|Net|`, apoi simbol ordinal) | registru + calcul | `SP:533`–`SP:537` |
| `OpeningDebitBalance` / `OpeningCreditBalance` | `Σ NetInitial` per partener × rol, despicat pe semn | registru | `SP:528`, `SP:548` |
| `ClosingDebitBalance` / `ClosingCreditBalance` | `Σ Net` (= initial + rulaj), despicat pe semn | registru | `SP:529`, `SP:550` |
| `FelId` (doar JSON, nu XML) | enum-ul `FelIdSaft` al identității | calcul | `SP:552` |

**Filtre și chei de agregare**:
- Cheia de acumulare: `(PartenerId, Rol, ContId)` (`SP:503`), apoi grupare pe
  `(Partener, Rol)` (`SP:525`).
- Se sare partenerul cu deschidere = închidere = mișcare = 0 (`SP:531`).
- Partenerii se citesc cu `IgnoreQueryFilters` — cel șters logic se declară
  (`SP:420`).
- Nu se filtrează `Storno`.

**Ce citește din DOCUMENT**:
- `Document.PredatorId` / `PrimitorId` alimentează mulțimea de repartitori
  citiți (`SP:411`–`SP:414`), dar NU soldurile.
- Lista `tertiReferiti` (partenerii de pe facturi și plăți) adaugă intrări cu
  sold ZERO pentru partenerii referiți dar absenți din agregat
  (`SP:584`, `SP:1398`–`SP:1415`) — `AccountID`-ul lor vine de pe factură/plată,
  nu din agregat.

**Cazuri speciale**:
- Doi parteneri cu ACELAȘI identificator ⇒ O intrare cu solduri CUMULATE +
  `PartenerDublat` (`SP:558`–`SP:565`, `CumuleazaSolduri` `SP:2944`).
- Rând pe cont de terț fără niciun partener pe laturi ⇒ `Neincluse`
  (`RepartitorNePartener` sau `FaraPartener`), cu Debit/Credit acumulate
  (`SP:484`–`SP:501`); se păstrează doar cele cu cifră nenulă (`SP:520`).
- Român/înregistrat TVA cu CUI care nu trece cifra de control ⇒ identitate `04`
  + `PartenerFaraCuiValid` (`SP:572`–`SP:578`).
- Adresă fără localitate ⇒ `"Nespecificat"`, avertisment UNUL per partener
  (`SP:259`–`SP:268`).
- Cusătura terților (`SP:1502`–`SP:1516`): `Σ Closing` din master files +
  `Σ Neincluse[Customers|Suppliers]` trebuie să dea soldul conturilor de rol
  din GLA.

---

## 4. MasterFiles → TaxTable

**Granul rândului**: **cod SAF-T de taxă FOLOSIT** în fișier (`SP:2877`).

Mulțimea codurilor se strânge PE PARCURS, nu dintr-o interogare de nomenclator:
`codTvaFolosit` primește coduri din rândurile de GL (`SP:666`), din liniile de
factură (`SP:1175`) și din ITV (`SP:639`). Pe modulul C se strânge din
`(TipTvaId, Sens)` distincte în `RegistruTva` (`SP:2223`–`SP:2232`).

| Câmp | Sursă | Fel | Fișier:linie |
|---|---|---|---|
| `TaxType` | constanta `"300"` | calcul | `SR:24`, `SP:2882` |
| `TaxCode` | `TipTva.CodSafTLivrare` / `CodSafTAchizitie`, după `RegistruTva.Sens` | nomenclator | `SP:655`, `SP:1169`, `SP:2229` |
| `Description` | `TipTva.Denumire` | nomenclator | `SP:2884` |
| `TaxPercentage` | `TipTva.Cota` — NOMENCLATORUL CURENT, nu snapshotul de pe rând | nomenclator | `SP:666`, `SP:2885` |
| `BaseRate` | constanta `1` („integral deductibil") | calcul | `SP:2888` |
| `Country` | constanta `"RO"` | calcul | `SP:2889` |

**Filtre**: codul `000000` (`TaxCodeNefiscal`) nu se declară (`SP:2879`).
Ordinea: cod ordinal.

**Ce citește din DOCUMENT**: nimic.

**Cazuri speciale**:
- Cod sintetic `380200` (`TaxCodeInchidereTva`, `SR:30`) pentru documentele
  `ITV`, cu cotă 0 și descriere scrisă în cod (`SP:639`–`SP:643`).
- Divergență structurală: `TaxTable.TaxPercentage` vine din nomenclatorul de
  ACUM (`TipTva.Cota`), iar `TaxInformation.TaxPercentage` de pe linii vine din
  SNAPSHOT-ul rândului (`RegistruTva.Cota`, `SP:669`, `SP:1173`).
- `TipTva` fără cod SAF-T pe direcția folosită ⇒ rândul iese `000/000000`, taxa
  se numără separat în `TvaFaraCodSaft` + un avertisment per TIP cu suma tuturor
  rândurilor lui (`SP:656`–`SP:663`, `SP:799`–`SP:803`).

---

## 5. MasterFiles → UOMTable + Products

**Granul rândului**: `Products` = **produs REFERIT de fișier**; `UOMTable` =
**cod de unitate folosit** (`ProduseSiUnitati`, `SP:2820`).

Mulțimea de produse: pe L din liniile de factură — `FacturaIntrareDetaliu.ProdusId`
/ `FacturaIesireDetaliu.ProdusId`, cu rezervă `Lot.ProdusId`
(`SP:867`–`SP:881`, `SP:1133`–`SP:1137`); pe C din loturile mișcate și din
stocul fizic (`SP:1874`, `SP:2111`).

| Câmp | Sursă | Fel | Fișier:linie |
|---|---|---|---|
| `ProductCode` | `Produs.Cod` (rezervă: GUID-ul) | nomenclator | `SP:2853` |
| `GoodsServicesID` | `"01"` dacă `TipMaterial.Clasa.Natura == Stoc`, altfel `"02"` | nomenclator + calcul | `SP:2828`, `SP:2854` |
| `Description` | `Produs.Denumire` | nomenclator | `SP:2855` |
| `ProductCommodityCode` | `Produs.CodNc` (rezervă `"0"`) | nomenclator | `SP:2834`–`SP:2838` |
| `ValuationMethod` | constanta `"FIFO"` | calcul | `SP:59`, `SP:2857` |
| `UOMBase` / `UOMStandard` | `Produs.UnitateMasura.Cod` (rezervă `"H87"`) | nomenclator | `SP:2840`–`SP:2847`, `SP:2858` |
| `UOMToUOMBaseConversionFactor` | constanta `1` | calcul | `SP:2861` |
| `UOMTableEntry.UnitOfMeasure` | codul distinct folosit | calcul | `SP:2870` |
| `UOMTableEntry.Description` | `UnitateMasura.Denumire` (rezervă: codul) | nomenclator | `SP:2866`–`SP:2871` |

**Filtre**: DOAR produsele referite, nu tot nomenclatorul (`SP:2818`); citite cu
`IgnoreQueryFilters` (`SP:2823`).

**Ce citește din DOCUMENT**: identitatea produsului vine de pe FRUNZA liniei
(`FacturaI*Detaliu.ProdusId`), nu din registru — `DocumentDetaliu` de bază nu
poartă produsul, doar `LotId` și `TipMaterialId` (`SP:858`).

**Cazuri speciale**: `FaraCodNc` și `FaraUnitateMasura` sunt avertismente cu
valoare de rezervă, nu refuzuri (`SP:2837`, `SP:2845`); `Produs.UM` (textul
liber) se citește DOAR ca să intre în mesajul avertismentului (`SP:2847`).

---

## 6. MasterFiles → AnalysisTypeTable

**Granul rândului**: **(tip dimensiune × `AnalysisID`) FOLOSIT** pe o linie de
GL (`EtichetePerioada`, `SP:2897`).

| Câmp | Sursă | Fel | Fișier:linie |
|---|---|---|---|
| `AnalysisType` | literalele `CC`/`P`/`U`/`SF`/`CF`/`CE` | calcul | `SP:596`–`SP:601` |
| `AnalysisTypeDescription` | dicționar scris în cod | calcul | `SP:2898`–`SP:2901` |
| `AnalysisID` | `Cod`-ul nomenclatorului dimensiunii (rezervă: GUID `N`) | nomenclator | `SP:2924`–`SP:2931` |
| `AnalysisIDDescription` | `Denumire`-a nomenclatorului | nomenclator | `SP:2929` |

Nomenclatoarele citite: `CodFunctional`, `CodEconomic`, `SursaFinantare`,
`Unitate`, `Proiect` (`SP:2907`–`SP:2916`); `CC` (centru de cost) e o CALITATE
de `Repartitor`, deci folosește același dicționar de repartitori (`SP:2919`).

**Filtre**: se declară doar tipurile efectiv înregistrate prin
`Inregistreaza` din `Analiza(debit, rând)` (`SP:589`–`SP:603`). Pe modulul C
tabela rămâne GOALĂ deliberat, fiindcă S nu emite `Analysis` pe nicio linie
(`SP:2241`).

**Ce citește din DOCUMENT**: nimic — dimensiunile stau pe rândul de registru.

---

## 7. MasterFiles → PhysicalStock (doar modulul C)

**Granul rândului**: **gestiune (`Repartitor`) × lot**.

**Sursa**: `AgregatStoc` (`SP:2592`) — O SINGURĂ interogare grupată peste
`RegistruStoc` cu `Data <= dataEnd`, cheia `(RepartitorId, LotId, TipStoc)`, cu
sume condiționate (`Data < dataStart` ⇒ inițial; `>= dataStart` ⇒ rulaj). REGISTRU
BRUT, fără snapshot — spre deosebire de `GeneralLedgerAccounts`.
`SoldPeCheie` (`SP:2613`) reduce apoi pe `(gestiune × lot)` peste `TipStoc`-urile
raportate.

| Câmp | Sursă | Fel | Fișier:linie |
|---|---|---|---|
| `WarehouseID` | `Repartitor.Cod`, rezervă `Denumire` tăiat la 35, rezervă GUID | nomenclator | `SP:1879`, `SP:2630` |
| `ProductCode` | `Produs.Cod` prin `Lot.ProdusId` | nomenclator | `SP:2205` |
| `ProductType` | `SR.ProductTypeDinCont(TipMaterial.ContImplicit.Simbol)` | politică + nomenclator | `SP:1818`, `SP:1880`, `SR:418` |
| `StockAccountNo` | `LotId`, DOAR dacă produsul are >1 lot în aceeași gestiune | calcul | `SP:1896`–`SP:1903` |
| `StockAccountCommodityCode` | `Produs.CodNc` (rezervă `"0"`) | nomenclator | `SP:2207` |
| `OwnerID` | `SR.OwnerIdRaportor` = `"00"` + CUI-ul societății | nomenclator + calcul | `SP:1640`, `SR:398` |
| `UOMPhysicalStock` | `UOMBase` al produsului (rezervă `"H87"`) | nomenclator | `SP:2206` |
| `UOMToUOMPhysicalStockConversionFactor` | constanta `1` | calcul | `SP:1882` |
| `UnitPrice` | `Lot.PretUnitar`, rotunjit la 2 zecimale `AwayFromZero` | nomenclator + calcul | `SP:1810`, `SP:1884` |
| `OpeningStockQuantity` / `OpeningStockValue` | `Σ CantitateInitiala` / `ValoareInitiala` pe cheie | registru | `SP:1714`, `SP:1885` |
| `ClosingStockQuantity` / `ClosingStockValue` | `Σ (Initiala + Rulaj)` pe cheie | registru | `SP:1716`, `SP:1887` |
| `StockCharacteristic` / `…Value` | constantele `("0","0")` | calcul | `SP:1889`, `SR:431` |

**Filtre și chei de agregare**:
- `TipStoc`-urile RAPORTATE = cele care apar într-o `PoliticaMiscareSaft` CU cod
  (`SP:1687`) — soldurile de `Consum`/`Folosinta` nu sunt patrimoniu în magazie.
- Cheile: deschideri ∪ închideri ∪ chei cu mișcare în lună (`SP:1832`–`SP:1834`);
  intrarea rămâne în fișier dacă a avut o mișcare, chiar cu ambele capete zero
  (`SP:1846`–`SP:1849`).
- Cheia fără lot rezolvabil în nomenclator se sare (`SP:1850`).
- Deschiderea unei chei născute în lună e ABSENȚĂ, nu zero (`exista` în
  `SoldPeCheie`, `SP:2616`).

**Ce citește din DOCUMENT**: nimic — `PhysicalStock` e pur registru + nomenclatoare.

**Cazuri speciale**:
- Sold final NEGATIV ⇒ se declară CA ATARE + `SoldNegativ` (`SP:1857`).
- Cantitate 0 la ambele capete cu valoare nenulă ⇒ `ReziduValoricFaraCantitate`
  (`SP:1868`).
- Produs fără cont de stoc ⇒ `ProductType` iese `"0"` + `ProdusFaraContStoc`,
  avertisment UNUL per produs (`SP:1853`).
- Solduri pe `TipStoc` NERAPORTAT: n-au document, deci nu pot fi `Neincluse` —
  ies ca avertisment `SoldPeTipStocNeraportat` cu cifra lor (`SP:1721`–`SP:1737`).
- `PhysicalStock` gol ⇒ fișierul C se refuză cu 422 la ușă (`SC:186`) și la
  scriitor (`SX:117`–`SX:131`).

---

## 8. GeneralLedgerEntries

**Granul rândului**: trei niveluri —
`Journal` = **cod de tip document**;
`Transaction` = **document** (o tranzacție per document, indiferent de storno);
`TransactionLine` = **rând de `RegistruContabil` × latură** (două linii per rând).

**Sursa**: `RegistruContabil` proiectat PLAT (`RandGl`, `SP:82`), filtrat
`Data >= dataStart && Data <= dataEnd && DocumentId != null` (`SP:331`–`SP:347`).
Rândurile de deschidere ies (sunt solduri, nu tranzacții); rândurile de storno
INTRĂ, nefiltrate.

### Journal

| Câmp | Sursă | Fel | Fișier:linie |
|---|---|---|---|
| `JournalID` / `Type` | codul tipului documentului, prin `ApiProiectii.CoduriTip` → `Document.ClrType` → `TipDocument.Cod` | document (discriminator) + nomenclator | `SP:370`, `SP:711`, `CT:25` |
| `Description` | `TipDocument.Denumire` | nomenclator | `SP:310`–`SP:314`, `SP:713` |

### Transaction

| Câmp | Sursă | Fel | Fișier:linie |
|---|---|---|---|
| `TransactionID` | `DocumentId` ca GUID | document | `SP:728` |
| `Period` / `PeriodYear` | parametrii `luna`/`an` ai cererii (CONSTANTE pe tot fișierul) | calcul | `SP:729`–`SP:730` |
| `TransactionDate` | `MIN(RegistruContabil.Data)` al rândurilor documentului | registru | `SP:725`, `SP:731` |
| `Description` | `TipDocument.Denumire` + `Document.Numar` | nomenclator + **document** | `SP:717` |
| `SystemEntryDate` | `Document.DataOperare` (rezervă: `TransactionDate`) | **document** | `SP:733` |
| `GLPostingDate` | = `TransactionDate` | calcul | `SP:735` |
| `CustomerID` / `SupplierID` | identitatea societății; înlocuită dacă documentul are EXACT un partener pe laturi, cu rolul dat de primul cont de terț de pe rânduri | **document** + registru | `SP:736`, `SP:741`–`SP:747`, `SP:2957`, `SP:2968` |

### TransactionLine

| Câmp | Sursă | Fel | Fișier:linie |
|---|---|---|---|
| `RecordID` | poziția crescătoare în tranzacție | calcul | `SP:749`, `SP:762` |
| `AccountID` | `SimbolSaft(ContDebitId)` / `SimbolSaft(ContCreditId)` | registru + nomenclator | `SP:763` |
| `Analysis` | cele 6 dimensiuni ale LATURII (`CC`,`P`,`U`,`SF`,`CF`,`CE`) | registru | `SP:589`–`SP:603`, `SP:771` |
| `CustomerID` / `SupplierID` | `IdentitatiLatura`: cont fără rol ⇒ societatea pe ambele; cont cu rol ⇒ partenerul rândului pe latura rolului | registru + nomenclator | `SP:678`–`SP:696` |
| `Description` | `Descriere` a frunzei liniei-sursă, rezervă descrierea documentului | **document** (frunză) | `SP:609`–`SP:617`, `SP:755`, `SP:766` |
| `DebitCreditIndicator` | `"D"` pe prima latură, `"C"` pe a doua | calcul | `SP:756`, `SP:767` |
| `DebitAmount` / `CreditAmount` | `RegistruContabil.Valoare` (aceeași cifră pe ambele laturi) | registru | `SP:768`, `SX:466` |
| `CurrencyCode` / `CurrencyAmount` | constanta `"RON"` și aceeași `Valoare` | calcul | `SP:769`–`SP:770` |
| `TaxInformation.*` | `TaxaRandului` (mai jos) | registru TVA | `SP:633`–`SP:674`, `SP:775` |
| `NumberOfEntries` (secțiune) | numărul de TRANZACȚII | calcul | `SX:439` |
| `TotalDebit` / `TotalCredit` (secțiune) | `Σ` pe liniile emise, fiecare pe latura ei | calcul | `SP:789`–`SP:790`, `SX:440` |

**`TaxInformation` pe linia de GL** (`SP:633`):
- `ITV` (închiderea lunară de TVA), identificat prin TIPUL documentului, nu prin
  simbol de cont ⇒ cod fix `380200`, `TaxAmount = 0` (`SP:638`).
- Altfel: se caută faptul fiscal în `RegistruTva` pe cheia `(DetaliuId, Storno)`
  (`SP:359`–`SP:362`), și se emite DOAR dacă rândul atinge
  `TipTva.ContTvaDeductibil` sau `ContTvaColectat` (`SP:647`). `ContTvaNeexigibil`
  (4428) e EXCLUS deliberat (`SP:650`).
- `TaxPercentage` = `RegistruTva.Cota`, `TaxBase` = `RegistruTva.Baza`,
  `TaxAmount` = `RegistruTva.Tva` — toate SNAPSHOT de pe rândul fiscal
  (`SP:669`).
- Restul ⇒ `000/000000` cu `TaxAmount = 0` (`SP:629`).

**Filtre și chei de agregare**:
- Documentele: `RegistruContabil` al perioadei ∪ `RegistruTva` al perioadei
  (`SP:367`–`SP:369`) — un document care n-a postat nimic contabil are totuși
  fapte fiscale.
- `RegistruTva` se filtrează pe **PerioadaDeclarare** (`PerioadaAn`/`PerioadaLuna`,
  `TvaProiectii.IntreLuni`, `TP:145`), iar `RegistruContabil` pe **`Data`**
  — DOUĂ repere diferite în aceeași proiecție.
- Ordinea documentelor: `Document.Data`, apoi `Document.Numar`, apoi ID
  (`SP:704`–`SP:707`); ordinea rândurilor: `Data`, `NumarNota`, `Id`
  (`SP:750`–`SP:753`).

**Ce citește din DOCUMENT și nu din registru**:
`Document.Numar` (descrierea tranzacției), `Document.DataOperare`
(`SystemEntryDate`), `Document.PredatorId`/`PrimitorId` (identitățile la nivel
de tranzacție), `Document.Data` (doar ordinea), `ClrType` (jurnalul), și
`Descriere` de pe frunzele `NotaContabilaDetaliu` / `DecontDetaliu` /
`FacturaIesireDetaliu` (descrierea liniei).

**Cazuri speciale**:
- `TransactionDate` e MINIMUL rândurilor, nu `Document.Data` — un document
  operat luna trecută și stornat acum apare cu data stornării (`SP:718`–`SP:725`).
- Rând pe cont de terț fără partener pe laturi ⇒ societatea pe ambele
  identificatoare + `TertFaraPartener` (`SP:686`–`SP:692`).
- Rândul cu DOI parteneri (compensare `401 = 4111`): fiecare latură își ia
  partenerul propriu, rolul îl dă contul laturii (`SP:457`–`SP:468`).
- `NumarNota` se citește (`SP:334`) dar NU se emite — servește doar la ordonare.

---

## 9. SourceDocuments → SalesInvoices / PurchaseInvoices

**Granul rândului**: `Invoice` = **document × jumătate (storno)** —
o factură operată și stornată în aceeași lună produce DOUĂ `Invoice`
(`SP:1001`–`SP:1007`). `InvoiceLine` = **linie de `DocumentDetaliu`**.

**Mulțimea de documente**: coduri de tip HARD-CODATE ca ancore ale seed-ului de
nucleu — vânzare `["FCL","RDC"]`, cumpărare `["FCT","RLF"]`, retur
`["RLF","RDC"]` (`SP:71`–`SP:73`, `SP:806`–`SP:807`).

### Invoice

| Câmp | Sursă | Fel | Fișier:linie |
|---|---|---|---|
| `InvoiceNo` | `Document.Numar` | **document** | `SP:1069` |
| `InvoiceDate` | `MIN(Data)` al rândurilor de registru ale JUMĂTĂȚII; rezervă `Document.Data` | registru (rezervă document) | `SP:1015`–`SP:1017`, `SP:1070` |
| `InvoiceType` | `"381"` dacă storno sau retur, altfel `"380"` | calcul | `SP:1071`, `SR:210` |
| `SelfBillingIndicator` | constanta `"0"` | calcul | `SP:1072` |
| `AccountID` | contul cu rolul așteptat cel mai frecvent pe rândurile jumătății; rezervă: conturile CONEXULUI autogenerat | registru | `SP:1023`–`SP:1036` |
| `CustomerID` / `SupplierID` | `Id406` al partenerului de pe rândul care poartă contul de terț; rezervă: partenerul unic al laturilor documentului | registru + **document** | `SP:1049`–`SP:1058`, `SP:1074` |
| `BillingAddress` | adresa `Partener` | nomenclator | `SP:1077` |
| `Period` / `PeriodYear` | luna/anul cererii | calcul | `SX:646` |
| `NetTotal` | `Σ InvoiceLineAmount` | calcul | `SP:1202` |
| `GrossTotal` | `Σ (net + TVA-ul liniei)`; pe `Capitalizat` TVA-ul vine din `RegistruTva.Tva` | calcul + registru TVA | `SP:1207` |
| `TaxInformationTotals` | grupare a liniilor pe `(TaxType, TaxCode)` cu `Σ TaxBase` / `Σ TaxAmount` | calcul | `SP:1210`–`SP:1219` |
| `NumberOfEntries` (secțiune) | numărul de facturi | calcul | `SX:634` |
| `TotalDebit` / `TotalCredit` (secțiune) | `Σ NetTotal` pe o latură, `0` pe cealaltă, după sensul secțiunii | calcul | `SX:635`–`SX:636` |

### InvoiceLine

| Câmp | Sursă | Fel | Fișier:linie |
|---|---|---|---|
| `LineNumber` | poziția crescătoare | calcul | `SP:1181`, `SP:1184` |
| `AccountID` | contul contrapartidei de pe rândul de registru al liniei; pe liniile de STOC ale facturii de intrare: `ContDebitId` al rândului de RECEPȚIE de pe NIR-ul conex | registru | `SP:1096`–`SP:1119`, `SP:912`–`SP:949` |
| `Analysis` | dimensiunile LATURII contrapartidei (sau ale rândului de recepție) | registru | `SP:1199` |
| `ProductCode` / `ProductDescription` / `InvoiceUOM` | `Produs` (prin `FacturaI*Detaliu.ProdusId`, rezervă `Lot.ProdusId`) | nomenclator | `SP:1133`–`SP:1135`, `SP:1436`–`SP:1438` |
| `Quantity` | `|DocumentDetaliu.Cantitate|`; `0` devine `1` | **document** + calcul | `SP:1158`, `SP:1164` |
| `UnitPrice` | `FacturaIntrareDetaliu.PretUnitar` / `FacturaIesireDetaliu.PretUnitar`; dacă `0`, `|valoare| / cantitate` rotunjit la bani | **document** (frunză) + calcul | `SP:867`–`SP:876`, `SP:1159`–`SP:1163` |
| `TaxPointDate` | data JUMĂTĂȚII (= `InvoiceDate`) | registru | `SP:1191` |
| `Description` | `Descriere` a frunzei, rezervă `TipMaterial.Denumire`, rezervă `InvoiceNo` | **document** + nomenclator | `SP:1192`–`SP:1194` |
| `InvoiceLineAmount` | `semn × DocumentDetaliu.Valoare`; pe regim `Capitalizat` însă `RegistruTva.Baza` | **document** / registru TVA | `SP:1156`–`SP:1157` |
| `DebitCreditIndicator` | `"C"` pe vânzare, `"D"` pe cumpărare — CONSTANT pe secțiune | calcul | `SP:1198` |
| `TaxInformation.*` | `RegistruTva` (Cota/Baza/Tva) + `TipTva.CodSafT*` | registru TVA + nomenclator | `SP:1167`–`SP:1176` |

**Filtre și chei de agregare**:
- Jumătățile: valorile distincte de `Storno` de pe rândurile contabile; dacă
  documentul n-are rânduri contabile, jumătățile se citesc din `RegistruTva`
  (`SP:1001`–`SP:1005`).
- Liniile: `DocumentDetaliu` al documentului, ordonate pe ID (`SP:855`–`SP:862`).
- Faptul fiscal se caută pe `(DetaliuId, Storno)` (`SP:1141`).
- Fără rotunjire în proiecție; rotunjirea e a fișierului (`Bani` la 2 zecimale
  `AwayFromZero`, `SX:688`; `Cantitate` la 6, `SX:692`).

**Ce citește din DOCUMENT și nu din registru** (lista critică):
`Document.Numar`, `Document.Data` (rezervă + ordine), `DocumentDetaliu.Cantitate`,
`DocumentDetaliu.Valoare`, `DocumentDetaliu.ValoareTva`, `DocumentDetaliu.TipMaterialId`,
`DocumentDetaliu.LotId`, `DocumentDetaliu.TipTvaId`, `FacturaI*Detaliu.PretUnitar`,
`FacturaI*Detaliu.ProdusId`, `FacturaIesireDetaliu.Descriere`,
`FacturaIntrare.Valuta`, `Document.PredatorId`/`PrimitorId`,
`Document.Autogenerat`/`DocumentSursaId` (pentru conex).

**Cazuri speciale**:
- Linia de COST a returului de la client (`RDC` cu lot și fără `TipTva`) se SARE
  din factură — rămâne doar în GL (`SP:1092`).
- `Capitalizat` (achiziție fără drept de deducere): `DocumentDetaliu.Valoare` e
  BRUT, deci netul se ia din `RegistruTva.Baza`, iar brutul se reface cu
  `RegistruTva.Tva` (`SP:1144`–`SP:1157`, `SP:1207`).
- Contul de terț poate sta pe CONEXUL autogenerat, nu pe factură (achiziție
  intra-comunitară / taxare inversă: singurele rânduri ale facturii sunt
  `4426 = 4427`) — `SP:812`–`SP:853`.
- Linia de stoc a facturii de intrare fără rând contabil propriu: contul se
  citește de pe rândul de RECEPȚIE al NIR-ului conex, găsit prin
  `Lot.LinieIntrareId` → `RegistruStoc` (cantitate > 0, nestornat) → `DetaliuId`
  → `RegistruContabil.ContDebitId` (`SP:890`–`SP:949`).
- Fără contrapartidă ȘI fără recepție ⇒ `Neincluse/FaraContrapartida`
  (`SP:1120`–`SP:1130`); fără cont de terț ⇒ `Neincluse/ContFaraRol`
  (`SP:1037`–`SP:1045`); fără partener ⇒ `Neincluse/DocumentFaraPartener`
  (`SP:1059`–`SP:1062`).
- `InvoiceNo` duplicat NU se discriminează — se strigă doar (`SP:1235`–`SP:1242`).
- Valuta: `FacturaIntrare.Valuta != RON` ⇒ avertisment, fără conversie și fără
  curs (`SP:1080`–`SP:1085`).
- Fapte fiscale ale tipurilor FĂRĂ secțiune de facturi (DEC, NTC, bon) ⇒
  `Neincluse/TipFaraSectiuneFacturi`, agregat pe `(document × storno × sens)`
  (`SP:1451`–`SP:1486`).

---

## 10. SourceDocuments → Payments

**Granul rândului**: `Payment` = **document × jumătate (storno)**;
`PaymentLine` = **linie de `DocumentDetaliu`**.

**Mulțimea**: coduri hard-codate `["PLT","INC"]` (`SP:74`, `SP:808`).

| Câmp | Sursă | Fel | Fișier:linie |
|---|---|---|---|
| `PaymentRefNo` | `Document.Numar` | **document** | `SP:1340` |
| `TransactionDate` | `MIN(Data)` al rândurilor jumătății; rezervă `Document.Data` | registru | `SP:1343`–`SP:1345` |
| `Period` / `PeriodYear` | luna/anul cererii | calcul | `SX:487` |
| `PaymentMethod` / `PaymentMechanism` | `SR.MetodaPlata(DocumentTrezorerie.TipInstrument)`: Casă/Chitanță ⇒ `("01","10")`, Cec ⇒ `("03","20")`, rest ⇒ `("03","42")` | **document** (frunză) + calcul | `SP:1245`–`SP:1248`, `SP:1317`, `SR:220` |
| `Description` | `TipDocument.Denumire` + `Document.Numar` (+ `" (storno)"`) | nomenclator + **document** | `SP:1335`, `SP:1348` |
| `GrossTotal` | `Σ PaymentLineAmount` | calcul | `SP:1384` |
| `LineNumber` | poziția crescătoare | calcul | `SP:1367` |
| `SourceDocumentID` | `Document.Numar` al documentului stins, DOAR dacă `Imperechere` dă exact unul | **document** + împerechere | `SP:1249`–`SP:1258`, `SP:1319` |
| `AccountID` | contul cu rol de terț al rândului liniei; rezervă: debit pe PLT / credit pe INC; rezervă: contul de terț al documentului | registru | `SP:1356`–`SP:1372` |
| `CustomerID` / `SupplierID` | partenerul contrapartidei (`PrimitorId` pe PLT, `PredatorId` pe INC), pe latura dată de rolul contului | **document** + registru | `SP:1267`, `SP:1301`–`SP:1303` |
| `Description` (linie) | `Descriere` a frunzei, rezervă descrierea plății | **document** | `SP:1375` |
| `DebitCreditIndicator` | `"D"` pe PLT, `"C"` pe INC — CONSTANT | calcul | `SP:1379` |
| `PaymentLineAmount` | `semn × (DocumentDetaliu.Valoare + ValoareTva)` | **document** | `SP:1380` |
| `Analysis` | dimensiunile laturii rândului plății | registru | `SP:1381` |
| `TaxInformation` | ÎNTOTDEAUNA `000/000000`, `TaxAmount = 0` | calcul | `SP:1382` |
| `NumberOfEntries` / `TotalDebit` / `TotalCredit` (secțiune) | numărul de plăți; `Σ` pe liniile cu indicatorul respectiv | calcul | `SX:485`–`SX:490` |

**Filtre și excluderi**:
- Viramentul intern (ambele laturi `ContPropriu`) se SARE complet — rămâne doar
  în GL, și NU e raportat ca `Neinclus` (`SP:1272`–`SP:1274`).
- Plată către un `Angajat` ⇒ societatea pe ambele identificatoare +
  `PlataCatreAngajat` (`SP:1306`–`SP:1311`).
- Rândurile plății fără niciun cont cu `RolTert` ⇒ `Neincluse/ContFaraRol`
  (`SP:1293`–`SP:1299`).
- Contrapartidă care nu e nici partener, nici angajat ⇒
  `Neincluse/DocumentFaraPartener` (`SP:1312`–`SP:1315`).
- Jumătățile de storno se sparg exact ca la facturi (`SP:1322`–`SP:1332`).

**Ce citește din DOCUMENT și nu din registru**: `Document.Numar`,
`DocumentTrezorerie.TipInstrument`, `DocumentDetaliu.Valoare` + `ValoareTva`
(suma plății!), `Document.PredatorId`/`PrimitorId`, `Imperechere.DocumentId`
(documentul stins) și `Document.Numar` al acestuia.

---

## 11. SourceDocuments → MovementOfGoods (doar modulul C)

**Granul rândului**: `StockMovement` = **(document × storno × cod de mișcare)**;
`StockMovementLine` = **rând de `RegistruStoc`**.

**Sursa**: `RegistruStoc` proiectat plat (`RandStoc`, `SP:2435`), filtrat
`Data >= dataStart && Data <= dataEnd && DocumentId != null` (`SP:1693`–`SP:1700`).

**Potrivirea politicii** (`SP:1657`–`SP:1682`): `PoliticaMiscareSaft` pe cheia
`(TipDocumentId × TipStoc × Semn?)`, cu regulă exactă pe semn înaintea celei
generice. **Semnul e al REGULII, nu al rândului**:
`(Storno ? −1 : +1) × sign(Cantitate, rezervă Valoare)` (`SP:1913`).

| Câmp | Sursă | Fel | Fișier:linie |
|---|---|---|---|
| `MovementReference` | `SR.MovementReference(codTip, Numar, cod dacă grupul se sparge, storno, discriminant)`, max 35, tăiat de la ÎNCEPUT | **document** + politică + calcul | `SP:2044`, `SR:463` |
| `MovementDate` | `MIN(RegistruStoc.Data)` al grupului | registru | `SP:2162` |
| `MovementPostingDate` | `Document.DataOperare`, DOAR dacă e în perioadă; altfel se OMITE + avertisment | **document** | `SP:2147`–`SP:2156` |
| `MovementType` | `PoliticaMiscareSaft.CodMiscare` | politică | `SP:2164` |
| `DocumentReference.DocumentType` | codul tipului documentului | document (discriminator) | `SP:2165` |
| `DocumentReference.DocumentNumber` | `Document.Numar` | **document** | `SP:2166` |
| `TransactionID` (pe linie) | `DocumentId` GUID | document | `SP:2167` |
| `LineNumber` | poziția crescătoare | calcul | `SP:2115` |
| `AccountID` | `ProductTypeDinCont(TipMaterial.ContImplicit.Simbol)` al produsului lotului | politică + nomenclator | `SP:2089`, `SP:2116` |
| `CustomerID` / `SupplierID` | `SR.TertiLinieStoc(rol, idPartener, idSocietate)`: Client ⇒ `(partener, "0")`, Furnizor ⇒ `("0", partener)`, Niciunul sau partener lipsă ⇒ `(raportor, raportor)` | politică + **document** | `SP:2083`, `SR:380` |
| `ProductCode` | `Produs.Cod` prin `Lot.ProdusId` | nomenclator | `SP:2210` |
| `StockAccountNo` | `LotId`, doar dacă produsul are >1 lot în gestiune | calcul | `SP:2122` |
| `Quantity` | `RegistruStoc.Cantitate` — SEMNATĂ ca în registru (storno = invers) | registru | `SP:2125` |
| `UnitOfMeasure` | `UOMBase` al produsului (rezervă `"H87"`) | nomenclator | `SP:2211` |
| `UOMToUOMPhysicalStockConversionFactor` | constanta `1` | calcul | `SP:2126` |
| `BookValue` | `RegistruStoc.Valoare` | registru | `SP:2127` |
| `MovementSubType` | același cod ca `MovementType` (modelul n-are a doua axă) | politică | `SP:2130` |
| `NumberOfMovementLines` (secțiune) | numărul total de linii emise | calcul | `SP:2190` |
| `TotalQuantityReceived` / `Issued` | `Σ Quantity > 0` / `|Σ Quantity < 0|` | calcul | `SP:2191`–`SP:2194` |

**Partenerul mișcării** (`SP:1988`): partenerul unic al laturilor documentului;
dacă documentul e AUTOGENERAT, partenerul laturilor documentului-SURSĂ (NIR ← FCT,
DSC ← FCL) — `SP:1750`–`SP:1762`.

**Filtre și chei de agregare**:
- Gruparea: `(DocumentId, Storno, Cod)`, ordonată pe `MIN(Data)`, apoi
  `DocumentId`, `Storno`, `Cod` (`SP:2034`–`SP:2039`).
- Un document se SPARGE pe coduri doar când poartă mai multe pe aceeași jumătate
  (`SP:1984`–`SP:1986`, `SP:2043`).
- Rolul e AL GRUPULUI: rolul ne-`Niciunul` dacă e singurul distinct, altfel rolul
  primei linii pe `Id` + avertisment `RolTertMixt` (`SP:2063`–`SP:2072`).

**Ce citește din DOCUMENT și nu din registru**: `Document.Numar`,
`Document.DataOperare`, `Document.PredatorId`/`PrimitorId`,
`Document.Autogenerat`/`DocumentSursaId`, și `PredatorId`/`PrimitorId` ale
documentului-sursă.

**Cazuri speciale — TREI destinații diferite pentru ce nu intră**:
1. `Excluse` = politică existentă FĂRĂ cod, cu `Motiv` scris de om
   (`SP:1937`–`SP:1949`).
2. `Neincluse/FaraCodMiscare` = nicio politică pe cheie (`SP:1917`–`SP:1935`).
3. `Neincluse/CodMiscareNecunoscut` = cod în afara nomenclatorului legii
   (`SR.CoduriMiscare`, `SR:320`) — re-verificat fiindcă seed-ul și conectoarele
   scriu pe ușa non-secured (`SP:1957`–`SP:1977`).
4. `Neincluse/FaraContStoc` = produs fără `TipMaterial.ContImplicit`; linia iese
   din fișier fiindcă `AccountID` e obligatoriu (`SP:2090`–`SP:2109`).

Alte capcane:
- `(codTip, Numar)` NU e identitate: documentele care se ciocnesc primesc
  discriminantul `#1`…`#n` pe ordinea `DocumentId` + avertisment
  (`SP:2006`–`SP:2025`).
- `MovementTypeTable` declară DOAR codurile folosite, cu descrierea EXCLUSIV din
  nomenclatorul legii (`SP:2139`, `SP:2177`).
- Grupul ale cărui linii au căzut toate se sare, fără mișcare goală (`SP:2133`).

---

## 12. Cusăturile și cifrele care se calculează la citire (`SaftRezumat`)

Nu sunt câmpuri de fișier, dar sunt CITIRI care nu există nicăieri ca date —
se derivă la fiecare cerere (`SD:527`, `SP:1518`, `SP:2361`):

- **L**: `TvaGl` vs `TvaRegistru` vs `TvaCapitalizat` vs `TvaFaraCodSaft`
  (`SP:1525`–`SP:1528`); `BazaFacturi*` vs `BazaNeincluse*` vs `BazaRegistru*`
  per sens (`SP:1529`–`SP:1536`); `ClosingGla` vs `ClosingBalanta` per cont
  (`SP:1488`–`SP:1500`); cusătura terților `Closing + Neincluse == ClosingGla`
  per rol (`SP:1509`–`SP:1516`).
- **C**: S1 `Opening + Σ registru == Closing` per intrare (`SP:2253`–`SP:2265`);
  S5 aceeași egalitate cu `Σ` din LINIILE EMISE (`SP:2277`–`SP:2291`);
  S2 `miscari + excluse + neincluse == registru` (`SP:2390`); S3 `Σ ClosingValue`
  per cont de stoc vs balanță, spartă pe TIPUL documentului prin două interogări
  grupate suplimentare (`SP:2293`–`SP:2334`, `ComponenteS3` `SP:2509`);
  S4 integritatea referințelor (produse, coduri de mișcare, identități de terț,
  `MovementReference` duplicate) (`SP:2336`–`SP:2350`).
- `Neincluse` pleacă pe sârmă AGREGAT per cauză, cu ≤ 5 exemple
  (`AgregaNeincluse` `SP:192`); `Avertismente` la fel, per cod (`SP:1562`).
- Rotunjirea e EXCLUSIV a fișierului: bani la 2 zecimale `AwayFromZero`
  (`SX:688`), cantități la 6 (`SX:692`); proiecția păstrează `decimal` exact
  (`SP:26`).
