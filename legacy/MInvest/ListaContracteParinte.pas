unit ListaContracteParinte;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxStyles, cxCustomData, cxGraphics, cxFilter, cxData,
  cxDataStorage, cxEdit, DB, cxDBData, cxGridLevel, cxGridCustomTableView,
  cxGridTableView, cxGridDBTableView, cxClasses, cxControls,
  cxGridCustomView, cxGrid, cxDropDownEdit, cxCalendar, cxMaskEdit,
  cxLookupEdit, cxDBLookupEdit, cxDBLookupComboBox, cxLabel, cxContainer,
  cxTextEdit, Buttons, ExtCtrls, cxLookAndFeels, cxLookAndFeelPainters,
  ZAbstractRODataset, ZAbstractDataset, ZDataset, cxNavigator,
  cxDataControllerConditionalFormattingRulesManagerDialog, Vcl.ComCtrls, dxCore,
  cxDateUtils;

type
  TfrmListaContracteP = class(TForm)
    gridContracte: TcxGrid;
    grdContracte: TcxGridDBTableView;
    grdAditionale: TcxGridDBTableView;
    grdAditionaleidContracte: TcxGridDBColumn;
    grdAditionaleidParinte: TcxGridDBColumn;
    grdAditionaleidInvContabilitate: TcxGridDBColumn;
    grdAditionaleidPrestatorContabilitate: TcxGridDBColumn;
    grdAditionaleidBeneficiarContabilitate: TcxGridDBColumn;
    grdAditionaleValoare: TcxGridDBColumn;
    grdAditionaleNrContract: TcxGridDBColumn;
    grdAditionaleDataContract: TcxGridDBColumn;
    grdAditionaleTipContract: TcxGridDBColumn;
    grdAditionaleStare: TcxGridDBColumn;
    grdAditionaleDataStart: TcxGridDBColumn;
    grdAditionaleDataStop: TcxGridDBColumn;
    grdAditionaleTermenFinalizare: TcxGridDBColumn;
    grdAditionaleDataOrdinIncepere: TcxGridDBColumn;
    grdAditionaleManProiectBeneficiar: TcxGridDBColumn;
    grdAditionaleManProiectOfertant: TcxGridDBColumn;
    grdAditionaleGarantieLucrariStart: TcxGridDBColumn;
    grdAditionaleGarantieLucrariStop: TcxGridDBColumn;
    nivelContracte: TcxGridLevel;
    dsContracte: TDataSource;
    grdContracteidContracte: TcxGridDBColumn;
    grdContracteNrContract: TcxGridDBColumn;
    grdContracteDataContract: TcxGridDBColumn;
    grdContractePrestator: TcxGridDBColumn;
    grdContracteValoare: TcxGridDBColumn;
    grdContracteTipContract: TcxGridDBColumn;
    grdContracteDataOrdinIncepere: TcxGridDBColumn;
    grdContracteDataPVTerminare: TcxGridDBColumn;
    pnl1: TPanel;
    btnReset: TSpeedButton;
    txtFiltruNrContr: TcxTextEdit;
    lbl1: TcxLabel;
    cbxFiltruExecutant: TcxLookupComboBox;
    lbl2: TcxLabel;
    lbl3: TcxLabel;
    dtFiltruDataContr: TcxDateEdit;
    cxstylrpstry1: TcxStyleRepository;
    style1: TcxStyle;
    dsListaExecutanti: TDataSource;
    grdContracteidParinte: TcxGridDBColumn;
    grdContracteStare: TcxGridDBColumn;
    grdContracteidTipuriContracte: TcxGridDBColumn;
    grdContracteidStariContracte: TcxGridDBColumn;
    grdContracteDurataGarantieAni: TcxGridDBColumn;
    grdContracteDurataOrdinLuni: TcxGridDBColumn;
    grdContracteProcentGarantieDepusa: TcxGridDBColumn;
    grdContracteProcentGarantieRetinuta: TcxGridDBColumn;
    grdContracteNrOrdinIncepere: TcxGridDBColumn;
    grdContracteNrPVTerminare: TcxGridDBColumn;
    grdContracteNumarPVReceptie: TcxGridDBColumn;
    grdContracteManProiectBeneficiar: TcxGridDBColumn;
    grdContracteManProiectOfertant: TcxGridDBColumn;
    grdContracteDurataOrdinAni: TcxGridDBColumn;
    grdContracteDurataGarantieLuni: TcxGridDBColumn;
    grdContracteCursEuroData: TcxGridDBColumn;
    grdContracteCursEuro: TcxGridDBColumn;
    grdContracteDataPVReceptie: TcxGridDBColumn;
    qryListaExecutanti: TZQuery;
    qryContracte: TZQuery;
    procedure grdContracteDblClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure qryContracte1FilterRecord(DataSet: TDataSet;
      var Accept: Boolean);
    procedure txtFiltruNrContrPropertiesChange(Sender: TObject);
    procedure dtFiltruDataContrPropertiesChange(Sender: TObject);
    procedure cbxFiltruExecutantPropertiesCloseUp(Sender: TObject);
    procedure btnResetClick(Sender: TObject);
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  frmListaContracteP: TfrmListaContracteP;

implementation
uses
  CommonDBVar, DateUnit, MInvestCommon;

{$R *.dfm}

procedure TfrmListaContracteP.grdContracteDblClick(Sender: TObject);
begin
//in functie de contractul parinte selectat setam volri pe variabille din Utils
//folosite ulterior pentru noul contract aditional
 contractAditional := True;
 modificaValGarantie := False;
 idSelectat := grdContracte.DataController.GetValue(grdContracte.DataController.FocusedRecordIndex,
               grdContracte.DataController.GetItemByFieldName('idContracte').Index);

 idTipuriContracte := grdContracte.DataController.GetValue(grdContracte.DataController.FocusedRecordIndex,
                      grdContracte.DataController.GetItemByFieldName('idTipuriContracte').Index);
 idStariContracte := grdContracte.DataController.GetValue(grdContracte.DataController.FocusedRecordIndex,
                     grdContracte.DataController.GetItemByFieldName('idStariContracte').Index);
 DurataGarantieAni := grdContracte.DataController.GetValue(grdContracte.DataController.FocusedRecordIndex,
                      grdContracte.DataController.GetItemByFieldName('DurataGarantieAni').Index);
 DurataOrdinLuni := grdContracte.DataController.GetValue(grdContracte.DataController.FocusedRecordIndex,
                    grdContracte.DataController.GetItemByFieldName('DurataOrdinLuni').Index);
 ProcentGarantieDepusa := grdContracte.DataController.GetValue(grdContracte.DataController.FocusedRecordIndex,
                          grdContracte.DataController.GetItemByFieldName('ProcentGarantieDepusa').Index);
 ProcentGarantieRetinuta := grdContracte.DataController.GetValue(grdContracte.DataController.FocusedRecordIndex,
                            grdContracte.DataController.GetItemByFieldName('ProcentGarantieRetinuta').Index);
 DataOrdinIncepere := grdContracte.DataController.GetValue(grdContracte.DataController.FocusedRecordIndex,
                      grdContracte.DataController.GetItemByFieldName('DataOrdinIncepere').Index);
 NrOrdinIncepere := grdContracte.DataController.GetValue(grdContracte.DataController.FocusedRecordIndex,
                     grdContracte.DataController.GetItemByFieldName('NrOrdinIncepere').Index);
 DataPVTerminare := grdContracte.DataController.GetValue(grdContracte.DataController.FocusedRecordIndex,
                    grdContracte.DataController.GetItemByFieldName('DataPVTerminare').Index);
 NrPVTerminare := grdContracte.DataController.GetValue(grdContracte.DataController.FocusedRecordIndex,
                  grdContracte.DataController.GetItemByFieldName('NrPVTerminare').Index);
 NumarPVReceptie := grdContracte.DataController.GetValue(grdContracte.DataController.FocusedRecordIndex,
                    grdContracte.DataController.GetItemByFieldName('NumarPVReceptie').Index);
 DataPVReceptie := grdContracte.DataController.GetValue(grdContracte.DataController.FocusedRecordIndex,
                   grdContracte.DataController.GetItemByFieldName('DataPVReceptie').Index);
 ManProiectBeneficiar := grdContracte.DataController.GetValue(grdContracte.DataController.FocusedRecordIndex,
                         grdContracte.DataController.GetItemByFieldName('ManProiectBeneficiar').Index);
 ManProiectOfertant := grdContracte.DataController.GetValue(grdContracte.DataController.FocusedRecordIndex,
                       grdContracte.DataController.GetItemByFieldName('ManProiectOfertant').Index);
 DurataOrdinAni := grdContracte.DataController.GetValue(grdContracte.DataController.FocusedRecordIndex,
                   grdContracte.DataController.GetItemByFieldName('DurataOrdinAni').Index);
 DurataGarantieLuni := grdContracte.DataController.GetValue(grdContracte.DataController.FocusedRecordIndex,
                       grdContracte.DataController.GetItemByFieldName('DurataGarantieLuni').Index);
 CursEuroData := grdContracte.DataController.GetValue(grdContracte.DataController.FocusedRecordIndex,
                 grdContracte.DataController.GetItemByFieldName('CursEuroData').Index);
 CursEuro := grdContracte.DataController.GetValue(grdContracte.DataController.FocusedRecordIndex,
                       grdContracte.DataController.GetItemByFieldName('CursEuro').Index);
 Close;
end;

procedure TfrmListaContracteP.FormCreate(Sender: TObject);
begin
  ReplaceEmptyConnection(Self);
//afisam setul de date in grid
 qryContracte.Close;
 qryContracte.SQL.Text := 'exec spListaContracteParinte ' + IntToStr(commondbvar.IdUtilizator);
 qryContracte.Open;
 qryListaExecutanti.Close;
 qryListaExecutanti.Open;
end;

procedure TfrmListaContracteP.qryContracte1FilterRecord(DataSet: TDataSet;
  var Accept: Boolean);
begin
//filtru pe set de date
 Accept := True;

 if Trim(txtFiltruNrContr.Text) <> '' then
  Accept := Accept and
            (Pos(AnsiLowerCase(txtFiltruNrContr.Text), AnsiLowerCase(qryContracte.FieldByName('NrContract').AsString)) > 0);

 if dtFiltruDataContr.Text <> '' then
  Accept := Accept and
            (Pos(AnsiLowerCase(dtFiltruDataContr.Text), AnsiLowerCase(qryContracte.FieldByName('DataContract').AsString)) > 0);
end;

procedure TfrmListaContracteP.txtFiltruNrContrPropertiesChange(
  Sender: TObject);
begin
 qryContracte.DisableControls;
 qryContracte.Filtered := False;
 qryContracte.Filtered := True;
 qryContracte.EnableControls;
end;

procedure TfrmListaContracteP.dtFiltruDataContrPropertiesChange(
  Sender: TObject);
begin
 qryContracte.DisableControls;
 qryContracte.Filtered := False;
 qryContracte.Filtered := True;
 qryContracte.EnableControls;
end;

procedure TfrmListaContracteP.cbxFiltruExecutantPropertiesCloseUp(
  Sender: TObject);
begin
//filtru pe lista contracxte in functie de executant
 if Trim(cbxFiltruExecutant.Text) = '' then Exit;
 qryContracte.DisableControls;
 qryContracte.Close;
 qryContracte.SQL.Text := 'exec spListaContracteFiltru ' + IntToStr(cbxFiltruExecutant.EditValue);
 qryContracte.Open;
 qryContracte.Filtered := False;
 qryContracte.Filtered := True;
 qryContracte.EnableControls;
end;

procedure TfrmListaContracteP.btnResetClick(Sender: TObject);
begin
  //resetare set de date
 nrFiltruContr := '';
 dataFiltruContr := '';
 txtFiltruNrContr.Text := '';
 dtFiltruDataContr.EditValue := null;
 cbxFiltruExecutant.EditValue := null;
 qryContracte.DisableControls;
 qryContracte.Filtered := False;
 qryContracte.Close;
 qryContracte.Open;
 qryContracte.EnableControls;
end;

end.
