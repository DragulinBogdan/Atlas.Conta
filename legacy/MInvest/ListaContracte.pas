unit ListaContracte;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, Menus, StdCtrls, ExtCtrls, DB, cxLookAndFeelPainters, cxStyles,
  cxCustomData, cxGraphics, cxFilter, cxData, cxDataStorage, cxEdit,
  cxDBData, cxSplitter, cxPC, cxGridLevel, cxGridCustomTableView,
  cxGridTableView, cxGridDBTableView, cxClasses, cxControls,
  cxGridCustomView, cxGrid, cxButtons, cxPropertiesStore, Grids, DBGrids,
   cxDBLookupComboBox, cxCheckBox, cxCurrencyEdit, cxTextEdit,
  cxCalc, cxDropDownEdit, cxMaskEdit, cxLookupEdit, cxDBLookupEdit,
  cxLabel, cxContainer, Buttons, cxCalendar, cxLookAndFeels,
  ZAbstractRODataset, ZAbstractDataset, ZDataset, cxNavigator, dxBarBuiltInMenu,
  Vcl.ComCtrls, dxCore, cxDateUtils,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxDateRanges,
  dxScrollbarAnnotations;

type
  TfrmListaContracte = class(TForm)
    pnlBottom: TPanel;
    btnAdd: TcxButton;
    btnDelete: TcxButton;
    btnClose: TcxButton;
    btnModify: TcxButton;
    pnlMain: TPanel;
    grdContracte: TcxGridDBTableView;
    nivelContracte: TcxGridLevel;
    gridContracte: TcxGrid;
    pcDetalii: TcxPageControl;
    cxSplitter1: TcxSplitter;
    tsOfertanti: TcxTabSheet;
    tsFiles: TcxTabSheet;
    gridContracteFilesDBTableView1: TcxGridDBTableView;
    gridContracteFilesLevel1: TcxGridLevel;
    gridContracteFiles: TcxGrid;
    gridContracteFilesDBTableView1idContracteFiles: TcxGridDBColumn;
    gridContracteFilesDBTableView1Denumire: TcxGridDBColumn;
    gridContracteFilesDBTableView1Descriere: TcxGridDBColumn;
    gridContracteFilesDBTableView1Document: TcxGridDBColumn;
    cxPropStore: TcxPropertiesStore;
    nivelAditionale: TcxGridLevel;
    grdAditionale: TcxGridDBTableView;
    cxStyleRepository1: TcxStyleRepository;
    cxStyle1: TcxStyle;
    cxStyleRepository2: TcxStyleRepository;
    cxStyle2: TcxStyle;
    cxStyleRepository3: TcxStyleRepository;
    cxStyle3: TcxStyle;
    cxStyle4: TcxStyle;
    cxStyle5: TcxStyle;
    grdContracteidContracte: TcxGridDBColumn;
    grdContracteidParinte: TcxGridDBColumn;
    grdContractePrestator: TcxGridDBColumn;
    grdContracteNrContract: TcxGridDBColumn;
    grdContracteDataContract: TcxGridDBColumn;
    grdContracteTipContract: TcxGridDBColumn;
    grdContracteStare: TcxGridDBColumn;
    grdContracteDataOrdinIncepere: TcxGridDBColumn;
    grdContracteDataPVTerminare: TcxGridDBColumn;
    dsListaOfertanti: TDataSource;
    qryListaOfertanti: TZQuery;
    dsListaAdreseImobil: TDataSource;
    qryListaAdreseImobil: TZQuery;
    dsTipuriFinantari: TDataSource;
    qryTipuriFinantari: TZQuery;
    qryContracteFilesUSR: TZQuery;
    dsContracteFilesUSR: TDataSource;
    qryListaContracteOfertanti: TZQuery;
    dsContracteOfertantiUSR: TDataSource;
    dsObiecteOfertantiUSR: TDataSource;
    qryListaObiecteOfertanti: TZQuery;
    gridContracteOfertanti: TcxGrid;
    vwContracteOfertanti: TcxGridDBTableView;
    vwContracteOfertantiidContracteOfertanti: TcxGridDBColumn;
    vwContracteOfertantiidParinte: TcxGridDBColumn;
    vwContracteOfertantiidContracte: TcxGridDBColumn;
    vwContracteOfertantiidOfertanti: TcxGridDBColumn;
    vwContracteOfertantiliderAsociere: TcxGridDBColumn;
    vwContracteOfertantipretFaraTVALei: TcxGridDBColumn;
    vwContracteOfertantipretFaraTVAEuro: TcxGridDBColumn;
    vwContracteOfertantivaloareTVALei: TcxGridDBColumn;
    vwContracteOfertantipretTVALei: TcxGridDBColumn;
    vwContracteOfertanticontGarantie: TcxGridDBColumn;
    vwContracteOfertantisalvat: TcxGridDBColumn;
    vwContracteOfertantiprocentGarantieDepusa: TcxGridDBColumn;
    vwContracteOfertantivaloareGarantieDepusa: TcxGridDBColumn;
    vwContracteOfertantiprocentGarantieRetinuta: TcxGridDBColumn;
    vwContracteOfertantivaloareGarantieRetinuta: TcxGridDBColumn;
    vwObiecteOfertanti: TcxGridDBTableView;
    vwObiecteOfertantiidContracteOfertanti: TcxGridDBColumn;
    vwObiecteOfertantiidParinte: TcxGridDBColumn;
    vwObiecteOfertantiidInvContabilitate: TcxGridDBColumn;
    vwObiecteOfertantiidContracte: TcxGridDBColumn;
    vwObiecteOfertantipretFaraTVALei: TcxGridDBColumn;
    vwObiecteOfertantipretFaraTVAEuro: TcxGridDBColumn;
    vwObiecteOfertantipretTVALei: TcxGridDBColumn;
    vwObiecteOfertantivaloareTVALei: TcxGridDBColumn;
    vwObiecteOfertantiidFinantariPropuneri: TcxGridDBColumn;
    vwObiecteOfertanticursValutar: TcxGridDBColumn;
    vwObiecteOfertantidataCursValutar: TcxGridDBColumn;
    vwObiecteOfertantisalvat: TcxGridDBColumn;
    lvContracteOfertanti: TcxGridLevel;
    lvObiecteOfertanti: TcxGridLevel;
    vwContracteOfertanticontCurent: TcxGridDBColumn;
    grdContracteValoare: TcxGridDBColumn;
    pnl1: TPanel;
    split1: TcxSplitter;
    txtFiltruNrContr: TcxTextEdit;
    lbl1: TcxLabel;
    cbxFiltruExecutant: TcxLookupComboBox;
    lbl2: TcxLabel;
    lbl3: TcxLabel;
    dtFiltruDataContr: TcxDateEdit;
    btnReset: TSpeedButton;
    qryListaExecutanti: TZQuery;
    dsListaExecutanti: TDataSource;
    grdAditionaleidContracte: TcxGridDBColumn;
    grdAditionaleidParinte: TcxGridDBColumn;
    grdAditionaleNrContract: TcxGridDBColumn;
    grdAditionaleDataContract: TcxGridDBColumn;
    grdAditionalePrestator: TcxGridDBColumn;
    grdAditionaleValoare: TcxGridDBColumn;
    grdAditionaleTipContract: TcxGridDBColumn;
    grdAditionaleStare: TcxGridDBColumn;
    grdAditionaleDataOrdinIncepere: TcxGridDBColumn;
    grdAditionaleDataPVTerminare: TcxGridDBColumn;
    qryContracte: TZQuery;
    dsContracte: TDataSource;
    qryAditionale: TZQuery;
    dsAditionale: TDataSource;
    procedure FormCreate(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure btnCloseClick(Sender: TObject);
    procedure btnModifyClick(Sender: TObject);
    procedure btnDeleteClick(Sender: TObject);
    procedure grdContracteCellDblClick(
      Sender: TcxCustomGridTableView;
      ACellViewInfo: TcxGridTableDataCellViewInfo; AButton: TMouseButton;
      AShift: TShiftState; var AHandled: Boolean);
    procedure grdAditionaleCellDblClick(Sender: TcxCustomGridTableView;
      ACellViewInfo: TcxGridTableDataCellViewInfo; AButton: TMouseButton;
      AShift: TShiftState; var AHandled: Boolean);
    procedure grdContracteFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure grdAditionaleFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure grdContracteCellClick(Sender: TcxCustomGridTableView;
      ACellViewInfo: TcxGridTableDataCellViewInfo; AButton: TMouseButton;
      AShift: TShiftState; var AHandled: Boolean);
    procedure grdAditionaleCellClick(Sender: TcxCustomGridTableView;
      ACellViewInfo: TcxGridTableDataCellViewInfo; AButton: TMouseButton;
      AShift: TShiftState; var AHandled: Boolean);
    procedure txtFiltruNrContrPropertiesChange(Sender: TObject);
    procedure dtFiltruDataContrPropertiesChange(Sender: TObject);
    procedure btnResetClick(Sender: TObject);
    procedure cbxFiltruExecutantPropertiesCloseUp(Sender: TObject);
    procedure btnAddClick(Sender: TObject);
  private
    { Private declarations }
    actAditional: Integer;

  public
    { Public declarations }
    class procedure MainExecuteAction(Sender : TObject); 
  end;

implementation

{$R *.dfm}

uses  ModificaContract, MInvestCommon, FormulareUnit, CommonDBVar, DateUnit;
//------------------------------------------------------------------------------
procedure TfrmListaContracte.FormCreate(Sender: TObject);
begin
//descidem seturile de date
  ReplaceEmptyConnection(Self);
  begin
    qryContracte.Close;
    qryContracte.SQL.Text := 'exec spListaContracte ' + IntToStr(commondbvar.IdUtilizator);
    qryContracte.Open;
    //RefreshDataSet(qryContracte);
    RefreshDataSet(qryAditionale);
    RefreshDataSet(qryListaOfertanti);
    RefreshDataSet(qryListaAdreseImobil);
    RefreshDataSet(qryTipuriFinantari);
    //RefreshDataSet(qryContracteOfertantiUSR);
    RefreshDataSet(qryContracteFilesUSR);
    RefreshDataSet(qryListaExecutanti);


  //deschidem setul de date cu executanti si obiecte in functie de contract
     if qryContracte.RecordCount <> 0 then
    begin
     qryListaContracteOfertanti.Close;
     qryListaContracteOfertanti.ParamByName('idContracte').Value :=
     qryContracte.FieldByName('idContracte').Value;
     qryListaContracteOfertanti.Open;

     qryListaObiecteOfertanti.Close;
     qryListaObiecteOfertanti.ParamByName('idContracte').Value :=
     qryContracte.FieldByName('idContracte').Value;
     qryListaObiecteOfertanti.Open;
    end;
    //qryContracteOfertantiUSR.First;

   // OpenDataSet(qryContracteOfertanti);
  //  OpenDataSet(qryContracteFiles);
    LoadPreferences(cxPropStore);
  end;

  pcDetalii.ActivePage := tsOfertanti;
end;
//------------------------------------------------------------------------------
procedure TfrmListaContracte.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
  SavePreferences(cxPropStore);
end;
//------------------------------------------------------------------------------
procedure TfrmListaContracte.btnAddClick(Sender: TObject);
begin
  ModificareContract(-1);
end;

procedure TfrmListaContracte.btnCloseClick(Sender: TObject);
begin
  Close;
end;
//------------------------------------------------------------------------------
procedure TfrmListaContracte.btnModifyClick(Sender: TObject);
begin
//daca utilizatorul se afla pe nivelul 1 al gridului modificam inregistraare din qryContracte
//altfel din qryAditionale
  begin
   if actAditional = 1 then
    if not qryContracte.IsEmpty then
     ModificareContract(qryContracte.FieldByName('idContracte').AsInteger);

   if actAditional = 2 then
    if not qryContracte.IsEmpty then
     ModificareContract(qryAditionale.FieldByName('idContracte').AsInteger);
  end;
end;
//------------------------------------------------------------------------------
procedure TfrmListaContracte.btnDeleteClick(Sender: TObject);
begin
//stergere contract
  begin
    if actAditional = 1 then
     begin
      if qryContracte.IsEmpty then
       Exit;
      if StergereContract(qryContracte.FieldByName('idContracte').AsInteger) then
      begin
       if not qryContracte.Bof then
         qryContracte.Prior;

       RefreshDataset(qryContracte);
      end;
     end;

     if actAditional = 2 then
     begin
      if qryAditionale.IsEmpty then
       Exit;
      if StergereContract(qryAditionale.FieldByName('idContracte').AsInteger) then
      begin
       if not qryAditionale.Bof then
         qryAditionale.Prior;

       RefreshDataset(qryAditionale);
      end;
     end;
  end;
end;
//------------------------------------------------------------------------------
procedure TfrmListaContracte.grdContracteCellDblClick(
  Sender: TcxCustomGridTableView;
  ACellViewInfo: TcxGridTableDataCellViewInfo; AButton: TMouseButton;
  AShift: TShiftState; var AHandled: Boolean);
begin
  btnModify.Click;
end;
//------------------------------------------------------------------------------


procedure TfrmListaContracte.grdAditionaleCellDblClick(
  Sender: TcxCustomGridTableView;
  ACellViewInfo: TcxGridTableDataCellViewInfo; AButton: TMouseButton;
  AShift: TShiftState; var AHandled: Boolean);
begin
 btnModify.Click;
end;

procedure TfrmListaContracte.grdContracteFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
//actualizare set de date in functie de contract selectat
 qryListaContracteOfertanti.Close;
 qryListaContracteOfertanti.ParamByName('idContracte').Value :=
 qryContracte.FieldByName('idContracte').Value;
 qryListaContracteOfertanti.Open;

 qryListaObiecteOfertanti.Close;
 qryListaObiecteOfertanti.ParamByName('idContracte').Value :=
 qryContracte.FieldByName('idContracte').Value;
 qryListaObiecteOfertanti.Open;
end;

procedure TfrmListaContracte.grdAditionaleFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
  //actualizare set de date in functie de contract selectat
 qryListaContracteOfertanti.Close;
 qryListaContracteOfertanti.ParamByName('idContracte').Value :=
 qryAditionale.FieldByName('idContracte').Value;
 qryListaContracteOfertanti.Open;

 qryListaObiecteOfertanti.Close;
 qryListaObiecteOfertanti.ParamByName('idContracte').Value :=
 qryAditionale.FieldByName('idContracte').Value;
 qryListaObiecteOfertanti.Open;
end;

procedure TfrmListaContracte.grdContracteCellClick(
  Sender: TcxCustomGridTableView;
  ACellViewInfo: TcxGridTableDataCellViewInfo; AButton: TMouseButton;
  AShift: TShiftState; var AHandled: Boolean);
begin
  actAditional := 1;
end;

procedure TfrmListaContracte.grdAditionaleCellClick(
  Sender: TcxCustomGridTableView;
  ACellViewInfo: TcxGridTableDataCellViewInfo; AButton: TMouseButton;
  AShift: TShiftState; var AHandled: Boolean);
begin
  actAditional := 2;
end;

procedure TfrmListaContracte.txtFiltruNrContrPropertiesChange(
  Sender: TObject);
begin
 nrFiltruContr := txtFiltruNrContr.Text;
 qryContracte.DisableControls;
 qryContracte.Filtered := False;
 qryContracte.Filtered := True;
 qryContracte.EnableControls;
end;

procedure TfrmListaContracte.dtFiltruDataContrPropertiesChange(
  Sender: TObject);
begin
 dataFiltruContr := dtFiltruDataContr.Text;
 qryContracte.DisableControls;
 qryContracte.Filtered := False;
 qryContracte.Filtered := True;
 qryContracte.EnableControls;
end;

procedure TfrmListaContracte.btnResetClick(Sender: TObject);
begin
//resetare filtru
 nrFiltruContr := '';
 dataFiltruContr := '';
 txtFiltruNrContr.Text := '';
 dtFiltruDataContr.EditValue := null;
 cbxFiltruExecutant.EditValue := null;
 qryContracte.DisableControls;
 qryContracte.Filtered := False;
 qryContracte.Close;
 qryContracte.SQL.Text := 'exec spListaContracte ' + IntToStr(CommonDBVar.IdUtilizator);
 qryContracte.Open;
 qryContracte.EnableControls;
end;

procedure TfrmListaContracte.cbxFiltruExecutantPropertiesCloseUp(
  Sender: TObject);
begin
//actualizare set de date in functie de executant
 if Trim(cbxFiltruExecutant.Text) = '' then Exit;
 qryContracte.DisableControls;
 qryContracte.Close;
 qryContracte.SQL.Text := 'exec spListaContracteFiltru ' + IntToStr(cbxFiltruExecutant.EditValue);
 qryContracte.Open;
 qryContracte.Filtered := False;
 qryContracte.Filtered := True;
 qryContracte.EnableControls;
end;


class procedure TfrmListaContracte.MainExecuteAction(Sender: TObject);
begin
  with GetNewForm(TfrmListaContracte) do begin
    Show;
    WindowState := wsMaximized;
  end;
end;

initialization
  RegisterMenuItem('Cmd_InvestListaContracte', 'MInvest', 'Lista contracte', TfrmListaContracte.MainExecuteAction);
end.
