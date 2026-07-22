{*******************************************************}
{                                                       }
{         Add FastReport 4.0 String Lbrary              }
{                                                       }
{         Copyright (c) 1995, 1996 AO ROSNO             }
{         Copyright (c) 1997, 1998 Master-Bank          }
{                                                       }
{     Copyright (c) 2001-2008 by Stalker SoftWare       }
{                                                       }
{*******************************************************}

unit frxFuncStr;

interface

{$A+,B-,E-,R-}
{$I frx.inc}

uses
  SysUtils;

type
{$IFNDEF Delphi12}
  TfrCharSet = set of Char;
{$ELSE}
  TfrCharSet = TSysCharSet;
{$ENDIF}

 // RxLib
 function frWordPosition(const N: Integer; const S: string; const WordDelims: TfrCharSet): Integer;
 function frExtractWord(N: Integer; const S: string; const WordDelims: TfrCharSet): string;
 function frWordCount(const S: string; const WordDelims: TfrCharSet): Integer;
 function frIsWordPresent(const W, S: string; const WordDelims: TfrCharSet): Boolean;
 function frNPos(const C: string; S: string; N: Integer): Integer;
 function frReplaceStr(const S, Srch, Replace: string): string;

 // StLib
 function frReplicate(cStr: String; nLen :Integer) :String;
 function frPadRight(cStr: String; nLen: Integer; cChar :String) :String;
 function frPadLeft(cStr: String; nLen: Integer; cChar :String) :String;
 function frPadCenter( cStr: String; nWidth: Integer; cChar: String): String;
 function frEndPos(cStr, cSubStr: String) :Integer;
 function frCompareStr(cStr1, cStr2: String) :Integer;

 function frLeftCopy(cStr: String; nNum: Integer): String;
 function frRightCopy(cStr: String; nNum: Integer): String;

implementation


{$IFNDEF Delphi12}
function CharInSet(C: Char; const CharSet: TSysCharSet): Boolean;
begin
 Result := C in CharSet;
end; { CharInSet }
{$ENDIF}

{--------------------------------------------------------------------}
{ Возвращает позицию первого символа N-го слова в строке S, используя}
{ параметр WordDelims (типа TCharSet) как разделитель между словами  }
{--------------------------------------------------------------------}
function frWordPosition(const N: Integer; const S: string; const WordDelims: TfrCharSet): Integer;
var
  Count, I: Integer;

begin

 Count := 0;
 I := 1;
 Result := 0;
 while (I <= Length(S)) and (Count <> N) do begin
   { skip over delimiters }
   while (I <= Length(S)) and CharInSet(S[I], WordDelims) do Inc(I);
   { if we're not beyond end of S, we're at the start of a word }
   if I <= Length(S) then Inc(Count);
   { if not finished, find the end of the current word }
   if Count <> N then
     while (I <= Length(S)) and not CharInSet(S[I], WordDelims) do Inc(I)
   else Result := I;
 end; { while }

end; { frWordPosition }

{--------------------------------------------------------------------}
{ Выделяет N-ое слово из строки S, используя WordDelims как          }
{ разделитель между словами                                          }
{--------------------------------------------------------------------}
function frExtractWord(N: Integer; const S: string; const WordDelims: TfrCharSet): string;
var
  I: Integer;
  Len: Integer;

begin

 Len := 0;
 I := frWordPosition(N, S, WordDelims);
 if I <> 0 then
   { find the end of the current word }
   while (I <= Length(S)) and not CharInSet(S[I], WordDelims) do begin
     { add the I'th character to result }
     Inc(Len);
     SetLength(Result, Len);
     Result[Len] := S[I];
     Inc(I);
   end; { while }
 SetLength(Result, Len);

end; { frExtractWord }

{--------------------------------------------------------------------}
{ Считает число слов в строке S, используя параметр WordDelims как   }
{ разделитель между словами                                          }
{--------------------------------------------------------------------}
function frWordCount(const S: string; const WordDelims: TfrCharSet): Integer;
var
  SLen, I: Cardinal;

begin

 Result := 0;
 I := 1;
 SLen := Length(S);
 while I <= SLen do begin
   while (I <= SLen) and CharInSet(S[I], WordDelims) do Inc(I);
   if I <= SLen then Inc(Result);
   while (I <= SLen) and not CharInSet(S[I], WordDelims) do Inc(I);
 end; { while }

end; { frWordCount }

{--------------------------------------------------------------------}
{ Определяет, присутствует ли слово W в строке S, используя символы  }
{ WordDelims как возможные разделители между словами                 }
{--------------------------------------------------------------------}
function frIsWordPresent(const W, S: string; const WordDelims: TfrCharSet): Boolean;
var
  Count, I: Integer;

begin

 Result := False;
 Count := frWordCount(S, WordDelims);
 for I := 1 to Count do
   if frExtractWord(I, S, WordDelims) = W then begin
     Result := True;
     Exit;
   end; { if }

end; { frIsWordPresent }

{--------------------------------------------------------------------}
{ Ищет позицию N-го вхождения подстроки C в стpоке S                 }
{--------------------------------------------------------------------}
function frNPos(const C: string; S: string; N: Integer): Integer;
var
  I, P, K: Integer;

begin

 Result := 0;
 K := 0;
 for I := 1 to N do begin
   P := Pos(C, S);
   Inc(K, P);
   if (I = N) and (P > 0) then begin
     Result := K;
     Exit;
   end; { if }
   if P > 0 then Delete(S, 1, P)
   else Exit;
 end; { for }

end; { frNPos }

{--------------------------------------------------------------------}
{ Функция заменяет в строке S все вхождения подстроки Srch на        }
{ подстроку, переданную в качестве аргумента Replace.                }
{--------------------------------------------------------------------}
function frReplaceStr(const S, Srch, Replace: string): string;
var
  I: Integer;
  Source: string;

begin

 Source := S;
 Result := '';
 repeat
   I := Pos(Srch, Source);
   if I > 0 then begin
     Result := Result + Copy(Source, 1, I - 1) + Replace;
     Source := Copy(Source, I + Length(Srch), MaxInt);
   end
   else Result := Result + Source;
 until I <= 0;

end; { frReplaceStr }

{--------------------------------------------------------------------}
{ Возвращает nLen символов вида String                               }
{--------------------------------------------------------------------}
function frReplicate(cStr: String; nLen :Integer) :String;
var
  nCou :Integer;

begin

 Result := '';
 for nCou := 1 to nLen do
   Result := Result + cStr;

end; { Replicate }

{--------------------------------------------------------------------}
{ Возвращает Строку заполненую символами cChar Слева до длины nLen   }
{--------------------------------------------------------------------}
function frPadLeft(cStr: String; nLen: Integer; cChar :String) :String;
var
  S :String;

begin

 S := Trim(cStr);
 Result := frReplicate(cChar, nLen-Length(S))+S;

end ; { frPadLeft }

{--------------------------------------------------------------------}
{ Возвращает Строку заполненую символами cChar Справа до длины nLen  }
{--------------------------------------------------------------------}
function frPadRight(cStr: String; nLen: Integer; cChar :String) :String ;
var
  S :String;

begin

 S := Trim(cStr);
 Result := S+frReplicate(cChar, nLen-Length(S));

end; { frPadRight }

{--------------------------------------------------------------------}
{ Возвращает центрированую строку заполненую символами cChar с обоих }
{ сторон                                                             }
{--------------------------------------------------------------------}
function frPadCenter( cStr: String; nWidth: Integer; cChar: String): String;
var
  nPerSide :Integer;
  cResult  :String;

begin

 nPerSide := (nWidth - Length(cStr)) div 2;
 cResult := frPadLeft(cStr, (Length(cStr) + nPerSide), cChar);
 Result := frPadRight(cResult, nWidth, cChar);

end; { frPadCenter }

{----------------------------------------------------------------}
{ Ищет с строке подстроку начиная с конца и возвращает номер     }
{ позиции с которого он нашел подстроку или 0 если ненашел       }
{----------------------------------------------------------------}
function frEndPos(cStr, cSubStr: String) :Integer;
var
  nCou   :Integer;
  nLenSS :Integer;
  nLenS  :Integer;

begin

 nLenSS := Length(cSubStr);
 nLenS  := Length(cStr);
 Result := 0 ;

 if nLenSS > nLenS then Exit;

 for nCou := nLenS downto 1 do
   if Copy( cStr, nCou, nLenSS ) = cSubStr then begin
     Result := nCou;
     Exit;
   end; { if }

end; { frEndPos }

{--------------------------------------------------------------------}
{ Возвращает подстроку начиная с самого первого символа              }
{--------------------------------------------------------------------}
function frLeftCopy( cStr: String; nNum: Integer ): String;
begin
 Result := Copy( cStr, 1, nNum );
end; { frLeftCopy }

{--------------------------------------------------------------------}
{ Возвращает подстроку начиная с самого последнего символа           }
{--------------------------------------------------------------------}
function frRightCopy( cStr: String; nNum: Integer ): String;
begin
 Result := '';
 if nNum > Length( cStr ) then Exit;
 Result := Copy( cStr, (Length(cStr) - nNum + 1), Length(cStr) );
end; { frRightCopy }

{----------------------------------------------------------------}
{ Сравнивает 1-ую и 2-ую стороку и возвращает номер позиции      }
{ начиная с которого 1-ая строка отличается от 2-ой строки.      }
{----------------------------------------------------------------}
function frCompareStr(cStr1, cStr2: String) :Integer;
var
  nLenMax :Integer;
  nCou    :Integer;

begin

 Result := 0;

 if Length( cStr1 ) > Length( cStr2 ) then
   nLenMax := Length( cStr1 )
 else
   nLenMax := Length( cStr2 );

 for nCou := 1 to nLenMax do
   if Copy( cStr1, nCou, 1) <> Copy( cStr2, nCou, 1) then begin
     Result := nCou;
     Exit;
   end; { if }

end; { frCompareStr }

end.
