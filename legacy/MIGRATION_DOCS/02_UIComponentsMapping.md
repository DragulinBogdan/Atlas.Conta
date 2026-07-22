# Mapare Componente UI: DevExpress VCL → XAF

## COMPONENTE GRID

### cxGrid + cxGridDBTableView → GridListEditor (XAF List View)

**Delphi VCL:**
```pascal
// DataUnit.pas
object cxGrid1: TcxGrid
  object cxGridDBTableView1: TcxGridDBTableView
    DataController.DataSource = DataSource1
    object cxGridDBColumn1: TcxGridDBColumn
      DataBinding.FieldName = 'CONT'
      Caption = 'Cont'
      Width = 100
    end
    object cxGridDBColumn2: TcxGridDBColumn
      DataBinding.FieldName = 'ROMANA'
      Caption = 'Denumire'
      Width = 300
    end
  end
end
```

**XAF Echivalent:**
```xml
<!-- Model.DesignedDiffs.xafml -->
<Application>
  <Views>
    <ListView Id="ChartOfAccounts_ListView" EditorType="GridListEditor">
      <Columns>
        <ColumnInfo Id="AccountCode" Width="100" Caption="Cont" Index="0" />
        <ColumnInfo Id="Description" Width="300" Caption="Denumire" Index="1" />
        <ColumnInfo Id="InitialDebitBalance" Width="120" DisplayFormat="{0:N2}"
                    Caption="Sold Inițial D" Index="2" />
        <ColumnInfo Id="InitialCreditBalance" Width="120" DisplayFormat="{0:N2}"
                    Caption="Sold Inițial C" Index="3" />
      </Columns>
    </ListView>
  </Views>
</Application>
```

**Customizare în cod:**
```csharp
public class ChartOfAccountsListViewController : ObjectViewController<ListView, ChartOfAccounts>
{
    protected override void OnActivated()
    {
        base.OnActivated();

        if (View.Editor is GridListEditor gridEditor)
        {
            // Customizare GridView
            gridEditor.GridView.OptionsView.ShowGroupPanel = false;
            gridEditor.GridView.OptionsView.ColumnAutoWidth = false;

            // Format condiționat
            gridEditor.GridView.CustomRowCellStyle += (s, e) =>
            {
                if (e.Column.FieldName == "AccountCode" && e.RowHandle >= 0)
                {
                    var account = (ChartOfAccounts)gridEditor.GridView.GetRow(e.RowHandle);
                    if (account.IsSummary)
                    {
                        e.Appearance.Font = new Font(e.Appearance.Font, FontStyle.Bold);
                        e.Appearance.BackColor = Color.LightGray;
                    }
                }
            };
        }
    }
}
```

---

### cxGridDBBandedTableView → BandedGridView (XAF)

**Delphi VCL:**
```pascal
object cxGridDBBandedTableView1: TcxGridDBBandedTableView
  Bands = <
    item
      Caption = 'Identificare'
      Index = 0
    end
    item
      Caption = 'Solduri'
      Index = 1
    end>

  object colCont: TcxGridDBBandedColumn
    BandIndex = 0
    Position.BandIndex = 0
    Position.RowIndex = 0
    Position.ColIndex = 0
  end
end
```

**XAF Echivalent (Blazor - BandedGridView):**
```csharp
// Customizare în cod - Blazor nu suportă bands direct din model
public class AccountingEntryBandedViewController : ObjectViewController<ListView, AccountingEntry>
{
    protected override void OnActivated()
    {
        base.OnActivated();

        if (View.Editor is DevExpress.ExpressApp.Blazor.Editors.Grid.DxGridListEditor blazorGrid)
        {
            // Blazor: Grupare coloane prin nested LayoutGroups
            // Sau folosire DxDataGrid cu column groups
        }
    }
}
```

**XAF WinForms - BandedGridView complet:**
```csharp
public class AccountingEntryWinBandedViewController : ObjectViewController<ListView, AccountingEntry>
{
    protected override void OnActivated()
    {
        base.OnActivated();

        if (View.Editor is GridListEditor gridEditor)
        {
            // Conversie la BandedGridView
            var bandedView = new BandedGridView(gridEditor.Grid);
            gridEditor.Grid.MainView = bandedView;

            // Definire benzi
            var identificationBand = new GridBand { Caption = "Identificare", VisibleIndex = 0 };
            var accountsBand = new GridBand { Caption = "Conturi", VisibleIndex = 1 };
            var amountsBand = new GridBand { Caption = "Sume", VisibleIndex = 2 };

            bandedView.Bands.AddRange(new[] { identificationBand, accountsBand, amountsBand });

            // Alocare coloane la benzi
            var colEntryNumber = bandedView.Columns["EntryNumber"];
            if (colEntryNumber != null)
            {
                colEntryNumber.OwnerBand = identificationBand;
                colEntryNumber.VisibleIndex = 0;
            }

            var colDebitAccount = bandedView.Columns["DebitAccount"];
            if (colDebitAccount != null)
            {
                colDebitAccount.OwnerBand = accountsBand;
                colDebitAccount.VisibleIndex = 0;
            }

            var colAmount = bandedView.Columns["Amount"];
            if (colAmount != null)
            {
                colAmount.OwnerBand = amountsBand;
                colAmount.VisibleIndex = 0;
                colAmount.DisplayFormat.FormatType = FormatType.Numeric;
                colAmount.DisplayFormat.FormatString = "N2";
            }
        }
    }
}
```

---

### cxDBTreeList → TreeListEditor (XAF)

**Delphi VCL:**
```pascal
object cxDBTreeList1: TcxDBTreeList
  DataController.DataSource = DataSource1
  DataController.ParentField = 'PARINTE'
  DataController.KeyField = 'CONT'

  object cxDBTreeListColumn1: TcxDBTreeListColumn
    DataBinding.FieldName = 'CONT'
    Caption = 'Cont'
  end

  object cxDBTreeListColumn2: TcxDBTreeListColumn
    DataBinding.FieldName = 'ROMANA'
    Caption = 'Denumire'
  end
end
```

**XAF Echivalent:**
```xml
<!-- Model.DesignedDiffs.xafml -->
<ListView Id="ChartOfAccounts_ListView_Tree" EditorTypeName="DevExpress.ExpressApp.TreeListEditors.Win.TreeListEditor">
  <Columns>
    <ColumnInfo Id="AccountCode" Width="150" SortIndex="0" SortOrder="Ascending" />
    <ColumnInfo Id="Description" Width="400" />
    <ColumnInfo Id="InitialDebitBalance" Width="120" DisplayFormat="{0:N2}" />
  </Columns>
</ListView>
```

**Configurare TreeList în cod:**
```csharp
public class ChartOfAccountsTreeViewController : ObjectViewController<ListView, ChartOfAccounts>
{
    protected override void OnViewControlsCreated()
    {
        base.OnViewControlsCreated();

        if (View.Editor is TreeListEditor treeEditor)
        {
            // Configurare ierarhie
            treeEditor.TreeList.ParentFieldName = nameof(ChartOfAccounts.ParentAccount);
            treeEditor.TreeList.KeyFieldName = nameof(ChartOfAccounts.AccountCode);

            // Opțiuni
            treeEditor.TreeList.OptionsView.ShowIndicator = false;
            treeEditor.TreeList.OptionsView.AutoWidth = false;

            // Expandare automată la nivelul 1
            treeEditor.TreeList.ExpandToLevel(1);

            // Iconiță pentru conturi sumatoare
            treeEditor.TreeList.GetStateImageIndex += (s, e) =>
            {
                var account = e.Node.Tag as ChartOfAccounts;
                e.ImageIndex = account?.IsSummary == true ? 1 : 0;
            };
        }
    }
}
```

---

## COMPONENTE EDITOARE

### cxDBTextEdit → StringPropertyEditor

**Delphi VCL:**
```pascal
object cxDBTextEdit1: TcxDBTextEdit
  DataBinding.DataField = 'EXPLICATIE'
  Properties.MaxLength = 254
  Width = 400
end
```

**XAF Echivalent:**
```csharp
public class AccountingEntry : BaseObject
{
    [Size(4096)] // MaxLength
    [FieldSize(FieldSizeAttribute.Unlimited)] // pentru memo în grid
    public virtual string Description { get; set; }
}
```

```xml
<!-- Model -->
<DetailView Id="AccountingEntry_DetailView">
  <Items>
    <PropertyEditor Id="Description" PropertyName="Description"
                    RowCount="3" /> <!-- Multiline pentru Description lung -->
  </Items>
</DetailView>
```

---

### cxDBDateEdit → DateTimePropertyEditor

**Delphi VCL:**
```pascal
object cxDBDateEdit1: TcxDBDateEdit
  DataBinding.DataField = 'DATA'
  Properties.DateButtons = [btnToday, btnClear]
  Properties.DisplayFormat = 'dd.mm.yyyy'
end
```

**XAF Echivalent:**
```csharp
public class AccountingEntry : BaseObject
{
    [DisplayFormat("{0:dd.MM.yyyy}")] // Format românesc
    public virtual DateTime EntryDate { get; set; }
}
```

**Customizare format în Application Model:**
```xml
<PropertyEditor Id="EntryDate" PropertyName="EntryDate"
                DisplayFormat="{0:dd.MM.yyyy HH:mm}" />
```

---

### cxDBCurrencyEdit → DecimalPropertyEditor

**Delphi VCL:**
```pascal
object cxDBCurrencyEdit1: TcxDBCurrencyEdit
  DataBinding.DataField = 'VALOARE'
  Properties.DisplayFormat = ',0.00'
  Properties.Alignment.Horz = taRightJustify
end
```

**XAF Echivalent:**
```csharp
public class AccountingEntry : BaseObject
{
    [DisplayFormat("{0:N2}")] // 2 zecimale, separator mii
    [ModelDefault("DisplayFormat", "{0:N2}")]
    [ModelDefault("EditMask", "n2")]
    public virtual decimal Amount { get; set; }
}
```

---

### cxDBCheckBox → BooleanPropertyEditor

**Delphi VCL:**
```pascal
object cxDBCheckBox1: TcxDBCheckBox
  DataBinding.DataField = 'SUMATOR'
  Properties.ValueChecked = 1
  Properties.ValueUnchecked = 0
end
```

**XAF Echivalent:**
```csharp
public class ChartOfAccounts : BaseObject
{
    [ImmediatePostData] // Update UI imediat la schimbare
    public virtual bool IsSummary { get; set; }
}
```

---

### cxDBImageComboBox → Enum Property Editor

**Delphi VCL:**
```pascal
object cxDBImageComboBox1: TcxDBImageComboBox
  DataBinding.DataField = 'FCTCONT'
  Properties.Items = <
    item
      Value = 'D'
      Description = 'Debitor'
      ImageIndex = 0
    end
    item
      Value = 'C'
      Description = 'Creditor'
      ImageIndex = 1
    end
    item
      Value = 'B'
      Description = 'Bilateral'
      ImageIndex = 2
    end>
end
```

**XAF Echivalent:**
```csharp
public enum AccountType
{
    [ImageName("debit_icon")]
    Debit = 0,

    [ImageName("credit_icon")]
    Credit = 1,

    [ImageName("bilateral_icon")]
    Bilateral = 2
}

public class ChartOfAccounts : BaseObject
{
    [ImmediatePostData]
    public virtual AccountType AccountType { get; set; }
}
```

---

### cxDBLookupComboBox → LookupPropertyEditor

**Delphi VCL:**
```pascal
object cxDBLookupComboBox1: TcxDBLookupComboBox
  DataBinding.DataField = 'ID_REPARTITORI'
  Properties.KeyFieldNames = 'ID_REPARTITORI'
  Properties.ListFieldNames = 'NUME'
  Properties.ListSource = DataSourceRepartitori
end
```

**XAF Echivalent:**
```csharp
public class AccountingEntry : BaseObject
{
    // Simplu: Association cu entitate
    [Association("CostCenter-DebitEntries")]
    public virtual CostCenter DebitCostCenter { get; set; }

    // Lookup va fi generat automat cu DisplayName din CostCenter
}

public class CostCenter : BaseObject
{
    [Size(1000)]
    public virtual string Name { get; set; }

    // Default display member
    public override string ToString() => Name;
}
```

**Customizare Lookup:**
```xml
<!-- Model -->
<PropertyEditor Id="DebitCostCenter" PropertyName="DebitCostCenter"
                LookupEditorMode="AllItems"
                DataSourceCriteria="[IsInternal] = True" />
```

---

## BUTOANE ȘI ACȚIUNI

### cxButton → SimpleAction / PopupWindowShowAction

**Delphi VCL:**
```pascal
object btnPost: TcxButton
  Caption = 'Contabilizează'
  Enabled = True
  OnClick = btnPostClick
end

procedure TfrmNote.btnPostClick(Sender: TObject);
begin
  if QryNote.FieldByName('STARE').AsInteger = 0 then
  begin
    // Validări
    if ValidateEntry then
    begin
      QryNote.Edit;
      QryNote.FieldByName('STARE').AsInteger := 1;
      QryNote.FieldByName('DATA_OPERARE').AsDateTime := Now;
      QryNote.Post;
      ShowMessage('Nota a fost contabilizată!');
    end;
  end;
end;
```

**XAF Echivalent:**
```csharp
public class PostAccountingEntryController : ObjectViewController<DetailView, AccountingEntry>
{
    private SimpleAction postAction;

    public PostAccountingEntryController()
    {
        postAction = new SimpleAction(this, "PostEntry", PredefinedCategory.RecordEdit)
        {
            Caption = "Contabilizează",
            ConfirmationMessage = "Sigur doriți să contabilizați această notă?",
            ImageName = "Action_Grant",
            PaintStyle = ActionItemPaintStyle.CaptionAndImage
        };

        postAction.Execute += PostAction_Execute;
    }

    protected override void OnActivated()
    {
        base.OnActivated();
        postAction.Active["HasObject"] = View.CurrentObject != null;
        UpdateActionState();
    }

    private void PostAction_Execute(object sender, SimpleActionExecuteEventArgs e)
    {
        var entry = (AccountingEntry)View.CurrentObject;

        // Validări
        if (entry.Status != 0)
        {
            throw new UserFriendlyException("Nota este deja contabilizată!");
        }

        if (string.IsNullOrEmpty(entry.DebitAccount) || string.IsNullOrEmpty(entry.CreditAccount))
        {
            throw new UserFriendlyException("Conturile debit și credit sunt obligatorii!");
        }

        // Contabilizare
        entry.Status = 1; // Posted
        entry.OperationDate = DateTime.Now;

        ObjectSpace.CommitChanges();

        Application.ShowViewStrategy.ShowMessage(
            "Nota a fost contabilizată cu succes!",
            InformationType.Success,
            3000,
            InformationType.Success);
    }

    private void UpdateActionState()
    {
        var entry = View.CurrentObject as AccountingEntry;
        postAction.Enabled["NotPosted"] = entry?.Status == 0;
    }
}
```

---

### Popup Window cu parametri → PopupWindowShowAction

**Delphi VCL:**
```pascal
procedure TfrmBalanta.btnGenerateClick(Sender: TObject);
var
  DataStart, DataEnd: TDateTime;
begin
  if SelectDateRange(DataStart, DataEnd) then
  begin
    GenerateReport(DataStart, DataEnd);
  end;
end;
```

**XAF Echivalent:**
```csharp
public class GenerateBalanceReportController : ObjectViewController<DetailView, ChartOfAccounts>
{
    private PopupWindowShowAction generateReportAction;

    public GenerateBalanceReportController()
    {
        generateReportAction = new PopupWindowShowAction(this, "GenerateBalanceReport", "Reports")
        {
            Caption = "Generează Balanță",
            ImageName = "BO_Report"
        };

        generateReportAction.CustomizePopupWindowParams += GenerateReportAction_CustomizePopupWindowParams;
        generateReportAction.Execute += GenerateReportAction_Execute;
    }

    private void GenerateReportAction_CustomizePopupWindowParams(object sender, CustomizePopupWindowParamsEventArgs e)
    {
        // Creează obiect non-persistent pentru parametri
        var parametersObject = ObjectSpace.CreateObject<ReportParameters>();
        parametersObject.StartDate = new DateTime(DateTime.Now.Year, 1, 1);
        parametersObject.EndDate = DateTime.Now;

        var detailView = Application.CreateDetailView(ObjectSpace, parametersObject);
        e.View = detailView;
    }

    private void GenerateReportAction_Execute(object sender, PopupWindowShowActionExecuteEventArgs e)
    {
        var parameters = (ReportParameters)e.PopupWindowViewCurrentObject;
        var account = (ChartOfAccounts)View.CurrentObject;

        // Generare raport
        ShowBalanceReport(account, parameters.StartDate, parameters.EndDate);
    }
}

[DomainComponent] // Non-persistent object
public class ReportParameters
{
    [Required]
    public virtual DateTime StartDate { get; set; }

    [Required]
    public virtual DateTime EndDate { get; set; }

    public virtual bool IncludeSubAccounts { get; set; }
}
```

---

## LAYOUT & CONTAINERE

### cxPageControl + cxTabSheet → TabbedGroup (XAF Layout)

**Delphi VCL:**
```pascal
object cxPageControl1: TcxPageControl
  object cxTabSheet1: TcxTabSheet
    Caption = 'Date Generale'
    // controale...
  end

  object cxTabSheet2: TcxTabSheet
    Caption = 'Clasificare Bugetară'
    // controale...
  end
end
```

**XAF Echivalent:**
```xml
<!-- Model -->
<DetailView Id="AccountingEntry_DetailView">
  <Layout>
    <LayoutGroup Id="Main" Direction="Vertical">
      <TabbedGroup Id="Tabs">
        <LayoutGroup Id="GeneralTab" Caption="Date Generale" Index="0">
          <LayoutItem Id="EntryNumber" />
          <LayoutItem Id="EntryDate" />
          <LayoutItem Id="Description" />
        </LayoutGroup>

        <LayoutGroup Id="BudgetTab" Caption="Clasificare Bugetară" Index="1">
          <LayoutItem Id="FunctionalCode" />
          <LayoutItem Id="EconomicCode" />
          <LayoutItem Id="OrganizationalUnitId" />
        </LayoutGroup>

        <LayoutGroup Id="AccountsTab" Caption="Conturi" Index="2">
          <LayoutItem Id="DebitAccount" />
          <LayoutItem Id="CreditAccount" />
          <LayoutItem Id="DebitCostCenter" />
          <LayoutItem Id="CreditCostCenter" />
        </LayoutGroup>
      </TabbedGroup>
    </LayoutGroup>
  </Layout>
</DetailView>
```

---

### cxGroupBox → LayoutGroup

**Delphi VCL:**
```pascal
object cxGroupBox1: TcxGroupBox
  Caption = 'Solduri Inițiale'
  // controale...
end
```

**XAF Echivalent:**
```xml
<DetailView Id="ChartOfAccounts_DetailView">
  <Layout>
    <LayoutGroup Id="Main">
      <LayoutGroup Id="InitialBalances" Caption="Solduri Inițiale" Direction="Horizontal">
        <LayoutItem Id="InitialDebitBalance" Caption="Debit" />
        <LayoutItem Id="InitialCreditBalance" Caption="Credit" />
      </LayoutGroup>
    </LayoutGroup>
  </Layout>
</DetailView>
```

---

## SPLIT VIEWS

### cxSplitter → Master-Detail View (XAF)

**Delphi VCL:**
```pascal
object pnlMaster: TPanel
  Align = alLeft
  // cxGrid master
end

object cxSplitter1: TcxSplitter
  Align = alLeft
end

object pnlDetail: TPanel
  Align = alClient
  // cxGrid detail
end
```

**XAF Echivalent (Dashboard View):**
```xml
<DashboardView Id="AccountingDashboard">
  <Items>
    <DashboardViewItem Id="ChartOfAccountsListView" ViewId="ChartOfAccounts_ListView" />
    <DashboardViewItem Id="AccountingEntriesListView" ViewId="AccountingEntry_ListView"
                       MasterViewItem="ChartOfAccountsListView"
                       RelationName="ChartOfAccounts-DebitEntries" />
  </Items>
  <Layout Direction="Horizontal">
    <LayoutItem Id="ChartOfAccountsListView" RelativeSize="30" />
    <LayoutItem Id="AccountingEntriesListView" RelativeSize="70" />
  </Layout>
</DashboardView>
```

---

## SUMMARY

| Componentă VCL | XAF Echivalent | Complexitate |
|----------------|----------------|--------------|
| cxGrid | GridListEditor | ⭐ Simplă |
| cxGridDBBandedTableView | BandedGridView (WinForms) | ⭐⭐ Medie |
| cxDBTreeList | TreeListEditor | ⭐⭐ Medie |
| cxDBTextEdit | StringPropertyEditor | ⭐ Simplă |
| cxDBDateEdit | DateTimePropertyEditor | ⭐ Simplă |
| cxDBCurrencyEdit | DecimalPropertyEditor | ⭐ Simplă |
| cxDBLookupComboBox | LookupPropertyEditor | ⭐ Simplă |
| cxButton | SimpleAction | ⭐⭐ Medie |
| cxPageControl | TabbedGroup | ⭐ Simplă |
| cxGroupBox | LayoutGroup | ⭐ Simplă |

**Observații:**
- XAF generează automat majoritatea UI-ului din Business Objects
- Customizările se fac preferabil în Application Model (XML)
- Controllers permit customizări avansate în cod C#
- Blazor are limitări la componente complexe (ex: BandedGrid)
- WinForms oferă acces complet la toate controalele DevExpress
