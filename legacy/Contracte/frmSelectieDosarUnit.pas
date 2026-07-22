unit frmSelectieDosarUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxGraphics, cxControls, cxLookAndFeels, cxLookAndFeelPainters,
  cxCustomData, cxStyles, cxTL, cxMaskEdit, cxTLdxBarBuiltInMenu,
  cxContainer, cxEdit, Menus, StdCtrls, cxButtons, cxTextEdit,
  cxDropDownEdit, cxCalendar, cxInplaceContainer, cxDBTL, cxTLData, cxPC,
  DB, ZAbstractRODataset, ZAbstractDataset, ZDataset, cxCheckBox, ExtCtrls,
  cxClasses, cxGridLevel, cxGrid, cxFilter, cxData, cxDataStorage,
  cxDBData, cxCurrencyEdit, cxGridCustomPopupMenu, cxGridPopupMenu,
  cxGridCustomTableView, cxGridTableView, cxGridBandedTableView,
  cxGridDBBandedTableView, cxGridCustomView, cxGridDBTableView,
  dxBarBuiltInMenu, cxNavigator, Vcl.ComCtrls, dxCore, cxDateUtils,
  ZAbstractConnection, ZConnection,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxDateRanges,
  dxScrollbarAnnotations;

type
  TfrmSelectieDosar = class(TForm)
    DTDosare: TDataSource;
    QryDosare: TZQuery;
    pnlBottom: TPanel;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    nivelDosar: TcxGridLevel;
    gridDosare: TcxGrid;
    viewDosar: TcxGridDBBandedTableView;
    lbNumarDosar: TLabel;
    edNrDosar: TcxTextEdit;
    lbDataContract: TLabel;
    edDataDosar: TcxDateEdit;
    btnAdauga: TcxButton;
    viewDosaridDosar: TcxGridDBBandedColumn;
    viewDosarnumarDosar: TcxGridDBBandedColumn;
    viewDosardataDosar: TcxGridDBBandedColumn;
    procedure BtnOkClick(Sender: TObject);
    procedure BtnCancelClick(Sender: TObject);
    procedure btnAdaugaClick(Sender: TObject);
    procedure viewDosarCellDblClick(Sender: TcxCustomGridTableView;
      ACellViewInfo: TcxGridTableDataCellViewInfo; AButton: TMouseButton;
      AShift: TShiftState; var AHandled: Boolean);
    procedure viewDosarFocusedRecordChanged(Sender: TcxCustomGridTableView;
      APrevFocusedRecord, AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure FormCreate(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
    function GetDosarDetails(const AIdDosar: Variant): Variant; overload;
    function GetDosarDetails: Variant; overload;
    function GetNumarDosar: Variant;
    function GetDataDosar: Variant;
    procedure SetIdDosar(const AIdDosar: Variant);
    function GetIdDosar: Variant;
  end;

implementation

uses
  DateUtils, ZeosDBUtile, cxDataUtils, CommonDBVar, Math, ATSZDBUtils;

{$R *.dfm}

procedure TfrmSelectieDosar.BtnOkClick(Sender: TObject);
begin
  if Parent is TCustomForm then
    TCustomForm(Parent).ModalResult := mrOk
  else
    ModalResult := mrOk;
end;

procedure TfrmSelectieDosar.FormCreate(Sender: TObject);
begin
  ZeosDBUtile.OpenDataSets(Self);
  DBRefresh(QryDosare);
end;

procedure TfrmSelectieDosar.btnAdaugaClick(Sender: TObject);
begin
  if QryDosare.Locate('numarDosar;dataDosar', VarArrayOf([edNrDosar.EditValue, edDataDosar.EditValue]), []) then
    raise Exception.Create('Acest dosar exista deja in lista!'#13'Va rog selectati pozitia din lista!');
  QryDosare.Append;
  QryDosare['numarDosar'] := edNrDosar.EditValue;
  QryDosare['dataDosar']  := edDataDosar.EditValue;
  QryDosare.Post;
end;

procedure TfrmSelectieDosar.BtnCancelClick(Sender: TObject);
begin
  if Parent is TCustomForm then
    TCustomForm(Parent).ModalResult := mrCancel
  else
    ModalResult := mrCancel;
end;

function TfrmSelectieDosar.GetDosarDetails(
  const AIdDosar: Variant): Variant;
var
  lRecordIdx  : Integer;
  lRecord     : TcxCustomGridRecord;
begin
  viewDosar.DataController.FindRecordIndexByKey(AIDDosar);
  if (lRecordIdx > -1) and (lRecordIdx < viewDosar.ViewData.RecordCount) then begin
    lRecord := viewDosar.ViewData.Records[lRecordIdx];
    if Assigned(lRecord) and lRecord.IsData then begin
      Result := ValueSafeToStr(lRecord.Values[viewDosarnumarDosar.Index]) + '/ ' +
                ValueSafeToStr(lRecord.Values[viewDosardataDosar.Index]);
    end
    else
      Result := Null;
  end
  else
    Result := Null;
end;

function TfrmSelectieDosar.GetDataDosar: Variant;
var
  lRecord     : TcxCustomGridRecord;
begin
  lRecord := viewDosar.Controller.FocusedRecord;
  if Assigned(lRecord) and lRecord.IsData then
    Result := lRecord.Values[viewDosardataDosar.Index]
  else
    Result := Null;
end;

function TfrmSelectieDosar.GetDosarDetails: Variant;
var
  lRecord     : TcxCustomGridRecord;
begin
  lRecord := viewDosar.Controller.FocusedRecord;
  if Assigned(lRecord) and lRecord.IsData then begin
    Result := ValueSafeToStr(lRecord.Values[viewDosarnumarDosar.Index]) + '/ ' +
              ValueSafeToStr(lRecord.Values[viewDosardataDosar.Index]);
  end
  else
    Result := Null;
end;

function TfrmSelectieDosar.GetIdDosar: Variant;
begin
  if Assigned(viewDosar.Controller.FocusedRecord) and (viewDosar.Controller.FocusedRecord.IsData) then
    Result := viewDosar.Controller.FocusedRecord.Values[viewDosaridDosar.Index]
  else
    Result := Null;
end;

function TfrmSelectieDosar.GetNumarDosar: Variant;
var
  lRecord     : TcxCustomGridRecord;
begin
  lRecord := viewDosar.Controller.FocusedRecord;
  if Assigned(lRecord) and lRecord.IsData then
    Result := lRecord.Values[viewDosarnumarDosar.Index]
  else
    Result := Null;
end;

procedure TfrmSelectieDosar.SetIdDosar(const AIdDosar: Variant);
begin
  QryDosare.Refresh;
  if QryDosare.Locate('idDosar', AIdDosar, []) then begin
    edNrDosar.EditValue := QryDosare['numarDosar'];
    edDataDosar.EditValue := QryDosare['dataDosar'];
  end;
end;

procedure TfrmSelectieDosar.viewDosarCellDblClick(
  Sender: TcxCustomGridTableView; ACellViewInfo: TcxGridTableDataCellViewInfo;
  AButton: TMouseButton; AShift: TShiftState; var AHandled: Boolean);
begin
  BtnOk.Click;
end;

procedure TfrmSelectieDosar.viewDosarFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord; ANewItemRecordFocusingChanged: Boolean);
begin
  BtnOk.Enabled := (AFocusedRecord <> nil) and (AFocusedRecord.IsData);
end;

end.
