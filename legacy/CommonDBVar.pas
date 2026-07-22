unit CommonDBVar;

interface

uses
  Classes, Messages, Windows, Menus, Graphics, dxTL, dxDBTl, dxDBCtrl, cxTL,
  cxDBTL, DB, cxImageComboBox, ZDataSet, cxRepartitorPanel, Variants,
  cxGridCustomTableView, Forms, SysUtils, cxGridDBTableView,
  cxGridDBBandedTableView, DxInspRw, Controls, cxButtons, ComCtrls,
  ActnList
  , IniFiles
  , cxGridCustomView
  , cxGridPopupMenu
  , cxGridTableView
  , cxEdit
  , cxMaskEdit
  , cxSpinEdit
  , cxCalc
  , cxCheckBox
  , cxCurrencyEdit
  , cxTimeEdit
  , cxBlobEdit
  , cxMemo
  , cxImage
  , cxCalendar
  , cxDropDownEdit
  , cxGridBandedTableView
  ;

const
 DefaultcxProps: array[TFieldType] of TcxCustomEditPropertiesClass = (
    TcxMaskEditProperties,       { ftUnknown }
    TcxMaskEditProperties,       { ftString }
    TcxSpinEditProperties,       { ftSmallint }
    TcxSpinEditProperties,       { ftInteger }
    TcxSpinEditProperties,       { ftWord }
    TcxCheckBoxProperties,       { ftBoolean }
    TcxCalcEditProperties,       { ftFloat }
    TcxCurrencyEditProperties,   { ftCurrency }
    TcxCurrencyEditProperties,   { ftBCD }
    TcxDateEditProperties,       { ftDate }
    TcxTimeEditProperties,       { ftTime }
    TcxDateEditProperties,       { ftDateTime }
    TcxBlobEditProperties,       { ftBytes }
    TcxBlobEditProperties,       { ftVarBytes }
    TcxSpinEditProperties,       { ftAutoInc }
    TcxImageProperties,          { ftBlob }
    TcxMemoProperties,           { ftMemo }
    TcxImageProperties,          { ftGraphic }
    TcxMemoProperties,           { ftFmtMemo }
    TcxBlobEditProperties,       { ftParadoxOle }
    TcxBlobEditProperties,       { ftDBaseOle }
    TcxBlobEditProperties,       { ftTypedBinary }
    nil,                         { ftCursor }
    TcxMaskEditProperties,       { ftFixedChar }
    TcxMaskEditProperties,       { ftWideString }
    TcxSpinEditProperties,       { ftLargeInt }
    TcxMaskEditProperties,       { ftADT }
    TcxMaskEditProperties,       { ftArray }
    TcxMaskEditProperties,       { ftReference }
    TcxMaskEditProperties,       { ftDataSet }
    TcxImageProperties,          { ftOraBlob }
    TcxMemoProperties,           { ftOraClob }
    TcxMaskEditProperties,       { ftVariant }
    TcxMaskEditProperties,       { ftInterface }
    TcxMaskEditProperties,       { ftIDispatch }
    TcxMaskEditProperties,       { ftGuid }
    TcxDateEditProperties,       { ftTimeStamp }
    TcxDateEditProperties        { ftFMTBcd }

    {$IFDEF VER230}
    , TcxMaskEditProperties {ftFixedWideChar}
    , TcxMemoProperties {ftWideMemo}
    , nil {ftOraTimeStamp}
    , nil {ftOraInterval}
    , TcxSpinEditProperties {ftLongWord}
    , TcxSpinEditProperties {ftShortint}
    , TcxSpinEditProperties {ftByte}
    , TcxSpinEditProperties {ftExtended}
    , nil {ftConnection}
    , nil {ftParams}
    , nil {ftStream}
    , nil {ftTimeStampOffset}
    , nil {ftObject}
    , TcxCalcEditProperties  {ftSingle}
    {$ENDIF}
  );

type

  EContaHandledError = class(Exception);

  PStructuraColoane = ^StructuraColoane;
  StructuraColoane = record
    Captura : String[100];
    TipColoana : Integer;
    FieldName : String[100];
    SeRepeta : Boolean;
    Vizibila : Boolean;
  end;

  TCrackAtsDBTreeList = class(TCustomdxDBTreeListControl);

  PStockInfo = ^StockInfo;
  StockInfo = record
    IdGestTipStock : Integer;
    Predator : Integer; {Predator = 1, Primitor = 2; Nom = 0 }
    IdGestTipMaterial  : Integer;
    Semn : Integer; {Semnul pentru cantitate daca alegem pentru primitor inmultim cantitatea cu semnul (-) }
    Denumire : String;
    Descriere : String;
  end;

var
  bIsParented     : Boolean;
  bAskLoginInfo   : Boolean = True;
  bAskConnection  : Boolean = True;
  gParentWnd      : HWND;
  gParentObject   : TObject;


  MinDataFisc,
  MaxDataFisc      : TDateTime;
  AnFiscal         : Integer;
  IdLogin          : Integer;
  IsAdmin          : Boolean;
  IdUtilizator     : Integer;
  IdFunctiune      : Integer;
  IdDepartament    : Integer;
  NumeLogin        : String;
  NumeLoginComplet : String;
  ParolaUser       : String;
  Departament      : String;
  ExeVerStr        : String;
  AppName          : String;
  WindowsNT        : string;
  ConnectProtocol  : string;
  SocName          : String;
  ValidationKey    : String;

  eurekaSmtpFrom       : String = 'consjud@cjc.ro';
  eurekaemailSendMode  : Integer = 3;
  eurekaReceiveAddress,
  eurekaSubject,
  eurekaBodyHeader,
  eurekaSmtpIp,
  eurekaSmtpPort,
  eurekaSmtpUserId,
  eurekaSmtpPass       : String;


  ExtendedConfiguration : Boolean;
  ZeroruriCulegere      : Integer=4;
  ListStockInfo : TList = nil;
  Proiect : Variant;

{ Variabile globale pentru antetele rapoartelor }
var
  antetNumeSocietate   : String;
  antetAdresaSocietate : String;
  antetCodFiscal       : String;
  antetTelefon         : String;
  antetLocalitate      : String;
  antetJudet           : String;
  antetFax             : String;
  antetEmail           : String;
  antetImagine         : TGraphic = nil;
  antetAdresaWeb       : String;
  gMainActionList      : TActionList;

  IsRightEnable       : Boolean = False;
  SaveINIToDB         : Boolean = False;

function IsMyFormCovered(const MyForm: TWinControl): Boolean;
function IsMyFormVisible(const MyForm : TWinControl): Boolean;

procedure RegisterMenuItem(const aName, aCategory, aCaption: string; aExecuteEvent : TNotifyEvent);
function TestParmsForString(aSearch : String) : Boolean;
function DelphiRunning: Boolean;

function GetHostName: String;
function IP_Address: String;
function IP_Addres: String;
function ExeVersion: String;
function GetPassword: String;

function TempDirectory: String;
function TempFileName: String;
function IsValidDateStr(ADate: String): Boolean;
function IsValidDate(ADate: Variant): Boolean;
function GetNiceText(const aString:String): String;
function GetAppFileName: String;

function GetIPClassAddress(const aIP:String): String;

procedure DrawProcent(aCanvas: TCanvas; ARect: TRect; Procent: Integer;aBackColor: TColor = clAqua; aFrontColor: TColor = clNavy; const custStr : String='');

function cxFindBandByName(cxGrid : TcxGridDBBandedTableView; aBandName : String) : TcxGridBand; overload;
function cxFindBandByName(aTreeList : TcxDBTreeList; aBandName : String) : TcxTreeListBand; overload;
function cxFindColumnByFieldName (cxTree : TcxDBTreeList; cxFieldName : String) : TcxDBTreeListColumn;
function cxFindItemByComboValue(cxItems : TcxImageComboBoxItems; aValue : Variant) : TcxImageComboBoxItem;
function cxFindColumnByUserName (cxTree : TcxDBTreeList; cxUserName : String) : TcxDBTreeListColumn;
function cxFindColumnByTag (cxTree : TcxDBTreeList; cxTagValue : Integer) : TcxDBTreeListColumn;
function cxFindFirstVisibleColumn (cxTree : TcxDBTreeList) : TcxDBTreeListColumn;
function cxFindNodeByKeyValue(cxGrid : TcxGridDBTableView; KeyValue : Variant) : TcxCustomGridRecord;

function FindColumnByTag (Tree : TCustomdxDBTreeList; TagValue : Integer) : TdxDBTreeListColumn;

function InternalPositioning(Val: String; Tree: TdxDBTreeList; ColumnName : String): Boolean; overload;
function InternalPositioning(Val: String; Tree: TdxDBTreeList): Boolean; overload;
function InternalPositioning(Val: String;  Sender: TObject): Boolean; overload;
function InternalPositioning(Val: String; Tree: TcxDBTreeList; ColumnName : String): Boolean; overload;
function InternalPositioning(Val: String; Tree : TcxDBTreeList) : Boolean; overload;

procedure DoCheckOpen(aDataSet : TZQuery);
procedure DoCheckClose(aDataSet : TZQuery);
procedure DoCheckPostDataSet(aDataSet : TDataSet);

procedure AddInternalPopup(AcxGridPopupMenu : TcxGridPopupMenu; ASenderMenu: TComponent;  AHitTest: TcxCustomGridHitTest; X, Y: Integer; var AllowPopup: Boolean);

function IsValueEmpty(AValue : Variant): Boolean;
function GetInteger(aValue : Variant) : Integer; overload;
function GetCurrency(ARecord: TcxCustomGridRecord; AIndex: Integer): Currency;
function GetInteger(ARecord: TcxCustomGridRecord; AIndex: Integer): Integer; overload;
function GetBoolean(AValue : Variant): Boolean; overload;
function GetBoolean(ARecord: TcxCustomGridRecord; AIndex: Integer): Boolean;  overload;
function GetString(ARecord: TcxCustomGridRecord; AIndex: Integer): String;
function GetDateTime(ARecord: TcxCustomGridRecord; AIndex: Integer): TDateTime;

function GetdxCurrency(ANode : TdxTreeListNode; AIndex : Integer): Currency;

procedure PopulateReportContext(RepFolder : string; btnRap : TcxButton; ReportClick : TNotifyEvent);

 {IsNumeric}
function  IsNumeric(const AValue : String): Boolean;
procedure SetFilterOnDataSet(lQry: TZQuery; lCondition: String); overload;
procedure SetFilterOnDataSet(lQry: TZReadOnlyQuery; lCondition: String); overload;
function  EchivalareCol(ColName : String) : String;

function SetKeyOnPanelTree(aPanel : TcxRepartitorPanel; aValue : Variant;
      TextIndex, SearchIndex : Integer; SearchTree : TcxDBTreeList;
      const ByKeyValue : Boolean = True; const OnlyChildren : Boolean = True) : Boolean;

function LocateOnKeyNode(SearchTree : TcxDbTreeList;
      SearchIndex : Integer; aValue : Variant; const ExactValue : Boolean = False) : TcxTreeListNode;

function SetProiectContext(idProiect : Variant) : Boolean;

function IniFile: TIniFile;
function GetSettingsFileName: String;
function StorageReadValue(aIdent : String; const aDefValue : String = ''; const ASectionName: string = 'GeneralSettings') : String;
procedure StorageWriteValue(aIdent : String; aValue : String; const ASectionName: string = 'GeneralSettings');

procedure StorageReadCxTree(ATree: TcxCustomTreeList; const AIdentName: String = '');
procedure StorageWriteCxTree(ATree: TcxCustomTreeList; const AIdentName: String = '');

procedure StorageReadDxTree(ATree: TCustomdxTreeListControl; const AIdentName: String = '');
procedure StorageWriteDxTree(ATree: TCustomdxTreeListControl; const AIdentName: String = '');

procedure StorageReadCxView(AView: TcxCustomGridView; const AIdentName: String = '');
procedure StorageWriteCxView(AView: TcxCustomGridView; const AIdentName: String = '');

function LocalForceDirectories(Dir: string): Boolean;

function GetValidComponentName (const s:string) : String;
function FindComponentEx(const Name: string): TComponent;

procedure cxSetPropertiesOnField(aProperties : TcxCustomEditProperties; aField : TField);
procedure cxSetColumnDetail(aColumn : TcxGridColumn; aField : TField; const aBaseName : String = ''); overload;
procedure cxSetColumnDetail(aColumn : TcxDBTreeListColumn; aField : TField; const aBaseName : String = ''); overload;

function cxCreateMissingColumns(aDataSet : TDataSet; aGrid : TcxGridDBBandedTableView) : Boolean; overload;
function cxCreateMissingColumns(aDataSet : TDataSet; aGrid : TcxGridDBTableView) : Boolean ; overload;
procedure cxCreateMissingColumns(aDataSet : TDataSet; aTree : TcxDBTreeList); overload;

procedure ExtractFileFromResource(const _ResourceName, _Filename: string);
function GetMaxLevel(ATreeList: TcxTreeList): Integer;
procedure SetExpandLevels(aTreeList : TcxTreeList; ExpandLevels: TToolBar; InternalExpand : TNotifyEvent);
function WideStringToString(const ws: WideString; const codePage: Word = 0): AnsiString;

function GetMainActionList : TActionList;

function IsAdminLoggedOn: Boolean;
function IsPowerUserLoggedOn: Boolean;

const
  WM_REFRESH_QUERY = WM_USER + 1;
  ExceptIsHooked : Boolean = False;

implementation

uses
  Dialogs
  , ZLib
  , Soap.EncdDecd
  , cxVariants
  , SysConst
  , Types
  , DateUtils
  , ZeosDBUtile
  , Registry
  , IdStack
  , dxGrClEx
  , cxTextEdit
  , cxDBData
  , cxGridStdPopupMenu
  , cxGridCustomPopupMenu
  , cxGridMenuOperations
  , dxBar
  , cxGridDBDataDefinitions
  , dxCompsUtile
  , AtlasDBUtils
  ;

var
  gLocalDir     : String = '';
  gLocalPath    : String = '';
  gProfileDir   : String = '';
  gProfilePath  : String = '';
  gIniFile: TIniFile;


procedure InitProfilePath;
var
  lProfileDir : String;
  lRegistry   : TRegistry;
begin
  gLocalDir   := ExtractFileDir(ParamStr(0));
  gLocalPath  := ExtractFilePath(ParamStr(0));
  lProfileDir := '';
  lRegistry   := TRegistry.Create(KEY_READ);
  try
    lRegistry.RootKey := HKEY_CURRENT_USER;
    if (lRegistry.OpenKey('Software\Microsoft\Windows\CurrentVersion\Explorer\Shell Folders', False)) and
       (lRegistry.ValueExists('AppData')) then
       lProfileDir := lRegistry.ReadString('AppData');
  finally
    lRegistry.Free;
  end;
  if (lProfileDir = '') then lProfileDir := gLocalDir;
  lProfileDir := IncludeTrailingPathDelimiter( lProfileDir ) + 'Atlas Settings';
  if not DirectoryExists(lProfileDir) and not ForceDirectories(lProfileDir) then
    ShowMessage('Eroare creare director setari :' + lProfileDir + #13#10 + SysErrorMessage(GetLastError));
  gProfileDir  := lProfileDir;
  gProfilePath := IncludeTrailingPathDelimiter( gProfileDir );
end;

function GetSettingsFileName: String;
begin
  if gProfilePath = '' then
    InitProfilePath;
  if gProfilePath = '' then gProfilePath := '.\';
  Result := gProfilePath + 'SetariContabilitate.ini';
end;

function IniFile: TIniFile;
begin
  if not Assigned(gIniFile) then
    gIniFile := TIniFile.Create(GetSettingsFileName);
  Result := gIniFile;
end;

function GetValidComponentName(const s: string): string;
var
  x: Integer;
  c: Char;
  I : Integer;
begin
  SetLength(Result, Length(s));
  x := 0;
  for I := 1 to Length(s) do begin
    c:= s[i];
    if CharInSet(c, ['A'..'Z', 'a'..'z', '0'..'9', '_']) then begin
      Inc(x);
      Result[x] := c;
    end;
  end;
  SetLength(Result, x);
  if x = 0 then
    Result := '_'
  else if CharInSet(Result[1], ['0'..'9']) then
    Result := '_' + Result;
  // Optional uniqueness protection follows. Choose one.
  //Result := Result + IntToStr(Checksum(s));
end;

function WideStringToString(const ws: WideString; const codePage: Word = 0): AnsiString;
var
  l: integer;
begin
  if ws = '' then
    Result := ''
  else
  begin
    l := WideCharToMultiByte(codePage,
      WC_COMPOSITECHECK or WC_DISCARDNS or WC_SEPCHARS or WC_DEFAULTCHAR,
      @ws[1], - 1, nil, 0, nil, nil);
    SetLength(Result, l - 1);
    if l > 1 then
      WideCharToMultiByte(codePage,
        WC_COMPOSITECHECK or WC_DISCARDNS or WC_SEPCHARS or WC_DEFAULTCHAR,
        @ws[1], - 1, @Result[1], l - 1, nil, nil);
  end;
end; { WideStringToString }

procedure RegisterMenuItem(const aName, aCategory, aCaption: string; aExecuteEvent : TNotifyEvent);
var
  lAction : TAction;
  lActionList : TActionList;
begin
  //dam prioritate la procedurile inregistrate din forme
  lActionList := GetMainActionList;
  lAction := TAction.Create(nil);
  lAction.ActionList := lActionList;
  lAction.Category := aCategory;
  lAction.Caption := aCaption;
  lAction.Name := aName;
  lAction.OnExecute := aExecuteEvent;
end;

function TestParmsForString(aSearch : String) : Boolean;
var
  I : Integer;
begin
  Result := False;
  for i := 1 to ParamCount - 1 do
    if (LowerCase(ParamStr(i)) = LowerCase(aSearch))  then  begin
      Result := True;
      Break;
    end;
end;

function GetMaxLevel(ATreeList: TcxTreeList): Integer;
var
  I: Integer;
begin
  Result := -1;
  with ATreeList do
  for I := 0 to AbsoluteCount - 1 do
    if Result < AbsoluteItems[I].Level then
      Result := AbsoluteItems[I].Level;
end;


function localExtractFilePath(const FileName: string): string;
var
  I: Integer;
begin
  I := LastDelimiter(PathDelim + DriveDelim, FileName);
  Result := Copy(FileName, 1, I);
end;

procedure DoCheckPostDataSet(aDataSet : TDataSet);
begin
  if aDataSet.State in [dsEdit, dsInsert] then begin
      aDataSet.Post;
      aDataSet.Edit;
  end;
end;

procedure DoCheckOpen(aDataSet : TZQuery);
begin
  if not aDataSet.Active then aDataSet.Open;
end;

procedure DoCheckClose(aDataSet : TZQuery);
begin
  if aDataSet.Active then aDataSet.Close;
end;

function GetAppFileName: String;
begin
  if IsLibrary then
     Result := GetModuleName(HInstance)
  else Result := ParamStr(0);
end;

procedure DrawProcent(aCanvas: TCanvas; ARect: TRect;
          Procent: Integer;aBackColor: TColor; aFrontColor: TColor;const custStr : String);
var
  SRect  : TRect;
  S      : String;
  IsOverFlow: Boolean;
begin
   if Procent < 0 then
      Procent := 0;
   aCanvas.Brush.Color := aBackColor;
   aCanvas.FillRect(ARect);
   ACanvas.Brush.Color := clBlack;
   ACanvas.FrameRect(ARect);
   ARect := Rect(ARect.Left + 1, ARect.Top + 2, ARect.Right-1, ARect.Bottom - 2);
   SRect := ARect;
   IsOverFlow := Procent > 10000;
   if IsOverFlow then
      SRect.Right := SRect.Left + ARect.Right - ARect.Left
   else
      SRect.Right := SRect.Left + (Trunc((ARect.Right - ARect.Left) * Procent/10000));
   SRect.Top   := SRect.Top + 1;
   SRect.Bottom:= SRect.Bottom - 1;
   //ACanvas.Brush.Color := clAqua;
   ACanvas.Brush.Color := $00EBAA70;
   ACanvas.FillRect(ARect);
   if IsOverFlow then
      ACanvas.Brush.Color := clRed
   else
      ACanvas.Brush.Color := aFrontColor;
   ACanvas.FillRect(SRect);
   if custStr = '' then begin
     S := Format('%2d.%2d', [(Procent div 100), Procent mod 100])+'%';
     S := StringReplace(S, ' ','0', [rfReplaceAll]);
   end
   else
     S := custStr;
   { Scriem Si Progresul }
   if IsOverFlow then
      aCanvas.Font.Style := aCanvas.Font.Style + [fsBold];
   if Procent div 100 > 30 then
      aCanvas.Font.Color := clWhite
   else
      aCanvas.Font.Color := clBlack;
   SetBkMode(aCanvas.Handle, TRANSPARENT);
   ARect.Top   := ARect.Top + 1;
   ARect.Bottom:= ARect.Bottom - 1;
   DrawText(aCanvas.Handle, PChar(S), Length(S), ARect, DT_CENTER + DT_SINGLELINE + DT_VCENTER);
end;

var
  lFormatInited  : Boolean = False;
  lFormatSettings: TFormatSettings;

function GetLocalFormat: TFormatSettings;
begin
  if not lFormatInited then begin
    lFormatSettings := TFormatSettings.Create(GetThreadLocale);
    lFormatSettings.ShortDateFormat := 'yyyy/mm/dd hh:nn:ss';
    lFormatSettings.LongDateFormat := 'yyyy/mm/dd hh:nn:ss';
    lFormatSettings.DateSeparator := '-';
    lFormatSettings.TimeSeparator := ':';
    lFormatInited := True;
  end;
  Result := lFormatSettings;
end;

function IsValidDateStr(ADate: String): Boolean;
var
  lDate: TDateTime;
begin
  Result := (
              TryStrToDateTime(ADate, lDate) or
              TryStrToDateTime(ADate, lDate, GetLocalFormat)
             ) and IsValidDate(lDate);
end;

function IsValidDate(ADate: Variant): Boolean;
begin
  Result := not( VarIsEmpty(ADate) or VarIsNull(ADate) or VarIsNullDate(ADate) ) and
            VarIsDate(ADate) and (CompareDate(ADate, EncodeDate(1899, 12, 30)) = GreaterThanValue);
end;

function GetPassword: String;
begin
  Result := 'Q'+'A'+'Z'+'}'+'"'+'?'+'x'+'c'+'v'+'>'+'<'+'M';
end;

function TempDirectory: String;
var Buffer: array[1..255] of Char;
begin
  GetTempPath(255, @Buffer[1]);
  Result := StrPas(PChar(@Buffer[1]));
end;

function TempFileName: String;
var Buffer: array[1..255] of Char;
    aTmpDir: String;
begin
  aTmpDir := TempDirectory;
  if GetTempFileName( @aTmpDir[1], 'PRN', 0, @Buffer[1]) <> 0 then
     Result := StrPas(PChar(@Buffer[1]))
  else
     raise EContaHandledError.Create('Nu se poate crea fisierul temporar !');
end;

function ExeVersion: String;
var ExePChar  : PChar;
    aHandle   : DWord;
    aSize     : Integer;
    lpData    : Pointer;
    lpBuffer  : Pointer;
    lpSize    : DWord;
    LangStr   : String;
    LangId    : DWord;
begin
  Result := '0.0.0.0';
  if ExeVerStr = '' then begin
     ExePChar := PChar(ParamStr(0));
     aSize := GetFileVersionInfoSize(ExePChar, aHandle);
     if aSize = 0 then Exit;//RaiseLastWin32Error;
     lpData := AllocMem(aSize);
     try
       if not GetFileVersionInfo(ExePChar, 0, aSize, lpData) then
          Exit;//Abort;
       if not VerQueryValue(lpData, '\VarFileInfo\Translation', lpBuffer, lpSize) then
          Exit;//Abort;
       LangId  := PDWord(lpBuffer)^;
       LangStr := Format('%.4x%.4x',[LangId and $0FFFF,LangId shr 16]);
       if not VerQueryValue(lpData, PChar('\StringFileInfo\'+LangStr+'\FileVersion'), lpBuffer, lpSize) then
          Exit;//Abort;
       ExeVerStr := StrPas(PChar(lpBuffer));
     finally
       FreeMem(lpData, aSize);
     end;
  end;
  Result := ExeVerStr;
end;

function GetHostName: String;
var Len: DWord;
    Buffer : array[1..255] of Char;
begin
  Len:=255;
  GetComputerName(@Buffer[1],Len);
  Result := StrPas(PChar(@Buffer[1]));
end;

function IP_Addres: String;
begin
  if GStack = nil then
     TIdStack.IncUsage;
  Result := GStack.LocalAddress;
  if GStack <> nil then
     TIdStack.DecUsage;
end;

function IP_Address: String;
begin
  if GStack = nil then
     TIdStack.IncUsage;
  Result := GStack.LocalAddresses.CommaText;
  if GStack <> nil then
     TIdStack.DecUsage;
end;


function GetNiceText(const aString:String): String;
var I: Integer;
begin
  Result := StringReplace(aString,'_',' ',[rfReplaceAll]);
  Result := StringReplace(Result,'  ',' ',[rfReplaceAll]);
  Result := Trim(LowerCase(Result));
  if Length(Result)>1 then begin
    I := 2;
    Result[1] := UpCase(Result[1]);
    while I<Length(Result) do begin
      if Result[i]=' ' then
         Result[i+1] := UpCase(Result[i+1]);
      Inc (I);
    end;
  end;
end;

function ReverseString(const aString: String): String;
var I: Integer;
begin
  I := Length(aString);
  Result := '';
  while I > 0 do begin Result := Result + aString[I]; Dec(I) end;
end;

function GetIPClassAddress(const aIP:String): String;
var tmpStr: String;
begin
  tmpStr := ReverseString(aIP);
  if pos('.', tmpStr)>0 then
     Result := ReverseString(copy(tmpStr,pos('.', tmpStr)+1, Length(tmpStr)))
  else Result := aIP;
end;

function InternalPositioning(Val: String;  Tree: TdxDBTreeList; ColumnName : String): Boolean;
var
   aStr : String;
   aNode : TdxTreeListNode;
   aColIndex: Integer;
begin
  Result := False;
  aStr := Val;
  aNode := nil;
  if aStr <> '' then begin
    Tree.TopNode;
    Tree.FullCollapse;
    //deteminam coloana
    if not((Trim(ColumnName) = '') or (UpperCase(Trim(ColumnName)) = UpperCase(Tree.KeyField))) then begin
       if Tree.FindColumnByFieldName(ColumnName) <> nil then aColIndex := Tree.FindColumnByFieldName(ColumnName).Index
       else aColIndex := -1;
    end else aColIndex := -1;

    while (not Assigned(aNode)) and (Length(aStr) > 0) do begin
      if aColIndex <> -1 then
         TCrackAtsDBTreeList(Tree).FindNodeByText(aColIndex, aStr, sdDown, aNode)
      else
         aNode := Tree.FindNodeByKeyValue(aStr);
      if not Assigned(aNode) then Delete(aStr, Length(aStr), 1);
    end;

    if aNode <> nil then begin
      while aNode.HasChildren do begin
        aNode.Expand(False);
        aNode := aNode.Items[0];
      end;
      Result := True;
      aNode.Focused := True;
      aNode.MakeVisible;
      if aColIndex = -1 then
        Tree.ApplyBestFit(Tree.VisibleColumns[0])
      else
        Tree.ApplyBestFit(Tree.Columns[aColIndex]);
      //TCrackAtsDBTreeList(Tree).DoSearch(aStr, sdNone, False);
      Tree.StartSearch(-1, aStr);
    end;
  end;
end;


function InternalPositioning(Val: String; Tree: TdxDBTreeList): Boolean;
begin
  InternalPositioning(Val, Tree, '');
end;

function InternalPositioning(Val: String; Sender: TObject): Boolean; overload;
var
  aCol : String;
begin
  if (Sender is TdxDBTreeListPopupColumn) and (TdxDBTreeListPopupColumn(Sender).PopupControl is TdxDBTreeList) and
     Assigned(TdxDBTreeListPopupColumn(Sender).PopupControl) then
       InternalPositioning(Val,   TdxDBTreeList(TdxDBTreeListPopupColumn(Sender).PopupControl))
  else
  if (Sender is TdxInspectorTextPopupRow) and (TdxInspectorTextPopupRow(Sender).PopupControl is TdxDBTreeList) and
     Assigned(TdxInspectorTextPopupRow(Sender).PopupControl) then
  begin

      if TObject(TdxInspectorTextPopupRow(Sender).Tag) is TParam then
        aCol := EchivalareCol(TParam(TdxInspectorTextPopupRow(Sender).Tag).Name)
      else
        aCol := '';

      InternalPositioning(Val,   TdxDBTreeList(TdxInspectorTextPopupRow(Sender).PopupControl), aCol);
  end;
end;

function cxFindFirstVisibleColumn (cxTree : TcxDBTreeList) : TcxDBTreeListColumn;
var I : Integer;
begin
  Result  := nil;
  for I := 0 to cxTree.ColumnCount- 1 do
    if cxTree.Columns[I].Visible then begin
      Result := TcxDBTreeListColumn(cxTree.Columns[I]);
      Break;
    end;
end;

function cxFindColumnByTag (cxTree : TcxDBTreeList; cxTagValue : Integer) : TcxDBTreeListColumn;
var I : Integer;
begin
  Result  := nil;
  for I := 0 to cxTree.ColumnCount- 1 do
    if cxTree.Columns[I].Tag = cxTagValue then begin
      Result := TcxDBTreeListColumn(cxTree.Columns[I]);
      Break;
    end;
end;

function cxFindColumnByFieldName (cxTree : TcxDBTreeList; cxFieldName : String) : TcxDBTreeListColumn;
var I : Integer;
begin
  Result  := nil;
  for I := 0 to cxTree.ColumnCount- 1 do
    if AnsiCompareText(TcxDBTreeListColumn(cxTree.Columns[I]).DataBinding.FieldName , cxFieldName) = 0 then begin
      Result := TcxDBTreeListColumn(cxTree.Columns[I]);
      Break;
    end;
end;

function cxFindColumnByUserName (cxTree : TcxDBTreeList; cxUserName : String) : TcxDBTreeListColumn;
var I : Integer;
begin
  Result  := nil;
  for I := 0 to cxTree.ColumnCount- 1 do
    if AnsiCompareText(TcxDBTreeListColumn(cxTree.Columns[I]).Name , cxUserName) = 0 then begin
      Result := TcxDBTreeListColumn(cxTree.Columns[I]);
      Break;
    end;
end;


function InternalPositioning(Val: String; Tree: TcxDBTreeList; ColumnName : String): Boolean;
var aStr : String;
   aNode : TcxTreeListNode;
   aColIndex: Integer;
begin
  Result := False;
  aStr := Val;
  if aStr <> '' then begin
    Tree.TopNode;
    Tree.FullCollapse;
    if  not((Trim(ColumnName) = '') or (UpperCase(Trim(ColumnName)) = UpperCase(Tree.DataController.KeyField))) then
      if cxFindColumnByFieldName(Tree, ColumnName) <> nil then begin
        aColIndex := cxFindColumnByFieldName(Tree, ColumnName).ItemIndex;
        aNode := Tree.FindNodeByText(Val, Tree.Columns[aColIndex]);
      end
      else
        aNode := Tree.FindNodeByKeyValue(Val, nil)
    else
      aNode := Tree.FindNodeByKeyValue(Val, nil);
    if aNode <> nil then begin
      if aNode.HasChildren then aNode.Expand(True);
      Result := True;
      aNode.MakeVisible;
      aNode.Focused := True;
    end;
  end;
end;

function InternalPositioning(Val: String; Tree : TcxDBTreeList) : Boolean; overload;
begin
   Result := InternalPositioning(Val, Tree, '');
end;

function EchivalareCol(ColName : String) : String;
begin
  Result := ColName;
  (*
  if (UpperCase(Result) = 'COD_ECONOMIC') or (UpperCase(Result) = 'COD_FUNCTIONAL') then
    Result := 'COD_BUGET';
  *)
end;

procedure SetFilterOnDataSet(lQry: TZQuery; lCondition: String);
begin
  if (Trim(lCondition) = '')
    and ( (Trim(lQry.Filter) <> '') or lQry.Filtered) then begin
     if Assigned(lQry.OnFilterRecord) then lQry.OnFilterRecord := nil;
     lQry.Filtered := False;
     lQry.Filter := '';
  end
  else begin
    if Assigned(lQry.OnFilterRecord) then lQry.OnFilterRecord := nil;
    lQry.Filtered := False;
    lQry.Filter := lCondition;
    lQry.Filtered := True;
  end;
end;

procedure SetFilterOnDataSet(lQry: TZReadOnlyQuery; lCondition: String); overload;
begin
  if (Trim(lCondition) = '')
    and ( (Trim(lQry.Filter) <> '') or lQry.Filtered) then begin
     if Assigned(lQry.OnFilterRecord) then lQry.OnFilterRecord := nil;
     lQry.Filtered := False;
     lQry.Filter := '';
  end
  else begin
    if Assigned(lQry.OnFilterRecord) then lQry.OnFilterRecord := nil;
    lQry.Filtered := False;
    lQry.Filter := lCondition;
    lQry.Filtered := True;
  end;
end;

function IsNumeric(const AValue : String): Boolean;
const cNumeric = ['0'..'9', '.', ','];
var J: Integer;
begin
  Result := Length(AValue ) > 0;
  if Result then
    for J := 1 to Length(AValue) do begin
      if (J=1) and (AValue[J]='-') then Continue;
      if not (AValue[J] in cNumeric) then begin
        Result := False;
        Break;
      end;
    end;
end;

function SetKeyOnPanelTree(aPanel: TcxRepartitorPanel; aValue: Variant; TextIndex, SearchIndex: Integer;
  SearchTree: TcxDBTreeList; const ByKeyValue: Boolean; const OnlyChildren : Boolean): Boolean;
var
  ReasonToExit : Boolean;
  lNode : TcxTreeListNode;
begin
 Result := False;
 aPanel.Text := '';
 ReasonToExit :=
               ((VarIsNull(aValue)) or (Trim(VarToStr(aValue)) = ''))
            or (not Assigned(SearchTree));
 if ReasonToExit then Exit;
 lNode := nil;
 if ByKeyValue then
   lNode := SearchTree.FindNodeByKeyValue(aValue, nil)
 else
      if SearchIndex <> -1 then begin
         lNode := LocateOnKeyNode(SearchTree, SearchIndex, aValue, True);
      end;
 Result := Assigned(lNode) and (not OnlyChildren or not(lNode.HasChildren));
 if Result then begin
   if TextIndex <> -1 then
           aPanel.Text := lNode.Texts[TextIndex];
   try
     aPanel.Tag := TcxDBTreeListNode(lNode).KeyValue;
     aPanel.EditInput.Tag := aPanel.Tag;
   except
     aPanel.Tag := -1;
     aPanel.EditInput.Tag := aPanel.Tag;
   end;
 end;
end;


type
  PFindTextInfo = ^TFindTextInfo;
  TFindTextInfo = record
    Text: string;
    Column: TcxTreeListColumn;
  end;

  
function LocalStartCompare(ANode: TcxTreeListNode; AData: Pointer): Boolean;
begin
   Result :=  (AnsiPos(PFindTextInfo(AData)^.Text, ANode.Texts[PFindTextInfo(AData)^.Column.ItemIndex]) = 1);
end;

function LocalExactCompare(ANode: TcxTreeListNode; AData: Pointer): Boolean;
begin
   Result :=  (AnsiCompareStr(PFindTextInfo(AData)^.Text, ANode.Texts[PFindTextInfo(AData)^.Column.ItemIndex]) = 0);
end;

function LocateOnKeyNode(SearchTree: TcxDbTreeList;
  SearchIndex: Integer; aValue: Variant; const ExactValue : Boolean = False): TcxTreeListNode;
var
  aFindInfo : TFindTextInfo;
begin
  aFindInfo.Text := VarToStr(aValue);
  aFindInfo.Column := SearchTree.Columns[SearchIndex];
  if ExactValue then
    Result := SearchTree.Find(@aFindInfo,nil, False, True, LocalExactCompare)
  else
    Result := SearchTree.Find(@aFindInfo,nil, False, True, LocalStartCompare);
end;

function SetProiectContext(idProiect : Variant) : Boolean;
begin
  Result := DBProcExists('spSetProjectContext');
  if Result then begin
    try
      DBExecSQLFmt('exec [spSetProjectContext] %s, %d', [ValueToStr(idProiect), IdLogin]);
      Result := True;
    except
      Result := False;
    end;
  end;
end;

function IsValueEmpty(AValue : Variant): Boolean;
begin
  Result := VarIsEmpty(AValue) or VarIsClear(AValue) or VarIsNull(AValue);
end;

function GetDateTime(ARecord: TcxCustomGridRecord; AIndex: Integer): TDateTime;
begin
  if IsValueEmpty(ARecord.Values[AIndex]) then
    Result := 0
  else
    Result := ARecord.Values[AIndex];
end;

function GetCurrency(ARecord: TcxCustomGridRecord; AIndex: Integer): Currency;
begin
  if IsValueEmpty(ARecord.Values[AIndex]) then
    Result := 0
  else
    Result := ARecord.Values[AIndex];
end;

function GetString(ARecord: TcxCustomGridRecord; AIndex: Integer): String;
begin
  if IsValueEmpty(ARecord.Values[AIndex]) then
    Result := ''
  else
    Result := ARecord.Values[AIndex];
end;

function GetInteger(ARecord: TcxCustomGridRecord; AIndex: Integer): Integer;
begin
  Result := GetInteger(ARecord.Values[AIndex]);
end;

function GetInteger(aValue : Variant) : Integer;
begin
  if IsValueEmpty(aValue) then
    Result := 0
  else
    Result := aValue;
end;

function GetBoolean(AValue : Variant): Boolean;
begin
  if IsValueEmpty(AValue) then
    Result := False
  else
    Result := AValue;
end;

function GetBoolean(ARecord: TcxCustomGridRecord; AIndex: Integer): Boolean;
begin
  Result := GetBoolean(ARecord.Values[AIndex]);
end;

function FindColumnByTag (Tree : TCustomdxDBTreeList; TagValue : Integer) : TdxDBTreeListColumn;
var I : Integer;
begin
  Result  := nil;
  for I := 0 to Tree.ColumnCount- 1 do
    if Tree.Columns[I].Tag = TagValue then begin
      Result := Tree.Columns[I];
      Break;
    end;
end;

function StorageReadValue(aIdent : String; const aDefValue : String = ''; const ASectionName: string = 'GeneralSettings') : String;
begin
  Result := IniFile.ReadString(ASectionName, aIdent, aDefValue);
//  Result := ValueSafeToStr(DBGetScallarFmt('exec [spReadIniValue] %d, %s, %s, %s', [IdUtilizator, ValueToStr(ASectionName), ValueToStr(aIdent), ValueToStr(aDefValue)]));
end;

procedure StorageWriteValue(aIdent : String; aValue : String; const ASectionName: string = 'GeneralSettings');
begin
  IniFile.WriteString(ASectionName, aIdent, aValue);
//  DBExecSQLFmt('exec [spWriteIniValue] %d, %s, %s, %s', [IdUtilizator, ValueToStr(ASectionName), ValueToStr(aIdent), ValueToStr(aValue)]);
end;

function GetComponentPath(AComponent: TComponent): String;
begin
  Result := '';
  if AComponent.Name = '' then
    raise Exception.Create('Nu se pot salva setarile pentru o componenta fara nume !');
  if Assigned(AComponent.Owner) then begin
    if AComponent.Owner.InheritsFrom(TCustomForm) then begin
      if AComponent.Owner.Name = '' then
        Result := AComponent.Owner.ClassName
      else
        Result := AComponent.Owner.Name;
    end
    else
      Result := GetComponentPath(AComponent.Owner);
    Result := Result + '.' + AComponent.Name;
  end
  else
    Result := AComponent.Name;
end;

procedure StorageReadCxTree(ATree: TcxCustomTreeList; const AIdentName: String);
var
  lIdentName  : String;
  lStrValue   : AnsiString;
  lInStream   : TStringStream;
  lOutStream  : TMemoryStream;
  lZStream    : TZDecompressionStream;
begin
  if AIdentName > '' then lIdentName := AIdentName
  else lIdentName := GetComponentPath(ATree);
  lStrValue := IniFile.ReadString('TreeList', lIdentName, '');
  if lStrValue > '' then begin
    lInStream := TStringStream.Create(lStrValue);
    try
      lOutStream := TMemoryStream.Create;
      try
        DecodeStream(lInStream, lOutStream);
        lOutStream.Position := 0;
        lZStream := TZDecompressionStream.Create(lOutStream);
        try
          ATree.RestoreFromStream(lZStream);
        finally
          lZStream.Free;
        end;
      finally
        lOutStream.Free;
      end;
    finally
      lInStream.Free;
    end;
  end;
end;

procedure StorageWriteCxTree(ATree: TcxCustomTreeList; const AIdentName: String);
var
  lIdentName  : String;
  lStrValue   : AnsiString;
  lSStream    : TStringStream;
  lCXStream   : TMemoryStream;
  lZStream    : TZCompressionStream;
begin
  if AIdentName > '' then lIdentName := AIdentName
  else lIdentName := GetComponentPath(ATree);
  lCXStream := TMemoryStream.Create;
  try
    lZStream := TZCompressionStream.Create(lCXStream);
    try
      ATree.StoreToStream(lZStream);
    finally
      lZStream.Free;
    end;
    lSStream := TStringStream.Create;
    try
      lCXStream.Position := 0;
      EncodeStream(lCXStream, lSStream);
      IniFile.WriteString('TreeList', lIdentName, lSStream.DataString);
    finally
      lSStream.Free;
    end;
  finally
    lCXStream.Free;
  end;
end;

procedure StorageReadDxTree(ATree: TCustomdxTreeListControl; const AIdentName: String = '');
begin
  ATree.LoadFromIniFile(GetSettingsFileName);
end;

procedure StorageWriteDxTree(ATree: TCustomdxTreeListControl; const AIdentName: String = '');
begin
  ATree.SaveToIniFile(GetSettingsFileName);
end;

procedure StorageReadCxView(AView: TcxCustomGridView; const AIdentName: String = '');
var
  lIdentName  : String;
  lStrValue   : AnsiString;
  lInStream   : TStringStream;
  lOutStream  : TMemoryStream;
  lZStream    : TZDecompressionStream;
begin
  if AIdentName > '' then lIdentName := AIdentName
  else lIdentName := GetComponentPath(AView);
  lStrValue := IniFile.ReadString('TreeList', lIdentName, '');
  if lStrValue > '' then begin
    lInStream := TStringStream.Create(lStrValue);
    try
      lOutStream := TMemoryStream.Create;
      try
        DecodeStream(lInStream, lOutStream);
        lOutStream.Position := 0;
        lZStream := TZDecompressionStream.Create(lOutStream);
        try
          AView.RestoreFromStream(lZStream);
        finally
          lZStream.Free;
        end;
      finally
        lOutStream.Free;
      end;
    finally
      lInStream.Free;
    end;
  end;
end;

procedure StorageWriteCxView(AView: TcxCustomGridView; const AIdentName: String = '');
var
  lIdentName  : String;
  lStrValue   : AnsiString;
  lSStream    : TStringStream;
  lCXStream   : TMemoryStream;
  lZStream    : TZCompressionStream;
begin
  if AIdentName > '' then lIdentName := AIdentName
  else lIdentName := GetComponentPath(AView);
  lCXStream := TMemoryStream.Create;
  try
    lZStream := TZCompressionStream.Create(lCXStream);
    try
      AView.StoreToStream(lZStream);
    finally
      lZStream.Free;
    end;
    lSStream := TStringStream.Create;
    try
      lCXStream.Position := 0;
      EncodeStream(lCXStream, lSStream);
      IniFile.WriteString('TreeList', lIdentName, lSStream.DataString);
    finally
      lSStream.Free;
    end;
  finally
    lCXStream.Free;
  end;
end;

function LocalForceDirectories(Dir: string): Boolean;
var
  E: EInOutError;
begin
  Result := True;
  if Dir = '' then
  begin
    E := EInOutError.CreateRes(@SCannotCreateDir);
    E.ErrorCode := 3;
    raise E;
  end;
  Dir := ExcludeTrailingPathDelimiter(Dir);
{$IFDEF MSWINDOWS}
  if (Length(Dir) < 3) or DirectoryExists(Dir)
    or (localExtractFilePath(Dir) = Dir) then Exit; // avoid 'xyz:\' problem.
{$ENDIF}
{$IFDEF LINUX}
  if (Dir = '') or DirectoryExists(Dir) then Exit;
{$ENDIF}
  Result := LocalForceDirectories(localExtractFilePath(Dir)) and CreateDir(Dir);
end;

function DelphiRunning: Boolean;
var
  H1, H2, H3, H4: Hwnd;
const
  (*{x$IFDEF Delphi}*)
    {$IFDEF VER90}
      A0: array[0..10] of Char = 'Delphi 2.0'#0;
    {$ELSE}
      {$IFDEF VER100}
        A0: array[0..8] of Char = 'Delphi 3'#0;
      {$ELSE}
        {$IFDEF VER130}
          A0: array[0..8] of Char = 'Delphi 5'#0;
        {$ELSE}
          {$IFDEF VER120}
            A0: array[0..8] of Char = 'Delphi 4'#0;
          {$ELSE}
            A0: array[0..8] of Char = 'Delphi 7'#0;
          {$ENDIF}
        {$ENDIF}
      {$ENDIF}
    {$ENDIF}
 (*
  {$ELSE}
    {$IFDEF VER130}
      A0: array[0..12] of Char = 'C++Builder 5'#0;
    {$ELSE}
      {$IFDEF VER125}
        A0: array[0..12] of Char = 'C++Builder 4'#0;
      {$ELSE}
        A0: array[0..10] of Char = 'C++Builder'#0;
      {$ENDIF}
    {$ENDIF}
  {$ENDIF}*)
  A1: array[0..12] of Char = 'TApplication'#0;
  A2: array[0..13] of Char = 'TAlignPalette'#0;
  A3: array[0..18] of Char = 'TPropertyInspector'#0;
  A4: array[0..11] of Char = 'TAppBuilder'#0;

begin
  H1 := FindWindow(A1, A0);
  H2 := FindWindow(A2, nil);
  H3 := FindWindow(A3, nil);
  H4 := FindWindow(A4, nil);
  Result :=
  {$IFDEF VER150}
    (H1 <> 0) and (H2 <> 0) and
  {$ELSE}
    (H3 <> 0) and
  {$ENDIF}
   (H4 <> 0);
end;

type
  TCrackcxCustomEditProperties = class(TcxCustomEditProperties);

procedure cxSetPropertiesOnField(aProperties : TcxCustomEditProperties; aField : TField);
begin
  if aProperties = nil then Exit;
  if aProperties is TcxCurrencyEditProperties then begin
    TcxCurrencyEditProperties(aProperties).DisplayFormat := ',0.00;-,0.00';
    TcxCurrencyEditProperties(aProperties).Alignment.Horz := taRightJustify;
  end
  else
  if aProperties is TcxCalcEditProperties then
    TcxCalcEditProperties(aProperties).Alignment.Horz := taRightJustify
  else begin
    TCrackcxCustomEditProperties(aProperties).Alignment.Horz := taLeftJustify;
    TCrackcxCustomEditProperties(aProperties).Alignment.Vert := taVCenter;
  end;
end;


procedure cxSetColumnDetail(aColumn : TcxGridColumn; aField : TField; const aBaseName : String);
begin
   with aColumn do begin
      if DataBinding is TcxGridItemDBDataBinding then
        TcxGridItemDBDataBinding(DataBinding).FieldName := aField.FieldName;
      Caption := GetNiceText(aField.FieldName);
      HeaderAlignmentHorz := taCenter;
      Name :=  aBaseName + '_' + GetValidComponentName(aField.FieldName);
      aColumn.PropertiesClass := DefaultcxProps[aField.DataType];
      cxSetPropertiesOnField(aColumn.Properties, aField);
      Options.Editing := False;
      Options.Filtering := True;
      aColumn.Visible := False;
      aColumn.VisibleForCustomization := True;
//          aColumn.VisibleInQuickCustomizationPopup
   end;
end;

procedure cxSetColumnDetail(aColumn : TcxDBTreeListColumn; aField : TField; const aBaseName : String = '');
begin
    with aColumn do begin
      Caption.Text := GetNiceText(aField.FieldName);
      Caption.AlignHorz := taCenter;
      DataBinding.FieldName := aField.FieldName;
      Name :=  aBaseName + '_' + GetValidComponentName(aField.FieldName);
      aColumn.PropertiesClass := DefaultcxProps[aField.DataType];
      cxSetPropertiesOnField(aColumn.Properties, aField);
      Options.Editing := False;
      Visible := False;
    end;
end;


function cxCreateMissingColumns(aDataSet : TDataSet; aGrid : TcxGridDBBandedTableView) : Boolean;
var
  aColumn : TcxGridDBBandedColumn;
  I : Integer;
  aCustomize : Boolean;
begin
  Result := False;
  aCustomize := aGrid.OptionsCustomize.ColumnsQuickCustomization;
  for I := aGrid.ColumnCount -1  downto 0 do
    if  aGrid.Columns[I].DataBinding.FieldName = '' then aGrid.Columns[I].Free; 
  try
   aDataSet.DisableControls;
   aGrid.OptionsCustomize.ColumnsQuickCustomization := False;
   for I := 0 to aDataSet.FieldCount- 1 do
     if TcxGridDBBandedTableView(aGrid).GetColumnByFieldName(aDataSet.Fields[I].FieldName) = nil then begin
       aColumn := TcxGridDBBandedColumn(aGrid.FindItemByName(aGrid.Name + '_' + GetValidComponentName(aDataSet.Fields[I].FieldName)));
       Result := True;
       if aColumn = nil then begin
         aColumn := aGrid.CreateColumn;
         aColumn.Position.BandIndex := 1;
         aColumn.Position.ColIndex := 0;
         aColumn.Position.RowIndex := 0;
         aColumn.Width             := 100;
       end;
       cxSetColumnDetail(aColumn, aDataSet.Fields[I], aGrid.Name);
     end;
   finally
     aDataSet.EnableControls;
     aGrid.OptionsCustomize.ColumnsQuickCustomization := aCustomize;
   end;
end;

function cxCreateMissingColumns(aDataSet : TDataSet; aGrid : TcxGridDBTableView) : Boolean;
var
  aColumn : TcxGridDBColumn;
  I : Integer;
begin
  try
   aDataSet.DisableControls;
   Result := False;
   for I := aGrid.ColumnCount -1  downto 0 do
     if  aGrid.Columns[I].DataBinding.FieldName = '' then aGrid.Columns[I].Free;
   for I := 0 to aDataSet.FieldCount- 1 do
     if TcxGridDBTableView(aGrid).GetColumnByFieldName(aDataSet.Fields[I].FieldName) = nil then begin
       aColumn := TcxGridDBColumn(aGrid.FindItemByName(aGrid.Name + '_' + GetValidComponentName(aDataSet.Fields[I].FieldName)));
       Result := True;
       if aColumn = nil then begin
         aColumn := aGrid.CreateColumn;
       end;
       cxSetColumnDetail(aColumn, aDataSet.Fields[I], aGrid.Name);
     end;
   finally
     aDataSet.EnableControls;
   end;
end;

procedure cxCreateMissingColumns(aDataSet : TDataSet; aTree : TcxDBTreeList); overload;
var
  lColumn : TcxDBTreeListColumn;
  I : Integer;
  lItem : TcxDBTreeListColumn;
begin
  try
   aDataSet.DisableControls;
   for I := 0 to aDataSet.FieldCount- 1 do
     if aTree.GetColumnByFieldName(aDataSet.Fields[I].FieldName) = nil then begin
     lItem := cxFindColumnByUserName(aTree, aTree.Name + '_' + GetValidComponentName(aDataSet.Fields[I].FieldName));
     if (lItem <> nil) and (lItem is TcxDBTreeListColumn) and (TcxDBTreeListColumn(lItem).DataBinding.FieldName = '')
     then begin
        TcxDBTreeListColumn(lItem).Caption.AlignHorz := taCenter;
        TcxDBTreeListColumn(lItem).DataBinding.FieldName := aDataSet.Fields[I].FieldName;
        TcxDBTreeListColumn(lItem).Caption.Text :=  GetNiceText(aDataSet.Fields[I].FieldName);

        TcxDBTreeListColumn(lItem).PropertiesClass := DefaultcxProps[aDataSet.Fields[I].DataType];
        TcxTextEditProperties(TcxDBTreeListColumn(lItem).Properties).Alignment.Horz := taLeftJustify;
        TcxTextEditProperties(TcxDBTreeListColumn(lItem).Properties).Alignment.Vert := taVCenter;
     end;

     if lItem = nil then begin
       lColumn :=  TcxDBTreeListColumn(aTree.CreateColumn);
       cxSetColumnDetail(lColumn, aDataSet.Fields[I], aTree.Name);
     end;
     end;
   finally
     aDataSet.EnableControls;
   end;
end;

function cxFindNodeByKeyValue(cxGrid : TcxGridDBTableView; KeyValue : Variant) : TcxCustomGridRecord;
var
  lRecIndex : Integer;
begin
  lRecIndex := cxGrid.DataController.FindRecordIndexByKey(KeyValue);
  if lRecIndex = -1 then Result := nil
  else Result := cxGrid.ViewData.GetRecordByRecordIndex(lRecIndex);
end;


function GetdxCurrency(ANode : TdxTreeListNode; AIndex : Integer): Currency;
begin
  if  (ANode = nil) or
     (Trim(ANode.Strings[AIndex]) = '') or
     (VarIsEmpty(ANode.Values[AIndex])) or
     (VarIsNull(ANode.Values[AIndex])) then Result := 0
  else Result := ANode.Values[AIndex];
end;


procedure ExtractFileFromResource(const _ResourceName, _Filename: string);
var
  ResStream: TResourceStream;
  FileStream: TFileStream;
begin
  ResStream := TResourceStream.Create(HInstance, _ResourceName, RT_RCDATA);
  try
    FileStream := TFileStream.Create(_Filename, fmCreate);
    try
      FileStream.CopyFrom(ResStream, 0);
    finally
      FileStream.Free;
    end;
  finally
    ResStream.Free;
  end;
end;

procedure PopulateReportContext(RepFolder : string; btnRap : TcxButton; ReportClick : TNotifyEvent);
var
  lDataSet  : TDataSet;
  lMenuItem : TMenuItem;
  lRepMenu  : TPopupMenu;
begin
  if btnRap.Kind <> cxbkDropDown then Exit;
  lRepMenu := btnRap.DropDownMenu as TPopupMenu;
  if not Assigned(lRepMenu) then begin
    lRepMenu := TPopupMenu.Create(btnRap.Owner);
    btnRap.DropDownMenu := lRepMenu;
  end;
  lDataSet := DBNewQueryFmt('exec [SP_GET_RAPOARTE] %s, %d', [ValueToStr(RepFolder), IdUtilizator]);
  try
    lDataSet.Open;
    lRepMenu.Items.Clear;
    btnRap.Visible := not lDataSet.IsEmpty;
    lDataSet.First;
    while not lDataSet.Eof do begin
      lMenuItem         := TMenuItem.Create(lRepMenu);
      lMenuItem.Name    := 'Rap_' + IntToStr(Abs(lDataSet.FieldByName('ITEM_ID').AsInteger));
      lMenuItem.Caption := lDataSet.FieldByName('DENUMIRE').AsString;
      lMenuItem.Tag     := lDataSet.FieldByName('ITEM_ID').AsInteger;
      lMenuItem.OnClick := ReportClick;
      lRepMenu.Items.Add(lMenuItem);
      lDataSet.Next;
    end;
  finally
    lDataSet.Free;
  end;
end;

type TCrackToolButton = class(TToolButton);

procedure SetExpandLevels(aTreeList : TcxTreeList; ExpandLevels: TToolBar; InternalExpand : TNotifyEvent);
var I: Integer;
    aToolButton : TToolButton;
    FMaxLevel : Integer;    
begin
  { Stergem butoanele anterioare }
  for I := ExpandLevels.ButtonCount-1 downto 0 do
    ExpandLevels.Buttons[I].Free;
  { Creem noile Butoane }
  FMaxLevel := GetMaxLevel(TcxTreeList(aTreeList));
  for I := FMaxLevel+1 downto 1 do begin
    aToolButton := TToolButton.Create(ExpandLevels);
    aToolButton.Style := tbsCheck;
    aToolButton.Tag := I;
    aToolButton.Grouped := True;
    aToolButton.Caption := IntToStr(I);
    aToolButton.OnClick := InternalExpand;
    TCrackToolButton(aToolButton).SetToolBar(ExpandLevels);
  end;
  ExpandLevels.Width := (FMaxLevel + 1) * ExpandLevels.ButtonWidth;
end;


function GetMainActionList : TActionList;
begin
  if gMainActionList = nil then
    gMainActionList := TActionList.Create(nil);
  Result := gMainActionList;
end;

procedure AddInternalPopup(AcxGridPopupMenu : TcxGridPopupMenu; ASenderMenu: TComponent;  AHitTest: TcxCustomGridHitTest; X, Y: Integer; var AllowPopup: Boolean);
var
  lMenu : TcxPopupMenuInfo;
  I : Integer;
  lPopupMenu : TPopupMenu;
  lMainItem: TMenuItem;

  ldxPopupMenu : TdxBarPopupMenu;
  ldxMainItem : TdxBarSubItem;

  lFound : Boolean;

  lOperation : TcxGridPopupMenuOperation;

  function AddMenuItem(lOperation: TcxGridPopupMenuOperation) : TMenuItem;
  begin
    Result := TMenuItem.Create(lPopupMenu);
    if lOperation = nil then
      Result.Caption := '-'
    else
    begin
      Result.Caption := lOperation.Caption;
      Result.Checked := lOperation.Down;
      Result.Enabled := lOperation.Enabled;
      Result.ImageIndex := lOperation.ImageIndex;
      Result.Visible := lOperation.Visible;
      Result.OnClick := lOperation.DoExecute;
    end;
  end;

  function AdddxMenuItem(lOperation: TcxGridPopupMenuOperation) : TdxBarButton;
  begin
    if lOperation = nil then Exit;
    Result := TdxBarButton.Create(ldxPopupMenu);
    Result.ButtonStyle := bsChecked;
    Result.Tag := Integer(lOperation);
    Result.Caption := lOperation.Caption;
    if lOperation.Down then begin
       Result.ButtonStyle := bsChecked;
       Result.Down := True;
    end;
    Result.Enabled := lOperation.Enabled;
    Result.ImageIndex := lOperation.ImageIndex;
    Result.OnClick := lOperation.DoExecute;
  end;

begin
  lMenu := AcxGridPopupMenu.BuiltInPopupMenus.FindPopupMenuInfo(AcxGridPopupMenu.HitGridView, AcxGridPopupMenu.HitType, AcxGridPopupMenu.HitTest);

  if (lMenu <> nil ) and (ASenderMenu <> lMenu.PopupMenu) then begin
    if ASenderMenu is TPopupMenu then begin
      lPopupMenu := ASenderMenu as TPopupMenu;
      lFound := False;
      for I := 0 to lPopupMenu.Items.Count - 1 do
        if lPopupMenu.Items[I].Name = '_ItemCmdProprietati' then begin
          lFound := True;
          Break;
        end;
     if not lFound then begin
       lMainItem := TMenuItem.Create(lPopupMenu);
       lMainItem.Name := '_ItemCmdProprietati';
       lMainItem.Caption := 'Proprietati Grid';
       lPopupMenu.Items.Add(lMainItem);
       if lMenu.PopupMenu is TdxBarPopupMenu then begin
         for I := 0 to TdxBarPopupMenu(lMenu.PopupMenu).ItemLinks.Count - 1 do begin
          lOperation := TcxGridPopupMenuOperation(TdxBarPopupMenu(lMenu.PopupMenu).ItemLinks[I].Item.Tag);
          if lOperation <> nil then begin
            if lOperation.BeginGroup then
              lMainItem.Add(AddMenuItem(nil));
            lMainItem.Add(AddMenuItem(lOperation));
          end;
         end;
       end;
       if lMenu.PopupMenu is TPopupMenu then begin
         for I := 0 to TPopupMenu(lMenu.PopupMenu).Items.Count - 1 do begin
          lOperation := cxGetGridPopupMenuOperation(TPopupMenu(lMenu.PopupMenu).Items[I]);
          if lOperation <> nil then begin
            if lOperation.BeginGroup then
              lMainItem.Add(AddMenuItem(nil));
            lMainItem.Add(AddMenuItem(lOperation));
          end;
         end;
       end;
     end;
    end;

    if ASenderMenu is TdxBarPopupMenu then begin
      ldxPopupMenu := ASenderMenu as TdxBarPopupMenu;
      lFound := False;
      for I := 0 to ldxPopupMenu.ItemLinks.Count -1  do
        if (ldxPopupMenu.ItemLinks[I].Item is TdxBarSubItem) and
            (ldxPopupMenu.ItemLinks[I].Item.Name = '_dxItemCmdProprietati')
        then
        begin
          lFound := True;
          Break;
        end;

      if not lFound  then begin
        ldxMainItem := TdxBarSubItem.Create(ldxPopupMenu);
        ldxMainItem.Caption := 'Proprietati Grid';
        ldxMainItem.Name := '_dxItemCmdProprietati';
        ldxPopupMenu.ItemLinks.Add(ldxMainItem);
        if lMenu.PopupMenu is TdxBarPopupMenu then begin
          for I := 0 to TdxBarPopupMenu(lMenu.PopupMenu).ItemLinks.Count - 1 do begin
            lOperation := TcxGridPopupMenuOperation(TdxBarPopupMenu(lMenu.PopupMenu).ItemLinks[I].Item.Tag);
            if lOperation <> nil then
              ldxMainItem.ItemLinks.Add(AdddxMenuItem(lOperation)).BeginGroup := lOperation.BeginGroup;
          end;
        end;
        if lMenu.PopupMenu is TPopupMenu then begin
          for I := 0 to TPopupMenu(lMenu.PopupMenu).Items.Count - 1 do begin
            lOperation := cxGetGridPopupMenuOperation(TPopupMenu(lMenu.PopupMenu).Items[I]);
            if lOperation <> nil then
              ldxMainItem.ItemLinks.Add(AdddxMenuItem(lOperation)).BeginGroup := lOperation.BeginGroup;
          end;
        end;
      end;
    end;
  end;
end;

function IsMemberOfGroup(const DomainAliasRid: DWORD): Boolean;
{ Returns True if the logged-on user is a member of the specified local
  group. Always returns True on Windows 9x/Me. }
const
  SECURITY_NT_AUTHORITY: TSIDIdentifierAuthority =
    (Value: (0, 0, 0, 0, 0, 5));
  SECURITY_BUILTIN_DOMAIN_RID = $00000020;
  SE_GROUP_ENABLED           = $00000004;
  SE_GROUP_USE_FOR_DENY_ONLY = $00000010;
var
  Sid: PSID;
  CheckTokenMembership: function(TokenHandle: THandle; SidToCheck: PSID;
    var IsMember: BOOL): BOOL; stdcall;
  IsMember: BOOL;
  Token: THandle;
  GroupInfoSize: DWORD;
  GroupInfo: PTokenGroups;
  I: Integer;
begin
  if Win32Platform <> VER_PLATFORM_WIN32_NT then begin
    Result := True;
    Exit;
  end;

  Result := False;

  if not AllocateAndInitializeSid(SECURITY_NT_AUTHORITY, 2,
     SECURITY_BUILTIN_DOMAIN_RID, DomainAliasRid,
     0, 0, 0, 0, 0, 0, Sid) then
    Exit;
  try
    { Use CheckTokenMembership if available. MSDN states:
      "The CheckTokenMembership function should be used with Windows 2000 and
      later to determine whether a specified SID is present and enabled in an
      access token. This function eliminates potential misinterpretations of
      the active group membership if changes to access tokens are made in
      future releases." }
    CheckTokenMembership := nil;
    if Lo(GetVersion) >= 5 then
      CheckTokenMembership := GetProcAddress(GetModuleHandle(advapi32),
        'CheckTokenMembership');
    if Assigned(CheckTokenMembership) then begin
      if CheckTokenMembership(0, Sid, IsMember) then
        Result := IsMember;
    end
    else begin
      GroupInfo := nil;
      if not OpenThreadToken(GetCurrentThread, TOKEN_QUERY, True, Token ) then begin
        if GetLastError <> ERROR_NO_TOKEN then
          Exit;
        if not OpenProcessToken(GetCurrentProcess, TOKEN_QUERY, Token ) then
          Exit;
      end;
      try
        GroupInfoSize := 0;
        if not GetTokenInformation(Token, TokenGroups, nil, 0, GroupInfoSize) and
           (GetLastError <> ERROR_INSUFFICIENT_BUFFER) then
          Exit;

        GetMem(GroupInfo, GroupInfoSize);
        if not GetTokenInformation(Token, TokenGroups, GroupInfo,
           GroupInfoSize, GroupInfoSize) then
          Exit;

        for I := 0 to GroupInfo.GroupCount-1 do begin
          if EqualSid(Sid, GroupInfo.Groups[I].Sid) and
             (GroupInfo.Groups[I].Attributes and (SE_GROUP_ENABLED or
              SE_GROUP_USE_FOR_DENY_ONLY) = SE_GROUP_ENABLED) then begin
            Result := True;
            Break;
          end;
        end;
      finally
        FreeMem(GroupInfo);
        CloseHandle(Token);
      end;
    end;
  finally
    FreeSid(Sid);
  end;
end;

function IsAdminLoggedOn: Boolean;
{ Returns True if the logged-on user is a member of the Administrators local
  group. Always returns True on Windows 9x/Me. }
const
  DOMAIN_ALIAS_RID_ADMINS = $00000220;
begin
  Result := IsMemberOfGroup(DOMAIN_ALIAS_RID_ADMINS);
end;

function IsPowerUserLoggedOn: Boolean;
{ Returns True if the logged-on user is a member of the Power Users local
  group. Always returns True on Windows 9x/Me. }
const
  DOMAIN_ALIAS_RID_POWER_USERS = $00000223;
begin
  Result := IsMemberOfGroup(DOMAIN_ALIAS_RID_POWER_USERS);
end;

function IsMyFormCovered(const MyForm: TWinControl): Boolean;
var
   lhdc : HDC;
   rcClip, rcClient : TRect;
begin
  lhdc := GetDC(MyForm.Handle);
  Result := False;
  if (lhdc <> 0) then begin
        case GetClipBox(lhdc, rcClip) of
        NULLREGION: begin
            Result := True;
        end;
        SIMPLEREGION: begin
            GetClientRect(MyForm.Handle, rcClient);
            if EqualRect(rcClient, rcClip) then begin
                Result := False;
            end else begin
               Result := False;
            end;
        end;
        COMPLEXREGION: begin
           Result := False;
        end
        else begin
            Result := False;
          end;
        end;
        // If we wanted, we could also use RectVisible
        // or PtVisible - or go totally overboard by
        // using GetClipRgn
        ReleaseDC(MyForm.Handle, lhdc);
  end;
end;

{----------------------------------------------------------}
function IsMyFormCovered1(const MyForm: TForm): Boolean;
var
   MyRect: TRect;
   MyRgn, TempRgn: HRGN;
   RType: Integer;
   hw: HWND;
begin
  MyRect := MyForm.BoundsRect;            // screen coordinates
  MyRgn := CreateRectRgnIndirect(MyRect); // MyForm not overlapped region
  hw := GetTopWindow(0);                  // currently examined topwindow
  RType := SIMPLEREGION;                  // MyRgn type

// From topmost window downto MyForm, build the not overlapped portion of MyForm
  while (hw<>0) and (hw <> MyForm.handle) and (RType <> NULLREGION) do
  begin
    // nothing to do if hidden window
    if IsWindowVisible(hw) then
    begin
      GetWindowRect(hw, MyRect);
      TempRgn := CreateRectRgnIndirect(MyRect);// currently examined window region
      RType := CombineRgn(MyRgn, MyRgn, TempRgn, RGN_DIFF); // diff intersect
      DeleteObject( TempRgn );
    end; {if}
    if RType <> NULLREGION then // there's a remaining portion
      hw := GetNextWindow(hw, GW_HWNDNEXT);
  end; {while}

  DeleteObject(MyRgn);
  Result := RType = NULLREGION;
end;

function IsMyFormVisible(const MyForm : TWinControl): Boolean;
begin
  Result:= MyForm.visible and
           isWindowVisible(MyForm.Handle) and
           not IsMyFormCovered(MyForm);
end;

function cxFindItemByComboValue(cxItems : TcxImageComboBoxItems; aValue : Variant) : TcxImageComboBoxItem;
var I : Integer;
begin
  Result := nil;
  for I := 0 to cxItems.Count - 1 do
    if cxItems[I].Value = aValue then begin
      Result := cxItems[I];
      Break;
    end;
end;


function cxFindBandByName(cxGrid : TcxGridDBBandedTableView; aBandName : String) : TcxGridBand;
var
  I : Integer;
begin
  Result := nil;
  for I := 0 to cxGrid.Bands.Count - 1 do
    if SameText(cxGrid.Bands[I].Caption, aBandName) then begin
      Result := cxGrid.Bands[I];
      Break;
    end;
end;

function cxFindBandByName(aTreeList : TcxDBTreeList; aBandName : String) : TcxTreeListBand; overload;
var
  I : Integer;
begin
  Result := nil;
  for I := 0 to aTreeList.Bands.Count - 1 do
    if SameText(aTreeList.Bands[I].Caption.Text, aBandName) then begin
      Result := aTreeList.Bands[I];
      Break;
    end;
end;

function FindComponentEx(const Name: string): TComponent;
var
  FormName: string;
  CompName: string;
  P: Integer;
  Found: Boolean;
  Form: TForm;
  I: Integer;
begin
  // Split up in a valid form and a valid component name
  P := Pos('.', Name);
  if P = 0 then
  begin
    raise Exception.Create('No valid form name given');
  end;
  FormName := Copy(Name, 1, P - 1);
  CompName := Copy(Name, P + 1, High(Integer));
  Found    := False;
  // find the form
  for I := 0 to Screen.FormCount - 1 do
  begin
    Form := Screen.Forms[I];
    // case insensitive comparing
    if AnsiSameText(Form.Name, FormName) then
    begin
      Found := True;
      Break;
    end;
  end;
  if Found then
  begin
    for I := 0 to Form.ComponentCount - 1 do
    begin
      Result := Form.Components[I];
      if AnsiSameText(Result.Name, CompName) then Exit;
    end;
  end;
  Result := nil;
end;


initialization
  antetImagine  := nil;
  Proiect       := Null;
finalization
  if Assigned(gIniFile) then
    gIniFile.Free;
  if Assigned(gMainActionList) then
    gMainActionList.Free;
  if antetImagine <> nil then FreeAndNil(antetImagine);
  if GStack <> nil then FreeAndNil(GStack);
end.
