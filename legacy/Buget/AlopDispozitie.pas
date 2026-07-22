unit AlopDispozitie;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, Buttons, ExtCtrls, Db, ZDataSet, Menus, cxLookAndFeelPainters,
  cxButtons, cxGroupBox,
  cxRepartitorPanel, cxCalendar, cxTextEdit, cxControls, cxContainer,
  cxEdit, cxMaskEdit, cxDropDownEdit, cxStyles, cxGraphics,
  cxDataStorage, cxDBData, cxGridLevel, cxClasses,
  cxGridCustomView, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGrid, cxCurrencyEdit, cxImageComboBox,
  cxGridBandedTableView, cxGridDBBandedTableView, cxButtonEdit, cxTL,
  cxInplaceContainer, cxTLData, cxDBTL, cxProgressBar,
  cxDBEdit, cxMRUEdit, dxmdaset,
  ZAbstractRODataset, ZAbstractDataset,
  cxPC, cxTLdxBarBuiltInMenu, cxLookAndFeels, cxCustomData, cxFilter, cxData,
  cxSplitter, ZSqlUpdate, dxLayoutControl, dxLayoutcxEditAdapters,
  cxCheckBox, cxNavigator, dxBarBuiltInMenu, dxLayoutContainer,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxDateRanges,
  dxScrollbarAnnotations;

const
  WM_REFRESH_DATA = WM_USER+1;
  WM_POSTDATA = WM_USER+2;

type
  TfrmAlopDispozitie = class(TForm)
    pnDocument: TPanel;
    DTDispozitie: TDataSource;
    qryDispozitie: TZQuery;
    qryDispozitieDefalcare: TZQuery;
    pnBugete: TPanel;
    Panel7: TPanel;
    Label11: TLabel;
    pnBottom: TPanel;
    BtnOk: TcxButton;
    btnRapoarte: TcxButton;
    btnCancel: TcxButton;
    BtnModificare: TcxButton;
    btnAnuleazaAng: TcxButton;
    cxStyleRepository: TcxStyleRepository;
    cxStyle1: TcxStyle;
    cxStyle2: TcxStyle;
    cxStyleRepository1: TcxStyleRepository;
    cxStyle3: TcxStyle;
    cxTreeBugete: TcxDBTreeList;
    cxTreeBugeteDESCRIERE: TcxDBTreeListColumn;
    cxTreeBugeteDENUMIRE: TcxDBTreeListColumn;
    cxTreeBugeteCOD_ECRAN: TcxDBTreeListColumn;
    edtFiltruBuget: TcxImageComboBox;
    cxStyle4: TcxStyle;
    cxStyle5: TcxStyle;
    btnNewAng: TcxButton;
    cxTreeBugeteCOD_FUNCTIONAL: TcxDBTreeListColumn;
    cxTreeBugeteID_ANALITIC: TcxDBTreeListColumn;
    Label2: TLabel;
    Label3: TLabel;
    edDataDisp: TcxDBDateEdit;
    Label4: TLabel;
    Label5: TLabel;
    Label6: TLabel;
    edTipDisp: TcxDBImageComboBox;
    edPlatitor: TcxDBPopupEdit;
    Label8: TLabel;
    edPlatitorCont: TcxDBMRUEdit;
    Label9: TLabel;
    edPlatitorBanca: TcxDBTextEdit;
    edBeneficiar: TcxDBPopupEdit;
    Label7: TLabel;
    edBeneficiarCont: TcxDBMRUEdit;
    Label10: TLabel;
    edBeneficiarBanca: TcxDBTextEdit;
    Label12: TLabel;
    edDataUtilizare: TcxDBDateEdit;
    DTDefalcare: TDataSource;
    edNrDisp: TcxDBButtonEdit;
    pnDisp: TPanel;
    pnBeneficiar: TPanel;
    pnPlatitor: TPanel;
    DTRep: TDataSource;
    qryRep: TZQuery;
    Label1: TLabel;
    edCodPlatitor: TcxDBTextEdit;
    Label13: TLabel;
    edCodBeneficiar: TcxDBTextEdit;
    qryTemplate: TZQuery;
    pnClient: TPanel;
    cxTreeRepartitori: TcxDBTreeList;
    cxTreeRepartitoriNUME: TcxDBTreeListColumn;
    cxTreeRepartitoriADRESA: TcxDBTreeListColumn;
    cxTreeRepartitoriCONT: TcxDBTreeListColumn;
    cxTreeRepartitoriCODFISC: TcxDBTreeListColumn;
    cxTreeRepartitoriGESTINT: TcxDBTreeListColumn;
    cxGridDispozitie: TcxGrid;
    GridDispozitie: TcxGridDBTableView;
    GridDispozitieid_bg_plan_economic: TcxGridDBColumn;
    GridDispozitieCodEcran: TcxGridDBColumn;
    GridDispozitieDenEcran: TcxGridDBColumn;
    GridDispozitieCredite_Anual: TcxGridDBColumn;
    GridDispozitieSumaDispozitie: TcxGridDBColumn;
    GridDispozitieDisponibilDupa: TcxGridDBColumn;
    GridDispozitieprocent: TcxGridDBColumn;
    GridDispozitieIdDefalcare: TcxGridDBColumn;
    GridDispozitieL: TcxGridLevel;
    tabFunctional: TcxTabControl;
    cxSplitterNotaJust: TcxSplitter;
    lcNotaJust: TdxLayoutControl;
    lcNotaJustGroup_Root: TdxLayoutGroup;
    updDispozitieDefalcare: TZUpdateSQL;
    Label14: TLabel;
    edNrCerere: TcxDBButtonEdit;
    Label15: TLabel;
    edDataCerere: TcxDBDateEdit;
    GridDispozitieDisponibilTrimInainte: TcxGridDBColumn;
    GridDispozitieDisponibilTrimDupa: TcxGridDBColumn;
    edMotivatie: TcxDBButtonEdit;
    Label16: TLabel;
    GridDispozitieCredite_trim: TcxGridDBColumn;
    GridDispozitieCredite_Deschise: TcxGridDBColumn;
    pnDispozitieTop: TPanel;
    pnDispozitieTopBottom: TPanel;
    pnDispHeadCerere: TPanel;
    stilIntroducere: TcxStyle;
    stilReadOnly: TcxStyle;
    cmbListaLuni: TcxComboBox;
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure BtnOkClick(Sender: TObject);
    procedure btnAnexa2Click(Sender: TObject);
    procedure edPredatorEnter(Sender: TObject);
    procedure edPredatorKeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
    procedure FormCreate(Sender: TObject);
    procedure FunctionalBarPopupInitPopup(Sender: TObject);
    procedure qryDispozitieNewRecord(DataSet: TDataSet);
    procedure BtnModificareClick(Sender: TObject);
    procedure btnAnuleazaAngClick(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure btnCancelClick(Sender: TObject);
    procedure ieFiltruPropertiesChange(Sender: TObject);
    procedure cxTreeRepartitoriKeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
    procedure cxTreeRepartitoriDblClick(Sender: TObject);
    procedure qryDispozitieAfterOpen(DataSet: TDataSet);
    procedure btnNewAngClick(Sender: TObject);
    procedure edPredatorPropertiesPopup(Sender: TObject);
    procedure pnBottomResize(Sender: TObject);
    procedure edNrDispPropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
    procedure edDataDispPropertiesValidate(Sender: TObject;
      var DisplayValue: Variant; var ErrorText: TCaption;
      var Error: Boolean);
    procedure edDataUtilizarePropertiesValidate(Sender: TObject;
      var DisplayValue: Variant; var ErrorText: TCaption;
      var Error: Boolean);
    procedure edPlatitorPropertiesCloseQuery(Sender: TObject;
      var CanClose: Boolean);
    procedure edPlatitorPropertiesPopup(Sender: TObject);
    procedure edPlatitorPropertiesInitPopup(Sender: TObject);
    procedure edPlatitorContPropertiesButtonClick(Sender: TObject);
    procedure edPlatitorContPropertiesCloseUp(Sender: TObject);
    procedure cxTreeBugeteDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure DTDispozitieDataChange(Sender: TObject; Field: TField);
    procedure pnDocumentResize(Sender: TObject);
    procedure pnDispResize(Sender: TObject);
    procedure tabFunctionalChange(Sender: TObject);
    procedure GridDispozitieFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure qryTemplateAfterOpen(DataSet: TDataSet);
    procedure qryDispozitieDefalcareAfterOpen(DataSet: TDataSet);
    procedure GridDispozitieStylesGetContentStyle(
      Sender: TcxCustomGridTableView; ARecord: TcxCustomGridRecord;
      AItem: TcxCustomGridTableItem; var AStyle: TcxStyle);
    procedure cmbListaLuniPropertiesChange(Sender: TObject);
    procedure edTipDispPropertiesChange(Sender: TObject);
  private
    FCurentDispozitie: Integer;
    IsInLoading      : Boolean;
    FErrRecord : String;
    FExecOnValidation: TNotifyEvent;
    FIdDefalcare: Variant;
    procedure RecalcItemsi;
    procedure SetCurentDispozitie(const Value: Integer);
    { Private declarations }
    procedure DeschideDataSet;
    procedure ActivateGrid;
     procedure ForcePostAll;
    procedure ClearDispozitii;
    function  ValidareDispozitieEcran(const NeedFilled : Boolean = True) : Boolean;
    procedure LocalModificValidation(Sender: TObject);
    function  NewDispozitie: Integer;
    procedure DataDispozitieChange(Sender:TField);
    procedure SetIdDefalcare(const Value: Variant);
        function AreSumeDefalcate(IdDispozitie: Integer): Boolean;

    procedure IdRepartitoriChange(Sender: TField);
  protected
    FCopyList : TStringList;
    FCopyFields : String;
    FCopyValues : String;
    procedure SetDetaliiValidare(IsValidat : Boolean);
    function IsDispValidat : Boolean;
    procedure ValidateSuma(Sender : TField);
    procedure WMRefreshField(var Message: TMessage); message WM_REFRESH_DATA;
    procedure WMPostData(var Message: TMessage); message WM_PostData;
    procedure ReportClick(Sender: TObject);
  public
    FModificare : Boolean;
    FVerificareSalvare: Boolean;
    FEditModified : Boolean;
    FCFArray : Variant;
    procedure SetTabsOnId(IdRep : Integer);
    procedure SetCodFunctional(CodF : String; Analitic : Integer);
    procedure TestGolireEcran;
    procedure ReadDispozitie(const IdDispozitie : Integer = 0);
    procedure LoadDispozitie(IdDispozitie : Integer);
    property  CurentDispozitie: Integer read FCurentDispozitie write SetCurentDispozitie;
    property  IdDefalcare : Variant read FIdDefalcare write SetIdDefalcare;
    property  ExecOnValidation : TNotifyEvent read FExecOnValidation write FExecOnValidation;
    { Public declarations }
  end;

function  ModificareDispozitie(IdDispozitie : Integer) : TForm;
procedure PrintDispozitie(lIdDispozitie : Integer; ang : Boolean);


implementation

{$R *.DFM}

uses
  dateUtils, dxCompsUtile, ZeosDBUtile, CommonDBVar, ConcurentUsersUnit, Variants, DateUnit,
  rapInclude,
  AlopDispVizualizare, ATSZDBUtils, FormulareUnit;


procedure TfrmAlopDispozitie.SetCurentDispozitie(const Value: Integer);
var
  I : Integer;
begin
  FCFArray := Null;
  tabFunctional.Tabs.Clear;
  FCurentDispozitie := Value;
  qryDispozitieDefalcare.Close;
  for I := 0 to qryDispozitieDefalcare.Params.Count -1 do
    qryDispozitieDefalcare.Params[I].Clear;
  with qryDispozitie do begin
    if FCurentDispozitie = -1 then
       FCurentDispozitie := NewDispozitie
    else begin
      Close;
      Params[0].Value := Value;
      Open;
    end;
    if FieldByName('DataDispozitie').Value = Null then
      edDataDisp.Clear;
    SetDetaliiValidare(FieldByName('VALIDAT').AsInteger=1);
  end;
  ActivateGrid;
end;

procedure TfrmAlopDispozitie.ReadDispozitie(const IdDispozitie : Integer = 0);
begin
  if IdDispozitie = 0 then
    CurentDispozitie := ValueSafeToInt(
      DBGetScallarFmt('exec [spAlopDispozitieNevalidate] %d, null', [idUtilizator]),
      -1)
  else
    CurentDispozitie := IdDispozitie;
end;



procedure TfrmAlopDispozitie.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  DBPost(qryDispozitie);
  SetFilterOnDataSet(frmData.qryBGPlanFunctional, '');
  Action := caFree;
end;

procedure TfrmAlopDispozitie.BtnOkClick(Sender: TObject);
begin
  DBPost(qryDispozitie);
  DBPost(qryDispozitieDefalcare);

  if BtnOk.Tag = -1 then begin
    DBSetFieldValue(qryDispozitie, 'VALIDAT', 0)
  end
  else begin
    if not ValidareDispozitieEcran(False) then begin
      MessageDlg(FErrRecord, mtError, [mbOk],0);
      Abort;
    end
    else begin
      DBExecSqlFmt('UPDATE ALOP_DISPOZITIE SET VALIDAT = 1 WHERE ID_ALOP_DISPOZITIE = %d', [FCurentDispozitie]);

      DBRefresh(qryDispozitie);
      if Assigned(FExecOnValidation) then FExecOnValidation(Self)
      else LocalModificValidation(Self);
    end;
  end;

  SetDetaliiValidare(qryDispozitie.FieldByName('VALIDAT').AsInteger = 1);
end;


procedure TfrmAlopDispozitie.btnAnexa2Click(Sender: TObject);
var
  ang : Boolean;
begin
  ang := True;
  if Sender is TcxButton then
    if TcxButton(Sender).Tag = 1 then ang := False;

   PrintDispozitie(FCurentDispozitie, ang);
   FModificare := False;
   //if ang then ClearDispozitii; //Close;
end;

procedure TfrmAlopDispozitie.edPredatorEnter(Sender: TObject);
begin
  if csDestroying in ComponentState then Exit;
//  with TcxPopupEdit(Sender) do DroppedDown := True;
end;

procedure TfrmAlopDispozitie.edPredatorKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if (Shift = []) and ((Key > 32) or (Key in [8,27, 13])) then begin
     with TcxPopupEdit(Sender) do DroppedDown := True;
     Key := 0;
  end;
end;

function TfrmAlopDispozitie.NewDispozitie: Integer;
begin
  DBStartTransaction;
  try
    with GetTmpADOQuery do
      try
        SQL.Add('exec spAlopNewDispozitie :id_utilizatori, :data_emitere');
        Params.ParamByName('id_utilizatori').Value := IdUtilizator;
        Params.ParamByName('data_emitere').Value   := Null;
        Open;
        Result := Fields[0].AsInteger;
      finally
        Free;
      end;
    DBCommit;
    qryDispozitie.Close;
    qryDispozitie.Params[0].Value := Result;
    qryDispozitie.Open;
{
    FunctionalBar.KeyValue := Null;
    FunctionalBar.ValidateWithPopup := False;
    FunctionalBar.EditInput.Text := '';
    FunctionalBar.ListaInput.Text := '';
    FunctionalBar.ValidateWithPopup := True;
}
    FEditModified := False;
  except
    on E: Exception do begin
       DBRollBack;
       raise EContaHandledError.Create('Nu se poate adauga dispozitia !'#13#10'EROARE : '+E.Message);
    end;
  end;
end;

procedure TfrmAlopDispozitie.FormCreate(Sender: TObject);
var
  I: Integer;
begin

  if DBGetSetare('dispozitiiPeLuni') = '1' then begin
    cmbListaLuni.Visible := True;
    for I := Low(FormatSettings.LongMonthNames) to High(FormatSettings.LongMonthNames) do
      cmbListaLuni.Properties.Items.Add(FormatSettings.LongMonthNames[I]);
  end
  else cmbListaLuni.Visible := False;

  FCopyList := TStringList.Create;
  tabFunctional.Tabs.Clear;
  FCFArray := null;
  DBRefresh(qryRep);
{
  FunctionalBar.ValidateEditText := True;
  FunctionalBar.OnlySelectChild := True;
}
  FEditModified := False;
  FModificare := False;
  //FExecOnValidation := LocalModificValidation;
  FExecOnValidation := nil;
  FVerificareSalvare := DBProcExists('spAlopBugetDisponibilVerificareCamp');

  FillImageCombo(edtFiltruBuget.Properties, 'SELECT * FROM BG_TIPURI_BUGET', 'ID_BG_TIPURI_BUGET', 'DENUMIRE', Null, '<Toate tipurile de bugete>');

  cxTreeBugete.FullExpand;
  qryTemplate.Open;

  PopulateReportContext('Rapoarte Dispozitie Bugetara', btnRapoarte, ReportClick);
end;


procedure TfrmAlopDispozitie.FunctionalBarPopupInitPopup(Sender: TObject);
begin
  with TcxPopupEdit(Sender).Properties do begin
    if PopupWidth < TcxPopupEdit(Sender).Width then PopupWidth := TcxPopupEdit(Sender).Width;
  end;
end;

procedure TfrmAlopDispozitie.qryDispozitieNewRecord(DataSet: TDataSet);
begin
  //Dataset.FieldByName('TipDispozitie').AsInteger := 1;
  if CurentDispozitie <= 0 then Exit;
  raise EContaHandledError.Create('Eroare interna -> inchideti ecranul si mai accesati odata meniul !');
end;


procedure TfrmAlopDispozitie.BtnModificareClick(Sender: TObject);
var
  lIdDisp: Integer;
begin
  TestGolireEcran;

  if not IsDispValidat then
  begin
    if not AreSumeDefalcate(CurentDispozitie) then
    begin
      DBExecSQLFmt('exec [spAlopAnuleazaDispozitie] %d', [CurentDispozitie]);
      ShowMessage('Dispozitia a fost anulată deoarece nu avea sume.');
    end
    else
    begin
      ShowMessage('Dispozitia NU a fost anulată deoarece conține sume.');
    end;
  end;

  lIdDisp := SelectieDispozitie;

  if lIdDisp <> -1 then
  begin
   // ShowMessage('Se va face refresh pentru IdDispozitie = ' + IntToStr(lIdDisp));
    LoadDispozitie(lIdDisp);
  end;
end;





procedure TfrmAlopDispozitie.LoadDispozitie(IdDispozitie: Integer);
var
  lDataSet: TDataSet;
begin
  lDataSet := DBNewQueryFmt('exec [spAlopLoadDispozitie] %d, %d', [IdUtilizator, IdDispozitie]);
  try
    lDataSet.Open;
   ReadDispozitie(IdDispozitie);
  // ShowMessage('Dispozitia incarcata = ' + IntToStr(CurentDispozitie));
  // ShowMessage('Dispozitia incarcata VALIDAT = ' + qryDispozitie.FieldByName('VALIDAT').AsString);

  finally
    lDataSet.Free;

  end;
   if not IsInLoading then begin
    DBRefresh(qryDispozitieDefalcare);
  end;
end;

procedure TfrmAlopDispozitie.btnAnuleazaAngClick(Sender: TObject);
var
  lNr : String;
  lData : String;
begin
  if (CurentDispozitie > 0) and (FEditModified) then begin
    //TestGolireEcran;
    lNr := qryDispozitie.FieldByName('NrDispozitie').AsString;
    lData := qryDispozitie.FieldByName('DataDispozitie').AsString;
    if (MessageDlg(Format('Doriti stergerea dispozitiei nr. : %s din data  %s ?', [lNr, lData]),
         mtConfirmation, [mbYes, mbNo], 0) in [mrNo, mrNone]) then
       Abort;
    DBExecSQLFmt('exec [spAlopAnuleazaDispozitie] %d', [CurentDispozitie]);

    qryDispozitieDefalcare.Params.ParamByName('COD_BUGET').Value := Null;
    if Assigned(FExecOnValidation) then FExecOnValidation(Self)
    else LocalModificValidation(Self);
    ClearDispozitii;
  end;
end;

procedure TfrmAlopDispozitie.ActivateGrid;
begin
  cxGridDispozitie.Enabled := not IsDispValidat  and (CurentDispozitie > 0) ;//and (FunctionalBar.KeyValue <> null);
  if cxGridDispozitie.Enabled then
    GridDispozitie.Styles.Background.Color := clWindow
  else
    GridDispozitie.Styles.Background.Color := clBtnFace;
end;



procedure TfrmAlopDispozitie.FormDestroy(Sender: TObject);
begin
  FCopyList.Free;
  ExitSingleUser;
end;

procedure TfrmAlopDispozitie.ClearDispozitii;
begin
  ReadDispozitie;
end;

function TfrmAlopDispozitie.ValidareDispozitieEcran (const NeedFilled : Boolean = True): Boolean;
//var lNode : TcxTreeListNode;
begin
  FErrRecord := '';
  if qryDispozitie.State in [dsEdit, dsInsert] then qryDispozitie.Post;

  with qryDispozitie do begin
    if FieldByName('NrDispozitie').AsString = '' then
      FErrRecord := FErrRecord + 'Trebuie completat [Nr Dispozitie] !' + #13#10;
    if FieldByName('TipDispozitie').AsInteger = 0 then
      FErrRecord := FErrRecord + 'Trebuie completat [pentru] !' + #13#10;
    if FieldByName('IdPlatitor').AsInteger = 0 then
      FErrRecord := FErrRecord + 'Trebuie completat [Platitor] !' + #13#10;
    if FieldByName('IdBeneficiar').AsInteger = 0 then
      FErrRecord := FErrRecord + 'Trebuie completat [Beneficiar] !' + #13#10;
    if not IsValidDate(FieldByName('DataDispozitie').AsDateTime) then
      FErrRecord := FErrRecord + 'Trebuie completata data dispozitiei [Data Dispozitie] !' + #13#10;
    if not IsValidDate(FieldByName('DataUtilizare').AsDateTime) then
      FErrRecord := FErrRecord + 'Trebuie completata data utilizare [Data utilizare] !' + #13#10;
  end;


  Result := (FErrRecord = '');
end;

procedure TfrmAlopDispozitie.btnCancelClick(Sender: TObject);
begin
  Close;
end;


procedure TfrmAlopDispozitie.ieFiltruPropertiesChange(Sender: TObject);
begin
  if Trim(edtFiltruBuget.EditValue) = '-1' then
      SetFilterOnDataSet(frmData.qryBGPlanFunctional, '')
  else
      SetFilterOnDataSet(frmData.qryBGPlanFunctional,
        'ID_BG_TIPURI_BUGET= ' +   IntToStr(edtFiltruBuget.EditValue));
end;

procedure TfrmAlopDispozitie.cxTreeRepartitoriKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
    with TcxDBTreeList(Sender) do
      if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
        (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk
  else if Key = VK_ESCAPE then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrCancel;
end;

procedure TfrmAlopDispozitie.cxTreeRepartitoriDblClick(Sender: TObject);
begin
    with TcxDBTreeList(Sender) do
      if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
        (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk
end;

procedure TfrmAlopDispozitie.qryDispozitieAfterOpen(DataSet: TDataSet);
begin
  DataSet.FieldByName('IdPlatitor').OnChange := IdRepartitoriChange;
  DataSet.FieldByName('IdBeneficiar').OnChange := IdRepartitoriChange;
  DataSet.FieldByName('DataDispozitie').OnChange := DataDispozitieChange;
  IdRepartitoriChange(DataSet.FieldByName('IdPlatitor'));
  IdRepartitoriChange(DataSet.FieldByName('IdBeneficiar'));
  qryDispozitieDefalcare.ParamByName('IdDisp').AsInteger := DataSet.FieldByName('id_Alop_Dispozitie').AsInteger;
  qryDispozitieDefalcare.ParamByName('Data_Dispozitie').AsDateTime := DataSet.FieldByName('DataDispozitie').AsDateTime;
 if not IsInLoading then begin
    DBRefresh(qryDispozitieDefalcare);
  end;

  if (cmbListaLuni.Visible) and not edDataUtilizare.DataBinding.Field.IsNull then
    cmbListaLuni.ItemIndex := MonthOf(edDataUtilizare.DataBinding.Field.AsDateTime) - 1;

end;

function ModificareDispozitie(IdDispozitie : Integer) : TForm;
begin
  if EnterSingleUser(TfrmAlopDispozitie) then begin
    Result := TForm(GetNewForm(TfrmAlopDispozitie));
    with TfrmAlopDispozitie(Result) do begin
      FModificare := True;

      WindowState := wsMaximized;
      DBExecSQLFmt('exec [spAlopDispozitieInvalideazaCulegere] %d', [IdUtilizator]);
      LoadDispozitie(IdDispozitie);
      FExecOnValidation := LocalModificValidation;
    end;
  end;
end;

procedure TfrmAlopDispozitie.LocalModificValidation(Sender: TObject);
begin
  DBExecSQLFmt('exec [spAlopDispozitieRevalideazaCulegere] %d', [IdUtilizator]);
end;


procedure PrintDispozitie(lIdDispozitie : Integer; ang : Boolean);
var
  aIdReport : Integer;
  aStr : String;
begin
   DateUnit.IdDispozitie := lIdDispozitie;
   aStr := '';

   if ang then
      aStr :=  'DispozitieRap'
   else
      aStr := 'DispozitieCentr';

   aIdReport := -1;
   if aStr <> '' then
     aIdReport :=  DateUnit.GetItemId(aStr);

   if aIdReport <> -1 then begin
     LoadReport(aIdReport);
     //WriteReportToRepository(aIdReport, 'Angajament', IdAngajament);
   end;
end;

procedure TfrmAlopDispozitie.tabFunctionalChange(Sender: TObject);
var
   I : Integer;
   CodF : String;
   Analitic : Integer;
begin
  I := tabFunctional.TabIndex;
  if VarIsArray(FCFArray) then begin
    CodF := FCFArray[I][0];
    Analitic := FCFArray[I][2];
    SetCodFunctional(CodF, Analitic);
  end
  else
    SetCodFunctional('', 0);
end;

procedure TfrmAlopDispozitie.SetCodFunctional(CodF: String;
  Analitic: Integer);
begin
  if (CodF <> '')  then begin
    qryDispozitieDefalcare.ParamByName('COD_BUGET').AsString := CodF;
    if Analitic = 0 then
      qryDispozitieDefalcare.ParamByName('ID_ANALITIC').Clear
    else
      qryDispozitieDefalcare.ParamByName('ID_ANALITIC').AsInteger := Analitic;
  end
  else begin
    qryDispozitieDefalcare.ParamByName('COD_BUGET').Clear;
    qryDispozitieDefalcare.ParamByName('ID_ANALITIC').Clear;
  end;
  if not IsInLoading then begin
    DBRefresh(qryDispozitieDefalcare);
  end;
  ActivateGrid;
end;


procedure TfrmAlopDispozitie.SetDetaliiValidare(IsValidat: Boolean);
begin
     //    ShowMessage('VALIDAT acum = ' + BoolToStr(IsValidat, True));
  if IsValidat then begin
    btnOk.Caption := 'Editare';
    btnOk.Tag := -1;
  end
  else begin
    btnOk.Caption := 'Salvare';
    btnOk.Tag := 0;
  end;
  btnRapoarte.Enabled := IsValidat;


  edNrDisp.Enabled := not IsValidat;
  edDataDisp.Enabled := not IsValidat;
  edTipDisp.Enabled := not IsValidat;
  edPlatitor.Enabled := not IsValidat;
  edPlatitorCont.Enabled := not IsValidat;
  edPlatitorBanca.Enabled := not IsValidat;
  edBeneficiar.Enabled := not IsValidat;
  edBeneficiarCont.Enabled := not IsValidat;
  edBeneficiarBanca.Enabled := not IsValidat;
  edDataUtilizare.Enabled := not IsValidat;
//  FunctionalBar.EditsEnabled := not IsValidat;
  ActivateGrid;
end;

function TfrmAlopDispozitie.IsDispValidat: Boolean;
begin
  Result := False;
  if not qryDispozitie.IsEmpty then
    Result := (qryDispozitie.FieldByName('VALIDAT').AsInteger = 1);
end;

procedure TfrmAlopDispozitie.btnNewAngClick(Sender: TObject);
begin
  TestGolireEcran;
  if not IsDispValidat then DBExecSQLFmt('exec [spAlopGolesteDispozitie] %d', [CurentDispozitie]);
  ClearDispozitii;
end;
  function TfrmAlopDispozitie.AreSumeDefalcate(IdDispozitie: Integer): Boolean;
begin
  Result := DBGetScallarFmt(
    'SELECT COUNT(*) FROM ALOP_DISPOZITIE_DEFALCARE WHERE ID_ALOP_DISPOZITIE = %d AND ISNULL(SumaDispozitie, 0) > 0',
    [IdDispozitie]
  ) > 0;
end;
procedure TfrmAlopDispozitie.TestGolireEcran;
  function HasSume : Boolean;
  begin
    Result := (qryDispozitieDefalcare.Active);
    if Result then begin
      qryDispozitieDefalcare.Filter := 'SumeDispozitie > 0';
      qryDispozitieDefalcare.Filtered := True;
      Result :=  (qryDispozitieDefalcare.RecordCount > 0);
      qryDispozitieDefalcare.Filtered := False;
    end;
  end;
begin
 if (not qryDispozitie.Active) or qryDispozitie.IsEmpty then Exit;
  if (not qryDispozitieDefalcare.Active) or qryDispozitieDefalcare.IsEmpty then Exit;
  if not IsDispValidat and not HasSume then
    if (MessageDlg('Modificarea unui document din arhiva duce la pierderea dispozitiei din ecran ! '+#13+#10+'Doriti continuarea ?', mtConfirmation, [mbYes, mbNo], 0) = mrNo) then
      Abort;
end;

 procedure TfrmAlopDispozitie.ForcePostAll;
begin


  if qryDispozitieDefalcare.State in [dsEdit, dsInsert] then
    qryDispozitieDefalcare.Post;

  if qryDispozitie.State in [dsEdit, dsInsert] then
    qryDispozitie.Post;
end;


procedure TfrmAlopDispozitie.edPredatorPropertiesPopup(Sender: TObject);
var
  lNode : TcxTreeListNode;
  lIdRep : Integer;
begin
  if not (Sender is TcxPopupEdit) then Exit;
  lIdRep := TcxPopupEdit(Sender).Tag;
  lNode := cxTreeRepartitori.FindNodeByKeyValue(lIdRep, nil);
  if lNode <> nil then begin
    lNode.Focused := True;
    lNode.MakeVisible;
  end;
end;

procedure TfrmAlopDispozitie.edTipDispPropertiesChange(Sender: TObject);
var
  lAnalitic: Variant;
begin
//  DBPost(qryDispozitie);
  if ValueHasValue(edTipDisp.EditingValue) then
    if ValueSafeToInt(edTipDisp.EditingValue, 1) = 1 then
      lAnalitic := qryDispozitie['idBeneficiar']
    else
      lAnalitic := qryDispozitie['idPlatitor']
  else
    lAnalitic := Null;
  if ValueHasValue(lAnalitic) then begin
    SetTabsOnId(lAnalitic);
    qryDispozitieDefalcare.ParamByName('ID_ANALITIC').Value := lAnalitic;
    tabFunctionalChange(nil);
   if not IsInLoading then begin
    DBRefresh(qryDispozitieDefalcare);
  end;
  end;
end;

procedure TfrmAlopDispozitie.RecalcItemsi;
var
  I : Integer;
  lLastPoz: TBookmark;
begin
  if qryDispozitieDefalcare.Active and not qryDispozitieDefalcare.IsEmpty then begin
    qryDispozitieDefalcare.DisableControls;
    lLastPoz := qryDispozitieDefalcare.GetBookmark;
    try
      IsInLoading := True;
      try
        qryDispozitieDefalcare.First;
        while not qryDispozitieDefalcare.Eof do begin
          if qryDispozitieDefalcare.FieldByName('utilizat').AsBoolean then begin
            DBGoEdit(qryDispozitieDefalcare);
            for I := 0 to FCopyList.Count-1 do
              ValidateSuma(qryDispozitieDefalcare.FieldByName(FCopyList[I]));
            DBPost(qryDispozitieDefalcare);
          end;
          qryDispozitieDefalcare.Next;
        end;
      finally
        IsInLoading := False;
      end;
    finally
      qryDispozitieDefalcare.GotoBookmark(lLastPoz);
      qryDispozitieDefalcare.FreeBookmark(lLastPoz);
      qryDispozitieDefalcare.EnableControls;
    end;
  end;
end;

procedure TfrmAlopDispozitie.pnBottomResize(Sender: TObject);
begin
  BtnCancel.Left := pnBottom.Width - BtnCancel.Width - 5;
  btnRapoarte.Left := BtnCancel.Left - btnRapoarte.Width - 2;
  BtnOk.Left := btnRapoarte.Left - BtnOk.Width - 6;
end;

procedure TfrmAlopDispozitie.edNrDispPropertiesButtonClick(Sender: TObject;
  AButtonIndex: Integer);
begin
  edNrDisp.EditValue := DBGetScallarFmt('exec [spAlopNumarDispozitie] %d', [CurentDispozitie]);
end;

procedure TfrmAlopDispozitie.edDataDispPropertiesValidate(Sender: TObject;
  var DisplayValue: Variant; var ErrorText: TCaption; var Error: Boolean);
begin
  if Error then begin
     ErrorText := '';
     raise EContaHandledError.Create('Data introdusa este invalida ! ');
  end;
  PostMessage(Handle, WM_POSTDATA, 0, 0);
end;

procedure TfrmAlopDispozitie.edDataUtilizarePropertiesValidate(
  Sender: TObject; var DisplayValue: Variant; var ErrorText: TCaption;
  var Error: Boolean);
begin
  if Error then begin
     ErrorText := '';
     raise EContaHandledError.Create('Data introdusa este invalida ! ');
  end;
end;

type
  TAccesscxPopupEdit = class(TcxPopupEdit);

procedure TfrmAlopDispozitie.edPlatitorPropertiesCloseQuery(
  Sender: TObject; var CanClose: Boolean);
var
  lNode : TcxDBTreeListNode;
  lIdRep : Integer;
  lIdField, lNumeField, lCodField : string;
begin
  if DBRecordExists('ALOP_DISPOZITIE_DEFALCARE', 'id_alop_dispozitie = ' + ValueToStr(CurentDispozitie) + ' and suma <> 0') then begin
    if (MessageDlg('Exista inregistrari pentru beneficiarul curent ! Doriti stergerea acestora ?', mtConfirmation, [mbYes, mbNo], 0)<> mrYes) then begin
      TAccesscxPopupEdit(Sender).PopupWindow.ModalResult := mrCancel;
    end
    else
      DBExecSQLFmt('exec [spAlopDispStergeDefalcare] %d', [CurentDispozitie]);
  end;
  if TcxDBPopupEdit(Sender).Tag = 1 then begin
                                      lIdField := 'IdPlatitor';
                                      lNumeField := 'NumePlatitor';
                                      lCodField := 'CodPlatitor';
                                    end
                                    else begin
                                      lIdField := 'IdBeneficiar';
                                      lNumeField := 'NumeBeneficiar';
                                      lCodField := 'CodBeneficiar';
                                    end;
  with TAccesscxPopupEdit(Sender) do
    if PopupWindow.ModalResult = mrOk then begin
       lNode := TcxDBTreeListNode(cxTreeRepartitori.FocusedNode);
       if Assigned(lNode) then begin
          lIdRep := lNode.KeyValue;
          DBGoEdit(qryDispozitie);
          qryDispozitie.FieldByName(lIdField).AsInteger := lIdRep;
          qryDispozitie.FieldByName(lNumeField).AsString := VarToStr(lNode.Values[cxTreeRepartitoriNUME.ItemIndex]);
          qryDispozitie.FieldByName(lCodField).AsString := VarToStr(lNode.Values[cxTreeRepartitoriCODFISC.ItemIndex]);
          DBPost(qryDispozitie);
       end;
    end;
end;

procedure TfrmAlopDispozitie.edPlatitorPropertiesPopup(Sender: TObject);
var
 lNode : TcxTreeListNode;
 lFieldName : string;
begin
  if TcxDBPopupEdit(Sender).Tag = 1 then lFieldName := 'IdPlatitor'
                                    else lFieldName := 'IdBeneficiar';
  lNode := cxTreeRepartitori.FindNodeByKeyValue(qryDispozitie.FieldbyName(lFieldName).AsInteger, nil);
  if lNode <> nil then begin
    lNode.Focused := True;
    lNode.MakeVisible;
  end;
end;

procedure TfrmAlopDispozitie.edPlatitorPropertiesInitPopup(
  Sender: TObject);
var
  lEdit: TcxDBPopupEdit;
begin
  lEdit := TcxDBPopupEdit(Sender);
  if lEdit.Properties.PopupWidth < lEdit.Width then lEdit.Properties.PopupWidth := lEdit.Width;
end;

procedure TfrmAlopDispozitie.IdRepartitoriChange(Sender: TField);
var
  lPlatitor   : Boolean;
  lIsScadere  : Boolean;
  lIdRep      : Integer;
  lCustomEdit : TcxDBMRUEdit;
begin
  lPlatitor   := SameText(Sender.FieldName, 'IdPlatitor');
  lIsScadere  := ValueSafeToInt(qryDispozitie['TipDispozitie'], 1) = -1;
  if lPlatitor then lCustomEdit := edPlatitorCont
  else lCustomEdit := edBeneficiarCont;
  lCustomEdit.Properties.Items.Clear;
  if TryStrToInt(Sender.AsString, lIdRep) then begin
    FillMRUComboFmt(lCustomEdit.Properties, 'SELECT DISTINCT CONT FROM .dbo.fnDefaultRepartitorConturi(%d)', [lIdRep], 'CONT');
    if not (lPlatitor xor lIsScadere) then begin
      SetTabsOnId(lIdRep);
      tabFunctionalChange(nil);
    end;
  end;
end;

procedure TfrmAlopDispozitie.edPlatitorContPropertiesButtonClick(
  Sender: TObject);
var
  IdRep : Integer;
  lDataSet: TDataSet;
  lIdField, lContField, lBancaField : String;
begin
  if TcxDBMRUEdit(Sender).Tag = 1 then begin
    lContField := 'ContPlatitor';
    lBancaField := 'BancaPlatitor';
    lIdField := 'IdPlatitor';
  end else begin
    lContField := 'ContBeneficiar';
    lBancaField := 'BancaBeneficiar';
    lIdField := 'IdBeneficiar';
  end;
  IdRep := qryDispozitie.FieldByName(lIdField).AsInteger;
  if IdRep > 0 then begin
    lDataSet := DBNewQueryFmt('SELECT TOP 1 * FROM .dbo.fnDefaultRepartitorConturi(%d)', [IdRep]);
    try
      lDataSet.Open;
      if not lDataSet.IsEmpty then begin
        if not (qryDispozitie.State in [dsEdit, dsInsert]) then qryDispozitie.Edit;
        qryDispozitie.FieldByName(lContField).AsString  := lDataSet.FieldByName('CONT').AsString;
        qryDispozitie.FieldByName(lBancaField).AsString := lDataSet.FieldByName('BANCA_DENUMIRE').AsString;
        qryDispozitie.Post;
      end;
    finally
      lDataSet.Free;
    end;
  end;
end;

procedure TfrmAlopDispozitie.edPlatitorContPropertiesCloseUp(
  Sender: TObject);
var
  IdRep : Integer;
  lDataSet: TDataSet;
  lIdField, lBancaField, lContField, lContValue : String;
begin
  if TcxDBMRUEdit(Sender).Tag = 1 then begin
    lBancaField := 'BancaPlatitor';
    lIdField := 'IdPlatitor';
    lContField := 'ContPlatitor';
  end else begin
    lBancaField := 'BancaBeneficiar';
    lIdField := 'IdBeneficiar';
    lContField := 'ContBeneficiar';
  end;
  IdRep := qryDispozitie.FieldByName(lIdField).AsInteger;
  lContValue := qryDispozitie.FieldByName(lContField).AsString;
  if IdRep > 0 then begin
    lDataSet := DBNewQueryFmt('SELECT TOP 1 * FROM .dbo.fnDefaultRepartitorConturi(%d) where CONT LIKE ''%%%s%%''', [IdRep, lContValue]);

    try
      lDataSet.Open;
      if lDataSet.IsEmpty then begin
        if not (qryDispozitie.State in [dsEdit, dsInsert]) then qryDispozitie.Edit;
        qryDispozitie.FieldByName(lBancaField).AsString := lDataSet.FieldByName('BANCA_DENUMIRE').AsString;
        qryDispozitie.Post;
      end;
    finally
      lDataSet.Free;
    end;
  end;
end;

procedure TfrmAlopDispozitie.cmbListaLuniPropertiesChange(Sender: TObject);
begin
  DBSetFieldValue(edDataUtilizare.DataBinding.Field, Trunc(EndOfTheMonth( EncodeDate(YearOf(Date), cmbListaLuni.ItemIndex + 1, 1)) ));
end;

procedure TfrmAlopDispozitie.cxTreeBugeteDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
 Value := ANode.Values[cxTreeBugeteCOD_ECRAN.ItemIndex] + ': '+ANode.Values[cxTreeBugeteDENUMIRE.ItemIndex];
end;

procedure TfrmAlopDispozitie.DTDispozitieDataChange(Sender: TObject;
  Field: TField);
begin
  FEditModified := True;
end;

procedure TfrmAlopDispozitie.pnDocumentResize(Sender: TObject);
begin
  //
end;

procedure TfrmAlopDispozitie.pnDispResize(Sender: TObject);
begin
  //
  pnPlatitor.Width := pnDisp.Width div 2;
end;

procedure TfrmAlopDispozitie.ValidateSuma(Sender: TField);
var
  lIdDisp  : Integer;
  lCopyFields : String;
  lCopyValues : string;
begin
ShowMessage(Sender.ClassName + ': ' + Sender.FieldName + ' = ' + Sender.AsString);


  if FVerificareSalvare then
    DBExecSQLFmt('exec [spAlopBugetDisponibilVerificareCamp] %s, %s, %s',
      [ValueToStr('<row'+DBRowToXML(Sender.DataSet)+'/>'), ValueToStr(Sender.FieldName), ValueToStr(Sender.Value)]);
  if FVerificareSalvare then
    DBExecSQLFmt('exec [spAlopBugetDisponibilVerificareCamp] %s, %s, %s',
      [ValueToStr('<row'+DBRowToXML(Sender.DataSet)+'/>'), ValueToStr(Sender.FieldName), ValueToStr(Sender.Value)]);

  with DBNewQuery('DECLARE @VALOARE MONEY') do
    try
      if (Sender.IsNull) or (Sender.AsCurrency = 0) then Sql.Add('SET @VALOARE = NULL')
      else Sql.Add('SET @VALOARE = :'+Sender.FieldName);
      if Sender.DataSet.FieldByName('INTRODUS').AsInteger = 1 then begin
        Sql.Add('UPDATE ALOP_DISPOZITIE_DEFALCARE SET '+Sender.FieldName+' = @VALOARE, MOMENT = getdate(), IDUTILIZATOR = '+ IntToStr(IdUtilizator) + ' WHERE ID_ALOP_DISPOZITIE_DEFALCARE = :ID_ALOP_DISPOZITIE_DEFALCARE ' );
        SQL.Add('select :ID_ALOP_DISPOZITIE_DEFALCARE');
      end
      else begin
        lCopyFields := StringReplace(FCopyFields, Sender.FieldName+',', '', [rfIgnoreCase, rfReplaceAll] );
        lCopyValues := StringReplace(FCopyValues, ':'+Sender.FieldName+',', '', [rfIgnoreCase, rfReplaceAll] );
        Sql.Add('INSERT INTO ALOP_DISPOZITIE_DEFALCARE (id_alop_dispozitie, CodFunctional, id_oi_unitati, CodEconomic, Id_oi_Proiecte, TipBuget, SemnDispozitie, DisponibilInainte,  ' + lCopyFields + Sender.FieldName+', DisponibilDupa,  Moment, IdUtilizator)');
        Sql.Add('VALUES(:id_alop_dispozitie, :CodFunctional, :id_oi_unitati, :CodEconomic, :Id_oi_Proiecte, :TipBuget, :SemnDispozitie, :DisponibilInainte, ' + lCopyValues + ' @VALOARE, :DisponibilDupa,  GETDATE(), ' + IntToStr(IdUtilizator)+')');
        SQL.Add('select scope_identity()');
      end;
      DataSource := DTDefalcare;
      Open;
      lIdDisp := Fields[0].AsInteger;
      Close;
      SQL.Text := 'exec spAlopDispozitieRecalc ' + IntToStr(lIdDisp) + ', ''' + Sender.FieldName + '''';
      ExecSQL;
    finally
       Free;
    end;
  { Transmitem refresh pentru nodul pe care ne aflam acum }
  if not IsInLoading then
    PostMessage(Handle, WM_REFRESH_DATA, 0, 0);
end;

procedure TfrmAlopDispozitie.WMRefreshField(var Message: TMessage);
begin
  if not IsInLoading then begin
    DBRefresh(qryDispozitieDefalcare);
  end;
end;

procedure TfrmAlopDispozitie.DeschideDataSet;
begin
  if not qryDispozitieDefalcare.Active then
    qryDispozitieDefalcare.Open
  else begin
    if not IsInLoading then begin
      DBRefresh(qryDispozitieDefalcare);
    end;
  end;
end;


procedure TfrmAlopDispozitie.SetTabsOnId(IdRep: Integer);
var
  lDataSet  : TDataSet;
  I         : Integer;
begin
  tabFunctional.OnChange := nil;
  tabFunctional.Tabs.Clear;
  lDataSet := DBNewQueryFmt('exec spAlopDispCFonRep %d, %d', [IdRep, FCurentDispozitie]);
  try
    lDataSet.Open;
    if not lDataSet.IsEmpty then begin
      FCFArray := VarArrayCreate([0, lDataSet.RecordCount], varVariant);
      while not lDataSet.Eof do begin
        I := tabFunctional.Tabs.Add(lDataSet.Fields[1].AsString);
        FCFArray[I] := VarArrayOf([lDataSet.Fields[0].AsString, lDataSet.Fields[1].AsString, lDataSet.Fields[3].AsInteger, lDataSet.Fields[4].AsBoolean]);
        lDataSet.Next;
      end;
    end
    else
      FCFArray := Null
  finally
    lDataSet.Free;
  end;
  tabFunctional.OnChange := tabFunctionalChange;
end;

procedure TfrmAlopDispozitie.GridDispozitieFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
var
  lItem: TcxCustomGridTableItem;
begin
  if (AFocusedRecord <> nil) and AFocusedRecord.IsData then begin
    IdDefalcare := AFocusedRecord.Values[GridDispozitieIdDefalcare.Index];
    lItem := GridDispozitie.GetColumnByFieldName('esteFrunza');
    GridDispozitie.OptionsData.Editing := not Assigned(lItem) or ValueIsTrue(AFocusedRecord.Values[lItem.Index]);
  end;
end;

procedure TfrmAlopDispozitie.GridDispozitieStylesGetContentStyle(
  Sender: TcxCustomGridTableView; ARecord: TcxCustomGridRecord;
  AItem: TcxCustomGridTableItem; var AStyle: TcxStyle);
var
  lItem: TcxCustomGridTableItem;
begin
  AStyle := stilIntroducere;
  if Assigned(ARecord) and ARecord.IsData then begin
    lItem := GridDispozitie.GetColumnByFieldName('esteFrunza');
    if Assigned(lItem) then begin
      if ValueIsFalse(ARecord.Values[lItem.Index]) then
        AStyle := stilReadOnly;
    end;
  end;
end;

procedure TfrmAlopDispozitie.SetIdDefalcare(const Value: Variant);
begin
  FIdDefalcare := Value;
end;

procedure TfrmAlopDispozitie.qryTemplateAfterOpen(DataSet: TDataSet);
var
  lEditor : TcxDBCurrencyEdit;
  lLayoutItem : TdxLayoutItem;
begin
  //lcNotaJust.Clear;
  with DataSet do begin
    First;
    while not eof do begin
      if FindComponent('edt_'+ FieldByName('field_name').AsString) = nil then begin
        lEditor := TcxDBCurrencyEdit.Create(Self);
        lEditor.Name := 'edt_'+ FieldByName('field_name').AsString;
        lEditor.DataBinding.DataSource := DTDefalcare;
        lEditor.DataBinding.DataField := FieldByName('field_name').AsString;
        lEditor.Properties.DisplayFormat := ',0.00;-,0.00';
        lEditor.Properties.UseThousandSeparator := True;
        lEditor.Properties.UseDisplayFormatWhenEditing := True;
        lEditor.Properties.ReadOnly := (FieldByName('formula').AsString <> '');
        if lEditor.Properties.ReadOnly then
          lEditor.Style.Color := $00DBDBDB
        else
          lEditor.Style.Color := $00DDFFFF;

        lLayoutItem := TdxLayoutItem.Create(Self);
        lLayoutItem.Parent := lcNotaJustGroup_Root;
        lLayoutItem.Control := lEditor;
        lLayoutItem.CaptionOptions.Width := 150;
        lLayoutItem.CaptionOptions.Text := FieldByName('nr_crt').AsString + ' ' + FieldByName('descriere').AsString;
        lLayoutItem.ControlOptions.ShowBorder := False;
        lLayoutItem.ControlOptions.AlignVert := avCenter;
      end;
      Next;
    end;
  end;
end;

procedure TfrmAlopDispozitie.qryDispozitieDefalcareAfterOpen(
  DataSet: TDataSet);
var
  I : Integer;
  lField : TField;
  lFrunza: TcxGridDBColumn;
begin
  FCopyList.Clear;
  for I := 0 to lcNotaJust.ControlCount - 1 do
    if lcNotaJust.Controls[I] is TcxDBCurrencyEdit then begin
      lField := DataSet.FindField(TcxDBCurrencyEdit(lcNotaJust.Controls[I]).DataBinding.DataField);
      if Assigned(lField) then begin
        lField.ReadOnly := False;
        lField.OnChange := ValidateSuma;
        FCopyList.Add(lField.FieldName);
      end;
    end;
  lField := DataSet.FindField('SumaDispozitie');
  if Assigned(lField) then begin
    lField.ReadOnly := False;
    lField.OnChange := ValidateSuma;
  end;
  FCopyFields := '';
  FCopyValues := '';
  for I := 0 to FCopyList.Count -1  do begin
    FCopyFields := FCopyFields + FCopyList[I] +', ';
    FCopyValues := FCopyValues  + ':' + FCopyList[I]+ ', ';
  end;
  lField := DataSet.FindField('esteFrunza');
  if Assigned(lField) then begin
    lFrunza := GridDispozitie.GetColumnByFieldName(lField.FieldName);
    if not Assigned(lFrunza) then begin
      lFrunza := GridDispozitie.CreateColumn;
      lFrunza.DataBinding.FieldName := lField.FieldName;
      lFrunza.PropertiesClassName   := 'TcxCheckBoxProperties';
    end;
  end;
end;

procedure TfrmAlopDispozitie.ReportClick(Sender: TObject);
begin
  SetRapParam('id_alop_dispozitie', CurentDispozitie);
  LoadReport(TMenuItem(Sender).Tag);
end;

procedure TfrmAlopDispozitie.DataDispozitieChange(Sender: TField);
begin
  qryDispozitieDefalcare.ParamByName('Data_Dispozitie').AsDateTime := Sender.AsDateTime;
  if not IsInLoading then begin
    DBRefresh(qryDispozitieDefalcare);
  end;
end;

procedure TfrmAlopDispozitie.WMPostData(var Message: TMessage);
begin
  if qryDispozitie.State in [dsEdit, dsInsert] then qryDispozitie.Post;
end;

end.

