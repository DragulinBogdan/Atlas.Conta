unit PersistGridSettings;

interface

uses cxStorage, Windows, IniFiles, Classes, SysUtils, Dialogs;

type
  { TcxIniFileReader }

  TcxDBIniFileReader = class(TcxCustomReader)
  private
    FStorageVersion : Integer;
    FIniFile: TMemIniFile;
    FPathList: TStringList;
    FObjectNameList: TStringList;
    FClassNameList: TStringList;
    FIsEmpty : Boolean;
    function DecodeString(S: string): string;
    procedure CreateLists;
    procedure GetSectionDetail(const ASection: string; var APath, AObjectName, AClassName: string);
  protected
    procedure BeginRead; override;
    function CanRead: Boolean; override;
    procedure EndRead; override;
  public
    constructor Create(const AStorageName: string; AStorageStream: TStream); override;
    destructor Destroy; override;
    procedure ReadProperties(const AObjectName, AClassName: string; AProperties: TStrings); override;
    function ReadProperty(const AObjectName, AClassName, AName: string): Variant; override;
    procedure ReadChildren(const AObjectName, AClassName: string; AChildrenNames,
        AChildrenClassNames: TStrings); override;
  end;

  { TcxIniFileWriter }

  TcxDBIniFileWriter = class(TcxCustomWriter)
  private
    FIniFile: TMemIniFile;
    function EncodeString(const S: string): string;
  protected
    procedure BeginWrite; override;
    procedure EndWrite; override;
  public
    constructor Create(const AStorageName: string; AStream: TStream; AReCreate: Boolean = True); overload; override;
    destructor Destroy; override;
    procedure BeginWriteObject(const AObjectName, AClassName: string); override;
    procedure WriteProperty(const AObjectName, AClassName, AName: string; AValue: Variant); override;
  end;

  
procedure LoadSettingToStrings(AIdentifier : String; AStrings : TStrings);
procedure SaveSettingFromStrings(AIdentifier : string; AStrings : TStrings);

procedure LoadSettingToStream(AIdentifier : String; AStream : TStream);
procedure SaveSettingFromStream(AIdentifier : string; AStream : TStream);

implementation

uses Variants, ZDataset, CommonDBVar, ZAbstractRODataset, DB, ZeosDBUtile;

procedure LoadSettingToStrings(AIdentifier : String; AStrings : TStrings);
begin
  if not Assigned(AStrings) then Exit;
  AStrings.Clear;
  AStrings.Text := ValueSafeToStr(DBGetScallarFmt('exec spUtilizatoriGetSetare %d, %s', [iUserID, ValueToStr(AIdentifier)]));
end;

procedure LoadSettingToStream(AIdentifier : String; AStream : TStream);
var
  lStrList : TStringList;
begin
  if not Assigned(AStream) then Exit;
  lStrList := TStringList.Create;
  LoadSettingToStrings(AIdentifier, lStrList);
  AStream.Size := 0;
  lStrList.SaveToStream(AStream);
  lStrList.Free;
end;


procedure SaveSettingFromStrings(AIdentifier : string; AStrings : TStrings);
begin
  DBExecSqlFmt('exec [spUtilizatoriSetSetare] %d, %s, %s', [iUserID, ValueToStr(AIdentifier), ValueToStr(AStrings.Text)]);
end;


procedure SaveSettingFromStream(AIdentifier : string; AStream : TStream);
var
  lStrList : TStringList;
begin
  if not Assigned(AStream) then Exit;
  AStream.Position := 0;
  lStrList := TStringList.Create;
  lStrList.Clear;
  lStrList.LoadFromStream(AStream);
  SaveSettingFromStrings(AIdentifier, lStrList);
  lStrList.Free;
end;


function DateTimeOrStr(AValue: string): Variant;
var
  ADateTimeValue: TDateTime;
begin
  if TryStrToDateTime(AValue, ADateTimeValue) then
    Result := ADateTimeValue
  else
    Result := AValue;
end;

function IsStringValue(const AValue: string): Boolean;
begin
  Result := cxIsQuotedStr(AValue);
end;


{ TcxDBIniFileReader }

constructor TcxDBIniFileReader.Create(const AStorageName: string; AStorageStream: TStream);
var
  lStrList : TStringList;
begin
  inherited Create(AStorageName, AStorageStream);
  FIsEmpty := False;
  FIniFile := TMemIniFile.Create('');
  lStrList := TStringList.Create;
  lStrList.Clear;
  LoadSettingToStrings(AStorageName,lStrList);
     FIsEmpty := (Trim(lStrList.Text) = '');
     FIniFile.SetStrings(lStrList);
    lStrList.Free;
  end;

destructor TcxDBIniFileReader.Destroy;
begin
  FreeAndNil(FIniFile);
  FreeAndNil(FPathList);
  FreeAndNil(FObjectNameList);
  FreeAndNil(FClassNameList);

  inherited Destroy;
end;

procedure TcxDBIniFileReader.EndRead;
begin
  inherited;

end;

procedure TcxDBIniFileReader.ReadChildren(const AObjectName, AClassName: string;
  AChildrenNames, AChildrenClassNames: TStrings);
var
  I: Integer;
  AParentPath: string;
begin
  CreateLists;

  if AObjectName <> '' then
    AParentPath := UpperCase(AObjectName) + '/'
  else
    AParentPath := UpperCase(AObjectName);

  for I := 0 to FPathList.Count - 1 do
  begin
    if FPathList[I] = AParentPath then
    begin
      AChildrenNames.Add(FObjectNameList[I]);
      AChildrenClassNames.Add(FClassNameList[I]);
    end;
  end;
end;

procedure TcxDBIniFileReader.ReadProperties(const AObjectName, AClassName: string; AProperties: TStrings);
var
  ASectionName: string;
begin
  ASectionName := AObjectName + ': ' + AClassName;
  FIniFile.ReadSection(ASectionName, AProperties);
end;

function TcxDBIniFileReader.ReadProperty(const AObjectName, AClassName, AName: string): Variant;
var
  ASectionName: string;
  AValue: string;
  AIntegerValue: Integer;
  ARealValue: Double;
  ACode: Integer;
begin
  ASectionName := AObjectName + ': ' + AClassName;
  AValue := FIniFile.ReadString(ASectionName, AName, '');

  if IsStringValue(AValue) then
    Result := DecodeString(AValue)
  else
  begin
    Val(AValue, AIntegerValue, ACode);
    if ACode = 0 then
      Result := AIntegerValue
    else
    begin
      Val(AValue, ARealValue, ACode);
      if ACode = 0 then
        Result := ARealValue
      else
        Result := DateTimeOrStr(AValue);
    end;
  end;
end;

procedure TcxDBIniFileReader.BeginRead;
begin
  FStorageVersion := FIniFile.ReadInteger('Main', 'Version', -1);
end;

function TcxDBIniFileReader.CanRead: Boolean;
begin
  Result := not FIsEmpty;
end;

function TcxDBIniFileReader.DecodeString(S: string): string;

  function DecodeStringV2: string;
  var
    I: Integer;
  begin
    Result := '';
    I := 1;
    while I <= Length(S)  do
    begin
      if S[I] = '\' then
      begin
        Inc(I);
        if I <= Length(S) then
        begin
          if S[I] = 'n' then
            Result := Result + #13#10
          else if S[I] = 't' then
            Result := Result + #9
          else
            Result := Result + '\';
        end;
      end
      else
        Result := Result + S[I];
      Inc(I);
    end;
  end;

  function DecodeStringV0: string;
  begin
    Result := S;
    Result := StringReplace(Result, ' \n', #13#10, [rfReplaceAll, rfIgnoreCase]);
    Result := StringReplace(Result, '\\n', '\n', [rfReplaceAll, rfIgnoreCase]);
  end;

begin
  S := cxDequotedStr(S);
  case FStorageVersion of
    1, 2: Result := DecodeStringV2;
    0: Result := DecodeStringV0;
  else //-1
    Result := S;
  end;
end;

procedure TcxDBIniFileReader.CreateLists;
var
  ASectionList: TStringList;
  I: Integer;
  APath: string;
  AObjectName: string;
  AClassName: string;
begin
  if (FPathList = nil) or (FObjectNameList = nil) or (FClassNameList = nil) then
  begin
    FPathList := TStringList.Create;
    FObjectNameList := TStringList.Create;
    FClassNameList := TStringList.Create;
    ASectionList := TStringList.Create;
    try
      FIniFile.ReadSections(ASectionList);
      for I := 0 to ASectionList.Count - 1 do
      begin
        GetSectionDetail(ASectionList[I], APath, AObjectName, AClassName);
        FPathList.Add(UpperCase(APath));
        FObjectNameList.Add(AObjectName);
        FClassNameList.Add(AClassName);
      end;
    finally
      ASectionList.Free;
    end;
  end;
end;

procedure TcxDBIniFileReader.GetSectionDetail(const ASection: string; var APath, AObjectName, AClassName: string);
var
  I: Integer;
  AName: string;
begin
  AName := '';
  APath := '';
  AObjectName := '';
  AClassName := '';

  for I := 1 to Length(ASection) do
    if ASection[I] = '/' then
    begin
      APath := APath + AName + '/';
      AName := '';
    end
    else
      if ASection[I] = ':' then
      begin
        AObjectName := AName;
        AName := '';
      end
      else
        AName := AName + ASection[I];
  AClassName := Trim(AName);
end;

{ TcxDBIniFileWriter }

constructor TcxDBIniFileWriter.Create(const AStorageName: string; AStream: TStream; AReCreate: Boolean = True);
begin
  inherited Create(AStorageName, AStream, AReCreate);

  FIniFile := TMemIniFile.Create('');
  {$IFDEF DELPHI12}FIniFile.Encoding := TEncoding.UTF8;{$ENDIF}
  if FReCreate then
    FIniFile.Clear;
  FIniFile.CaseSensitive := False;
end;

destructor TcxDBIniFileWriter.Destroy;
begin
  FreeAndNil(FIniFile);
  inherited Destroy;
end;

procedure TcxDBIniFileWriter.BeginWriteObject(const AObjectName, AClassName: string);
begin
  FIniFile.WriteString(AObjectName + ': ' + AClassName, '', '');
end;

procedure TcxDBIniFileWriter.WriteProperty(const AObjectName, AClassName, AName: string;
  AValue: Variant);
var
  ASectionName: string;
begin
  ASectionName := AObjectName + ': ' + AClassName;
  case VarType(AValue) of
    varShortInt, varWord, varLongWord, varInt64,
    varSmallInt, varInteger, varByte:
      FIniFile.WriteInteger(ASectionName, AName, AValue);
    varSingle, varDouble, varCurrency:
      FIniFile.WriteFloat(ASectionName, AName, AValue);
  {$IFDEF DELPHI12}
    varUString,
  {$ENDIF}
    varString, varOleStr:
      FIniFile.WriteString(ASectionName, AName, EncodeString(AValue));
    varDate:
      FIniFile.WriteDateTime(ASectionName, AName, AValue);
  end;
end;

const
  cxIniFileStorageVersion = 2;

procedure TcxDBIniFileWriter.BeginWrite;
begin
  FIniFile.WriteInteger('Main', 'Version', cxIniFileStorageVersion);
end;

procedure TcxDBIniFileWriter.EndWrite;
var
  lStrList : TStringList;
begin
  lStrList := TStringList.Create;
  FIniFile.GetStrings(lStrList);
  SaveSettingFromStrings(FStorageName, lStrList);
    lStrList.Free;
end;

function TcxDBIniFileWriter.EncodeString(const S: string): string;
begin
  Result := StringReplace(S, '\', '\\', [rfReplaceAll]);
  Result := StringReplace(Result, #13#10, '\n', [rfReplaceAll]);
  Result := StringReplace(Result, #9, '\t', [rfReplaceAll]);
  Result := cxQuotedStr(Result);
end;


end.
