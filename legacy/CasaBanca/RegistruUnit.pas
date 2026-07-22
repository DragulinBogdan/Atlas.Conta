unit RegistruUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ExtCtrls, Menus, ImgList, ActnList, ZDataSet, Db, dxmdaset, dxExEdtr, dxEdLib, dxDBELib,
  StdCtrls, Spin, dxCntner, dxEditor, Buttons, dxGrClEx, dxTL, dxDBCtrl, SyncProgressUnit,
  CommonCasa, CommonDBVar, AcceptTransferUnit, ContainerUnit, MaintenanceUnit, cxControls,
  dxStatusBar, AlopDisponibil, cxButtons, cxContainer, cxEdit, cxCheckBox, dxDBTLCl, dxDBTL,
  cxLookAndFeelPainters, cxGraphics, cxLookAndFeels, ZAbstractRODataset, ZAbstractDataset,
  cxGroupBox, FontGroupBox, ATSDBEvaluator, cxTextEdit, cxMaskEdit,
  cxDropDownEdit, cxImageComboBox, cxCustomData, cxStyles, cxTL, cxCalendar,
  cxCurrencyEdit, cxTLdxBarBuiltInMenu,
  cxDataControllerConditionalFormattingRulesManagerDialog, cxClasses,
  cxInplaceContainer, cxDBTL, cxTLData, cxProgressBar, Vcl.ComCtrls, dxCore,
  cxDateUtils, cxSpinEdit, dxScrollbarAnnotations;

type
  TCrackATSDBTreeList = class(TdxDBTreeList);

  PStersRec = ^TStersRec;
  TStersRec = record
    ID : Integer;
    TableName : String[100];
    NextSters : PStersRec;
  end;

  PHintRec = ^THintRec;
  THintRec = record
    ID : String;
    Denumire : String;
    Explicatie : WideString;
  end;


  TFrmRegistru = class(TForm)
    pnRest: TPanel;
    Splitter2: TSplitter;
    pnTop: TPanel;
    gbInformation: TPanel;
    edCurentHouse: TdxPopupEdit;
    DTRegistru: TDataSource;
    MemRegistru: TdxMemData;
    GridRegistru: TdxDBTreeList;
    GridRegistruCOD_CB: TdxDBTreeListMaskColumn;
    GridRegistruCOD_CasaTransfer: TdxDBTreeListImageColumn;
    GridRegistruDATA: TdxDBTreeListDateColumn;
    GridRegistruEXPLICATIE: TdxDBTreeListMaskColumn;
    GridRegistruACHITAT: TdxDBTreeListCurrencyColumn;
    GridRegistruNRDOC: TdxDBTreeListMaskColumn;
    GridRegistruPOZ: TdxDBTreeListMaskColumn;
    GridRegistruCONT_CSP: TdxDBTreeListPopupColumn;
    GridRegistruCODGEST: TdxDBTreeListPopupColumn;
    GridRegistruTIPDOC: TdxDBTreeListPopupColumn;
    GridRegistruECL: TdxDBTreeListImageColumn;
    GridRegistruSOLD: TdxDBTreeListMaskColumn;
    GridRegistruID_LISTA: TdxDBTreeListMaskColumn;
    GridRegistruSORTFIELD: TdxDBTreeListColumn;
    GridRegistruPROJ: TdxDBTreeListPopupColumn;
    GridRegistruRecId: TdxDBTreeListColumn;
    GridRegistruON_SERVER: TdxDBTreeListImageColumn;
    GridRegistruID_PARINTE: TdxDBTreeListMaskColumn;
    GridRegistruVALIDATA: TdxDBTreeListImageColumn;
    GridRegistruTRANSFER: TdxDBTreeListImageColumn;
    GridRegistruCOD_CBT: TdxDBTreeListMaskColumn;
    GridRegistruCOD: TdxDBTreeListMaskColumn;
    GridRegistruTIP_CHELTVEN: TdxDBTreeListPopupColumn;
    edData: TdxDateEdit;
    QryRegistru: TZQuery;
    Cmd_RegistruCasa: TActionList;
    Cmd_EchilibrarePlata: TAction;
    Cmd_AdaugaPlata: TAction;
    Cmd_DeletePlata: TAction;
    Cmd_SalveazaPlata: TAction;
    Label1: TLabel;
    chkExpand: TCheckBox;
    btnMemo: TSpeedButton;
    btnSwitchFilters: TSpinButton;
    ImaginiEcl: TImageList;

    gpTipDefalcare: TGroupBox;
    rbCont: TRadioButton;
    rbProiect: TRadioButton;
    MemCont: TdxMemData;
    MemReg: TdxMemData;
    MemProj: TdxMemData;
    ImaginiConturi: TImageList;
    CheckList: TImageList;
    GridRegistruPopup: TPopupMenu;
    AdaugaPlataIncasare: TMenuItem;
    StergerePlata: TMenuItem;
    EchilibreazaPlataIncasare: TMenuItem;
    Cmd_TransferaPlata: TAction;
    rbFara: TRadioButton;
    btnRecalcSold: TSpeedButton;
    pnDetail: TcxCollapsedGroup;
    Splitter1: TSplitter;
    DBExplicCont: TdxDBMemo;
    DBExplicProj: TdxDBMemo;
    Transferinaltacasa: TMenuItem;
    Cmd_AcceptaTransfer: TAction;

    AcceptaTransfer: TMenuItem;
    Cmd_Validate: TAction;
    Validare: TMenuItem;
    Cmd_UnValidate: TAction;
    Cmd_AnuleazaTransfer: TAction;
    chkEfectiv: TCheckBox;
    Cmd_GenereazaDiferenta: TAction;
    GenereazaDiferenta: TMenuItem;

    AnuleazaTransfer: TMenuItem;
    pnDecont: TPanel;
    lbNrDec: TLabel;
    edtNrDecont: TcxSpinEdit;
    edtDataDecont: TcxDateEdit;
    edtDetaliiDecont: TcxPopupEdit;
    btnFindDecont: TSpeedButton;
    DTJustificari: TDataSource;
    QryJustificari: TZQuery;
    Label2: TLabel;
    edtSumaDecont: TcxCurrencyEdit;
    Cmd_JustificareAvans: TAction;
    btnDelJust: TSpeedButton;
    btnSaveLocal: TSpeedButton;
    GridRegistruORGANIGRAMA: TdxDBTreeListPopupColumn;
    GridRegistruRESURSA: TdxDBTreeListPopupColumn;
    GridRegistruID_PROIECT: TdxDBTreeListMaskColumn;
    GridRegistruID_TIPURI_CHELTVEN: TdxDBTreeListMaskColumn;
    GridRegistruID_ORGANIGRAMA: TdxDBTreeListMaskColumn;
    GridRegistruID_RESURSA: TdxDBTreeListMaskColumn;
    Cmd_Renumeroteaza: TAction;
    Renumeroteaza: TMenuItem;
    N1: TMenuItem;
    N2: TMenuItem;
    N3: TMenuItem;
    Label4: TLabel;
    Splitter3: TSplitter;
    pnFilter: TPanel;
    chkFilter: TCheckBox;
    lblFilter: TLabel;
    btnSummary: TSpeedButton;
    pnSummary: TPanel;
    SummStatus: TdxStatusBar;
    SelectedSumm: TdxStatusBar;
    DeValidare1: TMenuItem;
    N4: TMenuItem;
    JustificareAvans: TMenuItem;
    btnLegenda: TSpeedButton;
    ValidariList1: TImageList;
    btnErrors: TSpeedButton;
    Cmd_RenumeroteazaAll: TAction;
    RenumeroteazaEcran1: TMenuItem;
    QryShare_Point: TZQuery;
    btnNewDecont: TSpeedButton;
    Cmd_Import: TAction;
    ImportdinaltaCasa1: TMenuItem;
    CmdErrors: TAction;
    Cmd_SaveLocal: TAction;
    Cmd_RecalculateSold: TAction;
    Cmd_ShowDetail: TAction;
    Cmd_ShowSummary: TAction;
    Cmd_ShowLegend: TAction;
    Cmd_ValideazaIesire: TAction;
    StyleController: TdxEditStyleController;
    ValidariList: TImageList;
    Cmd_Flag: TAction;
    FlagInregistrareaCurenta1: TMenuItem;
    GridRegistruNR_DECONT: TdxDBTreeListMaskColumn;
    GridRegistruDATA_DECONT: TdxDBTreeListDateColumn;
    TreeStructura: TdxDBTreeList;
    TreeStructuraCOD_CB: TdxDBTreeListMaskColumn;
    TreeStructuraCOD_PARINTE: TdxDBTreeListMaskColumn;
    TreeStructuraDENUMIRE: TdxDBTreeListMaskColumn;
    TreeStructuraCRSP_LEI: TdxDBTreeListMaskColumn;
    TreeStructuraDENV: TdxDBTreeListMaskColumn;
    TreeStructuraC_O: TdxDBTreeListMaskColumn;
    TreeStructuraDATA_SOLD: TdxDBTreeListDateColumn;
    TreeStructuraCASIER: TdxDBTreeListImageColumn;
    TreeStructuraVALIDATOR: TdxDBTreeListImageColumn;
    TreeStructuraADMIN: TdxDBTreeListImageColumn;
    TreeStructuraIS_BANCA: TdxDBTreeListCheckColumn;
    TreeStructuraIS_AVANS: TdxDBTreeListCheckColumn;
    TreeStructuraIS_TEMPOR: TdxDBTreeListCheckColumn;
    TreeStructuraID_REPARTITORI: TdxDBTreeListMaskColumn;
    QryStructure: TZQuery;
    DTStructure: TDataSource;
    ImagesStructura: TImageList;
    TreeStructuraICON: TdxDBTreeListMaskColumn;
    ImgCasa: TImage;
    Cmd_GotoRecord: TAction;
    GridRegistruC_O: TdxDBTreeListImageColumn;
    GridRegistruV_O: TdxDBTreeListImageColumn;
    GridRegistruV_O_1: TdxDBTreeListImageColumn;
    Cmd_VenitCasa: TAction;
    ImportCasaBanca1: TMenuItem;
    GridRegistruCOD_TRANSFER: TdxDBTreeListMaskColumn;
    PozitionareInregistrare1: TMenuItem;
    btnPreferedHouse: TSpeedButton;
    edNrZile: TdxSpinEdit;
    Cmd_SetBandSize: TAction;
    SetareMarimeBanda1: TMenuItem;
    Cmd_DispozitiePlata: TAction;
    TiparesteDispozitiePlata1: TMenuItem;
    edtTextFiltru: TdxEdit;
    DecontPopupMenu: TPopupMenu;
    Cmd_ReunesteDecont: TMenuItem;
    QryJustificariUpdate: TZQuery;
    CmdDecont: TAction;
    DecontareDocumentFurnizor1: TMenuItem;
    GridRegistruNR_EXTRAS: TdxDBTreeListMaskColumn;
    GridRegistruDATA_EXTRAS: TdxDBTreeListDateColumn;
    TreeStructuraID_VALUTA: TdxDBTreeListMaskColumn;
    TreeStructuraDESCRIERE: TdxDBTreeListMaskColumn;
    GridRegistruCURS_SCHIMB: TdxDBTreeListButtonColumn;
    Cmd_TransferaPozitie: TAction;
    GridRegistruTIP_DOC: TdxDBTreeListColumn;
    GridRegistruDATA_DOCUM: TdxDBTreeListColumn;
    GridRegistruTOTALDOC: TdxDBTreeListColumn;
    MemFact: TdxMemData;
    rbFact: TRadioButton;
    GridRegistruID_REPARTITOR: TdxDBTreeListPopupColumn;
    GridRegistruNR_DOCUM: TdxDBTreeListColumn;
    GridRegistruORDONANTARE: TdxDBTreeListPopupColumn;
    GridRegistruCOD_FUNCTIONAL: TdxDBTreeListColumn;
    GridRegistruCOD_ECONOMIC: TdxDBTreeListColumn;
    GridRegistruCONT_CSP1: TdxDBTreeListColumn;
    GridRegistruID_GEST_DOCUM: TdxDBTreeListColumn;
    GridRegistruORD: TdxDBTreeListPopupColumn;
    pnDeconturi: TPanel;
    Bevel1: TBevel;
    btnOkDecont: TcxButton;
    btnCancelDecont: TcxButton;
    chkDif: TcxCheckBox;
    CmdCopyColumn: TAction;
    Copiazacoloanacurenta1: TMenuItem;
    GridRegistruID_OI_PROIECTE: TdxDBTreeListMaskColumn;
    GridRegistruID_OI_UNITATI: TdxDBTreeListMaskColumn;
    GridRegistruID_ORDONANTARE_DEFALCARE: TdxDBTreeListMaskColumn;
    GridRegistruID_ANGAJAMENTE_DEFALCARE: TdxDBTreeListMaskColumn;
    GridRegistruDETALIIBuget: TdxDBTreeListPopupColumn;
    edListaData: TcxImageComboBox;
    btnAdaugaDecont: TcxButton;
    edtCodGest: TcxPopupEdit;
    Cmd_AdaugaPozitieNoua: TAction;
    Cmd_AdaugaDefalcare: TAction;
    AdaugaPoziteNouaPlataIncasare1: TMenuItem;
    AdaugareDefalcarePlataIncasare1: TMenuItem;
    BtnModificaDecont: TSpeedButton;
    GridRegistruINCASARI: TdxDBTreeListCurrencyColumn;
    GridRegistruPLATI: TdxDBTreeListCurrencyColumn;
    TreeDecontariCOD: TcxDBTreeListColumn;
    TreeDecontariNR_DECONT: TcxDBTreeListColumn;
    TreeDecontariDATA_DECONT: TcxDBTreeListColumn;
    TreeDecontariCODGEST: TcxDBTreeListColumn;
    TreeDecontariCODSECTIE: TcxDBTreeListColumn;
    TreeDecontariNUME: TcxDBTreeListColumn;
    TreeDecontariDATA: TcxDBTreeListColumn;
    TreeDecontariSUMA_DECONT: TcxDBTreeListColumn;
    TreeDecontariCHEIE: TcxDBTreeListColumn;
    TreeDecontariCOD_CBT: TcxDBTreeListColumn;
    TreeDecontariAVANS: TcxDBTreeListColumn;
    TreeDecontariRETURNAT: TcxDBTreeListColumn;
    TreeDecontariJUSTIFICAT: TcxDBTreeListColumn;
    TreeDecontariDIFERENTA: TcxDBTreeListColumn;
    TreeDecontariPROCENT: TcxDBTreeListColumn;
    TreeDecontari: TcxDBTreeList;
    cxStyleRepository1: TcxStyleRepository;
    cxStyle1: TcxStyle;
    cxStyle2: TcxStyle;
    cxStyle3: TcxStyle;
    cxStyle4: TcxStyle;
    cxStyle5: TcxStyle;
    cxStyle6: TcxStyle;
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure FormCreate(Sender: TObject);
     procedure SafePost(DataSet: TDataSet);
    procedure GridRegistruKeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
    procedure edDataDateChange(Sender: TObject);
    procedure GridRegistruCONT_CSPCloseUp(Sender: TObject; var Text: String; var Accept: Boolean);
    procedure GridRegistruCODGESTCloseUp(Sender: TObject; var Text: String; var Accept: Boolean);
    procedure GridRegistruTIPDOCCloseUp(Sender: TObject; var Text: String; var Accept: Boolean);
    procedure Cmd_AdaugaPlataExecute(Sender: TObject);
    procedure GridRegistruCustomDrawCell(Sender: TObject; ACanvas: TCanvas;
      ARect: TRect; ANode: TdxTreeListNode; AColumn: TdxTreeListColumn;
      ASelected, AFocused, ANewItemRow: Boolean; var AText: String;
      var AColor: TColor; AFont: TFont; var AAlignment: TAlignment; var ADone: Boolean);
    procedure GridRegistruChangeNode(Sender: TObject; OldNode, Node: TdxTreeListNode);
    procedure GridRegistruChangeColumn(Sender: TObject; Node: TdxTreeListNode; Column: Integer);
    procedure chkExpandClick(Sender: TObject);
    procedure GridRegistruGetLevelColor(Sender: TObject; ALevel: Integer; var AColor: TColor);
    procedure MemRegistruCalcFields(DataSet: TDataSet);
    procedure btnSwitchFiltersUpClick(Sender: TObject);
    procedure btnSwitchFiltersDownClick(Sender: TObject);
    procedure Cmd_SchimbaDefalcareExecute(Sender: TObject);
    procedure GridRegistruDeletion(Sender: TObject; Node: TdxTreeListNode);
    procedure Cmd_EchilibrarePlataExecute(Sender: TObject);
    procedure pnDetailResize(Sender: TObject);
    procedure gbInformationDblClick(Sender: TObject);
    procedure GridRegistruPROJCloseUp(Sender: TObject; var Text: String;
      var Accept: Boolean);
    procedure TreeProiecteGetSelectedIndex(Sender: TObject;
      Node: TdxTreeListNode; var Index: Integer);
    procedure TreeProiecteDblClick(Sender: TObject);
    procedure TreeProiecteKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure FormDestroy(Sender: TObject);
    procedure Cmd_DeletePlataExecute(Sender: TObject);
    procedure MemRegistruINCASARIChange(Sender: TField);
    procedure GridRegistruKeyPress(Sender: TObject; var Key: Char);
    procedure DBExplicContChange(Sender: TObject);
    procedure Cmd_TransferaPlataExecute(Sender: TObject);
    procedure Cmd_AcceptaTransferExecute(Sender: TObject);
    procedure MemRegistruBeforeDelete(DataSet: TDataSet);
    procedure Cmd_ValidateExecute(Sender: TObject);
    procedure Cmd_UnValidateExecute(Sender: TObject);

    procedure Cmd_AnuleazaTransferExecute(Sender: TObject);
    procedure chkEfectivClick(Sender: TObject);
    procedure GridRegistruCODGESTPopup(Sender: TObject;
      const EditText: String);
    procedure GridRegistruCONT_CSPPopup(Sender: TObject;
      const EditText: String);
    procedure GridRegistruTIPDOCPopup(Sender: TObject;
      const EditText: String);
    procedure GridRegistruPROJPopup(Sender: TObject;
      const EditText: String);
    procedure GridRegistruColumnSorting(Sender: TObject;
      Column: TdxTreeListColumn; var Allow: Boolean);
    procedure GridRegistruCODGESTGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure Cmd_GenereazaDiferentaExecute(Sender: TObject);
    procedure GridRegistruPROJGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure TreeDecontariKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure TreeDecontariDblClick(Sender: TObject);
    procedure Cmd_JustificareAvansExecute(Sender: TObject);
    procedure btnFindDecontClick(Sender: TObject);
    procedure btnDelJustClick(Sender: TObject);
    procedure BitBtn1Click(Sender: TObject);
    procedure GridRegistruTIP_CHELTVENGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure GridRegistruORGANIGRAMAGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure GridRegistruRESURSAGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure Cmd_RenumeroteazaExecute(Sender: TObject);
    procedure edtDetaliiDecontInitPopup(Sender: TObject);
    procedure chkFilterClick(Sender: TObject);
    procedure GridRegistruSelectedCountChange(Sender: TObject);
    procedure GridRegistruDATADateValidateInput(Sender: TObject;
      const AText: String; var ADate: TDateTime; var AMessage: String;
      var AError: Boolean);
    procedure Cmd_RenumeroteazaAllExecute(Sender: TObject);
    procedure btnNewDecontClick(Sender: TObject);
    procedure Cmd_ImportExecute(Sender: TObject);
    procedure CmdErrorsExecute(Sender: TObject);
    procedure Cmd_SaveLocalExecute(Sender: TObject);
    procedure Cmd_RecalculateSoldExecute(Sender: TObject);
    procedure Cmd_ShowDetailExecute(Sender: TObject);
    procedure Cmd_ShowLegendExecute(Sender: TObject);
    procedure Cmd_ShowSummaryExecute(Sender: TObject);
    procedure GridRegistruMouseMove(Sender: TObject; Shift: TShiftState; X,
      Y: Integer);
    procedure GridRegistruPopupPopup(Sender: TObject);
    procedure Cmd_ValideazaIesireExecute(Sender: TObject);
    procedure FlagInregistrareaCurenta1Click(Sender: TObject);
    procedure TreeStructuraKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure edCurentHouseInitPopup(Sender: TObject);
    procedure edCurentHouseCloseUp(Sender: TObject; var Text: String;
      var Accept: Boolean);
    procedure TreeStructuraDblClick(Sender: TObject);
    procedure edCurentHousePopup(Sender: TObject; const EditText: String);
    procedure Cmd_VenitCasaExecute(Sender: TObject);
    procedure Cmd_GotoRecordExecute(Sender: TObject);
    procedure btnPreferedHouseClick(Sender: TObject);
    procedure edNrZileValidate(Sender: TObject; var ErrorText: String;
      var Accept: Boolean);
    procedure edNrZileKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure Cmd_SetBandSizeExecute(Sender: TObject);
    procedure Cmd_DispozitiePlataExecute(Sender: TObject);
    procedure edtTextFiltruKeyPress(Sender: TObject; var Key: Char);
    procedure edtTextFiltruDblClick(Sender: TObject);
    procedure MemRegistruNewRecord(DataSet: TDataSet);
    procedure FormActivate(Sender: TObject);
    procedure TreeStructuraDENUMIREGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure Cmd_ReunesteDecontClick(Sender: TObject);
    procedure TreeDecontariDIFERENTACustomDrawCell(Sender: TObject;
      ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
      AColumn: TdxTreeListColumn; ASelected, AFocused,
      ANewItemRow: Boolean; var AText: String; var AColor: TColor;
      AFont: TFont; var AAlignment: TAlignment; var ADone: Boolean);
    procedure CmdDecontExecute(Sender: TObject);
    procedure GridRegistruCURS_SCHIMBButtonClick(Sender: TObject;
      AbsoluteIndex: Integer);
    procedure Cmd_TransferaPozitieExecute(Sender: TObject);
    procedure GridRegistruTIPDOCValidate(Sender: TObject;
      var ErrorText: String; var Accept: Boolean);
    procedure GridRegistruTIP_CHELTVENValidate(Sender: TObject;
      var ErrorText: String; var Accept: Boolean);
    procedure GridRegistruPROJValidate(Sender: TObject;
      var ErrorText: String; var Accept: Boolean);
    procedure GridRegistruCODGESTValidate(Sender: TObject;
      var ErrorText: String; var Accept: Boolean);
    procedure GridRegistruID_GEST_DOCUMPopup(Sender: TObject;
      const EditText: String);
    procedure GridRegistruORDONANTARECloseUp(Sender: TObject;
      var Text: String; var Accept: Boolean);
    procedure GridRegistruORDCloseUp(Sender: TObject; var Text: String;
      var Accept: Boolean);
    procedure btnOkDecontClick(Sender: TObject);
    procedure btnCancelDecontClick(Sender: TObject);
    procedure chkDifClick(Sender: TObject);
    procedure edDataDateValidateInput(Sender: TObject; const AText: String;
      var ADate: TDateTime; var AMessage: String; var AError: Boolean);
    procedure CmdCopyColumnExecute(Sender: TObject);
    procedure GridRegistruDETALIIBugetInitPopup(Sender: TObject);
    procedure GridRegistruDETALIIBugetCloseUp(Sender: TObject; var Text: string;
      var Accept: Boolean);
    procedure GridRegistruCollapsing(Sender: TObject;
      Node: TdxTreeListNode; var Allow: Boolean);
    procedure edListaDataPropertiesChange(Sender: TObject);
    procedure btnAdaugaDecontClick(Sender: TObject);
    procedure Cmd_AdaugaPozitieNouaExecute(Sender: TObject);
    procedure Cmd_AdaugaDefalcareExecute(Sender: TObject);
    procedure BtnModificaDecontClick(Sender: TObject);
    procedure edtCodGestPropertiesCloseUp(Sender: TObject);
    procedure TreeDecontariNUMEGetDisplayText(Sender: TcxTreeListColumn;
      ANode: TcxTreeListNode; var Value: string);
    procedure edtDetaliiDecontPropertiesCloseQuery(Sender: TObject;
      var CanClose: Boolean);
  private
    FFormula  : TATSFormula;
    FEvaluator: TATSEvaluator;
    FDefalcareBuget: TfrmAlopDisponibil;
    FStartInterval: TDate;
    FEndInterval: TDate;
    FCurentHouse: Integer;
    LocalId : Integer;

    FHintProjList : TStringList;

    FSoldInitial : Currency;
    FIsModified : Boolean;
    FCurentHouseType: TCurentHouseType;
    FIsAvans: Boolean;
    FIsCreditor : Boolean;
    FCodDecont: Integer;
    FSignLogin : Integer;
    FHasDied: Boolean;
    FFilter: String;
    FHouseRigths: TListaDrepturi;
    FIsTemporHouse: Boolean;
    FAfisTransferEdit: TDisplayImageEdit;
    TransferHintNode : TdxTreeListNode;
    FIdValuta: Integer;

    procedure VerificaPozitieEchilibrata(ANode: TdxTreeListNode = nil);
    procedure WmDropDownImgColumn(var Message: TMessage); message WM_DROPDOWN_IMAGECOLUMN;
    procedure WmMoveToGridColIndex(var Message: TMessage); message WM_MOVETOCOLUMNINDEX;
    procedure WmCheckCursSchimb(var Message: TMessage); message WM_CheckCursSchimb;
    procedure WMHideProgress(var message : TMessage); message WM_HideProgress;
     procedure SoftDeleteCurrentRecord;

    procedure ValidareContContabil(Sender: TField);
    procedure ValidareCodRep(Sender : TField);
    function  ValidareDataEx(AData: TDateTime) : Boolean;
    procedure ValidareSumaCopil(Sender : TField);
    procedure ValidareTipDoc(Sender : TField);
    procedure ValidareProj(Sender: TField);
    procedure ValidareCheltVen(Sender: TField);

    procedure InternalValidateCont(Val: String; Tree: TdxDbTreeList; const lTag : Integer = -1);overload;
    procedure InternalValidateCont(Val: String; Tree: TdxDbTreeList; var InternalValue : Variant);overload;
    procedure InternalValidateExact(Val : String; Tree: TdxDbTreeList; var InternalValue : Variant; const ForceById : Word = 0; const lTag : Integer = -1);

    procedure UniteDataSets(var ResultData : TdxMemData; FirstData, SecondData : TdxMemData);
    procedure FilterDefalcareOn(DataSet: TDataSet; var Accept: Boolean);
    procedure FilterDefalcareOff(DataSet: TDataSet; var Accept: Boolean);
    procedure FilterMain(DataSet: TDataSet; var Accept: Boolean);
    procedure GlobalChange(Sender: TField);
    function  GetHouseByIndex(Index : String): String;
    function  GetHouseContByIndex(Index : String): String;
    procedure SetCodDecont(const Value: Integer);
    procedure SetHasDied(const Value: Boolean);
    procedure SetFiltered(const Value: String);

    function SetVALIDATAonRecord(IDLista : String; ValidValue : Integer) : Boolean;

    function GetFullNodeString(aId : Integer; aIndex : Integer;  TreeList : TdxDBTreeList) : WideString;
    //function GetNumeUtilizator(IdUtilizator : Integer) : Integer;

    //o sa o folosesc sa aflu urmatorul node de unde incepe Kalupul dupa data
    function GetNextDataNode(CurrentNode : TdxTreeListNode):TdxTreeListNode;
    procedure RenumeroteazaNode(aNode: TdxTreeListNode);
    function GetLocalId : Integer;
    procedure SetAfisTransferEdit(const Value: TDisplayImageEdit);
    function CanDoValidation(Validation : Boolean) : Boolean;
    procedure SetCurentHouse(Id : Integer);
    procedure SetCurentDecont(aNrDecont : Integer; aDataDecont : TDatetime; aCodGest : Integer);
    function GetCurentHint: String;
    function GetValCodDecont: Variant;

  protected
    function  GetDefalcareType: Integer;
    procedure AdaugaDecontNou(ANrDecont, ADataDecont, ACodGest, ASumaDecont: Variant);
    procedure ModificaDecont(ANrDecont, ADataDecont, ADataRegistru, ACodGest, ASumaDecont: Variant);
    //punem pe dataset dupa cheia de cautare valoarea pentru camp
    procedure PostOnParent(MemSearch : TDataSet; aKey : String; aFieldName : String; aValue : Variant);

    procedure RefreshQueryDeconturi;
    procedure ClearDecontInfo;

    {validarea sumei pentru copil}
    procedure ValidateSumeEcl(Node: TdxTreeListNode; NewValue: Currency);
    {procesarea mesajului care verifica echilibraqrea defalcarii}
    procedure SetStareCurentaNota(var Message: TMessage); message WM_SET_STARE_SOLD;
    {genereaza schimbare datei de inceput si ia in calcul toate variabilele care tin de data}
    procedure ChangePeriod(var Message: TMessage); message WM_SET_DATA;
    {face suma absoluta inte plati si incasari}
    function  GetUnitedValue(aNode : TdxTreeListNode): Currency;
    {face suma dintre -plati + incasari}
    function  GetRealLineValue(aNode : TdxTreeListNode): Currency;
    {face suma dintre -abs(plati) + abs(incasari)}
    function  GetRealAbsoluteLineValue(aNode : TdxTreeListNode): Currency;
    {face disabled editorul pentru una dintre coloanele plati sau incasari}
    procedure DisableEditor(aNode : TdxTreeListNode);
    {acorda drepturile specifice pe fiecare utilizator}
    procedure SetRights(aRightType : TListaDrepturi);
    {intoarce numele campului completat PLATI sau INCASARI}
    function  GetFieldIndex(aNode: TdxTreeListNode) : String;
    {procedura sincronizeaza doua dataseturi pe baza unei conditii}
    procedure SyncDataSet(FromDataSet : TdxMemData; var ToDataSet : TdxMemData; Condition : Boolean);
    {functia returneaza soldul initial in functie de parametrii de start}
    function GetSoldInitial:Currency;
    {procedura care creaza forma pentru progress si asigneaza evenimentul pentru incaracare}
    procedure CreateProgressState;
    {procedura pentru progress general}
    procedure ProgressDataset(DataSet: TDataSet; Progress, MaxProgress: Integer);
//    procedure ProgressADODataset(DataSet: TDataSet; Progress, MaxProgress: Integer; var EventStatus: TEventStatus);
    {procedura initializeaza modul de validate}
    procedure InitCulegere;
    {procedura incarca in amemdataset tipul de defalcare}
    procedure LoadRecordSet(aMemDataset : TdxMemData; TipLista : TTipLista);
    {muta nodul aNode cu n Direction}
    function  MoveBy(aNode : TdxTreeListNode; Direction :Integer) : Boolean;
    {pregateste defalcarea}
    procedure DoSetDataSet(TipLista : TTipLista);
    {adauga o noua linie setului de date}
    procedure AdaugaMemRegistru(aNode : TdxDBTreeListNode; ForcedEntry : Boolean = False);
    procedure AdaugaMemRegistruNew(const aNodeInfo : TLineNodeInfo; ForcedEntry : Boolean = False);
    procedure GetLineInfo(const aNode : TdxDBTreeListNode; var aLineInfo : TLineNodeInfo);

    function GetActionDate : TDateTime;
    {adauga inregistrari pentru fiecare proiect selectat din arbore}
    procedure AdaugaCopiiProj(aList : TStringList);
    //adaugare  de noduri copii pentru partea de proiecte
    procedure AdaugaCopiiProjCamp(aList : TStringList; aFieldName : String; const lNode : TdxDBTreeListNode = nil );
    procedure AdaugaCopiiBugetCamp(aList : TStringList; aFieldName : String; const ANode : TdxDBTreeListNode = nil );
    // adaugare de noduri copii pentru partea de facturi
    procedure UpdateItemsiDecont (oldId, NewId : integer);
    {stabileste ordinea operatiilor in zi si in cadrul aceluiasi parinte}
    function GetNextPosition(aData : TDateTime; ParentID : String = '') : Integer;

    procedure SHARE_MOVE(Id, NewId : Integer; Table_Name : String; Id_Utilizator : Integer);

    //transforma campul ID din local in nr de pe server
    procedure CompleteIdentity(aQry : TZQuery; Tbl_name : String; IdField : String; IsTransfer : Boolean; aCont, aProj, AFact : TZQuery);
    procedure CompleteNewIds(aQry : TZQuery; Tbl_name : String; IdField : String);
    procedure CompleteNewIdsF(aQry: TZQuery; Tbl_name,  IdFieldS, IdFieldD: String);
    //actulizare pentru tabelele copii pentru campul parinte luat de pe server
    procedure UpdateChild(aQry :  TZQuery; lOldId, lNewId : Integer);
    procedure UpdateChildF(aQry :  TZQuery; lOldId, lNewId : Integer);

    function GetParentKeyId(aMemData : TdxMemData; aKey : String) : Integer;
    procedure GetTipCasa(aId : Integer; var aTipCasa : TTipCasa);
    procedure FreeAndNilCasa(var aTipCasa : TTipCasa);
    procedure RefreshStructure;
    {procedura de afisare dispozitie de plata}
    procedure StructuraActualizare(aNode : TdxDBTreeListNode);
    // calculul de hash pentru radul curent transmis la program
    function CalcRowHash(GUID : String; aQry : TZQuery) : String;
  public
    constructor Create(AOwner: TComponent); override;

    procedure UpdateSoldCasa;
  public
    {marcatori vizuali}
    {pentru cod}
    CodCurent   : Variant;
    {pentru data}
    DataEmitere : TDateTime;

    {daca se aduce un set de date de pe server nu se mai face verificarea}
    InternalAdd : Boolean;

    {daca trebuie reactualizat pe server}
    NeedServer : Boolean;

    {daca este in sincronizare de defalcari}
    IsSyncro : Boolean;

    {marcheaza faptul ca nu se face validare asupra datelor la incarcare}
    IsInLoad : Boolean;

    {marcheaza faptul ca se adauga o inregistrare noua in dataset}
    IsInAdd : Boolean;

    {defalcarea curenta  fara, conturi, proiecte}
    CurentrbTag : Integer;
    CurentPoz   : Integer;

    {detalii progress}
    ProgressForm : TfrmProgress;
    ProgressCaption : String;

    {pentru Decontari}
    PersoanaDecont : String;
    CodCasaDecont  : Integer;

    BusealaStearsa : Boolean;

    LegendForm : TFrmSettings;

    TotalIncasari, TotalPlati : Currency;
    FiltratIncasari, FiltratPlati : Currency;
    SelectedIncasari, SelectedPlati : Currency;
    SelectedIncasariChild, SelectedPlatiChild : Currency;
    TotalCount : Integer;

    DeconturiList : TStringList;

    RecValidate : PStersRec;
    {pentru id-urile sterse}
    RecSterse : PStersRec;
    procedure AddToDeleted(aID : Integer; aTableName : String);
    procedure ProcessDeleted;

    {adaugam la validare si inreg din alte case}
    procedure AddToValidation(Cod : Integer; Validare : Boolean = True);
    procedure SaveValidation;

    procedure SaveNow;

    {mutain arhiva tabelei atablename id -ul }
    procedure MoveToArchive(aTableName : String; aId, aNewId : Integer; Sign : Integer = -1);


    procedure LocalSyncronizeSharePoint;

    {verifica daca sunt buseli}
    function CheckForFiles : Boolean;
    procedure MoveSavesToArchive(Id : Integer);

    { Verifica conditiile inainte de salvare }
    procedure VerifyBeforeSave;

    procedure GetDeconturiList(var lDeconturiList : TStringList);
    {face reactualizarea setului de date de pe server in cazul modificarii parametriilor}
    procedure RefreshDataSet;
    {salveaza modificariile}
    procedure SaveToDb;
    {salveaza tabelele local}
    procedure SaveToLocalComputer;
    {incarca tabelel din local}
    procedure LoadFromLocalComputer;
    {cacluleaza soldul de la nodul curent in jos}
    procedure CalculateSold(aNode :  TdxTreeListNode);
    {sterge proiectele checkuite}
    procedure ClearCheckState(aTree :TdxDBTreeList;  aCheckState : TStringList);
    {face sincronizarea intre defalcari}
    procedure SyncronizeDataSets(NeedServerSupport : Boolean; TipDefalcare : Integer);
    {adauga a ceea ce s-a lucrat local pe server}
    procedure AddRecordsToDB;
    {sterge share-ul la iesire si face update-ul in tabela de conflicte}
    procedure DeleteShare(Sign : Integer=-1);

    {refresh Deconturi}
    procedure RefreshDeconturi;
    {creaza noile pozitii in tabele}
    function CompleteDataSetForDB(
        aGeneratedId: String;
        aNewId: Integer; aTip: TTipLista;
        const aData: TZQuery; aMemData: TdxMemData; aParentId : Integer;
        const aTransferID : Integer=0): Integer;
    {la schimbarea unui detaliu de casa se face trecerea pentru sincronizare}
    procedure AssignOnChange(State : Boolean);
    {transfer intre casa donor si casa acceptor in memregistru}
    procedure MakeTransfer(aNode : TdxTreeListNode; CasaAcceptor : Integer; DataAcceptare : TDateTime; Suma : Currency;
       Stare, StareAcceptor : Integer; IsAccepted : Boolean;
       aNrDecont : Integer; aDataDecont : TDateTime);
    {procedura completeaza date pentru transfer}
    function  PuneTransfer(aMemData : TdxMemData; aQry : TZQuery) : Integer;

    {Calcul toatal}
    function  CalculateTotal(aMemDataSet : TdxMemData; aFieldName : String) : Currency;
    procedure CalculateTotalFix(aMemDataSet : TdxMemData; var Incasari, Plati: Currency);

    procedure SetMainFilter(Filter : String; State : Boolean);
    procedure CheckForSave(Force : Boolean = False);

    //procedure GotoCod(Cod : Integer; CodCB : Integer; Data : TDateTime);
    procedure GotoCod(var Message : TMessage); message WM_LOCALIZE;
    {properties}
    property  CurentHint       : String            read GetCurentHint;
    property  StartInterval    : TDate             read FStartInterval    write FStartInterval;
    property  EndInterval      : TDate             read FEndInterval      write FEndInterval;
    property  CurentHouse      : Integer           read FCurentHouse      write FCurentHouse;
    property  CurentHouseType  : TCurentHouseType  read FCurentHouseType  write FCurentHouseType;
    property  HouseRigths      : TListaDrepturi    read FHouseRigths      write FHouseRigths;
    property  IsTemporHouse    : Boolean           read FIsTemporHouse    write FIsTemporHouse;
    property  SoldInitial      : Currency          read FSoldInitial      write FSoldInitial;
    property  IsModified       : Boolean           read FIsModified       write FIsModified;
    property  IsAvans          : Boolean           read FIsAvans          write FIsAvans;
    property  CodDecont        : Integer           read FCodDecont        write SetCodDecont;
    property  ValCodDecont     : Variant           read GetValCodDecont;
    property  HasDied          : Boolean           read FHasDied          write SetHasDied;
    property  Filter           : String            read FFilter           write SetFiltered;
    property  IdValuta         : Integer           read FIdValuta        write  FIdValuta;
    property  AfisTransferEdit : TDisplayImageEdit read FAfisTransferEdit write SetAfisTransferEdit;
 end;

{
  IN BAZA DE DATE

  'COD_CB', - cod casa banca - camp folosit intern
  'COD', - identificator unic al tranzactiei - pe server este id in local creste de la 1 la ... in functie de nr de adaugari
  'SOLD', - sold pana la inregistrarea curenta
  'VAL_CRSP', - nefolosit vazut fox
  'ACHITAT',  - in caz ca a fost avans la ats arata rest de plata
  'CURS_SCHIM',  -cursul de schimb nefolosit
  'ECL',  - echilibrat
  'DATAEM',  - data modificarii/ operatiei
  'C_O',  - codul de operaror
  'V_O',  - codul de operator la validare
  'NR_LIST'  - este folosit pentru imperechere cu actele din celelate module
  'COD_CBT', - cod case destinatie in cazul unui transfer
  'TRANSFER', - cod transfer vezi documentatie de la 0 - 13
  'COD_TRANSFER', - codul inregistrarii copie din casa de destinatie
  'NR_DECONT',  - numarul de decont in caz ca trensferul este o justificare avans
  'DATA_DECONT',  - data decontrului
  'PARENT_COD', - pentru justificare avans in casa de justificare se foloseste campul pentru a defalca copii relativ la o sursa de alimentare
  'VALIDATA' - inregistrarea curenta este validata
  'VALIDATION_HASH' - hash de validare al inregistrarii


  'CODGEST', - campul de identificare al repartitorului
  'DATA', - data tranzactiei
  'TIPDOC', - identificator tipului de document
  'NRDOC',  - numarul de document
  'POZ',  - pozitia in cadrul zilei ( trebuie sa se poata modifica)
  'EXPLICATIE', - explicatie scurta care apare in registru tiparit
  'INCASARI', - intrari
  'PLATI', - iesiri
  'MEXPLIC',  - camp memo de explicat


  'CONT_CSP', - cont contabil corespondent
}



{
  PE CLIENT

  'ID_LISTA' - identificator unic din cauza ca am id-uri pe server si local
            se compune din 1.COD  sau 0.Cod Generat Local
  'ID_PARINTE' - parintele din lista

  'COD_CB', - cod casa banca - camp folosit intern
  'COD', - identificator unic al tranzactiei - pe server este id in local creste de la 1 la ... in functie de nr de adaugari
  'SOLD', - sold pana la inregistrarea curenta
  'VAL_CRSP', - nefolosit vazut fox
  'ACHITAT',  - in caz ca a fost avans la ats arata rest de plata
  'CURS_SCHIM',  -cursul de schimb nefolosit
  'ECL',  - echilibrat
  'DATAEM',  - data modificarii/ operatiei
  'C_O',  - codul de operaror
  'V_O',  - codul de operator la validare
  'NR_LIST'  - este folosit pentru imperechere cu actele din celelate module
  'COD_CBT', - cod case destinatie in cazul unui transfer
  'TRANSFER', - cod transfer vezi documentatie de la 0 - 13
  'COD_TRANSFER', - codul inregistrarii copie din casa de destinatie
  'NR_DECONT',  - numarul de decont in caz ca trensferul este o justificare avans
  'DATA_DECONT',  - data decontrului
  'PARENT_COD', - pentru justificare avans in casa de justificare se foloseste campul pentru a defalca copii relativ la o sursa de alimentare
  'VALIDATA' - inregistrarea curenta este validata
  'VALIDATION_HASH' - hash de validare al inregistrarii
  'DATA_ACCEPT' - data acceptarii transferului (doar in ecran)  in cazul unui transfer cu data destinatie precizata

  'DATA', - data tranzactiei
  'TIPDOC', - identificator tipului de document
  'NRDOC',  - numarul de document
  'POZ',  - pozitia in cadrul zilei ( trebuie sa se poata modifica)
  'EXPLICATIE', - explicatie scurta care apare in registru tiparit
  'INCASARI', - intrari
  'PLATI', - iesiri

  'MEXPLIC',  - camp memo de explicat
  'CONT_CSP', - cont contabil corespondent
  'CODGEST', - campul de identificare al repartitorului

  'ON_SERVER' - variabila dupa care se face scrierea sau nu pe server a informatiei
  'SOLD_NOU' ??
  'SORTFIELD' - camp de sortare a ordinii in grid (calculat vezi on calc)

  'PEXPLIC' - explicatie defalcare pe proiect

  'ID_PROIECT' - cod proiect
  'ID_TIPURI_CHELTVEN' - tipul de cheltuiala in timp a devenit plan functional
  'ID_ORGANIGRAMA' - functia
  'ID_RESURSA' - numele resuseri

}

var
  StrActualizare : PStrActualizare;

  CasierViewRight : array[1..11] of String[20]
      = ('DATA', 'INCASARI', 'PLATI', 'EXPLICATIE', 'TIPDOC', 'NRDOC', 'POZ', 'MEXPLIC', 'CODGEST', 'NR_EXTRAS', 'DATA_EXTRAS');
  ContabilViewRight : array[1..8] of String[20]
      = ('CONT_CSP', 'MEXPLIC', 'CODGEST', 'PEXPLIC', 'ID_PROIECT', 'ID_TIPURI_CHELTVEN', 'ID_ORGANIGRAMA', 'ID_RESURSA');
//  ValidatorViewRight  : array[1..1] of String[20]
//      = ();

  TransferBlock : array[1..5] of String[20]
     = {pot edita}('CONT_CSP', 'ID_PROIECT', 'ID_TIPURI_CHELTVEN', 'ID_ORGANIGRAMA', 'ID_RESURSA');
      // nu pot edita ('DATA', 'INCASARI', 'PLATI', 'EXPLICATIE', 'TIPDOC', 'NRDOC', 'MEXPLIC');
  TransferLocalBlock : array[1..5] of String[20]
     = {pot edita}('CONT_CSP', 'ID_PROIECT', 'ID_TIPURI_CHELTVEN', 'ID_ORGANIGRAMA', 'ID_RESURSA');
      // nu pot edita ('DATA', 'INCASARI', 'PLATI', 'EXPLICATIE', 'TIPDOC', 'NRDOC', 'MEXPLIC');

  ValidationBlock : array[1..5] of String[20]
     = ('CONT_CSP', 'ID_PROIECT', 'ID_TIPURI_CHELTVEN', 'ID_ORGANIGRAMA', 'ID_RESURSA');

{  NoCustomizingInternal : array[1..5] of String[20]
     = ('ID_LISTA', 'ID_PARINTE', 'COD_CB', 'COD', 'VAL_CRSP', 'CURS_SCHIM', 'DATAEM', 'C_O', 'V_O', );}

implementation

uses
  CasaUnit, dxCompsUtile, DateUnit, ConflictUnit, TransferUnit, FileCtrl, ErrorUnit, Variants,
  DetaliiDecontUnit, MessagesUnit, Math, MD5, ATSZDBUtils, AlegDecontUnit,
  ZeosDBUtile, DecontariUnit, AlopLichidare, unitMemTableEx;

{$R *.DFM}

procedure TFrmRegistru.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;
procedure TFrmRegistru.SafePost(DataSet: TDataSet);
begin
  if Assigned(DataSet) and (DataSet.State in [dsEdit, dsInsert]) then
    DataSet.Post;
end;
procedure TFrmRegistru.FormCreate(Sender: TObject);
var
  lNode     : TdxTreeListNode;
  lDataSet: TDataSet;
begin
  {setam cautare in grid partial}
  {setam locul de unde se iau parametrii gridului}
  lDataSet := DBNewQuery('select * from .dbo.fnModelCBStructura()');
  try
    lDataSet.Open;
    MemRegistru.CreateFieldsFromDataSet(lDataSet);
  finally
    lDataSet.Free;
  end;
      if not Assigned(MemRegistru.FindField('STARE')) then
begin
  with MemRegistru do
  begin
    Open;
    FieldDefs.Add('STARE', ftInteger, 0, False);
    Close;
    Open;
  end;
end;
  IsRightEnable := IsRightEnable and not IsAdmin;
  FHintProjList := TStringList.Create;
  IsInAdd := False;

{  if ParamStr(1) = 'update' then begin
    DoUpdateStructure;
  end;
 }

  New(StrActualizare);
  ZeroMemory(StrActualizare, SizeOf(TStrActualizare));

  CurentSaveDir :=  ExtractFileDir(Application.ExeName);
  CurentSaveDir := GetNextDir(CurentSaveDir);
  FSignLogin := CommonDBVar.IdLogin;
  FHasDied   := False;
  BusealaStearsa := False;

  DeconturiList := TStringList.Create;

  FIsModified := False;
  RecSterse := nil;
  RecValidate := nil;

  {stabilim defalcarea curenta}
  CurentrbTag := 0;
  {configuram progresul pentru incarcarea datelor}
  CreateProgressState;
  {citim setariile pentru culori}
  ReadSettingsRegistru;

  frmCasaContainer.TreePlan.SearchType := ModDeCautare;
  frmCasaContainer.TreeRepartitori.SearchType := ModDeCautare;
  frmCasaContainer.TreeTipDoc.SearchType := ModDeCautare;

  try
    if QryStructure.Active then QryStructure.Active := False;
    QryStructure.Params.ParamByName('COD_UTILIZATOR').Value := IdUtilizator;
    QryStructure.Params.ParamByName('IS_ADMIN').Value := IsAdmin or not IsRightEnable;
    if ModAfisTree then
       QryStructure.Params.ParamByName('DISP_WAY').Value := 1
    else
       QryStructure.Params.ParamByName('DISP_WAY').Value := 0;
    QryStructure.Open;

    QryStructure.Filter := 'COD_CB > 0';
    QryStructure.Filtered := True;
    PopulateImage(QryStructure, GridRegistruCOD_CasaTransfer.Values, GridRegistruCOD_CasaTransfer.Descriptions, 'COD_CB', 'DENUMIRE');
    PopulateImage(FrmData.QryOperatori, GridRegistruC_O.Values, GridRegistruC_O.Descriptions, 'ID_UTILIZATORI', 'NUMEINTREG');

(*CHRIS SALVARE FACTURI
    frmData.QryOrdCassa.Close;
    frmData.QryOrdCassa.Open;
    frmData.QOrdonantari.Close;
    frmData.QOrdonantari.Open;
*)

//    frmData.QryOrdCassa.Filter := 'REST_PLATA<>0';
//    frmData.QryOrdCassa.Filtered := True;

    GridRegistruV_O.Values.Assign(GridRegistruC_O.Values);
    GridRegistruV_O.Descriptions.Assign(GridRegistruC_O.Descriptions);

    GridRegistruV_O_1.Values.Assign(GridRegistruC_O.Values);
    GridRegistruV_O_1.Descriptions.Assign(GridRegistruC_O.Descriptions);

    QryStructure.Filtered := False;
    QryStructure.Filter := '';


    {facem disabled evenimentul de schimbare}
    edData.OnDateChange := nil;
    {setm parametrii de deschidere}

    if (IsSaveDataStart) and (Saved_DataCasa > 0) then
      edData.Date := Saved_DataCasa
    else
      edData.Date := Date;

    if IsSaveTipDefalcare then
      Case Saved_TipDefalcare of
        0 : rbFara.Checked := True;
        1 : rbCont.Checked := True;
        2 : rbProiect.Checked := True;
      end;

    if IsSavePeZi then
       chkEfectiv.Checked := Saved_PeZI;

    if IsSaveZileAnt then begin
      edNrZile.OnValidate := nil;
      edNrZile.Value := Saved_ZileAnt;
      edNrZile.OnValidate := edNrZileValidate;
    end;

    FStartInterval := edData.Date;
    FEndInterval   := 0;
    if chkEfectiv.Checked then FEndInterval := edData.Date
    else FEndInterval   := 0;
    if chkEfectiv.Checked then
      if edNrZile.IntValue < 0 then FEndInterval := FEndInterval - edNrZile.IntValue
      else if edNrZile.IntValue > 0 then FStartInterval := FStartInterval -  edNrZile.IntValue;

    pnDecont.Visible := False;
    pnTop.Height := 40;

    {punem la loc evenimentele}
    edData.OnDateChange := edDataDateChange;

    lNode := TreeStructura.TopNode;
    while (lNode <> nil) and (lNode.HasChildren) do begin
      if lNode.HasChildren then
        lNode := lNode.GetFirstChild
      else
        lNode := nil;
    end;

    if Assigned(lNode) then begin
      FCurentHouse := PreferedHouseId;
      if FCurentHouse = -1 then
        FCurentHouse := DefaultHouseId;
      if FCurentHouse = -1 then
        FCurentHouse := TdxDbTreeListNode(lNode).Id;
      SetCurentHouse(FCurentHouse);
    end;

    FDefalcareBuget := TfrmAlopDisponibil.Create(Self);
    FDefalcareBuget.tabLegal.TabVisible := False;
    FDefalcareBuget.MultipleSelection := True;
    GridRegistruDETALIIBuget.PopupControl := FDefalcareBuget;
    GridRegistruPROJ.Visible := False;
    GridRegistruTIP_CHELTVEN.Visible := False;
    GridRegistruORGANIGRAMA.Visible := False;
    GridRegistruRESURSA.Visible := False;

    StorageReadDxTree(GridRegistru);

  finally
  end;
end;

procedure TFrmRegistru.SoftDeleteCurrentRecord;
var
  aIDLista: Integer;
  SQLQuery: string;
begin
  if Not Assigned(GridRegistru.FocusedNode) then Exit;

  aIDLista := GridRegistru.FocusedNode.Values[GridRegistruCOD.Index];

  if aIDLista > 0 then
  begin

    SQLQuery := Format('UPDATE DBO.BREGISTRU SET STARE = 0 WHERE COD = %d', [aIDLista]);


    ShowMessage(SQLQuery);


    DBExecSQLFmt('%s', [SQLQuery]);


    RefreshDataSet;
  end;
end;












procedure TFrmRegistru.GridRegistruKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);

  function ClearOnDelete(aFieldName : String; aKey : Word) : Boolean;
  var OldEdit : Boolean;
  begin
    Result := False;
    if (aKey = VK_DELETE) and (GridRegistru.FocusedField = MemRegistru.FindField(aFieldName)) then begin
       OldEdit := MemRegistru.State in [dsEdit, dsInsert];
       if not OldEdit then MemRegistru.Edit;
       MemRegistru.FindField(aFieldName).Clear;
       if aFieldName = 'COD_FUNCTIONAL' then
         MemRegistru.FindField('ID_PROIECT').Clear;
       if aFieldName = 'ID_PROIECT' then
         MemRegistru.FindField('COD_FUNCTIONAL').Clear;
       if aFieldName = 'ID_TIPURI_CHELTVEN' then
         MemRegistru.FindField('COD_ECONOMIC').Clear;
       if aFieldName = 'COD_ECONOMIC' then
         MemRegistru.FindField('ID_TIPURI_CHELTVEN').Clear;
       if aFieldName = 'DETALII_BUGET' then begin
         MemRegistru.FindField('ID_PROIECT').Clear;
         MemRegistru.FindField('COD_FUNCTIONAL').Clear;
         MemRegistru.FindField('ID_TIPURI_CHELTVEN').Clear;
         MemRegistru.FindField('COD_ECONOMIC').Clear;
         MemRegistru.FindField('ID_OI_PROIECTE').Clear;
         MemRegistru.FindField('ID_OI_UNITATI').Clear;
         MemRegistru.FindField('ID_ANGAJAMENTE_DEFALCARE').Clear;
         MemRegistru.FindField('ID_ORDONANTARE_DEFALCARE').Clear;
       end;
       MemRegistru.Post;
       if OldEdit then MemRegistru.Edit;
       Result := True;
    end;
  end;

begin
  if (Key = VK_DOWN) and (GridRegistru.LastNode = GridRegistru.FocusedNode) then
    Cmd_AdaugaPlata.Execute;

  ClearOnDelete('CODGEST', Key);

  ClearOnDelete('CURS_SCHIMB', Key);

  if ClearOnDelete('CONT_CSP', Key) then
     if Assigned(GridRegistru.FocusedNode) then begin
        StructuraActualizare(TdxDBTreeListNode(GridRegistru.FocusedNode));
        PostMessage(Handle, WM_SET_STARE_SOLD, Integer(StrActualizare), Integer(True));
     end;

  ClearOnDelete('TIPDOC', Key);
  if ClearOnDelete('ID_PROIECT', Key) or ClearOnDelete('COD_FUNCTIONAL', Key) then
    if Assigned(GridRegistru.FocusedNode) then begin
        StructuraActualizare(TdxDBTreeListNode(GridRegistru.FocusedNode));
        PostMessage(Handle, WM_SET_STARE_SOLD, Integer(StrActualizare), Integer(True));
    end;

  ClearOnDelete('COD_ECONOMIC', Key);
  ClearOnDelete('ID_TIPURI_CHELTVEN', Key);
  ClearOnDelete('ID_ORGANIGRAMA', Key);
  ClearOnDelete('ID_RESURSA', Key);

  ClearOnDelete('DETALII_BUGET', Key);

end;

procedure TFrmRegistru.edDataDateChange(Sender: TObject);
begin
  if csDestroying in ComponentState then Exit;
  CheckForSave;
  if edData.Date < 0 then edData.Date := Date
  else begin
    FStartInterval := edData.Date;
    if chkEfectiv.Checked then FEndInterval := edData.Date
    else FEndInterval   := 0;
    if chkEfectiv.Checked then
      if edNrZile.IntValue < 0 then FEndInterval := FEndInterval - edNrZile.IntValue
      else if edNrZile.IntValue > 0 then FStartInterval := FStartInterval -  edNrZile.IntValue;
    {daca data este activata sau daca este un decont numai atunci deschidem dataset-urile}
     if (edData.Enabled) or (FIsAvans and (FCodDecont>0)) then begin
      RefreshDataSet;
    end;
    UpdateSoldCasa;
  end;
end;

procedure TFrmRegistru.GridRegistruCONT_CSPCloseUp(Sender: TObject;
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
          Field.Value := lNode.Id;
          Field.DataSet.Post;
          if lEditabil then Field.DataSet.Edit;
          Text := lNode.Id;
          Accept := False;
       end;
    end;
end;

procedure TFrmRegistru.GridRegistruCODGESTCloseUp(Sender: TObject;
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
          Field.Value := lNode.Id;
          Field.DataSet.Post;
          if lEditabil then Field.DataSet.Edit;
          Text := lNode.Id;
          Accept := False;
       end;
    end;
end;

procedure TFrmRegistru.InitCulegere;
var aDataSet :TDataSet;
begin
  aDataSet := DTRegistru.DataSet;

  { Validare conturi introduse }
  aDataSet.FieldByName('CONT_CSP').OnValidate := ValidareContContabil;

  { Validare Proiectul introduse  }
  aDataSet.FieldByName('ID_PROIECT').OnValidate := ValidareProj;

  { Validare CheltVen introduse  }
  aDataSet.FieldByName('ID_TIPURI_CHELTVEN').OnValidate := ValidareCheltVen;

  { Validare Cod Repartitor}
  aDataSet.FieldByName('CODGEST').OnValidate  := ValidareCodRep;

  {Validare Tip Document}
  aDataSet.FieldByName('TIPDOC').OnValidate  := ValidareTipDoc;

  {Validare Suma Introdusa}
  aDataSet.FieldByName('PLATI').OnValidate := ValidareSumaCopil;

  {Validare Suma Introdusa}
  aDataSet.FieldByName('INCASARI').OnValidate := ValidareSumaCopil;
end;

procedure TFrmRegistru.GridRegistruTIPDOCCloseUp(Sender: TObject;
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
          Field.AsString := lNode.Strings[frmcasaContainer.TreeTipDocTIP_DOC.Index];
          Field.DataSet.Post;
          if lEditabil then Field.DataSet.Edit;
          Text := lNode.Strings[frmcasaContainer.TreeTipDocTIP_DOC.Index];
          Accept := False;
       end;
    end;
end;

procedure TFrmRegistru.Cmd_AdaugaPlataExecute(Sender: TObject);
Var
  lNode: TdxDBTreeListNode;
  LineInfo : TLineNodeInfo;
begin
   if MemRegistru.State in [dsEdit, dsInsert] then MemRegistru.Post;
   if not Assigned(GridRegistru.FocusedNode) then lNode := nil
   else lNode := TdxDBTreeListNode(GridRegistru.FocusedNode);
   if (FIsAvans) and (FCodDecont < 0) then begin
     MessageBeep(MB_ICONEXCLAMATION);
     Exit;
   end;
   //AdaugaMemRegistru(lNode);
   GetLineInfo(lNode, LineInfo);
   {in caz ca nu avem inregistrari}
   if (lNode = nil) and (MemRegistru.RecordCount = 0) then begin
     LineInfo.ID := 'X';
     LineInfo.RealLineValue := -1;
   end;
   AdaugaMemRegistruNew(LineInfo);
   GridRegistru.FocusedColumn := 0;
end;

procedure TFrmRegistru.ValidareContContabil(Sender: TField);
begin
  if InternalAdd then Exit;
  if Sender.IsNull then Exit;
  if Sender.AsString = '%' then begin
    MemRegistru.FieldByName('ECL').AsInteger := 0;
  end
  else
     InternalValidateCont(Trim(Sender.AsString), frmCasaContainer.TreePlan);
end;

procedure TFrmRegistru.GridRegistruCustomDrawCell(Sender: TObject;
  ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  AColumn: TdxTreeListColumn; ASelected, AFocused, ANewItemRow: Boolean;
  var AText: String; var AColor: TColor; AFont: TFont;
  var AAlignment: TAlignment; var ADone: Boolean);


  procedure ChangeTransferCell(var myFont : TFont; var myColor:TColor);
  var  I : Integer;
  begin
    I := GetAsInteger(ANode, GridRegistruTRANSFER.Index);
    if I > 0 then begin
      myColor := TransferDiplayTable[I].Color;
      SetConstantToFont(myFont, TransferDiplayTable[I].FontInfo);
    end;
  end;


begin
  {daca este copil}
  if ANode.Strings[GridRegistruID_PARINTE.Index] > '' then begin
    {daca mai are parinte}
    if Assigned(ANode.Parent) then begin
       AColor :=  cl_SecondLevelColor;
       SetConstantToFont(AFont, ft_SecondLevelColor);
    end
    {daca parintele e sters}
    else begin
       AColor :=  cl_DeletedSecondLevelColor;
       SetConstantToFont(AFont, ft_DeletedSecondLevelColor);
    end;
  end
  else begin
     AColor := cl_FirstLevelColor;
     SetConstantToFont(AFont, ft_FirstLevelColor);
  end;

  if ValueHasValue(CodCurent) then
    if ValueSameValue(CodCurent, ANode.Values[GridRegistruID_PARINTE.Index]) then begin
      AColor := cl_ChildColor;
      SetConstantToFont(AFont, ft_ChildColor);
    end
    else if ValueSameValue(TdxDBTreeListNode(ANode).Id, CodCurent) then begin
      AColor := cl_ParentColor;
      SetConstantToFont(AFont, ft_ParentColor);
    end;

   if (AColumn= GridRegistruPLATI) or (AColumn= GridRegistruINCASARI) or (AColumn= GridRegistruSOLD) then
    if ValueSafeToCurrency(ANode.Values[AColumn.Index]) = 0 then begin
       if (AColumn= GridRegistruSOLD) then AText := '0'
       else AText := '';
       end
    else if ValueHasValue(ANode.Values[AColumn.Index]) then
      AText := FormatFloat(CurrFormat, ValueSafeToCurrency(ANode.Values[AColumn.Index]));

  if ValueSafeToInt(ANode.Values[GridRegistruVALIDATA.Index]) in [1, 3, 4] then begin
    AColor := cl_Validare;
    SetConstantToFont(AFont, ft_Validare);
  end;

  if AColumn = GridRegistruDATA then
    if ValueSameValue(DataEmitere, ANode.Values[GridRegistruDATA.Index]) then begin
      AColor := cl_DataColor;
      SetConstantToFont(AFont, ft_DataColor);
    end;

  if AFocused and (AColumn.Index = GridRegistru.FocusedAbsoluteIndex) then begin
     AColor := cl_FocusedColor;
     SetConstantToFont(AFont, ft_FocusedColor);
  end;

  if AColumn.Index = GridRegistruECL.Index then
     ChangeTransferCell(AFont, AColor);

  if FAfisTransferEdit <> ModAfisTranfer then
    AfisTransferEdit := ModAfisTranfer;
end;

procedure TFrmRegistru.GridRegistruChangeNode(Sender: TObject; OldNode,
  Node: TdxTreeListNode);
Var lParent : String;
begin
  if (Node = nil) or IsInAdd or IsInLoad then Exit;
  DataEmitere := Node.Values[GridRegistruDATA.Index];

  if Trim(Node.Strings[GridRegistruID_PARINTE.Index])= '' then
    lParent := '-1'
  else
    lParent := Node.Strings[GridRegistruID_PARINTE.Index];

  if (lParent = '-1') then CodCurent := TdxDBTreeListNode(Node).Id
  else CodCurent := Node.Values[GridRegistruID_PARINTE.Index];

  if IsRightEnable then begin
    Cmd_AdaugaPlata.Enabled := ((lParent <> '-1') and (GetAsInteger(Node, GridRegistruECL.Index)=0)) or (td_Casier in FHouseRigths);
    Cmd_AdaugaPozitieNoua.Enabled := (td_Casier in FHouseRigths);
    Cmd_AdaugaDefalcare.Enabled   := ((lParent <> '-1') and (GetAsInteger(Node, GridRegistruECL.Index)=0)) or (td_Casier in FHouseRigths);
    Cmd_EchilibrarePlata.Enabled := ((lParent <> '-1') and (GetAsInteger(Node, GridRegistruECL.Index)=0)) or (td_Casier in FHouseRigths);
    Cmd_DeletePlata.Enabled := ((lParent <> '-1')) or (td_Casier in FHouseRigths);
  end;

  GridRegistru.Invalidate;

  if Trim(Node.Strings[GridRegistruPOZ.Index]) = '' then CurentPoz := 0
  else CurentPoz := Node.Values[GridRegistruPOZ.Index];
  DisableEditor(Node);
end;

procedure TFrmRegistru.GridRegistruChangeColumn(Sender: TObject;
  Node: TdxTreeListNode; Column: Integer);
var
  lParent : String;
begin
  DisableEditor(Node);

  if Trim(Node.Strings[GridRegistruID_PARINTE.Index])='' then
    lParent := '-1'
  else
    lParent := Node.Strings[GridRegistruID_PARINTE.Index];

  if (lParent = '-1') then CodCurent := TdxDBTreeListNode(Node).Id
  else CodCurent := Node.Values[GridRegistruID_PARINTE.Index];

  if IsRightEnable then begin
    Cmd_AdaugaPlata.Enabled := ((lParent <> '-1') and (GetAsInteger(Node, GridRegistruECL.Index)=0)) or (td_Casier in FHouseRigths);
    Cmd_AdaugaPozitieNoua.Enabled := (td_Casier in FHouseRigths);
    Cmd_AdaugaDefalcare.Enabled   := ((lParent <> '-1') and (GetAsInteger(Node, GridRegistruECL.Index)=0)) or (td_Casier in FHouseRigths);
    Cmd_EchilibrarePlata.Enabled := ((lParent <> '-1') and (GetAsInteger(Node, GridRegistruECL.Index)=0)) or (td_Casier in FHouseRigths);
    Cmd_DeletePlata.Enabled := ((lParent <> '-1')) or (td_Casier in FHouseRigths);
  end;

  if not(IsOnQuestion) then begin
    if GridRegistru.Columns[TdxDBTreeList(Sender).GetFocusedAbsoluteIndex(Column)] is TdxDBTreeListPopupColumn then
       PostMessage(Handle, WM_DROPDOWN_IMAGECOLUMN, 0, 3);
    if GridRegistru.Columns[TdxDBTreeList(Sender).GetFocusedAbsoluteIndex(Column)] is TdxDBTreeListImageColumn then
       PostMessage(Handle, WM_DROPDOWN_IMAGECOLUMN, 0, 0);
    if GridRegistru.Columns[TdxDBTreeList(Sender).GetFocusedAbsoluteIndex(Column)] is TdxDBTreeListMRUColumn then
       PostMessage(Handle, WM_DROPDOWN_IMAGECOLUMN, 1, 0);
  end;
end;

procedure TFrmRegistru.InternalValidateCont(Val: String;
  Tree: TdxDbTreeList; const lTag : Integer = -1);
var MustDrop: Boolean;
    SelNode : TdxDBTreeListNode;
    lSearchC : TdxDBTreeListColumn;
begin
  lSearchC := FindColumnByTag(Tree, lTag);
  Tree.EndSearch;
  { Se valideaza contul de buget introdus }
  MustDrop := (Val = '') or (Val = '?');
  { Incercam sa gasim nodul posibil }
  SelNode := nil;
  if not MustDrop then begin
     while (not Assigned(SelNode)) and (Length(Val) > 0) do begin
       if lSearchC <> nil then
         TCrackATSDBTreeList(Tree).FindNodeByText(lSearchC.Index, Val, sdNone, TdxTreeListNode(SelNode))
       else
         SelNode := Tree.FindNodeByKeyValue(Val);
       if not Assigned(SelNode) then begin MustDrop := True; Delete(Val, Length(Val), 1); end;
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
     if Assigned(SelNode) then SelNode.Focused := True;
  end;

  if (MustDrop) and (Assigned(GridRegistru.InplaceEditor)) and not IsInLoad then begin
      if SelNode <> nil then begin SelNode.MakeVisible;
        if SelNode.HasChildren then SelNode.Expanded := True; end
      else Tree.StartSearch(-1, Val);
      SendMessage(GridRegistru.InplaceEditor.Handle, CM_DROPDOWNPOPUPFORM, 0, 0);
      Abort;
  end;
end;

procedure TFrmRegistru.ValidareCodRep(Sender: TField);
begin
  if not Sender.IsNull then
     if IsNumeric(Trim(Sender.AsString)) then InternalValidateCont(Trim(Sender.AsString), frmCasaContainer.TreeRepartitori, -999)
end;

procedure TFrmRegistru.chkExpandClick(Sender: TObject);
begin
  if chkExpand.Checked then
     GridRegistru.FullExpand
  else GridRegistru.FullCollapse;
end;

procedure TFrmRegistru.ValidareTipDoc(Sender: TField);
begin
  if not Sender.IsNull then InternalValidateCont(Trim(Sender.AsString), frmCasaContainer.TreeTipDoc);
end;

procedure TFrmRegistru.GridRegistruGetLevelColor(Sender: TObject;
  ALevel: Integer; var AColor: TColor);
begin
  AColor := cl_FirstLevelColor;
end;


procedure TFrmRegistru.RefreshDataSet;
var OldModified : Boolean;
begin

  if not (BusealaStearsa) then begin
     BusealaStearsa := True;
  end;
  if IsRightEnable then
    if FHouseRigths = [] then Exit;

  SetMainFilter('', False);

  FSignLogin :=  CommonDBVar.IdLogin;
  GridRegistru.DataSource := nil;
  GridRegistru.OnDeletion := nil;
  GridRegistru.OnChangeNode := nil;
  GridRegistru.BeginUpdate;

  LocalId := 2;

  {sincronizam ecranul cu inregistrariile curente}
  AssignOnChange(False);
  IsSyncro := True;

  {incarcam seturile de date}
  ProgressCaption := StrLoadRegistru;
  LoadRecordSet(MemReg, tl_fara);
  ProgressCaption := StrLoadCont;
  LoadRecordSet(MemCont, tl_cont);
  ProgressCaption := StrLoadProj;
  LoadRecordSet(MemProj, tl_proiect);

  GetDeconturiList(DeconturiList);
  ProgressCaption := StrLoadShare;
  QryShare_Point.Active := False;
  QryShare_Point.Params.ParamByName('ID_UTILIZATOR').Value := IdLogin;
  QryShare_Point.Open;

  { Aplicăm filtrul pentru a afișa doar înregistrările salvate (stare = 1) }
  if MemRegistru.Active then begin
    MemRegistru.Filter := 'STARE = 1';
    MemRegistru.Filtered := True;
  end;

  {deschidem datasetul cu datele curente}
  Cmd_SchimbaDefalcareExecute(nil);
  CurentPoz := 1;

  GridRegistru.EndUpdate;
  GridRegistru.DataSource := DTRegistru;
  GridRegistru.OnChangeNode := GridRegistruChangeNode;
  GridRegistru.OnDeletion := GridRegistruDeletion;

  if not MemRegistru.Active then MemRegistru.Active := True;
  MemRegistru.First;

  GridRegistru.Invalidate;

  FSoldInitial := GetSoldInitial;
  CalculateSold(TdxDBTreeListNode(GridRegistru.Items[0]));

  AssignOnChange(True);

  OldModified := FIsModified;
  MemRegistruINCASARIChange(nil);
  FIsModified := OldModified;

  CalculateTotalFix(MemRegistru, TotalIncasari, TotalPlati);

  TotalCount := GridRegistru.Count;
  SummStatus.Panels[0].Text := Format('%s/%s', [IntToStr(TotalCount), '0']);
  SummStatus.Panels[2].Text := FormatFloat(CurrFormat, TotalIncasari);
  SummStatus.Panels[4].Text := FormatFloat(CurrFormat, TotalPlati);
  SummStatus.Panels[7].Text := '0.00';
  SummStatus.Panels[9].Text := '0.00';

  SetMainFilter(FFilter, chkFilter.Checked);
end;


procedure TFrmRegistru.CalculateSold(aNode: TdxTreeListNode);
var
  lSold, lPlati, lIncasari : Currency;
  lEditabil : Boolean;
    I : Integer;
  lBookMark : TBookmark;
  lNode     : TdxTreeListNode;
begin
  {rutina calculeaza si actualizeaza soldul pe o linie care nu are parinte}
  if Assigned(ANode) and not Assigned(ANode.Parent) then begin
    GridRegistru.BeginUpdate;
  try
      if ANode.Index - 1 >= 0 then
        lSold := ValueSafeToCurrency(GridRegistru.Items[ANode.Index-1].Values[GridRegistruSOLD.Index])
    else
        lSold := FSoldInitial;
      lEditabil := MemRegistru.State in dsEditModes;
      lBookMark := MemRegistru.GetBookmark;
    MemRegistru.DisableControls;
      try
        for I:= aNode.Index to GridRegistru.Count -1 do begin
          lNode     := GridRegistru.Items[I];
          lPlati    := ValueSafeToCurrency(lNode.Values[GridRegistruPLATI.Index]);
          lIncasari := ValueSafeToCurrency(lNode.Values[GridRegistruINCASARI.Index]);
          if not (ValueSafeToInt(lNode.Values[GridRegistruTRANSFER.Index]) in [2, 6, 9, 10]) then
            if FIsCreditor then
              lSold := lSold + ( 1) * lPlati + (-1) * lIncasari
            else
              lSold := lSold + (-1) * lPlati + ( 1) * lIncasari;
          if MemRegistru.Locate('ID_LISTA', lNode.Values[GridRegistruID_LISTA.Index], []) then begin
            if MemRegistru.FieldByName('SOLD').AsCurrency <> lSold then begin
              DBGoEdit(MemRegistru);
              MemRegistru.FieldByName('SOLD').AsCurrency := lSold;
              DBPost(MemRegistru);
               end;
             end;
           end;
      finally
        MemRegistru.GotoBookmark(lBookMark);
        MemRegistru.FreeBookmark(lBookMark);
        MemRegistru.EnableControls;
        if lEditabil then DBGoEdit(MemRegistru);
    end;
  finally
      GridRegistru.EndUpdate;
    GridRegistru.Invalidate;
    end;
  end;
end;

procedure TFrmRegistru.MemRegistruCalcFields(DataSet: TDataSet);
var OldInLoad : Boolean;
begin
   if IsInLoad then Exit;
   OldInLoad := IsInLoad;
   with DataSet do begin
     IsInLoad := True;
     FieldByName('SORTFIELD').AsString := Format('%8.0f',[(FieldByName('DATA').AsDateTime)]) + '|'+ Format('%.8x', [FieldByName('POZ').AsInteger])+'|'+Format('%.8x', [FieldByName('RecId').AsInteger]);
     IsInLoad := OldInLoad;
   end;
end;

procedure TFrmRegistru.btnSwitchFiltersUpClick(Sender: TObject);
var aNode : TdxTreeListNode;
begin
  if not Assigned(GridRegistru.FocusedNode) then Exit;
  aNode := GridRegistru.FocusedNode;
  if not(MoveBy(TdxDBTreeListNode(aNode), -1)) then ShowMessage(StrMoveNotDone)
  else
    CalculateSold(GridRegistru.FocusedNode);
end;

procedure TFrmRegistru.btnSwitchFiltersDownClick(Sender: TObject);
var aNode : TdxTreeListNode;
begin
  if not Assigned(GridRegistru.FocusedNode) then Exit;
  aNode := GridRegistru.FocusedNode;
  if not(MoveBy(TdxDBTreeListNode(aNode), 1)) then ShowMessage(StrMoveNotDone)
  else
    CalculateSold(aNode);
end;

function TFrmRegistru.MoveBy(aNode: TdxTreeListNode;
  Direction: Integer): Boolean;
var
  aExNode : TdxTreeListNode;
    exPoz, Poz : Integer;
    exId, Id  : String;
begin
  Result := False;
  aExNode := nil;
  { Mutarea o face dupa parinti intotdeauna }
  if not Assigned(aNode) then
    Exit;
  if Assigned(aNode.Parent) then aNode := aNode.Parent;
  if (aNode.Index + Direction < GridRegistru.Count) and (aNode.Index + Direction > -1) then
    aExNode := GridRegistru.Items[aNode.Index + Direction];
  //comparam dupa data si parenid
  if Assigned(aExNode) then begin
      if (aExNode.Strings[GridRegistruDATA.Index] = aNode.Strings[GridRegistruDATA.Index]) then begin
        exPoz := GetAsInteger(aExNode, GridRegistruPOZ.Index);
        Poz   := GetAsInteger(aNode, GridRegistruPOZ.Index);

        if (exPoz = Poz) then ShowMessage('Trebuie facuta o renumerotare ! ');

        GridRegistru.BeginUpdate;

        Id   := aNode.Strings[GridRegistruID_LISTA.Index];
        exId := aExNode.Strings[GridRegistruID_LISTA.Index];
        {facem update in data set dupa cele doua id-uri}
        if MemRegistru.Locate('ID_LISTA', exId, []) then begin
           if not(MemRegistru.State in [dsEdit, dsInsert]) then MemRegistru.Edit;
           MemRegistru.FieldByName('POZ').AsInteger := Poz;
           MemRegistru.Post;
        end;
        if MemRegistru.Locate('ID_LISTA', Id, []) then begin
           if not(MemRegistru.State in [dsEdit, dsInsert]) then MemRegistru.Edit;
           MemRegistru.FieldByName('POZ').AsInteger := exPoz;
           MemRegistru.Post;
        end;
        //MemRegistru.EnableControls;
        GridRegistru.EndUpdate;
        Result := True;
      end;
  end;
end;


procedure TFrmRegistru.Cmd_SchimbaDefalcareExecute(Sender: TObject);
var
    OldModified : Boolean;
begin
  GridRegistru.DataSource := nil;
  GridRegistru.OnDeletion := nil;
  GridRegistru.OnChangeNode := nil;
  GridRegistru.BeginUpdate;
  IsInLoad := True;

  {sincronizam datasetul local}
  if not(IsSyncro) then SyncronizeDataSets(False, CurentrbTag);
  IsSyncro := False;
  CurentrbTag := GetDefalcareType;

  {incarcam noile date}
  case CurentrbTag of
    0 : {fara defalcare}
        begin
           GridRegistru.Bands[2].Visible :=False;
           GridRegistru.Bands[3].Visible :=False;
           GridRegistru.Bands[5].Visible :=False;
           ProgressCaption := StrLoadRegistru;
           SyncDataSet(MemReg, MemRegistru, True);
           //IsSyncro := True;
        end;
    1 : {defalcare cont}
        begin
          ProgressCaption := StrLoadCont;
          DoSetDataSet(tl_cont);
        end;
    2 : {defalcare proiecte}
        begin
          ProgressCaption := StrLoadProj;
          DoSetDataSet(tl_proiect);
        end;
    4 : {defalcare facturi}
        begin
          ProgressCaption := StrLoadFact;
          DoSetDataSet(tl_proiect);
          DoSetDataSet(tl_facturi);
        end;
  end;
  InitCulegere;
  IsInLoad := False;
  DBExplicProj.Enabled := (CurentrbTag = rbProiect.Tag);
  GridRegistru.DataSource := DTRegistru;
  GridRegistru.OnChangeNode := GridRegistruChangeNode;
  GridRegistru.OnDeletion := GridRegistruDeletion;
  GridRegistru.EndUpdate;
  GridRegistru.Invalidate;

  chkExpandClick(chkExpand);

  OldModified := FIsModified;
  MemRegistruINCASARIChange(nil);
  FIsModified := OldModified;

  if Self.Visible then GridRegistru.SetFocus;
end;

procedure TFrmRegistru.DoSetDataSet(TipLista : TTipLista);
//var Band : TdxTreeListBand;
begin
  GridRegistru.Bands[2].Visible := (TipLista = tl_cont);
  GridRegistru.Bands[3].Visible := (TipLista = tl_proiect);
  GridRegistru.Bands[5].Visible := (TipLista = tl_facturi);
  {if Cont then
    Band := GridRegistru.Bands[2]
  else
    Band := GridRegistru.Bands[3];}
  {caracteristici banda}
//  Band.Width := 600;
  //Band.Sizing := False;
  case TipLista of
    tl_cont    : UniteDataSets(MemRegistru, MemReg, MemCont);
    tl_proiect : UniteDataSets(MemRegistru, MemReg, MemProj);
(*CHRIS SALVARE FACTURI
    tl_facturi : UniteDataSets(MemRegistru, MemReg, MemFact);
*)
  end;
end;

procedure TFrmRegistru.LoadRecordSet(aMemDataset: TdxMemData; TipLista: TTipLista);
var
  lDataSet  : TDataSet;
  lProcName, SQLQuery, CodDecontStr, DataStartStr, DataEndStr : String;
begin
  aMemDataset.DisableControls;
  if aMemDataset.Active then aMemDataset.Active := False;
  if not aMemDataset.Active then aMemDataset.Active := True;

  lProcName := GetExec(TipLista);

  // 🔹 Convertim datele corect în formatul acceptat de SQL Server
  DataStartStr := FormatDateTime('YYYY-MM-DD', FStartInterval);
  DataEndStr := FormatDateTime('YYYY-MM-DD', FEndInterval);

  // 🔹 Convertim ValCodDecont corect (NULL dacă e gol)
  if Trim(ValueToStr(ValCodDecont)) = '' then
    CodDecontStr := 'NULL'
  else
    CodDecontStr := ValueToStr(ValCodDecont); // Nu folosim `QuotedStr` pe numere

  // 🔹 Construim query-ul SQL corect
  SQLQuery := Format('exec %s %d, ''%s'', ''%s'', %d, %d, %s',
                     [
                       lProcName,
                       FCurentHouse,
                       DataStartStr,  // Formatarea corectă pentru SQL
                       DataEndStr,    // Formatarea corectă pentru SQL
                       FSignLogin,
                       0,
                       CodDecontStr   // Nu folosim ghilimele pe NULL sau INT
                     ]);

  // 🔹 Afișăm query-ul pentru debugging
//  ShowMessage(SQLQuery);

  // 🔹 Creăm dataset-ul corect
  lDataSet := DBNewQuery(SQLQuery);

  try
    lDataSet.Open;

    // 🔹 Aplicăm filtrul doar dacă există coloana "STARE"
    if lDataSet.FindField('STARE') <> nil then
    begin
      lDataSet.Filter := 'STARE = 1';
      lDataSet.Filtered := True;
    end;

    // 🔹 Copierea datelor în memorie
    DBCopyFromDataSet(aMemDataset, lDataSet, False);
  finally
    lDataSet.Free;
  end;

  aMemDataset.EnableControls;
end;




procedure TFrmRegistru.UniteDataSets(var ResultData: TdxMemData;
  FirstData, SecondData: TdxMemData);
begin
  {UNESTE DOUA DATA SETURI IN MEMREGISTRU}
  ResultData.DisableControls;
  FirstData.DisableControls;
  IsInLoad := True;
  if ResultData.Active then ResultData.Active := False;
  ResultData.Active := True;

  {INCARCAM PRIMUL DATASET}
  if not(FirstData.Active) then FirstData.Active := True;
  DBLoadFromDataSet(ResultData, FirstData, False);

  {INCARCAM SI AL DOILE DATASET}
  if not(SecondData.Active) then SecondData.Active := True;
  DBLoadFromDataSet(ResultData, SecondData, False);

  {SETUL VA FI ORDONAT DUPA SORTFIELD CARE ESTE UN CAMP CALCULAT}
  if not(ResultData.Active) then ResultData.Active := True;

  {Aplica filtrul curent}
  //ApplyFilters();

  IsInLoad := False;
  ResultData.EnableControls;
  FirstData.EnableControls;
end;


procedure TFrmRegistru.ValidareSumaCopil(Sender: TField);
const Lock: Boolean = False;
begin
  if Lock then Exit;
  Lock := True;
  try
    if (MemRegistru.FieldByName('ID_PARINTE').AsString = '') then
        if Assigned(Sender.DataSet) then
          MemRegistru.FieldByName('ECL').AsInteger := Integer(Boolean((Sender.DataSet.FieldByName('PLATI').AsCurrency + Sender.DataSet.FieldByName('INCASARI').AsCurrency) <> 0))
        else
          MemRegistru.FieldByName('ECL').AsInteger := Integer(Boolean(Sender.AsCurrency <> 0))
    else ValidateSumeEcl(GridRegistru.FocusedNode, Sender.AsCurrency);
  finally
     Lock := False;
  end;
end;

procedure TFrmRegistru.ValidateSumeEcl(Node: TdxTreeListNode;
  NewValue: Currency);
var lTotalCod,
    lTotalDef: Currency;
    I : Integer;
begin
  if (not Assigned(Node))
    or (Node.Strings[GridRegistruID_PARINTE.Index] = '')
    {or (Node.Strings[GridRegistruCONT_CSP.Index] <> '%') }
  then Exit;
  { Daca nu este echilibrata incercam sa vedem daca s-a echilibrat intre timp }
  lTotalDef := 0;
  if Node.HasChildren then begin
     { Trebuie sa vedem daca suma de pe nodul curent este egala cu suma de pe toti copii xcare nu mai au copii }
     lTotalCod := NewValue;
     for I := 0 to Node.Count-1 do
       if not Node.Items[I].HasCHildren then
         lTotalDef := lTotalDef + GetUnitedValue(Node.Items[I]);
     StructuraActualizare(TdxDBTreeListNode(Node));
     PostMessage(Handle, WM_SET_STARE_SOLD, Integer(StrActualizare), Integer(lTotalDef = lTotalCod));
  end
  else
    if Assigned(Node.Parent) then
    begin
       lTotalCod := GetUnitedValue(Node.Parent);
       for I := 0 to Node.Parent.Count-1 do
       if not Node.Parent.Items[i].HasChildren then
       begin
         if Node.Parent.Items[I] = Node then lTotalDef := lTotalDef + NewValue
         else lTotalDef := lTotalDef +GetUnitedValue(Node.Parent.Items[I]);
       end;
       StructuraActualizare(TdxDBTreeListNode(Node.Parent));
       PostMessage(Handle, WM_SET_STARE_SOLD, Integer(StrActualizare), Integer(lTotalDef = lTotalCod));
    end;
end;

procedure TFrmRegistru.SetStareCurentaNota(var Message: TMessage);
var
    Ecl : Boolean;
    lStrActulizare : PStrActualizare;

    procedure SetEchilibrat(lIdItems: String; lCamp : String = 'ID_LISTA');
    var OldEdit: Boolean;
        aBook : TBookMark;
        //folosite pentru iesirea din locate loop
        lOldPos : String;
     begin
        with MemRegistru do
          try
            aBook := MemRegistru.GetBookmark;
            lOldPos := '';
            First;
            while Locate(lCamp + ';ECL' , VarArrayOf ([lIdItems, Integer(not(ECL))]), []) do begin
               //facem compararea pe cam de ID
               if lOldPos = FieldByName('ID_LISTA').AsString then Break;
               lOldPos := FieldByName('ID_LISTA').AsString;
               if FieldByName('ECL').AsInteger <> Word(Ecl) then begin
  FieldByName('ECL').OnChange := nil;
  OldEdit := State in [dsEdit, dsInsert];
  if not OldEdit then Edit;
  FieldByName('ECL').AsInteger := Word(Ecl);
  // Actualizează și câmpul "stare"
  FieldByName('stare').AsInteger := Word(Ecl);
  Post;
  if OldEdit then Edit;
  FieldByName('ECL').OnChange := GlobalChange;
end;

            end;
          finally
            MemRegistru.GotoBookmark(aBook);
            MemRegistru.FreeBookMark(aBook);
          end;
     end;

begin
  { Setam stare curenta pentru nota compusa }
  { In WParam avem informatiile despre Nodul Parinte al Platii sau incasarii }
  { In LParam avem 0 - pentru neechilibrata, 1 - pentru Echilibrata }

  if MemRegistru.RecordCount = 0 then Exit;
  lStrActulizare := PStrActualizare(Message.WParam);
  if Assigned(lStrActulizare) then
     try
       GridRegistru.BeginUpdate;
       if lStrActulizare^.Id <> '' then begin
          Ecl := Boolean(Message.LParam);
          SetEchilibrat(lStrActulizare^.Id, 'ID_LISTA');
          if lStrActulizare^.Defalcat then SetEchilibrat(lStrActulizare^.ParentId, 'ID_PARINTE');
       end;
    finally
       GridRegistru.EndUpdate;
    end;
end;


procedure TFrmRegistru.GridRegistruDeletion(Sender: TObject;
  Node: TdxTreeListNode);
var lNode: TdxDBTreeListNode;
    I : Integer;
    lTotal, lPartial: Currency;
begin
  if (Node <> nil) and (Node.Parent <> nil) then begin
     lNode := TdxDBTreeListNode(Node.Parent);
     if Node.Deleting and lNode.Deleting then Exit;
     lTotal := GetUnitedValue(lNode);
     lPartial := 0;
     for I := 0 to lNode.Count-1 do
       if lNode.Items[I] <> Node then lPartial := lPartial + GetUnitedValue(lNode.Items[I]);
     if not lNode.HasChildren then lPartial := lTotal;
     StructuraActualizare(TdxDBTreeListNode(lNode));
     PostMessage(Handle, WM_SET_STARE_SOLD, Integer(StrActualizare), Integer(lTotal = lPartial));
  end;
end;

procedure TFrmRegistru.GridRegistruDETALIIBugetCloseUp(Sender: TObject;
  var Text: string; var Accept: Boolean);
var
  lNode   : TdxDBTreeListNode;
  lContCsp: String;
begin
  if Accept then begin
    lNode := TdxDBTreeListNode(GridRegistru.FocusedNode);
    if Assigned(lNode) then begin
      AdaugaCopiiBugetCamp(FDefalcareBuget.InfoList, '', lNode);
      lContCsp := ValueSafeToStr(lNode.Values[GridRegistruCONT_CSP.Index]);
      GridRegistruCONT_CSP.ReadOnly := (lContCsp > '') and (lContCsp <> '%');
      VerificaPozitieEchilibrata(lNode);
    end
  end;
  FDefalcareBuget.ClearAllList;
end;

procedure TFrmRegistru.GridRegistruDETALIIBugetInitPopup(Sender: TObject);
begin
  FDefalcareBuget.PrepareCulegere(MemRegistru['CODGEST'],
                                   MemRegistru['COD_FUNCTIONAL'],
                                   MemRegistru['COD_ECONOMIC'],
                                   MemRegistru['ID_ANGAJAMENTE_DEFALCARE'],
                                   MemRegistru['ID_ORDONANTARE_DEFALCARE'],
                                   MemRegistru['ID_OI_UNITATI'],
                                   MemRegistru['ID_OI_PROIECTE'],
                                   MemRegistru['Data']);
end;

function TFrmRegistru.GetUnitedValue(aNode: TdxTreeListNode): Currency;
begin
  Result := 0;

  if aNode.Strings[GridRegistruPLATI.Index] > '' then
     Result := Result + aNode.Values[GridRegistruPLATI.Index];

  if aNode.Strings[GridRegistruINCASARI.Index] > '' then
     Result := Result + aNode.Values[GridRegistruINCASARI.Index];
end;

function TFrmRegistru.GetRealLineValue(aNode: TdxTreeListNode): Currency;
begin
  Result := 0;

  if aNode.Strings[GridRegistruPLATI.Index] > '' then
    Result := Result - aNode.Values[GridRegistruPLATI.Index];

  if aNode.Strings[GridRegistruINCASARI.Index]> '' then
    Result := Result + aNode.Values[GridRegistruINCASARI.Index];
end;

function TFrmRegistru.GetRealAbsoluteLineValue(aNode: TdxTreeListNode): Currency;
begin
  Result := 0;

  if aNode.Strings[GridRegistruPLATI.Index] > '' then
    Result := Result - Abs(aNode.Values[GridRegistruPLATI.Index]);

  if aNode.Strings[GridRegistruINCASARI.Index]> '' then
    Result := Result + Abs(aNode.Values[GridRegistruINCASARI.Index]);
end;


procedure TFrmRegistru.DisableEditor(aNode: TdxTreeListNode);
var I : Integer;
    aParent : TdxTreeListNode;
    aCol : TdxDBTreeListColumn;

  procedure CheckAndDisable(aColumn : TdxDBTreeListColumn; aState : Boolean);
  begin
    if (aColumn.Tag <> 1) then aColumn.DisableEditor := aState;
  end;

  procedure DisableInRight(aColumn : TdxDBTreeListColumn);
  var J : Integer;
  begin
    if not IsRightEnable then Exit;
    if td_Casier in FHouseRigths then begin
      //daca este casier atunci are drept sa umble la coloanele de sold
      for J := Low(CasierViewRight) to High(CasierViewRight) do
        if aColumn.FieldName = CasierViewRight[J] then begin
           CheckAndDisable(aColumn, False);
           Break;
        end;
    end
    else if (td_Validator in FHouseRigths)  then begin
        {contabilitate}
        for J := Low(ContabilViewRight) to High(ContabilViewRight) do
          if aColumn.FieldName = ContabilViewRight[J] then begin
             CheckAndDisable(aColumn, False);
             Break;
          end;
    end
    else if not (td_Administrator in FHouseRigths) then
       CheckAndDisable(aColumn, True);
  end;


begin
  if not Assigned(aNode) then Exit;

  if Assigned(GridRegistru.InplaceEditor) and (GridRegistru.InplaceEditor.IsVisible) then
    try
      GridRegistru.CancelEditor;
    except
     on E:Exception do begin
       EContaHandledError.Create('A intervenit o eroare la validare !'#13#10 + E.Message);
     end
    end;

  DBExplicProj.Enabled := False;
  for I:= 0 to GridRegistru.VisibleColumnCount-1 do CheckAndDisable(GridRegistru.VisibleColumns[I], False);

  {daca este o inregistrare validata atunci nu se mai poate modifica}
  if GetAsInteger(aNode, GridRegistruValidata.Index) in [1, 3, 4] then begin
     for I:= 0 to GridRegistru.VisibleColumnCount-1 do CheckAndDisable(GridRegistru.VisibleColumns[I], True);
     for I := Low(ValidationBlock) to High(ValidationBlock) do begin
        aCol :=  GridRegistru.FindColumnByFieldName(ValidationBlock[I]);
        if aCol <> nil then
          CheckAndDisable(aCol, False);
     end;
  end
  else begin
  {facem disable la plati sau incasari in functie de valorarea completata}
  {trebuie luat in calcul si valoarea parintelui daca parintele are incasare vom completa doar incasare}
  if Assigned(aNode.Parent) then aParent := aNode.Parent
                            else aParent := aNode;
  if GridRegistru.FocusedAbsoluteIndex in [GridRegistruINCASARI.Index, GridRegistruPLATI.Index] then
    if (GetRealAbsoluteLineValue(aParent)> 0) then CheckAndDisable(GridRegistruPLATI, True)
      else
   if (GetRealAbsoluteLineValue(aParent)< 0)  or (FIsAvans) then CheckAndDisable(GridRegistruINCASARI, True);

    {daca este o inregistrare trasferata atunci ea se poate defalca numai daca este }
    if not Assigned(aNode.Parent) then begin
      if ((aNode.Strings[GridRegistruTRANSFER.Index] <> '') and (aNode.Strings[GridRegistruTRANSFER.Index] <> '0')) then begin
        for I:= 0 to GridRegistru.VisibleColumnCount-1 do CheckAndDisable(GridRegistru.VisibleColumns[I], True);

       case GetAsInteger(aNode, GridRegistruTRANSFER.Index) of
          {pe aici nu trebuie sa intre}
          0  :;
          {fara modificare}
          2, 3, 4, 6, 7, 8, 9, 10 :begin
            for I := Low(TransferBlock) to High(TransferBlock) do begin
              aCol :=  GridRegistru.FindColumnByFieldName(TransferBlock[I]);
              if aCol <> nil then
                CheckAndDisable(aCol, False);
            end;
            //CheckAndDisable(GridRegistruCONT_CSP, False);
            //CheckAndDisable(GridRegistruCODGEST, False);
            //CheckAndDisable(GridRegistruPROJ, False);
         end;
          {permite numai defalcarea defalcare}
          1, 5, 11, 12, 13 : begin
            for I := Low(TransferLocalBlock) to High(TransferLocalBlock) do begin
              aCol :=  GridRegistru.FindColumnByFieldName(TransferLocalBlock[I]);
              if aCol <> nil then
                CheckAndDisable(aCol, False);
            end;

          end;
       end;
    end;
    end;
  end;

  {pe partea de proiecte daca nu s-a completat }
  if rbProiect.Checked then begin
     CheckAndDisable(GridRegistruTIP_CHELTVEN, True);
     CheckAndDisable(GridRegistruORGANIGRAMA, True);
     CheckAndDisable(GridRegistruRESURSA, True);

     if (Trim(aNode.Strings[GridRegistruPROJ.Index]) <> '') and (Trim(aNode.Strings[GridRegistruPROJ.Index]) <> '%') then
        CheckAndDisable(GridRegistruTIP_CHELTVEN, False);
     if Trim(aNode.Strings[GridRegistruTIP_CHELTVEN.Index]) <> '' then
        CheckAndDisable(GridRegistruORGANIGRAMA, False);
     if Trim(aNode.Strings[GridRegistruORGANIGRAMA.Index]) <> '' then
        CheckAndDisable(GridRegistruRESURSA, False);
  end;


  {capul de pozitie trebuie sa fie editabil}
  GridRegistru.ColumnByFieldName('POZ').DisableEditor := False;

  {verificam existenta filtrului}
  if chkFilter.Checked then begin
    CheckAndDisable(GridRegistruINCASARI, True);
    CheckAndDisable(GridRegistruPLATI, True);
    CheckAndDisable(GridRegistruPOZ, True);
    CheckAndDisable(GridRegistruDATA, True);
  end;

  if Assigned(aNode.Parent) then begin
    DBExplicProj.Enabled := (CurentrbTag = rbProiect.Tag);
//    if rbProiect.Checked then begin
//      CheckAndDisable(GridRegistruCONT_CSP, True);
//      CheckAndDisable(GridRegistruCODGEST, True);
//    end;
    {why!!}
    {DisableEditor(aNode.Parent);}
  end
  else
    begin
      DBExplicProj.Enabled := False;
      //for I:= 0 to GridRegistru.VisibleColumnCount-1 do  DisableInRight(GridRegistru.VisibleColumns[I]);
    end;
end;

procedure TFrmRegistru.Cmd_EchilibrarePlataExecute(Sender: TObject);
var Node,
    lNode: TdxDBTreeListNode;
    I : Integer;
    lTotal, lPartial: Currency;
    OldState: Boolean;
begin
  { Aici verificam daca nu cumva se dezechilibreaza nota }
  Node := TdxDBTreeListNode(GridRegistru.FocusedNode);
  if Node = nil then Exit;
  if (Node.Strings[GridRegistruID_PARINTE.Index] = '') or (Node.Parent = nil) then
     raise EContaHandledError.Create(StrECLErrorOnParent);

  lNode := TdxDBTreeListNode(Node.Parent);
  lTotal := GetUnitedValue(lNode);
  lPartial := 0;
  for I := 0 to lNode.Count-1 do
    if lNode.Items[I] <> Node then lPartial := lPartial + GetUnitedValue(lNode.Items[I]);

  { Salvam Valoarea pe pozitia curenta }
  with MemRegistru do begin
    //DisableControls;
    GridRegistru.BeginUpdate;
    try
       if MemRegistru.FieldByName('ID_LISTA').AsString <> Node.Id then
          if not MemRegistru.Locate('ID_LISTA', Node.Id, []) then
             raise EContaHandledError.Create( Format(StrErrorOnECLNote,[IntToStr(Node.Id)]) );
       OldState := MemRegistru.State in [dsEdit, dsInsert];
       if not OldState then MemRegistru.Edit;
       MemRegistru.FieldByName(GetFieldIndex(lNode)).AsCurrency := lTotal - lPartial;
       MemRegistru.Post;
       if OldState then MemRegistru.Edit;
       StructuraActualizare(TdxDBTreeListNode(lNode));
       PostMessage(Handle, WM_SET_STARE_SOLD, Integer(StrActualizare), Integer(True));
    finally
       //EnableControls;
       GridRegistru.EndUpdate;
    end;

  end;
end;

function TFrmRegistru.GetFieldIndex(aNode: TdxTreeListNode): String;
begin
//  if aNode.Strings
  if GetRealAbsoluteLineValue(aNode)< 0 then Result := 'PLATI'
  else Result := 'INCASARI';
end;

procedure TFrmRegistru.pnDetailResize(Sender: TObject);
begin
   DBExplicCont.Width := pnDetail.Width div 3;
   pnFilter.Width := pnDetail.Width div 3
end;

procedure TFrmRegistru.gbInformationDblClick(Sender: TObject);
begin
  pnTop.Height := pnTop.Height xor (35+Integer(Boolean(FIsAvans))*15 );
end;

procedure TFrmRegistru.GridRegistruPROJCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
var lNode{, lParentNode}: TdxDBTreeListNode;
//    lEditabil: Boolean;
    CheckedProjList : TStringList;
begin
  with TdxDBTreeListPopupColumn(Sender) do begin
    CheckedProjList := frmCasaContainer.GetProjList(TdxDBTreeList(PopupControl));
    if Accept then begin
       lNode := TdxDBTreeListNode(GridRegistru.FocusedNode);
       if Sender = GridRegistruPROJ then
         AdaugaCopiiProjCamp(CheckedProjList, 'ID_PROIECT', lNode)
       else
       if Sender = GridRegistruTIP_CHELTVEN then
         AdaugaCopiiProjCamp(CheckedProjList, 'ID_TIPURI_CHELTVEN', lNode)
       else
         AdaugaCopiiProjCamp(CheckedProjList, FieldName, lNode);
       {if CheckedProjList.Count > 1 then AdaugaCopiiProjCamp(CheckedProjList, FieldName, lParentNode)
       else
         if Assigned(lNode) then begin
            lEditabil := Field.DataSet.State in [dsEdit, dsInsert];
            if not lEditabil then Field.DataSet.Edit;
            Field.Value := lNode.Id;
            Field.DataSet.Post;
            if lEditabil then Field.DataSet.Edit;
            Text := lNode.Id;
            Accept := False;
         end;}
         if not VarIsNull(lNode.Values[GridRegistruCONT_CSP.Index]) then
          if VarToStr(lNode.Values[GridRegistruCONT_CSP.Index]) <>'%' then
          if VarToStr(lNode.Values[GridRegistruCONT_CSP.Index]) <>'' then
             GridRegistruCONT_CSP.ReadOnly := True;

    end;
    ClearCheckState(TdxDBTreeList(PopupControl), CheckedProjList);
    CheckedProjList.Clear;
  end;
end;

procedure TFrmRegistru.TreeProiecteGetSelectedIndex(Sender: TObject;
  Node: TdxTreeListNode; var Index: Integer);
begin
  Index := Node.ImageIndex;
end;

procedure TFrmRegistru.TreeProiecteDblClick(Sender: TObject);
begin
  with TdxDBTreeList(Sender) do
    if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
end;

procedure TFrmRegistru.TreeProiecteKeyDown(Sender: TObject; var Key: Word;
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

procedure TFrmRegistru.ValidareProj(Sender: TField);
var
  lValue : Variant;
begin
  if InternalAdd then Exit;
  if Sender.IsNull then  Exit;
  InternalValidateExact(Sender.AsString, frmCasaContainer.TreeFunctional, lValue, 1, -1);
  if lValue <> null then
    Sender.DataSet.FieldByName('COD_FUNCTIONAL').AsString := lValue;
  {if Sender.AsString = '%' then begin
    MemRegistruECL.AsInteger := 0;
  end
  else
     InternalValidateCont(Trim(Sender.AsString), frmCasaContainer.TreeFunctional);}
end;

procedure TFrmRegistru.FormDestroy(Sender: TObject);
var I : Integer;
    Dec : PDEcontInf;
    Hint : PHintRec;
begin

  DefaultHouseId := FCurentHouse;

  Saved_DataCasa := edData.Date;
  Saved_TipDefalcare := 0 * Integer(rbFara.Checked) + 1 * Integer(rbCont.Checked) + 2* Integer(rbProiect.Checked);
  Saved_PeZI := chkEfectiv.Checked;
  Saved_ZileAnt := edNrZile.IntValue;

  WriteSettingsRegistru;

  GridRegistru.OnDeletion := nil;

  if Assigned(DeconturiList) then begin
    for I := DeconturiList.Count-1 downto 0 do begin
        Dec := PDEcontInf(DeconturiList.Objects[I]);
        Dispose(Dec);
    end;
    DeconturiList.Free;
  end;

  if Assigned(FHintProjList) then begin
      for I := FHintProjList.Count-1 downto 0 do begin
        Hint := PHintRec(FHintProjList.Objects[I]);
        Dispose(Hint);
    end;
    FHintProjList.Free;
  end;

  if Assigned(StrActualizare) then Dispose(StrActualizare);
  if Assigned(frmSearchErrors) then frmSearchErrors.Free;
  if Assigned(ProgressForm) then ProgressForm.Free;
  if Assigned(LegendForm) then LegendForm.Free;

  StorageWriteDxTree(GridRegistru);

  ErrorUnit.IfAvailableDestroy;
end;

procedure TFrmRegistru.AdaugaCopiiBugetCamp(aList: TStringList;
  aFieldName: String; const ANode: TdxDBTreeListNode);
var
    I:Integer;
  lSum      : Currency;
    LineInfo : TLineNodeInfo;
  lLastPos  : TBookMark;
  lSearchID,
  lContCsp,
  lCodGest  : Variant;
  lField    : TField;
  lObjInfo  : PDateLista;
begin
  {procedura adauga dupa ce s-au selectat proiectele de distributie si adauga copii pentru nodul curent selectat}
  {folosim lnode ca baza pentru copii ce vor urma}
  if Assigned(ANode) then begin
    IsInLoad  := True;
    GetLineInfo(ANode, LineInfo);
    GridRegistru.BeginUpdate;
    MemRegistru.DisableControls;
  try
      lField    := MemRegistru.FieldByName(GetFieldIndex(ANode));
      lSum      := GetUnitedValue(ANode);
      lContCsp  := MemRegistru['CONT_CSP'];
      lCodGest  := MemRegistru['CODGEST'];
      if (ValueSafeToStr(lContCsp) = '') or (ValueSafeToStr(lContCsp) = '%') then
        lContCsp := Null;

      if Assigned(ANode.Parent) then
        lSearchID := ANode.ParentId
      else
        lSearchID := ANode.Id;

      lLastPos  := MemRegistru.GetBookmark;
    try
        if MemRegistru.Locate('ID_LISTA', lSearchID, []) then begin
          DBGoEdit(MemRegistru);
          MemRegistru['ON_SERVER'] := 0;
        MemRegistru.Post;
        FIsModified := True;
      end;
    finally
        MemRegistru.GotoBookmark(lLastPos);
        MemRegistru.FreeBookmark(lLastPos);
    end;

    for I:= 0 to aList.Count-1 do begin
      {adaugam in ecran inregistariile cu id_proiect selectate}
        if (I <> 0) or not Assigned(ANode.Parent) then
         AdaugaMemRegistruNew(LineInfo, True);
         //AdaugaMemRegistru(lNode, True);
        DBGoEdit(MemRegistru);
        lField.AsCurrency := RoundTo(lSum/aList.Count, -1*CurrDecimal);
         if I=aList.Count-1 then
          lField.AsCurrency := lField.AsCurrency + lSum - aList.Count*(RoundTo(lSum/aList.Count, -1*CurrDecimal));
        lObjInfo := PDateLista(aList.Objects[I]);
        MemRegistru['DETALII_BUGET']            := lObjInfo^.Descriere;
        MemRegistru['COD_FUNCTIONAL']           := lObjInfo^.CodFunctional;
        MemRegistru['ID_OI_UNITATI']            := lObjInfo^.IdUnitate;
        MemRegistru['COD_ECONOMIC']             := lObjInfo^.CodEconomic;
        MemRegistru['ID_OI_PROIECTE']           := lObjInfo^.IdProiect;
        MemRegistru['ID_ANGAJAMENTE_DEFALCARE'] := lObjInfo^.IdAng;
        MemRegistru['ID_ORDONANTARE_DEFALCARE'] := lObjInfo^.IdOrd;
        MemRegistru['CONT_CSP']                 := lContCsp;
        MemRegistru['CODGEST']                  := lCodGest;
        DBPost(MemRegistru);
    end;
  finally
      MemRegistru.EnableControls;
      GridRegistru.EndUpdate;
      chkExpandClick(chkExpand);            // In cazul in care avem defalcare fortam o expandare la nodurile nou adaugate
    IsInLoad := False;
  end;
end;

end;

procedure TFrmRegistru.AdaugaCopiiProj(aList: TStringList);
var I:Integer;
    aNewId : Integer;
    lNode : TdxDBTreeListNode;
    LineInfo : TLineNodeInfo;
begin
  {procedura adauga dupa ce s-au selectat proiectele de distributie si adauga copii pentru nodul curent selectat}
  lNode := TdxDBTreeListNode(GridRegistru.FocusedNode);
  if lNode = nil then Exit;
  GridRegistru.BeginUpdate;
  for I:=0 to aList.Count-1 do begin
    {adaugam in ecran inregistariile cu id_proiect selectate}
    aNewId :=  StrToInt(aList.Strings[I]);
    //AdaugaMemRegistru(lNode, True);
    GetLineInfo(lNode, LineInfo);
    AdaugaMemRegistruNew(LineInfo, True);
    with MemRegistru do begin
       if not(State in [dsEdit, dsInsert]) then Edit;
       FieldByName('ID_PROIECT').AsInteger := aNewId;
       //todo retrive project name
    end;
  end;
  GridRegistru.EndUpdate;
end;

procedure TFrmRegistru.AdaugaMemRegistru(aNode: TdxDBTreeListNode;
  ForcedEntry : Boolean);
begin
  {procedura adauga un copil dup nodul aNode}
  if chkFilter.Checked then Exit;
  with MemRegistru do begin
    if (Assigned(aNode) and (GetRealLineValue(aNode)<>0)) or (not Assigned(GridRegistru.LastNode)) then
      try
         GridRegistru.BeginUpdate;
         IsInAdd := True;
         Append;
         FieldByName('COD_CB').AsInteger := FCurentHouse;
         FieldByName('C_O').AsInteger := IdUtilizator;//FSignLogin;

         if FIsAvans then
           FieldByName('DATA').AsDateTime := edtDataDecont.Date
         else
           if chkEfectiv.Checked then
              FieldByName('DATA').AsDateTime := edData.Date
           else
              FieldByName('DATA').AsDateTime := Date;

         FieldByName('DATAEM').AsDateTime := Date;

         FieldByName('COD_CB').AsInteger := FCurentHouse;
         FieldByName('TRANSFER').AsInteger := 0;
         FieldByName('COD_TRANSFER').AsInteger := 0;

         if (not Assigned(aNode)) or ((aNode.Strings[GridRegistruECL.Index] = '1') and (not ForcedEntry)) then
         begin
            FieldByName('ID_PARINTE').Value := Null;
         end
         else begin
            FieldByName('TRANSFER').AsInteger := GetAsInteger(aNode, GridRegistruTRANSFER.Index);
            if Assigned(aNode.Parent) then begin
               FieldByName('ID_PARINTE').Value := aNode.ParentId;
               if VarIsType(aNode.Parent.Values[GridRegistruDATA.Index], varDate) then
                    FieldByName('DATA').AsDateTime := VarToDateTime(aNode.Parent.Values[GridRegistruDATA.Index]);
             end
            else begin
               FieldByName('ID_PARINTE').Value := aNode.Id;
               if VarIsType(aNode.Values[GridRegistruDATA.Index], varDate) then
                  FieldByName('DATA').AsDateTime := VarToDateTime(aNode.Values[GridRegistruDATA.Index]);
            end;
         end;

         FieldByName('ECL').AsInteger := 0;
         {pozitia in cadru zilei}
         if (DataEmitere = 0) and (Assigned(aNode)) then
           DataEmitere := aNode.Values[GridRegistruDATA.Index]
         else
           DataEmitere := FieldByName('DATA').AsDateTime;

         FieldByName('POZ').AsInteger := GetNextPosition(DataEmitere, Trim(FieldByName('ID_PARINTE').AsString));

         if FCodDecont > 0 then begin
            FieldByName('NR_DECONT').AsInteger := Round(edtNrDecont.Value);
            FieldByName('DATA_DECONT').AsDateTime := edtDataDecont.Date;
            FieldByName('PARENT_COD').AsInteger := FCodDecont;
         end;

         FieldByName('ON_SERVER').AsInteger := 0;
         Post;
         if (CurentrbTag = 2) and VarIsNull(FieldByName('ID_PARINTE').Value) then // defalcare pe proiecte
            GridRegistruCONT_CSP.ReadOnly := False;
      finally
        //EnableControls;
        IsInAdd := False;
        GridRegistru.EndUpdate;
      end;
    end;
end;

procedure TFrmRegistru.ClearCheckState(aTree: TdxDBTreeList; aCheckState : TStringList);
var I:Integer;
   aNode : TdxTreeListNode;
begin
  aTree.BeginUpdate;
  for I:= 0 to aCheckState.Count-1 do begin
    aNode := aTree.FindNodeByKeyValue(StrToInt(aCheckState.Strings[I]));
    while aNode <> nil do begin
       aNode.ImageIndex := 0;
       aNode := aNode.Parent;
    end;
  end;
  aTree.EndUpdate;
end;

procedure TFrmRegistru.Cmd_DeletePlataExecute(Sender: TObject);
var
  lNode: TdxDBTreeListNode;
  I: Integer;
  lMessage: String;
  ErrorList: TStringList;
  ErrString: String;

function CanDelete(aNode: TdxTreeListNode): Boolean;
begin
  Result := True;
  if aNode <> nil then
    Result := not (GetAsInteger(aNode, GridRegistruVALIDATA.Index) in [1, 3, 4]);
end;

begin
  if Not Assigned(GridRegistru.FocusedNode) then Exit;
  lNode := TdxDBTreeListNode(GridRegistru.FocusedNode);

  ErrorList := TStringList.Create;

  if GridRegistru.SelectedCount > 1 then
    lMessage := StrQuestionOnParents
  else if (Assigned(lNode)) and (lNode.HasChildren) then
    lMessage := StrQuestionWithChild
  else
    lMessage := StrQuestionOnParent;

  if MessageDlg(lMessage, mtConfirmation, [mbYes, mbNo], 0) = mrYes then
  begin
    if GridRegistru.SelectedCount > 1 then
      for I := GridRegistru.SelectedCount - 1 downto 0 do
      begin
        lNode := TdxDBTreeListNode(GridRegistru.SelectedNodes[I]);
        if CanDelete(lNode) then
          SoftDeleteCurrentRecord
        else
          ErrorList.Add(
            'Înregistrarea cu poziția ' + lNode.Strings[GridRegistruPOZ.Index] +
            ' Nr. ' + lNode.Strings[GridRegistruNRDOC.Index] + ' din ziua ' +
            lNode.Strings[GridRegistruDATA.Index]);
      end
    else if CanDelete(lNode) then
      SoftDeleteCurrentRecord
    else
      ErrorList.Add(
        'Înregistrarea cu poziția ' + lNode.Strings[GridRegistruPOZ.Index] +
        ' Nr. ' + lNode.Strings[GridRegistruNRDOC.Index] + ' din ziua ' +
        lNode.Strings[GridRegistruDATA.Index]);
  end;

  if ErrorList.Count > 0 then
  begin
    for I := 0 to ErrorList.Count - 1 do
      ErrString := ErrString + #13#10 + ErrorList.Strings[I];
    MessageDlg('Următoarele înregistrări nu au putut fi șterse: ' + ErrString, mtError, [mbOK], 0);
  end;

  ErrorList.Free;
  GridRegistru.Invalidate;
  FIsModified := True;
end;


procedure TFrmRegistru.SyncronizeDataSets(NeedServerSupport: Boolean; TipDefalcare : Integer);
begin
  {daca este necesara sincronizarea cu serverul atunci sincronizam dataseturiile si incercam sa rezolvam conflictele }
   if NeedServerSupport then
      SaveToDb
   else begin
     {se face sincronizarea intre dataseturile locale}
     ProgressCaption := StrSynRegistru;
     SyncDataSet(MemRegistru, MemReg, True);
     {testam ce sincronizare face pentru copil}
     case TipDefalcare of
        0: ;
        1: begin
           ProgressCaption := StrSyncCont;
           SyncDataSet(MemRegistru, MemCont, False);
        end;
        2: begin
           ProgressCaption := StrSyncProj;
           SyncDataSet(MemRegistru, MemProj, False);
        end;
        4: begin
           ProgressCaption := StrSyncProj;
(*CHRIS SALVARE FACTURI
           SyncDataSet(MemRegistru, MemFact, False);
           SyncDataSet(MemRegistru, MemProj, False);
*)
        end;
     end;
   end;
end;

procedure TFrmRegistru.SyncDataSet(FromDataSet: TdxMemData; var ToDataSet: TdxMemData; Condition : Boolean);
var
   lOldFiltred : Boolean;
   lOldFilterEvent : TFilterRecordEvent;
begin
  FromDataSet.DisableControls;
  ToDataSet.DisableControls;
  {daca condite atunci numai registru}
  //salvam starea initala
  lOldFilterEvent := FromDataSet.OnFilterRecord;
  lOldFiltred := FromDataSet.Filtered;
  //stabilim filtrul
  FromDataSet.Filtered := True;
  if Condition then
    FromDataSet.OnFilterRecord := FilterDefalcareOff
  else
    {altfel numai defalcare}
    FromDataSet.OnFilterRecord := FilterDefalcareOn;

  if ToDataSet.Active then ToDataSet.Active := False;
  ToDataSet.Active := True;
  ToDataSet.LoadFromDataSet(FromDataSet);
  FromDataSet.Filtered := lOldFiltred;
  FromDataSet.OnFilterRecord := lOldFilterEvent;
  ToDataSet.EnableControls;
  FromDataSet.EnableControls;
end;

procedure TFrmRegistru.FilterDefalcareOff(DataSet: TDataSet; var Accept: Boolean);
begin
  Accept := (DataSet.FieldByName('ID_PARINTE').AsString = '');
end;

procedure TFrmRegistru.FilterDefalcareOn(DataSet: TDataSet; var Accept: Boolean);
begin
   Accept := (DataSet.FieldByName('ID_PARINTE').AsString <> '');
end;


procedure TFrmRegistru.AddRecordsToDB;
var
    aNewId : Integer;
    aCont, aProj : TZQuery;
//    aFact : TZQuery;
    aKey : String;
    aPkey : Integer;
    aRegistru : TZQuery;

begin
(*CHRIS SALVARE FACTURI
   with GetTmpADOQuery do
   try
     SQL.Add('SET IDENTITY_INSERT GEST_DECONTARI ON');
     ExecSQL;
   finally
     Free;
   end;
*)
    {MODIFICAM TOT CE ESTE PE SERVER}
    {bregistru}
    GridRegistru.BeginUpdate;
    GridRegistru.DataSource := nil;
    SyncronizeDataSets(False, CurentrbTag);
    MemRegistru.DisableControls;
    {folosim optiunea BatchOptimistic pentru a nu ransfera de fiecare data inregistrari}
    aRegistru := DBNewUpdateQuery('SELECT TOP 0 * FROM BREGISTRU ');
    aRegistru.CachedUpdates := True;
    aRegistru.Active := True;
    {breg_x}
    aCont := DBNewUpdateQuery('SELECT TOP 0 * FROM BREG_X');
    aCont.CachedUpdates := True;
    aCont.Active := True;
    {breg_p}
    aProj := DBNewUpdateQuery('SELECT TOP 0 * FROM BREG_P');
    aProj.CachedUpdates := True;
    aProj.Active := True;

(*CHRIS SALVARE FACTURI
    {facturi}
    aFact := GetTmpADOQuery;
    aFact.CachedUpdates := True;
    aFact.SQL.Add(' SELECT TOP 0 * FROM GEST_DECONTARI');
    aFact.Open;
*)

(*CHRIS SALVARE FACTURI
    LocalADOOption(aFact);
*)
    try
      MemReg.First;
      while not(MemReg.Eof) do begin
        {daca nu se afla deja pe server}
        if MemReg.FieldByName('ON_SERVER').AsInteger = 0 then
        begin
           {daca nu este copil atunci este din bregistru}
           aKey  := MemReg.FieldByName('ID_LISTA').AsString;
           aPKey := GetParentKeyId(MemReg, MemReg.FieldByName('ID_PARINTE').AsString);
           {luam noul id pentru registru}
           aNewId := CompleteDataSetForDB(aKey, 0, tl_fara, aRegistru, MemReg, aPKey);
           {pentru copii care trebuie dusi pe server completam id-ul parintelui}
           CompleteDataSetForDB(aKey, aNewId, tl_cont, aCont, MemCont,  aPKey);
           CompleteDataSetForDB(aKey, aNewId, tl_proiect, aProj, MemProj, aPKey);
(*CHRIS SALVARE FACTURI
           CompleteDataSetForDB(aKey, aNewId, tl_facturi, aFact, MemFact, aPKey);
*)
        end;
        MemReg.Next;
      end;

      {se face mutarea pe server}
(*CHRIS SALVARE FACTURI
      CompleteIdentity(aRegistru, 'BREGISTRU', 'COD', True, aCont, aProj, AFact);
*)
      CompleteIdentity(aRegistru, 'BREGISTRU', 'COD', True, aCont, aProj, nil);
      CompleteIdentity(aCont, 'BREG_X', 'ID_BREG_X', False, nil, nil, nil);
      CompleteIdentity(aProj, 'BREG_P', 'ID_BREG_P', False, nil, nil, nil);
(*CHRIS SALVARE FACTURI
      CompleteIdentity(aFact, 'GEST_DECONTARI', 'ID_GEST_DECONTARI', False, nil, nil, nil);
*)

      CompleteNewIds(aCont, 'BREGISTRU', 'BREG_COD');
      CompleteNewIds(aProj, 'BREGISTRU', 'BREG_COD');
(*CHRIS SALVARE FACTURI
      CompleteNewIds(aFact, 'BREGISTRU','ID_BREGISTRU');
*)
      SaveValidation;
      ProcessDeleted;
          LocalSyncronizeSharePoint;
        DBStartTransaction;
      try

          if QryShare_Point.UpdateStatus in [usModified, usInserted, usDeleted] then begin QryShare_Point.ApplyUpdates; QryShare_Point.CommitUpdates; end;
        if aRegistru.UpdateStatus in [usModified, usInserted, usDeleted] then begin aRegistru.ApplyUpdates; aRegistru.CommitUpdates; end;
        if aCont.UpdateStatus     in [usModified, usInserted, usDeleted] then begin aCont.ApplyUpdates; aCont.CommitUpdates; end;
        if aProj.UpdateStatus     in [usModified, usInserted, usDeleted] then begin aProj.ApplyUpdates; aProj.CommitUpdates; end;
(*CHRIS SALVARE FACTURI
        if aFact.UpdateStatus     in [usModified, usInserted, usDeleted] then aFact.UpdateBatch;
*)
        DeleteShare;
        DBCommit;
      except
        on E: Exception do begin
          DBRollBack;
          //MessageDlg('A intervenit o erroare la salvarea datelor' + #13#10 + E.Message,mtError,[mbOK],0);
          raise Exception.Create('A intervenit o erroare la salvarea datelor' + #13#10 + E.Message);
        end;
      end;

    finally
      {se face eliberarea resurselor}
      aRegistru.Free;
      aCont.Free;
      aProj.Free;
(*CHRIS SALVARE FACTURI
      aFact.Free;
*)
      MemRegistru.EnableControls;
      GridRegistru.DataSource := DTRegistru;
      GridRegistru.EndUpdate;
      DBExecSQL('SET IDENTITY_INSERT GEST_DECONTARI OFF');
    end;
end;

procedure TFrmRegistru.SaveToDb;

  function IsDataConflict(aDb: String): Boolean;
  begin
    Result := ValueSafeToInt(DBGetScallarFmt('exec [share_conflict] %s, %d', [ValueToStr(aDB), FSignLogin])) > 0;
  end;

  function ShareType(aTableName: String): TTipLista;
  begin
    Result := tl_fara;
    if aTableName = ShareTables[1, 0] then
      Result := tl_cont
    else if aTableName = ShareTables[2, 0] then
      Result := tl_proiect
    else if aTableName = ShareTables[3, 0] then
      Result := tl_facturi;
  end;

  function GetMemDataByType(aTip: TTipLista): TdxMemData;
  begin
    Result := nil;
    case aTip of
      tl_fara: Result := MemReg;
      tl_cont: Result := MemCont;
      tl_proiect: Result := MemProj;
      (*CHRIS SALVARE FACTURI
      tl_facturi: Result := MemFact;
      *)
    end;
  end;

var
  I: Integer;
  IsConflict: Boolean;
  aTip: TTipLista;
  aMemData: TdxMemData;
  FilterEvent: TFilterRecordEvent;
  lWasFiltred: Boolean;
  OldIsInLoad: Boolean;
  bm: TBookMark;
begin
  // Dacă dataset-ul e în stare de editare/inserare, postăm modificările curente
  if MemRegistru.State in [dsEdit, dsInsert] then
    MemRegistru.Post;

  lWasFiltred := MemRegistru.Filtered;
  FilterEvent := MemRegistru.OnFilterRecord;
  MemRegistru.OnFilterRecord := nil;
  MemRegistru.Filtered := False;

  try
    VerifyBeforeSave;

    // Parcurgem toate înregistrările din MemRegistru și setăm câmpul STARE la 1
    bm := MemRegistru.GetBookmark;
    try
      MemRegistru.DisableControls;
      MemRegistru.First;
      while not MemRegistru.Eof do
      begin
        // Setăm STARE la 1 pentru fiecare record
        MemRegistru.Edit;
        MemRegistru.FieldByName('STARE').AsInteger := 1;
        MemRegistru.Post;
        MemRegistru.Next;
      end;
    finally
      MemRegistru.GotoBookmark(bm);
      MemRegistru.FreeBookmark(bm);
      MemRegistru.EnableControls;
    end;

    // Sincronizăm dataseturile
    if not(IsSyncro) then
      SyncronizeDataSets(False, CurentrbTag);

    try
      IsConflict := False;
      for I := 0 to High(ShareTables) do
        if IsDataConflict(ShareTables[I, 0]) then
        begin
          aTip := ShareType(ShareTables[I, 0]);
          aMemData := GetMemDataByType(aTip);
          // Rezolvăm conflictul, dacă este cazul
          if HandleConflict(aMemData, FStartInterval, FEndInterval, FCurentHouse, aTip) then
          begin
            OldIsInLoad := IsInLoad;
            IsInLoad := True;
            SyncDataSet(aMemData, MemRegistru, True);
            IsInLoad := OldIsInLoad;
          end;
        end;
      // După rezolvarea conflictelor
      if not(IsConflict) then
        AddRecordsToDB
      else
      begin
        SaveValidation;
        ProcessDeleted;
        try
          DBStartTransaction;
          LocalSyncronizeSharePoint;
          if QryShare_Point.UpdateStatus in [usModified, usInserted, usDeleted] then
          begin
            QryShare_Point.ApplyUpdates;
            QryShare_Point.CommitUpdates;
          end;
          DeleteShare;
          DBCommit;
        except
          DBRollBack;
        end;
      end;
      FIsModified := False;
      RecSterse := nil;
      RecValidate := nil;
    except
      on E: Exception do
      begin
        try
          SaveToLocalComputer;
        except
          on LocalE: Exception do
          begin
            raise Exception.CreateFmt('Eroare salvare date in local : '#13#10'%s'#13#10'În urma erorii de salvare pe server : '#13#10'%s', [LocalE.Message, E.Message]);
          end;
        end;
        FIsModified := False;
        RecSterse := nil;
        RecValidate := nil;
        FHasDied := True;
        raise Exception.CreateFmt('Eroare salvare date : %s', [E.Message]);
      end;
    end;
  finally
    MemRegistru.OnFilterRecord := FilterEvent;
    MemRegistru.Filtered := lWasFiltred;

    GridRegistru.FullExpand;
    RefreshDeconturi;
  end;
end;

procedure TFrmRegistru.DeleteShare(Sign : Integer);
var
  lDataSet  : TZReadOnlyQuery;
   I:Integer;
begin
  if FHasDied then Exit;
  if Sign = -1 then Sign := FSignLogin;
  lDataSet := DBNewQuery('exec [share_arhivare] :TABLE_NAME, :ID_UTILIZATOR, :KEY_FIELD, :IS_MODIFIED');
    try
    lDataSet.Params.ParamByName('ID_UTILIZATOR').Value := Sign;
    for I := Low(ShareTables) to High(ShareTables) do begin
      lDataSet.Params.ParamByName('TABLE_NAME').Value   := ShareTables[I, 0];
      lDataSet.Params.ParamByName('KEY_FIELD').Value    := ShareTables[I, 1];
      lDataSet.Params.ParamByName('IS_MODIFIED').Value  := FIsModified;
      lDataSet.ExecSQL;
      end;
    finally
    lDataSet.Free;
    end;
end;

procedure TFrmRegistru.MoveToArchive(aTableName: String; aId,
  aNewId: Integer; Sign : Integer);
begin
   if (aNewId = -1) or (FHasDied) then Exit;
   if Sign = -1 then Sign :=  FSignLogin;

       SHARE_MOVE(aId, aNewId, aTableName, Sign);
end;

function TFrmRegistru.GetSoldInitial: Currency;
begin
  Result := ValueSafeToCurrency( DBGetScallarFmt('exec [sp_get_sold_initial] %s, %d, %s',
                                    [
                                      ValueDateToStr(FStartInterval),
                                      FCurentHouse,
                                      ValueToStr(ValCodDecont)
                                    ], 0
                                  )
                               );
end;

function TFrmRegistru.CompleteDataSetForDB(aGeneratedId : String; aNewId: Integer;
  aTip: TTipLista; const aData: TZQuery; aMemData : TdxMemData; aParentId : Integer;const aTransferID : Integer) : Integer;

  function CompleteCont : Integer;
  begin
     {breg_x}
     aData.FieldByName('BREG_COD').Value   := aNewId;
     aData.FieldByName('CONT_CSP').Value   := aMemData.FieldByName('CONT_CSP').AsString;
     aData.FieldByName('VALOARE').Value    := aMemData.FieldByName('PLATI').AsCurrency + aMemData.FieldByName('INCASARI').AsCurrency;
     aData.FieldByName('EXPLICATIE').Value := aMemData.FieldByName('EXPLICATIE').AsString;
     aData.FieldByName('C_O').Value        := aMemData.FieldByName('C_O').AsInteger;
     aData.FieldByName('DATA').Value       := aMemData.FieldByName('DATA').AsDateTime;
     aData.FieldByName('POZ').Value        := aMemData.FieldByName('POZ').AsInteger;
     Result := GetLocalId;
     aData.FieldByName('ID_BREG_X').Value  := Result;
     aData.Post;
  end;

 function CompleteProj : Integer;
  begin
    {breg_p}
    aData.FieldByName('BREG_COD').Value   := aNewId;
    aData.FieldByName('VALOARE').Value    := aMemData.FieldByName('PLATI').AsCurrency + aMemData.FieldByName('INCASARI').AsCurrency;
    aData.FieldByName('EXPLICATIE').Value := aMemData.FieldByName('EXPLICATIE').AsString;
    aData.FieldByName('C_O').Value        := aMemData.FieldByName('C_O').AsInteger;
    aData.FieldByName('PEXPLIC').Value    := aMemData.FieldByName('PEXPLIC').Value;
    aData.FieldByName('CONT_CSP').Value := aMemData.FieldByName('CONT_CSP').AsString;
    aData.FieldByName('CODGEST').Value  := aMemData.FieldByName('CODGEST').AsString;

    if not VarIsNull(aMemData.FieldByName('ID_PROIECT').Value) then
       aData.FieldByName('ID_PROIECT').Value := aMemData.FieldByName('ID_PROIECT').AsInteger;

    aData.FieldByName('COD_FUNCTIONAL').Value := aMemData.FieldByName('COD_FUNCTIONAL').AsString;
    aData.FieldByName('COD_ECONOMIC').Value := aMemData.FieldByName('COD_ECONOMIC').AsString;
    if not VarIsNull(aMemData.FieldByName('ID_OI_UNITATI').Value) then
      aData.FieldByName('ID_OI_UNITATI').Value := aMemData.FieldByName('ID_OI_UNITATI').AsInteger;
    if not VarIsNull(aMemData.FieldByName('ID_OI_PROIECTE').Value) then
      aData.FieldByName('ID_OI_PROIECTE').Value := aMemData.FieldByName('ID_OI_PROIECTE').AsInteger;
    if not VarIsNull(aMemData.FieldByName('ID_ORDONANTARE_DEFALCARE').Value) then
      aData.FieldByName('ID_ORDONANTARE_DEFALCARE').Value := aMemData.FieldByName('ID_ORDONANTARE_DEFALCARE').AsInteger;
    if not VarIsNull(aMemData.FieldByName('ID_ANGAJAMENTE_DEFALCARE').Value) then
      aData.FieldByName('ID_ANGAJAMENTE_DEFALCARE').Value := aMemData.FieldByName('ID_ANGAJAMENTE_DEFALCARE').AsInteger;


    if not VarIsNull( aMemData.FieldByName('ID_TIPURI_CHELTVEN').Value) then
      aData.FieldByName('ID_TIPURI_CHELTVEN').Value := aMemData.FieldByName('ID_TIPURI_CHELTVEN').AsInteger;
    if not VarIsNull(aMemData.FieldByName('ID_ORGANIGRAMA').Value) then
      aData.FieldByName('ID_ORGANIGRAMA').Value := aMemData.FieldByName('ID_ORGANIGRAMA').AsInteger;
    if not VarIsNull(aMemData.FieldByName('ID_RESURSA').Value) then
     aData.FieldByName('ID_RESURSA').Value := aMemData.FieldByName('ID_RESURSA').AsInteger;
    aData.FieldByName('POZ').Value        := aMemData.FieldByName('POZ').AsInteger;
    Result := GetLocalId;
    aData.FieldByName('ID_BREG_P').Value  := Result;
    aData.Post;
  end;

  function CompleteFact : Integer;
  begin
    {gest_decontari}

    if aMemData.FieldByName('NR_LIST').AsInteger <> 0 then
      aData.FieldByName('ID_BREGISTRU').Value   := aMemData.FieldByName('NR_LIST').AsInteger
    else
      aData.FieldByName('ID_BREGISTRU').Value   := aNewId;
    DBExecSQLFmt('DELETE FROM GEST_DECONTARI WHERE ID_BREGISTRU = %s', [ValueToStr(aData['ID_BREGISTRU'])]);
    aData['SUMA']           := aMemData.FieldByName('PLATI').AsCurrency + aMemData.FieldByName('INCASARI').AsCurrency;
    aData['ID_GEST_DOCUM']  := aMemData.FieldByName('ID_GEST_DOCUM').AsInteger;
    Result := GetLocalId;
    aData.Fields.FindField('ID_GEST_DECONTARI').ReadOnly := False;
    aData.FieldByName('ID_GEST_DECONTARI').Value  := Result;
    aData.Post;
    DBExecSQL('UPDATE GEST_DOCUM SET ACHITAT = ISNULL(ACHITAT, 0) + :SUMA WHERE ID_GEST_DOCUM = :ID_GEST_DOCUM', aData);
  end;


  function CompleteRegistru(tId : Integer = -1) : Integer;
  var I  :Integer;
     aTransfer : Variant;
  begin
    if not(aData.State in [dsEdit, dsInsert]) then aData.Edit;
    Result := -1;
    for I := 0  to aData.FieldCount -1 do begin
     {daca campul este ce de identity}
        if aData.Fields[I].FieldName = 'COD' then begin
          Result := GetLocalId;
          aData.Fields[I].Value := Result;
        end
        else
         if (aData.Fields[I].FieldName = 'COD_TRANSFER') and (tId <> -1) then begin
            aData.Fields[I].Value := tId;
         end
         else
           if (aData.Fields[I].FieldName = 'TRANSFER') then begin
              case aMemData.FieldByName(aData.Fields[I].FieldName).AsInteger of
                 -1 : aTransfer := 0;
                 3 : aTransfer := 4;
                 7 : aTransfer := 8;
                 9 : aTransfer := 10;
                 12 : aTransfer := 1;
                 13 : aTransfer := 1;
                 else
                 aTransfer := aMemData.FieldByName(aData.Fields[I].FieldName).AsInteger;
              end;
              aData.Fields[I].Value := aTransfer;
           end
           else
              if aMemData.FindField(aData.Fields[I].FieldName) <> nil then
                 aData.Fields[I].Value := aMemData.FieldByName(aData.Fields[I].FieldName).AsVariant;
    end;

    //aData.FieldByName('VALIDATION_HASH').AsString := CalcRowHash(CommonDBVar.ValidationKey, aData);
    aData.Post;
    //actualizam in aData si pentru tId cod_transfer
    if tId <> -1 then
       if aData.Locate('COD', tId, []) then begin
         if not (aData.State in [dsEdit, dsInsert]) then aData.Edit;
         aData.FieldByName('COD_TRANSFER').AsInteger := Result;
         aData.Post;
       end;
  end;

var
   aBasicField, aTableName : String;
   aResult :  Integer;
   IdDocum : integer;
   Iese : Boolean;
   aId : Integer;
   aBookMark  : TBookMark;
begin
  case aTip of
    tl_fara : begin
       aBasicField := 'ID_LISTA';
       aTableName := 'BREGISTRU';
    end;

    tl_cont : begin
      aBasicField := 'ID_PARINTE';
      aTableName  := 'BREG_X';
    end;

    tl_proiect : begin
       aBasicField := 'ID_PARINTE';
       aTableName  := 'BREG_P';
    end;
    tl_facturi : begin
       aBasicField := 'ID_PARINTE';
       aTableName  := 'GEST_DECONTARI';
    end;
  end;

  aResult := aParentId;

  Iese := False;
  try
    aBookMark := aMemData.GetBookmark;
    aMemData.First;
    IdDOcum := -1;
    if aMemData.Locate(aBasicField, aGeneratedId, []) then
      while (aMemData.FieldByName(aBasicField).AsString = aGeneratedId) and not(Iese) do begin
         aId := -1;
         case aTip of
           tl_fara    : begin
             {completam eventualul transfer}
             if aParentId > 0 then
               aResult := aParentId
             else begin
               aData.Append;
               if (aMemData.FieldByName('TRANSFER').AsInteger in [3,7,9, 12, 13])
               or (aMemData.FieldByName('TRANSFER').AsInteger =-1) //copie identica
               //or (aMemData.FieldByName('COD_TRANSFER').AsInteger in [3,7,9])
                then begin
                 aId := PuneTransfer(aMemData, aData);
                 aData.Post;
                 aData.Append;
               end;
               aResult := CompleteRegistru(aId);
             end;
           end;
           tl_cont    : begin
             if aTransferID <> 0 then begin
               aData.Append;
               CompleteCont;
             end;
             aData.Append;
             aResult := CompleteCont;
           end;
           tl_proiect : begin
             aData.Append;
             aResult := CompleteProj;
           end;
           tl_facturi  : begin
             if IdDocum <> aMemData.FieldByName('ID_GEST_DOCUM').AsInteger then
             begin
               aData.Append;
               aResult := CompleteFact;
               IdDOcum := aMemData.FieldByName('ID_GEST_DOCUM').AsInteger;
             end;
              with GetTmpADOQuery do
              try
                begin
                   SQL.Add('INSERT INTO GEST_DEFALCARE_DECONTARI ');
                   SQL.Add('(ID_GEST_DECONTARI, ID_GEST_ITEMSI, SUMA) VALUES ');
                   SQL.Add('(' + IntTOStr(aResult)+','+ IntTOStr(aMemData.FIeldByName('ID_UNIC_MODUL').AsInteger)+', :SUMA)');
                   Params.Parambyname('SUMA').Value := aMemData.FieldByName('PLATI').AsCurrency + aMemData.FieldByName('INCASARI').AsCurrency;
                   ExecSQL;
                end;
              finally
                Free;
              end;
           end;
         end;
         if aMemData.FieldByName('ID_LISTA').AsString >'1.' then
            MoveToArchive(aTableName, aMemData.FieldByName('COD').AsInteger, aResult);
         aMemData.Next;
         Iese := aMemData.Eof;
      end;
    finally
      aMemData.GotoBookmark(aBookMark);
      aMemData.FreeBookmark(aBookMark);
    end;

  {ne intereseaza numai cand se completeaza BREGISTRU pentru a avea id-ul pentru celelalte tabele}
  Result := aResult;
end;

procedure TFrmRegistru.MemRegistruINCASARIChange(Sender: TField);
var aNode, aRefNode : TdxTreeListNode;
    lTotalDef, lTotalCod : Currency;
    I  : Integer;
    EclCondition : Boolean;
begin
  VerificaPozitieEchilibrata();
    if not Assigned(GridRegistru.FocusedNode) then Exit;
  aRefNode := GridRegistru.FocusedNode;
  CalculateSold(GridRegistru.FocusedNode);

  if Assigned(aRefNode.Parent) then begin
     aNode := aRefNode.Parent;
     lTotalDef := GetUnitedValue(aRefNode);
  end
  else begin
    aNode := aRefNode;
    if aNode.Count = 0 then lTotalDef := GetUnitedValue(aNode) else lTotalDef := 0;
  end;

  lTotalCod := GetUnitedValue(aNode);

  for I := 0 to aNode.Count-1 do
  begin
      if not aNode.Items[i].HasChildren then
      begin
       if aNode.Items[I] = aRefNode then lTotalDef := lTotalDef + 0 //GetUnitedValue(aRefNode)
       else lTotalDef := lTotalDef + GetUnitedValue(aNode.Items[I]);
      end;
  end;

  EclCondition := True;
  case CurentrbTag of
    0 : EclCondition := True;
    1 : EclCondition := GridRegistru.FocusedNode.Strings[GridRegistruCONT_CSP.Index]<>'%';
    2 : EclCondition := GridRegistru.FocusedNode.Strings[GridRegistruPROJ.Index]<>'%';
  end;
  StructuraActualizare(TdxDBTreeListNode(aNode));
  PostMessage(Handle, WM_SET_STARE_SOLD, Integer(StrActualizare), Integer((ECLCondition) and (lTotalDef = lTotalCod) and (lTotalDef+lTotalCod <>0)));
  {trebuie luat in considerare daca se modifica suma din plati sau incasari si la transferul facut}
end;

procedure TFrmRegistru.GlobalChange(Sender: TField);
const
  FieldNames:  array[0..20, 0..1] of String[20] =
  (('CODGEST', '0'),
   ('CURS_SCHIMB', '0'),
   ('DATA', '1'),
   ('TIPDOC', '0'),
   ('NRDOC', '0'),
   ('POZ', '1'),
   ('EXPLICATIE', '0'),
   ('INCASARI', '1'),
   ('PLATI', '1'),
   ('CONT_CSP', '0'),
   ('VAL_CRSP', '0'),
   ('ACHITAT', '0'),
   ('NR_LIST', '0'),
   ('ID_PROIECT', '0'),
   ('PROIECT', '0'),
   ('SOLD', '0'),
   ('TRANSFER', '0'),
   ('ID_ORGANIGRAMA', '0'),
   ('ID_TIPURI_CHELTVEN', '0'),
   ('ID_RESURSA', '0'),
   ('VALIDATA', '0')
   );
var
  I : Integer;
  lPrevEdit : Boolean;
begin
  if IsInLoad then Exit;
  for I := 0 to High(FieldNames) do
    if Sender.FieldName = FieldNames[I,0] then begin
      lPrevEdit := DBGoEdit(Sender.DataSet);
      DBSetFieldValue(Sender.DataSet, 'ON_SERVER', 0);
         PostOnParent(Sender.DataSet, Sender.DataSet.FieldByName('ID_PARINTE').AsString, 'ON_SERVER', 0);
       if FieldNames[I,1] ='1' then MemRegistruINCASARIChange(Sender);
      if lPrevEdit then DBGoEdit(Sender.DataSet);
       //AssignOnChange(True);
    end;
  FIsModified := True;
end;


procedure TFrmRegistru.AssignOnChange(State : Boolean);
var I : Integer;
begin
  for I := 0 to MemRegistru.FieldCount -1 do begin
    if State then
      MemRegistru.Fields[I].OnChange := GlobalChange
    else
      MemRegistru.Fields[I].OnChange := nil;
  end;
end;

constructor TFrmRegistru.Create(AOwner: TComponent);
begin
  inherited Create(AOwner);
  FEvaluator := TATSEvaluator.Create(Self);
end;

procedure TFrmRegistru.CreateProgressState;
begin
  ProgressForm := TfrmProgress.Create(Self);
  {
  MemCont.OnLoadProgress     := ProgressDataset;
  MemProj.OnLoadProgress     := ProgressDataset;
(*CHRIS SALVARE FACTURI
  MemFact.OnLoadProgress     := ProgressDataset;
*)
  MemReg.OnLoadProgress      := ProgressDataset;
  MemRegistru.OnLoadProgress := ProgressDataset;
//  QryShare_Point.OnFetchProgress := ProgressADODataset;
  }
end;

procedure TFrmRegistru.ProgressDataset(DataSet: TDataSet; Progress,
  MaxProgress: Integer);
var DivFraction : Integer;
begin
  if ProgressStep <= 0 then Exit;
  if (not Assigned(ProgressForm)) then Exit;
  DivFraction := (MaxProgress * ProgressStep) div 100;
  if DivFraction = 0 then DivFraction := 1;
  if ((Progress mod DivFraction <> 0) and (Progress <> MaxProgress) and (Progress <> 1))  then Exit;
  {pe declasare setam captionul si detalile pentru progress}
  if Progress = 1 then
     with ProgressForm do  begin
       GlobalLoadProgress.Position := 0;
       GlobalLoadProgress.Min := 0;
       GlobalLoadProgress.max := MaxProgress;
       Caption := ProgressCaption;
       PostMessage(Self.Handle, WM_HideProgress, 1,1);
     end;

  ProgressForm.GlobalLoadProgress.Position := Progress;

  if Progress = MaxProgress then
    PostMessage(Self.Handle, WM_HideProgress, 0,0);
end;

procedure TFrmRegistru.WmDropDownImgColumn(var Message: TMessage);
begin
  if Assigned(GridRegistru.InplaceEditor) then begin
     if (GridRegistru.InplaceEditor is TdxInplaceDropDownEdit) and (GridRegistru.InplaceEditor.IsVisible) then
        TdxInplaceDropDownEdit(GridRegistru.InplaceEditor).DroppedDown := True;
     case Message.WParam of
       0: ;
       1: ;
     end;
  end;
end;

procedure TFrmRegistru.WmMoveToGridColIndex(var Message: TMessage);
begin
  with Message do TdxDBTreeList(WParam).FocusedColumn := LParam;
end;


procedure TFrmRegistru.GridRegistruKeyPress(Sender: TObject;
  var Key: Char);
begin
  if (Assigned(GridRegistru.InplaceEditor)) and (Key = '?')
      and
      (
      (GridRegistru.FocusedField = MemRegistru.FindField('ID_ORGANIGRAMA')) or
      (GridRegistru.FocusedField = MemRegistru.FindField('ID_RESURSA')) or
      (GridRegistru.FocusedField = MemRegistru.FindField('ID_TIPURI_CHELTVEN')) or
      (GridRegistru.FocusedField = MemRegistru.FindField('CONT_CSP')) or
      (GridRegistru.FocusedField = MemRegistru.FindField('ID_PROIECT')) or
      (GridRegistru.FocusedField = MemRegistru.FindField('CODGEST')) or
      (GridRegistru.FocusedField = MemRegistru.FindField('TIPDOC'))
      ) then
     PostMessage(GridRegistru.InplaceEditor.Handle, CM_DROPDOWNPOPUP, 0, 0);
end;

procedure TFrmRegistru.DBExplicContChange(Sender: TObject);
begin
  if MemRegistru.FieldByName('ON_SERVER').AsInteger <> 0 then
       MemRegistru.FieldByName('ON_SERVER').AsInteger := 0;
end;

procedure TFrmRegistru.Cmd_TransferaPlataExecute(Sender: TObject);
var aNode : TdxTreeListNode;
    aTransferStare, aDestStare : Integer;
    IsBanca, DestBanca: Boolean;
    aTipCasa : TTipCasa;
    aTransfer : TfrmTransfer;
    lDestDate : TDateTime;
    lTopIndex : Integer;
begin
  if not Assigned(GridRegistru.FocusedNode) then Exit;
  aNode := GridRegistru.FocusedNode;

  (*
  {daca linia are copii}
  if (aNode.HasChildren) then begin
     raise EContaHandledError.Create(StrTransferCopii);
     Exit;
  end;
  *)
  {daca suma esta validata atunci eroare}
  if GetAsInteger(ANode, GridRegistruVALIDATA.Index) in [1,3,4] then begin
     raise EContaHandledError.Create(StrTranferValidat);
     Exit;
  end;
  {daca suma curenta este o incasare}
  if (aNode.Strings[GridRegistruPLATI.Index]='') or (aNode.Values[GridRegistruPLATI.Index]=0) then begin
     raise EContaHandledError.Create(StrTranferIncasare);
     Exit;
  end;
  {daca linia este deja un transfer}
  if ( aNode.Strings[GridRegistruTRANSFER.Index]<>'') and (aNode.Strings[GridRegistruTRANSFER.Index]<>'0') then begin
     raise EContaHandledError.Create(StrTransferTransfer);
     Exit;
  end;

  lTopIndex := GridRegistru.TopIndex;
  aTransfer := TfrmTransfer.Create(self);
  with aTransfer do
    try
       aTipCasa := nil;
       aTipCasa := TTipCasa.Create;
       DefaultIndex := FCurentHouse;
       GetTipCasa(FCurentHouse, aTipCasa);
       IsBanca := aTipCasa.IsBanca;

       FTreeList := TreeStructura;
       SetTreeList;
       TipDestinatie := [tt_Casa, tt_Banca];

       edtCasa.Text := edCurentHouse.Text;
       edtSuma.Value := aNode.Values[GridRegistruPLATI.Index];
       if not TextToDateEx(aNode.Strings[GridRegistruDATA.Index], lDestDate) then
          lDestDate := Date;
       edtDataDest.Date := lDestDate;
       edtDataPlec.Date := lDestDate;
       edtDataPlec.Text := edtDataDest.Text;
       ShowModal;
       {daca este casa sau banca transferul difera}

       aTransferStare := Integer(not(IsBanca))*3 + Integer(IsBanca)*7;
       {daca se accepta transferul}
       if ModalResult = mrOk then begin
         DestroyTreeList;
         GetTipCasa(HouseIndex, aTipCasa);
         DestBanca := aTipCasa.IsBanca;
         if chkConfirm.Checked then
             aDestStare := Integer(not(DestBanca))*2 + Integer(DestBanca)*6
         else
             aDestStare := Integer(not(DestBanca))*1 + Integer(DestBanca)*5;
         MakeTransfer(aNode, HouseIndex, edtDataDest.Date, edtSuma.Value, aTransferStare, aDestStare ,not(chkConfirm.Checked), -1,0);
       end;
    finally
      aTipCasa.Free;
      Free;
      GridRegistru.TopIndex := lTopIndex;
    end;
end;

procedure TFrmRegistru.MakeTransfer(aNode: TdxTreeListNode;
  CasaAcceptor: Integer; DataAcceptare: TDateTime; Suma : Currency; Stare, StareAcceptor : Integer; IsAccepted :Boolean;
  aNrDecont : Integer; aDataDecont : TDateTime);
begin
 // if (aNode = nil) or (Assigned(aNode.Parent)){ or (aNode.HasChildren) }then Exit;
  try
     GridRegistru.BeginUpdate;
     //MemRegistru.DisableControls;
      with MemRegistru do begin
        {am localizat}
        if Locate('ID_LISTA', aNode.Strings[GridRegistruID_LISTA.Index], []) then begin
           if not(State in [dsEdit, dsInsert]) then Edit;
           if Stare = -1 then
             FieldByName('TRANSFER').Value      := -1
           else
             FieldByName('TRANSFER').AsInteger      := Stare;
           FieldByName('COD_CBT').AsInteger       := CasaAcceptor;
           FieldByName('DATA_ACCEPT').AsDateTime  := DataAcceptare;
           if StareAcceptor = -1 then
             FieldByName('COD_TRANSFER').Value      := null
           else
             FieldByName('COD_TRANSFER').AsInteger  :=  StareAcceptor;


           if not FIsAvans then begin
             if aNrDecont > 0 then
                FieldByName('NR_DECONT').AsInteger     := aNrDecont
             else
                FieldByName('NR_DECONT').Clear;

             if aDataDecont>0 then
                FieldByName('DATA_DECONT').AsDateTime  := aDataDecont
             else
                FieldByName('DATA_DECONT').Clear;
           end;
           Post;
        end;
      end;
   finally
      //  MemRegistru.EnableControls;
      GridRegistru.EndUpdate;
      GridRegistru.Invalidate;
   end;
end;

procedure TFrmRegistru.Cmd_AcceptaTransferExecute(Sender: TObject);
var aNode : TdxTreeListNode;
    aTransferStare, aSwitch : Integer;
    IsBanca : Boolean;
    aTipCasa : TTipCasa;
    lDestDate : TDateTime;
    lTopIndex : Integer;
begin
  if not Assigned(GridRegistru.FocusedNode) or (GridRegistru.FocusedNode.Strings[GridRegistruTRANSFER.Index]='') then Exit;
  aNode := GridRegistru.FocusedNode;

  {daca linia este copil}
  if Assigned(aNode.Parent) then begin
     raise EContaHandledError.Create(StrAcceptareCopii);
     Exit;
  end;
//  {daca suma esta validata atunci eroare}
//  if GetAsInteger(ANode, GridRegistruVALIDATA.Index) in [1,3,4] then begin
//     raise EContaHandledError.Create(StrAcceptareValidat);
//     Exit;
//  end;
  {daca suma curenta este o plata}
  if ((aNode.Strings[GridRegistruINCASARI.Index]='') or (aNode.Values[GridRegistruINCASARI.Index]=0)) then begin
     raise EContaHandledError.Create(StrAcceptarePlati);
     Exit;
  end;
  {daca linia este deja un transfer acceptat}
  if not(Integer(aNode.Values[GridRegistruTRANSFER.Index]) in [2,6,10,{?}11]) then begin
     raise EContaHandledError.Create(StrAcceptareFail + #13#10+ ' Stare Curenta : '+ TransferState[Integer(aNode.Values[GridRegistruTRANSFER.Index])]);
     Exit;
  end;

  lTopIndex := GridRegistru.TopIndex;
  with TfrmAcceptTransfer.Create(Self) do
    try
      aTipCasa := nil;
      aTipCasa := TTipCasa.Create;
      {configuram valorile default}
      edtCasaPlecare.Text := GetHouseByIndex(aNode.Strings[GridRegistruCOD_CBT.Index]);
      edtCasaDest.Text    := GetHouseByIndex(IntToStr(FCurentHouse));
      edtSuma.Value       := aNode.Values[GridRegistruINCASARI.Index];
      if not TextToDateEx(aNode.Strings[GridRegistruDATA.Index], lDestDate) then
         lDestDate := Date;
      edtDataDest.Date  := lDestDate;
      btnReject.Enabled := (GetAsInteger(aNode, GridRegistruTRANSFER.Index)<9);
      GetTipCasa(FCurentHouse, aTipCasa);
      IsBanca := aTipCasa.IsBanca;
      ShowModal;
      case ModalResult of
         {daca se accepta}
         mrYes: begin
           if GetAsInteger(aNode, GridRegistruTRANSFER.Index) = 10 then begin
              aTransferStare := 11;
              aSwitch := -11;
           end
           else begin
              aTransferStare := Integer(IsBanca)*5 + Integer(not(IsBanca))*1;
              aSwitch := -1;
           end;
           MakeTransfer(aNode, GetAsInteger(aNode, GridRegistruCOD_CBT.Index), edtDataDest.Date, edtSuma.Value , aTransferStare, aSwitch, False, -1, 0);
           CalculateSold(aNode);
         end;
         {daca se rejecteaza}
         mrNo: begin
           aTransferStare := 9;
           MakeTransfer(aNode,  GetAsInteger(aNode, GridRegistruCOD_CBT.Index), edtDataDest.Date, edtSuma.Value , aTransferStare, -10, True, -1, 0);
         end;
      end;
    finally
      aTipCasa.Free;
      Free;
      GridRegistru.TopIndex := lTopIndex;
    end;

end;

procedure TFrmRegistru.AddToDeleted(aID: Integer; aTableName : String);
var aRec : PStersRec;
begin
   if RecSterse = nil then begin
     New(RecSterse);
     with RecSterse^ do begin
       ID := aID;
       TableName := aTableName;
       NextSters := nil;
     end;
   end
   else begin
     New(aRec);
     with aRec^ do begin
       ID := aID;
       TableName := aTableName;
       if RecSterse <> nil then NextSters := RecSterse;
     end;
     RecSterse := aRec;
   end;
end;

procedure TFrmRegistru.ProcessDeleted;
var CurentRec : PStersRec;
begin
   if RecSterse = nil then Exit;
   repeat
     CurentRec := RecSterse;
     MoveToArchive(CurentRec^.TableName, CurentRec^.Id, -2);
     RecSterse := CurentRec^.NextSters;
     Dispose(CurentRec);
   until RecSterse = nil;
end;

procedure TFrmRegistru.MemRegistruBeforeDelete(DataSet: TDataSet);
var
  lTableName: String;
begin
   if DataSet.FieldByName('ID_LISTA').AsString >'1.' then begin
    lTableName := 'BREGISTRU';
    if not DataSet.FieldByName('ID_PARINTE').IsNull then
       case CurentrbTag of
          0 :;
        1 : lTableName := 'BREG_X';
        2 : lTableName := 'BREG_P';
       end;
    AddToDeleted(DataSet.FieldByName('COD').AsInteger, lTableName);
     if CurentRbTag =4 then
     begin
        AddToDeleted(DataSet.FieldByName('COD').AsInteger, 'GEST_DEFALCARE_DECONTARI');
        AddToDeleted(DataSet.FieldByName('ID_BREG_P').AsInteger, 'BREG_P');
        AddToDeleted(DataSet.FieldByName('ID_GEST_DECONTARI').AsInteger, 'GEST_DECONTARI');
     end;
   end;
end;

function TFrmRegistru.PuneTransfer(aMemData: TdxMemData;
  aQry: TZQuery) : Integer;
const
  KnownFields : array [0..7] of String = ('COD_CB', 'DATA', 'PLATI', 'INCASARI', 'SOLD', 'COD_CBT', 'COD_TRANSFER', 'TRANSFER');

var I :Integer;
    Sold : Currency;
    Data : TDate;

  function SeAfla(aStr : String) : Boolean;
  var I:Integer;
  begin
    Result := False;
    for I:= 0 to High(KnownFields) do
      if aStr = KnownFields[I] then begin
        Result := True;
        Break;
      end;
  end;

begin
  Result := -1;
  for I := 0  to aQry.FieldCount -1 do
   {daca campul este ce de identity}
   if (aQry.Fields[I].FieldName = 'COD') then begin
      Result := GetLocalId;
      aQry.Fields[I].Value := Result;
   end
   else
     if SeAfla(aQry.Fields[I].FieldName)  then
        Continue
     else
        if aMemData.FindField(aQry.Fields[I].FieldName) <> nil then
            aQry.Fields[I].Value := aMemData.FieldByName(aQry.Fields[I].FieldName).AsVariant;
//        aQry.Fields[I].Value := aMemData.FieldByName(aQry.Fields[I].FieldName).AsVariant;

  {completam campurile pentru transfer}
  Sold := aMemData.FieldByName('INCASARI').AsCurrency - aMemData.FieldByName('PLATI').AsCurrency;
  if aMemData.FieldByName('COD_TRANSFER').AsInteger < 0 then
       Sold := (-1)* Sold;

  if Sold > 0 then begin
    aQry.FieldByName('PLATI').AsCurrency := Sold;
  end
  else begin
    aQry.FieldByName('INCASARI').AsCurrency := (-1) * Sold;
  end;

  Data := aMemData.FieldByName('DATA_ACCEPT').AsDateTime;
  if Data <= 0 then
     Data := aMemData.FieldByName('DATA').AsDateTime;


  aQry.FieldByName('DATA').AsDateTime := Data;
  aQry.FieldByName('COD_CB').AsInteger := aMemData.FieldByName('COD_CBT').AsInteger;
  aQry.FieldByName('COD_CBT').AsInteger := aMemData.FieldByName('COD_CB').AsInteger;
  aQry.FieldByName('TRANSFER').AsInteger := ABS(aMemData.FieldByName('COD_TRANSFER').AsInteger);
  aQry.FieldByName('NR_LIST').Value := Null;

  aQry.FieldByName('COD_TRANSFER').AsInteger := cst_TransferCode;
 // aQry.FieldByName('COD_TRANSFER').AsInteger := aMemData.FieldByName('COD').AsInteger;
end;

function TFrmRegistru.GetHouseByIndex(Index: String): String;
var aBookmark : TBookmark;
begin
  Result := '';
  RefreshStructure;
  with QryStructure do
    try
      aBookmark := GetBookmark;
      if Locate('COD_CB', Index,[]) then begin
        Result := '['+Trim(Index)+ '] '+ Trim(FieldByName('DENUMIRE').AsString);
      end;
      GotoBookmark(aBookmark);
      FreeBookmark(aBookmark);
    finally
    end;
end;


procedure TFrmRegistru.Cmd_ValidateExecute(Sender: TObject);
var aNode: TdxTreeListNode;
    I : Integer;
    lTopIndex : Integer;
    ValidValue : Integer;
begin
  {se verifica drepturile utilizatorului}
  if not IsRightEnable and not IsAdmin then Exit;


//  if (td_Administrator in FHouseRigths) or IsAdmin then ValidField := 'V_O'
//  else ValidField := 'V_O_1';

  if (td_Administrator in FHouseRigths) or IsAdmin then ValidValue := 4
  else ValidValue := 1;

  {se valideaza inregistrarea curenta}
  if not Assigned(GridRegistru.FocusedNode) then Exit;

  lTopIndex := GridRegistru.TopIndex;


  if GridRegistru.SelectedCount > 1 then begin
    //IdInitial := GridRegistru.SelectedNodes[0].Strings[GridRegistruID_LISTA.Index];
    for I := 0 to GridRegistru.SelectedCount -1 do begin
      if GetAsInteger(GridRegistru.SelectedNodes[I], GridRegistruEcl.Index) <> 1 then
          Raise EContaHandledError.Create(StrNonEclExist);
      if (ValidValue <> 4) and (GetAsInteger(GridRegistru.SelectedNodes[I], GridRegistruValidata.Index) in [3,4]) then
          Raise EContaHandledError.Create(StrValidByAdminS);
      end;

    with MemRegistru do
    try
       DisableControls;
       for I := 0 to GridRegistru.SelectedCount -1 do
          SetVALIDATAonRecord(GridRegistru.SelectedNodes[I].Values[GridRegistruID_LISTA.Index], ValidValue);
    finally
      EnableControls;
    end;
  end
  else begin
    aNode := GridRegistru.FocusedNode;
    if Assigned(aNode.Parent) then raise EContaHandledError.Create(StrValidareCopii);
    if GetAsInteger(aNode, GridRegistruECL.Index) <> 1 then Raise EContaHandledError.Create(StrNonEclExist);
    if (ValidValue <> 4) and (GetAsInteger(aNode, GridRegistruValidata.Index) in [3,4]) then
         Raise EContaHandledError.Create(StrValidByAdmin);

    SetVALIDATAonRecord(MemRegistru.FieldByName('ID_LISTA').AsString, ValidValue);
  end;

  GridRegistru.TopIndex := lTopIndex;
  //in caz de validare daca eram in edit ma lasa sa modific dupa validare
  DisableEditor(GridRegistru.FocusedNode);
end;

procedure TFrmRegistru.Cmd_UnValidateExecute(Sender: TObject);
var aNode : TdxTreeListNode;
    I : Integer;
    lTopIndex : Integer;
    aValidValue : Set of 0..6;
begin
  {se verifica drepturile utilizatorului}
  if not IsRightEnable and not IsAdmin then Exit;

//  if (td_Administrator in FHouseRigths) or IsAdmin then ValidField := 'V_O'
//  else ValidField := 'V_O_1';

  {se valideaza inregistrarea curenta}
  if not Assigned(GridRegistru.FocusedNode) then Exit;

  lTopIndex := GridRegistru.TopIndex;

  aValidValue := [0];
  if not (td_Administrator in FHouseRigths) then
    aValidValue := aValidValue + [3,4];

  if GridRegistru.SelectedCount > 1 then begin
    for I := 0 to GridRegistru.SelectedCount -1 do
      if GetAsInteger(GridRegistru.SelectedNodes[I], GridRegistruEcl.Index) <> 1 then
          Raise EContaHandledError.Create(StrNonEclExist);
      if not(td_Administrator in FHouseRigths) and (GetAsInteger(GridRegistru.SelectedNodes[I], GridRegistruValidata.Index) in [3,4]) then
          Raise EContaHandledError.Create(StrValidByAdminS);


    with MemRegistru do
    try
       DisableControls;
       for I := 0 to GridRegistru.SelectedCount -1 do
           if not(GetAsInteger(GridRegistru.SelectedNodes[I], GridRegistruVALIDATA.Index) in aValidValue) then
              SetVALIDATAonRecord(GridRegistru.SelectedNodes[I].Values[GridRegistruID_LISTA.Index], 0);
    finally
       EnableControls;
    end;
  end
  else begin
    aNode := GridRegistru.FocusedNode;
    if Assigned(aNode.Parent) then raise EContaHandledError.Create(StrValidareCopii);
    if GetAsInteger(aNode, GridRegistruECL.Index) <> 1 then  Raise EContaHandledError.Create(StrNonEclExist);
    if not(td_Administrator in FHouseRigths) and (GetAsInteger(aNode, GridRegistruValidata.Index) in [3,4]) then
          Raise EContaHandledError.Create(StrValidByAdmin);


     if not(GetAsInteger(GridRegistru.FocusedNode, GridRegistruVALIDATA.Index) in aValidValue) then
       SetVALIDATAonRecord(MemRegistru.FieldByName('ID_LISTA').AsString, 0);

  end;
  GridRegistru.TopIndex := lTopIndex;
  //in caz de validare daca eram in edit ma lasa sa modific dupa validare
  DisableEditor(GridRegistru.FocusedNode);

end;

procedure TFrmRegistru.LoadFromLocalComputer;
var aSaveRec : TSaveRec;
   LoadOk : Boolean;

procedure LoadDateFromFile(aFileName : String; var SaveRec: TSaveRec; var IsOk : Boolean);
var aFile : File of TSaveRec;

begin
  IsOk := True;
  AssignFile(aFile, aFileName);
  {$I-}
    Reset(aFile);
  {$I+}
  if IOResult=0 then begin
     Read(aFile, SaveRec);
  end
  {pe else = scuze s-au pierdut datele!}
  else IsOk := False;
  CloseFile(aFile);
end;

begin
  {trebuie sa incarcam datele si evenimentele din momentul caderii}
  LoadDateFromFile(Format(SaveFormat, [CurentSaveDir, SaveFileDate]), aSaveRec, LoadOk);
  {setam situatia caderii}
  with aSaveRec do  begin
    FSignLogin := SIdLogin;
    CommonDbvar.IdUtilizator := SIdUtilizator;
    FCurentHouse := SCurrentHouse;
    FStartInterval := SDataStart;
    FEndInterval := SDataEnd;
    FIsAvans := SIsAvans;
    FCodDecont := SCodDecont;
    FSoldInitial := SSoldInitial;
  end;
  {incarcam }
  AssignOnChange(False);
  if MemReg.Active then  MemReg.Active := False;
  if MemCont.Active then MemCont.Active := False;
  if MemProj.Active then MemProj.Active := False;
(*CHRIS SALVARE FACTURI
  if MemFact.Active then MemFact.Active := False;
*)

  MemReg.Active := True;
  MemCont.Active := True;
  MemProj.Active := True;
(*CHRIS SALVARE FACTURI
  MemFact.Active := True;
*)

  MemReg.LoadFromBinaryFile(Format(SaveFormat, [CurentSaveDir, SaveFileReg]));
  MemCont.LoadFromBinaryFile(Format(SaveFormat, [CurentSaveDir, SaveFileCont]));
  MemProj.LoadFromBinaryFile(Format(SaveFormat, [CurentSaveDir, SaveFileProj]));
(*CHRIS SALVARE FACTURI
  MemFact.LoadFromBinaryFile(Format(SaveFormat, [CurentSaveDir, SaveFileFact]));
*)

  edData.OnDateChange := nil;
  edCurentHouse.OnChange:= nil;
  chkEfectiv.OnClick := nil;

  edData.Date := FStartInterval;
  SetCurentHouse(FCurentHouse);
  chkEfectiv.Checked := aSaveRec.SEstePeZi;

  edData.OnDateChange := edDataDateChange;
  chkEfectiv.OnClick := chkEfectivClick;

  IsSyncro := True;
  case aSaveRec.SRbCurent of
      0 : rbFara.Checked := True;
      1 : rbCont.Checked := True;
      2 : rbProiect.Checked := True;
  end;
  AssignOnChange(True);
  MoveSavesToArchive(FSignLogin);
  FIsModified := True;
end;

procedure TFrmRegistru.SaveToLocalComputer;
var
     aSaveRec : TSaveRec;

  function GetFileName(const AFileName: String): String;
  begin
    Result := Format(SaveFormat, [CurentSaveDir, AFileName]);
  end;

  procedure SaveDateToFile(aFileName : String; SaveRec : TSaveRec);
  var
    aSaveFile : file of TSaveRec;
begin
  AssignFile(aSaveFile, aFileName);
  {$I-}
  Rewrite(aSaveFile);
  {$I+}
  if IOResult = 0 then begin
    Write(aSaveFile, SaveRec);
    CloseFile(aSaveFile);
  end
  else
      raise Exception.CreateFmt('Eroare la salvarea datelor in fisier : %s'#13#10'Eroare : %s', [AFileName, SysErrorMessage(GetLastError)]);
  end;

  procedure CheckFileName(const aFileName: String);
  begin
    if FileExists(aFileName) and not DeleteFile(aFileName) then
      raise Exception.CreateFmt('Eroare la stergerea fisierului : %s'#13#10'Eroare : %s', [AFileName, SysErrorMessage(GetLastError)]);
end;


begin
  {procedura trebuie sa salveze parametrii luati de pe server}

  with aSaveRec do begin
     SIdLogin       := FSignLogin;
     SIdUtilizator  := CommonDbvar.IdUtilizator;
     SCurrentHouse  := FCurentHouse;
     SDataStart     := FStartInterval;
     SDataEnd       := FEndInterval;
     SEstePeZi      := chkEfectiv.Checked;
     SIsAvans       := FIsAvans;
     SCodDecont     := FCodDecont;
     SRbCurent      := CurentrbTag;
     SSoldInitial   := FSoldInitial;
  end;

  if not DirectoryExists(CurentSaveDir) then
    if not ForceDirectories(CurentSaveDir) then
      raise Exception.CreateFmt('Eroare la initializarea directorului : %s'#13#10'Eroare : %s', [CurentSaveDir, SysErrorMessage(GetLastError)]);

  SaveDateToFile(GetFileName(SaveFileDate), aSaveRec);
  CheckFileName(GetFileName(SaveFileReg));
  CheckFileName(GetFileName(SaveFileCont));
  CheckFileName(GetFileName(SaveFileProj));

  MemReg.SaveToBinaryFile (GetFileName(SaveFileReg));
  MemCont.SaveToBinaryFile(GetFileName(SaveFileCont));
  MemProj.SaveToBinaryFile(GetFileName(SaveFileProj));

end;

procedure TFrmRegistru.Cmd_AnuleazaTransferExecute(Sender: TObject);
var
   OldState : Boolean;
   Transfer : Integer;
   aNode : TdxTreeListNode;
   lId : Integer;
begin
  if not Assigned(GridRegistru.FocusedNode) then Exit;
  aNode := GridRegistru.FocusedNode;
  {daca suma esta validata atunci eroare}
  if GetAsInteger(ANode, GridRegistruVALIDATA.Index) in [1,3,4] then begin
     raise EContaHandledError.Create(StrTranferValidat);
     Exit;
  end;

  lId := MemRegistru.FieldByName('COD_TRANSFER').AsInteger;
  OldState := MemRegistru.State in [dsEdit, dsInsert];
  if not OldState then MemRegistru.Edit;
  Transfer := MemRegistru.FieldByName('TRANSFER').AsInteger;
  MemRegistru.FieldByName('TRANSFER').AsInteger := 0;
  MemRegistru.FieldByName('COD_TRANSFER').AsInteger := 0;
  MemRegistru.Post;
  if OldState then MemRegistru.Edit;
  if not (Transfer in [3,7,9,12,13]) then
    if MessageDlg(StrCancelTransfer, mtConfirmation, [mbYes, mbNo], 0) = mrYes then begin
       AddToDeleted(lId , 'BREGISTRU');
    end;
end;

procedure TFrmRegistru.chkEfectivClick(Sender: TObject);
begin
  edDataDateChange(nil);
end;

procedure TFrmRegistru.GridRegistruCODGESTPopup(Sender: TObject;
  const EditText: String);
var lSearchC : TdxDBTreeListColumn;
begin
  if frmdata.QryRepartitori.Filtered then begin
    frmdata.QryRepartitori.Filter := '';
    frmdata.QryRepartitori.Filtered := False;
  end;
  lSearchC := FindColumnByTag(frmCasaContainer.TreeRepartitori, -1);
  if (IsNumeric(EditText)) or (lSearchC = nil) then
      InternalPositioning(StringReplace(EditText,'?', '',[]), frmCasaContainer.TreeRepartitori)
  else
    InternalPositioning(StringReplace(EditText,'?', '',[]),  frmCasaContainer.TreeRepartitori, lSearchC.FieldName);
end;

procedure TFrmRegistru.GridRegistruCONT_CSPPopup(Sender: TObject;
  const EditText: String);
begin
  InternalPositioning(EditText, frmCasaContainer.TreePlan);
end;

procedure TFrmRegistru.GridRegistruTIPDOCPopup(Sender: TObject;
  const EditText: String);
begin
  InternalPositioning(EditText, frmCasaContainer.TreeTipDoc);
end;

procedure TFrmRegistru.GridRegistruPROJPopup(Sender: TObject;
  const EditText: String);
var lSearchC : TdxDBTreeListColumn;
    //lValue : Variant;
begin
  lSearchC := FindColumnByTag(TdxDBTreeList(TdxDBTreeListPopupColumn(Sender).PopupControl), -1);
  if lSearchC <> nil then
    InternalPositioning(StringReplace(EditText,'?', '',[]),  TdxDBTreeList(TdxDBTreeListPopupColumn(Sender).PopupControl), lSearchC.FieldName)
  else
    InternalPositioning(StringReplace(EditText,'?', '',[]), TdxDBTreeList(TdxDBTreeListPopupColumn(Sender).PopupControl));
end;


procedure TFrmRegistru.CheckForSave;
begin
  if Self.FIsModified then
    if Force then SaveToDB
    else
        case
             MessageDlg(StrWantToSave, mtInformation, [mbYes, mbNo, mbCancel], 0) of
               mrYes : SaveToDb;
               mrCancel : Abort;
        end;
  Self.FIsModified := False;
  RecSterse := nil;
  RecValidate := nil;
  DeleteShare;
  SetMainFilter('', False);
  MemRegistru.Active := False;
  MemReg.Active := False;
  MemProj.Active := False;
  MemCont.Active := False;
(*CHRIS SALVARE FACTURI
  MemFact.Active := False;
*)
end;

procedure TFrmRegistru.GridRegistruColumnSorting(Sender: TObject;
  Column: TdxTreeListColumn; var Allow: Boolean);
begin
   Allow := (Column = GridRegistruSORTFIELD);
end;

procedure TFrmRegistru.GridRegistruCODGESTGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
var lNode: TdxDBTreeListNode;
begin
  if (AText <> '') and (IsNumeric(AText)) then begin
     lNode := frmCasaContainer.TreeRepartitori.FindNodeByKeyValue(AText);
     if Assigned(lNode) then begin
        AText := Trim(lNode.Strings[frmCasaContainer.TreeRepartitoriNUME.Index]);
        if Trim(lNode.Strings[frmCasaContainer.TreeRepartitoriCODSECTIE.Index]) <> '' then
          AText := Trim(lNode.Strings[frmCasaContainer.TreeRepartitoriCODSECTIE.Index])+' : '+ AText ;
     end;
  end;
end;

procedure TFrmRegistru.Cmd_GenereazaDiferentaExecute(Sender: TObject);
var aNode : TdxTreeListNode;
    OldEdit : Boolean;
    aSum, aSignSum, aParentSum : Currency;
    I : Integer;
    LineInfo : TLineNodeInfo;
begin
  {procedura va genera o noua inregistrare plata sau incasare care se va transfera}
  if (FCodDecont<0) or not Assigned(GridRegistru.FocusedNode) then Exit;
  aNode := GridRegistru.FocusedNode;
  if aNode.HasChildren then
       raise EContaHandledError.Create(StrJustHasChildren);

  if Assigned(aNode.Parent) then
      raise EContaHandledError.Create(StrJustIsChild);

  if aNode.Values[GridRegistruECL.Index] <> 1 then
      raise EContaHandledError.Create(StrNotEchilbrate);


   aSignSum := 0;
   for I := 0 to GridRegistru.Count -1 do
      aSignSum := aSignSum + GetRealLineValue(GridRegistru.Items[I]);

   aParentSum := edtSumaDecont.Value;
   aSum := aParentSum + aSignSum;


   //de facut si pe sume negative
   if aSum > 0 then begin
     {generam o noua inregistare}

     GetLineInfo(TdxDbTreeListNode(aNode), LineInfo);
     AdaugaMemRegistruNew(LineInfo);
     //AdaugaMemRegistru(TdxDbTreeListNode(aNode));
     with MemRegistru do begin
       OldEdit := State in [dsEdit, dsInsert];
       if not OldEdit then Edit;
//       FieldByName('EXPLICATIE').AsString := 'Nota diferenta pt casa ' + GetHouseByIndex(IntToStr(CodCasaDecont));
       FieldByName('EXPLICATIE').AsString := Format(StrShortNotaDiferenta,
          [
            GetHouseByIndex(IntToStr(CodCasaDecont)), //casa
            edtNrDecont.Text,                         //nr
            edtDataDecont.Text,                       //data
            edtDetaliiDecont.Text                     //persoana
          ]);
       FieldByName('MEXPLIC').AsString := Format(StrNotaDiferenta,
                [GetHouseByIndex(IntToStr(CodCasaDecont)),
                 FormatFloat(CurrFormat, aParentSum),
                 PersoanaDecont,
                 GetHouseByIndex(IntToStr(FCurentHouse)),
                 FormatFloat(CurrFormat,aParentSum - aSum),
                 FormatFloat(CurrFormat,aSum)]);
       FieldByName('CONT_CSP').AsString := GetHouseContByIndex(IntToStr(CodCasaDecont));
       if FIsAvans then begin
          if FCodDecont > 0 then
             FieldByName('PARENT_COD').AsInteger := FCodDecont;
          FieldByName('NR_DECONT').AsInteger := Round(edtNrDecont.Value);
          FieldByName('DATA_DECONT').AsDateTime := edtDataDecont.Date;
       end;
       if aSum > 0 then FieldByName('PLATI').AsCurrency := aSum;
       Post;
       if OldEdit then Edit;
     end;
   end;
end;

procedure TFrmRegistru.GridRegistruPROJGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
var lNode: TdxDBTreeListNode;
begin
  if (AText= '') and (ANode.Strings[GridRegistruID_PROIECT.Index] <> '' ) then
  //if (AText <> '') and (AText <> '%') and (AText <> '?') and (IsNumeric(StringReplace(AText,'.', '|', [rfReplaceAll]))) then
  begin
     lNode := frmCasaContainer.TreeFunctional.FindNodeByKeyValue(ANode.Strings[GridRegistruID_PROIECT.Index]);
     if Assigned(lNode) then AText := Trim(lNode.Strings[frmCasaContainer.TreeFunctionalCOD_BUGET.Index])+ ' '+
     Trim(lNode.Strings[frmCasaContainer.TreeFunctionalDENUMIRE.Index]);
  end;
end;

function TFrmRegistru.GetNextPosition(aData: TDateTime; ParentID : String): Integer;
var aNode : TdxDBTreeListNode;

  function FindFirstNode(aTreeList: TdxDBTreeList; aColumnIndex: Integer; aValue: Variant): TdxDBTreeListNode;
  var
    lNode: TdxTreeListNode;
  begin
    lNode := aTreeList.Items[0];
    while Assigned(lNode) do
    begin
      if lNode.Values[aColumnIndex] = aValue then
        Break;

      lNode := lNode.GetNext;
    end;

    Result := TdxDBTreeListNode(lNode);
  end;

  function Max(a,b : Integer) : Integer;
  begin
    Result := a;
    if b>a then Result := b;
  end;

begin
  Result := 0;
  aNode := nil;

    if ParentID <> '' then begin
    aNode := GridRegistru.FindNodeByKeyValue(ParentID);
    if aNode <> nil then begin
      aNode := TdxDBTreeListNode(aNode.GetFirstChild);
      if aNode <> nil then
        repeat
          Result := Max(Result, GetAsInteger(aNode , GridRegistruPOZ.Index));
          aNode := TdxDBTreeListNode(aNode.GetNextSibling);
        until (aNode = nil);
    end;
  end
  else begin
    aNode := FindFirstNode(GridRegistru, GridRegistruDATA.Index, aData);
    if Assigned(aNode) then
     repeat
       Result := Max(Result, GetAsInteger(aNode , GridRegistruPOZ.Index));
       aNode := TdxDBTreeListNode(aNode.GetNext);
    until (aNode = nil) or not(aNode.Values[GridRegistruDATA.Index] = aData);
  end;

  Result := Result + 1;
end;

procedure TFrmRegistru.TreeDecontariKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if ((Key in [VK_UP, VK_DOWN]) and (ssAlt in Shift)) or
      ((Key = VK_F4) and not (ssAlt in Shift)) or (Key = VK_ESCAPE) then
    GetParentForm(TControl(Sender)).ModalResult := mrCancel;
  if (Key = VK_RETURN) and (TdxDBTreeList(Sender).FocusedNode <> nil)
     and (not TdxDBTreeList(Sender).FocusedNode.HasChildren) then
    GetParentForm(TControl(Sender)).ModalResult := mrOk;
end;

procedure TFrmRegistru.TreeDecontariNUMEGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: string);
var
  lProcent: Variant;
begin
  lProcent := ANode.Values[TreeDecontariPROCENT.ItemIndex];
  if ValueHasValue(lProcent) then
    Value := Value + Format(' (%2d.%2d)', [Integer(lProcent * 100) div 100, Integer(lProcent * 100) mod 100]);
end;

procedure TFrmRegistru.TreeDecontariDblClick(Sender: TObject);
begin
  GetParentForm(TControl(Sender)).ModalResult := mrOk;
end;

procedure TFrmRegistru.SetCodDecont(const Value: Integer);
begin
  FCodDecont := Value;
  if FCodDecont > 0 then RefreshDataSet;
end;

procedure TFrmRegistru.Cmd_JustificareAvansExecute(Sender: TObject);
var
  aNode : TdxTreeListNode;
  lSrcValuta,
  lDstValuta,
  aTransferStare,
  aDestStare  : Integer;
  IsBanca,
  DestBanca   : Boolean;
  aTipCasa    : TTipCasa;

  lCursSrc,
  lCursDst,
  lSumaTransferata : Currency;

    lCodGest, lNrDecont, lCasa : Integer;
    lDataDecont, lData, lDestDate : TDateTime;

  lContCrsp : String;
begin

  if not Assigned(GridRegistru.FocusedNode) then
    raise Exception.Create('Selectati pozitia la care doriti sa introduceti justificarea !');

  if Assigned(GridRegistru.FocusedNode) then begin
    aNode := GridRegistru.FocusedNode;

    {daca linia are copii}
    {
    if (aNode.HasChildren) then begin
       raise EContaHandledError.Create(StrTransferCopii);
       Exit;
    end;
    }
    {daca suma esta validata atunci eroare}
    if GetAsInteger(ANode, GridRegistruVALIDATA.Index) in [1,3,4] then begin
       raise EContaHandledError.Create(StrTranferValidat);
       Exit;
    end;

    (*
    {daca suma curenta este o incasare}
    if (aNode.Strings[GridRegistruPLATI.Index]='') or (aNode.Values[GridRegistruPLATI.Index]=0) then begin
       raise EContaHandledError.Create(StrTranferIncasare);
       Exit;
    end;
    *)

    {daca linia este deja un transfer}
    if ( aNode.Strings[GridRegistruTRANSFER.Index]<>'') and (aNode.Strings[GridRegistruTRANSFER.Index]<>'0') then begin
       raise EContaHandledError.Create(StrTransferTransfer);
       Exit;
    end;

    if Trim(aNode.Strings[GridRegistruCODGEST.Index]) = '' then
      if MessageDlg(StrNoRepartitorOnDecont, mtConfirmation, [mbYes, mbNo], 0) = mrNo then Exit;

      lContCrsp := Trim(aNode.Strings[GridRegistruCONT_CSP.Index]);
      if lContCrsp = '' then
        raise Exception.Create('Va rugam completati contul corespondent !');
    end;

  with TfrmTransfer.Create(self) do
    try
       aTipCasa := nil;
       aTipCasa := TTipCasa.Create;
       GetTipCasa(FCurentHouse, aTipCasa);
       IsBanca := aTipCasa.IsBanca;
       lSrcValuta := aTipCasa.TipValuta;

       FTreeList := TreeStructura;
       SetTreeList;
       DefaultIndex := FCurentHouse;
       TipDestinatie := [tt_CasaDecont , tt_BancaDecont];
       // Dupa setarea Tree si TipDestinatie
       ContCrsp       := lContCrsp;

       Deconturi.Assign(DeconturiList);
       Deconturi.Sort;
       CodGest := MemRegistru.FieldByName('CODGEST').AsString;

       edtCasa.Text := edCurentHouse.Text;
       if Assigned(aNode) then begin
       edtSuma.Value := GetRealLineValue(aNode);
       if not TextToDateEx(aNode.Strings[GridRegistruDATA.Index], lDestDate) then
         lDestDate := Date;
       end
       else begin
        edtSuma.Value := 0;
        lDestDate     := Date;
       end;

       edtDataDest.Date := lDestDate;
       edtDataPlec.Date := lDestDate;
       ledDataDec.Date := lDestDate;
       edtDataPlec.Text := edtDataDest.Text;
      edtDataDest.Enabled := False;
      edtDataPlec.Enabled := False;


       ShowModal;
       {daca este casa sau banca transferul difera}

       aTransferStare := Integer(not(IsBanca))*3 + Integer(IsBanca)*7;
       {daca se accepta transferul}
       if ModalResult = mrOk then begin
         DestroyTreeList;
         GetTipCasa(HouseIndex, aTipCasa);
         DestBanca := aTipCasa.IsBanca;
         lDstValuta := aTipCasa.TipValuta;

         if chkConfirm.Checked then
             aDestStare := Integer(not(DestBanca))*2 + Integer(DestBanca)*6
         else
             aDestStare := Integer(not(DestBanca))*1 + Integer(DestBanca)*5;

         lSumaTransferata := edtSuma.Value;
         if lSrcValuta <> lDstValuta then begin
          lCursSrc  := GetCursValutar(lSrcValuta, edtDataDest.Date);
          lDestDate := GetCursValutar(lDstValuta, edtDataDest.Date);
          if lCursSrc <> lCursDst then
            if (lCursDst <> 0) and (lCursSrc <> 0) then
              lSumaTransferata := (lSumaTransferata * lCursSrc) / lCursDst
         end;

         //if (-1)*GetRealLineValue(aNode) < 0 then  aDestStare := (-1)* aDestStare;
         MakeTransfer(aNode, HouseIndex, edtDataDest.Date, lSumaTransferata, aTransferStare, aDestStare ,not(chkConfirm.Checked), Round(edtNrDec.Value), ledDataDec.Date);

         if rb_DispozitieId <> - 1 then begin
           aNode := GridRegistru.FocusedNode;

           if Assigned(aNode) then begin
             lDataDecont := 0;
             lNrDecont := -1;
             lCodGest := 0;
             lCasa := FCurentHouse;
             lData := edData.Date;
             if Trim(aNode.Strings[GridRegistruDATA_DECONT.Index]) <> '' then
               lDataDecont := aNode.Values[GridRegistruDATA_DECONT.Index];
             if Trim(aNode.Strings[GridRegistruCODGEST.Index]) <> '' then
               lCodGest := MemRegistru.FieldByName('CODGEST').AsInteger;
             if Trim(aNode.Strings[GridRegistruNR_DECONT.Index]) <> '' then
               lNrDecont := aNode.Values[GridRegistruNR_DECONT.Index];
             if Trim(aNode.Strings[GridRegistruDATA.Index]) <> '' then
               lData := TDateTime(aNode.Values[GridRegistruDATA.Index]);

             if (lNrdecont <> -1) and (lDataDecont <> 0) and (MessageDlg(StrAskForDispozitie, mtConfirmation, [mbYes, mbNo], 0) = mrYes) then begin
               if Self.IsModified then begin
                  SaveToDb;
                  Self.FIsModified := False;
                  RecSterse := nil;
                  RecValidate := nil;
                  DeleteShare;
                  SetMainFilter('', False);
                  MemRegistru.Active := False;
                  MemReg.Active := False;
                  MemProj.Active := False;
                  MemCont.Active := False;
(*CHRIS SALVARE FACTURI
                  MemFact.Active := False;
*)
               end;

               edData.OnDateChange := nil;
               edData.Date := lData;
               edData.OnDateChange := edDataDateChange;
               SetCurentHouse(lCasa);
               if MemRegistru.Locate('DATA_DECONT;NR_DECONT;CODGEST', VarArrayOf([lDataDecont, lNrDecont, LCodGest]), []) then begin
                 PrintDispozitie(MemRegistru.FieldByName('COD').AsInteger);
                 //RegistrerCodCasa(MemRegistru.FieldByName('COD').AsInteger);
               end;
             end;

           end;

         end;
       end;
    finally
      aTipCasa.Free;
      Free;
    end;
end;


procedure TFrmRegistru.btnFindDecontClick(Sender: TObject);
begin
  edtDetaliiDecont.DroppedDown  := True;
  TreeDecontariCHEIE.Focused    := True;
  TreeDecontari.SearchingText   := Format('%d|%s', [Integer(edtNrDecont.EditValue), FormatDateTime('dd''/''mm''/''yyyy', edtDataDecont.Date)]);
end;

procedure TFrmRegistru.btnDelJustClick(Sender: TObject);
begin
  if FCodDecont>0 then
    {le mutam in arhiva ...}
    if MessageDlg( Format(StrQuestionOnDeleteDecont,
                   [IntToStr(Round(edtNrDecont.Value)),
                    FormatDateTime('dd/mm/yyyy', edtDataDecont.Date),
                    PersoanaDecont
                    ]), mtConfirmation, [mbYes, mbNo], 0) = mrYes then begin
       AddToDeleted(FCodDecont, 'BREGISTRU');
       FIsModified := True;
       SaveToDb;
       MemRegistru.Active := False;
       MemReg.Active := False;
       MemCont.Active := False;
       MemProj.Active := False;
(*CHRIS SALVARE FACTURI
       MemFact.Active := False;
*)
       RefreshQueryDeconturi;
       UpdateSoldCasa;
    end;
end;

{
procedure TFrmRegistru.ProgressADODataset(DataSet: TDataSet;
  Progress, MaxProgress: Integer; var EventStatus: TEventStatus);
begin
  ProgressDataset(DataSet, Progress, MaxProgress);
end;
}

procedure TFrmRegistru.BitBtn1Click(Sender: TObject);
begin
  LoadFromLocalComputer;

end;

procedure TFrmRegistru.SetHasDied(const Value: Boolean);
begin
  FHasDied := Value;
  {daca moare conexiunea sa nu se mai faca nici o schimbare}
  if FHasDied then begin
    gbInformation.Enabled := False;
    pnDecont.Enabled := False;
  end;
end;


procedure TFrmRegistru.MoveSavesToArchive(Id : Integer);
var Term : String;
begin
  Term := IntToStr(Id);
  if not DirectoryExists(SlashSep(CurentSaveDir, ArhivaDir)) then
    if not CreateDir(SlashSep(CurentSaveDir, ArhivaDir)) then
       raise EContaHandledError.Create('Cannot create '+ SlashSep(CurentSaveDir, ArhivaDir));

  if FileExists(Format(SaveFormat, [CurentSaveDir, SaveFileDate])) then
     MoveFileEx(PChar(Format(SaveFormat, [CurentSaveDir, SaveFileDate])), PChar(Format(SaveFormat, [SlashSep(CurentSaveDir, ArhivaDir), SaveFileDate + Term])), MOVEFILE_REPLACE_EXISTING);

  if FileExists(Format(SaveFormat, [CurentSaveDir, SaveFileReg])) then
     MoveFileEx(PChar(Format(SaveFormat, [CurentSaveDir, SaveFileReg])), PChar(Format(SaveFormat, [SlashSep(CurentSaveDir, ArhivaDir), SaveFileReg + Term])),MOVEFILE_REPLACE_EXISTING);

  if FileExists(Format(SaveFormat, [CurentSaveDir, SaveFileCont])) then
     MoveFileEx(PChar(Format(SaveFormat, [CurentSaveDir, SaveFileCont])), PChar(Format(SaveFormat, [SlashSep(CurentSaveDir, ArhivaDir), SaveFileCont + Term])),MOVEFILE_REPLACE_EXISTING);

  if FileExists(Format(SaveFormat, [CurentSaveDir, SaveFileProj])) then
     MoveFileEx(PChar(Format(SaveFormat, [CurentSaveDir, SaveFileProj])),
       PChar(Format(SaveFormat, [SlashSep(CurentSaveDir, ArhivaDir), SaveFileProj + Term])), MOVEFILE_REPLACE_EXISTING);
end;

function TFrmRegistru.CheckForFiles: Boolean;
begin
  Result := False;
  if FileExists(Format(SaveFormat, [CurentSaveDir, SaveFileDate]))
     and FileExists(Format(SaveFormat, [CurentSaveDir, SaveFileReg]))
     and FileExists(Format(SaveFormat, [CurentSaveDir, SaveFileCont]))
     and FileExists(Format(SaveFormat, [CurentSaveDir, SaveFileProj]))
  then Result := True;
end;



procedure TFrmRegistru.GridRegistruTIP_CHELTVENGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
var lNode: TdxDBTreeListNode;
begin
  if (AText= '') and (ANode.Strings[GridRegistruID_TIPURI_CHELTVEN.Index] <> '' ) then
//  if (AText <> '') and (IsNumeric(StringReplace(AText,'.', '|', [rfReplaceAll]))) then
  begin
     lNode := frmCasaContainer.TreeEconomic.FindNodeByKeyValue(ANode.Strings[GridRegistruID_TIPURI_CHELTVEN.Index]);
     if Assigned(lNode) then AText := Trim(lNode.Strings[frmCasaContainer.TreeEconomicCOD_BUGET.Index])+' '+Trim(lNode.Strings[frmCasaContainer.TreeEconomicDENUMIRE.Index]);
  end;
end;

procedure TFrmRegistru.GridRegistruORGANIGRAMAGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
var lNode: TdxDBTreeListNode;
begin
  if AText <> '' then begin
     lNode := frmCasaContainer.TreeOrganigrama.FindNodeByKeyValue(AText);
     if Assigned(lNode) then AText := Trim(lNode.Strings[frmCasaContainer.TreeOrganigramaDENUMIRE.Index]);
  end;
end;

procedure TFrmRegistru.GridRegistruRESURSAGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
var lNode: TdxDBTreeListNode;
begin
  if AText <> '' then begin
     lNode := frmCasaContainer.TreeCheltituitori.FindNodeByKeyValue(AText);
     if Assigned(lNode) then AText := Trim(lNode.Strings[frmCasaContainer.TreeCheltituitoriNUME.Index]);
  end;
end;

procedure TFrmRegistru.AdaugaCopiiProjCamp(aList: TStringList;
  aFieldName: String; const lNode : TdxDBTreeListNode);
var
    I:Integer;
    aNewId : Integer;
    aSum : Currency;
    LineInfo : TLineNodeInfo;
    aSearchId : String;
    aBook : TBookMark;
    aContcsp : string;
begin
  {procedura adauga dupa ce s-au selectat proiectele de distributie si adauga copii pentru nodul curent selectat}
  {folosim lnode ca baza pentru copii ce vor urma}
  if not Assigned(lNode) then Exit;
  try
    GridRegistru.BeginUpdate;
    IsInLoad := True;
    aSum := GetUnitedValue(lNode);
    aContCSP := '';
    if not VarIsNull(lNode.Values[GridRegistruCONT_CSP.Index]) then
       aContCsp := VarToStr(lNode.Values[GridRegistruCONT_CSP.Index]);
    GetLineInfo(lNode, LineInfo);
    aSearchId := VarToStr(lNode.Id);
    if Assigned(lNode.Parent) then aSearchId := VarToStr(lNode.ParentId);
    try
      aBook := MemRegistru.GetBookmark;
      if MemRegistru.Locate('ID_LISTA', aSearchId, []) then begin
        if not(MemRegistru.State in [dsEdit, dsInsert]) then MemRegistru.Edit;
        MemRegistru.FieldByName('ON_SERVER').AsInteger := 0;
        MemRegistru.Post;
        FIsModified := True;
      end;
    finally
      MemRegistru.GotoBookmark(aBook);
      MemRegistru.FreeBookmark(aBook);
    end;

    for I:= 0 to aList.Count-1 do begin
      {adaugam in ecran inregistariile cu id_proiect selectate}
      aNewId :=  StrToInt(aList.Strings[I]);
      if (I <> 0) or not Assigned(lNode.Parent) then
         AdaugaMemRegistruNew(LineInfo, True);
         //AdaugaMemRegistru(lNode, True);
      with MemRegistru do begin
         if not(State in [dsEdit, dsInsert]) then Edit;
         FieldByName(GetFieldIndex(lNode)).AsCurrency := RoundTo(aSum/aList.Count, -1*CurrDecimal);
         if I=aList.Count-1 then
           FieldByName(GetFieldIndex(lNode)).AsCurrency := FieldByName(GetFieldIndex(lNode)).AsCurrency + aSum- aList.Count*(RoundTo(aSum/aList.Count, -1*CurrDecimal));
         if (I<> 0) or not Assigned(lNode.Parent)then begin
              FieldByName('ID_PROIECT').AsInteger := GetAsInteger(lNode,GridRegistruID_PROIECT.Index);
              FieldByName('ID_TIPURI_CHELTVEN').AsInteger := GetAsInteger(lNode, GridRegistruID_TIPURI_CHELTVEN.Index);
              FieldByName('ID_ORGANIGRAMA').AsInteger := GetAsInteger(lNode,GridRegistruID_ORGANIGRAMA.Index);
              FieldByName('ID_RESURSA').AsInteger := GetAsInteger(lNode, GridRegistruID_RESURSA.Index);
         end;
         if (aContCSP <> '%') and (aContCSP <> '') then
             FieldByName('CONT_CSP').Value := aContCSP;
         FieldByName(aFieldName).AsInteger := aNewId;
      end;
    end;
  finally
    IsInLoad := False;
    GridRegistru.EndUpdate;
  end;
end;

procedure TFrmRegistru.SetFiltered(const Value: String);
begin
  FFilter := Value;
  lblFilter.Caption := FFilter;
  if Trim(FFilter) <> '' then SetMainFilter(FFilter, True);
end;

procedure TFrmRegistru.SetMainFilter(Filter: String; State : Boolean);
begin
  FEvaluator.Active := False;
  FEvaluator.ClearFields;
  FFormula := nil;
  if ((not Assigned(MemRegistru.OnFilterRecord)) and (not State)) or (Trim(FFilter) = '')  then Exit;
  if (State) and (MemRegistru.Active) then begin
    chkFilter.Checked := True;
    FEvaluator.Active := False;
    FEvaluator.AddDataSet(MemRegistru);
    FFormula := FEvaluator.AddFormula(nil, '', Filter);
    FEvaluator.Active := True;
    MemRegistru.OnFilterRecord := FilterMain;
    MemRegistru.Filtered := True;
    CalculateTotalFix(MemRegistru, FiltratIncasari, FiltratPlati);

    SummStatus.Panels[0].Text := Format('%s/%s', [IntToStr(TotalCount), IntToStr(GridRegistru.Count)]);
    SummStatus.Panels[7].Text := FormatFloat(CurrFormat, FiltratIncasari);
    SummStatus.Panels[9].Text := FormatFloat(CurrFormat, FiltratPlati);

  end
  else begin
    MemRegistru.Filtered := False;
    MemRegistru.OnFilterRecord := nil;
  end;
end;

procedure TFrmRegistru.FilterMain(DataSet: TDataSet; var Accept: Boolean);
var
  lValue : Variant;
begin
  if Assigned(FFormula) then begin
    lValue := FFormula.Calculate;
    Accept := not ValueIsFalse(lValue);
  end;
end;

procedure TFrmRegistru.Cmd_RenumeroteazaExecute(Sender: TObject);
var lNode : TdxTreeListNode;
begin
  if not Assigned(GridRegistru.FocusedNode) then Exit;
  lNode := GridRegistru.FocusedNode;
  GridRegistru.BeginUpdate;
  MemRegistru.DisableControls;
  try
    RenumeroteazaNode(lNode);
  finally
      MemRegistru.EnableControls;
      GridRegistru.EndUpdate;
  end;
end;

procedure TFrmRegistru.edtCodGestPropertiesCloseUp(Sender: TObject);
var
  lNode: TdxDBTreeListNode;
begin
  lNode := TdxDBTreeListNode(frmCasaContainer.TreeRepartitori.FocusedNode);
  if GetParentForm(edtCodGest.Properties.PopupControl).ModalResult = mrOk then begin
    edtCodGest.Tag      := Integer(lNode.Id);
    edtCodGest.EditText := lNode.Strings[frmCasaContainer.TreeRepartitoriNUME.Index];
  end;
end;

procedure TFrmRegistru.edtDetaliiDecontInitPopup(Sender: TObject);
begin
  if edtDetaliiDecont.Properties.PopupWidth < edtDetaliiDecont.Width then
    edtDetaliiDecont.Properties.PopupWidth := edtDetaliiDecont.Width;
end;

procedure TFrmRegistru.edtDetaliiDecontPropertiesCloseQuery(Sender: TObject;
  var CanClose: Boolean);
var
  lNode : TcxTreeListNode;
begin
  CheckForSave;
  edtDetaliiDecont.Text := '';

  if GetParentForm(edtDetaliiDecont.Properties.PopupControl).ModalResult = mrOk then begin
    lNode := TreeDecontari.FocusedNode;
    if Assigned(lNode) then begin
      edtNrDecont.EditValue   := lNode.Values[TreeDecontariNR_DECONT.ItemIndex];
      edtDataDecont.EditValue := lNode.Values[TreeDecontariDATA_DECONT.ItemIndex];
      edtSumaDecont.EditValue := lNode.Values[TreeDecontariSUMA_DECONT.ItemIndex];
      PersoanaDecont          := Trim(lNode.Values[TreeDecontariNUME.ItemIndex]);
      edtCodGest.EditText     := PersoanaDecont;
      edtCodGest.Tag          := lNode.Values[TreeDecontariCODGEST.ItemIndex];
      if not ValueHasValue(lNode.Values[TreeDecontariCOD_CBT.ItemIndex]) then
        CodCasaDecont := 1
      else CodCasaDecont := lNode.Values[TreeDecontariCOD_CBT.ItemIndex];
      edtDetaliiDecont.Text := Trim(lNode.Texts[TreeDecontariCODSECTIE.ItemIndex]) +':'+ PersoanaDecont+ '|' + Trim(lNode.Texts[TreeDecontariSUMA_DECONT.ItemIndex]);
      btnDelJust.Enabled := True;
      {set fcoddecont and load}
      CodDecont := lNode.Values[TreeDecontariCOD.ItemIndex];
      UpdateSoldCasa;
      if Self.Visible then GridRegistru.SetFocus;
    end;
  end;
end;

procedure TFrmRegistru.chkFilterClick(Sender: TObject);
begin
  if Assigned(GridRegistru.InplaceEditor) then GridRegistru.InplaceEditor.Reset;
  SetMainFilter(FFilter, chkFilter.Checked);
end;

function TFrmRegistru.CalculateTotal(aMemDataSet: TdxMemData;
  aFieldName: String): Currency;
var
  lBookMark : TBookmark;
  lField : TField;
begin
  Result := 0;
  aMemDataSet.DisableControls;
  lBookMark := aMemDataSet.GetBookmark;
    try
    lField := aMemDataSet.FindField(aFieldName);
    aMemDataSet.First;
    while not aMemDataSet.Eof do begin
      if lField.DataType in [ftCurrency, ftFloat, ftInteger] then
         Result := Result + lField.AsCurrency
        else
           Result := Result + 1;
      aMemDataSet.Next;
      end;
    aMemDataSet.GotoBookmark(lBookMark);
    finally
    aMemDataSet.FreeBookmark(lBookMark);
    aMemDataSet.EnableControls;
  end;
end;

procedure TFrmRegistru.CalculateTotalFix(aMemDataSet: TdxMemData;
  var Incasari, Plati : Currency);
var
  lBookMark : TBookmark;
begin
  Incasari := 0;
  Plati := 0;
  aMemDataSet.DisableControls;
  lBookMark := aMemDataSet.GetBookmark;
    try
    aMemDataSet.First;
    while not aMemDataSet.Eof do begin
      Incasari  := Incasari + aMemDataSet.FieldByName('INCASARI').AsCurrency;
      Plati     := Plati    + aMemDataSet.FieldByName('PLATI').AsCurrency;
      aMemDataSet.Next;
      end;
    aMemDataSet.GotoBookmark(lBookMark);
    finally
    aMemDataSet.FreeBookmark(lBookMark);
    aMemDataSet.EnableControls;
  end;
end;


procedure TFrmRegistru.GridRegistruSelectedCountChange(Sender: TObject);
var I : Integer;
    N1, N2 : Integer;
begin
  SelectedIncasari := 0;
  SelectedPlati := 0;
  SelectedIncasariChild := 0;
  SelectedPlatiChild := 0;
  N1 := 0;
  N2 := 0;

  for I := 0 to GridRegistru.SelectedCount -1 do begin
     if Assigned(TdxDBTreeListNode(GridRegistru.SelectedNodes[I]).Parent) then begin
        SelectedIncasariChild := SelectedIncasariChild + GetAsCurrency(GridRegistru.SelectedNodes[I], GridRegistruINCASARI.Index);
        SelectedPlatiChild := SelectedPlatiChild + GetAsCurrency(GridRegistru.SelectedNodes[I], GridRegistruPLATI.Index);
        Inc(N2);
     end
     else begin
        SelectedIncasari := SelectedIncasari + GetAsCurrency(GridRegistru.SelectedNodes[I], GridRegistruINCASARI.Index);
        SelectedPlati := SelectedPlati + GetAsCurrency(GridRegistru.SelectedNodes[I], GridRegistruPLATI.Index);
        Inc(N1);
     end;
  end;

  SelectedSumm.Panels[2].Text := FormatFloat(CurrFormat, SelectedIncasari);
  SelectedSumm.Panels[4].Text := FormatFloat(CurrFormat, SelectedPlati);
  SelectedSumm.Panels[7].Text := FormatFloat(CurrFormat, SelectedIncasariChild);
  SelectedSumm.Panels[9].Text := FormatFloat(CurrFormat, SelectedPlatiChild);
  SelectedSumm.Panels[0].Text := Format('%s/%s/%s', [IntToStr(GridRegistru.SelectedCount), IntToStr(N1), IntToStr(N2)]);

end;


procedure TFrmRegistru.SetRights(aRightType: TListaDrepturi);
begin
   if not IsRightEnable then Exit;
   Cmd_AdaugaPlata.Enabled := False;
   Cmd_AdaugaPozitieNoua.Enabled  := False;
   Cmd_AdaugaDefalcare.Enabled    := False;
   Cmd_EchilibrarePlata.Enabled := False;
   Cmd_DeletePlata.Enabled := False;
   Cmd_Renumeroteaza.Enabled := False;
   Cmd_TransferaPlata.Enabled := False;
   Cmd_AcceptaTransfer.Enabled := False;
   Cmd_AnuleazaTransfer.Enabled := False;
   Cmd_JustificareAvans.Enabled := False;
   Cmd_GenereazaDiferenta.Enabled := False;
   Cmd_Validate.Enabled := False;
   Cmd_UnValidate.Enabled := False;
   Cmd_Flag.Enabled := False;

   {operatii de linie }
   Cmd_AdaugaPlata.Enabled := (td_Casier in aRightType);
   Cmd_AdaugaPozitieNoua.Enabled  := Cmd_AdaugaPlata.Enabled;
   Cmd_AdaugaDefalcare.Enabled    := Cmd_AdaugaPlata.Enabled;
   Cmd_EchilibrarePlata.Enabled   := Cmd_AdaugaPlata.Enabled;
   Cmd_DeletePlata.Enabled        := Cmd_AdaugaPlata.Enabled;
   Cmd_Renumeroteaza.Enabled      := Cmd_AdaugaPlata.Enabled;

   {drept de transfer}
   Cmd_TransferaPlata.Enabled := (td_Casier in aRightType) or (td_Validator in aRightType);
   Cmd_AcceptaTransfer.Enabled := ((td_Casier in aRightType) or (td_Validator in aRightType)) and not(FIsAvans);
   Cmd_AnuleazaTransfer.Enabled := (td_Casier in aRightType) or (td_Validator in aRightType);

   {drept de justificare}
   Cmd_JustificareAvans.Enabled := ((td_Casier in aRightType) or (td_Validator in aRightType)) and not(FIsAvans);
   Cmd_GenereazaDiferenta.Enabled := ((td_Casier in aRightType) or (td_Validator in aRightType)) and (FIsAvans);

   {drept de validare}
   Cmd_Validate.Enabled := (td_Administrator in aRightType) or (td_Validator in aRightType);
   Cmd_UnValidate.Enabled := (td_Administrator in  aRightType) or (td_Validator in aRightType);
   Cmd_Flag.Enabled := (td_Administrator in  aRightType);

   rbFara.Enabled := False;
   rbCont.Enabled := False;
   rbProiect.Enabled := False;

   rbFara.Enabled := (td_Casier in aRightType);
   rbCont.Enabled := (td_Validator in aRightType) or (td_Administrator in aRightType);
   rbProiect.Enabled := (td_Validator in aRightType) or (td_Administrator in aRightType);
end;

procedure TFrmRegistru.GotoCod(var Message: TMessage);
var aLocalizeRecord : TLocalizeRecord;
begin
//  aNode := TdxDBTreeListNode(Message.WParam);
  try
    aLocalizeRecord := TLocalizeRecord(PLocalizeRecord(Message.LParam)^);
    chkEfectiv.Checked := True;
    with aLocalizeRecord do begin
      FStartInterval := Data;
      FEndInterval := Data;
      edData.OnDateChange := nil;
      edData.Date := Data;
      SetCurentHouse(CodCB);
      Application.ProcessMessages;
      if FIsAvans then begin
        // incarcam decontul
        SetCurentDecont(NrDecont, DataDecont, CodGest);
      end;
      MemRegistru.Locate('ID_LISTA', '1.'+ Trim(IntToStr(Cod)), []);
    end;
  finally
    edData.OnDateChange := edDataDateChange;
  end;
end;

(*procedure TFrmRegistru.ValidareData(Sender: TField);
begin
  if not chkEfectiv.Checked then begin
     if (DataEmitereStr = '') then
       DataEmitereStr := Sender.AsString;
       //aNode.Strings[GridRegistruDATA.Index];
     MemRegistru.FieldByName('POZ').AsInteger :=GetNextPosition(DataEmitereStr);
  end;
end;*)

procedure TFrmRegistru.GridRegistruDATADateValidateInput(Sender: TObject;
  const AText: String; var ADate: TDateTime; var AMessage: String;
  var AError: Boolean);
begin
  AError := not IsValidDate(ADate);
  if not AError then AError := not ValidareDataEx(ADate);
  if AError then begin
     AMessage := 'Data Invalida !';
     raise EContaHandledError.Create(AMessage);
//     Abort;
  end;
end;

function TFrmRegistru.ValidareDataEx(AData: TDateTime) : Boolean;
var
  lState : Boolean;
begin
   Result := True;
   try
    lState := DBGoEdit(MemRegistru);
    MemRegistru['POZ']  := GetNextPosition(aData);
    MemRegistru['DATA'] := AData;
    DBPost(MemRegistru);
    if lState then DBGoEdit(MemRegistru);
   except
      Result := False;
   end;
end;

function TFrmRegistru.GetNextDataNode(CurrentNode: TdxTreeListNode): TdxTreeListNode;
var
  lDate : Variant;
begin
  if not Assigned(CurrentNode) then
    Result := GridRegistru.TopNode
  else begin
    Result := CurrentNode;
    { Ne pozitionam la primul nivel }
    while Assigned(Result.Parent) do Result := Result.Parent;
    lDate := Result.Values[GridRegistruDATA.Index];
    repeat
      Result := Result.GetNextSibling;
    until not Assigned(Result) or not ValueSameValue(lDate, Result.Values[GridRegistruDATA.Index]);
  end;
  end;

  procedure TFrmRegistru.RenumeroteazaNode(aNode : TdxTreeListNode);

  procedure SetInnerNodes(ANode: TdxTreeListNode; const APoz: Integer);
  var
    I: Integer;
  begin
    if MemRegistru.Locate('ID_LISTA', ANode.Values[GridRegistruID_LISTA.Index], []) then
      DBSetFieldValue(MemRegistru, 'POZ', APoz);
    for I := 0 to ANode.Count-1 do
      SetInnerNodes(ANode.Items[I], APoz);
end;

var
  lData       : Variant;
  lNewPoz     : Integer;
  lFirstNode  : TdxTreeListNode;
begin
  lData := aNode.Values[GridRegistruDATA.Index];
  { ne pozitionam pe prima pozitie din zi }
  lFirstNode := aNode;
  { Mergem la nivelul TdxTreeList }
  while Assigned(lFirstNode.Parent) do
    lFirstNode := lFirstNode.Parent;
  { Parcurgem la nivelul Tree-ului inregistrarile anterioare si ne pozitionam pe prima inregistrare din ziua curenta }
  while (lFirstNode.GetPrevSibling <> nil) and ValueSameValue(lData, lFirstNode.GetPrevSibling.Values[GridRegistruDATA.Index]) do
    lFirstNode := lFirstNode.GetPrevSibling;
  IsInLoad := True;
  try
    lNewPoz := 1;
    while Assigned(lFirstNode) and ValueSameValue(lFirstNode.Values[GridRegistruDATA.Index], lData) do begin
      SetInnerNodes(lFirstNode, lNewPoz);
      Inc(lNewPoz);
      lFirstNode := lFirstNode.GetNextSibling;
      end;
  finally
    IsInLoad := False;
end;
end;



procedure TFrmRegistru.Cmd_RenumeroteazaAllExecute(Sender: TObject);
var
  lNode : TdxTreeListNode;
begin
  GridRegistru.BeginUpdate;
  MemRegistru.DisableControls;
  try
    lNode := GetNextDataNode(nil);
    while Assigned(lNode) do begin
      RenumeroteazaNode(lNode);
      lNode := GetNextDataNode(lNode);
    end;
  finally
    MemRegistru.EnableControls;
    GridRegistru.EndUpdate;
  end;
end;

procedure TFrmRegistru.SHARE_MOVE(Id, NewId: Integer; Table_Name: String;
  Id_Utilizator: Integer);
begin
  if not QryShare_Point.Active then Exit;
  with QryShare_Point do begin
     SafePost(MemRegistru);

     if Locate('ID;TABLE_NAME;ID_UTILIZATOR', VarArrayOf([Id, Table_Name, Id_Utilizator]),[]) then begin
       Edit;
       FieldByName('TEMP_ID').Value := FieldByName('NEW_ID').Value;
       FieldByName('NEW_ID').AsInteger := NewId;
       Post;
     end
     else begin
       Append;
       FieldByName('TABLE_NAME').AsString := Table_Name;
       FieldByName('ID_UTILIZATOR').AsInteger := Id_Utilizator;
       FieldByName('ID').AsInteger := Id;
       FieldByName('NEW_ID').AsInteger := NewId;
       FieldByName('TEMP_ID').Value := Null;
       Post;
     end;

  end;
end;

function TFrmRegistru.GetLocalId: Integer;
begin
  Inc(LocalId);
  Result := (-1)*LocalId;
end;


procedure TFrmRegistru.CompleteIdentity(aQry: TZQuery; Tbl_name : String; IdField: String;IsTransfer: Boolean; aCont, aProj, aFact : TZQuery);
var aBook : TBookMark;
    aLocalId : Integer;
    Max : Integer;
    EditState : Boolean;
    lOldID, lNewID : Integer;

 function GetNextLocalBulkId(var LocalId : Integer; MaxLocalId : Integer) : Integer;
 begin
   Inc(LocalId);
   if LocalId <= MaxLocalId then  Result := aLocalId
   else Raise EContaHandledError.Create('S-a depasit nr alocat pt id-uri !!!');
 end;

 procedure UpdateShare(OldID, NewID : Integer);
 var OldState :Boolean;
 begin
   with QryShare_Point do begin
    SafePost(MemRegistru);

     if Locate('NEW_ID', OldID, []) then begin
       OldState := State in [dsEdit, dsInsert];
       if not OldState then Edit;
       FieldByName('OLD_ID').AsInteger := OldID;
       FieldByName('NEW_ID').AsInteger := NewID;
       Post;
       if OldState then Edit;
     end;
   end;
 end;

begin

  if aQry.RecordCount <= 0 then Exit;
  aLocalId := GetNextBulkId(Tbl_name, aQry.RecordCount, Max);

  with aQry do begin
   SafePost(MemRegistru);

    First;
    while not eof do begin
      if IsTransfer then begin
         if FieldByName(IdField).AsInteger <= 0 then begin
             lOldID := FieldByName(IdField).AsInteger;
             lNewID := GetNextLocalBulkId(aLocalId, Max);
             EditState := State in [dsEdit, dsInsert];
             if not EditState then Edit;
             FieldByName(IdField).AsInteger := lNewID;
             Post;
             if EditState then Edit;
             UpdateShare(lOldID, lNewID);
             //if FieldByName('COD_TRANSFER').AsInteger < 0 then begin
               if FieldByName('COD_TRANSFER').AsInteger = cst_TransferCode then begin
                   EditState := State in [dsEdit, dsInsert];
                   if not EditState then Edit;
                   FieldByName('COD_TRANSFER').AsInteger := 0;
                   Post;
                   if EditState then Edit;
               end;
               aBook := GetBookmark;
               if Locate('COD_TRANSFER', lOldID, []) then begin
                 EditState := State in [dsEdit, dsInsert];
                 if not EditState then Edit;
                 FieldByName('COD_TRANSFER').AsInteger := lNewID;
                 Post;
                 if EditState then Edit;
               end;
               GotoBookmark(aBook);
               FreeBookmark(aBook);
             //end;
         end;
      end
      else
         if FieldByName(IdField).AsInteger <=0 then begin
            EditState := State in [dsEdit, dsInsert];
            if not EditState then Edit;
            lOldID := FieldByName(IdField).AsInteger;
            lNewID := GetNextLocalBulkId(aLocalId, Max);
            FieldByName(IdField).AsInteger := lNewId;
            Post;
            if CurentrbTag =4 then
              UpdateItemsiDecont(lOldID, lNewId);

            if EditState then Edit;
            UpdateShare(lOldID, lNewID);
          end;
      // actualizam campul in copii
      if IsTransfer then begin
        UpdateChild(aCont, lOldID, lNewID);
        UpdateChild(aProj, lOldID, lNewID);
(*CHRIS SALVARE FACTURI
        UpdateChildF(aFact, lOldID, lNewID);
*)
      end;
      Next;
    end;
  end;
end;

procedure TFrmRegistru.btnNewDecontClick(Sender: TObject);
var
  lFrmDecont: TfrmDetaliiDecont;
begin
  lFrmDecont := TfrmDetaliiDecont.Create(nil);
     try
    lFrmDecont.edtNrDec.EditValue       := edtNrDecont.EditValue;
    lFrmDecont.edtDataDec.EditValue     := edtDataDecont.Date;
    lFrmDecont.CodGest                  := edtCodGest.Tag;
    lFrmDecont.edtRep.EditValue         := edtCodGest.EditValue;
    lFrmDecont.edtSumaDecont.EditValue  := edtSumaDecont.EditValue;
    Self.CheckForSave;
    lFrmDecont.edtRep.Properties.PopupControl := frmCasaContainer.TreeRepartitori;
    if lFrmDecont.ShowModal = mrOk then begin
      AdaugaDecontNou(lFrmDecont.edtNrDec.EditValue, lFrmDecont.edtDataDec.EditValue, lFrmDecont.CodGest, lFrmDecont.edtSumaDecont.EditValue);
       end;
     finally
    lFrmDecont.Free;
   end;
end;

procedure TFrmRegistru.Cmd_ImportExecute(Sender: TObject);

function CompleteRegistruLine(DataIn : TDateTime; CodGest: Variant; Ecl : Integer; Cont, Explicatie, NrDoc, TipDoc : String; Incasari, Plati:Currency) : Boolean;
var
  lNode: TdxDBTreeListNode;
  EditState : Boolean;
  LineInfo : TLineNodeInfo;
begin
  Result := True;
  if MemRegistru.State in [dsEdit, dsInsert] then MemRegistru.Post;
  if not Assigned(GridRegistru.FocusedNode) then lNode := nil
  else lNode := TdxDBTreeListNode(GridRegistru.FocusedNode);
  IsInLoad := True;
  GetLineInfo(lNode, LineInfo);
  AdaugaMemRegistruNew(LineInfo);
  //AdaugaMemRegistru(lNode, False);
  with MemRegistru do begin
    DisableControls;
    EditState := State in [dsEdit, dsInsert];
    if not EditState then Edit;
    IsInLoad := True;
    FieldByName('ECL').AsInteger := Ecl;
    FieldByName('CODGEST').Value := CodGest;
    FieldByName('CONT_CSP').AsString := Cont;
    FieldByName('INCASARI').AsCurrency := Incasari;
    FieldByName('PLATI').AsCurrency := Plati;
    FieldByName('EXPLICATIE').AsString := Explicatie;
    FieldByName('TIPDOC').AsString := TipDoc;
    FieldByName('NRDOC').AsString := NrDoc;
    FieldByName('DATA').AsDateTime := DataIn;
    Post;
    if EditState then Edit;
    EnableControls;
  end;
end;

procedure ImportForCod(CodList : String);
var
  lDataSet: TDataSet;
begin
  lDataSet := DBNewQueryFmt('SELECT CODGEST, CONT_CSP, INCASARI, PLATI, EXPLICATIE, MEXPLIC, TIPDOC, NRDOC, DATA, ECL FROM BREGISTRU'#13#10+
                            'WHERE COD IN (%s)', [CodList]);
    try
    lDataSet.Open;
    lDataSet.First;
    while not lDataSet.Eof do begin
          CompleteRegistruLine(
        lDataSet.FieldByName('DATA').AsDateTime,
        lDataSet.FieldByName('CODGEST').Value,
        lDataSet.FieldByName('ECL').AsInteger,
        lDataSet.FieldByName('CONT_CSP').AsString,
        lDataSet.FieldByName('EXPLICATIE').AsString,
        lDataSet.FieldByName('NRDOC').AsString,
        lDataSet.FieldByName('TIPDOC').AsString,
        lDataSet.FieldByName('INCASARI').AsCurrency,
        lDataSet.FieldByName('PLATI').AsCurrency
          );
      lDataSet.Next;
      end;
    MemRegistruINCASARIChange(nil);
    finally
    lDataSet.Free;
    end;
end;

begin
  if (FIsAvans) and (FCodDecont < 0) then
    Exit;
(*  frmImportCasa := TfrmImportCasa.Create(Self);
  with frmImportCasa do
    try
      edtHouse.Clear;
      edtHouse.Values.Clear;
      edtHouse.Descriptions.Clear;
      edtHouse.Values.Assign(edCurentHouse.Values);
      edtHouse.Descriptions.Assign(edCurentHouse.Descriptions);
      IndexRepartitor := TreeRepartitoriNUME.Index;
      edtRepartitor.PopupControl := TreeRepartitori;
      TreeRepartitori.ControlStyle := TreeRepartitori.ControlStyle + [csClickEvents];
      if FIsAvans then
        IsDecont := 1
      else IsDecont := 2;
      RefreshDataSet;
      ShowModal;
      if ModalResult = mrOk then begin
      if ImportList.Count >=1 then
        ImportForCod(ImportList.CommaText);
     end;
    finally
      Free;
    end;*)
end;

function TFrmRegistru.GetHouseContByIndex(Index: String): String;
var aBookmark : TBookmark;
begin
  Result := '';
  RefreshStructure;
  with QryStructure do
    try
      aBookmark :=  GetBookmark;
      if Locate('COD_CB', Index,[]) then begin
        Result := FieldByName('CRSP_LEI').AsString;
      end;
      GotoBookmark(aBookmark);
      FreeBookmark(aBookmark);
    finally
    end;
end;


procedure TFrmRegistru.CmdErrorsExecute(Sender: TObject);
begin
  TAction(Sender).Tag := TAction(Sender).Tag xor 1;
  if TAction(Sender).Tag = 1 then begin
    if not Assigned(frmSearchErrors) then begin
      frmSearchErrors := TfrmSearchErrors.Create(Self);
      frmSearchErrors.edCasa.Values.Assign(GridRegistruCOD_CasaTransfer.Values);
      frmSearchErrors.edCasa.Descriptions.Assign(GridRegistruCOD_CasaTransfer.Descriptions);
      frmSearchErrors.GridErrorsCOD_CB.Values.Assign(GridRegistruCOD_CasaTransfer.Values);
      frmSearchErrors.GridErrorsCOD_CB.Descriptions.Assign(GridRegistruCOD_CasaTransfer.Descriptions);
      frmSearchErrors.LocalizeHandle := Self.Handle;
      frmSearchErrors.Top := Self.Height - frmSearchErrors.Height - 10;
      frmSearchErrors.Left := Self.Width - frmSearchErrors.Width - 10;
    end;
    frmSearchErrors.Show;
    frmSearchErrors.BringToFront;
  end
  else
    if Assigned(frmSearchErrors) then frmSearchErrors.Hide;
end;

procedure TFrmRegistru.Cmd_SaveLocalExecute(Sender: TObject);
//var lNode : TdxTreeListNode;
begin
  //if not DelphiRunning then Exit;
{  frmRegistruSimplu := TfrmRegistruSimplu.Create(Self);
  with frmRegistruSimplu do
    try
      frmRegistruSimplu.ShowModal;
    finally
      frmRegistruSimplu.Free;
    end;
 }
 {
  lNode := GridRegistru.FocusedNode;
  if not Assigned(lNode) then  Exit;
  EditInregCB(Integer(lNode.Values[GridRegistruCOD.Index]));
 }
  (*
  if MemRegistru.State in [dsEdit,dsInsert] then MemRegistru.Post;
  {sincronizam dataseturile}
  if not(IsSyncro) then SyncronizeDataSets(False, CurentrbTag);
  SaveToLocalComputer;
  HasDied := True;
  *)
end;

procedure TFrmRegistru.Cmd_RecalculateSoldExecute(Sender: TObject);
begin
  if not Assigned(GridRegistru.FocusedNode) then Exit;
  CalculateSold(GridRegistru.FocusedNode);
  GridRegistru.Invalidate;
end;

procedure TFrmRegistru.Cmd_ShowDetailExecute(Sender: TObject);
begin
  TAction(Sender).Tag := TAction(Sender).Tag xor 1;
  pnDetail.Visible := (TAction(Sender).Tag = 1);
end;

procedure TFrmRegistru.Cmd_ShowLegendExecute(Sender: TObject);
begin
  TAction(Sender).Tag := TAction(Sender).Tag xor 1;
  if TAction(Sender).Tag = 1 then begin
    if not Assigned(LegendForm) then begin
      LegendForm := TFrmSettings.Create(Self);
      with LegendForm do Begin
        pnBottom.Visible := False;
        FormStyle := fsStayOnTop;
        BorderIcons := [];
        tabSetariGrid.Enabled := False;
        tabSettings.Enabled := False;
        tabTransfer.Enabled := False;
        rbColor.Visible := False;
        rbFont.Visible := False;
        BorderStyle := bsDialog;
      end;
    end;
      LegendForm.ApplyCurrentSettings;
      LegendForm.Show;
  end
  else begin
    if Assigned(LegendForm) then LegendForm.Hide;
  end;
end;

procedure TFrmRegistru.Cmd_ShowSummaryExecute(Sender: TObject);
begin
  TAction(Sender).Tag := TAction(Sender).Tag xor 1;
  pnSummary.Visible := (TAction(Sender).Tag = 1);
end;

procedure TFrmRegistru.SetAfisTransferEdit(const Value: TDisplayImageEdit);
begin
  FAfisTransferEdit := Value;
  case Value of
    die_TextIcon : begin
      GridRegistruTRANSFER.Images := FrmData.ImaginiTransfer;
      GridRegistruTRANSFER.ShowDescription := True;
    end;
    die_HintIcon : begin
       GridRegistruTRANSFER.Images := FrmData.ImaginiTransfer;
       GridRegistruTRANSFER.ShowDescription := False;
    end;
    die_Image : begin
       GridRegistruTRANSFER.Images := nil;
       GridRegistruTRANSFER.ShowDescription := True;
    end;
  end;
end;

procedure TFrmRegistru.GridRegistruMouseMove(Sender: TObject;
  Shift: TShiftState; X, Y: Integer);
Var Info : TdxTreeListHitInfo;
    aIndex : Integer;
    NewHint : String;
    Hint : PHintRec;


function GetDescriptionsonIndex(col_Descriptions : TdxDBTreeListImageColumn; ValueIndex : String): String;
var
  lIndex : Integer;
begin
  lIndex := col_Descriptions.Values.IndexOf(ValueIndex);
  Result := '';
  if lIndex > -1 then
   Result := col_Descriptions.Descriptions.Strings[lIndex];
end;

begin
//  exit;
   Info := TdxDBTreeList(Sender).GetHitInfo(Point(X,Y));
   //daca am atins noduri din coloane imageedit sau ShowDescription
   if Info.Column = GridRegistruVALIDATA.Index then begin
      if Assigned(Info.Node) then begin
        aIndex := FHintProjList.IndexOf(Info.Node.Strings[GridRegistruV_O.Index]);
        if aIndex > -1 then
          NewHint := PHintRec(FHintProjList.Objects[aIndex])^.Explicatie
        else begin
          if (Trim(Info.Node.Strings[GridRegistruVALIDATA.Index]) <> '') then begin
            New(Hint);
            Hint^.Explicatie :=  '';
            if Trim(Info.Node.Strings[GridRegistruV_O.Index]) <> '' then begin
                NewHint := GetDescriptionsonIndex(GridRegistruV_O, Trim(Info.Node.Strings[GridRegistruV_O.Index]));
                if NewHint <> '' then
                  Hint^.Explicatie := ' de catre utilizatorul : '  + NewHint;
            end;
            Hint^.Explicatie := 'Inregistrare : ' + GetDescriptionsonIndex(GridRegistruVALIDATA, Info.Node.Strings[GridRegistruVALIDATA.Index]) + Hint^.Explicatie;
            Hint^.ID         := Info.Node.Strings[GridRegistruVALIDATA.Index] + '|' +  Trim(Info.Node.Strings[GridRegistruV_O.Index]);
            NewHint          := Hint^.Explicatie;
            FHintProjList.AddObject(Hint^.ID, TObject(Hint));
          end;
        end;
        if (GridRegistru.Hint <> NewHint)  then begin
          GridRegistru.ShowHint := False;
          Application.CancelHint;
          TransferHintNode := Info.Node;
          GridRegistru.Hint := NewHint;
          GridRegistru.ShowHint := (NewHint <> '');
        end;
      end;
   end
   else if (Info.Column >-1) and (GridRegistru.Columns[Info.Column] is TdxDBTreeListImageColumn)
     and not (TdxDBTreeListImageColumn(GridRegistru.Columns[Info.Column]).ShowDescription)then
   begin
       if Assigned(Info.Node) then begin
         aIndex := -1;
         if Trim(Info.Node.Strings[GridRegistru.Columns[Info.Column].Index]) <> '' then
            aIndex := Info.Node.Values[GridRegistru.Columns[Info.Column].Index];
         if aIndex > -1 then
            NewHint := TdxDBTreeListImageColumn(GridRegistru.Columns[Info.Column]).Descriptions.Strings[aIndex]
         else NewHint := '';
         if not Assigned(TransferHintNode) or (TransferHintNode <> Info.Node) or (GridRegistru.Hint <> NewHint)  then begin
            GridRegistru.ShowHint := False;
            Application.CancelHint;
            TransferHintNode := Info.Node;
            GridRegistru.Hint := NewHint;
            GridRegistru.ShowHint := (NewHint <> '');
         end;
       end;
   end
   else
       if rbProiect.Checked then begin
            if Info.Column = GridRegistruPROJ.Index then
              if Assigned(Info.Node) then begin
                aIndex := FHintProjList.IndexOf(Info.Node.Strings[GridRegistruID_PROIECT.Index]);
                if aIndex > -1 then
                  NewHint := PHintRec(FHintProjList.Objects[aIndex])^.Explicatie
                else begin
                  if (Trim(Info.Node.Strings[GridRegistruID_PROIECT.Index]) <> '') then begin
                    New(Hint);
                    Hint^.Explicatie := GetFullNodeString(Info.Node.Values[GridRegistruID_PROIECT.Index], frmCasaContainer.TreeFunctionalDENUMIRE.Index, frmCasaContainer.TreeFunctional);
                    Hint^.ID := Info.Node.Strings[GridRegistruID_PROIECT.Index];
                    NewHint := Hint^.Explicatie;
                    FHintProjList.AddObject(Hint^.ID, TObject(Hint));
                  end;
                end;
                if (GridRegistru.Hint <> NewHint)  then begin
                  GridRegistru.ShowHint := False;
                  Application.CancelHint;
                  TransferHintNode := Info.Node;
                  GridRegistru.Hint := NewHint;
                  GridRegistru.ShowHint := (NewHint <> '');
                end;
              end;
         end
   else begin
       Application.CancelHint;
       TransferHintNode := nil;
       GridRegistru.ShowHint := False;
   end;
   PostMessage(EtichetaHandle, WM_SET_BARHINT, 0, 0);
end;

function TFrmRegistru.CanDoValidation(Validation: Boolean): Boolean;
var aNode : TdxTreeListNode;
    I : Integer;
    aValidValue : Set of 0..6;
begin
  Result := False;

  if IsRightEnable then
    if not((td_Administrator in FHouseRigths) or (td_Validator in FHouseRigths)) then Exit;

  if not Assigned(GridRegistru.FocusedNode) then Exit;
  aNode := GridRegistru.FocusedNode;

  aValidValue := [];
  case Validation of
    True  : aValidValue := [4];
    False : aValidValue := [0];
  end;

  if Validation = True then
    if  not(td_Administrator in FHouseRigths) then
       aValidValue := aValidValue + [1,3];

  Result := True;

  if GridRegistru.SelectedCount > 1 then begin
    for I:= 0 to GridRegistru.SelectedCount -1 do begin
      if (GetAsInteger(GridRegistru.SelectedNodes[I], GridRegistruVALIDATA.Index) in aValidValue) then begin
        Result := False;
        Exit;
      end;
      if GetAsInteger(GridRegistru.SelectedNodes[I], GridRegistruEcl.Index) <> 1 then begin
        Result := False;
        Exit;
      end;
    end;
  end
  else begin
    if Assigned(aNode.Parent) then begin
      Result := False;
      Exit;
    end;
    if (GetAsInteger(aNode, GridRegistruVALIDATA.Index) in aValidValue) then begin
        Result := False;
        Exit;
    end;
    if GetAsInteger(aNode, GridRegistruECL.Index) <> 1 then begin
      Result := False;
      Exit;
    end;
  end;
end;

procedure TFrmRegistru.GridRegistruPopupPopup(Sender: TObject);
begin
  if IsRightEnable or (td_Administrator in FHouseRigths) then begin
    Cmd_Validate.Enabled := CanDoValidation(True);
    Cmd_UnValidate.Enabled := CanDoValidation(False);
    Cmd_Flag.Enabled := CanDoValidation(True);
  end;
end;

procedure TFrmRegistru.CompleteNewIds(aQry: TZQuery; Tbl_name,
  IdField: String);
var aState : Boolean;
    NewId : Integer;
begin
  if aQry.RecordCount <=0 then Exit;
  with aQry do begin
    First;
    While not eof do begin
      if QryShare_Point.Locate('OLD_ID;TABLE_NAME;ID_UTILIZATOR', VarArrayOf([FieldByName(IdField).AsInteger, Tbl_Name, FSignLogin]), []) then begin
        NewId := QryShare_Point.FieldByName('NEW_ID').AsInteger;
        aState := State in [dsEdit, dsInsert];
        if not aState then Edit;
        FieldByName(IdField).AsInteger := NewId;
        Post;
        if aState then Edit;
      end;
      Next;
    end;
  end;
end;

procedure TFrmRegistru.CompleteNewIdsF(aQry: TZQuery; Tbl_name,
  IdFieldS, IdFieldD: String);
var aState : Boolean;
    NewId : Integer;
begin
  if aQry.RecordCount <=0 then Exit;
  with aQry do begin
    First;
    While not eof do begin
      if QryShare_Point.Locate('OLD_ID;TABLE_NAME;ID_UTILIZATOR', VarArrayOf([FieldByName(IdFieldS).AsInteger, Tbl_Name, FSignLogin]), []) then begin
        NewId := QryShare_Point.FieldByName('NEW_ID').AsInteger;
        aState := State in [dsEdit, dsInsert];
        if not aState then Edit;
        FieldByName(IdFieldD).AsInteger := NewId;
        Post;
        if aState then Edit;
      end;
      Next;
    end;
  end;
end;

function TFrmRegistru.GetParentKeyId(aMemData: TdxMemData;
  aKey: String): Integer;
var aBookMark : TBookMark;
begin
  Result := -1;
  aBookMark := nil;
  with aMemData do
    try
      aBookMark := GetBookmark;
      if Locate('ID_LISTA', aKey, []) then begin
         Result := FieldByName('COD').AsInteger;
      end;
    finally
      GotoBookmark(aBookMark);
      FreeBookmark(aBookMark);
    end;
end;

procedure TFrmRegistru.Cmd_ValideazaIesireExecute(Sender: TObject);
begin
  //
end;

function TFrmRegistru.GetFullNodeString(aId: Integer; aIndex : Integer;
  TreeList: TdxDbTreeList): WideString;
var aNode : TdxTreeListNode;
begin
  with TreeList do begin
    Result := '';
    aNode := FindNodeByKeyValue(aId);
    while aNode <> nil do begin
       if Result <> '' then  Result := aNode.Strings[aIndex] + ' -> ' + Result
       else  Result := aNode.Strings[aIndex];
       aNode := aNode.Parent;
    end;
  end;
end;

procedure TFrmRegistru.FlagInregistrareaCurenta1Click(Sender: TObject);
var aNode : TdxTreeListNode;
    aState : Boolean;
    aBookMark : TBookMark;
begin
  if GridRegistru.FocusedNode = nil then Exit;
  aNode := GridRegistru.FocusedNode;

  with MemRegistru do
     try
       DisableControls;
       aBookMark := GetBookmark;
       if Locate('ID_LISTA', aNode.Values[GridRegistruID_LISTA.Index], []) then begin
          aState := State in [dsEdit, dsInsert];
          if not aState then Edit;
          FieldByName('STARE').AsInteger := 0;  // În loc să ștergem, setăm stare 0
          Post;
       end;
       GotoBookmark(aBookMark);
       FreeBookMark(aBookMark);
     finally
        EnableControls;
     end;
end;


procedure TFrmRegistru.TreeStructuraKeyDown(Sender: TObject; var Key: Word;
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

procedure TFrmRegistru.edCurentHouseInitPopup(Sender: TObject);
begin
  RefreshStructure;
  edCurentHouse.PopupWidth  := Max(edCurentHouse.Width, edCurentHouse.PopupWidth);
end;

procedure TFrmRegistru.edCurentHouseCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
var aNode : TdxDBTreeListNode;
    aTipCasa : TTipCasa;
    TempColIndex : Integer;
begin
 if Accept then begin
   CheckForSave;
   Text := '';
   if (Assigned(ImgCasa.Picture.Graphic)) then
      ImgCasa.Picture.Graphic.Assign(nil);

   with TdxPopupEdit(Sender) do
       aNode := TdxDBTreeListNode(TdxDBTreeList(PopupControl).FocusedNode);
       if Assigned(aNode) then begin

          ImagesStructura.GetBitmap(aNode.Values[TreeStructuraICON.Index] , ImgCasa.Picture.Bitmap);
          Text := Trim(aNode.Strings[TreeStructuraDENUMIRE.Index]);

          if Self.Visible then GridRegistru.SetFocus;
          FCurentHouse := aNode.Values[TreeStructuraCOD_CB.Index];
          RegisterCasa(FCurentHouse);

          edData.Enabled := True;
        edListaData.Enabled := True;
          chkEfectiv.Enabled := True;

        FillImageComboFmt(edListaData.Properties, 'select distinct data from bregistru where cod_cb = %d order by data', [FCurentHouse], 0, 0, False);

          aTipCasa := nil;
          GetTipCasa(FCurentHouse, aTipCasa);
        FIsCreditor := aTipCasa.IsCreditor;

          {daca este casa de justificari}
          FIsAvans := aTipCasa.IsAvans;
          FHouseRigths := aTipCasa.Drepturi;
          {setam daca este o casa de incercari}
          FIsTemporHouse := aTipCasa.IsTempor;
        // bobo 2016-04-26
        //btnNewDecont.Enabled := FIsTemporHouse;

          SetRights(FHouseRigths);

          if FIsAvans then begin
            {resetam variabilele}
            edtNrDecont.Value := 0;
            edtDataDecont.Date := Date;
            edtSumaDecont.Value := 0;
            PersoanaDecont:= '';
            CodCasaDecont := 0;
            edtDetaliiDecont.Text := '';
            btnDelJust.Enabled := False;
            FCodDecont := -1;

            if MemRegistru.Active then MemRegistru.Active := False;
            MemRegistru.Active := True;
            edData.Enabled := False;
            edListaData.Enabled := False;
            chkEfectiv.Enabled := False;
            if (IsRightEnable and (FHouseRigths <> [])) or (not IsRightEnable) then begin
               pnDecont.Visible := True;
               pnTop.Height := 65;
               GetDeconturiList(DeconturiList);
               QryJustificari.Close;
               QryJustificari.Params.ParamByName('COD_CB').Value := FCurentHouse;
               QryJustificari.Open;
            end;
          end
          else begin
               FCodDecont := -1;
               if (QryJustificari.Active) then QryJustificari.Close;
               pnDecont.Visible := False;
               pnTop.Height := 40;
          end;

         {daca este banca}
         if aTipCasa.IsBanca then begin
           GridRegistruINCASARI.Caption := 'Plati';
           GridRegistruPLATI.Caption    := 'Incasari';
           if GridRegistruPLATI.ColIndex > GridRegistruINCASARI.ColIndex then begin
             TempColIndex := GridRegistruINCASARI.ColIndex;
             GridRegistruINCASARI.ColIndex := GridRegistruPLATI.ColIndex;
             GridRegistruPLATI.ColIndex := TempColIndex;
           end;
         end
         else begin
           GridRegistruINCASARI.Caption := 'Incasari';
           GridRegistruPLATI.Caption    := 'Plati';
           if GridRegistruPLATI.ColIndex < GridRegistruINCASARI.ColIndex then begin
             TempColIndex := GridRegistruINCASARI.ColIndex;
             GridRegistruINCASARI.ColIndex := GridRegistruPLATI.ColIndex;
             GridRegistruPLATI.ColIndex := TempColIndex;
           end;
         end;

         //GridRegistruCURS_SCHIMB.Visible := False;
         GridRegistruCURS_SCHIMB.Visible := (aTipCasa.TipValuta > 1);
         FIdValuta := aTipCasa.TipValuta;

         {daca data este activata sau daca este un decont numai atunci deschidem dataset-urile}
         if (edData.Enabled) or (FIsAvans and (FCodDecont>0)) then begin
           RefreshDataSet;
         end;
         FreeAndNilCasa(aTipCasa);
         UpdateSoldCasa;
       end;
   end;
   if GridRegistruSORTFIELD.Sorted <> csUP then
     GridRegistruSORTFIELD.Sorted := csUp;
end;

procedure TFrmRegistru.TreeStructuraDblClick(Sender: TObject);
begin
  with TdxDBTreeList(Sender) do
    if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
end;

procedure TFrmRegistru.GetTipCasa(aId: Integer; var aTipCasa: TTipCasa);
begin
  RefreshStructure;
  CommonCasa.GetTipCasa(aId, QryStructure, aTipCasa);
end;

procedure TFrmRegistru.FreeAndNilCasa(var aTipCasa: TTipCasa);
begin
  aTipCasa.Free;
  Pointer(aTipCasa) := nil;
end;

procedure TFrmRegistru.SetCurentHouse(Id: Integer);
var aNode : TdxTreeListNode;
  Accept : Boolean;
  Text : String;
begin
  with TreeStructura do
    aNode := FindNodeByKeyValue(Id);
    if (aNode <> nil) then begin
      aNode.MakeVisible;
      aNode.Focused := True;
      Accept := True;
      edCurentHouseCloseUp(edCurentHouse, Text, Accept);
      if Accept then edCurentHouse.Text := Text;
    end;
end;

procedure TFrmRegistru.edCurentHousePopup(Sender: TObject;
  const EditText: String);
var
  lNode: TdxTreeListNode;
begin
  TreeStructura.EndSearch;
  lNode := TreeStructura.FindNodeByKeyValue(FCurentHouse);
  if Assigned(lNode) then lNode.Focused := True;
end;

procedure TFrmRegistru.AddToValidation(Cod: Integer; Validare : Boolean);
var aRec : PStersRec;
begin
   if RecValidate = nil then begin
     New(RecValidate);
     with RecValidate^ do begin
       ID := Cod;
       if Validare then
         TableName := 'V'
       else TableName := 'D';
       NextSters := nil;
     end;
   end
   else begin
     New(aRec);
     with aRec^ do begin
       ID := Cod;
       if Validare then
         TableName := 'V'
       else TableName := 'D';
       if RecValidate <> nil then NextSters := RecValidate;
     end;
     RecValidate := aRec;
   end;
end;

procedure TFrmRegistru.SaveValidation;
var
  lCurentRec  : PStersRec;
  lDataSet    : TZReadOnlyQuery;
  lValidField : String;
begin
   if RecValidate = nil then Exit;
  if IsRightEnable then
    lValidField := 'V_O'
  else
    lValidField := 'V_O_1';

  lDataSet := DBNewQueryFmt('update bregistru set validata = :VALIDARE, %s = %d where isnull(nr_list, cod) = :COD',
                  [
                    lValidField,
                    IdUtilizator
                  ]);
   repeat
    lCurentRec := RecValidate;
    lDataSet.Params.ParamByName('COD').AsInteger      := lCurentRec^.ID;
    lDataSet.Params.ParamByName('VALIDARE').AsInteger := IfThen(lCurentRec^.TableName = 'V', 1, 0);
    lDataSet.ExecSql;
    RecValidate := RecValidate^.NextSters;
    Dispose(lCurentRec);
   until RecValidate = nil;
end;

procedure TFrmRegistru.Cmd_VenitCasaExecute(Sender: TObject);
var aNode : TdxTreeListNode;
    aTransferStare, aDestStare : Integer;
    IsBanca, DestBanca: Boolean;
    aTipCasa : TTipCasa;
    lDestDate : TdateTime;
    lTopIndex : Integer;
begin
  if not Assigned(GridRegistru.FocusedNode) then Exit;
  aNode := GridRegistru.FocusedNode;


  {daca linia are copii}
{
  if (aNode.HasChildren) then begin
     raise EContaHandledError.Create(StrTransferCopii);
     Exit;
  end;
 }
  {daca suma esta validata atunci eroare}
  if GetAsInteger(ANode, GridRegistruVALIDATA.Index) in  [1,3,4] then begin
     raise EContaHandledError.Create(StrTranferValidat);
     Exit;
  end;
  {daca suma curenta este o plata}
  if (aNode.Strings[GridRegistruINCASARI.Index]='') or (aNode.Values[GridRegistruINCASARI.Index]=0) then begin
     raise EContaHandledError.Create(StrTransferPlata);
     Exit;
  end;

  {daca linia este deja un transfer}
  if ( aNode.Strings[GridRegistruTRANSFER.Index]<>'') and (aNode.Strings[GridRegistruTRANSFER.Index]<>'0') then begin
     raise EContaHandledError.Create(StrTransferTransfer);
     Exit;
  end;

  lTopIndex := GridRegistru.TopIndex;
  with TfrmTransfer.Create(self) do
    try
       aTipCasa := nil;
       aTipCasa := TTipCasa.Create;
       GetTipCasa(FCurentHouse, aTipCasa);
       IsBanca := aTipCasa.IsBanca;

       DefaultIndex := FCurentHouse;
       FTreeList := TreeStructura;
       SetTreeList;
       TipDestinatie := [tt_Casa, tt_Banca];

       edtCasa.Text := edCurentHouse.Text;
       edtSuma.Value := -1.0 * aNode.Values[GridRegistruINCASARI.Index];
       if not TextToDateEx(aNode.Strings[GridRegistruDATA.Index],lDestDate) then
          lDestDate:= Date;
       edtDataDest.Date := lDestDate;
       edtDataPlec.Date := lDestDate;
       edtDataPlec.Text := edtDataDest.Text;

       ShowModal;
       {daca este casa sau banca transferul difera}
       aTransferStare := Integer(not(IsBanca))*12 + Integer(IsBanca)*13;
       {daca se accepta transferul}
       if ModalResult = mrOk then begin
         DestroyTreeList;
         GetTipCasa(HouseIndex, aTipCasa);
         DestBanca := aTipCasa.IsBanca;
         if chkConfirm.Checked then
             aDestStare := Integer(not(DestBanca))*4 + Integer(DestBanca)*8
         else
             aDestStare := Integer(not(DestBanca))*4 + Integer(DestBanca)*8;

         MakeTransfer(aNode, HouseIndex, edtDataDest.Date, edtSuma.Value,  aTransferStare, aDestStare, not(chkConfirm.Checked), -1,0);
       end;
    finally
      aTipCasa.Free;
      Free;
      GridRegistru.TopIndex := lTopIndex;
    end;

end;

procedure TFrmRegistru.RefreshStructure;
var
  lFilter : String;
  lFiltered : Boolean;
begin
 if CaseModified then begin
   QryStructure.DisableControls;
   lFiltered := QryStructure.Filtered;
   lFilter := QryStructure.Filter;
   if QryStructure.Active then QryStructure.Active := False;
   if ModAfisTree then
     QryStructure.Params.ParamByName('DISP_WAY').Value := 1
   else
     QryStructure.Params.ParamByName('DISP_WAY').Value := 0;
   QryStructure.Open;
   QryStructure.Filter := lFilter;
   QryStructure.Filtered := lFiltered;
   QryStructure.EnableControls;
   CaseModified := False;
 end;
end;

procedure TFrmRegistru.Cmd_GotoRecordExecute(Sender: TObject);
var
  LocalizeRecord : PLocalizeRecord;
  aNode : TdxTreeListNode;
  Cod : Integer;
  aDate : TDateTime;
  aQry : TZReadOnlyQuery;
begin
  if not Assigned(GridRegistru.FocusedNode) then Exit;
  aNode := GridRegistru.FocusedNode;
  Cod := GetAsInteger(aNode, GridRegistruCOD_TRANSFER.Index);
  if Cod > 0 then begin
    LocalizeRecord := nil;
    aQry := GetTmpADOQuery;
    with aQry do
      try
        aQry.SQL.Add('EXEC SP_CASA_GET_DETAILS :COD');
        aQry.Params.ParamByName('COD').Value := Cod;
        Open;
        if not IsEmpty then begin
           New(LocalizeRecord);
           LocalizeRecord^.CodCb := aQry.FieldByName('COD_CB').AsInteger;
           aDate := aQry.FieldByName('DATA').AsDateTime;
           LocalizeRecord^.Data := Trunc(aDate);
           if aQry.FindField('LOCAL_COD') <> nil then
              LocalizeRecord^.Cod := aQry.FieldByName('LOCAL_COD').AsInteger
           else
              LocalizeRecord^.Cod := aQry.FieldByName('COD').AsInteger;
           if aQry.FindField('NR_DECONT') <> nil then begin
             LocalizeRecord^.NrDecont := aQry.FieldByName('NR_DECONT').AsInteger;
             LocalizeRecord^.DataDecont := aQry.FieldByName('DATA_DECONT').AsDateTime;
             if aQry.FieldByName('CODGEST').AsString <> '' then
               LocalizeRecord^.CodGest := aQry.FieldByName('CODGEST').AsInteger
             else
               LocalizeRecord^.CodGest := 0;
           end
        end;
      finally
        aQry.Free;
      end;
    if LocalizeRecord <> nil then
      PostMessage(Self.Handle, WM_LOCALIZE, 0, Integer(LocalizeRecord));
  end;
end;

procedure TFrmRegistru.btnPreferedHouseClick(Sender: TObject);
var
  Str : String;
begin
  if PreferedHouseId <> -1 then
    Str := 'Casa Default este : ' + GetHouseByIndex(IntToStr(PreferedHouseId));

  if MessageDlg('Doriti sa stabiliti Casa Implicita ' + GetHouseByIndex(IntToStr(FCurentHouse)) + ' ?'+ #13#10+
    Str, mtInformation, [mbYes, mbNo], 0) = mrYes then PreferedHouseId := FCurentHouse;
end;

procedure TFrmRegistru.SaveNow;
begin
  CheckForSave(True);
 // if FIsAvans then

  if MemRegistru.State in [dsEdit, dsInsert] then
  begin
    MemRegistru.FieldByName('STARE').AsInteger := 1;
    MemRegistru.Post;
  end;
 SetCurentHouse(FCurentHouse);
//  else RefreshDataSet;
end;

procedure TFrmRegistru.edNrZileValidate(Sender: TObject;
  var ErrorText: String; var Accept: Boolean);
begin
   edDataDateChange(nil);
end;

procedure TFrmRegistru.edNrZileKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if Key = VK_RETURN then begin
    PostMessage(Handle, WM_SET_DATA, 0, 0);
  end;
end;

procedure TFrmRegistru.ChangePeriod(var Message: TMessage);
begin
 edDataDateChange(nil);
end;

procedure TFrmRegistru.Cmd_SetBandSizeExecute(Sender: TObject);
var
  I : Integer;
  S : String;
begin
  I := GridRegistru.Bands[0].Width;
  S := IntToStr(I);
  if InputQuery('Marime Banda Stare', 'Setare Marime Banda Stare', S) then begin
    try
      I:= StrToInt(S);
      GridRegistru.Bands[0].Width := I;
    except
    end;
  end;
end;

procedure TFrmRegistru.Cmd_DispozitiePlataExecute(Sender: TObject);
var aNode : TdxTreeListNode;
begin
  if not Assigned(GridRegistru.FocusedNode) then Exit;
  aNode := GridRegistru.FocusedNode;
  PrintDispozitie(aNode.Values[GridRegistruCOD.Index]);
end;

procedure TFrmRegistru.edtTextFiltruKeyPress(Sender: TObject;
  var Key: Char);
begin
  if Key = #13 then
    if Trim(edtTextFiltru.Text) <> '' then
      Filter := StringReplace(edtTextFiltru.Hint, '<|>', edtTextFiltru.Text, [rfReplaceAll])
    else Filter := '';
end;

procedure TFrmRegistru.edtTextFiltruDblClick(Sender: TObject);
var
  S : String;
begin
  S := InputBox('Filtru ', 'Filtru ', 'explicatie like ''%<|>%''');
  if S <> '' then
    edtTextFiltru.Hint := S;
end;

procedure TFrmRegistru.UpdateChild(aQry: TZQuery; lOldId,
  lNewId: Integer);
begin
  if aQry.RecordCount = 0 then Exit;
  with aQry do begin
    while Locate('BREG_COD', lOldId, []) do begin
      if not(State in [dsEdit, dsInsert]) then Edit;
      FieldByName('BREG_COD').AsInteger := lNewId;
      Post;
    end;
  end;
end;

procedure TFrmRegistru.StructuraActualizare(aNode: TdxDBTreeListNode);
begin
  with StrActualizare^ do begin
    Id := '';
    ParentId := '';
    Defalcat := False;
  end;
  if not Assigned(aNode) then Exit;
  with StrActualizare^ do begin
    Id := VarToStr(aNode.Id);
    if VarIsStr(aNode.ParentId) then
      ParentId := VarToStr(aNode.ParentId);
    if aNode.HasChildren then
      ParentId := VarToStr(aNode.Id);
    Defalcat := (ParentId <> '');
  end;
end;

procedure TFrmRegistru.AdaugaMemRegistruNew(const aNodeInfo : TLineNodeInfo; ForcedEntry : Boolean = False);
begin
  {procedura adauga un copil dup nodul aNode}
  if chkFilter.Checked then Exit;
  with MemRegistru do begin
    if (aNodeInfo.RealLineValue <> 0) or ((aNodeInfo.RealLineValue = 0) and  ForcedEntry) then
      try
         GridRegistru.BeginUpdate;
         IsInAdd := True;
         Append;
         if (aNodeInfo.ID = '') or ((aNodeInfo.ECL) and not(ForcedEntry)) then
         begin
            FieldByName('ID_PARINTE').Value := Null;
         end
         else begin
            FieldByName('TRANSFER').AsInteger := aNodeInfo.TransferState;
            FieldByName('ID_PARINTE').Value := aNodeInfo.ID;
            if aNodeInfo.DataParinte <> 0 then
               FieldByName('DATA').AsDateTime := aNodeInfo.DataParinte;
         end;

         FieldByName('COD_CB').AsInteger := FCurentHouse;
         FieldByName('C_O').AsInteger := IdUtilizator;//FSignLogin;
         if FIsAvans then
           FieldByName('DATA').AsDateTime := edtDataDecont.Date
         else
           if chkEfectiv.Checked then
              FieldByName('DATA').AsDateTime := edData.Date
           else
              FieldByName('DATA').AsDateTime := GetActionDate;
         FieldByName('DATAEM').AsDateTime := GetActionDate;
         FieldByName('COD_CB').AsInteger := FCurentHouse;
         FieldByName('TRANSFER').AsInteger := 0;
         FieldByName('COD_TRANSFER').AsInteger := 0;

         FieldByName('ECL').AsInteger := 0;
         {pozitia in cadru zilei}
         if (DataEmitere = 0) and (aNodeInfo.ID <> '') then
           DataEmitere := aNodeInfo.DataParinte
         else
           //trebuie sa folosim functia lui de conversie din data in string
           DataEmitere := FieldByName('DATA').AsDateTime;

         FieldByName('POZ').AsInteger := GetNextPosition(DataEmitere, Trim(FieldByName('ID_PARINTE').AsString));
         if FCodDecont > 0 then begin
            FieldByName('NR_DECONT').AsInteger := Round(edtNrDecont.Value);
            FieldByName('DATA_DECONT').AsDateTime := edtDataDecont.Date;
            FieldByName('PARENT_COD').AsInteger := FCodDecont;
         end;
         FieldByName('ON_SERVER').AsInteger := 0;
         Post;
         if (CurentrbTag = 2) and VarIsNull(FieldByName('ID_PARINTE').Value) then // defalcare pe proiecte
             GridRegistruCONT_CSP.ReadOnly := False;
      finally
        //EnableControls;
        IsInAdd := False;
        GridRegistru.EndUpdate;
      end;
    end;

end;

procedure TFrmRegistru.MemRegistruNewRecord(DataSet: TDataSet);
begin
  if IsInAdd then begin
    with DataSet do begin
      FieldByName('ID_LISTA').AsString := '0.'+IntToStr(FieldByName('RecId').AsInteger+1);
      FieldByName('COD').AsInteger := FieldByName('RecId').AsInteger + 1;
       FieldByName('STARE').AsInteger := 0;
    end;
  end;
end;

function TFrmRegistru.GetActionDate: TDateTime;
begin
  Result := Date;
end;

procedure TFrmRegistru.GetLineInfo(const aNode: TdxDBTreeListNode;
  var aLineInfo: TLineNodeInfo);
begin
  {initializare}
  with aLineInfo do begin
    ID := '';
    RealLineValue := 0;
    ECL := True;
    TransferState := 0;
    DataParinte := 0;
  end;
  if aNode = nil then Exit;

  with aLineInfo do begin
    if Assigned(aNode.Parent) then begin
      ID          := ValueSafeToStr(aNode.ParentId);
      DataParinte := ValueSafeToDateTime(aNode.Parent.Values[GridRegistruDATA.Index]);
    end else begin
      ID          := ValueSafeToStr(aNode.Id);
      DataParinte := ValueSafeToDateTime(aNode.Values[GridRegistruDATA.Index]);
    end;
    RealLineValue := GetRealLineValue(aNode);
    ECL           := ValueSafeToBoolean(aNode.Values[GridRegistruECL.Index]);
    TransferState := ValueSafeToInt(aNode.Values[GridRegistruTRANSFER.Index]);
  end;
end;


function TFrmRegistru.CalcRowHash(GUID: String; aQry: TZQuery): String;
const
  ValidationFields :  array[0..13] of String[20] =
  (
      {1}   ('CODGEST'),
      {2}   ('DATA'),
      {3}   ('TIPDOC'),
      {4}   ('NRDOC'),
      {5}   ('EXPLICATIE'),
      {6}   ('INCASARI'),
      {7}   ('PLATI'),
      {8}   ('CONT_CSP'),
      {9}   ('VAL_CRSP'),
      {10}  ('NR_LIST'),
      {11}  ('TRANSFER'),
      {12}  ('VALIDATA'),
      {13}  ('V_O'),
      {14}  ('V_O_1')
);

var I: Integer;

begin
  Result := '';
  for I := Low(ValidationFields) to High(ValidationFields) do
     Result := Result + Trim(aQry.FieldByName(ValidationFields[I]).AsString);
  Result := Result + GUID;
  Result := MD5Print(MD5String(Result));
end;

procedure TFrmRegistru.FormActivate(Sender: TObject);
begin
  if Self.Visible and GridRegistru.CanFocus then GridRegistru.SetFocus;
end;


procedure TFrmRegistru.TreeStructuraDENUMIREGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
begin
  Exit;
  AText := AText + '('+ANode.Strings[TreeStructuraCRSP_LEI.Index]+')';
end;



procedure TFrmRegistru.Cmd_ReunesteDecontClick(Sender: TObject);
var
  lNrDecont, lCodGest : Integer;
  lDataDec : TDateTime;
  lOldNrDecont,
  lOldCodGest,
  lOldDataDec : Variant;
  Go : Boolean;
  lNode : TcxTreeListNode;
begin
   lNode := TreeDecontari.FocusedNode;
   if lNode = nil then Exit;
   lOldNrDecont := lNode.Values[TreeDecontariNR_DECONT.ItemIndex];
   lOldDataDec  := lNode.Values[TreeDecontariDATA_DECONT.ItemIndex];
   lOldCodGest  := lNode.Values[TreeDecontariCODGEST.ItemIndex];
   lNrDecont := 0;
   lDataDec := 0;
   lCodGest := 0;
   CheckForSave;
   frmAlegDecont := TfrmAlegDecont.Create(nil);
   with frmAlegDecont do
     try
       Deconturi := DeconturiList;
       Deconturi.Sorted := True;
       edtRep.PopupControl := frmCasaContainer.TreeRepartitori;
       HouseIndex := FCurentHouse;
       NrDecont := lOldNrDecont;
       DataDecont := lOldDataDec;
       CodGest := lOldCodGest;
       ShowModal;
       Go := ModalResult = mrOk;
       if Go then begin
         lNrDecont := edtNrDec.IntValue;
         lDataDec := edtDataDec.Date;
         lCodGest := CodGest;
       end;
     finally
       edtRep.PopupControl := nil;
       Free;
     end;
   {must enter in database}
   if not Go then Exit;

    if (lOldNrDecont <> lNrDecont) or (lDataDec <> lOldDataDec) or (lCodGest <> lOldCodGest) then begin
      with QryJustificariUpdate do
        try
          Params.ParamByName('NEW_NR_DECONT').Value := lNrDecont;
          Params.ParamByName('NEW_DATA_DECONT').Value := lDataDec;
          Params.ParamByName('NEW_CODGEST').Value := lCodGest;

          Params.ParamByName('OLD_NR_DECONT').Value := lOldNrDecont;
          Params.ParamByName('OLD_DATA_DECONT').Value := lOldDataDec;
          Params.ParamByName('OLD_CODGEST').Value := lOldCodGest;
          ExecSQL;
        except
          on E:Exception do
            raise EContaHandledError.Create('Eroare la salvarea datelor : ' + #13#10  + E.Message);
        end;
      RefreshQueryDeconturi;
    end;
end;

procedure TFrmRegistru.TreeDecontariDIFERENTACustomDrawCell(
  Sender: TObject; ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  AColumn: TdxTreeListColumn; ASelected, AFocused, ANewItemRow: Boolean;
  var AText: String; var AColor: TColor; AFont: TFont;
  var AAlignment: TAlignment; var ADone: Boolean);
begin
  aFont.Color := clWhite;
  if Pos('-', AText) > 0 then AColor := clRed
  else AColor := clGreen;
end;

procedure TFrmRegistru.GetDeconturiList(var lDeconturiList: TStringList);
var I : Integer;
    aDec : PDecontInf;
begin
  for I := lDeconturiList.Count -1 downto 0 do begin
    if Assigned(lDeconturiList.Objects[I]) then begin
      aDec := PDEcontInf(lDeconturiList.Objects[I]);
      Dispose(aDec);
    end;
    lDeconturiList.Strings[I]:= '';
  end;

  lDeconturiList.Clear;

  with GetTmpADOQuery do
    try
      SQL.Add('SELECT A.NR_DECONT, A.DATA_DECONT, A.COD_CB, A.CODGEST FROM BREGISTRU A JOIN CASIERIE B ON (A.COD_CB = B.COD_CB)');
      SQL.Add(' WHERE A.NR_DECONT IS NOT NULL AND A.DATA_DECONT IS NOT NULL AND ISNULL(B.IS_AVANS,0) = 1 AND A.PARENT_COD IS NULL GROUP BY A.COD_CB, A.NR_DECONT, A.DATA_DECONT, A.CODGEST');
      Open;
      First;
      while not eof do begin
        New(aDec);
        aDec^.NrDec := FieldByName('NR_DECONT').AsInteger;
        aDec^.DataDecont := FieldByName('DATA_DECONT').AsDateTime;
        aDec^.Cod_Casa := FieldByName('COD_CB').AsInteger;
        aDec^.Cod_Gest := Trim(FieldByName('CODGEST').AsString);
        lDeconturiList.AddObject(Trim(FieldByName('COD_CB').AsString)+ '|' + Trim(FieldByName('NR_DECONT').AsString) + '~' + Trim(FieldByName('CODGEST').AsString), TObject(aDec));
        Next;
      end;
    finally
      Free;
    end;
end;

procedure TFrmRegistru.SetCurentDecont(aNrDecont: Integer;
  aDataDecont: TDatetime; aCodGest: Integer);
var
  lNode : TcxTreeListNode;
  lCanClose: Boolean;
begin
  if QryJustificari.Locate('NR_DECONT;DATA_DECONT;CODGEST', VarArrayOf([aNrDecont, aDataDecont, aCodGest]), []) then begin
    lNode := TreeDecontari.FindNodeByKeyValue(QryJustificari['COD']);
    if Assigned(lNode) then begin
      lNode.MakeVisible;
      lNode.Focused := True;
      edtDetaliiDecontPropertiesCloseQuery(edtDetaliiDecont, lCanClose);
    end;
  end;
end;

function TFrmRegistru.GetCurentHint: String;
begin
  Result := GridRegistru.Hint;
end;

function TFrmRegistru.SetVALIDATAonRecord(IDLista : String; ValidValue : Integer) : Boolean;
var
  aState : Boolean;
begin
  Result := False;
  with MemRegistru do
     if Locate('ID_LISTA', IDLista, []) then begin
          aState := State in [dsEdit, dsInsert];
          if not aState then Edit;
          FieldByName('VALIDATA').AsInteger := ValidValue;
          FieldByName('V_O').AsInteger := IdUtilizator;
          if FieldByName('COD_TRANSFER').AsInteger > 0 then AddToValidation(FieldByName('COD_TRANSFER').AsInteger, False);
          Post;
          Result := True;
       end;
end;


procedure TFrmRegistru.VerifyBeforeSave;
var
  lLastPoz : TBookMark;
  lMustAsk : Boolean;
  lKeepPosition : Boolean;
begin
  MemRegistru.DisableControls;
  try
    lLastPoz := MemRegistru.GetBookMark;
    lMustAsk := True;
    if rbFact.Checked then lMustAsk := False;
    lKeepPosition := False;
    MemRegistru.First;
    while not MemRegistru.Eof do begin
      if (lMustAsk) and
         (Trim(MemRegistru.FieldByName('ID_PARINTE').AsString) = '') and
         ((MemRegistru.FieldByName('CONT_CSP').IsNull) or
          (Trim(MemRegistru.FieldByName('CONT_CSP').AsString) = '')) then
      begin
         case MessageDlg('Nu aveti contul corespondent completat pentru inregistrarea ' + MemRegistru.RecIdField.AsString + ' !' + #13#10 +
                       'Doriti sa completati contul corespondent?', mtConfirmation, [mbYes, mbNo, mbNoToAll], 0) of
            mrYes :
              begin
                lKeepPosition := True;
                Abort;
              end;
            mrNo  :
              begin
                if not (MemRegistru.State in [dsEdit, dsInsert]) then
                  MemRegistru.Edit;
                MemRegistru.FieldByName('STARE').AsInteger := 0; // Marcăm ca incompletă
                MemRegistru.Post;
              end;
            mrNoToAll:
              lMustAsk := False;
         end;
      end;
      MemRegistru.Next;
    end;
  finally
    if not lKeepPosition then
      MemRegistru.GotoBookMark(lLastPoz);
    MemRegistru.FreeBookMark(lLastPoz);
    MemRegistru.EnableControls;
  end;
end;


procedure TFrmRegistru.CmdDecontExecute(Sender: TObject);
var
  BancaCod : Integer;
//  lTopIndex : Integer;
begin
  if Self.FIsModified then begin
    CheckForSave(True);
    RefreshDataSet;
  end;

  BancaCod := MemRegistru.FieldByName('COD').AsInteger;
  DoDecont(BancaCod);
end;

procedure TFrmRegistru.WmCheckCursSchimb(var Message: TMessage);
begin
  //

end;

procedure TFrmRegistru.GridRegistruCURS_SCHIMBButtonClick(Sender: TObject;
  AbsoluteIndex: Integer);
var
  ValoareValuta : Currency;
  lData : TDateTime;
  lRep : Integer;
begin
  //ideea este ca in momentul in care se selecteaza primul buton se ia cursul pentru ziua trecuta in data
  //apoi pentru cel de-al doilea buton sau daca nu este valoare in nomenclator

  if not IsValidDate(MemRegistru.FieldByName('DATA').AsDateTime) then begin
    //mesaj de eroare si pozitionare pe campul de data
    MessageDlg('Nu aveti completata data tranzactiei ! Dupa completare veti putea alege valoarea cursului !', mtError, [mbOK], 0);
    if Assigned(GridRegistru.InplaceEditor) and (GridRegistru.InplaceEditor.IsVisible) then begin
      GridRegistru.CancelEditor;
      if GridRegistruDATA.Visible then begin
        GridRegistru.FocusedColumn := GridRegistruDATA.Index;
        GridRegistru.ShowEditor;
      end;
    end
  end
  else begin
    //data cursului
    lData := MemRegistru.FieldByName('DATA').AsDateTime;
  end;

  if Trim(MemRegistru.FieldByName('CODGEST').AsString) = '' then
    lRep := -1
  else
    lRep := MemRegistru.FieldByName('CODGEST').AsInteger;

  if AbsoluteIndex = 0 then begin
     ValoareValuta := GetCursValutar(FIdValuta, lData);
  end;

  if (ValoareValuta = -1) or (AbsoluteIndex = 1) then begin
    ValoareValuta := GetCursForm(FIdValuta, lData, lRep);
  end;
  if ValoareValuta <> - 1 then begin
    if not(MemRegistru.State in [dsEdit, dsInsert]) then MemRegistru.Edit;
    MemRegistru.FieldByName('CURS_SCHIMB').AsCurrency := ValoareValuta;
    MemRegistru.Post;
  end;
end;

procedure TFrmRegistru.Cmd_TransferaPozitieExecute(Sender: TObject);
var aNode : TdxTreeListNode;
    //IsBanca, DestBanca: Boolean;
    aTipCasa : TTipCasa;
    aTransfer : TfrmTransfer;
    lDestDate : TDateTime;
    lTopIndex : Integer;
begin
  if not Assigned(GridRegistru.FocusedNode) then Exit;
  aNode := GridRegistru.FocusedNode;

(*
  {daca linia are copii}
  if (aNode.HasChildren) then begin
     raise EContaHandledError.Create(StrTransferCopii);
     Exit;
  end;
 *)
  {daca suma esta validata atunci eroare}
  if GetAsInteger(ANode, GridRegistruVALIDATA.Index) in [1,3,4] then begin
     raise EContaHandledError.Create(StrTranferValidat);
     Exit;
  end;

 (*
  {daca suma curenta este o incasare}
  if (aNode.Strings[GridRegistruPLATI.Index]='') or (aNode.Values[GridRegistruPLATI.Index]=0) then begin
     raise EContaHandledError.Create(StrTranferIncasare);
     Exit;
  end;
  *)
  {daca linia este deja un transfer}
  if (( aNode.Strings[GridRegistruTRANSFER.Index]<>'') and (aNode.Strings[GridRegistruTRANSFER.Index]<>'0'))
    or (( aNode.Strings[GridRegistruCOD_TRANSFER.Index]<>'') and ( aNode.Strings[GridRegistruCOD_TRANSFER.Index]<>'0')) then begin
     raise EContaHandledError.Create(StrTransferTransfer);
     Exit;
  end;

  lTopIndex := GridRegistru.TopIndex;
  aTransfer := TfrmTransfer.Create(self);
  with aTransfer do
    try
       aTipCasa := nil;
       aTipCasa := TTipCasa.Create;
       DefaultIndex := FCurentHouse;
       GetTipCasa(FCurentHouse, aTipCasa);
       //IsBanca := aTipCasa.IsBanca;

       FTreeList := TreeStructura;
       SetTreeList;
       TipDestinatie := [tt_Casa, tt_Banca];

       edtCasa.Text := edCurentHouse.Text;
       edtSuma.Value := GetRealLineValue(aNode);
//       aNode.Values[GridRegistruPLATI.Index];
       if not TextToDateEx(aNode.Strings[GridRegistruDATA.Index], lDestDate) then
          lDestDate := Date;
       edtDataDest.Date := lDestDate;
       edtDataPlec.Date := lDestDate;
       edtDataPlec.Text := edtDataDest.Text;
       ShowModal;
       {daca se accepta transferul}
       if ModalResult = mrOk then begin
         DestroyTreeList;
         GetTipCasa(HouseIndex, aTipCasa);
         //DestBanca := aTipCasa.IsBanca;
         MakeTransfer(aNode, HouseIndex, edtDataDest.Date, edtSuma.Value, -1, -1 ,not(chkConfirm.Checked), -1,0);
       end;
    finally
      aTipCasa.Free;
      Free;
      GridRegistru.TopIndex := lTopIndex;
    end;
end;

procedure TFrmRegistru.InternalValidateCont(Val: String;
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
  if (MustDrop) and (Assigned(GridRegistru.InplaceEditor)) and not IsInLoad then begin
      if SelNode <> nil then begin SelNode.MakeVisible;
        if SelNode.HasChildren then SelNode.Expanded := True; end
      else Tree.StartSearch(-1, Val);
      SendMessage(GridRegistru.InplaceEditor.Handle, CM_DROPDOWNPOPUPFORM, 0, 0);
      Abort;
  end;
end;

procedure TFrmRegistru.GridRegistruTIPDOCValidate(Sender: TObject;
  var ErrorText: String; var Accept: Boolean);
var
   lValue : Variant;
   lVal : String;
begin
  if Accept and Assigned(GridRegistru.InplaceEditor) and
     (GridRegistru.InplaceEditor.IsVisible) and
      (GridRegistru.InplaceEditor is TdxInplaceTextEdit)
      and (TdxInplaceTextEdit(GridRegistru.InplaceEditor).Text <> '')
   then begin
     lVal := TdxInplaceTextEdit(GridRegistru.InplaceEditor).Text;
     InternalValidateExact(lVal, frmCasaContainer.TreeTipDoc, lValue);
     if (lValue <> null) and (lVal <> lValue) then begin
       TdxInplaceTextEdit(GridRegistru.InplaceEditor).Text := lValue;
       TdxInplaceTextEdit(GridRegistru.InplaceEditor).ValidateEdit;
       TdxInplaceTextEdit(GridRegistru.InplaceEditor).Modified := True;
     end
  end;
end;

procedure TFrmRegistru.InternalValidateExact(Val: String;
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

procedure TFrmRegistru.GridRegistruTIP_CHELTVENValidate(Sender: TObject;
  var ErrorText: String; var Accept: Boolean);
var
   lValue : Variant;
   lVal : String;
   CheckedProjList : TStringList;
   lNode : TdxDBTreeListNode;
begin
  if Accept and Assigned(GridRegistru.InplaceEditor) and
     (GridRegistru.InplaceEditor.IsVisible) and
      (GridRegistru.InplaceEditor is TdxInplaceTextEdit)
      and (TdxInplaceTextEdit(GridRegistru.InplaceEditor).Text <> '')
   then begin
     lVal := TdxInplaceTextEdit(GridRegistru.InplaceEditor).Text;
     InternalValidateExact(lVal, frmCasaContainer.TreeEconomic, lValue, 2, -1);
     if (lValue <> null) then begin
        lNode := TdxDBTreeListNode(GridRegistru.FocusedNode);
        CheckedProjList := frmCasaContainer.GetProjList(frmCasaContainer.TreeEconomic);
        CheckedProjList.Clear;
        CheckedProjList.Add(VarToStr(lValue));
        AdaugaCopiiProjCamp(CheckedProjList, 'ID_TIPURI_CHELTVEN', lNode);
        ClearCheckState(frmCasaContainer.TreeEconomic, CheckedProjList);
        CheckedProjList.Clear;
     end;
     if lValue = null then begin
        InternalValidateCont(lVal, frmCasaContainer.TreeEconomic, lValue);
     end;
  end;
end;


procedure TFrmRegistru.GridRegistruPROJValidate(Sender: TObject;
  var ErrorText: String; var Accept: Boolean);
var
   lValue : Variant;
   lVal : String;
   CheckedProjList : TStringList;
   lNode : TdxDBTreeListNode;
begin
  if Accept and Assigned(GridRegistru.InplaceEditor) and
     (GridRegistru.InplaceEditor.IsVisible) and
      (GridRegistru.InplaceEditor is TdxInplaceTextEdit)
      and (TdxInplaceTextEdit(GridRegistru.InplaceEditor).Text <> '')
   then begin
     lVal := TdxInplaceTextEdit(GridRegistru.InplaceEditor).Text;
     //InternalValidateExact(lVal, frmCasaContainer.TreeFunctional, lValue);
     InternalValidateExact(lVal, frmCasaContainer.TreeFunctional, lValue, 2, -1);
     if (lValue <> null) then begin
        lNode := TdxDBTreeListNode(GridRegistru.FocusedNode);
        CheckedProjList := frmCasaContainer.GetProjList(frmCasaContainer.TreeFunctional);
        CheckedProjList.Clear;
        CheckedProjList.Add(VarToStr(lValue));
        AdaugaCopiiProjCamp(CheckedProjList, 'ID_PROIECT', lNode);
        ClearCheckState(frmCasaContainer.TreeFunctional, CheckedProjList);
        CheckedProjList.Clear;
     end;
     if lValue = null then begin
        InternalValidateCont(lVal, frmCasaContainer.TreeFunctional, lValue);
     end;
  end;
end;

procedure TFrmRegistru.ValidareCheltVen(Sender: TField);
var
  lValue : Variant;
begin
  if InternalAdd then Exit;
  if Sender.IsNull then  Exit;
  InternalValidateExact(Sender.AsString, frmCasaContainer.TreeEconomic, lValue, 1, -1);
  if lValue <> null then
    Sender.DataSet.FieldByName('COD_ECONOMIC').AsString := lValue;
end;



procedure TFrmRegistru.GridRegistruCODGESTValidate(Sender: TObject;
  var ErrorText: String; var Accept: Boolean);
var
   lValue : Variant;
   lVal : String;
begin
  if Accept and Assigned(GridRegistru.InplaceEditor) and
     (GridRegistru.InplaceEditor.IsVisible) and
      (GridRegistru.InplaceEditor is TdxInplaceTextEdit)
      and (TdxInplaceTextEdit(GridRegistru.InplaceEditor).Text <> '')
   then begin
     lVal := TdxInplaceTextEdit(GridRegistru.InplaceEditor).Text;
     InternalValidateExact(lVal, frmCasaContainer.TreeRepartitori, lValue, 2, -1);
     if (lValue <> null) and (lVal <> VarToStr(lValue)) then begin
       TdxInplaceTextEdit(GridRegistru.InplaceEditor).Text := VarToStr(lValue);
       TdxInplaceTextEdit(GridRegistru.InplaceEditor).ValidateEdit;
       TdxInplaceTextEdit(GridRegistru.InplaceEditor).Modified := True;
     end;
     if lValue = null then
       InternalValidateCont(lVal, frmCasaContainer.TreeRepartitori, lValue);
  end;
end;

procedure TFrmRegistru.PostOnParent(MemSearch: TDataSet; aKey,
  aFieldName: String; aValue: Variant);
var
  aBookMark : TBookMark;
  aState : Boolean;
begin
  if Trim(aKey) = '' then Exit;
  with MemSearch do
  try
    aBookMark := GetBookmark;
    if Locate('ID_LISTA', aKey, []) then begin
      aState := State in [dsEdit, dsInsert];
      if not aState then Edit;
      FieldByName(aFieldName).Value := aValue;
      Post;
      if aState then Edit;
    end;
  finally
    GotoBookmark(aBookMark);
    FreeBookmark(aBookMark);
  end;
end;

procedure TFrmRegistru.LocalSyncronizeSharePoint;
var
  lqry : TZReadOnlyQuery;
    lDataSource : TDataSource;
begin
  lqry := GetTmpADOQuery;
  with lqry do
    try
      lDataSource := TDataSource.Create(Self);
      lDataSource.DataSet := QryShare_Point;
      SQL.Add('if not exists(select top 1 1 from share_point where id_share_point = :id_share_point) ');
      SQL.Add('insert into share_point(table_name, id_utilizator, id, new_id, modificator, temp_id, old_id)');
      SQL.Add('values (:table_name, :id_utilizator, :id, :new_id, :modificator, :temp_id, :old_id)');
      SQL.Add('else ');
      SQL.Add('update share_point set table_name = :table_name, id_utilizator = :id_utilizator, id = :id, new_id = :new_id, modificator = :modificator, temp_id = :temp_id, old_id = :old_id where id_share_point = :id_share_point');
      DataSource := lDataSource;
      QryShare_Point.First;
      while not QryShare_Point.Eof do begin
        ExecSQL;
        QryShare_Point.Next;
      end;
    finally
      lDataSource.Free;
      Free;
      DBRefresh(QryShare_Point);
    end;
end;

procedure TFrmRegistru.GridRegistruID_GEST_DOCUMPopup(Sender: TObject;
  const EditText: String);
var
   aRep : Variant;
begin
  aRep := GridRegistru.FocusedNode.Values[GridRegistruID_REPARTITOR.Index];
  if VarIsNull(aRep) then
  begin
    ShowMessage('Completati intai repartitorul');
    Exit;
  end;
end;


procedure TFrmRegistru.UpdateChildF(aQry: TZQuery; lOldId,
  lNewId: Integer);
begin
  if (aQry = nil)  or (aQry.RecordCount = 0)  then Exit;
  with aQry do begin
    while Locate('ID_BREGISTRU', lOldId, []) do begin
      if not(State in [dsEdit, dsInsert]) then Edit;
      FieldByName('ID_BREGISTRU').AsInteger := lNewId;
      Post;
    end;
  end;

end;

procedure TFrmRegistru.GridRegistruORDONANTARECloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
(*
var
    lNode: TdxDBTreeListNode;
    CheckedProjList : TStringList;
    aQry , aTmpQ : TZQuery;
    i, cntrec : integer;
    aNewId : Integer;
    LineInfo,  LI1  : TLineNodeInfo;
    aSearchId : String;
    aBook : TBookMark;
    cond : string;
    aSum, aSumDistr : Currency;
*)
begin
{daca s-a selectat, luam documenetele de pe ordonanatare si le trecem in GEST_DECONTARI, iar itemsii documentelor in GEST_DEFALCARE_DECONTARI}
(*
  with TdxDBTreeListPopupColumn(Sender) do
  begin
    CheckedProjList := frmCasaContainer.GetProjList(TdxDBTreeList(PopupControl));
    if Accept then
    begin
      lNode := TdxDBTreeListNode(GridRegistru.FocusedNode);
      if not Assigned(lNode) then Exit;
      aSum := GetUnitedValue(lNode);
      try
        GridRegistru.BeginUpdate;
        IsInLoad := True;
        GetLineInfo(lNode, LineInfo);
        aSearchId := VarToStr(lNode.Id);
        if Assigned(lNode.Parent) then aSearchId := VarToStr(lNode.ParentId);
         try
            aBook := MemRegistru.GetBookmark;
            if MemRegistru.Locate('ID_LISTA', aSearchId, []) then begin
              if not(MemRegistru.State in [dsEdit, dsInsert]) then MemRegistru.Edit;
              MemRegistru.FieldByName('ON_SERVER').AsInteger := 0;
              MemRegistru.Post;
              FIsModified := True;
            end;
          finally
            MemRegistru.GotoBookmark(aBook);
            MemRegistru.FreeBookmark(aBook);
          end;
         aQry := GetTmpADOQuery;
         cond := '';
         for i :=0 to CheckedProjList.Count -1 do
         begin
             if cond <> '' then
               cond := cond +',';
             cond := cond + CheckedProjList[i];
         end;
        aQry.Close;
        aQry.SQL.Clear;

        aQry.SQL.Add('SELECT B.ID_GEST_DOCUM, SUM(C.REST_PLATA) as REST_PLATA, B.NR_DOCUM,(select cod_docum from gest_tip_docum where id_gest_tip_docum =B.id_gest_tip_docum) as tip_doc,');
        aQry.SQL.Add(' B.DATA_DOCUM, B.TOTALDOC, C.Id_REPARTITORI, C.CONT, C.Id_ALOP_ORDONANTARE');
        aQry.SQL.Add('FROM fnLichidare() C ');
        aQry.SQL.Add('LEFT JOIN  GEST_ITEMSI I ON (C.ID_UNIC_MODUL = I.ID_GEST_ITEMSI)');
        aQry.SQL.Add('JOIN GEST_DOCUM B ON (I.ID_GEST_DOCUM = B.ID_GEST_DOCUM)');
        aQry.SQL.Add('LEFT JOIN CNOTE_IMPERECHERE D ON (C.NR_NOTA=D.NR_OBL)');
        aQry.SQL.Add('WHERE C.Id_UNIC_MODUL IN (' + cond +' )' );
        aQry.SQL.Add('GROUP BY B.ID_GEST_DOCUM, B.NR_DOCUM, B.Id_GEST_TIP_DOCUM, B.DATA_DOCUM, B.TOTALDOC, C.ID_REPARTITORI, C.CONT, C.ID_ALOP_ORDONANTARE ');
        aQry.Open;
        aSumDistr := 0;
        while not aQry.EOf do
        begin
        {adaugam in ecran inregistariile cu id_proiect selectate}
            atmpQ := GetTmpADOQuery;
            atmpQ.Close;
            atmpQ.SQL.Add('SELECT * FROM fnLichidare() WHERE ID_UNIC_MODUL in (SELECT ID_GEST_ITEMSI FROM GEST_ITEMSI WHERE ID_GEST_DOCUM='+intTOStr(aQry.FieldByName('ID_GEST_DOCUM').AsInteger)+') and ID_UNIc_MODUl in (' +cond+')');
            atmpQ.Open;
            while not atmpQ.Eof do
            begin
                    aNewId :=  atmpQ.FieldByName('ID_UNIC_MODUL').AsInteger;
                    AdaugaMemRegistruNew(LineInfo, True);
                    with MemRegistru do begin
                      if not(State in [dsEdit, dsInsert]) then Edit;
                         FieldByName('ID_GEST_DOCUM').AsInteger := aQry.FieldByName('ID_GEST_DOCUM').AsInteger;
                         FieldByname('TIP_DOC').Value := aQry.FieldByName('TIP_DOC').Value;
                         FieldByname('NR_DOCUM').Value := aQry.FieldByName('NR_DOCUM').Value;
                         FieldByname('DATA_DOCUM').Value := aQry.FieldByName('DATA_DOCUM').Value;
                         FieldByname('TOTALDOC').Value := aQry.FieldByName('TOTALDOC').AsCurrency;
                         FieldByname('ID_ALOP_ORDONANTARE').Value := aQry.FieldByName('ID_ALOP_ORDONANTARE').AsInteger;;
                         FieldByname('CONT_CSP').Value := aQry.FieldByName('CONT').AsString;;
                         FieldByname('CODGEST').Value := aQry.FieldByName('ID_REPARTITORI').AsInteger;
                         FieldByname('ID_REPARTITOR').Value := aQry.FieldByName('ID_REPARTITORI').AsInteger;
                         FieldByName(GetFieldIndex(lNode)).AsCurrency := aTmpQ.FieldByName('REST_PLATA').AsCurrency;
                         FieldByname('ID_ALOP_ORDONANTARE').Value := aTmpQ.FieldByName('ID_ALOP_ORDONANTARE').AsCurrency;;
                         FieldByName('ID_UNIC_MODUL').AsInteger := aNewId;
                         FieldByname('CONT_CSP').Value := aTmpQ.FieldByName('CONT').AsString;;
                         FieldByname('CODGEST').Value := aTmpQ.FieldByName('ID_REPARTITORI').AsInteger;;
                         FieldByName('ID_PROIECT').Value := atmpQ.FieldByName('ID_UNIC_MODUL').AsInteger;//GetAsInteger(TdxDBTreeListNode(GridRegistru.FocusedNode), GridRegistruID_PROIECT.Index);;
                         FieldByName('COD_FUNCTIONAL').Value := aTMpQ.FieldByName('COD_FUNCTIONAL').AsString;
                         FieldByName('COD_ECONOMIC').Value := aTMpQ.FieldByName('COD_ECONOMIC').AsString;
                     end;
                     aSumDistr := aSumDistr + aTmpQ.FieldByName('REST_PLATA').AsCurrency;
                 aTMpQ.Next;
            end;
            aQry.Next;
        end;
        finally
          IsInLoad := False;
          GridRegistru.EndUpdate;
        end;
      end;
    end;
    ClearCheckState(frmCasaContainer.TreeOrdonantari, CheckedProjList);
    CheckedProjList.Clear;
    aQry.Free;
    aTmpQ.Free;
  *)
end;



procedure TFrmRegistru.UpdateItemsiDecont(oldId, NewId: integer);
begin
  DBExecSQLFmt('UPDATE GEST_DEFALCARE_DECONTARI SET ID_GEST_DECONTARI=%d WHERE ID_GEST_DECONTARI=%d', [NewId, OldId] );
    end;

procedure TFrmRegistru.UpdateSoldCasa;
var
  lSuma : String;
begin
  lSuma := FormatFloat('#,##0.00', SoldInitial);
  TFrmCasa(Owner).lbSoldInitial.Caption := Format(StrSoldInitianl, [DateToStr(FStartInterval), lSuma]);
end;

procedure TFrmRegistru.GridRegistruORDCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
(*
var
    lNode: TdxDBTreeListNode;
    ord : integer;
    aQry , aTmpQ : TZQuery;
    i, cntrec : integer;
    aNewId : Integer;
    LineInfo : TLineNodeInfo;
    aSearchId : String;
    aBook : TBookMark;
    cond : string;
    aSum, aSumDistr : Currency;
*)
begin
{daca s-a selectat, luam documenetele de pe ordonanatare si le trecem in GEST_DECONTARI, iar itemsii documentelor in GEST_DEFALCARE_DECONTARI}

(*
  ord := frmData.QOrdonantari.FieldByName('ID_ALOP_ORDONANTARE').AsInteger;
    if Accept then
    begin
      lNode := TdxDBTreeListNode(GridRegistru.FocusedNode);
      if not Assigned(lNode) then Exit;
      aSum := GetUnitedValue(lNode);
      try
        GridRegistru.BeginUpdate;
        IsInLoad := True;
        GetLineInfo(lNode, LineInfo);
        aSearchId := VarToStr(lNode.Id);
        if Assigned(lNode.Parent) then aSearchId := VarToStr(lNode.ParentId);
         try
            aBook := MemRegistru.GetBookmark;
            if MemRegistru.Locate('ID_LISTA', aSearchId, []) then begin
              if not(MemRegistru.State in [dsEdit, dsInsert]) then MemRegistru.Edit;
              MemRegistru.FieldByName('ON_SERVER').AsInteger := 0;
              MemRegistru.Post;
              FIsModified := True;
            end;
          finally
            MemRegistru.GotoBookmark(aBook);
            MemRegistru.FreeBookmark(aBook);
          end;
         aQry := GetTmpADOQuery;
         aQry.SQL.Text := 'exec spRunBeforeRapNote';
         aQry.ExecSQL;

         aQry.Close;
         aQry.Sql.Clear;
         aQry.SQL.Add('SELECT ID_UNIC_MODUL FROM fnLichidare() WHERE REST_PLATA<>0 AND ID_ALOP_ORDONANTARE=' + IntTOStr(Ord));
         aQry.Open;
         cond := '';
         while not aQry.EOF do
         begin
             if cond <> '' then
               cond := cond +',';
             cond := cond + IntTOStr(aQry.FIeldByNAme('ID_UNIC_MODUL').AsInteger);
             aqry.Next;
         end;
        aQry.Close;
        aQry.SQL.Clear;

        aQry.SQL.Add('SELECT B.ID_GEST_DOCUM, SUM(C.REST_PLATA) as REST_PLATA, B.NR_DOCUM,(select cod_docum from gest_tip_docum where id_gest_tip_docum =B.id_gest_tip_docum) as tip_doc,');
        aQry.SQL.Add(' B.DATA_DOCUM, B.TOTALDOC, C.Id_REPARTITORI, C.CONT, C.Id_ALOP_ORDONANTARE');
        aQry.SQL.Add('FROM fnLichidare() C ');
        aQry.SQL.Add('LEFT JOIN  GEST_ITEMSI I ON (C.ID_UNIC_MODUL = I.ID_GEST_ITEMSI)');
        aQry.SQL.Add('JOIN GEST_DOCUM B ON (I.ID_GEST_DOCUM = B.ID_GEST_DOCUM)');
        aQry.SQL.Add('LEFT JOIN CNOTE_IMPERECHERE D ON (C.NR_NOTA=D.NR_OBL)');
        aQry.SQL.Add('WHERE C.Id_UNIC_MODUL IN (' + cond +' )' );
        aQry.SQL.Add('GROUP BY B.ID_GEST_DOCUM, B.NR_DOCUM, B.Id_GEST_TIP_DOCUM, B.DATA_DOCUM, B.TOTALDOC, C.ID_REPARTITORI, C.CONT, C.ID_ALOP_ORDONANTARE ');
        aQry.Open;
        aSumDistr := 0;
        while not aQry.EOf do
        begin
        {adaugam in ecran inregistariile cu id_proiect selectate}
            atmpQ := GetTmpADOQuery;
            atmpQ.Close;
            atmpQ.SQL.Add('SELECT * FROM fnLichidare() WHERE ID_UNIC_MODUL in (SELECT ID_GEST_ITEMSI FROM GEST_ITEMSI WHERE ID_GEST_DOCUM='+intTOStr(aQry.FieldByName('ID_GEST_DOCUM').AsInteger)+') and ID_UNIc_MODUl in (' +cond+')');
            atmpQ.Open;
            while not atmpQ.Eof do
            begin
                    aNewId :=  atmpQ.FieldByName('ID_UNIC_MODUL').AsInteger;
                    AdaugaMemRegistruNew(LineInfo, True);
                    with MemRegistru do begin
                      if not(State in [dsEdit, dsInsert]) then Edit;
                         FieldByName('ID_GEST_DOCUM').AsInteger := aQry.FieldByName('ID_GEST_DOCUM').AsInteger;
                         FieldByname('TIP_DOC').Value := aQry.FieldByName('TIP_DOC').Value;
                         FieldByname('NR_DOCUM').Value := aQry.FieldByName('NR_DOCUM').Value;
                         FieldByname('DATA_DOCUM').Value := aQry.FieldByName('DATA_DOCUM').Value;
                         FieldByname('TOTALDOC').Value := aQry.FieldByName('TOTALDOC').AsCurrency;
                         FieldByname('ID_ALOP_ORDONANTARE').Value := aQry.FieldByName('ID_ALOP_ORDONANTARE').AsInteger;;
                         FieldByname('CONT_CSP').Value := aQry.FieldByName('CONT').AsString;;
                         FieldByname('CODGEST').Value := aQry.FieldByName('ID_REPARTITORI').AsInteger;
                         FieldByname('ID_REPARTITOR').Value := aQry.FieldByName('ID_REPARTITORI').AsInteger;
                         FieldByName(GetFieldIndex(lNode)).AsCurrency := aTmpQ.FieldByName('REST_PLATA').AsCurrency;
                         FieldByname('ID_ALOP_ORDONANTARE').Value := aTmpQ.FieldByName('ID_ALOP_ORDONANTARE').AsCurrency;;
                         FieldByName('ID_UNIC_MODUL').AsInteger := aNewId;
                         FieldByname('CONT_CSP').Value := aTmpQ.FieldByName('CONT').AsString;;
                         FieldByname('CODGEST').Value := aTmpQ.FieldByName('ID_REPARTITORI').AsInteger;;
                         FieldByName('ID_PROIECT').Value := atmpQ.FieldByName('ID_UNIC_MODUL').AsInteger;//GetAsInteger(TdxDBTreeListNode(GridRegistru.FocusedNode), GridRegistruID_PROIECT.Index);;
                         FieldByName('COD_FUNCTIONAL').Value := aTMpQ.FieldByName('COD_FUNCTIONAL').AsString;
                         FieldByName('COD_ECONOMIC').Value := aTMpQ.FieldByName('COD_ECONOMIC').AsString;
                     end;
                     aSumDistr := aSumDistr + aTmpQ.FieldByName('REST_PLATA').AsCurrency;
                 aTMpQ.Next;
            end;
            aQry.Next;
        end;
        finally
          IsInLoad := False;
          GridRegistru.EndUpdate;
        end;
      end;
    aQry.Free;
    aTmpQ.Free;
  *)
end;

procedure TFrmRegistru.RefreshDeconturi;
begin
  if FIsAvans then
    RefreshQueryDeconturi;
end;
      procedure SafePost(DataSet: TDataSet);
begin
  if DataSet.State in [dsEdit, dsInsert] then
    DataSet.Post;
end;

procedure TFrmRegistru.btnOkDecontClick(Sender: TObject);
begin
  GetParentForm(pnDeconturi).ModalResult := mrOk;
end;

procedure TFrmRegistru.btnCancelDecontClick(Sender: TObject);
begin
  GetParentForm(pnDeconturi).ModalResult := mrCancel;
end;

procedure TFrmRegistru.chkDifClick(Sender: TObject);
begin
  QryJustificari.Filtered := False;
  if chkDif.Checked then
    QryJustificari.Filter := 'DIFERENTA <> 0'
  else
    QryJustificari.Filter := '';
  QryJustificari.Filtered := chkDif.Checked;
end;

procedure TFrmRegistru.edDataDateValidateInput(Sender: TObject;
  const AText: String; var ADate: TDateTime; var AMessage: String;
  var AError: Boolean);
begin
  if AError then begin
    AError := False;
    raise EContaHandledError.Create('Data de Inceput nu este corecta !');
  end;
end;

procedure TFrmRegistru.CmdCopyColumnExecute(Sender: TObject);
var
   lValue : Variant;
begin
  if (GridRegistru.FocusedNode <> nil) and (GridRegistru.FocusedColumn+1 >-1) and (GridRegistru.FocusedNode.Index > 0) then begin
    lValue := GridRegistru.Items[GridRegistru.FocusedNode.Index - 1].Values[GridRegistru.VisibleColumns[GridRegistru.FocusedColumn].Index];
    DBSetFieldValue(MemRegistru, GridRegistru.VisibleColumns[GridRegistru.FocusedColumn].FieldName, lValue );
  end;
end;

procedure TFrmRegistru.WMHideProgress(var message: TMessage);
begin
  if not Assigned(ProgressForm) then Exit;
  if message.WParam = 1 then begin
    if not ProgressForm.Visible then begin
      ProgressForm.Visible := True;
      ProgressForm.BringToFront;
    end;
  end
  else
    ProgressForm.Visible := False;
end;

function TFrmRegistru.GetValCodDecont: Variant;
begin
  if FCodDecont > 0 then
    Result := FCodDecont
  else
    Result := Null;
end;

procedure TFrmRegistru.GridRegistruCollapsing(Sender: TObject;
  Node: TdxTreeListNode; var Allow: Boolean);
begin
  Allow := not chkExpand.Checked;
end;

procedure TFrmRegistru.edListaDataPropertiesChange(Sender: TObject);
begin
  edData.Date := edListaData.EditingValue;
end;

procedure TFrmRegistru.btnAdaugaDecontClick(Sender: TObject);
begin
  btnNewDecont.Click;
end;

procedure TFrmRegistru.AdaugaDecontNou(ANrDecont, ADataDecont, ACodGest, ASumaDecont: Variant);
var
  lSearchKey  : String;
  lNode       : TcxTreeListNode;
begin
  DBExecSQLFmt('exec [spAdaugaJustificare] @refSesiune = %d, @refUtilizator = %d, @refCasa = %d, @refRepatitor = %d, @nrDecont = %d, @dataDecont = %s, @sumaDecont = %s',
               [
                IdLogin,
                IdUtilizator,
                CurentHouse,
                ACodGest,
                ANrDecont,
                ValueToStr(ADataDecont),
                ValueToStr(ASumaDecont)
               ]);
  MemReg.Active := False;
  MemCont.Active := False;
  MemProj.Active := False;

  RefreshQueryDeconturi;
  lSearchKey  := Trim(IntToStr(ANrDecont)) + '|'+ Trim(FormatDateTime('dd''/''mm''/''yyyy', ADataDecont))+ '~'+Trim(IntToStr(ACodGest));
  lNode       := TreeDecontari.FindNodeByText(lSearchKey, TreeDecontariCHEIE);
  if Assigned(lNode) then begin
    lNode.MakeVisible;
    lNode.Focused           := True;
    edtNrDecont.EditValue   := lNode.Values[TreeDecontariNR_DECONT.ItemIndex];
    edtDataDecont.EditValue := lNode.Values[TreeDecontariDATA_DECONT.ItemIndex];
    edtSumaDecont.EditValue := lNode.Values[TreeDecontariSUMA_DECONT.ItemIndex];
    PersoanaDecont          := Trim(lNode.Texts[TreeDecontariNUME.ItemIndex]);
    CodCasaDecont           := lNode.Values[TreeDecontariCOD_CBT.ItemIndex];
    edtDetaliiDecont.Text   := Trim(lNode.Texts[TreeDecontariCODSECTIE.ItemIndex]) +':'+ PersoanaDecont+ '|' + Trim(lNode.Texts[TreeDecontariSUMA_DECONT.ItemIndex]);
    btnDelJust.Enabled      := True;
    CodDecont               := lNode.Values[TreeDecontariCOD.ItemIndex];
    UpdateSoldCasa;
    if Self.Visible then GridRegistru.SetFocus;
  end;
end;

procedure TFrmRegistru.ClearDecontInfo;
begin
  {resetam variabilele}
  FCodDecont          := -1;
  btnDelJust.Enabled  := False;
  edtNrDecont.Text    := '';
  edtDataDecont.Date  := Date;
  edtSumaDecont.Value := 0;
  PersoanaDecont      := '';
  edtCodGest.Tag      := 0;
  edtCodGest.Text     := '';
  CodCasaDecont       := 0;
end;

procedure TFrmRegistru.RefreshQueryDeconturi;
begin
  QryJustificari.Close;
  ClearDecontInfo;
  QryJustificari.Open;
end;

function TFrmRegistru.GetDefalcareType: Integer;
begin
  Result := rbCont.Tag * Integer(rbCont.Checked) + rbFara.Tag * Integer(rbFara.Checked) + rbProiect.Tag * Integer(rbProiect.Checked) + rbFact.Tag * Integer(rbFact.Checked);
end;

procedure TFrmRegistru.Cmd_AdaugaPozitieNouaExecute(Sender: TObject);
var
  LineInfo : TLineNodeInfo;
begin
  if (FIsAvans) and (FCodDecont < 0) then begin
    MessageBeep(MB_ICONEXCLAMATION);
    Exit;
  end;
  DBPost(MemRegistru);
  LineInfo.ID   := '';
  LineInfo.RealLineValue := -1;
  LineInfo.ECL  := True;
  AdaugaMemRegistruNew(LineInfo);
  GridRegistru.FocusedColumn := 0;
end;

procedure TFrmRegistru.Cmd_AdaugaDefalcareExecute(Sender: TObject);
var
  lNode: TdxDBTreeListNode;
  LineInfo : TLineNodeInfo;
begin
  if (FIsAvans) and (FCodDecont < 0) then begin
   MessageBeep(MB_ICONEXCLAMATION);
   Exit;
  end;
  lNode := TdxDBTreeListNode(GridRegistru.FocusedNode);
  if Assigned(lNode) then begin
    DBPost(MemRegistru);
    GetLineInfo(lNode, LineInfo);
    AdaugaMemRegistruNew(LineInfo, True);
    GridRegistru.FocusedColumn := 0;
  end;
end;

procedure TFrmRegistru.VerificaPozitieEchilibrata(ANode: TdxTreeListNode = nil);
var
  lTotalCod,
  lTotalPozitii : Currency;
  lEchilibrat   : Boolean;
  I             : Integer;
begin
  if not Assigned(ANode) then
    ANode := GridRegistru.FocusedNode;
  if Assigned(ANode) then begin
    CalculateSold(ANode);
    if Assigned(ANode.Parent) then begin
      lTotalCod := GetUnitedValue(ANode.Parent);
      ANode := ANode.Parent;
    end
    else
      lTotalCod := GetUnitedValue(ANode);

    lEchilibrat := ANode.Count = 0;
    if not lEchilibrat then begin
      lTotalPozitii := 0;
      for I := 0 to ANode.Count-1 do
        lTotalPozitii := lTotalPozitii + GetUnitedValue(ANode.Items[I]);
      lEchilibrat := lTotalPozitii = lTotalCod;
    end;

    if lEchilibrat then
      case CurentrbTag of
        1 : lEchilibrat := lEchilibrat and (GridRegistru.FocusedNode.Strings[GridRegistruCONT_CSP.Index] <> '%');
        2 : lEchilibrat := lEchilibrat and (GridRegistru.FocusedNode.Strings[GridRegistruPROJ.Index] <> '%');
      end;

    StructuraActualizare(TdxDBTreeListNode(aNode));
    PostMessage(Handle, WM_SET_STARE_SOLD, NativeInt(StrActualizare), NativeInt(lEchilibrat));
  end;
end;

procedure TFrmRegistru.BtnModificaDecontClick(Sender: TObject);
begin
  ModificaDecont(edtNrDecont.EditValue, edtDataDecont.EditValue, edData.Date, edtCodGest.Tag, edtSumaDecont.EditValue);
end;

procedure TFrmRegistru.ModificaDecont(ANrDecont, ADataDecont, ADataRegistru, ACodGest, ASumaDecont: Variant);
var
  lSearchKey  : String;
  lCodDecont  : Integer;
  lNode       : TcxTreeListNode;
  lFrmDecont  : TfrmDetaliiDecont;
begin
  Self.CheckForSave;
  lSearchKey := Trim(IntToStr(ANrDecont)) + '|'+ Trim(FormatDateTime('dd''/''mm''/''yyyy', ADataDecont))+ '~'+Trim(IntToStr(ACodGest));
  lNode := TreeDecontari.FindNodeByText(lSearchKey, TreeDecontariCHEIE);
  if Assigned(lNode) then begin
    lCodDecont := lNode.Values[TreeDecontariCOD.ItemIndex];
    lFrmDecont := TfrmDetaliiDecont.Create(nil);
    try
      lFrmDecont.edtRep.Properties.PopupControl := frmCasaContainer.TreeRepartitori;
      lFrmDecont.edtNrDec.EditValue             := ANrDecont;
      lFrmDecont.edtDataDec.EditValue           := ADataDecont;
      lFrmDecont.CodGest                        := ACodGest;
      lFrmDecont.edtSumaDecont.EditValue        := ASumaDecont;
      lFrmDecont.edtSumaDecont.Enabled          := False;
      lFrmDecont.edtDataRegistru.Visible        := True;
      lFrmDecont.edtDataRegistru.EditValue      := ADataRegistru;
      if lFrmDecont.ShowModal = mrOk then begin
        DBExecSQLFmt('exec [spModificaJustificare] @refSesiune = %d, @refUtilizator = %d, @refCasa = %d, @refDecont = %d, @refRepartitor = %d, @nrDecont = %s, @dataDecont = %s, @dataRegistru = %s, @sumaDecont = %s',
                     [
                      IdLogin,
                      IdUtilizator,
                      CurentHouse,
                      lCodDecont,
                      lFrmDecont.CodGest,
                      ValueToStr(lFrmDecont.edtNrDec.EditValue),
                      ValueToStr(lFrmDecont.edtDataDec.EditValue),
                      ValueToStr(lFrmDecont.edtDataRegistru.EditValue),
                      ValueToStr(lFrmDecont.edtSumaDecont.EditValue)
                     ]);
        MemReg.Active := False;
        MemCont.Active := False;
        MemProj.Active := False;
        RefreshQueryDeconturi;
        lNode := TreeDecontari.FindNodeByText(IntToStr(lCodDecont), TreeDecontariCOD);
        if Assigned(lNode) then begin
          lNode.MakeVisible;
          lNode.Focused               := True;
          edtNrDecont.EditValue       := lNode.Values[TreeDecontariNR_DECONT.ItemIndex];
          edtDataDecont.EditValue     := lNode.Values[TreeDecontariDATA_DECONT.ItemIndex];
          edtSumaDecont.EditValue     := lNode.Values[TreeDecontariSUMA_DECONT.ItemIndex];
          PersoanaDecont              := Trim(lNode.Texts[TreeDecontariNUME.ItemIndex]);
          CodCasaDecont               := lNode.Values[TreeDecontariCOD_CBT.ItemIndex];
          edtDetaliiDecont.EditValue  := Trim(lNode.Texts[TreeDecontariCODSECTIE.ItemIndex]) +':'+ PersoanaDecont+ '|' + Trim(lNode.Texts[TreeDecontariSUMA_DECONT.ItemIndex]);
          btnDelJust.Enabled          := True;
          CodDecont                   := lNode.Values[TreeDecontariCOD.ItemIndex];
          UpdateSoldCasa;
          if Self.Visible then GridRegistru.SetFocus;
        end;
      end;
    finally
      lFrmDecont.Free;
    end;
  end;
end;

end.
