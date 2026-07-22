unit fmSelectieRepartitorUnit;

interface

uses
  Forms, Classes, Controls, ExtCtrls, cxGraphics, cxControls,
  cxLookAndFeels, cxLookAndFeelPainters, cxCustomData, cxStyles, cxTL,
  cxTLdxBarBuiltInMenu, cxInplaceContainer, cxTLData, cxDBTL, DB,
  ZAbstractRODataset, ZDataset, cxPC, cxContainer, cxEdit, cxMaskEdit,
  cxTextEdit, cxDropDownEdit, cxImageComboBox, StdCtrls, cxProgressBar,
  dxScrollbarAnnotations;

type
  TfmSelectieRepartitor = class(TForm)
    dtRepartitori: TDataSource;
    qryRepartitori: TZReadOnlyQuery;
    cxTreeRepartitori: TcxDBTreeList;
    cxTreeRepartitoriNUME: TcxDBTreeListColumn;
    cxTreeRepartitoriADRESA: TcxDBTreeListColumn;
    cxTreeRepartitoriCONT: TcxDBTreeListColumn;
    cxTreeRepartitoriCODFISC: TcxDBTreeListColumn;
    cxTreeRepartitoriGESTINT: TcxDBTreeListColumn;
    procedure cxTreeRepartitoriKeyUp(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure cxTreeRepartitoriDblClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
  public
    procedure SetFiltru(ATipGestInt: Integer; const ATipRepartitori: String);
  end;

implementation

{$R *.DFM}

uses
  ZeosDBUtile,
  Windows;

procedure TfmSelectieRepartitor.cxTreeRepartitoriKeyUp(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
    with TcxDBTreeList(Sender) do
      if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
        (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk
  else if Key = VK_ESCAPE then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrCancel;
end;

procedure TfmSelectieRepartitor.FormCreate(Sender: TObject);
begin
  OpenDataSets(Self);
end;

procedure TfmSelectieRepartitor.SetFiltru(ATipGestInt: Integer;
  const ATipRepartitori: String);
begin
  qryRepartitori.ParamByName('refUser').AsInteger          := iUserID;
  qryRepartitori.ParamByName('tipRelatie').AsInteger       := ATipGestInt;
  qryRepartitori.ParamByName('tipuriRepartitori').AsString := ATipRepartitori;
  DBRefresh(qryRepartitori);
end;

procedure TfmSelectieRepartitor.cxTreeRepartitoriDblClick(Sender: TObject);
begin
  with TcxDBTreeList(Sender) do
      if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
        (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk;
end;

end.