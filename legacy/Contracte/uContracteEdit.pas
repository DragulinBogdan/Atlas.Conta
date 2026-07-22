unit uContracteEdit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxGraphics, cxLookAndFeels, cxLookAndFeelPainters, Menus,
  StdCtrls, cxButtons, ExtCtrls, DegradePanel, DB, ZAbstractRODataset,
  ZAbstractDataset, ZDataset, cxControls, cxStyles, cxCustomData, cxFilter,
  cxData, cxDataStorage, cxEdit, cxDBData, cxGridCustomTableView,
  cxGridTableView, cxGridDBTableView, cxGridLevel, cxClasses,
  cxGridCustomView, cxGrid, dxLayoutControl, Mask, DBCtrls,
  dxLayoutcxEditAdapters, cxContainer, cxTextEdit, cxDBEdit, cxCalc,
  cxSpinEdit, cxMaskEdit, cxDropDownEdit, cxCalendar,
  cxImageComboBox, dxmdaset, cxButtonEdit,
  cxLookupEdit, cxDBLookupEdit, cxDBLookupComboBox, dateUnit,
  dxLayoutControlAdapters, dxLayoutContainer, cxNavigator,
  cxDataControllerConditionalFormattingRulesManagerDialog, cxCurrencyEdit,
  dxDateRanges, dxScrollbarAnnotations;

type
  TfrmContractEdit = class(TForm)
    pnBottom: TPanel;
    btnSave: TcxButton;
    btnCancel: TcxButton;
    Panel1: TPanel;
    DTContract: TDataSource;
    lyContract: TdxLayoutControl;
    lyContractGroup_Root1: TdxLayoutGroup;
    edNrContract: TcxDBTextEdit;
    lyContractItem1: TdxLayoutItem;
    edDataContract: TcxDBDateEdit;
    lyContractItem2: TdxLayoutItem;
    edValValuta: TcxDBCalcEdit;
    lyContractItem5: TdxLayoutItem;
    edValoare: TcxDBCalcEdit;
    lyContractItem7: TdxLayoutItem;
    edDurataContract: TcxDBSpinEdit;
    lyContractItem8: TdxLayoutItem;
    edDataSemnare: TcxDBDateEdit;
    lyContractItem9: TdxLayoutItem;
    edDataInceput: TcxDBDateEdit;
    lyContractItem10: TdxLayoutItem;
    edDataSfarsit: TcxDBDateEdit;
    lyContractItem11: TdxLayoutItem;
    edProcentGarantie: TcxDBCalcEdit;
    lyContractItem12: TdxLayoutItem;
    edNrDosar: TcxDBTextEdit;
    lyContractItem13: TdxLayoutItem;
    edDataDosar: TcxDBDateEdit;
    lyContractItem14: TdxLayoutItem;
    edTotalValuta: TcxDBCalcEdit;
    lyContractItem15: TdxLayoutItem;
    edTotal: TcxDBCalcEdit;
    lyContractItem16: TdxLayoutItem;
    edGarantieTotal: TcxDBCalcEdit;
    lyContractItem17: TdxLayoutItem;
    edGarantieConstituita: TcxDBCalcEdit;
    lyContractItem18: TdxLayoutItem;
    edGarantieVarsata: TcxDBCalcEdit;
    lyContractItem19: TdxLayoutItem;
    NrDataContract: TdxLayoutGroup;
    lyContractGroup4: TdxLayoutGroup;
    ValoareContract: TdxLayoutGroup;
    Totalizare: TdxLayoutGroup;
    Desfasurare: TdxLayoutGroup;
    Conditii: TdxLayoutGroup;
    Achizitie: TdxLayoutGroup;
    lyContractGroup1: TdxLayoutGroup;
    lyContractGroup3: TdxLayoutGroup;
    AlteDetalii: TdxLayoutGroup;
    edTipValuta: TcxDBImageComboBox;
    lyContractItem4: TdxLayoutItem;
    edCurs: TcxDBCalcEdit;
    lyContractItem6: TdxLayoutItem;
    edPrestator: TcxDBLookupComboBox;
    lyContractItem21: TdxLayoutItem;
    edBeneficiar: TcxDBLookupComboBox;
    lyContractItem20: TdxLayoutItem;
    edParinte: TcxDBButtonEdit;
    lyContractItem3: TdxLayoutItem;
    btnNew: TcxButton;
    btnAnuleaza: TcxButton;
    BtnModificare: TcxButton;
    qryContract: TZQuery;
    DTConsortiu: TDataSource;
    qryConsortiu: TZQuery;
    GridConsortiu: TcxGridDBTableView;
    GridConsortiuL1: TcxGridLevel;
    cxGridConsortiu: TcxGrid;
    lyContractItem22: TdxLayoutItem;
    Consortiu: TdxLayoutGroup;
    Panel2: TPanel;
    lyContractItem23: TdxLayoutItem;
    btnAddConsortiu: TcxButton;
    btnDelConsortiu: TcxButton;
    GridConsortiuid_contracte: TcxGridDBColumn;
    GridConsortiuid_repartitori: TcxGridDBColumn;
    GridConsortiulider: TcxGridDBColumn;
    GridConsortiuid_contracte_consortiu: TcxGridDBColumn;
    dxLayoutItem2: TdxLayoutItem;
    edProiect: TcxDBLookupComboBox;
    procedure edParintePropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
    procedure MemContractNewRecord(DataSet: TDataSet);
    procedure edParintePropertiesNewLookupDisplayText(Sender: TObject;
      const AText: TCaption);
    procedure qryContractNewRecord(DataSet: TDataSet);
    procedure btnCancelClick(Sender: TObject);
    procedure btnSaveClick(Sender: TObject);
    procedure qryContractAfterOpen(DataSet: TDataSet);
    procedure btnNewClick(Sender: TObject);
    procedure btnAnuleazaClick(Sender: TObject);
    procedure BtnModificareClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure btnAddConsortiuClick(Sender: TObject);
    procedure qryConsortiuNewRecord(DataSet: TDataSet);
    procedure btnDelConsortiuClick(Sender: TObject);
    procedure edPrestatorPropertiesInitPopup(Sender: TObject);
    procedure edPrestatorPropertiesCloseUp(Sender: TObject);
    procedure edBeneficiarPropertiesInitPopup(Sender: TObject);
    procedure edBeneficiarPropertiesCloseUp(Sender: TObject);
    procedure FormCreate(Sender: TObject);
  private
    FIdContract: Integer;
    { Private declarations }
    function TestUniqueNumber : Boolean;
    procedure DoCalcValuta(Sender : TField);
    procedure DoPerioadaChange(Sender : TField);
    procedure DoProcentGarantieChange(Sender: TField);
    procedure DoValoareChange(Sender: TField);
    procedure ReadContractParinte(aIdContract : Integer);
    procedure SetIdContract(const Value: Integer);
    function  ValidareEcran : String;
    function  IsContractValidat : Boolean;
    procedure TestGolireEcran;
    procedure SetDetaliiValidare(AIsFinalizat : Boolean);
  public
    { Public declarations }
    IsInLoading : Boolean;
    procedure SetCalcDefaults;
    procedure ReadContractEcran(const aIdContract : Integer = 0);
    procedure LoadContract(aId : Integer);
    class function  NewContract : Integer;

    class function OKToModify(aId : Integer; const aSilent : Boolean = False) : Boolean;
    property IdContract : Integer read FIdContract write SetIdContract;
  end;


function ModificareContract(aIdContract : Integer) : TForm;


implementation

uses
  ZeosDBUtile, uContracte, DateUtils, cxDateUtils, ppComm, CommonDBVar, ConcurentUsersUnit,
  dxCompsUtile, FormulareUnit;

{$R *.dfm}

function ModificareContract(aIdContract : Integer) : TForm;
begin
  if not TfrmContractEdit.OKToModify(aIdContract) then Exit;
  if EnterSingleUser(TfrmContractEdit) then begin
    Result := TForm(GetNewForm(TfrmContractEdit));
    with TfrmContractEdit(Result) do begin
      WindowState := wsMaximized;
      LoadContract(aIdContract);
    end;
  end;
end;

procedure TfrmContractEdit.edBeneficiarPropertiesCloseUp(Sender: TObject);
begin
  frmData.DTRepartitori.DataSet.Filter := '';
  frmData.DTRepartitori.DataSet.Filtered := False;
end;

procedure TfrmContractEdit.edBeneficiarPropertiesInitPopup(Sender: TObject);
begin
  frmData.DTRepartitori.DataSet.Filter := 'GESTINT = 1 OR GESTINT IS NULL';
  frmData.DTRepartitori.DataSet.Filtered := True;
end;

procedure TfrmContractEdit.edParintePropertiesButtonClick(Sender: TObject;
  AButtonIndex: Integer);
var
  lIdContract : Integer;
  lDesc : string;
begin
  if AButtonIndex = 1 then begin
    edParinte.EditValue := null;
    edParinte.Hint := '';
  end
  else begin
     lIdContract := SelectieContract(lDesc);
     if lIdContract = -1 then begin
       edParinte.EditValue := null;
       edParinte.Hint := '';
     end
     else begin
       edParinte.EditValue := lIdContract;
       edParinte.Hint := lDesc;
       ReadContractParinte(lIdContract);
     end;
  end;
end;

procedure TfrmContractEdit.MemContractNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('STARE').AsInteger := 1;
end;

procedure TfrmContractEdit.DoCalcValuta(Sender: TField);
begin
  Sender.DataSet.FieldByName('VALOARE').AsCurrency :=
    Sender.DataSet.FieldByName('VALOARE_VALUTA').AsCurrency *
    Sender.DataSet.FieldByName('CURS_VALUTAR').AsCurrency;
end;

procedure TfrmContractEdit.DoPerioadaChange(Sender: TField);
begin
  if cxIsDateValid(Sender.DataSet.FieldByName('DATA_INCEPUT').AsDateTime) and
     cxIsDateValid(Sender.DataSet.FieldByName('DATA_SFARSIT').AsDateTime) then
      Sender.DataSet.FieldByName('DURATA_CONTRACT').AsInteger :=
        Trunc(
        Sender.DataSet.FieldByName('DATA_SFARSIT').AsDateTime -
        Sender.DataSet.FieldByName('DATA_INCEPUT').AsDateTime);

end;

procedure TfrmContractEdit.DoProcentGarantieChange(Sender: TField);
begin
  qryContract['TOTAL_GARANTIE'] := Sender.AsCurrency * ValueSafeToCurrency(qryContract['VALOARE'], 0) / 100.00;
  qryContract['TOTAL_GARANTIE_CONSTITUITA'] := qryContract['TOTAL_GARANTIE'];
  qryContract['TOTAL_GARANTIE_VARSATA'] := qryContract['TOTAL_GARANTIE'];
end;

procedure TfrmContractEdit.DoValoareChange(Sender: TField);
begin
  qryContract['TOTAL_GARANTIE'] := Sender.AsCurrency * ValueSafeToCurrency(qryContract['GARANTIE_PROCENT'], 0) / 100.00;
  qryContract['TOTAL_GARANTIE_CONSTITUITA'] := qryContract['TOTAL_GARANTIE'];
  qryContract['TOTAL_GARANTIE_VARSATA'] := qryContract['TOTAL_GARANTIE'];
end;

procedure TfrmContractEdit.SetCalcDefaults;
begin
  qryContract.FieldByName('VALOARE_VALUTA').OnChange := DoCalcValuta;
  qryContract.FieldByName('CURS_VALUTAR').OnChange := DoCalcValuta;
  qryContract.FieldByName('DATA_INCEPUT').OnChange := DoPerioadaChange;
  qryContract.FieldByName('DATA_SFARSIT').OnChange := DoPerioadaChange;
  qryContract.FieldByName('VALOARE').OnChange := DoValoareChange;
  qryContract.FieldByName('GARANTIE_PROCENT').OnChange := DoProcentGarantieChange;
end;

procedure TfrmContractEdit.ReadContractParinte(aIdContract: Integer);
begin
   with GetTmpADOQuery do
   try
     SQL.Add('exec spContractSetParinte '+ IntToStr(IdContract) + ',' +  IntToStr(aIdContract));
     ExecSQL;
     ReadContractEcran(IdContract);
   finally
     Free;
   end;
end;

procedure TfrmContractEdit.edParintePropertiesNewLookupDisplayText(
  Sender: TObject; const AText: TCaption);
begin
 // AText := edParinte.Hint;
end;

procedure TfrmContractEdit.edPrestatorPropertiesCloseUp(Sender: TObject);
begin
  frmData.DTRepartitori.DataSet.Filter := '';
  frmData.DTRepartitori.DataSet.Filtered := False;
end;

procedure TfrmContractEdit.edPrestatorPropertiesInitPopup(Sender: TObject);
begin
  frmData.DTRepartitori.DataSet.Filter := 'GESTINT = 0 OR GESTINT IS NULL';
  frmData.DTRepartitori.DataSet.Filtered := True;
end;

procedure TfrmContractEdit.qryContractNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('STARE').AsInteger := 2;
end;

procedure TfrmContractEdit.ReadContractEcran;
begin
  //citim contractul curent din ecran dupa stare = 2 pentru utilizator curent
  //daca sunt mai multe va fi deschis ultimul
  with GetTmpADOQuery do
  try
    Sql.Add('exec spContractNevalidate ' + IntToStr(IdUtilizator) + ', ' + IntToStr(aIdContract));
    Open;
    IsInLoading := True;
    if not IsEmpty then begin
       if False and (RecordCount > 1) then begin
         //TODO : lista de selectie contracte
       end else begin
         IdContract := FieldByName('id_contracte').AsInteger;
       end;
    end else
       IdContract := -1; //facem unul nou
    IsInLoading := False;    
  finally
    Free;
  end;
end;

procedure TfrmContractEdit.SetIdContract(const Value: Integer);
begin
  FIdContract := Value;
  with qryContract do begin
    if FIdContract = -1 then
      FIdContract := NewContract
    else begin
      Close;
      ParamByName('id_contracte').AsInteger := FIdContract;
      Open;
    end;
  end;
  qryConsortiu.Close;
  qryConsortiu.ParamByName('id_contracte').AsInteger := FIdContract;
  qryConsortiu.Open;
end;

class function TfrmContractEdit.NewContract: Integer;
begin
  with GetTmpADOQuery do
  try
     SQL.Add('exec spContractNewContract ' + IntToStr(IdUtilizator));
     Open;
     Result := Fields[0].AsInteger;
  finally
    Free;
  end;
end;

function TfrmContractEdit.ValidareEcran: String;
begin
  Result := '';
  if qryContract.FieldByName('NR_CONTRACT').AsString = '' then
    Result := Result + 'Trebuie completat [Nr Contract] !' + #13#10;
  if not IsValidDate(qryContract.FieldByName('DATA_CONTRACT').AsDateTime) then
    Result := Result + 'Trebuie completata data contractului [Data contract] !' + #13#10;
  if qryContract.FieldByName('ID_PRESTATOR').AsInteger = 0 then
    Result := Result + 'Trebuie completat prestatorul [Prestator] !' + #13#10;
  if qryContract.FieldByName('ID_BENEFICIAR').AsInteger = 0 then
    Result := Result + 'Trebuie completat beneficiarul [Beneficiar] !' + #13#10;
  if qryContract.FieldByName('VALOARE').AsCurrency = 0 then
    Result := Result + 'Trebuie completata valoarea contractului [Valoare] !' + #13#10;
end;

procedure TfrmContractEdit.btnCancelClick(Sender: TObject);
begin
  Close;
end;

function TfrmContractEdit.IsContractValidat: Boolean;
begin
  Result := False;
  if not qryContract.IsEmpty then
    Result := (qryContract.FieldByName('stare').AsInteger = 1);
end;

procedure TfrmContractEdit.btnSaveClick(Sender: TObject);
var
  lErrRecord : string;
begin
  DoCheckPostDataSet(qryConsortiu);
  DoCheckPostDataSet(qryContract);
  if btnSave.Tag= -1 then begin
    if OKToModify(IdContract) then DBSetFieldValue(qryContract, 'stare', 2);
  end else begin
    lErrRecord := ValidareEcran;
    if not (lErrRecord = '') then begin
      MessageDlg(lErrRecord, mtError, [mbOk],0);
//      SetNextControl;
      Abort;
    end else
      if TestUniqueNumber then begin
        DBExecSQLFmt('exec [spContractValidare] %d', [IdContract]);
        qryContract.Refresh;
        qryConsortiu.Refresh;
      end;
   end;
   if not qryContract.IsEmpty then
     SetDetaliiValidare(qryContract.FieldByName('stare').AsInteger = 1);
end;

class function TfrmContractEdit.OKToModify(aId: Integer;
  const aSilent: Boolean): Boolean;
var
  lErrorMsg: Variant;
begin
  lErrorMsg := DBGetScallarFmt('exec [spContractUsed] %d', [aId]);
  Result    := not ValueHasValue(lErrorMsg);
  if not Result and not aSilent then
    MessageDlg(ValueToStr(lErrorMsg), mtError, [mbOK], 0);
end;

function TfrmContractEdit.TestUniqueNumber: Boolean;
var
  lNumar, lExplicatie : String;
  lValue  : Variant;
begin
  //testam daca numarul alocat este unic si intrebam daca vrea sa-l pastreze sau sa genereze unul nou
  Result := True;
  lValue := DBGetScallarFmt('exec [spContractTestNumber] %d', [IdContract]);
  if ValueHasValue(lValue) then begin
    lNumar      := ValueSafeToStr(lValue[0]);
    lExplicatie := ValueSafeToStr(lValue[1]);
    case MessageDlg(Format('Numarul %s este deja folosit in %s. ', [lNumar, lExplicatie])+#13+#10+'Doriti continuarea si folosirea numarului curent ?', mtConfirmation, [mbYes, mbNo], 0) of
         mrYes :  ;
         mrNo, mrNone :  Result := False;
     end;
  end;
end;

procedure TfrmContractEdit.LoadContract(aId: Integer);
begin
  DBExecSQLFmt('exec [spContractLoad] %d, %d', [IdUtilizator, aId]);
  ReadContractEcran(aId);
end;

procedure TfrmContractEdit.qryContractAfterOpen(DataSet: TDataSet);
begin
  SetCalcDefaults;
  lyContract.Enabled := DTContract.DataSet.Active;
end;

procedure TfrmContractEdit.TestGolireEcran;
begin
  if not IsContractValidat and not qryContract.IsEmpty then
  if (MessageDlg('Modificarea unui document din arhiva duce la pierderea contractului din ecran ! '+#13+#10+'Doriti continuarea ?', mtConfirmation, [mbYes, mbNo], 0) = mrNo) then
      Abort;
end;

procedure TfrmContractEdit.btnNewClick(Sender: TObject);
begin
  TestGolireEcran;
  IdContract := NewContract;
end;

procedure TfrmContractEdit.btnAnuleazaClick(Sender: TObject);
var
  lNr : String;
  lData : String;
begin
  if (IdContract > 0) and (qryContract.State in [dsEdit]) then begin
    lNr := qryContract.FieldByName('nr_contract').AsString;
    lData := qryContract.FieldByName('data_contract').AsString;
    if (MessageDlg(Format('Doriti stergerea angajamentului nr. : %s din data  %s ?', [lNr, lData]),
         mtConfirmation, [mbYes, mbNo], 0) in [mrNo, mrNone]) then
       Abort;
    DBExecSQLFmt('exec [spContractAnuleaza] %d, %d', [IdContract, IdUtilizator]);
    ReadContractEcran;
  end;
end;

procedure TfrmContractEdit.BtnModificareClick(Sender: TObject);
var
   lIdContract : Integer;
   lDesc : string;
begin
  TestGolireEcran;
  lIdContract := SelectieContract(lDesc);
  if lIdContract <> -1 then
    LoadContract(lIdContract);
end;

procedure TfrmContractEdit.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  DoCheckPostDataSet(qryContract);
  DoCheckPostDataSet(qryConsortiu);
  
    Action := caFree;
end;

procedure TfrmContractEdit.FormCreate(Sender: TObject);
begin
  FillImageCombo(edTipValuta.Properties, 'spNmclValute', 0, 1);
end;

procedure TfrmContractEdit.SetDetaliiValidare(AIsFinalizat: Boolean);
begin
 if AIsFinalizat then begin
    btnSave.Caption := 'Editare';
    btnSave.Tag := -1;
  end
  else begin
    btnSave.Caption := 'Salvare';
    btnSave.Tag := 0;
  end;
  lyContract.Enabled := not AIsFinalizat;
 
end;

procedure TfrmContractEdit.btnAddConsortiuClick(Sender: TObject);
begin
   if not qryConsortiu.Active then Exit;
   qryConsortiu.Append;
end;

procedure TfrmContractEdit.qryConsortiuNewRecord(DataSet: TDataSet);
begin
   DataSet.FieldByName('id_contracte').AsInteger := FIdContract;
end;

procedure TfrmContractEdit.btnDelConsortiuClick(Sender: TObject);
begin
  if MessageDlg('Doriti stergerea din consortiu a repartitorului selectat ?', mtConfirmation, [mbYes, mbNo], 0) = mrNo then
    Abort;
  qryConsortiu.Delete;
end;

end.
