unit ModificareDocUnit;

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
  cxLookAndFeels;

type
  TfrmModificDocument = class(TForm)
    AtsfCheckBox1: TdxfCheckBox;  
    Label1: TLabel;
    Label2: TLabel;
    Bevel1: TBevel;
    Label10: TLabel;
    Imagini: TImageList;
    Enabled: TImageList;
    DTTemplateCrid: TDataSource;
    Color: TColorDialog;
    tabDefaDoc: TTabControl;
    PageDescDocum: TPageControl;
    tbDescDocum: TTabSheet;
    pnDescDoc: TPanel;
    Splitter3: TSplitter;
    Panel3: TPanel;
    BtnModifyStockPredator: TSpeedButton;
    BtnModifyStockPrimitor: TSpeedButton;
    chkStockPredator: TdxfCheckBox;
    edTipStockPred: TdxDBImageEdit;
    Panel4: TPanel;
    GridTemplate: TdxDBGrid;
    GridTemplateCLASS_NAME: TdxDBGridPickColumn;
    GridTemplatePOS: TdxDBGridSpinColumn;
    GridTemplateFIELD_NAME: TdxDBGridPickColumn;
    GridTemplateCAPTION: TdxDBGridMaskColumn;
    GridTemplateMIN_WIDTH: TdxDBGridSpinColumn;
    GridTemplateMAX_WIDTH: TdxDBGridSpinColumn;
    GridTemplateCAPTION_ALIGN: TdxDBGridImageColumn;
    GridTemplateALIGN: TdxDBGridImageColumn;
    GridTemplateCOLOR: TdxDBGridButtonColumn;
    GridTemplateFONT_NAME: TdxDBGridPickColumn;
    GridTemplateFONT_COLOR: TdxDBGridButtonColumn;
    GridTemplateEDIT_MASK: TdxDBGridMaskColumn;
    GridTemplateFONT_SIZE: TdxDBGridSpinColumn;
    GridTemplateVISIBLE: TdxDBGridCheckColumn;
    edTipStockPrim: TdxDBImageEdit;
    InspTemplate: TdxDBInspector;
    AtsInspectorDBMaskRow1: TdxInspectorDBMaskRow;
    AtsInspectorDBCheckRow1: TdxInspectorDBCheckRow;
    AtsInspectorDBMaskRow2: TdxInspectorDBMaskRow;
    AtsInspectorDBMaskRow3: TdxInspectorDBMaskRow;
    AtsInspectorDBImageRow1: TdxInspectorDBImageRow;
    InspTemplateFIELD_NAME: TdxInspectorDBPickRow;
    AtsInspectorDBSpinRow1: TdxInspectorDBSpinRow;
    AtsInspectorDBSpinRow2: TdxInspectorDBSpinRow;
    AtsInspectorDBImageRow2: TdxInspectorDBImageRow;
    AtsInspectorDBImageRow3: TdxInspectorDBImageRow;
    AtsInspectorDBSpinRow3: TdxInspectorDBSpinRow;
    AtsInspectorDBButtonRow1: TdxInspectorDBButtonRow;
    AtsInspectorDBButtonRow2: TdxInspectorDBButtonRow;
    AtsInspectorDBSpinRow4: TdxInspectorDBSpinRow;
    AtsInspectorDBCheckRow2: TdxInspectorDBCheckRow;
    AtsInspectorDBButtonRow3: TdxInspectorDBButtonRow;
    AtsInspectorDBCheckRow3: TdxInspectorDBCheckRow;
    tabTratareCODMAT: TTabSheet;
    Label5: TLabel;
    edDocumentConex: TdxDBImageEdit;
    Label6: TLabel;
    edTipDescarcare: TdxDBImageEdit;
    edChkNumarAuto: TdxDBCheckEdit;
    edPrefix: TdxDBEdit;
    edNumarStart: TdxDBSpinEdit;
    edNumarEnd: TdxDBSpinEdit;
    LbReportInfo: TLabel;
    Label4: TLabel;
    edTiparireAutomata: TdxfCheckBox;
    LbZileGratie: TLabel;
    edZileValabiltiate: TdxDBSpinEdit;
    AtsDBCheckEdit1: TdxDBCheckEdit;
    Label8: TLabel;
    Label9: TLabel;
    AtsDBImageEdit2: TdxDBImageEdit;
    GridValidari: TdxDBGrid;
    QryValidari: TZQuery;
    DTValidari: TDataSource;
    GridValidariID_FUNCTIUNE: TdxDBGridImageColumn;
    GridValidariTIP_VALIDARE: TdxDBGridImageColumn;
    GridValidariZILE_GRATIE: TdxDBGridSpinColumn;
    edListaFunctii: TdxPopupEdit;
    edZileGratie: TdxSpinEdit;
    edPrioritate: TdxSpinEdit;
    edTipValidare: TdxImageEdit;
    BtnModifica: TSpeedButton;
    BtnDelete: TSpeedButton;
    BtnAdauga: TSpeedButton;
    edRaportGenerat: TdxPopupEdit;
    TreeFunctiuni: TdxDBTreeList;
    TreeFunctiuniCOD_FUNCTIE: TdxDBTreeListMaskColumn;
    TreeFunctiuniDENUMIRE: TdxDBTreeListMaskColumn;
    TreeFunctiuniDESCRIERE: TdxDBTreeListMaskColumn;
    TreeReportList: TdxDBTreeList;
    TreeReportListCAPTURA: TdxDBTreeListMaskColumn;
    TreeReportListIS_REPORT: TdxDBTreeListCheckColumn;
    GridValidariPRIORITATE: TdxDBGridImageColumn;
    tabContabilitate: TTabSheet;
    GridTipuriMateriale: TdxDBGrid;
    GridModContare: TdxDBGrid;
    Label7: TLabel;
    Label11: TLabel;
    Label12: TLabel;
    Label13: TLabel;
    edContDebitor: TdxPopupEdit;
    edContCreditor: TdxPopupEdit;
    edFormulaNota: TdxButtonEdit;
    edClasificatieEcNota: TdxButtonEdit;
    BtnAddNota: TSpeedButton;
    BtnModifyNota: TSpeedButton;
    BtnDeleteNota: TSpeedButton;
    Label3: TLabel;
    edTipuriMaterial: TdxImageEdit;
    BtnAddMaterial: TSpeedButton;
    BtnModifyMaterial: TSpeedButton;
    BtnDeleteMaterial: TSpeedButton;
    DTTipuriMateriale: TDataSource;
    QryTipuriMateriale: TZQuery;
    DTModContare: TDataSource;
    QryModContare: TZQuery;
    GridModContareCONT_DEBITOR: TdxDBGridMaskColumn;
    GridModContareCONT_CREDITOR: TdxDBGridMaskColumn;
    GridModContareID_GEST_TIP_MATERIAL: TdxDBGridImageColumn;
    GridTipuriMaterialeID_GEST_TIP_MATERIAL: TdxDBGridImageColumn;
    GridTipuriMaterialeGENEREAZA_CODMAT: TdxDBGridCheckColumn;
    BtnModifyReport: TSpeedButton;
    GridTipuriMaterialeACCEPT_STOCK_NEGATIV: TdxDBGridCheckColumn;
    Label14: TLabel;
    edTipListaCampuri: TdxImageEdit;
    QryFieldItemsi: TZQuery;
    QryFieldDocum: TZQuery;
    TreePlan: TdxDBTreeList;
    TreePlanCONT: TdxDBTreeListMaskColumn;
    TreePlanROMANA: TdxDBTreeListMaskColumn;
    TreePlanSID: TdxDBTreeListMaskColumn;
    TreePlanSIC: TdxDBTreeListMaskColumn;
    BtnAdaugaColoana: TSpeedButton;
    GridTemplateFORMULA_CALCUL: TdxDBGridMaskColumn;
    BtnCopy: TSpeedButton;
    ppCopiereMenu: TPopupMenu;
    InspTemplateSE_INSUMEAZA: TdxInspectorDBCheckRow;
    AtsInspectorDBMaskRow4: TdxInspectorDBSpinRow;
    tabEvolutieDocument: TTabSheet;
    qryTipProduse: TZQuery;
    gridPozitiiDocum: TdxDBGrid;
    Panel1: TPanel;
    CmdCampuriLipsa: TMenuItem;
    GridModContareCOD_ECONOMIC: TdxDBGridMaskColumn;
    GridModContareCOD_FUNCTIONAL: TdxDBGridMaskColumn;
    edtEsteActiv: TcxDBCheckBox;
    edCod: TcxDBTextEdit;
    edNume: TcxDBTextEdit;
    edDescriere: TcxDBMemo;
    edSuportaFiliala: TcxDBCheckBox;
    ChkComplementare: TcxDBCheckBox;
    edPredator: TcxDBImageComboBox;
    edPrimitor: TcxDBImageComboBox;
    edtIsPrinting: TdxDBCheckEdit;
    imgDMEdit: TdxDBImageEdit;
    DTReportList: TDataSource;
    QryReportList: TZQuery;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
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
    procedure GridTemplateCustomDraw(Sender: TObject; ACanvas: TCanvas;
      ARect: TRect; ANode: TdxTreeListNode; AColumn: TdxDBTreeListColumn;
      const AText: String; AFont: TFont; var AColor: TColor; ASelected,
      AFocused: Boolean; var ADone: Boolean);
    procedure BtnModifyStockPredatorClick(Sender: TObject);
    procedure BtnModifyStockPrimitorClick(Sender: TObject);
    procedure ChkComplementeazaPredatorClick(Sender: TObject);
    procedure BtnAdaugaClick(Sender: TObject);
    procedure BtnModificaClick(Sender: TObject);
    procedure BtnDeleteClick(Sender: TObject);
    procedure TreeFunctiuniDblClick(Sender: TObject);
    procedure TreeFunctiuniKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure edListaFunctiiCloseUp(Sender: TObject; var Text: String;
      var Accept: Boolean);
    procedure edTiparireAutomataClick(Sender: TObject);
    procedure edRaportGeneratCloseUp(Sender: TObject; var Text: String;
      var Accept: Boolean);
    procedure edZileValabiltiateChange(Sender: TObject);
    procedure TreePlanROMANAGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure QryTipuriMaterialeNewRecord(DataSet: TDataSet);
    procedure edTipuriMaterialChange(Sender: TObject);
    procedure GridTipuriMaterialeChangeNode(Sender: TObject; OldNode,
      Node: TdxTreeListNode);
    procedure GridModContareChangeNode(Sender: TObject; OldNode,
      Node: TdxTreeListNode);
    procedure QryTipuriMaterialeAfterOpen(DataSet: TDataSet);
    procedure QryModContareAfterOpen(DataSet: TDataSet);
    procedure BtnAddMaterialClick(Sender: TObject);
    procedure edContDebitorCloseUp(Sender: TObject; var Text: String;
      var Accept: Boolean);
    procedure edContCreditorCloseUp(Sender: TObject; var Text: String;
      var Accept: Boolean);
    procedure TreePlanDblClick(Sender: TObject);
    procedure TreePlanKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure BtnModifyMaterialClick(Sender: TObject);
    procedure BtnAddNotaClick(Sender: TObject);
    procedure BtnModifyNotaClick(Sender: TObject);
    procedure edContDebitorChange(Sender: TObject);
    procedure BtnDeleteMaterialClick(Sender: TObject);
    procedure BtnDeleteNotaClick(Sender: TObject);
    procedure BtnModifyReportClick(Sender: TObject);
    procedure edTipListaCampuriChange(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure BtnAdaugaColoanaClick(Sender: TObject);
    procedure BtnCopyClick(Sender: TObject);
    procedure edClasificatieEcNotaButtonClick(Sender: TObject;
      AbsoluteIndex: Integer);
    procedure QryFieldItemsiNewRecord(DataSet: TDataSet);
    procedure CmdCampuriLipsaClick(Sender: TObject);
    procedure edPredatorPropertiesChange(Sender: TObject);
  private
    { Private declarations }
    FFieldImplicit: Boolean;
    FIsInAddNota: Boolean;
    FItemFieldList,
    FDocumFieldList: TStringList;
    FContDebitor,
    FContCreditor: String;
    FIdDefaDocum : Integer;
    OldTabIndex : Integer;
    FDocEnabled : array[0..3] of Boolean;
    FTemplModificat : Boolean;
    function QryDocs: TDataSet;
    function QryDefaDocs: TDataSet;
    procedure SetCurentRecord(PredIn, PrimIn: Boolean);
    procedure SetDefaDocExists;
    function GetModified: Boolean;
    function GetIdTipDocument: Integer;
    procedure SaveGridTemplate;
    procedure EditTipStock(AField: String; ATipStoc: String);
    procedure RefreshTipStock;

    procedure UpdateEnableNota;
    function  ExistsValue(AGrid: TdxDBGrid; AIndex: Integer; AValue: String): Boolean;

  protected
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

uses DateUnit, Variants,
  SelBugetUnit,
  StockUnit, AdaugareColoanaUnit,
  CommonDBVar;

type TCrackTabControl = class(TCustomTabControl);

procedure TfrmModificDocument.FormCreate(Sender: TObject);
var
  FakeQry: TZQuery;
begin

  { Incarcam si meniurile pentru preluare detaliere document }
  PageDescDocum.ActivePage := tbDescDocum;
  LoadPreluareMeniu;
  DoRefreshDataSet(FrmData.QryTipuriMateriale);
  DoRefreshDataSet(QryReportList);
  FItemFieldList := TStringList.Create;
  FDocumFieldList := TStringList.Create;
  FDocumFieldList.Sorted := True;
  FItemFieldList.Sorted  := True;
  FakeQry := GetTmpADOQuery;
  with FakeQry do
    try
       ParamCheck := False;
       if ExistSQLObject('SP_GEST_GET_CAMPURI_GEST_ITEMSI') = 1 then
         Sql.Add('exec SP_GEST_GET_CAMPURI_GEST_ITEMSI')
       else
         Sql.Add('SELECT NAME FROM SYSCOLUMNS WHERE ID = OBJECT_ID(''GEST_GNMCL'') ORDER BY COLID');
       Open;
       while not Eof do begin
         FItemFieldList.Add(Fields[0].AsString);
         Next;
       end;
       Close;
       Sql.Clear;
       if ExistSQLObject('SP_GEST_GET_CAMPURI_GEST_DOCUM') = 1 then
         Sql.Add('exec SP_GEST_GET_CAMPURI_GEST_DOCUM')
       else
         Sql.Add('SELECT NAME FROM SYSCOLUMNS WHERE ID = OBJECT_ID(''GEST_DOCUM'') ORDER BY COLID');
       Open;
       while not Eof do begin
         FDocumFieldList.Add(Fields[0].AsString);
         Next;
       end;
       if frmData.QryDefaDoc.FindField('TethysTipDoc') <> nil  then begin
         imgDMEdit.Visible := True;
         imgDMEdit.DataField := 'TethysTipDoc';
         SQL.Clear;
         SQL.Add('exec spTethysTipDocumente');
         Open;
         imgDMEdit.Values.Add('');
         imgDMEdit.Descriptions.Add('Neasignat');
         PopulateImage(FakeQry, imgDMEdit.Values, imgDMEdit.Descriptions, 'tipDocumentID', 'tipDocument');
       end
       else
         imgDMEdit.DataField := '';
    finally
       Free;
    end;
  GridTemplateFIELD_NAME.Items.Assign(FItemFieldList);
  InspTemplateFIELD_NAME.Items.Assign(FItemFieldList);
  FTemplModificat := False;



  PopulateImage(QryDocs, edDocumentConex.Values, edDocumentConex.Descriptions,
               'ID_GEST_TIP_DOCUM', 'COD_DOCUM', True, 'Fara Document');

  PopulateImage(FrmData.QryTipuriMateriale,
                edTipuriMaterial.Values,
                edTipuriMaterial.Descriptions,
               'ID_GEST_TIP_MATERIAL',
               'DENUMIRE',True, 'Pe document');
               
  GridTipuriMaterialeID_GEST_TIP_MATERIAL.Values.Assign(edTipuriMaterial.Values);
  GridTipuriMaterialeID_GEST_TIP_MATERIAL.Descriptions.Assign(edTipuriMaterial.Descriptions);
  GridModContareID_GEST_TIP_MATERIAL.Values.Assign(edTipuriMaterial.Values);
  GridModContareID_GEST_TIP_MATERIAL.Descriptions.Assign(edTipuriMaterial.Descriptions);

  PopulateImage(FrmData.QryFunctiuni,
                GridValidariID_FUNCTIUNE.Values,
                GridValidariID_FUNCTIUNE.Descriptions,
               'ID_FUNCTIUNI', 'DENUMIRE');
  RefreshTipStock;
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
 OldTabIndex := -1;
 FFieldImplicit := False;
 edPredatorPropertiesChange(edPredator);
 edPredatorPropertiesChange(edPrimitor);
 FTemplModificat := FFieldImplicit;
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
  if OldTabIndex > -1 then
    SaveGridTemplate;
  with QryDefaDocs do
    if Locate('ID_GEST_TIP_DOCUM;PREDATOR_INTERN;PRIMITOR_INTERN',
                  VarArrayOf([IdTipDocument, PredIn, PrimIn]), []) then begin
       FIdDefaDocum := FieldByName('ID_GEST_DEFA_DOCUM').AsInteger;
       QryFieldItemsi.Close;
       QryFieldDocum.Close;
       edTipListaCampuriChange(edTipListaCampuri);
       { Intializam template-ul de culegere }
       edRaportGenerat.Tag := FieldByName('ID_REPORT').AsInteger;
       edTiparireAutomata.Checked := edRaportGenerat.Tag <> -1;
       if not edTiparireAutomata.Checked then
          edRaportGenerat.Text := 'Nu se genereaza nici un raport'
       else begin
          lNode := TreeReportList.FindNodeByKeyValue(-1 * edRaportGenerat.Tag);
          if Assigned(lNode) then edRaportGenerat.Text := lNode.Strings[TreeReportListCAPTURA.Index];
       end;
       QryValidari.Close;
       QryTipuriMateriale.Close;
       QryModContare.Close;
       qryTipProduse.Close;
       QryTipuriMateriale.Params[0].Value := FIdDefaDocum;
       QryModContare.Params[0].Value      := FIdDefaDocum;
       QryValidari.Params[0].Value        := FIdDefaDocum;
       qryTipProduse.Params[0].Value      := FIdDefaDocum;
       QryValidari.Open;
       QryTipuriMateriale.Open;
       QryModContare.Open;
       qryTipProduse.Open;
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

procedure TfrmModificDocument.GridTemplateCustomDraw(Sender: TObject;
  ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  AColumn: TdxDBTreeListColumn; const AText: String; AFont: TFont;
  var AColor: TColor; ASelected, AFocused: Boolean; var ADone: Boolean);
var
  lColStr, lFontStr: String;
begin
  lColStr  := ANode.Strings[GridTemplateCOLOR.Index];
  lFontStr := ANode.Strings[GridTemplateFONT_COLOR.Index];
  if Trim(lColStr) > '' then begin
     AColor := TColor(StrToInt(lColStr));
  end;
  if Trim(lFontStr) > '' then begin
     AFont.Color := TColor(StrToInt(lFontStr));
  end;
end;

procedure TfrmModificDocument.BtnModifyStockPredatorClick(Sender: TObject);
begin
  EditTipStock(edTipStockPred.DataField, Trim(edTipStockPred.Text));
end;

procedure TfrmModificDocument.EditTipStock(AField: String; ATipStoc: String);
var IsNew: Boolean;
begin
  IsNew := (ATipStoc = '') or (ATipStoc = '-1');
  if not IsNew then begin
     if not FrmData.QryTipStock.Locate('ID_GEST_TIP_STOC', ATipStoc, []) then
        raise EContaHandledError.Create('EROARE interna : - nu se poate localiza tipul de stock !');
     FrmData.QryTipStock.Edit;
  end
  else begin
    FrmData.QryTipStock.Append;
    FrmData.QryTipStock.FieldByName('DENUMIRE').AsString := 'Stoc nou';
  end;
  with TfrmStock.Create(Self) do
    try
       edDocument.Values.Assign(edDocumentConex.Values);
       edDocument.Descriptions.Assign(edDocumentConex.Descriptions);
       GridDefaDocID_GEST_TIP_DOCUM.Values.Assign(edDocumentConex.Values);
       GridDefaDocID_GEST_TIP_DOCUM.Descriptions.Assign(edDocumentConex.Descriptions);

       if ShowModal = mrOk then begin
          FrmData.QryTipStock.Post;
          if IsNew then begin
             if not (QryDefaDocs.State in [dsEdit, dsInsert]) then QryDefaDocs.Edit;
             QryDefaDocs.FieldByName(AField).AsInteger := FrmData.QryTipStock.FieldByName('ID_GEST_TIP_STOC').AsInteger;
             QryDefaDocs.Post;
          end
       end
       else FrmData.QryTipStock.Cancel;
    finally
       Free;
    end;
end;

procedure TfrmModificDocument.BtnModifyStockPrimitorClick(Sender: TObject);
begin
  EditTipStock(edTipStockPrim.DataField, Trim(edTipStockPrim.Text));
end;

procedure TfrmModificDocument.ChkComplementeazaPredatorClick(
  Sender: TObject);
begin
  edPredatorPropertiesChange(edPredator);
end;

procedure TfrmModificDocument.RefreshTipStock;
begin
  PopulateImage(FrmData.QryTipStock, edTipStockPred.Values, edTipStockPred.Descriptions,
               'ID_GEST_TIP_STOC', 'DENUMIRE', True, 'Fara stoc');
  edTipStockPred.Values.Add('-1');
  edTipStockPred.Descriptions.Add('Nomenclator Materiale');
  edTipStockPrim.Values.Assign(edTipStockPred.Values);
  edTipStockPrim.Descriptions.Assign(edTipStockPred.Descriptions);
end;

procedure TfrmModificDocument.SaveGridTemplate;
var OldEdit: Boolean;
begin
  if QryFieldItemsi.State in [dsInsert, dsEdit] then QryFieldItemsi.Post;
  if QryFieldDocum.State  in [dsInsert, dsEdit] then QryFieldDocum.Post;
  with FrmData.QryDefaDoc do begin
    OldEdit := State in [dsEdit, dsInsert];
    if not OldEdit then Edit;
    if not edTiparireAutomata.Checked then FieldByName('ID_REPORT').AsInteger := -1
    else FieldByName('ID_REPORT').AsInteger := edRaportGenerat.Tag;
    Post;
    if OldEdit then Edit;
    FTemplModificat := False;
  end;
end;

procedure TfrmModificDocument.BtnAdaugaClick(Sender: TObject);
begin
  with QryValidari do begin
    Append;
    FieldByName('ID_GEST_DEFA_DOCUM').AsInteger := FrmData.QryDefaDoc.FieldByName('ID_GEST_DEFA_DOCUM').AsInteger;
    FieldByName('ZILE_GRATIE').AsInteger        := edZileGratie.IntValue;
    FieldByName('PRIORITATE').AsInteger         := edPrioritate.IntValue;
    FieldByName('TIP_VALIDARE').AsString        := edTipValidare.Text;
    FieldByName('ID_FUNCTIUNE').AsInteger       := edListaFunctii.Tag; 
    Post;
  end;
end;

procedure TfrmModificDocument.BtnModificaClick(Sender: TObject);
begin
  with QryValidari do begin
    Edit;
    FieldByName('ID_GEST_DEFA_DOCUM').AsInteger := FrmData.QryDefaDoc.FieldByName('ID_GEST_DEFA_DOCUM').AsInteger;
    FieldByName('ZILE_GRATIE').AsInteger        := edZileGratie.IntValue;
    FieldByName('PRIORITATE').AsInteger         := edPrioritate.IntValue;
    FieldByName('TIP_VALIDARE').AsString        := edTipValidare.Text;
    FieldByName('ID_FUNCTIUNE').AsInteger       := edListaFunctii.Tag;
    Post;
  end;
end;

procedure TfrmModificDocument.BtnDeleteClick(Sender: TObject);
begin
  if (QryValidari.IsEmpty) or
     (MessageDlg('Doriti stergerea validarii pentru documentul curent?', mtConfirmation, [mbYes, mbNo], 0) <> mrYes) then Exit;
  QryValidari.Delete;
end;

procedure TfrmModificDocument.TreeFunctiuniDblClick(Sender: TObject);
begin
  with TdxDBTreeList(Sender) do
    if FocusedNode <> nil then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
end;

procedure TfrmModificDocument.TreeFunctiuniKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ((Key in [VK_UP, VK_DOWN]) and (ssAlt in Shift)) or
      ((Key = VK_F4) and not (ssAlt in Shift)) or (Key = VK_ESCAPE) then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(False);
  if (Key = VK_RETURN) and (TdxDBTreeList(Sender).FocusedNode <> nil) then
     (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
end;

procedure TfrmModificDocument.edListaFunctiiCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
var
  lNode: TdxDBTreeListNode;
begin
  if Accept then begin
    lNode := TdxDBTreeListNode(TreeFunctiuni.FocusedNode);
    if Assigned(lNode) then Text := lNode.Strings[TreeFunctiuniDENUMIRE.Index];
    edListaFunctii.Tag := lNode.Id;
  end;
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

procedure TfrmModificDocument.edRaportGeneratCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
var lNode: TdxDBTreeListNode;
begin
  if Accept then begin
    lNode := TdxDBTreeListNode(TreeReportList.FocusedNode);
    if Assigned(lNode) then Text := lNode.Strings[TreeReportListCAPTURA.Index];
    edRaportGenerat.Tag := -1*lNode.Id;
    FTemplModificat := True;
  end;
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
  DataSet.FieldByName('ID_GEST_DEFA_DOCUM').AsInteger := FrmData.QryDefaDoc.FieldByName('ID_GEST_DEFA_DOCUM').AsInteger;
end;

procedure TfrmModificDocument.edTipuriMaterialChange(Sender: TObject);
begin
  { Activam sau dezactivam butoanele de adaugare, modificare }
  if edTipuriMaterial.Text = '-1' then begin
     BtnAddMaterial.Enabled := False;
     BtnModifyMaterial.Enabled := False;
  end
  else begin
     BtnAddMaterial.Enabled := not ExistsValue(GridTipuriMateriale, GridTipuriMaterialeID_GEST_TIP_MATERIAL.Index, edTipuriMaterial.Text);
  end;
  UpdateEnableNota;
end;

function TfrmModificDocument.ExistsValue(AGrid: TdxDBGrid; AIndex: Integer;
  AValue: String): Boolean;
var I: Integer;
begin
  Result := False;
  for I := 0 to AGrid.Count-1 do
    if AGrid.Items[I].Strings[AIndex] = AValue then begin
       Result := True;
       Break;
    end;
end;

procedure TfrmModificDocument.GridTipuriMaterialeChangeNode(Sender: TObject;
  OldNode, Node: TdxTreeListNode);
begin
  BtnDeleteMaterial.Enabled := Node <> nil;
  { Actualizam si informatiile }
  if BtnDeleteMaterial.Enabled then begin
     edTipuriMaterial.Text := Node.Strings[GridTipuriMaterialeID_GEST_TIP_MATERIAL.Index];
  end;
end;

procedure TfrmModificDocument.GridModContareChangeNode(Sender: TObject;
  OldNode, Node: TdxTreeListNode);
var
  lNode: TdxTreeListNode;
begin
  if FIsInAddNota then Exit;
  BtnDeleteNota.Enabled := Node <> nil;
  { Actualizam si informatiile }
  if BtnDeleteNota.Enabled then begin
     FContDebitor := Node.Strings[GridModContareCONT_DEBITOR.Index];
     lNode := TreePlan.FindNodeByKeyValue(FContDebitor);
     if Assigned(lNode) then edContDebitor.Text := lNode.Strings[1];
     FContCreditor := Node.Strings[GridModContareCONT_CREDITOR.Index];
     lNode := TreePlan.FindNodeByKeyValue(FContCreditor);
     if Assigned(lNode) then edContCreditor.Text := lNode.Strings[1];
     edTipuriMaterial.Text := Node.Strings[GridModContareID_GEST_TIP_MATERIAL.Index];
     edClasificatieEcNota.Text := Node.Strings[GridModContareCOD_ECONOMIC.Index];
  end;
end;

procedure TfrmModificDocument.QryTipuriMaterialeAfterOpen(
  DataSet: TDataSet);
begin
  GridTipuriMaterialeChangeNode(GridTipuriMateriale, nil, GridTipuriMateriale.TopNode);
end;

procedure TfrmModificDocument.QryModContareAfterOpen(DataSet: TDataSet);
begin
  GridModContareChangeNode(GridModContare, nil, GridModContare.TopNode);
end;

procedure TfrmModificDocument.BtnAddMaterialClick(Sender: TObject);
begin
  QryTipuriMateriale.DisableControls;
  QryTipuriMateriale.Append;
  QryTipuriMateriale.FieldByName('GENEREAZA_CODMAT').AsBoolean := False;
  QryTipuriMateriale.FieldByName('ID_GEST_TIP_MATERIAL').AsString  := edTipuriMaterial.Text;
  QryTipuriMateriale.Post;
  QryTipuriMateriale.EnableControls;
  edTipuriMaterialChange(edTipuriMaterial);
end;

procedure TfrmModificDocument.edContDebitorCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
var
  lNode: TdxDBTreeListNode;
begin
  if Accept then begin
     lNode := TdxDBTreeListNode(TreePlan.FocusedNode);
     if Assigned(lNode) then begin
        FContDebitor := lNode.Id;
        Text         := lNode.Strings[1];
     end;
  end;
end;

procedure TfrmModificDocument.edContCreditorCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
var lNode: TdxDBTreeListNode;
begin
  if Accept then begin
     lNode := TdxDBTreeListNode(TreePlan.FocusedNode);
     if Assigned(lNode) then begin
        FContCreditor := lNode.Id;
        Text          := lNode.Strings[1];
     end;
  end;
end;

procedure TfrmModificDocument.TreePlanDblClick(Sender: TObject);
begin
  { Inchidem Cu Accept }
  with TdxDBTreeList(Sender) do
    if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
end;

procedure TfrmModificDocument.TreePlanKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ((Key in [VK_UP, VK_DOWN]) and (ssAlt in Shift)) or
      ((Key = VK_F4) and not (ssAlt in Shift)) or (Key = VK_ESCAPE) then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(False);
  if (Key = VK_RETURN) and (TdxDBTreeList(Sender).FocusedNode <> nil)
     and (not TdxDBTreeList(Sender).FocusedNode.HasChildren) then
     (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
end;

procedure TfrmModificDocument.UpdateEnableNota;
begin
{  if edTipuriMaterial.Text = '-1' then
     BtnAddNota.Enabled    := not ExistsValue(GridModContare, GridModContareID_GEST_TIP_MATERIAL.Index, '-1')
  else BtnAddNota.Enabled  := not ExistsValue(GridModContare, GridModContareID_GEST_TIP_MATERIAL.Index, edTipuriMaterial.Text);}
  BtnAddNota.Enabled       := (Trim(FContDebitor) > '')
                              and (Trim(FContCreditor)>'');
//                              and (Trim(edFormulaNota.Text) > '');
  BtnModifyNota.Enabled    := BtnAddNota.Enabled;
end;

procedure TfrmModificDocument.BtnModifyMaterialClick(Sender: TObject);
begin
  QryTipuriMateriale.Edit;
  QryTipuriMateriale.FieldByName('ID_GEST_TIP_MATERIAL').AsString  := edTipuriMaterial.Text;
  QryTipuriMateriale.Post;
  edTipuriMaterialChange(edTipuriMaterial);
end;

procedure TfrmModificDocument.BtnAddNotaClick(Sender: TObject);
begin
  FIsInAddNota := True;
  try
    QryModContare.Append;
    QryModContare.FieldByName('ID_GEST_TIP_MATERIAL').AsString := edTipuriMaterial.Text;
    QryModContare.FieldByName('CONT_DEBITOR').AsString         := FContDebitor;
    QryModContare.FieldByName('CONT_CREDITOR').AsString        := FContCreditor;
    QryModContare.FieldByName('FORMULA').AsString              := edFormulaNota.Text;
    QryModContare.FieldByName('COD_ECONOMIC').AsString         := edClasificatieEcNota.Text;
    QryModContare.Post;
  finally
    FIsInAddNota := False;
  end;
  UpdateEnableNota;
end;

procedure TfrmModificDocument.BtnModifyNotaClick(Sender: TObject);
begin
  QryModContare.Edit;
  QryModContare.FieldByName('ID_GEST_TIP_MATERIAL').AsString := edTipuriMaterial.Text;
  QryModContare.FieldByName('CONT_DEBITOR').AsString         := FContDebitor;
  QryModContare.FieldByName('CONT_CREDITOR').AsString        := FContCreditor;
  QryModContare.FieldByName('FORMULA').AsString              := edFormulaNota.Text;
  QryModContare.FieldByName('COD_ECONOMIC').AsString         := edClasificatieEcNota.Text;
  QryModContare.Post;
  UpdateEnableNota;
end;

procedure TfrmModificDocument.edContDebitorChange(Sender: TObject);
begin
  if TdxPopupEdit(Sender).Text = '%' then
    if Sender = edContDebitor then FContDebitor := '%'
    else FContCreditor := '%';
  UpdateEnableNota;
end;

procedure TfrmModificDocument.BtnDeleteMaterialClick(Sender: TObject);
var lNode: TdxDBGridNode;
begin
  lNode := TdxDBGridNode(GridTipuriMateriale.FocusedNode);
  if Assigned(lNode) then lNode.Delete;
end;

procedure TfrmModificDocument.BtnDeleteNotaClick(Sender: TObject);
var
  lNode: TdxDBGridNode;
begin
  lNode := TdxDBGridNode(GridModContare.FocusedNode);
  if Assigned(lNode) then lNode.Delete;
end;

procedure TfrmModificDocument.BtnModifyReportClick(Sender: TObject);
{RBUILDER}//var lCustomReport: TCustomReport;
begin
  {lCustomReport := TCustomReport(LoadReportEx(edRaportGenerat.Tag).Owner);
  with lCustomReport do
    try
       RapDesigner.ShowModal;
    finally
       Free;
    end;    }
end;

procedure TfrmModificDocument.edTipListaCampuriChange(Sender: TObject);

  procedure TestAndOpen(AQry: TZQuery);
   begin
     if not AQry.Active then begin
        AQry.Params[0].Value := FIdDefaDocum;
        AQry.Open;
        if (AQry.IsEmpty) and
           (MessageDlg('Nu aveti nici un camp definit. Doriti generarea campurilor implicite?', mtConfirmation, [mbYes, mbNo], 0) = mrYes) then
              with GetTmpADOQuery do
                try
                   ParamCheck := False;
                   if AQry.Tag = 0 then
                      Sql.Add('INSERT INTO GEST_DEFA_DOCUM_ITEMSI (ID_GEST_DEFA_DOCUM, FIELD_NAME, CAPTION, CAPTION_ALIGN, POS, READONLY, VISIBLE, REQUIRED)')
                   else Sql.Add('INSERT INTO GEST_DEFA_DOCUM_DOCUMENT (ID_GEST_DEFA_DOCUM, FIELD_NAME, CAPTION, CAPTION_ALIGN, POS, READONLY, VISIBLE, REQUIRED)');
                   Sql.Add('SELECT '+IntToStr(FIdDefaDocum)+', NAME AS FIELD_NAME, NAME AS CAPTION, 2 AS CAPTION_ALIGN, COLID AS POS,');
                   Sql.Add('CONVERT(BIT, 0) AS READONLY, CONVERT(BIT, 1) AS VISIBLE, CONVERT(BIT, 0) AS REQUIRED');
                   if AQry.Tag = 0 then
                      Sql.Add('FROM SYSCOLUMNS WHERE ID = OBJECT_ID(''CULGEST_ITEMSI'') ORDER BY COLID')
                   else Sql.Add('FROM SYSCOLUMNS WHERE ID = OBJECT_ID(''CULGEST_DOCUM'') ORDER BY COLID');
                   ExecSql;
                   AQry.Active     := False;
                   AQry.Active     := True;
                   FFieldImplicit  := True;
                finally
                   Free;
                end;
     end;
     DTTemplateCrid.DataSet := AQry;
     if AQry.Tag = 0 then begin
        GridTemplateFIELD_NAME.Items.Assign(FItemFieldList);
        InspTemplateFIELD_NAME.Items.Assign(FItemFieldList);
        GridTemplate.KeyField := 'ID_GEST_DEFA_DOCUM_ITEMSI';
     end
     else begin
        GridTemplateFIELD_NAME.Items.Assign(FDocumFieldList);
        InspTemplateFIELD_NAME.Items.Assign(FDocumFieldList);
        GridTemplate.KeyField := 'ID_GEST_DEFA_DOCUM_DOCUMENT';
     end;
   end;

begin
  { Trecem forumulele de calcul pe document sau pe itemsi }
  if edTipListaCampuri.Text = '0' then
     TestAndOpen(QryFieldItemsi)
  else TestAndOpen(QryFieldDocum);
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
  FIsInAddNota := True;
  try
    QryModContare.Append;
    QryModContare.FieldByName('ID_GEST_TIP_MATERIAL').AsString := lValues[1];
    QryModContare.FieldByName('CONT_DEBITOR').AsString         := lValues[2];
    QryModContare.FieldByName('CONT_CREDITOR').AsString        := lValues[3];
    QryModContare.FieldByName('FORMULA').AsString              := lValues[4];
    QryModContare.FieldByName('CONDITIE').AsString             := lValues[5];
    edFormulaNota.Text := lValues[4];
    QryModContare.Post;
  finally
    FIsInAddNota := False;
  end;
  UpdateEnableNota;
end;

procedure TfrmModificDocument.edClasificatieEcNotaButtonClick(Sender: TObject;
  AbsoluteIndex: Integer);
var
  aRes, aCodEco : String;
  idProiect : Integer;
begin
  aCodEco := edClasificatieEcNota.Text;
  aRes := NewSelectarePlanEconomic(aCodEco, idProiect, '', -1, True);
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

begin
  with GetTmpADOQuery do
    try
      Sql.Add('EXEC SP_GET_LST_DEFALCARI_DOCUMENTE');
      Open;
      while not Eof do begin
        lRootItem := GetMenuLevel(Trim(FieldByName('DEN_DOCUM').AsString));
        lTmpItem := TMenuItem.Create(lRootItem);
        lTmpItem.Caption := Trim(FieldByName('DETALII_DOCUM').AsString);
        lTmpItem.OnClick := DoPreluareDocum;
        lTmpItem.Tag     := FieldByName('ID_GEST_DEFA_DOCUM').AsInteger;
        lRootItem.Add(lTmpItem);
        Next;
      end;
    finally
      Free;
    end;
end;

procedure TfrmModificDocument.DoPreluareDocum(Sender: TObject);
begin
  with GetTmpADOQuery do
    try
      Sql.Add('EXEC SP_PRELUARE_DEFALCARE_DOCUMENT '+IntToStr(TMenuItem(Sender).Tag)+', '+IntToStr(IdTipDocument)+', ');
      Sql.Add(IntToStr(1 - (tabDefaDoc.TabIndex div 2))+', '+IntToStr(1 - (tabDefaDoc.TabIndex mod 2)));
      ExecSql;
      RefreshDefalcari;
    finally
      Free;
    end;
end;

procedure TfrmModificDocument.RefreshDefalcari;
var
  lLastID : Integer;
begin
  with FrmData.DTDefaDoc.DataSet do begin
    DisableControls;
    Close;
    Open;
    EnableControls;
  end;
  with QryDefaDocs do begin
    lLastID := FieldByName('ID_GEST_DEFA_DOCUM').AsInteger;
    DisableControls;
    Close;
    Open;
    Locate('ID_GEST_DEFA_DOCUM', lLastID, []);
    LoadRemainings;
    EnableControls;
  end;
end;

procedure TfrmModificDocument.QryFieldItemsiNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('ID_GEST_DEFA_DOCUM').AsInteger := FIdDefaDocum;
end;

procedure TfrmModificDocument.CmdCampuriLipsaClick(Sender: TObject);
begin
  with GetTmpADOQuery do
    try
      Sql.Add('EXEC SP_GEST_DOCUM_CAMPURI_LIPSA '+ IntToStr(FIdDefaDocum));
      ExecSql;
      RefreshDefalcari;
    finally
      Free;
    end;
end;

procedure TfrmModificDocument.edPredatorPropertiesChange(Sender: TObject);
var lPredVal,
    lPrimVal: Integer;

    procedure SetControlEnabled(AControl: TWinControl; AEnabled: Boolean);
    var J: Integer;
     begin
       for J := 0 to AControl.ControlCount-1 do
         if AControl.Controls[J] is TWinControl then
            SetControlEnabled(TWinControl(AControl.Controls[J]), AEnabled);
       AControl.Enabled := AEnabled;
     end;

    procedure SetFirstTab;
     var I: Integer;
    begin
      SetControlEnabled(pnDescDoc, False);
      tabDefaDoc.Enabled := False;
      for I := Low(FDocEnabled) to High(FDocEnabled) do
        if FDocEnabled[I] then begin
           SetControlEnabled(pnDescDoc, True);
           tabDefaDoc.Enabled := True;
           tabDefaDoc.TabIndex := I;
           tabDefaDocChange(tabDefaDoc);
           Break;
        end;
    end;

begin
  if Trim(edPredator.Text) > '' then lPredVal := edPredator.ItemIndex else lPredVal  := 0;
  if Trim(edPrimitor.Text) > '' then lPrimVal := edPrimitor.ItemIndex else lPrimVal  := 0;
  FDocEnabled[0] := (lPredVal and 01 = 01) and (lPrimVal and 01 = 01) and (not ChkComplementare.Checked);
  FDocEnabled[1] := (lPredVal and 01 = 01) and (lPrimVal and 02 = 02);
  FDocEnabled[2] := (lPredVal and 02 = 02) and (lPrimVal and 01 = 01);
  FDocEnabled[3] := (lPredVal and 02 = 02) and (lPrimVal and 02 = 02) and (not ChkComplementare.Checked);
  SetDefaDocExists;
  TCrackTabControl(tabDefaDoc).UpdateTabImages;
  { Selectam daca avem ce }
  SetFirstTab;
end;


end.
