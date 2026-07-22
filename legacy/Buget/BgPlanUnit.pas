
unit BgPlanUnit;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms,
  Dialogs, Menus, ActnList, ImgList, StdCtrls, ExtCtrls, cxGraphics,
  cxTL, cxControls, cxInplaceContainer, cxTLData,
  cxDBTL, cxMaskEdit, cxCheckBox, cxContainer,
  cxEdit, cxTextEdit, cxPC,
  cxSplitter, cxVGrid, cxDBVGrid, cxCurrencyEdit, cxImageComboBox,
  cxDropDownEdit,  cxLookAndFeelPainters, cxButtons,
  cxTLdxBarBuiltInMenu,
  cxLookAndFeels, cxStyles, cxCustomData, dxBarBuiltInMenu,
  cxDataControllerConditionalFormattingRulesManagerDialog,
  dxScrollbarAnnotations, Data.DB, ZAbstractRODataset, ZAbstractDataset,
  ZDataset, cxLookupEdit, cxDBLookupEdit, cxDBLookupComboBox;

const
  WM_UPDATECLASAFC = WM_USER + 1;
  WM_UPDATECLASAEC = WM_USER + 2;

type
  TfrmBGPlan = class(TForm)
    ImaginiConturi: TImageList;
    PaginaClasificatii: TcxPageControl;
    ppCF: TPopupMenu;
    ppCE: TPopupMenu;
    AdaugaCapitol1: TMenuItem;
    Adaugasubcapitol1: TMenuItem;
    MutaCapitolulpeSintetic1: TMenuItem;
    AdaugaSubtitlu1: TMenuItem;
    AdaugaSubtitlu2: TMenuItem;
    MutaTitlupeSintetic1: TMenuItem;
    N1: TMenuItem;
    cxTabFunctional: TcxTabSheet;
    cxTabEconomic: TcxTabSheet;
    pnFunctDetalii: TPanel;
    SplitterFunct: TcxSplitter;
    pnFunctClient: TPanel;
    cxFunctParams: TcxDBVerticalGrid;
    Panel4: TPanel;
    Label2: TLabel;
    cxTreeFunctional: TcxDBTreeList;
    cxTreeFunctionalCOD_FUNCTIONAL: TcxDBTreeListColumn;
    cxTreeFunctionalDENUMIRE: TcxDBTreeListColumn;
    cxTreeFunctionalDESCRIERE: TcxDBTreeListColumn;
    cxTreeFunctionalPLANIFICAT1: TcxDBTreeListColumn;
    cxTreeFunctionalPLANIFICAT2: TcxDBTreeListColumn;
    cxTreeFunctionalPLANIFICAT3: TcxDBTreeListColumn;
    cxTreeFunctionalPLANIFICAT4: TcxDBTreeListColumn;
    cxTreeFunctionalCLASA: TcxDBTreeListColumn;
    cxTreeFunctionalCAPITOL: TcxDBTreeListColumn;
    cxTreeFunctionalESTE_LUCRARE: TcxDBTreeListColumn;
    cxTreeFunctionalESTE_NIVEL_RAPORTARE: TcxDBTreeListColumn;
    cxFunctParamsCOD_BUGET: TcxDBEditorRow;
    cxFunctParamsDENUMIRE: TcxDBEditorRow;
    cxFunctParamsDESCRIERE: TcxDBEditorRow;
    cxFunctParamsCAPITOL: TcxDBEditorRow;
    cxFunctParamsESTE_LUCRARE: TcxDBEditorRow;
    cxFunctParamsESTE_NIVEL_RAPORTARE: TcxDBEditorRow;
    pnEcoDetalii: TPanel;
    cxSplitter1: TcxSplitter;
    pnEcoClient: TPanel;
    cxEcoDetalii: TcxDBVerticalGrid;
    cxEcoDetaliiCOD_BUGET: TcxDBEditorRow;
    cxEcoDetaliiDENUMIRE: TcxDBEditorRow;
    edtFiltruBuget: TcxImageComboBox;
    cxTreeEconomic: TcxDBTreeList;
    cxTreeEconomicCOD_ECONOMIC: TcxDBTreeListColumn;
    cxTreeEconomicDENUMIRE: TcxDBTreeListColumn;
    cxTreeEconomicDESCRIERE: TcxDBTreeListColumn;
    cxTreeEconomicNUMAR_RAND: TcxDBTreeListColumn;
    cxTreeEconomicCLASA: TcxDBTreeListColumn;
    cxFunctParamsID_BG_TIPURI_BUGET: TcxDBEditorRow;
    cxFunctParamsID_BG_PLAN_FUNCTIONAL: TcxDBEditorRow;
    cxFunctParamsCLASA: TcxDBEditorRow;
    cxFunctParamsTIP_BUGET: TcxDBEditorRow;
    cxFunctParamsESTE_STANDARD: TcxDBEditorRow;
    cxFunctParamsTIP_REFLECTARE: TcxDBEditorRow;
    cxEcoDetaliiID_BG_PLAN_ECONOMIC: TcxDBEditorRow;
    cxEcoDetaliiID_PARINTE: TcxDBEditorRow;
    cxEcoDetaliiCLASA: TcxDBEditorRow;
    cxFunctParamsCategoryRow1: TcxCategoryRow;
    cxFunctParamsCategoryRow2: TcxCategoryRow;
    cxEcoDetaliiESTE_STANDARD: TcxDBEditorRow;
    cxEcoDetaliiBOLD: TcxDBEditorRow;
    cxEcoDetaliiTIP_REFLECTARE: TcxDBEditorRow;
    cxEcoDetaliiCategoryRow1: TcxCategoryRow;
    cxEcoDetaliiCategoryRow2: TcxCategoryRow;
    Actuni: TActionList;
    CmdAdaugaCapitol: TAction;
    CmdAdaugaSubCapitol: TAction;
    CmdCapitolPeSintetic: TAction;
    CmdAdaugaTitlu: TAction;
    CmdAdaugaSubtitlu: TAction;
    CmdTitluPeSintetic: TAction;
    CmdStergeCapitol: TAction;
    CmdStergeTitlu: TAction;
    N2: TMenuItem;
    Stergeclasificatiecurenta1: TMenuItem;
    N3: TMenuItem;
    StergeTitlu1: TMenuItem;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    cxFunctParamsTIP_SECTIUNE: TcxDBEditorRow;
    cxEcoDetaliiTIP_SECTIUNE: TcxDBEditorRow;
    cxEcoDetaliiINTRODUCERE_ESTIMARE: TcxDBEditorRow;
    cxEcoDetaliiINTRODUCERE_CA: TcxDBEditorRow;
    cxFunctParamsALIAS_CONT: TcxDBEditorRow;
    cxDBTreeList1: TcxDBTreeList;
    ZQuery1: TZQuery;
    qryCPV: TZQuery;
    dtCPV: TDataSource;
    cxEcoDetaliiCategoryRow3: TcxCategoryRow;
    cxEcoDetaliiDBEditorRow1: TcxDBEditorRow;
    cxLookupComboBox1: TcxLookupComboBox;
    qryCPVCPV_AFISARE: TStringField;
    qryCPVcodCPV: TStringField;
    qryCPVDENUMIRE: TStringField;

    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure BtnOkClick(Sender: TObject);
    procedure BtnCancelClick(Sender: TObject);
    procedure CmdAdaugaCapitolExecute(Sender: TObject);
    procedure CmdAdaugaTitluExecute(Sender: TObject);
    procedure CmdAdaugaSubCapitolExecute(Sender: TObject);
    procedure CmdAdaugaSubtitluExecute(Sender: TObject);
    procedure CmdTitluPeSinteticExecute(Sender: TObject);
    procedure CmdCapitolPeSinteticExecute(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure cxTreeFunctionalDragOver(Sender, Source: TObject; X,
      Y: Integer; State: TDragState; var Accept: Boolean);
    procedure cxTreeEconomicDragOver(Sender, Source: TObject; X,
      Y: Integer; State: TDragState; var Accept: Boolean);
    procedure cxTreeFunctionalEndDrag(Sender, Target: TObject; X,
      Y: Integer);
    procedure cxTreeEconomicGetNodeImageIndex(Sender: TcxCustomTreeList;
      ANode: TcxTreeListNode; AIndexType: TcxTreeListImageIndexType;
      var AIndex: TImageIndex);
    procedure cxTreeFunctionalGetNodeImageIndex(Sender: TcxCustomTreeList;
      ANode: TcxTreeListNode; AIndexType: TcxTreeListImageIndexType;
      var AIndex: TImageIndex);

    procedure cxTreeFunctionalDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure cxTreeEconomicDESCRIEREGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode;
      var Value: String);
    procedure edtFiltruBugetPropertiesChange(Sender: TObject);
    procedure FormCloseQuery(Sender: TObject; var CanClose: Boolean);
    procedure CmdStergeCapitolExecute(Sender: TObject);
    procedure CmdStergeTitluExecute(Sender: TObject);
    procedure cxTreeEconomicDragDrop(Sender, Source: TObject; X,
      Y: Integer);
    procedure cxTreeFunctionalDragDrop(Sender, Source: TObject; X,
      Y: Integer);
    procedure cxTreeFunctionalKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure cxTreeFunctionalCustomDrawDataCell(Sender: TcxCustomTreeList;
      ACanvas: TcxCanvas; AViewInfo: TcxTreeListEditCellViewInfo;
      var ADone: Boolean);
    procedure qryCPVCalcFields(DataSet: TDataSet);



  private
    { Private declarations }
    procedure cxMutaInSus(ANode: TcxDBTreeListNode);
    procedure cxUpdateClasa(ANode: TcxDBTreeListNode);
    procedure cxSetClasaFromNode(ANode: TcxDBTreeListNode);
    procedure cxSetCurentNode(ATree: TcxDBTreeList);

    function  GetNextChildFunctional(aCont: Variant): Variant;
    function  GetNextChildEconomic(aCont: Variant): Variant;
    procedure PopulateTipBuget;
    procedure TestFunctional(aCont : String; aOperatie : String; const inAnalitic : Boolean = False);
    procedure TestEconomic(aCont : String; aOperatie : String; const inAnalitic : Boolean = False);

    procedure WmUpdateClasaFc(var aMessage : TMessage); message WM_UPDATECLASAFC;
    procedure WmUpdateClasaEc(var aMessage : TMessage); message WM_UPDATECLASAEC;
   private
    FTypedText: string;


  public
    { Public declarations }
  end;


implementation

{$R *.DFM}

uses
  dxCompsUtile, ZeosDBUtile, ATSZDBUtils, DateUnit,
  StrUtils, CommonDBVar;

procedure TfrmBGPlan.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  DBRefresh(FrmData.qryBGPlanFunctionalComplet);
  Action := caFree;
end;

procedure TfrmBGPlan.BtnOkClick(Sender: TObject);
begin
  DBPost([FrmData.qryBGPlanFunctional, FrmData.qryBGPlanEconomic]);
  if fsModal in FormState then ModalResult := mrOk
  else Close;
end;

procedure TfrmBGPlan.BtnCancelClick(Sender: TObject);
begin
  FrmData.qryBGPlanFunctional.CancelUpdates;
  FrmData.qryBGPlanEconomic.CancelUpdates;
  Close;
end;



procedure TfrmBGPlan.CmdAdaugaCapitolExecute(Sender: TObject);
var
    //pfID: Integer;
    aParent : Variant;
begin
  if cxTreeFunctional.FocusedNode <> nil then
     if cxTreeFunctional.FocusedNode.Parent <> nil then begin
        AParent := TcxDBTreeListNode(cxTreeFunctional.FocusedNode).ParentKeyValue;
     end
     else AParent := Null
  else AParent := Null;
  with FrmData.qryBGPlanFunctional do
    try
      DisableControls;
      Append;
      FieldByName('COD_FUNCTIONAL').AsString := GetNextChildFunctional(aParent);
      FieldByName('ID_PARINTE').Value  := AParent;
      FieldByName('DENUMIRE').AsString := 'CAPITOL NOU';
      FieldByName('TIP_REFLECTARE').AsInteger := 0;
      FieldByName('TIP_SECTIUNE').AsInteger := 0;
  finally
     cxSetCurentNode(cxTreeFunctional);
  end;
end;

procedure TfrmBGPlan.CmdAdaugaTitluExecute(Sender: TObject);
var aParent : Variant;
begin
  if cxTreeEconomic.FocusedNode <> nil then
     if cxTreeEconomic.FocusedNode.Parent <> nil then
        AParent := TcxDBTreeListNode(cxTreeEconomic.FocusedNode).ParentKeyValue
     else AParent := Null
  else AParent := Null;
  with FrmData.qryBGPlanEconomic do
    try
      DisableControls;
      Append;
      FieldByName('COD_ECONOMIC').AsString := GetNextChildEconomic(aParent);
      FieldByName('ID_PARINTE').Value  := AParent;
      FieldByName('DENUMIRE').AsString := 'TITLU NOU';
      FieldByName('TIP_REFLECTARE').AsInteger := 0;
      FieldByName('TIP_SECTIUNE').AsInteger := 0;
  finally
      cxSetCurentNode(cxTreeEconomic);
    end;
end;

procedure TfrmBGPlan.CmdAdaugaSubCapitolExecute(Sender: TObject);
var lNode: TcxDBTreeListNode;
    aParent : Variant;
begin
  lNode := TcxDBTreeListNode(cxTreeFunctional.FocusedNode);
  if Assigned(lNode) then
    aParent := lNode.KeyValue
  else
    aParent := Null;
  with FrmData.qryBGPlanFunctional do
    try
      DisableControls;
      Append;
      FieldByName('DENUMIRE').AsString := 'CAPITOL NOU';
      FieldByName('ID_PARINTE').Value  := aParent;
      FieldByName('COD_FUNCTIONAL').AsString := GetNextChildFunctional(aParent);
      FieldByName('TIP_REFLECTARE').AsInteger := 0;
      FieldByName('TIP_SECTIUNE').AsInteger := 0;
  finally
      cxSetCurentNode(cxTreeFunctional);
  end;
end;

procedure TfrmBGPlan.CmdAdaugaSubtitluExecute(Sender: TObject);
var lNode: TcxDBTreeListNode;
    aParent : Variant;
begin
  lNode := TcxDBTreeListNode(cxTreeEconomic.FocusedNode);
  if Assigned(lNode) then
    aParent := lNode.KeyValue
  else
    aParent := Null;
  with FrmData.qryBGPlanEconomic do
  try
      DisableControls;
      Append;
      FieldByName('DENUMIRE').AsString := 'CAPITOL NOU';
      FieldByName('ID_PARINTE').Value  := aParent;
      FieldByName('COD_ECONOMIC').AsString := GetNextChildEconomic(aParent);
      FieldByName('TIP_REFLECTARE').AsInteger := 0;
      FieldByName('TIP_SECTIUNE').AsInteger := 0;      
  finally
      cxSetCurentNode(cxTreeEconomic);
  end;
end;

procedure TfrmBGPlan.CmdTitluPeSinteticExecute(Sender: TObject);
begin
  cxMutaInSus(TcxDBTreeListNode(cxTreeEconomic.FocusedNode));
end;

procedure TfrmBGPlan.CmdCapitolPeSinteticExecute(Sender: TObject);
begin
  cxMutaInSus(TcxDBTreeListNode(cxTreeFunctional.FocusedNode));
end;



procedure TfrmBGPlan.FormCreate(Sender: TObject);
begin
  DBRefresh([frmData.qryBGPlanFunctional, frmData.qryBGPlanEconomic]);
  cxTreeEconomic.FullExpand;
  cxTreeFunctional.FullExpand;
  PopulateTipBuget;



  cxTreeFunctional.FindPanel.Behavior := fcbSearch;
  cxTreeFunctional.FindPanel.HighlightSearchResults := True;
  cxTreeFunctional.FindPanel.UseDelayedFind := True;
  cxTreeFunctional.FindPanel.ApplyInputDelay := 300;

  cxTreeEconomic.FindPanel.Behavior := fcbSearch;
  cxTreeEconomic.FindPanel.HighlightSearchResults := True;
  cxTreeEconomic.FindPanel.UseDelayedFind := True;
  cxTreeEconomic.FindPanel.ApplyInputDelay := 300;



   //ShowMessage(BoolToStr(cxTreeFunctional.FindPanel.HighlightSearchResults, True));

          qryCPV.Close;
  qryCPV.Open;

  cxLookupComboBox1.Properties.DropDownListStyle := lsFixedList;
  cxLookupComboBox1.Properties.DropDownWidth := 380;
end;



//------------------------------------------------------------------------------
function TfrmBGPlan.GetNextChildFunctional(aCont: Variant): Variant;
var TmpCont: String;
    StartIndex: Integer;
begin
  { Luam Urmatorul Analitic disponibil }
  with GetTmpADOQuery do
    try
      if DBProcExists('SP_GET_NEXT_CHILD_BG_FUNCTIONAL') then begin
        if (VarIsNull(aCont)) or (VarIsEmpty(aCont)) then begin
          Sql.Add('SELECT MAX(COD_FUNCTIONAL) FROM BG_PLAN_FUNCTIONAL WHERE ID_PARINTE IS NULL');
          StartIndex := 1;
          Open;
          TmpCont := Fields[0].AsString;
          while (StartIndex <= Length(TmpCont)) and (TmpCont[StartIndex] = '.') do Inc(StartIndex);

          if StartIndex < Length(TmpCont) then
             Result := Copy(TmpCont, 1, Length(TmpCont) - 1) + Chr(Ord(TmpCont[Length(TmpCont)])+1)
          else
              Result := VarToStr(aCont) + '.1';
        end;
      end
      else begin
        Sql.Add('EXEC SP_GET_NEXT_CHILD_BG_FUNCTIONAL '''+VarToStr(aCont)+'''');
        //StartIndex := Length(VarToStr(aCont));
        try
          Open;
          Result := Fields[0].AsString;
        except
          Result := '';
        end;
      end;
    finally
      Free;
    end;
end;


function TfrmBGPlan.GetNextChildEconomic(aCont: Variant): Variant;
var TmpCont: String;
    StartIndex: Integer;
begin
  { Luam Urmatorul Analitic disponibil }
  with GetTmpADOQuery do
    try
       if (VarIsNull(aCont)) or (VarIsEmpty(aCont)) then begin
          Sql.Add('SELECT MAX(COD_BUGET) FROM BUGET_PLAN_ECONOMIC WHERE ID_PARINTE IS NULL');
          StartIndex := 1;
          Open;
          TmpCont := Fields[0].AsString;
          while (StartIndex <= Length(TmpCont)) and (TmpCont[StartIndex] = '.') do Inc(StartIndex);

          if StartIndex < Length(TmpCont) then
              Result := Copy(TmpCont, 1, Length(TmpCont) - 1) + Chr(Ord(TmpCont[Length(TmpCont)])+1)
          else
              Result := VarToStr(aCont) + '.1';
          end
       else begin
          Sql.Add('EXEC SP_GET_NEXT_CHILD_BG_ECONOMIC '''+VarToStr(aCont)+'''');
          try
            Open;
            Result := Fields[0].AsString;
          except
            Result := '';
          end;
       end;
     finally
       Free;
    end;
end;

procedure TfrmBGPlan.PopulateTipBuget;
var
  lDataSet  : TDataSet;
begin
  lDataSet := DBNewQuery('SELECT TIP_BUGET + '' ''+  DENUMIRE AS DENUMIRE,  * FROM BG_TIPURI_BUGET');
  try
    lDataSet.Open;
    FillImageCombo(edtFiltruBuget.Properties, lDataSet, 'ID_BG_TIPURI_BUGET', 'DENUMIRE', Null, '<Toate tipurile de bugete>');
    FillImageCombo(cxFunctParamsID_BG_TIPURI_BUGET.Properties.EditProperties, lDataSet, 'ID_BG_TIPURI_BUGET', 'DENUMIRE');
    FillImageCombo(cxFunctParamsTIP_BUGET.Properties.EditProperties, lDataSet, 'TIP_BUGET', 'TIP_BUGET');
  finally
    lDataSet.Free;
  end;
end;

procedure TfrmBGPlan.qryCPVCalcFields(DataSet: TDataSet);
begin
      qryCPV.FieldByName('CPV_AFISARE').AsString :=
    qryCPV.FieldByName('codCPV').AsString + ' - ' +
    qryCPV.FieldByName('DENUMIRE').AsString;
end;

procedure TfrmBGPlan.cxUpdateClasa(ANode: TcxDBTreeListNode);
var
  lIsFunctional : Boolean;
begin
  if aNode = nil then Exit;
  lIsFunctional := (TcxDBTreeList(ANode.TreeList) = cxTreeFunctional);
  try
    TcxDBTreeList(ANode.TreeList).BeginUpdate;
    cxSetClasaFromNode(ANode);
  finally
    TcxDBTreeList(ANode.TreeList).EndUpdate;
  end;
  { Actualizam si clasa din Planificare Bugetara }
  with GetTmpADOQuery do
    try
       ParamCheck := False;
       if lIsFunctional then begin
          Sql.Add('EXEC SP_BG_ACTUALIZEZA_CLASA 0');
       end
       else begin
          Sql.Add('EXEC SP_BG_ACTUALIZEZA_CLASA 1');
       end;
       ExecSql;
    finally
       Free;
    end;
end;








procedure TfrmBGPlan.cxSetClasaFromNode(ANode: TcxDBTreeListNode);
var J: Integer;

  function cxGetClasaFromNode(Node: TcxDBTreeListNode): String;
   begin
     Result := RightStr('000000' + IntToStr(Integer(Node.KeyValue)), 6);
     if Assigned(Node.Parent) and not (Node.Parent is TcxTreeListRootNode) then Result := cxGetClasaFromNode(TcxDBTreeListNode(Node.Parent))+Result;
     if  (Length(Result)>1) and (RightStr(Result, 1) <> '.') then Result := Result + '.';
   end;

begin
  if not Assigned(ANode) then Exit;
  with TcxDBTreeList(ANode.TreeList) do begin
    //BeginUpdate;
    try
      with DataController.DataSource.DataSet do begin
        if FieldByName(DataController.KeyField).AsInteger <> ANode.KeyValue then
           if not Locate(DataController.KeyField, ANode.KeyValue, []) then raise EContaHandledError.Create('Eroare localizare inregistrare !');
        if not (State in [dsEdit, dsInsert]) then Edit;
        FieldByName('CLASA').AsString := cxGetClasaFromNode(ANode);
        if State in [dsEdit, dsInsert] then Post;
      end;
      if ANode.HasChildren then
         for J := 0 to ANode.Count - 1 do
           cxSetClasaFromNode(TcxDBTreeListNode(ANode.Items[J]));
    finally
      //EndUpdate;
    end;
  end;
end;

procedure TfrmBGPlan.cxTreeFunctionalDragOver(Sender, Source: TObject; X,
  Y: Integer; State: TDragState; var Accept: Boolean);
begin
//
end;

procedure TfrmBGPlan.cxTreeEconomicDragOver(Sender, Source: TObject; X,
  Y: Integer; State: TDragState; var Accept: Boolean);
begin
//
end;

procedure TfrmBGPlan.cxTreeFunctionalEndDrag(Sender, Target: TObject; X,
  Y: Integer);
begin
  cxUpdateClasa(TcxDBTreeListNode(TcxDBTreeList(Sender).FocusedNode));
end;

procedure TfrmBGPlan.cxTreeEconomicGetNodeImageIndex(Sender: TcxCustomTreeList;
  ANode: TcxTreeListNode; AIndexType: TcxTreeListImageIndexType;
  var AIndex: TImageIndex);
begin
  if ANode.Level > 2 then AIndex := 2 else AIndex := ANode.Level;
end;




procedure TfrmBGPlan.cxTreeFunctionalCustomDrawDataCell(
  Sender: TcxCustomTreeList; ACanvas: TcxCanvas;
  AViewInfo: TcxTreeListEditCellViewInfo; var ADone: Boolean);
var
  Node: TcxTreeListNode;
  Col: TcxTreeListColumn;
  CellText: string;
  LowerText, LowerSearch: string;
  FoundPos, SearchLen: Integer;
  DrawRect: TRect;
  Chunk: string;
  OffsetX: Integer;
  StartIndex: Integer;
  OldFontSize, NewChunkWidth: Integer;
begin
  if FTypedText = '' then Exit;


  Node := AViewInfo.Node;
  Col := AViewInfo.Column;
  CellText := Node.Texts[Col.ItemIndex];


  LowerText := LowerCase(CellText);
  LowerSearch := LowerCase(FTypedText);
  SearchLen := Length(FTypedText);
  StartIndex := 1;

  DrawRect := AViewInfo.BoundsRect;
  ACanvas.FillRect(DrawRect);
  OffsetX := 0;


  while True do
  begin
    FoundPos := PosEx(LowerSearch, LowerText, StartIndex);
    if FoundPos = 0 then
    begin

      Chunk := Copy(CellText, StartIndex, MaxInt);
      ACanvas.Font.Color := clWindowText;
      ACanvas.TextOut(DrawRect.Left + OffsetX, DrawRect.Top, Chunk);
      OffsetX := OffsetX + ACanvas.TextWidth(Chunk);
      Break;
    end
    else
    begin

      Chunk := Copy(CellText, StartIndex, FoundPos - StartIndex);
      ACanvas.Font.Color := clWindowText;
      ACanvas.TextOut(DrawRect.Left + OffsetX, DrawRect.Top, Chunk);
      OffsetX := OffsetX + ACanvas.TextWidth(Chunk);


      Chunk := Copy(CellText, FoundPos, SearchLen);
      OldFontSize := ACanvas.Font.Size;
      ACanvas.Font.Size := OldFontSize + 2;
      ACanvas.Font.Color := clYellow;
      NewChunkWidth := ACanvas.TextWidth(Chunk);
      ACanvas.TextOut(DrawRect.Left + OffsetX, DrawRect.Top, Chunk);
      OffsetX := OffsetX + NewChunkWidth;
      ACanvas.Font.Size := OldFontSize;


      StartIndex := FoundPos + SearchLen;
    end;
  end;

  ADone := True;
end;





procedure TfrmBGPlan.cxTreeFunctionalDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Texts[cxTreeFunctionalCOD_FUNCTIONAL.ItemIndex] + ' : ' +
    ANode.Texts[cxTreeFunctionalDENUMIRE.ItemIndex];
end;

procedure TfrmBGPlan.cxTreeEconomicDESCRIEREGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := ANode.Texts[cxTreeEconomicCOD_ECONOMIC.ItemIndex] + ' : ' +
    ANode.Texts[cxTreeEconomicDENUMIRE.ItemIndex];
end;

procedure TfrmBGPlan.cxSetCurentNode(ATree: TcxDBTreeList);
var lNode: TcxDBTreeListNode;
    lId  : Integer;
begin
  with ATree do begin
    { Luam Id-ul nou creat }
    with DataController.DataSource.DataSet do
      if State in [dsEdit, dsInsert] then begin
         try
            Post;
            lId := FieldByName(DataController.KeyField).AsInteger;
         finally
           ATree.FullRefresh;
         end;
      end
    else lId := FieldByName(DataController.KeyField).AsInteger;

    DataController.DataSource.DataSet.EnableControls;

    lNode := TcxDBTreeListNode(FindNodeByKeyValue(lId, nil));
    if Assigned(lNode) then begin
       { Calculam Clasa }
       lNode.MakeVisible;
       lNode.Focused := True;
       cxUpdateClasa(lNode);
    end;
  end;
end;



procedure TfrmBGPlan.cxMutaInSus(ANode: TcxDBTreeListNode);
var lNode: TcxDBTreeListNode;
    lId,
    lParent: Variant;
begin
  lNode := TcxDBTreeListNode(ANode);
  if (Assigned(lNode)) and ((Assigned(lNode.Parent) and not(lNode.Parent is TcxTreeListRootNode))) then begin
     lId := lNode.KeyValue;
     lParent := TcxDBTreeListNode(lNode.Parent).ParentKeyValue;
     { Modificam Parintele }
     // lNode.MoveTo(lNode.Parent, natlInsert); - nu functioneaza ... nu actualizeaza DataSet-ul
     with TcxDBTreeList(lNode.TreeList) do begin
       if DataController.DataSource.DataSet.FieldByName(DataController.KeyField).AsInteger <> lId then
          DataController.DataSource.DataSet.Locate(DataController.KeyField, lId, []);
       if not (DataController.DataSource.DataSet.State in [dsEdit, dsInsert]) then DataController.DataSource.DataSet.Edit;
       DataController.DataSource.DataSet.FieldByName(DataController.ParentField).Value := lParent;
       if DataController.DataSource.DataSet.State in [dsEdit, dsInsert] then DataController.DataSource.DataSet.Post;
       lNode := TcxDBTreeListNode(FindNodeByKeyValue(lId, nil));
       if Assigned(lNode) then
          cxUpdateClasa(lNode);
     end;
  end;
end;

procedure TfrmBGPlan.edtFiltruBugetPropertiesChange(Sender: TObject);
begin
  if Trim(edtFiltruBuget.EditValue) = '-1' then
      SetFilterOnDataSet(frmData.qryBGPlanFunctional, '')
  else
      SetFilterOnDataSet(frmData.qryBGPlanFunctional,
        'ID_BG_TIPURI_BUGET= ' +   IntToStr(edtFiltruBuget.EditValue));
end;

procedure TfrmBGPlan.FormCloseQuery(Sender: TObject;
  var CanClose: Boolean);
var
  lSave : Boolean;
begin
  if (frmData.qryBGPlanFunctional.State in dsEditModes) or (frmData.qryBGPlanEconomic.State in dsEditModes) then begin
     lSave := (MessageDlg('Ati efectuat modificari asupra clasificatilor. Doriti salvarea modificarilor ?', mtConfirmation, [mbYes, mbNo], 0) = mrYes);
     CanClose := False;
     if lSave then BtnOk.Click
     else BtnCancel.Click;
  end
  else
    SetFilterOnDataSet(frmData.qryBGPlanFunctional, '');
end;

procedure TfrmBGPlan.CmdStergeCapitolExecute(Sender: TObject);
var
  lNode : TcxDBTreeListNode;
  aCont, aParentCont, aClass : String;
begin
  lNode := TcxDBTreeListNode(cxTreeFunctional.FocusedNode);
  if lNode = nil then Exit;
  aCont := lNode.KeyValue;
  aClass := lNode.Texts[cxTreeFunctionalCOD_FUNCTIONAL.ItemIndex];
  if (MessageDlg(Format('Doriti stergerea clasificatiei functionale curente %s ?', [aClass]),  mtConfirmation, [mbYes, mbNo], 0) <> mrYes) then
       Abort;
  TestFunctional(aCont, 'stergerea', True);
  aParentCont := '';
  if (lNode.Parent <> nil) and not (lNode.Parent is TcxTreeListRootNode) then aParentCont := lNode.ParentKeyValue;
  if frmData.qryBGPlanFunctional.Locate('ID_BG_PLAN_FUNCTIONAL', aCont, []) then frmData.qryBGPlanFunctional.Delete;
  frmData.qryBGPlanFunctional.Locate('ID_BG_PLAN_FUNCTIONAL', aParentCont, []);
end;

procedure TfrmBGPlan.CmdStergeTitluExecute(Sender: TObject);
var
  lNode : TcxDBTreeListNode;
  aCont, aParentCont, aClass : String;
begin
  lNode := TcxDBTreeListNode(cxTreeEconomic.FocusedNode);
  if lNode = nil then Exit;
  aCont := lNode.KeyValue;
  aClass := lNode.Texts[cxTreeEconomicCOD_ECONOMIC.ItemIndex];  
  if (MessageDlg(Format('Doriti stergerea clasificatiei economice curente %s ?', [aClass]),  mtConfirmation, [mbYes, mbNo], 0) <> mrYes) then
       Abort;
  TestEconomic(aCont, 'stergerea', True);
  aParentCont := '';
  if (lNode.Parent <> nil) and not (lNode.Parent is TcxTreeListRootNode) then aParentCont := lNode.ParentKeyValue;
  if frmData.qryBGPlanEconomic.Locate('ID_BG_PLAN_ECONOMIC', aCont, []) then frmData.qryBGPlanEconomic.Delete;
  frmData.qryBGPlanEconomic.Locate('ID_BG_PLAN_ECONOMIC', aParentCont, []);
end;

procedure TfrmBGPlan.TestEconomic(aCont, aOperatie: String;
  const inAnalitic: Boolean);
begin
  if ValueSafeToInt( DBGetScallarFmt('exec [sp_EconomicTestUse] %s, %d', [ValueToStr(aCont), Integer(inAnalitic)]) ) <> 0 then begin
    if IsAdmin then begin
      if (MessageDlg(Format('Acest cod este folosit in cadrul aplicatie! Sunteti siguri ca doriti %s codului %s ?', [aOperatie, aCont]),  mtError, [mbYes, mbNo], 0) <> mrYes) then
        Abort;
    end else begin
      ShowEroare('Acest cod este folosit in cadrul aplicatie! Numai administratorul poate sterge un cod folosit !' );
      Abort;
    end;
  end;
end;

procedure TfrmBGPlan.TestFunctional(aCont, aOperatie: String;
  const inAnalitic: Boolean);
begin
  if ValueSafeToInt( DBGetScallarFmt('exec [sp_FunctionalTestUse] %s, %d', [ValueToStr(aCont), Integer(inAnalitic)]) ) <> 0 then begin
    if IsAdmin then begin
      if (MessageDlg(Format('Acest cod este folosit in cadrul aplicatie! Sunteti siguri ca doriti %s codului %s ?', [aOperatie, aCont]),  mtError, [mbYes, mbNo], 0) <> mrYes) then
        Abort;
    end
    else begin
      ShowEroare('Acest cod este folosit in cadrul aplicatie! Numai administratorul poate sterge un cod folosit !' );
      Abort;
    end;
  end;
end;

procedure TfrmBGPlan.cxTreeEconomicDragDrop(Sender, Source: TObject; X,
  Y: Integer);
var
  lId : Integer;
begin
  if (Assigned(Source)) and (Sender = Source) then begin
    lId := TcxDBTreeListNode(TcxDBTreeList(Sender).DragNode).KeyValue;
    PostMessage(Handle, WM_UPDATECLASAEC, lId, lId);
  end;
end;

procedure TfrmBGPlan.WmUpdateClasaEc(var aMessage: TMessage);
var
  lNode : TcxDBTreeListNode;
begin
  lNode :=  TcxDBTreeListNode(cxTreeEconomic.FindNodeByKeyValue(aMessage.LParam, nil));
  if lNode <> nil then
    cxUpdateClasa(lNode);
end;

procedure TfrmBGPlan.WmUpdateClasaFc(var aMessage: TMessage);
var
  lNode : TcxDBTreeListNode;
begin
  lNode :=  TcxDBTreeListNode(cxTreeFunctional.FindNodeByKeyValue(aMessage.LParam, nil));
  if lNode <> nil then cxUpdateClasa(lNode);
end;

procedure TfrmBGPlan.cxTreeFunctionalDragDrop(Sender, Source: TObject; X,
  Y: Integer);
var
  lId : Integer;
begin
  if (Assigned(Source)) and (Sender = Source) then begin
    lId := TcxDBTreeListNode(TcxDBTreeList(Sender).DragNode).KeyValue;
    PostMessage(Handle, WM_UPDATECLASAFC, lId, lId);
  end;
end;

procedure TfrmBGPlan.cxTreeFunctionalGetNodeImageIndex(
  Sender: TcxCustomTreeList; ANode: TcxTreeListNode;
  AIndexType: TcxTreeListImageIndexType; var AIndex: TImageIndex);
begin
  if ANode.Level > 2 then AIndex := 2 else AIndex := ANode.Level;
end;

procedure TfrmBGPlan.cxTreeFunctionalKeyDown(Sender: TObject; var Key: Word; Shift: TShiftState);
begin
  case Key of
    VK_BACK:
      begin
        if FTypedText <> '' then
          Delete(FTypedText, Length(FTypedText), 1);
        Key := 0;
      end;
    VK_ESCAPE:
      begin
        FTypedText := '';
        Key := 0;
      end;
    VK_DECIMAL, VK_OEM_PERIOD:
      begin
        FTypedText := FTypedText + '.';
        Key := 0;
      end;
  else
    if (Key >= 32) and (Key <= 126) then
    begin
      FTypedText := FTypedText + Char(Key);
      Key := 0;
    end;
  end;


 // ShowMessage('FTypedText = ' + FTypedText);

  cxTreeFunctional.Invalidate;
end;



end.
