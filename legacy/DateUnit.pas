unit DateUnit;

interface


uses
  Windows, cxGridTableView, cxVGrid, cxClasses, cxStyles, cxTL,
  WUpdateWiz, WUpdateLanguages, ImgList, Controls, WUpdate, DB, ZDataSet,
  Classes, Messages, SysUtils, Graphics,  Forms, Dialogs, ExtCtrls, dxExEdtr,
  dxEdLib, ZAbstractRODataset, ZAbstractDataset, ZDbcDbLibStatement, ZConnection,
  ZSqlUpdate, WUpdateLanguagesU, ZAbstractConnection, dxLayoutLookAndFeels,
  cxLocalization, dxServerModeData, dxServerModeADODataSource, cxGrid;

type

 TUpdateLocation = class(TObject)
  private
    FPassword: String;
    FUserName: String;
    FAdresaCompleta: String;
    FDirectory: String;
    FUpdateType: TWebUpdateType;
    FIsEnabled: Boolean;
  public
    property AdresaCompleta: String read FAdresaCompleta write FAdresaCompleta;
    property UserName: String read FUserName write FUserName;
    property Password: String read FPassword write FPassword;
    property Directory: String read FDirectory write FDirectory;
    property UpdateType: TWebUpdateType read FUpdateType write FUpdateType;
    property IsEnabled : Boolean read FIsEnabled write FIsEnabled;
  end;

  TfrmData = class(TDataModule)
    DTPlanCont: TDataSource;
    DTRepartitori: TDataSource;
    DTOperatori: TDataSource;
    DTDocumente: TDataSource;
    DTDefaDoc: TDataSource;
    DTTipStock: TDataSource;
    DTDefaStock: TDataSource;
    DTFunctiuni: TDataSource;
    AutoUpdate: TWebUpdate;
    DTTipDoc: TDataSource;
    DTValoriValute: TDataSource;
    DTOrganigrama: TDataSource;
    ImaginiTransfer: TImageList;
    DTCasaSalariati: TDataSource;
    DTCasaFunctie: TDataSource;
    ErrorList: TImageList;
    DTBugetProiecte: TDataSource;
    DTBugetTipOrdonator: TDataSource;
    DTAntetUnitate: TDataSource;
    DTDelegati: TDataSource;
    DTMijTransport: TDataSource;
    DTTipMIjTransport: TDataSource;
    DTCursuri: TDataSource;
    SemnImagini: TImageList;
    DTRepTipuri: TDataSource;
    DTOIUnitati: TDataSource;
    DTOIUnitatiTipuri: TDataSource;
    UpdateWizard: TWebUpdateWizard;
    WebUpdateWizardRomanian: TWebUpdateWizardRomanian;
    DTBGPlanFunctional: TDataSource;
    DTBGPlanEconomic: TDataSource;
    DTOIProiecteTipuri: TDataSource;
    DTOIProiecte: TDataSource;
    DTBGTipuriBuget: TDataSource;
    NavBarSmallImages: TImageList;
    BarManagerImages: TImageList;
    imMain: TImageList;
    NavBarLargeImages: TImageList;
    DUStyleRepository: TcxStyleRepository;
    cxStyle1: TcxStyle;
    TreeListStyleSheetHighContrastWhite: TcxTreeListStyleSheet;
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
    cxVerticalGridStyleSheetHighContrastWhite: TcxVerticalGridStyleSheet;
    cxStyle13: TcxStyle;
    cxStyle14: TcxStyle;
    cxStyle15: TcxStyle;
    cxStyle16: TcxStyle;
    cxStyle17: TcxStyle;
    cxStyle18: TcxStyle;
    DTGESTTipProduse: TDataSource;
    DTGESTTipStoc: TDataSource;
    DTBGPlanFunctionalComplet: TDataSource;
    cxStyle19: TcxStyle;
    cxStyle20: TcxStyle;
    cxStyle21: TcxStyle;
    cxStyle22: TcxStyle;
    TreeListStyleSheetUserFormat4: TcxTreeListStyleSheet;
    cxStyle23: TcxStyle;
    cxStyle24: TcxStyle;
    cxStyle25: TcxStyle;
    cxStyle26: TcxStyle;
    cxStyle27: TcxStyle;
    cxStyle28: TcxStyle;
    cxStyle29: TcxStyle;
    cxStyle30: TcxStyle;
    cxStyle31: TcxStyle;
    cxStyle32: TcxStyle;
    cxStyle33: TcxStyle;
    GridTableViewStyleSheetRainyDay: TcxGridTableViewStyleSheet;
    cxStyle34: TcxStyle;
    cxStyle35: TcxStyle;
    cxStyle36: TcxStyle;
    cxStyle37: TcxStyle;
    cxStyle38: TcxStyle;
    cxStyle39: TcxStyle;
    cxStyle40: TcxStyle;
    cxStyle41: TcxStyle;
    cxStyle42: TcxStyle;
    cxStyle43: TcxStyle;
    cxStyle44: TcxStyle;
    cxStyle45: TcxStyle;
    cxStyle46: TcxStyle;
    DTOrdCasa: TDataSource;
    DTOrdonantari: TDataSource;
    cxStyle47: TcxStyle;
    cxStyle48: TcxStyle;
    cxStyle49: TcxStyle;
    GridTableViewStyleSheetCustom: TcxGridTableViewStyleSheet;
    cxStyle50: TcxStyle;
    cxStyle51: TcxStyle;
    cxStyle52: TcxStyle;
    cxStyle53: TcxStyle;
    cxStyle54: TcxStyle;
    cxStyle55: TcxStyle;
    cxStyle56: TcxStyle;
    cxStyle57: TcxStyle;
    cxStyle58: TcxStyle;
    cxStyle59: TcxStyle;
    cxStyle60: TcxStyle;
    cxStyle61: TcxStyle;
    cxStyle62: TcxStyle;
    cxStyle63: TcxStyle;
    DTRepGrupa: TDataSource;
    DTRepDomeniu: TDataSource;
    DTCTipDoc: TDataSource;
    cxStyle64: TcxStyle;
    cxStyle65: TcxStyle;
    cxStyle66: TcxStyle;
    cxStyle67: TcxStyle;
    cxStyle68: TcxStyle;
    dbContabilitate: TZConnection;
    QryPlanCont: TZQuery;
    QryRepartitori: TZQuery;
    QryOperatori: TZQuery;
    QryDocumente: TZQuery;
    QryDefaDoc: TZQuery;
    QryTipStock: TZQuery;
    QryDefaStock: TZQuery;
    QryFunctiuni: TZQuery;
    QryTipDoc: TZQuery;
    QryValoriValute: TZQuery;
    QryOrganigrama: TZQuery;
    QryCasaSalariati: TZQuery;
    QryCasaFunctie: TZQuery;
    QryBugetProiecte: TZQuery;
    QryBugetTipOrdonator: TZQuery;
    QryAntetUnitate: TZQuery;
    QryDelegati: TZQuery;
    QryMijTransport: TZQuery;
    QryTipMijTransport: TZQuery;
    QryCursuri: TZQuery;
    QryRepTipuri: TZQuery;
    qryOIUnitati: TZQuery;
    qryOIUnitatiTipuri: TZQuery;
    qryBGPlanFunctional: TZQuery;
    qryBGPlanEconomic: TZQuery;
    qryOIProiecteTipuri: TZQuery;
    qryOIProiecte: TZQuery;
    qryBGTipuriBuget: TZQuery;
    qryGESTTipProduse: TZQuery;
    qryGESTTipStoc: TZQuery;
    qryBGPlanFunctionalComplet: TZQuery;
    QryOrdCassa: TZQuery;
    QOrdonantari: TZQuery;
    QryRepGrupa: TZQuery;
    QryRepDomeniu: TZQuery;
    QryCTipDoc: TZQuery;
    usOIUnitati: TZUpdateSQL;
    usOIProiecte: TZUpdateSQL;
    stilDelete: TcxStyle;
    procedure DataModuleCreate(Sender: TObject);
    procedure QryDefaStockNewRecord(DataSet: TDataSet);
    procedure QryTipStockNewRecord(DataSet: TDataSet);
    procedure QryDefaDocNewRecord(DataSet: TDataSet);
    procedure QryRepartitoriNewRecord(DataSet: TDataSet);
    procedure QryFunctiuniNewRecord(DataSet: TDataSet);
    procedure QryOperatoriNewRecord(DataSet: TDataSet);
    procedure dbContabilitateAfterConnect(Sender: TObject);
    procedure AutoUpdateAppRestart(Sender: TObject; var allow: Boolean);
    procedure AutoUpdateCustomValidate(Sender: TObject; msg, param: String;
      var allow: Boolean);
    procedure QryPlanProiectNewRecord(DataSet: TDataSet);
    procedure QryTipCheltVenNewRecord(DataSet: TDataSet);
    procedure QryPlanContAfterPost(DataSet: TDataSet);
    procedure QryPlanContAfterDelete(DataSet: TDataSet);
    procedure QryFunctionalNewRecord(DataSet: TDataSet);
    procedure QryBugetProiecteNewRecord(DataSet: TDataSet);
    procedure QryBugetProiecteAfterInsert(DataSet: TDataSet);
    procedure QryCursuriNewRecord(DataSet: TDataSet);
    procedure QryCursuriAfterInsert(DataSet: TDataSet);
    procedure DataModuleDestroy(Sender: TObject);
    procedure AutoUpdateStatus(Sender: TObject; statusstr: String;
      statuscode, errcode: Integer);
    procedure qryOIUnitatiAfterInsert(DataSet: TDataSet);
    procedure qryOIUnitatiTipuriNewRecord(DataSet: TDataSet);
    procedure qryOIUnitatiTipuriAfterInsert(DataSet: TDataSet);
    procedure qryOIUnitatiAfterOpen(DataSet: TDataSet);
    procedure dbContabilitateAfterReconnect(Sender: TObject);
  private
    FID_Tip_CheltVen: Integer;
    procedure ReadSQLSettings;
    function GetNextCod: Integer;
    function GetIdItems: Integer;
    procedure SetLocaleStr(Locale, LocaleType: Integer; const Value: String);
   { Private declarations }
    procedure DoOnNewForm(const className: String; formObj: TComponent);
  public
    FUpdateLocations : TStringList;
    FUpdateDescriptions : TStringList;
    IsRestarting  :Boolean;
    IsUpdateError : Boolean;
    UpdWizInExecute : Boolean;
    UpdExecuteClasic : Boolean;
    procedure RefreshUpdateLocations(DB : TZConnection);
    function  DoUpdateFromLocation(UpdateLocation : TUpdateLocation; Forced : Boolean=False): Boolean;
    procedure AutoUpdates(DB:TZConnection);
    property  MaxCod : Integer read GetNextCod;
    property  IdItem: Integer read GetIdItems;
    property  ID_Tip_CheltVen : Integer read FID_Tip_CheltVen write FID_Tip_CheltVen;

    { Public declarations }
  end;

function  GetTmpADOQuery: TZReadOnlyQuery;
function  GetNextId(TblName: String): Integer;
function  GetNextBulkId(TblName: String; Count : Integer; var MaxID : Integer): Integer;
procedure TesteazaVersiune(AVersion: String);
function  GetItemId(aReportVirtualId : String) : Integer;
procedure ShowEroare(const AMessage: String); overload;
procedure ShowEroare(const AFormat: String; const AParams: array of const); overload;
//intoarce 1 daca exista si 0 in caz ca nu exista
procedure SetTipAngajament(aList : TStrings; const Description : Boolean = False);
procedure PopulateImage(aDataSet: TDataSet; aValues, aDescs : TStrings; aValue, aDesc: String; AAll: Boolean=False; AAllDesc: String='');
//bara de hint de pe main form
procedure SetHintInfo(HintText : String);
procedure SetRapParam(aName : String; aValue : Variant);


var
  frmData: TfrmData;
  RegKeyPrefix : String;
  IdAngajament, IdOrdonantare, IdDispozitie,
  IdGestDocum  : Integer;

implementation

{$R *.DFM}

uses
  TypInfo,
  dxCompsUtile,
  AtlasSkinUnit,
  HookForms,
  regionalSettingsUnit,
  ATSZDBUtils,
  ZeosDBUtile,
  SetParamsUnitADO,
  Registry,
  CommonDBVar,
  {CommonCasa,} UpdStructure,
  Variants, dxDBInRw, dxDBTL, dxDBTLCL,
  dxStatusBar, RapImplicit, svnInfo, Winsock,
  AutoUpdate, cxFormats, IdTCPClient, dxCore;

function PortTCP_IsOpen(const APort: Integer; const AAddress: string):
    Boolean;
var
  LTcpClient: TIdTCPClient;
begin
  LTcpClient := TIdTCPClient.Create(nil);
  try
    try
      LTcpClient.Host := AAddress;      //which server to test
      LTcpClient.Port := APort;         //which port to test
      LTcpClient.ConnectTimeout := 200; //assume a port to be clodes if it does not respond within 200ms (some ports will immediately reject, others are using a "stealth" mechnism)
      LTcpClient.Connect;               //try to connect
      result := true;                   //port is open
    except
      result := false;
    end;
  finally
    freeAndNil(LTcpClient);
  end;
end;


procedure SetHintInfo(HintText : String);
var
  aStatusBar : TdxStatusBar;
begin
  aStatusBar := nil;
  if Application.MainForm.FindComponent('MainStatusBar') is TdxStatusBar then
    aStatusBar := TdxStatusBar(Application.MainForm.FindComponent('MainStatusBar'));
  if aStatusBar <> nil then
    aStatusBar.Panels[3].Text := HintText;
end;


procedure ShowEroare(const AMessage: String);
begin
  raise EContaHandledError.Create(AMessage);
end;

procedure ShowEroare(const AFormat: String; const AParams: array of const);
begin
  raise EContaHandledError.CreateFmt(AFormat, AParams);
end;

procedure SetTipAngajament(aList : TStrings; const Description : Boolean);
begin
  aList.Clear;
  if Description then begin
    aList.Add('Bugetar Global');
    aList.Add('Legal');
    aList.Add('Bugetar Individual');
  end
  else
    aList.CommaText := '0,1,2';
end;

function GetItemId(aReportVirtualId : String) : Integer;
begin
  Result := ValueSafeToInt( DBGetScallarFmt('SELECT ITEM_ID FROM RAPOARTE_ASOCIERE WHERE NUME_RAPORT = %s', [ValueToStr(aReportVirtualId)]), -1 );
  if Result = -1 then
    DBExecSQLFmt('INSERT INTO RAPOARTE_ASOCIERE(NUME_RAPORT, ITEM_ID) VALUES (%s, -1)', [ValueToStr(aReportVirtualId)]);
  if (Result = -1) and IsAdmin then EditareRapoarteImplicite;
  //todo administrare daca este -1
end;

procedure PopulateImage(aDataSet: TDataSet; aValues, aDescs : TStrings; aValue, aDesc: String; AAll: Boolean=False; AAllDesc: String='');
var OldPoz : TBookmark;
    lValField,
    lDescField: TField;
begin
  aValues.Clear;
  aDescs.Clear;
  if AAll then begin
     AValues.Add('-1');
     aDescs.Add(AAllDesc);
  end;
  lValField := aDataSet.FindField(aValue);
  lDescField := aDataSet.FindField(aDesc);
  if not Assigned(lValField) then Exit;
  if not Assigned(lDescField) then lDescField := lValField;
  with aDataSet do begin
    OldPoz := GetBookmark;
    DisableControls;
    try
       First;
       while not Eof do begin
         aValues.Add(lValField.AsString);
         aDescs.Add(lDescField.AsString);
         Next;
       end;
    finally
       GotoBookmark(OldPoz);
       FreeBookmark(OldPoz);
       EnableControls;
    end;
  end;
end;

procedure TfrmData.DataModuleCreate(Sender: TObject);
var
  lPerioadaFiscala: Variant;
begin

  dbContabilitate.Properties.Add('workstation=' + CommonDBVar.GetHostName);
  if UpdateApp() then begin
    bIsCanceling := True;
    Exit;
  end;

  UpdWizInExecute   := False;
  UpdExecuteClasic  := True;
  FUpdateLocations  := TStringList.Create;
  FUpdateDescriptions := TStringList.Create;

  InitRegionalSettings;

  RegisterCRAdoParam('ID_GEST_DOCUM'  , IdGestDocum);
  RegisterCRAdoParam('ID_ANGAJAMENT'  , IdAngajament);
  RegisterCRAdoParam('ID_DISPOZITIE'  , IdDispozitie);
  RegisterCRAdoParam('ID_ORDONANTARE' , IdOrdonantare);
  RegisterCRAdoParam('COD_FUNCTIONAL' , ftString);
  RegisterCRAdoParam('COD_ECONOMIC'   , ftString);

  AppName := 'Contabilitate\';
  RegKeyPrefix := 'Software\ATS\'+AppName;
  AutoUpdate.LastURLEntry.Section := 'LastDate';
  AutoUpdate.LastURLEntry.Key     := RegKeyPrefix+'\LastUpdate';
  AutoUpdate.LastURLEntry.Save    := True;
  SetDBConnection(dbContabilitate, False);
  OpenDataModule(Self);
  DBInitGeneralParams(dbContabilitate);
  DBSetBlobSize(2 * 1024 * 1024);

  if bIsCanceling then Exit;
  ReadSQLSettings;
  lPerioadaFiscala := DBGetScallar('exec [spDateFiscale]');
  AnFiscal    := lPerioadaFiscala[0];
  MinDataFisc := lPerioadaFiscala[1];
  MaxDataFisc := lPerioadaFiscala[2];

  UpdStructure.DoUpdateStructure;

  gOnAfterNewForm := DoOnNewForm;

end;

function GetNextId(TblName: String): Integer;
begin
  Result := ValueSafeToInt( DBGetScallarFmt('exec SP_GET_NEXT_VALUE %s', [ValueToStr(TblName)], 0), -1 );
end;

function  GetTmpADOQuery: TZReadOnlyQuery;
begin
  Result := TZReadOnlyQuery(DBNewQuery());
end;

function TfrmData.GetNextCod: Integer;
begin
  Result := GetNextId('COD_CITEMS');
end;

function TfrmData.GetIdItems: Integer;
begin
  Result := GetNextId('CITEMS');
end;

procedure TfrmData.QryDefaStockNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('ID_GEST_DEFA_STOC').AsInteger := GetNextId('GEST_DEFA_STOC');
  DataSet.FieldByName('ID_GEST_TIP_STOC').AsInteger := QryTipStock.FieldByName('ID_GEST_TIP_STOC').AsInteger;
end;

procedure TfrmData.QryTipStockNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('ID_GEST_TIP_STOC').AsInteger := GetNextId('GEST_TIP_STOC');
end;

procedure TfrmData.QryDefaDocNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('ID_GEST_DEFA_DOCUM').AsInteger := GetNextId('GEST_DEFA_DOCUM');
end;

procedure TfrmData.QryRepartitoriNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('GESTINT').AsBoolean        := False;
  DataSet.FieldByName('ID_PARINTE').Clear;
end;

procedure TfrmData.QryFunctiuniNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('DENUMIRE').AsString    := 'Functie noua';
  DataSet.FieldByName('STARE').AsInteger      := 1;
end;

procedure TfrmData.QryOperatoriNewRecord(DataSet: TDataSet);
begin                                                                                                                      
  DataSet.FieldByName('STARE').AsInteger := 1;
  DataSet.FieldByName('DREPTURI').AsInteger := 0;
  DataSet.FieldByName('Nume').AsString := 'User';
  DataSet.FieldByName('NumeIntreg').AsString := 'Utilizator Nou';
end;

procedure TfrmData.dbContabilitateAfterConnect(Sender: TObject);
begin
  DBExecSQL(dbContabilitate,
    'SET CONCAT_NULL_YIELDS_NULL ON'#13#10+
    'SET ARITHABORT ON'#13#10+
    'SET ANSI_NULLS ON'#13#10+
    'SET ANSI_NULL_DFLT_ON ON'#13#10+
    'SET ANSI_PADDING ON'#13#10+
    'SET ANSI_WARNINGS ON'#13#10+
    'SET QUOTED_IDENTIFIER ON');

  if not DelphiRunning or TestParmsForString ('/force') then begin
    AutoUpdates(dbContabilitate);
    if UpdateApp(dbContabilitate) then begin
       bIsCanceling := True;
       Exit;
    end;
    TesteazaVersiune(ExeVersion);
  end;

end;

procedure TfrmData.dbContabilitateAfterReconnect(Sender: TObject);
begin
  dbContabilitate.ExecuteDirect(
    'SET CONCAT_NULL_YIELDS_NULL ON'#13#10+
    'SET ARITHABORT ON'#13#10+
    'SET ANSI_NULLS ON'#13#10+
    'SET ANSI_NULL_DFLT_ON ON'#13#10+
    'SET ANSI_PADDING ON'#13#10+
    'SET ANSI_WARNINGS ON'#13#10+
    'SET QUOTED_IDENTIFIER ON');
  dbContabilitate.ExecuteDirect(Format('exec [spPrgReconectare] %d, %d', [IdUtilizator, IdLogin]));
end;

procedure TfrmData.AutoUpdateAppRestart(Sender: TObject;
  var allow: Boolean);
begin
  Allow  := True;
  IsRestarting := True;
  bIsCanceling := True;
end;

procedure TfrmData.AutoUpdateCustomValidate(Sender: TObject; msg,
  param: String; var allow: Boolean);
begin
  if UpdWizInExecute then Allow := True
  else
    if Allow then
    Allow := MessageDlg('Exista o versiune noua a programului ATLAS!'#13#10'Doriti actualizarea versiunii curente?',
                       mtConfirmation, [mbYes, mbNo], 0) = mrYes;
end;

procedure TfrmData.QryPlanProiectNewRecord(DataSet: TDataSet);
begin
  Dataset.FieldByName('ID_PROIECTE').AsInteger := GetNextId('PROIECTE');
end;

procedure TfrmData.QryTipCheltVenNewRecord(DataSet: TDataSet);
var aId : Integer;
begin
  with DataSet do begin
     aId := GetNextId('TIPURI_CHELTVEN');
     FieldByName('ID_TIPURI_CHELTVEN').AsInteger := aId;
     FieldByName('DENUMIRE').AsString := 'Tip Nou';
     FID_Tip_CheltVen := aId;
  end;
end;

procedure TfrmData.QryPlanContAfterPost(DataSet: TDataSet);
begin
  DBRefresh(DataSet);
end;

procedure TfrmData.QryPlanContAfterDelete(DataSet: TDataSet);
begin
  DBRefresh(DataSet);
end;

function GetNextBulkId(TblName: String; Count : Integer; var MaxId : Integer): Integer;
var
  lValues: Variant;
begin
  lValues := DBGetScallarFmt('EXEC SP_GET_NEXT_BULK_VALUE %s, %d', [ValueToStr(TblName), Count]);
  if VarIsArray(lValues) then begin
    Result  := lValues[0];
    MaxId   := lValues[1];
  end
  else begin
    Result := 0;
    MaxId  := 0;
  end;
end;

procedure TesteazaVersiune(AVersion: String);
var
   lCurVer,
   lLocVer: TStringList;
   lStrLocVer: String;
   I : Integer;
begin
  if not DelphiRunning then begin
    if ValueHasValue( DBGetScallar('SELECT TOP 1 1 FROM SYSCOLUMNS WHERE ID = OBJECT_ID(''AUTO_UPDATES'') AND NAME LIKE ''MINIMUM_VERSION''') ) then begin
      lStrLocVer := ValueSafeToStr(DBGetScallar('SELECT TOP 1 MINIMUM_VERSION FROM AUTO_UPDATES ORDER BY ISNULL(MINIMUM_VERSION, '''') DESC'));
      if lStrLocVer > '' then begin
        lCurVer := TStringList.Create;
        lLocVer := TStringList.Create;
        try
          lCurVer.CommaText := StringReplace(lStrLocVer, '.', ',', [rfReplaceAll]);
          lLocVer.CommaText := StringReplace(AVersion, '.', ',', [rfReplaceAll]);
          if lCurVer.Count <> lLocVer.Count then Exit;
          for I := 0 to lCurVer.Count-1 do begin
            if lCurVer[I] > lLocVer[I] then begin
               { Fortam actualizarea versiunii stergand din registri cheia care contine ultima actualizare }
               with TRegistry.Create do
                 try
                    RootKey := HKEY_CURRENT_USER;
                    DeleteKey(FrmData.AutoUpdate.LastURLEntry.Key);
                 finally
                    Free;
                 end;
               bIsCanceling := True;
               raise EContaHandledError.Create('EROARE : Versiunea '+AVersion+' este mai mica decat versiunea minima acceptata de server : '+lStrLocVer+
                                      #13#10'Executia programului nu poat   e continua - la urmatoare pornire se va realiza actualizarea automata a executabilului !');
            end;
          end;
        finally
          lCurVer.Free;
          lLocVer.Free;
        end;
      end;
    end;
  end;
end;

procedure TfrmData.SetLocaleStr(Locale, LocaleType: Integer;
  const Value: String);
var
  Buffer: array[0..255] of Char;
  aPChar: PChar;
begin
  if length(Value) <= 0 then Exit;
  aPChar := @Value[1];
  ZeroMemory(@Buffer[0],255);
  Move(aPChar^, Buffer, Length(Value));
  if not SetLocaleInfo(Locale, LocaleType, Buffer) then
    RaiseLastOSError;
end;

procedure TfrmData.QryFunctionalNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('ID_PARINTE').Clear;
  if DataSet.FindField('TIP_REFLECTARE') <> nil then
    DataSet.FieldByName('TIP_REFLECTARE').AsInteger := 0;
end;

procedure TfrmData.QryBugetProiecteNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('DENUMIRE_PROIECT').AsString    := 'Proiect nou';
  DataSet.FieldByName('STARE').AsBoolean      := True;
end;

procedure TfrmData.QryBugetProiecteAfterInsert(DataSet: TDataSet);
var aState : Boolean;
begin
  if DataSet.FieldByName('ID_BUGET_PROIECTE').Value = null then begin
    aState := (DataSet.State in [dsEdit, dsInsert]);
    DataSet.Post;
    DataSet.Refresh;
    if aState then DataSet.Edit;
  end;
end;

procedure TfrmData.QryCursuriNewRecord(DataSet: TDataSet);
begin
  with DataSet do begin
    FieldByName('DENUMIRE').AsString := 'Curs Nou';
  end;
end;

procedure TfrmData.QryCursuriAfterInsert(DataSet: TDataSet);
begin
  if DataSet.State in [dsEdit, dsInsert] then begin
    DataSet.Post;
    DataSet.Edit;
  end;
end;

procedure TfrmData.RefreshUpdateLocations(DB: TZConnection);
var
  lUpdateLoca : TUpdateLocation;
  I           : Integer;
  lDataSet    : TDataSet;
begin
  for I := 0 to FUpdateLocations.Count-1 do
   TUpdateLocation(FUpdateLocations.Objects[I]).Free;
  FUpdateLocations.Clear;
  FUpdateLocations.Sorted := True;
  FUpdateLocations.Duplicates := dupAccept;
  FUpdateDescriptions.Clear;
  FUpdateDescriptions.Sorted := True;
  FUpdateDescriptions.Duplicates := dupAccept;
  DBExecSQL('SET CONCAT_NULL_YIELDS_NULL ON');
  lDataSet := DBNewQuery('SELECT * FROM AUTO_UPDATES WHERE STARE=1');
  try
    lDataSet.Open;
    while not lDataSet.Eof do begin
      lUpdateLoca := TUpdateLocation.Create;
      lUpdateLoca.UpdateType     := TWebUpdateType(lDataSet.FieldByName('UPDATE_TYPE').AsInteger);
      lUpdateLoca.AdresaCompleta := lDataSet.FieldByName('LOCATION').AsString;
      lUpdateLoca.Directory      := lDataSet.FieldByName('DIRECTORY').AsString;
      lUpdateLoca.UserName       := 'ats';
      lUpdateLoca.Password       := 'atlas';
      lUpdateLoca.IsEnabled      := (lDataSet.FieldByName('STARE').AsBoolean);
      if lUpdateLoca.UpdateType in [ftpUpdate, httpUpdate] then
        if (GetIPClassAddress(lUpdateLoca.AdresaCompleta) = GetIPClassAddress(IP_Addres)) then begin
          if lUpdateLoca.IsEnabled then begin
            FUpdateLocations.AddObject('1.' + lDataSet.FieldByName('DIRECTORY').AsString, lUpdateLoca);
            FUpdateDescriptions.Add('(1).' + lDataSet.FieldByName('DIRECTORY').AsString + Trim(lDataSet.FieldByName('LOCATION').AsString));
          end
          else begin
            FUpdateLocations.AddObject('4.' + lDataSet.FieldByName('DIRECTORY').AsString, lUpdateLoca);
            FUpdateDescriptions.Add('(4).' + lDataSet.FieldByName('DIRECTORY').AsString + Trim(lDataSet.FieldByName('LOCATION').AsString));
          end;
        end
        else begin
          FUpdateLocations.AddObject('2.' + lDataSet.FieldByName('DIRECTORY').AsString, lUpdateLoca);
          FUpdateDescriptions.Add('(2).' + lDataSet.FieldByName('DIRECTORY').AsString + Trim(lDataSet.FieldByName('LOCATION').AsString));
        end
      else begin
        FUpdateLocations.AddObject('0.' + lDataSet.FieldByName('DIRECTORY').AsString, lUpdateLoca);
        FUpdateDescriptions.Add('(0).' + lDataSet.FieldByName('DIRECTORY').AsString + Trim(lDataSet.FieldByName('LOCATION').AsString));
      end;
      lDataSet.Next;
    end;
  finally
    lDataSet.Free;
  end;
end;

//type THackConnection = class(TZAbstractConnection);

procedure TfrmData.DataModuleDestroy(Sender: TObject);
var
  I: Integer;
begin
  if FUpdateLocations <> nil then begin
    for I := 0 to FUpdateLocations.Count-1 do
      TUpdateLocation(FUpdateLocations.Objects[I]).Free;
    FUpdateLocations.Free;
    FUpdateDescriptions.Free;
  end;
end;

procedure TfrmData.DoOnNewForm(const className: String; formObj: TComponent);
var
  lDefaFontSize : Integer;

  procedure ReplaceTransparentComponent(AComponent: TComponent);
  var
    I             : Integer;
    lPropInfo     : PPropInfo;
    lPropObj      : TObject;
    lStyles       : TcxStyleRepository;
    lStyle        : TcxStyle;
  begin
    if AComponent is TcxStyleRepository then begin
      lStyles := TcxStyleRepository(AComponent);
      for I := 0 to lStyles.Count-1 do begin
        lStyle := TcxStyle(lStyles.Items[I]);
        if lStyle.Font.Size < lDefaFontSize then
          lStyle.Font.Size := lDefaFontSize;
      end;
    end;
    lPropInfo := GetPropInfo(AComponent, 'Transparent');
    if Assigned(lPropInfo) then
      SetOrdProp(AComponent, lPropInfo, Ord(True));
    lPropInfo := GetPropInfo(AComponent, 'Properties');
    if Assigned(lPropInfo) and (lPropInfo^.PropType^.Kind = tkClass) then begin
      lPropObj := TObject(GetOrdProp(AComponent, lPropInfo));
      if Assigned(lPropObj) then begin
        lPropInfo := GetPropInfo(lPropObj, 'Transparent');
        if Assigned(lPropInfo) then
          SetOrdProp(lPropObj, lPropInfo, Ord(True));
      end;
    end;
    for I := 0 to AComponent.ComponentCount-1 do
      ReplaceTransparentComponent(AComponent.Components[I]);
  end;

var
  lForm: TCustomForm;

begin
  if formObj.InheritsFrom(TCustomForm) then begin
    lForm := TCustomForm(formObj);
    lDefaFontSize := DBGetSetare('marimeFontCautare', lForm.Font.Size);
    lForm.DoubleBuffered  := True;
    ReplaceTransparentComponent(formObj);
  end;
  AddcxPopupComponent(formObj);
end;

function TfrmData.DoUpdateFromLocation(UpdateLocation: TUpdateLocation;
  Forced: Boolean): Boolean;


  function ExistsWizard(DataMod : TDataModule; anAutoUpdate : TWebUpdate) : TWebUpdateWizard;
  var I : Integer;
  begin
    Result := nil;
    if UpdExecuteClasic then Result := nil
    else
    for I := 0 to DataMod.ComponentCount - 1 do
      if (DataMod.Components[I] is TWebUpdateWizard) and (TWebUpdateWizard(DataMod.Components[I]).WebUpdate = anAutoUpdate) then begin
        Result := TWebUpdateWizard(DataMod.Components[I]);
        Break;
      end;
  end;

  var UpdateWiz : TWebUpdateWizard;


begin
   Result := False;
   if AutoUpdate.UpdateType = ftpUpdate then
     UpdateLocation.IsEnabled := PortTCP_IsOpen(AutoUpdate.Port, UpdateLocation.AdresaCompleta);
   if (UpdateLocation.IsEnabled) or (Forced) then begin
     AutoUpdate.UpdateType   := UpdateLocation.UpdateType;
     if AutoUpdate.UpdateType = ftpUpdate then
        AutoUpdate.Host      := UpdateLocation.AdresaCompleta
     else AutoUpdate.URL     := UpdateLocation.AdresaCompleta;
     //AutoUpdate.FTPPassive   := True;
     AutoUpdate.FTPDirectory := UpdateLocation.Directory;
     AutoUpdate.UserID       := UpdateLocation.UserName;
     AutoUpdate.Password     := UpdateLocation.Password;
     try
       UpdateWiz :=  ExistsWizard(Self, AutoUpdate);
       if UpdateWiz <> nil then begin
         UpdWizInExecute := True;
         UpdateWiz.Execute;
         UpdWizInExecute := False;
       end
       else AutoUpdate.DoUpdate;
       Result := True;
     except
     end;
   end;
end;

procedure TfrmData.AutoUpdates(DB: TZConnection);
var  I : Integer;
begin
  IsRestarting := False;
  RefreshUpdateLocations(DB);
  for I := 0 to FUpdateLocations.Count-1 do begin
     if (TUpdateLocation(FUpdateLocations.Objects[I]).UpdateType = ftpUpdate) and
         (FUpdateLocations[I] = '2') then Continue;
     DoUpdateFromLocation(TUpdateLocation(FUpdateLocations.Objects[I]));
     if IsRestarting then begin
         bIsCanceling := True;
         Break;
     end;
  end;
end;

procedure TfrmData.AutoUpdateStatus(Sender: TObject; statusstr: String;
  statuscode, errcode: Integer);
begin
  if errcode<>0 then
     IsUpdateError := True;
end;


procedure TfrmData.qryOIUnitatiAfterInsert(DataSet: TDataSet);
var aState : Boolean;
begin
  if DataSet.FieldByName('ID_OI_UNITATI').Value = null then begin
    aState := (DataSet.State in [dsEdit, dsInsert]);
    DataSet.Post;
    DataSet.Refresh;
    if aState then DataSet.Edit;
  end;
end;

procedure TfrmData.qryOIUnitatiTipuriNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('DENUMIRE').AsString    := '<Tip Nou>';
  DataSet.FieldByName('STARE').AsBoolean      := True;
end;

procedure TfrmData.qryOIUnitatiTipuriAfterInsert(DataSet: TDataSet);
var aState : Boolean;
begin
  if DataSet.FieldByName('ID_OI_UNITATI_TIPURI').Value = Null then begin
    aState := (DataSet.State in [dsEdit, dsInsert]);
    DataSet.Post;
    DataSet.Refresh;
    if aState then DataSet.Edit;
  end;
end;

procedure SetRapParam(aName : String; aValue : Variant);
begin
   SetCRAdoParamByName(aName, aValue);
end;

procedure TfrmData.qryOIUnitatiAfterOpen(DataSet: TDataSet);
begin
  DBExecSQL('exec spOIUnitatiVerifcaDetalii')
end;

procedure TfrmData.ReadSQLSettings;
var
  lField    : TField;
  lDataSet  : TDataSet;
begin
  lDataSet := DBNewQueryFmt('SELECT * FROM SETARI_UTILIZATOR WHERE ID_UTILIZATOR = %d', [-1]);
  try
    lDataSet.Open;
    if not lDataSet.IsEmpty then begin
      lField := lDataSet.FindField('RIGHT_ENABLED');
      if Assigned(lField) then
        IsRightEnable := ValueIsTrue(lField.Value);
      lField := lDataSet.FindField('SAVE_LOCATION');
      if Assigned(lField) then
        SaveINIToDB := ValueIsTrue(lField.Value);
      // Fortam stocarea setarilor in fisiere ini
      SaveINIToDB := False;
    end;
  finally
    lDataSet.Free;
  end;
  if DBProcExists('spPrgConfigEureka') then begin
    lDataSet := DBNewQuery('exec [spPrgConfigEureka]');
    try
      lDataSet.Open;
       eurekaemailSendMode  := lDataSet.FieldByName('emailSendMode').AsInteger;
       eurekaSmtpFrom       := lDataSet.FieldByName('SmtpFrom').AsString;
       eurekaReceiveAddress := lDataSet.FieldByName('ReceiveAddress').AsString;
       eurekaSubject        := lDataSet.FieldByName('Subject').AsString+ ' -> Contabilitate ver. '+ExeVersion + '(svnVer.: ' + svnRevision  + ') ' +' ['+szDBName+'/'+szServerName+']';
       eurekaBodyHeader     := lDataSet.FieldByName('BodyHeader').AsString;
       eurekaSmtpIp         := lDataSet.FieldByName('SmtpIp').AsString;
       eurekaSmtpPort       := lDataSet.FieldByName('SmtpPort').AsString;
       eurekaSmtpUserId     := lDataSet.FieldByName('SmtpUserId').AsString;
       eurekaSmtpPass       := lDataSet.FieldByName('SmtpPass').AsString;
    finally
      lDataSet.Free;
    end;
  end;
end;

end.
