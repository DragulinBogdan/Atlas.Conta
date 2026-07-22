unit fmSelectieCEUnit;

interface

uses
  Forms, Classes, Controls, ExtCtrls, cxGraphics, cxControls,
  cxLookAndFeels, cxLookAndFeelPainters, cxCustomData, cxStyles, cxTL,
  cxTLdxBarBuiltInMenu, cxInplaceContainer, cxTLData, cxDBTL, DB,
  ZAbstractRODataset, ZDataset, cxPC, cxContainer, cxEdit, cxMaskEdit,
  cxTextEdit, cxDropDownEdit, cxImageComboBox, StdCtrls, cxProgressBar,
  ZAbstractDataset, cxCurrencyEdit, cxCheckBox, cxFilter, cxData,
  cxDataStorage, cxDBData, cxGridCustomTableView, cxGridTableView,
  cxGridBandedTableView, cxGridDBBandedTableView, cxGridCustomView,
  cxClasses, cxGridLevel, cxGrid, Menus, cxButtons, cxLabel, dxBarBuiltInMenu,
  cxNavigator, dxScrollbarAnnotations, dxDateRanges;

type
  TfmSelectieCE = class(TForm)
    pnBugete: TPanel;
    Panel7: TPanel;
    chkDoarFolosite: TcxCheckBox;
    cxTreeEconomic: TcxDBTreeList;
    cxTreeEconomicid_bg_plan_economic: TcxDBTreeListColumn;
    cxTreeEconomicid_parinte: TcxDBTreeListColumn;
    cxTreeEconomiccod_economic: TcxDBTreeListColumn;
    cxTreeEconomiccod_ecran: TcxDBTreeListColumn;
    cxTreeEconomicid_oi_proiecte: TcxDBTreeListColumn;
    cxTreeEconomicdenumire: TcxDBTreeListColumn;
    cxTreeEconomicdescriere: TcxDBTreeListColumn;
    cxTreeEconomictip: TcxDBTreeListColumn;
    cxTreeEconomicfolosit: TcxDBTreeListColumn;
    cxTreeEconomicid_bg_tipuri_buget: TcxDBTreeListColumn;
    cxTreeEconomicid_analitic: TcxDBTreeListColumn;
    stiluriCF: TcxStyleRepository;
    stilProcent: TcxStyle;
    stilUnitate: TcxStyle;
    dtCEDeAngajat: TDataSource;
    qryCEDeAngajat: TZReadOnlyQuery;
    pageEconomic: TcxPageControl;
    tabEconomic: TcxTabSheet;
    tabEconomicProiect: TcxTabSheet;
    qryProiectList: TZQuery;
    dtProiectList: TDataSource;
    cxTreeEconomicCLASA: TcxDBTreeListColumn;
    cxTreeEconomicid_oi_unitati: TcxDBTreeListColumn;
    cxTreeEconomiceste_procentual: TcxDBTreeListColumn;
    pnBottom: TPanel;
    nivelProiecte: TcxGridLevel;
    gridProiecte: TcxGrid;
    viewProiecte: TcxGridDBBandedTableView;
    viewProiectenumeProiect: TcxGridDBBandedColumn;
    viewProiectedescProiect: TcxGridDBBandedColumn;
    viewProiectenumeUnitate: TcxGridDBBandedColumn;
    viewProiectedescUnitate: TcxGridDBBandedColumn;
    viewProiecteesteProcentual: TcxGridDBBandedColumn;
    viewProiectecod_functional: TcxGridDBBandedColumn;
    viewProiectecod_economic: TcxGridDBBandedColumn;
    viewProiecteid_oi_unitati: TcxGridDBBandedColumn;
    viewProiecteid_oi_proiecte: TcxGridDBBandedColumn;
    viewProiecteplanificat: TcxGridDBBandedColumn;
    viewProiecteca: TcxGridDBBandedColumn;
    viewProiecteang_legal_anual: TcxGridDBBandedColumn;
    viewProiecteang_legal_multi: TcxGridDBBandedColumn;
    viewProiecteang_bug_anual: TcxGridDBBandedColumn;
    viewProiecteang_bug_multi: TcxGridDBBandedColumn;
    viewProiecteang_legal: TcxGridDBBandedColumn;
    viewProiecteang_bugetar: TcxGridDBBandedColumn;
    viewProiecteprocCA: TcxGridDBBandedColumn;
    viewProiecteprocBuget: TcxGridDBBandedColumn;
    btnCancel: TcxButton;
    btnOk: TcxButton;
    viewProiecteangajat: TcxGridDBBandedColumn;
    Panel1: TPanel;
    lbSumaProiect: TcxLabel;
    edSumaProiect: TcxCurrencyEdit;
    cxTreeEconomiccod_functional: TcxDBTreeListColumn;
    cxTreeEconomicprevederi: TcxDBTreeListColumn;
    cxTreeEconomicanterior: TcxDBTreeListColumn;
    cxTreeEconomicprocAngajat: TcxDBTreeListColumn;
    procedure cxTreeEconomicDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure cxTreeEconomicDblClick(Sender: TObject);
    procedure cxTreeEconomicKeyUp(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure FormCreate(Sender: TObject);
    procedure pageEconomicChange(Sender: TObject);
    procedure chkDoarFolositePropertiesEditValueChanged(Sender: TObject);
    procedure cxTreeEconomicStylesGetContentStyle(Sender: TcxCustomTreeList;
      AColumn: TcxTreeListColumn; ANode: TcxTreeListNode;
      var AStyle: TcxStyle);
    procedure qryProiectListAfterOpen(DataSet: TDataSet);
    procedure viewProiecteDblClick(Sender: TObject);
    procedure viewProiecteKeyUp(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure btnOkClick(Sender: TObject);
  private
    FDataSet: TDataSet;
    procedure TryCloseBuget(AModalOK: Boolean);
    procedure TryCloseProiect(AModalOK: Boolean);
    procedure CloseUpForm(AModalOK: Boolean);
  protected
    procedure TryOpenDataSet(ADataSet: TDataSet);
    procedure TryLocateField(ATree: TcxDBTreeList; AColumn: TcxTreeListColumn; const AFieldList: array of String);
  public
    procedure OpenDataSets;
    procedure BeforePopup;
    procedure SetParameterDataSet(ADataSet: TDataSet; const AParamName: String; const AParamValue: Variant);
    procedure SetParameter(const AParamName: String; const AParamValue: Variant);
    property  DataSet: TDataSet read FDataSet write FDataSet;
  end;

implementation

{$R *.DFM}

uses
  TypInfo, ZeosDBUtile, dxCompsUtile, CommonDBVar, Windows, Math;

procedure TfmSelectieCE.cxTreeEconomicDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ValueSafeToStr(ANode.Values[cxTreeEconomiccod_ecran.ItemIndex]) + ': ' + ValueSafeToStr(ANode.Values[cxTreeEconomicDENUMIRE.ItemIndex]);
end;

procedure TfmSelectieCE.cxTreeEconomicDblClick(Sender: TObject);
begin
  TryCloseBuget(True);
end;

procedure TfmSelectieCE.cxTreeEconomicKeyUp(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
    TryCloseBuget(True)
  else if Key = VK_ESCAPE then
    TryCloseBuget(False);
end;

procedure TfmSelectieCE.FormCreate(Sender: TObject);
begin
  ZeosDBUtile.OpenDataSets(Self);
  AddcxPopupComponent(Self);
  SetParameterDataSet(qryCEDeAngajat, 'refUser', iUserID);
  SetParameterDataSet(qryProiectList, 'refUser', iUserID);
  cxTreeEconomic.FullExpand;
  tabEconomic.TabStop := False;
  tabEconomicProiect.TabStop := False;
end;

procedure TfmSelectieCE.SetParameter(const AParamName: String;
  const AParamValue: Variant);
begin
  SetParameterDataSet(qryProiectList, AParamName, AParamValue);
  SetParameterDataSet(qryCEDeAngajat, AParamName, AParamValue);
end;

procedure TfmSelectieCE.TryOpenDataSet(ADataSet: TDataSet);
const
  CParamNames: array[0..1] of String = ('cod_functional', 'dataAng');
var
  I: Integer;
  lParam    : TParam;
  lPropInfo : PPropInfo;
  lParams   : TParams;
begin
  lPropInfo := GetPropInfo(ADataSet, 'Params', []);
  if Assigned(lPropInfo) then begin
    lParams := TParams(GetOrdProp(ADataSet, lPropInfo));
    if not ADataSet.Active and Assigned(lParams) then begin
      for I := Low(CParamNames) to High(CParamNames) do begin
        lParam := lParams.FindParam(CParamNames[I]);
        if Assigned(lParam) and not ValueHasValue(lParam.Value) then
          Exit;
      end;
      ADataSet.Open;
    end;
  end;
end;

procedure TfmSelectieCE.OpenDataSets;
begin
  if not qryProiectList.Active then qryProiectList.Open;
  if not qryCEDeAngajat.Active then qryCEDeAngajat.Open;
end;

procedure TfmSelectieCE.pageEconomicChange(Sender: TObject);
begin
  if Assigned(FDataSet) then 
    if pageEconomic.ActivePage = tabEconomic then
      TryLocateField(cxTreeEconomic, cxTreeEconomicCOD_ECRAN, ['COD_ECRAN', 'COD_BUGET', 'COD_ECONOMIC'])
    else
    if Assigned(FDataSet) then
      qryProiectList.Locate('id_oi_proiecte', FDataSet['id_oi_proiecte'], []);
end;

procedure TfmSelectieCE.TryLocateField(ATree: TcxDBTreeList; AColumn: TcxTreeListColumn;
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

procedure TfmSelectieCE.BeforePopup;
begin
  if (qryProiectList.Active) and (qryProiectList.RecordCount > 0) then
    pageEconomic.ActivePage := tabEconomicProiect
  else
    pageEconomic.ActivePage := tabEconomic;
  pageEconomicChange(pageEconomic);
end;

procedure TfmSelectieCE.SetParameterDataSet(ADataSet: TDataSet;
  const AParamName: String; const AParamValue: Variant);
var
  lParam    : TParam;
  lPropInfo : PPropInfo;
  lParams   : TParams;
begin
  lPropInfo := GetPropInfo(ADataSet, 'Params', []);
  if Assigned(lPropInfo) then begin
    lParams := TParams(GetOrdProp(ADataSet, lPropInfo));
    if Assigned(lParams) then begin
      lParam := lParams.FindParam(AParamName);
      if Assigned(lParam) then begin
        if not ValueSameValue(lParam.Value, AParamValue) then begin
          ADataSet.Close;
          lParam.Value := AParamValue;
        end;
      end;
    end;
  end;
end;

procedure TfmSelectieCE.chkDoarFolositePropertiesEditValueChanged(
  Sender: TObject);
begin
  SetParameterDataSet(qryCEDeAngajat, 'doarFolosit', chkDoarFolosite.EditingValue);
  OpenDataSets;
end;

procedure TfmSelectieCE.cxTreeEconomicStylesGetContentStyle(
  Sender: TcxCustomTreeList; AColumn: TcxTreeListColumn;
  ANode: TcxTreeListNode; var AStyle: TcxStyle);
begin
  if (AColumn = cxTreeEconomicprocAngajat) then
    AStyle := stilProcent
  else
  if ValueHasValue(ANode.Values[cxTreeEconomicid_analitic.ItemIndex]) then
    AStyle := stilUnitate;
end;

procedure TfmSelectieCE.qryProiectListAfterOpen(DataSet: TDataSet);
begin
  qryProiectList.FieldDefs.Find('angajat').Attributes := qryProiectList.FieldDefs.Find('angajat').Attributes - [faReadonly];
  qryProiectList.FindField('angajat').ReadOnly := False;
end;

procedure TfmSelectieCE.viewProiecteDblClick(Sender: TObject);
begin
  TryCloseProiect(True);
end;

procedure TfmSelectieCE.CloseUpForm(AModalOK: Boolean);
var
  lForm : TCustomForm;
begin
  lForm := GetParentForm(pageEconomic);
  if Assigned(lForm) then
    if AModalOk then
      lForm.ModalResult := mrOk
    else
      lForm.ModalResult := mrCancel;
end;

procedure TfmSelectieCE.viewProiecteKeyUp(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
    TryCloseProiect(True)
  else if Key = VK_ESCAPE then
    TryCloseProiect(False);
end;

procedure TfmSelectieCE.TryCloseBuget(AModalOK: Boolean);
begin
  if not AModalOK or
     (Assigned(cxTreeEconomic.FocusedNode) and not cxTreeEconomic.FocusedNode.HasChildren) then
     CloseUpForm(AModalOK);
end;

procedure TfmSelectieCE.TryCloseProiect(AModalOK: Boolean);
begin
  if not AModalOK or Assigned(viewProiecte.Controller.FocusedRecord) then
     CloseUpForm(AModalOK);
end;

procedure TfmSelectieCE.btnOkClick(Sender: TObject);
begin
  if pageEconomic.ActivePage = tabEconomic then
    TryCloseBuget(True)
  else
    TryCloseProiect(True);
end;

end.
