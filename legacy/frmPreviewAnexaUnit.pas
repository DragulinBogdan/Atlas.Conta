unit frmPreviewAnexaUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, DB, ZDataSet, dxExEdtr, StdCtrls, ExtCtrls, dxCntner, dxTL, dxDBCtrl,
  dxDBGrid, dxEditor, dxPSGlbl, dxPSUtl, dxBkgnd, dxWrap, dxPrnDev, cxGridExportLink,
  dxPSFillPatterns, dxPSEdgePatterns, dxPSCore, Menus, cxLookAndFeelPainters, cxButtons,
  cxGraphics, cxDataStorage, cxEdit, cxDBData, cxGridLevel, cxClasses, cxControls,
  cxGridCustomView, cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGrid,
  cxGridCustomPopupMenu, cxGridPopupMenu, cxContainer, cxTextEdit, cxMaskEdit, cxDropDownEdit,
  cxImageComboBox, dxPScxCommon, cxCalendar, ZAbstractRODataset, ZAbstractDataset, cxDrawTextUtils,
  dxPSPrVwStd, dxPSPrVwAdv,  dxPScxEditorProducers, dxPScxExtEditorProducers, dxPScxPageControlProducer,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, dxPSEngn, dxPrnPg, dxPSCompsProvider,
  ComCtrls, dxCore, cxDateUtils, cxNavigator, dxPSPDFExportCore, dxPSPDFExport, dxPSPrVwRibbon,
  dxPScxGridLnk, dxPScxGridLayoutViewLnk, dxDateRanges, dxScrollbarAnnotations,
  dxBarBuiltInMenu;

type
  TfrmAnexePreview = class(TForm)
    dtAnexa: TDataSource;
    qryAnexa: TZQuery;
    pnBottom: TPanel;
    pnTop: TPanel;
    lbStart: TLabel;
    LbEnd: TLabel;
    lbDirectii: TLabel;
    printerAnexa: TdxComponentPrinter;
    btnExport: TcxButton;
    cxGridAnexa: TcxGrid;
    gridAnexaL: TcxGridLevel;
    gridAnexa: TcxGridDBTableView;
    edAnexa: TcxImageComboBox;
    dtLstAnexe: TDataSource;
    qryLstAnexe: TZQuery;
    edStartDate: TcxDateEdit;
    edEndDate: TcxDateEdit;
    PopupExport: TPopupMenu;
    menuXLS: TMenuItem;
    menuHTML: TMenuItem;
    menuTXT: TMenuItem;
    menuXML: TMenuItem;
    BtnOk: TcxButton;
    btnPrint: TcxButton;
    btnApply: TcxButton;
    cxGridPopupMenu: TcxGridPopupMenu;
    anexaLink: TdxGridReportLink;
    procedure FormCreate(Sender: TObject);
    procedure qryAnexaAfterOpen(DataSet: TDataSet);
    procedure btnApplyClick(Sender: TObject);
    procedure btnPrintClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure BtnOkClick(Sender: TObject);
    procedure edAnexaPropertiesChange(Sender: TObject);
    procedure menuXLSClick(Sender: TObject);
    procedure pnBottomResize(Sender: TObject);
    procedure pnTopResize(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
  private
    FTitleAnexa: string;
    procedure SetTitleAnexa(const Value: string);
    procedure SetIdAnexeBilant(const Value: Integer);
    { Private declarations }
  protected
    FIdAnexe : Integer;
  public
    { Public declarations }
    property TitleAnexa: string read FTitleAnexa write SetTitleAnexa;
    property IdAnexeBilant : Integer read FIdAnexe write SetIdAnexeBilant;
  end;

procedure PreviewAnexa(const IdAnexa: Integer; const Title: String);

implementation

{$R *.dfm}

uses
  dxCompsUtile, DateUnit, FormulareUnit, DateUtils, ZeosDBUtile;

procedure PreviewAnexa(const IdAnexa: Integer; const Title: String);
var
  aPrev : TfrmAnexePreview;
begin
  aPrev := TfrmAnexePreview(GetNewForm(TfrmAnexePreview));

  with aPrev do begin
    if (IdAnexa <= 0) then begin
      if not aPrev.qryLstAnexe.IsEmpty then IdAnexeBilant := aPrev.qryLstAnexe.FieldByName('ID_ANEXE_BILANT').AsInteger;
    end
    else
      IdAnexeBilant   := IdAnexa;
    TitleAnexa := Title;
    WindowState := wsMaximized;
    Show;
  end;
end;

procedure TfrmAnexePreview.FormCreate(Sender: TObject);
begin
  DBRefresh(qryLstAnexe);
  FillImageCombo(edAnexa.Properties, qryLstAnexe, 'ID_ANEXE_BILANT', 'DENUMIRE');
  edStartDate.Date := EncodeDate(YearOf(Date), 01, 01);
  edEndDate.Date   := EncodeDate(YearOf(Date)+1, 01, 01) - 1;
  printerAnexa.CurrentLink := anexaLink;
end;

procedure TfrmAnexePreview.qryAnexaAfterOpen(DataSet: TDataSet);
var I: Integer;
begin
   gridAnexa.ClearItems;
   gridAnexa.DataController.CreateAllItems();
//   gridAnexa.DestroyColumns;
//   gridAnexa.CreateDefaultColumns(qryAnexa, gridAnexa);
   for I := 0 to gridAnexa.ColumnCount-1 do begin
     gridAnexa.Columns[I].HeaderAlignmentHorz := taCenter;
     gridAnexa.Columns[I].HeaderAlignmentVert := vaCenter;
     if SameText(gridAnexa.Columns[I].DataBinding.FieldName, 'pe_cont') then gridAnexa.Columns[I].Visible := False;
     if SameText(gridAnexa.Columns[I].DataBinding.FieldName, 'bold') then gridAnexa.Columns[I].Visible := False;
     if SameText(gridAnexa.Columns[I].DataBinding.FieldName, 'id_anexe_randuri') then gridAnexa.Columns[I].Visible := False;
     if SameText(gridAnexa.Columns[I].DataBinding.FieldName, 'Id') then gridAnexa.Columns[I].Visible := False;
     if SameText(gridAnexa.Columns[I].DataBinding.FieldName, 'Anexa') then gridAnexa.Columns[I].Visible := False;
     if (gridAnexa.Columns[I].Width > 1000) then
      if SameText(gridAnexa.Columns[I].DataBinding.FieldName, 'Denumire') then gridAnexa.Columns[I].Width := 250
      else gridAnexa.Columns[I].Width := 80
   end;
end;

procedure TfrmAnexePreview.btnApplyClick(Sender: TObject);
begin
  qryAnexa.Close;
  qryAnexa.Params[0].Value := FIdAnexe;
  qryAnexa.Params[1].Value := edStartDate.Date;
  qryAnexa.Params[2].Value := edEndDate.Date;
  qryAnexa.Params[3].Value := '';//edTableList.Text;
  qryAnexa.Open;
end;

procedure TfrmAnexePreview.btnPrintClick(Sender: TObject);
begin
  printerAnexa.Preview(True);
end;

procedure TfrmAnexePreview.SetTitleAnexa(const Value: string);
begin
  FTitleAnexa := Value;
  anexaLink.ReportTitle.Text := Value;
end;

procedure TfrmAnexePreview.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  
  Action := caFree;
end;

procedure TfrmAnexePreview.BtnOkClick(Sender: TObject);
begin
  Close;
end;

procedure TfrmAnexePreview.SetIdAnexeBilant(const Value: Integer);
begin
  FIdAnexe := Value;
  edAnexa.EditValue := FIdAnexe;
end;

procedure TfrmAnexePreview.edAnexaPropertiesChange(Sender: TObject);
begin
  FIdAnexe := edAnexa.EditValue;
end;

procedure TfrmAnexePreview.menuXLSClick(Sender: TObject);
var
  SD: TSaveDialog;
begin
  SD := TSaveDialog.Create(nil);
  try
    SD.Title := TMenuItem(Sender).Caption;
    SD.Filter := TMenuItem(Sender).Hint;
    if SD.Execute then
        case TComponent(Sender).Tag of
          0: ExportGridToExcel (SD.FileName, cxGridAnexa); //XLS
          1: ExportGridToHTML  (SD.FileName, cxGridAnexa); //HTML
          2: ExportGridToText  (SD.FileName, cxGridAnexa); //TXT
          3: ExportGridToXML   (SD.FileName, cxGridAnexa); //XML
        end;
  finally
    SD.Free;
  end;
end;


procedure TfrmAnexePreview.pnBottomResize(Sender: TObject);
begin
  BtnOk.Left := pnBottom.Width - BtnOk.Width - 5;
end;

procedure TfrmAnexePreview.pnTopResize(Sender: TObject);
begin
  btnPrint.Left := pnTop.Width - btnPrint.Width - 5;
  btnApply.Left := btnPrint.Left - btnApply.Width - 2;
end;

procedure TfrmAnexePreview.FormDestroy(Sender: TObject);
begin
  if anexaLink <> nil then
     FreeAndNil(anexaLink);
end;

end.
