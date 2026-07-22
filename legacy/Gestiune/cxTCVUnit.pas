unit cxTCVUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, ExtCtrls, Db, ZDataSet, DetaliiDocumUnit, ATSDBEvaluator, Buttons,
  DBSumLst, RepartitorPanel, cxButtons, ActnList, ImgList, Menus, cxControls,
  cxContainer, cxEdit, cxLabel, cxDBLabel, AlopDisponibil, dxExEdtr, dxCntner,
  dxDBTL, dxDBELib, dxDBTLCl, dxGrClms, dxDBCtrl, dxDBGrid, dxGrClEx, dxTL,
  dxfProgressBar, dxEdLib, dxEditor, dxmdaset, frxClass, cxLookAndFeelPainters,
  cxPC, cxCheckBox, cxGraphics, cxDataStorage, cxDBData, cxGridLevel, cxClasses,
  cxGridCustomView,  cxGridCustomTableView, cxGridTableView, cxGridDBTableView,
  cxGrid, ZAbstractRODataset, ZAbstractDataset, cxLookAndFeels, cxStyles, cxCustomData,
  cxFilter, cxData, dxBarBuiltInMenu, cxNavigator, cxSplitter, unitMemTableEx,
  cxTextEdit, cxMaskEdit, cxDropDownEdit, cxLookupEdit, cxDBLookupEdit, cxDBLookupComboBox,
  cxGroupBox, cxRepartitorPanel, cxTL, cxTLdxBarBuiltInMenu, cxInplaceContainer, cxTLData,
  cxDBTL, Vcl.ComCtrls, dxCore, cxDateUtils, cxCalendar, cxCurrencyEdit,
  cxImageComboBox, dxDateRanges, cxDataControllerConditionalFormattingRulesManagerDialog,
  cxButtonEdit, dxScrollbarAnnotations;

const
  WM_SENDPOSTITEMS        = WM_USER + 4;
  cst_DeleteAll : Boolean = False;
  ErrorOnSameCodMat       = False;

type
  TCrackATSDBTreeList = class(TdxDBTreeList);

  TGestiune = record
    FCodRep : Integer;
    GestInt : Boolean;
    Assigned: Boolean;
  end;

  TfrmcxTcv = class(TForm)
    pnDocument: TPanel;
    lbDocument: TLabel;
    lbDataDoc: TLabel;
    edNumarDoc: TcxButtonEdit;
    edPredator: TcxRepartitorPanel;
    edPrimitor: TcxRepartitorPanel;
    lbTipDocument: TLabel;
    pnBottom: TPanel;
    pnClient: TPanel;
    DTItemsi: TDataSource;
    QryItemsi: TZQuery;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    BtnValidare: TcxButton;
    DTDropDownRep: TDataSource;
    editStyle: TdxEditStyleController;
    QryDocument: TZQuery;
    GridItemsi: TdxDBGrid;
    GridItemsiDENMAT: TdxDBGridButtonColumn;
    GridItemsiTIPMAT: TdxDBGridMRUColumn;
    GridItemsiCODMAT: TdxDBGridColumn;
    GridItemsiID_GEST_TIP_STOCK: TdxDBGridMaskColumn;
    GridItemsiSTARE: TdxDBGridMaskColumn;
    GridItemsiLOHN: TdxDBGridCheckColumn;
    GridItemsiDATA_COD: TdxDBGridDateColumn;
    GridItemsiDATA_EXPIRARE: TdxDBGridDateColumn;
    GridItemsiDETALII_ANGAJAMENT: TdxDBGridPopupColumn;
    GridItemsiCOTA_TVA: TdxDBGridCurrencyColumn;
    GridItemsiPRET_LIVRARE_VALUTA: TdxDBGridCurrencyColumn;
    GridItemsiPRET_LIVRARE_TVA: TdxDBGridCurrencyColumn;
    GridItemsiPRET_RECEPTIE: TdxDBGridCurrencyColumn;
    GridItemsiCANTITATE: TdxDBGridCurrencyColumn;
    GridItemsiSTOCK_AFTER: TdxDBGridCurrencyColumn;
    GridItemsiSTOCK_BEFORE: TdxDBGridCurrencyColumn;
    GridItemsiUM: TdxDBGridMRUColumn;
    GridItemsiPRET_UNITAR: TdxDBGridCurrencyColumn;
    GridItemsiPRET_RECEPTIE_TVA: TdxDBGridCurrencyColumn;
    GridItemsiVALOARE_RECEPTIE: TdxDBGridCurrencyColumn;
    GridItemsiTVA_RECEPTIE: TdxDBGridCurrencyColumn;
    GridItemsiVALOARE_RECEPTIE_TVA: TdxDBGridCurrencyColumn;
    GridItemsiTVA: TdxDBGridCurrencyColumn;
    GridItemsiPRODUS: TdxDBGridImageColumn;
    GridItemsiID_GEST_TIP_MATERIAL: TdxDBGridPopupColumn;
    GridItemsiSEMN_CANTITATE: TdxDBGridImageColumn;
    GridItemsiSE_EMITE: TdxDBGridCheckColumn;
    GridItemsiNR_CHITANTA: TdxDBGridButtonColumn;
    GridItemsiCOD_BARA: TdxDBGridMaskColumn;
    GridItemsiContD: TdxDBGridPopupColumn;
    GridItemsiContC: TdxDBGridPopupColumn;
    GridItemsiCategorie: TdxDBGridPopupColumn;
    GridItemsiRepD: TdxDBGridPopupColumn;
    GridItemsiRepC: TdxDBGridPopupColumn;
    GridItemsiCOD_FUNCTIONAL: TdxDBGridPopupColumn;
    GridItemsiCOD_ECONOMIC: TdxDBGridPopupColumn;
    QryDrepturiDocs: TZQuery;
    DTDocument: TDataSource;
    BtnModificaDoc: TcxButton;
    SaveProgress: TdxfProgressBar;
    BtnDelDocument: TcxButton;
    pnDetaliiFct: TPanel;
    Label1: TLabel;
    edDelegat: TcxRepartitorPanel;
    gridDelegati: TdxDBGrid;
    gridDelegatiNUME_COMPLET: TdxDBGridMaskColumn;
    gridDelegatiCNP_DELEGAT: TdxDBGridMaskColumn;
    lblMasina: TLabel;
    edMijTransport: TcxRepartitorPanel;
    gridMijTransport: TdxDBGrid;
    gridMijTransportNUMAR_AUTO: TdxDBGridMaskColumn;
    gridDelegatiTIP_BI: TdxDBGridImageColumn;
    gridDelegatiSERIE_BI: TdxDBGridMaskColumn;
    gridDelegatiNR_BI: TdxDBGridMaskColumn;
    btnValidareConex: TcxButton;
    TreeTipMaterial: TdxDBTreeList;
    DTTipuriMateriale: TDataSource;
    QryTipuriMatriale: TZQuery;
    TreeTipMaterialID_GEST_TIP_MATERIAL: TdxDBTreeListMaskColumn;
    TreeTipMaterialDENUMIRE: TdxDBTreeListColumn;
    TreeTipMaterialGENEREAZA_CODMAT: TdxDBTreeListCheckColumn;
    TreeTipMaterialACCEPT_STOCK_NEGATIV: TdxDBTreeListCheckColumn;
    TreeTipMaterialID_PARINTE: TdxDBTreeListMaskColumn;
    TreeTipMaterialID_GEST_TIP_PRODUSE: TdxDBTreeListMaskColumn;
    TreeTipMaterialTIP_PRODUS: TdxDBTreeListImageColumn;
    btnCopyPositions: TcxButton;
    btnCopyStocPred: TcxButton;
    BevelSeparare: TBevel;
    StyleController: TdxEditStyleController;
    TCVAction: TActionList;
    Cmd_DelDoc: TAction;
    TCVImageList: TImageList;
    Cmd_ModifyDoc: TAction;
    Cmd_CopyPoz: TAction;
    Cmd_StocPerioada: TAction;
    TreeTipMaterialCOD_ECONOMIC: TdxDBTreeListMaskColumn;
    TreeTipMaterialCONT_CAUTARE: TdxDBTreeListMaskColumn;
    ppMeniu: TPopupMenu;
    CmdStegereGrid: TAction;
    CmdAdaugareGrid: TAction;
    CmdSchimbaSemn: TAction;
    Adaugare1: TMenuItem;
    Stergerepozitie1: TMenuItem;
    N1: TMenuItem;
    Schimbasemnpozitiecurenta1: TMenuItem;
    btnDM: TcxButton;
    BtnDetaliiDocum: TcxButton;
    pageControl: TcxPageControl;
    tabDoc: TcxTabSheet;
    tabNota: TcxTabSheet;
    pnNotaContabila: TPanel;
    TreePlan: TdxDBTreeList;
    TreePlanCONT: TdxDBTreeListMaskColumn;
    TreePlanROMANA: TdxDBTreeListMaskColumn;
    TreePlanFctCont: TdxDBTreeListColumn;
    TreeCategorii: TdxDBTreeList;
    TreeCategoriiId: TdxDBTreeListMaskColumn;
    TreeCategoriiDenumire: TdxDBTreeListMaskColumn;
    TreeCategoriiPrefix: TdxDBTreeListColumn;
    DTCategorii: TDataSource;
    qryCategorii: TZQuery;
    chkGenAutomat: TcxCheckBox;
    pnNota: TPanel;
    GridNotaC: TcxGridDBTableView;
    cxGridNotaCLevel: TcxGridLevel;
    cxGridNotaC: TcxGrid;
    btnGenereazaNote: TcxButton;
    DTNoteDoc: TDataSource;
    qryNoteDoc: TZQuery;
    GridNotaCid: TcxGridDBColumn;
    GridNotaCid_gest_docum: TcxGridDBColumn;
    GridNotaCid_gest_itemsi: TcxGridDBColumn;
    GridNotaCid_gest_defa_docum_nota_model: TcxGridDBColumn;
    GridNotaCContD: TcxGridDBColumn;
    GridNotaCRepartitorDebit: TcxGridDBColumn;
    GridNotaCContC: TcxGridDBColumn;
    GridNotaCRepartitorCredit: TcxGridDBColumn;
    GridNotaCCodFunctional: TcxGridDBColumn;
    GridNotaCCodEconomic: TcxGridDBColumn;
    GridNotaCidAngajamenteDefalcare: TcxGridDBColumn;
    GridNotaCvaloare: TcxGridDBColumn;
    GridNotaCexplicatie: TcxGridDBColumn;
    GridNotaCdescriere_document: TcxGridDBColumn;
    GridNotaCcod_document: TcxGridDBColumn;
    GridNotaCnr_docum: TcxGridDBColumn;
    GridNotaCdata_document: TcxGridDBColumn;
    GridNotaCdata_scadenta: TcxGridDBColumn;
    qryReadOnlyProd: TZQuery;
    N2: TMenuItem;
    CmdGenereazaNumere: TAction;
    CmdCopyContinut: TAction;
    Copierecontinut1: TMenuItem;
    N3: TMenuItem;
    Genereazanumereinserie1: TMenuItem;
    DTRep: TDataSource;
    DTRepartitori: TDataSource;
    btnBonFiscal: TcxButton;
    Cmd_SpargePeProcente: TAction;
    Procente1: TMenuItem;
    pnDocDetalii: TPanel;
    pnDocumentBody: TPanel;
    splitterDetalii: TcxSplitter;
    edTipDocument: TcxLookupComboBox;
    edDataDoc: TcxDateEdit;
    TreeRepartitori: TcxDBTreeList;
    TreeRepartitoriNUME: TcxDBTreeListColumn;
    TreeRepartitoriADRESA: TcxDBTreeListColumn;
    TreeRepartitoriCONT: TcxDBTreeListColumn;
    TreeRepartitoriCODFISC: TcxDBTreeListColumn;
    TreeRepartitoriGESTINT: TcxDBTreeListColumn;
    TreeRepartitoriTIP_GESTIUNE: TcxDBTreeListColumn;
    procedure BtnCancelClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure pnDocumentResize(Sender: TObject);
    procedure edDataDocValidate(Sender: TObject; var ErrorText: String;
      var Accept: Boolean);
    procedure QryItemsiNewRecord(DataSet: TDataSet);
    procedure QryItemsiAfterOpen(DataSet: TDataSet);
    procedure QryDocumentNewRecord(DataSet: TDataSet);
    procedure BtnOkClick(Sender: TObject);
    procedure GridItemsiDENMATButtonClick(Sender: TObject;
      AbsoluteIndex: Integer);
    procedure TreeRepartitoriCustomDrawCell(Sender: TObject;
      ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
      AColumn: TdxTreeListColumn; ASelected, AFocused,
      ANewItemRow: Boolean; var AText: String; var AColor: TColor;
      AFont: TFont; var AAlignment: TAlignment; var ADone: Boolean);
    procedure FormCreate(Sender: TObject);
    procedure GridItemsiCustomDrawCell(Sender: TObject; ACanvas: TCanvas;
      ARect: TRect; ANode: TdxTreeListNode; AColumn: TdxTreeListColumn;
      ASelected, AFocused, ANewItemRow: Boolean; var AText: String;
      var AColor: TColor; AFont: TFont; var AAlignment: TAlignment;
      var ADone: Boolean);
    procedure QryItemsiBeforeDelete(DataSet: TDataSet);
    procedure QryItemsiAfterPost(DataSet: TDataSet);
    procedure GridItemsiKeyPress(Sender: TObject; var Key: Char);
    procedure FormDestroy(Sender: TObject);
    procedure edNumarDocKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure QryItemsiBeforePost(DataSet: TDataSet);
    procedure GridItemsiCustomDrawColumnHeader(Sender: TObject;
      AColumn: TdxTreeListColumn; ACanvas: TCanvas; ARect: TRect;
      var AText: String; var AColor: TColor; AFont: TFont;
      var AAlignment: TAlignment; var ASorted: TdxTreeListColumnSort;
      var ADone: Boolean);
    procedure BtnDetaliiDocumClick(Sender: TObject);
    procedure edPredatorButtonClick(Sender: TObject);
    procedure GridItemsiDETALII_ANGAJAMENTInitPopup(Sender: TObject);
    procedure GridItemsiDETALII_ANGAJAMENTCloseUp(Sender: TObject;
      var Text: String; var Accept: Boolean);
    procedure QryDocumentAfterPost(DataSet: TDataSet);
    procedure EvaluatorDocumEvaluate(Sender: TObject; Eval: String;
      Args: array of Variant; ArgCount: Integer; var Value: Variant;
      var Done: Boolean);
    procedure pnClientResize(Sender: TObject);
    procedure edDelegatPopupInitPopup(Sender: TObject);
    procedure edDelegatValidate(Sender: TObject; var AKeyValue: Variant);
    procedure edDelegatButtonClick(Sender: TObject);
    procedure edDelegatExit(Sender: TObject);
    procedure edMijTransportButtonClick(Sender: TObject);
    procedure edMijTransportValidate(Sender: TObject;
      var AKeyValue: Variant);
    procedure edMijTransportExit(Sender: TObject);
    procedure GridItemsiKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure btnValidareConexClick(Sender: TObject);
    procedure TreeTipMaterialDblClick(Sender: TObject);
    procedure TreeTipMaterialKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure GridItemsiID_GEST_TIP_MATERIALGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure GridItemsiID_GEST_TIP_MATERIALPopup(Sender: TObject;
      const EditText: String);
    procedure GridItemsiID_GEST_TIP_MATERIALCloseUp(Sender: TObject;
      var Text: String; var Accept: Boolean);
    procedure TreeTipMaterialCustomDrawCell(Sender: TObject;
      ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
      AColumn: TdxTreeListColumn; ASelected, AFocused,
      ANewItemRow: Boolean; var AText: String; var AColor: TColor;
      AFont: TFont; var AAlignment: TAlignment; var ADone: Boolean);
    procedure Cmd_DelDocExecute(Sender: TObject);
    procedure Cmd_ModifyDocExecute(Sender: TObject);
    procedure Cmd_CopyPozExecute(Sender: TObject);
    procedure Cmd_StocPerioadaExecute(Sender: TObject);
    procedure FormKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure FormCloseQuery(Sender: TObject; var CanClose: Boolean);
    procedure edNumarDocButtonClick(Sender: TObject;
      AbsoluteIndex: Integer);
    procedure GridItemsiSelectedCountChange(Sender: TObject);
    procedure GridItemsiID_GEST_TIP_MATERIALValidate(Sender: TObject;
      var ErrorText: String; var Accept: Boolean);
    procedure GridItemsiNR_CHITANTAButtonClick(Sender: TObject;
      AbsoluteIndex: Integer);
    procedure pnBottomResize(Sender: TObject);
    procedure TreeRepartitoriDblClick(Sender: TObject);
    procedure Schimbasemnpozitiecurenta1Click(Sender: TObject);
    procedure CmdAdaugareGridExecute(Sender: TObject);
    procedure CmdStegereGridExecute(Sender: TObject);
    procedure btnDMClick(Sender: TObject);
    procedure TreePlanCustomDrawCell(Sender: TObject; ACanvas: TCanvas;
      ARect: TRect; ANode: TdxTreeListNode; AColumn: TdxTreeListColumn;
      ASelected, AFocused, ANewItemRow: Boolean; var AText: String;
      var AColor: TColor; AFont: TFont; var AAlignment: TAlignment;
      var ADone: Boolean);
    procedure TreePlanKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure TreePlanDblClick(Sender: TObject);
    procedure GridItemsiContDPopup(Sender: TObject;
      const EditText: String);
    procedure GridItemsiContDValidate(Sender: TObject;
      var ErrorText: String; var Accept: Boolean);
    procedure TreePlanROMANAGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure GridItemsiContDCloseUp(Sender: TObject; var Text: String;
      var Accept: Boolean);
    procedure GridItemsiCategorieGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure GridItemsiCategoriePopup(Sender: TObject;
      const EditText: String);
    procedure GridItemsiCategorieCloseUp(Sender: TObject; var Text: String; var Accept: Boolean);
    procedure pageControlChange(Sender: TObject);
    procedure btnGenereazaNoteClick(Sender: TObject);
    procedure GridItemsiChangeNode(Sender: TObject; OldNode,
      Node: TdxTreeListNode);
    procedure chkGenAutomatPropertiesChange(Sender: TObject);
    procedure TreeRepKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure TreeRepDblClick(Sender: TObject);
    procedure CmdCopyContinutExecute(Sender: TObject);
    procedure CmdGenereazaNumereExecute(Sender: TObject);
    procedure GridItemsiCategorieValidate(Sender: TObject;
      var ErrorText: String; var Accept: Boolean);
    procedure btnBonFiscalClick(Sender: TObject);
    procedure Cmd_SpargePeProcenteExecute(Sender: TObject);
    procedure QryItemsiAfterDelete(DataSet: TDataSet);
    procedure GridItemsiDETALII_ANGAJAMENTCloseQuery(Sender: TObject;
      var CanClose: Boolean);
    procedure edTipDocumentEnter(Sender: TObject);
    procedure edTipDocumentPropertiesCloseUp(Sender: TObject);
    procedure edTipDocumentPropertiesEditValueChanged(Sender: TObject);
    procedure edTipDocumentPropertiesInitPopup(Sender: TObject);
    procedure edMijTransportPopupCloseUp(Sender: TObject);
    procedure edDelegatPopupCloseUp(Sender: TObject);
    procedure edPredatorValidate(Sender: TObject; var AKeyValue: Variant);
    procedure edPredatorPopupInitPopup(Sender: TObject);
    procedure TreeRepartitoriCustomDrawDataCell(Sender: TcxCustomTreeList;
      ACanvas: TcxCanvas; AViewInfo: TcxTreeListEditCellViewInfo;
      var ADone: Boolean);
    procedure cxDateEdit1PropertiesValidate(Sender: TObject;
      var DisplayValue: Variant; var ErrorText: TCaption; var Error: Boolean);
    procedure GridItemsiRepCCloseUp(Sender: TObject; var Text: string;
      var Accept: Boolean);
    procedure GridItemsiRepDInitPopup(Sender: TObject);
    procedure GridItemsiRepCGetText(Sender: TObject; ANode: TdxTreeListNode;
      var AText: string);
    procedure TreeRepartitoriKeyUp(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure edNumarDocPropertiesValidate(Sender: TObject;
      var DisplayValue: Variant; var ErrorText: TCaption; var Error: Boolean);
  private
    FSumator              : TDBSumList;
    FEvaluatorDocum       : TATSEvaluator;
    FEvaluator            : TATSEvaluator;
    FExistTratareServer   : Boolean;
    FIdParinte            : Integer;
    FTipDocument          : Variant;
    FFieldDocumList       : TStringList;
    IsInLoading           : Boolean;
    FComplementare        : Boolean;
    FNrConfirmari         : Integer;
    FGestPredator,
    FGestPrimitor         : Integer;
    FIdDefaDocum          : Integer;
    FTipDocConex          : Integer;
    FTipDescarcare        : Integer;
    FPrimitor             : TGestiune;
    FPredator             : TGestiune;
    FIdDocument           : Integer;
    FTipuriMatList        : TStringList;
    FDenTipMatrList       : TStringList;
    FDetaliiDocum         : TFrmDetaliiDocum;
    FDefalcareBuget       : TfrmAlopDisponibil;
    ForcePrint            : Boolean;
    FDelegat              : Integer;
    FMijlocTransport      : Integer;
    FDefaultCodFunctional : String;
    FDefaultUnitate       : Integer;
    FDefaultCodEconomic   : String;
    IsInLoad              : Boolean;
    memIntern             : TAtsMemData;
    memExtern             : TAtsMemData;
    memTot                : TAtsMemData;
    procedure RefreshFirstDocum;
    procedure UpdateCodEconomic();
    procedure WmSendPostItems(var Message: TMessage); message WM_SENDPOSTITEMS;

    function GetInternalTipMat(AIdGestTipMat: String): String;
    function GetInternalNumeCategorie(aIdCategorie:String) : String;

    procedure SetTipDocument(const Value: Variant);
    procedure SetPredator(const Value: Integer);
    procedure SetPrimitor(const Value: Integer);
    procedure SetBooleanField(DataSet: TDataSet);
    procedure SetNextControl;

    procedure CANTLVRValidate(Sender: TField);
    procedure ValoareReceptieChange(Sender: TField);
    procedure DoDenMatChange(Sender: TField);
    procedure DoCodBaraChange(Sender: TField);

    procedure SetDocumentField(AFieldName: String; AValue: Variant);

    procedure ActivateGrid;
    procedure ActivateDMCon(aQry : TDataSet);
    function  GetNextDocNumber(AIdDefDoc: Integer; APrefix: String; AStart, AEnd: Integer): String;
    function  ExistsCodMat(ACodMat: Integer): Boolean;
    function  ShowStock(ANode: TdxTreeListNode): Boolean; overload;
    function  ShowStock(ATipMat: String=''): Boolean; overload;
    function  GetTipMatFromDenMat(const ADenMat: String): String;
    function  GetCodMatCount(TipMat : String; var aCodMat: Integer; var aDenMat : String): Integer;
    function  GetTipuriStocuri(ADefaDocum: Integer): Integer;
    procedure GetTipuriMateriale(ADefaDocum : Integer);
    function  SelectFromNomenclator(const ATipMat: String) : Boolean;

    procedure SetIdDocument(const Value: Integer);
    procedure DisableEditor(aProdus : String);
    { Private declarations }

    procedure SetNewDocNumber;
    function  GetStockCodMat(ACodMat: Integer): Double;
    procedure SaveCurentCodMat(ADataSet: TDataSet; const AStockBefore, AStockField: String; ForceEdit: Boolean = False; const ForceAppend : Boolean = False);
    procedure CopyFromStock(ADataSet: TdxMemData; const AStockBefore, AStockField: String;const ForceAppend : Boolean = False);
    procedure InitTipuriMateriale;
    procedure InitCategorii;
    procedure InitReadOnlyFields;
    function  IsNewCodMat: Boolean;
    function  IsExistCodMat: Boolean;
    function  AcceptStockNegativ: Boolean;
    procedure TestItems;

    function GetRigthsFromDocum(AField: String): Integer;

    procedure ShowDetaliiDocument();
    procedure HideDetaliiDocument();
    procedure TiparireDocument();
    procedure SetNumarZerouri(NrZeroruri: Integer; AColumn: TdxDBTreeListColumn=nil);

    procedure SetMRUValue(ACol: TdxDBGridMRUColumn; AValue: String);
    procedure InternalFilterDocument(DataSet: TDataSet; var Accept: Boolean);

    procedure SetRepartitorFilter(AFilter: String);
    procedure SetDelegatiFilter(AFilter: String);
    procedure SetMijlocTransportFilter(AFilter: String);

    procedure SetDelegat(const Value: Integer);
    procedure SetMijlocTransport(const Value: Integer);
    procedure AutoKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure DelegatKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure InternalValidateExact(Val: String; Tree: TdxDbTreeList;
      var InternalValue: Variant; const ForceById : Word = 0; const lTag : Integer = -1);
    procedure InternalValidateCont(Val: String; Tree: TdxDbTreeList;
      var InternalValue: Variant);

    procedure ClearListStockInfo;
    procedure TestIdDocument;
  private
  protected
    procedure TestModificare(const AOperatie: String; const AFieldName: String; const AValue: Variant); overload;
    procedure TestModificare(const AOperatie: String); overload;

    //old not used
    function  TestUniqueNumber(aIdCulgestDocum : Integer;const OnlyWarning : Boolean = True) : Boolean;
    function  ShowStockOld(ATipMat: String=''): Boolean;
    function  GetIDTipStock(ADefaDocum: Integer): Integer;
    procedure PopulateMemRep;
    procedure GetCodMatList(AList: TStringList);
  public
    constructor Create(AOwner: TComponent); override;
  public
    NotifyForm        : TCustomForm;
    IsSQLFiltered     : Boolean;
    FIdGestDocumConex : Integer;
    FSumatorFieldList : TStringList;
    IsInSaveDoc       : Boolean;
    FTipDocTethys     : String;
    procedure PrintBonFiscal;
    procedure ValidareDocument(Tiparire : Boolean = False);
    procedure PopulateSumatorFieldList;
    procedure CopyDocToCulegere(const AIdDocument: Integer);
    procedure AppendDocToCulegere(const AIdDocument: Integer; IsAppend : Boolean);
    procedure AppendStocPerioada(const AIdDocument : Integer; DataStoc : TDateTime; Predator : Integer; Cont : String; IsAppend : Boolean);
    procedure ReadDocument;
    function  NewDocument: Integer;
    procedure UpdateSumColumns;
    procedure WriteToDb;
    procedure TestAndSwitchCantitate;


    property  TipDocument: Variant read FTipDocument write SetTipDocument;
    property  Predator   : TGestiune read FPredator write FPredator;
    property  Primitor   : TGestiune read FPrimitor write FPrimitor;
    property  IdDocument : Integer read FIdDocument write SetIdDocument;
    property  DocIdParinte: Integer read FIdParinte write FIdParinte;
    property  Delegat : Integer read FDelegat write SetDelegat;
    property  MijlocTransport : Integer read FMijlocTransport write SetMijlocTransport;
    { Public declarations }
  end;

procedure PrintDocument2DB(APrintToScreen: Boolean = False; ARaiseError: Boolean = True);
procedure PrintDocument2DBFR(APrintToScreen: Boolean = False; ARaiseError: Boolean = True);
procedure WriteDocumentFizic2DB(Sender: TObject; AStream: TStream);

implementation

{$R *.DFM}
uses
  ZeosDBUtile,
  dxCompsUtile,
  ConcurentUsersUnit,
  frxZLib,
  DBGridAutoEnter,
  SelStockUnit,
  dxFilter,
  ATSZDBUtils,
  Variants,
  ReconciliereDocumUnit,
  CommonDBVar,
  DateUnit,
  MainUnit,
  StrUtils,
  UnitStocPerioada,
  Gest_ModifyDocum,
  ImportTethys,
  frxADOComponents,
  frxCustomDB,
  RapInclude,
  UnitAddSerii,
  OERepartitoriUnit;

{ TfrmcxTcv }
var
  lFRStackReports : TStringList;

const
  IsChacedReprintDocum: Boolean = False;
  IsPrintOnScreen : Boolean = True;

procedure ClearFRReportStack;
var I: Integer;
begin
  for I := 0 to lFRStackReports.Count-1 do
    TfrxReport(lFRStackReports.Objects[I]).Free;
  lFRStackReports.Clear;
end;

procedure PrintDocument2DB(APrintToScreen: Boolean = False; ARaiseError: Boolean = True);
begin
  if rapInclude.IsFastReport then
    PrintDocument2DBFR(APrintToScreen, ARaiseError);
end;

procedure PrintDocument2DBFR(APrintToScreen: Boolean = False; ARaiseError: Boolean = True);
var
  lCurentRep  : TfrxReport;
  lParam      : TfrxParamItem;
  lRepId      : Integer;
  I           : Integer;
  lMemStream  : TMemoryStream;

  function GetReport: TfrxReport;
  var
    lIndex: Integer;
  begin
    if not IsChacedReprintDocum then
      Result := mainForm.FRrapExplorer.Explorer.LoadReport(lRepId, False)
    else begin
      lIndex := lFRStackReports.IndexOf(IntToStr(lRepId));
      if lIndex = -1 then begin
        lIndex := lFRStackReports.AddObject(IntToStr(lRepId), mainForm.FRrapExplorer.Explorer.LoadReport(lRepId, False));
        Result := TfrxReport(lFRStackReports.Objects[lIndex]);
      end
      else begin
        Result := TfrxReport(lFRStackReports.Objects[lIndex]);
        if (Result = nil) then Result := mainForm.FRrapExplorer.Explorer.LoadReport(lRepId, False);
      end;
    end;
    Result.PreviewOptions.Modal := False;
  end;

begin
  lRepId := ValueSafeToInt( DBGetScallarFmt('exec [spGestReportFromDocum] %d', [DateUnit.IdGestDocum]) );
  if lRepId <> -1  then begin
    lCurentRep := GetReport;
    for I := 0 to lCurentRep.DataSets.Count - 1 do begin
       lParam := TfrxADOQuery(lCurentRep.DataSets.Items[I].DataSet).Params.Find('ID_GEST_DOCUM');
       if Assigned(lParam) then
          lParam.Value := DateUnit.IdGestDocum;
    end;
    lCurentRep.OnRunDialogs := 'Test';
    lMemStream := TMemoryStream.Create;
    lCurentRep.PrepareReport(True);
    lCurentRep.PreviewPages.SaveToStream(lMemStream);
    WriteDocumentFizic2DB(nil, lMemStream);
    lMemStream.Free;
    if APrintToScreen then begin
      lCurentRep.ShowPreparedReport;
      MainForm.TabReport(lCurentRep);
    end;
  end;
end;

procedure TfrmcxTcv.SetMRUValue(ACol: TdxDBGridMRUColumn; AValue: String);
begin
  if ACol.Items.IndexOf(AValue) = -1 then
     ACol.Items.Add(AValue);
end;

procedure TfrmcxTcv.ReadDocument;
begin
  IdDocument := ValueSafeToInt( DBGetScallarFmt('exec [spGestListaDocumente] %d', [IdUtilizator], 0), -1);
  qryNoteDoc.Close;
end;

procedure TfrmcxTcv.BtnCancelClick(Sender: TObject);
begin
  Close;
end;

procedure TfrmcxTcv.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  if Assigned(NotifyForm) then
     TfrmGEST_ModifyDocum(NotifyForm).SetToItemsi;
  FEvaluatorDocum.Active := False;
  FEvaluator.Active := False;
  Action := caFree;
end;

procedure TfrmcxTcv.pnDocumentResize(Sender: TObject);
var
  lNumarLeft : Integer;
begin
  lNumarLeft          := (pnDocument.Width - edNumarDoc.Width - 20 - edDataDoc.Width) div 2;
  edNumarDoc.Left     := lNumarLeft;
  lbDocument.Left     := lNumarLeft + (edNumarDoc.Width - lbDocument.Width) div 2;
  edDataDoc.Left      := lNumarLeft + edNumarDoc.Width + 20;
  lbDataDoc.Left      := edDataDoc.Left + (edDataDoc.Width - lbDataDoc.Width) div 2;
  edPredator.Width := edNumarDoc.Left - edPredator.Left - 10;
  edPrimitor.Left  := edDataDoc.Left + edDataDoc.Width + 10;
  edPrimitor.Width := pnDocument.Width - edPrimitor.Left - 10;
  lbTipDocument.Left  := (pnDocument.Width - lbTipDocument.Width - 10 - edTipDocument.Width) div 2;
  edTipDocument.Left  := lbTipDocument.Left + lbTipDocument.Width + 10;
  btnCopyStocPred.Left  := pnDocument.Width - btnCopyStocPred.Width - 10;
  btnCopyPositions.Left := btnCopyStocPred.Left - btnCopyPositions.Width - 5;
  BtnModificaDoc.Left   := btnCopyPositions.Left - BtnModificaDoc.Width - 5;
  BtnDelDocument.Left   := BtnModificaDoc.Left - BtnDelDocument.Width - 5;
end;

procedure TfrmcxTcv.SetTipDocument(const Value: Variant);
begin
  FTipDocument := Value;
  FPredator.FCodRep := 0;
  FPrimitor.FCodRep := 0;
  FPredator.Assigned := False;
  FPrimitor.Assigned := False;

  if FTipDocument = 0 then Exit;

  if FrmData.QryDocumente.FieldByName('ID_GEST_TIP_DOCUM').IsNull then
    ShowMessage('ID_GEST_TIP_DOCUM este NULL!');

  if FrmData.QryDocumente.FieldByName('ID_GEST_TIP_DOCUM').AsInteger <> Value then
  begin
    ShowMessage('ID_GEST_TIP_DOCUM: ' + IntToStr(FrmData.QryDocumente.FieldByName('ID_GEST_TIP_DOCUM').AsInteger));

    if not FrmData.QryDocumente.Locate('ID_GEST_TIP_DOCUM', Value, []) then
      ShowEroare('Nu se poate gasi descrierea documentului !');
  end;

  if FrmData.QryDocumente.FieldByName('TIP_PREDATOR').IsNull then
    ShowMessage('TIP_PREDATOR este NULL!')
  else
    FGestPredator := FrmData.QryDocumente.FieldByName('TIP_PREDATOR').AsInteger;

  if FrmData.QryDocumente.FieldByName('TIP_PRIMITOR').IsNull then
    ShowMessage('TIP_PRIMITOR este NULL!')
  else
    FGestPrimitor := FrmData.QryDocumente.FieldByName('TIP_PRIMITOR').AsInteger;

  if FrmData.QryDocumente.FieldByName('COMPLEMENTEAZA_GEST').IsNull then
    ShowMessage('COMPLEMENTEAZA_GEST este NULL!')
  else
    FComplementare := FrmData.QryDocumente.FieldByName('COMPLEMENTEAZA_GEST').AsBoolean;

  if not IsInLoading then
  begin
    SetDocumentField('ID_GEST_TIP_DOCUM', FrmData.QryDocumente['ID_GEST_TIP_DOCUM']);
    SetDocumentField('COD_DOCUM', FrmData.QryDocumente['COD_DOCUM']);
  end;

  if ((FGestPredator and 01 = 01) and (not FPredator.GestInt)) or
     ((FGestPredator and 02 = 02) and (FPredator.GestInt)) then
  begin
    SetPredator(-1);
    edPredator.Text := '';
  end;

  if ((FGestPrimitor and 01 = 01) and (not FPrimitor.GestInt)) or
     ((FGestPrimitor and 02 = 02) and (FPrimitor.GestInt)) then
  begin
    SetPrimitor(-1);
    edPrimitor.Text := '';
  end;

  ActivateGrid;
  SetNextControl;
end;


procedure TfrmcxTcv.edPredatorValidate(Sender: TObject; var AKeyValue: Variant);
begin
  TRepartitorPanel(Sender).Tag := AKeyValue;
  if Sender = edPredator then SetPredator(TRepartitorPanel(Sender).Tag)
  else SetPrimitor(TRepartitorPanel(Sender).Tag);
  ClearListStockInfo;
  FreeAndNil(ListStockInfo);
end;

procedure TfrmcxTcv.SetPredator(const Value: Integer);
var
  lNode: TcxDBTreeListNode;
begin
  FPredator.FCodRep   := Value;
  FPredator.Assigned  := Value > 0;
  if FPredator.Assigned then begin
     lNode := TreeRepartitori.FindNodeByKeyValue(Value);
     if Assigned(lNode) then begin
        FPredator.GestInt := GetBoolean(lNode.Values[TreeRepartitoriGESTINT.ItemIndex]);
        edPredator.Text   := lNode.Values[TreeRepartitoriNUME.ItemIndex];
     end
     else if FrmData.QryRepartitori.Locate('ID_REPARTITORI', Value, []) then begin
             FPredator.GestInt := FrmData.QryRepartitori.FieldByName('GESTINT').AsBoolean;
             edPredator.Text   := FrmData.QryRepartitori.FieldByName('NUME').AsString;
     end;
     SetDocumentField('ID_PREDATOR', FPredator.FCodRep);
     SetDocumentField('PREDATOR_INTERN', FPredator.GestInt);
     SetDocumentField('PREDATOR', edPredator.Text);
     ActivateGrid;
     SetNextControl;
  end
  else begin
    ActivateGrid;
  end;
end;

procedure TfrmcxTcv.SetPrimitor(const Value: Integer);
var lNode: TcxDBTreeListNode;
begin
  FPrimitor.FCodRep := Value;
  FPrimitor.Assigned := Value > 0;
  { Citim si Tipul Gestiunii }
  if FPrimitor.Assigned then begin
     lNode := TreeRepartitori.FindNodeByKeyValue(Value);
     if Assigned(lNode) then begin
        FPrimitor.GestInt :=  GetBoolean(lNode.Values[TreeRepartitoriGESTINT.ItemIndex]);
        edPrimitor.Text   := lNode.Values[TreeRepartitoriNUME.ItemIndex];
     end
     else if FrmData.QryRepartitori.Locate('ID_REPARTITORI', Value, []) then begin
             FPrimitor.GestInt := FrmData.QryRepartitori.FieldByName('GESTINT').AsBoolean;
             edPrimitor.Text   := FrmData.QryRepartitori.FieldByName('NUME').AsString;
     end;
     SetDocumentField('ID_PRIMITOR', FPrimitor.FCodRep);
     SetDocumentField('PRIMITOR_INTERN', FPrimitor.GestInt);
     SetDocumentField('PRIMITOR', edPrimitor.Text);
     ActivateGrid;
     SetNextControl;
  end
end;

type
  TdxDBGridImageColumnAccess = class(TdxDBGridImageColumn);

procedure TfrmcxTcv.ActivateGrid;
var
  lColumn    : TdxDBTreeListColumn;
  lDataSet   : TDataSet;
  lSortColumn: TStringList;
  lField     : TField;
  lFieldName,
  lFormula,
  lFormat   : String;

  procedure TryToFitColumns;
  var I, TotalWidth: Integer;
   begin
     GridItemsi.ApplyBestFit(nil);
     TotalWidth := GridItemsi.Bands[0].Width;
     for I := 0 to GridItemsi.ColumnCount-1 do
       if (GridItemsi.Columns[I].BandIndex > 0) and
          (GridItemsi.Columns[I].Visible) then TotalWidth := TotalWidth + GridItemsi.Columns[I].Width;
     if GridItemsi.Width > (TotalWidth - 250) then GridItemsi.OptionsView := GridItemsi.OptionsView + [edgoAutoWidth]
     else GridItemsi.OptionsView := GridItemsi.OptionsView - [edgoAutoWidth];
   end;

  procedure HideGridColumns;
  var
    I: Integer;
  begin
    for I := 0 to GridItemsi.ColumnCount -1 do
      if GridItemsi.Columns[I].Tag = -1 then
        GridItemsi.Columns[I].Visible := False;
  end;


var
  qryDefaDoc  : TDataSet;
  J           : Integer;

begin

  FEvaluatorDocum.Active := False;
  FEvaluator.Active := False;
  FEvaluatorDocum.Active := True;
  FEvaluator.Active := True;

  QryItemsi.OnNewRecord := QryItemsiNewRecord;

  lFormat := DupeString('0', ZeroruriCulegere);
  lFormat := ',0.'+lFormat+';-,0.'+lFormat;
  btnCopyPositions.Enabled := (IdDocument <> -1);
  FEvaluator.ClearFields;
  FEvaluatorDocum.ClearFields;
  FFieldDocumList.Clear;
  FSumator.SumCollection.Clear;

  if IsInLoading then Exit;
  FIdDefaDocum := -1;
  BtnDetaliiDocum.Enabled := False;
  qryDefaDoc := frmData.QryDefaDoc;
  GridItemsi.Enabled := (FIdDocument > 0) and (FTipDocument > 0) and (FPredator.FCodRep > 0) and (FPrimitor.FCodRep > 0)
                         and qryDefaDoc.Locate('ID_GEST_TIP_DOCUM;PREDATOR_INTERN;PRIMITOR_INTERN',
                                VarArrayOf([FTipDocument, FPredator.GestInt, FPrimitor.GestInt]), []);

  if GridItemsi.Enabled then begin
    FIdDefaDocum := qryDefaDoc['ID_GEST_DEFA_DOCUM'];
    SetDocumentField('ID_GEST_DEFA_DOCUM', FIdDefaDocum);
    if FDetaliiDocum.Visible then begin
      if FDetaliiDocum.Inspector.DataController.DataSource = nil then
        FDetaliiDocum.Inspector.DataController.DataSource := DTDocument;
      FDetaliiDocum.IdDefaDocum := FIdDefaDocum;
    end;
    BtnDetaliiDocum.Enabled := True;
    GridItemsi.Color        := clWindow;
    GridItemsi.BeginUpdate;
    lSortColumn := TStringList.Create;
    try
      btnValidareConex.Enabled := ValueHasValue(qryDefaDoc['ID_DOCUMENT_CONEX']);
      ActivateDMCon(qryDefaDoc);
      if DBProcExists('spGestCodFunctionOnDocum') then begin
        lDataSet := DBNewQueryFmt('exec [spGestCodFunctionOnDocum] %s, %s, %s, %s',
          [ValueToStr(FPredator.FCodRep), ValueToStr(FPrimitor.FCodRep), ValueToStr(FIdDefaDocum), ValueToStr(FIdDocument)]);
        try
          lDataSet.Open;
          if not lDataSet.IsEmpty then begin
            FDefaultCodFunctional := ValueToStr(lDataSet.Fields[0].Value);
            FDefaultUnitate       := ValueSafeToInt(lDataSet.Fields[1].Value, 0);
          end;
        finally
          lDataSet.Free;
        end;
        if ValueIsTrue(qryDefaDoc['NUMAR_AUTOMAT']) then begin
          edNumarDoc.EditValue := GetNextDocNumber(FIdDefaDocum, qryDefaDoc['NUMAR_PREFIX'], qryDefaDoc['NUMAR_START'], qryDefaDoc['NUMAR_END']);
          SetDocumentField('NR_DOCUM', edNumarDoc.EditValue);
        end;
      end;
      InitTipuriMateriale;
      InitCategorii;
      InitReadOnlyFields;
      HideGridColumns;
      lDataSet := DBNewQueryFmt('exec [spGestCulegereDocumItemsi] %s, %s', [ValueToStr(FIdDefaDocum), ValueToStr(IdUtilizator)]);
      try
        lDataSet.Open;
        while not lDataSet.Eof do begin
          lColumn := GridItemsi.FindColumnByFieldName(lDataSet['FIELD_NAME']);
          { Numai pentru cele din banda de informatii ... primele 2 campuri raman mereu afisate }
          if not Assigned(lColumn) and (lDataSet.FindField('autoCreate') <> nil) and ValueIsTrue(lDataSet['autoCreate']) then begin
            lColumn := GridItemsi.CreateColumn(TdxDBGridMaskColumn);
            lColumn.HeaderAlignment := taCenter;
            lColumn.Width           := 164;
            lColumn.Visible         := False;
            lColumn.BandIndex       := 1;
          end;
          if Assigned(lColumn) then begin
            lSortColumn.AddObject(Format('%10d', [lDataSet.FieldByName('POS').AsInteger]), lColumn);
            lColumn.Caption   := lDataSet.FieldByName('CAPTION').AsString;
            if ((lColumn.BandIndex >= 0) or (lColumn.Tag = -1)) and ValueHasValue(lDataSet['VISIBLE']) then
              lColumn.Visible := ValueIsTrue(lDataSet['VISIBLE']);
            if ValueHasValue(lDataSet['READONLY']) then begin
              lColumn.ReadOnly := ValueIsTrue(lDataSet['READONLY']);
              lColumn.DisableEditor := lColumn.ReadOnly;
            end;
            lColumn.Tag := Integer((((lColumn.Tag = 1) and (lColumn.BandIndex = 0)) and (lColumn.Visible)) or ValueIsTrue(lDataSet['REQUIRED']));
            if ValueHasValue(lDataSet['FONT_NAME']) then
              lColumn.Font.Name := ValueSafeToStr(lDataSet['FONT_NAME'], lColumn.Font.Name);
            if ValueHasValue(lDataSet['COLOR']) then
              lColumn.Color := TColor(lDataSet.FieldByName('COLOR').AsInteger);
            if ValueHasValue(lDataSet['FONT_COLOR']) then
              lColumn.Font.Color := TColor(lDataSet.FieldByName('FONT_COLOR').AsInteger);
            if ValueHasValue(lDataSet['FONT_SIZE']) then
              lColumn.Font.Color := TColor(lDataSet.FieldByName('FONT_SIZE').AsInteger);
            if ValueHasValue(lDataSet['EDIT_MASK']) then
              if lColumn is TdxDBGridCurrencyColumn then
                TdxDBGridCurrencyColumn(lColumn).DisplayFormat := ValueSafeToStr(lDataSet['EDIT_MASK'], TdxDBGridCurrencyColumn(lColumn).DisplayFormat)
              else
                if lColumn is TdxDBGridMaskColumn then
                  TdxDBGridMaskColumn(lColumn).EditMask := ValueSafeToStr(lDataSet['EDIT_MASK'], TdxDBGridMaskColumn(lColumn).EditMask);
            if ValueIsTrue(lDataSet['SUM_TOTAL']) then begin
              GridItemsi.ShowSummaryFooter := True;
              TdxDBGridImageColumnAccess(lColumn).SummaryFormat := lFormat;
              lColumn.SummaryFooterType := cstSum;
            end
            else
              lColumn.SummaryFooterType := cstNone;
          end;
          if ValueHasValue(lDataSet['FORMULA_CALCUL']) then
            FEvaluator.AddFormula(QryItemsi, lDataSet.FieldByName('FIELD_NAME').AsString, lDataSet.FieldByName('FORMULA_CALCUL').AsString);
          if lColumn is TdxDBGridImageColumn then
            TdxDBGridImageColumnAccess(lColumn).ImmediateDropDown := True;
          lDataSet.Next;
        end;
      finally
        lDataSet.Free;
      end;

      lDataSet := DBNewQueryFmt('SELECT * FROM GEST_DEFA_DOCUM_DOCUMENT WHERE ID_GEST_DEFA_DOCUM = %s', [ValueToStr(FIdDefaDocum)]);
      try
        lDataSet.Open;
        while not lDataSet.Eof do begin
          lFormula := ValueSafeToStr(lDataSet['FORMULA_CALCUL']);
          if lFormula > '' then begin
          if pos('SUM', lFormula) = 1 then begin
            lFieldName := StringReplace(lFormula, 'SUM', '', [rfReplaceAll, rfIgnoreCase]);
            lFieldName := StringReplace(lFieldName, 'ROUND', '', [rfReplaceAll, rfIgnoreCase]);
            lFieldName := StringReplace(lFieldName, '(', '', [rfReplaceAll, rfIgnoreCase]);
            lFieldName := StringReplace(lFieldName, ')', '', [rfReplaceAll, rfIgnoreCase]);
            lField     := QryItemsi.FindField(lFieldName);
            if Assigned(lField) then begin
              with TDBSum(FSumator.SumCollection.Add) do begin
                FieldName := lFieldName;
                GroupOperation := goSum;
              end;
              FFieldDocumList.AddObject(UpperCase(lField.FieldName), FEvaluatorDocum.AddFormula(QryDocument, lDataSet.FieldByName('FIELD_NAME').AsString, lFormula));
            end;
          end
          else
            FEvaluatorDocum.AddFormula(QryDocument, lDataSet.FieldByName('FIELD_NAME').AsString, lFormula);

          end;
          lDataSet.Next;
        end;
        FSumator.RecalcAll;
        { Mereu punem angajamentul pe ReadOnly }
        GridItemsiDETALII_ANGAJAMENT.ReadOnly := True;
        GridItemsiCategorie.ReadOnly := True;
        { Ascundem coloanele care nu au fost explicit specificate }
        lSortColumn.Sorted := True;
        for J := 0 to GridItemsi.ColumnCount - 1 do
          if (GridItemsi.Columns[J].BandIndex > 0) and (lSortColumn.IndexOfObject(GridItemsi.Columns[J]) = -1) then
            GridItemsi.Columns[J].Visible := False;
        { Sortam corect coloanele }
        for J := 0 to lSortColumn.Count-1 do
          with TdxDBTreeListColumn(lSortColumn.Objects[J]) do
            ColIndex := J;
        TryToFitColumns;
        SetNumarZerouri(ZeroruriCulegere);
      finally
        lDataSet.Free;
      end;
    finally
      lSortColumn.Free;
      GridItemsi.EndUpdate;
    end;
  end
  else
    GridItemsi.Color := clBtnFace;
  PopulateSumatorFieldList;
end;

procedure TfrmcxTcv.edDataDocValidate(Sender: TObject; var ErrorText: String;
  var Accept: Boolean);
begin
  if not IsInLoading and IsValidDate(edDataDoc.EditValue) then begin
    SetDocumentField('DATA_DOCUM', edDataDoc.Date);
    SetNextControl;
  end;
end;

procedure TfrmcxTcv.QryItemsiNewRecord(DataSet: TDataSet);
var
  oldLoading : Boolean;
begin
  TestModificare('Adaugare Pozitie Document');
  oldLoading := IsInLoading;
  if not oldLoading then
     IsInLoading := True;
  try
    try
      SetBooleanField(DataSet);
      DataSet.FieldByName('ID_CULGEST_DOCUM').AsInteger := FIdDocument;
      if FDefaultCodFunctional <> '' then
        DataSet.FieldByName('COD_FUNCTIONAL').AsString := FDefaultCodFunctional;
      if FDefaultUnitate <> 0 then
        DataSet.FieldByName('ID_OI_UNITATI').AsInteger := FDefaultUnitate;
      if FDefaultCodEconomic <> '' then
        DataSet.FieldByName('COD_ECONOMIC').AsString := FDefaultCodEconomic;

      if (DataSet.FieldByName('COD_FUNCTIONAL').AsString <> '') and
         (DataSet.FieldByName('COD_ECONOMIC').AsString <> '') then begin
           FDefalcareBuget.CodEconomic     := DataSet.FieldByName('COD_ECONOMIC').AsString;
           FDefalcareBuget.CodFunctional   := DataSet.FieldByName('COD_FUNCTIONAL').AsString;

        if DataSet.FieldByName('ID_ANGAJAMENTE_DEFALCARE').AsInteger > 0 then begin
           FDefalcareBuget.IdAngajament := DataSet.FieldByName('ID_ANGAJAMENTE_DEFALCARE').AsInteger
        end
        else begin
           FDefalcareBuget.IdFurnizor := FPredator.FCodRep;
        end;
        DataSet.FieldByName('DETALII_ANGAJAMENT').AsString := FDefalcareBuget.Descriere;
      end;

    except
      on E: Exception do
         ShowEroare('EROARE : '+E.Message);
    end;
  finally
    if not oldLoading then
       IsInLoading := False;
  end;
end;

procedure TfrmcxTcv.WriteToDb;
begin
  DBPost([QryDocument, QryItemsi]);

  QryItemsi.DisableControls;
  GridItemsi.BeginUpdate;
  FDetaliiDocum.Inspector.BeginUpdate;
  SaveProgress.Visible := True;
  try
    IsInSaveDoc := True;
    SaveProgress.Min := 0;
    if QryItemsi.RecordCount > 0 then SaveProgress.Max := QryItemsi.RecordCount
    else SaveProgress.Max := 1;
    SaveProgress.Position := 0;

    if TestUniqueNumber(FIdDocument, True) then
      if MessageDlg('Numarul de document : '+Trim(edNumarDoc.Text)+' a fost folosit deja !'#13#10+
                    'Doriti folosirea numarului si pentu documentul curent ?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then begin
        if (FrmData.QryDefaDoc.Locate('ID_GEST_DEFA_DOCUM', FIdDefaDocum, [])) and (FrmData.QryDefaDoc.FieldByName('NUMAR_AUTOMAT').AsBoolean) then
          if (MessageDlg('Tipul de document curent este configurat sa genereze automat numere. Doriti pastrarea numarului configurat de dumneavoastra (Yes) sau generarea unui numar automat (No) ?', mtConfirmation, [mbYes, mbNo], 0) = mrNo) then begin
            SetDocumentField('NR_DOCUM', ''); //resetam pentru generare automata
            SetNewDocNumber;
          end;
        end
        else begin
          edNumarDoc.SetFocus;
          ShowEroare('Numarul de document '+Trim(edNumarDoc.Text)+' exista deja si trebuie inlocuit !');
        end;

     DBStartTransaction;
     try
        GridItemsi.SetFocus;
        QryItemsi.First;
        while not QryItemsi.Eof do begin
          TestItems;
          SaveProgress.Position := SaveProgress.Position + 1;
          Application.ProcessMessages;
          Next;
        end;
        UpdateSumColumns;
        DateUnit.IdGestDocum := ValueSafeToInt(DBGetScallarFmt('exec [spGestValideazaDocument] %d, %d', [FIdDocument, IdUtilizator]));
        FNrConfirmari := DBExecSQLFmt('exec [spGestIntrodValidari] %d, %d', [DateUnit.IdGestDocum, FIdDefaDocum]);
        if FrmData.QryDefaDoc.Locate('ID_GEST_DEFA_DOCUM', FIdDefaDocum, []) then begin
          if FrmData.QryDefaDoc.FieldByName('ID_DOCUMENT_CONEX').IsNull then begin
            FTipDocConex := -1;
            FTipDescarcare := 0;
          end
          else begin
            FTipDocConex := FrmData.QryDefaDoc.FieldByName('ID_DOCUMENT_CONEX').AsInteger;
            FTipDescarcare := FrmData.QryDefaDoc.FieldByName('TIP_DESCARCARE').AsInteger;
          end;
        end
        else FTipDocConex := -1;
       DBCommit;
     except
       on E: Exception do begin
         DBRollBack;
         ShowEroare('EROARE validare document : '+E.Message);
       end;
     end;
     DBRefresh([QryDocument, QryItemsi]);
  finally
     IsInSaveDoc := False;
     QryItemsi.EnableControls;
     GridItemsi.EndUpdate;
     FDetaliiDocum.Inspector.EndUpdate;
     SaveProgress.Visible := False;
  end;
  TiparireDocument;
end;


procedure TfrmcxTcv.QryItemsiAfterOpen(DataSet: TDataSet);
var
  lLastPos : TBookMark;
begin
  QryItemsi.FieldByName('CANTITATE').OnValidate := CANTLVRValidate;
  QryItemsi.FieldByName('valoare_receptie_tva').OnChange  := ValoareReceptieChange;
  QryItemsi.FieldByName('DENMAT').OnChange      := DoDenMatChange;
  QryItemsi.FieldByName('COD_BARA').OnChange    := DoCodBaraChange;
  lLastPos := QryItemsi.GetBookMark;
  try
     QryItemsi.First;
     while not QryItemsi.Eof do begin
       SetMRUValue(GridItemsiTIPMAT, QryItemsi.FieldByName('TIPMAT').AsString);
       SetMRUValue(GridItemsiUM, QryItemsi.FieldByName('UM').AsString);
       QryItemsi.Next;
     end;
  finally
     QryItemsi.GotoBookMark(lLastPos);
     QryItemsi.FreeBookmark(lLastPos);
  end;
  UpdateCodEconomic();
end;

function TfrmcxTcv.ShowStock(ANode: TdxTreeListNode): Boolean;
begin
  Result := ShowStock(aNode.Strings[GridItemsiTIPMAT.Index]);
end;

function TfrmcxTcv.ShowStock(ATipMat: String=''): Boolean;
var
  I : Integer;
  lList: TStringList;
begin
  if ListStockInfo = nil then
    GetTipuriStocuri(FIdDefaDocum);
  with TfrmSelStock.Create(Application) do
    try
       IdGestDefaDocum := FIdDefaDocum;
       Predator        := FPredator.FCodRep;
       Primitor        := FPrimitor.FCodRep;

       if IsValidDate(edDataDoc.EditValue) then begin
         DataDoc           := edDataDoc.Date;
         DataStoc          := edDataDoc.Date;
         edtDataStock.Date := edDataDoc.Date;
       end
       else begin
         DataDoc           := Date;
         DataStoc          := Date;
         edtDataStock.Date := Date;
         edtChangeDataStoc.Checked := True;
       end;

       TcxImageComboBoxProperties(GridStockPRODUS.Properties).Items.Clear;
       for I := 0 to GridItemsiPRODUS.Values.Count - 1 do
         with TcxImageComboBoxProperties(GridStockPRODUS.Properties).Items.Add do begin
           Value := GridItemsiPRODUS.Values[I];
           Description := GridItemsiPRODUS.Descriptions[I];
         end;
       lList := TStringList.Create;
       try
        GetCodMatList(lList);
        InitStock(lList);
       finally
         lList.Free;
       end;
       OpenStock;
       TipMat := ATipMat;
       WindowState := wsMaximized;
       Result := ShowModal = mrOk;
       if Result then begin
          SetFilterSelectate();
          CopyFromStock(MemStock, 'CANT_PREDATOR', 'CANTITATE_SELECTATA');
          GridItemsi.FocusedField := QryItemsi.FindField('CANTITATE');
          ValoareReceptieChange(QryItemsi.FindField('valoare_receptie_tva'));
       end;
    finally
       Free;
    end;
end;

procedure TfrmcxTcv.SetIdDocument(const Value: Integer);
begin
  FIdDocument := Value;
  with QryDocument do begin
    if FIdDocument = -1 then
       FIdDocument := NewDocument
    else begin
      Close;
      Params[0].Value := FIdDocument;
      Open;
      //daca este sters intre timp
      if IsEmpty then ReadDocument;
    end;
    IsInLoading := True;
    try
      //resetam detaliile le reface ActivateGrid daca se ajunge acolo
      FDetaliiDocum.IdDefaDocum := FieldByName('ID_GEST_DEFA_DOCUM').AsInteger;
      edTipDocument.EditValue   := FieldByName('ID_GEST_TIP_DOCUM').AsInteger;
      SetTipDocument(FieldByName('ID_GEST_TIP_DOCUM').AsInteger);
      SetPredator(FieldByName('ID_PREDATOR').AsInteger);
      SetPrimitor(FieldByName('ID_PRIMITOR').AsInteger);
      SetDelegat(FieldByName('ID_REPARTITORI_DELEGATI').AsInteger);
      SetMijlocTransport(FieldByName('ID_REPARTITORI_TRANSPORT').AsInteger);
      edNumarDoc.Text := FieldByName('NR_DOCUM').AsString;
      if (not FieldByName('ID_MODIFICARE').IsNull) and (FieldByName('ID_MODIFICARE').AsInteger > 0) then
         FIdParinte := FieldByName('ID_MODIFICARE').AsInteger
      else FIdParinte := -1;
      if not FieldByName('DATA_DOCUM').IsNull then
         edDataDoc.Date  := FieldByName('DATA_DOCUM').AsDateTime
      else edDataDoc.Text := '';
    finally
      IsInLoading := False;
    end;
  end;
  QryItemsi.Close;
  QryItemsi.Params.ParamByName('ID').Value := FIdDocument;
  QryItemsi.Open;
  ActivateGrid;
  if not QryItemsi.IsEmpty then
    DisableEditor(QryItemsi.FieldByName('PRODUS').AsString);
  SetNextControl;
end;

procedure TfrmcxTcv.QryDocumentNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('ID_UTILIZATORI').AsInteger := IdUtilizator;
  SetBooleanField(DataSet);
end;

procedure TfrmcxTcv.SetBooleanField(DataSet: TDataSet);
var I: Integer;
begin
  with DataSet do
    for I := 0 to FieldCount-1 do
      if Fields[I] is TBooleanField then
         Fields[I].AsBoolean := False;
end;

procedure TfrmcxTcv.BtnOkClick(Sender: TObject);
begin
  if not IsInSaveDoc then begin
    ValidareDocument(TButton(Sender).Tag = 1);
    RefreshFirstDocum;
  end;
end;

function TfrmcxTcv.GetNextDocNumber(AIdDefDoc: Integer; APrefix: String;
  AStart, AEnd: Integer): String;
var TmpNrDoc: String;
    lStart  : Integer;
    Error   : Integer;
begin
  APrefix := Trim(APrefix);
  if DBProcExists('spGestNextNrDocum') then begin
    Result := ValueSafeToStr( DBGetScallarFmt('exec [spGestNextNrDocum] %d, %d, %s, %d, %d', [AIdDefDoc, IdDocument, ValueToStr(APrefix), AStart, AEnd]));
  end
  else begin
    tmpNrDoc := ValueSafeToStr( DBGetScallarFmt('SELECT TOP 1 NR_DOCUM FROM GEST_DOCUM WHERE ID_GEST_DEFA_DOCUM = %d ORDER BY LEN(NR_DOCUM) DESC, NR_DOCUM DESC', [AIdDefDoc]));
    if pos(APrefix, TmpNrDoc) > 0 then
      System.Delete(TmpNrDoc, 1, Length(APrefix));
    Val(TmpNrDoc, lStart, Error);
    if (Error = 0) and (lStart >= AStart) then
      if lStart <= AEnd then
        Inc(lStart)
      else
        ShowEroare('Nu mai aveti numere disponibile pentru tipul de document specificat !'#13#10'Martiti marja de numere din intretinere documente !')
    else
      lStart := AStart;
    AStart := lStart;
    Result := APrefix+IntToStr(AStart);
  end;
end;

procedure TfrmcxTcv.SetNextControl;

  procedure SetActiveControl(AControl: TWinControl);
   begin
     if IsMyFormVisible(AControl) and Self.Enabled and Self.Visible then begin
       if Self.CanFocus and AControl.Enabled and AControl.Visible and AControl.CanFocus and AControl.Showing then AControl.SetFocus // Self.FocusControl(AControl)
                        else
                          if AControl.Enabled and AControl.Visible then Self.ActiveControl := AControl;
     end;
   end;

begin
  if IsInLoading then Exit;
  if FTipDocument > 0 then
    if FPredator.Assigned then
       if Trim(edNumarDoc.Text) > '' then
         if IsValidDate(edDataDoc.EditValue) then
            if FPrimitor.Assigned then SetActiveControl(GridItemsi)
            else SetActiveControl(edPrimitor)
         else SetActiveControl(edDataDoc)
       else SetActiveControl(edNumarDoc)
    else SetActiveControl(edPredator)
  else SetActiveControl(edTipDocument);
end;

procedure TfrmcxTcv.GridItemsiDENMATButtonClick(Sender: TObject;
  AbsoluteIndex: Integer);
var
  cntTipStock  : Integer;
begin
  cntTipStock := GetTipuriStocuri(FIdDefaDocum);
  if cntTipStock > -1 then
    ShowStock(GridItemsi.FocusedNode)
  else
    SelectFromNomenclator('');
end;

procedure TfrmcxTcv.TreeRepartitoriCustomDrawDataCell(Sender: TcxCustomTreeList;
  ACanvas: TcxCanvas; AViewInfo: TcxTreeListEditCellViewInfo;
  var ADone: Boolean);
var
  IsIntern: Boolean;
begin
  IsIntern :=   GetBoolean(AViewInfo.Node.Values[TreeRepartitoriGESTINT.ItemIndex]);
  if IsIntern then ACanvas.Font.Color := clBlue
  else ACanvas.Font.Color := clRed;
end;

procedure TfrmcxTcv.TreeRepartitoriDblClick(Sender: TObject);
begin
  if Assigned(TreeRepartitori.FocusedNode) then
    GetParentForm(TcxDBTreeList(Sender)).ModalResult := mrOK;
end;

procedure TfrmcxTcv.TreeRepartitoriKeyUp(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if ((Key in [VK_UP, VK_DOWN]) and (ssAlt in Shift)) or
      ((Key = VK_F4) and not (ssAlt in Shift)) or (Key = VK_ESCAPE) then
    GetParentForm(TcxDBTreeList(Sender)).ModalResult := mrCancel;
  if Key = VK_RETURN then
    TreeRepartitoriDblClick(TreeRepartitori);
end;

procedure TfrmcxTcv.TreeRepartitoriCustomDrawCell(Sender: TObject;
  ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  AColumn: TdxTreeListColumn; ASelected, AFocused, ANewItemRow: Boolean;
  var AText: String; var AColor: TColor; AFont: TFont;
  var AAlignment: TAlignment; var ADone: Boolean);
var
  IsIntern: Boolean;
begin
  IsIntern := GetBoolean(ANode.Values[TreeRepartitoriGESTINT.ItemIndex]);
  if IsIntern then AFont.Color := clBlue
  else AFont.Color := clRed;
end;

function TfrmcxTcv.NewDocument: Integer;
begin
  DBStartTransaction();
  try
    QryDocument.Open;
    DBSetFieldValue(QryDocument, 'ID_UTILIZATORI', IdUtilizator);
    Result := QryDocument['ID_CULGEST_DOCUM'];
    DBCommit();
  except
    on E: Exception do begin
      DBRollBack();
      ShowEroare('Nu se poate adauga documentul !'#13#10'EROARE : '+E.Message);
    end;
  end;
end;

procedure TfrmcxTcv.SetDocumentField(AFieldName: String; AValue: Variant);
var
  lField: TField;
begin
  TestModificare('Modificare Camp Document', AFieldName, AValue);
  TestIdDocument;
  lField := QryDocument.FindField(AFieldName);
  if not Assigned(lField) then Exit;
  if not (QryDocument.State in [dsEdit, dsInsert]) then
     QryDocument.Edit;
  lField.Value := AValue;
  QryDocument.Post;
end;

procedure TfrmcxTcv.FormCreate(Sender: TObject);
var
  lColumn   : TcxDBTreeListColumn;
  Col1      : TcxTreeListColumn;
  lDataSet  : TDataSet;
  I: Integer;
  ANode: TcxTreeListNode;
begin
  FSumatorFieldList := TStringList.Create;
  FIdParinte := -1;
  IsSQLFiltered := False;
  NotifyForm := nil;
  lDataSet := DBNewQuery('exec [spGestTipMatMRU]');
  try
    lDataSet.Open;
    while not lDataSet.Eof do begin
      GridItemsiTIPMAT.Items.Add(lDataSet.Fields[0].AsString);
      lDataSet.Next;
    end;
    DBSetSQLQuery(lDataSet, 'exec [spGestUMMRU]');
    lDataSet.Open;
    while not lDataSet.Eof do begin
      GridItemsiUM.Items.Add(lDataSet.Fields[0].AsString);
      lDataSet.Next;
    end;
    if DBProcExists('spGestGetTipProduse') then begin
      DBSetSQLQuery(lDataSet, 'exec [spGestGetTipProduse]');
      lDataSet.Open;
      GridItemsiPRODUS.Values.Clear;
      GridItemsiPRODUS.Descriptions.Clear;
      while not lDataSet.Eof do begin
        GridItemsiPRODUS.Values.Add(lDataSet.FieldByName('TIP_PRODUS').AsString);
        GridItemsiPRODUS.Descriptions.Add(lDataSet.FieldByName('DENUMIRE').AsString);
        lDataSet.Next;
      end;
      TreeTipMaterialTIP_PRODUS.Values.Assign(GridItemsiPRODUS.Values);
      TreeTipMaterialTIP_PRODUS.Descriptions.Assign(GridItemsiPRODUS.Descriptions);
    end;
  finally
    lDataSet.Free;
  end;
  QryDrepturiDocs.Close;
  QryDrepturiDocs.Params[0].Value := IdFunctiune;
  QryDrepturiDocs.Open;

  FDetaliiDocum := TFrmDetaliiDocum.Create(Self);
  FDetaliiDocum.ButonLink := BtnDetaliiDocum;
  FDetaliiDocum.Inspector.DataController.DataSource := DTDocument;
  FDetaliiDocum.BorderStyle := bsNone;
  FDetaliiDocum.Parent      := pnDocDetalii;
  FDetaliiDocum.Align       := alClient;
  FDetaliiDocum.Visible     := True;

  FDefalcareBuget := TfrmAlopDisponibil.Create(Self);
  FDefalcareBuget.tabOrd.TabVisible := False;

  GridItemsiDETALII_ANGAJAMENT.PopupControl := FDefalcareBuget;
  GridItemsiCOD_FUNCTIONAL.PopupControl     := FDefalcareBuget;
  GridItemsiCOD_ECONOMIC.PopupControl       := FDefalcareBuget;

  if FrmData.QryRepartitori.FindField('USED_COUNT') <> nil then begin

    Col1 := TreeRepartitori.CreateColumn(TreeRepartitori.Bands[0]);
    lColumn :=  Col1 as TcxDBTreeListColumn;
    with lColumn do begin
      PropertiesClass :=  TcxMaskEditProperties;
      DataBinding.FieldName := 'USED_COUNT';
      Caption.AlignHorz := taCenter;
      Caption.Text   := 'Zero';
      Width     := 40;
    end;

  for I := 0 to TreeRepartitori.AbsoluteCount - 1 do
  begin
    ANode := TreeRepartitori.AbsoluteItems[I];
    if ANode.Values[lColumn.ItemIndex] <>  'Rep. Folosit' then
      ANode.Visible := false;
  end;
//    TreeRepartitori.Filter.Add(lColumn, 0, 'Rep. Folosit', otGreater);
  end;

  edDelegat.OnEditKeyDown := DelegatKeyDown;
  edMijTransport.OnEditKeyDown := AutoKeyDown;

  DateUnit.IdGestDocum := -1;
  FFieldDocumList := TStringList.Create;
  FFieldDocumList.Sorted := True;
  FTipuriMatList := TStringList.Create;
  FTipuriMatList.Sorted := True;
  FTipuriMatList.Duplicates := dupIgnore;
  RegisterGrid(GridItemsi);
  FDenTipMatrList := TStringList.Create;
  FDenTipMatrList.Duplicates := dupAccept;

  memIntern := TAtsMemData.Create(Self);
  memExtern := TAtsMemData.Create(Self);
  memTot    := TAtsMemData.Create(Self);

  PopulateMemRep;
end;

procedure TfrmcxTcv.CANTLVRValidate(Sender: TField);
begin
  { Validare Stock }
  TestModificare('Modificare Camp Pozitie', Sender.FieldName, Sender.Value);
  if IsInLoading then
     Exit;
  if (not IsNewCodMat) and (IsExistCodMat) then begin
     if QryItemsi.FieldByName('STOCK_BEFORE').AsInteger < Sender.AsInteger then
        if MessageDlg('Ati ales o cantitate mai mare decat cea pe care o aveti in stoc!'#13#10'Doriti continuarea?',
                      mtConfirmation, [mbYes, mbNo],0) <> mrYes then Abort;
     QryItemsi.FieldByName('STOCK_AFTER').AsInteger := QryItemsi.FieldByName('STOCK_BEFORE').AsInteger - Sender.AsInteger;
  end
  else QryItemsi.FieldByName('STOCK_AFTER').AsInteger := Sender.AsInteger;
end;

procedure TfrmcxTcv.GridItemsiCustomDrawCell(Sender: TObject;
  ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  AColumn: TdxTreeListColumn; ASelected, AFocused, ANewItemRow: Boolean;
  var AText: String; var AColor: TColor; AFont: TFont;
  var AAlignment: TAlignment; var ADone: Boolean);

  function GetRealIndex: Integer;
   var I: Integer;
   begin
     Result := -1;
     for I := 0 to GridItemsi.VisibleColumnCount-1 do
       if GridItemsi.VisibleColumns[I] = AColumn then begin
          Result := I;
          Break;
       end;
   end;

var
  StockAfter: Integer;
begin
  if (AFocused) and (GetRealIndex = GridItemsi.FocusedColumn) then begin
     AColor := clAqua;
     AFont.Color := clBlack;
  end
  else begin
    if not (ANode.HasChildren) and (Trim(ANode.Strings[GridItemsiSTOCK_AFTER.Index]) > '') then
       StockAfter := ANode.Values[GridItemsiSTOCK_AFTER.Index]
    else StockAfter := 0;
    if StockAfter < 0 then begin AColor := clRed; AFont.Color := clYellow; end;
  end;
end;

procedure TfrmcxTcv.QryItemsiBeforeDelete(DataSet: TDataSet);
begin
  TestModificare('Stergere Pozitie Document');
  if IsInLoading then Exit;
  if not cst_DeleteAll then
    case MessageDlg('Doriti stergerea pozitie curente de document?', mtConfirmation, [mbYes, mbNo, mbYesToAll], 0) of
       mrYes :;
       mrYesToAll :  cst_DeleteAll := True;
       else Abort;
    end;
end;

procedure TfrmcxTcv.QryItemsiAfterPost(DataSet: TDataSet);
begin
  GridItemsi.ApplyBestFit(nil);
end;

function TfrmcxTcv.ExistsCodMat(ACodMat: Integer): Boolean;
var I: Integer;
begin
  Result := False;
  for I := 0 to GridItemsi.Count-1 do
    if GridItemsi.Items[I].Strings[GridItemsiCODMAT.Index] = IntToStr(ACodMat) then begin
       Result := True;
       Break;
    end;
end;

procedure TfrmcxTcv.GridItemsiKeyPress(Sender: TObject; var Key: Char);
begin
  if (Assigned(GridItemsi.InplaceEditor)) and (Key = '?')
     and ( (GridItemsi.FocusedField = QryItemsi.FindField('DETALII_ANGAJAMENT')) or
           (GridItemsi.FocusedField = QryItemsi.FindField('RepD')) or
           (GridItemsi.FocusedField = QryItemsi.FindField('RepC'))
           ) then
     PostMessage(GridItemsi.InplaceEditor.Handle, CM_DROPDOWNPOPUP, 0, 0);
end;

procedure TfrmcxTcv.FormDestroy(Sender: TObject);
begin
  ClearListStockInfo;
  FreeAndNil(ListStockInfo);
  FFieldDocumList.Free;
  FDetaliiDocum.Free;
  FDenTipMatrList.Free;
  FTipuriMatList.Free;
  FSumatorFieldList.Free;
  ExitSingleUser;
end;

procedure TfrmcxTcv.SetNewDocNumber;
begin
  { Seteaza un nou numar de document in cazul in care cel anterioar este folosit deja }
  if not FrmData.QryDefaDoc.Locate('ID_GEST_DEFA_DOCUM', FIdDefaDocum, []) then begin
    edNumarDoc.SetFocus;
    ShowEroare('Nu exista descriere pentru tipul curent de document !');
  end;
  if FrmData.QryDefaDoc.FieldByName('NUMAR_AUTOMAT').AsBoolean then begin
    edNumarDoc.Text := GetNextDocNumber(FIdDefaDocum,
                                        FrmData.QryDefaDoc.FieldByName('NUMAR_PREFIX').AsString,
                                        FrmData.QryDefaDoc.FieldByName('NUMAR_START').AsInteger,
                                        FrmData.QryDefaDoc.FieldByName('NUMAR_END').AsInteger);
    SetDocumentField('NR_DOCUM', edNumarDoc.Text);
  end
  else begin
    edNumarDoc.SetFocus;
    ShowEroare('Numarul de document nu se poate genera automat !'#13#10'Va rugam modificati manual numarul de document !');
  end;
end;

procedure TfrmcxTcv.edNumarDocKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if (not IsInLoading) and (Key = VK_RETURN) then SetNextControl;
end;

procedure TfrmcxTcv.edNumarDocPropertiesValidate(Sender: TObject;
  var DisplayValue: Variant; var ErrorText: TCaption; var Error: Boolean);
begin
 if not IsInLoading then
  SetDocumentField('NR_DOCUM', edNumarDoc.EditValue);
end;

procedure TfrmcxTcv.TestItems;
var
   FRealStock  : Double;
   FNewCodMat  : Boolean;
   I           : Integer;
   lField      : TField;

   procedure SetItemsiField(AFieldName: String; AValue: Variant);
    begin
      if not (QryItemsi.State in [dsEdit, dsInsert]) then QryItemsi.Edit;
      QryItemsi.FieldByName(AFieldName).Value := AValue;
    end;


   function GetCodSumator: Integer;
   var
    I: Integer;
    lField: TField;
    lDataSet: TDataSet;
   begin
      Result := ValueSafeToInt( DBGetScallar('SELECT ID_GEST_SUMATOR FROM GEST_SUMATOR WHERE TIPMAT = :TIPMAT AND DENMAT = :DENMAT', QryItemsi) );
      if Result = 0 then begin
        lDataSet := DBNewUpdateQuery('SELECT * FROM GEST_SUMATOR WHERE ID_GEST_SUMATOR = -1');
        try
          lDataSet.Open;
          lDataSet.Append;
          for I := 0 to lDataSet.FieldCount - 1 do begin
            if AnsiCompareText(lDataSet.Fields[I].FieldName, 'ID_GEST_SUMATOR') <> 0 then
              lField := QryItemsi.FindField(lDataSet.Fields[I].FieldName)
            else lField := nil;
            if Assigned(lField) then lDataSet.Fields[I].Value := lField.Value
          end;
          lDataSet.Post;
          Result := lDataSet.FieldByName('ID_GEST_SUMATOR').AsInteger;
        finally
          lDataSet.Free;
        end;
      end;
    end;

  function NewGetCodSumator : Integer;
  var
    I         : Integer;
    lDataSet  : TDataSet;
    lField    : TField;
    lSQL      : WideString;
  begin
    if FSumatorFieldList.Count =0 then begin
      Result := GetCodSumator;
      Exit;
    end;
    lSQL := '';
    for I := 0 to FSumatorFieldList.Count -1 do
      if lSQL = '' then
        lSQL := FSumatorFieldList.Strings[I] + ' = :' + FSumatorFieldList.Strings[I]
      else
        lSQL := lSQL + ' AND ' + FSumatorFieldList.Strings[I] + ' = :' + FSumatorFieldList.Strings[I];
    Result := ValueSafeToInt( DBGetScallar('SELECT ID_GEST_SUMATOR FROM GEST_SUMATOR WHERE ' + lSQL, QryItemsi) );
    if Result = 0 then begin
      lDataSet := DBNewUpdateQuery('SELECT * FROM GEST_SUMATOR WHERE ID_GEST_SUMATOR = -1');
      try
        lDataSet.Open;
        lDataSet.Append;
        for I := 0 to FSumatorFieldList.Count -1 do begin
          lField := QryItemsi.FindField(FSumatorFieldList.Strings[I]);
          if Assigned(lField) then lDataSet[FSumatorFieldList.Strings[I]] := lField.Value
        end;
        lDataSet.Post;
        Result := lDataSet.FieldByName('ID_GEST_SUMATOR').AsInteger;
      finally
        lDataSet.Free;
      end;
    end;
  end;

var
  lNmcl: TZQuery;
begin
  FNewCodMat := ((IsNewCodMat) and (FIdParinte = -1)) or (not IsExistCodMat);
  if (FIdParinte > -1) and (IsExistCodMat) then begin
    lNmcl := DBNewUpdateQuery('SELECT * FROM GEST_GNMCL WHERE CODMAT = :CODMAT', QryItemsi);
    try
      lNmcl.Open;
      if not lNmcl.IsEmpty then begin
        lNmcl.Edit;
        for I := 0 to lNmcl.FieldCount-1 do begin
          lField := QryItemsi.FindField(lNmcl.Fields[I].FieldName);
          if Assigned(lField) and not SameText(lField.FieldName, 'CODMAT') then
            lNmcl.Fields[I].Value := lField.Value;
        end;
        lNmcl.FieldByName('ID_GEST_SUMATOR').AsInteger := NewGetCodSumator;
        DBPost(lNmcl);
        SetItemsiField('CODMAT', lNmcl['CODMAT']);
        if IsNewCodMat then
          DBExecSQL('exec [spTCVCopiazaProcenteToNmcl] :ID_CULGEST_ITEMSI, :CODMAT', QryItemsi);
      end;
    finally
      lNmcl.Free;
    end;
  end;
  if FNewCodMat then begin
    lNmcl := DBNewUpdateQuery('SELECT * FROM GEST_GNMCL WHERE CODMAT = -1');
    try
      lNmcl.Open;
      lNmcl.Append;
      for I := 0 to lNmcl.FieldCount-1 do begin
        lField := QryItemsi.FindField(lNmcl.Fields[I].FieldName);
        if Assigned(lField) and not SameText(lField.FieldName, 'CODMAT') then
          lNmcl.Fields[I].Value := lField.Value;
      end;
      lNmcl['ID_INITIAL']       := QryItemsi['CODMAT'];
      lNmcl['ID_GEST_SUMATOR']  := NewGetCodSumator;
      DBPost(lNmcl);
      SetItemsiField('CODMAT', lNmcl['CODMAT']);
      DBExecSql('exec [spTCVCopiazaProcenteToNmcl] :ID_CULGEST_ITEMSI, :CODMAT', QryItemsi);
    finally
      lNmcl.Free;
    end;
  end
  else if not IsNewCodMat then begin
    FRealStock := GetStockCodMat(QryItemsi.FieldByName('CODMAT').AsInteger);
    if FRealStock > 0 then begin
      if FRealStock < QryItemsi.FieldByName('STOCK_BEFORE').AsFloat then
        if FRealStock > QryItemsi.FieldByName('CANTITATE').AsFloat then
          SetItemsiField('STOCK_BEFORE', FRealStock)
        else begin
          if not AcceptStockNegativ then begin
            case MessageDlg(Format('Nu mai aveti in stock cantitatea integrala!'#13#10'Cantitate : %f - Stock : %f'#13#10+
                                   'Doriti modificarea cantitatii cu stocul disponibil?'#13#10+
                                   'Yes - Modificare cantitate, Cancel - Abandon', [QryItemsi.FieldByName('CANTITATE').AsFloat, FRealStock]), mtConfirmation, [mbYes, mbCancel], 0) of
                mrYes:
                  begin
                    SetItemsiField('STOCK_BEFORE', FRealStock);
                    SetItemsiField('CANTITATE', FRealStock);
                  end;
                mrCancel:
                  Abort;
              end;
            end
            else begin
              case MessageDlg(Format('Nu mai aveti in stock cantitatea integrala!'#13#10'Cantitate : %f - Stock : %f'#13#10+
                                     'Doriti modificarea cantitatii cu stocul disponibil?'#13#10+
                                     'Yes - Modificare cantitate, No - Salvarea cantitate integrala, Cancel - Abandon', [QryItemsi.FieldByName('CANTITATE').AsFloat, FRealStock]), mtConfirmation, [mbYes, mbNo, mbCancel], 0) of
                mrYes:
                  begin
                    SetItemsiField('STOCK_BEFORE', FRealStock);
                    SetItemsiField('CANTITATE', FRealStock);
                  end;
                mrNo:
                  ;
                mrCancel:
                  Abort;
              end;
            end;
           end
      end
      else
      if (FRealStock = 0) and (QryItemsi.FieldByName('CANTITATE').AsInteger < 0) then begin
        SetItemsiField('STOCK_BEFORE', FRealStock);
        SetItemsiField('STOCK_AFTER', -1 * QryItemsi.FieldByName('CANTITATE').AsInteger);
      end
      else if not AcceptStockNegativ and not (QryItemsi.FieldByName('SEMN_CANTITATE').AsInteger = -1) and not (QryItemsi.FieldByName('CANTITATE').AsInteger < 0) then begin
        GridItemsi.FocusedField := QryItemsi.FieldByName('DENMAT');
        if FRealStock = 0 then
          ShowEroare('Nu mai aveti materialul : ['+QryItemsi.FieldByName('DENMAT').AsString+'] pe stoc !')
        else
        if FRealStock < 0 then
          if (MessageDlg('Atentie materialul ['+QryItemsi.FieldByName('DENMAT').AsString+'] are stoc negativ ( ' + FloatToStr(FRealStock) +' ). Doriti continuarea (Yes) sau abandonati (No) ?', mtError, [mbYes, mbNo], 0) = mrNo) then
            ShowEroare('Nu mai aveti materialul : ['+QryItemsi.FieldByName('DENMAT').AsString+'] pe stoc !');
      end;
  end;
  DBPost(QryItemsi);
end;

function TfrmcxTcv.GetStockCodMat(ACodMat: Integer): Double;
begin
  Result := DBGetScallarFmt('EXEC spGestStockByCodMat %d, %d, %d, null', [FIdDefaDocum, FPredator.FCodRep, ACodMat], 'STOCK');
end;

function TfrmcxTcv.IsExistCodMat: Boolean;
begin
  Result := (not QryItemsi.FieldByName('CODMAT').IsNull) and (QryItemsi.FieldByName('CODMAT').AsInteger > 0);
end;

procedure TfrmcxTcv.QryItemsiBeforePost(DataSet: TDataSet);
var I: Integer;
    lField: TField;

  function IsValidField: Boolean;
   begin
     Result := (not Assigned(lField)) or
               (not lField.IsNull) or
               (Trim(lField.AsString) > '');
   end;
begin
  if IsInLoading then
     Exit;

  { Validam Campurile obligatorii }
  for I := 0 to GridItemsi.ColumnCount-1 do begin
    lField := GridItemsi.Columns[I].Field;
    if ((GridItemsi.Columns[I].Tag) = 1) and (not IsValidField) then begin
       if GridItemsi.Columns[I].Visible then
          GridItemsi.FocusedField := lField
       else
          ShowEroare('ATENTIE -> Campul '+lField.FieldName+' este obligatoriu de introdus insa nu este visibil !'#13#10+
                     'Pentru a-l face visibil selectati din lista de coloane disponibile in Customizare coloane');
       ShowEroare('Campul '+GridItemsi.Columns[I].Caption+' trebuie introdus obligatoriu !');
    end;
  end;

  if (not IsNewCodMat) and (not IsExistCodMat) then begin
     GridItemsi.FocusedField := QryItemsi.FindField('DENMAT');
     GridItemsi.ShowEditor;
     ShowEroare('Pentru acest tip de material trebuie sa alegeti o pozitie din stocuri !');
  end;

end;

procedure TfrmcxTcv.GridItemsiCustomDrawColumnHeader(Sender: TObject;
  AColumn: TdxTreeListColumn; ACanvas: TCanvas; ARect: TRect;
  var AText: String; var AColor: TColor; AFont: TFont;
  var AAlignment: TAlignment; var ASorted: TdxTreeListColumnSort;
  var ADone: Boolean);
begin
  if AColumn.Tag = 1 then begin
     if AColumn.BandIndex > 0 then AColor := clRed;
     AFont.Style := AFont.Style + [fsBold];
  end;
end;

procedure WriteDocumentFizic2DB(Sender: TObject; AStream: TStream);
var
  lDest  : TMemoryStream;

  procedure PrepareStream(const Stream : TStream);
  var
    lZipStream: TZCompressionStream;
  begin
     AStream.Position := 0;
     lZipStream := TZCompressionStream.Create(Stream, zcDefault);
     try
       lZipStream.CopyFrom(AStream, 0);
     finally
       lZipStream.Free;
     end;
  end;

begin
  if DBProcExists('spGestUploadFormaFizica') then begin
    lDest := TMemoryStream.Create;
    try
      PrepareStream(lDest);
      DBExecSQLFmt( 'exec spGestUploadFormaFizica %d, %s', [DateUnit.IdGestDocum, DBStreamToStr(lDest)]);
    finally
      lDest.Free;
    end;
  end;
end;

procedure TfrmcxTcv.ShowDetaliiDocument;
begin
  splitterDetalii.OpenSplitter;
end;

procedure TfrmcxTcv.HideDetaliiDocument;
begin
  splitterDetalii.CloseSplitter;
  BtnDetaliiDocum.Down := False;
end;

procedure TfrmcxTcv.TiparireDocument;
var
  lIdDefaDocConex : Integer;
  lPrevIdInitial  : Integer;
  lFindStr        : String;
  lPrevPrint      : Boolean;
begin
  { Avem in IdDefaDocum si raportul care se tipareste }
  lPrevIdInitial := DateUnit.IdGestDocum;
  try
    FIdGestDocumConex := -1;
    lPrevPrint := (FNrConfirmari < 1) or (ForcePrint);
    try
      PrintDocument2DB(lPrevPrint);
    except
    end;
    case FTipDescarcare of
      0,1:
        if FTipDocConex > 0 then begin
          if FTipDescarcare = 0 then
            lFindStr := 'ID_GEST_TIP_DOCUM;PRIMITOR_INTERN;PREDATOR_INTERN'
          else
            lFindStr := 'ID_GEST_TIP_DOCUM;PREDATOR_INTERN;PRIMITOR_INTERN';
          if FrmData.QryDefaDoc.Locate(lFindStr, VarArrayOf([FTipDocConex, FPrimitor.GestInt, FPredator.GestInt]), []) then begin
            lIdDefaDocConex       := FrmData.QryDefaDoc.FieldByName('ID_GEST_DEFA_DOCUM').AsInteger;
            DateUnit.IdGestDocum  := ValueSafeToInt( DBGetScallarFmt('exec [spGestCreateDocConex] %d, %d, %d', [DateUnit.IdGestDocum, lIdDefaDocConex, IdUtilizator], 0 ) );
            if DateUnit.IdGestDocum > 0 then begin
              FIdGestDocumConex := DateUnit.IdGestDocum;
              FNrConfirmari     := DBExecSQLFmt('exec [spGestIntrodValidari] %d, %d', [DateUnit.IdGestDocum, lIdDefaDocConex] );
              try
                PrintDocument2DB(lPrevPrint);
              except
              end;
            end;
          end;
        end;
      2:
        begin
          DBExecSQLFmt('exec [spCopiazaOrdonanareFromFactura] %d, %d', [IdUtilizator, lPrevIdInitial]);
        end;
    end;
  finally
    DateUnit.IdGestDocum := lPrevIdInitial;
  end;
end;

procedure TfrmcxTcv.InitTipuriMateriale;
begin
  FTipuriMatList.Clear;
  FDenTipMatrList.Clear;
  if QryTipuriMatriale.Active then QryTipuriMatriale.Active := False;
  QryTipuriMatriale.Params.ParamByName('ID_GEST_DEFA_DOCUM').Value := FIdDefaDocum;
  QryTipuriMatriale.Active := True;
  while not QryTipuriMatriale.Eof do begin
    FDenTipMatrList.AddObject(QryTipuriMatriale.FieldByName('DENUMIRE').AsString, TObject(QryTipuriMatriale.FieldByName('ID_GEST_TIP_MATERIAL').AsInteger));
    FTipuriMatList.AddObject(QryTipuriMatriale.FieldByName('ID_GEST_TIP_MATERIAL').AsString,
      TObject(Integer(QryTipuriMatriale.FieldByName('GENEREAZA_CODMAT').AsBoolean) + Integer(QryTipuriMatriale.FieldByName('ACCEPT_STOCK_NEGATIV').AsBoolean) shl 16));
    QryTipuriMatriale.Next;
  end;
end;

function TfrmcxTcv.IsNewCodMat: Boolean;
var
  lIndex: Integer;
begin
  lIndex := FTipuriMatList.IndexOf(QryItemsi.FieldByName('ID_GEST_TIP_MATERIAL').AsString);
  Result := (lIndex = -1) or ((lIndex > -1) and (Boolean(Integer(FTipuriMatList.Objects[lIndex]) and $0FFFF)));
end;

function TfrmcxTcv.AcceptStockNegativ: Boolean;
var
  lIndex: Integer;
begin
  lIndex := FTipuriMatList.IndexOf(QryItemsi.FieldByName('ID_GEST_TIP_MATERIAL').AsString);
  Result := (lIndex = -1) or (Boolean(Integer(FTipuriMatList.Objects[lIndex]) shr 16));
end;

procedure TfrmcxTcv.BtnDetaliiDocumClick(Sender: TObject);
begin
  if BtnDetaliiDocum.Down then
    ShowDetaliiDocument
  else
    HideDetaliiDocument;
end;

procedure TfrmcxTcv.InternalFilterDocument(DataSet: TDataSet;
  var Accept: Boolean);
begin
  Accept := QryDrepturiDocs.Locate('ID_GEST_TIP_DOCUM', DataSet.FieldByName('ID_GEST_TIP_DOCUM').AsInteger, []);
end;

function TfrmcxTcv.GetRigthsFromDocum(AField: String): Integer;
begin
  Result := 0;
  if QryDrepturiDocs.Locate('ID_GEST_TIP_DOCUM', FTipDocument, []) then
     while (not QryDrepturiDocs.Eof) and (QryDrepturiDocs.FieldByName('ID_GEST_TIP_DOCUM').AsInteger = FTipDocument) do begin
       if FrmData.QryDefaDoc.Locate('ID_GEST_DEFA_DOCUM', QryDrepturiDocs.FieldByName('ID_GEST_DEFA_DOCUM').AsInteger, []) then
          if FrmData.QryDefaDoc.FieldByName(AField).AsBoolean then
             Result := Result or $01
          else Result := Result or $02;
       QryDrepturiDocs.Next;
     end;
end;

procedure TfrmcxTcv.ValidareDocument(Tiparire : Boolean = False);
var
  lPredator,
  lNrDocum: String;
begin
  DBPost([QryDocument, QryItemsi]);
  TestIdDocument;
  if btnDM.Visible and (QryDocument.FieldByName('TethysId').AsString = '') then
    ShowEroare('Documentul %s nu are asociat un document din Tethys. Asociati un document', [lNrDocum]);
  if ValueSafeToDateTime(edDataDoc.EditValue) = 0.0 then
    ShowEroare('Documentul %s nu are data selectata !', [lNrDocum]);
  if QryItemsi.IsEmpty then
       ShowEroare('Documentul %s nu are pozitii introduse !', [lNrDocum]);
  ForcePrint := Tiparire;
  lPredator  := Trim(edPredator.Text);
  lNrDocum   := Trim(edNumarDoc.Text);
  if FIdParinte <= 0 then
    if TestUniqueNumber(FIdDocument, False) then
      ShowEroare('Documentul numar %s exista deja introdus la %s', [lNrDocum, lPredator]);
  WriteToDb;
  if FIdParinte > 0 then
     Reconciliere(FIdParinte, DateUnit.IdGestDocum);
end;

procedure TfrmcxTcv.CopyDocToCulegere(const AIdDocument: Integer);
begin
  { Copiem documentul in zona tampon pentru modificare }
  DBStartTransaction;
  try
    DBExecSQLFmt('exec [spGestDocumInCache] %d, %d', [AIdDocument, IdUtilizator]);
    DBCommit;
  except
    on E: Exception do begin
      DBRollBack;
      raise;
    end;
  end;
end;

procedure TfrmcxTcv.SetNumarZerouri(NrZeroruri: Integer; AColumn: TdxDBTreeListColumn=nil);
var I: Integer;
    lFormat : String;

     procedure SetColumnDecimal;
      begin
        with TdxDBGridCurrencyColumn(AColumn) do begin
          DisplayFormat := lFormat;
          DecimalPlaces := NrZeroruri;
          if (Assigned(Field)) and (Field is TFloatField) then begin
             TFloatField(Field).Precision     := NrZeroruri;
             TFloatField(Field).DisplayFormat := lFormat;
          end;
        end;
      end;
begin
  { Setam numarul de zerouri }
  lFormat := DupeString('0', NrZeroruri);
  lFormat := ',0.'+lFormat+';-,0.'+lFormat;
  if (AColumn <> nil) and (AColumn is TdxDBGridCurrencyColumn) then
     SetColumnDecimal
  else
     for I := 0 to GridItemsi.ColumnCount-1 do
       if GridItemsi.Columns[I] is TdxDBTreeListCurrencyColumn then begin
          AColumn := GridItemsi.Columns[I];
       SetColumnDecimal;
     end;
end;

procedure TfrmcxTcv.SetRepartitorFilter(AFilter: String);
var
  lDataSet: TDataSet;
begin
  if Pos('TRUE', UpperCase(AFilter)) > 0 then
    lDataSet := memIntern
  else
  if Pos('FALSE', UpperCase(AFilter)) > 0 then
    lDataSet := memExtern
  else
    lDataSet := memTot;

  if TreeRepartitori.DataController.DataSource.DataSet <> lDataSet then begin
    TreeRepartitori.BeginUpdate;
    try
      TreeRepartitori.DataController.DataSource.DataSet := lDataSet;
    finally
      TreeRepartitori.EndUpdate;
    end;
  end;
end;

procedure TfrmcxTcv.edTipDocumentEnter(Sender: TObject);
begin
  if Sender is TcxRepartitorPanel then
    TcxRepartitorPanel(Sender).ListaInput.DroppedDown := True
  else
  if Sender is TcxCustomDropDownEdit then
    TcxCustomDropDownEdit(Sender).DroppedDown := True;
end;

procedure TfrmcxTcv.edTipDocumentPropertiesCloseUp(Sender: TObject);
begin
  { Anulam filtrul in cazul in care avem unul }
  FrmData.QryDocumente.OnFilterRecord := nil;
  FrmData.QryDocumente.Filtered       := False;
  SetRepartitorFilter('');
end;

procedure TfrmcxTcv.edTipDocumentPropertiesEditValueChanged(Sender: TObject);
begin
  SetTipDocument(edTipDocument.EditValue);
  ClearListStockInfo;
  FreeAndNil(ListStockInfo);
  btnValidareConex.Enabled := False;
end;

procedure TfrmcxTcv.edTipDocumentPropertiesInitPopup(Sender: TObject);
begin
  FrmData.QryDocumente.OnFilterRecord := InternalFilterDocument;
  FrmData.QryDocumente.Filtered       := True;
  frmData.QryDocumente.Locate('ID_GEST_TIP_DOCUM', edTipDocument.EditValue, []);
end;

procedure TfrmcxTcv.edPredatorButtonClick(Sender: TObject);
begin
  DBRefresh(frmData.qryRepartitori);
  with TFrmOERepartitori.Create(Application) do
  try
    WindowState := wsMaximized;
    ShowModal;
  finally
    Free;
    PopulateMemRep;
  end;
end;

procedure TfrmcxTcv.edPredatorPopupInitPopup(Sender: TObject);
var
  lGestType     : Integer;
  lOtherGestIn  : Boolean;
  lCanComplement: Boolean;
  lNode         : TcxDBTreeListNode;
  lCurentGest   : Integer;
  lIsPredator   : Boolean;
  lRepartitor   : TcxRepartitorPanel;
  lPopupEdit    : TcxPopupEdit;
begin
  lPopupEdit  := TcxPopupEdit(Sender);
  lRepartitor := TcxRepartitorPanel(lPopupEdit.Owner);
  lIsPredator := lPopupEdit = edPredator.ListaInput;
  if lIsPredator then begin
    lGestType       := FGestPredator;
    lOtherGestIn    := FPrimitor.GestInt;
    lCanComplement  := FPrimitor.Assigned;
    lCurentGest     := FPredator.FCodRep;
    TreeRepartitori.Tag := 0;
  end
  else begin
    lGestType       := FGestPrimitor;
    lOtherGestIn    := FPredator.GestInt;
    lCanComplement  := FPredator.Assigned;
    lCurentGest     := FPrimitor.FCodRep;
    TreeRepartitori.Tag := 1;
  end;
  if (lCanComplement) and (FComplementare) and (lGestType = 3) then
     if lOtherGestIn then lGestType := 2
     else lGestType := 1;
  if lGestType = 3 then
     if lIsPredator then lGestType := GetRigthsFromDocum('PREDATOR_INTERN')
     else lGestType := GetRigthsFromDocum('PRIMITOR_INTERN');
  case lGestType of
    1: SetRepartitorFilter('GESTINT = True');
    2: SetRepartitorFilter('GESTINT = False');
    3: SetRepartitorFilter('');
  end;
  //TreeRepartitori.DataSource := DTDropDownRep;
  { Implicit ne pozitionam pe departamentul la care suntem asignati }
  if (lGestType = 1) and (lCurentGest < 1) then
     lCurentGest := IdDepartament;
  if lCurentGest > 0 then begin
    lNode := TreeRepartitori.FindNodeByKeyValue(lCurentGest);
    if not Assigned(lNode) then lNode := TreeRepartitori.FindNodeByKeyValue(IdDepartament);
    if Assigned(lNode) and not ValueSameValue(lNode.KeyValue, lRepartitor.KeyValue) then
      lRepartitor.KeyValue := lNode.KeyValue;
  end;
  { Stabilim dimensiunea }
  lPopupEdit.Properties.PopupMinWidth := lPopupEdit.Width;
end;

procedure TfrmcxTcv.GridItemsiDETALII_ANGAJAMENTInitPopup(Sender: TObject);
begin
  TestModificare('Modificare Angajament - Informatii Executie');
  DBPost(QryItemsi);
  if IsNewCodMat then begin
    if not ValueHasValue(QryItemsi['cantitate']) then begin
      GridItemsi.FocusedColumn := GridItemsiCANTITATE.Index;
      MessageDlg('Va rugam introduceti cantitatea !', mtError, [mbOk], 0);
      Abort;
    end;
    if not ValueHasValue(QryItemsi['pret_unitar']) then begin
      GridItemsi.FocusedColumn := GridItemsiPRET_UNITAR.Index;
      MessageDlg('Va rugam introduceti valoare pozitiei !', mtError, [mbOk], 0);
      Abort;
    end;
    FDefalcareBuget.IsOnDocument := True;
    FDefalcareBuget.SetSelectieProcent(QryItemsi['id_culgest_itemsi'], QryItemsi['valoare_receptie_tva'], QryItemsi['cantitate']);
  end
  else begin
    FDefalcareBuget.IsOnDocument := False;
    FDefalcareBuget.DisableEditProcent;
  end;
  FDefalcareBuget.PrepareCulegere(FPredator.FCodRep,
                                  QryItemsi['COD_FUNCTIONAL'],
                                  QryItemsi['COD_ECONOMIC'],
                                  QryItemsi['ID_ANGAJAMENTE_DEFALCARE'],
                                  QryItemsi['ID_ORDONANTARE_DEFALCARE'],
                                  QryItemsi['ID_OI_UNITATI'],
                                  QryItemsi['ID_OI_PROIECTE'],
                                  QryDocument['DATA_DOCUM']);
end;

procedure TfrmcxTcv.GridItemsiDETALII_ANGAJAMENTCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
begin
  if Accept then begin
    FDefalcareBuget.SalveazaDefalcareProcenti;
    { Modificam Informatiile }
    DBGoEdit(QryItemsi);
    QryItemsi['ID_ANGAJAMENTE_DEFALCARE'] := FDefalcareBuget.IdAngajament;
    QryItemsi['ID_ORDONANTARE_DEFALCARE'] := FDefalcareBuget.IdOrdonantare;
    QryItemsi['ID_OI_UNITATI']            := FDefalcareBuget.IdUnitati;
    QryItemsi['ID_OI_PROIECTE']           := FDefalcareBuget.IdProiecte;
    QryItemsi['COD_FUNCTIONAL']           := FDefalcareBuget.CodFunctional;
    QryItemsi['COD_ECONOMIC']             := FDefalcareBuget.CodEconomic;
    QryItemsi['DETALII_ANGAJAMENT']       := FDefalcareBuget.Descriere;
    DBPost(QryItemsi);
    UpdateCodEconomic();
  end;
end;

procedure TfrmcxTcv.QryDocumentAfterPost(DataSet: TDataSet);
begin
  TestIdDocument;
end;

procedure TfrmcxTcv.EvaluatorDocumEvaluate(Sender: TObject; Eval: String;
  Args: array of Variant; ArgCount: Integer; var Value: Variant;
  var Done: Boolean);
//var DBSum: TDBSum;
begin
  Exit;
  PostMessage(Handle, WM_SENDPOSTITEMS, 0, 0);
end;



procedure TfrmcxTcv.UpdateSumColumns;
begin
  { recalculam elementele de la nivelul documentului }
end;

procedure TfrmcxTcv.pnClientResize(Sender: TObject);
begin
//
end;

procedure TfrmcxTcv.edDelegatPopupCloseUp(Sender: TObject);
begin
  if GetParentForm(edDelegat.PopupEdit.PopupControl).ModalResult = mrOk then
    SetDelegat(edDelegat.KeyValue);
end;

procedure TfrmcxTcv.edDelegatPopupInitPopup(Sender: TObject);
begin
  { Stabilim dimensiunea }
  with TdxPopupEdit(Sender) do
    if PopupWidth < Width then PopupWidth := Width;
end;

procedure TfrmcxTcv.edDelegatValidate(Sender: TObject;
  var AKeyValue: Variant);
var
   lNode: TdxDBTreeListNode;
   lTree : TdxDBTreeList;
   lTextC, lSearchC : TdxDBTreeListColumn;
begin
   if not(TRepartitorPanel(Sender).PopupEdit.PopupControl is TdxDBTreeList) then Exit;
   lTree := TdxDBTreeList(TRepartitorPanel(Sender).PopupEdit.PopupControl);
   lNode := lTree.FindNodeByKeyValue(AKeyValue);
   TRepartitorPanel(Sender).Text := '';
   TRepartitorPanel(Sender).TextEdit.Text := '';
   if Assigned(lNode) then begin
      try
        TRepartitorPanel(Sender).Tag := AKeyValue;
      except
        TRepartitorPanel(Sender).Tag := -1;
      end;
      lTextC := lTree.FindColumnByFieldName(TRepartitorPanel(Sender).ListField);
      lSearchC := lTree.FindColumnByFieldName(TRepartitorPanel(Sender).KeyField);
      if lTextC <> nil then
           TRepartitorPanel(Sender).Text := lNode.Strings[lTextC.Index];
      if lSearchC <> nil then
         TRepartitorPanel(Sender).TextEdit.Text := lNode.Strings[lSearchC.Index];
   end;

  SetDelegat(TRepartitorPanel(Sender).Tag);
end;

procedure TfrmcxTcv.edDelegatButtonClick(Sender: TObject);
begin
  MainForm.OpenDelegati(FPrimitor.FCodRep);
end;

procedure TfrmcxTcv.SetDelegat(const Value: Integer);
var lNode: TdxDBGridNode;
begin
  FDelegat := Value;
  if FDelegat > 0 then begin
     lNode :=  gridDelegati.FindNodeByKeyValue(Value);
     if Assigned(lNode) then begin
      edDelegat.Text  := frmData.QryDelegati.FieldByName('NUME_COMPLET').AsString;
     end;
     SetDocumentField('ID_REPARTITORI_DELEGATI', FDelegat);
  end
  else begin
    SetDocumentField('ID_REPARTITORI_DELEGATI', NULL);
    edDelegat.EditInput.Text := '';
    edDelegat.PopupEdit.Text := '';
  end;
end;

procedure TfrmcxTcv.edDelegatExit(Sender: TObject);
begin
//  SetDelegat(edDelegat.KeyValue);
end;

procedure TfrmcxTcv.SetDelegatiFilter(AFilter: String);
var  NewFiltered : Boolean;
begin
  with frmData.QryDelegati do begin
    NewFiltered := Filter <> AFilter;
    if NewFiltered then Filter := AFilter;
    Filtered := Filter <> '';
  end;
end;

procedure TfrmcxTcv.edMijTransportButtonClick(Sender: TObject);
begin
  MainForm.OpenMijlTransport(FPrimitor.FCodRep);
end;

procedure TfrmcxTcv.SetMijlocTransport(const Value: Integer);
var lNode: TdxDBGridNode;
begin
  FMijlocTransport := Value;
  if FMijlocTransport > 0 then begin
     lNode :=  gridMijTransport.FindNodeByKeyValue(Value);
     if Assigned(lNode) then begin
      edMijTransport.Text  := frmData.QryMijTransport.FieldByName('NUMAR_AUTO').AsString;
     end;
     SetDocumentField('ID_REPARTITORI_TRANSPORT', FMijlocTransport);
  end
  else begin
    SetDocumentField('ID_REPARTITORI_TRANSPORT', NULL);
    edMijTransport.EditInput.Text := '';
    edMijTransport.PopupEdit.Text := '';
  end;
end;

procedure TfrmcxTcv.edMijTransportValidate(Sender: TObject;
  var AKeyValue: Variant);
var
   lNode: TdxDBTreeListNode;
   lTree : TdxDBTreeList;
   lTextC, lSearchC : TdxDBTreeListColumn;
begin
   if not(TRepartitorPanel(Sender).PopupEdit.PopupControl is TdxDBTreeList) then Exit;
   lTree := TdxDBTreeList(TRepartitorPanel(Sender).PopupEdit.PopupControl);
   lNode := lTree.FindNodeByKeyValue(AKeyValue);
   TRepartitorPanel(Sender).Text := '';
   TRepartitorPanel(Sender).TextEdit.Text := '';
   if Assigned(lNode) then begin
      try
        TRepartitorPanel(Sender).Tag := AKeyValue;
      except
        TRepartitorPanel(Sender).Tag := -1;
      end;
      lTextC := lTree.FindColumnByFieldName(TRepartitorPanel(Sender).ListField);
      lSearchC := lTree.FindColumnByFieldName(TRepartitorPanel(Sender).KeyField);
      if lTextC <> nil then
           TRepartitorPanel(Sender).Text := lNode.Strings[lTextC.Index];
      if lSearchC <> nil then
         TRepartitorPanel(Sender).TextEdit.Text := lNode.Strings[lSearchC.Index];
      //SetNextControl;
   end;

//  TRepartitorPanel(Sender).Tag := AKeyValue;
  SetMijlocTransport(TRepartitorPanel(Sender).Tag);
end;

procedure TfrmcxTcv.edMijTransportExit(Sender: TObject);
begin
//  SetMijlocTransport(edDelegat.KeyValue);
end;

procedure TfrmcxTcv.edMijTransportPopupCloseUp(Sender: TObject);
begin
  if GetParentForm(edMijTransport.PopupEdit.PopupControl).ModalResult = mrOk then
    SetMijlocTransport(edMijTransport.KeyValue);
end;

procedure TfrmcxTcv.SetMijlocTransportFilter(AFilter: String);
var  NewFiltered : Boolean;
begin
  with frmData.QryMijTransport do begin
    NewFiltered := Filter <> AFilter;
    if NewFiltered then Filter := AFilter;
    Filtered := Filter <> '';
  end;
end;

procedure TfrmcxTcv.GridItemsiKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
var
  OldEdit : Boolean;
begin
  if (Key in [VK_BACK, VK_DELETE]) and (GridItemsi.FocusedField = QryItemsi.FindField('DETALII_ANGAJAMENT')) then begin
     OldEdit := QryItemsi.State in [dsEdit, dsInsert];
     if not OldEdit then QryItemsi.Edit;
     QryItemsi.FindField('DETALII_ANGAJAMENT').Clear;
     QryItemsi.FindField('COD_FUNCTIONAL').Clear;
     QryItemsi.FindField('COD_ECONOMIC').Clear;
     QryItemsi.FindField('ID_OI_UNITATI').Clear;
     QryItemsi.FindField('ID_OI_PROIECTE').Clear;
     QryItemsi.FindField('ID_ANGAJAMENTE_DEFALCARE').Clear;
     QryItemsi.FindField('ID_ORDONANTARE_DEFALCARE').Clear;
     QryItemsi.Post;
     if OldEdit then QryItemsi.Edit;
  end;
    if (Key in [VK_BACK, VK_DELETE]) and (GridItemsi.FocusedField = QryItemsi.FindField('RepD')) then begin
     OldEdit := QryItemsi.State in [dsEdit, dsInsert];
     if not OldEdit then QryItemsi.Edit;
     QryItemsi.FindField('RepD').Clear;
     QryItemsi.Post;
     if OldEdit then QryItemsi.Edit;
  end;
    if (Key in [VK_BACK, VK_DELETE]) and (GridItemsi.FocusedField = QryItemsi.FindField('RepC')) then begin
     OldEdit := QryItemsi.State in [dsEdit, dsInsert];
     if not OldEdit then QryItemsi.Edit;
     QryItemsi.FindField('RepC').Clear;
     QryItemsi.Post;
     if OldEdit then QryItemsi.Edit;
  end;
  if (Key = VK_SPACE) and (ssCtrl in Shift) then
    TestAndSwitchCantitate;
end;

procedure TfrmcxTcv.btnValidareConexClick(Sender: TObject);
begin
  if IsInSaveDoc then Exit;
  if QryItemsi.State in [dsEdit, dsInsert] then
       QryItemsi.Post;
  ValidareDocument(TButton(Sender).Tag = 1);
  { Salvam noul document }
  if FIdGestDocumConex > -1 then begin
    CopyDocToCulegere(FIdGestDocumConex);
    { Incarcam itemsii }
    ReadDocument;
  end
  else
    Cmd_DelDocExecute(nil);
end;

procedure TfrmcxTcv.TreeTipMaterialDblClick(Sender: TObject);
begin
  with TdxDBTreeList(Sender) do
    if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
end;

procedure TfrmcxTcv.TreeTipMaterialKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if ((Key in [VK_UP, VK_DOWN]) and (ssAlt in Shift)) or
      ((Key = VK_F4) and not (ssAlt in Shift)) or (Key = VK_ESCAPE) then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(False);
  if (Key = VK_RETURN) and (TdxDBTreeList(Sender).FocusedNode <> nil)
     and (not TdxDBTreeList(Sender).FocusedNode.HasChildren) then
     (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
end;

procedure TfrmcxTcv.GridItemsiID_GEST_TIP_MATERIALGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
begin
  if AText <> '' then
     AText := GetInternalTipMat(AText);
end;

procedure TfrmcxTcv.GridItemsiID_GEST_TIP_MATERIALPopup(Sender: TObject;
  const EditText: String);
var
  aProdus : String;
begin
  aProdus := QryItemsi.FieldByName('PRODUS').AsString;
  if (Trim(aProdus) <> '')  and (TreeTipMaterialTIP_PRODUS.Descriptions.Count > 0) then begin
     TreeTipMaterial.Filter.Clear;
     TreeTipMaterial.FullRefresh;
     TreeTipMaterial.Filter.Add(TreeTipMaterialTIP_PRODUS, aProdus,
        TreeTipMaterialTIP_PRODUS.Descriptions[TreeTipMaterialTIP_PRODUS.Values.IndexOf(aProdus)]);
  end;
  InternalPositioning(StringReplace(EditText,'?', '',[]), TdxDBTreeList(TdxDBTreeListPopupColumn(Sender).PopupControl), 'ID_GEST_TIP_MATERIAL');
end;

procedure TfrmcxTcv.GridItemsiID_GEST_TIP_MATERIALCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
var
  lNode : TdxDBTreeListNode;
  lEditabil : Boolean;

    procedure UpdateIfVisible(AFieldName: String);
    var
      lField    : TField;
      lColumn   : TdxDBTreeListColumn;
    begin
      lField  := QryItemsi.FindField(AFieldName);
      if Assigned(lField) then begin
        lColumn := GridItemsi.FindColumnByFieldName(lField.FieldName);
        if lColumn.Visible and not ValueSameValue(lField.Value, QryTipuriMatriale[lField.FieldName]) then
          lField.Value := QryTipuriMatriale[lField.FieldName];
      end;
    end;

var
  lColumn: TdxDBTreeListColumn;

begin
  with TdxDBTreeListPopupColumn(Sender) do begin
    if Accept then begin
      lNode := TdxDBTreeListNode(TdxDBTreeList(PopupControl).FocusedNode);
      if Assigned(lNode) then begin
        lEditabil   := DBGoEdit(Field.DataSet);
        Field.Value := lNode.Id;
        if not ValueSameValue(Field.DataSet['PRODUS'], lNode.Values[TreeTipMaterialTIP_PRODUS.Index]) then
          Field.DataSet['PRODUS'] := lNode.Values[TreeTipMaterialTIP_PRODUS.Index];
        UpdateIfVisible('ContD');
        UpdateIfVisible('ContC');
        lColumn := TreeTipMaterial.FindColumnByFieldName('COD_ECONOMIC');
        if Assigned(lColumn) then begin
          Field.DataSet['COD_ECONOMIC']       := lNode.Values[lColumn.Index];
          Field.DataSet['COD_FUNCTIONAL']     := FDefaultCodFunctional;
          Field.DataSet['ID_OI_UNITATI']      := FDefaultUnitate;
          Field.DataSet['DETALII_ANGAJAMENT'] := FDefalcareBuget.SilentValidateDesc(Field.DataSet['COD_ECONOMIC'], Field.DataSet['COD_FUNCTIONAL'], Field.DataSet['ID_ANGAJAMENTE_DEFALCARE'], Field.DataSet['ID_OI_UNITATI']);
        end;
        IsInLoading := True;
        DBPost(Field.DataSet);
        IsInLoading := False;
        if lEditabil then DBGoEdit(Field.DataSet);
        Text := lNode.Id;
        Accept := False;
      end;
    end;
  end;
end;

procedure TfrmcxTcv.TreeTipMaterialCustomDrawCell(Sender: TObject;
  ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  AColumn: TdxTreeListColumn; ASelected, AFocused, ANewItemRow: Boolean;
  var AText: String; var AColor: TColor; AFont: TFont;
  var AAlignment: TAlignment; var ADone: Boolean);
var
  aProdus : String;
begin

  if TreeTipMaterial.Filter.IsColumnFilterExist(TreeTipMaterialTIP_PRODUS) then Exit;
  aProdus := QryItemsi.FieldByName('PRODUS').AsString;
  if ANode.Strings[TreeTipMaterialTIP_PRODUS.Index]= aProdus then AFont.Color := clBlue
  else AFont.Color := clRed;

end;

procedure TfrmcxTcv.AppendDocToCulegere(const AIdDocument: Integer; IsAppend : Boolean);
begin
  { Copiem documentul in zona tampon pentru modificare }
  DBExecSQLFmt('exec [spGestAppendInCache] %d, %d, %d, %d', [AIdDocument, IdUtilizator, IdDocument, Integer(IsAppend)]);
end;

function TfrmcxTcv.ShowStockOld(ATipMat: String=''): Boolean;
var I: Integer;
    lField: TField;
    lList: TStringList;
begin
  with TfrmSelStock.Create(Application) do
    try
       IdGestDefaDocum := FIdDefaDocum;
       Predator        := FPredator.FCodRep;
       Primitor        := FPrimitor.FCodRep;

       if IsValidDate(edDataDoc.EditValue) then begin
         DataDoc           := edDataDoc.Date;
         DataStoc          := edDataDoc.Date;
         edtDataStock.Date := edDataDoc.Date;
       end
       else begin
         DataDoc           := Date;
         DataStoc          := Date;
         edtDataStock.Date := Date;
         edtChangeDataStoc.Checked := True;
       end;

       TipMat          := ATipMat;
       lList := TStringList.Create;
       try
        GetCodMatList(lList);
        InitStock(lList);
       finally
         lList.Free;
       end;
       OpenStock;
       Result := ShowModal = mrOk;
       if Result then begin
          if ExistsCodMat(DTStock.DataSet.FieldByName('CODMAT').AsInteger) and ErrorOnSameCodMat then begin
             MessageDlg('Acest material a fost deja selectat !'#13#10'Modificat cantitatea la pozitia existenta!'#13#10+
                        'Pentru a anula selectia curenta tastati ESCAPE !', mtError, [mbOk], 0);
             Result := False;
             Exit;
          end;
          if DTStock.DataSet.FieldByName('DATACOD').AsDateTime > DataDoc then begin
             Result := MessageDlg('Data documentului de intrare este mai mica decat data documentului curent !'#13#10+
                                  'Doriti totusi preluarea materialului curent din stock?', mtConfirmation, [mbYes, mbNo], 0) = mrYes;
             if not Result then Exit;
          end;
          { Luam Din Stock }
          IsInLoading := True;
          try
            QryItemsi.Edit;
            for I := 0 to DTStock.DataSet.FieldCount-1 do begin
              if AnsiCompareText(DTStock.DataSet.Fields[I].FieldName, 'CANTITATE') <> 0 then begin
                lField := QryItemsi.FindField(DTStock.DataSet.Fields[I].FieldName);
                if Assigned(lField) then
                   lField.Value := DTStock.DataSet.Fields[I].Value;
              end;
            end;
            QryItemsi.FieldByName('STOCK_BEFORE').AsFloat := DTStock.DataSet.FieldByName('CANT_PREDATOR').AsFloat;
            QryItemsi.FieldByName('CANTITATE').AsFloat := DTStock.DataSet.FieldByName('CANT_PREDATOR').AsFloat;
            QryItemsi.Post;
          finally
            IsInLoading := False;
          end;
          GridItemsi.FocusedField := QryItemsi.FieldByName('CANTITATE');
       end;
    finally
       Free;
    end;
end;

procedure TfrmcxTcv.DoDenMatChange(Sender: TField);
var
//  lAccept: Boolean;
  lQuery     : TZReadOnlyQuery;
  lCntCodMat : Integer;
  lDenTipMat : String;
  ARealDenMat: String;
  ACodMat    : Integer;
//  idGesTipStock : Integer;
  cntTipStock  : Integer;
  lIdGestItems : Integer;
  lSelectStoc : Integer;
begin
   if IsInLoading then
      Exit;
  { Daca avem tipstoc -1 atunci deschidem nomenclatorul
    Daca denumirea se afla in nomenclator trecem peste partea de validare }

  lDenTipMat := GetTipMatFromDenMat(Sender.AsString);
  lCntCodMat := GetCodMatCount(lDenTipMat, ACodMat, ARealDenMat);
  cntTipStock := GetTipuriStocuri(FIdDefaDocum);

  if FExistTratareServer then begin
    if (pos('?', Sender.AsString) > 0) then
      if (cntTipStock > -1) then begin
         ShowStock(GridItemsi.FocusedNode);
         Abort;
      end;
    if QryDocument.State in [dsEdit, dsInsert] then QryDocument.Post;
    if QryItemsi.State in [dsEdit, dsInsert] then QryItemsi.Post;
    lIdGestItems := QryItemsi.FieldByName('ID_CULGEST_ITEMSI').AsInteger;
    lQuery := GetTmpADOQuery;
    with lQuery do
      try
         SQL.Add('exec spGestValidateMaterial ' + IntToStr(lIdGestItems) );
         Open;
         lSelectStoc := -1;
         if not IsEmpty then
           lSelectStoc := Fields[0].AsInteger;
         if lSelectStoc = 1 then begin
           ShowStock(GridItemsi.FocusedNode);
           Abort;
         end
         else if lSelectStoc = -99 then begin //fortam selectie nomenclator
           if SelectFromNomenclator(lDenTipMat) then Abort;
         end
         else if lSelectStoc = 0 then begin
           // nu facem nimik
         end
         else
             raise EContaHandledError.Create('Tipul de produs si de material nu este configurat corect pe documentul curent! Contactati Administratorul !');
      finally
        Free;
      end;
  end
  else begin
   //old stuff ca sa mai mearga la ceilalti clienti

   //  idGesTipStock := GetIDTipStock(FIdDefaDocum);

  if cntTipStock > -1 then
     { Daca avem stock}
     if lCntCodMat <> 1 then begin
        { Daca avem 0 sau mai multe repere, deschidem nomenclatorul }
        ShowStock(GridItemsi.FocusedNode);
        Abort;
     end
     else begin
        { Daca avem exact un reper, incarcam stock-ul maxim si restul pentru reperul respectiv }
        { Daca avem Sender.AsString <> ARealDenMat atunci inseamna ca avem un material nou in cadrul tipului de material }
        if (not SameText(Sender.AsString, ARealDenMat)) or (cntTipStock <> 1) then begin
           if ShowStock(GridItemsi.FocusedNode) then
              Abort;
        end
        else begin
          { Aici ajunge in momentul in care avem un denmat care se potriveste, citim stock-ul si restul elementelor }
          lQuery := GetTmpADOQuery;
          try
            lQuery.Sql.Add('exec spGetStockCodMat :codMat, :IdGestTipStock, :DATA');
            lQuery.Params[0].Value := ACodMat;
            lQuery.Params[1].Value := PStockInfo(ListStockInfo[0])^.IdGestTipStock;
            lQuery.Params[2].Value := edDataDoc.Date;
            lQuery.Open;
            if not lQuery.IsEmpty then begin
              SaveCurentCodMat(lQuery, 'CANTITATE_PREDATOR', 'CANTITATE', True);
              Abort;
            end;
          finally
            lQuery.Free;
          end;
        end;
     end
  else begin
    { Daca avem deschidere de nomenclator facem verificarea completa a denumiririi}
    if pos('?', Sender.AsString) > 0 then
       if SelectFromNomenclator(lDenTipMat) then
          Abort;
  end;
  end;
end;

function TfrmcxTcv.GetTipMatFromDenMat(const ADenMat: String): String;
var
  J : Integer;
begin
  Result := Trim(ADenMat);
  J := pos(' ', Result);
  if J > 0 then
     Result := Trim(Copy(Result, 1, J));
end;

procedure TfrmcxTcv.SaveCurentCodMat(ADataSet: TDataSet; const AStockBefore, AStockField: String; ForceEdit: Boolean = False; const ForceAppend : Boolean = False);
var
  I: Integer;
  lField : TField;
begin
  IsInLoading := True;
  try
    if ForceAppend or ((not ForceEdit) and (not QryItemsi.Locate('ID_CULGEST_DOCUM;CODMAT', VarArrayOf([FIdDocument, ADataSet.FieldByName('CODMAT').AsInteger]), []))) then
       QryItemsi.Append
    else
       QryItemsi.Edit;
    for I := 0 to ADataSet.FieldCount-1 do begin
      if AnsiCompareText(ADataSet.Fields[I].FieldName, 'CANTITATE') <> 0 then begin
         lField := QryItemsi.FindField(ADataSet.Fields[I].FieldName);
         if Assigned(lField) then
            lField.Value := ADataSet.Fields[I].Value;
      end;
    end;
    QryItemsi['DETALII_ANGAJAMENT'] := FDefalcareBuget.SilentValidateDesc(QryItemsi['COD_ECONOMIC'], QryItemsi['COD_FUNCTIONAL'], QryItemsi['ID_ANGAJAMENTE_DEFALCARE']);
    if ADataSet.FindField('PRODUS') = nil then
       QryItemsi.FieldByName('PRODUS').AsString   := 'M';
    QryItemsi.FieldByName('STOCK_BEFORE').AsFloat := ADataSet.FieldByName(AStockBefore).AsCurrency;
    if ADataSet.FieldByName('SEMN_CANTITATE').AsInteger = 0 then //nomenclator
      QryItemsi.FieldByName('CANTITATE').AsFloat := ADataSet.FieldByName(AStockField).AsFloat
    else
      QryItemsi.FieldByName('CANTITATE').AsFloat := ADataSet.FieldByName('SEMN_CANTITATE').AsInteger * ADataSet.FieldByName(AStockField).AsFloat;
    QryItemsi.Post;
  finally
    IsInLoading := False;
  end;
end;

function TfrmcxTcv.GetCodMatCount(TipMat: String; var aCodMat: Integer;
  var aDenMat: String): Integer;
begin
  aCodMat := -1;
  aDenMat := '';
  with GetTmpADOQuery do
    try
      Sql.Add('exec spGetStockTipMat '+QuotedStr(TipMat));
      Open;
      if IsEmpty then
         Result := -1
      else begin
         Result := RecordCount;
         if Result = 1 then begin
           aCodMat := FieldByName('CODMAT').AsInteger;
           aDenMat := FieldByName('DENMAT').AsString;
         end;
      end;
    finally
      Free;
    end;
end;

procedure TfrmcxTcv.GetCodMatList(AList: TStringList);
var
  I: Integer;
begin
  AList.Clear;
  for I := 0 to GridItemsi.Count-1 do
    AList.Add(ValueSafeToStr(GridItemsi.Items[I].Values[GridItemsiCODMAT.Index]));
end;

function TfrmcxTcv.GetIDTipStock(ADefaDocum: Integer): Integer;
begin
  Result := DBGetScallarFmt('exec [spGestGetTipStock] %d', [ADefaDocum]);
end;

function TfrmcxTcv.SelectFromNomenclator(const ATipMat: String): Boolean;
var I : Integer;
begin
  ClearListStockInfo;
  GetTipuriMateriale(FIdDefaDocum);
  { Afisam formularul de selectie denmat, tipmat }
  with TfrmSelStock.Create(Application) do
    try
       InitNomenclator;
       TcxImageComboBoxProperties(GridStockPRODUS.Properties).Items.Clear;
       for I := 0 to GridItemsiPRODUS.Values.Count - 1 do
         with TcxImageComboBoxProperties(GridStockPRODUS.Properties).Items.Add do begin
           Value := GridItemsiPRODUS.Values[I];
           Description := GridItemsiPRODUS.Descriptions[I];
         end;
       QryStock.SQL.Clear;
       QryStock.SQL.Add('exec spGestFullNomenclator :dataDoc, ' + IntToStr(FIdDefaDocum));
       QryStock.Params[0].Value := edDataDoc.Date;
       OpenNomenclator;
       TipMat := ATipMat;
       Result := ShowModal = mrOk;
       if Result then begin
          SetFilterSelectate();
          CopyFromStock(MemStock, 'CANT_PREDATOR', 'CANTITATE_SELECTATA',True);
          GridItemsi.FocusedField := QryItemsi.FindField('CANTITATE');
       end;
    finally
       Free;
    end;
end;

procedure TfrmcxTcv.CopyFromStock(ADataSet: TdxMemData; const AStockBefore,
  AStockField: String;const ForceAppend : Boolean = False);
var
  lIsEdit: Boolean;
begin
  try
    IsInLoading := True;
    lIsEdit := QryItemsi.State = dsEdit;
    if QryItemsi.State in [dsEdit, dsInsert] then begin
       QryItemsi.Cancel;
       if lIsEdit then
          QryItemsi.Delete;
    end;
    ADataSet.First;
    while not ADataSet.Eof do begin
      SaveCurentCodMat(ADataSet, 'CANT_PREDATOR', 'CANTITATE_SELECTATA', False, ForceAppend);
      ADataSet.Next;
    end;
  finally
    IsInLoading := False;
  end;
end;

constructor TfrmcxTcv.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  FEvaluatorDocum := TATSEvaluator.Create(Self);
  FEvaluator      := TATSEvaluator.Create(Self);
  FSumator        := TDBSumList.Create(Self);
end;

procedure TfrmcxTcv.cxDateEdit1PropertiesValidate(Sender: TObject;
  var DisplayValue: Variant; var ErrorText: TCaption; var Error: Boolean);
begin
  if not IsInLoading and IsValidDate(edDataDoc.EditValue) then begin
    SetDocumentField('DATA_DOCUM', edDataDoc.Date);
    SetNextControl;
  end;
end;

function TfrmcxTcv.GetInternalTipMat(AIdGestTipMat: String): String;
var
  lIdTipMat: Integer;
  lError   : Integer;
begin
  Result := AIdGestTipMat;
  Val(AIdGestTipMat, lIdTipMat, lError);
  if lError = 0 then begin
     for lError := 0 to FDenTipMatrList.Count-1 do
      if Integer(FDenTipMatrList.Objects[lError]) = lIdTipMat then begin
         Result := FDenTipMatrList.Strings[lError];
         Break;
      end
  end;
end;

procedure TfrmcxTcv.GridItemsiRepCCloseUp(Sender: TObject; var Text: string;
  var Accept: Boolean);
begin
  Accept := Assigned(TreeRepartitori.FocusedNode);
  if Accept then begin
    Text  := TcxDBTreeListNode(TreeRepartitori.FocusedNode).KeyValue;
  end;
end;

procedure TfrmcxTcv.GridItemsiRepCGetText(Sender: TObject; ANode: TdxTreeListNode;
  var AText: string);
begin
  AText := ValueSafeToStr(memTot.Lookup('ID_REPARTITORI', TdxDBGridPopupColumn(Sender).Field.Value, 'NUME'));
end;

procedure TfrmcxTcv.GridItemsiRepDInitPopup(Sender: TObject);
var
  lValue  : Variant;
  lNode   : TcxDBTreeListNode;
  lGestInt: Boolean;
begin
  if Sender = GridItemsiRepC then
    lGestInt := FPredator.GestInt
  else
    lGestInt := FPrimitor.GestInt;
  if lGestInt then
    SetRepartitorFilter('GESTINT=TRUE')
  else
    SetRepartitorFilter('GESTINT=FALSE');
  lValue  := TdxDBGridPopupColumn(Sender).Field.Value;
  lNode   := TreeRepartitori.FindNodeByKeyValue(lValue);
  if Assigned(lNode) then begin
    lNode.MakeVisible;
    lNode.Focused := True;
  end;
end;

procedure TfrmcxTcv.AppendStocPerioada(const AIdDocument : Integer; DataStoc : TDateTime; Predator : Integer; Cont : String; IsAppend : Boolean);
begin
  { Copiem documentul in zona tampon pentru modificare }
  with GetTmpADOQuery do
    try
       ParamCheck := True;
       Sql.Add('EXEC spGestStocInCache :ID_CULGEST_DOCUM, :ID_UTILIZATOR, :PREDATOR, :DATA_STOC, :CONT, :APPEND');
       Params.ParamByName('ID_CULGEST_DOCUM').Value := AIdDocument;
       Params.ParamByName('ID_UTILIZATOR').Value := IdUtilizator;
       Params.ParamByName('PREDATOR').Value := Predator;
       Params.ParamByName('DATA_STOC').Value := DataStoc;
       Params.ParamByName('CONT').Value := Cont;
       Params.ParamByName('APPEND').Value := IsAppend;
       ExecSql;
    finally
       Free;
    end;
end;

procedure TfrmcxTcv.PopulateSumatorFieldList;
var
  lDataSet: TDataSet;
begin
  FSumatorFieldList.Clear;
  if DBProcExists('spGestFieldsSumator') then begin
    lDataSet := DBNewQuery('exec [spGestFieldsSumator]');
    try
      lDataSet.Open;
      lDataSet.First;
      while not lDataSet.Eof do begin
        FSumatorFieldList.Add(lDataSet.FieldByName('FIELD_NAME').AsString);
        lDataSet.Next;
      end;
    finally
      lDataSet.Free;
    end;
  end;
end;

procedure TfrmcxTcv.Cmd_DelDocExecute(Sender: TObject);
begin
  if not IsInSaveDoc then begin
    DBExecSQLFmt('exec [spGestEmptyDocum] %d, %d', [IdUtilizator, IdDocument]);
    RefreshFirstDocum;
  end;
end;

procedure TfrmcxTcv.Cmd_ModifyDocExecute(Sender: TObject);
var
  lFrmDocum : TfrmGEST_ModifyDocum;
begin
  if IsInSaveDoc then Exit;
  lFrmDocum := TfrmGEST_ModifyDocum.Create(Application);
  try
    lFrmDocum.IsSelection := True;
    if lFrmDocum.ShowModal = mrOk then begin
      if (Self.QryItemsi.Active) and (not Self.QryItemsi.IsEmpty) then
         case MessageDlg('Aveti pozitii introduse in ecranul de culegere !'#13#10'Doriti salvarea acestora?',
                         mtConfirmation, [mbYes, mbNo, mbCancel], 0) of
           mrCancel: Exit;
           mrYes   : ValidareDocument;
           mrNo    : ;
         end;
      FIdParinte   := lFrmDocum.SelectedDocument;
      { Salvam noul document }
      CopyDocToCulegere(FIdParinte);
      { Incarcam itemsii }
      ReadDocument;
    end;
  finally
    lFrmDocum.Free;
  end;
end;

procedure TfrmcxTcv.Cmd_CopyPozExecute(Sender: TObject);
var
  IsDelete : Boolean;
begin
  if IsInSaveDoc then Exit;
  // facem copierea de pozitii asociate unui document introdus
  IsDelete  := True;
  with TfrmGEST_ModifyDocum.Create(Application) do
    try
       if ShowModal = mrOk then begin;
          if (Self.QryItemsi.Active) and (not Self.QryItemsi.IsEmpty) then
             case MessageDlg('Aveti pozitii introduse in ecranul de culegere !'#13#10'Doriti adaugarea celor din document (Yes) sau inlocuirea cu cele din document (No) ?',
                             mtConfirmation, [mbYes, mbNo, mbCancel], 0) of
               mrCancel: Exit;
               mrYes   : IsDelete  := False;
               mrNo    : ;
             end;
          FIdParinte   := SelectedDocument;
          AppendDocToCulegere(FIdParinte, IsDelete);
          { Incarcam itemsii }
          ReadDocument;
       end;
    finally
       Free;
    end;
end;

procedure TfrmcxTcv.Cmd_StocPerioadaExecute(Sender: TObject);
var
  IsDelete : Boolean;
begin
  if IsInSaveDoc then Exit;
  // facem copierea de pozitii asociate unui document introdus
  if (not FPredator.GestInt) or (IdDocument = -1) then Exit;

  IsDelete  := True;
  with TfrmCopyStock.Create(Application) do
    try
       if IsValidDate(edDataDoc.EditValue) then
         DataStoc := edDataDoc.Date
       else
         DataStoc := -1;
       CodPredator := FPredator.FCodRep;
       if ShowModal = mrOk then begin;
          if (Self.QryItemsi.Active) and (not Self.QryItemsi.IsEmpty) then
             case MessageDlg('Aveti pozitii introduse in ecranul de culegere !'#13#10'Doriti adaugarea celor din document (Yes) sau inlocuirea cu cele din document (No) ?',
                             mtConfirmation, [mbYes, mbNo, mbCancel], 0) of
               mrCancel: Exit;
               mrYes   : IsDelete  := False;
               mrNo    : ;
             end;
          AppendStocPerioada(IdDocument,  DataStoc, CodPredator, ContStoc, IsDelete);
          { Incarcam itemsii }
          ReadDocument;
       end;
    finally
       Free;
    end;
end;

procedure TfrmcxTcv.WmSendPostItems(var Message: TMessage);
var OldIsInLoading : Boolean;
begin
  OldIsInLoading := IsInLoading;
  IsInLoading := True;
  if QryItemsi.State in [dsEdit, dsInsert] then
      QryItemsi.Post;
  IsInLoading := OldIsInLoading;
end;

procedure TfrmcxTcv.FormKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
   if (Key= VK_DELETE) then begin
     if (Screen.ActiveControl = edDelegat) then
        SetDelegat(-1);
     if (Screen.ActiveControl = edMijTransport) then
        SetMijlocTransport(-1);
   end;
end;

procedure TfrmcxTcv.DelegatKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if (Key= VK_DELETE) then SetDelegat(-1);
end;

procedure TfrmcxTcv.AutoKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if (Key= VK_DELETE) then SetMijlocTransport(-1);
end;

procedure TfrmcxTcv.FormCloseQuery(Sender: TObject; var CanClose: Boolean);
begin
  if IsInSaveDoc then begin
    MessageDlg('Se salveaza documentul curent ! Nu se poate inchide ecranul !', mtWarning, [mbOK], 0);
    CanClose := False;
  end;
  if QryDocument.State in [dsEdit, dsInsert] then
    QryDocument.Post;

  if QryItemsi.State in [dsEdit, dsInsert] then
    if MessageDlg('Doriti sa salvati pozitia curenta ?', mtConfirmation, [mbYes, mbNo], 0) = mrYes then
       try
         QryItemsi.Post;
       except
         on E: Exception do begin
           CanClose := False;
           ShowEroare('EROARE : '+E.Message);
         end;
       end
    else begin
      QryItemsi.Cancel;
    end;
end;

procedure TfrmcxTcv.edNumarDocButtonClick(Sender: TObject;
  AbsoluteIndex: Integer);
begin
  if QryDocument.State in [dsEdit, dsInsert] then QryDocument.Post;
  if FIdDefaDocum = -1 then
    ShowEroare('Completati predatorul si primitorul pentru a putea selecta contextul documentului ! ')
  else begin
    SetNewDocNumber;
    SetNextControl;
  end;
end;

function TfrmcxTcv.TestUniqueNumber(aIdCulgestDocum : Integer; const OnlyWarning : Boolean): Boolean;
begin
  Result := ValueIsTrue( DBGetScallarFmt('exec [spGestTestDocumentDublu] %d, %d', [aIdCulgestDocum, Integer(OnlyWarning)]) );
end;

procedure TfrmcxTcv.GridItemsiSelectedCountChange(Sender: TObject);
begin
  cst_DeleteAll := False;
end;

procedure TfrmcxTcv.TestAndSwitchCantitate;

  procedure SwitchCantitate;
  begin
    if not (QryItemsi.State in [dsEdit, dsInsert]) then QryItemsi.Edit;
    QryItemsi.FieldByName('CANTITATE').AsFloat := -1 * QryItemsi.FieldByName('CANTITATE').AsFloat;
    QryItemsi.Post;
  end;

var
   I : Integer;
begin
   if GridItemsi.SelectedCount > 0 then begin
     for I := 0 to GridItemsi.SelectedCount -1 do begin
       with QryItemsi do begin
         if not QryItemsi.Locate('ID_CULGEST_ITEMSI', TdxDBGridNode(GridItemsi.SelectedNodes[I]).Id, []) then Continue;
         SwitchCantitate;
       end;
     end;
   end
   else begin
       with QryItemsi do begin
         if  Locate('ID_CULGEST_ITEMSI', TdxDBGridNode(GridItemsi.FocusedNode).Id, []) then SwitchCantitate;
       end;
   end;
end;

function TfrmcxTcv.GetTipuriStocuri(ADefaDocum: Integer): Integer;
var
  lInfo     : PStockInfo;
  lDataSet  : TDataSet;
begin
  lDataSet := DBNewQueryFmt('exec spGestGetTipuriStock %d', [ADefaDocum]);
  try
    lDataSet.Open;
    if lDataSet.IsEmpty then Result := -1
    else begin
      if ListStockInfo = nil then ListStockInfo := TList.Create
      else ClearListStockInfo;
      while not lDataSet.Eof do begin
        New(lInfo);
        lInfo^.IdGestTipStock := lDataSet['ID_GEST_TIP_STOC'];
        lInfo^.Predator       := lDataSet['PREDATOR'];
        lInfo^.Semn           := lDataSet['SEMN'];
        lInfo^.Denumire       := lDataSet['DENUMIRE'];
        lInfo^.Descriere      := lDataSet['DESCRIERE'];
        ListStockInfo.Add(lInfo);
        lDataSet.Next;
      end;
      Result := ListStockInfo.Count;
    end;
  finally
    lDataSet.Free;
  end;
end;

procedure TfrmcxTcv.GridItemsiID_GEST_TIP_MATERIALValidate(Sender: TObject;
  var ErrorText: String; var Accept: Boolean);
var
   lValue : Variant;
   lVal : String;
begin
  if Accept and Assigned(GridItemsi.InplaceEditor) and
     (GridItemsi.InplaceEditor.IsVisible) and
      (GridItemsi.InplaceEditor is TdxInplaceTextEdit)
      and (TdxInplaceTextEdit(GridItemsi.InplaceEditor).Text <> '')
   then begin
     lVal := TdxInplaceTextEdit(GridItemsi.InplaceEditor).Text;
     InternalValidateExact(lVal, TreeTipMaterial, lValue, 2, -1);
     if (lValue <> null) and (lVal <> VarToStr(lValue)) then begin
       TdxInplaceTextEdit(GridItemsi.InplaceEditor).Text := VarToStr(lValue);
       TdxInplaceTextEdit(GridItemsi.InplaceEditor).ValidateEdit;
       TdxInplaceTextEdit(GridItemsi.InplaceEditor).Modified := True;
     end;
     if lValue = null then
       InternalValidateCont(lVal, TreeTipMaterial, lValue);
  end;
end;

procedure TfrmcxTcv.InternalValidateExact(Val: String;
  Tree: TdxDbTreeList; var InternalValue: Variant; const ForceById : Word = 0; const lTag : Integer = -1);
var SelNode : TdxDBTreeListNode;
    lSearchC : TdxDBTreeListColumn;
    lOldSearchType : TdxTLSearchType;
begin
  lSearchC := FindColumnByTag(Tree, lTag);
  Tree.EndSearch;
  { Incercam sa gasim nodul posibil }
  SelNode := nil;
  lOldSearchType := Tree.SearchType;
  Tree.SearchType := stExact;
  if (ForceById <> 1)  and (lSearchC <> nil) then begin
      TCrackATSDBTreeList(Tree).FindNodeByText(lSearchC.Index, Val, sdNone, TdxTreeListNode(SelNode));
  end
  else
     SelNode := Tree.FindNodeByKeyValue(Val);
  if Assigned(SelNode) and not(SelNode.HasChildren) then begin
    if ForceById = 2 then InternalValue := SelNode.Id
    else
       if (lSearchC <> nil) then InternalValue := SelNode.Values[lSearchC.Index]
                            else InternalValue := SelNode.Id;
  end
  else InternalValue := null;
  Tree.SearchType := lOldSearchType;
end;


procedure TfrmcxTcv.InternalValidateCont(Val: String;
   Tree: TdxDbTreeList; var InternalValue: Variant);
var MustDrop: Boolean;
    SelNode : TdxDBTreeListNode;
    lSearchC : TdxDBTreeListColumn;
    lOldSearchType : TdxTLSearchType;
begin
  InternalValue :=  Val;
  lSearchC := FindColumnByTag(Tree, -1);
  Tree.EndSearch;
  { Se valideaza contul de buget introdus }
  MustDrop := (Val = '') or (Val = '?');
  { Incercam sa gasim nodul posibil }
  SelNode := nil;
  lOldSearchType := Tree.SearchType;
  Tree.SearchType := stExact;
  if not MustDrop then begin
     while (not Assigned(SelNode)) and (Length(Val) > 0) do begin
       if lSearchC <> nil then begin
         TCrackATSDBTreeList(Tree).FindNodeByText(lSearchC.Index, Val, sdNone, TdxTreeListNode(SelNode));
       end
       else
         SelNode := Tree.FindNodeByKeyValue(Val);
       if Assigned(SelNode) and not MustDrop then
           InternalValue := SelNode.Values[lSearchC.Index]
       else
       //if not Assigned(SelNode) then
       begin
           MustDrop := True;
           Delete(Val, Length(Val), 1);
       end;
     end;
     if not MustDrop then MustDrop := (not Assigned(SelNode)) or (SelNode.HasChildren);
     if (Assigned(SelNode)) and (MustDrop) then begin
        while SelNode.Count > 0 do SelNode := TdxDBTreeListNode(SelNode.Items[0]);
        SelNode.Focused := True;
     end;
  end
  else begin
     { Mergem Pe Focused Node in jos }
     SelNode := TdxDBTreeListNode(Tree.FocusedNode);
     while (Assigned(SelNode)) and (SelNode.Count > 0) do SelNode := TdxDBTreeListNode(SelNode.Items[0]);
     if Assigned(SelNode) then begin
       SelNode.Focused := True;
       SelNode.MakeVisible;
     end;
  end;
  Tree.SearchType := lOldSearchType;
  if (MustDrop) and (Assigned(GridItemsi.InplaceEditor)) and not IsInLoad then begin
      if SelNode <> nil then begin SelNode.MakeVisible;
        if SelNode.HasChildren then SelNode.Expanded := True; end
      else Tree.StartSearch(-1, Val);
      SendMessage(GridItemsi.InplaceEditor.Handle, CM_DROPDOWNPOPUPFORM, 0, 0);
      Abort;
  end;
end;

procedure TfrmcxTcv.RefreshFirstDocum;
begin
  DBRefresh(QryDocument);
  ReadDocument;
end;

procedure TfrmcxTcv.GridItemsiNR_CHITANTAButtonClick(Sender: TObject;
  AbsoluteIndex: Integer);
var
  lField: TField;
begin
  DoCheckPostDataSet(QryItemsi);
  lField := QryItemsi.FindField('NR_CHITANTA');
  if Assigned(lField) and DBProcExists('spGestNextNrChitanta') then begin
    IsInLoading := True;
    DBSetFieldValue(QryItemsi, 'NR_CHITANTA', ValueSafeToStr( DBGetScallar('exec [spGestNextNrChitanta]') ) );
    IsInLoading := False;
  end;
end;

procedure TfrmcxTcv.pnBottomResize(Sender: TObject);
begin
  BtnCancel.Left := pnBottom.Width - BtnCancel.Width - 5;
  BtnOk.Left := BtnCancel.Left - BtnOk.Width - 2;
  BtnValidare.Left := BtnOk.Left - BtnValidare.Width - 2;
  BevelSeparare.Left := BtnValidare.Left - 4;
  btnValidareConex.Left := BtnValidare.Left - btnValidareConex.Width - 6;
  btnDM.Left := btnValidareConex.Left - btnDM.Width - 6;
end;

procedure TfrmcxTcv.Schimbasemnpozitiecurenta1Click(Sender: TObject);
var Key: Word;
    Shift: TShiftState;
begin
  Key := VK_SPACE;
  Shift := [ssCtrl];
  GridItemsiKeyDown(GridItemsi, Key, Shift);
end;

procedure TfrmcxTcv.CmdAdaugareGridExecute(Sender: TObject);
begin
   TestIdDocument;
   if (GridItemsi.State = tsEditing) then GridItemsi.CloseEditor;
   GridItemsi.ClearSelection;
   GridItemsi.DataSource.DataSet.Insert;
   GridItemsi.ShowEditor;
end;

procedure TfrmcxTcv.CmdStegereGridExecute(Sender: TObject);
begin
  TestIdDocument;
  GridItemsi.DeleteSelection;
end;

procedure TfrmcxTcv.btnDMClick(Sender: TObject);
var
  aRec : PRecTethys;
begin
  if (IsInLoading) or (FTipDocTethys = '') then Exit;
  aRec := SelectieLegatura(True, FTipDocTethys);
  if aRec = nil then Exit;
  SetDocumentField('TethysId',  aRec^.registruID);
end;

procedure TfrmcxTcv.ActivateDMCon(aQry : TDataSet);
begin
  FTipDocTethys := '';
  btnDM.Visible := False;
  if aQry.FindField('TethysTipDoc') <> nil then begin
    if aQry.FieldByName('TethysTipDoc').AsString <> '' then begin
      btnDM.Visible := True;
      FTipDocTethys := aQry.FieldByName('TethysTipDoc').AsString;
    end;
  end;
end;

procedure TfrmcxTcv.ClearListStockInfo;
var
  I : Integer;
begin
  if ListStockInfo = nil then Exit;
  for I := ListStockInfo.Count -1 downto 0 do
    Dispose(PStockInfo(ListStockInfo.Items[I]));
  ListStockInfo.Clear;
end;

procedure TfrmcxTcv.DoCodBaraChange(Sender: TField);
var
  lDataSet    : TZReadOnlyQuery;
  cntTipStock : Integer;
begin
   if IsInLoading then Exit;
  cntTipStock := GetTipuriStocuri(FIdDefaDocum);
  if cntTipStock > -1 then begin
    lDataSet := DBNewQueryFmt('exec spGestGetStockCodBara %s, %d, %s', [Sender.AsString, IdDocument, ValueToStr(edDataDoc.Date)]);
    try
      lDataSet.Open;
      if not lDataSet.IsEmpty then begin
        SaveCurentCodMat(lDataSet, 'CANTITATE_PREDATOR', 'CANTITATE', True);
        Abort;
      end;
    finally
      lDataSet.Free;
    end;
  end;
end;

procedure TfrmcxTcv.TreePlanCustomDrawCell(Sender: TObject; ACanvas: TCanvas;
  ARect: TRect; ANode: TdxTreeListNode; AColumn: TdxTreeListColumn;
  ASelected, AFocused, ANewItemRow: Boolean; var AText: String;
  var AColor: TColor; AFont: TFont; var AAlignment: TAlignment;
  var ADone: Boolean);

  function GetValidColIndex(ACol: TdxTreeListColumn; Tree: TdxDBTreeList): Integer;
  var i: Integer;
  begin
    Result := - 1;
    for i := 0 to Tree.ColumnCount - 1 do
    begin
      Inc(Result);
      if Tree.Columns[i] = ACol then Break;
    end;
  end;

var
  s: String;
begin
  if ANode.HasChildren then AFont.Color := clGray;
  if TdxDBTreeListColumn(AColumn).FieldName = 'FCTCONT' then
  begin
    s := ANode.Strings[GetValidColIndex(AColumn, TreePlan)];
    if s = 'B' then AColor := clSkyBlue
    else if s = 'C' then AColor := clLime
         else if s = 'D' then AColor := clFuchsia;
  end;
end;

procedure TfrmcxTcv.TreePlanKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;

  if ((Key in [VK_UP, VK_DOWN]) and (ssAlt in Shift)) or
      ((Key = VK_F4) and not (ssAlt in Shift)) or (Key = VK_ESCAPE) then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(False);
  if (Key = VK_RETURN) and (TdxDBTreeList(Sender).FocusedNode <> nil)
     and (not TdxDBTreeList(Sender).FocusedNode.HasChildren) then
     (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
end;

procedure TfrmcxTcv.TreePlanDblClick(Sender: TObject);
begin
  with TdxDBTreeList(Sender) do
    if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
    begin
      (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
    end;
end;

procedure TfrmcxTcv.GridItemsiContDPopup(Sender: TObject;
  const EditText: String);
begin
  InternalPositioning(StringReplace(EditText,'?', '',[]), TdxDBTreeList(TdxDBTreeListPopupColumn(Sender).PopupControl));
end;

procedure TfrmcxTcv.GridItemsiContDValidate(Sender: TObject;
  var ErrorText: String; var Accept: Boolean);
var
   lValue : Variant;
   lVal : String;
begin
  if Accept and Assigned(GridItemsi.InplaceEditor) and
     (GridItemsi.InplaceEditor.IsVisible) and
      (GridItemsi.InplaceEditor is TdxInplaceTextEdit)
      and (TdxInplaceTextEdit(GridItemsi.InplaceEditor).Text <> '')
   then begin
     lVal := TdxInplaceTextEdit(GridItemsi.InplaceEditor).Text;
     InternalValidateExact(lVal, TreePlan, lValue, 2, -1);
     if (lValue <> null) and (lVal <> VarToStr(lValue)) then begin
       TdxInplaceTextEdit(GridItemsi.InplaceEditor).Text := VarToStr(lValue);
       TdxInplaceTextEdit(GridItemsi.InplaceEditor).ValidateEdit;
       TdxInplaceTextEdit(GridItemsi.InplaceEditor).Modified := True;
     end;
     if lValue = null then
       InternalValidateCont(lVal, TreePlan, lValue);
  end;
end;

procedure TfrmcxTcv.TreePlanROMANAGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
begin
  AText := Trim(ANode.Strings[TreePlanCONT.Index])+' : '+Trim(AText);
end;

procedure TfrmcxTcv.GridItemsiContDCloseUp(Sender: TObject; var Text: String;
  var Accept: Boolean);
var
  lNode : TdxDBTreeListNode;
begin
  with TdxDBTreeListPopupColumn(Sender) do begin
    if Accept then begin
      lNode := TdxDBTreeListNode(TdxDBTreeList(PopupControl).FocusedNode);
      if Assigned(lNode) then begin
        DBSetFieldValue(Field, lNode.Id, False);
        Text := lNode.Id;
        Accept := False;
      end;
    end;
  end;
end;

procedure TfrmcxTcv.InitCategorii;
begin
  if qryCategorii.Active then qryCategorii.Active := False;
  qryCategorii.Params.ParamByName('ID_GEST_DEFA_DOCUM').Value := FIdDefaDocum;
  qryCategorii.Params.ParamByName('IdUtilizator').Value := IdUtilizator;
  qryCategorii.Active := True;
end;

procedure TfrmcxTcv.GridItemsiCategorieGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
begin
  if AText <> '' then
    AText := GetInternalNumeCategorie(AText);
end;

function TfrmcxTcv.GetInternalNumeCategorie(aIdCategorie: String): String;
var
  lId: Integer;
  lError   : Integer;
begin
  Result := aIdCategorie;
  if not qryCategorii.Active then Exit;
  Val(aIdCategorie, lId, lError);
  if lError = 0 then begin
     if qryCategorii.Locate('idGestCategorii',lId, []) then  begin
        Result := qryCategorii.FieldByName('Denumire').AsString;
      end;
  end;
end;

procedure TfrmcxTcv.GridItemsiCategoriePopup(Sender: TObject;
  const EditText: String);
begin
  InternalPositioning(StringReplace(EditText,'?', '',[]), TdxDBTreeList(TdxDBTreeListPopupColumn(Sender).PopupControl));
end;

procedure TfrmcxTcv.GridItemsiCategorieCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
var
  lNode : TdxDBTreeListNode;
begin
  with TdxDBTreeListPopupColumn(Sender) do begin
    if Accept then begin
      lNode := TdxDBTreeListNode(TdxDBTreeList(PopupControl).FocusedNode);
      if Assigned(lNode) then begin
        DBSetFieldValue(Field, lNode.Id, False);
        Text := lNode.Id;
        Accept := False;
      end;
    end;
  end;
end;

procedure TfrmcxTcv.pageControlChange(Sender: TObject);
begin
  if chkGenAutomat.Checked and (qryNoteDoc.Active = False) then
    btnGenereazaNote.Click;
end;

procedure TfrmcxTcv.btnGenereazaNoteClick(Sender: TObject);
begin
  if QryDocument.State in [dsEdit, dsInsert] then QryDocument.Post;
  if QryItemsi.State in [dsEdit, dsInsert] then QryItemsi.Post;
  qryNoteDoc.Close;
  qryNoteDoc.Params.ParamByName('idCul').Value := IdDocument;
  qryNoteDoc.Active := True;
end;

procedure TfrmcxTcv.InitReadOnlyFields;
begin
  if qryReadOnlyProd.Active then qryReadOnlyProd.Active := False;
  qryReadOnlyProd.Params.ParamByName('ID_GEST_DEFA_DOCUM').Value := FIdDefaDocum;
  qryReadOnlyProd.Active := True;
end;

procedure TfrmcxTcv.GridItemsiChangeNode(Sender: TObject; OldNode,
  Node: TdxTreeListNode);
begin
  DisableEditor(QryItemsi.FieldByName('PRODUS').AsString);
end;

procedure TfrmcxTcv.DisableEditor(aProdus: String);
var
  I : Integer;

  function IsSpecial(aName : String) : Boolean;
  begin
     aName := UpperCase(aName);
     Result := (aName = 'DETALII_ANGAJAMENT')
              or (aName = 'SEMN_CANTITATE')
              or (aName =  'NR_CHITANTA')
              or (aName = 'CATEGORIE')
              ;
  end;

begin
   if not qryReadOnlyProd.Active then Exit;
   qryReadOnlyProd.Locate('ID_GEST_TIP_PRODUSE', -1, []);
   for I := 0 to GridItemsi.VisibleColumnCount -1 do begin
     if not IsSpecial(GridItemsi.VisibleColumns[I].FieldName) then
     if (Pos('~'+UpperCase(GridItemsi.VisibleColumns[I].FieldName)+'~',  qryReadOnlyProd.FieldByName('FIELDS').AsString) > 0) then begin
       TdxDBGridColumn(GridItemsi.VisibleColumns[I]).DisableEditor := True;
       TdxDBGridColumn(GridItemsi.VisibleColumns[I]).ReadOnly := True;
     end
     else  begin
       TdxDBGridColumn(GridItemsi.VisibleColumns[I]).DisableEditor := False;
       TdxDBGridColumn(GridItemsi.VisibleColumns[I]).ReadOnly := False;
     end;
   end;
   if  qryReadOnlyProd.Locate('PRODUS', aProdus, []) then begin
     for I := 0 to GridItemsi.VisibleColumnCount -1 do begin
       if not IsSpecial(GridItemsi.VisibleColumns[I].FieldName) and (Pos('~'+UpperCase(GridItemsi.VisibleColumns[I].FieldName)+'~',  UpperCase(qryReadOnlyProd.FieldByName('FIELDS').AsString)) > 0) then begin
         TdxDBGridColumn(GridItemsi.VisibleColumns[I]).DisableEditor := True;
         TdxDBGridColumn(GridItemsi.VisibleColumns[I]).ReadOnly := True;
       end;
     end;
   end;
end;

procedure TfrmcxTcv.chkGenAutomatPropertiesChange(Sender: TObject);
begin
   SetDocumentField('NOTA_AUTOMAT',chkGenAutomat.Checked);
end;

procedure TfrmcxTcv.TreeRepKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;

  if ((Key in [VK_UP, VK_DOWN]) and (ssAlt in Shift)) or
      ((Key = VK_F4) and not (ssAlt in Shift)) or (Key = VK_ESCAPE) then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(False);
  if (Key = VK_RETURN) and (TdxDBTreeList(Sender).FocusedNode <> nil)
     and (not TdxDBTreeList(Sender).FocusedNode.HasChildren) then
     (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
end;

procedure TfrmcxTcv.TreeRepDblClick(Sender: TObject);
begin
  with TdxDBTreeList(Sender) do
    if (FocusedNode <> nil) {and (not FocusedNode.HasChildren)} then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
end;

procedure TfrmcxTcv.CmdCopyContinutExecute(Sender: TObject);
var
   lValue : Variant;
begin
  if (not GridItemsi.VisibleColumns[GridItemsi.FocusedColumn].ReadOnly ) and (GridItemsi.VisibleColumns[GridItemsi.FocusedColumn].FieldName <> 'DETALII_ANGAJAMENT') and (GridItemsi.FocusedNode <> nil) and (GridItemsi.FocusedColumn >-1) and (GridItemsi.FocusedNode.Index > 0) then begin
    lValue := GridItemsi.Items[GridItemsi.FocusedNode.Index - 1].Values[GridItemsi.VisibleColumns[GridItemsi.FocusedColumn].Index];
    DBSetFieldValue(QryItemsi, GridItemsi.VisibleColumns[GridItemsi.FocusedColumn].FieldName, lValue );
  end;
end;

procedure TfrmcxTcv.GetTipuriMateriale(ADefaDocum: Integer);
var
  SInfo : PStockInfo;
begin
  with GetTmpADOQuery do
    try
      Sql.Add('exec spGestNomenclatorTipMat '+IntToStr(ADefaDocum));
      Open;
      if not IsEmpty then begin
        if ListStockInfo = nil then ListStockInfo := TList.Create
           else ClearListStockInfo;
        while not eof do begin
          New(SInfo);
          with SInfo^ do begin
            IdGestTipMaterial := FieldByName('ID_GEST_TIP_MATERIAL').AsInteger;
            Predator := FieldByName('PREDATOR').AsInteger;
            Semn := FieldByName('SEMN').AsInteger;
            Denumire := FieldByName('DENUMIRE').AsString;
            Descriere := FieldByName('DESCRIERE').AsString;
          end;
          ListStockInfo.Add(SInfo);
          Next;
        end;
      end;
    finally
      Free;
    end;
end;

procedure TfrmcxTcv.CmdGenereazaNumereExecute(Sender: TObject);
begin
  if QryDocument.State in [dsEdit, dsInsert] then QryDocument.Post;
  if QryItemsi.State in [dsEdit, dsInsert] then QryItemsi.Post;
  if AddSeriiFromCurrent(IdDocument, QryItemsi.FieldbyName('ID_CULGEST_ITEMSI').AsInteger) then
    ReadDocument;
end;


procedure TfrmcxTcv.PopulateMemRep;
begin
  DBCopyDataSet(memIntern, 'SELECT ID_REPARTITORI, ID_PARINTE, NUME, ADRESA, GESTINT, COD_FISCAL, TIP_GESTIUNE, CONT FROM REPARTITORI WHERE ISNULL(GESTINT, 1) = 1');
  DBCopyDataSet(memExtern, 'SELECT ID_REPARTITORI, ID_PARINTE, NUME, ADRESA, GESTINT, COD_FISCAL, TIP_GESTIUNE, CONT FROM REPARTITORI WHERE ISNULL(GESTINT,0) = 0');
  DBCloneDataSet(memIntern, memTot, True);
  DBCloneDataSet(memExtern, memTot, False);
end;

procedure TfrmcxTcv.GridItemsiCategorieValidate(Sender: TObject;
  var ErrorText: String; var Accept: Boolean);
var
   lValue : Variant;
   lVal : String;
begin
  if Accept and Assigned(GridItemsi.InplaceEditor) and
     (GridItemsi.InplaceEditor.IsVisible) and
      (GridItemsi.InplaceEditor is TdxInplaceTextEdit)
      and (TdxInplaceTextEdit(GridItemsi.InplaceEditor).Text <> '')
   then begin
     lVal := TdxInplaceTextEdit(GridItemsi.InplaceEditor).Text;
     InternalValidateExact(lVal, TreePlan, lValue, 2, -1);
     if (lValue <> null) and (lVal <> VarToStr(lValue)) then begin
       TdxInplaceTextEdit(GridItemsi.InplaceEditor).Text := VarToStr(lValue);
       TdxInplaceTextEdit(GridItemsi.InplaceEditor).ValidateEdit;
       TdxInplaceTextEdit(GridItemsi.InplaceEditor).Modified := True;
     end;
     if lValue = null then
       InternalValidateCont(lVal, TreePlan, lValue);
  end;
end;

procedure TfrmcxTcv.TestIdDocument;
begin
  DBRefresh(QryDocument);
  if QryDocument.IsEmpty then ReadDocument;
end;

procedure TfrmcxTcv.btnBonFiscalClick(Sender: TObject);
begin
  DoCheckPostDataSet(QryDocument);
  DoCheckPostDataSet(QryItemsi);

  PrintBonFiscal;
end;

procedure TfrmcxTcv.PrintBonFiscal;
begin
  ShowMessage('Nu este configurat corect driverul de Casa de Marcat. Contactati administratorul de sistem pentru configurare ! ');
end;

procedure TfrmcxTcv.Cmd_SpargePeProcenteExecute(Sender: TObject);
var
  lText: String;
  lAccept: Boolean;
begin
  TestIdDocument;
  if (GridItemsi.State = tsEditing) then GridItemsi.CloseEditor;
  GridItemsi.ClearSelection;
  GridItemsiDETALII_ANGAJAMENTInitPopup(GridItemsiDETALII_ANGAJAMENT);
  FDefalcareBuget.Position := poMainFormCenter;
  if FDefalcareBuget.Visible then FDefalcareBuget.Hide;
  lAccept := FDefalcareBuget.ShowModal = mrOk;
  GridItemsiDETALII_ANGAJAMENTCloseUp(GridItemsiDETALII_ANGAJAMENT, lText, lAccept);
end;

procedure TfrmcxTcv.QryItemsiAfterDelete(DataSet: TDataSet);
begin
  UpdateCodEconomic();
end;

procedure TfrmcxTcv.UpdateCodEconomic;
begin
  FDetaliiDocum.UpdateCodEconomic(IdDocument);
end;

procedure TfrmcxTcv.ValoareReceptieChange(Sender: TField);
begin
  TestModificare('Modificare Camp Pozitie', Sender.FieldName, Sender.Value);
  if IsInLoading then Exit;
  if (not IsNewCodMat) and (IsExistCodMat) then begin
    DBExecSQL('exec [spTCVCopiazaProcenteFromNmcl] :ID_CULGEST_ITEMSI, :CODMAT, :CANTITATE, :VALOARE_RECEPTIE_TVA', QryItemsi);
    UpdateCodEconomic();
  end;
end;

procedure TfrmcxTcv.GridItemsiDETALII_ANGAJAMENTCloseQuery(Sender: TObject;
  var CanClose: Boolean);
begin
  CanClose := FDefalcareBuget.CanSelectDefalcare;
end;

procedure TfrmcxTcv.TestModificare(const AOperatie, AFieldName: String;
  const AValue: Variant);
begin
  DBExecSQLFmt('exec [spCheckTCVModificareDocument] %d, %d, %s, %s, %s', [FIdDocument, FIdParinte, ValueToStr(AOperatie), ValueToStr(AFieldName), ValueToStr(AValue)]);
end;

procedure TfrmcxTcv.TestModificare(const AOperatie: String);
begin
  TestModificare(AOperatie, '', Null);
end;

initialization
  lFRStackReports := TStringList.Create;
  lFRStackReports.Sorted := True;
finalization
  lFRStackReports.Free;
end.

