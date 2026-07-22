
  ---
  📋 PLAN DETALIAT DE MIGRARE

  Aplicație Contabilitate: Delphi XE2 VCL → .NET Core XAF + EF Core

  ---
  🎯 REZUMAT EXECUTIV

  Aplicația Curentă (Delphi XE2)

  - Cod: 232 fișiere .pas (~105,153 linii), 185 fișiere .dfm
  - Bază de date: SQL Server cu 312 tabele
  - UI Framework: DevExpress VCL (cxGrid, cxTreeList, cxEditors)
  - ORM: ZEOS Database Components
  - Raportare: FastReport
  - Versiune: 3.1.1.497

  Aplicația Țintă (.NET Core)

  - .NET Version: .NET 8.0 (LTS)
  - Framework: DevExpress XAF 24.2+
  - ORM: Entity Framework Core 8.0
  - UI: Blazor Server + WinForms (optional)
  - Raportare: DevExpress Reports

  ---
  📊 MAPAREA ENTITĂȚILOR ȘI MODULELOR

  1. MODULE PRINCIPALE → XAF MODULES

  | Modul Delphi           | Fișiere  | XAF Module Echivalent     | Prioritate    |
  |------------------------|----------|---------------------------|---------------|
  | conta/ (Contabilitate) | 18       | ContabilityModule         | ⭐⭐⭐ CRITICĂ   |
  | Buget/                 | 27       | BudgetModule              | ⭐⭐⭐ CRITICĂ   |
  | CasaBanca/             | 20+      | TreasuryModule            | ⭐⭐⭐ CRITICĂ   |
  | Gestiune/              | 15+      | InventoryModule           | ⭐⭐ IMPORTANTĂ |
  | Nomenclatoare/         | 7        | NomenclaturesModule       | ⭐⭐ IMPORTANTĂ |
  | Repartitori/           | 5        | CostCentersModule         | ⭐⭐ IMPORTANTĂ |
  | Raportare/             | Multiple | Reports Module (XAF)      | ⭐⭐ IMPORTANTĂ |
  | Anexe/                 | 8        | FinancialStatementsModule | ⭐ MEDIE       |
  | Contracte/             | 4        | ContractsModule           | ⭐ MEDIE       |

  2. ENTITĂȚI PRINCIPALE → BUSINESS OBJECTS (EF Core)

  🔹 Contabilitate (13 entități principale)

~~~
  // CPLAN - Plan de Conturi
  public class ChartOfAccounts : BaseObject
  {
      public virtual string AccountCode { get; set; } // CONT
      public virtual string Description { get; set; } // ROMANA
      public virtual string ParentAccount { get; set; } // PARINTE
      public virtual decimal InitialDebitBalance { get; set; } // SID
      public virtual decimal InitialCreditBalance { get; set; } // SIC
      public virtual bool IsSummary { get; set; } // SUMATOR
      public virtual bool IsAnalytic { get; set; } // IS_ANALITIC
      public virtual AccountType AccountType { get; set; } // FCTCONT (D/C/B)
      public virtual int FiscalYear { get; set; } // AN_FISCAL

      // Navigation
      public virtual IList<AccountingEntry> DebitEntries { get; set; }
      public virtual IList<AccountingEntry> CreditEntries { get; set; }
  }
~~~

~~~
  // CNOTE - Note Contabile
  public class AccountingEntry : BaseObject
  {
      public virtual string EntryNumber { get; set; } // NRDOC
      public virtual DateTime EntryDate { get; set; } // DATA
      public virtual string Description { get; set; } // EXPLICATIE
      public virtual decimal Amount { get; set; } // VALOARE

      public virtual string DebitAccount { get; set; } // CONTD
      public virtual string CreditAccount { get; set; } // CONTC
      public virtual string Journal { get; set; } // JURNAL

      public virtual CostCenter DebitCostCenter { get; set; } // REPARTITOR_DEBIT
      public virtual CostCenter CreditCostCenter { get; set; } // REPARTITOR_CREDIT

      public virtual string FunctionalCode { get; set; } // COD_FUNCTIONAL
      public virtual string EconomicCode { get; set; } // COD_ECONOMIC

      public virtual int Status { get; set; } // STARE (0=draft, 1=posted)
      public virtual int ModuleSource { get; set; } // MODUL

      // Audit
      public virtual ApplicationUser CreatedBy { get; set; } // C_O
      public virtual DateTime OperationDate { get; set; } // DATA_OPERARE
  }
~~~

~~~
  // CJURNALE - Jurnale
  public class Journal : BaseObject
  {
      public virtual string JournalCode { get; set; } // JURNAL
      public virtual string Description { get; set; } // DENUMIRE
      public virtual bool IsClosingJournal { get; set; } // INCHIDERE
  }
~~~

~~~
  // REPARTITORI - Centre de Cost / Parteneri
  public class CostCenter : BaseObject
  {
      public virtual string Code { get; set; } // CODSECTIE
      public virtual string Name { get; set; } // NUME
      public virtual string Address { get; set; } // ADRESA
      public virtual string FiscalCode { get; set; } // COD_FISCAL
      public virtual string BankAccount { get; set; } // CONT
      public virtual bool IsInternal { get; set; } // GESTINT

      // Navigation
      public virtual IList<AccountingEntry> DebitEntries { get; set; }
      public virtual IList<AccountingEntry> CreditEntries { get; set; }
  }
~~~

  🔹 Buget (8 entități principale)

~~~
  // BG_VERSIUNE - Versiuni Buget
  public class BudgetVersion : BaseObject
  {
      public virtual int FiscalYear { get; set; } // AN_FISCAL
      public virtual int Version { get; set; } // VERSIUNE
      public virtual int Revision { get; set; } // REVIZIE
      public virtual BudgetType BudgetType { get; set; } // TIP_BUGET

      public virtual DateTime CreatedDate { get; set; } // DATA_CREARE
      public virtual DateTime? ApprovedDate { get; set; } // DATA_APROBARE
      public virtual ApplicationUser CreatedBy { get; set; } // ID_UTILIZATORI_CREAT
      public virtual ApplicationUser ApprovedBy { get; set; } // ID_UTILIZATORI_APROBAT

      public virtual bool IsBlocked { get; set; } // isBlocked
      public virtual string Explanation { get; set; } // EXPLICATIE

      // Navigation
      public virtual IList<BudgetAllocation> Allocations { get; set; }
  }
~~~

~~~
  // BG_DESCHIDERE - Deschideri Bugetare
  public class BudgetAllocation : BaseObject
  {
      public virtual BudgetVersion BudgetVersion { get; set; }
      public virtual string FunctionalCode { get; set; } // COD_FUNCTIONAL
      public virtual string EconomicCode { get; set; } // COD_ECONOMIC

      public virtual decimal AllocatedAmount { get; set; } // SUMA_BUGET
      public virtual decimal CommittedAmount { get; set; } // SUMA_ANGAJATA
      public virtual decimal LiquidatedAmount { get; set; } // SUMA_LICHIDATĂ
      public virtual decimal PaidAmount { get; set; } // SUMA_PLĂTITĂ
  }
~~~

~~~
  // ALOP_ANGAJAMENTE - Angajamente ALOP
  public class Commitment : BaseObject
  {
      public virtual string CommitmentNumber { get; set; } // NR_ANGAJAMENT
      public virtual DateTime CommitmentDate { get; set; } // DATA_ANGAJAMENT
      public virtual decimal Amount { get; set; } // SUMA

      public virtual string FunctionalCode { get; set; }
      public virtual string EconomicCode { get; set; }
      public virtual CostCenter Beneficiary { get; set; }

      // Navigation
      public virtual IList<Liquidation> Liquidations { get; set; }
  }
~~~

~~~
  // ALOP_LICHIDARE - Lichidări ALOP
  public class Liquidation : BaseObject
  {
      public virtual Commitment Commitment { get; set; }
      public virtual string LiquidationNumber { get; set; }
      public virtual DateTime LiquidationDate { get; set; }
      public virtual decimal Amount { get; set; }

      // Navigation
      public virtual IList<PaymentOrder> PaymentOrders { get; set; }
  }
~~~

~~~
  // ALOP_ORDONANTARE - Ordonanțări
  public class PaymentOrder : BaseObject
  {
      public virtual Liquidation Liquidation { get; set; }
      public virtual string OrderNumber { get; set; }
      public virtual DateTime OrderDate { get; set; }
      public virtual decimal Amount { get; set; }
  }
~~~

  🔹 Gestiune (6 entități principale)

~~~
  // GEST_DOCUM - Documente Gestiune
  public class InventoryDocument : BaseObject
  {
      public virtual string DocumentNumber { get; set; } // NR_DOCUM
      public virtual DateTime DocumentDate { get; set; } // DATA_DOCUM
      public virtual InventoryDocumentType DocumentType { get; set; } // ID_GEST_TIP_DOCUM

      public virtual CostCenter Supplier { get; set; } // ID_PREDATOR
      public virtual CostCenter Receiver { get; set; } // ID_PRIMITOR

      public virtual decimal TotalAmount { get; set; } // TOTALDOC
      public virtual decimal VAT { get; set; } // TOTALTVA
      public virtual int Status { get; set; } // STARE

      // Navigation
      public virtual IList<InventoryDocumentItem> Items { get; set; }
  }
~~~

~~~
  // GEST_ITEMSI - Poziții Document
  public class InventoryDocumentItem : BaseObject
  {
      public virtual InventoryDocument Document { get; set; }
      public virtual Product Product { get; set; } // ID_GEST_GNMCL

      public virtual decimal Quantity { get; set; } // CANTITATE
      public virtual decimal UnitPrice { get; set; } // PRET_UNITAR
      public virtual decimal Amount { get; set; } // VALOARE
      public virtual decimal VAT { get; set; } // TVA
  }
~~~

~~~
  // GEST_GNMCL - Nomenclator Produse
  public class Product : BaseObject
  {
      public virtual string ProductCode { get; set; } // COD
      public virtual string Name { get; set; } // DENUMIRE
      public virtual string Description { get; set; } // DESCRIERE
      public virtual string UnitOfMeasure { get; set; } // UM

      public virtual ProductType ProductType { get; set; } // ID_GEST_TIP_PRODUSE
      public virtual decimal StandardPrice { get; set; } // PRET_STANDARD
      public virtual bool IsActive { get; set; }
  }
~~~

  3. COMPONENTE UI → XAF VIEWS & EDITORS

  | Componentă VCL Delphi      | Instanțe | XAF Echivalent                       | Observații                         |
  |----------------------------|----------|--------------------------------------|------------------------------------|
  | cxGrid                     | 298      | GridListEditor (XAF)                 | Grid principal pentru List Views   |
  | cxGridDBTableView          | 130      | -                                    | Generat automat de XAF             |
  | cxGridDBBandedTableView    | 261      | BandedGridView                       | Coloane grupate în benzi           |
  | cxDBTreeList               | 83       | TreeListEditor                       | Structuri ierarhice (plan conturi) |
  | cxDBTextEdit               | 73       | StringPropertyEditor                 | Edit text standard                 |
  | cxDBDateEdit               | 34       | DateTimePropertyEditor               | Selector dată                      |
  | cxDBCheckBox               | 41       | BooleanPropertyEditor                | Checkbox                           |
  | cxDBCurrencyEdit           | 38       | DecimalPropertyEditor                | Valori monetare                    |
  | cxButton                   | 510      | SimpleAction / PopupWindowShowAction | Butoane → Actions                  |
  | cxPageControl / cxTabSheet | 35/82    | TabbedGroup (Layout)                 | Tab-uri în Detail View             |

  ---
  🚀 FAZE DE IMPLEMENTARE

  FAZA 1: PREGĂTIRE ȘI SETUP (2-3 săptămâni)

  1.1 Setup Proiect XAF (.NET 8)

  # Creare soluție XAF
  dotnet new xaf --name Contabilitate.XAF --framework net8.0

  # Structură rezultată:
  # Contabilitate.XAF.Module/          (Business Objects, Logica Business)
  # Contabilitate.XAF.Blazor.Server/   (Aplicație Blazor)
  # Contabilitate.XAF.Win/             (Aplicație WinForms - optional)

  1.2 Configurare Entity Framework Core

  // appsettings.json
  {
    "ConnectionStrings": {
      "ConnectionString": "Server=10.20.0.218;Database=Contabilitate_XAF_2025;User Id=admin;Password=***;TrustServerCertificate=True;"
    }
  }

~~~
  // DbContext
  public class ContabilitateDbContext : DbContext
  {
      public ContabilitateDbContext(DbContextOptions<ContabilitateDbContext> options)
          : base(options) { }

      // DbSets pentru fiecare entitate
      public DbSet<ChartOfAccounts> ChartOfAccounts { get; set; }
      public DbSet<AccountingEntry> AccountingEntries { get; set; }
      public DbSet<Journal> Journals { get; set; }
      public DbSet<CostCenter> CostCenters { get; set; }
      // ... etc

      protected override void OnModelCreating(ModelBuilder modelBuilder)
      {
          base.OnModelCreating(modelBuilder);

          // Mapare la tabele existente
          modelBuilder.Entity<ChartOfAccounts>().ToTable("CPLAN");
          modelBuilder.Entity<AccountingEntry>().ToTable("CNOTE");
          modelBuilder.Entity<Journal>().ToTable("CJURNALE");
          modelBuilder.Entity<CostCenter>().ToTable("REPARTITORI");

          // Configurare relații și indecși
          // ...
      }
  }
~~~

  1.3 Reverse Engineering Baza de Date

  # Generare entități din baza existentă (Database First)
  dotnet ef dbcontext scaffold "Server=10.20.0.218;Database=contabilitate_Sotanga_2025;User Id=admin;Password=***;" \
      Microsoft.EntityFrameworkCore.SqlServer \
      --output-dir BusinessObjects/Generated \
      --context-dir Data \
      --context ContabilitateDbContext \
      --data-annotations \
      --force

  1.4 Instalare Pachete NuGet

  <PackageReference Include="DevExpress.ExpressApp" Version="24.2.*" />
  <PackageReference Include="DevExpress.ExpressApp.EFCore" Version="24.2.*" />
  <PackageReference Include="DevExpress.ExpressApp.Blazor" Version="24.2.*" />
  <PackageReference Include="DevExpress.ExpressApp.Security.EFCore" Version="24.2.*" />
  <PackageReference Include="DevExpress.ExpressApp.Validation.Blazor" Version="24.2.*" />
  <PackageReference Include="DevExpress.ExpressApp.ReportsV2.Blazor" Version="24.2.*" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.*" />
  <PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="8.0.*" />

  ---
  FAZA 2: MIGRARE ENTITĂȚI CORE (4-6 săptămâni)

  2.1 Modul Contabilitate (⭐⭐⭐ Săptămâna 1-2)

  Pași:
  1. Creare Business Objects: ChartOfAccounts, AccountingEntry, Journal, CostCenter
  2. Configurare EF Core Mapping: Mapare la CPLAN, CNOTE, CJURNALE, REPARTITORI
  3. Implementare Validări:

  ~~~
  [RuleRequiredField("AccountCode", DefaultContexts.Save)]
  [RuleUniqueValue("AccountCode", DefaultContexts.Save)]
  public class ChartOfAccounts : BaseObject
  {
      [Size(100)]
      public virtual string AccountCode { get; set; }

      [RuleRequiredField]
      [Size(1000)]
      public virtual string Description { get; set; }

      // Validare sold
      [RuleFromBoolProperty("ValidBalance", DefaultContexts.Save,
          "Soldul inițial trebuie să fie pe o singură parte (debit sau credit)")]
      public bool ValidBalance => (InitialDebitBalance == 0 || InitialCreditBalance == 0);
  }
~~~

  4. Migrare Proceduri Stocate:
    - sp_get_fisa_cont_new → Repository method + LINQ to Entities
    - spPlanUpdateDetalii → Business logic în Controllers

~~~
  Exemplu migrare sp_get_fisa_cont_new:
  public class AccountingController : ViewController<DetailView>
  {
      public async Task<List<AccountStatementLine>> GetAccountStatement(
          string accountCode,
          DateTime startDate,
          DateTime endDate,
          string functionalCode = null,
          string economicCode = null,
          int? costCenterId = null)
      {
          using var uow = ObjectSpace.CreateUnitOfWork();

          // Calcul sold inițial
          var initialBalance = await uow.Query<AccountingEntry>()
              .Where(e => e.DebitAccount == accountCode || e.CreditAccount == accountCode)
              .Where(e => e.EntryDate < startDate)
              .Where(e => e.Status == 1) // STARE = 1 (posted)
              .GroupBy(e => 1)
              .Select(g => new {
                  Debit = g.Where(e => e.DebitAccount == accountCode).Sum(e => e.Amount),
                  Credit = g.Where(e => e.CreditAccount == accountCode).Sum(e => e.Amount)
              })
              .FirstOrDefaultAsync();

          // Extragere mișcări în perioadă
          var entries = await uow.Query<AccountingEntry>()
              .Where(e => e.DebitAccount == accountCode || e.CreditAccount == accountCode)
              .Where(e => e.EntryDate >= startDate && e.EntryDate < endDate)
              .Where(e => e.Status == 1)
              .WhereIf(functionalCode != null, e => e.FunctionalCode == functionalCode)
              .WhereIf(economicCode != null, e => e.EconomicCode == economicCode)
              .OrderBy(e => e.EntryDate)
              .ToListAsync();

          // Calcul sold curent și construire result
          // ...
      }
  }
~~~

  2.2 Modul Buget (⭐⭐⭐ Săptămâna 3-4)

  Pași:
  1. Creare entități: BudgetVersion, BudgetAllocation, Commitment, Liquidation, PaymentOrder
  2. Implementare workflow ALOP (Angajament → Lichidare → Ordonanțare → Plată)
  3. Implementare calcul disponibil bugetar
  4. UI pentru planificare bugetară cu grid-uri grupate (banded)

~~~
  // Exemplu calcul disponibil
  public class BudgetAvailabilityCalculator
  {
      public decimal CalculateAvailable(BudgetAllocation allocation)
      {
          return allocation.AllocatedAmount
               - allocation.CommittedAmount
               - allocation.GetPendingCommitments();
      }

      [Action(Caption = "Verifică Disponibil")]
      public void CheckBudgetAvailability(BudgetAllocation allocation)
      {
          var available = CalculateAvailable(allocation);
          if (available < 0)
          {
              throw new UserFriendlyException(
                  $"Buget insuficient! Disponibil: {available:N2}");
          }
      }
  }
~~~

  2.3 Modul Casa și Banca (⭐⭐⭐ Săptămâna 5)

  Pași:
  1. Creare entități: BankAccount, CashDocument, BankStatement
  2. Import extrase bancare (Excel/CSV)
  3. Reconciliere automată
  4. Generare ordine de plată

  2.4 Modul Gestiune (⭐⭐ Săptămâna 6)

  Pași:
  1. Creare entități: Product, InventoryDocument, InventoryDocumentItem
  2. Calcul stocuri (FIFO/LIFO/CMP)
  3. Generare note contabile automate

  ---
  FAZA 3: MIGRARE LOGICĂ BUSINESS & CONTROLLERS (3-4 săptămâni)

  3.1 Transcriere Logică Business din Delphi

  Pattern: Event Handler Delphi → XAF Controller

~~~
  // Delphi VCL
  procedure TfrmNoteNew.OnNewRecord(Sender: TObject);
  begin
    QryNote.FieldByName('DATA').AsDateTime := Now;
    QryNote.FieldByName('STARE').AsInteger := 0; // Draft
    QryNote.FieldByName('C_O').AsInteger := CurrentUserID;
  end;
~~~

  ↓↓↓ Devine ↓↓↓

~~~
  // XAF Controller
  public class AccountingEntryController : ObjectViewController<DetailView, AccountingEntry>
  {
      protected override void OnActivated()
      {
          base.OnActivated();
          ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;
      }

      private void ObjectSpace_ObjectChanged(object sender, ObjectChangedEventArgs e)
      {
          if (e.Object is AccountingEntry entry && e.PropertyName == null)
          {
              // New record
              entry.EntryDate = DateTime.Now;
              entry.Status = 0; // Draft
              entry.CreatedBy = (ApplicationUser)SecuritySystem.CurrentUser;
          }
      }
  }
~~~

  3.2 Implementare Actions (Butoane → Menu Commands)

~~~
  // SimpleAction - Buton simplu
  public class PostAccountingEntryController : ObjectViewController<DetailView, AccountingEntry>
  {
      public PostAccountingEntryController()
      {
          var postAction = new SimpleAction(this, "PostEntry", PredefinedCategory.RecordEdit)
          {
              Caption = "Contabilizează",
              ConfirmationMessage = "Sigur doriți să contabilizați această notă?",
              ImageName = "Action_Grant"
          };
          postAction.Execute += PostAction_Execute;
      }

      private void PostAction_Execute(object sender, SimpleActionExecuteEventArgs e)
      {
          var entry = (AccountingEntry)View.CurrentObject;

          // Validări
          if (string.IsNullOrEmpty(entry.DebitAccount) || string.IsNullOrEmpty(entry.CreditAccount))
              throw new UserFriendlyException("Conturile debit și credit sunt obligatorii!");

          // Contabilizare
          entry.Status = 1; // Posted
          entry.OperationDate = DateTime.Now;

          ObjectSpace.CommitChanges();

          Application.ShowViewStrategy.ShowMessage("Nota a fost contabilizată cu succes!",
              InformationType.Success);
      }
  }
~~~

  3.3 Implementare Validări Complexe

~~~
  // Validare balanță (Debit = Credit)
  [RuleFromBoolProperty("BalancedEntry", DefaultContexts.Save,
      "Nota contabilă trebuie să fie echilibrată (Debit = Credit)",
      TargetCriteria = "Status = 1")] // Doar pentru note contabilizate
  public bool IsBalanced
  {
      get
      {
          var totalDebit = GetRelatedEntries()
              .Where(e => e.DebitAccount == this.AccountCode)
              .Sum(e => e.Amount);

          var totalCredit = GetRelatedEntries()
              .Where(e => e.CreditAccount == this.AccountCode)
              .Sum(e => e.Amount);

          return Math.Abs(totalDebit - totalCredit) < 0.01m;
      }
  }
~~~

  ---
  FAZA 4: MIGRARE RAPOARTE (2-3 săptămâni)

  4.1 FastReport → DevExpress Reports

  Rapoarte Prioritare:
  1. Fișă cont (sp_get_fisa_cont_new)
  2. Balanță de verificare
  3. Situații bugetare ALOP
  4. Rapoarte gestiune (NIR, fișe stoc)
  5. Registru casă/bancă

  Exemplu creare raport:

~~~
  // Controller pentru rapoarte
  public class AccountStatementReportController : ObjectViewController<DetailView, ChartOfAccounts>
  {
      public AccountStatementReportController()
      {
          var reportAction = new SimpleAction(this, "ShowAccountStatement", "Reports")
          {
              Caption = "Fișă Cont",
              ImageName = "BO_Report"
          };
          reportAction.Execute += (s, e) => ShowAccountStatementReport();
      }

      private void ShowAccountStatementReport()
      {
          var account = (ChartOfAccounts)View.CurrentObject;

          // Parametri raport
          var parameters = new Dictionary<string, object>
          {
              ["AccountCode"] = account.AccountCode,
              ["StartDate"] = new DateTime(DateTime.Now.Year, 1, 1),
              ["EndDate"] = DateTime.Now
          };

          // Afișare raport
          var objectSpace = Application.CreateObjectSpace(typeof(ReportDataV2));
          var report = objectSpace.FindObject<ReportDataV2>(
              CriteriaOperator.Parse("DisplayName = ?", "Fișă Cont"));

          ReportServiceController.ShowPreview(report, parameters);
      }
  }
~~~

  4.2 Creare Template-uri Rapoarte

  Folosind DevExpress Report Designer:
  - Import date din proceduri stocate sau LINQ
  - Layout similar cu rapoartele Delphi
  - Export PDF, Excel, Word

  ---
  FAZA 5: MIGRARE UI & CUSTOMIZĂRI (3-4 săptămâni)

  5.1 Customizare Layout Detail Views

~~~
  // Model.DesignedDiffs.xafml
  <DetailView Id="AccountingEntry_DetailView">
    <Layout>
      <LayoutGroup Id="Main">
        <LayoutGroup Id="Header" Caption="Antet Notă" Direction="Horizontal">
          <LayoutItem Id="EntryNumber" />
          <LayoutItem Id="EntryDate" />
          <LayoutItem Id="Journal" />
        </LayoutGroup>

        <LayoutGroup Id="Accounts" Caption="Conturi" Direction="Horizontal">
          <LayoutGroup Id="Debit">
            <LayoutItem Id="DebitAccount" />
            <LayoutItem Id="DebitCostCenter" />
          </LayoutGroup>
          <LayoutGroup Id="Credit">
            <LayoutItem Id="CreditAccount" />
            <LayoutItem Id="CreditCostCenter" />
          </LayoutGroup>
        </LayoutGroup>

        <LayoutGroup Id="Details" Caption="Detalii">
          <LayoutItem Id="Amount" />
          <LayoutItem Id="Description" />
          <LayoutItem Id="FunctionalCode" />
          <LayoutItem Id="EconomicCode" />
        </LayoutGroup>
      </LayoutGroup>
    </Layout>
  </DetailView>
~~~

  5.2 Configurare Grid-uri (List Views)

~~~
  // Application Model
  <ListView Id="AccountingEntry_ListView">
    <Columns>
      <ColumnInfo Id="EntryNumber" Width="100" Index="0" />
      <ColumnInfo Id="EntryDate" Width="100" DisplayFormat="{0:dd.MM.yyyy}" Index="1" />
      <ColumnInfo Id="Journal" Width="120" Index="2" />
      <ColumnInfo Id="DebitAccount" Width="100" Index="3" />
      <ColumnInfo Id="CreditAccount" Width="100" Index="4" />
      <ColumnInfo Id="Amount" Width="120" DisplayFormat="{0:N2}" Index="5" />
      <ColumnInfo Id="Description" Width="300" Index="6" />
    </Columns>

    <Filters>
      <Filter Id="Draft" Criteria="[Status] = 0" Caption="Proiect" />
      <Filter Id="Posted" Criteria="[Status] = 1" Caption="Contabilizat" />
      <Filter Id="CurrentMonth" Criteria="[EntryDate] &gt;= LocalDateTimeThisMonth()" />
    </Filters>
  </ListView>
~~~

  5.3 Implementare TreeList (Plan Conturi Ierarhic)

~~~
  public class ChartOfAccountsTreeListViewController : ObjectViewController<ListView, ChartOfAccounts>
  {
      protected override void OnActivated()
      {
          base.OnActivated();

          // Configurare TreeList în cod
          if (View is ListView listView && listView.Editor is GridListEditor gridEditor)
          {
              gridEditor.Grid.OptionsView.ShowIndicator = false;

              // Activare mod arbore
              gridEditor.Grid.ViewCollection.Add(new TreeListView(gridEditor.Grid));

              // Configurare relație părinte-copil
              gridEditor.Grid.DataSource = ObjectSpace.GetObjectsQuery<ChartOfAccounts>();
              gridEditor.Grid.ParentFieldName = nameof(ChartOfAccounts.ParentAccount);
              gridEditor.Grid.KeyFieldName = nameof(ChartOfAccounts.AccountCode);
          }
      }
  }
~~~

  ---
  FAZA 6: SECURITATE & PERMISIUNI (1-2 săptămâni)

  6.1 Configurare Security System

~~~
  // Startup.cs / Program.cs
  builder.Services.AddXaf(builder.Configuration, (application, services) => {
      application.Security = new SecurityStrategyComplex<ApplicationUser>(
          typeof(IdentityAuthenticationProvider));
  });
~~~

~~~
  // Definire roluri
  public class SecurityConfiguration
  {
      public static void ConfigureSecurity(IObjectSpace objectSpace)
      {
          // Rol Administrator
          var adminRole = objectSpace.FindObject<PermissionPolicyRole>(
              CriteriaOperator.Parse("Name = ?", "Administrators"));
          if (adminRole == null)
          {
              adminRole = objectSpace.CreateObject<PermissionPolicyRole>();
              adminRole.Name = "Administrators";
              adminRole.IsAdministrative = true; // Acces complet
          }

          // Rol Contabil
          var accountantRole = objectSpace.FindObject<PermissionPolicyRole>(
              CriteriaOperator.Parse("Name = ?", "Contabil"));
          if (accountantRole == null)
          {
              accountantRole = objectSpace.CreateObject<PermissionPolicyRole>();
              accountantRole.Name = "Contabil";

              // Permisiuni specifice
              accountantRole.AddTypePermission<AccountingEntry>(
                  SecurityOperations.FullObjectAccess, SecurityPermissionState.Allow);
              accountantRole.AddTypePermission<ChartOfAccounts>(
                  SecurityOperations.Read, SecurityPermissionState.Allow);
              accountantRole.AddTypePermission<Journal>(
                  SecurityOperations.Read, SecurityPermissionState.Allow);
          }

          objectSpace.CommitChanges();
      }
  }
~~~

  6.2 Audit Trail

  XAF oferă modul Audit Trail built-in pentru înregistrarea modificărilor:

~~~
  // Activare Audit Trail
  public class ContabilitateModule : ModuleBase
  {
      public override void Setup(XafApplication application)
      {
          base.Setup(application);
          application.SetupComplete += Application_SetupComplete;
      }

      private void Application_SetupComplete(object sender, EventArgs e)
      {
          var auditTrailService = ((XafApplication)sender).ServiceProvider
              .GetService<IAuditTrailService>();

          // Configurare audit pentru entități specifice
          auditTrailService.AddAuditedType(typeof(AccountingEntry));
          auditTrailService.AddAuditedType(typeof(BudgetAllocation));
      }
  }
~~~

  ---
  FAZA 7: MIGRARE DATE & TESTING (2-3 săptămâni)

  7.1 Strategie Migrare Date

  Opțiune 1: Folosire Bază de Date Existentă
  - Mapare directă la tabele existente prin EF Core
  - Avantaj: Date rămân în aceeași bază
  - Dezavantaj: Structură legacy, dificil de extins

  Opțiune 2: Migrare Completă
  - Creare bază nouă cu EF Core Migrations
  - Script de migrare date din baza veche în baza nouă
  - Avantaj: Structură curată, optimizată
  - Dezavantaj: Timp mai lung, risc migrare

  Recomandat: Opțiunea 1 inițial, apoi trecere treptată la Opțiunea 2

  // Script migrare exemple (opțional)
  public class DataMigrationService
  {
      public async Task MigrateChartOfAccounts()
      {
          using var oldDbContext = new OldContabilitateContext();
          using var newDbContext = new ContabilitateDbContext();

          var oldAccounts = await oldDbContext.CPLAN.ToListAsync();

          foreach (var oldAccount in oldAccounts)
          {
              var newAccount = new ChartOfAccounts
              {
                  AccountCode = oldAccount.CONT,
                  Description = oldAccount.ROMANA,
                  ParentAccount = oldAccount.PARINTE,
                  InitialDebitBalance = oldAccount.SID,
                  InitialCreditBalance = oldAccount.SIC,
                  IsSummary = oldAccount.SUMATOR,
                  IsAnalytic = oldAccount.IS_ANALITIC ?? false,
                  // ... mapare completă
              };

              newDbContext.ChartOfAccounts.Add(newAccount);
          }

          await newDbContext.SaveChangesAsync();
      }
  }

  7.2 Plan de Testare

  Teste Unitare
  [Fact]
  public void AccountingEntry_ShouldCalculateBalance_Correctly()
  {
      // Arrange
      var entry = new AccountingEntry
      {
          DebitAccount = "411",
          CreditAccount = "707",
          Amount = 1000m
      };

      // Act
      var isBalanced = entry.IsBalanced;

      // Assert
      Assert.True(isBalanced);
  }

  Teste Integrare
  - Test creare notă contabilă completă (cu validări)
  - Test calcul disponibil bugetar
  - Test generare rapoarte
  - Test import extrase bancare

  Teste UAT (User Acceptance Testing)
  - Scenarii reale de utilizare cu utilizatori finali
  - Comparație side-by-side Delphi vs XAF
  - Validare performanță (rapoarte, liste mari)

  ---
  FAZA 8: DEPLOYMENT & GO-LIVE (1-2 săptămâni)

  8.1 Pregătire Deployment

  Server Requirements:
  - Windows Server 2019/2022 sau Linux (pentru Blazor)
  - IIS 10+ sau Kestrel
  - SQL Server 2019/2022
  - .NET 8 Runtime

  Deployment Blazor Server:
  # Publicare aplicație
  dotnet publish -c Release -o ./publish

  # Configurare IIS
  - Creare Application Pool (.NET CLR Version: No Managed Code)
  - Creare Website/Application
  - Bind la port (ex: 443 HTTPS)

  Deployment WinForms (ClickOnce):
  <Project>
    <PropertyGroup>
      <PublishUrl>\\server\apps\Contabilitate\</PublishUrl>
      <InstallUrl>\\server\apps\Contabilitate\</InstallUrl>
      <ApplicationRevision>1</ApplicationRevision>
      <ApplicationVersion>1.0.0.%2a</ApplicationVersion>
    </PropertyGroup>
  </Project>

  8.2 Plan de Tranziție

  Faza Pilot (2-4 săptămâni):
  1. Rulare paralelă Delphi + XAF
  2. Comparare rezultate (rapoarte, balanțe)
  3. Training utilizatori (2-3 sesiuni)
  4. Colectare feedback, ajustări

  Go-Live:
  1. Backup complet bază de date
  2. Switch la aplicația XAF
  3. Monitorizare intensivă (1 săptămână)
  4. Suport dedicat utilizatori

  ---
  📈 ESTIMĂRI TIMP & RESURSE

  Durata Totală Estimată: 20-28 săptămâni (5-7 luni)

  | Fază                    | Săptămâni | Echipă Necesară           |
  |-------------------------|-----------|---------------------------|
  | Fază 1: Setup           | 2-3       | 1 dev senior .NET         |
  | Fază 2: Entități Core   | 4-6       | 2 dev .NET + 1 DBA        |
  | Fază 3: Logică Business | 3-4       | 2 dev .NET                |
  | Fază 4: Rapoarte        | 2-3       | 1 dev + 1 report designer |
  | Fază 5: UI              | 3-4       | 2 dev .NET                |
  | Fază 6: Securitate      | 1-2       | 1 dev senior              |
  | Fază 7: Testing         | 2-3       | 2 QA + devs               |
  | Fază 8: Deployment      | 1-2       | 1 dev + 1 sysadmin        |

  Echipă Recomandată:
  - 2-3 developeri .NET (experiență XAF, EF Core)
  - 1 DBA (SQL Server)
  - 1-2 QA testers
  - 1 business analyst / PO
  - 1 sysadmin (deployment)

  ---
  ⚠️ RISCURI & MITIGĂRI

  | Risc                                      | Impact | Probabilitate | Mitigare                                               |
  |-------------------------------------------|--------|---------------|--------------------------------------------------------|
  | Proceduri stocate complexe greu de migrat | MARE   | MARE          | Refactoring incremental, păstrare SP temporar          |
  | Performanță slabă pe liste mari           | MEDIU  | MEDIU         | Implementare Server Mode, optimizare query-uri         |
  | Rezistență utilizatori la schimbare       | MARE   | MARE          | Training intens, UI familiar, rulare paralelă          |
  | Logică business undocumented              | MARE   | MARE          | Reverse engineering cod Delphi, consultare utilizatori |
  | Bugs în producție după Go-Live            | MEDIU  | MARE          | Testing extensiv, pilot, backup, rollback plan         |

  ---
  🎯 RECOMANDĂRI FINALE

  Prioritizare Module

  1. Contabilitate (core business) - PRIO 1
  2. Buget (ALOP obligatoriu) - PRIO 1
  3. Casa/Banca (fluxuri zilnice) - PRIO 2
  4. Gestiune - PRIO 3
  5. Raportare (în paralel cu toate) - PRIO 1

  Abordare Recomandată

  - Incrementală: Modul cu modul, nu Big Bang
  - Agile: Sprint-uri 2 săptămâni, review constant
  - Pilot obligatoriu: Minimum 1 lună rulare paralelă
  - Documentare: Wiki intern cu toate customizările

  Beneficii XAF vs Delphi VCL

  ✅ Platform-independent (Web + Desktop din același cod)
  ✅ Mentenanță redusă (UI generat automat)
  ✅ Security built-in (autentificare, autorizare, audit)
  ✅ Rapoarte moderne (DevExpress Reports > FastReport)
  ✅ Extensibilitate (Controllers, Actions, Module)
  ✅ Cloud-ready (deploy Azure/AWS)
  ✅ Mobile-friendly (Blazor responsive)

  ---
  📚 RESURSE UTILE

  Documentație XAF

  - https://docs.devexpress.com/eXpressAppFramework/112670
  - https://docs.devexpress.com/eXpressAppFramework/402972
  - https://docs.devexpress.com/eXpressAppFramework/402125

  Training DevExpress

  - https://www.youtube.com/c/DevExpress/videos
  - https://supportcenter.devexpress.com/ticket/list?preset=recent

  Community

  - https://supportcenter.devexpress.com/ticket/list
  - https://github.com/DevExpress/XAF

  ---

● ---
  📊 REZUMAT FINAL

  Am finalizat analiza completă și am creat un plan detaliat de migrare de la Delphi XE2 + DevExpress VCL către .NET Core + XAF + Entity Framework Core.

  Ce am analizat:

  ✅ Structura proiectului Delphi:
  - 232 fișiere .pas (~105k linii cod)
  - 185 fișiere .dfm
  - 29 directoare cu module specializate
  - 2,300+ componente DevExpress VCL

  ✅ Baza de date SQL Server:
  - 312 tabele
  - Proceduri stocate complexe (sp_get_fisa_cont_new, etc.)
  - Relații între entități contabilitate, buget, gestiune

  ✅ Documentație DevExpress:
  - Componentele VCL (cxGrid, cxTreeList, etc.) → XAF Views & Editors
  - XAF architecture: Business Objects, List/Detail Views, Controllers, Actions
  - Entity Framework Core integration

  Planul de migrare include:

  📋 8 Faze concrete (5-7 luni):
  1. Setup proiect XAF + EF Core
  2. Migrare entități core (Contabilitate, Buget, Casa/Banca)
  3. Transcriere logică business (Event handlers → Controllers)
  4. Migrare rapoarte (FastReport → DevExpress Reports)
  5. Customizări UI (Layout, Grid-uri, TreeList ierarhic)
  6. Securitate & audit trail
  7. Migrare date & testing
  8. Deployment & Go-Live

  💻 Exemple de cod concrete pentru:
  - Business Objects cu EF Core
  - Controllers și Actions
  - Migrare proceduri stocate → LINQ
  - Configurare Security System
  - Rapoarte parametrizate

  ⚠️ Riscuri identificate și mitigări:
  - Proceduri stocate complexe
  - Performanță pe liste mari
  - Rezistență la schimbare
  - Logică business nedocumentată

  🎯 Echipă recomandată: 2-3 dev .NET, 1 DBA, 1-2 QA, 1 business analyst

  Beneficii majore ale migrării:

  - ✨ Cross-platform: Web (Blazor) + Desktop (WinForms) din același cod
  - 🚀 Dezvoltare rapidă: UI generat automat din Business Objects
  - 🔒 Security built-in: Autentificare, autorizare, audit trail
  - 📊 Rapoarte moderne: DevExpress Reports cu export PDF/Excel
  - ☁️ Cloud-ready: Deploy pe Azure/AWS
  - 📱 Responsive: UI adaptat pentru tablete/mobile

  Următorii pași recomandați:
  1. Review plan cu echipa și stakeholders
  2. Proof of Concept (2 săptămâni): Modul Contabilitate simplificat
  3. Aprobare buget și timeline
  4. Start Faza 1: Setup proiect

  Dacă aveți întrebări despre planul de migrare sau doriți detalii suplimentare despre anumite secțiuni, sunt aici să vă ajut!