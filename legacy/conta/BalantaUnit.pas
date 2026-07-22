unit BalantaUnit;

interface

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  StdCtrls, Db, ZDataSet, ExtCtrls, ComCtrls, ToolWin,
  NoteEronateUnit, Menus, ActnList, Buttons, cxControls, cxContainer, cxEdit,
  cxTextEdit, cxMaskEdit, cxDropDownEdit, cxDBLookupComboBox,
  cxLookAndFeelPainters, cxButtons, ZAbstractRODataset, ZAbstractDataset,
  cxGraphics, cxCustomData, cxTL, cxTLdxBarBuiltInMenu, cxInplaceContainer,
  cxTLData, cxDBTL, cxCurrencyEdit, cxSplitter, cxSpinEdit, cxCalendar,
  cxImageComboBox, cxCheckBox, cxLookAndFeels, cxStyles, dxPSGlbl, dxPSUtl,
  dxPSEngn, dxPrnPg, dxBkgnd, dxWrap, dxPrnDev, dxPSCompsProvider, dxPSFillPatterns,
  dxPSEdgePatterns, dxPSCore, dxPScxCommon, cxCheckComboBox, dxCore, cxDateUtils,
  dxPSPDFExportCore, dxPSPDFExport, cxDrawTextUtils, dxPSPrVwStd, dxPSPrVwAdv,
  dxPSPrVwRibbon, dxPScxPageControlProducer, dxPScxGridLnk,
  dxPScxGridLayoutViewLnk, dxPScxEditorProducers, dxPScxExtEditorProducers,
  dxPScxTLLnk, cxClasses,
  cxDataControllerConditionalFormattingRulesManagerDialog,
  dxScrollbarAnnotations;

type
  RecParams = record
    ParamName : String[100];
    ParamRealPos : Integer;
    QryParamPos  : Integer;
    QryInternalName : String[100];
    QryInternalValue : String[100];
  end;

  ParamVector = Array of RecParams;

  TFrmBalanta = class(TForm)
    QryBalanta: TZQuery;
    DTBalanta: TDataSource;
    pnAll: TPanel;
    PnClient: TPanel;
    pnTools: TPanel;
    ExpandLevels: TToolBar;
    pnLeft: TScrollBox;
    Splitter1: TSplitter;
    Label1: TLabel;
    Label2: TLabel;
    Label3: TLabel;
    ppMeniu: TPopupMenu;
    CmdNote: TActionList;
    Cmd_FisaCont: TAction;
    FisaCont1: TMenuItem;
    ppListaTipBalanta: TMenuItem;
    N1: TMenuItem;
    ppSaveTip: TMenuItem;
    ppStergeTipBalanta: TMenuItem;
    Cmd_SalveazaTipBalanta: TAction;
    Cmd_StergeTipBalanta: TAction;
    Cmd_ListaTipBalanta: TAction;
    btnGo: TSpeedButton;
    btnGenerareNote: TSpeedButton;
    btnGenNoteInchidere: TSpeedButton;
    cxTreeBalanta: TcxDBTreeList;
    cxTreeBalantaCONT: TcxDBTreeListColumn;
    cxTreeBalantaCONT_PLAN: TcxDBTreeListColumn;
    cxTreeBalantaCOD_FUNCTIONAL: TcxDBTreeListColumn;
    cxTreeBalantaCOD_ECONOMIC: TcxDBTreeListColumn;
    cxTreeBalantaROMANA: TcxDBTreeListColumn;
    cxTreeBalantaCODREP: TcxDBTreeListColumn;
    cxTreeBalantaFCTCONT: TcxDBTreeListColumn;
    cxTreeBalantaSID: TcxDBTreeListColumn;
    cxTreeBalantaSIC: TcxDBTreeListColumn;
    cxTreeBalantaS_P_D: TcxDBTreeListColumn;
    cxTreeBalantaS_P_C: TcxDBTreeListColumn;
    cxTreeBalantaR_P_D: TcxDBTreeListColumn;
    cxTreeBalantaR_P_C: TcxDBTreeListColumn;
    cxTreeBalantaT_P_D: TcxDBTreeListColumn;
    cxTreeBalantaT_P_C: TcxDBTreeListColumn;
    cxTreeBalantaR_L_D: TcxDBTreeListColumn;
    cxTreeBalantaR_L_C: TcxDBTreeListColumn;
    cxTreeBalantaR_T_D: TcxDBTreeListColumn;
    cxTreeBalantaR_T_C: TcxDBTreeListColumn;
    cxTreeBalantaT_L_D: TcxDBTreeListColumn;
    cxTreeBalantaT_L_C: TcxDBTreeListColumn;
    cxTreeBalantaS_T_D: TcxDBTreeListColumn;
    cxTreeBalantaS_T_C: TcxDBTreeListColumn;
    cxTreeBalantaS_D: TcxDBTreeListColumn;
    cxTreeBalantaS_C: TcxDBTreeListColumn;
    cxTreeBalantaNIVEL: TcxDBTreeListColumn;
    cxTreeBalantaEXPLICATIE: TcxDBTreeListColumn;
    edAnRaportare: TcxSpinEdit;
    edDataStart: TcxDateEdit;
    edDataEnd: TcxDateEdit;
    edListaLuni: TcxImageComboBox;
    edCont: TcxImageComboBox;
    edUnitate: TcxCheckComboBox;
    edTipBalanta: TcxImageComboBox;
    chkPreluareNote: TcxCheckBox;
    ChkAplicaCulori: TcxCheckBox;
    ChkDoarMiscari: TcxCheckBox;
    ChkPeRepartitori: TcxCheckBox;
    ChkPeMateriale: TcxCheckBox;
    chkConsolidata: TcxCheckBox;
    ChkArataFunctionalitate: TcxCheckBox;
    chkSintetic: TcxCheckBox;
    chkExecutie: TcxCheckBox;
    BtnOk: TcxButton;
    btnRaportare: TcxButton;
    N2: TMenuItem;
    Cmd_ExporXLS: TAction;
    edGrup: TcxImageComboBox;
    Label4: TLabel;
    edProiect: TcxImageComboBox;
    Label5: TLabel;
    tiparireBalanta: TdxComponentPrinter;
    linkBalanta: TcxDBTreeListReportLink;
    ppTiparireBalanta: TMenuItem;
    N3: TMenuItem;
    ppExportBalanta: TMenuItem;
    ppExportExcel: TMenuItem;
    ppExportXML: TMenuItem;
    ppExportCSV: TMenuItem;
    ppExportHTML: TMenuItem;
    saveDialogExcel: TSaveDialog;
    saveDialogCSV: TSaveDialog;
    saveDialogXML: TSaveDialog;
    saveDialogHTML: TSaveDialog;
    edClasaFunctionala: TcxCheckComboBox;
    lbValuta: TLabel;
    edTipValuta: TcxImageComboBox;
    Label6: TLabel;
    Label7: TLabel;
    Label8: TLabel;
    Label9: TLabel;
    lbTipDefalcare: TLabel;
    edTipDefalcare: TcxImageComboBox;
    edTipInchidere: TcxComboBox;
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure FormCreate(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure Cmd_FisaContExecute(Sender: TObject);
    procedure ChkAplicaCuloriClick(Sender: TObject);
    procedure Cmd_SalveazaTipBalantaExecute(Sender: TObject);
    procedure BtnOkClick(Sender: TObject);
    procedure CheckErori;
    procedure btnGoClick(Sender: TObject);
    procedure ChkArataFunctionalitateClick(Sender: TObject);
    procedure btnGenerareNoteClick(Sender: TObject);
    procedure btnGenNoteInchidereClick(Sender: TObject);
    procedure cxTreeBalantaCustomDrawDataCell(Sender: TcxCustomTreeList;
      ACanvas: TcxCanvas; AViewInfo: TcxTreeListEditCellViewInfo;
      var ADone: Boolean);
    procedure cxTreeBalantaEXPLICATIEGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure edAnRaportarePropertiesChange(Sender: TObject);
    procedure edListaLuniPropertiesChange(Sender: TObject);
    procedure Cmd_ExporXLSExecute(Sender: TObject);
    procedure edGrupPropertiesChange(Sender: TObject);
    procedure ppExportExcelClick(Sender: TObject);
    procedure ppTiparireBalantaClick(Sender: TObject);
    procedure ppExportXMLClick(Sender: TObject);
    procedure ppExportCSVClick(Sender: TObject);
    procedure ppExportHTMLClick(Sender: TObject);
  private
    FColorLevel: TList;
    FSelectedLevel: Integer;
    FNoteEronate  : TfrmNoteEronate;
    FBalantaParams : ParamVector;
    MinYear, MaxYear : Integer;
    MinData, MaxData : TDateTime;
    FirstRun : Boolean;
    procedure LoadTemplateBalanta(Sender: TObject);
    procedure InternalExpand(Sender: TObject);
    function IsValidAnFiscal(Data : TDateTime) : Boolean;
    function GetCheckedList(AChecked: TcxCheckComboBox): String;
    { Private declarations }
  protected
    { Protected declarations }
    procedure ReportClick(Sender: TObject);
  public
    { Public declarations }
    procedure SelectUltimaLuna;
  published
    class function IsMultiInstance: Boolean;
  end;

implementation

uses
  Math, dxCompsUtile, ZeosDBUtile, cxTLExportLink, DateUnit, Variants, FisaContUnit, SaveTemplateUnit, cxStatusKeeper,
  CommonDBVar, RapInclude;

{$R *.DFM}

procedure TFrmBalanta.FormClose(Sender: TObject; var Action: TCloseAction);
begin
  
    Action := caFree;
end;


procedure TFrmBalanta.InternalExpand(Sender: TObject);
var I: Integer;

  procedure ExpandInner(aNode: TcxTreeListNode; Level: Integer);
   var J : Integer;
   begin
     if Level > 0 then begin
        aNode.Expand(False);
        for J := 0 to aNode.Count-1 do
          ExpandInner(aNode.Items[J], Level-1);
     end;
   end;

begin
  with TToolButton(Sender) do begin
    FSelectedLevel := Tag;
    cxTreeBalanta.BeginUpdate;
    cxTreeBalanta.FullCollapse;
    try
       for I := 0 to cxTreeBalanta.Count-1 do
         ExpandInner(cxTreeBalanta.Items[I], Tag-1);
    finally
       cxTreeBalanta.EndUpdate;
    end;
  end;
end;

procedure TFrmBalanta.FormCreate(Sender: TObject);
var
  I: Integer;
  lMenuItem: TMenuItem;
  D, M, Y: Word;
  lFormatCurrency: String;
  lDataSet : TDataSet;
begin

  FirstRun := True;
  PopulateReportContext(Self.ClassName, btnRaportare, ReportClick);
  
  FNoteEronate := TfrmNoteEronate.Create(Self);
  FNoteEronate.WindowState := wsMaximized;

  lFormatCurrency := ',0.00;-,0.00';
  //lFormatCurrency := ',0;-,0';

  for I := 0 to cxTreeBalanta.ColumnCount - 1 do
   if cxTreeBalanta.Columns[I].PropertiesClassName = 'TcxCurrencyEditProperties' then
      TcxCurrencyEditProperties(cxTreeBalanta.Columns[I].Properties).DisplayFormat := lFormatCurrency;

  { Citim si tipurile de balanta salvate in baza de date }
  { si le scriem in popup-ul ppListaTipBalanta }

  DecodeDate(Date, Y, M, D);

  lDataSet := DBNewQuery('SELECT ID_TEMPLATE, DEN_TEMPLATE, SHORT_CUT, DESCRIERE FROM TEMPLATE_BALANTA WHERE STARE=1');
  try
    lDataSet.Open;
     while not lDataSet.Eof do begin
       lMenuItem := TMenuItem.Create(ppMeniu);
       with lMenuItem do begin
         Tag      := lDataSet.Fields[0].AsInteger;
         Caption  := lDataSet.Fields[1].AsString;
         Hint     := lDataSet.Fields[3].AsString;
         OnClick  := LoadTemplateBalanta;
         ShortCut := TextToShortCut(lDataSet.Fields[2].AsString);
       end;
       ppListaTipBalanta.Add(lMenuItem);
       lDataSet.Next;
     end;
    DBSetSQLQuery(lDataSet, 'exec spDateStartBalanta');
    lDataSet.Open;
    if not lDataSet.IsEmpty then begin
     MinYear := lDataSet.Fields[0].AsInteger;
     MaxYear := lDataSet.Fields[1].AsInteger;
     MinData := lDataSet.Fields[2].AsDateTime;
     MaxData := lDataSet.Fields[3].AsDateTime;
    end
    else begin
     MinYear := Y;
     MaxYear := Y;
    end;
  finally
    lDataSet.Free;
  end;

  FillImageCombo(edCont.Properties, 'SELECT CONT, CONT + '':'' + ROMANA FROM CPLAN AS A WHERE BALANTA IN (''T'', ''R'') AND NOT EXISTS (SELECT 1 FROM CPLAN WHERE PARINTE = A.CONT)',
                 0, 1, Null, 'Toate Conturile');
  
  FillImageCombo(edGrup.Properties, 'SELECT id_oi_grupe, denumire FROM OI_GRUPE', 'id_oi_grupe', 'denumire', Null, 'Toate Grupele');
  FillImageCombo(edProiect.Properties, 'SELECT id_oi_proiecte, denumire from oi_proiecte order by denumire', 0, 1, Null, 'Toate Proiectele');
  FillCheckCombo(edClasaFunctionala.Properties, 'exec [spClaseFunctionaleNote]', 'cod_functional', 'cod_functional', Null, 'Toate Clasele Functionale');
  FillImageComboFmt(edTipValuta.Properties, 'exec [spListaValuteFolosite] %d, %d', [IdLogin, IdUtilizator], 'id_valuta_tip', 'denumire', Null, 'Toate Valutele');
  if DBProcExists('spContaGetListUnitati') then
    FillCheckCombo(edUnitate.Properties, 'exec [spContaGetListUnitati] 1', 'Id', 'Denumire', Null, 'Toate Unitatile');
  FillImageCombo(edTipDefalcare.Properties, 'exec [spCplanDefalcare]', 'tip', 'denumire', Null, 'Defalcare Cont');

  FColorLevel := TList.Create;
  FColorLevel.Add(Pointer(clSkyBlue));
  FColorLevel.Add(Pointer(clAqua));
  FColorLevel.Add(Pointer($00A8FFFF));
  FColorLevel.Add(Pointer($00D5FFAA));

  edListaLuni.Properties.Items.Clear;

  for I := 1 to 12 do begin
    with edListaLuni.Properties.Items.Add do begin
      Value := IntToStr(I);
      Description := LongMonthNames[I];
    end;
  end;
  edAnRaportare.Value := MinYear;
  if Y = MinYear then
    edListaLuni.EditValue := IntToStr(M)
  else begin
    edListaLuni.EditValue := IntToStr(12);
    edAnRaportare.Value := MinYear;
  end;
end;

procedure TFrmBalanta.FormDestroy(Sender: TObject);
begin
  FColorLevel.Free;
  FNoteEronate.Free;
end;

function TFrmBalanta.GetCheckedList(AChecked: TcxCheckComboBox): String;
var
  I: Integer;
  lResults: TStringList;
  lCheckStates: TcxCheckStates;
begin
  CalculateCheckStates(AChecked.EditValue, AChecked.Properties.Items, AChecked.Properties.EditValueFormat, lCheckStates);
  lResults := TStringList.Create;
  try
    for I := 0 to AChecked.Properties.Items.Count-1 do
      if lCheckStates[I] = cbsChecked then
        lResults.Add(AChecked.Properties.Items[I].ShortDescription);
    Result := lResults.CommaText;
  finally
    lResults.Free;
  end;
end;

procedure TFrmBalanta.Cmd_FisaContExecute(Sender: TObject);
var lNode: TcxDBTreeListNode;
    lCodRep: Integer;
    lContPlan,
    lContDesc,
    lCodE, lCodF : String;
begin
  lNode := TcxDBTreeListNode(cxTreeBalanta.FocusedNode);
  if lNode <> nil then begin
     if Trim(lNode.Texts[cxTreeBalantaCODREP.ItemIndex]) = '' then lCodRep := -1
     else lCodRep := lNode.Values[cxTreeBalantaCODREP.ItemIndex];
     lCodE := Trim(lNode.Texts[cxTreeBalantaCOD_ECONOMIC.ItemIndex]);
     lCodF := Trim(lNode.Texts[cxTreeBalantaCOD_FUNCTIONAL.ItemIndex]);
     lContPlan := lNode.Texts[cxTreeBalantaCONT_PLAN.ItemIndex];
     lContDesc := lContPlan + ' CF : ' + lCodF + ' CE : ' + lCodE;
//     lContDesc := lNode.Texts[cxTreeBalantaROMANA.ItemIndex];
     ShowFisaCont(lNode.KeyValue, lContPlan, lContDesc, edDataStart.Date, edDataEnd.Date, edTipInchidere.ItemIndex, QryBalanta);
  end;
end;


procedure TFrmBalanta.ChkAplicaCuloriClick(Sender: TObject);
begin
  cxTreeBalanta.BeginUpdate;
  cxTreeBalanta.EndUpdate;
end;


procedure cxTreeLoadFromStream(aTree: TcxDBTreeList; aStream: TStream);
var
  lReader : TReader;
  S: String;
  I, ID, J: Integer;
  lBand: TcxTreeListBand;
  lColumn: TcxTreeListColumn;
  FList: TStringList;
begin
  with ATree do begin
    lReader := TReader.Create(AStream, 4096);
    FList := TStringList.Create;
    try
       BeginUpdate;
       { Citim Setarile }
       aTree.OptionsView.GridLines := TcxTreeListGridLines(lReader.ReadInteger);
       aTree.Preview.Visible := lReader.ReadBoolean;
       aTree.OptionsView.BandLineHeight := lReader.ReadInteger;
       aTree.OptionsView.Bands := lReader.ReadBoolean;
       aTree.OptionsView.Headers := lReader.ReadBoolean;
       aTree.OptionsView.HeaderAutoHeight := lReader.ReadBoolean;
       { Am Terminat de citit setarile }

       { Citim Banzile }
       for I := 0 to Bands.Count - 1 do begin
         ID    := lReader.ReadInteger;
         lBand := nil;
         for J := 0 to Bands.Count-1 do
           if Bands[J].ID = ID then begin
              lBand := Bands[J];
              Break;
           end;
         if lBand <> nil then begin
            lBand.Visible := lReader.ReadBoolean;
            lBand.Width   := lReader.ReadInteger;
            lBand.Caption.Text := lReader.ReadStr;
            Str(lReader.ReadInteger: 11, S);
            FList.AddObject(S, lBand);
         end;
       end;

       { Sortam Benzile cum s-au salvat }
       FList.Sorted := True;
       for I := 0 to FList.Count - 1 do
         TcxTreeListBand(FList.Objects[I]).Index := I;

       { Citim Coloanele }
       FList.Clear;
       FList.Sorted := False;
       for I := 0 to ColumnCount - 1 do begin
         lColumn := Columns[I];
         with lColumn do begin
           Font.Size  := lReader.ReadInteger;
           Font.Name  := lReader.ReadStr;
           Font.Color := TColor(lReader.ReadInteger);
           Color      := TColor(lReader.ReadInteger);
           Caption.Text    := lReader.ReadStr;
           Visible    := lReader.ReadBoolean;
           Width      := lReader.ReadInteger;
           SortOrder     := TcxDataSortOrder(lReader.ReadInteger);
         end;
         Str(lReader.ReadInteger : 11, S);
         FList.AddObject(S, lColumn);
       end;
       FList.Sorted := True;
       for I := 0 to FList.Count-1 do
         TcxTreeListColumn(FList.Objects[I]).ItemIndex := I;
       try
         for I := 0 to ColumnCount - 1 do begin
           lColumn := Columns[I];
           lColumn.Position.BandIndex := lReader.ReadInteger;
           lColumn.Position.RowIndex  := lReader.ReadInteger;
           lColumn.Position.ColIndex := lReader.ReadInteger;
           lColumn.Position.LineCount := lReader.ReadInteger;
         end;
       finally
       end;
       EndUpdate;
    finally
       lReader.Free;
       FList.Free;
    end;
  end;
end;


procedure cxTreeSaveToStream(aTree: TcxDBTreeList; aStream: TStream);
var
  lWriter : TWriter;
  I: Integer;
begin
  with aTree do begin
    lWriter := TWriter.Create(AStream, 4096);
    try
       { Scriem Setarile }
       lWriter.WriteInteger(Integer(aTree.OptionsView.GridLines));
       lWriter.WriteBoolean(aTree.Preview.Visible);
       lWriter.WriteInteger(aTree.OptionsView.BandLineHeight);
       lWriter.WriteBoolean(aTree.OptionsView.Bands);
       lWriter.WriteBoolean(aTree.OptionsView.Headers);
       lWriter.WriteBoolean(aTree.OptionsView.HeaderAutoHeight);       
       { Am Terminat de scris setarile }

       { Scriem Banzile }
       for I := 0 to Bands.Count - 1 do begin
         lWriter.WriteInteger(Bands[I].ID);
         lWriter.WriteBoolean(Bands[I].Visible);
         lWriter.WriteInteger(Bands[I].Width);
         lWriter.WriteStr(Bands[I].Caption.Text);
         lWriter.WriteInteger(Bands[I].Index);
       end;

       { Scriem Coloanele }
       for I := 0 to ColumnCount - 1 do
         with Columns[I] do begin
           lWriter.WriteInteger(Font.Size);
           lWriter.WriteStr(Font.Name);
           lWriter.WriteInteger(Integer(Font.Color));
           lWriter.WriteInteger(Integer(Color));
           lWriter.WriteStr(Caption.Text);
           lWriter.WriteBoolean(Visible);
           lWriter.WriteInteger(Width);
           lWriter.WriteInteger(Integer(SortOrder));
           lWriter.WriteInteger(ItemIndex);
         end;
         
       { Scriem si asezarea }
       for I := 0 to ColumnCount - 1 do begin
         lWriter.WriteInteger(Columns[I].Position.BandIndex);
         lWriter.WriteInteger(Columns[I].Position.RowIndex);
         lWriter.WriteInteger(Columns[I].Position.ColIndex);
         lWriter.WriteInteger(Columns[I].Position.LineCount);                  
       end;

    finally
       lWriter.Free;
    end;
  end;
end;

procedure TFrmBalanta.LoadTemplateBalanta(Sender: TObject);
var
  lStream   : TStream;
  lDataSet  : TDataSet;
begin
  lDataSet := DBNewQueryFmt('SELECT TEMPLATE FROM TEMPLATE_BALANTA WHERE ID_TEMPLATE = %d', [TMenuItem(Sender).Tag]);
  try
    lDataSet.Open;
    if not lDataSet.IsEmpty then begin
      lStream := lDataSet.CreateBlobStream(lDataSet.Fields[0], bmRead);
      try
        cxTreeLoadFromStream(cxTreeBalanta, lStream);
      finally
        lStream.Free;
      end;
    end;
  finally
    lDataSet.Free;
  end;
end;

procedure TFrmBalanta.Cmd_SalveazaTipBalantaExecute(Sender: TObject);
var
  lStream: TMemoryStream;
  lMenuItem: TMenuItem;
begin
  with TfrmSaveTemplate.Create(Application) do
    try
      edName.Text := 'TemplateNou'+IntToStr(GetNextId('TEMPLATE_BALANTA'));
      if ShowModal = mrOk then begin
        lStream := TMemoryStream.Create;
        try
          cxTreeSaveToStream(cxTreeBalanta, lStream);
          DBExecSQLFmt('INSERT INTO TEMPLATE_BALANTA (DEN_TEMPLATE, TEMPLATE, SHORT_CUT, DESCRIERE, STARE)'#13#10+
                       'VALUES (%s, %s, %s, %s, 1)',
                         [
                          ValueToStr(edName.EditValue),
                          DBStreamToStr(lStream),
                          ValueToStr(edShortCut.EditValue),
                          ValueToStr(edHint.EditValue)
                         ]);
          lMenuItem := TMenuItem.Create(ppMeniu);
          lMenuItem.Caption   := edName.EditingText;
          lMenuItem.Hint      := edHint.Lines.Text;
          lMenuItem.ShortCut  := TextToShortCut(edShortCut.EditText);
          lMenuItem.Tag       := LastIdentSession();
          ppListaTipBalanta.Add(lMenuItem);
        finally
          lStream.Free;
        end;
      end;
    finally
      Free;
    end;
end;

procedure TFrmBalanta.BtnOkClick(Sender: TObject);
begin
  Close;
end;

procedure TFrmBalanta.CheckErori;
begin
  with FNoteEronate do begin
    // Sa evitam LoadAllRecords
    DTNoteEronate.DataSet := nil;
    QryNoteErr.Close;
    QryNoteErr.Params[0].Value := edDataStart.Date;
    QryNoteErr.Params[1].Value := edDataEnd.Date;
    QryNoteErr.Open;
    if not QryNoteErr.IsEmpty then begin
       DTNoteEronate.DataSet := QryNoteErr;
       viewNoteEronate.ApplyBestFit(nil);
       ShowModal;
    end;
    QryNoteErr.Close;
  end;
end;

procedure TFrmBalanta.btnGoClick(Sender: TObject);
var
  lStream: TMemoryStream;
  lSaveRefresh: Boolean;
begin
  if CommonDBVar.IsValidDate(edDataStart.EditValue) then edDataStart.ValidateEdit(False);
  if CommonDBVar.IsValidDate(edDataEnd.EditValue)  then edDataEnd.ValidateEdit(False);

  if not IsValidAnFiscal(edDataStart.Date) then begin
    MessageDlg('Data Start nu corespunde perioadei fiscale asociate bazei de date !', mtError, [mbOK], 0);
    edDataStart.Date := MinData;
    Abort;
  end;

  if not IsValidAnFiscal(edDataEnd.Date) then begin
    MessageDlg('Data Sfarsit nu corespunde perioadei fiscale asociate bazei de date !', mtError, [mbOK], 0);
    edDataEnd.Date := MaxData;
    Abort;
  end;

  if (pos(' ', edDataStart.Text) > 0) or
     (pos(' ', edDataEnd.Text) > 0) then Exit;
  { Testam daca nu cumva avem note eronate }
  CheckErori;
  Self.Caption := 'Balanta : '+FormatDateTime('dd.mm.yyyy', edDataStart.Date)+' - '+ FormatDateTime('dd.mm.yyyy', edDataEnd.Date);

  QryBalanta.Params.ParamByName('DATA_MIN').Value       := edDataStart.Date;
  QryBalanta.Params.ParamByName('DATA_MAX').Value       := edDataEnd.Date;
  QryBalanta.Params.ParamByName('DOAR_MISCARI').Value   := ChkDoarMiscari.EditValue;
  QryBalanta.Params.ParamByName('PE_REPARTITORI').Value := ChkPeRepartitori.EditValue;
  QryBalanta.Params.ParamByName('PE_MATERIALE').Value   := ChkPeMateriale.EditValue;
  QryBalanta.Params.ParamByName('CONT').Value           := edCont.EditValue;
  QryBalanta.Params.ParamByName('CU_RECALCUL').Value    := IfThen(chkPreluareNote.Checked, 1, 0);
  QryBalanta.Params.ParamByName('param').Value          := Null;
  QryBalanta.Params.ParamByName('CU_INCHIDERE').Value   := edTipInchidere.ItemIndex;
  QryBalanta.Params.ParamByName('CONSOLIDATA').Value    := chkConsolidata.EditValue;
  QryBalanta.Params.ParamByName('COD_FUNCTIONAL').Value := GetCheckedList(edClasaFunctionala);
  QryBalanta.Params.ParamByName('ID_OI_GRUPE').Value    := edGrup.EditValue;
  QryBalanta.Params.ParamByName('ID_PROIECT').Value     := edProiect.EditValue;
  QryBalanta.Params.ParamByName('IS_SINTETIC').Value    := chkSintetic.EditValue;
  QryBalanta.Params.ParamByName('ID_UNITATE').Value     := GetCheckedList(edUnitate);
  QryBalanta.Params.ParamByName('TIP_BALANTA').Value    := edTipBalanta.EditValue;
  QryBalanta.Params.ParamByName('CU_EXECUTIE').Value    := chkExecutie.EditValue;
  QryBalanta.Params.ParamByName('tipDefalcare').Value   := edTipDefalcare.EditValue;
  Screen.Cursor := crHourGlass;
  try
    Application.ProcessMessages;
    DBRefresh(QryBalanta);
    if FirstRun then begin
      cxCreateMissingColumns(QryBalanta, cxTreeBalanta);
      FirstRun := False;
    end;
    SetExpandLevels(TcxTreeList(cxTreeBalanta), ExpandLevels, InternalExpand);
  finally
    Screen.Cursor := crDefault;
  end;
end;

procedure TFrmBalanta.ChkArataFunctionalitateClick(Sender: TObject);
begin
   cxTreeBalanta.BeginUpdate;
   cxTreeBalanta.EndUpdate;
end;

procedure TFrmBalanta.btnGenerareNoteClick(Sender: TObject);
begin
  DBExecSQL('exec SP_GENERARE_NOTE_SERVER');
end;

procedure TFrmBalanta.btnGenNoteInchidereClick(Sender: TObject);
begin
  DBExecSQL('exec SP_GENERARE_NOTE_SERVER_INCHIDERE');
end;

class function TFrmBalanta.IsMultiInstance: Boolean;
begin
  Result := True;
end;

function TFrmBalanta.IsValidAnFiscal(Data: TDateTime): Boolean;
var
  D, M, Y : Word;
begin
  Result := True;
  DecodeDate(Data, Y, M, D);
  if (Y < MinYear) or  (Y > MaxYear) then
    Result := False; 
end;

procedure TFrmBalanta.cxTreeBalantaCustomDrawDataCell(
  Sender: TcxCustomTreeList; ACanvas: TcxCanvas;
  AViewInfo: TcxTreeListEditCellViewInfo; var ADone: Boolean);


    function GetCurrency(ANode : TcxTreeListNode; AIndex: Integer): Currency;
    begin
      if (Trim(ANode.Texts[AIndex]) = '') or
         (VarIsEmpty(ANode.Values[AIndex])) or
         (VarIsNull(ANode.Values[AIndex])) then Result := 0
      else Result := ANode.Values[AIndex];
    end;

    procedure TestColumn(NewColumn: TcxTreeListColumn);
    var
      lValue : Currency;
    begin
      if NewColumn = AViewInfo.Column then begin
         lValue := GetCurrency(AViewInfo.Node, NewColumn.ItemIndex);
         if lValue < 0 then
            ACanvas.Brush.Color := $008000FF;
      end;
    end;

begin
  if ChkAplicaCulori.Checked then begin
    if AViewInfo.Node.Level < FColorLevel.Count then ACanvas.Brush.Color := TColor(FColorLevel[AViewInfo.Node.Level])
  end;
  if (ChkArataFunctionalitate.Checked) and
     (Length(aViewInfo.Node.Texts[cxTreeBalantaCONT.ItemIndex]) > 1) and
     (aViewInfo.Node.Texts[cxTreeBalantaFCTCONT.ItemIndex] > '') then
     case aViewInfo.Node.Texts[cxTreeBalantaFCTCONT.ItemIndex][1] of
       'B': ACanvas.Brush.Color := clSkyBlue;  //albastru
       'D': ACanvas.Brush.Color := clFuchsia;  //verde
       'C': ACanvas.Brush.Color := clLime;  //galben
     end;
  { Afisam sumele in rosu }
  TestColumn(cxTreeBalantaSID);
  TestColumn(cxTreeBalantaSIC);
  TestColumn(cxTreeBalantaS_D);
  TestColumn(cxTreeBalantaS_C);
end;

procedure TFrmBalanta.cxTreeBalantaEXPLICATIEGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := Trim(ANode.Texts[cxTreeBalantaCONT.ItemIndex]) + ' : ' + Trim(ANode.Texts[cxTreeBalantaROMANA.ItemIndex]);
end;

procedure TFrmBalanta.edAnRaportarePropertiesChange(Sender: TObject);
begin
  if (edAnRaportare.Value < MinYear) or (edAnRaportare.Value > MaxYear) then begin
    MessageDlg('Acest an nu face parte din perioada fiscala asociata bazei de date !', mtError, [mbOK], 0);
    edAnRaportare.Value := MinYear;
    Abort;
  end;
  edDataStart.Date := EncodeDate(edAnRaportare.Value, 1, 1);
  edDataEnd.Date   := EncodeDate(edAnRaportare.Value+1, 1, 1)-1;
end;

procedure TFrmBalanta.edListaLuniPropertiesChange(Sender: TObject);
var
  lLuna: Integer;
begin
  lLuna := ValueSafeToInt(edListaLuni.EditValue, 0);
  if lLuna > 0 then begin
    edDataStart.Date := EncodeDate(edAnRaportare.Value, lLuna, 1);
    if lLuna = 12 then
      edDataEnd.Date := EncodeDate(edAnRaportare.Value+1, 1, 1) - 1
    else edDataEnd.Date := EncodeDate(edAnRaportare.Value, lLuna+1, 1)-1;
  end;
end;


procedure TFrmBalanta.ReportClick(Sender: TObject);
begin
  SetRapParam('DATA_MIN', edDataStart.Date);
  SetRapParam('DATA_MAX',  edDataEnd.Date);
  SetRapParam('DOAR_MISCARI', ChkDoarMiscari.Checked);
  SetRapParam('PE_REPARTITORI', ChkPeRepartitori.Checked);
  SetRapParam('PE_MATERIALE', ChkPeMateriale.Checked);
  SetRapParam('CONT', edCont.EditValue);
  SetRapParam('CU_RECALCUL', Integer(chkPreluareNote.Checked));
  SetRapParam('param', Null);
  SetRapParam('CU_INCHIDERE', edTipInchidere.ItemIndex);
  SetRapParam('CONSOLIDATA', Integer(chkConsolidata.Checked));
  SetRapParam('COD_FUNCTIONAL', GetCheckedList(edClasaFunctionala));
  if chkSintetic.Checked then SetRapParam('IS_SINTETIC', 1)
                          else SetRapParam('IS_SINTETIC',  -1);
  if DBProcExists('spContaGetListUnitati') then
     SetRapParam('ID_UNITATE', GetCheckedList(edUnitate));
  SetRapParam('TIP_BALANTA', StrToInt(edTipBalanta.EditValue));
  SetRapParam('CU_EXECUTIE', chkExecutie.Checked);
  LoadReport(TMenuItem(Sender).Tag);
end;


procedure TFrmBalanta.SelectUltimaLuna;
begin
  edListaLuni.EditValue := DBGetScallarFmt('exec [spBalantaDate] %d', [IdUtilizator]);
end;

procedure TFrmBalanta.Cmd_ExporXLSExecute(Sender: TObject);
var
  SD: TSaveDialog;
begin
  SD := TSaveDialog.Create(nil);
  try
    SD.Title := 'Export in xls';
    SD.Filter := 'Fisiere XLS |*.xls';
    if SD.Execute then
      cxExportTLToExcel(SD.FileName, cxTreeBalanta, True, True, False);
  finally
    SD.Free;
  end;
end;

procedure TFrmBalanta.edGrupPropertiesChange(Sender: TObject);
var
  lProjStr: String;
  lDataSet: TDataSet;
begin
  if VarIsNull(edGrup.EditingValue) or VarIsEmpty(edGrup.EditingValue) or (edGrup.EditingValue = -1) then
    lProjStr := 'select id_oi_proiecte, denumire from oi_proiecte order by denumire'
  else
    lProjStr := Format('select id_oi_proiecte, denumire from oi_proiecte where id_oi_proiecte in (select id_oi_proiecte from oi_grupe_proiecte where id_oi_grupe = %d) order by denumire',
                            [Integer(edGrup.EditingValue)]);
  FillImageCombo(edProiect.Properties, lProjStr, 'id_oi_proiecte', 'denumire', -1, 'Toate Proiectele');
end;

procedure TFrmBalanta.ppExportExcelClick(Sender: TObject);
begin
  if saveDialogExcel.Execute then
    cxExportTLToExcel(saveDialogExcel.FileName, cxTreeBalanta);
end;

procedure TFrmBalanta.ppTiparireBalantaClick(Sender: TObject);
begin
  linkBalanta.ReportTitle.Text := Format('Balanta de la %s pana la %s',
                                  [
                                    FormatDateTime('dd.MM.yyyy', edDataStart.EditValue),
                                    FormatDateTime('dd.MM.yyyy', edDataEnd.EditValue)
                                  ]);
  linkBalanta.Preview(True);
end;

procedure TFrmBalanta.ppExportXMLClick(Sender: TObject);
begin
  if saveDialogXML.Execute then
    cxExportTLToXML(saveDialogXML.FileName, cxTreeBalanta);
end;

procedure TFrmBalanta.ppExportCSVClick(Sender: TObject);
begin
  if saveDialogCSV.Execute then
    cxExportTLToText(saveDialogCSV.FileName, cxTreeBalanta);
end;

procedure TFrmBalanta.ppExportHTMLClick(Sender: TObject);
begin
  if saveDialogHTML.Execute then
    cxExportTLToHTML(saveDialogHTML.FileName, cxTreeBalanta);
end;

end.
