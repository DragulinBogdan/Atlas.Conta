unit AlopOrdVizualizare;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxControls, cxContainer,
  cxEdit, cxGroupBox, DB, ZDataSet, cxLookAndFeelPainters,
  ZAbstractRODataset, ZAbstractDataset,
  cxGraphics,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, cxDataStorage,
  cxDBData, cxGridLevel, cxClasses, cxGridCustomView,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGrid,
  cxGridBandedTableView, cxGridDBBandedTableView, cxNavigator, dxDateRanges,
  dxScrollbarAnnotations;

type
  TfrmAlopOrdVizualizare = class(TForm)
    gbBox: TcxGroupBox;
    qryListaOrd: TZQuery;
    DTListaOrd: TDataSource;
    nivelDocumenteLichidare: TcxGridLevel;
    gridDocumenteLichidare: TcxGrid;
    viewDocumenteLichidare: TcxGridDBBandedTableView;
  private
    FIdOrdonantare: Integer;
    { Private declarations }
  public
    { Public declarations }
    property IdOrdonantare : Integer read FIdOrdonantare;
  end;

var
  frmAlopOrdVizualizare: TfrmAlopOrdVizualizare;

implementation

uses
  dateUnit;

{$R *.dfm}

end.
