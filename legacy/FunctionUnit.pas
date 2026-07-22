unit FunctionUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms,
  Dialogs, ComCtrls, ExtCtrls, dxExEdtr, dxorgchr, dxdborgc, dxCntner,
  dxTL, dxDBCtrl, dxDBTL, dxDBTLCl, dxInspct, dxDBInsp, dxInspRw,
  dxDBInRw, Buttons, Menus, ImgList, DegradePanel,
  cxGraphics, cxControls, cxLookAndFeelPainters,
  cxLookAndFeels, dxorgcedadv, cxCustomData, cxStyles, cxTL, cxMaskEdit,
  cxCalendar, cxCurrencyEdit, cxTLdxBarBuiltInMenu,
  cxDataControllerConditionalFormattingRulesManagerDialog, cxClasses,
  cxInplaceContainer, cxDBTL, cxTLData, cxImageList, cxDBTreeListExUnit, cxEdit,
  cxMemo, cxCheckBox, cxSpinEdit, cxButtonEdit, cxImageComboBox, cxVGrid,
  Variants, cxDBVGrid, dxBarBuiltInMenu, cxPC, Vcl.StdCtrls, cxButtons,
  dxScrollbarAnnotations, Data.DB;

type
  TfrmFunctii = class(TForm)
    PageDeps: TcxPageControl;
    StatusBar: TStatusBar;
    tabTree: TcxTabSheet;
    tabOrganigrama: TcxTabSheet;
    Organigrama: TdxDbOrgChart;
    pnTools: TPanel;
    BtnAddDep: TcxButton;
    BtnAddSubDep: TcxButton;
    BtnDelDepartament: TcxButton;
    BtnUtilizatori: TcxButton;
    ppTipDepartament: TPopupMenu;
    ppDreptunghi: TMenuItem;
    ppRoundedRect: TMenuItem;
    ppEllipse: TMenuItem;
    ppDiamond: TMenuItem;
    Splitter1: TSplitter;
    pnTop: TDegradePanel;
    TreeDepartamenteDENUMIRE: TcxDBTreeListColumn;
    TreeDepartamenteDATA_INTRARE: TcxDBTreeListColumn;
    TreeDepartamenteDATA_IESIRE: TcxDBTreeListColumn;
    TreeDepartamenteTELEFON: TcxDBTreeListColumn;
    TreeDepartamenteSTARE: TcxDBTreeListColumn;
    TreeDepartamente: TcxDBTreeListEx;
    ImaginiFunctii: TcxImageList;
    InspFunctii: TcxDBVerticalGrid;
    InspFunctiiID_FUNCTIUNI: TcxDBEditorRow;
    InspFunctiiID_PARINTE: TcxDBEditorRow;
    InspFunctiiID_UTILIZATORI: TcxDBEditorRow;
    InspFunctiiID_INITIAL: TcxDBEditorRow;
    InspFunctiiCOD_FUNCTIE: TcxDBEditorRow;
    InspFunctiiDENUMIRE: TcxDBEditorRow;
    InspFunctiiATRIBUTII: TcxDBEditorRow;
    InspFunctiiDATA_INTRARE: TcxDBEditorRow;
    InspFunctiiDATA_IESIRE: TcxDBEditorRow;
    InspFunctiiSHAPE_TYPE: TcxDBEditorRow;
    InspFunctiiSHAPE_LEFT_TOP: TcxDBEditorRow;
    InspFunctiiSHAPE_RIGHT_BOTT: TcxDBEditorRow;
    InspFunctiiSHAPE_COLOR: TcxDBEditorRow;
    InspFunctiiSHAPE_FONT_COL: TcxDBEditorRow;
    InspFunctiiSHAPE_FONT_NAME: TcxDBEditorRow;
    InspFunctiiCategoryRow1: TcxCategoryRow;
    InspFunctiiCategoryRow2: TcxCategoryRow;
    InspFunctiiCategoryRow3: TcxCategoryRow;
    InspFunctiiSTARE: TcxDBEditorRow;
    InspFunctiiCategoryRow4: TcxCategoryRow;
    InspFunctiiPOS_ID: TcxDBEditorRow;
    InspFunctiiDESCRIERE: TcxDBEditorRow;
    InspFunctiiDBEditorRow1: TcxDBEditorRow;
    InspFunctiiID_DEPARTAMENTE: TcxDBEditorRow;
    DataSource1: TDataSource;
    procedure FormCreate(Sender: TObject);
    procedure BtnAddDepClick(Sender: TObject);
    procedure BtnAddSubDepClick(Sender: TObject);
    procedure RefreshOrganigrama;
    procedure OrganigramaLoadNode(Sender: TObject; Node: TdxOcNode);
    procedure BtnDelDepartamentClick(Sender: TObject);
    procedure BtnUtilizatoriClick(Sender: TObject);
    procedure InspFunctiiRow24ButtonClick(Sender: TObject;
      AbsoluteIndex: Integer);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure TreeDepartamenteGetNodeImageIndex(Sender: TcxCustomTreeList;
      ANode: TcxTreeListNode; AIndexType: TcxTreeListImageIndexType;
      var AIndex: TImageIndex);
  private
    { Private declarations }
    function GetSelectedFunction: Variant;
  public
    { Public declarations }
  end;

implementation

{$R *.dfm}

uses
  ZeosDBUtile, dxCompsUtile, DateUnit, AlegUtilizatoriUnit;

procedure TfrmFunctii.FormCreate(Sender: TObject);
begin
  Organigrama.ShapeFieldName := 'SHAPE_TYPE';
  Organigrama.ColorFieldName := 'SHAPE_COLOR';
  Organigrama.WidthFieldName := 'SHAPE_LEFT_TOP';
  Organigrama.HeightFieldName := 'SHAPE_RIGHT_BOTT';

  frmData.QryRepartitori.Filtered := True;
  frmData.QryRepartitori.Filter := 'GESTINT = True';
  try
    FillImageCombo(InspFunctiiID_DEPARTAMENTE.Properties.EditProperties, frmData.QryRepartitori, 'ID_REPARTITORI', 'NUME');
  finally
    frmData.QryRepartitori.Filter := '';
    frmData.QryRepartitori.Filtered := False;
  end;
end;

function TfrmFunctii.GetSelectedFunction: Variant;
begin
  if PageDeps.ActivePage = tabTree then
    if Assigned(treeDepartamente.FocusedNode) then
      Result := TcxDBTreeListNode(TreeDepartamente.FocusedNode).KeyValue
    else
      Result := Null
  else
    if Assigned(Organigrama.Selected) then
      Result := TdxDbOcNode(Organigrama.Selected).Key
    else
      Result := Null;
end;

procedure TfrmFunctii.BtnAddDepClick(Sender: TObject);
begin
  { Adaugam o functie noua }
  FrmData.QryFunctiuni.Append;
  FrmData.QryFunctiuni.FieldByName('ID_PARINTE').Clear;
  RefreshOrganigrama;
end;

procedure TfrmFunctii.RefreshOrganigrama;
//var
//  Bookmark : TDataSource;
begin
//  Bookmark := Organigrama.DataSource;
//  Organigrama.DataSource := nil;
//  Organigrama.DataSource := Bookmark;

  Organigrama.DataSource.DataSet.DisableControls;

  if (Organigrama.DataSource.DataSet.State in dsEditModes) then
    Organigrama.DataSource.DataSet.Post;

  Organigrama.DataSource.DataSet.Close;
  Organigrama.DataSource.DataSet.Open;

  Organigrama.DataSource.DataSet.EnableControls;
end;

procedure TfrmFunctii.BtnAddSubDepClick(Sender: TObject);
var
  idParinte : Integer;
begin
  idParinte := GetSelectedFunction;
  {adaugam o functie subordonata}
  FrmData.QryFunctiuni.Append;
  DBSetFieldValue(FrmData.QryFunctiuni, 'ID_PARINTE', idParinte);
  RefreshOrganigrama;
end;

procedure TfrmFunctii.OrganigramaLoadNode(Sender: TObject;
  Node: TdxOcNode);
begin
  Node.ImageAlign := iaTC;
  if Node.Level < 2 then Node.ImageIndex := Node.Level else Node.ImageIndex := 2;
end;

procedure TfrmFunctii.TreeDepartamenteGetNodeImageIndex(
  Sender: TcxCustomTreeList; ANode: TcxTreeListNode;
  AIndexType: TcxTreeListImageIndexType; var AIndex: TImageIndex);
begin
  if AIndexType in [tlitImageIndex, tlitSelectedIndex] then begin
    if ANode = TreeDepartamente.Root then
      AIndex := 0
    else
      if ANode.HasChildren then
        AIndex := 1
      else
        AIndex := 2;
  end;
end;

procedure TfrmFunctii.BtnDelDepartamentClick(Sender: TObject);
begin
  if PageDeps.ActivePage = tabTree then
    TreeDepartamente.DeleteSelection
  else
    Organigrama.Delete(Organigrama.Selected);
  RefreshOrganigrama;
end;

procedure TfrmFunctii.BtnUtilizatoriClick(Sender: TObject);
var
  lFunctie: Variant;
begin
  lFunctie := GetSelectedFunction;
  if ValueHasValue(lFunctie) then
    ModificaUtilizatori(lFunctie);
end;

procedure TfrmFunctii.InspFunctiiRow24ButtonClick(Sender: TObject;
  AbsoluteIndex: Integer);
begin
  BtnUtilizatori.Click;
end;

procedure TfrmFunctii.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  Action := caFree;
end;

end.
