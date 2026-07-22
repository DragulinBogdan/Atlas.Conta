unit OI_UnitatiUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms,
  Dialogs, ComCtrls, ExtCtrls, dxExEdtr, dxorgchr, dxdborgc, dxCntner,
  dxTL, dxDBCtrl, dxDBCtrl, dxDBTLCl, dxInspct, dxDBInsp, dxInspRw,
  dxDBInRw, Buttons, Menus, ImgList, JvExControls, JvComponent, JvGradientHeaderPanel,
  Grids, DBGrids, dxEditor, dxEdLib, StdCtrls, dxDBELib, DB,
  cxLookAndFeelPainters, cxButtons, 
  cxPC, cxControls, cxSplitter;


const
  WM_SYNC_UNIQUE = WM_USER + 1;

type
  TfrmOIUnitati = class(TForm)
    PageDeps: TcxPageControl;
    StatusBar: TStatusBar;
    tabTree: TcxTabSheet;
    tabOrganigrama: TcxTabSheet;
    TreeDepartamente: TdxDBTreeList;
    Organigrama: TdxDbOrgChart;
    TreeDepartamenteDENUMIRE: TdxDBTreeListMaskColumn;
    pnTools: TPanel;
    BtnAddDir: TcxButton;
    BtnAddSubDep: TcxButton;
    BtnDelDepartament: TcxButton;
    ppTipDepartament: TPopupMenu;
    ppDreptunghi: TMenuItem;
    ppRoundedRect: TMenuItem;
    ppEllipse: TMenuItem;
    ppDiamond: TMenuItem;
    Imagini: TImageList;
    pnRight: TPanel;
    InspFunctii: TdxDBInspector;
    InspFunctiiID_FUNCTIUNI: TdxInspectorDBMaskRow;
    InspFunctiiID_PARINTE: TdxInspectorDBMaskRow;
    InspFunctiiID_UTILIZATORI: TdxInspectorDBMaskRow;
    InspFunctiiDENUMIRE: TdxInspectorDBMaskRow;
    InspFunctiiATRIBUTII: TdxInspectorDBMemoRow;
    InspFunctiiSHAPE_TYPE: TdxInspectorDBMaskRow;
    InspFunctiiSHAPE_LEFT_TOP: TdxInspectorDBMaskRow;
    InspFunctiiSHAPE_RIGHT_BOTT: TdxInspectorDBMaskRow;
    InspFunctiiSHAPE_COLOR: TdxInspectorDBMaskRow;
    InspFunctiiSHAPE_FONT_COL: TdxInspectorDBMaskRow;
    InspFunctiiSHAPE_FONT_NAME: TdxInspectorDBMaskRow;
    InspFunctiiRow20: TdxInspectorDBRow;
    InspFunctiiRow21: TdxInspectorDBRow;
    InspFunctiiRow22: TdxInspectorDBRow;
    InspFunctiiRow23: TdxInspectorDBRow;
    InspFunctiiPOS_ID: TdxInspectorDBSpinRow;
    InspFunctiiDESCRIERE: TdxInspectorDBMemoRow;
    InspFunctiiTIP_ORDONATOR: TdxInspectorDBImageRow;
    TreeDepartamenteID_OI_UNITATI: TdxDBTreeListMaskColumn;
    TreeDepartamenteID_PARINTE: TdxDBTreeListMaskColumn;
    TreeDepartamenteUNITATATEA_URMARITA: TdxDBTreeListCheckColumn;
    TreeDepartamenteARE_CONT: TdxDBTreeListCheckColumn;
    TreeDepartamenteARE_CONTABILITATE: TdxDBTreeListCheckColumn;
    TreeDepartamenteNUME_ORDONANTATOR: TdxDBTreeListMaskColumn;
    TreeDepartamenteUNITATEA_CENTRALIZATOARE: TdxDBTreeListCheckColumn;
    TreeDepartamenteBANCA: TdxDBTreeListMaskColumn;
    TreeDepartamenteBANCA_COD: TdxDBTreeListMaskColumn;
    TreeDepartamenteBANCA_CONT: TdxDBTreeListMaskColumn;
    TreeDepartamenteCOD_FUNCTIONAL: TdxDBTreeListColumn;
    InspFunctiiUNITATATEA_URMARITA: TdxInspectorDBCheckRow;
    InspFunctiiARE_CONT: TdxInspectorDBCheckRow;
    InspFunctiiARE_CONTABILITATE: TdxInspectorDBCheckRow;
    InspFunctiiNUME_ORDONANTATOR: TdxInspectorDBMaskRow;
    InspFunctiiUNITATEA_CENTRALIZATOARE: TdxInspectorDBCheckRow;
    InspFunctiiBANCA: TdxInspectorDBMaskRow;
    InspFunctiiBANCA_COD: TdxInspectorDBMaskRow;
    InspFunctiiBANCA_CONT: TdxInspectorDBMaskRow;
    InspFunctiiCOD_FUNCTIONAL: TdxInspectorDBRow;
    InspFunctiiRow37: TdxInspectorDBRow;
    BtnOk: TcxButton;
    Splitter1: TcxSplitter;
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
    procedure InspFunctiiKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure InspFunctiiExit(Sender: TObject);
    procedure PopuleazaListaOrdonantatori;
    procedure ieOrdonantatoriChange(Sender: TObject);
    procedure inspOrdonantatoriExit(Sender: TObject);
    procedure inspOrdonantatoriChangeNode(Sender: TObject; OldNode, Node: TdxInspectorNode);
    procedure BugetOrdonantatoriAfterPost(DataSet: TDataSet);
    procedure BugetDirectiiAfterScroll(DataSet: TDataSet);
    procedure SpeedButton1Click(Sender: TObject);
    procedure BtnOkClick(Sender: TObject);
    procedure FormShow(Sender: TObject);
  private
    { Private declarations }
    FInternalConta : Integer;
    FBugetDirectiiAfterScroll: TDataSetNotifyEvent;
    FBugetOrdonantatoriAfterPost: TDataSetNotifyEvent;
    procedure RefreshDataSet;
    procedure InternalValidateUrmarire(Sender : TField);
    procedure WMHandleUnique(var Message : TMessage); message WM_SYNC_UNIQUE;
  public
    { Public declarations }
  end;

var
  frmOIUnitati : TfrmOIUnitati;  

implementation

{$R *.dfm}

uses DateUnit, Variants, AlegUtilizatoriUnit, ZDataSet, CommonDBVar;

procedure TfrmOIUnitati.FormCreate(Sender: TObject);
begin
  RefreshDataSet;
  FInternalConta := -1;
  Organigrama.ShapeFieldName := 'SHAPE_TYPE';
  Organigrama.ColorFieldName := 'SHAPE_COLOR';
  Organigrama.WidthFieldName := 'SHAPE_LEFT_TOP';
  Organigrama.HeightFieldName := 'SHAPE_RIGHT_BOTT';

  with frmData.QryBugetOrdonantatori do
  begin
    FBugetOrdonantatoriAfterPost := AfterPost;
    AfterPost := BugetOrdonantatoriAfterPost;
  end;

  with frmData.qryOIUnitati do
  begin
    FBugetDirectiiAfterScroll := AfterScroll;
    AfterScroll := BugetDirectiiAfterScroll;
    frmData.qryOIUnitati.FieldByName('UNITATEA_URMARITA').OnValidate  := InternalValidateUrmarire;
  end;

  PopuleazaListaOrdonantatori;
  PopulateImage(FrmData.QryBugetTipOrdonator,
                InspFunctiiTIP_ORDONATOR.Values,
                InspFunctiiTIP_ORDONATOR.Descriptions,
                'ID_BUGET_TIP_ORDONATOR', 'DENUMIRE');
end;

procedure TfrmOIUnitati.BtnAddDirClick(Sender: TObject);
begin
  { Adaugam o functie noua }
  FrmData.qryOIUnitati.Append;
  FrmData.qryOIUnitati.FieldByName('ID_PARINTE').Clear;
  FrmData.qryOIUnitati.Post;
end;

procedure TfrmOIUnitati.BtnAddSubDepClick(Sender: TObject);
var ParentId: Variant;
begin
  { Adaugam o subfunctie noua in contextul curent
    Daca este organigrama sau tree }
  if PageDeps.ActivePage = tabTree then begin
     if TreeDepartamente.FocusedNode <> nil then ParentId := TdxDBTreeListNode(TreeDepartamente.FocusedNode).Id
     else ParentId := Null;
  end
  else if Organigrama.Selected <> nil then ParentId := TdxDbOcNode(Organigrama.Selected).Key
       else ParentId := Null;
  FrmData.qryOIUnitati.Append;
  FrmData.qryOIUnitati.FieldByName('ID_PARINTE').AsInteger := ParentId;
  FrmData.qryOIUnitati.Post;
end;

procedure TfrmOIUnitati.TreeDepartamenteGetImageIndex(Sender: TObject;
  Node: TdxTreeListNode; var Index: Integer);
begin
  if Node.Level < 2 then Index := Node.Level else Index := 2;
end;

procedure TfrmOIUnitati.OrganigramaLoadNode(Sender: TObject; Node: TdxOcNode);
begin
  Node.ImageAlign := iaTC;
  if Node.Level < 2 then Node.ImageIndex := Node.Level else Node.ImageIndex := 2;
end;

procedure TfrmOIUnitati.TreeDepartamenteGetSelectedIndex(Sender: TObject;
  Node: TdxTreeListNode; var Index: Integer);
begin
  Index := Node.ImageIndex;
end;

procedure TfrmOIUnitati.BtnDelDepartamentClick(Sender: TObject);
begin
  if PageDeps.ActivePage = tabTree then begin
     if TreeDepartamente.FocusedNode <> nil then TdxDBTreeListNode(TreeDepartamente.FocusedNode).Delete;
  end
  else if Organigrama.Selected <> nil then Organigrama.Delete(Organigrama.Selected);
end;

procedure TfrmOIUnitati.FormClose(Sender: TObject; var Action: TCloseAction);
begin

  FrmData.qryOIUnitati.AfterScroll := FBugetDirectiiAfterScroll;
  FrmData.qryOIUnitati.AfterPost := FBugetOrdonantatoriAfterPost;

  with frmData.qryOIUnitati do
    if State in dsEditModes then Post;
  
    Action := caFree;
end;

procedure TfrmOIUnitati.InspFunctiiKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if Key = 13 then
    with frmData.qryOIUnitati do
      if State in dsEditModes then Post;
end;

procedure TfrmOIUnitati.InspFunctiiExit(Sender: TObject);
begin
  with frmData.qryOIUnitati do
    if State in dsEditModes then Post;
end;

procedure TfrmOIUnitati.PopuleazaListaOrdonantatori;
begin
{
  with GetTmpADOQuery do
  try
    SQL.Text := 'SELECT ID_BUGET_ORDONANTATORI, NUME+'' ''+PRENUME AS NUME FROM BUGET_ORDONANTATORI';
    Open;
    While not Eof do
    begin
      ieOrdonantatori.Values.Append(FieldByName('ID_BUGET_ORDONANTATORI').AsString);
      ieOrdonantatori.Descriptions.Append(FieldByName('NUME').AsString);
      Next;
    end;
  finally
    Free;
  end;
}
end;

procedure TfrmOIUnitati.ieOrdonantatoriChange(Sender: TObject);
begin
  with frmData.qryOIUnitati do
    if State in dsEditModes then Post;
end;

procedure TfrmOIUnitati.inspOrdonantatoriExit(Sender: TObject);
begin
  with frmData.QryBugetOrdonantatori do
    if State in dsEditModes then Post;
end;

procedure TfrmOIUnitati.inspOrdonantatoriChangeNode(Sender: TObject;
  OldNode, Node: TdxInspectorNode);
begin
  with frmData.QryBugetOrdonantatori do
    if State in dsEditModes then Post;
end;

procedure TfrmOIUnitati.BugetOrdonantatoriAfterPost(DataSet: TDataSet);
var s: String;
begin
  {with ieOrdonantatori do
    with frmData.QryBugetOrdonantatori do
    begin
      s := FieldByName('Nume').AsString + ' ' + FieldByName('Prenume').AsString;
      Descriptions[Values.IndexOf(FieldByName('ID_BUGET_ORDONANTATORI').AsString)] := s;
    end;
    ieOrdonantatori.Text := s;}
end;

procedure TfrmOIUnitati.SpeedButton1Click(Sender: TObject);
var IDOrd: String;
begin
  with GetTempQuery do
  try
    SQL.Text := 'Select top 1 * from Buget_Ordonantatori';
    Open;
    Edit;
    Append;
    FieldByName('Nume').AsString := '';
    Post;
    IDOrd := FieldByName('ID_BUGET_ORDONANTATORI').AsString;
  finally
    Free;
  end;

  with frmData.qryOIUnitati do
  begin
    if not(State in dsEditModes) then Edit;
    FieldByName('ID_BUGET_ORDONANTATORI').AsString := IDOrd;
    Post;
  end;

{  with ieOrdonantatori do
  begin
    Values.Append(IDOrd);
    Descriptions.Append('');
    Text := '';
  end;
  inspOrdonantatori.SetFocus;}
end;

procedure TfrmOIUnitati.BugetDirectiiAfterScroll(DataSet: TDataSet);
begin
//  inspOrdonantatori.Enabled := frmData.qryOIUnitati.FieldByName('ID_BUGET_ORDONANTATORI').AsString <> '';
end;

procedure TfrmOIUnitati.InternalValidateUrmarire(Sender: TField);
begin
  if not Sender.AsBoolean then Exit;
  if (Sender= nil) or (UpperCase(Sender.FieldName) <> 'UNITATEA_URMARITA') then Exit;
  FInternalConta := Sender.DataSet.FieldByName('ID_OI_UNITATI').AsInteger;

  PostMessage(Handle, WM_SYNC_UNIQUE, 0, 0);

end;

procedure TfrmOIUnitati.WMHandleUnique(var Message: TMessage);
var
  aBook : TBookmark;
begin
  if FInternalConta = -1 then Exit;
  with FrmData.qryOIUnitati  do
    try
      DisableControls;
      aBook := GetBookmark;
      First;
      while not eof do begin
        if FInternalConta <> FieldByName('ID_OI_UNITATI').AsInteger then
          if FieldByName('UNITATEA_URMARITA').AsBoolean then begin
              if not(State in [dsEdit, dsInsert]) then Edit;
              FieldByName('UNITATEA_URMARITA').AsBoolean := False;
              Post;
          end;
        Next;
      end;
    finally
      GotoBookmark(aBook);
      FreeBookmark(aBook);
      EnableControls;
    end;
end;

procedure TfrmOIUnitati.BtnOkClick(Sender: TObject);
begin
  Close;
end;

procedure TfrmOIUnitati.FormShow(Sender: TObject);
begin
  WindowState := wsMaximized;
end;

procedure TfrmOIUnitati.RefreshDataSet;
begin
  DoRefreshDataSet(frmData.qryOIUnitati);
end;

end.
