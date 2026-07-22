unit SaveTemplateUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, Buttons, cxGraphics, cxLookAndFeelPainters,
  Menus, cxButtons,  cxControls, cxContainer, cxEdit, cxTextEdit,
  cxMaskEdit, cxDropDownEdit, cxMemo,
  cxLookAndFeels;

type
  TfrmSaveTemplate = class(TForm)
    LbNumeTempl: TLabel;
    LbDescrTempl: TLabel;
    LbShortCut: TLabel;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    edName: TcxTextEdit;
    edShortCut: TcxComboBox;
    edHint: TcxMemo;
    procedure FormCreate(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

implementation


{$R *.DFM}

procedure TfrmSaveTemplate.FormCreate(Sender: TObject);
var I: TShortCut;
    S: String;
begin
  { Populam Short-cutirile }
  for I := Low(TShortCut) to High(TShortCut) do begin
    S := Trim(ShortCutToText(I));
    if S > '' then edShortCut.Properties.Items.Add(S);
  end;
end;

end.
  
