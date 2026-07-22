unit ClassFiltru;

interface

uses DB, DBTables, ZDataSet, Classes, Graphics, Controls;

type

TTipConexiune = (tcAND, tcOR);

TGrupFiltre = class;

TFiltruClass = class(TCollectionItem)
  private
    FNumeFiltru: String;
    FFilteredDataSet: TDataSet;
    FIsOneLevel: Boolean;
    FFilter: String;
    FColor: TColor;
    FActiv: Boolean;
    FIdUtilizator: Integer;
    FIdLogin: Integer;
    FConnection: TTipConexiune;
    procedure SetFilter(const Value: String);
    procedure SetActiv(const Value: Boolean);
  public
    constructor Create(Collection: TCollection); override;
    destructor Destroy; override;
    procedure Assign(Source: TPersistent); override;
    procedure ChangeFilter;
    function  LoadFilter(aFilterId : Integer):Boolean;
    function  SaveFilter(aFilterId, aParentId : Integer):Boolean;
    procedure ApplyFiltered;
    property  IsOneLevel : Boolean read FIsOneLevel write FIsOneLevel;
    property  IdUtilizator : Integer read FIdUtilizator;
    property  IdLogin : Integer read FIdLogin;
  published
    property  Filter : String read FFilter write SetFilter;
    property  Color : TColor read FColor write FColor default clBlack;
    property  FilteredDataSet : TDataSet read FFilteredDataSet write FFilteredDataSet;
    property  NumeFiltru : String read FNumeFiltru write FNumeFiltru;
    property  Active : Boolean read FActiv write SetActiv;
    property  Connection : TTipConexiune read FConnection write FConnection default tcAND;
end;

TCriteriiFiltru = class(TCollection)
 private
    FFiltru : TGrupFiltre;
 protected
    procedure Update(Item: TCollectionItem); override;
    procedure UpdateAll;
 public
    constructor Create(Filtru: TGrupFiltre);
    function GetOwner: TPersistent; override;
    function Add: TFiltruClass;
end;


TGrupFiltre = class(TComponent)
  private
    FFiltre : TCriteriiFiltru;
    FDataSet : TDataSet;
    procedure SetFilter(const Value: TCriteriiFiltru);
    function  GetFilter: String;
    procedure SetDataSet(const Value: TDataSet);
  public
    constructor Create(AOwner: TComponent); override;
    destructor Destroy; override;
    property Filter : String read GetFilter;
  published
    property Filtre : TCriteriiFiltru read FFiltre write SetFilter;
    property DataSet : TDataSet read FDataSet write SetDataSet;
end;


TDrawFilter = class
  private
  public
  published
end;



procedure Register;

implementation


{ TFiltruClass }

procedure TFiltruClass.ApplyFiltered;
begin
   if FFilteredDataSet.Filtered then  FFilteredDataSet.Filtered := False;
   FFilteredDataSet.Filter := FFilter;
   FFilteredDataSet.Filtered := True;
end;


procedure TFiltruClass.Assign(Source: TPersistent);
begin
  if (Source is TFiltruClass) then begin
     Filter := TFiltruClass(Source).Filter;
     Color := TFiltruClass(Source).Color;
     FilteredDataSet := TFiltruClass(Source).FilteredDataSet;
     NumeFiltru := TFiltruClass(Source).NumeFiltru;
     Active := TFiltruClass(Source).Active;
  end
  else inherited Assign(Source);
end;

procedure TFiltruClass.ChangeFilter;
begin
end;

constructor TFiltruClass.Create(Collection: TCollection);
begin
  inherited Create(Collection);
  TCriteriiFiltru(Collection).UpdateAll;
end;

destructor TFiltruClass.Destroy;
begin
  inherited Destroy;
end;

function TFiltruClass.LoadFilter(aFilterId: Integer):Boolean;
var aQry : TZQuery;
begin
  aQry := TZQuery.Create(nil);
  with aQry do
    try
      SQL.Add('SELECT * FROM FILTRE WHERE ID_FILTRE = :ID');
      Params.ParamByName('ID').Value := aFilterId;
      Params.ParamByName('ID_UTILIZATOR').Value := IdUtilizator;
      Open;
      Result := not(IsEmpty);
      if Result then
        FFilter := FieldByName('FILTER_STRING').AsString;
        FNumeFiltru := FieldByName('DENUMIRE').AsString;
      Close;
    finally
      Free;
    end;
end;

function TFiltruClass.SaveFilter(aFilterId, aParentId : Integer) : Boolean;
var aQry :TZQuery;
begin
  Result := False;
  aQry := TZQuery.Create(nil);
  with aQry do
    try
      LockType := ltBatchOptimistic;
      SQL.Add('SELECT * FROM FILTRE WHERE ID_FILTRE = :ID_FILTRE');
      Params.ParamByName('ID_FILTRE').Value := aFilterId;
      Open;
      if not(IsEmpty) then Edit
      else Append;
      FieldByName('DENUMIRE').AsString       := FNumeFiltru;
      FieldByName('ID_UTILIZATOR').AsInteger := FIdUtilizator;
      FieldByName('LOGIN_MOD').AsInteger     := FIdLogin;
      FieldByName('FILTER_STRING').AsString  := FFilter;
      //FieldByName('ID_PARENT').DataType := ftInteger;
      if aParentId > 0 then
         FieldByName('ID_PARENT').AsInteger := aParentId
      else
         FieldByName('ID_PARENT').Clear;
      Post;
      if UpdateStatus in [usModified, usInserted] then UpdateBatch;
    finally
      Free;
    end;
end;

procedure TFiltruClass.SetActiv(const Value: Boolean);
begin
  FActiv := Value;
//  if Value := False then FFilter := '';
end;

procedure TFiltruClass.SetFilter(const Value: String);
begin
   FFilter := Value;
end;

{ TCriterieFiltru }

function TCriteriiFiltru.Add: TFiltruClass;
begin
  Result := TFiltruClass(inherited Add);
  Result.FFilteredDataSet := FFiltru.DataSet;
end;

constructor TCriteriiFiltru.Create(Filtru: TGrupFiltre);
begin
  inherited Create(TFiltruClass);
  FFiltru := Filtru;
end;


function TCriteriiFiltru.GetOwner: TPersistent;
begin
   Result := FFiltru;
end;


procedure TCriteriiFiltru.Update(Item: TCollectionItem);
begin
  if Item <> nil then
    TFiltruClass(Item).FilteredDataSet := FFiltru.DataSet;
end;

{ TGrupFiltre }

constructor TGrupFiltre.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  FFiltre := TCriteriiFiltru.Create(Self);
end;

destructor TGrupFiltre.Destroy;
begin
  FFiltre.Free;
  inherited Destroy;
end;

function TGrupFiltre.GetFilter: String;
var I : Integer;

  function ConexString(aTip : TTipConexiune) : String;
  begin
     if aTip = tcAND then Result := ' AND '
     else Result := ' OR ';
  end;

begin
  Result := '';
  for I := 0  to FFiltre.Count - 1 do
    if TFiltruClass(FFiltre.Items[I]).Active then
      if Result  = '' then
        Result := '(' + TFiltruClass(FFiltre.Items[I]).Filter + ')'
      else
        Result := Result + ConexString(TFiltruClass(FFiltre.Items[I]).Connection)+ '(' + TFiltruClass(FFiltre.Items[I]).Filter + ')';
end;

procedure TGrupFiltre.SetDataSet(const Value: TDataSet);
var I : Integer;
begin
  FDataSet := Value;
  for I := 0 to Filtre.Count - 1 do
    TFiltruClass(Filtre.items[I]).FilteredDataSet := Value;
end;

procedure TGrupFiltre.SetFilter(const Value: TCriteriiFiltru);
begin
  FFiltre := Value;
end;

procedure TCriteriiFiltru.UpdateAll;
var I : Integer;
begin
   for I := 0 to Count - 1 do
     TFiltruClass(Items[I]).FilteredDataSet := FFiltru.DataSet;
end;


procedure Register;
begin
  RegisterComponents('Dialogs', [TGrupFiltre]);
end;



end.

