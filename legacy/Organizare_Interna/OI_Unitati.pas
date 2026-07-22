unit OI_Unitati;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms,
  Dialogs, ComCtrls, ExtCtrls, dxorgchr, dxdborgc,
  Buttons, Menus, ImgList, StdCtrls, DB,
  cxLookAndFeelPainters, cxButtons,
  cxPC, cxControls, cxSplitter, cxGraphics, cxTL,
  cxInplaceContainer, cxTLData, cxDBTL, cxMaskEdit, cxCheckBox,
  cxContainer, cxEdit, cxTextEdit, cxSpinEdit,  cxVGrid,
  cxMemo, cxImageComboBox, cxButtonEdit, cxDBVGrid,
  cxTLdxBarBuiltInMenu, cxLookAndFeels, cxCustomData, cxStyles, cxCurrencyEdit,
  cxFilter, cxData, cxDataStorage, cxDBData, cxDBLookupComboBox,
  cxGridLevel, cxGridCustomTableView, cxGridTableView, cxGridDBTableView,
  cxClasses, cxGridCustomView, cxGrid, cxDBEdit, ZAbstractRODataset,
  ZAbstractDataset, ZDataset, dxBarBuiltInMenu, cxNavigator,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxorgcedadv,
  dxDateRanges, dxScrollbarAnnotations, cxDropDownEdit, cxMRUEdit;


const
  WM_SYNC_UNIQUE = WM_USER + 1;
  WM_POST_DATA = WM_USER + 2;

type
  TfrmOIUnitatiNew = class(TForm)
    PageDeps: TcxPageControl;
    StatusBar: TStatusBar;
    tabOrganigrama: TcxTabSheet;
    Organigrama: TdxDbOrgChart;
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
    Splitter1: TcxSplitter;
    tabArbore: TcxTabSheet;
    cxTreeUnitati: TcxDBTreeList;
    cxTreeUnitatiID_OI_UNITATI: TcxDBTreeListColumn;
    cxTreeUnitatiID_OI_UNITATI_TIPURI: TcxDBTreeListColumn;
    cxTreeUnitatiID_PARINTE: TcxDBTreeListColumn;
    cxTreeUnitatiDENUMIRE: TcxDBTreeListColumn;
    cxTreeUnitatiDESCRIERE: TcxDBTreeListColumn;
    cxTreeUnitatiUNITATEA_URMARITA: TcxDBTreeListColumn;
    cxTreeUnitatiNUME_ORDONANTATOR: TcxDBTreeListColumn;
    cxTreeUnitatiID_UTILIZATORI: TcxDBTreeListColumn;
    cxTreeUnitatiSTARE: TcxDBTreeListColumn;
    cxTreeUnitatiUNITATEA_CENTRALIZATOARE: TcxDBTreeListColumn;
    cxTreeUnitatiBANCA: TcxDBTreeListColumn;
    cxTreeUnitatiBANCA_COD: TcxDBTreeListColumn;
    cxTreeUnitatiBANCA_CONT: TcxDBTreeListColumn;
    cxTreeUnitatiCOD_FUNCTIONAL: TcxDBTreeListColumn;
    cxTreeUnitatiSHAPE_TYPE: TcxDBTreeListColumn;
    cxTreeUnitatiSHAPE_COLOR: TcxDBTreeListColumn;
    cxTreeUnitatiSHAPE_LEFT_TOP: TcxDBTreeListColumn;
    cxTreeUnitatiSHAPE_RIGHT_BOTTOM: TcxDBTreeListColumn;
    cxTreeUnitatiSHAPE_POS_ID: TcxDBTreeListColumn;
    cxTreeUnitatiSHAPE_FONT_COLOR: TcxDBTreeListColumn;
    cxTreeUnitatiSHAPE_FONT_NAME: TcxDBTreeListColumn;
    cxTreeUnitatiARE_CONT: TcxDBTreeListColumn;
    cxTreeUnitatiARE_CONTABILITATE: TcxDBTreeListColumn;
    pnSettings: TPanel;
    edtIdX: TcxSpinEdit;
    edtIdY: TcxSpinEdit;
    Label1: TLabel;
    Label2: TLabel;
    chkZoom: TcxCheckBox;
    edtLinieWidth: TcxSpinEdit;
    Label3: TLabel;
    chkRotit: TcxCheckBox;
    chkAnimat: TcxCheckBox;
    chk3D: TcxCheckBox;
    ColorDialog: TColorDialog;
    cxInspFunctii: TcxDBVerticalGrid;
    cxInspFunctiiCategoryRow1: TcxCategoryRow;
    cxInspFunctiiID_BUGET_DIRECTII: TcxDBEditorRow;
    cxInspFunctiiID_PARINTE: TcxDBEditorRow;
    cxInspFunctiiID_UTILIZATORI: TcxDBEditorRow;
    cxInspFunctiiCategoryRow2: TcxCategoryRow;
    cxInspFunctiiDENUMIRE: TcxDBEditorRow;
    cxInspFunctiiDESCRIERE: TcxDBEditorRow;
    cxInspFunctiiATRIBUTII: TcxDBEditorRow;
    cxInspFunctiiID_BUGET_TIP_ORDONATOR: TcxDBEditorRow;
    cxInspFunctiiCategoryRow3: TcxCategoryRow;
    cxInspFunctiiUNITATEA_URMARITA: TcxDBEditorRow;
    cxInspFunctiiARE_CONT: TcxDBEditorRow;
    cxInspFunctiiARE_CONTABILITATE: TcxDBEditorRow;
    cxInspFunctiiNUME_ORDONANTATOR: TcxDBEditorRow;
    cxInspFunctiiUNITATEA_CENTRALIZATOARE: TcxDBEditorRow;
    cxInspFunctiiCategoryRow4: TcxCategoryRow;
    cxInspFunctiiBANCA: TcxDBEditorRow;
    cxInspFunctiiBANCA_COD: TcxDBEditorRow;
    cxInspFunctiiBANCA_CONT: TcxDBEditorRow;
    cxInspFunctiiCategoryRow5: TcxCategoryRow;
    cxInspFunctiiSHAPE_POS_ID: TcxDBEditorRow;
    cxInspFunctiiSHAPE_TYPE: TcxDBEditorRow;
    cxInspFunctiiSHAPE_RIGHT_BOTTOM: TcxDBEditorRow;
    cxInspFunctiiSHAPE_LEFT_TOP: TcxDBEditorRow;
    cxInspFunctiiSHAPE_COLOR: TcxDBEditorRow;
    cxInspFunctiiSHAPE_FONT_COLOR: TcxDBEditorRow;
    cxInspFunctiiSHAPE_FONT_NAME: TcxDBEditorRow;
    cxInspFunctiiEste_Interna: TcxDBEditorRow;
    DTCF: TDataSource;
    QryCF: TZQuery;
    DTBuget: TDataSource;
    qryBuget: TZQuery;
    cxPageControl: TcxPageControl;
    tabImplicit: TcxTabSheet;
    Label5: TLabel;
    btnCFAdd: TcxButton;
    btnCFDel: TcxButton;
    btnCFUpd: TcxButton;
    edCF: TcxDBButtonEdit;
    GridCF: TcxGrid;
    GridCFV: TcxGridDBTableView;
    GridCFVID_REPARTITORI_BUGET: TcxGridDBColumn;
    GridCFVID_REPARTITORI: TcxGridDBColumn;
    GridCFVCOD_FUNCTIONAL: TcxGridDBColumn;
    GridCFL: TcxGridLevel;
    btnPlanificare: TcxButton;
    tabBuget: TcxTabSheet;
    GridBuget: TcxGrid;
    GridBugetV: TcxGridDBTableView;
    GridBugetVid: TcxGridDBColumn;
    GridBugetVcod_functional: TcxGridDBColumn;
    GridBugetVcod_economic: TcxGridDBColumn;
    GridBugetVid_bg_versiune: TcxGridDBColumn;
    GridBugetVid_oi_proiecte: TcxGridDBColumn;
    GridBugetVan_fiscal: TcxGridDBColumn;
    GridBugetVrevizie: TcxGridDBColumn;
    GridBugetVplanificat1: TcxGridDBColumn;
    GridBugetVplanificat2: TcxGridDBColumn;
    GridBugetVplanificat3: TcxGridDBColumn;
    GridBugetVplanificat4: TcxGridDBColumn;
    GridBugetVplanificat: TcxGridDBColumn;
    GridBugetVden_functional: TcxGridDBColumn;
    GridBugetVden_economic: TcxGridDBColumn;
    GridBugetLevel: TcxGridLevel;
    Timer1: TTimer;
     procedure Timer1Timer(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure BtnAddDirClick(Sender: TObject);
    procedure BtnAddSubDepClick(Sender: TObject);
    procedure OrganigramaLoadNode(Sender: TObject; Node: TdxOcNode);
    procedure BtnDelDepartamentClick(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure InspFunctiiKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure InspFunctiiExit(Sender: TObject);
    procedure PopuleazaListaOrdonantatori;
    procedure ieOrdonantatoriChange(Sender: TObject);
    procedure BugetDirectiiAfterScroll(DataSet: TDataSet);
    procedure SpeedButton1Click(Sender: TObject);
    procedure BtnOkClick(Sender: TObject);
    procedure cxTreeUnitatiDragOver(Sender, Source: TObject; X, Y: Integer;
      State: TDragState; var Accept: Boolean);
    procedure chkZoomClick(Sender: TObject);
    procedure edtIdXPropertiesChange(Sender: TObject);
    procedure edtIdYPropertiesChange(Sender: TObject);
    procedure chkRotitClick(Sender: TObject);
    procedure chkAnimatClick(Sender: TObject);
    procedure chk3DClick(Sender: TObject);
    procedure edtLinieWidthPropertiesChange(Sender: TObject);
    procedure cxTreeUnitatiGetNodeImageIndex(Sender: TcxCustomTreeList;
      ANode: TcxTreeListNode; AIndexType: TcxTreeListImageIndexType;
      var AIndex: TImageIndex);
    procedure InspFunctiiSHAPE_COLORButtonClick(Sender: TObject;
      AbsoluteIndex: Integer);
    procedure InspFunctiiSHAPE_TYPEChange(Sender: TObject);
    procedure cxInspFunctiiSHAPE_TYPEEditPropertiesChange(Sender: TObject);
    procedure cxInspFunctiiSHAPE_COLOREditPropertiesButtonClick(
      Sender: TObject; AButtonIndex: Integer);
    procedure cxInspFunctiiDrawValue(Sender: TObject; ACanvas: TcxCanvas;
      APainter: TcxvgPainter; AValueInfo: TcxRowValueInfo;
      var Done: Boolean);
    procedure cxInspFunctiiKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure cxInspFunctiiExit(Sender: TObject);
    procedure cxInspFunctiiCOD_FUNCTIONALEditPropertiesButtonClick(
      Sender: TObject; AButtonIndex: Integer);
    procedure FormDestroy(Sender: TObject);
    procedure cxTreeUnitatiFocusedNodeChanged(Sender: TcxCustomTreeList;
      APrevFocusedNode, AFocusedNode: TcxTreeListNode);
    procedure btnCFAddClick(Sender: TObject);
    procedure btnCFDelClick(Sender: TObject);
    procedure btnCFUpdClick(Sender: TObject);
    procedure btnPlanificareClick(Sender: TObject);
    procedure edCFPropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
    procedure QryCFNewRecord(DataSet: TDataSet); private
    { Private declarations }
    FInternalConta : Integer;
    FBugetDirectiiAfterScroll: TDataSetNotifyEvent;
    FBugetOrdonantatoriAfterPost: TDataSetNotifyEvent;
    procedure AddUnitate(const Parented : Boolean=False);
    procedure DataNewRecord(DataSet : TDataSet);
    procedure RefreshDataSet;
    procedure InternalValidateUrmarire(Sender : TField);
    procedure WMHandleUnique(var Message : TMessage); message WM_SYNC_UNIQUE;
    procedure WMPost(var Message : TMessage); message WM_POST_DATA;
   procedure WMForceRefresh(var Message: TMessage); message WM_USER + 200;

  public
    { Public declarations }
  end;

var
  frmOIUnitatiNew : TfrmOIUnitatiNew;

implementation

{$R *.dfm}

uses
  dxCompsUtile, ZeosDBUtile, DateUnit, Variants, CommonDBVar,
  SelBugetUnit, MainUnit;

procedure TfrmOIUnitatiNew.FormCreate(Sender: TObject);
begin

   Timer1.Interval := 1000;
  Timer1.Enabled := True;
  frmdata.qryOIUnitati.OnNewRecord := DataNewRecord;
  RefreshDataSet;
  FInternalConta              := -1;
  Organigrama.ShapeFieldName  := 'SHAPE_TYPE';
  Organigrama.ColorFieldName  := 'SHAPE_COLOR';
  Organigrama.WidthFieldName  := 'SHAPE_LEFT_TOP';
  Organigrama.HeightFieldName := 'SHAPE_RIGHT_BOTTOM';

   Width := Width - 1;
  Application.ProcessMessages;
  Width := Width + 1;
  with frmData.qryOIUnitati do
  begin
    FBugetDirectiiAfterScroll := AfterScroll;
    AfterScroll := BugetDirectiiAfterScroll;
    frmData.qryOIUnitati.FieldByName('UNITATEA_URMARITA').OnValidate  := InternalValidateUrmarire;
  end;

  PopuleazaListaOrdonantatori;
  FillImageCombo(cxInspFunctiiID_BUGET_TIP_ORDONATOR.Properties.EditProperties, FrmData.QryBugetTipOrdonator, 'ID_BUGET_TIP_ORDONATOR', 'DENUMIRE');
  edtIdX.Value := Organigrama.IndentX;
  edtIdY.Value := Organigrama.IndentY;
  cxInspFunctii.OptionsView.ScrollBars := ssVertical;
  cxInspFunctii.LookAndFeel.ScrollbarMode := sbmClassic;
  Self.Perform(WM_SYSCOMMAND, SC_MAXIMIZE, 0);
  Application.ProcessMessages;
  Sleep(100);
  PostMessage(Self.Handle, WM_USER + 200, 0, 0);
end;
procedure TfrmOIUnitatiNew.WMForceRefresh(var Message: TMessage);
begin
  cxPageControl.Realign;
  cxTreeUnitati.Realign;
  cxTreeUnitati.Repaint;
  Application.ProcessMessages;
  Sleep(100);
end;

procedure TfrmOIUnitatiNew.BtnAddDirClick(Sender: TObject);
begin
  AddUnitate;
end;

procedure TfrmOIUnitatiNew.BtnAddSubDepClick(Sender: TObject);
begin
  AddUnitate(True);
end;

procedure TfrmOIUnitatiNew.OrganigramaLoadNode(Sender: TObject; Node: TdxOcNode);
begin
  Node.ImageAlign := iaTC;
  if Node.Level < 2 then Node.ImageIndex := Node.Level else Node.ImageIndex := 2;
end;

procedure TfrmOIUnitatiNew.BtnDelDepartamentClick(Sender: TObject);
var
  lHaveReps : Boolean;
  lIdRep : Integer;
begin
  lHaveReps := False;
  lIdRep := frmData.qryOIUnitati.FieldByName('ID_OI_UNITATI').AsInteger;
  if (lIdRep > 0) and DBProcExists('SP_VERIFICARE_REPARTITOR') then
    lHaveReps := ValueSafeToInt( DBGetScallarFmt('exec [SP_VERIFICARE_REPARTITOR] %d', [lIdRep])) > 0;
  if lHaveReps then begin
    if MessageDlg('Repartitorul/Unitatea este deja folosit in cadrul aplicatiei !'#13#10'Doriti totusi stergerea lui?',
                  mtConfirmation, [mbYes, mbNo], 0) <> mrYes then Abort;
  end
  else
    if MessageDlg('Doriti stergerea unitatii curente ?',
                  mtConfirmation, [mbYes, mbNo], 0) <> mrYes then Abort;
  frmData.qryOIUnitati.Delete;

{
  if PageDeps.ActivePage = tabArbore then begin
     if cxTreeUnitati.FocusedNode <> nil then TcxDBTreeListNode(cxTreeUnitati.FocusedNode).Delete;
  end
  else if Organigrama.Selected <> nil then Organigrama.Delete(Organigrama.Selected);
}
end;

procedure TfrmOIUnitatiNew.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  FrmData.qryOIUnitati.AfterScroll := FBugetDirectiiAfterScroll;
  FrmData.qryOIUnitati.AfterPost := FBugetOrdonantatoriAfterPost;

  with frmData.qryOIUnitati do
    if State in dsEditModes then Post;
  
    Action := caFree;
end;

procedure TfrmOIUnitatiNew.InspFunctiiKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if Key = 13 then
    with frmData.qryOIUnitati do
      if State in dsEditModes then Post;
end;

procedure TfrmOIUnitatiNew.InspFunctiiExit(Sender: TObject);
begin
  with frmData.qryOIUnitati do
    if State in dsEditModes then Post;
end;

procedure TfrmOIUnitatiNew.PopuleazaListaOrdonantatori;
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

procedure TfrmOIUnitatiNew.ieOrdonantatoriChange(Sender: TObject);
begin
  with frmData.qryOIUnitati do
    if State in dsEditModes then Post;
end;

procedure TfrmOIUnitatiNew.SpeedButton1Click(Sender: TObject);
var IDOrd: String;
begin
  with GetTmpADOQuery do
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

procedure TfrmOIUnitatiNew.BugetDirectiiAfterScroll(DataSet: TDataSet);
begin
//  inspOrdonantatori.Enabled := frmData.qryOIUnitati.FieldByName('ID_BUGET_ORDONANTATORI').AsString <> '';
end;

procedure TfrmOIUnitatiNew.InternalValidateUrmarire(Sender: TField);
begin
  if not Sender.AsBoolean then Exit;
  if (Sender= nil) or (UpperCase(Sender.FieldName) <> 'UNITATEA_URMARITA') then Exit;
  FInternalConta := Sender.DataSet.FieldByName('ID_OI_UNITATI').AsInteger;

  PostMessage(Handle, WM_SYNC_UNIQUE, 0, 0);

end;

procedure TfrmOIUnitatiNew.WMHandleUnique(var Message: TMessage);
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

procedure TfrmOIUnitatiNew.BtnOkClick(Sender: TObject);
begin
  Close;
end;

procedure TfrmOIUnitatiNew.RefreshDataSet;
begin
  frmData.qryOIUnitati.Close;
  frmData.qryOIUnitati.Open;
end;
procedure TfrmOIUnitatiNew.Timer1Timer(Sender: TObject);
begin
  Timer1.Enabled := False; // Dezactivează timer-ul pentru a rula o singură dată
  PostMessage(Self.Handle, WM_USER + 200, 0, 0); // Apelează metoda WMForceRefresh
end;

procedure TfrmOIUnitatiNew.cxTreeUnitatiDragOver(Sender, Source: TObject;
  X, Y: Integer; State: TDragState; var Accept: Boolean);
begin
//
end;

procedure TfrmOIUnitatiNew.chkZoomClick(Sender: TObject);
begin
  Organigrama.Zoom := chkZoom.Checked;
end;

procedure TfrmOIUnitatiNew.edtIdXPropertiesChange(Sender: TObject);
begin
  Organigrama.IndentX := edtIdX.Value;
end;

procedure TfrmOIUnitatiNew.edtIdYPropertiesChange(Sender: TObject);
begin
  Organigrama.IndentY := edtIdY.Value;
end;

procedure TfrmOIUnitatiNew.chkRotitClick(Sender: TObject);
begin
  Organigrama.Rotated := chkRotit.Checked;
end;

procedure TfrmOIUnitatiNew.chkAnimatClick(Sender: TObject);
begin
  if chkAnimat.Checked then
    Organigrama.Options := Organigrama.Options  + [ocAnimate]
  else
    Organigrama.Options := Organigrama.Options  - [ocAnimate];
end;

procedure TfrmOIUnitatiNew.chk3DClick(Sender: TObject);
begin
  if chk3D.Checked then
    Organigrama.Options := Organigrama.Options  + [ocRect3D]
  else
    Organigrama.Options := Organigrama.Options  - [ocRect3D];
end;

procedure TfrmOIUnitatiNew.edtLinieWidthPropertiesChange(Sender: TObject);
begin
  Organigrama.LineWidth := edtLinieWidth.Value;
end;

procedure TfrmOIUnitatiNew.cxTreeUnitatiGetNodeImageIndex(Sender: TcxCustomTreeList;
  ANode: TcxTreeListNode; AIndexType: TcxTreeListImageIndexType;
  var AIndex: TImageIndex);
begin
  if ANode.Level < 2 then AIndex := ANode.Level else AIndex := 2;
end;

procedure TfrmOIUnitatiNew.InspFunctiiSHAPE_COLORButtonClick(
  Sender: TObject; AbsoluteIndex: Integer);
begin
   if frmData.qryOIUnitati.State in [dsEdit, dsInsert] then
      frmData.qryOIUnitati.Post;
   ColorDialog.Color   := frmData.qryOIUnitati.FieldByName('SHAPE_COLOR').AsInteger;
   ColorDialog.Execute;
   frmData.qryOIUnitati.Edit;
   frmData.qryOIUnitati.FieldByName('SHAPE_COLOR').AsInteger := ColorDialog.Color;
   frmData.qryOIUnitati.Post;
end;

procedure TfrmOIUnitatiNew.WMPost(var Message: TMessage);
begin
  DoCheckPostDataSet(frmData.qryOIUnitati);
end;

procedure TfrmOIUnitatiNew.InspFunctiiSHAPE_TYPEChange(Sender: TObject);
begin
  PostMessage(Handle, WM_POST_DATA, 0, 0);
end;

procedure TfrmOIUnitatiNew.cxInspFunctiiSHAPE_TYPEEditPropertiesChange(
  Sender: TObject);
begin
  PostMessage(Handle, WM_POST_DATA, 0, 0);
end;

procedure TfrmOIUnitatiNew.cxInspFunctiiSHAPE_COLOREditPropertiesButtonClick(
  Sender: TObject; AButtonIndex: Integer);
begin
   if frmData.qryOIUnitati.State in [dsEdit, dsInsert] then
      frmData.qryOIUnitati.Post;
   ColorDialog.Color   := frmData.qryOIUnitati.FieldByName('SHAPE_COLOR').AsInteger;
   ColorDialog.Execute;
   frmData.qryOIUnitati.Edit;
   frmData.qryOIUnitati.FieldByName('SHAPE_COLOR').AsInteger := ColorDialog.Color;
   frmData.qryOIUnitati.Post;
end;

procedure TfrmOIUnitatiNew.cxInspFunctiiDrawValue(Sender: TObject;
  ACanvas: TcxCanvas; APainter: TcxvgPainter; AValueInfo: TcxRowValueInfo;
  var Done: Boolean);
begin
  if AValueInfo.Row = cxInspFunctiiSHAPE_COLOR then begin
    if frmData.qryOIUnitati.FieldByName('SHAPE_COLOR').Value = Null then
      ACanvas.Brush.Color := clWhite
    else
      ACanvas.Brush.Color := frmData.qryOIUnitati.FieldByName('SHAPE_COLOR').AsInteger;
    ACanvas.FillRect(AValueInfo.ContentRect, ACanvas.Brush.Color);
  end;
end;

procedure TfrmOIUnitatiNew.cxInspFunctiiKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if Key = VK_RETURN then
    with frmData.qryOIUnitati do
      if State in dsEditModes then Post;
end;

procedure TfrmOIUnitatiNew.cxInspFunctiiExit(Sender: TObject);
begin
    with frmData.qryOIUnitati do
      if State in dsEditModes then Post;
end;

procedure TfrmOIUnitatiNew.cxInspFunctiiCOD_FUNCTIONALEditPropertiesButtonClick(
  Sender: TObject; AButtonIndex: Integer);
var
  aRes, aCod : String;
  aID : integer;
begin
  aCod := frmData.qryOIUnitati.FieldByName('COD_FUNCTIONAL').AsString;
  aRes := NewSelectarePlanFunctional(aCod, aId, True, True);
  if aRes <> '<Anulat>' then begin
    DBSetFieldValue(frmData.qryOIUnitati, 'COD_FUNCTIONAL', aCod);
    DBExecSQLFmt('exec [spRepartitorCodFunctionalDef] %s', [ValueToStr(frmData.qryOIUnitati['ID_OI_UNITATI'])] );
  end;
end;

procedure TfrmOIUnitatiNew.DataNewRecord(DataSet: TDataSet);
begin
end;

procedure TfrmOIUnitatiNew.FormDestroy(Sender: TObject);
begin
  frmdata.qryOIUnitati.OnNewRecord := nil;
end;

procedure TfrmOIUnitatiNew.AddUnitate(const Parented: Boolean);
var
  aId : Integer;
  aQry : TZReadOnlyQuery;
  ParentId: Variant;
begin
  if not Parented then ParentId := Null
  else begin
    { Adaugam o subfunctie noua in contextul curent
      Daca este organigrama sau tree }
    if PageDeps.ActivePage = tabArbore then begin
       if cxTreeUnitati.FocusedNode <> nil then ParentId := TcxDBTreeListNode(cxTreeUnitati.FocusedNode).KeyValue
       else ParentId := Null;
    end
    else if Organigrama.Selected <> nil then ParentId := TdxDbOcNode(Organigrama.Selected).Key
         else ParentId := Null;
  end;

  aQry := GetTmpADOQuery;
  with aQry do
    try
     if ParentId = Null then
       SQL.Add('exec spOIUnitatiAdd ''Directie Noua'', NULL')
     else
       SQL.Add('exec spOIUnitatiAdd ''Directie Noua'', ' + VarToStr(ParentId));
     Open;
     if not IsEmpty then begin
       aId := Fields[0].AsInteger;
       RefreshDataSet;
       frmData.qryOIUnitati.Locate('ID_OI_UNITATI', aId,[]);
     end;
    finally
      Free;
    end;
end;

procedure TfrmOIUnitatiNew.cxTreeUnitatiFocusedNodeChanged(
  Sender: TcxCustomTreeList; APrevFocusedNode,
  AFocusedNode: TcxTreeListNode);
var
  lIdUnitate : Integer;
begin
  if AFocusedNode = nil then Exit;
  lIdUnitate := GetInteger(TcxDBTreeListNode(AFocusedNode).KeyValue);
  qryBuget.Close;
  qryBuget.Params.ParamByName('ID_UNITATE').Value := lIdUnitate;
  qryBuget.Open;
  QryCF.Close;
  QryCF.Params.ParamByName('ID_UNITATE').Value := lIdUnitate;
  QryCF.Open;
end;

procedure TfrmOIUnitatiNew.btnCFAddClick(Sender: TObject);
begin
  if QryCF.Active then
    QryCF.Append;
end;

procedure TfrmOIUnitatiNew.btnCFDelClick(Sender: TObject);
begin
  if QryCF.Active and (QryCF.RecordCount>0) then
     QryCF.Delete;
end;

procedure TfrmOIUnitatiNew.btnCFUpdClick(Sender: TObject);
begin
  if QryCF.Active and (QryCF.State in [dsEdit, dsInsert]) then QryCF.Post;
end;

procedure TfrmOIUnitatiNew.btnPlanificareClick(Sender: TObject);
begin
  MainForm.Cmd_BGAprobatExecute(nil);
end;

procedure TfrmOIUnitatiNew.edCFPropertiesButtonClick(Sender: TObject;
  AButtonIndex: Integer);
var
  aCont : String;
  lIdUnitate : Integer;
begin
  aCont := qryCF.FieldByName(TcxDBButtonEdit(Sender).DataBinding.DataField).AsString;
  NewSelectarePlanFunctional(aCont, lIdUnitate, True, True);
  if aCont <> '<Anulat>' then begin
    qryCF.Edit;
    qryCF.FieldByName(TcxDBButtonEdit(Sender).DataBinding.DataField).Value := aCont;
    qryCF.Post;
  end;
end;

procedure TfrmOIUnitatiNew.QryCFNewRecord(DataSet: TDataSet);
var
  lNode: TcxTreeListNode;
begin
  lNode := cxTreeUnitati.FocusedNode;
  if Assigned(lNode) then
    DataSet['id_repartitori'] := lNode.Values[cxTreeUnitatiID_OI_UNITATI.ItemIndex]
  else
    raise Exception.Create('Selectati o unitate !');
end;

end.
