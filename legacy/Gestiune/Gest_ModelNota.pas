unit Gest_ModelNota;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxGraphics, 
  cxDataStorage, cxEdit, DB, cxDBData, cxGridLevel, cxClasses, cxControls,
  cxGridCustomView, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGrid,
  cxLookAndFeelPainters,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData;

type
  TfrmGestModelNota = class(TForm)
    GridModel: TcxGridDBTableView;
    cxGridModelL: TcxGridLevel;
    cxGridModel: TcxGrid;
  private
    { Private declarations }
  public
    { Public declarations }
  end;


implementation

{$R *.dfm}

end.
