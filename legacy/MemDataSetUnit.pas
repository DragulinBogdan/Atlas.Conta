unit MemDataSetUnit;

interface

uses Classes, DB, dxmdaset;

type
//  TdxMemData = class(TdxMemData);

  TOnNeedDataSet = procedure (const SQLOrder: String; var DataSet: TDataSet) of Object;

  TMemDataSet = class(TdxMemData)
  private
    FNewFilterRecord : TFilterRecordEvent;
    FOnNeedDataSet   : TOnNeedDataSet;
    FSQLOrder: TStrings;
    procedure SetSQLOrder(const Value: TStrings);
  protected

    procedure SetOnFilterRecord(const Value: TFilterRecordEvent); override;
    procedure DefaultFilter(DataSet: TDataSet; var Accept: Boolean);
    procedure InternalOpen; override;
    procedure InternalFilter(var Accept: Boolean); virtual;
    procedure CopyFieldStructure(DataSet: TDataSet);
    function  GetDataSet(const SQLOrder: String): TDataSet;
  public
    constructor Create(AOwner: TComponent); override;
  published
    property SQLOrder: TStrings read FSQLOrder write SetSQLOrder;
    property OnNeedDataSet: TOnNeedDataSet read FOnNeedDataSet write FOnNeedDataSet;
  end;

const
  SSQLHandlerNotSet: String = 'Evenimentul pentru executie SQL nu este setat !';

implementation

uses
  SysUtils;

{ TMemDataSet }

function CorrectFieldName(AFieldName: String): String;
var
  I: Integer;
begin
  Result := AFieldName;
  for I := 1 to Length(Result) do
    if not ((Result[I] in ['A'..'z']) or (Result[I] in ['0'..'9'])) then
      Result[I] := '_';
end;

procedure TMemDataSet.CopyFieldStructure(DataSet: TDataSet);
var
  AField : TField;
  i : Integer;
begin
  if (DataSet = nil) or (DataSet.FieldCount = 0) then exit;
  while FieldCount > 1 do
    Fields[FieldCount - 1].Free;

  if DataSet.FieldCount > 0 then
  begin
    for i := 0 to DataSet.FieldCount - 1 do
      if SupportedFieldType(DataSet.Fields[i].DataType)
      and (CompareText(DataSet.Fields[i].FieldName, 'RECID') <> 0) then
      begin
        AField := DefaultFieldClasses[DataSet.Fields[i].DataType].Create(self);
        with  DataSet.Fields[i] do
        begin
          AField.Name := self.Name + CorrectFieldName(FieldName);
          AField.DisplayLabel := DataSet.Fields[i].DisplayLabel;
          AField.DisplayWidth := DataSet.Fields[i].DisplayWidth;
          AField.EditMask := DataSet.Fields[i].EditMask;
          AField.FieldName := FieldName;
          if AField is TStringField then
            TStringField(AField).Size := Size;
          if AField is TBlobField then
            TBlobField(AField).Size := Size;
          if AField is TFloatField then
          begin
            TFloatField(AField).Currency := TFloatField(DataSet.Fields[i]).Currency;
            TFloatField(AField).Precision := TFloatField(DataSet.Fields[i]).Precision;
          end;
          AField.DataSet := self;
          AField.Calculated := Calculated;
          AField.Lookup := Lookup;
          if Lookup then
          begin
            AField.KeyFields := KeyFields;
            AField.LookupDataSet := LookupDataSet;
            AField.LookupKeyFields := LookupKeyFields;
            AField.LookupResultField := LookupResultField;
          end;
        end;
      end;
  end else
  begin
    DataSet.FieldDefs.Update;
    for i := 0 to DataSet.FieldDefs.Count - 1 do
      if SupportedFieldType(DataSet.FieldDefs[i].DataType) then
      begin
        AField := DefaultFieldClasses[DataSet.Fields[i].DataType].Create(self);
        with  DataSet.FieldDefs[i] do
        begin
          AField.Name := self.Name + Name;
          AField.FieldName := Name;
          if AField is TStringField then
                TStringField(AField).Size := Size;
          if AField is TBlobField then
                TBlobField(AField).Size := Size;
          AField.DataSet := self;
        end;
      end;
  end;
end;

constructor TMemDataSet.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  inherited SetOnFilterRecord(DefaultFilter);                            
end;

procedure TMemDataSet.DefaultFilter(DataSet: TDataSet;
  var Accept: Boolean);
begin
  InternalFilter(Accept);
  if Accept then
    if Assigned(FNewFilterRecord) then
       FNewFilterRecord(DataSet, Accept);
end;

function TMemDataSet.GetDataSet(const SQLOrder: String): TDataSet;
begin
  if Assigned(FOnNeedDataSet) then
     FOnNeedDataSet(SQLOrder, Result)
  else
     raise Exception.Create(SSQLHandlerNotSet);
end;

procedure TMemDataSet.InternalFilter(var Accept: Boolean);
begin
  Accept := True;
end;

procedure TMemDataSet.InternalOpen;
var
  lDataSet : TDataSet;
begin
  lDataSet := GetDataSet(FSQLOrder.Text);
  try
    CopyFieldStructure(lDataSet);
    inherited InternalOpen;
    { ATENTIE -> Se refera strict la cazul in care clasa curenta descinde din TdxMemData
                 si exista campul RecID indiferent data are campuri sau nu dataset-ul }
    if FieldCount > 1 then
       LoadFromDataSet(lDataSet);
  finally
    lDataSet.Free;
  end;
end;

procedure TMemDataSet.SetOnFilterRecord(const Value: TFilterRecordEvent);
begin
  if ( not Assigned(Value) and Assigned(FNewFilterRecord) ) or
     ( Assigned(Value) and ( not Assigned(FNewFilterRecord)) ) then begin
     FNewFilterRecord := Value;
     UpdateFilters;
  end;
end;

procedure TMemDataSet.SetSQLOrder(const Value: TStrings);
begin
  FSQLOrder.Assign(Value);
end;

end.
