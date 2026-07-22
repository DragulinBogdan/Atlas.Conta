unit OI_Proiecte;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxLookAndFeelPainters, cxGraphics, cxTL,
  cxMaskEdit, cxImageComboBox, cxClasses, cxInplaceContainer, cxDBTL,
  cxTLData, cxCheckBox, cxDBEdit, cxDropDownEdit, cxButtonEdit, cxTextEdit,
  ExtCtrls, StdCtrls, cxControls, cxContainer, cxEdit, cxGroupBox,
  cxButtons, DB, ZDataSet, cxDataStorage, cxDBData,
  cxGridLevel, cxGridCustomView, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGrid, cxMemo, AppEvnts, Menus,
  cxCurrencyEdit,  cxPC, dxDBCtrl, dxDBTL, cxCalendar,
  ZAbstractRODataset, ZAbstractDataset,
  cxTLdxBarBuiltInMenu, cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData,
  ZSqlUpdate, cxGridCustomPopupMenu, cxGridPopupMenu, cxDBLookupComboBox,
  ADODB, Buttons, dxmdaset, cxSplitter;

type
  TCrackAtsTree = class(TdxDBTreeList);
  TCrackAtsDBTreeList = class(TCustomdxDBTreeListControl);
  TfrmOIProiecte = class(TForm)
    pnContent: TPanel;
    cxGroupBox: TcxGroupBox;
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    Bevel1: TBevel;
    edtDenumire: TcxDBTextEdit;
    edtIdGestTipMaterial: TcxDBTextEdit;
    edtSeAfiseaza: TcxDBCheckBox;
    Panel1: TPanel;
    pnControl: TPanel;
    DTOIProiecte: TDataSource;
    edtDescriere: TcxDBMemo;
    DTOITipuriProiecte: TDataSource;
    cxPageControl: TcxPageControl;
    tabBuget: TcxTabSheet;
    tabContabilitate: TcxTabSheet;
    GridBugetV: TcxGridDBTableView;
    GridBugetLevel: TcxGridLevel;
    GridBuget: TcxGrid;
    DTBuget: TDataSource;
    GridBugetVid: TcxGridDBColumn;
    GridBugetVcod_functional: TcxGridDBColumn;
    GridBugetVcod_economic: TcxGridDBColumn;
    GridBugetVid_bg_versiune: TcxGridDBColumn;
    GridBugetVid_oi_proiecte: TcxGridDBColumn;
    GridBugetVan_fiscal: TcxGridDBColumn;
    GridBugetVrevizie: TcxGridDBColumn;
    GridBugetVplanificat1: TcxGridDBColumn;
    GridBugetVplanificat2: TcxGridDBColumn;
    GridBugetVplanificat3: TcxGridDBColumn;
    GridBugetVplanificat4: TcxGridDBColumn;
    GridBugetVplanificat: TcxGridDBColumn;
    GridBugetVden_functional: TcxGridDBColumn;
    GridBugetVden_economic: TcxGridDBColumn;
    GridConta: TcxGrid;
    GridContaL: TcxGridLevel;
    GridContaV: TcxGridDBTableView;
    GridContaVCONT: TcxGridDBColumn;
    GridContaVSOLD: TcxGridDBColumn;
    GridContaVSOLD_DEBITOR: TcxGridDBColumn;
    GridContaVSOLD_CREDITOR: TcxGridDBColumn;
    DTSolduri: TDataSource;
    tabImplicit: TcxTabSheet;
    GridCFV: TcxGridDBTableView;
    GridCFL: TcxGridLevel;
    GridCF: TcxGrid;
    DTCF: TDataSource;
    GridCFVID_REPARTITORI_BUGET: TcxGridDBColumn;
    GridCFVID_REPARTITORI: TcxGridDBColumn;
    GridCFVCOD_FUNCTIONAL: TcxGridDBColumn;
    GridCFVCOD_ECONOMIC: TcxGridDBColumn;
    GridCFVID_OI_PROIECTE: TcxGridDBColumn;
    GridCFVID_OI_UNITATI: TcxGridDBColumn;
    edtAreContabilitateProprie: TcxDBCheckBox;
    Label11: TLabel;
    cxDBTextEdit1: TcxDBTextEdit;
    pnTop: TPanel;
    btnRefresh: TcxButton;
    edtFiltCodFunctional: TcxButtonEdit;
    Label13: TLabel;
    cxDBCheckBox1: TcxDBCheckBox;
    Label4: TLabel;
    edDataProiect: TcxDBDateEdit;
    cxGridPopupMenu: TcxGridPopupMenu;
    TreeProiecte: TcxDBTreeList;
    TreeProiecteid_oi_proiecte: TcxDBTreeListColumn;
    TreeProiecteDenumire: TcxDBTreeListColumn;
    TreeProiecteid_parinte: TcxDBTreeListColumn;
    TreeProiecteID_OI_TIPURI_PROIECTE: TcxDBTreeListColumn;
    TreeProiectecod_proiect: TcxDBTreeListColumn;
    TreeProiecteSTARE: TcxDBTreeListColumn;
    qryOIProiecte: TADOQuery;
    qryOITipuriProiecte: TADOQuery;
    qryCF: TADOQuery;
    qryBuget: TADOQuery;
    qrySolduri: TADOQuery;
    txtFiltrare: TcxTextEdit;
    lbl1: TLabel;
    btn1: TSpeedButton;
    vwObiecte: TcxGridDBTableView;
    lvObiecte: TcxGridLevel;
    gridObiecte: TcxGrid;
    pnlObiecte: TPanel;
    mdtObiecte: TdxMemData;
    dsObiecte: TDataSource;
    intgrfldObiecteidObiect: TIntegerField;
    strngfldObiecteDenumire: TStringField;
    vwObiecteidObiect: TcxGridDBColumn;
    vwObiecteDenumire: TcxGridDBColumn;
    split1: TcxSplitter;
    pnl1: TPanel;
    btn2: TcxButton;
    pnl2: TPanel;
    btn3: TcxButton;
    btnDelete: TcxButton;
    procedure cxButton3Click(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure zqryOIProiecte11AfterInsert(DataSet: TDataSet);
    procedure FormCloseQuery(Sender: TObject; var CanClose: Boolean);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure GridBugetVcod_functionalGetCellHint(
      Sender: TcxCustomGridTableItem; ARecord: TcxCustomGridRecord;
      ACellViewInfo: TcxGridTableDataCellViewInfo; const AMousePos: TPoint;
      var AHintText: TCaption; var AIsHintMultiLine: Boolean;
      var AHintTextRect: TRect);
    procedure GridBugetVcod_economicGetCellHint(
      Sender: TcxCustomGridTableItem; ARecord: TcxCustomGridRecord;
      ACellViewInfo: TcxGridTableDataCellViewInfo; const AMousePos: TPoint;
      var AHintText: TCaption; var AIsHintMultiLine: Boolean;
      var AHintTextRect: TRect);
    procedure QrySolduriNewRecord(DataSet: TDataSet);
    procedure QrySolduriAfterOpen(DataSet: TDataSet);
    procedure zqryOIProiecte11AfterOpen(DataSet: TDataSet);
    procedure QryCFNewRecord(DataSet: TDataSet);
    procedure edtFiltCodFunctionalPropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
    procedure btnRefreshClick(Sender: TObject);
    procedure pnTopResize(Sender: TObject);
    procedure TreeProiecteFocusedNodeChanged(Sender: TcxCustomTreeList;
      APrevFocusedNode, AFocusedNode: TcxTreeListNode);
    procedure QrySolduriBeforeOpen(DataSet: TDataSet);
    procedure TreeProiecteCustomDrawDataCell(Sender: TcxCustomTreeList;
      ACanvas: TcxCanvas; AViewInfo: TcxTreeListEditCellViewInfo;
      var ADone: Boolean);
    procedure TreeProiecteDragOver(Sender, Source: TObject; X, Y: Integer;
      State: TDragState; var Accept: Boolean);
    procedure TreeProiecteDblClick(Sender: TObject);
    procedure qryOIProiecteFilterRecord(DataSet: TDataSet;
      var Accept: Boolean);
    procedure txtFiltrarePropertiesChange(Sender: TObject);
    procedure btn1Click(Sender: TObject);
    procedure btn3Click(Sender: TObject);
    procedure btnDeleteClick(Sender: TObject);
    procedure btn2Click(Sender: TObject);
  private
    InLoading : Boolean;
    FFilterCodFunctional : String;
    FFilterUnitate : Integer;

{    function InternalPositioning(Val: String; Tree: TdxDBTreeList; ColumnName : String): Boolean; overload;
    function InternalPositioning(Val: String; Tree: TdxDBTreeList): Boolean; overload;
    function InternalPositioning(Val: String;  Sender: TObject): Boolean; overload;
    function InternalPositioning(Val: String; Tree: TcxDBTreeList; ColumnName : String): Boolean; overload;
    function InternalPositioning(Val: String; Tree : TcxDBTreeList) : Boolean; overload;   }
    function NewSelectarePlanFunctional(var aCodFunctional : String; var aIdUnitate : Integer; const NumaiFrunze : Boolean = False; const FaraUnitati : boolean = False) : String;

    function GetTmpADOQuery: TADOQuery;
    function QryCreate: TADOQuery;

    procedure AddProiect(const Parented : Boolean=False);
    procedure SOLDChange(Sender: TField);
    procedure DoRefreshDataSet(aDataSet : TADOQuery);
    procedure DoCheckPostDataSet(aDataSet : TADOQuery);
    procedure ExecuteSQL(SQLText: String);
    { Private declarations }
  public
    { Public declarations }

    procedure RefreshTipProiecte;
    procedure PopulateControls;
  end;

  TCrackCxPopupEdit = class(TcxPopupEdit);

var
  frmOIProiecte: TfrmOIProiecte;

implementation

uses CommonDBVar, MainUnit, DMMain,//DateUnit,
  {PlanConturiUnit,} dxTL, SelBugetUnit, ListaPropuneri, Utils, dataUnit;

{$R *.dfm}

procedure TfrmOIProiecte.cxButton3Click(Sender: TObject);
begin
  //--atentie
 {MainForm.Cmd_OITipuriProiecte.Execute;
  RefreshTipProiecte;
  InternalPositioning(IntToStr(edtTipProiect.Tag), TreeTipProiect);  }
end;

procedure TfrmOIProiecte.RefreshTipProiecte;
begin
  DoRefreshDataSet(qryOITipuriProiecte);
end;

procedure TfrmOIProiecte.PopulateControls;
begin
  RefreshTipProiecte;

  PopulatecxImage(qryOITipuriProiecte, TcxImageComboBoxProperties(TreeProiecteID_OI_TIPURI_PROIECTE.Properties).Items,
  'ID_OI_TIPURI_PROIECTE', 'DENUMIRE' );

end;

procedure TfrmOIProiecte.FormCreate(Sender: TObject);
var
  IarrObiecte : Integer;
begin
  FFilterCodFunctional := '';
  FFilterUnitate := -1;
  InLoading := True;
  PopulateControls;
  DoRefreshDataSet(qryOIProiecte);
  InLoading := False;
  mdtObiecte.Active := True;
  for IarrObiecte := 1 to 20 do
    arrObiecte [ IarrObiecte ] := 0;
end;

procedure TfrmOIProiecte.zqryOIProiecte11AfterInsert(DataSet: TDataSet);
begin
  DataSet.FieldByName('STARE').AsBoolean := True;
end;

procedure TfrmOIProiecte.FormCloseQuery(Sender: TObject;
  var CanClose: Boolean);
begin
  //facem refresh la dataset cu starea 1
  DoCheckPostDataSet(qryOIProiecte);
  DoRefreshDataSet(qryOIProiecte);
end;

procedure TfrmOIProiecte.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
   Action := caFree;
end;

procedure TfrmOIProiecte.GridBugetVcod_functionalGetCellHint(
  Sender: TcxCustomGridTableItem; ARecord: TcxCustomGridRecord;
  ACellViewInfo: TcxGridTableDataCellViewInfo; const AMousePos: TPoint;
  var AHintText: TCaption; var AIsHintMultiLine: Boolean;
  var AHintTextRect: TRect);
begin
  AHintText := ARecord.Values[GridBugetVden_functional.Index];
end;

procedure TfrmOIProiecte.GridBugetVcod_economicGetCellHint(
  Sender: TcxCustomGridTableItem; ARecord: TcxCustomGridRecord;
  ACellViewInfo: TcxGridTableDataCellViewInfo; const AMousePos: TPoint;
  var AHintText: TCaption; var AIsHintMultiLine: Boolean;
  var AHintTextRect: TRect);
begin
  AHintText := ARecord.Values[GridBugetVden_economic.Index];
end;

procedure TfrmOIProiecte.QrySolduriNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('ID_REPARTITORI').AsInteger := qryOIProiecte.FieldByName('ID_OI_PROIECTE').AsInteger;  
end;

procedure TfrmOIProiecte.SOLDChange(Sender: TField);
begin
  Sender.DataSet.FieldByName('SOLD').AsCurrency :=
     Sender.DataSet.FieldByName('SOLD_DEBITOR').AsCurrency -
     Sender.DataSet.FieldByName('SOLD_CREDITOR').AsCurrency;
end;

procedure TfrmOIProiecte.QrySolduriAfterOpen(DataSet: TDataSet);
begin
  DataSet.FieldByName('SOLD_DEBITOR').OnChange := SOLDChange;
  DataSet.FieldByName('SOLD_CREDITOR').OnChange := SOLDChange;
end;

procedure TfrmOIProiecte.AddProiect(const Parented: Boolean);
var
  aId : Integer;
  aQry : TADOQuery;
  ParentId: Variant;
begin
  if not Parented then ParentId := Null
  else begin
    { Adaugam o subfunctie noua in contextul curent
      Daca este organigrama sau tree }
    if TreeProiecte.FocusedNode <> nil then ParentId := qryOIProiecte.FieldByName('ID_OI_PROIECTE').AsInteger
    else ParentId := Null;
  end;

  aQry := GetTmpADOQuery;
  with aQry do
    try
     if ParentId = Null then
       SQL.Add('exec spOIProiecteAdd ''<Proiect Nou>'', NULL')
     else
       SQL.Add('exec spOIProiecteAdd ''<Proiect Nou>'', ' + VarToStr(ParentId));
     Open;
     if not IsEmpty then begin
       aId := Fields[0].AsInteger;
       InLoading := True;
       DoRefreshDataSet(qryOIProiecte);
       InLoading := False;
       qryOIProiecte.Locate('ID_OI_PROIECTE', aId,[]);
     end;
    finally
      Free;
    end;
end;

procedure TfrmOIProiecte.zqryOIProiecte11AfterOpen(DataSet: TDataSet);
begin
  ExecuteSQL('exec spOIProiecteVerifcaDetalii');
 // LocalADOOption(qryOIProiecte);
end;

procedure TfrmOIProiecte.QryCFNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('ID_REPARTITORI').AsInteger := qryOIProiecte.FieldByName('ID_OI_PROIECTE').AsInteger;
end;

procedure TfrmOIProiecte.edtFiltCodFunctionalPropertiesButtonClick(
  Sender: TObject; AButtonIndex: Integer);
var
  lSelectStr : String;
begin
  if AButtonIndex = 0 then begin
    lSelectStr := NewSelectarePlanFunctional(FFilterCodFunctional, FFilterUnitate, True);
    if lSelectStr <> '<Anulat>' then begin
      edtFiltCodFunctional.Text := lSelectStr;
      btnRefreshClick(nil);
    end;
  end
  else begin
    edtFiltCodFunctional.Text := '';
    FFilterCodFunctional := '';
    FFilterUnitate := -1;
    btnRefreshClick(nil);
  end;
end;

procedure TfrmOIProiecte.btnRefreshClick(Sender: TObject);
begin
  DoCheckPostDataSet(qryOIProiecte);
  qryOIProiecte.Close;
  qryOIProiecte.SQL[1] := '';
  if (FFilterCodFunctional <> '') then begin
    qryOIProiecte.SQL[1] := 'WHERE ID_OI_PROIECTE IN (SELECT ID_REPARTITORI FROM REPARTITORI_BUGET WHERE COD_FUNCTIONAL = ''' + FFilterCodFunctional + '''';
    if FFilterUnitate <> -1 then
      qryOIProiecte.SQL[1] := qryOIProiecte.SQL[1]  + ' AND ID_OI_UNITATI = ' + IntToStr(FFilterUnitate);
    qryOIProiecte.SQL[1] := qryOIProiecte.SQL[1] + ')' ;
  end;
  InLoading := True;
 // ShowMessage(qryOIProiecte.SQL.Text);
  qryOIProiecte.Open;
  InLoading := False;
end;

procedure TfrmOIProiecte.pnTopResize(Sender: TObject);
begin
  btnRefresh.Left := pnTop.Width - btnRefresh.Width - 5;
end;

procedure TfrmOIProiecte.TreeProiecteFocusedNodeChanged(
  Sender: TcxCustomTreeList; APrevFocusedNode,
  AFocusedNode: TcxTreeListNode);
var
  lNode : TcxDBTreeListNode;
begin
  if InLoading then Exit;
  if not qryOITipuriProiecte.Active then RefreshTipProiecte;

  qryBuget.Close;
  qryBuget.Parameters.ParamByName('ID_PROIECTE').Value := qryOIProiecte.FieldByName('ID_OI_PROIECTE').AsInteger;
  qryBuget.Open;
  qrySolduri.Close;
  qrySolduri.Parameters.ParamByName('ID_PROIECTE').Value := qryOIProiecte.FieldByName('ID_OI_PROIECTE').AsInteger;
  qrySolduri.Open;
  qryCF.Close;
  qryCF.Parameters.ParamByName('ID_PROIECTE').Value := qryOIProiecte.FieldByName('ID_OI_PROIECTE').AsInteger;
  qryCF.Open;
end;


procedure TfrmOIProiecte.QrySolduriBeforeOpen(DataSet: TDataSet);
begin
  if DataSet.FindField('SOLD_DEBITOR') <> nil then
    DataSet.FieldByName('SOLD_DEBITOR').OnChange := nil;
  if DataSet.FindField('SOLD_CREDITOR') <> nil then
    DataSet.FieldByName('SOLD_CREDITOR').OnChange := nil;
end;

procedure TfrmOIProiecte.TreeProiecteCustomDrawDataCell(
  Sender: TcxCustomTreeList; ACanvas: TcxCanvas;
  AViewInfo: TcxTreeListEditCellViewInfo; var ADone: Boolean);
begin
  if AViewInfo.Node = nil then Exit;
  if (VarToStr(AViewInfo.Node.Values[TreeProiecteSTARE.ItemIndex]) = '') or
    (VarToStr(AViewInfo.Node.Values[TreeProiecteSTARE.ItemIndex])= 'False') then begin
    ACanvas.Font.Style := ACanvas.Font.Style + [fsStrikeOut];
    ACanvas.Font.Color := clRed;
  end
  else begin
    ACanvas.Font.Style := ACanvas.Font.Style - [fsStrikeOut];
    //ACanvas.Font.Color := clBlack;
  end;
end;

procedure TfrmOIProiecte.TreeProiecteDragOver(Sender, Source: TObject; X,
  Y: Integer; State: TDragState; var Accept: Boolean);
begin
//
end;

procedure TfrmOIProiecte.DoRefreshDataSet(aDataSet: TADOQuery);
begin
  aDataSet.DisableControls;
  if aDataSet.Active then aDataSet.Close;
  aDataSet.Active := True;
  aDataSet.EnableControls;
end;

{function TfrmOIProiecte.InternalPositioning(Val: String;
  Tree: TdxDBTreeList; ColumnName: String): Boolean;
var
   aStr : String;
   aNode : TdxTreeListNode;
   aColIndex: Integer;
begin
  Result := False;
  aStr := Val;
  aNode := nil;
  if aStr <> '' then begin
    Tree.TopNode;
    Tree.FullCollapse;
    //deteminam coloana
    if not((Trim(ColumnName) = '') or (UpperCase(Trim(ColumnName)) = UpperCase(Tree.KeyField))) then begin
       if Tree.FindColumnByFieldName(ColumnName) <> nil then aColIndex := Tree.FindColumnByFieldName(ColumnName).Index
       else aColIndex := -1;
    end else aColIndex := -1;

    while (not Assigned(aNode)) and (Length(aStr) > 0) do begin
      if aColIndex <> -1 then
         TCrackAtsDBTreeList(Tree).FindNodeByText(aColIndex, aStr, sdDown, aNode)
      else
         aNode := Tree.FindNodeByKeyValue(aStr);
      if not Assigned(aNode) then Delete(aStr, Length(aStr), 1);
    end;

    if aNode <> nil then begin
      if aNode.Count > 0 then aNode := aNode.Items[0];
      Result := True;
      aNode.Focused := True;
      aNode.MakeVisible;
      if aNode.HasChildren then aNode.Expand(True);
      if aColIndex = -1 then
        Tree.ApplyBestFit(Tree.VisibleColumns[0])
      else
        Tree.ApplyBestFit(Tree.Columns[aColIndex]);
      Tree.StartSearch(-1, aStr);
    end;
  end;
end;

function TfrmOIProiecte.InternalPositioning(Val: String;
  Tree: TdxDBTreeList): Boolean;
begin
  InternalPositioning(Val, Tree, '');
end;

function TfrmOIProiecte.InternalPositioning(Val: String;
  Sender: TObject): Boolean;
var
  aCol : String;
begin
  if (Sender is TdxDBTreeListPopupColumn) and (TdxDBTreeListPopupColumn(Sender).PopupControl is TdxDBTreeList) and
     Assigned(TdxDBTreeListPopupColumn(Sender).PopupControl) then
       InternalPositioning(Val,   TdxDBTreeList(TdxDBTreeListPopupColumn(Sender).PopupControl))
  else
  if (Sender is TdxInspectorTextPopupRow) and (TdxInspectorTextPopupRow(Sender).PopupControl is TdxDBTreeList) and
     Assigned(TdxInspectorTextPopupRow(Sender).PopupControl) then
  begin

      if TObject(TdxInspectorTextPopupRow(Sender).Tag) is TParam then
        aCol := EchivalareCol(TParam(TdxInspectorTextPopupRow(Sender).Tag).Name)
      else
        aCol := '';

      InternalPositioning(Val,   TdxDBTreeList(TdxInspectorTextPopupRow(Sender).PopupControl), aCol);
  end;
end;

function TfrmOIProiecte.InternalPositioning(Val: String;
  Tree: TcxDBTreeList; ColumnName: String): Boolean;
var aStr : String;
   aNode : TcxTreeListNode;
   aColIndex: Integer;
begin
  Result := False;
  aStr := Val;
  if aStr <> '' then begin
    Tree.TopNode;
    Tree.FullCollapse;
    if  not((Trim(ColumnName) = '') or (UpperCase(Trim(ColumnName)) = UpperCase(Tree.DataController.KeyField))) then
      if cxFindColumnByFieldName(Tree, ColumnName) <> nil then begin
        aColIndex := cxFindColumnByFieldName(Tree, ColumnName).ItemIndex;
        aNode := Tree.FindNodeByText(Val, Tree.Columns[aColIndex]);
      end
      else
        aNode := Tree.FindNodeByKeyValue(Val, nil)
    else
      aNode := Tree.FindNodeByKeyValue(Val, nil);
    if aNode <> nil then begin
      Result := True;
      aNode.MakeVisible;
      aNode.Focused := True;
    end;
  end;
end;

function TfrmOIProiecte.InternalPositioning(Val: String;
  Tree: TcxDBTreeList): Boolean;
begin
 Result := InternalPositioning(Val, Tree, '');
end;

procedure TfrmOIProiecte.DoCheckPostDataSet(aDataSet: TZQuery);
begin
  if aDataSet.State in [dsEdit, dsInsert] then begin
      aDataSet.Post;
      aDataSet.Edit;
  end;
end;      }

function TfrmOIProiecte.GetTmpADOQuery: TADOQuery;
begin
  Result := TADOQuery.Create(nil);
  with Result do begin
    Connection := dtmMain.dbContaConnection;
  end;
end;

procedure TfrmOIProiecte.ExecuteSQL(SQLText: String);
begin
  With QryCreate do
  try
    SQL.Text := SQLText;
    ExecSQL;
  finally
    Free;
  end;
end;

function TfrmOIProiecte.NewSelectarePlanFunctional(
  var aCodFunctional: String; var aIdUnitate: Integer; const NumaiFrunze,
  FaraUnitati: boolean): String;
var
  frmPlanBugete : TfrmSelBuget;
  aNode : TcxTreeListNode;
  aId : Integer;
begin
  frmPlanBugete := TfrmSelBuget.Create(nil);
  Result := '';
  with frmPlanBugete do
    try
      frmPlanBugete.Visible := False;
      FOnlyChild := NumaiFrunze;
      Caption := 'Selectie cod functional';
      cxtabFunctional.TabVisible := False;
      cxtabEconomic.TabVisible := False;
      PaginaClasificatii.ActivePageIndex := 0;
      cxTreeFunctional.PopupMenu := nil;
      DoRefreshDataSet(qryPlanFunctional);
      if FaraUnitati then begin
         qryPlanFunctional.Filtered := False;
         qryPlanFunctional.Filter := 'ID_OI_UNITATI <> 0 and ID_OI_UNITATI IS NOT NULL';
         qryPlanFunctional.Filtered := True;
      end
      else begin
         qryPlanFunctional.Filtered := False;
         qryPlanFunctional.Filter := '';
      end;
      if Trim(aCodFunctional) <> '' then begin
        aId := -1;
        if aIdUnitate > 0 then begin
          if qryPlanFunctional.Locate('COD_FUNCTIONAL;ID_OI_UNITATI', VarArrayOf([aCodFunctional, aIdUnitate]), []) then
            aId := qryPlanFunctional.FieldByName('ID_BG_PLAN_FUNCTIONAL').AsInteger;
        end
        else
          if qryPlanFunctional.Locate('COD_FUNCTIONAL', aCodFunctional, []) then
            aId := qryPlanFunctional.FieldByName('ID_BG_PLAN_FUNCTIONAL').AsInteger;
        aNode := cxTreeFunctional.FindNodeByKeyValue(aId, nil);
      end
      else
        aNode := cxTreeFunctional.TopNode;

      if aNode <> nil then begin
        aNode.MakeVisible;
        aNode.Focused := True;
      end;
      ActiveControl := cxTreeFunctional;
      ShowModal;
      if ModalResult = mrOk then begin
        aNode := cxTreeFunctional.FocusedNode;
        if (NumaiFrunze) and (aNode <> nil) and (aNode.HasChildren) then raise EContaHandledError.Create('Codul Functional selectat trebuie sa fie un analitic ! Va rugam refaceti selectia !');
        if (aNode <> nil) then begin
          aCodFunctional := aNode.Texts[cxTreeFunctionalCOD_FUNCTIONAL.ItemIndex];
          if aNode.Texts[cxTreeFunctionalID_ANALITIC.ItemIndex] <> '' then
            aIdUnitate := aNode.Values[cxTreeFunctionalID_ANALITIC.ItemIndex]
          else
            aIdUnitate := -1;
          Result := aNode.Texts[cxTreeFunctionalcod_ecran.ItemIndex];
        end;
      end
      else Result := '<Anulat>';
    finally
      Free;
    end;
end;

function TfrmOIProiecte.QryCreate: TADOQuery;
begin
  Result := TADOQuery.Create(dmModule);
  Result.Connection := dtmMain.dbContaConnection;
end;

procedure TfrmOIProiecte.DoCheckPostDataSet(aDataSet: TADOQuery);
begin
    if aDataSet.State in [dsEdit, dsInsert] then begin
      aDataSet.Post;
      aDataSet.Edit;
  end;
end;

procedure TfrmOIProiecte.TreeProiecteDblClick(Sender: TObject);
begin
 idSelectat := TreeProiecte.DataController.GetValue(TreeProiecte.FocusedNode.Index, 0);
 codInv := TreeProiecte.DataController.GetValue(TreeProiecte.FocusedNode.Index, 1);
 seteazaInv := True;
 selectareObiecte := False;
 Close;
end;

procedure TfrmOIProiecte.qryOIProiecteFilterRecord(DataSet: TDataSet;
  var Accept: Boolean);
begin
 Accept := True;

 if Trim(txtFiltrare.Text) <> '' then
 Accept := Accept and
           (Pos(AnsiLowerCase(txtFiltrare.Text), AnsiLowerCase(qryOIProiecte.FieldByName('Denumire').AsString)) > 0);
end;

procedure TfrmOIProiecte.txtFiltrarePropertiesChange(Sender: TObject);
begin
 qryOIProiecte.DisableControls;
 qryOIProiecte.Filtered := False;
 qryOIProiecte.Filtered := True;
 qryOIProiecte.EnableControls;
end;

procedure TfrmOIProiecte.btn1Click(Sender: TObject);
begin
 txtFiltrare.Text := '';
 qryOIProiecte.Filtered := False;
end;

procedure TfrmOIProiecte.btn3Click(Sender: TObject);
var
  exista: Boolean;
begin
// ShowMessage(IntToStr(TreeProiecte.Count));
 if TreeProiecte.Count > 0 then begin
 mdtObiecte.First;
  while not mdtObiecte.Eof do
   begin
    if mdtObiecte.FieldByName('idObiect').Value = TreeProiecte.DataController.GetValue(TreeProiecte.FocusedNode.Index, 0) then
     exista := True;
     mdtObiecte.Next;
   end;

 if not exista then
  begin
   mdtObiecte.Append;
   mdtObiecte.FieldByName('idObiect').Value := TreeProiecte.DataController.GetValue(TreeProiecte.FocusedNode.Index, 0);
   mdtObiecte.FieldByName('Denumire').Value := TreeProiecte.DataController.GetValue(TreeProiecte.FocusedNode.Index, 2);
   mdtObiecte.Post;
  end;
  end;
end;
                                                                                                          
procedure TfrmOIProiecte.btnDeleteClick(Sender: TObject);
begin
 if not mdtObiecte.IsEmpty then
  mdtObiecte.Delete;
end;

procedure TfrmOIProiecte.btn2Click(Sender: TObject);
var
  i : Integer;
begin
 seteazaInv := True;
 selectareObiecte := True;
 mdtObiecte.First;
 i := 1;
 while ( not mdtObiecte.Eof ) and ( i < 20 ) do
   begin
     arrObiecte[i] := mdtObiecte.FieldByName('idObiect').Value;
     i := i + 1;
     mdtObiecte.Next;
   end;
 Close;
end;

end.
