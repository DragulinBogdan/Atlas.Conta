unit Unit1;

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

type
  TFormAlop = class(TForm)
    pnDocument: TPanel;
    lbDocument: TLabel;
    LbDataNota: TLabel;
    LbPredator: TLabel;
    LbPrimitor: TLabel;
    Label1: TLabel;
    Label2: TLabel;
    lbContract: TLabel;
    Label3: TLabel;
    LbScopul: TLabel;
    Label4: TLabel;
    lbLegal: TLabel;
    lbNumar: TLabel;
    edtDetaliiContract: TcxPopupEdit;
    edPredator: TcxPopupEdit;
    edPrimitor: TcxPopupEdit;
    edDataDoc: TcxDateEdit;
    edTipAngajament: TcxImageComboBox;
    edNumarDoc: TcxButtonEdit;
    edRectificat: TcxButtonEdit;
    cxFunctionalBar: TcxPopupEdit;
    edNrProiect: TcxButtonEdit;
    edtScop: TcxTextEdit;
    edLegal: TcxButtonEdit;
    btnAdaugaPozitii: TcxButton;
    cxButton2: TcxButton;
    edSumaProiect: TcxCurrencyEdit;
    lbSumaProiect: TcxLabel;
    chkRectificareSold: TcxCheckBox;
    edtDetaliiDosar: TcxPopupEdit;
  private
    { Private declarations }
  public
    { Public declarations }
  end;

var
  FormAlop: TFormAlop;

implementation

{$R *.dfm}

end.
