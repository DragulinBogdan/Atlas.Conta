// ************************************************************************ //
// The types declared in this file were generated from data read from the
// WSDL File described below:
// WSDL     : http://www.infovalutar.ro/curs.asmx?WSDL
// Encoding : utf-8
// Version  : 1.0
// (23.02.2006 17:24:08 - 1.33.2.5)
// ************************************************************************ //

unit ImportCurs;

interface

uses InvokeRegistry, SOAPHTTPClient, Types, XSBuiltIns, SysUtils;

type

  // ************************************************************************ //
  // The following types, referred to in the WSDL document are not being represented
  // in this file. They are either aliases[@] of other types represented or were referred
  // to but never[!] declared in the document. The types from the latter category
  // typically map to predefined/known XML or Borland types; however, they could also 
  // indicate incorrect WSDL documents that failed to declare or import a schema type.
  // ************************************************************************ //
  // !:double          - "http://www.w3.org/2001/XMLSchema"
  // !:dateTime        - "http://www.w3.org/2001/XMLSchema"


  { "http://www.infovalutar.ro/" }
  IDMoneda = (
      ROL,
      EUR,
      USD,
      XAU,
      CAD,
      GBP,
      AUD,
      DEM,
      ATS,
      BEF,
      CHF,
      CZK,
      DKK,
      EGP,
      ESP,
      FIM,
      FRF,
      GRD,
      HUF,
      IEP,
      ITL,
      JPY,
      MDL,
      NLG,
      NOK,
      PLN,
      PTE,
      SEK,
      TRL,
      TRY_,
      XDR,
      XEU,
      RON
);


  // ************************************************************************ //
  // Namespace : http://www.infovalutar.ro/
  // soapAction: http://www.infovalutar.ro/%operationName%
  // transport : http://schemas.xmlsoap.org/soap/http
  // style     : document
  // binding   : CursSoap
  // service   : Curs
  // port      : CursSoap
  // URL       : http://www.infovalutar.ro/curs.asmx
  // ************************************************************************ //
  CursSoap = interface(IInvokable)
  ['{F8E06DD3-9D51-7010-722B-2F125526ADA6}']
    function  GetLatestValue(const Moneda: IDMoneda): Double; stdcall;
    function  GetValue(const TheDate: TXSDateTime; const Moneda: String{IDMoneda}): Double; stdcall;
  end;

function GetCursSoap(UseWSDL: Boolean=System.False; Addr: string=''; HTTPRIO: THTTPRIO = nil): CursSoap;
function IdValutaAsString(aIdValuta : IDMoneda) : String;
function StringAsIdValuta(aStr : String) : IDMoneda;
function GetValoareCurs(lDate : TDateTime; lSynonim : String) : Currency;

implementation

uses
  Dialogs;


var
  lCurs : CursSoap;

function GetCursSoap(UseWSDL: Boolean; Addr: string; HTTPRIO: THTTPRIO): CursSoap;
const
  defWSDL = 'http://www.infovalutar.ro/curs.asmx?WSDL';
  defURL  = 'http://www.infovalutar.ro/curs.asmx';
  defSvc  = 'Curs';
  defPrt  = 'CursSoap';
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
    Result := (RIO as CursSoap);
    if UseWSDL then
    begin
      RIO.WSDLLocation := Addr;
      RIO.Service := defSvc;
      RIO.Port := defPrt;
    end else
      RIO.URL := Addr;
  finally
    if (Result = nil) and (HTTPRIO = nil) then
      RIO.Free;
  end;
end;


function IdValutaAsString(aIdValuta : IDMoneda) : String;
begin


  case aIdValuta of
    ROL : Result := 'ROL';
    EUR : Result := 'EUR';
    USD : Result := 'USD';
    XAU : Result := 'XAU';
    CAD : Result := 'CAD';
    GBP : Result := 'GBP';
    AUD : Result := 'AUD';
    DEM : Result := 'DEM';
    ATS : Result := 'ATS';
    BEF : Result := 'BEF';
    CHF : Result := 'CHF';
    CZK : Result := 'CZK';
    DKK : Result := 'DKK';
    EGP : Result := 'EGP';
    ESP : Result := 'ESP';
    FIM : Result := 'FIM';
    FRF : Result := 'FRF';
    GRD : Result := 'GRD';
    HUF : Result := 'HUF';
    IEP : Result := 'IEP';
    ITL : Result := 'ITL';
    JPY : Result := 'JPY';
    MDL : Result := 'MDL';
    NLG : Result := 'NLG';
    NOK : Result := 'NOK';
    PLN : Result := 'PLN';
    PTE : Result := 'PTE';
    SEK : Result := 'SEK';
    TRL : Result := 'TRL';
    TRY_ : Result := 'TRY';
    XDR : Result := 'XDR';
    XEU : Result := 'XEU';
    RON : Result := 'RON';
    end;
end;

function StringAsIdValuta(aStr : String) : IDMoneda;

begin
  if aStr = 'ROL' then Result := ROL else
  if aStr = 'EUR' then Result := EUR else
  if aStr = 'USD' then Result := USD else
  if aStr = 'XAU' then Result := XAU else
  if aStr = 'CAD' then Result := CAD else
  if aStr = 'GBP' then Result := GBP else
  if aStr = 'AUD' then Result := AUD else
  if aStr = 'DEM' then Result := DEM else
  if aStr = 'ATS' then Result := ATS else
  if aStr = 'BEF' then Result := BEF else
  if aStr = 'CHF' then Result := CHF else
  if aStr = 'CZK' then Result := CZK else
  if aStr = 'DKK' then Result := DKK else
  if aStr = 'EGP' then Result := EGP else
  if aStr = 'FIM' then Result := FIM else
  if aStr = 'FRF' then Result := FRF else
  if aStr = 'GRD' then Result := GRD else
  if aStr = 'HUF' then Result := HUF else
  if aStr = 'IEP' then Result := IEP else
  if aStr = 'ITL' then Result := ITL else
  if aStr = 'JPY' then Result := JPY else
  if aStr = 'MDL' then Result := MDL else
  if aStr = 'NLG' then Result := NLG else
  if aStr = 'NOK' then Result := NOK else
  if aStr = 'PLN' then Result := PLN else
  if aStr = 'SEK' then Result := SEK else
  if aStr = 'TRL' then Result := TRL else
  if aStr = 'TRY' then Result := TRY_ else
  if aStr = 'XDR' then Result := XDR else
  if aStr = 'XEU' then Result := XEU else
  if aStr = 'RON' then Result := RON;
end;


function GetValoareCurs(lDate : TDateTime; lSynonim : String) : Currency;
var
  FOldDecimalSeparator : Char;
begin
   if Trim(lSynonim) = '' then Result := 0
   else
     try
         try
           //requestu soap schimba decimal separatoru' in TypeTrans fara sa-l mai seteze la loc
           FOldDecimalSeparator := DecimalSeparator;
           Result := lCurs.GetValue(DateTimeToXSDateTime(lDate), lSynonim);
         finally
           DecimalSeparator := FOldDecimalSeparator;
         end;
         //if Result > 1000 then
         //Result := Result / 10000.0;
     except
        on E:Exception do begin
         Result := 0;
         MessageDlg('Nu s-a putut efectua conexiunea internet catre serviciul online de actualizare a valutei.' + E.Message,  mtError, [mbOK], 0);
        end;
     end;
end;

procedure InitializeCurs;
begin
  if lCurs = nil then
    lCurs := GetCursSoap();
end;

procedure UnInitializeCurs;
begin
  //automat ???
end;

initialization
  InvRegistry.RegisterInterface(TypeInfo(CursSoap), 'http://www.infovalutar.ro/', 'utf-8');
  InvRegistry.RegisterDefaultSOAPAction(TypeInfo(CursSoap), 'http://www.infovalutar.ro/%operationName%');
  InvRegistry.RegisterInvokeOptions(TypeInfo(CursSoap), ioDocument);
  RemClassRegistry.RegisterXSInfo(TypeInfo(IDMoneda), 'http://www.infovalutar.ro/', 'IDMoneda');
  RemClassRegistry.RegisterExternalPropName(TypeInfo(IDMoneda), 'TRY_', 'TRY');

  InitializeCurs;
finalization
  UnInitializeCurs;
end.
