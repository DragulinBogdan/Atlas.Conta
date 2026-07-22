  unit Gest_StockProd;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  Db, ImgList, StdCtrls, ZDataSet,
  cxControls, cxContainer,
  cxEdit, cxTextEdit, cxDBEdit, cxMemo, cxGroupBox, cxGraphics, cxMaskEdit,
  cxDropDownEdit, cxImageComboBox, 
  cxDataStorage, cxDBData, cxClasses, cxGridCustomView,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGrid,
  cxGridLevel, Menus, cxLookAndFeelPainters, cxButtons, ExtCtrls,
  cxGridCustomPopupMenu, cxGridPopupMenu, DegradePanel, 
  ZAbstractRODataset, ZAbstractDataset,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, cxNavigator;


const WM_RecalcWidth = WM_USER + 1;

type
  TfrmGestStockProd = class(TForm)
    SemnImagini: TImageList;
    Imagini: TImageList;
    Label1: TLabel;
    Label2: TLabel;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    edDenumireDoc: TcxDBTextEdit;
    DTInfluenta: TDataSource;
    edDescriereDoc: TcxDBMemo;
    GrDescriereStock: TcxGroupBox;
    edTipStoc: TcxImageComboBox;
    gridInfluente: TcxGrid;
    nivelInfluente: TcxGridLevel;
    viewInfluente: TcxGridDBTableView;
    viewInfluentePREDATOR: TcxGridDBColumn;
    viewInfluenteSEMN: TcxGridDBColumn;
    viewInfluenteID_GEST_DEFA_STOC_TIP_PRODUSE: TcxGridDBColumn;
    viewInfluenteID_GEST_TIP_STOC: TcxGridDBColumn;
    viewInfluenteID_GEST_TIP_PRODUSE: TcxGridDBColumn;
    viewInfluenteSEMN_ITEMS: TcxGridDBColumn;
    edTipPredator: TcxImageComboBox;
    edTipProdus: TcxImageComboBox;
    edSemn: TcxImageComboBox;
    edSemnPozitie: TcxImageComboBox;
    BtnAdd: TcxButton;
    BtnModificare: TcxButton;
    BtnDelete: TcxButton;
    cxGridPopupMenu: TcxGridPopupMenu;
    qryInfluenta: TZQuery;
    qryInfluentaID_GEST_DEFA_STOC_TIP_PRODUSE: TIntegerField;
    qryInfluentaID_GEST_TIP_STOC: TIntegerField;
    qryInfluentaID_GEST_TIP_DOCUM: TIntegerField;
    qryInfluentaPREDATOR: TIntegerField;
    qryInfluentaID_GEST_TIP_PRODUSE: TIntegerField;
    qryInfluentaSEMN: TIntegerField;
    qryInfluentaSEMN_ITEMS: TIntegerField;
    qryInfluentaDENUMIRE: TWideStringField;
    qryInfluentaDESCRIERE: TWideStringField;
    pnTop: TDegradePanel;
    procedure BtnAddClick(Sender: TObject);
    procedure BtnDeleteClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure BtnModificareClick(Sender: TObject);
    procedure viewInfluenteColumnSizeChanged(
      Sender: TcxGridTableView; AColumn: TcxGridColumn);
    procedure FormShow(Sender: TObject);
    procedure FormResize(Sender: TObject);
    procedure viewInfluenteFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure edTipStocPropertiesChange(Sender: TObject);
  private
    FModified,
    FIsInInsert: Boolean;
    FIdGestTipDocum: Integer;
    FPredator: Integer;
    { Private declarations }
    procedure  WMRecalcWidth(var Message : TMessage); message WM_RecalcWidth;
    function NotExists(ATipStoc, APredator, ATipProdus, ASemn,
      ASemnPozitie: Variant): Boolean;
    procedure SetIdGestTipDocum(const Value: Integer);
    procedure SetPredator(const Value: Integer);
  public
    procedure RecalcEditorSize;
    procedure RefreshDataSet;
    property Modified: Boolean read FModified write FModified;
    property IdGestTipDocum : Integer read FIdGestTipDocum write SetIdGestTipDocum;
    property Predator : Integer read FPredator write SetPredator;
    { Public declarations }
  end;


procedure IntretinereGestDefaStockProdus(lIdGestTipDocum : Integer; lPredator : Integer);

implementation

{$R *.DFM}

uses
  dxCompsUtile, DateUnit, FormulareUnit, CommonDBVar, Variants;

procedure IntretinereGestDefaStockProdus(lIdGestTipDocum : Integer; lPredator : Integer);
var
  aFrm : TfrmGestStockProd;
begin
  aFrm := TfrmGestStockProd(GetNewForm(TfrmGestStockProd));
  with aFrm do
    try
      Predator := lPredator;
      IdGestTipDocum := lIdGestTipDocum;
      ShowModal;
    finally
      Free;
    end;
end;


procedure TfrmGestStockProd.BtnAddClick(Sender: TObject);
begin
  with qryInfluenta do begin
    FIsInInsert := True;
    Append;
    FieldByName('ID_GEST_TIP_STOC').Value    := edTipStoc.EditValue; 
    FieldByName('PREDATOR').Value            := edTipPredator.EditValue;
    FieldByName('ID_GEST_TIP_PRODUSE').Value := edTipProdus.EditValue;
    FieldByName('SEMN').Value                := edSemn.EditValue;
    FieldByName('SEMN_ITEMS').Value        := edSemnPozitie.EditValue;
    Post;
    FIsInInsert := False;
  end;
  FModified := True;
  edTipStocPropertiesChange(edTipStoc);
end;

procedure TfrmGestStockProd.BtnDeleteClick(Sender: TObject);
var lNode : TcxCustomGridRecord;
begin
  lNode := viewInfluente.Controller.FocusedRecord;
  if (Assigned(lNode) and lNode.IsData) and
     (MessageDlg('Doriti stergerea pozitie de calcul pentru stocuri ?', mtConfirmation, [mbYes, mbNo], 0) = mrYes) then begin
     viewInfluente.BeginUpdate;
     qryInfluenta.Delete;
     viewInfluente.EndUpdate;
     edTipStocPropertiesChange(edTipStoc);
  end;
end;

procedure TfrmGestStockProd.FormCreate(Sender: TObject);
begin
  FillImageCombo(edTipProdus.Properties, frmData.qryGESTTipProduse, 'ID_GEST_TIP_PRODUSE', 'DENUMIRE');
  viewInfluenteID_GEST_TIP_PRODUSE.Assign(edTipProdus.Properties);

  FillImageCombo(edTipStoc.Properties, frmData.qryGESTTipStoc, 'ID_GEST_TIP_STOC', 'DESCRIERE');
  viewInfluenteID_GEST_TIP_STOC.Properties.Assign(edTipStoc.Properties);
end;

procedure TfrmGestStockProd.BtnModificareClick(Sender: TObject);
begin
  with qryInfluenta do begin
    Edit;
    FieldByName('ID_GEST_TIP_STOC').Value    := edTipStoc.EditValue; 
    FieldByName('PREDATOR').Value            := edTipPredator.EditValue;
    FieldByName('ID_GEST_TIP_PRODUSE').Value := edTipProdus.EditValue;
    FieldByName('SEMN').Value                := edSemn.EditValue;
    FieldByName('SEMN_ITEMS').Value        := edSemnPozitie.EditValue;
    Post;
  end;
  FModified := True;
  edTipStocPropertiesChange(edTipStoc);
end;


procedure TfrmGestStockProd.viewInfluenteColumnSizeChanged(
  Sender: TcxGridTableView; AColumn: TcxGridColumn);
begin
  RecalcEditorSize;
end;

procedure TfrmGestStockProd.RecalcEditorSize;
begin
  edTipStoc.Width := viewInfluenteID_GEST_TIP_STOC.Width-1;
  edTipStoc.Left := 19;
  edTipPredator.Width := viewInfluentePREDATOR.Width-1;
  edTipPredator.Left := edTipStoc.Left + edTipStoc.Width + 1;
  edTipProdus.Width := viewInfluenteID_GEST_TIP_PRODUSE.Width-1;
  edTipProdus.Left := edTipPredator.Left + edTipPredator.Width + 1;
  edSemn.Width := viewInfluenteSEMN.Width-1;
  edSemn.Left := edTipProdus.Left + edTipProdus.Width + 1;
  edSemnPozitie.Width := viewInfluenteSEMN_ITEMS.Width-1;
  edSemnPozitie.Left := edSemn.Left + edSemn.Width + 1;
end;

procedure TfrmGestStockProd.FormShow(Sender: TObject);
begin
  RecalcEditorSize;
end;

procedure TfrmGestStockProd.FormResize(Sender: TObject);
begin
  PostMessage(Handle, WM_RecalcWidth, 0, 0 );
end;

procedure TfrmGestStockProd.WMRecalcWidth(var Message: TMessage);
begin
  viewInfluente.ViewInfo.HeaderViewInfo.AssignColumnWidths;
  RecalcEditorSize;
end;

procedure TfrmGestStockProd.viewInfluenteFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
  BtnDelete.Enabled := (not FIsInInsert) and (viewInfluente.ViewData.RowCount > 0) and (Assigned(AFocusedRecord) and (AFocusedRecord.IsData));
  if BtnDelete.Enabled then begin
     edTipProdus.EditValue   := AFocusedRecord.Values[viewInfluenteID_GEST_TIP_PRODUSE.Index];
     edTipStoc.EditValue     := AFocusedRecord.Values[viewInfluenteID_GEST_TIP_STOC.Index];
     edTipPredator.EditValue := AFocusedRecord.Values[viewInfluentePREDATOR.Index];
     edSemn.EditValue        := AFocusedRecord.Values[viewInfluenteSEMN.Index];
     edSemnPozitie.EditValue := AFocusedRecord.Values[viewInfluenteSEMN_ITEMS.Index];
  end;
end;


function TfrmGestStockProd.NotExists(ATipStoc, APredator, ATipProdus, ASemn, ASemnPozitie: Variant): Boolean;
var I: Integer;
begin
  Result := True;
  for I := 0 to viewInfluente.ViewData.RowCount -1 do
    if (viewInfluente.ViewData.Rows[I].Values[viewInfluenteID_GEST_TIP_STOC.Index]= ATipStoc) and
       (viewInfluente.ViewData.Rows[I].Values[viewInfluentePREDATOR.Index]= APredator) and
       (viewInfluente.ViewData.Rows[I].Values[viewInfluenteID_GEST_TIP_PRODUSE.Index]= ATipProdus) and
       (viewInfluente.ViewData.Rows[I].Values[viewInfluenteSEMN.Index]= ASemn) and
       (viewInfluente.ViewData.Rows[I].Values[viewInfluenteSEMN_ITEMS.Index]= ASemnPozitie) then begin
         Result := False;
         Break;
    end;

end;

procedure TfrmGestStockProd.edTipStocPropertiesChange(Sender: TObject);
begin
  BtnAdd.Enabled := (Trim(edTipStoc.Text) > '') and
                    (Trim(edTipPredator.Text) > '') and
                    (Trim(edTipProdus.Text) > '') and
                    (Trim(edSemn.Text) >  '') and
                    (Trim(edSemnPozitie.Text) >  '');
  if BtnAdd.Enabled then
     BtnAdd.Enabled := NotExists(edTipStoc.EditValue, edTipPredator.EditValue,
                    edTipProdus.EditValue, edSemn.EditValue, edSemnPozitie.EditValue);
  BtnDelete.Enabled := (viewInfluente.ViewData.RowCount > 0)
     and (viewInfluente.Controller.FocusedRecord <> nil);

end;

procedure TfrmGestStockProd.RefreshDataSet;
begin
  if qryInfluenta.Active then qryInfluenta.Close;
  qryInfluenta.Params.ParamByName('ID_GEST_TIP_DOCUM').Value :=FIdGestTipDocum;
//  if FPredator = -1 then
  qryInfluenta.Params.ParamByName('PREDATOR').Value := FPredator;  
  qryInfluenta.Params.ParamByName('PREDATOR2').Value := FPredator;
//  else
//    qryInfluenta.Params.ParamByName('PREDATOR').Value := FPredator;
  qryInfluenta.Open;
  //asta face ca la stergere, inserare, update sa se faca numai in tabela asta
//atentie  qryInfluenta.Properties['Unique Table'].Value := 'GEST_DEFA_STOC_TIP_PRODUSE';
end;

procedure TfrmGestStockProd.SetIdGestTipDocum(const Value: Integer);
begin
  FIdGestTipDocum := Value;
  RefreshDataSet;
end;

procedure TfrmGestStockProd.SetPredator(const Value: Integer);
begin
  FPredator := Value;
  edTipPredator.EditValue := FPredator;
  edTipPredator.Properties.ReadOnly := not(FPredator = -1);
end;

end.
