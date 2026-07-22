unit frmMutareRepUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, ExtCtrls, cxGraphics, cxControls, cxLookAndFeels,
  cxLookAndFeelPainters, cxStyles, cxCustomData, cxFilter, cxData,
  cxDataStorage, cxEdit, DB, cxDBData, ZAbstractRODataset,
  ZAbstractDataset, ZDataset, dxmdaset, cxGridLevel, cxClasses,
  cxGridCustomView, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGrid, cxNavigator;

type
  TfrmMutareRep = class(TForm)
    pnBottom: TPanel;
    GridDate: TcxGridDBTableView;
    GridDateLevel1: TcxGridLevel;
    cxGridDate: TcxGrid;
    DSDate: TDataSource;
    MemDate: TdxMemData;
    qryDate: TZQuery;
    GridDateidRepartitor: TcxGridDBColumn;
    GridDateModul: TcxGridDBColumn;
    GridDateDataTable: TcxGridDBColumn;
    GridDateFieldName: TcxGridDBColumn;
    GridDateKeyField: TcxGridDBColumn;
    GridDateKeyValue: TcxGridDBColumn;
    GridDatedocument: TcxGridDBColumn;
    GridDateexplicatie: TcxGridDBColumn;
    procedure qryDateAfterOpen(DataSet: TDataSet);
  private
    FIdRepartitor: Integer;
    procedure SetIdRepartitor(const Value: Integer);
    { Private declarations }
  public
    { Public declarations }
    property IdRepartitor : Integer read FIdRepartitor write SetIdRepartitor;
  end;

procedure MutareRepartitor(aIdRepartitor : Integer);

implementation

uses
  dateUnit;

{$R *.dfm}

procedure MutareRepartitor(aIdRepartitor : Integer);
begin
  
end;

procedure TfrmMutareRep.qryDateAfterOpen(DataSet: TDataSet);
begin
  MemDate.Active := False;
  MemDate.Active := True;
  MemDate.LoadFromDataSet(qryDate);
  qryDate.Close;
end;

procedure TfrmMutareRep.SetIdRepartitor(const Value: Integer);
begin
  FIdRepartitor := Value;
  qryDate.Close;
  qryDate.ParamByName('idRep').Value := FIdRepartitor;
  qryDate.Open;
end;

end.
