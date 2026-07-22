unit AlopDisponibil;

interface

uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms, Dialogs, ExtCtrls,
  StdCtrls, cxPC, cxControls, cxContainer, cxEdit, cxGroupBox, cxRepartitorPanel, cxGraphics, cxTL, cxMaskEdit,
  cxDataUtils, cxDataStorage, DB, cxDBData, cxCalendar, cxCurrencyEdit, cxTextEdit, cxGridCustomTableView, cxGridTableView,
  cxGridDBTableView, cxGridLevel, cxClasses, cxGridCustomView, cxGrid, cxInplaceContainer, cxDBTL, cxTLData, cxCheckBox, Menus,
  cxLookAndFeelPainters, cxButtons, ZDataSet, cxProgressBar, ZAbstractRODataset, ZAbstractDataset, cxTLdxBarBuiltInMenu,
  cxLookAndFeels, cxStyles, cxCustomData, cxFilter, cxData, ImgList, ZSqlUpdate, cxDBEdit, cxLabel, Mask, cxDropDownEdit,
  cxImageComboBox, dxBarBuiltInMenu, cxNavigator,
  cxDataControllerConditionalFormattingRulesManagerDialog, dxDateRanges,
  dxScrollbarAnnotations;

const
  AutoCalculProcenti : Boolean = False;

type
  PDateLista = ^TDateLista;
  TDateLista = record
    IdOrd         : Variant;
    IdAng         : Variant;
    CodFunctional : Variant;
    IdUnitate     : Variant;
    CodEconomic   : Variant;
    IdProiect     : Variant;
    ProcAng       : Currency;
    ValProcent    : Currency;
    DescProiect   : Variant;
    Descriere     : String;
  end;

  TfrmAlopDisponibil = class(TForm)
    pageBuget: TcxPageControl;
    tabLegal: TcxTabSheet;
    lbFurnizor: TLabel;
    lbListaPozitiiAngajamente: TLabel;
    Bevel2: TBevel;
    tabGlobal: TcxTabSheet;
    LbClasaFunctionala: TLabel;
    Label1: TLabel;
    Bevel1: TBevel;
    edFurnizor: TcxRepartitorPanel;
    edClasaFunctionala: TcxRepartitorPanel;
    TreeChild: TcxDBTreeList;
    TreeChildClasa: TcxDBTreeListColumn;
    TreeChildCOD_ECONOMIC: TcxDBTreeListColumn;
    TreeChildDENUMIRE: TcxDBTreeListColumn;
    TreeChildASIGNAT: TcxDBTreeListColumn;
    TreeChildPLANIFICAT: TcxDBTreeListColumn;
    TreeChildANGAJAT: TcxDBTreeListColumn;
    TreeChildREALIZAT: TcxDBTreeListColumn;
    TreeChildPROC_ANGAJAT: TcxDBTreeListColumn;
    TreeChildPROC_REALIZAT: TcxDBTreeListColumn;
    GridAngajateDBTableView1: TcxGridDBTableView;
    GridAngajate: TcxGrid;
    GridAngajateL: TcxGridLevel;
    GridAngajateV: TcxGridDBTableView;
    GridAngajateVeste_procentual: TcxGridDBColumn;
    GridAngajateVprocProcent: TcxGridDBColumn;
    GridAngajateVvalFacturare: TcxGridDBColumn;
    GridAngajateVNUMAR: TcxGridDBColumn;
    GridAngajateVDATA_EMITERE: TcxGridDBColumn;
    GridAngajateVCLASA_FUNCTIONALA: TcxGridDBColumn;
    GridAngajateVCOD_ECONOMIC: TcxGridDBColumn;
    GridAngajateVSCOPUL: TcxGridDBColumn;
    GridAngajateVDESCRIERE: TcxGridDBColumn;
    GridAngajateVANGAJAT: TcxGridDBColumn;
    GridAngajateVFACTURAT: TcxGridDBColumn;
    GridAngajateVRAMAS_DE_ANGAJAT: TcxGridDBColumn;
    ChkRepNeFacturat: TcxCheckBox;
    ChkArataDoarPlanificat: TcxCheckBox;
    ChkArataNerealizat: TcxCheckBox;
    TreeRepartitori: TcxDBTreeList;
    TreeRepartitoriNUME: TcxDBTreeListColumn;
    TreeRepartitoriADRESA: TcxDBTreeListColumn;
    TreeRepartitoriCONT: TcxDBTreeListColumn;
    TreeRepartitoriCODFISC: TcxDBTreeListColumn;
    TreeRepartitoriGESTINT: TcxDBTreeListColumn;
    TreeBugete: TcxDBTreeList;
    TreeBugeteNUMAR_RAND: TcxDBTreeListColumn;
    TreeBugeteDENUMIRE: TcxDBTreeListColumn;
    TreeBugeteCOD_BUGET: TcxDBTreeListColumn;
    BtnOk: TcxButton;
    BtnCancel: TcxButton;
    DTAngajamente: TDataSource;
    QryAngajamente: TZReadOnlyQuery;
    DTEconomic: TDataSource;
    QryEconomic: TZReadOnlyQuery;
    GridAngajateVID: TcxGridDBColumn;
    TreeBugeteCOD_ECRAN: TcxDBTreeListColumn;
    TreeBugeteID_ANALITIC: TcxDBTreeListColumn;
    TreeBugetecxDBTreeListColumn1: TcxDBTreeListColumn;
    GridAngajateVID_ANGAJAMENTE_DEFALCARE: TcxGridDBColumn;
    GridAngajateVID_ANALITIC: TcxGridDBColumn;
    GridAngajateVPROC_FACTURAT: TcxGridDBColumn;
    tabOrd: TcxTabSheet;
    Label4: TLabel;
    edFurnizorOrd: TcxRepartitorPanel;
    Label5: TLabel;
    Bevel3: TBevel;
    cxGridOrd: TcxGrid;
    cxGridDBTableView1: TcxGridDBTableView;
    GridOrd: TcxGridDBTableView;
    GridOrdIdAng: TcxGridDBColumn;
    GridOrdNr: TcxGridDBColumn;
    GridOrdData: TcxGridDBColumn;
    GridOrdCOD_FUNCTIONAL: TcxGridDBColumn;
    GridOrdCOD_ECONOMIC: TcxGridDBColumn;
    GridOrdDOCUMENTE: TcxGridDBColumn;
    GridOrdAngajat: TcxGridDBColumn;
    GridOrdOrdonantat: TcxGridDBColumn;
    GridOrdRamas: TcxGridDBColumn;
    GridOrdProcent: TcxGridDBColumn;
    GridOrdIdOrd: TcxGridDBColumn;
    GridOrdIdProiect: TcxGridDBColumn;
    GridOrdL: TcxGridLevel;
    chkOrdNeplatit: TcxCheckBox;
    DTOrdonantari: TDataSource;
    qryOrdonantari: TZReadOnlyQuery;
    TreeChildCOD_FUNCTIONAL: TcxDBTreeListColumn;
    TreeChildID_OI_UNITATI: TcxDBTreeListColumn;
    TreeChildID_OI_PROIECTE: TcxDBTreeListColumn;
    GridAngajateVid_oi_unitati: TcxGridDBColumn;
    GridAngajateVid_oi_proiecte: TcxGridDBColumn;
    GridOrdIdUnitate: TcxGridDBColumn;
    GridAngajateVSel: TcxGridDBColumn;
    GridOrdSel: TcxGridDBColumn;
    TreeChildID: TcxDBTreeListColumn;
    TreeChildCOD_ECONOMIC_ECRAN: TcxDBTreeListColumn;
    GridAngajateVContract_NR: TcxGridDBColumn;
    GridAngajateVContract_Data: TcxGridDBColumn;
    GridOrdContract_NR: TcxGridDBColumn;
    GridOrdContract_DATA: TcxGridDBColumn;
    lbSumaDeFacturat: TLabel;
    edSumaDeFacturat: TcxCurrencyEdit;
    edTipProiect: TcxImageComboBox;
    dtRepartitori: TDataSource;
    qryRepartitori: TZReadOnlyQuery;
    cxButton1: TcxButton;
    procedure edClasaFunctionalaPopupInitPopup(Sender: TObject);
    procedure edClasaFunctionalaValidate(Sender: TObject;
      var AKeyValue: Variant);
    procedure GridAngajateVFocusedRecordChanged(
      Sender: TcxCustomGridTableView; APrevFocusedRecord,
      AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure TreeBugeteNUMAR_RANDGetDisplayText(Sender: TcxTreeListColumn;
      ANode: TcxTreeListNode; var Value: String);
    procedure TreeBugeteDblClick(Sender: TObject);
    procedure TreeBugeteKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure edFurnizorValidate(Sender: TObject; var AKeyValue: Variant);
    procedure TreeChildClasaGetDisplayText(Sender: TcxTreeListColumn;
      ANode: TcxTreeListNode; var Value: String);
    procedure TreeChildFocusedNodeChanged(Sender: TcxCustomTreeList;
      APrevFocusedNode, AFocusedNode: TcxTreeListNode);
    procedure TreeChildDblClick(Sender: TObject);
    procedure BtnOkClick(Sender: TObject);
    procedure BtnCancelClick(Sender: TObject);
    procedure TreeChildCustomDrawCell(Sender: TObject; ACanvas: TcxCanvas;
      AViewInfo: TcxTreeListEditCellViewInfo; var ADone: Boolean);
    procedure ChkRepNeFacturatClick(Sender: TObject);
    procedure ChkArataDoarPlanificatClick(Sender: TObject);
    procedure ChkArataNerealizatClick(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure edFurnizorPopupPopup(Sender: TObject);
    procedure GridAngajateVDblClick(Sender: TObject);
    procedure TreeChildKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure GridAngajateVKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure pageBugetChange(Sender: TObject);
    procedure QryEconomicAfterOpen(DataSet: TDataSet);
    procedure TreeChildCustomDrawDataCell(Sender: TcxCustomTreeList;
      ACanvas: TcxCanvas; AViewInfo: TcxTreeListEditCellViewInfo;
      var ADone: Boolean);
    procedure GridOrdDblClick(Sender: TObject);
    procedure GridOrdFocusedRecordChanged(Sender: TcxCustomGridTableView;
      APrevFocusedRecord, AFocusedRecord: TcxCustomGridRecord;
      ANewItemRecordFocusingChanged: Boolean);
    procedure edFurnizorOrdValidate(Sender: TObject; var AKeyValue: Variant);
    procedure chkOrdNeplatitClick(Sender: TObject);
    procedure QryAngajamenteAfterOpen(DataSet: TDataSet);
    procedure qryOrdonantariAfterOpen(DataSet: TDataSet);
    procedure TreeChildMouseUp(Sender: TObject; Button: TMouseButton;
      Shift: TShiftState; X, Y: Integer);
    procedure TreeChildGetNodeImageIndex(Sender: TcxCustomTreeList;
      ANode: TcxTreeListNode; AIndexType: TcxTreeListImageIndexType;
      var AIndex: TImageIndex);
    procedure FormDestroy(Sender: TObject);
    procedure edFurnizorOrdPopupPopup(Sender: TObject);
    procedure GridAngajateVFocusedItemChanged(
      Sender: TcxCustomGridTableView; APrevFocusedItem,
      AFocusedItem: TcxCustomGridTableItem);
    procedure GridOrdKeyUp(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure cxButton1Click(Sender: TObject);
  private
    FGlobalList         : TStringList;
    FLegalList          : TStringList;
    FOrdList            : TStringList;

    FIdFurnizor         : Variant;
    FCodEconomic        : Variant;
    FLocateCodEconomic  : Variant;
    FCodFunctional      : Variant;
    FIdAngajament       : Variant;
    FIdUnitati          : Variant;
    FDataExecutie       : Variant;
    FIdProiecte         : Variant;
    FIdOrdonantare      : Variant;
    
    FNeedOrdonantare    : Boolean;
    FMultipleSelection  : Boolean;
    
    FDescriere          : String;
    FSumaCautare        : Currency;
    FIdOrdCautare       : Variant;
    FIdAngCautare       : Variant;
    FObiectivCautare    : Integer;
    FAngMissingColumns  : Boolean;
    FOrdMissingColumns  : Boolean;
    FIsOnDocument       : Boolean;
    
    procedure DrawGridProc(ANode: TcxTreeListNode; ARect: TRect; ACanvas: TCanvas; lCurentIndex, lTotalIndex: Integer);
    function  GetDescriere: String;
    procedure SetCodFunctional(const Value: Variant);
    procedure SetCurentBugetFilter;
    procedure SetAngFilter;
    procedure SetOrdFilter;
    function  CanCloseCurent: Boolean;
    procedure CloseCurent(AAccept: Boolean);
    procedure SetIdAngajament(const Value: Variant);
    procedure SetCodEconomic(const Value: Variant);
    procedure SetIdFurnizor(const Value: Variant);
    procedure SetIdUnitati(const Value: Variant);
    procedure SetDataExecutie(const Value: Variant);
    procedure SetIdProiecte(const Value: Variant);
    procedure SetIdOrdonantare(const Value: Variant);
    { Private declarations }
    procedure SetFocusInformation;
    procedure SetMultipleSelection(const Value: Boolean);
    function GetInfoList: TStringList;
    function FocusedDataSet: TDataSet;
  protected
    InKey : Boolean;
    FSumaDeAngajat      : Variant;
    FCulgestItemsID     : Variant;
    FCantitateDeAngajat : Variant;
    function  GetAngajamentDescriptions: String;
    procedure AdaugaAngLegalToList;
    procedure AdaugaAngGlobalToList(ANode: TcxTreeListNode);
    procedure ClearAngLegalFromList(const AIdAngDefalcare: String);
    procedure ClearAngGlobalFromList(ANode: TcxTreeListNode);
    procedure SetGlobalNodeState(GlobalNode: TcxTreeListNode; AState: Integer);
    procedure ClearList(aList : TStringList);
    procedure AngFieldSelChange (Sender:TField);
    procedure OrdSelFieldChange (Sender:TField);
    function  GetProjDetails(const AProjID: Variant): String;
    procedure SetProjDetails(objData: PDateLista);
    function  GetDetaliiPozitie(const ACodFunctional, ACodEconomic, AProiectID: Variant): String;
    procedure SetDetaliiPozitie(const aPrefix: String; ANumber, ADate: Variant; objData: PDateLista);
    procedure LocateByInfo;
  public
    { Public declarations }
    procedure ClearGlobalList;
    procedure ClearLegalList;
    procedure ClearOrdList;
    procedure ClearAllList;

    procedure SetSelectieProcent(const ACulgestItemsID, ASumaDeAngajat, ACantitateDeAngajat: Variant);
    function  GetAngajamenteLegaleXML: String;
    function  SumaDefalcataCorecta: Boolean;
    procedure SalveazaDefalcareProcenti;
    function  CanSelectDefalcare: Boolean;
    procedure DisableEditProcent;
    procedure UpdateSelected;

    function  SilentValidateDesc(ACodEconomic, ACodFunctional, AIdAngajament, AIdUnitati: Variant) : String; overload;
    function  SilentValidateDesc(ACodEconomic, ACodFunctional, AIdAngajament: Variant) : String; overload;
    procedure PrepareCulegere(AIdFurnizor, ACodFunctional, ACodEconomic, AIdAngajament, AIdOrdonantat, AIdUnitati, AIdProiect, ADataExecutie: Variant);

    property  NeedOrdonantare : Boolean read FNeedOrdonantare write FNeedOrdonantare;

    property  MultipleSelection : Boolean read FMultipleSelection write SetMultipleSelection;

    property  InfoList          : TStringList read GetInfoList;
    property  DataExecutie      : Variant   read FDataExecutie    write SetDataExecutie;
    property  IdFurnizor        : Variant   read FIdFurnizor      write SetIdFurnizor;
    property  Descriere         : String    read GetDescriere;


    property  IdOrdonantare     : Variant   read FIdOrdonantare   write SetIdOrdonantare;
    property  IdAngajament      : Variant   read FIdAngajament    write SetIdAngajament;
    property  CodFunctional     : Variant   read FCodFunctional   write SetCodFunctional;
    property  IdUnitati         : Variant   read FIdUnitati       write SetIdUnitati;
    property  CodEconomic       : Variant   read FCodEconomic     write SetCodEconomic;
    property  IdProiecte        : Variant   read FIdProiecte      write SetIdProiecte;
    property  SumaCautare       : Currency  read FSumaCautare     write FSumaCautare;
    property  IsOnDocument      : Boolean   read FIsOnDocument    write FIsOnDocument;
    property  ObiectivCautare   : Integer   read FObiectivCautare write FObiectivCautare;
    
  end;


implementation

uses
  dxCompsUtile, ZeosDBUtile, CommonDBVar, dateUnit, PersistGridSettings, DBClient;

{$R *.dfm}

function TfrmAlopDisponibil.CanCloseCurent: Boolean;
var
  lPrevPos: TBookmark;
  lDataSet: TDataSet;
  lField  : TField;
begin
  { Verificam daca se poate inchide selectia }
  lDataSet  := FocusedDataSet;
  lField    := lDataSet.FindField('Sel');
  Result    := not Assigned(lField);
  if not Result then begin
    lDataSet.DisableControls;
    try
      lPrevPos := lDataSet.GetBookmark;
      try
        lDataSet.First;
        while not lDataSet.Eof do begin
          if lField.AsBoolean then begin
            Result := not Result;
            break;
          end;
          lDataSet.Next;
        end;
      finally
        lDataSet.GotoBookmark(lPrevPos);
        lDataSet.FreeBookmark(lPrevPos);
      end;
    finally
      lDataSet.EnableControls;
    end;
  end;
end;

procedure TfrmAlopDisponibil.ClearAllList;
begin
  ClearGlobalList;
  ClearLegalList;
  ClearOrdList;  
end;

procedure TfrmAlopDisponibil.ClearGlobalList;

  procedure ClearNode(ANode: TcxTreeListNode);
  var
    I: Integer;
  begin
    if ANode.ImageIndex <> 0 then ANode.ImageIndex := 0;
    for I := 0 to ANode.Count-1 do
      ClearNode(ANode.Items[I]);
  end;

begin
  TreeChild.BeginUpdate;
  try
    ClearNode(TreeChild.Root);
    ClearList(FGlobalList);
  finally
    TreeChild.EndUpdate;
  end;
end;

procedure TfrmAlopDisponibil.ClearLegalList;
var
  lLastPoz: TBookMark;
begin
  if QryAngajamente.Active then begin
    QryAngajamente.FieldByName('Sel').OnChange := nil;
    lLastPoz := QryAngajamente.GetBookmark;
    try
      QryAngajamente.DisableControls;
      QryAngajamente.First;
      while not QryAngajamente.Eof do begin
        if QryAngajamente.FieldByName('SEL').AsBoolean then
          DBSetFieldValue(QryAngajamente, 'SEL', False);
        if ValueHasValue(QryAngajamente['valFacturare']) then
          DBSetFieldValue(QryAngajamente, 'valFacturare', 0);
        QryAngajamente.Next;
      end;
    finally
      QryAngajamente.GotoBookmark(lLastPoz);
      QryAngajamente.FreeBookmark(lLastPoz);
      QryAngajamente.EnableControls;
      QryAngajamente.FieldByName('Sel').OnChange := AngFieldSelChange;
    end;
  end;
  ClearList(FLegalList);
end;

procedure TfrmAlopDisponibil.ClearList(aList: TStringList);
begin
   if aList = nil then Exit;
   while aList.Count > 0 do begin
     if Assigned(aList.Objects[aList.Count-1]) then
       Dispose(PDateLista(aList.Objects[aList.Count-1]));
     aList.Delete(aList.Count-1);
   end;
end;

procedure TfrmAlopDisponibil.ClearOrdList;
begin
  if qryOrdonantari.Active then
  try
    qryOrdonantari.DisableControls;
    qryOrdonantari.First;
    while not qryOrdonantari.Eof do begin
      if qryOrdonantari.FieldByName('SEL').AsBoolean then begin
         qryOrdonantari.Edit;
         qryOrdonantari.FieldByName('SEL').AsBoolean := False;
         qryOrdonantari.Post;
      end;
      qryOrdonantari.Next;
    end;
  finally
    qryOrdonantari.EnableControls;
  end;
  ClearList(FOrdList);
end;

procedure TfrmAlopDisponibil.CloseCurent(AAccept: Boolean);
var
  lParentForm: TCustomForm;
begin
  if AAccept then begin
    UpdateSelected;
    ModalResult := mrOk;
  end
  else
    ModalResult := mrCancel;
  lParentForm := GetParentForm(Self);
  if lParentForm <> Self then
    lParentForm.ModalResult := Self.ModalResult;
end;

procedure TfrmAlopDisponibil.cxButton1Click(Sender: TObject);
begin
       try
    QryEconomic.Close;
    QryEconomic.SQL.Text := 'EXEC spAlopExecutieGlobal ' +
                            '@COD_BUGET = ''74.02.05.01'', ' +
                            '@RADACINA = '''', ' +
                            '@DATA = ''2025-02-25'', ' +
                            '@PE_FUNCTIONAL = 1, ' +
                            '@DIVIZOR = 1, ' +
                            '@REVIZIE = NULL, ' +
                            '@gestiune = 64, ' +
                            '@zecimale = 2';
    QryEconomic.Open;
    ShowMessage('Date încărcate cu succes!');
  except
    on E: Exception do
      ShowMessage('Eroare la execuția procedurii: ' + E.Message);
  end;
end;

procedure TfrmAlopDisponibil.DrawGridProc(ANode: TcxTreeListNode;
  ARect: TRect; ACanvas: TCanvas; lCurentIndex, lTotalIndex: Integer);
var lCurent, lTotal: Currency;
begin
  { Facem draw pe o coloana din grid }
  if Trim(ANode.Texts[lCurentIndex]) > '' then lCurent := ANode.Values[lCurentIndex]
  else lCurent := 0;
  if Trim(ANode.Texts[lTotalIndex]) > '' then lTotal := ANode.Values[lTotalIndex]
  else lTotal := 0;
  if lTotal > 0 then DrawProcent(ACanvas, ARect, Trunc(lCurent / lTotal * 10000), clAqua, clNavy)
  else DrawProcent(ACanvas, ARect, 0, clAqua, clNavy);
end;



procedure TfrmAlopDisponibil.edClasaFunctionalaPopupInitPopup(Sender: TObject);
begin
 

  with (Sender as TcxPopupEdit) do
    if Properties.PopupWidth < Width then
      Properties.PopupWidth := Width;
end;






procedure TfrmAlopDisponibil.edClasaFunctionalaValidate(Sender: TObject; var AKeyValue: Variant);
begin
 // ShowMessage('TEST2');

  FIdUnitati := DBGetScallarFmt(
    'select ID_ANALITIC from vBGPlanFunctionalComplet where ID_BG_PLAN_FUNCTIONAL = %s',
    [ValueToStr(edClasaFunctionala.KeyValue)]
  );

//  ShowMessage(Format('Lista incarcata'
//  ,
//    [ValueToStr(edClasaFunctionala.CodValue),
//     ValueToStr(FIdUnitati),
//     VarToStr(FDataExecutie)]));

  if (not QryEconomic.Active)
     or not ValueSameValue(QryEconomic.Params.ParamByName('COD_BUGET').Value, edClasaFunctionala.CodValue)
     or not ValueSameValue(QryEconomic.Params.ParamByName('ID_ANALITIC').Value, FIdUnitati)
     or not ValueSameValue(QryEconomic.Params.ParamByName('DATA').Value, FDataExecutie) then
  begin
    QryEconomic.Params.ParamByName('COD_BUGET').Value   := edClasaFunctionala.CodValue;
    QryEconomic.Params.ParamByName('DATA').Value        := FDataExecutie;
    QryEconomic.Params.ParamByName('ID_ANALITIC').Value := FIdUnitati;
    DBRefresh(QryEconomic);
  end
  else
    QryEconomicAfterOpen(QryEconomic);

 // ShowMessage('TEST3');
end;


function TfrmAlopDisponibil.GetDescriere: String;
var
  lList       : TStringList;
  I           : Integer;
begin
  Result := '';
  lList := InfoList;
  if Assigned(lList) then
    for I := 0 to lList.Count-1 do begin
      if Result > '' then
        Result := Result + ', ';
      Result := Result + PDateLista(lList.Objects[I])^.Descriere;
    end;
end;

function TfrmAlopDisponibil.GetInfoList: TStringList;
begin
  if pageBuget.ActivePage = tabLegal then
    Result := FLegalList
  else if pageBuget.ActivePage = tabOrd then
    Result := FOrdList
  else
    Result := FGlobalList;
end;

procedure TfrmAlopDisponibil.SetCodEconomic(const Value: Variant);
begin
  FCodEconomic       := Value;
  FLocateCodEconomic := Value;
end;

procedure TfrmAlopDisponibil.SetCodFunctional(const Value: Variant);
var
  lNode     : TcxDBTreeListNode;
  lcxColumn : TcxDBTreeListColumn;
  lId       : Variant;
begin
  FCodFunctional := Value;
  { Actualizam si codul curent selectat }
  if ValueHasValue(FCodFunctional) then begin
    lId := Null;
    if ValueHasValue(FIdUnitati) then begin
      lId := DBGetScallarFmt('select ID_BG_PLAN_FUNCTIONAL from vBGPlanFunctionalComplet where COD_FUNCTIONAL = %s and ID_ANALITIC = %s',
             [ValueToStr(FCodFunctional), ValueToStr(FIdUnitati)]);
      if not ValueHasValue(lId) then begin
        lId := DBGetScallarFmt('select ID_BG_PLAN_FUNCTIONAL from vBGPlanFunctionalComplet where COD_FUNCTIONAL = %s and ID_ANALITIC is null',
               [ValueToStr(FCodFunctional)]);
        if DBRecordExists('vBGPlanFunctionalComplet', 'ID_PARINTE', lId) then
          lId := DBGetScallarFmt('select ID_BG_PLAN_FUNCTIONAL from vBGPlanFunctionalComplet where COD_FUNCTIONAL = %s and ID_ANALITIC = %s',
                 [ValueToStr(FCodFunctional), ValueToStr(FIdProiecte)]);
      end;
    end;
    if ValueHasValue(lId) then begin
      //daca este parinte ne oprim
      if DBRecordExists('vBGPlanFunctionalComplet', 'ID_PARINTE', lId) then begin
         edClasaFunctionala.EditInput.EditValue   := FCodFunctional;
         edClasaFunctionala.ListaInput.EditValue  := '';
         QryEconomic.Close;
      end
      else begin
        lNode := TcxDBTreeListNode(TreeBugete.FindNodeByKeyValue(lId,nil));
        if Assigned(lNode) then begin
          lNode.Focused := True;
          edClasaFunctionala.EditInput.Text := lNode.Texts[TreeBugeteCOD_BUGET.ItemIndex];
          edClasaFunctionala.ListaInput.Text := lNode.Texts[TreeBugeteDENUMIRE.ItemIndex];
          edClasaFunctionala.PopupResult := mrOk;
          edClasaFunctionala.InternalCloseUp(nil);
          edClasaFunctionala.PopupResult := mrNone;
          FCodFunctional := Value;
        end;
      end;
    end else begin
      lcxColumn := cxFindColumnByFieldName(TreeBugete, edClasaFunctionala.CodField);
      if (edClasaFunctionala.CodField <> '') and (lcxColumn <> nil)  then begin
        lNode := TcxDBTreeListNode(TreeBugete.FindNodeByText(ValueSafeToStr(FCodFunctional), lcxColumn));
        if Assigned(lNode) then begin
          lNode.Focused := True;
          edClasaFunctionala.EditInput.Text := lNode.Texts[TreeBugeteCOD_BUGET.ItemIndex];
          edClasaFunctionala.ListaInput.Text := lNode.Texts[TreeBugeteDENUMIRE.ItemIndex];
          // edClasaFunctionala.KeyValue := lNode.KeyValue;
          edClasaFunctionala.PopupResult := mrOk;
          edClasaFunctionala.InternalCloseUp(nil);
          edClasaFunctionala.PopupResult := mrNone;
          FCodFunctional := Value;
        end;
      end
      else begin
        edClasaFunctionala.KeyValue := Value;
        lNode := TcxDBTreeListNode(TreeBugete.FindNodeByKeyValue(Value, nil));
        if Assigned(lNode) then begin
           lNode.Focused := True;
           edClasaFunctionala.EditInput.Text := lNode.Texts[TreeBugeteCOD_BUGET.ItemIndex];
           edClasaFunctionala.ListaInput.Text := lNode.Texts[TreeBugeteDENUMIRE.ItemIndex];
        end;
      end;
    end;
  end;
end;

procedure TfrmAlopDisponibil.SetCurentBugetFilter;
var AFilter: String;
begin
  AFilter := '';
  if ChkArataDoarPlanificat.Checked then
     AFilter := 'PLANIFICAT > 0';
  if ChkArataNerealizat.Checked then begin
     if AFilter > '' then AFilter := AFilter + ' AND ';
     AFilter := AFilter + 'RAMAS_DE_REALIZAT > 0';
  end;
  if QryEconomic.Filter <> AFilter then begin
     QryEconomic.Filtered := False;
     QryEconomic.Filter   := AFilter;
     QryEconomic.Filtered := True;
  end;
end;

procedure TfrmAlopDisponibil.SetAngFilter;
var
  AFilter: String;
  RecCount : Integer;  
begin
  AFilter := '';
  RecCount := 0;
  if ChkRepNeFacturat.Checked then
     AFilter := 'RAMAS_DE_ANGAJAT > 0';
  if QryAngajamente.Filter <> AFilter then begin
     QryAngajamente.Filtered := False;
     if QryAngajamente.Active then
       RecCount := QryAngajamente.RecordCount;
     QryAngajamente.Filter   := AFilter;
     QryAngajamente.Filtered := True;
     if QryAngajamente.Active and (QryAngajamente.RecordCount = 0) and (RecCount > 0) then
     begin
       ChkRepNeFacturat.Checked := False;
       SetAngFilter;
     end;     
  end;
end;

procedure TfrmAlopDisponibil.SetDataExecutie(const Value: Variant);
begin
  FDataExecutie := Value;
end;

procedure TfrmAlopDisponibil.SetFocusInformation;
begin
  if pageBuget.ActivePage = tabLegal then begin
    GridAngajateVFocusedRecordChanged(GridAngajateV, nil, GridAngajateV.Controller.FocusedRecord, False);
  end
  else if pageBuget.ActivePage = tabGlobal then begin
    TreeChildFocusedNodeChanged(TreeChild, nil, TreeChild.FocusedNode);
  end
  else if pageBuget.ActivePage = tabOrd then begin
    GridOrdFocusedRecordChanged(GridOrd, nil, GridOrd.Controller.FocusedRecord, True);
  end;
end;

procedure TfrmAlopDisponibil.SetIdAngajament(const Value: Variant);
begin
  FIdAngajament := Value;
  FIdAngCautare := Value;
end;

procedure TfrmAlopDisponibil.SetIdFurnizor(const Value: Variant);
var
  lNode: TcxDBTreeListNode;
begin
  FIdFurnizor := Value;
  edFurnizor.KeyValue := Value;
  edFurnizorOrd.KeyValue := Value;

  if (TreeRepartitori <> nil) and (TreeRepartitori.DataController.DataSource = nil) then begin
    TreeRepartitori.DataController.DataSource := dtRepartitori;
    dtRepartitori.DataSet := DBNewQueryFmt('exec [spGetUserRepartitori] %s, 4, %s', [ValueToStr(iUserID), ValueToStr('1')]);
  end;
  lNode := TcxDBTreeListNode(TreeRepartitori.FindNodeByKeyValue(Value, nil));
  if Assigned(lNode) then begin
     edFurnizor.TextEdit.Text := lNode.Texts[TreeRepartitoriCODFISC.ItemIndex];
     edFurnizor.ListaInput.Text := lNode.Texts[TreeRepartitoriNUME.ItemIndex];
     edFurnizorOrd.TextEdit.Text := lNode.Texts[TreeRepartitoriCODFISC.ItemIndex];
     edFurnizorOrd.ListaInput.Text := lNode.Texts[TreeRepartitoriNUME.ItemIndex];
  end;

  {decidem ce tab sa deschidem implicit}
  if ((GridOrd.DataController.DataSet <> nil)
      and (GridOrd.DataController.DataSet.RecordCount > 0))
      and tabOrd.TabVisible
  then pageBuget.ActivePage := tabOrd
  else begin
    if (GridAngajateV.DataController.DataSet <> nil)
        and (GridAngajateV.DataController.DataSet.RecordCount > 0)
        and tabLegal.TabVisible
    then pageBuget.ActivePage := tabLegal
    else pageBuget.ActivePage := tabGlobal;
  end;
end;

procedure TfrmAlopDisponibil.SetIdOrdonantare(const Value: Variant);
begin
  FIdOrdonantare  := Value;
  FIdOrdCautare   := Value;
end;

procedure TfrmAlopDisponibil.SetIdProiecte(const Value: Variant);
begin
  if (ValueSafeToInt(Value) = 0) or (ValueSafeToInt(Value) = -1) then
    FIdProiecte := Null
  else
    FIdProiecte := Value;
end;

function TfrmAlopDisponibil.SilentValidateDesc(ACodEconomic, ACodFunctional, AIdAngajament, AIdUnitati: Variant): String;
begin
  FIdUnitati      := AIdUnitati;
  FCodEconomic    := ACodEconomic;
  FCodFunctional  := ACodFunctional;
  IdAngajament    := AIdAngajament;
  Result          := Descriere;
end;

procedure TfrmAlopDisponibil.GridAngajateVFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord;
  ANewItemRecordFocusingChanged: Boolean);
begin
  GridAngajateVvalFacturare.Options.Editing := False;
  if Assigned(AFocusedRecord) and (AFocusedRecord.IsData) then begin
    FIdOrdonantare  := Null;
    FIdAngajament   := AFocusedRecord.Values[GridAngajateVID.Index];
    FCodFunctional  := AFocusedRecord.Values[GridAngajateVCLASA_FUNCTIONALA.Index];
    FIdUnitati      := AFocusedRecord.Values[GridAngajateVid_oi_unitati.Index];
    FCodEconomic    := AFocusedRecord.Values[GridAngajateVCOD_ECONOMIC.Index];
    FIdProiecte     := AFocusedRecord.Values[GridAngajateVid_oi_proiecte.Index];
    FDescriere      := '';//AFocusedRecord.DisplayTexts[GridAngajateVNUMAR.Index] + ' - ' + AFocusedRecord.DisplayTexts[GridAngajateVDATA_EMITERE.Index] + ' ';
    if ValueIsTrue(AFocusedRecord.Values[GridAngajateVSel.Index]) then begin
      GridAngajateVvalFacturare.Options.Editing := True;
    end;
  end;
end;

procedure TfrmAlopDisponibil.TreeBugeteNUMAR_RANDGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := Trim(ANode.Texts[TreeBugeteCOD_ECRAN.ItemIndex]) + ' : '+ Trim(ANode.Texts[TreeBugeteDENUMIRE.ItemIndex]);
end;

procedure TfrmAlopDisponibil.TreeBugeteDblClick(Sender: TObject);
begin
   with TcxDBTreeList(Sender) do
    if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
   GetParentForm(TcxDBTreeList(Sender)).ModalResult := mrOk;
end;

procedure TfrmAlopDisponibil.TreeBugeteKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
     TreeBugeteDblClick(TcxDBTreeList(Sender))
  else if Key = VK_ESCAPE then
   GetParentForm(TcxDBTreeList(Sender)).ModalResult := mrCancel;
end;

procedure TfrmAlopDisponibil.edFurnizorValidate(Sender: TObject;
  var AKeyValue: Variant);
begin
  //fortam refresh pe ordonantari
//  if (not QryAngajamente.Active)or (QryAngajamente.Params.ParamByName('ID_REPARTITOR').Value <> AKeyValue) then
  begin
    QryAngajamente.Close;
    QryAngajamente.Params.ParamByName('DATA').Value               := FDataExecutie;
    QryAngajamente.Params.ParamByName('ID_REPARTITOR').Value      := AKeyValue;
    QryAngajamente.Params.ParamByName('ID_CULGEST_ITEMSI').Value  := FCulgestItemsID;
    QryAngajamente.Open;
  end;
//  if Self.Visible then GridAngajate.SetFocus;
end;

procedure TfrmAlopDisponibil.TreeChildClasaGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: String);
begin
  Value := Trim(ANode.Texts[TreeChildCOD_ECONOMIC_ECRAN.ItemIndex])+' : '+Trim(ANode.Texts[TreeChildDENUMIRE.ItemIndex]);
end;

procedure TfrmAlopDisponibil.TreeChildFocusedNodeChanged(Sender: TcxCustomTreeList;
  APrevFocusedNode, AFocusedNode: TcxTreeListNode);
begin
  if not MultipleSelection and Assigned(AFocusedNode) then begin
     FIdAngajament   := Null;
     FIdOrdonantare  := Null;
     FCodFunctional  := AFocusedNode.Values[TreeChildCOD_FUNCTIONAL.ItemIndex];
     FIdUnitati      := AFocusedNode.Values[TreeChildID_OI_UNITATI.ItemIndex];
     FCodEconomic    := AFocusedNode.Values[TreeChildCOD_ECONOMIC.ItemIndex];
     FIdProiecte     := AFocusedNode.Values[TreeChildID_OI_PROIECTE.ItemIndex];
  end;
end;

procedure TfrmAlopDisponibil.TreeChildGetNodeImageIndex(
  Sender: TcxCustomTreeList; ANode: TcxTreeListNode;
  AIndexType: TcxTreeListImageIndexType; var AIndex: TImageIndex);
begin
  if AIndexType in [tlitStateIndex, tlitOverlayStateIndex] then
    AIndex := ANode.ImageIndex;
end;
    //modif seb
procedure TfrmAlopDisponibil.TreeChildDblClick(Sender: TObject);
var
  lNode: TcxTreeListNode;
begin
  lNode := TreeChild.FocusedNode;
  if Assigned(lNode) and not lNode.HasChildren then
  begin
    if MultipleSelection then
    begin
      InKey := true;
      TreeChildMouseUp(Sender, mbLeft, [], 0, 0);
    end
    else
    begin

      ClearGlobalList;
      AdaugaAngGlobalToList(lNode);
      BtnOkClick(nil);
    end;
  end;
end;
//



procedure TfrmAlopDisponibil.BtnOkClick(Sender: TObject);
begin
  if CanCloseCurent then
    CloseCurent(True);
end;

procedure TfrmAlopDisponibil.AngFieldSelChange(Sender: TField);
begin
  //in funtie de camp adaugam sau stergem din lista
  if Sender.AsBoolean then begin
    if not FMultipleSelection then begin
      ClearLegalList;
      if ValueHasValue(QryAngajamente['id_oi_proiecte']) then begin
        if ValueHasValue(QryAngajamente['procProcent']) and not ValueHasValue(QryAngajamente['valFacturare']) then
          QryAngajamente['valFacturare'] := ValueSafeToCurrency(FSumaDeAngajat) * ValueSafeToCurrency(QryAngajamente['procProcent']) / 100.00;
      end;
    end;
    AdaugaAngLegalToList;
  end
  else begin
    ClearAngLegalFromList(QryAngajamente.FieldByName('ID_ANGAJAMENTE_DEFALCARE').AsString);
  end;
  GridAngajateVFocusedRecordChanged(GridAngajateV, nil, GridAngajateV.Controller.FocusedRecord, False);
end;

procedure TfrmAlopDisponibil.BtnCancelClick(Sender: TObject);
begin
  CloseCurent(False);
end;

procedure TfrmAlopDisponibil.TreeChildCustomDrawCell(Sender: TObject;
  ACanvas: TcxCanvas; AViewInfo: TcxTreeListEditCellViewInfo;
  var ADone: Boolean);
begin
  if AViewInfo.Column = TreeChildPROC_ANGAJAT then begin
    DrawGridProc(AViewInfo.Node, AViewInfo.BoundsRect, ACanvas.Canvas, TreeChildANGAJAT.ItemIndex, TreeChildPLANIFICAT.ItemIndex);
    ADone := True;
  end
  else
  if AViewInfo.Column = TreeChildPROC_REALIZAT then begin
    DrawGridProc(AViewInfo.Node, AViewInfo.BoundsRect, ACanvas.Canvas, TreeChildREALIZAT.ItemIndex, TreeChildPLANIFICAT.ItemIndex);
    ADone := True;
  end
end;

procedure TfrmAlopDisponibil.ChkRepNeFacturatClick(Sender: TObject);
begin
  SetAngFilter;
end;

procedure TfrmAlopDisponibil.ChkArataDoarPlanificatClick(Sender: TObject);
begin
  SetCurentBugetFilter;
end;

procedure TfrmAlopDisponibil.ChkArataNerealizatClick(Sender: TObject);
begin
  SetCurentBugetFilter;
end;

procedure TfrmAlopDisponibil.chkOrdNeplatitClick(Sender: TObject);
begin
  SetOrdFilter;
end;

procedure TfrmAlopDisponibil.FormCreate(Sender: TObject);

  function NewList: TStringList;
  begin
    Result := TStringList.Create;
    Result.Duplicates     := dupIgnore;
    Result.Sorted         := True;
    Result.CaseSensitive  := False;
  end;

begin
  FDescriere          :='';
  FIsOnDocument       := False;
  MultipleSelection   := True;
  FNeedOrdonantare    := False;
  FGlobalList         := NewList;
  FLegalList          := NewList;
  FOrdList            := NewList;
  FIdUnitati          := Null;
  FIdOrdonantare      := Null;
  FIdAngajament       := Null;
  FDataExecutie       := Null;
  FAngMissingColumns  := True;
  FOrdMissingColumns  := True;
   TreeBugete.DataController.DataSource := frmData.DTBGPlanFunctionalComplet;

  edClasaFunctionala.ValidateEditText := True;
  edClasaFunctionala.OnlySelectChild := True;

  GridAngajateV.RestoreFromStorage(Self.Name + '.' + GridAngajateV.Name, TcxDBIniFileReader);
  GridOrd.RestoreFromStorage(Self.Name + '.' + GridOrd.Name, TcxDBIniFileReader);

  qryRepartitori.ParamByName('userID').AsInteger := iUserID;

  FillImageCombo(edTipProiect.Properties, 'select id_oi_proiecte, coalesce(rtrim(ltrim(cod_proiect)) + '' : '', '''') + rtrim(ltrim(denumire)) from oi_proiecte', 0, 1);
end;

procedure TfrmAlopDisponibil.FormDestroy(Sender: TObject);
begin

  GridAngajateV.StoreToStorage(Self.Name + '.' + GridAngajateV.Name, TcxDBIniFileWriter);
  GridOrd.StoreToStorage(Self.Name + '.' + GridOrd.Name, TcxDBIniFileWriter);

  FGlobalList.Free;
  FLegalList.Free;
  FOrdList.Free;
end;

procedure TfrmAlopDisponibil.edFurnizorOrdPopupPopup(Sender: TObject);
var
  lNode: TcxDBTreeListNode;
  lIdRep : Variant;
begin
  lIdRep := edFurnizorOrd.KeyValue;
  if lIdRep = null then Exit;
  lNode := TcxDBTreeListNode(TreeRepartitori.FindNodeByKeyValue(lIdRep, nil));
  if Assigned(lNode) then begin
    lNode.MakeVisible;
    lNode.Focused := True;
  end;
end;

procedure TfrmAlopDisponibil.edFurnizorOrdValidate(Sender: TObject;
  var AKeyValue: Variant);
begin
    // fortam refresh pe ordonantari
// if (not qryOrdonantari.Active)  or (qryOrdonantari.Params.ParamByName('ID_REPARTITOR').Value <> AKeyValue) then
  begin
    qryOrdonantari.Close;
    qryOrdonantari.Params.ParamByName('DATA').Value           := FDataExecutie;
    qryOrdonantari.Params.ParamByName('ID_REPARTITOR').Value  := AKeyValue;
    qryOrdonantari.Open;
  end;
end;

procedure TfrmAlopDisponibil.edFurnizorPopupPopup(Sender: TObject);
var
  lNode: TcxDBTreeListNode;
  lIdRep : Variant;
begin
  lIdRep := edFurnizor.KeyValue;
  if lIdRep = null then Exit;
  lNode := TcxDBTreeListNode(TreeRepartitori.FindNodeByKeyValue(lIdRep, nil));
  if Assigned(lNode) then begin
    lNode.MakeVisible;
    lNode.Focused := True;
  end;
end;

procedure TfrmAlopDisponibil.GridAngajateVDblClick(Sender: TObject);
begin
  if QryAngajamente.Active and not QryAngajamente.IsEmpty then begin
    DBSetFieldValue(QryAngajamente, 'SEL', True);
    if not MultipleSelection then
      BtnOkClick(nil);
  end;
end;

procedure TfrmAlopDisponibil.SetIdUnitati(const Value: Variant);
begin
  if (ValueSafeToInt(Value) = 0) or (ValueSafeToInt(Value) = -1) then
    FIdUnitati := Null
  else
    FIdUnitati := Value;
end;

procedure TfrmAlopDisponibil.SetMultipleSelection(const Value: Boolean);
begin
  FMultipleSelection := Value;
  {pt griduri mergem pe idea de dataset pt ca nu avem trei stari}
  GridAngajateVSel.Visible := FMultipleSelection;
  GridOrdSel.Visible := FMultipleSelection;
  ClearAllList;
  {pentru tree merge pe state image pentru ca ne permite filtrarea numai a copiilor}
  TreeChild.OptionsSelection.MultiSelect := Value;
end;

procedure TfrmAlopDisponibil.SetOrdFilter;
var
  AFilter: String;
  RecCount : Integer;
begin
  AFilter := '';
  RecCount := 0;
  if chkOrdNeplatit.Checked then
     AFilter := 'RAMAS_DE_ORDONANTAT > 0';
  if qryOrdonantari.Filter <> AFilter then begin
     qryOrdonantari.Filtered := False;
     if qryOrdonantari.Active then
       RecCount := qryOrdonantari.RecordCount;
     qryOrdonantari.Filter   := AFilter;
     qryOrdonantari.Filtered := True;
     if qryOrdonantari.Active and (qryOrdonantari.RecordCount = 0) and (RecCount > 0) then
     begin
       chkOrdNeplatit.Checked := False;
       SetOrdFilter;
     end;
  end;
end;

procedure TfrmAlopDisponibil.TreeChildKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if not MultipleSelection then begin
    if Key = VK_RETURN then
       TreeChildDblClick(TcxDBTreeList(Sender));
  end else begin
  if Assigned(TcxDBTreeList(Sender).OnMouseUp) and (Key = VK_SPACE) then begin
    InKey := True;
    TreeChildMouseUp(Sender, mbLeft, Shift, 0, 0);
  end;
  if (Key = VK_RETURN) and (TcxDBTreeList(Sender).FocusedNode <> nil)
     and (not TcxDBTreeList(Sender).FocusedNode.HasChildren) then
  begin
     if Assigned(TcxDBTreeList(Sender).OnMouseUp) and  (FGlobalList.Count = 0) then begin
       InKey := True;
       TreeChildMouseUp(Sender, mbLeft, Shift, 0, 0);
     end;
     BtnOkClick(nil);
  end;
  end;
end;

procedure TfrmAlopDisponibil.TreeChildMouseUp(Sender: TObject;
  Button: TMouseButton; Shift: TShiftState; X, Y: Integer);
var
  lInfo         : TcxTreeListHitTest;
  lCurentState  : Integer;
  lNode         : TcxTreeListNode;
begin
  lInfo := TreeChild.HitTest;
  if lInfo.HitAtStateImage or InKey then begin
    if not MultipleSelection then ClearGlobalList;
    if InKey then
      lNode := TreeChild.FocusedNode
    else
      lNode := lInfo.HitNode;
    if Assigned(lNode) then begin
      if lNode.ImageIndex = 1 then
        lCurentState := 2
      else
        lCurentState := 1;
      SetGlobalNodeState(lNode, lCurentState);
    end;
    InKey := False;
  end;
end;

procedure TfrmAlopDisponibil.GridAngajateVKeyDown(Sender: TObject;
  var Key: Word; Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;
  if Key = VK_RETURN then
     GridAngajateVDblClick(nil);
end;

procedure TfrmAlopDisponibil.GridOrdDblClick(Sender: TObject);
begin
  if MultipleSelection then begin
    if qryOrdonantari.Active and not qryOrdonantari.IsEmpty then begin
      DBSetFieldValue(qryOrdonantari, 'SEL', True);
      BtnOkClick(nil);
    end;
  end
  else begin
    if Assigned(GridOrd.Controller.FocusedItem) then
     BtnOkClick(nil);
  end;
end;

procedure TfrmAlopDisponibil.GridOrdFocusedRecordChanged(
  Sender: TcxCustomGridTableView; APrevFocusedRecord,
  AFocusedRecord: TcxCustomGridRecord; ANewItemRecordFocusingChanged: Boolean);
begin
  if Assigned(AFocusedRecord) and (AFocusedRecord.IsData) then begin
     FIdAngajament   := AFocusedRecord.Values[GridOrdIdAng.Index];
     FIdOrdonantare  := AFocusedRecord.Values[GridOrdIdOrd.Index];
     FCodFunctional  := AFocusedRecord.Values[GridOrdCOD_FUNCTIONAL.Index];
     FIdUnitati      := AFocusedRecord.Values[GridOrdIdUnitate.Index];
     FCodEconomic    := AFocusedRecord.Values[GridOrdCOD_ECONOMIC.Index];
     FIdProiecte     := AFocusedRecord.Values[GridOrdIdProiect.Index];
     FDescriere      := '';//AFocusedRecord.DisplayTexts[GridOrdNr.Index] + ' - ' + AFocusedRecord.DisplayTexts[GridOrdData.Index]+ ' '; 
  end;
end;

procedure TfrmAlopDisponibil.LocateByInfo;
begin
  GridAngajateV.DataController.Filter.Root.Clear;
  GridAngajateV.DataController.Filter.Active := False;
  GridOrd.DataController.Filter.Active := False;
  GridOrd.DataController.Filter.Root.Clear;
  if pageBuget.ActivePage = tabLegal then begin
    //incercam pozitionarea pe idAngajament
    if not QryAngajamente.Locate('ID_ANGAJAMENTE_DEFALCARE', FIdAngCautare, []) then
        //apoi dupa suma
        if QryAngajamente.Locate('ID_OI_PROIECTE', FObiectivCautare, []) then begin
          GridAngajateV.DataController.Filter.Root.Clear;
          GridAngajateV.DataController.Filter.Root.AddItem(GridAngajateVid_oi_proiecte, foEqual, FObiectivCautare, 'Obiectiv');
          GridAngajateV.DataController.Filter.Active := True;
        end
        else
          QryAngajamente.Locate('angajat', FSumaCautare, []);
    //facem refresh
  end
  else if pageBuget.ActivePage = tabGlobal then begin
  end
  else if pageBuget.ActivePage = tabOrd then begin
    //incercam pozitionarea pe idOrdonantare
    if not qryOrdonantari.Locate('ID_ORDONANTARE_DEFALCARE', FIdOrdCautare, []) then
    //incercam pozitionarea pe idAngajament
      if not qryOrdonantari.Locate('ID_ANGAJAMENTE_DEFALCARE', FIdAngCautare, []) then
        //apoi dupa suma
        if qryOrdonantari.Locate('ID_OI_PROIECTE', FObiectivCautare, []) then begin
          GridOrd.DataController.Filter.Root.Clear;
          GridOrd.DataController.Filter.Root.AddItem(GridOrdIdProiect, foEqual, FObiectivCautare, 'Obiectiv');
          GridOrd.DataController.Filter.Active := True;
        end
        else
        qryOrdonantari.Locate('angajat', FSumaCautare, []);
  end;
end;

procedure TfrmAlopDisponibil.OrdSelFieldChange(Sender: TField);
var
  objData   : PDateLista;
  lOrdString: String;
  lIndex    : Integer;
begin
  //in funtie de camp adaugam sau stergem din lista
  lOrdString := Sender.DataSet.FieldByName('ID_ORDONANTARE_DEFALCARE').AsString;
  lIndex     := FOrdList.IndexOf(lOrdString);
  if Sender.AsBoolean then begin
    if lIndex = -1 then begin
      New(objData);
      objData^.IdOrd          := Sender.DataSet['ID_ORDONANTARE_DEFALCARE'];
      objData^.IdAng          := Sender.DataSet['ID_ANGAJAMENTE_DEFALCARE'];
      objData^.CodFunctional  := Sender.DataSet['COD_FUNCTIONAL'];
      objData^.IdUnitate      := Sender.DataSet['ID_OI_UNITATI'];
      objData^.CodEconomic    := Sender.DataSet['COD_ECONOMIC'];
      objData^.IdProiect      := Sender.DataSet['ID_OI_PROIECTE'];
      SetProjDetails(objData);
      SetDetaliiPozitie('ORD', Sender.DataSet['NUMAR'], Sender.DataSet['DATA_EMITERE'], objData);
      FOrdList.AddObject(lOrdString, TObject(objData));
    end;
  end
  else begin
    if lIndex > -1 then begin
      if Assigned(FOrdList.Objects[lIndex]) then
        Dispose(PDateLista(FOrdList.Objects[lIndex]));
      FOrdList.Delete(lIndex);
    end;
  end;
end;

procedure TfrmAlopDisponibil.pageBugetChange(Sender: TObject);
begin
  LocateByInfo;
  SetFocusInformation;
end;

procedure TfrmAlopDisponibil.PrepareCulegere(AIdFurnizor, ACodFunctional, ACodEconomic, AIdAngajament, AIdOrdonantat, AIdUnitati, AIdProiect, ADataExecutie: Variant);

    function ClearValue(const AValue: Variant): Variant;
    begin
      if (ValueSafeToInt(AValue) = 0) or (ValueSafeToInt(AValue) = -1) then
        Result := Null
      else
        Result := AValue;
    end;
    
var
  lNode : TcxTreeListNode;

begin
  ClearAllList;
  if ValueHasValue(ACodFunctional) or ValueHasValue(ACodEconomic) then begin
    IdUnitati   := ClearValue(AIdUnitati);
    IdProiecte  := ClearValue(AIdProiect);
  end;

  FLocateCodEconomic  := ACodEconomic;
  DataExecutie        := ADataExecutie;
  CodFunctional       := ACodFunctional;
  CodEconomic         := ACodEconomic;
  IdFurnizor          := ClearValue(AIdFurnizor);
  IdAngajament        := ClearValue(AIdAngajament);
  IdOrdonantare       := ClearValue(AIdOrdonantat);

  if ValueHasValue(IdOrdonantare) then
    pageBuget.ActivePage := tabOrd
  else
  if ValueHasValue(IdAngajament) then
    pageBuget.ActivePage := tabLegal
  else
    pageBuget.ActivePage := tabGlobal;

  if ValueHasValue(FCodEconomic) and QryEconomic.Active and QryEconomic.Locate('COD_ECONOMIC;ID_OI_PROIECTE', VarArrayOf([FCodEconomic, FIdProiecte]), []) then begin
    lNode := TreeChild.FindNodeByKeyValue(qryEconomic['ID']);
    if Assigned(lNode) then begin
      lNode.Focused     := True;
      lNode.MakeVisible;
      SetGlobalNodeState(lNode, 1);
    end;
  end;
  
  LocateByInfo;
end;

procedure TfrmAlopDisponibil.QryAngajamenteAfterOpen(DataSet: TDataSet);
var
  lField: TField;
begin
  lField := DataSet.FieldByName('SEL');
  lField.ReadOnly := False;
  lField.OnChange := AngFieldSelChange;
  SetAngFilter;
  if FAngMissingColumns then begin
    cxCreateMissingColumns(DataSet, GridAngajateV);
    FAngMissingColumns := False;
  end;
  { Incaram lista deja selectata }
  ClearList(FLegalList);
  DataSet.DisableControls;
  DataSet.First;
  while not DataSet.Eof do begin
    if lField.AsBoolean then
      AdaugaAngLegalToList;
    DataSet.Next
  end;
  DataSet.EnableControls;
  lField := DataSet.FieldByName('valFacturare');
  lField.ReadOnly := False;
end;

procedure TfrmAlopDisponibil.QryEconomicAfterOpen(DataSet: TDataSet);
var
  lNode : TcxTreeListNode;
begin
  if ValueHasValue(FLocateCodEconomic) then begin
    lNode := TreeChild.FindNodeByText(ValueSafeToStr(FLocateCodEconomic), TreeChildCOD_ECONOMIC);
    if Assigned(lNode) then begin
      lNode.Focused := True;
      lNode.MakeVisible;
    end;
  end;
  ClearGlobalList;
  TreeChild.ApplyBestFit;
end;

procedure TfrmAlopDisponibil.qryOrdonantariAfterOpen(DataSet: TDataSet);
begin
  DataSet.FieldByName('SEL').ReadOnly := False;
  DataSet.FieldByName('SEL').OnChange := OrdSelFieldChange;
  SetOrdFilter;
  if FOrdMissingColumns then begin
    cxCreateMissingColumns(DataSet, GridOrd);
    FOrdMissingColumns := False;
  end;
end;

procedure TfrmAlopDisponibil.TreeChildCustomDrawDataCell(
  Sender: TcxCustomTreeList; ACanvas: TcxCanvas;
  AViewInfo: TcxTreeListEditCellViewInfo; var ADone: Boolean);
begin
  if AViewInfo.Column = TreeChildPROC_ANGAJAT then begin
    DrawGridProc(AViewInfo.Node, AViewInfo.BoundsRect, ACanvas.Canvas, TreeChildANGAJAT.ItemIndex, TreeChildPLANIFICAT.ItemIndex);
    ADone := True;
  end
  else
  if AViewInfo.Column = TreeChildPROC_REALIZAT then begin
    DrawGridProc(AViewInfo.Node, AViewInfo.BoundsRect, ACanvas.Canvas, TreeChildREALIZAT.ItemIndex, TreeChildPLANIFICAT.ItemIndex);
    ADone := True;
  end
end;

procedure TfrmAlopDisponibil.SetProjDetails(objData: PDateLista);
begin
  objData^.DescProiect := GetProjDetails(objData^.IdProiect);
end;

procedure TfrmAlopDisponibil.SalveazaDefalcareProcenti;
begin
  { Setam valorile initiale pentru id_oi_proiecte conform procentilor din partea de angajare }
  DBExecSQLFmt('exec [spTCVSalveazaSurseCulGest] %d, %d, %s', [IdLogin, IdUtilizator, ValueToStr(GetAngajamenteLegaleXML)]);
  UpdateSelected;
end;

procedure TfrmAlopDisponibil.AdaugaAngLegalToList;
var
  lObjData: PDateLista;
  lValue  : String;
begin
  lValue := QryAngajamente.FieldByName('ID_ANGAJAMENTE_DEFALCARE').AsString;
  if FLegalList.IndexOf(lValue) = -1 then begin
    New(lObjData);
    lObjData^.IdOrd          := Null;
    lObjData^.IdAng          := QryAngajamente['ID_ANGAJAMENTE_DEFALCARE'];
    lObjData^.CodFunctional  := QryAngajamente['COD_FUNCTIONAL'];
    lObjData^.IdUnitate      := QryAngajamente['ID_OI_UNITATI'];
    lObjData^.CodEconomic    := QryAngajamente['COD_ECONOMIC'];
    lObjData^.IdProiect      := QryAngajamente['ID_OI_PROIECTE'];
    lObjData^.ProcAng        := QryAngajamente.FieldByName('procProcent').AsCurrency;
    lObjData^.ValProcent     := QryAngajamente.FieldByName('valFacturare').AsCurrency;
    SetProjDetails(lObjData);
    SetDetaliiPozitie('AL', QryAngajamente['NUMAR'], QryAngajamente['DATA_EMITERE'], lObjData);
    FLegalList.AddObject(lValue, TObject(lObjData));
  end;
end;

procedure TfrmAlopDisponibil.SetSelectieProcent(const ACulgestItemsID, ASumaDeAngajat, ACantitateDeAngajat: Variant);
begin
  FCulgestItemsID             := ACulgestItemsID;
  FSumaDeAngajat              := ASumaDeAngajat;
  FCantitateDeAngajat         := ACantitateDeAngajat;
  edSumaDeFacturat.EditValue  := FSumaDeAngajat;
  lbSumaDeFacturat.Visible    := ValueSafeToCurrency(FSumaDeAngajat) > 0;
  edSumaDeFacturat.Visible    := lbSumaDeFacturat.Visible;
end;

procedure TfrmAlopDisponibil.ClearAngLegalFromList(
  const AIdAngDefalcare: String);
var
  lIndex: Integer;
begin
  lIndex := FLegalList.IndexOf(QryAngajamente.FieldByName('ID_ANGAJAMENTE_DEFALCARE').AsString);
  if lIndex > -1 then begin
    if Assigned(FLegalList.Objects[lIndex]) then
      Dispose(PDateLista(FLegalList.Objects[lIndex]));
    FLegalList.Delete(lIndex);
  end;
end;

procedure TfrmAlopDisponibil.DisableEditProcent;
begin
  GridAngajateVvalFacturare.Options.Editing := False;
end;

procedure TfrmAlopDisponibil.GridAngajateVFocusedItemChanged(
  Sender: TcxCustomGridTableView; APrevFocusedItem,
  AFocusedItem: TcxCustomGridTableItem);
begin
  GridAngajateV.OptionsBehavior.IncSearch := AFocusedItem <> GridAngajateVvalFacturare;
end;

function TfrmAlopDisponibil.GetAngajamentDescriptions: String;
begin
  if QryAngajamente.Active and not QryAngajamente.IsEmpty then
    Result := QryAngajamente.FieldByName('NUMAR').AsString + ' - '
              + FormatDateTime('dd/MM/yyyy', QryAngajamente.FieldByName('DATA_EMITERE').AsDateTime) + ' : '
              + QryAngajamente.FieldByName('cod_functional').AsString + '/' + QryAngajamente.FieldByName('cod_economic').AsString + ' : '
  else
    Result := '';
end;

function TfrmAlopDisponibil.GetAngajamenteLegaleXML: String;
var
  lXmlData : String;
  lDataSet : TDataSet;
  lSelField: TField;

    function FieldToXML(const AFieldName: String): String;
    var
      lField: TField;
    begin
      lField := lDataSet.FindField(AFieldName);
      if Assigned(lField) and not lField.IsNull then
        Result := Format(' %s="%s"', [AFieldName, DBFieldToStr(lField, False, '') ])
      else
        Result := '';
    end;

    function VariantToXML(const VariantName: String; const VariantValue: Variant): String;
    begin
      if ValueHasValue(VariantValue) then
        Result := Format(' %s="%s"', [VariantName, ValueToStr(VariantValue, False, '')])
      else
        Result := '';
    end;

begin
  Result := Format('<pozitieDocum%s%s%s>',
                    [
                      VariantToXML('refCulGest', FCulgestItemsID),
                      VariantToXML('valCulGest', FSumaDeAngajat),
                      VariantToXML('cantCulGest', FCantitateDeAngajat)
                    ]);
  lDataSet  := FocusedDataSet;
  lSelField := lDataSet.FindField('Sel');
  if Assigned(lSelField) then begin
    lDataSet.DisableControls;
    try
      lDataSet.First;
      while not lDataSet.Eof do begin
        if lSelField.AsBoolean then
          Result := Result + Format(#13#10#9'<angajament%s%s%s%s%s%s%s/>',
                    [
                      FieldToXML('id_angajamente_defalcare'),
                      FieldToXML('procProcent'),
                      FieldToXML('valFacturare'),
                      FieldToXML('cod_functional'),
                      FieldToXML('cod_economic'),
                      FieldToXML('id_oi_unitati'),
                      FieldToXML('id_oi_proiecte')]);
        lDataSet.Next;
      end;
    finally
      lDataSet.EnableControls;
    end;
  end;
  Result := Result + '</pozitieDocum>';
end;

function TfrmAlopDisponibil.FocusedDataSet: TDataSet;
begin
  if pageBuget.ActivePage = tabLegal then
    Result := QryAngajamente
  else
  if pageBuget.ActivePage = tabOrd then
    Result := qryOrdonantari
  else
  if pageBuget.ActivePage = tabGlobal then
    Result := QryEconomic
  else
    Result := nil;
end;

procedure TfrmAlopDisponibil.UpdateSelected;
var
  lList: TStringList;
  lObjData: PDateLista;

    function ListaCoduriEconomice : String;
    var
      I: Integer;
      lListaCoduri: TStringList;
    begin
      lListaCoduri := TStringList.Create;
      try
        lListaCoduri.Duplicates := dupIgnore;
        for I := 0 to lList.Count-1 do
          lListaCoduri.Add(PDateLista(lList.Objects[I])^.CodEconomic);
        Result := lListaCoduri.CommaText;
      finally
        lListaCoduri.Free;
      end;
    end;

begin
  lList := InfoList;
  if Assigned(lList) and (lList.Count > 0) then begin
    lObjData        := PDateLista(lList.Objects[0]);
    FIdAngajament   := lObjData^.IdAng;
    FIdOrdonantare  := lObjData^.IdOrd;
    FCodFunctional  := lObjData^.CodFunctional;
    FIdUnitati      := lObjData^.IdUnitate;
    FCodEconomic    := ListaCoduriEconomice;
    FIdProiecte     := lObjData^.IdProiect;
    FDescriere      := lObjData^.Descriere;
  end;
end;

function TfrmAlopDisponibil.CanSelectDefalcare: Boolean;
begin
  Result := not FIsOnDocument;
  if not Result then begin
    Result := ValueHasValue(FCulgestItemsID);
    if not Result then
      MessageDlg('Eroare la salvarea defalcarii pe surse de finantare !'#13#10'Nu exista legatura cu pozitia din document !', mtError, [mbOk], 0)
    else begin
      Result := SumaDefalcataCorecta;
    end;
  end;
end;

function TfrmAlopDisponibil.SumaDefalcataCorecta: Boolean;
var
  lLastPos        : TBookmark;
  lVerificaTotal  : Boolean;
  lSumaDeFacturat,
  lSumaTotala: Currency;
begin
  lVerificaTotal  := False;
  lSumaTotala     := 0;
  QryAngajamente.DisableControls;
  try
    lLastPos := QryAngajamente.GetBookmark;
    QryAngajamente.First;
    while not QryAngajamente.Eof do begin
      if ValueIsTrue(QryAngajamente['Sel']) then begin
        lSumaTotala     := lSumaTotala + QryAngajamente.FieldByName('valFacturare').AsCurrency;
        lVerificaTotal  := True;
      end;
      QryAngajamente.Next;
    end;
  finally
    QryAngajamente.GotoBookmark(lLastPos);
    QryAngajamente.FreeBookmark(lLastPos);
    QryAngajamente.EnableControls;
  end;
  Result := not lVerificaTotal;
  if not Result then begin
    lSumaDeFacturat := ValueSafeToCurrency(FSumaDeAngajat);
    if Round(lSumaTotala * 100) <> Round(lSumaDeFacturat * 100) then begin
      Result := MessageDlg(Format('Suma defalcata pe surse este diferita de suma specificata in pozitia de document !'#13#10'Surse : %m <> Document : %m'#13#10+
                        'Doriti totusi continuarea?'#13#10'Continuarea cu o suma diferita pe surse fata de pozitia din document are implicatii asupra contarilor !',
                                [
                                  lSumaTotala,
                                  ValueSafeToCurrency(FSumaDeAngajat)
                                ]), mtConfirmation, [mbYes, mbNo], 0) = mrYes;
    end
    else
      Result := True;
  end;
end;

procedure TfrmAlopDisponibil.SetDetaliiPozitie(const aPrefix: String; ANumber, ADate: Variant; objData: PDateLista);
begin
  objData^.Descriere := aPrefix + ':';
  if ValueHasValue(ANumber) then
    objData^.Descriere := objData^.Descriere + ' ' + ValueSafeToStr(ANumber);
  if ValueHasValue(ADate) then
    objData^.Descriere := objData^.Descriere + ' - ' + FormatDateTime('dd.MM.yyyy', ValueSafeToDateTime(ADate));
  objData^.Descriere   := objData^.Descriere + ' : ' + GetDetaliiPozitie(objData^.CodFunctional, objData^.CodEconomic, objData^.IdProiect);
end;

function TfrmAlopDisponibil.GetProjDetails(const AProjID: Variant): String;
var
  I: Integer;
begin
  Result := '';
  for I := 0 to edTipProiect.Properties.Items.Count-1 do begin
    if ValueSameValue(edTipProiect.Properties.Items[I].Value, AProjID) then begin
      Result := edTipProiect.Properties.Items[I].Description;
      Break;
    end;
  end;
end;

function TfrmAlopDisponibil.GetDetaliiPozitie(const ACodFunctional,
  ACodEconomic, AProiectID: Variant): String;
begin
  Result := '';
  if ValueHasValue(ACodFunctional) then
    Result := Result + ValueSafeToStr(ACodFunctional);
  if ValueHasValue(ACodEconomic) then
    Result := Result + '/' + ValueSafeToStr(ACodEconomic);
  if ValueHasValue(AProiectID) then
    Result := Result + ' : ' + GetProjDetails(AProiectID);
end;

function TfrmAlopDisponibil.SilentValidateDesc(ACodEconomic,
  ACodFunctional, AIdAngajament: Variant): String;
begin
  Result := SilentValidateDesc(ACodFunctional, ACodEconomic, AIdAngajament, Null);
end;

procedure TfrmAlopDisponibil.AdaugaAngGlobalToList(ANode: TcxTreeListNode);
var
  lObjData: PDateLista;
  lValue  : String;
begin
  if Assigned(ANode) and not ANode.HasChildren then begin
    lValue := ValueSafeToStr(ANode.Values[TreeChildID.ItemIndex]);
    if FGlobalList.IndexOf(lValue) = -1 then begin
      New(lObjData);
      lObjData^.IdOrd         := Null;
      lObjData^.IdAng         := Null;
      lObjData^.ProcAng       := 0;
      lObjData^.ValProcent    := 0;
      lObjData^.CodFunctional := ANode.Values[TreeChildCOD_FUNCTIONAL.ItemIndex];
      lObjData^.IdUnitate     := aNode.Values[TreeChildID_OI_UNITATI.ItemIndex];
      lObjData^.CodEconomic   := aNode.Values[TreeChildCOD_ECONOMIC.ItemIndex];
      lObjData^.IdProiect     := aNode.Values[TreeChildID_OI_PROIECTE.ItemIndex];
      SetProjDetails(lObjData);
      SetDetaliiPozitie('AG', Null, Null, lObjData);
      FGlobalList.AddObject(lValue, TObject(lObjData));
    end;
  end;
end;

procedure TfrmAlopDisponibil.ClearAngGlobalFromList(ANode: TcxTreeListNode);
var
  lIndex: Integer;
  lEntry: PDateLista;
  lValue: String;
begin
  if Assigned(ANode) then begin
    lValue := ValueSafeToStr(ANode.Values[TreeChildID.ItemIndex]);
    lIndex := FGlobalList.IndexOf(lValue);
    if lIndex > -1 then begin
      lEntry := PDateLista(FGlobalList.Objects[lIndex]);
      if Assigned(lEntry) then
        Dispose(lEntry);
      FGlobalList.Delete(lIndex);
    end;
  end;
end;

procedure TfrmAlopDisponibil.SetGlobalNodeState(GlobalNode: TcxTreeListNode;
  AState: Integer);
  
    procedure PuneCopii(aNode: TcxTreeListNode; State: Integer);
    var
      I : Integer;
    begin
      ANode.ImageIndex := State;
      if aNode.HasChildren then begin
        for I := 0 to aNode.Count-1 do
          PuneCopii(aNode.Items[I], State);
      end
      else begin
        if State = 1 then
          AdaugaAngGlobalToList(ANode)
        else
          ClearAngGlobalFromList(ANode);
      end;
    end;

    procedure PuneParinti(aNode: TcxTreeListNode; State: Integer);
    var
      J, I : Integer;
    begin
      if not Assigned(aNode) or (ANode is TcxTreeListRootNode) then Exit;
      if State = 2 then begin
        aNode.ImageIndex := State;
        PuneParinti(aNode.Parent, State);
      end
      else begin
        { Daca are cel putin un copil diferit de starea curenta -> trece in 2 - grayed}
        for I := 0 to aNode.Count-1 do
          if aNode.Items[I].ImageIndex <> State then begin
            State := 2;
            Break;
          end;
        aNode.ImageIndex := State;
        PuneParinti(aNode.Parent, State);
      end;
    end;

begin
  if Assigned(GlobalNode) then begin
    GlobalNode.TreeList.BeginUpdate;
    try
      PuneCopii   (GlobalNode , AState);
      PuneParinti (GlobalNode , AState);
    finally
      GlobalNode.TreeList.EndUpdate;
    end;
  end;
end;

procedure TfrmAlopDisponibil.GridOrdKeyUp(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if not (ssCtrl in Shift) then begin
    if Key = VK_RETURN then
      GridOrdDblClick(nil)
    else
    if Key = VK_SPACE then
      DBSetFieldValue(qryOrdonantari, 'SEL', not ValueIsTrue(qryOrdonantari['SEL']));
  end;
end;

end.
