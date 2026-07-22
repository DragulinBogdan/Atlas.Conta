unit frmSelectieContractUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxGraphics, cxControls, cxLookAndFeels, cxLookAndFeelPainters,
  cxCustomData, cxStyles, cxTL, cxMaskEdit, cxTLdxBarBuiltInMenu,
  cxContainer, cxEdit, Menus, StdCtrls, cxButtons, cxTextEdit,
  cxDropDownEdit, cxCalendar, cxInplaceContainer, cxDBTL, cxTLData, cxPC,
  DB, ZAbstractRODataset, ZAbstractDataset, ZDataset, cxCheckBox, ExtCtrls,
  cxClasses, cxGridLevel, cxGrid, cxFilter, cxData, cxDataStorage,
  cxDBData, cxCurrencyEdit, cxGridCustomPopupMenu, cxGridPopupMenu,
  cxGridCustomTableView, cxGridTableView, cxGridBandedTableView,
  cxGridDBBandedTableView, cxGridCustomView, cxGridDBTableView,
  dxBarBuiltInMenu, cxNavigator, Vcl.ComCtrls, dxCore, cxDateUtils,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxDateRanges,
  dxScrollbarAnnotations;

type
  TfrmSelectieContract = class(TForm)
    PageContract: TcxPageControl;
    tabSelContract: TcxTabSheet;
    tabDefContract: TcxTabSheet;
    Label5: TLabel;
    Label6: TLabel;
    lbDataContract: TLabel;
    edDataContract: TcxDateEdit;
    edNrContract: TcxTextEdit;
    DTContracte: TDataSource;
    QryContracte: TZQuery;
    chkFiltruDepartament: TcxCheckBox;
    chkFiltruPrestator: TcxCheckBox;
    pnlBottom: TPanel;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    nivelContract: TcxGridLevel;
    gridContracte: TcxGrid;
    viewContract: TcxGridDBBandedTableView;
    viewContractdenTipProgram: TcxGridDBBandedColumn;
    viewContractrefTipProgram: TcxGridDBBandedColumn;
    viewContractID_CONTRACT: TcxGridDBBandedColumn;
    viewContractcod_siruta: TcxGridDBBandedColumn;
    viewContractcodFiscal: TcxGridDBBandedColumn;
    viewContractID_PARINTE: TcxGridDBBandedColumn;
    viewContractNR_CONTRACT: TcxGridDBBandedColumn;
    viewContractDATA_CONTRACT: TcxGridDBBandedColumn;
    viewContractDESCRIERE: TcxGridDBBandedColumn;
    viewContractVALOARE: TcxGridDBBandedColumn;
    viewContractrest: TcxGridDBBandedColumn;
    viewContractid_beneficiar: TcxGridDBBandedColumn;
    viewContractBENEFICIAR: TcxGridDBBandedColumn;
    viewContractid_prestator: TcxGridDBBandedColumn;
    viewContractPRESTATOR: TcxGridDBBandedColumn;
    viewContractPROIECT: TcxGridDBBandedColumn;
    popupGrid: TcxGridPopupMenu;
    tabIntegrareOne: TcxTabSheet;
    viewProgrameOne: TcxGridDBTableView;
    nivelProgrameOne: TcxGridLevel;
    gridProgrameOne: TcxGrid;
    qryProgrameOne: TZReadOnlyQuery;
    dtProgrameOne: TDataSource;
    viewProgrameOnedenProgram: TcxGridDBColumn;
    viewProgrameOneidProgram: TcxGridDBColumn;
    viewProgrameOneid_contract: TcxGridDBColumn;
    viewProgrameOnecod_siruta: TcxGridDBColumn;
    viewProgrameOnetert: TcxGridDBColumn;
    viewProgrameOnecod_fiscal: TcxGridDBColumn;
    viewProgrameOnenumar: TcxGridDBColumn;
    viewProgrameOnedata: TcxGridDBColumn;
    viewProgrameOnevaloare: TcxGridDBColumn;
    viewProgrameOnerest: TcxGridDBColumn;
    viewProgrameOnerefContract: TcxGridDBColumn;
    popupIntegrareOne: TcxGridPopupMenu;
    procedure cxTreeContracteDblClick(Sender: TObject);
    procedure cxTreeContracteKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure edDataContractPropertiesValidate(Sender: TObject;
      var DisplayValue: Variant; var ErrorText: TCaption;
      var Error: Boolean);
    procedure FormCreate(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure chkFiltruDepartamentPropertiesChange(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure cxTreeContracteFocusedNodeChanged(Sender: TcxCustomTreeList;
      APrevFocusedNode, AFocusedNode: TcxTreeListNode);
    procedure BtnOkClick(Sender: TObject);
    procedure BtnCancelClick(Sender: TObject);
    procedure PageContractChange(Sender: TObject);
    procedure viewProgrameOneFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure viewContractDblClick(Sender: TObject);
    procedure viewContractEditKeyUp(Sender: TcxCustomGridTableView;
      AItem: TcxCustomGridTableItem; AEdit: TcxCustomEdit; var Key: Word;
      Shift: TShiftState);
  private
    { Private declarations }
    FDataContract : TDateTime;
    FIdContract   : Integer;
    FNrContract   : string;
    FIdPredator   : Integer;
    FIdAngajament : Integer;
    FIdPrimitor   : Integer;
    FFilterList   : TStringList;
    FIsFiltering  : Boolean;
    function GetDataContract: TDateTime;
    function GetIdContract: Integer;
    function GetNrContract: string;
    procedure ProcessScreenData;
    function IsContractDefined: Boolean;
    function GetNewIdContract : Integer;
    procedure ProcessFilter;
    procedure ResetFilter;
    function  GetSelectedOneContract: Integer;
    function  GetSelectedContract: Integer;
    procedure ActivateTreeContract(var Message: TMessage); message WM_USER+1;
    procedure SetIdContract(const Value: Integer);
    function GetrefOnContract: Variant;
    function GetrefOnTipProgram: Variant;
  public
    { Public declarations }

    procedure SaveContext;
    procedure RestoreContext;
    procedure RefreshContracte;
    procedure FilterContractByDepartament(aId : Integer; const aState : Boolean=True);
    procedure FilterContractByPrestator(aId : Integer; const aState : Boolean=True);
    function GetContractDetails(const AIdContract: Integer): String;

    property NrContract : string read GetNrContract;
    property DataContract : TDateTime read GetDataContract;
    property IdContract : Integer read GetIdContract write SetIdContract;
    property IdPredator : Integer read FIdPredator write FIdPredator;
    property IdPrimitor : Integer read FIdPrimitor write FIdPrimitor;
    property IdAngajament : Integer read FIdAngajament write FIdAngajament;
    property refOnTipProgram: Variant read GetrefOnTipProgram;
    property refOnContract: Variant read GetrefOnContract;
  end;

implementation

uses
  DateUtils, ZeosDBUtile, cxDataUtils, Math, CommonDBVar, ATSZDBUtils;

{$R *.dfm}

procedure TfrmSelectieContract.cxTreeContracteDblClick(Sender: TObject);
begin
   if (TcxDBTreeList(Sender).FocusedNode <> nil)  then
     if (GetParentForm(PageContract.Parent) is TcxPopupEditPopupWindow) then
        (GetParentForm(PageContract.Parent) as TcxPopupEditPopupWindow).ModalResult := mrOk
     else
       ModalResult := mrOk;
end;

procedure TfrmSelectieContract.cxTreeContracteKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
    with TcxDBTreeList(Sender) do
      if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then begin
         if (GetParentForm(PageContract.Parent) is TcxPopupEditPopupWindow) then
            (GetParentForm(PageContract.Parent) as TcxPopupEditPopupWindow).ModalResult := mrOk
         else
           ModalResult := mrOk;
      end
  else if Key = VK_ESCAPE then begin
    if (GetParentForm(PageContract.Parent) is TcxPopupEditPopupWindow) then
       (GetParentForm(PageContract.Parent) as TcxPopupEditPopupWindow).ModalResult := mrCancel
    else
      ModalResult := mrCancel;
  end;
end;

procedure TfrmSelectieContract.edDataContractPropertiesValidate(
  Sender: TObject; var DisplayValue: Variant; var ErrorText: TCaption;
  var Error: Boolean);
begin
  if Error then begin
     ErrorText := '';
     raise EContaHandledError.Create('Data introdusa este invalida ! ');
  end;
end;

procedure TfrmSelectieContract.RestoreContext;
var
  lPageIndex: Integer;
begin
  if not TryStrToInt( StorageReadValue( 'TabContract', '0', szDBName +  '_Angajamente'), lPageIndex) then
    lPageIndex := 0;
  if (lPageIndex > -1) and (lPageIndex < PageContract.PageCount) and PageContract.Pages[lPageIndex].Visible then
    PageContract.ActivePageIndex := lPageIndex;
end;

procedure TfrmSelectieContract.SaveContext;
begin
  StorageWriteValue('TabContract', IntToStr(PageContract.ActivePageIndex), szDBName +  '_Angajamente');
end;

procedure TfrmSelectieContract.FormCreate(Sender: TObject);
begin
  ZeosDBUtile.OpenDataSets(Self);
  tabIntegrareOne.TabVisible := ValueIsTrue(DBGetSetare('integrareOne'));
  RestoreContext;
  RefreshContracte;
  FFilterList := TStringList.Create;
  ResetFilter;
end;

procedure TfrmSelectieContract.RefreshContracte;
begin
  DTContracte.DataSet := nil;
  DBRefresh(QryContracte);
  if tabIntegrareOne.TabVisible then
    DBRefresh(qryProgrameOne);
  DTContracte.DataSet := QryContracte;
end;

procedure TfrmSelectieContract.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  SaveContext;
end;

function TfrmSelectieContract.GetDataContract: TDateTime;
begin
  ProcessScreenData;
  Result := FDataContract;
end;

function TfrmSelectieContract.GetIdContract: Integer;
begin
  ProcessScreenData;
  Result := FIdContract;
end;

function TfrmSelectieContract.GetNrContract: string;
begin
  ProcessScreenData;
  Result := FNrContract;
end;

procedure TfrmSelectieContract.ProcessScreenData;
var
  lRecord: TcxCustomGridRecord;
begin
  if PageContract.ActivePage = tabSelContract then
  begin
    lRecord := viewContract.Controller.FocusedRecord;
    if Assigned(lRecord) and (lRecord.IsData) then begin
      FIdContract   := lRecord.Values[viewContractID_CONTRACT.Index];
      FNrContract   := lRecord.Values[viewContractNR_CONTRACT.Index];
      FDataContract := lRecord.Values[viewContractDATA_CONTRACT.Index];
   end;
  end
  else
  if (pageContract.ActivePage = tabDefContract) or (not tabIntegrareOne.TabVisible) then begin
    //definim un contract nou
    FNrContract   := edNrContract.EditValue;
    FDataContract := edDataContract.Date;
    if (FIdContract = -1) or (FIdContract = 0) then
      FIdContract   := GetNewIdContract;
  end
  else begin
    lRecord := viewProgrameOne.Controller.FocusedRecord;
    if Assigned(lRecord) and (lRecord.IsData) then begin
      FIdContract   := lRecord.Values[viewProgrameOnerefContract.Index];
      FNrContract   := lRecord.Values[viewProgrameOnenumar.Index];
      FDataContract := lRecord.Values[viewProgrameOnedata.Index];
   end;
  end;
end;

function TfrmSelectieContract.GetNewIdContract: Integer;
begin
  Result := DBGetScallar('exec spAlopAngAddContract :NrContact, :DataContact, :Predator, :Primitor, :IdAng, :IdUtilizator',
                  [
                    edNrContract.EditValue,
                    edDataContract.Date,
                    FIdPredator,
                    FIdPrimitor,
                    FIdAngajament,
                    iUserID]);
  DBRefresh(QryContracte);
end;

procedure TfrmSelectieContract.chkFiltruDepartamentPropertiesChange(
  Sender: TObject);
var
  lIndex : Integer;
begin
  if FIsFiltering then Exit;
  lIndex := TcxCheckBox(Sender).Tag;
  FFilterList[lIndex] := IntToStr(Integer(TcxCheckBox(Sender).Checked)) + '=' + FFilterList.ValueFromIndex[lIndex];
  ProcessFilter;
end;

procedure TfrmSelectieContract.FilterContractByDepartament(aId: Integer; const aState : Boolean=True);
begin
  if aId <=0 then
    FFilterList[0] := '0='
  else begin
    FFilterList[0] := IntToStr(Integer(aState)) + '=(id_beneficiar='+IntToStr(aId) + ')';
  end;
  ProcessFilter;
end;

procedure TfrmSelectieContract.FilterContractByPrestator(aId: Integer; const aState : Boolean=True);
begin
  if aId <=0 then
    FFilterList[1] := '0='
  else begin
    FFilterList[1] := IntToStr(Integer(aState)) + '=(id_prestator='+IntToStr(aId) + ')';
  end;
  ProcessFilter;
end;

procedure TfrmSelectieContract.FormDestroy(Sender: TObject);
begin
  FFilterList.Free;
end;

procedure TfrmSelectieContract.ProcessFilter;
var
  I : Integer;
  lFilter : String;
begin
  if FIsFiltering then Exit;
  FIsFiltering := True;
  lFilter := '';
  for I := 0 to FFilterList.Count - 1 do
    if (FFilterList.ValueFromIndex[I] <> '') and (FFilterList.Names[I]<> '0') then begin
      case I of
        0 : chkFiltruDepartament.Checked := True;
        1 : chkFiltruPrestator.Checked := True;
      end;
      if lFilter = '' then
        lFilter := FFilterList.ValueFromIndex[I]
      else
        lFilter := lFilter + ' and ' + FFilterList.ValueFromIndex[I];
    end;
  QryContracte.Filtered := False;
  QryContracte.Filter := lFilter;
  QryContracte.Filtered := (lFilter <> '');
  FIsFiltering := False;
{
   if QryContracte.RecordCount = 0 then begin
      QryContracte.Filtered := False;
   end;
}
end;


procedure TfrmSelectieContract.ResetFilter;
begin
  FFilterList.Clear;
  FFilterList.Add('0=');
  FFilterList.Add('0=');
end;

procedure TfrmSelectieContract.cxTreeContracteFocusedNodeChanged(
  Sender: TcxCustomTreeList; APrevFocusedNode,
  AFocusedNode: TcxTreeListNode);
begin
  BtnOk.Enabled := Assigned(AFocusedNode) and not AFocusedNode.HasChildren;
end;

procedure TfrmSelectieContract.BtnOkClick(Sender: TObject);
begin
  if IsContractDefined then begin
    if PageContract.ActivePage = tabDefContract then
      FIdContract := 0
    else
    if PageContract.ActivePage = tabSelContract then
      FIdContract := GetSelectedContract
    else
    if (tabIntegrareOne.TabVisible) and (PageContract.ActivePage = tabIntegrareOne) then
      FIdContract := GetSelectedOneContract;
    if GetParentForm(PageContract.Parent) <> nil then
      GetParentForm(PageContract.Parent).ModalResult := mrOk
    else
      ModalResult := mrOk;
  end;
end;

procedure TfrmSelectieContract.BtnCancelClick(Sender: TObject);
begin
  if (GetParentForm(PageContract.Parent) is TcxPopupEditPopupWindow) then
     (GetParentForm(PageContract.Parent) as TcxPopupEditPopupWindow).ModalResult := mrCancel
  else
    ModalResult := mrCancel;
end;

procedure TfrmSelectieContract.PageContractChange(Sender: TObject);
begin
  btnOk.Enabled := (PageContract.ActivePage = tabDefContract);
  if PageContract.ActivePage <> tabDefContract then
    SendMessage(Handle,WM_USER+1, 0, 0);
end;

procedure TfrmSelectieContract.ActivateTreeContract(var Message: TMessage);
begin
   //ActiveControl := cxTreeContracte;
end;

function TfrmSelectieContract.GetContractDetails(
  const AIdContract: Integer): String;
var
  lRecordIdx : Integer;
  lRecord : TcxCustomGridRecord;
begin
  Result := '';
  if (AIdContract > 0) or (not tabIntegrareOne.TabVisible) then begin
    lRecordIdx := viewContract.DataController.FindRecordIndexByKey(AIdContract);
    if (lRecordIdx > -1) and (lRecordIdx < viewContract.ViewData.RecordCount) then begin
      lRecord := viewContract.ViewData.Records[lRecordIdx];
      Result  := 'Contract Nr. ' + lRecord.DisplayTexts[viewContractNR_CONTRACT.Index] + ' din ' + lRecord.DisplayTexts[viewContractDATA_CONTRACT.Index];
    end;
  end
  else begin
    lRecordIdx := viewProgrameOne.DataController.FindRecordIndexByKey(AIdContract);
    if (lRecordIdx > -1) and (lRecordIdx < viewProgrameOne.ViewData.RecordCount) then begin
      lRecord := viewProgrameOne.ViewData.Records[lRecordIdx];
      Result  := 'Contract Nr. ' + lRecord.DisplayTexts[viewProgrameOnenumar.Index] + ' din ' + lRecord.DisplayTexts[viewProgrameOnedata.Index];
    end;
  end;
end;

function TfrmSelectieContract.GetSelectedContract: Integer;
var
  lRecord : TcxCustomGridRecord;
begin
  lRecord := viewContract.Controller.FocusedRecord;
  if Assigned(lRecord) and lRecord.IsData then
    Result := ValueSafeToInt(lRecord.Values[viewContractID_CONTRACT.Index], 0)
  else
    Result := 0;
end;

function TfrmSelectieContract.GetSelectedOneContract: Integer;
var
  lRecord : TcxCustomGridRecord;
begin
  lRecord := viewProgrameOne.Controller.FocusedRecord;
  if Assigned(lRecord) and lRecord.IsData then
    Result := ValueSafeToInt(lRecord.Values[viewProgrameOnerefContract.Index], 0)
  else
    Result := 0;
end;

function TfrmSelectieContract.IsContractDefined: Boolean;
begin
  if PageContract.ActivePage = tabSelContract then
    Result := Assigned(viewContract.Controller.FocusedRecord) and
              viewContract.Controller.FocusedRecord.IsData
  else
  if pageContract.ActivePage = tabDefContract then
    Result := ValueHasValue(edNrContract.EditValue) and ValueHasValue(edDataContract.EditValue)
  else
    Result := Assigned(viewProgrameOne.Controller.FocusedRecord) and
              viewProgrameOne.Controller.FocusedRecord.IsData;
end;

procedure TfrmSelectieContract.SetIdContract(const Value: Integer);
var
  lRecordIndex: Integer;
begin
  FIdContract := Value;
  if Value = 0 then
    PageContract.ActivePage := tabDefContract
  else
  if (Value > 0) or not tabIntegrareOne.TabVisible then begin
    PageContract.ActivePage := tabSelContract;
    lRecordIndex := viewContract.DataController.FindRecordIndexByKey(FIdContract);
    viewContract.Controller.FocusRecord(lRecordIndex, True);
  end
  else
  if Value < 0 then begin
    PageContract.ActivePage := tabIntegrareOne;
    lRecordIndex := viewProgrameOne.DataController.FindRecordIndexByKey(FIdContract);
    viewProgrameOne.Controller.FocusRecord(lRecordIndex, True);
  end;
end;

function TfrmSelectieContract.GetrefOnContract: Variant;
var
  lIdContract : Integer;
begin
  lIdContract := IdContract;
  if lIdContract < 0 then begin
    Result := (-1 * lIdContract) div 100;
  end
  else
    Result := Null;
end;

function TfrmSelectieContract.GetrefOnTipProgram: Variant;
var
  lIdContract : Integer;
begin
  lIdContract := IdContract;
  if lIdContract < 0 then begin
    Result := (-1 * lIdContract) mod 100;
  end
  else
    Result := Null;
end;

procedure TfrmSelectieContract.viewContractDblClick(Sender: TObject);
begin
  BtnOk.Click;
end;

procedure TfrmSelectieContract.viewContractEditKeyUp(
  Sender: TcxCustomGridTableView; AItem: TcxCustomGridTableItem;
  AEdit: TcxCustomEdit; var Key: Word; Shift: TShiftState);
begin
  if Key = VK_RETURN then
    BtnOk.Click
  else
  if Key = VK_ESCAPE then
    BtnCancel.Click;
end;

procedure TfrmSelectieContract.viewProgrameOneFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
  BtnOk.Enabled := (AFocusedRecord <> nil) and (AFocusedRecord.IsData);
end;

end.
