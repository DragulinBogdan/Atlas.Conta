unit CommonCasa;

interface

uses Graphics, GraphicEx,IniFiles, Forms, SysUtils, Controls, Messages, Windows, dxTL, DB, Classes, ZDataSet;

type
  PDecontInf = ^TDecontInf;
  TDecontInf = record
    NrDec : Integer;
    DataDecont : TDateTime;
    Cod_Casa : Integer;
    Cod_Gest : String[30];
  end;

  PStrActualizare = ^TStrActualizare;
  TStrActualizare = record
    Id : String;
    ParentId : String;
    Defalcat : Boolean;
  end;

  TLineNodeInfo = record
    ID : String;
    RealLineValue : Currency;
    ECL : Boolean;
    TransferState : Integer;
    DataParinte : TDateTime;
    OrigId : String;
  end;

  TCurentHouseType = (cht_Casa, cht_Banca);

  TDisplayImageEdit = (die_TextIcon, die_HintIcon, die_Image);

  TTransferTo = (tt_Casa, tt_Banca, tt_Casadecont, tt_BancaDecont, tt_CasaTempor, tt_BancaTempor);

  TSetTransferTo = set of TTransferTo;

  TTipLista = (tl_fara, tl_cont, tl_proiect, tl_facturi );

  TTipDrepturi = (td_Casier, td_Validator, td_Administrator);

  TListaDrepturi = set of TTipDrepturi;

  TSaveRec = record
     SCurrentHouse : Integer;
     SDataStart    : TDate;
     SDataEnd      : TDate;
     SIsAvans      : Boolean;
     SEstePeZi     : Boolean;
     SIdLogin      : Integer;
     SIdUtilizator : Integer;
     SCodDecont    : Integer;
     SRbCurent     : Integer;
     SDataBuseala  : TDate;
     SSoldInitial  : Currency;
  end;


  TFontInfo = record
     Color    : TColor;
     FontName : TFontName;
     Style    : TFontStyles;
     Size     : Integer;
  end;

  TUserInfo = record
     IsCasier        : Boolean;
     IsDefalcator    : Boolean;
     IsAdministrator : Boolean;
  end;

  TTipCasa = class
     IsBanca    : Boolean;
     IsAvans    : Boolean;
     IsTempor   : Boolean;
     Drepturi   : TListaDrepturi;
     IsCreditor : Boolean;
     Cont_Csp   : String;
     Repartitor : Integer;
     TipValuta  : Integer;
  end;

  TLocalizeRecord = record
     Cod : Integer;
     CodCb : Integer;
     Data : TDateTime;
     NrDecont : Integer;
     DataDecont : TDateTime;
     CodGest : Integer;
  end;
  PLocalizeRecord = ^TLocalizeRecord;


  TDisplayTransfer = record
     Nume : String;
     FontInfo : TFontInfo;
     Color : TColor;
  end;



const
{mesaje windows}

 {frmcasa}
//  WM_SET_ETICHETA         = WM_USER + 900;
  WM_SET_INFO             = WM_USER + 901;
  WM_SET_BARHINT          = WM_USER + 905;
  WM_SET_CAPTION          = WM_USER + 1002;

 {frmRegistru}
  WM_SET_STARE_SOLD       = WM_USER + 902;
  WM_SET_DATA             = WM_USER + 903;
  WM_DROPDOWN_IMAGECOLUMN = WM_USER + 1;
  WM_MOVETOCOLUMNINDEX    = WM_USER + 2;
  WM_CheckCursSchimb      = WM_USER + 1100;
  WM_HideProgress         = WM_USER + 999;

 {frmSearchErrors}
  WM_LOCALIZE             = WM_USER + 3;
  cst_TransferCode        = -9999999;
  cst_LunaDinAn = '%s anul %s';
  cst_Saptamana = 'Sapt. %s (%s - %s)';

  Luni : array[1..12] of String[20]
      = ( 'Ianuarie',   'Februarie',  'Martie',     'Aprilie',    'Mai',        'Iunie',
          'Iulie',      'August',     'Septembrie', 'Octombrie',  'Noiembrie',  'Decembrie' );

  WM_REFRESH_RADIO = WM_USER+4;
  SaveFormat    = '%s\%s.tbl';
  ArhivaDir     = 'Arhiva';
  SaveFileReg   = 'MemReg';
  SaveFileCont  = 'MemCont';
  SaveFileProj  = 'MemProj';
  SaveFileFact  = 'MemFact';
  SaveFileDate  = 'Date';



  {nr maxim de stari de transfer}
  MaxTransfer = 13;

  TransferState : array[0..MaxTransfer] of String =
  (
    {0 - }'Normal',
    {1 - }'Primit din alta Casa - Acceptat',
    {2 - }'Primit din alta Casa - Neacceptat',
    {3 - }'Iesire catre alta Casa - Intializare operatie - Local',
    {4 - }'Iesire catre alta Casa - Confirmare Operatie pe Server',
    {5 - }'Intrare din Banca - Acceptat',
    {6 - }'Intrare din Banca - Neacceptat',
    {7 - }'Iesire catre Banca - Intializare operatie - Local',
    {8 - }'Iesire catre Banca - Confirmare Operatie pe Server',
    {9 - }'Reject - Initalizare Operatie - Local',
    {10- }'Reject - Confirmare Operatie pe Server',
    {11- }'Anunt de confirmare Reject Acceptat pe Server',
    {12- }'Intrare alta Casa - Intializare operatie - Local',
    {13- }'Intrare alta Banca - Intializare operatie - Local'
  );

  (*
  ShareTables : array[0..2, 0..1] of String =
      (
        //('BREGISTRU', 'ISNULL(A.NR_LIST, A.COD)'), //aici imi scoate inclusiv inregistrarea modificata
        ('BREGISTRU', 'A.COD'),
        //nu mai stiu de ce am pus-o pe aia de sus dar cred ca este de la faptul ca atunci cand salvez se dubleaza cand incarca din local si server
        ('BREG_X', 'A.ID_BREG_X'),
        ('BREG_P', 'A.ID_BREG_P')
      );

   *)

   ShareTables : array[0..4, 0..1] of String =
      (
        //('BREGISTRU', 'ISNULL(A.NR_LIST, A.COD)'), //aici imi scoate inclusiv inregistrarea modificata
        ('BREGISTRU', 'COD'),
        //nu mai stiu de ce am pus-o pe aia de sus dar cred ca este de la faptul ca atunci cand salvez se dubleaza cand incarca din local si server
        ('BREG_X', 'ID_BREG_X'),
        ('BREG_P', 'ID_BREG_P'),
        ('GEST_DECONTARI', 'ID_GEST_DECONTARI'),
        ('GEST_DEFALCARE_DECONTARI', 'ID_GEST_DEFALCARE_DECONTARI')
      );


  RegistruPath       = '\Software\ATS\Contabilitate\DBTreeRegistru';
  DeconturiPath      = '\Software\ATS\Contabilitate\DBTreeDecont';



  {procedura}
  procedure SetConstantToFont(aFont : TFont; aFontConstant : TFontInfo);
  {procedura}
  procedure SetFontToConstant(aFont : TFont; var aFontConstant : TFontInfo);
  {ia procedura de executie pentru memregistru}
  function  GetExec(aExec : TTipLista) : String;

  {proceduri de citire si salvare setari in fisier ini}
  {citire setari}
  procedure ReadSettingsRegistru(DefaultIni : Boolean  = False);

  {citste setariile in fisier ini - nume_exe+IdUtilizator.ini}
  procedure ReadSettingsRegistruFile(DefaultIni : Boolean  = False);
  {citeste setariile in baza de date}
  procedure ReadSettingsRegistruDB(DefaultIni : Boolean  = False);
  {citire setariile }
  procedure ReadEffectiveSettings(aIniFile : TCustomIniFile; DefaultIni : Boolean);
  procedure ReadAlways(aIniFile : TCustomIniFile);
  procedure ReadIfSetSettings(aIniFile : TCustomIniFile);

  {scrie in fisierul Ini sau in Baza de date setariile}
  {scriere setari}
  procedure WriteSettingsRegistru;
  {scriere in Fisier INI}
  procedure WriteSettingsRegistruFile;
  {Scriere in baza de date}
  procedure WriteSettingsRegistruDB;
  {Scriere Efectiva Setari}
  procedure WriteEffectiveSettings(aIniFile : TCustomIniFile);
  procedure WriteIfSetSettings(aIniFile : TCustomIniFile);
  procedure WriteAlways(aIniFile : TCustomIniFile);

  {citeste din Ini informatia despre Font in structura definita}
  procedure ReadFontToSection(aIniFile : TCustomIniFile; aSection : String; var aFontInfo : TFontInfo);
  {scrie in Ini informatia despre Font in structura definita}
  procedure WriteFontToSection(aIniFile : TCustomIniFile; aSection : String; aFontInfo : TFontInfo);
  {scrie in fisier ini o imagine}
  procedure WriteImageToSection(aIniFile : TCustomIniFile; aSection : String; aImage : Graphics.TBitmap);
  procedure ReadImageFromSection(aIniFile : TCustomIniFile; aSection : String; aImage : Graphics.TBitmap);


  {procedura de intretinere culori}
  procedure SettingsMaintenance;
  function  GetAsInteger(aStr : String) : Integer; overload;
  function  GetAsInteger(aNode : TdxTreeListNode; aIndex : Integer) : Integer; overload;
  function  GetAsCurrency(aNode : TdxTreeListNode; aIndex : Integer) : Currency;
  procedure RegisterCasa(Curent : Integer);
  procedure RegistrerCodCasa(Cod : Integer);
  procedure PopulateImageAndType(aDataSet: TDataSet; aValues, aDescs : TStrings; aValue, aDesc, aType: String);
  function  GetIntFromSearchType(aSearch : TdxTLSearchType): Integer;
  function  GetSearchTypeFromInt(aValue : Integer) : TdxTLSearchType;
  function  GetIntFromDispType(DispType : TDisplayImageEdit): Integer;
  function  GetDispTypeFromInt(aValue : Integer) : TDisplayImageEdit;
  procedure DrawProcent(aCanvas: TCanvas; ARect: TRect; Procent: Integer;aBackColor: TColor; aFrontColor: TColor);
  function  SlashSep(const Path, S: String): String;
  function  CurrFormat : String;
  procedure GetTipCasa(aId : Integer; aQry : TDataSet; var aTipCasa : TTipCasa);
  function  IsAdoQueryModified(aQry : TZQuery) : Boolean;
  function  GetNextDir(aPath : String) : String;
  function  PrintDispozitie(Cod : Integer) : Boolean;
  //functii pt curs valutar
  function  GetCursValutar(TipValuta : Integer; Data : TDateTime) : Currency;
  function  GetCursForm(TipValuta : Integer; Data : TDateTime; Repartitor : Integer) : Currency;

var
  (*  daca casa pe casa se tine cont de drepturi   TODO citit din baza de date*)


  EtichetaHandle : HWnd;

  CasaHandle, FiltruHandle, ListaHandle : HWnd;

  CurrDecimal : Integer = 2;
  strCurrencyFormat : String = '#,##0.';

  {frecventa de refresh}
  ProgressStep : Integer = 15;

  {setariile gridului}
  cl_FirstLevelColor         : TColor = $006F7740;
  cl_SecondLevelColor        : TColor = clTeal;
  cl_DeletedSecondLevelColor : TColor = clRed;
  cl_ChildColor              : TColor = $00B0B97D;
  cl_ParentColor             : TColor = $0087924E;
  cl_Background              : TColor = clWindow;
  cl_DataColor               : TColor = clAqua;
  cl_FocusedColor            : TColor = clBlue;
  cl_NormalColor             : TColor = clWhite;

  {font settings}
  ft_FirstLevelColor         : TFontInfo = (Color : clWhite; FontName : 'MS Sans Serif'; Style : []; Size : 8 );
  ft_SecondLevelColor        : TFontInfo = (Color : clWhite; FontName : 'MS Sans Serif'; Style : []; Size : 8 );
  ft_DeletedSecondLevelColor : TFontInfo = (Color : clBlack; FontName : 'MS Sans Serif'; Style : []; Size : 8 );
  ft_ChildColor              : TFontInfo = (Color : clWhite; FontName : 'MS Sans Serif'; Style : []; Size : 8 );
  ft_ParentColor             : TFontInfo = (Color : clWhite; FontName : 'MS Sans Serif'; Style : []; Size : 8 );
  ft_Background              : TFontInfo = (Color : clBlack; FontName : 'MS Sans Serif'; Style : []; Size : 8 );
  ft_DataColor               : TFontInfo = (Color : clBlack; FontName : 'MS Sans Serif'; Style : []; Size : 8 );
  ft_FocusedColor            : TFontInfo = (Color : clWhite; FontName : 'MS Sans Serif'; Style : []; Size : 8 );
  ft_NormalFont              : TFontInfo = (Color : clBlack; FontName : 'MS Sans Serif'; Style : []; Size : 8 );

{deconturi}
  {culori}
  clDLevel1                  : TColor = 8404992;
  clDLevel2                  : TColor = 16744448;
  clDLevel3A                 : TColor = 12615680;
  clDLevel4A                 : TColor = 12615808;
  clDLevel3B                 : TColor = 12615680;
  clDLevel4B                 : TColor = 12615808;
  {font}
  ftDLevel1                  : TFontInfo = (Color : 16777215; FontName : 'MS Sans Serif'; Style : []; Size : 8 );
  ftDLevel2                  : TFontInfo = (Color : 16777215; FontName : 'MS Sans Serif'; Style : []; Size : 8 );
  ftDLevel3A                 : TFontInfo = (Color : 16777215; FontName : 'MS Sans Serif'; Style : []; Size : 8 );
  ftDLevel4A                 : TFontInfo = (Color : 16777215; FontName : 'MS Sans Serif'; Style : []; Size : 8 );
  ftDLevel3B                 : TFontInfo = (Color : 16777215; FontName : 'MS Sans Serif'; Style : []; Size : 8 );
  ftDLevel4B                 : TFontInfo = (Color : 16777215; FontName : 'MS Sans Serif'; Style : []; Size : 8 );
{end deconturi}


  cl_Validare : TColor = clWhite;
  ft_Validare : TFontInfo = (Color : clBlack; FontName : 'MS Sans Serif'; Style : []; Size : 8 );

  IsOnQuestion        : Boolean = True;
  ModDeCautare        : TdxTLSearchType = stContain;
  ModAfisTranfer      : TDisplayImageEdit = die_TextIcon;

  CurentSaveDir : String = '.';
  ListOfCustomColors : TStringList;
  rb_DispozitieId : Integer = -1;
  CaseModified : Boolean = False;
  DefaultHouseId : Integer = -1;
  PreferedHouseId : Integer = -1;
  ModAfisTree : Boolean = False;
  rb_BREGCOD : Integer;

  TransferDiplayTable : array[0..MaxTransfer] of TDisplayTransfer;

  {partea de setari}
  IsSaveCasaDefault : Boolean = False;
  IsSavePeZi : Boolean = False;
  IsSaveZileAnt : Boolean = False;
  IsSaveTransfImg : Boolean = False;
  IsSaveDataStart : Boolean = False;
  IsSaveTipDefalcare : Boolean = False;
  {end partea de setari}


  Saved_PeZI : Boolean = True;
  Saved_ZileAnt : Integer = 0;
  Saved_DataCasa : TDateTime = 0;
  Saved_TipDefalcare : Integer = 0;

  NeedToWrite : Boolean = False;
  NeedToRead : Boolean = True;

implementation

uses
  ZeosDBUtile,
  MaintenanceUnit,
  rapInclude,
  SetParamsUnitADO,
  CommonDbVar,
  DateUnit,
  Dialogs,
  UnitSelectCurs;

function GetExec(aExec : TTipLista) : String;
begin
  case aExec of
    tl_fara    : Result := 'SP_CASA_LISTA';
    tl_cont    : Result := 'SP_CASA_LISTA_CONT';
    tl_proiect : Result := 'SP_CASA_LISTA_PROJ';
    tl_facturi : Result := 'SP_CASA_LISTA_FACT'
  end;
end;


procedure ReadSettingsRegistruDB(DefaultIni : Boolean  = False);
var
  aIniFile  : TMemIniFile;
  lStrList  : TStringList;
  lId       : Integer;
  aQry      : TZReadOnlyQuery;
begin
  lId := IdUtilizator;
  if DefaultIni then lId := -1;
  lStrList := TStringList.Create;

  aQry := GetTmpADOQuery;
  with aQry do
    try
      SQL.Add('SELECT SETARE FROM SETARI_UTILIZATOR WHERE ID_UTILIZATOR = ' + IntToStr(lId));
      Open;
      lStrList.Text := Fields[0].AsString;
    finally
      aQry.Free;
    end;
  aIniFile := TMemIniFile.Create('');
  aIniFile.SetStrings(lStrList);
  lStrList.Free;
  ReadEffectiveSettings(aIniFile, DefaultIni);  
end;

procedure ReadSettingsRegistruFile(DefaultIni : Boolean);
begin
  ReadEffectiveSettings(IniFile, DefaultIni);
end;


function GetIntFromSearchType(aSearch : TdxTLSearchType): Integer;
begin
  case aSearch of
    stStart : Result := 0;
    stContain : Result := 1;
    stRight : Result := 2
    else
      Result := 1;
  end;
end;


procedure WriteSettingsRegistru;
begin
  if SaveINIToDB then
    WriteSettingsRegistruDB
  else
    WriteSettingsRegistruFile;
end;

procedure SettingsMaintenance;
var aFrmSettings : TFrmSettings;
begin
  aFrmSettings := TFrmSettings.Create(nil);
  with aFrmSettings do
    try
      ApplyCurrentSettings;
      ShowModal;
      if Modalresult = mrOk then SaveSettings;
    finally
      Free;
    end;
end;

procedure SetConstantToFont(aFont : TFont; aFontConstant : TFontInfo);
begin
  aFont.Name  := aFontConstant.FontName;
  aFont.Size  := aFontConstant.Size;
  aFont.Color := aFontConstant.Color;
  aFont.Style := aFontConstant.Style;
end;

procedure SetFontToConstant(aFont : TFont; var aFontConstant : TFontInfo);
begin
  aFontConstant.FontName := aFont.Name;
  aFontConstant.Size     := aFont.Size;
  aFontConstant.Style    := aFont.Style;
  aFontConstant.Color    := aFont.Color;
end;

function GetAsInteger(aNode : TdxTreeListNode; aIndex : Integer) : Integer;
{var lStr: String;
    Error: Integer;}
begin
   Result := 0;
   if Trim(aNode.Strings[aIndex]) <> '' then begin
       Result := aNode.Values[aIndex];
      {lStr := aNode.Strings[aIndex];
      Val(lStr, Result, Error);
      if Error <> 0 then Result := 0;}
   end;
end;

procedure RegisterCasa(Curent : Integer);
begin
  RegisterCRAdoParam('COD_CB', Curent);
end;


procedure RegistrerCodCasa(Cod : Integer);
begin
  rb_BREGCOD := Cod;
  RegisterCRAdoParam('BREG_COD',rb_BREGCOD);
end;



procedure PopulateImageAndType(aDataSet: TDataSet; aValues, aDescs : TStrings; aValue, aDesc, aType: String);
var OldPoz : TBookmark;
    lValField,
    lDescField,
    lBancaField,
    lAvansField,
    lCasierField,
    lValidField,
    lAdminField,
    lTemporField,
    lContField,
    lCodGestField,
    lIdValutaField
    : TField;
    aTipCasa  : TTipCasa;
begin
  aValues.Clear;
  aDescs.Clear;
  lValField   := aDataSet.FindField(aValue);
  lDescField  := aDataSet.FindField(aDesc);

  lBancaField  := aDataSet.FindField('IS_BANCA');
  lAvansField  := aDataSet.FindField('IS_AVANS');
  lCasierField := aDataSet.FindField('CASIER');
  lValidField  := aDataSet.FindField('VALIDATOR');
  lAdminField  := aDataSet.FindField('ADMIN');
  lTemporField := aDataSet.FindField('IS_TEMPOR');
  lContField   := aDataSet.FindField('CRSP_LEI');
  lCodGestField := aDataSet.FindField('ID_REPARTITORI');
  lIdValutaField := aDataSet.FindField('ID_VALUTA');

  if not Assigned(lValField) then Exit;
  if not Assigned(lDescField) then lDescField := lValField;
  with aDataSet do begin
    OldPoz := GetBookmark;
    DisableControls;
    try
       First;
       while not Eof do begin
         aTipCasa := TTipCasa.Create;
         if lBancaField <> nil then
           aTipCasa.IsBanca := lBancaField.AsBoolean;
         if lAvansField <> nil then
           aTipCasa.IsAvans := lAvansField.AsBoolean;
         if lTemporField <> nil then
           aTipCasa.IsTempor := lTemporField.AsBoolean;
         if lContField <> nil then
           aTipCasa.Cont_Csp := lContField.AsString;
         if lCodGestField <> nil then
           aTipCasa.Repartitor := lCodGestField.AsInteger;

         aTipCasa.TipValuta := 1;           
         if lIdValutaField <> nil then
           aTipCasa.TipValuta := lIdValutaField.AsInteger;

         aTipCasa.Drepturi := [];
         if lCasierField.AsInteger = 1 then aTipCasa.Drepturi := aTipCasa.Drepturi + [td_Casier];
         if lValidField.AsInteger = 1 then aTipCasa.Drepturi := aTipCasa.Drepturi + [td_Validator];
         if lAdminField.AsInteger = 1 then aTipCasa.Drepturi := aTipCasa.Drepturi + [td_Administrator];
         aValues.AddObject(lValField.AsString, TObject(aTipCasa));
         aDescs.Add(Trim(lDescField.AsString) + '['+Trim(lValField.AsString)+']');
         Next;
       end;
    finally
       GotoBookmark(OldPoz);
       FreeBookmark(OldPoz);
       EnableControls;
    end;
  end;
end;

function GetSearchTypeFromInt(aValue : Integer) : TdxTLSearchType;
begin
  case aValue of
     0 : Result := stStart;
     1 : Result := stContain;
     2 : Result := stRight
     else
       Result := stContain;
  end;
end;

procedure DrawProcent(aCanvas: TCanvas; ARect: TRect; Procent: Integer;aBackColor: TColor; aFrontColor: TColor);
var
  SRect  : TRect;
  S      : String;
begin
   if Procent < 0 then Procent := 0;
   aCanvas.Brush.Color := aBackColor;
   aCanvas.FillRect(ARect);
   ACanvas.Brush.Color := clBlack;
   ACanvas.FrameRect(ARect);
   ARect := Rect(ARect.Left + 1, ARect.Top + 2, ARect.Right-1, ARect.Bottom - 2);
   SRect := ARect;
   SRect.Right := SRect.Left + (Trunc((ARect.Right - ARect.Left) * Procent/10000));
   SRect.Top   := SRect.Top + 1;
   SRect.Bottom:= SRect.Bottom - 1;
   ACanvas.Brush.Color := clAqua;
   ACanvas.FillRect(ARect);
   //ACanvas.Brush.Color := $00804000;
   ACanvas.Brush.Color := aFrontColor;
   ACanvas.FillRect(SRect);
   S := Format('%2d.%2d', [(Procent div 100) mod 100, Procent mod 100])+'%';
   S := StringReplace(S, ' ','0', [rfReplaceAll]);
   { Scriem Si Progresul }
   aCanvas.Font.Color := clBlack;
   SetBkMode(aCanvas.Handle, TRANSPARENT);
   ARect.Top   := ARect.Top + 1;
   ARect.Bottom:= ARect.Bottom - 1;
   DrawText(aCanvas.Handle, PChar(S), Length(S), ARect, DT_CENTER + DT_SINGLELINE + DT_VCENTER);
end;

function SlashSep(const Path, S: String): String;
begin
  if AnsiLastChar(Path)^ <> '\' then
    Result := Path + '\' + S
  else
    Result := Path + S;
end;


function CurrFormat : String;
var I :Integer;
   aStr : String;
begin
  aStr := '';
  for I := 0 to CurrDecimal -1 do
      aStr := aStr + '0';
  Result := strCurrencyFormat + aStr
end;

function GetAsCurrency(aNode : TdxTreeListNode; aIndex : Integer) : Currency;
begin
   Result := 0;
   if aNode.Strings[aIndex] <> '' then
     Result := aNode.Values[aIndex];
end;

function GetIntFromDispType(DispType : TDisplayImageEdit): Integer;
begin
  case DispType of
    die_TextIcon : Result := 0;
    die_HintIcon : Result := 1;
    die_Image : Result := 2
    else
      Result := 0;
  end;
end;

function GetDispTypeFromInt(aValue : Integer) : TDisplayImageEdit;
begin
  case aValue of
    0 : Result := die_TextIcon;
    1 : Result := die_HintIcon;
    2 : Result := die_Image
  else
    Result := die_TextIcon;
  end;
end;

function GetAsInteger(aStr : String) : Integer;
var Error: Integer;
begin
   Val(aStr, Result, Error);
   if Error <> 0 then Result := -1;
end;

procedure GetTipCasa(aId : Integer; aQry : TDataSet; var aTipCasa : TTipCasa);
var
  aBookMark : TBookMark;
begin
  {Atentie aTipCasa trebuie eliberat dupa aceea}
  if not Assigned(aTipCasa) or (aTipCasa = nil) then
    aTipCasa := TTipCasa.Create;
  with aQry do begin
    try
      aBookmark := GetBookMark;
      if Locate('COD_CB', aId, []) then begin
         aTipCasa.IsBanca := FieldByName('IS_BANCA').AsBoolean;
         aTipCasa.IsAvans := FieldByName('IS_AVANS').AsBoolean;
         aTipCasa.IsTempor := FieldByName('IS_TEMPOR').AsBoolean;
         aTipCasa.Drepturi := [];
         if FieldByName('CASIER').AsBoolean then aTipCasa.Drepturi := aTipCasa.Drepturi + [td_Casier];
         if FieldByName('VALIDATOR').AsBoolean then aTipCasa.Drepturi := aTipCasa.Drepturi + [td_Validator];
         if FieldByName('ADMIN').AsBoolean then aTipCasa.Drepturi := aTipCasa.Drepturi + [td_Administrator];
         aTipCasa.Cont_Csp    := FieldByName('CRSP_LEI').AsString;
         aTipCasa.IsCreditor  := UpperCase(ValueSafeToStr(DBGetScallarFmt('select fctcont from cplan where cont = %s', [ValueToStr(aTipCasa.Cont_Csp)]))) = 'C';
         aTipCasa.Repartitor  := FieldByName('ID_REPARTITORI').AsInteger;
         if FindField('ID_VALUTA')<> nil then
           aTipCasa.TipValuta := FieldByName('ID_VALUTA').AsInteger;
      end;
      GotoBookmark(aBookMark);
      FreeBookmark(aBookMark);
    finally
    end;
  end;
end;


function IsAdoQueryModified(aQry : TZQuery) : Boolean;
var
  aBook : TBookMark;
begin
  aBook := nil;
  Result := False;
  if aQry.CachedUpdates then Exit;
  with aQry do
    try
      aBook := GetBookMark;
      First;
      while (not Eof) and (not Result )do begin
        if aQry.UpdateStatus <> usUnmodified then
          Result := True; 
        Next;
      end;
    finally
      GotoBookMark(aBook);
      FreeBookMark(aBook);
    end;
end;



function DirExists(Name: string): Boolean;
{$IFDEF XWIN32}
var
  Code: Integer;
begin
  Result := False;
  if Length(Name) <= 0 then Exit;
  Code := GetFileAttributes(PAnsiChar(Name));
  Result := (Code <> -1) and (FILE_ATTRIBUTE_DIRECTORY and Code <> 0);
end;
{$ELSE}
var
  SR: TSearchRec;
begin
  if Name[Length(Name)] = '\' then Delete(Name, Length(Name), 1);
  if (Length(Name) = 2) and (Name[2] = ':') then
    Name := Name + '\*.*';
  Result := FindFirst(Name, faDirectory, SR) = 0;
  Result := Result and (SR.Attr and faDirectory <> 0);
end;
{$ENDIF}


function GetNextDir(aPath : String) : String;
var
  I :Integer;
begin
  I := 1;
  Result := aPath+'\'+IntToStr(I);
  if DirExists(Result) then
     repeat
       Result := aPath+'\'+IntToStr(I);
       I := I+1;
     until not DirExists(Result);
end;



function PrintDispozitie(Cod : Integer) : Boolean;
begin
  Result := True;
  try
    RegistrerCodCasa(Cod);
    LoadReport(rb_DispozitieId);
  except
    Result := False;
  end;

end;


procedure ReadFontToSection(aIniFile : TCustomIniFile; aSection : String; var aFontInfo : TFontInfo);
var aStyle : String;
begin
  with aIniFile do begin
    aFontInfo.FontName := ReadString(aSection, 'FontName', aFontInfo.FontName);
    aFontInfo.Size     := ReadInteger(aSection, 'Size', aFontInfo.Size);
    aFontInfo.Color    := TColor(ReadInteger(aSection, 'Color', aFontInfo.Color));
    aStyle             := ReadString(aSection, 'Style', '');
  end;

  aFontInfo.Style := [];

  if Pos('fsBold', aStyle)>0 then
    aFontInfo.Style := aFontInfo.Style + [fsBold];

  if Pos('fsItalic', aStyle)>0 then
    aFontInfo.Style := aFontInfo.Style + [fsItalic];

  if Pos('fsUnderline', aStyle)>0 then
    aFontInfo.Style := aFontInfo.Style + [fsUnderline];

  if Pos('fsStrikeOut', aStyle)>0 then
    aFontInfo.Style := aFontInfo.Style + [fsStrikeOut];
end;


procedure WriteFontToSection(aIniFile : TCustomIniFile; aSection : String; aFontInfo : TFontInfo);
var aStyle : String;
begin
  aStyle := '';
  if fsBold in aFontInfo.Style then
     aStyle := aStyle + ',fsBold';
  if fsItalic in aFontInfo.Style then
     aStyle := aStyle + ',fsItalic';
  if fsUnderline in aFontInfo.Style then
     aStyle := aStyle + ',fsUnderline';
  if fsStrikeOut in aFontInfo.Style then
     aStyle := aStyle + ',fsStrikeOut';

  with aIniFile do begin
    WriteString(aSection, 'FontName', aFontInfo.FontName);
    WriteInteger(aSection, 'Size', aFontInfo.Size);
    WriteInteger(aSection, 'Color', aFontInfo.Color);
    WriteString(aSection, 'Style', aStyle);
  end;
end;


procedure ReadSettingsRegistru(DefaultIni : Boolean  = False);
begin
  //ReadSettingsRegistruDB(DefaultIni);
  if not DefaultIni and not NeedToRead then Exit;
  if SaveINIToDB then
    ReadSettingsRegistruDB(DefaultIni)
  else
    ReadSettingsRegistruFile(DefaultIni);
  NeedToRead := False;
end;


procedure ReadEffectiveSettings(aIniFile : TCustomIniFile; DefaultIni : Boolean);
begin
  with aIniFile do begin
    ReadAlways(aIniFile);
    if not DefaultIni and not NeedToRead then Exit;
    ReadIfSetSettings(aIniFile);
    NeedToRead := False;
  end;
end;

procedure ReadAlways(aIniFile : TCustomIniFile);
begin
  with aIniFile do begin
  end;
end;

procedure ReadIfSetSettings(aIniFile : TCustomIniFile);
var
   lSection : String;
   I : Integer;
   Image : Graphics.TBitmap;
begin
  with aIniFile do begin
      {partea de setari}
        IsSaveCasaDefault   := ReadBool('SalvareSetari', 'IsSaveCasaDefault'  , IsSaveCasaDefault);
        IsSavePeZi          := ReadBool('SalvareSetari', 'IsSavePeZi'         , IsSavePeZi);
        IsSaveZileAnt       := ReadBool('SalvareSetari', 'IsSaveZileAnt'      , IsSaveZileAnt);
        IsSaveTransfImg     := ReadBool('SalvareSetari', 'IsSaveTransfImg'    , IsSaveTransfImg);
        IsSaveDataStart     := ReadBool('SalvareSetari', 'IsSaveDataStart'    , IsSaveDataStart);
        IsSaveTipDefalcare  := ReadBool('SalvareSetari', 'IsSaveTipDefalcare' , IsSaveTipDefalcare);
      {end partea de setari}

       DefaultHouseId := ReadInteger('CasaDefault', 'CasaDefault', DefaultHouseId);
       if IsSaveCasaDefault then
          PreferedHouseId := ReadInteger('CasaDefault', 'CasaPrefered', PreferedHouseId);


       cl_FirstLevelColor := TColor(ReadInteger('FirstLevel', 'FirstLevelColor', 7305024));
       ReadFontToSection(aIniFile, 'FirstLevel', ft_FirstLevelColor);

       cl_SecondLevelColor := TColor(ReadInteger('SecondLevel', 'SecondLevelColor', 8421376));
       ReadFontToSection(aIniFile, 'SecondLevel', ft_SecondLevelColor);

       cl_DeletedSecondLevelColor := TColor(ReadInteger('DeletedLevel', 'DeletedLevelColor', 255));
       ReadFontToSection(aIniFile, 'DeletedLevel', ft_DeletedSecondLevelColor);

       cl_ChildColor := TColor(ReadInteger('ChildLevel', 'ChildLevelColor', 11581821));
       ReadFontToSection(aIniFile, 'ChildLevel', ft_ChildColor);

       cl_ParentColor := TColor(ReadInteger('ParentLevel', 'ParentLevelColor', 8884814));
       ReadFontToSection(aIniFile, 'ParentLevel', ft_ParentColor);

       cl_DataColor := TColor(ReadInteger('DataHighlight', 'DataColor', 16776960));
       ReadFontToSection(aIniFile, 'DataHighlight', ft_DataColor);

       cl_FocusedColor := TColor(ReadInteger('Focused', 'BackGroundColor', 16711680));
       ReadFontToSection(aIniFile, 'Focused', ft_FocusedColor);

  {partea de transfer}
        if IsSaveTransfImg then Image := Graphics.TBitmap.Create;
        for I := 0 to  MaxTransfer do begin
          lSection := 'Transfer'+Trim(IntToStr(I));
          TransferDiplayTable[I].Nume := ReadString(lSection, 'NumeAfisat', TransferState[I]);
          ZeroMemory(@TransferDiplayTable[I].FontInfo, SizeOf(TFontInfo));
          CopyMemory(@TransferDiplayTable[I].FontInfo, @ft_NormalFont, SizeOf(TFontInfo));
          ReadFontToSection(aIniFile, lSection, TransferDiplayTable[I].FontInfo);
          TransferDiplayTable[I].Color := TColor(ReadInteger(lSection, 'BackGroundColor', cl_NormalColor));
          if IsSaveTransfImg then begin
            {citim imaginiile}
            ReadImageFromSection(aIniFile, 'IMAGINETRANSFER_'+IntToStr(I), Image);
            if not Image.Empty then
              FrmData.ImaginiTransfer.ReplaceMasked(I, Image, Image.TransparentColor);
          end;
        end;
        if IsSaveTransfImg then Image.Free;
  {endpartea de transfer}

       IsOnQuestion := ReadBool('SetariCulegere', 'IsQuestionMark', IsOnQuestion);
       ModDeCautare := GetSearchTypeFromInt(ReadInteger('SetariCulegere', 'ModCautare', 1));
       ModAfisTranfer := GetDispTypeFromInt(ReadInteger('SetariCulegere', 'ModAfisare',0));
       ModAfisTree := ReadBool('SetariCulegere', 'ModAfisTree', False);

       CurrDecimal :=  ReadInteger('SetariGenerale', 'NrZecimale', CurrDecimal);

   {partea de deconturi}
       clDLevel1   :=  ReadInteger('Decont1',  'BackGroundColor', 8404992);
       ReadFontToSection(aIniFile, 'Decont1', ftDLevel1);

       clDLevel2   :=  ReadInteger('Decont2',  'BackGroundColor', 16744448);
       ReadFontToSection(aIniFile, 'Decont2', ftDLevel2);

       clDLevel3A  :=  ReadInteger('Decont3A', 'BackGroundColor', 12615680);
       ReadFontToSection(aIniFile, 'Decont3A', ftDLevel3A);

       clDLevel3B  :=  ReadInteger('Decont3B', 'BackGroundColor', 12615680);
       ReadFontToSection(aIniFile, 'Decont3B', ftDLevel3B);

       clDLevel4A  :=  ReadInteger('Decont4A', 'BackGroundColor', 12615808);
       ReadFontToSection(aIniFile, 'Decont4A', ftDLevel4A);

       clDLevel4B  :=  ReadInteger('Decont4B', 'BackGroundColor', 12615808);
       ReadFontToSection(aIniFile, 'Decont4B', ftDLevel4B);
   {endpartea de deconturi}

       cl_Validare := ReadInteger('Validare', 'Validata', cl_Validare);
       ReadFontToSection(aIniFile, 'Validare', ft_Validare);

       if not Assigned(ListofCustomColors) then ListOfCustomColors := TStringList.Create;
       ListOfCustomColors.Clear;
       ReadSectionValues('ListOfCustomColors', ListOfCustomColors);

       rb_DispozitieId := ReadInteger('Rapoarte Predefinite', 'DispozitiePlata', rb_DispozitieId);

       if IsSavePeZi then
          Saved_PeZI := ReadBool('CasaDefault', 'BifaPeZi', Saved_PeZI);
       if IsSaveZileAnt then
          Saved_ZileAnt := ReadInteger('CasaDefault', 'ZileAnterioare', Saved_ZileAnt);
       if IsSaveDataStart then
          Saved_DataCasa := ReadDate('CasaDefault', 'DataCasa', Saved_DataCasa);
       if IsSaveTipDefalcare then
          Saved_TipDefalcare := ReadInteger('CasaDefault', 'TipDefalcare', Saved_TipDefalcare);

    end;
end;


procedure WriteEffectiveSettings(aIniFile : TCustomIniFile);
begin
   WriteAlways(aIniFile);
   if NeedToWrite then begin
     WriteIfSetSettings(aIniFile);
     NeedToWrite := False;
     NeedToRead := True;
   end else begin
     if Assigned(ListofCustomColors) then begin
       ListOfCustomColors.Free;
       ListOfCustomColors := nil;
     end;
   end;
end;

procedure WriteAlways(aIniFile : TCustomIniFile);
begin
  with aIniFile do begin
     WriteInteger('CasaDefault', 'CasaDefault', DefaultHouseId);
     if IsSaveCasaDefault then
        WriteInteger('CasaDefault', 'CasaPrefered', PreferedHouseId);
     if IsSavePeZi then
          WriteBool('CasaDefault', 'BifaPeZi', Saved_PeZI);
     if IsSaveZileAnt then
          WriteInteger('CasaDefault', 'ZileAnterioare', Saved_ZileAnt);
     if IsSaveDataStart then
          WriteDate('CasaDefault', 'DataCasa', Saved_DataCasa);
     if IsSaveTipDefalcare then
          WriteInteger('CasaDefault', 'TipDefalcare', Saved_TipDefalcare);
   end;
end;

procedure WriteIfSetSettings(aIniFile : TCustomIniFile);
var
  I : Integer;
  lSection : String;
  Image:Graphics.TBitmap;
begin
  with aIniFile do begin
      {partea de setari}
        WriteBool('SalvareSetari', 'IsSaveCasaDefault'  , IsSaveCasaDefault);
        WriteBool('SalvareSetari', 'IsSavePeZi'         , IsSavePeZi);
        WriteBool('SalvareSetari', 'IsSaveZileAnt'      , IsSaveZileAnt);
        WriteBool('SalvareSetari', 'IsSaveTransfImg'    , IsSaveTransfImg);
        WriteBool('SalvareSetari', 'IsSaveDataStart'    , IsSaveDataStart);
        WriteBool('SalvareSetari', 'IsSaveTipDefalcare' , IsSaveTipDefalcare);
      {end partea de setari}


       WriteInteger('FirstLevel', 'FirstLevelColor', cl_FirstLevelColor);
       WriteFontToSection(aIniFile, 'FirstLevel', ft_FirstLevelColor);

       WriteInteger('SecondLevel', 'SecondLevelColor', cl_SecondLevelColor);
       WriteFontToSection(aIniFile, 'SecondLevel', ft_SecondLevelColor);

       WriteInteger('DeletedLevel', 'DeletedLevelColor', cl_DeletedSecondLevelColor);
       WriteFontToSection(aIniFile, 'DeletedLevel', ft_DeletedSecondLevelColor);

       WriteInteger('ChildLevel', 'ChildLevelColor', cl_ChildColor);
       WriteFontToSection(aIniFile, 'ChildLevel', ft_ChildColor);

       WriteInteger('ParentLevel', 'ParentLevelColor', cl_ParentColor);
       WriteFontToSection(aIniFile, 'ParentLevel', ft_ParentColor);

       WriteInteger('DataHighlight', 'DataColor', cl_DataColor);
       WriteFontToSection(aIniFile, 'DataHighlight', CommonCasa.ft_DataColor);

       WriteInteger('Focused', 'BackGroundColor', cl_FocusedColor);
       WriteFontToSection(aIniFile, 'Focused', ft_FocusedColor);

       {partea de transfer}
       Image := Graphics.TBitmap.Create;
       for I := 0 to  MaxTransfer do begin
         lSection := 'Transfer'+Trim(IntToStr(I));
         WriteString(lSection, 'NumeAfisat',TransferDiplayTable[I].Nume);
         WriteInteger(lSection, 'BackGroundColor', TransferDiplayTable[I].Color);
         WriteFontToSection(aIniFile, lSection, TransferDiplayTable[I].FontInfo);
         if IsSaveTransfImg then begin
            {scriem imaginiile}
            Image.Dormant;
            Image.FreeImage;
            Image.ReleaseHandle;
            FrmData.ImaginiTransfer.GetBitmap(I, Image);
            WriteImageToSection(aIniFile, 'IMAGINETRANSFER_'+IntToStr(I), Image);
         end;
       end;
       Image.Free;
       {end partea de transfer}

       WriteBool('SetariCulegere', 'IsQuestionMark', IsOnQuestion);
       WriteInteger('SetariCulegere', 'ModCautare', GetIntFromSearchType(ModDeCautare));
       WriteInteger('SetariCulegere', 'ModAfisare', GetIntFromDispType(ModAfisTranfer));
       WriteBool('SetariCulegere', 'ModAfisTree', ModAfisTree);

       WriteInteger('SetariGenerale', 'NrZecimale', CurrDecimal);
   {partea de deconturi}
       {Deconturi Level1}
       WriteInteger('Decont1', 'BackGroundColor', clDLevel1);
       WriteFontToSection(aIniFile, 'Decont1', ftDLevel1);
       {Deconturi Level2}
       WriteInteger('Decont2', 'BackGroundColor', clDLevel2);
       WriteFontToSection(aIniFile, 'Decont2', ftDLevel2);
       {Deconturi Level3A}
       WriteInteger('Decont3A', 'BackGroundColor', clDLevel3A);
       WriteFontToSection(aIniFile, 'Decont3A', ftDLevel3A);
       {Deconturi Level3B}
       WriteInteger('Decont3B', 'BackGroundColor', clDLevel3B);
       WriteFontToSection(aIniFile, 'Decont3B', ftDLevel3B);
       {Deconturi Level4A}
       WriteInteger('Decont4A', 'BackGroundColor', clDLevel4A);
       WriteFontToSection(aIniFile, 'Decont4A', ftDLevel4A);
       {Deconturi Level4B}
       WriteInteger('Decont4B', 'BackGroundColor', clDLevel4B);
       WriteFontToSection(aIniFile, 'Decont4B', ftDLevel4B);
   {endpartea de deconturi}

       WriteInteger('Validare', 'Validata', cl_Validare);
       WriteFontToSection(aIniFile, 'Validare', ft_Validare);

       if Assigned(ListofCustomColors) then begin
         for I:= 0 to ListOfCustomColors.Count-1 do
            WriteString('ListOfCustomColors', ListOfCustomColors.Names[I], ListOfCustomColors.Values[ListOfCustomColors.Names[I]]);
         ListOfCustomColors.Free;
         ListOfCustomColors := nil;
       end;

       WriteInteger('Rapoarte Predefinite', 'DispozitiePlata', rb_DispozitieId);

    end;
end;

procedure WriteSettingsRegistruFile;
begin
  WriteEffectiveSettings(IniFile);
end;

procedure WriteSettingsRegistruDB;
var
   lIniFile   : TMemIniFile;
   lSettings  : TStringList;
   lPrevSettings: Variant;
begin
  lIniFile :=  TMemIniFile.Create('');
  lSettings := TStringList.Create;
  try
    lPrevSettings := DBGetScallarFmt('select setare from SETARI_UTILIZATOR where id_utilizator = %d', [IdUtilizator]);
    if ValueHasValue(lPrevSettings) then begin
      lSettings.Text := ValueSafeToStr( lPrevSettings );
      lIniFile.SetStrings(lSettings);
    end;
    WriteEffectiveSettings(lIniFile);
    DBExecSQLFmt('delete from SETARI_UTILIZATOR where id_utilizator = %d', [IdUtilizator]);
    lSettings.Clear;
    lIniFile.GetStrings(lSettings);
    DBExecSQLFmt('insert into SETARI_UTILIZATOR (setare, id_utilizator) values(%s, %d)', [ValueToStr(lSettings.Text), IdUtilizator]);
  finally
    lSettings.Free;
    lIniFile.Free;
  end;
end;

procedure WriteData(aPicture : Graphics.TBitmap; Stream: TStream);
var
  CName: string[63];
begin
  with Stream do
  begin
    if aPicture <> nil then
      CName := aPicture.ClassName else
      CName := '';
    Write(CName, Length(CName) + 1);
    if aPicture <> nil then
      aPicture.SaveToStream(Stream);
  end;
end;


procedure ReadData(Stream: TStream; var aPicture : Graphics.TBitmap);
var
  CName: string[63];
  NewGraphic: TGraphic;
  GraphicClass: TGraphicClass;
begin
  Stream.Read(CName[0], 1);
  Stream.Read(CName[1], Integer(CName[0]));
  GraphicClass := FileFormatList.FindGraphicByName(CName);
  if GraphicClass <> nil then
  begin
    NewGraphic := GraphicClass.Create;
    try
      NewGraphic.LoadFromStream(Stream);
      aPicture.Assign(NewGraphic);
    except
      NewGraphic.Free;
      raise;
    end;
  end;
end;



{procedure ConvertBinary(InStream, OutStream : TStream);
  const
    BytesPerLine = MaxInt DIV 2;
  var
    MultiLine: Boolean;
    I: Integer;
    Count: Longint;
    Buffer: array[0..BytesPerLine - 1] of Char;
    Text: array[0..BytesPerLine * 2 - 1] of Char;
  begin
    OutStream.Seek(0, soFromBeginning);
    Count := InStream.Size;
    MultiLine := Count >= BytesPerLine;
    while Count > 0 do
    begin
      if MultiLine then begin
        OutStream.Write(sLineBreak[1], Length(sLineBreak));
      end;
      if Count >= BytesPerLine then I := BytesPerLine else I := Count;
      InStream.Read(Buffer, I);
      BinToHex(Buffer, Text, I);
      OutStream.Write(Text, I * 2);
      Dec(Count, I);
    end;
end;}

procedure ConvertBinary(InStream : TStream; OutStream : TStream);
  const
    Line : String[11] = 'Linia%d=';
  var
    I, LineCount : Integer;
    Count: Longint;
    Buffer: array[0..254] of Char;
    Text: array[0..SizeOf(Buffer) * 2 - 1] of Char;
    LineHead : String;
  begin
    OutStream.Seek(0, soFromBeginning);
    Count := InStream.Size;
    LineCount := Count div SizeOf(Buffer);
    if Count div SizeOf(Buffer) <> 0 then Inc(LineCount, 1);
    LineHead := Format('%d', [LineCount]) + sLineBreak;
    OutStream.Write(LineHead[1], Length(LineHead));
    LineCount := 0;
    while Count > 0 do
    begin
      if Count >= SizeOf(Buffer) then I := SizeOf(Buffer) else I := Count;
      InStream.Read(Buffer, I);
      BinToHex(Buffer, Text, I);
      Inc(LineCount,1);
      LineHead := Format(Line, [LineCount]);
      OutStream.Write(LineHead[1], Length(LineHead));
      OutStream.Write(Text, I * 2);
      OutStream.Write(sLineBreak[1], Length(sLineBreak));
      Dec(Count, I);
    end;
end;

procedure ReadHexToBinary(InString : String; OutStream: TStream);
var
  Count: Integer;
  Buffer: array[0..255] of Char;
  I : Integer;
begin
  OutStream.Position := 0;
  I := 1;
  while I < Length(InString) do begin
    Count := HexToBin(PAnsiChar(@InString[I]), Buffer, SizeOf(Buffer));
    OutStream.Write(Buffer, Count);
    I := I  + Count * 2;
  end;
end;



procedure WriteImageToSection(aIniFile : TCustomIniFile; aSection : String; aImage : Graphics.TBitmap);
var
  BinStream :TMemoryStream;
  StrStream : TStringStream;
  S: AnsiString;
begin
  BinStream := TMemoryStream.Create;
  try
    StrStream := TStringStream.Create(s);
    try
      WriteData(aImage, BinStream);
      BinStream.Seek(0, soFromBeginning);
      StrStream.Size := 0;
      ConvertBinary(BinStream, StrStream);
      StrStream.Seek(0, soFromBeginning);
      aIniFile.WriteString(aSection, 'NR_LINII', StrStream.DataString);
    finally
      StrStream.Free;
    end;
  finally
    BinStream.Free
  end;
end;



procedure ReadImageFromSection(aIniFile : TCustomIniFile; aSection : String; aImage : Graphics.TBitmap);
var
  BinStream :TMemoryStream;
  StrStream : TStringStream;
  S: AnsiString;
  LineCount, I : Integer;
begin
  LineCount := aIniFile.ReadInteger(aSection, 'NR_LINII', 0);
  S := '';
  for I:= 1 to LineCount do
     S:= S + aIniFile.ReadString(aSection, 'Linia' + IntToStr(I), '');
  BinStream := TMemoryStream.Create;
  try
    StrStream := TStringStream.Create(s);
    try
      BinStream.Size := 0;
      ReadHexToBinary(strStream.DataString, BinStream);
      BinStream.Position := 0;
      aImage.Dormant;
      aImage.FreeImage;
      aImage.ReleaseHandle;
      ReadData(BinStream, aImage);
    finally
      StrStream.Free;
    end;
  finally
    BinStream.Free
  end;
end;

function GetCursValutar(TipValuta : Integer; Data : TDateTime) : Currency;
begin
  Result := ValueSafeToCurrency( DBGetScallarFmt('exec [SP_GET_COTATIE_ZI] %d, %s', [TipValuta, ValueDateToStr(Data)]), -1 );
end;

function GetCursForm(TipValuta : Integer; Data : TDateTime; Repartitor : Integer) : Currency;
var
  lSelectCursForm : TfrmSelectCursValutar;
begin
  lSelectCursForm := TfrmSelectCursValutar.Create(nil);
  try
    lSelectCursForm.TipValuta := TipValuta;
    lSelectCursForm.DataCurs := Data;
    lSelectCursForm.IdRep := Repartitor;
    lSelectCursForm.CompleteScreenInformation;
    lSelectCursForm.ShowModal;
    Result := lSelectCursForm.ValutaValue;
  finally
    lSelectCursForm.Free;
  end;
end;

end.
