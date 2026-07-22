unit NoteUnitNew;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  Db, ZDataSet, Buttons, StdCtrls, ExtCtrls, cxControls, cxContainer, cxEdit, cxTextEdit, cxMaskEdit,
  cxDropDownEdit, cxDBLookupComboBox,
  cxLookAndFeelPainters, cxButtons, Menus,  cxGraphics, cxImageComboBox, dxExEdtr, dxDBGrid,
  dxDBTLCl, dxGrClms, dxDBCtrl, dxTL, dxCntner, 
  dxEditor, ZAbstractRODataset, ZAbstractDataset,
  cxDataStorage, cxDBData,
  cxGridLevel, cxClasses, cxGridCustomView, cxGridCustomTableView,
  cxGridTableView, cxGridDBTableView, cxGrid, cxCalendar, cxCheckBox,
  cxGridCustomPopupMenu, cxGridPopupMenu,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, Vcl.ComCtrls,
  dxCore, cxDateUtils, cxNavigator, cxCurrencyEdit, dxDateRanges,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxBarBuiltInMenu,
  dxScrollbarAnnotations;

type
  TFrmListaNoteNew = class(TForm)
    DTListaNote: TDataSource;
    QryListaNote: TZQuery;
    pnClient: TPanel;
    pnTop: TPanel;
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    LbCurent: TLabel;
    Label4: TLabel;
    btnIstoric: TcxButton;
    edOperator: TcxImageComboBox;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    GridIstoricNoteL: TcxGridLevel;
    cxGridIstoricNote: TcxGrid;
    GridIstoricNote: TcxGridDBTableView;
    GridIstoricNoteJURNAL: TcxGridDBColumn;
    GridIstoricNoteNRDOC: TcxGridDBColumn;
    GridIstoricNoteDATA: TcxGridDBColumn;
    GridIstoricNoteEXPLICATIE: TcxGridDBColumn;
    GridIstoricNoteCONT_DEBT: TcxGridDBColumn;
    GridIstoricNoteREPARTITOR_DEBIT: TcxGridDBColumn;
    GridIstoricNoteCONT_CRED: TcxGridDBColumn;
    GridIstoricNoteREPARTITOR_CREDIT: TcxGridDBColumn;
    GridIstoricNoteVALOARE: TcxGridDBColumn;
    GridIstoricNoteMODUL: TcxGridDBColumn;
    GridIstoricNoteBUGET: TcxGridDBColumn;
    GridIstoricNoteCOD: TcxGridDBColumn;
    GridIstoricNotePOZ: TcxGridDBColumn;
    GridIstoricNoteECL: TcxGridDBColumn;
    GridIstoricNoteCOMPUSA: TcxGridDBColumn;
    GridIstoricNoteCONTD: TcxGridDBColumn;
    GridIstoricNoteCONTC: TcxGridDBColumn;
    GridIstoricNoteC_O: TcxGridDBColumn;
    GridIstoricNoteDATA_OPERARE: TcxGridDBColumn;
    GridIstoricNoteID_INITIAL: TcxGridDBColumn;
    GridIstoricNoteID_PARINTE: TcxGridDBColumn;
    GridIstoricNoteSTARE: TcxGridDBColumn;
    GridIstoricNoteCOD_FUNCTIONAL: TcxGridDBColumn;
    GridIstoricNoteCOD_ECONOMIC: TcxGridDBColumn;
    GridIstoricNoteDATA_OP: TcxGridDBColumn;
    GridIstoricNoteNR_OP: TcxGridDBColumn;
    GridIstoricNoteDATA_CONTRACT: TcxGridDBColumn;
    GridIstoricNoteNR_CONTRACT: TcxGridDBColumn;
    GridIstoricNoteID: TcxGridDBColumn;
    edListaAni: TcxImageComboBox;
    edListaLuni: TcxImageComboBox;
    edNrNota: TcxTextEdit;
    edData: TcxDateEdit;
    edNrUnic: TcxTextEdit;
    ChkShowAll: TcxCheckBox;
    GridIstoricNoteDENJURNAL: TcxGridDBColumn;
    GridIstoricNoteNumeOperator: TcxGridDBColumn;
    cxGridPopupMenu: TcxGridPopupMenu;
    procedure aGridIstoricNoteCustomDraw(Sender: TObject; ACanvas: TCanvas;
      ARect: TRect; ANode: TdxTreeListNode; AColumn: TdxDBTreeListColumn;
      const AText: String; AFont: TFont; var AColor: TColor; ASelected,
      AFocused: Boolean; var ADone: Boolean);
    procedure FormCreate(Sender: TObject);
    procedure aGridIstoricNoteChangeNode(Sender: TObject; OldNode,
      Node: TdxTreeListNode);
    procedure aGridIstoricNoteFilterChanged(Sender: TObject;
      ADataSet: TDataSet; const AFilterText: String);
    procedure QryListaNoteAfterOpen(DataSet: TDataSet);
    procedure FormShow(Sender: TObject);
    procedure aGridIstoricNoteKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure BtnCancelClick(Sender: TObject);
    procedure aGridIstoricNoteCODGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure btnIstoricClick(Sender: TObject);
    procedure GridIstoricNoteFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure GridIstoricNoteCustomDrawCell(Sender: TcxCustomGridTableView;
      ACanvas: TcxCanvas; AViewInfo: TcxGridTableDataCellViewInfo;
      var ADone: Boolean);
    procedure GridIstoricNoteKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure GridIstoricNoteDataControllerFilterChanged(Sender: TObject);
    procedure edListaAniPropertiesChange(Sender: TObject);
    procedure edNrNotaPropertiesChange(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
  private
    FCurentMonth: Integer;
    FForceCopy: Boolean;
    procedure SetCurentMonth(const Value: Integer);
  private
    CurentNr: String;
    CurentPoz: String;
    CurentId : String;
    OldFilter: String;
    FIsFirstTime : Boolean;    
    { Private declarations }
  public
    { Public declarations }
    procedure RefreshFilter;
    procedure SetModalForm;
    function GetNotaFromBack: Integer;
    property CurentMonth: Integer read FCurentMonth write SetCurentMonth;
    property ForceCopy : Boolean read FForceCopy write FForceCopy;
  end;

implementation

{$R *.DFM}

uses
  ZeosDBUtile, dxCompsUtile, DateUnit, Variants, CommonDBVar, IstoricNota;

procedure TFrmListaNoteNew.aGridIstoricNoteCustomDraw(Sender: TObject;
  ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  AColumn: TdxDBTreeListColumn; const AText: String; AFont: TFont;
  var AColor: TColor; ASelected, AFocused: Boolean; var ADone: Boolean);
var aValue: Currency;
begin
  if ANode.HasChildren then Exit;
  if AFocused then begin
     AColor := clBlue;
     AFont.Color := clYellow;
     Exit;
  end;
  if ANode.Strings[GridIstoricNoteSTARE.Index] <> '1' then
     AFont.Style := AFont.Style + [fsStrikeOut];
  if CurentNr = ANode.Strings[GridIstoricNoteCOD.Index] then begin
     if ANode.Strings[GridIstoricNoteID_PARINTE.Index] = CurentPoz then
        AColor := clTeal
      else if VarToStr(TdxDBGridNode(ANode).Id) = CurentPoz then
              AColor := clGreen
           else AColor := clAqua;
  end;
  if Trim(aNode.Strings[GridIstoricNoteVALOARE.Index]) > '' then
    try
       aValue := Trunc(aNode.Values[GridIstoricNoteVALOARE.Index]);
    except
       aValue := 0;
    end
  else aValue := 0;
  if aValue < 0 then begin AFont.Color := clRed; AFont.Style := AFont.Style + [fsBold]; end;
end;

procedure LoadRepartitori(ARow: TdxDBGridImageColumn; Intern: Integer);
var OldPoz : TBookmark;
    lValField,
    lDescField: TField;
begin
  ARow.Values.Clear;
  ARow.Descriptions.Clear;

  lValField := FrmData.QryRepartitori.FindField('ID_REPARTITORI');
  lDescField := FrmData.QryRepartitori.FindField('NUME');
  with FrmData.QryRepartitori do begin
    OldPoz := GetBookmark;
    DisableControls;
    try
       First;
       while not Eof do begin
         if (Intern = 2) or
            ( (Intern = 0) and (not FrmData.QryRepartitori.FieldByName('GESTINT').AsBoolean)) or
            ( (Intern = 1) and (FrmData.QryRepartitori.FieldByName('GESTINT').AsBoolean)) then begin
            aRow.Values.Add(lValField.AsString);
            aRow.Descriptions.Add(lDescField.AsString);
         end;
         Next;
       end;
    finally
       GotoBookmark(OldPoz);
       FreeBookmark(OldPoz);
       EnableControls;
    end;
  end;
end;

procedure TFrmListaNoteNew.FormCreate(Sender: TObject);
var
  lFindItem : TcxImageComboBoxItem;
begin

  FIsFirstTime  := True;
  FForceCopy    := False;
  FillImageCombo(edOperator.Properties, frmData.QryOperatori, 'ID_UTILIZATORI', 'NUMEINTREG', Null, 'Toti Utilizatorii');
  FillImageCombo(edListaAni.Properties, 'exec [spNoteGetAni]', 0, 0, 0, 'Toti Anii');
  if DBProcExists('spListaModuleImport') then
    FillImageComboFmt(GridIstoricNoteMODUL.Properties, 'exec [spListaModuleImport] %d, %d', [IdLogin, IdUtilizator], 0, 1);
  CurentNr := '';

  lFindItem := edListaAni.Properties.FindItemByValue(IntToStr(AnFiscal));
  if Assigned(lFindItem) then
    edListaAni.EditValue := AnFiscal
  else
  if edListaAni.Properties.Items.Count > 0 then
    edListaAni.ItemIndex := edListaAni.Properties.Items.Count -1;

  StorageReadCxView(GridIstoricNote);
end;

procedure TFrmListaNoteNew.FormDestroy(Sender: TObject);
begin
  StorageWriteCxView(GridIstoricNote);
end;

procedure TFrmListaNoteNew.aGridIstoricNoteChangeNode(Sender: TObject; OldNode,
  Node: TdxTreeListNode);
var lParent: Integer;
begin
  if not Assigned(Node) then Exit;
  BtnOk.Enabled := Node.Strings[GridIstoricNoteSTARE.Index] = '1';

  CurentNr := Node.Strings[GridIstoricNoteCOD.Index];
  if Trim(Node.Strings[GridIstoricNoteID_PARINTE.Index]) > '' then
    try
      lParent := Node.Values[GridIstoricNoteID_PARINTE.Index];
    except
      lParent := -1;
    end
  else lParent := -1;
  if lParent = -1 then CurentPoz := VarToStr(TdxDBGridNode(Node).Id)
  else CurentPoz := IntToStr(lParent);

  CurentId := VarToStr(TdxDBGridNode(Node).Id);
  LbCurent.Caption := CurentPoz + ' : '+CurentNr;
  GridIstoricNote.Invalidate;
end;

procedure TFrmListaNoteNew.SetCurentMonth(const Value: Integer);
begin
  FCurentMonth := Value;
  if FCurentMonth > 0 then RefreshFilter;
end;

procedure TFrmListaNoteNew.RefreshFilter;
var OldActive: Boolean;
    lFilter  : String;

    procedure Add2Filter(ACondition: Boolean; AString: String);
     begin
       if ACondition then begin
          if lFilter > '' then lFilter := lFilter + ' AND ';
          lFilter := lFilter + AString;
       end;
     end;
     
begin
  lFilter := '';
  Add2Filter((edNrUnic.Text) > '', 'NR_OP='+Trim(edNrUnic.Text));
  Add2Filter(not ChkShowAll.Checked, 'STARE=1');
//  Add2Filter(GridIstoricNote.FilterRow.InfoText > '', GridIstoricNote.FilterRow.InfoText);
  Add2Filter(edListaAni.EditValue > 0, 'YEAR(DATA) = '+VarToStr(edListaAni.EditValue));
  Add2Filter(edListaLuni.EditValue > 0, 'MONTH(DATA) = '+VarToStr(edListaLuni.EditValue));
  Add2Filter(Trim(edNrNota.Text) > '', 'NRDOC LIKE '+Trim(QuotedStr(edNrNota.Text)));
  if IsValidDate(edData.EditValue) then
  Add2Filter(pos(' ', edData.Text) = 0, 'DATA = CONVERT(DATETIME, '+QuotedStr(FormatDateTime('dd/mm/yyyy', edData.Date))+', 103)');
  if (not VarIsEmpty(edOperator.EditValue)) and (not VarIsNull(edOperator.EditValue)) and (edOperator.EditValue <> -1) then
     Add2Filter(True, 'C_O = '+VarToStr(edOperator.EditValue));
  if lFilter > '' then lFilter := 'WHERE '+lFilter;
  if OldFilter <> lFilter then begin
     OldActive := QryListaNote.Active;
     if OldActive then QryListaNote.Active := False;
     QryListaNote.Sql[4] := lFilter;
     try   
       if OldActive then QryListaNote.Active := True;
       OldFilter := lFilter;
     except
       QryListaNote.Sql[4] := OldFilter;
       QryListaNote.Active := True;
     end;
  end;
end;

procedure TFrmListaNoteNew.aGridIstoricNoteFilterChanged(Sender: TObject;
  ADataSet: TDataSet; const AFilterText: String);
begin
  RefreshFilter;  
end;

function TFrmListaNoteNew.GetNotaFromBack: Integer;
begin
  { Stornam complet nota selectat si o aducem in ecranul de culegere }
  Result := -1;
  if (GridIstoricNote.Controller.FocusedRecord <> nil) and (GridIstoricNote.Controller.FocusedRecord.IsData) then begin
     Result := StrToInt(CurentNr);
     DBExecSQLFmt('EXEC spCNoteModificaNota %d, %d, %s', [Result, IdUtilizator, ValueToStr(CurentId)]);
  end;
end;

procedure TFrmListaNoteNew.QryListaNoteAfterOpen(DataSet: TDataSet);
begin
{
  aGridIstoricNoteChangeNode(aGridIstoricNote, nil, aGridIstoricNote.TopNode);
  aGridIstoricNote.ApplyBestFit(nil);
}
  if FIsFirstTime then begin
    FIsFirstTime := False;
    cxCreateMissingColumns(QryListaNote, GridIstoricNote);
  end;
  GridIstoricNote.ApplyBestFit;
  btnIstoric.Enabled := (not QryListaNote.IsEmpty) and (GridIstoricNote.Controller.FocusedItem <> nil);
end;

procedure TFrmListaNoteNew.FormShow(Sender: TObject);
begin
  WindowState := wsMaximized;
end;

procedure TFrmListaNoteNew.aGridIstoricNoteKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if (Key = VK_RETURN) and (ssCtrl in Shift)
     and (BtnOk.Enabled) then BtnOk.Click;
end;

procedure TFrmListaNoteNew.BtnCancelClick(Sender: TObject);
begin
  Close;
end;

procedure TFrmListaNoteNew.SetModalForm;
begin
  BtnOk.ModalResult := mrOk;
  BtnOk.OnClick     := nil;
end;

procedure TFrmListaNoteNew.aGridIstoricNoteCODGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
var lNode: TdxTreeListNode;
begin
  { Pentru informatiile din grupare }
  if (Assigned(ANode)) and (ANode.HasChildren) then begin
     lNode := ANode.Items[0];
     while (Assigned(lNode)) and (lNode.HasChildren) and
           (Assigned(lNode.Items[0])) do lNode := lNode.Items[0];
     if Assigned(lNode) then
        AText := 'Nota : '+lNode.Strings[GridIstoricNoteNRDOC.Index]+' din : '+lNode.Strings[GridIstoricNoteDATA.Index];
  end;
end;

procedure TFrmListaNoteNew.btnIstoricClick(Sender: TObject);
begin
  with TfrmIstoricNota.Create(Application) do
  begin
    qryNota.SQL.Text := 'Select * from cnote where nrdoc = ''' + QryListaNote.FieldByName('NRDOC').AsString+'''';
    qryNota.Open;
    WindowState := wsMaximized;
    Caption := 'Istoric Nota [Numar: '+ QryListaNote.FieldByName('NRDOC').AsString + ']';
    ShowModal;
  end;
end;

procedure TFrmListaNoteNew.GridIstoricNoteFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
var lParent: Integer;
begin
  if not Assigned(AFocusedRecord) then Exit;
  if not AFocusedRecord.IsData then Exit;

  BtnOk.Enabled :=
    (AFocusedRecord.DisplayTexts[GridIstoricNoteSTARE.Index] = '1')
    or FForceCopy;

  CurentNr := AFocusedRecord.DisplayTexts[GridIstoricNoteCOD.Index];
  if Trim(AFocusedRecord.DisplayTexts[GridIstoricNoteID_PARINTE.Index]) > '' then
    try
      lParent := AFocusedRecord.Values[GridIstoricNoteID_PARINTE.Index];
    except
      lParent := -1;
    end
  else lParent := -1;
  if lParent = -1 then CurentPoz := VarToStr(AFocusedRecord.Values[GridIstoricNoteId.Index])
  else CurentPoz := IntToStr(lParent);

  CurentId := VarToStr(AFocusedRecord.Values[GridIstoricNoteId.Index]);
  LbCurent.Caption := CurentPoz + ' : '+CurentNr;
  GridIstoricNote.Invalidate;
end;

procedure TFrmListaNoteNew.GridIstoricNoteCustomDrawCell(
  Sender: TcxCustomGridTableView; ACanvas: TcxCanvas;
  AViewInfo: TcxGridTableDataCellViewInfo; var ADone: Boolean);
var aValue: Currency;
begin
  if not Assigned(AViewInfo.Item) then Exit;
  if AViewInfo.GridRecord.Focused then begin
     ACanvas.Brush.Color := clBlue;
     ACanvas.Font.Color := clYellow;
     Exit;
  end;
  if AViewInfo.GridRecord.DisplayTexts[GridIstoricNoteSTARE.Index] <> '1' then
     ACanvas.Font.Style := ACanvas.Font.Style + [fsStrikeOut];
  if CurentNr = AViewInfo.GridRecord.DisplayTexts[GridIstoricNoteCOD.Index] then begin
     if AViewInfo.GridRecord.DisplayTexts[GridIstoricNoteID_PARINTE.Index] = CurentPoz then
        ACanvas.Brush.Color := clTeal
      else if VarToStr(AViewInfo.Value) = CurentPoz then
              ACanvas.Brush.Color := clGreen
           else ACanvas.Brush.Color := clAqua;
  end;
  if Trim(AViewInfo.GridRecord.DisplayTexts[GridIstoricNoteVALOARE.Index]) > '' then
    try
       aValue := Trunc(AViewInfo.GridRecord.Values[GridIstoricNoteVALOARE.Index]);
    except
       aValue := 0;
    end
  else aValue := 0;
  if aValue < 0 then begin ACanvas.Font.Color := clRed; ACanvas.Font.Style := ACanvas.Font.Style + [fsBold]; end;
end;

procedure TFrmListaNoteNew.GridIstoricNoteKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if (Key = VK_RETURN) and (ssCtrl in Shift)
     and (BtnOk.Enabled) then BtnOk.Click;
end;

procedure TFrmListaNoteNew.GridIstoricNoteDataControllerFilterChanged(
  Sender: TObject);
begin
  RefreshFilter;
end;

procedure TFrmListaNoteNew.edListaAniPropertiesChange(Sender: TObject);
const
  csLunaSQL: String = 'exec [spNoteGetLuniAn] %s';
var
  I: Integer;
begin
  FillImageComboFmt(edListaLuni.Properties, csLunaSQL, [ValueToStr(edListaAni.EditValue)], 0, 0, 0, 'Toate Lunile');
  if edListaLuni.Properties.Items.Count > 0 then begin
    for I := 0 to edListaLuni.Properties.Items.Count - 1 do
      if I in [Low(LongMonthNames)..High(LongMonthNames)] then
        edListaLuni.Properties.Items[I].Description := LongMonthNames[I];
    edListaLuni.ItemIndex :=  edListaLuni.Properties.Items.Count - 1; //edListaLuni.Values[edListaLuni.Values.Count-1];
  end;
end;

procedure TFrmListaNoteNew.edNrNotaPropertiesChange(Sender: TObject);
begin
  RefreshFilter;
end;

end.
