# Migrare Proceduri Stocate: SQL → C# / LINQ

## PROCEDURI STOCATE CRITICE IDENTIFICATE

### 1. sp_get_fisa_cont_new - FIȘĂ CONT

**Complexitate**: ⭐⭐⭐⭐⭐ FOARTE MARE

**Parametri:**
```sql
@cont varchar(100),
@data_start datetime,
@data_end datetime,
@cu_inchidere bit = null,
@is_on_lucrare int = 0,
@id_unitate int = null,
@criteriu_centru_cost int = null,
@criteriu_codrep int = null,
@criteriu_material varchar(100) = null,
@criteriu_cod_functional varchar(100) = null,
@criteriu_cod_economic varchar(100) = null,
@criteriu_id_oi_unitati int = null,
@criteriu_id_oi_proiecte int = null,
@criteriu_este_detaliere bit = null,
@criteriu_titlu varchar(100) = null,
@criteriu_id_oi_grupe int = null
```

**Logică:**
1. Calcul sold inițial (din CPLAN + solduri_repartitori)
2. Extragere note contabile în perioadă cu filtrare complexă
3. Calcul sold curent pentru fiecare mișcare
4. Suport pentru defalcare pe criterii (functional, economic, unitate, proiect)

**Strategie Migrare:**
- **Opțiune 1**: Păstrare SP temporar, apel din C# prin raw SQL
- **Opțiune 2**: Refactoring complet în C# + LINQ (RECOMANDAT pe termen lung)

**Exemplu migrare în C#:**

```csharp
public class AccountStatementService
{
    private readonly IObjectSpace objectSpace;

    public AccountStatementService(IObjectSpace objectSpace)
    {
        this.objectSpace = objectSpace;
    }

    public async Task<AccountStatementResult> GetAccountStatement(
        string accountCode,
        DateTime startDate,
        DateTime endDate,
        AccountStatementCriteria criteria = null)
    {
        criteria ??= new AccountStatementCriteria();

        // Normalizare date
        startDate = startDate.Date;
        endDate = endDate.Date.AddDays(1);

        // 1. CALCUL SOLD INIȚIAL
        var initialBalance = await CalculateInitialBalance(accountCode, startDate, criteria);

        // 2. EXTRAGERE NOTE CONTABILE ÎN PERIOADĂ
        var entries = await GetAccountingEntries(accountCode, startDate, endDate, criteria);

        // 3. CALCUL SOLD CURENT
        var lines = CalculateRunningBalance(entries, initialBalance, criteria);

        return new AccountStatementResult
        {
            AccountCode = accountCode,
            StartDate = startDate,
            EndDate = endDate,
            InitialBalance = initialBalance,
            Entries = lines,
            FinalBalance = lines.LastOrDefault()?.RunningBalance ?? initialBalance
        };
    }

    private async Task<AccountBalance> CalculateInitialBalance(
        string accountCode,
        DateTime startDate,
        AccountStatementCriteria criteria)
    {
        // Sold din CPLAN
        var account = await objectSpace.GetObjectsQuery<ChartOfAccounts>()
            .Where(a => a.AccountCode == accountCode)
            .FirstOrDefaultAsync();

        if (account == null)
            throw new UserFriendlyException($"Contul {accountCode} nu există!");

        decimal debitBalance = account.InitialDebitBalance;
        decimal creditBalance = account.InitialCreditBalance;

        // Sold din solduri_repartitori (dacă sunt criterii de filtrare)
        if (criteria.HasCostCenterCriteria || criteria.HasBudgetCriteria || criteria.HasProjectCriteria)
        {
            var costCenterBalances = await objectSpace.GetObjectsQuery<CostCenterBalance>()
                .Where(b => b.Account == accountCode)
                .WhereIf(criteria.CostCenterId.HasValue, b => b.CostCenterId == criteria.CostCenterId)
                .WhereIf(!string.IsNullOrEmpty(criteria.FunctionalCode), b => b.FunctionalCode == criteria.FunctionalCode)
                .WhereIf(!string.IsNullOrEmpty(criteria.EconomicCode), b => b.EconomicCode == criteria.EconomicCode)
                .WhereIf(criteria.OrganizationalUnitId.HasValue, b => b.OrganizationalUnitId == criteria.OrganizationalUnitId)
                .WhereIf(criteria.ProjectId.HasValue, b => b.ProjectId == criteria.ProjectId)
                .ToListAsync();

            debitBalance = costCenterBalances.Sum(b => b.DebitBalance);
            creditBalance = costCenterBalances.Sum(b => b.CreditBalance);
        }

        // Adăugare mișcări anterioare datei de start
        var priorEntries = await GetAccountingEntries(accountCode, DateTime.MinValue, startDate, criteria);

        debitBalance += priorEntries.Where(e => e.IsDebit).Sum(e => e.Amount);
        creditBalance += priorEntries.Where(e => !e.IsDebit).Sum(e => e.Amount);

        return new AccountBalance
        {
            DebitBalance = debitBalance,
            CreditBalance = creditBalance,
            AccountType = account.AccountType
        };
    }

    private async Task<List<AccountStatementEntry>> GetAccountingEntries(
        string accountCode,
        DateTime startDate,
        DateTime endDate,
        AccountStatementCriteria criteria)
    {
        // Extragere note DEBIT
        var debitEntries = objectSpace.GetObjectsQuery<AccountingEntry>()
            .Where(e => e.DebitAccount == accountCode)
            .Where(e => e.EntryDate >= startDate && e.EntryDate < endDate)
            .Where(e => e.Status == 1); // Posted only

        // Filtre opționale
        if (!criteria.IncludeClosingEntries)
            debitEntries = debitEntries.Where(e => !e.Journal.IsClosingJournal);

        if (criteria.CostCenterId.HasValue)
            debitEntries = debitEntries.Where(e => e.DebitCostCenterId == criteria.CostCenterId);

        if (!string.IsNullOrEmpty(criteria.FunctionalCode))
        {
            var functionalClass = GetBudgetClass(criteria.FunctionalCode);
            debitEntries = debitEntries.Where(e =>
                e.DebitFunctionalCode != null &&
                e.DebitFunctionalCode.StartsWith(functionalClass));
        }

        if (!string.IsNullOrEmpty(criteria.EconomicCode))
        {
            var economicClass = GetBudgetClass(criteria.EconomicCode);
            debitEntries = debitEntries.Where(e =>
                e.DebitEconomicCode != null &&
                e.DebitEconomicCode.StartsWith(economicClass));
        }

        if (criteria.OrganizationalUnitId.HasValue)
            debitEntries = debitEntries.Where(e => e.DebitOrganizationalUnitId == criteria.OrganizationalUnitId);

        if (criteria.ProjectId.HasValue)
            debitEntries = debitEntries.Where(e => e.DebitProjectId == criteria.ProjectId);

        if (!string.IsNullOrEmpty(criteria.MaterialCode))
            debitEntries = debitEntries.Where(e => e.MaterialAnalyticCode == criteria.MaterialCode);

        // Extragere note CREDIT (analog)
        var creditEntries = objectSpace.GetObjectsQuery<AccountingEntry>()
            .Where(e => e.CreditAccount == accountCode)
            .Where(e => e.EntryDate >= startDate && e.EntryDate < endDate)
            .Where(e => e.Status == 1);

        if (!criteria.IncludeClosingEntries)
            creditEntries = creditEntries.Where(e => !e.Journal.IsClosingJournal);

        if (criteria.CostCenterId.HasValue)
            creditEntries = creditEntries.Where(e => e.CreditCostCenterId == criteria.CostCenterId);

        // ... apply same filters for credit side

        // Combinare și transformare
        var debitList = await debitEntries
            .Select(e => new AccountStatementEntry
            {
                EntryId = e.Id,
                EntryNumber = e.EntryNumber,
                EntryDate = e.EntryDate,
                Journal = e.Journal,
                Description = e.Description,
                CorrespondingAccount = e.CreditAccount,
                CostCenter = e.DebitCostCenter?.Name,
                FunctionalCode = e.DebitFunctionalCode,
                EconomicCode = e.DebitEconomicCode,
                Amount = e.Amount,
                IsDebit = true,
                DocumentType = e.DocumentType,
                DocumentNumber = e.DocumentNumber,
                DocumentDate = e.DocumentDate,
                CreatedBy = e.CreatedBy?.UserName,
                ReferenceDate = e.ReferenceDate ?? e.EntryDate
            })
            .ToListAsync();

        var creditList = await creditEntries
            .Select(e => new AccountStatementEntry
            {
                EntryId = e.Id,
                EntryNumber = e.EntryNumber,
                EntryDate = e.EntryDate,
                Journal = e.Journal,
                Description = e.Description,
                CorrespondingAccount = e.DebitAccount,
                CostCenter = e.CreditCostCenter?.Name,
                FunctionalCode = e.CreditFunctionalCode,
                EconomicCode = e.CreditEconomicCode,
                Amount = e.Amount,
                IsDebit = false,
                DocumentType = e.DocumentType,
                DocumentNumber = e.DocumentNumber,
                DocumentDate = e.DocumentDate,
                CreatedBy = e.CreatedBy?.UserName,
                ReferenceDate = e.ReferenceDate ?? e.EntryDate
            })
            .ToListAsync();

        // Combinare și sortare
        var allEntries = debitList.Concat(creditList)
            .OrderBy(e => e.EntryDate)
            .ThenBy(e => e.ReferenceDate)
            .ThenBy(e => e.EntryNumber)
            .ThenBy(e => e.EntryId)
            .ToList();

        return allEntries;
    }

    private List<AccountStatementEntry> CalculateRunningBalance(
        List<AccountStatementEntry> entries,
        AccountBalance initialBalance,
        AccountStatementCriteria criteria)
    {
        decimal runningDebit = initialBalance.DebitBalance;
        decimal runningCredit = initialBalance.CreditBalance;

        foreach (var entry in entries)
        {
            if (entry.IsDebit)
                runningDebit += entry.Amount;
            else
                runningCredit += entry.Amount;

            // Calcul sold în funcție de tipul contului
            if (initialBalance.AccountType == "D" ||
                (initialBalance.AccountType == "B" && runningDebit >= runningCredit))
            {
                entry.RunningBalance = runningDebit - runningCredit;
                entry.BalanceSide = "D";
            }
            else
            {
                entry.RunningBalance = runningCredit - runningDebit;
                entry.BalanceSide = "C";
            }
        }

        return entries;
    }

    private string GetBudgetClass(string code)
    {
        // Ex: "01.01" → "01.01%"
        // Extrage clasa pentru filtrare LIKE
        return code?.Substring(0, Math.Min(code.Length, 5)) + "%";
    }
}

// DTOs
public class AccountStatementCriteria
{
    public int? CostCenterId { get; set; }
    public string FunctionalCode { get; set; }
    public string EconomicCode { get; set; }
    public int? OrganizationalUnitId { get; set; }
    public int? ProjectId { get; set; }
    public string MaterialCode { get; set; }
    public string Title { get; set; } // Primele 2 cifre cod economic
    public bool IncludeClosingEntries { get; set; }

    public bool HasCostCenterCriteria => CostCenterId.HasValue;
    public bool HasBudgetCriteria => !string.IsNullOrEmpty(FunctionalCode) || !string.IsNullOrEmpty(EconomicCode);
    public bool HasProjectCriteria => ProjectId.HasValue || OrganizationalUnitId.HasValue;
}

public class AccountStatementEntry
{
    public int EntryId { get; set; }
    public string EntryNumber { get; set; }
    public DateTime EntryDate { get; set; }
    public string Journal { get; set; }
    public string Description { get; set; }
    public string CorrespondingAccount { get; set; }
    public string CostCenter { get; set; }
    public string FunctionalCode { get; set; }
    public string EconomicCode { get; set; }
    public decimal Amount { get; set; }
    public bool IsDebit { get; set; }
    public decimal RunningBalance { get; set; }
    public string BalanceSide { get; set; } // "D" or "C"
    public string DocumentType { get; set; }
    public string DocumentNumber { get; set; }
    public DateTime? DocumentDate { get; set; }
    public string CreatedBy { get; set; }
    public DateTime ReferenceDate { get; set; }
}

public class AccountBalance
{
    public decimal DebitBalance { get; set; }
    public decimal CreditBalance { get; set; }
    public string AccountType { get; set; } // D, C, B
}

public class AccountStatementResult
{
    public string AccountCode { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public AccountBalance InitialBalance { get; set; }
    public List<AccountStatementEntry> Entries { get; set; }
    public decimal FinalBalance { get; set; }
}
```

**Utilizare în XAF:**
```csharp
public class AccountStatementReportController : ObjectViewController<DetailView, ChartOfAccounts>
{
    public AccountStatementReportController()
    {
        var showStatementAction = new PopupWindowShowAction(this, "ShowAccountStatement", "Reports")
        {
            Caption = "Fișă Cont",
            ImageName = "BO_Report"
        };

        showStatementAction.CustomizePopupWindowParams += (s, e) =>
        {
            var criteria = ObjectSpace.CreateObject<AccountStatementCriteria>();
            criteria.StartDate = new DateTime(DateTime.Now.Year, 1, 1);
            criteria.EndDate = DateTime.Now;

            var detailView = Application.CreateDetailView(ObjectSpace, criteria);
            e.View = detailView;
        };

        showStatementAction.Execute += async (s, e) =>
        {
            var account = (ChartOfAccounts)View.CurrentObject;
            var criteria = (AccountStatementCriteria)e.PopupWindowViewCurrentObject;

            var service = new AccountStatementService(ObjectSpace);
            var result = await service.GetAccountStatement(
                account.AccountCode,
                criteria.StartDate,
                criteria.EndDate,
                criteria);

            // Afișare raport sau export
            ShowReport(result);
        };
    }
}
```

---

### 2. spPlanUpdateDetalii - UPDATE PROPRIETĂȚI CONT

**Logică:**
```sql
CREATE PROCEDURE spPlanUpdateDetalii(@contDest varchar(100), @parinte varchar(100))
AS
BEGIN
  UPDATE a
  SET a.sumator = b.sumator,
      a.fctcont = b.fctcont,
      a.tip = b.tip,
      a.balanta = b.balanta,
      a.is_sintetic = 0
  FROM cplan a, cplan b
  WHERE a.cont = @contDest AND b.cont = @parinte
END
```

**Strategie Migrare:**
Implementare în business logic (event handler `OnSaving`)

```csharp
public class ChartOfAccounts : BaseObject
{
    [Size(100)]
    public virtual string ParentAccount { get; set; }

    protected override void OnSaving()
    {
        base.OnSaving();

        // Update proprietăți de la părinte când se setează ParentAccount
        if (!string.IsNullOrEmpty(ParentAccount) && Session.IsNewObject(this))
        {
            var parent = Session.FindObject<ChartOfAccounts>(
                CriteriaOperator.Parse("AccountCode = ?", ParentAccount));

            if (parent != null)
            {
                this.IsSummary = parent.IsSummary;
                this.AccountType = parent.AccountType;
                this.Type = parent.Type;
                this.BalanceType = parent.BalanceType;
                this.IsSynthetic = false;
            }
        }
    }
}

// SAU: Controller pentru propagare proprietăți
public class ChartOfAccountsController : ObjectViewController<DetailView, ChartOfAccounts>
{
    protected override void OnActivated()
    {
        base.OnActivated();
        ObjectSpace.ObjectChanged += ObjectSpace_ObjectChanged;
    }

    private void ObjectSpace_ObjectChanged(object sender, ObjectChangedEventArgs e)
    {
        if (e.Object is ChartOfAccounts account && e.PropertyName == nameof(ChartOfAccounts.ParentAccount))
        {
            if (!string.IsNullOrEmpty(account.ParentAccount))
            {
                var parent = ObjectSpace.GetObjectsQuery<ChartOfAccounts>()
                    .FirstOrDefault(a => a.AccountCode == account.ParentAccount);

                if (parent != null)
                {
                    account.IsSummary = parent.IsSummary;
                    account.AccountType = parent.AccountType;
                    account.Type = parent.Type;
                    account.BalanceType = parent.BalanceType;
                    account.IsSynthetic = false;
                }
            }
        }
    }
}
```

---

### 3. sp_gest_get_lista_doc - LISTĂ DOCUMENTE GESTIUNE

**Logică:** Returnează liste de documente cu filtrare complexă (tip doc, dată, gestiune, etc.)

**Strategie Migrare:**
Folosire LINQ to Entities cu Expression Trees pentru filtre dinamice

```csharp
public class InventoryDocumentListService
{
    public async Task<List<InventoryDocument>> GetDocuments(DocumentListCriteria criteria)
    {
        var query = objectSpace.GetObjectsQuery<InventoryDocument>();

        // Filtre
        if (criteria.DocumentTypeId.HasValue)
            query = query.Where(d => d.DocumentTypeId == criteria.DocumentTypeId);

        if (criteria.StartDate.HasValue)
            query = query.Where(d => d.DocumentDate >= criteria.StartDate);

        if (criteria.EndDate.HasValue)
            query = query.Where(d => d.DocumentDate < criteria.EndDate);

        if (criteria.SupplierId.HasValue)
            query = query.Where(d => d.SupplierId == criteria.SupplierId);

        if (criteria.ReceiverId.HasValue)
            query = query.Where(d => d.ReceiverId == criteria.ReceiverId);

        if (criteria.Status.HasValue)
            query = query.Where(d => d.Status == criteria.Status);

        // Sortare
        query = query.OrderByDescending(d => d.DocumentDate)
                     .ThenByDescending(d => d.DocumentNumber);

        return await query.ToListAsync();
    }
}
```

---

### 4. spCplanSoldMultianual - CALCUL SOLD MULTIANUAL

**Logică:** Calcul sold conturi pe mai mulți ani fiscali

**Strategie Migrare:**
```csharp
public class MultiYearBalanceCalculator
{
    public async Task<Dictionary<int, AccountBalance>> CalculateMultiYearBalance(
        string accountCode,
        int startYear,
        int endYear)
    {
        var result = new Dictionary<int, AccountBalance>();

        for (int year = startYear; year <= endYear; year++)
        {
            var startDate = new DateTime(year, 1, 1);
            var endDate = new DateTime(year, 12, 31);

            var balance = await CalculateYearBalance(accountCode, startDate, endDate);
            result[year] = balance;
        }

        return result;
    }

    private async Task<AccountBalance> CalculateYearBalance(
        string accountCode,
        DateTime startDate,
        DateTime endDate)
    {
        var account = await objectSpace.GetObjectsQuery<ChartOfAccounts>()
            .FirstOrDefaultAsync(a => a.AccountCode == accountCode && a.FiscalYear == startDate.Year);

        var entries = await objectSpace.GetObjectsQuery<AccountingEntry>()
            .Where(e => (e.DebitAccount == accountCode || e.CreditAccount == accountCode))
            .Where(e => e.EntryDate >= startDate && e.EntryDate <= endDate)
            .Where(e => e.Status == 1)
            .ToListAsync();

        var debitSum = entries.Where(e => e.DebitAccount == accountCode).Sum(e => e.Amount);
        var creditSum = entries.Where(e => e.CreditAccount == accountCode).Sum(e => e.Amount);

        return new AccountBalance
        {
            DebitBalance = (account?.InitialDebitBalance ?? 0) + debitSum,
            CreditBalance = (account?.InitialCreditBalance ?? 0) + creditSum,
            AccountType = account?.AccountType ?? "B"
        };
    }
}
```

---

## STRATEGIE GENERALĂ DE MIGRARE

### Faza 1: Identificare și Documentare (1 săptămână)
- [ ] Inventariere toate SP-urile folosite
- [ ] Analiză dependențe (ce SP apelează alte SP)
- [ ] Identificare SP critice vs. SP simple

### Faza 2: Migrare SP Simple (2 săptămâni)
- [ ] SP cu logică simplă → LINQ direct
- [ ] SP de validare → RuleFromBoolProperty
- [ ] SP de calcul → Business Object computed properties

### Faza 3: Migrare SP Complexe (4 săptămâni)
- [ ] Refactoring sp_get_fisa_cont_new → AccountStatementService
- [ ] Refactoring sp_gest_get_lista_doc → Repository methods
- [ ] Testare extensivă (comparație rezultate SQL vs. C#)

### Faza 4: Optimizare (2 săptămâni)
- [ ] Profiling query-uri LINQ
- [ ] Adăugare indecși pe coloane filtrate frecvent
- [ ] Implementare caching pentru query-uri repetitive
- [ ] Considerat EF Core Compiled Queries pentru performanță

---

## ALTERNATIVE - PĂSTRARE SP TEMPORARĂ

Dacă timpul este critic, se pot păstra SP-urile și apela din C#:

```csharp
public class LegacyStoredProcedureService
{
    private readonly IObjectSpace objectSpace;

    public async Task<DataTable> ExecuteAccountStatement(
        string accountCode,
        DateTime startDate,
        DateTime endDate)
    {
        var connection = ((EFCoreObjectSpace)objectSpace).DbContext.Database.GetDbConnection();

        using var command = connection.CreateCommand();
        command.CommandText = "sp_get_fisa_cont_new";
        command.CommandType = CommandType.StoredProcedure;

        command.Parameters.Add(new SqlParameter("@cont", accountCode));
        command.Parameters.Add(new SqlParameter("@data_start", startDate));
        command.Parameters.Add(new SqlParameter("@data_end", endDate));

        await connection.OpenAsync();

        using var reader = await command.ExecuteReaderAsync();
        var dataTable = new DataTable();
        dataTable.Load(reader);

        return dataTable;
    }
}
```

**Avantaje:**
- Migrare rapidă inițială
- Risc redus de bugs

**Dezavantaje:**
- Dependență de SQL Server
- Dificil de testat
- Imposibil de folosit cu alte DBMS
- Nu beneficiază de Entity Framework change tracking

---

## CHECKLIST MIGRARE SP

- [ ] sp_get_fisa_cont_new → AccountStatementService (⭐⭐⭐⭐⭐)
- [ ] spPlanUpdateDetalii → Business logic (⭐⭐)
- [ ] sp_gest_get_lista_doc → Repository (⭐⭐⭐)
- [ ] spCplanSoldMultianual → MultiYearBalanceCalculator (⭐⭐⭐)
- [ ] sp_get_note_imperechere → LINQ query (⭐⭐)
- [ ] sp_get_mapare_conturi_ordine_plata → Configuration table (⭐)

**Legendă:**
⭐ = Simplă (< 1 zi)
⭐⭐⭐ = Medie (2-3 zile)
⭐⭐⭐⭐⭐ = Complexă (1-2 săptămâni)
