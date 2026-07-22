unit DetaliiDocumUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  cxGridCustomTableView, cxGridTableView, cxGridDBTableView, cxGrid,
  cxGraphics, cxControls, cxLookAndFeels, cxLookAndFeelPainters,
  cxCustomData, cxStyles, cxTL, cxMaskEdit, cxTLdxBarBuiltInMenu,
  cxContainer, cxEdit, Menus, cxFilter, cxData, cxDataStorage, DB,
  cxDBData, ImgList, ZAbstractRODataset, ZAbstractDataset, ZDataset,
  cxGridLevel, cxClasses, cxGridCustomView, cxButtonEdit, cxDBEdit,
  cxVGrid, cxDBVGrid, StdCtrls, cxButtons, cxRepartitorPanel,
  cxDropDownEdit, cxCalendar, cxTextEdit, cxImageComboBox, cxCheckBox,
  cxGroupBox, ExtCtrls, dxNavBarCollns, dxNavBarBase, dxNavBar,
  cxInplaceContainer, cxDBTL, cxTLData, cxCurrencyEdit, cxNavigator,
  Vcl.ComCtrls, dxCore, cxDateUtils,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxDateRanges,
  dxScrollbarAnnotations;

type
  TfrmDetaliiDocum = class(TForm)
    grDocumentPlata: TcxGroupBox;
    chkAchitat: TcxCheckBox;
    edTipDoc: TcxImageComboBox;
    edNrDoc: TcxTextEdit;
    edDataPlata: TcxDateEdit;
    lbTipDoc: TLabel;
    lnNrDoc: TLabel;
    lbDataDoc: TLabel;
    DTStructure: TDataSource;
    QryStructure: TZReadOnlyQuery;
    ImagesStructura: TImageList;
    TreeStructura: TcxDBTreeList;
    TreeStructuraCOD_CB: TcxDBTreeListColumn;
    TreeStructuraCOD_PARINTE: TcxDBTreeListColumn;
    TreeStructuraDENUMIRE: TcxDBTreeListColumn;
    TreeStructuraDENV: TcxDBTreeListColumn;
    TreeStructuraC_O: TcxDBTreeListColumn;
    TreeStructuraDATA_SOLD: TcxDBTreeListColumn;
    TreeStructuraCASIER: TcxDBTreeListColumn;
    TreeStructuraVALIDATOR: TcxDBTreeListColumn;
    TreeStructuraADMIN: TcxDBTreeListColumn;
    TreeStructuraIS_BANCA: TcxDBTreeListColumn;
    TreeStructuraIS_AVANS: TcxDBTreeListColumn;
    TreeStructuraIS_TEMPOR: TcxDBTreeListColumn;
    TreeStructuraID_REPARTITORI: TcxDBTreeListColumn;
    TreeStructuraICON: TcxDBTreeListColumn;
    TreeStructuraID_VALUTA: TcxDBTreeListColumn;
    TreeStructuraCRSP_LEI: TcxDBTreeListColumn;
    TreeStructuraDESCRIERE: TcxDBTreeListColumn;
    cxStyleRepository1: TcxStyleRepository;
    cxStyle1: TcxStyle;
    RPCasaBanca: TcxRepartitorPanel;
    lbNrDocConex: TLabel;
    lbDataDocConex: TLabel;
    lbDetaliiDocConex: TLabel;
    edDBNrDocConex: TcxDBButtonEdit;
    edDBDataDocConex: TcxDBDateEdit;
    lbChitantaPePozitie: TLabel;
    chkEmiteChitanta: TcxCheckBox;
    Inspector: TcxDBVerticalGrid;
    NavPanel: TdxNavBar;
    grupDetaliiPlata: TdxNavBarGroup;
    grupDetaliiDocument: TdxNavBarGroup;
    grupDocumentConext: TdxNavBarGroup;
    grupDetaliiPlataControl: TdxNavBarGroupControl;
    grupDetaliiDocumentControl: TdxNavBarGroupControl;
    grupDocumentConextControl: TdxNavBarGroupControl;
    grDocConex: TcxGroupBox;
    grupCentralizareEconomic: TdxNavBarGroup;
    grupCentralizareEconomicControl: TdxNavBarGroupControl;
    viewTotalEconomic: TcxGridDBTableView;
    nivelTotalEconomic: TcxGridLevel;
    gridTotalEconomic: TcxGrid;
    dtDocumentEconomic: TDataSource;
    qryDocumentEconomic: TZReadOnlyQuery;
    viewTotalEconomiccodEconomic: TcxGridDBColumn;
    viewTotalEconomicangajat: TcxGridDBColumn;
    viewTotalEconomicfacturat: TcxGridDBColumn;
    viewTotalEconomiccod_functional: TcxGridDBColumn;
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure chkAchitatClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure TreeStructuraDblClick(Sender: TObject);
    procedure TreeStructuraKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure edCasaBancaPropertiesPopup(Sender: TObject);
    procedure RPCasaBancaEditChange(Sender: TObject);
    procedure RPCasaBancaEditValidate(Sender: TObject;
      var DisplayValue: Variant; var ErrorText: TCaption;
      var Error: Boolean);
    procedure RPCasaBancaPopupCloseUp(Sender: TObject);
    procedure RPCasaBancaPopupInitPopup(Sender: TObject);
    procedure RPCasaBancaValidate(Sender: TObject; var AKeyValue: Variant);
    procedure edTipDocPropertiesValidate(Sender: TObject;
      var DisplayValue: Variant; var ErrorText: TCaption;
      var Error: Boolean);
    procedure edNrDocPropertiesValidate(Sender: TObject;
      var DisplayValue: Variant; var ErrorText: TCaption;
      var Error: Boolean);
    procedure edDataPlataPropertiesValidate(Sender: TObject;
      var DisplayValue: Variant; var ErrorText: TCaption;
      var Error: Boolean);
    procedure chkAchitatPropertiesValidate(Sender: TObject;
      var DisplayValue: Variant; var ErrorText: TCaption;
      var Error: Boolean);
    procedure FormShow(Sender: TObject);
    procedure chkEmiteChitantaClick(Sender: TObject);
  private
    FButonLink: TcxButton;
    FIdDefaDocum: Integer;
    FIdGestDocum: Integer;
    procedure UpdateDocumente;
    procedure SetIdDefaDocum(const Value: Integer);
    function  FindRowByFieldName(const AFieldName: String): TcxDBEditorRow;
    function  FindRowByPartialFieldName(const AFieldName: String): TcxDBEditorRow;
    procedure TestContColumn;
    procedure HideAllColumns;
    procedure SetDocumentField(AFieldName: String; AValue: Variant);
    { Private declarations }
  public
    { Public declarations }
    FCurentHouse : Integer;
    FIsInLoad : Boolean;
    FHasDocConex : Boolean;
    procedure SetCasaBanca;
    procedure UpdateCodEconomic(ACulGestDocum: Integer);
    procedure ReadPlata;
    procedure ReadDocumentConex;
    property ButonLink: TcxButton read FButonLink write FButonLink;
    property IdDefaDocum: Integer read FIdDefaDocum write SetIdDefaDocum;
    property IdGestDocum : Integer read FIdGestDocum write FIdGestDocum;
  end;

implementation

uses
  ZeosDBUtile, dxCompsUtile, cxEditDBRegisteredRepositoryItems, DateUnit, CommonDBVar, Variants, MainUnit;

{$R *.DFM}

procedure TfrmDetaliiDocum.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  FButonLink.Down := False;
  Action := caHide;
end;

procedure TfrmDetaliiDocum.SetIdDefaDocum(const Value: Integer);
begin
  if Value <> FIdDefaDocum then begin
     FIdDefaDocum := Value;
     if FIdDefaDocum  = -1 then HideAllColumns;
     UpdateDocumente;
     TestContColumn;
     ReadPlata;
     ReadDocumentConex;
  end;
end;

procedure TfrmDetaliiDocum.TestContColumn;
var
  lRow : TcxDBEditorRow;
begin
  lRow := FindRowByFieldName('CONT');
  if Assigned(lRow) then begin
    lRow.Properties.EditPropertiesClass := TcxImageComboBoxProperties;
    FillImageCombo(lRow.Properties.EditProperties, 'exec [SP_GET_CONT_DOCUM]', 'CONT', 'DENUMIRE');
  end;
end;

procedure TfrmDetaliiDocum.HideAllColumns;
var
  I : Integer;
begin
  for I := 0 to Inspector.Rows.Count-1 do
    Inspector.Rows[I].Visible := False;
end;

procedure TfrmDetaliiDocum.UpdateDocumente;
var
  lDataSet: TDataSet;
  lRow    : TcxDBEditorRow;
  lField  : TField;
begin
  lDataSet := DBNewQueryFmt('SELECT * FROM GEST_DEFA_DOCUM_DOCUMENT WHERE ID_GEST_DEFA_DOCUM = %d ORDER BY POS', [FIdDefaDocum]);
  try
    lDataSet.Open;
    while not lDataSet.Eof do begin
      lRow := FindRowByFieldName(lDataSet.FieldByName('FIELD_NAME').AsString);
      if not Assigned(lRow) and Assigned(Inspector.DataController.DataSet) then begin
        lField := Inspector.DataController.DataSet.FindField(lDataSet.FieldByName('FIELD_NAME').AsString);
        if Assigned(lField) and (lDataSet.FieldByName('VISIBLE').AsBoolean) then begin
          lRow := TcxDBEditorRow(Inspector.Add(TcxDBEditorRow));
          lRow.Properties.DataBinding.FieldName := lField.FieldName;
          if pos('CONT', UpperCase(lField.FieldName)) > 0 then begin
            lRow.Properties.EditPropertiesClass := TcxImageComboBoxProperties;
            FillImageCombo(lRow.Properties.EditProperties, 'exec [SP_GET_CONT_DOCUM]', 'CONT', 'DENUMIRE');
          end
          else
          if pos('ID_TIP_VALUTA', UpperCase(lField.FieldName)) > 0 then begin
            lRow.Properties.EditPropertiesClass := TcxImageComboBoxProperties;
            FillImageCombo(lRow.Properties.EditProperties, 'spNmclValute', 0, 1);
          end
          else
            lRow.Properties.RepositoryItem := GetDefaultEditDBRepositoryItems.GetItemByField(lField);
        end;
      end;
      if Assigned(lRow) then begin
        if AnsiCompareText(lDataSet.FieldByName('CAPTION').AsString, lRow.Properties.DataBinding.FieldName) <> 0 then
          lRow.Properties.Caption := lDataSet.FieldByName('CAPTION').AsString;
        if not lDataSet.FieldByName('VISIBLE').IsNull then lRow.Visible := lDataSet.FieldByName('VISIBLE').AsBoolean;
        if not lDataSet.FieldByName('READONLY').IsNull then lRow.Properties.Options.Editing := not lDataSet.FieldByName('READONLY').AsBoolean;
        lRow.Tag := Integer(lDataSet.FieldByName('REQUIRED').AsBoolean);
        if not lDataSet.FieldByName('FONT_NAME').IsNull then begin
          lRow.Styles.Content.Font.Name := lDataSet.FieldByName('FONT_NAME').AsString;
          lRow.Styles.Header.Font.Name  := lRow.Styles.Content.Font.Name;
        end;
        if not lDataSet.FieldByName('COLOR').IsNull then begin
          lRow.Styles.Content.Color := TColor(lDataSet.FieldByName('COLOR').AsInteger);
          lRow.Styles.Header.Color  := lRow.Styles.Content.Color;
        end;
        if not lDataSet.FieldByName('FONT_COLOR').IsNull then begin
          lRow.Styles.Content.TextColor := TColor(lDataSet.FieldByName('FONT_COLOR').AsInteger);
          lRow.Styles.Header.TextColor  := lRow.Styles.Content.TextColor;
        end;
        if not lDataSet.FieldByName('FONT_SIZE').IsNull then begin
          lRow.Styles.Content.Font.Size := lDataSet.FieldByName('FONT_SIZE').AsInteger;
          lRow.Styles.Header.Font.Size  := lRow.Styles.Content.Font.Size;
        end;
      end;
      lDataSet.Next;
    end;
  finally
    lDataSet.Free;
  end;
end;

procedure TfrmDetaliiDocum.chkAchitatClick(Sender: TObject);
begin
  RPCasaBanca.Enabled := chkAchitat.Checked;
  edTipDoc.Enabled := chkAchitat.Checked;
  edNrDoc.Enabled := chkAchitat.Checked;
  edDataPlata.Enabled := chkAchitat.Checked;
  lbTipDoc.Enabled := chkAchitat.Checked;
  lbDataDoc.Enabled := chkAchitat.Checked;
  lnNrDoc.Enabled := chkAchitat.Checked;
  QryStructure.Close;
  QryStructure.Params.ParamByName('COD_UTILIZATOR').Value := IdUtilizator;
  QryStructure.Params.ParamByName('IS_ADMIN').Value := True;
  QryStructure.Params.ParamByName('DISP_WAY').Value := 0;
  QryStructure.Open;
end;

procedure TfrmDetaliiDocum.FormCreate(Sender: TObject);
begin
  FIsInLoad := False;
  chkAchitatClick(nil);
  edTipDoc.Clear;
  with frmData.QryTipDoc do begin
    First;
    while not eof do begin
      with edTipDoc.Properties.Items.Add do begin
        Value := FieldByName('TIP_DOC').AsString;
        Description := FieldByName('TIP_DOC').AsString + ' - ' + FieldByName('DENUMIRE').AsString;
      end;
      Next;
    end;
  end;
  ReadPlata;  
end;

procedure TfrmDetaliiDocum.TreeStructuraDblClick(Sender: TObject);
begin
   with TcxDBTreeList(Sender) do
    if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk;
end;

procedure TfrmDetaliiDocum.TreeStructuraKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
     TreeStructuraDblClick(TcxDBTreeList(Sender))
  else if Key = VK_ESCAPE then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrCancel;
end;

procedure TfrmDetaliiDocum.edCasaBancaPropertiesPopup(Sender: TObject);
begin
  InternalPositioning(IntToStr(FCurentHouse), TreeStructura);
end;

procedure TfrmDetaliiDocum.RPCasaBancaEditChange(Sender: TObject);
begin
   if FIsInLoad then Exit;
   TRpATSEdit(Sender).Tag := -1;
end;

procedure TfrmDetaliiDocum.RPCasaBancaEditValidate(Sender: TObject;
  var DisplayValue: Variant; var ErrorText: TCaption; var Error: Boolean);
var
   lTree : TcxDBTreeList;
   lPanel : TcxRepartitorPanel;
   lTextC, lSearchC : TcxDBTreeListColumn;
   lTextIndex, lSearchIndex : Integer;
   lNode : TcxTreeListNode;
begin
   if FIsInLoad then Exit;
   TRpATSEdit(Sender).Tag := -1;
   if not(Sender is TRpAtsEdit) then Exit;
   lPanel := TcxRepartitorPanel(TRpAtsEdit(Sender).Parent);
   if not(lPanel.PopupEdit.PopupControl is TcxDBTreeList) then Exit;
   lTree := TcxDBTreeList(lPanel.PopupEdit.PopupControl);
   lSearchC := cxFindColumnByTag(lTree, -1);
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
      if not SetKeyOnPanelTree(lPanel, TRpATSEdit(Sender).Text, lTextIndex, lSearchIndex, lTree, (cxFindColumnByFieldName(lTree, lTree.DataController.KeyField).ItemIndex = lSearchIndex)) then begin
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
    SetCasaBanca;
  end;
end;

procedure TfrmDetaliiDocum.RPCasaBancaPopupCloseUp(Sender: TObject);
var
  lNode: TcxTreeListNode;
  lcxDBTree : TcxDBTreeList;
  lIdColumn : TcxTreeListColumn;
  lRepPanel :  TcxRepartitorPanel;
begin
   if FIsInLoad then Exit;
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

procedure TfrmDetaliiDocum.RPCasaBancaPopupInitPopup(Sender: TObject);
begin
  if FIsInLoad then Exit;
  with TcxPopupEdit(Sender).Properties do begin
    if PopupWidth < TcxPopupEdit(Sender).Width then PopupWidth := TcxPopupEdit(Sender).Width;
  end;
end;


procedure TfrmDetaliiDocum.RPCasaBancaValidate(Sender: TObject;
  var AKeyValue: Variant);
var
   lNode: TcxDBTreeListNode;
   lTree : TcxDBTreeList;
   lTextC, lSearchC : TcxDBTreeListColumn;
begin
   if FIsInLoad then Exit;
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
  if (RPCasaBanca.Tag <> -1)  then begin
    SetCasaBanca;
  end
  else
    SetCasaBanca;
end;

procedure TfrmDetaliiDocum.SetCasaBanca;
begin
  if FIdGestDocum = -1 then Exit;
  SetDocumentField('DECONT_COD_CB', RPCasaBanca.Tag);
end;

procedure TfrmDetaliiDocum.SetDocumentField(AFieldName: String; AValue: Variant);
var
  lField  : TField;
  lDataSet: TDataSet;
begin
  lDataSet := Inspector.DataController.DataSet;
  if not FIsInLoad and Assigned(lDataSet) then begin
    lField := lDataSet.FindField(AFieldName);
    if Assigned(lField) then begin
      if not (lField.DataSet.State in dsEditModes) then
        lField.DataSet.Edit;
      lField.Value := AValue;
      lField.DataSet.Post;
    end;
  end;
end;


procedure TfrmDetaliiDocum.edTipDocPropertiesValidate(Sender: TObject;
  var DisplayValue: Variant; var ErrorText: TCaption; var Error: Boolean);
begin
  SetDocumentField('DECONT_TIPDOC', edTipDoc.EditValue);
end;

procedure TfrmDetaliiDocum.edNrDocPropertiesValidate(Sender: TObject;
  var DisplayValue: Variant; var ErrorText: TCaption; var Error: Boolean);
begin
  SetDocumentField('DECONT_NRDOC', edNrDoc.Text);
end;

procedure TfrmDetaliiDocum.edDataPlataPropertiesValidate(Sender: TObject;
  var DisplayValue: Variant; var ErrorText: TCaption; var Error: Boolean);
begin
  SetDocumentField('DECONT_DATA', edDataPlata.Date);
end;

procedure TfrmDetaliiDocum.chkAchitatPropertiesValidate(Sender: TObject;
  var DisplayValue: Variant; var ErrorText: TCaption; var Error: Boolean);
begin
  SetDocumentField('DECONT_GENERATE', chkAchitat.Checked);
end;

procedure TfrmDetaliiDocum.ReadPlata;
var
  lDataSet : TDataSet;
  lField : TField;
  lNode : TcxTreeListNode;
begin
  lDataSet := Inspector.DataController.DataSet;
  if Assigned(lDataSet) then begin
    FIsInLoad := True;
    try
      lField := lDataSet.FindField('DECONT_GENERATE');
      chkAchitat.Checked := Assigned(lField) and lField.AsBoolean;

      rpCasaBanca.EditInput.EditValue := Null;
      rpCasaBanca.ListaInput.EditValue := Null;
      lField := lDataSet.FindField('DECONT_COD_CB');
      if Assigned(lField) and ValueHasValue(lField.Value) then begin
        rpCasaBanca.Tag := lField.AsInteger;
        lNode := TreeStructura.FindNodeByKeyValue(lField.Value);
        if Assigned(lNode) then begin
          lNode.Focused := True;
          lNode.MakeVisible;
          rpCasaBanca.EditInput.EditValue := lNode.Values[TreeStructuraCRSP_LEI.ItemIndex];
          rpCasaBanca.ListaInput.EditText := lNode.Values[TreeStructuraDENUMIRE.ItemIndex];
        end;
      end
      else rpCasaBanca.Tag := -1;

      lField := lDataSet.FindField('DECONT_NRDOC');
      if Assigned(lField) then edNrDoc.EditValue := lField.Value else edNrDoc.EditValue := Null;

      lField := lDataSet.FindField('DECONT_DATA');
      if Assigned(lField) and not lField.IsNull then edDataPlata.EditValue := lField.AsDateTime else edDataPlata.EditValue := Date;
      
      lField := lDataSet.FindField('DECONT_TIPDOC');
      if Assigned(lField) then edTipDoc.EditValue := ValueSafeToStr(lField.Value);

    finally
      FIsInLoad := False;
    end;
  end;
end;

procedure TfrmDetaliiDocum.ReadDocumentConex;
begin
  grupDocumentConext.Visible              := FHasDocConex;
  edDBNrDocConex.DataBinding.DataSource   := Inspector.DataController.DataSource;
  edDBDataDocConex.DataBinding.DataSource := Inspector.DataController.DataSource;
end;

procedure TfrmDetaliiDocum.FormShow(Sender: TObject);
begin
  edDBNrDocConex.DataBinding.DataSource   := Inspector.DataController.DataSource;
  edDBDataDocConex.DataBinding.DataSource := Inspector.DataController.DataSource;
end;

procedure TfrmDetaliiDocum.chkEmiteChitantaClick(Sender: TObject);
begin
  SetDocumentField('SE_GEN_CHITANTA', chkAchitat.Checked);
end;

function TfrmDetaliiDocum.FindRowByFieldName(
  const AFieldName: String): TcxDBEditorRow;
var
  I: Integer;
begin
  Result := nil;
  for I := 0 to Inspector.Rows.Count-1 do begin
    if Inspector.Rows[I].InheritsFrom(TcxDBEditorRow) then begin
      Result := TcxDBEditorRow(Inspector.Rows[I]);
      if Assigned(Result.Properties) and Assigned(Result.Properties.DataBinding) and SameText(Result.Properties.DataBinding.FieldName, AFieldName) then
        Break
      else
        Result := nil;
    end
    else
      Result := nil;
  end;
end;

function TfrmDetaliiDocum.FindRowByPartialFieldName(
  const AFieldName: String): TcxDBEditorRow;
var
  I: Integer;
begin
  Result := nil;
  for I := 0 to Inspector.Rows.Count-1 do begin
    if Inspector.Rows[I].InheritsFrom(TcxDBEditorRow) then begin
      Result := TcxDBEditorRow(Inspector.Rows[I]);
      if Assigned(Result.Properties) and Assigned(Result.Properties.DataBinding) and SameText(Copy(Result.Properties.DataBinding.FieldName, 1, Length(AFieldName)), AFieldName) then
        Break
      else
        Result := nil;
    end
    else
      Result := nil;
  end;
end;

procedure TfrmDetaliiDocum.UpdateCodEconomic(ACulGestDocum: Integer);
begin
  qryDocumentEconomic.Close;
  qryDocumentEconomic.Params[0].AsInteger := ACulGestDocum;
  qryDocumentEconomic.Open;
end;

end.
