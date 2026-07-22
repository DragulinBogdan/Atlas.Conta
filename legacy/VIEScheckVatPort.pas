// ************************************************************************ //
// The types declared in this file were generated from data read from the
// WSDL File described below:
// WSDL     : http://ec.europa.eu/taxation_customs/vies/api/checkVatPort?wsdl
// Encoding : UTF-8
// Codegen  : [wfDebug,wfUseSerializerClassForAttrs]
// Version  : 1.0
// (09.10.2009 15:16:44 - 1.33.2.5)
// ************************************************************************ //

unit VIEScheckVatPort;

interface

uses InvokeRegistry, SOAPHTTPClient, Types, XSBuiltIns;

type

  // ************************************************************************ //
  // The following types, referred to in the WSDL document are not being represented
  // in this file. They are either aliases[@] of other types represented or were referred
  // to but never[!] declared in the document. The types from the latter category
  // typically map to predefined/known XML or Borland types; however, they could also 
  // indicate incorrect WSDL documents that failed to declare or import a schema type.
  // ************************************************************************ //
  // !:string          - "http://www.w3.org/2001/XMLSchema"
  // !:date            - "http://www.w3.org/2001/XMLSchema"
  // !:boolean         - "http://www.w3.org/2001/XMLSchema"


  { "urn:ec.europa.eu:taxud:vies:services:checkVat:types" }
  matchCode = (_, _2, _3);

  companyTypeCode =  type WideString;      { "urn:ec.europa.eu:taxud:vies:services:checkVat:types" }

  // ************************************************************************ //
  // Namespace : urn:ec.europa.eu:taxud:vies:services:checkVat
  // transport : http://schemas.xmlsoap.org/soap/http
  // style     : document
  // binding   : checkVatBinding
  // service   : checkVatService
  // port      : checkVatPort
  // URL       : http://ec.europa.eu/taxation_customs/vies/api/checkVatPort
  // ************************************************************************ //
  checkVatPortType = interface(IInvokable)
  ['{0F901373-2432-32E2-C99D-95B53AE83C79}']
    procedure checkVat(const countryCode: WideString; const vatNumber: WideString; out outcountryCode: WideString; out outvatNumber: WideString; out requestDate: TXSDate; out valid: Boolean; out name: WideString; out address: WideString); stdcall;
{
    procedure checkVatApprox(const countryCode: WideString; const vatNumber: WideString; const traderName: WideString; const traderCompanyType: companyTypeCode; const traderStreet: WideString; const traderPostcode: WideString; const traderCity: WideString; const requesterCountryCode: WideString; const requesterVatNumber: WideString; out outcountryCode: WideString;
                             out outvatNumber: WideString; out requestDate: TXSDate; out valid: Boolean; out outtraderName: WideString; out outtraderCompanyType: companyTypeCode; out traderStreet: WideString; out traderPostcode: WideString; out traderCity: WideString; out traderNameMatch: matchCode;
                             out traderCompanyTypeMatch: matchCode; out traderStreetMatch: matchCode; out traderPostcodeMatch: matchCode; out traderCityMatch: matchCode; out requestIdentifier: WideString); stdcall;
}
  end;

function GetcheckVatPortType(UseWSDL: Boolean=System.False; Addr: string=''; HTTPRIO: THTTPRIO = nil): checkVatPortType;


implementation

function GetcheckVatPortType(UseWSDL: Boolean; Addr: string; HTTPRIO: THTTPRIO): checkVatPortType;
const
  defWSDL = 'http://ec.europa.eu/taxation_customs/vies/api/checkVatPort?wsdl';
  defURL  = 'http://ec.europa.eu/taxation_customs/vies/api/checkVatPort';
  defSvc  = 'checkVatService';
  defPrt  = 'checkVatPort';
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
    Result := (RIO as checkVatPortType);
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


initialization
  InvRegistry.RegisterInterface(TypeInfo(checkVatPortType), 'urn:ec.europa.eu:taxud:vies:services:checkVat', 'UTF-8');
  InvRegistry.RegisterDefaultSOAPAction(TypeInfo(checkVatPortType), '');
  InvRegistry.RegisterInvokeOptions(TypeInfo(checkVatPortType), ioDocument);
  RemClassRegistry.RegisterXSInfo(TypeInfo(companyTypeCode), 'urn:ec.europa.eu:taxud:vies:services:checkVat:types', 'companyTypeCode');
  RemClassRegistry.RegisterXSInfo(TypeInfo(matchCode), 'urn:ec.europa.eu:taxud:vies:services:checkVat:types', 'matchCode');
  RemClassRegistry.RegisterExternalPropName(TypeInfo(matchCode), '_', '');
  RemClassRegistry.RegisterExternalPropName(TypeInfo(matchCode), '_2', '1');
  RemClassRegistry.RegisterExternalPropName(TypeInfo(matchCode), '_3', '2');

end.
