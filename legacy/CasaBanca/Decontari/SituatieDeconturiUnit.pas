unit SituatieDeconturiUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ExtCtrls, HeadPanel, StdCtrls, CommonCasa, ZAbstractRODataset, ZAbstractDataset,
  cxGraphics, cxControls,
  cxLookAndFeels, cxLookAndFeelPainters, cxContainer, cxEdit, cxTextEdit,
  cxMaskEdit, cxDropDownEdit, cxImageComboBox, Vcl.ComCtrls, dxCore,
  cxDateUtils, cxCalendar, cxCustomData, cxStyles, cxTL, cxTLdxBarBuiltInMenu,
  cxDataControllerConditionalFormattingRulesManagerDialog, cxInplaceContainer,
  cxDBTL, cxTLData, dxExEdtr, dxCntner, Data.DB, ZDataset, dxDBTLCl, dxTL,
  dxDBCtrl, dxDBTL, dxScrollbarAnnotations;

type
  TFrmSituatieDeconturi = class(TForm)
    pnTop: THeadPanel;
    pnContent: TPanel;
    DTDecont: TDataSource;
    QryDecont: TZQuery;
    GridDeconturi: TcxDBTreeList;
    GridDeconturiNIVEL: TcxDBTreeListColumn;
    GridDeconturiCOD_CASA: TcxDBTreeListColumn;
    GridDeconturiDENUMIRE: TcxDBTreeListColumn;
    GridDeconturiEXPLICATIE: TcxDBTreeListColumn;
    GridDeconturiNR_DECONT: TcxDBTreeListColumn;
    GridDeconturiDATA_DECONT: TcxDBTreeListColumn;
    GridDeconturiDATA_OPERATIE: TcxDBTreeListColumn;
    GridDeconturiSUMA_RIDICATA: TcxDBTreeListColumn;
    GridDeconturiSUMA_JUSTIFICATA: TcxDBTreeListColumn;
    GridDeconturiDIFERENTA: TcxDBTreeListColumn;
    GridDeconturiCODGEST: TcxDBTreeListColumn;
    GridDeconturiCOD: TcxDBTreeListColumn;
    GridDeconturiID: TcxDBTreeListColumn;
    GridDeconturiID_PARINTE: TcxDBTreeListColumn;
    pnDef: TPanel;
    Label1: TLabel;
    Label2: TLabel;
    edtDeLa: TcxDateEdit;
    edtPanaLa: TcxDateEdit;
    Label3: TLabel;
    edtCasa: TcxImageComboBox;
    Label4: TLabel;
    edtRepartitor: TcxPopupEdit;
    TreeRepartitori: TdxDBTreeList;
    TreeRepartitoriCONT: TdxDBTreeListMaskColumn;
    TreeRepartitoriNUME: TdxDBTreeListMaskColumn;
    TreeRepartitoriCODSECTIE: TdxDBTreeListMaskColumn;
    TreeRepartitoriADRESA: TdxDBTreeListMaskColumn;
    TreeRepartitoriGESTINT: TdxDBTreeListCheckColumn;
    TreeRepartitoriTIPGEST: TdxDBTreeListMaskColumn;
    procedure FormCreate(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure GridDeconturiGetLevelColor(Sender: TObject; ALevel: Integer;
      var AColor: TColor);
    procedure TreeRepartitoriDblClick(Sender: TObject);
    procedure TreeRepartitoriKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure edtDeLaDateChange(Sender: TObject);
    procedure edtRepartitorPropertiesCloseUp(Sender: TObject);
    procedure edtRepartitorPropertiesInitPopup(Sender: TObject);
  private
    procedure SetColorAndFont(Level: Integer; var aFont: TFont; var aColor: TColor);
    { Private declarations }
  public
    { Public declarations }
    procedure RefreshData(DeLa, PanaLa :TDateTime; CodCasa : Integer; Repartitor : Integer);
  end;

implementation

{$R *.DFM}

uses
  ZeosDBUtile, dxCompsUtile, Variants, DateUnit;

procedure TFrmSituatieDeconturi.RefreshData(DeLa, PanaLa: TDateTime;
  CodCasa, Repartitor: Integer);
begin
 if CodCasa = -1 then Exit;
  with QryDecont do begin
    Close;

    {data_min}
    if Dela < 0 then
      Params.ParamByName('DELA').Value := Null
    else
      Params.ParamByName('DELA').Value := DeLa;
    {data_max}
    if PanaLa < 0 then
      Params.ParamByName('PANALA').Value := Null
    else
      Params.ParamByName('PANALA').Value := PanaLa;
    {cod casa}
    Params.ParamByName('COD_CASA').Value := CodCasa;
    {repartitorul}
    if Repartitor < 0 then
      Params.ParamByName('ID_REPARTITOR').Value := Null
    else
      Params.ParamByName('ID_REPARTITOR').Value := Repartitor;

    Open;
  end;
end;

procedure TFrmSituatieDeconturi.FormCreate(Sender: TObject);
begin
  RefreshData(-1,-1,-1,-1);
  FillImageCombo(edtCasa.Properties, 'SELECT COD_CB, DENUMIRE FROM CASIERIE WHERE ISNULL(IS_AVANS,0)=1', 0, 1);
  ReadSettingsRegistru;
end;

procedure TFrmSituatieDeconturi.edtRepartitorPropertiesCloseUp(Sender: TObject);
var
  lNode: TcxDBTreeListNode;
begin
  if GetParentForm(TreeRepartitori).ModalResult = mrOk then begin
    lNode := TcxDBTreeListNode(TreeRepartitori.FocusedNode);
    if Assigned(lNode) then begin
      edtRepartitor.EditValue := ValueToStr(lNode.KeyValue) + ' : ' + ValueToStr(lNode.Values[TreeRepartitoriNUME.Index]);
      edtRepartitor.Tag       := NativeInt(lNode.KeyValue);
    end;
  end;
  if ValueHasValue(edtCasa.EditValue) then
     RefreshData(edtDeLa.Date,edtPanaLa.Date,-1, TreeRepartitori.Tag)
  else RefreshData(edtDeLa.Date,edtPanaLa.Date,StrToInt(Trim(edtCasa.Text)), TreeRepartitori.Tag);
end;

procedure TFrmSituatieDeconturi.edtRepartitorPropertiesInitPopup(
  Sender: TObject);
begin
  if edtRepartitor.Properties.PopupWidth < edtRepartitor.Width then
    edtRepartitor.Properties.PopupMinWidth := edtRepartitor.Width;
end;

procedure TFrmSituatieDeconturi.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TFrmSituatieDeconturi.SetColorAndFont(Level : Integer; var  aFont : TFont; var  aColor : TColor);
begin
  case Level of
    1 : begin
      SetConstantToFont(aFont, ftDLevel1);
      AColor := clDLevel1;
    end;
    2 : begin
      SetConstantToFont(aFont, ftDLevel2);
      AColor := clDLevel2;
    end;
    3 : begin
      SetConstantToFont(aFont, ftDLevel3A);
      AColor := clDLevel3A;
    end;
    4 : begin
      SetConstantToFont(aFont, ftDLevel4A);
      AColor := clDLevel4A;
    end;
    5 : begin
      SetConstantToFont(aFont, ftDLevel3B);
      AColor := clDLevel3B;
    end;
    6 : begin
      SetConstantToFont(aFont, ftDLevel4B);
      AColor := clDLevel4B;
    end;
  end;
end;

procedure TFrmSituatieDeconturi.GridDeconturiGetLevelColor(Sender: TObject;
  ALevel: Integer; var AColor: TColor);
var aFont : TFont;
begin
  SetColorAndFont(aLevel, aFont, AColor);
end;

procedure TFrmSituatieDeconturi.TreeRepartitoriDblClick(Sender: TObject);
begin
  with TcxDBTreeList(Sender) do
    if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
      GetParentForm(TreeRepartitori).ModalResult := mrOk;
end;

procedure TFrmSituatieDeconturi.TreeRepartitoriKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if ((Key in [VK_UP, VK_DOWN]) and (ssAlt in Shift)) or
      ((Key = VK_F4) and not (ssAlt in Shift)) or (Key = VK_ESCAPE) then
    GetParentForm(TreeRepartitori).ModalResult := mrCancel;
  if (Key = VK_RETURN) and (TreeRepartitori.FocusedNode <> nil)
     and (not TreeRepartitori.FocusedNode.HasChildren) then
    GetParentForm(TreeRepartitori).ModalResult := mrOk;
end;

procedure TFrmSituatieDeconturi.edtDeLaDateChange(Sender: TObject);
begin
  RefreshData(edtDeLa.Date, edtPanaLa.Date, ValueSafeToInt(edtCasa.EditValue), ValueSafeToInt(edtRepartitor.Tag));
end;

end.
 
