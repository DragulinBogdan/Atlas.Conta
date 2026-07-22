unit AcceptTransferUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, Buttons, dxCntner, dxEditor, dxExEdtr, dxEdLib, ExtCtrls,
  HeadPanel;

type
  TfrmAcceptTransfer = class(TForm)
    pnTop: THeadPanel;
    pnBottom: TPanel;
    btnAccept: TSpeedButton;
    btnReject: TSpeedButton;
    btnCancel: TSpeedButton;
    pnRest: TPanel;
    Label3: TLabel;
    Label1: TLabel;
    Label2: TLabel;
    Label4: TLabel;
    edtCasaPlecare: TdxEdit;
    edtCasaDest: TdxEdit;
    edtSuma: TdxCurrencyEdit;
    edtDataDest: TdxDateEdit;
    StyleController: TdxEditStyleController;
    procedure btnAcceptClick(Sender: TObject);
    procedure btnCancelClick(Sender: TObject);
    procedure btnRejectClick(Sender: TObject);
  private
    FPrintingBon: Boolean;
    { Private declarations }
  public
    { Public declarations }
    procedure PrintBonTransfer(State  :Boolean);
    property  PrintingBon : Boolean read FPrintingBon write FPrintingBon;
  end;


implementation

{$R *.DFM}

procedure TfrmAcceptTransfer.btnAcceptClick(Sender: TObject);
begin
  ModalResult := mrYes;
  if FPrintingBon then
    PrintBonTransfer(Boolean(TBitBtn(Sender).Tag = 1));
end;


procedure TfrmAcceptTransfer.PrintBonTransfer(State: Boolean);
begin
  //todo
end;

procedure TfrmAcceptTransfer.btnCancelClick(Sender: TObject);
begin
  ModalResult := mrCancel;
end;

procedure TfrmAcceptTransfer.btnRejectClick(Sender: TObject);
begin
  ModalResult := mrNo;
  if FPrintingBon then
    PrintBonTransfer(Boolean(TBitBtn(Sender).Tag = 1));
end;

end.
