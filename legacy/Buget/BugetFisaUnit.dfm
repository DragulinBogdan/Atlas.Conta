object frmFisaBugetara: TfrmFisaBugetara
  Left = 288
  Top = 85
  AutoScroll = False
  Caption = 'Fisa Bugetara'
  ClientHeight = 590
  ClientWidth = 914
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poDefault
  OnActivate = FormActivate
  OnClose = FormClose
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object pnTop: TPanel
    Left = 0
    Top = 0
    Width = 914
    Height = 91
    Align = alTop
    BevelOuter = bvNone
    TabOrder = 0
    DesignSize = (
      914
      91)
    object cxLabel1: TcxLabel
      Left = 8
      Top = 8
      Caption = 'Clasificatie Functionala:'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
    end
    object RPFunctional: TcxRepartitorPanel
      Tag = -1
      Left = 163
      Top = -3
      Anchors = [akLeft, akTop, akRight]
      TabOrder = 1
      PopupEdit.AutoSize = True
      PopupEdit.AutoSelect = True
      PopupEdit.CharCase = ecNormal
      PopupEdit.Color = clWindow
      PopupEdit.Enabled = True
      PopupEdit.Font.Charset = DEFAULT_CHARSET
      PopupEdit.Font.Color = clWindowText
      PopupEdit.Font.Height = -11
      PopupEdit.Font.Name = 'MS Sans Serif'
      PopupEdit.Font.Style = []
      PopupEdit.HideEditCursor = True
      PopupEdit.HideSelection = True
      PopupEdit.PopupAutoSize = False
      PopupEdit.PopupAlignment = taLeftJustify
      PopupEdit.PopupClientEdge = False
      PopupEdit.PopupControl = cxTreeFunctionalComplet
      PopupEdit.PopupFlatBorder = True
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 619
      TextEdit.AutoSelect = True
      TextEdit.AutoSize = True
      TextEdit.CharCase = ecNormal
      TextEdit.Color = clWindow
      TextEdit.Enabled = True
      TextEdit.Font.Charset = DEFAULT_CHARSET
      TextEdit.Font.Color = clWindowText
      TextEdit.Font.Height = -11
      TextEdit.Font.Name = 'MS Sans Serif'
      TextEdit.Font.Style = []
      TextEdit.Height = 21
      TextEdit.HideSelection = True
      TextEdit.Style.Color = clWindow
      TextEdit.Visible = True
      TextEdit.Width = 80
      ButonEdit.Caption = '...'
      ButonEdit.Visible = True
      ButonEdit.Color = clBlack
      ButonEdit.Font.Charset = DEFAULT_CHARSET
      ButonEdit.Font.Color = clWindowText
      ButonEdit.Font.Height = -11
      ButonEdit.Font.Name = 'MS Sans Serif'
      ButonEdit.Font.Style = []
      ButonEdit.Flat = True
      ButonEdit.Enabled = True
      OnlySelectChild = False
      ValidateEditText = False
      ValidateWithPopup = True
      CodField = 'COD_FUNCTIONAL'
      KeyField = 'ID_BG_PLAN_FUNCTIONAL'
      ListField = 'DENUMIRE'
      OnPopupInitPopup = RPFunctionalPopupInitPopup
      OnValidate = RPFunctionalValidate
      Height = 31
      Width = 739
    end
    object edCategorie: TcxImageComboBox
      Left = 8
      Top = 65
      EditValue = 0
      Properties.ClearKey = 46
      Properties.ImmediatePost = True
      Properties.ImmediateUpdateText = True
      Properties.Items = <
        item
          Description = 'Buget General'
          ImageIndex = 0
          Value = 0
        end
        item
          Description = 'Proiecte/Investitii'
          Value = 1
        end
        item
          Description = 'Unitati/Subunitati'
          Value = 2
        end>
      Properties.OnChange = edCategoriePropertiesChange
      Properties.OnValidate = edCategoriePropertiesValidate
      TabOrder = 2
      Width = 145
    end
    object lbDefalcare: TcxLabel
      Left = 184
      Top = 67
      AutoSize = False
      Caption = 'Proiect:'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
      Properties.Alignment.Horz = taRightJustify
      Visible = False
      Height = 17
      Width = 53
      AnchorX = 237
    end
    object RPProiect: TcxRepartitorPanel
      Tag = -1
      Left = 243
      Top = 58
      Anchors = [akLeft, akTop, akRight]
      TabOrder = 4
      Visible = False
      PopupEdit.AutoSize = True
      PopupEdit.AutoSelect = True
      PopupEdit.CharCase = ecNormal
      PopupEdit.Color = clWindow
      PopupEdit.Enabled = True
      PopupEdit.Font.Charset = DEFAULT_CHARSET
      PopupEdit.Font.Color = clWindowText
      PopupEdit.Font.Height = -11
      PopupEdit.Font.Name = 'MS Sans Serif'
      PopupEdit.Font.Style = []
      PopupEdit.HideEditCursor = True
      PopupEdit.HideSelection = True
      PopupEdit.PopupAutoSize = False
      PopupEdit.PopupAlignment = taLeftJustify
      PopupEdit.PopupClientEdge = False
      PopupEdit.PopupFlatBorder = True
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 539
      TextEdit.AutoSelect = True
      TextEdit.AutoSize = True
      TextEdit.CharCase = ecNormal
      TextEdit.Color = clWindow
      TextEdit.Enabled = True
      TextEdit.Font.Charset = DEFAULT_CHARSET
      TextEdit.Font.Color = clWindowText
      TextEdit.Font.Height = -11
      TextEdit.Font.Name = 'MS Sans Serif'
      TextEdit.Font.Style = []
      TextEdit.Height = 21
      TextEdit.HideSelection = True
      TextEdit.Style.Color = clWindow
      TextEdit.Visible = True
      TextEdit.Width = 80
      ButonEdit.Caption = '...'
      ButonEdit.Visible = True
      ButonEdit.Color = clBlack
      ButonEdit.Font.Charset = DEFAULT_CHARSET
      ButonEdit.Font.Color = clWindowText
      ButonEdit.Font.Height = -11
      ButonEdit.Font.Name = 'MS Sans Serif'
      ButonEdit.Font.Style = []
      ButonEdit.Flat = True
      ButonEdit.Enabled = True
      OnlySelectChild = False
      ValidateEditText = False
      ValidateWithPopup = True
      CodField = 'ID_OI_PROIECTE'
      OnPopupInitPopup = RPFunctionalPopupInitPopup
      OnValidate = RPProiectValidate
      Height = 31
      Width = 659
    end
    object cxLabel2: TcxLabel
      Left = 8
      Top = 40
      Caption = 'Clasificatie Economica'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
    end
    object RPEconomic: TcxRepartitorPanel
      Tag = -1
      Left = 163
      Top = 29
      Anchors = [akLeft, akTop, akRight]
      TabOrder = 6
      PopupEdit.AutoSize = True
      PopupEdit.AutoSelect = True
      PopupEdit.CharCase = ecNormal
      PopupEdit.Color = clWindow
      PopupEdit.Enabled = True
      PopupEdit.Font.Charset = DEFAULT_CHARSET
      PopupEdit.Font.Color = clWindowText
      PopupEdit.Font.Height = -11
      PopupEdit.Font.Name = 'MS Sans Serif'
      PopupEdit.Font.Style = []
      PopupEdit.HideEditCursor = True
      PopupEdit.HideSelection = True
      PopupEdit.PopupAutoSize = False
      PopupEdit.PopupAlignment = taLeftJustify
      PopupEdit.PopupClientEdge = False
      PopupEdit.PopupControl = cxTreeEconomic
      PopupEdit.PopupFlatBorder = True
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 619
      TextEdit.AutoSelect = True
      TextEdit.AutoSize = True
      TextEdit.CharCase = ecNormal
      TextEdit.Color = clWindow
      TextEdit.Enabled = True
      TextEdit.Font.Charset = DEFAULT_CHARSET
      TextEdit.Font.Color = clWindowText
      TextEdit.Font.Height = -11
      TextEdit.Font.Name = 'MS Sans Serif'
      TextEdit.Font.Style = []
      TextEdit.Height = 21
      TextEdit.HideSelection = True
      TextEdit.Style.Color = clWindow
      TextEdit.Visible = True
      TextEdit.Width = 80
      ButonEdit.Caption = '...'
      ButonEdit.Visible = True
      ButonEdit.Color = clBlack
      ButonEdit.Font.Charset = DEFAULT_CHARSET
      ButonEdit.Font.Color = clWindowText
      ButonEdit.Font.Height = -11
      ButonEdit.Font.Name = 'MS Sans Serif'
      ButonEdit.Font.Style = []
      ButonEdit.Flat = True
      ButonEdit.Enabled = True
      OnlySelectChild = False
      ValidateEditText = False
      ValidateWithPopup = True
      CodField = 'COD_ECONOMIC'
      KeyField = 'ID_BG_PLAN_ECONOMIC'
      ListField = 'DENUMIRE'
      OnPopupInitPopup = RPFunctionalPopupInitPopup
      OnValidate = RPEconomicValidate
      Height = 31
      Width = 739
    end
  end
  object pnClient: TPanel
    Left = 0
    Top = 91
    Width = 914
    Height = 301
    Align = alClient
    BevelOuter = bvNone
    TabOrder = 1
    object cxGrid: TcxGrid
      Left = 0
      Top = 0
      Width = 914
      Height = 301
      Align = alClient
      Enabled = False
      TabOrder = 0
      object GridBugetar: TcxGridDBBandedTableView
        NavigatorButtons.ConfirmDelete = False
        OnFocusedRecordChanged = GridBugetarFocusedRecordChanged
        DataController.DataSource = DTLista
        DataController.KeyFieldNames = 'ID_BUGET_FISA_BUGET'
        DataController.Summary.DefaultGroupSummaryItems = <>
        DataController.Summary.FooterSummaryItems = <>
        DataController.Summary.SummaryGroups = <>
        OptionsView.ColumnAutoWidth = True
        OptionsView.GroupByBox = False
        OptionsView.HeaderAutoHeight = True
        OptionsView.Indicator = True
        OptionsView.BandHeaderLineCount = 2
        Styles.Background = frmData.cxStyle48
        Styles.Header = frmData.cxStyle47
        Styles.BandHeader = frmData.cxStyle47
        Bands = <
          item
            Caption = 'Nr.Crt.'
            Options.HoldOwnColumnsOnly = True
            Width = 36
          end
          item
            Caption = 'Data'
            Options.HoldOwnColumnsOnly = True
            Width = 76
          end
          item
            Caption = 'Buget Anual'
            Options.HoldOwnColumnsOnly = True
            Width = 98
          end
          item
            Caption = 'Buget Trimestrial'
            Options.HoldOwnColumnsOnly = True
            Width = 101
          end
          item
            Caption = 'Propunere / Angajament'
            Options.HoldOwnColumnsOnly = True
            Width = 102
          end
          item
            Caption = 'Furnizor'
            Options.HoldOwnColumnsOnly = True
            Width = 102
          end
          item
            Caption = 'Explicatii'
            Options.HoldOwnColumnsOnly = True
            Width = 249
          end
          item
            Caption = 'Ordonatare / Plata'
            Options.HoldOwnColumnsOnly = True
            Width = 115
          end
          item
            Caption = 'Disponibil'
            Options.HoldOwnColumnsOnly = True
            Width = 130
          end>
        object GridBugetarNR_CRT: TcxGridDBBandedColumn
          AlternateCaption = 'Nr. Crt.'
          Caption = '0'
          DataBinding.FieldName = 'NR_CRT'
          HeaderAlignmentHorz = taCenter
          HeaderAlignmentVert = vaCenter
          Width = 53
          Position.BandIndex = 0
          Position.ColIndex = 5
          Position.RowIndex = 0
        end
        object GridBugetarDATA: TcxGridDBBandedColumn
          AlternateCaption = 'Data'
          Caption = '1'
          DataBinding.FieldName = 'DATA'
          HeaderAlignmentHorz = taCenter
          HeaderAlignmentVert = vaCenter
          Width = 93
          Position.BandIndex = 1
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object GridBugetarBUGET_ANUAL: TcxGridDBBandedColumn
          AlternateCaption = 'Buget Anual'
          Caption = '2'
          DataBinding.FieldName = 'BUGET_ANUAL'
          HeaderAlignmentHorz = taCenter
          HeaderAlignmentVert = vaCenter
          Options.Editing = False
          Width = 102
          Position.BandIndex = 2
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object GridBugetarBUGET_TRIM: TcxGridDBBandedColumn
          AlternateCaption = 'Buget Trimestrial'
          Caption = '3'
          DataBinding.FieldName = 'BUGET_TRIM'
          HeaderAlignmentHorz = taCenter
          HeaderAlignmentVert = vaCenter
          Options.Editing = False
          Width = 103
          Position.BandIndex = 3
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object GridBugetarANGAJAMENT: TcxGridDBBandedColumn
          AlternateCaption = 'Propunere / Angajament'
          Caption = '4'
          DataBinding.FieldName = 'ANGAJAMENT'
          HeaderAlignmentHorz = taCenter
          HeaderAlignmentVert = vaCenter
          Width = 103
          Position.BandIndex = 4
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object GridBugetarFURNIZOR: TcxGridDBBandedColumn
          AlternateCaption = 'Furnizor'
          Caption = '5'
          DataBinding.FieldName = 'FURNIZOR'
          PropertiesClassName = 'TcxPopupEditProperties'
          Properties.PopupSysPanelStyle = True
          HeaderAlignmentHorz = taCenter
          HeaderAlignmentVert = vaCenter
          Width = 63
          Position.BandIndex = 5
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object GridBugetarEXPLICATII: TcxGridDBBandedColumn
          AlternateCaption = 'Explicatii'
          Caption = '6'
          DataBinding.FieldName = 'EXPLICATII'
          HeaderAlignmentHorz = taCenter
          HeaderAlignmentVert = vaCenter
          Width = 214
          Position.BandIndex = 6
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object GridBugetarORDONANTARE: TcxGridDBBandedColumn
          AlternateCaption = 'Ordonantare'
          Caption = '7'
          DataBinding.FieldName = 'ORDONANTARE'
          HeaderAlignmentHorz = taCenter
          HeaderAlignmentVert = vaCenter
          Width = 100
          Position.BandIndex = 7
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object GridBugetarID_BUGET: TcxGridDBBandedColumn
          DataBinding.FieldName = 'ID_BUGET'
          Visible = False
          VisibleForCustomization = False
          Position.BandIndex = 0
          Position.ColIndex = 6
          Position.RowIndex = 0
        end
        object GridBugetarID_ANGAJAMENT: TcxGridDBBandedColumn
          DataBinding.FieldName = 'ID_ANGAJAMENT'
          Visible = False
          VisibleForCustomization = False
          Position.BandIndex = 0
          Position.ColIndex = 7
          Position.RowIndex = 0
        end
        object GridBugetarID_ORDONANTARE: TcxGridDBBandedColumn
          DataBinding.FieldName = 'ID_ORDONANTARE'
          Visible = False
          VisibleForCustomization = False
          Position.BandIndex = 0
          Position.ColIndex = 8
          Position.RowIndex = 0
        end
        object GridBugetarID_BUGET_FISA_BUGET: TcxGridDBBandedColumn
          DataBinding.FieldName = 'ID_BUGET_FISA_BUGET'
          Visible = False
          VisibleForCustomization = False
          Position.BandIndex = 0
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object GridBugetarCOD_FUNCTIONAL: TcxGridDBBandedColumn
          DataBinding.FieldName = 'COD_FUNCTIONAL'
          Visible = False
          VisibleForCustomization = False
          Position.BandIndex = 0
          Position.ColIndex = 1
          Position.RowIndex = 0
        end
        object GridBugetarCOD_ECONOMIC: TcxGridDBBandedColumn
          DataBinding.FieldName = 'COD_ECONOMIC'
          Visible = False
          VisibleForCustomization = False
          Position.BandIndex = 0
          Position.ColIndex = 2
          Position.RowIndex = 0
        end
        object GridBugetarID_OI_UNITATI: TcxGridDBBandedColumn
          DataBinding.FieldName = 'ID_OI_UNITATI'
          Visible = False
          VisibleForCustomization = False
          Position.BandIndex = 0
          Position.ColIndex = 3
          Position.RowIndex = 0
        end
        object GridBugetarID_OI_PROIECTE: TcxGridDBBandedColumn
          DataBinding.FieldName = 'ID_OI_PROIECTE'
          Visible = False
          VisibleForCustomization = False
          Position.BandIndex = 0
          Position.ColIndex = 4
          Position.RowIndex = 0
        end
        object GridBugetarDISP_ANUAL: TcxGridDBBandedColumn
          Caption = 'Anual (col 2-4)'
          HeaderAlignmentHorz = taCenter
          HeaderAlignmentVert = vaCenter
          HeaderGlyphAlignmentHorz = taCenter
          Position.BandIndex = 8
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object GridBugetarDISP_TRIM: TcxGridDBBandedColumn
          Caption = 'Trim. (col. 3-6)'
          HeaderAlignmentHorz = taCenter
          HeaderAlignmentVert = vaCenter
          HeaderGlyphAlignmentHorz = taCenter
          Position.BandIndex = 8
          Position.ColIndex = 1
          Position.RowIndex = 0
        end
      end
      object GridBugetarL: TcxGridLevel
        GridView = GridBugetar
      end
    end
    object cxTreeUnitati: TcxDBTreeList
      Left = 154
      Top = 217
      Width = 250
      Height = 150
      Bands = <
        item
        end>
      DataController.DataSource = frmData.DTOIUnitati
      DataController.ParentField = 'ID_PARINTE'
      DataController.KeyField = 'ID_OI_UNITATI'
      OptionsBehavior.ExpandOnIncSearch = True
      OptionsBehavior.IncSearch = True
      OptionsBehavior.IncSearchItem = cxTreeUnitatiDESCRIERE
      OptionsData.Editing = False
      OptionsData.Deleting = False
      OptionsView.ColumnAutoWidth = True
      RootValue = -1
      TabOrder = 1
      Visible = False
      OnDblClick = cxTreeFunctionalDblClick
      OnKeyDown = cxTreeFunctionalKeyDown
      object cxTreeUnitatiID_OI_UNITATI: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ID_OI_UNITATI'
        Position.ColIndex = 3
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeUnitatiID_OI_UNITATI_TIPURI: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ID_OI_UNITATI_TIPURI'
        Position.ColIndex = 4
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeUnitatiID_PARINTE: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ID_PARINTE'
        Position.ColIndex = 2
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeUnitatiDENUMIRE: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'DENUMIRE'
        Position.ColIndex = 0
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeUnitatiDESCRIERE: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        Caption.GlyphAlignHorz = taCenter
        Caption.Text = 'Institutie/Unitate'
        DataBinding.FieldName = 'DESCRIERE'
        Position.ColIndex = 1
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeUnitatiUNITATATEA_URMARITA: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'UNITATATEA_URMARITA'
        Position.ColIndex = 11
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeUnitatiNUME_ORDONANTATOR: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'NUME_ORDONANTATOR'
        Position.ColIndex = 10
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeUnitatiID_UTILIZATORI: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ID_UTILIZATORI'
        Position.ColIndex = 13
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeUnitatiSTARE: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'STARE'
        Position.ColIndex = 12
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeUnitatiUNITATEA_CENTRALIZATOARE: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'UNITATEA_CENTRALIZATOARE'
        Position.ColIndex = 9
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeUnitatiBANCA: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'BANCA'
        Position.ColIndex = 6
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeUnitatiBANCA_COD: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'BANCA_COD'
        Position.ColIndex = 5
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeUnitatiBANCA_CONT: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'BANCA_CONT'
        Position.ColIndex = 8
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeUnitatiCOD_FUNCTIONAL: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'COD_FUNCTIONAL'
        Position.ColIndex = 7
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
    end
    object cxTreeProiecte: TcxDBTreeList
      Left = 146
      Top = 57
      Width = 250
      Height = 150
      Bands = <
        item
        end>
      DataController.DataSource = frmData.DTOIProiecte
      DataController.ParentField = 'ID_PARINTE'
      DataController.KeyField = 'ID_OI_PROIECTE'
      OptionsBehavior.ExpandOnIncSearch = True
      OptionsBehavior.IncSearch = True
      OptionsBehavior.IncSearchItem = cxTreeProiecteDESCRIERE
      OptionsData.Editing = False
      OptionsData.Deleting = False
      OptionsView.ColumnAutoWidth = True
      RootValue = -1
      TabOrder = 2
      Visible = False
      OnDblClick = cxTreeFunctionalDblClick
      OnKeyDown = cxTreeFunctionalKeyDown
      object cxTreeProiecteID_OI_PROIECTE: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ID_OI_PROIECTE'
        Position.ColIndex = 3
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeProiecteID_OI_TIPURI_PROIECTE: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ID_OI_TIPURI_PROIECTE'
        Position.ColIndex = 4
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeProiecteID_PARINTE: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ID_PARINTE'
        Position.ColIndex = 2
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeProiecteDENUMIRE: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'DENUMIRE'
        Position.ColIndex = 0
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeProiecteDESCRIERE: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        Caption.AlignVert = vaCenter
        Caption.Text = 'Descriere'
        DataBinding.FieldName = 'DESCRIERE'
        Position.ColIndex = 1
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeProiecteSTARE: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'STARE'
        Position.ColIndex = 6
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeProiecteCOD_FUNCTIONAL: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'COD_FUNCTIONAL'
        Position.ColIndex = 5
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
    end
    object cxTreeFunctional: TcxDBTreeList
      Left = 378
      Top = 96
      Width = 250
      Height = 150
      Bands = <
        item
        end>
      DataController.DataSource = frmData.DTBGPlanFunctional
      DataController.ParentField = 'ID_PARINTE'
      DataController.KeyField = 'ID_BG_PLAN_FUNCTIONAL'
      OptionsBehavior.ExpandOnIncSearch = True
      OptionsBehavior.IncSearch = True
      OptionsBehavior.IncSearchItem = cxTreeFunctionalDESCRIERE
      OptionsData.Editing = False
      OptionsData.Deleting = False
      OptionsView.ColumnAutoWidth = True
      RootValue = -1
      TabOrder = 3
      Visible = False
      OnDblClick = cxTreeFunctionalDblClick
      OnKeyDown = cxTreeFunctionalKeyDown
      object cxTreeFunctionalCOD_FUNCTIONAL: TcxDBTreeListColumn
        Tag = -1
        Visible = False
        DataBinding.FieldName = 'COD_FUNCTIONAL'
        Position.ColIndex = 9
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalID_BG_TIPURI_BUGET: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ID_BG_TIPURI_BUGET'
        Position.ColIndex = 8
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalID_OI_UNITATI: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ID_OI_UNITATI'
        Position.ColIndex = 7
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalID_BG_PLAN_FUNCTIONAL: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ID_BG_PLAN_FUNCTIONAL'
        Position.ColIndex = 12
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalDENUMIRE: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'DENUMIRE'
        Position.ColIndex = 11
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalDESCRIERE: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        Caption.AlignVert = vaCenter
        Caption.Text = 'Cap. Subcap. Paragraf.'
        DataBinding.FieldName = 'COD_FUNCTIONAL'
        Position.ColIndex = 10
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
        OnGetDisplayText = cxTreeFunctionalDESCRIEREGetDisplayText
      end
      object cxTreeFunctionalNUMAR_RAND: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'NUMAR_RAND'
        Position.ColIndex = 6
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalID_PARINTE: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ID_PARINTE'
        Position.ColIndex = 2
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalCLASA: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'CLASA'
        Position.ColIndex = 1
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalCAPITOL: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'CAPITOL'
        Position.ColIndex = 0
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalESTE_LUCRARE: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ESTE_LUCRARE'
        Position.ColIndex = 5
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalTIP_BUGET: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'TIP_BUGET'
        Position.ColIndex = 4
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalESTE_STANDARD: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ESTE_STANDARD'
        Position.ColIndex = 3
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
    end
    object cxTreeEconomic: TcxDBTreeList
      Left = 538
      Top = 64
      Width = 250
      Height = 150
      Bands = <
        item
        end>
      DataController.DataSource = frmData.DTBGPlanEconomic
      DataController.ParentField = 'ID_PARINTE'
      DataController.KeyField = 'ID_BG_PLAN_ECONOMIC'
      OptionsBehavior.ExpandOnIncSearch = True
      OptionsBehavior.IncSearch = True
      OptionsBehavior.IncSearchItem = cxTreeEconomicDESCRIERE
      OptionsData.Editing = False
      OptionsData.Deleting = False
      OptionsView.ColumnAutoWidth = True
      RootValue = -1
      TabOrder = 4
      Visible = False
      OnDblClick = cxTreeFunctionalDblClick
      OnKeyDown = cxTreeFunctionalKeyDown
      object cxTreeEconomicID_BG_PLAN_ECONOMIC: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ID_BG_PLAN_ECONOMIC'
        Position.ColIndex = 2
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeEconomicCOD_ECONOMIC: TcxDBTreeListColumn
        Tag = -1
        Visible = False
        DataBinding.FieldName = 'COD_ECONOMIC'
        Position.ColIndex = 3
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeEconomicDENUMIRE: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'DENUMIRE'
        Position.ColIndex = 0
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeEconomicDESCRIERE: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        Caption.AlignVert = vaCenter
        Caption.Text = 'Titlu, Art. Aliniat'
        DataBinding.FieldName = 'DESCRIERE'
        Position.ColIndex = 1
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
        OnGetDisplayText = cxTreeEconomicDESCRIEREGetDisplayText
      end
      object cxTreeEconomicNUMAR_RAND: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'NUMAR_RAND'
        Position.ColIndex = 6
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeEconomicID_PARINTE: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ID_PARINTE'
        Position.ColIndex = 7
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeEconomicCLASA: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'CLASA'
        Position.ColIndex = 4
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeEconomicESTE_LOCAL: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ESTE_LOCAL'
        Position.ColIndex = 5
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
    end
    object cxTreeFunctionalComplet: TcxDBTreeList
      Left = 408
      Top = 260
      Width = 250
      Height = 150
      Bands = <
        item
        end>
      DataController.DataSource = frmData.DTBGPlanFunctionalComplet
      DataController.ParentField = 'ID_PARINTE'
      DataController.KeyField = 'ID_BG_PLAN_FUNCTIONAL'
      OptionsBehavior.ExpandOnIncSearch = True
      OptionsBehavior.IncSearch = True
      OptionsData.Editing = False
      OptionsData.Deleting = False
      OptionsView.ColumnAutoWidth = True
      RootValue = -1
      TabOrder = 5
      Visible = False
      OnCustomDrawDataCell = cxTreeFunctionalCompletCustomDrawDataCell
      OnDblClick = cxTreeFunctionalDblClick
      OnKeyDown = cxTreeFunctionalKeyDown
      object cxTreeFunctionalCompletCOD_FUNCTIONAL: TcxDBTreeListColumn
        Tag = -1
        Visible = False
        DataBinding.FieldName = 'COD_FUNCTIONAL'
        Position.ColIndex = 9
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalCompletID_BG_TIPURI_BUGET: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ID_BG_TIPURI_BUGET'
        Position.ColIndex = 8
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalCompletID_OI_UNITATI: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ID_OI_UNITATI'
        Position.ColIndex = 7
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalCompletID_BG_PLAN_FUNCTIONAL: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ID_BG_PLAN_FUNCTIONAL'
        Position.ColIndex = 11
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalCompletDENUMIRE: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'DENUMIRE'
        Position.ColIndex = 10
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalCompletDESCRIERE: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        Caption.Text = 'Cap. Subcap. Paragraf.'
        DataBinding.FieldName = 'cod_ecran'
        Position.ColIndex = 13
        Position.RowIndex = 0
        Position.BandIndex = 0
        SortOrder = soAscending
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
        OnGetDisplayText = cxTreeFunctionalCompletDESCRIEREGetDisplayText
      end
      object cxTreeFunctionalCompletNUMAR_RAND: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'NUMAR_RAND'
        Position.ColIndex = 6
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalCompletID_PARINTE: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ID_PARINTE'
        Position.ColIndex = 2
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalCompletCLASA: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'CLASA'
        Position.ColIndex = 1
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalCompletCAPITOL: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'CAPITOL'
        Position.ColIndex = 0
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalCompletESTE_LUCRARE: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ESTE_LUCRARE'
        Position.ColIndex = 5
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalCompletTIP_BUGET: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'TIP_BUGET'
        Position.ColIndex = 4
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalCompletESTE_STANDARD: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ESTE_STANDARD'
        Position.ColIndex = 3
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalCompletID_OI_PROIECTE: TcxDBTreeListColumn
        Visible = False
        DataBinding.FieldName = 'ID_OI_PROIECTE'
        Position.ColIndex = 12
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object cxTreeFunctionalCompletCOD_ECRAN: TcxDBTreeListColumn
        Visible = False
        Caption.Text = 'Cod'
        DataBinding.FieldName = 'cod_ecran'
        Position.ColIndex = 14
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
    end
  end
  object pnBottom: TPanel
    Left = 0
    Top = 392
    Width = 914
    Height = 198
    Align = alBottom
    TabOrder = 2
  end
  object qryFisaBuget: TZQuery
    Connection = frmData.dbContabilitate
    AfterOpen = qryFisaBugetAfterOpen
    OnNewRecord = qryFisaBugetNewRecord
    SQL.Strings = (
      
        'exec spBugetFisaOperare :codFunctional, :codEconomic, :idUnitate' +
        ', :idProiecte'
      '')
    Params = <
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'codFunctional'
        ParamType = ptUnknown
        Size = 100
        Value = '51.02'
      end
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'codEconomic'
        ParamType = ptUnknown
        Size = 100
        Value = '10.01.01'
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'idUnitate'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'idProiecte'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 40
    Top = 456
    ParamData = <
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'codFunctional'
        ParamType = ptUnknown
        Size = 100
        Value = '51.02'
      end
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'codEconomic'
        ParamType = ptUnknown
        Size = 100
        Value = '10.01.01'
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'idUnitate'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'idProiecte'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object MemLista: TdxMemData
    Indexes = <>
    SortOptions = []
    BeforeDelete = MemListaBeforeDelete
    OnNewRecord = MemListaNewRecord
    Left = 8
    Top = 424
  end
  object DTLista: TDataSource
    DataSet = MemLista
    Left = 8
    Top = 456
  end
end
