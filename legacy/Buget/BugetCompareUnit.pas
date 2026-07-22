unit BugetCompareUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, ExtCtrls, StdCtrls;

type
  TfrmBugetComparare = class(TForm)
    Label1: TLabel;
    Bevel1: TBevel;
    procedure FormCreate(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
    procedure Appear;

  end;


implementation

{$R *.dfm}

procedure TfrmBugetComparare.Appear;
var
  I : Integer;
begin
  try
    for i:=3 to 23 do begin
       alphablendvalue:=i*10;
       sleep(8);
    end;
  except
  end;

end;


procedure TfrmBugetComparare.FormCreate(Sender: TObject);
begin
  AlphaBlendValue := 230;
  AlphaBlend := True;
end;

end.
