unit ReconciliereDocumUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  Db, ZDataSet, StdCtrls, Buttons, dxCntner, dxTL, dxDBCtrl,
  dxDBTL, dxDBTLCl, ImgList, dxExEdtr,
  ZAbstractRODataset, ZAbstractDataset;

type
  TfrmReconciliere = class(TForm)
    DTDocumente: TDataSource;
    QryDocumente: TZQuery;
    TreeModificari: TdxDBTreeList;
    BtnOk: TBitBtn;
    TreeModificariDESCRIERE: TdxDBTreeListMaskColumn;
    TreeModificariID_DOCUMENT: TdxDBTreeListMaskColumn;
    Imagini: TImageList;
    TreeModificariTIP_CANTITATE: TdxDBTreeListImageColumn;
    TreeModificariTIP_PRET: TdxDBTreeListImageColumn;
    procedure TreeModificariDblClick(Sender: TObject);
  private
    { Private declarations }
    procedure AplicaModificari;
    function  MustModify: Boolean;
  public
    { Public declarations }
  end;

procedure Reconciliere(OldId: Integer; NewId: Integer);

implementation

{$R *.DFM}

uses DateUnit, TCVUnit;

procedure Reconciliere(OldId: Integer; NewId: Integer);
begin
  Exit;
  with TfrmReconciliere.Create(Application) do
    try
       QryDocumente.Close;
       QryDocumente.Params[0].Value := OldId;
       QryDocumente.Params[1].Value := NewId;
       QryDocumente.Open;
       { Intai verificam daca avem conflicte }
       if MustModify then begin
          { Afisam ce a modificat }
          ShowModal;
          { Facem inainte ca sa nu mai avem probleme }
          AplicaModificari;
       end;
    finally
       Free;
    end;
end;


{ TfrmReconciliere }

{ TfrmReconciliere }

procedure TfrmReconciliere.AplicaModificari;
var I, J, T: Integer;
    lDocNode,
    lCodNode,
    lItemNode : TdxDBTreeListNode;
    lDocId    : String;
    FakeQry   : TZReadOnlyQuery;
begin
  Exit;
  { Aplicam modificarile necesare }
  FakeQry := GetTmpADOQuery;
  try
    FakeQry.ParamCheck := False;
    FakeQry.Sql.Add('UPDATE GEST_DOCUM SET STARE = 0');
    FakeQry.Sql.Add('');
    for I := 0 to TreeModificari.Count - 1 do begin
      lDocNode := TdxDBTreeListNode(TreeModificari.Items[I]);
      for J := 0 to lDocNode.Count-1 do begin
        lCodNode := TdxDBTreeListNode(lDocNode.Items[J]);
        for T := 0 to lCodNode.Count-1 do begin
          lItemNode := TdxDBTreeListNode(lCodNode.Items[T]);
          lDocId    := lItemNode.Strings[TreeModificariID_DOCUMENT.Index];
          if (Trim(lDocId) > '') and (lItemNode.Strings[TreeModificariTIP_CANTITATE.Index] = '1') then begin
             FakeQry.Sql[1] := 'WHERE ID_GEST_DOCUM = '+lDocId+' AND STARE=1';
             FakeQry.ExecSql;
          end;
        end;
      end;
    end;
  finally
    FakeQry.Free;
  end;
end;

function TfrmReconciliere.MustModify: Boolean;
var I, J: Integer;
begin
  Result := False;
  for I := 0 to TreeModificari.Count-1 do
    for J := 0 to TreeModificari.Items[I].Count - 1 do
      if TreeModificari.Items[I].Items[J].Count > 0 then begin
         Result := not Result;
         Exit;
      end;
end;

procedure TfrmReconciliere.TreeModificariDblClick(Sender: TObject);
var lNode: TdxDBTreeListNode;
    lDocId: Integer;
begin
  { Deschidem documentul sa-l poata modifica }
  lNode := TdxDBTreeListNode(TreeModificari.FocusedNode);
  if (Assigned(lNode)) and (lNode.Level = 2) and (Trim(lNode.Strings[TreeModificariID_DOCUMENT.Index]) > '') then begin
     lDocId := StrToInt(Trim(lNode.Strings[TreeModificariID_DOCUMENT.Index]));
     with TFrmTCV.Create(Application) do
       try
          FormStyle  := fsNormal;
          Visible    := False;
          WindowState := wsMaximized;
          { Setam documentul curent }
          ReadDocument;
          if (QryItemsi.Active) and (not QryItemsi.IsEmpty) then
             case MessageDlg('Aveti pozitii introduse in ecranul de culegere !'#13#10'Doriti salvarea acestora?',
                             mtConfirmation, [mbYes, mbNo, mbCancel], 0) of
               mrCancel: Exit;
               mrYes   : ValidareDocument;
               mrNo    : ;
             end;
          DocIdParinte := lDocId;
          CopyDocToCulegere(lDocId);
          ReadDocument;

          ShowModal;
          { Reafisam documentele in conflict }
          Self.QryDocumente.Close;
          Self.QryDocumente.Open;
       finally
          Free;
       end;
  end;
end;

end.
