unit ATSZDBUtils;

interface

uses
  CommonDBVar, Messages, Windows, Buttons, Stdctrls, Forms, Db, SysUtils, ZDataSet, Classes, Registry,
  ZConnection;

type TdxADOOnValidate = procedure (Sender: TDataSet; UpdateKind: TUpdateStatus; var Action:TDataAction) of Object;

procedure GetHeaderSocietate;
function GetQryDescription(aQryName : string ) : string;
procedure OpenDataModule(DataModul: TComponent; Config: Boolean=True);

procedure ConfigDB(ADataBase:TZConnection);
function GetCurentDB(aData: TZConnection;AList: TStringList): Integer;
function Login: Integer;

function DecryptValididationKey(const Passwd, KryptGuid : String) : String;
function ConfirmaParola: Boolean;

function GetLastUserEntry : String;
procedure SetLastUserEntry(NumeLogin : String);

function DBGetSetare(const AParamName: String): Variant; overload;
function DBGetSetare(const AParamName: String; const ADefaValue: Variant): Variant; overload;

implementation

uses
  ZeosDBUtile, Variants, dxmdaset, Dialogs, Controls, LogUnit, LoginFrmUnit, Graphics, GraphicEx,
  IniFiles, atlasStartUp, DCPUtils;

var
  CurrentZDataSet       : TZConnection;
  FDBStateReadOnly      : Boolean = False;
  FLocalIsReadOnly      : Boolean = False;
  GlobalQryDescription  : TStringList;
  szProtocol            : String = '';
  szDefProtocol         : String = 'mssql';
  szProviderName        : String = 'SQLOLEDB.1';
  szDefaultConn         : String = '';
  szSSPI                : Boolean = False;

function DBGetSetare(const AParamName: String; const ADefaValue: Variant): Variant;
begin
  Result := DBGetScallarFmt('select dbo.fnParamSoc(1, %s, null)', [ValueToStr(AParamName)]);
  if ValueHasValue(ADefaValue) and not ValueHasValue(Result) then
    Result := ADefaValue;
end;

function DBGetSetare(const AParamName: String): Variant;
begin
  Result := DBGetSetare(AParamName, Null);
end;

function GetTempQry(AReadOnly : Boolean = True): TZQuery;
begin
  Result := TZQuery.Create(CurrentZDataSet);

  with Result do begin
    Connection     := CurrentZDataSet;
    ReadOnly := AReadOnly;
  end;
end;

function IsReadOnlyDB: Boolean;

  function GetValue(const aSqlOrder: String): Variant;
  var
    lDataSet: TDataSet;
  begin
    lDataSet := DBNewQuery(aSqlOrder);
    try
      lDataSet.Open;
      Result := lDataSet.Fields[1].Value;
    finally
      lDataSet.Free;
    end;
  end;

begin
  if not FDBStateReadOnly then begin
    if DBProcExists('sp_dboption') then
      FLocalIsReadOnly :=
        AnsiCompareText(GetValue('declare @db varchar(200) set @db = db_name() exec sp_dboption @db, ''read only'''), 'ON') = 0
    else
      FLocalIsReadOnly := ValueIsTrue(GetValue('select name, is_read_only from sys.databases where name = '''+CurrentZDataSet.Database+''''));
    FDBStateReadOnly := True;
  end;
  Result := FLocalIsReadOnly;
end;

function GetNextId(aTbl: String): Integer;
begin
  Result := -1;
  with GetTempQry do
    try
       ParamCheck := False;
       Sql.Add('EXEC SP_GET_NEXT_VALUE '+QuotedStr(aTbl));
       Open;
       Result := Fields[0].AsInteger;
    finally
       Free;
    end;
end;

function GetADOServerDate: TDateTime;
begin
  with GetTempQry do
    try
       Sql.Add('SELECT GETDATE() AS DATA');
       Open;
       Result := Fields[0].AsDateTime;
    finally
       Free;
    end;
end;

function MD5Print(const AString: String): String;
begin
  Result := UpperCase(StringToHash('MD5', AnsiString(AString)));
end;

function ConfirmaParola: Boolean;
var
  aFrmLogin : TLoginFrm;
begin
  aFrmLogin := TLoginFrm.Create(nil);
  try
     aFrmLogin.Utilizator.Text := NumeLogin;
     aFrmLogin.Utilizator.Enabled := False;
     Result := False;
     while (not Result) and (aFrmLogin.ShowModal = mrOk) do begin
       Result := MD5Print(aFrmLogin.Parola.Text) = ParolaUser;
       if not Result then
          ShowMessage('Parola introdusa nu este corecta !');
     end;
  finally
     aFrmLogin.Free;
  end;
end;

function Login: Integer;
var
  lFrmLogin: TLoginFrm;

  procedure InitForm;
  begin
    if bIsParented then
      lFrmLogin.FormStyle     := fsStayOnTop;
    lFrmLogin.NumeAplicatie.Caption := Copy(AppName, 1, Length(AppName)-1);
    if szPrevUserName = '' then
      szPrevUserName := GetLastUserEntry;
    lFrmLogin.Caption         := Application.Title;
    lFrmLogin.Utilizator.Text := szPrevUserName;
    lFrmLogin.Parola.Text     := szPrevPassword;
    if lFrmLogin.Utilizator.Text <> '' then
      lFrmLogin.ActiveControl := lFrmLogin.Parola;
  end;

var
  lDataSet  : TDataSet;
begin
  Result    := -1;
  lFrmLogin := TLoginFrm.Create(nil);
  try
    InitForm;
    if bAskLoginInfo and (lFrmLogin.ShowModal <> mrOk) then Exit;    
    repeat
      lDataSet := DBNewQueryFmt('select * from utilizatori where nume like %s', [ValueToStr(lFrmLogin.Utilizator.Text)]);
      try
        lDataSet.Open;
        if not lDataSet.IsEmpty then begin
          if SameText(ValueSafeToStr(lDataSet['PAROLA']), MD5Print(lFrmLogin.Parola.Text)) then begin
            Result    := lDataSet['ID_UTILIZATORI'];
            NumeLogin := lFrmLogin.Utilizator.Text;
            SetLastUserEntry(NumeLogin);
            if lDataSet.FindField('NUMEINTREG') <> nil   then NumeLoginComplet := ValueSafeToStr(lDataSet['NUMEINTREG']);
            if lDataSet.FindField('NUME_COMPLET') <> nil then NumeLoginComplet := ValueSafeToStr(lDataSet['NUME_COMPLET']);
            if lDataSet.FindField('DREPTURI') <> nil     then IsAdmin := ValueSafeToInt(lDataSet['DREPTURI']) = 1 else IsAdmin := True;
            if lDataSet.FindField('ID_FUNCTIUNI') <> nil then begin
              IdFunctiune := ValueSafeToInt(lDataSet['ID_FUNCTIUNI']);
              IdDepartament := ValueSafeToInt(DBGetScallarFmt('select id_departamente from functiuni where id_functiuni=%s', [ValueToStr(IdFunctiune)]));
            end;
            GetHeaderSocietate;
          end
          else
            MessageDlg('Parola introdusa nu este corecta !', mtInformation, [mbOk], -1);
        end
        else
          MessageDlg('Utilizatorul ' + lFrmLogin.Utilizator.Text + ' nu exista !', mtInformation, [mbOk], -1);
      finally
        lDataSet.Free;
      end;
    until (Result <> -1) or (lFrmLogin.ShowModal = mrCancel);
  finally
    lFrmLogin.Free;
  end;
end;

procedure ConfigDB(ADataBase:TZConnection);
var
  DBList, ExplList, ConList: TStringList;
  lIniFile : TIniFile;
  lRegistry : TRegistry;

  function GetRegistryConnections(aRootKey : HKEY; const aSimbol : String = '[L]') : Integer;
  var
    I : Integer;
  begin
    lRegistry.RootKey := aRootKey;
    ConList.Clear;
    if lRegistry.OpenKey('Software\ATS\'+AppName+'Connects', False) then begin
      { Luam Lista de Conexiuni }
      lRegistry.GetValueNames(ConList);
      lRegistry.CloseKey;
      { Luam decat bazele Valide }
      for I := ConList.Count - 1 downto 0 do
        if not lRegistry.OpenKey('Software\ATS\'+AppName+'Servers\'+ConList[I], False) then
          ConList.Delete(I)
        else
          lRegistry.CloseKey;
      { Pentru intrarile valide in registri luam descrierea }
      lRegistry.OpenKey('Software\ATS\'+AppName+'Connects', False);
      for I := 0  to ConList.Count - 1 do begin
        DBList.Add(lRegistry.ReadString(ConList[I])+ aSimbol);
        ExplList.Add(ConList[I]);
      end;
      lRegistry.CloseKey;
    end;
    Result := ConList.Count;
  end;

  function GetIniConnections() : Integer;
  var
    lExePath  : string;
    I         : Integer;
  begin
    Result := 0;
    lExePath := ExtractFilePath(ParamStr(0));
    if FileExists(lExePath+'conexiuni.ini') then
      lIniFile := TIniFile.Create(lExePath+'conexiuni.ini')
    else
    if FileExists(lExePath+'connect.ini') then
      lIniFile := TIniFile.Create(lExePath+'connect.ini')
    else
    if FileExists('.\conexiuni.ini') then
      lIniFile := TIniFile.Create('.\conexiuni.ini')
    else
    if FileExists('.\connect.ini') then
      lIniFile := TIniFile.Create('.\connect.ini')
    else
      lIniFile := nil;
    if lIniFile <> nil then begin
      lIniFile.ReadSections(ConList);
      for I := 0  to ConList.Count - 1 do begin
        DBList.Add(lIniFile.ReadString(ConList[I],'DescriptionConnect', 'Neprecizat')+'[I]');
        ExplList.Add(ConList[I])
      end;
      Result := ConList.Count;
    end;
  end;

  procedure GetConnectSettings(const aIndex : Integer = 0);
  begin
    { Citim Setarile corespunzatoare }
    if (Pos('[I]', DBList[aIndex]) > 0)  then begin
      if (lIniFile <> nil) then begin
        szServerName  := lIniFile.ReadString(ExplList[aIndex],'Server','Neprecizat');
        szDBName      := lIniFile.ReadString(ExplList[aIndex],'Database','Neprecizat');
        WindowsNT     := lIniFile.ReadString(ExplList[aIndex],'WindowsNT','Neprecizat');
        szProtocol    := lIniFile.ReadString(ExplList[aIndex],'Protocol', '');
      end;
    end
    else begin
      if Pos('[L]', DBList[aIndex]) > 0 then
        lRegistry.RootKey := HKEY_LOCAL_MACHINE
      else
        lRegistry.RootKey := HKEY_CURRENT_USER;
      lRegistry.OpenKey('Software\ATS\'+AppName+'Servers\'+ExplList[aIndex], False);
      szServerName  := lRegistry.ReadString('Server');
      szDBName      := lRegistry.ReadString('DataBase');
      WindowsNT     := lRegistry.ReadString('WindowsNT');
      szProtocol    := lRegistry.ReadString('Protocol');
      lRegistry.CloseKey;
    end;
    DBSetConnectParams(ADataBase);
  end;

var
  I : Integer;

begin
  ADataBase.Connected:=False;
  DBList := TStringList.Create;
  ExplList := TStringList.Create;
  ConList := TStringList.Create;
  lRegistry := TRegistry.Create(KEY_READ);
    try
       DBList.Clear;
       ExplList.Clear;
       GetRegistryConnections(HKEY_LOCAL_MACHINE, '[L]');
       GetRegistryConnections(HKEY_CURRENT_USER, '[C]');
       GetIniConnections;

       if (DBList.Count=0) and bAskConnection then begin
         bIsCanceling := True;
         raise EContaHandledError.Create('Nu aveti definita nici o conexiune corecta !'#13#10'Contactati Administratorul de Sistem !');
       end else
       if (DBList.Count = 1) and bAskConnection then begin
         GetConnectSettings;
       end else
          if DBList.Count >0 then begin
             I := GetCurentDB(aDataBase,DBList);
             if I = -1 then begin
                bIsCanceling := True;
             end
             else begin
                { Citim Setarile corespunzatoare }
            GetConnectSettings(I);
             end;
          end;
    finally
       FreeAndNil(lIniFile);
       lRegistry.Free;
       DBList.Free;
       ExplList.Free;
       ConList.Free;
    end;
end;

function GetCurentDB(aData: TZConnection;AList: TStringList): Integer;
var aFrm: TForm;
    aCmb: TComboBox;
    SizeCmb: Integer;
begin
  Result := -1;
  aFrm := TForm.Create(Application);
  with aFrm do
    try
       BorderStyle := bsDialog;
       Position := poScreenCenter;
       Width := 200;
       Height := 100;
       Caption := 'Alegeti Baza De Date !';
       aCmb := TComboBox.Create(aFrm);
       aCmb.Parent := aFrm;
       aCmb.Left := 10;
       aCmb.Top :=10;
       aCmb.Width := 170;
       SizeCmb := aCmb.Width;
       with TBitBtn.Create(aFrm) do begin
         Parent := aFrm;
         Left := (200-Width) div 2;
         Top  := 40;
         Kind := bkOK;
       end;
       try
          aCmb.Items.AddStrings(AList);
          if aCmb.Items.Count>0 then
             aCmb.ItemIndex := 0
          else
             raise EContaHandledError.Create('Nu aveti nici o societate valida !');
          aCmb.Style := csDropDownList;
          SendMessage(aCmb.Handle, CB_SETDROPPEDWIDTH,SizeCmb,0);
          if ShowModal=1 {mrOk} then begin
             Result       := aCmb.ItemIndex;
             szServerName := aList[Integer(aCmb.Items.Objects[aCmb.ItemIndex])];
             SocName      := aCmb.Items[aCmb.ItemIndex];
          end
          else Result := -1;
          if not ExtendedConfiguration then aData.Connected := False;
       except
         on E:Exception do begin
           bIsCanceling := True;
           raise EContaHandledError.Create('Nu se poate conecta la serverul : '+szServerName+#13#10'Eroare : '+E.Message+#13#10#13#10'Contactati Administratorul Aplicatiei !');
         end;
       end;
    finally
       Free;
    end;
end;

function GetQryDescription(aQryName : string ) : string;
var
  lIndex : Integer;
begin
  lIndex := GlobalQryDescription.IndexOf(aQryName);
  if lIndex <> -1 then Result := GlobalQryDescription[lIndex]
                  else Result := 'Nomenclator ' + StringReplace(aQryName, 'qry', '', [rfReplaceAll, rfIgnoreCase]);
end;

procedure OpenDataModule(DataModul: TComponent; Config: Boolean=True);
var I : Integer;
    lDataSet: TDataSet;
    lErrMsg : String;
    lYesByDefault : Boolean;

    function GetDataBase: TZConnection;
    var Y: Integer;
      begin
        Result := nil;
        for Y := 0 to DataModul.ComponentCount-1 do
            if (DataModul.Components[Y] is TZConnection) then begin
              Result := TZConnection(DataModul.Components[Y]);
              Break;
            end;
      end;
begin
  lYesByDefault := False;
  CurrentZDataSet := GetDataBase;
  CurrentZDataSet.Connected := False;
  if not Assigned (CurrentZDataSet) then Exit;
  lDataSet := nil;
  try
    if Assigned(Logo) then begin
      Logo.Progress.MinValue := 0;
      Logo.Progress.Progress := 0;
      Logo.Progress.MaxValue := DataModul.ComponentCount-1;
    end;

    if Config and bAskConnection then
      ConfigDB(CurrentZDataSet)
    else begin
      DBSetConnectParams(CurrentZDataSet);
      bIsCanceling := False;
    end;
    Application.ProcessMessages;
    if not bIsCanceling then
      try
        CurrentZDataSet.Connected := True;
      except
        on E: Exception do begin
          bIsCanceling := True;
          raise EContaHandledError.Create('Eroare de conectare : '+E.Message);
        end;
      end;
    if bIsCanceling then begin
      Exit;
    end;
    IdUtilizator := Login;
    if IdUtilizator = -1 then begin
      bIsCanceling := True;
      CurrentZDataSet.Connected := False;
      raise EContaHandledError.Create('Procedura de autentificare a fost abandonata !');
    end;

    { Logam Operatia de autentificare }
    if not IsReadOnlyDb then
      with TZQuery.Create(nil) do
        try
          Connection := CurrentZDataSet;
          Sql.Add('INSERT INTO LOGARE_USERS (ID_UTILIZATORI, NUME_LOGIN, NUME_COMPLET, STATIE, VERSION_ID, SPID, LOGIN_TIME, HOSTNAME, PROGRAM_NAME, NT_DOMAIN, NT_USERNAME, NET_ADDRESS, IP_ADDRESS, IP_ADDRES)');
          Sql.Add('SELECT B.ID_UTILIZATORI, :LOGIN_NAME, :COMPLET_NAME, :STATIE, :VERSION_ID, SPID, LOGIN_TIME, HOSTNAME, PROGRAM_NAME, NT_DOMAIN, NT_USERNAME, NET_ADDRESS, :IP_ADDRESS, :IP_ADDRES');
          Sql.Add('FROM MASTER.DBO.SYSPROCESSES A, UTILIZATORI B WHERE SPID = @@SPID AND B.ID_UTILIZATORI = '+IntToStr(IdUtilizator));
          Params.ParamByName('LOGIN_NAME').Value   := NumeLogin;
          Params.ParamByName('COMPLET_NAME').Value := NumeLoginComplet;
          Params.ParamByName('STATIE').Value       := GetHostName;
          Params.ParamByName('VERSION_ID').Value   := ExeVersion;
          Params.ParamByName('IP_ADDRESS').Value   := IP_Address;
          Params.ParamByName('IP_ADDRES').Value    := IP_Addres;
          ExecSql;
          Sql.Clear;
          Sql.Add('SELECT SCOPE_IDENTITY() AS COL');
          Open;
          IdLogin := Fields[0].AsInteger;
        finally
          Free;
        end;
    for I := 0 to DataModul.ComponentCount-1 do begin
      if Assigned(Logo) then begin
        Logo.Progress.Progress := I;
        Logo.lbStareCurenta.Caption := GetQryDescription(DataModul.Components[I].Name);
      end;
      Application.ProcessMessages;
      if (DataModul.Components[I] is TDataSet)
         and (DataModul.Components[I].Tag=1) then begin
         lDataSet := TDataSet(DataModul.Components[I]);
         try
           lDataSet.Active := True;
         except
           on E: Exception do
             if not lYesByDefault then
               case MessageDlg('Nu se poate deschide dataset-ul :'+lDataSet.Name+#13#10+'Eroare : '+E.Message+#13#10'Doriti continuarea?', mtConfirmation, [mbYes, mbYesToAll, mbNo], 0)              of
                 mrNo :  raise;
                 mrYes :;
                 mrYesToAll : lYesByDefault := True;
               end;
         end;
      end;
      lDataSet := nil;
    end;
  except
    on E: Exception do begin
      bIsCanceling := True;
      lErrMsg := 'Eroare pornire aplicatie : '+E.Message;
      if Assigned(lDataSet) then lErrMsg := lErrMsg+#13#10'Obiectul : '+lDataSet.Name;
      raise EContaHandledError.Create(lErrMsg+#13#10'Aplicatia nu poate continua !');
    end;
  end;
end;

procedure WriteMem(Addr: pointer; const Data; Size: DWord);
var
  OldProtectionCode: DWord;
begin
  VirtualProtect(Addr, Size, PAGE_EXECUTE_READWRITE, @OldProtectionCode);
  Move(Data, Addr^, Size);
  VirtualProtect(Addr, Size, oldProtectionCode, @OldProtectionCode);
  FlushInstructionCache(GetCurrentProcess, Addr, Size);
end;


function DecryptValididationKey(const Passwd, KryptGuid : String) : String;
{$IFDEF CONTA_CRYPT}
var
  GUID : TGUID;
  Crypters : TCipher_Blowfish;
  {$ENDIF}
begin
  Result := '';
  if KryptGuid = '' then Exit;

  {$IFDEF CONTA_CRYPT}
  Crypters := TCipher_Blowfish.Create(Passwd, nil);
  Crypters.Mode := cmCTS;
  Result := Crypters.CodeString(KryptGuid, paDecode, fmtMIME64);
  Crypters.Free;
  try
    GUID := StringToGUID(Result);
  except
    Result := '';
  end;
  {$ENDIF}
end;

procedure GetHeaderSocietate;
var
  lDataSet      : TDataSet;
  lGraphicClass : TGraphicClass;
  lBlobStream   : TStream;
begin
  FreeAndNil(antetImagine);
  if DBTableExists('ANTET_DATE_SOCIETATE') then begin
    DBSetBlobSize(2147483647);
    try
      lDataSet := DBNewQuery('select top 1 * from ANTET_DATE_SOCIETATE');
      try
        lDataSet.Open;
        antetNumeSocietate    := ValueSafeToStr(lDataSet['NUME_SOCIETATE']);
        antetAdresaSocietate  := ValueSafeToStr(lDataSet['ADRESA_SOCIETATE']);
        antetCodFiscal        := ValueSafeToStr(lDataSet['COD_FISCAL']);
        antetTelefon          := ValueSafeToStr(lDataSet['TELEFON']);
        antetLocalitate       := ValueSafeToStr(lDataSet['LOCALITATE']);
        antetJudet            := ValueSafeToStr(lDataSet['JUDET']);
        antetFax              := ValueSafeToStr(lDataSet['FAX']);
        antetEmail            := ValueSafeToStr(lDataSet['EMAIL']);
        antetAdresaWeb        := ValueSafeToStr(lDataSet['ADRESA_WEB']);
        lGraphicClass         := FileFormatList.FindGraphicByName(ValueSafeToStr(lDataSet['IMAGINE_CLASS'], 'TBitmap'));
        if Assigned(lGraphicClass) then begin
          try
            antetImagine  := lGraphicClass.Create;
            lBlobStream   := lDataSet.CreateBlobStream(lDataSet.FieldByName('IMAGINE'), bmRead);
            try
              antetImagine.LoadFromStream(lBlobStream);
            finally
              lBlobStream.Free;
            end;
          except
            on E: Exception do begin
              antetImagine := nil;
            end;
          end;
        end;
      finally
        lDataSet.Free;
      end;
    finally
      DBSetBlobSize(2048);
    end;
  end;
end;

const
  LastUserValueName: String = 'LastUser';

function GetLastUserEntry : String;
var
  lReg : TRegistry;
begin
  lReg := TRegistry.Create(KEY_READ);
  try
    lReg.RootKey := HKEY_CURRENT_USER;
    if lReg.OpenKey('Software\ATS\'+AppName+'Settings', False) and lReg.ValueExists(LastUserValueName) then
      Result := lReg.ReadString(LastUserValueName)
    else
      Result := '';
  finally
    lReg.Free;
  end;
end;

procedure SetLastUserEntry(NumeLogin : String);
var
  lReg  : TRegistry;
begin
  lReg := TRegistry.Create;
  try
    lReg.RootKey := HKEY_CURRENT_USER;
    if NumeLogin = '' then begin
      if lReg.OpenKey('Software\ATS\'+AppName+'Settings', False) then
        lReg.DeleteValue(LastUserValueName);
    end
    else begin
      if lReg.OpenKey('Software\ATS\'+AppName+'Settings', True) then
        lReg.WriteString(LastUserValueName, NumeLogin);
    end;
  finally
    lReg.Free;
  end;
end;

procedure DBStartTransaction;
begin
  DBConnection.ExecuteDirect('BEGIN TRANSACTION');
end;
procedure DBCommit;
begin
  DBConnection.ExecuteDirect('COMMIT');
end;

procedure DBRollBack;
begin
  DBConnection.ExecuteDirect('IF @@TRANCOUNT > 0 ROLLBACK');
end;

function  DBInTransaction: Boolean;
begin
  Result := DBTranCount > 0;
end;

procedure DBSetConnectParams(AConnection: TZConnection);
var
  lAdoConnStr: String;
begin
  szSSPI := (AnsiCompareText(WindowsNT, 'true') = 0);
  if szProtocol = '' then
    szProtocol := szDefProtocol;


  AConnection.Connected := False;
  AConnection.Protocol := szProtocol;
  if szProtocol = 'ado' then begin
    if not szSSPI then
      lAdoConnStr := Format('Provider=%s;Persist Security Info=True;User ID=%s;Initial Catalog=%s;Data Source=%s;Application Name=%s;Workstation ID=%s;Password=%s;',
                          [
                            szProviderName,
                            szUserName,
                            szDBName,
                              szServerName,
                              'Contabiliate',
                              CommonDBVar.GetHostName,
                              szPassword
                              ])
    else
      lAdoConnStr := Format('Provider=%s;Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=%s;Data Source=%s;Application Name=%s;Workstation ID=%s',
                            [
                              szProviderName,
                              szDBName,
                              szServerName,
                              CommonDBVar.AppName,
                              CommonDBVar.GetHostName
                              ]);
    AConnection.Database := lAdoConnStr;
  end
  else begin
    if szSSPI then begin
      AConnection.Properties.Add('trusted=True');
    end
    else begin
      AConnection.User     := szUserName;
      AConnection.Password := szPassword;
    end;
    AConnection.Properties.Add('workstation=' + CommonDBVar.GetHostName);
    AConnection.Properties.Add('appname=' + CommonDBVar.AppName);

    if szWebLocation > '' then
      AConnection.Properties.Values['remoteAddr'] := szWebLocation;
    if szDBSRV > '' then
      AConnection.Properties.Values['dbSRV'] := szDBSRV;

    AConnection.HostName := szServerName;
    AConnection.Database := szDBName;
    AConnection.Catalog :=  szDBName;
  end;
end;

//------------------------------------------------------------------------------
procedure qryOpen(qa: array of TZQuery);
var i: Integer;
begin
  for i := Low(qa) to High(qa) do
    with qa[i] do
      if not Active then Open;
end;
//------------------------------------------------------------------------------
procedure qryClose(qa: array of TZQuery);
var i: Integer;
begin
  for i := Low(qa) to High(qa) do
    with qa[i] do
      if Active then Close;
end;
//------------------------------------------------------------------------------
procedure QryPost(qrs: array of TZQuery);
var i: Integer;
begin
  for i := Low(qrs) to High(qrs) do
    If qrs[i].State in [dsEdit, dsInsert] then begin
      qrs[i].Post;
      qrs[i].Connection.Commit;
    end;
end;
//------------------------------------------------------------------------------
procedure QryRefresh(qrs: array of TZQuery);
var i: integer;
begin
  for i := Low(qrs) to High(qrs) do
    with qrs[i] do
    begin
      If Active then Close;
      Open;
    end;
end;
//------------------------------------------------------------------------------
procedure QryDRefresh(qrs: array of TZQuery);
var i: Integer;
begin
  for i := Low(qrs) to High(qrs) do
    with qrs[i] do
    begin
      DisableControls;
      if Active then Close;
      Open;
      EnableControls;
  end;
end;
//------------------------------------------------------------------------------
procedure QryEdit(qrs: array of TZQuery);
var i: Integer;
begin
  for i := Low(qrs) to High(qrs) do
    if not (qrs[i].State in dsEditModes) then
        qrs[i].Edit;
end;

initialization
  GlobalQryDescription := TStringList.Create;
finalization
  if GlobalQryDescription<> nil then
    GlobalQryDescription.Free;
end.
