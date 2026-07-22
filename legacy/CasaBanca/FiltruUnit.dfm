object FrmFiltru: TFrmFiltru
  Tag = 1
  Left = 253
  Top = 225
  ClientHeight = 605
  ClientWidth = 854
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
  object pnFiltre: TPanel
    Left = 0
    Top = 121
    Width = 854
    Height = 484
    Align = alClient
    BevelOuter = bvNone
    TabOrder = 7
    DesignSize = (
      854
      484)
    object lbFiltru: TLabel
      Left = 5
      Top = 8
      Width = 5
      Height = 16
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -13
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object pnRest: TPanel
      Left = 1
      Top = 25
      Width = 860
      Height = 388
      Anchors = [akLeft, akTop, akRight, akBottom]
      TabOrder = 5
      ExplicitHeight = 357
      object Splitter2: TSplitter
        Left = 425
        Top = 1
        Width = 4
        Height = 386
        ExplicitHeight = 355
      end
      object pnRight: TPanel
        Left = 429
        Top = 1
        Width = 430
        Height = 386
        Align = alClient
        BevelOuter = bvNone
        Caption = 'pnRight'
        TabOrder = 0
        ExplicitHeight = 355
        object GridRecentFilter: TdxDBGrid
          Left = 0
          Top = 24
          Width = 430
          Height = 362
          SearchType = stStart
          Bands = <
            item
            end>
          DefaultLayout = True
          HeaderPanelRowCount = 1
          KeyField = 'ID_CACHE_FILTRE'
          SummaryGroups = <>
          SummarySeparator = ', '
          Align = alClient
          TabOrder = 0
          DataSource = DTCache
          Filter.Criteria = {00000000}
          LookAndFeel = lfUltraFlat
          OptionsView = [edgoAutoWidth, edgoBandHeaderWidth, edgoUseBitmap]
          OnChangeNode = GridRecentFilterChangeNode
          ExplicitHeight = 331
          object GridRecentFilterID_CACHE_FILTRE: TdxDBGridMaskColumn
            Visible = False
            Width = 433
            BandIndex = 0
            RowIndex = 0
            FieldName = 'ID_CACHE_FILTRE'
          end
          object GridRecentFilterDENUMIRE: TdxDBGridMaskColumn
            Caption = 'Denumire'
            Width = 121
            BandIndex = 0
            RowIndex = 0
            FieldName = 'DENUMIRE'
          end
          object GridRecentFilterFILTER_STRING: TdxDBGridMaskColumn
            Caption = 'Filter'
            Width = 205
            BandIndex = 0
            RowIndex = 0
            FieldName = 'FILTER_STRING'
          end
          object GridRecentFilterCOMENT: TdxDBGridMaskColumn
            Visible = False
            Width = 6017
            BandIndex = 0
            RowIndex = 0
            FieldName = 'COMENT'
          end
          object GridRecentFilterDATA_FILTRU: TdxDBGridDateColumn
            Caption = 'Data Filtru'
            Width = 101
            BandIndex = 0
            RowIndex = 0
            FieldName = 'DATA_FILTRU'
          end
          object GridRecentFilterSTARE: TdxDBGridMaskColumn
            Visible = False
            Width = 252
            BandIndex = 0
            RowIndex = 0
            FieldName = 'STARE'
          end
          object GridRecentFilterID_UTILIZATOR: TdxDBGridMaskColumn
            Visible = False
            Width = 341
            BandIndex = 0
            RowIndex = 0
            FieldName = 'ID_UTILIZATOR'
          end
          object GridRecentFilterID_LOGIN: TdxDBGridMaskColumn
            Visible = False
            Width = 252
            BandIndex = 0
            RowIndex = 0
            FieldName = 'ID_LOGIN'
          end
        end
        object pnRightTop: TPanel
          Left = 0
          Top = 0
          Width = 430
          Height = 24
          Align = alTop
          BevelOuter = bvNone
          TabOrder = 1
          DesignSize = (
            430
            24)
          object edtCacheFiltru: TdxDBButtonEdit
            Left = 1
            Top = 0
            Width = 425
            TabOrder = 0
            Anchors = [akLeft, akTop, akRight]
            DataField = 'FILTER_STRING'
            DataSource = DTCache
            OnChange = edtCacheFiltruChange
            OnSelectionChange = edtCacheFiltruChange
            Buttons = <
              item
                Default = True
              end>
            OnButtonClick = edtCacheFiltruButtonClick
            ExistButtons = True
          end
        end
      end
      object pnFiltru: TPanel
        Left = 1
        Top = 1
        Width = 424
        Height = 386
        Align = alLeft
        BevelOuter = bvNone
        Caption = 'pnFiltru'
        TabOrder = 1
        ExplicitHeight = 355
        object Splitter1: TSplitter
          Left = 231
          Top = 23
          Height = 363
          Align = alRight
          ExplicitLeft = 228
          ExplicitHeight = 332
        end
        object DBInspector: TdxDBInspector
          Left = 234
          Top = 23
          Width = 190
          Height = 363
          Align = alRight
          Color = clWindow
          DataSource = DTFiltre
          DefaultFields = False
          TabOrder = 0
          DividerPos = 91
          GridColor = clBtnFace
          PaintStyle = ipsNET
          ExplicitHeight = 378
          Data = {
            24010000060000000800000000000000260000004400420049006E0073007000
            6500630074006F007200440045004E0055004D00490052004500080000000000
            0000300000004400420049006E00730070006500630074006F00720046004900
            4C005400450052005F0053005400520049004E00470008000000000000002000
            00004400420049006E00730070006500630074006F0072005300540041005200
            45000800000000000000220000004400420049006E0073007000650063007400
            6F00720043004F004D0045004E00540008000000000000001E00000044004200
            49006E00730070006500630074006F00720052006F0077003700080000000000
            00001E0000004400420049006E00730070006500630074006F00720052006F00
            7700380000000000}
          object DBInspectorDENUMIRE: TdxInspectorDBMaskRow
            Caption = 'Denumire'
            FieldName = 'DENUMIRE'
          end
          object DBInspectorFILTER_STRING: TdxInspectorDBMaskRow
            Caption = 'Filtru'
            ReadOnly = True
            FieldName = 'FILTER_STRING'
          end
          object DBInspectorID_UTILIZATOR: TdxInspectorDBMaskRow
            Visible = False
            FieldName = 'ID_UTILIZATOR'
          end
          object DBInspectorLOGIN_MOD: TdxInspectorDBMaskRow
            Visible = False
            FieldName = 'LOGIN_MOD'
          end
          object DBInspectorSTARE: TdxInspectorDBMaskRow
            Caption = 'Stare'
            FieldName = 'STARE'
          end
          object DBInspectorCOMENT: TdxInspectorDBMemoRow
            Caption = 'Comentariu'
            RowHeight = 50
            FieldName = 'COMENT'
          end
          object DBInspectorRow7: TdxInspectorDBRow
            Caption = 'Detalii Filtru'
            IsCategory = True
          end
          object DBInspectorRow8: TdxInspectorDBRow
            Caption = 'Stare'
            IsCategory = True
          end
        end
        object FilterTree: TdxDBTreeList
          Left = 0
          Top = 23
          Width = 231
          Height = 363
          SearchType = stStart
          Bands = <
            item
            end>
          DefaultLayout = True
          HeaderPanelRowCount = 1
          KeyField = 'ID_FILTRE'
          ParentField = 'ID_PARENT'
          Align = alClient
          TabOrder = 1
          DataSource = DTFiltre
          LookAndFeel = lfUltraFlat
          OptionsDB = [etoCancelOnExit, etoCanDelete, etoCanInsert, etoCanNavigation, etoCheckHasChildren, etoConfirmDelete, etoLoadAllRecords]
          OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoRowSelect, etoUseBitmap, etoUseImageIndexForSelected]
          TreeLineColor = clGrayText
          OnChangeNode = FilterTreeChangeNode
          ExplicitWidth = 228
          ExplicitHeight = 332
          object FilterTreeRecId: TdxDBTreeListColumn
            Visible = False
            Width = 37
            BandIndex = 0
            RowIndex = 0
            FieldName = 'RecId'
          end
          object FilterTreeDENUMIRE: TdxDBTreeListMaskColumn
            Caption = 'Denumire'
            Width = 143
            BandIndex = 0
            RowIndex = 0
            FieldName = 'DENUMIRE'
          end
          object FilterTreeFILTER_STRING: TdxDBTreeListMaskColumn
            Caption = 'Filtru'
            Width = 155
            BandIndex = 0
            RowIndex = 0
            FieldName = 'FILTER_STRING'
          end
          object FilterTreeID_PARENT: TdxDBTreeListMaskColumn
            Visible = False
            Width = 40
            BandIndex = 0
            RowIndex = 0
            FieldName = 'ID_PARENT'
          end
          object FilterTreeID_FILTRE: TdxDBTreeListMaskColumn
            Visible = False
            Width = 37
            BandIndex = 0
            RowIndex = 0
            FieldName = 'ID_FILTRE'
          end
          object FilterTreeID_UTILIZATOR: TdxDBTreeListMaskColumn
            Visible = False
            Width = 49
            BandIndex = 0
            RowIndex = 0
            FieldName = 'ID_UTILIZATOR'
          end
          object FilterTreeLOGIN_MOD: TdxDBTreeListMaskColumn
            Visible = False
            Width = 40
            BandIndex = 0
            RowIndex = 0
            FieldName = 'LOGIN_MOD'
          end
          object FilterTreeSTARE: TdxDBTreeListMaskColumn
            Visible = False
            Width = 37
            BandIndex = 0
            RowIndex = 0
            FieldName = 'STARE'
          end
        end
        object pnTopFiltre: TPanel
          Left = 0
          Top = 0
          Width = 424
          Height = 23
          Align = alTop
          BevelOuter = bvNone
          TabOrder = 2
          DesignSize = (
            424
            23)
          object edtFiltru: TdxDBButtonEdit
            Left = 1
            Top = 0
            Width = 420
            TabOrder = 0
            Anchors = [akLeft, akTop, akRight]
            DataField = 'FILTER_STRING'
            DataSource = DTFiltre
            OnChange = edtFiltruChange
            OnSelectionChange = edtFiltruChange
            Buttons = <
              item
                Default = True
              end>
            OnButtonClick = edtFiltruButtonClick
            ExistButtons = True
          end
        end
      end
    end
    object btnSave: TBitBtn
      Left = 153
      Top = 417
      Width = 70
      Height = 20
      Anchors = [akLeft, akBottom]
      Caption = 'Save'
      TabOrder = 0
      ExplicitTop = 386
    end
    object btnDelete: TBitBtn
      Left = 227
      Top = 417
      Width = 75
      Height = 20
      Anchors = [akLeft, akBottom]
      Caption = 'Delete'
      TabOrder = 1
      OnClick = btnDeleteClick
      ExplicitTop = 386
    end
    object btnNewFilter: TBitBtn
      Left = 4
      Top = 417
      Width = 67
      Height = 20
      Anchors = [akLeft, akBottom]
      Caption = 'Filtru Nou'
      TabOrder = 2
      OnClick = btnNewFilterClick
      ExplicitTop = 386
    end
    object btnSub: TBitBtn
      Left = 74
      Top = 417
      Width = 75
      Height = 20
      Anchors = [akLeft, akBottom]
      Caption = 'SubFiltru Nou'
      TabOrder = 3
      OnClick = btnSubClick
      ExplicitTop = 386
    end
    object btnSwitchToFiltre: TBitBtn
      Left = 437
      Top = 418
      Width = 84
      Height = 20
      Anchors = [akLeft, akBottom]
      Caption = '<- Creaza Filtru'
      TabOrder = 4
      OnClick = btnSwitchToFiltreClick
      ExplicitTop = 387
    end
  end
  object pnTop: TPanel
    Left = 0
    Top = 0
    Width = 854
    Height = 121
    Align = alTop
    BevelOuter = bvNone
    TabOrder = 0
    object bvTop: TBevel
      Left = 0
      Top = 0
      Width = 854
      Height = 8
      Align = alTop
      Shape = bsBottomLine
      ExplicitWidth = 862
    end
    object lbTop: TLabel
      Left = 15
      Top = 0
      Width = 158
      Height = 13
      Caption = 'Perioada  de selectie pentru casa'
      Transparent = False
    end
  end
  object pnTimeDelaLa: TPanel
    Tag = 1
    Left = 239
    Top = 64
    Width = 245
    Height = 50
    TabOrder = 1
    object Label2: TLabel
      Left = 5
      Top = 27
      Width = 25
      Height = 13
      Caption = 'De la'
    end
    object Label3: TLabel
      Left = 135
      Top = 27
      Width = 12
      Height = 13
      Caption = 'La'
    end
    object edDataDeLa: TdxDateEdit
      Left = 37
      Top = 22
      Width = 88
      TabOrder = 0
      OnKeyPress = edDataDeLaKeyPress
      Date = 37987.000000000000000000
      UseEditMask = True
      OnDateChange = edDataDeLaDateChange
      StoredValues = 4
    end
    object edDataLa: TdxDateEdit
      Left = 152
      Top = 22
      Width = 88
      TabOrder = 1
      Date = 37987.000000000000000000
      UseEditMask = True
      StoredValues = 4
    end
    object rbDelaLa: TRadioButton
      Left = 4
      Top = 2
      Width = 101
      Height = 17
      Alignment = taLeftJustify
      Caption = 'De la data la data'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      TabOrder = 2
    end
  end
  object pnTimeSaptamana: TPanel
    Tag = 2
    Left = 0
    Top = 13
    Width = 410
    Height = 50
    TabOrder = 2
    object Label4: TLabel
      Left = 6
      Top = 2
      Width = 72
      Height = 13
      Caption = 'In Saptamna'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
    end
    object Label5: TLabel
      Left = 4
      Top = 29
      Width = 69
      Height = 13
      Caption = 'In Saptamana '
    end
    object Label6: TLabel
      Left = 246
      Top = 27
      Width = 37
      Height = 13
      Caption = 'din luna'
    end
    object edSaptamana: TdxImageEdit
      Left = 77
      Top = 21
      Width = 158
      TabOrder = 0
    end
    object edLunaAn: TdxImageEdit
      Left = 293
      Top = 20
      Width = 114
      TabOrder = 1
      OnChange = edLunaAnChange
    end
    object rbSaptamana: TRadioButton
      Tag = 2
      Left = 4
      Top = 2
      Width = 105
      Height = 17
      Alignment = taLeftJustify
      Caption = 'In Saptamana'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      TabOrder = 2
    end
  end
  object pnTimeZiua: TPanel
    Tag = 5
    Left = 411
    Top = 13
    Width = 122
    Height = 50
    TabOrder = 3
    object edZi: TdxDateEdit
      Left = 25
      Top = 21
      Width = 88
      TabOrder = 0
      Date = 37987.000000000000000000
      UseEditMask = True
      StoredValues = 4
    end
    object rbZiua: TRadioButton
      Left = 4
      Top = 1
      Width = 66
      Height = 16
      Alignment = taLeftJustify
      Caption = 'In Ziua'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      TabOrder = 1
    end
  end
  object pnTimeAnul: TPanel
    Tag = 4
    Left = 706
    Top = 12
    Width = 154
    Height = 50
    TabOrder = 4
    object edAn: TdxImageEdit
      Left = 32
      Top = 20
      Width = 114
      TabOrder = 0
    end
    object rbAnul: TRadioButton
      Left = 4
      Top = 1
      Width = 69
      Height = 17
      Alignment = taLeftJustify
      Caption = 'In Anul'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      TabOrder = 1
    end
  end
  object pnTimeLunaAn: TPanel
    Tag = 3
    Left = 534
    Top = 12
    Width = 172
    Height = 50
    TabOrder = 5
    object edLuna: TdxImageEdit
      Left = 15
      Top = 21
      Width = 154
      TabOrder = 0
    end
    object rbLuna: TRadioButton
      Left = 4
      Top = 2
      Width = 67
      Height = 16
      Alignment = taLeftJustify
      Caption = 'In Luna'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      TabOrder = 1
    end
  end
  object pnTimePeriod: TPanel
    Tag = 6
    Left = 0
    Top = 64
    Width = 233
    Height = 50
    TabOrder = 6
    object edNrZile: TdxSpinEdit
      Left = 23
      Top = 23
      Width = 49
      TabOrder = 0
    end
    object rb_Zile: TRadioButton
      Left = 105
      Top = 13
      Width = 40
      Height = 16
      Alignment = taLeftJustify
      Caption = 'Zile'
      Checked = True
      TabOrder = 1
      TabStop = True
    end
    object rb_Sapt: TRadioButton
      Left = 105
      Top = 28
      Width = 73
      Height = 16
      Alignment = taLeftJustify
      Caption = 'Saptamani'
      TabOrder = 2
    end
    object rb_Luni: TRadioButton
      Left = 185
      Top = 13
      Width = 40
      Height = 16
      Alignment = taLeftJustify
      Caption = 'Luni'
      TabOrder = 3
    end
    object rb_Ani: TRadioButton
      Left = 185
      Top = 29
      Width = 40
      Height = 17
      Alignment = taLeftJustify
      Caption = 'Ani'
      TabOrder = 4
    end
    object rbLast: TRadioButton
      Left = 4
      Top = 2
      Width = 101
      Height = 16
      Alignment = taLeftJustify
      Caption = '<B>Ultimile n.....</B>'
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
      ParentFont = False
      TabOrder = 5
    end
  end
  object DTFiltre: TDataSource
    DataSet = MemFiltre
    Left = 438
    Top = 130
  end
  object MemFiltre: TZQuery
    Connection = frmData.dbContabilitate
    OnNewRecord = MemFiltreNewRecord
    SQL.Strings = (
      'SELECT *  FROM FILTRE WHERE ID_UTILIZATOR= :ID_UTILIZATOR')
    Params = <
      item
        DataType = ftInteger
        Name = 'ID_UTILIZATOR'
        ParamType = ptUnknown
      end>
    Left = 470
    Top = 130
    ParamData = <
      item
        DataType = ftInteger
        Name = 'ID_UTILIZATOR'
        ParamType = ptUnknown
      end>
  end
  object DTCache: TDataSource
    DataSet = QryCache
    Left = 502
    Top = 130
  end
  object QryCache: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      
        'SELECT *  FROM CACHE_FILTRE WHERE ID_UTILIZATOR= :ID_UTILIZATOR ' +
        'ORDER BY DATA_FILTRU DESC')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_UTILIZATOR'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 534
    Top = 130
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_UTILIZATOR'
        ParamType = ptUnknown
        Size = 4
      end>
  end
end
