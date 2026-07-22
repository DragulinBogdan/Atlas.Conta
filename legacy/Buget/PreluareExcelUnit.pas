unit PreluareExcelUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  Db, dxmdaset, ExtCtrls, StdCtrls,
  cxGroupBox, cxRepartitorPanel, cxControls, cxContainer, cxEdit, cxLabel,
  cxGraphics, cxTL, cxMaskEdit, cxInplaceContainer,
  cxDBTL, cxTLData, cxDropDownEdit, cxTextEdit, cxImageComboBox,
   cxDataStorage, cxDBData,
  cxGridLevel, cxClasses, cxGridCustomView, cxGridCustomTableView,
  cxGridTableView, cxGridDBTableView, cxGrid, cxCurrencyEdit,
  cxGridCustomPopupMenu, cxGridPopupMenu, Menus, cxLookAndFeelPainters,
  cxButtons, cxProgressBar, 
  cxTLdxBarBuiltInMenu,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, cxNavigator,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxBarBuiltInMenu,
  dxDateRanges;

type
  TfrmPreluareExcel = class(TForm)
    DTGenerate: TDataSource;
    Preluate: TdxMemData;
    PreluateCOD_FUNCTIONAL: TStringField;
    PreluateCOD_ECONOMIC: TStringField;
    PreluatePLANIFICAT_REST: TCurrencyField;    
    PreluatePLANIFICAT1: TCurrencyField;
    PreluatePLANIFICAT2: TCurrencyField;
    PreluatePLANIFICAT3: TCurrencyField;
    PreluatePLANIFICAT4: TCurrencyField;
    pnTop: TPanel;
    PreluateEROARE: TIntegerField;
    LbAnFiscal: TLabel;
    LbRevizie: TLabel;
    LbMultiplicator: TLabel;
    PreluateCOD_ECONOMIC_ECRAN: TStringField;
    cxLabel1: TcxLabel;
    RPFunctional1: TcxRepartitorPanel;
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
    cxTreeProiecte: TcxDBTreeList;
    cxTreeProiecteID_OI_PROIECTE: TcxDBTreeListColumn;
    cxTreeProiecteID_OI_TIPURI_PROIECTE: TcxDBTreeListColumn;
    cxTreeProiecteID_PARINTE: TcxDBTreeListColumn;
    cxTreeProiecteDENUMIRE: TcxDBTreeListColumn;
    cxTreeProiecteDESCRIERE: TcxDBTreeListColumn;
    cxTreeProiecteSTARE: TcxDBTreeListColumn;
    cxTreeProiecteCOD_FUNCTIONAL: TcxDBTreeListColumn;
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
    RPProiect: TcxRepartitorPanel;
    lbDefalcare: TcxLabel;
    edCategorie: TcxImageComboBox;
    edAnFiscal: TcxImageComboBox;
    edMultiplicator: TcxImageComboBox;
    edRevizie: TcxImageComboBox;
    GridPreluare: TcxGrid;
    GridPreluareL: TcxGridLevel;
    GridPreluareV: TcxGridDBTableView;
    GridPreluareVEROARE: TcxGridDBColumn;
    GridPreluareVCOD_FUNCTIONAL: TcxGridDBColumn;
    GridPreluareVCOD_ECONOMIC_ECRAN: TcxGridDBColumn;
    GridPreluareVCOD_ECONOMIC: TcxGridDBColumn;
    GridPreluareVPLANIFICAT1: TcxGridDBColumn;
    GridPreluareVPLANIFICAT2: TcxGridDBColumn;
    GridPreluareVPLANIFICAT3: TcxGridDBColumn;
    GridPreluareVPLANIFICAT4: TcxGridDBColumn;
    cxTreeEconomic: TcxDBTreeList;
    cxTreeEconomicID_BG_PLAN_ECONOMIC: TcxDBTreeListColumn;
    cxTreeEconomicCOD_ECONOMIC: TcxDBTreeListColumn;
    cxTreeEconomicDENUMIRE: TcxDBTreeListColumn;
    cxTreeEconomicDESCRIERE: TcxDBTreeListColumn;
    cxTreeEconomicNUMAR_RAND: TcxDBTreeListColumn;
    cxTreeEconomicID_PARINTE: TcxDBTreeListColumn;
    cxTreeEconomicCLASA: TcxDBTreeListColumn;
    cxTreeEconomicESTE_LOCAL: TcxDBTreeListColumn;
    cxGridPopupMenu: TcxGridPopupMenu;
    BtnPreia: TcxButton;
    BtnCancel: TcxButton;
    PreluareProgress: TcxProgressBar;
    GridPreluareVPLANIFICAT_REST: TcxGridDBColumn;
    PreluatePLUS1AN: TCurrencyField;
    PreluatePLUS2AN: TCurrencyField;
    PreluatePLUS3AN: TCurrencyField;
    GridPreluareVPLUS1AN: TcxGridDBColumn;
    GridPreluareVPLUS2AN: TcxGridDBColumn;
    GridPreluareVPLUS3AN: TcxGridDBColumn;
    procedure BtnCancelClick(Sender: TObject);
    procedure BtnPreiaClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure RPFunctional1PopupInitPopup(Sender: TObject);
    procedure cxTreeFunctionalDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure cxTreeProiecteDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure cxTreeUnitatiDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure edCategoriePropertiesChange(Sender: TObject);
    procedure GridPreluareVCOD_FUNCTIONALPropertiesPopup(Sender: TObject);
    procedure GridPreluareVCOD_FUNCTIONALPropertiesCloseQuery(
      Sender: TObject; var CanClose: Boolean);
    procedure cxTreeEconomicDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure GridPreluareVCOD_ECONOMICPropertiesCloseQuery(
      Sender: TObject; var CanClose: Boolean);
    procedure GridPreluareVCOD_ECONOMICPropertiesPopup(Sender: TObject);
    procedure cxTreeUnitatiDblClick(Sender: TObject);
    procedure cxTreeUnitatiKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure cxTreeFunctionalDblClick(Sender: TObject);
    procedure cxTreeFunctionalKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure cxTreeEconomicDblClick(Sender: TObject);
    procedure cxTreeEconomicKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
  private
    { Private declarations }
  public
    { Public declarations }
    procedure SetCodFunctional(Value : String);
    procedure SetContext(CodFunctional: String; CodProiect: Integer; CodUnitate: Integer);
  end;


implementation

{$R *.DFM}

uses
  ZeosDBUtile, ZDataSet, DateUnit, DateUtils, CommonDBVar, Variants;

procedure TfrmPreluareExcel.BtnCancelClick(Sender: TObject);
begin
  ModalResult := mrCancel;
end;

procedure TfrmPreluareExcel.BtnPreiaClick(Sender: TObject);
var
  lFakeQry: TZReadOnlyQuery;
  lFactor : Integer;

     procedure SetDataType(ParamName: String; aDataType : TFieldType);
     var I: Integer;
      begin
        for I := 0 to lFakeQry.Params.Count-1 do
          if AnsiCompareText(lFakeQry.Params[I].Name, ParamName) = 0 then begin
             lFakeQry.Params[I].DataType := aDataType;
          end;
      end;


     procedure SetParam(ParamName: String; ParamValue : Variant);
     var I: Integer;
      begin
        for I := 0 to lFakeQry.Params.Count-1 do
          if AnsiCompareText(lFakeQry.Params[I].Name, ParamName) = 0 then begin
             lFakeQry.Params[I].Value := ParamValue;
          end;
      end;

begin
  if not((edCategorie.EditValue = null) or (edCategorie.EditValue = 0)) then
    if RPProiect.CodValue = Null then begin
      MessageDlg('Completati campul pentru ' + StringReplace(lbDefalcare.Caption, ' :', '', [rfReplaceAll]),mtError, [mbOk], 0);
      edCategorie.SetFocus;
      Exit;
    end;
  if Trim(RPFunctional1.EditInput.Text)='' then begin
    MessageDlg('Completati clasificatia functionala !',mtError, [mbOk], 0);
    RPFunctional1.EditInput.SetFocus;
    Exit;
  end;

  { Preluam Datele }
  if Preluate.IsEmpty then Exit;
  PreluareProgress.Properties.Min := 0;
  PreluareProgress.Properties.Max := Preluate.RecordCount;
  PreluareProgress.Position := 0;
  PreluareProgress.Visible := True;
  lFakeQry := GetTmpADOQuery;

  try
    lFakeQry.Sql.Add('exec spBugetDelPreiaExcel :COD_FUNCTIONAL, :AN_FISCAL, :REVIZIE, :ID_OI_PROIECTE, :ID_OI_UNITATI');
//    lFakeQry.Sql.Add('DELETE FROM BUGET_PLANIFICARE WHERE COD_FUNCTIONAL = :COD_FUNCTIONAL AND AN_FISCAL = :AN_FISCAL AND REVIZIE = :REVIZIE');
    SetDataType('COD_FUNCTIONAL', ftString);
    SetDataType('AN_FISCAL', ftInteger);
    SetDataType('REVIZIE', ftInteger);
    SetParam('REVIZIE', StrToInt(edRevizie.EditValue));
    SetParam('AN_FISCAL', StrToInt(edAnFiscal.EditValue));
    SetParam('COD_FUNCTIONAL', RPFunctional1.TextEdit.Text);
    SetParam('ID_OI_PROIECTE', Null);
    SetParam('ID_OI_UNITATI', Null);
    if edCategorie.EditValue = 1 then begin
      SetParam('ID_OI_PROIECTE', RPProiect.CodValue);
    end
    else if edCategorie.EditValue = 2 then begin
      SetParam('ID_OI_UNITATI', RPProiect.CodValue);
    end;
//    SetParam('COD_FUNCTIONAL', Preluate.FieldByName('COD_FUNCTIONAL').AsString);
    lFakeQry.ExecSQL;
    lFakeQry.SQL.Clear;

    lFakeQry.Sql.Add('exec spBugetPreiaExcel :COD_FUNCTIONAL, :COD_ECONOMIC, :AN_FISCAL, :REVIZIE, :PLANIFICAT_REST, :PLANIFICAT1, :PLANIFICAT2, :PLANIFICAT3, :PLANIFICAT4, :ID_OI_PROIECTE, :ID_OI_UNITATI, :PLUS1AN, :PLUS2AN, :PLUS3AN');

    SetDataType('COD_FUNCTIONAL', ftString);
    SetDataType('COD_ECONOMIC', ftString);
    SetDataType('AN_FISCAL', ftInteger);
    SetDataType('REVIZIE', ftInteger);
    SetDataType('PLANIFICAT_REST', ftCurrency);
    SetDataType('PLANIFICAT1', ftCurrency);
    SetDataType('PLANIFICAT2', ftCurrency);
    SetDataType('PLANIFICAT3', ftCurrency);
    SetDataType('PLANIFICAT4', ftCurrency);
    SetDataType('PLUS1AN', ftCurrency);
    SetDataType('PLUS2AN', ftCurrency);
    SetDataType('PLUS3AN', ftCurrency);

    SetParam('REVIZIE', StrToInt(edRevizie.EditValue));
    SetParam('AN_FISCAL', StrToInt(edAnFiscal.EditValue));
    SetParam('COD_FUNCTIONAL', RPFunctional1.TextEdit.Text);
    SetParam('ID_OI_PROIECTE', null);
    SetParam('ID_OI_UNITATI', null);
    if edCategorie.EditValue = 1 then begin
      SetParam('ID_OI_PROIECTE', RPProiect.CodValue);
    end
    else if edCategorie.EditValue = 2 then begin
      SetParam('ID_OI_UNITATI', RPProiect.CodValue);
    end;
    lFactor := StrToInt(edMultiplicator.EditValue);
    with Preluate do begin
      First;
      while not Eof do begin
        if not(Preluate.FieldByName('EROARE').AsInteger in [0, 6, 8]) then begin
          PreluareProgress.Position := PreluareProgress.Position + 1;
          Application.ProcessMessages;          
          Next;
          Continue;
        end;
        SetParam('COD_ECONOMIC', FieldByName('COD_ECONOMIC').AsString);
        if Preluate.FieldByName('EROARE').AsInteger = 8 then begin
          SetParam('PLANIFICAT_REST', Null);
          SetParam('PLANIFICAT1', Null);
          SetParam('PLANIFICAT2', Null);
          SetParam('PLANIFICAT3', Null);
          SetParam('PLANIFICAT4', Null);
        end
        else begin
          SetParam('PLANIFICAT_REST', FieldByName('PLANIFICAT_REST').AsCurrency * lFactor);
          SetParam('PLANIFICAT1', FieldByName('PLANIFICAT1').AsCurrency * lFactor);
          SetParam('PLANIFICAT2', FieldByName('PLANIFICAT2').AsCurrency * lFactor);
          SetParam('PLANIFICAT3', FieldByName('PLANIFICAT3').AsCurrency * lFactor);
          SetParam('PLANIFICAT4', FieldByName('PLANIFICAT4').AsCurrency * lFactor);
        end;
        SetParam('PLUS1AN', FieldByName('PLUS1AN').AsCurrency * lFactor);
        SetParam('PLUS2AN', FieldByName('PLUS2AN').AsCurrency * lFactor);
        SetParam('PLUS3AN', FieldByName('PLUS3AN').AsCurrency * lFactor);
        lFakeQry.ExecSql;
        PreluareProgress.Position := PreluareProgress.Position + 1;
        Application.ProcessMessages;
        Next;
      end;
    end;
  finally
    PreluareProgress.Visible := False;
    lFakeQry.Free;
  end;
end;

procedure TfrmPreluareExcel.FormCreate(Sender: TObject);
var
  anCurent : Integer;
  MarkYear : Boolean;
  aMaxRevizie : Integer;
  lastV : Integer;
begin
  DBRefresh([frmData.qryBGPlanFunctional, frmData.qryOIUnitati, frmData.qryOIProiecte]);
  anCurent := YearOf(Date);
  MarkYear := False;
  with GetTmpADOQuery do
    try
       SQL.Add('exec spBugetAnFiscal');
       Open;
       edAnFiscal.Properties.Items.Clear;
       while not Eof do begin
         if Fields[0].AsInteger = anCurent  then MarkYear := True;
         with edAnFiscal.Properties.Items.Add do begin
           Description := 'Anul : '+Fields[0].AsString;
           Value := Fields[0].AsString;
           lastV := Index;
         end;
         Next;
       end;
       if not MarkYear then begin
         with edAnFiscal.Properties.Items.Add do begin
           Description := 'Anul : '+IntToStr(anCurent);
           Value := IntToStr(anCurent);
         end;
       end;
       if edAnFiscal.Properties.Items.Count > 0 then
          if not MarkYear then edAnFiscal.ItemIndex := lastV else edAnFiscal.ItemIndex := edAnFiscal.Properties.Items.Count-1;
       Close;
       Sql.Clear;
       Sql.Add('exec spBugetRevizii');
       Open;
       edRevizie.Properties.Items.Clear;
       aMaxRevizie := 0;
       while not Eof do begin
         with edRevizie.Properties.Items.Add do begin
           Description := 'Revizia : '+Fields[0].AsString;
           Value := Fields[0].AsString;
         end;
         if aMaxRevizie < Fields[0].AsInteger then aMaxRevizie := Fields[0].AsInteger;
         Next;
       end;
       with edRevizie.Properties.Items.Add do begin
          Description := 'Revizia Noua : ' + IntToStr(aMaxRevizie+1);
          Value := IntToStr(aMaxRevizie+1);
        end;
       if edRevizie.Properties.Items.Count > 1 then edRevizie.ItemIndex := edRevizie.Properties.Items.Count-2
       else edRevizie.ItemIndex := edRevizie.Properties.Items.Count-1;
    finally
       Free;
    end;
  RPFunctional1.OnlySelectChild := True;
  RPFunctional1.ValidateEditText := True;
  RPProiect.OnlySelectChild := False;
  RPProiect.ValidateEditText := True;    
end;

procedure TfrmPreluareExcel.RPFunctional1PopupInitPopup(Sender: TObject);
begin
  with TcxPopupEdit(Sender).Properties do begin
    if PopupWidth < TcxPopupEdit(Sender).Width then PopupWidth := TcxPopupEdit(Sender).Width;
  end;
end;

procedure TfrmPreluareExcel.cxTreeFunctionalDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Values[cxTreeFunctionalCOD_FUNCTIONAL.ItemIndex] + ': '+ANode.Values[cxTreeFunctionalDENUMIRE.ItemIndex];
end;

procedure TfrmPreluareExcel.cxTreeProiecteDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Texts[cxTreeProiecteDENUMIRE.ItemIndex] ;
  if Trim(ANode.Texts[cxTreeProiecteCOD_FUNCTIONAL.ItemIndex]) <> '' then
    Value := Value + '('+Trim(ANode.Texts[cxTreeProiecteCOD_FUNCTIONAL.ItemIndex])+')';
end;

procedure TfrmPreluareExcel.cxTreeUnitatiDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Texts[cxTreeUnitatiDENUMIRE.ItemIndex];
end;

procedure TfrmPreluareExcel.edCategoriePropertiesChange(Sender: TObject);
begin
  lbDefalcare.Visible := not(edCategorie.ItemIndex = 0);
  RPProiect.Visible := lbDefalcare.Visible;
  case edCategorie.EditValue of
    0 : begin
    end;
    1 : begin
      lbDefalcare.Caption := 'Proiect :';
      RPProiect.CodField := 'ID_OI_PROIECTE';
      RPProiect.KeyField := 'ID_OI_PROIECTE';
      RPProiect.ListField := 'DENUMIRE';
      RPProiect.PopupEdit.PopupControl := cxTreeProiecte;
    end;
    2 : begin
      lbDefalcare.Caption := 'Unitate :';
      RPProiect.CodField := 'ID_OI_UNITATI';
      RPProiect.KeyField := 'ID_OI_UNITATI';
      RPProiect.ListField := 'DENUMIRE';
      RPProiect.PopupEdit.PopupControl := cxTreeUnitati;
    end;
  end;
end;

procedure TfrmPreluareExcel.SetCodFunctional(Value: String);
begin
  RPFunctional1.EditInput.Text := Value;
  RPFunctional1.ForceValidateEditText;
end;

procedure TfrmPreluareExcel.GridPreluareVCOD_FUNCTIONALPropertiesPopup(
  Sender: TObject);
begin
  with TcxPopupEdit(Sender) do
   InternalPositioning(StringReplace(TcxPopupEdit(Sender).Text,'?', '',[]), cxTreeEconomic, 'COD_FUNCTIONAL');
end;

type
  TAccesscxPopupEdit = class(TcxPopupEdit);

procedure TfrmPreluareExcel.GridPreluareVCOD_FUNCTIONALPropertiesCloseQuery(
  Sender: TObject; var CanClose: Boolean);
var
  lNode : TcxDBTreeListNode;
begin
  with TAccesscxPopupEdit(Sender) do
    if PopupWindow.ModalResult = mrOk then begin
         lNode := TcxDBTreeListNode(cxTreeFunctional.FocusedNode);
         if Assigned(lNode) then
           DBSetFieldValue(Preluate, 'COD_FUNCTIONAL', lNode.Values[cxTreeFunctionalCOD_FUNCTIONAL.ItemIndex]);
    end;
end;

procedure TfrmPreluareExcel.cxTreeEconomicDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Values[cxTreeEconomicCOD_ECONOMIC.ItemIndex] + ': '+ ANode.Values[cxTreeEconomicDENUMIRE.ItemIndex];
end;

procedure TfrmPreluareExcel.GridPreluareVCOD_ECONOMICPropertiesCloseQuery(
  Sender: TObject; var CanClose: Boolean);
var
  lNode : TcxDBTreeListNode;
begin
  with TAccesscxPopupEdit(Sender) do
    if PopupWindow.ModalResult = mrOk then begin
         lNode := TcxDBTreeListNode(cxTreeEconomic.FocusedNode);
         if Assigned(lNode) then begin
           DBSetFieldValue(Preluate, 'COD_ECONOMIC', lNode.Values[cxTreeEconomicCOD_ECONOMIC.ItemIndex]);
           if lNode.HasChildren then begin
             if (Preluate.FieldByName('PLUS1AN').AsCurrency <> 0) or (Preluate.FieldByName('PLUS2AN').AsCurrency <> 0) or (Preluate.FieldByName('PLUS3AN').AsCurrency <> 0) then
               DBSetFieldValue(Preluate, 'EROARE', 8)
             else
               DBSetFieldValue(Preluate, 'EROARE', 7);
           end
           else
             DBSetFieldValue(Preluate, 'EROARE', 6);
         end;
    end;
end;

procedure TfrmPreluareExcel.GridPreluareVCOD_ECONOMICPropertiesPopup(
  Sender: TObject);
begin
  with TcxPopupEdit(Sender) do
   InternalPositioning(StringReplace(TcxPopupEdit(Sender).Text,'?', '',[]), cxTreeEconomic, 'COD_ECONOMIC');
end;

procedure TfrmPreluareExcel.SetContext(CodFunctional: String; CodProiect,
  CodUnitate: Integer);
begin
  edCategorie.ItemIndex := 0;
  if CodProiect <> -1 then begin
     edCategorie.ItemIndex := 1;
     RPProiect.EditInput.Text := IntToStr(CodProiect);
  end;
  if CodUnitate <> -1 then begin
     edCategorie.ItemIndex := 2;
     RPProiect.EditInput.Text := IntToStr(CodUnitate);
  end;
  RPProiect.ForceValidateEditText;
  RPFunctional1.EditInput.Text := CodFunctional;
  RPFunctional1.ForceValidateEditText;
end;

procedure TfrmPreluareExcel.cxTreeUnitatiDblClick(Sender: TObject);
begin
   with TcxDBTreeList(Sender) do
    if (FocusedNode <> nil) then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk;
end;

procedure TfrmPreluareExcel.cxTreeUnitatiKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
     cxTreeUnitatiDblClick(TcxDBTreeList(Sender))
  else if Key = VK_ESCAPE then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrCancel;
end;

procedure TfrmPreluareExcel.cxTreeFunctionalDblClick(Sender: TObject);
begin
   with TcxDBTreeList(Sender) do
    if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk;
end;

procedure TfrmPreluareExcel.cxTreeFunctionalKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
     cxTreeFunctionalDblClick(TcxDBTreeList(Sender))
  else if Key = VK_ESCAPE then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrCancel;
end;

procedure TfrmPreluareExcel.cxTreeEconomicDblClick(Sender: TObject);
begin
   with TcxDBTreeList(Sender) do
    if (FocusedNode <> nil) then
      (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk;
end;

procedure TfrmPreluareExcel.cxTreeEconomicKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
     cxTreeEconomicDblClick(TcxDBTreeList(Sender))
  else if Key = VK_ESCAPE then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrCancel;
end;

end.
