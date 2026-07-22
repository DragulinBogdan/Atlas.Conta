unit AnexeParametriiLista;

interface
uses Classes, ZDataSet, DB, Controls, ZConnection;

type
  TAnexeParameterType = (ptDefault, ptTextEdit, ptImageComboBox, ptComboBox, ptPopupEdit, ptCurrencyEdit,
                    ptSpinEdit, ptDateEdit, ptTimeEdit, ptCheckBox, ptLookupComboBox, ptAnLuna);

  TAnexeParamList = class;

  TAnexeParamItem = class
  private
    FName         : String;
    FValue        : Variant;
    FAlias        : String;
    FDescription  : String;
    FParameterType    : TAnexeParameterType;
    FParentField  : String;
    FKeyField     : String;
    FIDField      : String;
    FDisplayField : String;
    FSourceTable  : String;
    FFieldList    : String;
    FValueList    : String;
    FDescList     : String;
    FColumnAutoWidth: Boolean;
    FControlWidth: Integer;
    FID           : Integer;
    FQuery        : TDataSet;
    FDataSource   : TDataSource;
    FPopupControl : TControl;
    FDataType: TFieldType;
  public
    property Name         : String     read FName         write FName;
    property Value        : Variant    read FValue        write FValue;
    property Alias        : String     read FAlias        write FAlias;
    property Description  : String     read FDescription  write FDescription;
    property ParameterType    : TAnexeParameterType read FParameterType    write FParameterType;
    property ParentField  : String     read FParentField  write FParentField;
    property KeyField     : String     read FKeyField     write FKeyField;
    property IDField      : String     read FIDField      write FIDField;
    property DisplayField : String     read FDisplayField write FDisplayField;
    property SourceTable  : String     read FSourceTable  write FSourceTable;
    property FieldList    : String     read FFieldList    write FFieldList;
    property ValueList    : String     read FValueList    write FValueList;
    property DescList     : String     read FDescList     write FDescList;
    property ID           : Integer    read FID           write FID;
    property Query        : TDataSet   read FQuery        write FQuery;
    property DataSource   : TDataSource read FDataSource  write FDataSource;
    property PopupControl : TControl   read FPopupControl write FPopupControl;
    property ColumnAutoWidth: Boolean  read FColumnAutoWidth write FColumnAutoWidth;
    property ControlWidth : Integer    read FControlWidth write FControlWidth;
    property DataType : TFieldType read FDataType write FDataType;
  end;


 TAnexeParamList = class
  private
    FParams : TList;
    FIdAnexa: Integer;
    function GetParam(Index: Integer): TAnexeParamItem;
    function GetParamCount: Integer;
  public
    constructor Create; virtual;
    destructor  Destroy; override;
    function    AddParam: TAnexeParamItem;
    function    FindParam(const ParamName: String): TAnexeParamItem;
    function    HasParam(const ParamName: String): Boolean;
    function    ParamByName(const ParamName: String): TAnexeParamItem;
    procedure   ReadAnexaParams(ADBCon: TZConnection);
    procedure   SetAnexaParams(ADBCon: TZConnection);
    procedure   ClearParams;
  public
    property ParamCount: Integer read GetParamCount;
    property Params[Index: Integer]: TAnexeParamItem read GetParam;
    property IdAnexa : Integer read FIdAnexa write FIdAnexa;
  end;

  function DBNewQuery(ADBCon: TZConnection; ASQLText: String; AOwner: TComponent=nil): TZQuery;
implementation

uses SysUtils;

{TAnexeParamList}
//------------------------------------------------------------------------------
constructor TAnexeParamList.Create;
begin
  inherited;
  FParams := TList.Create;
  FIdAnexa := 0;
end;
//------------------------------------------------------------------------------
destructor TAnexeParamList.Destroy;
begin
  ClearParams;
  FParams.Free;
  inherited;
end;
//------------------------------------------------------------------------------
procedure TAnexeParamList.ClearParams;
var lParam: TAnexeParamItem;
begin
  while FParams.Count > 0 do
  begin
    lParam := TAnexeParamItem(FParams[0]);
    lParam.Free;
    FParams.Delete(0);
  end;
end;
//------------------------------------------------------------------------------
function TAnexeParamList.AddParam: TAnexeParamItem;
begin
  Result := TAnexeParamItem.Create;
  Result.Query := nil;
  Result.DataSource := nil;
  Result.PopupControl := nil;

  FParams.Add(Result);
end;
//------------------------------------------------------------------------------
function TAnexeParamList.FindParam(const ParamName: String): TAnexeParamItem;
var
  I: Integer;
begin
  Result := nil;
  for I := 0 to ParamCount - 1 do
    if SameText(Params[I].Name, ParamName) then
    begin
      Result := Params[I];
      Break;
    end;
end;
//------------------------------------------------------------------------------
function TAnexeParamList.HasParam(const ParamName: String): Boolean;
begin
  Result := FindParam(ParamName) <> nil;
end;
//------------------------------------------------------------------------------
function TAnexeParamList.ParamByName(const ParamName: String): TAnexeParamItem;
begin
  Result := FindParam(ParamName);
  if not Assigned(Result) then
    raise Exception.Create('Parametrul '+ParamName+' nu poate fi gasit !');
end;
//------------------------------------------------------------------------------
function TAnexeParamList.GetParam(Index: Integer): TAnexeParamItem;
begin
  Result := TAnexeParamItem(FParams[Index]);
end;
//------------------------------------------------------------------------------
function TAnexeParamList.GetParamCount: Integer;
begin
  Result := FParams.Count;
end;
//------------------------------------------------------------------------------
function DBNewQuery(ADBCon: TZConnection; ASQLText: String; AOwner: TComponent=nil): TZQuery;
begin
  if Assigned(AOwner) then
    Result := TZQuery.Create(AOwner)
  else
    Result := TZQuery.Create(ADBCon.Owner);
  Result.Connection := ADBCon;
  Result.SQL.Text := ASQLText;
  Result.Open;
end;
//------------------------------------------------------------------------------
procedure TAnexeParamList.ReadAnexaParams(ADBCon: TZConnection);
var
  lParam: TAnexeParamItem;
begin
   ClearParams;
    with DBNewQuery(ADBCon, Format('exec spAnexeDetaliiParametru %s', [IntToStr(FIdAnexa)])) do
    try
      Open;
      if not IsEmpty then
      begin
        lParam := Self.FindParam(FieldByName('ParamName').AsString);
        if not Assigned(lParam) then
        begin
          lParam := self.AddParam;
          lParam.Name := FieldByName('ParamName').AsString;
        end;

        lParam.ParameterType    := TAnexeParameterType(FieldByName('ParamType').AsInteger);
        lParam.KeyField         := FieldByName('KeyField').AsString;
        lParam.DisplayField     := FieldByName('DisplayField').AsString;
        lParam.Alias            := FieldByName('Caption').AsString;
        lParam.Description      := FieldByName('Description').AsString;
        lParam.ValueList        := FieldByName('ValueList').AsString;
        lParam.FieldList        := FieldByName('FieldList').AsString;
        lParam.DescList         := FieldByName('DescriptionList').AsString;
        lParam.SourceTable      := FieldByName('SourceTable').AsString;
        lParam.IDField          := FieldByName('IDField').AsString;
        lParam.ParentField      := FieldByName('ParentField').AsString;
        lParam.ID               := FieldByName('IDParametru').AsInteger;
        lParam.ColumnAutoWidth  := FieldByName('ColumnAutoWidth').AsBoolean;
        lParam.ControlWidth     := FieldByName('ControlWidth').AsInteger;
      end;
    finally
      Free;
    end;
end;

procedure TAnexeParamList.SetAnexaParams(ADBCon: TZConnection);
var
  i: Integer;
  aQry : TZQuery;
begin
  aQry := TZQuery.Create(nil);
  with aQry do
    try
      SQL.Add('exec spAnexaSetParam :idAnexeBilant, :ParamName, :Value, :DataType');
      for I := 0 to ParamCount-1 do begin
         Params.ParamByName('idAnexeBilant').Value := IdAnexa;
         Params.ParamByName('ParamName').Value := Params[I].Name;
         Params.ParamByName('DataType').Value := Params[I].DataType;
         Params.ParamByName('Value').Value := Params[I].Value;
         ExecSQL;
      end;
    finally
      Free;
    end;
end;


end.
