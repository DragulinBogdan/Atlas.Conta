
 unit OERepartitoriUnit;
interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ExtCtrls,  StdCtrls, Buttons, Db, ZDataSet, cxPC, cxControls,
  dxmdaset, cxStyles, cxGraphics,
  cxDataStorage, cxEdit, cxDBData, cxGridLevel, cxClasses,
  cxGridCustomView, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGrid, cxLookAndFeelPainters,
  cxButtons, Menus, cxTextEdit, cxContainer,
   cxSplitter, cxCurrencyEdit, cxMaskEdit,
  cxCheckBox, cxDropDownEdit, cxImageComboBox, cxButtonEdit,
  cxTL, frmOERepartioriEditUnit,
  cxInplaceContainer, cxDBTL, cxTLData, dxNavBarCollns,
  dxNavBarBase, dxNavBar,
  ZAbstractRODataset, ZAbstractDataset,
  cxTLdxBarBuiltInMenu,
  cxLookAndFeels, cxCustomData, cxFilter, cxData, cxGroupBox, cxLabel,
  cxLookupEdit, cxDBLookupEdit, cxDBLookupComboBox, dxBarBuiltInMenu,
  cxNavigator, dxScrollbarAnnotations, dxDateRanges, cxCustomListBox, cxListBox;

type
  TFrmOERepartitori = class(TForm)
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    pnDetalii: TPanel;
    pnTools: TPanel;
    BtnAdd: TcxButton;
    BtnDelete: TcxButton;
    BtnReunire: TcxButton;
    pageRepartitori: TcxPageControl;
    tabSolduri: TcxTabSheet;
    DTSolduriComplet: TDataSource;
    gridSolduriLevel: TcxGridLevel;
    gridLstSolduri: TcxGrid;
    qrySolduri: TZQuery;
    gridViewSolduri: TcxGridDBTableView;
    pnLeft: TPanel;
    pnScreen: TPanel;
    cxStyleRepository1: TcxStyleRepository;
    GridTableViewStyleSheetDevExpress: TcxGridTableViewStyleSheet;
    cxStyle1: TcxStyle;
    cxStyle2: TcxStyle;
    cxStyle3: TcxStyle;
    cxStyle4: TcxStyle;
    cxStyle5: TcxStyle;
    cxStyle6: TcxStyle;
    cxStyle7: TcxStyle;
    cxStyle8: TcxStyle;
    cxStyle9: TcxStyle;
    cxStyle10: TcxStyle;
    cxStyle11: TcxStyle;
    cxStyle12: TcxStyle;
    cxStyle13: TcxStyle;
    cxStyle14: TcxStyle;
    cxSplitter: TcxSplitter;
    NavBar: TdxNavBar;
    NavBarFiltru: TdxNavBarGroup;
    tabRep: TcxTabSheet;
    TreeListRep: TcxDBTreeList;
    TreeListRepNUME: TcxDBTreeListColumn;
    TreeListRepCONT_CRSP: TcxDBTreeListColumn;
    TreeListRepCODSECTIE: TcxDBTreeListColumn;
    TreeListRepADRESA: TcxDBTreeListColumn;
    TreeListRepGESTINT: TcxDBTreeListColumn;
    TreeListRepTIP_GESTIUNE: TcxDBTreeListColumn;
    TreeListRepID: TcxDBTreeListColumn;
    pnBottom: TPanel;
    btnAddAgentEconomic: TcxButton;
    btnRefresh: TcxButton;
    TreeListRepCOD_FISCAL: TcxDBTreeListColumn;
    TreeListRepTipRepartitor: TcxDBTreeListColumn;
    TimerDetalii: TTimer;
    btnRaportare: TcxButton;
    chkDrag: TcxCheckBox;
    btnModify: TcxButton;
    splitDetalii: TcxSplitter;
    gridViewSolduriID_SOLDURI_REPARTITORI: TcxGridDBColumn;
    gridViewSolduriID_REPARTITORI: TcxGridDBColumn;
    gridViewSolduriCONT: TcxGridDBColumn;
    gridViewSolduriSOLD_CREDITOR: TcxGridDBColumn;
    gridViewSolduriSOLD_DEBITOR: TcxGridDBColumn;
    gridViewSolduriCOD_FUNCTIONAL: TcxGridDBColumn;
    gridViewSolduriCOD_ECONOMIC: TcxGridDBColumn;
    gridViewSolduriID_OI_UNITATI: TcxGridDBColumn;
    gridViewSolduriID_OI_PROIECTE: TcxGridDBColumn;
    grDetaliiSold: TcxGroupBox;
    lbUnitate: TcxLabel;
    edUnitate: TcxLookupComboBox;
    lbProiect: TcxLabel;
    edProiect: TcxLookupComboBox;
    dtProiecte: TDataSource;
    qryProiecte: TZReadOnlyQuery;
    dtUnitate: TDataSource;
    qryUnitate: TZReadOnlyQuery;
    dtRepartitori: TDataSource;
    qryRepartitori: TZQuery;
    dtFunctional: TDataSource;
    qryFunctional: TZReadOnlyQuery;
    dtEconomic: TDataSource;
    qryEconomic: TZReadOnlyQuery;
    dtConturi: TDataSource;
    qryConturi: TZReadOnlyQuery;
    lbCont: TcxLabel;
    edCont: TcxLookupComboBox;
    edRepartitor: TcxLookupComboBox;
    lbRepartitor: TcxLabel;
    lbFunctional: TcxLabel;
    lbEconomic: TcxLabel;
    edEconomic: TcxLookupComboBox;
    edFunctional: TcxLookupComboBox;
    lbCredit: TcxLabel;
    lbDebit: TcxLabel;
    edDebit: TcxCurrencyEdit;
    edCredit: TcxCurrencyEdit;
    btnAdaugaSold: TcxButton;
    btnModificaSold: TcxButton;
    btnDeleteSold: TcxButton;
    tmrLoading: TTimer;
    lblLoading: TLabel;
    btnAdaugaCUIinMasa: TcxButton;
    cxListBox1: TcxListBox;
    procedure BtnCancelClick(Sender: TObject);
    procedure BtnOkClick(Sender: TObject);
    procedure BtnAddClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure BtnReunireClick(Sender: TObject);
    procedure BtnDeleteClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure pageRepartitoriChange(Sender: TObject);
    procedure TreeListRepFocusedNodeChanged(Sender: TCxCustomTreeList;
      APrevFocusedNode, AFocusedNode: TcxTreeListNode);
    procedure btnPlanificareClick(Sender: TObject);
    procedure GridBancaVFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure btnAddAgentEconomicClick(Sender: TObject);
    procedure btnRefreshClick(Sender: TObject);
    procedure TreeListRepDragOver(Sender, Source: TObject; X, Y: Integer;
      State: TDragState; var Accept: Boolean);
    procedure pnBottomResize(Sender: TObject);
    procedure TimerDetaliiTimer(Sender: TObject);
    procedure chkDragPropertiesEditValueChanged(Sender: TObject);
    procedure btnModifyClick(Sender: TObject);
    procedure gridViewSolduriFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure btnAdaugaSoldClick(Sender: TObject);
    procedure btnModificaSoldClick(Sender: TObject);
    procedure btnDeleteSoldClick(Sender: TObject);
    procedure tmrLoadingTimer(Sender: TObject);
    procedure btnAdaugaCUIinMasaClick(Sender: TObject);
  private
    { Private declarations }
    FIdRepartitor: Integer;
    FInfoRepartitor : TfrmOERepartitoriEdit;
    TipRepFilter : String;
    procedure RefreshAfterSave;

    procedure NavPopulateTipuriRepartitori;
    procedure NavFiltruClick(Sender : TObject);
    procedure SetIdRepartitor(const Value: Integer);
    procedure RepFilterRecord(DataSet: TDataSet; var Accept: Boolean);
    procedure ReportClick(Sender: TObject);

    procedure EditareRepartitor(aIdRepartitor : Integer);
  public
    { Public declarations }
    property IdRepartitor : Integer read FIdRepartitor write SetIdRepartitor;
  end;



procedure SelectRepartitor(var IdRepartitor : Integer; var Denumire : String); overload;
procedure SelectRepartitor(var IdRepartitor : Integer; var Denumire, Adresa, DenumireScurta : String); overload;
procedure IntretinereRepartitor(lCurentRep : Variant);


implementation

{$R *.DFM}

uses
  ZeosDBUtile,
  ATSZDBUtils, ReunireRepUnit, DateUnit, CommonDBVar,
  MainUnit, RapInclude,DBXJSON,RepartitorAnafUnit;


procedure IntretinereRepartitor(lCurentRep : Variant);
var
  fmSel : TFrmOERepartitori;
begin
  fmSel := TFrmOERepartitori.Create(nil);
  try
    fmSel.WindowState := wsMaximized;
    fmSel.ShowModal;
  finally
    fmSel.Free;
  end;
end;

procedure TFrmOERepartitori.BtnCancelClick(Sender: TObject);
begin
  if fsModal in FormState then ModalResult := mrCancel
                          else Close;
end;

procedure TFrmOERepartitori.BtnOkClick(Sender: TObject);
begin
  if fsModal in FormState then ModalResult := mrOk
  else begin
    Close;
  end;
end;

procedure TFrmOERepartitori.BtnAddClick(Sender: TObject);
begin
  with QryRepartitori do begin
    DisableControls;
    try
      Append;
      FieldByName('NUME').AsString := 'REPARTITOR NOU';
      FieldByName('TIP_REPARTITOR').AsString := '|' + TipRepFilter + '|';
      Post;
      if TipRepFilter <> '' then begin
        FInfoRepartitor.IdRepartitor := FieldByName('ID_REPARTITORI').AsInteger;
        FInfoRepartitor.SetTipRep(True, StrToInt(TipRepFilter));
      end;
    finally
      EnableControls;
    end;
  end;
  EditareRepartitor(QryRepartitori.FieldByName('id_repartitori').AsInteger);
   btnRefreshClick(nil);
end;

procedure TFrmOERepartitori.FormCreate(Sender: TObject);
begin

      lblLoading.Color := clYellow;
  lblLoading.Font.Color := clBlack;
  lblLoading.Font.Style := [fsBold];
  lblLoading.Alignment := taCenter;
  lblLoading.Layout := tlCenter;
  lblLoading.Transparent := False;
  lblLoading.AutoSize := False;
  lblLoading.Width := 500;
  lblLoading.Height := 59;
//     tmrLoading.Interval := 8000;
  tmrLoading.Enabled := True;

  PopulateReportContext(Self.ClassName, btnRaportare, ReportClick);
  DBRefresh([qryRepartitori]);
  FInfoRepartitor := TfrmOERepartitoriEdit.Create(Self);
  FInfoRepartitor.DTRepartitori.DataSet := qryRepartitori;
  FInfoRepartitor.IsReadOnly := True;
  FInfoRepartitor.Parent := pnDetalii;
  FInfoRepartitor.SetInfoConfig;
  NavPopulateTipuriRepartitori;
end;



procedure TFrmOERepartitori.BtnReunireClick(Sender: TObject);
var
   lNode : TcxDBTreeListNode;
begin
  lNode := TcxDBTreeListNode(TreeListRep.FocusedNode);
  if Assigned(lNode) then
    with TfrmReunireRep.Create(Self) do
      try
         IdRepartitor := lNode.KeyValue;
         ShowModal;
      finally
         Free;
      end;
end;

procedure TFrmOERepartitori.BtnDeleteClick(Sender: TObject);
var
  lQry : TZReadOnlyQuery;
  lFoundNotDeleted : Boolean;
  lIdRep : Integer;
  I : Integer;

   function TestAndDelete(lIdRep : Integer; const lShowExist : Boolean = True) : Boolean;
   var
     lExistRep : Boolean;
   begin
     lExistRep := False;
     if lQry <> nil then begin
       if lQry.Active then lQry.Close;
       lQry.Params.ParamByName('IdRep').Value := lIdRep;
       lQry.Open;
       lExistRep := lQry.Fields[0].AsInteger > 0;
     end;
     Result := lExistRep;
     if lShowExist and  lExistRep then
        if MessageDlg('Repartitorul este deja folosit in cadrul aplicatiei !'#13#10'Doriti totusi stergerea lui?',
                  mtError, [mbYes, mbNo], 0) <> mrYes then Abort;

     if not lShowExist and Result then Exit;

 with TZQuery.Create(nil) do
try
  Connection := qryRepartitori.Connection;
  SQL.Text := 'DELETE FROM repartitori WHERE ID_REPARTITORI = :ID';
  ParamByName('ID').AsInteger := lIdRep;
  ExecSQL;
finally
  Free;
end;
qryRepartitori.Refresh;


   end;


begin
  lQry := nil;
  if DBProcExists('SP_VERIFICARE_REPARTITOR') then
    lQry := DBNewQuery('EXEC SP_VERIFICARE_REPARTITOR  :IdRep');

  try
    QryRepartitori.DisableControls;
    lFoundNotDeleted := False;
    if TreeListRep.SelectionCount > 1 then begin
      if MessageDlg('Doriti stergerea repartitorilor selectati !', mtConfirmation, [mbYes, mbNo], 0) <> mrYes then Abort;
      for I := TreeListRep.SelectionCount - 1 downto 0 do begin
        //nu stergem parintele
        if TreeListRep.Selections[I].HasChildren then Continue;
        lIdRep := TreeListRep.Selections[I].Values[TreeListRepID.ItemIndex];
        lFoundNotDeleted := lFoundNotDeleted or TestAndDelete(lIdRep, False);
      end;
      if lFoundNotDeleted then begin
         MessageDlg('Nu s-au putut sterge toti repartitori selectati pentru ca o parte din ei sunt folositi in aplicatie !', mtInformation, [mbOK], 0);
         Abort;
      end;
    end
    else begin
      if MessageDlg('Doriti stergerea repartitorului curent !', mtConfirmation, [mbYes, mbNo], 0) <> mrYes then Abort;

      if TreeListRep.FocusedNode.HasChildren then begin
        MessageDlg('Nu s-a putut sterge repartitorul curent deoarece are repartitori subordonati !', mtError, [mbOK], 0);
        Abort;
      end;

      lIdRep := QryRepartitori.FieldByName('ID_REPARTITORI').AsInteger;
      TestAndDelete(lIdRep, True);
    end;
  finally
     QryRepartitori.EnableControls;
     FreeAndNil(lQry);
  end;
end;

procedure TFrmOERepartitori.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  if QryRepartitori.Filtered then begin
    QryRepartitori.Filtered := False;
    QryRepartitori.Filter := '';
  end;

  if FInfoRepartitor <> nil then
    FInfoRepartitor.Free;


    Action := caFree;
end;

procedure TFrmOERepartitori.pageRepartitoriChange(Sender: TObject);

  procedure RefreshDataSets(const ADataSets: array of TDataSet);
  var
    I: Integer;
  begin
    for I := Low(ADataSets) to High(ADataSets) do
      if ADataSets[I].Active then
        ADataSets[I].Refresh
      else
        ADataSets[I].Open;
  end;

begin
  if pageRepartitori.ActivePage = tabSolduri then
    RefreshDataSets([qryProiecte, qryUnitate, qryRepartitori, qryFunctional, qryEconomic, qryConturi, qrySolduri]);
end;

procedure TFrmOERepartitori.NavPopulateTipuriRepartitori;
var
  lNewBar : TdxNavBarItem;
  lDataSet: TDataSet;
begin
  NavBar.Items.Clear;
  lNewBar := NavBar.Items.Add;
  with lNewBar do begin
    Caption := '<Toate tipurile>';
    Tag := -1;
    NavBarFiltru.CreateLink(lNewBar);
    OnClick := NavFiltruClick;
  end;
  lDataSet := DBNewQuery('select * from repartitori_tipuri order by 1');
  try
    lDataSet.Open;
    while not lDataSet.Eof do begin
      lNewBar := NavBar.Items.Add;
      lNewBar.Caption := lDataSet.FieldByName('DENUMIRE').AsString;
      lNewBar.Tag := lDataSet.FieldByName('ID_REPARTITORI_TIPURI').AsInteger;
      lNewBar.OnClick := NavFiltruClick;
      NavBarFiltru.CreateLink(lNewBar);
      lDataSet.Next;
    end;
  finally
    lDataSet.Free;
  end;
end;

procedure TFrmOERepartitori.NavFiltruClick(Sender: TObject);
begin
  if not (Sender is TdxNavBarItem) then Exit;
  if TdxNavBarItem(Sender).Tag = -1 then TipRepFilter := ''
  else TipRepFilter := IntToStr(TdxNavBarItem(Sender).Tag);
  try
    QryRepartitori.DisableControls;
    QryRepartitori.Filtered := False;
    QryRepartitori.OnFilterRecord := nil;
    if TipRepFilter = '' then Exit;
    QryRepartitori.OnFilterRecord := RepFilterRecord;
    //FrmData.QryRepartitori.Filter := 'TIP_REPARTITOR  LIKE ' + QuotedStr('%'+TipRepFilter+'%') ;
    QryRepartitori.Filtered := True;
  finally
    QryRepartitori.EnableControls;
  end;
end;


procedure TFrmOERepartitori.TreeListRepFocusedNodeChanged(Sender: TCxCustomTreeList;
  APrevFocusedNode, AFocusedNode: TcxTreeListNode);
begin
   if Assigned(AFocusedNode) then
      IdRepartitor := QryRepartitori.FieldByName('ID_REPARTITORI').AsInteger
   else
      IdRepartitor := -1;
end;

procedure TFrmOERepartitori.btnPlanificareClick(Sender: TObject);
begin
  //MainForm.Cmd_BGAprobatExecute(nil);
end;

procedure TFrmOERepartitori.GridBancaVFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
  //
end;


procedure SelectRepartitor(var IdRepartitor : Integer; var Denumire : String);
var
  Adresa, DenumireScurta :String;
begin
  SelectRepartitor(IdRepartitor, Denumire, Adresa, DenumireScurta);
end;

procedure TFrmOERepartitori.btnAddAgentEconomicClick(Sender: TObject);
var
  frmCautareAgentEconomic : TfrmOERepartitoriEdit;
begin
  frmCautareAgentEconomic := TfrmOERepartitoriEdit.Create(nil);
  with frmCautareAgentEconomic do
    try
      DTRepartitori.DataSet := qryRepartitori;
      edtCUI.Text := ValueSafeToStr(QryRepartitori['COD_FISCAL']);
      FormStyle := fsNormal;
      Visible := False;
      ShowModal;
    finally
      Free;
    end;
end;

procedure TFrmOERepartitori.btnRefreshClick(Sender: TObject);
var Id : Integer;
begin
  with QryRepartitori do
  try
    if not IsEmpty then Id := FieldByName('ID_REPARTITORI').AsInteger;
    Close;
    Filter := '';
    Filtered := False;
    Open;
    if not IsEmpty then Locate('ID_REPARTITORI', Id, []);
  finally
  end;
end;

procedure SelectRepartitor(var IdRepartitor : Integer; var Denumire, Adresa, DenumireScurta : String);
var
  fmSel : TFrmOERepartitori;
begin
  IdRepartitor := -1;
  Denumire := '';
  fmSel := TFrmOERepartitori.Create(nil);
  try
    fmSel.pnLeft.Visible := False;
    fmSel.cxSplitter.Visible := False;
    fmSel.pnDetalii.Visible := False;
    fmSel.pageRepartitori.HideTabs := True;
    fmSel.FormStyle := fsNormal;
    fmSel.Visible := False;
    fmSel.ShowModal;
    if (fmSel.ModalResult = mrOk) and (fmSel.TreeListRep.FocusedNode <> nil) then begin
      Denumire := fmSel.TreeListRep.FocusedNode.Texts[fmSel.TreeListRepNUME.ItemIndex];
      DenumireScurta := fmSel.TreeListRep.FocusedNode.Texts[fmSel.TreeListRepCODSECTIE.ItemIndex];
      Adresa := fmSel.TreeListRep.FocusedNode.Texts[fmSel.TreeListRepADRESA.ItemIndex];
      IdRepartitor := TcxDBTreeListNode(fmSel.TreeListRep.FocusedNode).KeyValue;
    end;
  finally
    fmSel.Free;
  end;
end;


procedure TFrmOERepartitori.SetIdRepartitor(const Value: Integer);
begin
  TimerDetalii.Enabled := False;
  FIdRepartitor := Value;
  TimerDetalii.Enabled := True;
end;

procedure TFrmOERepartitori.TimerDetaliiTimer(Sender: TObject);
begin
  if (FInfoRepartitor <> nil) and (FInfoRepartitor.Visible) then begin
    if TreeListRep.SelectionCount = 1 then
      FInfoRepartitor.RefreshRow(FIdRepartitor);
    FInfoRepartitor.IdRepartitor := FIdRepartitor;
  end;
  TimerDetalii.Enabled := False;
end;

procedure TFrmOERepartitori.tmrLoadingTimer(Sender: TObject);
begin
      lblLoading.Visible := False;
  tmrLoading.Enabled := False;
end;

procedure TFrmOERepartitori.TreeListRepDragOver(Sender, Source: TObject; X,
  Y: Integer; State: TDragState; var Accept: Boolean);
begin
//
end;

procedure TFrmOERepartitori.pnBottomResize(Sender: TObject);
begin
  BtnCancel.Left := pnBottom.Width - BtnCancel.Width - 2;
  BtnOk.Left := BtnCancel.Left - BtnOk.Width - 5;
end;

procedure TFrmOERepartitori.RepFilterRecord(DataSet: TDataSet;
  var Accept: Boolean);
begin
  Accept := (pos('|'  + TipRepFilter + '|', DataSet.FieldByName('TIP_REPARTITOR').AsString) > 0 )
end;

procedure TFrmOERepartitori.ReportClick(Sender: TObject);
begin
  //SetRapParam('ID_REPARTITORI', QryRepartitori.FieldByName('ID_REPARTITORI').AsInteger);
  LoadReport(TMenuItem(Sender).Tag);
end;

procedure TFrmOERepartitori.chkDragPropertiesEditValueChanged(
  Sender: TObject);
begin
  if not chkDrag.Checked then TreeListRep.OnDragOver := nil
                     else TreeListRep.OnDragOver := TreeListRepDragOver;
end;


procedure TFrmOERepartitori.btnModifyClick(Sender: TObject);
begin
  EditareRepartitor(QryRepartitori.FieldByName('id_repartitori').AsInteger);
end;

procedure TFrmOERepartitori.EditareRepartitor(aIdRepartitor: Integer);
var
  lfrmCautareAgentEconomic: TfrmOERepartitoriEdit;
begin
  lfrmCautareAgentEconomic := TfrmOERepartitoriEdit.Create(nil);
  with lfrmCautareAgentEconomic do
    try
      DTRepartitori.DataSet := qryRepartitori;
      FormStyle := fsNormal;
      Visible := False;
      IdRepartitor := QryRepartitori.FieldByName('id_repartitori').AsInteger;

      if TipRepFilter <> '' then
        FInfoRepartitor.SetTipRep(True, StrToInt(TipRepFilter));

      SetEditConfig;


      OnAfterSave := RefreshAfterSave;

      while not (QryRepartitori.State in [dsEdit, dsInsert]) do
        QryRepartitori.Edit;

      ShowModal;

      if ModalResult = mrOk then
      begin
        if QryRepartitori.State in [dsEdit, dsInsert] then
          QryRepartitori.Post;
      end
      else
        QryRepartitori.Cancel;
    finally
      Free;
    end;
end;
 procedure TFrmOERepartitori.RefreshAfterSave;
begin
  btnRefreshClick(nil);
end;

procedure TFrmOERepartitori.gridViewSolduriFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
  if Assigned(AFocusedRecord) and AFocusedRecord.IsData then begin
    edUnitate.EditValue     := AFocusedRecord.Values[gridViewSolduriID_OI_UNITATI.Index];
    edProiect.EditValue     := AFocusedRecord.Values[gridViewSolduriID_OI_PROIECTE.Index];
    edRepartitor.EditValue  := AFocusedRecord.Values[gridViewSolduriID_REPARTITORI.Index];
    edCont.EditValue        := AFocusedRecord.Values[gridViewSolduriCONT.Index];
    edEconomic.EditValue    := AFocusedRecord.Values[gridViewSolduriCOD_ECONOMIC.Index];
    edFunctional.EditValue  := AFocusedRecord.Values[gridViewSolduriCOD_FUNCTIONAL.Index];
    edDebit.EditValue       := AFocusedRecord.Values[gridViewSolduriSOLD_DEBITOR.Index];
    edCredit.EditValue      := AFocusedRecord.Values[gridViewSolduriSOLD_CREDITOR.Index];
  end;
end;

procedure TFrmOERepartitori.btnAdaugaCUIinMasaClick(Sender: TObject);
var
  qry: TZQuery;
  count: Integer;
    counter: Integer;
begin
  cxListBox1.Visible := True;

  cxListBox1.Items.BeginUpdate;
  try
    cxListBox1.Items.Clear;

    qry := TZQuery.Create(nil);
    try
      qry.Connection := frmData.dbContabilitate;
      qry.SQL.Text :=
        'SELECT DISTINCT gd.cif_emitent ' +
        'FROM gest_docum_EFACT gd ' +
        'WHERE gd.cif_emitent IS NOT NULL ' +
        'AND NOT EXISTS (' +
        '  SELECT 1 FROM REPARTITORI r WHERE r.COD_FISCAL = gd.cif_emitent)';
      qry.Open;


      count := qry.RecordCount;


      cxListBox1.Items.Add('Total CUI-uri gasite care nu sunt in REPARTITORI: ' + IntToStr(count));
      cxListBox1.Items.Add('----------------------');

       counter := 0;

      while not qry.Eof do
      begin
        cxListBox1.Items.Add('CUI ' + ': ' + qry.FieldByName('cif_emitent').AsString);
        cxListBox1.Items.Add('----------------------');
        qry.Next;
      end;
    finally
      qry.Free;
    end;
  finally
    cxListBox1.Items.EndUpdate;
  end;
end;







procedure TFrmOERepartitori.btnAdaugaSoldClick(Sender: TObject);
begin
  gridViewSolduri.BeginUpdate();
  try
    qrySolduri.Append;
    qrySolduri['ID_OI_UNITATI']   := edUnitate.EditValue;
    qrySolduri['ID_OI_PROIECTE']  := edProiect.EditValue;
    qrySolduri['ID_REPARTITORI']  := edRepartitor.EditValue;
    qrySolduri['CONT']            := edCont.EditValue;
    qrySolduri['COD_ECONOMIC']    := edEconomic.EditValue;
    qrySolduri['COD_FUNCTIONAL']  := edFunctional.EditValue;
    qrySolduri['SOLD_DEBITOR']    := edDebit.EditValue;
    qrySolduri['SOLD_CREDITOR']   := edCredit.EditValue;
    qrySolduri.Post;
  finally
    gridViewSolduri.EndUpdate;
  end;
end;

procedure TFrmOERepartitori.btnModificaSoldClick(Sender: TObject);
begin
  gridViewSolduri.BeginUpdate();
  try
    qrySolduri.Edit;
    qrySolduri['ID_OI_UNITATI']   := edUnitate.EditValue;
    qrySolduri['ID_OI_PROIECTE']  := edProiect.EditValue;
    qrySolduri['ID_REPARTITORI']  := edRepartitor.EditValue;
    qrySolduri['CONT']            := edCont.EditValue;
    qrySolduri['COD_ECONOMIC']    := edEconomic.EditValue;
    qrySolduri['COD_FUNCTIONAL']  := edFunctional.EditValue;
    qrySolduri['SOLD_DEBITOR']    := edDebit.EditValue;
    qrySolduri['SOLD_CREDITOR']   := edCredit.EditValue;
    qrySolduri.Post;
  finally
    gridViewSolduri.EndUpdate;
  end;
end;

procedure TFrmOERepartitori.btnDeleteSoldClick(Sender: TObject);
begin
  if MessageDlg('Doriti stergerea pozitiei de sold ?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then
    qrySolduri.Delete;
end;

end.
