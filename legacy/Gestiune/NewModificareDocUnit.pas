unit NewModificareDocUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, dxInspRw, dxDBInRw, dxInspct, dxDBInsp, ExtCtrls,
  dxExEdtr, dxEdLib, dxDBELib, dxDBTLCl, dxGrClms, dxDBCtrl,
  dxDBGrid, dxTL, dxCntner, dxfCheckBox, Db, 
  ImgList, dxEditor, ComCtrls, Buttons, ZDataSet, dxDBTL, Menus,
  cxControls, cxContainer, cxEdit,
  cxCheckBox, cxDBEdit, cxTextEdit, cxMemo, cxGraphics, cxMaskEdit,
  cxDropDownEdit, cxImageComboBox, cxLookAndFeelPainters, cxButtons,
  ZAbstractRODataset, ZAbstractDataset,
  cxLookAndFeels, cxStyles, cxSpinEdit, cxButtonEdit, cxVGrid, cxDBVGrid,
  cxInplaceContainer, cxLabel, dxBarBuiltInMenu, cxPC, cxGroupBox, cxSplitter,
  cxCustomData, cxFilter, cxData, cxDataStorage, cxNavigator, cxDBData,
  cxGridLevel, cxClasses, cxGridCustomView, cxGridCustomTableView,
  cxGridTableView, cxGridDBTableView, cxGrid, dxDateRanges,
  cxDataControllerConditionalFormattingRulesManagerDialog,
  dxScrollbarAnnotations;

type
  TfrmModificDocument = class(TForm)
    chkStockPrimitor: TcxCheckBox;
    Label1: TcxLabel;
    Label2: TcxLabel;
    Bevel1: TBevel;
    Label10: TcxLabel;
    Imagini: TImageList;
    Enabled: TImageList;
    DTTemplateCrid: TDataSource;
    Color: TColorDialog;
    tabDefaDoc: TcxTabControl;
    PageDescDocum: TcxPageControl;
    tbDescDocum: TcxTabSheet;
    pnDescClient: TPanel;
    Splitter3: TcxSplitter;
    grDetaliiDoc: TcxGroupBox;
    BtnModifyStockPredator: TcxButton;
    BtnModifyStockPrimitor: TcxButton;
    chkStockPredator: TcxCheckBox;
    edTipStockPred: TcxDBImageComboBox;
    edTipStockPrim: TcxDBImageComboBox;
    tabTratareCODMAT: TcxTabSheet;
    Label5: TcxLabel;
    edDocumentConex: TcxDBImageComboBox;
    Label6: TcxLabel;
    edTipDescarcare: TcxDBImageComboBox;
    edChkNumarAuto: TcxDBCheckBox;
    edPrefix: TcxDBMaskEdit;
    edNumarStart: TcxDBSpinEdit;
    edNumarEnd: TcxDBSpinEdit;
    LbReportInfo: TcxLabel;
    edTiparireAutomata: TcxCheckBox;
    LbZileGratie: TcxLabel;
    edZileValabiltiate: TcxDBSpinEdit;
    AtsDBCheckEdit1: TcxDBCheckBox;
    Label8: TcxLabel;
    Label9: TcxLabel;
    edModDescarcare: TcxDBImageComboBox;
    QryValidari: TZQuery;
    DTValidari: TDataSource;
    edListaFunctii: TcxPopupEdit;
    edZileGratie: TcxSpinEdit;
    edPrioritate: TcxSpinEdit;
    edTipValidare: TcxImageComboBox;
    BtnModifica: TcxButton;
    BtnDelete: TcxButton;
    BtnAdauga: TcxButton;
    edRaportGenerat: TcxPopupEdit;
    TreeFunctiuni: TdxDBTreeList;
    TreeFunctiuniCOD_FUNCTIE: TdxDBTreeListMaskColumn;
    TreeFunctiuniDENUMIRE: TdxDBTreeListMaskColumn;
    TreeFunctiuniDESCRIERE: TdxDBTreeListMaskColumn;
    TreeReportList: TdxDBTreeList;
    TreeReportListCAPTURA: TdxDBTreeListMaskColumn;
    TreeReportListIS_REPORT: TdxDBTreeListCheckColumn;
    tabContabilitate: TcxTabSheet;
    Label7: TcxLabel;
    Label11: TcxLabel;
    Label12: TcxLabel;
    Label13: TcxLabel;
    edContDebitor: TcxPopupEdit;
    edContCreditor: TcxPopupEdit;
    edFormulaNota: TcxButtonEdit;
    edClasificatieEcNota: TcxButtonEdit;
    BtnAddNota: TcxButton;
    BtnModifyNota: TcxButton;
    BtnDeleteNota: TcxButton;
    Label3: TcxLabel;
    edTipuriMaterial: TcxImageComboBox;
    BtnAddMaterial: TcxButton;
    BtnModifyMaterial: TcxButton;
    BtnDeleteMaterial: TcxButton;
    DTTipuriMateriale: TDataSource;
    QryTipuriMateriale: TZQuery;
    DTModContare: TDataSource;
    QryModContare: TZQuery;
    BtnModifyReport: TcxButton;
    Label14: TcxLabel;
    edTipListaCampuri: TcxImageComboBox;
    QryFieldItemsi: TZQuery;
    QryFieldDocum: TZQuery;
    BtnAdaugaColoana: TcxButton;
    BtnCopy: TcxButton;
    ppCopiereMenu: TPopupMenu;
    qryTipProduse: TZQuery;
    CmdCampuriLipsa: TMenuItem;
    edtEsteActiv: TcxDBCheckBox;
    edCod: TcxDBTextEdit;
    edNume: TcxDBTextEdit;
    edDescriere: TcxDBMemo;
    edSuportaFiliala: TcxDBCheckBox;
    ChkComplementare: TcxDBCheckBox;
    edPredator: TcxDBImageComboBox;
    edPrimitor: TcxDBImageComboBox;
    edtIsPrinting: TcxDBCheckBox;
    imgDMEdit: TcxDBImageComboBox;
    DTReportList: TDataSource;
    QryReportList: TZQuery;
    TreePlan: TdxDBTreeList;
    TreePlanCONT: TdxDBTreeListMaskColumn;
    TreePlanROMANA: TdxDBTreeListMaskColumn;
    TreePlanFctCont: TdxDBTreeListColumn;
    btnModifyPosition: TcxButton;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    inspTemplate: TcxDBVerticalGrid;
    inspTemplateCAPTION: TcxDBEditorRow;
    inspTemplateVISIBLE: TcxDBEditorRow;
    inspTemplateFONT_NAME: TcxDBEditorRow;
    inspTemplateEDIT_MASK: TcxDBEditorRow;
    inspTemplateCLASS_NAME: TcxDBEditorRow;
    inspTemplateFIELD_NAME: TcxDBEditorRow;
    inspTemplateMIN_WIDTH: TcxDBEditorRow;
    inspTemplateMAX_WIDTH: TcxDBEditorRow;
    inspTemplateCAPTION_ALIGN: TcxDBEditorRow;
    inspTemplateALIGN: TcxDBEditorRow;
    inspTemplatePOS: TcxDBEditorRow;
    inspTemplateCOLOR: TcxDBEditorRow;
    inspTemplateFONT_COLOR: TcxDBEditorRow;
    inspTemplateFONT_SIZE: TcxDBEditorRow;
    inspTemplateREQUIRED: TcxDBEditorRow;
    inspTemplateFORMULA_CALCUL: TcxDBEditorRow;
    inspTemplateREADONLY: TcxDBEditorRow;
    inspTemplateSUM_TOTAL: TcxDBEditorRow;
    inspTemplatePRECEDENTA: TcxDBEditorRow;
    inspTemplateautoCreate: TcxDBEditorRow;
    gridTemplate: TcxGrid;
    viewTemplate: TcxGridDBTableView;
    nivelTemplate: TcxGridLevel;
    viewTemplateCLASS_NAME: TcxGridDBColumn;
    viewTemplatePOS: TcxGridDBColumn;
    viewTemplateFIELD_NAME: TcxGridDBColumn;
    viewTemplateCAPTION: TcxGridDBColumn;
    viewTemplateMIN_WIDTH: TcxGridDBColumn;
    viewTemplateMAX_WIDTH: TcxGridDBColumn;
    viewTemplateCAPTION_ALIGN: TcxGridDBColumn;
    viewTemplateALIGN: TcxGridDBColumn;
    viewTemplateCOLOR: TcxGridDBColumn;
    viewTemplateFONT_NAME: TcxGridDBColumn;
    viewTemplateFONT_COLOR: TcxGridDBColumn;
    viewTemplateEDIT_MASK: TcxGridDBColumn;
    viewTemplateFONT_SIZE: TcxGridDBColumn;
    viewTemplateVISIBLE: TcxGridDBColumn;
    viewTemplateFORMULA_CALCUL: TcxGridDBColumn;
    viewValidari: TcxGridDBTableView;
    nivelValidari: TcxGridLevel;
    gridValidari: TcxGrid;
    viewValidariID_FUNCTIUNE: TcxGridDBColumn;
    viewValidariZILE_GRATIE: TcxGridDBColumn;
    viewValidariPRIORITATE: TcxGridDBColumn;
    viewValidariTIP_VALIDARE: TcxGridDBColumn;
    gridTipuriMaterial: TcxGrid;
    viewTipMaterial: TcxGridDBTableView;
    viewTipMaterialID_GEST_TIP_MATERIAL: TcxGridDBColumn;
    viewTipMaterialGENEREAZA_CODMAT: TcxGridDBColumn;
    viewTipMaterialACCEPT_STOCK_NEGATIV: TcxGridDBColumn;
    nivelTipMaterial: TcxGridLevel;
    gridModContare: TcxGrid;
    viewModContare: TcxGridDBTableView;
    viewModContareID_GEST_TIP_MATERIAL: TcxGridDBColumn;
    viewModContareCONT_DEBITOR: TcxGridDBColumn;
    viewModContareCONT_CREDITOR: TcxGridDBColumn;
    viewModContareCOD_ECONOMIC: TcxGridDBColumn;
    viewModContareCOD_FUNCTIONAL: TcxGridDBColumn;
    nivelModContare: TcxGridLevel;
    procedure FormCreate(Sender: TObject);
    procedure FormCloseQuery(Sender: TObject; var CanClose: Boolean);
    procedure InspectorColoaneCOLORButtonClick(Sender: TObject;
      AbsoluteIndex: Integer);
    procedure InspectorColoaneFONT_COLORDrawValue(Sender: TdxInspectorRow;
      ACanvas: TCanvas; ARect: TRect; var AText: String; AFont: TFont;
      var AColor: TColor; var ADone: Boolean);
    procedure tabDefaDocChanging(Sender: TObject;
      var AllowChange: Boolean);
    procedure tabDefaDocChange(Sender: TObject);
    procedure tabDefaDocGetImageIndex(Sender: TObject; TabIndex: Integer;
      var ImageIndex: Integer);
    procedure BtnModifyStockPredatorClick(Sender: TObject);
    procedure BtnModifyStockPrimitorClick(Sender: TObject);
    procedure ChkComplementeazaPredatorClick(Sender: TObject);
    procedure BtnAdaugaClick(Sender: TObject);
    procedure BtnModificaClick(Sender: TObject);
    procedure BtnDeleteClick(Sender: TObject);
    procedure TreeFunctiuniDblClick(Sender: TObject);
    procedure TreeFunctiuniKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure edTiparireAutomataClick(Sender: TObject);
    procedure edZileValabiltiateChange(Sender: TObject);
    procedure TreePlanROMANAGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure QryTipuriMaterialeNewRecord(DataSet: TDataSet);
    procedure edTipuriMaterialChange(Sender: TObject);
    procedure BtnAddMaterialClick(Sender: TObject);
    procedure TreePlanDblClick(Sender: TObject);
    procedure TreePlanKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure BtnModifyMaterialClick(Sender: TObject);
    procedure BtnAddNotaClick(Sender: TObject);
    procedure BtnModifyNotaClick(Sender: TObject);
    procedure BtnDeleteMaterialClick(Sender: TObject);
    procedure BtnDeleteNotaClick(Sender: TObject);
    procedure edTipListaCampuriChange(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure BtnAdaugaColoanaClick(Sender: TObject);
    procedure BtnCopyClick(Sender: TObject);
    procedure edClasificatieEcNotaButtonClick(Sender: TObject;
      AbsoluteIndex: Integer);
    procedure QryFieldItemsiNewRecord(DataSet: TDataSet);
    procedure CmdCampuriLipsaClick(Sender: TObject);
    procedure edPredatorPropertiesChange(Sender: TObject);
    procedure btnModifyPositionClick(Sender: TObject);
    procedure edRaportGeneratPropertiesCloseUp(Sender: TObject);
    procedure edListaFunctiiPropertiesCloseUp(Sender: TObject);
    procedure edContDebitorPropertiesCloseUp(Sender: TObject);
    procedure edContDebitorPropertiesPopup(Sender: TObject);
    procedure edContCreditorPropertiesCloseUp(Sender: TObject);
    procedure edContCreditorPropertiesPopup(Sender: TObject);
    procedure viewTipMaterialFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure viewModContareFocusedRecordChanged(Sender: TcxCustomGridTableView;
      APrevFocusedRecord, AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure edContDebitorKeyPress(Sender: TObject; var Key: Char);
  private
    { Private declarations }
    FFieldImplicit: Boolean;
    FItemFieldList,
    FDocumFieldList: TStringList;
    FContDebitor,
    FContCreditor : String;
    FIdDefaDocum  : Integer;
    OldTabIndex   : Integer;
    FDocEnabled   : array[0..3] of Boolean;
    FTemplModificat : Boolean;
    function QryDocs: TDataSet;
    function QryDefaDocs: TDataSet;
    procedure SetCurentRecord(PredIn, PrimIn: Boolean);
    procedure SetDefaDocExists;
    function GetModified: Boolean;
    function GetIdTipDocument: Integer;
    procedure SaveGridTemplate;
    procedure EditTipStock(AField: String; const AValue: Variant);
    procedure RefreshTipStock;

    procedure UpdateEnableNota;

  protected
    procedure PostValidare(const AAppend: Boolean);
    procedure PostContare(const AAppend: Boolean);
    procedure LoadPreluareMeniu;
    procedure DoPreluareDocum(Sender: TObject);
    procedure RefreshDefalcari;
  public
    { Public declarations }
    procedure LoadRemainings;
    procedure SaveRemainings;
    property  Modified : Boolean read GetModified;
    property  IdTipDocument : Integer read GetIdTipDocument;
  end;

implementation

{$R *.DFM}

uses
  dxCompsUtile, ZeosDBUtile, DateUnit, Variants,
  SelBugetUnit,
  StockUnit, AdaugareColoanaUnit, 
  CommonDBVar, InflProduseFields;

procedure TfrmModificDocument.FormCreate(Sender: TObject);

  function GetFieldList(AProcName, AObjectName: String): TStringList;
  var
    lDataSet  : TDataSet;
    lSQLOrder :String;
  begin
    Result := TStringList.Create;
    Result.Sorted := True;
    if DBProcExists(AProcName) then
      lSQLOrder := 'exec [' + AProcName+ ']'
    else
      lSQLOrder := 'SELECT NAME FROM SYSCOLUMNS WHERE ID = OBJECT_ID('''+AObjectName+''') ORDER BY COLID';
    lDataSet := DBNewQuery(lSQLOrder);
    try
      lDataSet.Open;
      while not lDataSet.Eof do begin
        Result.Add(lDataSet.Fields[0].AsString);
        lDataSet.Next;
      end;
    finally
      lDataSet.Free;
    end;
  end;

begin
  PageDescDocum.ActivePageIndex := 0;
  { Incarcam si meniurile pentru preluare detaliere document }
  LoadPreluareMeniu;
  DBRefresh([QryReportList]);
  FItemFieldList  := GetFieldList('SP_GEST_GET_CAMPURI_GEST_ITEMSI', 'GEST_GNMCL');
  FDocumFieldList := GetFieldList('SP_GEST_GET_CAMPURI_GEST_DOCUM' , 'GEST_DOCUM');
  if frmData.QryDefaDoc.FindField('TethysTipDoc') <> nil then begin
    imgDMEdit.Visible := True;
    imgDMEdit.DataBinding.DataField := 'TethysTipDoc';
    FillImageCombo(imgDMEdit.Properties, 'exec [spTethysTipDocumente]', 'tipDocumentID', 'tipDocument', Null, 'Neasignat');
  end;

  TcxComboBoxProperties(viewTemplateFIELD_NAME.Properties).Items.Assign(FItemFieldList);
  TcxComboBoxProperties(inspTemplateFIELD_NAME.Properties.EditProperties).Items.Assign(FItemFieldList);

  FTemplModificat := False;

  FillImageCombo(viewValidariID_FUNCTIUNE.Properties, frmData.QryFunctiuni, 'ID_FUNCTIUNI', 'DENUMIRE');
  FillImageCombo(edDocumentConex.Properties, 'select * from gest_tip_docum', 'ID_GEST_TIP_DOCUM', 'COD_DOCUM', True, 'Fara Document');
  FillImageCombo(edTipuriMaterial.Properties, 'spNmclTipMaterial', 0, 1, True, 'Pe document');

  viewTipMaterialID_GEST_TIP_MATERIAL.Properties.Assign(edTipuriMaterial.Properties);
  viewModContareID_GEST_TIP_MATERIAL.Properties.Assign(edTipuriMaterial.Properties);

  RefreshTipStock;
  RefreshDefalcari;
  tabDefaDocChange(tabDefaDoc);
   viewTemplateVISIBLE.PropertiesClassName := 'TcxCheckBoxProperties';
  with TcxCheckBoxProperties(viewTemplateVISIBLE.Properties) do
  begin
    ValueChecked := True;
    ValueUnchecked := False;

  end;

  inspTemplateVISIBLE.Properties.EditPropertiesClass := TcxCheckBoxProperties;
  with TcxCheckBoxProperties(inspTemplateVISIBLE.Properties.EditProperties) do
  begin
    ValueChecked := True;
    ValueUnchecked := False;
  end;
end;

procedure TfrmModificDocument.FormCloseQuery(Sender: TObject;
  var CanClose: Boolean);
begin
  CanClose := (ModalResult = mrOk) or (not Modified);
  if not CanClose then
     CanClose := MessageDlg('Doriti abandonarea modificarilor facute?',
                            mtConfirmation, [mbYes, mbNo], 0) = mrYes;
end;

procedure TfrmModificDocument.LoadRemainings;
begin
  OldTabIndex     := -1;
  FFieldImplicit  := False;
  edPredatorPropertiesChange(edPredator);
  edPredatorPropertiesChange(edPrimitor);
  FTemplModificat := FFieldImplicit;
end;

procedure TfrmModificDocument.PostContare(const AAppend: Boolean);
begin
  QryModContare.DisableControls;
  try
    if AAppend then QryModContare.Append else QryModContare.Edit;
    QryModContare['ID_GEST_TIP_MATERIAL'] := edTipuriMaterial.EditValue;
    QryModContare['CONT_DEBITOR']         := FContDebitor;
    QryModContare['CONT_CREDITOR']        := FContCreditor;
    QryModContare['FORMULA']              := edFormulaNota.EditValue;
    QryModContare['COD_ECONOMIC']         := edClasificatieEcNota.EditValue;
    QryModContare.Post;
  finally
    QryModContare.EnableControls;
  end;
  UpdateEnableNota;
end;

procedure TfrmModificDocument.PostValidare(const AAppend: Boolean);
begin
  QryValidari.DisableControls;
  try
    if AAppend then QryValidari.Append else QryValidari.Edit;
    QryValidari['ID_GEST_DEFA_DOCUM'] := FrmData.QryDefaDoc['ID_GEST_DEFA_DOCUM'];
    QryValidari['ZILE_GRATIE']        := edZileGratie.EditValue;
    QryValidari['PRIORITATE']         := edPrioritate.EditValue;
    QryValidari['TIP_VALIDARE']       := edTipValidare.EditValue;
    QryValidari['ID_FUNCTIUNE']       := edListaFunctii.Tag;
    QryValidari.Post;
  finally
    QryValidari.EnableControls;
  end;
end;

procedure TfrmModificDocument.SaveRemainings;
begin
 SaveGridTemplate;
end;

function TfrmModificDocument.QryDocs: TDataSet;
begin
  Result := FrmData.QryDocumente;
end;

procedure TfrmModificDocument.InspectorColoaneCOLORButtonClick(
  Sender: TObject; AbsoluteIndex: Integer);
begin
  if Color.Execute then
     with TdxInspectorDBButtonRow(Sender) do begin
       if not (Field.DataSet.State in [dsEdit, dsInsert]) then
          Field.DataSet.Edit;
       Field.AsInteger := Integer(Color.Color);
     end;
end;

procedure TfrmModificDocument.InspectorColoaneFONT_COLORDrawValue(
  Sender: TdxInspectorRow; ACanvas: TCanvas; ARect: TRect;
  var AText: String; AFont: TFont; var AColor: TColor; var ADone: Boolean);
begin
  if Trim(AText) > '' then AColor := TColor(StrToInt(AText));
  AText := '';
end;

procedure TfrmModificDocument.tabDefaDocChanging(Sender: TObject;
  var AllowChange: Boolean);
begin
  OldTabIndex := tabDefaDoc.TabIndex;
end;

procedure TfrmModificDocument.tabDefaDocChange(Sender: TObject);
begin
  if not FDocEnabled[tabDefaDoc.TabIndex] then begin
     if OldTabIndex > -1 then tabDefaDoc.TabIndex := OldTabIndex
  end
  else SetCurentRecord(not Boolean(tabDefaDoc.TabIndex div 2), not Boolean(tabDefaDoc.TabIndex mod 2));
end;

procedure TfrmModificDocument.SetCurentRecord(PredIn, PrimIn: Boolean);
var
  lNode     : TdxDBTreeListNode;
begin
  if OldTabIndex > -1 then SaveGridTemplate;
  with QryDefaDocs do
    if Locate('ID_GEST_TIP_DOCUM;PREDATOR_INTERN;PRIMITOR_INTERN',
                  VarArrayOf([IdTipDocument, PredIn, PrimIn]), []) then begin
       FIdDefaDocum := FieldByName('ID_GEST_DEFA_DOCUM').AsInteger;
       QryFieldItemsi.Close;
       QryFieldDocum.Close;
       edTipListaCampuriChange(edTipListaCampuri);
       { Intializam template-ul de culegere }
       edRaportGenerat.Tag        := FieldByName('ID_REPORT').AsInteger;
       edTiparireAutomata.Checked := edRaportGenerat.Tag <> -1;
       if not edTiparireAutomata.Checked then
          edRaportGenerat.EditText := 'Nu se genereaza nici un raport'
       else begin
          lNode := TreeReportList.FindNodeByKeyValue(-1 * edRaportGenerat.Tag);
          if Assigned(lNode) then edRaportGenerat.Text := lNode.Strings[TreeReportListCAPTURA.Index];
       end;
       QryValidari.Params[0].Value        := FIdDefaDocum;
       QryModContare.Params[0].Value      := FIdDefaDocum;
       QryTipuriMateriale.Params[0].Value := FIdDefaDocum;
       qryTipProduse.Params[0].Value      := FIdDefaDocum;
       DBRefresh([QryValidari, QryModContare, QryTipuriMateriale, qryTipProduse]);
    end
    else Raise EContaHandledError.Create('EROARE INTERNA : - nu se poate selecta defalcarea tipului de document !');
end;

procedure TfrmModificDocument.tabDefaDocGetImageIndex(Sender: TObject;
  TabIndex: Integer; var ImageIndex: Integer);
begin
  ImageIndex := Integer(FDocEnabled[TabIndex]);
end;

function TfrmModificDocument.GetModified: Boolean;
begin
  Result := (QryDocs.Modified) or (FTemplModificat) or (QryValidari.Modified) or (qryTipProduse.Modified);
end;

procedure TfrmModificDocument.SetDefaDocExists;

  procedure SetState(Predator: Boolean; Primitor: Boolean; Stare: Boolean);
  begin
     with QryDefaDocs do
       if Locate('ID_GEST_TIP_DOCUM;PREDATOR_INTERN;PRIMITOR_INTERN',
                 VarArrayOf([IdTipDocument, Predator, Primitor]), []) then begin
          if not Stare then Delete;
       end
       else if Stare then begin
          Append;
          FieldByName('ID_GEST_TIP_DOCUM').AsInteger := IdTipDocument;
          FieldByName('PREDATOR_INTERN').AsBoolean   := Predator;
          FieldByName('PRIMITOR_INTERN').AsBoolean   := Primitor;
          Post;
       end;
  end;

begin
  { Testam Toate cele patru cazuri }
  with QryDefaDocs do begin
    DisableControls;
    try
      if OldTabIndex > -1 then SaveGridTemplate;
      SetState(True, True, FDocEnabled[0]);
      SetState(True, False, FDocEnabled[1]);
      SetState(False, True, FDocEnabled[2]);
      SetState(False, False, FDocEnabled[3]);
    finally
      EnableControls;
    end;
  end;
end;

function TfrmModificDocument.QryDefaDocs: TDataSet;
begin
  Result := FrmData.QryDefaDoc;
end;

function TfrmModificDocument.GetIdTipDocument: Integer;
begin
  Result := QryDocs.FieldByName('ID_GEST_TIP_DOCUM').AsInteger;
end;

procedure TfrmModificDocument.BtnModifyStockPredatorClick(Sender: TObject);
begin
  EditTipStock(edTipStockPred.DataBinding.DataField, edTipStockPred.EditValue);
end;

procedure TfrmModificDocument.EditTipStock(AField: String; const AValue: Variant);
var
  lfrmStock : TfrmStock;
begin
  if ValueHasValue(AValue) then begin
     if not FrmData.QryTipStock.Locate('ID_GEST_TIP_STOC', AValue, []) then
        raise EContaHandledError.Create('EROARE interna : - nu se poate localiza tipul de stock !');
     FrmData.QryTipStock.Edit;
  end
  else begin
    FrmData.QryTipStock.Append;
    FrmData.QryTipStock.FieldByName('DENUMIRE').AsString := 'Stoc nou';
  end;
  lfrmStock := TfrmStock.Create(Self);
  try
    lfrmStock.edDocument.Properties.Assign(edDocumentConex.Properties);
    lfrmStock.viewDefaDocID_GEST_TIP_DOCUM.Properties.Assign(edDocumentConex.Properties);
    if lfrmStock.ShowModal = mrOk then begin
      frmData.QryTipStock.Post;
      DBSetFieldValue(frmData.QryDefaDoc, AField, frmData.QryTipStock['ID_GEST_TIP_STOC']);
    end
    else
      frmData.QryTipStock.Cancel;
  finally
    lfrmStock.Free;
  end;
end;

procedure TfrmModificDocument.BtnModifyStockPrimitorClick(Sender: TObject);
begin
  EditTipStock(edTipStockPrim.DataBinding.DataField, Trim(edTipStockPrim.Text));
end;

procedure TfrmModificDocument.ChkComplementeazaPredatorClick(
  Sender: TObject);
begin
  edPredatorPropertiesChange(edPredator);
end;

procedure TfrmModificDocument.RefreshTipStock;
begin
  FillImageCombo(edTipStockPred.Properties, frmData.QryTipStock, 'ID_GEST_TIP_STOC', 'DENUMIRE', True, 'Fara stoc');
  edTipStockPrim.Properties.Assign(edTipStockPred.Properties);
end;

procedure TfrmModificDocument.SaveGridTemplate;
begin
  DBPost([QryFieldItemsi, QryFieldDocum]);
  if not edTiparireAutomata.Checked then
    DBSetFieldValue(frmData.QryDefaDoc, 'ID_REPORT', Null)
  else
    DBSetFieldValue(frmData.QryDefaDoc, 'ID_REPORT', edRaportGenerat.Tag);
  FTemplModificat := False;
end;

procedure TfrmModificDocument.BtnAdaugaClick(Sender: TObject);
begin
  PostValidare(True);
end;

procedure TfrmModificDocument.BtnModificaClick(Sender: TObject);
begin
  PostValidare(False);
end;

procedure TfrmModificDocument.BtnDeleteClick(Sender: TObject);
begin
  if (QryValidari.IsEmpty) or
     (MessageDlg('Doriti stergerea validarii pentru documentul curent?', mtConfirmation, [mbYes, mbNo], 0) <> mrYes) then Exit;
  QryValidari.Delete;
end;

procedure TfrmModificDocument.TreeFunctiuniDblClick(Sender: TObject);
begin
  if TdxDBTreeList(Sender).FocusedNode <> nil then
    GetParentForm(TControl(Sender)).ModalResult := mrOk;
end;

procedure TfrmModificDocument.TreeFunctiuniKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if Key = VK_ESCAPE then
    GetParentForm(TControl(Sender)).ModalResult := mrCancel
  else
    TreeFunctiuniDblClick(Sender);
end;

procedure TfrmModificDocument.edTiparireAutomataClick(Sender: TObject);
begin
  FTemplModificat            := True;
  LbReportInfo.Enabled       := edTiparireAutomata.Checked;
  edRaportGenerat.Enabled    := LbReportInfo.Enabled;
  LbZileGratie.Enabled       := LbReportInfo.Enabled;
  edZileValabiltiate.Enabled := LbReportInfo.Enabled;
  GridValidari.Enabled       := LbReportInfo.Enabled;
  edListaFunctii.Enabled     := LbReportInfo.Enabled;
  edZileGratie.Enabled       := LbReportInfo.Enabled;
  edPrioritate.Enabled       := LbReportInfo.Enabled;
  edTipValidare.Enabled      := LbReportInfo.Enabled;
  BtnAdauga.Enabled          := LbReportInfo.Enabled;
  BtnModifica.Enabled        := LbReportInfo.Enabled;
  BtnDelete.Enabled          := LbReportInfo.Enabled;
end;

procedure TfrmModificDocument.edZileValabiltiateChange(Sender: TObject);
begin
  FTemplModificat := True;
end;

procedure TfrmModificDocument.TreePlanROMANAGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
begin
  AText := Trim(ANode.Strings[TreePlanCONT.Index])+' : '+Trim(AText);
end;

procedure TfrmModificDocument.QryTipuriMaterialeNewRecord(
  DataSet: TDataSet);
begin
  DataSet['ID_GEST_DEFA_DOCUM'] := FIdDefaDocum;
end;

procedure TfrmModificDocument.edTipuriMaterialChange(Sender: TObject);
begin
  UpdateEnableNota;
end;

procedure TfrmModificDocument.BtnAddMaterialClick(Sender: TObject);
begin
  QryTipuriMateriale.Append;
  QryTipuriMateriale['GENEREAZA_CODMAT']      := False;
  QryTipuriMateriale['ID_GEST_TIP_MATERIAL']  := edTipuriMaterial.EditValue;
  DBPost(QryTipuriMateriale);
  edTipuriMaterialChange(edTipuriMaterial);
end;

procedure TfrmModificDocument.TreePlanDblClick(Sender: TObject);
begin
  { Inchidem Cu Accept }
  with TdxDBTreeList(Sender) do
    if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
      GetParentForm(TdxDBTreeList(Sender)).ModalResult := mrOk;
end;

procedure TfrmModificDocument.TreePlanKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if Key = VK_ESCAPE then
    GetParentForm(TdxDBTreeList(Sender)).ModalResult := mrCancel
  else
  if Key = VK_RETURN then  
    TreePlanDblClick(Sender);
end;

procedure TfrmModificDocument.UpdateEnableNota;
begin
  BtnAddNota.Enabled       := (Trim(FContDebitor) > '')
                              and (Trim(FContCreditor)>'');
  BtnModifyNota.Enabled    := BtnAddNota.Enabled;
end;

procedure TfrmModificDocument.viewModContareFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord; ANewItemRecordFocusingChanged: Boolean);
begin
  btnDeleteNota.Enabled := Assigned(AFocusedRecord) and AFocusedRecord.IsData;
  if btnDeleteNota.Enabled then begin
    FContDebitor                    := AFocusedRecord.Values[viewModContareCONT_DEBITOR.Index];
    FContCreditor                   := AFocusedRecord.Values[viewModContareCONT_CREDITOR.Index];
    edContDebitor.EditValue         := FContDebitor;
    edContCreditor.EditValue        := FContCreditor;
    edTipuriMaterial.EditValue      := AFocusedRecord.Values[viewModContareID_GEST_TIP_MATERIAL.Index];
    edClasificatieEcNota.EditValue  := AFocusedRecord.Values[viewModContareCOD_ECONOMIC.Index];
  end;
end;

procedure TfrmModificDocument.viewTipMaterialFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord; ANewItemRecordFocusingChanged: Boolean);
begin
  BtnDeleteMaterial.Enabled := Assigned(AFocusedRecord) and AFocusedRecord.IsData;
  if BtnDeleteMaterial.Enabled then
    edTipuriMaterial.EditValue := AFocusedRecord.Values[viewTipMaterialID_GEST_TIP_MATERIAL.Index];
end;

procedure TfrmModificDocument.BtnModifyMaterialClick(Sender: TObject);
begin
  DBSetFieldValue(QryTipuriMateriale, 'ID_GEST_TIP_MATERIAL', edTipuriMaterial.EditValue);
  edTipuriMaterialChange(edTipuriMaterial);
end;

procedure TfrmModificDocument.BtnAddNotaClick(Sender: TObject);
begin
  PostContare(True);
end;

procedure TfrmModificDocument.BtnModifyNotaClick(Sender: TObject);
begin
  PostContare(False);
end;

procedure TfrmModificDocument.BtnDeleteMaterialClick(Sender: TObject);
begin
  viewTipMaterial.DataController.DeleteFocused;
end;

procedure TfrmModificDocument.BtnDeleteNotaClick(Sender: TObject);
begin
  viewModContare.DataController.DeleteFocused;
end;

procedure TfrmModificDocument.edTipListaCampuriChange(Sender: TObject);

  procedure TestAndOpen(AQry: TZQuery);
  var
    lSqlOrder: String;
    lTblName : String;
  begin
    if not AQry.Active then begin
      AQry.Params[0].Value := FIdDefaDocum;
      AQry.Open;
      if (AQry.IsEmpty) and
         (MessageDlg('Nu aveti nici un camp definit. Doriti generarea campurilor implicite?', mtConfirmation, [mbYes, mbNo], 0) = mrYes) then begin
        if AQry.Tag = 0 then lTblName := 'GEST_DEFA_DOCUM_ITEMSI' else lTblName := 'GEST_DEFA_DOCUM_DOCUMENT';
        lSqlOrder := Format('INSERT INTO %s (ID_GEST_DEFA_DOCUM, FIELD_NAME, CAPTION, CAPTION_ALIGN, POS, READONLY, VISIBLE, REQUIRED) ', [lTblName]);
        lSqlOrder := lSqlOrder + 'SELECT '+IntToStr(FIdDefaDocum)+', NAME AS FIELD_NAME, NAME AS CAPTION, 2 AS CAPTION_ALIGN, COLID AS POS, CONVERT(BIT, 0) AS READONLY, CONVERT(BIT, 1) AS VISIBLE, CONVERT(BIT, 0) AS REQUIRED ';
        if AQry.Tag = 0 then lTblName := 'CULGEST_ITEMSI' else lTblName := 'CULGEST_DOCUM';
        lSqlOrder := lSqlOrder + Format('FROM SYSCOLUMNS WHERE ID = OBJECT_ID(''%s'') ORDER BY COLID', [lTblName]);
        DBExecSQL(lSqlOrder);
        DBRefresh(AQry);
      end;
    end;
    DTTemplateCrid.DataSet := AQry;
    if AQry.Tag = 0 then begin
      TcxComboBoxProperties(viewTemplateFIELD_NAME.Properties).Items.Assign(FItemFieldList);
      TcxComboBoxProperties(inspTemplateFIELD_NAME.Properties.EditProperties).Items.Assign(FItemFieldList);
      viewTemplate.DataController.KeyFieldNames := 'ID_GEST_DEFA_DOCUM_ITEMSI';
     end
     else begin
      TcxComboBoxProperties(viewTemplateFIELD_NAME.Properties).Items.Assign(FDocumFieldList);
      TcxComboBoxProperties(inspTemplateFIELD_NAME.Properties.EditProperties).Items.Assign(FDocumFieldList);
      viewTemplate.DataController.KeyFieldNames := 'ID_GEST_DEFA_DOCUM_DOCUMENT';
     end;
   end;

begin
  { Trecem forumulele de calcul pe document sau pe itemsi }
  if edTipListaCampuri.EditValue = 0 then
    TestAndOpen(QryFieldItemsi)
  else
    TestAndOpen(QryFieldDocum);
end;

procedure TfrmModificDocument.FormDestroy(Sender: TObject);
begin
  FItemFieldList.Free;
  FDocumFieldList.Free;
end;

procedure TfrmModificDocument.BtnAdaugaColoanaClick(Sender: TObject);
begin
  AdaugaColoanaNoua(QryFieldItemsi);
end;

procedure TfrmModificDocument.BtnCopyClick(Sender: TObject);
var
  lValues: array [1..5] of String;
begin
  lValues[1] := QryModContare.FieldByName('ID_GEST_TIP_MATERIAL').AsString;
  lValues[2] := QryModContare.FieldByName('CONT_DEBITOR').AsString;
  lValues[3] := QryModContare.FieldByName('CONT_CREDITOR').AsString;
  lValues[4] := QryModContare.FieldByName('FORMULA').AsString;
  lValues[5] := QryModContare.FieldByName('CONDITIE').AsString;

  QryModContare.Append;
  QryModContare.FieldByName('ID_GEST_TIP_MATERIAL').AsString := lValues[1];
  QryModContare.FieldByName('CONT_DEBITOR').AsString         := lValues[2];
  QryModContare.FieldByName('CONT_CREDITOR').AsString        := lValues[3];
  QryModContare.FieldByName('FORMULA').AsString              := lValues[4];
  QryModContare.FieldByName('CONDITIE').AsString             := lValues[5];
  edFormulaNota.Text := lValues[4];
  QryModContare.Post;
  UpdateEnableNota;
end;

procedure TfrmModificDocument.edClasificatieEcNotaButtonClick(Sender: TObject;
  AbsoluteIndex: Integer);
var
  aRes, aCodEco : String;
  idProiect : Integer;
begin
  aCodEco := edClasificatieEcNota.Text;
  aRes := NewSelectarePlanEconomic(aCodEco, idProiect, '', -1, True, Self);
  if aRes <> '<Anulat>' then begin
      edClasificatieEcNota.Text := aCodEco;
  end;
end;

procedure TfrmModificDocument.LoadPreluareMeniu;
var
  lRootItem,
  lTmpItem: TMenuItem;

    function GetMenuLevel(ACaption: String): TMenuItem;
    var I: Integer;
    begin
      Result := nil;
      for I := 0 to ppCopiereMenu.Items.Count-1 do begin
        Result := ppCopiereMenu.Items[I];
        if CompareText(Result.Caption, ACaption) = 0 then break
        else Result := nil;
      end;
      if Result = nil then begin
        Result := TMenuItem.Create(ppCopiereMenu.Items);
        Result.Caption := ACaption;
        ppCopiereMenu.Items.Add(Result);
      end;
    end;
var
  lDataSet: TDataSet;
begin
  lDataSet := DBNewQuery('exec [SP_GET_LST_DEFALCARI_DOCUMENTE]');
  try
    lDataSet.Open;
    while not lDataSet.Eof do begin
      lRootItem := GetMenuLevel(Trim(lDataSet.FieldByName('DEN_DOCUM').AsString));
      lTmpItem := TMenuItem.Create(lRootItem);
      lTmpItem.Caption := Trim(lDataSet.FieldByName('DETALII_DOCUM').AsString);
      lTmpItem.OnClick := DoPreluareDocum;
      lTmpItem.Tag     := lDataSet.FieldByName('ID_GEST_DEFA_DOCUM').AsInteger;
      lRootItem.Add(lTmpItem);
      lDataSet.Next;
    end;
  finally
    lDataSet.Free;
  end;
end;

procedure TfrmModificDocument.DoPreluareDocum(Sender: TObject);
begin
  DBExecSQLFmt('exec [SP_PRELUARE_DEFALCARE_DOCUMENT] %d, %d, %d, %d',
    [TMenuItem(Sender).Tag, IdTipDocument, 1 - tabDefaDoc.TabIndex div 2, 1 - tabDefaDoc.TabIndex mod 2]);
  RefreshDefalcari;
end;

procedure TfrmModificDocument.RefreshDefalcari;
begin
  DBRefresh(QryDefaDocs);
  LoadRemainings;
end;

procedure TfrmModificDocument.QryFieldItemsiNewRecord(DataSet: TDataSet);
begin
  DataSet['ID_GEST_DEFA_DOCUM'] := FIdDefaDocum;
end;

procedure TfrmModificDocument.CmdCampuriLipsaClick(Sender: TObject);
begin
  DBExecSQLFmt('exec [SP_GEST_DOCUM_CAMPURI_LIPSA] %d', [FIdDefaDocum]);
  RefreshDefalcari;
end;

procedure TfrmModificDocument.edListaFunctiiPropertiesCloseUp(Sender: TObject);
var
  lNode: TdxDBTreeListNode;
begin
  lNode := TdxDBTreeListNode(TreeFunctiuni.FocusedNode);
  if Assigned(lNode) then Text := lNode.Strings[TreeFunctiuniDENUMIRE.Index];
  edListaFunctii.Tag := lNode.Id;
end;

procedure TfrmModificDocument.edPredatorPropertiesChange(Sender: TObject);
var
  lPredVal,
  lPrimVal: Integer;
begin
  lPredVal  := ValueSafeToInt(edPredator.EditValue);
  lPrimVal  := ValueSafeToInt(edPrimitor.EditValue);
  FDocEnabled[0] := (lPredVal and 01 = 01) and (lPrimVal and 01 = 01) and (not ChkComplementare.Checked);
  FDocEnabled[1] := (lPredVal and 01 = 01) and (lPrimVal and 02 = 02);
  FDocEnabled[2] := (lPredVal and 02 = 02) and (lPrimVal and 01 = 01);
  FDocEnabled[3] := (lPredVal and 02 = 02) and (lPrimVal and 02 = 02) and (not ChkComplementare.Checked);
  SetDefaDocExists;
end;


procedure TfrmModificDocument.edRaportGeneratPropertiesCloseUp(Sender: TObject);
var
  lNode: TdxDBTreeListNode;
begin
  lNode := TdxDBTreeListNode(TreeReportList.FocusedNode);
  if Assigned(lNode) then begin
    edRaportGenerat.EditValue := lNode.Strings[TreeReportListCAPTURA.Index];
    edRaportGenerat.Tag := -1*lNode.Id;
    FTemplModificat := True;
  end;
end;

procedure TfrmModificDocument.btnModifyPositionClick(Sender: TObject);
begin
  if not (edTipListaCampuri.Text = '0') then Exit;
  InfluentareTipProduseField(FIdDefaDocum, QryFieldItemsi.FieldByName('FIELD_NAME').AsString);
end;

procedure TfrmModificDocument.edContCreditorPropertiesCloseUp(Sender: TObject);
var
  lNode: TdxDBTreeListNode;
begin
  lNode := TdxDBTreeListNode(TreePlan.FocusedNode);
  if Assigned(lNode) then begin
    FContCreditor             := lNode.Id;
    edContCreditor.EditValue  := lNode.Strings[1];
  end;
  UpdateEnableNota;
end;

procedure TfrmModificDocument.edContCreditorPropertiesPopup(Sender: TObject);
var
  lNode: TdxDBTreeListNode;
begin
  TreePlan.EndSearch;
  lNode := TreePlan.FindNodeByKeyValue(FContCreditor);
  if Assigned(lNode) then begin
    lNode.Focused := True;
    lNode.MakeVisible;
  end;
end;

procedure TfrmModificDocument.edContDebitorKeyPress(Sender: TObject;
  var Key: Char);
begin
  if Key = '%' then begin
    TcxPopupEdit(Sender).EditValue := '% : La Urmatoarele';
    if Sender = edContDebitor then FContDebitor := '%'
    else FContCreditor := '%';
    UpdateEnableNota;
  end;
end;

procedure TfrmModificDocument.edContDebitorPropertiesCloseUp(Sender: TObject);
var
  lNode: TdxDBTreeListNode;
begin
  lNode := TdxDBTreeListNode(TreePlan.FocusedNode);
  if Assigned(lNode) then begin
    FContDebitor            := lNode.Id;
    edContDebitor.EditValue := lNode.Strings[1];
  end;
  UpdateEnableNota;
end;

procedure TfrmModificDocument.edContDebitorPropertiesPopup(Sender: TObject);
var
  lNode: TdxDBTreeListNode;
begin
  TreePlan.EndSearch;
  lNode := TreePlan.FindNodeByKeyValue(FContDebitor);
  if Assigned(lNode) then begin
    lNode.Focused := True;
    lNode.MakeVisible;
  end;
end;

end.
