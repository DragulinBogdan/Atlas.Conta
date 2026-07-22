unit ModificaContract;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, Menus, StdCtrls, ExtCtrls, DB,  Math,
  cxLookAndFeelPainters, cxGraphics, cxStyles, cxCustomData, cxFilter,
  cxData, cxDataStorage, cxEdit, cxDBData, cxDBLookupComboBox, cxCalc,
  cxDBEdit, cxDropDownEdit, cxGridLevel, cxGridCustomTableView,
  cxGridTableView, cxGridDBTableView, cxClasses, cxControls,
  cxGridCustomView, cxGrid, cxPC, cxMemo, cxLookupEdit, cxDBLookupEdit,
  cxCalendar, cxContainer, cxTextEdit, cxMaskEdit, cxSpinEdit, cxButtons,
  ZAbstractRODataset, ZAbstractDataset, ZDataset, cxButtonEdit, Buttons,
  cxGroupBox, cxCurrencyEdit, cxCheckBox, Grids, DBGrids, dxmdaset,
  cxLookAndFeels, Vcl.ComCtrls, dxCore, cxDateUtils, dxBarBuiltInMenu,
  cxNavigator, cxDataControllerConditionalFormattingRulesManagerDialog,
  dxDateRanges, dxScrollbarAnnotations;

type
  TfrmModificareContract = class(TForm)
    pnlBottom: TPanel;
    btnCancel: TcxButton;
    btnSave: TcxButton;
    pnlTop: TPanel;
    pcDetalii: TcxPageControl;
    tsOfertanti: TcxTabSheet;
    tsFiles: TcxTabSheet;
    qryContracteUSR: TZQuery;
    dsContracteUSR: TDataSource;
    qryContracteFilesUSR: TZQuery;
    dsContracteFilesUSR: TDataSource;
    gridContracteFilesDBTableView1: TcxGridDBTableView;
    gridContracteFilesLevel1: TcxGridLevel;
    gridContracteFiles: TcxGrid;
    btnAdd: TcxButton;
    btnDelete: TcxButton;
    btnViewFile: TcxButton;
    btnLoadFile: TcxButton;
    gridContracteFilesDBTableView1Denumire: TcxGridDBColumn;
    gridContracteFilesDBTableView1Descriere: TcxGridDBColumn;
    mnuFiles: TPopupMenu;
    miFilesView: TMenuItem;
    miFilesPrint: TMenuItem;
    pnlValoarePopup: TPanel;
    Label88: TLabel;
    Label89: TLabel;
    Label90: TLabel;
    Label91: TLabel;
    edtValoare: TcxDBCalcEdit;
    edtValoareEuro: TcxDBCalcEdit;
    edtCursEuro: TcxDBCalcEdit;
    edtCursEuroData: TcxDBDateEdit;
    btnCalcValoare: TcxButton;
    btnCalcValoareEuro: TcxButton;
    lbl2: TLabel;
    lbl4: TLabel;
    qryListaContracteP: TZQuery;
    dsListaContracteP: TDataSource;
    cxGroupBox1: TcxGroupBox;
    dtOrdinData: TcxDBDateEdit;
    txtOrdinNumar: TcxDBTextEdit;
    lbl6: TLabel;
    lbl7: TLabel;
    pnl1: TPanel;
    lbl10: TLabel;
    cbbAchizitii: TcxDBLookupComboBox;
    cxGroupBox2: TcxGroupBox;
    lbl8: TLabel;
    lbl9: TLabel;
    dtTerminareData: TcxDBDateEdit;
    txtTerminareNumar: TcxDBTextEdit;
    cxGroupBox4: TcxGroupBox;
    lbl17: TLabel;
    lbl18: TLabel;
    dtTermenFinalizare: TcxDBDateEdit;
    txtNumarReceptie: TcxDBTextEdit;
    lbl19: TLabel;
    lbl20: TLabel;
    lbl21: TLabel;
    dtDataGarantie: TcxDateEdit;
    crDurataGAni: TcxDBCurrencyEdit;
    crDurataGLuni: TcxDBCurrencyEdit;
    qryListaOfertanti: TZQuery;
    dsListaOfertanti: TDataSource;
    dsListaAdreseImobil: TDataSource;
    qryListaAdreseImobil: TZQuery;
    pmObiecteOfertanti: TPopupMenu;
    Adaugaofertant1: TMenuItem;
    Adaugaobiect1: TMenuItem;
    vwContracteOfertanti: TcxGridDBTableView;
    lvContracteOfertanti: TcxGridLevel;
    gridContracteOfertanti: TcxGrid;
    lvObiecteOfertanti: TcxGridLevel;
    vwObiecteOfertanti: TcxGridDBTableView;
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
    vwObiecteOfertantiidContracteOfertanti: TcxGridDBColumn;
    vwObiecteOfertantiidParinte: TcxGridDBColumn;
    vwObiecteOfertantiidInvContabilitate: TcxGridDBColumn;
    vwObiecteOfertantiidContracte: TcxGridDBColumn;
    vwObiecteOfertantiidFinantariPropuneri: TcxGridDBColumn;
    vwObiecteOfertantipretFaraTVALei: TcxGridDBColumn;
    vwObiecteOfertantipretFaraTVAEuro: TcxGridDBColumn;
    vwObiecteOfertantivaloareTVALei: TcxGridDBColumn;
    vwObiecteOfertantipretTVALei: TcxGridDBColumn;
    vwObiecteOfertanticursValutar: TcxGridDBColumn;
    vwObiecteOfertantidataCursValutar: TcxGridDBColumn;
    pnl3: TPanel;
    lbl1: TLabel;
    lbl3: TLabel;
    lbl12: TLabel;
    lbl22: TLabel;
    lbl5: TLabel;
    btnListaInvestConta: TSpeedButton;
    btn2: TSpeedButton;
    dtDataContract: TcxDBDateEdit;
    cbbTipContract: TcxDBLookupComboBox;
    cbbStareContract: TcxDBLookupComboBox;
    cbbDenInvestitie: TcxDBLookupComboBox;
    txtNrContract: TcxDBTextEdit;
    pnl2: TPanel;
    lbl14: TLabel;
    lbl15: TLabel;
    Adaugafisier1: TMenuItem;
    pnl4: TPanel;
    lbl24: TLabel;
    dtDataCursEuroG: TcxDBDateEdit;
    lbl25: TLabel;
    edtCursEuroG: TcxDBCalcEdit;
    lbl26: TLabel;
    cxstylrpstry1: TcxStyleRepository;
    cxstyl1: TcxStyle;
    cxstylrpstry2: TcxStyleRepository;
    cxstyl2: TcxStyle;
    cxstylrpstry3: TcxStyleRepository;
    cxstyl3: TcxStyle;
    cxstylrpstry4: TcxStyleRepository;
    cxstyl4: TcxStyle;
    cxstylrpstry5: TcxStyleRepository;
    cxstyl5: TcxStyle;
    cxstylrpstry6: TcxStyleRepository;
    cxstyl6: TcxStyle;
    cxstylrpstry7: TcxStyleRepository;
    cxstyl7: TcxStyle;
    lbl13: TLabel;
    crOrdinDurataAni: TcxDBCurrencyEdit;
    lbl16: TLabel;
    crOrdinDurataLuni: TcxDBCurrencyEdit;
    lbl27: TLabel;
    dtDataTerminareOrdin: TcxDateEdit;
    vwContracteOfertantiprocentGarantieDepusa: TcxGridDBColumn;
    vwContracteOfertantivaloareGarantieDepusa: TcxGridDBColumn;
    vwContracteOfertantiprocentGarantieRetinuta: TcxGridDBColumn;
    vwContracteOfertantivaloareGarantieRetinuta: TcxGridDBColumn;
    pnl5: TPanel;
    lbl23: TLabel;
    edtGarantieRetinuta: TcxDBCalcEdit;
    lbl28: TLabel;
    edtGarantieDepusa: TcxDBCalcEdit;
    lbl29: TLabel;
    lbl30: TLabel;
    qryTipuriFinantari: TZQuery;
    dsTipuriFinantari: TDataSource;
    vwContracteOfertanticontCurent: TcxGridDBColumn;
    pnlValoareCuTVA: TPanel;
    lbl31: TLabel;
    lbl33: TLabel;
    lbl34: TLabel;
    edtValoareCuTVA: TcxDBCalcEdit;
    edtCursVTVA: TcxDBCalcEdit;
    dtCursDataTVA: TcxDBDateEdit;
    qryObiecteOfertantiUSR: TZQuery;
    qryContracteOfertantiUSR: TZQuery;
    dsContracteOfertantiUSR: TDataSource;
    dsObiecteOfertantiUSR: TDataSource;
    cbbManProiectOfertant: TcxDBLookupComboBox;
    cbbManProiectBeneficiar: TcxDBLookupComboBox;
    Label1: TLabel;
    cxDBTextEdit1: TcxDBTextEdit;
    qryTipuriContracte: TZQuery;
    dsTipuriContracte: TDataSource;
    qryStariContracte: TZQuery;
    dsStariContracte: TDataSource;
    qryAchizitii: TZQuery;
    dsAchizitii: TDataSource;
    qryOfertanti: TZQuery;
    dsOfertanti: TDataSource;
    dsContracte: TDataSource;
    qryContracte: TZQuery;
    qryAditionale: TZQuery;
    dsAditionale: TDataSource;
    procedure SyncButtons;
    function LoadFile: Boolean;
    procedure FormCreate(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure btnSaveClick(Sender: TObject);
    procedure btnCancelClick(Sender: TObject);
    procedure btnDeleteClick(Sender: TObject);
    procedure btnLoadFileClick(Sender: TObject);
    procedure pcDetaliiChange(Sender: TObject);
    procedure miFilesViewClick(Sender: TObject);
    procedure miFilesPrintClick(Sender: TObject);
    procedure btnViewFileClick(Sender: TObject);
    procedure btnCalcValoareClick(Sender: TObject);
    procedure btnCalcValoareEuroClick(Sender: TObject);
    procedure btnListaInvestContaClick(Sender: TObject);
    procedure btn2Click(Sender: TObject);
    procedure cbbAchizitiiPropertiesEditValueChanged(Sender: TObject);
    procedure crDurataGAniPropertiesEditValueChanged(Sender: TObject);
    procedure crDurataGLuniPropertiesEditValueChanged(Sender: TObject);
    procedure dtTerminareDataPropertiesEditValueChanged(Sender: TObject);
    procedure Adaugaofertant1Click(Sender: TObject);
    procedure Adaugaobiect1Click(Sender: TObject);
    procedure vwContracteOfertantiInitEdit(Sender: TcxCustomGridTableView;
      AItem: TcxCustomGridTableItem; AEdit: TcxCustomEdit);
    procedure vwObiecteOfertantiInitEdit(Sender: TcxCustomGridTableView;
      AItem: TcxCustomGridTableItem; AEdit: TcxCustomEdit);
    procedure vwObiecteOfertantipretFaraTVALeiPropertiesCloseUp(
      Sender: TObject);
    procedure Adaugafisier1Click(Sender: TObject);
    procedure vwObiecteOfertantipretFaraTVALeiPropertiesInitPopup(
      Sender: TObject);
    procedure vwContracteOfertantiprocentGarantieRetinutaPropertiesCloseUp(
      Sender: TObject);
    procedure crOrdinDurataAniPropertiesEditValueChanged(Sender: TObject);
    procedure crOrdinDurataLuniPropertiesEditValueChanged(Sender: TObject);
    procedure dtOrdinDataPropertiesEditValueChanged(Sender: TObject);
    procedure vwContracteOfertantiliderAsocierePropertiesEditValueChanged(
      Sender: TObject);
    procedure vwObiecteOfertantivaloareTVALeiPropertiesInitPopup(
      Sender: TObject);
    procedure vwObiecteOfertantivaloareTVALeiPropertiesCloseUp(
      Sender: TObject);
    procedure qryObiecteOfertantiUSRAfterOpen(DataSet: TDataSet);
    procedure vwObiecteOfertantivaloareTVALeiPropertiesPopup(
      Sender: TObject);
    procedure vwContracteOfertantiMouseDown(Sender: TObject;
      Button: TMouseButton; Shift: TShiftState; X, Y: Integer);
  private
    FidContracte, FvwActiv: Integer;
    initial: TBookmark;

    procedure SetidContracte(const aidContracte: Integer);
    procedure SchimbaDataGarantie;
    procedure SchimbaDataGarantieOrdin;
    procedure ListaObiecteConta(Sender: TObject; AButtonIndex: Integer);
    procedure ProcMess1(var AMsg: TMessage); message WM_USER + 51;
    procedure ProcMess2(var AMsg: TMessage); message WM_USER + 52;
    procedure ProcMess3(var AMsg: TMessage); message WM_USER + 53;
  public
    FromMenu : boolean;
    class procedure MainExecuteAction(Sender : TObject);
    property idContracte: Integer read FidContracte write SetidContracte;

  end;

var
  Fsterge: Boolean;

procedure ModificareContract(aidContracte: Integer; IsFromMenu : boolean= False; Arhiva : boolean=False);
function StergereContract(aidContracte: Integer): Boolean;
function ArhivareContract(aidContracte: Integer): Boolean;

implementation

{$R *.dfm}

uses Utils, FormulareUnit, DateUtils,
     InvestOI_Proiecte, ListaContracteParinte, DateUnit, CommonDBVar,
     MInvestCommon
     ;

//------------------------------------------------------------------------------
procedure ModificareContract(aidContracte: Integer; IsFromMenu : boolean= False; Arhiva : boolean = False);
var
  lForm: TfrmModificareContract;
  qry: TZQuery;
begin
  lForm := TfrmModificareContract(GetNewForm(TfrmModificareContract));
  with lForm do
  begin
    btnAdd.Enabled := not Arhiva;
    btnDelete.Enabled := not Arhiva;
    btnSave.Enabled := not Arhiva;
    btnLoadFile.Enabled := not Arhiva;
    if Arhiva then
    begin
      //daca Arhiva are valoare true, se modifica linia doi, din query si se face selectie din tabele cu _ARH
      //altfel proprietatea SQL ramane cea initiala
      qryContracteUSR.SQL[2] := ' from Contracte_ARH ';
      qryContracteOfertantiUSR.SQl[2] := ' from ContracteOfertanti_ARH ';
      qryContracteFilesUSR.SQL[2] := ' from ContracteFiles_ARH ';
    end;
  end;
  lForm.idContracte := aidContracte;
  lForm.FromMenu := (IsFRomMenu) and (aidContracte = -1);

  //intoarcem departament utiliztor (beneficiar investitie)
  if aidContracte = -1 then
   begin
    qry := GetTmpMInvestQry;
     with qry do
     try
      SQL.Text := 'spIntoarceDepartament ' + IntToStr(idUtilizator);
      Open;
      if qry.RecordCount <> 0 then
       if lForm.qryContracteUSR.FieldByName('ManProiectBeneficiar').IsNull then
        begin
         lForm.qryContracteUSR.Edit;
         lForm.qryContracteUSR.FieldByName('ManProiectBeneficiar').Value := qry.Fields[0].Value;
        end;
     finally
      Free;
     end;

     //setari pe componente in functie de actiunea utiliztorului
     with lForm do
      begin
       qryContracteUSR.Edit;
       qryContracteUSR.FieldByName('NrContract').Value := '';
       qryContracteUSR.FieldByName('stare').Value := 0;
       qryContracteUSR.Post;
       cbbTipContract.Properties.ReadOnly := False;
       cbbStareContract.Properties.ReadOnly := False;
       txtOrdinNumar.Properties.ReadOnly := False;
       dtOrdinData.Properties.ReadOnly := False;
       crDurataGAni.Properties.ReadOnly := False;
       crDurataGLuni.Properties.ReadOnly := False;
       cbbManProiectBeneficiar.Properties.ReadOnly := False;
       cbbManProiectOfertant.Properties.ReadOnly := False;
       btnListaInvestConta.Enabled := True;
       btn2.Enabled := True;
      end;
   end;
end;
//------------------------------------------------------------------------------
function StergereContract(aidContracte: Integer): Boolean;
const
  lcTables: array[1..3] of String = ('ContracteOfertanti', 'ContracteFiles', 'Contracte');
var
  lQry: TZQuery;
  lidContracte: String;
  i, lidOperatiuni: Integer;
begin
  Result := False;
  if MessageDlg('Sigur doriti stergerea contractului ? ', mtCOnfirmation, [mbYes, mbNo], 0) = mrNo then
    Exit;

  //todo: verificare daca se poate sterge contractul
  lQry := GetTmpMInvestQry;
  try
    lidContracte := IntToStr(aidContracte);
    lidOperatiuni := BeginOp('Stergere contract ' + lidContracte);

    for i := Low(lcTables) to High(lcTables) do
     begin
      ArchiveRecord(lcTables[i], 'idContracte', aidContracte, lidOperatiuni);
     end;

    for i := Low(lcTables) to High(lcTables) do
     begin
      lQry.SQL.Text := 'delete from ' + lcTables[i] + ' where idContracte = ' + lidContracte;
      lQry.ExecSQL;
     end;

   EndOp(aidContracte);
   Result := True;
  finally
   lQry.Free;
  end;
end;
//------------------------------------------------------------------------------
procedure TfrmModificareContract.SetidContracte(const aidContracte: Integer);
begin
 //verificam daca avem document deschis
  if (FidContracte <> gcUnassigned) and ((FidContracte = gcNewRecord) or (FidContracte <> aidContracte)) then
  begin
    MessageDlg('Exista deja un document deschis!' + #13#10 +
      'Trebuie intai sa il inchideti pe acesta pentru a continua!', mtError, [mbOK], 0);
    Exit;
  end;

  if aidContracte = gcNewRecord then
  begin
   Caption := 'Adaugare contract';
   if Parent is TcxTabSheet then
    TcxTabSheet(Parent).Caption := Caption;
  end
  else
  begin
   //todo: verifica ce anume se poate modifica din contract
  end;

  // setam id-ul de contract ca parametru, si deschidem query-urile
  OpenQryWithParam(Self, 'idContracte', aidContracte);

  FidContracte := aidContracte;
end;
//------------------------------------------------------------------------------
procedure TfrmModificareContract.SyncButtons;
begin
  if pcDetalii.ActivePage = tsOfertanti then
  begin
   btnViewFile.Visible := False;
   btnLoadFile.Visible := False;
   Adaugafisier1.Visible := False;
   Adaugaofertant1.Visible := True;
   Adaugaobiect1.Visible := True;
  end
  else if pcDetalii.ActivePage = tsFiles then
  begin
   btnViewFile.Visible := True;
   btnLoadFile.Visible := True;
   Adaugaofertant1.Visible := False;
   Adaugaobiect1.Visible := False;
   Adaugafisier1.Visible := True;
  end;
end;
//------------------------------------------------------------------------------
function TfrmModificareContract.LoadFile: Boolean;
begin
  with qryContracteFilesUSR do
   begin
    Result := LoadDBFile(TBlobField(FieldByName('Document')), FieldByName('Denumire'));
   end;
end;
//------------------------------------------------------------------------------
procedure TfrmModificareContract.FormCreate(Sender: TObject);
begin
  ReplaceEmptyConnection(Self);
  FidContracte := gcUnassigned;
  modificaValGarantie := True;
  Fsterge := True;

  pcDetalii.ActivePage := tsOfertanti;
  SyncButtons;
  //deschidem seturi de date
  begin
    RefreshDataSet(qryTipuriContracte);
    RefreshDataSet(qryStariContracte);
    RefreshDataSet(qryAchizitii);
    RefreshDataSet(qryOfertanti);

    qryListaContracteP.close;
    qryListaContracteP.ParamByName('idUtilizator').Value := CommonDBVar.idUtilizator;
    qryListaContracteP.Open;

    //RefreshDataSet(qryListaContracteP);
    RefreshDataSet(qryListaOfertanti);
    RefreshDataSet(qryListaAdreseImobil);
    RefreshDataSet(qryTipuriFinantari);
  end;
  //qryContracteOfertantiUSR.Open;
  //qryObiecteOfertantiUSR.Open;
  vwObiecteOfertantiidInvContabilitate.Properties.OnButtonClick := ListaObiecteConta;

end;
//------------------------------------------------------------------------------
procedure TfrmModificareContract.FormClose(Sender: TObject;
  var Action: TCloseAction);
var
  lqry: TZQuery;
begin
  qryContracteUSR.DisableControls;
  qryContracteUSR.ShowRecordTypes  := [usModified,usInserted,usDeleted];
  //qryContracteUSR.Filtered := True;
 //verificam daca datele nu au fost salvate si afisam mesaj
  if qryContracteUSR.RecordCount > 0 then
  begin
   if MessageDlg('Datele NU au fost salvate ! Sigur doriti sa inchideti?', mtConfirmation, [mbOK, mbCancel], 0) = mrCancel then
    begin
     qryContracteUSR.ShowRecordTypes  := [usModified,usInserted,usDeleted, usUnmodified];
     qryContracteUSR.Filtered := False;
     qryContracteUSR.EnableControls;
     Abort;
    end;
  end;

 //actualizare set de date contracte
  begin
    qryContracte.Filtered := False;
    qryContracte.Close;
    qryContracte.SQL.Text := 'exec spListaContracte ' + IntToStr(CommonDBVar.idUtilizator);
    qryContracte.Open;
    qryAditionale.Filtered := False;
    qryAditionale.Close;
    qryAditionale.Open;
  end;

  Action := caFree;
//  FreeParent(Self);
end;
//------------------------------------------------------------------------------
procedure TfrmModificareContract.btnSaveClick(Sender: TObject);
var
  lidContracte, lidOperatiuni: Integer;
  lDenumire : string;
  lliderAsociere: Boolean;
begin
  //verificare campuri obligatorii
  if Trim(txtNrContract.Text) = '' then
   begin
    MessageDlg('Completati campul <Numar contract>!', mtInformation, [mbOK], 0);
    Exit;
   end;

  if Trim(dtDataContract.Text) = '' then
   begin
    MessageDlg('Completati campul <Data contract>!', mtInformation, [mbOK], 0);
    Exit;
   end;

  if Trim(cbbTipContract.Text) = '' then
   begin
    MessageDlg('Completati campul <Tip contract>!', mtInformation, [mbOK], 0);
    Exit;
   end;

  if Trim(cbbStareContract.Text) = '' then
   begin
    MessageDlg('Completati campul <Stare contract>!', mtInformation, [mbOK], 0);
    Exit;
   end;

//  if Trim(cbbManProiectBeneficiar.Text) = '' then
//   begin
//    MessageDlg('Completati campul <Manager proiect (beneficiar>!', mtInformation, [mbOK], 0);
//    Exit;
//   end;

  if qryContracteOfertantiUSR.IsEmpty then
   begin
    MessageDlg('Adaugati executantii!', mtInformation, [mbOK], 0);
    Exit;
   end;

  lliderAsociere := False;
  //verificam daca a fost selectat lider de asociere
  qryContracteOfertantiUSR.DisableControls;
  qryContracteOfertantiUSR.First;
  while not qryContracteOfertantiUSR.Eof do
   begin
    if qryContracteOfertantiUSR.FieldByName('liderAsociere').AsBoolean then
     lliderAsociere := True;
    qryContracteOfertantiUSR.Next;
   end;
  qryContracteOfertantiUSR.EnableControls;

  if not lliderAsociere then
   begin
    if MessageDlg('Nu ati selectat liderul de asociere. Doriti salvarea datelor?', mtConfirmation, [mbOK, mbCancel], 0) = mrCancel then
     Exit;
   end;

     qryObiecteOfertantiUSR.Edit;
  if qryObiecteOfertantiUSR.State in [dsEdit, dsInsert] then
    qryObiecteOfertantiUSR.Post;

  qryContracteOfertantiUSR.Edit;
  if qryContracteOfertantiUSR.State in [dsEdit, dsInsert] then
   qryContracteOfertantiUSR.Post;

  if idContracte = -1 then
   lidOperatiuni := BeginOp('Adaugare contract')
  else
    begin
     lidOperatiuni := BeginOp('Modificare contract ' + IntToStr(idContracte));

     //salvam versiunea anterioara a contractului
     ArchiveRecord('Contracte', 'idContracte', idContracte, lidOperatiuni);
     ArchiveRecord('ContracteOfertanti', 'idContracte', idContracte, lidOperatiuni);
     ArchiveRecord('ContracteFiles', 'idContracte', idContracte, lidOperatiuni);
    end;

  qryContracteUSR.Edit;
  qryContracteUSR.FieldByName('idOperatiuni').Value := lidOperatiuni;
  qryContracteUSR.FieldByName('stare').Value := 1;
  SaveDataSet(qryContracteUSR);

  //lidContracte := qryContracteUSR.FieldByName('idContracte').AsInteger;
  //lDenumire := cbbObiectConta.Text;   //qryContracteUSR.FieldByName('Denumire').AsString;

  //salvam seturile de date
  qryContracteOfertantiUSR.DisableControls;
  qryContracteOfertantiUSR.First;
  while not qryContracteOfertantiUSR.Eof do
  begin
    qryContracteOfertantiUSR.Edit;
    qryContracteOfertantiUSR.FieldByName('stare').AsInteger := 1;
    qryContracteOfertantiUSR.FieldByName('idContracte').Value :=   qryContracteUSR.FieldByName('idContracte').Value;
    qryContracteOfertantiUSR.Post;
    qryContracteOfertantiUSR.Next;
  end;
  try
    qryContracteOfertantiUSR.ApplyUpdates;
    qryContracteOfertantiUSR.CommitUpdates;
  except
    qryContracteOfertantiUSR.CancelUpdates;
  end;
  qryContracteOfertantiUSR.EnableControls;

  qryObiecteOfertantiUSR.DisableControls;
  qryObiecteOfertantiUSR.First;
  while not qryObiecteOfertantiUSR.Eof do
  begin
    qryObiecteOfertantiUSR.Edit;
    qryObiecteOfertantiUSR.FieldByName('stare').AsInteger := 1;
    qryObiecteOfertantiUSR.FieldByName('idContracte').Value :=   qryContracteUSR.FieldByName('idContracte').Value;
    qryObiecteOfertantiUSR.Post;
    qryObiecteOfertantiUSR.Next;
  end;
  try
    qryObiecteOfertantiUSR.ApplyUpdates;
    qryObiecteOfertantiUSR.CommitUpdates;
  except
    qryObiecteOfertantiUSR.CancelUpdates;
  end;
  qryObiecteOfertantiUSR.EnableControls;

  if qryContracteFilesUSR.State in [dsEdit, dsInsert] then
    qryContracteFilesUSR.Post;
 { qryContracteFilesUSR.DisableControls;
  qryContracteFilesUSR.First;
  while not qryContracteFilesUSR.Eof do
  begin
    qryContracteFilesUSR.Edit;
    qryContracteFilesUSR.FieldByName('idContracte').AsInteger := lidContracte;
    qryContracteFilesUSR.Post;
    qryContracteFilesUSR.Next;
  end;
  qryContracteFilesUSR.UpdateBatch;  }

  begin
    EndOp(lidOperatiuni);

    if qryContracte.Active then
      RefreshDataSet(qryContracte);
  end;
  if FromMenu then
  begin
      InsertInArbore('C', lIdContracte, 11,  lDenumire);
  end;
  qryContracteOfertantiUSR.EnableControls;
  qryObiecteOfertantiUSR.EnableControls;
  qryContracteFilesUSR.EnableControls;

  Fsterge := False;
  MessageDlg('Datele au fost salvate !', mtInformation, [mbOK], 0);
end;
//------------------------------------------------------------------------------
procedure TfrmModificareContract.btnCancelClick(Sender: TObject);
begin
  Close;
end;
//------------------------------------------------------------------------------
procedure TfrmModificareContract.btnDeleteClick(Sender: TObject);
begin
  if pcDetalii.ActivePage = tsOfertanti then
  begin
    if FvwActiv = 1 then
    if not qryContracteOfertantiUSR.IsEmpty then
     qryContracteOfertantiUSR.Delete;

    if FvwActiv = 2 then
    if not qryObiecteOfertantiUSR.IsEmpty then
     qryObiecteOfertantiUSR.Delete;
  end
  else if pcDetalii.ActivePage = tsFiles then
  begin
    if not qryContracteFilesUSR.IsEmpty then
      qryContracteFilesUSR.Delete;
  end;
end;
//------------------------------------------------------------------------------
procedure TfrmModificareContract.btnLoadFileClick(Sender: TObject);
begin
  LoadFile;
end;
//------------------------------------------------------------------------------
procedure TfrmModificareContract.pcDetaliiChange(Sender: TObject);
begin
  SyncButtons;
end;
//------------------------------------------------------------------------------
procedure TfrmModificareContract.miFilesViewClick(Sender: TObject);
begin
//deschidem doumente existente in baza dedate
 with qryContracteFilesUSR do
  begin
   if not IsEmpty then
    begin
     OpenTmpDBFile(GetAppTempFolder, FieldByName('Denumire').AsString, TBlobField(FieldByName('Document')));
    end;
  end;
end;
//------------------------------------------------------------------------------
procedure TfrmModificareContract.miFilesPrintClick(Sender: TObject);
begin
  //deschidem doumente existente in baza dedate
  with qryContracteFilesUSR do
  begin
   if not IsEmpty then
    begin
      OpenTmpDBFile(GetAppTempFolder, FieldByName('Denumire').AsString, TBlobField(FieldByName('Document')), 'print');
    end;
  end;
end;
//------------------------------------------------------------------------------
procedure TfrmModificareContract.btnViewFileClick(Sender: TObject);
begin
  miFilesView.Click;
end;
//------------------------------------------------------------------------------
procedure TfrmModificareContract.btnCalcValoareClick(Sender: TObject);
begin
 if edtCursEuro.Value <> 0 then
  begin
    edtValoare.EditValue := RoundTo(edtValoareEuro.Value * edtCursEuro.Value, -2);
    edtValoare.PostEditValue;
  end;
end;
//------------------------------------------------------------------------------
procedure TfrmModificareContract.btnCalcValoareEuroClick(Sender: TObject);
begin
 if edtCursEuro.Value <> 0 then
  begin
   edtValoareEuro.EditValue := RoundTo(edtValoare.Value / edtCursEuro.Value, -2);
   edtValoareEuro.PostEditValue;
  end;
end;
//------------------------------------------------------------------------------
function ArhivareContract(aidContracte: Integer): Boolean;
const
  lcTables: array[1..3] of String = ('ContracteOfertanti', 'ContracteFiles', 'Contracte');
var
  lQry: TZQuery;
  lidContracte: String;
  i, lidOperatiuni: Integer;
begin
//arhivare contracte - adaugare in tabele _ARH
  lQry := GetTmpMInvestQry;
  try
    lidContracte := IntToStr(aidContracte);
    lidOperatiuni := BeginOp('Arhivare contract ' + lidContracte);

   for i := Low(lcTables) to High(lcTables) do
    begin
      ArhiveazaInregistrare(lcTables[i], 'idContracte', aidContracte, lidOperatiuni);
    end;
    EndOp(aidContracte);
    Result := True;
  finally
   lQry.Free;
  end;
end;

procedure TfrmModificareContract.btnListaInvestContaClick(Sender: TObject);
var
  qry: TZQuery;
begin
 with TfrmListaContracteP.Create(Self) do
  try
   contractAditional := False;
   ShowModal;
  finally
    //selectam contractul parinte si atribuim valori variabilelor din "Utils"
   if contractAditional then
    begin
     Self.qryContracteUSR.Edit;
     if not varisnull(idSelectat) then
     Self.qryContracteUSR.FieldByName('idParinte').Value := idSelectat;
     if not varisnull(idTipuriContracte) then
     Self.qryContracteUSR.FieldByName('idTipuriContracte').Value := idTipuriContracte;
     if not varisnull(idStariContracte) then
     Self.qryContracteUSR.FieldByName('idStariContracte').Value := idStariContracte;
     if not varisnull(DurataGarantieAni) then
     Self.qryContracteUSR.FieldByName('DurataGarantieAni').Value := DurataGarantieAni;
     if not varisnull(DurataGarantieLuni) then
     Self.qryContracteUSR.FieldByName('DurataGarantieLuni').Value := DurataGarantieLuni;
     if not varisnull(DataOrdinIncepere) then
     Self.qryContracteUSR.FieldByName('DataOrdinIncepere').Value := DataOrdinIncepere;
     if not varisnull(NrOrdinIncepere) then
     Self.qryContracteUSR.FieldByName('NrOrdinIncepere').Value := NrOrdinIncepere;
     if not varisnull(DataPVTerminare) then
     Self.qryContracteUSR.FieldByName('DataPVTerminare').Value := DataPVTerminare;
     if not varisnull(NrPVTerminare) then
     Self.qryContracteUSR.FieldByName('NrPVTerminare').Value := NrPVTerminare;
     if not varisnull(NumarPVReceptie) then
     Self.qryContracteUSR.FieldByName('NumarPVReceptie').Value := NumarPVReceptie;
     if not varisnull(DataPVReceptie) then
     Self.qryContracteUSR.FieldByName('DataPVReceptie').Value := DataPVReceptie;
     if not varisnull(ManProiectBeneficiar) then
     Self.qryContracteUSR.FieldByName('ManProiectBeneficiar').Value := ManProiectBeneficiar;
     if not varisnull(ManProiectOfertant) then
     Self.qryContracteUSR.FieldByName('ManProiectOfertant').Value := ManProiectOfertant;
     if not varisnull(DurataOrdinAni) then
     Self.qryContracteUSR.FieldByName('DurataOrdinAni').Value := DurataOrdinAni;
     if not varisnull(DurataOrdinLuni) then
     Self.qryContracteUSR.FieldByName('DurataOrdinLuni').Value := DurataOrdinLuni;
     if not varisnull(ProcentGarantieRetinuta) then
     Self.qryContracteUSR.FieldByName('ProcentGarantieRetinuta').Value := ProcentGarantieRetinuta;
     if not varisnull(ProcentGarantieDepusa) then
     Self.qryContracteUSR.FieldByName('ProcentGarantieDepusa').Value := ProcentGarantieDepusa;
     if not varisnull(CursEuro) then
     Self.qryContracteUSR.FieldByName('CursEuro').Value := CursEuro;
     if not varisnull(CursEuroData) then
     Self.qryContracteUSR.FieldByName('CursEuroData').Value := CursEuroData;
     //if (not varisnull(ProcentGarantieDepusa)) and (not varisnull(Valoare)) then
     //SchimbaDataGarantie;

     //selectam executantii si obiectele din contract si le copiem la noul contract
     qry := GetTmpMInvestQry;
     qry.SQL.Text := 'exec spAdaugaObiecteAditional ' + VarToStr(idSelectat) + ', ' +
                       qryContracteUSR.FieldByName('idContracte').AsString;
     qry.ExecSQL;
     qryContracteOfertantiUSR.Close;
     qryContracteOfertantiUSR.ParamByName('idContracte').Value := qryContracteUSR.FieldByName('idContracte').AsString;
     qryContracteOfertantiUSR.Open;
     qryObiecteOfertantiUSR.Close;
     qryObiecteOfertantiUSR.ParamByName('idContracte').Value := qryContracteUSR.FieldByName('idContracte').AsString;
     qryObiecteOfertantiUSR.Open;
     qry.Free;

     cbbTipContract.Properties.ReadOnly := True;
     cbbStareContract.Properties.ReadOnly := True;
     txtOrdinNumar.Properties.ReadOnly := True;
     dtOrdinData.Properties.ReadOnly := True;
     crDurataGAni.Properties.ReadOnly := True;
     crDurataGLuni.Properties.ReadOnly := True;
     cbbManProiectBeneficiar.Properties.ReadOnly := True;
     cbbManProiectOfertant.Properties.ReadOnly := True;
    end;
   Free;
  end;
end;

procedure TfrmModificareContract.btn2Click(Sender: TObject);
var
  qry: TZQuery;
begin
//resetare valori campuri
 qryContracteUSR.Edit;
 qryContracteUSR.FieldByName('idParinte').Value := 0;
 qryContracteUSR.FieldByName('idTipuriContracte').Value := 0;
 qryContracteUSR.FieldByName('idStariContracte').Value := 0;
 qryContracteUSR.FieldByName('ManProiectOfertant').Value := 0;
 qryContracteUSR.FieldByName('ManProiectBeneficiar').Value := 0;
 qryContracteUSR.FieldByName('DurataOrdinAni').Value := null;
 qryContracteUSR.FieldByName('DurataOrdinLuni').Value := null;
 qryContracteUSR.FieldByName('ProcentGarantieRetinuta').Value := null;
 qryContracteUSR.FieldByName('ProcentGarantieDepusa').Value := null;
 qryContracteUSR.FieldByName('DataOrdinIncepere').Value := null;
 qryContracteUSR.FieldByName('NrOrdinIncepere').Value := null;
 qryContracteUSR.FieldByName('DataPVTerminare').Value := null;
 qryContracteUSR.FieldByName('NrPVTerminare').Value := null;
 qryContracteUSR.FieldByName('NumarPVReceptie').Value := null;
 qryContracteUSR.FieldByName('DataPVReceptie').Value := null;
 qryContracteUSR.FieldByName('CursEuro').Value := null;
 qryContracteUSR.FieldByName('CursEuroData').Value := null;
 dtDataGarantie.EditValue := null;

 //setare stare = 0 la inregistrarile copiate pe noul contract aditional
 qry := GetTmpMInvestQry;
 qry.SQL.Text := 'exec spEliminaObiecteAditional ' + qryContracteUSR.FieldByName('idContracte').AsString;
 qry.ExecSQL;
 qryContracteOfertantiUSR.Close;
 qryContracteOfertantiUSR.ParamByName('idContracte').Value := qryContracteUSR.FieldByName('idContracte').AsString;
 qryContracteOfertantiUSR.Open;
 qryObiecteOfertantiUSR.Close;
 qryObiecteOfertantiUSR.ParamByName('idContracte').Value := qryContracteUSR.FieldByName('idContracte').AsString;
 qryObiecteOfertantiUSR.Open;
 qry.Free;

 cbbTipContract.Properties.ReadOnly := False;
 cbbStareContract.Properties.ReadOnly := False;
 txtOrdinNumar.Properties.ReadOnly := False;
 dtOrdinData.Properties.ReadOnly := False;
 crDurataGAni.Properties.ReadOnly := False;
 crDurataGLuni.Properties.ReadOnly := False;
 cbbManProiectBeneficiar.Properties.ReadOnly := False;
 cbbManProiectOfertant.Properties.ReadOnly := False;
 modificaValGarantie := True;
end;

procedure TfrmModificareContract.cbbAchizitiiPropertiesEditValueChanged(
  Sender: TObject);
{var
  lQry: TZQuery;
  lTmpDate: TDateTime;}
begin

 {  lQry := TADOQuery.Create(Self);
  try
    lQry.Connection := dtmMain.dbConnection;
    lQry.SQL.Text := 'select * from Achizitii where idAchizitii = :idAchizitii';
    lQry.Parameters.ParamByName('idAchizitii').Value := cbbAchizitii.EditValue;
    lQry.Open;
    lQry.First;

    qryContracteUSR.Edit;

    if qryContracteUSR.FieldByName('idTipuriContracte').IsNull then
      qryContracteUSR.FieldByName('idTipuriContracte').AsInteger :=
        lQry.FieldByName('idTipuriContracte').AsInteger;

    if qryContracteUSR.FieldByName('DataOrdinIncepere').IsNull and
      not lQry.FieldByName('DurataStart').IsNull then
    begin
      qryContracteUSR.FieldByName('DataStart').AsDateTime :=
        lQry.FieldByName('DurataStart').AsDateTime;

      if qryContracteUSR.FieldByName('DataPVTerminare').IsNull then
      begin
        lTmpDate := lQry.FieldByName('DurataStart').AsDateTime;
        if not lQry.FieldByName('DurataAni').IsNull then
          lTmpDate := IncYear(lTmpDate, lQry.FieldByName('DurataAni').AsInteger);
        if not lQry.FieldByName('DurataLuni').IsNull then
          lTmpDate := IncMonth(lTmpDate, lQry.FieldByName('DurataLuni').AsInteger);
        if not lQry.FieldByName('DurataZile').IsNull then
          lTmpDate := IncDay(lTmpDate, lQry.FieldByName('DurataZile').AsInteger);

        if lTmpDate <> lQry.FieldByName('DurataStart').AsDateTime then
          qryContracteUSR.FieldByName('DataPVTerminare').AsDateTime := lTmpDate;
      end;
    end;

    if qryContracteUSR.State in [dsInsert, dsEdit] then
      qryContracteUSR.Post;

    lQry.Close;
  finally
    lQry.Free;
  end;      }
end;

procedure TfrmModificareContract.SchimbaDataGarantie;
begin
  //calculam data garantie in functie de ani si/sau luni introduse
 if (not VarIsNull(dtTerminareData.EditValue)) and
    (not VarIsNull(crDurataGAni.EditValue) or not VarIsNull(crDurataGLuni.EditValue)) then
  begin
   if (not VarIsNull(crDurataGAni.EditValue)) and (not VarIsNull(crDurataGLuni.EditValue)) then
    dtDataGarantie.EditValue := IncMonth(IncYear(dtTerminareData.EditValue, crDurataGAni.EditValue), crDurataGLuni.EditValue)
   else
    begin
     if not VarIsNull(crDurataGAni.EditValue) then
      dtDataGarantie.EditValue := IncYear(dtTerminareData.EditValue, crDurataGAni.EditValue);

     if not VarIsNull(crDurataGLuni.EditValue) then
      dtDataGarantie.EditValue := IncMonth(dtTerminareData.EditValue, crDurataGLuni.EditValue);
    end;
  end;
end;

procedure TfrmModificareContract.crDurataGAniPropertiesEditValueChanged(
  Sender: TObject);
begin
 SchimbaDataGarantie;
end;

procedure TfrmModificareContract.crDurataGLuniPropertiesEditValueChanged(
  Sender: TObject);
begin
 SchimbaDataGarantie;
end;

procedure TfrmModificareContract.dtTerminareDataPropertiesEditValueChanged(
  Sender: TObject);
begin
 SchimbaDataGarantie;
end;

procedure TfrmModificareContract.ListaObiecteConta(Sender: TObject;
  AButtonIndex: Integer);
begin
 with TfrmOIProiecteInvest.Create(Self) do
  try
   seteazaInv := False;
   ShowModal;
  finally
   if seteazaInv then
   qryObiecteOfertantiUSR.FieldByName('idInvContabilitate').AsInteger := idSelectat;
   Free;
  end;
end;

procedure TfrmModificareContract.Adaugaofertant1Click(Sender: TObject);
begin
//adugam ofertanti
  if pcDetalii.ActivePage = tsOfertanti then
  begin
    qryContracteOfertantiUSR.Append;
    qryContracteOfertantiUSR.FieldByName('liderAsociere').Value := False;
    qryContracteOfertantiUSR.FieldByName('idContracte').Value :=
     qryContracteUSR.FieldByName('idContracte').Value;
    qryContracteOfertantiUSR.FieldByName('stare').Value := 0;
    qryContracteOfertantiUSR.FieldByName('procentGarantieDepusa').AsCurrency :=
    qryContracteUSR.FieldByName('ProcentGarantieDepusa').AsCurrency;
    qryContracteOfertantiUSR.FieldByName('procentGarantieRetinuta').AsCurrency :=
    qryContracteUSR.FieldByName('ProcentGarantieRetinuta').AsCurrency;
    if qryContracteOfertantiUSR.State in [dsEdit, dsInsert] then
     qryContracteOfertantiUSR.Post;
   // gridContracteOfertanti.SetFocus;
  end
  else if pcDetalii.ActivePage = tsFiles then
  begin
    qryContracteFilesUSR.Append;
    if not LoadFile then
      if qryContracteFilesUSR.State = dsInsert then
        qryContracteFilesUSR.Delete;

    gridContracteFiles.SetFocus;
  end;
end;

procedure TfrmModificareContract.Adaugaobiect1Click(Sender: TObject);
var
  i: Integer;
begin
 //verificam daca avem adaugati ofertanti
 if qryContracteOfertantiUSR.IsEmpty then
  begin
   ShowMessage('Adaugati executantul!');
   Exit;
  end;

  with TfrmOIProiecteInvest.Create(Self) do
  try
   seteazaInv := False;
   ShowModal;
  finally

   if seteazaInv then
    begin
     if not selectareObiecte then
      begin
        //adaugam obiecte
       qryObiecteOfertantiUSR.Append;
       qryObiecteOfertantiUSR.FieldByName('idInvContabilitate').AsInteger := idSelectat;
       qryObiecteOfertantiUSR.FieldByName('idParinte').AsInteger :=
        qryContracteOfertantiUSR.FieldByName('idContracteOfertanti').AsInteger;
       qryObiecteOfertantiUSR.FieldByName('idContracte').Value :=
        qryContracteUSR.FieldByName('idContracte').Value;
       qryObiecteOfertantiUSR.FieldByName('stare').Value := 0;
       if qryObiecteOfertantiUSR.State in [dsEdit, dsInsert] then
        qryObiecteOfertantiUSR.Post;
      end
     else
      begin
       for i := 0 to vwObiecte.DataController.RowCount - 1 do
        begin
          //adaugam obiecte 
         qryObiecteOfertantiUSR.Append;
         qryObiecteOfertantiUSR.FieldByName('idInvContabilitate').AsInteger :=
         vwObiecte.DataController.GetValue(i, 0);
         qryObiecteOfertantiUSR.FieldByName('idParinte').AsInteger :=
          qryContracteOfertantiUSR.FieldByName('idContracteOfertanti').AsInteger;
         qryObiecteOfertantiUSR.FieldByName('idContracte').Value :=
          qryContracteUSR.FieldByName('idContracte').Value;
         qryObiecteOfertantiUSR.FieldByName('stare').Value := 0;
         if qryObiecteOfertantiUSR.State in [dsEdit, dsInsert] then
          qryObiecteOfertantiUSR.Post;
        end;
      end;
    end;
   Free;
  end;
end;

procedure TfrmModificareContract.vwContracteOfertantiInitEdit(
  Sender: TcxCustomGridTableView; AItem: TcxCustomGridTableItem;
  AEdit: TcxCustomEdit);
begin
 qryContracteOfertantiUSR.Edit;
 FvwActiv := 1;
 initial := qryContracteOfertantiUSR.GetBookmark;
end;

procedure TfrmModificareContract.vwObiecteOfertantiInitEdit(
  Sender: TcxCustomGridTableView; AItem: TcxCustomGridTableItem;
  AEdit: TcxCustomEdit);
begin
 qryObiecteOfertantiUSR.Edit;
 FvwActiv := 2;
end;

procedure TfrmModificareContract.vwObiecteOfertantipretFaraTVALeiPropertiesCloseUp(
  Sender: TObject);
begin
   //PostMessage(Self.Handle, WM_USER + 52, 0, 0);
end;

procedure TfrmModificareContract.Adaugafisier1Click(Sender: TObject);
begin
  if pcDetalii.ActivePage = tsOfertanti then
  begin
  with TfrmOIProiecteInvest.Create(Self) do
  try
   seteazaInv := False;
   ShowModal;
  finally
   if seteazaInv then
    begin
     //adaugam documente
     qryObiecteOfertantiUSR.Append;
     qryObiecteOfertantiUSR.FieldByName('idInvContabilitate').AsInteger := idSelectat;
     qryObiecteOfertantiUSR.FieldByName('idParinte').AsInteger :=
     qryContracteOfertantiUSR.FieldByName('idContracteOfertanti').AsInteger;
     qryObiecteOfertantiUSR.FieldByName('idContracte').Value :=
      qryContracteUSR.FieldByName('idContracte').Value;
     qryObiecteOfertantiUSR.FieldByName('stare').Value := 0;
     //qryObiecteOfertantiUSR.FieldByName('idDelphi').Value := FidDelphi;
     //qryObiecteOfertantiUSR.FieldByName('idUtilizator').Value := Utils.idUtilizator;
     if qryObiecteOfertantiUSR.State in [dsEdit, dsInsert] then
      qryObiecteOfertantiUSR.Post;
      //qryObiecteOfertantiUSR.Refresh;
     //gridContracteOfertanti.SetFocus;
    end;
   Free;
  end;
  end
  else if pcDetalii.ActivePage = tsFiles then
  begin
    qryContracteFilesUSR.Append;
    qryContracteFilesUSR.FieldByName('idContracte').Value :=
     qryContracteUSR.FieldByName('idContracte').Value;
    qryContracteFilesUSR.FieldByName('stare').Value := 0;
    if not LoadFile then
      if qryContracteFilesUSR.State = dsInsert then
        qryContracteFilesUSR.Delete;

    gridContracteFiles.SetFocus;
  end;  
end;

procedure TfrmModificareContract.vwObiecteOfertantipretFaraTVALeiPropertiesInitPopup(
  Sender: TObject);
begin
 {if not qryContracteUSR.FieldByName('CursEuro').IsNull then
  if qryObiecteOfertantiUSR.FieldByName('cursValutar').IsNull then
   qryObiecteOfertantiUSR.FieldByName('cursValutar').AsCurrency :=
   qryContracteUSR.FieldByName('CursEuro').AsCurrency;

 if not qryContracteUSR.FieldByName('CursEuroData').IsNull then
  if qryObiecteOfertantiUSR.FieldByName('dataCursValutar').IsNull then
   qryObiecteOfertantiUSR.FieldByName('dataCursValutar').AsCurrency :=
   qryContracteUSR.FieldByName('CursEuroData').AsCurrency;     }
end;

procedure TfrmModificareContract.ProcMess1(var AMsg: TMessage);
begin
 //calculam valoare garantie retinuta
 qryContracteOfertantiUSR.FieldByName('valoareGarantieRetinuta').AsCurrency :=
 qryContracteOfertantiUSR.FieldByName('pretFaraTVALei').AsCurrency *
 (qryContracteOfertantiUSR.FieldByName('procentGarantieRetinuta').AsCurrency / 100);
end;

procedure TfrmModificareContract.vwContracteOfertantiprocentGarantieRetinutaPropertiesCloseUp(
  Sender: TObject);
begin
 PostMessage(Self.Handle, WM_USER + 51, 0, 0);
end;

procedure TfrmModificareContract.ProcMess2(var AMsg: TMessage);
var
  idContract: Integer;
  faraTVAlei, faraTVAeuro, TVA, cuTVAlei: Currency;
begin
//calcul sume contract
 qryObiecteOfertantiUSR.Edit;
 qryObiecteOfertantiUSR.FieldByName('pretTVALei').AsCurrency :=
 RoundTo(edtValoare.Value * (24/100), -2);
 qryObiecteOfertantiUSR.FieldByName('valoareTVALei').AsCurrency :=
 RoundTo(edtValoare.Value + (edtValoare.Value * (24/100)), -2);
 qryObiecteOfertantiUSR.Post;

 faraTVAlei := 0;
 faraTVAeuro := 0;
 TVA := 0;
 cuTVAlei := 0;
 idContract := qryContracteOfertantiUSR.FieldByName('idContracteOfertanti').AsInteger;

 qryObiecteOfertantiUSR.DisableControls;
 qryObiecteOfertantiUSR.First;
 while not qryObiecteOfertantiUSR.Eof do
  begin
   if qryObiecteOfertantiUSR.FieldByName('idParinte').AsInteger = idContract then
   begin
    faraTVAlei := faraTVAlei + qryObiecteOfertantiUSR.FieldByName('pretFaraTVALei').AsCurrency;
    faraTVAeuro := faraTVAeuro + qryObiecteOfertantiUSR.FieldByName('pretFaraTVAEuro').AsCurrency;
    TVA := TVA + qryObiecteOfertantiUSR.FieldByName('pretTVALei').AsCurrency;
    cuTVAlei := cuTVAlei + qryObiecteOfertantiUSR.FieldByName('valoareTVALei').AsCurrency;
   end;
    qryObiecteOfertantiUSR.Next;
  end;
  //qryContracteOfertantiUSR.UpdateBatch;
  qryObiecteOfertantiUSR.EnableControls;

  qryContracteOfertantiUSR.Edit;
  qryContracteOfertantiUSR.FieldByName('pretFaraTVALei').AsCurrency := faraTVAlei;
  qryContracteOfertantiUSR.FieldByName('pretFaraTVAEuro').AsCurrency := faraTVAeuro;
  qryContracteOfertantiUSR.FieldByName('pretTVALei').AsCurrency := TVA;
  qryContracteOfertantiUSR.FieldByName('valoareTVALei').AsCurrency := cuTVAlei;

  qryContracteOfertantiUSR.FieldByName('valoareGarantieRetinuta').AsCurrency :=
  qryContracteOfertantiUSR.FieldByName('pretFaraTVALei').AsCurrency *
  (qryContracteOfertantiUSR.FieldByName('procentGarantieRetinuta').AsInteger / 100);
  qryContracteOfertantiUSR.Post;

end;

procedure TfrmModificareContract.SchimbaDataGarantieOrdin;
begin
  if (not VarIsNull(dtOrdinData.EditValue)) and
    (not VarIsNull(crOrdinDurataAni.EditValue) or not VarIsNull(crOrdinDurataLuni.EditValue)) then
  begin
   if (not VarIsNull(crOrdinDurataAni.EditValue)) and (not VarIsNull(crOrdinDurataLuni.EditValue)) then
    dtDataTerminareOrdin.EditValue := IncMonth(IncYear(dtOrdinData.EditValue, crOrdinDurataAni.EditValue), crOrdinDurataLuni.EditValue)
   else
    begin
     if not VarIsNull(crOrdinDurataAni.EditValue) then
      dtDataTerminareOrdin.EditValue := IncYear(dtOrdinData.EditValue, crOrdinDurataAni.EditValue);

     if not VarIsNull(crOrdinDurataLuni.EditValue) then
      dtDataTerminareOrdin.EditValue := IncMonth(dtOrdinData.EditValue, crOrdinDurataLuni.EditValue);
    end;
  end;
end;

procedure TfrmModificareContract.crOrdinDurataAniPropertiesEditValueChanged(
  Sender: TObject);
begin
 SchimbaDataGarantieOrdin;
end;

procedure TfrmModificareContract.crOrdinDurataLuniPropertiesEditValueChanged(
  Sender: TObject);
begin
 SchimbaDataGarantieOrdin;
end;

procedure TfrmModificareContract.dtOrdinDataPropertiesEditValueChanged(
  Sender: TObject);
begin
 SchimbaDataGarantieOrdin;
end;

procedure TfrmModificareContract.vwContracteOfertantiliderAsocierePropertiesEditValueChanged(
  Sender: TObject);
begin
 if qryContracteOfertantiUSR.RecordCount = 1 then Exit;
 qryContracteOfertantiUSR.DisableControls;
 qryContracteOfertantiUSR.First;
 while not qryContracteOfertantiUSR.Eof do
  begin
   qryContracteOfertantiUSR.Edit;
   qryContracteOfertantiUSR.FieldByName('liderAsociere').AsBoolean := False;
   qryContracteOfertantiUSR.Next;
  end;
  qryContracteOfertantiUSR.GotoBookmark(initial);
  qryContracteOfertantiUSR.EnableControls;
  qryContracteOfertantiUSR.Edit;
  qryContracteOfertantiUSR.FieldByName('liderAsociere').AsBoolean := True;
end;

procedure TfrmModificareContract.vwObiecteOfertantivaloareTVALeiPropertiesInitPopup(
  Sender: TObject);
begin
 if not qryContracteUSR.FieldByName('CursEuro').IsNull then
  if qryObiecteOfertantiUSR.FieldByName('cursValutar').IsNull then
   qryObiecteOfertantiUSR.FieldByName('cursValutar').AsCurrency :=
   qryContracteUSR.FieldByName('CursEuro').AsCurrency;

 if not qryContracteUSR.FieldByName('CursEuroData').IsNull then
  if qryObiecteOfertantiUSR.FieldByName('dataCursValutar').IsNull then
   qryObiecteOfertantiUSR.FieldByName('dataCursValutar').AsCurrency :=
   qryContracteUSR.FieldByName('CursEuroData').AsCurrency;
end;

procedure TfrmModificareContract.ProcMess3(var AMsg: TMessage);
var
  idContract: Integer;
  faraTVAlei, faraTVAeuro, TVA, cuTVAlei: Currency;
begin
 qryObiecteOfertantiUSR.GotoBookmark(initial);

 qryObiecteOfertantiUSR.Edit;
 qryObiecteOfertantiUSR.FieldByName('pretFaraTVALei').AsCurrency :=
 RoundTo(edtValoareCuTVA.Value / 1.24, -2);

 if IsNull(edtCursEuro.Value, 0) <> 0 then
  qryObiecteOfertantiUSR.FieldByName('pretFaraTVAEuro').AsCurrency :=
  RoundTo((edtValoareCuTVA.Value / 1.24) / edtCursEuro.Value, -2);

 if IsNull(edtValoareCuTVA.Value, 0) then
  qryObiecteOfertantiUSR.FieldByName('pretTVALei').AsCurrency :=
  RoundTo((edtValoareCuTVA.Value / 1.24) * 0.24, -2);

 //qryObiecteOfertantiUSR.FieldByName('valoareTVALei').AsCurrency :=
 //RoundTo(edtValoare.Value + (edtValoare.Value * (24/100)), -2);
 //qryObiecteOfertantiUSR.Post;

 faraTVAlei := 0;
 faraTVAeuro := 0;
 TVA := 0;
 cuTVAlei := 0;
 idContract := qryContracteOfertantiUSR.FieldByName('idContracteOfertanti').AsInteger;

 qryObiecteOfertantiUSR.DisableControls;
 qryObiecteOfertantiUSR.First;
 while not qryObiecteOfertantiUSR.Eof do
  begin
   if qryObiecteOfertantiUSR.FieldByName('idParinte').AsInteger = idContract then
   begin
    faraTVAlei := faraTVAlei + qryObiecteOfertantiUSR.FieldByName('pretFaraTVALei').AsCurrency;
    faraTVAeuro := faraTVAeuro + qryObiecteOfertantiUSR.FieldByName('pretFaraTVAEuro').AsCurrency;
    TVA := TVA + qryObiecteOfertantiUSR.FieldByName('pretTVALei').AsCurrency;
    cuTVAlei := cuTVAlei + qryObiecteOfertantiUSR.FieldByName('valoareTVALei').AsCurrency;
   end;
    qryObiecteOfertantiUSR.Next;
  end;
  //qryContracteOfertantiUSR.UpdateBatch;
  qryObiecteOfertantiUSR.GotoBookmark(initial);
  qryObiecteOfertantiUSR.EnableControls;

  qryContracteOfertantiUSR.Edit;
  qryContracteOfertantiUSR.FieldByName('pretFaraTVALei').AsCurrency := faraTVAlei;
  qryContracteOfertantiUSR.FieldByName('pretFaraTVAEuro').AsCurrency := faraTVAeuro;
  qryContracteOfertantiUSR.FieldByName('pretTVALei').AsCurrency := TVA;
  qryContracteOfertantiUSR.FieldByName('valoareTVALei').AsCurrency := cuTVAlei;

  qryContracteOfertantiUSR.FieldByName('valoareGarantieRetinuta').AsCurrency :=
  qryContracteOfertantiUSR.FieldByName('pretFaraTVALei').AsCurrency *
  (qryContracteOfertantiUSR.FieldByName('procentGarantieRetinuta').AsInteger / 100);
  qryContracteOfertantiUSR.Post;
end;

procedure TfrmModificareContract.vwObiecteOfertantivaloareTVALeiPropertiesCloseUp(
  Sender: TObject);
begin
 PostMessage(Self.Handle, WM_USER + 53, 0, 0);
end;

procedure TfrmModificareContract.qryObiecteOfertantiUSRAfterOpen(
  DataSet: TDataSet);
begin
 qryObiecteOfertantiUSR.SortedFields := 'idParinte';
end;

procedure TfrmModificareContract.vwObiecteOfertantivaloareTVALeiPropertiesPopup(
  Sender: TObject);
begin
 initial := qryObiecteOfertantiUSR.GetBookmark;
end;

procedure TfrmModificareContract.vwContracteOfertantiMouseDown(
  Sender: TObject; Button: TMouseButton; Shift: TShiftState; X,
  Y: Integer);
var
  HitTest: TcxCustomGridHitTest;
begin
 HitTest := (Sender as TcxGridSite).GridView.ViewInfo.GetHitTest(X, Y);
 if HitTest is TcxGridExpandButtonHitTest then
  if TcxGridExpandButtonHitTest(HitTest).GridRecord <> nil then
   TcxGridExpandButtonHitTest(HitTest).GridRecord.Focused := True;
end;


class procedure TfrmModificareContract.MainExecuteAction(Sender: TObject);
begin
  ModificareContract(-1, True);
end;

initialization
  RegisterMenuItem('Cmd_InvestAdaugaContract', 'MInvest', 'Adaugare contract', TfrmModificareContract.MainExecuteAction);
end.
