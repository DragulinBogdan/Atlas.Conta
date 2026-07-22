unit BugetImpAnexe;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  cxControls, ExtCtrls, StdCtrls, Mask, Buttons, cxGraphics, cxLookAndFeels,
  cxLookAndFeelPainters, Vcl.Menus, cxContainer, cxEdit, cxCustomData, cxStyles,
  cxTL, cxMaskEdit, cxTLdxBarBuiltInMenu,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxCore,
  dxCoreClasses, dxHashUtils, dxSpreadSheetCore, dxSpreadSheetCoreHistory,
  dxSpreadSheetCoreStyles, dxSpreadSheetCoreStrs,
  dxSpreadSheetConditionalFormatting, dxSpreadSheetConditionalFormattingRules,
  dxSpreadSheetClasses, dxSpreadSheetContainers, dxSpreadSheetFormulas,
  dxSpreadSheetHyperlinks, dxSpreadSheetFunctions, dxSpreadSheetStyles,
  dxSpreadSheetGraphics, dxSpreadSheetPrinting, dxSpreadSheetTypes,
  dxSpreadSheetUtils, dxSpreadSheetFormattedTextUtils, dxBarBuiltInMenu,
  dxSpreadSheet, Data.DB, ZAbstractRODataset, ZAbstractDataset, ZDataset,
  cxInplaceContainer, cxDBTL, cxTLData, cxDBEdit, cxDropDownEdit, cxSpinEdit,
  cxCustomListBox, cxListBox, cxTextEdit, cxImageComboBox, cxProgressBar,
  cxButtons, AnexeParametriiLista;

type
  TfrmImportAnexeXLS = class(TForm)
    pnTools: TPanel;
    Label2: TLabel;
    edFileName: TEdit;
    BtnAuto: TcxButton;
    BtnGenerare: TcxButton;
    Progress: TcxProgressBar;
    Label4: TLabel;
    edCamp: TcxImageComboBox;
    Label6: TLabel;
    Label5: TLabel;
    edColoana: TcxImageComboBox;
    edListaAsocieri: TcxListBox;
    Label3: TLabel;
    edCampIdentificare: TcxImageComboBox;
    Label7: TLabel;
    Label8: TLabel;
    edColoanaIdentificare: TcxImageComboBox;
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
    Label11: TLabel;
    edNrZerouri: TcxImageComboBox;
    edZerouri: TcxSpinEdit;
    btnAdauga: TcxButton;
    btnSterge: TcxButton;
    edListaCaption: TcxListBox;
    qryHead: TZQuery;
    pnParam: TPanel;
    Label9: TLabel;
    edtUnitate: TcxPopupEdit;
    Label10: TLabel;
    edtPerioada: TcxImageComboBox;
    Label1: TLabel;
    edtAnexa: TcxImageComboBox;
    btnEditParams: TcxButton;
    DTHead: TDataSource;
    edIdParam: TcxDBTextEdit;
    btnOpenFile: TcxButton;
    openFile: TOpenDialog;
    Excel: TdxSpreadSheet;
    procedure edFileNameChange(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure BtnGenerareClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure btnAdaugaClick(Sender: TObject);
    procedure btnStergeClick(Sender: TObject);
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
    procedure btnEditParamsClick(Sender: TObject);
    procedure btnOpenFileClick(Sender: TObject);
    procedure ExcelActiveSheetChanged(Sender: TObject);
    procedure ExcelProgress(Sender: TObject; Percent: Integer);
  private
    FBook       : TdxSpreadSheetTableView;
    FIdUnitate  : Integer;
    FIdAnexa    : Integer;
    FIdPerioada : Integer;
    FIdAnexeCentParam : Integer;
    { Private declarations }
    procedure InitEXCELFile(FileName: String);
    function HasParams(idAnexaBilant : Integer): Boolean;

    function Pos2Str(Pos: Integer): String;
    procedure RefreshListaAnexe;
    procedure CheckHeaderDateSet;
  protected
    lParamList : TAnexeParamList;
    function IntToCoord(X, Y: Integer): String;
    procedure StoreLastSettings;
    procedure InitParamList;

  public
    { Public declarations }
  end;

procedure ImportaAnexa(idAnexa : Integer);

implementation

{$R *.DFM}

uses
  dxCompsUtile, ZeosDBUtile, dateUnit, Variants, AnexeParametriiCul, CommonDBVar;

procedure ImportaAnexa(idAnexa : Integer);
begin

end;

procedure SetImageEdit(AEdit: TcxImageComboBox; AStrings: TStrings);
var J: Integer;
 begin
   AEdit.Properties.Items.Clear;
   AEdit.Text := '-1';
   for J := 0 to AStrings.Count-1 do
     with AEdit.Properties.Items.Add do begin
       Value := IntToStr(Integer(AStrings.Objects[J]));
       Description := AStrings[J];
     end;
 end;

function GetCellName(Index: Integer): String;
 begin
   //Index := Index + 1;
   if Index > 25 then
      Result := GetCellName(Index div 26 - 1)+GetCellName(Index mod 26)
   else Result := Chr(Index + Ord('A') );
 end;


procedure TfrmImportAnexeXLS.edFileNameChange(Sender: TObject);
begin
  if FileExists(edFileName.Text) then
     InitEXCELFile(edFileName.Text);
end;

procedure TfrmImportAnexeXLS.InitEXCELFile(FileName: String);
var CanSelect: Boolean;
begin
  { Incercam sa incarcam fisierul excel }
  Progress.Position := 0;
  Progress.Properties.Max := 100;
  //try
    Excel.ClearAll;
//  except
//  end;
  try
     Excel.LoadFromFile(edFileName.Text);
     CanSelect := True;
     ExcelActiveSheetChanged(Excel);
     BtnAuto.Enabled := True;
     Excel.History.Clear;
  except
    on E: Exception do begin
       Excel.ClearAll;
       raise EContaHandledError.Create('Eroare la incarcarea datelor din fisier !'#13#10'Eroare : '+E.Message);
    end;
  end;

end;

procedure TfrmImportAnexeXLS.FormCreate(Sender: TObject);
begin
  FIdAnexa := -1;
  FIdUnitate := -1;
  FIdPerioada := -1;
  RefreshListaAnexe;
end;

procedure TfrmImportAnexeXLS.BtnGenerareClick(Sender: TObject);

  function SearchValueByColumn(AColIndex: Integer; const AValue: String): Boolean;
  var
    I: Integer;
  begin
    Result := False;
    for I := 0 to FBook.Rows.Count-1 do
      if SameText(FBook.Cells[I, AColIndex].AsString, AValue) then begin
        Result := True;
        Break;
      end;
  end;

var
  I, J : Integer;
  lPrevDataSet: TDataSet;
  lIdAnexaCol : Integer;
  lSir        : String;
  lValoare    : Currency;
  lCell       : TdxSpreadSheetCell;

begin
  if FIdAnexa= -1 then  begin
    MessageDlg('Setati anexa de import!', mtError, [mbOK], 0);
    edtAnexa.SetFocus;
    Abort;
  end;

  if FIdUnitate= -1 then  begin
    MessageDlg('Setati unitatea de import !', mtError, [mbOK], 0);
    edtUnitate.SetFocus;
    Abort;
  end;
  if FIdPerioada= -1 then  begin
    MessageDlg('Setati perioada fiscala !', mtError, [mbOK], 0);
    edtPerioada.SetFocus;
    Abort;
  end;

  if (edCampIdentificare.EditValue = null) or (edColoanaIdentificare.EditValue = null) then  begin
    MessageDlg('Setati detaliile de identificare (Camp identificare  si coloana excel) !', mtError, [mbOK], 0);
    Abort;
  end;

  DBExecSQLFmt('exec [spCDAnexeCentralizareDel] %d, %d, %d, %d', [FIdAnexeCentParam, FIdUnitate, FIdAnexa, FIdPerioada]);

  lPrevDataSet := DBNewQueryFmt('select *, %s as CampIdentificare from CDAnexeRanduri where  idCDAnexe = %d', [ValueToStr(edCampIdentificare.EditValue, False), FIdAnexa]);
  try
    lPrevDataSet.Open;
    Excel.History.BeginAction(TdxSpreadSheetHistoryFormatCellAction);
    try
      Progress.Position := 0;
      Progress.Properties.Max := FBook.Rows.Count;
      for I := 0 to FBook.Rows.Count-1 do begin
        Progress.Position := I;
        for J := 0 to edListaAsocieri.Count - 1 do begin
          lSir := copy(edListaAsocieri.Items[J], 1, pos('=',edListaAsocieri.Items[J])-1);
          lIdAnexaCol := ValueSafeToInt(lSir, -1);
          if lIdAnexaCol = -1 then begin
            lSir  := copy(edListaAsocieri.Items[J] , pos('=',edListaAsocieri.Items[J]) + 1, length(edListaAsocieri.Items[J]) - pos('=',edListaAsocieri.Items[J]));
            lCell := FBook.Cells[I, StrToInt(lSir)];
            lValoare := ValueSafeToCurrency(lCell.AsCurrency, 0);
            if lValoare <> 0 then begin
              lCell := FBook.Cells[I, edColoanaIdentificare.EditValue];
              if lPrevDataSet.Locate(edCampIdentificare.EditValue, lCell.AsString, [loCaseInsensitive]) then begin
                DBExecSQLFmt('exec [spCDAddAnexeCentralizare] %d, %d, %d, %d, %d, %d, %d, %s',
                  [
                    FIdAnexeCentParam,
                    FIdUnitate,
                    lIdAnexaCol,
                    ValueToStr(lPrevDataSet['id_anexe_randuri']),
                    FIdAnexa,
                    FIdPerioada,
                    IdUtilizator,
                    ValueToStr(lValoare)
                  ]);
              end;
            end;
          end;
        end;
      end;
    finally
      Excel.History.EndAction(False);
    end;
  finally
    lPrevDataSet.Free;
  end;
end;

procedure TfrmImportAnexeXLS.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  if lParamList <> nil then begin
    lParamList.ClearParams;
    lParamList.Free;
  end;

    Action := caFree;
end;

procedure TfrmImportAnexeXLS.ExcelActiveSheetChanged(Sender: TObject);
var
  lCells: TStringList;
  I     : Integer;
begin
  edColoana.EditValue := Null;
  FBook := Excel.ActiveSheet as TdxSpreadSheetTableView;
  lCells := TStringList.Create;
  try
    for I := 0 to FBook.Columns.Count-1 do
      lCells.AddObject(GetCellName(I), TObject(I));
    SetImageEdit(edColoana, lCells);
    SetImageEdit(edColoanaIdentificare, lCells);
  finally
    lCells.Free;
  end;
end;

procedure TfrmImportAnexeXLS.ExcelProgress(Sender: TObject; Percent: Integer);
begin
  if Progress.Position <> Percent then begin
    Progress.Position := Percent;
    Application.ProcessMessages;
  end;
end;

function TfrmImportAnexeXLS.IntToCoord(X, Y: Integer): String;
begin
  Result := Pos2Str(X) + IntToStr(Y);
end;

function TfrmImportAnexeXLS.Pos2Str(Pos: Integer): String;
var
  i, j: Integer;
begin
  if Pos > 26 then
  begin
    i := Pos mod 26;
    j := Pos div 26;
    if i = 0 then
      Result := Chr(64 + j - 1)
    else
      Result := Chr(64 + j);
    if i = 0 then
      Result := Result + chr(90)
    else
      Result := Result + Chr(64 + i);
  end
  else
    Result := Chr(64 + Pos);
end;



procedure TfrmImportAnexeXLS.RefreshListaAnexe;
begin
  FillImageCombo(edtAnexa.Properties, 'exec [spCDLstAnexe]', 'idCDAnexe', 'Denumire');
  FillImageCombo(edtPerioada.Properties, 'exec [spCDLstAnexePerioade]', 'ID', 'Descriere');
end;

procedure TfrmImportAnexeXLS.btnAdaugaClick(Sender: TObject);
begin
  if (edCamp.ItemIndex < 0) or (edColoana.ItemIndex < 0) then begin
    ShowMessage('Alegeti campul si coloana asociata');
    Exit;
  end;
  edListaCaption.Items.Add( edCamp.EditText + '=' + edColoana.EditText);
  edListaAsocieri.Items.Add( edCamp.EditValue + '=' + edColoana.EditValue );
  edCamp.Properties.Items.Delete(edCamp.ItemIndex);
  edColoana.Properties.Items.Delete(edColoana.ItemIndex);
end;

procedure TfrmImportAnexeXLS.btnOpenFileClick(Sender: TObject);
begin
  openFile.FileName := edFileName.Text;
  if openFile.Execute then
    edFileName.Text := openFile.FileName;
end;

procedure TfrmImportAnexeXLS.btnStergeClick(Sender: TObject);
var
  lIndex    : Integer;
  lValues,
  lCaptions : String;
begin
  lIndex := edListaCaption.ItemIndex;
  if (lIndex < 0) then begin
    ShowMessage('Alegeti combinatia pe care doriti s-o stergeti');
    Exit;
  end;
  lValues   := edListaAsocieri.Items[lIndex];
  lCaptions := edListaCaption.Items[lIndex];

  with edCamp.Properties.Items.Add do begin
    Value       := Copy(lValues, 1, pos('=', lValues)-1);
    Description := Copy(lCaptions, 1, pos('=', lCaptions)-1);
  end;
  edCamp.ItemIndex := edCamp.Properties.Items.Count-1;

  with edColoana.Properties.Items.Add do begin
    Value       := Copy(lValues, pos('=', lValues)+1, Length(lValues));
    Description := Copy(lCaptions, pos('=', lCaptions)+1, Length(lCaptions));
  end;
  edColoana.ItemIndex := edColoana.Properties.Items.Count-1;

  edListaAsocieri.Items.Delete(lIndex);
  edListaCaption.Items.Delete(lIndex);
end;

procedure TfrmImportAnexeXLS.edtAnexaPropertiesChange(Sender: TObject);
begin
  edListaAsocieri.Items.Clear;
  edListaCaption.Items.Clear;
  FIdAnexa := ValueSafeToInt(edtAnexa.EditValue, -1);
  edtAnexa.Tag := FIdAnexa;
  if FIdAnexa <> -1 then begin
    btnEditParams.Visible := HasParams(FIdAnexa);
    CheckHeaderDateSet;
    FillImageComboFmt(edCampIdentificare.Properties, 'exec [spCDLstAnexeCapRanduri] %d', [FIdAnexa], 'NumeCamp', 'NumeCamp');
    FillImageComboFmt(edCamp.Properties, 'exec [spCDLstAnexeColoane] %d, 1', [FIdAnexa], 'idCDAnexeColoane', 'CAPTURA');
  end;
end;

procedure TfrmImportAnexeXLS.edtUnitatePropertiesInitPopup(
  Sender: TObject);
begin
  with TcxPopupEdit(Sender).Properties do begin
    if PopupWidth < TcxPopupEdit(Sender).Width then PopupWidth := TcxPopupEdit(Sender).Width;
  end;
end;

procedure TfrmImportAnexeXLS.cxTreeUnitatiDblClick(Sender: TObject);
begin
   with TcxDBTreeList(Sender) do
    if (FocusedNode <> nil) then begin
     (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk;
   end;
end;

procedure TfrmImportAnexeXLS.cxTreeUnitatiKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then begin
     if Assigned(TcxDBTreeList(Sender).OnDblClick) then TcxDBTreeList(Sender).OnDblClick(Sender);
  end
  else if Key = VK_ESCAPE then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrCancel;
end;

procedure TfrmImportAnexeXLS.edtUnitatePropertiesPopup(Sender: TObject);
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

procedure TfrmImportAnexeXLS.edtUnitatePropertiesCloseQuery(
  Sender: TObject; var CanClose: Boolean);
var
  lNode : TcxDBTreeListNode;
  lIdUnitate : Integer;
begin
  FIdUnitate := -1;
  with TAccesscxPopupEdit(Sender) do
    if PopupWindow.ModalResult = mrOk then begin
       lNode := TcxDBTreeListNode(cxTreeUnitati.FocusedNode);
       if Assigned(lNode) then begin
          lIdUnitate := lNode.KeyValue;
          Text := VarToStr(lNode.Values[cxTreeUnitatiDENUMIRE.ItemIndex]);
          if FIdUnitate <> lIdUnitate then begin begin
            FIdUnitate := lIdUnitate;
            CheckHeaderDateSet;
          end;
          end;
       end;
    end;
end;

procedure TfrmImportAnexeXLS.edtPerioadaPropertiesChange(Sender: TObject);
begin
  if edtPerioada.EditValue <> null then edtPerioada.Tag := edtPerioada.EditValue
  else edtPerioada.Tag := -1;
  FIdPerioada := edtPerioada.Tag;
  CheckHeaderDateSet;
end;

procedure TfrmImportAnexeXLS.edNrZerouriPropertiesChange(Sender: TObject);
begin
  if IsNumeric(edNrZerouri.EditValue) then
    edZerouri.Value := StrToInt(edNrZerouri.EditValue)
  else
    edZerouri.Value := 1;
end;

procedure TfrmImportAnexeXLS.CheckHeaderDateSet;
var
  I, ldim : Integer;
  lSearch : String;
  lValueList : Variant;

begin
  BtnGenerare.Enabled := False;
  if (FIdAnexa = -1) or (FIdUnitate = -1) or (FIdPerioada = -1) then Exit;

  InitParamList;

  if (lParamList <> nil) and (btnEditParams.Visible) then
    for I := 0 to lParamList.ParamCount -1  do
      If VarToStr(lParamList.Params[I].Value) = '' then Exit;
      
  if not qryHead.Active then DBRefresh(qryHead);
  lSearch := 'idCDAnexe;idCDAnexeUnitati;idCDAnexePF';
  lValueList := VarArrayCreate([0, 2], varVariant);
  //lValueList := VarArrayOf([FIdAnexa, FIdUnitate, FIdPerioada]);
  lValueList[0] := FIdAnexa;
  lValueList[1] := FIdUnitate;
  lValueList[2] := FIdPerioada;
  ldim := 3;
  if (lParamList <> nil) and (btnEditParams.Visible) then
    for I := 0 to lParamList.ParamCount -1  do begin
      lSearch := lSearch + ';' + lParamList.Params[I].Name;
      ldim := ldim + 1;
      VarArrayRedim(lValueList, lDim);
      lValueList[lDim-1] := lParamList.Params[I].Value;
    end;
  //ShowMessage(VarToStr(lValueList[0]) + ' ' + VarToStr(lValueList[1]) + ' ' + VarToStr(lValueList[2]) + ' ' + VarToStr(lValueList[3]));
  if not qryHead.Locate(lSearch, lValueList, []) then begin
     qryHead.Append;
     qryHead.FieldByName('idCDAnexe').AsInteger := FIdAnexa;
     qryHead.FieldByName('idCDAnexeUnitati').AsInteger := FIdUnitate;
     qryHead.FieldByName('idCDAnexePF').AsInteger := FIdPerioada;
     if (lParamList <> nil) and (btnEditParams.Visible) then
        for I := 0 to lParamList.ParamCount - 1 do
          qryHead.FieldByName(lParamList.Params[I].Name).Value := lParamList.Params[I].Value;
     qryHead.Post;
  end;
  FIdAnexeCentParam := qryHead.FieldByName('idCDAnexeIntrodus').AsInteger;

  BtnGenerare.Enabled := True;
end;

procedure TfrmImportAnexeXLS.btnEditParamsClick(Sender: TObject);
var
  lForm: TfrmAnexeParametriiCul;
begin
   lForm :=  TfrmAnexeParametriiCul.Create(Application);
   lForm.FIdAnexaBilant := FIdAnexa;

   InitParamList;

    with lForm do
    try
      FParams := lParamList;
      FIdAnexaBilant := FIdAnexa;
      ShowModal;
      if FOk then begin
        CheckHeaderDateSet;
        //lParamList.SetAnexaParams(frmData.dbContabilitate);
      end;
    finally
      lForm.Free;
    end;
end;

function TfrmImportAnexeXLS.HasParams(idAnexaBilant: Integer): Boolean;
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

procedure TfrmImportAnexeXLS.StoreLastSettings;
begin

end;

procedure TfrmImportAnexeXLS.InitParamList;
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

end.
