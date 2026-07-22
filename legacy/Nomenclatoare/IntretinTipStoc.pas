unit IntretinTipStoc;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxLookAndFeelPainters, cxStyles, cxGraphics,
  cxDataStorage, cxEdit, DB, cxDBData, cxCheckBox,
  cxTextEdit, ImgList, Menus, AppEvnts, cxVGrid, cxGridTableView, ZDataSet,
  cxDBVGrid, cxInplaceContainer, cxContainer,
  cxGroupBox, cxGridLevel, cxGridCustomTableView, cxGridDBTableView,
  cxClasses, cxControls, cxGridCustomView, cxGrid, StdCtrls, cxButtons,
  ExtCtrls, cxTL,
  cxTLData, cxDBTL, cxGridBandedTableView, cxGridDBBandedTableView,
  cxMaskEdit, DBCtrls, cxDBEdit, cxDropDownEdit, cxImageComboBox,
  DegradePanel, 
  cxGridCustomPopupMenu, cxGridPopupMenu, cxSplitter,
  ZAbstractRODataset, ZAbstractDataset,
  cxTLdxBarBuiltInMenu,
  cxLookAndFeels, cxCustomData, cxFilter, cxData, cxNavigator, dxDateRanges,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxBarBuiltInMenu,
  dxScrollbarAnnotations;

type
  TfrmIntretinTipStoc = class(TForm)
    pnBottom: TPanel;
    cxStyleRepository1: TcxStyleRepository;
    cxStyle1: TcxStyle;
    cxStyle2: TcxStyle;
    cxStyle3: TcxStyle;
    cxStyle4: TcxStyle;
    cxStyle5: TcxStyle;
    cxStyle6: TcxStyle;
    cxStyle7: TcxStyle;
    cxStyle8: TcxStyle;
    cxStyle9: TcxStyle;
    cxStyle10: TcxStyle;
    cxStyle11: TcxStyle;
    cxStyle12: TcxStyle;
    cxStyle13: TcxStyle;
    cxStyle14: TcxStyle;
    cxStyle15: TcxStyle;
    cxStyle16: TcxStyle;
    cxStyle17: TcxStyle;
    cxStyle18: TcxStyle;
    cxStyle19: TcxStyle;
    cxStyle20: TcxStyle;
    cxStyle21: TcxStyle;
    cxStyle22: TcxStyle;
    cxStyle23: TcxStyle;
    cxStyle24: TcxStyle;
    cxStyle25: TcxStyle;
    cxStyle26: TcxStyle;
    cxStyle27: TcxStyle;
    cxStyle28: TcxStyle;
    cxStyle29: TcxStyle;
    cxStyle30: TcxStyle;
    cxStyle31: TcxStyle;
    cxStyle32: TcxStyle;
    cxStyle33: TcxStyle;
    cxStyle34: TcxStyle;
    cxStyle35: TcxStyle;
    cxStyle36: TcxStyle;
    cxStyle37: TcxStyle;
    cxStyle38: TcxStyle;
    cxStyle39: TcxStyle;
    cxStyle40: TcxStyle;
    cxStyle41: TcxStyle;
    cxStyle42: TcxStyle;
    cxStyle43: TcxStyle;
    cxStyle44: TcxStyle;
    cxStyle45: TcxStyle;
    cxStyle46: TcxStyle;
    cxStyle47: TcxStyle;
    cxStyle48: TcxStyle;
    cxStyle49: TcxStyle;
    cxStyle50: TcxStyle;
    cxStyle51: TcxStyle;
    cxStyle52: TcxStyle;
    cxStyle53: TcxStyle;
    cxStyle54: TcxStyle;
    cxStyle55: TcxStyle;
    cxStyle56: TcxStyle;
    cxStyle57: TcxStyle;
    cxStyle58: TcxStyle;
    cxStyle59: TcxStyle;
    cxStyle60: TcxStyle;
    cxStyle61: TcxStyle;
    cxStyle62: TcxStyle;
    GridTableViewStyleSheetUserFormat4: TcxGridTableViewStyleSheet;
    cxVerticalGridStyleSheetStormVGA: TcxVerticalGridStyleSheet;
    ConfMenu: TPopupMenu;
    mnuConfigureazaTipMat: TMenuItem;
    mnuDeseleteazaTot: TMenuItem;
    mnuInfluentaStock: TMenuItem;
    ImgList: TImageList;
    DTTipStoc: TDataSource;
    QryTipStoc: TZQuery;
    btnOk: TcxButton;
    pnContent: TPanel;
    pnLeft: TPanel;
    grpBoxNivStoc: TcxGroupBox;
    grpBoxGrupaStoc: TcxGroupBox;
    GridGrupaStoc: TcxGrid;
    TreeListNivStoc: TcxDBTreeList;
    DTGrupaStoc: TDataSource;
    QryGrupaStoc: TZQuery;
    GridGrupaStocLevel1: TcxGridLevel;
    cxStyle63: TcxStyle;
    GridGrupaStocDBTableView1: TcxGridDBTableView;
    GridGrupaStocDBTableView1id_gest_grupa_stoc: TcxGridDBColumn;
    GridGrupaStocDBTableView1denumire: TcxGridDBColumn;
    DTNivelStoc: TDataSource;
    QryNivelStoc: TZQuery;
    TreeListNivStocid_gest_nivel_stoc: TcxDBTreeListColumn;
    TreeListNivStocdenumire: TcxDBTreeListColumn;
    TreeListNivStocPARENT_ID: TcxDBTreeListColumn;
    TreeListStyleSheetSlate: TcxTreeListStyleSheet;
    cxStyle64: TcxStyle;
    cxStyle65: TcxStyle;
    cxStyle66: TcxStyle;
    cxStyle67: TcxStyle;
    cxStyle68: TcxStyle;
    cxStyle69: TcxStyle;
    cxStyle70: TcxStyle;
    cxStyle71: TcxStyle;
    cxStyle72: TcxStyle;
    cxStyle73: TcxStyle;
    cxStyle74: TcxStyle;
    Bevel1: TBevel;
    pnTop: TDegradePanel;
    cxGridPopupMenu1: TcxGridPopupMenu;
    pnBot: TPanel;
    Label1: TLabel;
    edtDenumire: TcxDBTextEdit;
    Label2: TLabel;
    edtDescriere: TcxDBTextEdit;
    Label3: TLabel;
    edtNivelStoc: TcxDBImageComboBox;
    Label4: TLabel;
    edtGrupaStoc: TcxDBImageComboBox;
    Panel1: TPanel;
    cxGridTipStoc: TcxGrid;
    cxGridTipStocDBTableView1: TcxGridDBTableView;
    cxGridTipStocDBTableView1ID_GEST_TIP_STOC: TcxGridDBColumn;
    cxGridTipStocDBTableView1DENUMIRE: TcxGridDBColumn;
    cxGridTipStocDBTableView1DESCRIERE: TcxGridDBColumn;
    cxGridTipStocDBTableView1TIP_DESCARCARE: TcxGridDBColumn;
    cxGridTipStocDBTableView1DATA_CALCUL: TcxGridDBColumn;
    cxGridTipStocDBTableView1ID_PARINTE: TcxGridDBColumn;
    cxGridTipStocDBTableView1id_gest_nivel_stoc: TcxGridDBColumn;
    cxGridTipStocDBTableView1id_gest_grupa_stoc: TcxGridDBColumn;
    cxGridTipStocLevel1: TcxGridLevel;
    btnAddProdus: TcxButton;
    btnStergeProd: TcxButton;
    cxSplitter: TcxSplitter;
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure FormCreate(Sender: TObject);
    procedure FormResize(Sender: TObject);
    procedure GridGrupaStocDBTableView1FocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure cxGridTipStocDBTableView1CustomDrawCell(
      Sender: TcxCustomGridTableView; ACanvas: TcxCanvas;
      AViewInfo: TcxGridTableDataCellViewInfo; var ADone: Boolean);
    procedure GridGrupaStocDBTableView1CustomDrawCell(
      Sender: TcxCustomGridTableView; ACanvas: TcxCanvas;
      AViewInfo: TcxGridTableDataCellViewInfo; var ADone: Boolean);
    procedure btnAddProdusClick(Sender: TObject);
    procedure btnStergeProdClick(Sender: TObject);
    procedure btnOkClick(Sender: TObject);
    procedure pnBottomResize(Sender: TObject);
    procedure pnTopResize(Sender: TObject);
    procedure edtNivelStocKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure edtGrupaStocKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure TreeListNivStocCustomDrawDataCell(Sender: TcxCustomTreeList;
      ACanvas: TcxCanvas; AViewInfo: TcxTreeListEditCellViewInfo;
      var ADone: Boolean);
    procedure TreeListNivStocFocusedNodeChanged(Sender: TcxCustomTreeList;
      APrevFocusedNode, AFocusedNode: TcxTreeListNode);
  private
    { Private declarations }
    FIdSelectedGrupaStoc : Integer;
    FIdSelectedNivelStoc : Integer;
    procedure PopulateDropDowns;
    procedure PostModifications;
  public
    { Public declarations }

  end;


implementation

uses
  ZeosDBUtile, DateUnit, CommonDBVar;

{$R *.dfm}

procedure TfrmIntretinTipStoc.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  PostModifications;

    Action := caFree;
end;

procedure TfrmIntretinTipStoc.FormCreate(Sender: TObject);
begin
  DBRefresh([QryGrupaStoc, QryNivelStoc, QryTipStoc]);
  PopulateDropDowns;
  FIdSelectedNivelStoc := -1;
  FIdSelectedGrupaStoc := -1;
end;

procedure TfrmIntretinTipStoc.FormResize(Sender: TObject);
begin
  grpBoxGrupaStoc.Height := pnLeft.Height div 2;
end;

procedure TfrmIntretinTipStoc.PopulateDropDowns;
var
 aItem : TcxImageComboBoxItem;
begin
  edtNivelStoc.Properties.Items.Clear;
  with QryNivelStoc do
  try
    First;
    while not eof do begin
      aItem := TcxImageComboBoxItem(edtNivelStoc.Properties.Items.Add);
      aItem.Value := FieldByName('ID_GEST_NIVEL_STOC').AsInteger;
      aItem.Description := FieldByName('DENUMIRE').AsString;
      Next;
    end;
  finally
    TcxImageComboBoxProperties(cxGridTipStocDBTableView1id_gest_nivel_stoc.Properties).Assign(edtNivelStoc.Properties);
    First;
  end;

  edtGrupaStoc.Properties.Items.Clear;
  with QryGrupaStoc do
  try
    First;
    while not eof do begin
      aItem := TcxImageComboBoxItem(edtGrupaStoc.Properties.Items.Add);
      aItem.Value := FieldByName('ID_GEST_GRUPA_STOC').AsInteger;
      aItem.Description := FieldByName('DENUMIRE').AsString;
      Next;
    end;
  finally
    TcxImageComboBoxProperties(cxGridTipStocDBTableView1id_gest_grupa_stoc.Properties).Assign(edtGrupaStoc.Properties);
    First;
  end;


end;

procedure TfrmIntretinTipStoc.GridGrupaStocDBTableView1FocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
  FIdSelectedGrupaStoc := -1;
  if Assigned(AFocusedRecord) and AFocusedRecord.IsData then
   FIdSelectedGrupaStoc := AFocusedRecord.Values[GridGrupaStocDBTableView1id_gest_grupa_stoc.Index];
  cxGridTipStocDBTableView1.Invalidate(True);   
end;

procedure TfrmIntretinTipStoc.cxGridTipStocDBTableView1CustomDrawCell(
  Sender: TcxCustomGridTableView; ACanvas: TcxCanvas;
  AViewInfo: TcxGridTableDataCellViewInfo; var ADone: Boolean);


  function VarIsNotRight(Value : Variant) : Boolean;
  begin
    Result := (VarIsEmpty(Value) or VarIsClear(Value) or VarIsNull(Value));
  end;

var
  lNode : TcxCustomGridRecord;
begin
  if AViewInfo = nil then Exit;
  lNode := AViewInfo.RecordViewInfo.GridRecord;
  if lNode = nil then Exit;
  if not VarIsNotRight(lNode.Values[cxGridTipStocDBTableView1id_gest_nivel_stoc.Index]) then
    if lNode.Values[cxGridTipStocDBTableView1id_gest_nivel_stoc.Index] = FIdSelectedNivelStoc then begin
      //ACanvas.Font.Style := ACanvas.Font.Style + [fsBold];
      ACanvas.Brush.Color := clSkyBlue;
    end;

  if not VarIsNotRight(lNode.Values[cxGridTipStocDBTableView1id_gest_grupa_stoc.Index]) then
    if lNode.Values[cxGridTipStocDBTableView1id_gest_grupa_stoc.Index] = FIdSelectedGrupaStoc then begin
      ACanvas.Font.Style := ACanvas.Font.Style + [fsBold];
      //ACanvas.Brush.Color := clSkyBlue;
      ACanvas.Pen.Color := clRed;
    end;

end;

procedure TfrmIntretinTipStoc.GridGrupaStocDBTableView1CustomDrawCell(
  Sender: TcxCustomGridTableView; ACanvas: TcxCanvas;
  AViewInfo: TcxGridTableDataCellViewInfo; var ADone: Boolean);
begin
  if AViewInfo.Focused then
   ACanvas.Font.Style := ACanvas.Font.Style + [fsBold];
end;

procedure TfrmIntretinTipStoc.btnAddProdusClick(Sender: TObject);
begin
  with QryTipStoc do begin
    Append;
    FieldByName('ID_GEST_TIP_STOC').AsInteger := GetNextId('GEST_TIP_STOC');
    FieldByName('DENUMIRE').AsString := 'Tip Stoc Nou';
    FieldByName('ID_GEST_NIVEL_STOC').AsInteger := FIdSelectedNivelStoc;
    FieldByName('ID_GEST_GRUPA_STOC').AsInteger := FIdSelectedGrupaStoc;
    Post;
    Edit;
  end;
end;

procedure TfrmIntretinTipStoc.btnStergeProdClick(Sender: TObject);
begin
  if (MessageDlg(
     Format('Doriti stergerea tipului de stoc %s (%d)', [ QryTipStoc.FieldByName('DENUMIRE').AsString , QryTipStoc.FieldByName('ID_GEST_TIP_STOC').AsInteger ]),
     mtConfirmation, [mbYes, mbNo], 0) = mrYes) then
  QryTipStoc.Delete;
end;

procedure TfrmIntretinTipStoc.PostModifications;
begin
  if QryTipStoc.State in [dsEdit, dsInsert] then QryTipStoc.Post;
end;

procedure TfrmIntretinTipStoc.btnOkClick(Sender: TObject);
begin
  if fsModal in FormState then ModalResult := mrOk
  else Close;
end;

procedure TfrmIntretinTipStoc.pnBottomResize(Sender: TObject);
begin
  btnOk.Left := pnBottom.Width - btnOk.Width - 8; 
end;

procedure TfrmIntretinTipStoc.pnTopResize(Sender: TObject);
begin
  btnStergeProd.Left := pnTop.Width - btnStergeProd.Width - 8;
  btnAddProdus.Left := btnStergeProd.Left - btnAddProdus.Width - 8;
end;

procedure TfrmIntretinTipStoc.edtNivelStocKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if Key = VK_DELETE then
    DBSetFieldValue(QryTipStoc, 'id_gest_nivel_stoc', null);
end;

procedure TfrmIntretinTipStoc.edtGrupaStocKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if Key = VK_DELETE then
    DBSetFieldValue(QryTipStoc, 'id_gest_grupa_stoc', null);
end;

procedure TfrmIntretinTipStoc.TreeListNivStocCustomDrawDataCell(
  Sender: TcxCustomTreeList; ACanvas: TcxCanvas;
  AViewInfo: TcxTreeListEditCellViewInfo; var ADone: Boolean);
begin
 if AViewInfo.Focused then
   ACanvas.Font.Style := ACanvas.Font.Style + [fsBold];
end;

procedure TfrmIntretinTipStoc.TreeListNivStocFocusedNodeChanged(
  Sender: TcxCustomTreeList; APrevFocusedNode,
  AFocusedNode: TcxTreeListNode);
begin
  FIdSelectedNivelStoc := -1;
  if AFocusedNode <> nil then
    FIdSelectedNivelStoc := AFocusedNode.Values[TreeListNivStocid_gest_nivel_stoc.ItemIndex];
  cxGridTipStocDBTableView1.Invalidate(True);
end;

end.
