unit NoteInchidereUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, ExtCtrls, DegradePanel, Menus, cxLookAndFeelPainters, StdCtrls,
  cxButtons, 
   cxGraphics, 
  cxDataStorage, cxEdit, DB, cxDBData, ZDataSet, cxGridLevel, cxClasses,
  cxControls, cxGridCustomView, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGrid, cxGridCustomPopupMenu, cxGridPopupMenu,
  cxImageComboBox, cxButtonEdit, cxTL, cxMaskEdit, cxInplaceContainer,
  cxDBTL, cxTLData, cxDropDownEdit, cxCheckBox,
  ZAbstractRODataset, ZAbstractDataset,
  cxTLdxBarBuiltInMenu,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, cxNavigator,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxBarBuiltInMenu,
  dxDateRanges, cxTextEdit, dxScrollbarAnnotations;

type
  TfrmNoteInchidere = class(TForm)
    pnTop: TDegradePanel;
    pnContent: TPanel;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    GridInchidereV: TcxGridDBTableView;
    GridInchidereL: TcxGridLevel;
    GridInchidere: TcxGrid;
    dtNoteInchidere: TDataSource;
    qryNoteInchidere: TZQuery;
    GridInchidereVID_NOTE_INCHIDERE: TcxGridDBColumn;
    GridInchidereVCONT_BALANTA: TcxGridDBColumn;
    GridInchidereVCONT_INCHIDERE: TcxGridDBColumn;
    GridInchidereVEXPLICATIE: TcxGridDBColumn;
    GridInchidereVPRIORITATE: TcxGridDBColumn;
    GridInchidereVTIP_SUMA: TcxGridDBColumn;
    GridInchidereVMOD_INCHIDERE: TcxGridDBColumn;
    cxGridPopupMenu: TcxGridPopupMenu;
    TreePlan: TcxDBTreeList;
    TreePlanCONT: TcxDBTreeListColumn;
    TreePlanROMANA: TcxDBTreeListColumn;
    TreePlanFCTCONT: TcxDBTreeListColumn;
    TreePlanSID: TcxDBTreeListColumn;
    TreePlanSIC: TcxDBTreeListColumn;
    TreePlanDescriere: TcxDBTreeListColumn;
    btnGenerare: TcxButton;
    btnRaportare: TcxButton;
    GridInchidereVFORMULA_CONDITIE: TcxGridDBColumn;
    GridInchidereVCOD_FUNCTIONAL: TcxGridDBColumn;
    GridInchidereVCOD_ECONOMIC: TcxGridDBColumn;
    TreeFunctional: TcxDBTreeList;
    TreeFunctionalDescriere: TcxDBTreeListColumn;
    TreeFunctionalCodF: TcxDBTreeListColumn;
    TreeFunctionalDenumire: TcxDBTreeListColumn;
    TreeFunctionalID: TcxDBTreeListColumn;
    TreeEconomic: TcxDBTreeList;
    TreeEconomicDescriere: TcxDBTreeListColumn;
    TreeEconomicCodE: TcxDBTreeListColumn;
    TreeEconomicDenumire: TcxDBTreeListColumn;
    TreeEconomicID: TcxDBTreeListColumn;
    btnDelFunctional: TcxButton;
    btnDelEconomic: TcxButton;
    procedure qryNoteInchidereNewRecord(DataSet: TDataSet);
    procedure FormCreate(Sender: TObject);
    procedure GridInchidereVCONT_BALANTAPropertiesButtonClick(
      Sender: TObject; AButtonIndex: Integer);
    procedure TreePlanDescriereGetDisplayText(Sender: TcxTreeListColumn;
      ANode: TcxTreeListNode; var Value: String);
    procedure TreePlanKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure TreePlanDblClick(Sender: TObject);
    procedure GridInchidereVCONT_BALANTAPropertiesCloseQuery(
      Sender: TObject; var CanClose: Boolean);
    procedure GridInchidereVCONT_BALANTAPropertiesPopup(Sender: TObject);
    procedure GridInchidereVCONT_INCHIDEREPropertiesPopup(Sender: TObject);
    procedure GridInchidereVCONT_INCHIDEREPropertiesCloseQuery(
      Sender: TObject; var CanClose: Boolean);
    procedure BtnOkClick(Sender: TObject);
    procedure BtnCancelClick(Sender: TObject);
    procedure btnGenerareClick(Sender: TObject);
    procedure btnRaportareClick(Sender: TObject);
    procedure GridInchidereVFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure GridInchidereVFORMULA_CONDITIEPropertiesButtonClick(
      Sender: TObject; AButtonIndex: Integer);
    procedure TreeFunctionalDescriereGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure TreeEconomicDescriereGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure GridInchidereVCOD_FUNCTIONALPropertiesCloseQuery(
      Sender: TObject; var CanClose: Boolean);
    procedure GridInchidereVCOD_ECONOMICPropertiesCloseQuery(
      Sender: TObject; var CanClose: Boolean);
    procedure GridInchidereVCOD_FUNCTIONALPropertiesPopup(Sender: TObject);
    procedure GridInchidereVCOD_ECONOMICPropertiesPopup(Sender: TObject);
    procedure btnDelFunctionalClick(Sender: TObject);
    procedure btnDelEconomicClick(Sender: TObject);
    procedure TreePlanCustomDrawDataCell(Sender: TcxCustomTreeList;
      ACanvas: TcxCanvas; AViewInfo: TcxTreeListEditCellViewInfo;
      var ADone: Boolean);
  private
    { Private declarations }
    FNeedChild : Boolean;
    procedure DoSaveDataSet;
    procedure TreeLocate(Sender : TObject);
  protected
    procedure ValidareFormula(Sender : TObject);
  public
    { Public declarations }
  end;

var
  frmNoteInchidere: TfrmNoteInchidere;

implementation

uses
  dxCompsUtile, ZeosDBUtile, dateUnit, CommonDBVar, PlanConturiUnit,
  rapInclude,
  UnitFormule;

{$R *.dfm}

procedure TfrmNoteInchidere.qryNoteInchidereNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('PRIORITATE').AsInteger := 1;
  DataSet.FieldByName('EXPLICATIE').AsString := 'Note de Inchidere ';
  if TcxImageComboBoxProperties(GridInchidereVTIP_SUMA.Properties).Items.Count > 0 then
    DataSet.FieldByName('TIP_SUMA').Value := TcxImageComboBoxProperties(GridInchidereVTIP_SUMA.Properties).Items[0].Value;
  if TcxImageComboBoxProperties(GridInchidereVMOD_INCHIDERE.Properties).Items.Count > 0 then
    DataSet.FieldByName('MOD_INCHIDERE').Value := TcxImageComboBoxProperties(GridInchidereVMOD_INCHIDERE.Properties).Items[0].Value;
end;

procedure TfrmNoteInchidere.FormCreate(Sender: TObject);
begin
  FillImageCombo(GridInchidereVTIP_SUMA.Properties, 'exec [spNoteInchidereTipSuma]', 'VALOARE', 'DESCRIERE');
  FillImageCombo(GridInchidereVMOD_INCHIDERE.Properties, 'exec [spNoteInchidereModInchidere]', 'VALOARE', 'DESCRIERE');
  DBRefresh(qryNoteInchidere);
end;

procedure TfrmNoteInchidere.GridInchidereVCONT_BALANTAPropertiesButtonClick(
  Sender: TObject; AButtonIndex: Integer);
var
  aCont : String;
  aExactCont : Boolean;
begin
  if AButtonIndex = 0 then begin
    aExactCont := (TcxGridDBColumn(Sender).Tag=1);
    aCont := qryNoteInchidere.FieldByName(TcxGridDBColumn(Sender).DataBinding.FieldName).AsString;
    if SelectareContPlan(aCont, aExactCont) then begin
      if not aExactCont then aCont := aCont + '%';
      DBSetFieldValue(qryNoteInchidere, TcxGridDBColumn(Sender).DataBinding.FieldName, aCont);
    end;
  end;
end;


procedure TfrmNoteInchidere.TreePlanDescriereGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  if ANode = nil then Exit;
  Value := ANode.Texts[TreePlanCONT.ItemIndex] + ' :  ' + ANode.Texts[TreePlanROMANA.ItemIndex];
end;

procedure TfrmNoteInchidere.TreePlanKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
     TreePlanDblClick(TcxDBTreeList(Sender))
  else if Key = VK_ESCAPE then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrCancel;
end;

procedure TfrmNoteInchidere.TreePlanDblClick(Sender: TObject);
begin
   with TcxDBTreeList(Sender) do
    if (FocusedNode <> nil) and (not FNeedChild or not FocusedNode.HasChildren) then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk;
end;

type
  TAccesscxPopupEdit = class(TcxPopupEdit);

procedure TfrmNoteInchidere.GridInchidereVCONT_BALANTAPropertiesCloseQuery(
  Sender: TObject; var CanClose: Boolean);
var
  lNode : TcxDBTreeListNode;
  lCont : String;
begin
  with TAccesscxPopupEdit(Sender) do
    if PopupWindow.ModalResult = mrOk then begin
       lNode := TcxDBTreeListNode(TreePlan.FocusedNode);
       if Assigned(lNode) then begin
          lCont := lNode.KeyValue;
          if lNode.HasChildren then lCont := lCont + '%';
          //Text := VarToStr(lNode.Values[TreePlanCONT.ItemIndex]);
          DBSetFieldValue(qryNoteInchidere, 'CONT_BALANTA', lCont);
       end;
    end;
end;

procedure TfrmNoteInchidere.GridInchidereVCONT_BALANTAPropertiesPopup(
  Sender: TObject);
begin
  TreeLocate(Sender);
  FNeedChild :=  False;
end;

procedure TfrmNoteInchidere.GridInchidereVCONT_INCHIDEREPropertiesPopup(
  Sender: TObject);
begin
  TreeLocate(Sender);
  FNeedChild :=  True;
end;

procedure TfrmNoteInchidere.GridInchidereVCONT_INCHIDEREPropertiesCloseQuery(
  Sender: TObject; var CanClose: Boolean);
var
  lNode : TcxDBTreeListNode;
  lCont : String;
begin
  with TAccesscxPopupEdit(Sender) do
    if PopupWindow.ModalResult = mrOk then begin
       lNode := TcxDBTreeListNode(TreePlan.FocusedNode);
       if Assigned(lNode) then begin
          lCont := lNode.KeyValue;
          DBSetFieldValue(qryNoteInchidere, 'CONT_INCHIDERE', lCont);
       end;
    end;
end;

procedure TfrmNoteInchidere.TreeLocate(Sender: TObject);
var
  lText : String;
begin
  if Sender is TcxPopupEdit then begin
    lText := TcxPopupEdit(Sender).Text;
    lText := StringReplace(lText, '%', '', [rfReplaceAll]);
    InternalPositioning(lText, TreePlan, 'CONT');
  end;
end;

procedure TfrmNoteInchidere.DoSaveDataSet;
begin
  if qryNoteInchidere.State in [dsEdit, dsInsert] then
    qryNoteInchidere.Post;
end;

procedure TfrmNoteInchidere.BtnOkClick(Sender: TObject);
begin
  DoSaveDataSet;
  if fsModal in FormState then ModalResult := mrOk
  else Close;
end;

procedure TfrmNoteInchidere.BtnCancelClick(Sender: TObject);
begin
  if fsModal in FormState then ModalResult := mrCancel
  else Close;
end;

procedure TfrmNoteInchidere.btnGenerareClick(Sender: TObject);
begin
  DoSaveDataSet;
  with GetTmpADOQuery do
    try
      ParamCheck := False;
      Sql.Add('exec SP_GENERARE_NOTE_SERVER_INCHIDERE');
      ExecSql;
    finally
      Free;
    end;
end;

procedure TfrmNoteInchidere.btnRaportareClick(Sender: TObject);
var
  aIdReport : Integer;
begin
  aIdReport :=  DateUnit.GetItemId('RapNoteInchidere');
  if aIdReport <> -1 then begin
     LoadReport(aIdReport);
     //WriteReportToRepository(aIdReport, 'Raport Note Inchidere', -1);
  end;
end;

procedure TfrmNoteInchidere.GridInchidereVFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
//var  aHint : String;
begin
{  aHint := '';
  if AFocusedRecord <> nil then begin
    aHint := AFocusedRecord.DisplayTexts[GridInchidereVTIP_SUMA.Index] + 'al contului ' + AFocusedRecord.DisplayTexts[GridInchidereVCONT_BALANTA.Index];
    if AFocusedRecord.Values[GridInchidereVTIP_SUMA_POZITIV.Index] = 'True' then
      aHint := 'daca ' + aHint + ' este pozitiv';
    aHint := aHint + ' se inchide prin ' + AFocusedRecord.DisplayTexts[GridInchidereVCONT_INCHIDERE.Index];
  end;
  SetHintInfo(aHint);}
end;

procedure TfrmNoteInchidere.GridInchidereVFORMULA_CONDITIEPropertiesButtonClick(
  Sender: TObject; AButtonIndex: Integer);
var
  aFrmEditor : TFrmFormula;
  OldFormula : String;
  aQry : TZReadOnlyQuery;
begin
  //editare formula
  aFrmEditor := TFrmFormula.Create(Self);
  with aFrmEditor do
    try
      OldFormula := qryNoteInchidere.FieldByName('FORMULA_CONDITIE').AsString;
      FormulaEdit.Text := OldFormula;
      aQry := GetTmpADOQuery;
      with aQry do
        try
          Name := 'Balanta';
          SQL.Add('exec spNoteInchidereCampuriAnexa');
          Open;
          aFrmEditor.AddItems(aFrmEditor.DataFieldPopup, aQry, True, AddFieldCont);
        finally
          Free;
        end;
      OnValidare := ValidareFormula;
      ShowModal;
      if ModalResult = mrOk then begin
        OldFormula := Trim(FormulaEdit.Text);
        DBSetFieldValue(qryNoteInchidere, 'FORMULA_CONDITIE', OldFormula);
      end;
    finally
      Free;
    end;
end;

procedure TfrmNoteInchidere.ValidareFormula(Sender: TObject);
var
  lFrmEdit  : TFrmFormula;
begin
  //Sender este TFrmFormula -- tre sa activam ok-ul
  if not (Sender is TFrmFormula) then Exit;
  lFrmEdit := TFrmFormula(Sender);
  try
    ShowMessage('Valoare : ' + ValueSafeToStr( DBGetScallarFmt('exec [spNoteInchidereTestFormula] %s', [ValueToStr(lFrmEdit.FormulaEdit.Text)]) ) );
    lFrmEdit.OkBtn.Visible := True;
  except
    on E: Exception do begin
      lFrmEdit.OkBtn.Visible := False;
      MessageDlg('Eroare la validarea formulei ! Reevaluati formula de calcul !'#13#10+E.Message, mtError, [mbOK], 0);
    end;
  end;
end;

procedure TfrmNoteInchidere.TreeFunctionalDescriereGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Texts[TreeFunctionalCodF.ItemIndex] + ' - ' + ANode.Texts[TreeFunctionalDenumire.ItemIndex];
end;

procedure TfrmNoteInchidere.TreeEconomicDescriereGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Texts[TreeEconomicCodE.ItemIndex] + ' - ' + ANode.Texts[TreeEconomicDenumire.ItemIndex];
end;

procedure TfrmNoteInchidere.GridInchidereVCOD_FUNCTIONALPropertiesCloseQuery(
  Sender: TObject; var CanClose: Boolean);
var
  lNode : TcxDBTreeListNode;
  lCont : String;
begin
  with TAccesscxPopupEdit(Sender) do
    if PopupWindow.ModalResult = mrOk then begin
       lNode := TcxDBTreeListNode(TreeFunctional.FocusedNode);
       if Assigned(lNode) then begin
          lCont := lNode.Values[TreeFunctionalCodF.ItemIndex];
          if lNode.HasChildren then lCont := lCont + '%';
          DBSetFieldValue(qryNoteInchidere, 'COD_FUNCTIONAL', lCont);
          DBSetFieldValue(qryNoteInchidere, 'ID_FUNCTIONAL', lNode.KeyValue);
       end;
    end;
end;

procedure TfrmNoteInchidere.GridInchidereVCOD_ECONOMICPropertiesCloseQuery(
  Sender: TObject; var CanClose: Boolean);
var
  lNode : TcxDBTreeListNode;
  lCont : String;
begin
  with TAccesscxPopupEdit(Sender) do
    if PopupWindow.ModalResult = mrOk then begin
       lNode := TcxDBTreeListNode(TreeEconomic.FocusedNode);
       if Assigned(lNode) then begin
          lCont := lNode.Values[TreeEconomicCodE.ItemIndex];
          if lNode.HasChildren then lCont := lCont + '%';
          DBSetFieldValue(qryNoteInchidere, 'COD_ECONOMIC', lCont);
          DBSetFieldValue(qryNoteInchidere, 'ID_ECONOMIC', lNode.KeyValue);
       end;
    end;
end;

procedure TfrmNoteInchidere.GridInchidereVCOD_FUNCTIONALPropertiesPopup(
  Sender: TObject);
var
  lText : String;
begin
  FNeedChild := False;
  if Sender is TcxPopupEdit then begin
    lText := TcxPopupEdit(Sender).Text;
    lText := StringReplace(lText, '%', '', [rfReplaceAll]);
    InternalPositioning(lText, TreeFunctional, 'COD_FUNCTIONAL');
  end;
end;

procedure TfrmNoteInchidere.GridInchidereVCOD_ECONOMICPropertiesPopup(
  Sender: TObject);
var
  lText : String;
begin
  FNeedChild := False;
  if Sender is TcxPopupEdit then begin
    lText := TcxPopupEdit(Sender).Text;
    lText := StringReplace(lText, '%', '', [rfReplaceAll]);
    InternalPositioning(lText, TreeEconomic, 'COD_ECONOMIC');
  end;
end;

procedure TfrmNoteInchidere.btnDelFunctionalClick(Sender: TObject);
begin
  DBSetFieldValue(qryNoteInchidere, 'COD_FUNCTIONAL', Null);
  DBSetFieldValue(qryNoteInchidere, 'ID_FUNCTIONAL', Null);
end;

procedure TfrmNoteInchidere.btnDelEconomicClick(Sender: TObject);
begin
  DBSetFieldValue(qryNoteInchidere, 'COD_ECONOMIC', Null);
  DBSetFieldValue(qryNoteInchidere, 'ID_ECONOMIC', Null);
end;

procedure TfrmNoteInchidere.TreePlanCustomDrawDataCell(
  Sender: TcxCustomTreeList; ACanvas: TcxCanvas;
  AViewInfo: TcxTreeListEditCellViewInfo; var ADone: Boolean);
var s: String;
begin
  if AViewInfo.Node.HasChildren then ACanvas.Font.Color := clGray;
  if AViewInfo.Column = TreePlanFCTCONT then
    if AViewInfo.Node <> nil then begin
      s := AViewInfo.Node.Values[TreePlanFCTCONT.ItemIndex];
      if s = 'B' then ACanvas.Brush.Color := clSkyBlue
      else if s = 'C' then ACanvas.Brush.Color := clLime
           else if s = 'D' then ACanvas.Brush.Color := clFuchsia;
      ACanvas.FillRect(AViewInfo.BoundsRect);
      ADone := False;
    end;
end;


end.
