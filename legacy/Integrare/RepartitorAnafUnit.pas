unit RepartitorAnafUnit;

interface

uses
  SysUtils, Registry, idURI, Classes, DBXJson, Dialogs, WinInet;

function GetRepartitorInfo(const ACui: Integer;out lscpTVA: Boolean): TJsonObject; overload;

implementation

uses
  Windows, ZeosDBUtile, ATSZDBUtils, IdTCPConnection, IdSSLOpenSSL, IdTCPClient, IdHTTP;

procedure SetUpProxy(AHttp: TIdHTTP);
var
  lRegistry: TRegistry;
  lURI: TIdURI;
  lProxyList: TStringList;
  lProxyURI: String;
  lProtIndex: Integer;
  lIsSecure: Boolean;
   lscpTVA: Boolean;
begin
  lIsSecure := Assigned(AHttp.IOHandler) and (AHttp.IOHandler is TIdSSLIOHandlerSocketOpenSSL);
  lRegistry := TRegistry.Create(KEY_READ);
  try
    lRegistry.RootKey := HKEY_CURRENT_USER;
    if lRegistry.OpenKeyReadOnly('Software\Microsoft\Windows\CurrentVersion\Internet Settings') then
    begin
      if lRegistry.ReadInteger('ProxyEnable') = 1 then
      begin
        lProxyURI := lRegistry.ReadString('ProxyServer');
        lProxyList := TStringList.Create;
        try
          lProxyList.Delimiter := ';';
          lProxyList.DelimitedText := lProxyURI;
          lProxyList.CaseSensitive := False;
          if lProxyList.Count > 0 then
          begin
            if lIsSecure then
              lProtIndex := lProxyList.IndexOfName('https')
            else
              lProtIndex := lProxyList.IndexOfName('http');
            if lProtIndex = -1 then
              lProtIndex := 0;
            lProxyURI := lProxyList[lProtIndex];
            lProxyURI := StringReplace(lProxyURI, '=', '://', [rfReplaceAll]);
          end;
        finally
          lProxyList.Free;
        end;
        lURI := TIdURI.Create(lProxyURI);
        try
          AHttp.ProxyParams.ProxyPort := StrToIntDef(lURI.Port, 0);
          AHttp.ProxyParams.ProxyServer := lURI.Host;
          AHttp.ProxyParams.ProxyUsername := lURI.Username;
          AHttp.ProxyParams.ProxyPassword := lURI.Password;
        finally
          lURI.Free;
        end;
      end;
    end;
  finally
    lRegistry.Free;
  end;
end;

type
  TVerifyPeerClass = class
    function DoLocalVerifyPeer(Certificate: TIdX509; AOk: Boolean; ADepth, AError: Integer): Boolean;
  end;

function TVerifyPeerClass.DoLocalVerifyPeer(Certificate: TIdX509; AOk: Boolean; ADepth, AError: Integer): Boolean;
begin
  Result := True;
end;

var
  GlobalSSLHandler: TIdSSLIOHandlerSocketOpenSSL;

procedure InitGlobalSSLHandler;
begin
  if not Assigned(GlobalSSLHandler) then
  begin
   GlobalSSLHandler := TIdSSLIOHandlerSocketOpenSSL.Create(nil);
    GlobalSSLHandler.SSLOptions.Method := sslvTLSv1_2;
    GlobalSSLHandler.SSLOptions.SSLVersions := [sslvTLSv1_2];
    GlobalSSLHandler.SSLOptions.Mode := sslmUnassigned;
    GlobalSSLHandler.SSLOptions.VerifyMode := [];
    GlobalSSLHandler.SSLOptions.VerifyDepth := 0;
    GlobalSSLHandler.OnVerifyPeer := TVerifyPeerClass.Create.DoLocalVerifyPeer;
  end;
end;

function GetRepartitorInfo(const ACui: Integer; out lscpTVA: Boolean): TJsonObject; overload;
var
  lHttp: TIdHTTP;
  PostData, ResponseText: string;
  RequestStream, ResponseStream: TStringStream;
  JSONObj, lFirstEntry, lDateGenerale, lTVA, lSplitTVA, lTvaIncasare: TJSONObject;
  lRepFound: TJSONArray;
  lDenumire, lAdresa, lCui, lnrRegCom, lJudet, lLocalitate: string;
   lstatusSplitTVA, lstatusTvaIncasare: string;
  FoundValue, DateGeneraleValue, TVAValue, SplitTVAValue, ScopTVAValue, TvaIncasareValue: TJSONPair;

begin
//  ShowMessage('Incepe interogarea ANAF');
  Result := nil;
   lscpTVA := False;
  PostData := '[{"cui": ' + IntToStr(ACui) + ', "data": "' + FormatDateTime('yyyy-mm-dd', Date) + '"}]';
//  ShowMessage('Trimitem JSON: ' + PostData);

  InitGlobalSSLHandler;

  lHttp := TIdHTTP.Create(nil);
  try
    lHttp.IOHandler := GlobalSSLHandler;
  lHttp.Request.ContentType := 'application/json';
  lHttp.Request.UserAgent := 'PostmanRuntime/7.43.0';
  lHttp.Request.Accept := 'application/json';
  lHttp.Request.Connection := 'keep-alive';
  lHttp.Request.AcceptEncoding := 'gzip, deflate, br';
  lHttp.Request.AcceptLanguage := 'ro-RO,ro;q=0.9,en-US;q=0.8,en;q=0.7';
  lHttp.HandleRedirects := True;
  lHttp.ReadTimeout := 15000;
  lHttp.ConnectTimeout := 15000;

    RequestStream := TStringStream.Create(PostData, TEncoding.UTF8);
    ResponseStream := TStringStream.Create;
    try
      try
        lHttp.Post('https://webservicesp.anaf.ro/api/PlatitorTvaRest/v9/tva', RequestStream, ResponseStream);
        ResponseText := ResponseStream.DataString;
//        ShowMessage('Raspuns JSON: ' + Copy(ResponseText, 1, 900));

        JSONObj := TJSONObject.ParseJSONValue(ResponseText) as TJSONObject;
        if JSONObj = nil then
          raise Exception.Create('Eroare: JSON-ul returnat nu este valid!');

        FoundValue := JSONObj.Get('found');
        if not Assigned(FoundValue) or not (FoundValue.JsonValue is TJSONArray) then
          raise Exception.Create('Eroare: Campul "found" nu exista sau nu este un array!');

        lRepFound := TJSONArray(FoundValue.JsonValue);
        if lRepFound.Size = 0 then
          raise Exception.Create('Atentie: JSON-ul returnat de ANAF nu contine date!');

        lFirstEntry := lRepFound.Get(0) as TJSONObject;
        if lFirstEntry = nil then
          raise Exception.Create('Eroare: Nu s-a putut extrage primul obiect din JSON!');

        DateGeneraleValue := lFirstEntry.Get('date_generale');
        if Assigned(DateGeneraleValue) and (DateGeneraleValue.JsonValue is TJSONObject) then
        begin
          lDateGenerale := TJSONObject(DateGeneraleValue.JsonValue);

          if Assigned(lDateGenerale.Get('denumire')) then
            lDenumire := lDateGenerale.Get('denumire').JsonValue.Value
          else
            lDenumire := 'N/A';

          if Assigned(lDateGenerale.Get('adresa')) then
            lAdresa := lDateGenerale.Get('adresa').JsonValue.Value
          else
            lAdresa := 'N/A';

          if Assigned(lDateGenerale.Get('cui')) then
            lCui := lDateGenerale.Get('cui').JsonValue.Value
          else
            lCui := 'N/A';

          if Assigned(lDateGenerale.Get('nrRegCom')) then
            lnrRegCom := lDateGenerale.Get('nrRegCom').JsonValue.Value
          else
            lnrRegCom := 'N/A';

          if Assigned(lDateGenerale.Get('judet')) then
            lJudet := lDateGenerale.Get('judet').JsonValue.Value
          else
            lJudet := 'N/A';

          if Assigned(lDateGenerale.Get('localitate')) then
            lLocalitate := lDateGenerale.Get('localitate').JsonValue.Value
          else
            lLocalitate := 'N/A';
             if Assigned(lDateGenerale.Get('oras')) then
            lLocalitate := lDateGenerale.Get('oras').JsonValue.Value
          else
            lLocalitate := 'N/A';


        end;


ScopTVAValue := lFirstEntry.Get('inregistrare_scop_Tva');
if Assigned(ScopTVAValue) and (ScopTVAValue.JsonValue is TJSONObject) then
begin
  if Assigned(TJSONObject(ScopTVAValue.JsonValue).Get('scpTVA')) then
  begin
    if TJSONObject(ScopTVAValue.JsonValue).Get('scpTVA').JsonValue is TJSONTrue then
      lscpTVA := True
    else
      lscpTVA := False;
  end
  else
    lscpTVA := False;
end
else
  lscpTVA := False;




SplitTVAValue := lFirstEntry.Get('inregistrare_SplitTVA');
if Assigned(SplitTVAValue) and (SplitTVAValue.JsonValue is TJSONObject) then
begin
  if Assigned(TJSONObject(SplitTVAValue.JsonValue).Get('statusSplitTVA')) then
  begin
    if TJSONObject(SplitTVAValue.JsonValue).Get('statusSplitTVA').JsonValue is TJSONTrue then
      lstatusSplitTVA := 'True'
    else if TJSONObject(SplitTVAValue.JsonValue).Get('statusSplitTVA').JsonValue is TJSONFalse then
      lstatusSplitTVA := 'False'
    else
      lstatusSplitTVA := 'N/A';
  end
  else
    lstatusSplitTVA := 'N/A';
end
else
  lstatusSplitTVA := 'N/A';


TvaIncasareValue := lFirstEntry.Get('inregistrare_RTVAI');
if Assigned(TvaIncasareValue) and (TvaIncasareValue.JsonValue is TJSONObject) then
begin
  if Assigned(TJSONObject(TvaIncasareValue.JsonValue).Get('statusTvaIncasare')) then
  begin
    if TJSONObject(TvaIncasareValue.JsonValue).Get('statusTvaIncasare').JsonValue is TJSONTrue then
      lstatusTvaIncasare := 'True'
    else if TJSONObject(TvaIncasareValue.JsonValue).Get('statusTvaIncasare').JsonValue is TJSONFalse then
      lstatusTvaIncasare := 'False'
    else
      lstatusTvaIncasare := 'N/A';
  end
  else
    lstatusTvaIncasare := 'N/A';
end
else
  lstatusTvaIncasare := 'N/A';




         Result := JSONObj;
         Exit;
      except
        on E: Exception do
          ShowMessage('Eroare: ' + E.Message);
      end;
    finally
      FreeAndNil(RequestStream);
      FreeAndNil(ResponseStream);
    end;
  finally
    FreeAndNil(lHttp);
  end;
end;

end.
