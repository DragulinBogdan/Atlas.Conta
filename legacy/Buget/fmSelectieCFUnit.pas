unit fmSelectieCFUnit;

interface

uses
  Forms, Classes, Controls, ExtCtrls, cxGraphics, cxControls,
  cxLookAndFeels, cxLookAndFeelPainters, cxCustomData, cxStyles, cxTL,
  cxTLdxBarBuiltInMenu, cxInplaceContainer, cxTLData, cxDBTL, DB,
  ZAbstractRODataset, ZDataset, cxPC, cxContainer, cxEdit, cxMaskEdit,
  cxTextEdit, cxDropDownEdit, cxImageComboBox, StdCtrls, ZAbstractDataset,
  cxCheckBox, cxCurrencyEdit, cxProgressBar, dxBarBuiltInMenu, cxClasses,
  cxDataControllerConditionalFormattingRulesManagerDialog,
  dxScrollbarAnnotations;

type
  TfmSelectieCF = class(TForm)
    PageFunctional: TcxPageControl;
    tabFunctional: TcxTabSheet;
    pnBugete: TPanel;
    Panel7: TPanel;
    lbTipBuget: TLabel;
    edtFiltruBuget: TcxImageComboBox;
    cxTreeBugete: TcxDBTreeList;
    tabFunctionalContract: TcxTabSheet;
    TreeDetaliiContract: TcxDBTreeList;
    TreeDetaliiContractid: TcxDBTreeListColumn;
    TreeDetaliiContractid_parinte: TcxDBTreeListColumn;
    TreeDetaliiContractcod_functional: TcxDBTreeListColumn;
    TreeDetaliiContractid_oi_unitati: TcxDBTreeListColumn;
    TreeDetaliiContractcod_economic: TcxDBTreeListColumn;
    TreeDetaliiContractid_oi_proiecte: TcxDBTreeListColumn;
    TreeDetaliiContractcod_ecran: TcxDBTreeListColumn;
    TreeDetaliiContractcod_proiect: TcxDBTreeListColumn;
    TreeDetaliiContractNume: TcxDBTreeListColumn;
    TreeDetaliiContractValoare: TcxDBTreeListColumn;
    TreeDetaliiContractidFurnizor: TcxDBTreeListColumn;
    qryContracteInfo: TZReadOnlyQuery;
    DTContracteInfo: TDataSource;
    chkDoarFolosite: TcxCheckBox;
    dtCFDeAngajat: TDataSource;
    qryCFDeAngajat: TZReadOnlyQuery;
    cxTreeBugeteid_bg_plan_functional: TcxDBTreeListColumn;
    cxTreeBugeteid_parinte: TcxDBTreeListColumn;
    cxTreeBugetecod_functional: TcxDBTreeListColumn;
    cxTreeBugetecod_ecran: TcxDBTreeListColumn;
    cxTreeBugeteid_oi_unitati: TcxDBTreeListColumn;
    cxTreeBugetedenumire: TcxDBTreeListColumn;
    cxTreeBugetedescriere: TcxDBTreeListColumn;
    cxTreeBugetetip: TcxDBTreeListColumn;
    cxTreeBugeteplanificat: TcxDBTreeListColumn;
    cxTreeBugeteca: TcxDBTreeListColumn;
    cxTreeBugeteang_legal_anual: TcxDBTreeListColumn;
    cxTreeBugeteang_legal_multi: TcxDBTreeListColumn;
    cxTreeBugeteang_bug_anual: TcxDBTreeListColumn;
    cxTreeBugeteang_bug_multi: TcxDBTreeListColumn;
    cxTreeBugeteang_legal: TcxDBTreeListColumn;
    cxTreeBugeteang_bugetar: TcxDBTreeListColumn;
    cxTreeBugetenerepartizat: TcxDBTreeListColumn;
    cxTreeBugetefolosit: TcxDBTreeListColumn;
    cxTreeBugeteid_bg_tipuri_buget: TcxDBTreeListColumn;
    cxTreeBugeteid_analitic: TcxDBTreeListColumn;
    cxTreeBugeteprocCA: TcxDBTreeListColumn;
    cxTreeBugeteprocBuget: TcxDBTreeListColumn;
    stiluriCF: TcxStyleRepository;
    stilProcent: TcxStyle;
    stilNormal: TcxStyle;
    procedure cxTreeBugeteDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure cxTreeBugeteDblClick(Sender: TObject);
    procedure cxTreeBugeteKeyUp(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure FormCreate(Sender: TObject);
    procedure edtFiltruBugetPropertiesChange(Sender: TObject);
    procedure PageFunctionalChange(Sender: TObject);
    procedure chkDoarFolositePropertiesEditValueChanged(Sender: TObject);
    procedure cxTreeBugeteStylesGetContentStyle(Sender: TcxCustomTreeList;
      AColumn: TcxTreeListColumn; ANode: TcxTreeListNode; var AStyle: TcxStyle);
  private
    FDataSet: TDataSet;
  protected
    procedure TryOpenDataSet(ADataSet: TZReadOnlyQuery);
    procedure TryLocateField(ATree: TcxDBTreeList; AColumn: TcxTreeListColumn; const AFieldList: array of String);
  public
    procedure BeforePopup;
    procedure SetParameterDataSet(ADataSet: TZReadOnlyQuery; const AParamName: String; const AParamValue: Variant);
    procedure SetParameter(const AParamName: String; const AParamValue: Variant);
    property  DataSet: TDataSet read FDataSet write FDataSet;
  end;
  
implementation

{$R *.DFM}

uses
  ZeosDBUtile, dxCompsUtile, CommonDBVar, Windows;

procedure TfmSelectieCF.cxTreeBugeteDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Values[cxTreeBugeteCOD_ECRAN.ItemIndex] + ': '+ANode.Values[cxTreeBugeteDENUMIRE.ItemIndex];
end;

procedure TfmSelectieCF.cxTreeBugeteDblClick(Sender: TObject);
begin
  with TcxDBTreeList(Sender) do
      if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
        (GetParentForm(PageFunctional) as TcxPopupEditPopupWindow).ModalResult := mrOk;
end;

procedure TfmSelectieCF.cxTreeBugeteKeyUp(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
    with TcxDBTreeList(Sender) do
      if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
        (GetParentForm(PageFunctional) as TcxPopupEditPopupWindow).ModalResult := mrOk
  else if Key = VK_ESCAPE then
   (GetParentForm(PageFunctional) as TcxPopupEditPopupWindow).ModalResult := mrCancel;
end;

procedure TfmSelectieCF.cxTreeBugeteStylesGetContentStyle(
  Sender: TcxCustomTreeList; AColumn: TcxTreeListColumn; ANode: TcxTreeListNode;
  var AStyle: TcxStyle);
begin
  if (AColumn = cxTreeBugeteprocCA) or (AColumn = cxTreeBugeteprocBuget) then
    AStyle := stilProcent
  else
    AStyle := stilNormal;
end;

procedure TfmSelectieCF.FormCreate(Sender: TObject);
begin
  OpenDataSets(Self);
  AddcxPopupComponent(Self);
  SetParameterDataSet(qryCFDeAngajat, 'refUser', iUserID);
  FillImageCombo(edtFiltruBuget.Properties, 'select * from bg_tipuri_buget', 'id_bg_tipuri_buget', 'denumire', -1, '<Toate tipurile de bugete>');
  cxTreeBugete.FullExpand;
  tabFunctional.TabStop := False;
  tabFunctionalContract.TabStop := False;
end;

procedure TfmSelectieCF.edtFiltruBugetPropertiesChange(Sender: TObject);
begin
  if ValueSafeToInt(edtFiltruBuget.EditValue) = -1 then
    SetFilterOnDataSet(qryCFDeAngajat, '')
  else
    SetFilterOnDataSet(qryCFDeAngajat, 'ID_BG_TIPURI_BUGET= ' + ValueToStr(edtFiltruBuget.EditValue));
end;

procedure TfmSelectieCF.SetParameter(const AParamName: String;
  const AParamValue: Variant);
begin
  SetParameterDataSet(qryContracteInfo, AParamName, AParamValue);
  SetParameterDataSet(qryCFDeAngajat, AParamName, AParamValue);
end;

procedure TfmSelectieCF.TryOpenDataSet(ADataSet: TZReadOnlyQuery);
var
  I: Integer;
begin
  if not ADataSet.Active then begin
    for I := 0 to ADataSet.Params.Count-1 do
      if not ValueHasValue(ADataSet.Params[I].Value) then
        Exit;
    ADataSet.Open;
  end;
end;

procedure TfmSelectieCF.PageFunctionalChange(Sender: TObject);
begin
  if Assigned(FDataSet) then 
    if PageFunctional.ActivePage = tabFunctional then
      TryLocateField(cxTreeBugete, cxTreeBugeteCOD_ECRAN, ['COD_ECRAN', 'COD_BUGET', 'COD_FUNCTIONAL'])
    else
      TryLocateField(TreeDetaliiContract, TreeDetaliiContractidFurnizor, ['ID_LST_REPARTITORI']);
end;

procedure TfmSelectieCF.TryLocateField(ATree: TcxDBTreeList; AColumn: TcxTreeListColumn;
  const AFieldList: array of String);
var
  I: Integer;
  lField: TField;
  lNode : TcxTreeListNode;
begin
  if Assigned(FDataSet) then begin
    for I := Low(AFieldList) to High(AFieldList) do begin
      lField := FDataSet.FindField(AFieldList[I]);
      if Assigned(lField) then begin
        lNode := ATree.FindNodeByText(lField.AsString, AColumn);
        if Assigned(lNode) then begin
          lNode.Expand(True);
          lNode.Focused := True;
          Exit;
        end;
      end;
    end;
  end;
end;

procedure TfmSelectieCF.BeforePopup;
begin
  if (qryContracteInfo.Active) and (qryContracteInfo.RecordCount > 0) then
    PageFunctional.ActivePage := tabFunctionalContract
  else
    PageFunctional.ActivePage := tabFunctional;
  PageFunctionalChange(PageFunctional);
end;

procedure TfmSelectieCF.SetParameterDataSet(ADataSet: TZReadOnlyQuery;
  const AParamName: String; const AParamValue: Variant);
var
  lParam  : TParam;
begin
  lParam := ADataSet.Params.FindParam(AParamName);
  if Assigned(lParam) then begin
    if not ValueSameValue(lParam.Value, AParamValue) then begin
      ADataSet.Close;
      lParam.Value := AParamValue;
    end;
  end;
  if not ADataSet.Active then
    TryOpenDataSet(ADataSet);
end;

procedure TfmSelectieCF.chkDoarFolositePropertiesEditValueChanged(
  Sender: TObject);
begin
  SetParameterDataSet(qryCFDeAngajat, 'doarFolosit', chkDoarFolosite.EditingValue);
end;

end.
