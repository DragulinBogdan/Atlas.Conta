unit uMFinante;

interface

uses Classes, Controls, Windows, SysUtils, ExtActns, Dialogs,
  HTMLParser, Formatter, DOMCore, IdHTTP, IdCookieManager, Forms,
  frmMFinanteQuestionUnit;

type
  TRetriveFinanteInfo = class(TObject)
  private
    HtmlParser : THtmlParser;
    Formatter: TBaseFormatter;
    TempFileName : String;
    HtmlDoc: TDocument;
    CookieM : TIdCookieManager;
    HTTP : TidHttp;
    function DecodeString(aString: string): string;
  protected
    FCapthaForm : TfrmMFinanteQuestion;
    function Download_HTM(const sURL, sLocalFileName : string): Boolean;
    function NewDownload_HTM(sURL, sLocalFileName : string; const Query : String = ''; const usePost : Boolean = False): Boolean;
    procedure GetInformatii;
  public
    CodFiscal : String;
    NumeRepartitor : String;
    Adresa : String;
    Judet : String;
    NrRegComert : String;
    CodPostal : String;
    Telefon : String;
    Fax : String;
    IsError : Boolean;
    ProcessList : TStringList;
    JudeteList : TStringList;
    NumeList : TStringList;
    constructor Create;
    destructor Destroy; override;
    procedure ProcessCodFiscal(aCodFiscal : String);
    procedure ProcessNume(aNume : String; aJudet : String);
    function GetDescriere : string;
  end;

implementation


function WideStringToString(const ws: WideString; const codePage: Word = 0): AnsiString;
var
  l: integer;
begin
  if ws = '' then
    Result := ''
  else
  begin
    l := WideCharToMultiByte(codePage,
      WC_COMPOSITECHECK or WC_DISCARDNS or WC_SEPCHARS or WC_DEFAULTCHAR,
      @ws[1], - 1, nil, 0, nil, nil);
    SetLength(Result, l - 1);
    if l > 1 then
      WideCharToMultiByte(codePage,
        WC_COMPOSITECHECK or WC_DISCARDNS or WC_SEPCHARS or WC_DEFAULTCHAR,
        @ws[1], - 1, @Result[1], l - 1, nil, nil);
  end;
end; { WideStringToString }

{ TRetriveFinanteInfo }

constructor TRetriveFinanteInfo.Create;
var Buffer: array[1..255] of Char;
begin
  GetTempPath(255, @Buffer[1]);
  TempFileName := StrPas(PChar(@Buffer[1]));
  if GetTempFileName(@TempFileName[1], 'html', 0, @Buffer[1]) <> 0 then TempFileName :=  StrPas(PChar(@Buffer[1]))
                                                                   else TempFileName := 'c:\1.html';
  HtmlParser := THtmlParser.Create;
  ProcessList := TStringList.Create;
  Formatter := TTextFormatter.Create;

  JudeteList := TStringList.Create;
  JudeteList.Clear;
  NumeList := TStringList.Create;
  NumeList.Clear;
end;

destructor TRetriveFinanteInfo.Destroy;
begin
   if CookieM<> nil then CookieM.Free;
   Formatter.Free;
   HtmlDoc.Free;
   HtmlParser.Free;
   ProcessList.Free;
   JudeteList.Free;
   NumeList.Free;
   if HTTP <> nil then HTTP.Free;
   if FCapthaForm <> nil then FreeAndNil(FCapthaForm);
   inherited Destroy;
end;

function TRetriveFinanteInfo.Download_HTM(const sURL,
  sLocalFileName: string): Boolean;
begin
  Result := True;
  with TDownLoadURL.Create(nil) do
  try
    URL:=sURL;
    Filename:=sLocalFileName;
    try
      ExecuteTarget(nil) ;
    except
      Result:=False
    end;
  finally
    Free;
  end;
end;

function TRetriveFinanteInfo.GetDescriere: string;
begin
  Result := NumeRepartitor + #13#10 +
    CodFiscal + #13#10 +
    Adresa + #13#10 +
    Judet + #13#10 +
    NrRegComert + #13#10 +
    CodPostal+ #13#10 +
    Telefon + #13#10 +
    Fax;
end;

function TRetriveFinanteInfo.DecodeString(aString : string) : string;
begin
  Result := WideStringToString(UTF8Decode(aString));
end;

procedure TRetriveFinanteInfo.GetInformatii;
var
  I : Integer;
begin
  if ProcessList.Count = 0 then Exit;
  with Self do begin

    I := ProcessList.IndexOf('Denumire platitor:');
    if I > - 1 then begin
      I := I + 2;
      NumeRepartitor := DecodeString(ProcessList[I]);
      IsError := False;
    end;

    I := ProcessList.IndexOf('Adresa:');
    if I > - 1 then begin
      I := I + 2;
      Adresa := DecodeString(ProcessList[I]);
      IsError := False;
    end;

    I := ProcessList.IndexOf('Judetul:');
    if I > - 1 then begin
      I := I + 2;
      Judet := DecodeString(ProcessList[I]);
      IsError := False;
    end;

    I := ProcessList.IndexOf('Numar de inmatriculare la Registrul Comertului:');
    if I > - 1 then begin
      I := I + 2;
      NrRegComert := DecodeString(ProcessList[I]);
      IsError := False;
    end;

    I := ProcessList.IndexOf('Codul postal:');
    if I > - 1 then begin
      I := I + 2;
      CodPostal := DecodeString(ProcessList[I]);
      IsError := False;
    end;

    I := ProcessList.IndexOf('Telefon:');
    if I > - 1 then begin
      I := I + 2;
      Telefon := DecodeString(ProcessList[I]);
      IsError := False;
    end;

    I := ProcessList.IndexOf('Fax:');
    if I > - 1 then begin
      I := I + 2;
      Fax := DecodeString(ProcessList[I]);
      IsError := False;
    end;
  end;
end;

function TRetriveFinanteInfo.NewDownload_HTM(sURL, sLocalFileName: string;
  const Query: String; const usePost: Boolean): Boolean;
var
  Stream : TMemoryStream;
  PostStream : TMemoryStream;
begin
  try
     try
         PostStream := nil;
         Stream := TMemoryStream.Create;
         if HTTP = nil then begin
           HTTP := TIdHTTP.Create(nil);
           if CookieM = nil then
             CookieM := TIdCookieManager.Create(nil);
           //CookieM.CookieCollection.Clear;
           HTTP.HandleRedirects := True;
           HTTP.AllowCookies := True;
           HTTP.CookieManager := CookieM;
           HTTP.ReadTimeout := 30000;
           HTTP.ConnectTimeout := 20000;
         end;
     with HTTP do
     begin
         Request.UserAgent := 'MSIE 8.0';
         Request.Connection := 'Keep-Alive';
         Request.ProxyConnection := 'Keep-Alive';
         Request.Referer := 'http://www.mfinante.ro';
         //Request.CacheControl := 'no-cache';  //this force use no-cache
     end;
         if usePost then begin
           PostStream := TMemoryStream.Create;
           PostStream.WriteBuffer(Query[1], Length(Query));
           PostStream.Position := 0;
           HTTP.Request.ContentType := 'application/x-www-form-urlencoded';
           HTTP.Post(sURL, PostStream, Stream);
         end
         else begin
           if Query <> '' then
             sURL := sURL + '?' + Query;
           HTTP.Get(sURL,Stream);
         end;
         if FileExists(sLocalFileName) then DeleteFile(sLocalFileName);
         Stream.SaveToFile(sLocalFileName);
     except
         on E:Exception do
         begin
             IsError := True;
              //error handling
         end;
     end;
  finally
     if PostStream <> nil then PostStream.Free;
     Stream.Free;
  end;
end;

procedure TRetriveFinanteInfo.ProcessCodFiscal(aCodFiscal: String);
var
  downUrl,downQuery : String;
  S: AnsiString;
  F: TStream;
  lCaptcha : String;
  lWithCaptcha, lWithLock : Boolean;
begin
  try
    IsError := False;
    downUrl := 'http://www.mfinante.ro/agenticod.html';
    downQuery := 'pagina=domenii';
    NewDownload_HTM(downUrl, TempFileName, downQuery);
    S := '';
    F := TFileStream.Create(TempFileName, fmOpenRead);
    try
      SetLength(S, F.Size);
      F.Read(Pointer(S)^, F.Size*SizeOf(AnsiChar))
    finally
      F.Free
    end;

    lWithCaptcha := (Pos(('kaptcha.jpg'), S) > 0);
    repeat
      if lWithCaptcha then begin
        downUrl := 'http://www.mfinante.ro/kaptcha.jpg';
        downQuery := '';
        NewDownload_HTM(downUrl, TempFileName+ '.jpg');
        if FCapthaForm = nil then
           FCapthaForm := TfrmMFinanteQuestion.Create(nil);
        with FCapthaForm do
        try
          edCaptcha.Text := '';
          edImage.Picture.LoadFromFile(TempFileName+ '.jpg');
          ShowModal;
          if ModalResult = mrOk then
            lCaptcha :=  edCaptcha.Text;
        finally
          //Free;
        end;
      end;

      downUrl := 'http://www.mfinante.ro/infocodfiscal.html';
      if lWithCaptcha then
        downQuery := 'pagina=domenii&cod=' +Trim(aCodFiscal) + '&captcha='+lCaptcha+'&B1=VIZUALIZARE'
      else
        downQuery := 'pagina=domenii&cod=' +Trim(aCodFiscal) + '&B1=VIZUALIZARE';
      //NewDownload_HTM(downUrl, TempFileName);
      NewDownload_HTM(downUrl, TempFileName, downQuery, False);
      if IsError then Exit;
      S := '';
      F := TFileStream.Create(TempFileName, fmOpenRead);
      try
        SetLength(S, F.Size);
        F.Read(Pointer(S)^, F.Size*SizeOf(AnsiChar))
      finally
        F.Free
      end;
      lWithCaptcha := (Pos(('kaptcha.jpg'), S) > 0);
      lWithLock := (Pos('Server ocupat, va rugam reveniti', S) > 0);
      if lWithLock then
        CookieM.CookieCollection.Clear;
    until not lWithCaptcha and not lWithLock;

    if Pos('Datele se actualizeaz&#259', S) > 0 then
      ShowMessage('Datele se actualizeaza si nu se pot procura datele !')
    else begin
      try
        HtmlDoc := HtmlParser.parseString(S);
        ProcessList.Text := Formatter.getText(HtmlDoc);
      finally
      end;

      while (ProcessList.Count > 0)  and ( pos('AGENTUL ECONOMIC CU CODUL UNIC DE IDENTIFICARE',  ProcessList[0]) = 0 )do
        ProcessList.Delete(0);

      CodFiscal := Trim(aCodFiscal);
      IsError := True;
      NumeRepartitor := '';
      Adresa := '';
      Judet := '';
      NrRegComert := '';
      CodPostal := '';
      Telefon := '';
      Fax := '';
      GetInformatii;
    end;
  finally
    if FileExists(TempFileName) then DeleteFile(TempFileName);
  end;
end;

procedure TRetriveFinanteInfo.ProcessNume(aNume, aJudet: String);
var
  downUrl,downQuery : String;
  S: AnsiString;
  F: TStream;
  I : Integer;
begin
  try
    IsError := False;
    downUrl := 'http://www.mfinante.ro/agentinume.html';
    downQuery := 'pagina=domenii';
    NewDownload_HTM(downUrl, TempFileName, downQuery);

    S := '';
    F := TFileStream.Create(TempFileName, fmOpenRead);
    try
      SetLength(S, F.Size);
      F.Read(Pointer(S)^, F.Size*SizeOf(AnsiChar))
    finally
      F.Free
    end;

    if (JudeteList.Count = 0) or (aJudet = '') then begin
      JudeteList.Clear;
{      try
        HtmlDoc := HtmlParser.parseString(S);
        ProcessList.Text := Formatter.getText(HtmlDoc);
      finally
      end;
}
      ProcessList.Text :=  S;

      while (ProcessList.Count > 0)  and ( pos('<select name="judet" size="1" class="form2">',  ProcessList[0]) = 0 )do
        ProcessList.Delete(0);
      if ProcessList.Count > 0 then begin
        ProcessList.Delete(0);
        while (ProcessList.Count > 0)  and (Pos('</select>', ProcessList[0]) = 0) do begin
          S := Trim(ProcessList[0]);
          S := Trim(StringReplace(S, '<option value="', '', []));
          S := Trim(StringReplace(S, '</option>', '', []));
          S := Trim(StringReplace(S, '">', '=', []));
          if (Length(S) > 0) and not(S[1]='=') then JudeteList.Add(S);
          ProcessList.Delete(0);
        end;
      end;
    end;

    if (aJudet = '') or (aNume = '') then Exit;

    downUrl := 'http://www.mfinante.ro/numeCod.html';
    downQuery := 'pagina=domenii&judet='+ Trim(aJudet) + '&name=' +Trim(aNume)+'&submit=VIZUALIZARE';

    NewDownload_HTM(downUrl, TempFileName, downQuery, False);
    if IsError then Exit;
    S := '';
    F := TFileStream.Create(TempFileName, fmOpenRead);
    try
      SetLength(S, F.Size);
      F.Read(Pointer(S)^, F.Size*SizeOf(AnsiChar))
    finally
      F.Free
    end;
    ProcessList.Text := S;
    S := StringReplace(S, '<INPUT type="submit" name="submit" value="', '<td>',  [rfReplaceAll]);
    S := StringReplace(S, '"></INPUT>', '</td>', [rfReplaceAll]);

    try
      HtmlDoc := HtmlParser.parseString(S);
      ProcessList.Text := Formatter.getText(HtmlDoc);
    finally
    end;

    while (ProcessList.Count > 0)  and ( pos('Cod Unic de Identificare',  ProcessList[0]) = 0 )do
      ProcessList.Delete(0);

    while (ProcessList.Count > 0)  and (Pos('agentinume.html', ProcessList[ProcessList.Count - 1])= 0 ) do
      ProcessList.Delete(ProcessList.Count - 1);


    for I := ProcessList.Count - 1 downto 0 do
      if ( Trim(ProcessList[I]) = '')
        or (pos('Cod Unic de Identificare',  ProcessList[I]) > 0)
        or (pos('Denumirea agentului economic',  ProcessList[I]) > 0)
      then ProcessList.Delete(I);

    while (ProcessList.Count > 0)  and (Pos('agentinume.html', ProcessList[ProcessList.Count - 1]) > 0 ) do
      ProcessList.Delete(ProcessList.Count - 1);

    NumeList.Clear;
    I := 0;
    while I < ProcessList.Count do begin
      NumeList.Add(DecodeString(ProcessList[I]) + '=' + DecodeString(ProcessList[I+1]));
      I := I + 2;
    end;
  finally
    if FileExists(TempFileName) then DeleteFile(TempFileName);
  end;

end;

end.
