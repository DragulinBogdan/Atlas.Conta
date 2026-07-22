unit GenerareOPUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, ExtCtrls, dxCntner, dxTL, dxDBCtrl, dxDBGrid, Db, dxmdaset,
  ZDataSet, dxEdLib, dxExEdtr, dxEditor, dxDBTLCl, dxGrClms,
  unitMemTableEx, Buttons, dxDBTL, dxGrClEx, Menus, cxLookAndFeelPainters, cxButtons,
  ZAbstractRODataset, ZAbstractDataset, cxGraphics, cxLookAndFeels, cxControls,
  cxContainer, cxEdit, cxTextEdit, cxMaskEdit, cxDropDownEdit, cxImageComboBox,
  cxStyles, cxCustomData, cxFilter, cxData, cxDataStorage, cxNavigator,
  cxDBData, cxCalendar, cxCurrencyEdit, cxClasses, cxGridCustomTableView,
  cxGridTableView, cxGridDBTableView, cxGridLevel, cxGridCustomView, cxGrid,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxDateRanges,
  Vcl.ComCtrls, dxCore, cxDateUtils, dxScrollbarAnnotations;

type
  TfrmGenerareOP = class(TForm)
    pnClient: TPanel;
    GRTop: TGroupBox;
    Panel1: TPanel;
    Splitter1: TSplitter;
    GroupBox2: TGroupBox;
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    Label4: TLabel;
    Label5: TLabel;
    TblNota: TdxMemData;
    DTOp: TDataSource;
    QryFacturi: TZQuery;
    DTFacturi: TDataSource;
    pnTop: TPanel;
    Label6: TLabel;
    Label7: TLabel;
    Label8: TLabel;
    Label9: TLabel;
    edListaLuni: TcxImageComboBox;
    edData: TcxDateEdit;
    edNrNota: TcxMaskEdit;
    edOperator: TcxImageComboBox;
    edListaAni: TcxImageComboBox;
    edNrUnic: TcxMaskEdit;
    edNrOrdin: TcxImageComboBox;
    edDataOrdin: TdxDateEdit;
    edJurnal: TcxImageComboBox;
    edContDebit: TdxPopupEdit;
    edContCredit: TdxPopupEdit;
    QryDebit: TZQuery;
    QryCredit: TZQuery;
    DTDebit: TDataSource;
    DTCredit: TDataSource;
    TreeCredit: TdxDBTreeList;
    TreeDebit: TdxDBTreeList;
    TreeCreditCONT: TdxDBTreeListMaskColumn;
    TreeCreditROMANA: TdxDBTreeListMaskColumn;
    TreeDebitCONT: TdxDBTreeListMaskColumn;
    TreeDebitROMANA: TdxDBTreeListMaskColumn;
    TreeFunctional: TdxDBTreeList;
    TreeFunctionalDESCRIERE: TdxDBTreeListMaskColumn;
    TreeFunctionalDENUMIRE: TdxDBTreeListMaskColumn;
    TreeFunctionalCOD_BUGET: TdxDBTreeListMaskColumn;
    TreeEconomic: TdxDBTreeList;
    TreeEconomicDESCRIERE: TdxDBTreeListMaskColumn;
    TreeEconomicDENUMIRE: TdxDBTreeListMaskColumn;
    TreeEconomicCOD_BUGET: TdxDBTreeListMaskColumn;
    edGrupGridOp: TcxImageComboBox;
    DTOphturi: TDataSource;
    QryOPH: TZQuery;
    BtnAdaugaFactura: TcxButton;
    BtnRemoveFactura: TcxButton;
    BtnAnulareOP: TcxButton;
    btnCancel: TcxButton;
    BtnOk: TcxButton;
    BtnSalvare: TcxButton;
    gridOrdine: TcxGrid;
    viewOrdine: TcxGridDBTableView;
    nivelOrdine: TcxGridLevel;
    viewOrdineNRDOC: TcxGridDBColumn;
    viewOrdineTIP_DOCUMENT: TcxGridDBColumn;
    viewOrdineDATA: TcxGridDBColumn;
    viewOrdineEXPLICATIE: TcxGridDBColumn;
    viewOrdineVALOARE: TcxGridDBColumn;
    viewOrdineCONTD: TcxGridDBColumn;
    viewOrdineREPARTITOR_DEBIT: TcxGridDBColumn;
    viewOrdineCONTC: TcxGridDBColumn;
    viewOrdineREPARTITOR_CREDIT: TcxGridDBColumn;
    viewOrdineCOD_FUNCTIONAL: TcxGridDBColumn;
    viewOrdineCOD_ECONOMIC: TcxGridDBColumn;
    viewOrdineNR_OP: TcxGridDBColumn;
    viewOrdineDATA_OP: TcxGridDBColumn;
    viewOrdineTIP_NOTA: TcxGridDBColumn;
    cxStyleRepository1: TcxStyleRepository;
    cxStyle1: TcxStyle;
    cxStyle2: TcxStyle;
    cxStyle3: TcxStyle;
    cxStyle4: TcxStyle;
    viewNote: TcxGridDBTableView;
    nivelNote: TcxGridLevel;
    gridNote: TcxGrid;
    viewNoteJURNAL: TcxGridDBColumn;
    viewNoteNRDOC: TcxGridDBColumn;
    viewNoteDATA: TcxGridDBColumn;
    viewNoteEXPLICATIE: TcxGridDBColumn;
    viewNoteCONT_DEBT: TcxGridDBColumn;
    viewNoteREPARTITOR_DEBIT: TcxGridDBColumn;
    viewNoteCONT_CRED: TcxGridDBColumn;
    viewNoteREPARTITOR_CREDIT: TcxGridDBColumn;
    viewNoteVALOARE: TcxGridDBColumn;
    viewNoteMODUL: TcxGridDBColumn;
    viewNoteBUGET: TcxGridDBColumn;
    viewNoteCOD: TcxGridDBColumn;
    viewNotePOZ: TcxGridDBColumn;
    viewNoteECL: TcxGridDBColumn;
    viewNoteCOMPUSA: TcxGridDBColumn;
    viewNoteCONTD: TcxGridDBColumn;
    viewNoteCONTC: TcxGridDBColumn;
    viewNoteC_O: TcxGridDBColumn;
    viewNoteDATA_OPERARE: TcxGridDBColumn;
    viewNoteID_INITIAL: TcxGridDBColumn;
    viewNoteID_PARINTE: TcxGridDBColumn;
    viewNoteSTARE: TcxGridDBColumn;
    viewNoteCOD_FUNCTIONAL: TcxGridDBColumn;
    viewNoteCOD_ECONOMIC: TcxGridDBColumn;
    viewNoteDATA_OP: TcxGridDBColumn;
    viewNoteNR_OP: TcxGridDBColumn;
    cxStyleRepository2: TcxStyleRepository;
    cxStyle5: TcxStyle;
    cxStyle6: TcxStyle;
    cxStyle7: TcxStyle;
    cxStyle8: TcxStyle;
    procedure edListaAniChange(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure pnClientResize(Sender: TObject);
    procedure BtnAdaugaFacturaClick(Sender: TObject);
    procedure BtnRemoveFacturaClick(Sender: TObject);
    procedure TblNotaBeforeDelete(DataSet: TDataSet);
    procedure FormCloseQuery(Sender: TObject; var CanClose: Boolean);
    procedure TreeCreditROMANAGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure TreeDebitROMANAGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure edContDebitPopup(Sender: TObject; const EditText: String);
    procedure edContCreditPopup(Sender: TObject; const EditText: String);
    procedure TreeCreditDblClick(Sender: TObject);
    procedure TreeDebitKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure edContDebitCloseUp(Sender: TObject; var Text: String;
      var Accept: Boolean);
    procedure edContCreditCloseUp(Sender: TObject; var Text: String;
      var Accept: Boolean);
    procedure BtnOkClick(Sender: TObject);
    procedure GridOPCOD_FUNCTIONALCloseUp(Sender: TObject;
      var Text: String; var Accept: Boolean);
    procedure GridOPCOD_ECONOMICCloseUp(Sender: TObject; var Text: String;
      var Accept: Boolean);
    procedure TreeEconomicDESCRIEREGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure TreeFunctionalDESCRIEREGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure TblNotaAfterOpen(DataSet: TDataSet);
    procedure InternalValidateCont(Val: String; AColIndex: Integer; Tree: TdxDbTreeList);
    function  GetNodeByVal(ATree: TCustomdxTreeList; AColIndex: Integer; AVal: String; ASearchType: TdxTLSearchType = stExact): TdxTreeListNode;
    procedure TblNotaNewRecord(DataSet: TDataSet);
    procedure TblNotaAfterPost(DataSet: TDataSet);
    procedure edNrOrdinChange(Sender: TObject);
    procedure edDataOrdinValidate(Sender: TObject; var ErrorText: String;
      var Accept: Boolean);
    procedure SetGridEnabled(AEnabled: Boolean);
    procedure BtnAnulareOPClick(Sender: TObject);
    procedure edGrupGridOpChange(Sender: TObject);
    procedure viewOrdineFocusedRecordChanged(Sender: TcxCustomGridTableView;
      APrevFocusedRecord, AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure viewNoteDataControllerFilterChanged(Sender: TObject);
    procedure edNrNotaPropertiesEditValueChanged(Sender: TObject);
  private
    { Private declarations }
    FIsLoading :  Boolean;
    FTipuriDocPlata,
    FConturi   : TAtsMemData;
    OldFilter : String;
    FContOP: String;
    FDataOp: TDateTime;
    FDrepturi: Integer;
    procedure SetUserRights;
    procedure PostModificareCont;

    procedure RefreshFilter;
    procedure SetFirstEntry(AEdit: TdxPopupEdit; ATree: TdxDBTreeList);
    procedure ValidateCodEconomic(Sender: TField);
    procedure ValidareTipDocument(Sender: TField);
    procedure ValidareCodGest(Sender: TField);
    procedure AplicaContCreditor(ClasaEconomica, ClasaFunctionala: String);
    procedure AplicaContare(TipDocument: Integer; Gestiune: Integer);
    procedure UpdateConturi;
    procedure SetContOP(const Value: String);
    procedure SetDataOP(const Value: TDateTime);
    procedure SetDrepturi(const Value: Integer);
  public
    { Public declarations }
    procedure WriteToDb;
    procedure InitFields;
    procedure TestOp;
    property  ContOP: String read FContOP write SetContOP;
    property  DataOP: TDateTime read FDataOp write SetDataOP;
    property  Drepturi : Integer read FDrepturi write SetDrepturi;
  end;

implementation

{$R *.DFM}

uses
  dxCompsUtile, ZeosDBUtile, CommonDBVar, DateUnit, ATSZDBUtils;

procedure TfrmGenerareOP.edListaAniChange(Sender: TObject);
begin
  FillImageComboFmt(edListaLuni.Properties, 'SELECT MONTH(DATA) FROM CNOTE WHERE YEAR(DATA) = %d GROUP BY MONTH(DATA) ORDER BY 1', [ValueToStr(edListaAni.EditValue)], 0, 0);
  edListaLuni.Properties.DefaultDescription := 'Toate Lunile';
  if edListaLuni.Properties.Items.Count > 0 then
    edListaLuni.EditValue := edListaLuni.Properties.Items[edListaLuni.Properties.Items.Count-1].Value
  else
    edListaLuni.Clear;
end;

procedure TfrmGenerareOP.RefreshFilter;
const
  cst_Filter = '%s and ISNULL(TIP_NOTA, 1) = %d';
  
var
  lPrevActive : Boolean;
  lFilter     : String;

  procedure Add2Filter(const AFormat: String; const AValue: Variant; AQuoted: Boolean = True);
  begin
    if ValueHasValue(AValue) then begin
      if lFilter > '' then lFilter := lFilter + ' and ';
      lFilter := lFilter + Format(AFormat, [ValueToStr(AValue, AQuoted)]);
    end;

  end;

begin
  lFilter := 'STARE = 1';
  Add2Filter('NR_OP = %s', edNrUnic.EditValue);
  Add2Filter('YEAR(DATA) = %s', edListaAni.EditValue);
  Add2Filter('MONTH(DATA) = %s', edListaLuni.EditValue);
  Add2Filter('NRDOC = %s', edNrNota.EditValue);
  Add2Filter('DATA = %s', edData.EditValue);
  Add2Filter('C_O = %s', edOperator.EditValue);
  if viewNote.DataController.Filter.IsFiltering then
    Add2Filter('%s', viewNote.DataController.Filter.FilterText, False);
  if OldFilter <> lFilter then begin
     lPrevActive := QryFacturi.Active;
     if lPrevActive then QryFacturi.Active := False;
     QryFacturi.Sql[1] := Format(cst_Filter, [lFilter, 1]);
     QryOPH.Sql[1] := Format(cst_Filter, [lFilter, 2]);
     try
       if lPrevActive then QryFacturi.Active := True;
       //if QryFacturi.RecordCount <> 0 then
       QryOPH.Active := True;
       OldFilter := lFilter;
     except
       QryFacturi.Sql[1] := Format(cst_Filter, [OldFilter, 1]);
       QryOPH.Sql[1]     := Format(cst_Filter, [OldFilter, 2]);
       QryFacturi.Active := True;
       QryOPH.Active := True;
     end;
  end;
end;

procedure TfrmGenerareOP.WriteToDb;
begin
{}
end;

procedure TfrmGenerareOP.FormCreate(Sender: TObject);
begin

  SetUserRights;
  FDataOp := 0;

  FConturi := TAtsMemData.Create(Self);
  FTipuriDocPlata := TAtsMemData.Create(Self);

  DBLoadDataSet(FConturi, 'exec [SP_GET_MAPARE_CONTURI_ORDINE_PLATA]');
  DBLoadDataSet(FTipuriDocPlata, 'SELECT * FROM TIPURI_DOC_PLATA ORDER BY ID_TIP_DOC_PLATA');

  FillImageCombo(edNrOrdin.Properties, 'SELECT CONT_EXTRAS, DENUMIRE FROM LST_CONTURI_EXTRAS ORDER BY CONT_EXTRAS', 'CONT_EXTRAS', 'DENUMIRE');

  QryCredit.Params[0].Value := '5%';
  QryCredit.Open;
  SetFirstEntry(edContCredit, TreeCredit);

  QryDebit.Params[0].Value := '4%';
  QryDebit.Open;
  SetFirstEntry(edContDebit, TreeDebit);
  FillImageCombo(viewOrdineTIP_DOCUMENT.Properties, FTipuriDocPlata, 'ID_TIP_DOC_PLATA', 'DENUMIRE');
  FillImageCombo(viewNoteC_O.Properties, frmData.QryOperatori, 'ID_UTILIZATORI', 'NUME');

  FillImageCombo(viewNoteREPARTITOR_CREDIT.Properties, 'SELECT ID_REPARTITORI, NUME FROM REPARTITORI WHERE ISNULL(GESTINT, 0) = 0', 'ID_REPARTITORI', 'NUME');
  FillImageCombo(viewNoteREPARTITOR_DEBIT.Properties, 'SELECT ID_REPARTITORI, NUME FROM REPARTITORI WHERE ISNULL(GESTINT, 1) = 1', 'ID_REPARTITORI', 'NUME');

  viewOrdineREPARTITOR_DEBIT.Properties.Assign(viewNoteREPARTITOR_CREDIT.Properties);
  viewOrdineREPARTITOR_CREDIT.Properties.Assign(viewNoteREPARTITOR_DEBIT.Properties);

  FillImageCombo(viewNoteJURNAL.Properties, 'select jurnal, denumire from cjurnale order by denumire', 0, 1);
  edJurnal.Properties.Assign(viewNoteJURNAL.Properties);
  edJurnal.EditText := 'INCASARI';
  edOperator.Properties.Assign(viewNoteC_O.Properties);
  edOperator.Properties.DefaultDescription := 'Toti Operatorii';
  edOperator.Clear;

  FillImageCombo(edListaAni.Properties, 'SELECT YEAR(DATA) FROM CNOTE GROUP BY YEAR(DATA) ORDER BY 1', 0, 0);
  edListaAni.Properties.DefaultDescription := 'Toti Anii';
  if edListaAni.Properties.Items.Count > 0 then
    edListaAni.EditValue := edListaAni.Properties.Items[edListaAni.Properties.Items.Count-1].Value
  else
    edListaAni.Clear;
end;

procedure TfrmGenerareOP.pnClientResize(Sender: TObject);
begin
  GRTop.Height := (pnClient.Height - pnClient.BorderWidth * 2) div 2;
end;

procedure TfrmGenerareOP.InitFields;
begin
//  ATENTIE
  DefaultFieldClasses[ftAutoInc] := TIntegerField;
  TblNota.CreateFieldsFromDataSet(QryFacturi);
  DefaultFieldClasses[ftAutoInc] := TAutoIncField;
  TblNota.Open;
end;

procedure TfrmGenerareOP.BtnAdaugaFacturaClick(Sender: TObject);
var
  I : Integer;
  AField : TField;
begin
  if TblNota.Locate('NR', QryFacturi.FieldByName('NR').AsInteger, []) then begin
     MessageDlg('Factura este deja selectata !', mtError, [mbOk], 0);
     Abort;
  end;
  if Trim(edNrOrdin.Text) = '' then begin
     edNrOrdin.SetFocus;
     raise EContaHandledError.Create('Introduceti numarul ordinului de plata !');
  end;
  if edDataOrdin.Date <= 0 then begin
     edDataOrdin.SetFocus;
     raise EContaHandledError.Create('Introduceti data ordinului de plata !');
  end;  
  TblNota.DisableControls;
  try
     if not TblNota.Active then TblNota.Open;
     TblNota.Append;
     for i := 0 to QryFacturi.FieldCount - 1 do begin
      AField := TblNota.FindField(QryFacturi.Fields[i].FieldName);
      if (AField <> nil) and (AField <> TblNota.RecIdField) then
         AField.Value := QryFacturi.Fields[i].Value;
     end;

     TblNota.FieldByName('CONTC').AsString  := edContCredit.Text;
     TblNota.FieldByName('CONT_CRED').AsString := edContCredit.Text;
     TblNota.FieldByName('CONTD').AsString := edContDebit.Text;
     TblNota.FieldByName('CONT_DEBT').AsString := edContDebit.Text;
     TblNota.Post;
  finally
    TblNota.EnableControls;
  end;
end;

procedure TfrmGenerareOP.BtnRemoveFacturaClick(Sender: TObject);
begin
  if not TblNota.IsEmpty then
     TblNota.Delete;
end;

procedure TfrmGenerareOP.TblNotaBeforeDelete(DataSet: TDataSet);
begin
  if MessageDlg('Doriti sa stergeti factura asociata ordinului de plata ?', mtConfirmation, [mbYes, mbNo], 0) <> mrYes then Abort;
end;

procedure TfrmGenerareOP.FormCloseQuery(Sender: TObject;
  var CanClose: Boolean);
begin
  CanClose := FDrepturi = 2;
  if not CanClose then
    if (not TblNota.IsEmpty) then
       case MessageDlg('Aveti deja selecate facturi - Doriti salvarea lor?', mtConfirmation, [mbYes, mbNo, mbCancel], 0) of
         mrCancel : CanClose := False;
         mrNo     : CanClose := True;
         mrYes    : begin BtnSalvare.Click; CanClose := True; end;
       end
    else CanClose := True;
end;

procedure TfrmGenerareOP.SetFirstEntry(AEdit: TdxPopupEdit; ATree: TdxDBTreeList);
var lNode: TdxDBTreeListNode;

  function GetLastChild(ANode: TdxDBTreeListNode): TdxDBTreeListNode;
   begin
     if ANode.HasChildren then
        Result := GetLastChild(TdxDBTreeListNode(ANode.Items[0]))
     else Result := ANode;
   end;

begin
  if ATree.Count > 0 then
     lNode := GetLastChild(TdxDBTreeListNode(ATree.Items[0]))
  else lNode := nil;
  if Assigned(lNode) then AEdit.Text := lNode.Strings[0];
end;

procedure TfrmGenerareOP.TreeCreditROMANAGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
begin
  AText := ANode.Strings[TreeCreditCONT.Index]+' : '+AText;
end;

procedure TfrmGenerareOP.TreeDebitROMANAGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
begin
  AText := ANode.Strings[TreeDebitCONT.Index] + ' : '+ AText;
end;

procedure TfrmGenerareOP.edContDebitPopup(Sender: TObject;
  const EditText: String);
var lNode : TdxDBTreeListNode;
begin
  lNode := TreeDebit.FindNodeByKeyValue(EditText);
  if Assigned(lNode) then lNode.Focused := True;
end;

procedure TfrmGenerareOP.edContCreditPopup(Sender: TObject;
  const EditText: String);
var lNode : TdxDBTreeListNode;
begin
  lNode := TreeCredit.FindNodeByKeyValue(EditText);
  if Assigned(lNode) then lNode.Focused := True;
end;

procedure TfrmGenerareOP.TreeCreditDblClick(Sender: TObject);
begin
  with TdxDBTreeList(Sender) do
    if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
end;

procedure TfrmGenerareOP.TreeDebitKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if ((Key in [VK_UP, VK_DOWN]) and (ssAlt in Shift)) or
      ((Key = VK_F4) and not (ssAlt in Shift)) or (Key = VK_ESCAPE) then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(False);
  if (Key = VK_RETURN) and (TdxDBTreeList(Sender).FocusedNode <> nil)
     and (not TdxDBTreeList(Sender).FocusedNode.HasChildren) then
     (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
end;

procedure TfrmGenerareOP.edContDebitCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
var lNode : TdxDBTreeListNode;
begin
  lNode := TdxDBTreeListNode(TreeDebit.FocusedNode);
  edContDebit.HideEditCursor := False;
  if Assigned(lNode) then Text := lNode.Id
  else Text := '';
  edContDebit.Text := Text;
  edContDebit.HideEditCursor := True;
//  Abort;
end;

procedure TfrmGenerareOP.edContCreditCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
var lNode : TdxDBTreeListNode;
begin
  lNode := TdxDBTreeListNode(TreeCredit.FocusedNode);
  edContCredit.HideEditCursor := False;
  if Assigned(lNode) then Text := lNode.Id
  else Text := '';
  edContCredit.Text := Text;
  edContCredit.HideEditCursor := True;
//  Abort;
end;

procedure TfrmGenerareOP.BtnOkClick(Sender: TObject);
var lNrOP: Integer;
    lNrNota: Integer;

    procedure SetParamsFromDataSet(Params: TParams; DataSet: TDataSet);
    var
      I: Integer;
      lFieldName: String;
      lField: TField;
     begin
       for I := 0 to Params.Count-1 do begin
         lFieldName := Params[I].Name;
         lField := DataSet.FindField(lFieldName);
         if Assigned(lField) then Params[I].Assign(lField);
       end;
     end;

begin
  if FDrepturi = 2 then Exit;

  if (not TblNota.Active) or (TblNota.IsEmpty) then
     raise EContaHandledError.Create('Nu ati selectat nici o factura pentru ordinul de plata curent !');
  if Trim(edNrOrdin.Text) = '' then begin
     edNrOrdin.SetFocus;
     raise EContaHandledError.Create('Introduceti numarul ordinului de plata !');
  end;
  if edDataOrdin.Date <= 0 then begin
     edDataOrdin.SetFocus;
     raise EContaHandledError.Create('Introduceti data ordinului de plata !');
  end;
  try
    DBStartTransaction;
    with GetTmpADOQuery do
      try
         Sql.Add('UPDATE CNOTE SET STARE=0 WHERE STARE=1 AND ID_ORDIN_PLATA IN (SELECT ID_ORDINE_PLATA FROM ORDINE_PLATA WHERE NR_ORDIN = :NR_ORDIN AND DATA_ORDIN = :DATA)');
         Params[0].Value := Trim(edNrOrdin.Text);
         Params[1].Value := edDataOrdin.Date;
         ExecSql;
         Sql.Clear;
         Sql.Add('UPDATE ORDINE_PLATA SET STARE=0 FROM ORDINE_PLATA WHERE NR_ORDIN = :NR_ORDIN AND DATA_ORDIN = :DATA');
         Params[0].Value := Trim(edNrOrdin.Text);
         Params[1].Value := edDataOrdin.Date;
         ExecSql;
         Sql.Clear;
         lNrOP := GetNextId('ORDINE_PLATA');
         { Luam ID-ul OP-ului }
         Sql.Add('INSERT INTO ORDINE_PLATA (ID_ORDINE_PLATA, NR_ORDIN, DATA_ORDIN, CONT_DEBIT, CONT_CREDIT, ID_UTILIZATORI, STARE)');
         Sql.Add('VALUES (:ID_ORDINE_PLATA, :NR_ORDIN, :DATA_ORDIN, :CONT_DEBIT, :CONT_CREDIT, :ID_UTILIZATORI, 1)');
         Params[0].Value := lNrOp;
         Params[1].Value := edNrOrdin.Text;
         Params[2].Value := edDataOrdin.Date;
         Params[3].Value := edContDebit.Text;
         Params[4].Value := edContCredit.Text;
         Params[5].Value := IdUtilizator;
         ExecSql;
         lNrNota := GetNextId('NOTA_CONTABILA');
         Sql.Clear;
         Sql.Add('DECLARE @DATA DATETIME');
         Sql.Add('SET @DATA = GETDATE()');
         //MODIFICARE 22.01.2007 Cristi am adaugat BUGET_NR_OP pentru ca in caz ca se intra pe modificare nota contabila nrdoc devine primul nrdoc din nota respectiva
         Sql.Add('INSERT INTO CNOTE ( COD,  TIP_DOCUMENT,  POZ  ,  JURNAL ,  NRDOC,  DATA   ,  EXPLICATIE,  VALOARE,  ECL,  C_O,  CONTD,  CONTC,  CONT_DEBT,  CONT_CRED,  DATA_OPERARE,  REPARTITOR_CREDIT,  STARE,');
         Sql.Add(' COD_FUNCTIONAL,  COD_ECONOMIC,  NR_OP,  DATA_OP,  REPARTITOR_DEBIT, ID_ORDIN_PLATA, NR_VECHI, BUGET_NR_OP, TIP_NOTA)');
         Sql.Add('VALUES            (:XCOD, :TIP_DOCUMENT, :RECID, :XJURNAL, :NRDOC, :XDATADOC, :EXPLICATIE, :VALOARE, :ECL, :XC_O, :CONTD, :CONTC, :CONT_DEBT, :CONT_CRED,  @DATA       , :REPARTITOR_CREDIT, 1      ,');
         Sql.Add(':COD_FUNCTIONAL, :COD_ECONOMIC, :NR_OP, :DATA_OP, :REPARTITOR_DEBIT, :XID_ORDIN, :NR_OP, :NRDOC, :TIP_NOTA)');
         DataSource := DTOp;
         Params.ParamByName('XCOD').Value     := lNrNota;
         Params.ParamByName('XJURNAL').Value  := edJurnal.Text;
         Params.ParamByName('XDATADOC').Value  := edDataOrdin.Date;
         Params.ParamByName('XC_O').Value     := IdUtilizator;
         Params.ParamByName('XID_ORDIN').Value := lNrOP;
         with TblNota do begin
           TblNota.First;
           while not TblNota.Eof do begin
             SetParamsFromDataSet(Params, TblNota);
             ExecSql;
             TblNota.Next;
           end;
         end;
      finally
         Free;
      end;
      DBCommit;
      TblNota.Active := False;
      TblNota.Active := True;
      if TcxButton(Sender).Tag = 0 then
         ModalResult := mrOk
      else edNrOrdin.Text := '';
  except
    on E: Exception do begin
      DBRollBack;
      raise;
    end;
  end;
end;

procedure TfrmGenerareOP.GridOPCOD_FUNCTIONALCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
var lNode: TdxDBTreeListNode;
    lEditabil: Boolean;
begin
  with TdxDBTreeListPopupColumn(Sender) do
    if Accept then begin
       lNode := TdxDBTreeListNode(TdxDBTreeList(PopupControl).FocusedNode);
       if Assigned(lNode) then begin
          lEditabil := Field.DataSet.State in [dsEdit, dsInsert];
          if not lEditabil then Field.DataSet.Edit;
          Field.AsString := lNode.Strings[TreeFunctionalCOD_BUGET.Index];
          Field.DataSet.Post;
          if lEditabil then Field.DataSet.Edit;
          Text := lNode.Id;
          Accept := False;
       end;
    end;
end;

procedure TfrmGenerareOP.GridOPCOD_ECONOMICCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
var lNode: TdxDBTreeListNode;
    lEditabil: Boolean;
begin
  with TdxDBTreeListPopupColumn(Sender) do
    if Accept then begin
       lNode := TdxDBTreeListNode(TdxDBTreeList(PopupControl).FocusedNode);
       if Assigned(lNode) then begin
          lEditabil := Field.DataSet.State in [dsEdit, dsInsert];
          if not lEditabil then Field.DataSet.Edit;
          Field.AsString := lNode.Strings[TreeEconomicCOD_BUGET.Index];
          Field.DataSet.Post;
          if lEditabil then Field.DataSet.Edit;
          Text := lNode.Id;
          Accept := False;
       end;
    end;
end;

procedure TfrmGenerareOP.TreeEconomicDESCRIEREGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
begin
  AText := Trim(ANode.Strings[TreeEconomicCOD_BUGET.Index])+' : '+Trim(ANode.Strings[TreeEconomicDENUMIRE.Index]);
end;

procedure TfrmGenerareOP.TreeFunctionalDESCRIEREGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
begin
  AText := Trim(ANode.Strings[TreeFunctionalCOD_BUGET.Index])+' : '+Trim(ANode.Strings[TreeFunctionalDENUMIRE.Index]);
end;

procedure TfrmGenerareOP.TblNotaAfterOpen(DataSet: TDataSet);
var
 lField: TField;
begin
  with DataSet.FieldByName('COD_ECONOMIC') do begin
    Tag := 1;
    OnValidate := ValidateCodEconomic;
  end;
  with DataSet.FieldByName('COD_FUNCTIONAL') do begin
    Tag := 0;
    OnValidate := ValidateCodEconomic;
  end;
  with DataSet.FieldByName('TIP_DOCUMENT') do begin
    OnValidate := ValidareTipDocument;
  end;

  lField := DataSet.FindField('CODGEST');
  if Assigned(lField) then begin
    lField.OnValidate := ValidareCodGest;
  end;
end;

procedure TfrmGenerareOP.ValidateCodEconomic(Sender: TField);
begin
  if Sender.Tag = 0 then begin
     InternalValidateCont(Trim(Sender.AsString), TreeFunctionalCOD_BUGET.Index, TreeFunctional);
     AplicaContCreditor(Trim(TblNota.FieldByName('COD_ECONOMIC').AsString), Trim(Sender.AsString));
  end
  else begin
    InternalValidateCont(Trim(Sender.AsString), TreeEconomicCOD_BUGET.Index, TreeEconomic);
    { Schimbam si contul de credit in functie de clasificatia economica }
    AplicaContCreditor(Trim(Sender.AsString), Trim(TblNota.FieldByName('COD_FUNCTIONAL').AsString));
  end;
end;

procedure TfrmGenerareOP.viewNoteDataControllerFilterChanged(Sender: TObject);
begin
  RefreshFilter;
end;

procedure TfrmGenerareOP.viewOrdineFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord; ANewItemRecordFocusingChanged: Boolean);
begin
  if Assigned(AFocusedRecord) and AFocusedRecord.IsData then
    if ValueSafeToInt(AFocusedRecord.Values[viewOrdineTIP_NOTA.Index]) = 2 then
      SetGridEnabled(True)
    else
      SetGridEnabled(False);
end;

procedure TfrmGenerareOP.InternalValidateCont(Val: String;
  AColIndex: Integer; Tree: TdxDbTreeList);
var MustDrop: Boolean;
    SelNode : TdxTreeListNode;
begin
  Tree.EndSearch;
  { Se valideaza contul de buget introdus }
  MustDrop := (Val = '') or (Val = '?');
  { Incercam sa gasim nodul posibil }
  SelNode := nil;
  if not MustDrop then begin
     if AColIndex = -1 then begin
        SelNode := TdxDBTreeList(Tree).FindNodeByKeyValue(Val);
        MustDrop := not Assigned(SelNode);
     end
     else begin
       while (not Assigned(SelNode)) and (Length(Val) > 0) do begin
         SelNode := GetNodeByVal(Tree, AColIndex, Val);
         if not Assigned(SelNode) then Delete(Val, Length(Val), 1);
       end;
       MustDrop := (not Assigned(SelNode)) or (SelNode.HasChildren);
       if (Assigned(SelNode)) and (MustDrop) then begin
          while SelNode.Count > 0 do SelNode := SelNode.Items[0];
          SelNode.Focused := True;
       end;
     end;
  end
  else begin
     { Mergem Pe Focused Node in jos }
     SelNode := Tree.FocusedNode;
     while (Assigned(SelNode)) and (SelNode.Count > 0) do SelNode := SelNode.Items[0];
     if Assigned(SelNode) then SelNode.Focused := True;
  end;

  if (MustDrop) and (Assigned(viewOrdine.Controller.EditingController)) then begin
      if Assigned(SelNode) then begin SelNode.MakeVisible; SelNode.Focused := True; end
      else Tree.StartSearch(-1, Val);
      viewOrdine.Controller.EditingController.ShowEdit;
      Abort;
  end;
end;

type TCrackAtsTree = class(TdxTreeList);

function TfrmGenerareOP.GetNodeByVal(ATree: TCustomdxTreeList;
  AColIndex: Integer; AVal: String;
  ASearchType: TdxTLSearchType): TdxTreeListNode;
var OldSearch: TdxTLSearchType;
    lNode    : TdxTreeListNode;
begin
  Result := nil;
  with TCrackAtsTree(ATree) do begin
    OldSearch := SearchType;
    try
       SearchType := stExact;
       if FindNodeByText(AColIndex, AVal, sdNone, lNode) then Result := lNode;
    finally
       SearchType := OldSearch;
    end;
  end;
end;

procedure TfrmGenerareOP.TblNotaNewRecord(DataSet: TDataSet);
begin
  if Drepturi <> 2 then begin
    DataSet.FieldByName('ECL').AsInteger := 1;
    DataSet.FieldByName('TIP_NOTA').AsInteger := 2;
    DataSet.FieldByName('DATA').Value := edDataOrdin.Date;
    DataSet.FieldByName('C_O').Value := IdUtilizator;
    TblNota.FieldByName('CONTC').AsString := edContCredit.Text;
    TblNota.FieldByName('CONT_CRED').AsString := edContCredit.Text;
    TblNota.FieldByName('CONTD').AsString := edContDebit.Text;
    TblNota.FieldByName('CONT_DEBT').AsString := edContDebit.Text;
    if TblNota.FieldByName('NR_OP') <> nil then
       TblNota.FieldByName('NR_OP').AsInteger := GetNextId('POZITIE_NOTA');
  end;
  SetGridEnabled(True);
end;

procedure TfrmGenerareOP.AplicaContCreditor(ClasaEconomica, ClasaFunctionala: String);
var lFound: Boolean;
    lCont : String;
begin
  Exit;
  if pos('700', Trim(TblNota.FieldByName('CONTC').AsString)) = 1 then begin
     with FConturi do begin
       First;
       lFound := False;
       while not Eof do begin
         lFound := (ClasaEconomica = '') or (pos(Trim(FieldByName('COD_ECONOMIC').AsString), ClasaEconomica) = 1);
         if lFound then
            lFound := (Trim(FieldByName('COD_FUNCTIONAL').AsString)='') or (ClasaFunctionala = '') or (pos(Trim(FieldByName('COD_FUNCTIONAL').AsString), ClasaFunctionala) = 1);
         if lFound  then begin
            lCont := Trim(FieldByName('CONT_REAL').AsString);
            Break;
         end;
         Next;
       end;
       if lFound then begin
         TblNota.FieldByName('CONTC').AsString := lCont;
         TblNota.FieldByName('CONT_CRED').AsString := lCont;
       end;
     end;
  end
  else
  if pos('700', Trim(TblNota.FieldByName('CONTD').AsString)) = 1 then begin
     with FConturi do begin
       First;
       lFound := False;
       while not Eof do begin
         lFound := (ClasaEconomica = '') or (pos(Trim(FieldByName('COD_ECONOMIC').AsString), ClasaEconomica) = 1);
         if lFound then
            lFound := (Trim(FieldByName('COD_FUNCTIONAL').AsString)='') or (ClasaFunctionala = '') or (pos(Trim(FieldByName('COD_FUNCTIONAL').AsString), ClasaFunctionala) = 1);
         if lFound  then begin
            lCont := Trim(FieldByName('CONT_REAL').AsString);
            Break;
         end;
         Next;
       end;
       if lFound then begin
         TblNota.FieldByName('CONTD').AsString := lCont;
         TblNota.FieldByName('CONT_DEBT').AsString := lCont;
       end;
     end;
  end
end;

procedure TfrmGenerareOP.TblNotaAfterPost(DataSet: TDataSet);
begin
  viewNote.ApplyBestFit(nil);
  if (FDrepturi = 2) and (not FIsLoading) then PostModificareCont;
end;

procedure TfrmGenerareOP.edNrNotaPropertiesEditValueChanged(Sender: TObject);
begin
  RefreshFilter;
end;

procedure TfrmGenerareOP.edNrOrdinChange(Sender: TObject);
begin
  TestOp;
  UpdateConturi;
end;

procedure TfrmGenerareOP.TestOp;
var
  FakeQry: TZReadOnlyQuery;
  lMustAsk: Boolean;
begin
  if FIsLoading then Exit;
  FIsLoading := True;
  try
    { Testam OP -ul }
    if (Trim(edNrOrdin.Text) > '') and (edDataOrdin.Date > 0)
       and ((Trim(edNrOrdin.Text) <> Trim(FContOP)) or (FDataOp <> edDataOrdin.Date)) then begin
       FakeQry := GetTmpADOQuery;
       with FakeQry do
         try
            Sql.Add('SELECT TOP 1 ID_ORDINE_PLATA FROM ORDINE_PLATA AS A WHERE NR_ORDIN = :NR_ORDIN AND DATA_ORDIN = :DATA AND EXISTS (SELECT TOP 1 1 FROM CNOTE WHERE ID_ORDIN_PLATA = A.ID_ORDINE_PLATA AND STARE=1)');
            Params[0].Value := Trim(edNrOrdin.Text);
            Params[1].Value := edDataOrdin.Date;
            Open;
            lMustAsk := (Trim(FContOP) > '') and (FDataOp > 0)
                         and (FDrepturi <> 2)
                         and (TblNota.Active) and (not TblNota.IsEmpty);
            if lMustAsk then
              if MessageDlg('Aveti deja introdus ordinul de plata '+Trim(FContOP)+' din '+FormatDateTime('dd.mm.yyyy', FDataOp)+#13#10+
                            'Doriti procesarea unui nou extras?'+#13#10+
                            'Informatiile din ecran vor fi pierdute', mtConfirmation, [mbYes, mbNo], 0) <> mrYes then begin
                 edNrOrdin.Text := Trim(FContOP);
                 edDataOrdin.Date := FDataOp;
                 Abort;
              end;
            TblNota.Active := False;
            TblNota.Active := True;
            FContOP := Trim(edNrOrdin.Text);
            FDataOp := edDataOrdin.Date;
            if not IsEmpty then begin
               Close;
               Sql.Clear;
               Sql.Add('SELECT * FROM CNOTE WHERE STARE=1 AND ID_ORDIN_PLATA IN (SELECT ID_ORDINE_PLATA FROM ORDINE_PLATA WHERE NR_ORDIN = :NR_ORDIN AND DATA_ORDIN = :DATA AND STARE=1)');
               Params[0].Value := Trim(edNrOrdin.Text);
               Params[1].Value := edDataOrdin.Date;
               Open;
               TblNota.OnNewRecord := nil;
               try
                  TblNota.LoadFromDataSet(FakeQry);
               finally
                  TblNota.OnNewRecord := TblNotaNewRecord;
               end;
            end;
         finally
            Free;
         end;
       gridOrdine.SetFocus;
    end;
  finally
    FIsLoading := False;
  end;
end;

procedure TfrmGenerareOP.edDataOrdinValidate(Sender: TObject;
  var ErrorText: String; var Accept: Boolean);
begin
  TestOp;
end;

procedure TfrmGenerareOP.SetGridEnabled(AEnabled: Boolean);
var
  I: Integer;
begin
  if FDrepturi = 2 then begin
    for I := 0 to viewOrdine.ColumnCount-1 do
      viewOrdine.Columns[I].Options.Editing := False;

    viewOrdineCOD_FUNCTIONAL.Options.Editing := True;
    viewOrdineCOD_ECONOMIC.Options.Editing := True;
    viewOrdineTIP_DOCUMENT.Options.Editing := True;
    viewOrdineNRDOC.Options.Editing := True;
    viewOrdineVALOARE.Options.Editing := True;
    viewOrdineEXPLICATIE.Options.Editing := True;
    viewOrdineCONTD.Options.Editing := True;
    viewOrdineCONTC.Options.Editing := True;
    viewOrdineREPARTITOR_DEBIT.Options.Editing := True;
    viewOrdineREPARTITOR_CREDIT.Options.Editing := True;
  end
  else begin
    viewOrdineDATA.Options.Editing    := AEnabled;
    viewOrdineVALOARE.Options.Editing := True;
    viewOrdineTIP_DOCUMENT.Options.Editing := True;
    viewOrdineNRDOC.Options.Editing := True;
    viewOrdineEXPLICATIE.Options.Editing := True;
    viewOrdineREPARTITOR_DEBIT.Options.Editing := True;
    viewOrdineREPARTITOR_CREDIT.Options.Editing := True;
  end;
end;

procedure TfrmGenerareOP.ValidareTipDocument(Sender: TField);
begin
  AplicaContare(TblNota.FieldByName('TIP_DOCUMENT').AsInteger, Sender.AsInteger);
  Exit;
  if Sender.AsString = '1' then begin
     TblNota.FieldByName('CONTC').AsString     := edContCredit.Text;
     TblNota.FieldByName('CONT_CRED').AsString := edContCredit.Text;
     TblNota.FieldByName('CONTD').AsString     := edContDebit.Text;
     TblNota.FieldByName('CONT_DEBT').AsString := edContDebit.Text;
  end
  else if Sender.AsString = '2' then begin
     TblNota.FieldByName('CONTC').AsString     := '1301';
     TblNota.FieldByName('CONT_CRED').AsString := '1301';
     TblNota.FieldByName('CONTD').AsString     := edContCredit.Text;
     TblNota.FieldByName('CONT_DEBT').AsString := edContCredit.Text;
  end
  else if Sender.AsString = '3' then begin
     TblNota.FieldByName('CONTC').AsString     := edContCredit.Text;
     TblNota.FieldByName('CONT_CRED').AsString := edContCredit.Text;
     TblNota.FieldByName('CONTD').AsString     := '1301';
     TblNota.FieldByName('CONT_DEBT').AsString := '1301';
  end
  else if Sender.AsString = '4' then begin
     TblNota.FieldByName('CONTC').AsString     := edContDebit.Text;
     TblNota.FieldByName('CONT_CRED').AsString := edContDebit.Text;
     TblNota.FieldByName('CONTD').AsString     := edContCredit.Text;
     TblNota.FieldByName('CONT_DEBT').AsString := edContCredit.Text;
  end
  else if Sender.AsString = '5' then begin
     TblNota.FieldByName('CONTC').AsString     := edContCredit.Text;
     TblNota.FieldByName('CONT_CRED').AsString := edContCredit.Text;
     TblNota.FieldByName('CONTD').AsString     := edContDebit.Text;
     TblNota.FieldByName('CONT_DEBT').AsString := edContDebit.Text;
  end
  else if Sender.AsString = '6' then begin
     TblNota.FieldByName('CONTC').AsString     := edContDebit.Text;
     TblNota.FieldByName('CONT_CRED').AsString := edContDebit.Text;
     TblNota.FieldByName('CONTD').AsString     := edContCredit.Text;
     TblNota.FieldByName('CONT_DEBT').AsString := edContCredit.Text;
  end
  else if Sender.AsString = '7' then begin
     TblNota.FieldByName('CONTC').AsString     := edContCredit.Text;
     TblNota.FieldByName('CONT_CRED').AsString := edContCredit.Text;
     TblNota.FieldByName('CONTD').AsString     := edContDebit.Text;
     TblNota.FieldByName('CONT_DEBT').AsString := edContDebit.Text;
  end;
  if ( (TblNota.FieldByName('CONTC').AsString = '70007') or
       (TblNota.FieldByName('CONTC').AsString = '70008') ) and
     (TblNota.FieldByName('CONTD').AsString = '1301') then begin
     TblNota.FieldByName('CONTD').AsString     := '1302';
     TblNota.FieldByName('CONT_DEBT').AsString := '1302';
  end;
  AplicaContCreditor(Trim(TblNota.FieldByName('COD_ECONOMIC').AsString), Trim(TblNota.FieldByName('COD_FUNCTIONAL').AsString));
end;

procedure TfrmGenerareOP.SetContOP(const Value: String);
begin
  FContOP := Value;
end;

procedure TfrmGenerareOP.SetDataOP(const Value: TDateTime);
begin
  FDataOp := Value;
end;

procedure TfrmGenerareOP.SetUserRights;
begin
  { Setam Drepturile de access }
  with GetTmpADOQuery do
    try
       Sql.Add('SELECT ID_LST_LANGUAGES FROM UTILIZATORI WHERE ID_UTILIZATORI = '+IntToStr(IdUtilizator));
       Open;
       Drepturi := Fields[0].AsInteger;
    finally
       Free;
    end;
end;

procedure TfrmGenerareOP.SetDrepturi(const Value: Integer);
begin
  FDrepturi := Value;
  case FDrepturi of
    0, 1 : ;
    2    : { Avem contare }
           begin
             GRTop.Visible := False;
             edContDebit.Enabled := False;
             edContCredit.Enabled := False;
             BtnAdaugaFactura.Enabled := False;
             BtnRemoveFactura.Enabled := False;
             edJurnal.Enabled         := False;
           end;
  end;
end;

procedure TfrmGenerareOP.PostModificareCont;
begin
  { Salvam modificarile de conturi }
  //MODIFICARE 22.01.2007 Cristi am adaugat BUGET_NR_OP pentru ca in caz ca se intra pe modificare nota contabila nrdoc devine primul nrdoc din nota respectiva 
  with GetTmpADOQuery do
    try
       DataSource := DTOp;
       Sql.Add('UPDATE CNOTE SET');
       Sql.Add('VALOARE = :VALOARE, EXPLICATIE = :EXPLICATIE, NRDOC = :NRDOC, BUGET_NR_OP = :NRDOC, TIP_DOCUMENT = :TIP_DOCUMENT, COD_ECONOMIC = :COD_ECONOMIC, COD_FUNCTIONAL = :COD_FUNCTIONAL,');
       Sql.Add('CONTD = :CONTD, REPARTITOR_CREDIT = :REPARTITOR_CREDIT, REPARTITOR_DEBIT = :REPARTITOR_DEBIT, CONTC = :CONTC, CONT_DEBT = :CONT_DEBT , CONT_CRED = :CONT_CRED');
       Sql.Add('WHERE NR = :NR');
       ExecSQL;
    finally
       Free;
    end;
end;

procedure TfrmGenerareOP.BtnAnulareOPClick(Sender: TObject);
begin
  if (Trim(edNrOrdin.Text) > '') and (Trim(edDataOrdin.Text) > '') and (Trim(edDataOrdin.Text) <> edDataOrdin.GetBlankText) then
    if MessageDlg('Doriti anularea ordinului de plata '+edNrOrdin.Text+' din '+FormatDatetime('dd.mm.yyyy', edDataOrdin.Date),
                  mtConfirmation, [mbYes, mbNo], 0) = mrYes then
    with GetTmpADOQuery do
      try
         Sql.Add('UPDATE CNOTE SET STARE=0 WHERE STARE=1 AND ID_ORDIN_PLATA IN (SELECT ID_ORDINE_PLATA FROM ORDINE_PLATA WHERE NR_ORDIN = :NR_ORDIN AND DATA_ORDIN = :DATA)');
         Params[0].Value := Trim(edNrOrdin.Text);
         Params[1].Value := edDataOrdin.Date;
         ExecSql;
         Sql.Clear;
         Sql.Add('UPDATE ORDINE_PLATA SET STARE=0 FROM ORDINE_PLATA WHERE NR_ORDIN = :NR_ORDIN AND DATA_ORDIN = :DATA');
         Params[0].Value := Trim(edNrOrdin.Text);
         Params[1].Value := edDataOrdin.Date;
         ExecSql;
         TestOp;
      finally
         Free;
      end;
end;

procedure TfrmGenerareOP.ValidareCodGest(Sender: TField);
begin
  AplicaContare(TblNota.FieldByName('TIP_DOCUMENT').AsInteger, Sender.AsInteger);
end;

procedure TfrmGenerareOP.AplicaContare(TipDocument: Integer; Gestiune: Integer);
var
  lNewContD,
  lNewContC: String;
  lTipExtras: String;
  lFound    : Boolean;
  lIsRetur  : Boolean;
begin

  if FDrepturi = 2 then Exit;

  with FTipuriDocPlata do
    if Locate('ID_TIP_DOC_PLATA', TipDocument, []) then
      lIsRetur := FieldByName('SEMN').AsInteger = -1
    else lIsRetur := False;

  lTipExtras := Trim(edNrOrdin.Text);
  lNewContD := '';
  lNewContC := '';
  // In primul pas aflam modalitatea de contare
  with FConturi do begin
    if Locate('CONT_EXTRAS', lTipExtras, []) then begin
      lFound := False;
      // Verificam in cadrul contului de extras gestiunea
      while (FieldByName('CONT_EXTRAS').AsString = lTipExtras) and (FieldByName('CASA').AsInteger > 0) do begin
        if (Gestiune > 0) and (FieldByName('CASA').AsInteger = Gestiune) then begin
          lFound := True;
          if lIsRetur then begin
             lNewContD := Trim(FieldByName('CONT_CREDIT').AsString);
             lNewContC := Trim(FieldByName('CONT_DEBIT').AsString);
          end
          else begin
             lNewContD := Trim(FieldByName('CONT_DEBIT').AsString);
             lNewContC := Trim(FieldByName('CONT_CREDIT').AsString);
          end;
          Break;
        end;
        Next;
      end;
      if not lFound then begin
        if (FieldByName('CONT_EXTRAS').AsString = lTipExtras) and
             ( (FieldByName('CASA').IsNull) or (FieldByName('CASA').AsInteger = 0)) then begin
          if lIsRetur then begin
             lNewContD := Trim(FieldByName('CONT_CREDIT').AsString);
             lNewContC := Trim(FieldByName('CONT_DEBIT').AsString);
          end
          else begin
             lNewContD := Trim(FieldByName('CONT_DEBIT').AsString);
             lNewContC := Trim(FieldByName('CONT_CREDIT').AsString);
          end;
          lFound := True;
        end;
      end;

      if lFound then begin
        if not (TblNota.State in dsEditModes) then
          TblNota.Edit;
        TblNota.FieldByName('CONTD').AsString := lNewContD;
        TblNota.FieldByName('CONT_DEBT').AsString := lNewContD;
        TblNota.FieldByName('CONTC').AsString := lNewContC;
        TblNota.FieldByName('CONT_CRED').AsString := lNewContC;
      end;
      
    end;
  end;
end;

procedure TfrmGenerareOP.UpdateConturi;
var
  lTipExtras : String;
begin
  lTipExtras := Trim(edNrOrdin.Text);
  with FConturi do begin
    if Locate('CONT_EXTRAS', lTipExtras, []) then begin
      edContDebit.Text  := FieldByName('CONT_DEBIT').AsString;
      edContCredit.Text := FieldByName('CONT_CREDIT').AsString;
    end
    else begin
      edContDebit.Text  := '';
      edContCredit.Text := '';
    end;
  end;
end;

procedure TfrmGenerareOP.edGrupGridOpChange(Sender: TObject);
begin
  viewOrdine.Controller.ClearGrouping;
  case ValueSafeToInt(edGrupGridOp.EditValue) of
    1:
      viewOrdineCOD_FUNCTIONAL.GroupBy(0);
    2:
      viewOrdineCOD_ECONOMIC.GroupBy(0);
  end;
end;

end.
