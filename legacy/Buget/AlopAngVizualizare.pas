unit AlopAngVizualizare;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, ExtCtrls, cxControls, cxSplitter, StdCtrls, ImgList, dxDBGrid,
  dxGrClms, dxTL, dxDBCtrl, dxCntner, DB, ZDataSet, cxButtons, dxDBTLCl, dxDBTL,
  cxContainer, cxEdit, cxTextEdit, cxMaskEdit, cxDropDownEdit, cxImageComboBox,
  dxfCheckBox, dxExEdtr, cxGraphics, Menus, cxLookAndFeelPainters,
  cxDataStorage, cxDBData, cxGridLevel, cxClasses, cxGridCustomView,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGrid,
  cxGridCustomPopupMenu, cxGridPopupMenu, cxCurrencyEdit,
  ZAbstractRODataset, ZAbstractDataset,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, dxBar, ActnList,
  ZSqlUpdate, cxPC, cxCalendar, cxNavigator, dxBarBuiltInMenu,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxDateRanges,
  dxScrollbarAnnotations, Vcl.ComCtrls, dxCore, cxDateUtils;

type
  TfrmAlopAngajamenteVizualizare = class(TForm)
    Splitter: TcxSplitter;
    grDetaliereEconomica: TGroupBox;
    pnClient: TPanel;
    ImgList: TImageList;
    dtAngajamente: TDataSource;
    QryAngajamente: TZQuery;
    dtDetaliuEconomic: TDataSource;
    qryDetaliuEconomic: TZQuery;
    BtnModificare: TcxButton;
    btnAnuleazaAng: TcxButton;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    btnRefresh: TcxButton;
    pnTop: TPanel;
    lbOperator: TLabel;
    edOperator: TcxImageComboBox;
    lbUnitate: TLabel;
    edUnitate: TcxImageComboBox;
    pnBottom: TPanel;
    gridAngajamente: TcxGrid;
    viewAngajamente: TcxGridDBTableView;
    nivelAngajament: TcxGridLevel;
    viewAngajamentenume_utilizator: TcxGridDBColumn;
    viewAngajamenteid: TcxGridDBColumn;
    viewAngajamenteparent_id: TcxGridDBColumn;
    viewAngajamentenume_departament: TcxGridDBColumn;
    viewAngajamentenume_repartitor: TcxGridDBColumn;
    viewAngajamentenumar: TcxGridDBColumn;
    viewAngajamentedata_emitere: TcxGridDBColumn;
    viewAngajamentesuma_angajata: TcxGridDBColumn;
    viewAngajamenteTotalOrdonantat: TcxGridDBColumn;
    viewAngajamenteRamasOrdonantat: TcxGridDBColumn;
    viewAngajamenteSCOPUL: TcxGridDBColumn;
    viewAngajamenteTIP_ANGAJAMENT: TcxGridDBColumn;
    viewAngajamenteCOD_FUNCTIONAL: TcxGridDBColumn;
    cxGridPopupMenu: TcxGridPopupMenu;
    gridDetaliuEconomic: TcxGrid;
    nivelDetaliuEconomic: TcxGridLevel;
    viewDetaliuEconomic: TcxGridDBTableView;
    viewDetaliuEconomicCOD_FUNCTIONAL: TcxGridDBColumn;
    viewDetaliuEconomicDEN_FUNCTIONAL: TcxGridDBColumn;
    viewDetaliuEconomicDEN_ECONOMIC: TcxGridDBColumn;
    viewDetaliuEconomicID_ALOP_ANGAJAMENTE_DEFALCARE: TcxGridDBColumn;
    viewDetaliuEconomicID_ALOP_ANGAJAMENTE: TcxGridDBColumn;
    viewDetaliuEconomicCOD_ECONOMIC: TcxGridDBColumn;
    viewDetaliuEconomicAPROBATE: TcxGridDBColumn;
    viewDetaliuEconomicTOTAL_ANGAJATE: TcxGridDBColumn;
    viewDetaliuEconomicDISPONIBIL: TcxGridDBColumn;
    viewDetaliuEconomicID_VALUTA: TcxGridDBColumn;
    viewDetaliuEconomicANGAJAT_VALUTA: TcxGridDBColumn;
    viewDetaliuEconomicCURS_VALUTAR: TcxGridDBColumn;
    viewDetaliuEconomicANGAJAT: TcxGridDBColumn;
    viewDetaliuEconomicRAMAS_DE_ANGAJAT: TcxGridDBColumn;
    viewDetaliuEconomicDESCRIERE: TcxGridDBColumn;
    viewAngajamenteDATA_OPERARE: TcxGridDBColumn;
    cxGridPopupMenuDetail: TcxGridPopupMenu;
    viewAngajamenteID_ALOP_ANGAJAMENTE: TcxGridDBColumn;
    viewAngajamenteCoduriEconomice: TcxGridDBColumn;
    dtIstoricAngajament: TDataSource;
    qryIstoricAngajament: TZQuery;
    nivelIstoric: TcxGridLevel;
    viewIstoricAngajamente: TcxGridDBTableView;
    viewIstoricAngajamenteIdParinte: TcxGridDBColumn;
    viewIstoricAngajamenteordine: TcxGridDBColumn;
    viewIstoricAngajamentenume_utilizator: TcxGridDBColumn;
    viewIstoricAngajamenteid: TcxGridDBColumn;
    viewIstoricAngajamenteparent_id: TcxGridDBColumn;
    viewIstoricAngajamentenume_departament: TcxGridDBColumn;
    viewIstoricAngajamentenume_repartitor: TcxGridDBColumn;
    viewIstoricAngajamentenumar: TcxGridDBColumn;
    viewIstoricAngajamentedata_emitere: TcxGridDBColumn;
    viewIstoricAngajamentesuma_angajata: TcxGridDBColumn;
    viewIstoricAngajamenteTotalOrdonantat: TcxGridDBColumn;
    viewIstoricAngajamenteCoduriEconomice: TcxGridDBColumn;
    viewIstoricAngajamenteRamasOrdonantat: TcxGridDBColumn;
    viewIstoricAngajamenteID_ALOP_ANGAJAMENTE: TcxGridDBColumn;
    viewIstoricAngajamenteID_UTILIZATORI: TcxGridDBColumn;
    viewIstoricAngajamenteDATA_EMITERE_1: TcxGridDBColumn;
    viewIstoricAngajamenteID_DEPARTAMENT: TcxGridDBColumn;
    viewIstoricAngajamenteNUMAR_1: TcxGridDBColumn;
    viewIstoricAngajamenteSCOPUL: TcxGridDBColumn;
    viewIstoricAngajamenteID_LST_REPARTITORI: TcxGridDBColumn;
    viewIstoricAngajamenteVALIDAT: TcxGridDBColumn;
    viewIstoricAngajamenteTIME_IMPORT: TcxGridDBColumn;
    viewIstoricAngajamenteCLASA_FUNCTIONALA: TcxGridDBColumn;
    viewIstoricAngajamenteTIP_ANGAJAMENT: TcxGridDBColumn;
    viewIstoricAngajamenteRECTIFICARE: TcxGridDBColumn;
    viewIstoricAngajamenteID_CONTRACT: TcxGridDBColumn;
    viewIstoricAngajamenteID_ACT_ADITIONAL: TcxGridDBColumn;
    viewIstoricAngajamenteESTE_INCHIS: TcxGridDBColumn;
    viewIstoricAngajamenteID_PARINTE: TcxGridDBColumn;
    viewIstoricAngajamenteSTARE: TcxGridDBColumn;
    viewIstoricAngajamenteCOD_FUNCTIONAL: TcxGridDBColumn;
    viewIstoricAngajamenteDATA_OPERARE: TcxGridDBColumn;
    viewIstoricAngajamenteDATA: TcxGridDBColumn;
    viewIstoricAngajamenteDATA_ANULARE: TcxGridDBColumn;
    viewIstoricAngajamenteID_ANALITIC: TcxGridDBColumn;
    viewIstoricAngajamenteCOD_ECRAN: TcxGridDBColumn;
    viewAngajamenteProiect: TcxGridDBColumn;
    viewAngajamenteNR_CONTRACT: TcxGridDBColumn;
    viewAngajamenteDATA_CONTRACT: TcxGridDBColumn;
    AngAction: TActionList;
    CmdAsociereContract: TAction;
    PopupAngajamente: TPopupMenu;
    Cmd_Asocierecontract: TMenuItem;
    Cmd_DezasociereContract: TMenuItem;
    CmdDezasociereContract: TAction;
    qryAngUpdContract: TZQuery;
    ZUpdateSQL1: TZUpdateSQL;
    tcViza: TcxTabControl;
    N1: TMenuItem;
    CmdVizaTrezorerie: TAction;
    CmdDevizareTrezorerie: TAction;
    Vizattrezorerie1: TMenuItem;
    btnRapoarte: TcxButton;
    edProiect: TcxImageComboBox;
    lbProiect: TLabel;
    edCodFunctional: TcxImageComboBox;
    lbCodFunctional: TLabel;
    lbCodEconomic: TLabel;
    edCodEconomic: TcxImageComboBox;
    lbData: TLabel;
    edData: TcxDateEdit;
    procedure BtnModificareClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure BtnOkClick(Sender: TObject);
    procedure BtnCancelClick(Sender: TObject);
    procedure btnAnuleazaAngClick(Sender: TObject);
    procedure btnRefreshClick(Sender: TObject);
    procedure edProiectPropertiesChange(Sender: TObject);
    procedure FormShow(Sender: TObject);
    procedure pnBottomResize(Sender: TObject);
    procedure viewAngajamenteFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure viewIstoricAngajamenteFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure cxGridPopupMenuPopup(ASenderMenu: TComponent;
      AHitTest: TcxCustomGridHitTest; X, Y: Integer;
      var AllowPopup: Boolean);
    procedure CmdAsociereContractExecute(Sender: TObject);
    procedure CmdDezasociereContractExecute(Sender: TObject);
    procedure QryAngajamenteAfterOpen(DataSet: TDataSet);
    procedure tcVizaChange(Sender: TObject);
    procedure CmdVizaTrezorerieExecute(Sender: TObject);
    procedure CmdDevizareTrezorerieExecute(Sender: TObject);
    procedure pnTopResize(Sender: TObject);
  private
    { Private declarations }
    FIdAngajament  : Integer;
    FirstRunThis : Boolean;
    FIdAngajamentParinte: Integer;
    FIsOrdonantare: Integer;
    procedure SetIsOrdonantare(const Value: Integer);
    function GetCodEconomic: String;
    function GetCodFunctional: String;
    function GetIdOiProiecte: Integer;
    function GetIdOiUnitati: Integer;
    procedure SetCodEconomic(const Value: String);
    procedure SetCodFunctional(const Value: String);
    procedure SetIdOiProiecte(const Value: Integer);
    procedure SetIdOiUnitati(const Value: Integer);
  protected
    procedure UpdateAng(AIdAngajament : Integer; ANrContract : Variant; ADataContract : Variant; AIdContract : Variant);
    procedure ReportClick(Sender: TObject);
  public
    { Public declarations }
    function GetSQLCondition: String;
    procedure RefreshScreen;
    procedure RefreshData;
    property IdAngajament : Integer read FIdAngajament;
    property IdAngajamentParinte : Integer read FIdAngajamentParinte;
    property IsOrdonantare : Integer read FIsOrdonantare write SetIsOrdonantare;
    property IdOiUnitati   : Integer read GetIdOiUnitati   write SetIdOiUnitati;
    property IdOiProiecte  : Integer read GetIdOiProiecte  write SetIdOiProiecte;
    property CodFunctional : String  read GetCodFunctional write SetCodFunctional;
    property CodEconomic   : String  read GetCodEconomic   write SetCodEconomic;    
  end;


function SelectieAngajament(const aIsOrdonantare : Integer = 1) : Integer;


implementation

uses
  ATSZDBUtils, dxCompsUtile, ZeosDBUtile, AlopAngajamente, CommonDBVar,
  cxStorage, PersistGridSettings, frmSelectieContractUnit, RapInclude, FisaDetaliuUnit, frmProgressUnit, frxClass, DateUnit, MainUnit;

{$R *.dfm}

procedure TfrmAlopAngajamenteVizualizare.BtnModificareClick(
  Sender: TObject);
begin
 if (FIdAngajament <> -1) and OkToModify(FIdAngajament) then begin
   ModificareAngajament(FIdAngajament);
   btnRefreshClick(nil);
 end;
end;

procedure TfrmAlopAngajamenteVizualizare.FormCreate(Sender: TObject);
begin

  FirstRunThis := True;

  PopulateReportContext('Rapoarte Angajamente', btnRapoarte, ReportClick);

  FillImageComboFmt(edOperator.Properties       , 'exec [spAlopListaOperatoriAngajamente] %d, %d' , [IdLogin, IdUtilizator], 'ID_UTILIZATORI' , 'NUMEINTREG' , Null, 'Toti Utilizatorii');
  FillImageComboFmt(edUnitate.Properties        , 'exec [spAlopListaUnitatiAngajamente]   %d, %d' , [IdLogin, IdUtilizator], 'id_oi_unitati'  , 'denumire'   , Null, 'Toate Unitatile');
  FillImageComboFmt(edProiect.Properties        , 'exec [spAlopListaProiecteAngajamente]  %d, %d' , [IdLogin, IdUtilizator], 'id_oi_proiecte' , 'denumire'   , Null, 'Toate Proiectele');
  FillImageComboFmt(edCodFunctional.Properties  , 'exec [spAlopListaCFAngajamente]        %d, %d' , [IdLogin, IdUtilizator], 'cod_functional' , 'denumire'   , Null, 'Toate Clasificatiile Functionale');
  FillImageComboFmt(edCodEconomic.Properties    , 'exec [spAlopListaCEAngajamente]        %d, %d' , [IdLogin, IdUtilizator], 'cod_economic'   , 'denumire'   , Null, 'Toate Clasificatiile Economice');
  edData.EditValue := Date();

  FIsOrdonantare := 0;
  FIdAngajament := -1;
  FIdAngajamentParinte := -1;

end;

procedure TfrmAlopAngajamenteVizualizare.RefreshScreen;
begin

  if FirstRunThis then begin
  
    if FormStyle = fsNormal then
      edOperator.EditValue  := IdUtilizator
    else
      edOperator.EditValue  := Null;
      
    edProiect.EditValue       := Null;
    edUnitate.EditValue       := Null;
    edCodFunctional.EditValue := Null;
    edCodEconomic.EditValue   := Null;

  end;

  RefreshData;
  DBRefresh(qryDetaliuEconomic);

  if viewAngajamente.ItemCount > 0 then viewAngajamente.Items[0].Focused := True;

  if FirstRunThis then begin
    cxCreateMissingColumns(qryDetaliuEconomic, viewDetaliuEconomic);
    cxCreateMissingColumns(QryAngajamente, viewAngajamente);
    InitVisibleColumns(Self, viewDetaliuEconomic);
    InitVisibleColumns(Self, viewAngajamente);
    InitVisibleColumns(Self, viewIstoricAngajamente);

    StorageReadCxView(viewAngajamente);
    StorageReadCxView(viewIstoricAngajamente);
    StorageReadCxView(viewDetaliuEconomic);

    FirstRunThis := False;
  end;

end;

procedure TfrmAlopAngajamenteVizualizare.BtnOkClick(Sender: TObject);
begin
  if fsModal in FormState then begin
    if (IsOrdonantare>0) or OkToModify(FIdAngajament) then ModalResult := mrOk
                                                      else ModalResult := mrCancel;
  end
  else Close;
end;

procedure TfrmAlopAngajamenteVizualizare.BtnCancelClick(Sender: TObject);
begin
  if fsModal in FormState then ModalResult := mrCancel
  else Close;
end;

procedure TfrmAlopAngajamenteVizualizare.FormDestroy(Sender: TObject);
begin
  EmptyTipFise(tcViza.Tabs);
  StorageWriteCxView(viewAngajamente);
  StorageWriteCxView(viewIstoricAngajamente);
  StorageWriteCxView(viewDetaliuEconomic);
end;

function SelectieAngajament(const aIsOrdonantare : Integer) : Integer;
begin
  with TfrmAlopAngajamenteVizualizare.Create(nil) do
  try
    //if aIsOrdonantare > 0 then tcViza.Visible := False;
    IsOrdonantare := aIsOrdonantare;
    BtnModificare.Visible := False;
    btnAnuleazaAng.Visible := False;
    btnRapoarte.Visible := False;
    BtnOk.Visible := True;
    BtnCancel.Visible := True;
    WindowState := wsMaximized;
    if aIsOrdonantare = 1 then begin
        viewAngajamente.DataController.Filter.Root.AddItem(viewAngajamenteRamasOrdonantat, foNotEqual, 0, '0');
        viewIstoricAngajamente.DataController.Filter.Active := True;
    end;
    ShowModal;
    if ModalResult = mrOk then Result := IdAngajament else Result := -1;
  finally
    Free;
  end;
end;

procedure TfrmAlopAngajamenteVizualizare.btnAnuleazaAngClick(
  Sender: TObject);
var
  lNrAng : String;
  lDataAng : String;
begin
 if (FIdAngajament <> -1) and OkToModify(FIdAngajament) then begin
    lNrAng := QryAngajamente.FieldByName('NUMAR').AsString;
    lDataAng := QryAngajamente.FieldByName('DATA_EMITERE').AsString;
    if (MessageDlg(Format('Doriti stergerea angajamentului nr. : %s din data  %s ?', [
        lNrAng, lDataAng]), mtConfirmation, [mbYes, mbNo], 0) in [mrNo, mrNone]) then
       Abort;
    DBExecSQLFmt('exec [spAlopAnuleazaAngajament] %d', [FIdAngajament]);
    QryAngajamente.Close;
    qryIstoricAngajament.Close;
    DBRefresh(QryAngajamente);
    DBRefresh(qryIstoricAngajament);
 end;
end;

procedure TfrmAlopAngajamenteVizualizare.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmAlopAngajamenteVizualizare.btnRefreshClick(Sender: TObject);
begin
   RefreshScreen;
end;

 procedure TfrmAlopAngajamenteVizualizare.edProiectPropertiesChange(
  Sender: TObject);
begin
  if not FirstRunThis then
    RefreshData;
end;

procedure TfrmAlopAngajamenteVizualizare.FormShow(Sender: TObject);
begin
  RefreshScreen;
end;

procedure TfrmAlopAngajamenteVizualizare.pnBottomResize(Sender: TObject);
begin
  BtnCancel.Left := pnBottom.Width - BtnCancel.Width - 5;
  BtnOk.Left := BtnCancel.Left - BtnOk.Width - 2;
  btnRapoarte.Left := BtnOk.Left - btnRapoarte.Width - 2;
end;

procedure TfrmAlopAngajamenteVizualizare.viewAngajamenteFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
  if not Assigned(AFocusedRecord) then Exit;
  if not AFocusedRecord.IsData  then Exit;
  if fsCreating in FormState then Exit;
  if (FIdAngajament = GetInteger(AFocusedRecord, viewAngajamenteID_ALOP_ANGAJAMENTE.Index))
     and (qryDetaliuEconomic.Params.ParamByName('LEVEL').Value = 0)  then Exit;
  BtnOk.Enabled := False;
  FIdAngajament := GetInteger(AFocusedRecord, viewAngajamenteID_ALOP_ANGAJAMENTE.Index);
  if (FIdAngajament <> qryDetaliuEconomic.Params.ParamByName('ID_ALOP_ANGAJAMENTE').Value) or
      (qryDetaliuEconomic.Params.ParamByName('LEVEL').Value <> 0) then
  begin
    qryDetaliuEconomic.Close;
    qryDetaliuEconomic.Params.ParamByName('ID_ALOP_ANGAJAMENTE').Value := FIdAngajament;
    qryDetaliuEconomic.Params.ParamByName('LEVEL').Value := 0;
    qryDetaliuEconomic.Open;
  end;
  BtnOk.Enabled := (FIdAngajament <> -1);
end;

procedure TfrmAlopAngajamenteVizualizare.viewIstoricAngajamenteFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
  if not Assigned(AFocusedRecord) then Exit;
  if not AFocusedRecord.IsData  then Exit;
  if fsCreating in FormState then Exit;
  if (FIdAngajament = GetInteger(AFocusedRecord, viewIstoricAngajamenteID_ALOP_ANGAJAMENTE.Index))
     and (qryDetaliuEconomic.Params.ParamByName('LEVEL').Value = 1)  then Exit;

  BtnOk.Enabled := False;
  FIdAngajament := GetInteger(AFocusedRecord, viewIstoricAngajamenteID_ALOP_ANGAJAMENTE.Index);
  if (FIdAngajament <> qryDetaliuEconomic.Params.ParamByName('ID_ALOP_ANGAJAMENTE').Value) or
      (qryDetaliuEconomic.Params.ParamByName('LEVEL').Value <> 1) then
  begin
    qryDetaliuEconomic.Close;
    qryDetaliuEconomic.Params.ParamByName('ID_ALOP_ANGAJAMENTE').Value := FIdAngajament;
    qryDetaliuEconomic.Params.ParamByName('LEVEL').Value := 1;
    qryDetaliuEconomic.Open;
  end;
  BtnOk.Enabled := (FIdAngajament <> -1);
end;

procedure TfrmAlopAngajamenteVizualizare.RefreshData;
var
  lSQLCond : string;
begin
  lSQLCond := GetSQLCondition;
  DoCheckClose(qryDetaliuEconomic);
  DoCheckClose(QryAngajamente);
  DoCheckClose(qryIstoricAngajament);
  QryAngajamente.Params.ParamByName('IsOrd').Value              := Integer(IsOrdonantare>0);
  QryAngajamente.Params.ParamByName('IdUtilizator').Value       := edOperator.EditValue;
  QryAngajamente.Params.ParamByName('IdAnalitic').Value         := edProiect.EditValue;
  QryAngajamente.Params.ParamByName('tipViza').Value            := lSQLCond;
  qryIstoricAngajament.Params.ParamByName('IsOrd').Value        := Integer(IsOrdonantare>0);
  qryIstoricAngajament.Params.ParamByName('IdUtilizator').Value := edOperator.EditValue;
  qryIstoricAngajament.Params.ParamByName('IdAnalitic').Value   := edProiect.EditValue;
  qryIstoricAngajament.Params.ParamByName('tipViza').Value      := lSQLCond;
  if not FirstRunThis then
    DBRefresh([qryIstoricAngajament, QryAngajamente]);
end;

procedure TfrmAlopAngajamenteVizualizare.cxGridPopupMenuPopup(
  ASenderMenu: TComponent; AHitTest: TcxCustomGridHitTest; X, Y: Integer;
  var AllowPopup: Boolean);
begin
  AddInternalPopup(cxGridPopupMenu, ASenderMenu, AHitTest, X, Y, AllowPopup);
end;

procedure TfrmAlopAngajamenteVizualizare.CmdAsociereContractExecute(
  Sender: TObject);
var
  lSelectieContract : TfrmSelectieContract;
  lRecord : TcxCustomGridRecord;
begin
  lRecord := viewAngajamente.Controller.FocusedRecord;
  if Assigned(lRecord) and lRecord.IsData then begin
    lSelectieContract := TfrmSelectieContract.Create(nil);
    try
      lSelectieContract.RefreshContracte;
      lSelectieContract.IdPredator := QryAngajamente.FieldByName('id_departament').AsInteger;
      lSelectieContract.IdPrimitor := QryAngajamente.FieldByName('id_lst_repartitori').AsInteger;
      lSelectieContract.IdContract := QryAngajamente.FieldByName('id_contract').AsInteger;
      lSelectieContract.edNrContract.EditValue := QryAngajamente['nr_contract'];
      lSelectieContract.edDataContract.EditValue := QryAngajamente['data_contract'];
      lSelectieContract.IdAngajament := QryAngajamente.FieldByName('id').AsInteger;
      lSelectieContract.FilterContractByDepartament(lSelectieContract.IdPredator);
      lSelectieContract.FilterContractByPrestator(lSelectieContract.IdPrimitor, False);
      lSelectieContract.ShowModal;
      if lSelectieContract.ModalResult = mrOk then begin
        UpdateAng(lSelectieContract.IdAngajament, lSelectieContract.NrContract, lSelectieContract.DataContract, lSelectieContract.IdContract);
      end;
    finally
      {lSelectieContract.FilterContractByDepartament(-1);
      lSelectieContract.FilterContractByPrestator(-1);}
      lSelectieContract.Free;
    end;
  end;
end;

procedure TfrmAlopAngajamenteVizualizare.UpdateAng(AIdAngajament: Integer;
  ANrContract, ADataContract, AIdContract: Variant);
var
  lIdContract: Integer;
begin
  if ValueIsTrue(DBGetSetare('integrareOne')) and (AIdContract < 0) then begin
    lIdContract := AIdContract;
    DBExecSQLFmt('update alop_angajamente set nr_contract = %s, data_contract = %s, ref_One_TipProgram = %d, ref_One_Contract = %d, id_contract = null where id_alop_angajamente = %d',
                  [
                    ValueToStr(ANrContract),
                    ValueToStr(ADataContract),
                    (-1 * lIdContract) mod 100,
                    (-1 * lIdContract) div 100,
                    AIdAngajament]);
  end
  else begin
    qryAngUpdContract.ParamByName('idAng').Value := AIdAngajament;
    qryAngUpdContract.ParamByName('NrContract').Value := ANrContract;
    qryAngUpdContract.ParamByName('DataContract').Value := ADataContract;
    qryAngUpdContract.ParamByName('IdContract').Value := AIdContract;

    qryAngUpdContract.ExecSQL;
  end;

  if not (QryAngajamente.State in dsEditModes) then QryAngajamente.Edit;
  QryAngajamente.FieldByName('nr_contract').Value := ANrContract;
  QryAngajamente.FieldByName('data_contract').Value := ADataContract;
  if (QryAngajamente.State in dsEditModes) then QryAngajamente.Post;
end;

procedure TfrmAlopAngajamenteVizualizare.CmdDezasociereContractExecute(
  Sender: TObject);
var
  lRecord : TcxCustomGridRecord;
begin
  lRecord := viewAngajamente.Controller.FocusedRecord;
  if Assigned(lRecord) and lRecord.IsData then begin
    if MessageDlg(Format('Doriti dezasocierea contractului de la angajamentul %s - %s ?',
     [QryAngajamente.FieldByName('numar').AsString, FormatDateTime('dd/mm/yyyy', QryAngajamente.FieldByName('data_emitere').AsDateTime)])
      , mtConfirmation, [mbYes, mbNo], 0) = mrYes then
      UpdateAng(QryAngajamente.FieldByName('id').AsInteger, null, null, null);
  end;
end;

procedure TfrmAlopAngajamenteVizualizare.QryAngajamenteAfterOpen(
  DataSet: TDataSet);
begin
  DataSet.FieldByName('data_contract').ReadOnly := False;
  DataSet.FieldByName('nr_contract').ReadOnly := False;
end;

procedure TfrmAlopAngajamenteVizualizare.tcVizaChange(Sender: TObject);
begin
  if not FirstRunThis then
    RefreshData;
end;

procedure TfrmAlopAngajamenteVizualizare.CmdVizaTrezorerieExecute(
  Sender: TObject);
var
  lFrmAprob: TForm;
  Label1: TLabel;
  Label2: TLabel;
  BtnOk: TcxButton;
  BtnCancel: TcxButton;
  edDataAprobare: TcxDateEdit;
begin
  lFrmAprob := TForm.Create(Self);
  Label1 := TLabel.Create(lFrmAprob);
  Label2 := TLabel.Create(lFrmAprob);
  BtnOk := TcxButton.Create(lFrmAprob);
  BtnCancel := TcxButton.Create(lFrmAprob);
  edDataAprobare := TcxDateEdit.Create(lFrmAprob);

  with lFrmAprob do
  begin
    Left := 363;
    Top := 274;
    BorderStyle := bsDialog;
    Caption := 'Aprobare trezorerie';
    Color := clBtnFace;
    OldCreateOrder := False;
    Position := poMainFormCenter;
    PixelsPerInch := 96;
  end;
  with Label1 do
  begin
    Name := 'Label1';
    Parent := lFrmAprob;
    Left := 16;
    Top := 52;
    Width := 91;
    Height := 13;
    Caption := 'Data viza trezorerie';
  end;
  with Label2 do
  begin
    Name := 'Label2';
    Parent := lFrmAprob;
    Left := 8;
    Top := 8;
    Width := 275;
    Height := 25;
    Caption := 'Angajamentul a fost aprobat de trezorerie la data de  : ';
    ParentFont := False;
    WordWrap := True;
  end;
  with BtnOk do
  begin
    Name := 'BtnOk';
    Parent := lFrmAprob;
    Left := 138;
    Top := 104;
    Width := 65;
    Height := 26;
    Anchors := [akRight, akBottom];
    Caption := 'Ok';
    ModalResult := 1;
    TabOrder := 0;
  end;
  with BtnCancel do
  begin
    Name := 'BtnCancel';
    Parent := lFrmAprob;
    Left := 208;
    Top := 104;
    Width := 83;
    Height := 26;
    Anchors := [akRight, akBottom];
    Caption := 'Abandon';
    ModalResult := 2;
    TabOrder := 1;
  end;
  with edDataAprobare do
  begin
    Name := 'edDataAprobare';
    Parent := lFrmAprob;
    Left := 127;
    Top := 49;
    TabOrder := 2;
    Width := 121;
    Date := Now;
    Properties.SaveTime := False;
  end;

  with lFrmAprob do
  try
    ShowModal;
    if ModalResult = mrOk then begin
      DBExecSQLFmt('exec [spAlopAngVizaTrezorerie] %d, 1, %s', [IdAngajament, ValueToStr(edDataAprobare.Date)]);
      RefreshData;
    end;
  finally
    Free;
  end;
end;

procedure TfrmAlopAngajamenteVizualizare.ReportClick(Sender: TObject);
begin
  if (viewAngajamente.Controller.SelectedRowCount > 1) then begin
    if (TMenuItem(Sender).Tag <> -1) then begin
      ShowReportList(
        'Generare Raport Lista Ordonantari',
        TMenuItem(Sender).Tag,
        viewAngajamente.Controller.SelectedRowCount,
        procedure (Index: Integer; AReport: TfrxReport)
        var
          lIdAngajament: Integer;
        begin
          lIdAngajament := viewAngajamente.Controller.SelectedRows[Index].Values[viewAngajamenteID_ALOP_ANGAJAMENTE.Index];
          DateUnit.IdAngajament := lIdAngajament;
          mainForm.SetRaportParams(AReport);
        end);
    end;
  end
  else begin
    LoadReport(TMenuItem(Sender).Tag, 'IdAngajament', [IdAngajament]);
  end;
end;

procedure TfrmAlopAngajamenteVizualizare.CmdDevizareTrezorerieExecute(
  Sender: TObject);
begin
  if MessageDlg('Doriti anularea vizei pentru angajamentul curent selectat ? ', mtConfirmation, [mbYes, mbNo], 0) = mrOk then begin
    DBExecSQLFmt('exec spAlopAngVizaTrezorerie %d, 0', [IdAngajament]);
    RefreshData;
  end;
end;

procedure TfrmAlopAngajamenteVizualizare.SetIsOrdonantare(
  const Value: Integer);
begin
  FIsOrdonantare := Value;
  PopulateTipFiseBySQL('exec spAlopListaHeaderTab ' + IntToStr(IsOrdonantare), tcViza.Tabs);
  tcViza.Visible := tcViza.Tabs.Count > 0;
end;

procedure TfrmAlopAngajamenteVizualizare.pnTopResize(Sender: TObject);
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
  lGroupWidth := pnTop.Width div 6;
  lEditWidth  := lGroupWidth - 55;
  lLeft       := 0;
  SetEdit(lbUnitate       , edUnitate);
  SetEdit(lbProiect       , edProiect);
  SetEdit(lbCodFunctional , edCodFunctional);
  SetEdit(lbCodEconomic   , edCodEconomic);
  SetEdit(lbData          , edData);
  SetEdit(lbOperator      , edOperator);
end;

function TfrmAlopAngajamenteVizualizare.GetCodEconomic: String;
begin
  Result := ValueSafeToStr(edCodEconomic.EditValue);
end;

function TfrmAlopAngajamenteVizualizare.GetCodFunctional: String;
begin
  Result := ValueSafeToStr(edCodFunctional.EditValue);
end;

function TfrmAlopAngajamenteVizualizare.GetIdOiProiecte: Integer;
begin
  Result := ValueSafeToInt(edProiect.EditValue, -1);
end;

function TfrmAlopAngajamenteVizualizare.GetIdOiUnitati: Integer;
begin
  Result := ValueSafeToInt(edUnitate.EditValue, -1);
end;

procedure TfrmAlopAngajamenteVizualizare.SetCodEconomic(
  const Value: String);
begin
  edCodEconomic.EditValue := Value;
end;

procedure TfrmAlopAngajamenteVizualizare.SetCodFunctional(
  const Value: String);
begin
  edCodFunctional.EditValue := Value;
end;

procedure TfrmAlopAngajamenteVizualizare.SetIdOiProiecte(
  const Value: Integer);
begin
  edProiect.EditValue := Value;
end;

procedure TfrmAlopAngajamenteVizualizare.SetIdOiUnitati(
  const Value: Integer);
begin
  edUnitate.EditValue := Value;
end;

function TfrmAlopAngajamenteVizualizare.GetSQLCondition: String;
var
  lTipFisa : PTipFisa;
  lDefalcareSQL : String;

    procedure AddToDefalcare(const ASQL: String);
    begin
      if lDefalcareSQL > '' then
        lDefalcareSQL := lDefalcareSQL + ' and ';
      lDefalcareSQL := lDefalcareSQL + ASQL;
    end;

    procedure AddToWhere(const ASQL: String);
    begin
      if Result > '' then
        Result := Result + ' and ';
      Result := Result + ASQL;
    end;

begin
  Result := '';
  lDefalcareSQL := '';
  if (tcViza.TabIndex > -1) and (tcViza.TabIndex < tcViza.Tabs.Count) then begin
    lTipFisa := PTipFisa(TStrings(tcViza.Tabs).Objects[tcViza.TabIndex]);
    if Assigned(lTipFisa) then
      Result := lTipFisa^.SQLConditie;
  end;
  if ValueHasValue(edUnitate.EditValue) then
    AddToDefalcare('id_oi_unitati = ' + ValueToStr(edUnitate.EditValue));
  if ValueHasValue(edProiect.EditValue) then
    AddToDefalcare('id_oi_proiecte = ' + ValueToStr(edProiect.EditValue));
  if ValueHasValue(edCodFunctional.EditValue) then
    AddToWhere('cod_functional = ' + ValueToStr(edCodFunctional.EditValue));
  if ValueHasValue(edCodEconomic.EditValue) then
    AddToDefalcare('cod_economic = ' + ValueToStr(edCodEconomic.EditValue));
  if ValueHasValue(edData.EditValue) then
    AddToWhere('data_emitere <= ' + ValueToStr(edData.EditValue));
  if lDefalcareSQL > '' then
    AddToWhere(Format('id_alop_angajamente in (select id_alop_angajamente from alop_angajamente_defalcare where %s)', [lDefalcareSQL]));
end;

end.
