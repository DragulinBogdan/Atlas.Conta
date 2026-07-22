{******************************************}
{                                          }
{             FastReport v4.0              }
{         Language resource file           }
{                                          }
{         Copyright (c) 1998-2008          }
{           by Fast Reports Inc.           }
{                                          }
{         Copyright (c) 2001-2008          }
{           by Stalker SoftWare            }
{                                          }
{******************************************}
unit frxrcAddFunction;

interface

procedure frxAddDesignerRes();

implementation

uses
  frxRes;

procedure frxAddDesignerRes();
begin

  with frxResources do begin

    // SQL
    Add('CreateStr', 'Returns handled string <cStr> framed with single quotation marks for SQL query creation.');
    Add('CreateNum', 'Returns handled string <cStr> with number for SQL query creation. Possible point in string is replaced by dot');
    Add('CreateDate', 'Returns handled string <cStr> framed with single quotation marks. With date for SQL query creation. Before function usage place desirable date format (for example, dd/mm/yyyy) into the TfrAddFunctionLibrary.FormatDate property.');

    // String
    Add('WordPosition', 'Returns first symbol position of N <nNum> word in <cStr> string by usage of symbols in <cWordDelims> as spacers between words, and returns result.');
    Add('ExtractWord', 'Extracts N <nNum> word from <cStr> string by usage of symbols in <cWordDelims> as spacers between words, and returns result.');
    Add('WordCount', 'Counts number of words in <cStr> string by usage of symbols in <cWordDelims> as spacers between words and returns result.');
    Add('IsWordPresent', 'Defines whether <cWord> word is present in <cStr> string by usage of symbols in <cWordDelims> as spacers between words, and returns result.');
    Add('NPos', 'Searches substring <cSubStr>  N <nNum> entry position in <cStr> string, and returns result.');
    Add('ReplaceStr', 'Replaces all substring <cSrch> entries in <cStr1> string by <cReplace> substring, and returns result.');
    Add('Replicate', 'Returns line of <nLen> length consisting of <cStr> symbols.');
    Add('PadRight', 'Complements <cStr> string to the right to <nLen> length by symbols from <cChar> string, and returns result.');
    Add('PadLeft', 'Complements <cStr> string to the left to <nLen> length by symbols from <cChar> string, and returns result.');
    Add('PadCenter', 'Complements <cStr> string to the left and to the right to <nLen> length by symbols from <cChar> string centering it, and returns result.');
    Add('EndPos', 'Searches <cSubStr> substring in <cStr> string beginning with end, and returns position from which it finds substring.');
    Add('CompareStr', 'Compares <cStr1> string to <cStr2> string and returns position number from which <cStr1> string differs from <cStr2> string or 0, if they are equal.');
    Add('LeftCopy', 'Copies from <cStr> string starting with <nCount> symbols beginning and returns result.');
    Add('RightCopy', 'Copies from <String> string starting with <nCount> symbols ending and returns result.');
    Add('TrimLeft', 'Deletes left blanks from <cStr> string and returns result.');
    Add('TrimRight', 'Deletes right blanks from <cStr> string and returns result.');

    // Convert
    Add('StrToFloatDef', 'Converts <cFlt> string into number with floating point and returns result. In case of converting mistake it returns <nFltDef> default value.');
    Add('StrToIntDef', 'Converts <cStr> string into integer number and returns result. In case of converting mistake it returns <nDefault> default value.');
    Add('StrToDateDef', 'Converts <cDate> string into date and returns result. In case of converting mistake it returns <dDefault> default value.');

    // Date
    Add('DaysPerMonth', 'Returns number of days in month number <nMonth> for <nYear> year.');
    Add('FirstDayOfNextMonth', 'Returns first day of next month with regard to <dDate> date in form of date.');
    Add('FirstDayOfPrevMonth', 'Returns first day of previous month with regard to <dDate> date in form of date.');
    Add('LastDayOfPrevMonth', 'Returns last day of next month with regard to <dDate> date in form of date.');
    Add('IncYear', 'Increases <dDate> date to set number of years <nYear> and returns received date as result.');
    Add('IncDay', 'Increases <dDate> date to set number of days <nDelta> and returns received date as result.');
    Add('IncMonth', 'Increases <dDate> date at set number of months <nDelta> and returns received date as result.');
    Add('IncDate', 'Increases <dDate> date at <nDays> set number of days, <nMonth> number of months and <nYears> number of years, and returns received date as result.');
    Add('IncTime', 'Increases <dTime> time at <nHours> set number of hours, <nMinutes> number of minutes, <nSeconds> number of seconds and <nMSecs> number of milliseconds, and returns received time as result.');
    Add('DateDiff', 'Defines difference between <dDate1> and <dDate2> dates in <nDays> days, <nMonths> months, <nYears> years.');
    Add('QuarterOf', 'Returns date quarter number from <dDate>.');
    Add('GetYear', 'Returns year from <dDate> date.');
    Add('GetDay', 'Returns day from <dDate> date.');
    Add('GetMonth', 'Returns month number from <dDate> date.');
    Add('GetWeek', 'Returns week number from <dDate> date (based on ISO standard 8601)');

    // Other
    Add('Swap', 'Change <vVar1> and <vVar2> variables places. Variables can be used or controls properties can be used as variables. Variables can be of any type.');;
    Add('IsRangeNum', 'Returns True, if <nValue> is located in range between <nBeg> number and <nEnd> number.');
    Add('IsRangeDate', 'Returns True, if <dDate> is located in range between <dBegDate> date and <dEndDate> date.');
    Add('TStringsToString', 'TStrings string folding converting into simple string folding with #13#10 spacers.');
    Add('StringToTStrings', '<cStrings>  string folding converting into folding for TStrings.');

    // Variant
    Add('VarArrayOf','Creates and fills Variant univariate array.');
    Add('VarArrayRedim','Changes <A> array Variant sizes to <nHighBound> size.');   
    Add('VarFromDateTime','Converts <dDateTime> into Variant.');
    Add('VarToDateTime','Converts <V> into TDateTime.');
    Add('VarInRange', 'Returns True, if <AValue> is placed in range between <AMin> and <AMax>.');
    Add('VarIsClear','Returns True, if <A> is unassigned.');
    Add('VarIsArray','Returns True, if <V> is array.');

    // Math
    Add('RoundTo','Rounds a floating-point value to a specified digit or power of ten using "Banker`s rounding". <AValue> is the value to round. <ADigit> indicates the power of ten to which you want <AValue> rounded.'+
                  ' It can be any value from -37 to 37 (inclusive).  This method rounds to an even number in the case that <AValue> is not nearer to either value.');
    Add('SimpleRoundTo', 'Rounds a floating-point value to a specified digit or power of ten using asymmetric arithmetic rounding. <AValue> is the value to round. <ADigit> indicates the power of ten to which you want <AValue> rounded.'+
                  ' It can be any value from -37 to 37 (inclusive). This method always rounds to the larger value.');

  end; { with }

end; { frxAddDesignerRes }

initialization
  frxAddDesignerRes();

end.
