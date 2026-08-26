# D406 (SAF-T) — structura oficială, ancora 16.02.2026

> **Ce e D406.** Declarația informativă „Fișierul Standard de Control Fiscal"
> (Standard Audit File for Tax), schema OECD SAF-T 2.0 localizată pentru România.
>
> **Schema**: `RO_SAFT_SchemaDefCod_16.02.2026.xlsx`, **versiunea 4.1.16**, data
> documentului **21.11.2023**, ultima intrare în istoric **nr. 133 din 16.02.2026**
> („Adăugare cod nou în «WHT nomenclator» — 604040"). 55 de foi: schema propriu-zisă
> în foile 1–6, restul = 40 de nomenclatoare.
>
> **`AuditFileVersion` = `2.0`** („for OECD SAF-T 2.0 standard audit file").
>
> **Namespace producție**: `mfp:anaf:dgti:d406:declaratie:v1`
> **Namespace test**: `mfp:anaf:dgti:d406t:declaratie:v1`
> (ambele citite din bytecode-ul validatorului — `d406validator/v0/ValidatorImpl.class`,
> respectiv `d406tvalidator/v0/ValidatorImpl.class`. Prototipul C# din EServices
> emite din greșeală namespace-ul de TEST — vezi §D.6.)
>
> **Validatorul oficial**: DUKIntegrator, kit local `anaf/duk_SAFT_an_luna/`.
> `config/versiuniCurente.txt` = kit `1.4.18.3.3`, `D406;J2.2.8;P2.0.1`,
> `D406T;J2.2.8;P2.0.1` — dar `doc/D406IstoriaVersiunilor.txt` listează publicări
> până la **J2.2.15 (23-Sep-2025)**. **Kitul local e în urmă**; înainte de orice
> validare „de referință" trebuie rulat auto-update-ul, altfel validezi contra unor
> nomenclatoare vechi.
>
> **Actul normativ**: schema citează **OpANAF nr. 1783/2021** doar în antetul
> coloanei „Modul de raportare" din foile 1–5. **Ordinul NU e procurat local** —
> spre deosebire de D300 (`anaf/d300/OPANAF_174_2026.*`) și D394
> (`anaf/d394/OPANAF_3769_2015.*`, `OPANAF_2194_2025.*`). Consecință directă:
> **termenele de depunere, pragurile pe categorii de contribuabili (mari/mijlocii/
> mici) și perioada de grație NU pot fi afirmate din surse locale** (§A.6, §F.1).
>
> **Nu există XSD ca fișier** în kit. Validarea e compilată în `D406Validator.jar`
> (pachet `d406validator.v0`). Singurele fragmente de XSD care supraviețuiesc sunt
> notele de subsol [1]–[7] ale xlsx-ului (grupurile `xs:choice`) — extrase integral
> în §C.4.
>
> **Metoda**: totul de mai jos e citit din sursele locale enumerate în §Surse.
> Cifrele nomenclatoarelor sunt **măsurate** din xlsx, nu estimate. Mesajele
> validatorului sunt **verbatim** din constant-pool-ul claselor Java. Ce nu s-a
> putut confirma e în §F (24 de puncte). Documentul nu conține propuneri de design.

---

## A. Versiune, obligații, termene

### A.1 Ancorele de versiune

| Element | Valoare | Sursa |
|---|---|---|
| Versiunea documentului de schemă | **4.1.16**, data **21.11.2023** | xlsx, foaia `Copertă`, r.0–1 |
| Ultima intrare în istoric | **nr. 133, 16.02.2026** — „Adăugare cod nou în «WHT nomenclator» — 604040" | xlsx, `Istoric document` r.133 |
| `AuditFileVersion` (S.H.1) | **`2.0`** (Default Value declarat `2`), „Filled with 2.0, for OECD SAF-T 2.0 standard audit file" | xlsx, `5. Structures` r.46 |
| Namespace producție | **`mfp:anaf:dgti:d406:declaratie:v1`** | `d406validator/v0/ValidatorImpl.class` |
| Namespace test | **`mfp:anaf:dgti:d406t:declaratie:v1`** | `d406tvalidator/v0/ValidatorImpl.class` |
| Versiunile din kitul local | kit `1.4.18.3.3`; `D406;J2.2.8;P2.0.1`; `D406T;J2.2.8;P2.0.1` | `dist/config/versiuniCurente.txt` (3 linii, integral) |
| Ultima versiune publicată de validator | **J2.2.15, 23-Sep-2025** | `dist/doc/D406IstoriaVersiunilor.txt`, ultima intrare |
| Actul normativ citat | **OpANAF nr. 1783/2021** | xlsx, `2. MasterFiles` r.0 col.9, verbatim: `"Modul de raportare \n(Obligatoriu - Mandatory sau Opțional - Optional) \npentru\n secțiuni/ sub-secțiuni/ structuri și câmpuri \nconform OpANAF nr. 1783/2021"` |

### A.2 Istoricul nomenclatoarelor — ce s-a mișcat în ultimele 24 de luni

Toate schimbările sunt **de nomenclator**, nu de structură (xlsx, `Istoric document`
r.110–133):

| Ver. | Data | Ce |
|---|---|---|
| 115 | 31.07.2024 | Update „Corespondent rând D300" pe toate foile `Achizitii`: rd. 5→5/20, 5/5.1→5/5.1/20/20.1, 6→6/21, 7→7/22, 7/7.1→7/7.1/22/22.1, 8→8/23 |
| 116 | 17.10.2024 | Format IBAN: Côte d'Ivoire `28(1a,23n)`→`28(2a,22n)`, Mauritius `30(4a,19n,3a)`→`30(4a,22c)` |
| 117–119 | 04.03.2025 | +3 taxcode WHT (150060, 150070, 604030); explicație `BankAccount` în Header pentru societăți fără cont bancar; +51 coduri NC8 („NC25") |
| 122 | 05.06.2025 | coduri de țară **`EU`** și **`IM`**, pentru contribuabilii înrolați în OSS |
| 123 | 05.06.2025 | `TaxAccountingBasis` acceptă **`ONGE`** (persoane juridice fără scop patrimonial înregistrate și în scopuri de TVA) |
| 124 | 05.06.2025 | `HeaderComment` acceptă **`NL`** și pentru contribuabilii fără scop patrimonial înregistrați în scopuri de TVA |
| 125 | 26.06.2025 | plan de conturi nou: **`PlanConturiBNR - ordin 6`** |
| 126 | 30.07.2025 | **TaxCode-uri pentru cotele 21% și 11%** (Livrări + toate Achizițiile) |
| 127 | 12.08.2025 | corecție `341306` (TVA 21%) — „pentru evitare coliziune cu TaxCode existent 341304" |
| 128 | 22.09.2025 | `310357`, `310358` — cota 9% conform art. III din Legea 141/2025 (al doilea = furnizori care aplică TVA la încasare) |
| 129 | 28.10.2025 | `310359` — regularizări vânzări intracomunitare la distanță + servicii TBE (art. 278 alin. (1)) |
| 130 | 28.10.2025 | Update „Corespondent rând D300 după data de 01.08.2025" |
| 131–133 | 05.02–16.02.2026 | +27 coduri NC8 („NC26"); +6 coduri WHT (150080, 150090, 631090, 631100, 642050, 642060); +604040 |

Istoricul validatorului (`D406IstoriaVersiunilor.txt`) confirmă că schimbările de
cod sunt tot de nomenclator; două excepții relevante:
- **J2.0.25 (04-Aug-2023)**: „modificare postalCode **optional**"
- **J2.2.5 (19-Nov-2024)**: „**corectie validare AccountId (6 cifre)**" — vezi §F.18
- **J2.2.10 (17-Jun-2025)**: „**AnalysisTypeTable optional**, modificare
  validateDeclaredPeriod pt Headercomment=T"

### A.3 Modul de raportare — matricea oficială pe secțiuni

Foile 1–5 au șase coloane de raportare (19–24). Antetele lor, verbatim
(xlsx, `2. MasterFiles` r.1):

- **L** — „Raportare Lunara/ Trimestrială corelată cu decontul de TVA\* — *\*Entitățile
  neînregistrate în scop de TVA vor depune trimestrial*"
- **S** — „Raportare la cerere - Stocuri"
- **A** — „Raportare Anuala - Active"
- **N** — „Raportare D406 pentru Societățile nerezidente înregistrate în scopuri de TVA"
- **R** — „Raportare rectificativă D406 (care se depune pentru corecția erorilor
  materiale în declarațiile informative D406)"
- **D** — „Contribuabilii care au obligaţia să depună decontul special de taxă pe
  valoarea adăugată"

| Secțiune | „Modul de raportare" (col.9) | Card. | L | S | A | N | R | D |
|---|---|---|:-:|:-:|:-:|:-:|:-:|:-:|
| 1. Header | Mandatory | 1 | x | x | x | x | x | x |
| 2. MasterFiles | Mandatory | 1 | x | x | x | x | x | x |
| 2.1 GeneralLedgerAccounts | Mandatory | 1 | x | x | x | — | x | — |
| 2.2 Taxonomies | **Optional – this section is NEVER reported** | **0** | — | — | — | — | — | — |
| 2.3 Customers | Mandatory | 1 | x | — | — | — | x | — |
| 2.4 Suppliers | Mandatory | 1 | x | — | — | — | x | — |
| 2.5 TaxTable | Mandatory | 1 | x | x | — | x | x | x |
| 2.6 UOMTable | Mandatory | 1 | x | x | — | x | x | x |
| 2.7 AnalysisTypeTable | Mandatory | 1 | x | x | x | — | x | — |
| 2.8 MovementTypeTable | **Mandatory – by request** | 1 | — | x | — | — | x | — |
| 2.9 Products | Mandatory | 1 | x | x | — | x | x | x |
| 2.10 PhysicalStock | **Mandatory – by request** | 0..1 | — | x | — | — | x | — |
| 2.11 Owners | **Mandatory – by request** | 1 | — | x | — | — | x | — |
| 2.12 Assets | **Mandatory – once per year** | 1 | — | — | x | — | x | — |
| 3. GeneralLedgerEntries | Mandatory | 1 | x | — | — | — | x | — |
| 4.1 SalesInvoices | Mandatory | 1 | x | — | — | x | x | x |
| 4.2 PurchaseInvoices | Mandatory | 1 | *(celulă goală — §F.9)* | — | — | x | x | x |
| 4.3 Payments | Mandatory | 1 | x | — | — | — | x | — |
| 4.4 MovementOfGoods | **Mandatory – by request** | 1 | — | x | — | — | x | — |
| 4.5 AssetTransactions | **Mandatory – once per year** | 0..1 | — | — | x | — | x | — |

Validatorul confirmă gruparea: clasa `ValidatorExtensionImpl` are câmpurile
**`monthQuarterSections`**, **`onDemandSections`**, **`yearlySections`**,
**`externalCompaniesSections`**.

### A.4 Tipul declarației — `HeaderComment` (S.H.11), obligatoriu

| Cod | Înțeles |
|---|---|
| `L` | declarație **lunară** |
| `T` | declarație **trimestrială** |
| `A` | declarație **anuală** (Active) |
| `C` | declarație **la cerere** (Stocuri) |
| `NL` | nerezidenți lunar — *și* contribuabili fără scop patrimonial înregistrați în scopuri de TVA (mod. 124/05.06.2025) |
| `NT` | nerezidenți trimestrial |

Nota de context (xlsx, `1. Header` r.8–9, verbatim):

> „Câmpul S.H.11 HeaderComment a fost modificat din element OPȚIONAL în element
> OBLIGATORIU / pentru a permite raportarea tipului de declarație transmisă"

COM integral (xlsx, `5. Structures` r.56):

> „the non-residents registred for VAT in Romania - assimilated to **large
> taxpayers** - will fill this with „NL" (non-residents monthly) - the non-residents
> registred for VAT in Romania - assimilated to **medium or small taxpayers** - will
> fill this with „NT" (non-residents quarterly)"

Validatorul verifică **corelația tip ↔ perioadă**:
`Tipul declaratiei @0@ nu corespunde cu perioada declarata: @1@.@2@ - @3@.@4@`
(`ValidatorExtension1Impl`, metoda `validateDeclaredPeriod`).

CLI-ul primește perioada explicit (`doc/Instructiuni.txt`, ultima linie):

```
java -Xms250m -Xmx4g -jar DUKIntegrator_AnLunaUI.jar -v D406 d:\d406.xml $ $ an=2025 luna=8
```

### A.5 Raportarea modală (segmentarea fișierului)

Nota metodologică integrală (xlsx, foaia `Instrucțiuni` r.45, descrierea coloanei 10):

> „Transmiterea datelor folosind Declarația D406 - SAF-T este **modală**, adică
> raportarea datelor se poate realiza în mai multe părți (nu aceeași declarație de
> mai multe ori) – ci mai multe formulare D406 depuse în aceeași lună pentru fiecare
> contribuabil. […] Această metodă se folosește în cazul unor Declarații de
> dimensiuni foarte mari, iar **împărțirea se face la nivel de Sub-Secțiune**. […]
> Din considerente tehnice o sub-secțiune **nu poate fi împărțită** în mai multe
> extracții și trebuie obligatoriu transmisă printr-un singur formular D406. […]
> În cazul în care nu este nimic de raportat se va transmite doar tag-urile de
> început și de sfârșit ale sub-secțiunii sau structurii."

Consecințe verificabile:

- `SegmentIndex` (S.H.12) și `TotalSegmentsInSequence` (S.H.13) — ambele Mandatory,
  „Numeric value", „Value must be greater than 0".
- Validatorul: `elementul 'TotalSegmentsInSequence' [@0@] ar fi trebuit sa fie mai
  mare decat SegmentIndex [@1@]` (`AuditFile.class`) — vezi §F.13.
- **Coloana 10 relaxează la `Optional`** exact colecțiile repetitive și totalurile
  de secțiune. Diff-ul col.9 vs col.10, măsurat exhaustiv: `MF.GLA.1 Accounts`,
  `MF.C.1 Customer`, `MF.C.2 CompanyStructure`, `MF.S.1`, `MF.S.2`, `MF.TT.1`,
  `MF.UOM.1`, `MF.AT.1`, `MF.MT.1`, `MF.P.1`, `MF.O.1`, `MF.O.2`, `MF.A.1`,
  `GL.1 NumberOfEntries`, `GL.2 TotalDebit`, `GL.3 TotalCredit`, `GL.4 Journal`,
  `SD.SI.1–4`, `SD.PI.1–4`, `SD.P.1–4`, `SD.MG.1–4`, `SD.AT.2` — toate „Mandatory"
  pe col.9 și „Optional" pe col.10. **De aici cardinalitatea `0..1`/`0..*`** deși
  col.9 le marchează Mandatory.
- Validatorul refuză un prim modul fără MasterFiles:
  `Primul modul al unei declaratii trebuie sa contina cel putin o sectiune din MasterFiles!`
  (`MasterFiles.class`).

### A.6 Termene, praguri, perioadă de grație — **NU sunt în sursele locale**

Nici xlsx-ul, nici kitul DUK, nici documentele din `1C/` nu conțin OpANAF 1783/2021
sau vreun act care să fixeze categoriile de contribuabili, termenele de depunere sau
perioada de grație. Singura urmă e **titlul coloanei** („conform OpANAF nr. 1783/2021").

Ce se poate afirma **doar** pe baza schemei:
- periodicitatea e „corelată cu decontul de TVA", cu nota — „Entitățile
  neînregistrate în scop de TVA vor depune **trimestrial**";
- `Assets` = **anual** („Mandatory - once per year", în funcție de anul financiar al
  contribuabilului);
- `Stocks` = **la cerere** („Mandatory - by request").

Vezi §F.1 pentru ce trebuie procurat.

---

## B. Structura completă a XML-ului

### B.0 Notația și tipurile simple

ID-urile de câmp au forma `<Secțiune>.<Sub-secțiune><Index>` — `H` Header,
`MF` MasterFiles, `GL` GeneralLedgerEntries, `SD` SourceDocuments, `S` Structures
(xlsx, `Instrucțiuni` r.3–11).

**Tipuri simple** (xlsx, foaia `6. SimpleTypes`) — limitele dure ale fișierului:

| Tip | Bază XSD | Restricție |
|---|---|---|
| `SAFcodeType` | `xs:string` | **maxLength 9** |
| `SAFshorttextType` | `xs:string` | **maxLength 18** |
| `SAFmiddle1textType` | `xs:string` | **maxLength 35** |
| `SAFmiddle2textType` | `xs:string` | **maxLength 70** |
| `SAFlongtextType` | `xs:string` | **maxLength 256** |
| `SAFmonetaryType` | `xs:decimal` | totalDigits 18, **fractionalDigits 2**; „For negative value is accepted the `-` sign and the `()`"; „The acceptable delimiter for decimals is the **DECIMAL POINT**" |
| `SAFexchangerateType` | `xs:decimal` | totalDigits 18, fractionalDigits 4; „The exchange rate will be from **Valuta → RO**" |
| `SAFquantityType` | `xs:decimal` | totalDigits 22, **fractionalDigits 6** |
| `SAFweightType` | `xs:decimal` | totalDigits 14, fractionalDigits 3 |
| `SAFBaseRate` | `xs:decimal` | totalDigits 5, fractionalDigits 4, **restricție 0.0000–1.0000** |
| `ISOCountryCode` | `xs:string` | maxLength 2 |
| `ISOCurrencyCode` | `xs:string` | maxLength 3 |
| `Date` | `xs:date` | ISO 8601, `AAAA-LL-ZZ` |
| `Time` | `xs:time` | `HH:MM`, 24h |

Rotunjirea monetară, repetată pe fiecare câmp `SAFmonetaryType`:
„For aproximation is used the **Order no. 978 from 8 July 2005** of the Ministery of
Finance".

### B.1 `Header` (S.H.1–13) — cardinalitate 1

| ID | Element | Tip | O/Opt | Card. | Regulă / valoare |
|---|---|---|---|---|---|
| S.H.1 | `AuditFileVersion` | SAFcodeType | M | 1 | Default `2`; COM: „Filled with 2.0" |
| S.H.2 | `AuditFileCountry` | ISOCountryCode | M | 1 | Default **`RO`**; SEM: „Validation according to ISO3166-2-CountryCodes" |
| S.H.3 | `AuditFileRegion` | SAFcodeType | Opt | 0..1 | SYN+SEM: „Validation according to `ISO3166-1A2 - RO Dept Codes`" (`RO-AB`…`RO-B`) |
| S.H.4 | `AuditFileDateCreated` | Date | M | 1 | ISO 8601 |
| S.H.5 | `SoftwareCompanyName` | middle2 (70) | M | 1 | |
| S.H.6 | `SoftwareID` | longtext (256) | M | 1 | |
| S.H.7 | `SoftwareVersion` | shorttext (18) | M | 1 | |
| S.H.8 | `Company` | **CompanyHeaderStructure** | M | 1 | |
| S.H.9 | `DefaultCurrencyCode` | ISOCurrencyCode | M | 1 | Default **`RON`** |
| S.H.10 | `SelectionCriteria` | SelectionCriteriaStructure | M | 1 | |
| S.H.11 | `HeaderComment` | longtext | **M** | 1 | `L`/`T`/`A`/`C`/`NL`/`NT` (§A.4) |
| S.H.12 | `SegmentIndex` | SAFcodeType | M | 1 | numeric, > 0 |
| S.H.13 | `TotalSegmentsInSequence` | SAFcodeType | M | 1 | numeric, > 0 |
| H.2 | `TaxAccountingBasis` | shorttext | **M** | 1 | vezi mai jos |
| H.3 | `TaxEntity` | middle2 | Opt | 0..1 | „Company / Division / Branch reference" |

**`TaxAccountingBasis` (H.2) → planul de conturi validat.**

Setul compilat în validator (`Parameters_v3.class`, array-ul imediat înaintea listei
de taxCode-uri): `A`, `IFRS`, `BANK`, `INSURANCE`, `NORMA39`, `IFN`, `NORMA36`,
`NORMA14`, `ONG`, `ONGE`, `BNR6`.

| Cod | Cine (descrierea din xlsx, `1. Header` r.4) | Nomenclatorul de conturi (resursa din validator) |
|---|---|---|
| `A` | societăți comerciale generale, dublă partidă, plan general | `plan_conturi_bal_soc_com` |
| `I` | **Invoice Accounting** — nerezidenți / contribuabili cu obligația decontului special de TVA | `plan_conturi_invoice_I` |
| `IFRS` | societăți care aplică OMFP 2844/2016 | `plan_conturi_ifrs` |
| `BANK` | instituții de credit + instituții financiare nebancare | `plan_conturi_banci` |
| `INSURANCE` | societăți de asigurări | `plan_conturi_soc_asigurari` |
| `NORMA39` | leasing/financiare IFRS, Norma ASF 39/2015 | `plan_conturi_n39` |
| `IFN` | IFN, Regulament BNR 17/2015 | `plan_conturi_ifn` |
| `NORMA36` | brokeri asigurări/reasigurări, Norma ASF 36/2015 | `plan_conturi_n36` |
| `NORMA14` | pensii private, Norma ASF 14/2015 | `plan_conturi_norma14` |
| `ONG` | fără scop patrimonial, OMFP 3103/2017 | `plan_conturi_ONG` |
| `ONGE` | ONG **cu cod de TVA** pentru activitate economică | `plan_conturi_ONG` |
| *(BNR6)* | Ordin BNR 6/2015 | `plan_conturi_BNR6` |

*Maparea cod → resursă e dedusă din ordinea constantelor în constant pool
(`plan_conturi_*` urmate imediat de `A, IFRS, BANK, INSURANCE, NORMA39, IFN,
NORMA36, NORMA14, ONG, ONGE, BNR6`). `I` nu se distinge separat în array fiindcă
string-ul „I" e deduplicat cu descriptorul de tip Java — dar xlsx-ul îl declară
valid și resursa `plan_conturi_invoice_I` există în `Nomen_v3.properties`.*

Prefixele de identificare `10`/`11` (§B.4) sunt condiționate explicit de
`TaxAccountingBasis (H2=BANK)`.

### B.2 `MasterFiles`

#### 2.1 GeneralLedgerAccounts → `Account` (0..*)

| ID | Element | Tip | O/Opt | Regulă |
|---|---|---|---|---|
| MF.GLA.2 | `AccountID` | middle2 (70) | M | SYN: **„The account is an integer decimal number, not null"**; SEM: validat contra planului corespunzător lui `TaxAccountingBasis` |
| MF.GLA.3 | `AccountDescription` | longtext | M | |
| *(fără ID — §F.12)* | `StandardAccountID` | middle1 (35) | Opt | „AccountID based on the standard primarily used by the tax payer in their ERP" |
| MF.GLA.5 | `GroupingCategory` | middle1 | Opt | „Category for grouping the accounts, relevant in reconciling financial statements" |
| MF.GLA.6 | `GroupingCode` | middle1 | Opt | |
| MF.GLA.7 | `AccountType` | shorttext | M | **restricție: `Activ` / `Pasiv` / `Bifunctional`** (valori în ROMÂNĂ; confirmate ca array în `Parameters_v3.class`) |
| MF.GLA.8 | `AccountCreationDate` | Date | Opt | |
| MF.GLA.9–12 | `OpeningDebitBalance`/`OpeningCreditBalance`, `ClosingDebitBalance`/`ClosingCreditBalance` | monetary | M | **`xs:choice` pe fiecare pereche** (nota [1]) |

SEM pe solduri, verbatim:

> „If the account is created with a debit, use OpeningDebitBalance and do not report
> OpeningCreditBalance - meaning the element will not be present in the XML file.
> The OpeningDebitBalance may have positive or negative values - to accomodate also
> the **bifunctional accounts** and the negative balance in debit for the other types
> of accounts. **Practically - there are no restrictions on values (+/-) to be
> validated.**"

#### 2.2 Taxonomies — **niciodată raportată** (cardinalitate 0)

Structura există: MF.T.1–7 — `Taxonomies` → `Taxonomy` (`TaxonomyReference` M,
`TaxonomyElement` 0..* → `TaxonomyCode` M, `TaxonomyClusterID`,
`TaxonomyClusterContextID`, `AccountID` M).

#### 2.3 Customers → `Customer` (0..*) / 2.4 Suppliers → `Supplier` (0..*)

Structuri identice:

| ID | Element | Tip | O/Opt | Regulă |
|---|---|---|---|---|
| MF.C.2 / MF.S.2 | `CompanyStructure` | CompanyStructure | M | card. 0..1 |
| MF.C.3 / MF.S.3 | `CustomerID` / `SupplierID` | middle1 (35) | M | schema de prefixe §B.4 |
| MF.C.4 / MF.S.4 | `SelfBillingIndicator` | SAFcodeType | Opt | SEM: **„Use only the code 389 for self-invoice or with value 0 (zero)"**; Default `389` |
| MF.C.5 / MF.S.5 | `AccountID` | middle2 | M | contul analitic de terți |
| MF.C.6–9 / MF.S.6–9 | Opening/Closing Debit/Credit Balance | monetary | M | același `xs:choice` ca la conturi |

#### 2.5 TaxTable → `TaxTableEntry` (0..*) → `TaxCodeDetails` (1..*)

| ID | Element | Tip | O/Opt | Regulă |
|---|---|---|---|---|
| MF.TT.2 | `TaxType` | SAFcodeType | M | „Validation based on **TAX_IMP - Impozite**" |
| MF.TT.3 | `Description` | longtext | M | |
| MF.TT.5 | `TaxCode` | SAFcodeType | M | cele 11 foi de coduri de taxă (§E) |
| MF.TT.6/7 | `EffectiveDate` / `ExpirationDate` | Date | Opt | |
| MF.TT.8 | `Description` | longtext | Opt | |
| MF.TT.9 | `TaxPercentage` | Decimal | M | **`xs:choice` cu `FlatTaxRate`** (nota [2]) |
| MF.TT.10 | `FlatTaxRate` | AmountStructure | M | idem |
| MF.TT.11 | `BaseRate` | SAFBaseRate | M, **1..*** | „Standard is 100 (the whole amount is tax deductible). Example: 60 if only 60% of the total amount is tax deductible"; SYN: „Restriction: [0,0000 - 1,0000]"; Default `0` |
| MF.TT.12 | `Country` | ISOCountryCode | M | Default `RO` |
| MF.TT.13 | `Region` | SAFcodeType | Opt | `ISO3166-1A2 - RO Dept Codes` |

#### 2.6 UOMTable → `UOMTableEntry` (0..*)

`UnitOfMeasure` (M, nomenclatorul `Unitati_masura`) + `Description` (M).

#### 2.7 AnalysisTypeTable → `AnalysisTypeTableEntry` (0..*)

`AnalysisType` (M — „e.g. CC (for Cost Center), DEP (for department)"; COM: „The
company's cost/profit centers should be considered"), `AnalysisTypeDescription` (M),
`AnalysisID` (M — „e.g. 1200-HDOF-TR (for cost center treasury in the headoffice),
P-4800-123 (for project 123 in branch 4800)"), `AnalysisIDDescription` (M).
*Istoricul J2.2.10 (17-Jun-2025): „AnalysisTypeTable optional".*

#### 2.8 MovementTypeTable → `MovementTypeTableEntry` (0..*)

`MovementType` (M, **`Nomenclator stocuri`**, 19 coduri) + `Description` (M).

#### 2.9 Products → `Product` (0..*)

| ID | Element | Tip | O/Opt | Regulă |
|---|---|---|---|---|
| MF.P.2 | `ProductCode` | middle2 | M | |
| MF.P.3 | `GoodsServicesID` | SAFcodeType | Opt | „Validation for one of the two values possible: **01** or **02**" (01 = produse, 02 = servicii) |
| MF.P.4 | `ProductGroup` | middle2 | Opt | |
| MF.P.5 | `Description` | longtext | M | |
| MF.P.6 | `ProductCommodityCode` | middle1 | **M** | SYN: „**Cod NC must be the 8 digits version.** The validation will be done with the nomenclatures **NC8_2021_TARIC3 și Nom_aisg**" |
| MF.P.7 | `ProductNumberCode` | middle2 | Opt | „EAN or other code" |
| MF.P.8 | `ValuationMethod` | SAFcodeType | Opt | COM: „Filled in using one of the following method: FIFO / LIFO / Average cost" |
| MF.P.9 | `UOMBase` | SAFcodeType | M | `Unitati_masura` |
| MF.P.10 | `UOMStandard` | SAFcodeType | M | `Unitati_masura` |
| MF.P.11 | `UOMToUOMBaseConversionFactor` | Decimal | M | vezi SEM mai jos |
| MF.P.13/14 | `Tax` (0..*) → `TaxType` + `TaxCode` | SAFcodeType | M în `Tax` | |

SEM pe `ProductCommodityCode`, verbatim (cazurile în care legea îl cere):

> „Cod NC (8 digits) will be reported where it is required according to the Romanian
> law, particularly in instances like: \* import / export transactions
> \* acquisitions / supplies of food products subjected to reduced VAT rate
> \* intra-community movements subjected to intrastat reporting
> \* acquisitions / supplies subjected to local reversed VAT charge depending on NC
> code \* transactions with excisable products for which excise duties are
> determined ba…"

SEM pe `UOMStandard` / `UOMToUOMBaseConversionFactor`, verbatim:

> „When the conversion is not needed (because the unit of measure is from the
> nomenclature) the elements UOMStandard and UOMToUOMBaseConversionFactor will be
> reported with **value 0 (zero)**. **If UOMBase = UOMStandard,
> UOMToUOMBaseConversionFactor = 1; Otherwise, it cannot have value 1.**"

#### 2.10 PhysicalStock → `PhysicalStockEntry` (1..*)

`WarehouseID` (M, middle1, sursa declarată „Warehouse card (Fisa de magazie)"),
`LocationID` (Opt), `ProductCode` (M), `StockAccountNo` (Opt), `ProductType` (M,
shorttext), `ProductStatus` (Opt), **`StockAccountCommodityCode` (M — sursa
declarată „NC code")**, **`OwnerID` (M)**, `UOMPhysicalStock` (M),
`UOMToUOMBaseConversionFactor` (M), `UnitPrice` (M, „Fisa de magazine – pret
unitar"), `OpeningStockQuantity` / `OpeningStockValue` / `ClosingStockQuantity` /
`ClosingStockValue` (M), `StockCharacteristics` (M, **1..***) →
`StockCharacteristic` + `StockCharacteristicValue` (ambele M; valorile din
`SAFT_Nomenclator_StockChar`).

#### 2.11 Owners → `Owner` (0..*)

`CompanyStructure` (M, 0..1), `OwnerID` (Opt), `AccountID` (M).

#### 2.12 Assets → `Asset` (0..*)

`AssetID` (M), `AccountID` (M), `Description` (M), `AssetSupplier` (Opt 0..* →
`SupplierName` M, `SupplierID` M, `PostalAddress` M), `PurchaseOrderDate` (Opt),
`DateOfAcquisition` (M), `StartUpDate` (M — *devenit opțional în J2.0.22,
26-Mai-2023*), `Valuations` (M) → `Valuation` (1..*):

`AssetValuationType`, `ValuationClass`, `AcquisitionAndProductionCostsBegin`,
`AcquisitionAndProductionCostsEnd`, `InvestmentSupport`,
**`AssetLifeYear` | `AssetLifeMonth` (`xs:choice`, nota [3])**, `AssetAddition`,
`Transfers`, `AssetDisposal`, `BookValueBegin`, `DepreciationMethod`,
`DepreciationPercentage`, `DepreciationForPeriod`, `AppreciationForPeriod`,
`ExtraordinaryDepreciationsForPeriod` (1..* → `ExtraordinaryDepreciationMethod` +
`ExtraordinaryDepreciationAmountForPeriod`), `AccumulatedDepreciation`,
`BookValueEnd` — **toate Mandatory**.

### B.3 `GeneralLedgerEntries` (GL.1–33)

```
GeneralLedgerEntries
├── NumberOfEntries (nonNegativeInteger, M, 0..1)
├── TotalDebit  (monetary, M, 0..1)   ← poate fi NEGATIV
├── TotalCredit (monetary, M, 0..1)   ← poate fi NEGATIV
└── Journal (0..*)
    ├── JournalID (shorttext, M) · Description (longtext, M)
    │   Type (SAFcodeType, M — „Grouping mechanism for journals")
    └── Transaction (1..*)
        ├── TransactionID (middle2, M) · Period (M)
        │   PeriodYear (M, SYN: „(minInclusive 2020, maxInclusive Current-Year]")
        ├── TransactionDate (M, „Document date") · SourceID (Opt)
        │   TransactionType (Opt) · Description (M)
        ├── BatchID (Opt) · SystemEntryDate (M, „Date captured by system")
        │   GLPostingDate (M)
        ├── CustomerID (M) · SupplierID (M)      ← AMBELE obligatorii
        ├── SystemID (shorttext = MAX 18!, Opt)
        └── Line = TransactionLine (1..*)
            ├── RecordID (shorttext, M) · AccountID (middle2, M)
            ├── Analysis (AnalysisStructure, 0..*) · ValueDate (Opt)
            │   SourceDocumentID (Opt)
            ├── CustomerID (M) · SupplierID (M)  ← AMBELE obligatorii
            ├── Description (M)
            ├── DebitAmount | CreditAmount (AmountStructure, xs:choice — nota [4])
            └── TaxInformation (1..*)            ← OBLIGATORIU pe FIECARE linie
```

DESC pe `DebitAmount`/`CreditAmount`, verbatim:

> „The debit amount may be positive or negative, **to reflect the storno în black
> ink**. Negative amounts are prefixed with the minus („-") sign."

SEM: „If the transaction line is debit, use DebitAmount and **don't report
CreditAmount** - meaning the item is not reported (will not be present in the XML
file)."

### B.4 Identificarea partenerului — `RegistrationNumber` / `CustomerID` / `SupplierID`

Aceeași regulă se aplică la: **S.C.1** (`CompanyStructure.RegistrationNumber` =
Customer/Supplier/Owner), **MF.C.3/MF.S.3**, **GL.19/GL.20** (Transaction) și
**GL.28/GL.29** (TransactionLine), **S.I.23/S.I.26** (CustomerInfo/SupplierInfo),
**SD.P.22/23**, **SD.MG.21/22**, **MF.A.11**, **SD.AT.12**.

**Prefix de tip (2 cifre) + cod.** Validarea sintactică și cea semantică sunt
identice cuvânt cu cuvânt (xlsx, `5. Structures` r.30, coloanele 11 și 13):

| Prefix | Cine | Format | Validare |
|---|---|---|---|
| `00` | operator economic din România | `00` + CUI | „max length of the string without the prefix 00, must be **10 digits**. The substring **RO is not accepted**. CUI format is `#########C` – identification number 1–9 digits + 1 control digit". Ex.: `004221306` = Ministerul Finanțelor |
| `01` | operator din UE (≠ RO), înregistrat TVA, verificat VIES | `01` + ISO2 + cod TVA | ISO2 valid din ISO 3166-1. Ex.: `01EL123456789`, `01HU12345678` |
| `02` | operator din afara UE, înregistrat TVA | `02` + ISO2 + cod | ISO2 valid. Ex.: `02TK123005284` |
| `03` | PF cetățeni români (CNP), PF rezidente în RO (același format, prima cifră 7 sau 8), NIF nerezidenți | `03` + 13 cifre | „max 13 digits. The **first digit must be different from 0**" |
| `04` | PF care nu-și declară CNP-ul pe tranzacții (ex. comerț online) | `04` + cod client atribuit de operator | „the string is verified **not to contain special characters** (`.` `,` `!` `-` `?` etc.)" |
| `05` | operator din UE (≠ RO) **NEînregistrat** TVA | `05` + ISO2 + cod client | ISO2 valid |
| `06` | operator non-UE **NEînregistrat** TVA | `06` + ISO2 + cod client | ISO2 valid |
| `08` | client neidentificat cu cod fiscal, la POS (benzinării, alimentare) | **exact `080000000000000`** | „must be 13 digits. The value **must be equal with `080000000000000`**". „This code is restricted ONLY for such transactions, is not a replacement on invoices. **This code is NOT USED for SupplierID**" |
| `09` | PJ nerezidente înregistrate în România | `09` + NIF | max 13 digits, prima cifră ≠ 0 |
| `10` | clienți PJ nerezidente ai societăților bancare, neîncadrabili în 01/02/05/06/09 | `10` + ISO2 + cod | **„then TaxAccountingBasis (H2=BANK)"** + ISO2 valid + max **20** caractere după prefix |
| `11` | clienți PF nerezidente ai societăților bancare, neîncadrabili în 03 | `11` + ISO2 + cod | **„then TaxAccountingBasis (H2=BANK)"** + ISO2 valid + max 20 caractere |
| *orice altceva* | — | — | „In any other case the value is not validated and is return a validation error. (**Validation error – RegistrationNumber incorrect value**)" |
| `0` | — | — | „**RegistrationNumber can not be equal to „0" (zero)**" |

**„Partenerul fără CUI"** — nu există „gol". Există `04` (PF fără CNP declarat),
`05`/`06` (operatori străini neînregistrați) și `08` (POS anonim, valoare fixă).

**Valoarea `0`** e permisă DOAR pe `CustomerID`/`SupplierID`, și DOAR în pereche.
GL.19 SYN, verbatim:

> „If the element GL.19 CustomerID is reported with value „0" (zero), then the
> element GL.20 SupplierID **must be different from „0"**, meaning the identity of
> the partner from which the purchase was made (conventionally considered
> „supplier") is reported"

Simetric pe GL.20 / GL.28 / GL.29.

**Convenția de umplere a laturii libere** — GL.19/GL.20 COM, verbatim:

> „for transactions and transaction lines in section 3.GeneralLedgerEntries that
> **do not represent records of debts and receivables** for which, according to the
> applicable accounting regulations, there is an obligation to account for each
> individual or legal person, in the fields „SupplierID" and „CustomerID" **the
> unique code of the reporting taxpayer will be filled in**; for the transactions
> and lines of transactions that **represents records of debts and receivables**
> […] will be filled in the code „SupplierID", respectively „CustomerID" **as
> defined in the master files section (MasterFiles)**"

⚠️ **`Header.Company.RegistrationNumber` (S.CMH.1) e ALTĂ regulă.** Mesajele
validatorului (`AuditFile.class`, verbatim):

- `Pentru RegistrationNumber @0@ formatul este invalid. Primele doua caractere nu sunt 'RO'. Primele doua caractere trebuie sa fie 'RO' urmate de codul numeric sau campul trebuie sa contina doar codul numeric.`
- `Pentru RegistrationNumber @0@ formatul este invalid. Codul nu este numeric. Primele doua caractere trebuie sa fie 'RO' urmate de codul numeric sau campul trebuie sa contina doar codul numeric.`

Deci `RO12345678` sau `12345678` — **fără** prefix de tip. Semantica din xlsx
(S.CMH.1 SEM):

> „For non-residents with fiscal registration in Romania: Fiscal Registration Code
> (CIF). For residents: if is registered for VAT purposes, set the VAT Registration
> number **with the RO prefix**; otherwise set the Unique Registration Code (CUI)"

### B.5 `SourceDocuments`

#### 4.1 SalesInvoices / 4.2 PurchaseInvoices

Ambele au aceeași formă: `NumberOfEntries` (M, 0..1, sursa „ERP") + `TotalDebit`
(M, 0..1, sursa „GL") + `TotalCredit` (M, 0..1, sursa „GL") + `Invoice` (0..*,
`InvoiceStructure`).

#### `InvoiceStructure` (S.I.1–67)

| ID | Element | Tip | O/Opt | Card. | Regulă |
|---|---|---|---|---|---|
| S.I.1 | `InvoiceNo` | middle2 (70) | M | 1 | |
| S.I.2 | `CustomerInfo` | — | M | 1 | **`xs:choice` cu SupplierInfo** (nota [5]). SEM: „If invoice is related to a **sale**, fill in CustomerInfo and **do not report SupplierInfo**". COM: „In case of a sale, the supplier is the taxpayer." |
| S.I.3 | `SupplierInfo` | — | M | 1 | SEM: „If invoice is related to a **purchase**, fill in SupplierInfo…". COM: „In case of a purchase, the client/ buyer is the taxpayer." |
| S.I.4 | `AccountID` | middle2 | M | 1 | vezi COM mai jos |
| S.I.5 | `BranchStoreNumber` | middle1 | Opt | 0..1 | |
| S.I.6/7 | `Period` / `Period year` | int | Opt | 0..1 | an ∈ [2020, an curent] |
| S.I.8 | `Invoice date` | Date | M | 1 | |
| S.I.9 | `InvoiceType` | SAFcodeType | **M** | 1 | doar 6 coduri, vezi mai jos |
| S.I.10/11 | `Ship To` / `Ship From` | ShippingPointStructure | Opt | 0..1 | |
| S.I.12 | `Payment terms` | middle2 | Opt | 0..1 | |
| S.I.13 | `Self-billing indicator` | SAFcodeType | **M** | 1 | SEM: „Use only the code **389** for self-invoice **or with value 0 (zero)**"; Default `389` |
| S.I.14 | `Source ID` | middle1 | Opt | 0..1 | „Details of person or application that entered the transaction" |
| S.I.15 | `GL Posting Date` | Date | Opt | 0..1 | |
| S.I.16/17/18 | `Batch ID` / `System ID` / `TransactionID` | middle1/middle1/middle2 | Opt | 0..1 | |
| S.I.19 | `Receipt Numbers` | longtext | Opt | 0..1 | „The number(s) of the receipt(s) on this „consolidated invoice record". Can be a single number, a range or a list." COM: „To be filled in with the the reception No. assigned by the taxpayer" |
| S.I.20 | `Line` | InvoiceLine | M | **1..*** | |
| S.I.21 | `InvoiceSettlement` | — | Opt | 0..1 | |
| S.I.22 | `InvoiceDocumentTotals` | — | Opt | 0..1 | |

COM pe S.I.4 `AccountID`, verbatim (partea specifică RO):

> „**S.I.4 AccountID = analytical account number for sales (used to log the invoices)
> is MANDATORY filled with an account number derived from 411 Customers - like
> 411xxx, where xx is the number of account set buy the taxpayer**"

**`InvoiceType` (S.I.9) — doar 6 coduri admise.** Nota din foaia nomenclatorului
(xlsx, `Nom_Tipuri_facturi` r.3, verbatim):

> „Pentru completarea câmpului se va selecta una din cele **șase** coduri de mai jos:
> - codul **380** - Factură inițială,
> - codul **381** - **Factură storno (factură cu semnul minus indiferent de motivul
>   stornării)**,
> - codul **384** - Factura finală reemisă ca urmare a unei corecții a unei facturi
>   inițiale sau facturi storno,
> - codul **389** - Autofactură (indiferent de situația care a generat emiterea
>   autofacturii)
> - codul **751** - Factură - informații în scopuri contabile
> - codul **575** - Factura asiguratorului"

Foaia conține 43 de coduri UNTDID 1001 — restul de 37 sunt informative.

**`CustomerInfo` / `SupplierInfo`** (nota [5], fragment XSD extras integral):
`CustomerInfo` = `xs:choice(CustomerID | Name minOccurs=0)` + **`BillingAddress`
(AddressStructure) obligatoriu, în afara choice-ului**. Identic pentru
`SupplierInfo`.

⚠️ Validatorul respinge explicit ramura `Name` (`Invoice.class`, verbatim):

- `Pentru elementul 'Name' s-a completat valoarea [@0@]. Acest element nu trebuie folosit. Va rugam sa folositi CustomerID!`
- `Pentru elementul 'Name' s-a completat valoarea [@0@]. Acest element nu trebuie folosit. Va rugam sa folositi SupplierID!`

#### `InvoiceLine` (S.I.29–49)

| ID | Element | Tip | O/Opt | Regulă |
|---|---|---|---|---|
| S.I.29 | `LineNumber` | shorttext | Opt | |
| S.I.30 | `AccountID` | middle2 | **M** | plan de conturi |
| S.I.31 | `Analysis` | AnalysisStructure | Opt 0..* | |
| S.I.32 | `OrderReferences` | — | Opt 0..* | `OriginatingON` + `OrderDate` |
| S.I.33/34 | `ShipTo` / `ShipFrom` | — | Opt | |
| S.I.35 | `GoodsServicesID` | SAFcodeType | Opt | `01` bunuri / `02` servicii |
| S.I.36/37 | `ProductCode` / `ProductDescription` | — | Opt | |
| S.I.38 | `Delivery` | — | Opt | `xs:choice(MovementReference 1..* \| DeliveryDate \| DeliveryPeriod{FromDate,ToDate})` — nota [6] |
| S.I.39 | `Quantity` | SAFquantityType | **M** | |
| S.I.40 | `InvoiceUOM` | SAFcodeType | Opt | `Unitati_masura` |
| S.I.41 | `UOMtoUOMBaseConversionFactor` | Decimal | Opt | „Only needed when InvoiceUOM is reported and if different from the UOMBase" |
| S.I.42 | `UnitPrice` | monetary | **M** | SYN: „Unit price per one unit of measure în the default currency from the header. […] **This amount may be positive or negative amount**" |
| S.I.43 | `TaxPointDate` | Date | **M** | „Tax point date where recorded or **if not recorded then the Invoice date**" |
| S.I.44 | `References` → `CreditNote` | — | Opt | `Reference` („Credit note reference (where applicable) to original invoice") + `Reason` |
| S.I.45 | `Description` | longtext | **M** | |
| S.I.46 | `InvoiceLineAmount` | AmountStructure | **M** | „Amount for transaction **excluding taxes and freight charges**" |
| S.I.47 | `DebitCreditIndicator` | SAFcodeType | **M** | SYN: „Restriction for values: **C** for Credit and **D** for Debit". DESC: „Entry must correspond to entry reflected in General Ledger Entry. **Signing of lineamounts is relative to this indicator.** E.g. a return can lead to a negative aamount" |
| S.I.48 | `ShippingCostsAmount` | AmountStructure | Opt | |
| S.I.49 | `TaxInformation` | TaxInformationStructure | **M, 1..*** | |

`InvoiceSettlement` (S.I.60–63): `SettlementDiscount` (Opt),
**`SettlementAmount` (M)**, `SettlementDate` (Opt), `PaymentMechanism` (Opt).

`InvoiceDocumentTotals` (S.I.64–67): `TaxInformationTotals` (Opt 0..*, „Control
totals tax payable information. Per Tax Type/ Tax Code the Tax Base and Tax Amount
are summarized"), `ShippingCostsAmountTotal` (Opt),
**`NetTotal` (M** — „Control total sales value **excluding tax and shipping
costs**"**)**, **`GrossTotal` (M** — „Control total amount **including tax and
shipping costs**"**)**.

#### 4.3 Payments

`NumberOfEntries` + `TotalDebit` + `TotalCredit` (M, 0..1) + `Payment` (0..*).

`Payment` (SD.P.5–17): **`Payment ref no.` (M, middle1)**, `Period` / `Period Year`
(Opt), `TransactionID` (Opt), **`Transaction Date` (M)**, **`PaymentMethod` (M,
shorttext)**, **`Description` (M, longtext)**, `Batch ID` / `System ID` / `Source ID`
(Opt), **`Line` (M, 1..*)**, `PaymentSettlement` (Opt 0..1), `PaymentDocumentTotals`
(Opt 0..1).

`PaymentLine` (SD.P.18–28): `LineNumber` (Opt), `SourceDocumentID` (Opt),
**`AccountID` (M)**, `Analysis` (Opt 0..*), **`CustomerID` (M)**,
**`SupplierID` (M)**, `TaxPointDate` (Opt), `Description` (Opt),
**`DebitCreditIndicator` (M, `C`/`D`)**, **`PaymentLineAmount` (M, AmountStructure)**,
**`TaxInformation` (M, 1..*)**.

`PaymentSettlement` (SD.P.29–32): `SettlementDiscount`, `SettlementAmount`,
`SettlementDate`, `PaymentMechanism` — toate Opt.
`PaymentDocumentTotals` (SD.P.33–35): `TaxInformationTotals` (Opt 0..*), `Net Total`
(Opt), **`Gross Total` (M)**.

**Regula `PaymentMethod` ↔ `PaymentMechanism`** — cea mai concretă regulă de
corelare din toată schema.

SD.P.10 `PaymentMethod`, COM verbatim:

> „Filled in according to the following CODES: - **01** - for Cash - **02** - for
> Offset/ Netting - **03** - for Non-Cash - **98** - for Mutually defined - **99** -
> for Instrument not defined"

SD.P.10 SEM și SD.P.32 SEM (identice), verbatim:

> „Also, the value of SD.P.10 **must be correlated** to the value of SD.P.32 such
> that they **match with the tuples defined by the nomenclature**.
> Example: **PaymentMethod = „99" can only work with PaymentMechanism = „1"**"

SD.P.32 / S.I.63 `PaymentMechanism`, COM verbatim:

> „For this field is used the following codes: - code **10** - for cash payment used
> at the trezorery at registers - code **97** - for Clearing between partners
> (offset/ netting). For payments non-cash, use the following codes used by NAFA as
> payment channels: - code **42** - Payment to bank account - codul **48** - Bank
> card - codul **20** - Cheque - codul **68** - Online payment service.
> The other codes in the nomenclature Nom_Mecanisme_plati will be used when the
> payment channels will be available at NAFA."

Nota RO din foaia nomenclatorului (`Nom_Mecanisme_plati` r.2, verbatim):

> „Pentru completarea PaymentMethod se vor selecta codurile asociate de mai jos:
> - codul 01 - pentru Numerar
> - codul 02 - pentru Compensare
> - codul 03 - pentru Fără numerar
> - codul 98 - pentru Definit de comun acord
> - codul 99 - pentru Instrument nedefinit
>
> Pentru completarea câmpului PaymentMechanism se vor selecta, **cu prioritate**,
> coduri din cele de mai jos:
> - codul 10 - pentru plata în numerar, inclusiv pentru plățile efectuate la
>   casieriile trezoreriei
> - codul 97 - pentru Compensarea între parteneri (offset/ netting)
> - codul 42 - pentru plata prin transfer bancar
> - codul 48 - pentru card bancar
> - codul 20 - pentru Cec bancar
> - codul 68 - pentru Serviciul de plată online [Internet Banking]
>
> Celelalte coduri din nomenclatorul Mecanisme de plată vor fi utilizate în momentul
> în care aceste canale de plată vor fi disponibile în cadrul ANAF."

Validatorul impune tuplele:
`Pentru PaymentMechanism [@0@], valoarea PaymentMethod [@1@] nu este permisa!`
(`Payment.class`). Foaia are 83 de mecanisme, fiecare cu `PaymentMethod` asociat pe
coloana 1 (ex.: mecanismul `10` → `01`; `58`/`59` SEPA → `02` Compensare; `20` Cec →
`03`; `ZZZ` → `98`; `1` Instrument not defined → `99`).

#### 4.4 MovementOfGoods

`Number of movement lines` (M, 0..1, sursa „Inventory file"), `Total quantity
received` (M, 0..1), `Total quantity issued` (M, 0..1), `Stock movement` (0..*).

`StockMovement` (SD.MG.5–14): **`Movement reference` (M, middle1)**,
**`Movement date` (M)**, `Movement posting date` (Opt), `Movement posting time`
(Opt, Time), `Tax point date` (Opt, sursa „E.g. Invoice, dispatch note, customs
declaration"), **`Movement type` (M, SAFcodeType — `Nomenclator stocuri`)**,
`Source ID` / `System ID` (Opt), `Document reference` (Opt 0..1 → `Document type` M
+ `Document number` M + `Document line` Opt), **`Line` (M, 1..*)**.

`StockMovementLine` (SD.MG.18–33): **`Line number` (M)**, **`AccountID` (M)**,
`TransactionID` (Opt), **`CustomerID` (M)**, **`SupplierID` (M)**, `Ship to` /
`Ship from` (Opt), **`Product code` (M)**, `Stock account no` (Opt),
**`Quantity` (M)**, **`UnitOfMeasure` (M)**,
**`UOMToUOMPhysicalStockConversion` (M)** — numele real al elementului în validator
e `UOMToUOMPhysicalStockConversionFactor` (§F.23), `Book value` (Opt),
**`Movement subtype` (M, SAFcodeType)**, `Movement comments` (Opt),
`Tax information` (Opt 0..*).

Notele din foaia `Nomenclator stocuri` (r.24–25): codurile se folosesc **și** la
`MasterFiles/2.8 MovementTypeTable.MovementType` (OBLIGATORIU) **și** la
`SourceDocuments/StockMovement/StockMovementLine.MovementSubType` (OBLIGATORIU).

#### 4.5 AssetTransactions

`Number of asset transactions` (M, card. **1**, sursa „FAR") + `AssetTransaction`
(0..*): `AssetTransactionID` (M), `AssetID` (M),
**`AssetTransactionType` (M — `Nomenclator imobilizari`)**, `Description` (Opt),
`AssetTransactionDate` (M), `Supplier` (Opt 0..1: `Supplier Name` M, `SupplierID` M,
`Postal address` M), `TransactionID` (M, sursa „GL/ERP"),
`AssetTransactionValuations` (M) → `AssetTransactionValuation` (1..*):
`AssetValuationType` (Opt), `AcquisitionAndProductionCostOnTransaction` (M),
`BookValueOnTransaction` (M), `AssetTransactionAmount` (M).

### B.6 `Structures` (5.1–5.15) — structurile partajate

| Structură | Câmpuri (M = obligatoriu) |
|---|---|
| **5.1 AddressStructure** | `StreetName` (70), `Number` (18), `AdditionalAddressDetail` (70), `Building` (35), **`City` (35) M**, `PostalCode` (18), `Region` (35), **`Country` (ISO2) M**, `AddressType` (18) |
| **5.2 AmountStructure** | **`Amount` M**, **`CurrencyCode` M**, **`CurrencyAmount` M**, `ExchangeRate` (Opt) |
| **5.3 AnalysisStructure** | **`AnalysisType` (SAFcodeType) M**, **`AnalysisID` (longtext) M**, `AnalysisAmount` (AmountStructure, Opt) |
| **5.4 BankAccountStructure** | **`IBANNumber` M**, **`BankAccountNumber` M**, `BankAccountName` (Opt), `SortCode` (Opt) |
| **5.5 CompanyHeaderStructure** | **`RegistrationNumber` M**, **`Name` M**, **`Address` M 1..\***, **`Contact` (ContactHeaderStructure) M 1..\***, `TaxRegistration` (TaxIDStructure, Opt 0..*), **`BankAccount` M 1..\*** |
| **5.6 CompanyStructure** | **`RegistrationNumber` M** (regula prefixelor, §B.4), **`Name` (longtext) M**, **`Address` M 1..\***, `Contact` (Opt 0..*), `TaxRegistration` (Opt 0..*), `Bank Account` (Opt 0..*) |
| **5.7 ContactHeaderStructure** | **`ContactPerson` (PersonNameStructure) M**, **`Telephone` M**, `Fax`, `Email`, `Website` |
| **5.8 ContactInformationStructure** | **`ContactPerson` M**, restul Opt |
| **5.9 HeaderStructure** | S.H.1–13 (§B.1) |
| **5.10 InvoiceStructure** | S.I.1–67 (§B.5) |
| **5.11 PersonNameStructure** | `Title`, **`FirstName` M**, `Initials`, `LastNamePrefix`, **`LastName` M**, `BirthName`, `Salutation`, `OtherTitles` (0..*) |
| **5.12 SelectionCriteriaStructure** | `TaxReportingJurisdiction` (Opt), `CompanyEntity` (Opt), **`xs:choice` (nota [7])**: fie (`SelectionStartDate`, `SelectionEndDate`) fie (`PeriodStart`, `PeriodStartYear`, `PeriodEnd`, `PeriodEndYear`) — toate M; `Document type` (Opt), `Other Criteria` (Opt 0..*) |
| **5.13 ShippingPointStructure** | `DeliveryID`, `DeliveryDate`, `WarehouseID`, `LocationID`, `UCR` (unique consignment reference), `Address` — toate Opt |
| **5.14 TaxIDStructure** | **`TaxRegistrationNumber` M**, `TaxType` (Opt — **`Nomenclator_Regim_fiscal`**), `TaxNumber` (Opt), `TaxAuthority` (Opt, Default **`ANAF`**, SYN „Validation for a single value: reporting to NAFA"), `TaxVerificationDate` (Opt — „date when the VAT registration number was assigned to taxpayer") |
| **5.15 TaxInformationStructure** | **`TaxType` M**, **`TaxCode` M**, `TaxPercentage` (Opt), `TaxBase` (Opt), `TaxBaseDescription` (Opt), **`TaxAmount` (AmountStructure) M**, `TaxExemptionReason` (Opt), `TaxDeclarationPeriod` (Opt) |

`TaxIDStructure.TaxRegistrationNumber` SEM (identic cu S.C.1 COM), verbatim:

> „If the taxpayer is registered for VAT, set to VAT Registration Number **without RO
> prefix**; for **PFA set to CNP**; for CUI Supplier / foreign customer set to **VAT
> code used for the issued invoice**."

**Regula-cheie a lui `TaxInformation`** — S.TI.1 SYN + COM, verbatim:

> „Validation based on TAX_IMP - Impozite. For transactions registered in
> **GeneralLedgerEntries** section which are **not relevant to be reported for
> taxs**, it will be used **TaxType 000 and TaxCode 000000**. For **import
> operations**, under the **PurchaseInvoice** subsection, when registering an import
> invoice, use **TaxType 000 (3 zeros) and TaxCode 000000 (6 zeors)**"

S.TI.2 `TaxCode` SYN, verbatim:

> „Validation based on Legenda coduri taxa: - Livrari - Achizitii ded 100% -
> Achizitii ded 50%_baserate - Achizitii ded 50%_not_known - Achizitii ded 50% -
> Achizitii neded - Achizitii baserate - Achizitii not known - **WHT - nomenclator**
> - **TVA_NoteContabile** - Achizitii neded 50% **or 000000 (six zero's) for the
> other TaxType's than VAT** (all the classes above) **and WHT**"

---

## C. Regulile de validare

### C.1 Ce impune efectiv validatorul

Mesajele de mai jos sunt **verbatim** din constant-pool-ul claselor din
`D406Validator.jar` (`@0@`, `@1@`… = placeholderele de format ale validatorului):

| Clasă | Mesaj | Ce verifică |
|---|---|---|
| `AuditFile` | `Pentru RegistrationNumber @0@ formatul este invalid. Primele doua caractere nu sunt 'RO'. Primele doua caractere trebuie sa fie 'RO' urmate de codul numeric sau campul trebuie sa contina doar codul numeric.` | Header.Company.RegistrationNumber |
| `AuditFile` | `Pentru RegistrationNumber @0@ formatul este invalid. Codul nu este numeric. […]` | idem |
| `AuditFile` | `elementul 'TotalSegmentsInSequence' [@0@] ar fi trebuit sa fie mai mare decat SegmentIndex [@1@]` | coerența segmentării (§F.13) |
| `Customer` | `RegistrationNumber nu poate fi ”0” (zero)` | |
| `Customer` | `Pentru RegistrationNumber @0@ formatul este invalid` | prefixele 00–11 |
| `Customer` | `CustomerID nu poate fi ”0” (zero)` | |
| `Customer` | `Pentru CustomerID @0@ formatul este invalid` | idem, în MasterFiles |
| `AssetSupplier` | `SupplierID nu poate fi ”0” (zero)` / `Pentru SupplierID @0@ formatul este invalid` | |
| `Owner` | `OwnerID nu poate fi ”0” (zero)` / `Pentru OwnerID @0@ formatul este invalid` | |
| `BankAccount` | `Pentru IBANNumber '@0@' formatul este invalid` | mod-97 |
| `BankAccount` | `Pentru IBANNumber '@0@' codul tarii @1@ nu face parte din lista` | |
| `BankAccount` | `Pentru IBANNumber '@0@' formatul este invalid pentru codul tarii '@1@'` | masca per țară, din `resources/IBANFormat_v0..v3.properties` |
| `Invoice` | `Pentru elementul 'Name' s-a completat valoarea [@0@]. Acest element nu trebuie folosit. Va rugam sa folositi CustomerID!` (+ varianta SupplierID) | ramura `Name` din `xs:choice` e de facto interzisă (§F.14) |
| `Payment` | `Pentru PaymentMechanism [@0@], valoarea PaymentMethod [@1@] nu este permisa!` | tuplele metodă ↔ mecanism |
| `StockMovement` | `Pentru MovementPostingTime @0@ formatul trebuie sa fie HH:mm.` | |
| `MasterFiles` | `Primul modul al unei declaratii trebuie sa contina cel putin o sectiune din MasterFiles!` | raportare modală |
| `TaxInformation` | `Nu sunt acceptate valori NaN` | |
| `ValidatorExtension1Impl` | `ID-ul contului [@0@] trebuie sa se gaseasca in planul de conturi.` | `validateAccountId`, contra planului ales de `TaxAccountingBasis`. Câmpurile alăturate `sinteticAccounts` și `getListOfSinteticAccountsIdsThatStartWith` ⇒ potrivirea se face **și pe sintetic, prin prefix** |
| `ValidatorExtension1Impl` | `Suma (LEI) este @0@, care este diferita de CurrencyAmount x ExchangeRate -> diferita de @1@ x @2@ = ` | **`validateAmount`: `Amount == round(CurrencyAmount × ExchangeRate)`** — singura regulă aritmetică explicită găsită |
| `ValidatorExtension1Impl` | `Tipul declaratiei @0@ nu corespunde cu perioada declarata: @1@.@2@ - @3@.@4@` | `validateDeclaredPeriod`: `HeaderComment` ↔ `SelectionCriteria` |
| `ValidatorExtension1Impl` | `Pentru tara '@0@' codul regiunii nu trebuie completat. Stergeti codul regiunii: @1@.` | **`Region` NUMAI pentru RO** |
| `ValidatorExtension1Impl` | `Pentru tara '@0@' codul regiunii @1@ nu face parte din lista` | `additionalCheckRegion`, contra `_ISO_3166_2_RO` |

Metode și câmpuri vizibile în `ValidatorExtension1Impl`: `validateAccountId`,
`validateAmount`, `validateDeclaredPeriod`, `additionalCheckRegion`,
`lengthOfRomanianId`, `getSpecialCharacterCount`, `countryPrefix`, `restRestOfId`,
`taxAccountBasis`, `sinteticAccounts`, `countries_ISO3166`, `countryCodIso`,
`_ISO_3166_2_RO`, `taxAccountingBasisNomenMap`,
`AUDIT_FILE_TYPE{EXTERNAL_MONTHLY, EXTERNAL_QUATERLY}`.
Constante literale: `080000000000000`, regex-ul `[^A-Za-z0-9]` (verificarea „fără
caractere speciale" pentru prefixul `04`).

**Structura de secțiuni enumerată de validator** (`ValidatorExtensionImpl$SECTION_ELEMENTS`)
— XPath-urile exacte, utile ca listă de control la generare:

```
AuditFile/Header/:HeaderStructure
AuditFile/MasterFiles/GeneralLedgerAccounts/Account
AuditFile/MasterFiles/Taxonomies/Taxonomy
AuditFile/MasterFiles/Customers/Customer
AuditFile/MasterFiles/Suppliers/Supplier
AuditFile/MasterFiles/TaxTable/TaxTableEntry
AuditFile/MasterFiles/UOMTable/UOMTableEntry
AuditFile/MasterFiles/AnalysisTypeTable/AnalysisTypeTableEntry
AuditFile/MasterFiles/MovementTypeTable/MovementTypeTableEntry
AuditFile/MasterFiles/Products/Product
AuditFile/MasterFiles/PhysicalStock/PhysicalStockEntry
AuditFile/MasterFiles/Owners/Owner
AuditFile/MasterFiles/Assets/Asset
AuditFile/GeneralLedgerEntries/NumberOfEntries
AuditFile/GeneralLedgerEntries/TotalDebit
AuditFile/GeneralLedgerEntries/TotalCredit
AuditFile/GeneralLedgerEntries/Journal
AuditFile/SourceDocuments/SalesInvoices/NumberOfEntries
AuditFile/SourceDocuments/SalesInvoices/TotalDebit
AuditFile/SourceDocuments/SalesInvoices/TotalCredit
AuditFile/SourceDocuments/SalesInvoices/Invoice
AuditFile/SourceDocuments/PurchaseInvoices/NumberOfEntries
AuditFile/SourceDocuments/PurchaseInvoices/TotalDebit
AuditFile/SourceDocuments/PurchaseInvoices/TotalCredit
AuditFile/SourceDocuments/PurchaseInvoices/Invoice
AuditFile/SourceDocuments/Payments/NumberOfEntries
AuditFile/SourceDocuments/Payments/TotalDebit
AuditFile/SourceDocuments/Payments/TotalCredit
AuditFile/SourceDocuments/Payments/Payment
AuditFile/SourceDocuments/MovementOfGoods/NumberOfMovementLines
AuditFile/SourceDocuments/MovementOfGoods/TotalQuantityReceived
AuditFile/SourceDocuments/MovementOfGoods/TotalQuantityIssued
AuditFile/SourceDocuments/MovementOfGoods/StockMovement
AuditFile/SourceDocuments/AssetTransactions/NumberOfAssetTransactions
AuditFile/SourceDocuments/AssetTransactions/AssetTransaction
```

**`Parameters_v0..v3`** = patru snapshot-uri de nomenclator, fiecare cu resursele
`Nomen_vN.properties`, `NC8_2022_TARIC3_vN.properties`, `IBANFormat_vN.properties`,
selectate după anul din parametrul `an=`. `Nomen_vN.properties` conține exact 12
chei, cu numărul de valori măsurat:

| Cheie | Valori |
|---|---|
| `payments` | 83 |
| `plan_conturi_bal_soc_com` | 635 |
| `plan_conturi_invoice_I` | 3.103 |
| `plan_conturi_banci` | 1.546 |
| `plan_conturi_soc_asigurari` | 1.850 |
| `plan_conturi_ifrs` | 757 |
| `plan_conturi_n39` | 761 |
| `plan_conturi_ifn` | 1.546 |
| `plan_conturi_n36` | 518 |
| `plan_conturi_norma14` | 647 |
| `plan_conturi_ONG` | 690 |
| `plan_conturi_BNR6` | 833 |

### C.2 Ce spune schema despre totaluri — și ce NU spune

- `NumberOfEntries` / `TotalDebit` / `TotalCredit` (GL.1–3) și echivalentele pe
  SalesInvoices (SD.SI.1–3), PurchaseInvoices (SD.PI.1–3), Payments (SD.P.1–3), plus
  `NumberOfMovementLines` / `TotalQuantityReceived` / `TotalQuantityIssued`
  (SD.MG.1–3) și `NumberOfAssetTransactions` (SD.AT.1) — **există, sunt marcate
  Mandatory, dar schema NU enunță nicio regulă de egalitate cu suma liniilor**.
  Coloanele „Syntactic/Semantic Validation Rules" pentru ele conțin doar formatul
  zecimal („Decimal number, decimal point delimited, maxim 2 (two) digits after
  decimal point delimitator").
- `TotalDebit`/`TotalCredit` **pot fi negative** — SYN: „This amount may be positive
  or negative. Negative amounts are profixed with the minus („-") sign". Deci nu
  sunt sume de valori absolute.
- Validatorul are pentru fiecare câte o intrare în `SECTION_ELEMENTS` cu XPath — dar
  aceea e **detecția de secțiuni pentru raportarea modală**, nu o verificare
  aritmetică. **Nu există niciun mesaj de eroare de tip „total ≠ sumă".**
- **Concluzie**: pe baza surselor locale nu se poate afirma că DUK verifică
  `TotalDebit == Σ DebitAmount`. Singura verificare aritmetică găsită e
  `Amount == CurrencyAmount × ExchangeRate`. De măsurat empiric (§F.4).

### C.3 Formate

- **Date**: `xs:date`, ISO 8601, `AAAA-LL-ZZ`; exemplele date de ANAF: `2020-10-03`,
  `2021-04-12`. **Ore**: `HH:MM`, 24h (`01:25`, `15:03`).
- **Zecimale**: separator **punct**, fără separator de mii, fără notație
  științifică. Nota din `6. SimpleTypes`, verbatim: „The decimal fractional numbers
  are represented always with **DECIMAL POINT SEPARATOR** - „." !! this is an XML
  technical standard - **we do not change this in order to keep the information in
  the system EXACTLY as reported by taxpayers**."
- **Negative**: semnul `-`; la `SAFmonetaryType` și parantezele `()`.
- **Coduri** (`Nomenclator stocuri` nota 4, verbatim): „Aceste coduri sunt definite
  într-un câmp cu maxim 9 (nouă) caractere alfanumerice. Codurile se completează în
  bazele de date **aliniat la DREAPTA**, cu umplerea cu spații la partea STÂNGĂ. **În
  fișierul XML se comunică doar caracterele diferite de „spațiu"**."
- **Cod în afara nomenclatorului** (nota 5, verbatim): „Completarea acestui câmp cu
  valori diferite de cele din lista de mai sus - conduce la semnalarea unei **erori
  fatale, cu rejectarea declarației informative D406**."
- **Coduri de taxă INACTIVE** ⇒ **avertisment, nu eroare**
  (`Centralizator_Nomenclatoare`, verbatim): „codurile marcate ca inactive, nu sunt
  utilizate în prezent în raportarea SAF-T, iar raportarea anumitor tranzacții
  folosind codurile de taxă inactivă va fi **semnalată cu avertisment, și nu
  eroare**, pentru a permite depunerea unor **declarații rectificative pentru
  perioade trecute** de timp, de exemplu: pentru o perioadă în care cota de TVA a
  fost 24%".
- **Avertismentele nu blochează** (`doc/IntrebariFrecvente.rtf`, verbatim): „O
  declaratie care are numai atentionari este considerata **valida**. Sunt atentionari
  si nu erori si pentru faptul ca respectivele reguli se aplica in marea majoritate a
  cazurilor dar exista si exceptii pentru care respectiva regula nu mai trebuie
  respectata."

### C.4 Grupurile `xs:choice` (notele [1]–[7])

Singurele fragmente de XSD din tot kitul, extrase integral din notele de subsol ale
xlsx-ului:

| # | Locul | Alegerea |
|---|---|---|
| [1] | `MasterFiles/GeneralLedgerAccounts/Account`, `.../Customers/Customer`, `.../Suppliers/Supplier` | `OpeningDebitBalance` \| `OpeningCreditBalance`; apoi `ClosingDebitBalance` \| `ClosingCreditBalance` |
| [2] | `MasterFiles/TaxTable/TaxTableEntry` (TaxCodeDetails) | `TaxPercentage` \| `FlatTaxRate` |
| [3] | `MasterFiles/Assets/Asset/Valuations` | `AssetLifeYear` \| `AssetLifeMonth` |
| [4] | `/Journal/Transaction/TransactionLine` | `DebitAmount` \| `CreditAmount` |
| [5] | `InvoiceStructure` | `CustomerInfo` \| `SupplierInfo`; **în fiecare**: `xs:choice(CustomerID \| Name minOccurs=0)` urmat de `BillingAddress` (obligatoriu, în afara choice-ului) |
| [6] | `InvoiceStructure/InvoiceLine` (Delivery) | `MovementReference` (`maxOccurs="unbounded"`) \| `DeliveryDate` \| `DeliveryPeriod` |
| [7] | `SelectionCriteriaStructure` | (`SelectionStartDate`,`SelectionEndDate`) \| (`PeriodStart`,`PeriodStartYear`,`PeriodEnd`,`PeriodEndYear`) |

Documentația XSD a notei [7]: „Allows for a choice between selection on calendar
dates and periods according to the accounting system, e.g. 1 to 12 for a 12-months
accounting system."

### C.5 Rularea validatorului

```bat
cd D:\Dev\Atlas.Conta\anaf\duk_SAFT_an_luna\dist
jre8\bin\java.exe -jar DUKIntegrator.jar -d -v D406 D:\tmp\saft.xml D:\tmp\erori.txt

:: varianta an/lună
java -Xms250m -Xmx4g -jar DUKIntegrator_AnLunaUI.jar -v D406 d:\d406.xml $ $ an=2025 luna=8
```

- `tipDeclaratie` = `D406` (producție) sau `D406T` (test).
- `fisierRezultat` conține erorile și atenționările, sau **`ok`** dacă nu există
  niciuna. Implicit `<fisierXML>.err.txt`. **Ăsta e semnalul de automatizat.**
- `$` = „sari peste parametrul opțional, ia valoarea implicită".
- Prefixul `!` pe numele fișierului de erori păstrează atenționările separat în
  `.wrn.txt`, iar `.err.txt` primește `ok`. Prefixul `+` adaugă `ok` după
  atenționări. **Fără prefix, atenționările se pierd.**
- `-d` dezactivează auto-update-ul; `-c` se poate folosi **numai împreună cu `-d`**.
- La declarațiile fără ZIP atașat, parametrul `fisierZIP` trebuie să fie `0`.
- „Va recomandam sa evitati dezarhivarea si folosirea acestui kit dintr-o cale care
  contine **spatii** in numele directoarelor" (`CITESTE-MA.TXT`).
- Există clasa publică `Integrator` în `DUKIntegrator.jar`, gândită pentru apel din
  aplicații (sub AGPL, fiindcă folosește iText).

---

## D. Ce exportă 1C efectiv

Sursa acestei secțiuni: `1C/02-saft-export.md` (886 linii, citit integral; analiza
codului din `D:\Dev\Work\EServices\`, datată 2026-07-23).

### D.1 Arhitectura

Două implementări, cu maturitate foarte diferită:

1. **Export SQL — folosit în producție.** ~34 scripturi T-SQL în
   `EServices.Web\Migrations\DefaultDB\Saf-T\`, instalate ca proceduri/funcții printr-o
   migrație FluentMigrator. Două faze net separate:
   **(a) populare** — 8 proceduri `spSAFT*` citesc din view-urile `flax.*` (proiecția
   relațională a bazei 1C) și scriu în **30 de tabele de staging** din schema `saft`;
   **(b) serializare** — `spExportSafT` + 10 funcții `fnExportSAFT*` produc XML direct
   din staging cu `FOR XML AUTO, ELEMENTS, TYPE`.
   Punct de apel: `Modules\SafT\Header\HeaderEndpoint.cs:98` `GenerateHeader(An, Luna)`
   (job de fundal, `commandTimeout: 600`) + `:118` `ExportXML(HeaderID)` (sincron).
2. **`SafTProvider` — prototip C#, nevalidat.** Clase generate din XSD cu
   `XmlSchemaClassGenerator` + Dapper multi-mapping peste **aceleași tabele de
   staging**.

Lanțul complet:
`1C (_Reference<N>/_Document<N>) → view-uri flax.* → 30 tabele saft.* → FOR XML AUTO
→ post-procesare C# → fișier`.

Post-procesarea (`HeaderEndpoint.cs:126-133`): `RemoveDiacritics` (normalizare FormD,
eliminare `NonSpacingMark`, re-FormC) → `Regex.Replace(xml, "(</?)([A-Za-z][^ />]*)",
"$1nsSAFT:$2")` (prefixează **toate** tag-urile) → înlocuirea rădăcinii cu declarația
XML + `xmlns:nsSAFT="mfp:anaf:dgti:d406:declaratie:v1"`. Fișier
`SafT-{an}-{luna:D2}.xml`, întors ca `FileContentResult`, nepersistat.

Parametrizare: doar (an, lună) — luna calendaristică. `HeaderComment='L'`,
`SegmentIndex=1`, `TotalSegmentsInSequence=1`. **Fără streaming, fără segmentare** —
„documentul se materializează integral ca string în memoria procesului web, apoi se
aplică un `Regex.Replace` peste tot conținutul".

### D.2 Acoperirea reală

| Stare | Secțiuni |
|---|---|
| **Se raportează** | Header, GeneralLedgerAccounts, Customers, Suppliers, TaxTable, UOMTable, AnalysisTypeTable, Products, GeneralLedgerEntries, SalesInvoices, PurchaseInvoices, Payments |
| **Element gol (`''`)** | MovementTypeTable, Owners, Assets, MovementOfGoods |
| **Nu se emite deloc** | **PhysicalStock** (deși `saft.PhysicalStock` **e populat** de `spSAFTProduct`), AssetTransactions, Taxonomies |

Proceduri invocate doar în comentarii, inexistente (`spGenerareSAFT.sql:79-98`):
`spSAFTTaxonomy`, `spSAFTTaxTable`, `spSAFTOwners`, `spSAFTAssets`,
`spSAFTMovementOfGoods`, `spSAFTAssetTransactions`.
Cod mort instalat în producție: `spSAFTCustomer`, `spSAFTSupplier`.

### D.3 Tabel-nucleu: secțiune → procedură → staging → sursă 1C

| Secțiune | Populare | Serializare | Staging | Sursă `flax.*` |
|---|---|---|---|---|
| Header/Company | `spSAFTHeader` + `fnDetaliiSocietate` | inline | `saft.Header` | `Organizatii`, `InfoRg_InformatiaDeContact`, `InfoRg_PersoaneResponsabileDinOrganizatia`, `ConturiBancare` |
| GeneralLedgerAccounts | `spSAFTAccount` + `fnSAFTAccount` | `fnExportSAFTAccount` | `saft.Account` | `PlanConturi`, `Balanta`; + `dbo.PlanCont`, `saft.AccountMaps` |
| Customers / Suppliers | `spSAFTParteneri` + `vwDetaliiPartener`, `fnBalantaPerioadaCont` | `fnExportSAFTCustomers` / `…Suppliers` | `saft.Partener`, `saft.PartenerBalance` | `Partenerii`, `InfoRg_InformatiaDeContact`, `PersoaneDeContact`, `ConturiBancare`, `Tari`, `BalantaNivel3` |
| TaxTable | — | `fnExportSAFTTaxType` | — | `dbo.TaxCode`, **listă fixă de 8 coduri** |
| UOMTable | — (derivat) | `fnExportSAFTUOM` | `saft.Product` group by `UOMBase` | — |
| AnalysisTypeTable | — | `fnExportSAFTAnalysisType` | — | **constantă** `A / 000 / OPERATIUNI NEINCADRATE SAF-T` |
| Products / PhysicalStock | `spSAFTProduct` | `fnExportSAFTProduct` / **neexportat** | `saft.Product`, `saft.PhysicalStock` | `Nomenclator`, `UMNomenclator`, `UM`, `BalantaNivel3` |
| GeneralLedgerEntries | `spSAFTTransaction` | `fnExportSAFTJurnal` | `saft.Transaction`, `saft.TransactionLine` | `NoteContabile`, `DefalcareNote`, `FullTables` + 10 tabele de documente |
| SalesInvoices | `spSAFTSaleInvoice` | `fnExportSAFTInvoices(@headerID,1)` | `saft.Invoice(IsSell=1)`, `saft.InvoiceLine` | `VanzareMarfuriSiServiciiPrestate` + `_Marfuri/_Servicii/_Imobilizari/_FacturiAvans/_Reduceri`; `ReturDeLaClient` + … |
| PurchaseInvoices | `spSAFTPurchaseInvoice` | `fnExportSAFTInvoices(@headerID,0)` | idem `IsSell=0` | `AprovizionareMarfuriSiServiciiPrimite` + …; `ReturLaFurnizor` + … |
| Payments | `spSAFTPayments` | `fnExportSAFTPayments` | `saft.Payment`, `saft.PaymentLine` | `Plata`, `Plata_Details` |

### D.4 Câmpurile obligatorii extrase din `[Required]` al claselor XSD-generate

| Structură | Obligatorii (XSD, versiunea din 2023) |
|---|---|
| `Account` | AccountID, AccountDescription, AccountType |
| `Customer` / `Supplier` | CustomerID/SupplierID, AccountID |
| `AddressStructure` | **City, Country** (restul opționale) |
| `Product` | ProductCode, Description, **ProductCommodityCode**, UOMBase, UOMStandard, UOMToUOMBaseConversionFactor |
| `PhysicalStockEntry` | WarehouseID, ProductCode, ProductType, StockAccountCommodityCode, **OwnerID**, UOMPhysicalStock, UOMToUOMBaseConversionFactor, UnitPrice, Opening/ClosingStockQuantity, Opening/ClosingStockValue, StockCharacteristics |
| `Transaction` | TransactionID, Period, PeriodYear, TransactionDate, Description, **CustomerID, SupplierID**, SystemEntryDate, GLPostingDate, TransactionLine |
| `TransactionLine` | RecordID, AccountID, **CustomerID, SupplierID**, Description, TaxInformation |
| `InvoiceStructure` | InvoiceNo, AccountID, InvoiceDate, InvoiceType, SelfBillingIndicator, InvoiceLine |
| `InvoiceLine` | AccountID, Quantity, UnitPrice, Description, InvoiceLineAmount, DebitCreditIndicator, TaxInformation |
| `Payment` | PaymentRefNo, TransactionDate, PaymentMethod, Description, PaymentLine |
| `PaymentLine` | AccountID, **CustomerID, SupplierID**, DebitCreditIndicator, PaymentLineAmount, TaxInformation |
| `StockMovement` | MovementReference, MovementDate, MovementType, StockMovementLine |
| `StockMovementLine` | LineNumber, AccountID, **CustomerID, SupplierID**, ProductCode, Quantity, UnitOfMeasure, UOMToUOMPhysicalStockConversionFactor, MovementSubType |
| `Asset` | AssetID, AccountID, Description, DateOfAcquisition, StartUpDate |

Observația structurală (`02-saft-export.md` §3.1, verbatim):

> „**`CustomerID` și `SupplierID` sunt obligatorii AMBELE** pe `TransactionLine`,
> `PaymentLine` și `StockMovementLine`. Implementarea rezolvă asta punând
> identificatorul societății proprii (`'00' + TaxRegistrationNumber`) pe latura care
> nu e a terțului […] **E o convenție de raportare, nu un artefact 1C: se transferă
> identic.**"

### D.5 Mapările efective

**Plan de conturi** (§5.1) — trei mecanisme suprapuse:

- `fnSAFTAccount(@cont)`: elimină punctele (`302.4` → `3024`), apoi
  `select top 1 … from saft.AccountMaps where @cont like sourceAccount` — pattern
  LIKE, **`top 1` fără `order by`** ⇒ nedeterminist la potriviri multiple. Fallback =
  contul curățat. Tabela e seed-uită cu ~20 de rânduri (`131→4758`, `2111→21111`,
  `4111→41111`, `4311→43111`) ⇒ **maparea plan intern → plan ANAF e DATE, cu fallback
  identitate**.
- `StandardAccountID` (`spSAFTAccount.sql:18`):
  `select top 1 Account from PlanCont where a.AccountID like Account + '%' order by len(Account) desc`
  = cel mai lung prefix din planul standard.
- Filtre: se exclud conturile care încep cu `'9'` (clasa 9 de gestiune internă 1C) și
  cele fără nicio mișcare (`soldInitial`, `soldFinal`, `rulajDebit`, `rulajCredit`
  toate 0).
- `AccountType` din `Kind`: `0→'Activ'`, `1→'Pasiv'`, `2→'Bifunctional'`, altfel `'X'`.

**Solduri** (§5.2): `spSAFTAccount` citește `flax.Balanta` (nivel 1) pe
`Period = SelectionStartDate`; `fnBalantaPerioadaCont(@Period, @listaConturi)`
citește `flax.BalantaNivel3` **grupat pe `Valoare1_Id`** (nivelul care poartă
partenerul, pentru conturile de terți, sau nomenclatorul, pentru cele de stoc);
`SoldOnDebit = (Kind = 0) or (Kind = 2 and soldInitial > 0)`;
`soldFinal = soldIni + rulajDebit − rulajCredit` — **sold calculat, nu citit**.

> „**Sursele nu sunt reconciliate prin construcție**: soldurile vin din balanța 1C,
> iar `GeneralLedgerEntries` din `flax.NoteContabile`. Fișierul
> `VerificareBalante.sql` există exact pentru a compara cele două."

**Partenerul** (§5.3), din `vwDetaliiPartener.sql:88-103`:

| Prefix | Condiție | Conținut |
|---|---|---|
| `01` | primele 2 caractere ale CUI sunt cod ISO de țară valid | `01` + cod TVA intracomunitar |
| `03` | `fnIsValidCNP(cui) = 0` (PFA / II înregistrate cu CNP) | `03` + CNP |
| `00` | `fnIsValidCUI(cui) = 0` | `00` + CUI (fără prefixul `RO`) |
| `04` | orice altceva, **și toate persoanele fizice** | `04` + id intern derivat din codul 1C |

- Persoanele fizice primesc **deliberat** `04`: linia `'03' + CNP` e **comentată**,
  cu nota „Dacă se dorește raportarea CNP-ului" (`vwDetaliiPartener.sql:92-94`).
  „Decizie de confidențialitate, transferabilă ca **politică**."
- `TaxRegistrationNumber` și `TaxNumber` se emit **doar** la prefixul `00`.
- **Client vs. furnizor nu e atribut al partenerului**, ci al contului de sold:
  `fnExportSAFTCustomers` filtrează
  `left(AccountID,2)='41' or left(AccountID,3) in ('461','472','478')`;
  `fnExportSAFTSuppliers` filtrează `'40'` / `('462','471')`. Un partener cu ambele
  tipuri de sold apare în ambele secțiuni, câte o dată per cont.
- **Fallback-uri obligatorii**: `PartenerID`/`RegistrationNumber` → `'040000'`,
  `Name` → `'Implicit'`, `StreetName` → `'Nespecificat'`, `City` →
  `'Nespecificat'`/`'Nedeclarat'`, `Country` → `'RO'`, `AddressType` →
  `'StreetAddress'`.
- **Suprimare condiționată la serializare**: `Address` doar dacă `City` și `Country`
  nenule; `Contact` doar dacă `FirstName` **și** `LastName` nenule; `TaxRegistration`
  doar dacă `TaxRegistrationNumber` nenul; `BankAccount` doar dacă `IBANNumber` valid
  (`fnIsValidIBAN = 1`, se preferă contul în Lei).
- `Region` = mapare hardcodată denumire județ → ISO 3166-2:RO, **42 de valori
  duplicate identic în două fișiere**.
- `idPartener` (baza variantei `04`) derivă din convenția de codificare 1C (lungimi
  7/8/9, prefixe `SER`/`000`/`BV0`/`SED`) — **nu se transferă**.
- **Discrepanță între cele două implementări ale aceleiași reguli**:
  `vwDetaliiPartener.sql:43` folosește `substring(PartenerID, 2, len(PartenerID))`,
  iar `fnDetaliiSocietate.sql:44` folosește `substring(PartenerID, 3, ...)`. Prima
  variantă păstrează al doilea `0` din prefix. Marcat `[presupunere]` = bug.

**TVA — cel mai slab punct** (§5.4). `fnTaxDescription(@TipDoc, @TipOperatiune,
@TipLinie, @CotaTVA, @BaseAmount, @TVA)` **ignoră primii trei parametri** și decide
pe `@CotaTVA`, care e un **string liber din 1C**:

```sql
TaxType       = case when @CotaTVA = N'Neimpozabile' then '000' else '300' end
TaxCode       = case when @CotaTVA = N'Neimpozabile' then '000000'
                     else case when @CotaTVA = 'TVA19' then '310309' else '000000' end end
TaxPercentage = case when @CotaTVA = N'Neimpozabile' then 0.00 else 19.00 end
TaxBase       = case when @CotaTVA = N'Neimpozabile' then 0.00 else @BaseAmount end
```

Consecințe: cota 19% hardcodată (orice altă cotă iese greșit); **taxarea inversă cade
pe `TaxCode='000000'` cu `TaxPercentage=19.00`**; `fnExportSAFTTaxType` emite
`TaxTable` dintr-o **listă fixă de 8 coduri** (`380001`, `301101`, `301301`,
`308302`, `310309`, `380101`, `380200`, `380301`); pe `TransactionLine` TVA-ul e
complet fictiv: `TaxType='000'`, `TaxCode='000000'`, **`TaxPercentage=1.00`**
(„evident zgomot"), `TaxAmount=0.00`.

> „Atlas.Conta e deja net superior aici (decizia 36) […] Nimic de importat din SQL în
> afară de constatarea că **`TaxInformation` e obligatoriu pe fiecare linie**,
> inclusiv pe liniile de notă contabilă și de plată."

**Unități de măsură** (§5.5): `fnSAFTUMByDen(@denumire)` = `CASE` pe denumirea
românească → cod UN/ECE Rec 20: metru / metru liniar → `MTR`, metru patrat → `MTK`,
mc → `MTQ`, Centimetru → `CMT`, set → `SET`, tona → `TNE`, Kilogram → `KGM`, Bucată /
complet → `H87`, litru → `LTR`, ora → `HUR`. Default **`H87`**. Toate liniile de
servicii primesc `H87` necondiționat; `UOMToUOMBaseConversionFactor = 1.00` peste
tot. `UOMTable` = `select distinct UOMBase from saft.Product` ⇒ **nomenclatorul
raportat e derivat din utilizare, nu declarat**.

**Validatoare** (§5.6) — reguli generice RO, de portat ca atare:
`fnIsValidCNP` (13 cifre, cheia `279146358279`, `sum % 11` cu excepția `10 → 1`,
prima cifră în `1..9`, verificarea datei nașterii — secol din prima cifră: 1/2→1900,
3/4→1800, altfel 2000; prefixele 8/9 scutite),
`fnIsValidCUI` (elimină prefixul `RO`/`R`, cere 2–10 cifre, cheia `753217532`
aplicată pe revers, `(sum * 10) % 11` cu `10 → 0`),
`fnIsValidIBAN` (tabel de lungimi per țară, ~80 de țări, + mod-97 cu rotația primelor
4 caractere).

**Facturi** (§5.7): fereastra de timp diferă pe direcție — vânzarea filtrează pe
`[DateTime]`, cumpărarea pe `[DataFacturii]` (`spSAFTPurchaseInvoice.sql:43`);
filtre `Posted = 0x01` și `SumaDocument <> 0`; `InvoiceType` doar `'380'`/`'381'`
(returul e un tip de document 1C separat, unificat cu `union`); liniile vin din
**5 sub-tabele** unificate cu `union all` și etichetate prin `LineType`
(`Produs`/`Serviciu`/`Imobilizare`/`FacturaAvans`/`Reducere`), cu
`GoodsServicesID='01'` pentru bunuri și imobilizări; **`DebitCreditIndicator` e
constant pe direcție** (`'C'` la vânzare, `'D'` la cumpărare), nu per linie;
`ContCorespondent` pentru mărfuri are fallback hardcodat `'371.1'`;
`SystemID = convert(varchar(32), KeyField, 2)` (GUID-ul 1C în hex);
`NetTotal`/`GrossTotal` se recalculează la export ca `sum(Amount) − sum(TaxAmount)`
și `sum(Amount)` ⇒ **`Amount` pe linie include TVA-ul**;
`TaxDeclarationPeriod = dateadd(day, 4, @dataStart)` — marcat `[presupunere]`.

**Note contabile** (§5.8): filtru conturi din clasele `1..8` pe **ambele** laturi
(clasa 9 exclusă), `Suma <> 0`; **`JournalID` constant `1`**, `JournalDescription` =
denumirea tipului de document sursă (fallback `'General'`) ⇒ „jurnalul" = tipul
documentului; fiecare rând de notă (D/C pe același rând) se desface în **două**
`TransactionLine` prin cross join cu `(select IsOnDebit = 0 union select 1)`,
`RecordID = (LineNo − 1) * 2 + IsOnDebit + 1`; **recuperarea partenerului e cea mai
fragilă parte**: (1) `arePartener` din prefixul contului, (2) `flax.DefalcareNote`
(`KindRef = N'Parteneri'`), (3) **10 `UPDATE`-uri succesive** peste tipurile de
documente cunoscute (`spSAFTTransaction.sql:90-102`), agregate cu `max(...)` ⇒
**„o tranzacție cu doi parteneri diferiți raportează unul singur"**;
`SystemID = right(..., 18)` pe `Transaction`, `left(SystemID, 18)` pe `Payment` ⇒
**câmpul e limitat la 18 caractere**; la export join-ul pare inversat (`Customer` se
leagă când `isCustomer = 0`), replicat identic în `SafTProvider`
(`Models\Generare\GeneralLedger\GeneralLedger.cs:81-82`) — `[presupunere]` bug
propagat prin copy-paste.

**Plăți** (§5.9): sursa e **numai `flax.Plata` + `flax.Plata_Details`** — **nu se
raportează încasările** (`flax.Incasare` există și e folosită doar pentru recuperarea
partenerului pe note), nici extrasele de cont, nici compensările; ramurile
`Plata_AchitareSalarii` și `Plata_AchitareDariLaStat` sunt comentate;
**`PaymentMethod = '03'` hardcodat**; `DebitCreditIndicator` din prefixul contului
corespondent; `SourceDocumentID = convert(varchar(32), b.DocBaza_Id, 2)` =
documentul stins (imperecherea 1C, la nivel de linie de plată).

**Produse și stoc fizic** (§5.10): universul de produse = balanța conturilor `371%`,
`301%`, `302%` (nivel 3) **plus** produsele apărute în documentele lunii fără
sold/rulaj (patru `union`-uri), cărora li se atribuie solduri 0;
`ProductCode = convert(varchar(32), productID, 2)` (GUID hex), fallback codul intern,
apoi `'P_' + poziție`; `ProductCommodityCode` = `Nomenclator.NIC`, **fallback `0`**;
`ValuationMethod = 'FIFO'` hardcodat; `WarehouseID = N'DEPOZIT'` **hardcodat** (un
singur depozit fictiv), `ProductType = N'Marfa'` hardcodat, **`OwnerID = null`** deși
e `[Required]` în XSD; `UnitPrice` = (sold inițial + rulaj debit) / (cantitate
inițială + cantitate rulaj debit), cu protecție la 0.

### D.6 Bug-uri și fragilități documentate (§9)

**Bug-uri probabile**, marcate ca atare în analiză:

1. `spSAFTProduct.sql:87-88` — `ReturLaFurnizor` join-uit cu
   `flax.ReturDeLaClient_Marfuri`.
2. `vwDetaliiPartener.sql:43` — `substring(PartenerID, 2, ...)` în loc de `3`.
3. `fnExportSAFTJurnal.sql` / `GeneralLedger.cs:81` — `Customer` legat pe
   `isCustomer = 0`.
4. `fnSAFTAccount.sql:12` — `top 1` fără `order by` pe potrivire LIKE.
5. `spSAFTHeader.sql:87` — `from [flax].[Organizatii]` **fără `where`**: cu mai multe
   organizații se inserează mai multe headere.
6. `spSAFTPayments.sql:92-93` — fișierul de instalare conține, **în afara
   procedurii**, `declare @headerID … exec spSAFTPayments @headerID`, care **se
   execută la migrare**.

**Hardcodări care ar deveni bug-uri în alt deployment**:

| Locație | Valoare |
|---|---|
| `spSAFTHeader.sql:30` | `AuditFileRegion = 'RO-DB'` |
| `spSAFTHeader.sql:32-34` | `SoftwareCompanyName`/`ID`/`Version` |
| `spSAFTHeader.sql:63` | `TaxType` default `'100010'` |
| `fnTaxDescription.sql` | cota 19%, `TaxCode '310309'` |
| `fnExportSAFTTaxType.sql` | listă fixă de 8 coduri de taxă |
| `spSAFTProduct.sql:107,111,133` | `WarehouseID='DEPOZIT'`, `ProductType='Marfa'`, `ValuationMethod='FIFO'` |
| `spSAFTPayments.sql:26` | `PaymentMethod = '03'` |
| `spSAFTSaleInvoice.sql:77` | cont fallback `'371.1'` |
| `fnExportSAFTJurnal.sql` | `TaxPercentage = 1.00` |
| `fnExportSAFTAnalysisType.sql` | singura intrare `A/000` |
| `spSAFTAccount.sql:22` | `AccountCreationDate = '2010-12-31'` |
| `fnSAFTUMByDen.sql` | 12 denumiri de UM, în română, cu diacritice |

**`SafTProvider`** (§8): `GetMovementOfGoods()` și `GetAssetTransactions()` execută
`Query<...>(@"")` — **string SQL gol**; fără streaming (construiește tot `AuditFile`
în memorie); **namespace greșit** — clasele generate declară
`mfp:anaf:dgti:d406**t**:declaratie:v1` (TEST) în loc de producție ⇒ fișierul
serializat de provider ar fi respins; multi-mapping Dapper cu 10 tipuri în `splitOn`
și `[Amount]` alias-at de 3 ori; **un** test, cu connection string hardcodat
`Server=(local);Database=EServices;Integrated Security=true` și `headerID = 1`.

**Fragilități arhitecturale**: namespace-ul XML aplicat prin regex peste tot
documentul; eliminarea diacriticelor pe tot fișierul; `Dapper.SqlMapper.Settings.CommandTimeout`
setat **global** (static) pe durata exportului; fără segmentare; două surse
necorelate pentru solduri vs. rulaje; fișierele `.Back`/`Full` din același folder pot
suprascrie procedura activă.

### D.7 Checklistul §7.10, actualizat cu starea de azi

Sinteza lipsurilor din analiza 2026-07-23, reverificată în cod la data acestui
document:

| # | Lipsă din analiză | Stare azi |
|---|---|---|
| 1 | Adresă structurată + contact pe partener | **PARȚIAL REZOLVAT** — `Partener.Strada/Numar/DetaliiAdresa/Localitate/CodPostal/JudetId`, cu `MaxLength` luate din `AddressStructure` SAF-T (decizia 72a, `Repartitori.cs:78-124`). **Contactul rămâne lipsă** (34g) |
| — | `Region` ISO 3166-2 | **REZOLVAT** — `Judet` (`Cod` ISO 3166-2, `Denumire` cu `MaxLength(35)` = „lungimea câmpului `Region` din `AddressStructure`", `CodAuto`, `CodCnp`), `[ForbidCRUD]`, seed autoritar din `JudeteRo` (decizia 72b, `Judet.cs`) |
| 2 | Entitate `Societate` | **ÎNCĂ LIPSĂ** — nicio clasă `Societate` în `BusinessObjects\`. Blochează `Header`, `PaymentLine`, `TransactionLine` |
| 3 | Nomenclator `UnitateMasura` (cod UN/ECE + factor) | **ÎNCĂ LIPSĂ** — `Produs.UM` e `string` liber (`ProdusLot.cs:17`) |
| 4 | Cod NC (`ProductCommodityCode`) pe produs | **ÎNCĂ LIPSĂ** — `Produs` are doar `Cod`, `Denumire`, `UM`, `TipMaterialId` (`ProdusLot.cs:14-20`) |
| 5 | Tip document fiscal (`InvoiceType` 380/381/384/389) | **ÎNCĂ LIPSĂ** |
| 6 | Identificator scurt (≤18 caractere) stabil per document | **ÎNCĂ LIPSĂ** |
| 7 | Serviciu de balanță | **PARȚIAL REZOLVAT** — feliile 66/67 au livrat `Balanta` + balanța pliată; soldurile per partener/produs rămân de verificat |
| 8 | Câmp de descriere/explicație pe document și pe linie | **ÎNCĂ LIPSĂ** (`Description` e obligatoriu în 4 locuri) |
| 9 | `LineNumber` pe `DocumentDetaliu` | **ÎNCĂ LIPSĂ** |
| 10 | Valută + curs pe `FacturaIesire` și `RegistruContabil` | **ÎNCĂ LIPSĂ** — `AmountStructure` cere `CurrencyCode` + `CurrencyAmount` obligatoriu |
| 11 | Mapare `TipInstrumentPlata` → `PaymentMethod` ANAF | **ÎNCĂ LIPSĂ** |
| 12 | Mapare `TipStoc`/tip document → `MovementType` ANAF | **ÎNCĂ LIPSĂ** |
| 13 | Validatoare CNP/CUI/IBAN | parțial — `PlatitorTvaClient.CuiInterogabil` face doar filtrarea de candidați (decizia 72c) |
| 14 | `SelfBillingIndicator`, `AddressType`, `TaxAuthority` | trivial, nefăcut |

**Ce nu lipsește** (§7.10, verbatim): „TVA structural (`TipTva` cu coduri SAF-T
reale), partenerul per latură pe rândul contabil (`Dimensiuni`), legătura
plată↔factură (`Imperechere`), gestiunea reală ca `WarehouseID`, prețul unitar exact
pe lot, dimensiunile pentru `Analysis`."
Confirmat în cod: `TipTva.cs:35-36` are `CodSafTLivrare` și `CodSafTAchizitie`.

---

## E. Nomenclatoarele

Foaia `Centralizator_Nomenclatoare` listează **41 de nomenclatoare**, cu modul de
întreținere: „Prin aplicația NOMEN – ANAF" (impozite, bugete, județe, țări, valute),
„Nomenclator specific SAF-T" (codurile de taxă, UM, NC8, stocuri, imobilizări, regim
fiscal, StockChar, tipuri de facturi, mecanisme de plată, WHT), sau „Actualizate cu
rol de administrator nivel central aplicatie SAFT - Inspector digital (functionalitate
auditata)" (IBAN, planurile de conturi).

**Cifrele de mai jos sunt măsurate din xlsx**, nu estimate.

| # | Nomenclator | Foaie | Cheia | N | Alimentează | Ce din Atlas.Conta l-ar alimenta |
|---|---|---|---|---|---|---|
| 1 | Impozite și taxe | `TAX-IMP - Impozite` | cod 3 cifre | **354** | `TaxType` (MF.TT.2, S.TI.1, MF.P.13) | constantă: `300` pentru TVA, `000` pentru nefiscal/import |
| 2 | Categorii de bugete | `TAX-IMP - Bugete` | 1 cifră | 7 — `1` buget de stat, `2` asigurări sociale de stat, `3` asigurări de sănătate, `4` asigurări pentru șomaj, `5` instituții publice, `9` cont unic, `0` categorie tehnică | informativ | — |
| 3 | Coduri TVA **livrări** | `Livrari` | 6 cifre | **59** (`310301`–`310359`) | `TaxCode` | **`TipTva.CodSafTLivrare`** (există) |
| 4–12 | Coduri TVA **achiziții**, 8 foi | `Achizitii ded 100%` (`300nnn`–`309nnn`), `Achizitii baserate` (`36xnnn`), `Achizitii ded 50%_baserate` (`32xnnn`), `Achizitii ded 50%_not_known` (`33xnnn`), `Achizitii ded 50%` (`34xnnn`), `Achizitii neded` (`35xnnn`), `Achizitii neded 50%` (`39xnnn`), `Achizitii not known` (`37xnnn`) | 6 cifre | **111–112 fiecare** (inactive: ded 100% → 1, baserate → 19, neded → 31, not known → 1, restul → 73) | `TaxCode` | **`TipTva.CodSafTAchizitie`** (există). Dimensiunea „drept de deducere" (100 / 50 / nedeductibil / pro-rata / nedeterminat) e **ortogonală** cotei și regimului — încă nemodelată (36f) |
| 13 | TVA note contabile | `TVA_NoteContabile` | `380nnn` | **21** | `TaxCode` **doar în `GeneralLedgerEntries`** | `380001`–`380007` autocolectare (19/9/5/20/24/21/11%), `380101`–`380107` TVA neexigibilă, **`380200` Notă închidere TVA**, `380301`–`380306` art. 319 alin. (10). COM pe autocolectare: „**Acest cod se va utiliza numai pentru înregistrări contabile din secțiunea GeneralLedgerEntries**" |
| 14 | Impozite reținute la sursă | `WHT - nomenclator` | 6 cifre | 191 rânduri, **124 coduri distincte** | `TaxCode` | — |
| 15 | WHT D207 / cote | `WHT - D207` (24), `WHT - cote` | | | informativ | — |
| 16 | IBAN | `IBAN-ISO13616-1997` | ISO2 | **96 țări** (`Nr. caractere` + format bancar, ex. `RO=4a,16c`) | `BankAccountStructure.IBANNumber` | `ContPropriu.Iban` (societate); partenerii = satelit 34g, **lipsă** |
| 17 | **Județe** | `ISO3166-1A2 - RO Dept Codes` | `RO-XX` | **42** | `Address.Region`, `TaxTableEntry.Region`, `AuditFileRegion` | **`Judet.Cod`** (există, decizia 72b) |
| 18 | Țări | `ISO3166-2-CountryCodes` | ISO2 | **252** (incl. `EU`, `XK`, `XI`) | `Country`, prefixele 01/02/05/06/10/11 | **`Partener.Tara`** (există, decizia 71b) |
| 19 | Valute | `ISO4217CurrCodes` | ISO3 | **179** (+ cod numeric + nr. de zecimale) | `AmountStructure.CurrencyCode` | **LIPSĂ** (fără multi-valută) |
| 20–28, 41 | 10 planuri de conturi | `PlanConturiBalSocCom` (**648**), `PlanConturiIFRS`, `PlanConturiBanci`, `PlanConturiNebancare`, `PlanConturiIFRS_Norma39`, `PlanConturiSocAsigurari`, `PlanConturi_Norma36`, `PlanConturi_Norma14`, `PlanConturiONG`, `PlanConturiBNR - ordin 6` | simbol | | `AccountID` (peste tot) | **`Cont.Simbol`** — la profilul privat OMFP potrivirea e directă; e nevoie de normalizare (Atlas segmentează cu puncte) și, la bugetar, de o tabelă de mapare cu justificare (modelul `saft.AccountMaps`, coloana `Descriere` `NotNullable`) |
| 29 | **Unități de măsură** | `Unitati_masura` (UN/ECE Rec 21, V8) | cod | **2.162** | `UnitOfMeasure`, `UOMBase`, `UOMStandard`, `UOMPhysicalStock`, `InvoiceUOM` | **LIPSĂ** — `Produs.UM` e string liber |
| 30 | **NC8 / TARIC** | `NC8_2022_TARIC3 rev 2026` („NC8 - 2026", ultima actualizare 27.01.2026) | 8 cifre | **9.984** | `ProductCommodityCode`, `StockAccountCommodityCode` | **LIPSĂ** — niciun câmp pe `Produs` |
| 31 | **Mișcări de stoc** | `Nomenclator stocuri` | 1–3 cifre | **19** | `MovementType` (MF.MT.2) **și** `MovementSubType` (SD.MG.31) | derivabil din tipul documentului — vezi tabelul de mai jos |
| 32 | **Mișcări de active** | `Nomenclator imobilizari` | 1–3 cifre | **13** — `10` achiziție, `20` vânzare, `30` amortizare, `40` transfer intern, `50` casare, `60`/`70` reevaluare −/+, `80`/`90` plus/minus inventar, `100`/`110` ajustare/reversare de valoare, `120` titlu gratuit, `130` alte | `AssetTransactionType` | — (modul separat, decizia 9) |
| 33 | Regimuri fiscale | `Nomenclator_Regim_fiscal` | 6 cifre | **5** — `100010` persoană impozabilă înregistrată în scopuri de TVA, `100020` regim special de scutire pentru întreprinderi mici, `100030` cod de TVA anulat **ex-oficio** (art. 316 alin. (11)), `100040` **TVA la încasare**, `100050` persoană neimpozabilă | `TaxIDStructure.TaxType` | **derivabil** din `Partener.InregistratTva` + `TvaLaIncasare` + `InactivFiscal` (71b/72a) — dar `100030` ≠ `InactivFiscal` (§F.17) |
| 34 | Caracteristici stocuri | `SAFT_Nomenclator_StockChar` | | **3** — `blue_35` (colorant motorină, solvent blue 35, 5 mg ±10%/litru), `yellow_124` (marcator motorină, solvent yellow 124, 7 mg ±10%), `0` (orice alt caz) | `StockCharacteristicValue` (MaxLength 35) | `Lot.LotFabricatie`, `Lot.DataExpirare` — candidați pentru perechile caracteristică/valoare |
| 35 | **Tipuri de facturi** | `Nom_Tipuri_facturi` (derivat din ISO EN16931 / UNTDID 1001) | cod | **43** în foaie, **6 admise** pe `InvoiceType` | `S.I.9 InvoiceType` | **LIPSĂ** |
| 36 | **Mecanisme de plată** | `Nom_Mecanisme_plati` (UNCL 4461) | cod | **83** mecanisme → **5** `PaymentMethod` | `SD.P.10`, `SD.P.32`, `S.I.63` | `TipInstrumentPlata` (OrdinPlata/Cec/DispozitieCasa/Chitanta) — **maparea lipsește** |
| 37 | Catalog active fixe | `CatalogActive` | cod clasificare | **588** (+ durate normale de funcționare, ani) | informativ | — |
| 38 | Clase de asigurare | `Nom_asig` | 8 cifre | **75** | validat împreună cu NC8 pe `ProductCommodityCode` | — |
| 39–40 | Anexe informative | `An fiscal-perioade de raportare` (art. 16 Cod fiscal, text integral), `Nomenclator tari si valuta` (≈250 rânduri, sursa: structura F3000), `IBAN validation` | | | informativ | — |

**Codificarea mișcărilor de stoc** (`Nomenclator stocuri`, cele 19 valori):

| Cod | RO | EN |
|---|---|---|
| 10 | Achiziție | Purchase |
| 20 | Producție | Production |
| 30 | Vânzare | Sale of stock |
| 40 | Retur produse vândute | Return of sales |
| 50 | Retur produse achiziționate | Return of purchase |
| 60 | Reduceri comerciale primite | Discounts received |
| 70 | Consum | Consumption |
| 80 | Transfer intern | Internal Transfer |
| 90 | Cheltuieli ulterioare incluse în valoarea de intrare | Subsequent costs capitalized |
| 100 / 101 | Diferențe de preț pozitive / negative | Positive / Negative price difference |
| 110 / 120 | Plus / minus de inventar | Inventory count positive / negative adjustment |
| 130 / 140 | Ajustări / reluări de ajustări pentru deprecierea stocurilor | Impairment adjustments / Reversal |
| 150 | Bunuri acordate cu titlu gratuit | Goods granted free of charge |
| 160 | Bunuri degradate | Damaged goods |
| 170 | Bunuri expirate | Expired goods |
| 180 | Alte tranzacții | Other transactions |

**Nomenclatoare de valori fixe cerute de schemă, fără foaie proprie** (din
`1C/06-nomenclatoare-seed.md` §6, migrația `DefaultDB_20230626_2315_SafT.cs`,
confirmate ca array-uri în `Parameters_v0.class`):

- `AddressType` (5): `StreetAddress`, `PostalAddress`, `BillingAddress`,
  `ShipToAddress`, `ShipFromAddress`
- `AccountType` (3): `Activ`, `Pasiv`, `Bifunctional`
- `GoodsServicesID` (2): `01`, `02`
- `DebitCreditIndicator` (2): `C`, `D`
- `SelfBillingIndicator` (2): `389`, `0`
- `TaxAccountingBasis` (12, §B.1)
- `HeaderComment` (6, §A.4)

**Ordinea de seed recomandată** de `1C/06-nomenclatoare-seed.md` §8 (cu sursă gata
curățată în migrațiile EServices): 1. `County` (42, **dublă codificare** ISO +
cod numeric ANAF) + `Country` (249) — *deja acoperit de `Judet`* · 2.
`saft.MovementType` (19) · 3. `InvoiceType` / `AddressType` / `AccountingBasis` /
`CompanyTaxType` · 4. `TaxCode.CodD300` (721 — puntea taxCode SAF-T → rând D300) ·
5. `AccountMaps` per bază · 6. `Currency` (178).

---

## F. Ce a rămas neclar sau contradictoriu — 24 de puncte

### Lacune ale surselor locale

1. **Nu există niciun act normativ pentru D406 în repo.** Pentru D300 există
   `anaf/d300/OPANAF_174_2026.pdf` + `.txt` + `structura_D300_v12.pdf/.txt`; pentru
   D394 există `OPANAF_3769_2015`, `OPANAF_2194_2025`, XSD-ul `d394_20250917.xml`,
   `structD394_15092025`. Pentru D406: **nimic**. ⇒ **termenele, pragurile pe
   categorii de contribuabili și perioada de grație nu pot fi afirmate din surse
   locale.** De procurat OpANAF 1783/2021 + modificările lui.
2. **Nu există XSD ca fișier.** Doar bytecode-ul validatorului + 7 fragmente în
   notele de subsol. Obligativitatea reală (`minOccurs`) se cunoaște din coloanele
   xlsx și din `[Required]`-urile claselor XSD-generate ale lui 1C (§D.4) — care sunt
   din 2023.
3. **Nu există un exemplu de XML D406 valid** în repo. 1C avea unul
   (`D:\duk_SAFT_2023_08_04\SafT.xml`, shred-uit de `DeclaratieExemplu.sql`) — nu e
   aici.
4. **Regula totalurilor nu e enunțată nicăieri** (§C.2). Nici xlsx, nici mesajele
   validatorului nu conțin `Total* == Σ linii`. De măsurat empiric, cu un fișier
   deliberat inconsistent.
5. **Pragul de la care segmentarea devine obligatorie nu apare în nicio sursă**
   (semnalat și de 1C ca `[de verificat]`, `02-saft-export.md:92-93`).

### Contradicții între surse

6. **`TaxAccountingBasis` — regula sintactică e depășită.** Xlsx (`1. Header` r.4,
   col.11) scrie literal:
   `Values restricted to ”A”, ”I”, ”IFRS”, ”BANK”, ”INSURANCE”, ”NORMA39”, "IFN", "NORMA36"`
   — dar **descrierea aceluiași rând** enumeră și `NORMA14`, `ONG`, `ONGE`, iar
   validatorul compilat conține în plus `BNR6`. Coloana SYN n-a fost actualizată la
   modificările 123 (05.06.2025, ONGE) și 125 (26.06.2025, BNR6).
7. **`PlanConturiBalSocCom`: 648 de conturi în xlsx vs 635 în validator**
   (`Nomen_v3.properties`, cheia `plan_conturi_bal_soc_com` — identică ca lungime cu
   `anaf/plan_conturi_bal_soc_com.md`, 2.797 bytes). Autoritatea la validare e
   resursa din jar. De reconciliat înainte de a seed-ui.
8. **Kitul e inconsistent pe versiuni**: `versiuniCurente.txt` = J2.2.8, istoricul
   listează J2.2.15, arhiva se numește `duk_SAFT_an_luna_20260216.zip`, schema de
   coduri e 16.02.2026. Nu se poate spune contra cărei versiuni s-ar valida efectiv
   fără să rulezi auto-update-ul.
9. **`4.2 PurchaseInvoices` are celula goală pe coloana „Raportare
   Lunară/Trimestrială"**, deși `4.1 SalesInvoices` are `x` și toate câmpurile-copil
   `SD.PI.1–4` au `x`. Aproape sigur o omisiune de formatare — dar formal secțiunea
   nu e marcată ca lunară.
10. **`2.10 PhysicalStock`: col.9 = `Mandatory - by request`, col.10 = `Optional`** —
    spre deosebire de `2.8 MovementTypeTable`, `2.11 Owners` și `4.4 MovementOfGoods`,
    unde col.10 = `Mandatory`. Neclar dacă e intenționat (cardinalitatea e `0..1` la
    PhysicalStock vs `1` la celelalte) sau o inconsecvență.
11. **`4.5 AssetTransactions`: col.9 `Mandatory - once per year`, col.10 `Optional`,
    cardinalitate `0..1`**; dar `2.12 Assets` are col.10 `Mandatory`, cardinalitate
    `1`. Asimetrie neexplicată între cele două secțiuni anuale.
12. **ID-ul `MF.GLA.4` e orfan**: `StandardAccountID` apare pe rândul 7 al foii
    **fără ID în coloana 1**, iar `MF.GLA.4` e pe rândul 8, complet gol. Numerotarea
    sare.
13. **Mesajul validatorului pentru segmentare spune „mai mare decât"**:
    `TotalSegmentsInSequence […] ar fi trebuit sa fie mai mare decat SegmentIndex`.
    Pentru un fișier nesegmentat (`1`/`1`) condiția e falsă. Fie e o atenționare, fie
    textul e greșit și verificarea e `>=`. De confirmat empiric.
14. **`Name` în `CustomerInfo`/`SupplierInfo`**: schema îl declară ca ramură validă a
    lui `xs:choice` (nota [5], `minOccurs="0"`), dar validatorul îl respinge explicit:
    „Acest element nu trebuie folosit". Schema și validatorul spun lucruri diferite.
15. **`ProductCommodityCode` e obligatoriu pentru toate produsele**, dar codul NC
    n-are sens pentru servicii. SEM enumeră cazurile în care „se raportează unde e
    cerut de legea română" (import/export, alimente cu cotă redusă, Intrastat, taxare
    inversă pe cod NC, accizabile) — ceea ce sugerează o valoare de umplere pentru
    rest. 1C folosește **`0`**; xlsx-ul nu confirmă că `0` e acceptat, iar `NC8`
    conține `00000000` și `99999999`. **Nedeterminat.**
16. **`ValuationMethod`**: schema exemplifică „FIFO / LIFO / Average cost", fără
    nomenclator închis. Decizia 13 din Atlas e **identificare specifică pe lot**
    (apărată ca FIFO prin OMFP 96(3), decizia 51e). Nu se poate spune dacă
    „identificare specifică" e acceptată. 1C hardcodează `FIFO`. Marcat
    `[de verificat]` și în `02-saft-export.md` §7.3.
17. **`Nomenclator_Regim_fiscal` vs modelul Atlas**: `100030` = „cod de TVA anulat
    **ex officio**, art. 316 alin. (11)" — nu e același lucru cu
    `Partener.InactivFiscal` (care e `stare_inactiv.statusInactivi` din răspunsul
    ANAF, cf. comentariului din `Repartitori.cs`). Restanțele **D4-r1** (istoricul
    statutului de TVA) și **72-r4** (`StareInregistrare` neconsumată) ating exact
    acest punct. **Nedecis.**
18. **`AccountID` „integer decimal number, not null"** vs planul segmentat cu puncte
    al Atlas (`302.02.00`). Istoricul J2.2.5 (19-Nov-2024) notează „**corectie
    validare AccountId (6 cifre)**" — deci există o limită de lungime pe analitic pe
    care xlsx-ul nu o menționează (tipul declarat e `SAFmiddle2textType` = 70). De
    verificat empiric.
19. **`SystemID` = `SAFshorttextType` = 18 caractere**, iar cheile Atlas sunt `Guid`.
    1C rezolvă prin `left(...,18)` / `right(...,18)` — ceea ce **nu garantează
    unicitatea**. Problema e reală și nerezolvată în ambele sisteme.
20. **`AmountStructure` cere `CurrencyCode` și `CurrencyAmount` obligatoriu pe fiecare
    sumă**, iar validatorul verifică `Amount == CurrencyAmount × ExchangeRate`. Pentru
    un deployment 100% RON: `CurrencyCode='RON'`, `CurrencyAmount = Amount`,
    `ExchangeRate` absent sau 1 — **dar nu e clar dacă `ExchangeRate` lipsă trece
    verificarea aritmetică sau e tratat ca 0**.
21. **Numărul total de `TaxCode`-uri acceptate de validator nu se poate măsura exact**
    din constant pool (array-ul e întrerupt de alte constante). S-a citit un run
    contiguu de **177** de coduri, identic în `Parameters_v0..v3`, cu distribuția pe
    prefixe: `000`×1, `300`×26, `301`×10, `306`×11, `307`×11, `308`×7, `309`×5,
    `310`×37, `320`×26, `321`×10, `326`×11, `327`×11, `328`×6, `329`×5. Numărul real e
    de ordinul a ~1.100 (59 livrări + 8×111 achiziții + 21 note contabile + 124 WHT).
22. **Nu e clar cărui an îi corespunde fiecare `Parameters_vN`.** Există v0–v3 și
    resurse `Nomen_v0..v3`, `NC8_2022_TARIC3_v0..v3`, `IBANFormat_v0..v3`, iar CLI-ul
    primește `an=`. Maparea an→versiune e în bytecode-ul lui `Validator`/`Parameters`,
    necitit. Observație: cele patru liste de taxCode sunt **identice**, deci
    versionarea afectează alte liste (NC8, IBAN, plan de conturi).
23. **Nume de elemente inconsecvente între xlsx și validator**:
    `TotalSegmentsInSequence` (xlsx, `AuditFile.class`) vs `TotalSegmentsInsequence`
    (`ValidatorImpl.class`, cu „s" mic — și așa îl emite și 1C);
    `UOMToUOMPhysicalStockConversion` (xlsx) vs
    `UOMToUOMPhysicalStockConversionFactor` (validator);
    `AcquisitionAndProductionCostOnTransaction` (xlsx) vs
    `AcquisitionAndProductionCostsOnTransaction` (validator).
    **Autoritatea e validatorul**, dar nu s-a putut verifica pe un fișier real.
24. **`1C/02-saft-export.md` e din 2026-07-23** și marchează ca lipsă lucruri livrate
    ulterior de decizia 72 (adresa structurată, `Judet`). Checklistul §7.1 trebuie
    citit prin filtrul stării actuale (§D.7), altfel duplică muncă deja făcută.

---

## Surse

| # | Sursă | Ce acoperă | Încredere |
|---|---|---|---|
| 1 | `anaf/RO_SAFT_SchemaDefCod_16.02.2026.xlsx` — 55 de foi (schema în foile 1–6, 40 de nomenclatoare) | §A, §B, §C.2–C.4, §E | **Oficial, primar.** Toate foile enumerate; foile 1–6 citite integral (celule complete, nu doar antete); nomenclatoarele numărate programatic. |
| 2 | `anaf/duk_SAFT_an_luna/dist/lib/D406Validator.jar` + `D406TValidator.jar` | §C.1 (mesajele verbatim), §B.1 (setul `TaxAccountingBasis`), namespace-urile, XPath-urile de secțiune | **Oficial, primar, autoritatea la validare.** String-urile extrase din constant-pool-ul claselor (nu decompilare). Logica din jurul lor **nu** a fost citită — doar numele de metode și literalii. |
| 3 | `anaf/duk_SAFT_an_luna/dist/doc/` — `Instructiuni.txt`, `D406IstoriaVersiunilor.txt`, `IntrebariFrecvente.rtf`; `dist/config/versiuniCurente.txt`; `dist/CITESTE-MA.TXT` | §A.1, §A.2, §C.3, §C.5 | **Oficial.** Citite integral. |
| 4 | `anaf/taxImp.md`, `nom_Tipuri_facturi.md`, `countryCodIso.md`, `is3166_02.md`, `ISO_3166_2_RO.md`, `iban.md`, `plan_conturi_bal_soc_com.md`, `plan-conturi-omfp.csv` | §E (verificare încrucișată a numerelor) | **Extrase locale** din xlsx, făcute în felia P1. Concordă cu xlsx-ul cu excepția §F.7. |
| 5 | `1C/02-saft-export.md` (886 linii), `1C/05-toolchain-validare-anaf.md`, `1C/06-nomenclatoare-seed.md` | §D integral, §E (ordinea de seed) | **Analiză internă**, datată 2026-07-23, ancorată în cod (`D:\Dev\Work\EServices\`). Citită integral. Depășită parțial de decizia 72 (§F.24). |
| 6 | Codul curent: `Repartitori.cs`, `Judet.cs`, `ProdusLot.cs`, `TipTva.cs`, `BackOfficeDbContext.cs` | §D.7 (coloana „stare azi") | **Verificat direct.** Doar clasele numite; nu s-a făcut un audit complet al modelului. |

**Nu s-a consultat**: nicio sursă web, niciun act normativ. Ordinul OpANAF 1783/2021
lipsește (§F.1).

### Grad de încredere pe secțiune

| Secțiune | Încredere | Motiv |
|---|---|---|
| §A.1–A.5 — versiuni, matricea de raportare, modalitatea | **Foarte ridicată** | citite direct din xlsx și din kit, celulă cu celulă |
| §A.6 — termene, praguri | **Absentă** | sursa nu există local; nu s-a afirmat nimic |
| §B — structura, tipurile, cardinalitățile | **Foarte ridicată** pentru ce e în xlsx; **ridicată** pentru numele exacte de elemente (§F.23) | criteriul e xlsx-ul + confruntarea cu numele din validator |
| §B.4 — identificarea partenerului | **Foarte ridicată** | textul SYN/SEM citit integral, nu rezumat |
| §C.1 — mesajele validatorului | **Foarte ridicată** (verbatim) | extrase din bytecode; **dar** condițiile care le declanșează nu au fost citite din cod |
| §C.2 — totalurile | **Ridicată ca negație** | s-a căutat exhaustiv și nu s-a găsit nicio regulă; asta nu dovedește absența ei în cod |
| §D — exportul 1C | **Ridicată** | preluat dintr-o analiză ancorată în fișiere; nu s-a recitit codul EServices |
| §D.7 — coloana „stare azi" | **Ridicată** | verificat în cod pentru fiecare rând marcat REZOLVAT/LIPSĂ |
| §E — cifrele nomenclatoarelor | **Ridicată** | măsurate programatic; câteva antete pot fi numărate ca rânduri (marjă ±1 pe foaie) |
| §F — neclaritățile | **N/A** | e chiar lista de incertitudini |

### Fișiere sursă (local, gitignored: `anaf/`)

- `RO_SAFT_SchemaDefCod_16.02.2026.xlsx` — schema + toate nomenclatoarele
- `duk_SAFT_an_luna_20260216.zip` + `duk_SAFT_an_luna/` (dezarhivat, ≈252 MB) —
  kitul DUKIntegrator: `dist/lib/D406Validator.jar`, `D406TValidator.jar`,
  `D406Pdf.jar`, `DecValidation.jar`; `dist/doc/`; `dist/config/`; `jre6/`, `jre8/`
- `taxImp.md`, `nom_Tipuri_facturi.md`, `countryCodIso.md`, `is3166_02.md`,
  `ISO_3166_2_RO.md`, `iban.md`, `plan_conturi_bal_soc_com.md` — extrase brute
- `plan-conturi-omfp.csv` — planul OMFP 1802, folosit efectiv de `ProfilPrivat`
  (antetul real e `Account,ParentAccount,Romana`; din F16 pas 3 resursa
  seed-ului `Module/DatabaseUpdate/SeedData/plan-conturi-omfp.csv` are și
  coloana `Functie` D/C/B, derivată din A/P/A-P al anexei OMFP)
- **XSD-ul oficial există local** (găsit în pasul 3): `D:\Dev\Work\EServices\
  EServices.Web\Ro_SAFT_Schema_v247_20230306.xsd` — v2.4.7, namespace de TEST,
  structură identică cu producția; sursa ordinii și a numelor de elemente din
  `SaftXml` (`TotalSegmentsInsequence` cu `s` mic, `TaxAmount` =
  `AmountStructure`, `PaymentMechanism` în `PaymentSettlement`,
  `TaxInformationTotals` în `InvoiceDocumentTotals`, `BankAccountStructure` =
  `xs:choice` IBAN | număr de cont). Măsurat cu DUK J2.2.8 (pas 3): fișierul
  scenei `ok`; `-d` NU e utilizabil în CLI (procesul se agață — modul grafic);
  comanda care merge: `jre8\bin\java.exe -jar DUKIntegrator_AnLunaUI.jar -v
  D406 <xml> !<err> $ an=AAAA luna=LL`; `AccountID` de 7 cifre, `ExchangeRate`
  absent, `1/1` segmente, `ProductCommodityCode = 0`, diacriticele și `ONGE`
  TREC; `04` cu cratimă e RESPINS; codul NC e validat contra NC8 al anului.

---

## Unelte

Scripturile folosite pentru a produce documentul. Au trăit în directorul de scratchpad
al sesiunii (nu sunt comise); se pot rescrie din descrierile de mai jos. Toate rulează
pe **Python stdlib** — `openpyxl` nu se poate instala în mediul local (Python-ul e
gestionat de `uv`, `pip install` returnează „This environment is externally managed").
Bash-ul disponibil e incomplet (`head`, `tail` lipsesc); PowerShell e calea sigură.

| Script | Ce face |
|---|---|
| `xlsx.py` | Cititor de `.xlsx` pe stdlib (`zipfile` + `xml.etree`): parsează `sharedStrings.xml`, `workbook.xml` + relațiile, întoarce foile ca liste de rânduri, cu decodarea celulelor `s`/`inlineStr`/numerice și conversia referinței de coloană (`A1` → index). Rulat direct: `python xlsx.py <fișier> sheets` listează foile cu numărul de rânduri; `python xlsx.py <fișier> "<foaie>" [start] [end]` dumpează rândurile. |
| `cols.py` | Dump selectiv de coloane dintr-o foaie: `python cols.py <fișier> "<foaie>" "0,1,4" [start] [end] [maxlen]`. Înlocuiește newline-urile cu `⏎` și taie la `maxlen`, ca să încapă tabelele largi într-un singur ecran. Sare rândurile complet goale. |
| `dump.py` | Dumper specializat pe foile 1–5 ale schemei (care au aceeași structură de coloane): modul `compact` = o linie per câmp (ID, sub-secțiune, element, tip, obligativitate, cardinalitate, flag-urile de raportare, sursa); modul `rules` = același lucru plus `DESC`/`SRC`/`SYN`/`SEM`/`DEF`/`COM`, normalizate pe o singură linie și trunchiate la o lățime dată. A produs §B și §C.2. |
| `tva.py` | Detectează automat rândul de antet și coloanele (`Cod taxă in SAF-T`, `- Activ`, `Descriere cod`, `Corespondent rand D300`) în cele 11 foi de coduri de taxă, apoi numără codurile și pe cele marcate inactiv, cu intervalul min–max. A produs cifrele din §E rândurile 3–13. |
| `counts.py` | Numără intrările din restul nomenclatoarelor, pe (foaie, coloană-cheie, rând de start, regex al cheii): TAX-IMP, UM, județe, țări, valute, tipuri de facturi, mecanisme de plată, plan de conturi, NC8, catalog active, Nom_asig, WHT, stocuri, imobilizări, IBAN. Raportează total, unic și primele/ultimele valori — de aici s-a văzut că `WHT` are 191 de rânduri dar doar 124 de coduri distincte. |
| `jstrings.py` | Extractor de string-uri din bytecode Java **fără decompilator**: parsează constant-pool-ul unui `.class` (tag 1 = UTF8, cu sărituri corecte peste celelalte tag-uri, inclusiv dubla intrare pentru `long`/`double`), filtrează cu un regex și dedupe. Rulat pe arborele dezarhivat al lui `D406Validator.jar` a produs §C.1 (mesajele), XPath-urile din `SECTION_ELEMENTS`, seturile de valori din `Parameters_v*` și namespace-urile. |

Dezarhivarea jar-urilor s-a făcut cu `Expand-Archive` după copierea lor cu extensia
`.zip`, **în scratchpad, nu în repo**:

```powershell
Copy-Item "…\dist\lib\D406Validator.jar" "$sp\D406Validator.zip" -Force
Expand-Archive "$sp\D406Validator.zip" -DestinationPath "$sp\D406Validator" -Force
python jstrings.py "$sp\D406Validator" "@0@|@1@|@2@"
```
