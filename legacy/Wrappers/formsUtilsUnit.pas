unit formsUtilsUnit;

interface

uses
  Classes, Forms, tabsUtilsUnit, System.Generics.Collections;

type
  TFormUtils = class
  private
    class var
      FFormList : TList<TCustomForm>;
    class var
      FTabs     : TChromeProvider;
  protected
    class function VRecToVariant(const AVar: TVarRec): Variant;
    class function ConstToVariant(const AValues: array of Const): Variant;
  public
    class constructor Create;
    class destructor Destroy;
  public
    class function FindForm<T: TCustomForm>(): T;
    class function SetFormParams(AForm: TCustomForm; const ACaption, APropName: String; const APropValue: Variant): TCustomForm;

    class function ShowForm(AForm: TCustomForm; const ACaption: String=''): TCustomForm; overload;
    class function ShowForm(AForm: TCustomForm; const ACaption: String; const APropName: String; const APropValue: array of Const): TCustomForm; overload;
    class function ShowForm(AForm: TCustomForm; const ACaption: String; const APropName: String; APropValue: Variant): TCustomForm; overload;

    class function NewForm<T: TCustomForm>(const ACaption: String = ''; CreateNew: Boolean = False): T; overload;
    class function NewForm<T: TCustomForm>(const ACaption: String; CreateNew: Boolean; const APropName: STring; const APropValue: array of Const): T; overload;
    class function NewForm<T: TCustomForm>(const ACaption: String; CreateNew: Boolean; const APropName: String; const APropValue: Variant ): T; overload;
  public
    class property Tabs: TChromeProvider read FTabs;
  end;

implementation

uses
  Variants,
  TypInfo,
  SysUtils;

class function TFormUtils.VRecToVariant(const AVar: TVarRec): Variant;
begin
  case AVar.VType of
    vtInteger:
      Result := AVar.VInteger;
    vtBoolean:
      Result := AVar.VBoolean;
    vtChar:
      Result := AVar.VChar;
    vtExtended:
      Result := AVar.VExtended^;
    vtString:
      Result := AVar.VString^;
    vtPointer:
      Result := NativeInt(AVar.VPointer);
    vtPChar:
      Result := StrPas(AVar.VPChar);
    vtObject:
      Result := NativeInt(AVar.VObject);
    vtClass:
      Result := NativeInt(AVar.VClass);
    vtWideChar:
      Result := AVar.VWideChar;
    vtPWideChar:
      Result := StrPas(AVar.VPWideChar);
    vtAnsiString:
      Result := StrPas(PAnsiChar(AVar.VAnsiString));
    vtCurrency:
      Result := AVar.VCurrency^;
    vtVariant:
      Result := AVar.VVariant^;
    vtInterface:
      Result := NativeInt(AVar.VInterface);
    vtWideString:
      Result := StrPas(PWideChar(AVar.VWideString));
    vtInt64:
      Result := AVar.VInt64^;
    vtUnicodeString:
      Result := StrPas(PWideChar(AVar.VUnicodeString));
    else
      raise Exception.CreateFmt('Tip de data necunoscut : %d', [AVar.VType]);
  end;
end;

{ TFormUtils }

class function TFormUtils.NewForm<T>(const ACaption: String; CreateNew: Boolean): T;
begin
  Result := NewForm<T>(ACaption, CreateNew, '', []);
end;

class function TFormUtils.ConstToVariant(
  const AValues: array of Const): Variant;
var
  lValue: Variant;
  I     : Integer;
begin
  if Length(AValues) > 0 then begin
    Result := VarArrayCreate([0, High(AValues)-1], varVariant);
    for I := Low(AValues) to High(AValues) do
      lValue[I] := VRecToVariant(AValues[I]);
  end
  else
    Result := Null;
end;

class constructor TFormUtils.Create;
begin
  FFormList := TList<TCustomForm>.Create;
end;

class destructor TFormUtils.Destroy;
begin
  FFormList.Free;
end;

class function TFormUtils.FindForm<T>: T;
var
  I: Integer;
begin
  Result := nil;
  for I := 0 to FFormList.Count-1 do
    if FFormList[I].InheritsFrom(T) then begin
      Result := FFormList[I];
      Break;
    end;
end;

class function TFormUtils.NewForm<T>(const ACaption: String; CreateNew: Boolean;
  const APropName: STring; const APropValue: array of Const): T;
begin
  Result := NewForm<T>(ACaption, CreateNew, APropName, ConstToVariant(APropValue));
end;

class function TFormUtils.SetFormParams(AForm: TCustomForm; const ACaption,
  APropName: String; const APropValue: Variant): TCustomForm;
var
  I: Integer;
  lPropNames: TStringList;
begin
  Result := AForm;
  if ACaption > '' then Result.Caption := ACaption;
  if Length(APropName) > 0 then begin
    if pos(';', APropName) > 0 then begin
      lPropNames := TStringList.Create;
      try
        lPropNames.CommaText := APropName;
        for I := 0 to lPropNames.Count-1 do
          SetPropValue(Result, lPropNames[I], APropValue[I]);
      finally
        lPropNames.Free;
      end;
    end
    else
      SetPropValue(Result, APropName, APropValue);
  end;
end;

class function TFormUtils.ShowForm(AForm: TCustomForm; const ACaption: String): TCustomForm;
begin
  Result := ShowForm(AForm, ACaption, '', Null);
end;

class function TFormUtils.ShowForm(AForm: TCustomForm; const ACaption, APropName: String;
  const APropValue: array of Const): TCustomForm;
begin
  Result := ShowForm(AForm, ACaption, APropName, ConstToVariant(APropValue));
end;

class function TFormUtils.ShowForm(AForm: TCustomForm; const ACaption, APropName: String;
  APropValue: Variant): TCustomForm;
begin
  Result := AForm;
  FTabs.SelectTabFromForm( SetFormParams(Result, ACaption, APropName, APropValue) );
end;

class function TFormUtils.NewForm<T>(const ACaption: String; CreateNew: Boolean; const APropName: String;
  const APropValue: Variant): T;
var
  I: Integer;
  lPropNames: TStringList;
  A: TVarRec;
  lPropInfo: PPropInfo;

begin
  if not CreateNew then begin
    lPropInfo := GetPropInfo(T, 'IsMultiInstance', [tkMethod]);
    CreateNew := Assigned(lPropInfo) and Boolean(GetOrdProp(T, lPropInfo));
  end;
  if CreateNew then Result := FindForm<T>()
  else Result := nil;
  if not Assigned(Result) then begin
    Result := T(TCustomForm(T).Create(Application));
    FFormList.Add(Result);
  end;
  Result := ShowForm(Result, ACaption, APropName, APropValue);
end;

end.
