unit frmOERepartioriEditUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, Menus,
  cxLookAndFeelPainters, ExtCtrls, StdCtrls, cxGraphics, cxControls,
  cxLookAndFeels, cxContainer, cxEdit, cxStyles, cxCustomData, cxFilter,
  cxData, cxDataStorage, DB, cxDBData, cxCurrencyEdit, cxDBLookupComboBox,
  dxmdaset, ZAbstractRODataset, ZAbstractDataset, ZDataset, cxCheckBox,
  cxDBEdit, cxCheckListBox, cxButtonEdit, cxMaskEdit, cxDropDownEdit,
  cxImageComboBox, cxGridLevel, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxClasses, cxGridCustomView, cxGrid, cxPC,
  cxRadioGroup, cxButtons, cxTextEdit, Mask, DBCtrls,
  dxBarBuiltInMenu, cxNavigator, cxLookupEdit, cxDBLookupEdit,
  cxDataControllerConditionalFormattingRulesManagerDialog, cxCustomListBox,
  dxDateRanges, dxScrollbarAnnotations, DBXJSON, Vcl.ComCtrls;
 type
  TAfterSaveEvent = procedure of object;
type

  TfrmOERepartitoriEdit = class(TForm)
    pnTop: TPanel;
    edtCUI: TcxTextEdit;
    btnCautare: TcxButton;
    rbCautareCUI: TcxRadioButton;
    rbCautareNume: TcxRadioButton;
    edtNume: TcxTextEdit;
    btnSearch: TcxButton;
    DTSearch: TDataSource;
    qrySearch: TZQuery;
    cxPageContent: TcxPageControl;
    tabCautare: TcxTabSheet;
    cxGridSearch: TcxGrid;
    GridSearch: TcxGridDBTableView;
    GridSearchID_REPARTITORI: TcxGridDBColumn;
    GridSearchCODSECTIE: TcxGridDBColumn;
    GridSearchNUME: TcxGridDBColumn;
    GridSearchADRESA: TcxGridDBColumn;
    GridSearchCONT: TcxGridDBColumn;
    GridSearchCONT_CEC: TcxGridDBColumn;
    GridSearchBANCA: TcxGridDBColumn;
    GridSearchCODCLASM: TcxGridDBColumn;
    GridSearchCOD_FISCAL: TcxGridDBColumn;
    GridSearchREG_COMERT: TcxGridDBColumn;
    GridSearchID_TARI: TcxGridDBColumn;
    GridSearchID_JUDETE: TcxGridDBColumn;
    GridSearchTELEFON: TcxGridDBColumn;
    GridSearchFAX: TcxGridDBColumn;
    GridSearchEMAIL: TcxGridDBColumn;
    GridSearchCOMERCIANT: TcxGridDBColumn;
    GridSearchGESTINT: TcxGridDBColumn;
    GridSearchCOTA_DISCOUNT: TcxGridDBColumn;
    GridSearchCOTA_ADAOS: TcxGridDBColumn;
    GridSearchDATA_STOC_INI: TcxGridDBColumn;
    GridSearchDATA_SOLD_INI: TcxGridDBColumn;
    GridSearchSOLD_INITIAL: TcxGridDBColumn;
    GridSearchSNM: TcxGridDBColumn;
    GridSearchCONT_CRSP: TcxGridDBColumn;
    GridSearchPREFERAT: TcxGridDBColumn;
    GridSearchID_GEST_TIP_GEST: TcxGridDBColumn;
    GridSearchTIP_GESTIUNE: TcxGridDBColumn;
    GridSearchGRUP_LJ: TcxGridDBColumn;
    GridSearchID_UTILIZATORI: TcxGridDBColumn;
    GridSearchID_PARINTE: TcxGridDBColumn;
    GridSearchPRELUAT: TcxGridDBColumn;
    GridSearchnume_vechi: TcxGridDBColumn;
    GridSearchCLASA_FUNCTIONALA: TcxGridDBColumn;
    GridSearchTIP_REPARTITOR: TcxGridDBColumn;
    GridSearchCOD_FUNCTIONAL: TcxGridDBColumn;
    GridSearchcod_mifgest: TcxGridDBColumn;
    GridSearchcod_module: TcxGridDBColumn;
    GridSearchL: TcxGridLevel;
    tabDetaliiPrimare: TcxTabSheet;
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    Label4: TLabel;
    edDBTara: TcxDBImageComboBox;
    edDBJudet: TcxDBImageComboBox;
    Label5: TLabel;
    Label6: TLabel;
    edDBCUI: TcxDBTextEdit;
    edDBNume: TcxDBTextEdit;
    edDBNrComert: TcxDBTextEdit;
    edDBAdresa: TcxDBTextEdit;
    tabBanca: TcxTabSheet;
    GridBanca: TcxGrid;
    GridBancaV: TcxGridDBTableView;
    GridBancaVID_REPARTITORI_CONTURI: TcxGridDBColumn;
    GridBancaVCONT: TcxGridDBColumn;
    GridBancaVBANCA_DENUMIRE: TcxGridDBColumn;
    GridBancaVBANCA_COD: TcxGridDBColumn;
    GridBancaVDEFAULT_CONT: TcxGridDBColumn;
    GridBancaL: TcxGridLevel;
    btnBancaAdd: TcxButton;
    btnBancaModifca: TcxButton;
    btnBancaSterge: TcxButton;
    btnBancaDefault: TcxButton;
    tabContabilitate: TcxTabSheet;
    Label7: TLabel;
    Label8: TLabel;
    Label9: TLabel;
    GridConta: TcxGrid;
    GridContaV: TcxGridDBTableView;
    GridContaVCONT: TcxGridDBColumn;
    GridContaVSOLD: TcxGridDBColumn;
    GridContaVSOLD_DEBITOR: TcxGridDBColumn;
    GridContaVSOLD_CREDITOR: TcxGridDBColumn;
    GridContaL: TcxGridLevel;
    edtSoldDebit: TcxDBCurrencyEdit;
    edtSoldCredit: TcxDBCurrencyEdit;
    btnCAdd: TcxButton;
    btnCDel: TcxButton;
    btnCUpdate: TcxButton;
    edtCont: TcxDBImageComboBox;
    tabImplicit: TcxTabSheet;
    Label10: TLabel;
    Label11: TLabel;
    btnCFAdd: TcxButton;
    btnCFDel: TcxButton;
    btnCFUpd: TcxButton;
    edCE: TcxDBButtonEdit;
    edCF: TcxDBButtonEdit;
    GridCF: TcxGrid;
    GridCFV: TcxGridDBTableView;
    GridCFVID_REPARTITORI_BUGET: TcxGridDBColumn;
    GridCFVID_REPARTITORI: TcxGridDBColumn;
    GridCFVCOD_FUNCTIONAL: TcxGridDBColumn;
    GridCFVCOD_ECONOMIC: TcxGridDBColumn;
    GridCFVID_OI_PROIECTE: TcxGridDBColumn;
    GridCFVID_OI_UNITATI: TcxGridDBColumn;
    GridCFL: TcxGridLevel;
    btnPlanificare: TcxButton;
    DTCF: TDataSource;
    QryCF: TZQuery;
    DTBanca: TDataSource;
    qryBanca: TZQuery;
    pnBottom: TPanel;
    lbOras: TLabel;
    edDBOras: TcxDBImageComboBox;
    Label15: TLabel;
    edDBGrupa: TcxDBImageComboBox;
    Label16: TLabel;
    edDBDomeniu: TcxDBImageComboBox;
    Label12: TLabel;
    edtCodFunctional: TcxDBButtonEdit;
    Label17: TLabel;
    edDBPersoanaContact: TcxDBTextEdit;
    Label18: TLabel;
    edDBTelefon: TcxDBTextEdit;
    lbFax: TLabel;
    edDBFax: TcxDBTextEdit;
    Label20: TLabel;
    edDBeMail: TcxDBTextEdit;
    lbwww: TLabel;
    edDBWWW: TcxDBTextEdit;
    Label22: TLabel;
    btnSalvare: TcxButton;
    DTSolduriRep: TDataSource;
    QrySoldRep: TZQuery;
    tabTipuri: TcxTabSheet;
    edtTipRep: TcxCheckListBox;
    edTipGestiune: TcxDBImageComboBox;
    lbTipGestiune: TLabel;
    ChkTipGest: TcxDBCheckBox;
    MemFinante: TdxMemData;
    tabResult: TcxTabSheet;
    DTFinante: TDataSource;
    GridInfo: TcxGridDBTableView;
    cxGridInfoLevel: TcxGridLevel;
    cxGridInfo: TcxGrid;
    GridInfoCOD_FISCAL: TcxGridDBColumn;
    GridInfoNUME: TcxGridDBColumn;
    GridInfoADRESA: TcxGridDBColumn;
    GridInfoJUDET: TcxGridDBColumn;
    GridInfoIDJudet: TcxGridDBColumn;
    GridInfoNR_REG_COMERT: TcxGridDBColumn;
    GridInfoCOD_POSTAL: TcxGridDBColumn;
    GridInfoTELEFON: TcxGridDBColumn;
    GridInfoFAX: TcxGridDBColumn;
    GridInfoIsProcessed: TcxGridDBColumn;
    Label13: TLabel;
    edDBID: TcxDBTextEdit;
    Label19: TLabel;
    qryRefresh: TZQuery;
    rbCautareVies: TcxRadioButton;
    edtViesCUI: TcxTextEdit;
    edtVIESTara: TcxImageComboBox;
    edtJudet: TcxImageComboBox;
    DTRepartitori: TDataSource;
    Label21: TLabel;
    edtUnitate: TcxDBLookupComboBox;
    edCodIntern: TcxDBTextEdit;
    cxCheckBox1: TcxCheckBox;
    edTipTva: TcxDBImageComboBox;
    Label14: TLabel;
    ProgressBar1: TProgressBar;
    btnFacturi: TcxButton;

    procedure btnSearchClick(Sender: TObject);
    procedure rbCautareCUIClick(Sender: TObject);
    procedure GridSearchDblClick(Sender: TObject);
    procedure btnSalvareClick(Sender: TObject);
    procedure edtSoldDebitEnter(Sender: TObject);
    procedure btnCAddClick(Sender: TObject);
    procedure btnCDelClick(Sender: TObject);
    procedure btnCUpdateClick(Sender: TObject);
    procedure edCFPropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
    procedure edCEPropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
    procedure btnCFAddClick(Sender: TObject);
    procedure btnCFDelClick(Sender: TObject);
    procedure btnCFUpdClick(Sender: TObject);
    procedure QrySoldRepBeforeDelete(DataSet: TDataSet);
    procedure DTSolduriRepStateChange(Sender: TObject);
    procedure GridBancaVCustomDrawCell(Sender: TcxCustomGridTableView;
      ACanvas: TcxCanvas; AViewInfo: TcxGridTableDataCellViewInfo;
      var ADone: Boolean);
    procedure ChkTipGestPropertiesChange(Sender: TObject);
    procedure tabBancaResize(Sender: TObject);
    procedure QrySoldRepBeforePost(DataSet: TDataSet);
    procedure QryCFNewRecord(DataSet: TDataSet);
    procedure btnCautareClick(Sender: TObject);
    procedure btnBancaAddClick(Sender: TObject);
    procedure btnBancaModifcaClick(Sender: TObject);
    procedure btnBancaStergeClick(Sender: TObject);
    procedure btnBancaDefaultClick(Sender: TObject);
    procedure edDBJudetPropertiesEditValueChanged(Sender: TObject);
    procedure btnFacturiClick(Sender: TObject);
   // procedure cxButton1Click(Sender: TObject);

  private
    { Private declarations }
    FIdRepartitor: Integer;
    FIsReadOnly: Boolean;
    FOnAfterSave: TAfterSaveEvent;
     FIsFromAPI: Boolean;
     FNomenclatorLoaded: Boolean;
    procedure PopulateNomenclator;
    procedure LoadLocalitatiByJudet(ID_Judete: Integer);

    procedure PopulateTipuriRepartitori;
    procedure PopulateTipTva;
    procedure PopulateTipGest;
    procedure SetNewTipRep;
    procedure PopulateGrupaRepartitori;
   
    procedure SetIdRepartitor(const Value: Integer);
    procedure SetIsReadOnly(const Value: Boolean);

  procedure LoadNomenclatorFromDB;
  procedure RefreshNomenclator;
  protected
    procedure CompleteInfoCodFiscal(aCodFiscal : String);
  public
    { Public declarations }
    procedure RefreshRow(lIdRepartitor:Integer);
    procedure SetInfoConfig;
    procedure SetEditConfig;
    procedure SetTipRep(NewState: Boolean; lTipRep: Integer);
    property OnAfterSave: TAfterSaveEvent read FOnAfterSave write FOnAfterSave;
    property IdRepartitor : Integer read FIdRepartitor write SetIdRepartitor;
    property IsReadOnly : Boolean read FIsReadOnly write SetIsReadOnly;
  end;


implementation

{$R *.dfm}


uses
  RepartitorAnafUnit, dxCompsUtile, ZeosDBUtile, dateUnit, CommonDBVar, ATSZDBUtils,
 RepartitorContBanca, MainUnit, SelBugetUnit , uVies,TFormFacturi,AlopAngDisponibil;


procedure TfrmOERepartitoriEdit.btnSearchClick(Sender: TObject);
begin
  cxPageContent.ActivePage := tabCautare;
  with qrySearch do
  try
    DisableControls;
    qrySearch.AfterScroll := nil;
    if Active then Close;
    if rbCautareCUI.Checked then
    begin
      Params.ParamByName('nume').Value := null;
      Params.ParamByName('cui').Value := edtCUI.Text;
    end
    else
    begin
      Params.ParamByName('nume').Value := edtNume.Text;
      Params.ParamByName('cui').Value := null;
    end;
    Open;
  finally
    EnableControls;
    First;
  end;
end;

 procedure TfrmOERepartitoriEdit.LoadNomenclatorFromDB;
begin
  if FNomenclatorLoaded then
    Exit;

  // Populare TARI
  edDBTara.Properties.Items.Clear;
  with TZQuery.Create(nil) do
  try
    Connection := frmData.dbContabilitate;
    SQL.Text := 'SELECT ID_TARI, DENUMIRE FROM TARI ORDER BY DENUMIRE';
    Open;
    while not Eof do
    begin
      with edDBTara.Properties.Items.Add do
      begin
        Description := FieldByName('DENUMIRE').AsString;
        Value := FieldByName('ID_TARI').AsInteger;
      end;
      Next;
    end;
  finally
    Free;
  end;

  // Populare JUDETE
  edDBJudet.Properties.Items.Clear;
  with TZQuery.Create(nil) do
  try
    Connection := frmData.dbContabilitate;
    SQL.Text := 'SELECT ID_JUDETE, DENUMIRE FROM JUDETE ORDER BY DENUMIRE';
    Open;
    while not Eof do
    begin
      with edDBJudet.Properties.Items.Add do
      begin
        Description := FieldByName('DENUMIRE').AsString;
        Value := FieldByName('ID_JUDETE').AsInteger;
      end;
      Next;
    end;
  finally
    Free;
  end;

  // Populare LOCALITATI
//  edDBOras.Properties.Items.Clear;
//  with TZQuery.Create(nil) do
//  try
//    Connection := frmData.dbContabilitate;
//    SQL.Text := 'SELECT ID_LOCALITATI, DENUMIRE FROM LOCALITATI ORDER BY DENUMIRE';
//    Open;
//    while not Eof do
//    begin
//      with edDBOras.Properties.Items.Add do
//      begin
//        Description := FieldByName('DENUMIRE').AsString;
//        Value := FieldByName('ID_LOCALITATI').AsInteger;
//      end;
//      Next;
//    end;
//  finally
//    Free;
//  end;

  FNomenclatorLoaded := True;
end;
  procedure TfrmOERepartitoriEdit.LoadLocalitatiByJudet(ID_Judete: Integer);
begin
  edDBOras.Properties.Items.Clear;
  with TZQuery.Create(nil) do
  try
    Connection := frmData.dbContabilitate;
    SQL.Text := 'SELECT ID_LOCALITATI, DENUMIRE FROM LOCALITATI WHERE ID_JUDETE = :ID ORDER BY DENUMIRE';
    ParamByName('ID').AsInteger := ID_Judete;
    Open;
    while not Eof do
    begin
      with edDBOras.Properties.Items.Add do
      begin
        Description := FieldByName('DENUMIRE').AsString;
        Value := FieldByName('ID_LOCALITATI').AsInteger;
      end;
      Next;
    end;
  finally
    Free;
  end;
end;

procedure TfrmOERepartitoriEdit.rbCautareCUIClick(Sender: TObject);
begin

  edtCUI.Enabled := rbCautareCUI.Checked;
  edtNume.Enabled := rbCautareNume.Checked;
  edtJudet.Enabled := rbCautareNume.Checked;
  edtViesCUI.Enabled := rbCautareVies.Checked;
  edtVIESTara.Enabled := rbCautareVies.Checked;  
end;

procedure TfrmOERepartitoriEdit.GridSearchDblClick(Sender: TObject);
begin
  if tabDetaliiPrimare.TabVisible then
    cxPageContent.ActivePage := tabDetaliiPrimare;
end;

procedure TfrmOERepartitoriEdit.PopulateNomenclator;
begin
  // Populare TARI
  edDBTara.Properties.Items.Clear;
  with TZQuery.Create(nil) do
  try
    Connection := frmData.dbContabilitate;
    SQL.Text := 'SELECT ID_TARI, DENUMIRE FROM TARI ORDER BY DENUMIRE';
    Open;
    while not Eof do
    begin
      with edDBTara.Properties.Items.Add do
      begin
        Description := FieldByName('DENUMIRE').AsString;
        Value := FieldByName('ID_TARI').AsInteger;
      end;
      Next;
    end;
  finally
    Free;
  end;

  // Populare JUDETE
  edDBJudet.Properties.Items.Clear;
  with TZQuery.Create(nil) do
  try
    Connection := frmData.dbContabilitate;
    SQL.Text := 'SELECT ID_JUDETE, DENUMIRE FROM JUDETE ORDER BY DENUMIRE';
    Open;
    while not Eof do
    begin
      with edDBJudet.Properties.Items.Add do
      begin
        Description := FieldByName('DENUMIRE').AsString;
        Value := FieldByName('ID_JUDETE').AsInteger;
      end;
      Next;
    end;
  finally
    Free;
  end;

  // Populare LOCALITATI
  edDBOras.Properties.Items.Clear;
  with TZQuery.Create(nil) do
  try
    Connection := frmData.dbContabilitate;
    SQL.Text := 'SELECT ID_LOCALITATI, DENUMIRE FROM LOCALITATI ORDER BY DENUMIRE';
    Open;
    while not Eof do
    begin
      with edDBOras.Properties.Items.Add do
      begin
        Description := FieldByName('DENUMIRE').AsString;
        Value := FieldByName('ID_LOCALITATI').AsInteger;
      end;
      Next;
    end;
  finally
    Free;
  end;
end;



procedure TfrmOERepartitoriEdit.btnSalvareClick(Sender: TObject);
var
  i: Integer;
  selectedGrupa, selectedTipTVA: Variant;
  lChecked: Boolean;
  lTipRepId: Integer;
  qryCheck, qrySave: TZQuery;
  tipPlatitorTVA: Integer;
  repartitorExists: Boolean;
begin
  btnSalvare.Enabled := False;
  selectedTipTVA := Null;
  selectedGrupa := Null;

  for i := 0 to edtTipRep.Items.Count - 1 do
  begin
    lChecked := edtTipRep.Items[i].Checked;
    lTipRepId := edtTipRep.Items[i].Tag;
    SetTipRep(lChecked, lTipRepId);
  end;

  if edDBGrupa.ItemIndex <> -1 then
    selectedGrupa := edDBGrupa.Properties.Items[edDBGrupa.ItemIndex].Value
  else
    selectedGrupa := Null;

  if edTipTva.ItemIndex <> -1 then
    selectedTipTVA := edTipTva.Properties.Items[edTipTva.ItemIndex].Value
  else
    selectedTipTVA := Null;

  tipPlatitorTVA := Integer(cxCheckBox1.Checked);

  if Trim(edDBCUI.Text) = '' then
  begin
//    ShowMessage('Eroare: CUI-ul nu poate fi gol!');
//    Exit;
  end;


  qryCheck := TZQuery.Create(nil);
  try
    qryCheck.Connection := frmData.dbContabilitate;
    qryCheck.SQL.Text := 'SELECT ID_REPARTITORI FROM repartitori WHERE COD_FISCAL = :CUI';
    qryCheck.ParamByName('CUI').AsString := Trim(edDBCUI.Text);
    qryCheck.Open;

    repartitorExists := not qryCheck.IsEmpty;
  finally
    qryCheck.Free;
  end;


  qrySave := TZQuery.Create(nil);
  try
    qrySave.Connection := frmData.dbContabilitate;

    if FIdRepartitor > 0 then
    begin
      // Modificare repartitor existent
   qrySave.SQL.Text :=
  'UPDATE dbo.repartitori SET ' +
  'id_repartitori_grupe = :grupa, ' +
  'codsectie = :codsectie, ' +
  'tip_platitor_tva = :platitor_tva, ' +
  'tip_tva = :tip_tva, ' +
  'nume = :denumire, ' +
  'adresa = :adresa, ' +
  'REG_COMERT = :nrRegCom, ' +
  'GESTINT = :gestiune_interna, ' +
  'COD_FISCAL = :cui, ' +
  'id_tari = :id_tari, ' +
  'id_judete = :id_judete, ' +
  'id_localitati = :id_localitati ' +
  'WHERE ID_REPARTITORI = :idrep';


      qrySave.ParamByName('idrep').AsInteger := FIdRepartitor;
    end
    else
    begin
      // Inserare nou
    qrySave.SQL.Text :=
  'INSERT INTO dbo.repartitori ' +
  '(id_repartitori_grupe, codsectie, tip_platitor_tva, tip_tva, nume, adresa, REG_COMERT, GESTINT, COD_FISCAL, id_tari, id_judete, id_localitati) ' +
  'VALUES (:grupa, :codsectie, :platitor_tva, :tip_tva, :denumire, :adresa, :nrRegCom, :gestiune_interna, :cui, :id_tari, :id_judete, :id_localitati)';

    end;

    // Setare parametri comuni
    qrySave.ParamByName('denumire').AsString := Trim(edDBNume.Text);
    qrySave.ParamByName('adresa').AsString := Trim(edDBAdresa.Text);
    qrySave.ParamByName('nrRegCom').AsString := Trim(edDBNrComert.Text);
    qrySave.ParamByName('cui').AsString := Trim(edDBCUI.Text);
    qrySave.ParamByName('platitor_tva').AsInteger := tipPlatitorTVA;
    qrySave.ParamByName('gestiune_interna').AsBoolean := ChkTipGest.Checked;
   if VarIsNull(edTipTva.EditingValue) or VarIsEmpty(edTipTva.EditingValue)
   or (VarToStrDef(edTipTva.EditingValue, '') = '') then
  qrySave.ParamByName('tip_tva').Clear
else
  qrySave.ParamByName('tip_tva').AsInteger := VarAsType(edTipTva.EditingValue, varInteger);


     // ID_TARA
if not (VarIsNull(edDBTara.EditValue) or VarIsEmpty(edDBTara.EditValue)) then
  qrySave.ParamByName('id_tari').AsInteger := edDBTara.EditValue
else
  qrySave.ParamByName('id_tari').Clear;

if not (VarIsNull(edDBJudet.EditValue) or VarIsEmpty(edDBJudet.EditValue)) then
  qrySave.ParamByName('id_judete').AsInteger := edDBJudet.EditValue
else
  qrySave.ParamByName('id_judete').Clear;

if not (VarIsNull(edDBOras.EditValue) or VarIsEmpty(edDBOras.EditValue)) then
  qrySave.ParamByName('id_localitati').AsInteger := edDBOras.EditValue
else
  qrySave.ParamByName('id_localitati').Clear;





    // Grupă
    if not VarIsNull(selectedGrupa) and (VarToStr(selectedGrupa) <> '') then
    begin
      try
        qrySave.ParamByName('grupa').AsInteger := StrToInt(VarToStr(selectedGrupa));
      except
        on E: Exception do
        begin
          ShowMessage('Eroare conversie Grupa: ' + E.Message);
          Exit;
        end;
      end;
    end
    else
      qrySave.ParamByName('grupa').Clear;

   if Trim(edCodIntern.Text) <> '' then
  qrySave.ParamByName('codsectie').AsString := Trim(edCodIntern.Text)
else
  qrySave.ParamByName('codsectie').Clear;


    // Execută SQL
    qrySave.ExecSQL;

    // Mesaj succes
    if FIdRepartitor > 0 then
      ShowMessage('Datele au fost actualizate cu succes!')
    else
      ShowMessage('Datele au fost adăugate cu succes!');

  finally
    qrySave.Free;
  end;


  edtTipRep.Items.Clear;
  Application.ProcessMessages;
  Sleep(100);
  PopulateTipuriRepartitori;
  Self.Show;

  if Assigned(FOnAfterSave) then
    FOnAfterSave;
end;



procedure TfrmOERepartitoriEdit.edtSoldDebitEnter(Sender: TObject);
begin
  edtSoldDebit.Properties.ReadOnly  := not (edtSoldCredit.Value = 0);
  edtSoldCredit.Properties.ReadOnly := not (edtSoldDebit.Value = 0);
end;

procedure TfrmOERepartitoriEdit.btnBancaAddClick(Sender: TObject);
begin
    if not qryBanca.Active then
    Exit;

  qryBanca.Append;
  EditRepartitorCont(DTBanca);
end;

procedure TfrmOERepartitoriEdit.btnBancaDefaultClick(Sender: TObject);
var
  currentId: Integer;
begin
  if not qryBanca.Active or qryBanca.IsEmpty then Exit;

  currentId := qryBanca.FieldByName('ID_REPARTITORI_CONTURI').AsInteger;

  with TZQuery.Create(nil) do
  try
    Connection := frmData.dbContabilitate;


    SQL.Text := 'UPDATE REPARTITORI_CONTURI SET DEFAULT_CONT = 0 WHERE ID_REPARTITORI = :ID';
    ParamByName('ID').AsInteger := FIdRepartitor;
    ExecSQL;


    SQL.Text := 'UPDATE REPARTITORI_CONTURI SET DEFAULT_CONT = 1 WHERE ID_REPARTITORI_CONTURI = :IDC';
    ParamByName('IDC').AsInteger := currentId;
    ExecSQL;
  finally
    Free;
  end;

  qryBanca.Refresh;

end;

procedure TfrmOERepartitoriEdit.btnBancaModifcaClick(Sender: TObject);
begin
     if not qryBanca.Active or qryBanca.IsEmpty then
    Exit;

  if qryBanca.State <> dsEdit then
    qryBanca.Edit;

  EditRepartitorCont(DTBanca);
end;

procedure TfrmOERepartitoriEdit.btnBancaStergeClick(Sender: TObject);
begin
 if not qryBanca.Active or qryBanca.IsEmpty then
    Exit;

  if MessageDlg('Sigur doriți să ștergeți acest cont bancar?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then
  begin
    qryBanca.Delete;
  end;
end;

procedure TfrmOERepartitoriEdit.btnCAddClick(Sender: TObject);
begin
  QrySoldRep.Append;
end;



//procedure TfrmOERepartitoriEdit.btnCautareClick(Sender: TObject);
//var
//  lRepInfo, lRepDesc, lDateGenerale, lAdresaSediu: TJsonObject;
//  lRepFound: TJSONArray;
//  lCui, i: Integer;
//  lscpTVA: Boolean;
//  lsCui, denumire, adresa, nrRegCom, judet, localitate, tara: String;
//  selectedTipTVA, selectedGrupa: Variant;
//  qryCheck, qryInsert: TZQuery;
//
//function GetSafe(const ParentObj: TJSONObject; const APropName: String; const ADefault: String = ''): String;
//var
//  lProp: TJSONPair;
//begin
//  Result := ADefault;
//  if Assigned(ParentObj) then
//  begin
//    lProp := ParentObj.Get(APropName);
//    if Assigned(lProp) and Assigned(lProp.JsonValue) and (lProp.JsonValue is TJSONString) then
//      Result := UTF8Decode(RawByteString(TJSONString(lProp.JsonValue).Value));
//  end;
//end;
//
//begin
//  lsCui := Trim(edtCUI.Text);
//  if lsCui = '' then
//  begin
//    ShowMessage('Introduceți un CUI valid!');
//    Exit;
//  end;
//
//  if UpperCase(Copy(lsCui, 1, 2)) = 'RO' then
//  begin
//    if not TryStrToInt(Copy(lsCui, 3, Length(lsCui) - 2), lCui) then
//    begin
//      ShowMessage('CUI invalid: ' + lsCui);
//      Exit;
//    end;
//  end
//  else if not TryStrToInt(lsCui, lCui) then
//  begin
//    ShowMessage('CUI invalid: ' + lsCui);
//    Exit;
//  end;
//
//  // 1. Verificăm dacă CUI-ul există în baza de date
//  qryCheck := TZQuery.Create(nil);
//  try
//    qryCheck.Connection := frmData.dbContabilitate;
//    qryCheck.SQL.Text := 'SELECT nume, adresa, REG_COMERT, tip_platitor_tva FROM repartitori WHERE COD_FISCAL = :CUI';
//    qryCheck.ParamByName('CUI').AsString := IntToStr(lCui);
//    qryCheck.Open;
//
//    if not qryCheck.IsEmpty then
//    begin
//      // Dacă găsim datele, completăm formularul și nu mai apelăm API-ul
//      edDBNume.Text := qryCheck.FieldByName('nume').AsString;
//      edDbCUI.Text := IntToStr(lCui);
//      edDBNrComert.Text := qryCheck.FieldByName('REG_COMERT').AsString;
//      edDBAdresa.Text := qryCheck.FieldByName('adresa').AsString;
//      cxCheckBox1.Checked := qryCheck.FieldByName('tip_platitor_tva').AsInteger = 1;
//
//      ShowMessage('Datele au fost încărcate din baza de date!');
//      Exit;
//    end;
//  finally
//    qryCheck.Free;
//  end;
//
//  // 2. Dacă nu găsim CUI-ul în baza de date, apelăm API-ul ANAF
//  ProgressBar1.Visible := True;
//  ProgressBar1.Style := pbstNormal;
//  ProgressBar1.Min := 0;
//  ProgressBar1.Max := 100;
//  ProgressBar1.Position := 0;
//  Application.ProcessMessages;
//
//  try
//    for i := 1 to 50 do
//    begin
//      ProgressBar1.Position := i * 2;
//      Application.ProcessMessages;
//      Sleep(50);
//    end;
//
//    lRepInfo := GetRepartitorInfo(lCui, lscpTVA);
//    if lRepInfo = nil then
//    begin
//      ShowMessage('Eroare: ANAF nu a returnat un răspuns valid!');
//      Exit;
//    end;
//
//    try
//      lRepFound := lRepInfo.Get('found').JsonValue as TJSONArray;
//      if lRepFound.Size = 0 then
//      begin
//        ShowMessage('Atentie: JSON-ul returnat de ANAF nu conține date!');
//        Exit;
//      end;
//
//      lRepDesc := lRepFound.Get(0) as TJsonObject;
//      if not Assigned(lRepDesc) then
//      begin
//        ShowMessage('Eroare: Datele nu sunt disponibile!');
//        Exit;
//      end;
//
//      if Assigned(lRepDesc.Get('date_generale')) and (lRepDesc.Get('date_generale').JsonValue is TJSONObject) then
//        lDateGenerale := lRepDesc.Get('date_generale').JsonValue as TJSONObject
//      else
//      begin
//        ShowMessage('Eroare: Lipsesc informațiile generale!');
//        Exit;
//      end;
//
//      ProgressBar1.Position := 70;
//      Application.ProcessMessages;
//
//      denumire := GetSafe(lDateGenerale, 'denumire', 'N/A');
//      adresa := GetSafe(lDateGenerale, 'adresa', 'N/A');
//      nrRegCom := GetSafe(lDateGenerale, 'nrRegCom', 'N/A');
//
//      if Assigned(lRepDesc.Get('adresa_sediu_social')) and (lRepDesc.Get('adresa_sediu_social').JsonValue is TJSONObject) then
//        lAdresaSediu := lRepDesc.Get('adresa_sediu_social').JsonValue as TJSONObject
//      else
//      begin
//        ShowMessage('Eroare: Lipsesc informațiile despre sediu!');
//        Exit;
//      end;
//
//      judet := GetSafe(lAdresaSediu, 'sdenumire_Judet', 'N/A');
//      localitate := GetSafe(lAdresaSediu, 'sdenumire_Localitate', 'N/A');
//      tara := GetSafe(lAdresaSediu, 'stara', 'N/A');
//
//      edDBNume.Text := denumire;
//      edDbCUI.Text := IntToStr(lCui);
//      edDBNrComert.Text := nrRegCom;
//      edDBAdresa.Text := adresa;
//      cxCheckBox1.Checked := lscpTVA;
//
//      // 3. Inserăm datele în baza de date
//      qryInsert := TZQuery.Create(nil);
//      try
//        qryInsert.Connection := frmData.dbContabilitate;
//        qryInsert.SQL.Text :=
//          'INSERT INTO dbo.repartitori (COD_FISCAL, nume, adresa, REG_COMERT, tip_platitor_tva) ' +
//          'VALUES (:cui, :nume, :adresa, :nrRegCom, :tipTVA)';
//
//        qryInsert.ParamByName('cui').AsString := IntToStr(lCui);
//        qryInsert.ParamByName('nume').AsString := denumire;
//        qryInsert.ParamByName('adresa').AsString := adresa;
//        qryInsert.ParamByName('nrRegCom').AsString := nrRegCom;
//        qryInsert.ParamByName('tipTVA').AsInteger := Integer(lscpTVA);
//        qryInsert.ExecSQL;
//
//
//      finally
//        qryInsert.Free;
//      end;
//
//      for i := 70 to 100 do
//      begin
//        ProgressBar1.Position := i;
//        Application.ProcessMessages;
//        Sleep(20);
//      end;
//    finally
//      lRepInfo.Free;
//    end;
//
//  finally
//    ProgressBar1.Visible := False;
//  end;
//end;


procedure TfrmOERepartitoriEdit.btnCautareClick(Sender: TObject);
var
  lRepInfo, lRepDesc, lDateGenerale, lAdresaSediu: TJsonObject;
  lRepFound: TJSONArray;
  lCui, i: Integer;
  lscpTVA: Boolean;
  lsCui, denumire, adresa, nrRegCom, judet, localitate, tara, tipTvaCod: String;

  function GetSafe(const ParentObj: TJSONObject; const APropName: String; const ADefault: String = ''): String;
  var lProp: TJSONPair;
  begin
    Result := ADefault;
    if Assigned(ParentObj) then
    begin
      lProp := ParentObj.Get(APropName);
      if Assigned(lProp) and Assigned(lProp.JsonValue) and (lProp.JsonValue is TJSONString) then
        Result := UTF8Decode(RawByteString(TJSONString(lProp.JsonValue).Value));
    end;
  end;

begin
  lsCui := Trim(edtCUI.Text);
  if lsCui = '' then
  begin
    ShowMessage('Introduceti un CUI valid!');
    Exit;
  end;

  if UpperCase(Copy(lsCui, 1, 2)) = 'RO' then
  begin
    if not TryStrToInt(Copy(lsCui, 3, Length(lsCui) - 2), lCui) then
    begin
      ShowMessage('CUI invalid: ' + lsCui);
      Exit;
    end;
  end
  else if not TryStrToInt(lsCui, lCui) then
  begin
    ShowMessage('CUI invalid: ' + lsCui);
    Exit;
  end;

  ProgressBar1.Visible := True;
  ProgressBar1.Style := pbstNormal;
  ProgressBar1.Min := 0;
  ProgressBar1.Max := 100;
  ProgressBar1.Position := 0;
  Application.ProcessMessages;

  try
    for i := 1 to 50 do
    begin
      ProgressBar1.Position := i * 2;
      Application.ProcessMessages;
      Sleep(20);
    end;

    lRepInfo := GetRepartitorInfo(lCui, lscpTVA);
    if lRepInfo = nil then
    begin
      ShowMessage('Eroare: ANAF nu a returnat un raspuns valid!');
      Exit;
    end;

    try
      lRepFound := lRepInfo.Get('found').JsonValue as TJSONArray;
      if lRepFound.Size = 0 then
      begin
        ShowMessage('Atentie: JSON-ul returnat nu contine date!');
        Exit;
      end;

      lRepDesc := lRepFound.Get(0) as TJsonObject;
    if Assigned(lRepDesc.Get('date_generale')) and (lRepDesc.Get('date_generale').JsonValue is TJSONObject) then
        lDateGenerale := lRepDesc.Get('date_generale').JsonValue as TJSONObject
      else
      begin
        ShowMessage('Eroare: Lipsesc informatiile generale!');
        Exit;
      end;

      if Assigned(lRepDesc.Get('adresa_sediu_social')) and (lRepDesc.Get('adresa_sediu_social').JsonValue is TJSONObject) then
        lAdresaSediu := lRepDesc.Get('adresa_sediu_social').JsonValue as TJSONObject
      else
      begin
        ShowMessage('Eroare: Lipsesc informatiile despre sediu!');
        Exit;
      end;


      denumire := GetSafe(lDateGenerale, 'denumire');
      adresa := GetSafe(lDateGenerale, 'adresa');
      nrRegCom := GetSafe(lDateGenerale, 'nrRegCom');
      judet := GetSafe(lAdresaSediu, 'sdenumire_Judet');
      localitate := GetSafe(lAdresaSediu, 'sdenumire_Localitate');
      tara := GetSafe(lAdresaSediu, 'stara');
       if not (DTRepartitori.DataSet.State in [dsEdit, dsInsert]) then
        DTRepartitori.DataSet.Edit;
     DTRepartitori.DataSet.FieldByName('NUME').AsString := denumire;
DTRepartitori.DataSet.FieldByName('ADRESA').AsString := adresa;
DTRepartitori.DataSet.FieldByName('REG_COMERT').AsString := nrRegCom;
DTRepartitori.DataSet.FieldByName('COD_FISCAL').AsString := IntToStr(lCui);
DTRepartitori.DataSet.FieldByName('TIP_PLATITOR_TVA').AsInteger := Integer(lscpTVA);


      if GetSafe(lDateGenerale, 'statusSplitTVA') = 'True' then
        tipTvaCod := '4'
      else if GetSafe(lDateGenerale, 'statusTvaIncasare') = 'True' then
        tipTvaCod := '3'
      else if lscpTVA then
        tipTvaCod := '2'
      else
        tipTvaCod := '1';

      edTipTva.EditValue := tipTvaCod;

      for i := 70 to 100 do
      begin
        ProgressBar1.Position := i;
        Application.ProcessMessages;
        Sleep(10);
      end;
    finally
      lRepInfo.Free;
    end;

  finally
    ProgressBar1.Visible := False;
  end;
end;




procedure TfrmOERepartitoriEdit.edDBJudetPropertiesEditValueChanged(
  Sender: TObject);
begin
      if not VarIsNull(edDBJudet.EditValue) then
    LoadLocalitatiByJudet(StrToIntDef(VarToStr(edDBJudet.EditValue), 0));

end;

procedure TfrmOERepartitoriEdit.edCEPropertiesButtonClick(
  Sender: TObject; AButtonIndex: Integer);
var
  aCont : String;
  aIdProiect : Integer;  
begin
  aIdProiect := -1;
  aCont := qryCF.FieldByName(TcxDBButtonEdit(Sender).DataBinding.DataField).AsString;
  NewSelectarePlanEconomic(aCont,aIdProiect, '', -1, True);
  if aCont <> '<Anulat>' then begin
    qryCF.Edit;
    qryCF.FieldByName(TcxDBButtonEdit(Sender).DataBinding.DataField).Value := aCont;
    qryCF.Post;
  end;
end;

procedure TfrmOERepartitoriEdit.PopulateGrupaRepartitori;
begin
 edDBGrupa.Properties.Items.Clear;
with TZQuery.Create(nil) do
try
  Connection := frmData.dbContabilitate;
  SQL.Text := 'SELECT ID_REPARTITORI_GRUPE, denumire FROM REPARTITORI_GRUPE';
  Open;
  while not EOF do
  begin
    if FieldByName('ID_REPARTITORI_GRUPE').IsNull then
      Continue;

    with edDBGrupa.Properties.Items.Add do
    begin
      Description := FieldByName('denumire').AsString;
      Value := FieldByName('ID_REPARTITORI_GRUPE').AsInteger;
    end;
    Next;
  end;
finally
  Free;
end;

end;


procedure TfrmOERepartitoriEdit.btnCFAddClick(Sender: TObject);
begin
  QryCF.Append;
end;

procedure TfrmOERepartitoriEdit.btnCFDelClick(Sender: TObject);
begin
  QryCF.Delete;
end;

procedure TfrmOERepartitoriEdit.btnCFUpdClick(Sender: TObject);
begin
  if QryCF.State in [dsEdit, dsInsert] then QryCF.Post;
end;

procedure TfrmOERepartitoriEdit.PopulateTipuriRepartitori;
var
  lItem: TcxCheckListBoxItem;
  lQryCheck: TZQuery;
  i: Integer;
begin
  edtTipRep.Items.Clear;
  frmData.QryRepTipuri.Open;


  with frmData.QryRepTipuri do
  begin
    First;
    while not Eof do
    begin
      lItem := edtTipRep.Items.Add;
      with lItem do
      begin
        Text := FieldByName('DENUMIRE').AsString;
        Tag := FieldByName('ID_REPARTITORI_TIPURI').AsInteger;
        Checked := False;
      end;
      Next;
    end;
  end;


  lQryCheck := TZQuery.Create(nil);
  try
    lQryCheck.Connection := frmData.dbContabilitate;
    lQryCheck.SQL.Text := 'SELECT ID_REPARTITORI_TIPURI FROM dbo.REPARTITORI_CLASIFICATI WHERE ID_REPARTITORI = :IdRep';
    lQryCheck.ParamByName('IdRep').AsInteger := FIdRepartitor;
    lQryCheck.Open;



    while not lQryCheck.Eof do
    begin
      for i := 0 to edtTipRep.Items.Count - 1 do
        if edtTipRep.Items[i].Tag = lQryCheck.FieldByName('ID_REPARTITORI_TIPURI').AsInteger then
          edtTipRep.Items[i].Checked := True;
      lQryCheck.Next;
    end;
  finally
    lQryCheck.Free;
  end;
end;

procedure TfrmOERepartitoriEdit.PopulateTipTVA;
begin
  edTipTva.Properties.Items.Clear;

  with edTipTva.Properties.Items do
  begin
    with Add do
    begin
      Description := 'Fara TVA';
      Value := 1;
    end;

    with Add do
    begin
      Description := 'TVA Facturare';
      Value := 2;
    end;

    with Add do
    begin
      Description := 'TVA La Incasare';
      Value := 3;
    end;

    with Add do
    begin
      Description := 'Split TVA';
      Value := 4;
    end;
  end;
end;



procedure TfrmOERepartitoriEdit.SetTipRep(NewState: Boolean; lTipRep: Integer);
var
  lQryExec: TZQuery;
begin
  if FIdRepartitor = -1 then Exit;

  lQryExec := TZQuery.Create(nil);
  try
    lQryExec.Connection := frmData.dbContabilitate;
    lQryExec.SQL.Text := 'EXEC spOESetTipRep :idRep, :State, :TipRep';
    lQryExec.ParamByName('idRep').AsInteger := FIdRepartitor;
    lQryExec.ParamByName('State').AsInteger := Integer(NewState);
    lQryExec.ParamByName('TipRep').AsInteger := lTipRep;
    lQryExec.ExecSQL;


    RefreshRow(FIdRepartitor);
    DBRefresh([frmData.qryOIUnitati, frmData.qryOIProiecte]);
  finally
    lQryExec.Free;
  end;
end;


procedure TfrmOERepartitoriEdit.SetNewTipRep;
begin
end;


procedure TfrmOERepartitoriEdit.QrySoldRepBeforeDelete(
  DataSet: TDataSet);
begin
  if MessageDlg('Doriti stergerea soldului pentru contul : '+DataSet.FieldByName('CONT').AsString+'?',
                mtConfirmation, [mbYes, mbNo], 0) <> mrYes then Abort;
end;

procedure TfrmOERepartitoriEdit.DTSolduriRepStateChange(
  Sender: TObject);
begin
  btnCUpdate.Enabled := QrySoldRep.State in [dsEdit, dsInsert];
end;

procedure TfrmOERepartitoriEdit.PopulateTipGest;
begin
  if DBTableExists('GEST_TIP_GEST') then
    FillImageCombo(edTipGestiune.Properties, 'select * from GEST_TIP_GEST', 'ID_GEST_TIP_GEST', 'DENUMIRE');
end;

procedure TfrmOERepartitoriEdit.GridBancaVCustomDrawCell(
  Sender: TcxCustomGridTableView; ACanvas: TcxCanvas;
  AViewInfo: TcxGridTableDataCellViewInfo; var ADone: Boolean);
var
   lRecord     : TcxCustomGridRecord;
begin
  lRecord := AViewInfo.RecordViewInfo.GridRecord;
  if (lRecord = nil) or (AViewInfo.Selected) then Exit;
  if GetBoolean(lRecord, GridBancaVDEFAULT_CONT.Index) then
    ACanvas.Brush.Color := clAqua;
end;

procedure TfrmOERepartitoriEdit.SetInfoConfig;
begin
  Self.BorderStyle := bsNone;
  Self.Align := alClient;
  tabCautare.TabVisible := False;
  tabResult.TabVisible := False;
  pnTop.Visible := False;
  pnBottom.Visible := False;
  Self.Visible := True;
  cxPageContent.ActivePage := tabDetaliiPrimare;
  cxCheckBox1.Parent := tabDetaliiPrimare;
end;
    procedure TfrmOERepartitoriEdit.RefreshNomenclator;
begin
  FNomenclatorLoaded := False;
  LoadNomenclatorFromDB;
  ShowMessage('Nomenclatoarele au fost reîncărcate cu succes!');
end;

procedure TfrmOERepartitoriEdit.SetIdRepartitor(const Value: Integer);
var
  qryCheck: TZQuery;
  tipPlatitorTVA, grupeRepartitori, codSectie: Variant;
  i: Integer;
begin
  FIdRepartitor := Value;


   LoadNomenclatorFromDB;


  PopulateGrupaRepartitori;

  QryCF.Close;
  QryCF.Params.ParamByName('ID_REPARTITORI').Value := FIdRepartitor;
  QryCF.Open;
  qryBanca.Close;
  qryBanca.Params.ParamByName('ID_REPARTITORI').Value := FIdRepartitor;
  qryBanca.Open;
  QrySoldRep.Close;
  QrySoldRep.Params.ParamByName('ID_REPARTITORI').Value := FIdRepartitor;
  QrySoldRep.Open;

  SetNewTipRep;
  PopulateTipuriRepartitori;
  PopulateTipTVA;

  qryCheck := TZQuery.Create(nil);
  try
    qryCheck.Connection := frmData.dbContabilitate;
    qryCheck.SQL.Text :=
      'SELECT ID_TARI, ID_JUDETE, ID_LOCALITATI, tip_platitor_tva, id_repartitori_grupe, codsectie ' +
      'FROM repartitori WHERE ID_REPARTITORI = :ID';
    qryCheck.ParamByName('ID').AsInteger := FIdRepartitor;
    qryCheck.Open;

    if not qryCheck.IsEmpty then
    begin

      if not qryCheck.FieldByName('ID_TARI').IsNull then
        edDBTara.EditValue := qryCheck.FieldByName('ID_TARI').AsInteger
      else
        edDBTara.ItemIndex := -1;

     if not qryCheck.FieldByName('ID_JUDETE').IsNull then
begin
  edDBJudet.EditValue := qryCheck.FieldByName('ID_JUDETE').AsInteger;
  LoadLocalitatiByJudet(qryCheck.FieldByName('ID_JUDETE').AsInteger);
end
else
begin
  edDBJudet.ItemIndex := -1;
  edDBOras.Properties.Items.Clear;
end;


      if not qryCheck.FieldByName('ID_LOCALITATI').IsNull then
        edDBOras.EditValue := qryCheck.FieldByName('ID_LOCALITATI').AsInteger
      else
        edDBOras.ItemIndex := -1;


      tipPlatitorTVA := qryCheck.FieldByName('tip_platitor_tva').Value;
      codSectie := qryCheck.FieldByName('id_repartitori_grupe').Value;

      if VarIsNull(tipPlatitorTVA) then
        tipPlatitorTVA := 0;

      cxCheckBox1.Checked := (tipPlatitorTVA = 1);

      if not VarIsNull(grupeRepartitori) then
      begin
        for i := 0 to edDBGrupa.Properties.Items.Count - 1 do
        begin
          if edDBGrupa.Properties.Items[i].Value = grupeRepartitori then
          begin
            edDBGrupa.ItemIndex := i;
            Break;
          end;
        end;
      end
      else
        edDBGrupa.ItemIndex := -1;
    end
    else
    begin
      cxCheckBox1.Checked := False;
      edDBGrupa.ItemIndex := -1;
      edDBTara.ItemIndex := -1;
      edDBJudet.ItemIndex := -1;
      edDBOras.ItemIndex := -1;
    end;
  finally
    qryCheck.Free;
  end;
end;




procedure TfrmOERepartitoriEdit.ChkTipGestPropertiesChange(
  Sender: TObject);
begin
  lbTipGestiune.Enabled := ChkTipGest.Checked;
  edTipGestiune.Enabled := ChkTipGest.Checked;
end;

procedure TfrmOERepartitoriEdit.CompleteInfoCodFiscal(
  aCodFiscal: String);
begin
end;


procedure TfrmOERepartitoriEdit.tabBancaResize(Sender: TObject);
begin
 //
  edDBTelefon.Width := (tabBanca.Width - 20) div 2;
  edDBeMail.Width := (tabBanca.Width - 20) div 2;
  edDBFax.Left := edDBTelefon.Left + edDBTelefon.Width + 5;
  edDBFax.Width := edDBTelefon.Width;
  edDBWWW.Left := edDBTelefon.Left + edDBTelefon.Width + 5;
  edDBWWW.Width := edDBTelefon.Width;
  lbFax.Left := edDBFax.Left;
  lbwww.Left := edDBWWW.Left;
end;

procedure TfrmOERepartitoriEdit.QrySoldRepBeforePost(DataSet: TDataSet);
begin
  DataSet.FieldByName('SOLD').AsCurrency :=
    DataSet.FieldByName('SOLD_DEBITOR').AsCurrency -
    DataSet.FieldByName('SOLD_CREDITOR').AsCurrency;
end;

procedure TfrmOERepartitoriEdit.QryCFNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('ID_REPARTITORI').AsInteger := FIdRepartitor;
end;

procedure TfrmOERepartitoriEdit.RefreshRow;
begin
end;


procedure TfrmOERepartitoriEdit.SetIsReadOnly(const Value: Boolean);
begin
  FIsReadOnly := Value;

  //detalii repartitor
  edDBNume.Enabled := not FIsReadOnly;
  edDBCUI.Enabled := not FIsReadOnly;
  edDBNrComert.Enabled := not FIsReadOnly;
  edCodIntern.Enabled := not FIsReadOnly;
  edDBTara.Enabled := not FIsReadOnly;
  edDBJudet.Enabled := not FIsReadOnly;
  edDBOras.Enabled := not FIsReadOnly;
  edDBAdresa.Enabled := not FIsReadOnly;
  edDBGrupa.Enabled := not FIsReadOnly;
  edDBDomeniu.Enabled := not FIsReadOnly;

  //Alte detalii
  edDBPersoanaContact.Enabled := not FIsReadOnly;
  edDBTelefon.Enabled := not FIsReadOnly;
  edDBFax.Enabled := not FIsReadOnly;
  edDBeMail.Enabled := not FIsReadOnly;
  edDBWWW.Enabled := not FIsReadOnly;
  GridBanca.Enabled := not FIsReadOnly;
  btnBancaAdd.Enabled := not FIsReadOnly;
  btnBancaModifca.Enabled := not FIsReadOnly;
  btnBancaSterge.Enabled := not FIsReadOnly;
  btnBancaDefault.Enabled := not FIsReadOnly;

  //Situatie contabila initiala
  GridConta.Enabled := not FIsReadOnly;
  edtCont.Enabled := not FIsReadOnly;
  edtSoldDebit.Enabled := not FIsReadOnly;
  edtSoldCredit.Enabled := not FIsReadOnly;
  btnCAdd.Enabled := not FIsReadOnly;
  btnCDel.Enabled := not FIsReadOnly;
  btnCUpdate.Enabled := not FIsReadOnly;

  //pozitii buget
  GridCF.Enabled := not FIsReadOnly;
  edCF.Enabled := not FIsReadOnly;
  edCE.Enabled := not FIsReadOnly;
  edtCodFunctional.Enabled := not FIsReadOnly;
  btnCFAdd.Enabled := not FIsReadOnly;
  btnCFDel.Enabled := not FIsReadOnly;
  btnCFUpd.Enabled := not FIsReadOnly;
  btnPlanificare.Enabled := not FIsReadOnly;

  //tipuri repartitori
  edtTipRep.Enabled := not FIsReadOnly;
  ChkTipGest.Enabled := not FIsReadOnly;
  edTipGestiune.Enabled := not FIsReadOnly;
    cxCheckBox1.Enabled := not FIsReadOnly;
  edTipTva.Enabled := not FIsReadOnly;
end;

procedure TfrmOERepartitoriEdit.SetEditConfig;
begin
  tabCautare.TabVisible := False;
  tabResult.TabVisible := False;
//  pnTop.Visible := False;
  pnBottom.Visible := True;
  cxPageContent.ActivePage := tabDetaliiPrimare;
   cxCheckBox1.Parent := tabDetaliiPrimare;
end;
  procedure TfrmOERepartitoriEdit.btnCDelClick(Sender: TObject);
begin
  if not QrySoldRep.Active or QrySoldRep.IsEmpty then Exit;
  if MessageDlg('Sigur doriți să ștergeți această linie de sold?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then
    QrySoldRep.Delete;
end;

procedure TfrmOERepartitoriEdit.btnCUpdateClick(Sender: TObject);
begin
  if QrySoldRep.State in [dsEdit, dsInsert] then
    QrySoldRep.Post;
end;

procedure TfrmOERepartitoriEdit.btnFacturiClick(Sender: TObject);
var
  F: TfrmAlopAngDisponibil;
begin
  F := TfrmAlopAngDisponibil.Create(Self);
  try
    F.ShowModal;
  finally
    F.Free;
  end;
end;

procedure TfrmOERepartitoriEdit.edCFPropertiesButtonClick(Sender: TObject; AButtonIndex: Integer);
var
  aCont: String;
  aIdOI: Integer;
begin
  aIdOI := -1;
  aCont := qryCF.FieldByName(TcxDBButtonEdit(Sender).DataBinding.DataField).AsString;

  if aCont <> '<Anulat>' then
  begin
    qryCF.Edit;
    qryCF.FieldByName(TcxDBButtonEdit(Sender).DataBinding.DataField).Value := aCont;
    qryCF.Post;
  end;
end;

end.


