{ $HDR$}
{**********************************************************************}
{ File archived using GP-Version                                       }
{ GP-Version is Copyright 1999 by Quality Software Components Ltd      }
{                                                                      }
{ For further information / comments, visit our WEB site at            }
{ http://www.qsc.co.uk                                                 }
{**********************************************************************}
{}
{ $Log:  D:\Salarii\Taxe si impozite\ComponenteSRC\RBuilder\Source\daDBBDE.pas.z
{
{   Rev 1.0    06-02-2001 20:49:30  bobo
{ Proiect de Taxe si Impozite Locale
}
{}
{******************************************************************************}
{                                                                              }
{           ReportBuilder Data Access Developement Environment (DADE)          }
{                                                                              }
{             Copyright (c) 1996, 2000 Digital Metaphors Corporation           }
{                                                                              }
{******************************************************************************}

unit daADO;

interface

{$I ppIfDef.pas}

uses Classes, SysUtils, Forms, ExtCtrls, DB, ADODB,
     ppComm, ppClass, ppDBPipe, ppDB, ppDBBDE, ppClasUt, ppUtils, ppTypes,
     {$IFDEF Delphi7} Variants, {$ENDIF}
     {$IFDEF Delphi11} WideStrings, {$ENDIF}
     daDB, daQueryDataView, daDataView, daPreviewDataDlg, daSQL;

type

  {BDE Dependent DataView Classes:

     1.  BDE TDataSet descendants
           - TDataSets that can be children of a DataView.
           - Override the HasParent method of TComponent to return True
           - Must be registerd with the Delphi IDE using the RegisterNoIcon procedure

       a. TdaChildBDEQuery - TQuery descendant that can be a child of a DataView
       b. TdaChildBDETable - TTable descendant that can be a child of a DataView
       c. TdaChildBDEStoredProc - TStoredProc descendant that can be a child of a DataView


     2.  TdaBDESession
           - descendant of TppSession
           - implements GetDatabaseNames, GetTableNames, etc.

     3.  TdaBDEDataSet
          - descendant of TppDataSet
          - implements GetFieldNames for SQL

     4.  TdaBDEQueryDataView
          - descendant of TppQueryDataView
          - uses the above classes to create the required
            Query -> DataSource -> Pipeline -> Report connection
          - uses the SQL built by the QueryWizard to assign
            SQL to the TQuery etc.

      }

  { TdaChildBDEQuery }
  TdaChildBDEQuery = class(TADOQuery)
    public
      constructor Create(aOwner: TComponent); override;
      function HasParent: Boolean; override;
  end;  {class, TdaChildBDEQuery}

  { TdaChildBDETable }
  TdaChildBDETable = class(TADOTable)
    public
      function HasParent: Boolean; override;
  end;  {class, TdaChildBDETable}

  { TdaChildBDEStoredProc }
  TdaChildBDEStoredProc = class(TADOStoredProc)
    public
      function HasParent: Boolean; override;
  end;  {class, TdaChildBDEStoredProc}


  { TdaBDESession }
  TdaBDESession = class(TdaSession)
    private
      function GetAliasDriverName(const aAlias: String): String;
      function IsInterBase(const aDriverName: String): Boolean;
      function IsMSAccess(const aDriverName: String): Boolean;
      function IsMSSQLServer(const aDriverName: String): Boolean;
      function IsOracle(const aDriverName: String): Boolean;
      function IsParadox(const aDriverName: String): Boolean;
      function IsSybaseASA(const aDriverName: String): Boolean;
      function IsSybaseASE(const aDriverName: String): Boolean;

      procedure AddDatabase(aDatabase: TComponent);

    protected
      procedure SetDataOwner(aDataOwner: TComponent); override;

    public
      class function ClassDescription: String; override;
      class function DataSetClass: TdaDataSetClass; override;
      class function DatabaseClass: TComponentClass; override;

      procedure GetDatabaseNames(aList: TStrings); override;
      function  GetDatabaseType(const aDatabaseName: String): TppDatabaseType; override;
      procedure GetTableNames(const aDatabaseName: String; aList: TStrings); override;
      function  ValidDatabaseTypes: TppDatabaseTypes; override;

  end; {class, TdaBDESession}


  { TdaBDEDataSet }
  TdaBDEDataSet = class(TdaDataSet)
    private
      FQuery: TADOQuery;

      function GetQuery: TADOQuery;

    protected
      procedure BuildFieldList; override;
      function  GetActive: Boolean; override;
      procedure SetActive(Value: Boolean); override;
      procedure SetDatabase(aDatabase: TComponent); virtual;
      procedure SetDataName(const aDataName: String); override;

      property Query: TADOQuery read GetQuery;

    public
      constructor Create(aOwner: TComponent); override;
      destructor Destroy; override;

      class function ClassDescription: String; override;

      procedure GetFieldNamesForSQL(aList: TStrings; aSQL: TStrings); override;
      procedure GetFieldsForSQL(aList: TList; aSQL: TStrings); override;
      function GetMaxFieldAliasLength: Integer; override;

  end; {class, TdaBDEDataSet}


  { TdaBDEQueryDataView }
  TdaBDEQueryDataView = class(TdaQueryDataView)
    private
      FDataSource: TppChildDataSource;
      FQuery: TdaChildBDEQuery;

    protected
      property Query: TdaChildBDEQuery read FQuery;

    public
      constructor Create(aOwner: TComponent); override;
      destructor Destroy; override;

      class function SessionClass: TClass; override;

      procedure Init; override;
      procedure Loaded; override;
      procedure ConnectPipelinesToData; override;

      procedure SQLChanged; override;

    published
      property DataSource: TppChildDataSource read FDataSource;

  end; {class, TdaBDEQueryDataView}


  TdaADOQueryDataView = class(TdaBDEQueryDataView);

  TdaChildADOQuery    = class(TdaChildBDEQuery);

  { TdaBDETemplateDataView }
  TdaBDETemplateDataView = class(TdaQueryDataView)
    private
      FDatabaseName: String;

    protected
      procedure SetDatabaseName(aDatabaseName: String); virtual;

    public
      {defines BDESession as the session class}
      class function SessionClass: TClass; override;
      class function DataDesignerClass: TClass; override;

      {returns True}
      class function IsTemplate: Boolean; override;

      {descendants should call these to create data access objects}
      function CreateQuery: TADOQuery;
      function CreateTable(aTableName: String): TADOTable;
      function CreateStoredProc(aStoredProcName: String): TADOStoredProc;
      function CreateDataSource: TDataSource;
      function CreateDataPipeline: TppBDEPipeline;
      function CreatePipelineField(aTableName, aFieldName, aFieldAlias: String;
                             aDataPipeline: TppDataPipeline; aSearchable, aSortable: Boolean): TppField;
      function IsLinkable: Boolean; override;

      property DatabaseName: String read FDatabaseName write SetDatabaseName;

    published

      property Report;

  end; {class, TdaBDETemplateDataView}


  { TdaBDEQueryTemplateDataView }
  TdaBDEQueryTemplateDataView = class(TdaBDETemplateDataView)
    private
      FQuery: TADOQuery;
      FDataSource: TDataSource;
      FDataPipeline: TppBDEPipeline;

      function DuplicateFieldNames: Boolean;
      procedure UpdateFieldNamesFromSQL;
      
    protected
      procedure SetDatabaseName(aDatabaseName: String); override;

    public
      constructor Create(aOwner: TComponent); override;
      destructor Destroy; override;

      procedure CreateDataPipelines; override;
      procedure CreatePipelineFields(aDataPipeline: TppDataPipeline); override;

      procedure ConnectPipelinesToData; override;
      procedure DefineDataSelection; override;

      procedure Init; override;

      function  AddSelectTable(aTableName: String): TdaTable;
      function  AddSelectField(aTable: TdaTable; aFieldName, aFieldAlias: String; aSearchable, aSortable: Boolean): TdaField;
      procedure DefineSelectedFields; virtual;
      procedure DefineCalculatedFields; virtual;
      procedure DefineSelectionCriteria; virtual;
      procedure DefineSortOrder; virtual;

      procedure BuildSQL;
      procedure SQLChanged; override;

    published
      property DataPipeline: TppBDEPipeline read FDataPipeline write FDataPipeline;
      property Query: TADOQuery read FQuery;
      property DataSource: TDataSource read FDataSource;

  end; {class, TdaBDEQueryTemplateDataView}


  {global functions to access default BDE connection}
  function daGetDefaultBDEDatabase: TADOConnection;

  {utility routines}
  procedure daGetBDEDatabaseNames(aList: TStrings);
  function daGetBDEDatabaseForName(aDatabaseName: String): TADOConnection;
  function daGetBDESessionForDatabase(aDatabaseName: String): TObject;

  function daGetBDEDatabaseList: TppComponentList;

  procedure SetNullParams(aQry: TADOQuery);

  procedure RegisterConnection(aConn: TADOConnection);

const
  cDefaultConnection = 'DefaultADODatabase';
  cstTimeOut         : Integer = 1800;

var
  daGetDatabaseForName : function (aDatabaseName: String): TADOConnection = daGetBDEDatabaseForName;
  aDataBaseName      : String = cDefaultConnection;

implementation

var
  FBDEDatabase: TADOConnection;
  FBDEDatabaseList: TppComponentList;
  FLocalConnection: TADOConnection;

procedure RegisterConnection(aConn: TADOConnection);
begin
  FLocalConnection := aConn;
  if aConn <> nil then
     cstTimeOut := aConn.CommandTimeout;
end;

{******************************************************************************
 *
 ** C H I L D   B D E  D A T A   A C C E S S   C O M P O N E N T S
 *
{******************************************************************************}

{ Setam Parametrii pe null ... sa nu avem nici un fel de probleme }
procedure SetNullParams(aQry: TADOQuery);
var I: Integer;
begin
  for I := 0 to aQry.Parameters.Count-1 do begin
    if aQry.Parameters[I].DataType = ftUnknown then begin
      aQry.Parameters[I].DataType := ftString;
      aQry.Parameters[I].Direction := pdInput;
      aQry.Parameters[I].Size := 255;
      aQry.Parameters[I].Precision := 255;
    end;
    aQry.Parameters[I].Value    := Null;
  end;
end;


{------------------------------------------------------------------------------}
{ TdaChildBDEQuery.HasParent }

constructor TdaChildBDEQuery.Create(aOwner: TComponent);
begin
  inherited Create(aOwner);
  CommandTimeout := cstTimeOut;
  LockType       := ltReadOnly;

end;

function TdaChildBDEQuery.HasParent: Boolean;
begin
  Result := True;
end; {function, HasParent}

{------------------------------------------------------------------------------}
{ TdaChildBDETable.HasParent }

function TdaChildBDETable.HasParent: Boolean;
begin
  Result := True;
end; {function, HasParent}


{------------------------------------------------------------------------------}
{ TdaChildBDEStoredProc.HasParent }

function TdaChildBDEStoredProc.HasParent: Boolean;
begin
  Result := True;
end; {function, HasParent}




{******************************************************************************
 *
 ** B D E   S E S S I O N
 *
{******************************************************************************}

{------------------------------------------------------------------------------}
{ TdaBDESession.ClassDescription }

class function TdaBDESession.ClassDescription: String;
begin
  Result := 'BDESession';
end; {class function, ClassDescription}

{------------------------------------------------------------------------------}
{ TdaBDESession.DataSetClass }

class function TdaBDESession.DataSetClass: TdaDataSetClass;
begin
  Result := TdaBDEDataSet;
end; {class function, DataSetClass}

{------------------------------------------------------------------------------}
{ TdaBDESession.DatabaseClass }

class function TdaBDESession.DatabaseClass: TComponentClass;
begin
  Result := TADOConnection;
end; {class function, DatabaseClass}

{------------------------------------------------------------------------------}
{ TdaBDESession.GetTableNames }

procedure TdaBDESession.GetTableNames(const aDatabaseName: String; aList: TStrings);
var
  lDatabase: TADOConnection;

begin

  lDatabase := daGetBDEDatabaseForName(aDatabaseName);
  aList.Clear;
  
  with TADOQuery.Create(nil) do
    try
       Connection := lDataBase;
       LockType   := ltReadOnly;
       Sql.Add('SELECT NAME FROM SYSOBJECTS WHERE XTYPE IN (''U'', ''V'') AND OBJECTPROPERTY(ID, ''IsMsShipped'')=0');
       Open;
       while not Eof do begin
         aList.Add(Fields[0].AsString);
         Next;
       end;
    finally
       Free;
    end;

end; {procedure, GetTableNames}


{------------------------------------------------------------------------------}
{ TdaBDESession.AddDatabase }

procedure TdaBDESession.AddDatabase(aDatabase: TComponent);
begin

  if daGetBDEDatabaseList.IndexOf(aDatabase) < 0 then
    FBDEDatabaseList.Add(aDatabase);

end; {procedure, AddDatabase}


{------------------------------------------------------------------------------}
{ TdaBDESession.GetDatabaseNames }

procedure TdaBDESession.GetDatabaseNames(aList: TStrings);
var
  liIndex: Integer;
  lDBList: TStrings;
  lDatabase: TADOConnection;

begin
  {call utility routine to get list of database names}
  daGetBDEDatabaseNames(aList);

  lDBList := TStringList.Create;

  daGetDatabaseObjectsFromOwner(TdaSessionClass(Self.ClassType), lDBList, DataOwner);

  for liIndex := 0 to lDBList.Count-1 do
    begin
      lDatabase := TADOConnection(lDBList.Objects[liIndex]);

      if aList.IndexOf(lDatabase.Name) < 0 then
        aList.AddObject(lDatabase.Name, lDatabase);

      AddDatabase(lDatabase);

    end;

  lDBList.Free;

end; {procedure, GetDatabaseNames}

{------------------------------------------------------------------------------}
{ TdaBDESession.SetDataOwner }

procedure TdaBDESession.SetDataOwner(aDataOwner: TComponent);
var
  lList: TStringList;
begin

  inherited SetDataOwner(aDataOwner);

  lList := TStringList.Create;

  GetDatabaseNames(lList);

  lList.Free;

end; {procedure, SetDataOwner}


{------------------------------------------------------------------------------}
{ TdaBDESession.ValidDatabaseTypes }

function TdaBDESession.ValidDatabaseTypes: TppDatabaseTypes;
begin
  Result := [dtParadox, dtInterBase, dtMSAccess, dtMSSQLServer, dtSybaseASA, dtSybaseASE, dtOracle, dtOther];
end; {procedure, ValidDatabaseTypes}

{------------------------------------------------------------------------------}
{ TdaBDESession.GetDatabaseType }

function TdaBDESession.GetDatabaseType(const aDatabaseName: String): TppDatabaseType;
var
  lsDriverName: String;
begin

  lsDriverName := GetAliasDriverName(aDatabaseName);

  if IsParadox(lsDriverName) then
    Result := dtParadox

  else if IsInterBase(lsDriverName) then
    Result := dtInterBase

  else if IsMSAccess(lsDriverName) then
    Result := dtMSAccess

  else if IsMSSQLServer(lsDriverName) then
    Result := dtMSSQLServer

  else if IsSybaseASA(lsDriverName) then
    Result := dtSybaseASA

  else if IsSybaseASE(lsDriverName) then
    Result := dtSybaseASE

  else if IsOracle(lsDriverName) then
    Result := dtOracle

  else
    Result := dtOther;

end; {function, GetDatabaseType}

{------------------------------------------------------------------------------}
{ TdaBDESession.IsMSSQLServer }

function TdaBDESession.IsMSSQLServer(const aDriverName: String): Boolean;
var
  lsDriverName: String;
begin

  lsDriverName := Uppercase(aDriverName);

  Result := (Pos('SQL', lsDriverName) > 0);

  if (Result) then
    Result := (Pos('SERVER', lsDriverName) > 0);

end; {procedure, IsMSSQLServer}

{------------------------------------------------------------------------------}
{ TdaBDESession.IsSybaseASE }

function TdaBDESession.IsSybaseASE(const aDriverName: String): Boolean;
var
  lsDriverName: String;
begin

  lsDriverName := Uppercase(aDriverName);

  Result := (Pos('SYBASE', lsDriverName) > 0);

  if (Result) then
    Result := not(IsSybaseASA(aDriverName));

end; {procedure, IsSybaseASE}

{------------------------------------------------------------------------------}
{ TdaBDESession.IsSybaseASA}

function TdaBDESession.IsSybaseASA(const aDriverName: String): Boolean;
var
  lsDriverName: String;
begin

  lsDriverName := Uppercase(aDriverName);

  Result := (Pos('SQL', lsDriverName) > 0) or (Pos('ADAPTIVE', lsDriverName) > 0);

  if (Result) then
    Result := (Pos('ANYWHERE', lsDriverName) > 0);

end; {procedure, IsSybaseASA}

{------------------------------------------------------------------------------}
{ TdaBDESession.IsInterBase }

function TdaBDESession.IsInterBase(const aDriverName: String): Boolean;
var
  lsDriverName: String;
begin

  lsDriverName := Uppercase(aDriverName);

  Result := (Pos('INTRBASE', lsDriverName) > 0);

  if not(Result) then
    Result := (Pos('INTERBASE', lsDriverName) > 0);

end; {procedure, IsInterBase}

{------------------------------------------------------------------------------}
{ TdaBDESession.IsMSAccess }

function TdaBDESession.IsMSAccess(const aDriverName: String): Boolean;
var
  lsDriverName: String;
begin

  lsDriverName := Uppercase(aDriverName);

  Result := (Pos('ACCESS', lsDriverName) > 0);

end; {procedure, IsMSAccess}

{------------------------------------------------------------------------------}
{ TdaBDESession.IsParadox }

function TdaBDESession.IsParadox(const aDriverName: String): Boolean;
var
  lsDriverName: String;
begin

  lsDriverName := Uppercase(aDriverName);

  Result := (Pos('STANDARD', lsDriverName) > 0);

end; {procedure, IsParadox}

{------------------------------------------------------------------------------}
{ TdaBDESession.IsOracle }

function TdaBDESession.IsOracle(const aDriverName: String): Boolean;
var
  lsDriverName: String;
begin

  lsDriverName := Uppercase(aDriverName);

  Result := (Pos('ORACLE', lsDriverName) > 0);

end; {procedure, IsOracle}

{------------------------------------------------------------------------------}
{ TdaBDESession.GetAliasDriverName }

function TdaBDESession.GetAliasDriverName(const aAlias: String): String;
  
begin

  Result := 'ADOSession';

end; {function, GetAliasDriverName}


{******************************************************************************
 *
 ** B D E   D A T A S E T
 *
{******************************************************************************}

{------------------------------------------------------------------------------}
{ TdaBDEDataSet.Create }

constructor TdaBDEDataSet.Create(aOwner: TComponent);
begin

  inherited Create(aOwner);

  FQuery := nil;

end; {constructor, Create}

{------------------------------------------------------------------------------}
{ TdaBDEDataSet.Destroy }

destructor TdaBDEDataSet.Destroy;
begin

  FQuery.Free;

  inherited Destroy;

end; {destructor, Destroy}

{------------------------------------------------------------------------------}
{ TdaBDEDataSet.GetMaxFieldAliasLength }

function TdaBDEDataSet.GetMaxFieldAliasLength: Integer;
var
  lsExtension: String;
begin

  lsExtension := Copy(DataName, Length(DataName) - 2, 3);

  if (CompareText(lsExtension, 'DBF') = 0) then
    Result := 10
  else
    Result := 25

end; {function, GetMaxFieldAliasLength}

{------------------------------------------------------------------------------}
{ TdaBDEDataSet.ClassDescription }

class function TdaBDEDataSet.ClassDescription: String;
begin
  Result := 'BDEDataSet';
end; {class function, ClassDescription}

{------------------------------------------------------------------------------}
{ TdaBDEDataSet.GetActive }

function TdaBDEDataSet.GetActive: Boolean;
begin
  Result := GetQuery.Active
end; {function, GetActive}

{------------------------------------------------------------------------------}
{ TdaBDEDataSet.SetActive }

procedure TdaBDEDataSet.SetActive(Value: Boolean);
begin
  GetQuery.Active := Value;
end; {procedure, SetActive}

{------------------------------------------------------------------------------}
{ TdaBDEDataSet.GetQuery }

function TdaBDEDataSet.GetQuery: TADOQuery;
begin

  if (FQuery = nil) then begin
    FQuery := TADOQuery.Create(Self);
    FQuery.CommandTimeout := cstTimeOut;
    FQuery.LockType := ltReadOnly;
    if FQuery.Connection = nil then
      FQuery.Connection := FLocalConnection;
  end;

  Result := FQuery;

end; {procedure, GetQuery}

{------------------------------------------------------------------------------}
{ TdaBDEDataSet.SetDatabaseName }

procedure TdaBDEDataSet.SetDataBase(aDatabase: TComponent);
begin

  inherited SetDatabase(aDatabase);

  if GetQuery.Active then
    FQuery.Active := False;

  FQuery.Connection := daGetBDEDatabaseForName(aDatabase.ClassName);

end; {procedure, SetDatabaseName}

{------------------------------------------------------------------------------}
{ TdaBDEDataSet.SetDataName }

procedure TdaBDEDataSet.SetDataName(const aDataName: String);
begin

  inherited SetDataName(aDataName);

  {dataset cannot be active to set data name}
  if GetQuery.Active then
    FQuery.Active := False;

  {construct an SQL statment that returns an empty result set,
   this is used to get the field information }
  FQuery.SQL.Text := 'SELECT * FROM ' + aDataName +
                     ' WHERE ''c'' <> ''c'' ';

end; {procedure, SetDataName}

{------------------------------------------------------------------------------}
{ TdaBDEDataSet.BuildFieldList }

procedure TdaBDEDataSet.BuildFieldList;
var
  liIndex: Integer;
  lBDEField: TField;
  lField: TppField;
begin

  inherited BuildFieldList;

  if not(GetQuery.Active) then
    FQuery.Active := True;

  for liIndex := 0 to FQuery.FieldCount - 1 do
    begin
      lBDEField := FQuery.Fields[liIndex];

      lField := TppField.Create(nil);

      lField.TableName := DataName;
      lField.FieldName := lBDEField.FieldName;
      lField.FieldAlias := lBDEField.DisplayLabel;
      lField.FieldLength := lBDEField.Size;
      lField.DataType := ppConvertFieldType(lBDEField.DataType);
      lField.DisplayWidth := lBDEField.DisplayWidth;

      AddField(lField);
    end;

end; {function, BuildFieldList}

{------------------------------------------------------------------------------}
{ TdaBDEDataSet.GetFieldNamesForSQL }

procedure TdaBDEDataSet.GetFieldNamesForSQL(aList: TStrings; aSQL: TStrings);
var
  lQuery: TADOQuery;
begin
  aList.Clear;

  lQuery := TADOQuery.Create(Self);
  lQuery.CommandTimeout := cstTimeOut;
  lQuery.LockType       := ltReadOnly;

  if Database = nil then
    lQuery.Connection := daGetBDEDatabaseForName('')
  else
    lQuery.Connection := daGetBDEDatabaseForName(Database.ClassName);
  lQuery.SQL.Assign(aSQL);// := {$IFDEF Delphi11}TWideStrings({$ENDIF} aSQL {$IFDEF Delphi11}){$ENDIF};

  SetNullParams(lQuery);

  lQuery.GetFieldNames(aList);

  lQuery.Free;

end; {procedure, GetFieldNamesForSQL}

{------------------------------------------------------------------------------}
{ TdaBDEDataSet.GetFieldsForSQL }

procedure TdaBDEDataSet.GetFieldsForSQL(aList: TList; aSQL: TStrings);
var
  lQuery: TADOQuery;
  lBDEField: TField;
  lField: TppField;
  liIndex: Integer;
begin
  aList.Clear;

  lQuery := TADOQuery.Create(Self);

  try

    if Database = nil then
      lQuery.Connection := daGetBDEDatabaseForName('')
    else
      lQuery.Connection := daGetBDEDatabaseForName(Database.ClassName);
    lQuery.CommandTimeout := cstTimeOut;
    lQuery.LockType       := ltReadOnly;
    lQuery.SQL.Assign(aSQL);// := {$IFDEF Delphi11}TWideStrings({$ENDIF} aSQL {$IFDEF Delphi11}){$ENDIF};

    SetNullParams(lQuery);
    
    lQuery.Active := True;

    for liIndex := 0 to lQuery.FieldCount - 1 do
      begin
        lBDEField := lQuery.Fields[liIndex];

        lField := TppField.Create(nil);

        lField.FieldName := lBDEField.FieldName;
        lField.FieldAlias := lBDEField.DisplayLabel;
        lField.FieldLength := lBDEField.Size;
        lField.DataType := ppConvertFieldType(lBDEField.DataType);
        lField.DisplayWidth := lBDEField.DisplayWidth;

        aList.Add(lField);
      end;

  finally
    lQuery.Free;
    
  end;

end; {procedure, GetFieldsForSQL}


{******************************************************************************
 *
 ** B D E  Q U E R Y   D A T A V I E W
 *
{******************************************************************************}

{------------------------------------------------------------------------------}
{ TdaBDEQueryDataView.Create }

constructor TdaBDEQueryDataView.Create(aOwner: TComponent);
begin

  inherited Create(aOwner);

  {notes: 1. must use ChildQuery, ChildDataSource, ChildPipeline etc.
          2. use Self as owner for Query, DataSource etc.
          3. do NOT assign a Name }

  FQuery := TdaChildBDEQuery.Create(Owner);
  { Punem si numele ca sa poata fi salvat si incarcat cu TReader, TWriter }
  { Daca punem numele aici e ca draq ca nu mai merge modificarea raportului
    dobitocul nu distruge raportul il mai incarca o data pur si simplu }
  //FQuery.Name := GetUniqueUserName(Self); //GetValidName(Self);


  FDataSource := TppChildDataSource.Create(Self);

  FDataSource.DataSet := FQuery;

end; {constructor, Create}

{------------------------------------------------------------------------------}
{ TdaBDEQueryDataView.Destroy }

destructor TdaBDEQueryDataView.Destroy;
begin
  {FDataSource.Free;
   FQuery.Free;}

  inherited Destroy;

end; {destructor, Destroy}

{------------------------------------------------------------------------------}
{ TdaBDEQueryDataView.SessionClass }

class function TdaBDEQueryDataView.SessionClass: TClass;
begin
  Result := TdaBDESession;
end; {class function, SessionClass}

{------------------------------------------------------------------------------}
{ TdaBDEQueryDataView.ConnectPipelinesToData }

procedure TdaBDEQueryDataView.ConnectPipelinesToData;
begin

  if DataPipelineCount = 0 then Exit;

  {need to reconnect here}
  TppDBPipeline(DataPipelines[0]).DataSource := FDataSource;

end; {procedure, ConnectPipelinesToData}

{------------------------------------------------------------------------------}
{ TdaBDEQueryDataView.Init }

procedure TdaBDEQueryDataView.Init;
var
  lDataPipeline: TppChildBDEPipeline;

begin

  inherited Init;

  if DataPipelineCount > 0 then Exit;

  {note: DataView's owner must own the DataPipeline }
  lDataPipeline := TppChildBDEPipeline(ppComponentCreate(Self, TppChildBDEPipeline));
  lDataPipeline.DataSource := FDataSource;

  lDataPipeline.AutoCreateFields := False;

  {add DataPipeline to the dataview }
  lDataPipeline.DataView := Self;

end; {procedure, Init}

{------------------------------------------------------------------------------}
{ TdaBDEQueryDataView.SQLChanged }

procedure TdaBDEQueryDataView.SQLChanged;
begin

  if FQuery.Active then
    FQuery.Close;

  FQuery.Connection := daGetBDEDatabaseForName(SQL.DatabaseName);

  FQuery.SQL.Assign(SQL.MagicSQLText);  // := {$IFDEF Delphi11}TWideStrings({$ENDIF} SQL.MagicSQLText {$IFDEF Delphi11}){$ENDIF};

end; {procedure, SQLChanged}


{******************************************************************************
 *
 ** B D E  T E M P L A T E  D A T A V I E W
 *
{******************************************************************************}

{------------------------------------------------------------------------------}
{ TdaBDETemplateDataView.DataDesignerClass }

class function TdaBDETemplateDataView.DataDesignerClass: TClass;
begin
  Result := nil;
end; {class function, DataDesignerClass}

{------------------------------------------------------------------------------}
{ TdaBDETemplateDataView.IsTemplate }

class function TdaBDETemplateDataView.IsTemplate: Boolean;
begin
  Result := True;
end; {function, IsTemplate}

{------------------------------------------------------------------------------}
{ TdaBDETemplateDataView.IsLinkable }

function TdaBDETemplateDataView.IsLinkable: Boolean;
begin
  {BDETemplateDataView dataview is not linkable}
  Result := False;
end; {function, IsLinkable}

{------------------------------------------------------------------------------}
{ TdaBDETemplateDataView.SessionClass }

class function TdaBDETemplateDataView.SessionClass: TClass;
begin
  Result := TdaBDESession;
end; {class function, SessionClass}

{------------------------------------------------------------------------------}
{ TdaBDETemplateDataView.SetDatabaseName }

procedure TdaBDETemplateDataView.SetDatabaseName(aDatabaseName: String);
begin
  FDatabaseName := aDatabaseName;

end; {procedure, SetDatabaseName}


{------------------------------------------------------------------------------}
{ TdaBDETemplateDataView.CreateQuery }

function TdaBDETemplateDataView.CreateQuery: TADOQuery;
begin
  Result := TdaChildBDEQuery.Create(Self);
  Result.Connection := daGetBDEDatabaseForName(DatabaseName);
  Result.CommandTimeout := cstTimeOut;
  Result.LockType   := ltReadOnly;

end; {procedure, CreateQuery}

{------------------------------------------------------------------------------}
{ TdaBDETemplateDataView.CreateTable }

function TdaBDETemplateDataView.CreateTable(aTableName: String): TADOTable;
begin
  Result := TdaChildBDETable.Create(Self);
  Result.Connection := daGetBDEDatabaseForName(DatabaseName);

  Result.TableName := aTableName;


end; {procedure, CreateTable}

{------------------------------------------------------------------------------}
{ TdaBDETemplateDataView.CreateStoredProc }

function TdaBDETemplateDataView.CreateStoredProc(aStoredProcName: String): TADOStoredProc;
begin
  Result := TdaChildBDEStoredProc.Create(Self);
  Result.Connection := daGetBDEDatabaseForName(DatabaseName);

  Result.ProcedureName := aStoredProcName;


end; {procedure, CreateStoredProc}

{------------------------------------------------------------------------------}
{ TdaBDETemplateDataView.CreateDataSource }

function TdaBDETemplateDataView.CreateDataSource: TDataSource;
begin
  Result := TppChildDataSource.Create(Self);

end; {procedure, CreateDataSource}

{------------------------------------------------------------------------------}
{ TdaBDETemplateDataView.CreateDataPipeline }

function TdaBDETemplateDataView.CreateDataPipeline: TppBDEPipeline;
begin

  {note: DataView's owner must own the DataPipeline }
  Result := TppChildBDEPipeline(ppComponentCreate(Self, TppChildBDEPipeline));

  {add pipeline to the dataview's Pipelines[] array}
  Result.DataView := Self;

end; {procedure, CreateDataPipeline}



{------------------------------------------------------------------------------}
{ TdaBDETemplateDataView.CreatePipelineField }

function TdaBDETemplateDataView.CreatePipelineField(aTableName, aFieldName, aFieldAlias: String;
                             aDataPipeline: TppDataPipeline; aSearchable, aSortable: Boolean): TppField;
begin

  Result := TppField.Create(nil);

  {set field props}
  Result.TableName     := aTableName;
  Result.FieldName     := aFieldName;

  if (aFieldAlias <> '') then
    Result.FieldAlias    := aFieldAlias
  else
    Result.FieldAlias    := aFieldName;

  Result.DataType      := aDataPipeline.GetFieldDataType(aFieldName);
  Result.FieldLength   := aDataPipeline.GetFieldSize(aFieldName);
  Result.DisplayWidth  := aDataPipeline.GetFieldDisplayWidth(aFieldName);


  Result.Searchable    := aSearchable;
  Result.Sortable      := aSortable;

  {add field to the datapipeline's Fields[] array}
  Result.DataPipeline := aDataPipeline;

  {turn auto create fields off}
  if (aDataPipeline <> nil) and (aDataPipeline is TppDBPipeline) and
    TppDBPIpeline(aDataPipeline).AutoCreateFields then
    TppDBPIpeline(aDataPipeline).AutoCreateFields := False;


end; {procedure, CreatePipelineField}




{******************************************************************************
 *
 ** Query Template DataView
 *
{******************************************************************************}

{------------------------------------------------------------------------------}
{ TdaBDEQueryTemplateDataView.Create }

constructor TdaBDEQueryTemplateDataView.Create(aOwner: TComponent);
begin

  inherited Create(aOwner);

  {call CreateQuery method defined in the ancestor }
  FQuery := CreateQuery;

  {call CreateDataSource method defined in the ancestor }
  FDataSource := CreateDataSource;
  FDataSource.DataSet := FQuery;

  FDataPipeline := nil;

end; {constructor, Create}

{------------------------------------------------------------------------------}
{ TdaBDEQueryTemplateDataView.Destroy }

destructor TdaBDEQueryTemplateDataView.Destroy;
begin

  inherited Destroy;

end; {destructor, Destroy}

{------------------------------------------------------------------------------}
{ TdaBDEQueryTemplateDataView.SetDatabaseName }

procedure TdaBDEQueryTemplateDataView.SetDatabaseName(aDatabaseName: String);
begin
  inherited SetDatabaseName(aDatabaseName);

  FQuery.Connection := daGetBDEDatabaseForName(aDatabaseName);

//  ATENTIE
  if (FQuery <> nil) and (FQuery.Connection <> nil) then
     SQL.DatabaseName   := FQuery.Connection.Name
  else Sql.DataBaseName := 'DbRaportare';
  { Work Around pentru Rapoarte modificate cu ADO ca sa ruleze sub BDE }

end; {procedure, SetDatabaseName}

{------------------------------------------------------------------------------}
{ TdaBDEQueryTemplateDataView.Init }


procedure TdaBDEQueryTemplateDataView.Init;
var
  liIndex: Integer;
begin

  if (FDataPipeline <> nil) then Exit;

  DefineSelectedFields;
  DefineDataSelection;

  inherited Init;

  if (DataPipelineCount > 0) then Exit;

  CreateDataPipelines;
  ConnectPipelinesToData;

  for liIndex := 0 to DataPipelineCount-1 do
    CreatePipelineFields(DataPipelines[liIndex]);

end; {procedure, Init}

{------------------------------------------------------------------------------}
{ TdaBDEQueryTemplateDataView.CreateDataPipelines }

procedure TdaBDEQueryTemplateDataView.CreateDataPipelines;
begin

  {note: call CreateDataPipeline defined in ancestor }
  FDataPipeline := CreateDataPipeline;
  FDataPipeline.UserName := ppTextToIdentifier(ClassDescription);

  FDataPipeline.Name := FDataPipeline.UserName;

  {this gets displayed as the toolwindow caption}
  SQL.DataPipelineName := FDataPipeline.UserName;

  UserName := 'Query_' + FDataPipeline.Name;

end; {procedure, CreateDataPipelines}


{------------------------------------------------------------------------------}
{ TdaBDEQueryTemplateDataView.CreatePipelineFields }

procedure TdaBDEQueryTemplateDataView.CreatePipelineFields(aDataPipeline: TppDataPipeline);
var
  liIndex: Integer;
  lField: TdaField;
  lsFieldName: String;
begin

  if (FDataPipeline = aDataPipeline) then
    begin

      if DuplicateFieldNames then
        UpdateFieldNamesFromSQL;

      TppDBPipeline(aDataPipeline).AutoCreateFields := False;

      for liIndex := 0 to SQL.SelectFieldCount-1 do
        begin
          lField := SQL.SelectFields[liIndex];

          if (lField.SQLFieldName <> '') then
            lsFieldName := lField.SQLFieldName
          else
            lsFieldName := lField.FieldName;

          CreatePipelineField(lField.TableAlias, lsFieldName, lField.FieldAlias, aDataPipeline, lField.Searchable, lField.Sortable);
        end;

    end;

end; {procedure, CreatePipelineFields}

{------------------------------------------------------------------------------}
{ TdaBDEQueryTemplateDataView.DuplicateFieldNames }

function TdaBDEQueryTemplateDataView.DuplicateFieldNames: Boolean;
var
  lFieldNames: TStringList;
  liIndex: Integer;
  lsFieldName: String;
begin

  lFieldNames := TStringList.Create;
  lFieldNames.Sorted := True;

  Result := False;
  liIndex := 0;

  while not(Result) and (liIndex < SQL.SelectFieldCount) do
    begin
      lsFieldName := SQL.SelectFields[liIndex].FieldName;

      if (lFieldNames.IndexOf(lsFieldName) = -1) then
        begin
          lFieldNames.Add(lsFieldName);

          Inc(liIndex);
        end
      else
        Result := True;

    end;

  lFieldNames.Free;

end; {procedure, DuplicateFieldNames}

{------------------------------------------------------------------------------}
{ TdaBDEQueryTemplateDataView.UpdateFieldNamesFromSQL }

procedure TdaBDEQueryTemplateDataView.UpdateFieldNamesFromSQL;
begin

  {set these properties so call to Valid will not display a message}
  SQL.DataPipelineName := 'xxx';

  {call valid to set the SQLFieldName property of the selected fields}
  SQL.Valid;

end; {procedure, UpdateFieldNamesFromSQL}

{------------------------------------------------------------------------------}
{ TdaBDEQueryTemplateDataView.ConnectPipelinesToData }

procedure TdaBDEQueryTemplateDataView.ConnectPipelinesToData;
begin

  if (FDatapipeline = nil) then Exit;

  {connect datapipeline to datasource here}
  FDatapipeline.DataSource := FDataSource;

end; {procedure, ConnectPipelinesToData}

{------------------------------------------------------------------------------}
{ TdaBDEQueryTemplateDataView.DefineSelectedFields }

procedure TdaBDEQueryTemplateDataView.DefineSelectedFields;
begin

  SQL.Clear;

 {descendants add code here}

end; {procedure, DefineSelectedFields}

{------------------------------------------------------------------------------}
{ TdaBDEQueryTemplateDataView.DefineCalculatedFields }

procedure TdaBDEQueryTemplateDataView.DefineCalculatedFields;
begin

 {descendants add code here}

end; {procedure, DefineCalculatedFields}


{------------------------------------------------------------------------------}
{ TdaBDEQueryTemplateDataView.AddSelectTable }

function TdaBDEQueryTemplateDataView.AddSelectTable(aTableName: String): TdaTable;
begin
  Result := SQL.AddTable(aTableName);

end; {function, AddSelectTable}

{------------------------------------------------------------------------------}
{ TdaBDEQueryTemplateDataView.AddSelectField }

function TdaBDEQueryTemplateDataView.AddSelectField(aTable: TdaTable; aFieldName, aFieldAlias: String; aSearchable, aSortable: Boolean): TdaField;
begin


  Result := SQL.AddSelectField(aTable, aFieldName);

  if (aFieldAlias <> '')  then
    Result.FieldAlias := aFieldAlias;

  Result.Searchable := aSearchable;
  Result.Sortable   := aSortable;

end; {procedure, AddSelectField}


{------------------------------------------------------------------------------}
{ TdaBDEQueryTemplateDataView.DefineDataSelection }

procedure TdaBDEQueryTemplateDataView.DefineDataSelection;
begin

  SetActive(False);

  BuildSQL;

end; {procedure, DefineDataSelection}

{------------------------------------------------------------------------------}
{ TdaBDEQueryTemplateDataView.BuildSQL }

procedure TdaBDEQueryTemplateDataView.BuildSQL;
begin

  DefineSelectionCriteria;

  DefineSortOrder;

  FQuery.SQL.Assign(SQL.SQLText);  // := {$IFDEF Delphi11}TWideStrings({$ENDIF} SQL.SQLText {$IFDEF Delphi11}){$ENDIF};

end; {procedure, BuildSQL}


{------------------------------------------------------------------------------}
{ TdaBDEQueryTemplateDataView.DefineSelectionCriteria }

procedure TdaBDEQueryTemplateDataView.DefineSelectionCriteria;
var
  liIndex: Integer;
  lField: TppField;
  lTable: TdaTable;

begin

  if (FDataPipeline = nil) then Exit;


  {add code for selection criteria}
  SQL.ClearCriteria;

  for liIndex := 0 to FDataPipeline.FieldCount-1 do
    if FDataPipeline.Fields[liIndex].Search then
      begin
        lField := FDataPipeline.Fields[liIndex];
        lTable := SQL.GetTableForSQLAlias(lField.TableName);

        SQL.AddCriteriaField(lTable, lField.FieldName, dacoLike, lField.SearchExpression);


      end;

end; {procedure, DefineSelectionCriteria}


{------------------------------------------------------------------------------}
{ TdaBDEQueryTemplateDataView.DefineSortOrder }

procedure TdaBDEQueryTemplateDataView.DefineSortOrder;

var
  liIndex: Integer;
  lField: TppField;
  ldaField: TdaField;
  lTable: TdaTable;
  lbAscending: Boolean;
  lFields: TStringList;
begin

  if (FDataPipeline = nil) then Exit;

  {add code for sort order}
  SQL.ClearOrderByFields;


  {get list of sort fields}
  lFields := TStringList.Create;

  for liIndex := 0 to FDataPipeline.FieldCount - 1 do
    begin
      lField := FDataPipeline.Fields[liIndex];

      if lField.Sort then
        lFields.AddObject(Format('%8d', [lField.SortOrder]), lField);
    end;

  lFields.Sort;

  for liIndex := 0 to lFields.Count - 1 do
    begin
      lField := TppField(lFields.Objects[liIndex]);

      lTable := SQL.GetTableForSQLAlias(lField.TableName);

      ldaField := SQL.CreateFieldForTable(lTable, lField.FieldName);

      lbAscending := (lField.SortType = soAscending);

      SQL.AddOrderByField(ldaField, lbAscending);

      ldaField.Free;
     end;

  lFields.Free;

end; {procedure, DefineSortOrder}

{------------------------------------------------------------------------------}
{ TdaBDEQueryTemplateDataView.SQLChanged }

procedure TdaBDEQueryTemplateDataView.SQLChanged;
begin

  if FQuery.Active then
    FQuery.Close;

  FQuery.Connection := daGetBDEDatabaseForName(SQL.DatabaseName);
  FQuery.SQL.Assign(SQL.SQLText); // := {$IFDEF Delphi11}TWideStrings({$ENDIF} SQL.SQLText {$IFDEF Delphi11}){$ENDIF};

end; {procedure, SQLChanged}


{******************************************************************************
 *
 ** P R O C E D U R E S   A N D   F U N C T I O N S
 *
{******************************************************************************}

{------------------------------------------------------------------------------}
{ daGetDefaultBDEDatabase }

function daGetDefaultBDEDatabase: TADOConnection;
begin

  {create the default BDE Database, if needed}
  if (FBDEDatabase = nil) then
    begin
      {create default BDE Database}
      FBDEDatabase      := TADOConnection.Create(nil);
      FBDEDatabase.Name := cDefaultConnection;

    end;

  Result := FBDEDatabase;

end; {function, daGetDefaultBDEDatabase}

{------------------------------------------------------------------------------}
{ daGetBDEDatabaseNames }

procedure daGetBDEDatabaseNames(aList: TStrings);
begin

//  ATENTIE
//  Session.GetAliasNames(aList);

end; {procedure, daGetBDEDatabaseNames}

{------------------------------------------------------------------------------}
{ daGetBDEDatabaseForName }

function daGetBDEDatabaseForName(aDatabaseName: String): TADOConnection;
var
  liIndex: Integer;

  function FindFormConnection(aCmp: TComponent): TADOConnection;
   var I: Integer;
  begin
    Result := nil;
    for I := 0 to aCmp.ComponentCount-1 do begin
      if AnsiCompareText(aCmp.Components[I].Name, aDataBaseName) = 0 then begin
         Result := TADOConnection(aCmp.Components[I]);
         Break;
      end
      else Result := FindFormConnection(aCmp.Components[I]);
    end;
  end;

begin

  liIndex := 0;
  
  {check for a database object with this name}
  while (FLocalConnection = nil) and (liIndex < Screen.FormCount) do begin
    FLocalConnection := FindFormConnection(Screen.Forms[liIndex]);
    Inc(liIndex);
  end;

  Result := FLocalConnection;

end; {function, daGetBDEDatabaseForName}

{------------------------------------------------------------------------------}
{ daGetBDESessionForDatabase }

function daGetBDESessionForDatabase(aDatabaseName: String): TObject;

begin

  Result := nil;

  daGetBDEDatabaseForName(aDatabaseName);

  {note: use Sessions.FindSession rather than reference TDatabase.Session because
         TDatabase.Session is unreliable when csLoading}
// ATENTIE
{  if (lDatabase <> nil) then
    Result := Sessions.FindSession(lDatabase.SessionName);

  if (Result = nil) then
    Result := Session;}

end; {function, daGetBDESessionForDatabase}


{------------------------------------------------------------------------------}
{ daGetBDEDatabaseList }

function daGetBDEDatabaseList: TppComponentList;
begin
  if (FBDEDatabaseList = nil) then
    FBDEDatabaseList := TppComponentList.Create(nil);

  Result := FBDEDatabaseList;

end; {function, daGeTADOConnectionList}


{******************************************************************************
 *
 ** I N I T I A L I Z A T I O N   /   F I N A L I Z A T I O N
 *
{******************************************************************************}

procedure TdaBDEQueryDataView.Loaded;
begin

  inherited Loaded;

  Init;

  SQLChanged;

end;

initialization

  UnRegisterClasses([TdaChildBDEQuery, TdaChildBDETable, TdaChildBDEStoredProc]);

  RegisterClasses([TdaChildBDEQuery, TdaChildBDETable, TdaChildBDEStoredProc]);

  daRegisterSession(TdaBDESession);
  daRegisterDataSet(TdaBDEDataSet);

  daRegisterDataView(TdaBDEQueryDataView);

  {initialize internal reference variables}
  FBDEDatabase := nil;
  FBDEDatabaseList := nil;


finalization

  {free the default connection object}
  FBDEDatabase.Free;
  FBDEDatabaseList.Free;

  UnRegisterClasses([TdaChildBDEQuery, TdaChildBDETable, TdaChildBDEStoredProc]);

  daUnRegisterSession(TdaBDESession);
  daUnRegisterDataSet(TdaBDEDataSet);

  daUnRegisterDataView(TdaBDEQueryDataView);

end.
