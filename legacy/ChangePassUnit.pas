unit ChangePassUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, ExtCtrls, Menus, cxLookAndFeelPainters, cxButtons,
  cxGraphics,
  cxLookAndFeels, cxControls, cxContainer, cxEdit, cxTextEdit;

type
  TRecalcGuid = procedure (FOldPassword, FNewPassword : String);
  
  TfrmChangePass = class(TForm)
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    Label4: TLabel;
    LbUserName: TLabel;
    Image1: TImage;
    BtnOk: TcxButton;
    btnCancel: TcxButton;
    edParolaVeche: TcxTextEdit;
    edParolaNoua: TcxTextEdit;
    edConfirmare: TcxTextEdit;
    procedure BtnOkClick(Sender: TObject);
  private
    { Private declarations }
    FOldPassword: String;
  public
    function GetNewPassword(GuidEvent : TRecalcGuid = nil): String;
    { Public declarations }
  end;

function ChangePassword(OldPassWord: String; UserName: String; var NewPassword: String; GuidEvent : TRecalcGuid = nil; const DefParola : String = ''): Boolean;

implementation

{$R *.DFM}

uses MD5, CommonDBVar;

function ChangePassword(OldPassword: String; UserName: String; var NewPassword: String; GuidEvent : TRecalcGuid = nil; const DefParola : String = ''): Boolean;
begin
  with TfrmChangePass.Create(Application) do
    try
       FOldPassword := OldPassword;
       LbUserName.Caption := UserName;
       edParolaVeche.Enabled := True;
       if DefParola <> '' then begin
          edParolaVeche.Text := DefParola;
          edParolaVeche.Enabled := False;
       end;

       Result := ShowModal = mrOk;
       if Result then
          NewPassword := GetNewPassword(GuidEvent);
    finally
       Free;
    end;
end;

{ TfrmChangePass }
function TfrmChangePass.GetNewPassword(GuidEvent : TRecalcGuid): String;
begin
  Result := MD5Print(MD5String(edParolaNoua.Text));
  if Assigned(GuidEvent) then GuidEvent(edParolaVeche.Text, edParolaNoua.Text);
end;

procedure TfrmChangePass.BtnOkClick(Sender: TObject);
begin
  if (AnsiCompareText(edParolaVeche.Text, 'Contabilitate') <> 0) and
     (AnsiCompareText(MD5Print(MD5String(edParolaVeche.Text)), FOldPassword) <> 0) then
     raise EContaHandledError.Create('Parola veche nu corespunde !'#13#10'Va rugam reintroduceti !');
  if AnsiCompareText(edParolaNoua.Text, edConfirmare.Text) <> 0 then
     raise EContaHandledError.Create('Parola nu este confimata !'#13#10'Va rugam reintroduceti !');
  ModalResult := mrOk;
end;

end.
