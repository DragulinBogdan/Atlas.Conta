unit frmMFinanteQuestionUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxGraphics, cxLookAndFeels, cxLookAndFeelPainters, Menus,
  cxControls, cxContainer, cxEdit, cxTextEdit, StdCtrls, cxButtons,
  ExtCtrls, cxImage;

type
  TfrmMFinanteQuestion = class(TForm)
    btnOk: TcxButton;
    edCaptcha: TcxTextEdit;
    edImage: TcxImage;
  private
    { Private declarations }
  public
    { Public declarations }
  end;

implementation



{$R *.dfm}

end.
