unit Unit2;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs, StdCtrls, Buttons,
  ExtCtrls, Db, ZDataSet, Menus, cxLookAndFeelPainters, cxButtons, cxGroupBox, cxRepartitorPanel,
  cxCalendar, cxTextEdit, cxControls, cxContainer, cxEdit, cxMaskEdit, cxDropDownEdit, cxStyles,
  cxGraphics, cxDataUtils, cxDataStorage, cxDBData, cxGridLevel, cxClasses, cxGridCustomView,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGrid, cxCurrencyEdit, cxImageComboBox,
  cxGridBandedTableView, cxGridDBBandedTableView, cxButtonEdit, cxTL, cxInplaceContainer, cxTLData,
  cxDBTL, cxProgressBar, ZAbstractRODataset, ZAbstractDataset, cxTLdxBarBuiltInMenu, cxLookAndFeels,
  cxCustomData, cxFilter, cxData, cxPC, cxCheckBox, frmSelectieContractUnit, frmSelectieDosarUnit,
  ZSqlUpdate, cxSpinEdit, cxCalc, ActnList, cxGridCustomPopupMenu, cxGridPopupMenu, fmSelectieCEUnit,
  fmSelectieCFUnit, fmSelectieRepartitorUnit, cxLabel, cxNavigator, ComCtrls, dxCore, cxDateUtils,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxBarBuiltInMenu,
  dxDateRanges, dxScrollbarAnnotations;

const
  WM_REFRESH_DEFALCARE = WM_USER + 1;

type
  PInfoFunctional = ^TInfoFunctional;
  TInfoFunctional = record
    Id : Variant;
    IdUnitate : Variant;
    CodFunctional : string[100];
    CodEcran : string[100];
    Denumire : string[254];
  end;


  TFormAlop = class(TForm)
    pnDocument: TPanel;
    lbDocument: TLabel;
    LbDataNota: TLabel;
    LbPredator: TLabel;
    DTAngajamente: TDataSource;
    QryAngajamenteDefalcate: TZQuery;
    LbPrimitor: TLabel;
    QryAngajamente: TZQuery;
    Label1: TLabel;
    Label2: TLabel;
    lbContract: TLabel;
    edPredator: TcxPopupEdit;
    edPrimitor: TcxPopupEdit;
    edtDetaliiContract: TcxPopupEdit;
    edDataDoc: TcxDateEdit;
    edTipAngajament: TcxImageComboBox;
    edNumarDoc: TcxButtonEdit;
    edRectificat: TcxButtonEdit;
    Label3: TLabel;
    cxFunctionalBar: TcxPopupEdit;
    LbScopul: TLabel;
    edNrProiect: TcxButtonEdit;
    edtScop: TcxTextEdit;
    Label4: TLabel;
    lbLegal: TLabel;
    edLegal: TcxButtonEdit;
    btnAdaugaPozitii: TcxButton;
    cxButton2: TcxButton;
    edSumaProiect: TcxCurrencyEdit;
    lbSumaProiect: TcxLabel;
    chkRectificareSold: TcxCheckBox;
    lbNumar: TLabel;
    edtDetaliiDosar: TcxPopupEdit;

   // procedure edPrimitorPropertiesChange(Sender: TObject);
  private
    FIdProiect          : Variant;
    FAreDefalcareProcent: Boolean;
    FCurentAngajament: Integer;
    IsInLoading      : Boolean;
    FErrRecord : String;
    FExecOnValidation: TNotifyEvent;
    FInfoFunctional : PInfoFunctional;
    FSelectieContract : TfrmSelectieContract;
    FSelectieDosar    : TfrmSelectieDosar;
    FSelectieCE       : TfmSelectieCE;
    FSelectieCF       : TfmSelectieCF;
    FSelectieDep      : TfmSelectieRepartitor;
    FSelectieBen      : TfmSelectieRepartitor;
    FIsInDelete       : Boolean;


    { Private declarations }
  public
    { Public declarations }
  protected
  end;

implementation

{$R *.DFM}

uses
  Math, dxCompsUtile, ZeosDBUtile, CommonDBVar, ConcurentUsersUnit, Variants, rapInclude,
  AtlasUtils, FormulareUnit, AlopAngVizualizare, ATSZDBUtils, Types;
var FormAlop: TFormAlop;


end.
