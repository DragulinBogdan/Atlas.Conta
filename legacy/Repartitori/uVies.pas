// ************************************************************************ //
// The types declared in this file were generated from data read from the
// WSDL File described below:
// WSDL     : C:\Users\Cristi\Desktop\checkVatService.xml
//  >Import : C:\Users\Cristi\Desktop\checkVatService.xml>0
// Encoding : UTF-8
// Codegen  : [wfForceSOAP11+]
// Version  : 1.0
// (08.03.2012 14:58:37 - - $Rev: 45757 $)
// ************************************************************************ //

unit uVies;

interface

uses InvokeRegistry, SOAPHTTPClient, Types, XSBuiltIns;

const
  IS_OPTN = $0001;
  IS_NLBL = $0004;
  IS_REF  = $0080;


type

  // ************************************************************************ //
  // The following types, referred to in the WSDL document are not being represented
  // in this file. They are either aliases[@] of other types represented or were referred
  // to but never[!] declared in the document. The types from the latter category
  // typically map to predefined/known XML or Embarcadero types; however, they could also
  // indicate incorrect WSDL documents that failed to declare or import a schema type.
  // ************************************************************************ //
  // !:boolean         - "http://www.w3.org/2001/XMLSchema"[Gbl]
  // !:date            - "http://www.w3.org/2001/XMLSchema"[Gbl]
  // !:widestring          - "http://www.w3.org/2001/XMLSchema"[Gbl]

  checkVatResponse     = class;                 { "urn:ec.europa.eu:taxud:vies:services:checkVat:types"[Lit][GblElm] }
  checkVatApprox       = class;                 { "urn:ec.europa.eu:taxud:vies:services:checkVat:types"[Lit][GblElm] }
  checkVat             = class;                 { "urn:ec.europa.eu:taxud:vies:services:checkVat:types"[Lit][GblElm] }
  checkVatApproxResponse = class;               { "urn:ec.europa.eu:taxud:vies:services:checkVat:types"[Lit][GblElm] }

  { "urn:ec.europa.eu:taxud:vies:services:checkVat:types"[GblSmpl] }
  matchCode = (_1, _2);




  // ************************************************************************ //
  // XML       : checkVatResponse, global, <element>
  // Namespace : urn:ec.europa.eu:taxud:vies:services:checkVat:types
  // Serializtn: [xoLiteralParam]
  // Info      : Wrapper
  // ************************************************************************ //
  checkVatResponse = class(TRemotable)
  private
    FcountryCode: widestring;
    FvatNumber: widestring;
    FrequestDate: TXSDate;
    Fvalid: Boolean;
    Fname_: widestring;
    Fname__Specified: boolean;
    Faddress: widestring;
    Faddress_Specified: boolean;
    procedure Setname_(Index: Integer; const Awidestring: widestring);
    function  name__Specified(Index: Integer): boolean;
    procedure Setaddress(Index: Integer; const Awidestring: widestring);
    function  address_Specified(Index: Integer): boolean;
  public
    constructor Create; override;
    destructor Destroy; override;
  published
    property countryCode: widestring   read FcountryCode write FcountryCode;
    property vatNumber:   widestring   read FvatNumber write FvatNumber;
    property requestDate: TXSDate  read FrequestDate write FrequestDate;
    property valid:       Boolean  read Fvalid write Fvalid;
    property name_:       widestring   Index (IS_OPTN or IS_NLBL) read Fname_ write Setname_ stored name__Specified;
    property address:     widestring   Index (IS_OPTN or IS_NLBL) read Faddress write Setaddress stored address_Specified;
  end;

  companyTypeCode =  type widestring;      { "urn:ec.europa.eu:taxud:vies:services:checkVat:types"[GblSmpl] }


  // ************************************************************************ //
  // XML       : checkVatApprox, global, <element>
  // Namespace : urn:ec.europa.eu:taxud:vies:services:checkVat:types
  // Serializtn: [xoLiteralParam]
  // Info      : Wrapper
  // ************************************************************************ //
  checkVatApprox = class(TRemotable)
  private
    FcountryCode: widestring;
    FvatNumber: widestring;
    FtraderName: widestring;
    FtraderName_Specified: boolean;
    FtraderCompanyType: companyTypeCode;
    FtraderCompanyType_Specified: boolean;
    FtraderStreet: widestring;
    FtraderStreet_Specified: boolean;
    FtraderPostcode: widestring;
    FtraderPostcode_Specified: boolean;
    FtraderCity: widestring;
    FtraderCity_Specified: boolean;
    FrequesterCountryCode: widestring;
    FrequesterCountryCode_Specified: boolean;
    FrequesterVatNumber: widestring;
    FrequesterVatNumber_Specified: boolean;
    procedure SettraderName(Index: Integer; const Awidestring: widestring);
    function  traderName_Specified(Index: Integer): boolean;
    procedure SettraderCompanyType(Index: Integer; const AcompanyTypeCode: companyTypeCode);
    function  traderCompanyType_Specified(Index: Integer): boolean;
    procedure SettraderStreet(Index: Integer; const Awidestring: widestring);
    function  traderStreet_Specified(Index: Integer): boolean;
    procedure SettraderPostcode(Index: Integer; const Awidestring: widestring);
    function  traderPostcode_Specified(Index: Integer): boolean;
    procedure SettraderCity(Index: Integer; const Awidestring: widestring);
    function  traderCity_Specified(Index: Integer): boolean;
    procedure SetrequesterCountryCode(Index: Integer; const Awidestring: widestring);
    function  requesterCountryCode_Specified(Index: Integer): boolean;
    procedure SetrequesterVatNumber(Index: Integer; const Awidestring: widestring);
    function  requesterVatNumber_Specified(Index: Integer): boolean;
  public
    constructor Create; override;
  published
    property countryCode:          widestring           read FcountryCode write FcountryCode;
    property vatNumber:            widestring           read FvatNumber write FvatNumber;
    property traderName:           widestring           Index (IS_OPTN) read FtraderName write SettraderName stored traderName_Specified;
    property traderCompanyType:    companyTypeCode  Index (IS_OPTN) read FtraderCompanyType write SettraderCompanyType stored traderCompanyType_Specified;
    property traderStreet:         widestring           Index (IS_OPTN) read FtraderStreet write SettraderStreet stored traderStreet_Specified;
    property traderPostcode:       widestring           Index (IS_OPTN) read FtraderPostcode write SettraderPostcode stored traderPostcode_Specified;
    property traderCity:           widestring           Index (IS_OPTN) read FtraderCity write SettraderCity stored traderCity_Specified;
    property requesterCountryCode: widestring           Index (IS_OPTN) read FrequesterCountryCode write SetrequesterCountryCode stored requesterCountryCode_Specified;
    property requesterVatNumber:   widestring           Index (IS_OPTN) read FrequesterVatNumber write SetrequesterVatNumber stored requesterVatNumber_Specified;
  end;



  // ************************************************************************ //
  // XML       : checkVat, global, <element>
  // Namespace : urn:ec.europa.eu:taxud:vies:services:checkVat:types
  // Serializtn: [xoLiteralParam]
  // Info      : Wrapper
  // ************************************************************************ //
  checkVat = class(TRemotable)
  private
    FcountryCode: widestring;
    FvatNumber: widestring;
  public
    constructor Create; override;
  published
    property countryCode: widestring  read FcountryCode write FcountryCode;
    property vatNumber:   widestring  read FvatNumber write FvatNumber;
  end;



  // ************************************************************************ //
  // XML       : checkVatApproxResponse, global, <element>
  // Namespace : urn:ec.europa.eu:taxud:vies:services:checkVat:types
  // Serializtn: [xoLiteralParam]
  // Info      : Wrapper
  // ************************************************************************ //
  checkVatApproxResponse = class(TRemotable)
  private
    FcountryCode: widestring;
    FvatNumber: widestring;
    FrequestDate: TXSDate;
    Fvalid: Boolean;
    FtraderName: widestring;
    FtraderName_Specified: boolean;
    FtraderCompanyType: companyTypeCode;
    FtraderCompanyType_Specified: boolean;
    FtraderAddress: widestring;
    FtraderAddress_Specified: boolean;
    FtraderStreet: widestring;
    FtraderStreet_Specified: boolean;
    FtraderPostcode: widestring;
    FtraderPostcode_Specified: boolean;
    FtraderCity: widestring;
    FtraderCity_Specified: boolean;
    FtraderNameMatch: matchCode;
    FtraderNameMatch_Specified: boolean;
    FtraderCompanyTypeMatch: matchCode;
    FtraderCompanyTypeMatch_Specified: boolean;
    FtraderStreetMatch: matchCode;
    FtraderStreetMatch_Specified: boolean;
    FtraderPostcodeMatch: matchCode;
    FtraderPostcodeMatch_Specified: boolean;
    FtraderCityMatch: matchCode;
    FtraderCityMatch_Specified: boolean;
    FrequestIdentifier: widestring;
    procedure SettraderName(Index: Integer; const Awidestring: widestring);
    function  traderName_Specified(Index: Integer): boolean;
    procedure SettraderCompanyType(Index: Integer; const AcompanyTypeCode: companyTypeCode);
    function  traderCompanyType_Specified(Index: Integer): boolean;
    procedure SettraderAddress(Index: Integer; const Awidestring: widestring);
    function  traderAddress_Specified(Index: Integer): boolean;
    procedure SettraderStreet(Index: Integer; const Awidestring: widestring);
    function  traderStreet_Specified(Index: Integer): boolean;
    procedure SettraderPostcode(Index: Integer; const Awidestring: widestring);
    function  traderPostcode_Specified(Index: Integer): boolean;
    procedure SettraderCity(Index: Integer; const Awidestring: widestring);
    function  traderCity_Specified(Index: Integer): boolean;
    procedure SettraderNameMatch(Index: Integer; const AmatchCode: matchCode);
    function  traderNameMatch_Specified(Index: Integer): boolean;
    procedure SettraderCompanyTypeMatch(Index: Integer; const AmatchCode: matchCode);
    function  traderCompanyTypeMatch_Specified(Index: Integer): boolean;
    procedure SettraderStreetMatch(Index: Integer; const AmatchCode: matchCode);
    function  traderStreetMatch_Specified(Index: Integer): boolean;
    procedure SettraderPostcodeMatch(Index: Integer; const AmatchCode: matchCode);
    function  traderPostcodeMatch_Specified(Index: Integer): boolean;
    procedure SettraderCityMatch(Index: Integer; const AmatchCode: matchCode);
    function  traderCityMatch_Specified(Index: Integer): boolean;
  public
    constructor Create; override;
    destructor Destroy; override;
  published
    property countryCode:            widestring           read FcountryCode write FcountryCode;
    property vatNumber:              widestring           read FvatNumber write FvatNumber;
    property requestDate:            TXSDate          read FrequestDate write FrequestDate;
    property valid:                  Boolean          read Fvalid write Fvalid;
    property traderName:             widestring           Index (IS_OPTN or IS_NLBL) read FtraderName write SettraderName stored traderName_Specified;
    property traderCompanyType:      companyTypeCode  Index (IS_OPTN or IS_NLBL) read FtraderCompanyType write SettraderCompanyType stored traderCompanyType_Specified;
    property traderAddress:          widestring           Index (IS_OPTN) read FtraderAddress write SettraderAddress stored traderAddress_Specified;
    property traderStreet:           widestring           Index (IS_OPTN) read FtraderStreet write SettraderStreet stored traderStreet_Specified;
    property traderPostcode:         widestring           Index (IS_OPTN) read FtraderPostcode write SettraderPostcode stored traderPostcode_Specified;
    property traderCity:             widestring           Index (IS_OPTN) read FtraderCity write SettraderCity stored traderCity_Specified;
    property traderNameMatch:        matchCode        Index (IS_OPTN) read FtraderNameMatch write SettraderNameMatch stored traderNameMatch_Specified;
    property traderCompanyTypeMatch: matchCode        Index (IS_OPTN) read FtraderCompanyTypeMatch write SettraderCompanyTypeMatch stored traderCompanyTypeMatch_Specified;
    property traderStreetMatch:      matchCode        Index (IS_OPTN) read FtraderStreetMatch write SettraderStreetMatch stored traderStreetMatch_Specified;
    property traderPostcodeMatch:    matchCode        Index (IS_OPTN) read FtraderPostcodeMatch write SettraderPostcodeMatch stored traderPostcodeMatch_Specified;
    property traderCityMatch:        matchCode        Index (IS_OPTN) read FtraderCityMatch write SettraderCityMatch stored traderCityMatch_Specified;
    property requestIdentifier:      widestring           read FrequestIdentifier write FrequestIdentifier;
  end;


  // ************************************************************************ //
  // Namespace : urn:ec.europa.eu:taxud:vies:services:checkVat
  // transport : http://schemas.xmlsoap.org/soap/http
  // style     : document
  // use       : literal
  // binding   : checkVatBinding
  // service   : checkVatService
  // port      : checkVatPort
  // URL       : http://ec.europa.eu/taxation_customs/vies/services/checkVatService
  // ************************************************************************ //
  checkVatPortType = interface(IInvokable)
  ['{0F901373-2432-32E2-C99D-95B53AE83C79}']

    // Cannot unwrap:
    //     - More than one strictly out element was found
    function  checkVat(const parameters: checkVat): checkVatResponse; stdcall;

    // Cannot unwrap:
    //     - More than one strictly out element was found
    function  checkVatApprox(const parameters: checkVatApprox): checkVatApproxResponse; stdcall;
  end;

function GetcheckVatPortType(UseWSDL: Boolean=System.False; Addr: widestring=''; HTTPRIO: THTTPRIO = nil): checkVatPortType;


implementation
  uses SysUtils;

function GetcheckVatPortType(UseWSDL: Boolean; Addr: widestring; HTTPRIO: THTTPRIO): checkVatPortType;
const
  //defWSDL = 'C:\Users\Cristi\Desktop\checkVatService.xml';
  defWSDL = 'http://ec.europa.eu/taxation_customs/vies/checkVatService.wsdl';
  defURL  = 'http://ec.europa.eu/taxation_customs/vies/services/checkVatService';
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


constructor checkVatResponse.Create;
begin
  inherited Create;
  FSerializationOptions := [xoLiteralParam];
end;

destructor checkVatResponse.Destroy;
begin
  SysUtils.FreeAndNil(FrequestDate);
  inherited Destroy;
end;

procedure checkVatResponse.Setname_(Index: Integer; const Awidestring: widestring);
begin
  Fname_ := Awidestring;
  Fname__Specified := True;
end;

function checkVatResponse.name__Specified(Index: Integer): boolean;
begin
  Result := Fname__Specified;
end;

procedure checkVatResponse.Setaddress(Index: Integer; const Awidestring: widestring);
begin
  Faddress := Awidestring;
  Faddress_Specified := True;
end;

function checkVatResponse.address_Specified(Index: Integer): boolean;
begin
  Result := Faddress_Specified;
end;

constructor checkVatApprox.Create;
begin
  inherited Create;
  FSerializationOptions := [xoLiteralParam];
end;

procedure checkVatApprox.SettraderName(Index: Integer; const Awidestring: widestring);
begin
  FtraderName := Awidestring;
  FtraderName_Specified := True;
end;

function checkVatApprox.traderName_Specified(Index: Integer): boolean;
begin
  Result := FtraderName_Specified;
end;

procedure checkVatApprox.SettraderCompanyType(Index: Integer; const AcompanyTypeCode: companyTypeCode);
begin
  FtraderCompanyType := AcompanyTypeCode;
  FtraderCompanyType_Specified := True;
end;

function checkVatApprox.traderCompanyType_Specified(Index: Integer): boolean;
begin
  Result := FtraderCompanyType_Specified;
end;

procedure checkVatApprox.SettraderStreet(Index: Integer; const Awidestring: widestring);
begin
  FtraderStreet := Awidestring;
  FtraderStreet_Specified := True;
end;

function checkVatApprox.traderStreet_Specified(Index: Integer): boolean;
begin
  Result := FtraderStreet_Specified;
end;

procedure checkVatApprox.SettraderPostcode(Index: Integer; const Awidestring: widestring);
begin
  FtraderPostcode := Awidestring;
  FtraderPostcode_Specified := True;
end;

function checkVatApprox.traderPostcode_Specified(Index: Integer): boolean;
begin
  Result := FtraderPostcode_Specified;
end;

procedure checkVatApprox.SettraderCity(Index: Integer; const Awidestring: widestring);
begin
  FtraderCity := Awidestring;
  FtraderCity_Specified := True;
end;

function checkVatApprox.traderCity_Specified(Index: Integer): boolean;
begin
  Result := FtraderCity_Specified;
end;

procedure checkVatApprox.SetrequesterCountryCode(Index: Integer; const Awidestring: widestring);
begin
  FrequesterCountryCode := Awidestring;
  FrequesterCountryCode_Specified := True;
end;

function checkVatApprox.requesterCountryCode_Specified(Index: Integer): boolean;
begin
  Result := FrequesterCountryCode_Specified;
end;

procedure checkVatApprox.SetrequesterVatNumber(Index: Integer; const Awidestring: widestring);
begin
  FrequesterVatNumber := Awidestring;
  FrequesterVatNumber_Specified := True;
end;

function checkVatApprox.requesterVatNumber_Specified(Index: Integer): boolean;
begin
  Result := FrequesterVatNumber_Specified;
end;

constructor checkVat.Create;
begin
  inherited Create;
  FSerializationOptions := [xoLiteralParam];
end;

constructor checkVatApproxResponse.Create;
begin
  inherited Create;
  FSerializationOptions := [xoLiteralParam];
end;

destructor checkVatApproxResponse.Destroy;
begin
  SysUtils.FreeAndNil(FrequestDate);
  inherited Destroy;
end;

procedure checkVatApproxResponse.SettraderName(Index: Integer; const Awidestring: widestring);
begin
  FtraderName := Awidestring;
  FtraderName_Specified := True;
end;

function checkVatApproxResponse.traderName_Specified(Index: Integer): boolean;
begin
  Result := FtraderName_Specified;
end;

procedure checkVatApproxResponse.SettraderCompanyType(Index: Integer; const AcompanyTypeCode: companyTypeCode);
begin
  FtraderCompanyType := AcompanyTypeCode;
  FtraderCompanyType_Specified := True;
end;

function checkVatApproxResponse.traderCompanyType_Specified(Index: Integer): boolean;
begin
  Result := FtraderCompanyType_Specified;
end;

procedure checkVatApproxResponse.SettraderAddress(Index: Integer; const Awidestring: widestring);
begin
  FtraderAddress := Awidestring;
  FtraderAddress_Specified := True;
end;

function checkVatApproxResponse.traderAddress_Specified(Index: Integer): boolean;
begin
  Result := FtraderAddress_Specified;
end;

procedure checkVatApproxResponse.SettraderStreet(Index: Integer; const Awidestring: widestring);
begin
  FtraderStreet := Awidestring;
  FtraderStreet_Specified := True;
end;

function checkVatApproxResponse.traderStreet_Specified(Index: Integer): boolean;
begin
  Result := FtraderStreet_Specified;
end;

procedure checkVatApproxResponse.SettraderPostcode(Index: Integer; const Awidestring: widestring);
begin
  FtraderPostcode := Awidestring;
  FtraderPostcode_Specified := True;
end;

function checkVatApproxResponse.traderPostcode_Specified(Index: Integer): boolean;
begin
  Result := FtraderPostcode_Specified;
end;

procedure checkVatApproxResponse.SettraderCity(Index: Integer; const Awidestring: widestring);
begin
  FtraderCity := Awidestring;
  FtraderCity_Specified := True;
end;

function checkVatApproxResponse.traderCity_Specified(Index: Integer): boolean;
begin
  Result := FtraderCity_Specified;
end;

procedure checkVatApproxResponse.SettraderNameMatch(Index: Integer; const AmatchCode: matchCode);
begin
  FtraderNameMatch := AmatchCode;
  FtraderNameMatch_Specified := True;
end;

function checkVatApproxResponse.traderNameMatch_Specified(Index: Integer): boolean;
begin
  Result := FtraderNameMatch_Specified;
end;

procedure checkVatApproxResponse.SettraderCompanyTypeMatch(Index: Integer; const AmatchCode: matchCode);
begin
  FtraderCompanyTypeMatch := AmatchCode;
  FtraderCompanyTypeMatch_Specified := True;
end;

function checkVatApproxResponse.traderCompanyTypeMatch_Specified(Index: Integer): boolean;
begin
  Result := FtraderCompanyTypeMatch_Specified;
end;

procedure checkVatApproxResponse.SettraderStreetMatch(Index: Integer; const AmatchCode: matchCode);
begin
  FtraderStreetMatch := AmatchCode;
  FtraderStreetMatch_Specified := True;
end;

function checkVatApproxResponse.traderStreetMatch_Specified(Index: Integer): boolean;
begin
  Result := FtraderStreetMatch_Specified;
end;

procedure checkVatApproxResponse.SettraderPostcodeMatch(Index: Integer; const AmatchCode: matchCode);
begin
  FtraderPostcodeMatch := AmatchCode;
  FtraderPostcodeMatch_Specified := True;
end;

function checkVatApproxResponse.traderPostcodeMatch_Specified(Index: Integer): boolean;
begin
  Result := FtraderPostcodeMatch_Specified;
end;

procedure checkVatApproxResponse.SettraderCityMatch(Index: Integer; const AmatchCode: matchCode);
begin
  FtraderCityMatch := AmatchCode;
  FtraderCityMatch_Specified := True;
end;

function checkVatApproxResponse.traderCityMatch_Specified(Index: Integer): boolean;
begin
  Result := FtraderCityMatch_Specified;
end;

initialization
  { checkVatPortType }
  InvRegistry.RegisterInterface(TypeInfo(checkVatPortType), 'urn:ec.europa.eu:taxud:vies:services:checkVat', 'UTF-8');
  InvRegistry.RegisterDefaultSOAPAction(TypeInfo(checkVatPortType), '');
  InvRegistry.RegisterInvokeOptions(TypeInfo(checkVatPortType), ioDocument);
  InvRegistry.RegisterInvokeOptions(TypeInfo(checkVatPortType), ioLiteral);
  RemClassRegistry.RegisterXSClass(checkVatResponse, 'urn:ec.europa.eu:taxud:vies:services:checkVat:types', 'checkVatResponse');
  RemClassRegistry.RegisterExternalPropName(TypeInfo(checkVatResponse), 'name_', '[ExtName="name"]');
  RemClassRegistry.RegisterSerializeOptions(checkVatResponse, [xoLiteralParam]);
  RemClassRegistry.RegisterXSInfo(TypeInfo(companyTypeCode), 'urn:ec.europa.eu:taxud:vies:services:checkVat:types', 'companyTypeCode');
  RemClassRegistry.RegisterXSClass(checkVatApprox, 'urn:ec.europa.eu:taxud:vies:services:checkVat:types', 'checkVatApprox');
  RemClassRegistry.RegisterSerializeOptions(checkVatApprox, [xoLiteralParam]);
  RemClassRegistry.RegisterXSClass(checkVat, 'urn:ec.europa.eu:taxud:vies:services:checkVat:types', 'checkVat');
  RemClassRegistry.RegisterSerializeOptions(checkVat, [xoLiteralParam]);
  RemClassRegistry.RegisterXSInfo(TypeInfo(matchCode), 'urn:ec.europa.eu:taxud:vies:services:checkVat:types', 'matchCode');
  RemClassRegistry.RegisterExternalPropName(TypeInfo(matchCode), '_1', '1');
  RemClassRegistry.RegisterExternalPropName(TypeInfo(matchCode), '_2', '2');
  RemClassRegistry.RegisterXSClass(checkVatApproxResponse, 'urn:ec.europa.eu:taxud:vies:services:checkVat:types', 'checkVatApproxResponse');
  RemClassRegistry.RegisterSerializeOptions(checkVatApproxResponse, [xoLiteralParam]);

end.
