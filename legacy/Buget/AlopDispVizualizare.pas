unit AlopDispVizualizare;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, ExtCtrls, cxControls,
  cxSplitter, StdCtrls, ImgList, dxDBGrid,
  dxGrClms, dxTL, dxDBCtrl, dxCntner, DB, ZDataSet, 
  cxButtons, dxDBTLCl, dxDBTL, 
  cxContainer, cxEdit, cxTextEdit, cxMaskEdit, cxDropDownEdit,
  cxImageComboBox, dxExEdtr, cxGraphics, Menus,
  cxLookAndFeelPainters, 
  cxDataStorage, cxDBData, cxGridLevel, cxClasses, cxGridCustomView,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGrid,
  cxGridCustomPopupMenu, cxGridPopupMenu, cxCurrencyEdit,
  ZAbstractRODataset, ZAbstractDataset,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, cxNavigator,
  cxCheckBox, dxDateRanges,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxBarBuiltInMenu,
  dxScrollbarAnnotations;

type
  TfrmAlopDispVizualizare = class(TForm)
    grDispzotii: TGroupBox;
    Splitter: TcxSplitter;
    grDetaliereEconomica: TGroupBox;
    pnClient: TPanel;
    ImgList: TImageList;
    DTDisp: TDataSource;
    QryDisp: TZQuery;
    DTDispDetEco: TDataSource;
    QryDispDetEco: TZQuery;
    BtnModificare: TcxButton;
    btnAnuleazaAng: TcxButton;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    btnRefresh: TcxButton;
    pnTop: TPanel;
    Label3: TLabel;
    edOperator: TcxImageComboBox;
    qryOperatori: TZQuery;
    Label1: TLabel;
    edProiect: TcxImageComboBox;
    qryProiecte: TZQuery;
    pnBottom: TPanel;
    cxGridDispozitii: TcxGrid;
    GridDispoztitii: TcxGridDBTableView;
    GridDispozitiiLevel1: TcxGridLevel;
    cxGridPopupMenu: TcxGridPopupMenu;
    cxGridDetail: TcxGrid;
    GridDetailLevel1: TcxGridLevel;
    GridDetail: TcxGridDBTableView;
    cxGridPopupMenuDetail: TcxGridPopupMenu;
    GridDispoztitiiordine: TcxGridDBColumn;
    GridDispoztitiinume_utilizator: TcxGridDBColumn;
    GridDispoztitiiid: TcxGridDBColumn;
    GridDispoztitiiparent_id: TcxGridDBColumn;
    GridDispoztitiiNumePlatitor: TcxGridDBColumn;
    GridDispoztitiiNumeBeneficiar: TcxGridDBColumn;
    GridDispoztitiiNrDispozitie: TcxGridDBColumn;
    GridDispoztitiiDataDispozitie: TcxGridDBColumn;
    GridDispoztitiiDataUtilizare: TcxGridDBColumn;
    GridDispoztitiiSuma: TcxGridDBColumn;
    GridDispoztitiiid_alop_dispozitie: TcxGridDBColumn;
    GridDispoztitiiTipDispozitie: TcxGridDBColumn;
    GridDispoztitiiCodFunctional: TcxGridDBColumn;
    GridDispoztitiiIdAnalitic: TcxGridDBColumn;
    GridDispoztitiiIdPlatitor: TcxGridDBColumn;
    GridDispoztitiiContPlatitor: TcxGridDBColumn;
    GridDispoztitiiBancaPlatitor: TcxGridDBColumn;
    GridDispoztitiiIdBeneficiar: TcxGridDBColumn;
    GridDispoztitiiContBeneficiar: TcxGridDBColumn;
    GridDispoztitiiBancaBeneficiar: TcxGridDBColumn;
    GridDispoztitiiDataOperare: TcxGridDBColumn;
    GridDispoztitiiIdUtilizator: TcxGridDBColumn;
    GridDispoztitiiDataAnulare: TcxGridDBColumn;
    GridDispoztitiiStare: TcxGridDBColumn;
    GridDispoztitiiValidat: TcxGridDBColumn;
    GridDispoztitiiCodEcran: TcxGridDBColumn;
    GridDetailden_functional: TcxGridDBColumn;
    GridDetailden_economic: TcxGridDBColumn;
    GridDetailDenumire: TcxGridDBColumn;
    GridDetailid_alop_dispozitie_defalcare: TcxGridDBColumn;
    GridDetailid_alop_dispozitie: TcxGridDBColumn;
    GridDetailCodFunctional: TcxGridDBColumn;
    GridDetailIdOiUnitati: TcxGridDBColumn;
    GridDetailCodEconomic: TcxGridDBColumn;
    GridDetailIdProiect: TcxGridDBColumn;
    GridDetailSemnDispozitie: TcxGridDBColumn;
    GridDetailDisponibilInainte: TcxGridDBColumn;
    GridDetailSumaDispozitie: TcxGridDBColumn;
    GridDetailDisponibilDupa: TcxGridDBColumn;
    GridDetailMoment: TcxGridDBColumn;
    GridDetailIdUtilizator: TcxGridDBColumn;
    GridDetailTimeImport: TcxGridDBColumn;
    GridDetailCodFunctionalEcran: TcxGridDBColumn;
    GridDetailCodEconomicEcran: TcxGridDBColumn;
    GridDetailSuma: TcxGridDBColumn;
    btnRapoarte: TcxButton;
    procedure BtnModificareClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure BtnOkClick(Sender: TObject);
    procedure BtnCancelClick(Sender: TObject);
    procedure btnPropunereClick(Sender: TObject);
    procedure btnAnuleazaAngClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure btnRefreshClick(Sender: TObject);
    procedure edOperatorPropertiesChange(Sender: TObject);
    procedure edProiectPropertiesChange(Sender: TObject);
    procedure FormShow(Sender: TObject);
    procedure pnBottomResize(Sender: TObject);
    procedure GridDispoztitiiFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure FormDestroy(Sender: TObject);
  private
    { Private declarations }
    FIdDispozitie  : Integer;
    FirstRunThis : Boolean;
    procedure ReportClick(Sender: TObject);
  public
    { Public declarations }
    procedure RefreshScreen;
    property IdDispozitie : Integer read FIdDispozitie;
  end;


function SelectieDispozitie : Integer;


implementation

uses
  ZeosDBUtile, dxCompsUtile, DateUnit, AlopDispozitie, CommonDBVar, RapInclude;

{$R *.dfm}

procedure TfrmAlopDispVizualizare.BtnModificareClick(
  Sender: TObject);
begin
 if FIdDispozitie <> -1 then begin
   ModificareDispozitie(FIdDispozitie);
   btnRefreshClick(nil);
 end;
end;

procedure TfrmAlopDispVizualizare.FormCreate(Sender: TObject);
begin
  FirstRunThis := True;
  FIdDispozitie := -1;
  RefreshScreen;
  PopulateReportContext('Rapoarte Dispozitie Bugetara', btnRapoarte, ReportClick);

  StorageReadCxView(GridDispoztitii);
  StorageReadCxView(GridDetail);
end;

procedure TfrmAlopDispVizualizare.FormDestroy(Sender: TObject);
begin
  StorageWriteCxView(GridDispoztitii);
  StorageWriteCxView(GridDetail);
end;

procedure TfrmAlopDispVizualizare.RefreshScreen;
begin
  DBRefresh([qryOperatori, qryProiecte]);
  FillImageCombo(edOperator.Properties, qryOperatori, 'ID_UTILIZATORI', 'NUMEINTREG', Null,'Toti Utilizatorii');
  FillImageCombo(edProiect.Properties, qryProiecte, 'Id', 'Denumire', Null, 'Toate proiectele/unitatile');
  if FirstRunThis then begin
    edOperator.EditValue := IdUtilizator;
    edProiect.EditValue := -1;
  end;

  QryDisp.Params.ParamByName('IdUtilizator').Value := edOperator.EditValue;
  QryDisp.Params.ParamByName('IdAnalitic').Value := edProiect.EditValue;
  DBRefresh([QryDisp, QryDispDetEco]);

  if GridDispoztitii.ItemCount > 0  then begin
     GridDispoztitii.Items[0].Focused := True;
  end;

  if FirstRunThis then begin
    cxCreateMissingColumns(QryDispDetEco, GridDetail);
    cxCreateMissingColumns(QryDisp, GridDispoztitii);
    FirstRunThis := False;
  end;
end;


procedure TfrmAlopDispVizualizare.BtnOkClick(Sender: TObject);
begin
  if fsModal in FormState then ModalResult := mrOk
  else Close;
end;

procedure TfrmAlopDispVizualizare.BtnCancelClick(Sender: TObject);
begin
  if fsModal in FormState then ModalResult := mrCancel
  else Close;
end;

procedure TfrmAlopDispVizualizare.btnPropunereClick(
  Sender: TObject);
begin
  if not (Sender is  TcxButton) then Exit;
  PrintDispozitie(FIdDispozitie, (TcxButton(Sender).Tag = 0));
end;

function SelectieDispozitie : Integer;
begin
  with TfrmAlopDispVizualizare.Create(nil) do
  try
    BtnModificare.Visible := False;
    btnAnuleazaAng.Visible := False;
    btnRapoarte.Visible := False;
    BtnOk.Visible := True;
    BtnCancel.Visible := True;
    WindowState := wsMaximized;
    ShowModal;
    if ModalResult = mrOk then Result := IdDispozitie else Result := -1;
  finally
    Free;
  end;
end;

procedure TfrmAlopDispVizualizare.btnAnuleazaAngClick(
  Sender: TObject);
var
  lNr : String;
  lData : String;
begin
 if FIdDispozitie <> -1 then begin
    lNr := QryDisp.FieldByName('NrDispozitie').AsString;
    lData := QryDisp.FieldByName('DataDispozitie').AsString;
    if (MessageDlg(Format('Doriti stergerea dispozitiei nr. : %s din data  %s ?', [
        lNr, lData]), mtConfirmation, [mbYes, mbNo], 0) in [mrNo, mrNone]) then
       Abort;
    DBExecSQlFmt('exec [spAlopAnuleazaDispozitie] %d', [FIdDispozitie]);
    DBRefresh(QryDisp);
 end;
end;

procedure TfrmAlopDispVizualizare.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmAlopDispVizualizare.btnRefreshClick(Sender: TObject);
begin
   RefreshScreen;
end;

 procedure TfrmAlopDispVizualizare.edOperatorPropertiesChange(
  Sender: TObject);
begin
  QryDisp.Params.ParamByName('IdUtilizator').Value := edOperator.EditValue;
  QryDisp.Params.ParamByName('IdAnalitic').Value := edProiect.EditValue;
  DBRefresh([QryDisp, QryDispDetEco]);
end;

procedure TfrmAlopDispVizualizare.edProiectPropertiesChange(
  Sender: TObject);
begin
  QryDisp.Params.ParamByName('IdUtilizator').Value := edOperator.EditValue;
  QryDisp.Params.ParamByName('IdAnalitic').Value := edProiect.EditValue;
  DBRefresh([QryDisp, QryDispDetEco]);
end;

procedure TfrmAlopDispVizualizare.FormShow(Sender: TObject);
begin
  //WindowState := wsMaximized;
end;

procedure TfrmAlopDispVizualizare.pnBottomResize(Sender: TObject);
begin
  BtnCancel.Left := pnBottom.Width - BtnCancel.Width - 5;
  BtnOk.Left := BtnCancel.Left - BtnOk.Width - 2;
  btnRapoarte.Left := BtnOk.Left - btnRapoarte.Width - 2;
end;

procedure TfrmAlopDispVizualizare.GridDispoztitiiFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
  FIdDispozitie := -1;
  if fsCreating in FormState then Exit;
  QryDispDetEco.Close;
  BtnOk.Enabled := (FIdDispozitie <> -1);
  if not Assigned(AFocusedRecord) then Exit;
 // GridDispoztitii.Invalidate;
   FIdDispozitie := GetInteger(AFocusedRecord, GridDispoztitiiid_alop_dispozitie.Index);
  // ShowMessage('FOCUSED RECORD -> id: ' + IntToStr(FIdDispozitie) +
 // ' | Suma: ' + QryDisp.FieldByName('Suma').AsString);

  BtnOk.Enabled := (FIdDispozitie <> -1);
  QryDispDetEco.Open;
end;

procedure TfrmAlopDispVizualizare.ReportClick(Sender: TObject);
begin
  SetRapParam('id_alop_dispozitie', FIdDispozitie);
  LoadReport(TMenuItem(Sender).Tag);
end;

end.
