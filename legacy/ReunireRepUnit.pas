unit ReunireRepUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, dxDBCtrl, dxCntner, dxTL, Buttons, Db, ZDataSet,
  dxDBTL, dxEditor, dxEdLib, dxDBELib, dxExEdtr, Menus,
  cxLookAndFeelPainters, cxButtons,
  ZAbstractRODataset, ZAbstractDataset,
  cxGraphics,
  cxLookAndFeels;

type
  TfrmReunireRep = class(TForm)
    Label1: TLabel;
    Label2: TLabel;
    TreeRepartitori: TdxTreeList;
    GridRepartitori: TdxDBTreeList;
    DTRepartitori: TDataSource;
    QryRepartitori: TZQuery;
    GridRepartitoriNUME: TdxDBTreeListMaskColumn;
    GridRepartitoriCODSECTIE: TdxDBTreeListMaskColumn;
    TreeRepartitoriNume: TdxTreeListColumn;
    GridRepartitoriADRESA: TdxDBTreeListMaskColumn;
    GridRepartitoriCOD_FISCAL: TdxDBTreeListMaskColumn;
    TreeRepartitoriADRESA: TdxTreeListColumn;
    TreeRepartitoriCOD_FISCAL: TdxTreeListColumn;
    AtsDBEdit1: TdxDBEdit;
    Label3: TLabel;
    AtsDBEdit2: TdxDBEdit;
    AtsDBEdit3: TdxDBEdit;
    Label4: TLabel;
    AtsDBEdit4: TdxDBEdit;
    BtnAdauga: TcxButton;
    BtnRemove: TcxButton;
    BtnRemoveAll: TcxButton;
    BtnCancel: TcxButton;
    BtnOk: TcxButton;
    procedure BtnAdaugaClick(Sender: TObject);
    procedure BtnRemoveAllClick(Sender: TObject);
    procedure BtnRemoveClick(Sender: TObject);
    procedure GridRepartitoriChangeNode(Sender: TObject; OldNode,
      Node: TdxTreeListNode);
    procedure GridRepartitoriDblClick(Sender: TObject);
    procedure TreeRepartitoriDblClick(Sender: TObject);
    procedure BtnOkClick(Sender: TObject);
  private
    FIdRepartitor: Integer;
    FNameRep : String;
    procedure SetIdRepartitor(const Value: Integer);
    function  ExistaRep(AIdRep: Integer): Boolean;
    procedure SetDeleteEnabled;
    { Private declarations }
  public
    { Public declarations }
    procedure SalveazaModificari;

    property IdRepartitor: Integer read FIdRepartitor write SetIdRepartitor;
  end;

implementation

{$R *.DFM}

uses
  ZeosDBUtile, DateUnit, CommonDBVar, ATSZDBUtils;

procedure TfrmReunireRep.BtnAdaugaClick(Sender: TObject);
var lNode: TdxDBTreeListNode;
begin
  lNode := TdxDBTreeListNode(GridRepartitori.FocusedNode);
  if (not Assigned(lNode)) or (lNode.HasChildren) or (ExistaRep(lNode.Id)) then Exit;
  { Adaugam Repartitorul in Lista }
  with TreeRepartitori.Add do begin
    Data := Pointer(Integer(lNode.Id));
    Strings[TreeRepartitoriNume.Index]       := lNode.Strings[GridRepartitoriNUME.Index];
    Strings[TreeRepartitoriADRESA.Index]     := lNode.Strings[GridRepartitoriADRESA.Index];
    Strings[TreeRepartitoriCOD_FISCAL.Index] := lNode.Strings[GridRepartitoriCOD_FISCAL.Index];
  end;
  SetDeleteEnabled;
end;

procedure TfrmReunireRep.SalveazaModificari;
var
  lWhereString: String;
  lNameList   : String;
  I           : Integer;
  IdModificare: Integer;
  lTblList    : TZReadOnlyQuery;
  QryScalar   : TZReadOnlyQuery;

  procedure UpdateRep(ATbl: String; AKeyField: String; AField: String);
  begin
    with DBNewQuery do
      try
        ParamCheck := False;
        Sql.Add('SELECT TOP 1 1 FROM SYSCOLUMNS WHERE ID = OBJECT_ID(' + QuotedStr(Trim(ATbl)) + ') AND NAME LIKE ' + QuotedStr(Trim(AField)));
        Open;
        if not IsEmpty then
        begin
          Close;
          Sql.Clear;


          Sql.Add('INSERT INTO LST_MODIFICARI_REPARTITORI (ID_MODIFICARE, ID_LOGIN, TBL_NAME, KEY_NAME, FIELD_NAME, OLD_VALUE, NEW_VALUE, KEY_ID, OLD_NAME, NEW_NAME)');
          Sql.Add('SELECT ' + IntToStr(IdModificare) + ', ' + IntToStr(IdLogin) + ',');
          Sql.Add(QuotedStr(ATbl) + ', ' + QuotedStr(AKeyField) + ', ' + QuotedStr(AField) + ', ' + QuotedStr(lWhereString) + ', ' + IntToStr(FIdRepartitor) + ', CONVERT(VARCHAR(128), ' + AKeyField + '), ');
          Sql.Add(QuotedStr(FNameRep) + ', ' + QuotedStr(lNameList) + ' FROM ' + ATbl + ' WHERE ' + AField + ' IN (' + lWhereString + ')');

          Sql.Add('ALTER TABLE ' + ATbl + ' DISABLE TRIGGER ALL');
          Sql.Add('UPDATE ' + ATbl + ' SET ' + AField + ' = ' + IntToStr(FIdRepartitor) + ' WHERE ' + AField + ' IN (' + lWhereString + ')');
          Sql.Add('ALTER TABLE ' + ATbl + ' ENABLE TRIGGER ALL');

          ExecSql;
        end;
      finally
        Free;
      end;
  end;

begin
  lWhereString := '';
  lNameList := '';

  with TreeRepartitori do
    for I := 0 to Count - 1 do
    begin
      lWhereString := lWhereString + IntToStr(Integer(Items[I].Data)) + ',';
      lNameList := lNameList + Trim(Items[I].Strings[TreeRepartitoriNume.Index]) + ',';
    end;

  if lWhereString > '' then
  begin
    lWhereString := Copy(lWhereString, 1, Length(lWhereString) - 1);
    lNameList := Copy(lNameList, 1, Length(lNameList) - 1);

    DBStartTransaction;
    try

      QryScalar := DBNewQuery;
      try
        QryScalar.SQL.Text := 'exec SP_GET_NEXT_VALUE ''REUNIRE_REPARTITORI''';
        QryScalar.Open;
        if not QryScalar.IsEmpty then
          IdModificare := QryScalar.Fields[0].AsInteger
        else
          raise Exception.Create('SP_GET_NEXT_VALUE nu a returnat niciun rezultat!');
      finally
        QryScalar.Free;
      end;


      lTblList := DBNewQuery;
      try
        lTblList.SQL.Add('exec spGetRepTableList');
        lTblList.Open;
        while not lTblList.Eof do
        begin
          UpdateRep(
            lTblList.FieldByName('tblName').AsString,
            lTblList.FieldByName('keyName').AsString,
            lTblList.FieldByName('RepFieldName').AsString
          );
          lTblList.Next;
        end;


        lTblList.SQL.Clear;
        lTblList.SQL.Add('exec spEndRepUnificare ' + QuotedStr(lWhereString));
        lTblList.ExecSQL;
      finally
        lTblList.Free;
      end;

      DBCommit;
    except
      on E: Exception do
      begin
        DBRollBack;
        raise EContaHandledError.Create(E.Message);
      end;
    end;
  end;
end;


procedure TfrmReunireRep.SetIdRepartitor(const Value: Integer);
begin
  FIdRepartitor := Value;
  FNameRep := DBGetScallarFmt('SELECT NUME FROM REPARTITORI WHERE ID_REPARTITORI = %d', [Value]);
  QryRepartitori.Open;
end;

procedure TfrmReunireRep.BtnRemoveAllClick(Sender: TObject);
begin
  if MessageDlg('Doriti eliminarea tuturor repartitorilor din lista de reunire ?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then TreeRepartitori.ClearNodes;
  SetDeleteEnabled;
end;

procedure TfrmReunireRep.BtnRemoveClick(Sender: TObject);
var lNode: TdxTreeListNode;
begin
  lNode := TreeRepartitori.FocusedNode;
  if Assigned(lNode) then
     if MessageDlg('Doriti eliminarea repartitorului '+lNode.Strings[TreeRepartitoriNume.Index]+'?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then lNode.Free;
  SetDeleteEnabled;
end;

function TfrmReunireRep.ExistaRep(AIdRep: Integer): Boolean;
var I: Integer;
begin
  Result := AIdRep = FIdRepartitor;
  if not Result then
    for I := 0 to TreeRepartitori.Count-1 do
      if Integer(TreeRepartitori.Items[I].Data) = AIdRep then begin
         Result := True;
         Break;
      end;
end;

procedure TfrmReunireRep.GridRepartitoriChangeNode(Sender: TObject; OldNode,
  Node: TdxTreeListNode);
var lNode: TdxDBTreeListNode;
begin
  lNode := TdxDBTreeListNode(GridRepartitori.FocusedNode);
  BtnAdauga.Enabled := (Assigned(lNode)) and (not ExistaRep(lNode.Id));
end;

procedure TfrmReunireRep.SetDeleteEnabled;
var lNode: TdxDBTreeListNode;
begin
  lNode := TdxDBTreeListNode(GridRepartitori.FocusedNode);
  BtnAdauga.Enabled := (Assigned(lNode)) and (not ExistaRep(lNode.Id));
  BtnRemove.Enabled := TreeRepartitori.Count > 0;
  BtnRemoveAll.Enabled := BtnRemove.Enabled;
end;

procedure TfrmReunireRep.GridRepartitoriDblClick(Sender: TObject);
begin
  BtnAdauga.Click;
end;

procedure TfrmReunireRep.TreeRepartitoriDblClick(Sender: TObject);
begin
  BtnRemove.Click;
end;

procedure TfrmReunireRep.BtnOkClick(Sender: TObject);
begin
  if MessageDlg('Doriti ca toti repartitorii selectati sa fie transformati in repartitorul curent selectat ?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then
     SalveazaModificari;
  ModalResult := mrOk;
end;

end.
