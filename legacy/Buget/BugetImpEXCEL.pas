unit BugetImpEXCEL;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  cxControls, ExtCtrls, StdCtrls, Mask, cxContainer, cxEdit, cxGroupBox,
  cxRepartitorPanel, cxGraphics, cxButtons, cxTextEdit, cxMaskEdit, cxDropDownEdit,
  cxImageComboBox, cxTL, cxInplaceContainer, cxTLData, cxDBTL, cxProgressBar,
  cxLookAndFeelPainters, Menus, cxCheckBox, cxTLdxBarBuiltInMenu, cxLookAndFeels,
  cxCustomData, cxStyles, cxPC, OleCtrls, SHDocVw, OleServer, ExcelXP, dxCore,
  dxCoreClasses, dxHashUtils, dxSpreadSheetCore, dxSpreadSheetCoreHistory,
  dxSpreadSheetConditionalFormatting, dxSpreadSheetConditionalFormattingRules,
  dxSpreadSheetClasses, dxSpreadSheetContainers, dxSpreadSheetFormulas,
  dxSpreadSheetHyperlinks, dxSpreadSheetFunctions, dxSpreadSheetGraphics,
  dxSpreadSheetPrinting, dxSpreadSheetTypes, dxSpreadSheetUtils,
  dxBarBuiltInMenu, dxSpreadSheet, dxScrollbarAnnotations;

type
  TExcelView = (evcxSpreadSheet, evWebBrowser, evOle);

const
  cst_ExcelView =  evcxSpreadSheet;

type
  TfrmBxImportEXCEL = class(TForm)
    pnTools: TPanel;
    Label1: TLabel;
    Label2: TLabel;
    edFileName: TEdit;
    Label3: TLabel;
    Label4: TLabel;
    Label5: TLabel;
    Label6: TLabel;
    Label7: TLabel;
    ClasaFunctionala: TcxRepartitorPanel;
    BtnAuto: TcxButton;
    BtnGenerare: TcxButton;
    edClasaEconomica: TcxImageComboBox;
    edTrim1: TcxImageComboBox;
    edTrim2: TcxImageComboBox;
    edTrim3: TcxImageComboBox;
    edTrim4: TcxImageComboBox;
    TreeBugete: TcxDBTreeList;
    TreeBugeteCOD_BUGET: TcxDBTreeListColumn;
    TreeBugeteDENUMIRE: TcxDBTreeListColumn;
    TreeBugeteDESCRIERE: TcxDBTreeListColumn;
    TreeBugeteID_OI_UNITATI: TcxDBTreeListColumn;
    TreeBugeteID_OI_PROIECTE: TcxDBTreeListColumn;
    pbProgress: TcxProgressBar;
    ckSameSettings: TcxCheckBox;
    edRestante: TcxImageComboBox;
    Label8: TLabel;
    Label9: TLabel;
    edEstimat1: TcxImageComboBox;
    Label10: TLabel;
    edEstimat2: TcxImageComboBox;
    Label11: TLabel;
    edEstimat3: TcxImageComboBox;
    btnOpenFile: TcxButton;
    fileOpen: TOpenDialog;
    procedure edFileNameChange(Sender: TObject);
    procedure ExcelActiveSheetChanging(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure BtnAutoClick(Sender: TObject);
    procedure BtnGenerareClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure ClasaFunctionalaPopupInitPopup(Sender: TObject);
    procedure TreeBugeteDESCRIEREGetDisplayText(Sender: TcxTreeListColumn;
      ANode: TcxTreeListNode; var Value: String);
    procedure TreeBugeteDblClick(Sender: TObject);
    procedure TreeBugeteKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure edTrim1PropertiesEditValueChanged(Sender: TObject);
    procedure edTrim2PropertiesEditValueChanged(Sender: TObject);
    procedure edTrim3PropertiesEditValueChanged(Sender: TObject);
    procedure edTrim4PropertiesEditValueChanged(Sender: TObject);
    procedure edClasaEconomicaPropertiesEditValueChanged(Sender: TObject);
    procedure ExcelProgress(Sender: TObject; Percent: Integer);
    procedure ClasaFunctionalaButtonClick(Sender: TObject);
    procedure TreeBugeteCustomDrawDataCell(Sender: TcxCustomTreeList;
      ACanvas: TcxCanvas; AViewInfo: TcxTreeListEditCellViewInfo;
      var ADone: Boolean);
    procedure edRestantePropertiesEditValueChanged(Sender: TObject);
    procedure edEstimat1PropertiesEditValueChanged(Sender: TObject);
    procedure edEstimat2PropertiesEditValueChanged(Sender: TObject);
    procedure edEstimat3PropertiesEditValueChanged(Sender: TObject);
    procedure WebBrowserNavigateComplete2(ASender: TObject; const pDisp: IDispatch; const URL: OleVariant);
    procedure WebBrowserProgressChange(Sender: TObject; Progress,
      ProgressMax: Integer);
    procedure ExcelAppSheetActivate(ASender: TObject;
      const Sh: IDispatch);
    procedure FormDestroy(Sender: TObject);
    procedure btnOpenFileClick(Sender: TObject);
  private
    { Private declarations }


    FClasaEconomica: Integer;
    FTrim1: Integer;
    FTrim2: Integer;
    FTrim3: Integer;
    FTrim4: Integer;
    FRestante: Integer;
    FEstimat3: Integer;
    FEstimat1: Integer;
    FEstimat2: Integer;

    FExcelIsLoaded  : Boolean;
    FdxSheet        : TdxSpreadSheetTableView;

    ExcelViewer     : TWinControl;
    ExcelApp        : TExcelApplication;
    FExcelOLESheet  : TExcelWorksheet;
    FIsWebViewer    : Boolean;
    procedure ExcelLoadFromFile(FileName : String);
    procedure ExcelInitControl;
    procedure ExcelDeInitControl;
    procedure ExcelInitBrowser;
    procedure ExcelDeinitBrowser;
    procedure ExcelInitcxSpreadSeet;
    procedure ExcelFormatRow(ACol: Integer; AColor : Integer; IsCurrency: Boolean=True);
    procedure ExcelOnChangeSheet;
    function ExcelGetRowCount : Integer;
    function ExcelGetColumnCount : Integer;
    function ExcelGetText(ARow, ACol : Integer) : String;
    procedure ExcelClearCells(const ARowStart,AColStart, ARowStop, AColStop : Integer);
    procedure InitEXCELFile(FileName: String);
    function ConfigExcelWebBrowser : Boolean;

    function TesteazaSelectie: Boolean;
    procedure SetClasaEconomica(const Value: Integer);
    procedure SetTrim1(const Value: Integer);
    procedure SetTrim2(const Value: Integer);
    procedure SetTrim3(const Value: Integer);
    procedure SetTrim4(const Value: Integer);
    procedure SetRestante(const Value: Integer);
    procedure SetEstimat1(const Value: Integer);
    procedure SetEstimat2(const Value: Integer);
    procedure SetEstimat3(const Value: Integer);
  protected
    procedure RefreshDataSet;
  public
    { Public declarations }
    property  IsWebViewer    : Boolean read FIsWebViewer;
    property  ClasaEconomica : Integer read FClasaEconomica write SetClasaEconomica;
    property  Restante : Integer read FRestante write SetRestante;
    property  Trim1 : Integer read FTrim1 write SetTrim1;
    property  Trim2 : Integer read FTrim2 write SetTrim2;
    property  Trim3 : Integer read FTrim3 write SetTrim3;
    property  Trim4 : Integer read FTrim4 write SetTrim4;
    property  Estimat1 : Integer read FEstimat1 write SetEstimat1;
    property  Estimat2 : Integer read FEstimat2 write SetEstimat2;
    property  Estimat3 : Integer read FEstimat3 write SetEstimat3;
  end;

implementation

{$R *.DFM}

uses
  PreluareExcelUnit,
  Db,
  ZeosDBUtile,
  DateUtils,
  DateUnit,
  StrUtils,
  Variants,
  CommonDBVar,
  PatchExcel;

procedure TfrmBxImportEXCEL.edFileNameChange(Sender: TObject);
begin
  if FileExists(edFileName.Text) then
     InitEXCELFile(edFileName.Text);
end;

procedure TfrmBxImportEXCEL.InitEXCELFile(FileName: String);
begin
  { Incercam sa incarcam fisierul excel }
  if not ckSameSettings.Checked then begin
    FClasaEconomica := -1;
    FTrim1 := -1; FTrim2 := -1; FTrim3 := -1; FTrim4 := -1;
    FRestante := -1;
    FEstimat1 := -1;
    FEstimat2 := -1;
    FEstimat3 := -1;
  end;
  pbProgress.Position := 0;
  if ExcelViewer is TWebBrowser then begin
    ExcelDeinitBrowser;
    ExcelInitBrowser;
  end;
  ExcelLoadFromFile(edFileName.Text);
end;

procedure TfrmBxImportEXCEL.ExcelActiveSheetChanging(Sender: TObject);
begin
  FdxSheet := TdxCustomSpreadSheet(Sender).ActiveSheet as TdxSpreadSheetTableView;
  ExcelOnChangeSheet;
end;

procedure TfrmBxImportEXCEL.SetClasaEconomica(const Value: Integer);
begin
  if not Assigned(FdxSheet) and not Assigned(FExcelOLESheet) then Exit;
  if (Value > -1) and ((Value = FRestante) or (Value = FTrim2) or (Value = FTrim3) or (Value = FTrim4) or (Value=FTrim1)) then
     raise EContaHandledError.Create('Ati ales deja coloana pentru un trimestru !');
  if FClasaEconomica > -1 then ExcelFormatRow(FClasaEconomica, 1, False);
     { Stergem Formatarea anterioara }
  FClasaEconomica := Value;
  { Marcam cu albastru deschis coloana }
  if FClasaEconomica > -1 then ExcelFormatRow(FClasaEconomica, 27, False);
  BtnGenerare.Enabled := TesteazaSelectie;
end;

procedure TfrmBxImportEXCEL.FormCreate(Sender: TObject);
begin
  FClasaEconomica := -1;
  FRestante := -1;
FTrim1 := -1;
FTrim2 := -1;
FTrim3 := -1;
FTrim4 := -1;
FEstimat1 := -1;
FEstimat2 := -1;
FEstimat3 := -1;
BtnGenerare.Enabled := False;
  TreeBugete.FullExpand;
  if TreeBugete.TopVisibleNode <> nil then begin
    TreeBugete.TopVisibleNode.Focused := True;
    TreeBugete.TopVisibleNode.MakeVisible;
  end;
  ClasaFunctionala.OnlySelectChild := True;
  ClasaFunctionala.ValidateEditText := True;
  ExcelInitControl;
end;

procedure TfrmBxImportEXCEL.SetTrim4(const Value: Integer);
begin
  if not Assigned(FdxSheet) and not Assigned(FExcelOLESheet) then Exit;
  if (Value > -1) and ((Value = FRestante) or (Value = FTrim2) or (Value = FTrim3) or (Value = FTrim1) or (Value=FClasaEconomica)) then
     raise EContaHandledError.Create('Ati ales deja coloana pentru un trimestru !');
  if FTrim4 > -1 then ExcelFormatRow(FTrim4, 1);
     { Stergem Formatarea anterioara }
  FTrim4 := Value;
  { Marcam cu albastru deschis coloana }
  if FTrim4 > -1 then ExcelFormatRow(FTrim4, 25);
  BtnGenerare.Enabled := TesteazaSelectie;
end;

procedure TfrmBxImportEXCEL.SetTrim2(const Value: Integer);
begin
  if not Assigned(FdxSheet) and not Assigned(FExcelOLESheet) then Exit;
  if (Value > -1) and ((Value = FRestante) or (Value = FTrim1) or (Value = FTrim3) or (Value = FTrim4) or (Value=FClasaEconomica)) then
     raise EContaHandledError.Create('Ati ales deja coloana pentru un trimestru !');
  if FTrim2 > -1 then ExcelFormatRow(FTrim2, 1);
     { Stergem Formatarea anterioara }
  FTrim2 := Value;
  { Marcam cu albastru deschis coloana }
  if FTrim2 > -1 then ExcelFormatRow(FTrim2, 25);
  BtnGenerare.Enabled := TesteazaSelectie;
end;

procedure TfrmBxImportEXCEL.SetTrim3(const Value: Integer);
begin
  if not Assigned(FdxSheet) and not Assigned(FExcelOLESheet) then Exit;
  if (Value > -1) and ((Value = FRestante) or (Value = FTrim2) or (Value = FTrim1) or (Value = FTrim4) or (Value=FClasaEconomica)) then
     raise EContaHandledError.Create('Ati ales deja coloana pentru un trimestru !');
  if FTrim3 > -1 then ExcelFormatRow(FTrim3, 1);
     { Stergem Formatarea anterioara }
  FTrim3 := Value;
  { Marcam cu albastru deschis coloana }
  if FTrim3 > -1 then ExcelFormatRow(FTrim3, 25);
  BtnGenerare.Enabled := TesteazaSelectie;
end;

procedure TfrmBxImportEXCEL.SetTrim1(const Value: Integer);
begin
  if not Assigned(FdxSheet) and not Assigned(FExcelOLESheet) then Exit;
  if (Value > -1) and ((Value = FRestante) or(Value = FTrim2) or (Value = FTrim3) or (Value = FTrim4) or (Value=FClasaEconomica)) then
     raise EContaHandledError.Create('Ati ales deja coloana pentru import !');
  if FTrim1 > -1 then ExcelFormatRow(FTrim1, 1);
     { Stergem Formatarea anterioara }
  FTrim1 := Value;
  { Marcam cu albastru deschis coloana }
  if FTrim1 > -1 then ExcelFormatRow(FTrim1, 25);
  BtnGenerare.Enabled := TesteazaSelectie;
end;

procedure TfrmBxImportEXCEL.BtnAutoClick(Sender: TObject);
var
    lFound: Boolean;

    function TestString(Str: String; Valori: array of String): Boolean;
    var I: Integer;
     begin
       Result := False;
       for I := Low(Valori) to High(Valori) do
         if pos(UpperCase(Valori[I]), UpperCase(Str)) > 0 then begin
            Result := True;
            Break;
         end;
     end;

    function IsTrimestru(Str: String): Boolean;
     begin
       Result := TestString(Str, ['Trim', 'Plan']);
     end;

(*

     function ExcelcxTryFind : Boolean;
     var
       lExcelcx : TcxSpreadSheetBook;
       I, J : Integer;
       lText: String;
       lDeleteLine : TList;
       lLine,
       lTextCnt : Integer;
     begin
        if not Assigned(FdxSheet) then Exit;
        lExcelcx := TcxSpreadSheetBook(ExcelViewer);
        Result := False;
        lDeleteLine := TList.Create;
        try
          lExcelcx.History.BeginUpdate;
          for J := 0 to FdxSheet.RowCount do begin
            lTextCnt := 0;
            for I := 0 to FdxSheet.ColumnCount do begin
              lText := Trim(FdxSheet.GetCellObject(I, J).Text);
              if Trim(lText) > '' then Inc(lTextCnt);
            end;
            if lTextCnt > 1 then
              for I := 1 to FdxSheet.ColumnCount do begin
                lText := Trim(FdxSheet.GetCellObject(I, J).Text);
                lText := StringReplace(lText, IntToStr(YearOf(Date)), '', [rfReplaceAll]);
                lText := StringReplace(lText, LeftStr(IntToStr(YearOf(Date)),3), '', [rfReplaceAll]);
                if lText = '' then Continue;
                if IsTrimestru(lText) then
                   if (FTrim4 = -1) and (TestString(lText, ['IV', '4'])) then Trim4 := I
                   else if (FTrim3 = -1) and (TestString(lText, ['III', '3'])) then Trim3 := I
                   else if (FTrim2 = -1) and (TestString(lText, ['II', '2'])) then Trim2 := I
                   else if (FTrim1 = -1) and (TestString(lText, [' I', '1', '.I'])) then Trim1 := I
                   else if (FRestante = -1) and (TestString(lText, ['resta', 'rest', 'restante'])) then Restante := I;
                if (FClasaEconomica = -1) and (TestString(lText, ['Cod', ' Art', ' Titl', 'Clas', 'Eco'])) then ClasaEconomica := I;
                Result := TesteazaSelectie;
                if Result then Break
              end
            else lDeleteLine.Add(Pointer(J));
            //if lFound then Break;
          end;

          { Stergem liniile de sters }
          lExcelcx.BeginUpdate;
          try
            for I := lDeleteLine.Count-1 downto 0 do begin
              lLine := Integer(lDeleteLine[I]);
              FdxSheet.ClearCells(Rect(0, lLine, FdxSheet.ColumnCount, lLine), True);
            end;
          finally
            lExcelcx.EndUpdate;
          end;

        finally
          lDeleteLine.Free;
          lExcelcx.History.EndUpdate;
        end;

     end;

  function ExcelBrowserTryFind : Boolean;
  begin
    //todo
    Result := False;
  end;
*)
  function ExcelTryFind : Boolean;
  var
    I, J : Integer;
    lText: String;
    lDeleteLine : TList;
    lLine,
    lTextCnt : Integer;
  begin
    Result := False;
    lDeleteLine := TList.Create;
    try
      if ExcelViewer is TdxSpreadSheet then
        TdxSpreadSheet(ExcelViewer).History.BeginAction(TdxSpreadSheetHistoryClearCellsAction);
      for I := 0 to ExcelGetRowCount - 1 do begin
        lTextCnt := 0;
        for J := 0 to ExcelGetColumnCount - 1 do begin
          lText := ExcelGetText(I, J);
          if Trim(lText) > '' then Inc(lTextCnt);
        end;
        if lTextCnt > 1 then
          for J := 1 to ExcelGetColumnCount-1 do begin
            lText := ExcelGetText(I, J);
            lText := StringReplace(lText, IntToStr(YearOf(Date)), '', [rfReplaceAll]);
            lText := StringReplace(lText, LeftStr(IntToStr(YearOf(Date)),3), '', [rfReplaceAll]);
            if lText = '' then Continue;
            if IsTrimestru(lText) then
               if (FTrim4 = -1) and (TestString(lText, ['IV', '4'])) then Trim4 := I
               else if (FTrim3 = -1) and (TestString(lText, ['III', '3'])) then Trim3 := I
               else if (FTrim2 = -1) and (TestString(lText, ['II', '2'])) then Trim2 := I
               else if (FTrim1 = -1) and (TestString(lText, [' I', '1', '.I'])) then Trim1 := I
               else if (FRestante = -1) and (TestString(lText, ['resta', 'rest', 'restante'])) then Restante := I;
            if (FClasaEconomica = -1) and (TestString(lText, ['Cod', ' Art', ' Titl', 'Clas', 'Eco'])) then ClasaEconomica := I;
            Result := TesteazaSelectie;
            if Result then Break
          end
        else lDeleteLine.Add(Pointer(I));
        //if lFound then Break;
      end;

      { Stergem liniile de sters }
       if ExcelViewer is TdxSpreadSheet then
          TdxSpreadSheet(ExcelViewer).BeginUpdate;
      try
        for I := lDeleteLine.Count-1 downto 0 do begin
          lLine := Integer(lDeleteLine[I]);
          ExcelClearCells(lLine, 0, lLine, ExcelGetColumnCount-1);
        end;
      finally
        //lExcelcx.EndUpdate;
       if ExcelViewer is TdxSpreadSheet then
          TdxSpreadSheet(ExcelViewer).EndUpdate;
      end;

    finally
      lDeleteLine.Free;
      if ExcelViewer is TdxSpreadSheet then
        TdxSpreadSheet(ExcelViewer).History.EndAction(False);
    end;
  end;



begin
  { Prima data incercam sa descoperim ce coloane ne interseaza }
{
  if ExcelViewer is TWebBrowser then
     lFound := ExcelBrowserTryFind
  else
     lFound := ExcelcxTryFind;
}
  lFound := ExcelTryFind;
  edClasaEconomica.EditValue := IntToStr(FClasaEconomica);
  edRestante.EditValue   := IntToStr(FRestante);
  edTrim1.EditValue   := IntToStr(FTrim1);
  edTrim2.EditValue   := IntToStr(FTrim2);
  edTrim3.EditValue   := IntToStr(FTrim3);
  edTrim4.EditValue   := IntToStr(FTrim4);
  edEstimat1.EditValue   := IntToStr(FEstimat1);
  edEstimat2.EditValue   := IntToStr(FEstimat2);
  edEstimat3.EditValue   := IntToStr(FEstimat3);
  
  if lFound then begin
     BtnGenerare.Enabled   := True;
     ShowMessage('Verificati daca au fost depistate corect coloanele in urma procesului !')
  end
  else raise EContaHandledError.Create('Nu se pot gasi coloanele prin procedeul automat !'#13#10'Va rugam sa le specificati manual !');
end;

function TfrmBxImportEXCEL.TesteazaSelectie: Boolean;
begin
  Result := (FRestante > -1)  and (FTrim1 > -1) and (FTrim2 > -1) and (FTrim3 > -1) and (FTrim4 > -1) and (FClasaEconomica > -1);
end;

procedure TfrmBxImportEXCEL.BtnGenerareClick(Sender: TObject);
var
   lId, I: Integer;
   lCod: String;
   lRest, lTrim1, lTrim2, lTrim3, lTrim4, lEstimat1, lEstimat2, lEstimat3: String;
   lEconomic : String;
   DataSet: TDataSet;
   lPreluare : TfrmPreluareExcel;
   lCodUnitate,  lCodProiect: Integer;
   lEstimat : Boolean;

     function IsEmptyCell(Value: String): Boolean;
      begin
        Result := (Trim(Value) = '') or (Trim(Value) = '0');
      end;


     function ExistsCell(AColIndex: Integer; AValue : String): Boolean;
     var J: Integer;
      begin
        Result := False;
        for J := 0 to ExcelGetRowCount - 1  do
          if AnsiCompareText(Trim(ExcelGetText(J, AColIndex)), AValue) = 0 then begin
             Result := True;
             Break;
          end;
      end;

begin
  if not Assigned(ClasaFunctionala) then
  begin
    ShowMessage('ClasaFunctionala este nil');
    Exit;
  end;

  if ClasaFunctionala.KeyValue = Null then
  begin
    MessageDlg('Selectati clasificatia functionala corespunzatoare !', mtError, [mbOK], 0);
    ClasaFunctionala.EditInput.SetFocus;
    Exit;
  end;

  if not Assigned(FrmData) then
  begin
    ShowMessage('FrmData nu este asignat!');
    Exit;
  end;

  if not Assigned(FrmData.QryBGPlanEconomic) then
  begin
    ShowMessage('QryBGPlanEconomic este nil!');
    Exit;
  end;

  if not FrmData.QryBGPlanEconomic.Active then
  begin
    FrmData.QryBGPlanEconomic.Open;
    if not FrmData.QryBGPlanEconomic.Active then
    begin
      ShowMessage('QryBGPlanEconomic nu s-a putut deschide!');
      Exit;
    end;
  end;

  if not Assigned(ExcelViewer) then
  begin
    ShowMessage('ExcelViewer nu este asignat!');
    Exit;
  end;



  if ClasaFunctionala.KeyValue = Null then begin
    MessageDlg('Selectati clasificatia functionala corespunzatoare !', mtError, [mbOK], 0);
    ClasaFunctionala.EditInput.SetFocus;
    Abort;
  end;
//  Excel.History.MaxActions := 0;
  { Verificam pe baza nomenclatorului pe care il avem }
  { Facem verificarea daca are sau nu formula de calcul asociata }
  lPreluare := TfrmPreluareExcel.Create(Application);
  try
    if ExcelViewer is TdxSpreadSheet then
      TdxSpreadSheet(ExcelViewer).History.BeginAction(TdxSpreadSheetHistoryClearCellsAction);
    DataSet := lPreluare.Preluate;
    DataSet.Active := True;
    if not Assigned(DataSet) then
begin
  ShowMessage('DataSet este nil!');
  Exit;
end;

    for I := 0 to ExcelGetRowCount - 1 do begin
      lCod :=  ExcelGetText(I, FClasaEconomica);
      if lCod = '' then Continue;
      lRest  := ExcelGetText(I, FRestante);
      lTrim1 := ExcelGetText(I, FTrim1);
      lTrim2 := ExcelGetText(I, FTrim2);
      lTrim3 := ExcelGetText(I, FTrim3);
      lTrim4 := ExcelGetText(I, FTrim4);
      lEstimat1 := '0';
      lEstimat2 := '0';
      lEstimat3 := '0';
      if FEstimat1 <> 0 then
        lEstimat1 := ExcelGetText(I, FEstimat1);
      if FEstimat2 <> 0 then
        lEstimat2 := ExcelGetText(I, FEstimat2);
      if FEstimat3 <> 0 then
        lEstimat3 := ExcelGetText(I, FEstimat3);



      { Daca nu avem prevazut nimic nu mai importam }
      if IsNumeric(lTrim1) or IsNumeric(lTrim2) or IsNumeric(lTrim3) or IsNumeric(lTrim4) or IsNumeric(lRest) then begin
        if not IsNumeric(lRest) then lRest := '0';
        if not IsNumeric(lTrim1) then lTrim1 := '0';
        if not IsNumeric(lTrim2) then lTrim2 := '0';
        if not IsNumeric(lTrim3) then lTrim3 := '0';
        if not IsNumeric(lTrim4) then lTrim4 := '0';
        if not IsNumeric(lEstimat1) then lEstimat1 := '0';
        if not IsNumeric(lEstimat2) then lEstimat2 := '0';
        if not IsNumeric(lEstimat3) then lEstimat3 := '0';        
      end;
      if (not IsNumeric(lRest)) or (not IsNumeric(lTrim1)) or (not IsNumeric(lTrim2)) or (not IsNumeric(lTrim3)) or (not IsNumeric(lTrim4)) then Continue;
      if (lRest = '0') and (lTrim1 = '0') and (lTrim2 = '0') and (lTrim3 = '0') and (lTrim4 = '0') and (lEstimat1='0') and (lEstimat2='0') and (lEstimat3='0') then Continue;
      if (IsEmptyCell(lRest)) and (IsEmptyCell(lTrim1)) and (IsEmptyCell(lTrim2)) and (IsEmptyCell(lTrim3)) and (IsEmptyCell(lTrim4)) then Continue;
      DataSet.Append;
      DataSet.FieldByName('PLANIFICAT_REST').AsCurrency := StrToCurr(lRest);
      DataSet.FieldByName('PLANIFICAT1').AsCurrency := StrToCurr(lTrim1);
      DataSet.FieldByName('PLANIFICAT2').AsCurrency := StrToCurr(lTrim2);
      DataSet.FieldByName('PLANIFICAT3').AsCurrency := StrToCurr(lTrim3);
      DataSet.FieldByName('PLANIFICAT4').AsCurrency := StrToCurr(lTrim4);
      DataSet.FieldByName('PLUS1AN').AsCurrency := StrToCurr(lEstimat1);
      DataSet.FieldByName('PLUS2AN').AsCurrency := StrToCurr(lEstimat2);
      DataSet.FieldByName('PLUS3AN').AsCurrency := StrToCurr(lEstimat3);
      DataSet.FieldByName('COD_FUNCTIONAL').AsString := ClasaFunctionala.EditInput.Text;
      { Testam daca codul economic este ok }
      { Aici incercam sa facem niste ajustari }
      lEconomic := lCod;
      if pos(',', lEconomic) > 0 then lEconomic := StringReplace(lEconomic, ',','.', [rfReplaceAll]);
      DataSet.FieldByName('COD_ECONOMIC_ECRAN').AsString := lEconomic;
      if not FrmData.QryBGPlanEconomic.Locate('COD_ECONOMIC', lEconomic, []) then begin
         { Nu-l regasim direct incercam sa adaugam punct daca nu are }
         if (Length(lEconomic) > 3) and (pos('.', lEconomic) = 0) and
            (FrmData.QryBGPlanEconomic.Locate('COD_ECONOMIC', Copy(lEconomic, 1, Length(lEconomic)-2)+'.'+Copy(lEconomic, Length(lEconomic)-2, 2), [])) then begin
            DataSet.FieldByName('EROARE').AsInteger := 1;
            DataSet.FieldByName('COD_ECONOMIC').AsString := FrmData.QryBGPlanEconomic.FieldByName('COD_ECONOMIC').AsString;
         end
         else begin
            { Nu-l gasim nici cu punctul adaugat incerca sa gasim ce putem din el }
            while (lEconomic > '') do begin
              if FrmData.QryBGPlanEconomic.Locate('COD_ECONOMIC', lEconomic, []) then begin
                 DataSet.FieldByName('EROARE').AsInteger := 2;
                 DataSet.FieldByName('COD_ECONOMIC').AsString := lEconomic;
                 Break;
              end
              else lEconomic := Copy(lEconomic, 1, Length(lEconomic)-1);
            end;
            if lEconomic = '' then begin
               { Nu a fost gasit deloc }
               DataSet.FieldByName('EROARE').AsInteger := 3;
               DataSet.FieldByName('COD_ECONOMIC').AsString := lCod;
            end;
         end;
      end
      else begin
         lEstimat := (FrmData.QryBGPlanEconomic.FieldByName('INTRODUCERE_ESTIMARE').AsInteger = 1)
                and (
                  (DataSet.FieldByName('PLUS1AN').AsCurrency <> 0) or
                  (DataSet.FieldByName('PLUS2AN').AsCurrency <> 0) or
                  (DataSet.FieldByName('PLUS3AN').AsCurrency <> 0)
                );
         lId := FrmData.QryBGPlanEconomic.FieldByName('ID_BG_PLAN_ECONOMIC').AsInteger;
         { Daca l-am gasit incercam sa verificam daca este nod final sau daca este informatie agregata }
         if FrmData.QryBGPlanEconomic.Locate('ID_PARINTE', lId, []) then begin
            { Inseamna ca avem cel putin un copil care depinde de clasa curenta ...
              Aici avem 2 aspecte : - se ignora, deoarece avem si copilul la randul lui ...
                                    - se adauga deoarece nu avem copii si este un cod trebut partial, caz destul de des intalnit }

           {vedem daca are date despre estimare chiar daca este parinte}
           if lEstimat then begin
                DataSet.FieldByName('EROARE').AsInteger := 8;
                DataSet.FieldByName('COD_ECONOMIC').AsString := lEconomic;
           end
           else begin
             DataSet.FieldByName('PLUS1AN').AsCurrency := 0;
             DataSet.FieldByName('PLUS2AN').AsCurrency := 0;
             DataSet.FieldByName('PLUS3AN').AsCurrency := 0;
             if ExistsCell(FClasaEconomica, Trim(FrmData.QryBGPlanEconomic.FieldByName('COD_ECONOMIC').AsString)) then begin
                   { Primul caz }
                  DataSet.FieldByName('EROARE').AsInteger := 4;
                  DataSet.FieldByName('COD_ECONOMIC').AsString := lCod;
             end
             else begin
                  { Al doilea caz ... adaugam pe primul copil iesit in cale }
                  DataSet.FieldByName('EROARE').AsInteger := 5;
                  DataSet.FieldByName('COD_ECONOMIC').AsString := FrmData.QryBGPlanEconomic.FieldByName('COD_ECONOMIC').AsString;
             end;
           end;
         end
         else begin
           { Cazul fericit ... e totul in regula }
           DataSet.FieldByName('EROARE').AsInteger := 0;
           DataSet.FieldByName('COD_ECONOMIC').AsString := lCod;
         end;
      end;
      DataSet.Post;
    end;
    if Trim(ClasaFunctionala.EditInput.Text) <> '' then begin
      lCodUnitate := -1;
      lCodProiect := -1;
      if frmData.qryBGPlanFunctionalComplet.Locate('ID_BG_PLAN_FUNCTIONAL', ClasaFunctionala.KeyValue, []) then begin
        if frmData.qryBGPlanFunctionalComplet.FieldByName('ID_OI_UNITATI').AsString <> '' then
           lCodUnitate := frmData.qryBGPlanFunctionalComplet.FieldByName('ID_OI_UNITATI').AsInteger;
        if frmData.qryBGPlanFunctionalComplet.FieldByName('ID_OI_PROIECTE').AsString <> '' then
           lCodProiect := frmData.qryBGPlanFunctionalComplet.FieldByName('ID_OI_PROIECTE').AsInteger;
      end;
      lPreluare.SetContext(ClasaFunctionala.EditInput.Text, lCodProiect, lCodUnitate);
    end;
    if not DataSet.IsEmpty then lPreluare.ShowModal
    else ShowMessage('Nu exista inregistrari de preluat !');
  finally
    if ExcelViewer is TdxSpreadSheet then
      TdxSpreadSheet(ExcelViewer).History.EndAction(False);
    lPreluare.Free;
  end;
end;

procedure TfrmBxImportEXCEL.btnOpenFileClick(Sender: TObject);
begin
  fileOpen.FileName := edFileName.Text;
  if fileOpen.Execute then
    edFileName.Text := fileOpen.FileName;
end;

procedure TfrmBxImportEXCEL.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmBxImportEXCEL.ClasaFunctionalaPopupInitPopup(Sender: TObject);
begin
  with TcxPopupEdit(Sender).Properties do begin
    if PopupWidth < TcxPopupEdit(Sender).Width then PopupWidth := TcxPopupEdit(Sender).Width;
  end;
end;

procedure TfrmBxImportEXCEL.TreeBugeteDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Values[TreeBugeteCOD_BUGET.ItemIndex] + ': '+ANode.Values[TreeBugeteDENUMIRE.ItemIndex];
end;

procedure TfrmBxImportEXCEL.TreeBugeteDblClick(Sender: TObject);
begin
   with TcxDBTreeList(Sender) do
    if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk;
end;

procedure TfrmBxImportEXCEL.TreeBugeteKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then begin
     if Assigned(TcxDBTreeList(Sender).OnDblClick) then TcxDBTreeList(Sender).OnDblClick(Sender);
  end
  else if Key = VK_ESCAPE then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrCancel;
end;

procedure TfrmBxImportEXCEL.edTrim1PropertiesEditValueChanged(
  Sender: TObject);
begin
  if edTrim1.EditValue = Null then Exit;
  Trim1 := StrToInt(edTrim1.EditValue);
end;

procedure TfrmBxImportEXCEL.edTrim2PropertiesEditValueChanged(
  Sender: TObject);
begin
  if edTrim2.EditValue = Null then Exit;
  Trim2 := StrToInt(edTrim2.EditValue);
end;

procedure TfrmBxImportEXCEL.edTrim3PropertiesEditValueChanged(
  Sender: TObject);
begin
  if edTrim3.EditValue = Null then Exit;
  Trim3 := StrToInt(edTrim3.EditValue);
end;

procedure TfrmBxImportEXCEL.edTrim4PropertiesEditValueChanged(
  Sender: TObject);
begin
   if edTrim4.EditValue = Null then Exit;
   Trim4 := StrToInt(edTrim4.EditValue);
end;

procedure TfrmBxImportEXCEL.edClasaEconomicaPropertiesEditValueChanged(
  Sender: TObject);
begin
   if edClasaEconomica.EditValue = Null then Exit;
   ClasaEconomica := StrToInt(edClasaEconomica.EditValue);
end;

procedure TfrmBxImportEXCEL.ExcelProgress(Sender: TObject; Percent: Integer);
begin
  if pbProgress.Properties.Max <> 100 then
    pbProgress.Properties.Max := 100; 
  if pbProgress.Position <> Percent then begin
     pbProgress.Position := Percent;
     Application.ProcessMessages;
  end;
end;


procedure TfrmBxImportEXCEL.ClasaFunctionalaButtonClick(Sender: TObject);
begin
  RefreshDataSet;
end;

procedure TfrmBxImportEXCEL.RefreshDataSet;
begin
  DBRefresh(frmData.qryBGPlanFunctionalComplet);
end;

procedure TfrmBxImportEXCEL.TreeBugeteCustomDrawDataCell(
  Sender: TcxCustomTreeList; ACanvas: TcxCanvas;
  AViewInfo: TcxTreeListEditCellViewInfo; var ADone: Boolean);
begin
  ACanvas.Font.Style := ACanvas.Font.Style - [fsBold];
  if AViewInfo.Node.Texts[TreeBugeteID_OI_UNITATI.ItemIndex] <> '' then
    ACanvas.Font.Style := ACanvas.Font.Style + [fsBold];
  if AViewInfo.Node.Texts[TreeBugeteID_OI_PROIECTE.ItemIndex] <> '' then
    ACanvas.Font.Style := ACanvas.Font.Style + [fsBold];
end;

procedure TfrmBxImportEXCEL.edRestantePropertiesEditValueChanged(
  Sender: TObject);
begin
  if edRestante.EditValue = Null then Exit;
  Restante := StrToInt(edRestante.EditValue);
end;

procedure TfrmBxImportEXCEL.SetRestante(const Value: Integer);
begin
  if not Assigned(FdxSheet) and not Assigned(FExcelOLESheet) then
  begin
    ShowMessage('⚠️ FdxSheet și FExcelOLESheet sunt nil');
    Exit;
  end;

  if (Value > -1) and
     ((Value = FTrim1) or (Value = FTrim2) or (Value = FTrim3) or (Value = FTrim4) or (Value = FClasaEconomica)) then
    raise EContaHandledError.Create('Ati ales deja coloana pentru import !');

  if FRestante > -1 then
  begin
    try
      ExcelFormatRow(FRestante, 1);
    except
      on E: Exception do
      begin
        ShowMessage('❌ Eroare in ExcelFormatRow (sterge formatul): ' + E.Message);
        Exit;
      end;
    end;
  end;

  FRestante := Value;

  if FRestante > -1 then
  begin
    try
      ExcelFormatRow(FRestante, 25);
    except
      on E: Exception do
      begin
        ShowMessage('❌ Eroare in ExcelFormatRow (formateaza noua coloana): ' + E.Message);
        Exit;
      end;
    end;
  end;

  try
    BtnGenerare.Enabled := TesteazaSelectie;
  except
    on E: Exception do
      ShowMessage('❌ Eroare in TesteazaSelectie sau BtnGenerare: ' + E.Message);
  end;
end;



procedure TfrmBxImportEXCEL.SetEstimat1(const Value: Integer);
begin
  if not Assigned(FdxSheet) and not Assigned(FExcelOLESheet) then Exit;
  if (Value > -1) and ((Value = FTrim1) or (Value = FTrim2) or (Value = FTrim3) or (Value = FTrim4) or (Value=FClasaEconomica)
    or (Value=FEstimat1) or (Value = FEstimat2) or (Value=FEstimat3)) then
     raise EContaHandledError.Create('Ati ales deja coloana pentru import !');
  if FEstimat1 > -1 then ExcelFormatRow(FEstimat1, 1);
     { Stergem Formatarea anterioara }
  FEstimat1 := Value;
  { Marcam cu albastru deschis coloana }
  if FEstimat1 > -1 then ExcelFormatRow(FEstimat1, 25);
  BtnGenerare.Enabled := TesteazaSelectie;
end;

procedure TfrmBxImportEXCEL.SetEstimat2(const Value: Integer);
begin
  if not Assigned(FdxSheet) and not Assigned(FExcelOLESheet) then Exit;
  if (Value > -1) and ((Value = FTrim1) or (Value = FTrim2) or (Value = FTrim3) or (Value = FTrim4) or (Value=FClasaEconomica)
    or (Value=FEstimat1) or (Value = FEstimat2) or (Value=FEstimat3)) then
     raise EContaHandledError.Create('Ati ales deja coloana pentru import !');
  if FEstimat2 > -1 then ExcelFormatRow(FEstimat2, 1);
     { Stergem Formatarea anterioara }
  FEstimat2 := Value;
  { Marcam cu albastru deschis coloana }
  if FEstimat2 > -1 then ExcelFormatRow(FEstimat2, 25);
  BtnGenerare.Enabled := TesteazaSelectie;
end;

procedure TfrmBxImportEXCEL.SetEstimat3(const Value: Integer);
begin
  if not Assigned(FdxSheet) and not Assigned(FExcelOLESheet) then Exit;
  if (Value > -1) and ((Value = FTrim1) or (Value = FTrim2) or (Value = FTrim3) or (Value = FTrim4) or (Value=FClasaEconomica)
    or (Value=FEstimat1) or (Value = FEstimat2) or (Value=FEstimat3)) then
     raise EContaHandledError.Create('Ati ales deja coloana pentru import !');
  if FEstimat3 > -1 then ExcelFormatRow(FEstimat3, 1);
     { Stergem Formatarea anterioara }
  FEstimat3 := Value;
  { Marcam cu albastru deschis coloana }
  if FEstimat3 > -1 then ExcelFormatRow(FEstimat3, 25);
  BtnGenerare.Enabled := TesteazaSelectie;
end;

procedure TfrmBxImportEXCEL.edEstimat1PropertiesEditValueChanged(
  Sender: TObject);
begin
  if edEstimat1.EditValue = Null then Exit;
  Estimat1 := StrToInt(edEstimat1.EditValue);
end;

procedure TfrmBxImportEXCEL.edEstimat2PropertiesEditValueChanged(
  Sender: TObject);
begin
  if edEstimat2.EditValue = Null then Exit;
  Estimat2 := StrToInt(edEstimat2.EditValue);
end;

procedure TfrmBxImportEXCEL.edEstimat3PropertiesEditValueChanged(
  Sender: TObject);
begin
  if edEstimat3.EditValue = Null then Exit;
  Estimat3 := StrToInt(edEstimat3.EditValue);
end;

function TfrmBxImportEXCEL.ConfigExcelWebBrowser : Boolean;
begin
  Result := IsExcelInstalled;
  if Result then begin
    Result := WebPatchExcel;
  end;
end;

procedure TfrmBxImportEXCEL.ExcelLoadFromFile(FileName: String);
var
  lWebBrowser : TWebBrowser;
  lExcelBook  : TdxSpreadSheet;
begin
  if ExcelViewer is TWebBrowser then begin
    lWebBrowser := ExcelViewer as TWebBrowser;
    lWebBrowser.Navigate(FileName);
    while not FExcelIsLoaded do
      Application.ProcessMessages;
    if (ExcelApp = nil) then
      raise EContaHandledError.Create('Eroare la incarcarea datelor din fisier !')
    else begin
      ExcelApp.OnSheetActivate := ExcelAppSheetActivate;
      ExcelAppSheetActivate(nil, ExcelApp.ActiveWorkbook.ActiveSheet);
      BtnAuto.Enabled := True;
    end;
  end
  else begin
    lExcelBook := ExcelViewer as TdxSpreadSheet;
    lExcelBook.ClearAll;
    FExcelIsLoaded := False;
    try
       lExcelBook.LoadFromFile(FileName);
       ExcelActiveSheetChanging(lExcelBook);
       BtnAuto.Enabled := True;
       lExcelBook.History.Clear;
       FExcelIsLoaded := True;
    except
      on E: Exception do begin
         lExcelBook.ClearAll;
         raise EContaHandledError.Create('Eroare la incarcarea datelor din fisier !'#13#10'Eroare : '+E.Message);
      end;
  end;
  end;
end;

procedure TfrmBxImportEXCEL.ExcelInitControl;
begin
  ExcelApp := nil;
  FIsWebViewer := (cst_ExcelView = evWebBrowser) and ConfigExcelWebBrowser;
  if FIsWebViewer then begin
    ExcelInitBrowser;
  end
  else begin
    ExcelViewer := TdxSpreadSheet.Create(Self);
    ExcelInitcxSpreadSeet;
  end;
end;

procedure TfrmBxImportEXCEL.WebBrowserNavigateComplete2(ASender: TObject; const pDisp: IDispatch; const URL: OleVariant);
var
  lWB : _Workbook;
begin
  try
    if Supports(TWebBrowser(ExcelViewer).Document, IID__Workbook, lWB) then begin
       FExcelIsLoaded := False;
       ExcelApp := TExcelApplication.Create(nil);
      // ExcelApp.DisplayAlerts[0] := False;
       try
         ExcelApp.ConnectTo(lWB.Application);
       except
         FreeAndNil(ExcelApp);
       end;
       FExcelIsLoaded := True; 
    end;
  finally
  end;
end;

procedure TfrmBxImportEXCEL.WebBrowserProgressChange(Sender: TObject;
  Progress, ProgressMax: Integer);
begin
  if (Progress > 0) and (pbProgress.Properties.Max <> ProgressMax) then
   pbProgress.Properties.Max := ProgressMax;
  if pbProgress.Position <> Progress then begin
     pbProgress.Position := Progress;
     Application.ProcessMessages;
  end;
end;

procedure TfrmBxImportEXCEL.ExcelInitBrowser;
begin
  ExcelViewer := TWebBrowser.Create(Self);
  TOleControl(ExcelViewer).Parent := Self;
  with TWebBrowser(ExcelViewer) do begin
    Align               := alClient;
    OnNavigateComplete2 := WebBrowserNavigateComplete2;
    OnProgressChange    := WebBrowserProgressChange;
  end;
end;

procedure TfrmBxImportEXCEL.ExcelInitcxSpreadSeet;
begin
   with TdxSpreadSheet(ExcelViewer) do
   begin
    Parent        := Self;
    Left          := 0;
    Top           := 129;
    Width         := 800;
    Height        := 497;
    Align         := alClient;
    OptionsView.CellAutoHeight := False;
    OnActiveSheetChanged := ExcelActiveSheetChanging;
    OnProgress    := ExcelProgress;
  end;
end;

procedure TfrmBxImportEXCEL.ExcelFormatRow(ACol, AColor: Integer;
  IsCurrency: Boolean);
var
  I         : Integer;
  lExcelcx  : TdxSpreadSheet;
  lRange    : ExcelRange;
begin
  if ExcelViewer is TWebBrowser then begin
    if (ExcelApp <> nil) and (FExcelOLESheet <> nil) then begin
      lRange := FExcelOLESheet.Range[FExcelOLESheet.Cells.Item[1, ACol+1], FExcelOLESheet.Cells.Item[1, ACol+1]];
      lRange.EntireColumn.Interior.ColorIndex := AColor;
      lRange.EntireColumn.NumberFormat := '#,##0.00';
    end;
  end
  else begin
    lExcelcx := ExcelViewer as TdxSpreadSheet;
    try
      lExcelcx.BeginUpdate;
      lExcelcx.History.BeginAction(TdxSpreadSheetHistoryFormatCellAction);
     for I := 0 to FdxSheet.Rows.Count - 1 do
begin
  if Assigned(FdxSheet.Cells[I, ACol]) then
  begin
    FdxSheet.Cells[I, ACol].Style.Brush.BackgroundColor := dxExcelStandardColors[AColor];
    if IsCurrency then
      FdxSheet.Cells[I, ACol].AsCurrency := FdxSheet.Cells[I, ACol].AsCurrency;
  end;
end;

    finally
      lExcelcx.History.EndAction(False);
      lExcelcx.EndUpdate;
    end;
  end;
end;

procedure TfrmBxImportEXCEL.ExcelOnChangeSheet;
var
  lCells: TStringList;
  I : Integer;

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
       if Index > 701 then
          Result := GetCellName(Index div 702-1)+GetCellName(Index mod 702)
       else
       if Index > 25 then
          Result := GetCellName(Index div 26 - 1)+GetCellName(Index mod 26)
       else
          Result := Chr(Index + Ord('A') );
     end;

begin
  if ckSameSettings.Checked then begin
    ExcelFormatRow(FClasaEconomica, 27, False);
    ExcelFormatRow(FRestante, 25);
    ExcelFormatRow(FTrim1, 25);
    ExcelFormatRow(FTrim2, 25);
    ExcelFormatRow(FTrim3, 25);
    ExcelFormatRow(FTrim4, 25);
    Exit;
  end;

  edClasaEconomica.EditValue := Null;
  edRestante.EditValue   := Null;
  edTrim1.EditValue   := Null;
  edTrim2.EditValue   := Null;
  edTrim3.EditValue   := Null;
  edTrim4.EditValue   := Null;
  edEstimat1.EditValue := Null;
  edEstimat2.EditValue := Null;
  edEstimat3.EditValue := Null;

  lCells := TStringList.Create;
  try
     lCells.AddObject('Neselectat', TObject(-1));
     for I := 0 to ExcelGetColumnCount - 1 do
       lCells.AddObject(GetCellName(I), TObject(I));
     SetImageEdit(edRestante, lCells);
     SetImageEdit(edTrim1, lCells);
     SetImageEdit(edTrim2, lCells);
     SetImageEdit(edTrim3, lCells);
     SetImageEdit(edTrim4, lCells);
     SetImageEdit(edClasaEconomica, lCells);
     SetImageEdit(edEstimat1, lCells);
     SetImageEdit(edEstimat2, lCells);
     SetImageEdit(edEstimat3, lCells);
  finally
     lCells.Free;
  end;
end;

function TfrmBxImportEXCEL.ExcelGetRowCount: Integer;
begin
  Result := 0;
  if (ExcelViewer is TdxSpreadSheet) and (FdxSheet <> nil) then
    Result := FdxSheet.Rows.Count
  else
    if (ExcelApp <> nil) and (FExcelOLESheet <> nil) then
       Result := FExcelOLESheet.UsedRange[0].Rows.Count
end;

function TfrmBxImportEXCEL.ExcelGetText(ARow, ACol: Integer): String;
var
  Cell: TdxSpreadSheetCell;
begin
  Result := '';
  if (ExcelViewer is TdxSpreadSheet) and (FdxSheet <> nil) then
  begin
    Cell := FdxSheet.Cells[ARow, ACol];
    if Assigned(Cell) then
      Result := Cell.AsString
    else
      Result := '';
  end
  else if (ExcelApp <> nil) and (FExcelOLESheet <> nil) then
    Result := FExcelOLESheet.Cells.Item[ARow+1, ACol+1].Value;

  Result := Trim(Result);
end;


function TfrmBxImportEXCEL.ExcelGetColumnCount: Integer;
begin
  Result := 0;
  if (ExcelViewer is TdxSpreadSheet) and (FdxSheet <> nil) then
    Result := FdxSheet.Columns.Count
  else
    if (ExcelApp <> nil) and (FExcelOLESheet <> nil) then
       Result := FExcelOLESheet.UsedRange[0].Columns.Count
end;

procedure TfrmBxImportEXCEL.ExcelDeinitBrowser;
begin
  if ExcelApp <> nil then ExcelApp.Free;
  if ExcelViewer <> nil then FreeAndNil(TWebBrowser(ExcelViewer));
end;

procedure TfrmBxImportEXCEL.ExcelDeInitControl;
begin
  if cst_ExcelView = evWebBrowser then begin

  if FExcelOLESheet <> nil then
    FreeAndNil(FExcelOLESheet);
  if ExcelApp <> nil then begin
    ExcelApp.Quit;
    FreeAndNil(ExcelApp);
  end;
  if ExcelViewer <> nil then
    if ExcelViewer is TWebBrowser then begin
      TWebBrowser(ExcelViewer).Free;
      ExcelViewer := nil;
    end
    else begin
      FreeAndNil(ExcelViewer);
    end;
  end
  else begin
    if ExcelViewer is TdxSpreadSheet then
      TdxSpreadSheet(ExcelViewer).Free;
  end;
end;

procedure TfrmBxImportEXCEL.ExcelAppSheetActivate(
  ASender: TObject; const Sh: IDispatch);
begin
  if FExcelOLESheet <> nil then FExcelOLESheet.Free;
  FExcelOLESheet := TExcelWorksheet.Create(nil);
  try
    FExcelOLESheet.ConnectTo(Sh as _Worksheet);
  except
    FreeAndNil(FExcelOLESheet);
  end;
  ExcelOnChangeSheet;
end;

procedure TfrmBxImportEXCEL.ExcelClearCells(const ARowStart,AColStart, ARowStop, AColStop : Integer);
begin
  if (ExcelViewer is TdxSpreadSheet) and (FdxSheet <> nil) then
    FdxSheet.ClearCells(Rect(AColStart, ARowStart, AColStop, ARowStop))
  else
    if (ExcelApp <> nil) and (FExcelOLESheet <> nil) then
       FExcelOLESheet.Range[FExcelOLESheet.Cells.Item[ARowStart + 1, AColStart + 1], FExcelOLESheet.Cells.Item[ARowStop + 1, AColStop + 1]].ClearContents;
end;

procedure TfrmBxImportEXCEL.FormDestroy(Sender: TObject);
begin
  ExcelDeInitControl;
end;

end.
