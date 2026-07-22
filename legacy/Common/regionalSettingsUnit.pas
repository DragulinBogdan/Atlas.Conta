unit regionalSettingsUnit;

interface

procedure InitRegionalSettings;

implementation

uses
  Windows,
  Forms,
  SysUtils,
  VarUtils,
  dxExEdtr,
  cxDateUtils,
  cxFormats;

const
  CResult: array [False..True] of HRESULT = (VAR_INVALIDARG, VAR_OK);

var
  ODBCSettings : TFormatSettings;

function BackupVarI4FromStr(const strIn: WideString; LCID: Integer; dwFlags: Longint;
  out lOut: Longint): HRESULT; stdcall;
begin
  if LCID <> VAR_LOCALE_USER_DEFAULT then
    Result := VAR_NOTIMPL
  else
    Result := CResult[TryStrToInt(strIn, lOut)];
end;

function BackupVarR4FromStr(const strIn: WideString; LCID: Integer; dwFlags: Longint;
  out fltOut: Single): HRESULT; stdcall;
begin
  if LCID <> VAR_LOCALE_USER_DEFAULT then
    Result := VAR_NOTIMPL
  else
    Result := CResult[TryStrToFloat(strIn, fltOut)];
end;

function BackupVarR8FromStr(const strIn: WideString; LCID: Integer; dwFlags: Longint;
  out dblOut: Double): HRESULT; stdcall;
begin
  if LCID <> VAR_LOCALE_USER_DEFAULT then
    Result := VAR_NOTIMPL
  else
    Result := CResult[TryStrToFloat(strIn, dblOut)];
end;

function BackupVarDateFromStr(const strIn: WideString; LCID: DWORD; dwFlags: Longint;
  out dateOut: TDateTime): HRESULT; stdcall;
begin
  { Prima data incercam sa vedem daca este in format odbc yyyy-mm-dd hh:nn:ss }
  if TryStrToDateTime(strIn, dateOut, ODBCSettings) then
    Result := VAR_OK
  else
  if LCID <> VAR_LOCALE_USER_DEFAULT then
    Result := VAR_NOTIMPL
  else
    Result := CResult[TryStrToDateTime(strIn, dateOut)];
end;

function BackupVarCyFromStr(const strIn: WideString; LCID: DWORD; dwFlags: Longint;
  out cyOut: Currency): HRESULT; stdcall;
begin
  if LCID <> VAR_LOCALE_USER_DEFAULT then
    Result := VAR_NOTIMPL
  else
    Result := CResult[TryStrToCurr(strIn, cyOut)];
end;

function BackupVarBoolFromStr(const strIn: WideString; LCID: Integer; dwFlags: Longint;
  out boolOut: WordBool): HRESULT; stdcall;
var
  LBoolean: Boolean;
begin
  if LCID <> VAR_LOCALE_USER_DEFAULT then
    Result := VAR_NOTIMPL
  else
  begin
    Result := CResult[TryStrToBool(strIn, LBoolean)];
    boolOut := LBoolean;
  end;
end;


function BackupVarBStrFromCy(cyIn: Currency; LCID: Integer; dwFlags: Longint;
  out bstrOut: WideString): HRESULT; stdcall;
begin
  if LCID <> VAR_LOCALE_USER_DEFAULT then
    Result := VAR_NOTIMPL
  else
  begin
    bstrOut := CurrToStr(cyIn);
    Result := VAR_OK;
  end;
end;

function BackupVarBStrFromDate(dateIn: TDateTime; LCID: Integer; dwFlags: Longint;
  out bstrOut: WideString): HRESULT; stdcall;
begin
  if LCID <> VAR_LOCALE_USER_DEFAULT then
    Result := VAR_NOTIMPL
  else
  begin
    bstrOut := DateTimeToStr(dateIn);
    Result := VAR_OK;
  end;
end;

function BackupVarBStrFromBool(boolIn: WordBool; LCID: Integer; dwFlags: Longint;
  out bstrOut: WideString): HRESULT; stdcall;
begin
  if LCID <> VAR_LOCALE_USER_DEFAULT then
    Result := VAR_NOTIMPL
  else
  begin
    bstrOut := BoolToStr(boolIn);
    Result := VAR_OK;
  end;
end;

procedure InitRegionalSettings;
begin

  VarI4FromStr := BackupVarI4FromStr;
  VarR4FromStr := BackupVarR4FromStr;
  VarR8FromStr := BackupVarR8FromStr;
  VarDateFromStr := BackupVarDateFromStr;
  VarCyFromStr := BackupVarCyFromStr;
  VarBoolFromStr := BackupVarBoolFromStr;

  VarBstrFromCy := BackupVarBstrFromCy;
  VarBstrFromDate := BackupVarBstrFromDate;
  VarBstrFromBool := BackupVarBstrFromBool;

  FormatSettings.CurrencyFormat   := 3;
  FormatSettings.CurrencyDecimals := 2;
  FormatSettings.NegCurrFormat    := 8;
  FormatSettings.CurrencyString	  := '';
  
  FormatSettings.ShortDateFormat := 'dd/MM/yyyy';
  FormatSettings.DateSeparator   := '.';
  FormatSettings.ThousandSeparator  := '.';
  FormatSettings.DecimalSeparator   := ',';

  ODBCSettings.ShortDateFormat := 'yyyy/MM/dd';
  ODBCSettings.DateSeparator   := '-';
  ODBCSettings.ShortTimeFormat := 'hh:nn:ss';
  ODBCSettings.TimeSeparator   := ':';

  Application.UpdateFormatSettings := False;
  FormatSettings.LongMonthNames[1] := 'Ianuarie';
  FormatSettings.LongMonthNames[2] := 'Februarie';
  FormatSettings.LongMonthNames[3] := 'Martie';
  FormatSettings.LongMonthNames[4] := 'Aprilie';
  FormatSettings.LongMonthNames[5] := 'Mai';
  FormatSettings.LongMonthNames[6] := 'Iunie';
  FormatSettings.LongMonthNames[7] := 'Iulie';
  FormatSettings.LongMonthNames[8] := 'August';
  FormatSettings.LongMonthNames[9] := 'Septembrie';
  FormatSettings.LongMonthNames[10] := 'Octombrie';
  FormatSettings.LongMonthNames[11] := 'Noiembrie';
  FormatSettings.LongMonthNames[12] := 'Decembrie';

  FormatSettings.ShortMonthNames[1] := 'Ian';
  FormatSettings.ShortMonthNames[2] := 'Feb';
  FormatSettings.ShortMonthNames[3] := 'Mar';
  FormatSettings.ShortMonthNames[4] := 'Apr';
  FormatSettings.ShortMonthNames[5] := 'Mai';
  FormatSettings.ShortMonthNames[6] := 'Iun';
  FormatSettings.ShortMonthNames[7] := 'Iul';
  FormatSettings.ShortMonthNames[8] := 'Aug';
  FormatSettings.ShortMonthNames[9] := 'Sep';
  FormatSettings.ShortMonthNames[10] := 'Oct';
  FormatSettings.ShortMonthNames[11] := 'Noi';
  FormatSettings.ShortMonthNames[12] := 'Dec';

  FormatSettings.LongDayNames[1] := 'Duminica';
  FormatSettings.LongDayNames[2] := 'Luni';
  FormatSettings.LongDayNames[3] := 'Marti';
  FormatSettings.LongDayNames[4] := 'Miercuri';
  FormatSettings.LongDayNames[5] := 'Joi';
  FormatSettings.LongDayNames[6] := 'Vineri';
  FormatSettings.LongDayNames[7] := 'Sambata';

  FormatSettings.ShortDayNames[1] := 'Dum';
  FormatSettings.ShortDayNames[2] := 'Lun';
  FormatSettings.ShortDayNames[3] := 'Mar';
  FormatSettings.ShortDayNames[4] := 'Mie';
  FormatSettings.ShortDayNames[5] := 'Joi';
  FormatSettings.ShortDayNames[6] := 'Vin';
  FormatSettings.ShortDayNames[7] := 'Sam';

  cxFormatController.StartOfWeek := dMonday;
  cxUseSingleCharWeekNames  := False;
  UseDelphiDateTimeFormats  := True;
  cxFormatController.UseDelphiDateTimeFormats := True;

end;

end.
