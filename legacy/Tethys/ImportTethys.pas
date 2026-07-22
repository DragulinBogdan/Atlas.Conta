unit ImportTethys;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, StdCtrls, TethysAtlasWS,
  Rio, SOAPHTTPClient, Provider,
  Xmlxform, DB, DBClient, XMLIntf, XMLDoc,
  cxGraphics, cxDataStorage,
  cxEdit, cxDBData, cxGridLevel, cxClasses, cxControls, cxGridCustomView,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGrid, ZDataSet,
  cxGridCustomPopupMenu, cxGridPopupMenu, cxImageComboBox,
  Menus, cxLookAndFeelPainters, cxButtons,
  ZAbstractRODataset, ZAbstractDataset,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, xmldom, cxNavigator,
  dxDateRanges, cxDataControllerConditionalFormattingRulesManagerDialog,
  dxBarBuiltInMenu, dxScrollbarAnnotations;

//var
//  cst_DocTethys : String = 'f180acc0-ead2-4709-8c15-cd8148b150a0'; //factura fiscala


type
  PRecTethys = ^RecTethys;
  RecTethys = record
    registruID : String;
    nrInreg : Integer;
    dataInreg : String;
    tipDocumentID : String;
    stareID : String;    
  end;

//  TTethysAtlasWSSoap = class(TInterfacedObject, TethysAtlasWSSoap);

  TTethysObject = class(THTTPRIO{, TethysAtlasWSSoap})
  private
    FMyTethIntf: TethysAtlasWSSoap;
    procedure HTTPRIOBeforeExecute(const MethodName: String; {$IFNDEF VER230} var SOAPRequest: WideString{$ELSE}SOAPRequest:TStream{$ENDIF});
  public
    property MyTethIntf: TethysAtlasWSSoap read FMyTethIntf;// implements TethysAtlasWSSoap;
  end;

  TfrmSelectDM = class(TForm)
    ClientDataSet: TClientDataSet;
    DTLista: TDataSource;
    XMLTransformProvider: TXMLTransformProvider;
    GridFCT: TcxGridDBTableView;
    GridFCTL: TcxGridLevel;
    cxGrid: TcxGrid;
    Memo1: TMemo;
    GridFCTregistruID: TcxGridDBColumn;
    GridFCTnrInreg: TcxGridDBColumn;
    GridFCTdataInreg: TcxGridDBColumn;
    GridFCTprovenienta: TcxGridDBColumn;
    GridFCTadresa: TcxGridDBColumn;
    GridFCTtipDocumentID: TcxGridDBColumn;
    GridFCTdescriere: TcxGridDBColumn;
    GridFCTstareDocument: TcxGridDBColumn;
    GridFCTnivelCurent: TcxGridDBColumn;
    ClientDataSetregistruID: TStringField;
    ClientDataSetnrInreg: TStringField;
    ClientDataSetdataInreg: TStringField;
    ClientDataSetprovenienta: TStringField;
    ClientDataSetadresa: TStringField;
    ClientDataSettipDocumentID: TStringField;
    ClientDataSetdescriere: TStringField;
    ClientDataSetstareDocument: TStringField;
    ClientDataSetnivelCurent: TStringField;
    ClientDataSetPreview: TStringField;
    GridFCTPreview: TcxGridDBColumn;
    cxGridPopupMenu: TcxGridPopupMenu;
    qryListaImperecheri: TZQuery;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    btnRefresh: TcxButton;
    procedure ClientDataSetCalcFields(DataSet: TDataSet);
    procedure FormCreate(Sender: TObject);
    procedure GridFCTFocusedRecordChanged(Sender: TcxCustomGridTableView;
      APrevFocusedRecord, AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure ClientDataSetFilterRecord(DataSet: TDataSet;
      var Accept: Boolean);
    procedure btnRefreshClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
  private
    FIdTethys: String;
    FCurentRec: RecTethys;
    FIsValidat: Boolean;
    FTipDocTethys: String;
    procedure SetIsValidat(const Value: Boolean);
    { Private declarations }
  public
    { Public declarations }
    procedure PopulateNomenclatoare;
    procedure InterpretareMetode (const MethodName: String; SOAPResponse: TStream);
    property  IdTethys : String read FIdTethys write FIdTethys;
    property  CurentRec : RecTethys read FCurentRec write FCurentRec;
    property  IdGestDocum : Boolean read FIsValidat write SetIsValidat;
    property  IsValidat : Boolean read FIsValidat write SetIsValidat;
    property  TipDocTethys : String read FTipDocTethys write FTipDocTethys;
  end;


function SelectieLegatura(Validate : Boolean; TipDocId : String) : PRecTethys;

var
  gTethysObject : TTethysObject;

implementation


{$R 'ResurseTethys.res' 'ResurseTethys.rc'}


uses
  dxCompsUtile, ZeosDBUtile, dateUnit;

{$R *.dfm}


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



function SelectieLegatura(Validate : Boolean; TipDocId : String) : PRecTethys;
begin
  with TfrmSelectDM.Create(nil) do begin
    try
      TipDocTethys := TipDocId;
      btnRefreshClick(nil);
      IsValidat := Validate;
      ShowModal;
      if ModalResult = mrOk then begin
        New(Result);
        ZeroMemory(Result, SizeOf(RecTethys));
        Result^ := CurentRec;
      end
      else
        Result := nil;
    finally
      Free;
    end;
  end;
end;


procedure TfrmSelectDM.InterpretareMetode(const MethodName: String;
  SOAPResponse: TStream);
var
  XMLDoc: IXMLDocument;
  lDir : String;
begin
  SOAPResponse.Position := 0;
  lDir := IncludeTrailingPathDelimiter(ExtractFilePath(ParamStr(0)));
  ExtractFileFromResource(MethodName, lDir+ MethodName + '.xtr');
  Memo1.Lines.LoadFromStream(SOAPResponse);
  if not FileExists(lDir + MethodName + '.xtr') then Exit;
  XMLTransformProvider.TransformRead.TransformationFile := lDir + MethodName + '.xtr';
//  Caption := lDir + MethodName + '.xtr';
  ClientDataset.Active := FALSE;
  SOAPResponse.Position := 0;
  XMLDoc := NewXMLDocument;
  XMLDoc.Encoding := 'utf-8';
  SOAPResponse.Position := 0;
  XMLDoc.LoadFromStream(SOAPResponse);
  XMLTransformProvider.TransformRead.SourceXmlDocument := XMLDoc.GetDOMDocument;
  ClientDataset.Active := TRUE;
end;

procedure InitializeTethys;
begin
  if gTethysObject = nil then begin
    gTethysObject := TTethysObject.Create(nil);
    gTethysObject.FMyTethIntf := GetTethysAtlasWSSoap(True, '', nil, gTethysObject.HTTPRIOBeforeExecute);
  end;
end;

procedure UnInitializeTethys;
var
     sr: TSearchRec;
     FileAttrs: Integer;
     lDir : String;
begin
   //stergem fisiere xtr
   FileAttrs := faAnyFile;
   lDir := IncludeTrailingPathDelimiter(ExtractFilePath(ParamStr(0)));
   if SysUtils.FindFirst(lDir + '*.xtr', FileAttrs, sr) = 0 then begin
        repeat
          if ((sr.Attr and FileAttrs) = sr.Attr) and (sr.Name[1] <> '.') then begin
              DeleteFile(lDir + sr.name);
          end;
        until SysUtils.FindNext(sr) <> 0;
        FindClose(sr);
   end;

  if gTethysObject <> nil then begin
    //THTTPRIO(lTeth.FMyTethIntf).Free;
    gTethysObject.Free;
  end;

end;

procedure TfrmSelectDM.ClientDataSetCalcFields(DataSet: TDataSet);
begin
  DataSet.FieldByName('Preview').AsString :=  DataSet.FieldByName('provenienta').AsString + ' - ' + DataSet.FieldByName('adresa').AsString;
end;

procedure TfrmSelectDM.FormCreate(Sender: TObject);
begin
  InitializeTethys;
  ZeroMemory(@FCurentRec, SizeOf(RecTethys));
  DBRefresh(qryListaImperecheri);
  PopulateNomenclatoare;
  IsValidat := False;
end;

procedure TfrmSelectDM.PopulateNomenclatoare;
begin
  FillImageCombo(GridFCTtipDocumentID.Properties, 'SELECT * FROM TethysTIPURIDOCUMENTE', 'tipDocumentID', 'tipDocument');
  FillImageCombo(GridFCTstareDocument.Properties, 'SELECT * FROM TethysSTAREDOCUMENT', 'stareID', 'descriere');
end;

procedure TfrmSelectDM.GridFCTFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
  if AFocusedRecord = nil then Exit;
  if not AFocusedRecord.IsData then Exit;
  FCurentRec.nrInreg        := ValueSafeToInt(AFocusedRecord.Values[GridFCTnrInreg.Index]);
  FCurentRec.registruID     := ValueSafeToStr(AFocusedRecord.Values[GridFCTregistruID.Index]);
  FCurentRec.dataInreg      := ValueSafeToStr(AFocusedRecord.Values[GridFCTdataInreg.Index]);
  FCurentRec.tipDocumentID  := ValueSafeToStr(AFocusedRecord.Values[GridFCTtipDocumentID.Index]);
  FCurentRec.stareID        := ValueSafeToStr(AFocusedRecord.Values[GridFCTstareDocument.Index]);
end;

procedure TfrmSelectDM.ClientDataSetFilterRecord(DataSet: TDataSet;
  var Accept: Boolean);
begin
  Accept := not qryListaImperecheri.Locate('TT_id', DataSet.FieldByName('registruID').AsString, []);
end;

procedure TfrmSelectDM.btnRefreshClick(Sender: TObject);
begin
  DBRefresh(qryListaImperecheri);
  if gTethysObject = nil then Exit;
  gTethysObject.OnAfterExecute := InterpretareMetode;
  gTethysObject.MyTethIntf.GetRegistruTipDocument(FTipDocTethys);
end;

procedure TfrmSelectDM.SetIsValidat(const Value: Boolean);
var
  isDataOpen : Boolean;
begin
  FIsValidat := Value;
  isDataOpen := qryListaImperecheri.Active;
  if isDataOpen then qryListaImperecheri.Close;
  if FIsValidat then
    qryListaImperecheri.Params.ParamByName('stare').Value := 1
  else
    qryListaImperecheri.Params.ParamByName('stare').Value := 0;
  if isDataOpen then DBRefresh(qryListaImperecheri);

  ClientDataSet.Filtered  := False;
  ClientDataSet.Filtered  := True;
end;

procedure TfrmSelectDM.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
   UnInitializeTethys;
end;

{ TTethysObject }

procedure TTethysObject.HTTPRIOBeforeExecute;
begin
  Self.URL :=  TethysAtlasWS.defURL;  //'http://mail.cjc.ro:83/TethysAtlasWebService/TethysAtlasWS.asmx';
end;

initialization
  gTethysObject := nil;
  InitializeTethys;
finalization
  UnInitializeTethys;
end.
