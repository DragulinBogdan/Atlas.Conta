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
  cxCurrencyEdit, cxPC, cxCalendar, ZAbstractRODataset, ZAbstractDataset,
  cxTLdxBarBuiltInMenu, cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData,
  ZSqlUpdate, cxGridCustomPopupMenu, cxGridPopupMenu, cxDBLookupComboBox,
  cxSpinEdit, dxBarBuiltInMenu, cxNavigator,
  cxDataControllerConditionalFormattingRulesManagerDialog,
  dxScrollbarAnnotations, dxDateRanges, unit_AutoClientForm;

type
  TfrmOIProiecte = class(TParentedForm)
    pnContent: TcxGroupBox;
    cxGroupBox: TcxGroupBox;
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    Label8: TLabel;
    Bevel1: TBevel;
    edtDenumire: TcxDBTextEdit;
    edtIdGestTipMaterial: TcxDBTextEdit;
    edtSeAfiseaza: TcxDBCheckBox;
    Panel1: TcxGroupBox;
    pnControl: TcxGroupBox;
    btnAddProiect: TcxButton;
    btnDelTipMat: TcxButton;
    DTOIProiecte: TDataSource;
    qryOIProiecte: TZQuery;
    edtDescriere: TcxDBMemo;
    edtTipProiect: TcxPopupEdit;
    pnTipProiect: TcxGroupBox;
    Bevel3: TBevel;
    btnCancelTipProiect: TcxButton;
    btnOkTipProiect: TcxButton;
    cxButton3: TcxButton;
    TreeTipProiect: TcxDBTreeList;
    DTOITipuriProiecte: TDataSource;
    qryOITipuriProiecte: TZQuery;
    TreeTipProiectID_OI_TIPURI_PROIECTE: TcxDBTreeListColumn;
    TreeTipProiectDENUMIRE: TcxDBTreeListColumn;
    TreeTipProiectID_PARINTE: TcxDBTreeListColumn;
    cxPageControl: TcxPageControl;
    tabBuget: TcxTabSheet;
    tabContabilitate: TcxTabSheet;
    GridBugetV: TcxGridDBTableView;
    GridBugetLevel: TcxGridLevel;
    GridBuget: TcxGrid;
    DTBuget: TDataSource;
    qryBuget: TZQuery;
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
    edtCont: TcxDBButtonEdit;
    Label6: TLabel;
    edtSoldDebit: TcxDBCurrencyEdit;
    edtSoldCredit: TcxDBCurrencyEdit;
    Label9: TLabel;
    Label10: TLabel;
    cxButton1: TcxButton;
    cxButton2: TcxButton;
    cxButton4: TcxButton;
    DTSolduri: TDataSource;
    QrySolduri: TZQuery;
    tabImplicit: TcxTabSheet;
    btnCFAdd: TcxButton;
    btnCFDel: TcxButton;
    btnCFUpd: TcxButton;
    edCE: TcxDBButtonEdit;
    edCF: TcxDBButtonEdit;
    Label5: TLabel;
    Label7: TLabel;
    GridCFV: TcxGridDBTableView;
    GridCFL: TcxGridLevel;
    GridCF: TcxGrid;
    DTCF: TDataSource;
    QryCF: TZQuery;
    GridCFVID_REPARTITORI_BUGET: TcxGridDBColumn;
    GridCFVID_REPARTITORI: TcxGridDBColumn;
    GridCFVCOD_FUNCTIONAL: TcxGridDBColumn;
    GridCFVCOD_ECONOMIC: TcxGridDBColumn;
    GridCFVID_OI_PROIECTE: TcxGridDBColumn;
    GridCFVID_OI_UNITATI: TcxGridDBColumn;
    btnPlanificare: TcxButton;
    edtAreContabilitateProprie: TcxDBCheckBox;
    Label11: TLabel;
    cxDBTextEdit1: TcxDBTextEdit;
    pnTop: TcxGroupBox;
    btnRefresh: TcxButton;
    edtFiltCodFunctional: TcxButtonEdit;
    Label13: TLabel;
    cxDBCheckBox1: TcxDBCheckBox;
    Label4: TLabel;
    edDataProiect: TcxDBDateEdit;
    usOIProiecte: TZUpdateSQL;
    cxGridPopupMenu: TcxGridPopupMenu;
    TreeProiecte: TcxDBTreeList;
    TreeProiecteid_oi_proiecte: TcxDBTreeListColumn;
    TreeProiecteDenumire: TcxDBTreeListColumn;
    TreeProiecteid_parinte: TcxDBTreeListColumn;
    TreeProiecteID_OI_TIPURI_PROIECTE: TcxDBTreeListColumn;
    TreeProiectecod_proiect: TcxDBTreeListColumn;
    ckDragDrop: TcxCheckBox;
    TreeProiecteSTARE: TcxDBTreeListColumn;
    btnAddSubproiect: TcxButton;
    chkProcentual: TcxDBCheckBox;
    GridCFVPROCENT: TcxGridDBColumn;
    cxDBButtonEdit1: TcxDBButtonEdit;
    Label12: TLabel;
    cxDBButtonEdit2: TcxDBButtonEdit;
    Label14: TLabel;
    GridContaVCOD_FUNCTIONAL: TcxGridDBColumn;
    GridContaVCOD_ECONOMIC: TcxGridDBColumn;
    pnBotomSelect: TcxGroupBox;
    btnOkSelect: TcxButton;
    btnCancelSelect: TcxButton;
    GridCFVIBAN: TcxGridDBColumn;
    GridCFVcodAngajament: TcxGridDBColumn;
    GridCFVcodProgram: TcxGridDBColumn;
    GridCFVcodIndicator: TcxGridDBColumn;
    procedure TreeTipProiectDblClick(Sender: TObject);
    procedure btnOkTipProiectClick(Sender: TObject);
    procedure btnCancelTipProiectClick(Sender: TObject);
    procedure cxButton3Click(Sender: TObject);
    procedure edtTipProiectPropertiesInitPopup(Sender: TObject);
    procedure edtTipProiectPropertiesCloseQuery(Sender: TObject;
      var CanClose: Boolean);
    procedure TreeTipProiectChange(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure btnAddProiectClick(Sender: TObject);
    procedure btnDelTipMatClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure edtContPropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
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
    procedure cxButton1Click(Sender: TObject);
    procedure cxButton2Click(Sender: TObject);
    procedure cxButton4Click(Sender: TObject);
    procedure QrySolduriNewRecord(DataSet: TDataSet);
    procedure QrySolduriAfterOpen(DataSet: TDataSet);
    procedure btnCFAddClick(Sender: TObject);
    procedure btnCFDelClick(Sender: TObject);
    procedure btnCFUpdClick(Sender: TObject);
    procedure edCFPropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
    procedure edCEPropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
    procedure btnPlanificareClick(Sender: TObject);
    procedure qryOIProiecteAfterOpen(DataSet: TDataSet);
    procedure QryCFNewRecord(DataSet: TDataSet);
    procedure edtSoldDebitEnter(Sender: TObject);
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
    procedure ckDragDropPropertiesEditValueChanged(Sender: TObject);
    procedure TreeProiecteDragOver(Sender, Source: TObject; X, Y: Integer;
      State: TDragState; var Accept: Boolean);
    procedure btnAddSubproiectClick(Sender: TObject);
    procedure chkProcentualPropertiesEditValueChanged(Sender: TObject);
    procedure btnOkSelectClick(Sender: TObject);
    procedure btnCancelSelectClick(Sender: TObject);
    procedure qryOIProiecteNewRecord(DataSet: TDataSet);
  private
    FFilterCodFunctional : String;
    FFilterUnitate : Integer;
    FIsReadOnly: Boolean;
    procedure AddProiect(const Parented : Boolean=False);
    procedure SOLDChange(Sender: TField);
    procedure SetIsReadOnly(const Value: Boolean);
    function GetSelectedProject: Variant;
    { Private declarations }
  public
    { Public declarations }

    procedure RefreshTipProiecte;
    procedure PopulateControls;
  public
    property IsReadOnly: Boolean read FIsReadOnly write SetIsReadOnly;
    property SelectedProject: Variant read GetSelectedProject;
  end;

  TCrackCxPopupEdit = class(TcxPopupEdit);

function SelectProject(var AProjectID: Variant): Boolean;

implementation

uses
  dxCompsUtile, ZeosDBUtile, CommonDBVar, MainUnit, PlanConturiUnit, dxTL, SelBugetUnit;

{$R *.dfm}

function SelectProject(var AProjectID: Variant): Boolean;
var
  lFrmOIProiect: TfrmOIProiecte;
begin
  lFrmOIProiect := TfrmOIProiecte.Create(Application);
  try
    lFrmOIProiect.IsReadOnly  := True;
    lFrmOIProiect.Position    := poMainFormCenter;
    Result := lFrmOIProiect.ShowModal = mrOk;
    if Result then
      AProjectID := lFrmOIProiect.SelectedProject
    else
      AProjectID := Null;
  finally
    lFrmOIProiect.Free;
  end;
end;

procedure TfrmOIProiecte.TreeTipProiectDblClick(Sender: TObject);
begin
 if Assigned(Sender) and (Sender is TcxDBTreeList) then
  with TcxDBTreeList(Sender) do begin
    if (FocusedNode <> nil) and not(FocusedNode.HasChildren) then
       (GetParentForm(pnTipProiect) as TcxCustomEditPopupWindow).ModalResult := mrOk;

  end;
end;

procedure TfrmOIProiecte.btnOkSelectClick(Sender: TObject);
begin
  DBCommitUpdates(qryOIProiecte);
  ModalResult := mrOk;
  Close;
end;

procedure TfrmOIProiecte.btnOkTipProiectClick(Sender: TObject);
begin
   (GetParentForm(pnTipProiect) as TcxCustomEditPopupWindow).ModalResult := mrOk;
end;

procedure TfrmOIProiecte.btnCancelSelectClick(Sender: TObject);
begin
  ModalResult := mrCancel;
  Close;
end;

procedure TfrmOIProiecte.btnCancelTipProiectClick(Sender: TObject);
begin
  (GetParentForm(pnTipProiect) as TcxCustomEditPopupWindow).ModalResult := mrCancel;
//  (GetParentForm(pnTipProiect) as TcxCustomEditPopupWindow).CloseUp;
end;

procedure TfrmOIProiecte.cxButton3Click(Sender: TObject);
begin
  MainForm.Cmd_OITipuriProiecte.Execute;
  RefreshTipProiecte;
  InternalPositioning(IntToStr(edtTipProiect.Tag), TreeTipProiect);
end;

procedure TfrmOIProiecte.RefreshTipProiecte;
begin
  DBRefresh(qryOITipuriProiecte);
end;

procedure TfrmOIProiecte.edtTipProiectPropertiesInitPopup(Sender: TObject);
begin
  btnOkTipProiect.Enabled := False;
  RefreshTipProiecte;
  InternalPositioning(IntToStr(edtTipProiect.Tag), TreeTipProiect);
  if edtTipProiect.Properties.PopupWidth < edtTipProiect.Width then
    edtTipProiect.Properties.PopupWidth := edtTipProiect.Width;
end;

type
  TAccesscxPopupEdit = class(TcxPopupEdit);

procedure TfrmOIProiecte.edtTipProiectPropertiesCloseQuery(Sender: TObject;
  var CanClose: Boolean);
var
  lNode : TcxDBTreeListNode;
  lEditabil : Boolean;
begin
  with TAccesscxPopupEdit(Sender) do
    if PopupWindow.ModalResult = mrOk then begin
       lNode := TcxDBTreeListNode(TreeTipProiect.FocusedNode);
       if Assigned(lNode) then begin
          lEditabil := qryOIProiecte.State in [dsEdit, dsInsert];
          if not lEditabil then qryOIProiecte.Edit;
          qryOIProiecte.FieldByName('ID_OI_TIPURI_PROIECTE').Value := lNode.KeyValue;
          qryOIProiecte.Post;
          if lEditabil then qryOIProiecte.Edit;
          Text := VarToStr(lNode.Values[TreeTipProiectDENUMIRE.ItemIndex]);
       end;
    end;
end;

procedure TfrmOIProiecte.TreeTipProiectChange(Sender: TObject);
begin
  btnOkTipProiect.Enabled := (TreeTipProiect.FocusedNode <> nil) and not(TreeTipProiect.FocusedNode.HasChildren);
end;

procedure TfrmOIProiecte.PopulateControls;
begin
  RefreshTipProiecte;
  FillImageCombo(TreeProiecteID_OI_TIPURI_PROIECTE.Properties, qryOITipuriProiecte, 'ID_OI_TIPURI_PROIECTE', 'DENUMIRE' );
end;

procedure TfrmOIProiecte.FormCreate(Sender: TObject);
begin
  FFilterCodFunctional := '';
  FFilterUnitate := -1;
  PopulateControls;
  DBRefresh(qryOIProiecte);
  cxCreateMissingColumns(qryOIProiecte, TreeProiecte);
end;

procedure TfrmOIProiecte.btnAddProiectClick(Sender: TObject);
begin
  if not qryOIProiecte.Locate('DENUMIRE', '<Proiect Nou>', []) then
    AddProiect;
  qryOIProiecte.Refresh;
  qryOIProiecte.Last;
end;

procedure TfrmOIProiecte.btnDelTipMatClick(Sender: TObject);
var
  lHaveReps : Boolean;
  lIdRep : Integer;
begin
  lHaveReps := False;
  lIdRep := qryOIProiecte.FieldByName('ID_OI_PROIECTE').AsInteger;
  if (lIdRep > 0) and DBProcExists('SP_VERIFICARE_REPARTITOR') then
    lHaveReps := ValueSafeToInt( DBGetScallarFmt('exec [SP_VERIFICARE_REPARTITOR] %d', [lIdRep]) ) > 0;
  if lHaveReps then begin
    if MessageDlg('Repartitorul/Proiectul este deja folosit in cadrul aplicatiei !'#13#10'Doriti totusi stergerea lui?',
                  mtConfirmation, [mbYes, mbNo], 0) <> mrYes then Abort;
  end
  else
    if MessageDlg('Doriti stergerea proiectului curent ?',
                  mtConfirmation, [mbYes, mbNo], 0) <> mrYes then Abort;
  qryOIProiecte.Delete;

  {
  if (MessageDlg(
     Format('Doriti stergerea proiectului %s (%d)', [ qryOIProiecte.FieldByName('DENUMIRE').AsString , qryOIProiecte.FieldByName('ID_OI_PROIECTE').AsInteger ]),
     mtConfirmation, [mbYes, mbNo], 0) = mrYes) then
    qryOIProiecte.Delete;
  }
end;

procedure TfrmOIProiecte.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
   Action := caFree;
end;

procedure TfrmOIProiecte.edtContPropertiesButtonClick(Sender: TObject;
  AButtonIndex: Integer);
var
  aCont : String;
begin
  aCont := QrySolduri.FieldByName('CONT').AsString;
  if SelectareContPlan(aCont, True) then
    DBSetFieldValue(QrySolduri, 'CONT', aCont);
end;

procedure TfrmOIProiecte.GridBugetVcod_functionalGetCellHint(
  Sender: TcxCustomGridTableItem; ARecord: TcxCustomGridRecord;
  ACellViewInfo: TcxGridTableDataCellViewInfo; const AMousePos: TPoint;
  var AHintText: TCaption; var AIsHintMultiLine: Boolean;
  var AHintTextRect: TRect);
begin
  AHintText := ARecord.Values[GridBugetVden_functional.Index];
end;

function TfrmOIProiecte.GetSelectedProject: Variant;
var
  lNode: TcxDBTreeListNode;
begin
  lNode := TreeProiecte.FocusedNode as TcxDBTreeListNode;
  if Assigned(lNode) then begin
    Result := lNode.KeyValue;
  end
  else Result := Null;
end;

procedure TfrmOIProiecte.GridBugetVcod_economicGetCellHint(
  Sender: TcxCustomGridTableItem; ARecord: TcxCustomGridRecord;
  ACellViewInfo: TcxGridTableDataCellViewInfo; const AMousePos: TPoint;
  var AHintText: TCaption; var AIsHintMultiLine: Boolean;
  var AHintTextRect: TRect);
begin
  AHintText := ARecord.Values[GridBugetVden_economic.Index];
end;

procedure TfrmOIProiecte.cxButton1Click(Sender: TObject);
begin
  QrySolduri.Append; 
end;

procedure TfrmOIProiecte.cxButton2Click(Sender: TObject);
begin
  QrySolduri.Delete;
end;

procedure TfrmOIProiecte.cxButton4Click(Sender: TObject);
begin
  if QrySolduri.State in [dsEdit, dsInsert] then QrySolduri.Post;
end;

procedure TfrmOIProiecte.QrySolduriNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('ID_REPARTITORI').AsInteger := qryOIProiecte.FieldByName('ID_OI_PROIECTE').AsInteger;  
end;

procedure TfrmOIProiecte.SetIsReadOnly(const Value: Boolean);
begin
  FIsReadOnly := Value;
  pnControl.Visible := not FIsReadOnly;
  pnBotomSelect.Visible := FIsReadOnly;
  edtIdGestTipMaterial.Properties.ReadOnly := FIsReadOnly;
  cxDBTextEdit1.Properties.ReadOnly := FIsReadOnly;
  edtDenumire.Properties.ReadOnly := FIsReadOnly;
  edtDescriere.Properties.ReadOnly := FIsReadOnly;
  edtTipProiect.Properties.ReadOnly := FIsReadOnly;
  edtSeAfiseaza.Properties.ReadOnly := FIsReadOnly;
  edDataProiect.Properties.ReadOnly := FIsReadOnly;
  edtAreContabilitateProprie.Properties.ReadOnly := FIsReadOnly;
  cxDBCheckBox1.Properties.ReadOnly := FIsReadOnly;
  chkProcentual.Properties.ReadOnly := FIsReadOnly;
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

procedure TfrmOIProiecte.btnCFAddClick(Sender: TObject);
begin
  if QryCF.Active then 
    QryCF.Append;
end;

procedure TfrmOIProiecte.btnCFDelClick(Sender: TObject);
begin
  if QryCF.Active and (QryCF.RecordCount>0) then
     QryCF.Delete;
end;

procedure TfrmOIProiecte.btnCFUpdClick(Sender: TObject);
begin
  if QryCF.Active and (QryCF.State in [dsEdit, dsInsert]) then QryCF.Post;
end;

procedure TfrmOIProiecte.edCFPropertiesButtonClick(Sender: TObject;
  AButtonIndex: Integer);
var
  lDataSet    : TDataSet;
  lFieldName  : String;
  lCont       : String;
  lIdUnitate  : Integer;
  lButton     : TcxDBButtonEdit;
begin
  lButton := Sender as TcxDBButtonEdit;
  if Assigned(lButton) and Assigned(lButton.DataBinding.DataSource) and Assigned(lButton.DataBinding.DataSource.DataSet) then begin
    lDataSet    := TcxDBButtonEdit(Sender).DataBinding.DataSource.DataSet;
    lFieldName  := TcxDBButtonEdit(Sender).DataBinding.DataField;
    lIdUnitate  := lDataSet.FieldByName('id_oi_unitati').AsInteger;
    lCont       := lDataSet.FieldByName(lFieldName).AsString;
    NewSelectarePlanFunctional(lCont, lIdUnitate, True);
    if lCont <> '<Anulat>' then begin
      lDataSet.Edit;
      lDataSet.FieldByName(lFieldName).Value := lCont;
      lDataSet.FieldByName('id_oi_unitati').Clear;
      if lIdUnitate > 0 then
        lDataSet.FieldByName('id_oi_unitati').Value := lIdUnitate;
      lDataSet.Post;
    end;
  end;
end;

procedure TfrmOIProiecte.edCEPropertiesButtonClick(Sender: TObject;
  AButtonIndex: Integer);
var
  lDataSet    : TDataSet;
  lFieldName  : String;
  lCont       : String;
  lIdProiect  : Integer;
  lButton     : TcxDBButtonEdit;
begin
  lButton := Sender as TcxDBButtonEdit;
  if Assigned(lButton) and Assigned(lButton.DataBinding.DataSource) and Assigned(lButton.DataBinding.DataSource.DataSet) then begin
    lDataSet    := TcxDBButtonEdit(Sender).DataBinding.DataSource.DataSet;
    lFieldName  := TcxDBButtonEdit(Sender).DataBinding.DataField;
    lIdProiect  := -1;
    lCont       := lDataSet.FieldByName(lFieldName).AsString;
    NewSelectarePlanEconomic(lCont, lIdProiect, '', -1, True);
    if lCont <> '<Anulat>' then begin
      lDataSet.Edit;
      lDataSet.FieldByName(lFieldName).Value := lCont;
      lDataSet.Post;
    end;
  end;
end;

procedure TfrmOIProiecte.btnPlanificareClick(Sender: TObject);
begin
  MainForm.Cmd_BGAprobatExecute(nil);
end;


procedure TfrmOIProiecte.AddProiect(const Parented: Boolean);
var
  ParentId: Variant;
  lProject: Variant;
begin
  if not Parented then ParentId := Null
  else begin
    { Adaugam o subfunctie noua in contextul curent
      Daca este organigrama sau tree }
    if TreeProiecte.FocusedNode <> nil then ParentId := qryOIProiecte.FieldByName('ID_OI_PROIECTE').AsInteger
    else ParentId := Null;
  end;
  lProject := DBGetScallarFmt('exec [spOIProiecteAdd] %s, %s', [ValueToStr('<Proiect Nou>'), ValueToStr(ParentId)]);
  if ValueHasValue(lProject) then
    qryOIProiecte.Locate('ID_OI_PROIECTE', lProject, []);
end;

procedure TfrmOIProiecte.qryOIProiecteAfterOpen(DataSet: TDataSet);
begin
  DBExecSQL('exec spOIProiecteVerifcaDetalii');
  TreeProiecteFocusedNodeChanged(TreeProiecte, nil, TreeProiecte.FocusedNode);
end;

procedure TfrmOIProiecte.qryOIProiecteNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('STARE').AsBoolean := True;
end;

procedure TfrmOIProiecte.QryCFNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('ID_REPARTITORI').AsInteger := qryOIProiecte.FieldByName('ID_OI_PROIECTE').AsInteger;
end;

procedure TfrmOIProiecte.edtSoldDebitEnter(Sender: TObject);                
begin
  edtSoldDebit.Properties.ReadOnly := not (edtSoldCredit.Value = 0);
  edtSoldCredit.Properties.ReadOnly := not (edtSoldDebit.Value = 0);
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
  DBRefresh(qryOIProiecte);
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
  if not FIsReadOnly then begin
    edtTipProiect.Text := '';
    if not qryOITipuriProiecte.Active then RefreshTipProiecte;
    edtTipProiect.Tag := qryOIProiecte.FieldByName('ID_OI_TIPURI_PROIECTE').AsInteger;
    if edtTipProiect.Tag <> 0 then begin
      lNode := TcxDBTreeListNode(TreeTipProiect.FindNodeByKeyValue(edtTipProiect.Tag, nil));
      if lNode <> nil then edtTipProiect.Text := VarToStr(lNode.Values[TreeTipProiectDENUMIRE.ItemIndex]);
    end;
    qryBuget.Close;
    qryBuget.Params.ParamByName('ID_PROIECTE').Value := qryOIProiecte.FieldByName('ID_OI_PROIECTE').AsInteger;
    qryBuget.Open;
    QrySolduri.Close;
    QrySolduri.Params.ParamByName('ID_PROIECTE').Value := qryOIProiecte.FieldByName('ID_OI_PROIECTE').AsInteger;
    QrySolduri.Open;
    QryCF.Close;
    QryCF.Params.ParamByName('ID_PROIECTE').Value := qryOIProiecte.FieldByName('ID_OI_PROIECTE').AsInteger;
    QryCF.Open;
    GridCFVPROCENT.Visible := qryOIProiecte.FieldByName('ESTE_PROCENTUAL').AsBoolean;
  end
  else begin
    btnOkSelect.Enabled := Assigned(AFocusedNode);
  end;
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

procedure TfrmOIProiecte.ckDragDropPropertiesEditValueChanged(
  Sender: TObject);
begin
  if not ckDragDrop.Checked then TreeProiecte.OnDragOver := nil
                        else TreeProiecte.OnDragOver := TreeProiecteDragOver;
end;

procedure TfrmOIProiecte.TreeProiecteDragOver(Sender, Source: TObject; X,
  Y: Integer; State: TDragState; var Accept: Boolean);
begin
//
end;

procedure TfrmOIProiecte.btnAddSubproiectClick(Sender: TObject);
begin
  if not qryOIProiecte.Locate('DENUMIRE', '<Proiect Nou>', []) then
    AddProiect(True);
end;

procedure TfrmOIProiecte.chkProcentualPropertiesEditValueChanged(
  Sender: TObject);
begin
  QryCF.Close;
  QryCF.Params.ParamByName('ID_PROIECTE').Value := qryOIProiecte.FieldByName('ID_OI_PROIECTE').AsInteger;
  QryCF.Open;
  GridCFVPROCENT.Visible := chkProcentual.Checked;
end;

end.
