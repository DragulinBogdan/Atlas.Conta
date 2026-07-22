# Mapare Entități: Delphi → .NET Core XAF + EF Core

## TABELE PRINCIPALE → BUSINESS OBJECTS

### 1. MODUL CONTABILITATE

#### CPLAN → ChartOfAccounts
```csharp
public class ChartOfAccounts : BaseObject
{
    [Key]
    public virtual int Id { get; set; }

    // Mapare coloane CPLAN
    [Size(100), Required, Index(IsUnique = true)]
    public virtual string AccountCode { get; set; } // CONT

    [Size(1000)]
    public virtual string Description { get; set; } // ROMANA

    [Size(100)]
    public virtual string ParentAccount { get; set; } // PARINTE

    public virtual decimal InitialDebitBalance { get; set; } // SID
    public virtual decimal InitialCreditBalance { get; set; } // SIC
    public virtual decimal PeriodDebitBalance { get; set; } // SPD
    public virtual decimal PeriodCreditBalance { get; set; } // SPC
    public virtual decimal CurrentDebitBalance { get; set; } // SD
    public virtual decimal CurrentCreditBalance { get; set; } // SC

    public virtual bool IsSummary { get; set; } // SUMATOR
    public virtual bool IsAnalytic { get; set; } // IS_ANALITIC
    public virtual bool IsSynthetic { get; set; } // IS_SINTETIC

    [Size(1)]
    public virtual string AccountType { get; set; } // FCTCONT (D/C/B)

    [Size(1)]
    public virtual string Type { get; set; } // TIP

    [Size(20)]
    public virtual string BalanceType { get; set; } // BALANTA

    public virtual int? AccountLevel { get; set; } // CONT_LEVEL
    public virtual int? FiscalYear { get; set; } // AN_FISCAL

    [Size(3000)]
    public virtual string AccountClass { get; set; } // CLASA

    [Size(1)]
    public virtual string RegistryType { get; set; } // TIP_REGISTRU

    // Navigation properties
    [Association("ChartOfAccounts-DebitEntries")]
    public virtual IList<AccountingEntry> DebitEntries { get; set; }

    [Association("ChartOfAccounts-CreditEntries")]
    public virtual IList<AccountingEntry> CreditEntries { get; set; }
}
```

**Mapare EF Core:**
```csharp
modelBuilder.Entity<ChartOfAccounts>(entity =>
{
    entity.ToTable("CPLAN");
    entity.HasKey(e => e.Id);
    entity.Property(e => e.Id).HasColumnName("ID_CPLAN");
    entity.Property(e => e.AccountCode).HasColumnName("CONT").HasMaxLength(100);
    entity.Property(e => e.Description).HasColumnName("ROMANA").HasMaxLength(1000);
    entity.Property(e => e.ParentAccount).HasColumnName("PARINTE").HasMaxLength(100);
    // ... etc
});
```

---

#### CNOTE → AccountingEntry
```csharp
public class AccountingEntry : BaseObject
{
    [Key]
    public virtual int Id { get; set; } // NR

    // Identificare
    [Size(254)]
    public virtual string EntryNumber { get; set; } // NRDOC

    public virtual DateTime EntryDate { get; set; } // DATA

    [Size(254)]
    public virtual string Journal { get; set; } // JURNAL

    // Descriere
    [Size(4096)]
    public virtual string Description { get; set; } // EXPLICATIE

    // Sume
    public virtual decimal Amount { get; set; } // VALOARE

    // Conturi
    [Size(100), Required]
    public virtual string DebitAccount { get; set; } // CONTD

    [Size(100), Required]
    public virtual string CreditAccount { get; set; } // CONTC

    [Size(100)]
    public virtual string DebitAccountOriginal { get; set; } // CONT_DEBT (pentru note compuse)

    [Size(100)]
    public virtual string CreditAccountOriginal { get; set; } // CONT_CRED

    // Centre de cost / Repartitori
    public virtual int? DebitCostCenterId { get; set; } // REPARTITOR_DEBIT
    public virtual int? CreditCostCenterId { get; set; } // REPARTITOR_CREDIT

    [Association("CostCenter-DebitEntries")]
    public virtual CostCenter DebitCostCenter { get; set; }

    [Association("CostCenter-CreditEntries")]
    public virtual CostCenter CreditCostCenter { get; set; }

    // Clasificare bugetară
    [Size(100)]
    public virtual string FunctionalCode { get; set; } // COD_FUNCTIONAL

    [Size(100)]
    public virtual string DebitFunctionalCode { get; set; } // COD_FUNCTIONAL_D

    [Size(100)]
    public virtual string CreditFunctionalCode { get; set; } // COD_FUNCTIONAL_C

    [Size(100)]
    public virtual string EconomicCode { get; set; } // COD_ECONOMIC

    [Size(100)]
    public virtual string DebitEconomicCode { get; set; } // COD_ECONOMIC_D

    [Size(100)]
    public virtual string CreditEconomicCode { get; set; } // COD_ECONOMIC_C

    // Unități și proiecte
    public virtual int? OrganizationalUnitId { get; set; } // ID_OI_UNITATI
    public virtual int? DebitOrganizationalUnitId { get; set; } // ID_OI_UNITATI_D
    public virtual int? CreditOrganizationalUnitId { get; set; } // ID_OI_UNITATI_C

    public virtual int? ProjectId { get; set; } // ID_OI_PROIECTE
    public virtual int? DebitProjectId { get; set; } // ID_OI_PROIECTE_D
    public virtual int? CreditProjectId { get; set; } // ID_OI_PROIECTE_C

    // Stare și workflow
    public virtual int Status { get; set; } // STARE (0=draft, 1=posted, etc.)
    public virtual int? EntryType { get; set; } // TIP_NOTA
    public virtual bool IsCompound { get; set; } // COMPUSA
    public virtual int? CompoundId { get; set; } // COMPUSA (ID)
    public virtual int? ParentId { get; set; } // ID_PARINTE
    public virtual int? InitialId { get; set; } // ID_INITIAL

    // Module sursă
    public virtual int? SourceModule { get; set; } // MODUL (-1=conta, 1=gestiune, etc.)
    public virtual int? SourceModuleUniqueId { get; set; } // ID_UNIC_MODUL
    public virtual int? SourceDocumentId { get; set; } // ID_DOCUMENT_MODUL
    public virtual int? InventoryCode { get; set; } // CODGEST

    // Documente asociate
    [Size(128)]
    public virtual string DocumentType { get; set; } // TIP_DOCUMENT / COD_DOCUMENT

    [Size(254)]
    public virtual string DocumentNumber { get; set; } // NR_DOCUMENT

    public virtual DateTime? DocumentDate { get; set; } // DATA_DOCUMENT

    [Size(255)]
    public virtual string RelatedDocument { get; set; } // DOCUMENT

    [Size(255)]
    public virtual string RelatedDocumentConex { get; set; } // DOCUMENT_CONEX

    public virtual int? RelatedDocumentId { get; set; } // ID_DOCUMENT_CONEX

    // Material/Analitic
    public virtual int? MaterialId { get; set; } // ID_MATERIAL

    [Size(128)]
    public virtual string MaterialName { get; set; } // NUME_MATERIAL

    [Size(128)]
    public virtual string MaterialAnalyticCode { get; set; } // ANALITIC_MATERIAL

    public virtual int? AnalyticId { get; set; } // ID_ANALITIC

    // ALOP / Buget
    public virtual int? CommitmentDecompositionId { get; set; } // ID_ANGAJAMENTE_DEFALCARE
    public virtual int? ALOPCommitmentDecompositionId { get; set; } // ID_ALOP_ANGAJAMENTE_DEFALCARE
    public virtual int? OrderingDecompositionId { get; set; } // ID_ORDONANTARE_DEFALCARE

    // Contract
    public virtual int? ContractId { get; set; } // ID_CONTRACTE

    [Size(64)]
    public virtual string ContractNumber { get; set; } // NR_CONTRACT

    public virtual DateTime? ContractDate { get; set; } // DATA_CONTRACT

    // Plăți
    public virtual int? PaymentOrderId { get; set; } // ID_ORDIN_PLATA
    public virtual int? PaymentOrderNumber { get; set; } // NR_OP
    public virtual bool? IsPayment { get; set; } // ESTE_PLATA

    [Size(10)]
    public virtual string BudgetOperationNumber { get; set; } // BUGET_NR_OP

    public virtual DateTime? DueDate { get; set; } // DATA_SCADENTA

    // Audit și istoric
    public virtual int? CreatedByUserId { get; set; } // C_O

    [Association("ApplicationUser-CreatedEntries")]
    public virtual ApplicationUser CreatedBy { get; set; }

    public virtual DateTime? OperationDate { get; set; } // DATA_OPERARE
    public virtual DateTime? ReferenceDate { get; set; } // DATA_REFERINTA
    public virtual DateTime? GenerationDate { get; set; } // DATA_GENERARE
    public virtual DateTime? OperationDateOp { get; set; } // DATA_OP

    public virtual int? ModifiedByUserId { get; set; } // ID_UTILIZATOR_MODIFICARE
    public virtual DateTime? ModificationDate { get; set; } // DATA_MODIFICARE

    public virtual DateTime? DeletionDate { get; set; } // DATA_STERGERII

    // Altele
    [Size(11)]
    public virtual string Budget { get; set; } // BUGET

    public virtual byte[] ImportTimestamp { get; set; } // TIME_IMPORT

    [Size(100)]
    public virtual string ManualDescription { get; set; } // MAN_DEN
}
```

---

#### CJURNALE → Journal
```csharp
public class Journal : BaseObject
{
    [Key]
    public virtual int Id { get; set; }

    [Size(254), Required, Index(IsUnique = true)]
    public virtual string JournalCode { get; set; } // JURNAL

    [Size(254)]
    public virtual string Description { get; set; } // DENUMIRE

    public virtual bool IsClosingJournal { get; set; } // INCHIDERE

    // Navigation
    [Association("Journal-Entries")]
    public virtual IList<AccountingEntry> Entries { get; set; }
}
```

---

#### REPARTITORI → CostCenter
```csharp
public class CostCenter : BaseObject
{
    [Key]
    public virtual int Id { get; set; } // ID_REPARTITORI

    // Identificare
    [Size(50)]
    public virtual string Code { get; set; } // CODSECTIE

    [Size(1000), Required]
    public virtual string Name { get; set; } // NUME

    [Size(512)]
    public virtual string OldName { get; set; } // NUME_VECHI

    // Date identificare fiscală
    [Size(50)]
    public virtual string FiscalCode { get; set; } // COD_FISCAL

    [Size(50)]
    public virtual string CommerceRegistration { get; set; } // REG_COMERT

    [Size(50)]
    public virtual string TaxCode { get; set; } // SNM

    // Contact
    [Size(254)]
    public virtual string Address { get; set; } // ADRESA

    [Size(50)]
    public virtual string Phone { get; set; } // TELEFON

    [Size(50)]
    public virtual string Fax { get; set; } // FAX

    [Size(50)]
    public virtual string Email { get; set; } // EMAIL

    [Size(254)]
    public virtual string ContactPerson { get; set; } // PERSOANA_CONTACT

    [Size(254)]
    public virtual string WebAddress { get; set; } // ADRESA_WEB

    // Localizare
    public virtual int? CountryId { get; set; } // ID_TARI
    public virtual int? CountyId { get; set; } // ID_JUDETE
    public virtual int? LocalityId { get; set; } // ID_LOCALITATI
    public virtual int? SirutaCode { get; set; } // COD_SIRUTA

    // Conturi bancare
    [Size(100)]
    public virtual string BankAccount { get; set; } // CONT

    [Size(100)]
    public virtual string CheckAccount { get; set; } // CONT_CEC

    [Size(200)]
    public virtual string BankName { get; set; } // BANCA

    [Size(100)]
    public virtual string CorrespondenceAccount { get; set; } // CONT_CRSP

    // Clasificare
    public virtual bool IsInternal { get; set; } // GESTINT
    public virtual bool IsPreferred { get; set; } // PREFERAT
    public virtual bool IsGroupJournal { get; set; } // GRUP_LJ

    [Size(250)]
    public virtual string CostCenterType { get; set; } // TIP_REPARTITOR

    public virtual int? CostCenterGroupId { get; set; } // ID_REPARTITORI_GRUPE
    public virtual int? DomainId { get; set; } // ID_REPARTITORI_DOMENIU

    // Gestiune
    public virtual int? InventoryTypeId { get; set; } // ID_GEST_TIP_GEST
    public virtual int? InventoryManagementType { get; set; } // TIP_GESTIUNE

    public virtual decimal? DiscountRate { get; set; } // COTA_DISCOUNT
    public virtual decimal? MarkupRate { get; set; } // COTA_ADAOS

    public virtual DateTime? InitialStockDate { get; set; } // DATA_STOC_INI
    public virtual DateTime? InitialBalanceDate { get; set; } // DATA_SOLD_INI
    public virtual decimal? InitialBalance { get; set; } // SOLD_INITIAL

    // Buget
    [Size(254)]
    public virtual string FunctionalClass { get; set; } // CLASA_FUNCTIONALA

    [Size(100)]
    public virtual string FunctionalCode { get; set; } // COD_FUNCTIONAL

    // Ierarhie
    public virtual int? ParentId { get; set; } // ID_PARINTE

    [Association("CostCenter-Parent")]
    public virtual CostCenter Parent { get; set; }

    [Association("CostCenter-Children")]
    public virtual IList<CostCenter> Children { get; set; }

    // Utilizator responsabil
    public virtual int? ResponsibleUserId { get; set; } // ID_UTILIZATORI

    // Import/Migrare
    public virtual int? ImportedFlag { get; set; } // PRELUAT
    public virtual int? TransitionYearId { get; set; } // ID_TRECERE_AN

    // Coduri management
    public virtual int? MifGestCode { get; set; } // COD_MIFGEST
    public virtual int? ModuleCode { get; set; } // COD_MODULE

    // Manual/Legacy
    public virtual int? ManualCostCenterId { get; set; } // MAN_ID_REP
    public virtual int? ManualParentCostCenterId { get; set; } // MAN_ID_REP_PARENT

    [Size(31)]
    public virtual string ManualSymbol { get; set; } // MAN_SIMBOL

    public virtual int? ManualInt1 { get; set; } // MAN_I1

    [Size(100)]
    public virtual string ManualDescription { get; set; } // MAN_DEN

    [Size(100)]
    public virtual string ManualParentDescription { get; set; } // MAN_DENP

    public virtual string ManualAddress { get; set; } // MAN_ADRESA (MAX)

    // Navigation properties
    [Association("CostCenter-DebitEntries")]
    public virtual IList<AccountingEntry> DebitEntries { get; set; }

    [Association("CostCenter-CreditEntries")]
    public virtual IList<AccountingEntry> CreditEntries { get; set; }
}
```

---

### 2. MODUL BUGET

#### BG_VERSIUNE → BudgetVersion
```csharp
public class BudgetVersion : BaseObject
{
    [Key]
    public virtual int Id { get; set; } // ID_BG_VERSIUNE

    // Identificare
    public virtual int FiscalYear { get; set; } // AN_FISCAL
    public virtual int Version { get; set; } // VERSIUNE
    public virtual int Revision { get; set; } // REVIZIE

    public virtual int? BudgetTypeId { get; set; } // TIP_BUGET

    [Size(1)]
    public virtual string VersionType { get; set; } // TIP_VERSIUNE (I=inițial, A=actualizat, etc.)

    [Size(11)]
    public virtual string Description { get; set; } // DESCRIERE

    [Size(1000)]
    public virtual string Explanation { get; set; } // EXPLICATIE

    [Size(20)]
    public virtual string ApprovalDocument { get; set; } // ACT_APROBARE

    // Organizație
    public virtual int? OrganizationalUnitId { get; set; } // ID_OI_UNITATI
    public virtual int? ProjectId { get; set; } // ID_OI_PROIECTE

    [Size(254)]
    public virtual string FunctionalClass { get; set; } // CLASA_FUNCTIONALA

    // Creare
    public virtual int? CreatedByUserId { get; set; } // ID_UTILIZATORI_CREAT

    [Size(254)]
    public virtual string CreatedByDepartment { get; set; } // DEPARTAMENTE_CREAT

    public virtual DateTime? CreatedDate { get; set; } // DATA_CREARE

    // Aprobare
    public virtual int? ApprovedByUserId { get; set; } // ID_UTILIZATORI_APROBAT

    [Size(254)]
    public virtual string ApprovedByDepartment { get; set; } // DEPARTAMENTE_APROBAT

    public virtual DateTime? ApprovedDate { get; set; } // DATA_APROBARE

    [Size(254)]
    public virtual string ApprovalFunction { get; set; } // FUNCTIE

    // Stare
    public virtual bool IsBlocked { get; set; } // ISBLOCKED
    public virtual bool IsEstimated { get; set; } // ESTE_ESTIMAT
    public virtual bool TreasuryApproved { get; set; } // VIZAT_TREZORERIE
    public virtual DateTime? TreasuryApprovalDate { get; set; } // DATA_VIZA_TREZORERIE

    // Legacy
    [Size(100)]
    public virtual string ManualBudgetCode { get; set; } // MAN_CODBUGET

    // Navigation
    [Association("BudgetVersion-Allocations")]
    public virtual IList<BudgetAllocation> Allocations { get; set; }
}
```

#### BG_DESCHIDERE → BudgetAllocation
```csharp
public class BudgetAllocation : BaseObject
{
    [Key]
    public virtual int Id { get; set; } // ID (implicit)

    // Referință versiune buget
    public virtual int BudgetVersionId { get; set; } // ID_BG_VERSIUNE (FK)

    [Association("BudgetVersion-Allocations")]
    public virtual BudgetVersion BudgetVersion { get; set; }

    // Clasificare bugetară
    [Size(100)]
    public virtual string FunctionalCode { get; set; } // COD_FUNCTIONAL

    [Size(100)]
    public virtual string EconomicCode { get; set; } // COD_ECONOMIC

    // Organizație
    public virtual int? OrganizationalUnitId { get; set; } // ID_OI_UNITATI
    public virtual int? ProjectId { get; set; } // ID_OI_PROIECTE

    // Sume bugetare
    public virtual decimal InitialBudget { get; set; } // BUGET_INITIAL
    public virtual decimal ApprovedBudget { get; set; } // BUGET_APROBAT
    public virtual decimal AllocatedBudget { get; set; } // BUGET_DESCHIS
    public virtual decimal CommittedAmount { get; set; } // SUMA_ANGAJATA
    public virtual decimal LiquidatedAmount { get; set; } // SUMA_LICHIDATA
    public virtual decimal OrderedAmount { get; set; } // SUMA_ORDONANTATA
    public virtual decimal PaidAmount { get; set; } // SUMA_PLATITA

    // Calcul disponibil
    public decimal AvailableAmount => AllocatedBudget - CommittedAmount;
}
```

#### ALOP_ANGAJAMENTE → Commitment
```csharp
public class Commitment : BaseObject
{
    [Key]
    public virtual int Id { get; set; } // ID_ALOP_ANGAJAMENTE

    // Identificare
    [Size(50), Required]
    public virtual string CommitmentNumber { get; set; } // NR_ANGAJAMENT

    public virtual DateTime CommitmentDate { get; set; } // DATA_ANGAJAMENT

    public virtual int FiscalYear { get; set; } // AN_FISCAL

    // Sume
    public virtual decimal Amount { get; set; } // SUMA
    public virtual decimal AmountWithVAT { get; set; } // SUMA_TVA
    public virtual decimal LiquidatedAmount { get; set; } // SUMA_LICHIDATA
    public virtual decimal RemainingAmount { get; set; } // SUMA_RAMASA

    // Clasificare
    [Size(100)]
    public virtual string FunctionalCode { get; set; } // COD_FUNCTIONAL

    [Size(100)]
    public virtual string EconomicCode { get; set; } // COD_ECONOMIC

    public virtual int? OrganizationalUnitId { get; set; } // ID_OI_UNITATI
    public virtual int? ProjectId { get; set; } // ID_OI_PROIECTE

    // Beneficiar
    public virtual int? BeneficiaryId { get; set; } // ID_REPARTITORI (FK la REPARTITORI)

    [Association("CostCenter-Commitments")]
    public virtual CostCenter Beneficiary { get; set; }

    // Contract
    [Size(100)]
    public virtual string ContractNumber { get; set; } // NR_CONTRACT

    public virtual DateTime? ContractDate { get; set; } // DATA_CONTRACT

    // Obiect/Descriere
    [Size(500)]
    public virtual string Purpose { get; set; } // OBIECT

    [Size(2000)]
    public virtual string Description { get; set; } // DESCRIERE

    // Stare
    public virtual int Status { get; set; } // STARE (0=proiect, 1=aprobat, 2=anulat)
    public virtual bool IsMultiYear { get; set; } // MULTIANUAL

    // Tip angajament
    public virtual int? CommitmentTypeId { get; set; } // ID_ALOP_TIP_ANGAJAMENT

    // Audit
    public virtual int? CreatedByUserId { get; set; } // ID_UTILIZATOR_CREAT
    public virtual DateTime? CreatedDate { get; set; } // DATA_CREARE
    public virtual DateTime? OperationDate { get; set; } // DATA_OPERARE

    // Navigation
    [Association("Commitment-Decompositions")]
    public virtual IList<CommitmentDecomposition> Decompositions { get; set; }

    [Association("Commitment-Liquidations")]
    public virtual IList<Liquidation> Liquidations { get; set; }
}
```

#### ALOP_ANGAJAMENTE_DEFALCARE → CommitmentDecomposition
```csharp
public class CommitmentDecomposition : BaseObject
{
    [Key]
    public virtual int Id { get; set; } // ID

    // Referință angajament
    public virtual int CommitmentId { get; set; } // ID_ALOP_ANGAJAMENTE (FK)

    [Association("Commitment-Decompositions")]
    public virtual Commitment Commitment { get; set; }

    // Clasificare detaliată
    [Size(100)]
    public virtual string FunctionalCode { get; set; } // COD_FUNCTIONAL

    [Size(100)]
    public virtual string EconomicCode { get; set; } // COD_ECONOMIC

    public virtual int? OrganizationalUnitId { get; set; } // ID_OI_UNITATI
    public virtual int? ProjectId { get; set; } // ID_OI_PROIECTE

    // Sumă
    public virtual decimal Amount { get; set; } // SUMA

    // Legătură cu notele contabile
    [Association("CommitmentDecomposition-Entries")]
    public virtual IList<AccountingEntry> AccountingEntries { get; set; }
}
```

#### ALOP_LICHIDARE → Liquidation
```csharp
public class Liquidation : BaseObject
{
    [Key]
    public virtual int Id { get; set; } // ID_ALOP_LICHIDARE

    // Referință angajament
    public virtual int CommitmentId { get; set; } // ID_ALOP_ANGAJAMENTE (FK)

    [Association("Commitment-Liquidations")]
    public virtual Commitment Commitment { get; set; }

    // Identificare
    [Size(50)]
    public virtual string LiquidationNumber { get; set; } // NR_LICHIDARE

    public virtual DateTime LiquidationDate { get; set; } // DATA_LICHIDARE

    // Document justificativ
    [Size(100)]
    public virtual string DocumentType { get; set; } // TIP_DOCUMENT

    [Size(100)]
    public virtual string DocumentNumber { get; set; } // NR_DOCUMENT

    public virtual DateTime? DocumentDate { get; set; } // DATA_DOCUMENT

    // Sume
    public virtual decimal Amount { get; set; } // SUMA
    public virtual decimal AmountWithVAT { get; set; } // SUMA_TVA
    public virtual decimal OrderedAmount { get; set; } // SUMA_ORDONANTATA

    // Stare
    public virtual int Status { get; set; } // STARE

    // Audit
    public virtual DateTime? OperationDate { get; set; } // DATA_OPERARE

    // Navigation
    [Association("Liquidation-Decompositions")]
    public virtual IList<LiquidationDecomposition> Decompositions { get; set; }

    [Association("Liquidation-PaymentOrders")]
    public virtual IList<PaymentOrder> PaymentOrders { get; set; }
}
```

#### ALOP_ORDONANTARE → PaymentOrder
```csharp
public class PaymentOrder : BaseObject
{
    [Key]
    public virtual int Id { get; set; } // ID_ALOP_ORDONANTARE

    // Referință lichidare
    public virtual int LiquidationId { get; set; } // ID_ALOP_LICHIDARE (FK)

    [Association("Liquidation-PaymentOrders")]
    public virtual Liquidation Liquidation { get; set; }

    // Identificare
    [Size(50)]
    public virtual string OrderNumber { get; set; } // NR_ORDONANTARE

    public virtual DateTime OrderDate { get; set; } // DATA_ORDONANTARE

    // Sumă
    public virtual decimal Amount { get; set; } // SUMA
    public virtual decimal PaidAmount { get; set; } // SUMA_PLATITA

    // Stare
    public virtual int Status { get; set; } // STARE

    // Audit
    public virtual DateTime? OperationDate { get; set; } // DATA_OPERARE
}
```

---

### 3. MODUL GESTIUNE

#### GEST_DOCUM → InventoryDocument
```csharp
public class InventoryDocument : BaseObject
{
    [Key]
    public virtual int Id { get; set; } // ID_GEST_DOCUM

    // Tip document
    public virtual int DocumentTypeId { get; set; } // ID_GEST_TIP_DOCUM (FK)

    [Association("InventoryDocumentType-Documents")]
    public virtual InventoryDocumentType DocumentType { get; set; }

    public virtual int? DocumentDefinitionId { get; set; } // ID_GEST_DEFA_DOCUM

    // Identificare
    [Size(100), Required]
    public virtual string DocumentNumber { get; set; } // NR_DOCUM

    public virtual DateTime DocumentDate { get; set; } // DATA_DOCUM

    public virtual DateTime? IssueDate { get; set; } // DATA_EMITERE
    public virtual DateTime? OldDocumentDate { get; set; } // OLD_DATA_DOCUM (pentru istoric)

    // Parteneri
    public virtual int? SupplierId { get; set; } // ID_PREDATOR
    public virtual int? ReceiverId { get; set; } // ID_PRIMITOR

    [Association("CostCenter-SuppliedDocuments")]
    public virtual CostCenter Supplier { get; set; }

    [Association("CostCenter-ReceivedDocuments")]
    public virtual CostCenter Receiver { get; set; }

    // Sume
    public virtual decimal? TotalAmount { get; set; } // TOTALDOC
    public virtual decimal? GoodsValue { get; set; } // COST_MARFA
    public virtual decimal? TotalVAT { get; set; } // TOTALTVA
    public virtual decimal? Markup { get; set; } // ADAOS
    public virtual decimal? Discount { get; set; } // DISCOUNT
    public virtual decimal? ExciseDuty { get; set; } // ACCIZE
    public virtual decimal? ExciseValue { get; set; } // VALACCIZE

    public virtual decimal? MarkupRate { get; set; } // COTA_ADAOS
    public virtual decimal? DiscountRate { get; set; } // COTA_DISCNT

    public virtual int? MarkupType { get; set; } // TIP_ADAOS
    public virtual int? DiscountType { get; set; } // TIP_DISCNT
    public virtual int? PriceType { get; set; } // TIP_PRET

    // Valută
    public virtual int? CurrencyTypeId { get; set; } // ID_TIP_VALUTA
    public virtual decimal? ExchangeRate { get; set; } // CURS_SCHIMB
    public virtual decimal? CompanyExchangeRate { get; set; } // CURS_FIRMA
    public virtual decimal? TotalForeignCurrency { get; set; } // TOTALVALUT

    public virtual int? ExchangeRateId { get; set; } // ID_CURSURI

    [Size(250)]
    public virtual string ExchangeRateDescription { get; set; } // DESCRIERE_CURS

    // Plăți
    public virtual decimal? PaidAmount { get; set; } // ACHITAT
    public virtual int? PaymentTermDays { get; set; } // SCADENTA (zile)
    public virtual DateTime? DueDate { get; set; } // DATA_SCADENTA

    // Delegat / Transport
    [Size(50)]
    public virtual string DelegateName { get; set; } // NUME_DELEGAT

    public virtual int? DelegateId { get; set; } // ID_REPARTITORI_DELEGATI

    [Size(20)]
    public virtual string TransportMeans { get; set; } // MIJLTRANSPORT

    public virtual int? TransportMeansId { get; set; } // ID_REPARTITORI_MIJLOACE_TRANSPORT
    public virtual int? TransporterId { get; set; } // ID_REPARTITORI_TRANSPORT

    public virtual decimal? TransportValue { get; set; } // VALOARE_TRANSPORT
    public virtual decimal? TransportVAT { get; set; } // TVA_TRANSPORT

    // Descriere / Explicații
    public virtual string Description { get; set; } // EXPLICATIE (MAX)

    [Size(1024)]
    public virtual string ProductDescription { get; set; } // NPRODUS

    // Linked documents
    public virtual int? LinkedDocumentId { get; set; } // ID_DOCUMENT_CONEX

    [Size(100)]
    public virtual string LinkedDocumentNumber { get; set; } // NR_DOC_CONEX

    public virtual DateTime? LinkedDocumentDate { get; set; } // DATA_DOC_CONEX

    public virtual int? TransactionId { get; set; } // ID_TRANZACTIE

    // NIR / Receipt
    [Size(20)]
    public virtual string NIRNumber { get; set; } // NR_LIST (număr intrare recepție)

    [Size(20)]
    public virtual string PackageNumber { get; set; } // PACHET

    [Size(20)]
    public virtual string LineNumber { get; set; } // LINE

    [Size(10)]
    public virtual string DeliveryConditions { get; set; } // CONDLVR

    // Chitanță
    public virtual int? ReceiptNumber { get; set; } // NR_CHITANTA
    public virtual DateTime? ReceiptDate { get; set; } // DATA_CHITANTA
    public virtual bool GenerateReceipt { get; set; } // SE_GEN_CHITANTA

    // Bon carburant
    [Size(100)]
    public virtual string FuelVoucherSeries { get; set; } // SERIE_BON_CARBURANT

    [Size(100)]
    public virtual string FuelVoucherNumberFrom { get; set; } // NR_BON_CARBURANT_DE_LA

    [Size(100)]
    public virtual string FuelVoucherNumberTo { get; set; } // NR_BON_CARBURANT_PANA_LA

    // Gestionar
    [Size(200)]
    public virtual string WarehouseManager1 { get; set; } // GESTIONAR1

    [Size(200)]
    public virtual string WarehouseManager2 { get; set; } // GESTIONAR2

    // Comentarii custom
    [Size(200)]
    public virtual string Comment1 { get; set; } // COM1

    [Size(200)]
    public virtual string Comment2 { get; set; } // COM2

    [Size(200)]
    public virtual string Comment3 { get; set; } // COM3

    [Size(200)]
    public virtual string Comment4 { get; set; } // COM4

    [Size(200)]
    public virtual string Comment5 { get; set; } // COM5

    // Stare și validare
    public virtual int? Status { get; set; } // STARE (0=draft, 1=validated, -1=deleted)
    public virtual int? ValidationStatus { get; set; } // VALIDAT

    public virtual bool InTransit { get; set; } // PE_DRUM
    public virtual bool Transmitted { get; set; } // TRANSMIS
    public virtual bool IssuedHQ { get; set; } // EMIS_HQ
    public virtual bool AutoGenerated { get; set; } // AUTOGENERAT

    // Notă contabilă
    public virtual bool AutomaticAccountingEntry { get; set; } // NOTA_AUTOMAT

    [Size(100)]
    public virtual string AccountingEntryNumber { get; set; } // NR_NOTA

    public virtual DateTime? AccountingEntryDate { get; set; } // DATA_NOTA

    // Contabilitate
    [Size(100)]
    public virtual string AccountNumber { get; set; } // CONT_CONTABIL

    // Stergere / Istoric
    public virtual int? InitialDocumentId { get; set; } // ID_INITIAL (pentru versiuni document)
    public virtual int? ModificationId { get; set; } // ID_MODIFICARE

    public virtual DateTime? DeletionDate { get; set; } // DATA_STERGERE
    public virtual int? DeletedByUserId { get; set; } // ID_UTILIZATOR_STERGERE

    // Audit
    public virtual int? CreatedByUserId { get; set; } // ID_UTILIZATORI
    public virtual DateTime? OperationDate { get; set; } // DATA_OPERARE

    // Tethys integration
    [Size(36)]
    public virtual string TethysId { get; set; } // TETHYSID (GUID)

    // Document fizic scanat
    public virtual byte[] PhysicalDocument { get; set; } // DOCUMENT_FIZIC (varbinary MAX)

    // TVA la încasare
    public virtual bool VATOnReceipt { get; set; } // TVA_LA_INCASARE

    // Legacy
    [Size(100)]
    public virtual string ManualDescription { get; set; } // MAN_DEN

    // Navigation
    [Association("InventoryDocument-Items")]
    public virtual IList<InventoryDocumentItem> Items { get; set; }
}
```

#### GEST_GNMCL → Product
```csharp
public class Product : BaseObject
{
    [Key]
    public virtual int Id { get; set; } // ID_GEST_GNMCL

    // Identificare
    [Size(100), Required, Index(IsUnique = true)]
    public virtual string ProductCode { get; set; } // COD

    [Size(500), Required]
    public virtual string Name { get; set; } // DENUMIRE

    [Size(2000)]
    public virtual string Description { get; set; } // DESCRIERE

    [Size(50)]
    public virtual string ShortName { get; set; } // DENUMIRE_SCURTA

    // Clasificare
    public virtual int? ProductTypeId { get; set; } // ID_GEST_TIP_PRODUSE
    public virtual int? MaterialTypeId { get; set; } // ID_GEST_TIP_MATERIAL
    public virtual int? CategoryId { get; set; } // ID_GEST_CATEGORII

    // Coduri externe
    [Size(100)]
    public virtual string BarCode { get; set; } // COD_BARE

    [Size(100)]
    public virtual string CustomsCode { get; set; } // COD_VAMAL

    // UM
    [Size(20), Required]
    public virtual string UnitOfMeasure { get; set; } // UM

    // Prețuri
    public virtual decimal StandardPrice { get; set; } // PRET_STANDARD
    public virtual decimal SalePrice { get; set; } // PRET_VANZARE
    public virtual decimal MinimumPrice { get; set; } // PRET_MINIM
    public virtual decimal AveragePrice { get; set; } // PRET_MEDIU

    // Stocuri
    public virtual decimal CurrentStock { get; set; } // STOC_CURENT
    public virtual decimal MinimumStock { get; set; } // STOC_MINIM
    public virtual decimal MaximumStock { get; set; } // STOC_MAXIM

    // TVA
    public virtual decimal VATRate { get; set; } // COTA_TVA

    // Stare
    public virtual bool IsActive { get; set; } // ACTIV
    public virtual bool IsDeleted { get; set; } // STERS

    // Navigation
    [Association("Product-DocumentItems")]
    public virtual IList<InventoryDocumentItem> DocumentItems { get; set; }

    [Association("Product-BarCodes")]
    public virtual IList<ProductBarCode> BarCodes { get; set; }
}
```

---

## PROCEDURI STOCATE CRITICE

### sp_get_fisa_cont_new
**Logică**: Generează fișa de cont cu sold inițial, mișcări perioadă, sold final
**Parametri**: cont, data_start, data_end, criterii filtrare (functional, economic, repartitor, etc.)
**Migrare**: Repository pattern + LINQ to Entities în AccountStatementService

### spPlanUpdateDetalii
**Logică**: Update proprietăți cont (sumator, fctcont, tip, balanta) de la părinte
**Migrare**: Business logic în ChartOfAccounts.OnSaving() sau Controller

### sp_gest_get_lista_doc
**Logică**: Lista documente gestiune cu filtre complexe
**Migrare**: ObjectSpace.GetObjectsQuery<InventoryDocument>() cu criterii

---

## CHECKLIST MAPARE ENTITĂȚI

- [x] Tabele contabilitate (CPLAN, CNOTE, CJURNALE, REPARTITORI) → 4 entități
- [x] Tabele buget (BG_VERSIUNE, BG_DESCHIDERE, ALOP_*) → 6 entități
- [x] Tabele gestiune (GEST_DOCUM, GEST_GNMCL, GEST_ITEMSI) → 3 entități
- [ ] Tabele nomenclatoare (categorii, unități, tipuri)
- [ ] Tabele organizare internă (OI_UNITATI, OI_PROIECTE)
- [ ] Tabele utilizatori și securitate
- [ ] Tabele rapoarte (frRapoarte, REPORTS_*)
- [ ] Tabele audit (LOG_*, Audit)

---

**Următorul pas**: Implementare efectivă entități în Visual Studio cu EF Core
