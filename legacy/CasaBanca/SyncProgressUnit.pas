unit SyncProgressUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  dxfProgressBar;

type
  TfrmProgress = class(TForm)
    GlobalLoadProgress: TdxfProgressBar;
  private
    { Private declarations }
  public
    { Public declarations }
  end;

implementation

{$R *.DFM}

end.
