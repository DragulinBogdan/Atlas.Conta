unit UrmarireExecutieUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  Db, ZDataSet, ExtCtrls, dxCntner, dxInspct, dxDBInsp, dxTL, dxDBCtrl,
  dxDBTL, StdCtrls, dxEditor, dxExEdtr, dxEdLib, dxInspRw,
  dxDBInRw, dxDBTLCl, dxfCheckBox, Menus, ZAbstractRODataset, ZAbstractDataset,
  cxGraphics, cxControls, cxLookAndFeels, cxLookAndFeelPainters, cxContainer,
  cxEdit, cxCheckBox, cxTextEdit, cxMaskEdit, cxDropDownEdit, cxCalendar, cxImageComboBox,
  Vcl.ComCtrls, dxCore, cxDateUtils, cxSplitter, cxStyles, cxInplaceContainer,
  cxVGrid, cxDBVGrid, cxCurrencyEdit, cxCustomData, cxTL, cxTLdxBarBuiltInMenu,
  cxDBTL, cxTLData, cxDataControllerConditionalFormattingRulesManagerDialog;

type
  TfrmUrmarireExecutie = class(TForm)
    DTMaster: TDataSource;
    DTChild: TDataSource;
    QryMaster: TZQuery;
    QryChild: TZQuery;
    pnTools: TPanel;
    pnBottom: TPanel;
    pnClient: TPanel;
    splitterV: TcxSplitter;
    pnBugetare: TPanel;
    BugetMaster: TPanel;
    splitterH: TcxSplitter;
    pnBugetChild: TPanel;
    TreeMaster: TcxDBTreeList;
    TreeChild: TcxDBTreeList;
    LbTipClasificatie: TLabel;
    LbDataRaportare: TLabel;
    edDataRaportare: TcxDateEdit;
    LbClasificatie: TLabel;
    TreeMasterDENUMIRE: TcxDBTreeListColumn;
    TreeMasterASIGNAT: TcxDBTreeListColumn;
    TreeChildDENUMIRE: TcxDBTreeListColumn;
    TreeChildASIGNAT: TcxDBTreeListColumn;
    TreeMasterCOD_ECONOMIC: TcxDBTreeListColumn;
    TreeMasterCOD_FUNCTIONAL: TcxDBTreeListColumn;
    ChkArataFaraPlanificare: TcxCheckBox;
    TreeMasterCAPTURA: TcxDBTreeListColumn;
    TreeChildCAPTURA: TcxDBTreeListColumn;
    TreeChildCOD_ECONOMIC: TcxDBTreeListColumn;
    TreeChildCOD_FUNCTIONAL: TcxDBTreeListColumn;
    ppFisaBugetara: TPopupMenu;
    ppFisaBugetaraLaZi: TMenuItem;
    N1: TMenuItem;
    ppFisaBugetaraTrim1: TMenuItem;
    ppFisaBugetaraTrim2: TMenuItem;
    ppFisaBugetaraTrim3: TMenuItem;
    ppFisaBugetaraTrim4: TMenuItem;
    lbRevizie: TLabel;
    TreeMasterPLANIFICAT: TcxDBTreeListColumn;
    TreeMasterANGAJAT: TcxDBTreeListColumn;
    TreeMasterCONSUM: TcxDBTreeListColumn;
    TreeMasterPLATIT: TcxDBTreeListColumn;
    TreeMasterDISPONIBIL: TcxDBTreeListColumn;
    TreeMasterPROC_ANGAJAT: TcxDBTreeListColumn;
    TreeMasterPROC_PLATIT: TcxDBTreeListColumn;
    TreeMasterPROC_CONSUM: TcxDBTreeListColumn;
    TreeMasterRAMAS_DE_REALIZAT: TcxDBTreeListColumn;
    TreeChildPLANIFICAT: TcxDBTreeListColumn;
    TreeChildANGAJAT: TcxDBTreeListColumn;
    TreeChildCONSUM: TcxDBTreeListColumn;
    TreeChildPLATIT: TcxDBTreeListColumn;
    TreeChildDISPONIBIL: TcxDBTreeListColumn;
    TreeChildPROC_ANGAJAT: TcxDBTreeListColumn;
    TreeChildPROC_PLATIT: TcxDBTreeListColumn;
    TreeChildPROC_CONSUM: TcxDBTreeListColumn;
    TreeChildRAMAS_DE_REALIZAT: TcxDBTreeListColumn;
    lbGestiune: TLabel;
    edTipClasificatie: TcxImageComboBox;
    edClasificatie: TcxImageComboBox;
    edRevizie: TcxImageComboBox;
    edGestiune: TcxImageComboBox;
    vBuget: TcxDBVerticalGrid;
    vBugetCOD_ECONOMIC: TcxDBEditorRow;
    vBugetCOD_FUNCTIONAL: TcxDBEditorRow;
    vBugetDENUMIRE: TcxDBEditorRow;
    vBugetAN_FISCAL: TcxDBEditorRow;
    vBugetCategoryRow1: TcxCategoryRow;
    vBugetDBEditorRow1: TcxDBEditorRow;
    vBugetDBEditorRow2: TcxDBEditorRow;
    vBugetDBEditorRow3: TcxDBEditorRow;
    vBugetDBEditorRow4: TcxDBEditorRow;
    vBugetCategoryRow2: TcxCategoryRow;
    vBugetDBEditorRow5: TcxDBEditorRow;
    vBugetDBEditorRow6: TcxDBEditorRow;
    vBugetDBEditorRow7: TcxDBEditorRow;
    vBugetDBEditorRow8: TcxDBEditorRow;
    vBugetDBEditorRow9: TcxDBEditorRow;
    vBugetDBEditorRow10: TcxDBEditorRow;
    vBugetPLANIFICAT: TcxDBEditorRow;
    vBugetANGAJAT: TcxDBEditorRow;
    vBugetANGAJAT1: TcxDBEditorRow;
    vBugetPLANIFICAT1: TcxDBEditorRow;
    vBugetPLANIFICAT2: TcxDBEditorRow;
    vBugetANGAJAT2: TcxDBEditorRow;
    vBugetANGAJAT3: TcxDBEditorRow;
    vBugetPLANIFICAT3: TcxDBEditorRow;
    vBugetPLANIFICAT4: TcxDBEditorRow;
    vBugetANGAJAT4: TcxDBEditorRow;
    vBugetPLATIT: TcxDBEditorRow;
    vBugetPLANIFICAT5: TcxDBEditorRow;
    vBugetPLATIT1: TcxDBEditorRow;
    vBugetPLANIFICAT11: TcxDBEditorRow;
    vBugetPLATIT2: TcxDBEditorRow;
    vBugetPLANIFICAT21: TcxDBEditorRow;
    vBugetPLATIT3: TcxDBEditorRow;
    vBugetPLANIFICAT31: TcxDBEditorRow;
    vBugetPLATIT4: TcxDBEditorRow;
    vBugetPLANIFICAT41: TcxDBEditorRow;
    procedure FormCreate(Sender: TObject);
    procedure ChkArataFaraPlanificareClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure ppFisaBugetaraLaZiClick(Sender: TObject);
    procedure ppFisaBugetaraTrim1Click(Sender: TObject);
    procedure ppFisaBugetaraTrim2Click(Sender: TObject);
    procedure ppFisaBugetaraTrim3Click(Sender: TObject);
    procedure ppFisaBugetaraTrim4Click(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure edDataRaportarePropertiesChange(Sender: TObject);
    procedure edTipClasificatiePropertiesChange(Sender: TObject);
    procedure edReviziePropertiesChange(Sender: TObject);
    procedure edGestiunePropertiesChange(Sender: TObject);
    procedure TreeMasterFocusedNodeChanged(Sender: TcxCustomTreeList;
      APrevFocusedNode, AFocusedNode: TcxTreeListNode);
  private
    { Private declarations }
    procedure RefreshDataSet;
    procedure FisaBugetara(const ADate: TDateTime);
  public
    { Public declarations }
  end;

implementation

{$R *.DFM}

uses
  dxCompsUtile,
  ZeosDBUtile,
  DateUnit,
  rapInclude,
  SetParamsUnitADO,
  dxGridRefresher,
  CommonDBVar,
  Variants;

procedure TfrmUrmarireExecutie.RefreshDataSet;
begin
  { Recitim datele in functie de ce este selectat }
  QryMaster.ParamByName('FUNCTIONAL').Value := edTipClasificatie.EditValue;
  QryMaster.ParamByName('COD_BUGET').Value  := edClasificatie.EditValue;
  QryMaster.ParamByName('DATA').Value       := edDataRaportare.Date;
  QryMaster.ParamByName('DIVIZOR').Value    := 1;
  QryMaster.ParamByName('REVIZIE').Value    := edRevizie.EditValue;
  QryMaster.ParamByName('GESTIUNE').Value   := edGestiune.EditValue;
  DBRefresh(QryMaster);
end;

procedure TfrmUrmarireExecutie.FormCreate(Sender: TObject);
begin
  if not DBProcExists('SP_GET_LIST_EXECUTIE_GESTIUNI') then begin
    lbGestiune.Visible := False;
    edGestiune.Visible := False;
  end
  else begin
    QryMaster.SQL.Text := 'EXEC SP_BUGET_SITUATIE_EXECUTIE :COD_BUGET, '''', :DATA, :FUNCTIONAL, :DIVIZOR, :REVIZIE, :GESTIUNE';
    QryChild.SQL.Text  := 'EXEC SP_BUGET_SITUATIE_EXECUTIE :COD_BUGET, :RADACINA, :DATA, :FUNCTIONAL, :DIVIZOR, :REVIZIE, :GESTIUNE';
    FillImageCombo(edGestiune.Properties, 'exec [SP_GET_LIST_EXECUTIE_GESTIUNI]', 'ID', 'DENUMIRE', Null, '--Toate Gestiuniile--');
  end;
  FillImageCombo(edRevizie.Properties, 'exec [spAlopListaRevizie]', 'revizie', 'denRevizie');
  edDataRaportare.EditValue := Date;
  if edRevizie.Properties.Items.Count > 0 then
    edRevizie.EditValue     := edRevizie.Properties.Items[edRevizie.Properties.Items.Count-1].Value;
  edTipClasificatiePropertiesChange(edTipClasificatie);
end;

procedure TfrmUrmarireExecutie.TreeMasterFocusedNodeChanged(
  Sender: TcxCustomTreeList; APrevFocusedNode, AFocusedNode: TcxTreeListNode);
begin
  if Assigned(AFocusedNode) then begin
    QryChild.ParamByName('FUNCTIONAL').Value := 1 - edTipClasificatie.EditValue;
    if edTipClasificatie.EditValue = 1 then
      QryChild.ParamByName('COD_BUGET').Value := AFocusedNode.Values[TreeChildCOD_ECONOMIC.ItemIndex]
    else
      QryChild.ParamByName('COD_BUGET').Value := AFocusedNode.Values[TreeChildCOD_FUNCTIONAL.ItemIndex];
    QryChild.ParamByName('DATA').Value        := edDataRaportare.EditValue;
    QryChild.ParamByName('RADACINA').Value    := edClasificatie.EditValue;
    QryChild.ParamByName('DIVIZOR').Value     := 1;
    QryChild.ParamByName('REVIZIE').Value     := edRevizie.EditValue;
    QryChild.ParamByName('GESTIUNE').Value    := edGestiune.EditValue;
    DBRefresh(QryChild);
  end;
end;

procedure TfrmUrmarireExecutie.ChkArataFaraPlanificareClick(
  Sender: TObject);
begin
  if ChkArataFaraPlanificare.Checked then begin
     QryMaster.Filter := '';
     QryChild.Filter  := '';
  end
  else begin
     QryMaster.Filter := 'ASIGNAT=1';
     QryChild.Filter  := 'ASIGNAT=1';
  end;
  QryMaster.Filtered := QryMaster.Filter <> '';
  QryChild.Filtered := QryChild.Filter <> '';
end;

procedure TfrmUrmarireExecutie.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmUrmarireExecutie.ppFisaBugetaraLaZiClick(Sender: TObject);
begin
  FisaBugetara(edDataRaportare.Date);
end;

procedure TfrmUrmarireExecutie.FisaBugetara(const ADate: TDateTime);

  procedure SetParamValue(const AParamName: String; const AParamValue: Variant);
  var
    lParam : TAdoCRParam;
  begin
    lParam := RegisterCRAdoParam(AParamName);
    lParam.IsTemporal := True;
    lParam.Value      := AParamValue;
  end;

var
  lNode: TcxDBTreeListNode;
begin
  lNode := (TreeChild.FocusedNode as TcxDBTreeListNode);
  if Assigned(lNode) then begin
    SetParamValue('COD_ECONOMIC'  , lNode.Values[TreeChildCOD_ECONOMIC.ItemIndex]);
    SetParamValue('COD_FUNCTIONAL', lNode.Values[TreeChildCOD_FUNCTIONAL.ItemIndex]);
    SetParamValue('DATA'          , edDataRaportare.EditValue);
    SetParamValue('REVIZIE'       , edRevizie.EditValue);
    if ValueHasValue(edGestiune.EditValue) then
      SetParamValue('LST_TABLEID_REPARTITORI@VW_GESTIUNI@NUME', edGestiune.EditValue);
    LoadReport(DateUnit.GetItemId('FisaBugetara'));
  end;
end;

procedure TfrmUrmarireExecutie.ppFisaBugetaraTrim1Click(Sender: TObject);
var lYear, lMonth, lDay: Word;
begin
  DecodeDate(edDataRaportare.Date, lYear, lMonth, lDay);
  FisaBugetara(EncodeDate(lYear, 01, 01));
end;

procedure TfrmUrmarireExecutie.ppFisaBugetaraTrim2Click(Sender: TObject);
var lYear, lMonth, lDay: Word;
begin
  DecodeDate(edDataRaportare.Date, lYear, lMonth, lDay);
  FisaBugetara(EncodeDate(lYear, 04, 01));
end;

procedure TfrmUrmarireExecutie.ppFisaBugetaraTrim3Click(Sender: TObject);
var lYear, lMonth, lDay: Word;
begin
  DecodeDate(edDataRaportare.Date, lYear, lMonth, lDay);
  FisaBugetara(EncodeDate(lYear, 07, 01));
end;

procedure TfrmUrmarireExecutie.ppFisaBugetaraTrim4Click(Sender: TObject);
var lYear, lMonth, lDay: Word;
begin
  DecodeDate(edDataRaportare.Date, lYear, lMonth, lDay);
  FisaBugetara(EncodeDate(lYear, 10, 01));
end;

procedure TfrmUrmarireExecutie.FormDestroy(Sender: TObject);
var
  lString : String;
begin
  lString := '';
  RegisterCRAdoParam('COD_ECONOMIC', lString);
  RegisterCRAdoParam('COD_FUNCTIONAL', lString);
end;

procedure TfrmUrmarireExecutie.edDataRaportarePropertiesChange(
  Sender: TObject);
begin
  RefreshDataSet;
end;

procedure TfrmUrmarireExecutie.edTipClasificatiePropertiesChange(
  Sender: TObject);
begin
  if edTipClasificatie.EditValue = 0 then
    FillImageComboFmt(edClasificatie.Properties  , 'exec [spAlopListaCFExecutie] %d, %d' , [IdLogin, IdUtilizator], 'cod_functional' , 'denumire', Null, 'Toate Clasificatiile Functionale')
  else
    FillImageComboFmt(edClasificatie.Properties  , 'exec [spAlopListaCEExecutie] %d, %d' , [IdLogin, IdUtilizator], 'cod_economic' , 'denumire', Null, 'Toate Clasificatiile Economice');
  RefreshDataSet;
end;

procedure TfrmUrmarireExecutie.edReviziePropertiesChange(Sender: TObject);
begin
  RefreshDataSet;
end;

procedure TfrmUrmarireExecutie.edGestiunePropertiesChange(Sender: TObject);
begin
  RefreshDataSet;
end;

end.
