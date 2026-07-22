unit gestLabels;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, ExtCtrls, cxGraphics, 
  cxDataStorage, cxEdit, DB, cxDBData, cxGridLevel, cxClasses, cxControls,
  cxGridCustomView, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGrid, ZDataSet, cxImage, cxButtonEdit,
  ZAbstractRODataset, ZAbstractDataset,
  cxLookAndFeelPainters,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, cxNavigator;

type
  TfrmGestLabels = class(TForm)
    pnForm: TPanel;
    GridLabel: TcxGridDBTableView;
    GridLabelV: TcxGridLevel;
    cxGrid: TcxGrid;
    DTLabels: TDataSource;
    qryLabels: TZQuery;
    GridLabelid_gest_labels: TcxGridDBColumn;
    GridLabeldenumire: TcxGridDBColumn;
    GridLabelsizeWidth: TcxGridDBColumn;
    GridLabelsizeHeight: TcxGridDBColumn;
    GridLabelthumb: TcxGridDBColumn;
    GridLabelreport: TcxGridDBColumn;
    procedure FormCreate(Sender: TObject);
    procedure GridLabelGetCellHeight(Sender: TcxCustomGridTableView;
      ARecord: TcxCustomGridRecord; AItem: TcxCustomGridTableItem;
      ACellViewInfo: TcxGridTableDataCellViewInfo; var AHeight: Integer);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
  private
    { Private declarations }
    FCurrentZoom : Integer;
  public
    { Public declarations }
  end;

var
  frmGestLabels: TfrmGestLabels;

implementation

uses
  dateUnit, ZeosDBUtile;

{$R *.dfm}

procedure TfrmGestLabels.FormCreate(Sender: TObject);
begin
  DBRefresh(qryLabels);
  FCurrentZoom := 25;
end;

procedure TfrmGestLabels.GridLabelGetCellHeight(
  Sender: TcxCustomGridTableView; ARecord: TcxCustomGridRecord;
  AItem: TcxCustomGridTableItem;
  ACellViewInfo: TcxGridTableDataCellViewInfo; var AHeight: Integer);
var
  AEditValue: Variant;
  APicture: TPicture;
begin
  if AItem <> GridLabelthumb then
  begin
    AHeight := 0;
    Exit;
  end;
  AEditValue := ARecord.Values[GridLabelthumb.Index];
  if VarIsStr(AEditValue) or VarIsArray(AEditValue) then
  begin
    APicture := TPicture.Create;
    try
      LoadPicture(APicture,
        TcxImageProperties(GridLabelthumb.Properties).GraphicClass, AEditValue);
      AHeight := APicture.Height;
      AHeight := AHeight * FCurrentZoom div 100;
    finally
      APicture.Free;
    end;
  end;
end;

procedure TfrmGestLabels.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  DBPost(qryLabels);
end;

end.
