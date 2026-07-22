unit AlopLichidare;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, ExtCtrls, cxPC, cxControls, Menus, cxLookAndFeelPainters,
  StdCtrls, cxButtons, cxContainer, cxEdit, cxGroupBox, cxGraphics,
  cxTextEdit, cxMaskEdit, cxDropDownEdit, cxImageComboBox, cxCurrencyEdit,
  cxDBEdit, cxStyles, cxDataStorage, DB,
  cxDBData, cxGridLevel, cxClasses, cxGridCustomView,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGrid,
  cxSplitter, ZDataSet, cxGridCustomPopupMenu, cxGridPopupMenu, cxMemo,
  cxCheckBox, cxCalendar, cxButtonEdit, cxSpinEdit, cxLabel, 
  cxTL, cxInplaceContainer, cxTLData, cxDBTL, cxMRUEdit,
  ZAbstractRODataset, ZAbstractDataset,
  cxTLdxBarBuiltInMenu,
  cxLookAndFeels, cxCustomData, cxFilter, cxData, dxBarBuiltInMenu, cxNavigator,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxDateRanges,
  dxScrollbarAnnotations;

type
  TfrmAlopLichidare = class(TForm)
    pnBottom: TPanel;
    cxPageLichidare: TcxPageControl;
    tabObligatii: TcxTabSheet;
    tabContract: TcxTabSheet;
    gbObligatii: TPanel;
    cxGroupBox4: TcxGroupBox;
    Label1: TLabel;
    lbTipValuta: TLabel;
    Label3: TLabel;
    edModPlata: TcxDBImageComboBox;
    edTipValuta: TcxDBImageComboBox;
    edObligatii: TcxDBCurrencyEdit;
    Label4: TLabel;
    edAvans: TcxDBCurrencyEdit;
    lbCursSchimb: TLabel;
    edtCursSchimb: TcxDBCurrencyEdit;
    Label6: TLabel;
    Bevel1: TBevel;
    edSumaRON: TcxDBCurrencyEdit;
    Label7: TLabel;
    gbAngajament: TcxGroupBox;
    GridObligatiiV: TcxGridDBTableView;
    GridObligatiiL: TcxGridLevel;
    GridObligatii: TcxGrid;
    lbAngajament: TLabel;
    Bevel2: TBevel;
    GridAngDefV: TcxGridDBTableView;
    GridAngDefL: TcxGridLevel;
    GridAngDef: TcxGrid;
    DTAngDef: TDataSource;
    qryAngDef: TZQuery;
    GridAngDefVid_alop_angajamente_Defalcare: TcxGridDBColumn;
    GridAngDefVNUME_FURNIZOR: TcxGridDBColumn;
    GridAngDefVID_REPARTITORI: TcxGridDBColumn;
    GridAngDefVnume_departament: TcxGridDBColumn;
    GridAngDefVid_departament: TcxGridDBColumn;
    GridAngDefVcod_functional: TcxGridDBColumn;
    GridAngDefVDEN_FUNCTIONAL: TcxGridDBColumn;
    GridAngDefVcod_economic: TcxGridDBColumn;
    GridAngDefVDEN_ECONOMIC: TcxGridDBColumn;
    GridAngDefVSUMA_TOTALA: TcxGridDBColumn;
    GridAngDefVangajat_valuta: TcxGridDBColumn;
    GridAngDefVid_valuta: TcxGridDBColumn;
    GridAngDefVcurs_valutar: TcxGridDBColumn;
    GridAngDefVangajat: TcxGridDBColumn;
    ppGridAngDef: TcxGridPopupMenu;
    ppGridObligatii: TcxGridPopupMenu;
    ppObligatii: TPopupMenu;
    StegeObligatie1: TMenuItem;
    VizualizareObligatie1: TMenuItem;
    InfoMemo: TMemo;
    Label8: TLabel;
    tabDebugOrdonantare: TcxTabSheet;
    cxGrid1DBTableView1: TcxGridDBTableView;
    cxGrid1Level1: TcxGridLevel;
    cxGrid1: TcxGrid;
    DTOrdonantare: TDataSource;
    qryOrdonantare: TZQuery;
    cxGrid1DBTableView1ID_ALOP_ORDONANTARE: TcxGridDBColumn;
    cxGrid1DBTableView1ID_ALOP_ANGAJAMENTE: TcxGridDBColumn;
    cxGrid1DBTableView1NUMAR: TcxGridDBColumn;
    cxGrid1DBTableView1DATA_EMITERE: TcxGridDBColumn;
    cxGrid1DBTableView1ID_DEPARTAMENT: TcxGridDBColumn;
    cxGrid1DBTableView1DEPARTAMENT: TcxGridDBColumn;
    cxGrid1DBTableView1NATURA_CHELTUIELII: TcxGridDBColumn;
    cxGrid1DBTableView1DOCUMENTE_LICHIDATE: TcxGridDBColumn;
    cxGrid1DBTableView1MODUL_DE_PLATA: TcxGridDBColumn;
    cxGrid1DBTableView1ESTE_VALUTA: TcxGridDBColumn;
    cxGrid1DBTableView1TIP_VALUTA: TcxGridDBColumn;
    cxGrid1DBTableView1SUMA_VALUTA: TcxGridDBColumn;
    cxGrid1DBTableView1CURS_VALUTAR: TcxGridDBColumn;
    cxGrid1DBTableView1SUMA_DATORATA: TcxGridDBColumn;
    cxGrid1DBTableView1AVANSURI_ACORDATE: TcxGridDBColumn;
    cxGrid1DBTableView1SUMA_PLATA: TcxGridDBColumn;
    cxGrid1DBTableView1NUMAR_ECRAN: TcxGridDBColumn;
    cxGrid1DBTableView1VALIDAT: TcxGridDBColumn;
    cxGrid1DBTableView1NR_ORDINE: TcxGridDBColumn;
    cxGrid1DBTableView1ID_REPARTITORI: TcxGridDBColumn;
    cxGrid1DBTableView1NUME_REPARTITOR: TcxGridDBColumn;
    cxGrid1DBTableView1ADRESA_REPARTITOR: TcxGridDBColumn;
    cxGrid1DBTableView1CONT_REPARTITOR: TcxGridDBColumn;
    cxGrid1DBTableView1BANCA_REPARTITOR: TcxGridDBColumn;
    cxGrid1DBTableView1COD_BANCA_REPATITOR: TcxGridDBColumn;
    cxGrid1DBTableView1ID_UTILIZATORI: TcxGridDBColumn;
    cxSplitter1: TcxSplitter;
    cxGrid2DBTableView1: TcxGridDBTableView;
    cxGrid2Level1: TcxGridLevel;
    cxGrid2: TcxGrid;
    tabOrdonantare: TcxTabSheet;
    gpOrdonantare: TcxGroupBox;
    edtNaturaCheltuielii: TcxDBMemo;
    edtValidat: TcxDBCheckBox;
    edtNrOrdonantare: TcxDBButtonEdit;
    edtDataEmitere: TcxDBDateEdit;
    lbNrOrdonantare: TLabel;
    lbDataEmitere: TLabel;
    lbNrOrdine: TLabel;
    edtNrOrdine: TcxDBSpinEdit;
    Label9: TLabel;
    Label10: TLabel;
    btnAngajamentOrd: TcxButton;
    Label11: TLabel;
    Label12: TLabel;
    edtNrAngajament: TcxDBTextEdit;
    edtDataAngajament: TcxDBDateEdit;
    Label13: TLabel;
    btnCFAdd: TcxButton;
    btnCFDel: TcxButton;
    btnCFUpd: TcxButton;
    DTOrdDef: TDataSource;
    qryOrdDef: TZQuery;
    GridObligatiiVID_ALOP_ORDONANTARE_DEFALCARE: TcxGridDBColumn;
    GridObligatiiVID_ALOP_ORDONANTARE: TcxGridDBColumn;
    GridObligatiiVCOD_FUNCTIONAL: TcxGridDBColumn;
    GridObligatiiVCOD_ECONOMIC: TcxGridDBColumn;
    GridObligatiiVDISPONIBIL_INAINTE: TcxGridDBColumn;
    GridObligatiiVSUMA_PLATA: TcxGridDBColumn;
    GridObligatiiVDISPONIBIL_DUPA: TcxGridDBColumn;
    GridObligatiiVESTE_OPERATOR: TcxGridDBColumn;
    GridObligatiiVID_UTILIZATOR: TcxGridDBColumn;
    lbNumeFurnizor: TcxLabel;
    cgRep: TcxGroupBox;
    Label14: TLabel;
    Label15: TLabel;
    edtAdresaRepartitor: TcxDBButtonEdit;
    Label16: TLabel;
    Label17: TLabel;
    edtBancaRepartitor: TcxDBTextEdit;
    edtCodBanca: TcxDBTextEdit;
    Label18: TLabel;
    edtRepartitor: TcxDBPopupEdit;
    TreeRepartitori: TcxDBTreeList;
    DTRepartitori: TDataSource;
    qryRepartitori: TZQuery;
    TreeRepartitoriid_repartitori: TcxDBTreeListColumn;
    TreeRepartitorinume: TcxDBTreeListColumn;
    GridAngDefVnumar: TcxGridDBColumn;
    GridAngDefVdata_emitere: TcxGridDBColumn;
    GridAngDefVordonantat: TcxGridDBColumn;
    GridAngDefVdisponibil: TcxGridDBColumn;
    edRepNumarCont: TcxDBMRUEdit;
    btnValidare: TcxButton;
    btnValidareListare: TcxButton;
    edtEsteValuta: TcxDBCheckBox;
    lbSumaValuta: TLabel;
    edtSumaValuta: TcxDBCurrencyEdit;
    ppAngDef: TPopupMenu;
    Adaugapozitie1: TMenuItem;
    edtDocumenteLichidate: TcxDBMemo;
    cxStyleRepository1: TcxStyleRepository;
    cxStyle1: TcxStyle;
    btnNewOrd: TcxButton;
    btnAnuleazaOrd: TcxButton;
    BtnModificareOrd: TcxButton;
    pnTop: TPanel;
    cxSplitter2: TcxSplitter;
    GridAngDefVramas_de_angajat: TcxGridDBColumn;
    GridAngDefVdisponibil_de_angajat: TcxGridDBColumn;
    GridAngDefVid_analitic: TcxGridDBColumn;
    GridAngDefVid_oi_unitati: TcxGridDBColumn;
    GridAngDefVid_oi_proiecte: TcxGridDBColumn;
    GridObligatiiVSURSA_FINANTARE: TcxGridDBColumn;
    GridObligatiiVID_ALOP_ANGAJAMENTE_DEFALCARE: TcxGridDBColumn;
    GridObligatiiVID_OI_UNITATI: TcxGridDBColumn;
    GridObligatiiVID_OI_PROIECTE: TcxGridDBColumn;
    GridObligatiiVDISPONIBIL_TRIM_INAINTE: TcxGridDBColumn;
    GridObligatiiVDISPONIBIL_TRIM_DUPA: TcxGridDBColumn;
    GridObligatiiVDISPONIBIL_AN_INAINTE: TcxGridDBColumn;
    GridObligatiiVDISPONIBIL_DUPA_INAINTE: TcxGridDBColumn;
    btnEditRepartitor: TcxButton;
    btnDocumente: TcxButton;
    procedure StegeObligatie1Click(Sender: TObject);
    procedure cxPageLichidarePageChanging(Sender: TObject;
      NewPage: TcxTabSheet; var AllowChange: Boolean);
    procedure FormCreate(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure edtValidatPropertiesChange(Sender: TObject);
    procedure btnAngajamentOrdClick(Sender: TObject);
    procedure edtNrOrdonantarePropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
    procedure edtDataEmiterePropertiesValidate(Sender: TObject;
      var DisplayValue: Variant; var ErrorText: TCaption;
      var Error: Boolean);
    procedure GridAngDefVFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure btnCFDelClick(Sender: TObject);
    procedure btnCFUpdClick(Sender: TObject);
    procedure GridObligatiiVCOD_FUNCTIONALPropertiesButtonClick(
      Sender: TObject; AButtonIndex: Integer);
    procedure qryAngDefAfterOpen(DataSet: TDataSet);
    procedure GridAngDefVDblClick(Sender: TObject);
    procedure qryOrdDefAfterOpen(DataSet: TDataSet);
    procedure edtRepartitorPropertiesInitPopup(Sender: TObject);
    procedure edtAdresaRepartitorPropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
    procedure edRepNumarContPropertiesButtonClick(Sender: TObject);
    procedure edRepNumarContPropertiesCloseUp(Sender: TObject);
    procedure TreeRepartitoriDblClick(Sender: TObject);
    procedure TreeRepartitoriKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure edtRepartitorPropertiesCloseQuery(Sender: TObject;
      var CanClose: Boolean);
    procedure edtRepartitorPropertiesPopup(Sender: TObject);
    procedure qryOrdonantareAfterOpen(DataSet: TDataSet);
    procedure edtEsteValutaPropertiesChange(Sender: TObject);
    procedure btnValidareClick(Sender: TObject);
    procedure Adaugapozitie1Click(Sender: TObject);
    procedure DTOrdonantareDataChange(Sender: TObject; Field: TField);
    procedure btnValidareListareClick(Sender: TObject);
    procedure FormShow(Sender: TObject);
    procedure btnCFAddClick(Sender: TObject);
    procedure btnNewOrdClick(Sender: TObject);
    procedure btnAnuleazaOrdClick(Sender: TObject);
    procedure BtnModificareOrdClick(Sender: TObject);
    procedure btnEditRepartitorClick(Sender: TObject);
    procedure edRepNumarContPropertiesInitPopup(Sender: TObject);
    procedure btnDocumenteClick(Sender: TObject);
  protected
    procedure OrdonantareNoua(TestGolire: Boolean);
    procedure ValidareOrdonantare(CuOrdonantareNoua: Boolean);
  private
    IsInLoading     : Boolean;
    FIdAngajament   : Integer;
    FIdOrdonantare  : Integer;
    procedure OrdDefSumaPlataChange(Sender : TField);
    procedure OrdSumaPlataChange(Sender : TField);
    procedure OrdCursValutaChange(Sender : TField);
    procedure OrdIdValutaChange(Sender: TField);
    procedure IdRepartitorChange(Sender : TField);
    procedure SetIdAngajament(const Value: Integer);
    procedure SetIdOrdonantare(const Value: Integer);
    function  NewOrdonantare: Integer;
    procedure AdaugaAngDefOrdDef;
    function  IsOrdValid : Boolean;
    function TestUniqueNumber : Boolean;
    { Private declarations }
  public
    { Public declarations }
    FSelectedValuta : Integer;
    FSelectedCurs   : Currency;
    FIdFurnizorOrdonantare : Integer;
    procedure SetDetaliiRedactare(IsFinalizat : Boolean);
    procedure CalculeazaSumaPlata;
    procedure TestGolireEcran;
    procedure RefaDupaModificare;
    procedure SetStareEditare(ReadOnly : Boolean);
    procedure VerificaOrdonantare;
    procedure CalculateNrOrdine;
    procedure ReadOrdonantare;
    procedure CheckAndSave;
    procedure LoadOrdonantare(IdOrdonantare : Integer);
    property  IdAngajament : Integer read FIdAngajament write SetIdAngajament;
    property  IdOrdonantare : Integer read FIdOrdonantare write SetIdOrdonantare;
  end;

procedure PrintOrdonantare(lIdOrdonantare : Integer);
function ModificareOrdonantare(lIdOrdonantare : Integer) : TForm;

implementation

uses
  TypInfo, AlopObligatii, ZeosDBUtile, dxCompsUtile, DateUnit, AlopAngVizualizare, CommonDBVar,
  rapInclude, FormulareUnit, ConcurentUsersUnit, AlopOrdList,
  OERepartitoriUnit, ATSZDBUtils;

{$R *.dfm}

procedure TfrmAlopLichidare.SetIdAngajament(const Value: Integer);
begin
  FIdAngajament := Value;
  DoCheckClose(qryAngDef);
  if FIdAngajament = -1 then begin
     lbAngajament.Caption := 'Angajament neselectat';

  end
  else begin
    qryAngDef.Params[0].Value := FIdAngajament;
    qryAngDef.Params[1].Value := FIdOrdonantare;
    qryAngDef.Open;
    DBRefresh(qryOrdDef);
    lbAngajament.Caption := 'Angajamentul nr. ' +
      qryAngDef.FieldByName('NUMAR').AsString + ' din ' +
      FormatDatetime('dd/mm/yyyy', qryAngDef.FieldByName('DATA_EMITERE').AsDateTime);
    FSelectedValuta := qryAngDef.FieldByName('ID_VALUTA').AsInteger;
    FSelectedCurs := qryAngDef.FieldByName('curs_valutar').AsCurrency;
  end;
  //trecem automat obligatiile de plata marcate pentru repartitor pentru acest angajament
   //
end;

procedure TfrmAlopLichidare.StegeObligatie1Click(Sender: TObject);
begin
  btnCFDel.Click();
end;

procedure TfrmAlopLichidare.cxPageLichidarePageChanging(Sender: TObject;
  NewPage: TcxTabSheet; var AllowChange: Boolean);
begin
  if NewPage = tabOrdonantare then begin
    InfoMemo.Lines.Clear;
    InfoMemo.Lines.Add('Completare date primare ordonantare ');
    InfoMemo.Lines.Add('Se poate completa pe baza unui angajament ');
  end
  else
  if NewPage = tabObligatii then begin
    InfoMemo.Lines.Clear;
    InfoMemo.Lines.Add('Selectie obligatii de plata ');
    InfoMemo.Lines.Add('Se poate selecta in baza unui angajament legal sau direct o obligatie de plata ');
  end
  else if NewPage = tabContract then begin
    InfoMemo.Lines.Clear;
    InfoMemo.Lines.Add('Completam modul de plata si datele furnizorului pentru a ordonantarea de plata');
  end;
end;

procedure TfrmAlopLichidare.FormCreate(Sender: TObject);
begin
  edtValidat.Enabled := False;
  tabDebugOrdonantare.TabVisible := False;
  cxPageLichidare.ActivePage := tabOrdonantare;
  DBRefresh(qryRepartitori);
  FillImageCombo(edTipValuta.Properties, 'spNmclValute', 0, 1);
end;

function TfrmAlopLichidare.NewOrdonantare: Integer;
begin
  DBStartTransaction;
  try
    Result := ValueSafeToInt( DBGetScallarFmt('exec [spAlopNewOrdonantare] %d, null', [IdUtilizator]) );
    DBCommit;
  except
    on E: Exception do begin
       DBRollBack;
       raise EContaHandledError.Create('Nu se poate adauga angajamentul !'#13#10'EROARE : '+E.Message);
    end;
  end;
end;

procedure TfrmAlopLichidare.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  CheckAndSave;
  Action := caFree;
end;

procedure TfrmAlopLichidare.SetIdOrdonantare(const Value: Integer);
begin
  FIdOrdonantare := Value;
  if FIdOrdonantare = -1 then
  begin
    IdOrdonantare := NewOrdonantare;

    DoCheckClose(qryOrdonantare);
    qryOrdonantare.Params[0].Value := IdOrdonantare;
    DBRefresh(qryOrdonantare);

    if qryOrdonantare.Active then
    begin
      if not (qryOrdonantare.State in [dsEdit, dsInsert]) then
        qryOrdonantare.Edit;

      qryOrdonantare.FieldByName('DATA_EMITERE').AsDateTime := Date;
      qryOrdonantare.Post;
    end;
  end
  else
  begin
    DoCheckClose(qryOrdonantare);
    qryOrdonantare.Params[0].Value := FIdOrdonantare;
    DBRefresh(qryOrdonantare);

    DoCheckClose(qryOrdDef);
    qryOrdDef.Params[0].Value := FIdOrdonantare;
    DBRefresh(qryOrdDef);
  end;


  edtDataEmitere.EditValue := qryOrdonantare.FieldByName('DATA_EMITERE').AsDateTime;

  SetDetaliiRedactare(qryOrdonantare.FieldByName('REDACTAT').AsBoolean);
end;


procedure TfrmAlopLichidare.edtValidatPropertiesChange(Sender: TObject);
begin
  if not IsInLoading and not qryOrdonantare.Active then ReadOrdonantare;
  
  lbNrOrdonantare.Enabled := edtValidat.Checked;
  lbDataEmitere.Enabled := edtValidat.Checked;
  lbNrOrdine.Enabled := edtValidat.Checked;

  edtNrOrdonantare.Enabled := edtValidat.Checked;
  edtDataEmitere.Enabled   := edtValidat.Checked;
  edtNrOrdine.Enabled      := edtValidat.Checked;

//  btnOrdonantare.Enabled := edtValidat.Checked;
end;

procedure TfrmAlopLichidare.btnAngajamentOrdClick(Sender: TObject);
var
  lDataSet: TDataSet;

    procedure SetField(const DestName, SrcName: String);
    var
      lDstField,
      lSrcField: TField;
    begin
      lSrcField := lDataSet.FindField(SrcName);
      lDstField := qryOrdonantare.FindField(DestName);
      if Assigned(lSrcField) and Assigned(lDstField) then
        lDstField.Value := lSrcField.Value;
    end;

begin
  if not qryOrdonantare.Active then ReadOrdonantare;
  IdAngajament := SelectieAngajament;
  if IdAngajament = -1 then Exit;
  lDataSet := DBNewQueryFmt('exec [spAlopAngInfo] %d', [IdAngajament]);
  try
    lDataSet.Open;
    qryOrdonantare.DisableControls;
    if not lDataSet.IsEmpty then begin
      DBGoEdit(qryOrdonantare);

      if qryOrdonantare.FieldByName('DATA_EMITERE').AsDateTime < lDataSet.FieldByName('DATA').AsDateTime then
        qryOrdonantare.FieldByName('DATA_EMITERE').AsDateTime := lDataSet.FieldByName('DATA').AsDateTime;

      qryOrdonantare.FieldByName('ID_ALOP_ANGAJAMENTE').AsInteger := IdAngajament;

      SetField('NR_ANGAJAMENT'      , 'NUMAR');
      SetField('DATA_ANGAJAMENT'    , 'DATA');
      SetField('ID_DEPARTAMENT'     , 'ID_DEPARTAMENT');
      SetField('DEPARTAMENT'        , 'NUME_DEPARTAMENT');
      SetField('NATURA_CHELTUIELII' , 'NATURA_CHELTUIELII');
      SetField('ID_REPARTITORI'     , 'ID_REPARTITORI');
      SetField('NUME_REPARTITOR'    , 'NUME_REPARTITOR');
      SetField('ADRESA_REPARTITOR'  , 'ADRESA_REPARTITOR');
      SetField('ID_VALUTA'          , 'ID_VALUTA');
      SetField('CURS_VALUTAR'       , 'CURS_VALUTAR');
      qryOrdonantare.Post;
    end;
  finally
    qryOrdonantare.EnableControls;
    lDataSet.Free;
  end;
end;

procedure TfrmAlopLichidare.edtNrOrdonantarePropertiesButtonClick(
  Sender: TObject; AButtonIndex: Integer);
var
  lNumar : String;
begin
  if not IsInLoading and not qryOrdonantare.Active then ReadOrdonantare;
  DoCheckPostDataSet(qryOrdonantare);
  lNumar := ValueSafeToStr( DBGetScallarFmt('exec [spAlopOrdonatareNumar] %d', [IdOrdonantare]) );
  if not (qryOrdonantare.State in [dsEdit, dsInsert]) then qryOrdonantare.Edit;
  qryOrdonantare.FieldByName('NUMAR').AsString := lNumar;
  qryOrdonantare.Post;
end;

procedure TfrmAlopLichidare.ReadOrdonantare;
begin
  with DBNewQueryFmt('exec spAlopOrdonantariNeterminate %d', [IdUtilizator]) do
    try
       Open;
       IsInLoading := True;
       try
         if not IsEmpty then
            if (False) and (RecordCount > 1) then begin
               //deschidere lista de selectie
               IdOrdonantare := FieldByName('ID').AsInteger;
               SetStareEditare(FieldByName('REDACTAT').AsBoolean);
            end
            else begin
              IdOrdonantare := FieldByName('ID').AsInteger;
              SetStareEditare(FieldByName('REDACTAT').AsBoolean);
              if FieldByName('ID_ALOP_ANGAJAMENTE').AsInteger = 0 then IdAngajament := -1
              else IdAngajament :=  FieldByName('ID_ALOP_ANGAJAMENTE').AsInteger;
            end
         else begin
           IdOrdonantare := -1;
           IdAngajament := -1;
         end;
       finally
         IsInLoading := False;
       end;
    finally
       Free;
    end;
end;

procedure TfrmAlopLichidare.edtDataEmiterePropertiesValidate(
  Sender: TObject; var DisplayValue: Variant; var ErrorText: TCaption;
  var Error: Boolean);
begin
  if Error then begin
    ErrorText := '';
    raise EContaHandledError.Create('Data Emitere nu este corecta !');
  end
  else begin
    CalculateNrOrdine;
  end;
end;

procedure TfrmAlopLichidare.CalculateNrOrdine;
var
  lNumar: Integer;
begin
  if IsInLoading then Exit;
  try
    IsInLoading := True;
    DBPost(qryOrdonantare);
    lNumar := DBGetScallarFmt('exec spAlopOrdCalcNrOrdine %d', [FIdOrdonantare]);
    if lNumar > qryOrdonantare.FieldByName('NR_ORDINE').AsInteger then
      DBSetFieldValue(qryOrdonantare, 'NR_ORDINE', lNumar, True);
  finally
    IsInLoading := False;
  end;
end;

procedure TfrmAlopLichidare.GridAngDefVFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
  if Assigned(AFocusedRecord) and  AFocusedRecord.IsData then begin
    FIdFurnizorOrdonantare :=  AFocusedRecord.Values[GridAngDefVID_REPARTITORI.Index];
    lbNumeFurnizor.Caption := AFocusedRecord.Values[GridAngDefVNUME_FURNIZOR.Index];
  end;
end;

procedure TfrmAlopLichidare.btnCFDelClick(Sender: TObject);
begin
  if (qryOrdDef.Active) and not (qryOrdDef.IsEmpty) then qryOrdDef.Delete;
  CalculeazaSumaPlata;
end;

procedure TfrmAlopLichidare.btnCFUpdClick(Sender: TObject);
begin
  if qryOrdDef.Active and (qryOrdDef.State in [dsEdit, dsInsert]) then begin
    qryOrdDef.Post;
    CalculeazaSumaPlata;
  end;
end;

procedure TfrmAlopLichidare.btnDocumenteClick(Sender: TObject);
var
  lfmAlopObligatii: TfrmAlopObligatii;
begin
  if IdAngajament = -1 then
    raise Exception.Create('Selectati angajamentul in baza caruia efectuati ordonantarea !');
  lfmAlopObligatii := TfrmAlopObligatii.Create(nil);
  try
    lfmAlopObligatii.IdAngajament   := IdAngajament;
    lfmAlopObligatii.IdOrdonantare  := IdOrdonantare;
    if lfmAlopObligatii.Execute then begin
      DBSetFieldValue(qryOrdonantare, 'DOCUMENTE_LICHIDATE', lfmAlopObligatii.DocumenteJustificative);
      DBSetFieldValue(qryOrdonantare, 'SUMA_PLATA', lfmAlopObligatii.SumaDePlataTotala);
    end;
  finally
    lfmAlopObligatii.Free;
  end;
end;

procedure TfrmAlopLichidare.GridObligatiiVCOD_FUNCTIONALPropertiesButtonClick(
  Sender: TObject; AButtonIndex: Integer);
begin
  //Deschidem executia pe codul functional si economic sau angajamentul curent
end;

procedure TfrmAlopLichidare.qryAngDefAfterOpen(DataSet: TDataSet);
begin
  TcxMRUEditProperties(GridObligatiiVCOD_FUNCTIONAL.Properties).LookupItems.Clear;
  TcxMRUEditProperties(GridObligatiiVCOD_ECONOMIC.Properties).LookupItems.Clear;
  DataSet.First;
  while not (DataSet.eof) do begin
    TcxMRUEditProperties(GridObligatiiVCOD_FUNCTIONAL.Properties).LookupItems.Add(DataSet.FieldByName('COD_FUNCTIONAL').AsString);
    TcxMRUEditProperties(GridObligatiiVCOD_ECONOMIC.Properties).LookupItems.Add(DataSet.FieldByName('COD_ECONOMIC').AsString);    
    DataSet.Next;
  end;
end;

procedure TfrmAlopLichidare.GridAngDefVDblClick(Sender: TObject);
begin
  AdaugaAngDefOrdDef;
end;

procedure TfrmAlopLichidare.qryOrdDefAfterOpen(DataSet: TDataSet);
begin
  qryordDef.FieldByName('SUMA_PLATA').OnChange := OrdDefSumaPlataChange;
end;

procedure TfrmAlopLichidare.OrdDefSumaPlataChange(Sender: TField);
var
  aState : Boolean;
  IsAbort : Boolean;
  lStrMessage : String;
begin
  IsAbort := False;
  (*
  with Sender.DataSet do begin
    aState := State in [dsEdit, dsInsert];
    if not aState then Edit;
    if IsInLoading or (FieldByName('DISPONIBIL_INAINTE').AsCurrency  - Sender.AsCurrency >= 0) then
      FieldByName('DISPONIBIL_DUPA').AsCurrency := FieldByName('DISPONIBIL_INAINTE').AsCurrency  - Sender.AsCurrency
    else
     if MessageDlg('Pe clasificatia bugetara : '+FieldByNAme('COD_ECONOMIC').AsString+' nu mai aveti disponibila suma de angajat sau ati angajat o suma mai mare decat ati angajat  !'#13#10+
                       'Doriti totusi angajarea sumei : '+Sender.AsString+' ?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then
           FieldByName('DISPONIBIL_DUPA').AsCurrency := FieldByName('DISPONIBIL_INAINTE').AsCurrency  - Sender.AsCurrency
         else IsAbort := True;
    if IsInLoading or (FieldByName('DISPONIBIL_TRIM_INAINTE').AsCurrency  - Sender.AsCurrency >= 0) then
      FieldByName('DISPONIBIL_TRIM_DUPA').AsCurrency := FieldByName('DISPONIBIL_TRIM_INAINTE').AsCurrency  - Sender.AsCurrency
    else
     if MessageDlg('Pe clasificatia bugetara : '+FieldByNAme('COD_ECONOMIC').AsString+' nu mai aveti disponibila suma de angajat sau ati angajat o suma mai mare decat disponibilul trimestrial  !'#13#10+
                       'Doriti totusi angajarea sumei : '+Sender.AsString+' ?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then
           FieldByName('DISPONIBIL_TRIM_DUPA').AsCurrency := FieldByName('DISPONIBIL_TRIM_INAINTE').AsCurrency  - Sender.AsCurrency
         else IsAbort := True;
    if IsInLoading or (FieldByName('DISPONIBIL_AN_INAINTE').AsCurrency  - Sender.AsCurrency >= 0) then
      FieldByName('DISPONIBIL_AN_DUPA').AsCurrency := FieldByName('DISPONIBIL_AN_INAINTE').AsCurrency  - Sender.AsCurrency
    else
     if MessageDlg('Pe clasificatia bugetara : '+FieldByNAme('COD_ECONOMIC').AsString+' nu mai aveti disponibila suma de angajat sau ati angajat o suma mai mare decat disponibilul anual  !'#13#10+
                       'Doriti totusi angajarea sumei : '+Sender.AsString+' ?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then
           FieldByName('DISPONIBIL_AN_DUPA').AsCurrency := FieldByName('DISPONIBIL_AN_INAINTE').AsCurrency  - Sender.AsCurrency
         else IsAbort := True;
    if IsAbort then Sender.DataSet.Cancel
    else Post;
    if aState then Edit;
  end;
   *)
   lStrMessage := '';
   with Sender.DataSet do begin
    aState := State in [dsEdit, dsInsert];
    if not aState then Edit;
    if IsInLoading or (FieldByName('DISPONIBIL_INAINTE').AsCurrency  - Sender.AsCurrency >= 0) then
      FieldByName('DISPONIBIL_DUPA').AsCurrency := FieldByName('DISPONIBIL_INAINTE').AsCurrency  - Sender.AsCurrency
    else
     lStrMessage :='Pe clasificatia bugetara : '+FieldByName('COD_ECONOMIC').AsString+
            ' nu mai aveti disponibil de angajament sau ati angajat o suma mai mare decat cea din angajament !'#13#10;
    if IsInLoading or (FieldByName('DISPONIBIL_TRIM_INAINTE').AsCurrency  - Sender.AsCurrency >= 0) then
      FieldByName('DISPONIBIL_TRIM_DUPA').AsCurrency := FieldByName('DISPONIBIL_TRIM_INAINTE').AsCurrency  - Sender.AsCurrency
    else
     lStrMessage := lStrMessage + 'Pe clasificatia bugetara : '+FieldByName('COD_ECONOMIC').AsString+
                 ' nu mai aveti disponibila suma de angajat sau ati angajat o suma mai mare decat disponibilul trimestrial  !'#13#10;
    if IsInLoading or (FieldByName('DISPONIBIL_AN_INAINTE').AsCurrency  - Sender.AsCurrency >= 0) then
      FieldByName('DISPONIBIL_AN_DUPA').AsCurrency := FieldByName('DISPONIBIL_AN_INAINTE').AsCurrency  - Sender.AsCurrency
    else
       lStrMessage := lStrMessage + 'Pe clasificatia bugetara : '+FieldByName('COD_ECONOMIC').AsString+
                   ' nu mai aveti disponibila suma de angajat sau ati angajat o suma mai mare decat disponibilul anual  !'#13#10;

    if lStrMessage <> '' then begin
       if DBGetScallarFmt('select dbo.fnParamSoc(1, %s, null)', [ValueToStr('permitOrdonantarePeMinuc')]) = '1' then begin
         lStrMessage := lStrMessage +  'Doriti totusi ordonantarea sumei : '+Sender.AsString+' ?';
         if MessageDlg(lStrMessage, mtConfirmation, [mbYes, mbNo], 0) = mrYes then begin
            FieldByName('DISPONIBIL_DUPA').AsCurrency := FieldByName('DISPONIBIL_INAINTE').AsCurrency  - Sender.AsCurrency;
            FieldByName('DISPONIBIL_TRIM_DUPA').AsCurrency := FieldByName('DISPONIBIL_TRIM_INAINTE').AsCurrency  - Sender.AsCurrency;
            FieldByName('DISPONIBIL_AN_DUPA').AsCurrency := FieldByName('DISPONIBIL_AN_INAINTE').AsCurrency  - Sender.AsCurrency;
         end
         else IsAbort := True;
       end
       else begin
        MessageDlg(lStrMessage, mtError, [mbOk], 0);
        IsAbort := True;
       end;
    end;
    if IsAbort then Sender.DataSet.Cancel
    else Sender.DataSet.Post;
    if aState then Edit;
  end;

  if not IsAbort then CalculeazaSumaPlata
  else Abort;
end;

procedure TfrmAlopLichidare.edtRepartitorPropertiesInitPopup(
  Sender: TObject);
var
  lEdit: TcxDBPopupEdit;
begin
  lEdit := TcxDBPopupEdit(Sender);
  if lEdit.Properties.PopupWidth < lEdit.Width then lEdit.Properties.PopupWidth := lEdit.Width;
end;

procedure TfrmAlopLichidare.edtAdresaRepartitorPropertiesButtonClick(
  Sender: TObject; AButtonIndex: Integer);
var
  lIdRep  : Integer;
  lValue  : Variant;
begin
  lIdRep := qryOrdonantare.FieldByName('ID_REPARTITORI').AsInteger;
  if lIdRep > 0 then begin
    lValue := DBGetScallarFmt('SELECT ADRESA FROM REPARTITORI WHERE ID_REPARTITORI = %d', [lIdRep]);
    if ValueHasValue(lValue) then begin
      if not (qryOrdonantare.State in dsEditModes) then
        qryOrdonantare.Edit;
      qryOrdonantare.FieldByName('ADRESA_REPARTITOR').AsString := ValueSafeToStr(lValue);
      qryOrdonantare.Post;
    end;
  end;
end;

procedure TfrmAlopLichidare.edRepNumarContPropertiesButtonClick(
  Sender: TObject);
var
  lIdRep  : Integer;
  lDataSet: TDataSet;
begin
  lIdRep := qryOrdonantare.FieldByName('ID_REPARTITORI').AsInteger;
  if lIdRep > 0 then begin
    lDataSet := DBNewQueryFmt('SELECT TOP 1 * FROM .dbo.fnDefaultRepartitorConturi(%d)', [lIdRep]);
    try
      lDataSet.Open;
      if not lDataSet.IsEmpty then begin
        if not (qryOrdonantare.State in [dsEdit, dsInsert]) then qryOrdonantare.Edit;
        qryOrdonantare['CONT_REPARTITOR']     := lDataSet['CONT'];
        qryOrdonantare['BANCA_REPARTITOR']    := lDataSet['BANCA_DENUMIRE'];
        qryOrdonantare['COD_BANCA_REPATITOR'] := lDataSet['BANCA_COD'];
        qryOrdonantare.Post;
      end;
    finally
      lDataSet.Free;
    end;
  end;
end;

procedure TfrmAlopLichidare.edRepNumarContPropertiesCloseUp(
  Sender: TObject);
var
  lIdRep  : Integer;
  lDataSet: TDataSet;
begin
  lIdRep := qryOrdonantare.FieldByName('ID_REPARTITORI').AsInteger;
  if lIdRep > 0 then begin
    lDataSet := DBNewQueryFmt('SELECT TOP 1 * FROM .dbo.fnDefaultRepartitorConturi(%d) where CONT LIKE %s', [lIdRep, ValueToStr(edRepNumarCont.EditValue)]);
    try
      lDataSet.Open;
      if not lDataSet.IsEmpty then begin
        if not (qryOrdonantare.State in [dsEdit, dsInsert]) then qryOrdonantare.Edit;
        qryOrdonantare['BANCA_REPARTITOR']    := lDataSet['BANCA_DENUMIRE'];
        qryOrdonantare['COD_BANCA_REPATITOR'] := lDataSet['BANCA_COD'];
        qryOrdonantare.Post;
      end;  
    finally
      lDataSet.Free;
    end;
  end;
end;

procedure TfrmAlopLichidare.edRepNumarContPropertiesInitPopup(Sender: TObject);
begin
  IdRepartitorChange(qryOrdonantare.FieldByName('id_repartitori'));
end;

procedure TfrmAlopLichidare.TreeRepartitoriDblClick(Sender: TObject);
begin
   with TcxDBTreeList(Sender) do
    if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk;
end;

procedure TfrmAlopLichidare.TreeRepartitoriKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
     TreeRepartitoriDblClick(TcxDBTreeList(Sender))
  else if Key = VK_ESCAPE then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrCancel;
end;

type
  TAccesscxPopupEdit = class(TcxPopupEdit);

procedure TfrmAlopLichidare.edtRepartitorPropertiesCloseQuery(
  Sender: TObject; var CanClose: Boolean);
var
  lNode : TcxDBTreeListNode;
  lIdRep : Integer;
begin
  with TAccesscxPopupEdit(Sender) do
    if PopupWindow.ModalResult = mrOk then begin
       lNode := TcxDBTreeListNode(TreeRepartitori.FocusedNode);
       if Assigned(lNode) then begin
          lIdRep := lNode.KeyValue;
          //Text := VarToStr(lNode.Values[TreeRepartitorinume.ItemIndex]);
          if not (qryOrdonantare.State in [dsEdit, dsInsert]) then qryOrdonantare.Edit;
          qryOrdonantare['ID_REPARTITORI']      := lIdRep;
          qryOrdonantare['NUME_REPARTITOR']     := lNode.Values[TreeRepartitorinume.ItemIndex];
          qryOrdonantare['CONT_REPARTITOR']     := Null;
          qryOrdonantare['BANCA_REPARTITOR']    := Null;
          qryOrdonantare['COD_BANCA_REPATITOR'] := Null;
          qryOrdonantare.Post;
       end;
    end;
end;

procedure TfrmAlopLichidare.edtRepartitorPropertiesPopup(Sender: TObject);
var
 lNode : TcxTreeListNode;
begin
  lNode := TreeRepartitori.FindNodeByKeyValue(qryOrdonantare['id_repartitori'], nil);
  if lNode <> nil then begin
    lNode.Focused := True;
    lNode.MakeVisible;
  end;
end;

procedure TfrmAlopLichidare.qryOrdonantareAfterOpen(DataSet: TDataSet);
begin
  //btnValidare.Enabled  := not DataSet.FieldByName('REDACTAT').AsBoolean;
  DataSet.FieldByName('ID_REPARTITORI').OnChange  := IdRepartitorChange;
  DataSet.FieldByName('SUMA_PLATA').OnChange      := OrdSumaPlataChange;
  DataSet.FieldByName('SUMA_AVANS').OnChange      := OrdSumaPlataChange;
  DataSet.FieldByName('ID_VALUTA').OnChange       := OrdIdValutaChange;
  DataSet.FieldByName('CURS_VALUTAR').OnChange    := OrdCursValutaChange;
  DataSet.FieldByName('SUMA_VALUTA').OnChange     := OrdCursValutaChange;
end;

procedure TfrmAlopLichidare.IdRepartitorChange(Sender: TField);
var
  lDataSet: TDataSet;
begin
  edRepNumarCont.Properties.Items.Clear;
  lDataSet := DBNewQueryFmt('SELECT DISTINCT CONT FROM .dbo.fnDefaultRepartitorConturi(%s)', [ValueToStr(Sender.Value)]);
  try
    lDataSet.Open;
    while not lDataSet.Eof do begin
      edRepNumarCont.Properties.Items.Add(lDataSet.Fields[0].AsString);
      lDataSet.Next;
    end;
  finally
    lDataSet.Free;
  end;
end;

procedure TfrmAlopLichidare.LoadOrdonantare(IdOrdonantare: Integer);
begin
  DBExecSQLFmt('exec [spAlopLoadOrdonantare] %d, %d', [IdUtilizator, IdOrdonantare]);
  ReadOrdonantare;
end;

procedure TfrmAlopLichidare.edtEsteValutaPropertiesChange(Sender: TObject);
begin
  lbTipValuta.Enabled := edtEsteValuta.Checked;
  lbCursSchimb.Enabled := edtEsteValuta.Checked;
  lbSumaValuta.Enabled := edtEsteValuta.Checked;
  edTipValuta.Enabled := edtEsteValuta.Checked;
  edtCursSchimb.Enabled := edtEsteValuta.Checked;
  edtSumaValuta.Enabled := edtEsteValuta.Checked;
end;


procedure TfrmAlopLichidare.btnValidareClick(Sender: TObject);
begin
  ValidareOrdonantare(True);
end;

procedure TfrmAlopLichidare.OrdSumaPlataChange(Sender: TField);
const
  IsCalc : Boolean = True;
var
  lDataSet: TDataSet;
  lPrevEdit: Boolean;
begin
  if IsCalc then begin
    lDataSet    := Sender.DataSet;
    lPrevEdit   := lDataSet.State in dsEditModes;
    if not lPrevEdit then lDataSet.Edit;
    IsCalc := False;
    lDataSet.FieldByName('SUMA_DATORATA').AsCurrency := lDataSet.FieldByName('SUMA_AVANS').AsCurrency + lDataSet.FieldByName('SUMA_PLATA').AsCurrency;
    if qryOrdonantare.FieldByName('ESTE_VALUTA').AsBoolean then
      if (Sender = lDataSet.FindField('SUMA_PLATA')) and (lDataSet.FieldByName('CURS_VALUTAR').AsCurrency <> 0) then
        lDataSet.FieldByName('SUMA_VALUTA').AsCurrency := lDataSet.FieldByName('SUMA_PLATA').AsCurrency / lDataSet.FieldByName('CURS_VALUTAR').AsCurrency;
    lDataSet.Post;
    if lPrevEdit then lDataSet.Edit;
    IsCalc := True;
  end;
end;

procedure TfrmAlopLichidare.AdaugaAngDefOrdDef;
begin
   if qryAngDef.IsEmpty then begin
      if FIdAngajament <> -1 then begin
        MessageDlg('Angajamentul curent nu contine nici o pozitie care poate fi folosita in ordonantare', mtWarning, [mbOK], 0);
        Abort;
      end
      else begin
        MessageDlg('Trebuie selectat un angajament pentru a putea introduce pozitile de ordonantat', mtWarning, [mbOK], 0);
        Abort;
      end;
   end;
   if (GridAngDefV.Controller.FocusedRecord <> nil) and GridAngDefV.Controller.FocusedRecord.IsData then begin
      if qryOrdDef.Locate('id_alop_angajamente_defalcare',  qryAngDef.FieldByName('id_alop_angajamente_defalcare').AsInteger, []) then begin
        MessageDlg('Atentie pozitia curenta a angajamentului mai exista in ordonantarea curenta !', mtError, [mbOK], 0);
        Abort;
      end;
      if (1=0) and (qryAngDef.FieldByName('disponibil_de_ord').AsCurrency = 0) then begin
        MessageDlg('Atentie pozitia curenta a angajamentului nu mai are sume disponibile !', mtError, [mbOK], 0);
        Abort;
      end;
      if qryOrdDef.Active then begin
        qryOrdDef.Append;
        qryOrdDef.FieldByName('id_alop_ordonantare').AsInteger := FIdOrdonantare;
        qryOrdDef.FieldByName('este_operator').AsBoolean := True;
        if not qryAngDef.IsEmpty then begin
          qryOrdDef.FieldByName('id_alop_angajamente_defalcare').AsInteger :=
            qryAngDef.FieldByName('id_alop_angajamente_defalcare').AsInteger;
          qryOrdDef.FieldByName('id_oi_unitati').AsInteger :=   qryAngDef.FieldByName('id_oi_unitati').AsInteger;
          qryOrdDef.FieldByName('id_oi_proiecte').AsInteger :=  qryAngDef.FieldByName('id_oi_proiecte').AsInteger;
          qryOrdDef.FieldByName('COD_FUNCTIONAL').AsString := qryAngDef.FieldByName('COD_FUNCTIONAL').AsString;
          qryOrdDef.FieldByName('COD_ECONOMIC').AsString := qryAngDef.FieldByName('COD_ECONOMIC').AsString;
          qryOrdDef.FieldByName('DISPONIBIL_INAINTE').AsCurrency := qryAngDef.FieldByName('DispInainte').AsCurrency;
          qryOrdDef.FieldByName('DISPONIBIL_TRIM_INAINTE').AsCurrency := qryAngDef.FieldByName('DispTrimInainte').AsCurrency;
          qryOrdDef.FieldByName('DISPONIBIL_AN_INAINTE').AsCurrency := qryAngDef.FieldByName('DispAnInainte').AsCurrency;           
          qryOrdDef.FieldByName('SUMA_PLATA').AsCurrency := qryAngDef.FieldByName('disponibil_de_ord').AsCurrency;
        end;
        qryOrdDef.Post;
      end;
   end;
end;

procedure TfrmAlopLichidare.Adaugapozitie1Click(Sender: TObject);
begin
  AdaugaAngDefOrdDef;
end;

procedure TfrmAlopLichidare.VerificaOrdonantare;
var
  lErrorList  : TStringList;
  lDataSet    : TDataSet;
begin
  lErrorList := TStringList.Create;
  try
    CheckAndSave;
    lDataSet := DBNewQueryFmt('exec [spAlopOrdVerificaEcran] %d', [IdOrdonantare]);
    try
      lDataSet.Open;
      while not lDataSet.Eof do begin
        lErrorList.Add(lDataSet.FieldByName('Explicatie').AsString);
        lDataSet.Next;
      end;
    finally
      lDataSet.Free;
    end;
    if lErrorList.Count > 0 then
      raise Exception.Create(lErrorList.Text);
  finally
    lErrorList.Free;
  end;
end;

procedure TfrmAlopLichidare.DTOrdonantareDataChange(Sender: TObject;
  Field: TField);
begin
{  if (Field <> nil) and (Field.FieldName = 'REDACTAT') then
    btnValidare.Enabled := not Field.AsBoolean;}
end;

procedure TfrmAlopLichidare.SetStareEditare(ReadOnly: Boolean);
begin
{Bobo:Adaugat}
  qryOrdDef.ReadOnly := ReadOnly;
  qryOrdonantare.ReadOnly := ReadOnly;
end;

procedure TfrmAlopLichidare.OrdCursValutaChange(Sender: TField);
var
  lDataSet: TDataSet;
  lPrevEdit: Boolean;
begin
  if qryOrdonantare.FieldByName('ESTE_VALUTA').AsBoolean then begin
    if not IsInLoading then begin
      IsInLoading := True;
      lDataSet    := Sender.DataSet;
      lPrevEdit   := lDataSet.State in dsEditModes;
      if not lPrevEdit then lDataSet.Edit;
      lDataSet.FieldByName('SUMA_PLATA').AsCurrency := lDataSet.FieldByName('SUMA_VALUTA').AsCurrency * lDataSet.FieldByName('CURS_VALUTAR').AsCurrency;
      lDataSet.Post;
      if lPrevEdit then lDataSet.Edit;
      IsInLoading := False;
    end;
  end;
end;

procedure TfrmAlopLichidare.btnValidareListareClick(Sender: TObject);
begin
  CheckAndSave;
  ValidareOrdonantare(False);
  PrintOrdonantare(IdOrdonantare);
  OrdonantareNoua(False);
end;

procedure TfrmAlopLichidare.CheckAndSave;
begin
  if qryOrdonantare.State in [dsEdit, dsInsert] then QryOrdonantare.Post;
  if qryOrdDef.State in [dsEdit, dsInsert] then qryOrdDef.Post;
end;

procedure TfrmAlopLichidare.FormShow(Sender: TObject);
begin
  btnValidareListare.Left := pnBottom.Width - btnValidareListare.Width - 2;
  btnValidare.Left        := btnValidareListare.Left - btnValidare.Width - 5;
  InfoMemo.Width          := btnValidare.Left - InfoMemo.Left - 5;

  ReadOrdonantare;

  if qryOrdonantare.Active then
  begin
    if not (qryOrdonantare.State in [dsEdit, dsInsert]) then
      qryOrdonantare.Edit;

    qryOrdonantare.FieldByName('DATA_EMITERE').AsDateTime := Date;
    qryOrdonantare.Post;

    edtDataEmitere.EditValue := Date;
  end;
end;


procedure TfrmAlopLichidare.btnCFAddClick(Sender: TObject);
begin
  AdaugaAngDefOrdDef;
end;

procedure TfrmAlopLichidare.btnNewOrdClick(Sender: TObject);
begin
  OrdonantareNoua(True);
end;

procedure TfrmAlopLichidare.btnAnuleazaOrdClick(Sender: TObject);
begin
  if (IdOrdonantare > 0) then begin
    //TestGolireEcran;
    if (MessageDlg('Doriti stergerea ordonantarii curente ?', mtConfirmation, [mbYes, mbNo], 0) in [mrNo, mrNone]) then
       Abort;
    DBExecSQLFmt('exec [spAlopAnuleazaOrdonantare] %d', [IdOrdonantare]);
    RefaDupaModificare;    
    ReadOrdonantare;
  end;
end;

procedure TfrmAlopLichidare.BtnModificareOrdClick(Sender: TObject);
var
  lfrmAleg : TfrmALOPListaOrd;
begin
  TestGolireEcran;
  if not IsOrdValid then DBExecSQLFmt('exec [spAlopAnuleazaOrdonantare] %d', [IdOrdonantare]);
  lfrmAleg := TfrmALOPListaOrd.Create(Self);
  with lfrmAleg do
    try
      lfrmAleg.btnRefaOrdonantare.Visible := False;
      lfrmAleg.btnAnuleazaAng.Visible := False;
      lfrmAleg.btnOrdonantare.Visible := False;
      lfrmAleg.btnRefresh.Visible := True;
      lfrmAleg.BtnOk.Visible := True;
      lfrmAleg.BtnCancel.Visible := True;
      lfrmAleg.WindowState := wsMaximized;
      if ShowModal = mrOk then begin;
         LoadOrdonantare(lfrmAleg.IdOrdonantare);
      end;
    finally
      Free;
    end;
end;

function TfrmAlopLichidare.IsOrdValid: Boolean;
begin
  Result := False;
  if not qryOrdonantare.IsEmpty then
    Result := qryOrdonantare.FieldByName('REDACTAT').AsBoolean;
end;

procedure PrintOrdonantare(lIdOrdonantare : Integer);
var
  aIdReport : Integer;
begin

   DateUnit.IdOrdonantare := lIdOrdonantare;
   aIdReport :=  GetItemId('Ordonantare');
   if aIdReport <> -1 then begin
     LoadReport(aIdReport);
   end;
     //WriteReportToRepository(aIdReport, 'Ordonantare', lIdOrdonantare);
end;

function ModificareOrdonantare(lIdOrdonantare : Integer) : TForm;
begin
  if EnterSingleUser(TfrmAlopLichidare) then begin
    Result := TForm(GetNewForm(TfrmAlopLichidare));
    with TfrmAlopLichidare(Result) do begin
  
      WindowState := wsMaximized;
      DBExecSQLFmt('exec [spAlopOrdInvalidare] %d', [IdUtilizator]);
      LoadOrdonantare(lIdOrdonantare);
      ReadOrdonantare;
    end;
  end;
end;

procedure TfrmAlopLichidare.RefaDupaModificare;
begin
  DBExecSQLFmt('exec [spAlopOrdRevalidare] %d', [IdUtilizator]);
end;

procedure TfrmAlopLichidare.TestGolireEcran;

  procedure SetTransarentControl(AControl: TControl);
  var
    lPropInfo: PPropInfo;
    I: Integer;
  begin
    if Assigned(AControl) then begin
      if AControl is TLabel then
        TLabel(AControl).Transparent := True;
      {
      begin
        lPropInfo := GetPropInfo(AControl, 'Transparent');
        if Assigned(lPropInfo) then
          SetOrdProp(AControl, lPropInfo, Ord(True));
      end;
      }
      if AControl is TWinControl then begin
        for I := 0 to TWinControl(AControl).ControlCount-1 do
          SetTransarentControl(TWinControl(AControl).Controls[I]);
      end;
    end;
  end;

var
  lForm: TForm;
begin
  if not IsOrdValid and not qryOrdDef.IsEmpty then begin
    lForm := CreateMessageDialog('Aveti informatii introduse in ecran.'#13#10'Operatia nu poate continua !'#13#10'Mai intai SALVATI sau ANULATI documentul curent !', mtError, [mbOK]);
    try
      lForm.Font.Color  := clWhite;
      lForm.Font.Style  := lForm.Font.Style + [fsBold]; 
      lForm.Caption     := 'AVERTISMENT';
      lForm.Brush.Color := clRed;
      lForm.Position    := poScreenCenter;
      SetTransarentControl(lForm);
      lForm.ShowModal;
    finally
      lForm.Free;
    end;
    Abort;
  end;
end;

procedure TfrmAlopLichidare.CalculeazaSumaPlata;
var
  lBookMark : TBookMark;
  lEditState : Boolean;
  lSuma : Currency;
begin
  //daca este inchis sau gol punem 0
  if (not qryOrdDef.Active) or (qryOrdDef.IsEmpty) then begin
    DBSetFieldValue(qryOrdonantare, 'SUMA_PLATA', 0);
    Exit;
  end;
  //daca nu facem suma
  with qryOrdDef do
    try
      DisableControls;
      lEditState := qryOrdDef.State in dsEditModes;
      lBookMark := GetBookmark;
      First;
      lSuma := 0;
      while not eof do begin
        lSuma := lSuma + FieldByName('SUMA_PLATA').AsCurrency;
        Next;
      end;
      DBSetFieldValue(qryOrdonantare, 'SUMA_PLATA', lSuma);
      GotoBookmark(lBookMark);      
    finally
      EnableControls;
      FreeBookmark(lBookMark);
      if lEditState then qryOrdDef.Edit;
    end;
end;

procedure TfrmAlopLichidare.SetDetaliiRedactare(IsFinalizat: Boolean);
begin
//  edtValidat.Enabled            := not IsFinalizat;
  edtNrOrdonantare.Enabled      := not IsFinalizat and edtValidat.Checked;
  edtDataEmitere.Enabled        := not IsFinalizat and edtValidat.Checked;
  edtNrOrdine.Enabled           := not IsFinalizat and edtValidat.Checked;
  btnAngajamentOrd.Enabled      := not IsFinalizat;
  edtNrAngajament.Enabled       := not IsFinalizat;
  edtDataAngajament.Enabled     := not IsFinalizat;
  edtNaturaCheltuielii.Enabled  := not IsFinalizat;
  edtRepartitor.Enabled         := not IsFinalizat;
  edtAdresaRepartitor.Enabled   := not IsFinalizat;
  edRepNumarCont.Enabled        := not IsFinalizat;
  edtBancaRepartitor.Enabled    := not IsFinalizat;
  edtCodBanca.Enabled           := not IsFinalizat;
  GridAngDef.Enabled            := not IsFinalizat;
  GridObligatii.Enabled         := not IsFinalizat;
  btnCFAdd.Enabled              := not IsFinalizat;
  btnCFDel.Enabled              := not IsFinalizat;
  btnCFUpd.Enabled              := not IsFinalizat;
  edObligatii.Enabled           := not IsFinalizat;
  edAvans.Enabled               := not IsFinalizat;
  edSumaRON.Enabled             := not IsFinalizat;
  edModPlata.Enabled            := not IsFinalizat;
  edtEsteValuta.Enabled         := not IsFinalizat;
  edTipValuta.Enabled           := not IsFinalizat and edtEsteValuta.Checked;
  edtCursSchimb.Enabled         := not IsFinalizat and edtEsteValuta.Checked;
  edtSumaValuta.Enabled         := not IsFinalizat and edtEsteValuta.Checked;
  edtDocumenteLichidate.Enabled := not IsFinalizat;
end;

procedure TfrmAlopLichidare.btnEditRepartitorClick(Sender: TObject);
begin
  IntretinereRepartitor(qryOrdonantare.FieldByName('ID_REPARTITORI').AsInteger);
  DBRefresh(qryRepartitori);
end;

function TfrmAlopLichidare.TestUniqueNumber: Boolean;
var
  lNumar, lExplic : String;
  lValue  : Variant;
begin
  //testam daca numarul alocat este unic si intrebam daca vrea sa-l pastreze sau sa genereze unul nou
  Result := True;
  lValue := DBGetScallarFmt('exec [spAlopOrdTestNumber] %d', [IdOrdonantare]);
  if VarIsArray(lValue) and ValueHasValue(lValue[0]) and ValueHasValue(lValue[1]) then begin
    lNumar  := ValueSafeToStr(lValue[0]);
    lExplic := ValueSafeToStr(lValue[1]);
    case MessageDlg(Format('Numarul %s este deja folosit in %s. ', [lNumar, lExplic])+#13+#10+'Doriti generarea unui numar nou ?'+#13+#10+ '(Daca apasti Yes se va genera un nou numar, No va lasa numarul curent, Cancel revine in ecranul de culegere fara salvare.)', mtConfirmation, [mbYes, mbNo, mbCancel], 0) of
         mrYes : edtNrOrdonantarePropertiesButtonClick(edtNrOrdonantare, 0) ;
         mrNo :  ;
         mrCancel, mrNone: Result := False;
     end;
  end;
end;

procedure TfrmAlopLichidare.OrdIdValutaChange(Sender: TField);
var
  lDataSet: TDataSet;
begin
  if qryOrdonantare.FieldByName('ESTE_VALUTA').AsBoolean then begin
    lDataSet := DBNewQueryFmt('exec [spAlopAngCursSchimb] %s, %d', [ValueToStr(qryOrdonantare['DATA_EMITERE']), Sender.AsInteger]);
    try
      lDataSet.Open;
      if lDataSet.IsEmpty then
         qryOrdonantare.FieldByName('CURS_VALUTAR').AsCurrency := 1
      else if (lDataSet.Fields[1].AsInteger < 7) or
              (MessageDlg('Cursul de schimb pentru valuta selectata este mai vechi de 7 zile'#13#10+
                          'Ultima data la care a fost actualizat este : '+FormatDateTime('dd.mm.yyyy', lDataSet.Fields[2].AsDateTime)+#13#10+
                          'Doriti folosirea acestui curs ?', mtConfirmation, [mbYes, mbNo], 0) = mrYes) then
              qryOrdonantare.FieldByName('CURS_VALUTAR').AsCurrency := lDataSet.Fields[0].AsCurrency;

    finally
      lDataSet.Free;
    end;
  end;
end;

procedure TfrmAlopLichidare.OrdonantareNoua(TestGolire: Boolean);
begin
  if TestGolire then TestGolireEcran;
  if not IsOrdValid then DBExecSQLFmt('exec [spAlopGolesteOrdonantare] %d', [IdOrdonantare]);
  ReadOrdonantare;
  cxPageLichidare.ActivePage := tabOrdonantare;
end;
 //test
procedure TfrmAlopLichidare.ValidareOrdonantare(CuOrdonantareNoua: Boolean);
begin
  VerificaOrdonantare;
  if TestUniqueNumber then begin
    DBExecSQLFmt('exec [spAlopValidareOrdonantare] %d, %d', [FIdOrdonantare, 0]);
    qryOrdonantare.Refresh;
    RefaDupaModificare;
  end;
  if CuOrdonantareNoua then
    OrdonantareNoua(False);
end;

end.



