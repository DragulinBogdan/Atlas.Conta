object mainForm: TmainForm
  Left = 208
  Top = 45
  Caption = 'Contabilitate'
  ClientHeight = 518
  ClientWidth = 826
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  OnClose = FormClose
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  OnShow = FormShow
  PixelsPerInch = 96
  TextHeight = 13
  object HamMenu: TdxNavBar
    Left = 0
    Top = 0
    Width = 297
    Height = 493
    Align = alLeft
    Visible = False
    ActiveGroupIndex = -1
    TabOrder = 0
    LookAndFeel.NativeStyle = True
    View = 21
    OptionsBehavior.Common.AllowChildGroups = True
  end
  object pnlStatusBar: TPanel
    Left = 0
    Top = 493
    Width = 826
    Height = 25
    Align = alBottom
    TabOrder = 1
    object MainStatusBar: TdxStatusBar
      Left = 1
      Top = 4
      Width = 824
      Height = 20
      Panels = <
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Text = 'Utilizator'
          Width = 150
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Text = 'Baza'
          Width = 300
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Text = 'EcranCurent'
          Width = 100
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          PanelStyle.Color = clMoneyGreen
          PanelStyle.Font.Charset = DEFAULT_CHARSET
          PanelStyle.Font.Color = clWindowText
          PanelStyle.Font.Height = -11
          PanelStyle.Font.Name = 'MS Sans Serif'
          PanelStyle.Font.Style = [fsBold]
          PanelStyle.ParentFont = False
          Text = 'HintInfo'
        end>
      PaintStyle = stpsUseLookAndFeel
      LookAndFeel.Kind = lfOffice11
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
    end
  end
  object pnContent: TPanel
    Left = 297
    Top = 0
    Width = 529
    Height = 493
    Align = alClient
    Anchors = []
    BevelOuter = bvNone
    TabOrder = 2
    ExplicitLeft = 303
    ExplicitTop = -2
    object tabTop: TcxTabControl
      Left = 0
      Top = 0
      Width = 529
      Height = 33
      Align = alTop
      TabOrder = 0
      Properties.CloseButtonMode = cbmActiveAndHoverTabs
      Properties.CustomButtons.Buttons = <>
      Properties.Options = [pcoAlwaysShowGoDialogButton, pcoCloseButton, pcoGoDialog, pcoGradient, pcoGradientClientArea, pcoRedrawOnResize]
      Properties.Style = 9
      ClientRectBottom = 33
      ClientRectRight = 529
      ClientRectTop = 0
    end
  end
  object MainMenu: TATSMainBar
    AutoDockColor = False
    Font.Charset = EASTEUROPE_CHARSET
    Font.Color = clWindowText
    Font.Height = -12
    Font.Name = 'MS Sans Serif'
    Font.Style = []
    Categories.Strings = (
      'Default')
    Categories.ItemsVisibles = (
      2)
    Categories.Visibles = (
      True)
    DockColor = clBtnFace
    MenusShowRecentItemsFirst = False
    NotDocking = [dsNone, dsLeft, dsRight, dsBottom]
    PopupMenuLinks = <>
    RegistryPath = '\Software\ATS\Contabilitate\'
    Style = bmsUseLookAndFeel
    UseSystemFont = False
    OnMerge = MainMenuMerge
    Active = False
    BarName = 'mnuMainMenu'
    BarCaption = 'Meniu Principal'
    Commands = Comenzi
    OnAfterOpen = MainMenuAfterOpen
    Left = 144
    Top = 176
    PixelsPerInch = 96
  end
  object Actiuni: TActionList
    Left = 144
    Top = 112
    object Cmd_CulegereNote: TAction
      Category = 'Conta'
      Caption = 'Culegere Note Contabile'
      OnExecute = Cmd_CulegereNoteExecute
    end
    object Cmd_IntretinerePlanConturi: TAction
      Category = 'Conta'
      Caption = 'Cmd_IntretinerePlanConturi'
      OnExecute = Cmd_IntretinerePlanConturiExecute
    end
    object Cmd_IntretinerePlanBugete: TAction
      Category = 'Intretinere'
      Caption = 'Cmd_IntretinerePlanBugete'
      OnExecute = Cmd_IntretinerePlanBugeteExecute
    end
    object Cmd_Balanta: TAction
      Category = 'Conta'
      Caption = 'Cmd_Balanta'
      OnExecute = Cmd_BalantaExecute
    end
    object Cmd_GeneratorRapoarte: TAction
      Category = 'Rap'
      Caption = 'Cmd_GeneratorRapoarte'
    end
    object Cmd_IstoricNote: TAction
      Category = 'Conta'
      Caption = 'Cmd_IstoricNote'
      OnExecute = Cmd_IstoricNoteExecute
    end
    object Cmd_RefreshReports: TAction
      Category = 'Rap'
      Caption = 'Reafisare Rapoarte'
      OnExecute = Cmd_RefreshReportsExecute
    end
    object Cmd_ModificareMeniu: TAction
      Category = 'Intretinere'
      Caption = 'Modificare Meniu'
      OnExecute = Cmd_ModificareMeniuExecute
    end
    object Cmd_Execute_Report: TAction
      Category = 'Rap'
      Caption = 'Executie Raport'
    end
    object Cmd_IntretinNaturCheltuieli: TAction
      Category = 'Intretinere'
      Caption = 'Intretinere Nmcl Natura Cheltuieli'
    end
    object Cmd_IntretinModPlata: TAction
      Category = 'Intretinere'
      Caption = 'Intretinere Moduri Plata'
    end
    object Cmd_IntretinValute: TAction
      Category = 'Intretinere'
      Caption = 'Intretinere Nmcl Valute'
      OnExecute = Cmd_IntretinValuteExecute
    end
    object Cmd_IntretinBanci: TAction
      Category = 'Intretinere'
      Caption = 'Intretinere Banci'
    end
    object Cmd_IntretinereRepartitori: TAction
      Category = 'Intretinere'
      Caption = 'Intretinere Repartitori'
      OnExecute = Cmd_IntretinereRepartitoriExecute
    end
    object Cmd_IntretinereJurnale: TAction
      Category = 'Intretinere'
      Caption = 'Intretinere Jurnale'
      OnExecute = Cmd_IntretinereJurnaleExecute
    end
    object Cmd_IntretinereUtilizatori: TAction
      Category = 'Intretinere'
      Caption = 'Intretinere Utilizatori'
      OnExecute = Cmd_IntretinereUtilizatoriExecute
    end
    object Cmd_Angajamente: TAction
      Category = 'Alop'
      Caption = 'Introducere Angajamente'
      OnExecute = Cmd_AngajamenteExecute
    end
    object Cmd_TranzactiiCV: TAction
      Category = 'TCV'
      Caption = 'Tranzactii Cantitativ Valorice'
      OnExecute = Cmd_TranzactiiCVExecute
    end
    object Cmd_Documente: TAction
      Category = 'Administrare'
      Caption = 'Intretinere Documente'
      OnExecute = Cmd_DocumenteExecute
    end
    object Cmd_DocumenteValidare: TAction
      Category = 'TCV'
      Caption = 'Documente Validare'
      OnExecute = Cmd_DocumenteValidareExecute
    end
    object Cmd_IntretinereFunctii: TAction
      Category = 'Intretinere'
      Caption = 'Intretinere Functii'
      OnExecute = Cmd_IntretinereFunctiiExecute
    end
    object Cmd_IntretinereTipuriMateriale: TAction
      Category = 'Intretinere'
      Caption = 'Intretinere Tipuri Materiale'
      OnExecute = Cmd_IntretinereTipuriMaterialeExecute
    end
    object Cmd_WizardDocum: TAction
      Category = 'Administrare'
      Caption = 'Wizard Documente'
      OnExecute = Cmd_WizardDocumExecute
    end
    object Cmd_Decontari: TAction
      Category = 'CasaBanca'
      Caption = 'Decontari Contabile'
      OnExecute = Cmd_DecontariExecute
    end
    object Cmd_StocuriUnitate: TAction
      Category = 'TCV'
      Caption = 'Stocuri Unitate'
      OnExecute = Cmd_StocuriUnitateExecute
    end
    object Cmd_ImportNote: TAction
      Category = 'Conta'
      Caption = 'Import note contabile'
      OnExecute = Cmd_ImportNoteExecute
    end
    object Cmd_RegistruDocumente: TAction
      Category = 'TCV'
      Caption = 'Registru Documente'
      OnExecute = Cmd_RegistruDocumenteExecute
    end
    object Cmd_PlanificareBuget: TAction
      Category = 'Buget'
      Caption = 'Planificare Buget'
      OnExecute = Cmd_PlanificareBugetExecute
    end
    object Cmd_Registru: TAction
      Category = 'CasaBanca'
      Caption = 'Cmd_Registru'
      OnExecute = Cmd_RegistruExecute
    end
    object Cmd_FundamentareBuget: TAction
      Category = 'Buget'
      Caption = 'Fundamentare Buget'
      OnExecute = Cmd_FundamentareBugetExecute
    end
    object Cmd_ListaCasa: TAction
      Category = 'CasaBanca'
      Caption = 'Cmd_ListaCasa'
    end
    object Cmd_IntretinTipProiect: TAction
      Category = 'CasaBanca'
      Caption = 'Intretinere tip Proiect'
    end
    object Cmd_IntretinerePlanProiecte: TAction
      Category = 'CasaBanca'
      Caption = 'Intretinere Plan Proiecte'
    end
    object Cmd_IntretinereCasa: TAction
      Category = 'CasaBanca'
      Caption = 'Intretinere Case - Banci - Trezorerie'
      OnExecute = Cmd_IntretinereCasaExecute
    end
    object Cmd_IntretinereCulori: TAction
      Category = 'CasaBanca'
      Caption = 'Cmd_IntretinereCulori'
      OnExecute = Cmd_IntretinereCuloriExecute
    end
    object Cmd_IntretinerTipDoc: TAction
      Category = 'CasaBanca'
      Caption = 'Intretinere Tip Doc'
      OnExecute = Cmd_IntretinerTipDocExecute
    end
    object Cmd_CorespTipDoc: TAction
      Category = 'CasaBanca'
      Caption = 'Intertinere Corespondenta Casa Gestiune'
      OnExecute = Cmd_CorespTipDocExecute
    end
    object Cmd_IntretinereTipCheltuieli: TAction
      Category = 'CasaBanca'
      Caption = 'Intretinere Tip Cheltuieli/ Venituri'
      OnExecute = Cmd_IntretinereTipCheltuieliExecute
    end
    object Cmd_DocumenteValidate: TAction
      Category = 'TCV'
      Caption = 'Documente Validate'
      OnExecute = Cmd_DocumenteValidateExecute
    end
    object Cmd_IntretinereAnexeExecutieBugetara: TAction
      Category = 'Intretinere'
      Caption = 'Cmd_IntretinereAnexeExecutieBugetara'
      OnExecute = Cmd_IntretinereAnexeExecutieBugetaraExecute
    end
    object Cmd_RefreshDataSet: TAction
      Category = 'Administrare'
      Caption = 'Refresh DataSet'
      OnExecute = Cmd_RefreshDataSetExecute
    end
    object Cmd_Cascade: TAction
      Category = 'Fereastra'
      Caption = 'Cascadare Ferestre'
    end
    object Cmd_TileHorizontal: TAction
      Category = 'Fereastra'
      Caption = 'Incadrare orizontala'
    end
    object Cmd_UrmarireExecutie: TAction
      Category = 'Alop'
      Caption = 'Executie Bugetara'
      OnExecute = Cmd_UrmarireExecutieExecute
    end
    object Cmd_AngajamentGlobal: TAction
      Category = 'Alop'
      Caption = 'Angajamente Globale'
      OnExecute = Cmd_AngajamentGlobalExecute
    end
    object Cmd_IntretinereOrganigramaCasa: TAction
      Category = 'CasaBanca'
      Caption = 'Intretinere Organigrama Casa/Banca'
      OnExecute = Cmd_IntretinereOrganigramaCasaExecute
    end
    object Cmd_Deconturi: TAction
      Category = 'CasaBanca'
      Caption = 'Situatie Deconturi'
      OnExecute = Cmd_DeconturiExecute
    end
    object Cmd_PreluareBuget: TAction
      Category = 'CasaBanca'
      Caption = 'Preluare Buget'
      OnExecute = Cmd_PreluareBugetExecute
    end
    object Cmd_DefinireDecont: TAction
      Category = 'CasaBanca'
      Caption = 'Definire Decont'
      OnExecute = Cmd_DefinireDecontExecute
    end
    object Cmd_ValidareDecont: TAction
      Category = 'CasaBanca'
      Caption = 'Validare Deconturi'
      OnExecute = Cmd_ValidareDecontExecute
    end
    object Cmd_GenerareOP: TAction
      Category = 'PS1'
      Caption = 'Generare OP'
      OnExecute = Cmd_GenerareOPExecute
    end
    object Cmd_ModificareOP: TAction
      Category = 'PS1'
      Caption = 'Modificare OP'
      OnExecute = Cmd_ModificareOPExecute
    end
    object Cmd_IntretinereBugetDirectii: TAction
      Category = 'Buget'
      Caption = 'Intretinere Directii Buget'
      OnExecute = Cmd_IntretinereBugetDirectiiExecute
    end
    object Cmd_IntretinereBugetProiecte: TAction
      Category = 'Buget'
      Caption = 'Intretinere Proiecte Buget'
      OnExecute = Cmd_IntretinereBugetProiecteExecute
    end
    object Cmd_BugetAprobat: TAction
      Category = 'Buget'
      Caption = 'Introducere Buget Aprobat'
      OnExecute = Cmd_BugetAprobatExecute
    end
    object Cmd_IntretinereAntet: TAction
      Category = 'Intretinere'
      Caption = 'Intretinere Antet'
      OnExecute = Cmd_IntretinereAntetExecute
    end
    object Cmd_SchimbareParola: TAction
      Category = 'Intretinere'
      Caption = 'Schimbare Parola'
      OnExecute = Cmd_SchimbareParolaExecute
    end
    object Cmd_IntretinereBilant: TAction
      Category = 'Intretinere'
      Caption = 'Intretinere Formule Bilant'
    end
    object Cmd_IntretinereAnexeBilant: TAction
      Category = 'Intretinere'
      Caption = 'Intretinere Anexe Bilant'
      OnExecute = Cmd_IntretinereAnexeBilantExecute
    end
    object Cmd_IntretinereDelegati: TAction
      Category = 'Intretinere'
      Caption = 'Cmd_IntretinereDelegati'
      OnExecute = Cmd_IntretinereDelegatiExecute
    end
    object Cmd_IntretinereMijloaceTransport: TAction
      Category = 'Intretinere'
      Caption = 'Cmd_IntretinereMijloaceTransport'
      OnExecute = Cmd_IntretinereMijloaceTransportExecute
    end
    object Cmd_IntretinereTipuriMIjloaceTransport: TAction
      Category = 'Intretinere'
      Caption = 'Cmd_IntretinereTipuriMIjloaceTransport'
      OnExecute = Cmd_IntretinereTipuriMIjloaceTransportExecute
    end
    object Cmd_IntretinereNomenclator: TAction
      Category = 'Intretinere'
      Caption = 'Intretinere Nomenclator'
      OnExecute = Cmd_IntretinereNomenclatorExecute
    end
    object Cmd_ContareBugetara: TAction
      Category = 'Alop'
      Caption = 'Contare Executie Bugetara'
      OnExecute = Cmd_ContareBugetaraExecute
    end
    object Cmd_IntretinereTipProdus: TAction
      Category = 'Intretinere'
      Caption = 'Intretinere Tipuri Produse'
      OnExecute = Cmd_IntretinereTipProdusExecute
    end
    object Cmd_IntretinereTipMateriale: TAction
      Category = 'Intretinere'
      Caption = 'Intretinere Tipuri Materiale'
      OnExecute = Cmd_IntretinereTipMaterialeExecute
    end
    object Cmd_IntretinereTipStoc: TAction
      Category = 'Intretinere'
      Caption = 'Intretinere Tipuri Stocuri'
      OnExecute = Cmd_IntretinereTipStocExecute
    end
    object Cmd_IntretinereTipStoc_TipProdus: TAction
      Category = 'Intretinere'
      Caption = 'Intretinere influente produse pentru stoc'
      OnExecute = Cmd_IntretinereTipStoc_TipProdusExecute
    end
    object Cmd_About: TAction
      Category = 'Intretinere'
      Caption = 'About'
      OnExecute = Cmd_AboutExecute
    end
    object Cmd_InchiderePerioadeFiscale: TAction
      Category = 'Intretinere'
      Caption = 'Inchidere perioade fiscale'
      OnExecute = Cmd_InchiderePerioadeFiscaleExecute
    end
    object Cmd_IntretinereTipuriRepartitori: TAction
      Category = 'Intretinere'
      Caption = 'Intretinere Tipuri Repartitori'
      OnExecute = Cmd_IntretinereTipuriRepartitoriExecute
    end
    object Cmd_IntretinereOrganizatie: TAction
      Category = 'Intretinere'
      Caption = 'Intretinere Organizatie'
      OnExecute = Cmd_IntretinereOrganizatieExecute
    end
    object Cmd_BGFundamentare: TAction
      Category = 'Buget'
      Caption = 'Buget general fundamentare'
      OnExecute = Cmd_BGFundamentareExecute
    end
    object Cmd_OITipuriProiecte: TAction
      Category = 'OI'
      Caption = 'Intertinere Tipuri Proiecte'
      OnExecute = Cmd_OITipuriProiecteExecute
    end
    object Cmd_OIProiecte: TAction
      Category = 'OI'
      Caption = 'Intretinere Proiecte/Investitii'
      OnExecute = Cmd_OIProiecteExecute
    end
    object Cmd_IntertinereBGPlan: TAction
      Category = 'Buget'
      Caption = 'Intretinere Plan Bugete'
      OnExecute = Cmd_IntertinereBGPlanExecute
    end
    object Cmd_OEIntretinereRepartitori: TAction
      Category = 'OE'
      Caption = 'Interetinere Repartitori'
      OnExecute = Cmd_OEIntretinereRepartitoriExecute
    end
    object Cmd_GEST_RegistruDocumente: TAction
      Category = 'TCV'
      Caption = 'Registru Documente'
      OnExecute = Cmd_GEST_RegistruDocumenteExecute
    end
    object Cmd_ALOPAdaugaAngajament: TAction
      Category = 'Alop'
      Caption = 'Introducere Angajamente'
      OnExecute = Cmd_ALOPAdaugaAngajamentExecute
    end
    object Cmd_BGAprobat: TAction
      Category = 'Buget'
      Caption = 'Introducere Aprobat'
      OnExecute = Cmd_BGAprobatExecute
    end
    object Cmd_AlopListaAngajamente: TAction
      Category = 'Alop'
      Caption = 'Lista Angajamente'
      OnExecute = Cmd_AlopListaAngajamenteExecute
    end
    object Cmd_ALOPLichidare: TAction
      Category = 'Alop'
      Caption = 'Lichidare'
      OnExecute = Cmd_ALOPLichidareExecute
    end
    object Cmd_RapExport: TAction
      Category = 'Rap'
      Caption = 'Export Rapoarte'
    end
    object Cmd_RapImport: TAction
      Category = 'Rap'
      Caption = 'Import Rapoarte'
    end
    object Cmd_NotaSalarii: TAction
      Category = 'Conta'
      Caption = 'Editare Nota Salarii'
    end
    object Cmd_NoteInchidere: TAction
      Category = 'Conta'
      Caption = 'Configurare Note Inchidere'
      OnExecute = Cmd_NoteInchidereExecute
    end
    object Cmd_AlopListaOrdonantare: TAction
      Category = 'Alop'
      Caption = 'Lista Ordonantare'
      OnExecute = Cmd_AlopListaOrdonantareExecute
    end
    object Cmd_AlopIntretinereConturi: TAction
      Category = 'Alop'
      Caption = 'Intretinere Conturi executie'
      OnExecute = Cmd_AlopIntretinereConturiExecute
    end
    object Cmd_GenerarePlata: TAction
      Category = 'PS1'
      Caption = 'Generare Plata'
      OnExecute = Cmd_GenerarePlataExecute
    end
    object Cmd_NoteImperechere: TAction
      Category = 'Conta'
      Caption = 'Stingere obligatii note'
      OnExecute = Cmd_NoteImperechereExecute
    end
    object Cmd_RapImplicit: TAction
      Category = 'Administrare'
      Caption = 'Intretinere Rapoarte Implicite'
      OnExecute = Cmd_RapImplicitExecute
    end
    object Cmd_CulegeAnexeSubunitati: TAction
      Category = 'Buget'
      Caption = 'Introducere Anexe Subunitati'
      OnExecute = Cmd_CulegeAnexeSubunitatiExecute
    end
    object Cmd_CumulareAnexe: TAction
      Category = 'Buget'
      Caption = 'Cumulare Anexe'
      OnExecute = Cmd_CumulareAnexeExecute
    end
    object Cmd_FRGeneratorRapoarte: TAction
      Category = 'Rap'
      Caption = 'Generator Rapoarte'
      OnExecute = Cmd_FRGeneratorRapoarteExecute
    end
    object Cmd_FRExecute_Report: TAction
      Category = 'Rap'
      Caption = 'Executie Raport'
      OnExecute = Cmd_FRExecute_ReportExecute
    end
    object Cmd_PreluareAnexe: TAction
      Category = 'Buget'
      Caption = 'Preluare Anexe'
      OnExecute = Cmd_PreluareAnexeExecute
    end
    object Cmd_FisaBugetara: TAction
      Category = 'Buget'
      Caption = 'Fisa Bugetara'
      OnExecute = Cmd_FisaBugetaraExecute
    end
    object Cmd_TestEroare: TAction
      Category = 'Administrare'
      Caption = 'Test Eroare'
      OnExecute = Cmd_TestEroareExecute
    end
    object Cmd_SituatieTert: TAction
      Category = 'Conta'
      Caption = 'Situatie Tert'
      OnExecute = Cmd_SituatieTertExecute
    end
    object Cmd_IntretinereTipDoc: TAction
      Category = 'Conta'
      Caption = 'Intretinere Tip Documente Conta'
      OnExecute = Cmd_IntretinereTipDocExecute
    end
    object Cmd_PreviewAnexe: TAction
      Category = 'Buget'
      Caption = 'Previzualizare Anexe'
      OnExecute = Cmd_PreviewAnexeExecute
    end
    object CmdGenCodBara: TAction
      Category = 'Administrare'
      Caption = 'Generare Coduri Bara'
      OnExecute = CmdGenCodBaraExecute
    end
    object Cmd_AlopDispozitie: TAction
      Category = 'Alop'
      Caption = 'Dispozitie bugetara'
      OnExecute = Cmd_AlopDispozitieExecute
    end
    object Cmd_AlopListaDispozitii: TAction
      Category = 'Alop'
      Caption = 'Lista dispozitii bugetare'
      OnExecute = Cmd_AlopListaDispozitiiExecute
    end
    object Cmd_ContracteLista: TAction
      Category = 'Contracte'
      Caption = 'Lista contracte'
      OnExecute = Cmd_ContracteListaExecute
    end
    object Cmd_Contracte: TAction
      Category = 'Contracte'
      Caption = 'Adaugare contract'
      OnExecute = Cmd_ContracteExecute
    end
    object Cmd_TranspunerePlan: TAction
      Category = 'Conta'
      Caption = 'Transpunere plan conturi'
      OnExecute = Cmd_TranspunerePlanExecute
    end
    object Cmd_Compensari: TAction
      Category = 'CasaBanca'
      Caption = 'Compensari'
      OnExecute = Cmd_CompensariExecute
    end
    object Cmd_BazaSchimbare: TAction
      Category = 'Administrare'
      Caption = 'Schimbare baza'
      OnExecute = Cmd_BazaSchimbareExecute
    end
    object Cmd_GrupeProiect: TAction
      Category = 'Alop'
      Caption = 'Intretinere Grupe Proiect'
      OnExecute = Cmd_GrupeProiectExecute
    end
    object Cmd_RegistruNou: TAction
      Category = 'CasaBanca'
      Caption = 'Registru Casa / Banca'
      OnExecute = Cmd_RegistruNouExecute
    end
    object Cmd_PreluareExtrase: TAction
      Category = 'CasaBanca'
      Caption = 'Preluare Extrase'
      OnExecute = Cmd_PreluareExtraseExecute
    end
    object Cmd_ContracteManagInvest: TAction
      Category = 'Contracte'
      Caption = 'Contracte ManagInvest'
    end
    object Cmd_AfisareNavigare: TAction
      Category = 'Administrare'
      Caption = 'Afisare Navigare'
      OnExecute = Cmd_AfisareNavigareExecute
    end
    object Cmd_NavigareWeb: TAction
      Category = 'Administrare'
      Caption = 'Navigare Web'
      OnExecute = Cmd_NavigareWebExecute
    end
  end
  object Methods: TMethodProvider
    Active = False
    Controls = <
      item
        Name = 'Actiuni'
        Component = Actiuni
      end>
    Methods = <>
    Events = <>
    Left = 144
    Top = 56
  end
  object Comenzi: TATSZeosAppCommands
    Active = False
    KeyField = 'COD_BARA'
    ParentField = 'COD_PARINTE'
    CaptionField = 'ROMANA'
    HintField = 'MROMANA'
    ShortCutField = 'HOTKEY'
    CmdField = 'PROCNAME'
    TypeField = 'special'
    Methods = Methods
    RootKey = 0
    BeforeOpen = ComenziBeforeOpen
    AfterOpen = ComenziAfterOpen
    OnNewCommand = ComenziNewCommand
    Connection = frmData.dbContabilitate
    ActiveDB = False
    Params = <
      item
        DataType = ftUnknown
        Name = 'ID_UTILIZATORI'
        ParamType = ptUnknown
        Size = -1
      end
      item
        DataType = ftUnknown
        Name = 'IS_ADMIN'
        ParamType = ptUnknown
        Size = -1
      end>
    SQL.Strings = (
      'SELECT A.* '
      'FROM MESSPOP A'
      
        'WHERE (EXISTS(SELECT TOP 1 1 FROM MESSPOP_UTILIZATORI B WHERE A.' +
        'COD_BARA = B.COD_BARA AND B.ID_UTILIZATORI = :ID_UTILIZATORI)) O' +
        'R (:IS_ADMIN >0)'
      'ORDER BY A.COD_PARINTE, A.POZ'
      ' ')
    Left = 48
    Top = 56
  end
  object RapCommands: TATSZeosAppCommands
    Active = False
    KeyField = 'ID_REPORT'
    ParentField = 'ID_PARINTE'
    CaptionField = 'CAPTURA'
    CmdField = 'PROCNAME'
    Methods = Methods
    RootKey = 0
    Connection = frmData.dbContabilitate
    ActiveDB = False
    Params = <>
    SQL.Strings = (
      'EXEC SP_GET_REPORT_LIST')
    Left = 48
    Top = 176
  end
  object ApplicationEvents: TApplicationEvents
    OnException = ApplicationEventsException
    OnHint = ApplicationEventsHint
    OnShowHint = ApplicationEventsShowHint
    OnSettingChange = ApplicationEventsSettingChange
    Left = 240
    Top = 56
  end
  object FRRapCommands: TATSZeosAppCommands
    Active = False
    KeyField = 'ID_REPORT'
    ParentField = 'ID_PARINTE'
    CaptionField = 'CAPTURA'
    CmdField = 'PROCNAME'
    Methods = Methods
    RootKey = 0
    Connection = frmData.dbContabilitate
    ActiveDB = False
    Params = <>
    SQL.Strings = (
      'EXEC SP_GET_REPORT_LIST_FR')
    Left = 48
    Top = 120
  end
end
