unit Unit11;

interface

uses
  Winapi.Windows, Winapi.Messages, System.SysUtils, System.Variants, System.Classes, Vcl.Graphics,
  Vcl.Controls, Vcl.Forms, Vcl.Dialogs, ZAbstractConnection, ZConnection,
  cxGraphics, cxControls, cxLookAndFeels, cxLookAndFeelPainters, cxStyles,
  cxCustomData, cxFilter, cxData, cxDataStorage, cxEdit, cxNavigator,
  dxDateRanges, dxScrollbarAnnotations, Data.DB, cxDBData, cxGridCustomView,
  cxGridCustomTableView, cxGridTableView, cxGridBandedTableView,
  cxGridDBBandedTableView, ZAbstractRODataset, ZAbstractDataset, ZDataset,
  cxClasses, cxGridLevel, cxGrid;

type
  TForm1 = class(TForm)
    zConnection: TZConnection;
    nivelAngajament: TcxGridLevel;
    gridAngajamente: TcxGrid;
    qryAngajamente: TZQuery;
    qryDefalcare: TZQuery;
    dtAngajamente: TDataSource;
    dtDefalcare: TDataSource;
    nivelDefalcare: TcxGridLevel;
    viewAngajamente: TcxGridDBBandedTableView;
    viewDetalii: TcxGridDBBandedTableView;
    qryAngajamentenr_contract: TStringField;
    qryAngajamenteid_alop_angajamente: TIntegerField;
    qryAngajamentenumar: TStringField;
    qryAngajamentedata_emitere: TDateTimeField;
    qryAngajamenteid_lst_repartitori: TIntegerField;
    qryAngajamentecod_functional: TStringField;
    qryAngajamentecodurieconomice: TStringField;
    qryAngajamentedisponibil: TFloatField;
    qryAngajamenteramas_de_angajat: TFloatField;
    qryAngajamenteNUME_REPARTITOR: TStringField;
    viewAngajamentenr_contract: TcxGridDBBandedColumn;
    viewAngajamenteid_alop_angajamente: TcxGridDBBandedColumn;
    viewAngajamentenumar: TcxGridDBBandedColumn;
    viewAngajamentedata_emitere: TcxGridDBBandedColumn;
    viewAngajamenteid_lst_repartitori: TcxGridDBBandedColumn;
    viewAngajamentecod_functional: TcxGridDBBandedColumn;
    viewAngajamentecodurieconomice: TcxGridDBBandedColumn;
    viewAngajamentedisponibil: TcxGridDBBandedColumn;
    viewAngajamenteramas_de_angajat: TcxGridDBBandedColumn;
    viewAngajamenteNUME_REPARTITOR: TcxGridDBBandedColumn;
    qryDefalcareID_ALOP_ANGAJAMENTE_DEFALCARE: TIntegerField;
    qryDefalcareID_ALOP_ANGAJAMENTE: TIntegerField;
    qryDefalcareCOD_ECONOMIC: TStringField;
    qryDefalcareAPROBATE: TFloatField;
    qryDefalcareTOTAL_ANGAJATE: TFloatField;
    qryDefalcareDISPONIBIL: TFloatField;
    qryDefalcareANGAJAT_VALUTA: TFloatField;
    qryDefalcareANGAJAT: TFloatField;
    qryDefalcareRAMAS_DE_ANGAJAT: TFloatField;
    qryDefalcarenumar: TStringField;
    qryDefalcarecod_functional: TStringField;
    qryDefalcaredata_emitere: TDateTimeField;
    qryDefalcareid_lst_repartitori: TIntegerField;
    qryDefalcareNUME_REPARTITOR: TStringField;
    viewDetaliiID_ALOP_ANGAJAMENTE_DEFALCARE: TcxGridDBBandedColumn;
    viewDetaliiID_ALOP_ANGAJAMENTE: TcxGridDBBandedColumn;
    viewDetaliiCOD_ECONOMIC: TcxGridDBBandedColumn;
    viewDetaliiAPROBATE: TcxGridDBBandedColumn;
    viewDetaliiTOTAL_ANGAJATE: TcxGridDBBandedColumn;
    viewDetaliiDISPONIBIL: TcxGridDBBandedColumn;
    viewDetaliiANGAJAT_VALUTA: TcxGridDBBandedColumn;
    viewDetaliiANGAJAT: TcxGridDBBandedColumn;
    viewDetaliiRAMAS_DE_ANGAJAT: TcxGridDBBandedColumn;
    viewDetaliinumar: TcxGridDBBandedColumn;
    viewDetaliicod_functional: TcxGridDBBandedColumn;
    viewDetaliidata_emitere: TcxGridDBBandedColumn;
    viewDetaliiid_lst_repartitori: TcxGridDBBandedColumn;
    viewDetaliiNUME_REPARTITOR: TcxGridDBBandedColumn;
    procedure viewAngajamenteFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
      procedure FormCreate(Sender: TObject);
     procedure viewAngajamenteCellClick(Sender: TcxCustomGridTableView;
  ACellViewInfo: TcxGridTableDataCellViewInfo; AButton: TMouseButton;
  AShift: TShiftState; var AHandled: Boolean);

  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  Form1: TForm1;

implementation

{$R *.dfm}

procedure TForm1.viewAngajamenteFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
var
  i: Integer;
  MasterRow: TcxGridMasterDataRow;
  View: TcxGridDBBandedTableView;
begin
  if (Sender = nil) or not (Sender is TcxGridDBBandedTableView) then Exit;

  View := TcxGridDBBandedTableView(Sender);

  if (AFocusedRecord = nil) or (View.Controller.FocusedRow = nil) then Exit;

  for i := 0 to View.ViewData.RecordCount - 1 do
  begin
    if View.ViewData.Records[i] is TcxGridMasterDataRow then
    begin
      MasterRow := TcxGridMasterDataRow(View.ViewData.Records[i]);

      if MasterRow.Expanded and
         (View.Controller.FocusedRow is TcxGridMasterDataRow) and
         (MasterRow <> TcxGridMasterDataRow(View.Controller.FocusedRow)) then
        MasterRow.Expanded := False;
    end;
  end;
end;


procedure TForm1.viewAngajamenteCellClick(Sender: TcxCustomGridTableView;
  ACellViewInfo: TcxGridTableDataCellViewInfo; AButton: TMouseButton;
  AShift: TShiftState; var AHandled: Boolean);
//var
//  View: TcxGridDBBandedTableView;
//  ClickedRow, Row: TcxGridMasterDataRow;
//  I: Integer;
begin
//  if AButton <> mbLeft then Exit;
//
//  View := TcxGridDBBandedTableView(Sender);
//
//  if (ACellViewInfo = nil) or
//     (ACellViewInfo.GridRecord = nil) or
//     not (ACellViewInfo.GridRecord is TcxGridMasterDataRow) then Exit;
//
//  ClickedRow := TcxGridMasterDataRow(ACellViewInfo.GridRecord);
//
//
//  for I := 0 to View.ViewData.RecordCount - 1 do
//  begin
//    if (View.ViewData.Records[I] is TcxGridMasterDataRow) and
//       (View.ViewData.Records[I] <> ClickedRow) then
//    begin
//      Row := TcxGridMasterDataRow(View.ViewData.Records[I]);
//      Row.Expanded := False;
//    end;
//  end;
//
//
//  if not qryDefalcare.Active or qryDefalcare.IsEmpty then
//  begin
//    ClickedRow.Expanded := False;
//    Exit;
//  end;
//
//
//  ClickedRow.Expanded := not ClickedRow.Expanded;
//  if ClickedRow.Expanded then
//    viewDetalii.DataController.Refresh;
end;




procedure TForm1.FormCreate(Sender: TObject);
begin
 viewAngajamente.OnCellClick := viewAngajamenteCellClick;
end;

end.
