{*******************************************************}
{                                                       }
{         Add FastReport 4.0 Date Lbrary                }
{                                                       }
{         Copyright (c) 1995, 1996 AO ROSNO             }
{         Copyright (c) 1997, 1998 Master-Bank          }
{                                                       }
{      Copyright (c) 2001-2009 by Stalker SoftWare      }
{                                                       }
{*******************************************************}
unit frxFuncDate;

interface

{$B-,V-,R-,Q-}
{$I frx.inc}

uses
  SysUtils;

 // RxLib
 function frDaysPerMonth(nYear, nMonth: Integer): Integer;
 function frFirstDayOfNextMonth(dDate:TDateTime): TDateTime;
 function frFirstDayOfPrevMonth(dDate:TDateTime): TDateTime;
 function frLastDayOfPrevMonth(dDate:TDateTime): TDateTime;
 function frIncYear(dDate: TDateTime; nDelta: Integer): TDateTime;
 function frIncDay(dDate: TDateTime; nDelta: Integer): TDateTime;
 function frIncDate(dDate: TDateTime; nDays, nMonths, nYears: Integer): TDateTime;
 function frIncTime(dTime: TDateTime; nHours, nMinutes, nSeconds, nMSecs :Integer): TDateTime;
 procedure frDateDiff(dDate1, dDate2: TDateTime; var nDays, nMonths, nYears: Word);

 // StLib
 function frIsRangeDate(dBegDate, dEndDate, dDate: TDateTime) :Boolean;
 function frStrToDateDef(cDate: String; dDefault: TDateTime): TDateTime;
 function frValidDate(cDate :String) :Boolean;
 function frIsLeapYear(nYear: Integer): Boolean;
 function frIncMonth(dDate: TDateTime; nDelta: Integer): TDateTime;
 function frQuarterOf(dDate :TDateTime) :Integer;
 function frDay(dDate :TDateTime) :Word;
 function frMonth(dDate :TDateTime) :Word;
 function frYear(dDate :TDateTime) :Word;

 function frWeek(dDate :TDateTime) :Word;

implementation

uses
  frxFuncNum;

{--------------------------------------------------------------------}
{ Функция возвращает True, если год nYear високосный                 }
{--------------------------------------------------------------------}
function frIsLeapYear(nYear: Integer): Boolean;
begin
 Result := (nYear mod 4 = 0) and ((nYear mod 100 <> 0) or (nYear mod 400 = 0));
end; { frIsLeapYear }

{--------------------------------------------------------------------}
{ Функция возвращает число дней в месяце для указанного года.        }
{ Параметр nYear задает год, а nMonth - месяц                        }
{--------------------------------------------------------------------}
function frDaysPerMonth(nYear, nMonth: Integer): Integer;
const
  DaysInMonth: array[1..12] of Integer =
    (31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31);

begin

 Result := DaysInMonth[nMonth];
 if (nMonth = 2) and frIsLeapYear(nYear) then Inc(Result); // leap-year Feb is special

end; { frDaysPerMonth }

{--------------------------------------------------------------------}
{ Возвращает первый день следующего месяца относительно даты dDate   }
{ в виде даты TDateTime.                                             }
{--------------------------------------------------------------------}
function frFirstDayOfNextMonth(dDate:TDateTime): TDateTime;
var
  Year, Month, Day: Word;

begin

 DecodeDate(dDate, Year, Month, Day);
 Day := 1;
 if Month < 12 then
   Inc(Month)
 else begin
   Inc(Year);
   Month := 1;
 end; { if }
 Result := EncodeDate(Year, Month, Day);

end; { frFirstDayOfNextMonth }

{--------------------------------------------------------------------}
{ Возвращает первый день предыдущего месяца относительно даты dDate  }
{ в виде даты TDateTime.                                             }
{--------------------------------------------------------------------}
function frFirstDayOfPrevMonth(dDate:TDateTime): TDateTime;
var
  Year, Month, Day: Word;

begin

  DecodeDate(dDate, Year, Month, Day);
  Day := 1;
  if Month > 1 then
    Dec(Month)
  else begin
    Dec(Year);
    Month := 12;
  end;
  Result := EncodeDate(Year, Month, Day);

end; { frFirstDayOfPrevMonth }

{--------------------------------------------------------------------}
{ Возвращает последний день предыдущего месяца относительно даты     }
{ dDate в виде даты TDateTime.                                       }
{--------------------------------------------------------------------}
function frLastDayOfPrevMonth(dDate:TDateTime): TDateTime;
var
  D: TDateTime;
  Year, Month, Day: Word;

begin

  D := frFirstDayOfPrevMonth(dDate);
  DecodeDate(D, Year, Month, Day);
  Day := frDaysPerMonth(Year, Month);
  Result := EncodeDate(Year, Month, Day);

end; { frLastDayOfPrevMonth }

{--------------------------------------------------------------------}
{ Увеличивает дату dDate на заданное количество дней, месяцев и лет, }
{ возвращая полученную дату как результат.                           }
{--------------------------------------------------------------------}
function frIncDate(dDate: TDateTime; nDays, nMonths, nYears: Integer): TDateTime;
var
  D, M, Y: Word;
  Day, Month, Year: LongInt;

begin

 DecodeDate(dDate, Y, M, D);
 Year := Y; Month := M; Day := D;
 Inc(Year, nYears);
 Inc(Year, nMonths div 12);
 Inc(Month, nMonths mod 12);

 if Month < 1 then begin
   Inc(Month, 12);
   Dec(Year);
 end
 else
 if Month > 12 then begin
   Dec(Month, 12);
   Inc(Year);
 end; { if }

 if Day > frDaysPerMonth(Year, Month) then Day := frDaysPerMonth(Year, Month);
 Result := EncodeDate(Year, Month, Day) + nDays + Frac(dDate);

end; { frIncDate }

{--------------------------------------------------------------------}
{ Увеличивает время ATime на заданное количество часов, минут,       }
{ секунд и миллисекунд и возвращает полученное время как результат.  }
{--------------------------------------------------------------------}
function frIncTime(dTime: TDateTime; nHours, nMinutes, nSeconds, nMSecs :Integer): TDateTime;
begin

 Result := dTime + (nHours div 24) + (((nHours mod 24) * 3600000 +
   nMinutes * 60000 + nSeconds * 1000 + nMSecs) / MSecsPerDay);

 if Result < 0 then Result := Result + 1;

end; { frIncTimeEx }

{--------------------------------------------------------------------}
{ Увеличивает дату dDate на заданное количество nDelta дней,         }
{ возвращая полученную дату как результат.                           }
{--------------------------------------------------------------------}
function frIncDay(dDate: TDateTime; nDelta: Integer): TDateTime;
begin
 Result := dDate + nDelta;
end; { frIncDay }

{--------------------------------------------------------------------}
{ Увеличивает дату dDate на заданное количество nDelta лет,          }
{ возвращая полученную дату как результат.                           }
{--------------------------------------------------------------------}
function frIncYear(dDate: TDateTime; nDelta: Integer): TDateTime;
begin
 Result := frIncDate(dDate, 0, 0, nDelta);
end;  { frIncYear }

{--------------------------------------------------------------------}
{ Увеличивает дату dDate на заданное количество nDelta месяцев,      }
{ возвращая полученную дату как результат.                           }
{--------------------------------------------------------------------}
function frIncMonth(dDate: TDateTime; nDelta: Integer): TDateTime;
begin
 Result := frIncDate(dDate, 0, nDelta, 0);
end;  { frIncYear }

{--------------------------------------------------------------------}
{ Определяет разницу между датами, заданными Date1 и Date2 в днях,   }
{ месяцах и годах.                                                   }
{--------------------------------------------------------------------}
procedure frDateDiff(dDate1, dDate2: TDateTime; var nDays, nMonths, nYears: Word);
{ Corrected by Anatoly A. Sanko (2:450/73) }
var
  DtSwap: TDateTime;
  Day1, Day2, Month1, Month2, Year1, Year2: Word;
 
begin

 if dDate1 > dDate2 then begin
   DtSwap := dDate1;
   dDate1 := dDate2;
   dDate2 := DtSwap;
 end; { if }

 DecodeDate(dDate1, Year1, Month1, Day1);
 DecodeDate(dDate2, Year2, Month2, Day2);
 nYears := Year2 - Year1;
 nMonths := 0;
 nDays := 0;

 if Month2 < Month1 then begin
   Inc(nMonths, 12);
   Dec(nYears);
 end; { if }

 Inc(nMonths, Month2 - Month1);
 if Day2 < Day1 then begin
   Inc(nDays, frDaysPerMonth(Year1, Month1));
   if nMonths = 0 then begin
     Dec(nYears);
     nMonths := 11;
   end
   else Dec(nMonths);
 end; { if }
 Inc(nDays, Day2 - Day1);

end; { frDateDiffEx }

{----------------------------------------------------------------}
{ Возвращает True если указанная дата находится в заданном       }
{ диапазоне                                                      }
{ vBegDate  - Начала диапазона                                   }
{ vEndDate  - Конец диапазона                                    }
{ vDate     - Проверяемое число                                  }
{----------------------------------------------------------------}
function frIsRangeDate(dBegDate, dEndDate, dDate: TDateTime) :Boolean;
begin

 if (dDate >= dBegDate) and (dDate <= dEndDate) then
   Result := True
 else
   Result := False

end; { frIsRangeDate }

{--------------------------------------------------------------------}
{ Возвращает True, если cDate действительно является датой           }
{--------------------------------------------------------------------}
function frValidDate(cDate :String) :Boolean;
begin

 Result := True;
 try
   StrToDate(cDate)
 except
   Result := False;
 end; { try }

end; { frValidDate }

{----------------------------------------------------------------}
{ Конвертирует строку в дату. В случае ошибки конвертации        }
{ возвращает значение по умолчанию.                              }
{----------------------------------------------------------------}
function frStrToDateDef(cDate: String; dDefault: TDateTime): TDateTime;
begin

 try
   Result := StrToDate(cDate)
 except
   Result := dDefault;
 end; { try }

end; { frStrToDateDef }

{--------------------------------------------------------------------}
{ Возвращает номер квартала даты dDate.                              }
{--------------------------------------------------------------------}
function frQuarterOf(dDate :TDateTime) :Integer;
var
  nDay, nMonth, nYear: Word;

begin

 Result := 0;

 DecodeDate(dDate, nYear, nMonth, nDay);

 if frIsRangeNum(1, 3, nMonth) then
   Result := 1
 else
 if frIsRangeNum(4, 6, nMonth) then
   Result := 2
 else
 if frIsRangeNum(7, 9, nMonth) then
   Result := 3
 else
 if frIsRangeNum(10, 12, nMonth) then
   Result := 4

end; { QuarterOf }

{--------------------------------------------------}
{ Возвращает Число месяца по дате                  }
{--------------------------------------------------}
function frDay(dDate :TDateTime) :Word;
var
  nYear, nMonth, nDay: Word;

begin

 DecodeDate(dDate, nYear, nMonth, nDay);
 Result := nDay ;

end; { frDay }

{--------------------------------------------------}
{ Возвращает номер месяца по дате                  }
{--------------------------------------------------}
function frMonth(dDate :TDateTime) :Word;
var
  nYear, nMonth, nDay: Word;

begin

 DecodeDate(dDate, nYear, nMonth, nDay);
 Result := nMonth ;

end; { frMonth }

{--------------------------------------------------}
{ Возвращает год по дате                           }
{--------------------------------------------------}
function frYear(dDate :TDateTime) :Word;
var
  nYear, nMonth, nDay: Word;

begin

 DecodeDate(dDate, nYear, nMonth, nDay);
 Result := nYear ;

end; { frYear }

{--------------------------------------------------}
{ Возвращает номер недели по дате                  }
{ Interpretation of day numbers:                   }
{             ISO: 1 = Monday, 7 = Sunday          }
{ Delphi SysUtils: 1 = Sunday, 7 = Saturday        }
{--------------------------------------------------}
function frWeek(dDate :TDateTime) :Word;
var
  nDay, nMonth, nYear :Word;
  dFirstDate          :TDateTime;
  nDateDiff           :Integer;

begin

 nDay       := SysUtils.DayOfWeek(dDate)-1;
 dDate      := dDate + 3 - ((6 + nDay) mod 7);

 DecodeDate(dDate, nYear, nMonth, nDay);

 dFirstDate := EncodeDate(nYear, 1, 1);
 nDateDiff  := Trunc(dDate - dFirstDate);

 Result := 1 + (nDateDiff div 7);

end; { frWeek }

end.
