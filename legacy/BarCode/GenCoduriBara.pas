unit GenCoduriBara;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxGraphics, 
  cxDataStorage, cxEdit, DB, cxDBData, cxCheckBox, cxCurrencyEdit,
  cxImageComboBox, cxCalendar, cxGridLevel, cxGridCustomTableView,
  cxGridTableView, cxGridBandedTableView, cxGridDBBandedTableView,
  cxClasses, cxControls, cxGridCustomView, cxGrid, ExtCtrls, dxmdaset,
  ZDataSet, cxGridCustomPopupMenu, cxGridPopupMenu, dxExEdtr, dxEdLib,
  dxCntner, dxEditor, StdCtrls, cxContainer, cxTextEdit, Menus,
  cxLookAndFeelPainters, cxButtons,
  ZAbstractRODataset, ZAbstractDataset,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, cxMaskEdit,
  cxDropDownEdit, cxButtonEdit, cxNavigator, dxDateRanges,
  dxScrollbarAnnotations, dxBarBuiltInMenu;

type
  TfrmGenCoduriBara = class(TForm)
    Panel1: TPanel;
    grStock: TcxGrid;
    GridStock: TcxGridDBBandedTableView;
    GridStockSELECTAT: TcxGridDBBandedColumn;
    GridStockCANTITATE_SELECTATA: TcxGridDBBandedColumn;
    GridStockPRODUS: TcxGridDBBandedColumn;
    GridStockTIP_STOCK: TcxGridDBBandedColumn;
    GridStockCANTITATE: TcxGridDBBandedColumn;
    GridStockCONT: TcxGridDBBandedColumn;
    GridStockDATACOD: TcxGridDBBandedColumn;
    GridStockNR_DOCUM: TcxGridDBBandedColumn;
    GridStockCODMAT: TcxGridDBBandedColumn;
    GridStockID_INITIAL: TcxGridDBBandedColumn;
    GridStockID_UTILIZATORI: TcxGridDBBandedColumn;
    GridStockPRET_UNITAR: TcxGridDBBandedColumn;
    GridStockPRET_RECEPTIE: TcxGridDBBandedColumn;
    GridStockCOTA_TVA: TcxGridDBBandedColumn;
    GridStockPRET_RECEPTIE_TVA: TcxGridDBBandedColumn;
    GridStockDENMAT: TcxGridDBBandedColumn;
    GridStockTIPMAT: TcxGridDBBandedColumn;
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
    GridStockRecId: TcxGridDBBandedColumn;
    grLevel: TcxGridLevel;
    pnTop: TPanel;
    Panel2: TPanel;
    DTStock: TDataSource;
    MemStock: TdxMemData;
    QryStock: TZQuery;
    popupGrid: TcxGridPopupMenu;
    Label1: TLabel;
    edtTipStoc: TcxImageComboBox;
    edtCont: TcxButtonEdit;
    Label2: TLabel;
    ChkShowAllReady: TcxCheckBox;
    ChkShowData: TcxCheckBox;
    ChkShowNegative: TcxCheckBox;
    chkStockLazi: TcxCheckBox;
    btnConfigBarCode: TcxButton;
    btnGenerare: TcxButton;
    procedure QryStockAfterOpen(DataSet: TDataSet);
    procedure edtContButtonClick(Sender: TObject; AbsoluteIndex: Integer);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure FormCreate(Sender: TObject);
    procedure ChkShowAllReadyClick(Sender: TObject);
    procedure edtTipStocChange(Sender: TObject);
    procedure GridStockCustomDrawCell(Sender: TcxCustomGridTableView;
      ACanvas: TcxCanvas; AViewInfo: TcxGridTableDataCellViewInfo;
      var ADone: Boolean);
    procedure GridStockEditKeyDown(Sender: TcxCustomGridTableView;
      AItem: TcxCustomGridTableItem; AEdit: TcxCustomEdit; var Key: Word;
      Shift: TShiftState);
    procedure GridStockFocusedRecordChanged(Sender: TcxCustomGridTableView;
      APrevFocusedRecord, AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure GridStockKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure btnConfigBarCodeClick(Sender: TObject);
    procedure btnGenerareClick(Sender: TObject);
  private
    { Private declarations }
    IsInLoading : Boolean;
    procedure RefreshStocuri;
    procedure RefreshTipStoc;
    procedure SetCantitateSelectata(Sender: TField);
    procedure SetFilterCulegere;
    procedure SetFilterSelectate;
    procedure SetOnlySelect(DataSet: TDataSet; var Accept: Boolean);    
    procedure MemStockFilterRecord(DataSet: TDataSet; var Accept: Boolean);
    procedure InitializeGrid;
  public
    { Public declarations }
  end;

procedure ShowGetCoduriBara;

implementation

uses
  dxCompsUtile, ZeosDBUtile, PlanConturiUnit, DateUnit, CommonDBVar, configBarCode;

{$R *.dfm}

procedure ShowGetCoduriBara;
var
  lGetCoduriBara: TfrmGenCoduriBara;
begin
  lGetCoduriBara := TfrmGenCoduriBara.Create(nil);
  try
    lGetCoduriBara.ShowModal;
  finally
    lGetCoduriBara.Free;
  end;
end;

procedure TfrmGenCoduriBara.QryStockAfterOpen(DataSet: TDataSet);
begin
  with MemStock do
  begin
    MemStock.Tag := 1;
    try
      DisableControls;
      Filtered := False;
      OnFilterRecord := nil;
      if Active then
         Close;
      CopyFromDataSet(QryStock);
      FindField('CANTITATE_SELECTATA').OnChange := SetCantitateSelectata;
      SetFilterCulegere();
    finally
      InitializeGrid;
      MemStock.Tag := 0;
      EnableControls;
    end;
  end;
  ;
end;

procedure TfrmGenCoduriBara.edtContButtonClick(Sender: TObject;
  AbsoluteIndex: Integer);
var
  Cont : String;
begin
  Cont := edtTipStoc.Text;
  if SelectareContPlan(Cont, False) then begin
    edtCont.Text := Cont;
    RefreshTipStoc;
    RefreshStocuri;
  end;
end;

procedure TfrmGenCoduriBara.RefreshStocuri;
begin
  if IsInLoading then Exit;
  with QryStock do
    try
      DisableControls;
      if Active then Close;
      if Trim(edtTipStoc.Text) <> '' then Params.ParamByName('TIP_STOC').Value := edtTipStoc.Text
                                     else Params.ParamByName('TIP_STOC').Value := '1';
      Params.ParamByName('CU_MISCARI').Value := 1;
      if Trim(edtCont.Text) <> '' then
        Params.ParamByName('CONT').Value := edtCont.Text
      else
        Params.ParamByName('CONT').Value := '302';
      Open;
    finally
      EnableControls;
    end;

end;

procedure TfrmGenCoduriBara.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  
    Action := caFree;
end;

procedure TfrmGenCoduriBara.FormCreate(Sender: TObject);
var aQry : TZReadOnlyQuery;
begin
  IsInLoading := True;
  //PopulateRepartitori;
//  GridStocuri.SearchType := stContain;
  FillImageCombo(edtTipStoc.Properties, 'SELECT * FROM GEST_TIP_STOC', 'ID_GEST_TIP_STOC', 'DENUMIRE', 1, 'Stoc Unitate');
  if edtTipStoc.Properties.Items.Count > 0 then edtTipStoc.EditValue := edtTipStoc.Properties.Items[0].Value;
  if DBProcExists('SP_GET_GEST_PRODUSE') then
    FillImageCombo(GridStockPRODUS.Properties, 'exec [SP_GET_GEST_PRODUSE]', 'TIP_PRODUS', 'DENUMIRE');

  edtCont.Text := '371';
  RefreshTipStoc;
  IsInLoading := False;
  RefreshStocuri;

//  QryDocument.Open;
//  CreateReportContext;  
end;

procedure TfrmGenCoduriBara.SetCantitateSelectata(Sender: TField);
begin
  if Sender.AsCurrency > MemStock.FieldByName('CANTITATE').AsCurrency then
     if MessageDlg('Nu puteti scoate din stock mai mult decat aveti STOCK !'+MemStock.FieldByName('CANTITATE').AsString+' < '+Sender.AsString+
                   #13#10'Doriti abandonul sumei introduse si intorducerea uneia noi?', mtConfirmation, [mbYes, mbNo], 0) <> mrNo then
        Abort;
  if MemStock.Tag = 0 then
     MemStock.FieldByName('SELECTAT').AsBoolean := Sender.AsCurrency <> 0;
end;

procedure TfrmGenCoduriBara.SetFilterCulegere;
begin
  MemStock.OnFilterRecord := MemStockFilterRecord;
  ChkShowAllReadyClick(ChkShowAllReady);
end;

procedure TfrmGenCoduriBara.ChkShowAllReadyClick(Sender: TObject);
begin
  GridStock.BeginUpdate;
  try
    MemStock.Filtered := False;
    MemStock.Filtered := True;
  finally
    GridStock.EndUpdate;
  end;
end;

procedure TfrmGenCoduriBara.MemStockFilterRecord(DataSet: TDataSet;
  var Accept: Boolean);
begin
  {Daca Avem nomenclator}
(*  if (tabTipStock.Tabs.Count = 0) or
    (
     (FtabTipStock >-1) and Assigned(ListStockInfo) and (ListStockInfo.Count > 0) and
     (PStockInfo(ListStockInfo[FtabTipStock])^.Semn = 0)
    )
  then begin
    Accept := Accept and (DataSet.FieldByName('SEMN_CANTITATE').AsInteger = 0);
    Exit;
  end;
 *)
//  Accept := (ChkShowData.Checked) or (DataSet.FieldByName('DATACOD').AsDateTime <= FDataDoc);
  if (Accept) and (chkStockLaZi.Checked) then
     if (ChkShowNegative.Checked) then
        Accept := (Accept) and ((DataSet.FieldByName('CANTITATE').AsCurrency <> 0) or (DataSet.FieldByName('VALOARE').AsCurrency <> 0))
     else
        Accept := (Accept) and (DataSet.FieldByName('CANTITATE').AsCurrency > 0);
  if (Accept) and (not ChkShowNegative.Checked) and (not chkStockLaZi.Checked) then
      Accept := (Accept) and (DataSet.FieldByName('CANTITATE').AsCurrency > 0);

      (*
  if (Accept) and (ChkShowAllReady.Checked) then
     Accept := (Accept) and (FAllReady.IndexOf(DataSet.FieldByName('CODMAT').AsString) = -1);
  if (Accept) and ((FtabTipStock >-1) and Assigned(ListStockInfo) and (ListStockInfo.Count > 0)) then
     Accept := (Accept) and
       (DataSet.FieldByName('ID_GEST_TIP_STOCK').AsInteger = PStockInfo(ListStockInfo[FtabTipStock])^.IdGestTipStock)
        and
       (DataSet.FieldByName('PREDATOR').AsInteger = PStockInfo(ListStockInfo[FtabTipStock])^.Predator);
     *)
end;

procedure TfrmGenCoduriBara.InitializeGrid;
(*

var lColumn: TdxDBGridCurrencyColumn;
    lMColumn: TdxDBGridMaskColumn;
    I: Integer;

  function GetCaptionFromName(lFieldName: String): String;
  var lNode: TdxDBTreeListNode;
      lValue: Integer;
   begin

     lValue := pos('GEST_', lFieldName);
     if lValue > 0 then begin
        Delete(lFieldName, 1 , 5);
        lValue := StrToInt(lFieldName);
     end;

     lNode := TreeRepartitori.FindNodeByKeyValue(lValue);
     if Assigned(lNode) then Result := lNode.Strings[0]
     else Result := lFieldName;
   end;

begin
  {Trebuie sa adaugam restul de campuri si sa le incarcam pe cele vizibile din banda 0}

  GridStocuri.BeginUpdate;
  { Citim codurile gestiunilor }
  try
    for I := GridStocuri.ColumnCount - 1  downto  0  do
       if GridStocuri.Columns[I].BandIndex = 1 then
          GridStocuri.Columns[I].Free;

    for I := 0 to DataSet.FieldCount-1 do begin
      if (pos('GEST_', DataSet.Fields[I].FieldName) = 1) and (not Assigned(GridStocuri.FindColumnByFieldName(DataSet.Fields[I].FieldName)) ) then begin
         lColumn := TdxDBGridCurrencyColumn(GridStocuri.CreateColumn(TdxDBGridCurrencyColumn));
         lColumn.HeaderAlignment := taCenter;
         lColumn.Caption := GetCaptionFromName(DataSet.Fields[I].FieldName);
         lColumn.DisplayFormat := ',0.00;-,0.00';
         lColumn.FieldName := DataSet.Fields[I].FieldName;
         lColumn.BandIndex := 1;
         lColumn.Visible   := True;
         lColumn.DisableFilter := True;
      end;
      if not Assigned(GridStocuri.FindColumnByFieldName(DataSet.Fields[I].FieldName))  then begin
         lMColumn := TdxDBGridMaskColumn(GridStocuri.CreateColumn(TdxDBGridMaskColumn));
         lMColumn.HeaderAlignment := taCenter;
         lMColumn.Caption := GetCaptionFromName(DataSet.Fields[I].FieldName);
         lMColumn.FieldName := DataSet.Fields[I].FieldName;
         lMColumn.BandIndex := 1;
         lMColumn.Visible   := False;
      end;
    end;
  finally
     DTStocuri.DataSet := QryStocuri;
     GridStocuri.EndUpdate;
  end;
  GridStocuri.ApplyBestFit(nil);
end;
*)

var
  aColumn : TcxGridDBBandedColumn;
  I : Integer;


  function GetCaptionFromName(lFieldName: String): String;
  var //lNode: TdxDBTreeListNode;
      lValue: Integer;
   begin

     lValue := pos('GEST_', lFieldName);
     if lValue > 0 then begin
        Delete(lFieldName, 1 , 5);
        lValue := StrToInt(lFieldName);
     end;
     if frmData.QryRepartitori.Locate('ID_REPARTITORI', lValue, [])
     then
       Result := frmData.QryRepartitori.FieldByName('NUME').AsString
     else
       Result := lFieldName;
   end;

begin
   for I := GridStock.ColumnCount - 1  downto  0  do
    if GridStock.Columns[I].Position.BandIndex = 2 then
       GridStock.Columns[I].Free;
       
   for I := 0 to MemStock.FieldCount- 1 do begin
     if (pos('GEST_', MemStock.Fields[I].FieldName) = 1) and (TcxGridDBBandedTableView(GridStock).GetColumnByFieldName(MemStock.Fields[I].FieldName) = nil ) then begin
       aColumn := GridStock.CreateColumn;
        with aColumn do begin
          Caption := GetCaptionFromName(MemStock.Fields[I].FieldName);
          HeaderAlignmentHorz := taCenter;
          Position.BandIndex := 2;
          Position.ColIndex := 0;
          Position.RowIndex := 0;
          DataBinding.FieldName := MemStock.Fields[I].FieldName;
          Name :=  'GridStock_'+MemStock.Fields[I].FieldName;
          PropertiesClassName := 'TcxCurrencyEditProperties';
          TcxCurrencyEditProperties(Properties).Alignment.Horz := taLeftJustify;
          TcxCurrencyEditProperties(Properties).Alignment.Vert := taVCenter;
          TcxCurrencyEditProperties(Properties).DisplayFormat := ',0.00;-,0.00';
          Options.Editing := False;
          Options.Filtering := True;
          aColumn.Visible := True;
        end;
     end;
     if TcxGridDBBandedTableView(GridStock).GetColumnByFieldName(MemStock.Fields[I].FieldName) = nil then begin
       aColumn := GridStock.CreateColumn;
        with aColumn do begin
          Caption := GetNiceText(MemStock.Fields[I].FieldName);
          HeaderAlignmentHorz := taCenter;
          Position.BandIndex := 1;
          Position.ColIndex := 0;
          Position.RowIndex := 0;
          DataBinding.FieldName := MemStock.Fields[I].FieldName;
          Name :=  'GridStock_'+MemStock.Fields[I].FieldName;
          PropertiesClassName := 'TcxTextEditProperties';
          TcxTextEditProperties(Properties).Alignment.Horz := taLeftJustify;
          TcxTextEditProperties(Properties).Alignment.Vert := taVCenter;
          Options.Editing := False;
          Options.Filtering := True;
          aColumn.Visible := False;
        end;
     end;
   end;
end;



procedure TfrmGenCoduriBara.edtTipStocChange(Sender: TObject);
begin
  RefreshStocuri;
end;

procedure TfrmGenCoduriBara.GridStockCustomDrawCell(
  Sender: TcxCustomGridTableView; ACanvas: TcxCanvas;
  AViewInfo: TcxGridTableDataCellViewInfo; var ADone: Boolean);
var
   aStockIni,
   aStockEnter : Currency;
   lRecord     : TcxCustomGridRecord;
// aCodMat     : Integer;
begin
  lRecord := AViewInfo.RecordViewInfo.GridRecord;
  if (lRecord = nil) or (AViewInfo.Selected) then Exit;

 //aCodMat := GetInteger(lRecord, GridStockCODMAT.Index);
 begin
    aStockIni   := GetCurrency(lRecord, GridStockCANTITATE.Index);
    aStockEnter := GetCurrency(lRecord, GridStockCANTITATE_SELECTATA.Index);
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
        end;
  end;
end;

procedure TfrmGenCoduriBara.GridStockEditKeyDown(
  Sender: TcxCustomGridTableView; AItem: TcxCustomGridTableItem;
  AEdit: TcxCustomEdit; var Key: Word; Shift: TShiftState);
begin
  GridStockKeyDown(Sender, Key, Shift);
end;

procedure TfrmGenCoduriBara.GridStockFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
//var aCodMat, aIdTipStoc : Integer;
begin
{
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
}
end;

procedure TfrmGenCoduriBara.GridStockKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
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
      if (MemStock.FieldByName('CANTITATE_SELECTATA').AsCurrency = 0) and (MemStock.FieldByName('CANTITATE').AsCurrency > 0)  then
         MemStock.FieldByName('CANTITATE_SELECTATA').AsCurrency := MemStock.FieldByName('CANTITATE').AsCurrency;
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
          (*
          aCodMat := GetInteger(lRecord, GridStockCODMAT.Index);
          with MemStock do
           if (FieldByName('CODMAT').AsInteger <> aCodMat) or (Locate('CODMAT', aCodMat, [])) then
              SwitchCantitate;
           *)
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

procedure TfrmGenCoduriBara.RefreshTipStoc;
begin
  if DBProcExists('spGetTipStocOnCont') then
    FillImageComboFmt(edtTipStoc.Properties, 'exec [spGetTipStocOnCont] %s', [ValueToStr(edtCont.EditValue)], 'ID_GEST_TIP_STOC', 'DENUMIRE', 1, 'Stoc Unitate');
end;

procedure TfrmGenCoduriBara.btnConfigBarCodeClick(Sender: TObject);
begin
  with TfrmConfigBarCode.Create(nil) do
  try
     memText.Text := MemStock.FieldByName('CodBara').AsString;
     ShowModal;
  finally
    free;
  end;
end;

procedure TfrmGenCoduriBara.btnGenerareClick(Sender: TObject);
var
  aQry : TZReadOnlyQuery;
  lImageStream : TMemoryStream;
  lImage : TBitmap;
begin
  try
    aQry := GetTmpADOQuery;
    lImageStream := TMemoryStream.Create;
    lImage := TBitmap.Create;
    aQry.SQL.Add('exec spTabelaCodBaraCreate');
    aQry.ExecSQL;
    aQry.SQL.Clear;
    aQry.SQL.Add('set textsize 2147483647');
    aQry.ExecSQL;
    aQry.SQL.Clear;
    aQry.SQL.Add('exec spTabelaCodBaraAdd :codmat, :cantitate, :codbara, :image, :sizeWidth, :sizeHeight');
    SetFilterSelectate;
    MemStock.First;
    while not MemStock.eof do begin
      aQry.Params.ParamByName('codmat').Value := MemStock.FieldByName('codmat').AsInteger;
      aQry.Params.ParamByName('cantitate').Value := MemStock.FieldByName('CANTITATE_SELECTATA').AsCurrency;
      aQry.Params.ParamByName('codbara').Value := MemStock.FieldByName('codbara').AsString;
      lImage.ReleaseHandle;
      EncodeDataMatrixImage(MemStock.FieldByName('codbara').AsString, lImage);
      lImageStream.Clear;
      lImage.SaveToStream(lImageStream);
      aQry.Params.ParamByName('image').LoadFromStream(lImageStream, ftBlob);
      aQry.Params.ParamByName('sizeWidth').Value := lImage.Width;
      aQry.Params.ParamByName('sizeHeight').Value := lImage.Height;
      aQry.ExecSQL;
      MemStock.Next;
    end;
  finally
    MemStock.Filtered := False;
    MemStock.OnFilterRecord := nil;
    MemStock.UpdateFilters;
    MemStock.First;
    aQry.Free;
    lImageStream.Free;
    lImage.Free;
  end;
end;

procedure TfrmGenCoduriBara.SetFilterSelectate;
begin
  if MemStock.State in [dsEdit, dsInsert] then
     MemStock.Post;
  MemStock.Filtered := False;
  MemStock.OnFilterRecord := SetOnlySelect;
  MemStock.Filtered := True;
  MemStock.UpdateFilters;
  MemStock.First;
end;

procedure TfrmGenCoduriBara.SetOnlySelect(DataSet: TDataSet;
  var Accept: Boolean);
begin
  Accept := DataSet.FieldByName('SELECTAT').AsBoolean;
end;

end.
