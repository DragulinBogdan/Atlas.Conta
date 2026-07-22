unit AnexeParametriiCul;

interface

uses AnexeParametriiLista,
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, cxEdit, Menus,
  DB, ZDataSet, StdCtrls, cxButtons, cxControls,
  cxInplaceContainer, cxVGrid, cxDBTL, cxGrid, cxGridDBTableView,
  cxGridCustomView, frxADOComponents, cxDBLookupComboBox,
  cxDBData, cxGridLevel, cxGridCustomTableView, cxGridTableView,
  cxCheckBox, cxCurrencyEdit, cxGraphics, cxLookAndFeelPainters,
  ZAbstractRODataset, ZAbstractDataset, ZConnection,
  cxLookAndFeels, cxStyles,
  cxDataControllerConditionalFormattingRulesManagerDialog;

type
  TfrmAnexeParametriiCul = class(TForm)
    vgParamList: TcxVerticalGrid;
    btnOk: TcxButton;
    cxButton2: TcxButton;
    qryParamDefs: TZQuery;
    pmOptions: TPopupMenu;
    mnFilter: TMenuItem;
    mnExpand: TMenuItem;
    mnCollapse: TMenuItem;
    PopupMenu1: TPopupMenu;
    Reseteazacampuri1: TMenuItem;
    procedure cxButton2Click(Sender: TObject);
    procedure FormShow(Sender: TObject);
    procedure pmOptionsPopup(Sender: TObject);
    procedure mnFilterClick(Sender: TObject);
    procedure mnCollapseClick(Sender: TObject);
    procedure vgParamListKeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
    procedure vgParamListEnter(Sender: TObject);
    procedure vgParamListExit(Sender: TObject);
    procedure Reseteazacampuri1Click(Sender: TObject);
    procedure FormKeyUp(Sender: TObject; var Key: Word;
      Shift: TShiftState);
  private
    { Private declarations }
    FComp: TComponent;
    function GetPropClass(AParameterType: TAnexeParameterType): TcxCustomEditPropertiesClass; overload;
    function GetParamType(AType: String): TAnexeParameterType;
    function GetFieldSize(AParamID: Integer; AFName: String): Integer;
    function AddPopupTreeList(ADS: TDataSource; AParam: TAnexeParamItem): TcxDBTreeList;
    function AddPopupGrid(ADS: TDataSource; AParam: TAnexeParamItem): TcxGrid;
    function ControlUnderMouse: TControl;
    function GetControlHeight(AControl: TControl): Integer;
    function GetControlWidth(AControl: TControl): Integer;
    procedure PopKeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
    procedure PopDblClick(Sender: TObject);
    procedure SelectValueFromList;
    procedure SaveParams;
    function GetParamValue(AParam: TAnexeParamItem; AControl: TControl): Variant;
    function GetRowValue(ARow: TcxCustomRow): Variant;
    procedure CreateParamRows;
    procedure AddParamRow(AParam: TAnexeParamItem);  overload;
    procedure ActivateParamList;
    procedure SetParamValues;
  protected    
    procedure DeleteParamControls;
  public
    { Public declarations }
    FIdAnexaBilant : Integer;
    FOK: Boolean;
    FIDUtilizator: Integer;
    FParams: TAnexeParamList;
  end;

  function MonthName(AMonth: Integer): String;
  procedure PopupCloseUp(AObject: TObject);
  function StringIsEmpty(AString: String): Boolean;
  function ReplaceSemicolon(AString: String): String;


implementation

uses
  ZeosDBUtile, cxImageComboBox, cxCalendar, cxTextEdit, cxDropDownEdit,
  cxSpinEdit, cxTimeEdit, frxCustomDB, DateUtils, 
  dateUnit, cxDataUtils;

{$R *.dfm}
//------------------------------------------------------------------------------
function MonthName(AMonth: Integer): String;
begin
  Result := '';
  case AMonth of
     1: Result := 'Ianuarie';
     2: Result := 'Februarie';
     3: Result := 'Martie';
     4: Result := 'Aprilie';
     5: Result := 'Mai';
     6: Result := 'Iunie';
     7: Result := 'Iulie';
     8: Result := 'August';
     9: Result := 'Septembrie';
    10: Result := 'Octombrie';
    11: Result := 'Noiembrie';
    12: Result := 'Decembrie';
  end;
end;
//------------------------------------------------------------------------------
function StringIsEmpty(AString: String): Boolean;
begin
  Result := trim(AString) = '';     
end;
//------------------------------------------------------------------------------
function ReplaceSemicolon(AString: String): String;
begin
  Result := StringReplace(AString, ';', ',', [rfReplaceAll]);
end;
//------------------------------------------------------------------------------
function TfrmAnexeParametriiCul.GetPropClass(AParameterType: TAnexeParameterType): TcxCustomEditPropertiesClass;
begin
  case AParameterType of
    ptDefault:        Result := TcxTextEditProperties;
    ptTextEdit:       Result := TcxTextEditProperties;
    ptImageComboBox:  Result := TcxImageComboBoxProperties;
    ptComboBox:       Result := TcxComboBoxProperties;
    ptPopupEdit:      Result := TcxPopupEditProperties;
    ptCurrencyEdit:   Result := TcxCurrencyEditProperties;
    ptSpinEdit:       Result := TcxSpinEditProperties;
    ptDateEdit:       Result := TcxDateEditProperties;
    ptTimeEdit:       Result := TcxTimeEditProperties;
    ptCheckBox:       Result := TcxCheckBoxProperties;
    ptLookupComboBox: Result := TcxLookupComboBoxProperties;
  else
    Result := TcxTextEditProperties;
  end;
end;
//------------------------------------------------------------------------------
function TfrmAnexeParametriiCul.GetParamType(AType: String): TAnexeParameterType;
begin
  Result := ptDefault;
  if SameText(AType, 'TextEdit') then Result := ptTextEdit;
  if SameText(AType, 'ImageComboBox') then Result := ptImageComboBox;
  if SameText(AType, 'ComboBox') then Result := ptComboBox;
  if SameText(AType, 'PopupEdit')     then Result := ptPopupEdit;
  if SameText(AType, 'CurrencyEdit') then Result := ptCurrencyEdit;
  if SameText(AType, 'DateEdit') then Result := ptDateEdit;
  if SameText(AType, 'SpinEdit') then Result := ptSpinEdit;
  if SameText(AType, 'TimeEdit') then Result := ptTimeEdit;
  if SameText(AType, 'CheckBox') then Result := ptCheckBox;
  if SameText(AType, 'AnLuna') then Result := ptAnLuna;
end;
//------------------------------------------------------------------------------
procedure TfrmAnexeParametriiCul.cxButton2Click(Sender: TObject);
begin
  if TcxButton(Sender).Tag = 0 then
    SaveParams;
  Close;
end;
//------------------------------------------------------------------------------
procedure TfrmAnexeParametriiCul.FormShow(Sender: TObject);
var
  lRefresh: Boolean;
begin
  if (FParams = nil) then exit;
//  if FReport.DataSets.Count = 0 then exit;

  lRefresh := FParams.ParamCount > 0;
  if not lRefresh then
    FParams.ReadAnexaParams(frmData.dbContabilitate);
  CreateParamRows;
  if lRefresh then
    SetParamValues;
  ActivateParamList;
end;
//------------------------------------------------------------------------------
function TfrmAnexeParametriiCul.GetFieldSize(AParamID: Integer; AFName: String): Integer;
begin
  Result := ValueSafeToInt( DBGetScallarFmt('select top 1 ColWidth from ANEXE_PARAMETRII_COLOANE where ID_ANEXE_PARAMETRII=%d and ColName = %s',[AParamID, ValueToStr(AFName)]), 200);
end;
//------------------------------------------------------------------------------
function TfrmAnexeParametriiCul.AddPopupGrid(ADS: TDataSource; AParam: TAnexeParamItem): TcxGrid;
var
  lGrd: TcxGrid;
  lView: TcxGridDBTableView;
  i: Integer;
  lFName: String;
  lQry: TZReadOnlyQuery;
  lFieldList: String;
begin
  lQry := TZReadOnlyQuery(ADS.DataSet);
  lGrd := TcxGrid.Create(self);
  lGrd.Parent := self;
  lGrd.Visible := False;
  lView := TcxGridDBTableView(lgrd.CreateView(TcxGridDBTableView));
  lgrd.Levels.Add.GridView := lView;
  lFieldList := ','+ReplaceSemicolon(AParam.FieldList)+',';

  with lView do
  begin
    OptionsView.GroupByBox := False;
    OptionsData.Editing := False;
    OptionsData.Inserting := False;
    OptionsData.Deleting := False;
    OptionsData.Appending := False;
    DataController.DataSource := ADS;
    OptionsBehavior.IncSearch := True;
    PopupMenu := pmOptions;
    FilterRow.InfoText := 'Click aici pentru a defini un filtru';
    OnDblClick := PopDblClick;
    OnKeyDown := PopKeyDown;
  end;

  for i := 0 to lqry.FieldCount - 1 do
  begin
    lFName := lqry.Fields.Fields[i].FieldName;
    if pos(','+lFName+',', lFieldList) > 0 then
      with lView.CreateColumn do
      begin
        Caption := StringReplace(lFName, '_', ' ', [rfReplaceAll]);
        DataBinding.FieldName := lFName;
        Width := GetFieldSize(AParam.ID, lFName);
      end;
  end;

  lView.OptionsView.ColumnAutoWidth := AParam.ColumnAutoWidth;
  lgrd.Height:= GetControlHeight(lgrd);
  if AParam.ControlWidth > 0 then
    lGrd.Width := AParam.ControlWidth
  else
    lGrd.Width := GetControlWidth(lgrd);
  Result := lGrd;
end;
//------------------------------------------------------------------------------
function TfrmAnexeParametriiCul.AddPopupTreeList(ADS: TDataSource; AParam: TAnexeParamItem): TcxDBTreeList;
var
  lTreeList: TcxDBTreeList;
  i: Integer;
  lFName: String;
  lQry: TZReadOnlyQuery;
  lFieldList: String;
begin
  lQry := TZReadOnlyQuery(ADS.DataSet);
  lTreeList := TcxDBTreeList.Create(self);
  lFieldList := ','+ReplaceSemicolon(AParam.FieldList)+',';
  with lTreeList do
  begin
    Parent := self;
    Visible := False;
    OptionsData.Inserting := False;
    OptionsData.Editing := False;
    OptionsData.Deleting := False;
    DataController.DataSource := ADS;
    PopupMenu := pmOptions;
    OnDblClick := PopDblClick;
    OnKeyDown := PopKeyDown;
    for i := 0 to lqry.FieldCount - 1 do
    begin
      lFName := lqry.Fields.Fields[i].FieldName;
      if pos(','+lFName+',', lFieldList) > 0 then
        with TcxDBTreeListColumn(CreateColumn(Bands.FirstVisible)) do
        begin
          Caption.Text := StringReplace(lFName, '_', ' ', [rfReplaceAll]);
          DataBinding.FieldName := lFName;
          Width := GetFieldSize(AParam.ID, lFName);
        end;
    end;

    OptionsBehavior.IncSearch := True;
    OptionsBehavior.ExpandOnIncSearch := True;
    OptionsBehavior.IncSearchItem := VisibleColumns[0];
    OptionsBehavior.ImmediateEditor := False;

    OptionsView.ColumnAutoWidth := AParam.ColumnAutoWidth;
    DataController.KeyField := AParam.IDField;
    DataController.ParentField := AParam.ParentField;
    OptionsView.ShowRoot := DataController.ParentField <> DataController.KeyField;
    Height:= GetControlHeight(lTreeList);

    if AParam.ControlWidth > 0 then
      Width := AParam.ControlWidth
    else
      Width := GetControlWidth(lTreeList);
  end;
  Result := lTreeList;
end;
//------------------------------------------------------------------------------
function TfrmAnexeParametriiCul.ControlUnderMouse: TControl;
var hWnd: THandle;
begin
  hWnd := WindowFromPoint(Mouse.CursorPos);
  Result := FindControl(hWnd);

  {TcxGridSite --> TcxGridDBTableView; TcxControlScrollBar --> TcxGridSite}
  {TcxControlScrollBar --> TcxDBTreeList; TcxSizeGrip --> TcxGridDBTableView}
  if Result is TcxControlScrollBar then
    Result := TcxControlScrollBar(Result).Parent;
  if Result is TcxSizeGrip then
    Result := TcxSizeGrip(Result).Parent;
  if Result is TcxGridSite then
    Result := TControl(TcxGridSite(Result).GridView);
end;
//------------------------------------------------------------------------------
function TfrmAnexeParametriiCul.GetControlHeight(AControl: TControl): Integer;
var
  lCount: Integer;
  lSize: Integer;
begin
  Result := 300;
  lSize := 0;
  lCount := 0;
  if AControl is TcxGrid then
  begin
    lCount := TcxGridDBTableView(TcxGrid(AControl).Views[0]).DataController.RecordCount;
    lSize := lCount * 17; //17 = rowheight
  end
  else
    if AControl is TcxDBTreeList then
    begin
      lCount := TcxDBTreeList(AControl).AbsoluteCount;
      lSize := lCount * TcxDBTreeList(AControl).DefaultRowHeight;
    end;

  if lCount > 0 then
    if lSize > (Screen.Height div 2) then
      Result := Screen.Height div 2 - 30
    else
      Result := lSize + 50; //50 = header.height+scrollbar.height
end;
//------------------------------------------------------------------------------
function TfrmAnexeParametriiCul.GetControlWidth(AControl: TControl): Integer;
var
  i: Integer;
  lWidth: Integer;
begin
  Result := 300;
  lWidth := 0;
  if AControl is TcxGrid then
    with TcxGridDBTableView(TcxGrid(AControl).Views[0]) do
      for i := 0 to ColumnCount - 1 do
        lWidth := lWidth + Columns[i].Width
  else
    if AControl is TcxDBTreeList then
      with TcxDBTreeList(AControl) do
        for i := 0 to ColumnCount - 1 do
          lWidth := lWidth + Columns[i].Width;

  if lWidth > 0 then
    if lWidth > Screen.Width then Result := Screen.Width div 2
    else
      if (lWidth > Result) or (lWidth>150) then Result := lWidth + 20;    //20 = scrollbar width
end;
//------------------------------------------------------------------------------
procedure PopupCloseUp(AObject: TObject);
var PopupForm: TCustomForm;
begin
  PopupForm := GetParentForm(TControl(AObject));
  if PopupForm is TcxPopupEditPopupWindow then
    TcxPopupEditPopupWindow(PopupForm).CloseUp;
end;
//------------------------------------------------------------------------------
procedure TfrmAnexeParametriiCul.pmOptionsPopup(Sender: TObject);
var
  lComp: TComponent;
  lArbore: Boolean;
begin
  lComp := TComponent(ControlUnderMouse);
  if lComp = nil then exit;

  FComp := lComp;
  mnFilter.Visible := lComp is TcxGridDBTableView;
  if lComp is TcxGridDBTableView then
    mnFilter.Checked := TcxGridDBTableView(lComp).FilterRow.Visible;

  lArbore := lComp is TcxDBTreeList;
  if lArbore then lArbore := TcxDBTreeList(lComp).DataController.KeyField <> TcxDBTreeList(lComp).DataController.ParentField;

  mnExpand.Visible := lArbore;
  mnCollapse.Visible := mnExpand.Visible;
end;
//------------------------------------------------------------------------------
procedure TfrmAnexeParametriiCul.mnFilterClick(Sender: TObject);
begin
  if FComp = nil then exit;
  TcxGridDBTableView(FComp).FilterRow.Visible := TMenuItem(Sender).Checked;
end;
//------------------------------------------------------------------------------
procedure TfrmAnexeParametriiCul.mnCollapseClick(Sender: TObject);
begin
  if FComp = nil then exit;
  if TMenuItem(Sender).Tag = 0 then TcxDBTreeList(FComp).FullExpand
  else TcxDBTreeList(FComp).FullCollapse;
end;
//------------------------------------------------------------------------------
procedure TfrmAnexeParametriiCul.PopKeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
begin
  if Key = 13 then
  begin
    SelectValueFromList;
    PopupCloseUp(Sender);
  end;
  if Key = 27 then PopupCloseUp(Sender);
end;
//------------------------------------------------------------------------------
procedure TfrmAnexeParametriiCul.PopDblClick(Sender: TObject);
begin
  if Sender is TcxDBTreeList then
    if Assigned(TcxDBTreeList(Sender).FocusedNode) then
      if TcxDBTreeList(Sender).FocusedNode.HasChildren then exit;
  SelectValueFromList;
  PopupCloseUp(Sender);
end;
//------------------------------------------------------------------------------
procedure TfrmAnexeParametriiCul.SelectValueFromList;
var
  lControl: TControl;
  lQry: TZQuery;
  lRow: TcxEditorRow;
  lField: TField;
  lParam: TAnexeParamItem;
begin
  lRow := TcxEditorRow(vgParamList.FocusedRow);
  if lRow = nil then exit;
  lParam := TAnexeParamItem(lRow.Tag);
  if not Assigned(lParam) then exit;

  lControl := TcxPopupEditProperties(lRow.Properties.EditProperties).PopupControl;
  if lControl is TcxGrid then
    lQry := TZQuery(TcxGridDBTableView(TcxGrid(lControl).Views[0]).DataController.DataSource.DataSet)
  else
    lQry := TZQuery(TcxDBTreeList(lControl).DataController.DataSource.DataSet);

  lField := lQry.FindField(lParam.DisplayField);
  if Assigned(lField) then
  begin
    vgParamList.InplaceEditor.EditValue := lField.Value;
    lRow.Properties.Value := lField.Value;
  end;
end;
//------------------------------------------------------------------------------
procedure TfrmAnexeParametriiCul.vgParamListKeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
var lEditor: TcxMultiEditorRow;
  //============================================================================
  procedure FocusOK;
  begin
    vgParamList.HideEdit;
    btnOK.SetFocus;
  end;
  //============================================================================
begin
  case Key of
    VK_DOWN: if vgParamList.FocusedRow.Index = vgParamList.Rows.Count - 1 then FocusOK;
    VK_UP: if vgParamList.FocusedRow.Index = 0 then FocusOK;
    VK_DELETE: if vgParamList.FocusedRow is TcxEditorRow then
                 TcxEditorRow(vgParamList.FocusedRow).Properties.Value := NULL;
    VK_INSERT: if vgParamList.FocusedRow is TcxMultiEditorRow then
               begin
                 lEditor := TcxMultiEditorRow(vgParamList.FocusedRow);
                 if lEditor.Properties.Editors.Items[0].EditPropertiesClass = TcxImageComboBoxProperties then
                    lEditor.Properties.Editors.Items[0].Value := MonthOf(Today);
                 if lEditor.Properties.Editors.Items[1].EditPropertiesClass = TcxSpinEditProperties then
                    lEditor.Properties.Editors.Items[1].Value := YearOf(Today);
               end
               else
                 if TcxEditorRow(vgParamList.FocusedRow).Properties.EditPropertiesClass = TcxDateEditProperties then
                 begin
                   TcxEditorRow(vgParamList.FocusedRow).Properties.Value := Today;
                   if Assigned(vgParamList.InplaceEditor) then
                      vgParamList.InplaceEditor.EditValue := Today;
                 end;
  end;
end;
//------------------------------------------------------------------------------
procedure TfrmAnexeParametriiCul.vgParamListEnter(Sender: TObject);
begin
  vgParamList.OptionsView.ShowEditButtons := ecsbFocused;
end;
//------------------------------------------------------------------------------
procedure TfrmAnexeParametriiCul.vgParamListExit(Sender: TObject);
begin
  vgParamList.OptionsView.ShowEditButtons := ecsbNever;
end;
//------------------------------------------------------------------------------
procedure TfrmAnexeParametriiCul.SaveParams;
var
  i: Integer;
  lPRow: TcxCustomRow;
  lPar: TAnexeParamItem;
begin
  for i := 0 to vgParamList.Rows.Count - 1 do
  begin
    lPRow := TcxCustomRow(vgParamList.Rows.Items[i]);
    lPar := TAnexeParamItem(lPRow.Tag);
    if Assigned(lPar) then
      lPar.Value := GetRowValue(lPRow);
  end;
//  FParams.SetAnexaParams(frmData.dbContabilitate);

  FOK := True;
//  ModalResult := mrOk;
end;
//------------------------------------------------------------------------------
function TfrmAnexeParametriiCul.GetParamValue(AParam: TAnexeParamItem; AControl: TControl): Variant;
var
  locQry: TZQuery;
begin
  Result := NULL;
  if (AControl = nil) or (AParam = nil) then exit;
  locQry := nil;
  if AControl is TcxGrid then
    locQry := TZQuery(TcxGridDBTableView(TcxGrid(AControl).Views[0]).DataController.DataSource.DataSet)
  else
    if AControl is TcxDBTreeList then
      locQry := TZQuery(TcxDBTreeList(AControl).DataController.DataSource.DataSet);

  if Assigned(locQry) and not StringIsEmpty(AParam.KeyField) then
      Result := locQry.FieldByName(AParam.KeyField).Value;
end;
//------------------------------------------------------------------------------
function TfrmAnexeParametriiCul.GetRowValue(ARow: TcxCustomRow): Variant;
var
  lMRow: TcxMultiEditorRow;
  lERow: TcxEditorRow;
  lAn, lLuna: Integer;
begin
  Result := NULL;
  if ARow is TcxMultiEditorRow then
  begin
    lMRow := TcxMultiEditorRow(ARow);
    with lMRow.Properties.Editors do
      if (Items[0].EditPropertiesClass = TcxImageComboBoxProperties) and (Items[1].EditPropertiesClass = TcxSpinEditProperties) then
      begin
        lLuna := Items[0].Value;
        lAn :=   Items[1].Value;
        Result := lAn * 100 + lLuna;
      end;
  end
  else
    if ARow is TcxEditorRow then
    begin
      lERow := TcxEditorRow(ARow);
      if trim(VarToStr(lERow.Properties.Value))>'' then
        if (lERow.Properties.EditProperties is TcxPopupEditProperties) and (lERow.Properties.Value<>NULL) then
          Result := GetParamValue(TAnexeParamItem(lERow.Tag), TcxPopupEditProperties(lERow.Properties.EditProperties).PopupControl)
        else
          Result := lERow.Properties.Value;
    end;
end;
//------------------------------------------------------------------------------
procedure TfrmAnexeParametriiCul.Reseteazacampuri1Click(Sender: TObject);
var i, k: Integer;
begin
  for i := 0 to vgParamList.Rows.Count - 1 do
    with vgParamList.Rows do
      if Items[i] is TcxEditorRow then
        TcxEditorRow(Items[i]).Properties.Value := NULL
      else
        if Items[i] is TcxMultiEditorRow then
          with TcxMultiEditorRow(Items[i]).Properties.Editors do
            for k := 0 to Count - 1 do
              if Items[k].EditPropertiesClass = TcxImageComboBoxProperties then
                Items[k].Value := MonthOf(Today)
              else
                if Items[k].EditPropertiesClass = TcxSpinEditProperties then
                  Items[k].Value := YearOf(Today)
end;
//------------------------------------------------------------------------------
procedure TfrmAnexeParametriiCul.CreateParamRows;
var i: Integer;
begin
  vgParamList.ClearRows;
  for i := 0 to FParams.ParamCount - 1 do
    AddParamRow(FParams.Params[i]);
end;
//------------------------------------------------------------------------------
procedure TfrmAnexeParametriiCul.AddParamRow(AParam: TAnexeParamItem);
  //============================================================================
  procedure AddMultiEditorRow;
  var i: Integer;
  begin
    with TcxMultiEditorRow(vgParamList.Add(TcxMultiEditorRow)) do
    begin
      with Properties.Editors.Add do
      begin
        Tag := Integer(AParam);
        Caption := AParam.Alias;
        Width := 70;
        EditPropertiesClass := TcxImageComboBoxProperties;
        TcxImageComboBoxProperties(EditProperties).DropDownRows := 12;
        for i := 1 to 12 do
          with TcxImageComboBoxProperties(EditProperties).Items.Add do
          begin
            Value := i;
            Description := MonthName(i);
          end;
        if Trim(VarToStr(AParam.Value)) = '' then Value := MonthOf(Today)
        else Value := AParam.Value mod 100;
      end;
      with Properties.Editors.Add do
      begin
        if LowerCase(AParam.Alias) <> 'luna' then Caption := 'luna';
        EditPropertiesClass := TcxSpinEditProperties;
        TcxSpinEditProperties(EditProperties).MaxValue := 2099;
        TcxSpinEditProperties(EditProperties).MinValue := 1900;
        if Trim(VarToStr(AParam.Value)) = '' then Value := YearOf(Today)
        else Value := AParam.Value div 100;
      end;
    end;
  end;
  //============================================================================
  procedure PopulateComboBox(ACombo:TcxComboBoxProperties);
  begin
    if not StringIsEmpty(AParam.DescList) then
      ACombo.Items.Text := StringReplace(AParam.DescList, ';', #13#10, [rfReplaceAll]);
  end;
  //============================================================================
  procedure PopulateImageComboBox(ACombo:TcxImageComboBoxProperties);
  var
    lVal, lDesc: TStringList;
    j: Integer;
  begin
    lVal := TStringList.Create;
    lDesc    := TStringList.Create;
    try
      ACombo.BeginUpdate;
      lVal.Text := StringReplace(AParam.ValueList, ';', #13#10, [rfReplaceAll]);
      lDesc.Text := StringReplace(AParam.DescList, ';', #13#10, [rfReplaceAll]);
      for J := 0 to lVal.Count-1 do
        with ACombo.Items.Add do
        begin
          Value := lVal[J];
          if lDesc.Count > J then
            Description := lDesc[J]
          else
            Description := lVal[J];
        end;
    finally
      ACombo.EndUpdate;
      lVal.Free;
      lDesc.Free;
    end;
  end;
  //============================================================================
  procedure LoadComboBoxFromDB(ACombo: TcxComboBoxProperties);
  var
    lDataSet: TDataSet;
  begin
    if StringIsEmpty(AParam.FieldList) then exit;
    lDataSet := DBNewQueryFmt('select %s from %s', [ReplaceSemicolon(AParam.FieldList), AParam.SourceTable]);
    try
      ACombo.BeginUpdate;
      While not lDataSet.Eof do begin
        ACombo.Items.Add(lDataSet.Fields[0].AsString);
        Next;
      end;
    finally
      ACombo.EndUpdate;
      lDataSet.Free;
    end;
  end;
  //============================================================================
  procedure LoadImageComboBoxFromDB(ACombo: TcxImageComboBoxProperties);
  var
    lDataSet: TDataSet;
  begin
    if StringIsEmpty(AParam.KeyField) or StringIsEmpty(AParam.FieldList) then exit;
    lDataSet := DBNewQueryFmt('select %s, %s from %s', [AParam.KeyField, ReplaceSemicolon(AParam.FieldList), AParam.SourceTable]);
    try
      while not lDataSet.Eof do begin
        ACombo.BeginUpdate;
        with ACombo.Items.Add do begin
          Value       := lDataSet.FieldByName(AParam.KeyField).AsString;
          Description := lDataSet.FieldByName(AParam.FieldList).AsString;
        end;
        lDataSet.Next;
      end;
    finally
      ACombo.EndUpdate;
      lDataSet.Free;
    end;
  end;
  //============================================================================
  procedure LoadLookupFromDB(ACombo: TcxLookupComboBoxProperties);
  var
    lDataSet: TDataSet;
  begin
    if StringIsEmpty(AParam.KeyField) or StringIsEmpty(AParam.FieldList) then exit;
    lDataSet := DBNewQueryFmt('select %s, %s from %s', [AParam.KeyField, ReplaceSemicolon(AParam.FieldList), AParam.SourceTable]);
    ACombo.ListSource := TDataSource.Create(self);
    ACombo.ListSource.DataSet := lDataSet;
    ACombo.ListFieldNames := StringReplace(AParam.FieldList, ',', ';', [rfReplaceAll]);
    ACombo.KeyFieldNames := AParam.KeyField;
    ACombo.DropDownWidth := 400;
    ACombo.DropDownRows := 15;

    AParam.Query := lDataSet;
    AParam.DataSource := ACombo.ListSource;
  end;
  //============================================================================
  procedure LoadPopupEditFromDB(APopup: TcxPopupEditProperties);
  var
    lDataSet: TDataSet;
    lds: TDataSource;
    lFields: String;
  begin
    APopup.PopupSysPanelStyle := True;

    if StringIsEmpty(AParam.IDField) or StringIsEmpty(AParam.KeyField) or StringIsEmpty(AParam.FieldList) then exit;
    lFields := AParam.IDField+','+AParam.KeyField+','+ReplaceSemicolon(AParam.FieldList);
    if not StringIsEmpty(AParam.ParentField) then
      lFields := lFields + ',' + Aparam.ParentField;

    lFields := '['+StringReplace(lFields, ',', '],[', [rfReplaceAll])+']';
    lDataSet := DBNewQueryFmt('select %s from %s', [lFields, AParam.SourceTable]);
    lds := TDataSource.Create(self);
    lds.DataSet := lDataSet;
    lDataSet.Open;

    if StringIsEmpty(AParam.ParentField) then
      APopup.PopupControl := AddPopupGrid(lds, AParam)
    else
      APopup.PopupControl := AddPopupTreeList(lds, AParam);

    AParam.Query := lDataSet;
    AParam.DataSource := lds;
    AParam.PopupControl := APopup.PopupControl;
  end;
  //============================================================================
  procedure AddEditorRow;
  var
    lEditorRow: TcxEditorRow;
  begin
    lEditorRow := TcxEditorRow(vgParamList.Add(TcxEditorRow));
    with lEditorRow do
    begin
      Tag := Integer(AParam);
      Properties.Caption := AParam.Alias;
      Properties.EditPropertiesClass := GetPropClass(AParam.ParameterType);

      if (Properties.EditPropertiesClass = TcxCheckBoxProperties) and (AParam.Value = NULL) then
        Properties.Value := False;

      if trim(VarToStr(AParam.Value)) > '' then
        Properties.Value := AParam.Value;

      if StringIsEmpty(AParam.SourceTable) then
        case AParam.ParameterType of
          ptComboBox:      PopulateComboBox(TcxComboBoxProperties(Properties.EditProperties));
          ptImageComboBox: PopulateImageComboBox(TcxImageComboBoxProperties(Properties.EditProperties));
        end
      else
        case AParam.ParameterType of
          ptComboBox:       LoadComboBoxFromDB(TcxComboBoxProperties(Properties.EditProperties));
          ptImageComboBox:  LoadImageComboBoxFromDB(TcxImageComboBoxProperties(Properties.EditProperties));
          ptLookupComboBox: LoadLookupFromDB(TcxLookupComboBoxProperties(Properties.EditProperties));
          ptPopupEdit:      LoadPopupEditFromDB(TcxPopupEditProperties(Properties.EditProperties));
        end;
    end;
  end;
  //============================================================================
var
  lPos: Integer;
  lPType: String;
begin
  if SameText(AParam.Name, 'IDUtilizator') then exit;
  lPos := pos('@', AParam.Name);
  if (lPos>0) and (AParam.ParameterType = ptDefault) then
  begin
    AParam.Alias := Copy(AParam.Name, 1, lPos - 1);
    lPType := Copy(AParam.Name, lPos + 1, length(AParam.Name)-lPos+1);
    AParam.ParameterType := GetParamType(lPType);
  end;

  if StringIsEmpty(AParam.Alias) then AParam.Alias := AParam.Name;
  AParam.Alias := StringReplace(AParam.Alias, '_', ' ', [rfReplaceAll]);

  if AParam.ParameterType = ptAnLuna then
    AddMultiEditorRow
  else
    AddEditorRow;
end;
//------------------------------------------------------------------------------
procedure TfrmAnexeParametriiCul.ActivateParamList;
begin
  if vgParamList.Rows.Count > 0 then
  begin
    vgParamList.SetFocus;
    vgParamList.Rows.Items[0].Focused := True;
    vgParamList.ShowEdit;
  end;
end;
//------------------------------------------------------------------------------
procedure TfrmAnexeParametriiCul.SetParamValues;
var
  i: Integer;
  lParam: TAnexeParamItem;
  lRow: TcxCustomRow;
  lMRow: TcxMultiEditorRow;
  lERow: TcxEditorRow;
  //============================================================================
  procedure SetPopupEditRowValue(AParam: TAnexeParamItem; ARow: TcxEditorRow);
  var
    lControl: TControl;
    lQry: TZQuery;
    lPopupEdit: TcxPopupEditProperties;
  begin
    if ARow = nil then exit;
    lPopupEdit := TcxPopupEditProperties(ARow.Properties.EditProperties);
    lControl := lPopupEdit.PopupControl;
    if lControl is TcxGrid then
      lQry := TZQuery(TcxGridDBTableView(TcxGrid(lControl).Views[0]).DataController.DataSource.DataSet)
    else
      lQry := TZQuery(TcxDBTreeList(lControl).DataController.DataSource.DataSet);

    if lQry = nil then exit;

    if Trim(VarToStr(AParam.Value)) <> '' then
      if lQry.Locate(AParam.KeyField, AParam.Value, []) then
        ARow.Properties.Value := lQry.FieldByName(AParam.DisplayField).Value;
  end;
  //============================================================================
begin
  for i := 0 to vgParamList.Rows.Count - 1 do
  begin
    lRow := TcxCustomRow(vgParamList.Rows.Items[i]);
    lParam := TAnexeParamItem(lRow.Tag);
    if not Assigned(lParam) then Continue;
    
    if trim(VarToStr(lParam.Value))>'' then
      if lRow is TcxMultiEditorRow then
      begin
        lMRow := TcxMultiEditorRow(lRow);
        with lMRow.Properties.Editors do
          if (Items[0].EditPropertiesClass = TcxImageComboBoxProperties) and (Items[1].EditPropertiesClass = TcxSpinEditProperties) then
          begin
            Items[0].Value := lParam.Value mod 100;
            Items[1].Value := lParam.Value div 100;
          end;
      end
      else
        if lRow is TcxEditorRow then
        begin
          lERow := TcxEditorRow(lRow);
          if (lERow.Properties.EditProperties is TcxPopupEditProperties) and (lERow.Properties.Value<>NULL) then
            SetPopupEditRowValue(lParam, lERow)
          else
            lERow.Properties.Value := lParam.Value;
        end;
  end;
end;
//------------------------------------------------------------------------------
procedure TfrmAnexeParametriiCul.FormKeyUp(Sender: TObject; var Key: Word; Shift: TShiftState);
begin
end;
//------------------------------------------------------------------------------
procedure TfrmAnexeParametriiCul.DeleteParamControls;
var
  i: Integer;
  lParam: TAnexeParamItem;
begin
  for i := 0 to FParams.ParamCount - 1 do
  begin
    lParam := FParams.Params[i];

    if Assigned(lParam.Query) then
      lparam.Query.Free;

    if Assigned(lParam.DataSource) then
     lParam.DataSource.Free;

    if Assigned(lParam.PopupControl) then
      lparam.PopupControl.Free;
  end;
end;
//------------------------------------------------------------------------------
end.
