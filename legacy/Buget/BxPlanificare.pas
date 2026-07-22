unit BxPlanificare;

interface
                       
uses
  Windows, Messages, SysUtils, Variants, Classes, Graphics, Controls, Forms, Menus,
  cxSplitter, ImgList, dxmdaset, DB, ZDataSet, ToolWin, ComCtrls, cxDropDownEdit,
  cxImageComboBox, StdCtrls, cxButtons, cxMaskEdit, cxSpinEdit, cxControls, cxContainer,
  cxEdit, cxTextEdit, ExtCtrls, dxNavBarCollns, cxClasses, dxNavBarBase, dxNavBar,
  BxPlanContainer, cxGroupBox, cxStyles, cxInplaceContainer, cxVGrid, cxDBVGrid, cxCalendar,
  cxCheckBox, Dialogs, cxDBEdit, cxLookAndFeelPainters, cxGraphics, cxMemo, ZAbstractRODataset,
  ZAbstractDataset, ZSqlUpdate, cxTL, cxTLdxBarBuiltInMenu, cxDBTL,  cxLookupEdit, cxDBLookupEdit,
  cxDBLookupComboBox, cxPC,  cxLookAndFeels, cxCustomData, cxTLData, cxCurrencyEdit,
  frmFisaDetaliuUnit, BugetCompareUnit, dxBarBuiltInMenu,
  cxDataControllerConditionalFormattingRulesManagerDialog,
  dxScrollbarAnnotations;

const
  WM_REFRESHBUGET = WM_USER+2;

type
  PTipFisa = ^TTipFisa;
  TTipFisa = record
    Id : Integer;
    Denumire : String;
    SQLProc : string;
  end;

  TfrmBxPlanificare = class(TForm)
    pnAllForm: TPanel;
    Panel2: TPanel;
    Panel4: TPanel;
    Panel5: TPanel;
    SelectDefalcare: TcxTabControl;
    NavPanel: TdxNavBar;
    NavPanelGroup1: TdxNavBarGroup;
    NavPanelGroup2: TdxNavBarGroup;
    NavPanelGroup3: TdxNavBarGroup;
    NavPanelGroup1Control: TdxNavBarGroupControl;
    Label2: TLabel;
    Bevel1: TBevel;
    Label11: TLabel;
    Label3: TLabel;
    Label4: TLabel;
    lbHeadModIntroducere: TLabel;
    Bevel4: TBevel;
    lbModIntroducere: TLabel;
    lbNumarZecimale: TLabel;
    Label1: TLabel;
    Label15: TLabel;
    Label16: TLabel;
    lbDataAprobare: TLabel;
    edUnitate: TcxTextEdit;
    edAnFiscal: TcxSpinEdit;
    BtnDelVer: TcxButton;
    btnMuta: TcxButton;
    edNrZerouri: TcxImageComboBox;
    edZerouri: TcxSpinEdit;
    edZecimale: TcxSpinEdit;
    edProiect: TcxTextEdit;
    edFunctional: TcxTextEdit;
    edEconomic: TcxTextEdit;
    edIDVersiune: TcxTextEdit;
    ChkArataDoarPlanificat: TcxCheckBox;
    chkColoaneEstimari: TcxCheckBox;
    btnLastPlan: TcxButton;
    edDataAprobare: TcxDBDateEdit;
    btnAnuleazaVersiune: TcxButton;
    btnBlocheazaVers: TcxButton;
    btnAddVersiune: TcxButton;
    edVersiune: TcxLookupComboBox;
    NavPanelGroup2Control: TdxNavBarGroupControl;
    cxIspDetalii: TcxDBVerticalGrid;
    cxIspDetaliiCategoryRow1: TcxCategoryRow;
    cxIspDetaliiID_BG_VERSIUNE: TcxDBEditorRow;
    cxIspDetaliiDATA_CREARE: TcxDBEditorRow;
    cxIspDetaliiID_UTILIZATORI_CREAT: TcxDBEditorRow;
    cxIspDetaliiDEPARTAMENTE_CREAT: TcxDBEditorRow;
    cxIspDetaliiCategoryRow2: TcxCategoryRow;
    cxIspDetaliiDATA_APROBARE: TcxDBEditorRow;
    cxIspDetaliiID_UTILIZATORI_APROBAT: TcxDBEditorRow;
    cxIspDetaliiFUNCTIE: TcxDBEditorRow;
    cxIspDetaliiDEPARTAMENTE_APROBAT: TcxDBEditorRow;
    cxIspDetaliiCategoryRow3: TcxCategoryRow;
    cxIspDetaliiDBMultiEditorRow1: TcxDBMultiEditorRow;
    cxIspDetaliiEXPLICATIE: TcxDBEditorRow;
    cxIspDetaliiACT_APROBARE: TcxDBEditorRow;
    cxIspDetaliiCLASA_FUNCTIONALA: TcxDBEditorRow;
    cxIspDetaliiTIP_BUGET: TcxDBEditorRow;
    cxIspDetaliiVERSIUNE: TcxDBEditorRow;
    cxIspDetaliiAN_FISCAL: TcxDBEditorRow;
    cxIspDetaliiREVIZIE: TcxDBEditorRow;
    NavPanelGroup3Control: TdxNavBarGroupControl;
    Label12: TLabel;
    Bevel5: TBevel;
    Label13: TLabel;
    Label14: TLabel;
    Label5: TLabel;
    Bevel2: TBevel;
    Label6: TLabel;
    Label7: TLabel;
    Bevel3: TBevel;
    LbExcedent: TLabel;
    btnColorInside: TPanel;
    btnFontColor: TPanel;
    cxSplitter1: TcxSplitter;
    pnBottom: TPanel;
    pnInfo: TPanel;
    btnRaportare: TcxButton;
    splitV: TcxSplitter;
    QryClasaEconomica: TZQuery;
    DTClasEconomica: TDataSource;
    ClasaEconomica: TdxMemData;
    qryVersiune: TZQuery;
    DTVersiune: TDataSource;
    ImageList1: TImageList;
    ImageList2: TImageList;
    cxStyleRepository1: TcxStyleRepository;
    cxStyle1: TcxStyle;
    ColorDialog: TColorDialog;
    usVersiune: TZUpdateSQL;
    qryTemp: TZQuery;
    cxTreeClasificEco: TcxDBTreeList;
    cxTreeClasificEcoCOD_BUGET: TcxDBTreeListColumn;
    cxTreeClasificEcoDENUMIRE: TcxDBTreeListColumn;
    cxTreeClasificEcoREALIZAT: TcxDBTreeListColumn;
    cxTreeClasificEcoPLANIFICAT: TcxDBTreeListColumn;
    cxTreeClasificEcoPLANIFICAT_REST: TcxDBTreeListColumn;
    cxTreeClasificEcoPLANIFICAT1: TcxDBTreeListColumn;
    cxTreeClasificEcoPLANIFICAT2: TcxDBTreeListColumn;
    cxTreeClasificEcoPLANIFICAT3: TcxDBTreeListColumn;
    cxTreeClasificEcoPLANIFICAT4: TcxDBTreeListColumn;
    cxTreeClasificEcoCA: TcxDBTreeListColumn;
    cxTreeClasificEcoCA1: TcxDBTreeListColumn;
    cxTreeClasificEcoCA2: TcxDBTreeListColumn;
    cxTreeClasificEcoCA3: TcxDBTreeListColumn;
    cxTreeClasificEcoCA4: TcxDBTreeListColumn;    
    cxTreeClasificEcoPLUS1AN: TcxDBTreeListColumn;
    cxTreeClasificEcoPLUS2AN: TcxDBTreeListColumn;
    cxTreeClasificEcoPLUS3AN: TcxDBTreeListColumn;
    cxTreeClasificEcoPLUS4AN: TcxDBTreeListColumn;
    cxTreeClasificEcoAN_FISCAL: TcxDBTreeListColumn;
    cxTreeClasificEcoREVIZIE: TcxDBTreeListColumn;
    cxTreeClasificEcoINTRODUS: TcxDBTreeListColumn;
    cxTreeClasificEcoPLAN1: TcxDBTreeListColumn;
    cxTreeClasificEcoPLAN2: TcxDBTreeListColumn;
    cxTreeClasificEcoPLAN3: TcxDBTreeListColumn;
    cxTreeClasificEcoPLAN4: TcxDBTreeListColumn;
    cxTreeClasificEcoPLAN5: TcxDBTreeListColumn;
    cxTreeClasificEcoPLAN6: TcxDBTreeListColumn;
    cxTreeClasificEcoPLAN7: TcxDBTreeListColumn;
    cxTreeClasificEcoPLAN8: TcxDBTreeListColumn;
    cxTreeClasificEcoPLAN9: TcxDBTreeListColumn;
    cxTreeClasificEcoPLAN10: TcxDBTreeListColumn;
    cxTreeClasificEcoPLAN11: TcxDBTreeListColumn;
    cxTreeClasificEcoPLAN12: TcxDBTreeListColumn;
    chkVisualMark: TcxCheckBox;
    chkFitScreen: TcxCheckBox;
    cxTreeClasificEcoINTRODUCERE_ESTIMARE: TcxDBTreeListColumn;
    cxTreeClasificEcoINTRODUCERE_CA: TcxDBTreeListColumn;    
    pnControls: TPanel;
    TimerEnableControls: TTimer;
    pnNavPanel: TPanel;
    tabBuget: TcxTabControl;
    pmPreiaPlanificare: TPopupMenu;
    CmdPreia: TMenuItem;
    chkColoaneCA: TcxCheckBox;
    chkPlanLuna: TcxCheckBox;
    procedure edAnFiscalPropertiesChange(Sender: TObject);
    procedure FormCreate(Sender: TObject);
    procedure edZerouriChange(Sender: TObject);
    procedure edZecimaleChange(Sender: TObject);
    procedure cxSetZecimaleNr(ATree: TcxDBTreeList;AFormat: String);
    procedure BtnDelVerClick(Sender: TObject);
    procedure ChkArataDoarPlanificat1Click(Sender: TObject);
    procedure FormShow(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure ispDetaliiExit(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure chkVisualMarkClick(Sender: TObject);
    procedure cxSpinEdit1PropertiesChange(Sender: TObject);
    procedure edNrZerouriPropertiesChange(Sender: TObject);
    procedure ChkArataDoarPlanificatClick(Sender: TObject);
    procedure chkColoaneEstimariClick(Sender: TObject);
    procedure btnColorInsideClick(Sender: TObject);
    procedure btnLastPlanClick(Sender: TObject);
    procedure edDataAprobareExit(Sender: TObject);
    procedure btnAnuleazaVersiuneClick(Sender: TObject);
    procedure btnBlocheazaVersClick(Sender: TObject);
    procedure FormCloseQuery(Sender: TObject; var CanClose: Boolean);
    procedure btnAddVersiuneClick(Sender: TObject);
    procedure edVersiunePropertiesInitPopup(Sender: TObject);
    procedure edVersiunePropertiesEditValueChanged(Sender: TObject);
    procedure SelectDefalcareChange(Sender: TObject);
    procedure cxTreeClasificEcoCOD_BUGETGetDisplayText(
      Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: string);
    procedure cxTreeClasificEcoFocusedNodeChanged(Sender: TcxCustomTreeList;
      APrevFocusedNode, AFocusedNode: TcxTreeListNode);
    procedure cxTreeClasificEcoCustomDrawDataCell(Sender: TcxCustomTreeList;
      ACanvas: TcxCanvas; AViewInfo: TcxTreeListEditCellViewInfo;
      var ADone: Boolean);

    procedure cxTreeDetaliiCustomDrawDataCell(Sender: TcxCustomTreeList;
      ACanvas: TcxCanvas; AViewInfo: TcxTreeListEditCellViewInfo;
      var ADone: Boolean);

    procedure cxTreeClasificEcoFocusedColumnChanged(Sender: TcxCustomTreeList;
      APrevFocusedColumn, AFocusedColumn: TcxTreeListColumn);
    procedure cxTreeClasificEcoEnter(Sender: TObject);
    procedure cxTreeClasificEcoMouseMove(Sender: TObject; Shift: TShiftState; X,
      Y: Integer);
    procedure cxTreeClasificFuncCustomDrawDataCell(Sender: TcxCustomTreeList;
      ACanvas: TcxCanvas; AViewInfo: TcxTreeListEditCellViewInfo;
      var ADone: Boolean);
    procedure chkFitScreenClick(Sender: TObject);
    procedure TimerEnableControlsTimer(Sender: TObject);
    procedure splitVAfterOpen(Sender: TObject);
    procedure splitVAfterClose(Sender: TObject);
    procedure qryVersiuneNewRecord(DataSet: TDataSet);
    procedure tabBugetChange(Sender: TObject);
    procedure CmdPreiaClick(Sender: TObject);
    procedure chkColoaneCAClick(Sender: TObject);
    procedure chkPlanLunaClick(Sender: TObject);
    procedure btnMutaClick(Sender: TObject);
    procedure pmPreiaPlanificarePopup(Sender: TObject);
  private
    { Private declarations }
    FEstimariColumns : TList;
    FPlanColumns : TList;
    FLunaColumns : TList;
    FCAColumns : TList;
    FIsInLoading : Boolean;
    FMaxFunctionalLevel,
    FMaxEconomicLevel,
    FMaxTipProiectLevel,
    FMaxProiectLevel : Integer;
    FIsLocal: Boolean;
    FTabelaPlanificare: String;
    FTabelaConexa: String;
    frmBugetComparare : TfrmBugetComparare;
    procedure SetFilteredStatus;
    procedure InternalFilter(DataSet: TDataSet; var Accept: Boolean);
    procedure ClickButton(Sender: TToolButton);
    procedure ReportClick(Sender: TObject);
    procedure RefreshVersiuneTable(const ForceRefresh : Boolean = false);
    procedure SetTreeColumnReadOnlyByList(AReadOnly: Boolean;AList : TList);
    function  GetCapitol(AFunctional: String): String;
    procedure ValidarePlanificat(Sender: TField);
    procedure RefreshDataSet;
    procedure CloseDataSet;
    procedure WMRefreshcxField(var Message: TMessage); message WM_REFRESHBUGET;
    procedure ChangeHeaderPanel(ABugetPlanType: TBugetPlanType);
      //procedure DistribuieSumaPeLuni;
    procedure PreiaPlanificare(AIdVersiuneSursa, AIdVersiuneDestinatie : Integer; const PeUnitate : Boolean = False);
  protected
    FIsFundamentare: Boolean;
    FirstOpen : Boolean;
    FCurentTabelaPlanificare, FCurentTabelaConexa : String;
    procedure PopulateDetalii;
    procedure InitHintWindow;
    procedure TestAndSetGrid(Sender : TObject);
    procedure SetHeaderColumns;
    procedure ShowEstimari(VisibleState : Boolean);
    procedure ShowPlanLuna(VisibleState : Boolean);
    procedure ShowCA(VisibleState : Boolean);
    procedure RebuildSQLs;
    procedure GetLastPlanificare;
    procedure SetEnabledToControls(AState : Boolean);
    function GetTipVersiuneBuget : Variant;
    procedure CreateListaVersiuni;
    procedure TestStillValidVersion(aIdVersiune : Integer);
  public
    { Public declarations }
    FFisaDetaliu : TfrmFisaDetaliu;
    HeaderContainer : TfrmBxPlanContainer;
    HeaderGroupPanel : TcxGroupBox;
    posX, posY : Integer;

    FInternalCodFunctional : String;
    FInternalCodEconomic : String;
    FInternalProiect : Integer;
    FInternalUnitate : Integer;
    FIsVenit : Boolean; //daca este venit trebuie completat functional si asociat un sigur economic

    procedure SetContext;
    procedure SaveContext;
    procedure RestoreContext;
    procedure CreateVersiuneFields;
    procedure InitBGAprobat();

  published
    property TabelaConexa : String read FTabelaConexa write FTabelaConexa;
    property TabelaPlanificare : String read FTabelaPlanificare write FTabelaPlanificare;

    property IsFundamentare : Boolean read FIsFundamentare write FIsFundamentare default False;
    property IsLocal : Boolean read FIsLocal write FIsLocal default False;


  end;


procedure ExpandInner(aNode: TcxTreeListNode; Level: Integer);

function CreateBGFundamentare : TfrmBxPlanificare;
function CreateBGAprobat : TfrmBxPlanificare;

implementation

{$R *.DFM}

uses
  dxCompsUtile, ZeosDBUtile, cxStatusKeeper, DateUnit,
  Types, CommonDBVar , rapInclude , DateUtils;

var
  cst_InsideColor : TColor = clMoneyGreen;
  cst_FontColor : TColor = clMaroon;

function CreateBGAprobat : TfrmBxPlanificare;
begin
  Result := TfrmBxPlanificare.Create(Application);
  Result.InitBGAprobat;
end;

function CreateBGFundamentare : TfrmBxPlanificare;
begin
  Result := CreateBGAprobat;
  Exit;
  Result := TfrmBxPlanificare.Create(Application);
  Result.Caption := 'Fundamentare Buget';
  TfrmBxPlanificare(Result).chkColoaneEstimari.Checked := True;
  TfrmBxPlanificare(Result).chkColoaneCA.Checked := False;
  TfrmBxPlanificare(Result).TabelaPlanificare := 'BG_FUNDAMENTARE';
  TfrmBxPlanificare(Result).TabelaConexa := 'BG_F_VERSIUNE';
  TfrmBxPlanificare(Result).IsFundamentare := True;
  TfrmBxPlanificare(Result).IsLocal := False;
  TfrmBxPlanificare(Result).SetContext;
end;

procedure TfrmBxPlanificare.InitBGAprobat();
begin
  Caption := 'Buget Generat Aprobat';
  TabelaPlanificare := 'BG_APROBAT';
  TabelaConexa := 'BG_VERSIUNE';
  IsFundamentare := False;
  IsLocal := False;
  SetContext;
end;

procedure TfrmBxPlanificare.ValidarePlanificat(Sender: TField);
begin
  if not FIsInLoading then begin
    {$IFDEF NEW_VRESION}
    DBExecSQL(
      Format(
              'exec [spUpdateDiferentePlanificare] %d, %d, %s, %s, :id_bg_aprobat, :cod_economic, :cod_functional, :id_oi_unitati, :id_oi_proiecte, :id_versiune, :an_fiscal, :revizie',
              [ IdLogin, IdUtilizator, ValueToStr(FTabelaPlanificare), ValueToStr(FTabelaConexa) ]),
      QryClasaEconomica);
    {$ENDIF}
              
    { Facem actualizarea pe server si punem in coada de mesaje refresh-ul }
    with GetTmpADOQuery do
      try
         Sql.Add('DECLARE @VALOARE      MONEY');
         Sql.Add('DECLARE @PREV_VALOARE MONEY');
         if Sender.IsNull  then Sql.Add('SET @VALOARE = NULL')
         else
           if Integer(edZerouri.Value) > 0 then Sql.Add('SET @VALOARE = cast(:'+Sender.FieldName+' as money) * '+IntToStr(edZerouri.Value) + '.00')
           else Sql.Add('SET @VALOARE = :'+Sender.FieldName);
         Sql.Add(' SET @VALOARE = case when @VALOARE = 0 then NULL else @VALOARE end ');
         //if Sender.DataSet.FieldByName('INTRODUS').AsInteger = 1 then
         if Sender.DataSet.FieldByName('ID_BG_APROBAT').AsInteger <> 0  then begin
            //Sql.Add('UPDATE '+ FTabelaPlanificare+' SET '+Sender.FieldName+' = @VALOARE, MOMENT = getdate(), ID_UTILIZATOR = '+ IntToStr(IdUtilizator) + ' WHERE COD_FUNCTIONAL = :COD_FUNCTIONAL AND COD_ECONOMIC = :COD_ECONOMIC AND ID_'+ FTabelaConexa +' =  :ID_VERSIUNE AND (:ID_OI_UNITATI IS NULL OR ID_OI_UNITATI = :ID_OI_UNITATI) AND (:ID_OI_PROIECTE IS NULL OR ID_OI_PROIECTE = :ID_OI_PROIECTE)' )
            Sql.Add('UPDATE '+ FTabelaPlanificare+' SET '+Sender.FieldName+' = @VALOARE, MOMENT = getdate(), ID_UTILIZATOR = '+ IntToStr(IdUtilizator) + ' WHERE ID_BG_APROBAT = :ID_BG_APROBAT' );
         end
         else begin
            Sql.Add('INSERT INTO '+FTabelaPlanificare+' (CLASA_FUNCTIUNE, CLASA_ECONOMICA, COD_ECONOMIC, COD_FUNCTIONAL, '+Sender.FieldName+', AN_FISCAL, REVIZIE, ID_OI_UNITATI, ID_OI_PROIECTE, ID_'+ FTabelaConexa +', ID_UTILIZATOR, MOMENT)');
            Sql.Add('VALUES(:CLASA_FUNCTIUNE, :CLASA_ECONOMICA, :COD_ECONOMIC, :COD_FUNCTIONAL, @VALOARE, :AN_FISCAL, :REVIZIE, :ID_OI_UNITATI, :ID_OI_PROIECTE, :ID_VERSIUNE, ' + IntToStr(IdUtilizator)+', GETDATE())');
         end;
         DataSource := DTClasEconomica;
         ExecSql;
      finally
         Free;
      end;
    { Transmitem refresh pentru nodul pe care ne aflam acum }
    PostMessage(Handle, WM_REFRESHBUGET, 0, 0);
  end;
end;

procedure TfrmBxPlanificare.WMRefreshcxField(var Message: TMessage);
var
  lcxTreeState : TcxStatusKeeper;
begin
  lcxTreeState := TcxStatusKeeper.Create(Self);
  lcxTreeState.Storages.Add.Component := cxTreeClasificEco;
  try
    lcxTreeState.SaveState;
    RefreshDataSet;
  finally
    if lcxTreeState <> nil then
       lcxTreeState.LoadState;
  end;
end;



procedure TfrmBxPlanificare.PreiaPlanificare(AIdVersiuneSursa, AIdVersiuneDestinatie : Integer; const PeUnitate : Boolean);
begin
  with GetTmpADOQuery do
   try
      ParamCheck := False;
      SQL.Text := 'exec spBugetIntroducere_PreiaPlanificare ''' + FTabelaPlanificare + ''', ''' + FTabelaConexa + ''', ' + IntToStr(AIdVersiuneSursa) + ', ' + IntToStr(AIdVersiuneDestinatie);
      if PeUnitate then SQL.Text := SQL.Text   + ', ' + IntToStr(edUnitate.Tag) + ', ' + IntToStr(edProiect.Tag)+ ', ''' +  FInternalCodFunctional + ''', ''' + FInternalCodEconomic + '''';
      ExecSql;
      MessageDlg('Datele au fost preluate cu succes !', mtInformation, [mbOK], 0);
   finally
     Free;
   end;
end;

procedure TfrmBxPlanificare.RefreshDataSet;
var
   I: Integer;
   lDecision : Integer;
   lAnterior : Integer;

  function ExistPlanificare : Boolean;
   begin
     with GetTmpADOQuery do
       try
          ParamCheck := False;
          SQL.Text := 'exec spBugetIntroducereExistPlanificare ''' + FTabelaPlanificare + ''', ' + IntToStr(edVersiune.Tag) + ', '  + IntToStr(edUnitate.Tag) + ', ' + IntToStr(edProiect.Tag)+ ', ''' +  FInternalCodFunctional + ''', ''' + FInternalCodEconomic + '''';
          Open;
          Result := not IsEmpty;
       finally
          Free;
       end;
   end;

  function ExistaAnterior: Integer;
  begin
     with GetTmpADOQuery do
       try
          Close;
          SQL.Text := 'exec spBugetIntroducereExistAnterior ''' + FTabelaPlanificare + ''', ' + IntToStr(edVersiune.Tag) + ', ' +  IntToStr(edUnitate.Tag) + ', ' + IntToStr(edProiect.Tag)+ ', ''' +  FInternalCodFunctional + ''', ''' + FInternalCodEconomic + '''';
          Open;
          if not IsEmpty then
            Result := FieldByName('id_bg_versiune').AsInteger
          else
            Result := 0;
       finally
          Free;
       end;
  end;

  procedure ValidareIfFound(aFieldName : String; aTag : Integer);
  var
    lField : TField;
  begin
     lField := ClasaEconomica.FindField(aFieldName);
     if lField <> nil then
      with lField do begin
        OnValidate := ValidarePlanificat;
        Tag        := aTag;
      end;
  end;


begin
  if FIsInLoading then Exit;
  SetEnabledToControls(False);
  try
    FIsInLoading := True;
    try
      if (FTabelaConexa <> '') and DBTableExists(FTabelaConexa) then begin
        TestStillValidVersion(edVersiune.Tag);
      end;

      //inchidem dataset-uri
      ClasaEconomica.Active := False;
      FFisaDetaliu.qryFisaDetaliu.Close;


      if edVersiune.Tag > 0 then begin
        with QryClasaEconomica do begin
         if (not ExistPlanificare) then begin
            lAnterior := ExistaAnterior;
             if ( lAnterior> 0) then lDecision := MessageDlg('Doriti preluarea automata din planificarea anterioara ?', mtConfirmation, [mbYes, mbNo, mbCancel], 0);
         end;
         if lDecision = mrYes then
             PreiaPlanificare(lAnterior, edVersiune.Tag, False)
         else if lDecision = mrCancel then begin
            GetLastPlanificare;
            FIsInLoading := False;
            edZerouriChange(edVersiune);
            Exit;
         end;

          Close;
          //else  ChkArataDoarPlanificat.Checked := False;
          ParamByName('COD_FUNCTIONAL').Value   := FInternalCodFunctional;
          ParamByName('COD_ECONOMIC').Value     := FInternalCodEconomic;
          ParamByName('DIVIZOR').Value          := edZerouri.Value;
          ParamByName('ID_VERSIUNE').Value      := edVersiune.Tag;
          if edUnitate.Tag <> -1 then
            ParamByName('id_oi_unitati').Value  := edUnitate.Tag
          else
            ParamByName('id_oi_unitati').Value  := Null;
          if edProiect.Tag <> -1 then
            ParamByName('id_oi_proiecte').Value := edProiect.Tag
          else
            ParamByName('id_oi_proiecte').Value := Null;
          Open;
        end;

        ClasaEconomica.Active := False;
        ClasaEconomica.Fields.Clear;
        ClasaEconomica.LoadFromDataSet(QryClasaEconomica);
        QryClasaEconomica.Close;
        for I := 1 to 4 do begin
          ValidareIfFound('PLANIFICAT'+IntToStr(I), I);
          ValidareIfFound('CA'+IntToStr(I), I);          
          ValidareIfFound('PLUS'+IntToStr(I)+'AN', I);
        end;
        ValidareIfFound('REALIZAT', 5);
        ValidareIfFound('PLANIFICAT_REST', 0);


        FFisaDetaliu.SetParam('COD_FUNCTIONAL', FInternalCodFunctional);
        FFisaDetaliu.SetParam('COD_ECONOMIC', FInternalCodEconomic);
        FFisaDetaliu.SetParam('AN_FISCAL', edAnFiscal.Value);
        FFisaDetaliu.SetParam('REVIZIE', edVersiune.Tag);
        FFisaDetaliu.SetParam('DIVIZOR', edZerouri.Value);
        if edUnitate.Tag <> -1 then
          FFisaDetaliu.SetParam('ID_OI_UNITATI', edUnitate.Tag)
        else
          FFisaDetaliu.SetParam('ID_OI_UNITATI', null);
        if edProiect.Tag <> -1 then
           FFisaDetaliu.SetParam('ID_OI_PROIECTE', edProiect.Tag)
        else
           FFisaDetaliu.SetParam('ID_OI_PROIECTE', NULL);

        if edIDVersiune.Tag <> -1 then
          FFisaDetaliu.SetParam('ID_VERSIUNE', edIDVersiune.Tag)
        else
          FFisaDetaliu.SetParam('ID_VERSIUNE', NULL);


        if FirstOpen then begin
          cxTreeClasificEco.ApplyBestFit;
          FirstOpen := False;
        end;
        if splitV.State = ssOpened then begin
          FFisaDetaliu.RefreshDataSet;
        end;
      end;
     finally
      FIsInLoading := False;
    end;
   finally
    TimerEnableControls.Enabled := True;
  end;
end;

procedure TfrmBxPlanificare.FormCreate(Sender: TObject);
begin
  FEstimariColumns := TList.Create;
  FEstimariColumns.Add(TObject(cxTreeClasificEcoPLUS1AN.ItemIndex));
  FEstimariColumns.Add(TObject(cxTreeClasificEcoPLUS2AN.ItemIndex));
  FEstimariColumns.Add(TObject(cxTreeClasificEcoPLUS3AN.ItemIndex));
  FEstimariColumns.Add(TObject(cxTreeClasificEcoPLUS4AN.ItemIndex));
  FEstimariColumns.Add(TObject(cxTreeClasificEcoREALIZAT.ItemIndex));
  FPlanColumns := TList.Create;
  FPlanColumns.Add(TObject(cxTreeClasificEcoPLANIFICAT_REST.ItemIndex));
  FPlanColumns.Add(TObject(cxTreeClasificEcoPLANIFICAT1.ItemIndex));
  FPlanColumns.Add(TObject(cxTreeClasificEcoPLANIFICAT2.ItemIndex));
  FPlanColumns.Add(TObject(cxTreeClasificEcoPLANIFICAT3.ItemIndex));
  FPlanColumns.Add(TObject(cxTreeClasificEcoPLANIFICAT4.ItemIndex));
  FLunaColumns := TList.Create;
  FLunaColumns.Add(TObject(cxTreeClasificEcoPLAN1.ItemIndex));
  FLunaColumns.Add(TObject(cxTreeClasificEcoPLAN2.ItemIndex));
  FLunaColumns.Add(TObject(cxTreeClasificEcoPLAN3.ItemIndex));
  FLunaColumns.Add(TObject(cxTreeClasificEcoPLAN4.ItemIndex));
  FLunaColumns.Add(TObject(cxTreeClasificEcoPLAN5.ItemIndex));
  FLunaColumns.Add(TObject(cxTreeClasificEcoPLAN6.ItemIndex));
  FLunaColumns.Add(TObject(cxTreeClasificEcoPLAN7.ItemIndex));
  FLunaColumns.Add(TObject(cxTreeClasificEcoPLAN8.ItemIndex));
  FLunaColumns.Add(TObject(cxTreeClasificEcoPLAN9.ItemIndex));
  FLunaColumns.Add(TObject(cxTreeClasificEcoPLAN10.ItemIndex));
  FLunaColumns.Add(TObject(cxTreeClasificEcoPLAN11.ItemIndex));
  FLunaColumns.Add(TObject(cxTreeClasificEcoPLAN12.ItemIndex));
  FCAColumns := TList.Create;
  FCAColumns.Add(TObject(cxTreeClasificEcoCA1.ItemIndex));
  FCAColumns.Add(TObject(cxTreeClasificEcoCA2.ItemIndex));
  FCAColumns.Add(TObject(cxTreeClasificEcoCA3.ItemIndex));
  FCAColumns.Add(TObject(cxTreeClasificEcoCA4.ItemIndex));
  btnColorInside.Color := cst_InsideColor;
  btnFontColor.Color := cst_FontColor;
  FirstOpen := True;
  NavPanel.ActiveGroupIndex := 0;
  splitV.CloseSplitter;
  FInternalProiect := -1;
  FInternalUnitate := -1;
  HeaderContainer := TfrmBxPlanContainer.Create(Self);
  HeaderContainer.OnEditComplete := TestAndSetGrid;
  PopulateDetalii;
  RestoreContext;
end;

function TfrmBxPlanificare.GetCapitol(AFunctional: String): String;
var lIndex: Integer;
begin
  lIndex := pos('.', AFunctional);
  if lIndex > 0 then Result := Copy(AFunctional, 1, lIndex-1)
  else Result := AFunctional;
end;

procedure TfrmBxPlanificare.edZerouriChange(Sender: TObject);
var
  aIdEco : Integer;
  aNode : TcxTreeListNode;
begin
  aIdEco := -1;
  if cxTreeClasificEco.FocusedNode <> nil then
    try
      aIdEco := TcxDBTreeListNode(cxTreeClasificEco.FocusedNode).KeyValue;
    except
      aIdEco := -1;
    end;
  { Aplicam filtrul pentru afisare }
  if (Sender is TcxLookupComboBox) or (Sender is TcxSpinEdit) then
       TestAndSetGrid(nil);
  if (aIdEco <> -1) and (ClasaEconomica.Active) then begin
    aNode := cxTreeClasificEco.FindNodeByKeyValue(aIdEco);
    if aNode <> nil then begin
      aNode.Focused := True;
      aNode.MakeVisible;
    end
  end;
end;

procedure TfrmBxPlanificare.edZecimaleChange(Sender: TObject);
var lFormat: String;
  function Space(Nr: Integer): String;
  var I: Integer;
   begin
     Result := '';
     for I := 1 to Nr do Result := Result + '0';
   end;
begin
  lFormat := Space(edZecimale.Value);
  lFormat := ',0.'+lFormat+';-,0.'+lFormat;
  cxSetZecimaleNr(cxTreeClasificEco, lFormat);
end;


type TCrackToolButton = class(TToolButton);

procedure ExpandInner(aNode: TcxTreeListNode; Level: Integer);
var
  J : Integer;
begin
  if Level > 0 then begin
     aNode.Expand(False);
     for J := 0 to aNode.Count-1 do
       ExpandInner(aNode.Items[J], Level-1);
  end;
end;

procedure InternalExpandTree(aToolButton : TToolButton; aTree : TcxDBTreeList);
var I: Integer;
begin
  with aToolButton do begin
    aTree.BeginUpdate;
    aTree.FullCollapse;
    try
       for I := 0 to aTree.Count-1 do
         ExpandInner(aTree.Items[I], Tag-1);
    finally
       aTree.EndUpdate;
    end;
  end;
end;


procedure TfrmBxPlanificare.ClickButton(Sender: TToolButton);
begin
  Sender.Down := True;
  Sender.Click;
end;

procedure TfrmBxPlanificare.BtnDelVerClick(Sender: TObject);
begin
  if edVersiune.EditValue = null then Exit;
  if MessageDlg('Doriti anularea versiunii '+edVersiune.Text+' din anul '+edAnFiscal.Text+' pentru toate clasificatiile ?', mtConfirmation, [mbYes, mbNo], 0) <> mrYes then Exit;
  with GetTmpADOQuery do
    try
       SQL.Text := 'exec spBugetIntroducere_DeletePlanificare ' + QuotedStr(FTabelaPlanificare)+ ', ' + QuotedStr(FTabelaConexa)  + ', ' + IntToStr(edVersiune.Tag);
       ExecSql;
       GetLastPlanificare;
       RefreshDataSet;
    finally
       Free;
    end;
end;

procedure TfrmBxPlanificare.ChkArataDoarPlanificat1Click(
  Sender: TObject);
begin
  SetFilteredStatus;
end;

procedure TfrmBxPlanificare.SetFilteredStatus;
begin
  if ClasaEconomica.Filtered <> ChkArataDoarPlanificat.Checked then begin
     if ChkArataDoarPlanificat.Checked then begin
        ClasaEconomica.OnFilterRecord := InternalFilter;
        ClasaEconomica.Filtered       := True;
        ClasaEconomica.UpdateFilters;
     end
     else begin
        ClasaEconomica.OnFilterRecord := nil;
        ClasaEconomica.Filtered       := False;
        ClasaEconomica.UpdateFilters;
     end;
  end;
end;

procedure TfrmBxPlanificare.InternalFilter(DataSet: TDataSet;
  var Accept: Boolean);
begin
  Accept := DataSet.FieldByName('PLANIFICAT').AsCurrency > 0;
end;

procedure TfrmBxPlanificare.FormShow(Sender: TObject);
begin
  cxispDetalii.Visible := not FIsFundamentare;
  //btnMuta.Visible := FIsFundamentare;
  FillImageCombo(cxIspDetaliiID_UTILIZATORI_CREAT.Properties.EditProperties, 'select ID_UTILIZATORI, NUMEINTREG from UTILIZATORI order by NUMEINTREG', 'ID_UTILIZATORI', 'NUMEINTREG');
  cxIspDetaliiID_UTILIZATORI_APROBAT.Properties.EditProperties.Assign(cxIspDetaliiID_UTILIZATORI_CREAT.Properties.EditProperties);
end;

procedure TfrmBxPlanificare.FormClose(Sender: TObject;
  var Action: TCloseAction);
begin
  with QryVersiune do
    if State in dsEditModes then Post;
  Action := caFree;
end;

procedure TfrmBxPlanificare.ispDetaliiExit(Sender: TObject);
begin
  with QryVersiune do
    if State in dsEditModes then Post;
end;

procedure TfrmBxPlanificare.pmPreiaPlanificarePopup(Sender: TObject);
begin
  CreateListaVersiuni;
end;

procedure TfrmBxPlanificare.FormDestroy(Sender: TObject);
begin
  FEstimariColumns.Free;
  FPlanColumns.Free;
  FLunaColumns.Free;
  frmBugetComparare.Free;
  HeaderContainer.Free;
  FFisaDetaliu.Free;
end;

procedure TfrmBxPlanificare.InitHintWindow;
begin
  frmBugetComparare := TfrmBugetComparare.Create(Self);
  frmBugetComparare.Top  := 100;
  frmBugetComparare.Left := Self.Width - 150;
  frmBugetComparare.Show;
  SetWindowPos(frmBugetComparare.Handle, HWND_TOPMOST,0,0,0,0,SWP_NOMOVE or SWP_NOSIZE or SWP_NOACTIVATE or SWP_NOSENDCHANGING or SWP_NOZORDER);
  frmBugetComparare.Appear;
//  SetWindowPos(frmBugetComparare.handle,HWND_TOPMOST,0,0,0,0,SWP_NOMOVE or SWP_NOSIZE or SWP_NOACTIVATE or SWP_NOSENDCHANGING);
end;

procedure TfrmBxPlanificare.chkVisualMarkClick(Sender: TObject);
begin
  cxTreeClasificEco.Repaint;
end;

procedure TfrmBxPlanificare.cxSetZecimaleNr(ATree: TcxDBTreeList;
  AFormat: String);
var I: Integer;
begin
  for I := 0 to ATree.ColumnCount-1 do
    if ATree.Columns[I].PropertiesClassName = 'TcxCurrencyEditProperties' then
       TcxCurrencyEditProperties(ATree.Columns[I].Properties).DisplayFormat := AFormat;
end;

procedure TfrmBxPlanificare.cxSpinEdit1PropertiesChange(Sender: TObject);
begin
  if HeaderGroupPanel <> nil then  begin
    HeaderGroupPanel.Align := alNone;
    HeaderGroupPanel.Parent := HeaderContainer;
    HeaderGroupPanel.Visible := False;
  end;
  Panel5.Realign;
  //HeaderGroupPanel := HeaderContainer.GetPanel(cxSpinEdit1.Value);
  if HeaderGroupPanel <> nil then begin
    HeaderGroupPanel.Caption := '';
    HeaderGroupPanel.Parent := Panel5;
    HeaderGroupPanel.Visible := True;
    HeaderGroupPanel.Align := alClient;
    Panel5.Realign;
  end;
end;

procedure TfrmBxPlanificare.TestAndSetGrid(Sender: TObject);
begin
  if HeaderContainer.CheckCompleteValidation then
    begin
      FInternalCodFunctional := HeaderContainer.CodFunctional;
      FInternalCodEconomic   := HeaderContainer.CodEconomic;
      FInternalProiect       := HeaderContainer.Proiect;
      FInternalUnitate       := HeaderContainer.Unitate;
      edUnitate.Tag          := FInternalUnitate;
      edUnitate.Text         := HeaderContainer.DescriereUnitate;

      edProiect.Tag          := FInternalProiect;
      edProiect.Text         := HeaderContainer.DescriereProiect;

      edFunctional.Text      := HeaderContainer.CodFunctional;
      edFunctional.Tag       := 0;

      edEconomic.Text        := HeaderContainer.CodEconomic;
      edEconomic.Tag         := 0;
      //Sender = nil adica este apelat de mana Modificari de versiune, zeroruri
      //Sender <> nil atunci luam ultima versiune planificata
      //if Sender <> nil then GetLastPlanificare;
      RefreshDataSet;
      {
      ShowMessage('Cod Functional ' + FInternalCodFunctional + #13#10+
        'Cod Economic ' + FInternalCodEconomic+ #13#10+
        'Cod Proiect ' + IntToStr(FInternalProiect)+ #13#10+
        'Cod Unitate ' + IntToStr(FInternalUnitate) );
      }
    end
  else begin
    CloseDataSet;
  end;
end;

procedure TfrmBxPlanificare.ChangeHeaderPanel(ABugetPlanType: TBugetPlanType);
begin
  if HeaderGroupPanel <> nil then  begin
    HeaderGroupPanel.Align := alNone;
    HeaderGroupPanel.Parent := HeaderContainer;
    HeaderGroupPanel.Visible := False;
  end;
  Panel5.Realign;
  HeaderGroupPanel := HeaderContainer.GetPanel(ABugetPlanType);
  if HeaderGroupPanel <> nil then begin
    HeaderGroupPanel.Caption := '';
    HeaderGroupPanel.Parent := Panel5;
    HeaderGroupPanel.Visible := True;
    HeaderGroupPanel.Align := alClient;
    Panel5.Realign;
    RebuildSQLs;
  end;
  PopulateReportContext(Self.ClassName, btnRaportare, ReportClick);
end;

procedure TfrmBxPlanificare.CloseDataSet;
begin
  ClasaEconomica.Active := False;
  DoCheckClose(qryClasaEconomica);
  DoCheckClose(qryVersiune);
  if FFisaDetaliu <> nil then
    FFisaDetaliu.qryFisaDetaliu.Close;  
end;

procedure TfrmBxPlanificare.edNrZerouriPropertiesChange(Sender: TObject);
begin
  if IsNumeric(edNrZerouri.EditValue) then
    edZerouri.Value := StrToInt(edNrZerouri.EditValue)
  else
    edZerouri.Value := 1; 
end;



procedure TfrmBxPlanificare.edAnFiscalPropertiesChange(Sender: TObject);
begin
  RefreshVersiuneTable;
  SetHeaderColumns;
  edVersiunePropertiesEditValueChanged(edVersiune);
///  edZerouriChange(Sender);
end;

procedure TfrmBxPlanificare.SetHeaderColumns;
begin
   if edAnFiscal.Value <> null then begin
     cxTreeClasificEcoPLUS1AN.Caption.Text    := 'Estimari '  + IntToStr( edAnFiscal.Value + 1);
     cxTreeClasificEcoPLUS2AN.Caption.Text    := 'Estimari '  + IntToStr( edAnFiscal.Value + 2);
     cxTreeClasificEcoPLUS3AN.Caption.Text    := 'Estimari '  + IntToStr( edAnFiscal.Value + 3);
     cxTreeClasificEcoPLUS4AN.Caption.Text    := 'Estimari '  + IntToStr( edAnFiscal.Value + 4);
     cxTreeClasificEcoREALIZAT.Caption.Text   := 'Realizari ' + IntToStr( edAnFiscal.Value - 1);
     cxTreeClasificEcoPLANIFICAT.Caption.Text := 'Program '   + IntToStr( edAnFiscal.Value );
   end;
end;

procedure TfrmBxPlanificare.ChkArataDoarPlanificatClick(Sender: TObject);
begin
  SetFilteredStatus;
end;

procedure TfrmBxPlanificare.chkColoaneEstimariClick(Sender: TObject);
begin
  ShowEstimari(chkColoaneEstimari.Checked);
end;

procedure TfrmBxPlanificare.ShowEstimari(VisibleState: Boolean);
var
  lIndex : Integer;
  lBand : TcxTreeListBand;
begin
  lBand := cxFindBandByName(cxTreeClasificEco, 'Estimari');
  if lBand <>  nil then
    lBand.Visible := VisibleState;
  Exit;
  for lIndex := 0 to FEstimariColumns.Count - 1 do
    cxTreeClasificEco.Columns[Integer(FEstimariColumns[lIndex])].Visible := VisibleState;
end;

procedure TfrmBxPlanificare.RebuildSQLs;
begin
  //refacem procedurile care aduc date in ecran pentru fiecare tip de culgere
  CloseDataSet;
  
  if FTabelaPlanificare <> FCurentTabelaPlanificare then begin
    qryClasaEconomica.SQL.Text := 'EXEC spBugetIntroducere_'+ FTabelaPlanificare+' :ID_VERSIUNE, :DIVIZOR, :COD_FUNCTIONAL, :COD_ECONOMIC, :ID_OI_UNITATI, :ID_OI_PROIECTE';
    FCurentTabelaPlanificare   := FTabelaPlanificare;
  end;
end;

procedure TfrmBxPlanificare.SetContext;
begin
  edAnFiscal.Value := AnFiscal;
  if edVersiune.Tag = -1 then
    GetLastPlanificare;
  edZecimaleChange(edZecimale);
  SetFilteredStatus;
  SelectDefalcareChange(SelectDefalcare);
  ShowEstimari(chkColoaneEstimari.Checked);
  ShowCA(chkColoaneCA.Checked);
end;


procedure TfrmBxPlanificare.ReportClick(Sender: TObject);
begin
  SetRapParam('AN_FISCAL', edAnFiscal.Value);
  SetRapParam('VERSIUNE_BUGET', edVersiune.Tag);
  SetRapParam('COD_FUNCTIONAL', FInternalCodFunctional);
  SetRapParam('COD_ECONOMIC', FInternalCodEconomic);
  if FInternalProiect = -1 then
    SetRapParam('COD_PROIECT', null)
  else
    SetRapParam('COD_PROIECT', FInternalProiect);
  if FInternalUnitate = -1 then
    SetRapParam('COD_UNITATE', null)
  else
    SetRapParam('COD_UNITATE', FInternalUnitate);
  LoadReport(TMenuItem(Sender).Tag);
end;

procedure TfrmBxPlanificare.btnColorInsideClick(Sender: TObject);
var aColor : TColor;
begin
  if not Assigned(Sender) then Exit;
  ColorDialog.Color := TPanel(Sender).Color;
  if ColorDialog.Execute then begin
    aColor := ColorDialog.Color;
    with (Sender as TPanel) do Color := aColor;
  end;
  cst_InsideColor := btnColorInside.Color;
  cst_FontColor := btnFontColor.Color;
  cxTreeClasificEco.Repaint;
end;

procedure TfrmBxPlanificare.GetLastPlanificare;
begin
  edVersiune.EditValue := DBGetScallarFmt('exec [spBugetIntroducereGetLastPlanificare] %s, %s, %s, %s, %d, %d',
                            [
                              ValueToStr(FTabelaPlanificare),
                              ValueToStr(edVersiune.EditValue),
                              ValueToStr(FInternalCodFunctional),
                              ValueToStr(FInternalCodEconomic),
                              edUnitate.Tag,
                              edProiect.Tag
                            ], 'id_bg_versiune');
end;

procedure TfrmBxPlanificare.btnLastPlanClick(Sender: TObject);
begin
  GetLastPlanificare;
end;

procedure TfrmBxPlanificare.btnMutaClick(Sender: TObject);
begin
CreateListaVersiuni;
end;

procedure TfrmBxPlanificare.edDataAprobareExit(Sender: TObject);
begin
  with QryVersiune do
    if State in dsEditModes then Post;
end;

procedure TfrmBxPlanificare.btnAnuleazaVersiuneClick(Sender: TObject);
begin
  if ValueHasValue(edVersiune.EditValue) and (MessageDlg('Doriti anularea versiunii '+edVersiune.Text+' din anul '+edAnFiscal.Text+' pentru clasificatia curenta ?', mtConfirmation, [mbYes, mbNo], 0) = mrYes) then begin
    DBExecSQLFmt('exec [spBugetIntroducere_DeletePlanificare] %s, %s, %s, %d, %d, %s, %s',
                  [
                    ValueToStr(FTabelaPlanificare),
                    ValueToStr(FTabelaConexa),
                    ValueToStr(edVersiune.EditValue),
                    edUnitate.Tag,
                    edProiect.Tag,
                    ValueToStr(FInternalCodFunctional),
                    ValueToStr(FInternalCodEconomic)
                  ]);
    RefreshDataSet;
  end;
end;

procedure TfrmBxPlanificare.btnBlocheazaVersClick(Sender: TObject);
begin
  if ValueHasValue(edVersiune.EditValue) and (MessageDlg('Doriti (de)blocarea versiunii '+edVersiune.Text+' din anul '+edAnFiscal.Text+' pentru toate clasificatiile ?', mtConfirmation, [mbYes, mbNo], 0) = mrYes) then begin
    DBExecSQLFmt('exec [spBXBlockPlanificare] %s, %s, %s, %d', [ValueToStr(FTabelaPlanificare), ValueToStr(FTabelaConexa), ValueToStr(edVersiune.EditValue), btnBlocheazaVers.Tag]);
    RefreshDataSet;
    btnBlocheazaVers.Tag := btnBlocheazaVers.Tag xor 1;
  end;
end;

procedure TfrmBxPlanificare.RestoreContext;
begin
  StorageReadCxTree(cxTreeClasificEco);
  edNrZerouri.ItemIndex := StrToInt(StorageReadValue('ModIntroducere','3','Planificare'));
  edZecimale.Value := StrToInt(StorageReadValue('NrZecimale', '3', 'Planificare'));
  if StrToInt(StorageReadValue('AnFiscal', IntToStr(YearOf(Date)), 'Planificare')) = AnFiscal then begin
    edAnFiscal.Value := StrToInt(StorageReadValue('AnFiscal', IntToStr(YearOf(Date)), 'Planificare'));
    edVersiune.Tag := StrToInt(StorageReadValue('Versiune', '1', 'Planificare'));
  end
   else begin
    edAnFiscal.Value := AnFiscal;
    edVersiune.Tag := -1;
  end;
  ChkArataDoarPlanificat.Checked := (StorageReadValue('DoarPlanificare', '0', 'Planificare') = '1');
  chkColoaneEstimari.Checked := (StorageReadValue('Estimari', '0', 'Planificare') = '1');
  chkColoaneCA.Checked := (StorageReadValue('CA', '0', 'Planificare') = '1');  
  chkFitScreen.Checked := (StorageReadValue('FitScreen', '0', 'Planificare') = '1');
end;

procedure TfrmBxPlanificare.SaveContext;
begin
  StorageWriteValue('ModIntroducere', IntToStr(edNrZerouri.ItemIndex), 'Planificare');
  StorageWriteValue('NrZecimale', IntToStr(edZecimale.Value), 'Planificare');
  StorageWriteValue('AnFiscal', IntToStr(edAnFiscal.Value), 'Planificare');
  StorageWriteValue('Versiune', IntToStr(edVersiune.Tag), 'Planificare');
  StorageWriteValue('DoarPlanificare', IntToStr(Integer(ChkArataDoarPlanificat.Checked)), 'Planificare');
  StorageWriteValue('Estimari', IntToStr(Integer(chkColoaneEstimari.Checked)), 'Planificare');
  StorageWriteValue('CA', IntToStr(Integer(chkColoaneCA.Checked)), 'Planificare');
  StorageWriteValue('FitScreen', IntToStr(Integer(chkFitScreen.Checked)), 'Planificare');
  StorageWriteCxTree(cxTreeClasificEco);
end;

procedure TfrmBxPlanificare.FormCloseQuery(Sender: TObject;
  var CanClose: Boolean);
begin
  SaveContext;
end;

procedure TfrmBxPlanificare.btnAddVersiuneClick(Sender: TObject);
var
  lDataSet  : TDataSet;
begin
  lDataSet := DBNewQueryFmt('exec [spBugetIntroducereNewVersion] %s, %s', [ValueToStr(GetTipVersiuneBuget), ValueToStr(edAnFiscal.EditValue)]);
  try
    lDataSet.Open;
    edVersiune.Tag  := lDataSet.FieldByName('id_bg_versiune').AsInteger;
    edVersiune.Text := lDataSet.FieldByName('descriere').AsString;
    edZerouriChange(edVersiune);
  finally
    lDataSet.Free;
  end;
end;

procedure TfrmBxPlanificare.edVersiunePropertiesInitPopup(Sender: TObject);
begin
 if not qryVersiune.Active then
    RefreshVersiuneTable;
end;

procedure TfrmBxPlanificare.edVersiunePropertiesEditValueChanged(
  Sender: TObject);
begin
  if (VarToStr(edVersiune.EditValue) <> '') then begin
    TestStillValidVersion(edVersiune.EditValue);
  end;
  edZerouriChange(Sender);
end;

procedure TfrmBxPlanificare.SelectDefalcareChange(Sender: TObject);
begin
  case SelectDefalcare.TabIndex of
     0 : ChangeHeaderPanel(bptFunctional);
     1 : ChangeHeaderPanel(bptUnitateInterna);
     2 : ChangeHeaderPanel(bptUnitateExterna);
     3 : ChangeHeaderPanel(bptProiectFunctional);
     4 : ChangeHeaderPanel(bptEconomicUnitate);
  end;
  TestAndSetGrid(nil);
end;

procedure TfrmBxPlanificare.RefreshVersiuneTable;
begin
  if not qryVersiune.Active or (qryVersiune.ParamByName('an_fiscal').Value <> edAnFiscal.Value) or ForceRefresh then
  begin
    qryVersiune.Close;
    qryVersiune.ParamByName('an_fiscal').Value := edAnFiscal.Value;
    qryVersiune.ParamByName('tip_versiune').Value := GetTipVersiuneBuget;
    qryVersiune.Open;
  end;
end;


procedure TfrmBxPlanificare.cxTreeClasificEcoCOD_BUGETGetDisplayText(
  Sender: TcxTreeListColumn; ANode: TcxTreeListNode; var Value: string);
begin
  Value := Value + ': ' + ANode.Texts[cxTreeClasificEcoDENUMIRE.ItemIndex];
end;


procedure TfrmBxPlanificare.cxTreeClasificEcoCustomDrawDataCell(
  Sender: TcxCustomTreeList; ACanvas: TcxCanvas;
  AViewInfo: TcxTreeListEditCellViewInfo; var ADone: Boolean);
var
  lIntrodus: Boolean;
begin
  if ((not AViewInfo.Node.HasChildren) and ( FPlanColumns.IndexOf(TObject(AViewInfo.Column.ItemIndex)) <> -1 )) or
     ((FEstimariColumns.IndexOf(TObject(AViewInfo.Column.ItemIndex)) <> -1)
     and Assigned(AViewInfo.Node) and (AViewInfo.Node.Values[cxTreeClasificEcoINTRODUCERE_ESTIMARE.ItemIndex] = 1))
  then begin
    lIntrodus :=
              (AViewInfo.Node.Texts[cxTreeClasificEcoINTRODUS.ItemIndex] = '1');
(*
               or (
                 (AViewInfo.Node.Values[cxTreeClasificEcoINTRODUCERE_ESTIMARE.ItemIndex] = 1)
                 and (AViewInfo.Column.ItemIndex  in [ cxTreeClasificEcoPLUS1AN.ItemIndex,cxTreeClasificEcoPLUS2AN.ItemIndex, cxTreeClasificEcoPLUS3AN.ItemIndex,
                   cxTreeClasificEcoPLUS4AN.ItemIndex, cxTreeClasificEcoREALIZAT.ItemIndex ]));
*)
    if not lIntrodus then
      if chkVisualMark.Checked then ACanvas.Brush.Color := cst_InsideColor
                               else ACanvas.Brush.Color := clBtnFace;
  end
  else begin
    ACanvas.Font.Style := ACanvas.Font.Style + [fsBold];
  end;
  if chkVisualMark.Checked then ACanvas.Font.Color := cst_FontColor else ACanvas.Font.Color := clMaroon;
  if AViewInfo.Focused then begin
      ACanvas.Brush.Color := clWhite;
      ACanvas.Font.Color := clBlack;
  end;
end;

procedure TfrmBxPlanificare.cxTreeClasificEcoEnter(Sender: TObject);
begin
  cxTreeClasificEcoFocusedColumnChanged(cxTreeClasificEco, nil, cxTreeClasificEco.FocusedColumn);
end;

procedure TfrmBxPlanificare.cxTreeClasificEcoFocusedColumnChanged(
  Sender: TcxCustomTreeList; APrevFocusedColumn,
  AFocusedColumn: TcxTreeListColumn);
begin
  with cxTreeClasificEco do
    if (AFocusedColumn = cxTreeClasificEcoCOD_BUGET) then
       OptionsBehavior.IncSearch := True
    else OptionsBehavior.IncSearch := False;
end;

procedure TfrmBxPlanificare.cxTreeClasificEcoFocusedNodeChanged(
  Sender: TcxCustomTreeList; APrevFocusedNode, AFocusedNode: TcxTreeListNode);
begin
  SetTreeColumnReadOnlyByList(
    (not Assigned(AFocusedNode)) or (AFocusedNode.HasChildren) or
    (btnBlocheazaVers.Tag=1), FPlanColumns);
  SetTreeColumnReadOnlyByList(
      not(Assigned(AFocusedNode) and (AFocusedNode.Values[cxTreeClasificEcoINTRODUCERE_ESTIMARE.ItemIndex] = 1))
      or (btnBlocheazaVers.Tag=1)
     , FEstimariColumns);
  SetTreeColumnReadOnlyByList(
      not(Assigned(AFocusedNode) and (AFocusedNode.Values[cxTreeClasificEcoINTRODUCERE_CA.ItemIndex] = 1))
      or (btnBlocheazaVers.Tag=1)
     , FCAColumns);
  SetTreeColumnReadOnlyByList(
    (not Assigned(AFocusedNode)) or (AFocusedNode.HasChildren) or
    (btnBlocheazaVers.Tag=1), FLunaColumns);
end;

procedure TfrmBxPlanificare.cxTreeClasificEcoMouseMove(Sender: TObject;
  Shift: TShiftState; X, Y: Integer);
var
  aNode : TcxTreeListNode;
begin
  if frmBugetComparare = nil then Exit;
  posX := X;
  posY := Y;
  aNode := cxTreeClasificEco.GetNodeAt(posX, posY);
  if aNode <> nil then begin
    frmBugetComparare.Label1.Caption := aNode.Texts[cxTreeClasificEcoCOD_BUGET.ItemIndex];
  end;
end;

procedure TfrmBxPlanificare.cxTreeClasificFuncCustomDrawDataCell(
  Sender: TcxCustomTreeList; ACanvas: TcxCanvas;
  AViewInfo: TcxTreeListEditCellViewInfo; var ADone: Boolean);
begin
  if not AViewInfo.Node.HasChildren then begin end
  else ACanvas.Font.Style := ACanvas.Font.Style + [fsBold];
end;

procedure TfrmBxPlanificare.chkFitScreenClick(Sender: TObject);
begin
  cxTreeClasificEco.OptionsView.ColumnAutoWidth := chkFitScreen.Checked;
end;


procedure TfrmBxPlanificare.TimerEnableControlsTimer(Sender: TObject);
begin
  TimerEnableControls.Enabled := False;
  SetEnabledToControls(True);
end;

procedure TfrmBxPlanificare.SetEnabledToControls(AState: Boolean);
begin
  BtnDelVer.Enabled := AState;
  btnAddVersiune.Enabled := AState;
  btnMuta.Enabled := AState;
  btnLastPlan.Enabled := AState;
  btnBlocheazaVers.Enabled := AState;
  btnAnuleazaVersiune.Enabled := AState;
  NavPanel.Enabled := AState;
  pnControls.Enabled := AState;
end;


procedure TfrmBxPlanificare.PopulateDetalii;
begin
   FFisaDetaliu := TfrmFisaDetaliu.Create(self, 'exec spBXTipuriTotalizari',
      ':COD_FUNCTIONAL, :AN_FISCAL, :REVIZIE, :DIVIZOR, :ID_OI_UNITATI, :ID_OI_PROIECTE, :ID_VERSIUNE');
   FFisaDetaliu.BorderStyle := bsNone;
   FFisaDetaliu.Parent := pnBottom;
   FFisaDetaliu.Visible := FFisaDetaliu.HasTabs;
   FFisaDetaliu.Align := alClient;
   FFisaDetaliu.TreeDetaliu.OnCustomDrawDataCell := cxTreeDetaliiCustomDrawDataCell;
   splitV.Visible := FFisaDetaliu.Visible;
end;

procedure TfrmBxPlanificare.splitVAfterOpen(Sender: TObject);
begin
  if FFisaDetaliu <> nil then
    FFisaDetaliu.RefreshDataSet;
end;

procedure TfrmBxPlanificare.splitVAfterClose(Sender: TObject);
begin
  if FFisaDetaliu <> nil then
    FFisaDetaliu.qryFisaDetaliu.Close;
end;

procedure TfrmBxPlanificare.cxTreeDetaliiCustomDrawDataCell(
  Sender: TcxCustomTreeList; ACanvas: TcxCanvas;
  AViewInfo: TcxTreeListEditCellViewInfo; var ADone: Boolean);
begin
  if not AViewInfo.Node.HasChildren then begin end
  else ACanvas.Font.Style := ACanvas.Font.Style + [fsBold];
end;

function TfrmBxPlanificare.GetTipVersiuneBuget: Variant;
begin
    case tabBuget.TabIndex of
           0 :  Result := null;
           1 :  Result := 'B';
           2 :  Result := 'D';
    end;
end;

procedure TfrmBxPlanificare.qryVersiuneNewRecord(DataSet: TDataSet);
begin
   if GetTipVersiuneBuget = null then
     DataSet.FieldByName('tip_versiune').AsString := 'B'
  else
   DataSet.FieldByName('tip_versiune').AsString := GetTipVersiuneBuget;
end;

procedure TfrmBxPlanificare.tabBugetChange(Sender: TObject);
begin
  RefreshVersiuneTable;
  SetHeaderColumns;
  edVersiunePropertiesEditValueChanged(edVersiune);
end;

procedure TfrmBxPlanificare.CreateListaVersiuni;
const
  cstVersiuneText= 'Versiunea %s din %s';
var
  lBM : TBookmark;
  lMenuItem : TMenuItem;
begin
  pmPreiaPlanificare.Items.Clear;
  lBM := qryVersiune.GetBookmark;
  try
    qryVersiune.DisableControls;
    qryVersiune.First;
    while not qryVersiune.Eof do begin
      lMenuItem := TMenuItem.Create(pmPreiaPlanificare);
      lMenuItem.Caption :=
          Format( cstVersiuneText,
            [qryVersiune.FieldByName('tip_versiune').AsString + qryVersiune.FieldByName('revizie').AsString,
             FormatDateTime('dd/mm/yyyy', qryVersiune.FieldByName('data_aprobare').AsDateTime)
            ]);
      lMenuItem.Tag := qryVersiune.FieldByName('id_bg_versiune').AsInteger;
      lMenuItem.OnClick := CmdPreiaClick;
      pmPreiaPlanificare.Items.Add(lMenuItem);

      qryVersiune.Next;
    end;
  finally
    qryVersiune.GotoBookmark(lBM);
    qryVersiune.FreeBookmark(lBM);
    qryVersiune.EnableControls;
  end;


end;

procedure TfrmBxPlanificare.CmdPreiaClick(Sender: TObject);
var
  lIdVersiune : Integer;
  lVersAnt : String;
  lMessage : String;
begin
  if edVersiune.EditValue = null then Exit;
  if not (Sender is TMenuItem) then Exit;

  lIdVersiune := TMenuItem(Sender).Tag;

  lVersAnt := '';
  with GetTmpADOQuery do
  try
     SQL.Add('select top 1 tip_versiune + ltrim(rtrim(str(revizie))) as revizie from bg_versiune where id_bg_versiune = '+ IntToStr(lIdVersiune));
     Open;
     if not IsEmpty then lVersAnt := Fields[0].AsString;
  finally
     free;
  end;
  if lVersAnt = '' then Exit;
  lMessage := 'Doriti preluarea din versiunea ' + lVersAnt + ' in versiunea din ecran (versiunea '
    + IntToStr(edVersiune.EditValue) +  ') pentru clasificatia functionala '+ edFunctional.Text;
  if edUnitate.Text <> '' then
    lMessage := lMessage + ' pentru unitatea ' + edUnitate.Text;

  if MessageDlg(lMessage, mtConfirmation, [mbYes, mbNo], 0) = mrYes then begin
     PreiaPlanificare(lIdVersiune, edVersiune.EditValue, True);
     RefreshDataSet;
  end
  else begin
    MessageDlg('Datele nu au fost preluate la cererea utilizatorului', mtInformation, [mbOK], 0);
  end;
end;


procedure TfrmBxPlanificare.CreateVersiuneFields;
var
  I : Integer;
  lField : TField;
begin
  if qryVersiune.FieldCount = 0 then begin
    qryVersiune.FieldDefs.Update;
    for i := 0 to qryVersiune.FieldDefs.Count - 1 do
      qryVersiune.FieldDefs[i].CreateField(qryVersiune);

    lField := TStringField.Create(qryVersiune);
    lField.FieldName := 'descriere';
    lField.Size := 100;
    lField.FieldKind := fkCalculated;
    lField.ProviderFlags := [];
    lField.DataSet := qryVersiune;
  end;
end;

procedure TfrmBxPlanificare.TestStillValidVersion(aIdVersiune: Integer);
begin
  RefreshVersiuneTable(True);
  if qryVersiune.Locate('ID_' +FTabelaConexa, aIdVersiune, []) then begin
    edIDVersiune.Tag  := qryVersiune.FieldByName('ID_' + FTabelaConexa).AsInteger;
    edIDVersiune.Text := qryVersiune.FieldByName('ID_' + FTabelaConexa).AsString;
    edVersiune.Tag    := qryVersiune.FieldByName('ID_' + FTabelaConexa).AsInteger;
    btnBlocheazaVers.Tag := Integer(qryVersiune.FieldByName('isBlocked').AsBoolean);
    if btnBlocheazaVers.Tag = 0 then btnBlocheazaVers.Caption := 'Blocheaza versiune'
                                else btnBlocheazaVers.Caption := 'Deblocheaza versiune';
    edVersiune.Text := qryVersiune.FieldByName('descriere').AsString;
  end else begin
    edIDVersiune.Tag  := -1;
    edIDVersiune.Text := '';
    edVersiune.Tag    := -1;
    edVersiune.Text   := '';
  end;
end;

procedure TfrmBxPlanificare.ShowCA(VisibleState: Boolean);
var
  lIndex : Integer;
  lBand : TcxTreeListBand;
begin
  lBand := cxFindBandByName(cxTreeClasificEco, 'Credit de angajament');
  if lBand <>  nil then
    lBand.Visible := VisibleState;
  Exit;
  for lIndex := 0 to FCAColumns.Count - 1 do
    cxTreeClasificEco.Columns[Integer(FCAColumns[lIndex])].Visible := VisibleState;
end;

procedure TfrmBxPlanificare.SetTreeColumnReadOnlyByList(AReadOnly: Boolean;
  AList: TList);
var
  lIndex : Integer;
begin
  for lIndex := 0 to AList.Count - 1 do
    cxTreeClasificEco.Columns[Integer(AList[lIndex])].Options.Editing := not AReadOnly;
end;

procedure TfrmBxPlanificare.chkColoaneCAClick(Sender: TObject);
begin
  ShowCA(chkColoaneCA.Checked);
end;

procedure TfrmBxPlanificare.chkPlanLunaClick(Sender: TObject);
begin
  ShowPlanLuna(chkPlanLuna.Checked);

end;



procedure TfrmBxPlanificare.ShowPlanLuna(VisibleState: Boolean);
var
  lBand : TcxTreeListBand;
begin
  lBand := cxFindBandByName(cxTreeClasificEco, 'Buget Luna');
  if lBand <>  nil then
    lBand.Visible := VisibleState;
end;

end.
