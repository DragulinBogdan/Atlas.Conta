unit SelStockUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, dxCntner, dxDBCtrl, Db, ZDataSet, dxDBTLCl,
  dxExEdtr, dxEditor, 
  ExtCtrls, cxControls, cxPC, 
  cxCustomData, cxGraphics, cxFilter, cxDataStorage, cxEdit,
  cxDBData, cxGridLevel, cxClasses, cxGridCustomView,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGrid,
  cxGridBandedTableView, cxGridDBBandedTableView, cxCheckBox, cxSpinEdit,
  cxImageComboBox, cxCalendar, cxCurrencyEdit, cxGridCustomPopupMenu,
  cxGridPopupMenu, cxLabel, cxContainer, cxDBLabel,
  cxSplitter, cxTextEdit, cxMaskEdit, cxDropDownEdit, Menus, dxmdaset, dxDBGrid, dxGrClms, dxTL,
  cxLookAndFeelPainters, cxButtons,
  ZAbstractRODataset, ZAbstractDataset,
  cxLookAndFeels, cxStyles, cxData, Vcl.ComCtrls, dxCore, cxDateUtils,
  cxNavigator, dxBarBuiltInMenu, dxDateRanges,
  cxDataControllerConditionalFormattingRulesManagerDialog,
  dxScrollbarAnnotations;

const
  WM_DateStock = WM_User + 1;
type
  TfrmSelStock = class(TForm)
    DTStock: TDataSource;
    QryStock: TZQuery;
    MemStock: TdxMemData;
    Panel1: TPanel;
    pnTop: TPanel;
    Panel3: TPanel;
    pnBottom: TPanel;
    pageDesc: TcxPageControl;
    tabFisaCodMat: TcxTabSheet;
    tabStockAll: TcxTabSheet;
    spliterV: TcxSplitter;
    GridIstoricMaterial: TdxDBGrid;
    GridIstoricMaterialPREDATOR: TdxDBGridMaskColumn;
    GridIstoricMaterialPRIMITOR: TdxDBGridMaskColumn;
    GridIstoricMaterialCOD_DOCUM: TdxDBGridMaskColumn;
    GridIstoricMaterialNR_DOCUM: TdxDBGridMaskColumn;
    GridIstoricMaterialDATA_DOCUM: TdxDBGridDateColumn;
    GridIstoricMaterialOPERATOR: TdxDBGridMaskColumn;
    GridIstoricMaterialCANTITATE_BEFORE: TdxDBGridMaskColumn;
    GridIstoricMaterialCANTITATE: TdxDBGridMaskColumn;
    GridIstoricMaterialCANTITATE_AFTER: TdxDBGridMaskColumn;
    GridIstoricMaterialTIP_MATERIAL: TdxDBGridMaskColumn;
    GridIstoricMaterialSEMN: TdxDBGridImageColumn;
    GridIstoricMaterialPRET_UNITAR: TdxDBGridCurrencyColumn;
    GridIstoricMaterialVALOARE: TdxDBGridCurrencyColumn;
    dtFisaMaterial: TDataSource;
    qryFisaMaterial: TZQuery;
    dtStockAll: TDataSource;
    qryStockAll: TZQuery;
    gridStockAll: TdxDBGrid;
    grLevel: TcxGridLevel;
    grStock: TcxGrid;
    GridStock: TcxGridDBBandedTableView;
    GridStockID_GEST_TIP_STOCK: TcxGridDBBandedColumn;
    GridStockID_STOCK_PREDATOR: TcxGridDBBandedColumn;
    GridStockID_STOCK_PRIMITOR: TcxGridDBBandedColumn;
    GridStockSELECTAT: TcxGridDBBandedColumn;
    GridStockCANTITATE_SELECTATA: TcxGridDBBandedColumn;
    GridStockPRODUS: TcxGridDBBandedColumn;
    GridStockTIP_STOCK: TcxGridDBBandedColumn;
    GridStockCANTITATE: TcxGridDBBandedColumn;
    GridStockCANT_PREDATOR: TcxGridDBBandedColumn;
    GridStockCANT_PRIMITOR: TcxGridDBBandedColumn;
    GridStockCONT: TcxGridDBBandedColumn;
    GridStockID_PREDATOR: TcxGridDBBandedColumn;
    GridStockID_PRIMITOR: TcxGridDBBandedColumn;
    GridStockDATACOD: TcxGridDBBandedColumn;
    GridStockNR_DOCUM: TcxGridDBBandedColumn;
    GridStockCODMAT: TcxGridDBBandedColumn;
    GridStockID_ANGAJAMENTE_DEFALCARE: TcxGridDBBandedColumn;
    GridStockID_INITIAL: TcxGridDBBandedColumn;
    GridStockID_UTILIZATORI: TcxGridDBBandedColumn;
    GridStockPRET_UNITAR: TcxGridDBBandedColumn;
    GridStockPRET_RECEPTIE: TcxGridDBBandedColumn;
    GridStockCOTA_TVA: TcxGridDBBandedColumn;
    GridStockTIPMAT: TcxGridDBBandedColumn;
    GridStockDENMAT: TcxGridDBBandedColumn;
    GridStockUM: TcxGridDBBandedColumn;
    GridStockLOHN: TcxGridDBBandedColumn;
    GridStockDATA_COD: TcxGridDBBandedColumn;
    GridStockDATA_EXPIRARE: TcxGridDBBandedColumn;
    GridStockTVA: TcxGridDBBandedColumn;
    GridStockID_GEST_SUMATOR: TcxGridDBBandedColumn;
    GridStockLOT_FABRICATIE: TcxGridDBBandedColumn;
    GridStockUM_SUPLIMENTARA: TcxGridDBBandedColumn;
    GridStockCONVERSIE_UM: TcxGridDBBandedColumn;
    GridStockPRET_RECEPTIE_VALUTA: TcxGridDBBandedColumn;
    GridStockCOD_TARIF_VAMAL: TcxGridDBBandedColumn;
    GridStockTVA_AMANAT: TcxGridDBBandedColumn;
    GridStockADAOS: TcxGridDBBandedColumn;
    GridStockADAOS_IMPUS: TcxGridDBBandedColumn;
    GridStockCATEGORIE_GRUPARE: TcxGridDBBandedColumn;
    GridStockTIP_VALUTA_RECEPTIE: TcxGridDBBandedColumn;
    GridStockCOTA_ADAOS: TcxGridDBBandedColumn;
    GridStockCOTA_ADAOS_IMPUS: TcxGridDBBandedColumn;
    GridStockPRET_RECEPTIE_TVA: TcxGridDBBandedColumn;
    popupGrid: TcxGridPopupMenu;
    gridStockAllid: TdxDBGridMaskColumn;
    gridStockAllgestiune: TdxDBGridColumn;
    gridStockAllstock: TdxDBGridCurrencyColumn;
    gridStockAllpret_receptie: TdxDBGridCurrencyColumn;
    gridStockAllpret_receptie_tva: TdxDBGridCurrencyColumn;
    gridStockAllstockValoric: TdxDBGridCurrencyColumn;
    GridStockCANT_PREDATOR_ZI: TcxGridDBBandedColumn;
    dtStockSum: TDataSource;
    qryStockSum: TZQuery;
    tabStockSum: TcxTabSheet;
    GridStockSumator: TdxDBGrid;
    AtsDBGridMaskColumn1: TdxDBGridMaskColumn;
    AtsDBGridColumn1: TdxDBGridColumn;
    AtsDBGridCurrencyColumn1: TdxDBGridCurrencyColumn;
    AtsDBGridCurrencyColumn2: TdxDBGridCurrencyColumn;
    AtsDBGridCurrencyColumn3: TdxDBGridCurrencyColumn;
    AtsDBGridCurrencyColumn4: TdxDBGridCurrencyColumn;
    tabFisaMaterialSumator: TcxTabSheet;
    GridFisaMagSum: TdxDBGrid;
    AtsDBGridMaskColumn2: TdxDBGridMaskColumn;
    AtsDBGridMaskColumn3: TdxDBGridMaskColumn;
    AtsDBGridMaskColumn4: TdxDBGridMaskColumn;
    AtsDBGridMaskColumn5: TdxDBGridMaskColumn;
    AtsDBGridDateColumn1: TdxDBGridDateColumn;
    AtsDBGridImageColumn1: TdxDBGridImageColumn;
    AtsDBGridMaskColumn6: TdxDBGridMaskColumn;
    AtsDBGridMaskColumn7: TdxDBGridMaskColumn;
    AtsDBGridCurrencyColumn5: TdxDBGridCurrencyColumn;
    AtsDBGridCurrencyColumn6: TdxDBGridCurrencyColumn;
    AtsDBGridMaskColumn8: TdxDBGridMaskColumn;
    AtsDBGridMaskColumn9: TdxDBGridMaskColumn;
    AtsDBGridMaskColumn10: TdxDBGridMaskColumn;
    tabAltMat: TcxTabSheet;
    GridAltSumator: TdxDBGrid;
    DTAcelasiSumator: TDataSource;
    qryAcelasiSumator: TZQuery;
    GridAltSumatorid: TdxDBGridMaskColumn;
    GridAltSumatorgestiune: TdxDBGridColumn;
    GridAltSumatorcodmat: TdxDBGridMaskColumn;
    GridAltSumatorstock: TdxDBGridCurrencyColumn;
    GridAltSumatorpret_receptie: TdxDBGridCurrencyColumn;
    GridAltSumatorpret_receptie_tva: TdxDBGridCurrencyColumn;
    GridAltSumatorstockValoric: TdxDBGridCurrencyColumn;
    GridAltSumatortipmat: TdxDBGridColumn;
    GridAltSumatordenmat: TdxDBGridColumn;
    GridAltSumatorid_gest_sumator: TdxDBGridMaskColumn;
    tabFiseSumator: TcxTabSheet;
    pageSumator: TcxPageControl;
    cxTabControlFise: TcxTabControl;
    AtsDBGrid1: TdxDBGrid;
    AtsDBGridMaskColumn11: TdxDBGridMaskColumn;
    AtsDBGridMaskColumn12: TdxDBGridMaskColumn;
    AtsDBGridMaskColumn13: TdxDBGridMaskColumn;
    AtsDBGridMaskColumn14: TdxDBGridMaskColumn;
    AtsDBGridDateColumn2: TdxDBGridDateColumn;
    AtsDBGridImageColumn2: TdxDBGridImageColumn;
    AtsDBGridMaskColumn15: TdxDBGridMaskColumn;
    AtsDBGridMaskColumn16: TdxDBGridMaskColumn;
    AtsDBGridCurrencyColumn7: TdxDBGridCurrencyColumn;
    AtsDBGridCurrencyColumn8: TdxDBGridCurrencyColumn;
    AtsDBGridMaskColumn17: TdxDBGridMaskColumn;
    AtsDBGridMaskColumn18: TdxDBGridMaskColumn;
    AtsDBGridMaskColumn19: TdxDBGridMaskColumn;
    qryCodMaturi: TZQuery;
    cxDBLabel1: TcxDBLabel;
    cxLabel1: TcxLabel;
    GridStockRecId: TcxGridDBBandedColumn;
    ChkShowAllReady: TcxCheckBox;
    ChkShowNegative: TcxCheckBox;
    ChkShowData: TcxCheckBox;
    chkStockLazi: TcxCheckBox;
    edtChangeDataStoc: TcxCheckBox;
    edtDataStock: TcxDateEdit;
    tabTipStock: TcxTabControl;
    GridStockSEMN_CANTITATE: TcxGridDBBandedColumn;
    ppComenzi: TPopupMenu;
    CmdSelectiePozitie: TMenuItem;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    GridStockOrderCol: TcxGridDBBandedColumn;
    procedure FormCreate(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure edtChangeDataStocClick(Sender: TObject);
    procedure GridStockKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure GridStockMouseUp(Sender: TObject; Button: TMouseButton;
      Shift: TShiftState; X, Y: Integer);
    procedure MemStockFilterRecord(DataSet: TDataSet; var Accept: Boolean);
    procedure QryStockAfterOpen(DataSet: TDataSet);
    procedure ChkShowAllReadyClick(Sender: TObject);
    procedure GridStockCustomDrawCell(Sender: TcxCustomGridTableView;
      ACanvas: TcxCanvas; AViewInfo: TcxGridTableDataCellViewInfo;
      var ADone: Boolean);
    procedure GridStockEditKeyDown(Sender: TcxCustomGridTableView;
      AItem: TcxCustomGridTableItem; AEdit: TcxCustomEdit; var Key: Word;
      Shift: TShiftState);
    procedure spliterVRestore(Sender: TObject);
    procedure GridStockFocusedRecordChanged(Sender: TcxCustomGridTableView;
      APrevFocusedRecord, AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure pageDescChange(Sender: TObject);
    procedure BtnOkClick(Sender: TObject);
    procedure cxTabControlFiseChange(Sender: TObject);
    procedure spliterVAfterOpen(Sender: TObject);
    procedure tabTipStockChange(Sender: TObject);
    procedure tabTipStockDrawTabEx(AControl: TcxCustomTabControl;
      ATab: TcxTab; Font: TFont);
    procedure edtDataStockPropertiesChange(Sender: TObject);
    procedure CmdSelectiePozitieClick(Sender: TObject);
  private
    FPredator: Integer;
    FAllReady: TStringList;
    FPrimitor: Integer;
    FIdGestDefaDocum: Integer;
    FTipMat: String;
    FDataDoc: TDateTime;
    FDataStoc: TDateTime;
    FtabTipStock : Integer;
    FInLoading : Boolean;
    procedure WMDateStock(var Message : TMessage); message WM_DateStock;
    procedure SetCantitateSelectata(Sender: TField);

    procedure SetTipMat(const Value: String);
    procedure SetDataDoc(const Value: TDateTime);
    procedure SetDataStoc(const Value: TDateTime);
    procedure PopulateFise;
    { Private declarations }
  protected
    procedure CreateHiddenColumns;
  public
    { Public declarations }
    procedure CreateTabStock;
    procedure InitNomenclator;
    procedure InitStock(ACodMatList: TStringList);
    procedure OpenStock;
    procedure OpenNomenclator;
    procedure SetFilterSelectate;
    procedure SetFilterCulegere;
    procedure SetOnlySelect(DataSet: TDataSet; var Accept: Boolean);
    procedure MemStockNomFilterRecord(DataSet: TDataSet; var Accept: Boolean);

    property IdGestDefaDocum: Integer read FIdGestDefaDocum write FIdGestDefaDocum;
    property Predator       : Integer read FPredator write FPredator;
    property Primitor       : Integer read FPrimitor write FPrimitor;
    property DataDoc        : TDateTime read FDataDoc write SetDataDoc;
    property DataStoc       : TDateTime read FDataStoc write SetDataStoc;
    property TipMat         : String  read FTipMat write SetTipMat;
  end;

implementation

{$R *.DFM}

uses
  ZeosDBUtile, DateUnit, CommonDBVar, Variants;

{ TfrmSelStock }

procedure TfrmSelStock.OpenStock;
begin
  FInLoading := True;
  QryStock.Close;
  QryStock.Params.ParamByName('ID_GEST_DEFA_DOCUM').Value := FIdGestDefaDocum;
  QryStock.Params.ParamByName('ID_PREDATOR').Value := FPredator;
  QryStock.Params.ParamByName('ID_PRIMITOR').Value := FPrimitor;
  QryStock.Open;
  CreateHiddenColumns;
  GridStock.ApplyBestFit(nil);
  FInLoading := False; 
end;

procedure TfrmSelStock.FormCreate(Sender: TObject);
begin
  FtabTipStock := -1;
  spliterV.CloseSplitter;
  FAllReady := TStringList.Create;
  FAllReady.Sorted := True;
  FAllReady.Duplicates := dupIgnore;
end;

procedure TfrmSelStock.FormDestroy(Sender: TObject);
begin
  FAllReady.Free;
end;

procedure TfrmSelStock.InitStock(ACodMatList: TStringList);
begin
  pnTop.Visible := True;
  FAllReady.Assign(ACodMatList);
  CreateTabStock;
  FtabTipStock := 0;
  MemStock.OnFilterRecord := MemStockFilterRecord;
end;

procedure TfrmSelStock.edtChangeDataStocClick(Sender: TObject);
begin
  edtDataStock.Enabled := edtChangeDataStoc.Checked;
  if edtChangeDataStoc.Checked then begin
    DataStoc := edtDataStock.Date;
    OpenStock;
  end
  else begin
    DataStoc := DataDoc;
    OpenStock;
  end;
end;

procedure TfrmSelStock.WMDateStock(var Message: TMessage);
begin
  if IsValidDate(edtDataStock.EditValue) and edtChangeDataStoc.Checked then begin
    edtDataStock.ValidateEdit(True);
    DataStoc := edtDataStock.Date;
    OpenStock;
  end;
end;

procedure TfrmSelStock.GridStockKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
var
  I            : Integer;
//  aCodMat      : Integer;
  lRecord      : TcxCustomGridRecord;
  aRecId       : Integer;

  procedure SwitchCantitate;
  begin
    if not (MemStock.State in [dsEdit, dsInsert]) then MemStock.Edit;
    MemStock.FieldByName('SELECTAT').AsBoolean := not MemStock.FieldByName('SELECTAT').AsBoolean;
    if MemStock.FieldByName('SELECTAT').AsBoolean then begin
      if (MemStock.FieldByName('CANTITATE_SELECTATA').AsCurrency = 0) and (MemStock.FieldByName('CANT_PREDATOR_ZI').AsCurrency > 0)  then
         MemStock.FieldByName('CANTITATE_SELECTATA').AsCurrency := MemStock.FieldByName('CANT_PREDATOR_ZI').AsCurrency;
    end
    else
      MemStock.FieldByName('CANTITATE_SELECTATA').AsCurrency := 0;
    MemStock.Post;
  end;

begin
  if (Key = VK_SPACE) and (ssCtrl in Shift) then begin
    with GridStock do begin
      BeginUpdate;
      MemStock.DisableControls;
      try
        lRecord := Controller.FocusedRecord;
        if Controller.SelectedRowCount <> 0 then begin
          for I := 0 to Controller.SelectedRowCount - 1 do begin
            with MemStock do begin
              if Controller.SelectedRows[I] is TcxGridGroupRow then Continue;
              if not MemStock.Locate('RecID', Controller.SelectedRows[I].Values[GridStockRecId.Index], []) then Continue;
              SwitchCantitate;
            end;
          end;
        end
        else
        if Assigned(lRecord) then begin

          //facem cautare dupa keya unica
          if not( lRecord is TcxGridGroupRow) then begin
            aRecId := GetInteger(lRecord, GridStockRecId.Index);
            with MemStock do
             if (FieldByName('RecId').AsInteger <> aRecId) or (Locate('RecId', aRecId, [])) then
                SwitchCantitate;
          end;
        end;
      finally
        MemStock.EnableControls;
        GridStock.EndUpdate;
      end;
    end;
  end;
end;

procedure TfrmSelStock.GridStockMouseUp(Sender: TObject;
  Button: TMouseButton; Shift: TShiftState; X, Y: Integer);
var
  Info: TdxTreeListHitInfo;
  aNode : TdxDBGridNode;
  aUpdateId : Integer;
begin
  Info := TdxDBGrid(Sender).GetHitInfo(Point(X,Y));
  if Info.Column <> GridStockSELECTAT.Index then Exit;
  aNode := TdxDBGridNode(Info.Node);
  if aNode = nil then Exit;
  aUpdateId := aNode.Id;
  if Info.hitType = htLabel then begin
      with MemStock do begin
        if not MemStock.Locate('CODMAT', aUpdateId, []) then Exit;
        if not (MemStock.State in [dsEdit, dsInsert]) then MemStock.Edit;
        MemStock.FieldByName('SELECTAT').AsBoolean := not MemStock.FieldByName('SELECTAT').AsBoolean;
        if MemStock.FieldByName('SELECTAT').AsBoolean and (MemStock.FieldByName('CANTITATE_SELECTATA').AsCurrency = 0) and (MemStock.FieldByName('CANT_PREDATOR_ZI').AsCurrency > 0)  then
            MemStock.FieldByName('CANTITATE_SELECTATA').AsCurrency := MemStock.FieldByName('CANT_PREDATOR_ZI').AsCurrency;
        MemStock.Post;
     end;
  end;
end;

procedure TfrmSelStock.SetOnlySelect(DataSet: TDataSet;
  var Accept: Boolean);
begin
  Accept := DataSet.FieldByName('SELECTAT').AsBoolean;
end;

procedure TfrmSelStock.MemStockFilterRecord(DataSet: TDataSet;
  var Accept: Boolean);
begin
  {Daca Avem nomenclator}
  if (tabTipStock.Tabs.Count = 0) or
    (
     (FtabTipStock >-1) and Assigned(ListStockInfo) and (ListStockInfo.Count > 0) and
     (PStockInfo(ListStockInfo[FtabTipStock])^.Semn = 0)
    )
  then begin
    Accept := Accept and (DataSet.FieldByName('SEMN_CANTITATE').AsInteger = 0);
    Exit;
  end;

  Accept := (ChkShowData.Checked) or (DataSet.FieldByName('DATACOD').AsDateTime <= FDataDoc);
  if (Accept) and (chkStockLaZi.Checked) then
     if (ChkShowNegative.Checked) then
        Accept := (Accept) and ((DataSet.FieldByName('CANT_PREDATOR_ZI').AsCurrency <> 0) or (DataSet.FieldByName('VALOARE').AsCurrency <> 0))
     else
        Accept := (Accept) and (DataSet.FieldByName('CANT_PREDATOR_ZI').AsCurrency > 0);
  if (Accept) and (not ChkShowNegative.Checked) and (not chkStockLaZi.Checked) then
      Accept := (Accept) and (DataSet.FieldByName('CANT_PREDATOR').AsCurrency > 0);

  if (Accept) and (ChkShowAllReady.Checked) then
     Accept := (Accept) and (FAllReady.IndexOf(DataSet.FieldByName('CODMAT').AsString) = -1);
  if (Accept) and ((FtabTipStock >-1) and Assigned(ListStockInfo) and (ListStockInfo.Count > 0)) then
     Accept := (Accept) and
       (DataSet.FieldByName('ID_GEST_TIP_STOCK').AsInteger = PStockInfo(ListStockInfo[FtabTipStock])^.IdGestTipStock)
        and
       (DataSet.FieldByName('PREDATOR').AsInteger = PStockInfo(ListStockInfo[FtabTipStock])^.Predator);
end;

procedure TfrmSelStock.QryStockAfterOpen(DataSet: TDataSet);
var
   lOnFilterRecord :  TFilterRecordEvent;
begin
  with MemStock do begin
    MemStock.Tag := 1;
    try
      Filtered := False;
      lOnFilterRecord := OnFilterRecord;
      OnFilterRecord := nil;
      if Active then
         Close;
      CopyFromDataSet(QryStock);
      FindField('CANTITATE_SELECTATA').OnChange := SetCantitateSelectata;
      SetFilterCulegere();
      OnFilterRecord := lOnFilterRecord;      
    finally
      MemStock.Tag := 0;
    end;
  end;
end;

procedure TfrmSelStock.ChkShowAllReadyClick(Sender: TObject);
begin
  GridStock.BeginUpdate;
  try
    MemStock.Filtered := False;
    MemStock.Filtered := True;
  finally
    GridStock.EndUpdate;
  end;
end;

procedure TfrmSelStock.SetCantitateSelectata(Sender: TField);
begin
  if Sender.AsCurrency > MemStock.FieldByName('CANT_PREDATOR_ZI').AsCurrency then
     if MessageDlg('Nu puteti scoate din stock mai mult decat aveti STOCK !'+MemStock.FieldByName('CANTITATE').AsString+' < '+Sender.AsString+
                   #13#10'Doriti abandonul sumei introduse si intorducerea uneia noi?', mtConfirmation, [mbYes, mbNo], 0) <> mrNo then
        Abort;
  if MemStock.Tag = 0 then
     MemStock.FieldByName('SELECTAT').AsBoolean := Sender.AsCurrency <> 0;
end;

procedure TfrmSelStock.GridStockCustomDrawCell(
  Sender: TcxCustomGridTableView; ACanvas: TcxCanvas;
  AViewInfo: TcxGridTableDataCellViewInfo; var ADone: Boolean);
var
   aStockIni,
   aStockZi,
   aStockEnter : Currency;
   lRecord     : TcxCustomGridRecord;
   aCodMat     : Integer;
   aSemnCantitate : Integer;
begin

  lRecord := AViewInfo.RecordViewInfo.GridRecord;
  if (lRecord = nil) or (AViewInfo.Selected) then Exit;

  aCodMat := GetInteger(lRecord, GridStockCODMAT.Index);
  if FAllReady.IndexOf(IntToStr(ACodMAt)) > -1 then begin
     ACanvas.Brush.Color := clAqua;
     ACanvas.Font.Color  := clBlack;
  end
  else begin
    aStockIni   := GetCurrency(lRecord, GridStockCANTITATE.Index);
    aStockZi    := GetCurrency(lRecord, GridStockCANT_PREDATOR_ZI.Index);
    aStockEnter := GetCurrency(lRecord, GridStockCANTITATE_SELECTATA.Index);
    aSemnCantitate := GetInteger(lRecord, GridStockSEMN_CANTITATE.Index);
    if aSemnCantitate <> 0 then
      if aStockIni < 0 then
        begin
          ACanvas.Brush.Color := clRed;
          ACanvas.Font.Color := clYellow;
        end
      else
      if aStockEnter > 0 then
        begin
          ACanvas.Brush.Color := clYellow;
          ACanvas.Font.Color := clBlack;
        end
      else
      if aStockZi < aStockIni then
        begin
          ACanvas.Brush.Color := clBlue;
          ACanvas.Font.Color := clYellow;
        end;
  end;
end;

procedure TfrmSelStock.GridStockEditKeyDown(Sender: TcxCustomGridTableView;
  AItem: TcxCustomGridTableItem; AEdit: TcxCustomEdit; var Key: Word;
  Shift: TShiftState);
begin
  GridStockKeyDown(Sender, Key, Shift);
end;

procedure TfrmSelStock.spliterVRestore(Sender: TObject);
var
  lRecord : TcxCustomGridRecord;
  aCodMat : Integer;
begin
  lRecord := GridStock.Controller.FocusedRecord;
  if Assigned(lRecord) and (lRecord.IsData) then begin
    aCodMat := GetInteger(lRecord, GridStockCODMAT.Index);
  end;  
  qryFisaMaterial.Close;
  qryStockAll.Close;
  qryAcelasiSumator.Close;
  qryCodMaturi.Close;
  qryFisaMaterial.Params[0].Value := aCodMat;
  qryStockAll.Params[0].Value     := aCodMat;
  qryStockSum.Params[0].Value     := aCodMat;
  qryAcelasiSumator.Params[0].Value := aCodMat;
  qryCodMaturi.Params[0].Value := aCodMat;

  if Assigned(lRecord) then begin
    //aCodMat := GetInteger(lRecord, GridStockCODMAT.Index);
    pageDescChange(nil);
  end;
end;

procedure TfrmSelStock.GridStockFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
var
  aCodMat, aIdTipStoc : Integer;
begin
  if FInLoading then Exit;
  qryFisaMaterial.Close;
  qryStockAll.Close;
  qryAcelasiSumator.Close;
  qryCodMaturi.Close;
  if Assigned(AFocusedRecord) and not (AFocusedRecord is TcxGridGroupRow) then begin
    aCodMat := GetInteger(AFocusedRecord, GridStockCODMAT.Index);
    aIdTipStoc := GetInteger(AFocusedRecord, GridStockID_GEST_TIP_STOCK.Index);
    qryFisaMaterial.Params[0].Value   := aIdTipStoc;
    qryStockAll.Params[2].Value       := aIdTipStoc;
    qryStockSum.Params[2].Value       := aIdTipStoc;
    qryAcelasiSumator.Params[2].Value := aIdTipStoc;

    qryFisaMaterial.Params[1].Value   := aCodMat;
    qryStockAll.Params[0].Value       := aCodMat;
    qryStockSum.Params[0].Value       := aCodMat;
    qryAcelasiSumator.Params[0].Value := aCodMat;
    qryCodMaturi.Params[0].Value      := aCodMat;
    if (not (spliterV.State = ssClosed)) then
       pageDescChange(nil);
  end;
end;

procedure TfrmSelStock.pageDescChange(Sender: TObject);
begin
  if pageDesc.ActivePage = tabFisaCodMat then begin
    if qryFisaMaterial.Active then qryFisaMaterial.Close;
    qryFisaMaterial.Params.ParamByName('tipStock').Value := null;
    qryFisaMaterial.Open;
  end
  else
  if pageDesc.ActivePage = tabStockSum then begin
    if qryStockSum.Active then qryStockSum.Close;
    qryStockSum.Open;
  end
  else
  if pageDesc.ActivePage = tabFisaMaterialSumator then begin
    if qryFisaMaterial.Active then qryFisaMaterial.Close;
    qryFisaMaterial.Params.ParamByName('tipStock').Value := 'S';
    qryFisaMaterial.Open;
  end
  else
  if pageDesc.ActivePage = tabAltMat then begin
     if qryAcelasiSumator.Active then qryAcelasiSumator.Close;
     qryAcelasiSumator.Open
  end
  else
  if pageDesc.ActivePage = tabFiseSumator then begin
     PopulateFise;
     cxTabControlFiseChange(nil);
  end
  else
    qryStockAll.Open;
end;

procedure TfrmSelStock.SetTipMat(const Value: String);
begin
  FTipMat := Value;
  if Trim(FTipMat) > '' then
     GridStock.DataController.Filter.AddItem(nil, GridStockTIPMAT, foLike, FTipMat, 'Grupa : '+FTipMat);
end;

procedure TfrmSelStock.SetDataDoc(const Value: TDateTime);
begin
  FDataDoc := Value;
end;

procedure TfrmSelStock.OpenNomenclator;
begin
  FInLoading := True;
  DTStock.DataSet := nil;
  GridStockCANTITATE_SELECTATA.Visible := False;
  GridStockCANTITATE.Visible := False;
  GridStockCANT_PREDATOR_ZI.Visible := False;
  QryStock.Close;
  QryStock.Open;
  CreateHiddenColumns;
  DTStock.DataSet := MemStock;   
  //GridStock.ApplyBestFit(nil);
  FInLoading := False;
end;

procedure TfrmSelStock.SetDataStoc(const Value: TDateTime);
begin
  FDataStoc := Value;
  if QryStock.Params.FindParam('DATA_STOC') <> nil then
     QryStock.Params.ParamByName('DATA_STOC').Value := FDataStoc;
  qryFisaMaterial.Params[1].Value := FDataStoc;
  qryStockAll.Params[1].Value     := FDataStoc;
  qryStockSum.Params[1].Value     := FDataStoc;
  qryAcelasiSumator.Params[1].Value := FDataStoc;
end;

procedure TfrmSelStock.SetFilterSelectate;
begin
  if MemStock.State in [dsEdit, dsInsert] then
     MemStock.Post;
  MemStock.Filtered := False;
  MemStock.OnFilterRecord := SetOnlySelect;
  MemStock.Filtered := True;
  MemStock.UpdateFilters;
  MemStock.First;
end;

procedure TfrmSelStock.BtnOkClick(Sender: TObject);
var
  lValue : Variant;
begin
  lValue := MemStock.Lookup('SELECTAT', VarArrayOf([True]), 'CODMAT');

  if VarIsEmpty(lValue) or VarIsNull(lValue) or (VarIsType(lValue, varBoolean) and (not lValue)) then
     if MessageDlg('Nu ati ales nici o pozitie - Doriti sa abandonati selectia din nomenclator?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then
        ModalResult := mrCancel
    else Abort
  else
    ModalResult := mrOk;
end;

procedure TfrmSelStock.SetFilterCulegere;
begin
  ChkShowAllReadyClick(ChkShowAllReady);
end;

procedure TfrmSelStock.cxTabControlFiseChange(Sender: TObject);
var
  aCodMat : Integer;
  I : Integer;
begin
  if cxTabControlFise.Tabs.Count <= 0 then Exit;
  I :=  Pos('] ', cxTabControlFise.Tabs[cxTabControlFise.TabIndex].Caption);
  aCodMat := StrToInt(Copy(cxTabControlFise.Tabs[cxTabControlFise.TabIndex].Caption, 2, I - 2));
  qryFisaMaterial.Close;
  qryFisaMaterial.Params[0].Value := aCodMat;
  qryFisaMaterial.Params.ParamByName('tipStock').Value := null;
  qryFisaMaterial.Open;
end;

procedure TfrmSelStock.PopulateFise;
begin
  cxTabControlFise.Tabs.Clear;
  qryCodMaturi.Open;
  if qryCodMaturi.IsEmpty then Exit;

  with qryCodMaturi do begin
    First;
    while not Eof do begin
      cxTabControlFise.Tabs.Add('[' + FieldByName('CODMAT').AsString + '] ' +  FieldByName('DENUMIRE').AsString);
      Next;
    end;
  end;
  tabFiseSumator.Visible := (cxTabControlFise.Tabs.Count > 0);
end;

procedure TfrmSelStock.spliterVAfterOpen(Sender: TObject);
begin
  if (not (spliterV.State = ssClosed)) then
     pageDescChange(nil);
end;

procedure TfrmSelStock.tabTipStockChange(Sender: TObject);
begin
  FtabTipStock := tabTipStock.TabIndex;
  GridStock.BeginUpdate;
  try
    MemStock.Filtered := False;
    MemStock.Filtered := True;
  finally
    GridStock.EndUpdate;
  end;  
end;

procedure TfrmSelStock.tabTipStockDrawTabEx(AControl: TcxCustomTabControl;
  ATab: TcxTab; Font: TFont);
begin
  if (ListStockInfo = nil) or (ListStockInfo.Count < ATab.Index) then Exit;
  case PStockInfo(ListStockInfo[ATab.Index])^.Semn of
    1 : ATab.Color := $00D5FFAA;
    -1 : ATab.Color := $009595FF
    else
    ATab.Color := $00A8FFFF
  end;
end;

procedure TfrmSelStock.CreateTabStock;
var
  I : Integer;
  lDesc : String;
begin
  tabTipStock.Tabs.Clear;
  if (ListStockInfo = nil) or (ListStockInfo.Count = 0) then Exit;
  for I := 0 to ListStockInfo.Count - 1 do begin
    lDesc := PStockInfo(ListStockInfo[I])^.Denumire;
    if PStockInfo(ListStockInfo[I])^.Predator = 1 then
      lDesc := lDesc + '(predator)'
    else if PStockInfo(ListStockInfo[I])^.Predator = 2 then
      lDesc := lDesc + '(primitor)'
    else
      lDesc := lDesc;//'Nomenclator';
    tabTipStock.Tabs.Add(lDesc);
  end;
end;

procedure TfrmSelStock.CreateHiddenColumns;
var
  aColumn : TcxGridDBBandedColumn;
  I : Integer;
  lDataSet : TDataSet;
begin
  if DBProcExists('spImplicitItemsiDocumFields') then begin
    lDataSet := DBNewQueryFmt('exec [spImplicitItemsiDocumFields] %d, %d', [IdLogin, IdUtilizator]);
    lDataSet.Open;
  end
  else begin
    lDataSet := nil;
  end;
  try
   MemStock.DisableControls;
   for I := 0 to MemStock.FieldCount- 1 do
     if TcxGridDBBandedTableView(GridStock).GetColumnByFieldName(MemStock.Fields[I].FieldName) = nil then begin
       aColumn := GridStock.CreateColumn;
        with aColumn do begin
          Caption := GetNiceText(MemStock.Fields[I].FieldName);
          HeaderAlignmentHorz := taCenter;
          Position.BandIndex := 1;
          Position.ColIndex := 0;
          Position.RowIndex := 0;
          Width  := 100;
          DataBinding.FieldName := MemStock.Fields[I].FieldName;
          Name :=  'GridStock_'+MemStock.Fields[I].FieldName;
          PropertiesClassName := 'TcxTextEditProperties';
          TcxTextEditProperties(Properties).Alignment.Horz := taLeftJustify;
          TcxTextEditProperties(Properties).Alignment.Vert := taVCenter;
          Options.Editing   := False;
          Options.Filtering := True;
          aColumn.Visible   := Assigned(lDataSet) and Assigned(lDataSet.FindField(DataBinding.FieldName));
        end;
     end;
  finally
     MemStock.EnableControls;
     if Assigned(lDataSet) then lDataSet.Free;
  end;
  GridStock.ApplyBestFit(nil, True, True);
end;


procedure TfrmSelStock.edtDataStockPropertiesChange(Sender: TObject);
begin
  PostMessage(Handle, WM_DateStock, 0, 0);
end;

procedure TfrmSelStock.CmdSelectiePozitieClick(Sender: TObject);
var Key: Word;
    Shift: TShiftState;
begin
  Key := VK_SPACE;
  Shift := [ssCtrl];
  GridStockKeyDown(GridStock, Key, Shift);
end;

procedure TfrmSelStock.InitNomenclator;
begin
  GridStockOrderCol.SortOrder := soAscending;
  GridStockOrderCol.SortIndex := 0;
  pnTop.Visible := False;
  pnBottom.Visible := False;
  spliterV.Visible := False;
  CreateTabStock;
  FtabTipStock := 0;
  tabTipStock.Height := tabTipStock.ViewInfo.RowCount * 20;
  MemStock.OnFilterRecord := MemStockNomFilterRecord;
end;

procedure TfrmSelStock.MemStockNomFilterRecord(DataSet: TDataSet;
  var Accept: Boolean);
begin
  {Daca Avem nomenclator}
  if (tabTipStock.Tabs.Count = 0) or
    (
     (FtabTipStock >-1) and Assigned(ListStockInfo) and (ListStockInfo.Count > 0) and
     (PStockInfo(ListStockInfo[FtabTipStock])^.Semn = 0)
    )
  then begin
    Accept := Accept and (DataSet.FieldByName('SEMN_CANTITATE').AsInteger = 0);
    Exit;
  end;
  if (Accept) and ((FtabTipStock >-1) and Assigned(ListStockInfo) and (ListStockInfo.Count > 0)) then
     Accept := (Accept) and
       (DataSet.FieldByName('ID_GEST_TIP_MATERIAL').AsInteger = PStockInfo(ListStockInfo[FtabTipStock])^.IdGestTipMaterial);
end;

end.
