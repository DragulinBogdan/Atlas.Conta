namespace Atlas.Conta.BackOffice.Module.Saft;

// ARBORELE D406, ca DTO-uri PLATE (deciziile 6/7) — felia 16, D16-D4.
//
// Numele claselor sunt ale FELIEI, numele proprietăților sunt ale SCHEMEI
// (`AccountID`, `OpeningDebitBalance`, `TaxPercentage`…): fișierul se scrie
// dintr-un arbore care se citește ca schema, iar diferența dintre „ce am" și „ce
// cere formularul" se vede la o singură lectură. Zecimalele rămân `decimal`
// exacte (71g: bani exacți, rotunjirea e a fișierului); enum-urile pleacă STRING
// (57a); `null` înseamnă ABSENT în XML, niciodată 0.
//
// DTO-urile astea sunt și forma servită ca JSON pe `GET api/proiectii/saft`
// (pasul 4) — deci nimic din ele nu e „doar pentru scriitor".

// `AddressStructure` (5.1). `City` și `Country` sunt obligatorii în fișier —
// lipsa lor e AVERTISMENT + valoarea de rezervă, nu refuz (D16-D4).
public sealed class SaftAdresa {
    public string StreetName { get; set; }
    public string Number { get; set; }
    public string AdditionalAddressDetail { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    // `Region` = ISO 3166-2, DOAR pe RO (regula validatorului, 72b).
    public string Region { get; set; }
    public string Country { get; set; }
}

// `Header` (S.H.1–13) + `CompanyHeaderStructure` (5.5), pliate: antetul e un
// singur rând, iar o clasă per nivel de imbricare n-ar adăuga nimic.
public sealed class SaftHeader {
    public string AuditFileVersion { get; set; }
    public string AuditFileCountry { get; set; }
    public string AuditFileRegion { get; set; }
    public DateOnly AuditFileDateCreated { get; set; }
    public string SoftwareCompanyName { get; set; }
    public string SoftwareID { get; set; }
    public string SoftwareVersion { get; set; }
    public string DefaultCurrencyCode { get; set; }
    public string HeaderComment { get; set; }
    public string SegmentIndex { get; set; }
    public string TotalSegmentsInSequence { get; set; }
    public string TaxAccountingBasis { get; set; }

    // `Company` (5.5): `RegistrationNumber` e ALTĂ regulă decât prefixele de
    // partener (`RO`+CUI sau CUI) — vezi `SaftReguli`.
    public string RegistrationNumber { get; set; }
    public string Name { get; set; }
    public SaftAdresa Address { get; set; }
    public string ContactFirstName { get; set; }
    public string ContactLastName { get; set; }
    public string Telephone { get; set; }
    public string Email { get; set; }
    public string IBANNumber { get; set; }
    public string BankAccountNumber { get; set; }

    // `SelectionCriteria` (5.12, ramura pe perioade contabile).
    public int PeriodStart { get; set; }
    public int PeriodStartYear { get; set; }
    public int PeriodEnd { get; set; }
    public int PeriodEndYear { get; set; }
}

// `MasterFiles/GeneralLedgerAccounts/Account` (2.1). Soldurile sunt `xs:choice`
// (nota [1]): exact unul din fiecare pereche e nenul — de aceea sunt `decimal?`,
// nu `decimal` cu 0.
public sealed class SaftCont {
    public Guid ContId { get; set; }
    public string AccountID { get; set; }
    public string AccountDescription { get; set; }
    public string AccountType { get; set; }
    public decimal? OpeningDebitBalance { get; set; }
    public decimal? OpeningCreditBalance { get; set; }
    public decimal? ClosingDebitBalance { get; set; }
    public decimal? ClosingCreditBalance { get; set; }
}

// `Customers/Customer` și `Suppliers/Supplier` (2.3/2.4) — structuri IDENTICE,
// deci o singură clasă; lista în care stă rândul spune rolul.
public sealed class SaftTert {
    public Guid PartenerId { get; set; }
    // `CustomerID` / `SupplierID` (§B.4). Egal cu `RegistrationNumber` din
    // `CompanyStructure` — schema le cere pe amândouă.
    public string Id { get; set; }
    public string RegistrationNumber { get; set; }
    public string Name { get; set; }
    public SaftAdresa Address { get; set; }
    // `TaxRegistration` (5.14) — doar pe partenerii `00` înregistrați.
    public string TaxRegistrationNumber { get; set; }
    public string TaxType { get; set; }
    // `AccountID` (M): contul de terț cu cel mai mare |rulaj| dintre cele ale
    // rolului — soldurile de mai jos sunt Σ pe TOATE conturile rolului.
    public string AccountID { get; set; }
    public decimal? OpeningDebitBalance { get; set; }
    public decimal? OpeningCreditBalance { get; set; }
    public decimal? ClosingDebitBalance { get; set; }
    public decimal? ClosingCreditBalance { get; set; }
    // Diagnostic (nu e în schemă): ce fel de identificator a ieșit — `FelIdSaft`
    // ca string, ca ecranul să poată explica „de ce 04 și nu 00".
    public string FelId { get; set; }
}

// `TaxTable/TaxTableEntry/TaxCodeDetails` (2.5) — un rând per cod FOLOSIT.
public sealed class SaftTaxCode {
    public string TaxType { get; set; }
    public string TaxCode { get; set; }
    public string Description { get; set; }
    public decimal TaxPercentage { get; set; }
    // `SAFBaseRate` e restricționat 0.0000–1.0000, deși descrierea vorbește de
    // „100 = integral deductibil": se emite `1`.
    public decimal BaseRate { get; set; }
    public string Country { get; set; }
}

// `UOMTable/UOMTableEntry` (2.6).
public sealed class SaftUnitate {
    public string UnitOfMeasure { get; set; }
    public string Description { get; set; }
}

// `Products/Product` (2.9).
public sealed class SaftProdus {
    public Guid ProdusId { get; set; }
    public string ProductCode { get; set; }
    public string GoodsServicesID { get; set; }
    public string Description { get; set; }
    public string ProductCommodityCode { get; set; }
    public string ValuationMethod { get; set; }
    public string UOMBase { get; set; }
    public string UOMStandard { get; set; }
    public decimal UOMToUOMBaseConversionFactor { get; set; }
}

// `AnalysisTypeTable/AnalysisTypeTableEntry` (2.7) — doar tipurile FOLOSITE.
public sealed class SaftTipAnaliza {
    public string AnalysisType { get; set; }
    public string AnalysisTypeDescription { get; set; }
    public string AnalysisID { get; set; }
    public string AnalysisIDDescription { get; set; }
}

// `AnalysisStructure` (5.3) pe o linie de tranzacție.
public sealed class SaftAnaliza {
    public string AnalysisType { get; set; }
    public string AnalysisID { get; set; }
}

// `TaxInformationStructure` (5.15).
public sealed class SaftTaxInfo {
    public string TaxType { get; set; }
    public string TaxCode { get; set; }
    public decimal? TaxPercentage { get; set; }
    public decimal? TaxBase { get; set; }
    public decimal TaxAmount { get; set; }
}

// `GeneralLedgerEntries/Journal` (B.3) — un jurnal per TIP de document (D16:
// singura grupare care există în model și pe care `CoduriTip` o dă într-un query).
public sealed class SaftJurnal {
    public string JournalID { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public List<SaftTranzactie> Tranzactii { get; set; } = [];
}

public sealed class SaftTranzactie {
    public Guid DocumentId { get; set; }
    public string TransactionID { get; set; }
    public int Period { get; set; }
    public int PeriodYear { get; set; }
    public DateOnly TransactionDate { get; set; }
    public string Description { get; set; }
    public DateOnly SystemEntryDate { get; set; }
    public DateOnly GLPostingDate { get; set; }
    // AMBELE obligatorii (GL.19/GL.20): latura liberă = societatea raportoare.
    public string CustomerID { get; set; }
    public string SupplierID { get; set; }
    public List<SaftLinieTranzactie> Linii { get; set; } = [];
}

public sealed class SaftLinieTranzactie {
    public Guid RandRegistruId { get; set; }
    public Guid? DetaliuId { get; set; }
    public string RecordID { get; set; }
    public string AccountID { get; set; }
    public string CustomerID { get; set; }
    public string SupplierID { get; set; }
    public string Description { get; set; }
    // `xs:choice` (nota [4]): `D` ⇒ `DebitAmount`, `C` ⇒ `CreditAmount`.
    public string DebitCreditIndicator { get; set; }
    // Semnat: stornoul „în negru" e chiar convenția schemei („to reflect the
    // storno în black ink").
    public decimal Amount { get; set; }
    public string CurrencyCode { get; set; }
    public decimal CurrencyAmount { get; set; }
    public List<SaftAnaliza> Analiza { get; set; } = [];
    public SaftTaxInfo TaxInformation { get; set; }
}

// `SourceDocuments/SalesInvoices|PurchaseInvoices/Invoice` (4.1/4.2).
public sealed class SaftFactura {
    public Guid DocumentId { get; set; }
    // Stornoul e o FACTURĂ PROPRIE (aceeași unitate ca `nrFact` la D394:
    // Document × Storno).
    public bool Storno { get; set; }
    public string DocumentTip { get; set; }
    public string InvoiceNo { get; set; }
    public DateOnly InvoiceDate { get; set; }
    public string InvoiceType { get; set; }
    public string SelfBillingIndicator { get; set; }
    // Contul de terț al documentului (S.I.4, M).
    public string AccountID { get; set; }
    // `CustomerInfo`/`SupplierInfo` (`xs:choice`, nota [5]): ramura `Name` e
    // RESPINSĂ de validator, deci pe sârmă merge identificatorul; denumirea
    // rămâne pentru ecran.
    public string PartenerID { get; set; }
    // Cheia entității, pentru ecran (link, filtrare). `PartenerCheie`, nu
    // `PartenerId` cum e la `SaftTert`: acolo nu există un `PartenerID` alături,
    // aici da — iar două proprietăți care diferă DOAR prin capitalizare fac
    // `System.Text.Json` să arunce la serializare
    // (`SerializerPropertyNameConflict`), adică 500 pe endpoint-ul JSON. Măsurat
    // pe calea reală (V4), nu presupus.
    public Guid? PartenerCheie { get; set; }
    public string PartenerDenumire { get; set; }
    public SaftAdresa BillingAddress { get; set; }
    public List<SaftLinieFactura> Linii { get; set; } = [];
    public decimal NetTotal { get; set; }
    public decimal GrossTotal { get; set; }
    public List<SaftTaxInfo> TaxInformationTotals { get; set; } = [];
}

public sealed class SaftLinieFactura {
    public Guid DetaliuId { get; set; }
    public int LineNumber { get; set; }
    public string AccountID { get; set; }
    public string ProductCode { get; set; }
    public string ProductDescription { get; set; }
    public decimal Quantity { get; set; }
    public string InvoiceUOM { get; set; }
    public decimal UnitPrice { get; set; }
    public DateOnly TaxPointDate { get; set; }
    public string Description { get; set; }
    public decimal InvoiceLineAmount { get; set; }
    public string DebitCreditIndicator { get; set; }
    public List<SaftAnaliza> Analiza { get; set; } = [];
    public SaftTaxInfo TaxInformation { get; set; }
}

// `SourceDocuments/Payments/Payment` (4.3).
public sealed class SaftPlata {
    public Guid DocumentId { get; set; }
    // Ca la `SaftFactura`: jumătatea de STORNO e o plată proprie, cu sumele
    // negative (fixul F1 al review-ului). Unitatea e (Document × Storno).
    public bool Storno { get; set; }
    public string DocumentTip { get; set; }
    public string PaymentRefNo { get; set; }
    public DateOnly TransactionDate { get; set; }
    public string PaymentMethod { get; set; }
    public string PaymentMechanism { get; set; }
    public string Description { get; set; }
    public List<SaftLiniePlata> Linii { get; set; } = [];
    public decimal GrossTotal { get; set; }
}

public sealed class SaftLiniePlata {
    public Guid DetaliuId { get; set; }
    public int LineNumber { get; set; }
    // Numărul documentului stins, DACĂ imperecherea plății e unică.
    public string SourceDocumentID { get; set; }
    public string AccountID { get; set; }
    public string CustomerID { get; set; }
    public string SupplierID { get; set; }
    public string Description { get; set; }
    public string DebitCreditIndicator { get; set; }
    public decimal PaymentLineAmount { get; set; }
    public List<SaftAnaliza> Analiza { get; set; } = [];
    public SaftTaxInfo TaxInformation { get; set; }
}

// Ce NU intră în fișier, cu cauza și cifrele lui (D16-D4) — parte din CONTRACT,
// nu log: „un gard care tace devine capcană" (62f). Σ `Neincluse` + Σ emis ==
// Σ registrului, pe fiecare cusătură.
public sealed class SaftNeinclus {
    // `CauzaNeincludere` ca string (57a).
    public string Cauza { get; set; }
    // Secțiunea din care lipsește: „Customers", „Suppliers", „SalesInvoices"…
    public string Sectiune { get; set; }
    public string Sens { get; set; }
    public Guid? DocumentId { get; set; }
    public string DocumentNumar { get; set; }
    public string DocumentTip { get; set; }
    public Guid? DetaliuId { get; set; }
    public Guid? ContId { get; set; }
    public string ContSimbol { get; set; }
    public Guid? RepartitorId { get; set; }
    public string RepartitorDenumire { get; set; }
    // Cifrele fiscale (facturi) — null unde n-au sens.
    public decimal? Baza { get; set; }
    public decimal? Tva { get; set; }
    // Cifrele contabile (solduri de terți) — null unde n-au sens.
    public decimal? Debit { get; set; }
    public decimal? Credit { get; set; }
    public int Randuri { get; set; }

    // ── Felia 17 (SAF-T S): identitatea rândului de STOC care n-a intrat ─────
    // Aceeași clasă, nu una nouă: „ce nu intră și de ce" e UN vocabular, iar
    // ecranul îl citește la fel pe ambele declarații. Câmpurile rămân null pe L,
    // exact ca `Baza`/`Tva` pe S.
    public Guid? ProdusId { get; set; }
    public string ProdusCod { get; set; }
    // `TipStoc` ca string (57a) și semnul REGULII (nu al rândului: pe storno
    // semnul din registru e inversat) — împreună cu `DocumentTip` sunt exact
    // cheia politicii care lipsește.
    public string TipStoc { get; set; }
    public int? Semn { get; set; }
    public decimal? Cantitate { get; set; }
    public decimal? Valoare { get; set; }
    // Codul CULES din politică, pe cauza `CodMiscareNecunoscut` — singurul loc
    // în care „ce scrie în politică” e chiar informația care lipsește din
    // nomenclator. Null pe orice altă cauză.
    public string CodMiscare { get; set; }
}

// `Neincluse` AGREGAT per cauză — forma pe care o servește SUMARUL (F20-D5),
// aceeași ca `SaftAvertisment` (cod + număr + sumă + exemple), fiindcă e aceeași
// întrebare: „ce n-a intrat, de ce, cât, și dă-mi câteva”. Lista PLATĂ rămâne în
// `SaftDto.Neincluse` (fișierul + ModelCheck): nimic nu se pierde (73e). Ce se
// schimbă e ce pleacă pe sârmă — pe o lună reală lista are mii de rânduri pe
// care ecranul nu le citește unul câte unul, iar clientul o tăia la 200, adică
// ascundea TĂCUT restul (73-r10 / 74-r7). Agregatul e mărginit de numărul de
// CAUZE, deci merge întreg și nu minte.
public sealed class NeinclusAgregat {
    /// <summary>Câte exemple duce, cel mult, un agregat.</summary>
    public const int MaximExemple = 20;

    /// <summary>`CauzaNeincludere` ca string (57a) — cheia agregării.</summary>
    public string Cauza { get; set; }
    /// <summary>Câte INTRĂRI ale listei plate au cauza asta.</summary>
    public int Numar { get; set; }
    /// <summary>Σ `SaftNeinclus.Randuri` — câte rânduri de REGISTRU stau în spate.</summary>
    public int Randuri { get; set; }
    /// <summary>
    /// Cifra de bani a cauzei. Definiție UNICĂ, per rând, cu PRECEDENȚĂ — un rând
    /// poartă exact o familie de cifre (celelalte sunt null prin construcție), iar
    /// familiile nu se amestecă:
    /// <list type="bullet">
    /// <item>S (stocuri) ⇒ `Valoare`, SEMNATĂ ca în registru. Așa Σ pe agregate e
    /// chiar `SaftRezumat.NeincluseStocValoare`, adică termenul cusăturii S2 — iar
    /// un storno care anulează o gaură o anulează și aici.</item>
    /// <item>L, linii fiscale ⇒ |`Baza`|. Cusăturile de bani ale lui L își au deja
    /// termenii semnați în `Rezumat` (`BazaNeincluse*`); aici cifra răspunde la
    /// „cât de mare e problema”, iar valoarea absolută o ține vizibilă.</item>
    /// <item>L, solduri de terți (`Baza` null) ⇒ |`Debit`| + |`Credit`|. Un rând
    /// poartă în practică o singură latură, iar suma modulelor nu poate ieși 0
    /// dintr-o compensare care ar ascunde cauza.</item>
    /// </list>
    /// </summary>
    public decimal Suma { get; set; }
    /// <summary>
    /// Σ `Cantitate`, SEMNATĂ — doar pe S; null pe L, unde niciun rând n-o poartă.
    /// Convenția lui `SaftAvertisment.Suma`: null = „cauza asta n-are cantitate”,
    /// nu „cantitatea e zero”.
    /// </summary>
    public decimal? Cantitate { get; set; }
    /// <summary>
    /// PRIMELE ≤ <see cref="MaximExemple"/> rânduri, în ordinea (deterministă) a
    /// listei plate. NU e o paginare: e eșantionul din care omul recunoaște cazul,
    /// iar „câte din câte” se citește din `Numar`.
    /// </summary>
    public List<SaftNeinclus> Exemple { get; set; } = [];
}

// ═══ SAF-T S (stocuri) — felia 17, D17-D3 ═══════════════════════════════════
//
// ACELAȘI `SaftDto` (un DTO, un scriitor): S nu e un al doilea arbore, e ALTE
// secțiuni ale aceluiași `AuditFile`, selectate de `Header.HeaderComment` („C" =
// la cerere). Listele lui L rămân goale, listele de mai jos rămân goale pe L.

// `MasterFiles/MovementTypeTable/MovementTypeTableEntry` — codurile FOLOSITE în
// perioadă, cu descrierea din `SaftReguli.CoduriMiscare` (sursă unică).
public sealed class SaftTipMiscare {
    public string Cod { get; set; }
    public string Descriere { get; set; }
}

// `SourceDocuments/MovementOfGoods/StockMovement` (4.4). Unitatea e
// `(Document × Storno × CodMiscare)`: un document cu două coduri (ASM: produce
// `20`, consumă `70`) se SPARGE, iar jumătatea de storno e o mișcare proprie —
// exact convenția lui `381` de la facturi.
public sealed class SaftMiscareStoc {
    public Guid DocumentId { get; set; }
    public bool Storno { get; set; }
    public string MovementReference { get; set; }
    public DateOnly MovementDate { get; set; }
    // `DataOperare` a documentului (opțional în schemă) — null = absent.
    public DateOnly? MovementPostingDate { get; set; }
    public string MovementType { get; set; }
    // `DocumentReference` = tipul + numărul documentului din model.
    public string DocumentType { get; set; }
    public string DocumentNumber { get; set; }
    public string TransactionId { get; set; }
    public List<SaftLinieMiscareStoc> Linii { get; set; } = [];
}

// `StockMovementLine` — un rând de `RegistruStoc`, unu la unu.
public sealed class SaftLinieMiscareStoc {
    public Guid RandRegistruId { get; set; }
    public Guid? DetaliuId { get; set; }
    public int LineNumber { get; set; }
    // Contul de stoc al produsului (`TipMaterial.ContImplicit`). Obligatoriu —
    // lipsa lui scoate linia în `Neincluse`, nu inventează un cont (73e).
    public string AccountId { get; set; }
    // Convenția S: latura liberă e `"0"`, nu raportorul (vezi `TertiLinieStoc`).
    public string CustomerId { get; set; }
    public string SupplierId { get; set; }
    public Guid ProdusId { get; set; }
    public string ProductCode { get; set; }
    public Guid LotId { get; set; }
    public Guid RepartitorId { get; set; }
    // Identificatorul lotului — DOAR când produsul are > 1 lot în gestiune
    // (ghid p. 36); altfel absent.
    public string StockAccountNo { get; set; }
    // SEMNATĂ ca în registru (intrare +, ieșire −, stornoul inversat).
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; }
    public decimal UomConversionFactor { get; set; }
    public decimal BookValue { get; set; }
    // Același nomenclator ca `MovementType` — schema le cere pe amândouă.
    public string MovementSubType { get; set; }
    public string MovementComments { get; set; }
}

// `MasterFiles/PhysicalStock/PhysicalStockEntry` (2.10) — o intrare per
// `(gestiune × lot)` pe `TipStoc`-urile RAPORTATE: lotul E „prețul unitar
// aplicabil" al ghidului (p. 36), iar identificarea specifică (decizia 13) face
// granularitatea asta exactă, nu aproximativă.
public sealed class SaftStocFizic {
    public Guid RepartitorId { get; set; }
    public Guid LotId { get; set; }
    public Guid ProdusId { get; set; }
    public string WarehouseId { get; set; }
    public string ProductCode { get; set; }
    public string StockAccountNo { get; set; }
    public string ProductType { get; set; }
    public string StockAccountCommodityCode { get; set; }
    public string OwnerId { get; set; }
    public string UomPhysicalStock { get; set; }
    public decimal UomConversionFactor { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal OpeningQuantity { get; set; }
    public decimal OpeningValue { get; set; }
    public decimal ClosingQuantity { get; set; }
    public decimal ClosingValue { get; set; }
    public string StockCharacteristic { get; set; }
    public string StockCharacteristicValue { get; set; }
}

// Ce nu intră în fișier fiindcă AȘA S-A DECIS — agregat per politică, cu
// motivul scris de om lângă cifre. Deosebirea de `Neincluse` e de FOND: acolo e
// o gaură (politică lipsă), aici e o alegere (`PoliticaMiscareSaft.CodMiscare`
// null + `Motiv`). Cazul real: `+Consum` de pe bonul de consum — consumul rămâne
// pe responsabil (27a), dar nu mai e stoc în magazie.
public sealed class SaftExclus {
    public string TipDocument { get; set; }
    public string TipStoc { get; set; }
    // Semnul REGULII (`null` = orice semn), nu al rândului.
    public int? Semn { get; set; }
    public string Motiv { get; set; }
    // Câte rânduri de registru — ca `SaftAvertisment.Numar`, nu un număr de
    // document.
    public int Numar { get; set; }
    public decimal Cantitate { get; set; }
    public decimal Valoare { get; set; }
}

// O linie a cusăturii S3: soldul de stoc declarat pe un cont vs. `Balanta` pe
// ACELAȘI cont. Raportată, NU blocantă — registrul contabil poate purta 3xx și
// din note contabile sau deschideri fără lot, iar diferența e un fapt de citit,
// nu o eroare de generare.
public sealed class SaftDiferentaCont {
    public string Cont { get; set; }
    /// <summary>
    /// GUID-ul contului din spatele simbolului (F20-D5), ca S3 să poată trimite
    /// la fișă: cheia comparației e SIMBOLUL normalizat
    /// (`SaftReguli.ProductTypeDinCont`), iar fișa cere id-ul. Când mai multe
    /// conturi cad pe același simbol (analitice tăiate de normalizare) e cel cu
    /// simbolul cel mai scurt, apoi cel mai mic ordinal — adică sinteticul, care
    /// e chiar contul pe care omul îl deschide. `Guid.Empty` = niciun cont în
    /// spate (`SaftReguli.ProductTypeImplicit`, produsul fără cont de stoc): nu
    /// se inventează unul (73e), iar ecranul nu pune link.
    /// </summary>
    public Guid ContId { get; set; }
    public decimal ClosingStocFizic { get; set; }
    public decimal ClosingBalanta { get; set; }
    public decimal Diferenta { get; set; }
    // Diferența SPARTĂ pe tipul documentului care a produs-o (fixul F7 al
    // review-ului): „371 diferă cu 194.122,31” e o cifră pe care n-o poți
    // acționa, „din care NTC 194.000 și DSC 122,31” e. `Σ Componente.Diferenta
    // == Diferenta`, prin construcție — ambele laturi ies din ACELEAȘI două
    // agregate.
    public List<SaftComponentaCont> Componente { get; set; } = [];
}

// O componentă a diferenței S3: cât pune un TIP de document (sau deschiderea,
// care n-are document — 25e) de o parte și de alta a comparației. Cheia e codul
// tipului, nu documentul: pe o bază reală sunt zeci de mii de documente și
// douăzeci de tipuri.
public sealed class SaftComponentaCont {
    /// <summary>Codul tipului de document, sau „(deschidere)” pentru rândurile fără document.</summary>
    public string TipDocument { get; set; }
    public decimal StocFizic { get; set; }
    public decimal Balanta { get; set; }
    public decimal Diferenta { get; set; }
}

// Avertisment AGREGAT per cauză — aceeași formă ca `D394Avertisment`.
public sealed class SaftAvertisment {
    public string Cod { get; set; }
    public string Mesaj { get; set; }
    public int Numar { get; set; }
    public decimal? Suma { get; set; }
    public List<string> Exemple { get; set; } = [];
}

// CUSĂTURILE, calculate ÎN proiecție și RAPORTATE ca cifre (nu aruncate):
// ModelCheck le probează la cent, ecranul le arată, iar o divergență pe date
// reale devine un fapt măsurat, nu o excepție care oprește generarea.
public sealed class SaftRezumat {
    public int Tranzactii { get; set; }
    public int LiniiGl { get; set; }
    public int RanduriRegistru { get; set; }

    // (1) Partida dublă: `TotalDebit == TotalCredit == Σ RegistruContabil.Valoare`
    //     pe perioadă (semnat, doar rândurile cu document — cele de deschidere nu
    //     intră în GL).
    public decimal TotalDebit { get; set; }
    public decimal TotalCredit { get; set; }
    public decimal ValoareRegistruContabil { get; set; }

    // (2) TVA: Σ `TaxAmount` de pe rândurile GL cu taxă == Σ `RegistruTva.Tva`
    //     MINUS regimurile care nu postează (Capitalizat — TVA-ul e în cost, deci
    //     n-are rând contabil de TVA). Cifra se raportează, nu se ascunde.
    public decimal TvaGl { get; set; }
    public decimal TvaRegistru { get; set; }
    public decimal TvaCapitalizat { get; set; }
    // Al treilea termen al aceleiași cusături: taxa rândurilor al căror `TipTva`
    // n-are cod SAF-T pe direcția folosită. Ele ATING conturile de TVA (deci
    // există în contabilitate), dar ies `000/000000` — cifra nu se pierde, se
    // numără separat. `TvaGl + TvaCapitalizat + TvaFaraCodSaft == TvaRegistru`.
    public decimal TvaFaraCodSaft { get; set; }

    // (3) Facturi: pentru fiecare sens, Σ bazei rândurilor fiscale AȘEZATE pe
    //     linii de factură + Σ bazei celor NEINCLUSE == Σ `RegistruTva.Baza` al
    //     documentelor de tip factură.
    public decimal BazaFacturiAchizitie { get; set; }
    public decimal BazaFacturiLivrare { get; set; }
    public decimal BazaNeincluseAchizitie { get; set; }
    public decimal BazaNeincluseLivrare { get; set; }
    public decimal BazaRegistruAchizitie { get; set; }
    public decimal BazaRegistruLivrare { get; set; }

    // (4) Soldurile finale ale GLA == balanța (`ContabilProiectii.Balanta`),
    //     PER CONT. Suma netă (`ClosingGla`/`ClosingBalanta`) rămâne raportată,
    //     dar ea singură nu e o cusătură: două conturi cu erori de semn opus se
    //     anulează în ea (fixul F3 al review-ului). Cusătura e
    //     `ConturiDiferite == 0` peste `ConturiVerificate` conturi, cu
    //     `SumaAbsolutaClosing` ca martor al faptului că s-a verificat ceva —
    //     zero conturi diferite din zero conturi verificate nu e o probă.
    public decimal ClosingGla { get; set; }
    public decimal ClosingBalanta { get; set; }
    public int ConturiVerificate { get; set; }
    public int ConturiDiferite { get; set; }
    public decimal SumaAbsolutaClosing { get; set; }

    // (6) Terții: Σ soldurilor finale declarate în `Customers` + Σ soldurilor
    //     rămase în `Neincluse[Customers]` == Σ `Closing` din GLA pe conturile
    //     cu `RolTert == Client`; idem `Suppliers`/Furnizor. E cusătura care
    //     leagă master files de planul de conturi: un partener pierdut pe drum
    //     (repartitor care nu e partener, rând fără nicio latură de partener)
    //     nu mai poate dispărea tăcut — ori e într-o listă, ori e în `Neincluse`.
    public decimal ClosingClienti { get; set; }
    public decimal NeincluseClienti { get; set; }
    public decimal ClosingGlaClienti { get; set; }
    public decimal ClosingFurnizori { get; set; }
    public decimal NeincluseFurnizori { get; set; }
    public decimal ClosingGlaFurnizori { get; set; }

    public decimal NetTotalEmise { get; set; }
    public decimal NetTotalPrimite { get; set; }
    // Brutul facturilor și totalul plăților: cifre de REZUMAT pentru ecran, nu
    // cusături. Stau aici, nu în TS, fiindcă „TS nu calculează niciodată
    // sold/rest/total" (42c) — clientul numără rânduri, serverul adună bani.
    public decimal GrossTotalEmise { get; set; }
    public decimal GrossTotalPrimite { get; set; }
    public decimal TotalPlati { get; set; }

    public int NumarClienti { get; set; }
    public int NumarFurnizori { get; set; }
    public int NumarFacturiEmise { get; set; }
    public int NumarFacturiPrimite { get; set; }
    public int NumarPlati { get; set; }
    public int NumarProduse { get; set; }

    // ══ Cusăturile declarației S (felia 17, D17-D3) ══════════════════════════
    // Validatorul ANAF nu face NICIO aritmetică pe stocuri (Opening + intrări −
    // ieșiri = Closing nu e verificat, totalurile n-au regulă). Deci cusăturile
    // de mai jos sunt SINGURA garanție că fișierul spune adevărul — de aceea
    // sunt cifre în contract, nu aserțiuni ascunse în proiecție.

    public int NumarMiscari { get; set; }
    public int NumarLiniiMiscare { get; set; }
    public int NumarStocFizic { get; set; }
    public int NumarTipuriMiscare { get; set; }
    public int RanduriRegistruStoc { get; set; }

    // (S1) Pe FIECARE intrare de `PhysicalStock`: `Opening + Σ Quantity (lot ×
    //      gestiune, mișcările lunii) == Closing`, pe cantitate ȘI pe valoare.
    //      `StocIntrariDiferite == 0` peste `StocIntrari` intrări e cusătura;
    //      sumele globale sunt martorul că s-a verificat ceva. Un rând de
    //      registru al lunii care N-A intrat în fișier (fără politică, sau
    //      exclus pe un `TipStoc` raportat) se vede AICI, ca diferență.
    public int StocIntrari { get; set; }
    public int StocIntrariDiferite { get; set; }
    public decimal StocOpeningCantitate { get; set; }
    public decimal StocOpeningValoare { get; set; }
    public decimal StocMiscariCantitate { get; set; }
    public decimal StocMiscariValoare { get; set; }
    public decimal StocClosingCantitate { get; set; }
    public decimal StocClosingValoare { get; set; }

    // (S5) ACEEAȘI egalitate ca S1, dar cu Σ luată din LINIILE EMISE ÎN FIȘIER,
    //      nu din registru (fixul F2 al review-ului). S1 compară trei
    //      interogări pe registru — deci probează aritmetica agregatelor, dar
    //      nu spune NIMIC despre ce s-a scris în `MovementOfGoods`: dacă un
    //      rând a ieșit în `Neincluse`, S1 rămâne verde și fișierul e totuși
    //      incomplet. S5 pune fișierul de o parte a semnului egal. Când nimic
    //      nu se pierde, S5 ≡ S1; când se pierde ceva, S5 spune pe CE intrări.
    public int StocFizicVsMiscariDiferite { get; set; }
    public decimal StocEmiseCantitate { get; set; }
    public decimal StocEmiseValoare { get; set; }

    // Verdictul cumulat al lui S1 ȘI S5: registrul se închide pe sine ȘI
    // fișierul spune același lucru.
    public bool StocFizicBate { get; set; }

    // (S2) Nimic nu se pierde: `Σ mișcări + Σ Excluse + Σ Neincluse == Σ
    //      RegistruStoc` pe documentele lunii, pe TOATE `TipStoc`-urile
    //      (inclusiv cele neraportate — altfel egalitatea s-ar măsura pe sine).
    public decimal MiscariCantitate { get; set; }
    public decimal MiscariValoare { get; set; }
    public decimal ExcluseCantitate { get; set; }
    public decimal ExcluseValoare { get; set; }
    public decimal NeincluseStocCantitate { get; set; }
    public decimal NeincluseStocValoare { get; set; }
    public decimal RegistruStocCantitate { get; set; }
    public decimal RegistruStocValoare { get; set; }
    public bool RegistruStocBate { get; set; }

    // (S3) Σ `ClosingStockValue` per cont de stoc vs. `Closing` din `Balanta` pe
    //      același cont — MĂSURATĂ ȘI RAPORTATĂ, nu blocantă: registrul contabil
    //      poartă 3xx și din note contabile ori deschideri fără lot.
    public List<SaftDiferentaCont> StocPerCont { get; set; } = [];
    public decimal ClosingStocFizic { get; set; }
    public decimal ClosingBalantaStoc { get; set; }
    public int ConturiStocVerificate { get; set; }
    public int ConturiStocDiferite { get; set; }

    // (S4) Integritatea referințelor din fișier: fiecare `ProductCode` de pe
    //      mișcări și din stocul fizic e în `Products`, fiecare cod de mișcare e
    //      în `MovementTypeTable`, fiecare identificator de terț nenul are
    //      format valid (prefix `00`–`06` sau literalul `0`).
    public int ProduseReferite { get; set; }
    public int ProduseLipsa { get; set; }
    public int CoduriMiscareFolosite { get; set; }
    public int CoduriMiscareLipsa { get; set; }
    public int IdentitatiTertInvalide { get; set; }
    // Câte `MovementReference` se repetă în fișier (fixul F1): identitatea unei
    // mișcări e chiar referința, deci o repetiție e o coliziune de identitate,
    // nu un detaliu de prezentare. Zero prin construcție după discriminant —
    // cusătura există ca să se VADĂ dacă discriminantul n-a fost de ajuns.
    public int ReferinteDuplicate { get; set; }
    public bool ReferinteBat { get; set; }
}

public sealed class SaftDto {
    // Nenul ⇒ declarația NU se aplică bazei (profil bugetar: planul instituțiilor
    // publice nu e printre cele 12 `TaxAccountingBasis`), iar restul e GOL prin
    // construcție — nicio interogare pe registre nu s-a executat.
    public string Neaplicabil { get; set; }
    public int An { get; set; }
    public int Luna { get; set; }
    public DateOnly DataStart { get; set; }
    public DateOnly DataEnd { get; set; }

    public SaftHeader Header { get; set; }
    public List<SaftCont> Conturi { get; set; } = [];
    public List<SaftTert> Clienti { get; set; } = [];
    public List<SaftTert> Furnizori { get; set; } = [];
    public List<SaftTaxCode> Taxe { get; set; } = [];
    public List<SaftUnitate> Unitati { get; set; } = [];
    public List<SaftTipAnaliza> TipuriAnaliza { get; set; } = [];
    public List<SaftProdus> Produse { get; set; } = [];
    public List<SaftJurnal> Jurnale { get; set; } = [];
    public List<SaftFactura> FacturiEmise { get; set; } = [];
    public List<SaftFactura> FacturiPrimite { get; set; } = [];
    public List<SaftPlata> Plati { get; set; } = [];

    // ── Secțiunile modulului S (goale pe L, ca `Jurnale` pe S) ──────────────
    public List<SaftTipMiscare> TipuriMiscare { get; set; } = [];
    public List<SaftStocFizic> StocFizic { get; set; } = [];
    public List<SaftMiscareStoc> MiscariStoc { get; set; } = [];
    // Totalurile lui `MovementOfGoods` (schema le cere lângă listă). Calculate de
    // SERVER, ca tot restul: „TS nu calculează niciodată sold/rest/total" (42c).
    public int NumberOfMovementLines { get; set; }
    public decimal TotalQuantityReceived { get; set; }
    public decimal TotalQuantityIssued { get; set; }

    public List<SaftNeinclus> Neincluse { get; set; } = [];
    // Excluderile DELIBERATE (politică fără cod, cu motiv) — altă listă decât
    // `Neincluse`, fiindcă sunt alt fapt: o alegere, nu o gaură.
    public List<SaftExclus> Excluse { get; set; } = [];
    public List<SaftAvertisment> Avertismente { get; set; } = [];
    public SaftRezumat Rezumat { get; set; } = new();
}

// ═══ SUMARUL — ce se servește pe `GET api/proiectii/saft` (D16-D5, amendat) ═══
//
// `SaftDto` întreg E FIȘIERUL: pe o lună reală (clona Flax.Api, 09/2025) sunt
// 38,6 MiB de JSON — 13.966 tranzacții × ~4 linii de GL, 5.638 terți, 5.388
// facturi, 3.000 plăți. Regula moștenită de la D300/D394 („formularul nu se
// paginează") spunea că răspunsul e declarația ÎNTREAGĂ; datele au contrazis-o:
// ecranul nu afișează niciuna dintre liniile alea, iar browserul plătește
// oricum parsarea lor. Deci ușa JSON servește un SUMAR, iar listele trăiesc în
// exact două locuri — FIȘIERUL (`saft/xml`, streaming) și `SaftProiectii.Saft()`
// pentru ModelCheck. Nu e o paginare: e ALTĂ întrebare („cât e declarația?"),
// cu răspuns complet.
//
// Contoarele sunt ale SERVERULUI, nu lungimi numărate în TS (42c): clientul nu
// primește listele, deci nu le poate număra — iar dacă le-ar primi, tot n-ar
// avea voie. Totalurile per secțiune (debit/credit, net/brut, plăți) sunt deja
// în `Rezumat`, care merge întreg: e contractul cusăturilor, neschimbat.
public sealed class SaftSumarDto {
    public string Neaplicabil { get; set; }
    public int An { get; set; }
    public int Luna { get; set; }
    public DateOnly DataStart { get; set; }
    public DateOnly DataEnd { get; set; }

    // Antetul merge ÎNTREG: e un singur rând și e chiar ce se citește prima dată
    // pe o declarație („cine declară, pe ce perioadă, pe ce bază contabilă").
    public SaftHeader Header { get; set; }

    // Contoarele per secțiune, în ordinea în care secțiunile apar în fișier.
    // `Jurnale` = câte jurnale (tipuri de document), `Tranzactii` = câte
    // documente în total, `LiniiGl` = câte linii de tranzacție.
    public int Conturi { get; set; }
    public int Clienti { get; set; }
    public int Furnizori { get; set; }
    public int CoduriTaxa { get; set; }
    public int Unitati { get; set; }
    public int Produse { get; set; }
    public int TipuriAnaliza { get; set; }
    public int Jurnale { get; set; }
    public int Tranzactii { get; set; }
    public int LiniiGl { get; set; }
    public int FacturiEmise { get; set; }
    public int FacturiPrimite { get; set; }
    public int Plati { get; set; }

    // Contoarele modulului S (zero pe L, ca cele de mai sus pe S). `Excluse`
    // merge ÎNTREG, ca `Neincluse`: e mărginit de numărul de POLITICI (21 la
    // privat), nu de volumul registrului, și e exact partea pe care omul o
    // citește înainte de a depune.
    public int TipuriMiscare { get; set; }
    public int StocFizic { get; set; }
    public int MiscariStoc { get; set; }
    public int LiniiMiscare { get; set; }
    public List<SaftExclus> Excluse { get; set; } = [];

    // Cusăturile, `Neincluse` și avertismentele merg NESCHIMBATE: sunt mărginite
    // de numărul de CAUZE (avertismentele) sau de ce n-a intrat în fișier
    // (`Neincluse`) — adică exact partea pe care omul o citește înainte de a
    // depune. Ele sunt motivul pentru care ecranul există.
    public SaftRezumat Rezumat { get; set; } = new();
    // `Neincluse` merge AGREGAT per cauză (F20-D5): singura listă a sumarului
    // care nu era mărginită de nimic — pe o lună reală are mii de rânduri, iar
    // clientul o tăia la 200 fără să spună. Lista plată rămâne în `SaftDto`
    // (fișierul + ModelCheck); aici e răspunsul complet la întrebarea pe care
    // ecranul chiar o pune. Σ `Numar` == `SaftDto.Neincluse.Count` e cusătură
    // probată în ModelCheck, nu presupunere.
    public List<NeinclusAgregat> Neincluse { get; set; } = [];
    public List<SaftAvertisment> Avertismente { get; set; } = [];
}
