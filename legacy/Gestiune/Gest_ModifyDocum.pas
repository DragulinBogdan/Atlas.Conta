unit Gest_ModifyDocum;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs, StdCtrls,
  Db, ZDataSet, AlopDisponibil, Menus, cxButtons, cxEdit, cxMaskEdit, cxTextEdit,
  cxControls, cxPartialSearchUnit, cxPC, cxSplitter, ImgList, cxContainer, cxProgressBar,
  dxDBTLCl, dxDBTL, cxDropDownEdit, cxImageComboBox, dxExEdtr, cxLookAndFeelPainters,
  cxGraphics, dxDBGrid, ZAbstractRODataset, ZAbstractDataset, cxLookAndFeels, dxBarBuiltInMenu,
  cxStyles, cxCustomData, cxFilter, cxData, cxDataStorage, cxNavigator,
  cxDataControllerConditionalFormattingRulesManagerDialog, cxDBData,
  cxGridLevel, cxClasses, cxGridCustomView, cxGridCustomTableView,
  cxGridTableView, cxGridDBTableView, cxGrid, cxCalendar, cxCurrencyEdit, cxTL,
  cxCheckBox, cxTLdxBarBuiltInMenu, cxInplaceContainer, cxDBTL, cxTLData,
  Vcl.ExtCtrls, dxDateRanges, dxScrollbarAnnotations;

type
  TfrmGEST_ModifyDocum = class(TForm)
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    DTDocum: TDataSource;
    QryDocumListaDocum: TZReadOnlyQuery;
    DTItemsi: TDataSource;
    QryItemsiListaDocum: TZReadOnlyQuery;
    BtnAdauga: TcxButton;
    pnClient: TPanel;
    GrDocum: TGroupBox;
    GrDetalii: TGroupBox;
    BtnAnuleaza: TcxButton;
    BtnSterge: TcxButton;
    BtnRevalidariDocum: TcxButton;
    ppDetaliiMenu: TPopupMenu;
    ppIntroducereClasific: TMenuItem;
    itemFisaMaterial: TMenuItem;
    btnImpExp: TcxButton;
    SelectMenu: TPopupMenu;
    CmdImportXML: TMenuItem;
    ImportdinSQL1: TMenuItem;
    CmdExportXML: TMenuItem;
    CmdExportSQL: TMenuItem;
    SaveDialog: TSaveDialog;
    Splitter: TcxSplitter;
    ImgList: TImageList;
    cxTabControl: TcxTabControl;
    Progress: TcxProgressBar;
    pnDocument: TPopupMenu;
    mnuGenAng: TMenuItem;
    N1: TMenuItem;
    mnuSelDM: TMenuItem;
    mnuDezAsoc: TMenuItem;
    pnTop: TPanel;
    Label1: TLabel;
    edListaAni: TcxImageComboBox;
    Label2: TLabel;
    edListaLuni: TcxImageComboBox;
    Label3: TLabel;
    edOperator: TcxImageComboBox;
    btnRefresh: TcxButton;
    btnTiparire: TcxButton;
    mnuAsocierePlata: TMenuItem;
    N2: TMenuItem;
    viewDetaliiDocument: TcxGridDBTableView;
    nivelDetaliiDocument: TcxGridLevel;
    gridDetaliiDocument: TcxGrid;
    viewDetaliiDocumentTIPMAT: TcxGridDBColumn;
    viewDetaliiDocumentDESCRIERE: TcxGridDBColumn;
    viewDetaliiDocumentDENMAT: TcxGridDBColumn;
    viewDetaliiDocumentUM: TcxGridDBColumn;
    viewDetaliiDocumentDATA_COD: TcxGridDBColumn;
    viewDetaliiDocumentDATA_EXPIRARE: TcxGridDBColumn;
    viewDetaliiDocumentTIP_MATERIAL: TcxGridDBColumn;
    viewDetaliiDocumentCANTITATE: TcxGridDBColumn;
    viewDetaliiDocumentPRET_UNITAR: TcxGridDBColumn;
    viewDetaliiDocumentPRET_UNITAR_VALUTA: TcxGridDBColumn;
    viewDetaliiDocumentCOTA_TVA: TcxGridDBColumn;
    viewDetaliiDocumentPRET_TVA: TcxGridDBColumn;
    viewDetaliiDocumentPRET_TOTAL: TcxGridDBColumn;
    viewDetaliiDocumentTVA: TcxGridDBColumn;
    viewDetaliiDocumentPRET_TOTAL_TVA: TcxGridDBColumn;
    viewDetaliiDocumentCODMAT: TcxGridDBColumn;
    stiluriAfisare: TcxStyleRepository;
    stilInregistrareCurenta: TcxStyle;
    stilDocumentCurent: TcxStyle;
    stilDocumentConex: TcxStyle;
    stilDocumentAnulat: TcxStyle;
    treeDocument: TcxDBTreeList;
    TreeDocumentID_GEST_DOCUM: TcxDBTreeListColumn;
    TreeDocumentID_INITIAL: TcxDBTreeListColumn;
    TreeDocumentCOD_DOCUM: TcxDBTreeListColumn;
    TreeDocumentPREDATOR: TcxDBTreeListColumn;
    TreeDocumentPRIMITOR: TcxDBTreeListColumn;
    TreeDocumentNR_DOCUM: TcxDBTreeListColumn;
    TreeDocumentDATA_DOCUM: TcxDBTreeListColumn;
    TreeDocumentTOTAL_DOCUMENT: TcxDBTreeListColumn;
    TreeDocumentTOTAL_TVA: TcxDBTreeListColumn;
    TreeDocumentNUMEINTREG: TcxDBTreeListColumn;
    TreeDocumentDATA_OPERARE: TcxDBTreeListColumn;
    TreeDocumentID_DOCUMENT_CONEX: TcxDBTreeListColumn;
    TreeDocumentID_TRANZACTIE: TcxDBTreeListColumn;
    TreeDocumentAUTOGENERAT: TcxDBTreeListColumn;
    TreeDocumentSTARE_DOCUMENT: TcxDBTreeListColumn;
    procedure FormCreate(Sender: TObject);
    procedure BtnAdaugaClick(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure BtnAnuleazaClick(Sender: TObject);
    procedure FormResize(Sender: TObject);
    procedure FormKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure BtnStergeClick(Sender: TObject);
    procedure BtnRevalidariDocumClick(Sender: TObject);
    procedure ppIntroducereClasificClick(Sender: TObject);
    procedure itemFisaMaterialClick(Sender: TObject);
    procedure CmdExportSQLClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure CmdImportXMLClick(Sender: TObject);
    procedure cxTabControlDrawTabEx(AControl: TcxCustomTabControl;
      ATab: TcxTab; Font: TFont);
    procedure BtnOkClick(Sender: TObject);
    procedure BtnCancelClick(Sender: TObject);
    procedure mnuGenAngClick(Sender: TObject);
    procedure mnuSelDMClick(Sender: TObject);
    procedure mnuDezAsocClick(Sender: TObject);
    procedure btnRefreshClick(Sender: TObject);
    procedure edListaAniPropertiesChange(Sender: TObject);
    procedure edListaLuniPropertiesChange(Sender: TObject);
    procedure cxTabControlChange(Sender: TObject);
    procedure btnTiparireClick(Sender: TObject);
    procedure mnuAsocierePlataClick(Sender: TObject);
    procedure treeDocumentGetNodeImageIndex(Sender: TcxCustomTreeList;
      ANode: TcxTreeListNode; AIndexType: TcxTreeListImageIndexType;
      var AIndex: TImageIndex);
    procedure treeDocumentStylesGetContentStyle(Sender: TcxCustomTreeList;
      AColumn: TcxTreeListColumn; ANode: TcxTreeListNode; var AStyle: TcxStyle);
    procedure treeDocumentFocusedNodeChanged(Sender: TcxCustomTreeList;
      APrevFocusedNode, AFocusedNode: TcxTreeListNode);
  private
    FIsSelection: Boolean;
    FirstRunThis : Boolean;
    FDetaliereDocum : TfrmAlopDisponibil;
    FrmTCV : TCustomForm;
    FIdInitial        : Variant;
    FIdDocumentConex  : Variant;
    FIdGestDocum : Integer;
    FClassic : Boolean;
    FInLoading : Boolean;
    //FIdFurnzior : Integer;
    function  fmDetaliereDocum: TfrmAlopDisponibil;
    procedure RefreshScreen;
    procedure SetSQL;
    procedure TestClassic;
    procedure DefineListe;
    function GetIsTemporar: Boolean;
    function GetNodeDetails(ANode: TcxTreeListNode = nil): String;
  protected
    function GetDocumentID(ANode: TcxTreeListNode = nil): Integer;
    { Private declarations }
  public
    { Public declarations }
    procedure SetToFactura;
    procedure SetToItemsi;
    property IsTemporar : Boolean read GetIsTemporar;
    property IsSelection: Boolean read FIsSelection write FIsSelection;
    property SelectedDocument : Integer read FIdGestDocum;
  end;

implementation

{$R *.DFM}

uses
  dxCompsUtile,
  ZeosDBUtile,
  Variants,
  TCVUnit, DateUnit, ATSZDBUtils, CommonDBVar, SetParamsUnitADO,
  rapInclude,
  fmPlataDocumUnit,
  AlopAngajamente, ImportTethys;

procedure TfrmGEST_ModifyDocum.FormCreate(Sender: TObject);
begin
  FClassic := True;
  FIdGestDocum := -1;

  FirstRunThis := True;

  Progress.Visible := False;
  FInLoading := True;
  TestClassic;
  DefineListe;
  FInLoading := False;
  RefreshScreen;

  WindowState := wsMaximized;
end;

procedure TfrmGEST_ModifyDocum.BtnAdaugaClick(Sender: TObject);
begin
  SetToFactura;
end;

procedure TfrmGEST_ModifyDocum.SetToFactura;
begin
  FrmTCV := TFrmTCV.Create(Self);
  with TFrmTCV(FrmTCV) do begin
    Parent      := GrDetalii;
    BorderStyle := bsNone;
    Align       := alClient;
    Visible     := True;
    NotifyForm  := Self;
    ReadDocument;
  end;
  gridDetaliiDocument.Visible  := False;
  QryItemsiListaDocum.Active    := False;
end;

procedure TfrmGEST_ModifyDocum.SetToItemsi;
begin
  QryItemsiListaDocum.Active    := True;
  gridDetaliiDocument.Visible  := True;
end;

procedure TfrmGEST_ModifyDocum.FormDestroy(Sender: TObject);
begin
  if Assigned(FrmTCV) then
     TFrmTCV(FrmTCV).NotifyForm := nil;
end;

procedure TfrmGEST_ModifyDocum.BtnAnuleazaClick(Sender: TObject);
var
  lId  : Integer;
begin
  lId := GetDocumentID;
  if MessageDlg('Doriti anulearea documentului : ' + GetNodeDetails, mtConfirmation, [mbYes, mbNo], 0) = mrYes then begin
    if GetIsTemporar then
      DBExecSqlFmt('exec [spGestAnuleazaTMPDocum] %d', [lId])
    else
    DBExecSqlFmt('exec [spGestAnuleazaDocum] %d', [lId]);
    RefreshScreen;
  end;
end;

procedure TfrmGEST_ModifyDocum.FormResize(Sender: TObject);
begin
  GrDocum.Height := pnClient.Height div 2;
end;

procedure TfrmGEST_ModifyDocum.FormKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if (ssCtrl in Shift) and (ssShift in Shift) and (ssAlt in Shift) and
     ( (pos('ADMIN', UpperCase(NumeLogin)) = 1) or
       (pos('DELPHI', UpperCase(NumeLogin)) > 0) )then begin
     BtnSterge.Visible          := not(BtnSterge.Visible);
     BtnSterge.Enabled          := BtnSterge.Visible;
     BtnRevalidariDocum.Visible := BtnSterge.Visible;
     BtnRevalidariDocum.Enabled := BtnSterge.Visible;
     Progress.Visible           := BtnSterge.Visible;
     btnImpExp.Visible          := BtnSterge.Visible;
  end;
end;

procedure TfrmGEST_ModifyDocum.BtnStergeClick(Sender: TObject);
var
  lId  : Integer;
begin
  lId := GetDocumentID();
  if MessageDlg('Doriti stergerea documentului : ' + GetNodeDetails(), mtConfirmation, [mbYes, mbNo], 0) = mrYes then begin
    DBExecSqlFmt('exec [spGestStergeDoc] %d', [lId]);
    FormCreate(Self);
  end;
end;

procedure TfrmGEST_ModifyDocum.BtnRevalidariDocumClick(Sender: TObject);
var
  I, lRetry, lId, J: Integer;
  lNode: TcxTreeListNode;

    procedure ResetProgress;
     begin
        if treeDocument.Count > 1 then Progress.Properties.Max := treeDocument.Count-1
        else Progress.Properties.Max := 1;
        Progress.Position := 0;
        Caption :=  'Modificare Document';
        Application.ProcessMessages;
     end;

begin
  { Parcurgem documentele si le revalidam, generam si forma grafica asociata }
  try
    Progress.Properties.Min := 0;
    ResetProgress;
    I := 0;
    lRetry := 0;
    while I <= treeDocument.Count-1 do
    try
      lNode := treeDocument.Items[I];
      lId   := GetDocumentID(lNode);
      DateUnit.IdGestDocum := lId;
      PrintDocument2DBFR(False, False);
      DBExecSQLFmt('exec [spGestRebuildValidari] %d', [lId]);
      if lNode.HasChildren then
        for J := 0 to lNode.Count - 1 do begin
          IdGestDocum := GetDocumentID(lNode.Items[J]);
          PrintDocument2DBFR(False, False);
          DBExecSQLFmt('exec [spGestRebuildValidari] %d', [lId]);
        end;
      Progress.Position := I;
      Caption :=  'Modificare Document' + '[' + GetNodeDetails(lNode) + ']';
      Application.ProcessMessages;
      I := I + 1;
      lRetry := 0;
    except
      on E:Exception do
        if lRetry = 3 then
          case MessageDlg(Format('Eroare la reprocesare document [%s] ! '+ #13#10 + E.Message, [lId]), mtError, [mbAbort, mbRetry, mbIgnore], 0) of
            mrAbort : begin Break; end;
            mrRetry : begin lRetry := 0; end;
            mrIgnore : begin I := I + 1; lRetry := 0; end;
          end
        else lRetry := lRetry + 1;
    end;
  finally
    IdGestDocum := -1;
  end;
  ResetProgress;
end;

procedure TfrmGEST_ModifyDocum.ppIntroducereClasificClick(Sender: TObject);
var
  lRecord : TcxCustomGridRecord;
  lItemId: Integer;
  lFurnizor, lIdAng, lIdUnitate, lIdProiect, lIdOrd,
  lCodF, lCodEc,
  lDataExecutie : Variant;
begin
  lRecord := viewDetaliiDocument.Controller.FocusedRecord;
  if Assigned(lRecord) and lRecord.IsData then begin
    lFurnizor      := QryDocumListaDocum['ID_FURNIZOR'];
    lIdAng         := QryItemsiListaDocum['ID_ANGAJAMENTE_DEFALCARE'];
    lIdOrd         := QryItemsiListaDocum['ID_ORDONANTARE_DEFALCARE'];
    lCodF          := QryItemsiListaDocum['cod_functional'];
    lIdUnitate     := QryItemsiListaDocum['ID_OI_UNITATI'];
    lCodEc         := QryItemsiListaDocum['cod_economic'];
    lIdProiect     := QryItemsiListaDocum['ID_OI_PROIECTE'];
    lDataExecutie  :=  QryItemsiListaDocum['DATA_DOCUM'];

    fmDetaliereDocum.Position := poScreenCenter;
    DBRefresh(fmDetaliereDocum.QryAngajamente);
    fmDetaliereDocum.PrepareCulegere(lFurnizor, lCodF, lCodEc, lIdAng, lIdOrd, lIdUnitate, lIdProiect, lDataExecutie);
    if fmDetaliereDocum.ShowModal = mrOk then begin
      fmDetaliereDocum.UpdateSelected;
      DBExecSQLFmt('UPDATE GEST_ITEMSI SET ID_ANGAJAMENTE_DEFALCARE = %s, ID_ORDONANTARE_DEFALCARE = %s, COD_FUNCTIONAL = %s, COD_ECONOMIC = %s, ID_OI_UNITATI = %s, ID_OI_PROIECTE = %s '+
                   'WHERE ID_GEST_ITEMSI = %s', [
                    ValueToStr(fmDetaliereDocum.IdAngajament),
                    ValueToStr(fmDetaliereDocum.IdOrdonantare),
                    ValueToStr(fmDetaliereDocum.CodFunctional),
                    ValueToStr(fmDetaliereDocum.CodEconomic),
                    ValueToStr(fmDetaliereDocum.IdUnitati),
                    ValueToStr(fmDetaliereDocum.IdProiecte),
                    ValuetoStr(lItemId)
                    ]);
      DBRefresh(QryItemsiListaDocum);
    end;
  end;
end;

function TfrmGEST_ModifyDocum.fmDetaliereDocum: TfrmAlopDisponibil;
begin
  if not Assigned(FDetaliereDocum) then
    FDetaliereDocum := TfrmAlopDisponibil.Create(Self);
  Result := FDetaliereDocum;
end;

procedure TfrmGEST_ModifyDocum.itemFisaMaterialClick(Sender: TObject);
var
  lRecord : TcxCustomGridRecord;
begin
  lRecord := viewDetaliiDocument.Controller.FocusedRecord;
  if Assigned(lRecord) and (lRecord.IsData) then begin
    RegisterCRAdoParam('COD_MAT', ftInteger, True).Value := lRecord.Values[viewDetaliiDocumentCODMAT.Index];
    LoadReport(DateUnit.GetItemId('FisaMaterial'));
  end;
end;

procedure TfrmGEST_ModifyDocum.CmdExportSQLClick(Sender: TObject);
var
  lFileName : String;
  lFileContent : TStringList;
  lNode     : TcxTreeListNode;
  lDataSet  : TDataSet;
begin
  lNode := treeDocument.FocusedNode;
  if (FIdGestDocum = -1) or not Assigned(lNode) then Exit;

  if (MessageDlg(Format('Doriti exportul documentului %s din %s ?', [lNode.Texts[TreeDocumentNR_DOCUM.ItemIndex], lNode.Texts[TreeDocumentDATA_DOCUM.ItemIndex]] ), mtConfirmation, [mbYes, mbNo], 0) = mrNo) then Abort;

  SaveDialog.Filter := 'SQL Script File|*.SQL';
  if not SaveDialog.Execute then Exit;

  lFileName := SaveDialog.FileName;
  try
    lFileContent := TStringList.Create;
    lDataSet := DBNewQueryFmt('exec [SP_MODIFYDOC_SQL_HEADER] %d', [FIdGestDocum]);
    try
      lDataSet.Open;
      while not lDataSet.Eof do begin
        lFileContent.Add(lDataSet.Fields[0].AsString);
        lDataSet.Next;
      end;
      DBSetSQLQueryFmt(lDataSet, 'exec [SP_MODIFYDOC_SQL_GENERATE] %d', [FIdGestDocum]);
      lDataSet.Open;
      while not lDataSet.Eof do begin
        lFileContent.Add(lDataSet.Fields[0].AsString);
        lDataSet.Next;
      end;
      DBSetSQLQueryFmt(lDataSet, 'exec SP_MODIFYDOC_SQL_FOOTER %d', [FIdGestDocum]);
      lDataSet.Open;
      while not lDataSet.Eof do begin
        lFileContent.Add(lDataSet.Fields[0].AsString);
        lDataSet.Next;
      end;
    finally
      lDataSet.Free;
    end;
    lFileContent.SaveToFile(lFileName);
  finally
    lFileContent.Free;
  end;
end;

procedure TfrmGEST_ModifyDocum.RefreshScreen;
Var
 I : Integer;
 lDataSet : TDataSet;
 lColumn  : TcxGridDBColumn;
 lDocColumn: TcxDBTreeListColumn;
begin
  if FInLoading then Exit;
  if QryDocumListaDocum.Active then QryDocumListaDocum.Close;
  if QryItemsiListaDocum.Active then QryItemsiListaDocum.Close;

  if QryDocumListaDocum.Params.Count > 0 then begin
    QryDocumListaDocum.Params.ParamByName('an').Value := edListaAni.EditValue;
    QryDocumListaDocum.Params.ParamByName('luna').Value := edListaLuni.EditValue;
    QryDocumListaDocum.Params.ParamByName('utilizator').Value := edOperator.EditValue;
  end;

  treeDocument.OnFocusedNodeChanged := nil;
  try
    QryDocumListaDocum.Open;
  finally
    treeDocument.OnFocusedNodeChanged := treeDocumentFocusedNodeChanged;
  end;

  if FirstRunThis then begin
    if DBProcExists('spImplicitItemsiDocumFields') then begin
      lDataSet := DBNewQueryFmt('exec [spImplicitItemsiDocumFields] %d, %d', [IdLogin, IdUtilizator]);
      lDataSet.Open;
    end
    else begin
      lDataSet := nil;
    end;
    try
      for I := 0 to QryItemsiListaDocum.FieldCount - 1 do begin
        viewDetaliiDocument.BeginUpdate;
        //zona ats
        if viewDetaliiDocument.GetColumnByFieldName(QryItemsiListaDocum.Fields[I].FieldName) = nil then begin
          lColumn := viewDetaliiDocument.CreateColumn;
          lColumn.DataBinding.FieldName := QryItemsiListaDocum.Fields[I].FieldName;
          lColumn.HeaderAlignmentHorz := taCenter;
          lColumn.Caption := GetNiceText(QryItemsiListaDocum.Fields[I].FieldName);
          lColumn.Width   := 40;
          lColumn.Visible := Assigned(lDataSet) and lDataSet.Active and Assigned(lDataSet.FindField(QryItemsiListaDocum.Fields[I].FieldName));
        end;
        viewDetaliiDocument.EndUpdate;
      end;
    finally
      if Assigned(lDataSet) then lDataSet.Free;
    end;

    for I := 0 to QryDocumListaDocum.FieldCount - 1 do begin
      //zona ats
      treeDocument.BeginUpdate;
      if treeDocument.GetColumnByFieldName(QryDocumListaDocum.Fields[I].FieldName) = nil then begin
        lDocColumn := treeDocument.CreateColumn() as TcxDBTreeListColumn;
        lDocColumn.DataBinding.FieldName := QryDocumListaDocum.Fields[I].FieldName;
        lDocColumn.Caption.AlignHorz := taCenter;
        lDocColumn.Caption.Text      := GetNiceText(QryDocumListaDocum.Fields[I].FieldName);
        lDocColumn.Width             := 40;
        lDocColumn.Visible           := False;
      end;
      treeDocument.EndUpdate;
    end;
    FirstRunThis := False;
  end;

  treeDocument.ApplyBestFit;
  viewDetaliiDocument.ApplyBestFit(nil);
  Caption :=  'Modificare Document';
end;

procedure TfrmGEST_ModifyDocum.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmGEST_ModifyDocum.CmdImportXMLClick(Sender: TObject);
begin
//
end;

procedure TfrmGEST_ModifyDocum.treeDocumentFocusedNodeChanged(
  Sender: TcxCustomTreeList; APrevFocusedNode, AFocusedNode: TcxTreeListNode);
begin
  if Assigned(AFocusedNode) then begin
    FIdInitial        := AFocusedNode.Values[TreeDocumentID_INITIAL.ItemIndex];
    FIdDocumentConex  := AFocusedNode.Values[TreeDocumentID_DOCUMENT_CONEX.ItemIndex];
    FIdGestDocum      := AFocusedNode.Values[TreeDocumentID_GEST_DOCUM.ItemIndex];
    BtnOk.Enabled     := ValueSameValue(FIdInitial, FIdGestDocum);
    DBRefresh(QryItemsiListaDocum);
  end;
end;

procedure TfrmGEST_ModifyDocum.treeDocumentGetNodeImageIndex(
  Sender: TcxCustomTreeList; ANode: TcxTreeListNode;
  AIndexType: TcxTreeListImageIndexType; var AIndex: TImageIndex);
begin
  case AIndexType of
    tlitImageIndex,
    tlitSelectedIndex:
      if ANode.Level = 1 then AIndex := 2 else
      if ANode.HasChildren then AIndex := 1 else AIndex := 0;
  end;
end;

procedure TfrmGEST_ModifyDocum.treeDocumentStylesGetContentStyle(
  Sender: TcxCustomTreeList; AColumn: TcxTreeListColumn; ANode: TcxTreeListNode;
  var AStyle: TcxStyle);
begin
  if ANode.Focused then AStyle := stilInregistrareCurenta
  else
  if ValueSameValue(FIdInitial, ANode.Values[TreeDocumentID_INITIAL.ItemIndex]) then
    AStyle := stilDocumentCurent
  else
  if ValueSameValue(FIdDocumentConex, ANode.Values[TreeDocumentID_DOCUMENT_CONEX.ItemIndex]) then
    AStyle := stilDocumentConex;
end;

procedure TfrmGEST_ModifyDocum.cxTabControlDrawTabEx(
  AControl: TcxCustomTabControl; ATab: TcxTab; Font: TFont);
begin
  if fsCreating in FormState then Exit;
  if ATab.Index = 0 then ATab.Color := $00D5FFAA
  else ATab.Color := $009595FF;
end;

procedure TfrmGEST_ModifyDocum.SetSQL;
begin
  if QryDocumListaDocum.Active then QryDocumListaDocum.Close;
  if QryItemsiListaDocum.Active then QryItemsiListaDocum.Close;
  if FClassic or (cxTabControl.TabIndex = 0) then begin
    if DBProcExists('SP_GEST_ITEMSILISTADOCUM') then begin
         QryItemsiListaDocum.SQL.Clear;
         QryItemsiListaDocum.SQL.Add('EXEC SP_GEST_ITEMSILISTADOCUM :ID_GEST_DOCUM');
    end;
    if DBProcExists('SP_GEST_DOCUMLISTADOCUM') then begin
         QryDocumListaDocum.SQL.Clear;
         QryDocumListaDocum.SQL.Add('EXEC SP_GEST_DOCUMLISTADOCUM :an, :luna, :utilizator');
    end;
  end
  else begin
    QryItemsiListaDocum.SQL.Clear;
    QryItemsiListaDocum.SQL.Add('EXEC SP_GEST_TMP_ITEMSILISTADOCUM :ID_GEST_DOCUM');
    QryDocumListaDocum.SQL.Clear;
    QryDocumListaDocum.SQL.Add('EXEC SP_GEST_TMP_DOCUMLISTADOCUM :an, :luna, :utilizator');
  end;
end;

procedure TfrmGEST_ModifyDocum.TestClassic;
begin
  if DBProcExists('SP_GEST_TMP_DOCUMLISTADOCUM') and
     DBProcExists('SP_GEST_TMP_ITEMSILISTADOCUM') and
     DBProcExists('SP_GEST_TMP_DOCUMLISTADOCUM')
  then
    FClassic := False;
  if FClassic then begin
      cxTabControl.Visible := False;
    SetSQL;
  end
  else begin
    cxTabControl.Visible := True;
    SetSQL;
  end;
end;

procedure TfrmGEST_ModifyDocum.BtnOkClick(Sender: TObject);
begin
  if FIsSelection and (FIdGestDocum = -1) then
    raise EContaHandledError.Create('Va rugam selectati un anumit document pentru modificare !');
  if fsModal in FormState then ModalResult := mrOk
  else Close;
end;

procedure TfrmGEST_ModifyDocum.BtnCancelClick(Sender: TObject);
begin
  if fsModal in FormState then ModalResult := mrCancel
  else Close;
end;

procedure TfrmGEST_ModifyDocum.mnuGenAngClick(Sender: TObject);
var
  lIdGestDocum    : Integer;
  lIdAngajamente  : Integer;
begin
  lIdGestDocum := GetDocumentID();
  lIdAngajamente := DBGetScallarFmt('exec [spAlopAngajamentTCV] %d, %d', [lIdGestDocum, IdUtilizator], 0);
  if lIdAngajamente <> 0 then
     ModificareAngajament(lIdAngajamente)
  else
     MessageDlg('Nu s-au putut determina automat parametrii pentru ordonantare', mtError, [mbOK], 0);
end;

procedure TfrmGEST_ModifyDocum.mnuSelDMClick(Sender: TObject);
var
  lRec    : PRecTethys;
  aQry    : TZReadOnlyQuery;
  lIdGestDocum: Integer;
  lDocTip : String;
begin
  lIdGestDocum := GetDocumentID();
  lDocTip := DBGetScallarFmt('exec [spTethysTipDoc] %d', [lIdGestDocum], 0);
  lRec    := SelectieLegatura(True, lDocTip);
  if Assigned(lRec) then begin
    DBExecSQLFmt('exec [spTethysAddConex] %d, %s, %s, %s, %s, %d', [
      lIdGestDocum,
      ValueToStr(lRec.registruID),
      ValueToStr(lRec.stareID),
      ValueToStr(lRec.tipDocumentID),
      ValueToStr(lRec.dataInreg),
      lRec.nrInreg
      ]);
    DBRefresh(QryDocumListaDocum);
  end;
end;

procedure TfrmGEST_ModifyDocum.mnuAsocierePlataClick(Sender: TObject);
var
  lIdGestDocum : Integer;
begin
  lIdGestDocum := GetDocumentID();
  if ModificaPlataDocument(lIdGestDocum) then DBRefresh(QryDocumListaDocum);
end;

procedure TfrmGEST_ModifyDocum.mnuDezAsocClick(Sender: TObject);
begin
  DBExecSQLFmt('exec [spTethysDelConex] %d, null, null, null, null, null', [GetDocumentID()]);
end;

procedure TfrmGEST_ModifyDocum.btnRefreshClick(Sender: TObject);
begin
  RefreshScreen;
end;

procedure TfrmGEST_ModifyDocum.DefineListe;
var
  I       : Integer;
  lDataSet: TDataSet;
begin
  FillImageCombo(edOperator.Properties, frmData.QryOperatori, 'ID_UTILIZATORI', 'NUMEINTREG', -1, 'Toti Utilizatorii');
  if Trim(edOperator.EditText) = '' then
    edOperator.EditValue := -1;
  edListaAni.Properties.Items.Clear;
  with edListaAni.Properties.Items.Add do begin
    Description := 'Toti Anii';
    Value := Integer(-1);
  end;
  lDataSet := DBNewQuery('exec [spGestDocumGetAni]');
  try
    lDataSet.Open;
    while not lDataSet.Eof do begin
      with edListaAni.Properties.Items.Add do begin
        Description := lDataSet.Fields[0].AsString;
        Value := lDataSet.Fields[0].AsInteger;
      end;
      lDataSet.Next;
    end;
  finally
    lDataSet.Free;
  end;

  if edListaAni.Properties.Items.Count > 0 then begin
    edListaAni.EditValue := edListaAni.Properties.Items[edListaAni.Properties.Items.Count-1].Value;

    for I := 0 to edListaAni.Properties.Items.Count - 1 do
      if edListaAni.Properties.Items[I].Value = AnFiscal then begin
        edListaAni.EditValue := AnFiscal;
        Break;
      end;
  end;

end;

procedure TfrmGEST_ModifyDocum.edListaAniPropertiesChange(Sender: TObject);
begin
  edListaLuni.Properties.Items.Clear;
  with edListaLuni.Properties.Items.Add do begin
    Description := 'Toate Lunile';
    Value := Integer(-1);
  end;
  with GetTmpADOQuery do
    try
       Sql.Add('exec spGestDocumGetLuniAn '+ IntToStr(edListaAni.EditValue));
       Open;
       while not Eof do begin
         with edListaLuni.Properties.Items.Add do begin
            Description := LongMonthNames[Fields[0].AsInteger];
            Value := Fields[0].AsInteger;
         end;
         Next;
       end;
       if edListaLuni.Properties.Items.Count > 0 then
          edListaLuni.EditValue := edListaLuni.Properties.Items[0].Value;
    finally
       Free;
    end;
  RefreshScreen;
end;

procedure TfrmGEST_ModifyDocum.edListaLuniPropertiesChange(
  Sender: TObject);
begin
  RefreshScreen;
end;

function TfrmGEST_ModifyDocum.GetDocumentID(ANode: TcxTreeListNode): Integer;
begin
  if not Assigned(ANode) then ANode := TreeDocument.FocusedNode;
  if not Assigned(ANode) then
    raise Exception.Create('Nu aveti nici un document selectat !');
  Result := ValueSafeToInt(ANode.Values[TreeDocumentID_GEST_DOCUM.ItemIndex]);
end;

function TfrmGEST_ModifyDocum.GetIsTemporar: Boolean;
begin
  Result := False;
  if (cxTabControl.TabIndex>0) and cxTabControl.Visible then
    Result := True;
end;

function TfrmGEST_ModifyDocum.GetNodeDetails(ANode: TcxTreeListNode = nil): String;
begin
  if not Assigned(ANode) then ANode := treeDocument.FocusedNode;
  if not Assigned(ANode) then
    raise Exception.Create('Nu aveti nici un document selectat !');
  Result := Format('%s nr. : %s din : %s '#13#10' de la : %s la : %s',
                   [
                    ANode.Texts[TreeDocumentCOD_DOCUM.ItemIndex],
                    ANode.Texts[TreeDocumentNR_DOCUM.ItemIndex],
                    ANode.Texts[TreeDocumentDATA_DOCUM.ItemIndex],
                    ANode.Texts[TreeDocumentPREDATOR.ItemIndex],
                    ANode.Texts[TreeDocumentPRIMITOR.ItemIndex]
                   ]);
end;

procedure TfrmGEST_ModifyDocum.cxTabControlChange(Sender: TObject);
begin
  SetSQL;
  RefreshScreen;
end;

procedure TfrmGEST_ModifyDocum.btnTiparireClick(Sender: TObject);
var
  lId : Integer;
  lDataSet: TDataSet;
begin
  lId := GetDocumentID();
  lDataSet := DBNewQueryFmt('exec [spGestRetiparire] %d', [lId]);
  try
    lDataSet.Open;
    while not lDataSet.Eof do begin
      DateUnit.IdGestDocum := lDataSet.Fields[0].AsInteger;
      PrintDocument2DBFR(True, True);
      lDataSet.Next;
    end;
  finally
    lDataSet.Free;
  end;
end;

end.
