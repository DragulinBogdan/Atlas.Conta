unit BugetFisaUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxStyles, 
  cxGraphics, 
  cxEdit, DB, cxGridLevel, cxClasses, cxControls,
  cxGridCustomView, cxGridCustomTableView, cxGridTableView,
  cxGrid, ExtCtrls, dxmdaset, ZDataSet, cxTextEdit,
  cxMaskEdit, cxDropDownEdit, cxImageComboBox, cxGroupBox,
  cxRepartitorPanel, cxContainer, cxLabel, cxTL, cxInplaceContainer,
  cxDBTL, cxTLData, cxGridBandedTableView, cxGridDBBandedTableView,
  AlopInfo, cxLookAndFeelPainters, 
  cxDataStorage, cxDBData,
  ZAbstractRODataset, ZAbstractDataset,
  cxTLdxBarBuiltInMenu,
  cxLookAndFeels, cxCustomData, cxFilter, cxData;

type
  TfrmFisaBugetara = class(TForm)
    pnTop: TPanel;
    pnClient: TPanel;
    GridBugetar: TcxGridDBBandedTableView;
    GridBugetarL: TcxGridLevel;
    cxGrid: TcxGrid;
    qryFisaBuget: TZQuery;
    MemLista: TdxMemData;
    DTLista: TDataSource;
    GridBugetarID_BUGET_FISA_BUGET: TcxGridDBBandedColumn;
    GridBugetarCOD_FUNCTIONAL: TcxGridDBBandedColumn;
    GridBugetarCOD_ECONOMIC: TcxGridDBBandedColumn;
    GridBugetarID_OI_UNITATI: TcxGridDBBandedColumn;
    GridBugetarID_OI_PROIECTE: TcxGridDBBandedColumn;
    GridBugetarNR_CRT: TcxGridDBBandedColumn;
    GridBugetarDATA: TcxGridDBBandedColumn;
    GridBugetarBUGET_ANUAL: TcxGridDBBandedColumn;
    GridBugetarBUGET_TRIM: TcxGridDBBandedColumn;
    GridBugetarANGAJAMENT: TcxGridDBBandedColumn;
    GridBugetarFURNIZOR: TcxGridDBBandedColumn;
    GridBugetarEXPLICATII: TcxGridDBBandedColumn;
    GridBugetarORDONANTARE: TcxGridDBBandedColumn;
    GridBugetarID_BUGET: TcxGridDBBandedColumn;
    GridBugetarID_ANGAJAMENT: TcxGridDBBandedColumn;
    GridBugetarID_ORDONANTARE: TcxGridDBBandedColumn;
    pnBottom: TPanel;
    cxLabel1: TcxLabel;
    RPFunctional: TcxRepartitorPanel;
    edCategorie: TcxImageComboBox;
    lbDefalcare: TcxLabel;
    RPProiect: TcxRepartitorPanel;
    cxLabel2: TcxLabel;
    RPEconomic: TcxRepartitorPanel;
    cxTreeUnitati: TcxDBTreeList;
    cxTreeUnitatiID_OI_UNITATI: TcxDBTreeListColumn;
    cxTreeUnitatiID_OI_UNITATI_TIPURI: TcxDBTreeListColumn;
    cxTreeUnitatiID_PARINTE: TcxDBTreeListColumn;
    cxTreeUnitatiDENUMIRE: TcxDBTreeListColumn;
    cxTreeUnitatiDESCRIERE: TcxDBTreeListColumn;
    cxTreeUnitatiUNITATATEA_URMARITA: TcxDBTreeListColumn;
    cxTreeUnitatiNUME_ORDONANTATOR: TcxDBTreeListColumn;
    cxTreeUnitatiID_UTILIZATORI: TcxDBTreeListColumn;
    cxTreeUnitatiSTARE: TcxDBTreeListColumn;
    cxTreeUnitatiUNITATEA_CENTRALIZATOARE: TcxDBTreeListColumn;
    cxTreeUnitatiBANCA: TcxDBTreeListColumn;
    cxTreeUnitatiBANCA_COD: TcxDBTreeListColumn;
    cxTreeUnitatiBANCA_CONT: TcxDBTreeListColumn;
    cxTreeUnitatiCOD_FUNCTIONAL: TcxDBTreeListColumn;
    cxTreeProiecte: TcxDBTreeList;
    cxTreeProiecteID_OI_PROIECTE: TcxDBTreeListColumn;
    cxTreeProiecteID_OI_TIPURI_PROIECTE: TcxDBTreeListColumn;
    cxTreeProiecteID_PARINTE: TcxDBTreeListColumn;
    cxTreeProiecteDENUMIRE: TcxDBTreeListColumn;
    cxTreeProiecteDESCRIERE: TcxDBTreeListColumn;
    cxTreeProiecteSTARE: TcxDBTreeListColumn;
    cxTreeProiecteCOD_FUNCTIONAL: TcxDBTreeListColumn;
    cxTreeFunctional: TcxDBTreeList;
    cxTreeFunctionalCOD_FUNCTIONAL: TcxDBTreeListColumn;
    cxTreeFunctionalID_BG_TIPURI_BUGET: TcxDBTreeListColumn;
    cxTreeFunctionalID_OI_UNITATI: TcxDBTreeListColumn;
    cxTreeFunctionalID_BG_PLAN_FUNCTIONAL: TcxDBTreeListColumn;
    cxTreeFunctionalDENUMIRE: TcxDBTreeListColumn;
    cxTreeFunctionalDESCRIERE: TcxDBTreeListColumn;
    cxTreeFunctionalNUMAR_RAND: TcxDBTreeListColumn;
    cxTreeFunctionalID_PARINTE: TcxDBTreeListColumn;
    cxTreeFunctionalCLASA: TcxDBTreeListColumn;
    cxTreeFunctionalCAPITOL: TcxDBTreeListColumn;
    cxTreeFunctionalESTE_LUCRARE: TcxDBTreeListColumn;
    cxTreeFunctionalTIP_BUGET: TcxDBTreeListColumn;
    cxTreeFunctionalESTE_STANDARD: TcxDBTreeListColumn;
    cxTreeEconomic: TcxDBTreeList;
    cxTreeEconomicID_BG_PLAN_ECONOMIC: TcxDBTreeListColumn;
    cxTreeEconomicCOD_ECONOMIC: TcxDBTreeListColumn;
    cxTreeEconomicDENUMIRE: TcxDBTreeListColumn;
    cxTreeEconomicDESCRIERE: TcxDBTreeListColumn;
    cxTreeEconomicNUMAR_RAND: TcxDBTreeListColumn;
    cxTreeEconomicID_PARINTE: TcxDBTreeListColumn;
    cxTreeEconomicCLASA: TcxDBTreeListColumn;
    cxTreeEconomicESTE_LOCAL: TcxDBTreeListColumn;
    cxTreeFunctionalComplet: TcxDBTreeList;
    cxTreeFunctionalCompletCOD_FUNCTIONAL: TcxDBTreeListColumn;
    cxTreeFunctionalCompletID_BG_TIPURI_BUGET: TcxDBTreeListColumn;
    cxTreeFunctionalCompletID_OI_UNITATI: TcxDBTreeListColumn;
    cxTreeFunctionalCompletID_BG_PLAN_FUNCTIONAL: TcxDBTreeListColumn;
    cxTreeFunctionalCompletDENUMIRE: TcxDBTreeListColumn;
    cxTreeFunctionalCompletDESCRIERE: TcxDBTreeListColumn;
    cxTreeFunctionalCompletNUMAR_RAND: TcxDBTreeListColumn;
    cxTreeFunctionalCompletID_PARINTE: TcxDBTreeListColumn;
    cxTreeFunctionalCompletCLASA: TcxDBTreeListColumn;
    cxTreeFunctionalCompletCAPITOL: TcxDBTreeListColumn;
    cxTreeFunctionalCompletESTE_LUCRARE: TcxDBTreeListColumn;
    cxTreeFunctionalCompletTIP_BUGET: TcxDBTreeListColumn;
    cxTreeFunctionalCompletESTE_STANDARD: TcxDBTreeListColumn;
    cxTreeFunctionalCompletID_OI_PROIECTE: TcxDBTreeListColumn;
    cxTreeFunctionalCompletCOD_ECRAN: TcxDBTreeListColumn;
    GridBugetarDISP_ANUAL: TcxGridDBBandedColumn;
    GridBugetarDISP_TRIM: TcxGridDBBandedColumn;
    procedure qryFisaBugetAfterOpen(DataSet: TDataSet);
    procedure edCategoriePropertiesChange(Sender: TObject);
    procedure RPFunctionalPopupInitPopup(Sender: TObject);
    procedure cxTreeFunctionalDblClick(Sender: TObject);
    procedure cxTreeFunctionalKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure cxTreeFunctionalDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure cxTreeEconomicDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure cxTreeFunctionalCompletDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure FormCreate(Sender: TObject);
    procedure RPFunctionalValidate(Sender: TObject;
      var AKeyValue: Variant);
    procedure FormActivate(Sender: TObject);
    procedure RPEconomicValidate(Sender: TObject; var AKeyValue: Variant);
    procedure edCategoriePropertiesValidate(Sender: TObject;
      var DisplayValue: Variant; var ErrorText: TCaption;
      var Error: Boolean);
    procedure RPProiectValidate(Sender: TObject; var AKeyValue: Variant);
    procedure qryFisaBugetNewRecord(DataSet: TDataSet);
    procedure MemListaNewRecord(DataSet: TDataSet);
    procedure GridBugetarFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure MemListaBeforeDelete(DataSet: TDataSet);
    procedure cxTreeFunctionalCompletCustomDrawDataCell(
      Sender: TcxCustomTreeList; ACanvas: TcxCanvas;
      AViewInfo: TcxTreeListEditCellViewInfo; var ADone: Boolean);
  private
    { Private declarations }
    procedure RefreshMessage(var Message : TMessage); message WM_USER + 1;
    procedure SumaAngajamentChange(Sender : TField);
    procedure DataEmitereChange(Sender : TField);
  public
    { Public declarations }
    Info : TfrmALOPInfo;
    procedure ReadDataSet;

    procedure SetNextControl;
    procedure ActivateGrid;
  end;

var
  frmFisaBugetara: TfrmFisaBugetara;

implementation

uses
  dateUnit, CommonDBVar;

{$R *.dfm}

{ TfrmFisaBugetara }

procedure TfrmFisaBugetara.ReadDataSet;
begin
  if (RPFunctional.KeyValue = Unassigned) or
     (RPEconomic.KeyValue = Unassigned) or
     (edCategorie.EditValue <> 0 and RPProiect.KeyValue = Unassigned)
  then Exit;
  with qryFisaBuget do begin
    Close;
    Params.ParamByName('codFunctional').Value := RPFunctional.CodValue;
    Params.ParamByName('codEconomic').Value := RPEconomic.CodValue;
    if edCategorie.EditValue = 2 then
      Params.ParamByName('idUnitate').Value := RPProiect.CodValue
    else
      Params.ParamByName('idUnitate').Value := null;
    if edCategorie.EditValue = 1 then
      Params.ParamByName('idProiecte').Value := RPProiect.CodValue
    else
      Params.ParamByName('idProiecte').Value := null;
    Open;
  end;
end;

procedure TfrmFisaBugetara.qryFisaBugetAfterOpen(DataSet: TDataSet);
begin
  with Info do begin
    CodFunctional := RPFunctional.CodValue;
    CodEconomic := RPEconomic.CodValue;
  end;
  if MemLista.FindField('ANGAJAMENT') <> nil then
    MemLista.FieldByName('ANGAJAMENT').OnValidate := nil;
  if MemLista.FindField('DATA') <> nil then
    MemLista.FieldByName('DATA').OnValidate := nil;
  MemLista.Active := False;
  MemLista.LoadFromDataSet(qryFisaBuget);
  qryFisaBuget.Active := False;
  MemLista.FieldByName('ANGAJAMENT').OnValidate := SumaAngajamentChange;
  MemLista.FieldByName('DATA').OnValidate := DataEmitereChange;
end;

procedure TfrmFisaBugetara.edCategoriePropertiesChange(Sender: TObject);
begin
  lbDefalcare.Visible := not(edCategorie.ItemIndex = 0);
  RPProiect.Visible := lbDefalcare.Visible;
  case edCategorie.EditValue of
    0 : begin
    end;
    1 : begin
      lbDefalcare.Caption := 'Proiect :';
      RPProiect.CodField := 'ID_OI_PROIECTE';
      RPProiect.KeyField := 'ID_OI_PROIECTE';
      RPProiect.ListField := 'DENUMIRE';
      RPProiect.PopupEdit.PopupControl := cxTreeProiecte;
    end;
    2 : begin
      lbDefalcare.Caption := 'Unitate :';
      RPProiect.CodField := 'ID_OI_UNITATI';
      RPProiect.KeyField := 'ID_OI_UNITATI';
      RPProiect.ListField := 'DENUMIRE';
      RPProiect.PopupEdit.PopupControl := cxTreeUnitati;
    end;
  end;
end;

procedure TfrmFisaBugetara.RPFunctionalPopupInitPopup(Sender: TObject);
begin
  with TcxPopupEdit(Sender).Properties do begin
    if PopupWidth < TcxPopupEdit(Sender).Width then PopupWidth := TcxPopupEdit(Sender).Width;
  end;
end;

procedure TfrmFisaBugetara.cxTreeFunctionalDblClick(Sender: TObject);
begin
   with TcxDBTreeList(Sender) do
    if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk;
end;

procedure TfrmFisaBugetara.cxTreeFunctionalKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
     cxTreeFunctionalDblClick(TcxDBTreeList(Sender))
  else if Key = VK_ESCAPE then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrCancel;
end;

procedure TfrmFisaBugetara.cxTreeFunctionalDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Values[cxTreeFunctionalCOD_FUNCTIONAL.ItemIndex] + ': '+ANode.Values[cxTreeFunctionalDENUMIRE.ItemIndex];
end;

procedure TfrmFisaBugetara.cxTreeEconomicDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Values[cxTreeEconomicCOD_ECONOMIC.ItemIndex] + ': '+ ANode.Values[cxTreeEconomicDENUMIRE.ItemIndex];
end;

procedure TfrmFisaBugetara.cxTreeFunctionalCompletDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Values[cxTreeFunctionalCompletCod_ecran.ItemIndex] + ': '+ANode.Values[cxTreeFunctionalCompletDENUMIRE.ItemIndex];
end;

procedure TfrmFisaBugetara.FormCreate(Sender: TObject);
begin
  cxTreeFunctionalComplet.FullExpand;
  cxTreeFunctional.FullExpand;
  cxTreeEconomic.FullExpand;

  Info := TfrmALOPInfo.Create(Self);
  with Info do begin
    BorderStyle := bsNone;
    Parent := pnBottom;
    Align := alClient;
    Visible := True;
    HandleRefresh := Self.Handle;
    CodFunctional := '';
    CodEconomic := '';
  end;
end;

procedure TfrmFisaBugetara.RPFunctionalValidate(Sender: TObject;
  var AKeyValue: Variant);
begin
  if (cxTreeFunctionalComplet.FocusedNode.Texts[cxTreeFunctionalCompletID_OI_UNITATI.ItemIndex] <> '')  then begin
    edCategorie.EditValue := 2;
    RPProiect.Tag := cxTreeFunctionalComplet.FocusedNode.Values[cxTreeFunctionalCompletID_OI_UNITATI.ItemIndex];
    RPProiect.EditInput.Text := cxTreeFunctionalComplet.FocusedNode.Texts[cxTreeFunctionalCompletID_OI_UNITATI.ItemIndex];
    RPProiect.ForceValidateEditText;
  end
  else if (cxTreeFunctionalComplet.FocusedNode.Texts[cxTreeFunctionalCompletID_OI_PROIECTE.ItemIndex] <> '') then begin
    edCategorie.EditValue := 1;
    RPProiect.Tag := cxTreeFunctionalComplet.FocusedNode.Values[cxTreeFunctionalCompletID_OI_PROIECTE.ItemIndex];
    RPProiect.EditInput.Text := cxTreeFunctionalComplet.FocusedNode.Texts[cxTreeFunctionalCompletID_OI_PROIECTE.ItemIndex];
    RPProiect.ForceValidateEditText;
  end
  else begin
    edCategorie.EditValue := 0;
    RPProiect.Tag := -1;
  end;
  SetNextControl;
  ReadDataSet;
end;

procedure TfrmFisaBugetara.SetNextControl;
  procedure SetActiveControl(AControl: TWinControl);
   begin
       if Self.Visible and AControl.Visible and AControl.Enabled then AControl.SetFocus
       else if AControl.Visible and AControl.Enabled  then Self.ActiveControl := AControl;
   end;
begin
  if not Self.Visible or not Self.Active then Exit;
  if RPFunctional.KeyValue <> Unassigned then
    if RPEconomic.KeyValue <> Unassigned then
      if (edCategorie.EditValue = 0) or ( RPProiect.KeyValue <> Unassigned) then SetActiveControl(cxGrid)
                                                                     else SetActiveControl(edCategorie)
    else SetActiveControl(RPEconomic)
  else SetActiveControl(RPFunctional);
  ActivateGrid;
end;

procedure TfrmFisaBugetara.ActivateGrid;
begin
  if (RPFunctional.KeyValue = Unassigned) or
     (RPEconomic.KeyValue = Unassigned) or
     (edCategorie.EditValue <> 0 and RPProiect.KeyValue = Unassigned)
  then cxGrid.Enabled := False
  else cxGrid.Enabled := True;
//  cxGrid.Enabled := (RPFunctional.KeyValue<>Unassigned) and (RPEconomic.KeyValue<>Unassigned)  and (edCategorie.EditValue = 0 or RPProiect.KeyValue<>Unassigned);
  if cxGrid.Enabled then
    GridBugetar.Styles.Background.Color := clWindow
  else
    GridBugetar.Styles.Background.Color := clBtnFace;
end;

procedure TfrmFisaBugetara.FormActivate(Sender: TObject);
begin
  SetNextControl;
end;

procedure TfrmFisaBugetara.RPEconomicValidate(Sender: TObject;
  var AKeyValue: Variant);
begin
  SetNextControl;
  ReadDataSet;
end;

procedure TfrmFisaBugetara.edCategoriePropertiesValidate(Sender: TObject;
  var DisplayValue: Variant; var ErrorText: TCaption; var Error: Boolean);
begin
  SetNextControl;
  ReadDataSet;
end;

procedure TfrmFisaBugetara.RPProiectValidate(Sender: TObject;
  var AKeyValue: Variant);
begin
  SetNextControl;
  ReadDataSet;
end;

procedure TfrmFisaBugetara.qryFisaBugetNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('COD_FUNCTIONAL').Value := RPFunctional.CodValue;
  DataSet.FieldByName('COD_ECONOMIC').Value := RPEconomic.CodValue;
  if edCategorie.EditValue = 2 then
    DataSet.FieldByName('ID_OI_UNITATI').Value := RPProiect.CodValue
  else
    DataSet.FieldByName('ID_OI_UNITATI').Value := null;
  if edCategorie.EditValue = 1 then
    DataSet.FieldByName('ID_OI_PROIECTE').Value := RPProiect.CodValue
  else
    DataSet.FieldByName('ID_OI_PROIECTE').Value := null;

end;

procedure TfrmFisaBugetara.MemListaNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('COD_FUNCTIONAL').Value := RPFunctional.CodValue;
  DataSet.FieldByName('COD_ECONOMIC').Value := RPEconomic.CodValue;
  if edCategorie.EditValue = 2 then
    DataSet.FieldByName('ID_OI_UNITATI').Value := RPProiect.CodValue
  else
    DataSet.FieldByName('ID_OI_UNITATI').Value := null;
  if edCategorie.EditValue = 1 then
    DataSet.FieldByName('ID_OI_PROIECTE').Value := RPProiect.CodValue
  else
    DataSet.FieldByName('ID_OI_PROIECTE').Value := null;
end;

procedure TfrmFisaBugetara.GridBugetarFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
var
  idAng, idOrd : Integer;
  SumaAng, SumaOrd : Currency;
  dataEmitere : TDateTime;
begin
  if AFocusedRecord = nil then Exit;
  if not AFocusedRecord.IsData then Exit; 
  SumaAng := GetCurrency(AFocusedRecord, GridBugetarANGAJAMENT.Index);
  Info.AngEnabled := (SumaAng <> 0);
  SumaOrd := GetCurrency(AFocusedRecord, GridBugetarORDONANTARE.Index);
  Info.OrdEnabled := (SumaOrd <> 0);
  idAng := GetInteger(AFocusedRecord, GridBugetarID_ANGAJAMENT.Index);
  Info.IdAngajament := idAng;
  dataEmitere := GetInteger(AFocusedRecord, GridBugetarDATA.Index);
  Info.DataEmitere := dataEmitere;
  idOrd := GetInteger(AFocusedRecord, GridBugetarID_ORDONANTARE.Index);
  Info.IdOrdonantare := idOrd;
end;

procedure TfrmFisaBugetara.RefreshMessage(var Message: TMessage);
var
  lTopIndex : Integer;
  lFocusedRowIndex, lFocusedColumnIndex : Integer;
begin
  lTopIndex := GridBugetar.Controller.TopRecordIndex;
  lFocusedColumnIndex := GridBugetar.Controller.FocusedColumnIndex;
  lFocusedRowIndex := GridBugetar.Controller.FocusedRowIndex;
  ReadDataSet;
  GridBugetar.Controller.FocusedColumnIndex := lFocusedColumnIndex;
  GridBugetar.Controller.FocusedRowIndex := lFocusedRowIndex;
  GridBugetar.Controller.TopRecordIndex := lTopIndex;
end;


procedure TfrmFisaBugetara.SumaAngajamentChange(Sender: TField);
begin
  Info.SumaAngajament := Sender.AsCurrency;
end;

procedure TfrmFisaBugetara.DataEmitereChange(Sender: TField);
begin
  Info.DataEmitere := Sender.AsDateTime;
end;

procedure TfrmFisaBugetara.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmFisaBugetara.MemListaBeforeDelete(DataSet: TDataSet);
begin
  if Info.IdAngajament > 0 then Info.StergeAngajament;
  if Info.IdOrdonantare > 0 then Info.StergeOrdonantare;

  PostMessage(Handle, WM_USER+1, 0, 0 );
end;

procedure TfrmFisaBugetara.cxTreeFunctionalCompletCustomDrawDataCell(
  Sender: TcxCustomTreeList; ACanvas: TcxCanvas;
  AViewInfo: TcxTreeListEditCellViewInfo; var ADone: Boolean);
begin
  ACanvas.Font.Style := ACanvas.Font.Style - [fsBold];
  if AViewInfo.Node.Texts[cxTreeFunctionalCompletID_OI_UNITATI.ItemIndex] <> '' then
    ACanvas.Font.Style := ACanvas.Font.Style + [fsBold];
 if AViewInfo.Node.Texts[cxTreeFunctionalCompletID_OI_PROIECTE.ItemIndex] <> '' then
    ACanvas.Font.Style := ACanvas.Font.Style + [fsBold];
end;

end.
