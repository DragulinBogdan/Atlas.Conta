object frmIntretinCasa: TfrmIntretinCasa
  Left = 271
  Top = 140
  Caption = 'Intretinere Case'
  ClientHeight = 659
  ClientWidth = 823
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poDefault
  WindowState = wsMaximized
  OnClose = FormClose
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  PixelsPerInch = 96
  TextHeight = 13
  object splitterV: TcxSplitter
    Left = 551
    Top = 57
    Width = 8
    Height = 479
    HotZoneClassName = 'TcxXPTaskBarStyle'
    AlignSplitter = salRight
    Control = pnRight
  end
  object pnTop: THeadPanel
    Left = 0
    Top = 0
    Width = 823
    Height = 57
    Align = alTop
    Alignment = taLeftJustify
    BevelOuter = bvNone
    Caption = 'pnTop'
    TabOrder = 0
    ShapeType = stRoundRect
    InnerColor = 15444592
    OuterColor = 15788249
    Indent = 10
    Info = 'Intretinere Case - Banci'
    InfoFont.Charset = ANSI_CHARSET
    InfoFont.Color = clWhite
    InfoFont.Height = -13
    InfoFont.Name = 'Arial Black'
    InfoFont.Style = [fsBold]
    Layout = tlCenter
    BorderSize = 5
    ActAsCaption = False
    HideCaption = False
    DesignSize = (
      823
      57)
  end
  object pnClient: TPanel
    Left = 0
    Top = 57
    Width = 551
    Height = 479
    Align = alClient
    BevelOuter = bvNone
    TabOrder = 1
    object gridCasa: TcxGrid
      Left = 0
      Top = 0
      Width = 551
      Height = 279
      Align = alClient
      TabOrder = 0
      LookAndFeel.Kind = lfUltraFlat
      object viewCasa: TcxGridDBTableView
        Navigator.Buttons.CustomButtons = <>
        ScrollbarAnnotations.CustomAnnotations = <>
        DataController.DataSource = DTCasa
        DataController.Filter.MaxValueListCount = 1000
        DataController.Filter.Active = True
        DataController.Filter.AutoDataSetFilter = True
        DataController.KeyFieldNames = 'COD_CB'
        DataController.Summary.DefaultGroupSummaryItems = <>
        DataController.Summary.FooterSummaryItems = <>
        DataController.Summary.SummaryGroups = <>
        Filtering.ColumnPopup.MaxDropDownItemCount = 12
        NewItemRow.Visible = True
        OptionsData.Appending = True
        OptionsSelection.CellSelect = False
        OptionsSelection.HideFocusRectOnExit = False
        OptionsSelection.InvertSelect = False
        OptionsView.ColumnAutoWidth = True
        OptionsView.GroupByBox = False
        OptionsView.GroupFooters = gfVisibleWhenExpanded
        Preview.AutoHeight = False
        Preview.MaxLineCount = 2
        object viewCasaCOD_CB: TcxGridDBColumn
          DataBinding.FieldName = 'COD_CB'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 47
        end
        object viewCasaDENUMIRE: TcxGridDBColumn
          Caption = 'Denumire Casa/Banca'
          DataBinding.FieldName = 'DENUMIRE'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          Width = 81
        end
        object viewCasaCRSP_LEI: TcxGridDBColumn
          Caption = 'Cont Crsp.'
          DataBinding.FieldName = 'CRSP_LEI'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          Width = 39
        end
        object viewCasaID_VALUTA: TcxGridDBColumn
          Caption = 'Den Valuta'
          DataBinding.FieldName = 'ID_VALUTA'
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.DropDownRows = 7
          Properties.Items = <>
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          MinWidth = 16
          Width = 55
        end
        object viewCasaC_O: TcxGridDBColumn
          DataBinding.FieldName = 'C_O'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 26
        end
        object viewCasaSOLDINI_D: TcxGridDBColumn
          DataBinding.FieldName = 'SOLDINI_D'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 61
        end
        object viewCasaSOLDINI_C: TcxGridDBColumn
          DataBinding.FieldName = 'SOLDINI_C'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 60
        end
        object viewCasaDATA_SOLD: TcxGridDBColumn
          DataBinding.FieldName = 'DATA_SOLD'
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.DropDownRows = 7
          Properties.Items = <>
          Properties.ReadOnly = True
          Properties.ShowDescriptions = False
          Visible = False
          HeaderAlignmentHorz = taCenter
          MinWidth = 16
          Width = 97
        end
        object viewCasaCASIER: TcxGridDBColumn
          Caption = 'Casier'
          DataBinding.FieldName = 'CASIER'
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.DropDownRows = 7
          Properties.Items = <>
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          MinWidth = 16
          Width = 69
        end
        object viewCasaDEFALCATOR: TcxGridDBColumn
          Caption = 'Validator'
          DataBinding.FieldName = 'DEFALCATOR'
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.DropDownRows = 7
          Properties.Items = <>
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          MinWidth = 16
          Width = 69
        end
        object viewCasaADMIN: TcxGridDBColumn
          Caption = 'Administrator'
          DataBinding.FieldName = 'ADMIN'
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.DropDownRows = 7
          Properties.Items = <>
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          MinWidth = 16
          Width = 69
        end
        object viewCasaIS_BANCA: TcxGridDBColumn
          Caption = 'Este Banca'
          DataBinding.FieldName = 'IS_BANCA'
          PropertiesClassName = 'TcxCheckBoxProperties'
          Properties.Alignment = taLeftJustify
          Properties.NullStyle = nssUnchecked
          Properties.ReadOnly = True
          Properties.ValueGrayed = ''
          HeaderAlignmentHorz = taCenter
          MinWidth = 16
          Width = 69
        end
        object viewCasaIS_AVANS: TcxGridDBColumn
          Caption = 'Casa Decont'
          DataBinding.FieldName = 'IS_AVANS'
          PropertiesClassName = 'TcxCheckBoxProperties'
          Properties.Alignment = taLeftJustify
          Properties.NullStyle = nssUnchecked
          Properties.ReadOnly = True
          Properties.ValueGrayed = ''
          HeaderAlignmentHorz = taCenter
          MinWidth = 16
          Width = 69
        end
        object viewCasaID_REPARTITORI: TcxGridDBColumn
          Caption = 'Repartitor'
          DataBinding.FieldName = 'ID_REPARTITORI'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          Width = 34
        end
      end
      object nivelCasa: TcxGridLevel
        GridView = viewCasa
      end
    end
    object gridSold: TcxGrid
      Left = 0
      Top = 279
      Width = 551
      Height = 200
      Align = alBottom
      TabOrder = 1
      LookAndFeel.Kind = lfUltraFlat
      object viewSold: TcxGridDBTableView
        PopupMenu = ppSoldMenu
        Navigator.Buttons.CustomButtons = <>
        ScrollbarAnnotations.CustomAnnotations = <>
        DataController.DataSource = DTSoldInitial
        DataController.Filter.MaxValueListCount = 1000
        DataController.Filter.Active = True
        DataController.Filter.AutoDataSetFilter = True
        DataController.KeyFieldNames = 'COD'
        DataController.Summary.DefaultGroupSummaryItems = <>
        DataController.Summary.FooterSummaryItems = <>
        DataController.Summary.SummaryGroups = <>
        Filtering.ColumnPopup.MaxDropDownItemCount = 12
        NewItemRow.Visible = True
        OptionsSelection.HideFocusRectOnExit = False
        OptionsSelection.InvertSelect = False
        OptionsView.ColumnAutoWidth = True
        OptionsView.GroupByBox = False
        OptionsView.GroupFooters = gfVisibleWhenExpanded
        Preview.AutoHeight = False
        Preview.MaxLineCount = 2
        object viewSoldCOD_CB: TcxGridDBColumn
          DataBinding.FieldName = 'COD_CB'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 49
        end
        object viewSoldCOD: TcxGridDBColumn
          DataBinding.FieldName = 'COD'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 29
        end
        object viewSoldCODGEST: TcxGridDBColumn
          DataBinding.FieldName = 'CODGEST'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 58
        end
        object viewSoldDATA: TcxGridDBColumn
          Caption = 'Data Sold'
          DataBinding.FieldName = 'DATA'
          PropertiesClassName = 'TcxDateEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.DateButtons = [btnClear, btnToday]
          Properties.DateOnError = deToday
          Properties.InputKind = ikMask
          HeaderAlignmentHorz = taCenter
          Width = 53
        end
        object viewSoldTIPDOC: TcxGridDBColumn
          DataBinding.FieldName = 'TIPDOC'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 46
        end
        object viewSoldNRDOC: TcxGridDBColumn
          DataBinding.FieldName = 'NRDOC'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 45
        end
        object viewSoldPOZ: TcxGridDBColumn
          DataBinding.FieldName = 'POZ'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 28
        end
        object viewSoldEXPLICATIE: TcxGridDBColumn
          Caption = 'Explicatie'
          DataBinding.FieldName = 'EXPLICATIE'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          Width = 51
        end
        object viewSoldINCASARI: TcxGridDBColumn
          Caption = 'Incasari - Debit'
          DataBinding.FieldName = 'INCASARI'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          HeaderAlignmentHorz = taCenter
          Width = 77
        end
        object viewSoldPLATI: TcxGridDBColumn
          Caption = 'Plati - Credit'
          DataBinding.FieldName = 'PLATI'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          HeaderAlignmentHorz = taCenter
          Width = 62
        end
        object viewSoldSOLD: TcxGridDBColumn
          DataBinding.FieldName = 'SOLD'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 35
        end
        object viewSoldCONT_CSP: TcxGridDBColumn
          DataBinding.FieldName = 'CONT_CSP'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 63
        end
        object viewSoldVAL_CRSP: TcxGridDBColumn
          DataBinding.FieldName = 'VAL_CRSP'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 61
        end
        object viewSoldACHITAT: TcxGridDBColumn
          DataBinding.FieldName = 'ACHITAT'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 52
        end
        object viewSoldDATAEM: TcxGridDBColumn
          DataBinding.FieldName = 'DATAEM'
          PropertiesClassName = 'TcxDateEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.DateButtons = [btnClear, btnToday]
          Properties.DateOnError = deToday
          Properties.InputKind = ikRegExpr
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 51
        end
        object viewSoldC_O: TcxGridDBColumn
          DataBinding.FieldName = 'C_O'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 27
        end
        object viewSoldNR_LIST: TcxGridDBColumn
          DataBinding.FieldName = 'NR_LIST'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 51
        end
        object viewSoldMEXPLIC: TcxGridDBColumn
          DataBinding.FieldName = 'MEXPLIC'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 52
        end
        object viewSoldCURS_SCHIM: TcxGridDBColumn
          DataBinding.FieldName = 'CURS_SCHIM'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 76
        end
        object viewSoldSOLD_INITIAL: TcxGridDBColumn
          DataBinding.FieldName = 'SOLD_INITIAL'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 78
        end
        object viewSoldCOD_ARHIVA: TcxGridDBColumn
          DataBinding.FieldName = 'COD_ARHIVA'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 75
        end
        object viewSoldECL: TcxGridDBColumn
          DataBinding.FieldName = 'ECL'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          HeaderAlignmentHorz = taCenter
          Width = 26
        end
      end
      object nivelSold: TcxGridLevel
        GridView = viewSold
      end
    end
  end
  object pnRight: TPanel
    Left = 559
    Top = 57
    Width = 264
    Height = 479
    Align = alRight
    BevelOuter = bvNone
    TabOrder = 2
    object casaVerticalGrid: TcxDBVerticalGrid
      Left = 0
      Top = 0
      Width = 264
      Height = 273
      BorderStyle = cxcbsNone
      Align = alTop
      LookAndFeel.Kind = lfStandard
      OptionsView.CellTextMaxLineCount = 3
      OptionsView.AutoScaleBands = False
      OptionsView.GridLineColor = clBtnFace
      OptionsView.RowHeaderMinWidth = 30
      OptionsView.RowHeaderWidth = 132
      OptionsView.ValueWidth = 69
      Navigator.Buttons.CustomButtons = <>
      ScrollbarAnnotations.CustomAnnotations = <>
      TabOrder = 0
      DataController.DataSource = DTCasa
      Version = 1
      object casaVerticalGridDENUMIRE: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Den.Casa/Banca'
        Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'DENUMIRE'
        Styles.Header = frmData.cxStyle63
        ID = 0
        ParentID = -1
        Index = 0
        Version = 1
      end
      object casaVerticalGridCategoryRow1: TcxCategoryRow
        Expanded = False
        Properties.Caption = 'Detalii'
        ID = 1
        ParentID = -1
        Index = 1
        Version = 1
      end
      object casaVerticalGridCategoryRow2: TcxCategoryRow
        Expanded = False
        Properties.Caption = 'Validatori'
        ID = 2
        ParentID = -1
        Index = 2
        Version = 1
      end
      object casaVerticalGridIS_BANCA: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Este Banca'
        Properties.EditPropertiesClassName = 'TcxCheckBoxProperties'
        Properties.EditProperties.Alignment = taLeftJustify
        Properties.EditProperties.NullStyle = nssUnchecked
        Properties.EditProperties.ReadOnly = False
        Properties.EditProperties.ValueChecked = 'True'
        Properties.EditProperties.ValueGrayed = ''
        Properties.EditProperties.ValueUnchecked = 'False'
        Properties.DataBinding.FieldName = 'IS_BANCA'
        ID = 3
        ParentID = -1
        Index = 3
        Version = 1
      end
      object casaVerticalGridCASIER: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Casier'
        Properties.EditPropertiesClassName = 'TcxButtonEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.Buttons = <
          item
            Default = True
            Kind = bkEllipsis
          end>
        Properties.EditProperties.OnButtonClick = casaVerticalGridCASIEREditPropertiesButtonClick
        Properties.DataBinding.FieldName = 'CASIER'
        Properties.OnGetDisplayText = casaVerticalGridCASIERPropertiesGetDisplayText
        ID = 4
        ParentID = -1
        Index = 4
        Version = 1
      end
      object casaVerticalGridDEFALCATOR: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Validatori'
        Properties.EditPropertiesClassName = 'TcxButtonEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.Buttons = <
          item
            Default = True
            Kind = bkEllipsis
          end>
        Properties.EditProperties.OnButtonClick = casaVerticalGridCASIEREditPropertiesButtonClick
        Properties.DataBinding.FieldName = 'DEFALCATOR'
        Properties.OnGetDisplayText = casaVerticalGridCASIERPropertiesGetDisplayText
        ID = 5
        ParentID = -1
        Index = 5
        Version = 1
      end
      object casaVerticalGridADMIN: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Administratori'
        Properties.EditPropertiesClassName = 'TcxButtonEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.Buttons = <
          item
            Default = True
            Kind = bkEllipsis
          end>
        Properties.EditProperties.OnButtonClick = casaVerticalGridCASIEREditPropertiesButtonClick
        Properties.DataBinding.FieldName = 'ADMIN'
        Properties.OnGetDisplayText = casaVerticalGridCASIERPropertiesGetDisplayText
        ID = 6
        ParentID = -1
        Index = 6
        Version = 1
      end
      object casaVerticalGridIS_AVANS: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Casa Decont'
        Properties.EditPropertiesClassName = 'TcxCheckBoxProperties'
        Properties.EditProperties.Alignment = taLeftJustify
        Properties.EditProperties.NullStyle = nssUnchecked
        Properties.EditProperties.ReadOnly = False
        Properties.EditProperties.ValueChecked = 'True'
        Properties.EditProperties.ValueGrayed = ''
        Properties.EditProperties.ValueUnchecked = 'False'
        Properties.DataBinding.FieldName = 'IS_AVANS'
        ID = 7
        ParentID = -1
        Index = 7
        Version = 1
      end
      object casaVerticalGridID_REPARTITORI: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Repartitor Avans'
        Properties.EditPropertiesClassName = 'TcxPopupEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.PopupControl = TreeRepartitori
        Properties.EditProperties.PopupSysPanelStyle = True
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'ID_REPARTITORI'
        ID = 8
        ParentID = -1
        Index = 8
        Version = 1
      end
      object casaVerticalGridIS_AVANS1: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Casa Avans'
        Properties.EditPropertiesClassName = 'TcxCheckBoxProperties'
        Properties.EditProperties.Alignment = taLeftJustify
        Properties.EditProperties.NullStyle = nssUnchecked
        Properties.EditProperties.ReadOnly = False
        Properties.EditProperties.ValueChecked = 'True'
        Properties.EditProperties.ValueGrayed = ''
        Properties.EditProperties.ValueUnchecked = 'False'
        Properties.DataBinding.FieldName = 'IS_AVANS'
        ID = 9
        ParentID = -1
        Index = 9
        Version = 1
      end
      object casaVerticalGridIS_TEMPOR: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Casa Decont Temporar'
        Properties.EditPropertiesClassName = 'TcxCheckBoxProperties'
        Properties.EditProperties.Alignment = taLeftJustify
        Properties.EditProperties.NullStyle = nssUnchecked
        Properties.EditProperties.ReadOnly = False
        Properties.EditProperties.ValueChecked = 'True'
        Properties.EditProperties.ValueGrayed = ''
        Properties.EditProperties.ValueUnchecked = 'False'
        Properties.DataBinding.FieldName = 'IS_TEMPOR'
        ID = 10
        ParentID = -1
        Index = 10
        Version = 1
      end
      object casaVerticalGridID_VALUTA: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Valuta'
        Properties.EditPropertiesClassName = 'TcxImageComboBoxProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.DropDownRows = 7
        Properties.EditProperties.Items = <>
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'ID_VALUTA'
        ID = 11
        ParentID = -1
        Index = 11
        Version = 1
      end
      object casaVerticalGridCRSP_LEI: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Cont Coresp.'
        Properties.EditPropertiesClassName = 'TcxButtonEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.Buttons = <
          item
            Default = True
            Kind = bkEllipsis
          end>
        Properties.EditProperties.OnButtonClick = casaVerticalGridCRSP_LEIEditPropertiesButtonClick
        Properties.DataBinding.FieldName = 'CRSP_LEI'
        ID = 12
        ParentID = -1
        Index = 12
        Version = 1
      end
    end
    object soldInspector: TcxDBVerticalGrid
      Left = 0
      Top = 273
      Width = 264
      Height = 206
      BorderStyle = cxcbsNone
      Align = alClient
      LookAndFeel.Kind = lfStandard
      OptionsView.CellTextMaxLineCount = 3
      OptionsView.AutoScaleBands = False
      OptionsView.GridLineColor = clBtnFace
      OptionsView.RowHeaderMinWidth = 30
      OptionsView.RowHeaderWidth = 132
      OptionsView.ValueWidth = 67
      Navigator.Buttons.CustomButtons = <>
      ScrollbarAnnotations.CustomAnnotations = <>
      TabOrder = 1
      DataController.DataSource = DTSoldInitial
      ExplicitTop = 256
      ExplicitHeight = 223
      Version = 1
      object soldInspectorDATA: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Data Sold'
        Properties.EditPropertiesClassName = 'TcxDateEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.DateButtons = [btnClear, btnToday]
        Properties.EditProperties.DateOnError = deToday
        Properties.EditProperties.InputKind = ikMask
        Properties.DataBinding.FieldName = 'DATA'
        ID = 0
        ParentID = -1
        Index = 0
        Version = 1
      end
      object soldInspectorEXPLICATIE: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Explicatie'
        Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'EXPLICATIE'
        ID = 1
        ParentID = -1
        Index = 1
        Version = 1
      end
      object soldInspectorINCASARI: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Incasari'
        Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'INCASARI'
        ID = 2
        ParentID = -1
        Index = 2
        Version = 1
      end
      object soldInspectorPLATI: TcxDBEditorRow
        Expanded = False
        Properties.Caption = 'Plati'
        Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
        Properties.EditProperties.Alignment.Horz = taLeftJustify
        Properties.EditProperties.MaxLength = 0
        Properties.EditProperties.ReadOnly = False
        Properties.DataBinding.FieldName = 'PLATI'
        ID = 3
        ParentID = -1
        Index = 3
        Version = 1
      end
      object soldInspectorCategoryRow1: TcxCategoryRow
        Expanded = False
        Properties.Caption = 'Date Sold'
        ID = 4
        ParentID = -1
        Index = 4
        Version = 1
      end
    end
  end
  object pnBottom: TPanel
    Left = 0
    Top = 583
    Width = 823
    Height = 76
    Align = alBottom
    BevelInner = bvLowered
    BevelOuter = bvNone
    TabOrder = 3
    DesignSize = (
      823
      76)
    object btnOk: TSpeedButton
      Left = 726
      Top = 6
      Width = 75
      Height = 25
      Anchors = [akRight, akBottom]
      Caption = 'OK'
      Flat = True
      Glyph.Data = {
        DE010000424DDE01000000000000760000002800000024000000120000000100
        0400000000006801000000000000000000001000000010000000000000000000
        80000080000000808000800000008000800080800000C0C0C000808080000000
        FF0000FF000000FFFF00FF000000FF00FF00FFFF0000FFFFFF00333333333333
        3333333333333333333333330000333333333333333333333333F33333333333
        00003333344333333333333333388F3333333333000033334224333333333333
        338338F3333333330000333422224333333333333833338F3333333300003342
        222224333333333383333338F3333333000034222A22224333333338F338F333
        8F33333300003222A3A2224333333338F3838F338F33333300003A2A333A2224
        33333338F83338F338F33333000033A33333A222433333338333338F338F3333
        0000333333333A222433333333333338F338F33300003333333333A222433333
        333333338F338F33000033333333333A222433333333333338F338F300003333
        33333333A222433333333333338F338F00003333333333333A22433333333333
        3338F38F000033333333333333A223333333333333338F830000333333333333
        333A333333333333333338330000333333333333333333333333333333333333
        0000}
      NumGlyphs = 2
      OnClick = btnOkClick
    end
  end
  object pnTools: TPanel
    Left = 0
    Top = 536
    Width = 823
    Height = 47
    Align = alBottom
    Color = 16776176
    TabOrder = 4
    object BtnAddDir: TcxButton
      Left = 20
      Top = 8
      Width = 165
      Height = 34
      Caption = 'Adauga Casa - Banca'
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        424D361000000000000036000000280000002000000020000000010020000000
        000000000000C40E0000C40E00000000000000000000C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C7CFD301C6CD
        D008C3CBCC11C2CCCD0FC8D0D400C4CED006C1CDCE0AC2CDCE08C7CFD301C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C7CF
        D301C7CDD009C6C9CB14C6C5C622C2BAB842BEADA76ABCA49A86B9988B9EB98A
        7AC1BA806BDD958267D671A0749542934DB9378F43C93C9148C260A36E9495BC
        A44BC2CECF09C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C6CACC11C4C0C02CC3BAB83EBFB1AD57BDA39A82BB95
        88ACBA8978CCBE8471DAC0806AE8C27A62FAC3785FFEC27F68FFC98C77FFBF8E
        77FF5C7B39FF1A8024FF199739FF1AAD4EFF15B451FF1CB253FF1F9E43FF1C89
        2DF7439550BAB1C7BF22C8D0D400C8D0D400C8D0D400C8D0D400C6C8CB15BAA2
        9A82BE9B919EC19384C1C08773E4C68470F1C8836DF7C57F68FCC57E67FECB80
        68FFC2816CFFD4927EFFD29583FFC57E67FFC27B63FFE2DBD9FFEFF1ECFF5CA4
        61FF1E8D33FF36BC6BFF27C66BFF1CBB60FF1CB05BFF24B462FF2DC66FFF40C3
        76FF2E9D4AFF318C3CD1B2C8BF24C8D0D400C8D0D400C8D0D400C4C2C226C489
        76EDC17E68FFBA7963FFC5826DFFBF7F6BFFBB765FFFCB836CFFC2816DFFCE86
        6FFFCD9181FFF5B5A5FFEDB3A3FFCA856FFFC6816AFFE3E2E0FF7EB882FF238F
        36FF52C683FF3FC879FF26C56BFF82CCA2FFE5E1E3FFB6C1BBFF31C16FFF43C8
        7CFF5ACA8BFF389F52FF3D9249C7B7C6C026C7CFD301C8D0D400C5C5C71DBF8A
        79E3B26C53FFC37458FFC8846FFFC38471FFC27C65FFD78E77FFC98975FFD48D
        78FFD19888FFFABEAFFFF0BFB2FFCE8C77FFCC8772FFC2CFBFFF1F872BFF5BBC
        7EFF5AC98AFF45C87DFF30C670FF8CD3AAFFFEFEFEFFC1CEC7FF3AC174FF48C8
        7FFF59C989FF69C891FF258E36FF778455E4BECCCA14C8D0D400C6C9CB14C190
        82DAB06B53FFC27559FFCA8873FFC98B78FFC7836EFFDF9882FFCF907CFFD994
        80FFD49E8FFFFECABDFFF1DFD9FFD19280FFD28E7CFF679E68FF319645FF6FCA
        97FF59C889FF48C77EFF38C575FF90D2ADFFFEFEFEFFC2CEC7FF42C078FF4CC7
        81FF59C888FF6ACA93FF51B06FFF40833BE8BECCCA14C8D0D400C7CCCF0CC496
        88D2A86750FFB86F56FFC98976FFCE9180FFAF7765FFB77F6FFFBF8676FFDF9C
        8AFFAB7E70FFAD8E85FFAE9289FFD39381FFDD9786FF397E32FF4AA964FF78C6
        9AFFA5C2B2FF9CBBA9FF9ABBA8FFC7D7CEFFFEFEFEFFD9DCDAFF9DBAA9FF9DBB
        AAFF9FBAABFF7EB898FF63C187FF258630ECBDCBC916C8D0D400C7CED106C99D
        92C6AB7768FFB07C6DFFD29687FFE1A292FFCF9484FFD89B8AFFE1A191FFE7A5
        94FFE7A594FFE7A493FFE7A493FFE6A392FFEBA394FF408636FF4FB06CFF7CC9
        9DFFF4F8F6FFF9F9F9FFF9F9F9FFFCFCFCFFFEFEFEFFFDFDFDFFF9F9F9FFF9F9
        F9FFF4F3F3FF8DBBA1FF61C489FF1F872DF4BCCBC818C8D0D400C7CFD202CBA4
        9BBBF0B1A3FFEEB0A1FFEDAFA0FFEDAE9FFFEDAE9FFFECAD9EFFEBAC9EFFECAC
        9DFFEBAC9DFFECAB9CFFEBAB9CFFEAAA9BFFF0AB9DFF448839FF46AE66FF6EC7
        95FFC2DFCEFFCAE1D4FFCBE1D5FFE3ECE7FFFEFEFEFFF3F5F4FFCFE1D7FFCDE2
        D6FFCBE2D5FF87C5A2FF5CC385FF1E862CF3BCCBC818C8D0D400C7CFD301CDA9
        A0B2ECB0A2FFE9ADA0FFEBAFA1FFEDB0A2FFE7AB9DFFE6AA9CFFE7AB9CFFEEB0
        A2FFDDA597FFDAA597FFD5A193FFE4A899FFEAAA9DFF4F8641FF329E4DFF5BC9
        8AFF5AC588FF67C58FFF74C497FFB2D3C0FFFEFEFEFFCDD3D0FF7FC09BFF76C6
        98FF69C591FF61C88EFF55BC7CFF298936DFC0CCCC11C8D0D400C8D0D400CBA9
        A0AAB77F6EFFD5937FFFD89D8DFFE2A89BFFD9A394FFF5C1B5FFE3BEB2FFECAF
        A1FFD1BBB4FFF5EEEBFFE8DAD5FFDDA597FFE3A89AFF97A785FF1D8A2FFF57C5
        85FF61C98EFF74CA99FF87CAA4FFBCD5C7FFFEFEFEFFCACECCFF8FC5A6FF83CB
        A2FF73CA98FF64CA90FF37A153FF63915FB5C5CFD105C8D0D400C8D0D400C8A9
        A2A0B9816FFFE6A18DFFDCA291FFE5AEA1FFDFAC9EFFFFE8E2FFE7CCC5FFEEB3
        A6FFD1BBB4FFF7F0EDFFEADAD4FFDDA79AFFE4AB9EFFCEB8ADFF408E3FFF37A3
        55FF65C990FF7CCB9EFF93CAABFFC2D5CAFFFDFDFDFFD5D8D6FF96C5ABFF89CB
        A6FF76CA9AFF56BC7DFF208529FFA69F8BA1C7CFD301C8D0D400C8D0D400C5AA
        A399BB8575FFEEAC99FFDFA697FFE8B1A6FFDFC1B9FFFFFEFDFFE9D0CAFFF0B6
        ABFFD0B7B0FFF6ECE8FFEAD7D1FFDFAB9EFFE5AEA2FFD2B5AAFFBBB396FF2886
        2DFF41AB61FF76CA9AFF96CCADFFADC7B8FFBDCBC3FFB4C9BDFF99CAAEFF83CB
        A2FF5EC084FF1D892EFF63844AFFBEAEA398C8D0D400C8D0D400C8D0D400C2AA
        A291AB7C6EFFB38376FFC39285FFEDB9AEFFB3A19AFFBBB1AEFFC5A9A1FFF4BD
        B3FFB4968DFFB7A59FFFB39B93FFDDAA9EFFE8B3A7FFA5877CFFAE8B80FF817E
        5DFF267F28FF289540FF57B97AFF7BC89BFF8DCCA9FF82CBA2FF63C087FF3DA7
        5BFF198628FF53753EFFA3786BFFBEB2A78EC8D0D400C8D0D400C8D0D400C1AB
        A684EDBCB2FFECBBB1FFEFBDB4FFF7C4BBFFECBAB0FFEBB8AFFFEEBBB1FFF6C3
        BAFFEBB8AFFFE8B6ABFFE8B5ABFFF2BEB4FFF4C0B6FFE6B4A8FFE6B3A8FFE5B2
        A7FFD9B4A1FF668C50FF1D8225FF148526FF1B8C2FFF18892CFF148121FF3A80
        33FF86835DFFAD8277FF9E7468FFBFB2A885C8D0D400C8D0D400C8D0D400C0B0
        AB75EFC0B7FEF1C1B9FFF3C4BBFFF8C8C0FFF5C5BCFFF5C5BCFFF6C6BDFFF8C7
        BFFFF7C6BEFFF7C6BDFFF7C5BDFFF8C6BDFFF7C5BDFFF7C5BCFFF7C5BCFFF7C4
        BBFFF2C0B6FFDCAAA0FFF0C6BAFFD0BCA5FFBEB597FFC5B89CFFE0C0AFFFF9C7
        BEFFF7C5BDFFF5C3BAFFF1BFB5FFBFB0A97DC8D0D400C8D0D400C8D0D400C1B4
        B164C29486FEE3CAC2FFD4B5ABFFF2C4BCFFCDB0A8FFE0C7BFFFDDBBB1FFF8C9
        C1FFD1AAA0FFDFBAAFFFDAB1A6FFE7B8AFFFF1C2BAFFD2A295FFD8A597FFD4A1
        92FFE5B4AAFFDCAEA3FFE0B2A8FFDBACA1FFF2C2BAFFE0B1A7FFECBDB4FFE9BA
        B1FFEDBEB5FFECBDB4FFECBDB3FFBFB0AB74C8D0D400C8D0D400C8D0D400C4BD
        BC51C7A59BFBFEFEFEFFDFC9C3FFF1C6BFFFD1C1BCFFF7EDE9FFE4CAC2FFF8CB
        C4FFC9A99FFFE8CAC0FFDDB9ADFFE1B4AAFFEEC1B9FFC49382FFD59B87FFCA8D
        78FFDAAB9FFFDEB1A8FFAD7F72FF996858FFE0B2A8FF8F6052FFC49588FFA679
        6CFFC09085FFAF8175FFC09084FFC0B3AF67C8D0D400C8D0D400C8D0D400CAC6
        C741C8ADA5F8FEFEFEFFE0CBC5FFF2C9C2FFCFBCB6FFF4E7E2FFE2C5BDFFF8CF
        C9FFC7A59AFFE5C2B7FFDAB1A3FFE1B5ACFFEDC4BDFFC18D7BFFD29580FFC889
        73FFDAACA1FFDFB5ACFFAA7D71FF9E6A5AFFE0B5ACFF926151FFC6978BFFA376
        68FFC7978CFFA87A6DFFC39488FEC2B7B558C8D0D400C8D0D400C8D0D400C8C8
        C935C0A198F4AD9C97FFB3978FFFF4CDC7FFB29B94FFBCADA8FFC2A79FFFF9D2
        CEFFBE9B91FFCFAB9FFFCBA293FFDFB5ABFFEDC7C0FFBD8673FFCE8E78FFC784
        6DFFDAAEA3FFE0B9B1FFAC7F72FFA36C5BFFDFB5ADFF956251FFCA9B90FFA273
        66FFCC9D92FFA6776AFFC4988BFCC3BCBB46C8D0D400C8D0D400C8D0D400C8C9
        CB2AF0CDC9F0F8D2CEFFF3CEC8FFFCD7D3FFEAC4BFFFDEBBB6FFDFBCB5FFFCD7
        D3FFD3AFA7FFB9968CFFB18B80FFE2BAB2FFF1CCC7FF9A7063FF9A6D5FFF9968
        58FFD8AFA5FFE2BDB5FFA87E72FF916455FFE1B9B1FF8C6356FFCEA49AFFAE85
        79FFD2AAA2FFBD9489FFD2ABA3F6C4C1C135C8D0D400C8D0D400C8D0D400C8C9
        CC23E7C6C1EEF2D1CCFFF5D2CEFFFDD9D6FFFEDAD7FFFEDCD9FFFEDBD8FFFED9
        D6FFFEDAD7FFFEDAD7FFFED9D6FFFDD9D5FFFDD8D5FFF8D4D0FFF5D2CEFFF0CD
        C8FFEAC6C0FFE4C1BAFFEFCBC6FFEECBC6FFFBD7D3FFF6D4D0FFFBD7D3FFFCD7
        D4FFFDD8D4FFFEDAD7FFEDCAC4EFC7C7C829C8D0D400C8D0D400C8D0D400C6CA
        CC1EC8A8A3ECE9DBD6FFDAC0B7FFF3D2CDFFC6A298FFD5AFA5FFDCB6ADFFFAD9
        D6FFE9C4BEFFE5C0B8FFEAC7C2FFF9D6D3FFFEDBD8FFFEDEDBFFFEDDDAFFFEDC
        D9FFECC9C3FFE6C3BEFFFEDDDBFFFEDFDDFFFEDCDAFFFAD9D5FFF8D5D2FFE7C2
        BCFFF1CCC7FFD6ADA3FFDAB8B0E7C7C8CB21C8D0D400C8D0D400C8D0D400C7CA
        CD18C2A59DEAEDE1DCFFE1C9C1FFF2D1CDFFC0A096FFE7C5BAFFD6AEA1FFF4D7
        D3FFC19A8EFFD19885FFC48974FFDCB5ADFFF1D2CFFFBE8574FFCC9381FFCA97
        88FFDEB8B0FFE6C5C0FFD0A69CFFC39283FFE2BEB7FFA96F5BFFD3AAA1FFAD76
        64FFD8AEA5FFA97361FFD0ADA5E0C8CBCC1AC8D0D400C8D0D400C8D0D400C7CB
        CE12BE9F96E7DECFC9FFDFC6BDFFF2D1CDFFBE9D93FFE3BDAFFFD3A799FFF3D6
        D2FFC2988DFFD49783FFCB8C76FFDDB6ADFFF0D2CFFFB7725CFFC87B61FFC278
        5FFFD6AEA5FFE6C6C1FFB68476FFB87863FFDCB7B0FFAB6B56FFD6ADA4FFB076
        64FFDBB3A9FFAD7462FFCFADA5DCC8CBCE16C8D0D400C8D0D400C8D0D400C7CD
        D00CC49F97E3B0938CFFB0938BFFF0CBC8FFB08C82FFB28F82FFB78E80FFF2D1
        CEFFC0958AFFD0907BFFCB8771FFDBB3AAFFEFD1CEFFB7715AFFC87B62FFC178
        60FFD5ADA4FFE6C2BEFFB88475FFB97964FFD9B2ACFFB06D57FFD9AEA5FFA76F
        5DFFDAB1A8FFA07061FFCCACA4D9C7CBCE12C8D0D400C8D0D400C8D0D400C7CF
        D205BFA29BAED7ACA5F0E8BCB9FDF5C9C8FFE9BEBAFFD4ABA5FFC9A099FFF3CB
        C9FFBD948BFF936556FF8E5D4DFFD1A79FFFEEC9C6FFA96852FFC2775FFFC179
        60FFD3AAA0FFE5BAB5FFB58071FFB2735EFFD5AAA3FF966250FFDAB0A9FFC99F
        97FFECC4C1FFF3CECCFFD5B5B0D4C7CDCF0EC8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8CED107C5C8CA1CC5C2C237C3B7B465BFA5A09ECDACA7BED7B1
        ADD6E2B7B3ECE9BBB7F9EABCB8FEEEC1BFFFF0C4C1FFB1857AFF9C7064FF9366
        58FFCFA39BFFE1AFABFFC09086FFCA9A92FFEDBDBBFFEFC0BEFEEBBEBCFAE0B7
        B2EAD4B0ABCBC1A7A0A1C2B9B750C8CFD303C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C7CE
        D205C6C7C920C2BBBA47C0B1AE6BC6B0AC86C9ABA5ACD0A7A0D8DCB0ACE9E7B8
        B5F5D6A69FFEDCA7A1FEE0AFABF2D0A49FDFC8A8A2AEC3AFAD7AC0B7B651C6C8
        CB1CC7CFD302C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C7CFD301C7CBCD13C7C6C923C6C3
        C434C0B3B15EC1B3B162C6C3C52EC6CACC19C7CFD302C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400}
      TabOrder = 0
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      OnClick = BtnAddDirClick
    end
    object BtnDelDepartament: TcxButton
      Left = 190
      Top = 8
      Width = 163
      Height = 34
      Caption = 'Sterge Casa - Banca'
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        424D361000000000000036000000280000002000000020000000010020000000
        000000000000C40E0000C40E00000000000000000000C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C7CED302C5CB
        D10AC2C7D012C3CAD20BC3CAD208BBC1D315B7BED318B8BFD317BDC5D40FC6CE
        D302C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C7CF
        D301C7CCD00AC7C9CB15C6C5C525C2B9B747BDAAA570BB9F9A8AB9948BA4B987
        77C7B27B6CE2736196C73C3CB6C01B1CB4EA1414B6F41717B6F12829B5D95A5D
        C297A6ACD031C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C7CFD301C6C9CC12C4C0C02CC5BBB93FBFB0AC59BDA39985BB93
        86B1BA8878CDBE8470DCC07E69EBC17A61FBC2785FFFC4826BFFCA8E79FFB17E
        7EFF3A2698FF1717BFFF2323E2FF1616F3FF0C0DF6FF1818F5FF2929EDFF2525
        CFFF1C1CB4EB8D91CB5CC7CFD301C8D0D400C8D0D400C8D0D400C6C8CB15BAA2
        9A82BE9B909FC19384C1C08774E5C68470F1C8846DF7C47F68FCC67F68FFC97F
        68FFC4836EFFD59480FFCF9481FFC67D66FFBD7F6AFFEFECEBFFE6E3EFFF3A39
        BBFF2C2CCCFF4646FBFF2A2AFFFF1212FFFF0606FFFF1414FFFF2C2CFFFF4A4A
        FEFF5352EBFF1717B2F4898EC95DC8D0D400C8D0D400C8D0D400C4C2C226C489
        76EDC07E68FFBA7A63FFC9856FFFC1806BFFBE7861FFCC846CFFC8846FFFCB85
        6EFFD39786FFF5B6A6FFE7AE9EFFCA836DFFC18370FFF0F0F2FF7171CEFF2F2F
        C8FF6868F3FF7979D3FF6465CAFF5554CBFF1B1BF4FF3131EAFF6666CAFF7171
        CAFF7878CBFF6161E8FF2022B4E3B1B7D02AC8D0D400C8D0D400C5C5C71DBF8A
        79E3B36D53FFC27558FFCC8772FFC48572FFC77F68FFD88F78FFCF8C77FFD18B
        77FFD79E8EFFFABFB0FFECBDB1FFCF8B76FFC58976FFD3D2E6FF2020B7FF7575
        F3FF6C6CFBFFB3B3E7FFF7F7F5FFE9E9E4FF6564CDFFA9A9DCFFF5F5F2FFEFEF
        ECFF9292D8FF7F7FFDFF4141D1FF6D61A2B6C5CCD307C8D0D400C6C9CB14C190
        82DAB26D54FFC27559FFCD8B76FFCA8B79FFCC8770FFDF9983FFD4937FFFD692
        7FFFDAA495FFFECDC1FFEFDED9FFD5927FFFC98E7CFF7F7EC8FF3232C6FF8787
        FEFF6F6FFFFF6768EEFFE9E9F5FFFCFCFCFFD8D8DDFFEFEFF1FFFEFEFEFFCDCD
        E3FF6D6DF3FF7E7EFFFF6C6CECFF2D2EB3D7C0C7D40FC8D0D400C7CCCF0CC496
        88D2AB6852FFB86F56FFCC8C79FFCE917FFFB27966FFB67F6FFFC58B7AFFDE9B
        88FFAA7E71FFAE9087FFAF9188FFD89784FFD4947FFF46379BFF4142D0FF8383
        FFFF6E6EFFFF605FFCFFA2A2EAFFFEFEFEFFFEFEFEFFFEFEFEFFF3F3F3FF8686
        E3FF6F6FFEFF7A7AFFFF7676F4FF1D1DB5F0BCC3D417C8D0D400C7CED106C99D
        92C6AC7869FFB07C6DFFD49888FFE1A292FFCF9584FFD89B8AFFE2A291FFE7A5
        94FFE7A594FFE7A493FFE7A493FFE5A292FFE9A591FF563FA4FF3D3DD1FF7979
        FFFF6E6EFFFF6868FEFF8787E8FFF9F9FAFFFFFFFFFFFEFEFEFFD5D5DCFF7676
        E7FF7575FFFF7676FFFF7070F6FF1C1CB6F4BBC2D418C8D0D400C7CFD202CBA4
        9BBBEFB0A3FFEEB0A1FFEDAFA0FFEDAE9FFFEDAE9FFFECAD9EFFEBAC9EFFECAC
        9DFFEBAC9DFFECAB9CFFEBAB9CFFEAAA9AFFEDAC99FF6047A6FF3535CEFF6F6F
        FFFF6B6BFFFF7071F5FFC6C6E3FFFEFEFDFFFEFEFEFFFEFEFEFFF1F1F0FF9D9D
        CCFF7A7AFAFF7373FFFF6969F4FF1D1DB5EFBCC3D417C8D0D400C7CFD301CEA9
        A0B2ECB0A2FFE9ADA0FFEBAFA1FFEDB0A3FFE7AB9DFFE6AA9CFFE7AB9DFFEDAF
        A2FFDDA597FFDAA597FFD6A193FFE8AB9CFFE1A596FF876C9DFF2323C2FF6868
        FCFF6C6CFEFF9494E2FFF7F7F6FFFEFEFEFFD8D8E7FFE6E6F0FFFDFDFDFFD8D8
        DAFF8282DFFF7475FFFF5A5AEAFF2F30B6CFC2C9D30CC8D0D400C8D0D400CAA8
        A0AABE8372FFD4937FFFD29888FFE2A99CFFDCA697FFF5C3B7FFE0B8ACFFEBAF
        A1FFD6C5BFFFF5EDEBFFE8D8D2FFE4AB9DFFD7A194FFD0BCC2FF221FB3FF5959
        ECFF6D6DF8FFBDBDE1FFDBDBECFFE0E0ECFFC5C5EBFFC8C8EBFFDFDFEDFFD8D8
        EAFFA6A6D9FF7676FDFF3434CDFF7173C18BC8D0D400C8D0D400C8D0D400C8A9
        A1A0C38775FFE5A08DFFD39A8AFFE5AEA1FFE1AEA2FFFEEBE5FFE3C4BCFFEDB3
        A6FFD5C4BFFFF7F0EDFFE9D7D1FFE5AEA1FFD8A497FFE0C6BCFF8876B5FF1E1E
        BDFF6D6DF7FF8D8DFBFFAFAFFBFFCBCBFBFFDCDCFEFFD1D1FEFFBBBBFBFFA3A3
        FBFF8989FCFF5454E4FF231CABFFACA4B665C8D0D400C8D0D400C8D0D400C5AA
        A399C58C7BFFEDAA98FFD69E8FFFE7B1A6FFE1C6BFFFFEFEFDFFE3C6BEFFF0B7
        ABFFD4C0BAFFF6EBE7FFEAD7D0FFE8B3A7FFD9A599FFDDBFB5FFDCB8B2FF5445
        ABFF2323C1FF7171F2FFA6A6FEFFCACAFFFFD9D9FFFFCCCCFFFFB5B5FFFF908F
        FCFF5455E4FF1311ACFF8E6A96FEB9B1B856C8D0D400C8D0D400C8D0D400C2AA
        A291AE7E70FFB38376FFC08F82FFEBB8ADFFB1A29CFFBBB1ADFFC4A49BFFF4BD
        B2FFB39991FFB6A59FFFB59D94FFE7B3A8FFD9A79BFFA98B80FFAD8A7EFFA57E
        76FF6D54A3FF1514B3FF3737D1FF6262EAFF6E6EEEFF6A6AEDFF5454E3FF2525
        C5FF2821B1FF65486AFFBC8D80FCBDB8BC44C8D0D400C8D0D400C8D0D400C1AB
        A684EDBCB2FFECBBB1FFEFBDB4FFF6C3BAFFECBAB0FFEBB8AFFFEEBBB1FFF6C3
        BAFFEBB8AEFFE8B6ACFFE9B5ABFFF4C0B6FFF2BEB3FFE6B3A8FFE6B3A8FFE5B2
        A7FFE1ADA1FFB590A9FF715BB5FF2B24B2FF1312B0FF1917B1FF3D32AFFF5E45
        83FFB98C8EFF815C51FFBA8D80F6C0BFC434C8D0D400C8D0D400C8D0D400C0B0
        AB75EFC0B7FEF1C1B9FFF3C4BBFFF7C8C0FFF5C5BCFFF5C5BCFFF7C6BDFFF8C7
        BFFFF7C6BEFFF7C5BDFFF7C5BDFFF8C6BDFFF8C5BCFFF7C5BCFFF7C5BCFFF7C4
        BBFFE2B0A5FFEABAB1FFFACAC0FFF8C8BFFFEFC1BEFFF2C2BEFFFACABEFFF9C7
        BEFFF7C5BCFFF3C1B8FFE4B6ACEEC3C3C928C8D0D400C8D0D400C8D0D400C1B3
        B164C6988BFEE3CAC1FFD5B4ABFFEFC2B9FFCAAFA7FFE0C6BFFFD9B4AAFFF7C8
        C0FFD1ABA1FFDFB9AEFFDCB3A7FFF0C1B8FFE9BBB1FFD5A497FFD7A596FFD5A1
        93FFDCACA0FFEBBEB5FFDDAEA4FFDFB0A5FFEEC0B7FFE1B2A8FFF1C2BAFFE6B8
        AEFFF1C2B9FFE9B9B0FFE0B6ADE7C5C7C920C8D0D400C8D0D400C8D0D400C4BD
        BC51CAADA4FBFEFEFEFFE2CCC5FFEEC3BBFFCEBFBAFFF6ECE8FFDCBDB5FFF8CB
        C4FFC9ABA2FFE8C9BFFFDFBAAEFFEEC1B9FFE1B4ABFFCC9987FFD49A86FFC88D
        78FFD6A89DFFEAC2BAFFA17366FFA67567FFD1A59AFF946456FFD8ABA0FF9B6C
        5EFFD5A79CFF95685BFFC8A196E0C7C9CB1AC8D0D400C8D0D400C8D0D400CAC6
        C741C8B3ADF8FEFEFEFFE4D0CAFFEFC7C0FFCBB9B3FFF4E6E1FFDABBB2FFF8CE
        C8FFC7A69CFFE5C1B5FFDCB3A5FFEEC3BCFFE1B7AEFFCA9380FFD1937EFFC789
        74FFD6A99EFFEBC7C0FFA17265FFAA7869FFD1A69CFF986556FFD8ACA3FF9A69
        5AFFDAACA2FF926355FFC9A298DCC7CBCD16C8D0D400C8D0D400C8D0D400C8C8
        C935BB9E97F4AC9C97FFB99E96FFF2CCC6FFAD968FFFBDADA8FFC1A39AFFFAD2
        CDFFBB998FFFCFAA9EFFCEA495FFEEC4BDFFE0B7AFFFC68B77FFCE8D76FFC786
        70FFD6AA9FFFEDCBC5FFA47467FFAE7A6BFFCFA59BFF9D6757FFD8AFA6FF9B68
        59FFDCB1A8FF936254FFC7A399D8C7CBCE12C8D0D400C8D0D400C8D0D400C8C9
        CB2AF0CCC8F0F7D2CEFFF3CEC9FFFBD6D3FFE8C2BDFFDEBBB5FFE1BDB6FFFDD7
        D3FFCFAAA2FFB8948BFFB38C81FFEEC7C1FFE4BEB7FF996F61FF9A6C5EFF9F6E
        5FFFD5ABA1FFF0CFCAFF9A7064FFA27568FFD2A9A1FF90675BFFDEB7AFFFA37A
        6DFFE4BDB5FFB2897EFFCCAAA1D3C7CCCF0DC8D0D400C8D0D400C8D0D400C8C9
        CC23E5C4BFEEF2D1CCFFF6D3CEFFFDDAD7FFFEDAD7FFFEDCD9FFFEDBD8FFFED9
        D6FFFEDAD7FFFEDAD7FFFED9D6FFFDD8D5FFFCD8D4FFF7D4D0FFF4D1CDFFF1CD
        C9FFDCB4ABFFF6D4D0FFEDC9C4FFF1CEC9FFF9D6D3FFF7D5D1FFFCD8D4FFFCD8
        D4FFFDD8D5FFFEDBD8FFD0B2ACCDC7CED109C8D0D400C8D0D400C8D0D400C6CA
        CC1EBC9D96ECEADCD7FFDFC5BDFFF5D5D1FFC29C91FFD6B0A5FFDDB7AEFFFCDA
        D7FFE7C1BBFFE5C0BAFFEBC9C4FFFBD8D5FFFEDBD8FFFEDEDBFFFEDDDAFFFEDC
        D9FFDDB5ADFFF8D7D4FFFFDEDCFFFEDEDCFFFEDCDAFFF8D6D3FFF7D5D1FFE4BE
        B7FFF1CDC8FFD1A79CFFC9AAA2C6C7CED205C8D0D400C8D0D400C8D0D400C7CA
        CD18B6978DEAEEE1DCFFE5CEC7FFF4D6D3FFC19D92FFE7C4B8FFD7AFA2FFF8DA
        D7FFC09587FFD19784FFC68B77FFEDC9C3FFE7C5BFFFC28875FFCC9483FFCE9D
        90FFD7AFA6FFF6D8D5FFCB9E93FFCA9B8EFFD0A9A1FFAF7460FFD9B6AEFFAB70
        5CFFDAB4ADFFAE735FFFC7ACA5B8C7CFD302C8D0D400C8D0D400C8D0D400C7CB
        CE12B39187E7DFD0CAFFE2C9C0FFF5D6D2FFC09B90FFE3BCAFFFD6AB9EFFF8DA
        D7FFC19386FFD49782FFCD8D78FFEEC9C3FFE3C3BDFFBD745BFFC87B61FFC57F
        69FFD3ACA2FFF5D7D5FFB47B6AFFC08775FFC9A298FFB5745FFFD9B5AEFFB073
        5FFFDBB5AEFFB57865FFCDB7B1A7C8D0D400C8D0D400C8D0D400C8D0D400C7CC
        D00CC09A90E3B1948DFFB3958EFFF3CFCDFFAE897EFFB38F82FFBD9487FFF8D6
        D4FFBF9082FFD1907BFFCD8973FFEDC6C0FFE3C1BCFFBD735AFFC87B62FFC580
        6AFFD2ABA2FFF5D2D1FFB77C6AFFC18776FFC79D94FFB97761FFDAB3ADFFA76D
        59FFD9B3ACFFA97869FFCCB7B498C8D0D400C8D0D400C8D0D400C8D0D400C7CF
        D205C1A39DB0D6ACA6F0E8BDBAFEF4CAC8FFE7BDB9FFD3AAA4FFCCA49DFFF7CE
        CDFFB58A80FF926455FF905F4FFFE3BAB6FFE0B9B4FFAF6A53FFC4785FFFC681
        6BFFD1A79FFFF2C8C6FFB37866FFBB8271FFC1958BFFA26E5DFFE2BAB5FFCCA2
        9BFFF3CCCAFFF6D1D0FFC9B6B18BC8D0D400C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8CED107C5C8C91EC5C2C238C3B5B369C0A69FA1CEADA8BFD9B2
        AED9E2B7B4EEE9BBB8FAE9BCB8FEF1C4C2FFEABDB9FFAC7F75FF996D61FF9C6F
        62FFCEA198FFEDBBB9FFBC8C82FFD5A59EFFEEBEBBFFF0C1BFFEE8BDB9F7DDB5
        B1E2CFAFA9C2C0A9A390C6C4C62EC8D0D400C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8CD
        D107C6C6C823C2BAB94CC1B0AD6FC7B0AC89CAA9A4B4D1A8A2DCDFB2AEECE9BA
        B7F8CC9C93FEE6B0ACFCDEADA8EFCCA39CD7C8ACA99FC1B0AD71C4BDBD42C7CB
        CE11C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C7CFD302C7CACD16C7C6C826C4C0
        C038C0AFAD69C3B8B652C6C5C629C7CBCE13C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0
        D400C8D0D400C8D0D400C8D0D400C8D0D400C8D0D400}
      TabOrder = 1
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      OnClick = BtnDelDepartamentClick
    end
  end
  object TreeRepartitori: TdxDBTreeList
    Left = 219
    Top = 116
    Width = 409
    Height = 217
    SearchType = stStart
    Bands = <
      item
      end>
    DefaultLayout = True
    HeaderPanelRowCount = 1
    KeyField = 'ID_REPARTITORI'
    ParentField = 'ID_PARINTE'
    TabOrder = 6
    Visible = False
    DataSource = frmData.DTRepartitori
    LookAndFeel = lfUltraFlat
    OptionsBehavior = [etoAutoSearch, etoAutoSort]
    OptionsDB = [etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
    OptionsView = [etoAutoWidth, etoIndicator, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
    TreeLineColor = clGrayText
    object TreeRepartitoriNUME: TdxDBTreeListMaskColumn
      Caption = 'Denumire'
      DisableEditor = True
      HeaderAlignment = taCenter
      Width = 117
      BandIndex = 0
      RowIndex = 0
      FieldName = 'NUME'
    end
    object TreeRepartitoriCONT: TdxDBTreeListMaskColumn
      Caption = 'Cont'
      DisableEditor = True
      HeaderAlignment = taCenter
      Sorted = csUp
      Width = 49
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ID_REPARTITORI'
    end
    object TreeRepartitoriCODSECTIE: TdxDBTreeListMaskColumn
      Caption = 'Cod'
      DisableEditor = True
      HeaderAlignment = taCenter
      Width = 30
      BandIndex = 0
      RowIndex = 0
      FieldName = 'CODSECTIE'
    end
    object TreeRepartitoriADRESA: TdxDBTreeListMaskColumn
      Caption = 'Adresa'
      DisableEditor = True
      HeaderAlignment = taCenter
      Width = 102
      BandIndex = 0
      RowIndex = 0
      FieldName = 'ADRESA'
    end
    object TreeRepartitoriGESTINT: TdxDBTreeListCheckColumn
      Caption = 'Interna'
      DisableEditor = True
      HeaderAlignment = taCenter
      Width = 38
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
      Width = 59
      BandIndex = 0
      RowIndex = 0
      FieldName = 'TIP_GESTIUNE'
    end
  end
  object DTCasa: TDataSource
    DataSet = QryCase
    Left = 104
    Top = 144
  end
  object QryCase: TZQuery
    Connection = frmData.dbContabilitate
    AfterInsert = QryCaseAfterInsert
    AfterEdit = QryCaseAfterEdit
    SQL.Strings = (
      'SELECT * FROM CASIERIE')
    Params = <>
    Left = 175
    Top = 144
  end
  object DTSoldInitial: TDataSource
    DataSet = QrySoldInitial
    Left = 105
    Top = 203
  end
  object QrySoldInitial: TZQuery
    Connection = frmData.dbContabilitate
    OnNewRecord = QrySoldInitialNewRecord
    SQL.Strings = (
      'SELECT * FROM BREGISTRU '
      'WHERE ISNULL(SOLD_INITIAL,0) = 1 AND COD_CB = :COD_CB'
      'ORDER BY COD_CB, DATA, POZ ')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'COD_CB'
        ParamType = ptUnknown
        Size = 4
        Value = 1
      end>
    Left = 176
    Top = 203
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'COD_CB'
        ParamType = ptUnknown
        Size = 4
        Value = 1
      end>
  end
  object SoldActions: TActionList
    Left = 32
    Top = 145
    object Cmd_CalculateSold: TAction
      Caption = 'Calculeaza Sold'
      ShortCut = 16474
      OnExecute = Cmd_CalculateSoldExecute
    end
    object Cmd_Update: TAction
      Caption = 'Update'
      OnExecute = Cmd_UpdateExecute
    end
    object Cmd_SoldPlanConturi: TAction
      Caption = 'Sold Contabilitate'
      OnExecute = Cmd_SoldPlanConturiExecute
    end
  end
  object ppSoldMenu: TPopupMenu
    Left = 264
    Top = 145
    object CalculeazaSold1: TMenuItem
      Action = Cmd_CalculateSold
    end
    object SoldContabilitate1: TMenuItem
      Action = Cmd_SoldPlanConturi
    end
  end
end
