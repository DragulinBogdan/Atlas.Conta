unit AnexeParametrii;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxGraphics, 
  cxDataStorage, cxEdit, DB, cxDBData, cxImageComboBox, cxInplaceContainer,
  cxVGrid, cxGridCustomTableView, cxGridTableView, cxGridDBTableView,
  ZDataSet, cxGridLevel, cxClasses, cxControls, cxGridCustomView, cxGrid,
  Menus, cxLookAndFeelPainters, StdCtrls, cxButtons, cxSplitter, cxDBVGrid,
  ExtCtrls, cxCheckBox, cxCurrencyEdit, Buttons,
  ZAbstractRODataset, ZAbstractDataset,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, cxNavigator;

type
  TfrmIntretinAnexeParametrii = class(TForm)
    qryParam: TZQuery;
    DTParam: TDataSource;
    Panel1: TPanel;
    Panel2: TPanel;
    grd: TcxGrid;
    tvParam: TcxGridDBTableView;
    tvParamParamName: TcxGridDBColumn;
    tvParamCaption: TcxGridDBColumn;
    tvParamParamType: TcxGridDBColumn;
    tvParamDescription: TcxGridDBColumn;
    grdLevel1: TcxGridLevel;
    vgrid: TcxDBVerticalGrid;
    vgridSQLText: TcxDBEditorRow;
    vgridValueList: TcxDBEditorRow;
    vgridDescriptionList: TcxDBEditorRow;
    vgridSourceTable: TcxDBEditorRow;
    vgridKeyField: TcxDBEditorRow;
    vgridIDField: TcxDBEditorRow;
    vgridParentField: TcxDBEditorRow;
    vgridDisplayField: TcxDBEditorRow;
    vgridFieldList: TcxDBEditorRow;
    grdCols: TcxGrid;
    cxGridDBTableView1: TcxGridDBTableView;
    cxGridLevel1: TcxGridLevel;
    cxSplitter1: TcxSplitter;
    spCols: TcxSplitter;
    qryPopupCols: TZQuery;
    DTPopupCols: TDataSource;
    cxGridDBTableView1ColName: TcxGridDBColumn;
    cxGridDBTableView1ColWidth: TcxGridDBColumn;
    cxButton1: TcxButton;
    vgridColumnAutoWidth: TcxDBEditorRow;
    vgridConltrolWidth: TcxDBEditorRow;
    Panel3: TPanel;
    SpeedButton1: TSpeedButton;
    SpeedButton2: TSpeedButton;
    procedure cxButton1Click(Sender: TObject);
    procedure tvParamParamTypePropertiesCloseUp(Sender: TObject);
    procedure tvParamFocusedRecordChanged(Sender: TcxCustomGridTableView;
      APrevFocusedRecord, AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure SpeedButton1Click(Sender: TObject);
    procedure SpeedButton2Click(Sender: TObject);
    procedure FormShow(Sender: TObject);
    procedure qryPopupColsNewRecord(DataSet: TDataSet);
  private
    { Private declarations }
    procedure RefreshVGrid;
  public
    { Public declarations }
  end;


implementation
uses AnexeParametriiLista, dateUnit;
{$R *.dfm}
//------------------------------------------------------------------------------
procedure TfrmIntretinAnexeParametrii.cxButton1Click(Sender: TObject);
begin
  Close;
end;
//------------------------------------------------------------------------------
procedure TfrmIntretinAnexeParametrii.RefreshVGrid;
var
  PType: TAnexeParameterType;
begin
  PType := TAnexeParameterType(qryParam.FieldByName('ParamType').AsInteger);
  vgridSQLText.Visible := False;
  vgridValueList.Visible :=  PType in [ptImageComboBox];
  vgridDescriptionList.Visible := PType in [ptImageComboBox, ptComboBox];
  vgridSourceTable.Visible := PType in [ptLookupComboBox, ptPopupEdit, ptComboBox, ptImageComboBox];
  vgridKeyField.Visible := PType in [ptLookupComboBox, ptPopupEdit, ptImageComboBox];
  vgridIDField.Visible := PType in [ptPopupEdit];
  vgridParentField.Visible := PType in [ptPopupEdit];
  vgridDisplayField.Visible := PType in [ptPopupEdit];
  vgridFieldList.Visible := PType in [ptComboBox, ptImageComboBox, ptLookupComboBox, ptPopupEdit];
  vgridColumnAutoWidth.Visible := PType in [ptPopupEdit];
  vgridConltrolWidth.Visible := PType in [ptPopupEdit];

  grdCols.Visible := PType in [ptPopupEdit];
  spCols.Visible := grdCols.Visible;
end;
//------------------------------------------------------------------------------
procedure TfrmIntretinAnexeParametrii.tvParamParamTypePropertiesCloseUp(Sender: TObject);
begin
  RefreshVGrid;
end;
//------------------------------------------------------------------------------
procedure TfrmIntretinAnexeParametrii.tvParamFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
  RefreshVGrid;
end;
//------------------------------------------------------------------------------
procedure TfrmIntretinAnexeParametrii.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  if qryParam.State in dsEditModes then qryParam.Post;
  if qryPopupCols.State in dsEditModes then qryPopupCols.Post;
end;
//------------------------------------------------------------------------------
procedure TfrmIntretinAnexeParametrii.SpeedButton1Click(Sender: TObject);
begin
  qryParam.Append;
end;
//------------------------------------------------------------------------------
procedure TfrmIntretinAnexeParametrii.SpeedButton2Click(Sender: TObject);
begin
  if MessageDlg('Stergeti parametrul?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then
  begin
    with TZQuery.Create(nil) do
      try
        Connection := qryParam.Connection;
        SQL.Text := 'exec spAnexeParametriiDeleteCol '+qryParam.FieldByName('ID_ANEXE_PARAMETRII').AsString;
        ExecSQL;
      finally
        Free;
      end;
    qryParam.Delete;
  end;
end;
//------------------------------------------------------------------------------
procedure TfrmIntretinAnexeParametrii.FormShow(Sender: TObject);
begin
  qryParam.Open;
  qryPopupCols.Open;
end;
//------------------------------------------------------------------------------
procedure TfrmIntretinAnexeParametrii.qryPopupColsNewRecord(DataSet: TDataSet);
begin
  qryPopupCols.FieldByName('ID_ANEXE_PARAMETRII').AsInteger := qryParam.FieldByName('ID_ANEXE_PARAMETRII').AsInteger;
end;
//------------------------------------------------------------------------------
end.
