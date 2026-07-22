unit importExtraseUnit;

interface

uses
  SysUtils,
  Contnrs,
  Classes,
  Windows;

type
  TOnProgressExtras = procedure(Sender: TObject; const Position: Integer) of Object;
  
  TExtras = class;
  TTipLinieExtras =
    (
      etUnknown         ,// = 0,
      etSoldPrecedent   ,// = 1,
      etMiscareZilnica  ,// = 2,
      etRulajZi         ,// = 3,
      etTotalSume       ,// = 4,
      etSoldFinal       ,// = 5,
      etTotalDesc       ,// = 6,
      etDisponibil       // = 7
    );

  TLinieExtras = class
  private
    FLine       : String;
    FExtras     : TExtras;
    FDataDocStr : String;
    FCuiClient  : String;
    FCuiBeneficiar: String;
    FBicBancaDest: String;
    FSumaDebit: Double;
    FCuiPlatitor: String;
    FContClient: String;
    FExtrasType: TTipLinieExtras;
    FSumaCreditStr: String;
    FAn: Integer;
    FDataPlata: TDateTime;
    FContCoresp: String;
    FDataZi: TDateTime;
    FBicBancaExp: String;
    FExplicatii: String;
    FNumeBeneficiar: String;
    FIbanClient: String;
    FSumaDebitStr: String;
    FNrDoc: String;
    FNumePlatitor: String;
    FIbanBeneficiar: String;
    FIbanPlatitor: String;
    FDataDoc: TDateTime;
    FAnStr: String;
    FDataPlataStr: String;
    FDataZiStr: String;
    FNumeContClient: String;
    FSumaCredit: Double;
    FTipLine: Integer;
    FTipLinieStr: String;
    procedure SetAnStr(const Value: String);
    procedure SetDataDocStr(const Value: String);
    procedure SetDataPlataStr(const Value: String);
    procedure SetDataZiStr(const Value: String);
    procedure SetSumaCrediStr(const Value: String);
    procedure SetSumaDebitStr(const Value: String);
    procedure SetTipLinieStr(const Value: String);
  public
    constructor Create(AExtras: TExtras; const ALine: String);
  public
    property Extras         : TExtras           read FExtras;
    property ContClient     : String            read FContClient;
    property NumeContClient : String            read FNumeContClient;
    property ContCoresp     : String            read FContCoresp;
    property NrDoc          : String            read FNrDoc;
    property DataDocStr     : String            read FDataDocStr      write SetDataDocStr;
    property DataDoc        : TDateTime         read FDataDoc;
    property DataPlataStr   : String            read FDataPlataStr    write SetDataPlataStr;
    property DataPlata      : TDateTime         read FDataPlata;
    property CuiClient      : String            read FCuiClient;
    property CuiPlatitor    : String            read FCuiPlatitor;
    property CuiBeneficiar  : String            read FCuiBeneficiar;
    property SumaDebitStr   : String            read FSumaDebitStr    write SetSumaDebitStr;
    property SumaDebit      : Double            read FSumaDebit;
    property SumaCreditStr  : String            read FSumaCreditStr   write SetSumaCrediStr;
    property SumaCredit     : Double            read FSumaCredit;
    property IbanClient     : String            read FIbanClient;
    property IbanPlatitor   : String            read FIbanPlatitor;
    property IbanBeneficiar : String            read FIbanBeneficiar;
    property NumePlatitor   : String            read FNumePlatitor;
    property NumeBeneficiar : String            read FNumeBeneficiar;
    property BicBancaDest   : String            read FBicBancaDest;
    property BicBancaExp    : String            read FBicBancaExp;
    property Explicatii     : String            read FExplicatii;
    property AnStr          : String            read FAnStr           write SetAnStr;
    property An             : Integer           read FAn;
    property ExtrasType     : TTipLinieExtras   read FExtrasType;
    property DataZiStr      : String            read FDataZiStr       write SetDataZiStr;
    property TipLinieStr    : String            read FTipLinieStr     write SetTipLinieStr;
    property TipLinie       : Integer           read FTipLine;
    property DataZi         : TDateTime         read FDataZi;
  end;

  TContExtras = class
  private
    FExtras       : TExtras;
    FMiscari      : TObjectList;
    FTotalDesc    : TLinieExtras;
    FSoldFinal    : TLinieExtras;
    FRulajZi      : TLinieExtras;
    FDisponibil   : TLinieExtras;
    FTotalSume    : TLinieExtras;
    FSoldPrecedent: TLinieExtras;
    function GetMiscari(Index: Integer): TLinieExtras;
    function GetMiscariCount: Integer;
    procedure SetDisponibil(const Value: TLinieExtras);
    procedure SetRulajZi(const Value: TLinieExtras);
    procedure SetSoldFinal(const Value: TLinieExtras);
    procedure SetSoldPrecedent(const Value: TLinieExtras);
    procedure SetTotalDesc(const Value: TLinieExtras);
    procedure SetTotalSume(const Value: TLinieExtras);
  protected
    procedure AddRulaj(ALinie: TLinieExtras);
  public
    constructor Create(AExtras: TExtras);
    destructor Destroy; override;
  public
    property SoldPrecedent: TLinieExtras read FSoldPrecedent write SetSoldPrecedent;
    property MiscariCount : Integer read GetMiscariCount;
    property Miscari[Index: Integer]: TLinieExtras read GetMiscari;
    property RulajZi      : TLinieExtras read FRulajZi       write SetRulajZi;
    property TotalSume    : TLinieExtras read FTotalSume     write SetTotalSume;
    property SoldFinal    : TLinieExtras read FSoldFinal     write SetSoldFinal;
    property TotalDesc    : TLinieExtras read FTotalDesc     write SetTotalDesc;
    property Disponibil   : TLinieExtras read FDisponibil    write SetDisponibil;
  end;

  TExtras = class(TList)
  private
    FFileName   : String;
    FFormatData : TFormatSettings;
    FConturi    : TObjectList;
    FRulaje     : TObjectList;
    FLinii      : TObjectList;
    FEmptyLine  : TLinieExtras;
    FActive     : Boolean;
    FErrorDate: TDateTime;
    FOnProgress: TOnProgressExtras;
    FOnStartProgress: TOnProgressExtras;
    procedure SetActive(const Value: Boolean);
    function GetContExtras(Index: Integer): TContExtras;
    function GetConturiCount: Integer;
    function GetRulaj(Index: Integer): TLinieExtras;
    function GetRulajCount: Integer;
  protected
    function ProcessLine(const ALine: String): TLinieExtras;
    function AdaugaCont(ALinieExtras: TLinieExtras): TContExtras;
    function IsCorrectLine(const aLine: String): Boolean; virtual;
  public
    constructor Create(const AFileName: String);
    destructor Destroy; override;
  public
    procedure Open;
    procedure Close;
  public
    property Active: Boolean                read FActive write SetActive;
    property ContExtras[Index: Integer]: TContExtras read GetContExtras;
    property ContCount : Integer            read GetConturiCount;
    property Rulaj[Index: Integer]: TLinieExtras read GetRulaj;
    property RulajCount: Integer            read GetRulajCount;
    property FormatData: TFormatSettings    read FFormatData write FFormatData;
    property ErrorDate : TDateTime          read FErrorDate  write FErrorDate;
    property OnProgress: TOnProgressExtras  read FOnProgress write FOnProgress;
    property OnStartProgress: TOnProgressExtras read FOnStartProgress write FOnStartProgress;
  end;

const
  szTipStr : array[TTipLinieExtras] of String =
    (
      'Necunoscut',
      'Sold Precedent',
      'Miscar Zilnica',
      'Rulaj Zi',
      'Total Sume',
      'Sold Final',
      'Total Desc',
      'Disponibil'
    );


implementation

uses
  DateUtils,
  StrUtils;

{ TLinieExtras }

constructor TLinieExtras.Create(AExtras: TExtras; const ALine: String);
var
  I,
  lLen: Integer;

    function ExtractString: String;
    var
      lEndChar: Char;
    begin
      Result := '';
      if I <= lLen then begin
        if FLine[I] = '"' then begin
          lEndChar := '"';
          Inc(I);
        end
        else lEndChar := ',';
        while (I <= lLen) and (FLine[I] <> lEndChar) do begin
          Result := Result + FLine[I];
          Inc(I);
        end;
        if (I <= lLen) and (lEndChar = '"') then
          Inc(I);
        if (I <= lLen) and (FLine[I] = ',') then
          Inc(I);
        Result := Trim(Result);
      end;
    end;

begin
  inherited Create;
  FExtras := AExtras;
  FLine   := ALine;
  I       := 1;
  lLen    := Length(FLine);
  FContClient      := ExtractString;
  FNumeContClient  := ExtractString;
  FContCoresp      := ExtractString;
  FNrDoc           := ExtractString;
  DataDocStr       := ExtractString;
  DataPlataStr     := ExtractString;
  FCuiClient       := ExtractString;
  FCuiPlatitor     := ExtractString;
  FCuiBeneficiar   := ExtractString;
  SumaDebitStr     := ExtractString;
  SumaCreditStr    := ExtractString;
  FIbanClient      := ExtractString;
  FIbanPlatitor    := ExtractString;
  FIbanBeneficiar  := ExtractString;
  FNumePlatitor    := ExtractString;
  FNumeBeneficiar  := ExtractString;
  FBicBancaDest    := ExtractString;
  FBicBancaExp     := ExtractString;
  FExplicatii      := ExtractString;
  AnStr            := ExtractString;
  TipLinieStr      := ExtractString;
  DataZiStr        := ExtractString;
end;

procedure TLinieExtras.SetAnStr(const Value: String);
begin
  FAnStr := Value;
  FAn    := StrToIntDef(Value, 0);
end;

procedure TLinieExtras.SetDataDocStr(const Value: String);
begin
  FDataDocStr := Value;
  if not TryStrToDate(FDataDocStr, FDataDoc, FExtras.FormatData) then
    FDataDoc := FExtras.ErrorDate;
end;

procedure TLinieExtras.SetDataPlataStr(const Value: String);
begin
  FDataPlataStr := Value;
  if not TryStrToDate(FDataPlataStr, FDataPlata, FExtras.FormatData) then
    FDataPlata := FExtras.ErrorDate;
end;

procedure TLinieExtras.SetDataZiStr(const Value: String);
begin
  FDataZiStr := Value;
  if not TryStrToDate(FDataZiStr, FDataZi, FExtras.FormatData) then
    FDataZi := FExtras.ErrorDate;
end;

procedure TLinieExtras.SetSumaCrediStr(const Value: String);
begin
  FSumaCreditStr := Value;
  if not TryStrToFloat(FSumaCreditStr, FSumaCredit, FExtras.FormatData) then
    FSumaCredit := 0;
end;

procedure TLinieExtras.SetSumaDebitStr(const Value: String);
begin
  FSumaDebitStr := Value;
  if not TryStrToFloat(FSumaDebitStr, FSumaDebit, FExtras.FormatData) then
    FSumaDebit := 0;
end;

procedure TLinieExtras.SetTipLinieStr(const Value: String);
begin
  FTipLinieStr  := Value;
  FTipLine      := StrToIntDef(Value, 0);
  if (FTipLine = 0) or (not (FTipLine in [Ord(Low(TTipLinieExtras))..Ord(High(TTipLinieExtras))])) then
    FExtrasType := etUnknown
  else
    FExtrasType := TTipLinieExtras(Trunc(FTipLine));
end;

{ TExtras }

function TExtras.AdaugaCont(ALinieExtras: TLinieExtras): TContExtras;
begin
  Result := TContExtras.Create(Self);
  Result.SoldPrecedent := ALinieExtras;
  FConturi.Add(Result);
end;

procedure TExtras.Close;
begin
  FLinii.Clear;
end;

constructor TExtras.Create(const AFileName: String);
begin
  inherited Create;
  FFileName   := AFileName;
  FFormatData.ShortDateFormat := 'dd/mm/yyyy';
  FFormatData.DateSeparator   := '.';
  FFormatData.ThousandSeparator := #0;
  FFormatData.DecimalSeparator  := '.';
  FErrorDate  := EncodeDate(2000, 01, 01);
  FLinii      := TObjectList.Create(True);
  FConturi    := TObjectList.Create(True);
  FRulaje     := TObjectList.Create(False);
  FEmptyLine  := ProcessLine('');
end;

destructor TExtras.Destroy;
begin
  FLinii.Free;
  FRulaje.Free;
  FConturi.Free;
  inherited Destroy;
end;

function TExtras.GetContExtras(Index: Integer): TContExtras;
begin
  Result := TContExtras(FConturi[Index]);
end;

function TExtras.GetConturiCount: Integer;
begin
  Result := FConturi.Count;
end;

function TExtras.GetRulaj(Index: Integer): TLinieExtras;
begin
  Result := TLinieExtras(FRulaje[Index]);
end;

function TExtras.GetRulajCount: Integer;
begin
  Result := FRulaje.Count;
end;

function TExtras.IsCorrectLine(const aLine: String): Boolean;
var
  lLastStr  : String;
  lDateTime : TDateTime;
begin
  lLastStr := RightStr(Trim(aLine), 12);
  Result := (Length(lLastStr) = 12) and (lLastStr[1] = '"') and (lLastStr[12] = '"') 
            and TryStrToDate(Copy(lLastStr, 2, 10), lDateTime, FFormatData);
//            and YearOf(lDateTime)
end;

procedure TExtras.Open;
var
  lFile        : System.Text;

    function GetLineFromFile: String;
    var
      lReadLine: String;
    begin
      Result := '';
      while not Eof(lFile) do begin
        ReadLn(lFile, lReadLine);
        if Trim(lReadLine) > '' then Result := Result + Trim(lReadLine);
        if IsCorrectLine(Result) then
          break;
      end;
    end;

var
  lLine         : String;
  lLinieExtras  : TLinieExtras;
  lCurentCont   : TContExtras;
  
begin
  lCurentCont := nil;
  FileMode    := 2;
  AssignFile(lFile, FFileName);
  Reset(lFile);
  if Assigned(FOnStartProgress) then
    FOnStartProgress(Self, FileSize(lFile));
  while not Eof(lFile) do begin
    { Testam daca este linie completa :
      daca se termina in "xx.xx.xxxx .....
      unde xx.xx.xxxx reprezinta o data valida atunci linia este completa }
    lLine := GetLineFromFile;
    if Assigned(FOnProgress) then
      FOnProgress(Self, System.FilePos(lFile));
    if IsCorrectLine(lLine) then begin
      lLinieExtras := ProcessLine(lLine);
      if lLinieExtras.ExtrasType = etSoldPrecedent then
        lCurentCont := AdaugaCont(lLinieExtras)
      else begin
        if not Assigned(lCurentCont) then
          raise Exception.Create('Tip inregistrare incorect, lipseste soldul precedent !');
        case lLinieExtras.ExtrasType of
          etMiscareZilnica:
            begin
              lCurentCont.AddRulaj(lLinieExtras);
              FRulaje.Add(lLinieExtras);
            end;
          etRulajZi:
            lCurentCont.RulajZi    := lLinieExtras;
          etTotalSume:
            lCurentCont.TotalSume  := lLinieExtras;
          etSoldFinal:
            begin
              lCurentCont.SoldFinal  := lLinieExtras;
              lCurentCont := nil;
            end;
          etTotalDesc:
            lCurentCont.TotalDesc  := lLinieExtras;
          etDisponibil:
            lCurentCont.Disponibil := lLinieExtras;
          else
            raise Exception.CreateFmt('Tip de inregistrare necunoscut %d', [Ord(lLinieExtras.ExtrasType)]);
        end;
      end;
    end;
  end;
end;

function TExtras.ProcessLine(const ALine: String): TLinieExtras;
begin
  Result := TLinieExtras.Create(Self, ALine);
  FLinii.Add(Result);
end;

procedure TExtras.SetActive(const Value: Boolean);
begin
  FActive := Value;
end;

{ TContExtras }

procedure TContExtras.AddRulaj(ALinie: TLinieExtras);
begin
  FMiscari.Add(ALinie);
end;

constructor TContExtras.Create(AExtras: TExtras);
begin
  inherited Create;
  FMiscari       := TObjectList.Create(False);
  FExtras        := AExtras;
  FTotalDesc     := AExtras.FEmptyLine;
  FSoldFinal     := AExtras.FEmptyLine;
  FRulajZi       := AExtras.FEmptyLine;
  FDisponibil    := AExtras.FEmptyLine;
  FTotalSume     := AExtras.FEmptyLine;
  FSoldPrecedent := AExtras.FEmptyLine;
end;

destructor TContExtras.Destroy;
begin
  FMiscari.Free;
  inherited Destroy;
end;

function TContExtras.GetMiscari(Index: Integer): TLinieExtras;
begin
  Result := TLinieExtras(FMiscari[Index]);
end;

function TContExtras.GetMiscariCount: Integer;
begin
  Result := FMiscari.Count;
end;

procedure TContExtras.SetDisponibil(const Value: TLinieExtras);
begin
  if FDisponibil <> FExtras.FEmptyLine then
    raise Exception.Create('Eroare parsare fisier se dubleaza disponibilul pe un cont');
  FDisponibil := Value;
end;

procedure TContExtras.SetRulajZi(const Value: TLinieExtras);
begin
  if FDisponibil <> FExtras.FEmptyLine then
    raise Exception.Create('Eroare parsare fisier se dubleaza rulajul pe un cont');
  FRulajZi := Value;
end;

procedure TContExtras.SetSoldFinal(const Value: TLinieExtras);
begin
  if FDisponibil <> FExtras.FEmptyLine then
    raise Exception.Create('Eroare parsare fisier se dubleaza soldul final pe un cont');
  FSoldFinal := Value;
end;

procedure TContExtras.SetSoldPrecedent(const Value: TLinieExtras);
begin
  if FDisponibil <> FExtras.FEmptyLine then
    raise Exception.Create('Eroare parsare fisier se dubleaza soldul precedent pe un cont');
  FSoldPrecedent := Value;
end;

procedure TContExtras.SetTotalDesc(const Value: TLinieExtras);
begin
  if FDisponibil <> FExtras.FEmptyLine then
    raise Exception.Create('Eroare parsare fisier se dubleaza total desc pe un cont');
  FTotalDesc := Value;
end;

procedure TContExtras.SetTotalSume(const Value: TLinieExtras);
begin
  if FDisponibil <> FExtras.FEmptyLine then
    raise Exception.Create('Eroare parsare fisier se dubleaza total sume pe un cont');
  FTotalSume := Value;
end;

end.
