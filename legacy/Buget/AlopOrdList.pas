unit AlopOrdList;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, Menus, cxLookAndFeelPainters, DB, ZDataSet, StdCtrls, cxButtons,
  cxControls, cxSplitter, ExtCtrls, cxGraphics, cxDataStorage, cxEdit,
  cxDBData, cxGridLevel, cxClasses, cxGridCustomView, cxGridCustomTableView,
  cxGridTableView, cxGridDBTableView, cxGrid,  cxContainer, cxGroupBox,
  cxGridBandedTableView, cxGridDBBandedTableView, cxGridCustomPopupMenu,
  cxGridPopupMenu, cxCurrencyEdit, ZAbstractRODataset, ZAbstractDataset,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, cxTextEdit,
  cxMaskEdit, cxDropDownEdit, cxImageComboBox, cxCheckBox, cxNavigator,
  dxDateRanges, cxDataControllerConditionalFormattingRulesManagerDialog,
  dxBarBuiltInMenu, dxScrollbarAnnotations, cxProgressBar;

type
  TfrmALOPListaOrd = class(TForm)
    pnClient: TPanel;
    Splitter: TcxSplitter;
    BtnOk: TcxButton;
    btnRefaOrdonantare: TcxButton;
    BtnCancel: TcxButton;
    btnAnuleazaAng: TcxButton;
    QryOrd: TZQuery;
    DTOrd: TDataSource;
    DTOrdDetEco: TDataSource;
    QryOrdDetEco: TZQuery;
    gbLista: TcxGroupBox;
    cxGridLista: TcxGrid;
    gbDetalii: TcxGroupBox;
    cxGridDetalii: TcxGrid;
    btnRefresh: TcxButton;
    btnOrdonantare: TcxButton;
    cxGridPopupMenu1: TcxGridPopupMenu;
    cxGridPopupMenu2: TcxGridPopupMenu;
    GridOrd: TcxGridDBTableView;
    cxGridListaLevel1: TcxGridLevel;
    GridOrdid_alop_ordonantare: TcxGridDBColumn;
    GridOrddepartament: TcxGridDBColumn;
    GridOrdnume_repartitor: TcxGridDBColumn;
    GridOrdnatura_cheltuielii: TcxGridDBColumn;
    GridOrdnume_utilizator: TcxGridDBColumn;
    GridOrddocumente_lichidate: TcxGridDBColumn;
    GridOrdnumar: TcxGridDBColumn;
    GridOrddata_emitere: TcxGridDBColumn;
    GridOrdsuma_plata: TcxGridDBColumn;
    GridDetailLevel1: TcxGridLevel;
    GridOrdDetail: TcxGridDBTableView;
    GridOrdDetailid_alop_ordonantare_defalcare: TcxGridDBColumn;
    GridOrdDetailid_alop_ordonantare: TcxGridDBColumn;
    GridOrdDetailcod_functional: TcxGridDBColumn;
    GridOrdDetailcod_economic: TcxGridDBColumn;
    GridOrdDetailDenFunctional: TcxGridDBColumn;
    GridOrdDetailDenEconomic: TcxGridDBColumn;
    GridOrdDetaildisponibil_inainte: TcxGridDBColumn;
    GridOrdDetailsuma_plata: TcxGridDBColumn;
    GridOrdDetaildisponibil_dupa: TcxGridDBColumn;
    GridOrdDetaildisponibil_trim_inainte: TcxGridDBColumn;
    GridOrdDetaildisponibil_trim_dupa: TcxGridDBColumn;
    GridOrdCoduriEconomice: TcxGridDBColumn;
    GridOrdCoduriFunctionale: TcxGridDBColumn;
    GridOrdDetaildisponibil_an_inainte: TcxGridDBColumn;
    GridOrdDetaildisponibil_an_dupa: TcxGridDBColumn;
    GridOrdProiect: TcxGridDBColumn;
    GridOrdNR_CONTRACT: TcxGridDBColumn;
    GridOrdDATA_CONTRACT: TcxGridDBColumn;
    qryOrdOperatori: TZQuery;
    qryOrdProiecte: TZQuery;
    pnTop: TPanel;
    lbOperator: TLabel;
    lbUnitate: TLabel;
    lbProiect: TLabel;
    lbCodFunctional: TLabel;
    lbCodEconomic: TLabel;
    edOperator: TcxImageComboBox;
    edUnitate: TcxImageComboBox;
    edProiect: TcxImageComboBox;
    edCodFunctional: TcxImageComboBox;
    edCodEconomic: TcxImageComboBox;
    Label1: TLabel;
    edStareOrdonantare: TcxImageComboBox;
    GridOrdstareOrdonantare: TcxGridDBColumn;
    stiluriGrid: TcxStyleRepository;
    stilDelete: TcxStyle;
    procedure btnRefreshClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure BtnOkClick(Sender: TObject);
    procedure BtnCancelClick(Sender: TObject);
    procedure btnOrdonantareClick(Sender: TObject);
    procedure btnAnuleazaAngClick(Sender: TObject);
    procedure GridOrdFocusedRecordChanged(Sender: TcxCustomGridTableView;
      APrevFocusedRecord, AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure FormShow(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure edUnitatePropertiesChange(Sender: TObject);
    procedure pnTopResize(Sender: TObject);
    procedure chkShowHistoryPropertiesChange(Sender: TObject);
    procedure GridOrdStylesGetContentStyle(Sender: TcxCustomGridTableView;
      ARecord: TcxCustomGridRecord; AItem: TcxCustomGridTableItem;
      var AStyle: TcxStyle);
  private
    FIdOrdonantare: Integer;
    FirstRunThis : Boolean;

    function GetSQLCondition: String;
    { Private declarations }
  public
    { Public declarations }
    procedure RefreshScreen;
    procedure RefreshData;
    property IdOrdonantare : Integer read FIdOrdonantare;
  end;


implementation

uses
  dxCompsUtile, ZeosDBUtile, CommonDBVar, AlopLichidare,
  dateUnit, MainUnit, PersistGridSettings, frmProgressUnit, frxClass;

{$R *.dfm}

procedure TfrmALOPListaOrd.btnRefreshClick(Sender: TObject);
begin
  RefreshScreen;
end;

procedure TfrmALOPListaOrd.RefreshScreen;
begin
  DBRefresh([qryOrdOperatori, qryOrdProiecte]);

  if FirstRunThis then begin
    if FormStyle = fsNormal then
      edOperator.EditValue := IdUtilizator
    else
      edOperator.EditValue := Null;
    edProiect.EditValue := Null;
  end;

  RefreshData;
  DBRefresh(QryOrdDetEco);

  if GridOrd.ItemCount > 0 then begin
     GridOrd.Items[0].Focused := True;
     //TreeAngajamenteChangeNode(TreeAngajamente, nil, TreeAngajamente.TopNode);
  end;

  if FirstRunThis then begin
    cxCreateMissingColumns(QryOrdDetEco, GridOrdDetail);
    cxCreateMissingColumns(QryOrd, GridOrd);
    InitVisibleColumns(Self, GridOrd);
    InitVisibleColumns(Self, GridOrdDetail);
    StorageReadCxView(GridOrd);
    StorageReadCxView(GridOrdDetail);
    FirstRunThis := False;
  end;
end;

procedure TfrmALOPListaOrd.FormCreate(Sender: TObject);
begin
  FirstRunThis := True;
  FIdOrdonantare := -1;

  FillImageComboFmt(edOperator.Properties       , 'exec [spAlopListaOperatoriOrdonantari] %d, %d' , [IdLogin, IdUtilizator], 'ID_UTILIZATORI' , 'NUMEINTREG' , Null, 'Toti Utilizatorii');
  FillImageComboFmt(edUnitate.Properties        , 'exec [spAlopListaUnitatiOrdonantari]   %d, %d' , [IdLogin, IdUtilizator], 'id_oi_unitati'  , 'denumire'   , Null, 'Toate Unitatile');
  FillImageComboFmt(edProiect.Properties        , 'exec [spAlopListaProiecteOrdonantari]  %d, %d' , [IdLogin, IdUtilizator], 'id_oi_proiecte' , 'denumire'   , Null, 'Toate Proiectele');
  FillImageComboFmt(edCodFunctional.Properties  , 'exec [spAlopListaCFOrdonantari]        %d, %d' , [IdLogin, IdUtilizator], 'cod_functional' , 'denumire'   , Null, 'Toate Clasificatiile Functionale');
  FillImageComboFmt(edCodEconomic.Properties    , 'exec [spAlopListaCEOrdonantari]        %d, %d' , [IdLogin, IdUtilizator], 'cod_economic'   , 'denumire'   , Null, 'Toate Clasificatiile Economice');
  
end;

procedure TfrmALOPListaOrd.BtnOkClick(Sender: TObject);
begin
  if fsModal in FormState then ModalResult := mrOk
  else Close;
end;

procedure TfrmALOPListaOrd.BtnCancelClick(Sender: TObject);
begin
  if fsModal in FormState then ModalResult := mrCancel
  else Close;
end;

procedure TfrmALOPListaOrd.btnOrdonantareClick(Sender: TObject);
var
  lIdReport: Integer;
begin
  if (GridOrd.Controller.SelectedRowCount > 1) then begin
    lIdReport := GetItemId('Ordonantare');
    if (lIdReport <> -1) then begin
      ShowReportList(
        'Generare Raport Lista Ordonantari',
        lIdReport,
        GridOrd.Controller.SelectedRowCount,
        procedure (Index: Integer; AReport: TfrxReport)
        var
          lIdOrdonantare: Integer;
        begin
          lIdOrdonantare := GridOrd.Controller.SelectedRows[Index].Values[GridOrdid_alop_ordonantare.Index];
          DateUnit.IdOrdonantare := lIdOrdonantare;
          mainForm.SetRaportParams(AReport);
        end);
    end;
  end
  else begin
    PrintOrdonantare(IdOrdonantare);
  end;
end;

procedure TfrmALOPListaOrd.btnAnuleazaAngClick(Sender: TObject);
var
  lNrOrd : String;
  lDataOrd : String;
begin
 //FIdOrdonantare
 if FIdOrdonantare <> -1 then begin
    lNrOrd := QryOrd.FieldByName('numar').AsString;
    lDataOrd := QryOrd.FieldByName('data_emitere').AsString;
    if (MessageDlg(Format('Doriti stergerea ordonatarilor nr. : %s din data  %s ?', [
        lNrOrd, lDataOrd]), mtConfirmation, [mbYes, mbNo], 0) in [mrNo, mrNone]) then
       Abort;
    DBExecSQLFmt('exec [spAlopAnuleazaOrdonantare] %d', [FIdOrdonantare]);
    DBRefresh(QryOrd);
 end;
end;

procedure TfrmALOPListaOrd.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmALOPListaOrd.GridOrdFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
var
  lStare: Boolean;
begin
  if fsCreating in FormState then Exit;
  if not Assigned(AFocusedRecord) then Exit;
  if not AFocusedRecord.IsData  then Exit;
  FIdOrdonantare := GetInteger(AFocusedRecord, GridOrdid_alop_ordonantare.Index);
  lStare := GetBoolean(AFocusedRecord, GridOrdstareOrdonantare.Index);
  BtnOk.Enabled := (FIdOrdonantare <> -1) and lStare;
  btnRefaOrdonantare.Enabled := not lStare;
  btnAnuleazaAng.Enabled     := lStare;
  btnOrdonantare.Enabled     := lStare;
  if (QryOrdDetEco.Params.ParamByName('ID_ALOP_ORDONANTARE').Value <> FIdOrdonantare) then begin
    QryOrdDetEco.Close;
    QryOrdDetEco.Params.ParamByName('ID_ALOP_ORDONANTARE').Value := FIdOrdonantare;
    QryOrdDetEco.Open;
  end;
end;

procedure TfrmALOPListaOrd.GridOrdStylesGetContentStyle(
  Sender: TcxCustomGridTableView; ARecord: TcxCustomGridRecord;
  AItem: TcxCustomGridTableItem; var AStyle: TcxStyle);
begin
  if Assigned(ARecord) and ARecord.IsData then
    if not GetBoolean(ARecord, GridOrdstareOrdonantare.Index) then
      AStyle := stilDelete;
end;

procedure TfrmALOPListaOrd.RefreshData;
begin
  DoCheckClose(QryOrd);
  QryOrd.Params.ParamByName('IdUtilizator').Value := edOperator.EditValue;
  QryOrd.Params.ParamByName('IdAnalitic').Value   := edProiect.EditValue;
  qryOrd.Params.ParamByName('stareOrd').Value     := edStareOrdonantare.EditValue;
  QryOrd.Params.ParamByName('sqlCondition').Value := GetSQLCondition;
  DBRefresh(QryOrd);
end;

procedure TfrmALOPListaOrd.FormShow(Sender: TObject);
begin
  GridOrd.ApplyBestFit(nil);
  RefreshScreen;
end;

procedure TfrmALOPListaOrd.FormDestroy(Sender: TObject);
begin
  StorageWriteCxView(GridOrd);
  StorageWriteCxView(GridOrdDetail);
end;

procedure TfrmALOPListaOrd.edUnitatePropertiesChange(Sender: TObject);
begin
  if FirstRunThis then Exit;
  RefreshData;
end;

procedure TfrmALOPListaOrd.pnTopResize(Sender: TObject);
var
  lGroupWidth : Integer;
  lEditWidth  : Integer;
  lLeft       : Integer;

    procedure SetEdit(ALabel: TLabel; AControl: TControl);
    begin
      ALabel.Left    := lLeft + 10;
      ALabel.Width   := 50;
      AControl.Left  := lLeft + 15 + 50;
      AControl.Width := lEditWidth - 10;
      Inc(lLeft, lGroupWidth);
    end;

begin
  lGroupWidth := pnTop.Width div 5;
  lEditWidth  := lGroupWidth - 55;
  lLeft       := 0;
  SetEdit(lbUnitate       , edUnitate);
  SetEdit(lbProiect       , edProiect);
  SetEdit(lbCodFunctional , edCodFunctional);
  SetEdit(lbCodEconomic   , edCodEconomic);
  SetEdit(lbOperator      , edOperator);
end;

function TfrmALOPListaOrd.GetSQLCondition: String;

    procedure AddToWhere(const ASQL: String);
    begin
      if Result > '' then
        Result := Result + ' and ';
      Result := Result + ASQL;
    end;

begin
  Result := '';
  if ValueHasValue(edUnitate.EditValue) then
    AddToWhere('id_oi_unitati = ' + ValueToStr(edUnitate.EditValue));
  if ValueHasValue(edProiect.EditValue) then
    AddToWhere('id_oi_proiecte = ' + ValueToStr(edProiect.EditValue));
  if ValueHasValue(edCodFunctional.EditValue) then
    AddToWhere('cod_functional = ' + ValueToStr(edCodFunctional.EditValue));
  if ValueHasValue(edCodEconomic.EditValue) then
    AddToWhere('cod_economic = ' + ValueToStr(edCodEconomic.EditValue));
  if Result > '' then
    Result := Format('id_alop_ordonantare in (select id_alop_ordonantare from alop_ordonantare_defalcare where %s)', [Result]);
end;

procedure TfrmALOPListaOrd.chkShowHistoryPropertiesChange(Sender: TObject);
begin
  RefreshData;
end;

end.
