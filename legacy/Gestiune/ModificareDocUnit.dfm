object frmModificDocument: TfrmModificDocument
  Left = 331
  Top = 148
  Width = 733
  Height = 558
  Caption = 'Modificare Document'
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  PopupMenu = ppCopiereMenu
  Position = poScreenCenter
  OnCloseQuery = FormCloseQuery
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  DesignSize = (
    725
    527)
  PixelsPerInch = 96
  TextHeight = 13
  object Label1: TLabel
    Left = 22
    Top = 22
    Width = 81
    Height = 13
    Caption = 'Cod - Denumire :'
    Color = clBtnFace
    FocusControl = edCod
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWindowText
    Font.Height = -11
    Font.Name = 'Tahoma'
    Font.Style = []
    ParentColor = False
    ParentFont = False
    Transparent = True
  end
  object Label2: TLabel
    Left = 11
    Top = 2
    Width = 100
    Height = 13
    Caption = 'Detalii Document '
    Color = clBtnFace
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWindowText
    Font.Height = -11
    Font.Name = 'Tahoma'
    Font.Style = [fsBold]
    ParentColor = False
    ParentFont = False
    Transparent = True
  end
  object Bevel1: TBevel
    Left = 112
    Top = 8
    Width = 600
    Height = 3
    Anchors = [akLeft, akTop, akRight]
    Shape = bsTopLine
  end
  object Label10: TLabel
    Left = 22
    Top = 49
    Width = 95
    Height = 13
    Caption = 'Predator - Primitor :'
    Color = clBtnFace
    Font.Charset = DEFAULT_CHARSET
    Font.Color = clWindowText
    Font.Height = -11
    Font.Name = 'Tahoma'
    Font.Style = []
    ParentColor = False
    ParentFont = False
    Transparent = True
  end
  object tabDefaDoc: TTabControl
    Left = 8
    Top = 127
    Width = 699
    Height = 21
    Anchors = [akLeft, akTop, akRight]
    Images = Enabled
    Style = tsFlatButtons
    TabOrder = 6
    Tabs.Strings = (
      'Pred. in. <-> Prim. in.'
      'Pred. in. <-> Prim. ext.'
      'Pred. ext. <-> Prim. in.'
      'Pred. ext. <-> Prim. ext.')
    TabIndex = 0
    OnChange = tabDefaDocChange
    OnChanging = tabDefaDocChanging
    OnGetImageIndex = tabDefaDocGetImageIndex
  end
  object PageDescDocum: TPageControl
    Left = 8
    Top = 152
    Width = 700
    Height = 331
    ActivePage = tabContabilitate
    Anchors = [akLeft, akTop, akRight, akBottom]
    TabOrder = 8
    object tbDescDocum: TTabSheet
      Caption = 'Descriere mod tratare document'
      object pnDescDoc: TPanel
        Left = 0
        Top = 0
        Width = 692
        Height = 303
        Align = alClient
        BevelInner = bvLowered
        Caption = 'pnDescDoc'
        TabOrder = 0
        object Splitter3: TSplitter
          Left = 469
          Top = 2
          Height = 299
          Align = alRight
        end
        object Panel3: TPanel
          Left = 2
          Top = 2
          Width = 467
          Height = 299
          Align = alClient
          BevelOuter = bvLowered
          Caption = 'Panel3'
          TabOrder = 0
          object BtnModifyStockPredator: TSpeedButton
            Left = 251
            Top = 21
            Width = 23
            Height = 22
            Hint = 'Modifica tipul stocului'
            Caption = '...'
            OnClick = BtnModifyStockPredatorClick
          end
          object BtnModifyStockPrimitor: TSpeedButton
            Left = 251
            Top = 63
            Width = 23
            Height = 22
            Hint = 'Modifica tipul stocului'
            Caption = '...'
            OnClick = BtnModifyStockPrimitorClick
          end
          object Label4: TLabel
            Left = 282
            Top = 4
            Width = 108
            Height = 13
            Caption = 'Adauga Coloana Noua'
          end
          object Label14: TLabel
            Left = 282
            Top = 48
            Width = 101
            Height = 13
            Caption = 'Lista campuri pentru :'
          end
          object BtnAdaugaColoana: TSpeedButton
            Left = 280
            Top = 21
            Width = 145
            Height = 22
            Caption = 'Adauga Coloana Noua'
            OnClick = BtnAdaugaColoanaClick
          end
          object chkStockPredator: TdxfCheckBox
            Left = 8
            Top = 4
            Width = 134
            Height = 18
            Checked = True
            GroupIndex = 0
            Caption = 'Tip de stock la predator'
            Font.Charset = DEFAULT_CHARSET
            Font.Color = clWindowText
            Font.Height = -11
            Font.Name = 'MS Sans Serif'
            Font.Style = []
            ParentFont = False
            TabOrder = 0
          end
          object edTipStockPred: TdxDBImageEdit
            Left = 8
            Top = 22
            Width = 241
            TabOrder = 1
            Alignment = taLeftJustify
            DataField = 'ID_GEST_TIP_STOC_PREDATOR'
            DataSource = frmData.DTDefaDoc
            StoredValues = 1
          end
          object Panel4: TPanel
            Left = 1
            Top = 90
            Width = 465
            Height = 208
            Align = alBottom
            Anchors = [akLeft, akTop, akRight, akBottom]
            BevelInner = bvLowered
            Caption = 'Panel4'
            TabOrder = 2
            object GridTemplate: TdxDBGrid
              Left = 2
              Top = 2
              Width = 461
              Height = 204
              SearchType = stStart
              Bands = <
                item
                end>
              DefaultLayout = True
              HeaderPanelRowCount = 1
              KeyField = 'ID_GEST_DEFA_DOCUM_ITEMSI'
              SummaryGroups = <>
              SummarySeparator = ', '
              Align = alClient
              PopupMenu = ppCopiereMenu
              TabOrder = 0
              DataSource = DTTemplateCrid
              Filter.Active = True
              Filter.Criteria = {00000000}
              LookAndFeel = lfFlat
              OptionsBehavior = [edgoAutoSearch, edgoAutoSort, edgoDblClick, edgoEditing, edgoEnterShowEditor, edgoImmediateEditor, edgoMultiSort, edgoTabs, edgoTabThrough]
              OptionsDB = [edgoCanInsert, edgoCanNavigation, edgoLoadAllRecords, edgoUseBookmarks]
              OptionsView = [edgoAutoCalcPreviewLines, edgoAutoWidth, edgoBandHeaderWidth, edgoIndicator, edgoInvertSelect, edgoPreview, edgoUseBitmap]
              PreviewFieldName = 'FORMULA_CALCUL'
              OnCustomDraw = GridTemplateCustomDraw
              object GridTemplateCLASS_NAME: TdxDBGridPickColumn
                Caption = 'Mod Editare'
                DisableFilter = True
                DisableEditor = True
                HeaderAlignment = taCenter
                Visible = False
                Width = 100
                BandIndex = 0
                RowIndex = 0
                FieldName = 'CLASS_NAME'
              end
              object GridTemplatePOS: TdxDBGridSpinColumn
                Caption = 'Pos'
                DisableFilter = True
                DisableEditor = True
                HeaderAlignment = taCenter
                Sorted = csUp
                Width = 51
                BandIndex = 0
                RowIndex = 0
                FieldName = 'POS'
              end
              object GridTemplateFIELD_NAME: TdxDBGridPickColumn
                Caption = 'Nume Camp'
                DisableFilter = True
                HeaderAlignment = taCenter
                Width = 85
                BandIndex = 0
                RowIndex = 0
                FieldName = 'FIELD_NAME'
              end
              object GridTemplateCAPTION: TdxDBGridMaskColumn
                Caption = 'Captura'
                DisableFilter = True
                HeaderAlignment = taCenter
                Width = 106
                BandIndex = 0
                RowIndex = 0
                FieldName = 'CAPTION'
              end
              object GridTemplateMIN_WIDTH: TdxDBGridSpinColumn
                Caption = 'Min Latime'
                DisableFilter = True
                DisableEditor = True
                HeaderAlignment = taCenter
                Visible = False
                Width = 55
                BandIndex = 0
                RowIndex = 0
                FieldName = 'MIN_WIDTH'
              end
              object GridTemplateMAX_WIDTH: TdxDBGridSpinColumn
                Caption = 'Max Latime'
                DisableFilter = True
                DisableEditor = True
                HeaderAlignment = taCenter
                Visible = False
                Width = 55
                BandIndex = 0
                RowIndex = 0
                FieldName = 'MAX_WIDTH'
              end
              object GridTemplateCAPTION_ALIGN: TdxDBGridImageColumn
                Alignment = taLeftJustify
                Caption = 'Asezare capt.'
                DisableFilter = True
                HeaderAlignment = taCenter
                MinWidth = 16
                Width = 91
                BandIndex = 0
                RowIndex = 0
                FieldName = 'CAPTION_ALIGN'
                DefaultImages = False
                Descriptions.Strings = (
                  'Aliniat la stanga'
                  'Aliniat la dreapta'
                  'Centrat')
                ImageIndexes.Strings = (
                  '-1'
                  '-1'
                  '-1')
                ShowDescription = True
                Values.Strings = (
                  '0'
                  '1'
                  '2')
              end
              object GridTemplateALIGN: TdxDBGridImageColumn
                Alignment = taLeftJustify
                Caption = 'Asezare'
                DisableFilter = True
                DisableEditor = True
                HeaderAlignment = taCenter
                MinWidth = 16
                Visible = False
                Width = 62
                BandIndex = 0
                RowIndex = 0
                FieldName = 'ALIGN'
              end
              object GridTemplateCOLOR: TdxDBGridButtonColumn
                Caption = 'Culoare'
                DisableFilter = True
                DisableEditor = True
                HeaderAlignment = taCenter
                Width = 71
                BandIndex = 0
                RowIndex = 0
                FieldName = 'COLOR'
                Buttons = <
                  item
                    Default = True
                  end>
              end
              object GridTemplateFONT_NAME: TdxDBGridPickColumn
                Caption = 'Nume Font'
                DisableFilter = True
                DisableEditor = True
                HeaderAlignment = taCenter
                Visible = False
                Width = 85
                BandIndex = 0
                RowIndex = 0
                FieldName = 'FONT_NAME'
              end
              object GridTemplateFONT_COLOR: TdxDBGridButtonColumn
                Caption = 'Culoare Font'
                DisableFilter = True
                DisableEditor = True
                HeaderAlignment = taCenter
                Visible = False
                Width = 65
                BandIndex = 0
                RowIndex = 0
                FieldName = 'FONT_COLOR'
                Buttons = <
                  item
                    Default = True
                  end>
              end
              object GridTemplateEDIT_MASK: TdxDBGridMaskColumn
                Caption = 'Masca editare'
                DisableFilter = True
                DisableEditor = True
                HeaderAlignment = taCenter
                Visible = False
                Width = 68
                BandIndex = 0
                RowIndex = 0
                FieldName = 'EDIT_MASK'
              end
              object GridTemplateFONT_SIZE: TdxDBGridSpinColumn
                Caption = 'Marime Font'
                DisableFilter = True
                DisableEditor = True
                HeaderAlignment = taCenter
                Visible = False
                Width = 55
                BandIndex = 0
                RowIndex = 0
                FieldName = 'FONT_SIZE'
              end
              object GridTemplateVISIBLE: TdxDBGridCheckColumn
                Caption = 'Visible'
                HeaderAlignment = taCenter
                Width = 67
                BandIndex = 0
                RowIndex = 0
                FieldName = 'VISIBLE'
                ValueChecked = 'True'
                ValueUnchecked = 'False'
              end
              object GridTemplateFORMULA_CALCUL: TdxDBGridMaskColumn
                Caption = 'Formula Calcul'
                Visible = False
                BandIndex = 0
                RowIndex = 0
                FieldName = 'FORMULA_CALCUL'
              end
            end
          end
          object AtsfCheckBox1: TdxfCheckBox
            Left = 8
            Top = 45
            Width = 128
            Height = 18
            Checked = False
            GroupIndex = 0
            Caption = 'Tip de stock la primitor'
            Font.Charset = DEFAULT_CHARSET
            Font.Color = clWindowText
            Font.Height = -11
            Font.Name = 'MS Sans Serif'
            Font.Style = []
            ParentFont = False
            TabOrder = 3
          end
          object edTipStockPrim: TdxDBImageEdit
            Left = 8
            Top = 64
            Width = 241
            TabOrder = 4
            Alignment = taLeftJustify
            DataField = 'ID_GEST_TIP_STOC_PRIMITOR'
            DataSource = frmData.DTDefaDoc
            StoredValues = 1
          end
          object edTipListaCampuri: TdxImageEdit
            Left = 280
            Top = 64
            Width = 145
            TabOrder = 5
            Text = '0'
            OnChange = edTipListaCampuriChange
            Descriptions.Strings = (
              'Pentru pozitii'
              'Pentru document')
            ImageIndexes.Strings = (
              '0'
              '1')
            Values.Strings = (
              '0'
              '1')
          end
        end
        object InspTemplate: TdxDBInspector
          Left = 472
          Top = 2
          Width = 218
          Height = 299
          Align = alRight
          Color = clWindow
          DataSource = DTTemplateCrid
          DefaultFields = False
          TabOrder = 1
          DividerPos = 120
          Flat = True
          GridColor = clBtnFace
          PaintStyle = ipsNET
          Data = {
            9C02000013000000080000000000000016000000496E737054656D706C617465
            4649454C445F4E414D45080000000000000016000000417473496E7370656374
            6F7244424D61736B526F7731080000000000000016000000417473496E737065
            63746F7244425370696E526F7733080000000000000017000000417473496E73
            706563746F724442436865636B526F7731080000000000000017000000417473
            496E73706563746F724442496D616765526F7732080000000000000017000000
            417473496E73706563746F724442496D616765526F7733080000000000000016
            000000417473496E73706563746F7244424D61736B526F773308000000000000
            0017000000417473496E73706563746F724442496D616765526F773108000000
            0000000016000000417473496E73706563746F7244425370696E526F77310800
            00000000000016000000417473496E73706563746F7244425370696E526F7732
            080000000000000018000000417473496E73706563746F724442427574746F6E
            526F7731080000000000000016000000417473496E73706563746F7244424D61
            736B526F7732080000000000000018000000417473496E73706563746F724442
            427574746F6E526F7732080000000000000016000000417473496E7370656374
            6F7244425370696E526F7734080000000000000017000000417473496E737065
            63746F724442436865636B526F7733080000000000000017000000417473496E
            73706563746F724442436865636B526F77320800000000000000180000004174
            73496E73706563746F724442427574746F6E526F773308000000000000001600
            0000417473496E73706563746F7244424D61736B526F77340800000000000000
            18000000496E737054656D706C61746553455F494E53554D45415A4100000000}
          object AtsInspectorDBMaskRow1: TdxInspectorDBMaskRow
            Caption = 'CAPTURA'
            FieldName = 'CAPTION'
          end
          object AtsInspectorDBCheckRow1: TdxInspectorDBCheckRow
            Caption = 'VISIBILA'
            ValueChecked = 'True'
            ValueUnchecked = 'False'
            FieldName = 'VISIBLE'
          end
          object AtsInspectorDBMaskRow2: TdxInspectorDBMaskRow
            Caption = 'NUME FONT'
            FieldName = 'FONT_NAME'
          end
          object AtsInspectorDBMaskRow3: TdxInspectorDBMaskRow
            Caption = 'MASCA EDITARE'
            FieldName = 'EDIT_MASK'
          end
          object AtsInspectorDBImageRow1: TdxInspectorDBImageRow
            Caption = 'TIP COLOANA'
            FieldName = 'CLASS_NAME'
          end
          object InspTemplateFIELD_NAME: TdxInspectorDBPickRow
            Caption = 'NUME CAMP'
            FieldName = 'FIELD_NAME'
          end
          object AtsInspectorDBSpinRow1: TdxInspectorDBSpinRow
            Caption = 'LATIME MIN.'
            FieldName = 'MIN_WIDTH'
          end
          object AtsInspectorDBSpinRow2: TdxInspectorDBSpinRow
            Caption = 'LATIME MAX.'
            FieldName = 'MAX_WIDTH'
          end
          object AtsInspectorDBImageRow2: TdxInspectorDBImageRow
            Caption = 'ASEZARE CAPT.'
            DefaultImages = False
            Descriptions.Strings = (
              'Aliniat la stanga'
              'Aliniat la dreapta'
              'Centrat')
            ImageIndexes.Strings = (
              '-1'
              '-1'
              '-1')
            ShowDescription = True
            Values.Strings = (
              '0'
              '1'
              '2')
            FieldName = 'CAPTION_ALIGN'
          end
          object AtsInspectorDBImageRow3: TdxInspectorDBImageRow
            Caption = 'ASEZARE DATE'
            DefaultImages = False
            Descriptions.Strings = (
              'Aliniat la stanga'
              'Aliniat la dreapta'
              'Centrat')
            ImageIndexes.Strings = (
              '-1'
              '-1'
              '-1')
            ShowDescription = True
            Values.Strings = (
              '0'
              '1'
              '2')
            FieldName = 'ALIGN'
          end
          object AtsInspectorDBSpinRow3: TdxInspectorDBSpinRow
            Caption = 'POZITIE'
            FieldName = 'POS'
          end
          object AtsInspectorDBButtonRow1: TdxInspectorDBButtonRow
            Caption = 'CULOARE'
            OnDrawValue = InspectorColoaneFONT_COLORDrawValue
            ButtonOnly = True
            Buttons = <
              item
                Default = True
              end>
            OnButtonClick = InspectorColoaneCOLORButtonClick
            FieldName = 'COLOR'
          end
          object AtsInspectorDBButtonRow2: TdxInspectorDBButtonRow
            Caption = 'CULOARE FONT'
            OnDrawValue = InspectorColoaneFONT_COLORDrawValue
            ButtonOnly = True
            Buttons = <
              item
                Default = True
              end>
            OnButtonClick = InspectorColoaneCOLORButtonClick
            FieldName = 'FONT_COLOR'
          end
          object AtsInspectorDBSpinRow4: TdxInspectorDBSpinRow
            Caption = 'MARIME FONT'
            FieldName = 'FONT_SIZE'
          end
          object AtsInspectorDBCheckRow2: TdxInspectorDBCheckRow
            Caption = 'OBLIGATORIU'
            ValueChecked = 'True'
            ValueUnchecked = 'False'
            FieldName = 'REQUIRED'
          end
          object AtsInspectorDBButtonRow3: TdxInspectorDBButtonRow
            Caption = 'FORMULA'
            Buttons = <
              item
                Default = True
              end>
            FieldName = 'FORMULA_CALCUL'
          end
          object AtsInspectorDBCheckRow3: TdxInspectorDBCheckRow
            ValueChecked = 'True'
            ValueUnchecked = 'False'
            FieldName = 'READONLY'
          end
          object InspTemplateSE_INSUMEAZA: TdxInspectorDBCheckRow
            Caption = 'SE INSUMEAZA'
            ValueChecked = 'True'
            ValueUnchecked = 'False'
            FieldName = 'SUM_TOTAL'
          end
          object AtsInspectorDBMaskRow4: TdxInspectorDBSpinRow
            FieldName = 'PRECEDENTA'
          end
        end
      end
    end
    object tabTratareCODMAT: TTabSheet
      Caption = 'Descriere mecanisme automate'
      ImageIndex = 1
      DesignSize = (
        692
        303)
      object Label5: TLabel
        Left = 8
        Top = 5
        Width = 87
        Height = 13
        Caption = 'Document &conex :'
        FocusControl = edDocumentConex
      end
      object Label6: TLabel
        Left = 8
        Top = 90
        Width = 83
        Height = 13
        Caption = 'Mod tratare stock'
        FocusControl = edTipDescarcare
      end
      object LbReportInfo: TLabel
        Left = 32
        Top = 193
        Width = 126
        Height = 13
        Caption = 'Raportul generat automat :'
      end
      object LbZileGratie: TLabel
        Left = 32
        Top = 236
        Width = 140
        Height = 13
        Caption = 'Zile de gratie pentru validare :'
      end
      object Label8: TLabel
        Left = 296
        Top = 5
        Width = 145
        Height = 13
        Caption = 'Validari necesare pentru tiparie'
      end
      object Label9: TLabel
        Left = 8
        Top = 45
        Width = 146
        Height = 13
        Caption = 'Mod definire document conex :'
        FocusControl = edDocumentConex
      end
      object BtnModifica: TSpeedButton
        Left = 512
        Top = 48
        Width = 65
        Height = 22
        Caption = 'Modifica'
        OnClick = BtnModificaClick
      end
      object BtnDelete: TSpeedButton
        Left = 584
        Top = 48
        Width = 65
        Height = 22
        Caption = 'Sterge'
        OnClick = BtnDeleteClick
      end
      object BtnAdauga: TSpeedButton
        Left = 440
        Top = 48
        Width = 65
        Height = 22
        Caption = 'Adauga'
        OnClick = BtnAdaugaClick
      end
      object BtnModifyReport: TSpeedButton
        Left = 256
        Top = 208
        Width = 23
        Height = 22
        Caption = '...'
        OnClick = BtnModifyReportClick
      end
      object edDocumentConex: TdxDBImageEdit
        Left = 8
        Top = 22
        Width = 273
        TabOrder = 0
        Alignment = taLeftJustify
        DataField = 'ID_DOCUMENT_CONEX'
        DataSource = frmData.DTDefaDoc
        StoredValues = 1
      end
      object edTipDescarcare: TdxDBImageEdit
        Left = 8
        Top = 107
        Width = 273
        TabOrder = 1
        Alignment = taLeftJustify
        DataField = 'TIP_DESCARCARE'
        DataSource = frmData.DTDefaDoc
        DefaultImages = False
        StoredValues = 1
      end
      object edChkNumarAuto: TdxDBCheckEdit
        Left = 4
        Top = 129
        Width = 249
        TabOrder = 2
        Caption = 'Generare Automat &Numere ( Prefix - Start - End)'
        DataField = 'NUMAR_AUTOMAT'
        DataSource = frmData.DTDefaDoc
        ValueChecked = 'True'
        ValueUnchecked = 'False'
      end
      object edPrefix: TdxDBEdit
        Left = 8
        Top = 149
        Width = 53
        TabOrder = 3
        DataField = 'NUMAR_PREFIX'
        DataSource = frmData.DTDefaDoc
      end
      object edNumarStart: TdxDBSpinEdit
        Left = 72
        Top = 149
        Width = 105
        TabOrder = 4
        Alignment = taRightJustify
        DataField = 'NUMAR_START'
        DataSource = frmData.DTDefaDoc
        StoredValues = 1
      end
      object edNumarEnd: TdxDBSpinEdit
        Left = 184
        Top = 149
        Width = 97
        TabOrder = 5
        Alignment = taRightJustify
        DataField = 'NUMAR_END'
        DataSource = frmData.DTDefaDoc
        StoredValues = 1
      end
      object edTiparireAutomata: TdxfCheckBox
        Left = 8
        Top = 174
        Width = 276
        Height = 18
        Checked = True
        GroupIndex = 0
        Caption = 'Se genereaza automat raport la tiparirea documentului'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = []
        ParentFont = False
        TabOrder = 6
        OnClick = edTiparireAutomataClick
      end
      object edZileValabiltiate: TdxDBSpinEdit
        Left = 184
        Top = 233
        Width = 97
        TabOrder = 7
        DataField = 'PERIOADA_EXPIRARE'
        DataSource = frmData.DTDefaDoc
        OnChange = edZileValabiltiateChange
      end
      object AtsDBCheckEdit1: TdxDBCheckEdit
        Left = 32
        Top = 257
        Width = 185
        TabOrder = 8
        Caption = 'Se valideaza automat de emitent'
        DataField = 'AUTO_VALIDARE_EMITENT'
        DataSource = frmData.DTDefaDoc
        ValueChecked = 'True'
        ValueUnchecked = 'False'
      end
      object AtsDBImageEdit2: TdxDBImageEdit
        Left = 8
        Top = 62
        Width = 273
        TabOrder = 9
        Alignment = taLeftJustify
        DataField = 'TIP_DESCARCARE'
        DataSource = frmData.DTDefaDoc
        Descriptions.Strings = (
          'Generare la fel'
          'Complementeaza Gestiuni')
        ImageIndexes.Strings = (
          '0'
          '1')
        Values.Strings = (
          '0'
          '1')
        StoredValues = 1
      end
      object GridValidari: TdxDBGrid
        Left = 294
        Top = 72
        Width = 393
        Height = 219
        SearchType = stStart
        Bands = <
          item
          end>
        DefaultLayout = True
        HeaderPanelRowCount = 1
        KeyField = 'ID_GEST_TEMPLATE_VALIDARI'
        SummaryGroups = <>
        SummarySeparator = ', '
        TabOrder = 10
        DataSource = DTValidari
        Filter.Criteria = {00000000}
        LookAndFeel = lfFlat
        OptionsView = [edgoAutoWidth, edgoBandHeaderWidth, edgoUseBitmap]
        Anchors = [akLeft, akTop, akRight, akBottom]
        object GridValidariID_FUNCTIUNE: TdxDBGridImageColumn
          Alignment = taLeftJustify
          Caption = 'Functie'
          HeaderAlignment = taCenter
          MinWidth = 16
          Width = 128
          BandIndex = 0
          RowIndex = 0
          FieldName = 'ID_FUNCTIUNE'
          DefaultImages = False
          ShowDescription = True
        end
        object GridValidariZILE_GRATIE: TdxDBGridSpinColumn
          Caption = 'Zile Gratie'
          HeaderAlignment = taCenter
          Width = 54
          BandIndex = 0
          RowIndex = 0
          FieldName = 'ZILE_GRATIE'
        end
        object GridValidariPRIORITATE: TdxDBGridImageColumn
          Alignment = taLeftJustify
          Caption = 'Prioritate'
          HeaderAlignment = taCenter
          MinWidth = 16
          Width = 53
          BandIndex = 0
          RowIndex = 0
          FieldName = 'PRIORITATE'
          Descriptions.Strings = (
            'Cea mai mica'
            'Mica'
            'Normala'
            'Mare'
            'Cea mai mare')
          ImageIndexes.Strings = (
            '0'
            '1'
            '2'
            '3'
            '4')
          ShowDescription = True
          Values.Strings = (
            '0'
            '1'
            '2'
            '3'
            '4')
        end
        object GridValidariTIP_VALIDARE: TdxDBGridImageColumn
          Alignment = taLeftJustify
          Caption = 'Tip'
          HeaderAlignment = taCenter
          MinWidth = 16
          Width = 84
          BandIndex = 0
          RowIndex = 0
          FieldName = 'TIP_VALIDARE'
          Descriptions.Strings = (
            'Informare'
            'Validare efectiva'
            'Introducere'
            'Introducere/Validare')
          ImageIndexes.Strings = (
            '0'
            '1'
            '2'
            '3')
          ShowDescription = True
          Values.Strings = (
            '0'
            '1'
            '2'
            '3')
        end
      end
      object edListaFunctii: TdxPopupEdit
        Left = 294
        Top = 22
        Width = 139
        TabOrder = 11
        ReadOnly = False
        HideEditCursor = True
        PopupControl = TreeFunctiuni
        PopupFormBorderStyle = pbsSysPanel
        OnCloseUp = edListaFunctiiCloseUp
        StoredValues = 64
      end
      object edZileGratie: TdxSpinEdit
        Left = 440
        Top = 22
        Width = 41
        TabOrder = 12
        Value = 10.000000000000000000
      end
      object edPrioritate: TdxSpinEdit
        Left = 488
        Top = 22
        Width = 41
        TabOrder = 13
        Value = 1.000000000000000000
      end
      object edTipValidare: TdxImageEdit
        Left = 536
        Top = 22
        Width = 113
        TabOrder = 14
        Text = '1'
        Descriptions.Strings = (
          'Informare'
          'Validare Efectiva'
          'Introducere'
          'Introducere/Validare')
        ImageIndexes.Strings = (
          '0'
          '1'
          '2'
          '3')
        Values.Strings = (
          '0'
          '1'
          '2'
          '3')
      end
      object edRaportGenerat: TdxPopupEdit
        Left = 32
        Top = 206
        Width = 217
        TabOrder = 15
        HideEditCursor = True
        PopupControl = TreeReportList
        PopupFormBorderStyle = pbsSysPanel
        OnCloseUp = edRaportGeneratCloseUp
      end
      object TreeFunctiuni: TdxDBTreeList
        Left = 304
        Top = 88
        Width = 329
        Height = 201
        SearchType = stContain
        Bands = <
          item
          end>
        DefaultLayout = True
        HeaderPanelRowCount = 1
        KeyField = 'ID_FUNCTIUNI'
        ParentField = 'ID_PARINTE'
        TabOrder = 16
        Visible = False
        OnDblClick = TreeFunctiuniDblClick
        OnKeyDown = TreeFunctiuniKeyDown
        DataSource = frmData.DTFunctiuni
        LookAndFeel = lfFlat
        OptionsBehavior = [etoAutoSearch, etoAutoSort, etoDblClick]
        OptionsView = [etoAutoCalcPreviewLines, etoAutoWidth, etoBandHeaderWidth, etoInvertSelect, etoPreview, etoUseBitmap, etoUseImageIndexForSelected]
        PreviewFieldName = 'DESCRIERE'
        TreeLineColor = clGrayText
        object TreeFunctiuniDENUMIRE: TdxDBTreeListMaskColumn
          Caption = 'Denumire Functie'
          HeaderAlignment = taCenter
          Sorted = csUp
          Width = 229
          BandIndex = 0
          RowIndex = 0
          FieldName = 'DENUMIRE'
        end
        object TreeFunctiuniCOD_FUNCTIE: TdxDBTreeListMaskColumn
          Caption = 'Cod'
          HeaderAlignment = taCenter
          Width = 58
          BandIndex = 0
          RowIndex = 0
          FieldName = 'COD_FUNCTIE'
        end
        object TreeFunctiuniDESCRIERE: TdxDBTreeListMaskColumn
          HeaderAlignment = taCenter
          Visible = False
          Width = 420
          BandIndex = 0
          RowIndex = 0
          FieldName = 'DESCRIERE'
        end
      end
      object TreeReportList: TdxDBTreeList
        Left = 88
        Top = 24
        Width = 313
        Height = 201
        SearchType = stContain
        Bands = <
          item
          end>
        DefaultLayout = True
        HeaderPanelRowCount = 1
        KeyField = 'ID_REPORT'
        ParentField = 'ID_PARINTE'
        TabOrder = 17
        Visible = False
        OnDblClick = TreeFunctiuniDblClick
        OnKeyDown = TreeFunctiuniKeyDown
        DataSource = DTReportList
        LookAndFeel = lfFlat
        OptionsBehavior = [etoAutoSearch, etoAutoSort, etoDblClick]
        OptionsDB = [etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
        OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
        RootValue = '0'
        TreeLineColor = clGrayText
        object TreeReportListCAPTURA: TdxDBTreeListMaskColumn
          Caption = 'Nume Raport'
          HeaderAlignment = taCenter
          Sorted = csUp
          BandIndex = 0
          RowIndex = 0
          FieldName = 'CAPTURA'
        end
        object TreeReportListIS_REPORT: TdxDBTreeListCheckColumn
          Caption = 'Este Raport'
          HeaderAlignment = taCenter
          Visible = False
          Width = 100
          BandIndex = 0
          RowIndex = 0
          FieldName = 'IS_REPORT'
          ValueChecked = 'True'
          ValueUnchecked = 'False'
        end
      end
      object TreePlan: TdxDBTreeList
        Left = 336
        Top = 144
        Width = 337
        Height = 201
        SearchType = stContain
        Bands = <
          item
          end>
        DefaultLayout = True
        HeaderPanelRowCount = 1
        KeyField = 'CONT'
        ParentField = 'PARINTE'
        TabOrder = 18
        Visible = False
        OnDblClick = TreePlanDblClick
        OnKeyDown = TreePlanKeyDown
        DataSource = frmData.DTPlanCont
        LookAndFeel = lfFlat
        OptionsBehavior = [etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSearch, etoAutoSort, etoDblClick, etoDragExpand, etoDragScroll, etoEditing, etoEnterShowEditor, etoImmediateEditor, etoTabThrough]
        OptionsCustomize = [etoBandMoving, etoBandSizing, etoColumnMoving, etoColumnSizing, etoExtCustomizing, etoKeepColumnWidth]
        OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoRowSelect, etoUseBitmap, etoUseImageIndexForSelected]
        TreeLineColor = clGrayText
        object TreePlanCONT: TdxDBTreeListMaskColumn
          Caption = 'Cont'
          HeaderAlignment = taCenter
          Visible = False
          Width = 92
          BandIndex = 0
          RowIndex = 0
          FieldName = 'CONT'
        end
        object TreePlanROMANA: TdxDBTreeListMaskColumn
          Caption = 'Plan Cont'
          HeaderAlignment = taCenter
          Sorted = csUp
          Width = 227
          BandIndex = 0
          RowIndex = 0
          FieldName = 'ROMANA'
          OnGetText = TreePlanROMANAGetText
        end
        object TreePlanSID: TdxDBTreeListMaskColumn
          Width = 40
          BandIndex = 0
          RowIndex = 0
          FieldName = 'SID'
        end
        object TreePlanSIC: TdxDBTreeListMaskColumn
          Width = 40
          BandIndex = 0
          RowIndex = 0
          FieldName = 'SIC'
        end
      end
      object edtIsPrinting: TdxDBCheckEdit
        Left = 8
        Top = 277
        Width = 185
        TabOrder = 19
        Caption = 'Se tipareste Raport pe ecran'
        DataField = 'IS_PRINTING'
        DataSource = frmData.DTDefaDoc
        ValueChecked = 'True'
        ValueUnchecked = 'False'
        NullStyle = nsUnchecked
      end
      object imgDMEdit: TdxDBImageEdit
        Left = 6
        Top = 304
        Width = 281
        TabOrder = 20
        Visible = False
        DataSource = frmData.DTDefaDoc
      end
    end
    object tabContabilitate: TTabSheet
      Caption = 'Generare note contabile'
      ImageIndex = 2
      DesignSize = (
        692
        303)
      object Label7: TLabel
        Left = 345
        Top = 3
        Width = 66
        Height = 13
        Caption = 'Cont debitor : '
      end
      object Label11: TLabel
        Left = 501
        Top = 2
        Width = 66
        Height = 13
        Caption = 'Cont creditor :'
      end
      object Label12: TLabel
        Left = 345
        Top = 44
        Width = 100
        Height = 13
        Caption = 'Formula de contare : '
      end
      object Label13: TLabel
        Left = 345
        Top = 83
        Width = 106
        Height = 13
        Caption = 'Clasificatie Economica'
      end
      object BtnAddNota: TSpeedButton
        Left = 425
        Top = 128
        Width = 73
        Height = 22
        Caption = 'Adauga'
        OnClick = BtnAddNotaClick
      end
      object BtnModifyNota: TSpeedButton
        Left = 502
        Top = 128
        Width = 73
        Height = 22
        Caption = 'Modifica'
        OnClick = BtnModifyNotaClick
      end
      object BtnDeleteNota: TSpeedButton
        Left = 579
        Top = 128
        Width = 73
        Height = 22
        Caption = 'Sterge'
        Enabled = False
        OnClick = BtnDeleteNotaClick
      end
      object Label3: TLabel
        Left = 9
        Top = 3
        Width = 165
        Height = 13
        Caption = 'Lista tipurilor de materiale suportate'
      end
      object BtnAddMaterial: TSpeedButton
        Left = 109
        Top = 51
        Width = 73
        Height = 22
        Caption = 'Adauga'
        OnClick = BtnAddMaterialClick
      end
      object BtnModifyMaterial: TSpeedButton
        Left = 186
        Top = 51
        Width = 73
        Height = 22
        Caption = 'Modifica'
        OnClick = BtnModifyMaterialClick
      end
      object BtnDeleteMaterial: TSpeedButton
        Left = 263
        Top = 51
        Width = 73
        Height = 22
        Caption = 'Sterge'
        Enabled = False
        OnClick = BtnDeleteMaterialClick
      end
      object BtnCopy: TSpeedButton
        Left = 347
        Top = 128
        Width = 73
        Height = 22
        Caption = 'Copiaza'
        OnClick = BtnCopyClick
      end
      object GridTipuriMateriale: TdxDBGrid
        Left = 8
        Top = 80
        Width = 329
        Height = 203
        SearchType = stStart
        Bands = <
          item
          end>
        DefaultLayout = True
        HeaderPanelRowCount = 1
        KeyField = 'ID_GEST_ITEMSI_TIP_MATERIAL'
        SummaryGroups = <>
        SummarySeparator = ', '
        TabOrder = 0
        DataSource = DTTipuriMateriale
        Filter.Criteria = {00000000}
        LookAndFeel = lfFlat
        OptionsBehavior = [edgoAutoSearch, edgoAutoSort, edgoEditing, edgoEnterShowEditor, edgoImmediateEditor, edgoTabThrough, edgoVertThrough]
        OptionsDB = [edgoCanNavigation, edgoLoadAllRecords, edgoUseBookmarks]
        OptionsView = [edgoAutoWidth, edgoBandHeaderWidth, edgoUseBitmap]
        OnChangeNode = GridTipuriMaterialeChangeNode
        Anchors = [akLeft, akTop, akBottom]
        object GridTipuriMaterialeID_GEST_TIP_MATERIAL: TdxDBGridImageColumn
          Alignment = taLeftJustify
          Caption = 'Tip Material'
          DisableEditor = True
          HeaderAlignment = taCenter
          MinWidth = 16
          Width = 177
          BandIndex = 0
          RowIndex = 0
          FieldName = 'ID_GEST_TIP_MATERIAL'
          ShowDescription = True
        end
        object GridTipuriMaterialeGENEREAZA_CODMAT: TdxDBGridCheckColumn
          Caption = 'Cod nou'
          HeaderAlignment = taCenter
          Width = 71
          BandIndex = 0
          RowIndex = 0
          FieldName = 'GENEREAZA_CODMAT'
          ValueChecked = 'True'
          ValueUnchecked = 'False'
        end
        object GridTipuriMaterialeACCEPT_STOCK_NEGATIV: TdxDBGridCheckColumn
          Caption = 'Stock negativ'
          Width = 79
          BandIndex = 0
          RowIndex = 0
          FieldName = 'ACCEPT_STOCK_NEGATIV'
          ValueChecked = 'True'
          ValueUnchecked = 'False'
        end
      end
      object GridModContare: TdxDBGrid
        Left = 343
        Top = 156
        Width = 332
        Height = 126
        SearchType = stStart
        Bands = <
          item
          end>
        DefaultLayout = True
        HeaderPanelRowCount = 1
        KeyField = 'ID_GEST_DEFA_NOTA_CONT'
        SummaryGroups = <>
        SummarySeparator = ', '
        TabOrder = 1
        DataSource = DTModContare
        Filter.Criteria = {00000000}
        LookAndFeel = lfFlat
        OptionsBehavior = [edgoAutoSearch, edgoAutoSort]
        OptionsDB = [edgoCanNavigation, edgoLoadAllRecords, edgoUseBookmarks]
        OptionsView = [edgoAutoWidth, edgoBandHeaderWidth, edgoInvertSelect, edgoUseBitmap]
        OnChangeNode = GridModContareChangeNode
        Anchors = [akLeft, akTop, akRight, akBottom]
        object GridModContareID_GEST_TIP_MATERIAL: TdxDBGridImageColumn
          Alignment = taLeftJustify
          Caption = 'Tip Docum.'
          HeaderAlignment = taCenter
          MinWidth = 16
          Width = 84
          BandIndex = 0
          RowIndex = 0
          FieldName = 'ID_GEST_TIP_MATERIAL'
          ShowDescription = True
        end
        object GridModContareCONT_DEBITOR: TdxDBGridMaskColumn
          Caption = 'Debitor'
          HeaderAlignment = taCenter
          Width = 67
          BandIndex = 0
          RowIndex = 0
          FieldName = 'CONT_DEBITOR'
        end
        object GridModContareCONT_CREDITOR: TdxDBGridMaskColumn
          Caption = 'Creditor'
          HeaderAlignment = taCenter
          Width = 89
          BandIndex = 0
          RowIndex = 0
          FieldName = 'CONT_CREDITOR'
        end
        object GridModContareCOD_ECONOMIC: TdxDBGridMaskColumn
          Caption = 'Cod Economic'
          Width = 90
          BandIndex = 0
          RowIndex = 0
          FieldName = 'COD_ECONOMIC'
        end
        object GridModContareCOD_FUNCTIONAL: TdxDBGridMaskColumn
          Caption = 'Cod Functional'
          Visible = False
          Width = 160
          BandIndex = 0
          RowIndex = 0
          FieldName = 'COD_FUNCTIONAL'
        end
      end
      object edContDebitor: TdxPopupEdit
        Left = 345
        Top = 21
        Width = 145
        TabOrder = 2
        OnChange = edContDebitorChange
        PopupControl = TreePlan
        PopupFormBorderStyle = pbsSysPanel
        PopupMinWidth = 300
        PopupWidth = 300
        OnCloseUp = edContDebitorCloseUp
      end
      object edContCreditor: TdxPopupEdit
        Left = 501
        Top = 21
        Width = 142
        TabOrder = 3
        OnChange = edContDebitorChange
        PopupControl = TreePlan
        PopupFormBorderStyle = pbsSysPanel
        PopupMinWidth = 300
        PopupWidth = 300
        OnCloseUp = edContCreditorCloseUp
      end
      object edFormulaNota: TdxButtonEdit
        Left = 346
        Top = 59
        Width = 296
        TabOrder = 4
        OnChange = edContDebitorChange
        Buttons = <
          item
            Default = True
          end>
        ExistButtons = True
      end
      object edClasificatieEcNota: TdxButtonEdit
        Left = 345
        Top = 98
        Width = 296
        TabOrder = 5
        Buttons = <
          item
            Default = True
          end>
        OnButtonClick = edClasificatieEcNotaButtonClick
        ExistButtons = True
      end
      object edTipuriMaterial: TdxImageEdit
        Left = 8
        Top = 24
        Width = 329
        TabOrder = 6
        OnChange = edTipuriMaterialChange
      end
    end
    object tabEvolutieDocument: TTabSheet
      Caption = 'Mod Tratare Pozitie Din Document'
      ImageIndex = 3
      object gridPozitiiDocum: TdxDBGrid
        Left = 8
        Top = 64
        Width = 673
        Height = 257
        SearchType = stStart
        Bands = <
          item
          end>
        DefaultLayout = True
        HeaderPanelRowCount = 1
        SummaryGroups = <>
        SummarySeparator = ', '
        TabOrder = 0
        Filter.Criteria = {00000000}
        LookAndFeel = lfFlat
      end
      object Panel1: TPanel
        Left = 0
        Top = 0
        Width = 692
        Height = 57
        Align = alTop
        BevelInner = bvLowered
        TabOrder = 1
      end
    end
  end
  object edtEsteActiv: TcxDBCheckBox
    Left = 12
    Top = 95
    Caption = 'Este Activ ?'
    DataBinding.DataField = 'STARE'
    DataBinding.DataSource = frmData.DTDocumente
    Properties.ImmediatePost = True
    Properties.NullStyle = nssUnchecked
    TabOrder = 9
    Width = 81
  end
  object edCod: TcxDBTextEdit
    Left = 130
    Top = 18
    DataBinding.DataField = 'COD_DOCUM'
    DataBinding.DataSource = frmData.DTDocumente
    TabOrder = 0
    Width = 71
  end
  object edNume: TcxDBTextEdit
    Left = 208
    Top = 18
    DataBinding.DataField = 'DEN_DOCUM'
    DataBinding.DataSource = frmData.DTDocumente
    TabOrder = 1
    Width = 273
  end
  object edDescriere: TcxDBMemo
    Left = 488
    Top = 19
    Anchors = [akLeft, akTop, akRight]
    DataBinding.DataField = 'DESC_DOCUM'
    DataBinding.DataSource = frmData.DTDocumente
    TabOrder = 2
    Height = 95
    Width = 220
  end
  object edSuportaFiliala: TcxDBCheckBox
    Left = 12
    Top = 74
    Caption = 'Document suportat la filiala'
    DataBinding.DataField = 'SUPORTA_FILIALA'
    DataBinding.DataSource = frmData.DTDocumente
    Properties.NullStyle = nssUnchecked
    TabOrder = 3
    Width = 157
  end
  object ChkComplementare: TcxDBCheckBox
    Left = 176
    Top = 74
    Caption = 'Complementeaza tipul de primitor in functie de predator'
    DataBinding.DataField = 'COMPLEMENTEAZA_GEST'
    DataBinding.DataSource = frmData.DTDocumente
    Properties.NullStyle = nssUnchecked
    TabOrder = 7
    OnClick = ChkComplementeazaPredatorClick
    Width = 289
  end
  object edPredator: TcxDBImageComboBox
    Left = 130
    Top = 46
    DataBinding.DataField = 'TIP_PREDATOR'
    DataBinding.DataSource = frmData.DTDocumente
    Properties.Images = Imagini
    Properties.Items = <
      item
        Description = 'Nu accepta'
        ImageIndex = 0
        Value = 0
      end
      item
        Description = 'Accepta Intern'
        ImageIndex = 1
        Value = 1
      end
      item
        Description = 'Accepta Extern'
        ImageIndex = 2
        Value = 2
      end
      item
        Description = 'Accepta tot'
        ImageIndex = 3
        Value = 3
      end>
    Properties.OnChange = edPredatorPropertiesChange
    TabOrder = 10
    Width = 175
  end
  object edPrimitor: TcxDBImageComboBox
    Left = 306
    Top = 46
    DataBinding.DataField = 'TIP_PRIMITOR'
    DataBinding.DataSource = frmData.DTDocumente
    Properties.Images = Imagini
    Properties.Items = <
      item
        Description = 'Nu accepta'
        ImageIndex = 0
        Value = 0
      end
      item
        Description = 'Accepta Intern'
        ImageIndex = 1
        Value = 1
      end
      item
        Description = 'Accepta Extern'
        ImageIndex = 2
        Value = 2
      end
      item
        Description = 'Accepta tot'
        ImageIndex = 3
        Value = 3
      end>
    Properties.OnChange = edPredatorPropertiesChange
    TabOrder = 11
    Width = 175
  end
  object BtnOk: TcxButton
    Left = 555
    Top = 490
    Width = 75
    Height = 25
    Anchors = [akRight, akBottom]
    Caption = 'Ok'
    ModalResult = 1
    TabOrder = 4
  end
  object BtnCancel: TcxButton
    Left = 635
    Top = 490
    Width = 75
    Height = 25
    Anchors = [akRight, akBottom]
    Cancel = True
    Caption = 'Cancel'
    ModalResult = 2
    TabOrder = 5
  end
  object Imagini: TImageList
    Left = 520
    Top = 40
    Bitmap = {
      494C010104000900080010001000FFFFFFFFFF10FFFFFFFFFFFFFFFF424D3600
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
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000D6000000D6000000A50000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000CE000000D6000000DE000000DE000000D6000000CE000000AD000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000ADAD0000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000031310000BDBD0000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      CE001010E7003131FF002121FF002121FF001818F7000000DE000000D6000000
      AD00000000000000000000000000000000000000000000000000000000000000
      000000000000A5A5000052520000ADAD00000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000039390000B5B5000042420000C6C600000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000004A4A0000E7E70000E7E70000E7E70000E7E70000E7E70000CECE
      00000000000000000000000000000000000000000000000000000808D6004A4A
      FF00B5B5FF00ADADFF003939FF002121FF002929FF005252FF009C9CFF001818
      F7000000AD000000000000000000000000000000000000000000000000000000
      00009C9C00005A5A0000E7DE0000BDAD000031310000DEDE0000DEDE0000DEDE
      0000DEDE0000DEDE0000D6D60000000000000000000000000000DEDE0000DEDE
      0000DEDE0000DEDE00009C9C000042390000F7E70000BDBD000039390000C6C6
      00000000000000000000000000000000000000000000000000009C9C42008484
      000042420000D6B51800FFA56300FFBD8400FFCE9C00FFCE9400FFB57300FFA5
      5200CECE000000000000000000000000000000000000000000001818EF006B6B
      FF00CECEFF00EFF7FF00ADADF7004A4AFF007B7BFF00DEE7FF00E7E7FF006B6B
      FF000000C6000000000000000000000000000000000000000000000000009494
      000063630000E7BD2100EF9C4A008C7B00000808000029290000292900002929
      0000292900002929000021210000000000000000000010100000292900002929
      000029290000292900001818000029290000C6A51000F79C5200BDBD00003939
      0000C6C600000000000000000000000000000000000000000000B5A58C00E7C6
      5A00CEAD1800FFA57300DEA594007B6B6B007B736B007B736B00947B7300FFBD
      9C00E7945200000000000000000000000000000000001818D6003939FF004A4A
      FF008C8CFF00E7E7FF00EFEFFF00BDC6FF00DEE7FF00EFF7FF00ADADFF003131
      FF000000D6000000A500000000000000000000000000000000008C8C00006B6B
      0000C6CE0000BDB50000B58C1000A59C0000949C0000CEB50000CECE0000CECE
      0000CECE0000BDA50000000000000000000000000000847B2100CEBD0000CECE
      0000CECE0000CEC60000BDAD0000949C0000AD940000BD8C1000BDCE0000BDBD
      000039390000BDBD000000000000000000000000000000000000B5A59400FFB5
      8C00FF9C6300BD7B630029211800000000000000000000000000101010003121
      210029181000000000000000000000000000000000002121EF005252FF006363
      FF006B6BFF00A5ADFF00F7FFFF00F7F7FF00EFEFFF00ADADFF005252FF002121
      FF001818F7000000CE00000000000000000000000000000000009C8C5A00C6AD
      5200B5BD0000AD940000AD840000AD840000AD8C0000BD940000BD9C0000C69C
      0800F79C5A00B5BD0000000000000000000000000000A58C6B00FFB57300E79C
      3900BD9C0000BD9C0000B5940000AD8C0000AD840000AD840000ADAD0000BDC6
      1000BD947B00737B390000000000000000000000000000000000B5A59400FFC6
      AD00FFBDA500EFB59C008C6B6300000000000000000000000000000000000000
      000000000000000000000000000000000000000000002929EF006363FF006B6B
      FF007373FF00ADADFF00FFFFFF00FFFFFF00E7E7FF008484FF004242FF002121
      FF002121FF000000CE000000000000000000000000000000000000000000AD8C
      6300EFAD6B00D6B55200ADA50000AD940000AD9C0000ADAD0000ADAD0000ADAD
      0000BDAD18009CB50000000000000000000000000000A58C7B00FFC66B00DEBD
      1000ADAD0000ADAD0000ADA50000AD940000AD9C0000B5AD1000EFB57B00DEA5
      5A00736B63000000000000000000000000000000000000000000000000004231
      2900423129004231290029212100000000000000000000000000000000000000
      000000000000000000000000000000000000000000003131EF006B6BFF007B73
      FF00B5A5FF00E7E7FF00F7F7FF00E7E7FF00F7F7FF00BDC6FF005252FF002121
      FF002121FF000000CE0000000000000000000000000000000000000000000000
      0000AD846B00F7AD8400D6B552008C840000393908006B7B39006B7B39006B7B
      39006B7B39006B7339003942390000000000000000006B6B5A00847B5A007B7B
      39006B7B39006B7B39006B7339007B7B2900B5A51800EFBD7B00EF9C84007363
      5200000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000005A5A3900B5B50000B5B5
      0000B5B50000ADAD0000000000000000000000000000000000004242EF009494
      FF00E7E7FF00F7F7FF00CECEFF009C9CFF00DED6FF00EFEFFF00A59CFF003131
      FF000808D6000000000000000000000000000000000000000000000000000000
      000000000000B5846B00E7AD63007B8400000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000006B733900BDB51800E79C84007B6352000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000000000006B6B0000EFDE
      0800FFB54A00EFCE6B00000000000000000000000000000000002929EF008484
      FF00C6C6FF00CECEFF009494FF007373FF009C9CFF00D6D6FF009494FF002929
      FF000000C6000000000000000000000000000000000000000000000000000000
      00000000000000000000A5845A00A57B39000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000007B735200CE9C52007B635200000000000000
      0000000000000000000000000000000000000000000000000000000000006363
      2100A5A500006B6B0000000000000000000000000000181800009C940800F7AD
      4A00FFA58400EFBDA50000000000000000000000000000000000000000004242
      F7008484FF008C8CFF007373FF006B6BFF006B6BFF007B7BFF005252FF000808
      D600000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000007B6352002129210000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000063635A007B63520000000000000000000000
      000000000000000000000000000000000000000000000000000000000000AD9C
      7300FFCE5A00DEAD29009C9C00009C9C00009C9C0000AD9C0800F7AD4A00EF94
      7300734A4200EFBDA50000000000000000000000000000000000000000000000
      00002929EF004242F7006B6BFF006363FF004A4AFF001818EF001818D6000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000D6A59400FFBD7B00FFCE5200FFCE5200FFCE5200FFC65A00E79C84005A39
      3100000000009C847B0000000000000000000000000000000000000000000000
      000000000000000000002929EF002929EF001818CE0000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000BD9C9400BD948C00BD948C00BD948C00B59484008C736B000000
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
      000000000000000000000000FFFFFF0000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000FFFFFFFFFFFFFFFFFC7FFFFFFFFFFFFF
      F01FFC7FFE3FFC0FE00FF87FFE1FE807C007F001C00FC003C007E0018007C003
      8003C0018003C1C38003C0018003C0FF8003E0018007E1C38003F001800FFF81
      C007F87FFE1FE1C1C007FC7FFE3FE001E00FFE7FFE7FE001F01FFFFFFFFFF00B
      FC7FFFFFFFFFF81FFFFFFFFFFFFFFFFF00000000000000000000000000000000
      000000000000}
  end
  object Enabled: TImageList
    Left = 488
    Top = 40
    Bitmap = {
      494C010102000400080010001000FFFFFFFFFF10FFFFFFFFFFFFFFFF424D3600
      0000000000003600000028000000400000001000000001002000000000000010
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000EFEFEF00C6C6C600A5A5A500A5A5A500ADADAD00D6D6D600FFFF
      FF00000000000000000000000000000000000000000000000000000000000000
      000000000000000000004ADE63008CB594000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000FFFF
      FF00A5A5B5004A4A8C002929840029298C002929840029297300393963007B7B
      8400DEDEDE000000000000000000000000000000000000000000000000000000
      000000000000317B6B0000DE2900107B4A000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000F7F7F7007B7B
      AD002121A5002121AD002121AD002121AD002121AD002121A5002121A5002929
      7B005A5A6300CECECE0000000000000000000000000000000000000000000000
      00002139940000D6310000D6210000AD29002129AD0000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000FFFFFF007373B5002121
      B5002121B5002121BD002121BD002121BD002121BD002121B5002121AD002121
      A500292984005A5A6300DEDEDE00000000000000000000000000000000002129
      AD0000B5390000DE290000DE290000B518001852840000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000A5A5C6002121BD002929
      BD00ADADE7007B7BE7002121CE002121CE002121C6006363CE00C6C6EF004242
      BD002121AD0029297B008C8C8C000000000000000000000000002129B500089C
      390000CE210008D6290029E75A0000B51800087B420000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000E7E7F7004242C6002121C6004242
      BD00E7E7DE00FFFFFF007B7BE7002121CE006363D600EFEFF700FFFFFF007373
      CE002121B5002929A50052526B000000000000000000395AA500009C290000BD
      180008CE210018D639004AE76B0000D62100009C1000C6DEC600000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000A5A5EF002929CE002121D6002121
      DE006363BD00EFEFDE00FFFFFF00ADADF700EFEFF700FFFFF7008C8CCE002929
      CE002121C6002121BD0039398400000000009CA5E70010BD290000B5100008C6
      210018C63900B5E7B5009CEFAD0039E75A0000A510004AAD4A00000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000006B6BE7002929D6002929DE002121
      EF002121E7006B6BCE00F7F7EF00FFFFFF00FFFFFF008C8CDE002121D6002121
      DE002121D6002121CE0031319400000000000000000042948C0039D6520021B5
      42002142BD0000000000E7F7E7005AEF730000C61800008C08001831A5000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000006363E7002929E7002929EF002929
      F7002121EF00525AE700F7F7F700FFFFFF00FFFFFF006B6BEF002121E7002121
      E7002121DE002929D60031319C0000000000000000003139D600318C8C002931
      EF00000000000000000000000000A5F7B50039DE5200009C0800085A31000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000008484F7003131EF003131FF002929
      FF005A5AEF00EFEFEF00FFFFF700C6C6DE00EFEFEF00FFFFFF007373F7002121
      EF002929E7002929DE0042429C00000000000000000000000000000000000000
      0000000000000000000000000000BDC6D6006BE7730008BD1800007300001039
      8C00000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000C6C6FF004242F7004A4AFF005252
      FF00E7E7EF00FFFFF7008484D6002121EF006B6BC600EFEFDE00FFFFFF007373
      F7002121F7002929E7006B6BA500000000000000000000000000000000000000
      0000000000000000000000000000000000006394A5004ADE520000940000005A
      08002129D6000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000F7F7FF007373FF005A5AFF006B6B
      FF00C6C6CE008C8CDE001818FF002121FF002121FF006B6BCE00BDBDBD006363
      F7003131FF003939CE00C6C6C600000000000000000000000000000000000000
      0000000000000000000000000000000000000000000063C67B0029BD31000073
      0000084A42003939CE0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000DEDEFF005A5AFF007373
      FF007B7BFF007373FF005A5AFF004A4AFF005252FF005A5AFF005A5AFF005A5A
      FF003939FF009494BD0000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000005A6BEF005AD6630008A5
      0800005A00004A6B5A0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000CECEFF006B6B
      FF008C8CFF00A5A5FF00A5A5FF009C9CFF009494FF008484FF007373FF005252
      FF009494C600FFFFFF0000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000000000006B9CC6005ADE
      5A00009C0000005A0000B5CEB500000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000E7E7
      FF008484FF008484FF009C9CFF00A5A5FF009494FF007373FF006B6BFF00B5B5
      DE00000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000008CCE
      9C0039CE39004AC64A00EFFFEF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000F7F7FF00CECEFF009C9CFF009494FF009494FF00BDBDFF00E7E7FF000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000FFFF
      FF00A5E7A500F7FFF70000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000424D3E000000000000003E000000
      2800000040000000100000000100010000000000800000000000000000000000
      000000000000000000000000FFFFFF00F80FFCFF00000000E007F8FF00000000
      C003F07F000000008001E07F000000008001C07F000000000001803F00000000
      0001003F000000000001841F0000000000018E1F000000000001FE0F00000000
      0001FF07000000000001FF83000000008003FF8300000000C003FFC100000000
      E00FFFE100000000F01FFFE30000000000000000000000000000000000000000
      000000000000}
  end
  object DTTemplateCrid: TDataSource
    DataSet = QryFieldItemsi
    Left = 568
    Top = 40
  end
  object Color: TColorDialog
    Left = 520
    Top = 72
  end
  object QryValidari: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'SELECT * FROM GEST_TEMPLATE_VALIDARI'
      'WHERE ID_GEST_DEFA_DOCUM = :ID')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 600
    Top = 72
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object DTValidari: TDataSource
    DataSet = QryValidari
    Left = 568
    Top = 72
  end
  object DTTipuriMateriale: TDataSource
    DataSet = QryTipuriMateriale
    Left = 568
    Top = 104
  end
  object QryTipuriMateriale: TZQuery
    Connection = frmData.dbContabilitate
    AfterOpen = QryTipuriMaterialeAfterOpen
    OnNewRecord = QryTipuriMaterialeNewRecord
    SQL.Strings = (
      'SELECT * FROM GEST_ITEMSI_TIP_MATERIAL'
      'WHERE ID_GEST_DEFA_DOCUM = :ID')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 600
    Top = 104
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object DTModContare: TDataSource
    DataSet = QryModContare
    Left = 488
    Top = 104
  end
  object QryModContare: TZQuery
    Connection = frmData.dbContabilitate
    AfterOpen = QryModContareAfterOpen
    OnNewRecord = QryTipuriMaterialeNewRecord
    SQL.Strings = (
      'SELECT * FROM GEST_DEFA_NOTA_CONT'
      'WHERE ID_GEST_DEFA_DOCUM = :ID')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 520
    Top = 104
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object QryFieldItemsi: TZQuery
    Connection = frmData.dbContabilitate
    OnNewRecord = QryFieldItemsiNewRecord
    SQL.Strings = (
      'SELECT * FROM GEST_DEFA_DOCUM_ITEMSI'
      'WHERE ID_GEST_DEFA_DOCUM = :ID')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 600
    Top = 40
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object QryFieldDocum: TZQuery
    Tag = 1
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'SELECT * FROM GEST_DEFA_DOCUM_DOCUMENT'
      'WHERE ID_GEST_DEFA_DOCUM = :ID')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 632
    Top = 40
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object ppCopiereMenu: TPopupMenu
    Left = 455
    Top = 108
    object CmdCampuriLipsa: TMenuItem
      Caption = 'Campuri Lipsa'
      OnClick = CmdCampuriLipsaClick
    end
  end
  object qryTipProduse: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'select * from GEST_DEFA_ITEMSI_TIP_PRODUSE'
      'where id_gest_defa_docum = :ID')
    Params = <
      item
        DataType = ftUnknown
        Name = 'ID'
        ParamType = ptUnknown
        Size = -1
      end>
    Left = 520
    Top = 143
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'ID'
        ParamType = ptUnknown
        Size = -1
      end>
  end
  object DTReportList: TDataSource
    DataSet = QryReportList
    Left = 226
    Top = 95
  end
  object QryReportList: TZQuery
    Tag = 1
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'EXEC SP_GET_REPORT_LIST_CONFIG')
    Params = <>
    Left = 259
    Top = 94
  end
end
