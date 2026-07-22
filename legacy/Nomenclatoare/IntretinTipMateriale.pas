unit IntretinTipMateriale;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxLookAndFeelPainters, StdCtrls, cxButtons, ExtCtrls, AppEvnts,
  cxGraphics,
  cxStyles, cxTL,
  cxControls, cxInplaceContainer, cxTLData, cxDBTL, DB, ZDataSet, cxMaskEdit,
  cxCheckBox, cxContainer, cxEdit, cxClasses, ImgList, cxTextEdit,
  cxSpinEdit, cxImageComboBox, cxDataStorage, cxDBData,
  cxGridCustomView, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGrid, cxGridCardView, cxGridBandedTableView,
  cxGridDBBandedTableView, DBCtrls, cxDropDownEdit, cxDBEdit,
  cxGroupBox, cxButtonEdit, Menus, 
  cxSplitter, DegradePanel,dxDBTL, dxTL,
  ZAbstractRODataset, ZAbstractDataset,
  cxTLdxBarBuiltInMenu,
  cxLookAndFeels, cxCustomData, cxCurrencyEdit,
  cxDataControllerConditionalFormattingRulesManagerDialog,
  dxScrollbarAnnotations;

type
  TCrackAtsTree = class(TdxDBTreeList);
  TfrmIntretinereTipMat = class(TForm)
    pnBottom: TPanel;
    btnOk: TcxButton;
    pnContent: TPanel;
    pnLeft: TPanel;
    DTProdus: TDataSource;
    QryTipProdus: TZQuery;
    DTTipMaterial: TDataSource;
    QryTipMaterial: TZQuery;
    TreeTipProdus: TcxTreeList;
    cxStyleRepository1: TcxStyleRepository;
    TreeListStyleSheetStormVGA: TcxTreeListStyleSheet;
    cxStyle1: TcxStyle;
    cxStyle2: TcxStyle;
    cxStyle3: TcxStyle;
    cxStyle4: TcxStyle;
    cxStyle5: TcxStyle;
    cxStyle6: TcxStyle;
    cxStyle7: TcxStyle;
    cxStyle8: TcxStyle;
    cxStyle9: TcxStyle;
    cxStyle10: TcxStyle;
    cxStyle11: TcxStyle;
    CheckList: TImageList;
    TreeTipProdusDENUMIRE: TcxTreeListColumn;
    TreeTipProdusID_GEST_TIP_PRODUSE: TcxTreeListColumn;
    TreeTipProdusTIP_PRODUS: TcxTreeListColumn;
    TreeTipProdusSE_AFISEAZA: TcxTreeListColumn;
    TreeListStyleSheetUserFormat4: TcxTreeListStyleSheet;
    cxStyle12: TcxStyle;
    cxStyle13: TcxStyle;
    cxStyle14: TcxStyle;
    cxStyle15: TcxStyle;
    cxStyle16: TcxStyle;
    cxStyle17: TcxStyle;
    cxStyle18: TcxStyle;
    cxStyle19: TcxStyle;
    cxStyle20: TcxStyle;
    cxStyle21: TcxStyle;
    cxStyle22: TcxStyle;
    pnControl: TPanel;
    Label1: TLabel;
    edtDenumire: TcxDBTextEdit;
    Label2: TLabel;
    edtDescriere: TcxDBTextEdit;
    Label3: TLabel;
    edtIdGestTipMaterial: TcxDBTextEdit;
    Label4: TLabel;
    edtCodEconomic: TcxDBButtonEdit;
    Label6: TLabel;
    edtCont: TcxDBButtonEdit;
    Label8: TLabel;
    edtTipProdus: TcxDBImageComboBox;
    Label10: TLabel;
    edtContIesire: TcxDBButtonEdit;
    edtSeAfiseaza: TcxDBCheckBox;
    edtContIntrare: TcxDBButtonEdit;
    Label9: TLabel;
    Bevel1: TBevel;
    cxGroupBox: TcxGroupBox;
    TreeTipMat: TcxDBTreeList;
    TreeTipMatDENUMIRE: TcxDBTreeListColumn;
    TreeTipMatDESCRIERE: TcxDBTreeListColumn;
    TreeTipMatID_GEST_TIP_MATERIAL: TcxDBTreeListColumn;
    TreeTipMatCOD_ECONOMIC: TcxDBTreeListColumn;
    TreeTipMatMAN_ID: TcxDBTreeListColumn;
    TreeTipMatCONT: TcxDBTreeListColumn;
    TreeTipMatID_PARINTE: TcxDBTreeListColumn;
    TreeTipMatID_GEST_TIP_PRODUSE: TcxDBTreeListColumn;
    TreeTipMatCONT_INTRARE: TcxDBTreeListColumn;
    TreeTipMatCONT_IESIRE: TcxDBTreeListColumn;
    TreeTipMatSE_AFISEAZA: TcxDBTreeListColumn;
    Panel1: TPanel;
    btnAddTipMat: TcxButton;
    btnDelTipMat: TcxButton;
    NetscapeSplitter1: TcxSplitter;
    pnTop: TDegradePanel;
    btnImportPlan: TcxButton;
    chkAllTipProdus: TcxCheckBox;
    procedure TreeTipProdusMouseUp(Sender: TObject; Button: TMouseButton;
      Shift: TShiftState; X, Y: Integer);
    procedure FormCreate(Sender: TObject);
    procedure TreeTipProdusCustomDrawCell(Sender: TObject;
      ACanvas: TcxCanvas; AViewInfo: TcxTreeListEditCellViewInfo;
      var ADone: Boolean);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure chkAllTipProdusClick(Sender: TObject);
    procedure TreeTipMatDragOver(Sender, Source: TObject; X, Y: Integer;
      State: TDragState; var Accept: Boolean);
    procedure btnOkClick(Sender: TObject);
    procedure Label8DblClick(Sender: TObject);
    procedure btnAddTipMatClick(Sender: TObject);
    procedure edtCodEconomicPropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
    procedure edtContIntrarePropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
    procedure btnDelTipMatClick(Sender: TObject);
    procedure btnImportPlanClick(Sender: TObject);
    procedure pnBottomResize(Sender: TObject);
    procedure TreeTipProdusCustomDrawDataCell(Sender: TcxCustomTreeList;
      ACanvas: TcxCanvas; AViewInfo: TcxTreeListEditCellViewInfo;
      var ADone: Boolean);
    procedure TreeTipProdusGetNodeImageIndex(Sender: TcxCustomTreeList;
      ANode: TcxTreeListNode; AIndexType: TcxTreeListImageIndexType;
      var AIndex: TImageIndex);
  private
    { Private declarations }
  public
    { Public declarations }
    function GetFirstTipProdus: Integer;
    procedure SetFilterTipProdus(aTipProdus : String; cAdd : Boolean);
    procedure PopulateTree;
    procedure PopulateProduse;
    procedure ActualizeFilter;
    procedure LocalAddNode(aParentId : Integer);
  end;

var
  frmIntretinereTipMat: TfrmIntretinereTipMat;

implementation

uses
  ZeosDBUtile, DateUnit, PlanConturiUnit, SelBugetUnit, CommonDBVar, StrUtils;

{$R *.dfm}

function TfrmIntretinereTipMat.GetFirstTipProdus: Integer;
var
  i: Integer;
  lNode: TcxTreeListNode;
begin
  for i := 0 to TreeTipProdus.Count - 1 do begin
    lNode := TreeTipProdus.Items[i];
    if lNode.ImageIndex = 1 then begin
      Result := lNode.Values[TreeTipProdusID_GEST_TIP_PRODUSE.ItemIndex];
    end;
  end;    
end;

procedure TfrmIntretinereTipMat.SetFilterTipProdus(aTipProdus: String; cAdd: Boolean);
  function FormatFilter : String;
  var
     lstFLT : TStringList;
     I : Integer;
  begin
    if chkAllTipProdus.Checked then
      Result := ''
    else
    if aTipProdus = '' then
          Result := '(ID_GEST_TIP_PRODUSE = -1)'
    else
      if pos(',', aTipProdus) > 0 then begin
        lstFLT := TStringList.Create;
        lstFLT.CommaText := aTipProdus;
        Result := '';
        for I := 0 to lstFLT.Count -1 do
          if Result = '' then
            Result := '(ID_GEST_TIP_PRODUSE = ' + lstFLT[I] +')'
          else
            Result := Result +  ' OR (ID_GEST_TIP_PRODUSE = ' + lstFLT[I] +')';
        lstFLT.Free;
      end
      else
        Result := 'ID_GEST_TIP_PRODUSE = ' + aTipProdus;
  end;

var lFilter : String;
begin
  //
  if cAdd then
    if aTipProdus = '' then begin
      QryTipMaterial.Filtered := False;
      QryTipMaterial.Filter := '';
    end
    else begin
      lFilter := QryTipMaterial.Filter;
      if lFilter = '' then
        lFilter := FormatFilter
      else
        lFilter := lFilter + ' OR ' + FormatFilter;
      QryTipMaterial.Filter := lFilter;
      if not QryTipMaterial.Filtered then QryTipMaterial.Filtered := True;
    end
  else
    //if (aTipProdus <> '')  then
    begin
      QryTipMaterial.Filter := FormatFilter;
      if not QryTipMaterial.Filtered then QryTipMaterial.Filtered := True;
    end;
end;

procedure TfrmIntretinereTipMat.TreeTipProdusMouseUp(Sender: TObject;
  Button: TMouseButton; Shift: TShiftState; X, Y: Integer);
var
  aHitInfo : TcxTreeListHitTest;
  aNode : TcxTreeListNode;
  lPrevEvent: TNotifyEvent;
begin
  aHitInfo := TreeTipProdus.HitTest;
  if aHitInfo <> nil then
    if aHitInfo.HitAtImage then begin
      aNode := aHitInfo.HitNode;
      if aNode <> nil then begin
        if aNode.ImageIndex = 1 then begin
          aNode.ImageIndex := 0;

          // Dezactivam manual evenimentele cand schimbam valoarea
          lPrevEvent := chkAllTipProdus.OnClick;
          chkAllTipProdus.OnClick := nil;
          chkAllTipProdus.Checked := False;
          chkAllTipProdus.OnClick := lPrevEvent;
        end
        else begin
          aNode.ImageIndex := 1;
        end;

        ActualizeFilter;
      end;
    end;
end;

procedure TfrmIntretinereTipMat.FormCreate(Sender: TObject);
begin
  if QryTipProdus.Active then QryTipProdus.Close;
  QryTipProdus.Open;
  DBRefresh(QryTipMaterial);
  ActualizeFilter;
  PopulateProduse;
  PopulateTree;
  chkAllTipProdus.Checked := True;
  chkAllTipProdusClick(nil);
end;

procedure TfrmIntretinereTipMat.PopulateTree;
var
  aNode : TcxTreeListNode;
begin
  TreeTipProdus.Clear;
  with QryTipProdus do
  try
    First;
    while not eof do begin
      aNode := TreeTipProdus.Add;
      aNode.Values[TreeTipProdusDENUMIRE.ItemIndex] := FieldByName('DENUMIRE').AsString;
      aNode.Values[TreeTipProdusID_GEST_TIP_PRODUSE.ItemIndex] := FieldByName('ID_GEST_TIP_PRODUSE').AsInteger;
      aNode.Values[TreeTipProdusTIP_PRODUS.ItemIndex] := FieldByName('TIP_PRODUS').AsString;
      aNode.Values[TreeTipProdusSE_AFISEAZA.ItemIndex] := FieldByName('SE_AFISEAZA').AsBoolean;
      aNode.ImageIndex := 0;
      Next;
    end;
  finally
    Close;
  end;
end;

procedure TfrmIntretinereTipMat.TreeTipProdusCustomDrawCell(
  Sender: TObject; ACanvas: TcxCanvas;
  AViewInfo: TcxTreeListEditCellViewInfo; var ADone: Boolean);
begin
  if AViewInfo.Node <> nil then
   if AViewInfo.Node.Values[TreeTipProdusSE_AFISEAZA.ItemIndex] = True then
      ACanvas.Font.Style := ACanvas.Font.Style + [fsBold]
   else
      ACanvas.Font.Style := ACanvas.Font.Style - [fsBold];
end;

procedure TfrmIntretinereTipMat.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  if QryTipMaterial.State in [dsEdit,dsInsert] then QryTipMaterial.Post;

  Action := caFree;
end;

procedure TfrmIntretinereTipMat.ActualizeFilter;
var
  i: Integer;
  lNode: TcxTreeListNode;
  lId: Integer;
  lFilterString: String;
begin
  lFilterString := '';

  for i := 0 to TreeTipProdus.Count - 1 do begin
    lNode := TreeTipProdus.Items[i];
    if lNode.ImageIndex = 1 then begin
      lId := lNode.Values[TreeTipProdusID_GEST_TIP_PRODUSE.ItemIndex];

      if lFilterString = '' then
        lFilterString := IntToStr(lId)
      else
        lFilterString := lFilterString + ',' + IntToStr(lId);
    end;
  end;

  SetFilterTipProdus(lFilterString, False);
end;

procedure TfrmIntretinereTipMat.PopulateProduse;
var
 aItem : TcxImageComboBoxItem;
begin
  TcxImageComboBoxProperties(TreeTipMatID_GEST_TIP_PRODUSE.Properties).Items.Clear;

  with QryTipProdus do
  try
    First;
    while not eof do begin
      aItem := TcxImageComboBoxItem(TcxImageComboBoxProperties(TreeTipMatID_GEST_TIP_PRODUSE.Properties).Items.Add);
      aItem.Value := FieldByName('ID_GEST_TIP_PRODUSE').AsInteger;
      aItem.Description := FieldByName('DENUMIRE').AsString;
      Next;
    end;
  finally
    edtTipProdus.Properties.Assign(TcxImageComboBoxProperties(TreeTipMatID_GEST_TIP_PRODUSE.Properties));
  end;
end;

procedure TfrmIntretinereTipMat.chkAllTipProdusClick(Sender: TObject);
var
  aNode : TcxTreeListNode;
  I : Integer;
begin
  if not chkAllTipProdus.Checked then begin
    for I := 0 to TreeTipProdus.Count - 1 do begin
      aNode := TreeTipProdus.Items[I];
      aNode.ImageIndex := 0;
    end;
  end
  else begin
    for I := 0 to TreeTipProdus.Count - 1 do begin
      aNode := TreeTipProdus.Items[I];
      aNode.ImageIndex := 1;
    end;
  end;
  ActualizeFilter;
end;

procedure TfrmIntretinereTipMat.TreeTipMatDragOver(Sender, Source: TObject;
  X, Y: Integer; State: TDragState; var Accept: Boolean);
begin
//
end;

procedure TfrmIntretinereTipMat.btnOkClick(Sender: TObject);
begin
  if fsModal in FormState then ModalResult := mrOk
  else Close;
end;

procedure TfrmIntretinereTipMat.Label8DblClick(Sender: TObject);
begin
  TcxImageComboBoxProperties(TreeTipMatID_GEST_TIP_PRODUSE.Properties).Alignment.Horz := taLeftJustify;
  TcxImageComboBoxProperties(TreeTipMatID_GEST_TIP_PRODUSE.Properties).Alignment.Vert := taTopJustify;
end;

procedure TfrmIntretinereTipMat.btnAddTipMatClick(Sender: TObject);
//var aNode, aParentNode  : TCxTreeListNode;
begin
{  aNode := TreeTipMat.FocusedNode;
  if aNode <> nil then begin
  end;}
  LocalAddNode(0);
  //
end;

procedure TfrmIntretinereTipMat.LocalAddNode(aParentId: Integer);
var
  lFirstTipProdus: Integer;
begin
  with QryTipMaterial do
  try
    TreeTipMat.DataController.DataSet.DisableControls;
    QryTipMaterial.Append;
    QryTipMaterial.FieldByName('DESCRIERE').AsString := 'Material Nou';
    QryTipMaterial.FieldByName('DENUMIRE').AsString := 'Material Nou';
    QryTipMaterial.FieldByName('ID_PARINTE').Value := null;

    lFirstTipProdus := GetFirstTipProdus;
    if lFirstTipProdus > 0 then
      QryTipMaterial.FieldByName('ID_GEST_TIP_PRODUSE').AsInteger := lFirstTipProdus;

    QryTipMaterial.Post;
  finally
    TreeTipMat.DataController.DataSet.EnableControls;
  end;
end;

procedure TfrmIntretinereTipMat.edtCodEconomicPropertiesButtonClick(
  Sender: TObject; AButtonIndex: Integer);
var
  aRes, aCod : String;
  aId : Integer;
begin
  aCod := QryTipMaterial.FieldByName(TcxDBButtonEdit(Sender).DataBinding.DataField).AsString;
  aRes :=  NewSelectarePlanEconomic(aCod, aId, '', -1, True);
  if aRes <> '<Anulat>' then begin
    QryTipMaterial.Edit;
    QryTipMaterial.FieldByName(TcxDBButtonEdit(Sender).DataBinding.DataField).Value := aCod;
    QryTipMaterial.Post;
  end;
end;


procedure TfrmIntretinereTipMat.edtContIntrarePropertiesButtonClick(
  Sender: TObject; AButtonIndex: Integer);
var
  frmPlabConturi : TFrmPlanConturi;
  aNode : TcxDBTreeListNode;
  aCont : String;
begin
  aCont := QryTipMaterial.FieldByName(TcxDBButtonEdit(Sender).DataBinding.DataField).AsString;
  frmPlabConturi := TFrmPlanConturi.Create(nil);
  with frmPlabConturi do
    try
      Caption := 'Selectie cont';
      vContInfo.Visible := False;
      frmPlabConturi.Visible := False;
      TreePlan.PopupMenu := nil;
      TreePlan.ApplyBestFit();
      TreePlan.OptionsData.Editing := False;
      TreePlan.OptionsBehavior.IncSearch := True;
      if Trim(aCont) <> '' then begin
         aNode := TreePlan.FindNodeByKeyValue(aCont);
         if aNode <> nil then begin
           aNode.MakeVisible;
           aNode.Focused := True;
         end;
      end;
      ShowModal;
      if ModalResult = mrOk then begin
        aNode := TcxDBTreeListNode(TreePlan.FocusedNode);
        QryTipMaterial.Edit;
        QryTipMaterial.FieldByName(TcxDBButtonEdit(Sender).DataBinding.DataField).Value := aNode.KeyValue;
        if ((QryTipMaterial.FieldByName('DENUMIRE').AsString = 'Material Nou') and
          (QryTipMaterial.FieldByName('DESCRIERE').AsString = 'Material Nou'))
           or
           ((QryTipMaterial.FieldByName('DENUMIRE').AsString = '') and
          (QryTipMaterial.FieldByName('DESCRIERE').AsString = ''))

        then begin
          QryTipMaterial.FieldByName('DENUMIRE').Value := ValueToStr(aNode.KeyValue) + ' : ' +  Trim((GetNiceText(aNode.Texts[TreePlanROMANA.ItemIndex])));
          QryTipMaterial.FieldByName('DESCRIERE').Value :=  Trim((GetNiceText(aNode.Texts[TreePlanROMANA.ItemIndex])));
        end;
        QryTipMaterial.Post;
      end;
    finally
      frmPlabConturi.Free;
    end;
end;

procedure TfrmIntretinereTipMat.btnDelTipMatClick(Sender: TObject);
var
  aNode : TcxTreeListNode;
  I : Integer;
begin
  if TreeTipMat.SelectionCount <= 1 then begin
      aNode := TreeTipMat.FocusedNode;
      if aNode = nil then Exit;
      if (MessageDlg(Format('Doriti sa stergeti tipul de material %s ?', [String(aNode.Values[TreeTipMatDENUMIRE.ItemIndex])]), mtConfirmation, [mbYes, mbNo], 0) <> mrYes) then Abort;
      aNode.Delete;
  end
  else
  begin
   if (MessageDlg('Doriti sa stergeti tipurile de materiale selectate ?', mtConfirmation, [mbYes, mbNo], 0) <> mrYes) then Abort;
   try
     TreeTipMat.BeginUpdate;
     for I := TreeTipMat.SelectionCount - 1 downto 0 do begin
      aNode := TreeTipMat.Selections[I];
      if aNode = nil then Continue;
      aNode.Delete;
     end;
   finally
    TreeTipMat.EndUpdate;
   end;
  end;
end;

procedure TfrmIntretinereTipMat.btnImportPlanClick(Sender: TObject);

procedure AppendNode(lId : String; lDenumire : String; lParinte : String);
var
  lParentId : Integer;
begin
  lParentId := -1;
  if Trim(lParinte) <> '' then begin
    if QryTipMaterial.Locate('CONT', lParinte, []) then
      lParentId := QryTipMaterial.FieldByName('ID_GEST_TIP_MATERIAL').AsInteger;
  end;
  LocalAddNode(0);
  QryTipMaterial.Edit;
  QryTipMaterial.FieldByName('CONT').Value := lId;
  if lParentId <> -1 then
     QryTipMaterial.FieldByName('ID_PARINTE').AsInteger := lParentId;
  QryTipMaterial.FieldByName('DENUMIRE').Value := lId + ' : ' +  Trim((GetNiceText(lDenumire)));
  QryTipMaterial.FieldByName('DESCRIERE').Value :=  Trim((GetNiceText(lDenumire)));
  QryTipMaterial.Post;
end;

var
  frmPlabConturi : TFrmPlanConturi;
  aNode : TcxDBTreeListNode;
  I : Integer;
  NodeList : TStringList;
  lId, lNume, lParinte : String;
begin
  frmPlabConturi := TFrmPlanConturi.Create(nil);
  with frmPlabConturi do
    try
      Caption := 'Selectie multipla de conturi (Tineti apasa shift si selectati cu mouse-ul mai multe pozitii)';
      vContInfo.Visible := False;
      frmPlabConturi.Visible := False;
      TreePlan.PopupMenu := nil;
      TreePlan.ApplyBestFit();
      TreePlan.OptionsData.Editing := False;
      TreePlan.OptionsBehavior.IncSearch := True;
      TreePlan.OptionsSelection.MultiSelect := True;
      ShowModal;
      if ModalResult = mrOk then begin
        if TreePlan.SelectionCount >1  then begin
          NodeList := TStringList.Create;
          NodeList.Clear;
          NodeList.NameValueSeparator := '#';
          for I := 0 to TreePlan.SelectionCount - 1 do begin
            aNode := TcxDBTreeListNode(TreePlan.Selections[I]);
            NodeList.Add(VarToStr(aNode.KeyValue) + '#' + aNode.Texts[TreePlanROMANA.ItemIndex] +'|' + aNode.Texts[TreePlanPARINTE.ItemIndex]);
          end;
          NodeList.Sorted := True;
          for I := 0 to NodeList.Count - 1 do begin
            lId := NodeList.Names[I];
            lNume := NodeList.Values[lId];
            lParinte := RightStr(lNume, length(lNume) - pos('|', lNume));
            lNume := LeftStr(lNume, length(lNume) - length(lParinte) -1);
            AppendNode(lId, lNume, lParinte);
          end;
          NodeList.Free;
        end
        else begin
          aNode := TcxDBTreeListNode(TreePlan.FocusedNode);
          AppendNode(aNode.KeyValue, aNode.Texts[TreePlanROMANA.ItemIndex], aNode.Texts[TreePlanPARINTE.ItemIndex]);
        end;
      end;
    finally
      frmPlabConturi.Free;
    end;
end;


procedure TfrmIntretinereTipMat.pnBottomResize(Sender: TObject);
begin
   btnOk.Left := pnBottom.Width - btnOk.Width - 5;
end;

procedure TfrmIntretinereTipMat.TreeTipProdusCustomDrawDataCell(
  Sender: TcxCustomTreeList; ACanvas: TcxCanvas;
  AViewInfo: TcxTreeListEditCellViewInfo; var ADone: Boolean);
begin
  if AViewInfo.Node <> nil then
   if AViewInfo.Node.Values[TreeTipProdusSE_AFISEAZA.ItemIndex] = True then
      ACanvas.Font.Style := ACanvas.Font.Style + [fsBold]
   else
      ACanvas.Font.Style := ACanvas.Font.Style - [fsBold];
end;

procedure TfrmIntretinereTipMat.TreeTipProdusGetNodeImageIndex(
  Sender: TcxCustomTreeList; ANode: TcxTreeListNode;
  AIndexType: TcxTreeListImageIndexType; var AIndex: TImageIndex);
begin
 if AIndexType = tlitSelectedIndex then
   AIndex := ANode.ImageIndex
end;

end.
