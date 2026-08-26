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
    public List<SaftNeinclus> Neincluse { get; set; } = [];
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

    // Cusăturile, `Neincluse` și avertismentele merg NESCHIMBATE: sunt mărginite
    // de numărul de CAUZE (avertismentele) sau de ce n-a intrat în fișier
    // (`Neincluse`) — adică exact partea pe care omul o citește înainte de a
    // depune. Ele sunt motivul pentru care ecranul există.
    public SaftRezumat Rezumat { get; set; } = new();
    public List<SaftNeinclus> Neincluse { get; set; } = [];
    public List<SaftAvertisment> Avertismente { get; set; } = [];
}
