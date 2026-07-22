unit MInvestCommon;


interface
uses ZDataset, SysUtils, Classes, Forms, DB, cxPropertiesStore, ZConnection;

  function BeginOp(aDenumire: String): Integer;
  procedure EndOp(aidOperatiuni: Integer);
  function SessionID : Integer;
  function ArchiveRecord(aTableName, aKeyName: String; aKeyValue, aidOperatiuni: Integer): Boolean;
  procedure OpenQryWithParam(aForm: TForm; aParamName: String; aParamValue: OleVariant);
  procedure RefreshDataSet(aDataSet: TDataSet; aKeyName: String= '');
  procedure SaveDataSet(aDataSet: TDataSet);
  procedure InsertInArbore(tip_frunza : string; id_frunza : integer; RefTipFrunza : integer; Denumire : string);
  function GetAppTempFolder: String;
  function ArhiveazaInregistrare(aTableName, aKeyName: String; aKeyValue, aidOperatiuni: Integer): Boolean;
  function IsNull(AValue: Variant; ANewValue: Variant): Variant;

  procedure SavePreferences(aPropStore: TcxPropertiesStore);
  procedure LoadPreferences(aPropStore: TcxPropertiesStore);

  function GetMIinvestConnection : TZConnection;
  procedure ReplaceEmptyConnection ( aForm : TForm);
  function GetTmpMInvestQry : TZQuery;

var
  FSessionID : Integer = 0;

 dbConnectionMInvest : TZConnection = nil;
 arrObiecte : array [1..20] of integer;
 idSelectat: Integer;
 codInv: string;
 seteazaInv: Boolean;
 selectareObiecte: Boolean;
 nrFiltruContr, dataFiltruContr: string;

 contractAditional, modificaValGarantie: Boolean;

 idTipuriContracte, idStariContracte,
 ManProiectOfertant, ManProiectBeneficiar, NrOrdinIncepere, DataOrdinIncepere, DurataOrdinAni,
 DurataOrdinLuni, NrPVTerminare, DataPVTerminare, DurataGarantieAni, DurataGarantieLuni,
 NumarPVReceptie, DataPVReceptie, CursEuroData, CursEuro, ProcentGarantieDepusa, ProcentGarantieRetinuta: Variant;

const
  gcNewRecord = -1;
  gcUnassigned = -2;
  gcAppTempName = 'ManagInvest';  

implementation

uses DateUnit, CommonDBVar, Windows, Variants, cxStorage,
  ZAbstractConnection;


function GetMIinvestConnection : TZConnection;
begin
   if dbConnectionMInvest = nil then begin
     dbConnectionMInvest := TZConnection.Create(nil);
     dbConnectionMInvest.HostName := frmData.dbContabilitate.HostName;
     dbConnectionMInvest.Port := frmData.dbContabilitate.Port;
     dbConnectionMInvest.User := frmData.dbContabilitate.User;
     dbConnectionMInvest.Password := frmData.dbContabilitate.Password;
     dbConnectionMInvest.Protocol := frmData.dbContabilitate.Protocol;
     dbConnectionMInvest.Catalog := frmData.dbContabilitate.Catalog;
     dbConnectionMInvest.LoginPrompt := frmData.dbContabilitate.LoginPrompt;
     dbConnectionMInvest.Database := 'MInvest';
     dbConnectionMInvest.Connect;
   end;
   Result := dbConnectionMInvest;
end;

procedure ReplaceEmptyConnection (aForm : TForm);
var
  I : Integer;
begin
  for I := 0 to aForm.ComponentCount - 1 do
    if (aForm.Components[I] is TZQuery) and (TZQuery(aForm.Components[I]).Connection = nil) then
      TZQuery(aForm.Components[I]).Connection := GetMIinvestConnection;
end;

function GetTmpMInvestQry : TZQuery;
begin
  Result := TZQuery.Create(nil);
  with Result do begin
    Connection     := GetMIinvestConnection;
  end;
end;


function ArchiveRecord(aTableName, aKeyName: String; aKeyValue, aidOperatiuni: Integer): Boolean;
var
  lQry: TZQuery;
  lCreateScript: TStringList;
  lColType, lColDef: String;
  lCreateTable: Boolean;
  lFieldList:string;
begin
  Result := False;

  {ignora arhivarea tabelelor din partea de devize}
  if (LowerCase(Copy(aTableName, 1, 3))='dev') or SameText(aTableName, 'OferteLucrari')
    or SameText(aTableName, 'OferteDevizeLucrari') or SameText(aTableName, 'OferteDevizeItemsi') then Exit;


  lQry := GetTmpMInvestQry;
  lCreateScript := TStringList.Create;
  try

    lQry.SQL.Text := 'select count(*) as Total from sysobjects where id = OBJECT_ID(N''[' + aTableName + '_HIST]'') and OBJECTPROPERTY(id, N''IsUserTable'') = 1';
    lQry.Open;
    lQry.First;
    lCreateTable := lQry.FieldByName('Total').AsInteger = 0;

    lQry.Close;
    lQry.SQL.Text := 'sp_mshelpcolumns ''' + aTableName + '''';
    lQry.Open;
    lQry.First;

    if lCreateTable then
    begin
      lCreateScript.Add('CREATE TABLE [dbo].' + aTableName + '_HIST (');
      lCreateScript.Add('  [id' + aTableName + '_HIST] [int] IDENTITY (1, 1) NOT NULL,');
      lCreateScript.Add('  [idOperatiuni_HIST] [int],');
    end;

    lFieldList := '';

    while not lQry.Eof do
    begin
      if lCreateTable then
      begin
        lColType := LowerCase(lQry.FieldByName('col_typename').AsString);
        lColDef := '  [' + lQry.FieldByName('col_name').AsString + '] [' + lColType + ']';
        if Pos('char', lColType) > 0 then
          lColDef := lColDef + ' (' + lQry.FieldByName('col_len').AsString + ')';
        lColDef := lColDef + ',';
        lCreateScript.Add(lColDef);
      end;
      lFieldList := lFieldList + ','+lQry.FieldByName('col_name').AsString;
      lQry.Next;
    end;
    lQry.Close;

    if lCreateTable then
    begin
      lCreateScript.Add('  CONSTRAINT [PK_' + aTableName + '_HIST] PRIMARY KEY CLUSTERED');
      lCreateScript.Add('  (' + #13#10 + '    [id' + aTableName + '_HIST]');
      lCreateScript.Add('  ) ON [PRIMARY]' + #13#10 + ') ON [PRIMARY]');
      lQry.Close;

      lQry.SQL.Text := lCreateScript.Text;
      lQry.ExecSQL;
    end;

    lQry.SQL.Text := 'insert into ' + aTableName + '_HIST (idOperatiuni_HIST'+lFieldList+') select ' +
      IntToStr(aidOperatiuni) + ' as idOperatiuni_HIST, * from ' + aTableName +
      ' where ' + aKeyName + ' = ' + IntToStr(aKeyValue);

    lQry.ExecSQL;
  finally
    lQry.Free;
    lCreateScript.Free;
  end;
end;

function BeginOp(aDenumire: String): Integer;
var
  lQry: TZQuery;
begin
  Result := -1;

  lQry := GetTmpMInvestQry;
  try
    lQry.SQL.Text := 'select top 0 * from Operatiuni';
    lQry.Open;
    lQry.Append;
    lQry.FieldByName('idSesiuni').AsInteger := SessionID;
    lQry.FieldByName('Denumire').AsString := aDenumire;
    lQry.Post;
    Result := lQry.FieldByName('idOperatiuni').AsInteger;
    lQry.Close;
  finally
    lQry.Free;
  end;
end;
//------------------------------------------------------------------------------
procedure EndOp(aidOperatiuni: Integer);
var
  lQry: TZQuery;
begin
  lQry := GetTmpMInvestQry;
  try
    lQry.SQL.Text := 'update Operatiuni set DataEndOp = GETDATE() where idOperatiuni = :idOperatiuni';
    lQry.ParamByName('idOperatiuni').Value := aidOperatiuni;
    lQry.ExecSQL;
  finally
    lQry.Free;
  end;
end;
//------------------------------------------------------------------------------
procedure StartNewSession;
begin
  with GetTmpMInvestQry do
  try
    SQL.Add('exec spStartSession :idUtilizatori, :DataLocala, :Computer, :Username, :Versiune');
    ParamByName('idUtilizatori').Value := IdUtilizator;
    ParamByName('DataLocala').Value := Now;
    ParamByName('Computer').Value := GetHostName;
    ParamByName('Username').Value := NumeLogin;
    ParamByName('Versiune').Value := ExeVersion;
    Open;
    FSessionID := FieldByName('idSesiuni').AsInteger;
  finally
    Free;
  end;
end;

function SessionID : Integer;
begin
  if FSessionID = 0 then
    StartNewSession;
  Result :=  FSessionID;
end;
//------------------------------------------------------------------------------
procedure OpenQryWithParam(aForm: TForm; aParamName: String; aParamValue: OleVariant);
var
  lQry: TZQuery;
  lParam: TParam;
  i: Integer;
begin
//deschide set de date care au parametrii si atribuie valori pe parametrii
  if not Assigned(aForm) or (aParamName = '') then
    Exit;

  for i := 0 to aForm.ComponentCount - 1 do
  begin
    if aForm.Components[i] is TZQuery then
    begin
      lQry := TZQuery(aForm.Components[i]);
      lParam := lQry.Params.FindParam(aParamName);
      if Assigned(lParam) then
      begin
        lQry.Active := False;
        lParam.Value := aParamValue;
        lQry.Active := True;
      end;
    end;
  end;
end;
//------------------------------------------------------------------------------
procedure OpenDataSet(aDataSet: TDataSet);
begin
  aDataSet.Active := True;
  aDataSet.Tag := aDataSet.Tag + 1;
end;

//------------------------------------------------------------------------------
procedure RefreshDataSet(aDataSet: TDataSet; aKeyName: String);
var
  lKeyName: String;
  i, lKeyValue: Integer;
  lHaveKey: Boolean;
begin
//deschide set de date
  if aDataSet.Active then
  begin
    if aDataSet.State in [dsInsert, dsEdit] then
      aDataSet.Post;

    lHaveKey := False;
    if lKeyName = '' then
    begin
      for i := 0 to aDataSet.FieldCount - 1 do
      begin
        if aDataSet.Fields[i].DataType = ftAutoInc then
        begin
          lKeyName := aDataSet.Fields[i].FieldName;
          lHaveKey := True;
          Break;
        end;
      end;
    end
    else
      lHaveKey := True;

    if lHaveKey then
      lKeyValue := aDataSet.FieldByName(lKeyName).AsInteger
    else
    begin
      lKeyName := aDataSet.Fields[0].FieldName;
      lKeyValue := aDataSet.Fields[0].AsInteger;
    end;

    aDataSet.Close;
    aDataSet.Open;
    aDataSet.Locate(lKeyName, lKeyValue, []);
  end
  else
    OpenDataSet(aDataSet);
end;
//------------------------------------------------------------------------------
procedure SaveDataSet(aDataSet: TDataSet);
{var
  lDataSet: TCustomADODataSet;   }
begin
//salvare set de date
  if aDataSet.State in [dsInsert, dsEdit] then
    aDataSet.Post;
  if (aDataSet is TZQuery) and (TZQuery(aDataSet).CachedUpdates) then
    try
      TZQuery(aDataSet).ApplyUpdates;
      TZQuery(aDataSet).CommitUpdates;
    except
      TZQuery(aDataSet).CancelUpdates;
    end;

 {
  if aDataSet is TCustomADODataSet then
  begin
    lDataSet := TCustomADODataSet(aDataSet);
    if lDataSet.LockType = ltBatchOptimistic then
      lDataSet.UpdateBatch();
  end;
  }
end;
//------------------------------------------------------------------------------
procedure InsertInArbore(tip_frunza : string; id_frunza : integer; RefTipFrunza : integer; Denumire : string);
var
  lQry : TZQuery;
  new_id : integer;
begin
  lQry := GetTmpMInvestQry;
  try
    //lQry.SQl.Text  :=  ' select max(id_arbore) + 1 as new_id from DevArbore ';
    lQry.SQl.Text  :=  ' select isnull(LastKey,0) + 1 as new_id from LAST_KEY where Tabela =''DevArbore''';
    lQry.Open;
    new_id := lQry.FieldByName('new_id').AsInteger;

    lQry.Close;
    lQry.SQL.Text := ' insert into DevArbore (id_arbore, ref_parinte, denumire, tip_frunza, RefTipFrunza, id_frunza) values ' +
                 '(:id_arbore, :ref_parinte, :denumire, :tip_frunza, :RefTipFrunza, :id_frunza)';
    lQry.ParamByName('id_arbore').Value := new_id;
    lQry.ParamByName('ref_parinte').Value := 1;
    lQry.ParamByName('denumire').Value := Denumire;
    lQry.ParamByName('tip_frunza').Value := tip_frunza;
    lQry.ParamByName('Reftipfrunza').Value := RefTipFrunza;
    lQry.ParamByName('id_frunza').Value := id_frunza;
    lQry.ExecSql;
    lQry.Close;
    lQry.SQl.Text  :=  ' update LAST_KEY set LastKey= ' + IntToStr(new_id)+ ' where Tabela =''DevArbore''';
    lQry.ExecSQL;
  finally
  lQry.Free;
  end;
end;
//------------------------------------------------------------------------------
function GetAppTempFolder: String;
var
  lFolder: String;
begin
  SetLength(lFolder, MAX_PATH);
  SetLength(lFolder, GetTempPath(Length(lFolder), PChar(lFolder)));
  Result := ExpandFileName(lFolder + '\' + gcAppTempName);
end;
//------------------------------------------------------------------------------
function ArhiveazaInregistrare(aTableName, aKeyName: String;
  aKeyValue, aidOperatiuni: Integer): Boolean;
var
  lQry: TZQuery;
  lCreateScript: TStringList;
  lColType, lColDef: String;
begin
  Result := False;

  lQry := GetTmpMInvestQry;
  lCreateScript := TStringList.Create;
  try

    lQry.SQL.Text := 'select count(*) as Total from sysobjects where id = OBJECT_ID(N''[' + aTableName + '_ARH]'') and OBJECTPROPERTY(id, N''IsUserTable'') = 1';
    lQry.Open;
    lQry.First;
    if lQry.FieldByName('Total').AsInteger = 0 then
    begin
      lQry.Close;
      lQry.SQL.Text := 'sp_mshelpcolumns ''' + aTableName + '''';
      lQry.Open;
      lQry.First;

      lCreateScript.Add('CREATE TABLE [dbo].' + aTableName + '_ARH (');
      lCreateScript.Add('  [id' + aTableName + '_ARH] [int] IDENTITY (1, 1) NOT NULL,');
      lCreateScript.Add('  [idOperatiuni_ARH] [int],');
      while not lQry.Eof do
      begin
        lColType := LowerCase(lQry.FieldByName('col_typename').AsString);
        lColDef := '  [' + lQry.FieldByName('col_name').AsString + '] [' + lColType + ']';
        if Pos('char', lColType) > 0 then
          lColDef := lColDef + ' (' + lQry.FieldByName('col_len').AsString + ')';
        lColDef := lColDef + ',';
        lCreateScript.Add(lColDef);
        lQry.Next;
      end;

      lCreateScript.Add('  CONSTRAINT [PK_' + aTableName + '_ARH] PRIMARY KEY CLUSTERED');
      lCreateScript.Add('  (' + #13#10 + '    [id' + aTableName + '_ARH]');
      lCreateScript.Add('  ) ON [PRIMARY]' + #13#10 + ') ON [PRIMARY]');
      lQry.Close;

      lQry.SQL.Text := lCreateScript.Text;
      lQry.ExecSQL;
    end;

    lQry.Close;
    lQry.SQL.Text := 'insert into ' + aTableName + '_ARH select ' +
      IntToStr(aidOperatiuni) + ' as idOperatiuni_ARH, * from ' + aTableName +
      ' where ' + aKeyName + ' = ' + IntToStr(aKeyValue);
    lQry.ExecSQL;
    lQry.Close;
    lQry.SQL.Text := 'delete from ' + aTableName  +
      ' where ' + aKeyName + ' = ' + IntToStr(aKeyValue);
    lQry.ExecSQL;
  finally
    lQry.Free;
    lCreateScript.Free;
  end;
end;
//------------------------------------------------------------------------------
function IsNull(AValue: Variant; ANewValue: Variant): Variant;
begin
  if VarIsNull(AValue) or VarIsEmpty(AValue) then
    Result := ANewValue
  else
    Result := AValue;
end;
//------------------------------------------------------------------------------
//------------------------------------------------------------------------------
procedure SavePreferences(aPropStore: TcxPropertiesStore);
var
  lQry: TZQuery;
  lMemStream: TMemoryStream;
  lPropName: String;
begin
  if Assigned(aPropStore.Owner) then
  begin
    lPropName := aPropStore.Owner.ClassName;
    lQry := GetTmpMInvestQry;
    lMemStream := TMemoryStream.Create();
    try
      aPropStore.Active := False;
      aPropStore.StorageType := stStream;
      aPropStore.StorageStream := lMemStream;
      aPropStore.StoreTo();
      aPropStore.StorageStream := nil;
      lMemStream.Position := 0;
      
      lQry.SQL.Text := 'delete from UtilizatoriPreferinte where idUtilizatori = :idUtilizatori and Denumire = :Denumire';
      lQry.ParamByName('idUtilizatori').Value := commondbvar.IdUtilizator;
      lQry.ParamByName('Denumire').Value := lPropName;
      lQry.ExecSQL;
      lQry.SQL.Text := 'select top 0 * from UtilizatoriPreferinte';
      lQry.Open;
      lQry.Append;
      lQry.FieldByName('idUtilizatori').AsInteger := commondbvar.IdUtilizator;
      lQry.FieldByName('Denumire').AsString := lPropName;
      TBlobField(lQry.FieldByName('Stream')).LoadFromStream(lMemStream);
      lQry.Post;
      lQry.Close;
    finally
      lQry.Free;
      lMemStream.Free;
    end;
  end;
end;
//------------------------------------------------------------------------------
procedure LoadPreferences(aPropStore: TcxPropertiesStore);
var
  lQry: TZQuery;
  lMemStream: TMemoryStream;
  lPropName: String;
begin
  if Assigned(aPropStore.Owner) then
  begin
    lPropName := aPropStore.Owner.ClassName;
    lQry := GetTmpMInvestQry;
    lMemStream := TMemoryStream.Create();
    try
      lQry.SQL.Text := 'select top 1 * from UtilizatoriPreferinte where idUtilizatori = :idUtilizatori and Denumire = :Denumire';
      lQry.ParamByName('idUtilizatori').Value := commondbvar.IdUtilizator;
      lQry.ParamByName('Denumire').Value := lPropName;
      lQry.Open;

      if not (lQry.Bof and lQry.Eof) then
      begin
        TBlobField(lQry.FieldByName('Stream')).SaveToStream(lMemStream);
        lMemStream.Position := 0;
        aPropStore.Active := False;
        aPropStore.StorageType := stStream;
        aPropStore.StorageStream := lMemStream;
        aPropStore.RestoreFrom;
        aPropStore.StorageStream := nil;
      end;

      lQry.Close;
    finally
      lQry.Free;
      lMemStream.Free;
    end;
  end;
end;
//------------------------------------------------------------------------------
initialization
finalization
  if dbConnectionMInvest <> nil then
    dbConnectionMInvest.Free;
end.
