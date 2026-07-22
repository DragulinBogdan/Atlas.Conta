{*******************************************************}
{                                                       }
{         Add FastReport 4.0 Numeric Lbrary             }
{                                                       }
{      Copyright (c) 2001-2007 by Stalker SoftWare      }
{                                                       }
{*******************************************************}

unit frxFuncNum;

interface

{$A+,B-,E-,R-}
{$I frx.inc}

uses
  SysUtils;

 // StLib
 function frIsRangeNum(nBeg, nEnd, nValue: Extended) :Boolean;
 function frStrToFloatDef(cFlt:String; nFltDef :Extended) :Extended;

implementation

{----------------------------------------------------------------}
{ Возвращает True если указанное число находится в заданном      }
{ диапазоне                                                      }
{ nBeg   - Начала диапазона                                      }
{ nEnd   - Конец диапазона                                       }
{ nValue - Проверяемое число                                     }
{----------------------------------------------------------------}
function frIsRangeNum( nBeg, nEnd, nValue: Extended ) :Boolean;
begin

  if (nValue >= nBeg) and (nValue <= nEnd) then
    Result := True
  else
    Result := False

end; { IsRangeNum }

{--------------------------------------------------------------------}
{ Конвертирует строку в тип Float. В случае ошибки конвертации       }
{ возвращает значение по умолчанию.                                  }
{--------------------------------------------------------------------}
function frStrToFloatDef(cFlt:String; nFltDef :Extended) :Extended;
begin

 try
   Result := StrToFloat(cFlt);
 except
   Result := nFltDef;
 end; { try }

end; { frStrToFloatDef }

end.
