unit TCVUnitateUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ExtCtrls, dxDBGrid, dxCntner, dxTL, dxDBCtrl, dxDBTL, Db, ZDataSet,
  dxDBTLCl, dxGrClms, Menus, dxExEdtr, dxfCheckBox, dxEditor,
  dxEdLib, StdCtrls,
  ZAbstractRODataset, ZAbstractDataset, cxGraphics, cxControls,
  cxLookAndFeels, cxLookAndFeelPainters, cxFilterControl, cxDBFilterControl,
  cxCustomData, cxStyles, cxTL, cxTLdxBarBuiltInMenu, cxInplaceContainer,
  cxTLData, cxDBTL, cxMaskEdit, cxPC, cxClasses, cxEdit, cxCustomPivotGrid,
  cxPivotGrid, cxDBPivotGrid, dxBarBuiltInMenu,
  cxDataControllerConditionalFormattingRulesManagerDialog, cxFilter, cxData,
  cxDataStorage, cxNavigator, dxDateRanges, cxDBData, cxCurrencyEdit,
  cxGridCustomTableView, cxGridTableView, cxGridBandedTableView,
  cxGridDBBandedTableView, cxGridCustomView, cxGridLevel, cxGrid, cxContainer,
  cxTextEdit, cxDropDownEdit, cxImageComboBox, cxCheckBox, cxButtonEdit,
  dxScrollbarAnnotations;

const
  WM_UPDATE_VISIBLE_COLUMNS = WM_USER + 1;

type
  TfrmSituatieUnitate = class(TForm)
    pnBottom: TPanel;
    Panel2: TPanel;
    pnDetaliuMaterial: TPanel;
    splitterH: TSplitter;
    pnGestiuni: TPanel;
    splitterV: TSplitter;
    pnListaStock: TPanel;
    treeRepartitori: TcxDBTreeList;
    GridIstoricMaterial: TdxDBGrid;
    DTRepartitori: TDataSource;
    DTStocuri: TDataSource;
    DTDocumente: TDataSource;
    QryRepartitori: TZReadOnlyQuery;
    QryStocuri: TZReadOnlyQuery;
    QryDocument: TZReadOnlyQuery;
    GridIstoricMaterialPREDATOR: TdxDBGridMaskColumn;
    GridIstoricMaterialPRIMITOR: TdxDBGridMaskColumn;
    GridIstoricMaterialCOD_DOCUM: TdxDBGridMaskColumn;
    GridIstoricMaterialNR_DOCUM: TdxDBGridMaskColumn;
    GridIstoricMaterialDATA_DOCUM: TdxDBGridDateColumn;
    GridIstoricMaterialOPERATOR: TdxDBGridMaskColumn;
    GridIstoricMaterialCANTITATE: TdxDBGridMaskColumn;
    GridIstoricMaterialCANTITATE_BEFORE: TdxDBGridMaskColumn;
    GridIstoricMaterialCANTITATE_AFTER: TdxDBGridMaskColumn;
    GridIstoricMaterialTIP_MATERIAL: TdxDBGridMaskColumn;
    GridIstoricMaterialSEMN: TdxDBGridImageColumn;
    GridIstoricMaterialVALOARE: TdxDBGridCurrencyColumn;
    GridIstoricMaterialPRET_UNITAR: TdxDBGridCurrencyColumn;
    ppStocuri: TPopupMenu;
    ppFisaMaterial: TMenuItem;
    chkCuMiscari: TcxCheckBox;
    edtTipStoc: TcxImageComboBox;
    Label1: TLabel;
    Label2: TLabel;
    edtCont: TcxButtonEdit;
    treeRepartitoriNUME: TcxDBTreeListColumn;
    pageStocuri: TcxPageControl;
    tabStocuriClasic: TcxTabSheet;
    tabStocuriPivot: TcxTabSheet;
    qryStocuriPivot: TZReadOnlyQuery;
    dtStocuriPivot: TDataSource;
    pivotStocuri: TcxDBPivotGrid;
    pivotStocuriid_gest_tip_stoc: TcxDBPivotGridField;
    pivotStocuricodmat: TcxDBPivotGridField;
    pivotStocurigestiune: TcxDBPivotGridField;
    pivotStocurigestint: TcxDBPivotGridField;
    pivotStocurigest_predator: TcxDBPivotGridField;
    pivotStocurigest_primitor: TcxDBPivotGridField;
    pivotStocuriprodus: TcxDBPivotGridField;
    pivotStocuricod_docum: TcxDBPivotGridField;
    pivotStocurinr_docum: TcxDBPivotGridField;
    pivotStocuridata_docum: TcxDBPivotGridField;
    pivotStocuritipmat: TcxDBPivotGridField;
    pivotStocuridenmat: TcxDBPivotGridField;
    pivotStocurium: TcxDBPivotGridField;
    pivotStocuripret_unitar: TcxDBPivotGridField;
    pivotStocuristock: TcxDBPivotGridField;
    pivotStocuristockValoric: TcxDBPivotGridField;
    pivotStocuricont: TcxDBPivotGridField;
    gridStocuri: TcxGrid;
    nivelStocuri: TcxGridLevel;
    viewStocuri: TcxGridDBBandedTableView;
    viewStocuriCODMAT: TcxGridDBBandedColumn;
    viewStocuriTIPMAT: TcxGridDBBandedColumn;
    viewStocuriDENMAT: TcxGridDBBandedColumn;
    viewStocuriUM: TcxGridDBBandedColumn;
    viewStocuriCANTITATE: TcxGridDBBandedColumn;
    viewStocuriPRET_UNITAR: TcxGridDBBandedColumn;
    viewStocuriPRET_RECEPTIE: TcxGridDBBandedColumn;
    viewStocuriPRET_RECEPTIE_TVA: TcxGridDBBandedColumn;
    procedure QryStocuriAfterOpen(DataSet: TDataSet);
    procedure FormCreate(Sender: TObject);
    procedure ppFisaMaterialClick(Sender: TObject);
    procedure edtTipStocChange(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure CreateReportContext;
    procedure ReportClick(Sender: TObject);
    procedure TreeRepartitoriCustomDrawCell(Sender: TObject;
      ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
      AColumn: TdxTreeListColumn; ASelected, AFocused,
      ANewItemRow: Boolean; var AText: String; var AColor: TColor;
      AFont: TFont; var AAlignment: TAlignment; var ADone: Boolean);
    procedure treeRepartitoriNodeCheckChanged(Sender: TcxCustomTreeList;
      ANode: TcxTreeListNode; AState: TcxCheckBoxState);
    procedure pageStocuriChange(Sender: TObject);
    procedure chkCuMiscariPropertiesChange(Sender: TObject);
    procedure edtContPropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
    procedure QryRepartitoriAfterOpen(DataSet: TDataSet);
    procedure viewStocuriDataControllerFilterRecord(
      ADataController: TcxCustomDataController; ARecordIndex: Integer;
      var Accept: Boolean);
  private
    FIsFilterHandled: Boolean;
    { Private declarations }
    procedure RefreshStocuri;
    procedure UpdateFilterStock;
    procedure PopulateRepartitori;
    procedure LoadReport(const aReportID: Integer);
    function IsGestField(const aFieldName: String): Boolean; overload;
    function IsGestField(const aFieldName: String; out aGestID: Integer): Boolean; overload;
  protected
    procedure WmUpdateVisibleColumns(var Message: TMessage); message WM_UPDATE_VISIBLE_COLUMNS;
  public
    { Public declarations }
  end;

implementation

{$R *.DFM}

uses
  DateUnit, SetParamsUnitADO, rapInclude, PlanConturiUnit, ZeosDBUtile, Variants, dxCompsUtile;

procedure TfrmSituatieUnitate.QryRepartitoriAfterOpen(DataSet: TDataSet);

  procedure SetCheckedNode(ANode: TcxTreeListNode);
  var
    I: Integer;
  begin
    ANode.CheckGroupType := ncgCheckGroup;
    ANode.CheckState     := cbsChecked;
    for I := 0 to ANode.Count-1 do
      SetCheckedNode(ANode.Items[I]);
  end;

begin
  treeRepartitori.BeginUpdate;
  try
    treeRepartitori.OnNodeCheckChanged := nil;
    try
      SetCheckedNode(TreeRepartitori.Root);
    finally
      treeRepartitori.OnNodeCheckChanged := treeRepartitoriNodeCheckChanged;
    end;
  finally
    treeRepartitori.EndUpdate;
  end;
end;

procedure TfrmSituatieUnitate.QryStocuriAfterOpen(DataSet: TDataSet);

  function GetCaptionFromName(const aFieldName: String): String;
  var
    lNode   : TcxDBTreeListNode;
    lGestID : Integer;
  begin
    Result := aFieldName;
    if IsGestField(aFieldName, lGestID) then begin
      lNode := treeRepartitori.FindNodeByKeyValue(lGestID);
      if Assigned(lNode) then Result := ValueSafeToStr(lNode.Values[0]);
    end;
   end;

var
  I           : Integer;
  lIsGestField: Boolean;
  lColumn     : TcxGridDBBandedColumn;

begin
  PopulateRepartitori;
  {Trebuie sa adaugam restul de campuri si sa le incarcam pe cele vizibile din banda 0}
  viewStocuri.BeginUpdate;
  { Citim codurile gestiunilor }
  try
    DTStocuri.DataSet := nil;

    for I := viewStocuri.ColumnCount - 1 downto 0 do
       if viewStocuri.Columns[I].Position.BandIndex = 1 then
          viewStocuri.Columns[I].Free;

    for I := 0 to DataSet.FieldCount-1 do begin
      lColumn := viewStocuri.GetColumnByFieldName(DataSet.Fields[I].FieldName);
      lIsGestField := IsGestField(DataSet.Fields[I].FieldName);
      if not Assigned(lColumn) then begin
        lColumn := viewStocuri.CreateColumn;
        lColumn.HeaderAlignmentHorz := taCenter;
        lColumn.DataBinding.FieldName := DataSet.Fields[I].FieldName;
        lColumn.Position.BandIndex    := 1;
        lColumn.Caption               := GetCaptionFromName(DataSet.Fields[I].FieldName);
        lColumn.Visible               := lIsGestField;
        if lIsGestField then begin
          lColumn.PropertiesClass := TcxCurrencyEditProperties;
          TcxCurrencyEditProperties(lColumn.Properties).DisplayFormat := ',0.00;-,0.00';
        end;
      end
      else
      if lIsGestField then
        lColumn.Visible := True;
    end;
  finally
     DTStocuri.DataSet := QryStocuri;
     viewStocuri.EndUpdate;
  end;

  viewStocuri.ApplyBestFit(nil);

  FIsFilterHandled := False;
end;

procedure TfrmSituatieUnitate.FormCreate(Sender: TObject);
begin
  FIsFilterHandled := False;
  CreateReportContext;
  FillImageCombo(edtTipStoc.Properties, 'select id_gest_tip_stoc, denumire from gest_tip_stoc', 0, 1);
  edtCont.EditValue := '302';
  if edtTipStoc.Properties.Items.Count > 0 then
    edtTipStoc.EditValue := edtTipStoc.Properties.Items[0].Value
  else
    RefreshStocuri;
end;

function TfrmSituatieUnitate.IsGestField(const aFieldName: String;
  out aGestID: Integer): Boolean;
begin
  aGestID := 0;
  Result := (pos('GEST_', UpperCase(aFieldName)) = 1) and TryStrToInt(Copy(aFieldName, 6, Length(aFieldName)), aGestID);
end;

procedure TfrmSituatieUnitate.LoadReport(const aReportID: Integer);
var
  lRecord: TcxCustomGridRecord;
begin
  lRecord := viewStocuri.Controller.FocusedRecord;
  if Assigned(lRecord) and lRecord.IsData then begin
    RegisterCRAdoParam('COD_MAT', ftInteger, True).Value  := lRecord.Values[viewStocuriCODMAT.Index];
    RegisterCRAdoParam('CODMAT', ftInteger, True).Value   := lRecord.Values[viewStocuriCODMAT.Index];
    RegisterCRAdoParam('ID_GEST_TIP_STOC', ftInteger, True).Value := edtTipStoc.EditValue;
    LoadReport(aReportID);
  end
  else
    MessageDlg('Selectati o pozitie din stoc pentru care doriti fisa de material !', mtError, [mbOk], 0);
end;

function TfrmSituatieUnitate.IsGestField(const aFieldName: String): Boolean;
begin
  Result := pos('GEST_', UpperCase(aFieldName)) = 1;
end;

procedure TfrmSituatieUnitate.ppFisaMaterialClick(Sender: TObject);
begin
  LoadReport(DateUnit.GetItemId('FisaMaterial'));
end;

procedure TfrmSituatieUnitate.edtTipStocChange(Sender: TObject);
begin
  RefreshStocuri;
end;

procedure TfrmSituatieUnitate.PopulateRepartitori;
var
  lGestList : TStringList;
  I         : Integer;
  lFieldName: String;
begin
  lGestList := TStringList.Create;
  try
    for I := 0 to QryStocuri.FieldCount-1 do begin
      lFieldName := UpperCase(QryStocuri.Fields[I].FieldName);
      if pos('GEST_', lFieldName) = 1 then
        lGestList.Add(Copy(lFieldName, 6, Length(lFieldName)));
    end;
    QryRepartitori.Close;
    QryRepartitori.SQL.Text := 'select id_repartitori, nume, id_parinte = convert(int, null) from repartitori ';
    if lGestList.Count > 0 then
      QryRepartitori.SQL.Add('where id_repartitori in (' + lGestList.CommaText + ')')
    else
      QryRepartitori.SQL.Add('where 1 = 0');
    DBRefresh(QryRepartitori);
  finally
    lGestList.Free;
  end;
end;

procedure TfrmSituatieUnitate.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmSituatieUnitate.edtContPropertiesButtonClick(Sender: TObject;
  AButtonIndex: Integer);
var
  lCont : String;
begin
  lCont := edtTipStoc.EditText;
  if SelectareContPlan(lCont, False) then begin
    edtCont.EditValue := lCont;
    RefreshStocuri;
  end;
end;

procedure TfrmSituatieUnitate.RefreshStocuri;
begin
  qryStocuriPivot.Close;
  QryStocuri.Close;
  DBRefreshParams(QryStocuri, ['TIP_STOC', 'CU_MISCARI', 'CONT'], [edtTipStoc.EditValue, chkCuMiscari.EditValue, edtCont.EditValue], False);
  DBRefreshParams(qryStocuriPivot, ['refTipStock', 'CONT'], [edtTipStoc.EditValue, ValueSafeToStr(edtCont.EditValue) + '%'], False);
  pageStocuriChange(pageStocuri);
end;

procedure TfrmSituatieUnitate.chkCuMiscariPropertiesChange(Sender: TObject);
begin
  RefreshStocuri;
end;

procedure TfrmSituatieUnitate.CreateReportContext;
var
  lDataSet  : TDataSet;
  lItem     : TMenuItem;
begin
  ppStocuri.Items.Clear;
  lDataSet := DBNewQuery('exec [SP_GEST_RAPOARTE]');
  try
    lDataSet.Open;
    while not lDataSet.Eof do begin
      lItem := NewItem(lDataSet['DENUMIRE'], 0, False, True, ReportClick, 0, '');
      lItem.Tag := lDataSet['ITEM_ID'];
      ppStocuri.Items.Add(lItem);
      lDataSet.Next;
    end;
  finally
    lDataSet.Free;
  end;
end;

procedure TfrmSituatieUnitate.ReportClick(Sender: TObject);
begin
  LoadReport(TMenuItem(Sender).Tag);
end;

procedure TfrmSituatieUnitate.TreeRepartitoriCustomDrawCell(
  Sender: TObject; ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  AColumn: TdxTreeListColumn; ASelected, AFocused, ANewItemRow: Boolean;
  var AText: String; var AColor: TColor; AFont: TFont;
  var AAlignment: TAlignment; var ADone: Boolean);
var
  lFieldName : String;
begin
  lFieldName := 'GEST_' + ValueToStr(TcxDBTreeListNode(ANode).KeyValue);
  if viewStocuri.GetColumnByFieldName(lFieldName) <> nil then begin
    AFont.Style := AFont.Style + [fsBold];
    AColor      := clBtnFace;
    ADone       := True;
  end;
end;

procedure TfrmSituatieUnitate.treeRepartitoriNodeCheckChanged(
  Sender: TcxCustomTreeList; ANode: TcxTreeListNode;
  AState: TcxCheckBoxState);
var
  lColumn: TcxGridDBBandedColumn;
  lGestID: Integer;
begin
  lGestID := Integer(TcxDBTreeListNode(ANode).KeyValue);
  lColumn := viewStocuri.GetColumnByFieldName('GEST_'+IntToStr(lGestID));
  if Assigned(lColumn) then begin
    lColumn.Visible := AState = cbsChecked;
    UpdateFilterStock;
  end;
end;

procedure TfrmSituatieUnitate.UpdateFilterStock;

  function GetFilterList: String;
  var
    lColumn     : TcxGridDBBandedColumn;
    I           : Integer;
  begin
    Result := '';
    for I := 0 to viewStocuri.Bands[1].ColumnCount-1 do begin
      lColumn := TcxGridDBBandedColumn(viewStocuri.Bands[1].Columns[I]);
      if IsGestField(lColumn.DataBinding.FieldName) and lColumn.Visible then begin
        if Result > '' then Result := Result + ' or ';        
        Result := Result + lColumn.DataBinding.FieldName + ' <> 0';
      end;
    end;
  end;

begin
  QryStocuri.DisableControls;
  try
    QryStocuri.Filtered := False;
    QryStocuri.Filter   := GetFilterList;
    QryStocuri.Filtered := QryStocuri.Filter > '';
  finally
    QryStocuri.EnableControls;
  end;
end;

procedure TfrmSituatieUnitate.viewStocuriDataControllerFilterRecord(
  ADataController: TcxCustomDataController; ARecordIndex: Integer;
  var Accept: Boolean);
begin
  if not FIsFilterHandled then begin
    FIsFilterHandled := True;
    PostMessage(Handle, WM_UPDATE_VISIBLE_COLUMNS, 0, 0);
  end;
end;

procedure TfrmSituatieUnitate.WmUpdateVisibleColumns(var Message: TMessage);

  function ViewHasColumnData(AColumn: TcxGridDBBandedColumn): Boolean;
  var
    I: Integer;
  begin
    for I := 0 to viewStocuri.DataController.FilteredRecordCount-1 do begin
      Result := ValueHasValue( viewStocuri.DataController.Values[viewStocuri.DataController.FilteredRecordIndex[I], AColumn.Index] );
      if Result then Break;
    end;
  end;

var
  I: Integer;
  lColumn: TcxGridDBBandedColumn;
begin
  viewStocuri.BeginUpdate();
  try
    for I := 0 to viewStocuri.Bands[1].ColumnCount-1 do begin
      lColumn := TcxGridDBBandedColumn(viewStocuri.Bands[1].Columns[I]);
      if IsGestField(lColumn.DataBinding.FieldName) then
        lColumn.Visible := ViewHasColumnData(lColumn);
    end;
  finally
    viewStocuri.EndUpdate;
  end;
  FIsFilterHandled := False;
end;

procedure TfrmSituatieUnitate.pageStocuriChange(Sender: TObject);
begin
  if pageStocuri.ActivePage = tabStocuriClasic then
    DBRefresh(QryStocuri)
  else
  if pageStocuri.ActivePage = tabStocuriPivot then
    DBRefresh(qryStocuriPivot);
end;

end.
