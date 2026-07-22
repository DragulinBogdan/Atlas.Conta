unit CommonRepository;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxEdit, ComCtrls, cxEditRepositoryItems, cxGraphics, cxControls,
  cxLookAndFeels, cxLookAndFeelPainters, cxCustomData, cxStyles, cxTL,
  cxMaskEdit, cxTLdxBarBuiltInMenu, cxInplaceContainer, cxDBTL, cxTLData, cxDropDownEdit,
  cxContainer, cxTextEdit, cxDBEdit, DB, dxmdaset, cxFilter, cxData,
  cxDataStorage, cxDBData, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGridLevel, cxClasses, cxGridCustomView, cxGrid,
  cxVGrid, cxDBVGrid, cxCheckBox, cxImageComboBox, cxLookupEdit,
  cxDBLookupEdit, cxDBLookupComboBox, cxDBEditRepository, cxDisplayPopupEdit,
  cxCalendar, ZAbstractRODataset, ZAbstractDataset, ZDataset,
  ImgList, cxNavigator, cxDataControllerConditionalFormattingRulesManagerDialog,
  dxDateRanges, dxScrollbarAnnotations;

type
  TfrmRepo = class(TForm)
    EditRepo: TcxEditRepository;
    PageControl1: TPageControl;
    TabSheet1: TTabSheet;
    EditRepoCont: TcxEditRepositoryPopupItem;
    TreePlan: TcxDBTreeList;
    TreePlanDescriere: TcxDBTreeListColumn;
    TreePlanCONT: TcxDBTreeListColumn;
    TreePlanROMANA: TcxDBTreeListColumn;
    TreePlanFCTCONT: TcxDBTreeListColumn;
    TreePlanSID: TcxDBTreeListColumn;
    TreePlanSIC: TcxDBTreeListColumn;
    dxMemData1: TdxMemData;
    dxMemData1CONT: TStringField;
    TabSheet2: TTabSheet;
    TabSheet3: TTabSheet;
    TabSheet4: TTabSheet;
    TreeRepartitori: TcxDBTreeList;
    TreeRepartitoriNUME: TcxDBTreeListColumn;
    TreeRepartitoriADRESA: TcxDBTreeListColumn;
    TreeRepartitoriCONT_CRSP: TcxDBTreeListColumn;
    TreeRepartitoriCODFISC: TcxDBTreeListColumn;
    TreeRepartitoriGESTINT: TcxDBTreeListColumn;
    TreeRepartitoriTIP_GESTIUNE: TcxDBTreeListColumn;
    EditRepoRepartitor: TcxEditRepositoryPopupItem;
    cxGrid2DBTableView1: TcxGridDBTableView;
    cxGrid2Level1: TcxGridLevel;
    cxGrid2: TcxGrid;
    cxGrid2DBTableView1ID_REPARTITORI: TcxGridDBColumn;
    cxGrid2DBTableView1CODSECTIE: TcxGridDBColumn;
    cxGrid2DBTableView1NUME: TcxGridDBColumn;
    cxGrid2DBTableView1ADRESA: TcxGridDBColumn;
    cxGrid2DBTableView1CONT: TcxGridDBColumn;
    cxGrid2DBTableView1CONT_CEC: TcxGridDBColumn;
    cxGrid2DBTableView1BANCA: TcxGridDBColumn;
    cxGrid2DBTableView1CODCLASM: TcxGridDBColumn;
    cxGrid2DBTableView1COD_FISCAL: TcxGridDBColumn;
    TabSheet5: TTabSheet;
    EditRepoTipValuta: TcxEditRepositoryImageComboBoxItem;
    EditRepoUtilizator: TcxEditRepositoryImageComboBoxItem;
    EditRepoLookupRepartitor: TcxEditRepositoryLookupComboBoxItem;
    EditRepoLookupVama: TcxEditRepositoryLookupComboBoxItem;
    TabSheet6: TTabSheet;
    TreeTipMaterial: TcxDBTreeList;
    TreeTipMaterialDESCRIERE: TcxDBTreeListColumn;
    TreeTipMaterialID_GEST_TIP_MATERIAL: TcxDBTreeListColumn;
    TreeTipMaterialID_PARINTE: TcxDBTreeListColumn;
    EditRepoTipMaterial: TcxEditRepositoryDisplayPopupItem;
    TabSheet7: TTabSheet;
    cxTreeStructura: TcxDBTreeList;
    cxTreeStructuraCOD_CB: TcxDBTreeListColumn;
    cxTreeStructuraCOD_PARINTE: TcxDBTreeListColumn;
    cxTreeStructuraDENUMIRE: TcxDBTreeListColumn;
    cxTreeStructuraDENV: TcxDBTreeListColumn;
    cxTreeStructuraC_O: TcxDBTreeListColumn;
    cxTreeStructuraDATA_SOLD: TcxDBTreeListColumn;
    cxTreeStructuraCASIER: TcxDBTreeListColumn;
    cxTreeStructuraVALIDATOR: TcxDBTreeListColumn;
    cxTreeStructuraADMIN: TcxDBTreeListColumn;
    cxTreeStructuraIS_BANCA: TcxDBTreeListColumn;
    cxTreeStructuraIS_AVANS: TcxDBTreeListColumn;
    cxTreeStructuraIS_TEMPOR: TcxDBTreeListColumn;
    cxTreeStructuraID_REPARTITORI: TcxDBTreeListColumn;
    cxTreeStructuraICON: TcxDBTreeListColumn;
    cxTreeStructuraID_VALUTA: TcxDBTreeListColumn;
    cxTreeStructuraDESCRIERE: TcxDBTreeListColumn;
    cxTreeStructuraCRSP_LEI: TcxDBTreeListColumn;
    DTStructure: TDataSource;
    QryStructure: TZQuery;
    ImagesStructura: TImageList;
    EditRepoCBTStructura: TcxEditRepositoryPopupItem;
    cxTreeFunctional: TcxDBTreeList;
    cxTreeFunctionalCOD_FUNCTIONAL: TcxDBTreeListColumn;
    cxTreeFunctionalID_BG_TIPURI_BUGET: TcxDBTreeListColumn;
    cxTreeFunctionalID_OI_UNITATI: TcxDBTreeListColumn;
    cxTreeFunctionalID_BG_PLAN_FUNCTIONAL: TcxDBTreeListColumn;
    cxTreeFunctionalDENUMIRE: TcxDBTreeListColumn;
    cxTreeFunctionalDESCRIERE: TcxDBTreeListColumn;
    cxTreeFunctionalNUMAR_RAND: TcxDBTreeListColumn;
    cxTreeFunctionalID_PARINTE: TcxDBTreeListColumn;
    cxTreeFunctionalCLASA: TcxDBTreeListColumn;
    cxTreeFunctionalCAPITOL: TcxDBTreeListColumn;
    cxTreeFunctionalESTE_LUCRARE: TcxDBTreeListColumn;
    cxTreeFunctionalTIP_BUGET: TcxDBTreeListColumn;
    cxTreeFunctionalESTE_STANDARD: TcxDBTreeListColumn;
    cxTreeProiecte: TcxDBTreeList;
    cxTreeProiecteID_OI_PROIECTE: TcxDBTreeListColumn;
    cxTreeProiecteID_OI_TIPURI_PROIECTE: TcxDBTreeListColumn;
    cxTreeProiecteID_PARINTE: TcxDBTreeListColumn;
    cxTreeProiecteDENUMIRE: TcxDBTreeListColumn;
    cxTreeProiecteDESCRIERE: TcxDBTreeListColumn;
    cxTreeProiecteSTARE: TcxDBTreeListColumn;
    cxTreeProiecteCOD_FUNCTIONAL: TcxDBTreeListColumn;
    cxTreeUnitati: TcxDBTreeList;
    cxTreeUnitatiID_OI_UNITATI: TcxDBTreeListColumn;
    cxTreeUnitatiID_OI_UNITATI_TIPURI: TcxDBTreeListColumn;
    cxTreeUnitatiID_PARINTE: TcxDBTreeListColumn;
    cxTreeUnitatiDENUMIRE: TcxDBTreeListColumn;
    cxTreeUnitatiDESCRIERE: TcxDBTreeListColumn;
    cxTreeUnitatiUNITATATEA_URMARITA: TcxDBTreeListColumn;
    cxTreeUnitatiNUME_ORDONANTATOR: TcxDBTreeListColumn;
    cxTreeUnitatiID_UTILIZATORI: TcxDBTreeListColumn;
    cxTreeUnitatiSTARE: TcxDBTreeListColumn;
    cxTreeUnitatiUNITATEA_CENTRALIZATOARE: TcxDBTreeListColumn;
    cxTreeUnitatiBANCA: TcxDBTreeListColumn;
    cxTreeUnitatiBANCA_COD: TcxDBTreeListColumn;
    cxTreeUnitatiBANCA_CONT: TcxDBTreeListColumn;
    cxTreeUnitatiCOD_FUNCTIONAL: TcxDBTreeListColumn;
    cxTreeEconomic: TcxDBTreeList;
    cxTreeEconomicID_BG_PLAN_ECONOMIC: TcxDBTreeListColumn;
    cxTreeEconomicCOD_ECONOMIC: TcxDBTreeListColumn;
    cxTreeEconomicDENUMIRE: TcxDBTreeListColumn;
    cxTreeEconomicDESCRIERE: TcxDBTreeListColumn;
    cxTreeEconomicNUMAR_RAND: TcxDBTreeListColumn;
    cxTreeEconomicID_PARINTE: TcxDBTreeListColumn;
    cxTreeEconomicCLASA: TcxDBTreeListColumn;
    cxTreeEconomicESTE_LOCAL: TcxDBTreeListColumn;
    EditRepoTreeFunctional: TcxEditRepositoryPopupItem;
    EditRepoTreeEconomic: TcxEditRepositoryPopupItem;
    EditRepoTreeProiecte: TcxEditRepositoryDisplayPopupItem;
    EditRepoTreeUnitati: TcxEditRepositoryDisplayPopupItem;
    EditRepoLookupProiecte: TcxEditRepositoryLookupComboBoxItem;
    EditRepoLookupUnitati: TcxEditRepositoryLookupComboBoxItem;
    procedure TreePlanCustomDrawDataCell(Sender: TcxCustomTreeList;
      ACanvas: TcxCanvas; AViewInfo: TcxTreeListEditCellViewInfo;
      var ADone: Boolean);
    procedure TreePlanDblClick(Sender: TObject);
    procedure TreePlanKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure EditRepoContPropertiesCloseUp(Sender: TObject);
    procedure EditRepoContPropertiesPopup(Sender: TObject);
    procedure TreePlanDescriereGetDisplayText(Sender: TcxTreeListColumn;
      ANode: TcxTreeListNode; var Value: String);
    procedure TreeRepartitoriCustomDrawDataCell(Sender: TcxCustomTreeList;
      ACanvas: TcxCanvas; AViewInfo: TcxTreeListEditCellViewInfo;
      var ADone: Boolean);
    procedure EditRepoTipValutaPropertiesInitPopup(Sender: TObject);
    procedure EditRepoUtilizatorPropertiesInitPopup(Sender: TObject);
    procedure EditRepoTipMaterialPropertiesCustomDisplayValue(Sender: TObject;
      APopupControl: TControl; const AEditValue: Variant;
      var DisplayValue: Variant);
    procedure cxTreeEconomicDESCRIEREGetDisplayText(Sender: TcxTreeListColumn;
      ANode: TcxTreeListNode; var Value: string);
    procedure cxTreeFunctionalDESCRIEREGetDisplayText(Sender: TcxTreeListColumn;
      ANode: TcxTreeListNode; var Value: string);
  private
    { Private declarations }
  public
    { Public declarations }
    function GetDescriereOnTree(aTree : TcxDBTreeList; aId : Variant) : string;
  end;


procedure InitRepository;
procedure UninitRepository;

var
  LocalfrmRepo: TfrmRepo = nil;

function frmRepo: TfrmRepo;


implementation

uses
  dateUnit, dxCompsUtile, CommonDBVar;

{$R *.dfm}

function frmRepo : TfrmRepo;
begin
   if LocalfrmRepo = nil then
     LocalfrmRepo := TfrmRepo.Create(nil);
   Result := LocalfrmRepo;
end;


procedure TfrmRepo.TreePlanCustomDrawDataCell(Sender: TcxCustomTreeList;
  ACanvas: TcxCanvas; AViewInfo: TcxTreeListEditCellViewInfo;
  var ADone: Boolean);
var s: String;
begin
  if AViewInfo.Node.HasChildren then ACanvas.Font.Color := clGray;
  if AViewInfo.Column = TreePlanFCTCONT then
    if AViewInfo.Node <> nil then begin
      s := AViewInfo.Node.Values[TreePlanFCTCONT.ItemIndex];
      if s = 'B' then ACanvas.Brush.Color := clSkyBlue
      else if s = 'C' then ACanvas.Brush.Color := clLime
           else if s = 'D' then ACanvas.Brush.Color := clFuchsia;
      ACanvas.FillRect(AViewInfo.BoundsRect);
      ADone := False;
    end;
end;

procedure TfrmRepo.TreePlanDblClick(Sender: TObject);
var
  lTree : TcxDBTreeList;
begin
  lTree := TcxDBTreeList(Sender);
  if (lTree.FocusedNode <> nil) and ((lTree.Tag = 0) or not lTree.FocusedNode.HasChildren) then
     GetParentForm(lTree).ModalResult := mrOk;
//  if GetParentForm(lTree) is TcxPopupEditPopupWindow then
//        (GetParentForm(lTree) as TcxPopupEditPopupWindow).ModalResult := mrOk;
end;

procedure TfrmRepo.TreePlanKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
     TreePlanDblClick(TcxDBTreeList(Sender))
  else if Key = VK_ESCAPE then
   GetParentForm(TcxDBTreeList(Sender)).ModalResult := mrCancel;
end;

procedure TfrmRepo.cxTreeEconomicDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: string);
begin
  Value := ANode.Values[cxTreeEconomicCOD_ECONOMIC.ItemIndex] + ': '+ ANode.Values[cxTreeEconomicDENUMIRE.ItemIndex];
end;

procedure TfrmRepo.cxTreeFunctionalDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: string);
begin
  Value := ANode.Values[cxTreeFunctionalCOD_FUNCTIONAL.ItemIndex] + ': '+ANode.Values[cxTreeFunctionalDENUMIRE.ItemIndex];
end;

type
  TcxCrackCustomPopupEdit = class(TcxCustomPopupEdit);

procedure TfrmRepo.EditRepoContPropertiesCloseUp(Sender: TObject);
var
  lNode : TcxDBTreeListNode;
  lIdField : TcxDBTreeListColumn;
  lTree  : TcxDBTreeList;
  lValue : Variant;
  lDataSet : TDataSet;
  lEditable : Boolean;
  lProperties : TcxCustomEditProperties;
begin
  if  not Sender.InheritsFrom(TcxCustomPopupEdit) then Exit;
  lProperties := TcxCustomPopupEdit(Sender).ActiveProperties;
  if not (lProperties is TcxPopupEditProperties) then Exit;
  if not (TcxPopupEditProperties(lProperties).PopupControl is TcxDBTreeList) then Exit;
  {nu trebuie pus ImmediatePost pe true pentru ca la validare daca este in editare
    va face intai post la textedit si apoi la popup
  }
  //TcxPopupEditProperties(lProperties).ImmediatePost := False;
  lTree := TcxDBTreeList(TcxPopupEditProperties(lProperties).PopupControl);
  //cautam coloana cu tag -1 ca default in caz ca nu o gasim luam valoarea din keyfield
  lIdField   := cxFindColumnByTag(lTree, -1);
  with TcxCrackCustomPopupEdit(Sender) do
    if PopupWindow.ModalResult = mrOk then begin
       lNode := TcxDBTreeListNode(lTree.FocusedNode);
       if Assigned(lNode) then begin
          if lIdField = nil then lValue := lNode.KeyValue
                            else lValue := lNode.Values[lIdField.ItemIndex];
          if Sender is TcxDBPopupEdit then begin
                    lDataSet := TcxDBPopupEdit(Sender).DataBinding.Field.DataSet;
                    lEditable := lDataSet.State in [dsEdit, dsInsert];
                    if not lEditable then lDataSet.Edit;
                    TcxDBPopupEdit(Sender).DataBinding.Field.Value := lValue;
                    if lEditable then begin
                      lDataSet.Post;
                      lDataSet.Edit;
                    end;
          end
          else begin
            TcxPopupEdit(Sender).EditValue := lValue;
            TcxPopupEdit(Sender).PostEditValue;
          end;
       end;
    end;
end;

procedure TfrmRepo.EditRepoContPropertiesPopup(Sender: TObject);
var
 lNode : TcxTreeListNode;
 lTree : TcxDBTreeList;
 lValue : Variant;
 lProperties : TcxCustomEditProperties;
begin
  if  not Sender.InheritsFrom(TcxCustomPopupEdit) then Exit;
  lProperties := TcxCustomPopupEdit(Sender).ActiveProperties;
  if not (lProperties is TcxPopupEditProperties) then Exit;
  if not (TcxPopupEditProperties(lProperties).PopupControl is TcxDBTreeList) then Exit;
  lTree := TcxDBTreeList(TcxPopupEditProperties(lProperties).PopupControl);
  if (Sender is TcxDBPopupEdit) then begin
     if TcxDBPopupEdit(Sender).DataBinding.Field <> nil then lValue := TcxDBPopupEdit(Sender).DataBinding.Field.Value
                                                        else lValue := null;
  end
  else if Sender is TcxPopupEdit then lValue := TcxPopupEdit(Sender).EditValue;
  lNode := lTree.FindNodeByKeyValue(lValue);
  if lNode <> nil then begin
    {numai daca vrem analitice}
    while lNode.Count > 0 do lNode := lNode.Items[0];
    lNode.Focused := True;
    lNode.MakeVisible;
    lTree.SearchingText := lValue;
  end;
end;

procedure TfrmRepo.TreePlanDescriereGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  if ANode = nil then Exit;
  Value := ANode.Texts[TreePlanCONT.ItemIndex] + ' :  ' + ANode.Texts[TreePlanROMANA.ItemIndex];
end;

procedure TfrmRepo.TreeRepartitoriCustomDrawDataCell(
  Sender: TcxCustomTreeList; ACanvas: TcxCanvas;
  AViewInfo: TcxTreeListEditCellViewInfo; var ADone: Boolean);
var
  IsIntern: Boolean;
begin
  IsIntern :=  GetBoolean(AViewInfo.Node.Values[TreeRepartitoriGESTINT.ItemIndex]);

  if IsIntern then ACanvas.Font.Color := clBlue
              else ACanvas.Font.Color := clRed;

end;



procedure InitRepository;
begin
end;

procedure UninitRepository;
begin
  if LocalfrmRepo <> nil then
    FreeAndNil(LocalfrmRepo);
end;


procedure TfrmRepo.EditRepoTipMaterialPropertiesCustomDisplayValue(
  Sender: TObject; APopupControl: TControl; const AEditValue: Variant;
  var DisplayValue: Variant);
var
  lNode : TcxDBTreeListNode;
  lDescField  : TcxDBTreeListColumn;
  lTree  : TcxDBTreeList;
begin
  if not (APopupControl is TcxDBTreeList) then Exit;
  lTree := TcxDBTreeList(APopupControl);
  if VarIsNull(AEditValue)
    //or not (VarIsNumeric(AEditValue) = VarIsNumeric(lTree.RootValue))
  then
  begin
    DisplayValue := '';
    Exit;
  end;
  lDescField   := cxFindColumnByTag(lTree, -2);
  lNode := TcxDBTreeListNode(lTree.FindNodeByKeyValue(AEditValue));
  if Assigned(lNode) then begin
      if lDescField = nil then DisplayValue := VarToStr(AEditValue)
                         else  DisplayValue := VarToStr(lNode.Texts[lDescField.ItemIndex]);
  end;
end;

procedure TfrmRepo.EditRepoTipValutaPropertiesInitPopup(Sender: TObject);
begin
  if EditRepoTipValuta.Properties.Items.Count = 0 then
    FillImageCombo(EditRepoTipValuta.Properties, 'spNmclValute', 0, 1);
end;

procedure TfrmRepo.EditRepoUtilizatorPropertiesInitPopup(Sender: TObject);
begin
  if EditRepoUtilizator.Properties.Items.Count = 0 then
    FillImageCombo(EditRepoUtilizator.Properties, frmData.QryOperatori, 'ID_UTILIZATORI', 'NUMEINTREG', True, 'Toti Utilizatorii');
end;

function TfrmRepo.GetDescriereOnTree(aTree: TcxDBTreeList;
  aId: Variant): string;
var
  lColumn : TcxTreeListColumn;
  lNode : TcxDBTreeListNode;
  I : Integer;
begin
  lColumn := nil;
  lNode := aTree.FindNodeByKeyValue(aId);
  if lNode <> nil then begin
    for I := 0 to aTree.ColumnCount - 1 do
      if aTree.Columns[I].Tag = -2 then begin
        lColumn := aTree.Columns[I];
        Break;
      end;

    if lColumn <> nil then
      Result := lNode.Texts[lColumn.ItemIndex]
    else
      Result := VarToStr(lNode.KeyValue);
  end
  else Result := '';
end;

initialization

finalization
  UninitRepository;
end.
