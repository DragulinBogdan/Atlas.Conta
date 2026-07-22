object frmSelStock: TfrmSelStock
  Left = 352
  Top = 194
  ActiveControl = grStock
  Caption = 'Selectie materiale din stock'
  ClientHeight = 568
  ClientWidth = 1005
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  PixelsPerInch = 96
  TextHeight = 13
  object spliterV: TcxSplitter
    Left = 0
    Top = 290
    Width = 1005
    Height = 8
    Cursor = crVSplit
    HotZoneClassName = 'TcxMediaPlayer9Style'
    HotZone.SizePercent = 59
    AlignSplitter = salBottom
    MinSize = 1
    Control = pnBottom
    OnAfterOpen = spliterVAfterOpen
    ExplicitTop = 328
  end
  object pnTop: TPanel
    Left = 0
    Top = 0
    Width = 1005
    Height = 25
    Align = alTop
    BevelInner = bvRaised
    BevelOuter = bvLowered
    TabOrder = 1
    object ChkShowAllReady: TcxCheckBox
      Left = 3
      Top = 0
      Caption = 'Materialele deja preluate'
      TabOrder = 0
      OnClick = ChkShowAllReadyClick
    end
    object ChkShowNegative: TcxCheckBox
      Left = 242
      Top = 0
      Caption = 'Arata stoc negativ'
      TabOrder = 1
      OnClick = ChkShowAllReadyClick
    end
    object ChkShowData: TcxCheckBox
      Left = 142
      Top = 0
      Caption = 'Intrari anterioare'
      TabOrder = 2
      OnClick = ChkShowAllReadyClick
    end
    object chkStockLazi: TcxCheckBox
      Left = 352
      Top = 0
      Caption = 'Iesiri ulterioare'
      State = cbsChecked
      TabOrder = 3
      OnClick = ChkShowAllReadyClick
    end
    object edtChangeDataStoc: TcxCheckBox
      Left = 500
      Top = 0
      Caption = 'Schimba Data Stoc'
      TabOrder = 4
      OnClick = edtChangeDataStocClick
    end
    object edtDataStock: TcxDateEdit
      Left = 613
      Top = 0
      Enabled = False
      Properties.InputKind = ikMask
      Properties.OnChange = edtDataStockPropertiesChange
      TabOrder = 5
      Width = 134
    end
  end
  object Panel1: TPanel
    Left = 0
    Top = 480
    Width = 1005
    Height = 88
    Align = alBottom
    BevelInner = bvRaised
    BevelOuter = bvLowered
    TabOrder = 0
    DesignSize = (
      1005
      88)
    object cxDBLabel1: TcxDBLabel
      Left = 64
      Top = 8
      AutoSize = True
      DataBinding.DataField = 'CODMAT'
      DataBinding.DataSource = DTStock
      Properties.Depth = 1
      Properties.LabelEffect = cxleCool
      Properties.LabelStyle = cxlsOutLine
    end
    object cxLabel1: TcxLabel
      Left = 16
      Top = 8
      Caption = 'CodMat'
      Properties.Depth = 1
      Properties.LabelEffect = cxleCool
    end
    object BtnOk: TcxButton
      Left = 811
      Top = 6
      Width = 75
      Height = 25
      Anchors = [akRight, akBottom]
      Caption = 'Ok'
      Default = True
      TabOrder = 0
      OnClick = BtnOkClick
    end
    object BtnCancel: TcxButton
      Left = 892
      Top = 6
      Width = 75
      Height = 25
      Anchors = [akRight, akBottom]
      Cancel = True
      Caption = 'Abandon'
      ModalResult = 2
      TabOrder = 1
    end
  end
  object Panel3: TPanel
    Left = 0
    Top = 25
    Width = 1005
    Height = 265
    Align = alClient
    BevelInner = bvRaised
    BevelOuter = bvLowered
    TabOrder = 2
    ExplicitHeight = 303
    object grStock: TcxGrid
      Left = 2
      Top = 22
      Width = 1001
      Height = 241
      Align = alClient
      TabOrder = 0
      ExplicitHeight = 279
      object GridStock: TcxGridDBBandedTableView
        OnKeyDown = GridStockKeyDown
        Navigator.Buttons.CustomButtons = <>
        Navigator.Visible = True
        ScrollbarAnnotations.CustomAnnotations = <>
        OnCustomDrawCell = GridStockCustomDrawCell
        OnEditKeyDown = GridStockEditKeyDown
        OnFocusedRecordChanged = GridStockFocusedRecordChanged
        DataController.DataSource = DTStock
        DataController.KeyFieldNames = 'RecId'
        DataController.Summary.DefaultGroupSummaryItems = <>
        DataController.Summary.FooterSummaryItems = <>
        DataController.Summary.SummaryGroups = <>
        FilterRow.InfoText = 'Apasa aici pentru a defini filtrul'
        OptionsBehavior.AlwaysShowEditor = True
        OptionsBehavior.DragHighlighting = False
        OptionsBehavior.DragOpening = False
        OptionsBehavior.DragScrolling = False
        OptionsBehavior.FocusCellOnTab = True
        OptionsBehavior.IncSearch = True
        OptionsBehavior.IncSearchItem = GridStockDENMAT
        OptionsBehavior.ExpandMasterRowOnDblClick = False
        OptionsCustomize.ColumnsQuickCustomization = True
        OptionsCustomize.BandsQuickCustomization = True
        OptionsData.CancelOnExit = False
        OptionsData.Deleting = False
        OptionsData.DeletingConfirmation = False
        OptionsData.Inserting = False
        OptionsSelection.MultiSelect = True
        OptionsView.ColumnAutoWidth = True
        OptionsView.Footer = True
        OptionsView.GroupByBox = False
        OptionsView.Indicator = True
        Bands = <
          item
            Caption = 'Detalii Material'
            FixedKind = fkLeft
            Width = 756
          end
          item
            Caption = 'Detalii Stock'
            Width = 504
          end>
        object GridStockID_GEST_TIP_STOCK: TcxGridDBBandedColumn
          DataBinding.FieldName = 'ID_GEST_TIP_STOCK'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object GridStockID_STOCK_PREDATOR: TcxGridDBBandedColumn
          DataBinding.FieldName = 'ID_STOCK_PREDATOR'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 1
          Position.RowIndex = 0
        end
        object GridStockID_STOCK_PRIMITOR: TcxGridDBBandedColumn
          DataBinding.FieldName = 'ID_STOCK_PRIMITOR'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 2
          Position.RowIndex = 0
        end
        object GridStockSELECTAT: TcxGridDBBandedColumn
          Caption = 'Sel'
          DataBinding.FieldName = 'SELECTAT'
          PropertiesClassName = 'TcxCheckBoxProperties'
          Properties.FullFocusRect = True
          Properties.NullStyle = nssUnchecked
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Width = 37
          Position.BandIndex = 0
          Position.ColIndex = 3
          Position.RowIndex = 0
        end
        object GridStockCANTITATE_SELECTATA: TcxGridDBBandedColumn
          Caption = 'Selectat'
          DataBinding.FieldName = 'CANTITATE_SELECTATA'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DisplayFormat = ',0.00;-,0.00'
          HeaderAlignmentHorz = taCenter
          Options.Filtering = False
          Options.IncSearch = False
          Width = 60
          Position.BandIndex = 0
          Position.ColIndex = 4
          Position.RowIndex = 0
        end
        object GridStockPRODUS: TcxGridDBBandedColumn
          Caption = 'Categ Prod.'
          DataBinding.FieldName = 'PRODUS'
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Items = <>
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Width = 62
          Position.BandIndex = 0
          Position.ColIndex = 7
          Position.RowIndex = 0
        end
        object GridStockTIP_STOCK: TcxGridDBBandedColumn
          DataBinding.FieldName = 'TIP_STOCK'
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Items = <
            item
              Description = 'Stock Predator'
              ImageIndex = 0
              Value = 1
            end
            item
              Description = 'Stock Primitor'
              Value = 2
            end
            item
              Description = 'Stock Predator/Primitor'
              Value = 3
            end>
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 1
          Position.ColIndex = 2
          Position.RowIndex = 0
        end
        object GridStockCANTITATE: TcxGridDBBandedColumn
          Caption = 'Stock'
          DataBinding.FieldName = 'CANTITATE'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DisplayFormat = ',0.00;-,0.00'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Options.Filtering = False
          Width = 88
          Position.BandIndex = 0
          Position.ColIndex = 5
          Position.RowIndex = 0
        end
        object GridStockCANT_PREDATOR_ZI: TcxGridDBBandedColumn
          Caption = 'Stock Zi'
          DataBinding.FieldName = 'CANT_PREDATOR_ZI'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DisplayFormat = ',0.00;-,0.00'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Options.Filtering = False
          Width = 75
          Position.BandIndex = 0
          Position.ColIndex = 6
          Position.RowIndex = 0
        end
        object GridStockCANT_PREDATOR: TcxGridDBBandedColumn
          DataBinding.FieldName = 'CANT_PREDATOR'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DisplayFormat = ',0.00;-,0.00'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 1
          Position.ColIndex = 3
          Position.RowIndex = 0
        end
        object GridStockCANT_PRIMITOR: TcxGridDBBandedColumn
          DataBinding.FieldName = 'CANT_PRIMITOR'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DisplayFormat = ',0.00;-,0.00'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 1
          Position.ColIndex = 5
          Position.RowIndex = 0
        end
        object GridStockCONT: TcxGridDBBandedColumn
          Caption = 'Cont Mat.'
          DataBinding.FieldName = 'Cont'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 1
          Position.ColIndex = 6
          Position.RowIndex = 0
        end
        object GridStockID_PREDATOR: TcxGridDBBandedColumn
          DataBinding.FieldName = 'ID_PREDATOR'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 1
          Position.ColIndex = 7
          Position.RowIndex = 0
        end
        object GridStockID_PRIMITOR: TcxGridDBBandedColumn
          DataBinding.FieldName = 'ID_PRIMITOR'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 1
          Position.ColIndex = 8
          Position.RowIndex = 0
        end
        object GridStockDATACOD: TcxGridDBBandedColumn
          Caption = 'Data Cod'
          DataBinding.FieldName = 'DATACOD'
          PropertiesClassName = 'TcxDateEditProperties'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 1
          Position.ColIndex = 9
          Position.RowIndex = 0
        end
        object GridStockNR_DOCUM: TcxGridDBBandedColumn
          Caption = 'Nr. Doc'
          DataBinding.FieldName = 'NR_DOCUM'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Width = 60
          Position.BandIndex = 1
          Position.ColIndex = 10
          Position.RowIndex = 0
        end
        object GridStockCODMAT: TcxGridDBBandedColumn
          Caption = 'CodMat'
          DataBinding.FieldName = 'CODMAT'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 1
          Position.ColIndex = 11
          Position.RowIndex = 0
        end
        object GridStockID_ANGAJAMENTE_DEFALCARE: TcxGridDBBandedColumn
          DataBinding.FieldName = 'ID_ANGAJAMENTE_DEFALCARE'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 10
          Position.RowIndex = 0
        end
        object GridStockID_INITIAL: TcxGridDBBandedColumn
          DataBinding.FieldName = 'ID_INITIAL'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 11
          Position.RowIndex = 0
        end
        object GridStockID_UTILIZATORI: TcxGridDBBandedColumn
          DataBinding.FieldName = 'ID_UTILIZATORI'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 12
          Position.RowIndex = 0
        end
        object GridStockPRET_UNITAR: TcxGridDBBandedColumn
          DataBinding.FieldName = 'PRET_UNITAR'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 1
          Position.ColIndex = 12
          Position.RowIndex = 0
        end
        object GridStockPRET_RECEPTIE: TcxGridDBBandedColumn
          Caption = 'Pret Rec.'
          DataBinding.FieldName = 'PRET_RECEPTIE'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 1
          Position.ColIndex = 0
          Position.RowIndex = 0
        end
        object GridStockCOTA_TVA: TcxGridDBBandedColumn
          Caption = '% TVA'
          DataBinding.FieldName = 'COTA_TVA'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Width = 60
          Position.BandIndex = 1
          Position.ColIndex = 1
          Position.RowIndex = 0
        end
        object GridStockPRET_RECEPTIE_TVA: TcxGridDBBandedColumn
          Caption = 'Pret Rec. TVA'
          DataBinding.FieldName = 'PRET_RECEPTIE_TVA'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 1
          Position.ColIndex = 4
          Position.RowIndex = 0
        end
        object GridStockDENMAT: TcxGridDBBandedColumn
          Caption = 'Denumire'
          DataBinding.FieldName = 'DENMAT'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          SortIndex = 1
          SortOrder = soAscending
          Width = 152
          Position.BandIndex = 0
          Position.ColIndex = 9
          Position.RowIndex = 0
        end
        object GridStockTIPMAT: TcxGridDBBandedColumn
          Caption = 'Grupa'
          DataBinding.FieldName = 'TIPMAT'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Width = 49
          Position.BandIndex = 0
          Position.ColIndex = 8
          Position.RowIndex = 0
        end
        object GridStockUM: TcxGridDBBandedColumn
          DataBinding.FieldName = 'UM'
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Width = 38
          Position.BandIndex = 0
          Position.ColIndex = 13
          Position.RowIndex = 0
        end
        object GridStockLOHN: TcxGridDBBandedColumn
          DataBinding.FieldName = 'LOHN'
          PropertiesClassName = 'TcxCheckBoxProperties'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 14
          Position.RowIndex = 0
        end
        object GridStockDATA_COD: TcxGridDBBandedColumn
          DataBinding.FieldName = 'DATA_COD'
          PropertiesClassName = 'TcxDateEditProperties'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 15
          Position.RowIndex = 0
        end
        object GridStockDATA_EXPIRARE: TcxGridDBBandedColumn
          Caption = 'Data Expirare'
          DataBinding.FieldName = 'DATA_EXPIRARE'
          PropertiesClassName = 'TcxDateEditProperties'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 16
          Position.RowIndex = 0
        end
        object GridStockTVA: TcxGridDBBandedColumn
          DataBinding.FieldName = 'TVA'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 17
          Position.RowIndex = 0
        end
        object GridStockID_GEST_SUMATOR: TcxGridDBBandedColumn
          DataBinding.FieldName = 'ID_GEST_SUMATOR'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 18
          Position.RowIndex = 0
        end
        object GridStockLOT_FABRICATIE: TcxGridDBBandedColumn
          Caption = 'Lot Fabr'
          DataBinding.FieldName = 'LOT_FABRICATIE'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 19
          Position.RowIndex = 0
        end
        object GridStockUM_SUPLIMENTARA: TcxGridDBBandedColumn
          Caption = 'UM Supl.'
          DataBinding.FieldName = 'UM_SUPLIMENTARA'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 20
          Position.RowIndex = 0
        end
        object GridStockCONVERSIE_UM: TcxGridDBBandedColumn
          Caption = 'Conv. UM'
          DataBinding.FieldName = 'CONVERSIE_UM'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 21
          Position.RowIndex = 0
        end
        object GridStockPRET_RECEPTIE_VALUTA: TcxGridDBBandedColumn
          Caption = 'Pret Rec. Valuta'
          DataBinding.FieldName = 'PRET_RECEPTIE_VALUTA'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 22
          Position.RowIndex = 0
        end
        object GridStockCOD_TARIF_VAMAL: TcxGridDBBandedColumn
          Caption = 'Tarif Vama'
          DataBinding.FieldName = 'COD_TARIF_VAMAL'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 23
          Position.RowIndex = 0
        end
        object GridStockTVA_AMANAT: TcxGridDBBandedColumn
          Caption = 'TVA Amanat'
          DataBinding.FieldName = 'TVA_AMANAT'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 24
          Position.RowIndex = 0
        end
        object GridStockADAOS: TcxGridDBBandedColumn
          Caption = 'Adaos'
          DataBinding.FieldName = 'ADAOS'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 25
          Position.RowIndex = 0
        end
        object GridStockADAOS_IMPUS: TcxGridDBBandedColumn
          Caption = 'Adaos Imp'
          DataBinding.FieldName = 'ADAOS_IMPUS'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 26
          Position.RowIndex = 0
        end
        object GridStockCATEGORIE_GRUPARE: TcxGridDBBandedColumn
          Caption = 'Cat. Grupare'
          DataBinding.FieldName = 'CATEGORIE_GRUPARE'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 27
          Position.RowIndex = 0
        end
        object GridStockTIP_VALUTA_RECEPTIE: TcxGridDBBandedColumn
          Caption = 'Tip Valuta'
          DataBinding.FieldName = 'TIP_VALUTA_RECEPTIE'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 28
          Position.RowIndex = 0
        end
        object GridStockCOTA_ADAOS: TcxGridDBBandedColumn
          Caption = '% Adaos'
          DataBinding.FieldName = 'COTA_ADAOS'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 29
          Position.RowIndex = 0
        end
        object GridStockCOTA_ADAOS_IMPUS: TcxGridDBBandedColumn
          Caption = '% Adaos Impus'
          DataBinding.FieldName = 'COTA_ADAOS_IMPUS'
          Visible = False
          HeaderAlignmentHorz = taCenter
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 30
          Position.RowIndex = 0
        end
        object GridStockRecId: TcxGridDBBandedColumn
          DataBinding.FieldName = 'RecId'
          Visible = False
          Position.BandIndex = 0
          Position.ColIndex = 31
          Position.RowIndex = 0
        end
        object GridStockSEMN_CANTITATE: TcxGridDBBandedColumn
          Caption = 'Semn Cantitate'
          DataBinding.FieldName = 'SEMN_CANTITATE'
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Items = <
            item
              Description = 'Nomenclator'
              ImageIndex = 0
              Value = 0
            end
            item
              Description = 'Retur(-)'
              Value = -1
            end
            item
              Description = 'Adaugare(+)'
              Value = 1
            end>
          Visible = False
          Options.Editing = False
          Position.BandIndex = 0
          Position.ColIndex = 32
          Position.RowIndex = 0
        end
        object GridStockOrderCol: TcxGridDBBandedColumn
          DataBinding.FieldName = 'OrderCol'
          SortIndex = 0
          SortOrder = soAscending
          Position.BandIndex = 1
          Position.ColIndex = 13
          Position.RowIndex = 0
        end
      end
      object grLevel: TcxGridLevel
        GridView = GridStock
      end
    end
    object tabTipStock: TcxTabControl
      Left = 2
      Top = 2
      Width = 1001
      Height = 20
      Align = alTop
      TabOrder = 1
      Properties.CustomButtons.Buttons = <>
      Properties.MultiLine = True
      Properties.Style = 9
      Properties.TabHeight = 19
      Properties.TabSlants.Positions = [spLeft, spRight]
      LookAndFeel.Kind = lfOffice11
      TabSlants.Positions = [spLeft, spRight]
      OnChange = tabTipStockChange
      OnDrawTabEx = tabTipStockDrawTabEx
      ClientRectBottom = 20
      ClientRectRight = 1001
      ClientRectTop = 0
    end
  end
  object pnBottom: TPanel
    Left = 0
    Top = 298
    Width = 1005
    Height = 182
    Align = alBottom
    BevelInner = bvRaised
    BevelOuter = bvLowered
    TabOrder = 3
    ExplicitTop = 336
    object pageDesc: TcxPageControl
      Left = 2
      Top = 2
      Width = 1001
      Height = 178
      Align = alClient
      TabOrder = 0
      Properties.ActivePage = tabFisaCodMat
      Properties.CustomButtons.Buttons = <>
      OnChange = pageDescChange
      ClientRectBottom = 178
      ClientRectRight = 1001
      ClientRectTop = 24
      object tabFisaCodMat: TcxTabSheet
        Caption = 'Fisa Magazie'
        ImageIndex = 0
        object GridIstoricMaterial: TdxDBGrid
          Left = 0
          Top = 0
          Width = 1001
          Height = 154
          SearchType = stStart
          Bands = <
            item
            end>
          DefaultLayout = True
          HeaderPanelRowCount = 1
          KeyField = 'ID'
          ShowSummaryFooter = True
          SummaryGroups = <>
          SummarySeparator = ', '
          Align = alClient
          TabOrder = 0
          DataSource = dtFisaMaterial
          Filter.Criteria = {00000000}
          LookAndFeel = lfFlat
          OptionsBehavior = [edgoAutoSearch, edgoAutoSort, edgoEnterShowEditor]
          OptionsDB = [edgoCanNavigation, edgoLoadAllRecords, edgoUseBookmarks]
          OptionsView = [edgoAutoWidth, edgoBandHeaderWidth, edgoInvertSelect, edgoUseBitmap]
          ShowRowFooter = True
          object GridIstoricMaterialPREDATOR: TdxDBGridMaskColumn
            Caption = 'Predator'
            HeaderAlignment = taCenter
            Width = 93
            BandIndex = 0
            RowIndex = 0
            FieldName = 'PREDATOR'
          end
          object GridIstoricMaterialPRIMITOR: TdxDBGridMaskColumn
            Caption = 'Primitor'
            HeaderAlignment = taCenter
            Width = 121
            BandIndex = 0
            RowIndex = 0
            FieldName = 'PRIMITOR'
          end
          object GridIstoricMaterialCOD_DOCUM: TdxDBGridMaskColumn
            Caption = 'Doc'
            HeaderAlignment = taCenter
            Width = 45
            BandIndex = 0
            RowIndex = 0
            FieldName = 'COD_DOCUM'
          end
          object GridIstoricMaterialNR_DOCUM: TdxDBGridMaskColumn
            Caption = 'Nr'
            HeaderAlignment = taCenter
            Width = 60
            BandIndex = 0
            RowIndex = 0
            FieldName = 'NR_DOCUM'
          end
          object GridIstoricMaterialDATA_DOCUM: TdxDBGridDateColumn
            Caption = 'Data'
            HeaderAlignment = taCenter
            Width = 60
            BandIndex = 0
            RowIndex = 0
            FieldName = 'DATA_DOCUM'
          end
          object GridIstoricMaterialSEMN: TdxDBGridImageColumn
            Alignment = taLeftJustify
            Caption = 'Semn'
            HeaderAlignment = taCenter
            MinWidth = 16
            Width = 52
            BandIndex = 0
            RowIndex = 0
            FieldName = 'SEMN'
            DefaultImages = False
            Descriptions.Strings = (
              'Scazut'
              'Ignorat'
              'Adunat')
            Images = frmData.SemnImagini
            ImageIndexes.Strings = (
              '1'
              '-1'
              '0')
            Values.Strings = (
              '-1'
              '0'
              '1')
          end
          object GridIstoricMaterialCANTITATE_BEFORE: TdxDBGridMaskColumn
            Caption = 'Inainte'
            HeaderAlignment = taCenter
            Width = 56
            BandIndex = 0
            RowIndex = 0
            FieldName = 'CANTITATE_BEFORE'
          end
          object GridIstoricMaterialCANTITATE: TdxDBGridMaskColumn
            Caption = 'Cantitate'
            HeaderAlignment = taCenter
            Width = 56
            BandIndex = 0
            RowIndex = 0
            FieldName = 'CANTITATE'
          end
          object GridIstoricMaterialCANTITATE_AFTER: TdxDBGridMaskColumn
            Caption = 'Dupa'
            HeaderAlignment = taCenter
            Width = 57
            BandIndex = 0
            RowIndex = 0
            FieldName = 'CANTITATE_AFTER'
          end
          object GridIstoricMaterialTIP_MATERIAL: TdxDBGridMaskColumn
            Caption = 'Tip Mat'
            HeaderAlignment = taCenter
            Width = 74
            BandIndex = 0
            RowIndex = 0
            FieldName = 'TIP_MATERIAL'
          end
          object GridIstoricMaterialPRET_UNITAR: TdxDBGridCurrencyColumn
            Caption = 'Pret Unitar'
            HeaderAlignment = taCenter
            Width = 65
            BandIndex = 0
            RowIndex = 0
            FieldName = 'PRET_UNITAR'
            Nullable = False
          end
          object GridIstoricMaterialVALOARE: TdxDBGridCurrencyColumn
            Caption = 'Valoare'
            HeaderAlignment = taCenter
            Width = 44
            BandIndex = 0
            RowIndex = 0
            FieldName = 'VALOARE'
            Nullable = False
          end
          object GridIstoricMaterialOPERATOR: TdxDBGridMaskColumn
            Caption = 'Operator'
            HeaderAlignment = taCenter
            Width = 73
            BandIndex = 0
            RowIndex = 0
            FieldName = 'OPERATOR'
          end
        end
      end
      object tabStockAll: TcxTabSheet
        Caption = 'Stock Unitate'
        ImageIndex = 1
        object gridStockAll: TdxDBGrid
          Left = 0
          Top = 0
          Width = 1001
          Height = 154
          SearchType = stStart
          Bands = <
            item
            end>
          DefaultLayout = True
          HeaderPanelRowCount = 1
          KeyField = 'id'
          ShowSummaryFooter = True
          SummaryGroups = <>
          SummarySeparator = ', '
          Align = alClient
          TabOrder = 0
          DataSource = dtStockAll
          Filter.Criteria = {00000000}
          LookAndFeel = lfFlat
          OptionsBehavior = [edgoAutoSearch, edgoAutoSort, edgoEnterShowEditor]
          OptionsDB = [edgoCanNavigation, edgoLoadAllRecords, edgoUseBookmarks]
          OptionsView = [edgoAutoWidth, edgoBandHeaderWidth, edgoInvertSelect, edgoUseBitmap]
          ShowRowFooter = True
          object gridStockAllid: TdxDBGridMaskColumn
            HeaderAlignment = taCenter
            Visible = False
            BandIndex = 0
            RowIndex = 0
            FieldName = 'id'
          end
          object gridStockAllgestiune: TdxDBGridColumn
            Caption = 'Gestiune'
            HeaderAlignment = taCenter
            BandIndex = 0
            RowIndex = 0
            FieldName = 'gestiune'
          end
          object gridStockAllstock: TdxDBGridCurrencyColumn
            Caption = 'Stock'
            HeaderAlignment = taCenter
            BandIndex = 0
            RowIndex = 0
            FieldName = 'stock'
            Nullable = False
          end
          object gridStockAllpret_receptie: TdxDBGridCurrencyColumn
            Caption = 'Pret Receptie'
            HeaderAlignment = taCenter
            BandIndex = 0
            RowIndex = 0
            FieldName = 'pret_receptie'
            Nullable = False
          end
          object gridStockAllpret_receptie_tva: TdxDBGridCurrencyColumn
            Caption = 'Pret Receptie TVA'
            HeaderAlignment = taCenter
            BandIndex = 0
            RowIndex = 0
            FieldName = 'pret_receptie_tva'
            Nullable = False
          end
          object gridStockAllstockValoric: TdxDBGridCurrencyColumn
            Caption = 'Stock Valoric'
            HeaderAlignment = taCenter
            BandIndex = 0
            RowIndex = 0
            FieldName = 'stockValoric'
            Nullable = False
          end
        end
      end
      object tabStockSum: TcxTabSheet
        Caption = 'Stock Sumator'
        ImageIndex = 2
        object GridStockSumator: TdxDBGrid
          Left = 0
          Top = 0
          Width = 1001
          Height = 154
          SearchType = stStart
          Bands = <
            item
            end>
          DefaultLayout = True
          HeaderPanelRowCount = 1
          KeyField = 'id'
          ShowSummaryFooter = True
          SummaryGroups = <>
          SummarySeparator = ', '
          Align = alClient
          TabOrder = 0
          DataSource = dtStockSum
          Filter.Criteria = {00000000}
          LookAndFeel = lfFlat
          OptionsBehavior = [edgoAutoSearch, edgoAutoSort, edgoEnterShowEditor]
          OptionsDB = [edgoCanNavigation, edgoLoadAllRecords, edgoUseBookmarks]
          OptionsView = [edgoAutoWidth, edgoBandHeaderWidth, edgoInvertSelect, edgoUseBitmap]
          ShowRowFooter = True
          object AtsDBGridMaskColumn1: TdxDBGridMaskColumn
            HeaderAlignment = taCenter
            Visible = False
            BandIndex = 0
            RowIndex = 0
            FieldName = 'id'
          end
          object AtsDBGridColumn1: TdxDBGridColumn
            Caption = 'Gestiune'
            HeaderAlignment = taCenter
            BandIndex = 0
            RowIndex = 0
            FieldName = 'gestiune'
          end
          object AtsDBGridCurrencyColumn1: TdxDBGridCurrencyColumn
            Caption = 'Stock'
            HeaderAlignment = taCenter
            BandIndex = 0
            RowIndex = 0
            FieldName = 'stock'
            Nullable = False
          end
          object AtsDBGridCurrencyColumn2: TdxDBGridCurrencyColumn
            Caption = 'Pret Receptie'
            HeaderAlignment = taCenter
            BandIndex = 0
            RowIndex = 0
            FieldName = 'pret_receptie'
            Nullable = False
          end
          object AtsDBGridCurrencyColumn3: TdxDBGridCurrencyColumn
            Caption = 'Pret Receptie TVA'
            HeaderAlignment = taCenter
            BandIndex = 0
            RowIndex = 0
            FieldName = 'pret_receptie_tva'
            Nullable = False
          end
          object AtsDBGridCurrencyColumn4: TdxDBGridCurrencyColumn
            Caption = 'Stock Valoric'
            HeaderAlignment = taCenter
            BandIndex = 0
            RowIndex = 0
            FieldName = 'stockValoric'
            Nullable = False
          end
        end
      end
      object tabFisaMaterialSumator: TcxTabSheet
        Caption = 'Fisa Magazie Sumator'
        ImageIndex = 3
        object GridFisaMagSum: TdxDBGrid
          Left = 0
          Top = 0
          Width = 1001
          Height = 154
          SearchType = stStart
          Bands = <
            item
            end>
          DefaultLayout = True
          HeaderPanelRowCount = 1
          KeyField = 'ID'
          ShowSummaryFooter = True
          SummaryGroups = <>
          SummarySeparator = ', '
          Align = alClient
          TabOrder = 0
          DataSource = dtFisaMaterial
          Filter.Criteria = {00000000}
          LookAndFeel = lfFlat
          OptionsBehavior = [edgoAutoSearch, edgoAutoSort, edgoEnterShowEditor]
          OptionsDB = [edgoCanNavigation, edgoLoadAllRecords, edgoUseBookmarks]
          OptionsView = [edgoAutoWidth, edgoBandHeaderWidth, edgoInvertSelect, edgoUseBitmap]
          ShowRowFooter = True
          object AtsDBGridMaskColumn2: TdxDBGridMaskColumn
            Caption = 'Predator'
            HeaderAlignment = taCenter
            Width = 93
            BandIndex = 0
            RowIndex = 0
            FieldName = 'PREDATOR'
          end
          object AtsDBGridMaskColumn3: TdxDBGridMaskColumn
            Caption = 'Primitor'
            HeaderAlignment = taCenter
            Width = 121
            BandIndex = 0
            RowIndex = 0
            FieldName = 'PRIMITOR'
          end
          object AtsDBGridMaskColumn4: TdxDBGridMaskColumn
            Caption = 'Doc'
            HeaderAlignment = taCenter
            Width = 45
            BandIndex = 0
            RowIndex = 0
            FieldName = 'COD_DOCUM'
          end
          object AtsDBGridMaskColumn5: TdxDBGridMaskColumn
            Caption = 'Nr'
            HeaderAlignment = taCenter
            Width = 60
            BandIndex = 0
            RowIndex = 0
            FieldName = 'NR_DOCUM'
          end
          object AtsDBGridDateColumn1: TdxDBGridDateColumn
            Caption = 'Data'
            HeaderAlignment = taCenter
            Width = 60
            BandIndex = 0
            RowIndex = 0
            FieldName = 'DATA_DOCUM'
          end
          object AtsDBGridImageColumn1: TdxDBGridImageColumn
            Alignment = taLeftJustify
            Caption = 'Semn'
            HeaderAlignment = taCenter
            MinWidth = 16
            Width = 52
            BandIndex = 0
            RowIndex = 0
            FieldName = 'SEMN'
            DefaultImages = False
            Descriptions.Strings = (
              'Scazut'
              'Ignorat'
              'Adunat')
            Images = frmData.SemnImagini
            ImageIndexes.Strings = (
              '1'
              '-1'
              '0')
            Values.Strings = (
              '-1'
              '0'
              '1')
          end
          object AtsDBGridMaskColumn8: TdxDBGridMaskColumn
            Caption = 'Inainte'
            HeaderAlignment = taCenter
            Width = 56
            BandIndex = 0
            RowIndex = 0
            FieldName = 'CANTITATE_BEFORE'
          end
          object AtsDBGridMaskColumn6: TdxDBGridMaskColumn
            Caption = 'Cantitate'
            HeaderAlignment = taCenter
            Width = 56
            BandIndex = 0
            RowIndex = 0
            FieldName = 'CANTITATE'
          end
          object AtsDBGridMaskColumn9: TdxDBGridMaskColumn
            Caption = 'Dupa'
            HeaderAlignment = taCenter
            Width = 57
            BandIndex = 0
            RowIndex = 0
            FieldName = 'CANTITATE_AFTER'
          end
          object AtsDBGridMaskColumn7: TdxDBGridMaskColumn
            Caption = 'Tip Mat'
            HeaderAlignment = taCenter
            Width = 74
            BandIndex = 0
            RowIndex = 0
            FieldName = 'TIP_MATERIAL'
          end
          object AtsDBGridCurrencyColumn5: TdxDBGridCurrencyColumn
            Caption = 'Pret Unitar'
            HeaderAlignment = taCenter
            Width = 65
            BandIndex = 0
            RowIndex = 0
            FieldName = 'PRET_UNITAR'
            Nullable = False
          end
          object AtsDBGridCurrencyColumn6: TdxDBGridCurrencyColumn
            Caption = 'Valoare'
            HeaderAlignment = taCenter
            Width = 44
            BandIndex = 0
            RowIndex = 0
            FieldName = 'VALOARE'
            Nullable = False
          end
          object AtsDBGridMaskColumn10: TdxDBGridMaskColumn
            Caption = 'Operator'
            HeaderAlignment = taCenter
            Width = 73
            BandIndex = 0
            RowIndex = 0
            FieldName = 'OPERATOR'
          end
        end
      end
      object tabAltMat: TcxTabSheet
        Caption = 'Alte Materiale pe acelasi sumator'
        ImageIndex = 4
        object GridAltSumator: TdxDBGrid
          Left = 0
          Top = 0
          Width = 1001
          Height = 154
          SearchType = stStart
          Bands = <
            item
            end>
          DefaultLayout = True
          HeaderPanelRowCount = 1
          KeyField = 'id'
          ShowSummaryFooter = True
          SummaryGroups = <>
          SummarySeparator = ', '
          Align = alClient
          TabOrder = 0
          DataSource = DTAcelasiSumator
          Filter.Criteria = {00000000}
          LookAndFeel = lfFlat
          OptionsBehavior = [edgoAutoSearch, edgoAutoSort, edgoEnterShowEditor]
          OptionsDB = [edgoCanNavigation, edgoLoadAllRecords, edgoUseBookmarks]
          OptionsView = [edgoAutoWidth, edgoBandHeaderWidth, edgoInvertSelect, edgoUseBitmap]
          ShowRowFooter = True
          object GridAltSumatorcodmat: TdxDBGridMaskColumn
            Caption = 'Cod Mat'
            Width = 45
            BandIndex = 0
            RowIndex = 0
            FieldName = 'codmat'
          end
          object GridAltSumatorid: TdxDBGridMaskColumn
            Caption = 'Id'
            Visible = False
            Width = 320
            BandIndex = 0
            RowIndex = 0
            FieldName = 'id'
          end
          object GridAltSumatortipmat: TdxDBGridColumn
            Caption = 'Grupa'
            Width = 96
            BandIndex = 0
            RowIndex = 0
            FieldName = 'tipmat'
          end
          object GridAltSumatordenmat: TdxDBGridColumn
            Caption = 'Denumire'
            Width = 225
            BandIndex = 0
            RowIndex = 0
            FieldName = 'denmat'
          end
          object GridAltSumatorid_gest_sumator: TdxDBGridMaskColumn
            Caption = 'Cod Comun'
            Visible = False
            Width = 84
            BandIndex = 0
            RowIndex = 0
            FieldName = 'id_gest_sumator'
          end
          object GridAltSumatorpret_receptie: TdxDBGridCurrencyColumn
            Caption = 'Pret Receptie'
            Width = 73
            BandIndex = 0
            RowIndex = 0
            FieldName = 'pret_receptie'
            Nullable = False
          end
          object GridAltSumatorpret_receptie_tva: TdxDBGridCurrencyColumn
            Caption = 'Pret Receptie TVA'
            Width = 79
            BandIndex = 0
            RowIndex = 0
            FieldName = 'pret_receptie_tva'
            Nullable = False
          end
          object GridAltSumatorgestiune: TdxDBGridColumn
            Caption = 'Gestiune'
            Width = 121
            BandIndex = 0
            RowIndex = 0
            FieldName = 'gestiune'
          end
          object GridAltSumatorstock: TdxDBGridCurrencyColumn
            Caption = 'Stoc'
            Width = 88
            BandIndex = 0
            RowIndex = 0
            FieldName = 'stock'
            Nullable = False
          end
          object GridAltSumatorstockValoric: TdxDBGridCurrencyColumn
            Caption = 'Stoc Valoric'
            Width = 113
            BandIndex = 0
            RowIndex = 0
            FieldName = 'stockValoric'
            Nullable = False
          end
        end
      end
      object tabFiseSumator: TcxTabSheet
        Caption = 'Fise Magazie pe acelasi sumator'
        ImageIndex = 5
        object pageSumator: TcxPageControl
          Left = 0
          Top = 0
          Width = 1001
          Height = 133
          Align = alClient
          TabOrder = 0
          Properties.CustomButtons.Buttons = <>
          Properties.TabPosition = tpBottom
          ClientRectBottom = 133
          ClientRectRight = 1001
          ClientRectTop = 0
        end
        object cxTabControlFise: TcxTabControl
          Left = 0
          Top = 133
          Width = 1001
          Height = 21
          Align = alBottom
          TabOrder = 1
          Properties.CustomButtons.Buttons = <>
          Properties.TabIndex = 1
          Properties.TabPosition = tpBottom
          Properties.Tabs.Strings = (
            '1'
            '2'
            '3'
            '4'
            '5'
            '6')
          OnChange = cxTabControlFiseChange
          ClientRectRight = 1001
          ClientRectTop = 0
        end
        object AtsDBGrid1: TdxDBGrid
          Left = 0
          Top = 0
          Width = 1001
          Height = 133
          SearchType = stStart
          Bands = <
            item
            end>
          DefaultLayout = True
          HeaderPanelRowCount = 1
          KeyField = 'ID'
          ShowSummaryFooter = True
          SummaryGroups = <>
          SummarySeparator = ', '
          Align = alClient
          TabOrder = 2
          DataSource = dtFisaMaterial
          Filter.Criteria = {00000000}
          LookAndFeel = lfFlat
          OptionsBehavior = [edgoAutoSearch, edgoAutoSort, edgoEnterShowEditor]
          OptionsDB = [edgoCanNavigation, edgoLoadAllRecords, edgoUseBookmarks]
          OptionsView = [edgoAutoWidth, edgoBandHeaderWidth, edgoInvertSelect, edgoUseBitmap]
          ShowRowFooter = True
          object AtsDBGridMaskColumn11: TdxDBGridMaskColumn
            Caption = 'Predator'
            HeaderAlignment = taCenter
            Width = 93
            BandIndex = 0
            RowIndex = 0
            FieldName = 'PREDATOR'
          end
          object AtsDBGridMaskColumn12: TdxDBGridMaskColumn
            Caption = 'Primitor'
            HeaderAlignment = taCenter
            Width = 121
            BandIndex = 0
            RowIndex = 0
            FieldName = 'PRIMITOR'
          end
          object AtsDBGridMaskColumn13: TdxDBGridMaskColumn
            Caption = 'Doc'
            HeaderAlignment = taCenter
            Width = 45
            BandIndex = 0
            RowIndex = 0
            FieldName = 'COD_DOCUM'
          end
          object AtsDBGridMaskColumn14: TdxDBGridMaskColumn
            Caption = 'Nr'
            HeaderAlignment = taCenter
            Width = 60
            BandIndex = 0
            RowIndex = 0
            FieldName = 'NR_DOCUM'
          end
          object AtsDBGridDateColumn2: TdxDBGridDateColumn
            Caption = 'Data'
            HeaderAlignment = taCenter
            Width = 60
            BandIndex = 0
            RowIndex = 0
            FieldName = 'DATA_DOCUM'
          end
          object AtsDBGridImageColumn2: TdxDBGridImageColumn
            Alignment = taLeftJustify
            Caption = 'Semn'
            HeaderAlignment = taCenter
            MinWidth = 16
            Width = 52
            BandIndex = 0
            RowIndex = 0
            FieldName = 'SEMN'
            DefaultImages = False
            Descriptions.Strings = (
              'Scazut'
              'Ignorat'
              'Adunat')
            Images = frmData.SemnImagini
            ImageIndexes.Strings = (
              '1'
              '-1'
              '0')
            Values.Strings = (
              '-1'
              '0'
              '1')
          end
          object AtsDBGridMaskColumn17: TdxDBGridMaskColumn
            Caption = 'Inainte'
            HeaderAlignment = taCenter
            Width = 56
            BandIndex = 0
            RowIndex = 0
            FieldName = 'CANTITATE_BEFORE'
          end
          object AtsDBGridMaskColumn15: TdxDBGridMaskColumn
            Caption = 'Cantitate'
            HeaderAlignment = taCenter
            Width = 56
            BandIndex = 0
            RowIndex = 0
            FieldName = 'CANTITATE'
          end
          object AtsDBGridMaskColumn18: TdxDBGridMaskColumn
            Caption = 'Dupa'
            HeaderAlignment = taCenter
            Width = 57
            BandIndex = 0
            RowIndex = 0
            FieldName = 'CANTITATE_AFTER'
          end
          object AtsDBGridMaskColumn16: TdxDBGridMaskColumn
            Caption = 'Tip Mat'
            HeaderAlignment = taCenter
            Width = 74
            BandIndex = 0
            RowIndex = 0
            FieldName = 'TIP_MATERIAL'
          end
          object AtsDBGridCurrencyColumn7: TdxDBGridCurrencyColumn
            Caption = 'Pret Unitar'
            HeaderAlignment = taCenter
            Width = 65
            BandIndex = 0
            RowIndex = 0
            FieldName = 'PRET_UNITAR'
            Nullable = False
          end
          object AtsDBGridCurrencyColumn8: TdxDBGridCurrencyColumn
            Caption = 'Valoare'
            HeaderAlignment = taCenter
            Width = 44
            BandIndex = 0
            RowIndex = 0
            FieldName = 'VALOARE'
            Nullable = False
          end
          object AtsDBGridMaskColumn19: TdxDBGridMaskColumn
            Caption = 'Operator'
            HeaderAlignment = taCenter
            Width = 73
            BandIndex = 0
            RowIndex = 0
            FieldName = 'OPERATOR'
          end
        end
      end
    end
  end
  object DTStock: TDataSource
    DataSet = MemStock
    Left = 24
    Top = 104
  end
  object QryStock: TZQuery
    Connection = frmData.dbContabilitate
    AfterOpen = QryStockAfterOpen
    SQL.Strings = (
      
        'EXEC SP_GETSTOCK :ID_GEST_DEFA_DOCUM, :ID_PREDATOR, :ID_PRIMITOR' +
        ', :DATA_STOC')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_GEST_DEFA_DOCUM'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_PREDATOR'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_PRIMITOR'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA_STOC'
        ParamType = ptUnknown
        Size = 16
      end>
    Left = 25
    Top = 198
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_GEST_DEFA_DOCUM'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_PREDATOR'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_PRIMITOR'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA_STOC'
        ParamType = ptUnknown
        Size = 16
      end>
  end
  object MemStock: TdxMemData
    Indexes = <>
    SortOptions = []
    OnFilterRecord = MemStockFilterRecord
    Left = 56
    Top = 104
  end
  object dtFisaMaterial: TDataSource
    DataSet = qryFisaMaterial
    Left = 25
    Top = 137
  end
  object qryFisaMaterial: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      
        'EXEC SP_GETFISA_MATERIAL :ID_GEST_TIP_STOCK, :CODMAT, 0,  :dataD' +
        'ocum, :tipStock')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_GEST_TIP_STOCK'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'CODMAT'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'dataDocum'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'tipStock'
        ParamType = ptUnknown
        Size = 1
      end>
    Left = 57
    Top = 137
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_GEST_TIP_STOCK'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'CODMAT'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'dataDocum'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'tipStock'
        ParamType = ptUnknown
        Size = 1
      end>
  end
  object dtStockAll: TDataSource
    DataSet = qryStockAll
    Left = 25
    Top = 169
  end
  object qryStockAll: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      
        'exec spGetStockUnitateCodMat :codmat, :dataDocum, :ID_GEST_TIP_S' +
        'TOCK'
      '')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'CODMAT'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'dataDocum'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_GEST_TIP_STOCK'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 57
    Top = 169
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'CODMAT'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'dataDocum'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_GEST_TIP_STOCK'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object popupGrid: TcxGridPopupMenu
    Grid = grStock
    PopupMenus = <
      item
        GridView = GridStock
        HitTypes = [gvhtCell, gvhtRecord]
        Index = 0
        PopupMenu = ppComenzi
      end>
    Left = 56
    Top = 201
  end
  object dtStockSum: TDataSource
    DataSet = qryStockSum
    Left = 25
    Top = 233
  end
  object qryStockSum: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      
        'exec spGetStockUnitateCodSum :codmat, :dataDocum, :ID_GEST_TIP_S' +
        'TOCK'
      '')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'CODMAT'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'dataDocum'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_GEST_TIP_STOCK'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 57
    Top = 232
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'CODMAT'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'dataDocum'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_GEST_TIP_STOCK'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object DTAcelasiSumator: TDataSource
    DataSet = qryAcelasiSumator
    Left = 113
    Top = 105
  end
  object qryAcelasiSumator: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      
        'exec spGetStockUnitateAcelasiSum :codmat, :dataDocum, :ID_GEST_T' +
        'IP_STOCK'
      '')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'CODMAT'
        ParamType = ptUnknown
        Size = 4
        Value = 1356
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'dataDocum'
        ParamType = ptUnknown
        Size = 16
        Value = 39263d
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_GEST_TIP_STOCK'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 145
    Top = 105
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'CODMAT'
        ParamType = ptUnknown
        Size = 4
        Value = 1356
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'dataDocum'
        ParamType = ptUnknown
        Size = 16
        Value = 39263d
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_GEST_TIP_STOCK'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object qryCodMaturi: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'exec spGetListCodMat :codmat'
      '')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'CODMAT'
        ParamType = ptUnknown
        Size = 4
        Value = 1356
      end>
    Left = 145
    Top = 145
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'CODMAT'
        ParamType = ptUnknown
        Size = 4
        Value = 1356
      end>
  end
  object ppComenzi: TPopupMenu
    Left = 120
    Top = 184
    object CmdSelectiePozitie: TMenuItem
      Caption = 'Selectie pozitie    Ctrl+Space'
      OnClick = CmdSelectiePozitieClick
    end
  end
end
