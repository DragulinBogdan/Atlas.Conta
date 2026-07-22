object frmCompensari: TfrmCompensari
  Left = 324
  Top = 107
  Caption = 'Compensari terti'
  ClientHeight = 568
  ClientWidth = 804
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  OnClose = FormClose
  OnCreate = FormCreate
  OnShow = FormShow
  PixelsPerInch = 96
  TextHeight = 13
  object Panel2: TPanel
    Left = 0
    Top = 416
    Width = 804
    Height = 152
    Align = alBottom
    BevelOuter = bvNone
    TabOrder = 0
    ExplicitTop = 379
    object GroupBox3: TGroupBox
      Left = 0
      Top = 0
      Width = 804
      Height = 152
      Align = alClient
      Caption = 'Lista decontarilor efectuate'
      TabOrder = 0
      object GridImperecheri: TdxDBGrid
        Left = 2
        Top = 15
        Width = 800
        Height = 135
        SearchType = stContain
        Bands = <
          item
            Caption = 'Furnizor'
            Width = 380
          end
          item
            Caption = 'Suma'
            Width = 35
          end
          item
            Caption = 'Client'
            Width = 371
          end>
        DefaultLayout = False
        HeaderPanelRowCount = 1
        KeyField = 'ID_COMPENSARE'
        ShowSummaryFooter = True
        SummaryGroups = <>
        SummarySeparator = ', '
        Align = alClient
        PopupMenu = ppDecontari
        TabOrder = 0
        OnDblClick = GridImperecheriDblClick
        DataSource = DTImperecheri
        Filter.Active = True
        Filter.Criteria = {00000000}
        LookAndFeel = lfFlat
        OptionsBehavior = [edgoAutoSearch, edgoAutoSort]
        OptionsDB = [edgoCanNavigation, edgoLoadAllRecords, edgoUseBookmarks]
        OptionsView = [edgoAutoWidth, edgoBandHeaderWidth, edgoIndicator, edgoInvertSelect, edgoUseBitmap]
        ShowBands = True
        OnCustomDraw = GridImperecheriCustomDraw
        object GridImperecheriFURNIZOR_COD_DOCUM: TdxDBGridMaskColumn
          Caption = 'Tip Doc'
          HeaderAlignment = taCenter
          Width = 48
          BandIndex = 0
          RowIndex = 0
          FieldName = 'FURNIZOR_COD_DOCUM'
        end
        object GridImperecheriFURNIZOR_NR_DOCUM: TdxDBGridMaskColumn
          Caption = 'Nr Doc'
          DisableFilter = True
          HeaderAlignment = taCenter
          Width = 61
          BandIndex = 0
          RowIndex = 0
          FieldName = 'FURNIZOR_NR_DOCUM'
        end
        object GridImperecheriFURNIZOR_DATA_DOCUM: TdxDBGridDateColumn
          Caption = 'Data Doc'
          DisableFilter = True
          HeaderAlignment = taCenter
          Width = 70
          BandIndex = 0
          RowIndex = 0
          FieldName = 'FURNIZOR_DATA_DOCUM'
        end
        object GridImperecheriFURNIZOR_PREDATOR: TdxDBGridMaskColumn
          Caption = 'Predator'
          HeaderAlignment = taCenter
          Width = 67
          BandIndex = 0
          RowIndex = 0
          FieldName = 'FURNIZOR_PREDATOR'
        end
        object GridImperecheriFURNIZOR_PRIMITOR: TdxDBGridMaskColumn
          Caption = 'Primitor'
          HeaderAlignment = taCenter
          Width = 55
          BandIndex = 0
          RowIndex = 0
          FieldName = 'FURNIZOR_PRIMITOR'
        end
        object GridImperecheriFURNIZOR_ID_GEST_DOCUM: TdxDBGridMaskColumn
          Caption = 'Id Document'
          Visible = False
          Width = 87
          BandIndex = 0
          RowIndex = 0
          FieldName = 'FURNIZOR_ID_GEST_DOCUM'
        end
        object GridImperecheriSUMA: TdxDBGridCurrencyColumn
          Caption = 'Decontat'
          DisableFilter = True
          HeaderAlignment = taCenter
          Width = 35
          BandIndex = 1
          RowIndex = 0
          FieldName = 'SUMA'
          SummaryFooterType = cstSum
          SummaryFooterField = 'SUMA'
          SummaryFooterFormat = ',0.00;-,0.00'
          Nullable = False
        end
        object GridImperecheriSTARE: TdxDBGridImageColumn
          Alignment = taLeftJustify
          Caption = 'Stare'
          HeaderAlignment = taCenter
          MinWidth = 16
          Width = 70
          BandIndex = 1
          RowIndex = 0
          FieldName = 'STARE'
          Descriptions.Strings = (
            'Doc Invalid'
            'Valida'
            'Casa Invalid')
          ImageIndexes.Strings = (
            '0'
            '1'
            '2')
          ShowDescription = True
          Values.Strings = (
            '0'
            '1'
            '2')
        end
        object GridImperecheriCLIENT_COD_DOCUM: TdxDBGridMaskColumn
          Caption = 'Tip Doc'
          HeaderAlignment = taCenter
          Width = 48
          BandIndex = 2
          RowIndex = 0
          FieldName = 'CLIENT_COD_DOCUM'
        end
        object GridImperecheriCLIENT_NR_DOCUM: TdxDBGridMaskColumn
          Caption = 'Nr Doc'
          DisableFilter = True
          HeaderAlignment = taCenter
          Width = 61
          BandIndex = 2
          RowIndex = 0
          FieldName = 'CLIENT_NR_DOCUM'
        end
        object GridImperecheriCLIENT_DATA_DOCUM: TdxDBGridDateColumn
          Caption = 'Data Doc'
          DisableFilter = True
          HeaderAlignment = taCenter
          Width = 70
          BandIndex = 2
          RowIndex = 0
          FieldName = 'CLIENT_DATA_DOCUM'
        end
        object GridImperecheriCLIENT_PREDATOR: TdxDBGridMaskColumn
          Caption = 'Predator'
          HeaderAlignment = taCenter
          Width = 67
          BandIndex = 2
          RowIndex = 0
          FieldName = 'CLIENT_PREDATOR'
        end
        object GridImperecheriCLIENT_PRIMITOR: TdxDBGridMaskColumn
          Caption = 'Primitor'
          HeaderAlignment = taCenter
          Width = 55
          BandIndex = 2
          RowIndex = 0
          FieldName = 'CLIENT_PRIMITOR'
        end
        object GridImperecheriCLIENT_ID_GEST_DOCUM: TdxDBGridMaskColumn
          Caption = 'Id Document'
          Visible = False
          Width = 87
          BandIndex = 2
          RowIndex = 0
          FieldName = 'CLIENT_ID_GEST_DOCUM'
        end
      end
    end
  end
  object TabList: TcxTabControl
    Left = 0
    Top = 346
    Width = 804
    Height = 25
    Align = alBottom
    TabOrder = 1
    Properties.CustomButtons.Buttons = <>
    Properties.HotTrack = True
    Properties.NavigatorPosition = npLeftTop
    Properties.Style = 5
    Properties.TabIndex = 0
    Properties.TabPosition = tpBottom
    Properties.Tabs.Strings = (
      'Furnizor -> Client'
      'Client  -> Furnizor')
    OnChange = TabListChange
    ExplicitTop = 309
    ClientRectBottom = 1
    ClientRectRight = 804
    ClientRectTop = 0
  end
  object Panel3: TPanel
    Left = 0
    Top = 0
    Width = 804
    Height = 346
    Align = alClient
    BevelInner = bvRaised
    BevelOuter = bvLowered
    Caption = 'Panel3'
    TabOrder = 2
    OnResize = Panel3Resize
    ExplicitHeight = 309
    object GrCasa: TGroupBox
      Left = 2
      Top = 2
      Width = 800
      Height = 151
      Align = alTop
      Caption = 'Furnizori'
      TabOrder = 0
      object GridFurnizori: TdxDBGrid
        Left = 2
        Top = 15
        Width = 796
        Height = 134
        SearchType = stContain
        Bands = <
          item
          end>
        DefaultLayout = True
        HeaderPanelRowCount = 1
        KeyField = 'ID_GEST_DOCUM'
        ShowSummaryFooter = True
        SummaryGroups = <>
        SummarySeparator = ', '
        Align = alClient
        TabOrder = 0
        OnDblClick = GridFurnizoriDblClick
        DataSource = DTFurnizori
        Filter.Active = True
        Filter.Criteria = {00000000}
        LookAndFeel = lfFlat
        OptionsBehavior = [edgoAutoSearch, edgoAutoSort]
        OptionsDB = [edgoCanNavigation, edgoLoadAllRecords, edgoUseBookmarks]
        OptionsView = [edgoAutoWidth, edgoBandHeaderWidth, edgoInvertSelect, edgoUseBitmap]
        PreviewFieldName = 'COD'
        PreviewLines = 1
        OnChangeNode = GridFurnizoriChangeNode
        OnCustomDrawPreviewCell = GridFurnizoriCustomDrawPreviewCell
        OnFilterChanged = GridFurnizoriFilterChanged
        object GridFurnizoriCOD_DOCUM: TdxDBGridMaskColumn
          Caption = 'Tip Doc'
          HeaderAlignment = taCenter
          BandIndex = 0
          RowIndex = 0
          FieldName = 'COD_DOCUM'
        end
        object GridFurnizoriNR_DOCUM: TdxDBGridMaskColumn
          Caption = 'Nr'
          DisableFilter = True
          HeaderAlignment = taCenter
          Width = 60
          BandIndex = 0
          RowIndex = 0
          FieldName = 'NR_DOCUM'
        end
        object GridFurnizoriDATA_DOCUM: TdxDBGridDateColumn
          Caption = 'Data'
          HeaderAlignment = taCenter
          Sorted = csUp
          Width = 60
          BandIndex = 0
          RowIndex = 0
          FieldName = 'DATA_DOCUM'
        end
        object GridFurnizoriTOTALDOC: TdxDBGridCurrencyColumn
          Caption = 'Total'
          DisableFilter = True
          HeaderAlignment = taCenter
          Width = 80
          BandIndex = 0
          RowIndex = 0
          FieldName = 'TOTALDOC'
          SummaryFooterType = cstSum
          SummaryFooterField = 'TOTALDOC'
          SummaryFooterFormat = ',0.00;-,0.00'
          Nullable = False
        end
        object GridFurnizoriASIGNAT: TdxDBGridCurrencyColumn
          Caption = 'Asignat'
          DisableFilter = True
          HeaderAlignment = taCenter
          Width = 80
          BandIndex = 0
          RowIndex = 0
          FieldName = 'ASIGNAT'
          SummaryFooterType = cstSum
          SummaryFooterField = 'ASIGNAT'
          SummaryFooterFormat = ',0.00;-,0.00'
          Nullable = False
        end
        object GridFurnizoriPROCENT: TdxDBGridColumn
          Caption = 'Proc. %'
          DisableEditor = True
          HeaderAlignment = taCenter
          MinWidth = 120
          Width = 120
          BandIndex = 0
          RowIndex = 0
          OnCustomDrawCell = GridFurnizoriPROCENTCustomDrawCell
          FieldName = 'PROCENT'
          OnGetText = GridFurnizoriPROCENTGetText
        end
        object GridFurnizoriPREDATOR: TdxDBGridMaskColumn
          Caption = 'Predator'
          HeaderAlignment = taCenter
          Width = 100
          BandIndex = 0
          RowIndex = 0
          FieldName = 'PREDATOR'
        end
        object GridFurnizoriPRIMITOR: TdxDBGridMaskColumn
          Caption = 'Primitor'
          HeaderAlignment = taCenter
          Width = 100
          BandIndex = 0
          RowIndex = 0
          FieldName = 'PRIMITOR'
        end
        object GridFurnizoriPREDATOR_INTERN: TdxDBGridCheckColumn
          Caption = 'Pred. Intern'
          HeaderAlignment = taCenter
          Visible = False
          Width = 100
          BandIndex = 0
          RowIndex = 0
          FieldName = 'PREDATOR_INTERN'
          ValueChecked = 'True'
          ValueUnchecked = 'False'
        end
        object GridFurnizoriPRIMITOR_INTERN: TdxDBGridCheckColumn
          Caption = 'Prim. Intern'
          HeaderAlignment = taCenter
          Visible = False
          Width = 100
          BandIndex = 0
          RowIndex = 0
          FieldName = 'PRIMITOR_INTERN'
          ValueChecked = 'True'
          ValueUnchecked = 'False'
        end
        object GridFurnizoriID_PREDATOR: TdxDBGridMaskColumn
          Caption = 'Id Predator'
          HeaderAlignment = taCenter
          Visible = False
          BandIndex = 0
          RowIndex = 0
          FieldName = 'ID_PREDATOR'
        end
        object GridFurnizoriID_PRIMITOR: TdxDBGridMaskColumn
          Caption = 'Id Primitor'
          HeaderAlignment = taCenter
          Visible = False
          BandIndex = 0
          RowIndex = 0
          FieldName = 'ID_PRIMITOR'
        end
        object GridFurnizoriRAMAS: TdxDBGridCurrencyColumn
          Visible = False
          BandIndex = 0
          RowIndex = 0
          FieldName = 'RAMAS'
          Nullable = False
        end
      end
    end
    object GrDocumente: TGroupBox
      Left = 2
      Top = 161
      Width = 800
      Height = 183
      Align = alClient
      Caption = 'Clienti'
      TabOrder = 1
      ExplicitHeight = 146
      object GridClienti: TdxDBGrid
        Left = 2
        Top = 15
        Width = 796
        Height = 154
        SearchType = stContain
        Bands = <
          item
          end>
        DefaultLayout = True
        HeaderPanelRowCount = 1
        KeyField = 'ID_GEST_DOCUM'
        ShowSummaryFooter = True
        SummaryGroups = <>
        SummarySeparator = ', '
        Align = alTop
        TabOrder = 0
        OnDblClick = GridClientiDblClick
        DataSource = DTClienti
        Filter.Active = True
        Filter.Criteria = {00000000}
        LookAndFeel = lfFlat
        OptionsBehavior = [edgoAutoSearch, edgoAutoSort]
        OptionsDB = [edgoCanNavigation, edgoLoadAllRecords, edgoUseBookmarks]
        OptionsView = [edgoAutoWidth, edgoBandHeaderWidth, edgoInvertSelect, edgoUseBitmap]
        PreviewFieldName = 'COD_DOCUM'
        PreviewLines = 1
        OnChangeNode = GridClientiChangeNode
        OnCustomDrawPreviewCell = GridClientiCustomDrawPreviewCell
        Anchors = [akLeft, akTop, akRight, akBottom]
        OnFilterChanged = GridClientiFilterChanged
        ExplicitHeight = 117
        object GridClientiCOD_DOCUM: TdxDBGridMaskColumn
          Caption = 'Tip Doc'
          HeaderAlignment = taCenter
          BandIndex = 0
          RowIndex = 0
          FieldName = 'COD_DOCUM'
        end
        object GridClientiNR_DOCUM: TdxDBGridMaskColumn
          Caption = 'Nr'
          DisableFilter = True
          HeaderAlignment = taCenter
          Width = 60
          BandIndex = 0
          RowIndex = 0
          FieldName = 'NR_DOCUM'
        end
        object GridClientiDATA_DOCUM: TdxDBGridDateColumn
          Caption = 'Data'
          HeaderAlignment = taCenter
          Width = 60
          BandIndex = 0
          RowIndex = 0
          FieldName = 'DATA_DOCUM'
        end
        object GridClientiTOTALDOC: TdxDBGridCurrencyColumn
          Caption = 'Total'
          DisableFilter = True
          HeaderAlignment = taCenter
          Width = 80
          BandIndex = 0
          RowIndex = 0
          FieldName = 'TOTALDOC'
          SummaryFooterType = cstSum
          SummaryFooterField = 'TOTALDOC'
          SummaryFooterFormat = ',0.00;-,0.00'
          Nullable = False
        end
        object GridClientiASIGNAT: TdxDBGridCurrencyColumn
          Caption = 'Asignat'
          DisableFilter = True
          HeaderAlignment = taCenter
          Width = 80
          BandIndex = 0
          RowIndex = 0
          FieldName = 'ASIGNAT'
          SummaryFooterType = cstSum
          SummaryFooterField = 'ASIGNAT'
          SummaryFooterFormat = ',0.00;-,0.00'
          Nullable = False
        end
        object GridClientiPROCENT: TdxDBGridColumn
          Caption = 'Proc. %'
          DisableEditor = True
          HeaderAlignment = taCenter
          MinWidth = 120
          Width = 120
          BandIndex = 0
          RowIndex = 0
          OnCustomDrawCell = GridClientiPROCENTCustomDrawCell
          FieldName = 'PROCENT'
        end
        object GridClientiPREDATOR: TdxDBGridMaskColumn
          Caption = 'Predator'
          HeaderAlignment = taCenter
          Width = 100
          BandIndex = 0
          RowIndex = 0
          FieldName = 'PREDATOR'
        end
        object GridClientiPRIMITOR: TdxDBGridMaskColumn
          Caption = 'Primitor'
          HeaderAlignment = taCenter
          Sorted = csUp
          Width = 100
          BandIndex = 0
          RowIndex = 0
          FieldName = 'PRIMITOR'
        end
        object GridClientiPREDATOR_INTERN: TdxDBGridCheckColumn
          Caption = 'Pred. Intern'
          HeaderAlignment = taCenter
          Visible = False
          Width = 100
          BandIndex = 0
          RowIndex = 0
          FieldName = 'PREDATOR_INTERN'
          ValueChecked = 'True'
          ValueUnchecked = 'False'
        end
        object GridClientiPRIMITOR_INTERN: TdxDBGridCheckColumn
          Caption = 'Prim. Intern'
          HeaderAlignment = taCenter
          Visible = False
          Width = 100
          BandIndex = 0
          RowIndex = 0
          FieldName = 'PRIMITOR_INTERN'
          ValueChecked = 'True'
          ValueUnchecked = 'False'
        end
        object GridClientiID_PREDATOR: TdxDBGridMaskColumn
          Caption = 'Id Predator'
          HeaderAlignment = taCenter
          Visible = False
          BandIndex = 0
          RowIndex = 0
          FieldName = 'ID_PREDATOR'
        end
        object GridClientiID_PRIMITOR: TdxDBGridMaskColumn
          Caption = 'Id Primitor'
          HeaderAlignment = taCenter
          Visible = False
          BandIndex = 0
          RowIndex = 0
          FieldName = 'ID_PRIMITOR'
        end
        object GridClientiRAMAS: TdxDBGridCurrencyColumn
          Visible = False
          BandIndex = 0
          RowIndex = 0
          FieldName = 'RAMAS'
          Nullable = False
        end
      end
    end
    object Splitter2: TcxSplitter
      Left = 2
      Top = 153
      Width = 800
      Height = 8
      Cursor = crVSplit
      HotZoneClassName = 'TcxMediaPlayer9Style'
      AlignSplitter = salTop
      AutoSnap = True
      Control = GrCasa
    end
  end
  object pnTools: TPanel
    Left = 0
    Top = 371
    Width = 804
    Height = 37
    Align = alBottom
    BevelOuter = bvLowered
    TabOrder = 3
    ExplicitTop = 334
    DesignSize = (
      804
      37)
    object edSuma: TcxCurrencyEdit
      Left = 230
      Top = 8
      Properties.OnChange = edSumaPropertiesChange
      Properties.OnValidate = edSumaPropertiesValidate
      TabOrder = 0
      Width = 121
    end
    object BtnAdd: TcxButton
      Left = 8
      Top = 6
      Width = 65
      Height = 25
      Caption = 'Adauga'
      Enabled = False
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        424D360400000000000036000000280000001000000010000000010020000000
        000000000000C40E0000C40E00000000000000000000F8F8F800F8F8F800F8F8
        F800F8F8F800E7E7E7FFB1B7B1FF809980FF679467FF679467FF839C83FFB3BB
        B3FFE7E8E7FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F6F6
        F6FFBDC3BDFF579058FF20B523FF09DA0EFF01E907FF01E907FF09DB0EFF1CBB
        1FFF519753FFBDC5BDFFF6F6F6FFF8F8F800F8F8F800F8F8F800F7F7F7FFAEB7
        AEFF359C38FF03E00AFF00F308FF01F609FF16E21CFF16E01CFF08EE10FF00F4
        08FF01E609FF2CA52FFFADBCAEFFF8F8F800F8F8F800F8F8F800D0D4D0FF3B96
        3CFF02DD0AFF00F408FF00F808FF0AE912FFD3DAD3FFCECECEFF45CE49FF00F8
        08FF00F508FF00E408FF2FA332FFCED5CEFFF8F8F800F5F5F5FF769C76FF06CA
        0DFF00EB08FF00F808FF00F808FF0BE912FFE7F0E7FFEAEAEAFF48CF4BFF00F8
        08FF00F808FF00EE08FF02D30AFF659F66FFF4F4F4FFDDDFDDFF309A32FF00DA
        08FF00EF08FF00F808FF00F808FF0BE912FFE7F0E7FFEAEAEAFF48CF4BFF00F8
        08FF00F808FF00F308FF00DF08FF1FA522FFD4DBD4FFB9C5B9FF15A719FF00DB
        08FF46D64AFF82C684FF80C282FF88C589FFECEEECFFF0F0F0FF9FC59FFF80C2
        82FF80C282FF68BB6CFF00DB08FF0AB60FFFA9BEA9FFB0C1B0FF0EAA13FF00D6
        08FF77DF7AFFF4F4F4FFF4F4F4FFF4F4F4FFF7F7F7FFF8F8F800F4F4F4FFF4F4
        F4FFF3F3F3FFB5C9B5FF00D307FF02B808FF90B490FFB4C4B4FF0EA213FF00CC
        08FF4FD753FFA5E4A6FFA5E6A6FFA8E2A9FFF2F5F2FFF5F5F5FFC1E5C1FFA5E6
        A6FFA5E4A6FF87D589FF00CC08FF02AF08FF93B593FFCCD5CCFF18951AFF04BF
        0CFF01D709FF00EB08FF00F508FF0BE911FFE7F0E7FFEBEBEBFF49D34DFF00F6
        08FF00EE08FF00DC08FF05C30CFF0C9E0FFFB3C6B3FFF1F2F1FF459345FF0CAE
        11FF0CC712FF00D908FF00E708FF0BE012FFE7F0E7FFEAEAEAFF48CB4BFF00E9
        08FF00DC08FF08CA0FFF10B414FF269027FFE0E6E0FFF8F8F800A6BDA6FF1296
        14FF2EBB32FF08C210FF00D008FF0BCE12FFE7EFE7FFF2F2F2FF4EC752FF00D2
        08FF05C50DFF2BBF2FFF129F14FF81AB81FFF8F8F800F8F8F800F2F4F2FF659C
        65FF2DA42FFF4AC04EFF15BA1CFF05BB0BFF23B926FF25B928FF10B915FF13BA
        16FF43BF47FF36AC39FF479347FFE8ECE8FFF8F8F800F8F8F800F8F8F800E8EC
        E8FF659C65FF45A546FF80CB81FF65C767FF49BF4CFF49BF4BFF64C665FF83CD
        85FF4EAD4FFF4D944DFFD9E2D9FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8
        F800F2F4F2FFA5BFA5FF579A57FF68AA6AFF83BD84FF84BD84FF6DAD6DFF5198
        51FF92B493FFECF0ECFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
        F800F8F8F800F8F8F800F2F4F2FFCFDBCFFFB4C8B4FFB3C8B3FFCDD9CDFFF0F3
        F0FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800}
      TabOrder = 1
      OnClick = BtnAddClick
    end
    object BtnModify: TcxButton
      Left = 81
      Top = 6
      Width = 72
      Height = 25
      Caption = 'Modifica'
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        424D360400000000000036000000280000001000000010000000010020000000
        000000000000C40E0000C40E00000000000000000000F8F8F800F8F8F800F8F8
        F800F8F8F800F5F5F5FFE2E2E2FFC9BEB7FFB8A392FFB09A89FFAFA49BFFBFBF
        BEFFDFDFDFFFF4F4F4FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
        F800EFEFEFFFCCA991FFBC6E37FFC05B13FFC75E10FFC85E10FFC15B12FFB15F
        24FFA37859FFB6B6B5FFEDEDEDFFF8F8F800F8F8F800F8F8F800F8F8F800EDE7
        E2FFC78251FFC85F10FFD76D12FFDF7715FFDE7819FFDC781AFFDE7717FFD86F
        13FFC96010FFB06229FFAAA098FFEBEBEBFFF8F8F800F8F8F800F4F3F2FFCB81
        4BFFCE6410FFD47925FFBE9D7FFFBB9F86FFB48659FFC17B33FFC97D31FFD37C
        26FFDD791AFFD06712FFB46024FFB8B3B0FFF4F4F4FFF8F8F800DDAF8EFFCA61
        10FFE37A17FFD6B99CFFF1F1F1FFE3DED7FFD5CDC3FFC5B7A8FFB8894CFFC289
        3FFFCE8532FFDE7D1EFFCD6511FFB07348FFDEDEDEFFF4F0EDFFC56826FFE078
        13FFDB8625FFE1D7CAFFD6D3CBFFBC8E48FFC19045FFBE9759FFBAA077FFC392
        46FFC89442FFD48B31FFE07E1AFFBE5C16FFC1B9B3FFE8D0BEFFC96010FFE886
        1BFFD89535FFDFD5C7FFBAB1A1FFB4955CFFCC9D47FFCE9E48FFC59845FFC49B
        4FFFCE9E48FFD39A3EFFE18C26FFCD6712FFBE977AFFE0B292FFD26811FFE68E
        22FFC7A56AFFE7E3DBFFDDDDDDFFBDA674FFD5A94AFFD5A748FFC39E50FFBEB2
        98FFCDA347FFD6A644FFE0972FFFD77114FFC28A61FFDDAB88FFD36B12FFE794
        27FFD5A541FFD4C49AFFE5E5E4FFD6C391FFDDBC67FFD8B34FFFD1B97EFFE6E6
        E6FFBEB499FFCCA84FFFE09F35FFD87416FFC78C62FFE5C3AAFFCD6612FFE898
        2DFFDBAF50FFDDC681FFD4CFC3FFEEE8D7FFE9D9ADFFDDC582FFCDC3A4FFEFEE
        ECFFC2B89BFFCCB065FFE1A33EFFD26F18FFCFA180FFF2E6DDFFC45F17FFE891
        2DFFE0B15DFFE3D19CFFE6DEC8FFE9E8E3FFF0EAD8FFE2D3A8FFCBB36DFFE6E3
        DEFFB8AF97FFDAB766FFE59E40FFC55F14FFE6D1C3FFF8F8F7FFD28E5DFFD674
        20FFE5A555FFE1CA97FFE4D9BAFFCDCAC0FFD0CBBDFFC4B892FFBCB093FFECEC
        ECFFBDB39DFFDFB067FFDA802DFFC97940FFF5F4F4FFF8F8F800F1E2D8FFC566
        22FFDD8432FFE5B175FFE0CBA3FFD9CDAEFFD4CDBFFFE3E2E2FFEDEDEDFFF2F1
        F1FFCFAF87FFDF9145FFC4611AFFEAD5C6FFF8F8F800F8F8F800F8F8F800EACD
        BAFFC5641FFFD77C31FFE5A668FFE1B78AFFDCBE9AFFD1B897FFD7BDA2FFD8AB
        7FFFD7853FFFC46019FFE3BFA5FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8
        F800EFDED1FFD08551FFC66119FFD1762EFFD7823FFFD78441FFD27934FFC663
        1BFFCC7C43FFECD5C4FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
        F800F8F8F800F8F7F7FFEDD9C9FFDEAF8DFFD18B58FFD08854FFDDAA87FFEBD3
        C0FFF7F6F5FFF8F8F800F8F8F800F8F8F800F8F8F800}
      TabOrder = 2
      OnClick = BtnModifyClick
    end
    object BtnDelete: TcxButton
      Left = 161
      Top = 6
      Width = 65
      Height = 25
      Caption = 'Sterge'
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        424D360400000000000036000000280000001000000010000000010020000000
        000000000000C40E0000C40E00000000000000000000F8F8F800F8F8F800F8F8
        F800F8F8F800F8F8F800EDEDF1FFBFBFD5FF9191B9FF8F8FB7FFBBBBD3FFEAEA
        F0FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
        F800EFEFF2FF8787B2FF303089FF12129CFF0E0EACFF0D0DADFF12129EFF2B2B
        89FF7E7EACFFE9E9EFFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800DADA
        E5FF42428BFF0D0DA9FF0101DBFF0000E9FF0000EFFF0000F0FF0000EAFF0000
        DEFF0C0CAFFF393987FFCFCFDFFFF8F8F800F8F8F800F8F8F800EAEAF0FF3E3E
        88FF0707B8FF0000E1FF0101EEFF0000F6FF0000F8FF0000F8FF0000F6FF0202
        EFFF0000E3FF0505BEFF333386FFE1E1EAFFF8F8F800F8F8F8007B7BABFF0C0C
        A4FF0000D9FF1010E9FFACACF0FF4444F1FF0101F8FF0000F8FF4343F6FFA6A6
        EDFF1010EAFF0000DAFF0909ADFF68689EFFF7F7F8FFE5E5EDFF242483FF0000
        C4FF0101DBFFB6B6F1FFF8F8F800E6E6F3FF4545F1FF4343F6FFEAEAF6FFF7F7
        F8FFAFAFEDFF0202DEFF0000C9FF1B1B84FFD9D9E5FFB4B4CEFF111190FF0000
        C8FF0000D9FF4242E0FFEDEDF6FFF8F8F800EBEBF4FFEFEFF6FFF8F8F800EEEE
        F7FF4545E8FF0000DBFF0000CAFF0E0E95FFA4A4C4FF8C8CB4FF0C0C97FF0000
        C4FF0000D4FF0000E0FF4343E4FFEFEFF7FFF8F8F800F8F8F800F0F0F7FF4747
        EBFF0000E1FF0000D5FF0000C6FF09099DFF8787B2FF8B8BB4FF0C0C93FF0000
        BDFF0000CBFF0000D7FF5050E7FFF2F2F7FFF8F8F800F8F8F800EDEDF4FF4F4F
        E2FF0101D7FF0000CDFF0000BEFF09099AFF8787B2FFB2B2CDFF111188FF0000
        B3FF0A0AC2FF5555D9FFF0F0F6FFF8F8F800EDEDF6FFEBEBF5FFF8F8F800EBEB
        F2FF5353D7FF0A0AC4FF0000B5FF0E0E8DFFA3A3C3FFE5E5ECFF23237FFF0000
        A8FF1B1BBAFFB6B6E2FFF8F8F800E8E8F5FF3A3AD5FF3737CEFFE6E6F3FFF8F8
        F800B4B4E4FF1C1CBEFF0000AAFF1C1C7FFFD9D9E4FFF8F8F8007A7AA9FF0C0C
        8EFF1919B2FF5353C6FFB1B1DFFF4545CBFF0404C0FF0303C0FF4040C4FFB1B1
        E2FF5454C9FF1C1CB4FF080893FF65659DFFF7F7F8FFF8F8F800E9E9EFFF3C3C
        86FF080895FF4F4FC0FF8282D4FF6C6CD0FF5555CBFF5454CAFF6969CFFF8282
        D5FF5353C2FF070797FF323281FFDFDFE9FFF8F8F800F8F8F800F8F8F800D8D8
        E5FF404088FF12128BFF4F4FB9FFA4A4DBFFBFBFE4FFC0C0E4FFA7A7DDFF5454
        BCFF13138FFF373784FFCDCDDDFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8
        F800EDEDF2FF8484AFFF2E2E82FF1A1A85FF2E2E95FF2F2F95FF1B1B86FF2929
        80FF7A7AAAFFE6E6EDFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
        F800F8F8F800F8F8F800EBEBF0FFBCBCD3FF9191B7FF8D8DB6FFB7B7D1FFE8E8
        EEFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800}
      TabOrder = 3
      OnClick = BtnDeleteClick
    end
    object BtnDefalcare: TcxButton
      Left = 353
      Top = 6
      Width = 79
      Height = 25
      Caption = 'Defalcare'
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        424D360400000000000036000000280000001000000010000000010020000000
        000000000000C40E0000C40E00000000000000000000FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00BDBDBDFFE7E7E7FFFFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00F7EFF7FFF7EFEFFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF006B6B73FF638484FFE7FFFFFFFFEFE7FFCEC6BDFFB5AD
        A5FFC6B5ADFFC6B5ADFFA5948CFFCEC6BDFFFFFFFF00FFFFFF00EFEFEFFFDEDE
        DEFFCEBDB5FFCEC6BDFFAD948CFF6B8484FFC6FFFFFF848C8CFFB58473FFC6B5
        ADFFDEDECEFFEFE7DEFFCEBDB5FF947B6BFFC6C6BDFFCEBDBDFFBDADADFFB5A5
        94FFC6BDB5FFD6CEC6FFD6C6BDFFB5948CFF5A736BFF42B5BDFF6B8C94FFBD84
        84FFEFDEDEFFC6BDBDFFC6BDB5FFB59C8CFF948C94FFBDB5ADFFBDADA5FFB5AD
        9CFFE7DED6FFDECED6FFC6B5B5FFD6BDB5FF947B73FF395252FF39ADBDFF4A6B
        73FFDEB5B5FFFFFFFF00FFFFF7FFB5A594FFA59C9CFFD6C6C6FFD6C6C6FFC6AD
        B5FFEFE7E7FFF7F7F7FFEFE7E7FFDECECEFFFFFFFF00D6BDBDFF393139FF39A5
        ADFF5A9C9CFFCEADADFFFFE7E7FFA59484FFA59C9CFFE7DEDEFFEFEFEFFFBDAD
        ADFFCEBDBDFFDECECEFFD6C6BDFFB59C9CFFCEBDBDFFF7EFEFFFC69C9CFF4239
        39FF39B5B5FF428484FFAD9494FFD6B5A5FF9C9494FFD6C6C6FFD6C6C6FFC6B5
        B5FFCEC6C6FFEFE7E7FFEFEFE7FFD6C6C6FFF7EFEFFFF7FFF7FFFFFFFF00EFD6
        D6FF393139FF52A5A5FFADC6BDFF948484FFAD9C94FFDEDED6FFE7DEDEFFDED6
        D6FFE7DED6FFEFE7E7FFDED6D6FFBDA5A5FFCEBDBDFFD6C6C6FFD6CEC6FFD6C6
        C6FFAD948CFF7B5A5AFF94C6E7FF6B849CFFAD9484FFE7DEDEFFCEC6C6FFCEB5
        B5FFCEB5BDFFE7DEDEFFDEDEDEFFC6ADB5FFDECED6FFEFE7E7FFF7F7F7FFD6CE
        CEFFEFDEE7FFFFFFFF00DEF7FFFFC6BDBDFF8C736BFFE7E7E7FFDED6D6FFD6CE
        C6FFD6CEC6FFE7E7DEFFE7E7DEFFC6BDB5FFC6BDB5FFE7E7DEFFD6D6CEFFB5AD
        A5FFBDADA5FFD6CEBDFFDECEBDFF947B63FF8C7B73FFF7F7F7FFA59484FFA58C
        7BFF847363FF9C8473FF847363FF7B6B5AFF9C8C73FF6B5A42FF847363FF9C84
        73FF6B5242FF8C6B5AFF9C8C73FF847352FF948C7BFFFFFFFF00C6AD9CFFB59C
        8CFF948C7BFFBDAD9CFF9C8C7BFFAD948CFFD6C6B5FF8C7B6BFFC6ADA5FFCEB5
        ADFF948473FFCEB5ADFFE7CEC6FFBDA58CFF9C8C7BFFFFFFFF00B59C94FF9473
        63FFA59484FF8C7363FFB59C94FF8C736BFF9C8473FFAD9C8CFF84635AFFA58C
        84FF9C8C7BFF846B63FF8C6B5AFF7B5A4AFFD6CECEFFFFFFFF00FFFFFF00F7EF
        EFFF8C7373FFA59494FFCEC6BDFF7B635AFFDED6D6FFAD948CFF94847BFFDED6
        D6FF846B63FF9C8C84FFDECECEFFDED6DEFFFFFFFF00FFFFFF00FFFFFF00FFFF
        FF00D6CEC6FFDED6D6FFEFEFEFFFD6C6BDFFFFFFFF00DECECEFFDED6CEFFFFFF
        FF00CEC6BDFFE7DEDEFFFFFFFF00FFFFFF00FFFFFF00}
      TabOrder = 4
      OnClick = BtnDefalcareClick
    end
    object BtnAutoDecont: TcxButton
      Left = 545
      Top = 6
      Width = 127
      Height = 25
      Anchors = [akTop, akRight]
      Caption = 'Decontare Automata'
      TabOrder = 5
      Visible = False
      OnClick = BtnAutoDecontClick
    end
    object btnCautaAutomat: TcxButton
      Left = 680
      Top = 6
      Width = 115
      Height = 25
      Anchors = [akTop, akRight]
      Caption = 'Cautare Automata'
      OptionsImage.Glyph.SourceDPI = 96
      OptionsImage.Glyph.Data = {
        424D360400000000000036000000280000001000000010000000010020000000
        000000000000C40E0000C40E00000000000000000000FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00E7DEDEFFDECED6FFF7C6CEFFBDA58CFFAD8C7BFFEFEFEFFFFFFF
        FF00FFFFFF00FFFFFF00E7F7F7FFB5ADB5FFF7F7F7FFFFFFFF00DECEC6FFBDAD
        94FFC6AD94FFCE9C94FF9C9473FF84BD6BFFEFD6CEFFA57B63FFAD846BFFF7EF
        EFFFFFFFFF00F7FFFFFF9C7373FF520000FFCEC6CEFFF7F7F7FFBDA594FFC69C
        84FFD6B59CFFF7D6CEFFC6D6ADFF84E78CFFF7DEE7FFCEB59CFF94735AFF947B
        6BFFEFEFEFFFB58CA5FF630000FF9C6373FFF7F7F7FFE7E7E7FFCEB5A5FFE7CE
        BDFFDEC6BDFFDECEB5FFF7D6D6FFFFEFFFFFD6C6BDFF94736BFFAD8C8CFF9C7B
        63FF84635AFF7B1039FFAD738CFFFFFFFF00FFFFFF00DED6D6FFCEB59CFFE7CE
        BDFFE7D6C6FFEFE7D6FFFFEFEFFFF7EFEFFFBDA5ADFFCEB5B5FFFFE7C6FFFFEF
        C6FFCEAD9CFF845A5AFFEFEFEFFFFFFFFF00FFFFFF00DECECEFFCEB5ADFFDECE
        BDFFC6B59CFFC6A59CFFBDA59CFFDEC6BDFFE7C6C6FFFFF7E7FFFFEFD6FFFFEF
        DEFFFFFFFF00CEAD94FFBD9C8CFFFFFFFF00FFFFFF00DECECEFFCEBDA5FFC6B5
        A5FFC6AD9CFFC6B5A5FFD6CEBDFFE7CECEFFEFC6A5FFFFEFBDFFFFFFFF00FFFF
        FF00FFFFFF00EFD6C6FFC69C8CFFFFFFFF00FFFFFF00DEC6BDFFD6BDA5FFEFDE
        D6FFDECEC6FFCEB5ADFFC6A594FFE7DEDEFFDEB5A5FFF7E7CEFFFFFFFF00FFFF
        FF00FFFFFF00E7C6BDFFCEB59CFFFFFFFF00FFFFFF00DECECEFFCEB5A5FFC6AD
        9CFFC6AD9CFFCEAD9CFFCEB5ADFFF7EFE7FFDEC6C6FFCEA5A5FFEFCEBDFFEFD6
        CEFFE7CECEFFD6B5A5FFCEBDADFFFFFFFF00FFFFFF00D6CEC6FFCEBDADFFE7D6
        CEFFD6C6BDFFC6B5A5FFBDA59CFFE7DED6FFF7E7DEFFE7CEBDFFD6ADA5FFE7B5
        ADFFEFDECEFFDEBDA5FFCEB5A5FFFFFFFF00FFFFFF00DECECEFFCEBDADFFC6AD
        A5FFBDA594FFC6AD9CFFD6BDB5FFF7E7E7FFE7D6CEFFE7D6CEFFF7E7DEFFF7DE
        D6FFF7E7E7FFE7CEB5FFCEB59CFFFFFFFF00FFFFFF00DECECEFFCEB5A5FFF7E7
        D6FFE7D6CEFFF7E7DEFFFFF7E7FFF7E7DEFFEFDED6FFEFDECEFFEFD6CEFFEFD6
        CEFFF7DEDEFFDEC6ADFFCEB5ADFFFFFFFF00FFFFFF00EFEFEFFFCEADA5FFCEB5
        A5FFDEBDADFFBD9C8CFFAD8C7BFFA58C73FFA5846BFFD6B5A5FFF7E7DEFFEFD6
        CEFFFFE7DEFFDECEB5FFCEB5A5FFFFFFFF00FFFFFF00FFFFFF00F7F7F7FFDED6
        D6FFCEBDADFFAD846BFF945A4AFF945A42FF9C634AFFAD7B63FFC6A594FFE7CE
        C6FFF7EFEFFFDECEB5FFCEB5ADFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00F7F7F7FFE7DEDEFFDEC6BDFFCEAD94FFC69C84FFA57363FF946352FFA573
        6BFFD6B5B5FFC6AD9CFFBDA59CFFFFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
        FF00FFFFFF00FFFFFF00F7FFFFFFDEDED6FFCEBDB5FFCEB5B5FFDECEBDFFD6C6
        BDFFD6C6C6FFDECECEFFEFE7E7FFFFFFFF00FFFFFF00}
      TabOrder = 6
      OnClick = btnCautaAutomatClick
    end
    object btnRefresh: TcxButton
      Left = 440
      Top = 6
      Width = 78
      Height = 25
      Caption = 'Refresh'
      TabOrder = 7
      OnClick = btnRefreshClick
    end
  end
  object Splitter1: TcxSplitter
    Left = 0
    Top = 408
    Width = 804
    Height = 8
    Cursor = crVSplit
    HotZoneClassName = 'TcxMediaPlayer9Style'
    AlignSplitter = salBottom
    AutoSnap = True
    Control = Panel2
    ExplicitTop = 371
  end
  object DTFurnizori: TDataSource
    DataSet = QryFurnizori
    Left = 73
    Top = 80
  end
  object DTClienti: TDataSource
    DataSet = QryClienti
    Left = 73
    Top = 112
  end
  object DTImperecheri: TDataSource
    DataSet = QryImperecheri
    Left = 73
    Top = 144
  end
  object QryFurnizori: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'exec spCompensariTerti 0'
      ' ')
    Params = <>
    Left = 105
    Top = 80
  end
  object QryClienti: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'exec spCompensariTerti 1')
    Params = <>
    Left = 105
    Top = 112
  end
  object QryImperecheri: TZQuery
    Connection = frmData.dbContabilitate
    AfterOpen = QryImperecheriAfterOpen
    SQL.Strings = (
      'exec spCompensariEfectuate')
    Params = <>
    Left = 105
    Top = 144
  end
  object ChkStyle: TdxCheckEditStyleController
    BorderStyle = xbsFlat
    ButtonStyle = btsFlat
    HotTrack = True
    Shadow = True
    Left = 138
    Top = 82
  end
  object ppDecontari: TPopupMenu
    OnPopup = ppDecontariPopup
    Left = 106
    Top = 188
    object ppLocalizare: TMenuItem
      Caption = 'Localizare Documente'
      OnClick = ppLocalizareClick
    end
    object ppReconciliere: TMenuItem
      Caption = 'Reconciliere Documente'
      OnClick = ppReconciliereClick
    end
    object ppReparareDoc: TMenuItem
      Caption = 'Reasignare Document'
    end
    object ppModificareDocum: TMenuItem
      Caption = 'Modificare Decontare'
      OnClick = ppModificareDocumClick
    end
  end
end
