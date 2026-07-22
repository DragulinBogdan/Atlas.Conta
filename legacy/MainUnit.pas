unit MainUnit;

interface

{$I Contabilitate.inc}


uses
  Windows, Messages, SysUtils, Classes, Graphics, Controls, Forms, Dialogs,
  ATSMenu, ATSMainBarMenu, MethodProvider, ActnList, CommonDBVar, ActulUnit,
  ZDataSet, fmAnexeBilantUnit, StdCtrls, CulegereNoteUnit, PlanConturiUnit,
  svnInfo, ExtCtrls, cxControls, AppEvnts, Menus, cxContainer,
  cxEdit, cxTextEdit, cxMaskEdit, cxDropDownEdit, cxTL, cxInplaceContainer,
  cxTLData, cxDBTL, dxStatusBar, dxCompsUtile, FR4RapExplorer, frxClass,
  frmProgressUnit, ATLASSIUtils, cxGraphics, cxImageComboBox, cxProgressBar,
  cxCurrencyEdit, cxCheckBox, cxCalendar, cxSpinEdit, ZeosDBMenu,
  cxLookAndFeelPainters, cxTLdxBarBuiltInMenu, dxBar, cxLookAndFeels, cxCustomData,
  cxStyles, FormulareUnit, unit_AutoClientForm, cxClasses, formsUtilsUnit,
  dxBarBuiltInMenu, cxPC, dxWheelPicker, dxTabbedMDI, dxNavBar,
  dxNavBarGroupItems, dxNavBarCollns, dxNavBarBase, cxButtons, cxSplitter;

type
  TmainForm = class(TAllClientForm)
    MainMenu: TATSMainBar;
    Actiuni: TActionList;
    Methods: TMethodProvider;
    Comenzi: TATSZeosAppCommands;
    Cmd_CulegereNote: TAction;
    Cmd_IntretinerePlanConturi: TAction;
    Cmd_IntretinerePlanBugete: TAction;
    Cmd_Balanta: TAction;
    Cmd_GeneratorRapoarte: TAction;
    Cmd_IstoricNote: TAction;
    RapCommands: TATSZeosAppCommands;
    Cmd_RefreshReports: TAction;
    Cmd_ModificareMeniu: TAction;
    Cmd_Execute_Report: TAction;
    Cmd_IntretinNaturCheltuieli: TAction;
    Cmd_IntretinModPlata: TAction;
    Cmd_IntretinValute: TAction;
    Cmd_IntretinBanci: TAction;
    Cmd_IntretinereRepartitori: TAction;
    Cmd_IntretinereJurnale: TAction;
    Cmd_IntretinereUtilizatori: TAction;
    Cmd_Angajamente: TAction;
    Cmd_TranzactiiCV: TAction;
    Cmd_Documente: TAction;
    Cmd_DocumenteValidare: TAction;
    Cmd_IntretinereFunctii: TAction;
    Cmd_IntretinereTipuriMateriale: TAction;
    Cmd_WizardDocum: TAction;
    Cmd_Decontari: TAction;
    Cmd_StocuriUnitate: TAction;
    Cmd_ImportNote: TAction;
    Cmd_RegistruDocumente: TAction;
    Cmd_PlanificareBuget: TAction;
    Cmd_Registru: TAction;
    Cmd_ListaCasa: TAction;
    Cmd_IntretinTipProiect: TAction;
    Cmd_IntretinerePlanProiecte: TAction;
    Cmd_IntretinereCasa: TAction;
    Cmd_IntretinereCulori: TAction;
    Cmd_IntretinerTipDoc: TAction;
    Cmd_CorespTipDoc: TAction;
    Cmd_IntretinereTipCheltuieli: TAction;
    Cmd_DocumenteValidate: TAction;
    Cmd_RefreshDataSet: TAction;
    Cmd_Cascade: TAction;
    Cmd_TileHorizontal: TAction;
    Cmd_UrmarireExecutie: TAction;
    Cmd_AngajamentGlobal: TAction;
    Cmd_IntretinereOrganigramaCasa: TAction;
    Cmd_Deconturi: TAction;
    Cmd_PreluareBuget: TAction;
    Cmd_DefinireDecont: TAction;
    Cmd_ValidareDecont: TAction;
    Cmd_GenerareOP: TAction;
    Cmd_ModificareOP: TAction;
    Cmd_IntretinereAnexeBilant: TAction;
    Cmd_IntretinereBugetDirectii: TAction;
    Cmd_IntretinereBugetProiecte: TAction;
    Cmd_BugetAprobat: TAction;
    Cmd_IntretinereAntet: TAction;
    Cmd_SchimbareParola: TAction;
    Cmd_IntretinereBilant: TAction;
    Cmd_IntretinereDelegati: TAction;
    Cmd_IntretinereMijloaceTransport: TAction;
    Cmd_IntretinereTipuriMIjloaceTransport: TAction;
    Cmd_FundamentareBuget: TAction;
    Cmd_IntretinereNomenclator: TAction;
    Cmd_IntretinereAnexeExecutieBugetara: TAction;
    Cmd_ContareBugetara: TAction;
    Cmd_IntretinereTipProdus: TAction;
    Cmd_IntretinereTipMateriale: TAction;
    Cmd_IntretinereTipStoc: TAction;
    Cmd_IntretinereTipStoc_TipProdus: TAction;
    Cmd_About: TAction;
    Cmd_InchiderePerioadeFiscale: TAction;
    Cmd_IntretinereTipuriRepartitori: TAction;
    Cmd_IntretinereOrganizatie: TAction;
    Cmd_BGFundamentare: TAction;
    Cmd_OITipuriProiecte: TAction;
    MainStatusBar: TdxStatusBar;
    Cmd_OIProiecte: TAction;
    ApplicationEvents: TApplicationEvents;
    Cmd_IntertinereBGPlan: TAction;
    Cmd_OEIntretinereRepartitori: TAction;
    Cmd_GEST_RegistruDocumente: TAction;
    Cmd_ALOPAdaugaAngajament: TAction;
    Cmd_BGAprobat: TAction;
    Cmd_AlopListaAngajamente: TAction;
    Cmd_ALOPLichidare: TAction;
    Cmd_RapExport: TAction;
    Cmd_RapImport: TAction;
    Cmd_NotaSalarii: TAction;
    Cmd_NoteInchidere: TAction;
    Cmd_AlopListaOrdonantare: TAction;
    Cmd_AlopIntretinereConturi: TAction;
    Cmd_GenerarePlata: TAction;
    Cmd_NoteImperechere: TAction;
    Cmd_RapImplicit: TAction;
    Cmd_CulegeAnexeSubunitati: TAction;
    Cmd_CumulareAnexe: TAction;
    Cmd_FRGeneratorRapoarte: TAction;
    Cmd_FRExecute_Report: TAction;
    Cmd_PreluareAnexe: TAction;
    Cmd_FisaBugetara: TAction;
    Cmd_TestEroare: TAction;
    FRRapCommands: TATSZeosAppCommands;
    Cmd_SituatieTert: TAction;
    Cmd_IntretinereTipDoc: TAction;
    Cmd_PreviewAnexe: TAction;
    CmdGenCodBara: TAction;
    Cmd_AlopDispozitie: TAction;
    Cmd_AlopListaDispozitii: TAction;
    Cmd_ContracteLista: TAction;
    Cmd_Contracte: TAction;
    Cmd_TranspunerePlan: TAction;
    Cmd_Compensari: TAction;
    pnContent: TPanel;
    Cmd_BazaSchimbare: TAction;
    Cmd_GrupeProiect: TAction;
    Cmd_RegistruNou: TAction;
    Cmd_PreluareExtrase: TAction;
    Cmd_ContracteManagInvest: TAction;
    HamMenu: TdxNavBar;
    Cmd_AfisareNavigare: TAction;
    pnlStatusBar: TPanel;
    Cmd_NavigareWeb: TAction;
    tabTop: TcxTabControl;
    procedure FormCreate(Sender: TObject);
    procedure Cmd_CulegereNoteExecute(Sender: TObject);
    procedure Cmd_IntretinerePlanConturiExecute(Sender: TObject);
    procedure Cmd_IntretinerePlanBugeteExecute(Sender: TObject);
    procedure Cmd_IstoricNoteExecute(Sender: TObject);
    procedure Cmd_BalantaExecute(Sender: TObject);
    procedure ComenziNewCommand(Sender: TObject; Command: TATSCommand);
    procedure ComenziBeforeOpen(Sender: TObject);
    procedure ComenziAfterOpen(Sender: TObject);
    procedure Cmd_RefreshReportsExecute(Sender: TObject);
    procedure Cmd_ModificareMeniuExecute(Sender: TObject);
    procedure Cmd_IntretinValuteExecute(Sender: TObject);
    procedure Cmd_IntretinereRepartitoriExecute(Sender: TObject);
    procedure Cmd_IntretinereJurnaleExecute(Sender: TObject);
    procedure Cmd_IntretinereUtilizatoriExecute(Sender: TObject);
    procedure Cmd_AngajamenteExecute(Sender: TObject);
    procedure Cmd_TranzactiiCVExecute(Sender: TObject);
    procedure Cmd_DocumenteExecute(Sender: TObject);
    procedure Cmd_DocumenteValidareExecute(Sender: TObject);
    procedure Cmd_IntretinereFunctiiExecute(Sender: TObject);
    procedure Cmd_IntretinereTipuriMaterialeExecute(Sender: TObject);
    procedure Cmd_WizardDocumExecute(Sender: TObject);
    procedure Cmd_DecontariExecute(Sender: TObject);
    procedure Cmd_StocuriUnitateExecute(Sender: TObject);
    procedure Cmd_ImportNoteExecute(Sender: TObject);
    procedure Cmd_RegistruDocumenteExecute(Sender: TObject);
    procedure Cmd_PlanificareBugetExecute(Sender: TObject);
    procedure Cmd_IntretinereCasaExecute(Sender: TObject);
    procedure Cmd_RegistruExecute(Sender: TObject);
    procedure Cmd_IntretinereCuloriExecute(Sender: TObject);
    procedure Cmd_IntretinerTipDocExecute(Sender: TObject);
    procedure Cmd_CorespTipDocExecute(Sender: TObject);
    procedure Cmd_IntretinereTipCheltuieliExecute(Sender: TObject);
    procedure Cmd_DocumenteValidateExecute(Sender: TObject);
    procedure Cmd_RefreshDataSetExecute(Sender: TObject);
    procedure Cmd_UrmarireExecutieExecute(Sender: TObject);
    procedure Cmd_AngajamentGlobalExecute(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure Cmd_IntretinereOrganigramaCasaExecute(Sender: TObject);
    procedure Cmd_DeconturiExecute(Sender: TObject);
    procedure Cmd_PreluareBugetExecute(Sender: TObject);
    procedure FormClose(Sender: TObject; var Action: TCloseAction);
    procedure Cmd_DefinireDecontExecute(Sender: TObject);
    procedure Cmd_ValidareDecontExecute(Sender: TObject);
    procedure Cmd_GenerareOPExecute(Sender: TObject);
    procedure Cmd_ModificareOPExecute(Sender: TObject);
    procedure Cmd_IntretinereAnexeBilantExecute(Sender: TObject);
    procedure Cmd_IntretinereBugetDirectiiExecute(Sender: TObject);
    procedure Cmd_IntretinereBugetProiecteExecute(Sender: TObject);
    procedure Cmd_BugetAprobatExecute(Sender: TObject);
    procedure Cmd_IntretinereAntetExecute(Sender: TObject);
    procedure Cmd_SchimbareParolaExecute(Sender: TObject);
    procedure Cmd_IntretinereDelegatiExecute(Sender: TObject);
    procedure Cmd_IntretinereMijloaceTransportExecute(Sender: TObject);
    procedure Cmd_IntretinereTipuriMIjloaceTransportExecute(
      Sender: TObject);
    procedure Cmd_FundamentareBugetExecute(Sender: TObject);
    procedure Cmd_IntretinereNomenclatorExecute(Sender: TObject);
    procedure Cmd_IntretinereAnexeExecutieBugetaraExecute(Sender: TObject);
    procedure Cmd_ContareBugetaraExecute(Sender: TObject);
    procedure Cmd_IntretinereTipProdusExecute(Sender: TObject);
    procedure Cmd_IntretinereTipMaterialeExecute(Sender: TObject);
    procedure Cmd_IntretinereTipStocExecute(Sender: TObject);
    procedure Cmd_IntretinereTipStoc_TipProdusExecute(Sender: TObject);
    procedure Cmd_AboutExecute(Sender: TObject);
    procedure Cmd_InchiderePerioadeFiscaleExecute(Sender: TObject);
    procedure Cmd_IntretinereTipuriRepartitoriExecute(Sender: TObject);
    procedure Cmd_IntretinereOrganizatieExecute(Sender: TObject);
    procedure Cmd_BGFundamentareExecute(Sender: TObject);
    procedure Cmd_OITipuriProiecteExecute(Sender: TObject);
    procedure Cmd_OIProiecteExecute(Sender: TObject);
    procedure ApplicationEventsHint(Sender: TObject);
    procedure ApplicationEventsShowHint(var HintStr: String;
      var CanShow: Boolean; var HintInfo: Controls.THintInfo);
    procedure Cmd_IntertinereBGPlanExecute(Sender: TObject);
    procedure Cmd_OEIntretinereRepartitoriExecute(Sender: TObject);
    procedure FormShow(Sender: TObject);
    procedure Cmd_GEST_RegistruDocumenteExecute(Sender: TObject);
    procedure Cmd_ALOPAdaugaAngajamentExecute(Sender: TObject);
    procedure Cmd_BGAprobatExecute(Sender: TObject);
    procedure Cmd_AlopListaAngajamenteExecute(Sender: TObject);
    procedure TreeProiecteDblClick(Sender: TObject);
    procedure cxUnitatePopupPropertiesInitPopup(Sender: TObject);
    procedure Cmd_ALOPLichidareExecute(Sender: TObject);
    procedure Cmd_NoteInchidereExecute(Sender: TObject);
    procedure Cmd_AlopListaOrdonantareExecute(Sender: TObject);
    procedure Cmd_AlopIntretinereConturiExecute(Sender: TObject);
    procedure Cmd_GenerarePlataExecute(Sender: TObject);
    procedure Cmd_NoteImperechereExecute(Sender: TObject);
    procedure Cmd_RapImplicitExecute(Sender: TObject);
    procedure Cmd_CulegeAnexeSubunitatiExecute(Sender: TObject);
    procedure Cmd_CumulareAnexeExecute(Sender: TObject);
    procedure Cmd_FRGeneratorRapoarteExecute(Sender: TObject);
    procedure Cmd_FRExecute_ReportExecute(Sender: TObject);
    procedure FRrapExplorerCloseQueryExplorer(Sender: TObject;
      var CanClose: Boolean);
    procedure Cmd_PreluareAnexeExecute(Sender: TObject);
    procedure Cmd_FisaBugetaraExecute(Sender: TObject);
    procedure Cmd_TestEroareExecute(Sender: TObject);
    procedure FRrapExplorerNewReport(Sender: TObject; Report: TfrxReport;
      Id: Integer);
    procedure Cmd_SituatieTertExecute(Sender: TObject);
    procedure Cmd_IntretinereTipDocExecute(Sender: TObject);
    procedure Cmd_PreviewAnexeExecute(Sender: TObject);
    procedure MainMenuMerge(Sender, ChildBarManager: TdxBarManager;
      AddItems: Boolean);
    procedure CmdGenCodBaraExecute(Sender: TObject);
    procedure Cmd_AlopDispozitieExecute(Sender: TObject);
    procedure Cmd_AlopListaDispozitiiExecute(Sender: TObject);
    procedure ApplicationEventsSettingChange(Sender: TObject; Flag: Integer;
      const Section: string; var Result: Integer);
    procedure Cmd_ContracteListaExecute(Sender: TObject);
    procedure Cmd_ContracteExecute(Sender: TObject);
    procedure Cmd_TranspunerePlanExecute(Sender: TObject);
    procedure Cmd_CompensariExecute(Sender: TObject);
    procedure Cmd_BazaSchimbareExecute(Sender: TObject);
    procedure ApplicationEventsException(Sender: TObject; E: Exception);
    procedure Cmd_GrupeProiectExecute(Sender: TObject);
    procedure Cmd_RegistruNouExecute(Sender: TObject);
    procedure Cmd_PreluareExtraseExecute(Sender: TObject);
    procedure MainMenuAfterOpen(Sender: TObject);
    procedure Cmd_AfisareNavigareExecute(Sender: TObject);
    procedure Cmd_NavigareWebExecute(Sender: TObject);
  protected
    procedure WMRefreshDataSet(var Message: TMessage); message WM_REFRESH_QUERY;
    procedure InitMainMenu;
    procedure InitFormProvider;
    procedure FRrapExplorerAfterLoad(Sender: TfrxReport);
    procedure CreateMeniuSkin;
    procedure DoChangeSkin(Sender: TObject);
    procedure SetCurrentSkinName(const ASkinName: String);
    procedure SetSkinCommandExecute(Sender: TObject; Command: TATSCommand; var Allow: Boolean);
  private
    { Private declarations }
//    FTabTop: TrkSmartTabs;
//    FTabTop : TcxTabControl;
    FFastReportCommand: TATSCommand;

    FRProgressForm : TfrmProgressRap;
    procedure DoFRClick(Sender : TObject);
    procedure SIProcMessage(var Msg: TMessage); message SIM_PROCMESS;
    function  SelectTab(ATabName: String): Boolean;
  protected
    procedure LocalOnGetNewFormFinish(aForm : TForm);
    procedure StartProgress(Sender: TfrxReport; ProgressType: TfrxProgressType; Progress: Integer);
    procedure EndProgress(Sender: TfrxReport; ProgressType: TfrxProgressType; Progress: Integer);
    procedure DoProgress(Sender: TfrxReport; ProgressType: TfrxProgressType; Progress: Integer);
    procedure ResetCounterOnDialog(Page: TfrxDialogPage);
    procedure InitBar(aForm : TForm; var aBarManager : TdxBarManager);
    procedure CreateBarManagerFromMenu(aForm : TForm; aMenu: TMainMenu);
    procedure ConvertMenuToHamburgMenu(ABar: TdxBar; ANavBar: TdxNavBar);

    procedure RepOnPreview(Sender: TObject);
  public
    procedure SetRaportParams(aReport : TfrxReport);
  public
    FRrapExplorer: TFR4RapExplorer;
    procedure RefreshMenuBar;
    procedure DoShowReport(ARepIndex: Integer);
    procedure DoDesignReport(AReport: TfrxReport);
    procedure OpenDelegati(const aIdRepartitor : Integer = -1);
    procedure OpenMijlTransport(const aIdRepartitor : Integer = -1);
    procedure TabReport(AReport: TfrxReport);
  end;

var
  mainForm: TmainForm;
  FREvent : TNotifyEvent = nil;

implementation

uses
  WebNavUtils,
  AtlasUtils,
  AtlasSkinUnit,
  fmGrupaProiecteUnit,
  regionalSettingsUnit,
  fmFR4ExplorerUnit,
  frxDCtrl,
  frxUtils,
  frxZeosComponents,
  DB,
  ZeosDBUtile,
  ATSZDBUtils,
  FmEditZeosMenuUnit,
  ConcurentUsersUnit,
  infoCustomizeHook,
  DateUnit,
  BalantaUnit,
  DecontariUnit,
  FunctionUnit,
  DocumenteUnit,
  TCVUnit,
  UtilizatoriUnit,
  JurnaleUnit,
  AutoNoteContUnit,
  TCVUnitateUnit,
  IntretinNmclUnit,
  AngajamenteGlobaleUnit,
  UrmarireExecutieUnit,
  RegistruUnitEx,
  CasaUnit,
  IntretinereCaseUnit,
  CommonCasa,
  OrganigramaUnit,
  BugetImpEXCEL,
  fmContareBugetaraUnit,
  SituatieDeconturiUnit,
  DirectiiUnit,
  ProiectUnit,
  frmPreluareExtraseUnit,
  AntetUnit,
  ChangePassUnit,
  DelegatiUnit,
  MijloaceTransportUnit,
  NormalizareNomUnit,
  IntretinereTipProduse,
  IntretinTipMateriale,
  IntretinTipStoc,
  IntretinTipuriStocProdus,
  AboutUnit,
  UnitInchiderePer,
  TipRepartitori,
  BxPlanificare,
  OI_ProiecteTipuri,
  OI_Proiecte,
  BgPlanUnit,
  OI_Unitati,
  Gest_ModifyDocum,
  AlopAngajamente,
  AlopAngVizualizare,
  Variants,
  AlopLichidare,
  OERepartitoriUnit,
  NoteInchidereUnit,
  AlopOrdList,
  AlopIntretinereConturi,
  GenerarePlataUnit,
  ImperechereNote,
  RapImplicit,
  AnexeCulegere,
  AnexeCentralizare,
  BugetImpAnexe,
  BugetFisaUnit,
  imperechereTert,
  CTipuriDocumente,
  frmPreviewAnexaUnit,
  SetParamsUnitADO,
  frxDesgn,
  GenCoduriBara,
  AlopDispozitie,
  AlopDispVizualizare,
  NoteUnitNew,
  uContracte,
  uContracteEdit,
  ListaContracte,
  frmContaTranspunereAnUnit,
  DecontariCompensareUnit,
  dxGDIPlusAPI;

{$R *.DFM}

procedure TmainForm.FormCreate(Sender: TObject);
var
  AdminStr: String;
  lActionList : TActionList;
  I, J : Integer;
begin

//  FTabTop := TcxTabControl.Create(Self);
//  FTabTop.Parent := Self;

  FRProgressForm := nil;

  lActionList := GetMainActionList;
  for I := lActionList.ActionCount - 1 downto 0 do begin
    for J := 0 to Actiuni.ActionCount - 1 do
      if AnsiSameText(Actiuni.Actions[J].Name, lActionList.Actions[I].Name) then begin
        Actiuni.Actions[J].Name := Actiuni.Actions[J].Name + '_1';
        Break;
      end;
    lActionList.Actions[I].ActionList := Actiuni;
  end;

  FRrapExplorer := TFR4RapExplorer.Create(nil);

  FRrapExplorer.Active            := False;
  FRrapExplorer.Name              := 'FRrapExplorer';
  FRrapExplorer.AutoInitSQL       := False;
  FRrapExplorer.AutoShow          := False;
  FRrapExplorer.ModalShow         := True;
  FRrapExplorer.DeltaZile         := 3;
  FRrapExplorer.Compresat         := True;
  FRrapExplorer.Criptat           := False;
  FRrapExplorer.UserID            := 0;
  FRrapExplorer.SelectedDB        := False;
  FRrapExplorer.OnNewReport       := FRrapExplorerNewReport;
//  FRrapExplorer.OnAfterLoadREport := FRrapExplorerAfterLoad;
  FRrapExplorer.ZEOSConnection    := frmData.dbContabilitate;
  FRrapExplorer.Open;
  FRrapExplorer.ModalShow         := False;
  FRrapExplorer.OnDesignReport    := DoDesignReport;
  FRrapExplorer.Explorer.FPreviewFormParent := nil;

  if IsAdmin then AdminStr := ' [Administrator] ' else AdminStr := '';
  Caption := 'Atlas modul contabilitate ver. '+ExeVersion + '(svnVer.: ' + svnRevision  + ') ' +' ['+szDBName+'/'+szServerName+'] : '+NumeLoginComplet+ AdminStr +' Zeos ver. : '+FrmData.dbContabilitate.Version;

  MainStatusBar.Panels[0].Text := NumeLoginComplet;
  MainStatusBar.Panels[1].Text := szDBName+'/'+ExeVersion + ' (svnVer.: '+svnRevision+')' + '/ ' + szServerName;
  MainStatusBar.Panels[2].Text := '';
  MainStatusBar.Panels[3].Text := '';

  InitMainMenu;

  ATLASSIUtils.InitData(Self, MainMenu.MainMenuBar);

  AddCommandsHandlers(Actiuni);

end;

procedure TmainForm.Cmd_CulegereNoteExecute(Sender: TObject);
begin
  if EnterSingleUser(TFrmCulegereNote) then
    GetNewForm(TFrmCulegereNote);
end;

procedure TmainForm.Cmd_IntretinerePlanConturiExecute(Sender: TObject);
begin
  GetNewForm(TFrmPlanConturi);
end;

procedure TmainForm.Cmd_IntretinerePlanBugeteExecute(Sender: TObject);
begin
  GetNewForm(TfrmBGPlan, 'Intretinere de plan buget');
end;

procedure TmainForm.Cmd_IstoricNoteExecute(Sender: TObject);
begin
  with TFrmListaNoteNew(GetNewForm(TFrmListaNoteNew, 'Lista Completa Note contabile !')) do begin
    edOperator.EditValue  := IdUtilizator;
    edOperator.Visible    := True;
    RefreshFilter;
    QryListaNote.Open;
  end;
end;

procedure TmainForm.Cmd_BalantaExecute(Sender: TObject);
begin
  TFrmBalanta(GetNewForm(TFrmBalanta, '', True)).SelectUltimaLuna;
end;

procedure TmainForm.ComenziNewCommand(Sender: TObject;
  Command: TATSCommand);

  procedure AdaugaComanda(const AValue: Variant; const ACaption, AAction, AHint: String; Event: TATSOnExecuteCmd);
  begin
    with TATSCommand.Create(Command) do begin
      Caption   := ACaption;
      Value     := AValue;
      Hint      := AHint;
      OnExecute := EVent;
    end;
  end;

  procedure AdaugaComandaSkin(ASkinName: String; const ACaption: String; const ACommand: String);
  begin
    AdaugaComanda(ASkinName, ACaption, '', 'Setare Skin ' + ACaption, SetSkinCommandExecute);
  end;

var
  lSkinList: TStringList;
  I: Integer;
begin
  case Command.CmdType of
    ctSkinType :
      begin
        lSkinList := TStringList.Create;
        try
          if GetSkinNameList(lSkinList) then begin
            AdaugaComandaSkin('', 'Afisare normala', 'Cmd_Afisare_Normala');
            for I := 0 to lSkinList.Count-1 do begin
              AdaugaComandaSkin(lSkinList[I], lSkinList[I], 'Cmd_Afisare_'+lSkinList[I]);
            end;
          end;
        finally
          lSkinList.Free;
        end;
      end;
    ctReportListFast,
    ctMainPopup:
      FFastReportCommand := Command;
  end;
end;

procedure TmainForm.ConvertMenuToHamburgMenu(ABar: TdxBar; ANavBar: TdxNavBar);

  procedure CreateInnerMenu(const ACaption: String; AItemLinks: TdxBarItemLinks; AParentGroup: TdxNavBarGroup);
  var
    I: Integer;
    lGroup: TdxNavBarGroup;
    lItem : TdxNavBarItem;
  begin
    lGroup := ANavBar.Groups.Add;
    lGroup.Caption := ACaption;
    if Assigned(AParentGroup) then lGroup.MoveTo(AParentGroup, lGroup.ChildCount);
    for I := AItemLinks.Count-1 downto 0 do begin
      if AItemLinks[I].Item is TdxBarSubItem then
        CreateInnerMenu(AItemLinks[I].Item.Caption, TdxBarSubItem(AItemLinks[I].Item).ItemLinks, lGroup)
      else begin
        lItem := ANavBar.Items.Add;
        lItem.Caption := AItemLinks[I].Item.Caption;
        lItem.Action  := AItemLinks[I].Item.Action;
        lItem.OnClick := AItemLinks[I].Item.OnClick;
        lItem.Tag     := AItemLinks[I].Item.Tag;
        lGroup.CreateLink(lItem);
      end;
    end;
  end;

begin
  ANavBar.Items.Clear;
  ANavBar.Groups.Clear;
  CreateInnerMenu(ABar.Caption, ABar.ItemLinks, nil);
end;

procedure TmainForm.ComenziBeforeOpen(Sender: TObject);
begin
  FFastReportCommand := nil;
end;

procedure TmainForm.ComenziAfterOpen(Sender: TObject);
begin
  Cmd_RefreshReports.Execute;
end;

procedure TmainForm.Cmd_RefreshReportsExecute(Sender: TObject);
begin
  if FFastReportCommand <> nil then
    try
      if FRRapCommands.ActiveDB then FRRapCommands.ActiveDB := False;
      FRRapCommands.Active := True;
      FFastReportCommand.ClearCurentCmds;
      FFastReportCommand.LoadFromCmds(FRRapCommands);
      MainMenu.RefreshCommand(FFastReportCommand);
      FRRapCommands.Active := False;
      FRRapCommands.ActiveDB := False;
    except
     // Ignoram Exceptiile
    end;
end;

procedure TmainForm.Cmd_ModificareMeniuExecute(Sender: TObject);
begin
  if EditArboreApel(Comenzi, CommonDbVar.IdFunctiune, CommonDbVar.IdUtilizator, CommonDbVar.IsAdmin) then begin

    Comenzi.ActiveDB        := False;
    RapCommands.ActiveDB    := False;
    FRRapCommands.ActiveDB  := False;
    Comenzi.Active          := False;
    RapCommands.Active      := False;
    FRRapCommands.Active    := False;

    MainMenu.Active         := False;
    MainMenu.Active         := True;

    Comenzi.ActiveDB        := False;
    RapCommands.ActiveDB    := False;
    FRRapCommands.ActiveDB  := False;

  end;
end;

procedure TmainForm.Cmd_IntretinValuteExecute(Sender: TObject);
var
  formular : TfrmIntretinereNmcl;
begin
  formular := AddNmclForm('lst_valute', 'id_valuta', 'nomenclator valute');
  formular.ShowModal;
end;

procedure TmainForm.Cmd_IntretinereRepartitoriExecute(Sender: TObject);
begin
  GetNewForm(TFrmOERepartitori);
end;

procedure TmainForm.Cmd_IntretinereJurnaleExecute(Sender: TObject);
begin
  GetNewForm(TfrmJurnale);
end;

procedure TmainForm.Cmd_IntretinereUtilizatoriExecute(Sender: TObject);
begin
  GetNewForm(TfrmUtilizatori);
end;

procedure TmainForm.Cmd_AngajamenteExecute(Sender: TObject);
begin
  if EnterSingleUser(TfrmAlopAngajamente) then
    TfrmAlopAngajamente(GetNewForm(TfrmAlopAngajamente)).ReadAngajament();
end;

procedure TmainForm.Cmd_TranzactiiCVExecute(Sender: TObject);
begin
  if EnterSingleUser(TFrmTCV) then
    TFrmTCV(GetNewForm(TFrmTCV)).ReadDocument();
end;

procedure TmainForm.Cmd_DocumenteExecute(Sender: TObject);
begin
  GetNewForm(TfrmDocumente);
end;

procedure TmainForm.Cmd_DocumenteValidareExecute(Sender: TObject);
begin
  TfrmActual(GetNewForm(TfrmActual, 'Documente spre validare')).Validate := False;
end;

procedure TmainForm.Cmd_IntretinereFunctiiExecute(Sender: TObject);
begin
  GetNewForm(TfrmFunctii);
end;

procedure TmainForm.Cmd_IntretinereTipuriMaterialeExecute(Sender: TObject);
begin
  AddNmclForm('GEST_TIP_MATERIAL', 'ID_GEST_TIP_MATERIAL', 'nomenclator materiale').ShowModal;
end;

procedure TmainForm.Cmd_WizardDocumExecute(Sender: TObject);
begin
{
  with TfrmWzModDefaDocum.Create(Application) do
    try
       IdDocument := -1;
       ShowModal;
    finally
       Free;
    end;
}
end;

procedure TmainForm.Cmd_DecontariExecute(Sender: TObject);
begin
  TfrmDecontari(GetNewForm(TfrmDecontari, 'Decontari Furnizori/ Clienti')).RefreshIncasari();
end;

procedure TmainForm.Cmd_StocuriUnitateExecute(Sender: TObject);
begin
  GetNewForm(TfrmSituatieUnitate, 'Stocuri Unitate');
end;

procedure TmainForm.Cmd_ImportNoteExecute(Sender: TObject);
begin
  ShowWizardNoteCont;
end;

procedure TmainForm.Cmd_RegistruDocumenteExecute(Sender: TObject);
begin
  GetNewForm(TfrmGEST_ModifyDocum, 'Vizualizare Document');
end;

procedure TmainForm.Cmd_PlanificareBugetExecute(Sender: TObject);
begin
  TfrmBxPlanificare(GetNewForm(TfrmBxPlanificare)).InitBGAprobat;
end;

procedure TmainForm.Cmd_RegistruExecute(Sender: TObject);
begin
  if EnterSingleUser(TfrmCasa) then
    GetNewForm(TfrmCasa);
end;

procedure TmainForm.Cmd_IntretinereCasaExecute(Sender: TObject);
begin
  GetNewForm(TfrmIntretinCasa);
end;

procedure TmainForm.Cmd_IntretinereCuloriExecute(Sender: TObject);
begin
  SettingsMaintenance;
end;

procedure TmainForm.Cmd_IntretinerTipDocExecute(Sender: TObject);
begin
  AddNmclForm('tipuri_doc', 'id_tipuri_doc', 'Nomeclator tipuri de documente care apartin de casa').ShowModal;
end;

procedure TmainForm.Cmd_CorespTipDocExecute(Sender: TObject);
begin
//  with TfrmRemapare.Create(Self) do Show;
end;

procedure TmainForm.Cmd_IntretinereTipCheltuieliExecute(Sender: TObject);
begin
//  with TFrmTipCheltuiala.Create(Self) do Show;
end;

procedure TmainForm.Cmd_DocumenteValidateExecute(Sender: TObject);
begin
  TfrmActual(GetNewForm(TfrmActual, 'Documente validate')).Validate := True;
end;

procedure TmainForm.Cmd_RefreshDataSetExecute(Sender: TObject);
begin
  DBConnection.Reconnect;
end;

procedure TmainForm.Cmd_UrmarireExecutieExecute(Sender: TObject);
begin
  GetNewForm(TfrmUrmarireExecutie);
end;

procedure TmainForm.Cmd_AngajamentGlobalExecute(Sender: TObject);
begin
  GetNewForm(TfrmAngajamenteGlobale);
end;

procedure TmainForm.WMRefreshDataSet(var Message: TMessage);
begin
  Exit;
  if csDestroying in ComponentState then Exit;
  if (TDataSet(Message.WParam) <> nil) and (TDataSet(Message.WParam).Active = False) then
     TDataSet(Message.WParam).Active := True;
end;

procedure TmainForm.FormDestroy(Sender: TObject);
begin
  FormProvider.TabControl := nil;
  frmData.dbContabilitate.Disconnect;
  ATLASSIUtils.FreeData;
end;

procedure TmainForm.Cmd_IntretinereOrganigramaCasaExecute(Sender: TObject);
begin
  with TFrmOrganigrama.Create(Application) do
    try
      ShowModal;
    finally
      Free;
    end;
end;

procedure TmainForm.Cmd_DeconturiExecute(Sender: TObject);
begin
  GetNewForm(TFrmSituatieDeconturi, 'Situatie Deconturi');
end;

procedure TmainForm.Cmd_PreluareBugetExecute(Sender: TObject);
begin
  GetNewForm(TfrmBxImportEXCEL, 'Import Excel');
end;

procedure TmainForm.FormClose(Sender: TObject; var Action: TCloseAction);
var I: Integer;
begin
  for I := 0 to MainForm.ComponentCount - 1 do
    if MainForm.Components[I] is TForm then
      TForm(MainForm.Components[I]).Close;
  if FRrapExplorer <> nil then
    FRrapExplorer.Free;
end;

procedure TmainForm.Cmd_DefinireDecontExecute(Sender: TObject);
begin
{
  frmDefinireDecont := TfrmDefinireDecont.Create(Self);
  frmDefinireDecont.Visible := True;
  frmDefinireDecont.WindowState := wsMaximized;
}
end;

procedure TmainForm.Cmd_ValidareDecontExecute(Sender: TObject);
begin
{
  frmValidareDecont := TfrmValidareDecont.Create(Self);
  frmValidareDecont.CodCb := -1;
  frmValidareDecont.Visible := True;
  frmValidareDecont.WindowState := wsMaximized;
}
end;

procedure TmainForm.Cmd_GenerareOPExecute(Sender: TObject);
begin
  DoImperechere;
  {
 with TfrmGenerareOP.Create(Application) do
   try
      QryFacturi.Open;
      InitFields;
      ShowModal;
   finally
      Free;
   end;
 }
End;


procedure TmainForm.Cmd_ModificareOPExecute(Sender: TObject);
begin
  DoListare;
 {
 with TfrmGenerareOP.Create(Application) do
   try
      Drepturi := 2;
      QryFacturi.Open;
      InitFields;
      ShowModal;
   finally
      Free;
   end;
 }
end;

procedure TmainForm.Cmd_IntretinereBugetDirectiiExecute(Sender: TObject);
begin
  GetNewForm(TfrmBugetDirectii);
end;

procedure TmainForm.Cmd_IntretinereBugetProiecteExecute(Sender: TObject);
begin
  GetNewForm(TfrmBugetProiect);
end;

procedure TmainForm.Cmd_BugetAprobatExecute(Sender: TObject);
begin
{
  with TfrmBugetAprobat.Create(Application) do
    try
       WindowState := wsMaximized;
       ShowModal;
    finally
       Free;
    end;}
end;

procedure TmainForm.Cmd_IntretinereAntetExecute(Sender: TObject);
begin
  ShowIntretinereAntet;
end;

procedure TmainForm.Cmd_SchimbareParolaExecute(Sender: TObject);
var
  lNewPass: String;
begin
  if FrmData.QryOperatori.Locate('ID_UTILIZATORI', CommonDbVar.IdUtilizator, []) and
     ChangePassword(FrmData.QryOperatori['PAROLA'], FrmData.QryOperatori['NUMEINTREG'], lNewPass, nil) then
     DBSetFieldValue(FrmData.QryOperatori, 'PAROLA', lNewPass);
end;

procedure TmainForm.Cmd_IntretinereAnexeBilantExecute(Sender: TObject);
begin
  GetNewForm(TfrmAnexeBilant);
end;

procedure TmainForm.Cmd_IntretinereDelegatiExecute(Sender: TObject);
begin
  OpenDelegati;
end;

procedure TmainForm.Cmd_IntretinereMijloaceTransportExecute(
  Sender: TObject);
begin
  OpenMijlTransport;
end;

procedure TmainForm.Cmd_IntretinereTipuriMIjloaceTransportExecute(
  Sender: TObject);
begin
  AddNmclForm('LST_TIPURI_TRANSPORT', 'ID_LST_TIPURI_TRANSPORT', 'Nomenclator De Tipuri de Mijloace de Transport').ShowModal;
end;

procedure TmainForm.Cmd_FundamentareBugetExecute(Sender: TObject);
begin
  CreateBGFundamentare;
end;

procedure TmainForm.Cmd_IntretinereNomenclatorExecute(Sender: TObject);
begin
  GetNewForm(TfrmNormalizareNom);
end;

procedure TmainForm.Cmd_IntretinereAnexeExecutieBugetaraExecute(
  Sender: TObject);
begin
//  GetNewForm(TfrmAnexeEB, Application);
end;

procedure TmainForm.Cmd_ContareBugetaraExecute(Sender: TObject);
begin
  GetNewForm(TfrmContareBugetara);
end;

procedure TmainForm.InitMainMenu;
begin
  MainMenu.RegistryPath := RegKeyPrefix+'Meniu';
  Comenzi.Active := False;
  Comenzi.Params.ParamByName('ID_UTILIZATORI').Value := IdUtilizator;
  if IsAdmin then Comenzi.Params.ParamByName('IS_ADMIN').Value := 1
  else Comenzi.Params.ParamByName('IS_ADMIN').Value := 0;
  Methods.Active   := True;
  MainMenu.Active  := True;
  Comenzi.ActiveDB := False;
  RapCommands.ActiveDB := False;
  FRRapCommands.ActiveDB := False;
end;

procedure TmainForm.InitFormProvider;
begin
  tabTop.Height          := Self.Canvas.TextHeight('W') * 2;
//  FTabTop.Top             := 200;
//  FTabTop.Height          := FTabTop.Canvas.TextHeight('W') * 2;
//  FTabTop.Align           := alTop;
//  FTabTop.Visible         := True;
//  FTabTop.Properties.CloseButtonMode  := cbmActiveAndHoverTabs;
//  FTabTop.Properties.Options          := [pcoAlwaysShowGoDialogButton, pcoCloseButton, pcoGoDialog, pcoRedrawOnResize];
//  FTabTop.Properties.Style            := 9;
  pnContent.Align         := alClient;
  FormProvider.TabControl := tabTop;
  FormProvider.ClientArea := pnContent;
end;

procedure TmainForm.Cmd_IntretinereTipProdusExecute(Sender: TObject);
begin
  GetNewForm(TfrmIntretinereTipProd, 'Intretinere Tipuri Produse');
end;

procedure TmainForm.Cmd_IntretinereTipMaterialeExecute(Sender: TObject);
begin
  GetNewForm(TfrmIntretinereTipMat, 'Intretinere Tipuri Materiale');
end;

procedure TmainForm.Cmd_IntretinereTipStocExecute(Sender: TObject);
begin
  GetNewForm(TfrmIntretinTipStoc, 'Intretinere Tipuri Stocuri');
end;

procedure TmainForm.Cmd_IntretinereTipStoc_TipProdusExecute(
  Sender: TObject);
begin
  GetNewForm(TfrmIntertinTipStocProdus, 'Influente Stocuri Produs');
end;

procedure TmainForm.Cmd_AboutExecute(Sender: TObject);
begin
  ShowAboutForm;
end;

procedure TmainForm.Cmd_AfisareNavigareExecute(Sender: TObject);
begin
  HamMenu.Visible := not HamMenu.Visible;
  if HamMenu.Visible then
    ConvertMenuToHamburgMenu(MainMenu.BarByCaption(MainMenu.BarCaption), HamMenu);
end;

procedure TmainForm.Cmd_InchiderePerioadeFiscaleExecute(Sender: TObject);
begin
  IntretinerePerioadeFiscale;
end;

procedure TmainForm.Cmd_IntretinereTipuriRepartitoriExecute(
  Sender: TObject);
begin
  ShowTipuriRepartitori();
end;

procedure TmainForm.Cmd_IntretinereOrganizatieExecute(Sender: TObject);
begin
  GetNewForm(TfrmOIUnitatiNew);
end;

procedure TmainForm.OpenDelegati(const aIdRepartitor: Integer);
begin
  ShowDelegatii(aIdRepartitor);
end;

procedure TmainForm.OpenMijlTransport(const aIdRepartitor: Integer);
begin
  ShowMijlocTransport(aIdRepartitor);
end;

procedure TmainForm.Cmd_BGFundamentareExecute(Sender: TObject);
begin
   CreateBGFundamentare;
end;

procedure TmainForm.LocalOnGetNewFormFinish(aForm: TForm);
begin
//  if (aForm <> nil) and (aForm.FormStyle = fsMDIChild) then
//    AdvOfficeMDITabSet1.AddTab(aForm);
end;

procedure TmainForm.Cmd_OITipuriProiecteExecute(Sender: TObject);
begin
  ShowTipuriProiecte;
end;

procedure TmainForm.Cmd_OIProiecteExecute(Sender: TObject);
begin
  GetNewForm(TfrmOIProiecte);
end;

procedure TmainForm.ApplicationEventsHint(Sender: TObject);
begin
  SetHintInfo(Application.Hint);
end;

procedure TmainForm.ApplicationEventsSettingChange(Sender: TObject;
  Flag: Integer; const Section: string; var Result: Integer);
begin
  InitRegionalSettings;
end;

procedure TmainForm.ApplicationEventsShowHint(var HintStr: String;
  var CanShow: Boolean; var HintInfo: Controls.THintInfo);
begin
//  CanShow := (HintInfo.HintControl = FTabTop);
  CanShow := (HintInfo.HintControl = tabTop);
end;

procedure TmainForm.Cmd_IntertinereBGPlanExecute(Sender: TObject);
begin
  GetNewForm(TfrmBGPlan, 'Intretinere de plan buget');
end;

procedure TmainForm.Cmd_OEIntretinereRepartitoriExecute(Sender: TObject);
begin
  DBRefresh(frmData.qryRepartitori);
  GetNewForm(TFrmOERepartitori);
end;

procedure TmainForm.FormShow(Sender: TObject);
begin
  InitFormProvider;
end;

procedure TmainForm.Cmd_GEST_RegistruDocumenteExecute(Sender: TObject);
begin
  GetNewForm(TfrmGEST_ModifyDocum, 'Vizualizare Document');
end;

procedure TmainForm.Cmd_ALOPAdaugaAngajamentExecute(Sender: TObject);
begin
  if EnterSingleUser(TfrmAlopAngajamente) then
    TfrmAlopAngajamente(GetNewForm(TfrmAlopAngajamente)).ReadAngajament();
end;

procedure TmainForm.Cmd_BGAprobatExecute(Sender: TObject);
begin
  TfrmBxPlanificare(GetNewForm(TfrmBxPlanificare)).InitBGAprobat;
end;

procedure TmainForm.Cmd_AlopListaAngajamenteExecute(Sender: TObject);
begin
  TfrmAlopAngajamenteVizualizare(GetNewForm(TfrmAlopAngajamenteVizualizare, 'Vizualizare Angajamente')).IsOrdonantare := 0;
end;

procedure TmainForm.TreeProiecteDblClick(Sender: TObject);
begin
   with TcxDBTreeList(Sender) do
    if (FocusedNode <> nil) and (not FocusedNode.HasChildren) then
   (GetParentForm(TcxDBTreeList(Sender)) as TcxPopupEditPopupWindow).ModalResult := mrOk;
end;

procedure TmainForm.cxUnitatePopupPropertiesInitPopup(Sender: TObject);
begin
  DBRefresh(frmData.qryOIProiecte);
end;

procedure TmainForm.Cmd_ALOPLichidareExecute(Sender: TObject);
var aForm : TCustomForm;
begin
 aForm := GetNewForm(TfrmAlopLichidare);
  with TfrmAlopLichidare(aForm) do begin
    Caption := 'Lichidare';
    WindowState := wsMaximized;
    ReadOrdonantare;
    Show;
  end;
end;

procedure TmainForm.Cmd_NoteInchidereExecute(Sender: TObject);
begin
  with TfrmNoteInchidere.Create(nil) do
  try
     ShowModal;
  finally
    Free;
  end;
end;

procedure TmainForm.Cmd_AlopListaOrdonantareExecute(Sender: TObject);
var aForm : TCustomForm;
begin
 aForm := GetNewForm(TfrmALOPListaOrd, 'Vizualizare Ordonantare');
  with aForm do begin
    Caption := 'Vizualizare Ordonantare';
    WindowState := wsMaximized;
    Show;
  end;
end;

procedure TmainForm.Cmd_AlopIntretinereConturiExecute(Sender: TObject);
begin
  IntretinereAlopConturi;
end;

procedure TmainForm.Cmd_GenerarePlataExecute(Sender: TObject);
begin
  DoImperechere;
end;

procedure TmainForm.Cmd_NavigareWebExecute(Sender: TObject);
begin
  FormProvider.SetNewForm(NewAtlasNavPage('https://www.google.com'));
end;

procedure TmainForm.Cmd_NoteImperechereExecute(Sender: TObject);
begin
 if EnterSingleUser(TfrmImperechereNote) then
    with TfrmImperechereNote(GetNewForm(TfrmImperechereNote)) do
    begin
      WindowState := wsMaximized;
      Show;
    end;
end;

procedure TmainForm.Cmd_RapImplicitExecute(Sender: TObject);
begin
  EditareRapoarteImplicite;
end;

procedure TmainForm.Cmd_CulegeAnexeSubunitatiExecute(Sender: TObject);
begin
    with TfrmAnexeCulegere(GetNewForm(TfrmAnexeCulegere)) do
    begin
      WindowState := wsMaximized;
      Show;
    end;
end;

procedure TmainForm.Cmd_CumulareAnexeExecute(Sender: TObject);
begin
    with TfrmAnexeCentralizare(GetNewForm(TfrmAnexeCentralizare)) do
    begin
      WindowState := wsMaximized;
      Show;
    end;
end;

procedure TmainForm.Cmd_FRGeneratorRapoarteExecute(Sender: TObject);
var
  lForm: TForm;
begin
 if SelectTab('Explorer rapoarte') then exit;
 If not Assigned(FRrapExplorer.Explorer) then
    FRrapExplorer.Open;
//  FRrapExplorer.Parent := Self;
  lForm := FRrapExplorer.Explorer;
  if Assigned(lForm) then begin
    (*
	if lForm.FormStyle <> fsMDIChild then begin
      lForm.Visible := False;
      lForm.FormStyle := fsMDIChild;
      lForm.BorderStyle := bsSizeable;
      lForm.Align := alNone;
      lForm.WindowState := wsMaximized;
    end;
	*)
    SetNewForm(lForm);
  end;
  FRrapExplorer.Execute;
end;

procedure TmainForm.DoChangeSkin(Sender: TObject);
begin
  SetCurrentSkinName(TCustomAction(Sender).Caption);
end;

procedure TmainForm.DoDesignReport(AReport: TfrxReport);
var
  lDesigner   : TfrxDesignerForm;
  lSetAsModal : Boolean;
  lNewForm    : TForm;
begin

  lSetAsModal := GetAsyncKeyState(VK_CONTROL) and $08000 = $08000;
  if not lSetAsModal then begin
    AReport.PreviewOptions.MDIChild := False;
    AReport.PreviewOptions.Modal := False;
    AReport.OldStyleProgress := True;
    AReport.OnPreview := RepOnPreview;
    lNewForm := TForm.CreateNew(Self);
    lNewForm.BorderStyle := bsNone;
    lNewForm.Align       := alClient;
    AReport.DesignReportInPanel(lNewForm);

    lDesigner               := TfrxDesignerForm(AReport.Designer);
    lDesigner.DockTop.Align := alNone;
    CreateBarManagerFromMenu(lNewForm, lDesigner.Menu);
    lDesigner.DockTop.Top   := 50;
    lDesigner.DockTop.Align := alTop;

    lNewForm.Caption     := lDesigner.Caption;
    lNewForm.WindowState := wsMaximized;
    lNewForm.Tag := -8888;
    SetNewForm(lNewForm);
  end
  else
  begin
    AReport.DesignReport(True, False);
  end;

end;

procedure TmainForm.DoShowReport(ARepIndex: Integer);
var
  lReport : TfrxReport;
  lForm: TForm;
begin
  if not Assigned(FRrapExplorer.Explorer) then FRrapExplorer.Open;
  lReport := FRrapExplorer.Explorer.LoadReport(ARepIndex, False);
  lReport.EngineOptions.DestroyForms := False;
  lReport.PreviewOptions.Modal := False;
  lReport.PreviewOptions.Maximized := True;
  lReport.PreviewOptions.MDIChild := False;

  //lReport.PreviewFormParent := Self;
  //lReport.RefreshParamsProc := FRrapExplorer.Explorer.RefreshParams;

  //SetReportTimeOut(lReport, ctCommandTimeout);
//  lReport.OnProgressStart := StartProgress;
//  lReport.OnProgressStop  := EndProgress;
//  lReport.OnProgress := DoProgress;
//  lReport.Engine.OnRunDialog := ResetCounterOnDialog;


  SetRaportParams(lReport);

  lForm := FRrapExplorer.Explorer.DisplayReport(lReport);
  if Assigned(lForm) then begin
    SetNewForm(lForm);
    lForm.Tag := ARepIndex;
    SendMessage(lForm.Handle, WM_SYSCOMMAND, SC_MAXIMIZE, 0);
  end;
end;


procedure TmainForm.Cmd_FRExecute_ReportExecute(Sender: TObject);
var lReportID: Integer;
begin
  if not(Sender is TATSCommand) then exit;
  lReportID := -TATSCommand(Sender).Value;
  DoShowReport(lReportID);
end;


procedure TmainForm.FRrapExplorerAfterLoad(Sender: TfrxReport);
var
  lIndex: Integer;
begin
  lIndex := Sender.Errors.IndexOf('Failed to set calendar date or time');
  while lIndex > -1 do begin
    Sender.Errors.Delete(lIndex);
    lIndex := Sender.Errors.IndexOf('Failed to set calendar date or time');
  end;
  if Sender.Errors.Count > 0 then begin
    Sender.EngineOptions.NewSilentMode := simMessageBoxes;
    frxCommonErrorHandler(Sender, Sender.Errors.Text);
  end;
end;

procedure TmainForm.FRrapExplorerCloseQueryExplorer(Sender: TObject;
  var CanClose: Boolean);
begin
  Cmd_RefreshReports.Execute;
end;

procedure TmainForm.Cmd_PreluareAnexeExecute(Sender: TObject);
begin
  with GetNewForm(TfrmImportAnexeXLS, 'Import Excel') do begin
    Caption := 'Import Excel';
    WindowState := wsMaximized;
    Show;
  end;
end;

procedure TmainForm.Cmd_FisaBugetaraExecute(Sender: TObject);
begin
  with GetNewForm(TfrmFisaBugetara, 'Fisa Bugetara') do begin
    Caption := 'Fisa Bugetara';
    WindowState := wsMaximized;
    Show;
  end;
end;

procedure TmainForm.Cmd_TestEroareExecute(Sender: TObject);
begin
  //
  raise Exception.Create('Test Eroare !');
//  raise EContaHandledError.Create('Test Eroare !');
end;

procedure TmainForm.DoProgress(Sender: TfrxReport;
  ProgressType: TfrxProgressType; Progress: Integer);
begin
  if not FRProgressForm.Visible then
    FRProgressForm.Visible := True;
end;

procedure TmainForm.EndProgress(Sender: TfrxReport;
  ProgressType: TfrxProgressType; Progress: Integer);
begin
   FRProgressForm.Hide;
   FRProgressForm.Free;
   FRProgressForm := nil;
end;

procedure TmainForm.StartProgress(Sender: TfrxReport;
  ProgressType: TfrxProgressType; Progress: Integer);
begin
  if FRProgressForm = nil then
    FRProgressForm := TfrmProgressRap.Create(Self);
  FRProgressForm.Visible := False;
end;

procedure TmainForm.ResetCounterOnDialog(Page: TfrxDialogPage);
var
  I : Integer;
begin
  for I := 0 to Page.Objects.Count - 1 do begin
    if (TObject(Page.Objects[I]) is TfrxButtonControl) and  (TfrxButtonControl(Page.Objects[I]).ModalResult = mrOK) then  begin
      FREvent := TfrxButtonControl(Page.Objects[I]).Button.OnClick;
      TfrxButtonControl(Page.Objects[I]).Button.OnClick := DoFRClick;
    end;
    if (TObject(Page.Objects[I]) is TfrxBitBtnControl) and  (TfrxBitBtnControl(Page.Objects[I]).ModalResult = mrOK) then  begin
      FREvent := TfrxBitBtnControl(Page.Objects[I]).BitBtn.OnClick;
      TfrxBitBtnControl(Page.Objects[I]).BitBtn.OnClick := DoFRClick;
    end;
  end;
  Page.ShowModal;
  if Page.ModalResult = mrOk then
    if not FRProgressForm.Visible then FRProgressForm.Visible := True;

end;

procedure TmainForm.DoFRClick(Sender: TObject);
begin
  if not FRProgressForm.Visible then
    FRProgressForm.Visible := True;
  if Assigned(FREvent) then
    FREvent(Sender);
  if Sender is TButton then
    TButton(Sender).OnClick := FREvent;
  FREvent := nil;
end;

procedure TmainForm.SIProcMessage(var Msg: TMessage);
begin
   ATLASSIUtils.DoProcessMessage(Msg);
end;

procedure TmainForm.FRrapExplorerNewReport(Sender: TObject;
  Report: TfrxReport; Id: Integer);
begin

  Report.Variables[' ATLAS']     := Null;

  Report.Variables.AddVariable('ATLAS', 'COD_INSPECTOR'   , IdUtilizator);
  Report.Variables.AddVariable('ATLAS', 'ID_UTILIZATORI'  , IdUtilizator);
  Report.Variables.AddVariable('ATLAS', 'IdUtilizator'    , IdUtilizator);
  Report.EngineOptions.NewSilentMode := simSilent;

end;

procedure TmainForm.Cmd_SituatieTertExecute(Sender: TObject);
begin
  with GetNewForm(TfrmImperechereTert, 'Situatie Tert') do begin
    Caption := 'Situatie Tert';
    WindowState := wsMaximized;
    Show;
  end;
end;

procedure TmainForm.Cmd_IntretinereTipDocExecute(Sender: TObject);
begin
  with TfrmCTipuriDocumente.Create(nil) do
  try
     ShowModal;
  finally
    Free;
  end;
end;

procedure TmainForm.Cmd_PreviewAnexeExecute(Sender: TObject);
begin
  PreviewAnexa(-1, 'Vizualizare AnexeDDS');
end;


procedure TmainForm.SetCurrentSkinName(const ASkinName: String);
var
  lSkinName: String;
begin
  lSkinName := StringReplace(ASkinName, '&', '', [rfReplaceAll]);
  SetSkinName(lSkinName);
end;

procedure TmainForm.SetRaportParams(aReport: TfrxReport);
var
  I, J : Integer;
  lCRParam: TAdoCRParam;
begin
  for I := 0 to aReport.DataSets.Count -1 do
    for J := 0 to TfrxZEOSQuery(AReport.DataSets.Items[I].DataSet).Params.Count - 1 do begin
      lCRParam := CRAdoParamByName(TfrxZeosQuery(AReport.DataSets.Items[I].DataSet).Params.Items[J].Name);
      if Assigned(lCRParam) then begin
         TfrxZEOSQuery(AReport.DataSets.Items[I].DataSet).Params.Items[J].DataType := lCRParam.DataType;
         TfrxZEOSQuery(AReport.DataSets.Items[I].DataSet).Params.Items[J].Value := lCRParam.Value;
      end;
    end;
end;

procedure TmainForm.SetSkinCommandExecute(Sender: TObject; Command: TATSCommand;
  var Allow: Boolean);
begin
  SetCurrentSkinName(VarToStr(Command.Value));
end;

function TmainForm.SelectTab(ATabName: String): Boolean;
var
  lIndex: Integer;
begin
  lIndex := tabTop.Tabs.IndexOf(ATabName);
  Result := lIndex > -1;
  if Result then
    tabTop.TabIndex := lIndex;
end;

type
  TAccessdxBarManager = class(TdxBarManager);

procedure TmainForm.CreateBarManagerFromMenu(aForm : TForm; aMenu: TMainMenu);
  { Creem un meniu Bazat pe AtsBar 4.0}
var I   : Integer;
    aBar: tdxBarSubItem;
    aCat : Integer;
    Cool : Boolean;
    lBarManager : TAccessdxBarManager;

    function CreateCategory(aName: String): Integer;
     begin
        Result := lBarManager.Categories.Add(StringReplace(aName,'&','',[rfReplaceAll]));
     end;

     function GetMethod(aMethod: Pointer): TMethod;
      begin
        Result.Data := Self;
        Result.Code := aMethod;
      end;

     function GetMethodByName(aName: String): TNotifyEvent;
      var K: Integer;
      begin
        Result := nil;
        for K := 0 to Actiuni.ActionCount - 1 do
          if CompareText(Actiuni.Actions[K].Name, aName)=0 then begin
            Result := Actiuni.Actions[K].OnExecute;
            Break;
          end;
      end;

    procedure AddSubMeniu(aItem: TMenuItem; aBar: tdxBarSubItem; aCat: Integer);
    var J: Integer;
        TmpBar: tdxBarSubItem;
        Tmp   : tdxBarButton;
        altCat: Integer;

        procedure AddItem(T: Integer);
         begin
            if aItem.Items[T].Count>=1 then begin
               TmpBar := TdxBarSubItem(lBarManager.CreateItem(TdxBarSubItem, lBarManager, nil));
               with TmpBar do begin
                 Category := aCat;
                 MergeKind := mkNone;
                 if aMenu.Items[I].Action <> nil then
                   Action := aItem.Items[T].Action                 
                 else begin
                   Caption := aItem.Items[T].Caption;
                   ShortCut  := aItem.Items[T].ShortCut;
                   Tag       := aItem.Items[T].Tag;
                 end;
                 altCat := CreateCategory(Trim(aItem.Items[T].Caption));
                 AddSubMeniu(aItem.Items[T], TmpBar, altCat);
               end;
               aBar.ItemLinks.Add.Item := TmpBar;
            end
            else begin
               Tmp := tdxBarButton.Create(aForm);
               with Tmp do begin
                  Category := aCat;
                  MergeKind := mkNone;
                  if aItem.Items[T].Action <> nil then begin
                     Action := aItem.Items[T].Action;
                     //Caption := aItem.Items[T].Caption;
                     //Tag     := aItem.Items[T].Tag;
                     //ShortCut  := aItem.Items[T].ShortCut;
                     if aItem.Items[T].GroupIndex <> 0 then begin
                       ButtonStyle := bsChecked;
                       AllowAllUp := True;
                       //Down := TAction(aItem.Items[T].Action).Checked;
                     end;
                  end
                  else begin
                     Caption := aItem.Items[T].Caption;
                     Tag     := aItem.Items[T].Tag;
                     ShortCut  := aItem.Items[T].ShortCut;
                     if Trim(aItem.Items[T].Hint) <> '' then
                        OnClick := GetMethodByName(aItem.Items[T].Hint);
                     if not Assigned(OnClick) then
                        OnClick := TNotifyEvent(GetMethod(@aItem.Items[T].OnClick));
                  end;
                  Enabled   := (aItem.Items[T].Enabled)and (pos('<?>', Caption) = 0);
               end;
               aBar.ItemLinks.Add.Item := Tmp;
            end
         end;
     begin
       Cool := False;
       for J := 0 to aItem.Count - 1 do begin
         if Cool then Cool:=False
       else
         if not (Trim(aItem.Items[J].Caption)= '-') then
            AddItem(J)
         else
           if J < aItem.Count - 2 then begin
              AddItem(J+1);
              aBar.ItemLinks.Items[aBar.ItemLinks.Count-1].BeginGroup := True;
              Cool := True;
           end;
       end;
     end;
begin
  lBarManager := nil;
  InitBar(aForm, TdxBarManager(lBarManager));
  lBarManager.Images := aMenu.Images;
//  lBarManager.BeginUpdate();
  for I := 0 to aMenu.Items.Count - 1 do begin
    aBar := TdxBarSubItem(lBarManager.CreateItem(TdxBarSubItem, lBarManager, nil));
    with aBar do begin
       Category := 0;
       MergeKind := mkNone;
       if aMenu.Items[I].Action <> nil then
          Action := aMenu.Items[I].Action
       else begin
         Caption := aMenu.Items[I].Caption;
         ShortCut  := aMenu.Items[I].ShortCut;
         ImageIndex := aMenu.Items[I].ImageIndex;
       end;
       aCat := CreateCategory(aMenu.Items[I].Caption);
       AddSubMeniu(aMenu.Items[i], aBar, aCat);
    end;
    lBarManager.Bars[0].ItemLinks.Add.Item := aBar;
  end;

//  llBarManager.EndUpdate(True);
  for I := 0 to lBarManager.Bars.Count - 1 do
      with lBarManager.Bars[I] do
        if Control <> nil then
          with Control do
          begin
            UpdateControlState;
            RepaintBar;
          end;
end;

type
  TCrackActionList = class(TActionList);

procedure TmainForm.CreateMeniuSkin;
var
  I: Integer;
  skinAction : TAction;
  lSkinList  : TStringList;
begin
  lSkinList := TStringList.Create;
  try
    if GetSkinNameList(lSkinList) then begin

      skinAction := TAction.Create(Actiuni);
      skinAction.Name       := 'Cmd_Afisare_Normala';
      skinAction.Caption    := 'Afisare normala';
      skinAction.Category   := '09. Mod Afisare';
      skinAction.GroupIndex := 77;
      skinAction.AutoCheck  := True;
      skinAction.OnExecute  := DoChangeSkin;
      TCrackActionList(Actiuni).AddAction(skinAction);

      for I := 0 to lSkinList.Count-1 do begin
        skinAction := TAction.Create(Actiuni);
        skinAction.Name       := 'Cmd_Afisare_'+lSkinList[I];
        skinAction.Caption    := lSkinList[I];
        skinAction.OnExecute  := DoChangeSkin;
        skinAction.Category   := 'Mod Afisare';
        skinAction.AutoCheck  := True;
        skinAction.GroupIndex := 77;
        TCrackActionList(Actiuni).AddAction(skinAction);
      end;

    end;
  finally
    lSkinList.Free;
  end;
end;

procedure TmainForm.InitBar(aForm : TForm; var aBarManager : TdxBarManager);
begin
  FreeAndNil(aBarManager);

  aBarManager := TdxBarManager.Create(aForm);
  aBarManager.Name := Format('BarManager_%p', [Pointer(aForm)]);
  aBarManager.AlwaysMerge := True;
  aBarManager.Style := bmsFlat;
  with aBarManager.Bars.Add do begin
    AllowClose := False;
    AllowQuickCustomizing := False;
    BorderStyle := bbsNone;
    IsMainMenu := True;
    DockingStyle := dsTop;
    NotDocking := [dsNone, dsLeft, dsRight, dsBottom];
    Caption := 'Meniu';
    Visible := True;
  end;
end;

type
  TCrackdxBar = class(TdxBar);

procedure TmainForm.MainMenuAfterOpen(Sender: TObject);
begin
  if HamMenu.Visible then
    ConvertMenuToHamburgMenu(MainMenu.BarByCaption(MainMenu.BarCaption), HamMenu);
end;

procedure TmainForm.MainMenuMerge(Sender, ChildBarManager: TdxBarManager;
  AddItems: Boolean);
begin
  if (ChildBarManager <> nil) and (ChildBarManager.MainMenuBar <> nil) then
    TCrackdxBar(ChildBarManager.MainMenuBar).SetVisibility(True);
end;

type TCrackfmFR4Explorer = class(TfmFR4Explorer);

procedure TmainForm.RepOnPreview(Sender: TObject);
begin
  TCrackfmFR4Explorer(FRrapExplorer.Explorer).DoBeforeReportPreview(Sender);
end;

procedure TmainForm.CmdGenCodBaraExecute(Sender: TObject);
begin
  ShowGetCoduriBara
end;

procedure TmainForm.TabReport(AReport: TfrxReport);
var
  lForm : TForm;
begin
  lForm := AReport.PreviewForm;
  if Assigned(lForm) then
    SetNewForm(lForm);
end;

procedure TmainForm.Cmd_AlopDispozitieExecute(Sender: TObject);
begin
  if EnterSingleUser(TfrmAlopDispozitie) then
    TfrmAlopDispozitie(GetNewForm(TfrmAlopDispozitie)).ReadDispozitie();
end;

procedure TmainForm.Cmd_AlopListaDispozitiiExecute(Sender: TObject);
begin
  GetNewForm(TfrmAlopDispVizualizare, 'Vizualizare dispozitii');
end;

procedure DisableProcessWindowsGhosting;
var
  DisableProcessWindowsGhostingImp : procedure;
begin
  try
    @DisableProcessWindowsGhostingImp := GetProcAddress(GetModuleHandle('user32.dll'),'DisableProcessWindowsGhosting');
    if @DisableProcessWindowsGhostingImp <> nil then
      DisableProcessWindowsGhostingImp;
  except
    on E:Exception do begin
      OutputDebugString(PChar('DisableProcessWindowsGhostingerror:'+E.Message));
    end;
  end;
end;


procedure TmainForm.Cmd_ContracteListaExecute(Sender: TObject);
begin
  GetNewForm(TfrmContracte, 'Vizualizare Contracte');
end;


procedure TmainForm.Cmd_ContracteExecute(Sender: TObject);
begin
  if EnterSingleUser(TfrmContractEdit) then
    TfrmContractEdit(GetNewForm(TfrmContractEdit)).ReadContractEcran();
end;

procedure TmainForm.Cmd_TranspunerePlanExecute(Sender: TObject);
begin
  GetNewForm(TfrmContaTranspunereAn);
end;

procedure TmainForm.Cmd_CompensariExecute(Sender: TObject);
begin
  TfrmCompensari(GetNewForm(TfrmCompensari, 'Compensari Furnizori/ Clienti')).RefreshIncasari();
end;

procedure TmainForm.Cmd_BazaSchimbareExecute(Sender: TObject);
begin
  szPrevUserName := '';
  szPrevPassword := '';
  OpenDataModule(frmData, True);
  RefreshMenuBar;
end;

procedure TmainForm.RefreshMenuBar;
begin
  RapCommands.ActiveDB := False;
  RapCommands.Active := False;
  FRRapCommands.ActiveDB := False;
  FRRapCommands.Active := False;
  Comenzi.ActiveDB := False;
  Comenzi.Active   := False;
  MainMenu.Active  := False;
  MainMenu.Active  := True;
  Comenzi.ActiveDB := False;
  RapCommands.ActiveDB := False;
  FRRapCommands.ActiveDB := False;
end;

procedure TmainForm.ApplicationEventsException(Sender: TObject;
  E: Exception);
begin
  if not (E is EAbort) then
  begin
    if DBProcExists('spLogEroare') then
    begin
      DBExecSQLFmt('exec spLogEroare %d, %d, %s, %s, %s, %s', [
        IdUtilizator,
        IdLogin,
        ValueToStr(ExeVersion),
        ValueToStr(svnRevision),
        ValueToStr(E.ClassName),
        ValueToStr(E.Message)]);
    end;
    Application.ShowException(E);
  end;
end;

procedure TmainForm.Cmd_GrupeProiectExecute(Sender: TObject);
begin
  GetNewForm(TfmGrupaProiecte, 'Intretinere Grupe Proiecte');
end;

procedure TmainForm.Cmd_RegistruNouExecute(Sender: TObject);
begin
  if EnterSingleUser(TFrmRegistruEx) then
    GetNewForm(TFrmRegistruEx);
end;

procedure TmainForm.Cmd_PreluareExtraseExecute(Sender: TObject);
begin
  GetNewForm(TfrmPreluareExtrase);
end;

initialization
  DisableProcessWindowsGhosting;
end.

