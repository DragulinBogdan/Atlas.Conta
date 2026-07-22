unit AlopAngajamente;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs, StdCtrls, Buttons,
  ExtCtrls, Db, ZDataSet, Menus, cxLookAndFeelPainters, cxButtons, cxGroupBox, cxRepartitorPanel,
  cxCalendar, cxTextEdit, cxControls, cxContainer, cxEdit, cxMaskEdit, cxDropDownEdit, cxStyles,
  cxGraphics, cxDataUtils, cxDataStorage, cxDBData, cxGridLevel, cxClasses, cxGridCustomView,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGrid, cxCurrencyEdit, cxImageComboBox,
  cxGridBandedTableView, cxGridDBBandedTableView, cxButtonEdit, cxTL, cxInplaceContainer, cxTLData,
  cxDBTL, cxProgressBar, ZAbstractRODataset, ZAbstractDataset, cxTLdxBarBuiltInMenu, cxLookAndFeels,
  cxCustomData, cxFilter, cxData, cxPC, cxCheckBox, frmSelectieContractUnit, frmSelectieDosarUnit,
  ZSqlUpdate, cxSpinEdit, cxCalc, ActnList, cxGridCustomPopupMenu, cxGridPopupMenu, fmSelectieCEUnit,
  fmSelectieCFUnit, fmSelectieRepartitorUnit, cxLabel, cxNavigator, ComCtrls, dxCore, cxDateUtils,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxBarBuiltInMenu,
  dxDateRanges, dxScrollbarAnnotations;

const
  WM_REFRESH_DEFALCARE = WM_USER + 1;
  
type
  PInfoFunctional = ^TInfoFunctional;
  TInfoFunctional = record
    Id : Variant;
    IdUnitate : Variant;
    CodFunctional : string[100];
    CodEcran : string[100];
    Denumire : string[254];
  end;


  TfrmAlopAngajamente = class(TForm)
    pnDocument: TPanel;
    lbDocument: TLabel;
    LbDataNota: TLabel;
    LbPredator: TLabel;
    pnClient: TPanel;
    DTAngajamente: TDataSource;
    QryAngajamenteDefalcate: TZQuery;
    LbPrimitor: TLabel;
    QryAngajamente: TZQuery;
    Label1: TLabel;
    Label2: TLabel;
    pnBottom: TPanel;
    lbContract: TLabel;
    BtnOk: TcxButton;
    btnCancel: TcxButton;
    BtnModificare: TcxButton;
    btnAnuleazaAng: TcxButton;
    edPredator: TcxPopupEdit;
    edPrimitor: TcxPopupEdit;
    edtDetaliiContract: TcxPopupEdit;
    edDataDoc: TcxDateEdit;
    gridAngajamentDetaliu: TcxGrid;
    nivelAngajamentDetaliu: TcxGridLevel;
    viewAngajamentDetaliu: TcxGridDBBandedTableView;
    viewAngajamentDetaliuBUGET: TcxGridDBBandedColumn;
    viewAngajamentDetaliuDESC_BUGET: TcxGridDBBandedColumn;
    viewAngajamentDetaliuDESCRIERE: TcxGridDBBandedColumn;
    viewAngajamentDetaliuAPROBATE: TcxGridDBBandedColumn;
    viewAngajamentDetaliuTOTAL_ANGAJATE: TcxGridDBBandedColumn;
    viewAngajamentDetaliuDISPONIBIL: TcxGridDBBandedColumn;
    viewAngajamentDetaliuID_VALUTA: TcxGridDBBandedColumn;
    viewAngajamentDetaliuANGAJAT_VALUTA: TcxGridDBBandedColumn;
    viewAngajamentDetaliuCURS_VALUTAR: TcxGridDBBandedColumn;
    viewAngajamentDetaliuANGAJAT: TcxGridDBBandedColumn;
    viewAngajamentDetaliuRAMAS_DE_ANGAJAT: TcxGridDBBandedColumn;
    viewAngajamentDetaliuVALIDAT: TcxGridDBBandedColumn;
    edTipAngajament: TcxImageComboBox;
    edNumarDoc: TcxButtonEdit;
    cxStyleRepository: TcxStyleRepository;
    cxStyle1: TcxStyle;
    cxStyle2: TcxStyle;
    cxStyleRepository1: TcxStyleRepository;
    cxStyle3: TcxStyle;
    cxStyle4: TcxStyle;
    cxStyle5: TcxStyle;
    btnNewAng: TcxButton;
    edRectificat: TcxButtonEdit;
    Label3: TLabel;
    cxFunctionalBar: TcxPopupEdit;
    LbScopul: TLabel;
    edNrProiect: TcxButtonEdit;
    btnRapoarte: TcxButton;
    edtScop: TcxTextEdit;
    Label4: TLabel;
    lbLegal: TLabel;
    edLegal: TcxButtonEdit;
    ppMenu: TPopupMenu;
    Actiuni: TActionList;
    cxGridPopupMenu: TcxGridPopupMenu;
    ppAdauga: TMenuItem;
    ppSterge: TMenuItem;
    Cmd_AdaugaDetaliuAngajament: TAction;
    Cmd_StergeDetaliuAngajament: TAction;
    viewAngajamentDetaliuid_oi_proiecte: TcxGridDBBandedColumn;
    btnAdaugaPozitii: TcxButton;
    cxButton2: TcxButton;
    edSumaProiect: TcxCurrencyEdit;
    lbSumaProiect: TcxLabel;
    viewAngajamentDetaliueste_procentual: TcxGridDBBandedColumn;
    viewAngajamentDetaliuprocProiect: TcxGridDBBandedColumn;
    viewAngajamentDetaliuangProiect: TcxGridDBBandedColumn;
    stilProiectCurent: TcxStyle;
    stilNormal: TcxStyle;
    btnOkGenerare: TcxButton;
    lbNumar: TLabel;
    edtDetaliiDosar: TcxPopupEdit;
    Button1: TButton;
    chkRectificareSoldInitial: TcxCheckBox;
    chkRectificareSold: TcxCheckBox;
    procedure QryAngajamenteDefalcateNewRecord(DataSet: TDataSet);
    procedure QryAngajamenteDefalcateAfterOpen(DataSet: TDataSet);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure BtnOkClick(Sender: TObject);
    procedure pnDocumentResize(Sender: TObject);
    procedure edPredatorEnter(Sender: TObject);
    procedure edPredatorKeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
    procedure edDataDocValidate(Sender: TObject; var ErrorText: String; var Accept: Boolean);
    procedure aKeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
    procedure FormCreate(Sender: TObject);
    procedure SetAngFieldValue(FieldName: String; Value: Variant);
    procedure edDataDocChange(Sender: TObject);
    procedure edTipAngajamentChange(Sender: TObject);
    procedure edScopulKeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
    procedure QryAngajamenteDefalcateBeforeDelete(DataSet: TDataSet);
    procedure QryAngajamenteNewRecord(DataSet: TDataSet);
    procedure edTipAngajamentValidate(Sender: TObject; var ErrorText: String; var Accept: Boolean);
    procedure QryAngajamenteDefalcateAfterPost(DataSet: TDataSet);
    procedure BtnModificareClick(Sender: TObject);
    procedure btnAnuleazaAngClick(Sender: TObject);
    procedure edtDetaliiContractKeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
    procedure FormDestroy(Sender: TObject);
    procedure btnCancelClick(Sender: TObject);
    procedure edNumarDocKeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
    procedure edDataDocPropertiesValidate(Sender: TObject; var DisplayValue: Variant; var ErrorText: TCaption; var Error: Boolean);
    procedure edTipAngajamentPropertiesChange(Sender: TObject);
    procedure edTipAngajamentPropertiesValidate(Sender: TObject; var DisplayValue: Variant; var ErrorText: TCaption; var Error: Boolean);
    procedure edPredatorPropertiesInitPopup(Sender: TObject);
    procedure edPredatorPropertiesCloseUp(Sender: TObject);
    procedure edNumarDocPropertiesButtonClick(Sender: TObject; AButtonIndex: Integer);
    procedure edtDetaliiContractPropertiesInitPopup(Sender: TObject);
    procedure viewAngajamentDetaliuDESC_BUGETGetDisplayText(
      Sender: TcxCustomGridTableItem; ARecord: TcxCustomGridRecord;
      var AText: String);
    procedure btnNewAngClick(Sender: TObject);
    procedure edtDetaliiContractPropertiesPopup(Sender: TObject);
    procedure FormShow(Sender: TObject);
    procedure edPredatorPropertiesPopup(Sender: TObject);
    procedure FormActivate(Sender: TObject);
    procedure edDataDocPropertiesEditValueChanged(Sender: TObject);
    procedure pnBottomResize(Sender: TObject);
    procedure edRectificatPropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
    procedure edNrContractPropertiesChange(Sender: TObject);
    procedure edDataContractPropertiesEditValueChanged(Sender: TObject);
    procedure edDataContractPropertiesValidate(Sender: TObject;
      var DisplayValue: Variant; var ErrorText: TCaption;
      var Error: Boolean);
    procedure cxFunctionalBarPropertiesCloseUp(Sender: TObject);
    procedure cxFunctionalBarPropertiesPopup(Sender: TObject);
    procedure edNrProiectPropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
    procedure QryEconomicAfterOpen(DataSet: TDataSet);
    procedure edLegalPropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
    procedure Cmd_StergeDetaliuAngajamentUpdate(Sender: TObject);
    procedure Cmd_StergeDetaliuAngajamentExecute(Sender: TObject);
    procedure Cmd_AdaugaDetaliuAngajamentExecute(Sender: TObject);
    procedure cxGridProcentEnter(Sender: TObject);
    procedure viewAngajamentDetaliuBUGETPropertiesCloseUp(Sender: TObject);
    procedure viewAngajamentDetaliuBUGETPropertiesPopup(Sender: TObject);
    procedure viewAngajamentDetaliuBUGETPropertiesInitPopup(
      Sender: TObject);
    procedure viewAngajamentDetaliuFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure edSumaProiectPropertiesValidate(Sender: TObject;
      var DisplayValue: Variant; var ErrorText: TCaption;
      var Error: Boolean);
    procedure edSumaProiectKeyUp(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure btnOkGenerareAngajament(Sender: TObject);
    procedure btnOkGenerareFactura(Sender: TObject);
    procedure edNumarDocPropertiesEditValueChanged(Sender: TObject);
    procedure edNrProiectPropertiesEditValueChanged(Sender: TObject);
    procedure edtDetaliiContractPropertiesCloseUp(Sender: TObject);
    procedure edtScopPropertiesEditValueChanged(Sender: TObject);
    procedure cxCheckBox1PropertiesEditValueChanged(Sender: TObject);
    procedure viewAngajamentDetaliuStylesGetContentStyle(
      Sender: TcxCustomGridTableView; ARecord: TcxCustomGridRecord;
      AItem: TcxCustomGridTableItem; var AStyle: TcxStyle);
    procedure edtDetaliiDosarKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure edtDetaliiDosarPropertiesCloseUp(Sender: TObject);
    procedure edtDetaliiDosarPropertiesPopup(Sender: TObject);
    procedure Button1Click(Sender: TObject);
   // procedure chkRectificareSoldClick(Sender: TObject);
    procedure chkRectificareSoldInitialClick(Sender: TObject);

   // procedure edPrimitorPropertiesChange(Sender: TObject);
  private
    FIdProiect          : Variant;
    FAreDefalcareProcent: Boolean;
    FCurentAngajament: Integer;
    IsInLoading      : Boolean;
    FErrRecord : String;
    FExecOnValidation: TNotifyEvent;
    FInfoFunctional : PInfoFunctional;
    FSelectieContract : TfrmSelectieContract;
    FSelectieDosar    : TfrmSelectieDosar;
    FSelectieCE       : TfmSelectieCE;
    FSelectieCF       : TfmSelectieCF;
    FSelectieDep      : TfmSelectieRepartitor;
    FSelectieBen      : TfmSelectieRepartitor;
    FIsInDelete       : Boolean;
    procedure ClearAngInBaza;
    procedure ClearAngParinte;
    procedure PreiaSelectieCE(const IsSelected: Boolean=False);
    procedure RecalcItemsi;
    procedure SetCurentAngajament(const Value: Integer);
    procedure ValidareContBuget(Sender: TField);
    procedure ValidateValuta(Sender: TField);
    procedure ValidateAngajat(Sender: TField);
    procedure ValidateProiect(Sender: TField);
    procedure ValidateCursSchimb(Sender: TField);
    { Private declarations }
    procedure SetNextControl;
    function  cxNodeByText(aTree : TcxDBTreeList; aColumn: TcxTreeListColumn; AText : String) : TcxTreeListNode;
    procedure ActivateGrid;
    procedure ClearAngajamente;
    function  ValidareAngajamentEcran(const NeedFilled : Boolean = True) : Boolean;
    procedure LocalModificValidation(Sender: TObject);
    function  NewAngajament: Integer;
    procedure FillInfoFunctional;
    procedure ReportClick(Sender: TObject);
    procedure SetLegalVisibility(aVisible : Boolean);
    procedure SetAngConex(refAngConex: Integer);
    procedure HideControlsOnSQL;
    procedure WmRefreshDefalcare(var Message: TMessage); message WM_REFRESH_DEFALCARE;
  public
    FModificare   : Boolean;

    function TestUniqueNumber : Boolean;
    procedure TestGolireEcran;
    procedure ReadAngajament(const IdAngajament : Integer = 0);
    procedure SetContract(IdContract : Integer);
    procedure LoadAngajament(IdAngajament : Integer);
    property  CurentAngajament: Integer read FCurentAngajament write SetCurentAngajament;
    property  ExecOnValidation : TNotifyEvent read FExecOnValidation write FExecOnValidation;
    //  procedure IncarcaDate(const ACodEconomic: String; const ADisponibil: Currency);

    { Public declarations }
  protected
    procedure PopulateTipAngajament;
    procedure SetDetaliiValidare(IsValidat : Boolean);
    function IsAngValidat : Boolean;

  end;

function  ModificareAngajament(IdAngajament : Integer) : TForm;
procedure PrintAngajament(lIdAngajament : Integer; ang : Boolean);
function OkToModify(aId : Integer; const aSilent:Boolean = False): Boolean;


implementation

{$R *.DFM}

uses
  Math, dxCompsUtile, ZeosDBUtile, CommonDBVar, ConcurentUsersUnit, Variants, rapInclude,
  AtlasUtils, FormulareUnit, AlopAngVizualizare, ATSZDBUtils, Types, AlopAngDisponibil,
  DateUnit, AlopAngDefalcarePeSume;


function OkToModify(aId: Integer; const aSilent: Boolean): Boolean;
var
  lValue: Variant;
begin
  lValue := DBGetScallarFmt('exec [spAlopAngUsed] ', [aId]);
  if not aSilent and (ValueSafeToStr(lValue) > '')  then MessageDlg(ValueSafeToStr(lValue), mtError, [mbOK], 0);
end;

function ModificareAngajament(IdAngajament : Integer) : TForm;
begin
  if EnterSingleUser(TfrmAlopAngajamente) then begin
    Result := TForm(GetNewForm(TfrmAlopAngajamente));
    with TfrmAlopAngajamente(Result) do begin
      FModificare := True;
      WindowState := wsMaximized;
      DBExecSQLFmt('exec [spAlopInvalideazaCulegere] %d', [IdUtilizator]);
      LoadAngajament(IdAngajament);
      FExecOnValidation := LocalModificValidation;
    end;
  end;
end;


procedure PrintAngajament(lIdAngajament : Integer; ang : Boolean);
var
  aQry : TZReadOnlyQuery;
  aIdReport : Integer;
  aStr : String;
begin
   aStr := '';

   if ang then begin
    aStr := ValueSafeToStr(DBGetScallarFmt('exec [spAlopGetRaport] %d', [lIdAngajament]));
    if aStr = '' then
      aStr :=  'Angajament'
   end
   else
      aStr := 'Propunere';

   aIdReport := -1;
   if aStr <> '' then
     aIdReport := ValueSafeToInt( DBGetScallarFmt('SELECT ITEM_ID FROM RAPOARTE_ASOCIERE WHERE NUME_RAPORT = %s', [ValueToStr(aStr)]), -1 );

   if aIdReport <> -1 then begin
     LoadReport(aIdReport, 'ID_ANGAJAMENT', [lIdAngajament]);
     //WriteReportToRepository(aIdReport, 'Angajament', IdAngajament);
   end;
end;

procedure TfrmAlopAngajamente.QryAngajamenteDefalcateNewRecord(DataSet: TDataSet);
begin
  if CurentAngajament <= 0 then Abort;
  //setam id de versiune pentru parinte

  with DataSet do begin
    FieldByName('ID_UTILIZATORI').AsInteger       := IdUtilizator;
    FieldByName('ID_ALOP_ANGAJAMENTE').AsInteger  := CurentAngajament;
    if TcxImageComboBoxProperties(viewAngajamentDetaliuID_VALUTA.Properties).Items.Count > 0 then
       FieldByName('ID_VALUTA').AsInteger :=  TcxImageComboBoxProperties(viewAngajamentDetaliuID_VALUTA.Properties).Items[0].Value;
  end;
end;

procedure TfrmAlopAngajamente.SetCurentAngajament(const Value: Integer);
begin
  FCurentAngajament := Value;
  FSelectieCE.SetParameter('id_alop_angajament', FCurentAngajament);
  with QryAngajamente do begin
    if FCurentAngajament = -1 then
       FCurentAngajament := NewAngajament
    else begin
      Close;
      Params[0].Value := Value;
      Open;
    end;

    edNumarDoc.Text := FieldByName('NUMAR').AsString;
    edNrProiect.Text := FieldByName('NR_PROIECT').AsString;
    if FieldByName('DATA_EMITERE').Value = Null then
      edDataDoc.Clear
    else
      edDataDoc.Date  := FieldByName('DATA_EMITERE').AsDateTime;

    FSelectieContract.edNrContract.Text := FieldByName('NR_CONTRACT').AsString;
    if FieldByName('DATA_CONTRACT').Value = Null then
      FSelectieContract.edDataContract.Clear
    else
      FSelectieContract.edDataContract.Date  := FieldByName('DATA_CONTRACT').AsDateTime;
    FSelectieDosar.SetIdDosar(QryAngajamente['refDosar']);
    edtDetaliiDosar.EditValue := FSelectieDosar.GetIdDosar;

    if Trim(FieldByName('TIP_ANGAJAMENT').AsString) = '' then
      edTipAngajament.ItemIndex   := 0
    else
      edTipAngajament.EditValue   := FieldByName('TIP_ANGAJAMENT').AsInteger;
    edPredator.Tag  := FieldByName('ID_DEPARTAMENT').AsInteger;
    edPrimitor.Tag  := FieldByName('ID_LST_REPARTITORI').AsInteger;

    FInfoFunctional^.CodEcran := FieldByName('COD_ECRAN').AsString;
    FillInfoFunctional;

    FSelectieCE.SetParameter('cod_functional', FInfoFunctional^.CodFunctional);
    FSelectieCE.SetParameter('id_oi_unitati', FInfoFunctional^.IdUnitate);

    edtScop.Text := FieldByName('SCOPUL').AsString;
    SetContract(FieldByName('ID_CONTRACT').AsInteger);
    SetDetaliiValidare(FieldByName('VALIDAT').AsInteger=1);
  end;
  QryAngajamenteDefalcate.Close;
  QryAngajamenteDefalcate.ParamByName('ID').Value := FCurentAngajament;
  QryAngajamenteDefalcate.Open;

  ActivateGrid;
  SetNextControl;
end;

procedure TfrmAlopAngajamente.QryAngajamenteDefalcateAfterOpen(DataSet: TDataSet);
begin
  FAreDefalcareProcent := DataSet.FindField('procProiect') <> nil;
  DataSet.FieldByName('COD_ECRAN').OnValidate := ValidareContBuget;
  
  with DataSet.FieldByName('ID_VALUTA') do begin
    OnValidate := ValidateCursSchimb;
  end;
  with DataSet.FieldByName('CURS_VALUTAR') do begin
    Tag := 0;
    OnValidate := ValidateValuta;
  end;
  with DataSet.FieldByName('ANGAJAT_VALUTA') do begin
    Tag := 1;
    OnValidate := ValidateValuta;
  end;
  DataSet.FieldByName('ANGAJAT').OnValidate := ValidateAngajat;
  DataSet.FieldByName('id_oi_proiecte').OnValidate := ValidateProiect;
end;


procedure TfrmAlopAngajamente.chkRectificareSoldInitialClick(Sender: TObject);
var
  focusedRecord: TcxCustomGridRecord;
  currentValue: Boolean;
begin
  focusedRecord := viewAngajamentDetaliu.Controller.FocusedRecord;

  if not Assigned(focusedRecord) or not focusedRecord.IsData then
  begin
    MessageDlg('Selectează un rând valid în grilă înainte de a bifa opțiunea.', mtWarning, [mbOK], 0);
    Exit;
  end;

  QryAngajamenteDefalcate.RecNo := focusedRecord.RecordIndex + 1;

  if QryAngajamenteDefalcate.State in [dsBrowse] then
    QryAngajamenteDefalcate.Edit;

  currentValue := QryAngajamenteDefalcate.FieldByName('SoldRectificat').AsBoolean;

  QryAngajamenteDefalcate.FieldByName('SoldRectificat').AsBoolean := not currentValue;

  if not currentValue then
  begin

    QryAngajamenteDefalcate.FieldByName('APROBATE').AsCurrency := 0;
    QryAngajamenteDefalcate.FieldByName('TOTAL_ANGAJATE').AsCurrency := 0;
    QryAngajamenteDefalcate.FieldByName('DISPONIBIL').AsCurrency := 0;
    QryAngajamenteDefalcate.FieldByName('RAMAS_DE_ANGAJAT').AsCurrency := 0;
  end;

  QryAngajamenteDefalcate.Post;


  chkRectificareSoldInitial.OnClick := nil;
  chkRectificareSoldInitial.Checked := QryAngajamenteDefalcate.FieldByName('SoldRectificat').AsBoolean;
  chkRectificareSoldInitial.OnClick := chkRectificareSoldInitialClick;
end;



procedure TfrmAlopAngajamente.ValidareContBuget(Sender: TField);
var
  lNode       : TcxTreeListNode;
  lPrevValue  : String;
begin
  { Se face drop down si se invalideaza campul in cazul in care :
    se completeaza cu nimic sau ? sau clasificatia bugetara selectata nu este calsificatie finala }
  if not Sender.IsNull then begin
    lPrevValue  := Trim(Sender.AsString);
    if (lPrevValue <> '') and (lPrevValue <> '?') then begin
      lNode := cxNodeByText(FSelectieCE.cxTreeEconomic, FSelectieCE.cxTreeEconomiccod_ecran, lPrevValue);
      if Assigned(lNode) then begin
        if not lNode.HasChildren then begin
          { Testul cu frati nu are sens deoarece se face cautare exacta }
          QryAngajamenteDefalcate['CLASA_ECONOMICA']  := lNode.Values[FSelectieCE.cxTreeEconomicCLASA.ItemIndex];
          QryAngajamenteDefalcate['id_oi_proiecte']   := lNode.Values[FSelectieCE.cxTreeEconomicid_oi_proiecte.ItemIndex];
          QryAngajamenteDefalcate['APROBATE']         := lNode.Values[FSelectieCE.cxTreeEconomicprevederi.ItemIndex];
          QryAngajamenteDefalcate['TOTAL_ANGAJATE']   := lNode.Values[FSelectieCE.cxTreeEconomicanterior.ItemIndex];
          QryAngajamenteDefalcate.FieldByName('DISPONIBIL').AsCurrency := QryAngajamenteDefalcate.FieldByName('APROBATE').AsCurrency -
                                                                          QryAngajamenteDefalcate.FieldByName('TOTAL_ANGAJATE').AsCurrency;
          Exit;
        end
        else begin
          lNode.MakeVisible;
          lNode.Focused := True;
        end;
      end;
    end;
  end;
  Sender.OnValidate := nil;
  try
    Sender.Clear;
  finally
    Sender.OnValidate := ValidareContBuget;
  end;
  if viewAngajamentDetaliu.Controller.EditingController.IsEditing then
    if viewAngajamentDetaliu.Controller.EditingController.Edit is TcxCustomDropDownEdit then
      TcxCustomDropDownEdit(viewAngajamentDetaliu.Controller.EditingController).DroppedDown := True;
end;

procedure TfrmAlopAngajamente.ValidateValuta(Sender: TField);
begin
  with QryAngajamenteDefalcate do
    if Sender.Tag = 0 then
       FieldByName('ANGAJAT').AsCurrency := RoundTo( Sender.AsCurrency * FieldByName('ANGAJAT_VALUTA').AsCurrency, -2)
    else FieldByName('ANGAJAT').AsCurrency := RoundTo( Sender.AsCurrency * FieldByName('CURS_VALUTAR').AsCurrency, -2);
end;

procedure TfrmAlopAngajamente.ValidateAngajat(Sender: TField);
begin
  with QryAngajamenteDefalcate do begin
    if (IsInLoading) or (FieldByName('DISPONIBIL').AsCurrency - Sender.AsCurrency >= 0) then
      FieldByName('RAMAS_DE_ANGAJAT').AsCurrency := FieldByName('DISPONIBIL').AsCurrency - Sender.AsCurrency
    else begin
      case ValueSafeToInt( DBGetScallar('exec [spAlopAngVerificaDisponibil] :ID_ALOP_ANGAJAMENTE_DEFALCARE, :ANGAJAT, :DISPONIBIL', QryAngajamenteDefalcate)) of
        0 : FieldByName('RAMAS_DE_ANGAJAT').AsCurrency := FieldByName('DISPONIBIL').AsCurrency - Sender.AsCurrency;
        1 : begin
            if MessageDlg('Pe clasificatia bugetara : '+FieldByNAme('COD_ECONOMIC').AsString+' nu mai aveti disponibila suma de angajat !'#13#10+
                       'Doriti totusi angajarea sumei : '+Sender.AsString+' ?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then
                FieldByName('RAMAS_DE_ANGAJAT').AsCurrency := FieldByName('DISPONIBIL').AsCurrency - Sender.AsCurrency
            else Abort;
           end;
        2 : begin
              MessageDlg('Pe clasificatia bugetara : '+FieldByNAme('COD_ECONOMIC').AsString+' nu mai aveti disponibila suma de angajat !'#13#10+
                       'Va rugam corectati angajarea sumei : '+Sender.AsString+' ?', mtError, [mbOk], 0);
              Abort;
            end;
      end;
    end;
  end;
  if ValueIsTrue(QryAngajamenteDefalcate['este_procentual']) and ValueHasValue(QryAngajamenteDefalcate['angProiect']) and (ValueSafeToInt(QryAngajamenteDefalcate['procProiect']) = 0) then begin
    DBExecSQLFmt('exec [spAlopSetSumaProcentProiectAngajament] %d, %d, %s, %s, %s',
            [
              IdUtilizator,
              CurentAngajament,
              ValueToStr(QryAngajamenteDefalcate['id_oi_proiecte']),
              ValueToStr(QryAngajamenteDefalcate['angProiect']),
              ValueToStr(Sender.AsCurrency)
            ]);
    PostMessage(Self.Handle, WM_REFRESH_DEFALCARE, 0, 0);
  end;
end;

procedure TfrmAlopAngajamente.ReadAngajament(const IdAngajament : Integer = 0);
var
  lDataSet: TDataSet;
begin

  lDataSet := DBNewQueryFmt('exec spAlopAngajamenteNevalidate %d, %d', [IdUtilizator, IdAngajament]);
  try
    lDataSet.Open;
    IsInLoading := True;
    try
      if not lDataSet.IsEmpty then begin
        CurentAngajament  := lDataSet.FieldByName('ID').AsInteger;
        edPredator.Text   := lDataSet.FieldByName('PREDATOR').AsString;
        edPrimitor.Text   := lDataSet.FieldByName('PRIMITOR').AsString;
        edRectificat.Tag  := lDataSet.FieldByName('ID_PARINTE').AsInteger;
        edRectificat.Text := lDataSet.FieldByName('AngParinte').AsString;
        edLegal.Tag       := lDataSet.FieldByName('id_baza').AsInteger;
        edLegal.Text      := lDataSet.FieldByName('AngBaza').AsString;
      end
      else CurentAngajament := -1;
    finally
     IsInLoading := False;
    end;
  finally
    lDataSet.Free;
  end;
end;

procedure TfrmAlopAngajamente.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  FSelectieContract.SaveContext;
  Action := caFree;
end;

procedure TfrmAlopAngajamente.BtnOkClick(Sender: TObject);
begin
  DoCheckPostDataSet(QryAngajamenteDefalcate);
  //daca dam editare putem sa editam angajamentul curent
  if (BtnOk.Tag = -1) then begin
    if OkToModify(FCurentAngajament) then SetAngFieldValue('VALIDAT', 0);
  end
  //daca nu inseamna salvare
  else begin
    if not ValidareAngajamentEcran(False) then begin
      MessageDlg(FErrRecord, mtError, [mbOk],0);
      SetNextControl;
      Abort;
    end
    else begin
      if TestUniqueNumber then begin
        try
          DBExecSQLFmt('exec [spAlopValidareAngajament] %d, 0', [FCurentAngajament]);
          if Assigned(FExecOnValidation) then FExecOnValidation(Self)
          else LocalModificValidation(Self)
        finally
          QryAngajamente.Refresh;
        end;
      end;
    end;
  end;
  SetDetaliiValidare(QryAngajamente.FieldByName('VALIDAT').AsInteger = 1);
end;

procedure TfrmAlopAngajamente.pnDocumentResize(Sender: TObject);
begin
  edPredator.Width := edNumarDoc.Left - edPredator.Left - 10;
  edPrimitor.Left  := edNrProiect.Left + edNrProiect.Width + 10;
  LbPrimitor.Left  := edPrimitor.Left;
  edPrimitor.Width := pnDocument.Width - edPrimitor.Left - 10;
end;

procedure TfrmAlopAngajamente.edPredatorEnter(Sender: TObject);
begin
  if csDestroying in ComponentState then Exit;
  with TcxPopupEdit(Sender) do DroppedDown := True;
end;

procedure TfrmAlopAngajamente.edPredatorKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if (Shift = []) and ((Key > 32) or (Key in [8,27, 13])) then begin
     with TcxPopupEdit(Sender) do DroppedDown := True;
     Key := 0;
  end;
end;

procedure TfrmAlopAngajamente.SetNextControl;
  procedure SetActiveControl(AControl: TWinControl);
   var
     I : Integer;
   begin
     if AControl = edtScop then begin
       if Trim(edtScop.Text) = '' then begin
           if (edTipAngajament.EditValue = null) or (edTipAngajament.EditValue=-1) then
              I := -1;
            if I = -1 then
              edtScop.Text := 'Angajament nr : ' + edNumarDoc.Text + ' din data ' + edDataDoc.Text
            else
              edtScop.Text := edTipAngajament.Text +  ' nr : ' + edNumarDoc.Text + ' din data ' + edDataDoc.Text;
       end;
     end;
     if Self.Visible and AControl.Visible and AControl.Enabled then AControl.SetFocus
     else if AControl.Visible and AControl.Enabled  then Self.ActiveControl := AControl;
   end;
begin

  if IsInLoading then Exit;
  if not Self.Visible or not Self.Active then Exit; 

  if edPredator.Tag > 0 then
     if Trim(edNumarDoc.Text) > '' then
        if IsValidDate(edDataDoc.EditValue) then
           if edTipAngajament.ItemIndex > -1 then
              if edPrimitor.Tag > 0 then
                 if Trim(edtScop.Text) > '' then
                   if FInfoFunctional^.Id <> Null then SetActiveControl(gridAngajamentDetaliu)
                                                     else SetActiveControl(cxFunctionalBar)
                 else SetActiveControl(edtScop)
              else SetActiveControl(edPrimitor)
           else SetActiveControl(edTipAngajament)
        else SetActiveControl(edDataDoc)
     else SetActiveControl(edNumarDoc)
  else SetActiveControl(edPredator)
  
end;

procedure TfrmAlopAngajamente.edDataDocValidate(Sender: TObject;
  var ErrorText: String; var Accept: Boolean);
begin
  edDataDocChange(edDataDoc);
  SetNextControl;
end;

procedure TfrmAlopAngajamente.aKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if (not IsInLoading) and (Key = VK_RETURN) then SetNextControl;
end;

function TfrmAlopAngajamente.NewAngajament: Integer;
begin

  Result := DBGetScallarFmt('exec spAlopNewAngajament %d, %s, null', [IdUtilizator, ValueToStr(edTipAngajament.EditValue)]);

  QryAngajamente.Close;
  QryAngajamente.Params[0].Value := Result;
  QryAngajamente.Open;
  edPredator.Text := '';
  edPrimitor.Text := '';
  edNumarDoc.Text := '';
  edRectificat.Tag := 0;
  edRectificat.Text := '';
  edDataDoc.Clear;

  FInfoFunctional^.Id := null;
  FInfoFunctional^.CodFunctional := '';
  FInfoFunctional^.CodEcran := '';
  FInfoFunctional^.Denumire := '';

  QryAngajamenteDefalcate.Close;
  QryAngajamenteDefalcate.Params.ParamByName('ID').Value := -1;
  QryAngajamenteDefalcate.Open;
end;

procedure TfrmAlopAngajamente.FormCreate(Sender: TObject);
var
  aQry : TZQuery;
begin

  ZeosDBUtile.OpenDataSets(Self);
  AddcxPopupComponent(Self);
  
  PopulateReportContext('Rapoarte Angajamente', btnRapoarte, ReportClick);

  FSelectieCE := TfmSelectieCE.Create(Self);
  FSelectieCE.DataSet := QryAngajamenteDefalcate;
  FSelectieCF := TfmSelectieCF.Create(Self);
  FSelectieCF.DataSet := QryAngajamente;
  FSelectieDep  := TfmSelectieRepartitor.Create(Self);
  FSelectieDep.SetFiltru(1, '');
  FSelectieBen  := TfmSelectieRepartitor.Create(Self);
  FSelectieBen.SetFiltru(2, '');

  TcxPopupEditProperties(viewAngajamentDetaliuBUGET.Properties).PopupControl := FSelectieCE;
  cxFunctionalBar.Properties.PopupControl := FSelectieCF;
  edPrimitor.Properties.PopupControl      := FSelectieBen;
  edPredator.Properties.PopupControl      := FSelectieDep;

  //SetCheckedView(viewAngajamentDetaliu);

  New(FInfoFunctional);
  FillMemory(FInfoFunctional, SizeOf(TInfoFunctional), 0);
  FInfoFunctional^.Id := null;
  FInfoFunctional^.CodFunctional := '';
  FInfoFunctional^.CodEcran := '';
  FInfoFunctional^.Denumire := '';
  cxFunctionalBar.Tag := Integer(FInfoFunctional);

  FModificare := False;
  //FExecOnValidation := LocalModificValidation;
  FExecOnValidation := nil;
  FSelectieContract := TfrmSelectieContract.Create(Self);
  FSelectieContract.RestoreContext;
  FSelectieContract.edNrContract.Properties.OnChange :=  edNrContractPropertiesChange;
  FSelectieContract.edDataContract.Properties.OnEditValueChanged := edDataContractPropertiesEditValueChanged;
  FSelectieContract.edDataContract.Properties.OnValidate := edDataContractPropertiesValidate;
  edtDetaliiContract.Properties.PopupControl := FSelectieContract;

  FSelectieDosar  := TfrmSelectieDosar.Create(Self);
  edtDetaliiDosar.Properties.PopupControl := FSelectieDosar;

  PopulateTipAngajament;
  FillImageCombo(viewAngajamentDetaliuID_VALUTA.Properties, 'spNmclValute', 0, 1);
  FillImageCombo(viewAngajamentDetaliuid_oi_proiecte.Properties, 'exec [sp_oi_proiecte]', 'id_oi_proiecte', 'denumire', Null, 'Fara Proiect');

  edDataDoc.Date := Date;
  //SetExceptionMask([]);
  edDataDocChange(edDataDoc);
  HideControlsOnSQL;

end;

procedure TfrmAlopAngajamente.SetAngFieldValue(FieldName: String;
  Value: Variant);
var
  lPrevEdit: Boolean;
begin
  if CurentAngajament <= 0 then
    ClearAngajamente
  else
  if not IsInLoading and QryAngajamente.Active then begin
    lPrevEdit := DBGoEdit(QryAngajamente);
    QryAngajamente[FieldName] := Value;
    DBPost(QryAngajamente);
    if lPrevEdit then QryAngajamente.Edit;
  end;
end;

procedure TfrmAlopAngajamente.edDataDocChange(Sender: TObject);
begin
  if IsValidDate(edDataDoc.EditValue) then begin
     SetAngFieldValue('DATA_EMITERE', edDataDoc.Date);
     FSelectieCE.SetParameter('dataAng', edDataDoc.Date);
  end;
end;

procedure TfrmAlopAngajamente.edTipAngajamentChange(Sender: TObject);
begin
  SetAngFieldValue('TIP_ANGAJAMENT', edTipAngajament.EditValue);
end;

procedure TfrmAlopAngajamente.edScopulKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if Key = VK_RETURN then SetNextControl;
end;

procedure TfrmAlopAngajamente.ValidateCursSchimb(Sender: TField);
begin
  { Validam Cursul de schimb }
  with DBNewQueryFmt('exec [spAlopAngCursSchimb] %s, %s', [ValueToStr(edDataDoc.Date), Sender.AsString]) do
   try
      Open;
      if IsEmpty then
         QryAngajamenteDefalcate.FieldByName('CURS_VALUTAR').AsCurrency := 1
      else if (Fields[1].AsInteger < 7) or
              (MessageDlg('Cursul de schimb pentru valuta selectata este mai vechi de 7 zile'#13#10+
                          'Ultima data la care a fost actualizat este : '+FormatDateTime('dd.mm.yyyy', Fields[2].AsDateTime)+#13#10+
                          'Doriti folosirea acestui curs ?', mtConfirmation, [mbYes, mbNo], 0) = mrYes) then
              QryAngajamenteDefalcate.FieldByName('CURS_VALUTAR').AsCurrency := Fields[0].AsCurrency;

   finally
      Free;
   end;
end;

procedure TfrmAlopAngajamente.QryAngajamenteDefalcateBeforeDelete(
  DataSet: TDataSet);
begin
  if not FIsInDelete and
     (MessageDlg('Doriti stergerea pozitiei din angajament : '+DataSet.FieldByName('DESCRIERE').AsString+'?', mtConfirmation, [mbYes, mbNo], 0) <> mrYes) then Abort;
  FSelectieCE.SetParameter('', Null);
end;

procedure TfrmAlopAngajamente.QryAngajamenteNewRecord(DataSet: TDataSet);
begin
  if CurentAngajament <= 0 then Exit;
  raise EContaHandledError.Create('Eroare interna -> inchideti ecranul si mai accesati odata meniul !');
end;

procedure TfrmAlopAngajamente.edTipAngajamentValidate(Sender: TObject;
  var ErrorText: String; var Accept: Boolean);
begin
  SetNextControl;
end;

procedure TfrmAlopAngajamente.QryAngajamenteDefalcateAfterPost(
  DataSet: TDataSet);
begin
//  FSelectieCE.SetParameter('', Null);
end;

procedure TfrmAlopAngajamente.BtnModificareClick(Sender: TObject);
var
  idAng : Integer;
begin
  TestGolireEcran;
  if not IsAngValidat then DBExecSQLFmt('exec [spAlopAnuleazaAngajament] %d', [CurentAngajament]);
  idAng := SelectieAngajament(0);
  if idAng <> -1 then LoadAngajament(idAng);
end;


procedure TfrmAlopAngajamente.LoadAngajament(IdAngajament: Integer);
begin
  DBExecSqlFmt('exec [spAlopLoadAngajament] %d, %d', [IdUtilizator, IdAngajament]);
  ReadAngajament(IdAngajament);
  RecalcItemsi;
end;

procedure TfrmAlopAngajamente.btnAnuleazaAngClick(Sender: TObject);
var
  lNrAng : String;
  lDataAng : String;
begin
  if (CurentAngajament > 0) and QryAngajamente.Active and not QryAngajamente.IsEmpty then begin
    //TestGolireEcran;
    lNrAng := QryAngajamente.FieldByName('NUMAR').AsString;
    lDataAng := QryAngajamente.FieldByName('DATA_EMITERE').AsString;
    if (MessageDlg(Format('Doriti stergerea angajamentului nr. : %s din data  %s ?', [lNrAng, lDataAng]),
         mtConfirmation, [mbYes, mbNo], 0) in [mrNo, mrNone]) then
       Abort;
    DBExecSQLFmt('exec [spAlopAnuleazaAngajament] %d', [CurentAngajament]);
    if Assigned(FExecOnValidation) then FExecOnValidation(Self)
    else LocalModificValidation(Self);
    ClearAngajamente;
  end;
end;

procedure TfrmAlopAngajamente.ActivateGrid;
begin
  gridAngajamentDetaliu.Enabled := not IsAngValidat  and (CurentAngajament > 0) and (FInfoFunctional^.Id <> null);
  if gridAngajamentDetaliu.Enabled then
    viewAngajamentDetaliu.Styles.Background.Color := clWindow
  else
    viewAngajamentDetaliu.Styles.Background.Color := clBtnFace;
end;


procedure TfrmAlopAngajamente.edtDetaliiContractKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if Key = VK_DELETE then begin
    SetAngFieldValue('ID_CONTRACT', Null);
    FSelectieCF.SetParameter('idContract', Null);
  end;
end;

procedure TfrmAlopAngajamente.SetContract(IdContract: Integer);
begin
  FSelectieContract.RefreshContracte;
  edtDetaliiContract.Tag := IdContract;
  edtDetaliiContract.Text := FSelectieContract.GetContractDetails(IdContract);
  FSelectieCF.SetParameter('idContract', IdContract);
end;

procedure TfrmAlopAngajamente.FormDestroy(Sender: TObject);
begin
  Dispose(FInfoFunctional);
  FSelectieContract.Free;
  ExitSingleUser;
end;

procedure TfrmAlopAngajamente.ClearAngajamente;
begin
  ReadAngajament;
end;

function TfrmAlopAngajamente.ValidareAngajamentEcran (const NeedFilled : Boolean = True): Boolean;
begin
  FErrRecord := '';
  DBPost(QryAngajamente);
  DBPost(QryAngajamenteDefalcate);

  with DBNewQueryFmt('exec [spAlopAngVerificaEcran] %d', [CurentAngajament]) do
  try
    Open;
    if not IsEmpty then
      while not Eof do begin
        FErrRecord := FErrRecord + FieldByName('Explicatie').AsString + #13#10;
        Next;
      end;
  finally
    Free;
  end;
  Result := (FErrRecord = '');
end;

procedure TfrmAlopAngajamente.btnCancelClick(Sender: TObject);
begin
  Close;
end;

procedure TfrmAlopAngajamente.PopulateTipAngajament;
var
  lDataSet  : TDataSet;
  lItem     : TcxImageComboBoxItem;
begin
  edTipAngajament.Properties.Items.Clear;
  edTipAngajament.Properties.Items.BeginUpdate;
  try
    lDataSet := DBNewQuery('exec [spAlopLstTipuriAngajamente]');
    try
      lDataSet.Open;
      while not lDataSet.Eof do begin
         lItem := edTipAngajament.Properties.Items.Add as TcxImageComboBoxItem;
         lItem.Value        := lDataSet['ID'];
         lItem.Description  := lDataSet['DENUMIRE'];
         lItem.Tag          := lDataSet['TAG'];
         lDataSet.Next;
      end;
    finally
      lDataSet.Free;
    end;
  finally
    edTipAngajament.Properties.Items.EndUpdate;
  end;
end;



procedure TfrmAlopAngajamente.edNumarDocKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if (not IsInLoading) and (Key = VK_RETURN) then SetNextControl;
end;

procedure TfrmAlopAngajamente.edDataDocPropertiesValidate(Sender: TObject;
  var DisplayValue: Variant; var ErrorText: TCaption; var Error: Boolean);
begin
  if Error then begin
     ErrorText := '';
     raise EContaHandledError.Create('Data introdusa este invalida ! ');
  end;
  SetNextControl;
end;

procedure TfrmAlopAngajamente.edTipAngajamentPropertiesChange(
  Sender: TObject);
var
  lTag    : Integer;
  lTipBen : Integer;
begin
  SetAngFieldValue('TIP_ANGAJAMENT', edTipAngajament.EditingValue);
  ClearAngInBaza;
  lTag := edTipAngajament.Properties.Items[edTipAngajament.ItemIndex].Tag;
  FSelectieDep.SetFiltru(lTag mod 10, '');
  lTag    := lTag div 10;
  FSelectieBen.SetFiltru(lTag mod 10, '');
  lTag    := lTag div 10;
  SetLegalVisibility((lTag mod 10) = 0);
  lTag    := lTag div 10;
  SetAngConex(lTag);
  if FSelectieCE.qryCEDeAngajat.Active then begin
     FSelectieCE.qryCEDeAngajat.Refresh;
     RecalcItemsi;
  end;
end;

procedure TfrmAlopAngajamente.edTipAngajamentPropertiesValidate(
  Sender: TObject; var DisplayValue: Variant; var ErrorText: TCaption;
  var Error: Boolean);
begin
  SetNextControl;
end;

function TfrmAlopAngajamente.cxNodeByText(aTree: TcxDBTreeList;
  aColumn: TcxTreeListColumn; AText: String): TcxTreeListNode;
begin
  Result := aTree.FindNodeByText(AText, aColumn);
end;



procedure TfrmAlopAngajamente.edPredatorPropertiesInitPopup(
  Sender: TObject);
const
  CTipGest: array[Boolean] of String = ('False', 'True');
var
  lPopupEdit : TcxPopupEdit;
begin

  lPopupEdit := TcxPopupEdit(Sender);
  if lPopupEdit.Properties.PopupWidth < lPopupEdit.Width then
    lPopupEdit.Properties.PopupWidth := lPopupEdit.Width;
end;

type
  TAccesscxPopupEdit = class(TcxPopupEdit);

procedure TfrmAlopAngajamente.edPredatorPropertiesCloseUp(Sender: TObject);
var
  lFieldName: String;
  lSelForm  : TfmSelectieRepartitor;
  lNode : TcxDBTreeListNode;
begin
  if TAccesscxPopupEdit(Sender).PopupWindow.ModalResult = mrOk then begin
    if Sender = edPredator then begin
      lSelForm := FSelectieDep;
      lFieldName := 'ID_DEPARTAMENT';
    end
    else begin
      lSelForm := FSelectieBen;
      lFieldName := 'ID_LST_REPARTITORI';
    end;
    lNode := TcxDBTreeListNode(lSelForm.cxTreeRepartitori.FocusedNode);
    if Assigned(lNode) then begin
      SetAngFieldValue(lFieldName, lNode.KeyValue);
      TcxPopupEdit(Sender).Tag  := lNode.KeyValue;
      TcxPopupEdit(Sender).Text := lNode.Texts[lSelForm.cxTreeRepartitoriNUME.ItemIndex];
    end;
  end;
  ActivateGrid;
  SetNextControl;
end;

procedure TfrmAlopAngajamente.edNumarDocPropertiesButtonClick(
  Sender: TObject; AButtonIndex: Integer);
begin
  edNumarDoc.EditValue := DBGetScallarFmt('exec [spAlopNumarAngajament] %d', [CurentAngajament]);
end;

procedure TfrmAlopAngajamente.edtDetaliiContractPropertiesInitPopup(
  Sender: TObject);
var
  lEdit: TcxPopupEdit;
begin
  lEdit := TcxPopupEdit(Sender);
  if lEdit.Properties.PopupWidth < lEdit.Width then lEdit.Properties.PopupWidth := lEdit.Width;
end;

procedure TfrmAlopAngajamente.viewAngajamentDetaliuDESC_BUGETGetDisplayText(
  Sender: TcxCustomGridTableItem; ARecord: TcxCustomGridRecord;
  var AText: String);
var
  lNode: TcxTreeListNode;
begin
  lNode := cxNodeByText(FSelectieCE.cxTreeEconomic, FSelectieCE.cxTreeEconomiccod_ecran, AText);
  if Assigned(lNode) then AText := lNode.Texts[FSelectieCE.cxTreeEconomicDESCRIERE.ItemIndex];
end;


procedure TfrmAlopAngajamente.LocalModificValidation(Sender: TObject);
begin
  //refacem ce era ininte de modificare pentru utilizatorul curent
//  if FModificare then
  DBExecSQLFmt('exec [spAlopRevalideazaCulegere] %d', [IdUtilizator]);
end;

procedure TfrmAlopAngajamente.SetDetaliiValidare(IsValidat: Boolean);
begin
  if IsValidat then begin
    btnOkGenerare.Visible := True;
    { Daca are angajament conex butonul genereaza angajament conex }
    if btnOkGenerare.Tag <> 0 then begin
      btnOkGenerare.Caption := 'Generare';
      btnOkGenerare.OnClick := btnOkGenerareAngajament;
    end
    else begin
      btnOkGenerare.Caption := 'Factura';
      btnOkGenerare.OnClick := btnOkGenerareFactura;
    end;
    btnOk.Caption := 'Editare';
    btnOk.Tag := -1;
  end
  else begin
    btnOkGenerare.Visible := False;
    btnOk.Caption := 'Salvare';
    btnOk.Tag := 0;
  end;
  btnRapoarte.Enabled := IsValidat;


  edPredator.Enabled := not IsValidat;
  edNumarDoc.Enabled := not IsValidat;
  edDataDoc.Enabled := not IsValidat;
  edTipAngajament.Enabled := not IsValidat;
  edPrimitor.Enabled := not IsValidat;
  edtDetaliiContract.Enabled := not IsValidat;
  FSelectieContract.edNrContract.Enabled := not IsValidat;
  FSelectieContract.edDataContract.Enabled := not IsValidat;
  edtScop.Enabled := not IsValidat;
  cxFunctionalBar.Enabled := not IsValidat;
  ActivateGrid;
end;

function TfrmAlopAngajamente.IsAngValidat: Boolean;
begin
  Result := False;
  if not QryAngajamente.IsEmpty then
    Result := (QryAngajamente.FieldByName('VALIDAT').AsInteger = 1); 
end;

procedure TfrmAlopAngajamente.btnNewAngClick(Sender: TObject);
begin
  TestGolireEcran;
  if not IsAngValidat then DBExecSQLFmt('exec [spAlopGolesteAngajament] %d', [CurentAngajament]);
  ClearAngajamente;
end;

procedure TfrmAlopAngajamente.edtDetaliiContractPropertiesPopup(
  Sender: TObject);
begin
  FSelectieContract.FilterContractByDepartament(edPredator.Tag);
  FSelectieContract.FilterContractByPrestator(edPrimitor.Tag, False);
  FSelectieContract.IdContract := QryAngajamente.FieldByName('ID_CONTRACT').AsInteger;
end;

procedure TfrmAlopAngajamente.edtDetaliiDosarKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if Key = VK_DELETE then begin
    SetAngFieldValue('refDosar', Null);
    SetAngFieldValue('nr_dosar', Null);
    SetAngFieldValue('data_dosar', Null);
  end;
end;

procedure TfrmAlopAngajamente.edtDetaliiDosarPropertiesCloseUp(Sender: TObject);
begin
  if TAccesscxPopupEdit(Sender).PopupWindow.ModalResult = mrOk then begin
    SetAngFieldValue('refDosar', FSelectieDosar.GetIdDosar);
    SetAngFieldValue('nr_dosar', FSelectieDosar.GetNumarDosar);
    SetAngFieldValue('data_dosar', FSelectieDosar.GetDataDosar);
    edtDetaliiDosar.EditValue := FSelectieDosar.GetDosarDetails();
  end;
end;

procedure TfrmAlopAngajamente.edtDetaliiDosarPropertiesPopup(Sender: TObject);
begin
  FSelectieDosar.SetIdDosar(QryAngajamente['refDosar']);
end;

procedure TfrmAlopAngajamente.TestGolireEcran;
begin
  if not IsAngValidat and not QryAngajamenteDefalcate.IsEmpty then
    if (MessageDlg('Modificarea unui document din arhiva duce la pierderea angajamentului din ecran ! '+#13+#10+'Doriti continuarea ?', mtConfirmation, [mbYes, mbNo], 0) = mrNo) then
      Abort;
end;

procedure TfrmAlopAngajamente.FormShow(Sender: TObject);
begin
  SetNextControl;
end;

procedure TfrmAlopAngajamente.edPredatorPropertiesPopup(Sender: TObject);
var
  lSelForm: TfmSelectieRepartitor;
  lNode   : TcxDBTreeListNode;
begin
  if Sender = edPredator then
    lSelForm := FSelectieDep
  else
    lSelForm := FSelectieBen;
  if Sender is TcxPopupEdit then begin
    lNode := lSelForm.cxTreeRepartitori.FindNodeByKeyValue(TcxPopupEdit(Sender).Tag, nil);
    if Assigned(lNode) then begin
      lNode.Focused := True;
      lNode.MakeVisible;
    end;
  end;
end;



procedure TfrmAlopAngajamente.FormActivate(Sender: TObject);
begin
  SetNextControl;
end;

procedure TfrmAlopAngajamente.RecalcItemsi;
begin
  if not IsInLoading and QryAngajamenteDefalcate.Active and not QryAngajamenteDefalcate.IsEmpty then begin
    IsInLoading := True;
    try
      DBExecSQLFmt('exec [spRecalculAlopAngajamenteItemsi] %d, %d, %d', [IdLogin, IdUtilizator, CurentAngajament]);
    finally
      IsInLoading := False;
      QryAngajamenteDefalcate.Refresh;
    end;
  end;
end;

procedure TfrmAlopAngajamente.edDataDocPropertiesEditValueChanged(
  Sender: TObject);
begin
  if IsValidDate(edDataDoc.EditValue) then begin
     SetAngFieldValue('DATA_EMITERE', edDataDoc.Date);
     FSelectieCE.SetParameter('dataAng', edDataDoc.Date);
     FSelectieCF.SetParameter('dataAng', edDataDoc.Date);
  end;
end;

procedure TfrmAlopAngajamente.pnBottomResize(Sender: TObject);
begin
  BtnCancel.Left := pnBottom.Width - BtnCancel.Width - 5;
  btnRapoarte.Left := BtnCancel.Left - btnRapoarte.Width - 2;
  BtnOk.Left := btnRapoarte.Left - BtnOk.Width - 6;
end;

procedure TfrmAlopAngajamente.edRectificatPropertiesButtonClick(
  Sender: TObject; AButtonIndex: Integer);
var
  lIdAngajament : Integer;
begin
  case AButtonIndex of
    0:
      begin
        lIdAngajament := SelectieAngajament(2 + 2*(edTipAngajament.EditValue-1));
        if lIdAngajament = -1 then begin
          chkRectificareSold.Visible := False;
          ClearAngParinte;
        end
        else begin
          DBExecSQLFmt('exec spAlopAngSetParinte %d, %d', [CurentAngajament, lIdAngajament]);
          ReadAngajament(CurentAngajament);
          RecalcItemsi;
          chkRectificareSold.Checked := False;
          chkRectificareSold.Visible := DBProcExists('spAlopAngSuportaRectificareSold') and
                                        ValueIsTrue(DBGetScallarFmt('exec [spAlopAngSuportaRectificareSold] %d', [CurentAngajament]));
        end;
      end;
    1:
      begin
        chkRectificareSold.Visible := False;
        ClearAngParinte;
      end;
  end;
end;

procedure TfrmAlopAngajamente.edNrContractPropertiesChange(
  Sender: TObject);
begin
  SetAngFieldValue('NR_CONTRACT', FSelectieContract.edNrContract.EditValue);
end;

procedure TfrmAlopAngajamente.edDataContractPropertiesEditValueChanged(
  Sender: TObject);
begin
  SetAngFieldValue('DATA_CONTRACT', FSelectieContract.edDataContract.EditValue);
end;

procedure TfrmAlopAngajamente.edDataContractPropertiesValidate(
  Sender: TObject; var DisplayValue: Variant; var ErrorText: TCaption;
  var Error: Boolean);
begin
  edDataContractPropertiesEditValueChanged (FSelectieContract.edDataContract);
  if Error then begin
     ErrorText := '';
     raise EContaHandledError.Create('Data introdusa este invalida ! ');
  end;
  SetNextControl;
end;

function TfrmAlopAngajamente.TestUniqueNumber : Boolean; 
var
  lNumar, lExplicatie : String;
  lTip : string;
  lQry : TZReadOnlyQuery;
begin
  //testam daca numarul alocat este unic si intrebam daca vrea sa-l pastreze sau sa genereze unul nou
  Result := True;
  lQry := DBNewQueryFmt('exec [spAlopAngTestNumber] %d', [CurentAngajament]);
  with lQry do
    try
      Open;
      lNumar := Fields[0].AsString;
      lExplicatie := Fields[1].AsString;
      lTip := Fields[2].AsString;
      if not IsEmpty then
        case MessageDlg(Format('Numarul %s este deja folosit in %s. ', [lNumar, lExplicatie])+#13+#10+'Doriti generarea unui numar nou ?'+#13+#10+ '(Daca apasati Yes se va genera un nou numar, No va lasa numarul curent, Cancel revine in ecranul de culegere fara salvare.)', mtConfirmation, [mbYes, mbNo, mbCancel], 0) of

             mrYes : if lTip = 'proiect' then edNrProiectPropertiesButtonClick(edNrProiect, 0) else edNumarDocPropertiesButtonClick(edNumarDoc, 0) ;
             mrNo :  ;
             mrCancel, mrNone: Result := False;
         end;
    finally
      Free;
    end;
end;

procedure TfrmAlopAngajamente.FillInfoFunctional;
var
  lNode : TcxDBTreeListNode;
begin
   if FInfoFunctional^.CodEcran = '' then
     lNode := nil
   else
     lNode := TcxDBTreeListNode(FSelectieCF.cxTreeBugete.FindNodeByText(FInfoFunctional^.CodEcran, FSelectieCF.cxTreeBugeteCOD_ECRAN));
   if (lNode <> nil) then begin
     FInfoFunctional^.Id            := lNode.KeyValue;
     FInfoFunctional^.CodFunctional := lNode.Texts[FSelectieCF.cxTreeBugeteCOD_FUNCTIONAL.ItemIndex];
     FInfoFunctional^.Denumire      := lNode.Texts[FSelectieCF.cxTreeBugeteDESCRIERE.ItemIndex];
     FInfoFunctional^.IdUnitate     := lNode.Values[FSelectieCF.cxTreeBugeteID_ANALITIC.ItemIndex];
   end else begin
     FInfoFunctional^.Id := null;
     FInfoFunctional^.CodFunctional := '';
     FInfoFunctional^.Denumire := '';
     FInfoFunctional^.IdUnitate := null;
   end;
   cxFunctionalBar.Text := FInfoFunctional^.Denumire;
end;

procedure TfrmAlopAngajamente.cxFunctionalBarPropertiesCloseUp(
  Sender: TObject);
var
  lCodFunctional    : Variant;
  lIdUnitate        : Variant;
  lIsInsert         : Boolean;
  lCodEconomic      : Variant;
  lCodEconomicEcran : Variant;
  lIdProiect        : Variant;
  lNode             : TcxDBTreeListNode;
begin
  lIsInsert := False;
  with TAccesscxPopupEdit(Sender), FSelectieCF do
  if PopupWindow.ModalResult = mrOk then begin
    if PageFunctional.ActivePage = tabFunctionalContract then begin
      lNode := TcxDBTreeListNode(TreeDetaliiContract.FocusedNode);
      lCodFunctional  := lNode.Values[TreeDetaliiContractcod_functional.ItemIndex];
      lIdUnitate      := lNode.Values[TreeDetaliiContractid_oi_unitati.ItemIndex];

      if (QryAngajamenteDefalcate.Active) and (QryAngajamenteDefalcate.RecordCount > 0)
      and ((FInfoFunctional^.CodFunctional <> lCodFunctional) or (FInfoFunctional^.IdUnitate <> lIdUnitate)) then
        lIsInsert := False
      else
        lIsInsert := True;
      FInfoFunctional^.CodEcran := lNode.Texts[TreeDetaliiContractcod_ecran.ItemIndex];      
      lCodEconomic              := lNode.Values[TreeDetaliiContractcod_economic.ItemIndex];
      lCodEconomicEcran         := lNode.Values[TreeDetaliiContractcod_proiect.ItemIndex];
      lIdProiect                := lNode.Values[TreeDetaliiContractid_oi_proiecte.ItemIndex];
    end
    else begin
      lNode     := TcxDBTreeListNode(cxTreeBugete.FocusedNode);
      FInfoFunctional^.CodEcran := lNode.Texts[cxTreeBugeteCOD_ECRAN.ItemIndex];
      lCodFunctional            := lNode.Values[cxTreeBugeteCOD_FUNCTIONAL.ItemIndex];
      lIdUnitate                := lNode.Values[cxTreeBugeteID_ANALITIC.ItemIndex];
    end;
    FillInfoFunctional;
    SetAngFieldValue('COD_FUNCTIONAL', lCodFunctional);
    SetAngFieldValue('ID_ANALITIC', lIdUnitate);
    SetAngFieldValue('COD_ECRAN', FInfoFunctional^.CodEcran);
    IsInLoading := True;
    FSelectieCE.SetParameter('cod_functional', lCodFunctional);
    FSelectieCE.SetParameter('id_oi_unitate', lIdUnitate);
    IsInLoading := False;
    if lIsInsert and (QryAngajamenteDefalcate.Active)  then begin
      if QryAngajamenteDefalcate.Locate('COD_ECRAN', lCodEconomicEcran, []) then
        DBGoEdit(QryAngajamenteDefalcate)
      else
        QryAngajamenteDefalcate.Append;
      QryAngajamenteDefalcate['COD_ECRAN']      := lCodEconomicEcran;
      QryAngajamenteDefalcate['COD_ECONOMIC']   := lCodEconomic;
      QryAngajamenteDefalcate['ID_ANALITIC']    := lIdProiect;
      QryAngajamenteDefalcate['ID_OI_PROIECTE'] := lIdProiect;
      QryAngajamenteDefalcate['ID_OI_UNITATI']  := lIdUnitate;
      DBPost(QryAngajamenteDefalcate);
    end;
    ActivateGrid;
    SetNextControl;
    if ValueHasValue(lCodFunctional) or ValueHasValue(lIdUnitate) or (lIsInsert) then RecalcItemsi;
  end;
end;

procedure TfrmAlopAngajamente.cxFunctionalBarPropertiesPopup(
  Sender: TObject);
var
  lEdit: TcxPopupEdit;
begin
  FSelectieCF.BeforePopup;
  lEdit := TcxPopupEdit(Sender);
  if lEdit.Properties.PopupWidth < lEdit.Width then lEdit.Properties.PopupWidth := lEdit.Width;
end;

procedure TfrmAlopAngajamente.edNrProiectPropertiesButtonClick(
  Sender: TObject; AButtonIndex: Integer);
begin
  edNrProiect.EditValue := DBGetScallarFmt('exec [spAlopNumarProiectAngajament] %d', [CurentAngajament]);
end;

procedure TfrmAlopAngajamente.QryEconomicAfterOpen(DataSet: TDataSet);
begin
  SetAngFieldValue('id_bg_versiune', DataSet.FieldByName('id_bg_versiune').AsInteger);
end;

procedure TfrmAlopAngajamente.ReportClick(Sender: TObject);
begin
  LoadReport(TMenuItem(Sender).Tag, 'ID_ANGAJAMENT', [CurentAngajament]);
end;

procedure TfrmAlopAngajamente.edLegalPropertiesButtonClick(Sender: TObject;
  AButtonIndex: Integer);
var
  lIdAngajament : Integer;
begin
  if AButtonIndex = 1 then
    ClearAngInBaza
  else
  if AButtonIndex = 0 then begin
    lIdAngajament := SelectieAngajament(3+2*(edTipAngajament.EditValue-1));
    if lIdAngajament <> -1 then begin
      DBExecSQLFmt('exec [spAlopAngSetLegal] %d, %d', [CurentAngajament, lIdAngajament]);
      ReadAngajament(CurentAngajament);
      RecalcItemsi;
    end
    else
      ClearAngInBaza;
  end
end;

procedure TfrmAlopAngajamente.SetLegalVisibility(aVisible: Boolean);
begin
  lbLegal.Visible := aVisible;
  edLegal.Visible := aVisible;
end;

procedure TfrmAlopAngajamente.HideControlsOnSQL;
var
   lComponent : TComponent;
begin
  if not DBProcExists('spAlopAngHideControls') then Exit;
  with DBNewQuery('exec [spAlopAngHideControls]') do
  try
    Open;
    while not eof do begin
      lComponent := FindComponentEx(Fields[0].AsString);
      if (lComponent <> nil) and (lComponent is TControl) then
        TControl(lComponent).Visible := False;
      Next;
    end;
  finally
    Free;
  end;
end;

procedure TfrmAlopAngajamente.Cmd_StergeDetaliuAngajamentUpdate(Sender: TObject);
begin
  (Sender as TAction).Enabled := (viewAngajamentDetaliu.Controller.FocusedRecord <> nil);
end;

procedure TfrmAlopAngajamente.Cmd_StergeDetaliuAngajamentExecute(Sender: TObject);
var
  lDeleteAskMessage: String;
begin
  if viewAngajamentDetaliu.Controller.SelectedRowCount > 1 then
    lDeleteAskMessage := Format('Doriti stergea celor %d pozitii selectate ?', [viewAngajamentDetaliu.Controller.SelectedRowCount])
  else
    lDeleteAskMessage := Format('Doriti stergea pozitiei %s din lista de angajamente?', [ValueToStr(viewAngajamentDetaliuDESCRIERE.EditValue)]);
  if MessageDlg(lDeleteAskMessage, mtConfirmation, [mbYes, mbNo], 0) = mrYes then begin
    FIsInDelete := True;
    try
      viewAngajamentDetaliu.Controller.DeleteSelection;
    finally
      FIsInDelete := False;
    end;
  end;
end;

procedure TfrmAlopAngajamente.Cmd_AdaugaDetaliuAngajamentExecute(Sender: TObject);
begin
  FSelectieCE.BorderStyle := bsSizeable;
  FSelectieCE.cxTreeEconomic.FullExpand;
  FSelectieCE.Position    := poMainFormCenter;
  FSelectieCE.OpenDataSets;
  if FSelectieCE.ShowModal = mrOk then
    PreiaSelectieCE;
end;

procedure TfrmAlopAngajamente.cxGridProcentEnter(Sender: TObject);
begin
  DoCheckPostDataSet(QryAngajamenteDefalcate);
end;

procedure TfrmAlopAngajamente.viewAngajamentDetaliuBUGETPropertiesCloseUp(
  Sender: TObject);
begin
  if Sender is TcxPopupEdit then begin
    if TAccesscxPopupEdit(Sender).PopupWindow.ModalResult = mrOk then
      PreiaSelectieCE(True);
  end;
end;

procedure TfrmAlopAngajamente.viewAngajamentDetaliuBUGETPropertiesPopup(
  Sender: TObject);
begin
  if FSelectieCE.qryCEDeAngajat.Active then
    if not FSelectieCE.qryCEDeAngajat.Locate('COD_ECONOMIC;ID_ANALITIC', QryAngajamenteDefalcate['COD_ECONOMIC;ID_ANALITIC'], []) then
       FSelectieCE.qryCEDeAngajat.Locate('COD_ECONOMIC', QryAngajamenteDefalcate['COD_ECONOMIC'], []);
end;

procedure TfrmAlopAngajamente.viewAngajamentDetaliuBUGETPropertiesInitPopup(
  Sender: TObject);
begin
  FSelectieCE.BorderStyle := bsNone;
  FSelectieCE.OpenDataSets;
end;

procedure TfrmAlopAngajamente.PreiaSelectieCE(const IsSelected: Boolean=False);
var
  lNode: TcxTreeListNode;
  lCodEconomic,
  lCodFunctional,
  lUnitateID,
  lEsteProcent,
  lSuma,
  lProjectID: Variant;
  lRecord   : TcxCustomGridRecord;

var
  lIdAng  : Variant;
  lPrevID : Variant;

begin
  { Daca se selecteaza tab-ul de economic }
  if FSelectieCE.pageEconomic.ActivePage = FSelectieCE.tabEconomic then begin
    lNode := FSelectieCE.cxTreeEconomic.FocusedNode;
    if not Assigned(lNode) then
      Exit;
    lProjectID      := lNode.Values[FSelectieCE.cxTreeEconomicid_oi_proiecte.ItemIndex];
    lCodEconomic    := lNode.Values[FSelectieCE.cxTreeEconomiccod_economic.ItemIndex];
    lCodFunctional  := lNode.Values[FSelectieCE.cxTreeEconomiccod_functional.ItemIndex];
    lUnitateID      := lNode.Values[FSelectieCE.cxTreeEconomicid_oi_unitati.ItemIndex];
    lEsteProcent    := lNode.Values[FSelectieCE.cxTreeEconomiceste_procentual.ItemIndex];
  end
  else begin
    lRecord := FSelectieCE.viewProiecte.Controller.FocusedRecord;
    if not Assigned(lRecord) then
      Exit;
    lProjectID      := lRecord.Values[FSelectieCE.viewProiecteid_oi_proiecte.Index];
    lCodEconomic    := lRecord.Values[FSelectieCE.viewProiectecod_economic.Index];
    lCodFunctional  := lRecord.Values[FSelectieCE.viewProiectecod_functional.Index];
    lUnitateID      := lRecord.Values[FSelectieCE.viewProiecteid_oi_unitati.Index];
    lEsteProcent    := lRecord.Values[FSelectieCE.viewProiecteesteProcentual.Index];
  end;
  if IsSelected then
    lIdAng := QryAngajamenteDefalcate['id_alop_angajamente_defalcare']
  else
    lIdAng := Null;

  try
    lPrevID := DBGetScallarFmt('exec [spAdaugaPozitieAngajament] %s, %s, %s, %s, %s, %s, %s, %s',
                            [
                              ValueToStr(IdUtilizator),
                              ValueToStr(CurentAngajament),
                              ValueToStr(lCodEconomic),
                              ValueToStr(lCodFunctional),
                              ValueToStr(lProjectID),
                              ValueToStr(lUnitateID),
                              ValueToStr(lIdAng),
                              ValueToStr(lEsteProcent)
                            ], 0);
    if ValueHasValue(lPrevID) then
      QryAngajamenteDefalcate.Locate('id_alop_angajamente_defalcare', lPrevID, [])
  finally
    QryAngajamenteDefalcate.Refresh;
  end;
end;

procedure TfrmAlopAngajamente.viewAngajamentDetaliuFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
var
  lProjVisible: Boolean;
  lNewValue   : Variant;
begin
  FIdProiect    := Null;
  lNewValue     := Null;
  lProjVisible  := False;
  if Assigned(AFocusedRecord) and AFocusedRecord.IsData then begin
    if ValueIsTrue(AFocusedRecord.Values[viewAngajamentDetaliueste_procentual.Index]) then begin
      FIdProiect    := AFocusedRecord.Values[viewAngajamentDetaliuid_oi_proiecte.Index];
      lNewValue     := AFocusedRecord.Values[viewAngajamentDetaliuangProiect.Index];
      lProjVisible  := True;
    end;
  end;
  if lProjVisible <> lbSumaProiect.Visible then begin
    lbSumaProiect.Visible := lProjVisible;
    edSumaProiect.Visible := lProjVisible;
  end;
  if not ValueSameValue(lNewValue, edSumaProiect.EditValue) then begin
    edSumaProiect.Properties.OnValidate := nil;
    try
      edSumaProiect.EditValue := lNewValue;
    finally
      edSumaProiect.Properties.OnValidate := edSumaProiectPropertiesValidate;
    end;
  end;
  viewAngajamentDetaliu.LayoutChanged();
end;

procedure TfrmAlopAngajamente.viewAngajamentDetaliuStylesGetContentStyle(
  Sender: TcxCustomGridTableView; ARecord: TcxCustomGridRecord;
  AItem: TcxCustomGridTableItem; var AStyle: TcxStyle);
begin
  if ValueSameValue(ARecord.Values[viewAngajamentDetaliuid_oi_proiecte.Index], FIdProiect) then
    AStyle := stilProiectCurent
  else
    AStyle := stilNormal;
end;

procedure TfrmAlopAngajamente.edSumaProiectPropertiesValidate(
  Sender: TObject; var DisplayValue: Variant; var ErrorText: TCaption;
  var Error: Boolean);
begin
  if ValueHasValue(FIdProiect) and edSumaProiect.Visible then begin
    DBExecSQLFmt('exec [spAlopSetSumaProcentProiectAngajament] %d, %d, %s, %s', [IdUtilizator, CurentAngajament, ValueToStr(FIdProiect), ValueToStr(edSumaProiect.EditingValue)]);
    QryAngajamenteDefalcate.Refresh;
  end;
end;

procedure TfrmAlopAngajamente.edSumaProiectKeyUp(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if Key = VK_RETURN then SetNextControl;
end;

procedure TfrmAlopAngajamente.WmRefreshDefalcare(var Message: TMessage);
begin
  QryAngajamenteDefalcate.Refresh;
end;

procedure TfrmAlopAngajamente.ClearAngInBaza;
begin
  SetAngFieldValue('id_angajament_legal', Null);
  edLegal.Text := '';
  edLegal.Tag  := 0;
end;

procedure TfrmAlopAngajamente.ClearAngParinte;
begin
  SetAngFieldValue('id_parinte', Null);
  edRectificat.Text := '';
  edRectificat.Tag  := 0;
end;

procedure TfrmAlopAngajamente.ValidateProiect(Sender: TField);
begin
  FSelectieCE.SetParameter('id_oi_proiecte', Sender.Value);
end;

procedure TfrmAlopAngajamente.SetAngConex(refAngConex: Integer);
begin
  btnOkGenerare.Tag := refAngConex;
end;

procedure TfrmAlopAngajamente.btnOkGenerareAngajament(Sender: TObject);
var
  lIdAng: Integer;
begin
  lIdAng := DBGetScallarFmt('exec [spGenerareAngajamentConext] %d, %d, %d', [IdUtilizator, FCurentAngajament, btnOkGenerare.Tag]);
  LoadAngajament(lIdAng);
end;

procedure TfrmAlopAngajamente.btnOkGenerareFactura(Sender: TObject);

  procedure GenerareFactura;
  begin
    DBExecSQLFmt('exec [spCopiazaFacturaFromAngajament] %d, %d', [IdUtilizator, FCurentAngajament]);
  end;

var
  lIdDocument: Integer;
begin
  lIdDocument := ValueSafeToInt( DBGetScallarFmt('exec [spGestListaDocumente] %d', [IdUtilizator], 0), -1);
  if lIdDocument <> -1 then
    case MessageDlg('Exista deja definit un document in partea de culegere !'#13#10+
                    'Doriti suprascrierea lui?  (apasati Yes)'#13#10+
                    'Desciderea lui in editare? (apasati No)'#13#10+
                    'Sau abandonul?             (apasati Cancel)', mtConfirmation, [mbYes, mbNo, mbCancel], 0) of
      mrYes:
        begin
          DBExecSQLFmt('exec [spGestEmptyDocum] %d, %d', [IdUtilizator, lIdDocument]);
          GenerareFactura;
        end;
      mrNo:
        ;
      mrCancel:
        Abort;
    end
  else begin
    GenerareFactura;
  end;
  ExecuteCommand('Cmd_TranzactiiCV');
end;



 procedure SetSQLDefalcareInsertWithJoin(Q: TZQuery; const IDParinte: Integer);
begin
  with Q do
  begin
    Close;
    SQL.Clear;
    SQL.Text :=
      'SELECT D.*, A.NUMAR, A.DATA_EMITERE, A.COD_FUNCTIONAL ' +
      'FROM ALOP_ANGAJAMENTE_DEFALCARE D ' +
      'JOIN ALOP_ANGAJAMENTE A ON A.ID_ALOP_ANGAJAMENTE = D.ID_ALOP_ANGAJAMENTE ' +
      'WHERE D.ID_POZITIE_PARINTE = :ID_PARINTE';
    ParamByName('ID_PARINTE').AsInteger := IDParinte;
    Open;
  end;
end;


procedure TfrmAlopAngajamente.Button1Click(Sender: TObject);
var
  f:  TfrmAlopAngDisponibil;
begin
  f :=  TfrmAlopAngDisponibil.Create(Self);
  try
    f.ShowModal;
  finally
    f.Free;
  end;
end;




procedure TfrmAlopAngajamente.edNumarDocPropertiesEditValueChanged(
  Sender: TObject);
begin
  SetAngFieldValue('NUMAR', edNumarDoc.EditValue);
end;

procedure TfrmAlopAngajamente.edNrProiectPropertiesEditValueChanged(
  Sender: TObject);
begin
  SetAngFieldValue('NR_PROIECT', edNrProiect.Text);
end;

procedure TfrmAlopAngajamente.edtDetaliiContractPropertiesCloseUp(
  Sender: TObject);
var
  lIdContract : Integer;
  lDataContract : TDateTime;
  lNrContract : String;
begin
  FSelectieContract.FilterContractByDepartament(-1);
  FSelectieContract.FilterContractByPrestator(-1);
  if TAccesscxPopupEdit(Sender).PopupWindow.ModalResult = mrOk then
  begin
    FSelectieContract.IdPredator := edPredator.Tag;
    FSelectieContract.IdPrimitor := edPrimitor.Tag;
    FSelectieContract.IdAngajament := CurentAngajament;
    lNrContract := FSelectieContract.NrContract;
    lDataContract := FSelectieContract.DataContract;
    lIdContract := FSelectieContract.IdContract;
    if lIdContract <> 0 then begin
      TcxPopupEdit(Sender).Tag  := lIdContract;
      if ValueIsTrue(DBGetSetare('integrareOne')) and (lIdContract < 0) then begin
        SetAngFieldValue('ref_One_TipProgram', FSelectieContract.refOnTipProgram);
        SetAngFieldValue('ref_One_Contract', FSelectieContract.refOnContract);
        SetAngFieldValue('ID_CONTRACT', Null);
      end
      else begin
        SetAngFieldValue('ID_CONTRACT', lIdContract);
      end;
      SetAngFieldValue('NR_CONTRACT', FSelectieContract.NrContract);
      SetAngFieldValue('DATA_CONTRACT', FSelectieContract.DataContract);
      FSelectieCF.SetParameter('idContract', lIdContract);
      TcxPopupEdit(Sender).Text := 'Contract Nr. ' + lNrContract + ' din ' + FormatDateTime('dd.mm.yyyy', lDataContract);
      ActivateGrid;
      SetNextControl;
    end;
  end;
end;

procedure TfrmAlopAngajamente.edtScopPropertiesEditValueChanged(
  Sender: TObject);
begin
  SetAngFieldValue('SCOPUL', edtScop.Text);
end;

procedure TfrmAlopAngajamente.cxCheckBox1PropertiesEditValueChanged(
  Sender: TObject);
begin
  SetAngFieldValue('RECTIFICARE', chkRectificareSold.EditValue);
end;

end.
