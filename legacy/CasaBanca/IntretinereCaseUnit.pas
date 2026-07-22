unit IntretinereCaseUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs, DateUnit,
  Db, ZDataSet, ExtCtrls, HeadPanel, dxCntner, dxInspct, dxDBInsp, dxTL,
  dxDBCtrl, dxDBGrid, dxInspRw, dxDBInRw, CommonDBVar, dxExEdtr,
  Menus, ActnList, Buttons, dxDBTL, dxDBTLCl, dxGrClms,
  ZAbstractRODataset, ZAbstractDataset, cxGraphics, cxLookAndFeels,
  cxLookAndFeelPainters, StdCtrls, cxButtons, cxControls, cxStyles, cxEdit,
  cxMaskEdit, cxCheckBox, cxButtonEdit, cxDropDownEdit, cxImageComboBox,
  cxVGrid, cxDBVGrid, cxInplaceContainer, cxCustomData, cxFilter, cxData,
  cxDataStorage, cxNavigator, cxDBData, cxGridLevel, cxClasses,
  cxGridCustomView, cxGridCustomTableView, cxGridTableView, cxGridDBTableView,
  cxGrid, cxCalendar, cxSplitter, cxTextEdit, cxCurrencyEdit,
  cxDataControllerConditionalFormattingRulesManagerDialog, cxTL,
  cxTLdxBarBuiltInMenu, cxTLData, cxDBTL, cxDBTreeListExUnit, dxDateRanges,
  dxScrollbarAnnotations;

type
  TfrmIntretinCasa = class(TForm)
    DTCasa: TDataSource;
    QryCase: TZQuery;
    pnTop: THeadPanel;
    pnClient: TPanel;
    pnRight: TPanel;
    DTSoldInitial: TDataSource;
    QrySoldInitial: TZQuery;
    splitterV: TcxSplitter;
    pnBottom: TPanel;
    SoldActions: TActionList;
    Cmd_CalculateSold: TAction;
    ppSoldMenu: TPopupMenu;
    CalculeazaSold1: TMenuItem;
    Cmd_Update: TAction;
    btnOk: TSpeedButton;
    TreeRepartitori: TdxDBTreeList;
    TreeRepartitoriNUME: TdxDBTreeListMaskColumn;
    TreeRepartitoriCONT: TdxDBTreeListMaskColumn;
    TreeRepartitoriCODSECTIE: TdxDBTreeListMaskColumn;
    TreeRepartitoriADRESA: TdxDBTreeListMaskColumn;
    TreeRepartitoriGESTINT: TdxDBTreeListCheckColumn;
    TreeRepartitoriTIPGEST: TdxDBTreeListMaskColumn;
    Cmd_SoldPlanConturi: TAction;
    SoldContabilitate1: TMenuItem;
    pnTools: TPanel;
    BtnAddDir: TcxButton;
    BtnDelDepartament: TcxButton;
    casaVerticalGrid: TcxDBVerticalGrid;
    casaVerticalGridDENUMIRE: TcxDBEditorRow;
    casaVerticalGridCategoryRow1: TcxCategoryRow;
    casaVerticalGridCategoryRow2: TcxCategoryRow;
    casaVerticalGridIS_BANCA: TcxDBEditorRow;
    casaVerticalGridCASIER: TcxDBEditorRow;
    casaVerticalGridDEFALCATOR: TcxDBEditorRow;
    casaVerticalGridADMIN: TcxDBEditorRow;
    casaVerticalGridIS_AVANS: TcxDBEditorRow;
    casaVerticalGridID_REPARTITORI: TcxDBEditorRow;
    casaVerticalGridIS_AVANS1: TcxDBEditorRow;
    casaVerticalGridIS_TEMPOR: TcxDBEditorRow;
    casaVerticalGridID_VALUTA: TcxDBEditorRow;
    casaVerticalGridCRSP_LEI: TcxDBEditorRow;
    viewCasa: TcxGridDBTableView;
    nivelCasa: TcxGridLevel;
    gridCasa: TcxGrid;
    viewCasaCOD_CB: TcxGridDBColumn;
    viewCasaDENUMIRE: TcxGridDBColumn;
    viewCasaCRSP_LEI: TcxGridDBColumn;
    viewCasaID_VALUTA: TcxGridDBColumn;
    viewCasaC_O: TcxGridDBColumn;
    viewCasaSOLDINI_D: TcxGridDBColumn;
    viewCasaSOLDINI_C: TcxGridDBColumn;
    viewCasaDATA_SOLD: TcxGridDBColumn;
    viewCasaCASIER: TcxGridDBColumn;
    viewCasaDEFALCATOR: TcxGridDBColumn;
    viewCasaADMIN: TcxGridDBColumn;
    viewCasaIS_BANCA: TcxGridDBColumn;
    viewCasaIS_AVANS: TcxGridDBColumn;
    viewCasaID_REPARTITORI: TcxGridDBColumn;
    viewSold: TcxGridDBTableView;
    nivelSold: TcxGridLevel;
    gridSold: TcxGrid;
    viewSoldCOD_CB: TcxGridDBColumn;
    viewSoldCOD: TcxGridDBColumn;
    viewSoldCODGEST: TcxGridDBColumn;
    viewSoldDATA: TcxGridDBColumn;
    viewSoldTIPDOC: TcxGridDBColumn;
    viewSoldNRDOC: TcxGridDBColumn;
    viewSoldPOZ: TcxGridDBColumn;
    viewSoldEXPLICATIE: TcxGridDBColumn;
    viewSoldINCASARI: TcxGridDBColumn;
    viewSoldPLATI: TcxGridDBColumn;
    viewSoldSOLD: TcxGridDBColumn;
    viewSoldCONT_CSP: TcxGridDBColumn;
    viewSoldVAL_CRSP: TcxGridDBColumn;
    viewSoldACHITAT: TcxGridDBColumn;
    viewSoldDATAEM: TcxGridDBColumn;
    viewSoldC_O: TcxGridDBColumn;
    viewSoldNR_LIST: TcxGridDBColumn;
    viewSoldMEXPLIC: TcxGridDBColumn;
    viewSoldCURS_SCHIM: TcxGridDBColumn;
    viewSoldSOLD_INITIAL: TcxGridDBColumn;
    viewSoldCOD_ARHIVA: TcxGridDBColumn;
    viewSoldECL: TcxGridDBColumn;
    soldInspector: TcxDBVerticalGrid;
    soldInspectorDATA: TcxDBEditorRow;
    soldInspectorEXPLICATIE: TcxDBEditorRow;
    soldInspectorINCASARI: TcxDBEditorRow;
    soldInspectorPLATI: TcxDBEditorRow;
    soldInspectorCategoryRow1: TcxCategoryRow;
    procedure FormCreate(Sender: TObject);
    procedure QryCaseAfterInsert(DataSet: TDataSet);
    procedure QryCaseAfterEdit(DataSet: TDataSet);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure Cmd_CalculateSoldExecute(Sender: TObject);
    procedure Cmd_UpdateExecute(Sender: TObject);
    procedure btnOkClick(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure CasaInspectorID_REPARTIRORIPopup(Sender: TObject;
      const EditText: String);
    procedure Cmd_SoldPlanConturiExecute(Sender: TObject);
    procedure BtnAddDirClick(Sender: TObject);
    procedure BtnDelDepartamentClick(Sender: TObject);
    procedure casaVerticalGridCASIEREditPropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
    procedure QrySoldInitialNewRecord(DataSet: TDataSet);
    procedure casaVerticalGridCASIERPropertiesGetDisplayText(
      Sender: TcxCustomEditorRowProperties; ARecord: Integer;
      var AText: string);
    procedure casaVerticalGridCRSP_LEIEditPropertiesButtonClick(Sender: TObject;
      AButtonIndex: Integer);
  private
    { Private declarations }
    procedure SetRepartitorFilter(AFilter: String);
  public
    { Public declarations }
    UserValues, UserDescriptions : TStringList;
    procedure PopulateControls;
    function GetUtilizatori(AText : String) : String;
  end;

implementation

uses
  ZeosDBUtile, dxCompsUtile,
  AsocUtilizUnit, Variants, CommonCasa, PlanConturiUnit;

{$R *.DFM}

procedure TfrmIntretinCasa.FormCreate(Sender: TObject);
begin
  SetRepartitorFilter('GESTINT = True');
  UserValues := TStringList.Create;
  UserDescriptions := TStringList.Create;
  PopulateControls;
  DBRefresh([QryCase, QrySoldInitial]);
end;

procedure TfrmIntretinCasa.PopulateControls;
var
  lDataSet: TDataSet;
begin
  lDataSet := DBNewQuery('EXEC SP_GET_CASA_LISTA_TIP_VALUTE');
  try
    lDataSet.Open;
    if not lDataSet.IsEmpty then begin
      FillImageCombo(casaVerticalGridID_VALUTA.Properties.EditProperties, lDataSet, 'ID_UNIC', 'denumire');
      viewCasaID_VALUTA.Properties.Assign(casaVerticalGridID_VALUTA.Properties.EditProperties);
      casaVerticalGridID_VALUTA.Visible := True;
      viewCasaID_VALUTA.Visible         := True;
    end
    else begin
      casaVerticalGridID_VALUTA.Visible := False;
      viewCasaID_VALUTA.Visible         := False;
    end;
  finally
    lDataSet.Free;
  end;

  PopulateImage(FrmData.QryOperatori, UserValues, UserDescriptions, 'ID_UTILIZATORI', 'NUMEINTREG');

end;

procedure TfrmIntretinCasa.QryCaseAfterInsert(DataSet: TDataSet);
begin
  DataSet['COD_CB'] := GetNextId('CASIERIE');
  DataSet['C_O']    := iUserID;
  DataSet['CRSP_LEI'] := '';
  if not viewCasa.DataController.DataSet.IsEmpty then
  begin
    viewCasa.DataController.GotoLast;
  end;
end;

procedure TfrmIntretinCasa.QryCaseAfterEdit(DataSet: TDataSet);
begin
  DataSet['C_O'] := iUserID;
end;

procedure TfrmIntretinCasa.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  SetRepartitorFilter('');
  Action := caFree;
end;

procedure TfrmIntretinCasa.Cmd_CalculateSoldExecute(Sender: TObject);
var
  lSold   : Currency;
  lRecord : TcxCustomGridRecord;
begin
  lRecord := viewSold.Controller.FocusedRecord;
  if Assigned(lRecord) and lRecord.IsData then begin
    lSold := ValueSafeToCurrency( DBGetScallarFmt('exec [sp_get_sold_initial] %s, %s',
              [ValueToStr(lRecord.Values[viewSoldDATA.Index]),
               ValueToStr(lRecord.Values[viewSoldCOD_CB.Index])], 0) );
    if lSold > 0 then begin
      DBSetFieldValue(QrySoldInitial, 'INCASARI', lSold);
      DBSetFieldValue(QrySoldInitial, 'PLATI', 0);
    end
    else begin
      DBSetFieldValue(QrySoldInitial, 'INCASARI', 0);
      DBSetFieldValue(QrySoldInitial, 'PLATI', -1 * lSold);
    end;
  end;
end;

procedure TfrmIntretinCasa.Cmd_UpdateExecute(Sender: TObject);
begin
  DBCommitUpdates(QrySoldInitial);
  DBRefresh(QrySoldInitial);
end;

procedure TfrmIntretinCasa.btnOkClick(Sender: TObject);
begin
  DBCommitUpdates([QryCase, QrySoldInitial]);
  CaseModified := True;
  Close;
end;

procedure TfrmIntretinCasa.FormDestroy(Sender: TObject);
begin
  UserDescriptions.Free;
  UserValues.Free;
end;

function TfrmIntretinCasa.GetUtilizatori(AText: String): String;
var
  lList   : TStringList;
  I,
  lIndex  : Integer;
begin
  Result := Trim(AText);
  if Result > '' then begin
    lList := TStringList.Create;
    try
      lList.CommaText := Result;
      Result := '';
      for I := 0 to lList.Count-1 do begin
        lIndex := UserValues.IndexOf(lList[I]);
        if lIndex <> -1 then begin
          if Result > '' then Result := Result + ', ';
          Result := Result + UserDescriptions[lIndex];
        end;
      end;
    finally
      lList.Free;
    end;
  end;
end;

procedure TfrmIntretinCasa.CasaInspectorID_REPARTIRORIPopup(
  Sender: TObject; const EditText: String);
begin
  InternalPositioning(StringReplace(EditText,'?', '',[]), TreeRepartitori);
end;

procedure TfrmIntretinCasa.SetRepartitorFilter(AFilter: String);
var NewFiltered: Boolean;
  function SetSQLBoolean(SFilter: String): String;
   begin
     Result := StringReplace(UpperCase(SFilter), 'TRUE', '1', [rfReplaceAll, rfIgnoreCase]);
     Result := StringReplace(Result, 'FALSE', '0', [rfReplaceAll, rfIgnoreCase]);
   end;
begin
  with FrmData.QryRepartitori do begin
       NewFiltered := Filter <> AFilter;
       if NewFiltered then Filter := AFilter;
       Filtered := Filter <> '';
    end;
end;

procedure TfrmIntretinCasa.QrySoldInitialNewRecord(DataSet: TDataSet);
begin
  DataSet['COD']          := GetNextId('BREGISTRU');
  DataSet['COD_CB']       := QryCase['COD_CB'];
  DataSet['DATAEM']       := Date;
  DataSet['C_O']          := iUserID;
  DataSet['SOLD_INITIAL'] := 0;
end;

procedure TfrmIntretinCasa.casaVerticalGridCASIEREditPropertiesButtonClick(
  Sender: TObject; AButtonIndex: Integer);
var
  lStr    : String;
  lRow    : TcxDBEditorRow;
begin
  if casaVerticalGrid.FocusedRow is TcxDBEditorRow then begin
    lRow := TcxDBEditorRow(casaVerticalGrid.FocusedRow);
    lStr := ModificaUtilizatori(QryCase['COD_CB'], TdxInspectorDBButtonRow(Sender).Tag);
    DBSetFieldValue(QryCase, lRow.Properties.DataBinding.FieldName, lStr);
  end;
end;

procedure TfrmIntretinCasa.casaVerticalGridCASIERPropertiesGetDisplayText(
  Sender: TcxCustomEditorRowProperties; ARecord: Integer; var AText: string);
begin
  AText := GetUtilizatori(AText);
end;

procedure TfrmIntretinCasa.casaVerticalGridCRSP_LEIEditPropertiesButtonClick(
  Sender: TObject; AButtonIndex: Integer);
var
  frmPlabConturi : TFrmPlanConturi;
  lNode : TcxDBTreeListNode;
  lCont : String;
begin
  lCont := QryCase['CRSP_LEI'];
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
      if Trim(lCont) <> '' then begin
         lNode := TreePlan.FindNodeByKeyValue(lCont);
         if Assigned(lNode) then begin
           lNode.MakeVisible;
           lNode.Focused := True;
         end;
      end;
      ShowModal;
      if ModalResult = mrOk then begin
        lNode := TcxDBTreeListNode(TreePlan.FocusedNode);
        QryCase.Edit;
        QryCase['CRSP_LEI'] := lNode.KeyValue;
        if Trim(QryCase.FieldByName('DENUMIRE').AsString) = '' then
          QryCase.FieldByName('DENUMIRE').AsString := lNode.Texts[TreePlanROMANA.ItemIndex];
        QryCase.Post;
      end;
    finally
      frmPlabConturi.Free;
    end;
end;

procedure TfrmIntretinCasa.Cmd_SoldPlanConturiExecute(Sender: TObject);
var
  lSold   : Currency;
  lRecord : TcxCustomGridRecord;
begin
  lRecord := viewSold.Controller.FocusedRecord;
  if Assigned(lRecord) and lRecord.IsData then begin
    lSold := ValueSafeToCurrency( DBGetScallarFmt('exec [sp_cb_get_sold_contabilitate] %s, %s',
              [ValueToStr(lRecord.Values[viewSoldDATA.Index]),
               ValueToStr(lRecord.Values[viewSoldCOD_CB.Index])]) );
    if lSold > 0 then begin
      DBSetFieldValue(QrySoldInitial, 'INCASARI', lSold);
      DBSetFieldValue(QrySoldInitial, 'PLATI', 0);
    end
    else begin
      DBSetFieldValue(QrySoldInitial, 'INCASARI', 0);
      DBSetFieldValue(QrySoldInitial, 'PLATI', -1 * lSold);
    end;
  end;
end;

procedure TfrmIntretinCasa.BtnAddDirClick(Sender: TObject);
begin
  QryCase.Append;
end;

procedure TfrmIntretinCasa.BtnDelDepartamentClick(Sender: TObject);
begin
  QryCase.Delete;
end;

end.
