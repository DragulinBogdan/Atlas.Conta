unit OrganigramaUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ExtCtrls, HeadPanel, dxCntner, dxTL, dxDBCtrl, dxDBGrid, 
  Buttons, DB, dxExEdtr;

type
  TFrmOrganigrama = class(TForm)
    pnInfo: THeadPanel;
    pnTot: TPanel;
    GridFunctii: TdxDBGrid;
    GridFunctiiID_ORGANIGRAMA: TdxDBGridMaskColumn;
    GridFunctiiDENUMIRE: TdxDBGridMaskColumn;
    GridFunctiiID_PARINTE: TdxDBGridMaskColumn;
    pnBottom: TPanel;
    btnOk: TSpeedButton;
    procedure btnOkClick(Sender: TObject);
    procedure GridFunctiiDblClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

implementation

uses
  ZeosDBUtile, DateUnit, FunctieRepUnit;

{$R *.DFM}

procedure TFrmOrganigrama.btnOkClick(Sender: TObject);
begin
  DBPost(frmData.QryOrganigrama);
  ModalResult := mrOk;
end;

procedure TFrmOrganigrama.GridFunctiiDblClick(Sender: TObject);
var aIdOrganigrama : Integer;
    aNode : TdxTreeListNode;
begin
  if not Assigned(GridFunctii.FocusedNode) then Exit;
  if FrmData.QryOrganigrama.State in [dsEdit, dsInsert] then  FrmData.QryOrganigrama.Post;
  aNode := GridFunctii.FocusedNode;
  aIdOrganigrama := aNode.Values[GridFunctiiID_ORGANIGRAMA.Index];
  if aIdOrganigrama> 0 then ModificaUtilizatori(aIdOrganigrama);
end;

procedure TFrmOrganigrama.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  
    Action := caFree;
end;

end.
