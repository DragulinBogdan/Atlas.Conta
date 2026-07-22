unit DetaliiDecontUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  dxExEdtr, dxEdLib, dxCntner, dxEditor, StdCtrls, Buttons,
  dxfQuickTyp, ExtCtrls, HeadPanel, dxTL, dxDBTL, cxGraphics, cxControls,
  cxLookAndFeels, cxLookAndFeelPainters, cxContainer, cxEdit, Vcl.ComCtrls,
  dxCore, cxDateUtils, cxCurrencyEdit, cxDropDownEdit, cxCalendar, cxTextEdit,
  cxMaskEdit, cxSpinEdit;

type
  TfrmDetaliiDecont = class(TForm)
    pnRest: TPanel;
    lbNrDec: TLabel;
    lbDataDec: TLabel;
    Label1: TLabel;
    edtNrDec: TcxSpinEdit;
    edtDataDec: TcxDateEdit;
    edtRep: TcxPopupEdit;
    pnBottom: TPanel;
    btnOk: TSpeedButton;
    btnCancel: TSpeedButton;
    Label2: TLabel;
    edtSumaDecont: TcxCurrencyEdit;
    Label3: TLabel;
    edtDataRegistru: TcxDateEdit;
    procedure edtNrDecChange(Sender: TObject);
    procedure edtRepKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure edtRepEnter(Sender: TObject);
    procedure btnOkClick(Sender: TObject);
    procedure btnCancelClick(Sender: TObject);
    procedure edtRepPropertiesCloseQuery(Sender: TObject;
      var CanClose: Boolean);
  private
    FCodGest: Integer;
    procedure SetCodGest(const Value: Integer);
    { Private declarations }
  public
    { Public declarations }
    property CodGest : Integer read FCodGest write SetCodGest;
  end;

implementation

uses
  ZeosDBUtile,
  dxDBCtrl, CommonDBVar,
  ContainerUnit;

{$R *.DFM}

procedure TfrmDetaliiDecont.edtNrDecChange(Sender: TObject);
begin
  btnOk.Enabled :=
    ValueHasValue(edtNrDec.EditValue) and
    ValueHasValue(edtRep.EditValue) and
    ValueHasValue(edtDataDec.EditValue) and
    (not edtDataRegistru.Visible or ValueHasValue(edtDataRegistru.EditValue));
end;

procedure TfrmDetaliiDecont.edtRepKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if Key = VK_DELETE then begin
    edtRep.Text := '';
    CodGest := 0;
  end;
end;

procedure TfrmDetaliiDecont.edtRepPropertiesCloseQuery(Sender: TObject;
  var CanClose: Boolean);
var
  lNode: TdxDBTreeListNode;
begin
  lNode := TdxDBTreeListNode(frmCasaContainer.TreeRepartitori.FocusedNode);
  if GetParentForm(edtRep.Properties.PopupControl).ModalResult = mrOk then begin
    FCodGest          := Integer(lNode.Id);
    edtRep.EditValue  := lNode.Strings[frmCasaContainer.TreeRepartitoriNUME.Index];
  end;
end;

procedure TfrmDetaliiDecont.edtRepEnter(Sender: TObject);
begin
  if Trim(edtRep.Text) ='' then
    edtRep.DroppedDown := True;
end;

procedure TfrmDetaliiDecont.btnOkClick(Sender: TObject);
begin
  ModalResult := mrOk;
end;

procedure TfrmDetaliiDecont.btnCancelClick(Sender: TObject);
begin
  ModalResult := mrCancel;
end;

procedure TfrmDetaliiDecont.SetCodGest(const Value: Integer);
var
  lNode : TdxDBTreeListNode;
begin
  FCodGest := Value;
  if Assigned(TdxPopupEdit(edtRep).PopupControl) then begin
    lNode := TdxDBTreeList(TdxPopupEdit(edtRep).PopupControl).FindNodeByKeyValue(FCodGest);
    if Assigned(lNode) then
      edtRep.Text := Trim(lNode.Strings[frmCasaContainer.TreeRepartitoriCODSECTIE.Index])+' : '+Trim(lNode.Strings[frmCasaContainer.TreeRepartitoriNUME.Index]);
  end;
end;

end.

