# Modulele Business și Dependențe

## STRUCTURĂ MODULARĂ RECOMANDATĂ (XAF)

```
Contabilitate.XAF/
├── Contabilitate.XAF.Module/              # Modul principal (platform-independent)
│   ├── BusinessObjects/
│   │   ├── Accounting/                     # Modul Contabilitate
│   │   │   ├── ChartOfAccounts.cs
│   │   │   ├── AccountingEntry.cs
│   │   │   ├── Journal.cs
│   │   │   └── CostCenter.cs
│   │   ├── Budget/                         # Modul Buget
│   │   │   ├── BudgetVersion.cs
│   │   │   ├── BudgetAllocation.cs
│   │   │   ├── Commitment.cs
│   │   │   ├── Liquidation.cs
│   │   │   └── PaymentOrder.cs
│   │   ├── Inventory/                      # Modul Gestiune
│   │   │   ├── InventoryDocument.cs
│   │   │   ├── Product.cs
│   │   │   └── InventoryDocumentItem.cs
│   │   ├── Treasury/                       # Modul Casa/Banca
│   │   │   ├── BankAccount.cs
│   │   │   ├── CashDocument.cs
│   │   │   └── BankStatement.cs
│   │   ├── Organization/                   # Modul Organizare
│   │   │   ├── OrganizationalUnit.cs
│   │   │   ├── Project.cs
│   │   │   └── ProjectGroup.cs
│   │   └── Common/                         # Entități comune
│   │       ├── BudgetPlan.cs               # Plan funcțional/economic
│   │       └── DocumentType.cs
│   ├── Controllers/
│   │   ├── Accounting/
│   │   │   ├── AccountingEntryController.cs
│   │   │   ├── PostAccountingEntryController.cs
│   │   │   └── AccountStatementController.cs
│   │   ├── Budget/
│   │   │   ├── BudgetAllocationController.cs
│   │   │   ├── CommitmentController.cs
│   │   │   └── ALOPWorkflowController.cs
│   │   └── Inventory/
│   │       └── InventoryDocumentController.cs
│   ├── Services/
│   │   ├── AccountStatementService.cs
│   │   ├── BudgetAvailabilityCalculator.cs
│   │   └── InventoryValuationService.cs
│   └── Module.cs
│
├── Contabilitate.XAF.Module.Blazor/       # Customizări Blazor
│   ├── Controllers/
│   │   └── BlazorSpecificControllers/
│   └── Module.cs
│
├── Contabilitate.XAF.Module.Win/          # Customizări WinForms (optional)
│   ├── Controllers/
│   │   └── WinFormsSpecificControllers/
│   └── Module.cs
│
├── Contabilitate.XAF.Blazor.Server/       # Aplicație Blazor
│   ├── Startup.cs
│   └── appsettings.json
│
└── Contabilitate.XAF.Win/                 # Aplicație WinForms (optional)
    ├── Program.cs
    └── App.config
```

---

## DEPENDENȚE ÎNTRE MODULE

### 1. MODUL CONTABILITATE (CORE)

**Fișiere Delphi:**
- conta/PlanConturiUnit.pas
- conta/NoteUnitNew.pas
- conta/BalantaUnit.pas
- conta/FisaContUnit.pas
- conta/JurnaleUnit.pas

**Dependențe:**
- **NU DEPINDE** de alte module (este modul de bază)
- Oferă entități: ChartOfAccounts, AccountingEntry, Journal, CostCenter

**Module care DEPIND de Contabilitate:**
- Buget (generează note contabile pentru angajamente/lichidări)
- Gestiune (generează note contabile pentru intrări/ieșiri)
- Casa/Banca (generează note pentru încasări/plăți)

**XAF Implementation:**
```csharp
// Contabilitate.XAF.Module/Modules/AccountingModule.cs
public class AccountingModule : ModuleBase
{
    public AccountingModule()
    {
        // Înregistrare entități
        AdditionalExportedTypes.Add(typeof(ChartOfAccounts));
        AdditionalExportedTypes.Add(typeof(AccountingEntry));
        AdditionalExportedTypes.Add(typeof(Journal));
        AdditionalExportedTypes.Add(typeof(CostCenter));
    }

    public override void Setup(XafApplication application)
    {
        base.Setup(application);

        // Setup servicii
        application.ServiceProvider.GetService<IAccountingService>();
    }
}
```

---

### 2. MODUL BUGET

**Fișiere Delphi:**
- Buget/BugetContainer.pas
- Buget/BgPlanUnit.pas
- Buget/AlopAngajamente.pas
- Buget/AlopLichidare.pas
- Buget/AlopOrdonantare.pas

**Dependențe:**
- **DEPINDE DE:** Contabilitate (generează AccountingEntry)
- **DEPINDE DE:** Organizare Internă (OrganizationalUnit, Project)
- **DEPINDE DE:** Nomenclatoare (BudgetPlan funcțional/economic)

**Flux de date:**
```
BudgetAllocation
    ↓ (verificare disponibil)
Commitment (Angajament)
    ↓ (creare AccountingEntry)
Liquidation (Lichidare)
    ↓ (creare AccountingEntry)
PaymentOrder (Ordonanțare)
    ↓ (creare AccountingEntry + plată în Casa/Banca)
```

**XAF Implementation:**
```csharp
public class BudgetModule : ModuleBase
{
    public BudgetModule()
    {
        RequiredModuleTypes.Add(typeof(AccountingModule)); // Dependență explicită

        AdditionalExportedTypes.Add(typeof(BudgetVersion));
        AdditionalExportedTypes.Add(typeof(BudgetAllocation));
        AdditionalExportedTypes.Add(typeof(Commitment));
        // ...
    }

    public override void Setup(XafApplication application)
    {
        base.Setup(application);

        // Abonare la evenimente din Contabilitate
        application.ObjectSpaceCreated += Application_ObjectSpaceCreated;
    }

    private void Application_ObjectSpaceCreated(object sender, ObjectSpaceCreatedEventArgs e)
    {
        // Setup tracked for budget availability
        e.ObjectSpace.Committing += ObjectSpace_Committing;
    }

    private void ObjectSpace_Committing(object sender, CancelEventArgs e)
    {
        var objectSpace = (IObjectSpace)sender;

        // Validare disponibil bugetar înainte de commit
        foreach (var commitment in objectSpace.ModifiedObjects.OfType<Commitment>())
        {
            var calculator = new BudgetAvailabilityCalculator(objectSpace);
            if (!calculator.HasSufficientBudget(commitment))
            {
                throw new UserFriendlyException("Buget insuficient pentru angajament!");
            }
        }
    }
}
```

**Service pentru generare note contabile:**
```csharp
public class BudgetAccountingService
{
    private readonly IObjectSpace objectSpace;

    public void GenerateCommitmentEntry(Commitment commitment)
    {
        // Generare notă contabilă pentru angajament
        var entry = objectSpace.CreateObject<AccountingEntry>();

        entry.EntryNumber = GetNextEntryNumber("ANG");
        entry.EntryDate = commitment.CommitmentDate;
        entry.Journal = "ANGAJAMENTE";

        // Debit: 8066 (Angajamente bugetare)
        entry.DebitAccount = "8066";
        entry.DebitFunctionalCode = commitment.FunctionalCode;
        entry.DebitEconomicCode = commitment.EconomicCode;

        // Credit: 8068 (Disponibil bugetar)
        entry.CreditAccount = "8068";
        entry.CreditFunctionalCode = commitment.FunctionalCode;
        entry.CreditEconomicCode = commitment.EconomicCode;

        entry.Amount = commitment.Amount;
        entry.Description = $"Angajament {commitment.CommitmentNumber} - {commitment.Purpose}";

        entry.Status = 1; // Posted

        objectSpace.CommitChanges();
    }

    public void GenerateLiquidationEntry(Liquidation liquidation)
    {
        var entry = objectSpace.CreateObject<AccountingEntry>();

        entry.EntryNumber = GetNextEntryNumber("LICH");
        entry.EntryDate = liquidation.LiquidationDate;
        entry.Journal = "LICHIDARI";

        // Debit: 401 (Furnizori)
        entry.DebitAccount = "401";
        entry.DebitCostCenter = liquidation.Commitment.Beneficiary;

        // Credit: 8066 (Angajamente bugetare)
        entry.CreditAccount = "8066";
        entry.CreditFunctionalCode = liquidation.Commitment.FunctionalCode;
        entry.CreditEconomicCode = liquidation.Commitment.EconomicCode;

        entry.Amount = liquidation.Amount;
        entry.Description = $"Lichidare {liquidation.LiquidationNumber}";

        entry.Status = 1;

        objectSpace.CommitChanges();
    }
}
```

---

### 3. MODUL GESTIUNE

**Fișiere Delphi:**
- Gestiune/StockUnit.pas
- Gestiune/DocumenteUnit.pas
- Gestiune/TCVUnit.pas

**Dependențe:**
- **DEPINDE DE:** Contabilitate (generează AccountingEntry)
- **DEPINDE DE:** Nomenclatoare (Product, ProductType, UnitOfMeasure)

**Flux de date:**
```
InventoryDocument (NIR, factura, etc.)
    ↓ (are Items)
InventoryDocumentItem
    ↓ (referă Product)
Product
    ↓ (are ProductType, UnitOfMeasure)

La validare document:
    ↓ (generează note contabile)
AccountingEntry
```

**Service pentru generare note contabile:**
```csharp
public class InventoryAccountingService
{
    public void GenerateReceiptEntry(InventoryDocument document)
    {
        // Exemplu: NIR (Notă Intrare Recepție)
        foreach (var item in document.Items)
        {
            var entry = objectSpace.CreateObject<AccountingEntry>();

            entry.EntryNumber = GetNextEntryNumber("NIR");
            entry.EntryDate = document.DocumentDate;
            entry.Journal = "GESTIUNE";

            // Debit: 301 (Materiale)
            entry.DebitAccount = "301";
            entry.DebitCostCenter = document.Receiver;

            // Credit: 401 (Furnizori)
            entry.CreditAccount = "401";
            entry.CreditCostCenter = document.Supplier;

            entry.Amount = item.Amount;
            entry.Description = $"NIR {document.DocumentNumber} - {item.Product.Name}";

            // Link către document gestiune
            entry.SourceModule = 1; // Gestiune
            entry.SourceDocumentId = document.Id;
            entry.MaterialId = item.Product.Id;

            entry.Status = 1;

            objectSpace.CommitChanges();
        }

        // Notă pentru TVA
        if (document.TotalVAT > 0)
        {
            var vatEntry = objectSpace.CreateObject<AccountingEntry>();

            vatEntry.EntryNumber = GetNextEntryNumber("NIR-TVA");
            vatEntry.EntryDate = document.DocumentDate;
            vatEntry.Journal = "GESTIUNE";

            vatEntry.DebitAccount = "4426"; // TVA deductibilă
            vatEntry.CreditAccount = "401";

            vatEntry.Amount = document.TotalVAT.Value;

            vatEntry.Status = 1;

            objectSpace.CommitChanges();
        }
    }
}
```

---

### 4. MODUL CASA/BANCA

**Fișiere Delphi:**
- CasaBanca/CasaUnit.pas
- CasaBanca/RegistruUnit.pas
- CasaBanca/ImportCasaUnit.pas

**Dependențe:**
- **DEPINDE DE:** Contabilitate (generează AccountingEntry)
- **DEPINDE DE:** Buget (referă PaymentOrder)

**Flux de date:**
```
BankAccount (Cont bancar)
    ↓
CashDocument (Dispoziție plată, Încasare)
    ↓ (generează)
AccountingEntry (5121 = Casa, 5121 = Banca)
```

**Service:**
```csharp
public class TreasuryAccountingService
{
    public void GeneratePaymentEntry(CashDocument payment, PaymentOrder order)
    {
        var entry = objectSpace.CreateObject<AccountingEntry>();

        entry.EntryNumber = GetNextEntryNumber("PLATA");
        entry.EntryDate = payment.PaymentDate;
        entry.Journal = "CASA_BANCA";

        // Debit: 401 (Furnizor)
        entry.DebitAccount = "401";
        entry.DebitCostCenter = order.Liquidation.Commitment.Beneficiary;

        // Credit: 5121 (Bancă) sau 5311 (Casă)
        entry.CreditAccount = payment.BankAccount != null ? "5121" : "5311";

        entry.Amount = payment.Amount;
        entry.Description = $"Plată {payment.PaymentNumber} - OP {order.OrderNumber}";

        // Link către ordin plată
        entry.PaymentOrderId = order.Id;

        entry.Status = 1;

        objectSpace.CommitChanges();
    }
}
```

---

## GRAFIC DE DEPENDENȚE

```
┌─────────────────────┐
│   NOMENCLATOARE     │ (independent - tabele de bază)
│  - BudgetPlan       │
│  - ProductType      │
│  - DocumentType     │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│   ORGANIZARE        │ (independent)
│  - OrgUnit          │
│  - Project          │
└──────────┬──────────┘
           │
           ▼
┌─────────────────────┐
│   CONTABILITATE     │◄──────────────┐ (CORE - toate depind de el)
│  - ChartOfAccounts  │               │
│  - AccountingEntry  │               │
│  - Journal          │               │
│  - CostCenter       │               │
└──────────┬──────────┘               │
           │                          │
           │         ┌────────────────┘
           │         │
           ▼         ▼
     ┌─────────────────────┐    ┌─────────────────────┐
     │      BUGET          │    │     GESTIUNE        │
     │  - BudgetVersion    │    │  - InvDocument      │
     │  - Commitment       │    │  - Product          │
     │  - Liquidation      │    │  - InvDocItem       │
     │  - PaymentOrder     │    └──────────┬──────────┘
     └──────────┬──────────┘               │
                │                          │
                └──────────┬───────────────┘
                           │
                           ▼
                   ┌─────────────────────┐
                   │    CASA/BANCA       │
                   │  - BankAccount      │
                   │  - CashDocument     │
                   │  - BankStatement    │
                   └─────────────────────┘
```

---

## MODULE XAF RECOMANDATE

### Module Built-in de Activat

```csharp
// Startup.cs / Program.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddXaf(Configuration, builder =>
    {
        builder
            // Business Objects
            .AddBusinessObjectsModule()

            // Security
            .AddSecurityModule()
            .AddAuditTrailModule() // Audit pentru modificări

            // Reports
            .AddReportsV2Module() // DevExpress Reports

            // Validation
            .AddValidationModule() // Validări business

            // Conditional Appearance
            .AddConditionalAppearanceModule() // Stilizare condiționată

            // Dashboards (optional)
            .AddDashboardsModule()

            // Clone Object (optional)
            .AddCloneObjectModule();
    });
}
```

### Module Custom

```csharp
// Contabilitate.XAF.Module/Module.cs
public sealed class ContabilitateModule : ModuleBase
{
    public ContabilitateModule()
    {
        // Activare sub-module custom
        RequiredModuleTypes.Add(typeof(AccountingModule));
        RequiredModuleTypes.Add(typeof(BudgetModule));
        RequiredModuleTypes.Add(typeof(InventoryModule));
        RequiredModuleTypes.Add(typeof(TreasuryModule));
    }

    public override void Setup(XafApplication application)
    {
        base.Setup(application);

        // Înregistrare servicii
        application.ServiceProvider.AddScoped<IAccountStatementService, AccountStatementService>();
        application.ServiceProvider.AddScoped<IBudgetAvailabilityCalculator, BudgetAvailabilityCalculator>();
    }
}
```

---

## CHECKLIST DEPENDENȚE

### Faza 1: Module Independente
- [ ] Nomenclatoare (ProductType, DocumentType, etc.)
- [ ] Organizare (OrganizationalUnit, Project, ProjectGroup)

### Faza 2: Modul Core
- [ ] **Contabilitate** (ChartOfAccounts, AccountingEntry, Journal, CostCenter)
- [ ] Servicii: AccountStatementService

### Faza 3: Module Dependente
- [ ] **Buget** (după Contabilitate + Organizare)
  - [ ] BudgetVersion, BudgetAllocation
  - [ ] Commitment, Liquidation, PaymentOrder
  - [ ] BudgetAccountingService (generare note)

- [ ] **Gestiune** (după Contabilitate + Nomenclatoare)
  - [ ] InventoryDocument, Product, InventoryDocumentItem
  - [ ] InventoryAccountingService (generare note)

### Faza 4: Module Finale
- [ ] **Casa/Banca** (după Contabilitate + Buget)
  - [ ] BankAccount, CashDocument, BankStatement
  - [ ] TreasuryAccountingService

---

## BEST PRACTICES

### 1. Dependency Injection
```csharp
// Servicii înregistrate în DI container
services.AddScoped<IAccountingService, AccountingService>();
services.AddScoped<IBudgetService, BudgetService>();

// Utilizare în Controller
public class CommitmentController : ObjectViewController<DetailView, Commitment>
{
    private readonly IBudgetService budgetService;

    public CommitmentController(IBudgetService budgetService)
    {
        this.budgetService = budgetService;
    }
}
```

### 2. Event-Driven Architecture
```csharp
// Publisher (Buget Module)
public class Commitment : BaseObject
{
    protected override void OnSaved()
    {
        base.OnSaved();

        // Raise event
        CommitmentCreated?.Invoke(this, new CommitmentCreatedEventArgs(this));
    }

    public event EventHandler<CommitmentCreatedEventArgs> CommitmentCreated;
}

// Subscriber (Accounting Module)
application.ObjectSpaceCreated += (s, e) =>
{
    e.ObjectSpace.ObjectSaved += (sender, args) =>
    {
        if (args.Object is Commitment commitment)
        {
            var accountingService = new BudgetAccountingService(e.ObjectSpace);
            accountingService.GenerateCommitmentEntry(commitment);
        }
    };
};
```

### 3. Shared Interfaces
```csharp
// Interface pentru toate entitățile care generează note contabile
public interface IAccountingDocumentSource
{
    DateTime DocumentDate { get; }
    string DocumentNumber { get; }
    decimal TotalAmount { get; }
    List<AccountingEntry> GenerateAccountingEntries();
}

// Implementare în Commitment
public class Commitment : BaseObject, IAccountingDocumentSource
{
    public List<AccountingEntry> GenerateAccountingEntries()
    {
        // Logică generare note
    }
}
```

---

Acest fișier documentează interdependențele dintre module și oferă guideline pentru implementarea modulară în XAF.
