unit CulegereNoteUnit;

interface

{$I Contabilitate.inc}

uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ExtCtrls, dxCntner, StdCtrls, ImgList, DB, Menus, ZDataSet,  ActnList, DBSumLst,
  cxButtons, dxExEdtr, dxDBTLCl,
  dxGrClEx, dxTL, dxDBCtrl, dxDBTL, dxEdLib, dxEditor, dxmdaset,
  cxLookAndFeelPainters, AlopDisponibil, ZAbstractRODataset,
  ZAbstractDataset, cxGraphics, cxLookAndFeels, frmRepartitoriSelectUnit, cxDBTL,
  cxControls, cxContainer, cxEdit, Vcl.ComCtrls, dxCore, cxDateUtils,
  frmSelectieContractUnit, cxDropDownEdit, cxImageComboBox, cxMaskEdit, cxTextEdit, cxCalendar,
  cxCustomData, cxStyles, cxTL, cxTLdxBarBuiltInMenu,
  cxDataControllerConditionalFormattingRulesManagerDialog, cxInplaceContainer,
  cxTLData;

const
  WM_SET_STARE_NOTA = WM_USER + 1;
  WM_DROPDOWN_IMAGECOLUMN = WM_USER + 2;

type
  TfrmCulegereNote = class(TForm)
    pnDocument: TPanel;
    pnNota: TPanel;
    GridNota: TdxDBTreeList;
    lbDocument: TLabel;
    LbDataNota: TLabel;
    DataDoc: TcxDateEdit;
    NumarNota: TcxMaskEdit;
    GridNotaCONT_DEBT: TdxDBTreeListPopupColumn;
    GridNotaCONT_CRED: TdxDBTreeListPopupColumn;
    ImaginiEcl: TImageList;
    GridNotaECL: TdxDBTreeListImageColumn;
    GridNotaVALOARE: TdxDBTreeListCurrencyColumn;
    GridNotaPOZ: TdxDBTreeListMaskColumn;
    GridNotaCOMPUSA: TdxDBTreeListImageColumn;
    GridNotaCONTD: TdxDBTreeListMaskColumn;
    GridNotaCONTC: TdxDBTreeListMaskColumn;
    GridNotaBUGET: TdxDBTreeListPopupColumn;
    GridNotaID_CONTRACT: TdxDBTreeListPopupColumn;
    GridNotaCOD: TdxDBTreeListColumn;
    NotaPopup: TPopupMenu;
    LbDescJurnal: TLabel;
    edNrJurnal: TcxImageComboBox;
    BtnClose: TcxButton;
    BtnOk: TcxButton;
    DTItemsi: TDataSource;
    QryItemsi: TZQuery;
    QrySalveNota: TZQuery;
    TreePlan: TdxDBTreeList;
    TreePlanROMANA: TdxDBTreeListMaskColumn;
    TreePlanSID: TdxDBTreeListMaskColumn;
    TreePlanSIC: TdxDBTreeListMaskColumn;
    Cmd_NoteContabile: TActionList;
    Cmd_EchilibrareNota: TAction;
    Cmd_AdaugaNota: TAction;
    Cmd_PreiaNotaContabila: TAction;
    CmdAdaugaNota1: TMenuItem;
    CmdEchilibrareNota1: TMenuItem;
    CmdPreiaNotaContabila1: TMenuItem;
    Cmd_ModificaNota: TAction;
    ModificaNota1: TMenuItem;
    Cmd_DeleteNota: TAction;
    Cmd_SalveazaNota: TAction;
    TreePlanCONT: TdxDBTreeListMaskColumn;
    GridNotaREPARTITOR_CREDIT: TdxDBTreeListPopupColumn;
    ppStergeNota: TMenuItem;
    GridNotaEXPLICATIE: TdxDBTreeListMRUColumn;
    BtnValidare: TcxButton;
    Cmd_ValidareNota: TAction;
    Cmd_ValidareFinal: TAction;
    ValidareNota1: TMenuItem;
    N1: TMenuItem;
    ValidaresiIesire1: TMenuItem;
    LbTotal: TLabel;
    GridNotaNRDOC: TdxDBTreeListMaskColumn;
    GridNotaDATA: TdxDBTreeListDateColumn;
    GridNotaNR_OP: TdxDBTreeListColumn;
    GridNotaDATA_OP: TdxDBTreeListDateColumn;
    GridNotaREPARTITOR_DEBIT: TdxDBTreeListPopupColumn;
    GridNotaDATA_CONTRACT: TdxDBTreeListDateColumn;
    GridNotaNR_CONTRACT: TdxDBTreeListMaskColumn;
    GridNotaNR_PV: TdxDBTreeListMaskColumn;
    GridNotaDATA_PV: TdxDBTreeListDateColumn;
    chkRepVisible: TCheckBox;
    BtnModificare: TcxButton;
    TreePlanFctCont: TdxDBTreeListColumn;
    chkDeschidAutomat: TCheckBox;
    btnSterge: TcxButton;
    btnCopyNota: TcxButton;
    GridNotaNR_DOCUMENT: TdxDBTreeListMaskColumn;
    GridNotaDATA_DOCUMENT: TdxDBTreeListDateColumn;
    GridNotaDATA_SCADENTA: TdxDBTreeListDateColumn;
    TreeTipDoc: TdxDBTreeList;
    TreeTipDocTIP_DOC: TdxDBTreeListMaskColumn;
    TreeTipDocDENUMIRE: TdxDBTreeListMaskColumn;
    TreeTipDocID_TIPURI_DOC: TdxDBTreeListMaskColumn;
    GridNotaCOD_DOCUMENT: TdxDBTreeListPopupColumn;
    Cmd_CopiereContinut: TAction;
    N2: TMenuItem;
    CopiereContinut1: TMenuItem;
    N3: TMenuItem;
    mnuImportNote: TMenuItem;
    GridNotacod_functional_d: TdxDBTreeListPopupColumn;
    GridNotacod_functional_c: TdxDBTreeListPopupColumn;
    GridNotacod_economic_d: TdxDBTreeListPopupColumn;
    GridNotacod_economic_c: TdxDBTreeListPopupColumn;
    GridNotaid_oi_unitati_d: TdxDBTreeListPopupColumn;
    GridNotaid_oi_unitati_c: TdxDBTreeListPopupColumn;
    GridNotaid_oi_proiecte_d: TdxDBTreeListPopupColumn;
    GridNotaid_oi_proiecte_c: TdxDBTreeListPopupColumn;
    GridNotaCOD_FUNCTIONAL: TdxDBTreeListPopupColumn;
    GridNotaid_oi_unitati: TdxDBTreeListPopupColumn;
    GridNotaCOD_ECONOMIC: TdxDBTreeListPopupColumn;
    GridNotaid_oi_proiecte: TdxDBTreeListPopupColumn;
    qryVerificaDocument: TZQuery;
    procedure FormCreate(Sender: TObject);
    procedure FormCloseQuery(Sender: TObject; var CanClose: Boolean);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure BtnOkClick(Sender: TObject);
    procedure BtnCloseClick(Sender: TObject);
    procedure TreeBugeteDblClick(Sender: TObject);
    procedure TreePlanKeyUp(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure GridNotaCONT_DEBTCloseUp(Sender: TObject; var Text: String;
      var Accept: Boolean);
    procedure GridNotaChangeNode(Sender: TObject; OldNode,
      Node: TdxTreeListNode);
    procedure GridNotaDeletion(Sender: TObject; Node: TdxTreeListNode);
    procedure GridNotaKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure Cmd_AdaugaNotaExecute(Sender: TObject);
    procedure Cmd_DeleteNotaExecute(Sender: TObject);
    procedure Cmd_SalveazaNotaExecute(Sender: TObject);
    procedure Cmd_EchilibrareNotaExecute(Sender: TObject);
    procedure QryItemsiNewRecord(DataSet: TDataSet);
    procedure TreePlanROMANAGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure GridNotaCustomDrawCell(Sender: TObject; ACanvas: TCanvas;
      ARect: TRect; ANode: TdxTreeListNode; AColumn: TdxTreeListColumn;
      ASelected, AFocused, ANewItemRow: Boolean; var AText: String;
      var AColor: TColor; AFont: TFont; var AAlignment: TAlignment;
      var ADone: Boolean);
    procedure QryItemsiAfterOpen(DataSet: TDataSet);
    procedure Cmd_ModificaNotaExecute(Sender: TObject);
    procedure BtnModificareClick(Sender: TObject);
    procedure edNrJurnalChange(Sender: TObject);
    procedure NumarNotaKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure DataDocValidate(Sender: TObject; var ErrorText: String;
      var Accept: Boolean);
    procedure NumarNotaValidate(Sender: TObject; var ErrorText: String;
      var Accept: Boolean);
    procedure GridNotaREPARTITOR_CREDITGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);
    procedure GridNotaKeyPress(Sender: TObject; var Key: Char);
    procedure BtnValidareClick(Sender: TObject);
    procedure Cmd_ValidareNotaExecute(Sender: TObject);
    procedure TotalNotaSumItemChanged(Sender: TObject; Item: TDBSum);
    procedure GridNotaBUGETCloseUp(Sender: TObject; var Text: String;
      var Accept: Boolean);
    procedure GridNotaBUGETInitPopup(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure GridNotaREPARTITOR_DEBITInitPopup(Sender: TObject);
    procedure chkRepVisibleClick(Sender: TObject);
    procedure FormShow(Sender: TObject);
    procedure FormKeyUp(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure TreePlanCustomDrawCell(Sender: TObject; ACanvas: TCanvas;
      ARect: TRect; ANode: TdxTreeListNode; AColumn: TdxTreeListColumn;
      ASelected, AFocused, ANewItemRow: Boolean; var AText: String;
      var AColor: TColor; AFont: TFont; var AAlignment: TAlignment;
      var ADone: Boolean);
    procedure GridNotaEditing(Sender: TObject; Node: TdxTreeListNode;
      var Allow: Boolean);
    procedure GridNotaChangeColumn(Sender: TObject; Node: TdxTreeListNode;
      Column: Integer);
    procedure DataDocKeyDown(Sender: TObject; var Key: Word;
      Shift: TShiftState);
    procedure btnStergeClick(Sender: TObject);
    procedure GridNotaCONT_DEBTPopup(Sender: TObject;
      const EditText: String);
    procedure GridNotaCONT_CREDPopup(Sender: TObject;
      const EditText: String);
    procedure GridNotaREPARTITOR_DEBITPopup(Sender: TObject;
      const EditText: String);
    procedure GridNotaBUGETPopup(Sender: TObject; const EditText: String);
    procedure btnCopyNotaClick(Sender: TObject);
    procedure GridNotaEXPLICATIEButtonClick(Sender: TObject);
    procedure GridNotaCOD_DOCUMENTCloseUp(Sender: TObject;
      var Text: String; var Accept: Boolean);
    procedure GridNotaCOD_DOCUMENTPopup(Sender: TObject;
      const EditText: String);
    procedure GridNotaCOD_DOCUMENTValidate(Sender: TObject;
      var ErrorText: String; var Accept: Boolean);
    procedure Cmd_CopiereContinutExecute(Sender: TObject);
    procedure GridNotaREPARTITOR_DEBITCloseUp(Sender: TObject;
      var Text: String; var Accept: Boolean);
    procedure NumarNotaPropertiesValidate(Sender: TObject;
      var DisplayValue: Variant; var ErrorText: TCaption;
      var Error: Boolean);
    procedure DataDocPropertiesValidate(Sender: TObject;
      var DisplayValue: Variant; var ErrorText: TCaption;
      var Error: Boolean);
  procedure GridNotaBUGETCloseQuery(Sender: TObject;
      var CanClose: Boolean);
    procedure GridNotaID_CONTRACTInitPopup(Sender: TObject);
    procedure GridNotaID_CONTRACTCloseUp(Sender: TObject; var Text: string;
      var Accept: Boolean);
    procedure GridNotaID_CONTRACTGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: string);
  protected
    TotalNota: TDBSumList;
    procedure GridNotaGeneralOnPopup(Sender: TObject;
      const EditText: String);
    procedure GridNotaGeneralOnCloseUp(Sender: TObject; var Text: String;
      var Accept: Boolean);
    procedure GridNotaGeneralOnGetText(Sender: TObject;
      ANode: TdxTreeListNode; var AText: String);

    procedure InternalValidateCont(Val: String; AColIndex: Integer; Tree: TdxDbTreeList; const AllowChildren : Boolean = False); overload;
    procedure InternalValidateCont(Val: String; ATree: TcxDbTreeList; var InternalValue: Variant; const AllowChildren : Boolean = False); overload;
    procedure InternalValidateExact(Val : String; Tree: TdxDbTreeList; var InternalValue : Variant; const AllowChildren : Boolean = False; const ForceById : Word = 0; const lTag : Integer = -1);



    procedure ValidateNotaEcl(Node: TdxTreeListNode; NewValue: Currency);

    procedure SetStareCurentaNota(var Message: TMessage); message WM_SET_STARE_NOTA;

    procedure InitCulegere;

    procedure SetNextControl;
    procedure AddRepartitorToCache(const ARepID: Variant);
    function  GetRepartitorFromCache(const ARepID: Variant): String;

    procedure AplicaContCreditor(ClasaEconomica, ClasaFunc: String);
    procedure WmDropDownImgColumn(var Message: TMessage); message WM_DROPDOWN_IMAGECOLUMN;

    procedure CopyToItemsi(Nr : String);
    function IsInPeriod(aDate : TDateTime) : Boolean;
    procedure edNrContractPropertiesChange(Sender: TObject);
    procedure edDataContractPropertiesEditValueChanged(Sender: TObject);
  private
    FRepartitorCache: TStringList;
    FilterGestiuni : Boolean;
    Conturi : TdxMemData;
    FCurentIdNota: Integer;
    FDefalcareBuget  : TfrmAlopDisponibil;
    FSelectieRepartitor : TfrmRepartitoriSelect;
    FSelectieContract : TfrmSelectieContract;
    IsInAdaugareCont : Boolean;
    FIsInLoading     : Boolean;
    ForceSeekDown    : Boolean;

    MinData, MaxData : TDateTime;
    { Private declarations }
    { Proceduri de validare la nivel de camp }
    procedure ValidareContContabil(Sender: TField);
    procedure ValidareContBuget(Sender: TField);
    procedure ChangeContBuget(Sender : TField);
    { Validare Suma introduse pe nota }
    procedure ValidareSumaNota(Sender: TField);
    { Validare Repartitor }
    procedure ValidareCodRep(Sender: TField);
    {Validare CodDocument}
    procedure ValidareTipDoc(Sender : TField);
    procedure ValidareDataDocument(Sender : TField);
    procedure ValidareNrDocument(Sender : TField);
    procedure ValidareDateDocument(Sender : TField);
    { Validare atribute document }
    procedure UpdateItemsi;

    procedure SetIdNota(const Value: Integer);
    function  GetNodeByVal(ATree: TCustomdxTreeList; AColIndex: Integer; AVal: String; ASearchType: TdxTLSearchType = stExact): TdxTreeListNode;
    procedure SetDetaliiOnEmpty;
    procedure LoadPreluareMeniu;
    procedure DoPreluareNota(Sender: TObject);
    procedure RepOnSelectCloseEvent(Sender : TObject; Accept : Boolean);
  public
    procedure SetupColumnsEditors;
    function SalveazaItemsi : boolean;
    property CurentIdNota : Integer read FCurentIdNota write SetIdNota;
    { Public declarations }
  end;

implementation

uses
  ZeosDBUtile,
  dxCompsUtile,
  NoteUnitNew, ConcurentUsersUnit, Variants,
  CommonDBVar, DateUnit, DateUtils,
  ATSZDBUtils, CommonRepository, cxDataUtils;

{$R *.DFM}

procedure TfrmCulegereNote.FormCreate(Sender: TObject);
var
  lSumItem: TDBSum;
  lDataSet: TDataSet;
  lDataBalanta : Variant;
begin
  TotalNota := TDBSumList.Create(Self);
  TotalNota.DataSource      := DTItemsi;
  lSumItem                  := TDBSum(TotalNota.SumCollection.Add);
  lSumItem.FieldName        := 'VALOARE';
  lSumItem.GroupOperation   := goSum;
  TotalNota.SumItemChanged  := TotalNotaSumItemChanged;

  lDataBalanta := DBGetScallar('exec spDateStartBalanta');
  if VarIsArray(lDataBalanta) then begin
    MinData := lDataBalanta[2];
    MaxData := lDataBalanta[3];
  end;

  FRepartitorCache := TStringList.Create;
  DBRefresh(frmdata.QryPlanCont);
  GridNota.BeginUpdate;
  FilterGestiuni := False;

  GridNotaCONT_DEBT.BandIndex := 2;
  GridNotaCONT_CRED.BandIndex := 3;
  GridNotaREPARTITOR_DEBIT.BandIndex := 2;
  GridNotaREPARTITOR_CREDIT.BandIndex := 3;

  //trebuie reincarcate(puse cele de la design) pt ca le ia din fisier
  with GridNota do begin
    OptionsBehavior := [etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSort, etoDragExpand, etoDragScroll, etoEditing, etoEnterShowEditor, etoImmediateEditor, etoMultiSelect, etoTabs, etoTabThrough, etoVertThrough];
    OptionsDB       := [etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords];
    OptionsView     := [etoAutoWidth, etoBandHeaderWidth, etoIndicator, etoUseBitmap, etoUseImageIndexForSelected];
  end;

  with TreePlan do begin
    OptionsBehavior   := [etoAutoCopySelectedToClipboard, etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSearch, etoAutoSort, etoDblClick, etoDragExpand, etoDragScroll, etoEditing, etoEnterShowEditor, etoImmediateEditor, etoTabThrough];
    OptionsCustomize  := [etoBandMoving, etoBandSizing, etoColumnMoving, etoColumnSizing, etoExtCustomizing, etoKeepColumnWidth];
    OptionsView       := [etoAutoWidth, etoBandHeaderWidth, etoRowSelect, etoUseBitmap, etoUseImageIndexForSelected];
  end;

  Conturi := TdxMemData.Create(Self);

  if DBTableExists('MAPARE_COD_ECONOMIC') then begin
    lDataSet := DBNewQuery('exec spNoteMapareCodEconomic');
    try
      Conturi.LoadFromDataSet(lDataSet);
    finally
      lDataSet.Free;
    end;
  end;
  FSelectieRepartitor := TfrmRepartitoriSelect.Create(Self);
  FSelectieRepartitor.OnSelectCloseEvent := RepOnSelectCloseEvent;
  GridNotaREPARTITOR_CREDIT.PopupControl := FSelectieRepartitor;
  GridNotaREPARTITOR_DEBIT.PopupControl := FSelectieRepartitor;

  FDefalcareBuget := TfrmAlopDisponibil.Create(Self);
  FDefalcareBuget.MultipleSelection := False;
  GridNotaBUGET.PopupControl := FDefalcareBuget;
  ForceSeekDown := False;

  DataDoc.EditValue := Trunc(DBGetScallar('select getdate()'));

  { Deschidem tabela temporala pentru culegere note }
  QryItemsi.Close;
  QryItemsi.Params[0].Value := IdUtilizator;
  QryItemsi.Open;

  { Adaugam Tiurile de Jurnale Cunoscute }
  FillImageComboFmt(edNrJurnal.Properties, 'exec spNoteJurnaleUtilizator %d', [IdUtilizator], 'JURNAL', 'DENUMIRE');

  if (QryItemsi.RecordCount > 0) then
    edNrJurnal.EditValue := QryItemsi.FieldByName('JURNAL').AsString
  else
  if edNrJurnal.Properties.Items.Count > 0 then
    edNrJurnal.EditValue := edNrJurnal.Properties.Items[0].Value;

  InitCulegere;
  SetupColumnsEditors;
  LoadPreluareMeniu;

  chkRepVisible.Checked := (StorageReadValue('NoteAfisRep', '1') <> '0');
  chkDeschidAutomat.Checked := (StorageReadValue('NotePopup', '1') <> '0');
  StorageReadDxTree(GridNota);

  GridNota.EndUpdate;
  { Ajustam Dimensiunea la dimensiunea ecranului }
  WindowState := wsMaximized;
end;

procedure TfrmCulegereNote.FormCloseQuery(Sender: TObject; var CanClose: Boolean);
begin
  DoCheckPostDataSet(QryItemsi);
  CanClose := True;
  if QryItemsi.Modified then
    try
       SalveazaItemsi;
    except
      on E:Exception do
       CanClose := MessageDlg('Eroare la Salvarea Datelor !'#13#10'Doriti totusi parasirea ecranului de culegere ?'#13#10'Eroare : '+E.Message,mtError, [mbYes,mbNo],0)=mrYes;
    end;
  if chkRepVisible.Checked then StorageWriteValue('NoteAfisRep', '1') else StorageWriteValue('NoteAfisRep', '0');
  if chkDeschidAutomat.Checked then StorageWriteValue('NotePopup', '1') else StorageWriteValue('NotePopup', '0');
  StorageWriteDxTree(GridNota);
end;

function TfrmCulegereNote.SalveazaItemsi : Boolean;
begin
  if QryItemsi.State in [dsEdit, dsInsert] then QryItemsi.Post;
  if DBRecordExistsFmt('select top 1 1 from citems where ID_UTILIZATORI = %d AND (ECL = 0 OR ECL IS NULL)', [IdUtilizator]) then
     raise EContaHandledError.Create('Aveti note neechilibrate !'#13#10'Nu puteti salva nota curenta !');
  try
    DBStartTransaction;
    QryItemsi.ApplyUpdates;
    DBCommit;
    QryItemsi.CommitUpdates;
    Result := True;
  except
    on E:Exception do begin
      try
         Result := False;
         DBRollBack;
      except
      end;
      raise
    end;
  end;
end;

procedure TfrmCulegereNote.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  Action := caFree;
end;

procedure TfrmCulegereNote.BtnOkClick(Sender: TObject);
begin
  BtnValidare.Click;
  Close;
end;

procedure TfrmCulegereNote.BtnCloseClick(Sender: TObject);
begin
  Close;
end;

procedure TfrmCulegereNote.TreeBugeteDblClick(Sender: TObject);
begin
  { Inchidem Cu Accept }
  with TdxDBTreeList(Sender) do
    if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
    begin
      (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);

      if DBGetSetare('esteSocietateComerciala') = 1 then Exit;

      if Sender = TreePlan then
        if (frmdata.QryPlanCont.fieldbyname('cont').AsString[1]>'7') then
          with frmData.QryPlanCont.FieldByName('FctCont') do
          begin
            qryItemsi.Edit;
            GridNotaCONT_CRED.ReadOnly := False;
            if (AsString = 'D') then
            begin
              qryItemsi.FieldByName('CONT_DEBT').Value := frmdata.QryPlanCont.fieldbyname('cont').Value;
              qryItemsi.FieldByName('CONT_CRED').Value := 'X';
            end
            else if AsString='C' then
            begin
              qryItemsi.FieldByName('CONT_CRED').Value := frmdata.QryPlanCont.fieldbyname('cont').Value;
              qryItemsi.FieldByName('CONT_DEBT').Value := 'X';
            end;
            qryItemsi.Post;
            GridNotaCONT_CRED.ReadOnly :=True;
          end;
    end;
end;

procedure TfrmCulegereNote.TreePlanKeyUp(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if ssCtrl in Shift then Exit;

  if ((Key in [VK_UP, VK_DOWN]) and (ssAlt in Shift)) or
      ((Key = VK_F4) and not (ssAlt in Shift)) or (Key = VK_ESCAPE) then
    (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(False);
  if (Key = VK_RETURN) and (TdxDBTreeList(Sender).FocusedNode <> nil)
     and (not TdxDBTreeList(Sender).FocusedNode.HasChildren) then
     (GetParentForm(TdxDBTreeList(Sender)) as TdxPopupEditForm).ClosePopup(True);
end;

procedure TfrmCulegereNote.GridNotaCONT_DEBTCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
var
  lNode     : TdxDBTreeListNode;
  lEditabil : Boolean;
  lColumn   : TdxDBTreeListPopupColumn;
begin
  lColumn := TdxDBTreeListPopupColumn(Sender);
  if Accept then begin
    lNode := TdxDBTreeListNode(TdxDBTreeList(lColumn.PopupControl).FocusedNode);
    if Assigned(lNode) then begin
      lEditabil := DBGoEdit(lColumn.Field.DataSet);
      DBSetFieldValue(lColumn.Field, lNode.Id);
      if lEditabil then DBGoEdit(lColumn.Field.DataSet);
      Text    := lNode.Id;
      Accept  := False;
    end;
  end;
  if FrmData.QryRepartitori.Filtered then begin
     FrmData.QryRepartitori.Filter := '';
     FrmData.QryRepartitori.Filtered := False;
  end;
end;

procedure TfrmCulegereNote.ValidareContBuget(Sender: TField);
var
  lOut : Variant;
  lTree : TcxDBTreeList;
  lColumn : TdxDBTreeListColumn;
begin
  if Sender.IsNull then Exit;
  lColumn := GridNota.ColumnByFieldName(Sender.FieldName);
  if (lColumn <> nil) and (lColumn is TdxDBTreeListPopupColumn) and
    (TdxDBTreeListPopupColumn(lColumn).PopupControl <> nil) and
    (TdxDBTreeListPopupColumn(lColumn).PopupControl is TcxDBTreeList) then begin
    lTree := TcxDBTreeList(TdxDBTreeListPopupColumn(lColumn).PopupControl);
    InternalValidateCont(Trim(Sender.AsString), lTree, lOut, (lTree.Tag=0));
  end;
end;

procedure TfrmCulegereNote.ValidareContContabil(Sender: TField);
var IsOnDebit: Boolean;
begin
  if IsInAdaugareCont then Exit;

  if UpperCase(Sender.AsString) = 'X' then Exit;

  IsOnDebit := Sender.Tag = 1;
  if Sender.AsString = '%' then begin
     QryItemsi.FieldByName('ECL').AsInteger := 0;
     if IsOnDebit then QryItemsi.FieldByName('COMPUSA').AsString := '1'
     else QryItemsi.FieldByName('COMPUSA').AsString := '2';
     GridNotaChangeNode(GridNota, nil, GridNota.FocusedNode);
  end
  else InternalValidateCont(Trim(Sender.AsString), TreePlanCONT.Index, TreePlan);

  if DBGetSetare('esteSocietateComerciala') = 1 then Exit;

  if IsOnDebit then begin
     QryItemsi.FieldByName('CONTD').AsString := Sender.AsString;
     if Sender.AsString[1]>'7' then
       qryItemsi.FieldByName('CONT_CRED').Value := 'X'
     else
       if qryItemsi.FieldByName('CONT_CRED').AsString <> '' then begin
         IsInAdaugareCont := True;
         qryItemsi.FieldByName('CONT_CRED').AsString := StringReplace(qryItemsi.FieldByName('CONT_CRED').AsString, 'X', '', [rfReplaceAll]);
         IsInAdaugareCont := False;
       end;
  end
  else begin
     QryItemsi.FieldByName('CONTC').AsString := Sender.AsString;
     if Sender.AsString[1]>'7' then
       qryItemsi.FieldByName('CONT_DEBT').Value := 'X'
     else
       if qryItemsi.FieldByName('CONT_DEBT').AsString <> '' then begin
         IsInAdaugareCont := True;
         qryItemsi.FieldByName('CONT_DEBT').AsString := StringReplace(qryItemsi.FieldByName('CONT_DEBT').AsString, 'X', '', [rfReplaceAll]);
         IsInAdaugareCont := False;
       end;
  end;
end;

procedure TfrmCulegereNote.ValidareSumaNota(Sender: TField);
const Lock: Boolean = False;
begin
  { Validam suma Introdusa }
  if Lock then Exit;
  Lock := True;
  try
     if QryItemsi.FieldByName('COMPUSA').AsString = '0' then
        QryItemsi.FieldByName('ECL').AsInteger := Integer(Sender.AsCurrency <> 0)
     else ValidateNotaEcl(GridNota.FocusedNode, Sender.AsCurrency);
  finally
     Lock := False;
  end;
end;

procedure TfrmCulegereNote.InternalValidateCont(Val: String; AColIndex: Integer;
  Tree: TdxDbTreeList; const AllowChildren : Boolean = False);
var MustDrop: Boolean;
    SelNode : TdxTreeListNode;
begin
  Tree.EndSearch;
  { Se valideaza contul de buget introdus }
  MustDrop := (Val = '') or (Val = '?');
  { Incercam sa gasim nodul posibil }
  SelNode := nil;
  if not MustDrop then begin
     if AColIndex = -1 then begin
        SelNode := TdxDBTreeList(Tree).FindNodeByKeyValue(Val);
        MustDrop := not Assigned(SelNode);
     end
     else begin
       while (not Assigned(SelNode)) and (Length(Val) > 0) do begin
         SelNode := GetNodeByVal(Tree, AColIndex, Val);
         if not Assigned(SelNode) then begin MustDrop := True; Delete(Val, Length(Val), 1); end;
       end;
       if not MustDrop then MustDrop := (not Assigned(SelNode)) or not (AllowChildren or not SelNode.HasChildren);
       if (ForceSeekDown) and (Assigned(SelNode)) and (MustDrop) then begin
          while SelNode.Count > 0 do SelNode := SelNode.Items[0];
          SelNode.Focused := True;
       end;
     end;
  end
  else begin
     { Mergem Pe Focused Node in jos }
     SelNode := Tree.FocusedNode;
     while (Assigned(SelNode)) and (SelNode.Count > 0) do SelNode := SelNode.Items[0];
     if Assigned(SelNode) then SelNode.Focused := True;
  end;

  if (MustDrop) and (Assigned(GridNota.InplaceEditor)) then begin
      if SelNode <> nil then
        while SelNode.Count > 0 do SelNode := SelNode.Items[0];
      if Assigned(SelNode) then begin SelNode.MakeVisible; SelNode.Focused := True; end;
      if Assigned(SelNode) and SelNode.HasChildren then SelNode.Expand(True);
      Tree.StartSearch(-1, Val);
      SendMessage(GridNota.InplaceEditor.Handle, CM_DROPDOWNPOPUPFORM, 0, 0);
      Abort;
  end;
end;

procedure TfrmCulegereNote.SetStareCurentaNota(var Message: TMessage);
var lNode: TdxDBTreeListNode;
    I: Integer;
    Ecl : Integer;
    procedure SetEchilibrat(lIdItems: Integer);
    var OldEdit: Boolean;
     begin
       with QryItemsi do
         if (Locate('ID_CITEMS', lIdItems, [])) and
            (FieldByName('ECL').AsInteger <> Ecl) then begin
            OldEdit := State in [dsEdit, dsInsert];
            if not OldEdit then Edit;
            FieldByName('ECL').AsInteger := Ecl;
            Post;
            if OldEdit then Edit;
         end;
     end;
begin
  { Setam stare curenta pentru nota compusa }
  { In WParam avem Nodul Parinte al Notei }
  { In LParam avem 0 - pentru neechilibrata, 1 - pentru Echilibrata }
  lNode := TdxDBTreeListNode(Message.WParam);
  if Assigned(lNode) then
     try
       Ecl := Integer(Message.LParam);
       QryItemsi.DisableControls;
       SetEchilibrat(lNode.Id);
       for I := 0 to lNode.Count-1 do
         SetEchilibrat(TdxDBTreeListNode(lNode.Items[I]).Id);
    finally
       QryItemsi.EnableControls;
    end;
end;

procedure TfrmCulegereNote.GridNotaChangeNode(Sender: TObject; OldNode,
  Node: TdxTreeListNode);
begin
  GridNotaCONT_DEBT.ReadOnly         := False;
  GridNotaCONT_CRED.ReadOnly         := False;
  GridNotaREPARTITOR_CREDIT.ReadOnly := False;
  GridNotaREPARTITOR_DEBIT.ReadOnly  := False;
  if (Node <> nil) and (Node.Parent <> nil) then begin
     if Node.Strings[GridNotaCOMPUSA.Index] = '1' then begin
        GridNotaCONT_CRED.ReadOnly         := True;
        GridNotaREPARTITOR_CREDIT.ReadOnly := True;
     end
     else if Node.Strings[GridNotaCOMPUSA.Index] = '2' then begin
        GridNotaCONT_DEBT.ReadOnly        := True;
        GridNotaREPARTITOR_DEBIT.ReadOnly := True;
     end;
  end;
  GridNotaCONT_DEBT.DisableEditor         := GridNotaCONT_DEBT.ReadOnly;
  GridNotaCONT_CRED.DisableEditor         := GridNotaCONT_CRED.ReadOnly;
  GridNotaREPARTITOR_CREDIT.DisableEditor := GridNotaREPARTITOR_CREDIT.ReadOnly;
  GridNotaREPARTITOR_DEBIT.DisableEditor  := GridNotaREPARTITOR_DEBIT.ReadOnly;
end;

procedure TfrmCulegereNote.GridNotaDeletion(Sender: TObject;
  Node: TdxTreeListNode);
var lNode: TdxDBTreeListNode;
    I : Integer;
    lTotal, lPartial: Currency;
begin
  { Aici verificam daca nu cumva se dezechilibreaza nota }
  if (Node <> nil) and (Node.Parent <> nil) then begin
     lNode := TdxDBTreeListNode(Node.Parent);
     if Node.Deleting and lNode.Deleting then Exit;
     lTotal := GetdxCurrency(lNode, GridNotaVALOARE.Index);
     lPartial := 0;
     for I := 0 to lNode.Count-1 do
       if lNode.Items[I] <> Node then lPartial := lPartial + GetdxCurrency(lNode.Items[I], GridNotaVALOARE.Index);
     PostMessage(Handle, WM_SET_STARE_NOTA, Integer(lNode), Integer(lTotal = lPartial));
  end;
end;

procedure TfrmCulegereNote.ValidateNotaEcl(Node: TdxTreeListNode;
  NewValue: Currency);
var lTotalCod,
    lTotalDef: Currency;
    I : Integer;
begin
  if (not Assigned(Node)) or (Node.Strings[GridNotaCOMPUSA.Index] = '0') then Exit;

  { Daca nu este echilibrata incercam sa vedem daca s-a echilibrat intre timp }
  lTotalDef := 0;
  if Node.HasChildren then begin
     { Trebuie sa vedem daca suma de pe nodul curent este egala cu suma de pe toti copii }
     lTotalCod := NewValue;
     for I := 0 to Node.Count-1 do
       lTotalDef := lTotalDef + GetdxCurrency(Node.Items[I], GridNotaVALOARE.Index);
     PostMessage(Handle, WM_SET_STARE_NOTA, Integer(Node), Integer(lTotalDef = lTotalCod));
  end
  else
    if Assigned(Node.Parent) then begin
       lTotalCod := GetdxCurrency(Node.Parent, GridNotaVALOARE.Index);
       for I := 0 to Node.Parent.Count-1 do
         if Node.Parent.Items[I] = Node then lTotalDef := lTotalDef + NewValue
         else lTotalDef := lTotalDef + GetdxCurrency(Node.Parent.Items[I], GridNotaVALOARE.Index);
       PostMessage(Handle, WM_SET_STARE_NOTA, Integer(Node.Parent), Integer(lTotalDef = lTotalCod));
    end;
end;

procedure TfrmCulegereNote.GridNotaKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
var
    DelFieldName : String;

  function FindNextColumn(AIndex: Integer): Integer;
   begin
     if AIndex < TdxDBTreeList(Sender).VisibleColumnCount then begin
        Result := AIndex+1;
        while (Result < TdxDBTreeList(Sender).VisibleColumnCount) and
              (TdxDBTreeList(Sender).Columns[Result].ReadOnly) do Inc(Result);
        if Result = TdxDBTreeList(Sender).VisibleColumnCount then Result := 0;
     end else Result := 0;
   end;

  function FindPrevIndex(AIndex: Integer): Integer;
   begin
     if AIndex > 0 then begin
        Result := AIndex-1;
        while (Result > 0) and
              (TdxDBTreeList(Sender).Columns[Result].ReadOnly) do Dec(Result);
        if Result = 0 then Result := TdxDBTreeList(Sender).VisibleColumnCount;
     end else Result := TdxDBTreeList(Sender).VisibleColumnCount;
   end;

  function ClearOnDelete(aFieldName : String; aKey : Word; const aForce : Boolean = False) : Boolean;
  var OldEdit : Boolean;
  begin
    Result := False;
    if aForce or (DelFieldName=aFieldName) then begin
       OldEdit := QryItemsi.State in [dsEdit, dsInsert];
       if not OldEdit then QryItemsi.Edit;
       QryItemsi.FindField(aFieldName).Clear;
       QryItemsi.Post;
       if OldEdit then QryItemsi.Edit;
       Result := True;
    end;
  end;


begin
  if (Key = VK_DELETE) and (not Assigned(TdxDBTreeList(Sender).InplaceEditor) or not TdxDBTreeList(Sender).InplaceEditor.IsVisible) then begin
    DelFieldName := '';
    if GridNota.FocusedField <> nil then
      DelFieldName := UpperCase(GridNota.FocusedField.FieldName);
    ClearOnDelete('REPARTITOR_CREDIT', Key);
    ClearOnDelete('REPARTITOR_DEBIT', Key);
    ClearOnDelete('COD_FUNCTIONAL', Key);
    ClearOnDelete('COD_FUNCTIONAL_D', Key);
    ClearOnDelete('COD_FUNCTIONAL_C', Key);
    ClearOnDelete('COD_ECONOMIC', Key);
    ClearOnDelete('COD_ECONOMIC_D', Key);
    ClearOnDelete('COD_ECONOMIC_C', Key);
    ClearOnDelete('VALOARE', Key);
    ClearOnDelete('EXPLICATIE', Key);
    ClearOnDelete('ID_OI_UNITATI', Key);
    ClearOnDelete('ID_OI_UNITATI_D', Key);
    ClearOnDelete('ID_OI_UNITATI_C', Key);
    ClearOnDelete('ID_OI_PROIECTE', Key);
    ClearOnDelete('ID_OI_PROIECTE_D', Key);
    ClearOnDelete('ID_OI_PROIECTE_C', Key);
    if DelFieldName = 'DETALII_ANGAJAMENT' then begin
      ClearOnDelete('DETALII_ANGAJAMENT', Key, True);
      ClearOnDelete('ID_ANGAJAMENTE_DEFALCARE', Key, True);
      ClearOnDelete('COD_FUNCTIONAL', Key, True);
      ClearOnDelete('COD_ECONOMIC', Key, True);
      ClearOnDelete('ID_OI_UNITATI', Key, True);
      ClearOnDelete('ID_OI_PROIECTE', Key, True);
    end;

    if (DelFieldName = 'CONT_DEBT') and (QryItemsi.FieldByName('CONT_DEBT').AsString <> 'X') then begin
       IsInAdaugareCont := True;
       ClearOnDelete('CONT_DEBT', Key);
       if QryItemsi.FieldByName('CONT_CRED').AsString = 'X' then ClearOnDelete('CONT_CRED', Key, True);
       IsInAdaugareCont := False;
    end;
    if (DelFieldName = 'CONT_CRED') and (QryItemsi.FieldByName('CONT_CRED').AsString <> 'X') then begin
       IsInAdaugareCont := True;
       ClearOnDelete('CONT_CRED', Key);
       if QryItemsi.FieldByName('CONT_DEBT').AsString = 'X' then ClearOnDelete('CONT_DEBT', Key, True);
       IsInAdaugareCont := False;
    end;
  end;


  if (Key = VK_DOWN) and (GridNota.FocusedNode = GridNota.LastNode) then Cmd_AdaugaNota.Execute
  else if Key = VK_RETURN then
    with TdxDBTreeList(Sender) do begin
      FocusedColumn := FindNextColumn(FocusedColumn);
      ShowEditor;
      if Assigned(GridNota.InplaceEditor) then GridNota.InplaceEditor.SetFocus;
      if GridNota.VisibleColumns[FocusedColumn] is TdxDBTreeListPopupColumn then
        PostMessage(Handle, WM_DROPDOWN_IMAGECOLUMN, 0, 0);
    end
  else
  with TdxDBTreeList(Sender) do
    if Assigned(InplaceEditor) and
       (InplaceEditor is TdxInplaceTextEdit) and
       InplaceEditor.IsVisible then
      with TdxInplaceTextEdit(InplaceEditor) do
        if (Key = VK_LEFT) and (SelLength = 0) and (SelStart = 0) then
        begin
          CancelEditor;
          FocusedColumn := FindPrevIndex(FocusedColumn);
          SendMessage(GridNota.Handle,WM_KEYDOWN,VK_F2,0);
          SelStart := Length(Text);
          SelLength := 0;
        end
        else
          if (Key = VK_RIGHT) and (SelLength = 0) and (SelStart = Length(Text)) then
          begin
            CancelEditor;
            FocusedColumn := FindNextColumn(FocusedColumn);
            SendMessage(GridNota.Handle,WM_KEYDOWN,VK_F2,0);
            SelStart := 0;
            SelLength := 0;
          end;
end;

procedure TfrmCulegereNote.Cmd_AdaugaNotaExecute(Sender: TObject);
var
  lNode: TdxDBTreeListNode;
  SCompusa: String;
  lId : Integer;
begin
  if not ValueHasValue(edNrJurnal.EditValue) then begin
     edNrJurnal.SetFocus;
     raise EContaHandledError.Create('Selectati jurnalul la care apartine nota curenta !');
  end;
  if not ValueHasValue(NumarNota.EditValue) then begin
     NumarNota.SetFocus;
     raise EContaHandledError.Create('Introduceti numarul notei contabile !');
  end;
  if not ValueHasValue(DataDoc.EditValue) then begin
     DataDoc.SetFocus;
     raise EContaHandledError.Create('Introduceti data notei contabile !');
  end;
  if not IsInPeriod(DataDoc.Date) then begin
     DataDoc.SetFocus;
     raise EContaHandledError.Create('Data curenta nu este in perioada fiscala deschisa! (' + FormatDateTime('dd.mm.yyyy', MinData) + ' - ' + FormatDateTime('dd.mm.yyyy', MaxData)+ ')');
  end;

  { Adaugam o noua inregistrare }
  { Ne raportam la inregistrarea pe care suntem in acest moment }
  lNode := TdxDBTreeListNode(GridNota.FocusedNode);
  with QryItemsi do begin
    //GridNota.BeginUpdate;
    DisableControls;
    try
       Append;
       FieldByName('ECL').AsInteger := 0;
       if (not Assigned(lNode)) or (lNode.Strings[GridNotaCOMPUSA.Index] = '0') or
          (lNode.Strings[GridNotaECL.Index] = '1') then begin
          SCompusa := '0';
          QryItemsi.FieldByName('ID_PARINTE').Value        := Null;
       end
       else begin
          SCompusa := lNode.Strings[GridNotaCOMPUSA.Index];
          if Assigned(lNode.Parent) then begin
             QryItemsi.FieldByName('ID_PARINTE').AsInteger   := lNode.ParentId;
             lNode := TdxDBTreeListNode(lNode.Parent);
          end
          else QryItemsi.FieldByName('ID_PARINTE').AsInteger := lNode.Id;

          if SCompusa = '1' then begin
             QryItemsi.FieldByName('CONTC').AsString := lNode.Strings[GridNotaCONT_CRED.Index];
             QryItemsi.FieldByName('REPARTITOR_CREDIT').Value := lNode.Values[GridNotaREPARTITOR_CREDIT.Index];
          end
          else begin
             QryItemsi.FieldByName('CONTD').AsString := lNode.Strings[GridNotaCONT_DEBT.Index];
             QryItemsi.FieldByName('REPARTITOR_DEBIT').Value := lNode.Values[GridNotaREPARTITOR_DEBIT.Index];
          end;
          IsInAdaugareCont := True;
          QryItemsi.FieldByName('CONT_DEBT').AsString := QryItemsi.FieldByName('CONTD').AsString;
          QryItemsi.FieldByName('CONT_CRED').AsString := QryItemsi.FieldByName('CONTC').AsString;
          IsInAdaugareCont := False;

       end;
       QryItemsi.FieldByName('COMPUSA').AsString := SCompusa;
       QryItemsi.FieldByName('POZ').AsInteger := QryItemsi.FieldByName('ID_CITEMS').AsInteger;
       Post;
       lId := QryItemsi.FieldByName('ID_CITEMS').AsInteger;
    finally
       EnableControls;
       GridNota.FullRefresh;
       lNode := TdxDBTreeListNode(GridNota.FindNodeByKeyValue(lId));
       if lNode <> nil then begin
          lNode.Focused := True;
          lNode.MakeVisible;
          GridNotaChangeNode(GridNota, nil, lNode);
       end;
       { Stabilim si campul implicit de introducere }
       if SCompusa = '2' then GridNota.FocusedField := GridNotaCONT_CRED.Field
       else GridNota.FocusedField := GridNotaCONT_DEBT.Field;
    end;
  end;
 // GridNota.Invalidate;
end;

procedure TfrmCulegereNote.Cmd_DeleteNotaExecute(Sender: TObject);
var lNode: TdxDBTreeListNode;
    I    : Integer;  
    lMessage: String;
begin
  lNode := TdxDBTreeListNode(GridNota.FocusedNode);
  if GridNota.SelectedCount > 1 then
     lMessage := 'Doriti stergrea pozitiilor selectate?'
  else
    if (Assigned(lNode)) and (lNode.HasChildren) then
       lMessage := 'Doriti stergerea notei compuse din nota contabila?'
    else if Assigned(lNode) then lMessage := 'Doriti stergerea pozitie din nota contabila?'
         else Exit;

  if MessageDlg(lMessage, mtConfirmation, [mbYes, mbNo], 0) = mrYes then
     if GridNota.SelectedCount > 1 then
        for I := GridNota.SelectedCount-1 downto 0 do begin
          lNode := TdxDBTreeListNode(GridNota.SelectedNodes[I]);
          lNode.Delete
        end
     else lNode.Delete;
  GridNota.Invalidate;
end;

procedure TfrmCulegereNote.Cmd_SalveazaNotaExecute(Sender: TObject);
begin
  if not SalveazaItemsi then Exit;
  try
    DBStartTransaction;
    QrySalveNota.Params.ParamByName('ID_UTILIZATORI').AsInteger := IdUtilizator;
    QrySalveNota.Params.ParamByName('COD').AsInteger := GetNextId('NOTA_CONTABILA');
    QrySalveNota.ExecSql;
    DBCommit;
  except
    on E:Exception do begin
      try
         DBRollBack;
      except
      end;
      raise
    end;
  end;
  QryItemsi.Close;
  QryItemsi.Open;
end;

procedure TfrmCulegereNote.Cmd_EchilibrareNotaExecute(Sender: TObject);
var Node,
    lNode: TdxDBTreeListNode;
    I : Integer;
    lTotal, lPartial: Currency;
    OldState: Boolean;
begin
  { Aici verificam daca nu cumva se dezechilibreaza nota }
  Node := TdxDBTreeListNode(GridNota.FocusedNode);
  if Node = nil then Exit;
  if (Node.Strings[GridNotaCOMPUSA.Index] = '0') or (Node.Parent = nil) then
     raise EContaHandledError.Create('Nu puteti echilibra decat pozitii din note compuse !');

  lNode := TdxDBTreeListNode(Node.Parent);
  lTotal := GetdxCurrency(lNode, GridNotaVALOARE.Index);
  lPartial := 0;
  for I := 0 to lNode.Count-1 do
    if lNode.Items[I] <> Node then lPartial := lPartial + GetdxCurrency(lNode.Items[I], GridNotaVALOARE.Index);
  { Salvam Valoarea pe pozitia curenta }
  with QryItemsi do begin
    DisableControls;
    GridNota.BeginUpdate;   
    try
       if QryItemsi.FieldByName('ID_CITEMS').AsInteger <> Node.Id then
          if not QryItemsi.Locate('ID_CITEMS', Node.Id, []) then
             raise EContaHandledError.Create('EROARE : nu s-a gasit pozitia :'+IntToStr(Node.Id)+' din nota curenta !');
       OldState := QryItemsi.State in [dsEdit, dsInsert];
       if not OldState then QryItemsi.Edit;
       QryItemsi.FieldByName('VALOARE').AsCurrency := lTotal - lPartial;
       QryItemsi.Post;
       if OldState then QryItemsi.Edit;
       PostMessage(Handle, WM_SET_STARE_NOTA, Integer(lNode), Integer(True));
    finally
       EnableControls;
    end;
    GridNota.EndUpdate;
  end;
end;

procedure TfrmCulegereNote.QryItemsiNewRecord(DataSet: TDataSet);
begin
  QryItemsi.FieldByName('ID_UTILIZATORI').AsInteger := IdUtilizator;
  if QryItemsi.FindField('NR_OP') <> nil then
     QryItemsi.FieldByName('NR_OP').AsInteger := GetNextId('POZITIE_NOTA');
  QryItemsi['NRDOC']   := NumarNota.EditValue;
  QryItemsi['JURNAL']  := edNrJurnal.EditValue;
  QryItemsi['DATA']    := DataDoc.EditValue;
  QryItemsi['DATA_DOCUMENT'] := DataDoc.EditValue;
end;

procedure TfrmCulegereNote.TreePlanROMANAGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
begin
  AText := Trim(ANode.Strings[TreePlanCONT.Index])+' : '+Trim(AText);
end;

procedure TfrmCulegereNote.SetIdNota(const Value: Integer);
begin
  FCurentIdNota := Value;
end;

procedure TfrmCulegereNote.InitCulegere;
var
  lDataSet  : TDataSet;
  lColumn   : TdxDBTreeListColumn;
  I : Integer;
begin
  if not QryItemsi.IsEmpty then begin
    FIsInLoading    := True;
    QryItemsi.DisableControls;
    try
      edNrJurnal.EditValue := QryItemsi['JURNAL'];
      NumarNota.EditValue  := QryItemsi['NRDOC'];
      DataDoc.EditValue    := QryItemsi['DATA'];
      QryItemsi.First;
      while not QryItemsi.Eof do begin
        AddRepartitorToCache(QryItemsi['REPARTITOR_CREDIT']);
        AddRepartitorToCache(QryItemsi['REPARTITOR_DEBIT']);
        if GridNotaEXPLICATIE.Items.IndexOf(QryItemsi.FieldByName('EXPLICATIE').AsString) = -1 then
          GridNotaEXPLICATIE.Items.Add(QryItemsi.FieldByName('EXPLICATIE').AsString);
        QryItemsi.Next;
      end;
    finally
      FIsInLoading    := False;
      QryItemsi.EnableControls;
    end;
  end;
  GridNotaChangeNode(GridNota, nil, GridNota.TopNode);
  { Validare conturi introduse }
  with QryItemsi.FieldByName('CONT_DEBT') do begin
    Tag        := 1;
    OnValidate := ValidareContContabil;
  end;
  QryItemsi.FieldByName('CONT_CRED').OnValidate := ValidareContContabil;
  QryItemsi.FieldByName('DATA_DOCUMENT').OnValidate := ValidareDataDocument;

  { Validare Suma introduse pe nota }
  QryItemsi.FieldByName('VALOARE').OnValidate := ValidareSumaNota;
  { Validare Cod Repartitor}
  QryItemsi.FieldByName('REPARTITOR_CREDIT').OnValidate  := ValidareCodRep;
  QryItemsi.FieldByName('REPARTITOR_DEBIT').OnValidate  := ValidareCodRep;
  { Validare CodDocument}
  QryItemsi.FieldByName('COD_DOCUMENT').OnValidate  := ValidareTipDoc;

  QryItemsi.FieldByName('NR_DOCUMENT').OnValidate := ValidareNrDocument;


  QryItemsi.FieldByName(GridNotaCOD_FUNCTIONAL.FieldName).OnChange := ChangeContBuget;
  QryItemsi.FieldByName(GridNotaCOD_ECONOMIC.FieldName).OnChange := ChangeContBuget;
  QryItemsi.FieldByName(GridNotaid_oi_unitati.FieldName).OnChange := ChangeContBuget;
  QryItemsi.FieldByName(GridNotaid_oi_proiecte.FieldName).OnChange := ChangeContBuget;
  QryItemsi.FieldByName(GridNotaCOD_FUNCTIONAL.FieldName).OnValidate := ValidareContBuget;
  QryItemsi.FieldByName(GridNotaCOD_ECONOMIC.FieldName).OnValidate := ValidareContBuget;
  QryItemsi.FieldByName(GridNotaid_oi_unitati.FieldName).OnValidate := ValidareContBuget;
  QryItemsi.FieldByName(GridNotaid_oi_proiecte.FieldName).OnValidate := ValidareContBuget;

  if QryItemsi.FindField(GridNotacod_functional_d.FieldName) <> nil then
    QryItemsi.FieldByName(GridNotacod_functional_d.FieldName).OnValidate := ValidareContBuget;
  if QryItemsi.FindField(GridNotacod_functional_c.FieldName) <> nil then
    QryItemsi.FieldByName(GridNotacod_functional_c.FieldName).OnValidate := ValidareContBuget;
  if QryItemsi.FindField(GridNotacod_economic_d.FieldName) <> nil then
    QryItemsi.FieldByName(GridNotacod_economic_d.FieldName).OnValidate := ValidareContBuget;
  if QryItemsi.FindField(GridNotacod_economic_c.FieldName) <> nil then
    QryItemsi.FieldByName(GridNotacod_economic_c.FieldName).OnValidate := ValidareContBuget;
  if QryItemsi.FindField(GridNotaid_oi_unitati_d.FieldName) <> nil then
    QryItemsi.FieldByName(GridNotaid_oi_unitati_d.FieldName).OnValidate := ValidareContBuget;
  if QryItemsi.FindField(GridNotaid_oi_unitati_c.FieldName) <> nil then
    QryItemsi.FieldByName(GridNotaid_oi_unitati_c.FieldName).OnValidate := ValidareContBuget;
  if QryItemsi.FindField(GridNotaid_oi_proiecte_d.FieldName) <> nil then
    QryItemsi.FieldByName(GridNotaid_oi_proiecte_d.FieldName).OnValidate := ValidareContBuget;
  if QryItemsi.FindField(GridNotaid_oi_proiecte_c.FieldName) <> nil then
    QryItemsi.FieldByName(GridNotaid_oi_proiecte_c.FieldName).OnValidate := ValidareContBuget;


  { Verificam coloanele care urmeaza sa fie afisate }
  if DBTableExists('LISTA_COLOANE_NOTE') then begin
    lDataSet := DBNewQuery('SELECT FIELD_NAME, VISIBIL FROM LISTA_COLOANE_NOTE');
    try
      lDataSet.Open;
      while not lDataSet.Eof do begin
        lColumn := GridNota.FindColumnByFieldName(Trim(lDataSet.Fields[0].AsString));
        if Assigned(lColumn) then
          lColumn.Visible := Boolean(lDataSet.Fields[1].AsInteger);
        lDataSet.Next;
      end;
    finally
      lDataSet.Free;
    end;
  end;

  FreeAndNil(FSelectieContract);
  GridNotaID_CONTRACT.PopupControl := nil;
  if GridNotaID_CONTRACT.Visible then begin
    FSelectieContract := TfrmSelectieContract.Create(Self);
    FSelectieContract.RestoreContext;
    FSelectieContract.edNrContract.Properties.OnChange :=  edNrContractPropertiesChange;
    FSelectieContract.edDataContract.Properties.OnEditValueChanged := edDataContractPropertiesEditValueChanged;
    GridNotaID_CONTRACT.PopupControl := FSelectieContract;
  end;
  
  for I := 0 to GridNota.ColumnCount - 1 do
    if (GridNota.Columns[I].Visible) and (QryItemsi.FindField(GridNota.Columns[I].FieldName) = nil) then
        GridNota.Columns[I].Visible := False;
end;

procedure TfrmCulegereNote.ValidareCodRep(Sender: TField);
begin
//todo
//  if not Sender.IsNull then InternalValidateCont(Sender.AsString, -1, TreeRepartitori);
end;

procedure TfrmCulegereNote.GridNotaCustomDrawCell(Sender: TObject;
  ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  AColumn: TdxTreeListColumn; ASelected, AFocused, ANewItemRow: Boolean;
  var AText: String; var AColor: TColor; AFont: TFont;
  var AAlignment: TAlignment; var ADone: Boolean);

  function GetRealIndex: Integer;
   var I: Integer;
   begin
     Result := -1;
     for I := 0 to GridNota.VisibleColumnCount-1 do
       if GridNota.VisibleColumns[I] = AColumn then begin
          Result := I;
          Break;
       end;
   end;
   
begin
  if (AFocused) and (GetRealIndex = GridNota.FocusedColumn) then begin
     AColor := clBlue;
     AFont.Color := clWhite;
  end;
end;

procedure TfrmCulegereNote.QryItemsiAfterOpen(DataSet: TDataSet);
begin
  InitCulegere;
end;

procedure TfrmCulegereNote.Cmd_ModificaNotaExecute(Sender: TObject);
begin
  if not QryItemsi.IsEmpty then
     if MessageDlg('Doriti salvarea notei introduse inainte de a modifica o alta nota?'#13#10+
                   'In cazul validarii informatiile din ecranul de culegere vor fi pierdute !',
                   mtConfirmation, [mbYes, mbNo],0) = mrYes then Cmd_SalveazaNota.Execute;
  with TFrmListaNoteNew.Create(Self) do
    try
       edOperator.EditValue := IdUtilizator;
       edOperator.Visible := IsAdmin;
       SetModalForm;
       RefreshFilter;
       QryListaNote.Open;
       if ShowModal = mrOk then begin
          CurentIdNota := GetNotaFromBack;
          QryItemsi.Close;
          QryItemsi.Open;
       end;
    finally
       Free;
    end;
end;

procedure TfrmCulegereNote.BtnModificareClick(Sender: TObject);
begin
  Cmd_ModificaNota.Execute;
end;

procedure TfrmCulegereNote.SetNextControl;
  procedure SetActiveControl(AControl: TWinControl);
   begin
     if Self.Visible then AControl.SetFocus
     else Self.ActiveControl := AControl;
   end;
begin
  if ValueHasValue(edNrJurnal.EditValue) then
    if ValueHasValue(NumarNota.EditValue) then
       if ValueHasValue(DataDoc.EditValue) then SetActiveControl(GridNota)
       else SetActiveControl(DataDoc)
    else SetActiveControl(NumarNota)
  else SetActiveControl(edNrJurnal);
end;

procedure TfrmCulegereNote.edDataContractPropertiesEditValueChanged(
  Sender: TObject);
begin
  DBSetFieldValue(QryItemsi, 'DATA_CONTRACT', FSelectieContract.edDataContract.EditValue);
end;

procedure TfrmCulegereNote.edNrContractPropertiesChange(Sender: TObject);
begin
  DBSetFieldValue(QryItemsi, 'NR_CONTRACT', FSelectieContract.edNrContract.EditValue);
end;

procedure TfrmCulegereNote.edNrJurnalChange(Sender: TObject);
var
  lEditNota: String;
begin
  if not FIsInLoading then begin
    lEditNota := DBGetScallarFmt('exec spGetNumarNotaFromJurnal %d, %s',
        [IdUtilizator, ValueToStr(edNrJurnal.EditValue)]);
    if ValueHasValue(lEditNota) then begin
      if not QryItemsi.IsEmpty then
         case MessageDlg('Doriti salvarea notei introduse inainte de a modifica jurnalul?'#13#10+
                         '(Yes) salveaza nota curenta din ecran, (NO) notele din ecran vor fi mutate in jurnalul selectat, (Cancel) nu modifica nimic !',
                          mtConfirmation, [mbYes, mbNo, mbCancel],0) of
               mrYes    : Cmd_SalveazaNota.Execute;
               mrNo     : ;
               mrCancel : Abort;
         end;
      FIsInLoading := False;
      NumarNota.Text := lEditNota;
    end;
  end;
  UpdateItemsi;
  SetNextControl;
end;

procedure TfrmCulegereNote.NumarNotaKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if Key = VK_RETURN then SetNextControl;
end;

procedure TfrmCulegereNote.DataDocValidate(Sender: TObject;
  var ErrorText: String; var Accept: Boolean);
const IsErrorRaise : Boolean = False; 
begin
  if IsErrorRaise then Exit;
  IsErrorRaise  := False;
  Accept := Accept and Commondbvar.IsValidDate(DataDoc.EditValue);
  if Accept then UpdateItemsi;
  if not Accept then begin
    Accept := True;
    IsErrorRaise := True;
    ErrorText :='Data introdusa nu este valida';
    MessageDlg(ErrorText,  mtError, [mbOK], 0);
  end;
  SetNextControl;
  IsErrorRaise := False;  
end;

procedure TfrmCulegereNote.UpdateItemsi;
var
  OldPoz: TBookMark;
  lError: Variant;

  procedure CheckAndSetItems(FieldName: String; Value: Variant);
   begin
     if not (QryItemsi.State in [dsEdit, dsInsert]) then
        QryItemsi.Edit;
     QryItemsi.FieldByName(FieldName).Value := Value;
   end;
   
begin
  if FIsInLoading then Exit;

  if not ValueHasValue(DataDoc.EditValue) then begin
    raise EContaHandledError.Create('Data introdusa nu este valida');
    Abort;
  end;

  lError := DBGetScallarFmt('exec spCheckNota %s, %s, %s, %d',
                            [
                              ValueToStr(edNrJurnal.EditValue),
                              ValueToStr(NumarNota.EditValue),
                              ValueDateToStr(DataDoc.Date),
                              IdUtilizator]);
  if VarIsArray(lError) and ValueIsTrue(lError[0]) then
    MessageDlg(lError[1], mtError, [mbOk], 0);

  with QryItemsi do begin
    OldPoz := GetBookmark;
    FIsInLoading := True;
    try
       DisableControls;
       First;
       while not Eof do begin
          CheckAndSetItems('JURNAL' , edNrJurnal.EditValue);
          CheckAndSetItems('NRDOC'  , NumarNota.EditValue);
          if ValueHasValue(DataDoc.EditValue) then
             CheckAndSetItems('DATA', DataDoc.EditValue);
          if State = dsEdit then Post;
          Next;
       end;
    finally
       FIsInLoading := False;
       GotoBookmark(OldPoz);
       FreeBookmark(OldPoz);
       EnableControls;
    end;
  end;
end;

procedure TfrmCulegereNote.NumarNotaValidate(Sender: TObject;
  var ErrorText: String; var Accept: Boolean);
begin
  if Accept then UpdateItemsi;
end;

procedure TfrmCulegereNote.GridNotaREPARTITOR_CREDITGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
begin
  if (AText <> '') and IsNumeric(AText) then begin
      AText := FSelectieRepartitor.GetNumeByIdRepartitor(StrToInt(AText));
  end;
end;

procedure TfrmCulegereNote.GridNotaKeyPress(Sender: TObject; var Key: Char);
begin
  if (Key = '?') and ((GridNota.FocusedField = QryItemsi.FindField('REPARTITOR_CREDIT')) or (GridNota.FocusedField = QryItemsi.FindField('REPARTITOR_DEBIT')) or (GridNota.FocusedField = QryItemsi.FindField('DETALII_ANGAJAMENT')))
     and (Assigned(GridNota.InplaceEditor)) and
     (GridNota.InplaceEditor is TdxInplaceDropDownEdit) then
     TdxInplaceDropDownEdit(GridNota.InplaceEditor).DroppedDown := True;
  if ((GridNota.FocusedField = QryItemsi.FindField('REPARTITOR_CREDIT')) or (GridNota.FocusedField = QryItemsi.FindField('REPARTITOR_DEBIT')) or (GridNota.FocusedField = QryItemsi.FindField('DETALII_ANGAJAMENT')))
  then
     PostMessage(Handle, WM_DROPDOWN_IMAGECOLUMN, 0, 0);
end;

procedure TfrmCulegereNote.BtnValidareClick(Sender: TObject);
begin
  Cmd_SalveazaNota.Execute;
end;

procedure TfrmCulegereNote.Cmd_ValidareNotaExecute(Sender: TObject);
begin
  BtnValidare.Click;
end;

procedure TfrmCulegereNote.TotalNotaSumItemChanged(Sender: TObject;
  Item: TDBSum);
begin
  LbTotal.Caption := 'Total Nota : '+FormatFloat('#,0.00', Item.SumValue);
end;

type TCrackAtsTree = class(TdxTreeList);

function TfrmCulegereNote.GetNodeByVal(ATree: TCustomdxTreeList;
  AColIndex: Integer; AVal: String;
  ASearchType: TdxTLSearchType): TdxTreeListNode;
var OldSearch: TdxTLSearchType;
    lNode    : TdxTreeListNode;
begin
  Result := nil;
  with TCrackAtsTree(ATree) do begin
    OldSearch := SearchType;
    try
       SearchType := stExact;
       if FindNodeByText(AColIndex, AVal, sdNone, lNode) then Result := lNode;
    finally
       SearchType := OldSearch;
    end;
  end;
end;

procedure TfrmCulegereNote.GridNotaBUGETCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
begin
  if Accept then begin
     { Modificam Informatiile }
    FDefalcareBuget.UpdateSelected;
     if not (QryItemsi.State in [dsEdit, dsInsert]) then QryItemsi.Edit;
     if FDefalcareBuget.IdAngajament > 0 then QryItemsi.FieldByName('ID_ANGAJAMENTE_DEFALCARE').AsInteger := FDefalcareBuget.IdAngajament;
     if FDefalcareBuget.IdOrdonantare > 0 then QryItemsi.FieldByName('ID_ORDONANTARE_DEFALCARE').AsInteger := FDefalcareBuget.IdOrdonantare;
     if FDefalcareBuget.IdUnitati <= 0 then
       QryItemsi.FieldByName('ID_OI_UNITATI').Value := Null
     else
       QryItemsi.FieldByName('ID_OI_UNITATI').Value := FDefalcareBuget.IdUnitati;
     if FDefalcareBuget.IdProiecte <= 0 then
       QryItemsi.FieldByName('ID_OI_PROIECTE').Value := Null
     else
       QryItemsi.FieldByName('ID_OI_PROIECTE').Value := FDefalcareBuget.IdProiecte;
     QryItemsi.FieldByName('COD_FUNCTIONAL').AsString := FDefalcareBuget.CodFunctional;
     QryItemsi.FieldByName('COD_ECONOMIC').AsString := FDefalcareBuget.CodEconomic;
     QryItemsi.FieldByName('DETALII_ANGAJAMENT').AsString := FDefalcareBuget.Descriere;
     Accept := False;
  end;
end;

procedure TfrmCulegereNote.GridNotaBUGETInitPopup(Sender: TObject);
begin
  { Initializam Latime si inaltimea Controlului }
 {
  with QryItemsi do begin
    if FieldByName('ID_ANGAJAMENTE_DEFALCARE').AsInteger > 0 then
       FDefalcareBuget.IdAngajament := FieldByName('ID_ANGAJAMENTE_DEFALCARE').AsInteger
    else FDefalcareBuget.IdFurnizor := FieldByName('REPARTITOR_CREDIT').AsInteger;
    FDefalcareBuget.CodFunctional   := FieldByName('COD_FUNCTIONAL').AsString;
  end;}
end;

procedure TfrmCulegereNote.FormDestroy(Sender: TObject);
begin
  FRepartitorCache.Free;
  ExitSingleUser;
end;

procedure TfrmCulegereNote.GridNotaREPARTITOR_DEBITInitPopup(Sender: TObject);
begin
  if FilterGestiuni then Exit;
{  if Sender = GridNotaREPARTITOR_DEBIT then FrmData.QryRepartitori.Filter := 'GESTINT=True'
  else FrmData.QryRepartitori.Filter := 'GESTINT=False';
  FrmData.QryRepartitori.Filtered := True;}
end;

procedure TfrmCulegereNote.AplicaContCreditor(ClasaEconomica, ClasaFunc : String);
var lFound: Boolean;
    lCont : String;
begin
  if pos('600', Trim(QryItemsi.FieldByName('CONTD').AsString)) = 1 then begin
     with Conturi do begin
       First;
       lFound := False;
       while not Eof do begin
         lFound := (ClasaEconomica = '') or (pos(Trim(FieldByName('COD_ECONOMIC').AsString), ClasaEconomica) = 1);
         if lFound then
            lFound := (Trim(FieldByName('COD_FUNCTIONAL').AsString)='') or (ClasaFunc = '') or (pos(Trim(FieldByName('COD_FUNCTIONAL').AsString), ClasaFunc) = 1);
         if lFound  then begin
            lCont := Trim(FieldByName('CONT_REAL').AsString);
            Break;
         end;
         Next;
       end;
       if lFound then begin
         QryItemsi.FieldByName('CONTD').AsString := lCont;
         QryItemsi.FieldByName('CONT_DEBT').AsString := lCont;
       end;
     end;
  end;
end;

procedure TfrmCulegereNote.chkRepVisibleClick(Sender: TObject);
begin
  GridNotaREPARTITOR_DEBIT.Visible := chkRepVisible.Checked;
  GridNotaREPARTITOR_CREDIT.Visible := chkRepVisible.Checked;
end;

procedure TfrmCulegereNote.FormShow(Sender: TObject);
begin
  chkRepVisibleClick(nil);
end;

procedure TfrmCulegereNote.FormKeyUp(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if (Key = VK_SPACE) and (ssCtrl in Shift) and (ssShift in Shift) then btnModificare.Click;
end;

procedure TfrmCulegereNote.TreePlanCustomDrawCell(Sender: TObject;
  ACanvas: TCanvas; ARect: TRect; ANode: TdxTreeListNode;
  AColumn: TdxTreeListColumn; ASelected, AFocused, ANewItemRow: Boolean;
  var AText: String; var AColor: TColor; AFont: TFont;
  var AAlignment: TAlignment; var ADone: Boolean);

  function GetValidColIndex(ACol: TdxTreeListColumn; Tree: TdxDBTreeList): Integer;
  var i: Integer;
  begin
    Result := - 1;
    for i := 0 to Tree.ColumnCount - 1 do
    begin
      Inc(Result);
      if Tree.Columns[i] = ACol then Break;
    end;
  end;

var
  S: String;

begin
  if ANode.HasChildren then AFont.Color := clGray;
  if TdxDBTreeListColumn(AColumn).FieldName = 'FCTCONT' then
  begin
    s := ANode.Strings[GetValidColIndex(AColumn, TreePlan)];
    if s = 'B' then AColor := clSkyBlue
    else if s = 'C' then AColor := clLime
         else if s = 'D' then AColor := clFuchsia;
  end;
end;

procedure TfrmCulegereNote.GridNotaEditing(Sender: TObject;
  Node: TdxTreeListNode; var Allow: Boolean);
begin
  if GridNota.VisibleColumns[gridnota.FocusedColumn] = GridNotaCONT_DEBT then
    if QryItemsi.FieldByName('CONT_DEBT').AsString = 'X' then Allow := False;
  if GridNota.VisibleColumns[gridnota.FocusedColumn] = GridNotaCONT_CRED then
    if QryItemsi.FieldByName('CONT_CRED').AsString = 'X' then Allow := False;
end;

procedure TfrmCulegereNote.GridNotaChangeColumn(Sender: TObject;
  Node: TdxTreeListNode; Column: Integer);
begin
  if (GridNota.VisibleColumns[Column] is TdxDBTreeListImageColumn) and chkDeschidAutomat.Checked then
     PostMessage(Handle, WM_DROPDOWN_IMAGECOLUMN, 0, 0);
  if (GridNota.VisibleColumns[Column] is TdxDBTreeListPopupColumn) and chkDeschidAutomat.Checked then
     PostMessage(Handle, WM_DROPDOWN_IMAGECOLUMN, 0, 0);
end;

procedure TfrmCulegereNote.WmDropDownImgColumn(var Message: TMessage);
begin
  if (Assigned(GridNota.InplaceEditor)) and
     (GridNota.InplaceEditor.IsFocused) then begin
     if GridNota.InplaceEditor is TdxInplaceDropDownEdit then
        TdxInplaceDropDownEdit(GridNota.InplaceEditor).DroppedDown := True;
     case Message.WParam of
       0: ;
       1: ;
     end;
  end;

end;

procedure TfrmCulegereNote.DataDocKeyDown(Sender: TObject; var Key: Word;
  Shift: TShiftState);
begin
  if Key = VK_RETURN then SetNextControl;
end;

procedure TfrmCulegereNote.btnStergeClick(Sender: TObject);
begin
  DBExecSQLFmt('exec spNoteGolireItemsi %d', [IdUtilizator]);
  QryItemsi.Refresh;
  SetNextControl;
end;

procedure TfrmCulegereNote.GridNotaCONT_DEBTPopup(Sender: TObject;
  const EditText: String);
begin
  InternalPositioning(EditText, TreePlan);
end;

procedure TfrmCulegereNote.GridNotaCONT_CREDPopup(Sender: TObject;
  const EditText: String);
begin
  InternalPositioning(EditText, TreePlan);
end;

procedure TfrmCulegereNote.GridNotaREPARTITOR_DEBITPopup(Sender: TObject;
  const EditText: String);
begin
 // InternalPositioning(EditText, TreeRepartitori);
   FSelectieRepartitor.TreeRepartitori.CancelSearching;
   if (EditText <> '') and IsNumeric(EditText) then
     FSelectieRepartitor.IdRepartitor := StrToInt(EditText);
end;

procedure TfrmCulegereNote.GridNotaBUGETPopup(Sender: TObject;
  const EditText: String);
var
    lFurnizor, lIdAng, lIdOrd,
    lIdProiect, lIdUnitate : Integer;
    lCodF, lCodEc: String;
    lDataExecutie : TDateTime;
    lSuma : Currency;
    lInv : Integer;
begin
  FDefalcareBuget.ModalResult := mrNone;
  with QryItemsi do begin
  lFurnizor := -1;
  lIdAng := -1;
  lIdOrd := -1;
  lCodEc := '';
  lCodF := '';
  lIdProiect := -1;
  lIdUnitate := -1;
  lDataExecutie := FieldByName('DATA').AsDateTime;
  if FieldByName('ID_ANGAJAMENTE_DEFALCARE').AsInteger > 0 then
      lIdAng := FieldByName('ID_ANGAJAMENTE_DEFALCARE').AsInteger;

  if FieldByName('ID_ORDONANTARE_DEFALCARE').AsInteger > 0 then
      lIdOrd := FieldByName('ID_ORDONANTARE_DEFALCARE').AsInteger;


    if FieldByName('ID_OI_UNITATI').AsInteger > 0 then
      lIdUnitate := FieldByName('ID_OI_UNITATI').AsInteger;
    if FieldByName('ID_OI_PROIECTE').AsInteger > 0 then
      lIdProiect := FieldByName('ID_OI_PROIECTE').AsInteger;
    lCodF := FieldByName('COD_FUNCTIONAL').AsString;
    lCodEc := FieldByName('COD_ECONOMIC').AsString;
    lSuma := FieldByName('Valoare').AsCurrency;
    FDefalcareBuget.SumaCautare := lSuma;

    if (Length(FieldByName('CONT_CRED').AsString) > 0 ) and
       (FieldByName('CONT_CRED').AsString[1] in ['4']) and
       (FieldByName('REPARTITOR_CREDIT').AsInteger > 0) then begin
      lFurnizor := FieldByName('REPARTITOR_CREDIT').AsInteger;
      lInv := FieldByName('REPARTITOR_DEBIT').AsInteger;
    end
    else
    if (Length(FieldByName('CONT_DEBT').AsString) > 0 ) and
       (FieldByName('CONT_DEBT').AsString[1] in ['4']) and
       (FieldByName('REPARTITOR_DEBIT').AsInteger > 0) then begin
      lFurnizor := FieldByName('REPARTITOR_DEBIT').AsInteger;
      lInv := FieldByName('REPARTITOR_CREDIT').AsInteger;
    end
    else
    if (FieldByName('REPARTITOR_CREDIT').AsInteger > 0) then begin
       lFurnizor := FieldByName('REPARTITOR_CREDIT').AsInteger;
       lInv := FieldByName('REPARTITOR_DEBIT').AsInteger;       
    end
    else
    if (FieldByName('REPARTITOR_DEBIT').AsInteger > 0) then begin
       lFurnizor := FieldByName('REPARTITOR_DEBIT').AsInteger;
       lInv := FieldByName('REPARTITOR_CREDIT').AsInteger;       
    end;
    FDefalcareBuget.ObiectivCautare := lInv;

    FDefalcareBuget.PrepareCulegere(lFurnizor, lCodF, lCodEc, lIdAng, lIdOrd,
      lIdUnitate, lIdProiect, lDataExecutie);
  end;
 end;

procedure TfrmCulegereNote.CopyToItemsi;
begin
  DBExecSQLFmt('exec [spNoteCopyNr] %s, %s, %s, %s, %d', [ValueToStr(Nr), ValueToStr(NumarNota.EditValue), ValueToStr(DataDoc.EditValue), ValueToStr(edNrJurnal.EditValue), IdUtilizator]);
  QryItemsi.Close;
  QryItemsi.Open;
end;

procedure TfrmCulegereNote.btnCopyNotaClick(Sender: TObject);
begin
  if not ValueHasValue(edNrJurnal.EditValue) then begin
     edNrJurnal.SetFocus;
     raise EContaHandledError.Create('Selectati jurnalul la care apartine nota curenta !');
  end;
  if not ValueHasValue(NumarNota.EditValue) then begin
     NumarNota.SetFocus;
     raise EContaHandledError.Create('Introduceti numarul notei contabile !');
  end;
  if Trim(DataDoc.Text) = '' then begin
     DataDoc.SetFocus;
     raise EContaHandledError.Create('Introduceti data notei contabile !');
  end;

  with TFrmListaNoteNew.Create(Self) do
    try
       ForceCopy := True;
       edOperator.EditValue := IdUtilizator;
       edOperator.Visible := IsAdmin;
       SetModalForm;
       RefreshFilter;
       QryListaNote.Open;
       if ShowModal = mrOk then begin
          if (GridIstoricNote.Controller.FocusedRecord <> nil) and (GridIstoricNote.Controller.FocusedRecord.IsData) then begin
            CopyToItemsi(VarToStr(GridIstoricNote.Controller.FocusedRecord.Values[GridIstoricNoteID.Index]));
            QryItemsi.Close;
            QryItemsi.Open;
          end
       end;
    finally
       Free;
    end;
end;

procedure TfrmCulegereNote.GridNotaEXPLICATIEButtonClick(Sender: TObject);
begin
  //deschidem editor in ecran
  
end;

procedure TfrmCulegereNote.SetDetaliiOnEmpty;
begin
  if QryItemsi.FieldByName('ID_ANGAJAMENTE_DEFALCARE').AsInteger > 0 then Exit;
    DBSetFieldValue(QryItemsi, 'DETALII_ANGAJAMENT', 'AG: ' +
      QryItemsi.FieldByName('COD_FUNCTIONAL').AsString+'/'+QryItemsi.FieldByName('COD_ECONOMIC').AsString );
end;

procedure TfrmCulegereNote.GridNotaCOD_DOCUMENTCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
var lNode: TdxDBTreeListNode;
    lEditabil: Boolean;
begin
  with TdxDBTreeListPopupColumn(Sender) do
    if Accept then begin
       lNode := TdxDBTreeListNode(TdxDBTreeList(PopupControl).FocusedNode);
       if Assigned(lNode) then begin
          lEditabil := Field.DataSet.State in [dsEdit, dsInsert];
          if not lEditabil then Field.DataSet.Edit;
          Field.AsString := lNode.Strings[TreeTipDocTIP_DOC.Index];
          Field.DataSet.Post;
          if lEditabil then Field.DataSet.Edit;
          Text := lNode.Strings[TreeTipDocTIP_DOC.Index];
          Accept := False;
       end;
    end;
end;

procedure TfrmCulegereNote.GridNotaCOD_DOCUMENTPopup(Sender: TObject;
  const EditText: String);
begin
  InternalPositioning(EditText, TreeTipDoc);
end;

procedure TfrmCulegereNote.GridNotaCOD_DOCUMENTValidate(Sender: TObject;
  var ErrorText: String; var Accept: Boolean);
var
   lValue : Variant;
   lVal : String;
begin
  if Accept and Assigned(GridNota.InplaceEditor) and
     (GridNota.InplaceEditor.IsVisible) and
      (GridNota.InplaceEditor is TdxInplaceTextEdit)
      and (TdxInplaceTextEdit(GridNota.InplaceEditor).Text <> '')
   then begin
     lVal := TdxInplaceTextEdit(GridNota.InplaceEditor).Text;
     InternalValidateExact(lVal, TreeTipDoc, lValue);
     if (lValue <> null) and (lVal <> lValue) then begin
       TdxInplaceTextEdit(GridNota.InplaceEditor).Text := lValue;
       TdxInplaceTextEdit(GridNota.InplaceEditor).ValidateEdit;
       TdxInplaceTextEdit(GridNota.InplaceEditor).Modified := True;
     end
  end;
end;

procedure TfrmCulegereNote.ValidareTipDoc(Sender: TField);
begin
  if not Sender.IsNull then InternalValidateCont(Trim(Sender.AsString), -1, TreeTipDoc);
  ValidareDateDocument(Sender);
end;

procedure TfrmCulegereNote.InternalValidateExact(Val: String;
  Tree: TdxDbTreeList; var InternalValue: Variant; const AllowChildren : Boolean = False; const ForceById: Word=0;
  const lTag: Integer=-1);
var SelNode : TdxDBTreeListNode;
    lSearchC : TdxDBTreeListColumn;
    lOldSearchType : TdxTLSearchType;
begin
  lSearchC := FindColumnByTag(Tree, lTag);
  Tree.EndSearch;
  { Incercam sa gasim nodul posibil }
  SelNode := nil;
  lOldSearchType := Tree.SearchType;
  Tree.SearchType := stExact;
  if (ForceById <> 1)  and (lSearchC <> nil) then begin
      TCrackAtsTree(Tree).FindNodeByText(lSearchC.Index, Val, sdNone, TdxTreeListNode(SelNode));
  end
  else
     SelNode := Tree.FindNodeByKeyValue(Val);
  if Assigned(SelNode) and (AllowChildren or not(SelNode.HasChildren)) then begin
    if ForceById = 2 then InternalValue := SelNode.Id
    else
       if (lSearchC <> nil) then InternalValue := SelNode.Values[lSearchC.Index]
                            else InternalValue := SelNode.Id;
  end
  else InternalValue := null;
  Tree.SearchType := lOldSearchType;
end;

procedure TfrmCulegereNote.Cmd_CopiereContinutExecute(Sender: TObject);
var
   lValue : Variant;
begin
  if Assigned(GridNota.FocusedNode) and Assigned(GridNota.FocusedField) and
     (GridNota.FocusedNode.GetPrevSibling <> nil) then
    if not SameText(GridNota.FocusedField.FieldName, 'DETALII_ANGAJAMENT') then begin
      lValue := GridNota.FocusedNode.GetPrevSibling.Values[GridNota.FocusedAbsoluteIndex];
      DBSetFieldValue(QryItemsi, GridNota.FocusedField.FieldName, lValue);
    end;
end;


procedure TfrmCulegereNote.LoadPreluareMeniu;
var
  lTmpItem: TMenuItem;
  lDataSet: TDataSet;
begin
  lDataSet := DBNewQuery('exec [spCNoteModel]');
  try
    lDataSet.Open;
    while not lDataSet.Eof do begin
      lTmpItem := TMenuItem.Create(mnuImportNote);
      lTmpItem.Caption := Trim(lDataSet.FieldByName('Denumire').AsString);
      lTmpItem.OnClick := DoPreluareNota;
      lTmpItem.Tag     := lDataSet.FieldByName('modul').AsInteger;
      mnuImportNote.Add(lTmpItem);
      lDataSet.Next;
    end;
  finally
    lDataSet.Free;
  end;
end;

procedure TfrmCulegereNote.DoPreluareNota(Sender: TObject);
begin
  DBExecSQLFmt('exec [spCnoteModelPreluare] %d, %s, %s, %s, %d',
                [
                  TMenuItem(Sender).Tag,
                  ValueToStr(NumarNota.EditValue),
                  ValueToStr(DataDoc.Date),
                  ValueToStr(edNrJurnal.EditValue),
                  idUtilizator
                ] );
  QryItemsi.Refresh;

end;

function TfrmCulegereNote.IsInPeriod(aDate: TDateTime): Boolean;
begin
  Result := (aDate >= MinData) and (aDate <= MaxData);
end;

procedure TfrmCulegereNote.ValidareDataDocument(Sender: TField);
begin
  if Sender.Value <> null then
   Sender.DataSet.FieldByName('DATA_SCADENTA').AsDateTime :=  IncDay(Sender.AsDateTime, 30);
  ValidareDateDocument(Sender); 
end;

procedure TfrmCulegereNote.GridNotaREPARTITOR_DEBITCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
begin
  if Accept then begin
    Text := IntToStr(FSelectieRepartitor.IdRepartitor);
    AddRepartitorToCache(FSelectieRepartitor.IdRepartitor);
  end;
end;

procedure TfrmCulegereNote.RepOnSelectCloseEvent(Sender: TObject;
  Accept: Boolean);
begin
  (GetParentForm(TControl(Sender)) as TdxPopupEditForm).ClosePopup(Accept);
end;

procedure TfrmCulegereNote.SetupColumnsEditors;
begin

  //setam proprietatile pentru popuri

  // setam popurile pentru coloane
  GridNotaCOD_FUNCTIONAL.PopupControl := frmRepo.cxTreeFunctional;
  GridNotaCOD_FUNCTIONAL.OnPopup := GridNotaGeneralOnPopup;
  GridNotaCOD_FUNCTIONAL.OnCloseUp := GridNotaGeneralOnCloseUp;
  GridNotacod_functional_d.PopupControl := GridNotaCOD_FUNCTIONAL.PopupControl;
  GridNotacod_functional_d.OnPopup := GridNotaGeneralOnPopup;
  GridNotacod_functional_d.OnCloseUp := GridNotaGeneralOnCloseUp;
  GridNotacod_functional_c.PopupControl := GridNotaCOD_FUNCTIONAL.PopupControl;
  GridNotacod_functional_c.OnPopup := GridNotaGeneralOnPopup;
  GridNotacod_functional_c.OnCloseUp := GridNotaGeneralOnCloseUp;

  GridNotaCOD_ECONOMIC.PopupControl := frmRepo.cxTreeEconomic;
  GridNotaCOD_ECONOMIC.OnPopup := GridNotaGeneralOnPopup;
  GridNotaCOD_ECONOMIC.OnCloseUp := GridNotaGeneralOnCloseUp;
  GridNotacod_economic_d.PopupControl := GridNotaCOD_ECONOMIC.PopupControl;
  GridNotacod_economic_d.OnPopup := GridNotaGeneralOnPopup;
  GridNotacod_economic_d.OnCloseUp := GridNotaGeneralOnCloseUp;
  GridNotacod_economic_c.PopupControl := GridNotaCOD_ECONOMIC.PopupControl;
  GridNotacod_economic_c.OnPopup := GridNotaGeneralOnPopup;
  GridNotacod_economic_c.OnCloseUp := GridNotaGeneralOnCloseUp;

  GridNotaid_oi_unitati.PopupControl := frmRepo.cxTreeUnitati;
  GridNotaid_oi_unitati.OnPopup := GridNotaGeneralOnPopup;
  GridNotaid_oi_unitati.OnCloseUp := GridNotaGeneralOnCloseUp;
  GridNotaid_oi_unitati.OnGetText := GridNotaGeneralOnGetText;
  GridNotaid_oi_unitati_d.PopupControl := GridNotaid_oi_unitati.PopupControl;
  GridNotaid_oi_unitati_d.OnPopup := GridNotaGeneralOnPopup;
  GridNotaid_oi_unitati_d.OnCloseUp := GridNotaGeneralOnCloseUp;
  GridNotaid_oi_unitati_d.OnGetText := GridNotaGeneralOnGetText;
  GridNotaid_oi_unitati_c.PopupControl := GridNotaid_oi_unitati.PopupControl;
  GridNotaid_oi_unitati_c.OnPopup := GridNotaGeneralOnPopup;
  GridNotaid_oi_unitati_c.OnCloseUp := GridNotaGeneralOnCloseUp;
  GridNotaid_oi_unitati_c.OnGetText := GridNotaGeneralOnGetText;

  GridNotaid_oi_proiecte.PopupControl := frmRepo.cxTreeProiecte;
  GridNotaid_oi_proiecte.OnPopup := GridNotaGeneralOnPopup;
  GridNotaid_oi_proiecte.OnCloseUp := GridNotaGeneralOnCloseUp;
  GridNotaid_oi_proiecte.OnGetText := GridNotaGeneralOnGetText;
  GridNotaid_oi_proiecte_d.PopupControl := GridNotaid_oi_proiecte.PopupControl;
  GridNotaid_oi_proiecte_d.OnPopup := GridNotaGeneralOnPopup;
  GridNotaid_oi_proiecte_d.OnCloseUp := GridNotaGeneralOnCloseUp;
  GridNotaid_oi_proiecte_d.OnGetText := GridNotaGeneralOnGetText;
  GridNotaid_oi_proiecte_c.PopupControl := GridNotaid_oi_proiecte.PopupControl;
  GridNotaid_oi_proiecte_c.OnPopup := GridNotaGeneralOnPopup;
  GridNotaid_oi_proiecte_c.OnCloseUp := GridNotaGeneralOnCloseUp;
  GridNotaid_oi_proiecte_c.OnGetText := GridNotaGeneralOnGetText;
end;

procedure TfrmCulegereNote.GridNotaGeneralOnPopup(Sender: TObject;
  const EditText: String);
var
  lColumName : String;
  lTree : TcxDBTreeList;
begin
   if Sender is TdxDBTreeListPopupColumn then begin
       lTree := TcxDBTreeList(TdxDBTreeListPopupColumn(Sender).PopupControl);
       if cxFindColumnByTag(lTree, -1) <> nil then
         lColumName := cxFindColumnByTag(lTree, -1).DataBinding.FieldName
       else
         lColumName := '';
       InternalPositioning(EditText, lTree, lColumName);
   end
end;

procedure TfrmCulegereNote.GridNotaID_CONTRACTCloseUp(Sender: TObject;
  var Text: string; var Accept: Boolean);
var
  lIdContract : Integer;
  lDataContract : TDateTime;
  lNrContract : String;
begin
  FSelectieContract.FilterContractByDepartament(-1);
  FSelectieContract.FilterContractByPrestator(-1);
  if Accept then
  begin
    lNrContract := FSelectieContract.NrContract;
    lDataContract := FSelectieContract.DataContract;
    lIdContract := FSelectieContract.IdContract;
    if lIdContract <> 0 then begin
      if ValueIsTrue(DBGetSetare('integrareOne')) and (lIdContract < 0) then begin
        DBSetFieldValue(QryItemsi, 'ref_One_TipProgram', FSelectieContract.refOnTipProgram);
        DBSetFieldValue(QryItemsi, 'ref_One_Contract', FSelectieContract.refOnContract);
        DBSetFieldValue(QryItemsi, 'ID_CONTRACTE', Null);
      end
      else begin
        DBSetFieldValue(QryItemsi, 'ID_CONTRACTE', lIdContract);
      end;
      DBSetFieldValue(QryItemsi,'NR_CONTRACT', FSelectieContract.NrContract);
      DBSetFieldValue(QryItemsi,'DATA_CONTRACT', FSelectieContract.DataContract);
      Text := IntToStr(lIdContract);
      SetNextControl;
    end;
  end;

end;

procedure TfrmCulegereNote.GridNotaID_CONTRACTGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: string);
begin
  if Assigned(ANode) then
    AText := 'Contract Nr. ' + ValueSafeToStr(ANode.Values[GridNotaNR_CONTRACT.Index]) + ' din ' +
             FormatDateTime('dd.mm.yyyy', ValueSafeToDateTime(ANode.Values[GridNotaDATA_CONTRACT.Index]));
end;

procedure TfrmCulegereNote.GridNotaID_CONTRACTInitPopup(Sender: TObject);
begin
  FSelectieContract.FilterContractByDepartament(-1);
  FSelectieContract.FilterContractByPrestator(-1, False);
  FSelectieContract.IdContract := QryItemsi.FieldByName('ID_CONTRACTE').AsInteger;
end;

procedure TfrmCulegereNote.GridNotaGeneralOnCloseUp(Sender: TObject;
  var Text: String; var Accept: Boolean);
var
   lNode: TcxDBTreeListNode;
   lTree : TcxDBTreeList;
   lEditabil: Boolean;
   lColumn : TcxDBTreeListColumn;
begin
  with TdxDBTreeListPopupColumn(Sender) do
    if Accept then begin
       lTree := TcxDBTreeList(PopupControl);
       lNode := TcxDBTreeListNode(lTree.FocusedNode);
       if Assigned(lNode) then begin
          lColumn := cxFindColumnByTag(lTree, -1);
          lEditabil := Field.DataSet.State in [dsEdit, dsInsert];
          if not lEditabil then Field.DataSet.Edit;
          if lColumn <> nil then
            Field.Value := lNode.Values[lColumn.ItemIndex]
          else
            Field.Value := lNode.KeyValue;
          Field.DataSet.Post;
          if lEditabil then Field.DataSet.Edit;
          Text := Field.Value;
          Accept := False;
       end;
    end;
end;

procedure TfrmCulegereNote.InternalValidateCont(Val: String;
  ATree: TcxDbTreeList; var InternalValue: Variant; const AllowChildren : Boolean = False);
var
 lMustDrop      : Boolean;
 lNode          : TcxDBTreeListNode;
 lSearchColumn  : TcxDBTreeListColumn;


  function FindNode(const AString: String): TcxDBTreeListNode;
  begin
    if Assigned(lSearchColumn) then
      Result := TcxDBTreeListNode(ATree.FindNodeByText(AString, lSearchColumn))
    else
      Result := ATree.FindNodeByKeyValue(AString)
  end;

  procedure SetFirstChildVisible;
  begin
    if Assigned(lNode) then begin
      while lNode.Count > 0 do
        lNode := TcxDBTreeListNode(lNode.Items[0]);
      lNode.Focused := True;
      lNode.MakeVisible;
    end;
  end;

begin
  ATree.CancelSearching;
  InternalValue := Val;
  lSearchColumn := cxFindColumnByTag(ATree, -1);
  lMustDrop     := (Val = '') or (Val = '?');
  if not lMustDrop then begin
    lNode := FindNode(Val);
    { cautam de la cel mai mare string catre cel mai mic }
    while not Assigned(lNode) and (Length(Val) > 0) do begin
      if Assigned(lNode) and not lMustDrop then
        if Assigned(lSearchColumn) then
          InternalValue := lNode.Values[lSearchColumn.ItemIndex]
        else
          InternalValue := lNode.KeyValue
      else begin
        if not lMustDrop then lMustDrop := True;
        Delete(Val, Length(Val), 1);
      end;
    end;
    if not lMustDrop then
      lMustDrop := not Assigned(lNode) or (not AllowChildren and lNode.HasChildren);
    if lMustDrop and Assigned(lNode) then
      SetFirstChildVisible;
  end
  else begin
    lNode := TcxDBTreeListNode(ATree.FocusedNode);
    SetFirstChildVisible;
  end;

  if lMustDrop and Assigned(ATree.InplaceEditor) then begin
    if Assigned(lNode) and lNode.HasChildren then lNode.Expand(True);
    SetFirstChildVisible;
    ATree.SearchingText := Val;
    SendMessage(ATree.InplaceEditor.Handle, CM_DROPDOWNPOPUPFORM, 0, 0);
    Abort;
  end;
end;

procedure TfrmCulegereNote.ChangeContBuget(Sender: TField);
var
  I : Integer;
begin
  for I := 0 to Sender.DataSet.FieldCount - 1 do
    if (Sender.DataSet.Fields[I] <> Sender) and (Pos(UpperCase(Sender.FieldName), UpperCase(Sender.DataSet.Fields[I].FieldName)) > 0) then
      Sender.DataSet.Fields[I].Value := Sender.Value;
end;

procedure TfrmCulegereNote.GridNotaGeneralOnGetText(Sender: TObject;
  ANode: TdxTreeListNode; var AText: String);
var
  lTree : TcxDBTreeList;
begin
  if (AText <> '') and IsNumeric(AText) then begin
    if (Sender is TdxDBTreeListPopupColumn) and (TdxDBTreeListPopupColumn(Sender).PopupControl is TcxDBTreeList) then begin
      lTree := TdxDBTreeListPopupColumn(Sender).PopupControl as TcxDBTreeList;
      AText := frmRepo.GetDescriereOnTree(lTree, StrToInt(AText));
    end;
  end;
end;

procedure TfrmCulegereNote.ValidareNrDocument(Sender: TField);
begin
  ValidareDateDocument(Sender);
end;

procedure TfrmCulegereNote.ValidareDateDocument(Sender: TField);
var
  lDataSet : TDataSet;
begin
  lDataSet := Sender.DataSet;
  if lDataSet = nil then Exit;

  if ValueHasValue(lDataSet['NR_DOCUMENT']) and ValueHasValue(lDataSet['DATA_DOCUMENT']) and ValueHasValue(lDataSet['COD_DOCUMENT']) then begin
     qryVerificaDocument.Close;
     qryVerificaDocument.Open;
     if qryVerificaDocument.Fields[0].AsInteger <> 0 then
       raise EContaHandledError.Create( qryVerificaDocument.Fields[1].AsString);
  end;
end;

procedure TfrmCulegereNote.NumarNotaPropertiesValidate(Sender: TObject;
  var DisplayValue: Variant; var ErrorText: TCaption; var Error: Boolean);
begin
  if not Error then UpdateItemsi;
end;

procedure TfrmCulegereNote.DataDocPropertiesValidate(Sender: TObject;
  var DisplayValue: Variant; var ErrorText: TCaption; var Error: Boolean);
begin
  Error := Error or not ValueHasValue(DataDoc.EditingValue);
  if not Error then
    UpdateItemsi
  else
    ErrorText := 'Data introdusa nu este valida';
end;

procedure TfrmCulegereNote.AddRepartitorToCache(const ARepID: Variant);
var
  lRepID : NativeInt;
begin
  lRepID := ValueSafeToInt(ARepID);
  if lRepID <> 0 then
    if FRepartitorCache.IndexOfObject(TObject(lRepID)) = -1 then
      FRepartitorCache.AddObject( FSelectieRepartitor.GetNumeByIdRepartitor(lRepID), TObject(lRepID) );
end;

function TfrmCulegereNote.GetRepartitorFromCache(
  const ARepID: Variant): String;
var
  lRepID: Integer;
  lIndex: Integer;
begin
  lRepID := ValueSafeToInt( ARepID );
  if lRepID <> 0 then
    lIndex := FRepartitorCache.IndexOfObject(TObject(lRepID))
  else
    lIndex := -1;
  if lIndex > -1 then
    Result := FRepartitorCache[lIndex]
  else
    Result := '';
end;

procedure TfrmCulegereNote.GridNotaBUGETCloseQuery(Sender: TObject;
  var CanClose: Boolean);
begin
  CanClose := FDefalcareBuget.CanSelectDefalcare;
end;

end.
