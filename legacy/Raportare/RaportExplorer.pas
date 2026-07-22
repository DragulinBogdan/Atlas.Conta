unit RaportExplorer;

interface

uses
 Messages, SetParamsUnitADO, daDB, daADO, myChkBox, Windows, Classes, Controls, SysUtils,
  Forms, StdCtrls, ExtCtrls, Dialogs,
  daDatMan,
  Graphics, ppRichTx, ppViewr, DB, ppComm, ppCache, ppClass,
  ppProd, ppReport, ppRptExp, ppBands, ppDBBDE, ppEndUsr, ppDBPipe, ppDB, ppPrnabl, ppStrtch,
  ppDsgnDB, daDataModule, ppRelatv, ppModule, ppCTMain, raIDE, ppCTDsgn, ppTypes, ppTmplat,
  daDataView, ppChrt, ppChrtDP, ppChrtUI, ADODB, ppCtrls, OpenProgressUnit,
  dxCntner, dxTL, dxDBCtrl, dxDBTL, dxExEdtr, ppFormWrapper, idTCPClient,
  TXRB, TXComp, ppParameter, ppDesignLayer;

const
  WM_SEND_REPORT_TO_REPOSITORY = WM_USER + 1;
  WM_MUST_CLOSE_REPORT         = WM_USER + 2;

type

  TCustomReport = class(TForm)
    ppParameterList1: TppParameterList;
    dsTable: TDataSource;
    plTable: TppBDEPipeline;
    dsField: TDataSource;
    plField: TppBDEPipeline;
    RapDictionary: TppDataDictionary;
    RapDesigner: TppDesigner;
    dsItem: TDataSource;
    plItem: TppBDEPipeline;
    ppRaport: TppReport;
    dsFolder: TDataSource;
    plFolder: TppBDEPipeline;
    RapExplorer: TppReportExplorer;
    ppHeaderBand1: TppHeaderBand;
    ppDetailBand1: TppDetailBand;
    ppFooterBand1: TppFooterBand;
    dsJoin: TDataSource;
    plJoin: TppBDEPipeline;
    qryItem: TADOQuery;
    QryFolder: TADOQuery;
    QryTable: TADOQuery;
    QryField: TADOQuery;
    QryJoin: TADOQuery;
    QryItems: TADOQuery;
    QryItemsitem_id: TAutoIncField;
    QryItemsfolder_id: TIntegerField;
    QryItemsitem_name: TStringField;
    QryItemsitem_size: TIntegerField;
    QryItemsitem_type: TIntegerField;
    QryItemsmodified: TFloatField;
    QryItemsdeleted: TFloatField;
    QryItemstemplate: TBlobField;
    RapDevices: TExtraOptions;
    DbRaportare: TADOConnection;
    TreePlanConturi: TdxDBTreeList;
    TreePlanConturiROMANA: TdxDBTreeListMaskColumn;
    TreePlanConturiCONT: TdxDBTreeListMaskColumn;
    TreeEconomic: TdxDBTreeList;
    TreeEconomicCOD_BUGET: TdxDBTreeListMaskColumn;
    TreeEconomicDENUMIRE: TdxDBTreeListMaskColumn;
    TreeFunctional: TdxDBTreeList;
    TreeFunctionalCOD_BUGET: TdxDBTreeListMaskColumn;
    TreeFunctionalDENUMIRE: TdxDBTreeListMaskColumn;
    TreeRepartitori: TdxDBTreeList;
    TreeRepartitoriNUME: TdxDBTreeListMaskColumn;
    TreeRepartitoriADRESA: TdxDBTreeListMaskColumn;
    TreeRepartitoriCODREP: TdxDBTreeListMaskColumn;
    TreeRepartitoriCONT: TdxDBTreeListMaskColumn;
    ppAntet: TppBDEPipeline;
    TreeProiecte: TdxDBTreeList;
    TreeProiecteID_OI_PROIECTE: TdxDBTreeListMaskColumn;
    TreeProiecteDENUMIRE: TdxDBTreeListMaskColumn;
    TreeUnitati: TdxDBTreeList;
    TreeUnitatiID_OI_UNITATI: TdxDBTreeListMaskColumn;
    TreeUnitatiDENUMIRE: TdxDBTreeListMaskColumn;
    TreeUnitatiCOD_FUNCTIONAL: TdxDBTreeListColumn;
    procedure FormCreate(Sender: TObject);
    procedure ppRaportPreviewFormCreate(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure ppRaportPrintingComplete(Sender: TObject);
    procedure DbRaportareExecuteComplete(Connection: TADOConnection;
      RecordsAffected: Integer; const Error: Error;
      var EventStatus: TEventStatus; const Command: _Command;
      const Recordset: _Recordset);
    procedure TreePlanConturiDblClick(Sender: TObject);
    procedure TreePlanConturiKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure ppRaportBeforeOpenDataPipelines(Sender: TObject);
    procedure ppRaportBeforeAutoSearchDialogCreate(Sender: TObject);
    procedure ppRaportAfterOpenDataPipelines(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure RapExplorerClose(Sender: TObject; var Action: TCloseAction);
    procedure RapDesignerClose(Sender: TObject; var Action: TCloseAction);
  private
    FAbortForm: TForm;
    FIsTextBased: Boolean;
    QryAborted: Boolean;
    FDirectPrint: Boolean;
    FReportID: Integer;
    FPDFStream : TMemoryStream;
    FOriginalID: Integer;
    FShowFooter  : Boolean;
    FPreviewFormStyle: TFormStyle;
    FInExploreMode: Boolean;
    function  GetCanShowAbort: Boolean;
    procedure LoadEndEvent(Sender: TObject);
    procedure LoadStartTemplate(Sender: TObject; aStream: TStream);
    procedure ppRaportDialogCreate(Sender: TObject);
    procedure ppRaportAfterPrint(Sender: TObject);
    procedure PreviewFormCreateEvent(Sender: TObject);
    procedure PreviewFormCloseEvent(Sender: TObject);
    procedure OpenAssincronQuery(aData: TDataSet;aDataSource: TDataSource; Info: String);
    procedure DoAbort(Sender: TObject);
    procedure SendToRepository(Sender: TObject);
  protected
    procedure WmSendReportToRepository(var Message: TMessage); message WM_SEND_REPORT_TO_REPOSITORY;
    procedure WmMustCloseReport(var Message: TMessage); message WM_MUST_CLOSE_REPORT;
    procedure GetParameterValues(Sender: TObject; var Value: Variant);
  protected
    procedure LocalReceiveStream(Sender: TObject; AStream: TStream);
  public
    FCanShowAbort: Boolean;
    FCurentReportFile: String;
    FNoOnLoadEndEvent: Boolean;
    IsAutoClose : Boolean;
    procedure SetParamList;
    procedure ClearParamList(MustClose: Boolean = False);
    procedure RefreshParams();
    procedure FillDataViews(List: TList);
    procedure ppDirectToPDFDialogCreate(Sender: TObject);
    procedure ShowAbort(aCaption: String);
    procedure HideAbort;
    procedure LoadReportEx(ID: Integer);
    procedure LoadReport(ID: Integer; DirectToPrinter: Boolean=False; AutoClose: Boolean =False);
    procedure SetQuoted;
    function  OpenDataSets: Boolean;
    function  GetADOTmpQuery: TADOQuery;
    function  GetReportDataSet(AutoOpen: Boolean=True): TDataSet;
    property  DirectPrint: Boolean read FDirectPrint write FDirectPrint;
    property  ReportID : Integer read FReportID write FReportID;
    property  OriginalID: Integer read FOriginalID write FOriginalID;
    property  CanShowAbort : Boolean read GetCanShowAbort;
    property  PreviewFormStyle : TFormStyle read FPreviewFormStyle write FPreviewFormStyle;
    property  InExploreMode : Boolean read FInExploreMode write FInExploreMode stored False; 
  end;

type TAsyncADOThread = class (TThread)
  private
    FRelaQry: TADOQuery;
    FIsError: Boolean;
    {$IFDEF CLONE_ADO_DATASET}
    lConn   : TADOConnection;
    lQuery  : TADOQuery;
    {$ENDIF}
    FErrorMessage: String;
  protected
    {$IFDEF CLONE_ADO_DATASET}
    procedure Finalizare;
    {$ENDIF}
  public
    constructor Create(aADOQuery: TADOQuery); virtual;
    destructor Destroy; override;
    procedure Execute; override;
    property  IsError: Boolean read FIsError write FIsError;
    property  ErrorMessage: String read FErrorMessage write FErrorMessage;
  end;

type TTipTiparire = (ttText, ttGraphic);

procedure OpenAsynADOQuery(aQry: TADOQuery);

procedure LoadReport(aRepId: Integer; IsTextBased: Boolean=False;NrCopii: Integer=1; DirectToPrinter: Boolean=False; AutoClose: Boolean = False);
function  LoadReportEx(aRepId: Integer;NoOnLoadEnd: Boolean=False): TppReport;
function  FindGlobalComponent(const Name: string): TComponent;
procedure LocalDefaultHeader(Sender: TObject);

function SetCurentParams(aDataView: TdaCustomDataView; var MustClose: Boolean): Boolean;
function SetCurentParam(aQry: TObject; var MustClose: Boolean): Boolean;
function GetFrmProgress: TFrmOpenProgress;

procedure InitCacheEngine;
procedure ClearCacheEngine;
procedure UninitCacheEngine;

function GetBaseFileName: String;

procedure WriteReportToRepository(IdReport: Integer; Desc: String; AID: Integer);
procedure InternalGetStream(Sender: TObject; AStream: TStream);

procedure OpenDataPipeLines(DataView: TdaCustomDataView);

const
   AsyncOpen: Boolean = False;
   ShowAlert: Boolean = False;
   ReportHandled: Boolean = False;
   DefaultPort : String = 'LPT1';
   cst_EndReportLines: Integer = 0;
   cst_EndCopieLines : Integer = 3;
   IsVisibleProgress : Boolean = True;
   cst_DefaultShowFooter : Boolean = True;

var
   SConnection    : String;
   aFrmProgress   : TFrmOpenProgress;
   FCurrentThread : TAsyncADOThread;
   FCacheReports  : TStringList;
   TipTiparire    : TTipTiparire;
   TransferRaport   : Boolean = False;
   TransferMetaData : Boolean = False;
   RepositoryActive : Boolean = False;
   lDocRepositoryConnection: TidTCPClient;
   lDocSQLServerConnection : TADOConnection;
   lDocMetaSrvName : String;
   lDocMetaSrvDB   : String;
   lDocRepSrvName : String;
   lDocRepSrvPort : Integer;

implementation

{$R *.DFM}

uses FormulareUnit, ppFilDev, Variants, DirectPDFUnit, FunctiiUtilizator,
     CommonDBVar, DateUnit, ppClasUt, ppUtils, ppVar, IniFiles,
  IdTCPConnection, ATSZDBUtils;

type TCrackFooterBand = class(TppFooterBand);

procedure LocalDefaultFooter(Sender: TppFooterBand);
var OldHeight : Double;
    lLine : TppLine;
    lLabel: TppLabel;
    lPageNo: TppSystemVariable;
    lParameter: TppVariable;
begin
  if (not Assigned(Sender)) or
     (Assigned(Sender.Report.PrinterSetup) and (pos('5', Sender.Report.PrinterSetup.PaperName) > 0)) then Exit;
  { Decat prima                             data }
  if (Sender.Tag = 0) then begin
     if not Sender.Visible then begin
        Sender.Visible := True;
        Sender.Height  := 2 * 0.2708;
        OldHeight      := 0;
     end
     else begin
        OldHeight     := Sender.Height;
        Sender.Height := Sender.Height + 2 * 0.2708;
     end;
     if Sender.PrintHeight = phStatic then
        Sender.PrintHeight := phDynamic;
     if not Sender.PrintOnFirstPage then
        Sender.PrintOnFirstPage := True;
     if not Sender.PrintOnLastPage then
        Sender.PrintOnLastPage := True;
     lLine := TppLine.Create(Sender);
     with lLine do begin
        Band := Sender;
        Name := 'ppLineFooterDesc';
        UserName := 'LineFooterDesc';
        with Font do begin
          Name  := 'Arial';
          Size  := 8;
          Style := [fsBold];
          Color := clGrayText;
        end;
        Top      := OldHeight + 0.0208;
        Height   := 0.1562999934;
        ParentWidth := True;
     end;
     lLabel := TppLabel.Create(Sender);
     with lLabel do begin
        Band := Sender;
        with Font do begin
          Name  := 'Arial';
          Size  := 8;
          Style := [fsBold];
          Color := clGrayText;
        end;
        Name := 'ppLabelFooterDesc';
        UserName := 'LabelFooterDesc';
        Left     := 0.0625;
        Top      := OldHeight + 0.0521;
        Caption  := Sender.Report.Template.DatabaseSettings.Name + ' ('+IntToStr(TCustomReport(Sender.Report.Owner).ReportID)+')';
        AutoSize := True;
     end;
     lPageNo := TppSystemVariable.Create(Sender);
     with lPageNo do begin
        Band := Sender;
        with Font do begin
          Name  := 'Arial';
          Size  := 8;
          Style := [fsBold];
          Color := clGrayText;
        end;
        Alignment := taCenter;
        Name := 'ppLabelPageNoeDesc';
        UserName := 'ppLabelPageNoeDesc';
        Left     := lLine.Width / 2;
        Top      := OldHeight + 0.0521;
        VarType  := vtPageSetDesc;
        AutoSize := True;
     end;
     lParameter := TppVariable.Create(Sender);
     with lParameter do begin
        Band := Sender;
        with Font do begin
          Name  := 'Arial';
          Size  := 8;
          Style := [fsBold];
          Color := clGrayText;
        end;
        Name := 'ppLabelParamDesc';
        UserName := 'ppLabelParamDesc';
        WordWrap := True;
        Left     := 0.0625;
        Top      := OldHeight + 2 * 0.0521 + lLabel.Height;
        Height   := lPageNo.Height * 2;
        Width    := lLine.Width - Left * 2;
        OnCalc   := TCustomReport(Sender.Report.Owner).GetParameterValues;
     end;
     lLabel := TppLabel.Create(Sender);
     with lLabel do begin
        Band := Sender;
        with Font do begin
          Name  := 'Arial';
          Size  := 8;
          Style := [fsBold];
          Color := clGrayText;
        end;
        Alignment := taRightJustify;
        Name := 'ppLabelUserDesc';
        UserName := 'ppLabelUserDesc';
        Top      := OldHeight + 0.0521;
        Caption  := NumeLoginComplet + ' / ' + FormatDateTime('dd.mm.yyyy hh:nn:ss', Now);
        AutoSize := True;
        Left     := lLine.Width - lLabel.Width - 0.0625;
     end;
     Sender.Tag := 1;
  end;
end;

procedure WriteReportToRepository(IdReport: Integer; Desc: String; AID: Integer);
var
  lDevice: TPDFDevice;
  lCustomRep: TCustomReport;
begin
  if not RepositoryActive then Exit;
  lCustomRep := TCustomReport(LoadReportEx(IdReport, True).Owner);
  with lCustomRep do
    try
      lDevice := TPDFDevice.Create(nil);
      try
        FCanShowAbort := False;
        OriginalID   := AID;
        ppRaport.ShowCancelDialog    := False;
        ppRaport.ShowPrintDialog     := False;
        ppRaport.AfterPrint          := nil;
        ppRaport.PrinterSetup.Copies := 1;
        SetParamList;
        lDevice.Publisher    := ppRaport.Publisher;
        lDevice.OutputStream := lCustomRep.FPDFStream;
        if ppRaport.InitializeParameters then begin
          ppRaport.PrintToDevices;
          lCustomRep.SendToRepository(lCustomRep);
        end;
      finally
        lDevice.Free;
      end;
    finally
       lCustomRep.Close;
    end;
end;

function GetBaseFileName: String;
var
  y, m, d : Word;
begin
  DecodeDate(Date, y, m, d);
  Result := 'CONTA\Y_'+IntToStr(y)+'\M_'+IntToStr(m)+'\D_'+IntToStr(d)+'\';
end;

function DataConnectionString : String;
begin
  Result := 'Provider=SQLOLEDB.1;Password='+GetPassword+';Persist Security Info=True;User ID=ATSUserPrivilegiat;Initial Catalog='+szDBName+';Data Source='+szServerName;
end;

function DocSQLServerConnection: TADOConnection;
begin
  if lDocSQLServerConnection = nil then begin
    lDocSQLServerConnection := TADOConnection.Create(nil);
    with lDocSQLServerConnection do begin
      LoginPrompt := False;
      ConnectionString := 'Provider=SQLOLEDB.1;Password='+GetPassword+';Persist Security Info=True;User ID=ATSUserPrivilegiat;Initial Catalog='+lDocMetaSrvDB+';Data Source='+lDocMetaSrvName;
      try
        Connected := True;
      except
        TransferMetaData := False;
      end;
    end;
  end;
  Result := lDocSQLServerConnection;
end;

function DocRepositoryConnection: TIdTCPClient;
begin
  if lDocRepositoryConnection = nil then begin
     lDocRepositoryConnection := TIdTCPClient.Create(nil);
     with lDocRepositoryConnection do begin
        Host := lDocRepSrvName;
        Port := lDocRepSrvPort;
     end;
  end;
  Result := lDocRepositoryConnection;
  if not Result.Connected then Result.Connect();
end;

procedure SendToDocumentManagement(CustomReport: TCustomReport; Stream: TStream; Descryption: String; FileName: String; OriginalID: Integer);
var
  lLogID    : Integer;
  TipDocument,
  lNrDoc   : Integer;
  DocName,
  lDataDoc : String;
  lDirName, lFileName : String;
  lConn : TADOConnection;
begin

  { Scriem In Baza De Date Raportul Care S-a Tiparit }
  with CustomReport.GetADOTmpQuery do
    try
       Sql.Add('INSERT INTO LOG_RAPOARTE_EMISE (ID_UTILIZATORI, ITEM_ID, ITEM_NAME, MOMENT, REPOSITORY_PATH, ORIGINAL_ID, EXTENSION)');
       Sql.Add('SELECT '+IntToStr(IdUtilizator)+', '+IntToStr(CustomReport.ReportID)+', ''Raport'', GETDATE(), :REP_PATH, :ORIGINAL_ID, :EXTENSION');
       Sql.Add('SELECT IDENT_CURRENT(''LOG_RAPOARTE_EMISE'')');
       //Parameters[0].LoadFromStream(nil, ftBlob);
       Parameters[0].Value := lFileName;     // Repository Path
       Parameters[1].Value := OriginalID;    // Original ID
       Parameters[2].Value := '.pdf';        // Extension
       Open;
       lLogID := Fields[0].AsInteger;
       Close;
       lFileName := GetBaseFileName+'REP_'+IntToStr(CustomReport.ReportID)+'\ACT_'+IntToStr(lLogID);
       Sql.Clear;
       Sql.Add('UPDATE LOG_RAPOARTE_EMISE SET REPOSITORY_PATH = '+QuotedStr(lFileName)+' WHERE ID_LOG_RAPOARTE_EMISE = '+IntToStr(lLogID));
       ExecSql;
    finally
       Free;
    end;

  { Transmitem fisierul in repository }
  if RepositoryActive then
    with DocRepositoryConnection do begin
      if Connected then Disconnect;
      ConnectTimeout := 300;
      Connect;
      IOHandler.WriteLn('user:Contabilitate');
      IOHandler.WriteLn('parola:Atlas');
      IOHandler.WriteLn('post:'+lFileName);
      IOHandler.Write(Stream, 0, True);
      Disconnect;
    end;

  if TransferMetaData then begin
    TipDocument := 296;
    DocName     := 'Contabilitate';
    Descryption := 'RAPORT CONTABILITATE - Versiune PDF';
    { Citim calea catre fisier din procedura stocata din baza de date }
    lConn := DocSQLServerConnection;

    with TADOQuery.Create(nil) do
      try
        Connection := lConn;
        Sql.Add('EXEC SP_TAXE_ADD_CERERE '+QuotedStr(DocName)+', '+IntToStr(TipDocument)+', '+QuotedStr(Descryption));
        try
          Open;
          lDirName  := FieldByName('FOLDER').AsString;
          lFileName := FieldByName('FISIER').AsString;
          lNrDoc    := FieldByName('NR_INTRARE').AsInteger;
          lDataDoc  := FieldByName('DATA_INTRARE').AsString;
        except
          TransferMetaData := False;
           Exit;
        end;
      finally
        Free;
      end;

    if RepositoryActive then
      with DocRepositoryConnection do begin
      if Connected then Disconnect;
      ConnectTimeout := 300;
      Connect;
      IOHandler.WriteLn('user:Contabilitate');
      IOHandler.WriteLn('parola:Atlas');
      IOHandler.WriteLn('post:'+lDirName+lFileName);
      IOHandler.Write(Stream, 0, True);
      Disconnect;
    end;
  end;

  if ShowAlert then
    MessageDlg('Documentul a fost transmis cu succes in modulul de Document Management'#13#10'Numar inregistrare '+IntToStr(lNrDoc)+
               ' din data '+lDataDoc, mtInformation, [mbOk], 0);

end;

function PrintTextToPrn(const aFileName: String;const NrCopii:Integer=1;const aPortName: String='LPT1'): Boolean;
var aListFile: TStringList;
    PrnFile: TextFile;
    I, J, Final : Integer;
    Port: String;
    aIOResult : Integer;
begin
  if aPortName = '' then
     Port := DefaultPort
  else Port := aPortName;
  Result := False;
  aListFile := TStringList.Create;
  try
     aListFile.LoadFromFile(aFileName);
     { Aflam unde se termina Raportul }
     Final := aListFile.Count-1;
     while (Final >= 0) and (Trim(aListFile[Final])='') do
       Dec(Final);
     { Avem de tiparit }
     if Final > -1 then begin
        AssignFile(PrnFile, Port);
        {$I-}
        Rewrite(PrnFile);
        {$I+}
        aIOResult := IOResult;
        if aIOResult = 0 then begin
           Write(PrnFile, #$1b#$40);
           Write(PrnFile, #$1b#$12); // 10 CPI
           Write(PrnFile, #$1b#$0f); // Secventa ESCAPE pentru Condensat
           for I := 1 to NrCopii do begin
             for J := 0 to Final do
                WriteLn(PrnFile, aListFile[J]);
             if I < NrCopii then
                Write(PrnFile, cst_EndCopieLines);
             end;
           Write(PrnFile, #12);
           Write(PrnFile, cst_EndReportLines);
           CloseFile(PrnFile);
        end
        else raise EContaHandledError.Create('Eroare la accesarea fisierului : '+IntToStr(aIOResult));
     end;
     Result := True;
  finally
     aListFile.Free;
  end;
end;

procedure FreeAndNil(var Obj);
var
  Temp: TObject;
begin
  Temp := TObject(Obj);
  Pointer(Obj) := nil;
  Temp.Free;
end;

function  GetFrmProgress: TFrmOpenProgress;
begin
  if aFrmProgress = nil then
     aFrmProgress := TFrmOpenProgress.Create(nil);
  Result := aFrmProgress;
end;

procedure OpenAsynADOQuery(aQry: TADOQuery);
begin
  if aQry.Active then Exit;

  if AsyncOpen then
     aQry.ExecuteOptions := aQry.ExecuteOptions + [eoAsyncExecute]
  else aQry.ExecuteOptions := aQry.ExecuteOptions - [eoAsyncExecute];
  FCurrentThread := TAsyncADOThread.Create(aQry);
  with FCurrentThread do
    try
      GetFrmProgress.DataStart := Time;
      GetFrmProgress.CurentThread := FCurrentThread;
      Resume;
      while (not Terminated) and (not GetFrmProgress.IsCanceled)
            and (not IsError) do
        GetFrmProgress.StepIt;
        if IsError then
          raise EContaHandledError.Create(ErrorMessage);
        if GetFrmProgress.IsCanceled then
           TerminateThread(FCurrentThread.Handle, 0);
    finally
      FreeAndNil(FCurrentThread);
    end;
end;

procedure LocalDefaultHeader(Sender: TObject);
var aBand: TppTitleBand;
    aRichText: TppRichText;
begin
  Exit;
  aBand := TppTitleBand(Sender);
  if (aBand = nil) or (aBand.Report = nil) then Exit;
  aRichText := TppRichText(ppComponentCreate(aBand.Report, TppRichText));
  with aRichText do begin
    Band      := aBand;
    spLeft    := 1;
    spTop     := 0;
    spWidth   := 100;
    spHeight  := 50;
    Stretch   := True;
    ParentHeight := True;
    ParentWidth  := True;
{    RichText  := 'Teste de incercare !';}
    Transparent := True;
  end;
  { Incarcam Header-ul Default }
end;

procedure TCustomReport.GetParameterValues(Sender: TObject;
  var Value: Variant);
var I, J, K: Integer;
    Qry: TADOQuery;
    lStr : String;
    lResult : String;
    lParamUnique: TStringList;
begin
  lResult := '';
  lParamUnique := TStringList.Create;
  try
    lParamUnique.Sorted := True;
    lParamUnique.Duplicates := dupIgnore;
    { Stergem Parameterii in cazul in care exista }
    for I := 0 to Self.ComponentCount-1 do
      if Self.Components[I] is TdaCustomDataView then
         with TdaCustomDataView(Self.Components[I]) do
           for J := 0 to DataPipelineCount-1 do begin
             if (not (DataPipeLines[J] is TppChildBDEPipeline)) or
                (TppChildBDEPipeLine(DataPipeLines[J]).DataSource = nil) or
                (TppChildBDEPipeLine(DataPipeLines[J]).DataSource.DataSet = nil) or
                (not (TppChildBDEPipeLine(DataPipeLines[J]).DataSource.DataSet is TADOQuery)) then Continue;
             Qry := TADOQuery(TppChildBDEPipeLine(DataPipeLines[J]).DataSource.DataSet);
             for K := 0 to Qry.Parameters.Count-1 do begin
               if lParamUnique.IndexOf(Qry.Parameters[K].Name) = -1 then begin
                 lStr := Trim(VarToStr(Qry.Parameters[K].Value));
                 if lStr > '' then
                    if lResult > '' then lResult := lResult + ', '+ Qry.Parameters[K].Name+'='+lStr
                    else lResult := Qry.Parameters[K].Name+'='+lStr;
                 lParamUnique.Add(Qry.Parameters[K].Name);
               end;
             end;
           end;
  finally
    lParamUnique.Free;
  end;
  Value := lResult;
end;

procedure OpenDataPipeLines(DataView: TdaCustomDataView);
var I: Integer;
    aDataSet  : TDataSet;
    aQry      : TADOQuery;
    aRepo     : TCustomReport;
    aDataView : TdaADOQueryDataView;
begin
  if ReportHandled then Exit;
  { Initializam Optinerea datelor asincrone }
  if (DataView.Owner <> nil) and (DataView.Owner is TCustomReport) then
    aRepo := TCustomReport(DataView.Owner)
  else aRepo := nil;

  aDataView := TdaADOQueryDataView(DataView);

  if (not Assigned(aRepo)) {or (not aRepo.ppRaport.Printing)} then Exit;

  if aRepo.CanShowAbort then
    with GetFrmProgress do begin
       { Incercam Sa luam numele Raportului }
       Caption := aRepo.ppRaport.Template.DatabaseSettings.Name;
       SetTotalCount(aDataView.DataPipeLineCount*50);
       Active := IsVisibleProgress;
       if Active then Show;
    end;
    
  with aDataView do
  try
    for I := 0 to DataPipelineCount - 1 do begin
      if aRepo.CanShowAbort then
         GetFrmProgress.SetTotalPos((I+1)*50, DataPipelines[I].UserName);
      if (TppChildBDEPipeLine(DataPipelines[I]).DataSource <> nil) and
         (TppChildBDEPipeLine(DataPipelines[I]).DataSource.DataSet <> nil) then begin
         aDataSet := TppChildBDEPipeLine(DataPipelines[I]).DataSource.DataSet;
         if aDataSet.Active then Continue;
         if aDataSet is TADOQuery then begin
            aQry := TADOQuery(aDataSet);
            if aQry.Connection = nil then
               aQry.Connection := TCustomReport(aDataView.Owner).DbRaportare;
            { Trecem pe executie asyncrona }
            try
               if aRepo.CanShowAbort then begin
                 OpenAsynADOQuery(aQry);
                 if (GetFrmProgress.IsCanceled) and (not aQry.Active) then begin
                    {if aQry.RecordSet <> nil then
                       aQry.Recordset.Cancel;}
                    raise EContaHandledError.Create('S-a abandonat executia raportului !');
                 end;
               end
               else aQry.Active := True;
            finally
               aQry.AfterOpen := nil;
            end;
         end else aDataSet.Open;
      end;
    end;
  finally
    FreeAndNil(aFrmProgress);
  end;
end;

function SetCurentParams(aDataView: TdaCustomDataView; var MustClose: Boolean): Boolean;
var I: Integer;
begin
  if ReportHandled then begin Result := True; MustClose := False; Exit; end;
  Result := False;
  MustClose := False;
  with aDataView do
    for I := 0 to DataPipelineCount-1 do begin
      MustClose := (not SetCurentParam(DataPipeLines[I], MustClose)) or (MustClose);
      if MustClose then Break;
    end;
  Result := (Result) or (not MustClose);
  if Result then OpenDataPipeLines(aDataView);
end;

function SetCurentParam(aQry: TObject; var MustClose: Boolean): Boolean;
var Qry: TADOQuery;
    aRep: TCustomReport;

begin
  MustClose := False;
  Result := True;
  if (not (aQry is TppChildBDEPipeline)) or
     (TppChildBDEPipeLine(aQry).Active) or
     (TppChildBDEPipeLine(aQry).DataSource = nil) or
     (TppChildBDEPipeLine(aQry).DataSource.DataSet = nil) or
     (not (TppChildBDEPipeLine(aQry).DataSource.DataSet is TADOQuery)) then Exit;
  Qry := TADOQuery(TppChildBDEPipeLine(aQry).DataSource.DataSet);

  Result := SetMsSqlParams(Qry, False, False);
  if not Result then begin
    aRep := TCustomReport(TppChildBDEPipeLine(aQry).Owner);
    if (aRep <> nil) and (aRep.ppRaport <> nil) and (aRep.ppRaport.PreviewForm <> nil) then begin
       aRep.ppRaport.PreviewForm.ppOnActivate := nil;
       MustClose := (MustClose) or (True);
    end;
  end;
end;

function FindGlobalComponent(const Name: string): TComponent;
var
  I: Integer;
begin
  for I := 0 to Screen.FormCount - 1 do
  begin
    Result := Screen.Forms[I];
    if CompareText(Name, Result.Name) = 0 then Exit;
  end;
  for I := 0 to Screen.DataModuleCount - 1 do
  begin
    Result := Screen.DataModules[I];
    if CompareText(Name, Result.Name) = 0 then Exit;
  end;
  Result := nil;
end;

function LoadReportEx(aRepId: Integer;NoOnLoadEnd: Boolean=False): TppReport;
begin
  with TCustomReport.Create(nil) do
   try
      FNoOnLoadEndEvent := NoOnLoadEnd;
      LoadReportEx(aRepId);
      Result := ppRaport;
   except
      on E: Exception do begin
         Free;
         raise EContaHandledError.Create('Eroare : '+E.Message);
      end;
   end;
end;

procedure LoadReport(aRepId: Integer; IsTextBased: Boolean=False;NrCopii: Integer=1; DirectToPrinter: Boolean=False; AutoClose: Boolean=False);
begin
  if aRepId  = -1 then Exit;
  with TCustomReport.Create(nil) do
    try
       if IsTextBased then
          FIsTextBased := True
       else
          FIsTextBased := TipTiparire = ttText;
       ppRaport.PrinterSetup.Copies := NrCopii;
       LoadReport(aRepId, DirectToPrinter, AutoClose);
    finally
       if ppRaport.ModalPreview then
          Free;
    end;
end;

procedure TCustomReport.OpenAssincronQuery(aData: TDataSet;aDataSource: TDataSource;Info: String);
{var AreEroare : Boolean;
    ErrMsg    : String;}
begin
  Application.ProcessMessages;
  aData.Active := True;
  aDataSource.DataSet := aData;
{  AreEroare := False;
  with TAsincronQueryThread.Create(DbRaportare,aData,aDataSource) do
    try
       ShowAbort(Info);
       while (not Terminated) and (not QryAborted) do
         Application.ProcessMessages;
       AreEroare := AreEroare or HaveErrors;
       ErrMsg    := ErrorMsg;
    finally
       HideAbort;
       TerminateThread(Handle,0);
       Free;
    end;
    if AreEroare then
       raise EContaHandledError.Create(ErrMsg);
    if QryAborted then
       raise EContaHandledError.Create('Operatia a fost abandonata !');}
end;

procedure TCustomReport.FormCreate(Sender: TObject);
begin

  FInExploreMode := False;
  
  FPreviewFormStyle := fsMdiChild;

  RapDesigner.IniStorageName := IncludeTrailingPathDelimiter(ExtractFileDir(Application.ExeName)) + 'RBuilder.Ini';

  FDirectSetParams := False;

  FShowFooter := False;

  FCanShowAbort := True;

  FPDFStream := TMemoryStream.Create;

  RegisterCRAdoParam('COD_INSPECTOR', IdUtilizator);
  RegisterCRAdoParam('ID_UTILIZATORI', IdUtilizator);

  with RegisterCRAdoParam('CONT', ftString) do
    DropDownControl := TreePlanConturi;

  with RegisterCRAdoParam('COD_FUNCTIONAL', ftString) do
    DropDownControl := TreeFunctional;

  with RegisterCRAdoParam('COD_ECONOMIC', ftString) do
    DropDownControl := TreeEconomic;

  with RegisterCRAdoParam('COD_PROIECT', ftString) do
    DropDownControl := TreeProiecte;

  with RegisterCRAdoParam('COD_UNITATE', ftString) do
    DropDownControl := TreeUnitati;


  with RegisterCRAdoParam('ID_REPARTITOR_I', ftInteger) do begin
   // FrmData.QryRepartitori.Filter := 'GESTINT = True';
   // FrmData.QryRepartitori.Filtered := True;
    DropDownControl := TreeRepartitori;
  end;

  with RegisterCRAdoParam('ID_REPARTITOR_E', ftInteger) do begin
   // FrmData.QryRepartitori.Filter := 'GESTINT = False';
   // FrmData.QryRepartitori.Filtered := True;
    DropDownControl := TreeRepartitori;
  end;

  with RegisterCRAdoParam('ID_REPARTITOR', ftInteger) do begin
   // FrmData.QryRepartitori.Filter := '';
   // FrmData.QryRepartitori.Filtered := False;
    DropDownControl := TreeRepartitori;
  end;  

  DbRaportare.ConnectionString := DataConnectionString;
  { Obligatoriu executam asta altfel nu face fire la evenimentul AfterOpen }
  with GetADOTmpQuery do
    try
       Sql.Add('SET ANSI_WARNINGS OFF');
       ExecSql;
    finally
       Free;
    end;
    
  { Creem Formul pentru Abandonare }
  try
     RapDevices.PDF.Author := NumeLoginComplet + ' ('+NumeLogin+')';
  except
     RapDevices.PDF.Author := NumeLoginComplet ;
  end;
  RapDevices.PDF.Creator :='ATLAS';
  FAbortForm := TForm.Create(nil);
  with FAbortForm do begin
    Visible  := False;
    Position := poScreenCenter;
    Width    := 120;
    Height   := 60;
    BorderStyle := bsDialog;
    BorderIcons := BorderIcons - [biSystemMenu];
    Caption     := '';
    FormStyle   := fsStayOnTop;
    { Creem Butonul de Abandon }
    with TButton.Create(FAbortForm) do begin
      Parent  := FAbortForm;
      Left    := (120 - Width ) div 2;
      Top     := 5;
      Caption := 'Abandon';
      OnClick := DoAbort;
    end;
  end;
  ppRaport.Template.OnLoadEnd   := LoadEndEvent;
  ppRaport.Template.OnLoadStart := LoadStartTemplate;
  FIsTextBased := TipTiparire = ttText;
end;

procedure TCustomReport.LoadEndEvent(Sender: TObject);
begin
  { Stergem Parameterii in cazul in care exista }
  ppRaport.ShowAutoSearchDialog := True;
  ppRaport.AllowPrintToFile := True;
  ppRaport.ShowCancelDialog    := True;
  ppRaport.ShowPrintDialog     := True;
  ppRaport.OnPreviewFormCreate  := PreviewFormCreateEvent;
  ppRaport.OnPreviewFormClose   := PreviewFormCloseEvent;
  if FNoOnLoadEndEvent then begin
    ClearParamList;
    ppRaport.OnPrintingComplete  := ppRaportPrintingComplete;
    ppRaport.OnPrintDialogCreate := ppRaportDialogCreate;
    ppRaport.AfterPrint          := ppRaportAfterPrint;
  end;
  if FShowFooter then LocalDefaultFooter(ppRaport.FooterBand);
end;

procedure TCustomReport.LoadReport(ID: Integer; DirectToPrinter: Boolean=False; AutoClose: Boolean=False);
var I: Integer;
begin
  IsAutoClose := AutoClose;
  DirectPrint := DirectToPrinter;
  LoadReportEx(ID);
  { Testam daca exista cel putin un TppRichText in raport => Fortam Graphic}
  if FIsTextBased then
    for I := 0 to Self.ComponentCount-1 do
     if AnsiCompareText(Self.Components[I].ClassName, 'TppRichText') = 0 then begin
        FIsTextBased := False;
        Break;
     end;
  if DirectPrint then
     ppRaport.DeviceType := 'Printer'
  else
     ppRaport.DeviceType := 'Screen';
  ppRaport.Engine.Init;  // Daca nu se da Init aici se comporta bizar { Eroare a aparut la ADO }
  ppRaport.Print;
  IsAutoClose := False;
end;

procedure TCustomReport.PreviewFormCreateEvent(Sender: TObject);
begin
  with (ppRaport.PreviewForm) do begin
    FormStyle   := FPreviewFormStyle; // fsMDIChild;
    WindowState := wsMaximized;
    if FPreviewFormStyle = fsMDIChild then //fereastra noua
      SetNewForm(ppRaport.PreviewForm);
  end;
  TppViewer(ppRaport.PreviewForm.Viewer).ZoomSetting := zs100Percent;
  LocalDefaultHeader(ppRaport.TitleBand);
end;

procedure TCustomReport.ppRaportPreviewFormCreate(Sender: TObject);
begin
  TForm(ppRaport.PreviewForm).Caption := 'Afisare in Ecran  !';
end;

procedure TCustomReport.SetQuoted;
begin
  with GetADOTmpQuery do
    try
       Sql.Add('SET QUOTED_IDENTIFIER ON');
       ExecSql;
    finally
       Free;
    end;
end;

procedure TCustomReport.HideAbort;
begin
  FAbortForm.Close;
  FAbortForm.Hide;
end;

procedure TCustomReport.ShowAbort(aCaption: String);
begin
  QryAborted := False;
  FAbortForm.Caption := aCaption;
  FAbortForm.Show;
end;

procedure TCustomReport.FormDestroy(Sender: TObject);
begin
  FDirectSetParams := False;

  FPDFStream.Free;

  { ATENTIE Ca sa nu mai dea access violation }
  ClearTemporalParams;

  FreeAndNil(FAbortForm);

end;

procedure TCustomReport.DoAbort(Sender: TObject);
begin
  QryAborted := True;
end;

function TCustomReport.OpenDataSets: Boolean;
begin
  Result := True;
  with GetFrmProgress do begin
     { Incercam Sa luam numele Raportului }
     Caption := 'Generator de rapoarte';
     SetTotalCount(5);
     Active := IsVisibleProgress;
     if IsVisibleProgress then Show;
  end;
  try
    GetFrmProgress.SetTotalPos(1, 'Directoare');
    OpenAsynADOQuery(QryFolder);
    Result := (Result) and (not GetFrmProgress.IsCanceled);
    if not Result then Abort;
    GetFrmProgress.SetTotalPos(2, 'Lista Tabele');
    OpenAsynADOQuery(QryTable);
    Result := not GetFrmProgress.IsCanceled;
    if not Result then Abort;
    GetFrmProgress.SetTotalPos(3, 'Lista Campuri');
    OpenAsynADOQuery(QryField);
    Result := not GetFrmProgress.IsCanceled;
    if not Result then Abort;
    GetFrmProgress.SetTotalPos(4, 'Lista Relatii');
    OpenAsynADOQuery(QryJoin);
    Result := not GetFrmProgress.IsCanceled;
    if not Result then Abort;
    GetFrmProgress.SetTotalPos(5, 'Lista Rapoarte');
    OpenAsynADOQuery(QryItems);
    Result := not GetFrmProgress.IsCanceled;
    if not Result then Abort;
    FreeAndNil(aFrmProgress);
  except
    FreeAndNil(aFrmProgress);
    raise;
  end;
end;


procedure TCustomReport.LoadReportEx(ID: Integer);
begin
  FShowFooter := cst_DefaultShowFooter;
  FReportID := ID;
  qryItem.Active := False;
  QryItem.Parameters.ParamByName('ITEM_ID').Value := ID;
  try
    OpenAssincronQuery(qryItem,dsItem,'Citesc Raportul');
    with ppRaport.Template.DatabaseSettings do begin
      DataPipeline := plItem;
      Name := qryItem.FieldByName('ITEM_NAME').AsString;
      NameField := 'ITEM_NAME';
      TemplateField := 'TEMPLATE';
      ppRaport.Template.LoadFromDatabase;
    end;
  except
    on E: Exception do raise EContaHandledError.Create('Raportul nu este disponibil in contextul curent !');
  end;
end;

procedure TCustomReport.ppRaportPrintingComplete(Sender: TObject);
begin

  if (TransferRaport) and (AnsiCompareText(ppRaport.DeviceType, 'Screen') <> 0) then
    PostMessage(Self.Handle, WM_SEND_REPORT_TO_REPOSITORY, 0, 0);

  if IsAutoClose then
    if Assigned(ppRaport.PreviewForm) then
       ppRaport.PreviewForm.Close;
end;

procedure TCustomReport.ppRaportAfterPrint(Sender: TObject);
begin
  if (FIsTextBased) and (AnsiCompareText(ppRaport.DeviceType,'Screen')<>0) then begin
     PrintTextToPrn(FCurentReportFile, ppRaport.PrinterSetup.Copies,'');
     ppRaport.PreviewForm.Close;
  end;
end;

procedure TCustomReport.ppRaportDialogCreate(Sender: TObject);
begin
 if FIsTextBased then begin
    { Stabilim daca este cazul numele fisierului temporar }
    ppRaport.DeviceType := 'ReportTextFile';
    FCurentReportFile := TempFileName;
    ppRaport.PrintDialog.TextFileName := FCurentReportFile;
    ppRaport.PrintDialog.ModalResult := mrOk;
    PostMessage(ppRaport.PrintDialog.Handle, WM_KEYDOWN, VK_RETURN ,0)
 end
 else if DirectPrint then begin
    ppRaport.PrintDialog.ModalResult := mrOk;
    PostMessage(ppRaport.PrintDialog.Handle, WM_KEYDOWN, VK_RETURN ,0)
 end;
end;

function TCustomReport.GetReportDataSet(AutoOpen: Boolean=True): TDataSet;
begin
  Result := nil;
  if ppRaport.DataPipeLine <> nil then begin
     if AutoOpen then
        ppRaport.DataPipeLine.Open;
     if TppChildBDEPipeLine(ppRaport.DataPipeline).DataSource <> nil then
        Result := TdaChildADOQuery(TppChildBDEPipeLine(ppRaport.DataPipeline).DataSource.DataSet);
  end;
end;

procedure InitCacheEngine;
begin
  FCacheReports := TStringList.Create;
end;

procedure ClearCacheEngine;
var I: Integer;
begin
  for I := 0 to FCacheReports.Count-1 do
    TMemoryStream(FCacheReports.Objects[I]).Free;
  FCacheReports.Clear;
end;

procedure UninitCacheEngine;
begin
  ClearCacheEngine;
  FCacheReports.Free;
end;

procedure TCustomReport.LoadStartTemplate(Sender: TObject;
  aStream: TStream);
var BlobStream: TADOBlobStream;
begin
  { Suntem in explorerul de rapoarte
    Trebuie sa incarcam template-ul }
  if (dsItem.DataSet = QryItems) and (aStream.Size <= 1) then begin
     QryItem.Close;
     QryItem.Parameters.ParamByName('ITEM_ID').Value := QryItemsitem_id.AsInteger;
     QryItem.Open;
     BlobStream := TADOBlobStream.Create(TBlobField(QryItem.FieldByName('TEMPLATE')), bmRead);
     try
        aStream.CopyFrom(BlobStream, 0);
        aStream.Seek(0, soFromBeginning);
     finally
        BlobStream.Free;
     end;
     qryItem.Close;
  end;
end;

procedure TCustomReport.PreviewFormCloseEvent(Sender: TObject);
begin
  PostMessage(Self.Handle, WM_MUST_CLOSE_REPORT, 0, 0);
end;

function TCustomReport.GetADOTmpQuery: TADOQuery;
begin
  Result := TADOQuery.Create(Self);
  with Result do begin
    Connection := DbRaportare;
    LockType   := ltReadOnly;
  end;
end;

{ TAsyncADOThread }

constructor TAsyncADOThread.Create(aADOQuery: TADOQuery);
begin
  inherited Create(True);
  FIsError   := False;
  FRelaQry   := aADOQuery;
  FreeOnTerminate := False;
  {$IFDEF CLONE_ADO_DATASET}
  lConn  := TADOConnection.Create(nil);
  lConn.LoginPrompt := False;
  lConn.Mode := cmRead;
  lConn.IsolationLevel := ilReadUncommitted;
  lConn.CursorLocation := clUseClient;
  lConn.CommandTimeout := FRelaQry.CommandTimeout;

  if FRelaQry.Connection <> nil then
     lConn.ConnectionString := FRelaQry.Connection.ConnectionString
  else lConn.ConnectionString := FRelaQry.ConnectionString;

  lQuery := TADOQuery.Create(nil);
  lQuery.ExecuteOptions := lQuery.ExecuteOptions - [eoAsyncExecute];
  lQuery.Connection := lConn;
  lQuery.LockType := ltReadOnly;
  lQuery.CommandTimeout := FRelaQry.CommandTimeout;
  lQuery.CursorLocation := clUseClient;
  lQuery.SQL.Assign(FRelaQry.SQL);
  lQuery.Parameters.Assign(FRelaQry.Parameters);
  {$ENDIF}
end;

destructor TAsyncADOThread.Destroy;
begin
  {$IFDEF CLONE_ADO_DATASET}
  lQuery.Free;
  lConn.Free;
  {$ENDIF}
  inherited Destroy;
end;

procedure TAsyncADOThread.Execute;
begin
  try
     FIsError := False;
     {$IFDEF CLONE_ADO_DATASET}
     lQuery.Open;
     Synchronize(Finalizare);
     {$ELSE}
     FRelaQry.Open;
     {$ENDIF}
  except
     on E: Exception do begin
        FIsError := True;
        FErrorMessage := E.Message;
     end;
  end;
  Terminate;
end;

procedure TCustomReport.DbRaportareExecuteComplete(
  Connection: TADOConnection; RecordsAffected: Integer;
  const Error: Error; var EventStatus: TEventStatus;
  const Command: _Command; const Recordset: _Recordset);
begin
  if (Error <> nil) and (FCurrentThread <> nil) then begin
     FCurrentThread.IsError := True;
     FCurrentThread.ErrorMessage := 'Eroare : '+Error.Description;
     FCurrentThread.Terminate;
  end;
  {FCurrentThread.Terminate;}
end;

{$IFDEF CLONE_ADO_DATASET}
procedure TAsyncADOThread.Finalizare;
begin
  FRelaQry.Clone(lQuery);
end;
{$ENDIF}

procedure TCustomReport.ppDirectToPDFDialogCreate(Sender: TObject);
begin
  with TppReport(Sender) do begin
    PrintDialog.PrintToFile  := True;
    PrintDialog.TextFileName := ppGetTempFileName(ExtractFilePath(Application.ExeName), 'doc');
    PrintDialog.ModalResult  := mrOk;
    PostMessage(TppReport(Sender).PrintDialog.Handle, WM_KEYDOWN, VK_RETURN ,0)
  end;
end;

procedure TCustomReport.ClearParamList(MustClose: Boolean = False);
var I, J, K: Integer;
    Qry: TADOQuery;
begin
  for I := 0 to Self.ComponentCount-1 do
    if Self.Components[I] is TdaCustomDataView then
       with TdaCustomDataView(Self.Components[I]) do
         for J := 0 to DataPipelineCount-1 do begin
           if (not (DataPipeLines[J].InheritsFrom(TppChildBDEPipeline))) or
              //(TppChildBDEPipeLine(DataPipeLines[J]).Active) or
              (TppChildBDEPipeLine(DataPipeLines[J]).DataSource = nil) or
              (TppChildBDEPipeLine(DataPipeLines[J]).DataSource.DataSet = nil) or
              (not (TppChildBDEPipeLine(DataPipeLines[J]).DataSource.DataSet.InheritsFrom(TADOQuery))) then Continue;
           Qry := TADOQuery(TppChildBDEPipeLine(DataPipeLines[J]).DataSource.DataSet);
           if Qry.Active then
              if MustClose then begin Active := False; Qry.Active := False; end
              else
                for K := 0 to Qry.Parameters.Count-1 do
                  Qry.Parameters[K].Value := Null;
         end;
end;

procedure TCustomReport.TreePlanConturiDblClick(Sender: TObject);
begin
  { Inchidem Cu Accept }
  with TdxDBTreeList(Sender) do
    if (FocusedNode <> nil) then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
end;

procedure TCustomReport.TreePlanConturiKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if Key = VK_RETURN then begin
    with TdxDBTreeList(Sender) do
      if (FocusedNode <> nil) then
       (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
  end
  else if Key = VK_ESCAPE then
    with TdxDBTreeList(Sender) do
      if (FocusedNode <> nil) then
       (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(False);
end;

procedure TCustomReport.SendToRepository(Sender: TObject);
begin
  SendToDocumentManagement(Self, FPDFStream, 'Document Contabilitate', 'ACT_'+IntToStr(FOriginalID), FOriginalID);
end;

procedure InternalGetStream(Sender: TObject; AStream: TStream);
var
  lReport : TppReport;
begin
  lReport := TppReport(TComponent(Sender).Owner);
  TCustomReport(lReport.Owner).FPDFStream.CopyFrom(AStream, 0);
end;

procedure TestAndUnInitRepository;
begin
  if Assigned(lDocRepositoryConnection) then
    FreeAndNil(lDocRepositoryConnection);
  if Assigned(lDocSQLServerConnection) then
    FreeAndNil(lDocSQLServerConnection);
end;

procedure TestAndInitRepository;
var
  lIniFileName: String;
begin
  TransferRaport := False;
  lIniFileName := ChangeFileExt(ParamStr(0), '.rep');
  TransferRaport := FileExists(lIniFileName);
  if TransferRaport then begin
    with TIniFile.Create(lIniFileName) do
      try
         RepositoryActive := SectionExists('REPOSITORY');
         if RepositoryActive then begin
           lDocRepSrvName := ReadString('REPOSITORY', 'SERVER', '127.0.0.1');
           lDocRepSrvPort := ReadInteger('REPOSITORY', 'PORT', 17702);
           with TIdTCPClient.Create(nil) do
            try
              Host := lDocRepSrvName;
              Port := lDocRepSrvPort;
              try
                ConnectTimeout := 100;
                Connect;
              except
                RepositoryActive := False;
              end;
            finally
              Free;
            end;
         end;
         TransferMetaData := SectionExists('INFODOC_DB');
         lDocMetaSrvName  := ReadString('INFODOC_DB', 'SERVER', '');
         lDocMetaSrvDB    := ReadString('INFODOC_DB', 'DATABASE', '');
         TransferMetaData := (Trim(lDocMetaSrvName) > '') and (Trim(lDocMetaSrvDB) > '');
      finally
         Free;
      end;
  end;
end;

procedure TCustomReport.WmSendReportToRepository(var Message: TMessage);
var
  lOldShowDialog,
  lOldShowCancel : Boolean;
  OldPrintComplete : TNotifyEvent;
  OldDeviceName    : String;
  lOldStreamHandle : TPDFOnReceiveStream;
begin
  OriginalID := -1;
  lOldShowDialog := ppRaport.ShowPrintDialog;
  lOldShowCancel := ppRaport.ShowCancelDialog;
  OldPrintComplete := ppRaport.OnPrintingComplete;
  OldDeviceName    := ppRaport.DeviceType;

  lOldStreamHandle   := PDFOnReceiveStream;
  PDFOnReceiveStream := LocalReceiveStream;
  try
    ppRaport.ShowCancelDialog   := False;
    ppRaport.ShowPrintDialog    := False;
    ppRaport.OnPrintingComplete := SendToRepository;
    ppRaport.DeviceType         := TDirectPDFDevice.DeviceName;
    ppRaport.Engine.Reset;
    ppRaport.Print;
  finally
    ppRaport.ShowPrintDialog    := lOldShowDialog;
    ppRaport.ShowCancelDialog   := lOldShowCancel;
    ppRaport.OnPrintingComplete := OldPrintComplete;
    ppRaport.DeviceType         := OldDeviceName;
    PDFOnReceiveStream          := lOldStreamHandle;
  end;
end;

procedure TCustomReport.LocalReceiveStream(Sender: TObject;
  AStream: TStream);
begin
  FPDFStream.CopyFrom(AStream, 0);
end;

procedure TCustomReport.RefreshParams;
var
  I, J : Integer;
  Qry: TADOQuery;
  lQryList: TList;
//  lMustClose: Boolean;
  ldaViewList: TList;

begin
  lQryList := TList.Create;
  try
    for I := 0 to Self.ComponentCount-1 do
      if Self.Components[I] is TdaCustomDataView then
         with TdaCustomDataView(Self.Components[I]) do
           for J := 0 to DataPipelineCount-1 do begin
             if (not (DataPipeLines[J].InheritsFrom(TppChildBDEPipeline))) or
                //(TppChildBDEPipeLine(DataPipeLines[J]).Active) or
                (TppChildBDEPipeLine(DataPipeLines[J]).DataSource = nil) or
                (TppChildBDEPipeLine(DataPipeLines[J]).DataSource.DataSet = nil) or
                (not (TppChildBDEPipeLine(DataPipeLines[J]).DataSource.DataSet.InheritsFrom(TADOQuery))) then Continue;
             Qry := TADOQuery(TppChildBDEPipeLine(DataPipeLines[J]).DataSource.DataSet);
             if Qry.Active then begin
                Qry.Close;
                lQryList.Add(Qry);
             end;
           end;
    if lQryList.Count > 0 then
       for I := 0 to lQryList.Count-1 do begin
         if not FDirectSetParams then begin
            FDirectSetParams := SetMsSqlParams( TADOQuery(lQryList[I]), False, False );
            if not FDirectSetParams then Abort;
         end
         else
           if not SetMsSqlParams( TADOQuery(lQryList[I]), False, False ) then
              Abort;
       end;
       if FDirectSetParams then begin
        ldaViewList := TList.Create;
        try
          FillDataViews(ldaViewList);
          for I := 0 to ldaViewList.Count-1 do begin
            OpenDataPipeLines(TdaCustomDataView(ldaViewList[I]));
          end;
        finally
          ldaViewList.Free;
        end;
       end;

  finally
    lQryList.Free;
  end;
end;

procedure TCustomReport.ppRaportBeforeOpenDataPipelines(Sender: TObject);
begin
  Exit;
  SetParamList;
end;

procedure TCustomReport.FillDataViews(List: TList);
var
  I: Integer;
begin
  List.Clear;
  for I := 0 to Self.ComponentCount-1 do
    if Self.Components[I] is TdaCustomDataView then
       List.Add(Self.Components[I]);
end;

procedure TCustomReport.ppRaportBeforeAutoSearchDialogCreate(
  Sender: TObject);
begin
  if (ppRaport.DeviceType = 'Screen') or (FInExploreMode) then begin
     Self.RefreshParams;
     ppRaport.Engine.Reset;
  end;
end;

procedure TCustomReport.ppRaportAfterOpenDataPipelines(Sender: TObject);
begin
  FDirectSetParams := False;
end;

procedure TCustomReport.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  if FrmData.QryRepartitori.Filter <> '' then begin
    FrmData.QryRepartitori.Filter := '';
    FrmData.QryRepartitori.Filtered := False;
  end;
  if not ppRaport.ModalPreview then begin
     Action := caFree;
  end;
end;

procedure TCustomReport.WmMustCloseReport(var Message: TMessage);
begin
  { Ideea e sa apuce sa proceseze evenimentul de on PreviewFormClose }
  Self.Close;
end;

procedure TCustomReport.SetParamList;
var
  I: Integer;
  ldaViewList: TList;
  lMustClose : Boolean;
begin
  ldaViewList := TList.Create;
  try
    FillDataViews(ldaViewList);
    for I := 0 to ldaViewList.Count-1 do begin
      SetCurentParams(TdaCustomDataView(ldaViewList[I]), lMustClose);
      if lMustClose then
          Abort;
    end;
  finally
    ldaViewList.Free;
  end;
end;

function TCustomReport.GetCanShowAbort: Boolean;
begin
  Result := FCanShowAbort and ppRaport.ShowCancelDialog;
end;


procedure TCustomReport.RapExplorerClose(Sender: TObject;
  var Action: TCloseAction);
begin
  InExploreMode := False;
end;

procedure TCustomReport.RapDesignerClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

initialization
  aFrmProgress := nil;
  SConnection  := '';
//  InternalSetParam  := SetCurentParams;
  TestAndInitRepository;
  InitCacheEngine;
finalization
  UninitCacheEngine;
  TestAndUnInitRepository;
  if aFrmProgress <> nil then
     FreeAndNil(aFrmProgress);
end.
