object frmDetaliiDocum: TfrmDetaliiDocum
  Left = 303
  Top = 275
  Caption = 'Detalii Document'
  ClientHeight = 453
  ClientWidth = 351
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  FormStyle = fsStayOnTop
  OldCreateOrder = False
  OnClose = FormClose
  OnCreate = FormCreate
  OnShow = FormShow
  PixelsPerInch = 96
  TextHeight = 13
  object TreeStructura: TcxDBTreeList
    Left = 8
    Top = 120
    Width = 369
    Height = 185
    Bands = <
      item
        Caption.AlignHorz = taCenter
      end>
    DataController.DataSource = DTStructure
    DataController.ParentField = 'COD_PARINTE'
    DataController.KeyField = 'COD_CB'
    DataController.StateIndexField = 'ICON'
    LookAndFeel.Kind = lfOffice11
    Navigator.Buttons.CustomButtons = <>
    OptionsBehavior.ImmediateEditor = False
    OptionsBehavior.ConfirmDelete = False
    OptionsBehavior.DragCollapse = False
    OptionsBehavior.ExpandOnIncSearch = True
    OptionsBehavior.IncSearch = True
    OptionsBehavior.IncSearchItem = TreeStructuraCRSP_LEI
    OptionsBehavior.ShowHourGlass = False
    OptionsCustomizing.BandCustomizing = False
    OptionsCustomizing.BandVertSizing = False
    OptionsCustomizing.ColumnVertSizing = False
    OptionsData.Editing = False
    OptionsData.Deleting = False
    OptionsSelection.HideFocusRect = False
    OptionsView.CellTextMaxLineCount = -1
    OptionsView.ShowEditButtons = ecsbFocused
    OptionsView.ColumnAutoWidth = True
    ParentColor = False
    Preview.AutoHeight = False
    Preview.MaxLineCount = 2
    RootValue = -1
    ScrollbarAnnotations.CustomAnnotations = <>
    StateImages = ImagesStructura
    Styles.Preview = cxStyle1
    TabOrder = 0
    Visible = False
    OnDblClick = TreeStructuraDblClick
    OnKeyDown = TreeStructuraKeyDown
    object TreeStructuraCOD_CB: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'COD_CB'
      Width = 100
      Position.ColIndex = 11
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeStructuraCOD_PARINTE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'COD_PARINTE'
      Width = 100
      Position.ColIndex = 10
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeStructuraDENUMIRE: TcxDBTreeListColumn
      Caption.Text = 'Denumire Casa'
      DataBinding.FieldName = 'DENUMIRE'
      Width = 157
      Position.ColIndex = 0
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeStructuraDENV: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'DENV'
      Width = 100
      Position.ColIndex = 9
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeStructuraC_O: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'C_O'
      Width = 100
      Position.ColIndex = 12
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeStructuraDATA_SOLD: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'DATA_SOLD'
      Width = 100
      Position.ColIndex = 15
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeStructuraCASIER: TcxDBTreeListColumn
      Visible = False
      Caption.Text = 'Este Casier'
      DataBinding.FieldName = 'CASIER'
      Width = 100
      Position.ColIndex = 14
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeStructuraVALIDATOR: TcxDBTreeListColumn
      Visible = False
      Caption.Text = 'Este Contabil'
      DataBinding.FieldName = 'VALIDATOR'
      Width = 100
      Position.ColIndex = 13
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeStructuraADMIN: TcxDBTreeListColumn
      Visible = False
      Caption.Text = 'Este Administrator'
      DataBinding.FieldName = 'ADMIN'
      Width = 100
      Position.ColIndex = 8
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeStructuraIS_BANCA: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'IS_BANCA'
      Width = 100
      Position.ColIndex = 2
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeStructuraIS_AVANS: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'IS_AVANS'
      Width = 100
      Position.ColIndex = 3
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeStructuraIS_TEMPOR: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'IS_TEMPOR'
      Width = 100
      Position.ColIndex = 1
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeStructuraID_REPARTITORI: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_REPARTITORI'
      Width = 100
      Position.ColIndex = 4
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeStructuraICON: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ICON'
      Width = 100
      Position.ColIndex = 7
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeStructuraID_VALUTA: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_VALUTA'
      Width = 100
      Position.ColIndex = 6
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeStructuraCRSP_LEI: TcxDBTreeListColumn
      Tag = -1
      Caption.Text = 'Cont Contabil'
      DataBinding.FieldName = 'CRSP_LEI'
      Width = 115
      Position.ColIndex = 5
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object TreeStructuraDESCRIERE: TcxDBTreeListColumn
      Caption.Text = 'Descriere'
      DataBinding.FieldName = 'DESCRIERE'
      Width = 95
      Position.ColIndex = 16
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
  end
  object NavPanel: TdxNavBar
    Left = 0
    Top = 0
    Width = 351
    Height = 453
    Align = alClient
    ActiveGroupIndex = 0
    TabOrder = 1
    View = 14
    object grupCentralizareEconomic: TdxNavBarGroup
      Caption = 'Total Coduri Economice'
      SelectedLinkIndex = -1
      TopVisibleLinkIndex = 0
      OptionsGroupControl.ShowControl = True
      OptionsGroupControl.UseControl = True
      Links = <>
    end
    object grupDetaliiDocument: TdxNavBarGroup
      Caption = 'Detalii Document'
      SelectedLinkIndex = -1
      TopVisibleLinkIndex = 0
      OptionsGroupControl.ShowControl = True
      OptionsGroupControl.UseControl = True
      Links = <>
    end
    object grupDocumentConext: TdxNavBarGroup
      Caption = 'Detalii Document Conex'
      SelectedLinkIndex = -1
      TopVisibleLinkIndex = 0
      OptionsGroupControl.ShowControl = True
      OptionsGroupControl.UseControl = True
      Links = <>
    end
    object grupDetaliiPlata: TdxNavBarGroup
      Caption = 'Detalii Document Plata'
      SelectedLinkIndex = -1
      TopVisibleLinkIndex = 0
      OptionsGroupControl.ShowControl = True
      OptionsGroupControl.UseControl = True
      Links = <>
    end
    object grupCentralizareEconomicControl: TdxNavBarGroupControl
      Left = 13
      Top = 37
      Width = 308
      Height = 150
      TabOrder = 0
      GroupIndex = 0
      OriginalHeight = 150
      object gridTotalEconomic: TcxGrid
        Left = 0
        Top = 0
        Width = 308
        Height = 150
        Align = alClient
        TabOrder = 0
        object viewTotalEconomic: TcxGridDBTableView
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          DataController.DataSource = dtDocumentEconomic
          DataController.Summary.DefaultGroupSummaryItems = <>
          DataController.Summary.FooterSummaryItems = <
            item
              Kind = skSum
              Column = viewTotalEconomicfacturat
            end
            item
              Kind = skSum
              Column = viewTotalEconomicangajat
            end>
          DataController.Summary.SummaryGroups = <>
          OptionsData.CancelOnExit = False
          OptionsData.Deleting = False
          OptionsData.DeletingConfirmation = False
          OptionsData.Editing = False
          OptionsData.Inserting = False
          OptionsView.ColumnAutoWidth = True
          OptionsView.Footer = True
          OptionsView.GroupByBox = False
          object viewTotalEconomiccod_functional: TcxGridDBColumn
            Caption = 'CF'
            DataBinding.FieldName = 'cod_functional'
            HeaderAlignmentHorz = taCenter
            Width = 80
          end
          object viewTotalEconomiccodEconomic: TcxGridDBColumn
            Caption = 'CE'
            DataBinding.FieldName = 'codEconomic'
            HeaderAlignmentHorz = taCenter
            Width = 80
          end
          object viewTotalEconomicfacturat: TcxGridDBColumn
            Caption = 'Facturat'
            DataBinding.FieldName = 'facturat'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.DisplayFormat = ',0.00;(-,0.00)'
            HeaderAlignmentHorz = taCenter
            Width = 60
          end
          object viewTotalEconomicangajat: TcxGridDBColumn
            Caption = 'Angajat'
            DataBinding.FieldName = 'angajat'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.DisplayFormat = ',0.00;(-,0.00)'
            HeaderAlignmentHorz = taCenter
            Width = 61
          end
        end
        object nivelTotalEconomic: TcxGridLevel
          GridView = viewTotalEconomic
        end
      end
    end
    object grupDetaliiDocumentControl: TdxNavBarGroupControl
      Left = 13
      Top = 228
      Width = 308
      Height = 150
      TabOrder = 2
      GroupIndex = 1
      OriginalHeight = 150
      object Inspector: TcxDBVerticalGrid
        Left = 0
        Top = 0
        Width = 308
        Height = 150
        Align = alClient
        LookAndFeel.Kind = lfFlat
        OptionsView.CellAutoHeight = True
        OptionsView.CellTextMaxLineCount = 3
        OptionsView.AutoScaleBands = False
        OptionsView.GridLineColor = clBtnShadow
        OptionsView.RowHeaderMinWidth = 30
        OptionsView.RowHeaderWidth = 150
        OptionsView.ValueWidth = 157
        OptionsBehavior.GoToNextCellOnTab = True
        OptionsBehavior.RowSizing = True
        Navigator.Buttons.CustomButtons = <>
        ScrollbarAnnotations.CustomAnnotations = <>
        TabOrder = 0
        Version = 1
      end
    end
    object grupDocumentConextControl: TdxNavBarGroupControl
      Left = 13
      Top = 419
      Width = 308
      Height = 150
      Caption = 'grupDocumentConextControl'
      TabOrder = 3
      GroupIndex = 2
      OriginalHeight = 150
      object grDocConex: TcxGroupBox
        Left = 0
        Top = 0
        Align = alClient
        TabOrder = 0
        Height = 150
        Width = 308
        object lbNrDocConex: TLabel
          Left = 24
          Top = 32
          Width = 63
          Height = 13
          Caption = 'Nr Document'
        end
        object lbDetaliiDocConex: TLabel
          Left = 8
          Top = 8
          Width = 149
          Height = 13
          Caption = 'Detalii Document Conex : '
          Font.Charset = DEFAULT_CHARSET
          Font.Color = clWindowText
          Font.Height = -11
          Font.Name = 'MS Sans Serif'
          Font.Style = [fsBold]
          ParentFont = False
        end
        object lbDataDocConex: TLabel
          Left = 24
          Top = 60
          Width = 75
          Height = 13
          Caption = 'Data Document'
        end
        object lbChitantaPePozitie: TLabel
          Left = 8
          Top = 92
          Width = 316
          Height = 13
          Caption = 'Se emite chitanta pentru fiecare pozitie din document : '
          Font.Charset = DEFAULT_CHARSET
          Font.Color = clWindowText
          Font.Height = -11
          Font.Name = 'MS Sans Serif'
          Font.Style = [fsBold]
          ParentFont = False
        end
        object edDBDataDocConex: TcxDBDateEdit
          Left = 113
          Top = 60
          DataBinding.DataField = 'DATA_DOC_CONEX'
          Properties.ImmediatePost = True
          Properties.InputKind = ikMask
          Properties.SaveTime = False
          Properties.ShowTime = False
          TabOrder = 0
          Width = 112
        end
        object edDBNrDocConex: TcxDBButtonEdit
          Left = 113
          Top = 28
          DataBinding.DataField = 'NR_DOC_CONEX'
          Properties.Buttons = <
            item
              Default = True
              Kind = bkEllipsis
            end>
          TabOrder = 1
          Width = 112
        end
        object chkEmiteChitanta: TcxCheckBox
          Left = 24
          Top = 115
          Caption = 'Chitanta se emite pe pozitie ?'
          TabOrder = 2
          OnClick = chkEmiteChitantaClick
        end
      end
    end
    object grupDetaliiPlataControl: TdxNavBarGroupControl
      Left = 13
      Top = 610
      Width = 308
      Height = 150
      TabOrder = 1
      GroupIndex = 3
      OriginalHeight = 150
      object grDocumentPlata: TcxGroupBox
        Left = 0
        Top = 0
        Align = alClient
        Caption = 'Plata/Incasare'
        TabOrder = 0
        DesignSize = (
          308
          150)
        Height = 150
        Width = 308
        object lbTipDoc: TLabel
          Left = 10
          Top = 71
          Width = 67
          Height = 13
          Caption = 'Tip Document'
        end
        object lnNrDoc: TLabel
          Left = 10
          Top = 95
          Width = 63
          Height = 13
          Caption = 'Nr Document'
        end
        object lbDataDoc: TLabel
          Left = 10
          Top = 118
          Width = 75
          Height = 13
          Caption = 'Data Document'
        end
        object chkAchitat: TcxCheckBox
          Left = 6
          Top = 14
          Caption = 'Se achita/incaseaza'
          Properties.ImmediatePost = True
          Properties.OnValidate = chkAchitatPropertiesValidate
          TabOrder = 0
          OnClick = chkAchitatClick
        end
        object edTipDoc: TcxImageComboBox
          Left = 93
          Top = 66
          Anchors = [akLeft, akTop, akRight]
          Properties.ImmediatePost = True
          Properties.Items = <>
          Properties.OnValidate = edTipDocPropertiesValidate
          TabOrder = 1
          Width = 200
        end
        object edNrDoc: TcxTextEdit
          Left = 93
          Top = 90
          Anchors = [akLeft, akTop, akRight]
          Properties.OnValidate = edNrDocPropertiesValidate
          TabOrder = 2
          Width = 200
        end
        object edDataPlata: TcxDateEdit
          Left = 93
          Top = 114
          Anchors = [akLeft, akTop, akRight]
          Properties.ImmediatePost = True
          Properties.InputKind = ikMask
          Properties.OnValidate = edDataPlataPropertiesValidate
          TabOrder = 3
          Width = 200
        end
        object RPCasaBanca: TcxRepartitorPanel
          Tag = -1
          Left = 8
          Top = 31
          Anchors = [akLeft, akTop, akRight]
          TabOrder = 4
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
          PopupEdit.PopupControl = TreeStructura
          PopupEdit.PopupFlatBorder = False
          PopupEdit.PopupFormBorderStyle = True
          PopupEdit.PopupHeight = 200
          PopupEdit.PopupMinHeight = 100
          PopupEdit.PopupMinWidth = 300
          PopupEdit.PopupSizeable = True
          PopupEdit.PopupWidth = 300
          PopupEdit.Style.Color = clWindow
          PopupEdit.Visible = True
          PopupEdit.Width = 165
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
          DataSource = DTStructure
          CodField = 'CRSP_LEI'
          OnPopupCloseUp = RPCasaBancaPopupCloseUp
          OnPopupInitPopup = RPCasaBancaPopupInitPopup
          OnEditChange = RPCasaBancaEditChange
          OnEditValidate = RPCasaBancaEditValidate
          OnValidate = RPCasaBancaValidate
          Height = 32
          Width = 285
        end
      end
    end
  end
  object DTStructure: TDataSource
    DataSet = QryStructure
    Left = 112
    Top = 104
  end
  object QryStructure: TZReadOnlyQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      
        'EXEC SP_GET_CASA_STRUCTURA :COD_UTILIZATOR,  :IS_ADMIN, :DISP_WA' +
        'Y')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'COD_UTILIZATOR'
        ParamType = ptUnknown
        Size = 4
        Value = 1
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'IS_ADMIN'
        ParamType = ptUnknown
        Size = 4
        Value = 1
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'DISP_WAY'
        ParamType = ptUnknown
        Size = 4
        Value = 0
      end>
    Left = 160
    Top = 104
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'COD_UTILIZATOR'
        ParamType = ptUnknown
        Size = 4
        Value = 1
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'IS_ADMIN'
        ParamType = ptUnknown
        Size = 4
        Value = 1
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'DISP_WAY'
        ParamType = ptUnknown
        Size = 4
        Value = 0
      end>
  end
  object ImagesStructura: TImageList
    DrawingStyle = dsTransparent
    Left = 224
    Top = 40
    Bitmap = {
      494C010108000D00040010001000FFFFFFFFFF10FFFFFFFFFFFFFFFF424D3600
      0000000000003600000028000000400000003000000001002000000000000030
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00F7FFFF008484000084840000C6C6C600F7FFFF00F7FFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00F7FFFF000000000000000000000000000000
      000000000000F7FFFF008484000084848400C6C6C600F7FFFF00F7FFFF00FFFF
      FF00FFFFFF00FFFFFF0000000000000000000000000000000000000000000000
      0000C6C6C600848400008484840084840000C6C6C60084840000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000C6C6C600848400008484840084840000C6C6C60084840000000000000000
      000000000000000000000000000000000000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6C6C60084840000C6C6C6008484000084840000C6C6C600F7FF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF0000000000FFFFFF00000000000000
      0000FFFFFF00C6C6C60084840000848400008484840084840000C6C6C600F7FF
      FF00FFFFFF0000000000FFFFFF0000000000000000000000000000000000C6C6
      C600848400008484840084848400008484000084840000848400848400000000
      000000000000000000000000000000000000000000000000000000000000C6C6
      C600848400008484840084848400848484008484840084848400848400000000
      000000000000000000000000000000000000FFFFFF00FFFFFF00FFFFFF00F7FF
      FF00F7FFFF00C6C6C600C6DEC600C6DEC600C6DEC60084840000848400008484
      0000F7FFFF00FFFFFF00FFFFFF00FFFFFF0000000000FFFFFF0000000000FFFF
      FF00F7FFFF0084840000C6C6C600C6DEC600C6C6C60084840000848484008484
      0000C6DEC600F7FFFF00FFFFFF00FFFFFF000000000000000000000000008484
      0000848484008484000084840000008484000084840000848400008484008484
      0000000000000000000000000000000000000000000000000000000000008484
      000084848400C6C6C60084840000848484008484840084840000848484008484
      000000000000000000000000000000000000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00C6DEC60084840000C6DEC600F7FFFF00C6DEC600C6DEC600C6DEC6008484
      000084848400C6C6C600F7FFFF00F7FFFF00FFFFFF0000000000FFFFFF00FFFF
      FF00C6DEC60084840000C6DEC600F7FFFF00C6C6C600C6C6C600C6C6C6008484
      000084848400C6C6C600F7FFFF00FFFFFF0000000000C6C6C600848400008484
      840084848400848484000084840000FFFF0000FFFF0000FFFF00008484000084
      84008484840084840000C6C6C6000000000000000000C6C6C600848400008484
      8400848400008484840084848400C6C6C600C6DEC600C6DEC600848400008484
      84008484840084840000C6C6C60000000000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00C6C6C600C6C6C600C6DEC600C6DEC600C6DEC600F7FFFF00C6DEC600C6DE
      C600C6DEC60084848400C6C6C600FFFFFF00FFFFFF0000000000FFFFFF00FFFF
      FF0084840000C6C6C600C6DEC600C6C6C600C6DEC600C6DEC600C6C6C600C6C6
      C6008484000084848400C6C6C600F7FFFF00C6C6C60084848400848484008484
      8400848484008484000084840000848400008484000084840000C6DEC6008484
      840084848400008484008484840084840000C6C6C60084840000848484008484
      840084840000C6C6C60084840000C6C6C60084840000C6C6C600C6C6C6008484
      840084848400848484008484840084840000FFFFFF00FFFFFF00FFFFFF00F7FF
      FF0084840000C6DEC600F7FFFF00C6DEC600C6DEC600C6DEC600C6DEC600F7FF
      FF00C6DEC60084840000C6C6C600FFFFFF0000000000FFFFFF00FFFFFF00F7FF
      FF0084840000C6DEC600C6DEC600C6C6C600C6DEC600C6C6C600C6DEC600F7FF
      FF00C6C6C60084840000C6C6C600FFFFFF00848400008484840084848400C6C6
      C600F7FFFF00C6DEC60084848400000084000084840084848400848484008484
      840000848400008484000000840084848400848400008484840084848400C6C6
      C600F7FFFF00C6DEC60084848400000000008484840084848400848484008484
      840084848400848484000000000084848400FFFFFF00FFFFFF00FFFFFF00C6C6
      C600C6DEC600F7FFFF00F7FFFF00C6DEC600F7FFFF00C6DEC600C6DEC600C6DE
      C600C6DEC60084840000F7FFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00C6C6
      C600C6C6C600F7FFFF00C6DEC600C6DEC600C6DEC600C6C6C600C6DEC600C6DE
      C600C6C6C60084840000F7FFFF00FFFFFF0084848400C6C6C600F7FFFF00F7FF
      FF00F7FFFF00C6C6C60084848400848484008484840084848400848400008484
      0000C6C6C60084848400848484008484840084848400C6C6C600F7FFFF00FFFF
      FF00F7FFFF00C6C6C60084848400848484008484840084848400848400008484
      0000C6C6C600848400008484840084848400FFFFFF00FFFFFF00FFFFFF00C6C6
      C600F7FFFF00C6DEC600C6DEC600F7FFFF00C6DEC600C6DEC600C6DEC600C6DE
      C60084840000C6C6C600FFFFFF00FFFFFF00FFFFFF00FFFFFF00F7FFFF008484
      0000C6DEC600C6DEC600C6DEC600C6DEC600C6DEC600C6DEC600C6DEC600C6C6
      C60084840000C6C6C600FFFFFF00FFFFFF00C6C6C60084840000C6DEC600F7FF
      FF00F7FFFF00C6DEC600C6C6C60084840000848484000084840084840000F7FF
      FF00F7FFFF00F7FFFF008484840084840000C6C6C60084840000C6DEC600FFFF
      FF00F7FFFF00C6DEC600C6C6C60084840000848484008484840084840000F7FF
      FF00FFFFFF00F7FFFF008484000084840000FFFFFF00FFFFFF00F7FFFF00C6C6
      C600F7FFFF00F7FFFF00C6DEC600C6DEC600C6C6C600C6C6C600C6C6C600C6DE
      C60084840000F7FFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00C6DEC600C6C6
      C600F7FFFF00F7FFFF00C6DEC600C6DEC600C6C6C600C6C6C600C6DEC600C6C6
      C60084848400F7FFFF00FFFFFF0000000000000000008484000084840000F7FF
      FF00F7FFFF00F7FFFF00C6C6C60084848400848484000084840084848400C6C6
      C600C6DEC6008484840084840000C6C6C600000000008484000084840000F7FF
      FF00FFFFFF00F7FFFF00C6C6C60084848400848400008484840084848400C6C6
      C600C6DEC600848400008484000084840000FFFFFF00FFFFFF00C6C6C600C6DE
      C600C6DEC600C6DEC600F7FFFF00F7FFFF00C6C6C6008484840084840000C6C6
      C60084840000F7FFFF00FFFFFF00FFFFFF00FFFFFF00F7FFFF00C6C6C600C6C6
      C600C6C6C600C6DEC600F7FFFF00F7FFFF008484000084848400848400008484
      000084840000F7FFFF00FFFFFF00000000000000000000000000C6C6C6008484
      0000C6DEC600FFFFFF00F7FFFF008484000084840000C6C6C600848484000084
      840084848400C6C6C600C6C6C600C6C6C6000000000000000000C6C6C6008484
      0000C6DEC600FFFFFF00F7FFFF008484000084840000C6C6C600848400008484
      840084848400C6C6C600C6C6C600C6C6C600FFFFFF00F7FFFF00C6DEC6008484
      84008484840084848400C6DEC600C6DEC600C6C6C60084840000848400008484
      0000F7FFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00F7FFFF00C6C6C6008484
      00008484000084840000C6C6C600C6DEC600C6C6C60084840000848400008484
      8400C6DEC600FFFFFF000000000000000000000000000000000000000000C6C6
      C60084840000C6DEC600FFFFFF00F7FFFF008484840084840000C6DEC6008484
      84008484840084840000C6C6C60000000000000000000000000000000000C6C6
      C60084840000C6DEC600FFFFFF00F7FFFF0084840000C6C6C600C6C6C6008484
      00008484840084840000C6C6C60000000000FFFFFF00F7FFFF00C6DEC6008484
      00008484840000FF000000FF00008484840084840000C6C6C600C6C6C6008484
      0000FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00F7FFFF00C6C6C6008484
      00008484000084840000848484008484000084840000C6C6C600C6C6C6008484
      0000F7FFFF000000000000000000FFFFFF000000000000000000000000000000
      00008484000084840000C6C6C600848400008484840084840000C6C6C6008484
      0000848484008484840084848400C6C6C6000000000000000000000000000000
      00008484000084840000C6C6C60084840000848484008484000000000000C6C6
      C600848400008484840084848400C6C6C600FFFFFF00F7FFFF00C6DEC600C6C6
      C600C6DEC600C6DEC6008484840000FF000000FF00008484840084840000C6C6
      C600FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00F7FFFF00C6DEC600C6C6
      C600C6C6C600C6C6C6008484000084848400848484008484000084840000C6C6
      C600FFFFFF000000000000000000000000000000000000000000000000000000
      000000000000C6C6C60084840000C6C6C600C6C6C600C6C6C60084840000C6C6
      C600C6C6C6008484840084848400848400000000000000000000000000000000
      000000000000C6C6C6008484000084840000C6C6C600C6C6C60084840000C6C6
      C60000000000848484008484840084848400F7FFFF00FFFFFF00FFFFFF00F7FF
      FF00C6C6C600C6C6C600C6C6C60084840000848400008484840084840000F7FF
      FF00F7FFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00F7FFFF00F7FF
      FF00C6C6C600C6C6C600C6C6C60084840000848484008484840084840000F7FF
      FF00FFFFFF000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000C6C6C6008484
      0000848484008484000084848400840084000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000C6C6C6008484
      000084840000848400008484840084848400F7FFFF00FFFFFF00FFFFFF00FFFF
      FF00F7FFFF00C6DEC600C6C6C600C6C6C6008484000084840000C6C6C600FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF0000000000FFFF
      FF00F7FFFF00C6DEC600C6C6C600C6C6C6008484000084840000C6C6C600F7FF
      FF00FFFFFF000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000C6C6
      C6008484840084840000C6C6C600848484000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000C6C6
      C6008484840084840000C6C6C60084848400FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00F7FFFF00F7FFFF00C6C6C60084840000F7FFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF0000000000FFFFFF00FFFFFF0000000000FFFF
      FF00FFFFFF00FFFFFF00F7FFFF00F7FFFF00C6C6C60084840000C6DEC600FFFF
      FF00FFFFFF00FFFFFF0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000C6C6C6008484840084840000848400000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000C6C6C60084848400848400008484000000000000FFFFFF00C6DEC600C6DE
      C600C6C6C6008484840000848400008484000084840084848400C6C6C600C6DE
      C600C6DEC600C6C6C600F7FFFF000000000000000000F7FFFF00C6C6C600C6C6
      C600C6C6C6008484840084848400848400008484840084848400C6C6C600C6C6
      C600C6C6C600C6C6C600F7FFFF000000000000000000FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00F7FFFF00C6C6C60084848400C6DEC60084848400C6DEC600F7FF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF000000000000000000FFFFFF000000
      0000FFFFFF00F7FFFF00C6C6C60084848400C6DEC60084848400C6C6C600F7FF
      FF00FFFFFF000000000000000000FFFFFF00FFFFFF00FFFFFF00C6DEC600C6DE
      C600C6DEC6008484840000848400008484008484840084848400C6DEC600C6DE
      C600C6DEC600C6C6C600F7FFFF000000000000000000F7FFFF00C6DEC600C6C6
      C600C6C6C6008484840084848400848400008484000084848400C6C6C600C6DE
      C600C6C6C600C6C6C600F7FFFF0000000000FFFFFF00FFFFFF00F7FFFF00C6DE
      C600C6C6C6008484000084848400848400008484840084848400848400008484
      000084840000C6DEC600F7FFFF00F7FFFF00FFFFFF00FFFFFF00F7FFFF00C6DE
      C600C6C6C6008484000084848400848484008484000084848400848400008484
      0000C6C6C600C6C6C600F7FFFF00FFFFFF00FFFFFF00FFFFFF00C6DEC600C6DE
      C600C6DEC600848484000084840000FFFF008484000084848400C6DEC600C6DE
      C600C6DEC600C6C6C600F7FFFF000000000000000000F7FFFF00C6DEC600C6DE
      C600C6DEC6008484840084840000848400008484000084848400C6DEC600C6DE
      C600C6C6C600C6C6C600F7FFFF0000000000FFFFFF00C6DEC600848400008484
      8400848484008484840084840000848400008484840084840000848484008484
      0000008400000084000084848400C6DEC600FFFFFF00C6DEC600848400008484
      0000848400008484000084848400848484008484840084848400848484008484
      8400848484008484840084848400C6DEC600FFFFFF00F7FFFF00F7FFFF00F7FF
      FF00C6DEC6008484840084840000C6C6C6008484000084848400F7FFFF00F7FF
      FF00C6DEC600C6C6C600F7FFFF0000000000FFFFFF00F7FFFF00F7FFFF00F7FF
      FF00C6DEC60084840000C6C6C600C6C6C600C6C6C60084848400F7FFFF00F7FF
      FF00C6DEC600C6C6C600F7FFFF0000000000F7FFFF0084840000008400000084
      0000C6C6C600C6DEC60084848400848484008484840084840000848400008484
      000084840000008400000084000084848400F7FFFF0084840000848484008484
      8400C6C6C600C6DEC60084848400848484008484000084848400C6C6C6008484
      000084848400848484008484840084840000FFFFFF00FFFFFF00F7FFFF00F7FF
      FF00F7FFFF0084848400C6C6C600C6C6C6008484000084848400F7FFFF00F7FF
      FF00F7FFFF0084840000F7FFFF0000000000FFFFFF00F7FFFF00F7FFFF00F7FF
      FF00F7FFFF0084848400C6C6C600C6C6C600C6C6C60084840000F7FFFF00F7FF
      FF00C6DEC60084840000F7FFFF00000000008484840084840000848400008484
      000084840000C6DEC60084848400848484008484840084840000C6DEC6008484
      0000848400008484000000840000008400008484000084848400848484008484
      840084840000C6DEC60084848400848484008484000084848400C6C6C600C6C6
      C60084848400848484008484840084848400F7FFFF00C6C6C600C6DEC600F7FF
      FF00F7FFFF008484000084848400848484008484840084848400F7FFFF00F7FF
      FF00F7FFFF0084848400C6C6C60000000000F7FFFF00C6C6C600C6DEC600F7FF
      FF00F7FFFF008484000084848400848484008484840084848400FFFFFF00F7FF
      FF00F7FFFF0084848400C6C6C60000000000848400008484840084848400C6DE
      C600F7FFFF00C6DEC60084848400848484008484840000840000848484008484
      840084840000848400008484000000840000848400008484000084840000C6C6
      C600F7FFFF00C6DEC60084848400848484008484000084848400848400008484
      000084848400848484008484840084848400F7FFFF008484840084848400C6DE
      C600F7FFFF00F7FFFF00F7FFFF00F7FFFF00F7FFFF00F7FFFF00F7FFFF00C6DE
      C6008484840084848400C6DEC60000000000F7FFFF008484840084848400C6DE
      C600F7FFFF00F7FFFF00F7FFFF00FFFFFF00FFFFFF00F7FFFF00F7FFFF00C6DE
      C6008484840084848400C6DEC60000000000F7FFFF00C6DEC600C6DEC600F7FF
      FF00F7FFFF008484000084840000008400008484840000840000848400008484
      000084840000848400008484840084840000F7FFFF00C6DEC600C6DEC600F7FF
      FF00F7FFFF00C6C6C60084848400848484008484840084848400848484008484
      840084848400848484008484840084840000F7FFFF00C6C6C600008484008484
      8400C6C6C600F7FFFF00F7FFFF00F7FFFF00F7FFFF00F7FFFF00C6C6C6008484
      84000000840084848400F7FFFF0000000000F7FFFF00C6C6C600848484008484
      8400C6C6C600F7FFFF00FFFFFF00F7FFFF00F7FFFF00FFFFFF00C6C6C6008484
      84008484840084848400F7FFFF0000000000FFFFFF00F7FFFF00F7FFFF008484
      0000848484008484840084840000848400008484000084840000848400008484
      0000848400008484840084840000F7FFFF00FFFFFF00FFFFFF00F7FFFF008484
      0000848484008484840084848400848484008484840084848400848484008484
      84008484840084848400C6C6C600F7FFFF00FFFFFF00C6C6C600008484000000
      840084848400C6C6C600F7FFFF00F7FFFF00F7FFFF00C6C6C600848484000000
      84000000840084840000F7FFFF0000000000FFFFFF00C6C6C600848484008484
      840084848400C6C6C600F7FFFF00FFFFFF00F7FFFF00C6C6C600848484000000
      00008484840084840000F7FFFF0000000000FFFFFF00C6DEC600848484008484
      0000848400008484000084840000848400008484000084840000848484008484
      8400C6DEC600C6DEC600F7FFFF00FFFFFF00FFFFFF00C6DEC600848484008484
      8400848484008484840084848400848484008484840084848400848484008484
      0000C6C6C600F7FFFF00FFFFFF00FFFFFF00FFFFFF00C6DEC600848484000000
      8400008484008484840084840000F7FFFF00C6C6C60084848400848484008484
      840084848400C6C6C600FFFFFF0000000000FFFFFF00C6DEC600848484008484
      8400848484008484840084840000F7FFFF00C6C6C60084848400848484008484
      840084848400C6C6C600FFFFFF0000000000F7FFFF00C6DEC600848484008484
      000084840000848400008484000084840000848400008484840084840000C6DE
      C600F7FFFF00F7FFFF00F7FFFF00FFFFFF00F7FFFF00C6C6C600848484008484
      8400848484008484840084848400848484008484840084848400C6C6C600F7FF
      FF00F7FFFF00F7FFFF00F7FFFF0000000000FFFFFF00F7FFFF00C6C6C6000084
      8400008484008484840084848400848484008484840084848400C6C6C600C6C6
      C600C6C6C600F7FFFF00FFFFFF0000000000F7FFFF00F7FFFF00C6C6C6008484
      8400848484008484840084848400848484008484840084848400C6DEC600C6DE
      C600C6C6C600F7FFFF00FFFFFF0000000000F7FFFF0084848400848400008484
      0000848484008484840084848400848484008484840084840000C6DEC600C6DE
      C6008484840084848400C6C6C600F7FFFF00F7FFFF0084840000848484008484
      8400848484008484000084848400848484008484000084848400C6C6C600C6C6
      C6008484840084848400C6C6C600F7FFFF00FFFFFF00FFFFFF00F7FFFF00C6C6
      C6008484840084848400848484000084840000008400C6C6C600F7FFFF00F7FF
      FF00C6C6C600F7FFFF00FFFFFF0000000000FFFFFF00FFFFFF00F7FFFF00C6C6
      C6008484840084848400848484008484840084848400C6C6C600F7FFFF00F7FF
      FF00C6C6C600F7FFFF00FFFFFF0000000000C6DEC60084848400848400008484
      000084848400C6DEC60084848400848484008484840084840000848400008484
      8400848400000084000084848400F7FFFF00F7FFFF0084848400848484008484
      840084840000C6DEC60084848400848484008484000084848400848400008484
      0000848484008484840084840000F7FFFF00FFFFFF00FFFFFF00F7FFFF00F7FF
      FF00C6C6C60084848400848484000084840000008400C6C6C600F7FFFF00F7FF
      FF00C6C6C600F7FFFF00FFFFFF0000000000FFFFFF00FFFFFF00FFFFFF00F7FF
      FF00C6C6C60084840000848484008484840084848400C6C6C600F7FFFF00F7FF
      FF00C6C6C600F7FFFF000000000000000000F7FFFF0084840000848484008484
      0000008400008484000084848400848484008484840084840000848400008484
      84008484000084848400C6DEC600F7FFFF00F7FFFF0084840000848484008484
      8400848484008484000084848400848484008484000084848400848400008484
      84008484840084840000C6DEC600FFFFFF00FFFFFF00FFFFFF00FFFFFF00F7FF
      FF00F7FFFF00C6C6C600848400008484840084848400C6DEC600F7FFFF00F7FF
      FF00F7FFFF00F7FFFF00FFFFFF0000000000FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00F7FFFF00C6C6C600848400008484840084848400C6DEC600F7FFFF00F7FF
      FF00F7FFFF00F7FFFF00FFFFFF0000000000FFFFFF00F7FFFF00848400008484
      8400848484008484840084840000008400008484840000840000848484008484
      840084840000C6DEC600F7FFFF00FFFFFF00FFFFFF00F7FFFF00C6C6C6008484
      0000848400008484840084848400848484008484840084848400848484008484
      000084840000C6DEC600FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00F7FFFF00C6DEC600C6C6C600C6C6C600F7FFFF00FFFFFF00FFFF
      FF00F7FFFF00F7FFFF00FFFFFF000000000000000000FFFFFF00F7FFFF00FFFF
      FF00FFFFFF00F7FFFF00C6DEC600C6C6C600C6DEC600F7FFFF00FFFFFF00F7FF
      FF00FFFFFF00FFFFFF00FFFFFF0000000000FFFFFF00FFFFFF00F7FFFF00C6DE
      C600C6DEC600C6DEC60084848400848484008484840084848400C6DEC600C6DE
      C600F7FFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF0000000000F7FFFF00F7FF
      FF00C6DEC600C6C6C60084848400848484008484840084848400C6C6C600F7FF
      FF00F7FFFF0000000000FFFFFF00FFFFFF00F7FFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00F7FFFF00F7FFFF00F7FFFF00F7FFFF00FFFFFF00FFFFFF00FFFF
      FF00F7FFFF00F7FFFF00FFFFFF0000000000F7FFFF0000000000FFFFFF00F7FF
      FF00FFFFFF00FFFFFF00F7FFFF00F7FFFF00F7FFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF0000000000FFFFFF0000000000F7FFFF00FFFFFF00FFFFFF00FFFF
      FF00F7FFFF00C6DEC60084840000C6DEC6008484840084840000F7FFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF00C6DEC60084840000C6DEC6008484000084840000F7FFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00FFFFFF00424D3E000000000000003E000000
      2800000040000000300000000100010000000000800100000000000000000000
      000000000000000000000000FFFFFF0000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000008000F803F03FF03F0000B005E01FE01F
      0000A000E00FE00F000040008001800100004000000000000000800000000000
      00000000000000000000000000000000000000018000800000000001C000C000
      00000003E001E00100000006F000F02000000007F800F80800000007FFC0FFC0
      00002007FFE0FFE000012003FFF0FFF0800180018000D0060001800100000000
      0001800100000000000100010000000000010001000000000001000100000000
      0001000100000000000100010000000000010001000000000001000100000001
      0001000100000000000100010000000000010003000000000001000100000000
      0001800100004004000140050000000000000000000000000000000000000000
      000000000000}
  end
  object cxStyleRepository1: TcxStyleRepository
    Left = 216
    Top = 96
    PixelsPerInch = 96
    object cxStyle1: TcxStyle
      AssignedValues = [svFont, svTextColor]
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clBlue
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
      TextColor = clBlue
    end
  end
  object dtDocumentEconomic: TDataSource
    DataSet = qryDocumentEconomic
    Left = 112
    Top = 48
  end
  object qryDocumentEconomic: TZReadOnlyQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'exec [spAlopCulGestEconomic] :id_culgest_docum'
      '')
    Params = <
      item
        DataType = ftUnknown
        Name = 'id_culgest_docum'
        ParamType = ptUnknown
      end>
    Left = 160
    Top = 48
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'id_culgest_docum'
        ParamType = ptUnknown
      end>
  end
end
