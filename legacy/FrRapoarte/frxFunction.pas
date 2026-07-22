{******************************************}
{                                          }
{             FastReport v4.0              }
{           Add Function Library           }
{                                          }
{         Copyright (c) 1998-2009          }
{           by Fast Reports Inc.           }
{                                          }
{         Copyright (c) 2001-2009          }
{           by Stalker SoftWare            }
{                                          }
{******************************************}

unit frxFunction;

interface

{$I frx.inc}

uses
  Classes, SysUtils, fs_iinterpreter
{$IFDEF Delphi6}
 ,Variants, Math
{$ENDIF};

type
  TfrxAddFunctionLibrary = class(TComponent)
  private
    FFormatDate :String;
    procedure SetFormatDate(const Value: String);
  published
    constructor Create(AOwner: TComponent); override;
    property FormatDate: String read FFormatDate write SetFormatDate;
  end;

 procedure Register;

implementation

uses
  frxFuncStr, frxFuncNum, frxFuncDate, frxFuncSQL, frxrcAddFunction;

type
  TAddFunctionLibrary = class(TfsRTTIModule)
  private
    FCatDate  :String;
    FCatOther :String;
    FCatStr   :String;
    FCatSQL   :String;
    FCatConv  :String;
    FCatVar   :String;
    FCatMath  :String;
    function CallMethodStr(Instance: TObject; ClassType: TClass; const MethodName: String; Caller: TfsMethodHelper): Variant;
    function CallMethodNum(Instance: TObject; ClassType: TClass; const MethodName: String; Caller: TfsMethodHelper): Variant;
    function CallMethodDate(Instance: TObject; ClassType: TClass; const MethodName: String; Caller: TfsMethodHelper): Variant;
    function CallMethodSQL(Instance: TObject; ClassType: TClass; const MethodName: String; Caller: TfsMethodHelper): Variant;
    function CallMethodOther(Instance: TObject; ClassType: TClass; const MethodName: String; Caller: TfsMethodHelper): Variant;
    function CallMethodVariant(Instance: TObject; ClassType: TClass; const MethodName: String; Caller: TfsMethodHelper): Variant;
    function CallMethodMath(Instance: TObject; ClassType: TClass; const MethodName: String; Caller: TfsMethodHelper): Variant;
    function ConvCS(cStr :String) :TfrCharSet;

  public
    constructor Create(AScript: TfsScript); override;
  end;

var
  cFFormatDate :String;

{ TfrxAddFunctionLibrary }

{--------------------------------------------------------------------}
{ Создание TfrxAddFunctionLibrary                                    }
{--------------------------------------------------------------------}
constructor TfrxAddFunctionLibrary.Create(AOwner: TComponent);
begin
 inherited Create(AOwner);

 FFormatDate := '';
 cFFormatDate := '';
end; { Create }

{--------------------------------------------------------------------}
{ Запись свойства FormatDate                                         }
{--------------------------------------------------------------------}
procedure TfrxAddFunctionLibrary.SetFormatDate(const Value: String);
begin
 FFormatDate := Value;
 cFFormatDate := Value;
end; { SetFormatDate }

{ TAddFunctionLibrary }

{--------------------------------------------------------------------}
{ Создание библиотеки и Обработка вызова функций                     }
{--------------------------------------------------------------------}
constructor TAddFunctionLibrary.Create(AScript: TfsScript);
begin

 inherited Create(AScript);

 FCatStr   := 'ctString';
 FCatDate  := 'ctDate';
 FCatOther := 'ctOther';
 FCatSQL   := 'SQL';
 FCatConv  := 'ctConv';
 FCatVar   := 'Variant';
 FCatMath  := 'ctMath';

 with AScript do begin

   // SQL
   AddMethod('function CreateStr(cStr :String) :String', CallMethodSQL, FCatSQL);
   AddMethod('function CreateNum(cNum :String) :String', CallMethodSQL, FCatSQL);
   AddMethod('function CreateDate(cDate :String) :String', CallMethodSQL, FCatSQL);

   // String
   AddMethod('function WordPosition(const nNum: Integer; const cStr: String; const cWordDelims: String): Integer', CallMethodStr, FCatStr);
   AddMethod('function ExtractWord(nNum: Integer; const cStr: String; const cWordDelims: String): String', CallMethodStr, FCatStr);
   AddMethod('function WordCount(const cStr: String; const cWordDelims: String): Integer', CallMethodStr, FCatStr);
   AddMethod('function IsWordPresent(const cWord,  cStr: String; const cWordDelims: String): Boolean', CallMethodStr, FCatStr);
   AddMethod('function NPos(const cSubStr: String; cStr: String; nNum: Integer): Integer', CallMethodStr, FCatStr);
   AddMethod('function ReplaceStr(const cStr1, cSrch, cReplace: String): String', CallMethodStr, FCatStr);
   AddMethod('function Replicate(cStr: String; nLen :Integer) :String', CallMethodStr, FCatStr);
   AddMethod('function PadRight(cStr: String; nLen: Integer; cChar :String) :String', CallMethodStr, FCatStr);
   AddMethod('function PadLeft(cStr: String; nLen: Integer; cChar :String) :String', CallMethodStr, FCatStr);
   AddMethod('function PadCenter(cStr: String; nWidth: Integer; cChar: String): String', CallMethodStr, FCatStr);
   AddMethod('function EndPos(cStr, cSubStr: String) :Integer', CallMethodStr, FCatStr);
   AddMethod('function CompareStr(cStr1, cStr2: String) :Integer', CallMethodStr, FCatStr);
   AddMethod('function LeftCopy(cStr: String; nCount: Integer): String', CallMethodStr, FCatStr);
   AddMethod('function RightCopy(cStr: String; nCount: Integer): String', CallMethodStr, FCatStr);
   AddMethod('function TrimLeft(const cStr: String): String', CallMethodStr, FCatStr);
   AddMethod('function TrimRight(const cStr: String): String', CallMethodStr, FCatStr);

   // Convert
   AddMethod('function StrToFloatDef(cFlt:String; nFltDef :Extended) :Extended', CallMethodNum, FCatConv);
   AddMethod('function StrToIntDef(const cStr: String; const nDefault: Integer): Integer', CallMethodNum, FCatConv);
   AddMethod('function StrToDateDef(cDate: String; dDefault: TDateTime): TDateTime', CallMethodDate, FCatConv);

   // Date
   AddMethod('function DaysPerMonth(nYear, nMonth: Integer): Integer', CallMethodDate, FCatDate);
   AddMethod('function FirstDayOfNextMonth(dDate:TDateTime): TDateTime', CallMethodDate, FCatDate);
   AddMethod('function FirstDayOfPrevMonth(dDate:TDateTime): TDateTime', CallMethodDate, FCatDate);
   AddMethod('function LastDayOfPrevMonth(dDate:TDateTime): TDateTime', CallMethodDate, FCatDate);
   AddMethod('function IncYear(dDate: TDateTime; nDelta: Integer): TDateTime', CallMethodDate, FCatDate);
   AddMethod('function IncDay(dDate: TDateTime; nDelta: Integer): TDateTime', CallMethodDate, FCatDate);
   AddMethod('function IncDate(dDate: TDateTime; nDays, nMonths, nYears: Integer): TDateTime', CallMethodDate, FCatDate);
   AddMethod('function IncTime(dTime: TDateTime; nHours, nMinutes, nSeconds, nMSecs :Integer): TDateTime', CallMethodDate, FCatDate);
   AddMethod('procedure DateDiff(dDate1, dDate2: TDateTime; var nDays, nMonths, nYears: Word)', CallMethodDate, FCatDate);
   AddMethod('function IncMonth(dDate: TDateTime; nDelta: Integer): TDateTime', CallMethodDate, FCatDate);
   AddMethod('function QuarterOf(dDate :TDateTime) :Integer', CallMethodDate, FCatDate);
   AddMethod('function GetDay(dDate :TDateTime) :Word', CallMethodDate, FCatDate);
   AddMethod('function GetMonth(dDate :TDateTime) :Word', CallMethodDate, FCatDate);
   AddMethod('function GetYear(dDate :TDateTime) :Word', CallMethodDate, FCatDate);
   AddMethod('function GetWeek(dDate :TDateTime) :Word', CallMethodDate, FCatDate);

   // Other
   AddMethod('procedure Swap(var vVar1, vVar2 :Variant)', CallMethodOther, FCatOther);
   AddMethod('function IsRangeNum(nBeg, nEnd, nValue: Extended) :Boolean', CallMethodNum, FCatOther);
   AddMethod('function IsRangeDate(dBegDate, dEndDate, dDate: TDateTime) :Boolean', CallMethodDate, FCatOther);
   AddMethod('function TStringsToString(oStrings: TStrings): String', CallMethodOther, FCatOther);
   AddMethod('function StringToTStrings(cStrings: String): String', CallMethodOther, FCatOther);

   // Variant
   AddMethod('function VarArrayOf(const Values: array of Variant): Variant', CallMethodVariant, FCatVar);
   AddMethod('procedure VarArrayRedim(var A: Variant; HighBound: Integer)', CallMethodVariant, FCatVar);
   AddMethod('procedure VarClear(var V: Variant)', CallMethodVariant, FCatVar);
   AddMethod('function VarFromDateTime(dDateTime: TDateTime): Variant', CallMethodVariant, FCatVar);
   AddMethod('function VarToDateTime(const V: Variant): TDateTime)', CallMethodVariant, FCatVar);
   AddMethod('function VarInRange(const AValue, AMin, AMax: Variant): Boolean', CallMethodVariant, FCatVar);
   AddMethod('function VarIsClear(const V: Variant): Boolean', CallMethodVariant, FCatVar);
   AddMethod('function VarIsArray(const A: Variant): Boolean', CallMethodVariant, FCatVar);

   // Math
   AddMethod('function RoundTo(const AValue: Double; const ADigit: Integer): Double', CallMethodMath, FCatMath);
   AddMethod('function SimpleRoundTo(const AValue: Double; const ADigit: Integer): Double', CallMethodMath, FCatMath);

 end; { with }

end; { Create }

{--------------------------------------------------------------------}
{ Инициализация библиотеки для поддержки динамических SQL запросов   }
{--------------------------------------------------------------------}
function TAddFunctionLibrary.CallMethodSQL(Instance: TObject;
  ClassType: TClass; const MethodName: String; Caller: TfsMethodHelper): Variant;
begin

 if MethodName = 'CREATESTR' then
   Result := frCreateStr(Caller.Params[0])
 else
 if MethodName = 'CREATENUM' then
   Result := frCreateNum(Caller.Params[0])
 else
 if MethodName = 'CREATEDATE' then
   Result := frCreateDate(Caller.Params[0], cFFormatDate);

end; { CallMethodSQL }

{--------------------------------------------------------------------}
{ Инициализация библиотеки для дат                                   }
{--------------------------------------------------------------------}
function TAddFunctionLibrary.CallMethodDate(Instance: TObject;
  ClassType: TClass; const MethodName: String; Caller: TfsMethodHelper): Variant;

var
  p1, p2, p3 :Word;

begin

 if MethodName = 'DAYSPERMONTH' then
   Result := frDaysPerMonth(Caller.Params[0], Caller.Params[1])
 else
 if MethodName = 'FIRSTDAYOFNEXTMONTH' then
   Result := frFirstDayOfNextMonth(Caller.Params[0])
 else
 if MethodName = 'FIRSTDAYOFPREVMONTH' then
   Result := frFirstDayOfPrevMonth(Caller.Params[0])
 else
 if MethodName = 'LASTDAYOFPREVMONTH' then
   Result := frLastDayOfPrevMonth(Caller.Params[0])
 else
 if MethodName = 'INCYEAR' then
   Result := frIncYear(Caller.Params[0], Caller.Params[1])
 else
 if MethodName = 'INCDAY' then
   Result := frIncDay(Caller.Params[0], Caller.Params[1])
 else
 if MethodName = 'INCDATE' then
   Result := frIncDate(Caller.Params[0], Caller.Params[1], Caller.Params[2], Caller.Params[3])
 else
 if MethodName = 'INCTIME' then
   Result := frIncTime(Caller.Params[0], Caller.Params[1], Caller.Params[2], Caller.Params[3], Caller.Params[4])
 else
 if MethodName = 'DATEDIFF' then begin
   frDateDiff(Caller.Params[0], Caller.Params[1], p1, p2, p3);
   Caller.Params[2] := p1;
   Caller.Params[3] := p2;
   Caller.Params[4] := p3;
 end else
 if MethodName = 'ISRANGEDATE' then
   Result := frIsRangeDate(Caller.Params[0], Caller.Params[1], Caller.Params[2])
 else
 if MethodName = 'STRTODATEDEF' then
   Result := frStrToDateDef(Caller.Params[0], Caller.Params[1])
 else
 if MethodName = 'INCMONTH' then
   Result := frIncMonth(Caller.Params[0], Caller.Params[1])
 else
 if MethodName = 'QUARTEROF' then
   Result := frQuarterOf(Caller.Params[0])
 else
 if MethodName = 'GETDAY' then
   Result := frDay(Caller.Params[0])
 else
 if MethodName = 'GETMONTH' then
   Result := frMonth(Caller.Params[0])
 else
 if MethodName = 'GETYEAR' then
   Result := frYear(Caller.Params[0])
 else
 if MethodName = 'GETWEEK' then
   Result := frWeek(Caller.Params[0])

end; { CallMethodDate }

{--------------------------------------------------------------------}
{ Инициализация библиотеки для чисел                                 }
{--------------------------------------------------------------------}
function TAddFunctionLibrary.CallMethodNum(Instance: TObject;
  ClassType: TClass; const MethodName: String; Caller: TfsMethodHelper): Variant;
begin

 if MethodName = 'ISRANGENUM' then
   Result := frIsRangeNum(Caller.Params[0], Caller.Params[1], Caller.Params[2])
 else
 if MethodName = 'STRTOFLOATDEF' then
   Result := frStrToFloatDef(Caller.Params[0], Caller.Params[1])
 else
 if MethodName = 'STRTOINTDEF' then
   Result := StrToIntDef(Caller.Params[0], Caller.Params[1])

end; { CallMethodNum }

{--------------------------------------------------------------------}
{ Инициализация библиотеки для строк                                 }
{--------------------------------------------------------------------}
function TAddFunctionLibrary.CallMethodStr(Instance: TObject;
  ClassType: TClass; const MethodName: String; Caller: TfsMethodHelper): Variant;
begin

 if MethodName = 'WORDPOSITION' then
   Result := frWordPosition(Caller.Params[0], Caller.Params[1], ConvCS(Caller.Params[2]))
 else
 if MethodName = 'EXTRACTWORD' then
   Result := frExtractWord(Caller.Params[0], Caller.Params[1], ConvCS(Caller.Params[2]))
 else
 if MethodName = 'WORDCOUNT' then
   Result := frWordCount(Caller.Params[0], ConvCS(Caller.Params[1]))
 else
 if MethodName = 'ISWORDPRESENT' then
   Result := frIsWordPresent(Caller.Params[0], Caller.Params[1], ConvCS(Caller.Params[2]))
 else
 if MethodName = 'NPOS' then
   Result := frNPos(Caller.Params[0], Caller.Params[1], Caller.Params[2])
 else
 if MethodName = 'REPLACESTR' then
   Result := frReplaceStr(Caller.Params[0], Caller.Params[1], Caller.Params[2])
 else
 if MethodName = 'REPLICATE' then
   Result := frReplicate(Caller.Params[0], Caller.Params[1])
 else
 if MethodName = 'PADRIGHT' then
   Result := frPadRight(Caller.Params[0], Caller.Params[1], Caller.Params[2])
 else
 if MethodName = 'PADLEFT' then
   Result := frPadLeft(Caller.Params[0], Caller.Params[1], Caller.Params[2])
 else
 if MethodName = 'PADCENTER' then
   Result := frPadCenter(Caller.Params[0], Caller.Params[1], Caller.Params[2])
 else
 if MethodName = 'ENDPOS' then
   Result := frEndPos(Caller.Params[0], Caller.Params[1])
 else
 if MethodName = 'COMPARESTR' then
   Result := frCompareStr(Caller.Params[0], Caller.Params[1])
 else
 if MethodName = 'LEFTCOPY' then
   Result := frLeftCopy(Caller.Params[0], Caller.Params[1])
 else
 if MethodName = 'RIGHTCOPY' then
   Result := frRightCopy(Caller.Params[0], Caller.Params[1])
 else
 if MethodName = 'TRIMLEFT' then
   Result := TrimLeft(Caller.Params[0])
 else
 if MethodName = 'TRIMRIGHT' then
   Result := TrimRight(Caller.Params[0])

end; { CallMethodStr }

{--------------------------------------------------------------------}
{ Инициализация библиотеки для Variant                               }
{--------------------------------------------------------------------}
function TAddFunctionLibrary.CallMethodVariant(Instance: TObject;
  ClassType: TClass; const MethodName: String; Caller: TfsMethodHelper): Variant;

var
  aVar :Array of Variant;
  vVar :Variant;

begin

 if MethodName = 'VARARRAYOF' then begin
   aVar := Caller.Params[0];
   Result := VarArrayOf(aVar);
   SetLength(aVar, 0);
 end else
 if MethodName = 'VARARRAYREDIM' then begin
   vVar := Caller.Params[0];
   VarArrayRedim(vVar, Integer(Caller.Params[1]));
   Caller.Params[0] := vVar;
 end else
 if MethodName = 'VARCLEAR' then begin
   vVar := Caller.Params[0];
   VarClear(vVar);
   Caller.Params[0] := vVar;
 end else
 if MethodName = 'VARFROMDATETIME' then
   Result := VarFromDateTime(Caller.Params[0])
 else
 if MethodName = 'VARISCLEAR' then
   Result := VarIsClear(Caller.Params[0])
 else
 if MethodName = 'VARISARRAY' then
   Result := VarIsArray(Caller.Params[0])
 else
 if MethodName = 'VARTODATETIME' then
   Result := VarToDateTime(Caller.Params[0])
 else
 if MethodName = 'VARINRANGE' then
   Result := VarInRange(Caller.Params[0], Caller.Params[1], Caller.Params[2]);

end; { CallMethodVariant }

{--------------------------------------------------------------------}
{ Инициализация остальных ф-ий библиотеки                            }
{--------------------------------------------------------------------}
function TAddFunctionLibrary.CallMethodOther(Instance: TObject;
  ClassType: TClass; const MethodName: String; Caller: TfsMethodHelper): Variant;

var
  vTmp :Variant;

begin

 if MethodName = 'SWAP' then begin
   vTmp := Caller.Params[1];
   Caller.Params[1] := Caller.Params[0];
   Caller.Params[0] := vTmp;
 end else
 if MethodName = 'TSTRINGSTOSTRING' then begin
   Result := StringReplace(TStrings(Integer(Caller.Params[0])).Text, CHR(13), '#13', [rfReplaceAll]);
   Result := StringReplace(Result, CHR(10), '#10', [rfReplaceAll]);
 end else
 if MethodName = 'STRINGTOTSTRINGS' then begin
    Result := StringReplace(Caller.Params[0], '#13', CHR(13), [rfReplaceAll]);
    Result := StringReplace(Result, '#10', CHR(10), [rfReplaceAll]);
 end; { if }

end; { CallMethodOther }

{--------------------------------------------------------------------}
{ Инициализация математических ф-ий библиотеки                       }
{--------------------------------------------------------------------}
function TAddFunctionLibrary.CallMethodMath(Instance: TObject;
  ClassType: TClass; const MethodName: String; Caller: TfsMethodHelper): Variant;

begin

 if MethodName = 'ROUNDTO' then
   Result := RoundTo(Caller.Params[0], Caller.Params[1])
 else
 if MethodName = 'SIMPLEROUNDTO' then
   Result := SimpleRoundTo(Caller.Params[0], Caller.Params[1]);

end; { CallMethodMath }

{--------------------------------------------------------------------}
{ Конвертирование из типа String в тип TfrCharSet                    }
{--------------------------------------------------------------------}
function TAddFunctionLibrary.ConvCS(cStr :String) :TfrCharSet;
var
  i :Integer;

begin

 Result := [];
 for i := 1 to Length(cStr) do Include(Result, AnsiString(cStr)[i]);

end; { ConvCS }

procedure Register;
begin
 RegisterComponents('FastReport 4.0', [TfrxAddFunctionLibrary]);
end; { Register }

initialization
  fsRTTIModules.Add(TAddFunctionLibrary);

finalization
  if fsRTTIModules <> nil then
    fsRTTIModules.Remove(TAddFunctionLibrary);

end.

