unit GenerarePlataUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, ExtCtrls, Db, dxmdaset,
  ZDataSet, Buttons, Menus, cxGraphics, 
  cxTL, cxControls,
  cxInplaceContainer, cxTLData, cxDBTL, cxMaskEdit, 
  cxDataStorage, cxEdit, cxDBData, cxGridLevel,
  cxClasses, cxGridCustomView, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGrid, cxImageComboBox, cxCalendar, cxCurrencyEdit,
  cxTextEdit, cxDropDownEdit, cxLookAndFeelPainters, cxButtons, cxContainer,
  cxDataUtils, cxGridCustomPopupMenu, cxGridPopupMenu,
  ZAbstractRODataset, ZAbstractDataset,
  cxTLdxBarBuiltInMenu,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, cxSplitter,
  Vcl.ComCtrls, dxCore, cxDateUtils, cxNavigator,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxBarBuiltInMenu,
  dxDateRanges, dxScrollbarAnnotations;

type
  TfrmGenerarePlata = class(TForm)
    pnClient: TPanel;
    GRTop: TGroupBox;
    Panel1: TPanel;
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
    QryDebit: TZQuery;
    QryCredit: TZQuery;
    DTDebit: TDataSource;
    DTCredit: TDataSource;
    DTOphturi: TDataSource;
    QryOPH: TZQuery;
    cxTreeEconomic: TcxDBTreeList;
    cxTreeEconomicDESCRIERE: TcxDBTreeListColumn;
    cxTreeEconomicDENUMIRE: TcxDBTreeListColumn;
    cxTreeEconomicCOD_BUGET: TcxDBTreeListColumn;
    cxTreeFunctional: TcxDBTreeList;
    cxTreeFunctionalDESCRIERE: TcxDBTreeListColumn;
    cxTreeFunctionalDENUMIRE: TcxDBTreeListColumn;
    cxTreeFunctionalCOD_BUGET: TcxDBTreeListColumn;
    cxTreeCredit: TcxDBTreeList;
    cxTreeDebit: TcxDBTreeList;
    cxTreeCreditCONT: TcxDBTreeListColumn;
    cxTreeCreditROMANA: TcxDBTreeListColumn;
    cxTreeDebitCONT: TcxDBTreeListColumn;
    cxTreeDebitROMANA: TcxDBTreeListColumn;
    cxGridIstoricNoteL: TcxGridLevel;
    cxGridIstoricNote: TcxGrid;
    GridIstoricNote: TcxGridDBTableView;
    GridIstoricNoteJURNAL: TcxGridDBColumn;
    GridIstoricNoteNRDOC: TcxGridDBColumn;
    GridIstoricNoteDATA: TcxGridDBColumn;
    GridIstoricNoteEXPLICATIE: TcxGridDBColumn;
    GridIstoricNoteCONT_DEBT: TcxGridDBColumn;
    GridIstoricNoteREPARTITOR_DEBIT: TcxGridDBColumn;
    GridIstoricNoteCONT_CRED: TcxGridDBColumn;
    GridIstoricNoteREPARTITOR_CREDIT: TcxGridDBColumn;
    GridIstoricNoteVALOARE: TcxGridDBColumn;
    GridIstoricNoteMODUL: TcxGridDBColumn;
    GridIstoricNoteBUGET: TcxGridDBColumn;
    GridIstoricNoteCOD: TcxGridDBColumn;
    GridIstoricNotePOZ: TcxGridDBColumn;
    GridIstoricNoteECL: TcxGridDBColumn;
    GridIstoricNoteCOMPUSA: TcxGridDBColumn;
    GridIstoricNoteCONTD: TcxGridDBColumn;
    GridIstoricNoteCONTC: TcxGridDBColumn;
    GridIstoricNoteC_O: TcxGridDBColumn;
    GridIstoricNoteDATA_OPERARE: TcxGridDBColumn;
    GridIstoricNoteID_INITIAL: TcxGridDBColumn;
    GridIstoricNoteID_PARINTE: TcxGridDBColumn;
    GridIstoricNoteSTARE: TcxGridDBColumn;
    GridIstoricNoteCOD_FUNCTIONAL: TcxGridDBColumn;
    GridIstoricNoteCOD_ECONOMIC: TcxGridDBColumn;
    GridIstoricNoteDATA_OP: TcxGridDBColumn;
    GridIstoricNoteNR_OP: TcxGridDBColumn;
    cxGridOPL: TcxGridLevel;
    cxGridOP: TcxGrid;
    GridOP: TcxGridDBTableView;
    GridOPNRDOC: TcxGridDBColumn;
    GridOPTIP_DOCUMENT: TcxGridDBColumn;
    GridOPDATA: TcxGridDBColumn;
    GridOPEXPLICATIE: TcxGridDBColumn;
    GridOPVALOARE: TcxGridDBColumn;
    GridOPCONTD: TcxGridDBColumn;
    GridOPREPARTITOR_DEBIT: TcxGridDBColumn;
    GridOPCONTC: TcxGridDBColumn;
    GridOPREPARTITOR_CREDIT: TcxGridDBColumn;
    GridOPCOD_FUNCTIONAL: TcxGridDBColumn;
    GridOPCOD_ECONOMIC: TcxGridDBColumn;
    GridOPNR_OP: TcxGridDBColumn;
    GridOPDATA_OP: TcxGridDBColumn;
    GridOPTIP_NOTA: TcxGridDBColumn;
    BtnAdaugaFactura: TcxButton;
    BtnRemoveFactura: TcxButton;
    BtnAnulareOP: TcxButton;
    BtnSalvare: TcxButton;
    btnCancel: TcxButton;
    edContDebit: TcxPopupEdit;
    edContCredit: TcxPopupEdit;
    edDataOrdin: TcxDateEdit;
    edNrUnic: TcxTextEdit;
    edData: TcxDateEdit;
    edListaLuni: TcxImageComboBox;
    edOperator: TcxImageComboBox;
    edListaAni: TcxImageComboBox;
    edNrOrdin: TcxImageComboBox;
    edJurnal: TcxImageComboBox;
    edGrupGridOp: TcxImageComboBox;
    edNrNota: TcxTextEdit;
    pmGridIstoricNote: TcxGridPopupMenu;
    pmGridOP: TcxGridPopupMenu;
    GridOPNR: TcxGridDBColumn;
    Splitter1: TcxSplitter;
    procedure FormCreate(Sender: TObject);
    procedure pnClientResize(Sender: TObject);
    procedure BtnAdaugaFacturaClick(Sender: TObject);
    procedure BtnRemoveFacturaClick(Sender: TObject);
    procedure TblNotaBeforeDelete(DataSet: TDataSet);
    procedure FormCloseQuery(Sender: TObject; var CanClose: Boolean);
    procedure BtnOkClick(Sender: TObject);
    procedure TblNotaAfterOpen(DataSet: TDataSet);
    procedure InternalValidateCont(Val: String; AColIndex: Integer; Tree: TcxDBTreeList);
    function  GetNodeByVal(ATree: TcxDBTreeList; AColIndex: Integer; AVal: String): TcxTreeListNode;
    procedure TblNotaNewRecord(DataSet: TDataSet);
    procedure TblNotaAfterPost(DataSet: TDataSet);
    procedure SetGridEnabled(AEnabled: Boolean);
    procedure BtnAnulareOPClick(Sender: TObject);
    procedure QryOPHAfterOpen(DataSet: TDataSet);
    procedure GridIstoricNoteDataControllerFilterBeforeChange(
      Sender: TcxDBDataFilterCriteria; ADataSet: TDataSet; const AFilterText: String);
    procedure GridOPFocusedRecordChanged(Sender: TcxCustomGridTableView;
      APrevFocusedRecord, AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure cxTreeCreditROMANAGetDisplayText(Sender: TcxTreeListColumn;
      ANode: TcxTreeListNode; var Value: String);
    procedure cxTreeDebitROMANAGetDisplayText(Sender: TcxTreeListColumn;
      ANode: TcxTreeListNode; var Value: String);
    procedure edContDebitPropertiesPopup(Sender: TObject);
    procedure edContCreditPropertiesPopup(Sender: TObject);
    procedure edContDebitPropertiesCloseUp(Sender: TObject);
    procedure edContCreditPropertiesCloseUp(Sender: TObject);
    procedure GridOPCOD_FUNCTIONALPropertiesCloseUp(Sender: TObject);
    procedure GridOPCOD_ECONOMICPropertiesCloseUp(Sender: TObject);
    procedure cxTreeEconomicDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure cxTreeFunctionalDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure edGrupGridOpPropertiesChange(Sender: TObject);
    procedure cxTreeDebitDblClick(Sender: TObject);
    procedure cxTreeDebitKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure edListaAniPropertiesChange(Sender: TObject);
    procedure edNrOrdinPropertiesChange(Sender: TObject);
    procedure GridOPCOD_ECONOMICPropertiesPopup(Sender: TObject);
    procedure GridOPCOD_FUNCTIONALPropertiesPopup(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure edDataOrdinPropertiesChange(Sender: TObject);
    procedure edDataOrdinPropertiesEditValueChanged(Sender: TObject);
    procedure btnCancelClick(Sender: TObject);
    procedure edNrUnicPropertiesChange(Sender: TObject);
  private
    { Private declarations }

    FIsLoading :  Boolean;
    FIsInitalizare : Boolean;
    TipuriDocPlata,
    Conturi   : TdxMemData;
    OldFilter : String;
    FContOP: String;
    FDataOp: TDateTime;
    FDrepturi: Integer;
    procedure SetUserRights;
    procedure PostModificareCont;
    procedure RefreshFilter;
    procedure SetFirstEntry(AEdit: TcxPopupEdit; ATree: TcxDBTreeList);
    procedure ValidateCodEconomic(Sender: TField);
    procedure ValidareTipDocument(Sender: TField);
    procedure AplicaContCreditor(ClasaEconomica, ClasaFunctionala: String);
    procedure AplicaContare(TipDocument: Integer; Gestiune: Integer);
    procedure UpdateConturi;
    procedure SetContOP(const Value: String);
    procedure SetDataOP(const Value: TDateTime);
    procedure SetDrepturi(const Value: Integer);
    procedure AnulareOp(DataSet : TZReadOnlyQuery);
  protected
    procedure ValidareCodGest(Sender: TField);
  public
    { Public declarations }
    procedure WriteToDb;
    procedure InitFields;
    procedure TestOp;
    property  ContOP: String read FContOP write SetContOP;
    property  DataOP: TDateTime read FDataOp write SetDataOP;
    property  Drepturi : Integer read FDrepturi write SetDrepturi;
  end;


  procedure DoImperechere;
  procedure DoListare;

implementation

{$R *.DFM}

uses
  dxCompsUtile, ZeosDBUtile, CommonDBVar, DateUnit, Variants, FormulareUnit, ATSZDBUtils, PersistGridSettings;


procedure DoListare;
var aForm : TCustomForm;
begin
 aForm := GetNewForm(TfrmGenerarePlata, 'Generare Ordin de Plata');
 with TfrmGenerarePlata(aForm) do
   try
      Drepturi := 2;
      WindowState := wsMaximized;
      Show;
   finally
   end;
end;


procedure DoImperechere;
var aForm : TCustomForm;
begin
 aForm := GetNewForm(TfrmGenerarePlata, 'Generare Ordin de Plata');
 with TfrmGenerarePlata(aForm) do
   try
      WindowState := wsMaximized;
      Show;
   finally
   end;
end;


procedure TfrmGenerarePlata.RefreshFilter;
const
  cst_Filter = '%s and ISNULL(TIP_NOTA, 1) = %d';
var
  lFilter  : String;

  procedure AdaugaFiltru(const AFiltru: String);
  begin
    if lFilter > '' then lFilter := lFilter + ' AND ';
    lFilter := lFilter + AFiltru;
  end;

  procedure Add2Filter(const AFormat: String; const AEdit: TcxCustomEdit; const AIsExact: Boolean = True);
  var
    lValue: String;
  begin
    if ValueHasValue(AEdit.EditingValue) then
      if AIsExact then
        lValue := ValueToStr(AEdit.EditingValue)
      else
        lValue := ValueToStr(ValueToStr(AEdit.EditingValue, False) + '%')
    else
    if ValueHasValue(AEdit.EditValue) then
      if AIsExact then
        lValue := ValueToStr(AEdit.EditValue)
      else
        lValue := ValueToStr(ValueToStr(AEdit.EditValue, False) + '%')
    else
      lValue := '';
    if lValue > '' then
      AdaugaFiltru(Format(AFormat, [lValue]));
  end;


begin
  if FIsInitalizare then Exit;
  lFilter := GridIstoricNote.DataController.Filter.FilterText;
  Add2Filter('NR_OP like %s', edNrUnic, False);
  Add2Filter('YEAR(DATA) = %s', edListaAni);
  Add2Filter('MONTH(DATA) = %s', edListaLuni);
  Add2Filter('NRDOC like %s', edNrNota, False);
  Add2Filter('DATA = convert(datetime, %s, 120)', edData);
  Add2Filter('C_O = %s', edOperator);
  AdaugaFiltru('STARE = 1');
  lFilter := 'WHERE ' + lFilter;

  if OldFilter <> lFilter then begin
     if QryFacturi.Active then QryFacturi.Active := False;
     QryFacturi.Sql[1]  := Format(cst_Filter, [lFilter, 1]);
     QryOPH.Sql[1]      := Format(cst_Filter, [lFilter, 2]);
     try
       QryFacturi.Active  := True;
       QryOPH.Active      := True;
       OldFilter          := lFilter;
     except
       QryFacturi.Sql[1]  := Format(cst_Filter, [OldFilter, 1]);
       QryOPH.Sql[1]      := Format(cst_Filter, [OldFilter, 2]);
       QryFacturi.Active  := True;
       QryOPH.Active      := True;
     end;
  end;
end;

procedure TfrmGenerarePlata.WriteToDb;
begin
{}
end;

procedure LoadRepartitori(ARow: TcxGridDBColumn; Intern: Integer);
var OldPoz : TBookmark;
    lValField,
    lDescField: TField;
    lItems : TcxImageComboBoxItems;
begin
  if not (ARow.Properties is TcxImageComboBoxProperties) then Exit;
  lItems := TcxImageComboBoxProperties(ARow.Properties).Items;

  lItems.BeginUpdate;
  try
    lItems.Clear;

    lValField := FrmData.QryRepartitori.FindField('ID_REPARTITORI');
    lDescField := FrmData.QryRepartitori.FindField('NUME');
    with FrmData.QryRepartitori do begin
      OldPoz := GetBookmark;
      DisableControls;
      try
         First;
         while not Eof do begin
           {if (Intern = 2) or
              ( (Intern = 0) and (not FrmData.QryRepartitori.FieldByName('GESTINT').AsBoolean)) or
              ( (Intern = 1) and (FrmData.QryRepartitori.FieldByName('GESTINT').AsBoolean)) then begin}
              with lItems.Add as TcxImageComboBoxItem do begin
                Value := lValField.AsString;
                Description := lDescField.AsString;
              end;
           //end;
           Next;
         end;
      finally
         GotoBookmark(OldPoz);
         FreeBookmark(OldPoz);
         EnableControls;
      end;
    end;
  finally
    lItems.EndUpdate;
  end;
end;

procedure TfrmGenerarePlata.FormCreate(Sender: TObject);
var lFakeQry : TZReadOnlyQuery;
begin
  InitFields;

  FIsInitalizare := True;
  SetUserRights;
  FDataOp := 0;
  Conturi := TdxMemData.Create(Self);
  TipuriDocPlata := TdxMemData.Create(Self);

  lFakeQry := GetTmpADOQuery;
  try
     lFakeQry.Sql.Add('exec sp_get_mapare_conturi_ordine_plata');
     lFakeQry.Open;
     Conturi.LoadFromDataSet(lFakeQry);
     lFakeQry.Close;
     lFakeQry.Sql.Clear;
     lFakeQry.Sql.Add('exec spNoteTipuriDocPlata');
     lFakeQry.Open;
     TipuriDocPlata.LoadFromDataSet(lFakeQry);
     lFakeQry.Close;
     lFakeQry.Sql.Clear;
     lFakeQry.Sql.Add('exec spNoteLstExtrase');
     try
        lFakeQry.Open;
        FillImageCombo(edNrOrdin.Properties, lFakeQry, 'CONT_EXTRAS', 'DENUMIRE');
     except
     end;
  finally
    lFakeQry.Free;
  end;

  QryCredit.Params[0].Value := '%';
  QryCredit.Open;
  SetFirstEntry(edContCredit, cxTreeCredit);

  QryDebit.Params[0].Value := '%';
  QryDebit.Open;
  SetFirstEntry(edContDebit, cxTreeDebit);

  FillImageCombo(GridOPTIP_DOCUMENT.Properties, TipuriDocPlata, 'ID', 'DENUMIRE');

  FillImageCombo(GridIstoricNoteC_O.Properties, frmData.QryOperatori, 'ID_UTILIZATORI', 'NUMEINTREG');

(*
  LoadRepartitori(GridIstoricNoteREPARTITOR_CREDIT, 0);
  LoadRepartitori(GridIstoricNoteREPARTITOR_DEBIT, 1);
  LoadRepartitori(GridOPREPARTITOR_CREDIT, 1);
  LoadRepartitori(GridOPREPARTITOR_DEBIT, 0);
*)

  LoadRepartitori(GridIstoricNoteREPARTITOR_CREDIT, 2);

{  PopulatecxImage(FrmData.QryRepartitori,
                TcxImageComboBoxProperties(GridIstoricNoteREPARTITOR_CREDIT.Properties).Items,
                'ID_REPARTITORI', 'NUME');}

  GridIstoricNoteREPARTITOR_DEBIT.Properties.Assign(GridIstoricNoteREPARTITOR_CREDIT.Properties);
  GridOPREPARTITOR_CREDIT.Properties.Assign(GridIstoricNoteREPARTITOR_CREDIT.Properties);
  GridOPREPARTITOR_DEBIT.Properties.Assign(GridIstoricNoteREPARTITOR_CREDIT.Properties);

  GridOPREPARTITOR_CREDIT.Properties.ReadOnly := False;
  GridOPREPARTITOR_DEBIT.Properties.ReadOnly := False;

  FillImageCombo(GridIstoricNoteJURNAL.Properties, 'select jurnal, denumire from cjurnale order by denumire', 0, 1);
  edJurnal.Properties.Assign(GridIstoricNoteJURNAL.Properties);

  if edJurnal.Properties.FindItemByValue('INCASARI') <> nil then
     edJurnal.ItemIndex := edJurnal.Properties.FindItemByValue('INCASARI').Index;

  edOperator.Properties.Assign(GridIstoricNoteC_O.Properties);
  edOperator.Properties.ReadOnly := False;
  edOperator.Properties.OnChange := edNrUnicPropertiesChange;
  with edOperator.Properties.Items.Add do begin
    Value := '0';
    Description := 'Toti Operatorii';
    edOperator.ItemIndex := Index;
  end;

  FillImageCombo(edListaAni.Properties, 'exec [spNoteGetAni]', 0, 0, 0, 'Toti Anii');
  FIsInitalizare := False;
  if edListaAni.Properties.Items.Count > 0 then edListaAni.ItemIndex := edListaAni.Properties.Items.Count-1;

  GridIstoricNote.RestoreFromStorage(Self.Name + '.' + GridIstoricNote.Name, TcxDBIniFileReader);
  GridOP.RestoreFromStorage(Self.Name + '.' + GridOP.Name, TcxDBIniFileReader);
end;

procedure TfrmGenerarePlata.pnClientResize(Sender: TObject);
begin
  GRTop.Height := (pnClient.Height - pnClient.BorderWidth * 2) div 2;
end;

procedure TfrmGenerarePlata.InitFields;
begin
//  ATENTIE
  if QryFacturi.Active then QryFacturi.Close;
  QryFacturi.SQL[1] := 'where 1=0';
  QryFacturi.Open;
  DefaultFieldClasses[ftAutoInc] := TIntegerField;
  TblNota.CreateFieldsFromDataSet(QryFacturi);
  DefaultFieldClasses[ftAutoInc] := TAutoIncField;
  TblNota.Open;
  QryFacturi.SQL[1] := '';  
end;

procedure TfrmGenerarePlata.BtnAdaugaFacturaClick(Sender: TObject);
var
  I : Integer;
  AField : TField;
  lRepDebit, lRepCredit : Variant;
begin
  if TblNota.Locate('NR', QryFacturi.FieldByName('NR').AsInteger, []) then begin
     MessageDlg('Factura este deja selectata !', mtError, [mbOk], 0);
     Abort;
  end;
  if Trim(VarToStr(edNrOrdin.EditValue)) = '' then begin
     edNrOrdin.SetFocus;
     raise EContaHandledError.Create('Introduceti contul de extras !');
  end;
  if edDataOrdin.Date <= 0 then begin
     edDataOrdin.SetFocus;
     raise EContaHandledError.Create('Introduceti data ordinului de plata !');
  end;
  GridOP.BeginUpdate;
  TblNota.DisableControls;
  try
     if not TblNota.Active then TblNota.Open;
     TblNota.Append;
     TblNota.FieldByName('TIP_DOCUMENT').OnValidate := nil;
     for i := 0 to QryFacturi.FieldCount - 1 do begin
      AField := TblNota.FindField(QryFacturi.Fields[i].FieldName);
      if (AField <> nil) and (AField <> TblNota.RecIdField) then
         AField.Value := QryFacturi.Fields[i].Value;
     end;
     TblNota.FieldByName('TIP_DOCUMENT').OnValidate := ValidareTipDocument;
     TblNota.FieldByName('ECL').AsInteger := 1;
     TblNota.FieldByName('TIP_NOTA').AsInteger := 2;
     TblNota.FieldByName('DATA').Value := edDataOrdin.Date;
     TblNota.FieldByName('C_O').Value := IdUtilizator;
     if (edContDebit.Text = TblNota.FieldByName('CONTC').AsString) or
        (edContCredit.Text = TblNota.FieldByName('CONTD').AsString) then
     begin
        lRepDebit := TblNota.FieldByName('REPARTITOR_DEBIT').Value;
        lRepCredit := TblNota.FieldByName('REPARTITOR_CREDIT').Value;
        TblNota.FieldByName('REPARTITOR_DEBIT').Value := lRepCredit;
        TblNota.FieldByName('REPARTITOR_CREDIT').Value := lRepDebit;
     end;
     TblNota.FieldByName('CONTC').AsString  := edContCredit.Text;
     TblNota.FieldByName('CONT_CRED').AsString := edContCredit.Text;
     TblNota.FieldByName('CONTD').AsString := edContDebit.Text;
     TblNota.FieldByName('CONT_DEBT').AsString := edContDebit.Text;
     if TcxImageComboBoxProperties(GridOPTIP_DOCUMENT.Properties).Items.Count > 0 then
       TblNota.FieldByName('TIP_DOCUMENT').AsString := VarToStr(TcxImageComboBoxProperties(GridOPTIP_DOCUMENT.Properties).Items[0].Value);
     TblNota.Post;
  finally
    TblNota.EnableControls;
    GridOP.EndUpdate;
  end;
end;

procedure TfrmGenerarePlata.BtnRemoveFacturaClick(Sender: TObject);
begin
  if not TblNota.IsEmpty then
     TblNota.Delete;
end;

procedure TfrmGenerarePlata.TblNotaBeforeDelete(DataSet: TDataSet);
begin
  if MessageDlg('Doriti sa stergeti factura asociata ordinului de plata ?', mtConfirmation, [mbYes, mbNo], 0) <> mrYes then Abort;
end;

procedure TfrmGenerarePlata.FormCloseQuery(Sender: TObject;
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

procedure TfrmGenerarePlata.SetFirstEntry(AEdit: TcxPopupEdit; ATree: TcxDBTreeList);
var lNode: TcxDBTreeListNode;

  function GetLastChild(ANode: TcxDBTreeListNode): TcxDBTreeListNode;
   begin
     if ANode.HasChildren then
        Result := GetLastChild(TcxDBTreeListNode(ANode.Items[0]))
     else Result := ANode;
   end;

begin
  if ATree.Count > 0 then
     lNode := GetLastChild(TcxDBTreeListNode(ATree.Items[0]))
  else lNode := nil;
  if Assigned(lNode) then AEdit.Text := lNode.Texts[0];
end;

procedure TfrmGenerarePlata.BtnOkClick(Sender: TObject);
var lNrOP: Integer;
    lNrNota: Integer;
    lOrdin : String;
    lDataOrdin : TDatetime;
    lQry : TZReadOnlyQuery;

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
    lQry := GetTmpADOQuery;
    with lQry do
      try
         AnulareOp(lQry);
         Sql.Clear;
         lNrOP := GetNextId('ORDINE_PLATA');
         lOrdin := Trim(VarToStr(edNrOrdin.EditValue));
         lDataOrdin := edDataOrdin.Date;
         { Luam ID-ul OP-ului }
         Sql.Add('INSERT INTO ORDINE_PLATA (ID_ORDINE_PLATA, NR_ORDIN, DATA_ORDIN, CONT_DEBIT, CONT_CREDIT, ID_UTILIZATORI, STARE)');
         Sql.Add('VALUES (:ID_ORDINE_PLATA, :NR_ORDIN, :DATA_ORDIN, :CONT_DEBIT, :CONT_CREDIT, :ID_UTILIZATORI, 1)');
         Params[0].Value := lNrOp;
         Params[1].Value := lOrdin;
         Params[2].Value := lDataOrdin;
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
         Sql.Add('VALUES            (:XCOD, :TIP_DOCUMENT, :RECID, :XJURNAL, :NRDOC, :XDATADOC, :EXPLICATIE, :VALOARE, :ECL, :XC_O, :CONTD, :CONTC, :CONT_DEBT, :CONT_CRED,  @DATA, :REPARTITOR_CREDIT, 1      ,');
         Sql.Add(':COD_FUNCTIONAL, :COD_ECONOMIC, :NR_OP, :DATA_OP, :REPARTITOR_DEBIT, :XID_ORDIN, :NR_OP, :NRDOC, :TIP_NOTA)');
         DataSource := DTOp;
         Params.ParamByName('XCOD').Value      := lNrNota;
         Params.ParamByName('XJURNAL').Value   := Trim(VarToStr(edJurnal.EditValue));
         Params.ParamByName('XDATADOC').Value  := edDataOrdin.Date;
         Params.ParamByName('XC_O').Value      := IdUtilizator;
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

procedure TfrmGenerarePlata.TblNotaAfterOpen(DataSet: TDataSet);
{
var
  lField: TField;
}
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

  {
  lField := DataSet.FindField('REPARTITOR_CREDIT');
  if Assigned(lField) then begin
    lField.OnValidate := ValidareCodGest;
  end;

  lField := DataSet.FindField('REPARTITOR_DEBIT');
  if Assigned(lField) then begin
    lField.OnValidate := ValidareCodGest;
  end;

  lField := DataSet.FindField('CODGEST');
  if Assigned(lField) then begin
    lField.OnValidate := ValidareCodGest;
  end;
  }
end;

procedure TfrmGenerarePlata.ValidateCodEconomic(Sender: TField);
begin
  if Sender.Tag = 0 then begin
     InternalValidateCont(Trim(Sender.AsString), cxTreeFunctionalCOD_BUGET.ItemIndex, cxTreeFunctional);
     AplicaContCreditor(Trim(TblNota.FieldByName('COD_ECONOMIC').AsString), Trim(Sender.AsString));
  end
  else begin
    InternalValidateCont(Trim(Sender.AsString), cxTreeEconomicCOD_BUGET.ItemIndex, cxTreeEconomic);
    { Schimbam si contul de credit in functie de clasificatia economica }
    AplicaContCreditor(Trim(Sender.AsString), Trim(TblNota.FieldByName('COD_FUNCTIONAL').AsString));
  end;
end;

procedure TfrmGenerarePlata.InternalValidateCont(Val: String;
  AColIndex: Integer; Tree: TcxDBTreeList);
var MustDrop: Boolean;
    SelNode : TcxTreeListNode;
begin
  Tree.CancelSearching;
  { Se valideaza contul de buget introdus }
  MustDrop := (Val = '') or (Val = '?');
  { Incercam sa gasim nodul posibil }
  SelNode := nil;
  if not MustDrop then begin
     if AColIndex = -1 then begin
        SelNode := TcxDBTreeList(Tree).FindNodeByKeyValue(Val, nil);
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

  if (MustDrop) and (Assigned(GridOP.Controller.EditingController.Edit)) then begin
      if Assigned(SelNode) then begin
         SelNode.MakeVisible;
         SelNode.Focused := True;
      end
      else Tree.SearchingText := Val;
      if (GridOP.Controller.EditingController <> nil) and
         (GridOP.Controller.EditingController.Edit <> nil)
         and (GridOP.Controller.EditingController.Edit is TcxPopupEdit) then
        TcxPopupEdit(GridOP.Controller.EditingController.Edit).DroppedDown := True;
      //SendMessage(GridOP.Controller.EditingController.Edit.Handle, CM_DROPDOWNPOPUPFORM, 0, 0);
      Abort;
  end;
end;


function TfrmGenerarePlata.GetNodeByVal(ATree: TcxDBTreeList; AColIndex: Integer; AVal: String): TcxTreeListNode;
begin
  Result := ATree.FindNodeByText(AVal, ATree.Columns[AColIndex]);
end;

procedure TfrmGenerarePlata.TblNotaNewRecord(DataSet: TDataSet);
begin
    if Trim(VarToStr(edNrOrdin.EditValue)) = '' then begin
     edNrOrdin.SetFocus;
     raise EContaHandledError.Create('Introduceti contul de extras !');
  end;
  if edDataOrdin.Date <= 0 then begin
     edDataOrdin.SetFocus;
     raise EContaHandledError.Create('Introduceti data ordinului de plata !');
  end;
  if Drepturi <> 2 then begin
    DataSet.FieldByName('ECL').AsInteger := 1;
    DataSet.FieldByName('TIP_NOTA').AsInteger := 2;
    DataSet.FieldByName('DATA').Value := edDataOrdin.Date;
    DataSet.FieldByName('C_O').Value := IdUtilizator;
    DataSet.FieldByName('CONTC').AsString  := edContCredit.Text;
    DataSet.FieldByName('CONT_CRED').AsString := edContCredit.Text;
    DataSet.FieldByName('CONTD').AsString := edContDebit.Text;
    DataSet.FieldByName('CONT_DEBT').AsString := edContDebit.Text;
    if DataSet.FieldByName('NR_OP') <> nil then
       DataSet.FieldByName('NR_OP').AsInteger := GetNextId('POZITIE_NOTA');
  end;
  SetGridEnabled(True);
end;

procedure TfrmGenerarePlata.AplicaContCreditor(ClasaEconomica, ClasaFunctionala: String);
var lFound: Boolean;
    lCont : String;
begin
  Exit;
  if pos('700', Trim(TblNota.FieldByName('CONTC').AsString)) = 1 then begin
     with Conturi do begin
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
     with Conturi do begin
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

procedure TfrmGenerarePlata.TblNotaAfterPost(DataSet: TDataSet);
begin
  GridOP.ApplyBestFit(nil);
  if (FDrepturi = 2) and (not FIsLoading) then PostModificareCont;
end;

procedure TfrmGenerarePlata.TestOp;
var
  FakeQry: TZReadOnlyQuery;
  lMustAsk: Boolean;
begin
  if FIsLoading then Exit;
  FIsLoading := True;
  try
    { Testam OP -ul }
    if (Trim(VarToStr(edNrOrdin.EditValue)) > '') and (edDataOrdin.Date > 0)
       and ((Trim(VarToStr(edNrOrdin.EditValue)) <> Trim(FContOP)) or (FDataOp <> edDataOrdin.Date)) then begin
       lMustAsk := (Trim(FContOP) > '') and (FDataOp > 0)
                   and (FDrepturi <> 2)
                   and (TblNota.Active) and (not TblNota.IsEmpty);
       if lMustAsk then
        if MessageDlg('Aveti deja introdus ordinul de plata '+Trim(FContOP)+' din '+FormatDateTime('dd.mm.yyyy', FDataOp)+#13#10+
                      'Doriti procesarea unui nou extras?'+#13#10+
                      'Informatiile din ecran vor fi pierdute', mtConfirmation, [mbYes, mbNo], 0) <> mrYes then begin
           edNrOrdin.EditValue := Trim(FContOP);
           edDataOrdin.Date := FDataOp;
           Abort;
        end;

       //testam sa vedem daca avem op-uri
       FakeQry := GetTmpADOQuery;
       with FakeQry do
         try
            TblNota.Active := False;
            TblNota.Active := True;
            FContOP := Trim(VarToStr(edNrOrdin.EditValue));
            FDataOp := edDataOrdin.Date;

            Sql.Add('exec spNoteTestOp :NR_ORDIN , :DATA');
            Params[0].Value := FContOP;
            Params[1].Value := FDataOp;
            Open;
            if not IsEmpty then begin
               Close;
               Sql.Clear;
               Sql.Add('exec spNoteLoadOp :NR_ORDIN , :DATA');
               Params[0].Value := FContOP;
               Params[1].Value := FDataOp;
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
       cxGridOP.SetFocus;
    end;
  finally
    FIsLoading := False;
  end;
end;

procedure TfrmGenerarePlata.SetGridEnabled(AEnabled: Boolean);
var
  I: Integer;
begin
  if FDrepturi = 2 then begin
     for I := 0 to GridOP.ColumnCount-1 do
       GridOP.Columns[I].Options.Editing := False;

     GridOPCOD_FUNCTIONAL.Options.Editing := True;
     GridOPCOD_ECONOMIC.Options.Editing   := True;

     GridOPTIP_DOCUMENT.Options.Editing := True;
     GridOPNRDOC.Options.Editing        := True;
     GridOPVALOARE.Options.Editing      := True;
     GridOPEXPLICATIE.Options.Editing   := True;

     GridOPCONTD.Options.Editing   := True;
     GridOPCONTC.Options.Editing   := True;
     GridOPREPARTITOR_DEBIT.Options.Editing   := True;
     GridOPREPARTITOR_CREDIT.Options.Editing   := True;
  end
  else begin
    GridOPDATA.Options.Editing   := AEnabled;
    GridOPVALOARE.Options.Editing   := True;
    GridOPTIP_DOCUMENT.Options.Editing   := True;
    GridOPNRDOC.Options.Editing   := True;
    GridOPEXPLICATIE.Options.Editing   := True;
    GridOPREPARTITOR_DEBIT.Options.Editing   := True;
    GridOPREPARTITOR_CREDIT.Options.Editing   := True;
  end;
end;

procedure TfrmGenerarePlata.ValidareTipDocument(Sender: TField);
begin
  AplicaContare(TblNota.FieldByName('TIP_DOCUMENT').AsInteger, Sender.AsInteger);
end;

procedure TfrmGenerarePlata.SetContOP(const Value: String);
begin
  FContOP := Value;
end;

procedure TfrmGenerarePlata.SetDataOP(const Value: TDateTime);
begin
  FDataOp := Value;
end;

procedure TfrmGenerarePlata.SetUserRights;
begin
  { Setam Drepturile de access }
  with GetTmpADOQuery do
    try
       Sql.Add('exec spNoteOpRights '+IntToStr(IdUtilizator));
       Open;
       Drepturi := Fields[0].AsInteger;
    finally
       Free;
    end;
end;

procedure TfrmGenerarePlata.SetDrepturi(const Value: Integer);
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

procedure TfrmGenerarePlata.PostModificareCont;
begin
  { Salvam modificarile de conturi }
  //MODIFICARE 22.01.2007 Cristi am adaugat BUGET_NR_OP pentru ca in caz ca se intra pe modificare nota contabila nrdoc devine primul nrdoc din nota respectiva 
  with GetTmpADOQuery do
    try
       DataSource := DTOp;
       Sql.Add('UPDATE CNOTE SET');
       Sql.Add('VALOARE = :VALOARE, EXPLICATIE = :EXPLICATIE, NRDOC = :NRDOC, BUGET_NR_OP = ISNULL(BUGET_NR_OP, :NRDOC), TIP_DOCUMENT = :TIP_DOCUMENT, COD_ECONOMIC = :COD_ECONOMIC, COD_FUNCTIONAL = :COD_FUNCTIONAL,');
       Sql.Add('CONTD = :CONTD, REPARTITOR_CREDIT = :REPARTITOR_CREDIT, REPARTITOR_DEBIT = :REPARTITOR_DEBIT, CONTC = :CONTC, CONT_DEBT = :CONT_DEBT , CONT_CRED = :CONT_CRED');
       Sql.Add('WHERE NR = :NR');
       ExecSQL;
    finally
       Free;
    end;
end;

procedure TfrmGenerarePlata.BtnAnulareOPClick(Sender: TObject);
var
  lQry : TZReadOnlyQuery;
begin
  if (Trim(VarToStr(edNrOrdin.EditValue)) > '') and (Trim(edDataOrdin.Text) > '') and IsValidDate(edDataOrdin.EditValue) then
    if MessageDlg('Doriti anularea ordinului de plata '+ Trim(VarToStr(edNrOrdin.EditValue)) +' din '+ FormatDatetime('dd.mm.yyyy', edDataOrdin.Date),
                  mtConfirmation, [mbYes, mbNo], 0) = mrYes then begin
      lQry := GetTmpADOQuery;
      AnulareOp(lQry);
      TestOp;
      lQry.Free;
    end;
end;

procedure TfrmGenerarePlata.ValidareCodGest(Sender: TField);
begin
  AplicaContare(TblNota.FieldByName('TIP_DOCUMENT').AsInteger, Sender.AsInteger);
end;

procedure TfrmGenerarePlata.AplicaContare(TipDocument: Integer; Gestiune: Integer);
var
  lNewContD,
  lNewContC: String;
  lTipExtras: String;
  lFound    : Boolean;
  lIsRetur  : Boolean;
  lRepDebit, lRepCredit : Variant;
begin
  if FDrepturi = 2 then Exit;

  with TipuriDocPlata do
    if Locate('ID', TipDocument, []) then
      lIsRetur := FieldByName('SEMN').AsInteger = -1
    else lIsRetur := False;

  lTipExtras := Trim(VarToStr(edNrOrdin.EditValue));
  lNewContD := '';
  lNewContC := '';
  // In primul pas aflam modalitatea de contare

  with Conturi do begin
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
        if (lNewContD = TblNota.FieldByName('CONTC').AsString) or (lNewContC = TblNota.FieldByName('CONTD').AsString) then begin
          lRepDebit := TblNota.FieldByName('REPARTITOR_DEBIT').Value;
          lRepCredit := TblNota.FieldByName('REPARTITOR_CREDIT').Value;
          TblNota.FieldByName('REPARTITOR_DEBIT').Value := lRepCredit;
          TblNota.FieldByName('REPARTITOR_CREDIT').Value := lRepDebit;
        end;
        TblNota.FieldByName('CONTD').AsString := lNewContD;
        TblNota.FieldByName('CONT_DEBT').AsString := lNewContD;
        TblNota.FieldByName('CONTC').AsString := lNewContC;
        TblNota.FieldByName('CONT_CRED').AsString := lNewContC;
      end;

    end;
  end;
end;

procedure TfrmGenerarePlata.UpdateConturi;
var
  lTipExtras : String;
begin
  lTipExtras := Trim(VarToStr(edNrOrdin.EditValue));
  with Conturi do begin
    if Locate('CONT_EXTRAS', lTipExtras, []) then begin
      edContDebit.EditValue  := FieldByName('CONT_DEBIT').AsString;
      edContCredit.EditValue := FieldByName('CONT_CREDIT').AsString;
    end
    else begin
      edContDebit.EditValue  := '';
      edContCredit.EditValue := '';
    end;
  end;
end;


procedure TfrmGenerarePlata.QryOPHAfterOpen(DataSet: TDataSet);
begin
  // populam tabela de note daca avem oph pentru factura curenta
  //TblNota.LoadFromDataSet(QryOPH);
end;

procedure TfrmGenerarePlata.GridIstoricNoteDataControllerFilterBeforeChange(
  Sender: TcxDBDataFilterCriteria; ADataSet: TDataSet;
  const AFilterText: String);
begin
  RefreshFilter;
end;

procedure TfrmGenerarePlata.GridOPFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
var lTipNota: String;
begin
  if AFocusedRecord = nil then Exit;
  if not AFocusedRecord.IsData then Exit;
  lTipNota := VarToStr(AFocusedRecord.Values[GridOPTIP_NOTA.Index]);
  if lTipNota = '2' then
     SetGridEnabled(True)
  else SetGridEnabled(False);
end;


procedure TfrmGenerarePlata.cxTreeCreditROMANAGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Values[cxTreeCreditCONT.ItemIndex] + ' : ' + ANode.Values[cxTreeCreditROMANA.ItemIndex];
end;

procedure TfrmGenerarePlata.cxTreeDebitROMANAGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Values[cxTreeDebitCONT.ItemIndex] + ' : ' + ANode.Values[cxTreeDebitROMANA.ItemIndex];
end;

procedure TfrmGenerarePlata.edContDebitPropertiesPopup(Sender: TObject);
var lNode : TcxDBTreeListNode;
begin
  lNode := TcxDBTreeListNode(cxTreeDebit.FindNodeByKeyValue(edContDebit.EditText, nil));
  if Assigned(lNode) then lNode.Focused := True;
end;

procedure TfrmGenerarePlata.edContCreditPropertiesPopup(Sender: TObject);
var lNode : TcxDBTreeListNode;
begin
  lNode := TcxDBTreeListNode(cxTreeCredit.FindNodeByKeyValue(edContCredit.EditText, nil));
  if Assigned(lNode) then lNode.Focused := True;
end;

type
  TAccesscxPopupEdit = class(TcxPopupEdit);

procedure TfrmGenerarePlata.edContDebitPropertiesCloseUp(Sender: TObject);
var lNode : TcxDBTreeListNode;
begin
  if TAccesscxPopupEdit(edContDebit).PopupWindow.ModalResult = mrOk then begin
     lNode := TcxDBTreeListNode(cxTreeDebit.FocusedNode);
     edContDebit.Properties.HideSelection := False;
     if Assigned(lNode) then edContDebit.Text := lNode.KeyValue
     else edContDebit.Text := '';
     edContDebit.Properties.HideSelection := True;
  end;
end;


procedure TfrmGenerarePlata.edContCreditPropertiesCloseUp(Sender: TObject);
var lNode : TcxDBTreeListNode;
begin
  if TAccesscxPopupEdit(edContCredit).PopupWindow.ModalResult = mrOk then begin
     lNode := TcxDBTreeListNode(cxTreeCredit.FocusedNode);
     edContCredit.Properties.HideSelection := False;
     if Assigned(lNode) then edContCredit.Text := lNode.KeyValue
     else edContCredit.Text := '';
     edContCredit.Properties.HideSelection := True;
  end;
end;

procedure TfrmGenerarePlata.GridOPCOD_FUNCTIONALPropertiesCloseUp(
  Sender: TObject);
var lNode: TcxDBTreeListNode;
begin
  if not (Sender is TcxPopupEdit) then Exit;
  with GridOPCOD_FUNCTIONAL do
    if TAccesscxPopupEdit(Sender).PopupWindow.ModalResult = mrOk then begin
       lNode := TcxDBTreeListNode(cxTreeFunctional.FocusedNode);
       if Assigned(lNode) then
         DBSetFieldValue(GridOP.DataController.DataSet, DataBinding.FieldName, lNode.Values[cxTreeFunctionalCOD_BUGET.ItemIndex] );
    end;
end;

procedure TfrmGenerarePlata.GridOPCOD_ECONOMICPropertiesCloseUp(
  Sender: TObject);
var lNode: TcxDBTreeListNode;
begin
  if not (Sender is TcxPopupEdit) then Exit;
  with GridOPCOD_ECONOMIC do
    if TAccesscxPopupEdit(Sender).PopupWindow.ModalResult = mrOk then begin
       lNode := TcxDBTreeListNode(cxTreeEconomic.FocusedNode);
       if Assigned(lNode) then
         DBSetFieldValue(GridOP.DataController.DataSet, DataBinding.FieldName, lNode.Values[cxTreeEconomicCOD_BUGET.ItemIndex] );
    end;
end;

procedure TfrmGenerarePlata.cxTreeEconomicDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := Trim(ANode.Texts[cxTreeEconomicCOD_BUGET.ItemIndex])+' : '+Trim(ANode.Texts[cxTreeEconomicDENUMIRE.ItemIndex]);
end;

procedure TfrmGenerarePlata.cxTreeFunctionalDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := Trim(ANode.Texts[cxTreeFunctionalCOD_BUGET.ItemIndex])+' : '+Trim(ANode.Texts[cxTreeFunctionalDENUMIRE.ItemIndex]);
end;


procedure TfrmGenerarePlata.edGrupGridOpPropertiesChange(Sender: TObject);
var I : Integer;
   aCol : TcxGridDBColumn;
begin
  for I := 0 to GridOP.ColumnCount - 1 do
    TcxGridDBColumn(GridOP.Columns[I]).GroupIndex := -1;
  if edGrupGridOp.ItemIndex = 0 then
     aCol := nil
  else if edGrupGridOp.ItemIndex = 1 then
    aCol := GridOP.GetColumnByFieldName('COD_FUNCTIONAL')
  else if edGrupGridOp.ItemIndex = 2 then
    aCol := GridOP.GetColumnByFieldName('COD_ECONOMIC');

  if aCol <> nil then begin
    aCol.GroupIndex := 0;
    //GridOP.ShowGroupPanel := True;
  end;
 end;

procedure TfrmGenerarePlata.cxTreeDebitDblClick(Sender: TObject);
begin
  with TcxDBTreeList(Sender) do
    if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
    (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk;
end;

procedure TfrmGenerarePlata.cxTreeDebitKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ((Key in [VK_UP, VK_DOWN]) and (ssAlt in Shift)) or
      ((Key = VK_F4) and not (ssAlt in Shift)) or (Key = VK_ESCAPE) then
    (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrCancel;
  if (Key = VK_RETURN) and (TcxDBTreeList(Sender).FocusedNode <> nil)
     and (not TcxDBTreeList(Sender).FocusedNode.HasChildren) then
    (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk;
end;

procedure TfrmGenerarePlata.edListaAniPropertiesChange(Sender: TObject);
begin
  edListaLuni.Properties.Items.Clear;
  with edListaLuni.Properties.Items.Add do begin
    Description := 'Toate Lunile';
    Value := '0';
  end;
  with GetTmpADOQuery do
    try
       Sql.Add('exec spNoteGetLuniAn '+edListaAni.Text);
       Open;
       while not Eof do begin
         with edListaLuni.Properties.Items.Add do begin
             Description := LongMonthNames[Fields[0].AsInteger];
             Value := Fields[0].AsInteger;
         end;
         Next;
       end;
    finally
       Free;
    end;
  if edListaLuni.Properties.Items.Count > 0 then edListaLuni.ItemIndex := edListaLuni.Properties.Items.Count-1;
end;

procedure TfrmGenerarePlata.edNrOrdinPropertiesChange(Sender: TObject);
begin
  TestOp;
  UpdateConturi;
end;

procedure TfrmGenerarePlata.edNrUnicPropertiesChange(Sender: TObject);
begin
  RefreshFilter;
end;

procedure TfrmGenerarePlata.GridOPCOD_ECONOMICPropertiesPopup(
  Sender: TObject);
var
   lNode : TcxDBTreeListNode;
   lText : String;
begin
  if GridOP.Controller.FocusedRecord = nil then Exit;
  if not GridOP.Controller.FocusedRecord.IsData then Exit;  
  lText := Trim(VarToStr(GridOP.Controller.FocusedRecord.Values[GridOPCOD_ECONOMIC.Index]));
  if lText <> '' then begin
    lNode := TcxDBTreeListNode(cxTreeEconomic.FindNodeByText(lText, cxTreeEconomicCOD_BUGET));
    if lNode <> nil then begin
      lNode.Focused := True;
      lNode.MakeVisible;
    end;
    end;
end;


procedure TfrmGenerarePlata.GridOPCOD_FUNCTIONALPropertiesPopup(
  Sender: TObject);
var
   lNode : TcxDBTreeListNode;
   lText : String;
begin
  if GridOP.Controller.FocusedRecord = nil then Exit;
  lText := Trim(VarToStr(GridOP.Controller.FocusedRecord.Values[GridOPCOD_FUNCTIONAL.Index]));
  if lText <> '' then begin
    lNode := TcxDBTreeListNode(cxTreeFunctional.FindNodeByText(lText, cxTreeFunctionalCOD_BUGET));
    if lNode <> nil then begin
      lNode.Focused := True;
      lNode.MakeVisible;
    end;
    end;
end;

procedure TfrmGenerarePlata.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  
    Action := caFree;
  GridIstoricNote.StoreToStorage(Self.Name + '.' + GridIstoricNote.Name, TcxDBIniFileWriter);
  GridOP.StoreToStorage(Self.Name + '.' + GridOP.Name, TcxDBIniFileWriter);
end;

procedure TfrmGenerarePlata.edDataOrdinPropertiesChange(Sender: TObject);
begin
  if IsValidDate(edDataOrdin.EditValue) then
    edDataOrdin.ValidateEdit(False);
end;

procedure TfrmGenerarePlata.edDataOrdinPropertiesEditValueChanged(
  Sender: TObject);
begin
  TestOp;
end;

procedure TfrmGenerarePlata.AnulareOp(DataSet: TZReadOnlyQuery);
begin
    with DataSet do
      try
         Sql.Add('exec spNoteOpAnulare :nrOrdin, :dataOrdin, ' + IntToStr(IdUtilizator));
         Params[0].Value := Trim(VarToStr(edNrOrdin.EditValue));
         Params[1].Value := edDataOrdin.Date;
         ExecSql;
         SQL.Clear;
      finally
       end;
end;

procedure TfrmGenerarePlata.btnCancelClick(Sender: TObject);
begin
  Close;
end;

end.
