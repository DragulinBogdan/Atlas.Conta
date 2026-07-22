unit imperechereTert;
                            
interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxControls, ExtCtrls, cxContainer, cxEdit, cxTextEdit,
  cxMaskEdit, cxDropDownEdit, StdCtrls, 
  cxTL, DB, cxGridLevel,
  cxClasses, cxGridCustomView, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGrid, cxInplaceContainer, cxTLData, cxDBTL, ZDataSet,
  cxButtons, cxSplitter, 
  cxGridCustomPopupMenu, cxGridPopupMenu,
  cxLabel, cxDBLabel, cxImageComboBox, cxCheckBox, cxGraphics,
  cxDataStorage, cxDBData, Menus,
  cxLookAndFeelPainters,
  ZAbstractRODataset, ZAbstractDataset,
  cxTLdxBarBuiltInMenu,
  cxLookAndFeels, cxCustomData, cxStyles, cxFilter, cxData, cxNavigator,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxBarBuiltInMenu;

type
  TfrmImperechereTert = class(TForm)
    GridInfo: TcxGridDBTableView;
    GridInfoLevel: TcxGridLevel;
    cxGridInfo: TcxGrid;
    TreeCont: TcxDBTreeList;
    TreeTert: TcxDBTreeList;
    DTCont: TDataSource;
    qryCont: TZQuery;
    DTTert: TDataSource;
    qryTert: TZQuery;
    TreeContcont: TcxDBTreeListColumn;
    TreeContdenumire: TcxDBTreeListColumn;
    TreeContparinte: TcxDBTreeListColumn;
    TreeContfctcont: TcxDBTreeListColumn;
    TreeTertid: TcxDBTreeListColumn;
    TreeTertnume: TcxDBTreeListColumn;
    TreeContDescriere: TcxDBTreeListColumn;
    TreeTertcod_fiscal: TcxDBTreeListColumn;
    DTDateTert: TDataSource;
    qryDateTert: TZQuery;
    cxSplitter: TcxSplitter;
    GridInfodata: TcxGridDBColumn;
    GridInfodataScadenta: TcxGridDBColumn;
    GridInfocont: TcxGridDBColumn;
    GridInfocodRep: TcxGridDBColumn;
    GridInfoeste_plata: TcxGridDBColumn;
    GridInfoSemn: TcxGridDBColumn;
    GridInfoDocument: TcxGridDBColumn;
    GridInfoid: TcxGridDBColumn;
    GridInfoModul: TcxGridDBColumn;
    GridInfoid_unic_modul: TcxGridDBColumn;
    GridInfoid_document_modul: TcxGridDBColumn;
    GridInfoobligatie: TcxGridDBColumn;
    GridInfostingere: TcxGridDBColumn;
    GridInfodocStingere: TcxGridDBColumn;
    GridInfoplati: TcxGridDBColumn;
    GridInfoexplicatie: TcxGridDBColumn;
    GridInforamasObligatie: TcxGridDBColumn;
    GridInforamasPlata: TcxGridDBColumn;
    GridInfoNumeTert: TcxGridDBColumn;
    GridInfoCodFiscal: TcxGridDBColumn;
    pnLeft: TPanel;
    GridTert: TcxGridDBTableView;
    cxGridTertLevel: TcxGridLevel;
    cxGridTert: TcxGrid;
    label1: TLabel;
    edPopCont: TcxPopupEdit;
    btnSetupConturi: TcxButton;
    GridTertId: TcxGridDBColumn;
    GridTertNume: TcxGridDBColumn;
    GridTertCodFiscal: TcxGridDBColumn;
    GridTertSuma: TcxGridDBColumn;
    cxGridPopupMenu: TcxGridPopupMenu;
    pnAll: TPanel;
    Label2: TLabel;
    edNumeRepartitor: TcxDBLabel;
    pnTop: TPanel;
    cxSplitterTop: TcxSplitter;
    Bevel1: TBevel;
    btnEditRepartitor: TcxButton;
    pnInfo: TPanel;
    pnControlBar: TPanel;
    Label3: TLabel;
    edtInfoMod: TcxImageComboBox;
    Label4: TLabel;
    edtInfoDetaliere: TcxImageComboBox;
    Label5: TLabel;
    Label6: TLabel;
    Label7: TLabel;
    cxDBLabel1: TcxDBLabel;
    cxDBLabel2: TcxDBLabel;
    cxDBLabel3: TcxDBLabel;
    Label8: TLabel;
    cxDBLabel4: TcxDBLabel;
    Label9: TLabel;
    cxDBLabel5: TcxDBLabel;
    Label10: TLabel;
    cxDBLabel6: TcxDBLabel;
    Label11: TLabel;
    cxDBLabel7: TcxDBLabel;
    Label12: TLabel;
    cxDBLabel8: TcxDBLabel;
    chkFilter: TcxCheckBox;
    GridInfocontCSP: TcxGridDBColumn;
    GridInforepCSP: TcxGridDBColumn;
    GridInfoNrDocument: TcxGridDBColumn;
    GridInfoTotalramasObligatie: TcxGridDBColumn;
    GridInfoTotalRamasPlata: TcxGridDBColumn;
    GridInfoTotalDocStingere: TcxGridDBColumn;
    GridInfoTotalDocument: TcxGridDBColumn;
    GridInfoTotalStingere: TcxGridDBColumn;
    GridInfoTotalramasDocument: TcxGridDBColumn;
    GridInfoNumeRepartitor: TcxGridDBColumn;
    GridInfoDenCont: TcxGridDBColumn;
    GridInfoLevel2: TcxGridLevel;
    GridInfo2: TcxGridDBTableView;
    GridInfo2data: TcxGridDBColumn;
    GridInfo2dataScadenta: TcxGridDBColumn;
    GridInfo2cont: TcxGridDBColumn;
    GridInfo2codRep: TcxGridDBColumn;
    GridInfo2este_plata: TcxGridDBColumn;
    GridInfo2Semn: TcxGridDBColumn;
    GridInfo2Document: TcxGridDBColumn;
    GridInfo2id: TcxGridDBColumn;
    GridInfo2Modul: TcxGridDBColumn;
    GridInfo2id_unic_modul: TcxGridDBColumn;
    GridInfo2id_document_modul: TcxGridDBColumn;
    GridInfo2obligatie: TcxGridDBColumn;
    GridInfo2stingere: TcxGridDBColumn;
    GridInfo2docStingere: TcxGridDBColumn;
    GridInfo2plati: TcxGridDBColumn;
    GridInfo2explicatie: TcxGridDBColumn;
    GridInfo2ramasObligatie: TcxGridDBColumn;
    GridInfo2ramasPlata: TcxGridDBColumn;
    GridInfo2contCSP: TcxGridDBColumn;
    GridInfo2repCSP: TcxGridDBColumn;
    GridInfo2NrDocument: TcxGridDBColumn;
    GridInfo2TotalramasObligatie: TcxGridDBColumn;
    GridInfo2TotalRamasPlata: TcxGridDBColumn;
    GridInfo2TotalDocStingere: TcxGridDBColumn;
    GridInfo2TotalDocument: TcxGridDBColumn;
    GridInfo2TotalStingere: TcxGridDBColumn;
    GridInfo2TotalramasDocument: TcxGridDBColumn;
    GridInfo2NumeRepartitor: TcxGridDBColumn;
    GridInfo2DenCont: TcxGridDBColumn;
    GridInfo2DenModul: TcxGridDBColumn;
    DTDateTert2: TDataSource;
    qryDateTert2: TZQuery;
    procedure btnSetupConturiClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure TreeContDescriereGetDisplayText(Sender: TcxTreeListColumn;
      ANode: TcxTreeListNode; var Value: String);
    procedure TreeContDblClick(Sender: TObject);
    procedure TreeContKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure edPopContPropertiesPopup(Sender: TObject);
    procedure edPopFurnizorPropertiesPopup(Sender: TObject);
    procedure edPopFurnizorPropertiesCloseUp(Sender: TObject);
    procedure edPopContPropertiesCloseUp(Sender: TObject);
    procedure GridTertFocusedRecordChanged(Sender: TcxCustomGridTableView;
      APrevFocusedRecord, AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure btnEditRepartitorClick(Sender: TObject);
    procedure edtInfoModPropertiesChange(Sender: TObject);
    procedure edtInfoDetalierePropertiesChange(Sender: TObject);
    procedure chkFilterPropertiesChange(Sender: TObject);
  private
    { Private declarations }
    FCont : String;
    FTert : Variant;
    FIsLoading : Boolean;
    procedure SetCont(Value : String);
  public
    { Public declarations }
    procedure PopulateListe;
    procedure RefreshDataSet;
    procedure RefreshInfoDate;
  end;

implementation

uses
  ZeosDBUtile, dateUnit, AlopIntretinereConturi, OERepartitoriUnit;

{$R *.dfm}

procedure TfrmImperechereTert.btnSetupConturiClick(Sender: TObject);
begin
  IntretinereAlopConturi;
  DBRefresh(qryCont);
end;

procedure TfrmImperechereTert.FormCreate(Sender: TObject);
begin
  FIsLoading := True;
  PopulateListe;
  cxGridTert.Top := 64;
  FCont := '';
  FTert := Null;
  DBRefresh(qryCont);
  FIsLoading := False;
  if qryCont.RecordCount > 0 then
    SetCont(qryCont.FieldByName('Cont').AsString)
  else
    SetCont('');
end;

procedure TfrmImperechereTert.TreeContDescriereGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Texts[TreeContcont.ItemIndex] + ' ' + ANode.Texts[TreeContdenumire.ItemIndex];
end;

procedure TfrmImperechereTert.TreeContDblClick(Sender: TObject);
begin
   with TcxDBTreeList(Sender) do
    if (FocusedNode <> nil) (*and (not FocusedNode.HasChildren) *) then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk;
end;

procedure TfrmImperechereTert.TreeContKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
     TreeContDblClick(TcxDBTreeList(Sender))
  else if Key = VK_ESCAPE then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrCancel;
end;

procedure TfrmImperechereTert.edPopContPropertiesPopup(Sender: TObject);
var
 lNode : TcxTreeListNode;
begin
  TreeCont.FullExpand;
  lNode := TreeCont.FindNodeByKeyValue(FCont);
  if lNode <> nil then begin
    lNode.Focused := True;
    lNode.MakeVisible;
  end;
end;

procedure TfrmImperechereTert.edPopFurnizorPropertiesPopup(
  Sender: TObject);
var
 lNode : TcxTreeListNode;
begin
  lNode := TreeTert.FindNodeByKeyValue(FTert);
  if Assigned(lNode) then begin
    lNode.Focused := True;
    lNode.MakeVisible;
  end;
end;

type
  TAccesscxPopupEdit = class(TcxPopupEdit);

procedure TfrmImperechereTert.edPopFurnizorPropertiesCloseUp(
  Sender: TObject);
var
  lNode : TcxDBTreeListNode;
begin                                                 
  with TAccesscxPopupEdit(Sender) do
    if PopupWindow.ModalResult = mrOk then begin
       lNode := TcxDBTreeListNode(TreeTert.FocusedNode);
       if Assigned(lNode) then begin
          FTert := lNode.KeyValue;
          Text := VarToStr(lNode.Values[TreeTertnume.ItemIndex]);
          RefreshDataSet;
       end;
    end;
end;

procedure TfrmImperechereTert.edPopContPropertiesCloseUp(Sender: TObject);
var
  lNode : TcxDBTreeListNode;
begin                                                 
  with TAccesscxPopupEdit(Sender) do
    if PopupWindow.ModalResult = mrOk then begin
       lNode := TcxDBTreeListNode(TreeCont.FocusedNode);
       if Assigned(lNode) then begin
          FCont := lNode.KeyValue;
          Text := VarToStr(lNode.Texts[TreeContDescriere.ItemIndex]);
          RefreshDataSet;
       end;
    end;
end;

procedure TfrmImperechereTert.RefreshDataSet;
begin
  if not FIsLoading then begin
    qryTert.Params.ParamByName('Cont').Value := FCont;
    DBRefresh(qryTert);
  end;
end;

procedure TfrmImperechereTert.GridTertFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
  if FIsLoading then Exit;
  if AFocusedRecord  = nil then Exit;
  if not AFocusedRecord.IsData then Exit;  
  if FTert <> AFocusedRecord.Values[GridTertId.Index] then begin
    FTert := AFocusedRecord.Values[GridTertId.Index];
    RefreshInfoDate;
  end
end;

procedure TfrmImperechereTert.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
   Action := caFree;
end;

procedure TfrmImperechereTert.btnEditRepartitorClick(Sender: TObject);
begin
  IntretinereRepartitor(FTert);
  RefreshDataSet;
end;

procedure TfrmImperechereTert.RefreshInfoDate;
begin
  if not FIsLoading then begin
    qryDateTert.Params.ParamByName('Cont').Value    := FCont;
    qryDateTert.Params.ParamByName('CodRep').Value  := FTert;
    qryDateTert.Params.ParamByName('tipDate').Value := edtInfoMod.EditValue;

    qryDateTert2.Params.ParamByName('Cont').Value     := FCont;
    qryDateTert2.Params.ParamByName('CodRep').Value   := FTert;
    qryDateTert2.Params.ParamByName('tipDate').Value  := edtInfoMod.EditValue;

    DBRefresh([qryDateTert, qryDateTert2]);
  end;
end;

procedure TfrmImperechereTert.edtInfoModPropertiesChange(Sender: TObject);
begin
  RefreshInfoDate;
end;

procedure TfrmImperechereTert.edtInfoDetalierePropertiesChange(
  Sender: TObject);
begin
  RefreshInfoDate;
end;

procedure TfrmImperechereTert.SetCont(Value: String);
var
  lNode : TcxTreeListNode;
begin
  if FCont <> Value then begin
    FCont := Value;
    lNode := TreeCont.FindNodeByKeyValue(FCont, nil);
    if lNode <> nil then begin
       lNode.Focused := True;
       lNode.MakeVisible;
       edPopCont.Text := VarToStr(lNode.Texts[TreeContDescriere.ItemIndex]);
    end;
    RefreshDataSet;
  end;
end;

procedure TfrmImperechereTert.chkFilterPropertiesChange(Sender: TObject);
begin
  if chkFilter.Checked then begin
    qryTert.Filter := 'SUMA <> 0';
    qryTert.Filtered := True;
  end
  else begin
    qryTert.Filtered := False;
    qryTert.Filter := '';
  end;
end;

procedure TfrmImperechereTert.PopulateListe;
begin

end;

end.
