unit StockUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  dxCntner, dxEditor, dxExEdtr, dxEdLib, Buttons, dxTL, dxTLClms,
  Db, ImgList, StdCtrls, dxDBELib, dxDBCtrl, dxDBGrid, dxDBTLCl, dxGrClms,
  Menus, cxLookAndFeelPainters, cxButtons,
  cxGraphics,
  cxLookAndFeels, cxControls, cxContainer, cxEdit, cxTextEdit, cxMaskEdit,
  cxDropDownEdit, cxImageComboBox, cxMemo, cxDBEdit, cxStyles, cxCustomData,
  cxFilter, cxData, cxDataStorage, cxNavigator, cxDBData, cxGridCustomTableView,
  cxGridTableView, cxGridDBTableView, cxGridLevel, cxClasses, cxGridCustomView,
  cxGrid, dxDateRanges, cxDataControllerConditionalFormattingRulesManagerDialog,
  dxScrollbarAnnotations;

type
  TfrmStock = class(TForm)
    SemnImagini: TImageList;
    Imagini: TImageList;
    GrDescriereStock: TGroupBox;
    edDocument: TcxImageComboBox;
    edTipIntr: TcxImageComboBox;
    edSemn: TcxImageComboBox;
    BtnAdd: TcxButton;
    BtnDelete: TcxButton;
    Label1: TLabel;
    edDenumire: TcxDBMaskEdit;
    Label2: TLabel;
    edDescriere: TcxDBMemo;
    edTipMaterial: TcxImageComboBox;
    BtnModificare: TcxButton;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    viewDefaDoc: TcxGridDBTableView;
    nivelDefaDoc: TcxGridLevel;
    gridDefaDoc: TcxGrid;
    viewDefaDocID_GEST_TIP_DOCUM: TcxGridDBColumn;
    viewDefaDocPREDATOR: TcxGridDBColumn;
    viewDefaDocID_GEST_TIP_MATERIAL: TcxGridDBColumn;
    viewDefaDocSEMN: TcxGridDBColumn;
    viewDefaDocID_GEST_TIP_STOC: TcxGridDBColumn;
    procedure edDocumentChange(Sender: TObject);
    procedure BtnAddClick(Sender: TObject);
    procedure BtnDeleteClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure BtnModificareClick(Sender: TObject);
    procedure viewDefaDocFocusedRecordChanged(Sender: TcxCustomGridTableView;
      APrevFocusedRecord, AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
  private
    { Private declarations }
    procedure PostRecord(const AAppend: Boolean);
  public
    { Public declarations }
  end;

implementation

{$R *.DFM}

uses
  dxCompsUtile, ZeosDBUtile, Variants, DateUnit;

procedure TfrmStock.edDocumentChange(Sender: TObject);
begin
  btnAdd.Enabled := ValueHasValue(edDocument.EditValue) or ValueHasValue(edTipIntr.EditValue) or
                    ValueHasValue(edSemn.EditValue) or ValueHasValue(edTipMaterial.EditValue);
  if BtnAdd.Enabled then
    btnAdd.Enabled  := not frmData.QryDefaStock.Locate('id_gest_tip_docum;id_gest_tip_material;predator;semn',
                          VarArrayOf([edDocument.EditValue, edTipMaterial.EditValue, edTipIntr.EditValue, edSemn.EditValue]), []);
  BtnDelete.Enabled := DBHasRecord(frmData.QryDefaStock);
end;

procedure TfrmStock.BtnAddClick(Sender: TObject);
begin
  PostRecord(True);
end;

procedure TfrmStock.BtnDeleteClick(Sender: TObject);
begin
  viewDefaDoc.DataController.DeleteFocused;
end;

procedure TfrmStock.FormCreate(Sender: TObject);
begin
  FillImageCombo(edTipMaterial.Properties, 'spNmclTipMaterial', 0, 1, True, 'Indiferent');
  viewDefaDocID_GEST_TIP_MATERIAL.Properties.Assign(edTipMaterial.Properties);
  viewDefaDOC.DataController.Filter.Root.AddItem(viewDefaDocID_GEST_TIP_STOC, foEqual, FrmData.QryTipStock['ID_GEST_TIP_STOC'], FrmData.QryTipStock['DENUMIRE']);
end;

procedure TfrmStock.PostRecord(const AAppend: Boolean);
begin
  frmData.QryDefaStock.DisableControls;
  try
    if AAppend then frmData.QryDefaStock.Append else frmData.QryDefaStock.Edit;
    frmData.QryDefaStock['ID_GEST_TIP_DOCUM']     := edDocument.EditValue;
    frmData.QryDefaStock['ID_GEST_TIP_MATERIAL']  := edTipMaterial.EditValue;
    frmData.QryDefaStock['PREDATOR']              := edTipIntr.EditValue;
    frmData.QryDefaStock['SEMN']                  := edSemn.EditValue;
    frmData.QryDefaStock.Post;
  finally
    frmData.QryDefaStock.EnableControls;
  end;
end;

procedure TfrmStock.viewDefaDocFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord; ANewItemRecordFocusingChanged: Boolean);
begin
  if Assigned(AFocusedRecord) and AFocusedRecord.IsData then begin
    edDocument.EditValue    := AFocusedRecord.Values[viewDefaDocID_GEST_TIP_DOCUM.Index];
    edTipMaterial.EditValue := AFocusedRecord.Values[viewDefaDocID_GEST_TIP_MATERIAL.Index];
    edTipIntr.EditValue     := AFocusedRecord.Values[viewDefaDocPREDATOR.Index];
    edSemn.EditValue        := AFocusedRecord.Values[viewDefaDocSEMN.Index];
  end;
end;

procedure TfrmStock.BtnModificareClick(Sender: TObject);
begin
  PostRecord(False);
end;

end.
