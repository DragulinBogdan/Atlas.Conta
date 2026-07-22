unit CustomReport;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  DB, ZDataSet, frxClass, frxDesgn, frxBarcode, frxOLE, frxChart, frxRich, frxCross,
  frxChBox, frxGradient, frxDMPExport, frxDCtrl, frxGZip, frxADOComponents,
  fs_idbctrlsrtti, fs_idbrtti, fs_iinterpreter, fs_ipascal, fs_icpp, fs_ijs,
  fs_ibasic, fs_iclassesrtti, fs_igraphicsrtti, fs_iformsrtti,
  fs_iextctrlsrtti, fs_idialogsrtti, fs_iadortti, fs_ichartrtti,
  frxExportTXT, frxExportHTML, frxExportXLS, frxExportXML, frxExportRTF,
  frxExportImage, frxExportPDF, DBExtend;

type
  TReports = class(TDataModule)
    Designer: TfrxDesigner;
    BarCode: TfrxBarCodeObject;
    OLE: TfrxOLEObject;
    Chart: TfrxChartObject;
    RTF: TfrxRichObject;
    Cross: TfrxCrossObject;
    CheckBox: TfrxCheckBoxObject;
    Gradient: TfrxGradientObject;
    DotMatr: TfrxDotMatrixExport;
    Dialog: TfrxDialogControls;
    ZIP: TfrxGZipCompressor;
    ADO: TfrxADOComponents;
    DbRtti: TfsDBRTTI;
    DbCtrlsRtti: TfsDBCtrlsRTTI;
    ScriptScr: TfsScript;
    PascalScr: TfsPascal;
    CPlusPlusScr: TfsCPP;
    JavaScr: TfsJScript;
    BasicScr: TfsBasic;
    ClassesRtti: TfsClassesRTTI;
    GraphicRtti: TfsGraphicsRTTI;
    FormsRtti: TfsFormsRTTI;
    ExtCtrlsRtti: TfsExtCtrlsRTTI;
    DialogsRtti: TfsDialogsRTTI;
    AdoRtti: TfsADORTTI;
    ChartRtti: TfsChartRTTI;
    TextExp: TfrxTXTExport;
    HTMLExp: TfrxHTMLExport;
    XLSExp: TfrxXLSExport;
    XMLExp: TfrxXMLExport;
    RTFExp: TfrxRTFExport;
    JpgExp: TfrxJPEGExport;
    PDFExp: TfrxPDFExport;
    TiffExp: TfrxTIFFExport;
    Report: TfrxReport;
    ADOConnection1: TZConnection;
    procedure ReportsCreate(Sender: TObject);
  private
    { Private declarations }
  protected
    { Protected declarations }
    FReportId: Integer;
    FCurentStream: TMemoryStream;

    function GetNewQuery: TZQuery;

    procedure SaveReportToDb;

  public
    { Public declarations }
    procedure Explore;

    function  LoadReportEx(AReportId: Integer) : TfrxReport;
    function  NewReport: Integer;
    procedure DesignReport(AReportId: Integer);
    procedure LoadReport(AReportId: Integer);

  end;

  PReportEntry = ^TReportEntry;
  TReportEntry = record
    FReportId  : Integer;
    FReportStream: TMemoryStream;
  end;

var
  Reports: TReports;

function ReportList: TStringList;

implementation

{$R *.DFM}

uses ReportExplorer;

var
  FReportList: TStringList;

procedure ClearReportCache;
var
  I: Integer;
  lRepEntry: PReportEntry;
begin
  if FReportList <> nil then begin
     for I := 0 to FReportList.Count-1 do begin
       lRepEntry := PReportEntry(FReportList.Objects[I]);
       lRepEntry^.FReportStream.Free;
       FreeMem(lRepEntry, SizeOf(TReportEntry));
     end;
     FReportList.Free;
  end;
end;

function ReportList: TStringList;
begin
  if not Assigned(FReportList) then begin
     FReportList := TStringList.Create;
     { Se accepta duplicate, putem avea rapoarte cu acelasi nume in foldere diferite   }
     { Nu avem ambiguitate deoarece identificarea se face prin identificator de raport }
     FReportList.Duplicates := dupAccept;
  end;
  Result := FReportList;
end;


function CacheReport(AReports: TReports; AIdReport: Integer): TMemoryStream;
var
  I : Integer;
  lRepEntry : PReportEntry;
  lRepName  : String;
begin
  lRepEntry := nil;
  { Testam intai daca exista deja in lista de raporte din cache }
  for I := 0 to FReportList.Count-1 do begin
    if PReportEntry(FReportList.Objects[I])^.FReportId = AIdReport then begin
       lRepEntry := PReportEntry(FReportList.Objects[I]);
       Break;
    end;
  end;
  if lRepEntry = nil then
     with AReports.GetNewQuery do
       try
          ParamCheck := False;

          Sql.Add('SELECT ITEM_NAME, TEMPLATE FROM RB_ITEM WHERE ITEM_ID = '+IntToStr(AIdReport));
          Open;
          if IsEmpty then
             raise Exception.Create('Raportul cu identificatorul '+IntToStr(AIdReport)+' nu exista in baza de date !');
          lRepName := Fields[0].AsString;
          GetMem(lRepEntry, SizeOf(TReportEntry));
          lRepEntry^.FReportId := AIdReport;
          lRepEntry^.FReportStream := TMemoryStream.Create;
          TBlobField(Fields[1]).SaveToStream(lRepEntry^.FReportStream);
          ReportList.AddObject(lRepName, TObject(lRepEntry));
       finally
          Free;
       end;
  Result := lRepEntry^.FReportStream;
end;

procedure TReports.DesignReport(AReportId: Integer);
begin
  if LoadReportEx(AReportId).DesignPreviewPage then
     SaveReportToDb;
end;

procedure TReports.Explore;
begin
  { Afisam explorer-ul si tratam rapoartele .... }
  with TfrmRepExplorer.Create(Self) do
    try
       Reports := Self;
       ShowModal;
    finally
       Free;
    end;
end;

function TReports.GetNewQuery: TZQuery;
begin
  Result := TZQuery.Create(Self);
  with Result do begin
    Connection := ADO.DefaultDatabase;
    LockType   := ltReadOnly;
  end;
end;

procedure TReports.LoadReport(AReportId: Integer);
begin
  LoadReportEx(AReportId).PreviewForm.ShowModal;
end;

function TReports.LoadReportEx(AReportId: Integer) : TfrxReport;
begin
  { Incarcam un raport }
  FReportId := AReportId;
  FCurentStream := CacheReport(Self, AReportId);
  FCurentStream.Position := 0;
  Report.LoadFromStream(FCurentStream);
  Result := Report;
end;

function TReports.NewReport: Integer;
var
  lRepEntry: PReportEntry;
begin
  { Intoarcem un raport nou }
  Result := -1;
  FCurentStream := nil;
  FReportId     := -1;
  Report.Clear;
  Report.DesignReport;
  if MessageDlg('Doriti Salvare ?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then begin
    FCurentStream := TMemoryStream.Create;
    with GetNewQuery do
      try
         Sql.Add('INSERT INTO RB_ITEM (ITEM_NAME, ITEM_SIZE, MODIFIED, TEMPLATE)');
         Sql.Add('SELECT '+QuotedStr(Report.ReportOptions.Name)+', '+IntToStr(FCurentStream.Size)+', GETDATE(), NULL');
         Sql.Add('SELECT @@IDENTITY');
         Open;
         FReportId := Fields[0].AsInteger;
         GetMem(lRepEntry, SizeOf(TReportEntry));
         lRepEntry^.FReportId := FReportId;
         lRepEntry^.FReportStream := FCurentStream;
         SaveReportToDb;
         Result := FReportId;
      finally
         Free;
      end;
  end;
end;

procedure TReports.ReportsCreate(Sender: TObject);
var lName: String;
begin
  { Setam cheia din registry pentru aplicatia curenta }
  lName := ExtractFileName(ParamStr(0));
  Report.IniFile := '\Software\Reports\'+lName;
  Report.ReportOptions.Author := 'ATLAS '+lName;
  Report.ReportOptions.Compressed := True;
end;

procedure TReports.SaveReportToDb;
var lStream : TMemoryStream;
begin
  { Salvam Raportul in baza de date }
  lStream := TMemoryStream.Create;
  try
    Report.SaveToStream(lStream);
    with GetNewQuery do
      try
        Sql.Add('UPDATE RB_ITEM SET TEMPLATE = :TEMPLATE WHERE ITEM_ID = '+IntToStr(FReportId));
        lStream.Position := 0;
        Params[0].LoadFromStream(lStream, ftBlob);
        ExecSql;
        { Actualizam si cache-ul }
        FCurentStream.CopyFrom(lStream, 0);
      finally
         Free;
      end;
  finally
    lStream.Free;
  end;
end;

initialization
  FReportList := nil;
finalization
  ClearReportCache;
end.
