unit fmPlataDocumUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGrid,
  cxGraphics, cxControls, cxLookAndFeels, cxLookAndFeelPainters,
  cxCustomData, cxStyles, cxTL, cxMaskEdit, cxTLdxBarBuiltInMenu,
  cxContainer, cxEdit, Menus, cxFilter, cxData, cxDataStorage, DB,
  cxDBData, ImgList, ZAbstractRODataset, ZAbstractDataset, ZDataset,
  cxGridLevel, cxClasses, cxGridCustomView, cxButtonEdit, cxDBEdit,
  cxVGrid, cxDBVGrid, StdCtrls, cxButtons, cxRepartitorPanel,
  cxDropDownEdit, cxCalendar, cxTextEdit, cxImageComboBox, cxCheckBox,
  cxGroupBox, ExtCtrls, dxNavBarCollns, dxNavBarBase, dxNavBar,
  cxInplaceContainer, cxDBTL, cxTLData, cxCurrencyEdit, cxNavigator,
  Vcl.ComCtrls, dxCore, cxDateUtils,
  cxDataControllerConditionalFormattingRulesManagerDialog,
  dxScrollbarAnnotations, dxDateRanges;

type
  TfmPlataDocum = class(TForm)
    grDocumentPlata: TcxGroupBox;
    edTipDoc: TcxImageComboBox;
    edNrDoc: TcxTextEdit;
    edDataPlata: TcxDateEdit;
    lbTipDoc: TLabel;
    lnNrDoc: TLabel;
    lbDataDoc: TLabel;
    DTStructure: TDataSource;
    QryStructure: TZReadOnlyQuery;
    ImagesStructura: TImageList;
    TreeStructura: TcxDBTreeList;
    TreeStructuraCOD_CB: TcxDBTreeListColumn;
    TreeStructuraCOD_PARINTE: TcxDBTreeListColumn;
    TreeStructuraDENUMIRE: TcxDBTreeListColumn;
    TreeStructuraDENV: TcxDBTreeListColumn;
    TreeStructuraC_O: TcxDBTreeListColumn;
    TreeStructuraDATA_SOLD: TcxDBTreeListColumn;
    TreeStructuraCASIER: TcxDBTreeListColumn;
    TreeStructuraVALIDATOR: TcxDBTreeListColumn;
    TreeStructuraADMIN: TcxDBTreeListColumn;
    TreeStructuraIS_BANCA: TcxDBTreeListColumn;
    TreeStructuraIS_AVANS: TcxDBTreeListColumn;
    TreeStructuraIS_TEMPOR: TcxDBTreeListColumn;
    TreeStructuraID_REPARTITORI: TcxDBTreeListColumn;
    TreeStructuraICON: TcxDBTreeListColumn;
    TreeStructuraID_VALUTA: TcxDBTreeListColumn;
    TreeStructuraCRSP_LEI: TcxDBTreeListColumn;
    TreeStructuraDESCRIERE: TcxDBTreeListColumn;
    RPCasaBanca: TcxRepartitorPanel;
    lbSumaDocument: TLabel;
    edSuma: TcxCurrencyEdit;
    lbTipValuta: TLabel;
    edTipValuta: TcxImageComboBox;
    viewPlatiAnterioare: TcxGridDBTableView;
    nivelPlatiAnterioare: TcxGridLevel;
    gridPlatiAnterioare: TcxGrid;
    procedure FormCreate(Sender: TObject);
    procedure RPCasaBancaPopupInitPopup(Sender: TObject);
  private
    FIdGestDocum: Integer;
    { Private declarations }
  public
    { Public declarations }
    property IdGestDocum : Integer read FIdGestDocum write FIdGestDocum;
  end;

function ModificaPlataDocument(AIDGestDocum: Integer): Boolean;

implementation

uses
  ZeosDBUtile, dxCompsUtile, cxEditDBRegisteredRepositoryItems, DateUnit, CommonDBVar, Variants, MainUnit;

{$R *.DFM}

function ModificaPlataDocument(AIDGestDocum: Integer): Boolean;
var
  lfmPlataDocum: TfmPlataDocum;
begin
  lfmPlataDocum := TfmPlataDocum.Create(Application);
  try
    lfmPlataDocum.IdGestDocum := AIDGestDocum;
    Result := lfmPlataDocum.ShowModal = mrOk;
  finally
    lfmPlataDocum.Free;
  end;
end;

procedure TfmPlataDocum.FormCreate(Sender: TObject);
begin
  DBRefreshParams(QryStructure, ['COD_UTILIZATOR', 'IS_ADMIN', 'DISP_WAY'], [IdUtilizator, bIsAdmin, 0], True);
  FillImageCombo(edTipDoc.Properties, frmData.QryTipDoc, 'TIP_DOC', 'DENUMIRE');
  FillImageCombo(edTipValuta.Properties, 'spNmclValute', 0, 1);
end;

procedure TfmPlataDocum.RPCasaBancaPopupInitPopup(Sender: TObject);
begin
  with TcxPopupEdit(Sender).Properties do
    if PopupWidth < TcxPopupEdit(Sender).Width then PopupWidth := TcxPopupEdit(Sender).Width;
end;

end.
