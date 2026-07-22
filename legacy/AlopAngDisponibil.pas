unit AlopAngDisponibil;

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
  dxDateRanges, dxScrollbarAnnotations,cxLookupEdit, cxDBLookupComboBox,
  cxSplitter, cxDBEdit, dxmdaset, Bde.DBTables;

type
  TfrmAlopAngDisponibil = class(TForm)
    Panel1: TPanel;
    Panel2: TPanel;
    grDetaliiAngajament: TcxGroupBox;
    cxGroupBox1: TcxGroupBox;
    cxSplitter1: TcxSplitter;
    Panel3: TPanel;
    cxSplitter2: TcxSplitter;
    cxButton1: TcxButton;
    cxButton2: TcxButton;
    cxGrid1DBTableView1: TcxGridDBTableView;
    cxGrid1Level1: TcxGridLevel;
    cxGrid1: TcxGrid;
    Panel4: TPanel;
    griddefalcare: TcxGrid;
    griddefalcareDBTableView1: TcxGridDBTableView;
    niveldefalcare: TcxGridLevel;
    cxLabel1: TcxLabel;
    dtAngajamente: TDataSource;
    qryAngajamente: TZReadOnlyQuery;
    dtAngDefalcare: TDataSource;
    qryDefalcare: TZReadOnlyQuery;
    dtNewAng: TDataSource;
    tblNewAng: TdxMemData;
    qryAngajamenteID_ALOP_ANGAJAMENTE: TIntegerField;
    qryAngajamenteID_UTILIZATORI: TIntegerField;
    qryAngajamenteDATA_EMITERE: TDateTimeField;
    qryAngajamenteID_DEPARTAMENT: TIntegerField;
    qryAngajamenteNUMAR: TStringField;
    qryAngajamenteSCOPUL: TStringField;
    qryAngajamenteID_LST_REPARTITORI: TIntegerField;
    qryAngajamenteVALIDAT: TIntegerField;
    qryAngajamenteTIME_IMPORT: TBlobField;
    qryAngajamenteCLASA_FUNCTIONALA: TStringField;
    qryAngajamenteTIP_ANGAJAMENT: TIntegerField;
    qryAngajamenteRECTIFICARE: TBooleanField;
    qryAngajamenteID_CONTRACT: TIntegerField;
    qryAngajamenteID_ACT_ADITIONAL: TIntegerField;
    qryAngajamenteESTE_INCHIS: TBooleanField;
    qryAngajamenteID_PARINTE: TIntegerField;
    qryAngajamenteSTARE: TIntegerField;
    qryAngajamenteCOD_FUNCTIONAL: TStringField;
    qryAngajamenteDATA_OPERARE: TDateTimeField;
    qryAngajamenteDATA: TDateTimeField;
    qryAngajamenteDATA_ANULARE: TDateTimeField;
    qryAngajamenteID_ANALITIC: TIntegerField;
    qryAngajamenteCOD_ECRAN: TStringField;
    qryAngajamenteDATA_STERGERE: TDateTimeField;
    qryAngajamenteUSER_STERGERE: TIntegerField;
    qryAngajamenteCoduriEconomice: TStringField;
    qryAngajamenteProiect: TStringField;
    qryAngajamenteNR_CONTRACT: TStringField;
    qryAngajamenteDATA_CONTRACT: TDateTimeField;
    qryAngajamenteMAN_NR: TIntegerField;
    qryAngajamentenr_proiect: TStringField;
    qryAngajamentevizat_trezorerie: TBooleanField;
    qryAngajamenteid_bg_versiune: TIntegerField;
    qryAngajamentenecesita_viza_trezorerie: TBooleanField;
    qryAngajamentedata_viza_trezorerie: TDateTimeField;
    qryAngajamenteID_ANGAJAMENT_LEGAL: TIntegerField;
    qryAngajamenteman_id_orig: TIntegerField;
    qryAngajamenteman_id: TIntegerField;
    qryAngajamenteDATA_INTRODUCERE: TDateTimeField;
    qryAngajamenteref_One_TipProgram: TIntegerField;
    qryAngajamenteref_One_Contract: TIntegerField;
    qryAngajamentesumaContract: TFloatField;
    qryAngajamenteman_den: TStringField;
    qryAngajamentenr_dosar: TStringField;
    qryAngajamentedata_dosar: TDateTimeField;
    qryAngajamenterefDosar: TIntegerField;
    qryAngajamenteSoldPrecedent: TBooleanField;
    qryDefalcareID_ALOP_ANGAJAMENTE_DEFALCARE: TIntegerField;
    qryDefalcareID_ALOP_ANGAJAMENTE: TIntegerField;
    qryDefalcareID_UTILIZATORI: TIntegerField;
    qryDefalcareCOD_ECONOMIC: TStringField;
    qryDefalcareAPROBATE: TFloatField;
    qryDefalcareTOTAL_ANGAJATE: TFloatField;
    qryDefalcareDISPONIBIL: TFloatField;
    qryDefalcareID_VALUTA: TIntegerField;
    qryDefalcareANGAJAT_VALUTA: TFloatField;
    qryDefalcareCURS_VALUTAR: TFloatField;
    qryDefalcareANGAJAT: TFloatField;
    qryDefalcareRAMAS_DE_ANGAJAT: TFloatField;
    qryDefalcareVALIDAT: TIntegerField;
    qryDefalcareDESCRIERE: TStringField;
    qryDefalcareCLASA_ECONOMICA: TStringField;
    qryDefalcareID_ANALITIC: TIntegerField;
    qryDefalcareCOD_ECRAN: TStringField;
    qryDefalcareID_OI_UNITATI: TIntegerField;
    qryDefalcareID_OI_PROIECTE: TIntegerField;
    qryDefalcareeste_credit_angajament: TBooleanField;
    qryDefalcareeste_procentual: TBooleanField;
    qryDefalcareTIMESTAMP: TBlobField;
    qryDefalcaredata_curs: TDateTimeField;
    qryDefalcareprocProiect: TFloatField;
    qryDefalcareangProiect: TFloatField;
    qryDefalcareaprobatDocument: TFloatField;
    qryDefalcareangajatDocument: TFloatField;
    qryDefalcaredisponibilDocument: TFloatField;
    qryDefalcareramasDocument: TFloatField;
    qryDefalcareID_LST_REPARTITORI: TIntegerField;
    qryDefalcareSoldRectificat: TBooleanField;
    qryDefalcareID_POZITIE_PARINTE: TIntegerField;
    Label1: TLabel;
    Button1: TButton;
    txtCautareNumar: TcxTextEdit;
    griddefalcareDBTableView1COD_ECONOMIC: TcxGridDBColumn;
    griddefalcareDBTableView1TOTAL_ANGAJATE: TcxGridDBColumn;
    griddefalcareDBTableView1DISPONIBIL: TcxGridDBColumn;
    griddefalcareDBTableView1ANGAJAT_VALUTA: TcxGridDBColumn;
    griddefalcareDBTableView1ANGAJAT: TcxGridDBColumn;
    griddefalcareDBTableView1RAMAS_DE_ANGAJAT: TcxGridDBColumn;
    griddefalcareDBTableView1ID_LST_REPARTITORI: TcxGridDBColumn;
    StaticText1: TStaticText;
    StaticText2: TStaticText;
    griddefalcareDBTableView1data_curs: TcxGridDBColumn;
    edDisponibil: TcxDBCurrencyEdit;
    edRamasDeAngajat: TcxDBCurrencyEdit;
    qryDefalcareSubpoz: TZQuery;
    dtDefalcareSubpoz: TDataSource;
    qryDefalcareSubpozID_ALOP_ANGAJAMENTE_DEFALCARE: TIntegerField;
    qryDefalcareSubpozID_ALOP_ANGAJAMENTE: TIntegerField;
    qryDefalcareSubpozID_UTILIZATORI: TIntegerField;
    qryDefalcareSubpozCOD_ECONOMIC: TStringField;
    qryDefalcareSubpozAPROBATE: TFloatField;
    qryDefalcareSubpozTOTAL_ANGAJATE: TFloatField;
    qryDefalcareSubpozDISPONIBIL: TFloatField;
    qryDefalcareSubpozID_VALUTA: TIntegerField;
    qryDefalcareSubpozANGAJAT_VALUTA: TFloatField;
    qryDefalcareSubpozCURS_VALUTAR: TFloatField;
    qryDefalcareSubpozANGAJAT: TFloatField;
    qryDefalcareSubpozRAMAS_DE_ANGAJAT: TFloatField;
    qryDefalcareSubpozVALIDAT: TIntegerField;
    qryDefalcareSubpozDESCRIERE: TStringField;
    qryDefalcareSubpozCLASA_ECONOMICA: TStringField;
    qryDefalcareSubpozID_ANALITIC: TIntegerField;
    qryDefalcareSubpozCOD_ECRAN: TStringField;
    qryDefalcareSubpozID_OI_UNITATI: TIntegerField;
    qryDefalcareSubpozID_OI_PROIECTE: TIntegerField;
    qryDefalcareSubpozeste_credit_angajament: TBooleanField;
    qryDefalcareSubpozeste_procentual: TBooleanField;
    qryDefalcareSubpozTIMESTAMP: TBlobField;
    qryDefalcareSubpozdata_curs: TDateTimeField;
    qryDefalcareSubpozprocProiect: TFloatField;
    qryDefalcareSubpozangProiect: TFloatField;
    qryDefalcareSubpozaprobatDocument: TFloatField;
    qryDefalcareSubpozangajatDocument: TFloatField;
    qryDefalcareSubpozdisponibilDocument: TFloatField;
    qryDefalcareSubpozramasDocument: TFloatField;
    qryDefalcareSubpozID_LST_REPARTITORI: TIntegerField;
    qryDefalcareSubpozSoldRectificat: TBooleanField;
    qryDefalcareSubpozID_POZITIE_PARINTE: TIntegerField;
    cxGrid1DBTableView1ID_ALOP_ANGAJAMENTE_DEFALCARE: TcxGridDBColumn;
    cxGrid1DBTableView1COD_ECONOMIC: TcxGridDBColumn;
    cxGrid1DBTableView1TOTAL_ANGAJATE: TcxGridDBColumn;
    cxGrid1DBTableView1DISPONIBIL: TcxGridDBColumn;
    cxGrid1DBTableView1ANGAJAT_VALUTA: TcxGridDBColumn;
    cxGrid1DBTableView1ANGAJAT: TcxGridDBColumn;
    cxGrid1DBTableView1RAMAS_DE_ANGAJAT: TcxGridDBColumn;
    cxGrid1DBTableView1data_curs: TcxGridDBColumn;
    cxGrid1DBTableView1ID_LST_REPARTITORI: TcxGridDBColumn;
    cxGrid1DBTableView1ID_POZITIE_PARINTE: TcxGridDBColumn;
    ZUpdateSQLSubpoz: TUpdateSQL;
    qryExec: TZQuery;
    procedure FormShow(Sender: TObject);
   procedure griddefalcareDBTableView1FocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
      procedure btnAdaugaSubpozitieClick(Sender: TObject);
      procedure RecalculeazaSume;
    procedure Button1Click(Sender: TObject);
    procedure griddefalcareDBTableView1RecordChanged(
  ADataController: TcxCustomDataController; ARecordIndex, AItemIndex: Integer);
    procedure txtCautareNumarKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure qryDefalcareSubpozAfterPost(DataSet: TDataSet);
    procedure qryDefalcareSubpozAfterDelete(DataSet: TDataSet);
    procedure cxButton1Click(Sender: TObject);
    procedure griddefalcareDBTableView1ANGAJAT_VALUTAPropertiesEditValueChanged(
  Sender: TObject);
     procedure AdaugaSubpozitieNoua;
     procedure griddefalcareDBTableView1KeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
     procedure btnStergeSubpozitieClick(Sender: TObject);
   // procedure qryDefalcareSubpozBeforePost(DataSet: TDataSet);
    procedure qryDefalcareSubpozNewRecord(DataSet: TDataSet);
  private
       procedure ValutaChange(Sender: TField);


  public
    { Public declarations }
  protected

  end;



implementation

{$R *.DFM}

uses
  Math, dxCompsUtile, ZeosDBUtile, CommonDBVar, ConcurentUsersUnit, Variants, rapInclude,
  AtlasUtils, FormulareUnit, AlopAngVizualizare, ATSZDBUtils, Types,DateUnit;


procedure TfrmAlopAngDisponibil.txtCautareNumarKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
      if Key = VK_RETURN then
  begin
    Button1Click(nil);
    Key := 0;
  end;
end;


procedure TfrmAlopAngDisponibil.Button1Click(Sender: TObject);
begin
  qryDefalcare.Close;
  qryDefalcare.SQL.Text :=
  'SELECT d.* ' +
  'FROM alop_angajamente a ' +
  'JOIN alop_angajamente_defalcare d ON d.id_alop_angajamente = a.id_alop_angajamente ' +
  'WHERE a.numar = :numar ' +
  'AND d.id_pozitie_parinte IS NULL';


  qryDefalcare.ParamByName('numar').AsString := Trim(txtCautareNumar.Text);
  qryDefalcare.Open;

   if qryDefalcare.IsEmpty then
    ShowMessage('Nu exista inregistrari.');


end;
procedure TfrmAlopAngDisponibil.ValutaChange(Sender: TField);
var
  i, colIdx : Integer;
  Suma      : Double;
  View      : TcxGridDBTableView;
begin

  View   := cxGrid1DBTableView1;
  colIdx := View.GetColumnByFieldName('ANGAJAT_VALUTA').Index;


  Suma := 0;
  for i := 0 to View.DataController.RecordCount - 1 do
   Suma := Suma + VarAsType(
           View.DataController.Values[i, colIdx], varDouble);



  edRamasDeAngajat.Value := edDisponibil.Value - Suma;


  if edRamasDeAngajat.Value < 0 then
    edRamasDeAngajat.Style.Color := clRed
  else
    edRamasDeAngajat.Style.Color := clWindow;
end;

procedure TfrmAlopAngDisponibil.btnAdaugaSubpozitieClick(Sender: TObject);
begin
  if qryDefalcare.IsEmpty then
  begin
    ShowMessage('Selectează mai întâi o poziție principală.');
    Exit;
  end;

  qryExec.SQL.Text :=
    'EXEC sp_InsertAlopDefalcareSubpoz ' +
    ':ID_ALOP_ANGAJAMENTE, :ID_UTILIZATORI, :COD_ECONOMIC, :ID_POZITIE_PARINTE, ' +
    ':ANGAJAT_VALUTA, :RAMAS_DE_ANGAJAT, :ID_LST_REPARTITORI, :DATA_CURS';

  qryExec.ParamByName('ID_ALOP_ANGAJAMENTE').AsInteger := qryDefalcareID_ALOP_ANGAJAMENTE.AsInteger;
  qryExec.ParamByName('ID_UTILIZATORI').AsInteger := 1;
  qryExec.ParamByName('COD_ECONOMIC').AsString := qryDefalcareCOD_ECONOMIC.AsString;
  qryExec.ParamByName('ID_POZITIE_PARINTE').AsInteger := qryDefalcareID_ALOP_ANGAJAMENTE_DEFALCARE.AsInteger;
  qryExec.ParamByName('ANGAJAT_VALUTA').AsFloat := 0;
  qryExec.ParamByName('RAMAS_DE_ANGAJAT').AsFloat := 0;
  qryExec.ParamByName('ID_LST_REPARTITORI').AsInteger := 0;
  qryExec.ParamByName('DATA_CURS').AsDateTime := Now;

  qryExec.ExecSQL;

 // ShowMessage('Subpoziție adăugată.');
  qryDefalcareSubpoz.Close;
  qryDefalcareSubpoz.ParamByName('id_parinte').AsInteger := qryDefalcareID_ALOP_ANGAJAMENTE_DEFALCARE.AsInteger;
  qryDefalcareSubpoz.Open;
end;

 procedure TfrmAlopAngDisponibil.griddefalcareDBTableView1ANGAJAT_VALUTAPropertiesEditValueChanged(
  Sender: TObject);
begin
  RecalculeazaSume;
end;



procedure TfrmAlopAngDisponibil.griddefalcareDBTableView1FocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
var
  idParinte: Integer;
begin
  if not qryDefalcare.IsEmpty then
  begin
    idParinte := qryDefalcareID_ALOP_ANGAJAMENTE_DEFALCARE.AsInteger;
    edDisponibil.Value := qryDefalcareDisponibil.AsFloat;

    qryDefalcareSubpoz.Close;
    qryDefalcareSubpoz.ParamByName('id_parinte').AsInteger := idParinte;
    qryDefalcareSubpoz.Open;

    RecalculeazaSume;
  end
  else
  begin
    qryDefalcareSubpoz.Close;
    edDisponibil.Value := 0;
    edRamasDeAngajat.Value := 0;
  end;
end;


procedure TfrmAlopAngDisponibil.qryDefalcareSubpozAfterDelete(
  DataSet: TDataSet);
begin
RecalculeazaSume;
end;

procedure TfrmAlopAngDisponibil.qryDefalcareSubpozAfterPost(DataSet: TDataSet);
begin
RecalculeazaSume;
end;


procedure TfrmAlopAngDisponibil.qryDefalcareSubpozNewRecord(DataSet: TDataSet);
begin
  qryDefalcareSubpoz.FieldByName('ANGAJAT_VALUTA').AsFloat := 0;
  qryDefalcareSubpoz.FieldByName('ID_POZITIE_PARINTE').AsInteger := qryDefalcare.FieldByName('ID_ALOP_ANGAJAMENTE_DEFALCARE').AsInteger;

  end;

procedure TfrmAlopAngDisponibil.cxButton1Click(Sender: TObject);
begin
  if qryDefalcareSubpoz.State in [dsEdit, dsInsert] then
    qryDefalcareSubpoz.Post;

  qryExec.SQL.Text :=
    'EXEC sp_UpdateAlopDefalcareSubpoz ' +
    ':ID_ALOP_ANGAJAMENTE_DEFALCARE, :ANGAJAT_VALUTA, :RAMAS_DE_ANGAJAT, :ID_LST_REPARTITORI, :DATA_CURS';

  qryExec.ParamByName('ID_ALOP_ANGAJAMENTE_DEFALCARE').AsInteger := qryDefalcareSubpozID_ALOP_ANGAJAMENTE_DEFALCARE.AsInteger;
  qryExec.ParamByName('ANGAJAT_VALUTA').AsFloat := qryDefalcareSubpozANGAJAT_VALUTA.AsFloat;
  qryExec.ParamByName('RAMAS_DE_ANGAJAT').AsFloat := qryDefalcareSubpozRAMAS_DE_ANGAJAT.AsFloat;
  qryExec.ParamByName('ID_LST_REPARTITORI').AsInteger := qryDefalcareSubpozID_LST_REPARTITORI.AsInteger;
  qryExec.ParamByName('DATA_CURS').AsDateTime := qryDefalcareSubpozdata_curs.AsDateTime;

  qryExec.ExecSQL;

  ShowMessage('Salvat cu succes.');
  qryDefalcareSubpoz.Close;
  qryDefalcareSubpoz.Open;
end;

procedure TfrmAlopAngDisponibil.btnStergeSubpozitieClick(Sender: TObject);
begin
  if qryDefalcareSubpoz.IsEmpty then
  begin
    ShowMessage('Nu există nicio subpozitie selectată.');
    Exit;
  end;

  if MessageDlg('Ești sigur că vrei să ștergi această subpozitie?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then
  begin
    qryExec.SQL.Text := 'EXEC sp_DeleteAlopDefalcareSubpoz :ID_ALOP_ANGAJAMENTE_DEFALCARE';
    qryExec.ParamByName('ID_ALOP_ANGAJAMENTE_DEFALCARE').AsInteger := qryDefalcareSubpozID_ALOP_ANGAJAMENTE_DEFALCARE.AsInteger;
    qryExec.ExecSQL;

    ShowMessage('Subpozitia a fost stearsa.');
    qryDefalcareSubpoz.Close;
    qryDefalcareSubpoz.ParamByName('id_parinte').AsInteger := qryDefalcareID_ALOP_ANGAJAMENTE_DEFALCARE.AsInteger;
    qryDefalcareSubpoz.Open;
  end;
end;




procedure TfrmAlopAngDisponibil.FormShow(Sender: TObject);
var
  id: Integer;
begin
qryExec := TZQuery.Create(Self);
  qryExec.Connection := frmData.dbContabilitate;
  qryDefalcareSubpoz.FieldByName('ANGAJAT_VALUTA').OnChange := ValutaChange;


  if not qryAngajamente.IsEmpty then
  begin
    id := qryAngajamenteID_ALOP_ANGAJAMENTE.AsInteger;
    qryDefalcare.Close;
    qryDefalcare.ParamByName('id').AsInteger := qryAngajamenteID_ALOP_ANGAJAMENTE.AsInteger;
    qryDefalcare.Open;
    edDisponibil.Value := qryDefalcare.FieldByName('Disponibil').AsFloat;
     cxGrid1DBTableView1COD_ECONOMIC.Options.Editing := False;
  end;
end;

  procedure TfrmAlopAngDisponibil.griddefalcareDBTableView1RecordChanged(
  ADataController: TcxCustomDataController; ARecordIndex, AItemIndex: Integer);
begin
  RecalculeazaSume;
end;



procedure TfrmAlopAngDisponibil.griddefalcareDBTableView1KeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
begin
  if Key = VK_RETURN then
  begin
    if qryDefalcareSubpoz.State in [dsEdit, dsInsert] then
      qryDefalcareSubpoz.Post;

    RecalculeazaSume;
  end;
end;

procedure TfrmAlopAngDisponibil.RecalculeazaSume;
var
  SumaAlocata: Double;
begin
  SumaAlocata := 0;
  qryDefalcareSubpoz.DisableControls;
  try
    qryDefalcareSubpoz.First;
    while not qryDefalcareSubpoz.Eof do
    begin
      SumaAlocata := SumaAlocata + qryDefalcareSubpoz.FieldByName('ANGAJAT_VALUTA').AsFloat;
      qryDefalcareSubpoz.Next;
    end;
  finally
    qryDefalcareSubpoz.EnableControls;
  end;


  edRamasDeAngajat.Value := edDisponibil.Value - SumaAlocata;
end;
 procedure TfrmAlopAngDisponibil.AdaugaSubpozitieNoua;
begin
  if qryDefalcare.IsEmpty then Exit;

  qryDefalcareSubpoz.Append;
  qryDefalcareSubpoz.FieldByName('ID_ALOP_ANGAJAMENTE').AsInteger :=
    qryDefalcare.FieldByName('ID_ALOP_ANGAJAMENTE').AsInteger;

  qryDefalcareSubpoz.FieldByName('ID_POZITIE_PARINTE').AsInteger :=
    qryDefalcare.FieldByName('ID_ALOP_ANGAJAMENTE_DEFALCARE').AsInteger;


  qryDefalcareSubpoz.FieldByName('ANGAJAT_VALUTA').AsFloat := 0;
  qryDefalcareSubpoz.FieldByName('RAMAS_DE_ANGAJAT').AsFloat := 0;

  qryDefalcareSubpoz.Post;
end;



end.
