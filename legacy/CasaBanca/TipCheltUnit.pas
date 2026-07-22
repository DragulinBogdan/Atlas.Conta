unit TipCheltUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  dxInspRw, dxDBInRw, dxDBInsp, dxInspct, ExtCtrls, dxCntner, dxTL,
  dxDBCtrl, dxDBCtrl, StdCtrls, Buttons, dxDBTLCl, DB, DBTables, HeadPanel;

type
  TFrmTipCheltuiala = class(TForm)
    GrTipCheltuieli: TGroupBox;
    Splitter1: TSplitter;
    TreeCheltuieli: TdxDBTreeList;
    pn: TPanel;
    btnAdd: TBitBtn;
    btnAddChild: TBitBtn;
    btnDelete: TBitBtn;
    TreeCheltuieliDENUMIRE: TdxDBTreeListMaskColumn;
    TreeCheltuieliID_PARINTE: TdxDBTreeListMaskColumn;
    TreeCheltuieliTIP: TdxDBTreeListImageColumn;
    TreeCheltuieliCOD: TdxDBTreeListMaskColumn;
    pnBottom: TPanel;
    btnOk: TBitBtn;
    TreeCheltuieliID_TIPURI_CHELTVEN: TdxDBTreeListMaskColumn;
    TreeCheltuieliREALCOD: TdxDBTreeListMaskColumn;
    HeadPanel1: THeadPanel;
    procedure btnAddClick(Sender: TObject);
    procedure btnAddChildClick(Sender: TObject);
    procedure btnDeleteClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure TreeCheltuieliCODGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure TreeCheltuieliKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
  private
    { Private declarations }
  public
    { Public declarations }
    {Adugam un Nod Relativ la un Nod Parinte}
    function AddNode(aParentNode: TdxDBTreeListNode) : TdxDBTreeListNode;
    function GetLastCod(aNode:TdxDBTreeListNode) : Integer;
  end;

implementation

uses DateUnit;

{$R *.DFM}

function TFrmTipCheltuiala.AddNode(
  aParentNode: TdxDBTreeListNode): TdxDBTreeListNode;
var aId : Integer;
    aNewCod : Integer;
    OldState : Boolean;
begin
  with FrmData.QryTipCheltVen do begin
    {luam intai ultimul cod}
    aNewCod := GetLastCod(aParentNode)+1;
    {inseram copilul}
    Append;
    {identificam noul nod}
    aId := FrmData.ID_Tip_CheltVen;
    Result := TreeCheltuieli.FindNodeByKeyValue(aId);
    OldState := State in [dsEdit, dsInsert];
    if not OldState then Edit;

    if aParentNode <> nil then
       FieldByName('ID_PARINTE').AsInteger := aParentNode.Id
    else FieldByName('ID_PARINTE').Clear;

    FieldByName('COD').AsString := IntToStr(aNewCod);
    Post;
    if OldState then Edit;
  end;

end;

procedure TFrmTipCheltuiala.btnAddClick(Sender: TObject);
begin
  AddNode(nil);
end;

procedure TFrmTipCheltuiala.btnAddChildClick(Sender: TObject);

  function ArePozitii(aId : Integer) : Boolean;
  begin
    with GetTmpADOQuery do
      try
        SQL.Add('SELECT TOP 1 1 FROM BREG_P WHERE ID_TIPURI_CHELTVEN = :ID_TIPURI_CHELTVEN');
        Params.ParamByName('ID_TIPURI_CHELTVEN').Value := aID;
        Open;
        Result := (RecordCount>0);
        Close;
      finally
        Free;
      end;
  end;

  procedure Actualizare(aOldId, aNewId:Integer);
  begin
    with GetTmpADOQuery do
       try
          SQL.Add('UPDATE BREG_P SET ID_TIPURI_CHELTVEN = :ID_TIPURI_CHELTVEN WHERE ID_TIPURI_CHELTVEN = :OLD_ID_TIPURI_CHELTVEN');
          Params.ParamByName('ID_TIPURI_CHELTVEN').Value := aNewId;
          Params.ParamByName('OLD_ID_TIPURI_CHELTVEN').Value := aOldId;
          ExecSQL;
       finally
         Free;
       end;
  end;

var aNode, aNewNode : TdxDBTreeListNode;
    OldState : Boolean;
    aNewCod  : Integer;
begin
  {testam daca se afla inregistrari pe nivelul parinte}
  if not Assigned(TreeCheltuieli.FocusedNode) then Exit;
  aNode := TdxDBTreeListNode(TreeCheltuieli.FocusedNode);
  {daca nu are copii dar are pozitii pe parinte atunci trebuie creat un copil pentru acele pozitii}
  if not (aNode.HasChildren) and ArePozitii(aNode.Values[TreeCheltuieliID_TIPURI_CHELTVEN.Index]) then begin
    {creem un copil pentru asta sau gasim copilul cu codul 0 si actualizam in breg_p}
    aNewNode := AddNode(aNode);
    with FrmData.QryTipCheltVen do begin
      OldState := State in [dsEdit, dsInsert];
      if not OldState then Edit;
      FieldByName('DENUMIRE').AsString :=  'Cumulativ ' + aNode.Strings[TreeCheltuieliDENUMIRE.Index];
      FieldByName('COD').AsString := '0';
      Post;
      if OldState then Edit;
    end;
    Actualizare(aNode.Values[TreeCheltuieliID_TIPURI_CHELTVEN.Index], aNewNode.Values[TreeCheltuieliID_TIPURI_CHELTVEN.Index]);
  end;
  {aici se creaza noul copil}
  AddNode(aNode);
  {ne pozitionam pe parinte}
  aNode.MakeVisible;
  aNode.Focused := True;
end;

procedure TFrmTipCheltuiala.btnDeleteClick(Sender: TObject);
begin
  if Not Assigned(TreeCheltuieli.FocusedNode) then Exit;
end;

procedure TFrmTipCheltuiala.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

function TFrmTipCheltuiala.GetLastCod(aNode: TdxDBTreeListNode): Integer;
var I:Integer;
begin
  Result := 0;
  if aNode <> nil then begin
    if aNode.HasChildren then
       for I := 0 to aNode.Count-1 do
         if Result < aNode.Items[I].Values[TreeCheltuieliREALCOD.Index] then Result := aNode.Items[I].Values[TreeCheltuieliREALCOD.Index];
  end
  else begin
    for I := 0 to TreeCheltuieli.Count-1 do
      if Result < TreeCheltuieli.Items[I].Values[TreeCheltuieliREALCOD.Index] then Result := TreeCheltuieli.Items[I].Values[TreeCheltuieliREALCOD.Index];
  end;
end;

procedure TFrmTipCheltuiala.TreeCheltuieliCODGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
begin
  if Assigned(aNode.Parent) then
     AText := ANode.Parent.Strings[TreeCheltuieliCOD.Index] + '.'+ AText
  else
     AText := AText;
end;

procedure TFrmTipCheltuiala.TreeCheltuieliKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
   if ((Key = VK_INSERT) and not (ssCtrl in Shift)) then AddNode(nil);
end;

end.
