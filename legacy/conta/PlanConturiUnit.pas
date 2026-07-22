unit PlanConturiUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ComCtrls, ImgList, StdCtrls, ExtCtrls, dxInspRw, dxDBInRw, dxExEdtr,
  dxInspct, dxCntner, dxDBInsp, dxtree, dxTL, dxDBCtrl, Menus, ActnList, dxDBTLCl, ToolWin, DB, ZDataSet,
  cxLookAndFeelPainters, cxButtons,
  ZAbstractRODataset, ZAbstractDataset,
  cxGraphics,
  cxLookAndFeels, cxControls, cxStyles, cxEdit, cxMaskEdit, cxCheckBox,
  cxTextEdit, cxImageComboBox, cxDropDownEdit, cxCurrencyEdit, cxVGrid,
  cxDBVGrid, cxInplaceContainer, dxDBTL,
  cxDataControllerConditionalFormattingRulesManagerDialog, cxCustomData, cxTL,
  cxTLdxBarBuiltInMenu, cxClasses, cxDBTL, cxTLData, dxScrollbarAnnotations;

type
  TCrackAtsTree = class(TdxTreeList);
  TFrmPlanConturi = class(TForm)
    ImaginiConturi: TImageList;
    GrPlanConturi: TGroupBox;
    Splitter1: TSplitter;
    ppPlanConturi: TPopupMenu;
    Actiuni: TActionList;
    Cmd_AdaugaContAnalitic: TAction;
    Cmd_AdaugaContPeAcelasiNeivle: TAction;
    AdaugaNouAnalitic1: TMenuItem;
    AdaugaContPeNivelulCurent1: TMenuItem;
    Cmd_FisaContului: TAction;
    N1: TMenuItem;
    Cmd_ModificareContCurent: TAction;
    ModificareContCurent1: TMenuItem;
    N2: TMenuItem;
    ppMutaMaiSus: TMenuItem;
    Cmd_DeleteContCurent: TAction;
    StergeContCurent1: TMenuItem;
    ExpandLevels: TToolBar;
    TreeUnitate: TdxDBTreeList;
    DTUnitate: TDataSource;
    qryUnitate: TZQuery;
    TreeUnitateid: TdxDBTreeListMaskColumn;
    TreeUnitateDenumire: TdxDBTreeListColumn;
    TreeUnitateid_parinte: TdxDBTreeListMaskColumn;
    BtnOk: TcxButton;
    vContInfo: TcxDBVerticalGrid;
    vContInfoROMANA: TcxDBEditorRow;
    vContInfoSUMATOR: TcxDBEditorRow;
    vContInfoDBMultiEditorRow1: TcxDBMultiEditorRow;
    vContInfoBALANTA: TcxDBEditorRow;
    vContInfoDBEditorRow: TcxDBEditorRow;
    vContInfoIS_SINTETIC: TcxDBEditorRow;
    vContInfoSID: TcxDBEditorRow;
    vContInfoSIC: TcxDBEditorRow;
    vContInfoSPD: TcxDBEditorRow;
    vContInfoSPC: TcxDBEditorRow;
    vContInfoRD: TcxDBEditorRow;
    vContInfoRC: TcxDBEditorRow;
    vContInfoUnitate: TcxDBEditorRow;
    vContInfoSC: TcxDBEditorRow;
    vContInfoSD: TcxDBEditorRow;
    vContInfoTIP: TcxDBEditorRow;
    vContInfoTSC: TcxDBEditorRow;
    vContInfoTSD: TcxDBEditorRow;
    pnContInfo: TPanel;
    TreePlanCONT: TcxDBTreeListColumn;
    TreePlanROMANA: TcxDBTreeListColumn;
    TreePlanSID: TcxDBTreeListColumn;
    TreePlanSIC: TcxDBTreeListColumn;
    TreePlanFCTCONT: TcxDBTreeListColumn;
    TreePlanBALANTA: TcxDBTreeListColumn;
    TreePlanPARINTE: TcxDBTreeListColumn;
    TreePlan: TcxDBTreeList;
    stiluri: TcxStyleRepository;
    stilBifunctionalFunza: TcxStyle;
    stilCreditFrunza: TcxStyle;
    stilDebitFrunza: TcxStyle;
    stilBifunctional: TcxStyle;
    stilDebit: TcxStyle;
    stilCredit: TcxStyle;
    stilNormal: TcxStyle;
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure Cmd_AdaugaContAnaliticExecute(Sender: TObject);
    procedure Cmd_AdaugaContPeAcelasiNeivleExecute(Sender: TObject);
    procedure Cmd_ModificareContCurentExecute(Sender: TObject);
    procedure ppMutaMaiSusClick(Sender: TObject);
    procedure FormCloseQuery(Sender: TObject; var CanClose: Boolean);
    procedure BtnOkClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure Cmd_DeleteContCurentExecute(Sender: TObject);
    procedure ContUnitateDrawValue(Sender: TdxInspectorRow;
      ACanvas: TCanvas; ARect: TRect; var AText: String; AFont: TFont;
      var AColor: TColor; var ADone: Boolean);
    procedure ContUnitatePopup(Sender: TObject; const EditText: String);
    procedure TreeUnitateDblClick(Sender: TObject);
    procedure TreeUnitateKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure ContUnitateCloseUp(Sender: TObject; var Text: String;
      var Accept: Boolean);
    procedure TreePlanGetNodeImageIndex(Sender: TcxCustomTreeList;
      ANode: TcxTreeListNode; AIndexType: TcxTreeListImageIndexType;
      var AIndex: TImageIndex);
    procedure TreePlanFocusedNodeChanged(Sender: TcxCustomTreeList;
      APrevFocusedNode, AFocusedNode: TcxTreeListNode);
    procedure TreePlanNodeChanged(Sender: TcxCustomTreeList;
      ANode: TcxTreeListNode; AColumn: TcxTreeListColumn);
    procedure TreePlanStylesGetContentStyle(Sender: TcxCustomTreeList;
      AColumn: TcxTreeListColumn; ANode: TcxTreeListNode; var AStyle: TcxStyle);
  private
    { Private declarations }
    FMaxLevel : Integer;
    function GetNextChildCont(aCont: Variant): Variant;
    procedure TestContCurent(aCont : String; aOperatie : String; const inAnalitic : Boolean = False);
    procedure InternalExpand(Sender: TObject);
    procedure CreateLevelButtons;
    procedure AssignUnitate;
  public
    { Public declarations }
    FOnlyChild : Boolean;
    procedure SalveazaRecursiv(aId: Variant);
  end;


function SelectareContPlan(var CodCurent : String; const NumaiFrunze : Boolean = False) : Boolean;

type
  TCrackToolButton = class(TToolButton);
  
implementation


{$R *.DFM}

uses
  ZeosDBUtile, dxCompsUtile, Variants, ContUnit,  DateUnit, CommonDBVar;

procedure TFrmPlanConturi.TreePlanGetNodeImageIndex(Sender: TcxCustomTreeList;
  ANode: TcxTreeListNode; AIndexType: TcxTreeListImageIndexType;
  var AIndex: TImageIndex);
begin
  case AIndexType of
    tlitImageIndex:
      if ANode.HasChildren then
        if ANode.Expanded then
          AIndex := 2
        else
          AIndex := 0
      else
        AIndex := 1;
    tlitSelectedIndex:
      AIndex := ANode.ImageIndex;
  end;
end;

procedure TFrmPlanConturi.TreePlanNodeChanged(Sender: TcxCustomTreeList;
  ANode: TcxTreeListNode; AColumn: TcxTreeListColumn);
begin
  if FMaxLevel < ANode.Level then begin
    FMaxLevel := ANode.Level;
    CreateLevelButtons;
  end;
end;

procedure TFrmPlanConturi.TreePlanStylesGetContentStyle(
  Sender: TcxCustomTreeList; AColumn: TcxTreeListColumn; ANode: TcxTreeListNode;
  var AStyle: TcxStyle);
var
  lFctCont: String;
begin
  if Assigned(ANode) then begin
    lFctCont := ValueSafeToStr(ANode.Values[TreePlanFCTCONT.ItemIndex]);
    if ANode.HasChildren then begin
      if lFctCont = 'B' then AStyle := stilBifunctional
      else
      if lFctCont = 'D' then AStyle := stilDebit
      else
      if lFctCont = 'C' then AStyle := stilCredit
      else AStyle := stilNormal;
    end
    else begin
      if lFctCont = 'B' then AStyle := stilBifunctionalFunza
      else
      if lFctCont = 'D' then AStyle := stilDebitFrunza
      else
      if lFctCont = 'C' then AStyle := stilCreditFrunza
      else AStyle := stilNormal;
    end;
  end;
end;

procedure TFrmPlanConturi.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  
    Action := caFree;
end;

procedure TFrmPlanConturi.Cmd_AdaugaContAnaliticExecute(Sender: TObject);
var aParent : Variant;
    lDenCont: String;
    lNode   : TcxDBTreeListNode;
    lPrevCont: Variant;
begin
  lNode := TcxDBTreeListNode(TreePlan.FocusedNode);
  if lNode <> nil then begin
     aParent  := lNode.KeyValue;
     lDenCont := lNode.Texts[TreePlanROMANA.ItemIndex];
     if DBRecordExistsFmt('select top 1 1 from CNOTE WHERE STARE=1 AND (CONTC = %s OR CONTD = %s)', [ValueToStr(aParent), ValueToStr(aParent)]) then
        case MessageDlg('Aveti note contabile introduse pe contul '+QuotedStr(aParent)+#13#10+
                      'Doriti generarea automata a unui analitic pe care sa fie preluate notele contabile?'#13#10+
                      'Altfel acestea vor fi pierdute si nu se vor mai regasi in balanta!', mtConfirmation,
                      [mbYes, mbNo, mbCancel], 0) of
             mrYes: begin
                     TreePlan.BeginUpdate;
                     //edSimbol.EditMask := VarToStr(AParent)+'.999';
                     FrmData.QryPlanCont.Append;
                     FrmData.QryPlanCont.FieldByName('CONT').Value := aParent+'.0';
                     FrmData.QryPlanCont.FieldByName('ROMANA').AsString := lDenCont;
                     FrmData.QryPlanCont.FieldByName('PARINTE').Value := aParent;
                     FrmData.QryPlanCont.Post;
                     TreePlan.EndUpdate;
                     DBExecSQLFmt('exec [SP_TRANSFER_NOTE_PE_ANALITIC] %s, %s', [ValueToStr(FrmData.QryPlanCont['CONT']), ValueToStr(aParent)] );
                    end;
             mrNo: ;
             mrCancel: Abort;
        end;
  end
  else AParent := varNull;
  with TfrmContProp.Create(Application) do
    try
      edTip.Properties.Assign(vContInfoBALANTA.Properties.EditProperties);
      lPrevCont := frmData.QryPlanCont['CONT'];
      FrmData.QryPlanCont.Append;
      FrmData.QryPlanCont.FieldByName('ROMANA').Value  := lDenCont;
      FrmData.QryPlanCont.FieldByName('CONT').Value    := GetNextChildCont(aParent);
      FrmData.QryPlanCont.FieldByName('PARINTE').Value := aParent;
      FIntretin := Self;
      if ShowModal <> mrOk then
        frmData.QryPlanCont.Locate('CONT', lPrevCont, []);
    finally
      Free;
    end;
end;

function TFrmPlanConturi.GetNextChildCont(aCont: Variant): Variant;
var
  lTmpCont: String;
  lStartIndex: Integer;
begin
  { Luam Urmatorul Analitic disponibil }
  if DBProcExists('SP_GET_NEXT_CHILD_CONT') then
    Result := DBGetScallarFmt('exec [SP_GET_NEXT_CHILD_CONT] %s', [ValueToStr(aCont)] )
  else begin
    if not ValueHasValue(aCont) then begin
      lTmpCont := DBGetScallar('SELECT MAX(CONT) FROM CPLAN WHERE PARINTE IS NULL');
      lStartIndex := 1;
    end
    else begin
      lTmpCont := DBGetScallarFmt('SELECT TOP 1 CONT FROM CPLAN WHERE PARINTE LIKE %s ORDER BY LEN(CONT) DESC , CONT DESC', [ValueToStr(aCont)]);
      lStartIndex := Length(ValueSafeToStr(aCont));
    end;
    while (lStartIndex <= Length(lTmpCont)) and (lTmpCont[lStartIndex] = '.') do Inc(lStartIndex);
    if lStartIndex < Length(lTmpCont) then
      Result := Copy(lTmpCont, 1, Length(lTmpCont) - 1) + Chr(Ord(lTmpCont[Length(lTmpCont)])+1)
    else
      Result := aCont + '.01';
  end;
end;

procedure TFrmPlanConturi.Cmd_AdaugaContPeAcelasiNeivleExecute(
  Sender: TObject);
var aParent : Variant;
begin
  if TreePlan.FocusedNode <> nil then
     if TreePlan.FocusedNode.Parent <> nil then
        AParent := TcxDBTreeListNode(TreePlan.FocusedNode).ParentKeyValue
     else AParent := Null
  else AParent := Null;
  with TfrmContProp.Create(Application) do
    try
       edTip.Properties.Items.Assign(TcxCustomImageComboBoxProperties(vContInfoBALANTA.Properties.EditProperties).Items);
       TreePlan.BeginUpdate;
       //edSimbol.EditMask := VarToStr(AParent)+'.999';
       FrmData.QryPlanCont.Append;
       FrmData.QryPlanCont.FieldByName('CONT').Value := GetNextChildCont(aParent);
       if aParent <> FrmData.QryPlanCont.FieldByName('CONT').Value then
          FrmData.QryPlanCont.FieldByName('PARINTE').Value := aParent
       else FrmData.QryPlanCont.FieldByName('CONT').Value := Null;
       FrmData.QryPlanCont.Post;
       TreePlan.EndUpdate;
       FrmData.QryPlanCont.Edit;
       FIntretin := Self;
       ShowModal;
    finally
       Free;
    end;
end;

procedure TFrmPlanConturi.Cmd_ModificareContCurentExecute(Sender: TObject);
begin
  with TfrmContProp.Create(Application) do
    try
       edTip.Properties.Assign(vContInfoBALANTA.Properties.EditProperties);
       FrmData.QryPlanCont.Edit;
       FIntretin := Self;
       ShowModal;
    finally
       Free;
    end;
end;

procedure TFrmPlanConturi.ppMutaMaiSusClick(Sender: TObject);
var lNode: TcxDBTreeListNode;
    lParent: Variant;
    OldState: Boolean;
begin
  lNode := TcxDBTreeListNode(TreePlan.FocusedNode);
  if Assigned(lNode) and Assigned(lNode.Parent) then begin
     lParent := TcxDBTreeListNode(lNode.Parent).ParentKeyValue;
     OldState := FrmData.QryPlanCont.State in [dsEdit, dsInsert];
     if not OldState then FrmData.QryPlanCont.Edit;
     FrmData.QryPlanCont.FieldByName('PARINTE').Value := lParent;
     FrmData.QryPlanCont.Post;
     if OldState then FrmData.QryPlanCont.Edit;
  end;
end;

procedure TFrmPlanConturi.SalveazaRecursiv(aId: Variant);
var
  lNode: TcxDBTreeListNode;
  lTip, lDefa: Variant;
  OldId    : Variant;
  OldState : Boolean;

  procedure SetTreeRecurse(ANode: TcxDBTreeListNode);
  var I : Integer;
      lId: Variant;
   begin
     for I := 0 to ANode.Count-1 do begin
       if ValueSameValue(ANode.Items[I].Values[TreePlanFCTCONT.ItemIndex], lTip) or
          ValueSameValue(ANode.Items[I].Values[TreePlanBALANTA.ItemIndex], lDefa) then begin
          lId := TcxDBTreeListNode(ANode.Items[I]).KeyValue;
          if FrmData.QryPlanCont.Locate('CONT', lId, []) then begin
             FrmData.QryPlanCont.Edit;
             FrmData.QryPlanCont['FCTCONT'] := lTip;
             FrmData.QryPlanCont['BALANTA'] := lDefa;
             FrmData.QryPlanCont.Post;
          end;
       end;
       SetTreeRecurse(TcxDBTreeListNode(ANode.Items[I]));
     end;
   end;

begin
  lNode := TcxDBTreeListNode(TreePlan.FocusedNode);
  if (not Assigned(lNode)) or not ValueSameValue(lNode.KeyValue, aId) then
     lNode := TreePlan.FindNodeByKeyValue(aId);
  if Assigned(lNode) then begin
     lTip  := lNode.Values[TreePlanFCTCONT.ItemIndex];
     lDefa := lNode.Values[TreePlanBALANTA.ItemIndex];
     TreePlan.BeginUpdate;
     FrmData.QryPlanCont.DisableControls;
     OldId    := lNode.KeyValue;
     OldState := FrmData.QryPlanCont.State in [dsEdit, dsInsert];
     try
        SetTreeRecurse(lNode);
     finally
        if FrmData.QryPlanCont.FieldByName('CONT').AsString <> OldId then
           FrmData.QryPlanCont.Locate('CONT', OldId, []);
        if OldState then FrmData.QryPlanCont.Edit;
        FrmData.QryPlanCont.EnableControls;
        TreePlan.EndUpdate;
     end;
  end;
end;

procedure TFrmPlanConturi.FormCloseQuery(Sender: TObject;
  var CanClose: Boolean);
begin
   try
     if FrmData.QryPlanCont.State in [dsEdit, dsInsert] then begin
       FrmData.QryPlanCont.Post;
     end;
   except
   end;
end;

procedure TFrmPlanConturi.BtnOkClick(Sender: TObject);
begin
  if fsModal in FormState then ModalResult := mrOk
  else Close;
end;

procedure TFrmPlanConturi.FormCreate(Sender: TObject);
begin
  if DBProcExists('spCplanDefalcare') then
    FillImageCombo(vContInfoBALANTA.Properties.EditProperties, 'exec [spCplanDefalcare]', 'tip', 'denumire');
  FMaxLevel := -1;
  AssignUnitate;
  FOnlyChild := False;
  if not(fsModal in FormState) then
    DBRefresh(FrmData.QryPlanCont);
end;

function SelectareContPlan(var CodCurent : String; const NumaiFrunze : Boolean = False) : Boolean;
var
  frmPlanConturi : TFrmPlanConturi;
  aNode : TcxTreeListNode;
  aCont : String;
begin
  aCont := CodCurent;
  frmPlanConturi := TFrmPlanConturi.Create(nil);
  with frmPlanConturi do
    try
      frmPlanConturi.Visible := False;
      FOnlyChild := NumaiFrunze;
      Caption := 'Selectie cont';
      vContInfo.Visible := False;
      TreePlan.PopupMenu := nil;
      TreePlan.ApplyBestFit();
      TreePlan.OptionsData.Editing := False;
      TreePlan.OptionsBehavior.IncSearch := True;
      if Trim(aCont) <> '' then begin
        aNode := TreePlan.FindNodeByText(aCont, TreePlanCONT);
        if Assigned(aNode) then begin
          aNode.MakeVisible;
          aNode.Focused := True;
        end;
      end;
      Result := ShowModal = mrOk;
      if Result then begin
        aNode := TcxDBTreeListNode(TreePlan.FocusedNode);
        if (NumaiFrunze) and (aNode <> nil) and (aNode.HasChildren) then raise EContaHandledError.Create('Contul selectat trebuie sa fie un analitic ! Va rugam refaceti selectia !');
        if (aNode <> nil) then CodCurent := aNode.Texts[TreePlanCONT.ItemIndex];
      end;
    finally
      frmPlanConturi.Free;
    end;
end;

procedure TFrmPlanConturi.Cmd_DeleteContCurentExecute(Sender: TObject);
var
  lNode : TcxDBTreeListNode;
  aCont, aParentCont : Variant;
begin
  lNode := TcxDBTreeListNode(TreePlan.FocusedNode);
  if lNode = nil then Exit;
  aCont := lNode.KeyValue;
  if (MessageDlg(Format('Doriti stergerea contului %s ?', [aCont]),  mtConfirmation, [mbYes, mbNo], 0) <> mrYes) then
       Abort;
  TestContCurent(aCont, 'stergerea', True);
  aParentCont := Null;
  if lNode.Parent <> nil then aParentCont := lNode.ParentKeyValue;
  if frmData.QryPlanCont.Locate('CONT', aCont, []) then frmData.QryPlanCont.Delete;
  frmData.QryPlanCont.Locate('CONT', aParentCont, []);
end;


procedure TFrmPlanConturi.TestContCurent(aCont: String;
    aOperatie : String; const inAnalitic : Boolean);
begin
  if ValueSafeToInt( DBGetScallarFmt('exec [sp_ContTestUse] %s, %s', [ValueToStr(aCont), ValueToStr(inAnalitic)]) ) <> 0  then
    if IsAdmin then begin
      if (MessageDlg(Format('Acest cont este folosit in cadrul aplicatie! Sunteti siguri ca doriti %s contului %s ?', [aOperatie, aCont]),  mtError, [mbYes, mbNo], 0) <> mrYes) then
        Abort;
    end else begin
        ShowEroare('Acest cont este folosit in cadrul aplicatie! Numai administratorul poate sterge un cont folosit !' );
        Abort;
    end;
end;

procedure TFrmPlanConturi.InternalExpand(Sender: TObject);
var
  I: Integer;

  procedure ExpandInner(aNode: TcxTreeListNode; Level: Integer);
   var
    J : Integer;
   begin
     if Level > 0 then begin
        aNode.Expand(False);
        for J := 0 to aNode.Count-1 do
          ExpandInner(aNode.Items[J], Level-1);
     end;
   end;

begin
  with TToolButton(Sender) do begin
    TreePlan.BeginUpdate;
    TreePlan.FullCollapse;
    try
       for I := 0 to TreePlan.Count-1 do
         ExpandInner(TreePlan.Items[I], Tag-1);
    finally
       TreePlan.EndUpdate;
    end;
  end;
end;




procedure TFrmPlanConturi.CreateLevelButtons;
var I: Integer;
    aToolButton : TToolButton;
begin
  { Stergem butoanele anterioare }
  for I := ExpandLevels.ButtonCount-1 downto 0 do
    ExpandLevels.Buttons[I].Free;
  { Creem noile Butoane }
  for I := FMaxLevel+1 downto 1 do begin
    aToolButton := TToolButton.Create(ExpandLevels);
    aToolButton.Style := tbsCheck;
    aToolButton.Tag := I;
    aToolButton.Grouped := True;
    aToolButton.Caption := IntToStr(I);
    aToolButton.OnClick := InternalExpand;
    TCrackToolButton(aToolButton).SetToolBar(ExpandLevels);
  end;
  ExpandLevels.Width := (FMaxLevel + 1) * ExpandLevels.ButtonWidth;
end;

procedure TFrmPlanConturi.AssignUnitate;
begin
  vContInfoUnitate.Visible := DBProcExists('spContaGetListUnitati');
  if vContInfoUnitate.Visible then DBRefresh(qryUnitate);
end;

procedure TFrmPlanConturi.ContUnitateDrawValue(Sender: TdxInspectorRow;
  ACanvas: TCanvas; ARect: TRect; var AText: String; AFont: TFont;
  var AColor: TColor; var ADone: Boolean);
var
  aNode : TdxTreeListNode;
begin
  if not vContInfoUnitate.Visible then Exit;
  aNode := TreeUnitate.FindNodeByKeyValue(frmData.QryPlanCont.FieldByName('ID_UNITATE').AsInteger);
  if aNode <> nil then AText := aNode.Strings[TreeUnitateDenumire.Index];
end;

procedure TFrmPlanConturi.ContUnitatePopup(Sender: TObject;
  const EditText: String);
var aNode : TdxTreeListNode;
begin
  aNode := TreeUnitate.FindNodeByKeyValue(frmData.QryPlanCont.FieldByName('ID_UNITATE').AsInteger);
  if aNode <> nil then begin
    aNode.Focused := True;
    aNode.MakeVisible;
  end;
end;

procedure TFrmPlanConturi.TreeUnitateDblClick(Sender: TObject);
begin
  with TdxDBTreeList(Sender) do
    if (FocusedNode <> nil) (* and (not FocusedNode.HasChildren) *) then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
end;

procedure TFrmPlanConturi.TreeUnitateKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if ((Key in [VK_UP, VK_DOWN]) and (ssAlt in Shift)) or
      ((Key = VK_F4) and not (ssAlt in Shift)) or (Key = VK_ESCAPE) then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(False);
  if (Key = VK_RETURN) and (TdxDBTreeList(Sender).FocusedNode <> nil)
     (*and (not TdxDBTreeList(Sender).FocusedNode.HasChildren)*) then
     (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
end;

procedure TFrmPlanConturi.ContUnitateCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
var
  lId : Variant;
begin
  if Accept then begin
    lId := TreeUnitate.FocusedNode.Values[TreeUnitateid.Index];
    Text := TreeUnitate.FocusedNode.Strings[TreeUnitateDenumire.Index];
    DBSetFieldValue(frmData.QryPlanCont, 'ID_UNITATE', lId);
  end;
end;

procedure TFrmPlanConturi.TreePlanFocusedNodeChanged(Sender: TcxCustomTreeList;
  APrevFocusedNode, AFocusedNode: TcxTreeListNode);
begin
  if FOnlyChild then
    BtnOk.Enabled := (Assigned(AFocusedNode) and not(AFocusedNode.HasChildren))
end;

end.
