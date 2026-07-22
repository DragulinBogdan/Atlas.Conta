unit frAtlasFunctions;

interface

implementation

uses
  SysUtils, Classes, fs_iinterpreter, CommonDBVar;

type
  TAtlasFunctions = class(TfsRTTIModule)
  private
    function CallMethod(Instance: TObject; ClassType: TClass;
      const MethodName: String; var Params: Variant): Variant;
  public
    constructor Create(AScript: TfsScript); override;
  end;

function Adunare(a, b: Integer): Integer;
begin
  Result := a + b;
end;

function AdresaUnitate: String;
begin
  Result := CommonDBVar.antetAdresaSocietate;
end;

function CodFiscalUnitate: String;
begin
  Result := CommonDBVar.antetCodFiscal;
end;

function DenumireUnitate: String;
begin
  Result := CommonDBVar.antetNumeSocietate;
end;

function EmailUnitate: String;
begin
  Result := CommonDBVar.antetEmail;
end;

function JudetUnitate: String;
begin
  Result := CommonDBVar.antetJudet;
end;

function LocalitateUnitate: String;
begin
  Result := CommonDBVar.antetLocalitate;
end;

function TelefonUnitate: String;
begin
  Result := CommonDBVar.antetTelefon;
end;

function NumeCompletOperator: String;
begin
  Result := CommonDBVar.NumeLoginComplet;
end;

function NumeOperator: String;
begin
  Result := CommonDBVar.NumeLogin;
end;

function MoneySpell(Value:Currency):String;
Const

  aFirst20Str: array[0..19] of String =
     ('', 'unu', 'doi', 'trei', 'patru', 'cinci', 'sase', 'sapte', 'opt', 'noua', 'zece',
      'unsprezece', 'doisprezece', 'treisprezece', 'paisprezece', 'cincisprezece',
      'saisprezece', 'saptesprezece', 'optsprezece', 'nouasprezece');
  aZeciStr: array[1..8] of String =
     ('douazeci', 'treizeci', 'patruzeci', 'cincizeci', 'saizeci', 'saptezeci', 'optzeci', 'nouazeci');


    function IntegerToSpell(aInt: Int64;IsNested: Boolean=False): String;
    var FirstPart,
        SecondPart: Int64;
    begin
      if aInt >= 1000000000 then begin
         FirstPart  := aInt div 1000000000;
         SecondPart := aInt mod 1000000000;
         if FirstPart = 1 then Result := 'miliard'
         else if FirstPart > 19 then Result := IntegerToSpell(FirstPart, True)+' ' + 'de' + ' '+'miliarde'
              else Result := IntegerToSpell(FirstPart, True)+' '+'miliarde';
         Result := Result + ' ' + IntegerToSpell(SecondPart, True);
      end
      else
      if aInt >= 1000000 then begin
         FirstPart  := aInt div 1000000;
         SecondPart := aInt mod 1000000;
         if FirstPart = 1 then Result := 'un milion'
         else if FirstPart > 19 then Result := IntegerToSpell(FirstPart, True)+' ' + 'de' + ' '+'milioane'
              else Result := IntegerToSpell(FirstPart, True)+' '+'milioane';
         Result := Result + ' ' + IntegerToSpell(SecondPart, True);
      end
      else
        if aInt >= 1000 then begin
           FirstPart  := aInt div 1000;
           SecondPart := aInt mod 1000;
           if FirstPart = 1 then Result := 'o mie'
           else if FirstPart mod 100 > 19 then Result := IntegerToSpell(FirstPart, True)+ ' ' +'de' + ' '+'mii'
                else Result := IntegerToSpell(FirstPart, True)+' '+'mii';
           Result := Result + ' ' + IntegerToSpell(SecondPart, True);
        end
        else
          if aInt >= 100 then begin
             FirstPart  := aInt div 100;
             SecondPart := aInt mod 100;
             if FirstPart = 1 then Result := 'o suta'
             else Result := IntegerToSpell(FirstPart, True)+' '+'sute';
             Result := Result + ' ' + IntegerToSpell(SecondPart, True);
          end
          else
            if aInt >= 20 then begin
               FirstPart  := aInt div 10;
               SecondPart := aInt mod 10;
               Result := aZeciStr[FirstPart-1];
               if SecondPart > 0 then Result := Result + ' ' +'si' + ' ' + IntegerToSpell(SecondPart, True);
               //if FirstPart > 0 then Result := Result + 'de';
            end
            else
               if (aInt = 2) and (IsNested)  then
                  Result := 'doua'
               else Result := aFirst20Str[aInt];
    end;


  function CurrencyToSpell(aValue: Currency): String;
  begin
    if aValue <0 then begin
      Result := 'minus';
      aValue := Abs(aValue);
    end
    else Result := '';
    Result :=  Result + 
      IntegerToSpell(Trunc(aValue));
    if (aValue > 100) and (Trunc(aValue) mod 100> 20) then
       Result := Result + ' ' + 'de';
    Result := Result + ' ' + 'lei';
    if Round(Frac(aValue)*100) > 0 then
       Result := Result + ' ' + 'si'+' '+
                 IntegerToSpell(Round(Frac(aValue)*100))+' '+
                 'bani';
  end;
begin
  Result := CurrencyToSpell(Value);
end;


constructor TAtlasFunctions.Create;
begin
  inherited Create(aScript);

  with aScript do
  begin
    AddMethod('function Adunare(a, b: Integer): Integer', CallMethod, 'Functii utilizator', 'Functie de adunare');
    AddMethod('function AdresaUnitate: String', CallMethod, 'Functii utilizator', 'Adresa Unitate');
    AddMethod('function CodFiscalUnitate: String', CallMethod, 'Functii utilizator', 'CodFiscal Unitate');
    AddMethod('function DenumireUnitate: String', CallMethod, 'Functii utilizator', 'Denumire Unitate');
    AddMethod('function EmailUnitate: String', CallMethod, 'Functii utilizator', 'Email Unitate');
    AddMethod('function JudetUnitate: String', CallMethod, 'Functii utilizator', 'Judet Unitate');
    AddMethod('function LocalitateUnitate: String', CallMethod, 'Functii utilizator', 'Localitate Unitate');
    AddMethod('function TelefonUnitate: String', CallMethod, 'Functii utilizator', 'Telefon Unitate');

    AddMethod('function NumeCompletOperator: String', CallMethod, 'Functii utilizator', 'NumeCompletOperator');
    AddMethod('function NumeOperator: String', CallMethod, 'Functii utilizator', 'NumeOperator');

    AddMethod('function MoneySpell(Value:Currency):String',CallMethod, 'Functii utilizator', 'Transforma in litere parametrul');

  end;
end;

function TAtlasFunctions.CallMethod(Instance: TObject; ClassType: TClass; const MethodName: String; var Params: Variant): Variant;
begin
  if MethodName = 'ADUNARE' then
     Result := Adunare(Params[0], Params[1])
  else
  if MethodName = 'ADRESAUNITATE' then
     Result := AdresaUnitate
  else
  if MethodName = 'CODFISCALUNITATE' then
     Result := CodFiscalUnitate
  else
  if MethodName = 'DENUMIREUNITATE' then
     Result := DenumireUnitate
  else
  if MethodName = 'EMAILUNITATE' then
     Result := EmailUnitate
  else
  if MethodName = 'JUDETUNITATE' then
     Result := JudetUnitate
  else
  if MethodName = 'LOCALITATEUNITATE' then
     Result := LocalitateUnitate
  else
  if MethodName = 'TELEFONUNITATE' then
     Result := TelefonUnitate
  else
  if MethodName = 'NUMECOMPLETOPERATOR' then
     Result := NumeCompletOperator
  else
  if MethodName = 'NUMEOPERATOR' then
     Result := NumeOperator
  else
  if MethodName = 'MONEYSPELL' then
     Result := MoneySpell(Params[0]);
end;

initialization
  fsRTTIModules.Add(TAtlasFunctions);
finalization
  if fsRTTIModules <> nil then
    fsRTTIModules.Remove(TAtlasFunctions);


end.

