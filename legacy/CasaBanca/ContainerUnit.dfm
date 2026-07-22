object frmCasaContainer: TfrmCasaContainer
  Tag = -1
  Left = 0
  Top = 56
  Caption = 'frmCasaContainer'
  ClientHeight = 579
  ClientWidth = 1008
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  PixelsPerInch = 96
  TextHeight = 13
  object Label1: TLabel
    Left = 0
    Top = 219
    Width = 84
    Height = 13
    Caption = 'Clasa Functionala'
  end
  object Label2: TLabel
    Left = 0
    Top = 452
    Width = 82
    Height = 13
    Caption = 'Clasa Economica'
  end
  object Label3: TLabel
    Left = 384
    Top = 684
    Width = 60
    Height = 13
    Caption = 'Organigrama'
  end
  object Label4: TLabel
    Left = 4
    Top = 686
    Width = 45
    Height = 13
    Caption = 'Persoana'
  end
  object TreeFunctional: TdxDBTreeList
    Left = 8
    Top = 8
    Width = 521
    Height = 217
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'ID_BG_PLAN_FUNCTIONAL'
    ParentField = 'ID_PARINTE'
    BorderStyle = bsNone
    TabOrder = 0
    OnDblClick = TreeCheltituitoriDblClick
    OnKeyDown = TreeCheltituitoriKeyDown
    OnMouseUp = TreeCheltituitoriMouseUp
    DataSource = frmData.DTBGPlanFunctional
    Images = CheckList
    LookAndFeel = lfUltraFlat
    OptionsBehavior = [etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSearch, etoAutoSort, etoDblClick, etoDragExpand, etoDragScroll, etoTabThrough]
    OptionsDB = [etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
    OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoHotTrack, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
    PreviewLines = 1
    ShowPreviewGrid = False
    StateImages = CheckList
    TreeLineColor = clGrayText
    TreeLineStyle = tlSolid
    OnGetSelectedIndex = TreeCheltituitoriGetSelectedIndex
    object TreeFunctionalID_BUGET_PLAN_FUNCTIONAL: TdxDBTreeListMaskColumn
      Visible = False
      Width = 909
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_BG_PLAN_FUNCTIONAL'
    end
    object TreeFunctionalCOD_BUGET: TdxDBTreeListMaskColumn
      Tag = -1
      Caption = 'Cod Buget'
      Sorted = csUp
      Width = 187
      BandIndex = 0
      RowIndex = 0
      FieldName = 'COD_FUNCTIONAL'
    end
    object TreeFunctionalDENUMIRE: TdxDBTreeListMaskColumn
      Caption = 'Denumire'
      Width = 332
      BandIndex = 0
      RowIndex = 0
      FieldName = 'DENUMIRE'
    end
    object TreeFunctionalDESCRIERE: TdxDBTreeListMaskColumn
      Caption = 'Descriere'
      Visible = False
      Width = 270
      BandIndex = 0
      RowIndex = 0
      FieldName = 'DESCRIERE'
    end
    object TreeFunctionalNUMAR_RAND: TdxDBTreeListMaskColumn
      Visible = False
      Width = 1641
      BandIndex = 0
      RowIndex = 0
      FieldName = 'NUMAR_RAND'
    end
    object TreeFunctionalID_PARINTE: TdxDBTreeListMaskColumn
      Visible = False
      Width = 376
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_PARINTE'
    end
    object TreeFunctionalPLANIFICAT1: TdxDBTreeListMaskColumn
      Visible = False
      Width = 670
      BandIndex = 0
      RowIndex = 0
      FieldName = 'PLANIFICAT1'
    end
    object TreeFunctionalPLANIFICAT2: TdxDBTreeListMaskColumn
      Visible = False
      Width = 670
      BandIndex = 0
      RowIndex = 0
      FieldName = 'PLANIFICAT2'
    end
    object TreeFunctionalPLANIFICAT3: TdxDBTreeListMaskColumn
      Visible = False
      Width = 670
      BandIndex = 0
      RowIndex = 0
      FieldName = 'PLANIFICAT3'
    end
    object TreeFunctionalPLANIFICAT4: TdxDBTreeListMaskColumn
      Visible = False
      Width = 670
      BandIndex = 0
      RowIndex = 0
      FieldName = 'PLANIFICAT4'
    end
    object TreeFunctionalPLANIFICAT: TdxDBTreeListMaskColumn
      Visible = False
      Width = 670
      BandIndex = 0
      RowIndex = 0
      FieldName = 'PLANIFICAT'
    end
    object TreeFunctionalCLASA: TdxDBTreeListMaskColumn
      Caption = 'Clasa'
      Visible = False
      Width = 126
      BandIndex = 0
      RowIndex = 0
      FieldName = 'CLASA'
    end
  end
  object TreeEconomic: TdxDBTreeList
    Left = 0
    Top = 233
    Width = 521
    Height = 217
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'ID_BG_PLAN_ECONOMIC'
    ParentField = 'ID_PARINTE'
    BorderStyle = bsNone
    TabOrder = 1
    OnDblClick = TreeCheltituitoriDblClick
    OnKeyDown = TreeCheltituitoriKeyDown
    OnMouseUp = TreeCheltituitoriMouseUp
    Images = CheckList
    LookAndFeel = lfUltraFlat
    OptionsBehavior = [etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSearch, etoAutoSort, etoDblClick, etoDragExpand, etoDragScroll, etoTabThrough]
    OptionsDB = [etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
    OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoHotTrack, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
    StateImages = CheckList
    TreeLineColor = clGrayText
    TreeLineStyle = tlSolid
    OnGetSelectedIndex = TreeCheltituitoriGetSelectedIndex
    object TreeEconomicID_BUGET_PLAN_ECONOMIC: TdxDBTreeListMaskColumn
      Visible = False
      Width = 761
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_BG_PLAN_ECONOMIC'
    end
    object TreeEconomicCOD_BUGET: TdxDBTreeListMaskColumn
      Tag = -1
      Caption = 'Cod Buget'
      Sorted = csUp
      Width = 188
      BandIndex = 0
      RowIndex = 0
      FieldName = 'COD_ECONOMIC'
    end
    object TreeEconomicDENUMIRE: TdxDBTreeListMaskColumn
      Caption = 'Denumire'
      Width = 331
      BandIndex = 0
      RowIndex = 0
      FieldName = 'DENUMIRE'
    end
    object TreeEconomicDESCRIERE: TdxDBTreeListMaskColumn
      Caption = 'Descriere'
      Visible = False
      Width = 306
      BandIndex = 0
      RowIndex = 0
      FieldName = 'DESCRIERE'
    end
    object TreeEconomicNUMAR_RAND: TdxDBTreeListMaskColumn
      Visible = False
      Width = 1473
      BandIndex = 0
      RowIndex = 0
      FieldName = 'NUMAR_RAND'
    end
    object TreeEconomicID_PARINTE: TdxDBTreeListMaskColumn
      Visible = False
      Width = 340
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_PARINTE'
    end
    object TreeEconomicPLANIFICAT1: TdxDBTreeListMaskColumn
      Visible = False
      Width = 601
      BandIndex = 0
      RowIndex = 0
      FieldName = 'PLANIFICAT1'
    end
    object TreeEconomicPLANIFICAT2: TdxDBTreeListMaskColumn
      Visible = False
      Width = 601
      BandIndex = 0
      RowIndex = 0
      FieldName = 'PLANIFICAT2'
    end
    object TreeEconomicPLANIFICAT3: TdxDBTreeListMaskColumn
      Visible = False
      Width = 601
      BandIndex = 0
      RowIndex = 0
      FieldName = 'PLANIFICAT3'
    end
    object TreeEconomicPLANIFICAT4: TdxDBTreeListMaskColumn
      Visible = False
      Width = 601
      BandIndex = 0
      RowIndex = 0
      FieldName = 'PLANIFICAT4'
    end
    object TreeEconomicPLANIFICAT: TdxDBTreeListMaskColumn
      Visible = False
      Width = 601
      BandIndex = 0
      RowIndex = 0
      FieldName = 'PLANIFICAT'
    end
    object TreeEconomicCLASA: TdxDBTreeListMaskColumn
      Caption = 'Clasa'
      Visible = False
      Width = 97
      BandIndex = 0
      RowIndex = 0
      FieldName = 'CLASA'
    end
  end
  object TreeOrganigrama: TdxDBTreeList
    Left = 381
    Top = 465
    Width = 169
    Height = 217
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'ID_ORGANIGRAMA'
    ParentField = 'ID_PARINTE'
    BorderStyle = bsNone
    TabOrder = 2
    OnDblClick = TreeCheltituitoriDblClick
    OnKeyDown = TreeCheltituitoriKeyDown
    OnMouseUp = TreeCheltituitoriMouseUp
    DataSource = frmData.DTCasaFunctie
    Images = CheckList
    LookAndFeel = lfUltraFlat
    OptionsBehavior = [etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSearch, etoAutoSort, etoDblClick, etoDragExpand, etoDragScroll, etoTabThrough]
    OptionsDB = [etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
    OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoHotTrack, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
    ShowGrid = True
    StateImages = CheckList
    TreeLineColor = clGrayText
    OnGetSelectedIndex = TreeCheltituitoriGetSelectedIndex
    object TreeOrganigramaID_ORGANIGRAMA: TdxDBTreeListMaskColumn
      Visible = False
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_ORGANIGRAMA'
    end
    object TreeOrganigramaDENUMIRE: TdxDBTreeListMaskColumn
      Caption = 'Denumire'
      Sorted = csUp
      BandIndex = 0
      RowIndex = 0
      FieldName = 'DENUMIRE'
    end
    object TreeOrganigramaID_PARINTE: TdxDBTreeListMaskColumn
      Visible = False
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_PARINTE'
    end
  end
  object TreeCheltituitori: TdxDBTreeList
    Left = 1
    Top = 468
    Width = 377
    Height = 217
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'ID_REPARTITORI'
    ParentField = 'ID_PARINTE'
    BorderStyle = bsNone
    TabOrder = 3
    OnDblClick = TreeCheltituitoriDblClick
    OnKeyDown = TreeCheltituitoriKeyDown
    OnMouseUp = TreeCheltituitoriMouseUp
    DataSource = frmData.DTCasaSalariati
    Images = CheckList
    LookAndFeel = lfUltraFlat
    OptionsBehavior = [etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSearch, etoAutoSort, etoDblClick, etoDragExpand, etoDragScroll, etoTabThrough]
    OptionsDB = [etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
    OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoHideFocusRect, etoHotTrack, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
    ShowGrid = True
    StateImages = CheckList
    TreeLineColor = clGrayText
    TreeLineStyle = tlSolid
    OnGetSelectedIndex = TreeCheltituitoriGetSelectedIndex
    OnCustomDrawCell = TreeCheltituitoriCustomDrawCell
    object TreeCheltituitoriID_REPARTITORI: TdxDBTreeListMaskColumn
      Visible = False
      Width = 44
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_REPARTITORI'
    end
    object TreeCheltituitoriCODSECTIE: TdxDBTreeListMaskColumn
      Caption = 'Cod Intern'
      Width = 81
      BandIndex = 0
      RowIndex = 0
      FieldName = 'CODSECTIE'
    end
    object TreeCheltituitoriNUME: TdxDBTreeListMaskColumn
      Caption = 'Nume'
      Width = 210
      BandIndex = 0
      RowIndex = 0
      FieldName = 'NUME'
    end
    object TreeCheltituitoriADRESA: TdxDBTreeListMaskColumn
      Caption = 'Adresa'
      Width = 84
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ADRESA'
    end
    object TreeCheltituitoriCONT: TdxDBTreeListMaskColumn
      Visible = False
      Width = 40
      BandIndex = 0
      RowIndex = 0
      FieldName = 'CONT'
    end
    object TreeCheltituitoriCONT_CEC: TdxDBTreeListMaskColumn
      Visible = False
      Width = 59
      BandIndex = 0
      RowIndex = 0
      FieldName = 'CONT_CEC'
    end
    object TreeCheltituitoriBANCA: TdxDBTreeListMaskColumn
      Visible = False
      Width = 59
      BandIndex = 0
      RowIndex = 0
      FieldName = 'BANCA'
    end
    object TreeCheltituitoriCODCLASM: TdxDBTreeListMaskColumn
      Visible = False
      Width = 31
      BandIndex = 0
      RowIndex = 0
      FieldName = 'CODCLASM'
    end
    object TreeCheltituitoriCOD_FISCAL: TdxDBTreeListMaskColumn
      Visible = False
      Width = 114
      BandIndex = 0
      RowIndex = 0
      FieldName = 'COD_FISCAL'
    end
    object TreeCheltituitoriREG_COMERT: TdxDBTreeListMaskColumn
      Visible = False
      Width = 33
      BandIndex = 0
      RowIndex = 0
      FieldName = 'REG_COMERT'
    end
    object TreeCheltituitoriID_TARI: TdxDBTreeListMaskColumn
      Visible = False
      Width = 30
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_TARI'
    end
    object TreeCheltituitoriID_JUDETE: TdxDBTreeListMaskColumn
      Visible = False
      Width = 31
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_JUDETE'
    end
    object TreeCheltituitoriTELEFON: TdxDBTreeListMaskColumn
      Visible = False
      Width = 59
      BandIndex = 0
      RowIndex = 0
      FieldName = 'TELEFON'
    end
    object TreeCheltituitoriFAX: TdxDBTreeListMaskColumn
      Visible = False
      Width = 29
      BandIndex = 0
      RowIndex = 0
      FieldName = 'FAX'
    end
    object TreeCheltituitoriEMAIL: TdxDBTreeListMaskColumn
      Visible = False
      Width = 29
      BandIndex = 0
      RowIndex = 0
      FieldName = 'EMAIL'
    end
    object TreeCheltituitoriCOMERCIANT: TdxDBTreeListMaskColumn
      Visible = False
      Width = 59
      BandIndex = 0
      RowIndex = 0
      FieldName = 'COMERCIANT'
    end
    object TreeCheltituitoriGESTINT: TdxDBTreeListCheckColumn
      Visible = False
      Width = 47
      BandIndex = 0
      RowIndex = 0
      FieldName = 'GESTINT'
      ValueChecked = 'True'
      ValueUnchecked = 'False'
    end
    object TreeCheltituitoriCOTA_DISCOUNT: TdxDBTreeListMaskColumn
      Visible = False
      Width = 59
      BandIndex = 0
      RowIndex = 0
      FieldName = 'COTA_DISCOUNT'
    end
    object TreeCheltituitoriCOTA_ADAOS: TdxDBTreeListMaskColumn
      Visible = False
      Width = 59
      BandIndex = 0
      RowIndex = 0
      FieldName = 'COTA_ADAOS'
    end
    object TreeCheltituitoriDATA_STOC_INI: TdxDBTreeListDateColumn
      Visible = False
      Width = 51
      BandIndex = 0
      RowIndex = 0
      FieldName = 'DATA_STOC_INI'
    end
    object TreeCheltituitoriDATA_SOLD_INI: TdxDBTreeListDateColumn
      Visible = False
      Width = 51
      BandIndex = 0
      RowIndex = 0
      FieldName = 'DATA_SOLD_INI'
    end
    object TreeCheltituitoriSOLD_INITIAL: TdxDBTreeListMaskColumn
      Visible = False
      Width = 59
      BandIndex = 0
      RowIndex = 0
      FieldName = 'SOLD_INITIAL'
    end
    object TreeCheltituitoriSNM: TdxDBTreeListMaskColumn
      Visible = False
      Width = 29
      BandIndex = 0
      RowIndex = 0
      FieldName = 'SNM'
    end
    object TreeCheltituitoriCONT_CRSP: TdxDBTreeListMaskColumn
      Visible = False
      Width = 59
      BandIndex = 0
      RowIndex = 0
      FieldName = 'CONT_CRSP'
    end
    object TreeCheltituitoriPREFERAT: TdxDBTreeListCheckColumn
      Visible = False
      Width = 47
      BandIndex = 0
      RowIndex = 0
      FieldName = 'PREFERAT'
      ValueChecked = 'True'
      ValueUnchecked = 'False'
    end
    object TreeCheltituitoriID_GEST_TIP_GEST: TdxDBTreeListMaskColumn
      Visible = False
      Width = 51
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_GEST_TIP_GEST'
    end
    object TreeCheltituitoriTIP_GESTIUNE: TdxDBTreeListMaskColumn
      Visible = False
      Width = 40
      BandIndex = 0
      RowIndex = 0
      FieldName = 'TIP_GESTIUNE'
    end
    object TreeCheltituitoriGRUP_LJ: TdxDBTreeListCheckColumn
      Visible = False
      Width = 47
      BandIndex = 0
      RowIndex = 0
      FieldName = 'GRUP_LJ'
      ValueChecked = 'True'
      ValueUnchecked = 'False'
    end
    object TreeCheltituitoriID_UTILIZATORI: TdxDBTreeListMaskColumn
      Visible = False
      Width = 43
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_UTILIZATORI'
    end
    object TreeCheltituitoriID_PARINTE: TdxDBTreeListMaskColumn
      Visible = False
      Width = 33
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_PARINTE'
    end
    object TreeCheltituitoriINORG: TdxDBTreeListImageColumn
      Alignment = taLeftJustify
      Caption = 'Apartinere'
      MinWidth = 16
      Sorted = csUp
      Width = 100
      BandIndex = 0
      RowIndex = 0
      FieldName = 'IN_ORG'
    end
  end
  object TreeRepartitori: TdxDBTreeList
    Left = 576
    Top = 224
    Width = 433
    Height = 305
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'ID_REPARTITORI'
    ParentField = 'ID_PARINTE'
    TabOrder = 4
    Visible = False
    OnDblClick = TreeCheltituitoriDblClick
    OnKeyDown = TreeCheltituitoriKeyDown
    DataSource = frmData.DTRepartitori
    LookAndFeel = lfUltraFlat
    OptionsBehavior = [etoAutoSearch, etoAutoSort, etoDblClick]
    OptionsDB = [etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
    OptionsView = [etoAutoWidth, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
    TreeLineColor = clGrayText
    TreeLineStyle = tlSolid
    OnGetSelectedIndex = TreeCheltituitoriGetSelectedIndex
    object TreeRepartitoriCONT: TdxDBTreeListMaskColumn
      Alignment = taLeftJustify
      Caption = 'Cont'
      DisableEditor = True
      HeaderAlignment = taCenter
      Visible = False
      Width = 57
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_REPARTITORI'
    end
    object TreeRepartitoriNUME: TdxDBTreeListMaskColumn
      Tag = -1
      Caption = 'Denumire'
      DisableEditor = True
      HeaderAlignment = taCenter
      Sorted = csUp
      Width = 263
      BandIndex = 0
      RowIndex = 0
      FieldName = 'NUME'
    end
    object TreeRepartitoriCODSECTIE: TdxDBTreeListMaskColumn
      Caption = 'Cod'
      DisableEditor = True
      HeaderAlignment = taCenter
      Width = 28
      BandIndex = 0
      RowIndex = 0
      FieldName = 'CODSECTIE'
    end
    object TreeRepartitoriADRESA: TdxDBTreeListMaskColumn
      Caption = 'Adresa'
      DisableEditor = True
      HeaderAlignment = taCenter
      Width = 49
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ADRESA'
    end
    object TreeRepartitoriGESTINT: TdxDBTreeListCheckColumn
      Caption = 'Interna'
      DisableEditor = True
      HeaderAlignment = taCenter
      Width = 29
      BandIndex = 0
      RowIndex = 0
      FieldName = 'GESTINT'
      ValueChecked = 'True'
      ValueUnchecked = 'False'
    end
    object TreeRepartitoriTIPGEST: TdxDBTreeListMaskColumn
      Caption = 'Tip Gestiune'
      DisableEditor = True
      HeaderAlignment = taCenter
      Visible = False
      Width = 57
      BandIndex = 0
      RowIndex = 0
      FieldName = 'TIP_GESTIUNE'
    end
    object TreeRepartitoriID_REPARTITORI: TdxDBTreeListMaskColumn
      Alignment = taLeftJustify
      Caption = 'Identificator'
      Width = 62
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_REPARTITORI'
    end
  end
  object TreeTipDoc: TdxDBTreeList
    Left = 88
    Top = 376
    Width = 465
    Height = 185
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'TIP_DOC'
    ParentField = 'TIP_DOC'
    TabOrder = 5
    Visible = False
    OnDblClick = TreeCheltituitoriDblClick
    OnKeyDown = TreeCheltituitoriKeyDown
    DataSource = frmData.DTTipDoc
    LookAndFeel = lfUltraFlat
    OptionsBehavior = [etoAutoSearch, etoAutoSort, etoDblClick]
    OptionsDB = [etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
    OptionsView = [etoAutoWidth, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
    ShowRoot = False
    TreeLineColor = clGrayText
    OnGetSelectedIndex = TreeCheltituitoriGetSelectedIndex
    object TreeTipDocTIP_DOC: TdxDBTreeListMaskColumn
      Tag = -1
      Caption = 'Tip Doc'
      DisableEditor = True
      Sorted = csUp
      Width = 73
      BandIndex = 0
      RowIndex = 0
      FieldName = 'TIP_DOC'
    end
    object TreeTipDocDENUMIRE: TdxDBTreeListMaskColumn
      Caption = 'Denumire'
      Width = 378
      BandIndex = 0
      RowIndex = 0
      FieldName = 'DENUMIRE'
    end
    object TreeTipDocID_TIPURI_DOC: TdxDBTreeListMaskColumn
      DisableEditor = True
      Visible = False
      Width = 80
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_TIPURI_DOC'
    end
  end
  object TreePlan: TdxDBTreeList
    Left = 304
    Top = 188
    Width = 399
    Height = 205
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'CONT'
    ParentField = 'PARINTE'
    TabOrder = 6
    Visible = False
    OnDblClick = TreeCheltituitoriDblClick
    OnKeyDown = TreeCheltituitoriKeyDown
    DataSource = frmData.DTPlanCont
    LookAndFeel = lfUltraFlat
    OptionsBehavior = [etoAnsiSort, etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSearch, etoAutoSort, etoDblClick, etoDragExpand, etoDragScroll, etoEditing, etoEnterShowEditor, etoImmediateEditor, etoTabThrough]
    OptionsCustomize = [etoBandMoving, etoBandSizing, etoColumnMoving, etoColumnSizing, etoExtCustomizing, etoKeepColumnWidth]
    OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
    TreeLineColor = clGrayText
    OnGetSelectedIndex = TreeCheltituitoriGetSelectedIndex
    object TreePlanCONT: TdxDBTreeListMaskColumn
      Caption = 'Cont'
      DisableEditor = True
      HeaderAlignment = taCenter
      Sorted = csUp
      Width = 195
      BandIndex = 0
      RowIndex = 0
      FieldName = 'CONT'
    end
    object TreePlanROMANA: TdxDBTreeListMaskColumn
      Caption = 'Plan Cont'
      DisableEditor = True
      HeaderAlignment = taCenter
      Width = 143
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ROMANA'
    end
    object TreePlanSID: TdxDBTreeListMaskColumn
      DisableEditor = True
      Width = 31
      BandIndex = 0
      RowIndex = 0
      FieldName = 'SID'
    end
    object TreePlanSIC: TdxDBTreeListMaskColumn
      DisableEditor = True
      Width = 28
      BandIndex = 0
      RowIndex = 0
      FieldName = 'SIC'
    end
  end
  object TreeOrdonantari: TdxDBTreeList
    Left = 0
    Top = 0
    Width = 1089
    Height = 145
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'ID_UNIC_MODUL'
    ParentField = 'ID_UNIC_MODUL'
    TabOrder = 7
    Visible = False
    OnDblClick = TreeCheltituitoriDblClick
    OnMouseUp = TreeCheltituitoriMouseUp
    DataSource = frmData.DTOrdCasa
    Images = CheckList
    LookAndFeel = lfUltraFlat
    OptionsBehavior = [etoAutoSearch, etoAutoSort, etoDblClick]
    OptionsDB = [etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
    OptionsView = [etoAutoWidth, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
    ShowRoot = False
    TreeLineColor = clGrayText
    OnGetSelectedIndex = TreeCheltituitoriGetSelectedIndex
    object TreeOrdonantariID_UNIC_MODUL: TdxDBTreeListMaskColumn
      Visible = False
      Width = 21
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_UNIC_MODUL'
    end
    object TreeOrdonantariID_ALOP_ORDONANTARE: TdxDBTreeListMaskColumn
      Visible = False
      Width = 21
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_ALOP_ORDONANTARE'
    end
    object TreeOrdonantariNR_NOTA: TdxDBTreeListMaskColumn
      Visible = False
      Width = 21
      BandIndex = 0
      RowIndex = 0
      FieldName = 'NR_NOTA'
    end
    object TreeOrdonantariId_angajament: TdxDBTreeListMaskColumn
      Visible = False
      Width = 21
      BandIndex = 0
      RowIndex = 0
      FieldName = 'Id_angajament'
    end
    object TreeOrdonantariColumn19: TdxDBTreeListColumn
      Width = 119
      BandIndex = 0
      RowIndex = 0
      FieldName = 'REPARTITOR'
    end
    object TreeOrdonantariCont: TdxDBTreeListMaskColumn
      Width = 58
      BandIndex = 0
      RowIndex = 0
      FieldName = 'Cont'
    end
    object TreeOrdonantariCont_Coresp: TdxDBTreeListMaskColumn
      Width = 55
      BandIndex = 0
      RowIndex = 0
      FieldName = 'Cont_Coresp'
    end
    object TreeOrdonantarinr_conex: TdxDBTreeListMaskColumn
      Visible = False
      Width = 21
      BandIndex = 0
      RowIndex = 0
      FieldName = 'nr_conex'
    end
    object TreeOrdonantaricod_functional: TdxDBTreeListMaskColumn
      Width = 61
      BandIndex = 0
      RowIndex = 0
      FieldName = 'cod_functional'
    end
    object TreeOrdonantaricod_economic: TdxDBTreeListMaskColumn
      Width = 67
      BandIndex = 0
      RowIndex = 0
      FieldName = 'cod_economic'
    end
    object TreeOrdonantariValoare: TdxDBTreeListCurrencyColumn
      Width = 57
      BandIndex = 0
      RowIndex = 0
      FieldName = 'Valoare'
      Nullable = False
    end
    object TreeOrdonantariREST_PLATA: TdxDBTreeListCurrencyColumn
      Width = 68
      BandIndex = 0
      RowIndex = 0
      FieldName = 'REST_PLATA'
      Nullable = False
    end
    object TreeOrdonantariData_Scadenta: TdxDBTreeListDateColumn
      Width = 63
      BandIndex = 0
      RowIndex = 0
      FieldName = 'Data_Scadenta'
    end
    object TreeOrdonantariDocument: TdxDBTreeListMaskColumn
      Width = 87
      BandIndex = 0
      RowIndex = 0
      FieldName = 'Document'
    end
    object TreeOrdonantariData_Document: TdxDBTreeListDateColumn
      Width = 57
      BandIndex = 0
      RowIndex = 0
      FieldName = 'Data_Document'
    end
    object TreeOrdonantariDocument_Detaliu: TdxDBTreeListMaskColumn
      Sorted = csUp
      Width = 21
      BandIndex = 0
      RowIndex = 0
      FieldName = 'Document_Detaliu'
    end
    object TreeOrdonantariPlata: TdxDBTreeListCheckColumn
      Width = 17
      BandIndex = 0
      RowIndex = 0
      FieldName = 'Plata'
      ValueChecked = 'True'
      ValueUnchecked = 'False'
    end
    object TreeOrdonantariData: TdxDBTreeListDateColumn
      Width = 21
      BandIndex = 0
      RowIndex = 0
      FieldName = 'Data'
    end
    object TreeOrdonantariExplicatie: TdxDBTreeListMaskColumn
      Width = 22
      BandIndex = 0
      RowIndex = 0
      FieldName = 'Explicatie'
    end
  end
  object TreeListOrd: TdxDBTreeList
    Left = 600
    Top = 296
    Width = 465
    Height = 185
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'ID_ALOP_ORDONANTARE'
    ParentField = 'ID_ALOP_ORDONANTARE'
    TabOrder = 8
    Visible = False
    OnDblClick = TreeListOrdDblClick
    DataSource = frmData.DTOrdonantari
    LookAndFeel = lfUltraFlat
    OptionsBehavior = [etoAutoSearch, etoAutoSort, etoDblClick]
    OptionsDB = [etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
    OptionsView = [etoAutoWidth, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
    ShowRoot = False
    TreeLineColor = clGrayText
    object AtsDBTreeListMaskColumn1: TdxDBTreeListMaskColumn
      Tag = -1
      Caption = 'Tip Doc'
      DisableEditor = True
      Sorted = csUp
      Visible = False
      Width = 73
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_ALOP_ORDONANTARE'
    end
    object AtsDBTreeListMaskColumn2: TdxDBTreeListMaskColumn
      Caption = 'Ordonantare'
      Width = 378
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ORDONANTARE'
    end
  end
  object CheckList: TImageList
    Left = 248
    Top = 72
    Bitmap = {
      494C010106000900040010001000FFFFFFFFFF10FFFFFFFFFFFFFFFF424D3600
      0000000000003600000028000000400000002000000001002000000000000020
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00000000000000000000000000FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00000000000000000000000000FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00000000000000000000000000FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF0000000000000000000000000084848400C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF0000000000000000000000000084848400C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF0000000000000000000000000084848400C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF0000000000000000000000000084848400C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF0000000000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C60084848400C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00000000000000000000000000FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600848484008484840084848400C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF000000000000000000000000000000000000000000FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C6008484840084848400848484008484840084848400C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF000000000000000000FFFFFF00000000000000000000000000FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C6008484840084848400C6C6C600848484008484840084848400C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF0000000000FFFFFF00FFFFFF00FFFFFF0000000000000000000000
      0000FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C60084848400C6C6C600C6C6C600C6C6C60084848400848484008484
      8400C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00000000000000
      0000FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600848484008484
      8400C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF000000
      0000FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C6008484
      8400C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600FFFFFF00000000000000000000000000848484000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000C6C6C600FFFFFF00000000000000000000000000848484000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000C6C6C600FFFFFF00000000000000000000000000848484000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000C6C6C600FFFFFF00000000000000000000000000848484000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000C6C6C600FFFFFF00000000000000000000000000848484008484
      8400848484008484840084848400848484008484840084848400848484008484
      84008484840084848400FFFFFF00000000000000000000000000848484008484
      8400848484008484840084848400848484008484840084848400848484008484
      84008484840084848400FFFFFF00000000000000000000000000848484008484
      8400848484008484840084848400848484008484840084848400848484008484
      84008484840084848400FFFFFF00000000000000000000000000848484008484
      8400848484008484840084848400848484008484840084848400848484008484
      84008484840084848400FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000424D3E000000000000003E000000
      2800000040000000200000000100010000000000000100000000000000000000
      000000000000000000000000FFFFFF00FFFFFFFF00000000FFFFFFFF00000000
      C003C00300000000DFFBDFFB00000000DFFBD9FB00000000DFFBD0FB00000000
      DFFBD07B00000000DFFBD63B00000000DFFBDF1B00000000DFFBDF8B00000000
      DFFBDFCB00000000DFFBDFEB00000000DFFBDFFB00000000C003C00300000000
      FFFFFFFF00000000FFFFFFFF00000000FFFFFFFFFFFFFFFFC001C001C001C001
      C001C001C001C001C001C001C001C001C001C001C001C001C001C001C001C001
      C001C001C001C001C001C001C001C001C001C001C001C001C001C001C001C001
      C001C001C001C001C001C001C001C001C001C001C001C001C001C001C001C001
      FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF00000000000000000000000000000000
      000000000000}
  end
end
