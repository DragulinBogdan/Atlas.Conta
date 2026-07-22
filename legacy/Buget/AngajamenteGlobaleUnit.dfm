object frmAngajamenteGlobale: TfrmAngajamenteGlobale
  Left = 335
  Top = 220
  Caption = 'Angajamente Globale Implicite'
  ClientHeight = 437
  ClientWidth = 777
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  OnClose = FormClose
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object Panel1: TPanel
    Left = 0
    Top = 0
    Width = 777
    Height = 57
    Align = alTop
    BevelInner = bvRaised
    BevelOuter = bvLowered
    TabOrder = 0
    OnResize = Panel1Resize
    DesignSize = (
      777
      57)
    object Label3: TLabel
      Left = 328
      Top = 5
      Width = 52
      Height = 13
      Anchors = [akTop]
      Caption = 'An Fiscal : '
    end
    object Departament: TcxRepartitorPanel
      Left = 10
      Top = 5
      Caption = 'Departament'
      TabOrder = 0
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
      PopupEdit.HideEditCursor = False
      PopupEdit.HideSelection = True
      PopupEdit.PopupAutoSize = True
      PopupEdit.PopupAlignment = taLeftJustify
      PopupEdit.PopupClientEdge = False
      PopupEdit.PopupControl = TreeRepartitori
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = False
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 100
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 250
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 180
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
      ButonEdit.Flat = False
      ButonEdit.Enabled = True
      OnlySelectChild = False
      ValidateEditText = False
      ValidateWithPopup = True
      DataSource = frmData.DTRepartitori
      CodField = 'COD_FISCAL'
      KeyField = 'ID_REPARTITORI'
      ListField = 'NUME'
      OnValidate = DepartamentValidate
      Height = 47
      Width = 300
    end
    object edRepartitor: TcxRepartitorPanel
      Left = 470
      Top = 5
      Anchors = [akTop, akRight]
      Caption = 'edRepartitor'
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
      PopupEdit.HideEditCursor = False
      PopupEdit.HideSelection = True
      PopupEdit.PopupAutoSize = True
      PopupEdit.PopupAlignment = taLeftJustify
      PopupEdit.PopupClientEdge = False
      PopupEdit.PopupControl = TreeRepartitori
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = False
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 100
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 250
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 180
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
      ButonEdit.Flat = False
      ButonEdit.Enabled = True
      OnlySelectChild = False
      ValidateEditText = False
      ValidateWithPopup = True
      DataSource = frmData.DTRepartitori
      CodField = 'COD_FISCAL'
      KeyField = 'ID_REPARTITORI'
      ListField = 'NUME'
      OnValidate = edRepartitorValidate
      Height = 47
      Width = 300
    end
    object edAnFiscal: TcxSpinEdit
      Left = 327
      Top = 24
      Anchors = [akTop]
      TabOrder = 2
      Width = 121
    end
  end
  object Panel2: TPanel
    Left = 0
    Top = 396
    Width = 777
    Height = 41
    Align = alBottom
    BevelInner = bvRaised
    BevelOuter = bvLowered
    TabOrder = 1
    DesignSize = (
      777
      41)
    object BtnGenerare: TSpeedButton
      Left = 648
      Top = 8
      Width = 113
      Height = 22
      Anchors = [akRight, akBottom]
      Caption = 'Genereaza'
      OnClick = BtnGenerareClick
    end
  end
  object Panel3: TPanel
    Left = 0
    Top = 57
    Width = 777
    Height = 339
    Align = alClient
    BevelOuter = bvLowered
    TabOrder = 2
    object gridAngajamenteGlobale: TcxGrid
      Left = 1
      Top = 1
      Width = 775
      Height = 337
      Align = alClient
      TabOrder = 1
      LookAndFeel.Kind = lfFlat
      ExplicitLeft = 0
      ExplicitTop = 57
      ExplicitWidth = 777
      ExplicitHeight = 339
      object viewAngajamenteLocale: TcxGridDBTableView
        Navigator.Buttons.CustomButtons = <>
        DataController.DataSource = DTGlobale
        DataController.Filter.MaxValueListCount = 1000
        DataController.Filter.Active = True
        DataController.KeyFieldNames = 'ID'
        DataController.Summary.DefaultGroupSummaryItems = <>
        DataController.Summary.FooterSummaryItems = <>
        DataController.Summary.SummaryGroups = <>
        Filtering.ColumnPopup.MaxDropDownItemCount = 12
        OptionsBehavior.IncSearch = True
        OptionsBehavior.FocusCellOnCycle = True
        OptionsBehavior.ImmediateEditor = False
        OptionsData.CancelOnExit = False
        OptionsData.Editing = False
        OptionsData.Inserting = False
        OptionsSelection.MultiSelect = True
        OptionsSelection.HideFocusRectOnExit = False
        OptionsView.ColumnAutoWidth = True
        OptionsView.GroupByBox = False
        OptionsView.GroupFooters = gfVisibleWhenExpanded
        Preview.AutoHeight = False
        Preview.MaxLineCount = 2
        object viewAngajamenteLocaleCOD_FUNCTIONAL: TcxGridDBColumn
          Caption = 'Clasa Func.'
          DataBinding.FieldName = 'COD_FUNCTIONAL'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          Width = 177
        end
        object viewAngajamenteLocaleCOD_ECONOMIC: TcxGridDBColumn
          Caption = 'Clasa Ec.'
          DataBinding.FieldName = 'COD_ECONOMIC'
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          Width = 192
        end
        object viewAngajamenteLocaleAPROBATE: TcxGridDBColumn
          Caption = 'Aprobate'
          DataBinding.FieldName = 'APROBATE'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.AssignedValues.MaxValue = True
          Properties.AssignedValues.MinValue = True
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0;-,0'
          Properties.Nullable = False
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          Width = 116
        end
        object viewAngajamenteLocaleTOTAL_ANGAJATE: TcxGridDBColumn
          Caption = 'Total Ang'
          DataBinding.FieldName = 'TOTAL_ANGAJATE'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.AssignedValues.MaxValue = True
          Properties.AssignedValues.MinValue = True
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0;-,0'
          Properties.Nullable = False
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          Width = 123
        end
        object viewAngajamenteLocaleANGAJAT: TcxGridDBColumn
          Caption = 'Angajat'
          DataBinding.FieldName = 'ANGAJAT'
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.AssignedValues.MaxValue = True
          Properties.AssignedValues.MinValue = True
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0;-,0'
          Properties.Nullable = False
          Properties.ReadOnly = True
          HeaderAlignmentHorz = taCenter
          Width = 100
        end
        object viewAngajamenteLocaleDATA: TcxGridDBColumn
          Caption = 'Data'
          DataBinding.FieldName = 'DATA'
          PropertiesClassName = 'TcxDateEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Alignment.Vert = taTopJustify
          Properties.DateButtons = [btnClear, btnToday]
          Properties.DateOnError = deToday
          Properties.InputKind = ikRegExpr
          HeaderAlignmentHorz = taCenter
          Width = 65
        end
      end
      object nivelAngajamenteGlobale: TcxGridLevel
        GridView = viewAngajamenteLocale
      end
    end
    object TreeRepartitori: TdxDBTreeList
      Left = 192
      Top = 41
      Width = 409
      Height = 176
      SearchType = stContain
      Bands = <
        item
        end>
      DefaultLayout = True
      HeaderPanelRowCount = 1
      KeyField = 'ID_REPARTITORI'
      ParentField = 'ID_PARINTE'
      TabOrder = 0
      Visible = False
      OnDblClick = TreeRepartitoriDblClick
      OnKeyDown = TreeRepartitoriKeyDown
      DataSource = frmData.DTRepartitori
      LookAndFeel = lfFlat
      OptionsBehavior = [etoAnsiSort, etoAutoDragDrop, etoAutoDragDropCopy, etoAutoSearch, etoAutoSort, etoDblClick, etoImmediateEditor]
      OptionsDB = [etoCanInsert, etoCanNavigation, etoCheckHasChildren, etoLoadAllRecords]
      OptionsView = [etoAutoWidth, etoBandHeaderWidth, etoInvertSelect, etoUseBitmap, etoUseImageIndexForSelected]
      TreeLineColor = clGrayText
      object TreeRepartitoriNUME: TdxDBTreeListMaskColumn
        Caption = 'Denumire'
        HeaderAlignment = taCenter
        Width = 199
        BandIndex = 0
        RowIndex = 0
        FieldName = 'NUME'
      end
      object TreeRepartitoriADRESA: TdxDBTreeListMaskColumn
        Caption = 'Adresa'
        HeaderAlignment = taCenter
        Width = 99
        BandIndex = 0
        RowIndex = 0
        FieldName = 'ADRESA'
      end
      object TreeRepartitoriCONT: TdxDBTreeListMaskColumn
        Caption = 'Cont'
        HeaderAlignment = taCenter
        Width = 62
        BandIndex = 0
        RowIndex = 0
        FieldName = 'CONT'
      end
      object TreeRepartitoriCODFISC: TdxDBTreeListMaskColumn
        Caption = 'Cod Fiscal'
        HeaderAlignment = taCenter
        Width = 88
        BandIndex = 0
        RowIndex = 0
        FieldName = 'CODFISC'
      end
      object TreeRepartitoriGESTINT: TdxDBTreeListCheckColumn
        Caption = 'Tip Gestiune'
        HeaderAlignment = taCenter
        Width = 71
        BandIndex = 0
        RowIndex = 0
        FieldName = 'GESTINT'
        ValueChecked = 'True'
        ValueUnchecked = 'False'
      end
    end
  end
  object DTGlobale: TDataSource
    DataSet = QryGlobale
    Left = 48
    Top = 89
  end
  object QryGlobale: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'EXEC SP_BUGET_GET_GLOBAL :AN_FISCAL')
    Params = <
      item
        DataType = ftInteger
        Name = 'AN_FISCAL'
        ParamType = ptUnknown
        Size = -1
      end>
    Left = 112
    Top = 89
    ParamData = <
      item
        DataType = ftInteger
        Name = 'AN_FISCAL'
        ParamType = ptUnknown
        Size = -1
      end>
  end
end
