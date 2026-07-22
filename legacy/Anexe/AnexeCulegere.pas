unit AnexeCulegere;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxGraphics, cxControls,  cxContainer, cxEdit, cxTextEdit, cxMaskEdit, cxDropDownEdit,
  cxImageComboBox, StdCtrls,  cxStyles,
  DB, cxTL, dxmdaset, ZDataSet, cxInplaceContainer, cxDBTL, cxTLData,
  cxGridLevel, cxGridBandedTableView, cxGridDBBandedTableView,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxClasses,
  cxGridCustomView, cxGrid, ExtCtrls, CommonDbVar, cxSpinEdit,
  cxGridCustomPopupMenu, cxGridPopupMenu, 
  cxButtons, AnexeParametriiLista, Menus, cxLookAndFeelPainters,
  cxDataStorage, cxDBData, cxCheckBox,
  cxDBEdit,
  ZAbstractRODataset, ZAbstractDataset,
  cxTLdxBarBuiltInMenu,
  cxLookAndFeels, cxCustomData, cxFilter, cxData, cxNavigator,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxBarBuiltInMenu;

const
  WM_REFRESH_ANEXE = WM_USER+1;
  WM_BESTFIT = WM_USER+2;

type
  TfrmAnexeCulegere = class(TForm)
    pnTop: TPanel;
    cxGridAnexa: TcxGrid;
    cxGridAnexaDBTableView1: TcxGridDBTableView;
    cxGridAnexaDBTableView1Column1: TcxGridDBColumn;
    GridAnexa: TcxGridDBBandedTableView;
    GridAnexaColumn1: TcxGridDBBandedColumn;
    GridAnexaL: TcxGridLevel;
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
    QryAnexe: TZQuery;
    MemAnexe: TdxMemData;
    DTAnexe: TDataSource;
    Label4: TLabel;
    edNrZerouri: TcxImageComboBox;
    edZerouri: TcxSpinEdit;
    cxGridPopupMenu: TcxGridPopupMenu;
    btnDelDate: TcxButton;
    btnRefreshDate: TcxButton;
    qryHead: TZQuery;
    DTHead: TDataSource;
    pnParam: TPanel;
    Label2: TLabel;
    Label3: TLabel;
    Label1: TLabel;
    edtUnitate: TcxPopupEdit;
    edtPerioada: TcxImageComboBox;
    edtAnexa: TcxImageComboBox;
    btnEditParams: TcxButton;
    edIdParam: TcxDBTextEdit;
    procedure FormCreate(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure edtAnexaPropertiesChange(Sender: TObject);
    procedure edtUnitatePropertiesInitPopup(Sender: TObject);
    procedure cxTreeUnitatiDblClick(Sender: TObject);
    procedure cxTreeUnitatiKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure edtUnitatePropertiesPopup(Sender: TObject);
    procedure edtUnitatePropertiesCloseQuery(Sender: TObject;
      var CanClose: Boolean);
    procedure edtPerioadaPropertiesChange(Sender: TObject);
    procedure edNrZerouriPropertiesChange(Sender: TObject);
    procedure GridAnexaCustomDrawCell(Sender: TcxCustomGridTableView;
      ACanvas: TcxCanvas; AViewInfo: TcxGridTableDataCellViewInfo;
      var ADone: Boolean);
    procedure GridAnexaFocusedRecordChanged(Sender: TcxCustomGridTableView;
      APrevFocusedRecord, AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure edZerouriPropertiesChange(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure btnEditParamsClick(Sender: TObject);
    procedure btnDelDateClick(Sender: TObject);
    procedure btnRefreshDateClick(Sender: TObject);
  private
    { Private declarations }
    LstColoane : TList;
    FIdAnexa : Integer;
    FIdUnitate : Integer;
    FIdPerioada : Integer;
    FIdAnexeCentParam : Integer;    
    FIsInLoading : Boolean;
    FNeedBestFit : Boolean;
    function HasParams(idAnexaBilant : Integer): Boolean;
    procedure SetTipColumnProperties(aColumn : TcxGridDBBandedColumn; colStru : StructuraColoane);
    procedure WMRefreshField(var Message: TMessage); message WM_REFRESH_ANEXE;
    procedure WMBestFit(var Message: TMessage); message WM_BESTFIT;
  public
    { Public declarations }
    lParamList : TAnexeParamList;
    procedure InitParamList;    
    procedure ValidareSuma(Sender: TField);
    procedure RefreshListaAnexe;
    procedure CreateColumnOnBand(aBandIndex : Integer; Unitate : Integer);
    procedure BuildColoane;
    procedure CleanBuildColumns;
    procedure DeschideDataSet;
    function CheckHeaderDateSet : Boolean;

  end;


implementation

uses
   dxCompsUtile, ZeosDBUtile, dateUnit, StrUtils, AnexeParametriiCul;

{$R *.dfm}

{ TfrmAnexeCulegere }

procedure TfrmAnexeCulegere.RefreshListaAnexe;
var
  aQry : TZQuery;
  lId : Variant;
begin
  lId := edtAnexa.EditValue;
  FillImageCombo(edtAnexa.Properties, 'exec [spLstAnexe]', 'ID_ANEXE_BILANT', 'DENUMIRE');
  if lId <> Null then edtAnexa.EditValue := lId
  else if edtAnexa.Properties.Items.Count >0 then edtAnexa.ItemIndex := 0;
  FillImageCombo(edtPerioada.Properties, 'exec spLstAnexePerioade', 'ID', 'Descriere');
end;

procedure TfrmAnexeCulegere.FormCreate(Sender: TObject);
begin
  FNeedBestFit := False;
  FIdAnexa := -1;
  FIdUnitate := -1;
  FIdPerioada := -1;

  LstColoane := TList.Create;
  RefreshListaAnexe;
end;

procedure TfrmAnexeCulegere.FormDestroy(Sender: TObject);
begin
  if Assigned(LstColoane) then
    LstColoane.Free;
end;

procedure TfrmAnexeCulegere.SetTipColumnProperties(
  aColumn: TcxGridDBBandedColumn; colStru: StructuraColoane);
begin
   (*
   1 = varchar(254)
   2 = varchar(30)
   3 = datetime'
   4 = money'
   5 = int'
   6 = bigint'
   7 = float'
   8 = real'
   9 = bit'
   *)
  case colStru.TipColoana of
    0, 1, 2 : begin
      aColumn.PropertiesClassName := 'TcxTextEditProperties';
    end;
    3 : begin
      aColumn.PropertiesClassName := 'TcxDateEditProperties';
    end;
   4, 7, 8 : begin
      aColumn.PropertiesClassName := 'TcxCurrencyEditProperties';
   end;
   5, 6 : begin
      aColumn.PropertiesClassName := 'TcxSpinEditProperties';
   end;
   9 : begin
      aColumn.PropertiesClassName := 'TcxCheckBoxProperties';
   end;
  end;
  aColumn.Caption := GetNiceText(colStru.Captura);
  aColumn.HeaderAlignmentHorz := taCenter;
  aColumn.HeaderAlignmentVert := vaCenter;
  aColumn.DataBinding.FieldName := colStru.FieldName;
  aColumn.Visible := colStru.Vizibila;
end;

procedure TfrmAnexeCulegere.CreateColumnOnBand(aBandIndex,
  Unitate: Integer);
var
  I : Integer;
  aColumn : TcxGridDBBandedColumn;
begin
  for I := 0 To LstColoane.Count - 1 do begin
    aColumn :=  GridAnexa.CreateColumn;
    aColumn.Position.BandIndex := aBandIndex;
    //aColumn.Position.LineCount := 2;
    aColumn.Tag := Unitate;
    aColumn.Options.Editing := True;
    if Unitate = -1 then begin
      aColumn.Styles.Header := frmData.cxStyle46;
    end;
    SetTipColumnProperties(aColumn, PStructuraColoane(LstColoane.Items[I])^);
  end;
end;

procedure TfrmAnexeCulegere.edtAnexaPropertiesChange(Sender: TObject);
begin
  if edtAnexa.EditValue <> null then edtAnexa.Tag := edtAnexa.EditValue
  else edtAnexa.Tag := -1;
  FIdAnexa := edtAnexa.Tag;
  btnEditParams.Visible :=  HasParams(FIdAnexa);
  BuildColoane;
  FNeedBestFit := True;
  PostMessage(Handle, WM_REFRESH_ANEXE, 0, 0);
end;

procedure TfrmAnexeCulegere.BuildColoane;
var
  I : Integer;
  aQry : TZReadOnlyQuery;
  aBand : TcxGridBand;
  aColumn : TcxGridDBBandedColumn;
  colStru  : PStructuraColoane;
begin
  if edtAnexa.Tag = -1 then Exit;
  GridAnexa.BeginUpdate;
  for I := GridAnexa.ColumnCount - 1 downto 0 do
    GridAnexa.Columns[I].Free;
  CleanBuildColumns;
  aQry := GetTmpAdoQuery;
  with aQry do
    try
      SQL.Add('exec spLstAnexeColoane ' + IntToStr(edtAnexa.Tag) + ', 0');
      Open;
      GridAnexa.Bands.Clear;

      aBand := GridAnexa.Bands.Add;
      with aBand do begin
        Caption := edtAnexa.Text;
        HeaderAlignmentHorz := taCenter;
        HeaderAlignmentVert := vaCenter;
        FixedKind := fkLeft;
        Width := 100* RecordCount;
      end;
      New(colStru);
      while not eof do begin
        aColumn :=  GridAnexa.CreateColumn;
        //aColumn.Position.LineCount := 2;
        aColumn.Options.Editing := False;
        ZeroMemory(colStru, SizeOf(StructuraColoane));
        colStru^.Captura    := FieldByName('CAPTURA').AsString;
        colStru^.TipColoana := FieldByName('TIP_COLOANA').AsInteger;
        colStru^.SeRepeta   := False;
        colStru^.FieldName  := FieldByName('CAPTURA').AsString;
        colStru^.Vizibila   := FieldByName('VISIBLE').AsBoolean;
        SetTipColumnProperties(aColumn, colStru^);
        Next;
      end;
      Dispose(colStru);
      Close;
      SQL.Clear;
      SQL.Add('exec spLstAnexeColoane ' + IntToStr(edtAnexa.Tag) + ', 1');
      Open;
      while not eof do begin
        New(colStru);
        ZeroMemory(colStru, SizeOf(StructuraColoane));
        colStru^.Captura    := FieldByName('CAPTURA').AsString;
        colStru^.TipColoana := FieldByName('TIP_COLOANA').AsInteger;
        colStru^.SeRepeta   := True;
        colStru^.FieldName  := 'c_' + FieldByName('ID_ANEXE_COLOANE').AsString;
        colStru^.Vizibila   := FieldByName('VISIBLE').AsBoolean;
        LstColoane.Add(colStru);
        Next;
      end;

      {aBand := GridAnexa.Bands.Add;
      with aBand do begin
        Caption := 'Centralizare';
        HeaderAlignmentHorz := taCenter;
        HeaderAlignmentVert := vaCenter;
        FixedKind := fkRight;
        Width := 100 * LstColoane.Count;
      end;
      }

      CreateColumnOnBand(aBand.Index, -1);

(*      
      SQL.Clear;
      SQL.Add('exec spLstAnexeUnitati ' + IntToStr(edtAnexa.Tag) + ', 1');
      Open;
      while not eof do begin
        aBand := GridAnexa.Bands.Add;
        with aBand do begin
          Caption := FieldByName('DENUMIRE').AsString;
          HeaderAlignmentHorz := taCenter;
          HeaderAlignmentVert := vaCenter;
          //FixedKind := fkRight;
          Width := 100 * LstColoane.Count;
        end;
        CreateColumnOnBand(aBand.Index, FieldByName('id_oi_unitati').AsInteger);
        Next;
      end;
*)      
    finally
      Free;
    end;
   GridAnexa.EndUpdate;
end;

procedure TfrmAnexeCulegere.DeschideDataSet;
var
  I : Integer;
begin
  try
    MemAnexe.DisableControls;
    MemAnexe.Active := False;
    if not CheckHeaderDateSet then Exit;
    MemAnexe.Active := True;
    QryAnexe.Close;
    QryAnexe.Params.ParamByName('idParam').Value := FIdAnexeCentParam;
    QryAnexe.Params.ParamByName('idAnexa').Value := FIdAnexa;
    QryAnexe.Params.ParamByName('idUnitate').Value := FIdUnitate;
    QryAnexe.Params.ParamByName('idPerioadeFiscale').Value := FIdPerioada;
    QryAnexe.Params.ParamByName('multiplicator').Value := Integer(edZerouri.Value);
    QryAnexe.Open;
    MemAnexe.CopyFromDataSet(QryAnexe);
    for I := 0 to LstColoane.Count - 1 do
      MemAnexe.FindField(PStructuraColoane(LstColoane.Items[I]).FieldName).OnValidate := ValidareSuma;
    if FNeedBestFit then PostMessage(Handle, WM_BESTFIT, 0, 0);
  finally
    MemAnexe.EnableControls;
  end;
end;

procedure TfrmAnexeCulegere.edtUnitatePropertiesInitPopup(Sender: TObject);
begin
  with TcxPopupEdit(Sender).Properties do begin
    if PopupWidth < TcxPopupEdit(Sender).Width then PopupWidth := TcxPopupEdit(Sender).Width;
  end;
end;

procedure TfrmAnexeCulegere.cxTreeUnitatiDblClick(Sender: TObject);
begin
   with TcxDBTreeList(Sender) do
    if (FocusedNode <> nil) then begin
     (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk;
   end;
end;

procedure TfrmAnexeCulegere.cxTreeUnitatiKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then begin
     if Assigned(TcxDBTreeList(Sender).OnDblClick) then TcxDBTreeList(Sender).OnDblClick(Sender);
  end
  else if Key = VK_ESCAPE then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrCancel;
end;

procedure TfrmAnexeCulegere.edtUnitatePropertiesPopup(Sender: TObject);
var
 lNode : TcxTreeListNode;
begin
  lNode := cxTreeUnitati.FindNodeByKeyValue(FIdUnitate);
  if lNode <> nil then begin
    lNode.Focused := True;
    lNode.MakeVisible;
  end;
end;

type
  TAccesscxPopupEdit = class(TcxPopupEdit);

procedure TfrmAnexeCulegere.edtUnitatePropertiesCloseQuery(Sender: TObject;
  var CanClose: Boolean);
var
  lNode : TcxDBTreeListNode;
  lIdUnitate : Integer;
  lHandle : Integer;
begin
  lHandle := Self.Handle;
  FIdUnitate := -1;
  with TAccesscxPopupEdit(Sender) do
    if PopupWindow.ModalResult = mrOk then begin
       lNode := TcxDBTreeListNode(cxTreeUnitati.FocusedNode);
       if Assigned(lNode) then begin
          lIdUnitate := lNode.KeyValue;
          Text := VarToStr(lNode.Values[cxTreeUnitatiDENUMIRE.ItemIndex]);
          if FIdUnitate <> lIdUnitate then begin
            FIdUnitate := lIdUnitate;
            PostMessage(lHandle, WM_REFRESH_ANEXE, 0, 0);
          end;
       end;
    end;
end;

procedure TfrmAnexeCulegere.edtPerioadaPropertiesChange(Sender: TObject);
begin
  if edtPerioada.EditValue <> null then edtPerioada.Tag := edtPerioada.EditValue
  else edtPerioada.Tag := -1;
  FIdPerioada := edtPerioada.Tag;
  PostMessage(Handle, WM_REFRESH_ANEXE, 0, 0);
end;

procedure TfrmAnexeCulegere.ValidareSuma(Sender: TField);
var
  IdAnexeColoane : String;
  aQry : TZReadOnlyQuery;
begin
//  ShowMessage(Sender.FieldName + ' = ' + Sender.AsString);
  IdAnexeColoane := RightStr(Sender.FieldName, Length(Sender.FieldName) - 2);

  if not FIsInLoading then begin
    aQry := GetTmpADOQuery;
    with aQry do
      try
         Sql.Add('DECLARE @VALOARE MONEY');
         if Sender.IsNull  then Sql.Add('SET @VALOARE = NULL')
         else if Integer(edZerouri.Value) > 0 then Sql.Add('SET @VALOARE = :'+Sender.FieldName+' * '+IntToStr(edZerouri.Value))
         else Sql.Add('SET @VALOARE = :'+Sender.FieldName);
         Sql.Add('DECLARE @ID_ANEXE_COLOANE INT, @ID_ANEXE_RANDURI INT, @ID_OI_UNITATI INT, @ID_ANEXE_BILANT INT, @ID_PERIOADE_FISCALE INT, @ID_ANEXE_PARAM INT ');
         Sql.Add('SET @ID_ANEXE_COLOANE= :ID_ANEXE_COLOANE');
         Sql.Add('SET @ID_ANEXE_RANDURI= :ID_ANEXE_RANDURI');
         Sql.Add('SET @ID_OI_UNITATI= :ID_OI_UNITATI');
         Sql.Add('SET @ID_ANEXE_PARAM= :ID_PARAM');
         Sql.Add('SET @ID_ANEXE_BILANT= :ID_ANEXE_BILANT');
         Sql.Add('SET @ID_PERIOADE_FISCALE= :ID_PERIOADE_FISCALE ' );
         Sql.Add('IF EXISTS(SELECT TOP 1 1 FROM ANEXE_CENTRALIZARE WHERE ID_ANEXE_CENTRALIZARE_PARAM = @ID_ANEXE_PARAM AND ID_ANEXE_COLOANE = @ID_ANEXE_COLOANE');
         Sql.Add(' AND ID_ANEXE_RANDURI = @ID_ANEXE_RANDURI AND ID_OI_UNITATI = @ID_OI_UNITATI AND ID_ANEXE_BILANT = @ID_ANEXE_BILANT AND ID_PERIOADE_FISCALE = @ID_PERIOADE_FISCALE)');
         Sql.Add(' UPDATE ANEXE_CENTRALIZARE SET VALOARE = @VALOARE, ID_UTILIZATOR = ' + IntToStr(IdUtilizator) + ' WHERE ');
         Sql.Add('ID_ANEXE_COLOANE = @ID_ANEXE_COLOANE AND ID_ANEXE_RANDURI = @ID_ANEXE_RANDURI AND ID_OI_UNITATI = @ID_OI_UNITATI AND ID_ANEXE_BILANT = @ID_ANEXE_BILANT AND ID_PERIOADE_FISCALE = @ID_PERIOADE_FISCALE AND ID_ANEXE_CENTRALIZARE_PARAM = @ID_ANEXE_PARAM');
         Sql.Add(' ELSE ');
         Sql.Add('INSERT INTO ANEXE_CENTRALIZARE (ID_OI_UNITATI, ID_ANEXE_BILANT, ID_PERIOADE_FISCALE,  ID_ANEXE_CENTRALIZARE_PARAM, ID_ANEXE_COLOANE, ID_ANEXE_RANDURI, VALOARE, ID_UTILIZATOR)');
         Sql.Add('VALUES(@ID_OI_UNITATI, @ID_ANEXE_BILANT, @ID_PERIOADE_FISCALE,  @ID_ANEXE_PARAM, @ID_ANEXE_COLOANE, @ID_ANEXE_RANDURI,  @VALOARE, ' + IntToStr(IdUtilizator)+')');
         DataSource := DTAnexe;
         Params.ParamByName('ID_PARAM').Value := FIdAnexeCentParam;
         Params.ParamByName('ID_OI_UNITATI').Value := FIdUnitate;
         Params.ParamByName('ID_ANEXE_BILANT').Value := FIdAnexa;
         Params.ParamByName('ID_PERIOADE_FISCALE').Value := FIdPerioada;
         Params.ParamByName('ID_ANEXE_COLOANE').Value := IdAnexeColoane;
         ExecSql;
      finally
         Free;
      end;

    { Transmitem refresh pentru nodul pe care ne aflam acum }
    PostMessage(Handle, WM_REFRESH_ANEXE, 0, 1);
  end;

end;

procedure TfrmAnexeCulegere.edNrZerouriPropertiesChange(Sender: TObject);
begin
  if IsNumeric(edNrZerouri.EditValue) then
    edZerouri.Value := StrToInt(edNrZerouri.EditValue)
  else
    edZerouri.Value := 1;
end;

procedure TfrmAnexeCulegere.WMRefreshField(var Message: TMessage);
var
  lTopIndex : Integer;
  lFocusedRowIndex, lFocusedColumnIndex : Integer;
begin
  lTopIndex := GridAnexa.Controller.TopRecordIndex;
  lFocusedColumnIndex := GridAnexa.Controller.FocusedColumnIndex;
  lFocusedRowIndex := GridAnexa.Controller.FocusedRowIndex;
  DeschideDataSet;
  GridAnexa.Controller.FocusedColumnIndex := lFocusedColumnIndex;
  GridAnexa.Controller.FocusedRowIndex := lFocusedRowIndex;
  GridAnexa.Controller.TopRecordIndex := lTopIndex;  
end;

procedure TfrmAnexeCulegere.GridAnexaCustomDrawCell(
  Sender: TcxCustomGridTableView; ACanvas: TcxCanvas;
  AViewInfo: TcxGridTableDataCellViewInfo; var ADone: Boolean);
var
   lRecord     : TcxCustomGridRecord;
begin
   lRecord := AViewInfo.RecordViewInfo.GridRecord;
   if (lRecord = nil) or (AViewInfo.Selected) then Exit;
   if GridAnexa.GetColumnByFieldName('SeCalculeaza') <> nil then
     if GetInteger(lRecord, GridAnexa.GetColumnByFieldName('SeCalculeaza').Index) = 1 then begin
       ACanvas.Brush.Color := clAqua;
       ACanvas.Font.Color  := clBlack;
       ACanvas.Font.Style := ACanvas.Font.Style + [fsBold]; 
     end;
end;

procedure TfrmAnexeCulegere.GridAnexaFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
var
  SeCalc : Integer;
  I : Integer;
begin
  if not Assigned( AFocusedRecord) then Exit;
  if not AFocusedRecord.IsData then Exit;  
  if GridAnexa.GetColumnByFieldName('SeCalculeaza') = nil then Exit;
  SeCalc := GetInteger(AFocusedRecord, GridAnexa.GetColumnByFieldName('SeCalculeaza').Index);
  for I := GridAnexa.ColumnCount - 1 downto 0 do
    if GridAnexa.Columns[I].Tag = -1 then
      GridAnexa.Columns[I].Options.Editing := not (SeCalc = 1);
end;

procedure TfrmAnexeCulegere.edZerouriPropertiesChange(Sender: TObject);
begin
  PostMessage(Handle, WM_REFRESH_ANEXE, 0, 0);
end;

procedure TfrmAnexeCulegere.WMBestFit(var Message: TMessage);
begin
  if FNeedBestFit then
     GridAnexa.ApplyBestFit(nil, True, True);
  FNeedBestFit := False;
end;

procedure TfrmAnexeCulegere.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  CleanBuildColumns;
   Action := caFree;
end;

procedure TfrmAnexeCulegere.btnEditParamsClick(Sender: TObject);
var
  lForm: TfrmAnexeParametriiCul;
  lHandle : Integer;
begin
   lHandle := Self.Handle;
   lForm :=  TfrmAnexeParametriiCul.Create(Application);
   lForm.FIdAnexaBilant := FIdAnexa;
   InitParamList;
    with lForm do
    try
      FParams := lParamList;
      FIdAnexaBilant := FIdAnexa;
      ShowModal;
      if FOk then begin
         PostMessage(lHandle, WM_REFRESH_ANEXE, 0, 0);
        //lParamList.SetAnexaParams(frmData.dbContabilitate);
      end;
    finally
      lForm.Free;
    end;
end;

function TfrmAnexeCulegere.HasParams(idAnexaBilant: Integer): Boolean;
begin
  with GetTmpADOQuery do
    try
      SQL.Add('exec spAnexaHasParams ' + IntToStr(idAnexaBilant));
      Open;
      Result := Fields[0].AsBoolean;
    finally
      Free;
    end;
end;

procedure TfrmAnexeCulegere.CleanBuildColumns;
var I : Integer;
begin
  for I := LstColoane.Count-1 downto 0 do
    Dispose(PStructuraColoane(LstColoane[I]));
  LstColoane.Clear;
end;

procedure TfrmAnexeCulegere.btnDelDateClick(Sender: TObject);
begin
   if not CheckHeaderDateSet then Exit;
   if (MessageDlg('Doriti stergerea datelor introduse ? ', mtConfirmation, [mbYes, mbNo], 0) in [mrNo, mrNone]) then Abort;
   DBExecSqlFmt('exec [spAnexeCentralizareDel] %d, %d, %d, %d', [FIdAnexeCentParam, FIdAnexa, FIdUnitate, FIdPerioada] );
   DeschideDataSet;   
end;

procedure TfrmAnexeCulegere.btnRefreshDateClick(Sender: TObject);
begin
  PostMessage(Handle, WM_REFRESH_ANEXE, 0, 0);
end;

procedure TfrmAnexeCulegere.InitParamList;
var
    lOldIdAnexa : Integer;
begin
  if lParamList = nil then begin
    if btnEditParams.Visible then
       lParamList := TAnexeParamList.Create;
  end;
  if btnEditParams.Visible then begin
    lOldIdAnexa := lParamList.IdAnexa;
    lParamList.IdAnexa := FIdAnexa;
    if lOldIdAnexa <> FIdAnexa then
      lParamList.ReadAnexaParams(frmData.dbContabilitate);
  end;
end;

function TfrmAnexeCulegere.CheckHeaderDateSet : Boolean;
var
  I, ldim : Integer;
  lSearch : String;
  lValues : array of Variant;
begin
  Result := False;
  if (FIdAnexa = -1) or (FIdUnitate = -1) or (FIdPerioada = -1) then Exit;

  InitParamList;

  if (lParamList <> nil) and (btnEditParams.Visible) then
    for I := 0 to lParamList.ParamCount -1  do
      If VarToStr(lParamList.Params[I].Value) = '' then Exit;
      
  if not qryHead.Active then DBRefresh(qryHead);
  lSearch := 'id_anexe_bilant;id_oi_unitati;id_perioade_fiscale';
  SetLength(lValues, 3);
  lValues[0] := FIdAnexa;
  lValues[1] := FIdUnitate;
  lValues[2] := FIdPerioada;
  if (lParamList <> nil) and (btnEditParams.Visible) then
    for I := 0 to lParamList.ParamCount -1  do begin
      lSearch := lSearch + ';' + lParamList.Params[I].Name;
      SetLength(lValues, Length(lValues) + 1);
      lValues[Length(lValues)-1] := lParamList.Params[I].Value;
    end;
  //ShowMessage(VarToStr(lValueList[0]) + ' ' + VarToStr(lValueList[1]) + ' ' + VarToStr(lValueList[2]) + ' ' + VarToStr(lValueList[3]));
  if not qryHead.Locate(lSearch, VarArrayOf(lValues), []) then begin
     qryHead.Append;
     qryHead.FieldByName('id_anexe_bilant').AsInteger := FIdAnexa;
     qryHead.FieldByName('id_oi_unitati').AsInteger := FIdUnitate;
     qryHead.FieldByName('id_perioade_fiscale').AsInteger := FIdPerioada;
     if (lParamList <> nil) and (btnEditParams.Visible) then
        for I := 0 to lParamList.ParamCount - 1 do
          qryHead.FieldByName(lParamList.Params[I].Name).Value := lParamList.Params[I].Value;
     qryHead.Post;
  end;
  FIdAnexeCentParam := qryHead.FieldByName('id_anexe_centralizare_param').AsInteger;

  Result := True;
end;

end.
