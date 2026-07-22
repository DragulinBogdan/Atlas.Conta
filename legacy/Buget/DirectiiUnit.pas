unit DirectiiUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms,
  Dialogs, ComCtrls, ExtCtrls, dxExEdtr, dxorgchr, dxdborgc, dxCntner,
  dxTL, dxDBCtrl, dxInspct, dxDBInsp, dxInspRw,
  dxDBInRw, Buttons, Menus, ImgList, 
  dxEditor, dxEdLib, StdCtrls, dxDBELib, DB,
  DegradePanel, dxDBTLCl, dxDBTL,
  cxGraphics, cxControls, cxLookAndFeelPainters,
  cxLookAndFeels, cxStyles, cxEdit, cxMaskEdit, cxCalendar, cxMemo, cxCheckBox,
  cxSpinEdit, cxImageComboBox, cxVGrid, cxDBVGrid, cxInplaceContainer,
  dxorgcedadv, dxScrollbarAnnotations, ZAbstractRODataset, ZAbstractDataset,
  ZDataset, cxContainer, cxButtons, cxGroupBox, cxTextEdit, cxDropDownEdit,
  cxDBEdit;

type
  TfrmBugetDirectii = class(TForm)
    PageDeps: TPageControl;
    tabTree: TTabSheet;
    tabOrganigrama: TTabSheet;
    TreeDepartamente: TdxDBTreeList;
    Organigrama: TdxDbOrgChart;
    TreeDepartamenteDENUMIRE: TdxDBTreeListMaskColumn;
    TreeDepartamenteSTARE: TdxDBTreeListMaskColumn;
    pnTools: TPanel;
    BtnAddDir: TSpeedButton;
    BtnAddSubDep: TSpeedButton;
    BtnDelDepartament: TSpeedButton;
    ppTipDepartament: TPopupMenu;
    ppDreptunghi: TMenuItem;
    ppRoundedRect: TMenuItem;
    ppEllipse: TMenuItem;
    ppDiamond: TMenuItem;
    TreeDepartamenteDATA_IESIRE: TdxDBTreeListDateColumn;
    TreeDepartamenteDATA_INTRARE: TdxDBTreeListDateColumn;
    Splitter1: TSplitter;
    Imagini: TImageList;
    pnRight: TPanel;
    pnTopRight: TPanel;
    Splitter2: TSplitter;
    SpeedButton1: TSpeedButton;
    Label1: TLabel;
    ieOrdonantatori: TcxDBImageComboBox;
    pnTop: TDegradePanel;
    InspFunctii: TcxDBVerticalGrid;
    InspFunctiiID_BUGET_DIRECTII: TcxDBEditorRow;
    InspFunctiiID_PARINTE: TcxDBEditorRow;
    InspFunctiiID_UTILIZATORI: TcxDBEditorRow;
    InspFunctiiDENUMIRE: TcxDBEditorRow;
    InspFunctiiATRIBUTII: TcxDBEditorRow;
    InspFunctiiDATA_START_FUNCTIONARE: TcxDBEditorRow;
    InspFunctiiDATA_STOP_FUNCTIONARE: TcxDBEditorRow;
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
    InspFunctiiID_BUGET_TIP_ORDONATOR: TcxDBEditorRow;
    inspOrdonantatori: TcxDBVerticalGrid;
    inspOrdonantatoriNUME: TcxDBEditorRow;
    inspOrdonantatoriPRENUME: TcxDBEditorRow;
    inspOrdonantatoriDATA: TcxDBEditorRow;
    inspOrdonantatoriTIP: TcxDBEditorRow;
    QryBugetDirectii: TZQuery;
    DTBugetDirectii: TDataSource;
    dtBugetOrdonantatori: TDataSource;
    QryBugetOrdonantatori: TZQuery;
    pnBotomSelect: TcxGroupBox;
    btnOkSelect: TcxButton;
    btnCancelSelect: TcxButton;
    procedure FormCreate(Sender: TObject);
    procedure BtnAddDirClick(Sender: TObject);
    procedure BtnAddSubDepClick(Sender: TObject);
    procedure TreeDepartamenteGetImageIndex(Sender: TObject;
      Node: TdxTreeListNode; var Index: Integer);
    procedure OrganigramaLoadNode(Sender: TObject; Node: TdxOcNode);
    procedure TreeDepartamenteGetSelectedIndex(Sender: TObject;
      Node: TdxTreeListNode; var Index: Integer);
    procedure BtnDelDepartamentClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure ieOrdonantatoriChange(Sender: TObject);
    procedure inspOrdonantatoriExit(Sender: TObject);
    procedure inspOrdonantatoriChangeNode(Sender: TObject; OldNode, Node: TdxInspectorNode);
    procedure SpeedButton1Click(Sender: TObject);
    procedure QryBugetDirectiiNewRecord(DataSet: TDataSet);
    procedure QryBugetDirectiiAfterScroll(DataSet: TDataSet);
    procedure QryBugetOrdonantatoriAfterPost(DataSet: TDataSet);
    procedure btnOkSelectClick(Sender: TObject);
    procedure btnCancelSelectClick(Sender: TObject);
  private
    { Private declarations }
    procedure AddOrdonantatori(const Id: Variant; const ANume, APrenume: String);
  public
    { Public declarations }
  end;

implementation

{$R *.dfm}

uses
  ZeosDBUtile, dxCompsUtile, Variants;

procedure TfrmBugetDirectii.FormCreate(Sender: TObject);
begin
  Organigrama.ShapeFieldName := 'SHAPE_TYPE';
  Organigrama.ColorFieldName := 'SHAPE_COLOR';
  Organigrama.WidthFieldName := 'SHAPE_LEFT_TOP';
  Organigrama.HeightFieldName := 'SHAPE_RIGHT_BOTT';
  FillImageCombo(ieOrdonantatori.Properties, 'spNmclOrdonantatori', 0, 1);
  FillImageCombo(inspOrdonantatoriTIP.Properties.EditProperties, 'spNmclTipOrdonantator', 0, 1);

  QryBugetDirectii.Open;
  QryBugetOrdonantatori.Open;
end;

procedure TfrmBugetDirectii.AddOrdonantatori(const Id: Variant; const ANume, APrenume: String);
var
  lItem: TcxImageComboBoxItem;
begin
  lItem := ieOrdonantatori.Properties.Items.Add;
  lItem.Value := QryBugetOrdonantatori['ID_BUGET_ORDONANTATORI'];
  lItem.Description := QryBugetOrdonantatori['Nume'] + ' '  + QryBugetOrdonantatori['Prenume'];
end;

procedure TfrmBugetDirectii.BtnAddDirClick(Sender: TObject);
begin
  { Adaugam o functie noua }
  QryBugetDirectii.Append;
  if not (QryBugetDirectii.State in dsEditModes) then
    QryBugetDirectii.Edit;
  QryBugetDirectii.FieldByName('ID_PARINTE').Clear;

  DBPost(QryBugetDirectii);
end;

procedure TfrmBugetDirectii.BtnAddSubDepClick(Sender: TObject);
var
  ParentId: Variant;
begin
  { Adaugam o subfunctie noua in contextul curent
    Daca este organigrama sau tree }
  if PageDeps.ActivePage = tabTree then begin
     if TreeDepartamente.FocusedNode <> nil then ParentId := TdxDBTreeListNode(TreeDepartamente.FocusedNode).Id
     else ParentId := Null;
  end
  else if Organigrama.Selected <> nil then ParentId := TdxDbOcNode(Organigrama.Selected).Key
       else ParentId := Null;
  QryBugetDirectii.Append;
  QryBugetDirectii.FieldByName('ID_PARINTE').AsInteger := ParentId;
  DBPost(QryBugetDirectii);
end;

procedure TfrmBugetDirectii.btnCancelSelectClick(Sender: TObject);
begin
  ModalResult := mrCancel;
  Close;
end;

procedure TfrmBugetDirectii.TreeDepartamenteGetImageIndex(Sender: TObject;
  Node: TdxTreeListNode; var Index: Integer);
begin
  if Node.Level < 2 then Index := Node.Level else Index := 2;
end;

procedure TfrmBugetDirectii.OrganigramaLoadNode(Sender: TObject; Node: TdxOcNode);
begin
  Node.ImageAlign := iaTC;
  if Node.Level < 2 then Node.ImageIndex := Node.Level else Node.ImageIndex := 2;
end;

procedure TfrmBugetDirectii.TreeDepartamenteGetSelectedIndex(Sender: TObject;
  Node: TdxTreeListNode; var Index: Integer);
begin
  Index := Node.ImageIndex;
end;

procedure TfrmBugetDirectii.BtnDelDepartamentClick(Sender: TObject);
begin
  if PageDeps.ActivePage = tabTree then begin
     if TreeDepartamente.FocusedNode <> nil then TdxDBTreeListNode(TreeDepartamente.FocusedNode).Delete;
  end
  else if Organigrama.Selected <> nil then Organigrama.Delete(Organigrama.Selected);
end;

procedure TfrmBugetDirectii.btnOkSelectClick(Sender: TObject);
begin
  DBPost(QryBugetDirectii);
  ModalResult := mrOk;
  Close;
end;

procedure TfrmBugetDirectii.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmBugetDirectii.QryBugetDirectiiAfterScroll(DataSet: TDataSet);
begin
  inspOrdonantatori.Enabled := ValueHasValue(QryBugetDirectii['ID_BUGET_ORDONANTATORI']);
end;

procedure TfrmBugetDirectii.QryBugetDirectiiNewRecord(DataSet: TDataSet);
begin
  DataSet.FieldByName('DENUMIRE').AsString    := 'Directie noua';
  DataSet.FieldByName('STARE').AsBoolean      := True;
end;

procedure TfrmBugetDirectii.QryBugetOrdonantatoriAfterPost(DataSet: TDataSet);
begin
  AddOrdonantatori(QryBugetOrdonantatori['ID_BUGET_ORDONANTATORI'], QryBugetOrdonantatori['Nume'], QryBugetOrdonantatori['Prenume']);
  ieOrdonantatori.EditValue := QryBugetOrdonantatori['ID_BUGET_ORDONANTATORI'];
end;

procedure TfrmBugetDirectii.ieOrdonantatoriChange(Sender: TObject);
begin
  DBPost(QryBugetDirectii);
end;

procedure TfrmBugetDirectii.inspOrdonantatoriExit(Sender: TObject);
begin
  DBPost(QryBugetOrdonantatori);
end;

procedure TfrmBugetDirectii.inspOrdonantatoriChangeNode(Sender: TObject;
  OldNode, Node: TdxInspectorNode);
begin
  DBPost(QryBugetOrdonantatori);
end;

procedure TfrmBugetDirectii.SpeedButton1Click(Sender: TObject);
var
  lId: Variant;
begin
  lId := DBGetScallarFmt('insert into buget_ordonantatori (Nume, Prenume) values(%s, %s); select scope_identity();',
        [ValueToStr('Ordonantator'), ValueToStr('Nou')]);
  DBSetFieldValue(qryBugetDirectii, 'ID_BUGET_ORDONANTATORI', lId);
  AddOrdonantatori(lId, 'Ordonantator', 'Nou');
  ieOrdonantatori.EditValue := lId;
  inspOrdonantatori.SetFocus;
end;

end.
