unit AnexeCentralizare;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, ExtCtrls, cxGraphics,  StdCtrls, cxControls, cxContainer, cxEdit,
  cxTextEdit, cxMaskEdit, cxDropDownEdit, cxImageComboBox, ZDataSet, cxStyles, DB,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGridLevel,
  cxClasses, cxGridCustomView, cxGrid, cxGridBandedTableView, cxGridDBBandedTableView,
  cxSpinEdit, CommonDBVar, dxmdaset, cxGridCustomPopupMenu, cxGridPopupMenu,
  cxDataStorage, cxDBData, cxCheckBox,
  ZAbstractRODataset, ZAbstractDataset,
  cxLookAndFeelPainters,
  cxLookAndFeels, cxCustomData, cxFilter, cxData, cxNavigator;


const
  WM_REFRESH_ANEXE = WM_USER+1;

type

  TfrmAnexeCentralizare = class(TForm)
    pnTop: TPanel;
    edtAnexa: TcxImageComboBox;
    Label1: TLabel;
    cxGridAnexaDBTableView1: TcxGridDBTableView;
    cxGridAnexa: TcxGrid;
    cxGridAnexaDBTableView1Column1: TcxGridDBColumn;
    GridAnexaL: TcxGridLevel;
    GridAnexa: TcxGridDBBandedTableView;
    GridAnexaColumn1: TcxGridDBBandedColumn;
    DTAnexe: TDataSource;
    MemAnexe: TdxMemData;
    QryAnexe: TZQuery;
    cxGridPopupMenu: TcxGridPopupMenu;
    Label2: TLabel;
    edtPerioada: TcxImageComboBox;
    Label3: TLabel;
    edNrZerouri: TcxImageComboBox;
    edZerouri: TcxSpinEdit;
    procedure FormCreate(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure edtAnexaPropertiesChange(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure GridAnexaCustomDrawCell(Sender: TcxCustomGridTableView;
      ACanvas: TcxCanvas; AViewInfo: TcxGridTableDataCellViewInfo;
      var ADone: Boolean);
    procedure edNrZerouriPropertiesChange(Sender: TObject);
    procedure edZerouriPropertiesChange(Sender: TObject);
    procedure edtPerioadaPropertiesChange(Sender: TObject);
  private
    { Private declarations }
    LstColoane : TList;
    procedure SetTipColumnProperties(aColumn : TcxGridDBBandedColumn; colStru : StructuraColoane);
    procedure WMRefreshField(var Message: TMessage); message WM_REFRESH_ANEXE;    
  public
    { Public declarations }
    procedure RefreshListaAnexe;
    procedure CreateColumnOnBand(aBandIndex : Integer; Unitate : Integer);
    procedure DeschideDataSet;
    procedure BuildColoane;
    procedure CleanBuildColumns;
  end;


implementation

uses
  dxCompsUtile, dateUnit;

{$R *.dfm}

procedure TfrmAnexeCentralizare.RefreshListaAnexe;
var
  aQry : TZQuery;
  lId : Variant;
begin
  lId := edtAnexa.EditValue;
  FillImageCombo(edtAnexa.Properties, 'exec [spLstAnexe]', 'ID_ANEXE_BILANT', 'Denumire');
  if lId <> Null then edtAnexa.EditValue := lId
  else if edtAnexa.Properties.Items.Count>0 then edtAnexa.ItemIndex := 0;
  FillImageCombo(edtPerioada.Properties, 'exec [spLstAnexePerioade]', 'ID', 'Descriere');
end;

procedure TfrmAnexeCentralizare.FormCreate(Sender: TObject);
begin
  LstColoane := TList.Create;
  RefreshListaAnexe;
end;

procedure TfrmAnexeCentralizare.edtAnexaPropertiesChange(Sender: TObject);
begin
  if edtAnexa.EditValue <> null then edtAnexa.Tag := edtAnexa.EditValue
  else edtAnexa.Tag := -1;
  BuildColoane;
 // DeschideDataSet;
  PostMessage(Handle, WM_REFRESH_ANEXE, 0, 0);  
end;

procedure TfrmAnexeCentralizare.BuildColoane;
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
        aColumn.Position.LineCount := 2;
        ZeroMemory(colStru, SizeOf(StructuraColoane));
        colStru^.Captura    := FieldByName('CAPTURA').AsString;
        colStru^.TipColoana := FieldByName('TIP_COLOANA').AsInteger;
        colStru^.SeRepeta := False;
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
        colStru^.Captura    := FieldByName('CAPTURA').AsString;
        colStru^.TipColoana := FieldByName('TIP_COLOANA').AsInteger;
        colStru^.SeRepeta := True;
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
    finally
      Free;
    end;
   GridAnexa.EndUpdate;    
end;


procedure TfrmAnexeCentralizare.SetTipColumnProperties(
  aColumn: TcxGridDBBandedColumn; colStru : StructuraColoane);
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
  if aColumn.Tag <= 0 then
    aColumn.DataBinding.FieldName := colStru.FieldName
  else
    aColumn.DataBinding.FieldName := colStru.FieldName + '_' + IntToStr(aColumn.Tag);
  aColumn.Visible := colStru.Vizibila;
end;

procedure TfrmAnexeCentralizare.FormDestroy(Sender: TObject);
begin
  if Assigned(LstColoane) then
    LstColoane.Free;
end;

procedure TfrmAnexeCentralizare.CreateColumnOnBand(aBandIndex,
  Unitate: Integer);
var
  I : Integer;
  aColumn : TcxGridDBBandedColumn;
begin
  for I := 0 To LstColoane.Count - 1 do begin
    aColumn :=  GridAnexa.CreateColumn;
    aColumn.Position.BandIndex := aBandIndex;
    aColumn.Position.LineCount := 2;
    aColumn.Tag := Unitate;


    if Unitate = -1 then begin
      aColumn.Styles.Header := frmData.cxStyle45;
    end;
    SetTipColumnProperties(aColumn, PStructuraColoane(LstColoane.Items[I])^);
  end;
end;

procedure TfrmAnexeCentralizare.DeschideDataSet;
begin
  if edtAnexa.Tag= -1 then Exit;
  if edtPerioada.Tag  = -1 then Exit;
  QryAnexe.Close;
  QryAnexe.Params.ParamByName('idAnexa').Value := edtAnexa.Tag;
  QryAnexe.Params.ParamByName('idPerioadeFiscale').Value := edtPerioada.Tag;
  QryAnexe.Params.ParamByName('multiplicator').Value := Integer(Integer(edZerouri.Value));
  QryAnexe.Open;
end;

procedure TfrmAnexeCentralizare.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  CleanBuildColumns;
   Action := caFree;
end;

procedure TfrmAnexeCentralizare.GridAnexaCustomDrawCell(
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

procedure TfrmAnexeCentralizare.WMRefreshField(var Message: TMessage);
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

procedure TfrmAnexeCentralizare.edNrZerouriPropertiesChange(
  Sender: TObject);
begin
  if IsNumeric(edNrZerouri.EditValue) then
    edZerouri.Value := StrToInt(edNrZerouri.EditValue)
  else
    edZerouri.Value := 1;
end;

procedure TfrmAnexeCentralizare.edZerouriPropertiesChange(Sender: TObject);
begin
  PostMessage(Handle, WM_REFRESH_ANEXE, 0, 0);
end;

procedure TfrmAnexeCentralizare.edtPerioadaPropertiesChange(
  Sender: TObject);
begin
  if edtPerioada.EditValue <> null then edtPerioada.Tag := edtPerioada.EditValue
  else edtPerioada.Tag := -1;
  PostMessage(Handle, WM_REFRESH_ANEXE, 0, 0);
end;

procedure TfrmAnexeCentralizare.CleanBuildColumns;
var I : Integer;
begin
  for I := LstColoane.Count-1 downto 0 do
     Dispose(PStructuraColoane(LstColoane[I]));
  LstColoane.Clear;    
end;

end.
