object frmBxImportEXCEL: TfrmBxImportEXCEL
  Left = 277
  Top = 42
  Caption = 'Import EXCEL Planificare Bugetara'
  ClientHeight = 625
  ClientWidth = 800
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poDefault
  OnClose = FormClose
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  PixelsPerInch = 96
  TextHeight = 13
  object pnTools: TPanel
    Left = 0
    Top = 0
    Width = 800
    Height = 129
    Align = alTop
    BevelOuter = bvLowered
    TabOrder = 0
    DesignSize = (
      800
      129)
    object Label1: TLabel
      Left = 10
      Top = 11
      Width = 117
      Height = 13
      Caption = 'Clasificatia Functionala : '
      FocusControl = ClasaFunctionala
    end
    object Label2: TLabel
      Left = 10
      Top = 40
      Width = 79
      Height = 13
      Caption = 'Fisier de import : '
      FocusControl = edFileName
    end
    object Label3: TLabel
      Left = 452
      Top = 39
      Width = 91
      Height = 13
      Caption = 'Clasa Economica : '
      FocusControl = edClasaEconomica
    end
    object Label4: TLabel
      Left = 162
      Top = 67
      Width = 32
      Height = 13
      Caption = 'Trim. 1'
      FocusControl = edClasaEconomica
    end
    object Label5: TLabel
      Left = 297
      Top = 67
      Width = 32
      Height = 13
      Caption = 'Trim. 2'
      FocusControl = edClasaEconomica
    end
    object Label6: TLabel
      Left = 437
      Top = 67
      Width = 32
      Height = 13
      Caption = 'Trim. 3'
      FocusControl = edClasaEconomica
    end
    object Label7: TLabel
      Left = 573
      Top = 67
      Width = 32
      Height = 13
      Caption = 'Trim. 4'
      FocusControl = edClasaEconomica
    end
    object Label8: TLabel
      Left = 10
      Top = 67
      Width = 43
      Height = 13
      Caption = 'Restante'
      FocusControl = edClasaEconomica
    end
    object Label9: TLabel
      Left = 181
      Top = 87
      Width = 62
      Height = 13
      Caption = 'Estimat +1An'
      FocusControl = edClasaEconomica
    end
    object Label10: TLabel
      Left = 285
      Top = 87
      Width = 62
      Height = 13
      Caption = 'Estimat +2An'
      FocusControl = edClasaEconomica
    end
    object Label11: TLabel
      Left = 389
      Top = 87
      Width = 62
      Height = 13
      Caption = 'Estimat +3An'
      FocusControl = edClasaEconomica
    end
    object edFileName: TEdit
      Left = 93
      Top = 37
      Width = 316
      Height = 21
      BevelInner = bvNone
      BevelKind = bkFlat
      BevelOuter = bvNone
      BorderStyle = bsNone
      TabOrder = 1
      OnChange = edFileNameChange
    end
    object ClasaFunctionala: TcxRepartitorPanel
      Left = 128
      Top = 3
      Anchors = [akLeft, akTop, akRight]
      ParentBackground = False
      ParentColor = False
      Style.Color = clBtnFace
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
      PopupEdit.PopupAutoSize = False
      PopupEdit.PopupAlignment = taLeftJustify
      PopupEdit.PopupClientEdge = False
      PopupEdit.PopupControl = TreeBugete
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = False
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 100
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 250
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 543
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
      DataSource = frmData.DTBGPlanFunctionalComplet
      CodField = 'COD_FUNCTIONAL'
      KeyField = 'ID_BG_PLAN_FUNCTIONAL'
      ListField = 'DENUMIRE'
      OnPopupInitPopup = ClasaFunctionalaPopupInitPopup
      OnButtonClick = ClasaFunctionalaButtonClick
      Height = 30
      Width = 663
    end
    object BtnAuto: TcxButton
      Left = 715
      Top = 38
      Width = 79
      Height = 43
      Anchors = [akTop, akRight]
      Caption = 'Identifica'
      Enabled = False
      LookAndFeel.Kind = lfOffice11
      LookAndFeel.SkinName = ''
      TabOrder = 7
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      OnClick = BtnAutoClick
    end
    object BtnGenerare: TcxButton
      Left = 10
      Top = 96
      Width = 143
      Height = 25
      Caption = 'Genereaza Inregistrari'
      Enabled = False
      LookAndFeel.Kind = lfOffice11
      TabOrder = 8
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      ParentFont = False
      OnClick = BtnGenerareClick
    end
    object edClasaEconomica: TcxImageComboBox
      Left = 544
      Top = 36
      Properties.Items = <>
      Properties.OnEditValueChanged = edClasaEconomicaPropertiesEditValueChanged
      TabOrder = 2
      Width = 159
    end
    object edTrim1: TcxImageComboBox
      Left = 196
      Top = 63
      Properties.Items = <>
      Properties.OnEditValueChanged = edTrim1PropertiesEditValueChanged
      TabOrder = 3
      Width = 93
    end
    object edTrim2: TcxImageComboBox
      Left = 333
      Top = 63
      Properties.Items = <>
      Properties.OnEditValueChanged = edTrim2PropertiesEditValueChanged
      TabOrder = 4
      Width = 93
    end
    object edTrim3: TcxImageComboBox
      Left = 472
      Top = 63
      Properties.Items = <>
      Properties.OnEditValueChanged = edTrim3PropertiesEditValueChanged
      TabOrder = 5
      Width = 93
    end
    object edTrim4: TcxImageComboBox
      Left = 610
      Top = 63
      Properties.Items = <>
      Properties.OnEditValueChanged = edTrim4PropertiesEditValueChanged
      TabOrder = 6
      Width = 93
    end
    object pbProgress: TcxProgressBar
      Left = 680
      Top = 93
      Anchors = [akLeft, akRight, akBottom]
      AutoSize = False
      ParentColor = False
      Properties.BarBevelOuter = cxbvRaised
      Properties.BarStyle = cxbsGradient
      Properties.BeginColor = 3265321
      Properties.BorderWidth = 1
      Properties.EndColor = 11399085
      Properties.OverloadValue = 100.000000000000000000
      Properties.PeakValue = 100.000000000000000000
      Properties.SolidTextColor = True
      Style.BorderColor = clInfoBk
      Style.BorderStyle = ebsUltraFlat
      Style.Color = clWhite
      Style.Edges = [bLeft, bTop, bRight, bBottom]
      Style.Shadow = False
      Style.TextStyle = [fsBold]
      Style.TransparentBorder = True
      StyleHot.TextStyle = []
      TabOrder = 9
      Height = 28
      Width = 111
    end
    object ckSameSettings: TcxCheckBox
      Left = 556
      Top = 99
      Caption = 'Aplica setari pt toate'
      TabOrder = 10
    end
    object edRestante: TcxImageComboBox
      Left = 62
      Top = 63
      Properties.Items = <>
      Properties.OnEditValueChanged = edRestantePropertiesEditValueChanged
      TabOrder = 11
      Width = 93
    end
    object edEstimat1: TcxImageComboBox
      Left = 170
      Top = 103
      Properties.Items = <>
      Properties.OnEditValueChanged = edEstimat1PropertiesEditValueChanged
      TabOrder = 12
      Width = 93
    end
    object edEstimat2: TcxImageComboBox
      Left = 274
      Top = 103
      Properties.Items = <>
      Properties.OnEditValueChanged = edEstimat2PropertiesEditValueChanged
      TabOrder = 13
      Width = 93
    end
    object edEstimat3: TcxImageComboBox
      Left = 378
      Top = 103
      Properties.Items = <>
      Properties.OnEditValueChanged = edEstimat3PropertiesEditValueChanged
      TabOrder = 14
      Width = 93
    end
    object btnOpenFile: TcxButton
      Left = 415
      Top = 37
      Width = 21
      Height = 21
      Caption = '...'
      TabOrder = 15
      OnClick = btnOpenFileClick
    end
  end
  object TreeBugete: TcxDBTreeList
    Left = 112
    Top = 200
    Width = 297
    Height = 337
    Bands = <
      item
        Caption.AlignHorz = taCenter
      end>
    DataController.DataSource = frmData.DTBGPlanFunctionalComplet
    DataController.ParentField = 'ID_PARINTE'
    DataController.KeyField = 'ID_BG_PLAN_FUNCTIONAL'
    LookAndFeel.Kind = lfOffice11
    Navigator.Buttons.CustomButtons = <>
    OptionsBehavior.ImmediateEditor = False
    OptionsBehavior.ConfirmDelete = False
    OptionsBehavior.DragCollapse = False
    OptionsBehavior.ExpandOnIncSearch = True
    OptionsBehavior.IncSearch = True
    OptionsBehavior.IncSearchItem = TreeBugeteDESCRIERE
    OptionsBehavior.ShowHourGlass = False
    OptionsCustomizing.BandCustomizing = False
    OptionsCustomizing.BandVertSizing = False
    OptionsCustomizing.ColumnVertSizing = False
    OptionsData.CancelOnExit = False
    OptionsData.Editing = False
    OptionsData.Deleting = False
    OptionsSelection.CellSelect = False
    OptionsSelection.HideFocusRect = False
    OptionsSelection.InvertSelect = False
    OptionsView.CellTextMaxLineCount = -1
    OptionsView.ShowEditButtons = ecsbFocused
    OptionsView.ColumnAutoWidth = True
    ParentColor = False
    Preview.AutoHeight = False
    Preview.MaxLineCount = 2
    RootValue = -1
    ScrollbarAnnotations.CustomAnnotations = <>
    TabOrder = 1
    Visible = False
    OnCustomDrawDataCell = TreeBugeteCustomDrawDataCell
    OnDblClick = TreeBugeteDblClick
    OnKeyDown = TreeBugeteKeyDown
    object TreeBugeteCOD_BUGET: TcxDBTreeListColumn
      Visible = False
      Caption.Text = 'Cod Functional'
      DataBinding.FieldName = 'COD_FUNCTIONAL'
      Width = 106
      Position.ColIndex = 0
      Position.RowIndex = 0
      Position.BandIndex = 0
      SortOrder = soAscending
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeBugeteDENUMIRE: TcxDBTreeListColumn
      Visible = False
      Caption.Text = 'Denumire'
      DataBinding.FieldName = 'DENUMIRE'
      Width = 142
      Position.ColIndex = 2
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeBugeteDESCRIERE: TcxDBTreeListColumn
      Caption.Text = 'Buget'
      DataBinding.FieldName = 'DESCRIERE'
      Width = 100
      Position.ColIndex = 1
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
      OnGetDisplayText = TreeBugeteDESCRIEREGetDisplayText
    end
    object TreeBugeteID_OI_UNITATI: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_OI_UNITATI'
      Width = 100
      Position.ColIndex = 3
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeBugeteID_OI_PROIECTE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_OI_PROIECTE'
      Width = 100
      Position.ColIndex = 4
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
  end
  object fileOpen: TOpenDialog
    DefaultExt = '*.xls'
    Filter = 'Fisiere EXCEL|*.xls;*.xlsx;*.xlsm|All files (*.*)|*.*'
    Title = 'Selectati Fisierul Excel'
    Left = 496
    Top = 88
  end
end
