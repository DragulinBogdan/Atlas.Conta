unit SchimbareParolaUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, StdCtrls, Buttons, ExtCtrls, Menus, cxLookAndFeelPainters,
  cxButtons, cxControls, cxContainer,
  cxEdit, cxTextEdit;

type
  TfrmSchimbareParola = class(TForm)
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    Label4: TLabel;
    Image1: TImage;
    btnOk: TcxButton;
    btnCancel: TcxButton;
    edNumeUtilizator: TcxTextEdit;
    edParolaVeche: TcxTextEdit;
    edParolaNoua: TcxTextEdit;
    edConfirmare: TcxTextEdit;
    procedure btnOkClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

implementation

{$R *.dfm}

uses MD5, CommonDBVar, DateUnit;

procedure TfrmSchimbareParola.btnOkClick(Sender: TObject);
var
  lConfPwd : String;
  lLastPwd : String;
  lNewPwd  : String;
begin
  if edParolaNoua.Text <> edConfirmare.Text then
    raise Exception.Create('Parola confirmata nu corespunde !');
  lLastPwd := MD5Print(MD5String(edParolaVeche.Text));
  with GetTmpADOQuery do
    try
      Sql.Add('select parola from utilizatori where id_utilizatori = '+IntToStr(IdUtilizator));
      Open;
      lConfPwd := Fields[0].AsString;
    finally
      Free;
    end;
  if (not SameText(lLastPwd, lConfPwd)) and
     (not SameText(edParolaVeche.Text, 'Contabilitate')) then
    raise Exception.Create('Parola veche nu corespunde !');
  lNewPwd := MD5Print(MD5String(edParolaNoua.Text));
  with GetTmpADOQuery do
    try
      Sql.Add('update utilizatori set parola = '+QuotedStr(lNewPwd)+' where id_utilizatori = '+IntToStr(IdUtilizator));
      ExecSql;
    finally
      Free;
    end;
  Self.ModalResult := mrOk;
end;

procedure TfrmSchimbareParola.FormCreate(Sender: TObject);
begin
  edNumeUtilizator.Text := NumeLoginComplet;
end;

end.
