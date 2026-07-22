unit RapImplicit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms, Dialogs, ExtCtrls,
  cxGraphics, cxDataStorage, cxEdit, DB, cxDBData, cxGridLevel, cxClasses, cxControls,
  cxGridCustomView, cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGrid,
  ZDataSet, cxImageComboBox, StdCtrls, cxContainer, cxTextEdit, cxDBEdit, cxMaskEdit,
  cxButtonEdit, cxDropDownEdit, Menus, cxLookAndFeelPainters, cxButtons, ZAbstractRODataset,
  ZAbstractDataset, cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData,
  cxNavigator, dxDateRanges,
  cxDataControllerConditionalFormattingRulesManagerDialog;

type
  TfrmRapImplicit = class(TForm)
    pnBottom: TPanel;
    GridRap: TcxGridDBTableView;
    GridRapL: TcxGridLevel;
    cxGridRap: TcxGrid;
    DTRapImplicit: TDataSource;
    qryRapImplicit: TZQuery;
    GridRapID_RAPOARTE_ASOCIERE: TcxGridDBColumn;
    GridRapNUME_RAPORT: TcxGridDBColumn;
    GridRapITEM_ID: TcxGridDBColumn;
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    edtIdentificator: TcxDBTextEdit;
    edtDenumire: TcxDBTextEdit;
    edtRaport: TcxDBImageComboBox;
    BtnOk: TcxButton;
    procedure FormCreate(Sender: TObject);
    procedure edtRaportPropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
  private
    { Private declarations }
  public
    { Public declarations }
  end;


procedure EditareRapoarteImplicite;


implementation

uses
  ZeosDBUtile, dxCompsUtile, ChouseReportUnit, CommonDBVar;

{$R *.dfm}

procedure EditareRapoarteImplicite;
var
  lfrmRapImplicit : TfrmRapImplicit;
begin
  lfrmRapImplicit := TfrmRapImplicit.Create(nil);
  with lfrmRapImplicit do
    try
      ShowModal;
    finally
      Free;
    end;
end;

procedure TfrmRapImplicit.FormCreate(Sender: TObject);
begin
  qryRapImplicit.Connection := DBConnection;
  DBRefresh(qryRapImplicit);
  FillImageCombo(edtRaport.Properties, 'SELECT ITEM_ID, ITEM_NAME FROM REPORTS_ITEM', 'ITEM_ID', 'ITEM_NAME', True, 'Neasignat');
  TcxImageComboBoxProperties(GridRapITEM_ID.Properties).Items.Assign(edtRaport.Properties.Items);
end;

procedure TfrmRapImplicit.edtRaportPropertiesButtonClick(Sender: TObject;
  AButtonIndex: Integer);
var
  lfmSelect : TfrmChouseReport;
  lRepList  : String;
  lIdRep    : Integer;
begin
  lfmSelect := TFrmChouseReport.Create(Self);
  try
    lfmSelect.MultiSelect := False;
    lfmSelect.QryReports.Connection := DBConnection;
    lfmSelect.QryReports.Open;
    lRepList := qryRapImplicit.FieldByName('ITEM_ID').AsString;
    lfmSelect.SetReportLists(lRepList);
    if lfmSelect.ShowModal = mrOk then begin
      lRepList := lfmSelect.GetReportLists;
      if Trim(lRepList)<>'' then begin
        lIdRep := lfmSelect.CurentReportId;
      end
      else lIdRep := -1;
      DBSetFieldValue(qryRapImplicit, 'ITEM_ID', lIdRep);
    end;
  finally
    lfmSelect.Free;
  end;
end;

end.
