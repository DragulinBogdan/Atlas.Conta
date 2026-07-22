unit unitProiecteTCV;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxGraphics, cxLookAndFeels, cxLookAndFeelPainters, Menus,
  StdCtrls, cxButtons, Db;

type
  TfmProiecteTCV = class(TForm)
    BtnOk: TcxButton;
  private
    FDataSetDocum: TDataSet;
    FDataSetItemsi: TDataSet;
    { Private declarations }
  public
    { Public declarations }
    property DataSetDocum: TDataSet read FDataSetDocum write FDataSetDocum;
    property DataSetItemsi: TDataSet read FDataSetItemsi write FDataSetItemsi;
  end;

procedure SelectListaProiecte(ADataSetDocum, ADataSetItemsi: TDataSet);

implementation

{$R *.dfm}

procedure SelectListaProiecte(ADataSetDocum, ADataSetItemsi: TDataSet);
var
  lForm: TfmProiecteTCV;
begin
  Application.CreateForm(TfmProiecteTCV, lForm);
  try
    lForm.DataSetDocum  := ADataSetDocum;
    lForm.DataSetItemsi := ADataSetItemsi;
    lForm.ShowModal;
  finally
    lForm.Free;
  end;
end;

end.
