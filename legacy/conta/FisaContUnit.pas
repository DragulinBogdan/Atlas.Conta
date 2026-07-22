unit FisaContUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, Db, ZDataSet,  ComCtrls, ExtCtrls, cxControls,
  cxContainer, cxEdit, cxLabel, cxTextEdit, cxCurrencyEdit, cxDBEdit,
  Menus, cxLookAndFeelPainters,
  cxButtons, ZAbstractRODataset, ZAbstractDataset,  cxGraphics,
  cxDataStorage, cxDBData,
  cxGridLevel, cxClasses, cxGridCustomView, cxGridCustomTableView,
  cxGridTableView, cxGridDBTableView, cxGrid, cxMaskEdit, cxImageComboBox,
  cxCalendar, cxGridCustomPopupMenu, cxGridPopupMenu,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, cxNavigator,
  VCLTee.TeEngine, VCLTee.Series, VCLTee.TeeProcs, VCLTee.Chart, VCLTee.DBChart,
  dxDateRanges, cxDataControllerConditionalFormattingRulesManagerDialog,
  dxBarBuiltInMenu, cxGridChartView, cxGridDBChartView, dxScrollbarAnnotations;

type
  TfrmFisaCont = class(TForm)
    QryFisaCont: TZQuery;
    DTFisaCont: TDataSource;
    grRecapitulatie: TGroupBox;
    cxLabel1: TcxLabel;
    cxLabel2: TcxLabel;
    cxLabel3: TcxLabel;
    cxDBCurrencyEdit1: TcxDBCurrencyEdit;
    cxDBCurrencyEdit2: TcxDBCurrencyEdit;
    cxLabel4: TcxLabel;
    cxDBCurrencyEdit3: TcxDBCurrencyEdit;
    cxDBCurrencyEdit4: TcxDBCurrencyEdit;
    cxLabel5: TcxLabel;
    cxLabel6: TcxLabel;
    cxLabel7: TcxLabel;
    cxLabel8: TcxLabel;
    cxLabel9: TcxLabel;
    cxDBCurrencyEdit6: TcxDBCurrencyEdit;
    cxLabel10: TcxLabel;
    cxDBCurrencyEdit7: TcxDBCurrencyEdit;
    cxLabel11: TcxLabel;
    cxDBCurrencyEdit8: TcxDBCurrencyEdit;
    cxLabel12: TcxLabel;
    cxDBCurrencyEdit9: TcxDBCurrencyEdit;
    cxGridFisaCont: TcxGrid;
    GridFisaContL: TcxGridLevel;
    viewFisaCont: TcxGridDBTableView;
    viewFisaContNR_NOTA: TcxGridDBColumn;
    viewFisaContMODUL: TcxGridDBColumn;
    viewFisaContDATA_NOTA: TcxGridDBColumn;
    viewFisaContREPCont: TcxGridDBColumn;
    viewFisaContEXPLICATII: TcxGridDBColumn;
    viewFisaContCONT_CRSP: TcxGridDBColumn;
    viewFisaContDEBIT: TcxGridDBColumn;
    viewFisaContCREDIT: TcxGridDBColumn;
    viewFisaContD_C: TcxGridDBColumn;
    viewFisaContSOLD: TcxGridDBColumn;
    viewFisaContJURNAL: TcxGridDBColumn;
    viewFisaContDESCRIERE: TcxGridDBColumn;
    viewFisaContCOD_FUNCTIONAL: TcxGridDBColumn;
    viewFisaContCOD_ECONOMIC: TcxGridDBColumn;
    cxGridPopupMenu: TcxGridPopupMenu;
    viewFisaContRepCrsp: TcxGridDBColumn;
    viewFisaContUtilizator: TcxGridDBColumn;
    btnRaportare: TcxButton;
    BtnOk: TcxButton;
    cxLabel13: TcxLabel;
    cxLabel14: TcxLabel;
    cxDBCurrencyEdit5: TcxDBCurrencyEdit;
    cxLabel15: TcxLabel;
    cxDBCurrencyEdit10: TcxDBCurrencyEdit;
    viewFisaContTIP_DOCUMENT: TcxGridDBColumn;
    viewFisaContDATA_DOCUMENT: TcxGridDBColumn;
    viewFisaContNR_DOCUMENT: TcxGridDBColumn;
    nivelGrafic: TcxGridLevel;
    viewChart: TcxGridDBChartView;
    pnBottom: TPanel;
    viewChartS_I_C: TcxGridDBChartSeries;
    viewChartS_I_D: TcxGridDBChartSeries;
    viewChartR_P_C: TcxGridDBChartSeries;
    viewChartR_P_D: TcxGridDBChartSeries;
    viewChartT_S_C: TcxGridDBChartSeries;
    viewChartT_S_D: TcxGridDBChartSeries;
    viewChartS_F_C: TcxGridDBChartSeries;
    viewChartS_F_D: TcxGridDBChartSeries;
    viewChartDataGroup1: TcxGridDBChartDataGroup;
    viewChartDataGroup2: TcxGridDBChartDataGroup;
    viewChartDataGroup3: TcxGridDBChartDataGroup;
    viewChartDataGroup4: TcxGridDBChartDataGroup;
    procedure BtnOkClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure FormCreate(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
  private
    { Private declarations }
    FListaCriterii : TStringList;
    procedure ReportClick(Sender: TObject);
    procedure BuildFisaCont;
  public
    { Public declarations }
  end;

procedure ShowFisaCont(aCont, aContPlan, aDesc: String; StartD, EndD: TDateTime; CuInchidere : Integer = 0; aDataSet : TDataSet = nil);

implementation

{$R *.DFM}

uses
  ZeosDBUtile, dxCompsUtile, Variants, FormulareUnit, StrUtils, CommonDBVar, dateUnit, RapInclude;

procedure ShowFisaCont;
var
  lCodRep : Variant;
  lAnaliticMat: Variant;
  lIndex: Integer;
  lForm: TfrmFisaCont;
  lCaption : String;
  I : Integer;
  lFieldName : string;
begin

  aCont    := Trim(aCont);
  lCaption := 'Fisa Contului : '+ aCont + ' ' +aDesc+' de la data : '+FormatDateTime('dd.mm.yyyy', StartD)+' pana la : '+FormatDateTime('dd.mm.yyyy', EndD);

  lForm    := TfrmFisaCont(GetNewForm(TfrmFisaCont, lCaption, True));
  lForm.Caption   := lCaption;
  lForm.viewChart.Title.Text := 'Evolutia contului "'+aCont+'" in perioada : '+FormatDateTime('dd.mm.yyyy', StartD)+' - '+FormatDateTime('dd.mm.yyyy', EndD);
    { Luam decat radacina contului in cazul contului defalcat pe repartitor }
    { Eliminam ultima pozitie din analitic deoarece este codul de repartitor }

  lForm.QryFisaCont.DisableControls;
  try
    lForm.QryFisaCont.Close;
    lForm.QryFisaCont.Params.ParamByName('CONT').Value             := aContPlan;
    lForm.QryFisaCont.Params.ParamByName('DATA_START').Value       := StartD;
    lForm.QryFisaCont.Params.ParamByName('DATA_END').Value         := EndD;
    lForm.QryFisaCont.Params.ParamByName('CU_INCHIDERE').Value     := CuInchidere;

    if aDataSet <> nil then
      for I := 0  to lForm.FListaCriterii.Count - 1 do begin
        lFieldName := StringReplace(lForm.FListaCriterii[I], 'CRITERIU_', '', [rfIgnoreCase]);
        if (aDataSet.FindField(lFieldName) <> nil) and (lForm.QryFisaCont.Params.FindParam(lForm.FListaCriterii[I]) <> nil) then
          lForm.QryFisaCont.ParamByName(lForm.FListaCriterii[I]).Value := aDataSet.FieldByName(lFieldName).Value;
      end;

    lForm.QryFisaCont.Open;
    cxCreateMissingColumns(lForm.QryFisaCont, lForm.viewFisaCont);
  finally
    lForm.QryFisaCont.EnableControls;
  end;
  lForm.WindowState := wsMaximized;
  lForm.Show;

end;

procedure TfrmFisaCont.BtnOkClick(Sender: TObject);
begin
  Close;
end;


procedure TfrmFisaCont.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmFisaCont.ReportClick(Sender: TObject);
var
  I : Integer;
begin
  for I := 0 to QryFisaCont.Params.Count - 1 do
    SetRapParam(QryFisaCont.Params[I].Name, QryFisaCont.Params[I].Value );
  LoadReport(TMenuItem(Sender).Tag);
end;

procedure TfrmFisaCont.FormCreate(Sender: TObject);
begin
  FListaCriterii := TStringList.Create;
  PopulateReportContext(Self.ClassName, btnRaportare, ReportClick);
  FillImageCombo(viewFisaContMODUL.Properties, 'select modul, denumire from ModuleImport', 0, 1);
  BuildFisaCont;
end;

procedure TfrmFisaCont.FormDestroy(Sender: TObject);
begin
  FListaCriterii.Free;
end;

procedure TfrmFisaCont.BuildFisaCont;
var
  lProcName   : String;
  lParamName  : String;
  lDataSet    : TDataSet;
begin
  FListaCriterii.Clear;
  // rulam procedura pentru a extrage parametrii exec sp_sproc_columns 'SP_GET_FISA_CONT', 'dbo', null, null

  lProcName := 'sp_get_fisa_cont_new';
  QryFisaCont.Close;
  QryFisaCont.SQL.Text := 'exec ' + lProcName + ' @cont=:cont, @data_start=:data_start, @data_end=:data_end, @cu_inchidere=:cu_inchidere';
  lDataSet := DBNewQueryFmt('exec sp_sproc_columns %s, null, null, null', [lProcName]);
  try
    lDataSet.Open;
    lDataSet.First;
    //evitam prima pozitie
    lDataSet.Next;
    while not lDataSet.eof do begin
      lParamName := StringReplace(lDataSet.FieldByName('COLUMN_NAME').AsString, '@', '', [rfReplaceAll, rfIgnoreCase]);
      if QryFisaCont.Params.FindParam(lParamName) = nil then begin
        QryFisaCont.SQL.Text := QryFisaCont.SQL.Text + ', '+lDataSet.FieldByName('COLUMN_NAME').AsString +'=:' + lParamName;
        if UpperCase(LeftStr(lParamName, 9)) = 'CRITERIU_' then
          FListaCriterii.Add(lParamName);
      end;
      lDataSet.Next;
    end;
  finally
    lDataSet.Free;
  end;
end;

end.
