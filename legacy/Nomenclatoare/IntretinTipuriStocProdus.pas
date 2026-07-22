unit IntretinTipuriStocProdus;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxLookAndFeelPainters, StdCtrls, cxButtons, ExtCtrls,
  cxStyles, 
  cxGraphics, cxDataStorage, cxEdit, DB, cxDBData,
  cxGridLevel, cxClasses, cxControls, cxGridCustomView,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGrid,
  cxGridBandedTableView, ZDataSet, cxGridDBBandedTableView, dxmdaset,
  cxImageComboBox, cxTextEdit, 
  cxContainer, cxGroupBox, Menus, cxGridCustomPopupMenu, cxGridPopupMenu,
  DegradePanel,
  cxLookAndFeels, cxCustomData, cxFilter, cxData, cxNavigator, dxDateRanges,
  dxScrollbarAnnotations, dxBarBuiltInMenu;

const
  cst_ExplStr = 'La %s pentru %s pentru un item cu semn %s stocul %s %s cu cantitatea evidentiata';


type
  TfrmIntertinTipStocProdus = class(TForm)
    pnBottom: TPanel;
    btnOk: TcxButton;
    pnContent: TPanel;
    GridTipProdus: TcxGrid;
    GridTipProdusLevel1: TcxGridLevel;
    GridTipProdusBandedTableView1BandedColumn1: TcxGridBandedColumn;
    GridTipProdusBandedTableView1BandedColumn2: TcxGridBandedColumn;
    GridTipProdusBandedTableView1BandedColumn3: TcxGridBandedColumn;
    GridTipProdusBandedTableView1BandedColumn4: TcxGridBandedColumn;
    cxStyleRepository1: TcxStyleRepository;
    GridTipProdusBandedTableView1BandedColumn5: TcxGridBandedColumn;
    GridBandedTableViewStyleSheetUserFormat4: TcxGridBandedTableViewStyleSheet;
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
    GridTipProdusDBBandedTableView1: TcxGridDBBandedTableView;
    MemStoc: TdxMemData;
    DTStoc: TDataSource;
    GridTipProdusDBBandedTableView1DBBandedColumn1: TcxGridDBBandedColumn;
    cxGridPopupMenu: TcxGridPopupMenu;
    PopupMenu1: TPopupMenu;
    DoubleClik1: TMenuItem;
    Cmd_SetCrestere: TMenuItem;
    Cmd_SetDesCrestere: TMenuItem;
    Cmd_SetNull: TMenuItem;
    N1: TMenuItem;
    Cmd_Produse: TMenuItem;
    N2: TMenuItem;
    pnTop: TDegradePanel;
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure FormCreate(Sender: TObject);
    procedure GridTipProdusDBBandedTableView1CustomDrawCell(
      Sender: TcxCustomGridTableView; ACanvas: TcxCanvas;
      AViewInfo: TcxGridTableDataCellViewInfo; var ADone: Boolean);
    procedure GridTipProdusDBBandedTableView1MouseDown(Sender: TObject;
      Button: TMouseButton; Shift: TShiftState; X, Y: Integer);
    procedure btnOkClick(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure GridTipProdusDBBandedTableView1MouseMove(Sender: TObject;
      Shift: TShiftState; X, Y: Integer);
    procedure MemStocAfterScroll(DataSet: TDataSet);
    procedure Cmd_SetNullClick(Sender: TObject);
    procedure Cmd_ProduseClick(Sender: TObject);
  private
    { Private declarations }
    FHintList : TStringList;
    FCurentColumn : Integer;
    //FCurentStocDen : String;
    FCurentIdGestStoc : Integer;
    procedure UnpackColumnInfo(var IDGestTipStoc, IDGestTipDocum, Predator,
      SemnItems: Integer; var lColumnName: String);

    procedure CreateBandsColumns;
    procedure PopulateMemDataSet;
    procedure MEditValue(lFieldName : String; lValue : Variant);
  public
    { Public declarations }
    procedure AskForSave;
    procedure UpdateProdDr;
  end;


implementation

uses
  ZeosDBUtile, DateUnit, InflProduseUnit;

{$R *.dfm}

{ TfrmIntertinTipStocProdus }

procedure TfrmIntertinTipStocProdus.CreateBandsColumns;
var
  lQry  : TZReadOnlyQuery;
  lBand, lBandHead : TcxGridBand;
  lColumn : TcxGridDBBandedColumn;
  lPDIndex, lPMIndex : Integer;
  lCaption : string;
begin
  lCaption := '';
  FHintList.Clear;
  lQry := GetTmpADOQuery;
  with lQry do
    try
      GridTipProdusDBBandedTableView1.BeginUpdate;
      GridTipProdusDBBandedTableView1.Bands.Clear;
      lBand := GridTipProdusDBBandedTableView1.Bands.Add;
      with lBand do begin
        Caption := 'STOC';
        HeaderAlignmentHorz := taCenter;
        HeaderAlignmentVert := vaCenter;
        FixedKind := fkLeft;
        Width := 170;
      end;
      GridTipProdusDBBandedTableView1.DataController.KeyFieldNames := 'ID_GEST_TIP_STOC';

      lColumn := GridTipProdusDBBandedTableView1.CreateColumn;
      with lColumn do begin
        Caption := 'Identificator';
        HeaderAlignmentHorz := taCenter;
        Position.BandIndex := lBand.Index;
        Position.ColIndex := 0;
        Position.RowIndex := 0;
        DataBinding.FieldName := 'ID_GEST_TIP_STOC';
        Name := 'GridTipProdusDBBandedTableView1_ID_GEST_TIP_STOC';
        PropertiesClassName := 'TcxTextEditProperties';
        TcxTextEditProperties(Properties).Alignment.Horz := taCenter;
        TcxTextEditProperties(Properties).Alignment.Vert := taVCenter;
        Options.Editing := False;
        Options.Filtering := False;
        lColumn.Visible := False;
      end;

      lColumn := GridTipProdusDBBandedTableView1.CreateColumn;
      with lColumn do begin
        Caption := 'Denumire';
        HeaderAlignmentHorz := taCenter;
        Position.BandIndex := lBand.Index;
        Position.ColIndex := 0;
        Position.RowIndex := 0;
        DataBinding.FieldName := 'DENUMIRE';
        Name := 'GridTipProdusDBBandedTableView1_DENUMIRE';
        PropertiesClassName := 'TcxTextEditProperties';
        TcxTextEditProperties(Properties).Alignment.Horz := taCenter;
        TcxTextEditProperties(Properties).Alignment.Vert := taVCenter;
      end;
      SQL.Clear;
      SQL.Add('exec sp_gest_get_lista_doc');
      Open;
      First;
      while not eof do begin
        if lCaption <> FieldByName('COD_DOCUM').AsString then begin
          lCaption := FieldByName('COD_DOCUM').AsString;
          lBandHead := GridTipProdusDBBandedTableView1.Bands.Add;
          lBandHead.Width := 130;
          lBandHead.Caption := lCaption;
        end else begin
          lBandHead.Width := lBandHead.Width + 130;
        end;

        lBand := GridTipProdusDBBandedTableView1.Bands.Add;
        lBand.Caption := FieldByName('info').AsString;
        lBand.Width := 130;
        lBand.Position.BandIndex := lBandHead.Index;
        with GridTipProdusDBBandedTableView1.Bands.Add do begin
          Caption := 'PD';
          Position.BandIndex := lBand.Index;
          lPDIndex := Index;
        end;
        with GridTipProdusDBBandedTableView1.Bands.Add do begin
          Caption := 'PM';
          Position.BandIndex := lBand.Index;
          lPMIndex := Index;
        end;

        lColumn := GridTipProdusDBBandedTableView1.CreateColumn;
        with lColumn do begin
          PropertiesClassName := 'TcxImageComboBoxProperties';
          TcxImageComboBoxProperties(Properties).Alignment.Horz := taCenter;
          TcxImageComboBoxProperties(Properties).Alignment.Vert := taVCenter;
          TcxImageComboBoxProperties(Properties).Items.Clear;
          with TcxImageComboBoxItem(TcxImageComboBoxProperties(Properties).Items.Add) do begin
            Value := '-1';
            Description := '-';
          end;
          with TcxImageComboBoxItem(TcxImageComboBoxProperties(Properties).Items.Add) do begin
            Value := '1';
            Description := '+';
          end;
          with TcxImageComboBoxItem(TcxImageComboBoxProperties(Properties).Items.Add) do begin
            Value := Null;
            Description := ' ';
          end;
          Caption := '+';
          HeaderAlignmentHorz := taCenter;
          HeaderAlignmentVert := vaCenter;
           Position.BandIndex  := lPDIndex;
          lColumn.Tag := FieldByName('ID_GEST_DEFA_DOCUM').AsInteger;
          Name := 'GridTipProdusDBBandedTableView1_'+ FieldByName('ID_GEST_DEFA_DOCUM').AsString + '_PD_PLUS';
          FHintList.Values[Name] := Format(cst_ExplStr, [lQry.FieldByName('DEN_DOCUM').AsString, 'predator', '+', '%s', '%s']);
          Options.Filtering := False;
          Options.Editing := False;
          DataBinding.FieldName := 'DOC_' + FieldByName('ID_GEST_DEFA_DOCUM').AsString + '_PD_PLUS';
        end;

        lColumn := GridTipProdusDBBandedTableView1.CreateColumn;
        with lColumn do begin
          PropertiesClassName := 'TcxImageComboBoxProperties';
          TcxImageComboBoxProperties(Properties).Alignment.Horz := taCenter;
          TcxImageComboBoxProperties(Properties).Alignment.Vert := taVCenter;
          TcxImageComboBoxProperties(Properties).Items.Clear;
          with TcxImageComboBoxItem(TcxImageComboBoxProperties(Properties).Items.Add) do begin
            Value := '-1';
            Description := '-';
          end;
          with TcxImageComboBoxItem(TcxImageComboBoxProperties(Properties).Items.Add) do begin
            Value := '1';
            Description := '+';
          end;
          with TcxImageComboBoxItem(TcxImageComboBoxProperties(Properties).Items.Add) do begin
            Value := Null;
            Description := ' ';
          end;

          Caption := '-';
          HeaderAlignmentHorz := taCenter;
          HeaderAlignmentVert := vaCenter;
          Position.BandIndex  := lPDIndex;
          lColumn.Tag := FieldByName('ID_GEST_DEFA_DOCUM').AsInteger;
          Name := 'GridTipProdusDBBandedTableView1_'+ FieldByName('ID_GEST_DEFA_DOCUM').AsString + '_PD_MINUS';
          FHintList.Values[Name] := Format(cst_ExplStr, [lQry.FieldByName('DEN_DOCUM').AsString, 'predator', '-', '%s', '%s']);
          Options.Filtering := False;
          Options.Editing := False;
          DataBinding.FieldName := 'DOC_' + FieldByName('ID_GEST_DEFA_DOCUM').AsString + '_PD_MINUS';
        end;

        lColumn := GridTipProdusDBBandedTableView1.CreateColumn;
        with lColumn do begin
          PropertiesClassName := 'TcxImageComboBoxProperties';
          TcxImageComboBoxProperties(Properties).Alignment.Horz := taCenter;
          TcxImageComboBoxProperties(Properties).Alignment.Vert := taVCenter;
          TcxImageComboBoxProperties(Properties).Items.Clear;
          with TcxImageComboBoxItem(TcxImageComboBoxProperties(Properties).Items.Add) do begin
            Value := '-1';
            Description := '-';
          end;
          with TcxImageComboBoxItem(TcxImageComboBoxProperties(Properties).Items.Add) do begin
            Value := '1';
            Description := '+';
          end;
          with TcxImageComboBoxItem(TcxImageComboBoxProperties(Properties).Items.Add) do begin
            Value := Null;
            Description := ' ';
          end;

          Caption := '+';
          HeaderAlignmentHorz := taCenter;
          HeaderAlignmentVert := vaCenter;
          Position.BandIndex  := lPMIndex;
          lColumn.Tag := FieldByName('ID_GEST_DEFA_DOCUM').AsInteger;
          Name := 'GridTipProdusDBBandedTableView1_'+ FieldByName('ID_GEST_DEFA_DOCUM').AsString + '_PM_PLUS';
          FHintList.Values[Name] := Format(cst_ExplStr, [lQry.FieldByName('DEN_DOCUM').AsString, 'primitor', '+', '%s', '%s']);
          Options.Filtering := False;
          Options.Editing := False;
          DataBinding.FieldName := 'DOC_' + FieldByName('ID_GEST_DEFA_DOCUM').AsString + '_PM_PLUS';
        end;

        lColumn := GridTipProdusDBBandedTableView1.CreateColumn;
        with lColumn do begin
          PropertiesClassName := 'TcxImageComboBoxProperties';
          TcxImageComboBoxProperties(Properties).Alignment.Horz := taCenter;
          TcxImageComboBoxProperties(Properties).Alignment.Vert := taVCenter;
          TcxImageComboBoxProperties(Properties).Items.Clear;
          with TcxImageComboBoxItem(TcxImageComboBoxProperties(Properties).Items.Add) do begin
            Value := '-1';
            Description := '-';
          end;
          with TcxImageComboBoxItem(TcxImageComboBoxProperties(Properties).Items.Add) do begin
            Value := '1';
            Description := '+';
          end;
          with TcxImageComboBoxItem(TcxImageComboBoxProperties(Properties).Items.Add) do begin
            Value := Null;
            Description := ' ';
          end;

          Caption := '-';
          HeaderAlignmentHorz := taCenter;
          HeaderAlignmentVert := vaCenter;
          Position.BandIndex  := lPMIndex;
          lColumn.Tag := FieldByName('ID_GEST_DEFA_DOCUM').AsInteger;
          Name := 'GridTipProdusDBBandedTableView1_'+ FieldByName('ID_GEST_DEFA_DOCUM').AsString + '_PM_MINUS';
          FHintList.Values[Name] := Format(cst_ExplStr, [lQry.FieldByName('DEN_DOCUM').AsString, 'primitor', '-', '%s', '%s']);
          Options.Filtering := False;
          Options.Editing := False;
          DataBinding.FieldName := 'DOC_' + FieldByName('ID_GEST_DEFA_DOCUM').AsString + '_PM_MINUS';
        end;

        Next;
      end;
    finally
      GridTipProdusDBBandedTableView1.DataController.DataSource := DTStoc;
      GridTipProdusDBBandedTableView1.EndUpdate;
      Free;
    end;
end;

procedure TfrmIntertinTipStocProdus.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmIntertinTipStocProdus.FormCreate(Sender: TObject);
begin
  FHintList := TStringList.Create;
  try
    CreateBandsColumns;
  except
  end;
  PopulateMemDataSet;
end;

procedure TfrmIntertinTipStocProdus.PopulateMemDataSet;
var
  lDataSet: TDataSet;
begin
  lDataSet := DBNewQuery('exec [sp_gest_get_def_stoc]');
  try
    lDataSet.Open;
    if MemStoc.Active then MemStoc.Close;
    MemStoc.Active := True;
    MemStoc.CopyFromDataSet(lDataSet);
  finally
    lDataSet.Free;
  end;
end;

procedure TfrmIntertinTipStocProdus.GridTipProdusDBBandedTableView1CustomDrawCell(
  Sender: TcxCustomGridTableView; ACanvas: TcxCanvas;
  AViewInfo: TcxGridTableDataCellViewInfo; var ADone: Boolean);

  function VarIsNotRight(Value : Variant) : Boolean;
  begin
    Result := (VarIsEmpty(Value) or VarIsClear(Value) or VarIsNull(Value));
  end;

var
//  I : Integer;
  lNode : TcxCustomGridRecord;

begin
  ACanvas.Pen.Color := clWhite;
  if AViewInfo = nil then Exit;
  lNode := AViewInfo.RecordViewInfo.GridRecord;
  begin
    if VarIsNotRight(AViewInfo.Value) then begin
      ACanvas.Brush.Color := clGray;
      ACanvas.Font.Name := 'Arial Black';
      ACanvas.Font.Style := ACanvas.Font.Style + [fsBold];
      ACanvas.Font.Size := 11;
    end
    else
    if (AViewInfo.Value = '1') then begin
      ACanvas.Brush.Color := clSkyBlue;
      ACanvas.Font.Color := clRed;
      ACanvas.Font.Style := ACanvas.Font.Style + [fsBold];
      ACanvas.Font.Size := 11;
      ACanvas.Font.Name := 'Arial Black';
      if AViewInfo.Item <> nil then
        if pos('MINUS',AViewInfo.Item.Name) > 0 then
          ACanvas.Font.Color := clBlack;
    end
    else
    if (AViewInfo.Value = '-1') then begin
      ACanvas.Brush.Color := clYellow;
      ACanvas.Font.Name := 'Arial Black';
      ACanvas.Font.Size := 11;
      ACanvas.Font.Color := clRed;
      ACanvas.Font.Style := ACanvas.Font.Style + [fsBold];
      if AViewInfo.Item <> nil then
        if pos('PLUS',AViewInfo.Item.Name) > 0 then
          ACanvas.Font.Color := clBlack;
    end;
   end;
   if (FCurentIdGestStoc <> -1) and (FCurentColumn <> -1) then
     if (((lNode.Values[GridTipProdusDBBandedTableView1.FindItemByName('GridTipProdusDBBandedTableView1_ID_GEST_TIP_STOC').Index] = FCurentIdGestStoc)
          and  ((AViewInfo.Item.Index <= FCurentColumn ) or (FCurentColumn = -1))) or (AViewInfo.Item.Index = FCurentColumn))
        and
        (AViewInfo.Item.Index <> GridTipProdusDBBandedTableView1.FindItemByName('GridTipProdusDBBandedTableView1_ID_GEST_TIP_STOC').Index)

     then begin
       ACanvas.Brush.Color := clGreen;
     end;
end;


procedure TfrmIntertinTipStocProdus.GridTipProdusDBBandedTableView1MouseDown(
  Sender: TObject; Button: TMouseButton; Shift: TShiftState; X,
  Y: Integer);
var
  lHitInfo : TcxCustomGridHitTest;
begin
  lHitInfo := GridTipProdusDBBandedTableView1.GetHitTest(X, Y);
  if lHitInfo = nil then Exit;
  if lHitInfo.ViewInfo = nil then Exit;
  if not(ssDouble in Shift) then Exit;
  if (lHitInfo.HitTestCode <> htCell) then Exit;
  if TcxGridTableDataCellViewInfo(lHitInfo.ViewInfo).Item = nil then Exit;
  if TcxGridTableDataCellViewInfo(lHitInfo.ViewInfo).Item.Properties = nil then Exit;
  if TcxGridTableDataCellViewInfo(lHitInfo.ViewInfo).Item.PropertiesClassName = 'TcxImageComboBoxProperties' then
       if TcxGridTableDataCellViewInfo(lHitInfo.ViewInfo).Value = null then
         MEditValue(TcxGridDBBandedColumn(TcxGridTableDataCellViewInfo(lHitInfo.ViewInfo).Item).DataBinding.FieldName, 1)
       else if TcxGridTableDataCellViewInfo(lHitInfo.ViewInfo).Value = 1 then
         MEditValue(TcxGridDBBandedColumn(TcxGridTableDataCellViewInfo(lHitInfo.ViewInfo).Item).DataBinding.FieldName, -1)
       else
         MEditValue(TcxGridDBBandedColumn(TcxGridTableDataCellViewInfo(lHitInfo.ViewInfo).Item).DataBinding.FieldName, null);
end;

procedure TfrmIntertinTipStocProdus.MEditValue(lFieldName: String;
  lValue: Variant);
var
  lState : Boolean;
  lOldValue : Variant;
  IDGestTipStoc, IDGestTipDocum, Predator, SemnItems : Integer;
  lColumnName : String;
begin
  if lValue = 0 then lValue := null;
  lState := MemStoc.State in [dsEdit, dsInsert];
  if not lState then
    MemStoc.Edit;
  lOldValue := MemStoc.FieldByName(lFieldName).Value;
  MemStoc.FieldByName(lFieldName).Value := lValue;
  MemStoc.Post;
  if lState then MemStoc.Edit;
  MemStoc.FieldByName(lFieldName).Value;

  UnpackColumnInfo(IDGestTipStoc, IDGestTipDocum, Predator, SemnItems, lColumnName);
  InfluentareTipProduse(IDGestTipStoc, IDGestTipDocum, Predator, SemnItems, MemStoc.FieldByName(lFieldName).Value, lOldValue);
end;

procedure TfrmIntertinTipStocProdus.btnOkClick(Sender: TObject);
begin
  AskForSave;
  if fsModal in FormState then ModalResult := mrOk
  else Close;
end;

procedure TfrmIntertinTipStocProdus.FormDestroy(Sender: TObject);
begin
  FHintList.Free;
end;

procedure TfrmIntertinTipStocProdus.AskForSave;
begin

end;

procedure TfrmIntertinTipStocProdus.GridTipProdusDBBandedTableView1MouseMove(
  Sender: TObject; Shift: TShiftState; X, Y: Integer);
var
  aHitInfo : TcxCustomGridHitTest ;
  lNode : TcxCustomGridRecord;
  lStrVariatie : String;
begin
  FCurentColumn := -1;
  //FCurentStocDen := '';
  FCurentIdGestStoc := -1;
  aHitInfo := GridTipProdusDBBandedTableView1.GetHitTest(X, Y);
  if aHitInfo = nil then Exit;
  if aHitInfo.ViewInfo = nil then Exit;
  if (aHitInfo.HitTestCode <> htCell) then Exit;
  lStrVariatie := '';
  lNode := TcxGridTableDataCellViewInfo(aHitInfo.ViewInfo).RecordViewInfo.GridRecord;
  if TcxGridTableDataCellViewInfo(aHitInfo.ViewInfo).Item = nil then Exit;
  FCurentColumn := TcxGridDBBandedColumn(TcxGridTableDataCellViewInfo(aHitInfo.ViewInfo).Item).Index;
  if (lNode.Values[FCurentColumn] = null ) or (lNode.Values[FCurentColumn] = '0') then
    lStrVariatie := 'nu se modifica'
  else if lNode.Values[FCurentColumn] = '1' then
    lStrVariatie := 'creste'
  else if lNode.Values[FCurentColumn] = '-1' then
    lStrVariatie := 'descreste';
  if lStrVariatie = '' then FCurentColumn := -1;

  FCurentIdGestStoc := lNode.Values[GridTipProdusDBBandedTableView1.FindItemByName('GridTipProdusDBBandedTableView1_ID_GEST_TIP_STOC').Index];

  SetHintInfo(Format(FHintList.Values[TcxGridTableDataCellViewInfo(aHitInfo.ViewInfo).Item.Name],
                [lNode.Values[GridTipProdusDBBandedTableView1.FindItemByName('GridTipProdusDBBandedTableView1_DENUMIRE').Index],
                lStrVariatie
                ]) );

  GridTipProdusDBBandedTableView1.Invalidate();

end;

procedure TfrmIntertinTipStocProdus.MemStocAfterScroll(DataSet: TDataSet);
begin
  UpdateProdDr;
end;

procedure TfrmIntertinTipStocProdus.UpdateProdDr;
begin
end;

procedure TfrmIntertinTipStocProdus.Cmd_SetNullClick(Sender: TObject);
begin
  if (FCurentColumn = -1) or (FCurentIdGestStoc = -1) then Exit;
  MEditValue(GridTipProdusDBBandedTableView1.Columns[FCurentColumn].DataBinding.FieldName, TComponent(Sender).Tag)
end;

procedure TfrmIntertinTipStocProdus.UnpackColumnInfo(var IDGestTipStoc, IDGestTipDocum, Predator, SemnItems : Integer; var lColumnName : String);
begin
  IDGestTipStoc := -1;
  IDGestTipDocum := -1;
  Predator := -1;
  SemnItems := -1;
  lColumnName := '';
  IDGestTipStoc := MemStoc.FieldByName('id_gest_tip_stoc').AsInteger;
  lColumnName := GridTipProdusDBBandedTableView1.Columns[FCurentColumn].Name;
  IDGestTipDocum := GridTipProdusDBBandedTableView1.Columns[FCurentColumn].Tag;
  if pos('_PD_', lColumnName ) > 0 then Predator := 1
  else Predator := 2;
  if pos('_PLUS', lColumnName ) > 0 then SemnItems := 1
  else SemnItems := -1;

  lColumnName := 'DOC_' + IntToStr(IDGestTipDocum) + '_';
  if Predator = 1  then lColumnName := lColumnName  + 'PD_'  else lColumnName := lColumnName  + 'PM_';
  if SemnItems = 1 then lColumnName := lColumnName  + 'PLUS' else lColumnName := lColumnName  + 'MINUS';
end;

procedure TfrmIntertinTipStocProdus.Cmd_ProduseClick(Sender: TObject);
var
  IDGestTipStoc : Integer;
  Predator, SemnItems : Integer;
  lColumnName : String;
  IDGestTipDocum : Integer;
  Semn : Variant;

begin
  if (not MemStoc.Active) or (MemStoc.FindField('id_gest_tip_stoc') = nil) then Exit;
  UnpackColumnInfo(IDGestTipStoc, IDGestTipDocum, Predator, SemnItems, lColumnName);
  if lColumnName <> '' then
    Semn := MemStoc.FieldByName(lColumnName).Value;
  InfluentareTipProduse(IDGestTipStoc, IDGestTipDocum, Predator, SemnItems, Semn, -999);
//  UpdateTipProdus(IDGestTipStoc, IDGestTipDocum, Predator, SemnItems, Semn);
end;

end.
