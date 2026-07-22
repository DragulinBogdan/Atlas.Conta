unit BxPlanContainer;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxControls, cxContainer,
  cxEdit, cxGroupBox, cxLabel, ExtCtrls, cxGraphics,
  cxTL, cxInplaceContainer, cxTLData, cxDBTL,
  cxMaskEdit, cxCheckBox, cxRepartitorPanel, ZDataSet, cxLookAndFeelPainters,
  cxTLdxBarBuiltInMenu,
  cxLookAndFeels, cxCustomData, cxStyles, cxClasses,
  cxDataControllerConditionalFormattingRulesManagerDialog,
  dxScrollbarAnnotations;

type
  TBugetPlanType = (
    bptClasaFunctionala,        //1
    bptUnitateInterna,          //2
    bptUnitateExterna,          //-2
    bptBugetFunctional,         //3
    bptProiect,                 //4
    bptProiectBuget,            //5
    bptUnitateBugetFunctional,  //6
    bptEconomic,                //7
    bptFunctionalEconomic,      //8
    bptProiectFunctional,       //9
    bptFunctional,              //10
    bptEconomicUnitate          //11
  );

  TfrmBxPlanContainer = class(TForm)
    gr1: TcxGroupBox;
    cxLabel1: TcxLabel;
    RPFunctional1: TcxRepartitorPanel;
    gr2: TcxGroupBox;
    cxLabel2: TcxLabel;
    RPFunctional2: TcxRepartitorPanel;
    cxLabel3: TcxLabel;
    gr3: TcxGroupBox;
    cxLabel4: TcxLabel;
    RPFunctional3: TcxRepartitorPanel;
    cxLabel5: TcxLabel;
    RPTipBuget3: TcxRepartitorPanel;
    gr4: TcxGroupBox;
    cxLabel6: TcxLabel;
    RPProiect4: TcxRepartitorPanel;
    gr5: TcxGroupBox;
    cxLabel7: TcxLabel;
    RPProiect5: TcxRepartitorPanel;
    cxLabel8: TcxLabel;
    RPTipBuget5: TcxRepartitorPanel;
    gr6: TcxGroupBox;
    cxLabel9: TcxLabel;
    RPFunctional6: TcxRepartitorPanel;
    cxLabel10: TcxLabel;
    RPUnitate6: TcxRepartitorPanel;
    cxLabel11: TcxLabel;
    RPTipBuget6: TcxRepartitorPanel;
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
    cxListTipBuget: TcxDBTreeList;
    cxListTipBugetID_BG_TIPURI_BUGET: TcxDBTreeListColumn;
    cxListTipBugetDENUMIRE: TcxDBTreeListColumn;
    cxListTipBugetTIP_BUGET: TcxDBTreeListColumn;
    gr7: TcxGroupBox;
    cxLabel12: TcxLabel;
    RPEconomic7: TcxRepartitorPanel;
    cxTreeEconomic: TcxDBTreeList;
    cxTreeEconomicID_BG_PLAN_ECONOMIC: TcxDBTreeListColumn;
    cxTreeEconomicCOD_ECONOMIC: TcxDBTreeListColumn;
    cxTreeEconomicDENUMIRE: TcxDBTreeListColumn;
    cxTreeEconomicDESCRIERE: TcxDBTreeListColumn;
    cxTreeEconomicNUMAR_RAND: TcxDBTreeListColumn;
    cxTreeEconomicID_PARINTE: TcxDBTreeListColumn;
    cxTreeEconomicCLASA: TcxDBTreeListColumn;
    cxTreeEconomicESTE_LOCAL: TcxDBTreeListColumn;
    gr8: TcxGroupBox;
    cxLabel13: TcxLabel;
    RPFunctional8: TcxRepartitorPanel;
    cxLabel14: TcxLabel;
    RPEconomic8: TcxRepartitorPanel;
    gr9: TcxGroupBox;
    cxLabel15: TcxLabel;
    RPFunctional9: TcxRepartitorPanel;
    cxLabel16: TcxLabel;
    RPProiect9: TcxRepartitorPanel;
    cxTreeFunctionalComplet: TcxDBTreeList;
    cxTreeFunctionalCompletCOD_FUNCTIONAL: TcxDBTreeListColumn;
    cxTreeFunctionalCompletID_BG_TIPURI_BUGET: TcxDBTreeListColumn;
    cxTreeFunctionalCompletID_OI_UNITATI: TcxDBTreeListColumn;
    cxTreeFunctionalCompletID_BG_PLAN_FUNCTIONAL: TcxDBTreeListColumn;
    cxTreeFunctionalCompletDENUMIRE: TcxDBTreeListColumn;
    cxTreeFunctionalCompletNUMAR_RAND: TcxDBTreeListColumn;
    cxTreeFunctionalCompletID_PARINTE: TcxDBTreeListColumn;
    cxTreeFunctionalCompletCLASA: TcxDBTreeListColumn;
    cxTreeFunctionalCompletCAPITOL: TcxDBTreeListColumn;
    cxTreeFunctionalCompletESTE_LUCRARE: TcxDBTreeListColumn;
    cxTreeFunctionalCompletTIP_BUGET: TcxDBTreeListColumn;
    cxTreeFunctionalCompletESTE_STANDARD: TcxDBTreeListColumn;
    gr10: TcxGroupBox;
    cxLabel17: TcxLabel;
    RPFunctional10: TcxRepartitorPanel;
    cxTreeFunctionalCompletDESCRIERE: TcxDBTreeListColumn;
    cxTreeFunctionalCompletID_OI_PROIECTE: TcxDBTreeListColumn;
    cxTreeFunctionalCompletCOD_ECRAN: TcxDBTreeListColumn;
    cxTreeProiecte: TcxDBTreeList;
    cxTreeProiecteID_OI_PROIECTE: TcxDBTreeListColumn;
    cxTreeProiecteID_OI_TIPURI_PROIECTE: TcxDBTreeListColumn;
    cxTreeProiecteID_PARINTE: TcxDBTreeListColumn;
    cxTreeProiecteDENUMIRE: TcxDBTreeListColumn;
    cxTreeProiecteDESCRIERE: TcxDBTreeListColumn;
    cxTreeProiecteSTARE: TcxDBTreeListColumn;
    cxTreeProiecteCOD_FUNCTIONAL: TcxDBTreeListColumn;
    gr11: TcxGroupBox;
    cxLabel18: TcxLabel;
    RPEconomic11: TcxRepartitorPanel;
    cxLabel19: TcxLabel;
    RBUnitate11: TcxRepartitorPanel;
    RBUnitate12: TcxRepartitorPanel;
    procedure cxTreeFunctionalDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure cxTreeFunctionalDblClick(Sender: TObject);
    procedure cxTreeFunctionalKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure RPFunctional1PopupCloseUp(Sender: TObject);
    procedure cxTreeUnitatiDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure FormCreate(Sender: TObject);
    procedure cxTreeProiecteDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure cxListTipBugetDENUMIREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure cxTreeEconomicDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure RPFunctional3PopupInitPopup(Sender: TObject);
    procedure RPFunctional3PopupCloseUp(Sender: TObject);
    procedure RPTipBuget3EditChange(Sender: TObject);
    procedure RPTipBuget3EditValidate(Sender: TObject;
      var DisplayValue: Variant; var ErrorText: TCaption;
      var Error: Boolean);
    procedure RPTipBuget6PopupInitPopup(Sender: TObject);
    procedure RPFunctional1PopupInitPopup(Sender: TObject);
    procedure RPFunctional1Validate(Sender: TObject;
      var AKeyValue: Variant);
    procedure RPTipBuget3Validate(Sender: TObject; var AKeyValue: Variant);
    procedure RPTipBuget6Validate(Sender: TObject; var AKeyValue: Variant);
    procedure cxTreeUnitatiDENUMIREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure cxTreeFunctionalCompletDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure cxTreeUnitatiDblClick(Sender: TObject);
    procedure RBUnitate12PopupCloseUp(Sender: TObject);
    procedure RBUnitate12PopupInitPopup(Sender: TObject);
    procedure RBUnitate12Validate(Sender: TObject; var AKeyValue: Variant);
    procedure RPFunctional1ButtonClick(Sender: TObject);
    procedure cxTreeFunctionalCompletCustomDrawDataCell(
      Sender: TcxCustomTreeList; ACanvas: TcxCanvas;
      AViewInfo: TcxTreeListEditCellViewInfo; var ADone: Boolean);
  private
    FBugetPlanType: TBugetPlanType;
    FOnEditComplete: TNotifyEvent;
    { Private declarations }

    function GetCodEconomic: String;
    function GetCodFunctional: String;
    function GetProiect: Integer;
    function GetUnitate: Integer;
    procedure SetBugetPlanType(const Value: TBugetPlanType);
    procedure SetMaxConstraints;
    function GetDescriereEconomic: String;
    function GetDescriereFunctional: String;
    function GetDescriereProiect: String;
    function GetDescriereUnitate: String;

    procedure SetUnitateFilter(aFilter : String);


  public
    { Public declarations }
    procedure RefreshDataSet;
    procedure SetFilterOnDataSet(lQry : TZQuery; lCondition : String);
    function CheckCompleteValidation : Boolean;
    function GetPanel(ABugetPlanType : TBugetPlanType): TcxGroupBox;

    property BugetPlanType : TBugetPlanType read FBugetPlanType write SetBugetPlanType;

    property CodFunctional : String read GetCodFunctional;
    property CodEconomic : String read GetCodEconomic;
    property Proiect : Integer read GetProiect;
    property Unitate : Integer read GetUnitate;

    property DescriereUnitate : String read GetDescriereUnitate;
    property DescriereProiect : String read GetDescriereProiect;
    property DescriereFunctional : String read GetDescriereFunctional;
    property DescriereEconomic : String read GetDescriereEconomic;
    property OnEditComplete : TNotifyEvent read FOnEditComplete write FOnEditComplete;
  end;


implementation

uses
  ZeosDBUtile, DateUnit, CommonDBVar, cxDropDownEdit;

{$R *.dfm}


procedure TfrmBxPlanContainer.cxTreeFunctionalDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Values[cxTreeFunctionalCOD_FUNCTIONAL.ItemIndex] + ': '+ANode.Values[cxTreeFunctionalDENUMIRE.ItemIndex];
end;

procedure TfrmBxPlanContainer.cxTreeFunctionalDblClick(Sender: TObject);
begin
   with TcxDBTreeList(Sender) do
    if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk;
end;

procedure TfrmBxPlanContainer.cxTreeFunctionalKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
     if Assigned(TcxDBTreeList(Sender).OnDblClick) then TcxDBTreeList(Sender).OnDblClick(Sender) else cxTreeFunctionalDblClick(TcxDBTreeList(Sender))
  else if Key = VK_ESCAPE then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrCancel;
end;

procedure TfrmBxPlanContainer.RPFunctional1PopupCloseUp(Sender: TObject);
var
  lNode: TcxTreeListNode;
  lcxDBTree : TcxDBTreeList;
  lIdColumn : TcxTreeListColumn;
  lRepPanel :  TcxRepartitorPanel;
begin
  //folosim
  if not (Sender is TcxRpAtsPopupEdit) then Exit;
  lRepPanel :=  TcxRepartitorPanel(TcxRpAtsPopupEdit(Sender).Parent);
  if lRepPanel = nil then Exit;
  if lRepPanel.PopupResult = mrOk then begin
     if Assigned(lRepPanel.PopupEdit) and Assigned(lRepPanel.PopupEdit.PopupControl) and
         ((lRepPanel.PopupEdit.PopupControl) is TcxDBTreeList) then begin
      lcxDBTree := TcxDBTreeList(lRepPanel.PopupEdit.PopupControl);
      lNode := lcxDBTree.FocusedNode;
      if Assigned(lNode) then begin
        lRepPanel.Text := lNode.Texts[lcxDBTree.VisibleColumns[0].ItemIndex];
        //pentru ca nu avem nevoie de cheia primara caut coloana cu tag -1
        lIdColumn := cxFindColumnByTag(lcxDBTree, -1);
        //daca nu este caut keya primara

        if lIdColumn = nil then
           lIdColumn := cxFindColumnByFieldName(lcxDBTree, lcxDBTree.DataController.KeyField);

        if lIdColumn <> nil then
          lRepPanel.EditInput.Text := lNode.Values[lIdColumn.ItemIndex];
      end;
     end;
  end;
end;

procedure TfrmBxPlanContainer.cxTreeUnitatiDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  if Trim(ANode.Texts[cxTreeUnitatiCOD_FUNCTIONAL.ItemIndex]) <> '' then  Value := Trim(ANode.Texts[cxTreeUnitatiCOD_FUNCTIONAL.ItemIndex]) + ' : '
  else Value := '';
  Value := Value + ANode.Texts[cxTreeUnitatiDENUMIRE.ItemIndex];
end;

procedure TfrmBxPlanContainer.FormCreate(Sender: TObject);
begin
  RefreshDataSet;
  SetMaxConstraints;
end;

procedure TfrmBxPlanContainer.cxTreeProiecteDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Texts[cxTreeProiecteDENUMIRE.ItemIndex] ;
  if Trim(ANode.Texts[cxTreeProiecteCOD_FUNCTIONAL.ItemIndex]) <> '' then
    Value := Value + '('+Trim(ANode.Texts[cxTreeProiecteCOD_FUNCTIONAL.ItemIndex])+')';
end;

procedure TfrmBxPlanContainer.cxListTipBugetDENUMIREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := '(' + ANode.Texts[cxListTipBugetTIP_BUGET.ItemIndex] + ')  ' + ANode.Values[cxListTipBugetDENUMIRE.ItemIndex];
end;

procedure TfrmBxPlanContainer.cxTreeEconomicDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Values[cxTreeEconomicCOD_ECONOMIC.ItemIndex] + ': '+ ANode.Values[cxTreeEconomicDENUMIRE.ItemIndex]; 
end;

procedure TfrmBxPlanContainer.RPFunctional3PopupInitPopup(Sender: TObject);
begin
  if Trim(RPTipBuget3.TextEdit.Text) <> '' then
    SetFilterOnDataSet(frmData.qryBGPlanFunctional,
       'TIP_BUGET= ' +   QuotedStr(RPTipBuget3.TextEdit.Text));
  with TcxPopupEdit(Sender).Properties do begin
    if PopupWidth < TcxPopupEdit(Sender).Width then PopupWidth := TcxPopupEdit(Sender).Width;
  end;
end;

procedure TfrmBxPlanContainer.SetFilterOnDataSet(lQry: TZQuery; lCondition: String);
begin
  if (Trim(lCondition) = '')
    and ( (Trim(lQry.Filter) <> '') or lQry.Filtered) then begin
     lQry.Filtered := False;
     lQry.Filter := '';
  end
  else begin
    lQry.Filtered := False;
    lQry.Filter := lCondition;
    lQry.Filtered := True;
  end;
end;

procedure TfrmBxPlanContainer.RPFunctional3PopupCloseUp(Sender: TObject);
begin
   RPFunctional1PopupCloseUp(Sender);
   SetFilterOnDataSet(frmData.qryBGPlanFunctional, '');
end;

procedure TfrmBxPlanContainer.RPTipBuget3EditChange(Sender: TObject);
begin
  TRpATSEdit(Sender).Tag := -1;
end;

procedure TfrmBxPlanContainer.RPTipBuget3EditValidate(Sender: TObject;
  var DisplayValue: Variant; var ErrorText: TCaption; var Error: Boolean);
var
  lTree         : TcxDBTreeList;
  lPanel        : TcxRepartitorPanel;
  lTextC,
  lSearchC      : TcxDBTreeListColumn;
  lTextIndex,
  lSearchIndex  : Integer;
  lNode         : TcxTreeListNode;
begin
  TRpATSEdit(Sender).Tag := -1;
  if not(Sender is TRpAtsEdit) then Exit;
  lPanel  := TcxRepartitorPanel(TRpAtsEdit(Sender).Parent);
  if not(lPanel.PopupEdit.PopupControl is TcxDBTreeList) then Exit;
  lTree   := TcxDBTreeList(lPanel.PopupEdit.PopupControl);
  lSearchC  := cxFindColumnByTag(lTree, -1);
  if lSearchC = nil then
     lSearchC := cxFindColumnByFieldName(lTree, lTree.DataController.KeyField);
  if lTree.VisibleColumnCount >0 then
   lTextC := TcxDBTreeListColumn(lTree.VisibleColumns[0])
  else
  lTextC := cxFindFirstVisibleColumn(lTree);
  if lTextC = nil then cxFindColumnByFieldName(lTree, lPanel.ListField);
  if lTextC <> nil then lTextIndex := lTextC.ItemIndex else lTextIndex := -1;
  if lSearchC <> nil then lSearchIndex := lSearchC.ItemIndex else lSearchIndex := -1;
  if TRpATSEdit(Sender).Tag = -1 then begin
    if not SetKeyOnPanelTree(lPanel, TRpATSEdit(Sender).Text, lTextIndex, lSearchIndex, lTree, (cxFindColumnByFieldName(lTree, lTree.DataController.KeyField).ItemIndex = lSearchIndex), not (FBugetPlanType in [bptUnitateInterna, bptUnitateExterna])) then begin
      lNode := LocateOnKeyNode(lTree, lSearchIndex, TRpATSEdit(Sender).Text);
      if lNode <> nil then begin
        lNode.Focused := True;
        lNode.MakeVisible;
  //          if lNode.
        TRpATSEdit(Sender).Tag := -1;
      end
      else
       TRpATSEdit(Sender).Tag := -1;
      lPanel.ListaInput.SetFocus;
      lPanel.ListaInput.DroppedDown := True;
    end;
  end;
  if Assigned(TRpATSEdit(Sender).Owner) and (TRpATSEdit(Sender).Owner is TcxRepartitorPanel) then begin
  TcxRepartitorPanel(TRpATSEdit(Sender).Owner).Tag := TRpATSEdit(Sender).Tag;
  if (TRpATSEdit(Sender).Tag <> -1) and Assigned(FOnEditComplete) then
    FOnEditComplete(Sender);
  end;
end;


procedure TfrmBxPlanContainer.RPTipBuget6PopupInitPopup(Sender: TObject);
begin
  if Trim(RPTipBuget6.TextEdit.Text) <> '' then
    SetFilterOnDataSet(frmData.qryBGPlanFunctional,
       'TIP_BUGET= ' +   QuotedStr(RPTipBuget6.TextEdit.Text));
  with TcxPopupEdit(Sender).Properties do begin
    if PopupWidth < TcxPopupEdit(Sender).Width then PopupWidth := TcxPopupEdit(Sender).Width;
  end;
end;

function TfrmBxPlanContainer.GetCodEconomic: String;
begin
  case FBugetPlanType of
    bptClasaFunctionala,
    bptUnitateInterna,
    bptUnitateExterna,
    bptBugetFunctional,
    bptProiect,
    bptProiectBuget,
    bptUnitateBugetFunctional,
    bptProiectFunctional,
    bptFunctional:
      Result := '';
    bptEconomic:
      Result := RPEconomic7.TextEdit.Text;
    bptFunctionalEconomic:
      Result := RPEconomic8.TextEdit.Text;
    bptEconomicUnitate:
      Result := RPEconomic11.TextEdit.Text;
  end;
end;

function TfrmBxPlanContainer.GetCodFunctional: String;
begin
  case FBugetPlanType of
    bptClasaFunctionala:
      Result := RPFunctional1.TextEdit.Text;
    bptUnitateInterna,
    bptUnitateExterna:
      Result := RPFunctional2.TextEdit.Text;
    bptBugetFunctional:
      Result := RPFunctional3.TextEdit.Text;
    bptProiect: ;
    bptProiectBuget: ;
    bptUnitateBugetFunctional:
      Result := RPFunctional6.TextEdit.Text;
    bptEconomic: ;
    bptFunctionalEconomic:
      Result := RPFunctional8.TextEdit.Text;
    bptProiectFunctional:
      Result := RPFunctional9.TextEdit.Text;
    bptFunctional:
      Result := RPFunctional10.TextEdit.Text;
    else
      Result := '';
  end;
end;

function TfrmBxPlanContainer.GetProiect: Integer;
begin
  case FBugetPlanType of
    bptProiect:
      Result := StrToInt(RPProiect4.TextEdit.Text);
    bptProiectBuget:
      Result := StrToInt(RPProiect5.TextEdit.Text);
    bptProiectFunctional:
      Result := StrToInt(RPProiect9.TextEdit.Text);
    bptFunctional:
      if frmData.qryBGPlanFunctionalComplet.FieldByName('ID_OI_PROIECTE').Value = null then
        Result := -1
      else
        Result := frmData.qryBGPlanFunctionalComplet.FieldByName('ID_OI_PROIECTE').AsInteger;
    else Result := -1;
  end;
end;

function TfrmBxPlanContainer.GetUnitate: Integer;
begin
  case FBugetPlanType of
    bptUnitateInterna, bptUnitateExterna :
      Result := StrToInt(RBUnitate12.TextEdit.Text);
    bptUnitateBugetFunctional :
      Result := StrToInt(RPUnitate6.TextEdit.Text);
    bptFunctional,
    bptProiectFunctional :
      Result := ValueSafeToInt(frmData.qryBGPlanFunctionalComplet['ID_OI_UNITATI'], -1);
    bptEconomicUnitate :
      Result := StrToInt(RBUnitate11.TextEdit.Text);
    else Result := -1;
  end;
end;

procedure TfrmBxPlanContainer.SetBugetPlanType(const Value: TBugetPlanType);
begin
  FBugetPlanType := Value;
end;

function TfrmBxPlanContainer.GetPanel(ABugetPlanType: TBugetPlanType): TcxGroupBox;
begin
  BugetPlanType := ABugetPlanType;
  case ABugetPlanType of
    bptClasaFunctionala:
      Result := gr1;
    bptUnitateInterna,
    bptUnitateExterna:
      begin
        Result := gr2;
        RPFunctional2.ListaInput.EditValue := Null;
        RPFunctional2.EditInput.EditValue := Null;
        RPFunctional2.Tag := -1;
        RBUnitate12.PopupEdit.Text := '';
        RBUnitate12.TextEdit.Text := '';
        RBUnitate12.Tag := -1;
      end;
    bptBugetFunctional:
      Result := gr3;
    bptProiect:
      Result := gr4;
    bptProiectBuget:
      Result := gr5;
    bptUnitateBugetFunctional:
      Result := gr6;
    bptEconomic:
      Result := gr7;
    bptFunctionalEconomic:
      Result := gr8;
    bptProiectFunctional:
      Result := gr9;
    bptFunctional:
      Result := gr10;
    bptEconomicUnitate:
      Result := gr11;
    else
      Result := nil;
  end;
end;

procedure TfrmBxPlanContainer.SetMaxConstraints;
var
  I : Integer;
  aGr : TComponent;
begin
  I := 1;
  aGr := FindComponent('gr'+ IntToStr(I));
  while aGr <> nil do begin
    if aGr is TcxGroupBox then begin
      TcxGroupBox(aGr).Constraints.MaxHeight := TcxGroupBox(aGr).Height;
      TcxGroupBox(aGr).Constraints.MinHeight := TcxGroupBox(aGr).Height;
    end;
    I := I + 1;
    aGr := FindComponent('gr'+ IntToStr(I));
  end;
 end;

procedure TfrmBxPlanContainer.RPFunctional1PopupInitPopup(Sender: TObject);
begin
  with TcxPopupEdit(Sender).Properties do begin
    if PopupWidth < TcxPopupEdit(Sender).Width then PopupWidth := TcxPopupEdit(Sender).Width;
  end;
end;

function TfrmBxPlanContainer.CheckCompleteValidation : Boolean;
begin
  case FBugetPlanType of
    bptClasaFunctionala:
      Result := RPFunctional1.Tag <> -1;
    bptUnitateInterna,
    bptUnitateExterna:
      Result := (RPFunctional2.Tag <> -1) and (RBUnitate12.Tag<>-1);
    bptBugetFunctional:
      Result := RPFunctional3.Tag <> -1;
    bptProiect:
      Result := RPProiect4.Tag <> -1;
    bptProiectBuget:
      Result := (RPProiect5.Tag <> -1) and  (RPTipBuget5.Tag <> -1);
    bptUnitateBugetFunctional:
      Result := (RPUnitate6.Tag <> -1) and  (RPFunctional6.Tag <> -1);
    bptEconomic:
      Result := RPEconomic7.Tag <> -1;
    bptFunctionalEconomic:
      Result := (RPEconomic8.Tag <> -1) and  (RPFunctional8.Tag <> -1);
    bptProiectFunctional:
      Result := (RPProiect9.Tag <> -1) and  (RPFunctional9.Tag <> -1);
    bptFunctional:
      Result := RPFunctional10.Tag <> -1;
    bptEconomicUnitate:
      Result := (RPEconomic11.Tag <> -1) and (RBUnitate11.Tag <> -1);
    else
      Result := False;
  end;
end;

procedure TfrmBxPlanContainer.RPFunctional1Validate(Sender: TObject;
  var AKeyValue: Variant);
var
   lNode: TcxDBTreeListNode;
   lTree : TcxDBTreeList;
   lTextC, lSearchC : TcxDBTreeListColumn;
begin
  if not(TcxRepartitorPanel(Sender).PopupEdit.PopupControl is TcxDBTreeList) then Exit;
  lTree := TcxDBTreeList(TcxRepartitorPanel(Sender).PopupEdit.PopupControl);
  lNode := TcxDBTreeListNode(lTree.FindNodeByKeyValue(AKeyValue, nil));
  if Assigned(lNode) and lNode.HasChildren then Exit;
  TcxRepartitorPanel(Sender).Text := '';
  TcxRepartitorPanel(Sender).TextEdit.Text := '';
  if Assigned(lNode) then begin
    try
      TcxRepartitorPanel(Sender).Tag := AKeyValue;
    except
      TcxRepartitorPanel(Sender).Tag := -1;
    end;
    lSearchC := cxFindColumnByTag(lTree, -1);
    if lSearchC = nil then
      lSearchC := cxFindColumnByFieldName(lTree, lTree.DataController.KeyField);
    if lTree.VisibleColumnCount >0 then
      lTextC := TcxDBTreeListColumn(lTree.VisibleColumns[0])
    else
      lTextC := cxFindFirstVisibleColumn(lTree);
    if lTextC = nil then cxFindColumnByFieldName(lTree, TcxRepartitorPanel(Sender).ListField);
    if lTextC <> nil then
      TcxRepartitorPanel(Sender).Text := lNode.Texts[lTextC.ItemIndex];
    if lSearchC <> nil then
      TcxRepartitorPanel(Sender).TextEdit.Text := lNode.Texts[lSearchC.ItemIndex];
  end;
  if Assigned(FOnEditComplete) then
    FOnEditComplete(Sender);
end;

procedure TfrmBxPlanContainer.RPTipBuget3Validate(Sender: TObject;
  var AKeyValue: Variant);
var
   lNode: TcxDBTreeListNode;
   lTree : TcxDBTreeList;
   lTextC, lSearchC : TcxDBTreeListColumn;
begin
   if not(TcxRepartitorPanel(Sender).PopupEdit.PopupControl is TcxDBTreeList) then Exit;
   lTree := TcxDBTreeList(TcxRepartitorPanel(Sender).PopupEdit.PopupControl);
   lNode := TcxDBTreeListNode(lTree.FindNodeByKeyValue(AKeyValue, nil));
   if Assigned(lNode) and lNode.HasChildren then Exit;
   TcxRepartitorPanel(Sender).Text := '';
   TcxRepartitorPanel(Sender).TextEdit.Text := '';
   if Assigned(lNode) then begin
       try
         TcxRepartitorPanel(Sender).Tag := AKeyValue;
       except
         TcxRepartitorPanel(Sender).Tag := -1;
       end;
       lSearchC := cxFindColumnByTag(lTree, -1);
       if lSearchC = nil then
           lSearchC := cxFindColumnByFieldName(lTree, lTree.DataController.KeyField);
       if lTree.VisibleColumnCount >0 then
         lTextC := TcxDBTreeListColumn(lTree.VisibleColumns[0])
       else
        lTextC := cxFindFirstVisibleColumn(lTree);
       if lTextC = nil then cxFindColumnByFieldName(lTree, TcxRepartitorPanel(Sender).ListField);
       if lTextC <> nil then
          TcxRepartitorPanel(Sender).Text := lNode.Texts[lTextC.ItemIndex];
       if lSearchC <> nil then
          TcxRepartitorPanel(Sender).TextEdit.Text := lNode.Texts[lSearchC.ItemIndex];
    end;
   RPFunctional3.Tag := -1;
   RPFunctional3.EditInput.Text := '';
   RPFunctional3.TextEdit.Text := '';
   RPFunctional3.Text := '';   
   if Assigned(FOnEditComplete) then
      FOnEditComplete(Sender);
end;

procedure TfrmBxPlanContainer.RPTipBuget6Validate(Sender: TObject;
  var AKeyValue: Variant);
var
   lNode: TcxDBTreeListNode;
   lTree : TcxDBTreeList;
   lTextC, lSearchC : TcxDBTreeListColumn;
begin
   if not(TcxRepartitorPanel(Sender).PopupEdit.PopupControl is TcxDBTreeList) then Exit;
   lTree := TcxDBTreeList(TcxRepartitorPanel(Sender).PopupEdit.PopupControl);
   lNode := TcxDBTreeListNode(lTree.FindNodeByKeyValue(AKeyValue, nil));
   if Assigned(lNode) and lNode.HasChildren then Exit;
   TcxRepartitorPanel(Sender).Text := '';
   TcxRepartitorPanel(Sender).TextEdit.Text := '';
   if Assigned(lNode) then begin
       try
         TcxRepartitorPanel(Sender).Tag := AKeyValue;
       except
         TcxRepartitorPanel(Sender).Tag := -1;
       end;
       lSearchC := cxFindColumnByTag(lTree, -1);
       if lSearchC = nil then
           lSearchC := cxFindColumnByFieldName(lTree, lTree.DataController.KeyField);
       if lTree.VisibleColumnCount >0 then
         lTextC := TcxDBTreeListColumn(lTree.VisibleColumns[0])
       else
        lTextC := cxFindFirstVisibleColumn(lTree);
       if lTextC = nil then cxFindColumnByFieldName(lTree, TcxRepartitorPanel(Sender).ListField);
       if lTextC <> nil then
          TcxRepartitorPanel(Sender).Text := lNode.Texts[lTextC.ItemIndex];
       if lSearchC <> nil then
          TcxRepartitorPanel(Sender).TextEdit.Text := lNode.Texts[lSearchC.ItemIndex];
    end;
   RPFunctional6.Tag := -1;
   RPFunctional6.EditInput.Text := '';
   RPFunctional6.TextEdit.Text := '';
   RPFunctional6.Text := '';
   if Assigned(FOnEditComplete) then
      FOnEditComplete(Sender);
end;


function TfrmBxPlanContainer.GetDescriereEconomic: String;
var lNode : TcxTreeListNode;
begin
  lNode := cxTreeEconomic.FindNodeByText(CodEconomic, cxTreeEconomicCOD_ECONOMIC);
  if lNode <> nil then Result := lNode.Texts[cxTreeEconomicDESCRIERE.ItemIndex] else Result := '';
end;

function TfrmBxPlanContainer.GetDescriereFunctional: String;
var lNode : TcxTreeListNode;
begin
  lNode := cxTreeFunctional.FindNodeByText(CodFunctional, cxTreeFunctionalCOD_FUNCTIONAL);
  if lNode <> nil then Result := lNode.Texts[cxTreeFunctionalDESCRIERE.ItemIndex] else Result := '';
end;

function TfrmBxPlanContainer.GetDescriereProiect: String;
var lNode : TcxTreeListNode;
begin
  lNode := cxTreeProiecte.FindNodeByKeyValue(Proiect, nil);
  if lNode <> nil then Result := lNode.Texts[cxTreeProiecteDESCRIERE.ItemIndex] else Result := '';
end;

function TfrmBxPlanContainer.GetDescriereUnitate: String;
var lNode : TcxTreeListNode;
begin
  lNode := cxTreeUnitati.FindNodeByKeyValue(Unitate, nil);
  if lNode <> nil then Result := lNode.Texts[cxTreeUnitatiDESCRIERE.ItemIndex] else Result := '';
end;

procedure TfrmBxPlanContainer.cxTreeUnitatiDENUMIREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  if not (ANode.Texts[cxTreeUnitatiCOD_FUNCTIONAL.ItemIndex] <> '') then
    Value := Value + ' - ' + ANode.Texts[cxTreeUnitatiCOD_FUNCTIONAL.ItemIndex];
end;

procedure TfrmBxPlanContainer.cxTreeFunctionalCompletDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Values[cxTreeFunctionalCompletCod_ecran.ItemIndex] + ': '+ANode.Values[cxTreeFunctionalCompletDENUMIRE.ItemIndex];
end;

procedure TfrmBxPlanContainer.cxTreeUnitatiDblClick(Sender: TObject);
begin
   with TcxDBTreeList(Sender) do
    if (FocusedNode <> nil) then begin
     (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk;

{
      if (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).Parent.Parent.Name = 'gr2' then begin
        RPFunctional2.EditInput.Text := FocusedNode.Texts[cxTreeUnitatiCOD_FUNCTIONAL.ItemIndex];
        //RBUnitate12.ForceValidateEditText;
        RPFunctional2.ForceValidateEditText;
      end;
}
   end;
end;

procedure TfrmBxPlanContainer.SetUnitateFilter(aFilter: String);
var
  NewFiltered: Boolean;
begin
  with  frmData.qryOIUnitati do begin
   if not(AFilter = '') then begin
     NewFiltered := Filter <> AFilter;
     if NewFiltered then Filter := AFilter;
   end;
   Filtered := aFilter <> '';
  end;
end;

procedure TfrmBxPlanContainer.RBUnitate12PopupCloseUp(Sender: TObject);
begin
  //RPFunctional1PopupCloseUp(Sender);
  SetUnitateFilter('');
end;

procedure TfrmBxPlanContainer.RBUnitate12PopupInitPopup(Sender: TObject);
begin
  if FBugetPlanType = bptUnitateExterna then begin
    SetUnitateFilter('ESTE_INTERNA=0 or ESTE_INTERNA IS NULL');
  end
  else begin
    SetUnitateFilter('ESTE_INTERNA=1');
  end;

  with TcxPopupEdit(Sender).Properties do begin
    if PopupWidth < TcxPopupEdit(Sender).Width then PopupWidth := TcxPopupEdit(Sender).Width;
  end;
end;

procedure TfrmBxPlanContainer.RBUnitate12Validate(Sender: TObject;
  var AKeyValue: Variant);
var
   lNode: TcxDBTreeListNode;
   lTree : TcxDBTreeList;
   lTextC, lSearchC : TcxDBTreeListColumn;
begin
  if not(TcxRepartitorPanel(Sender).PopupEdit.PopupControl is TcxDBTreeList) then Exit;
  lTree := TcxDBTreeList(TcxRepartitorPanel(Sender).PopupEdit.PopupControl);
  lNode := TcxDBTreeListNode(lTree.FindNodeByKeyValue(AKeyValue, nil));
  if not Assigned(lNode) then Exit;
  TcxRepartitorPanel(Sender).Text := '';
  TcxRepartitorPanel(Sender).TextEdit.Text := '';
  if Assigned(lNode) then begin
    try
      TcxRepartitorPanel(Sender).Tag := AKeyValue;
    except
      TcxRepartitorPanel(Sender).Tag := -1;
    end;
    lSearchC := cxFindColumnByTag(lTree, -1);
    if lSearchC = nil then
      lSearchC := cxFindColumnByFieldName(lTree, lTree.DataController.KeyField);
    if lTree.VisibleColumnCount >0 then
      lTextC := TcxDBTreeListColumn(lTree.VisibleColumns[0])
    else
      lTextC := cxFindFirstVisibleColumn(lTree);
    if lTextC = nil then cxFindColumnByFieldName(lTree, TcxRepartitorPanel(Sender).ListField);
    if lTextC <> nil then
      TcxRepartitorPanel(Sender).Text := lNode.Texts[lTextC.ItemIndex];
    if lSearchC <> nil then
      TcxRepartitorPanel(Sender).TextEdit.Text := lNode.Texts[lSearchC.ItemIndex];
  end;
  if Assigned(FOnEditComplete) then
    FOnEditComplete(Sender);
end;


procedure TfrmBxPlanContainer.RefreshDataSet;
begin
  DBRefresh(
    [frmData.qryBGPlanFunctionalComplet,
     frmData.qryBGPlanFunctional,
     frmData.qryBGPlanEconomic,
     frmData.qryOIUnitati,
     frmData.qryOIProiecte,
     frmData.qryBGTipuriBuget]);
  cxTreeUnitati.FullExpand;
  cxTreeProiecte.FullExpand;
  cxTreeFunctional.FullExpand;
  cxTreeFunctionalComplet.FullExpand;
end;

procedure TfrmBxPlanContainer.RPFunctional1ButtonClick(Sender: TObject);
begin
  RefreshDataSet;
end;

procedure TfrmBxPlanContainer.cxTreeFunctionalCompletCustomDrawDataCell(
  Sender: TcxCustomTreeList; ACanvas: TcxCanvas;
  AViewInfo: TcxTreeListEditCellViewInfo; var ADone: Boolean);
begin
  ACanvas.Font.Style := ACanvas.Font.Style - [fsBold];
  if AViewInfo.Node.Texts[cxTreeFunctionalCompletID_OI_UNITATI.ItemIndex] <> '' then
    ACanvas.Font.Style := ACanvas.Font.Style + [fsBold];
 if AViewInfo.Node.Texts[cxTreeFunctionalCompletID_OI_PROIECTE.ItemIndex] <> '' then
    ACanvas.Font.Style := ACanvas.Font.Style + [fsBold];
end;

end.

