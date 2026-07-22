program Contabilitate;

{$R *.dres}

uses
  midaslib,
  PatchRound,
  AtlasStartUpUnit,
  constMessageUnit,
  debugProcessUnit,
  debugClientUnit,
  cxPartialSearchUnit,
  cxGridContentPopupMenuItems,
  SysUtils,
  Classes,
  ActiveX,
  ComObj,
  HookForms,
  infoCustomizeHook,
  Messages,
  Windows,
  Forms,
  Dialogs,
  frxATSComps,
  ZDbcWeb,
  ZeosDBUtile,
  ZeosDBLogUnit,
  atlasStartUp,
  logMessageUnit,
  AtlasSkinUnit,
  dxBar,
  WebNavUtils in 'Web\WebNavUtils.pas',
  IstoricNota in 'Conta\IstoricNota.pas',
  ImportCurs in 'ImportCurs.pas',
  svnInfo in 'svnInfo.pas',
  LogUnit in 'StartUp\LogUnit.pas',
  CommonDBVar in 'CommonDBVar.pas',
  frxPatchReport in 'FrRapoarte\frxPatchReport.pas',
  frxFunction in 'FrRapoarte\frxFunction.pas',
  frxFuncDate in 'FrRapoarte\frxFuncDate.pas',
  frxFuncNum in 'FrRapoarte\frxFuncNum.pas',
  frxFuncSQL in 'FrRapoarte\frxFuncSQL.pas',
  frxFuncStr in 'FrRapoarte\frxFuncStr.pas',
  frxrcAddFunction in 'FrRapoarte\frxrcAddFunction.pas',
  NoteUnitNew in 'Conta\NoteUnitNew.pas' {FrmListaNoteNew},
  ATSZDBUtils in 'ATSZDBUtils.pas',
  CulegereNoteUnit in 'Conta\CulegereNoteUnit.pas' {FmCulegereNote},
  DateUnit in 'DateUnit.pas' {frmData: TDataModule},
  PlanConturiUnit in 'Conta\PlanConturiUnit.pas' {FrmPlanConturi},
  MainUnit in 'MainUnit.pas' {mainForm},
  BalantaUnit in 'Conta\BalantaUnit.pas' {FrmBalanta},
  FisaContUnit in 'Conta\FisaContUnit.pas' {frmFisaCont},
  SaveTemplateUnit in 'SaveTemplateUnit.pas' {frmSaveTemplate},
  ContUnit in 'Conta\ContUnit.pas' {fmContProp},
  ConcurentUsersUnit in 'ConcurentUsersUnit.pas' {FrmConcurentUsers},
  IntretinNmclUnit in 'Nomenclatoare\IntretinNmclUnit.pas' {fmIntretinereNmcl},
  JurnaleUnit in 'Conta\JurnaleUnit.pas' {fmJurnale},
  UtilizatoriUnit in 'UtilizatoriUnit.pas' {fmUtilizatori},
  StockUnit in 'Gestiune\StockUnit.pas' {fmStock},
  SelStockUnit in 'Gestiune\SelStockUnit.pas' {fmSelStock},
  ActulUnit in 'ActulUnit.pas' {fmActual},
  FunctionUnit in 'FunctionUnit.pas' {fmFunctii},
  AlegUtilizatoriUnit in 'AlegUtilizatoriUnit.pas' {fmAlegeUtilizator},
  AdaugareColoanaUnit in 'AdaugareColoanaUnit.pas' {fmAdaugareColoana},
  DecontariUnit in 'Decontari\DecontariUnit.pas' {fmDecontari},
  TCVUnit in 'Gestiune\TCVUnit.pas' {frmTCV},
  cxTCVUnit in 'Gestiune\cxTCVUnit.pas' {frmcxTcv},
  TCVUnitateUnit in 'Gestiune\TCVUnitateUnit.pas' {fmSituatieUnitate},
  FiltruUnit in 'CasaBanca\FiltruUnit.pas' {FrmFiltru},
  IntretinereCaseUnit in 'CasaBanca\IntretinereCaseUnit.pas' {frmIntretinCasa},
  AutoNoteContUnit in 'AutoNoteContUnit.pas' {fmAutoNoteCont},
  AcceptTransferUnit in 'CasaBanca\AcceptTransferUnit.pas' {frmAcceptTransfer},
  ConflictUnit in 'CasaBanca\ConflictUnit.pas' {frmConflict},
  MaintenanceUnit in 'CasaBanca\MaintenanceUnit.pas' {FrmSettings},
  RegistruUnit in 'CasaBanca\RegistruUnit.pas' {FrmRegistru},
  RegistruUnitEx in 'CasaBanca\RegistruUnitEx.pas' {FrmRegistruEx},
  SyncProgressUnit in 'CasaBanca\SyncProgressUnit.pas' {frmProgress},
  CasaUnit in 'CasaBanca\CasaUnit.pas' {frmCasa},
  DefalcareDecontareUnit in 'Decontari\DefalcareDecontareUnit.pas' {fmDefalcareDecontare},
  DocumenteUnit in 'Gestiune\DocumenteUnit.pas' {fmDocumente},
  DetaliiDocumUnit in 'Gestiune\DetaliiDocumUnit.pas' {FmDetaliiDocum},
  fmPlataDocumUnit in 'Gestiune\fmPlataDocumUnit.pas' {fmPlataDocum},
  ChangePassUnit in 'ChangePassUnit.pas' {fmChangePass},
  CommonCasa in 'CasaBanca\CommonCasa.pas',
  VizualizareUnit in 'CasaBanca\VizualizareUnit.pas' {FrmListaCasa},
  ReconciliereDocumUnit in 'ReconciliereDocumUnit.pas' {fmReconciliere},
  ContainerUnit in 'CasaBanca\ContainerUnit.pas' {frmCasaContainer},
  FunctieRepUnit in 'CasaBanca\Proiecte\FunctieRepUnit.pas' {frmFunctieRep},
  OrganigramaUnit in 'CasaBanca\Proiecte\OrganigramaUnit.pas' {FrmOrganigrama},
  fmSelectieDeAngajatUnit in 'Buget\fmSelectieDeAngajatUnit.pas' {fmSelectieDeAngajat},
  fmSelectieCFUnit in 'Buget\fmSelectieCFUnit.pas' {fmSelectieCF},
  fmSelectieCEUnit in 'Buget\fmSelectieCEUnit.pas' {fmSelectieCE},
  fmSelectieRepartitorUnit in 'Buget\fmSelectieRepartitorUnit.pas' {fmSelectieRepartitor},
  UrmarireExecutieUnit in 'Buget\UrmarireExecutieUnit.pas' {fmUrmarireExecutie},
  AngajamenteGlobaleUnit in 'Buget\AngajamenteGlobaleUnit.pas' {fmAngajamenteGlobale},
  NoteEronateUnit in 'Conta\NoteEronateUnit.pas' {fmNoteEronate},
  UnitFormule in 'CasaBanca\Filtru\UnitFormule.pas' {FrmFormula},
  UnitParametrii in 'CasaBanca\Filtru\UnitParametrii.pas' {FrmParametrii},
  ReunireRepUnit in 'ReunireRepUnit.pas' {fmReunireRep},
  SituatieDeconturiUnit in 'CasaBanca\Decontari\SituatieDeconturiUnit.pas' {FrmSituatieDeconturi},
  ErrorUnit in 'CasaBanca\Errors\ErrorUnit.pas' {frmSearchErrors},
  AsocUtilizUnit in 'CasaBanca\AsocUtilizUnit.pas' {frmAsocUtilizatori},
  ReconciliereDecontariUnit in 'ReconciliereDecontariUnit.pas' {FrmReconcilereDecontari},
  PreluareExcelUnit in 'Buget\PreluareExcelUnit.pas' {fmPreluareExcel},
  DetaliiDecontUnit in 'CasaBanca\Decontari\DetaliiDecontUnit.pas' {frmDetaliiDecont},
  ImportCasaUnit in 'CasaBanca\ImportCasaUnit.pas' {frmImportCasa},
  DecontPickUnit in 'CasaBanca\Decontari\DecontPickUnit.pas' {frmDecontPick},
  MessagesUnit in 'CasaBanca\MessagesUnit.pas',
  ChouseReportUnit in 'Raportare\ChouseReportUnit.pas' {FrmChouseReport},
  TransferUnit in 'CasaBanca\TransferUnit.pas' {frmTransfer},
  UnitPerioade in 'CasaBanca\Filtru\UnitPerioade.pas' {frmSelectPeriod},
  GenerareOPUnit in 'GenerareOPUnit.pas' {fmGenerareOP},
  UpdStructure in 'CasaBanca\UpdStructure.pas',
  AlegDecontUnit in 'CasaBanca\Decontari\AlegDecontUnit.pas' {frmSample},
  DirectiiUnit in 'Buget\DirectiiUnit.pas' {fmBugetDirectii},
  ProiectUnit in 'Buget\ProiectUnit.pas' {fmBugetProiect},
  BugetContainer in 'Buget\BugetContainer.pas' {fmBugetContainer},
  AlegAngUnit in 'Buget\AlegAngUnit.pas' {fmBugetAlegAng},
  AntetUnit in 'AntetUnit.pas' {fmIntretinereAntet},
  DelegatiUnit in 'DelegatiUnit.pas' {frmDelegati},
  MijloaceTransportUnit in 'MijloaceTransportUnit.pas' {frmMijTransport},
  fmAnexeBilantUnit in 'Anexe\fmAnexeBilantUnit.pas' {fmAnexeBilant},
  NormalizareNomUnit in 'NormalizareNomUnit.pas' {frmNormalizareNom},
  DetaliiMaterialUnit in 'DetaliiMaterialUnit.pas' {frmSetareDetaliiNomenclator},
  PersistSetariUnit in 'Persistenta\PersistSetariUnit.pas',
  PersistGridSettings in 'Persistenta\PersistGridSettings.pas',
  ListaContracteParinte in 'MInvest\ListaContracteParinte.pas' {frmListaContracteP},
  ModificaContract in 'MInvest\ModificaContract.pas' {frmModificareContract},
  MInvestCommon in 'MInvest\MInvestCommon.pas',
  ImperechereFact in 'CasaBanca\ImperechereFact.pas' {frmSelectieFCT},
  IntretinereTipProduse in 'Nomenclatoare\IntretinereTipProduse.pas' {frmIntretinereTipProd},
  IntretinTipMateriale in 'Nomenclatoare\IntretinTipMateriale.pas' {frmIntretinereTipMat},
  IntretinTipuriStocProdus in 'Nomenclatoare\IntretinTipuriStocProdus.pas' {frmIntertinTipStocProdus},
  IntretinTipStoc in 'Nomenclatoare\IntretinTipStoc.pas' {frmIntretinTipStoc},
  InflProduseUnit in 'Nomenclatoare\InflProduseUnit.pas' {frmInflTipProduse},
  AboutUnit in 'AboutUnit.pas' {frmAbout},
  UnitInchiderePer in 'Conta\UnitInchiderePer.pas' {frmInchiderePerioada},
  UnitSelectCurs in 'UnitSelectCurs.pas' {frmSelectCursValutar},
  UnitStocPerioada in 'Gestiune\UnitStocPerioada.pas' {fmCopyStock},
  TipRepartitori in 'TipRepartitori.pas' {frmTipuriRepartitori},
  BugetCompareUnit in 'Buget\BugetCompareUnit.pas' {frmBugetComparare},
  OI_UnitatiTipuri in 'Organizare_Interna\OI_UnitatiTipuri.pas' {frmOIUnitatiTipuri},
  BxPlanificare in 'Buget\BxPlanificare.pas' {frmBxPlanificare},
  OI_Proiecte in 'Organizare_Interna\OI_Proiecte.pas' {frmOIProiecte},
  BgPlanUnit in 'Buget\BgPlanUnit.pas' {frmBGPlan},
  CTipuriDocumente in 'Conta\CTipuriDocumente.pas' {frmCTipuriDocumente},
  BxPlanContainer in 'Buget\BxPlanContainer.pas' {frmBxPlanContainer},
  OI_Unitati in 'Organizare_Interna\OI_Unitati.pas' {frmOIUnitatiNew},
  Gest_StockProd in 'Gestiune\Gest_StockProd.pas' {frmGestStockProd},
  Gest_ModifyDocum in 'Gestiune\Gest_ModifyDocum.pas' {frmGEST_ModifyDocum},
  AlopAngajamente in 'Buget\AlopAngajamente.pas' {frmAlopAngajamente},
  AlopAngVizualizare in 'Buget\AlopAngVizualizare.pas' {frmAlopAngajamenteVizualizare},
  AlopLichidare in 'Buget\AlopLichidare.pas' {frmAlopLichidare},
  AlopObligatii in 'Buget\AlopObligatii.pas' {frmAlopObligatii},
  OERepartitoriUnit in 'OERepartitoriUnit.pas' {FrmOERepartitori},
  AlopOrdList in 'Buget\AlopOrdList.pas' {frmALOPListaOrd},
  RepartitorContBanca in 'RepartitorContBanca.pas' {frmRepartitorContBanca},
  AlopDisponibil in 'Buget\AlopDisponibil.pas' {frmAlopDisponibil},
  NoteInchidereUnit in 'Conta\NoteInchidereUnit.pas' {frmNoteInchidere},
  BugetImpEXCEL in 'Buget\BugetImpEXCEL.pas' {frmBxImportEXCEL},
  AlopOrdVizualizare in 'Buget\AlopOrdVizualizare.pas' {frmAlopOrdVizualizare},
  AlopIntretinereConturi in 'Buget\AlopIntretinereConturi.pas' {frmAlopIntretinereCont},
  ImperechereUnit in 'ImperechereUnit.pas' {frmImperecheri},
  GenerarePlataUnit in 'GenerarePlataUnit.pas' {frmGenerarePlata},
  AnexeCentralizare in 'Anexe\AnexeCentralizare.pas' {frmAnexeCentralizare},
  DefalcareNoteImperecheate in 'Conta\DefalcareNoteImperecheate.pas' {frmDefalcareNote},
  ImperechereNote in 'Conta\ImperechereNote.pas' {frmImperechereNote},
  AnexeCulegere in 'Anexe\AnexeCulegere.pas' {frmAnexeCulegere},
  CBPozitie in 'CasaBanca\CBPozitie.pas' {frmCBPozitie},
  RapImplicit in 'Raportare\RapImplicit.pas' {frmRapImplicit},
  BugetFisaUnit in 'Buget\BugetFisaUnit.pas' {frmFisaBugetara},
  fmContareBugetaraUnit in 'Buget\fmContareBugetaraUnit.pas' {fmContareBugetara},
  BugetImpAnexe in 'Anexe\BugetImpAnexe.pas' {frmImportAnexeXLS},
  AlopInfo in 'Buget\AlopInfo.pas' {frmALOPInfo},
  TethysAtlasWS in 'Tethys\TethysAtlasWS.pas',
  ImportTethys in 'Tethys\ImportTethys.pas' {frmSelectDM},
  frmProgressUnit in 'frmProgressUnit.pas' {frmProgressRap},
  ATLASSIUtils in 'ATLASSIUtils.pas',
  frAtlasFunctions in 'FrRapoarte\frAtlasFunctions.pas',
  RapInclude in 'RapInclude.pas',
  imperechereTert in 'imperechereTert.pas' {frmImperechereTert},
  VIEScheckVatPort in 'VIEScheckVatPort.pas',
  OI_ProiecteTipuri in 'Organizare_Interna\OI_ProiecteTipuri.pas' {frmOITipuriProiecte},
  AnexeParametrii in 'Anexe\AnexeParametrii.pas' {frmIntretinAnexeParametrii},
  AnexeParametriiCul in 'Anexe\AnexeParametriiCul.pas' {frmAnexeParametriiCul},
  AnexeParametriiLista in 'Anexe\AnexeParametriiLista.pas',
  frmPreviewAnexaUnit in 'frmPreviewAnexaUnit.pas' {frmAnexePreview},
  AnexeParametriiAlocare in 'Anexe\AnexeParametriiAlocare.pas' {frmAsocParam},
  AnexeCopy in 'Anexe\AnexeCopy.pas' {frmAnexaCopy},
  DataMatrixBarcode in 'BarCode\DataMatrixBarcode.pas',
  dmtx in 'BarCode\dmtx.pas',
  configBarCode in 'BarCode\configBarCode.pas' {frmConfigBarCode},
  GenCoduriBara in 'BarCode\GenCoduriBara.pas' {frmGenCoduriBara},
  gestLabels in 'BarCode\gestLabels.pas' {frmGestLabels},
  Gest_ModelNota in 'Gestiune\Gest_ModelNota.pas' {frmGestModelNota},
  NewModificareDocUnit in 'Gestiune\NewModificareDocUnit.pas' {frmModificDocument},
  IntretinTipuriStocCategorii in 'Nomenclatoare\IntretinTipuriStocCategorii.pas' {frmIntertinStocCategorii},
  InflStocCategorie in 'Gestiune\InflStocCategorie.pas' {frmCategoriipeStoc},
  InflProduseFields in 'Gestiune\InflProduseFields.pas' {frmInflProdusFields},
  UnitAddSerii in 'Gestiune\UnitAddSerii.pas' {frmAddSerii},
  SelBugetUnit in 'Buget\SelBugetUnit.pas' {frmSelBuget},
  AlopDispozitie in 'Buget\AlopDispozitie.pas' {frmAlopDispozitie},
  AlopDispVizualizare in 'Buget\AlopDispVizualizare.pas' {frmAlopDispVizualizare},
  OpenProgressUnit in 'Raportare\OpenProgressUnit.pas' {FrmOpenProgress},
  uContracte in 'Contracte\uContracte.pas' {frmContracte},
  uContracteEdit in 'Contracte\uContracteEdit.pas' {frmContractEdit},
  CommonRepository in 'CommonRepository.pas' {frmRepo},
  frmOERepartioriEditUnit in 'frmOERepartioriEditUnit.pas' {frmOERepartitoriEdit},
  fmGrupaProiecteUnit in 'Repartitori\fmGrupaProiecteUnit.pas' {fmGrupaProiecte},
  uMFinante in 'Repartitori\uMFinante.pas',
  frmMFinanteQuestionUnit in 'Repartitori\frmMFinanteQuestionUnit.pas' {frmMFinanteQuestion},
  uVies in 'Repartitori\uVies.pas',
  frmRepartitoriSelectUnit in 'Repartitori\frmRepartitoriSelectUnit.pas' {frmRepartitoriSelect},
  frmMutareRepUnit in 'Repartitori\frmMutareRepUnit.pas' {frmMutareRep},
  frmSelectieContractUnit in 'Contracte\frmSelectieContractUnit.pas' {frmSelectieContract},
  frmSelectieDosarUnit in 'Contracte\frmSelectieDosarUnit.pas' {frmSelectieDosar},
  PatchExcel in 'Common\PatchExcel.pas',
  frmContaSolduriInitialeUnit in 'conta\frmContaSolduriInitialeUnit.pas' {frmContaSolduriInitiale},
  frmContaTranspunereAnUnit in 'conta\frmContaTranspunereAnUnit.pas' {frmContaTranspunereAn},
  InvestOI_Proiecte in 'MInvest\InvestOI_Proiecte.pas' {frmOIProiecteInvest},
  ListaContracte in 'MInvest\ListaContracte.pas' {frmListaContracte},
  DecontariCompensareUnit in 'Decontari\DecontariCompensareUnit.pas' {fmCompensari},
  frmFisaDetaliuUnit in 'FisaDetaliu\frmFisaDetaliuUnit.pas' {frmFisaDetaliu},
  FisaDetaliuUnit in 'FisaDetaliu\FisaDetaliuUnit.pas',
  regionalSettingsUnit in 'Common\regionalSettingsUnit.pas',
  unitProiecteTCV in 'Gestiune\unitProiecteTCV.pas' {fmProiecteTCV},
  importExtraseUnit in 'Extrase\importExtraseUnit.pas',
  frmPreluareExtraseUnit in 'Extrase\frmPreluareExtraseUnit.pas' {frmPreluareExtrase},
  formsUtilsUnit in 'Wrappers\formsUtilsUnit.pas',
  RepartitorAnafUnit in 'Integrare\RepartitorAnafUnit.pas',
  frmIntretinereJudete in 'frmIntretinereJudete.pas' {TfrmIntretinereJudete},
  TFormFacturi in 'Integrare\TFormFacturi.pas',
  Unit2 in 'Unit2.pas' {FormAlop},
  AlopAngDisponibil in 'AlopAngDisponibil.pas',
  Unit11 in 'Unit11.pas';

{$R *.RES}

{x$R TraducereRomana.RES}

procedure InitHelpFile;
var
  lResStream : TResourceStream;
  lHelpFile  : String;
begin
  lHelpFile := ChangeFileExt(ParamStr(0), '.pdf');
  if not FileExists(lHelpFile) then begin
    if FindResource(HInstance, 'HELP', PChar('HLP')) <> 0 then begin
      lResStream := TResourceStream.Create(HInstance, 'HELP', PChar('HLP'));
      try
        lResStream.SaveToFile(lHelpFile);
      finally
        lResStream.Free;
      end;
    end;
  end;
  Application.HelpFile := lHelpFile;
end;

procedure HideAppFromTaskBar;
begin
  Application.MainFormOnTaskBar := False;
  SetWindowLong(Application.Handle, GWL_EXSTYLE, GetWindowLong(Application.Handle, GWL_EXSTYLE) or WS_EX_TOOLWINDOW and not WS_EX_APPWINDOW);
  ShowWindow(Application.Handle, SW_HIDE);
end;

begin
  dxBarPlaySound := False;
  if not AtlasIsNewVersion and CheckAtlasStartUp then begin
    IsMultiThread := True;
    CoInitFlags   := COINIT_MULTITHREADED;
    InitSkinSupport;
    if not StartDebugProcess then begin
      if FindCmdLineSwitch('log') or FindCmdLineSwitch('logare') then
        InitLogDebugMessage;
      Forms.Application.Initialize;
      Forms.Application.Title := 'Contabilitate';
      if bIsParented then HideAppFromTaskBar
      else begin
        infoHookEx.AutoLoadAndSave := True;
        infoHookEx.Enabled := True;
      end;
      ShowLogos;
      try
        Forms.Application.CreateForm(TfrmData, frmData);
  // Application.CreateForm(TFormAlop, FormAlop);
  if bIsCanceling then
        begin
          HideLogos;
          Application.Terminate;
          Exit;
        end;
  if not bIsCanceling then begin
          InitHelpFile;
          Application.CreateForm(TMainForm, MainForm);
        end;
      finally
        HideLogos;
      end;
      Forms.Application.Run;
      FinalizeDebugMessage;
    end;
  end;
end.
