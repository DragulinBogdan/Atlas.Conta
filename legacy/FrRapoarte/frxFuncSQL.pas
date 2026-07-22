{*******************************************************}
{                                                       }
{         Add FastReport 4.0 SQL Support Lbrary         }
{                                                       }
{       Copyright (c) 2001-2006 by Stalker SoftWare     }
{                                                       }
{*******************************************************}

unit frxFuncSQL;

interface

uses
  SysUtils;

 function frCreateStr(cStr :String) :String;
 function frCreateNum(cNum :String ) :String;
 function frCreateDate(cDate :String; cFFormatDate: String) :String;


{$I frx.inc}


 // StLib

implementation

uses
   frxFuncStr, frxFuncDate;
   
{--------------------------------------------------------------------}
{ Возвращает строку cStr обрамленную одинарными кавычками для        }
{ создания SQL запроса                                               }
{--------------------------------------------------------------------}
function frCreateStr(cStr :String) :String;
begin
 if Trim(cStr) = '' then
   Result := 'null'
 else
   Result := CHR(39)+cStr+CHR(39);
end; { frCreateStr }

{--------------------------------------------------------------------}
{ Возвращает обработанную строку с числом для создания SQL запроса.  }
{ Возможная запятая в строке заменяется точкой.                      }
{--------------------------------------------------------------------}
function frCreateNum(cNum :String) :String;
begin
 if Trim(cNum) = '' then
   Result := 'null'
 else
   Result := frReplaceStr(cNum,DecimalSeparator,'.');
end; { frCreateNum }

{--------------------------------------------------------------------}
{ Возвращает обработанную строку cDate обрамленную одинарными        }
{ кавычками с датой для создания SQL запроса.                        }
{ cFEmptyDate   это пустая дата, которую может например возвратить   }
{               RxDateEdit.Text. Пример '  .  .    '                 }
{ cFFormatDate  это формат даты который понимает ваш SQL сервер.     }
{               Пример 'yyyy/mm/dd'                                  }
{--------------------------------------------------------------------}
function frCreateDate(cDate :String; cFFormatDate: String) :String;
begin

 if not frValidDate(cDate) then
   Result := 'null'
 else
   Result := CHR(39)+FormatDateTime( cFFormatDate, StrToDateTime(cDate) )+CHR(39);

end; { frCreateDate }

end.
