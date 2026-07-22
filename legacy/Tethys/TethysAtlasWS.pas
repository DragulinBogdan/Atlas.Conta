// ************************************************************************ //
// The types declared in this file were generated from data read from the
// WSDL File described below:
// WSDL     : http://192.168.1.4/TethysAtlasWebService/TethysAtlasWS.asmx?WSDL
// Encoding : utf-8
// Codegen  : [wfDebug,wfMapStringsToWideStrings,wfUseSerializerClassForAttrs]
// Version  : 1.0
// (04.06.2009 13:16:00 - 1.33.2.5)
// ************************************************************************ //

unit TethysAtlasWS;

interface

uses InvokeRegistry, SOAPHTTPClient, Types, Rio;

type

  // ************************************************************************ //
  // The following types, referred to in the WSDL document are not being represented
  // in this file. They are either aliases[@] of other types represented or were referred
  // to but never[!] declared in the document. The types from the latter category
  // typically map to predefined/known XML or Borland types; however, they could also
  // indicate incorrect WSDL documents that failed to declare or import a schema type.
  // ************************************************************************ //
  // !:int             - "http://www.w3.org/2001/XMLSchema"
  // !:string          - "http://www.w3.org/2001/XMLSchema"
  // !:boolean         - "http://www.w3.org/2001/XMLSchema"

  GetTipuriDocumenteResult = class;             { "http://localhost/TethysAtlasWebService/" }
  GetUtilizatoriResult = class;                 { "http://localhost/TethysAtlasWebService/" }
  GetStariDocumentResult = class;               { "http://localhost/TethysAtlasWebService/" }
  GetRegistruResult    = class;                 { "http://localhost/TethysAtlasWebService/" }
  GetRegistruTipDocumentResult = class;         { "http://localhost/TethysAtlasWebService/" }
  GetNoteDocumentResult = class;                { "http://localhost/TethysAtlasWebService/" }



  // ************************************************************************ //
  // Namespace : http://localhost/TethysAtlasWebService/
  // ************************************************************************ //
  GetTipuriDocumenteResult = class(TRemotable)
  private
    Fschema: WideString;
  published
    property schema: WideString read Fschema write Fschema;
  end;



  // ************************************************************************ //
  // Namespace : http://localhost/TethysAtlasWebService/
  // ************************************************************************ //
  GetUtilizatoriResult = class(TRemotable)
  private
    Fschema: WideString;
  published
    property schema: WideString read Fschema write Fschema;
  end;



  // ************************************************************************ //
  // Namespace : http://localhost/TethysAtlasWebService/
  // ************************************************************************ //
  GetStariDocumentResult = class(TRemotable)
  private
    Fschema: WideString;
  published
    property schema: WideString read Fschema write Fschema;
  end;



  // ************************************************************************ //
  // Namespace : http://localhost/TethysAtlasWebService/
  // ************************************************************************ //
  GetRegistruResult = class(TRemotable)
  private
    Fschema: WideString;
  published
    property schema: WideString read Fschema write Fschema;
  end;



  // ************************************************************************ //
  // Namespace : http://localhost/TethysAtlasWebService/
  // ************************************************************************ //
  GetRegistruTipDocumentResult = class(TRemotable)
  private
    Fschema: WideString;
  published
    property schema: WideString read Fschema write Fschema;
  end;



  // ************************************************************************ //
  // Namespace : http://localhost/TethysAtlasWebService/
  // ************************************************************************ //
  GetNoteDocumentResult = class(TRemotable)
  private
    Fschema: WideString;
  published
    property schema: WideString read Fschema write Fschema;
  end;


  // ************************************************************************ //
  // Namespace : http://localhost/TethysAtlasWebService/
  // soapAction: http://localhost/TethysAtlasWebService/%operationName%
  // transport : http://schemas.xmlsoap.org/soap/http
  // style     : document
  // binding   : TethysAtlasWSSoap
  // service   : TethysAtlasWS
  // port      : TethysAtlasWSSoap
  // URL       : http://192.168.1.4/TethysAtlasWebService/TethysAtlasWS.asmx
  // ************************************************************************ //
  TethysAtlasWSSoap = interface(IInvokable)
  ['{38B36D31-DFDF-916A-D114-AC911202C5A3}']
    function  GetTipuriDocumente: GetTipuriDocumenteResult; stdcall;
    function  GetUtilizatori: GetUtilizatoriResult; stdcall;
    function  GetStariDocument: GetStariDocumentResult; stdcall;
    function  GetRegistru(const tipDocumentID: WideString; const dataMin: WideString; const dataMax: WideString; const stareDocument: WideString; const nrInreg: Integer; const anul: Integer; const persoanaID: WideString): GetRegistruResult; stdcall;
    function  GetRegistruTipDocument(const tipDocumentID: WideString): GetRegistruTipDocumentResult; stdcall;
    function  OpereazaDocument(const registruID: WideString; const stareDocument: WideString; const utilizatorID: WideString; const stadiulID: Integer): Integer; stdcall;
    function  StadiiInsert(const stadiulID: Integer; const stadiul: WideString; const inactiv: Boolean; const modulID: Integer): Integer; stdcall;
    function  StadiiUpdate(const stadiulID: Integer; const stadiul: WideString; const inactiv: Boolean; const modulID: Integer): Integer; stdcall;
    function  StadiiDelete(const stadiulID: Integer): Integer; stdcall;
    function  GetNoteDocument(const registruID: WideString; const utilizatorID: WideString): GetNoteDocumentResult; stdcall;
    procedure InsertNoteDocument(const registruID: WideString; const utilizatorID: WideString; const nota: WideString); stdcall;
    procedure UpdateNoteDocument(const notaID: WideString; const utilizatorID: WideString; const nota: WideString); stdcall;
  end;

//function GetTethysAtlasWSSoap(UseWSDL: Boolean=System.False; Addr: string=''; HTTPRIO: THTTPRIO = nil): TethysAtlasWSSoap;
function GetTethysAtlasWSSoap(UseWSDL: Boolean=System.False; Addr: string=''; HTTPRIO: THTTPRIO = nil; MyBeforeExecute : TBeforeExecuteEvent = nil): TethysAtlasWSSoap;

const
  defSvc  = 'TethysAtlasWS';
  defPrt  = 'TethysAtlasWSSoap';
//  defWSDL = 'http://192.168.1.4/TethysAtlasWebService/TethysAtlasWS.asmx?WSDL';
//  defURL  = 'http://192.168.1.4/TethysAtlasWebService/TethysAtlasWS.asmx';

var
  defWSDL, defURL: String;

implementation


uses IniFiles, SysUtils;

//function GetTethysAtlasWSSoap(UseWSDL: Boolean; Addr: string; HTTPRIO: THTTPRIO): TethysAtlasWSSoap;
function GetTethysAtlasWSSoap(UseWSDL: Boolean; Addr: string; HTTPRIO: THTTPRIO; MyBeforeExecute : TBeforeExecuteEvent): TethysAtlasWSSoap;
var
  RIO: THTTPRIO;
begin
  Result := nil;
  if (Addr = '') then
  begin
    if UseWSDL then
      Addr := defWSDL
    else
      Addr := defURL;
  end;
  if HTTPRIO = nil then
    RIO := THTTPRIO.Create(nil)
  else
    RIO := HTTPRIO;
  try
    Result := (RIO as TethysAtlasWSSoap);
    if UseWSDL then
    begin
      RIO.WSDLLocation := Addr;
      RIO.Service := defSvc;
      RIO.Port := defPrt;
      if Assigned(MyBeforeExecute) then
        RIO.OnBeforeExecute := MyBeforeExecute;
    end else
      RIO.URL := Addr;
  finally
    if (Result = nil) and (HTTPRIO = nil) then
      RIO.Free;
  end;
end;


initialization
  with TIniFile.Create(ExtractFilePath(ParamStr(0))+'Tethys.ini') do
  try
    defWSDL := ReadString('URL', 'defWSDL', 'http://192.168.1.4/TethysAtlasWebService/TethysAtlasWS.asmx?WSDL');
    defURL :=  ReadString('URL', 'defURL',  'http://192.168.1.4/TethysAtlasWebService/TethysAtlasWS.asmx');
  finally
    Free;
  end;
  InvRegistry.RegisterInterface(TypeInfo(TethysAtlasWSSoap), 'http://localhost/TethysAtlasWebService/', 'utf-8');
  InvRegistry.RegisterDefaultSOAPAction(TypeInfo(TethysAtlasWSSoap), 'http://localhost/TethysAtlasWebService/%operationName%');
  InvRegistry.RegisterInvokeOptions(TypeInfo(TethysAtlasWSSoap), ioDocument);
  RemClassRegistry.RegisterXSClass(GetTipuriDocumenteResult, 'http://localhost/TethysAtlasWebService/', 'GetTipuriDocumenteResult');
  RemClassRegistry.RegisterXSClass(GetUtilizatoriResult, 'http://localhost/TethysAtlasWebService/', 'GetUtilizatoriResult');
  RemClassRegistry.RegisterXSClass(GetStariDocumentResult, 'http://localhost/TethysAtlasWebService/', 'GetStariDocumentResult');
  RemClassRegistry.RegisterXSClass(GetRegistruResult, 'http://localhost/TethysAtlasWebService/', 'GetRegistruResult');
  RemClassRegistry.RegisterXSClass(GetRegistruTipDocumentResult, 'http://localhost/TethysAtlasWebService/', 'GetRegistruTipDocumentResult');
  RemClassRegistry.RegisterXSClass(GetNoteDocumentResult, 'http://localhost/TethysAtlasWebService/', 'GetNoteDocumentResult');

end.
