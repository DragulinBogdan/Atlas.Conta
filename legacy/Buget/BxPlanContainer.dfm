object frmBxPlanContainer: TfrmBxPlanContainer
  Left = 278
  Top = 135
  ClientHeight = 610
  ClientWidth = 784
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object gr4: TcxGroupBox
    Left = 4
    Top = 96
    Caption = 'Proiect'
    TabOrder = 3
    DesignSize = (
      500
      57)
    Height = 57
    Width = 500
    object cxLabel6: TcxLabel
      Left = 8
      Top = 21
      Caption = 'Proiect:'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
    end
    object RPProiect4: TcxRepartitorPanel
      Tag = -1
      Left = 163
      Top = 14
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
      PopupEdit.HideEditCursor = False
      PopupEdit.HideSelection = True
      PopupEdit.PopupAutoSize = False
      PopupEdit.PopupAlignment = taLeftJustify
      PopupEdit.PopupClientEdge = False
      PopupEdit.PopupControl = cxTreeProiecte
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 210
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
      KeyField = 'ID_OI_PROIECTE'
      ListField = 'DENUMIRE'
      OnPopupCloseUp = RPFunctional1PopupCloseUp
      OnPopupInitPopup = RPFunctional1PopupInitPopup
      OnEditChange = RPTipBuget3EditChange
      OnEditValidate = RPTipBuget3EditValidate
      OnButtonClick = RPFunctional1ButtonClick
      OnValidate = RPFunctional1Validate
      Height = 31
      Width = 330
    end
  end
  object cxTreeFunctionalComplet: TcxDBTreeList
    Left = 265
    Top = 400
    Width = 250
    Height = 150
    Bands = <
      item
      end>
    DataController.DataSource = frmData.DTBGPlanFunctionalComplet
    DataController.ParentField = 'ID_PARINTE'
    DataController.KeyField = 'ID_BG_PLAN_FUNCTIONAL'
    LookAndFeel.Kind = lfOffice11
    Navigator.Buttons.CustomButtons = <>
    OptionsBehavior.ExpandOnIncSearch = True
    OptionsBehavior.IncSearch = True
    OptionsCustomizing.ColumnsQuickCustomization = True
    OptionsData.Editing = False
    OptionsData.Deleting = False
    OptionsView.ColumnAutoWidth = True
    RootValue = -1
    ScrollbarAnnotations.CustomAnnotations = <>
    TabOrder = 13
    OnCustomDrawDataCell = cxTreeFunctionalCompletCustomDrawDataCell
    OnDblClick = cxTreeFunctionalDblClick
    OnKeyDown = cxTreeFunctionalKeyDown
    object cxTreeFunctionalCompletCOD_FUNCTIONAL: TcxDBTreeListColumn
      Tag = -1
      Visible = False
      DataBinding.FieldName = 'COD_FUNCTIONAL'
      Width = 100
      Position.ColIndex = 9
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalCompletID_BG_TIPURI_BUGET: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_BG_TIPURI_BUGET'
      Width = 100
      Position.ColIndex = 8
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalCompletID_OI_UNITATI: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_OI_UNITATI'
      Width = 100
      Position.ColIndex = 7
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalCompletID_BG_PLAN_FUNCTIONAL: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_BG_PLAN_FUNCTIONAL'
      Width = 100
      Position.ColIndex = 11
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalCompletDENUMIRE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'DENUMIRE'
      Width = 100
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
      Width = 100
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
      Width = 100
      Position.ColIndex = 6
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalCompletID_PARINTE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_PARINTE'
      Width = 100
      Position.ColIndex = 2
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalCompletCLASA: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'CLASA'
      Width = 100
      Position.ColIndex = 1
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalCompletCAPITOL: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'CAPITOL'
      Width = 100
      Position.ColIndex = 0
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalCompletESTE_LUCRARE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ESTE_LUCRARE'
      Width = 100
      Position.ColIndex = 5
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalCompletTIP_BUGET: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'TIP_BUGET'
      Width = 100
      Position.ColIndex = 4
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalCompletESTE_STANDARD: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ESTE_STANDARD'
      Width = 100
      Position.ColIndex = 3
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalCompletID_OI_PROIECTE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_OI_PROIECTE'
      Width = 100
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
      Width = 100
      Position.ColIndex = 14
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
  end
  object gr1: TcxGroupBox
    Left = 8
    Top = 6
    Caption = 'Functional'
    TabOrder = 0
    DesignSize = (
      500
      49)
    Height = 49
    Width = 500
    object cxLabel1: TcxLabel
      Left = 8
      Top = 17
      Caption = 'Clasificatie Functionala:'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
    end
    object RPFunctional1: TcxRepartitorPanel
      Tag = -1
      Left = 163
      Top = 10
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
      PopupEdit.HideEditCursor = False
      PopupEdit.HideSelection = True
      PopupEdit.PopupAutoSize = False
      PopupEdit.PopupAlignment = taLeftJustify
      PopupEdit.PopupClientEdge = False
      PopupEdit.PopupControl = cxTreeFunctional
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 210
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
      OnPopupCloseUp = RPFunctional1PopupCloseUp
      OnPopupInitPopup = RPFunctional1PopupInitPopup
      OnEditChange = RPTipBuget3EditChange
      OnEditValidate = RPTipBuget3EditValidate
      OnButtonClick = RPFunctional1ButtonClick
      OnValidate = RPFunctional1Validate
      Height = 31
      Width = 330
    end
  end
  object cxTreeFunctional: TcxDBTreeList
    Left = 9
    Top = 400
    Width = 250
    Height = 150
    Bands = <
      item
      end>
    DataController.DataSource = frmData.DTBGPlanFunctional
    DataController.ParentField = 'ID_PARINTE'
    DataController.KeyField = 'ID_BG_PLAN_FUNCTIONAL'
    LookAndFeel.Kind = lfOffice11
    Navigator.Buttons.CustomButtons = <>
    OptionsBehavior.ExpandOnIncSearch = True
    OptionsBehavior.IncSearch = True
    OptionsBehavior.IncSearchItem = cxTreeFunctionalDESCRIERE
    OptionsCustomizing.ColumnsQuickCustomization = True
    OptionsData.Editing = False
    OptionsData.Deleting = False
    OptionsView.ColumnAutoWidth = True
    RootValue = -1
    ScrollbarAnnotations.CustomAnnotations = <>
    TabOrder = 6
    OnDblClick = cxTreeFunctionalDblClick
    OnKeyDown = cxTreeFunctionalKeyDown
    object cxTreeFunctionalCOD_FUNCTIONAL: TcxDBTreeListColumn
      Tag = -1
      Visible = False
      DataBinding.FieldName = 'COD_FUNCTIONAL'
      Width = 100
      Position.ColIndex = 9
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalID_BG_TIPURI_BUGET: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_BG_TIPURI_BUGET'
      Width = 100
      Position.ColIndex = 8
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalID_OI_UNITATI: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_OI_UNITATI'
      Width = 100
      Position.ColIndex = 7
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalID_BG_PLAN_FUNCTIONAL: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_BG_PLAN_FUNCTIONAL'
      Width = 100
      Position.ColIndex = 12
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalDENUMIRE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'DENUMIRE'
      Width = 100
      Position.ColIndex = 11
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalDESCRIERE: TcxDBTreeListColumn
      Caption.AlignHorz = taCenter
      Caption.Text = 'Cap. Subcap. Paragraf.'
      DataBinding.FieldName = 'COD_FUNCTIONAL'
      Width = 100
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
      Width = 100
      Position.ColIndex = 6
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalID_PARINTE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_PARINTE'
      Width = 100
      Position.ColIndex = 2
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalCLASA: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'CLASA'
      Width = 100
      Position.ColIndex = 1
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalCAPITOL: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'CAPITOL'
      Width = 100
      Position.ColIndex = 0
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalESTE_LUCRARE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ESTE_LUCRARE'
      Width = 100
      Position.ColIndex = 5
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalTIP_BUGET: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'TIP_BUGET'
      Width = 100
      Position.ColIndex = 4
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeFunctionalESTE_STANDARD: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ESTE_STANDARD'
      Width = 100
      Position.ColIndex = 3
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
  end
  object cxTreeUnitati: TcxDBTreeList
    Left = 9
    Top = 444
    Width = 250
    Height = 150
    Bands = <
      item
      end>
    DataController.DataSource = frmData.DTOIUnitati
    DataController.ParentField = 'ID_PARINTE'
    DataController.KeyField = 'ID_OI_UNITATI'
    LookAndFeel.Kind = lfOffice11
    Navigator.Buttons.CustomButtons = <>
    OptionsBehavior.ExpandOnIncSearch = True
    OptionsBehavior.IncSearch = True
    OptionsBehavior.IncSearchItem = cxTreeUnitatiDESCRIERE
    OptionsCustomizing.ColumnsQuickCustomization = True
    OptionsData.Editing = False
    OptionsData.Deleting = False
    OptionsView.ColumnAutoWidth = True
    RootValue = -1
    ScrollbarAnnotations.CustomAnnotations = <>
    TabOrder = 7
    OnDblClick = cxTreeUnitatiDblClick
    OnKeyDown = cxTreeFunctionalKeyDown
    object cxTreeUnitatiID_OI_UNITATI: TcxDBTreeListColumn
      Tag = -1
      Visible = False
      DataBinding.FieldName = 'ID_OI_UNITATI'
      Width = 100
      Position.ColIndex = 3
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiID_OI_UNITATI_TIPURI: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_OI_UNITATI_TIPURI'
      Width = 100
      Position.ColIndex = 4
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiID_PARINTE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_PARINTE'
      Width = 100
      Position.ColIndex = 2
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiDENUMIRE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'DENUMIRE'
      Width = 100
      Position.ColIndex = 0
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
      OnGetDisplayText = cxTreeUnitatiDENUMIREGetDisplayText
    end
    object cxTreeUnitatiDESCRIERE: TcxDBTreeListColumn
      Caption.AlignHorz = taCenter
      Caption.GlyphAlignHorz = taCenter
      Caption.Text = 'Institutie/Unitate'
      DataBinding.FieldName = 'DESCRIERE'
      Width = 100
      Position.ColIndex = 1
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
      OnGetDisplayText = cxTreeUnitatiDESCRIEREGetDisplayText
    end
    object cxTreeUnitatiUNITATATEA_URMARITA: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'UNITATATEA_URMARITA'
      Width = 100
      Position.ColIndex = 11
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiNUME_ORDONANTATOR: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'NUME_ORDONANTATOR'
      Width = 100
      Position.ColIndex = 10
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiID_UTILIZATORI: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_UTILIZATORI'
      Width = 100
      Position.ColIndex = 13
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiSTARE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'STARE'
      Width = 100
      Position.ColIndex = 12
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiUNITATEA_CENTRALIZATOARE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'UNITATEA_CENTRALIZATOARE'
      Width = 100
      Position.ColIndex = 9
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiBANCA: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'BANCA'
      Width = 100
      Position.ColIndex = 6
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiBANCA_COD: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'BANCA_COD'
      Width = 100
      Position.ColIndex = 5
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiBANCA_CONT: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'BANCA_CONT'
      Width = 100
      Position.ColIndex = 8
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeUnitatiCOD_FUNCTIONAL: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'COD_FUNCTIONAL'
      Width = 100
      Position.ColIndex = 7
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
  end
  object cxListTipBuget: TcxDBTreeList
    Left = 265
    Top = 444
    Width = 250
    Height = 150
    Bands = <
      item
      end>
    DataController.DataSource = frmData.DTBGTipuriBuget
    DataController.ParentField = 'ID_BG_TIPURI_BUGET'
    DataController.KeyField = 'ID_BG_TIPURI_BUGET'
    LookAndFeel.Kind = lfOffice11
    Navigator.Buttons.CustomButtons = <>
    OptionsBehavior.ExpandOnIncSearch = True
    OptionsBehavior.IncSearch = True
    OptionsBehavior.IncSearchItem = cxListTipBugetDENUMIRE
    OptionsCustomizing.ColumnsQuickCustomization = True
    OptionsData.Editing = False
    OptionsData.Deleting = False
    OptionsView.ColumnAutoWidth = True
    OptionsView.ShowRoot = False
    RootValue = -1
    ScrollbarAnnotations.CustomAnnotations = <>
    TabOrder = 8
    OnDblClick = cxTreeFunctionalDblClick
    OnKeyDown = cxTreeFunctionalKeyDown
    object cxListTipBugetID_BG_TIPURI_BUGET: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_BG_TIPURI_BUGET'
      Width = 100
      Position.ColIndex = 1
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxListTipBugetDENUMIRE: TcxDBTreeListColumn
      Caption.AlignHorz = taCenter
      Caption.Text = 'Tip Buget'
      DataBinding.FieldName = 'DENUMIRE'
      Width = 100
      Position.ColIndex = 0
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
      OnGetDisplayText = cxListTipBugetDENUMIREGetDisplayText
    end
    object cxListTipBugetTIP_BUGET: TcxDBTreeListColumn
      Tag = -1
      Visible = False
      DataBinding.FieldName = 'TIP_BUGET'
      Width = 100
      Position.ColIndex = 2
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
  end
  object gr7: TcxGroupBox
    Left = 4
    Top = 217
    Caption = 'Economic'
    TabOrder = 9
    DesignSize = (
      500
      51)
    Height = 51
    Width = 500
    object cxLabel12: TcxLabel
      Left = 8
      Top = 17
      Caption = 'Clasificatie Economica'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
    end
    object RPEconomic7: TcxRepartitorPanel
      Tag = -1
      Left = 163
      Top = 10
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
      PopupEdit.HideEditCursor = False
      PopupEdit.HideSelection = True
      PopupEdit.PopupAutoSize = False
      PopupEdit.PopupAlignment = taLeftJustify
      PopupEdit.PopupClientEdge = False
      PopupEdit.PopupControl = cxTreeEconomic
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 210
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
      OnPopupCloseUp = RPFunctional1PopupCloseUp
      OnPopupInitPopup = RPFunctional1PopupInitPopup
      OnEditChange = RPTipBuget3EditChange
      OnEditValidate = RPTipBuget3EditValidate
      OnButtonClick = RPFunctional1ButtonClick
      OnValidate = RPFunctional1Validate
      Height = 31
      Width = 330
    end
  end
  object cxTreeEconomic: TcxDBTreeList
    Left = 521
    Top = 400
    Width = 250
    Height = 150
    Bands = <
      item
      end>
    DataController.DataSource = frmData.DTBGPlanEconomic
    DataController.ParentField = 'ID_PARINTE'
    DataController.KeyField = 'ID_BG_PLAN_ECONOMIC'
    LookAndFeel.Kind = lfOffice11
    Navigator.Buttons.CustomButtons = <>
    OptionsBehavior.ExpandOnIncSearch = True
    OptionsBehavior.IncSearch = True
    OptionsBehavior.IncSearchItem = cxTreeEconomicDESCRIERE
    OptionsCustomizing.ColumnsQuickCustomization = True
    OptionsData.Editing = False
    OptionsData.Deleting = False
    OptionsView.ColumnAutoWidth = True
    RootValue = -1
    ScrollbarAnnotations.CustomAnnotations = <>
    TabOrder = 10
    OnDblClick = cxTreeFunctionalDblClick
    OnKeyDown = cxTreeFunctionalKeyDown
    object cxTreeEconomicID_BG_PLAN_ECONOMIC: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_BG_PLAN_ECONOMIC'
      Width = 100
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
      Width = 100
      Position.ColIndex = 3
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeEconomicDENUMIRE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'DENUMIRE'
      Width = 100
      Position.ColIndex = 0
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeEconomicDESCRIERE: TcxDBTreeListColumn
      Caption.AlignHorz = taCenter
      Caption.Text = 'Titlu, Art. Aliniat'
      DataBinding.FieldName = 'DESCRIERE'
      Width = 100
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
      Width = 100
      Position.ColIndex = 6
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeEconomicID_PARINTE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_PARINTE'
      Width = 100
      Position.ColIndex = 7
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeEconomicCLASA: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'CLASA'
      Width = 100
      Position.ColIndex = 4
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeEconomicESTE_LOCAL: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ESTE_LOCAL'
      Width = 100
      Position.ColIndex = 5
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
  end
  object gr8: TcxGroupBox
    Left = 86
    Top = 217
    Caption = 'Functional Economic'
    TabOrder = 11
    DesignSize = (
      500
      82)
    Height = 82
    Width = 500
    object cxLabel13: TcxLabel
      Left = 8
      Top = 16
      Caption = 'Clasificatie Functionala:'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
    end
    object RPFunctional8: TcxRepartitorPanel
      Tag = -1
      Left = 163
      Top = 9
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
      PopupEdit.HideEditCursor = False
      PopupEdit.HideSelection = True
      PopupEdit.PopupAutoSize = False
      PopupEdit.PopupAlignment = taLeftJustify
      PopupEdit.PopupClientEdge = False
      PopupEdit.PopupControl = cxTreeFunctional
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 210
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
      OnPopupCloseUp = RPFunctional1PopupCloseUp
      OnPopupInitPopup = RPFunctional1PopupInitPopup
      OnEditChange = RPTipBuget3EditChange
      OnEditValidate = RPTipBuget3EditValidate
      OnButtonClick = RPFunctional1ButtonClick
      OnValidate = RPFunctional1Validate
      Height = 31
      Width = 330
    end
    object cxLabel14: TcxLabel
      Left = 8
      Top = 49
      Caption = 'Clasificatie Economica'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
    end
    object RPEconomic8: TcxRepartitorPanel
      Tag = -1
      Left = 163
      Top = 42
      Anchors = [akLeft, akTop, akRight]
      TabOrder = 3
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
      PopupEdit.PopupControl = cxTreeEconomic
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 210
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
      OnPopupCloseUp = RPFunctional1PopupCloseUp
      OnPopupInitPopup = RPFunctional1PopupInitPopup
      OnEditChange = RPTipBuget3EditChange
      OnEditValidate = RPTipBuget3EditValidate
      OnButtonClick = RPFunctional1ButtonClick
      OnValidate = RPFunctional1Validate
      Height = 31
      Width = 330
    end
  end
  object gr9: TcxGroupBox
    Left = 241
    Top = 217
    Caption = 'Proiect Functional'
    TabOrder = 12
    DesignSize = (
      500
      85)
    Height = 85
    Width = 500
    object cxLabel15: TcxLabel
      Left = 8
      Top = 52
      Caption = 'Clasificatie Functionala:'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
    end
    object RPFunctional9: TcxRepartitorPanel
      Tag = -1
      Left = 163
      Top = 45
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
      PopupEdit.HideEditCursor = False
      PopupEdit.HideSelection = True
      PopupEdit.PopupAutoSize = False
      PopupEdit.PopupAlignment = taLeftJustify
      PopupEdit.PopupClientEdge = False
      PopupEdit.PopupControl = cxTreeFunctionalComplet
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 210
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
      OnPopupCloseUp = RPFunctional1PopupCloseUp
      OnPopupInitPopup = RPFunctional1PopupInitPopup
      OnEditChange = RPTipBuget3EditChange
      OnEditValidate = RPTipBuget3EditValidate
      OnButtonClick = RPFunctional1ButtonClick
      OnValidate = RPFunctional1Validate
      Height = 31
      Width = 330
    end
    object cxLabel16: TcxLabel
      Left = 8
      Top = 21
      Caption = 'Proiect:'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
    end
    object RPProiect9: TcxRepartitorPanel
      Tag = -1
      Left = 163
      Top = 14
      Anchors = [akLeft, akTop, akRight]
      TabOrder = 3
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
      PopupEdit.PopupControl = cxTreeProiecte
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 210
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
      KeyField = 'ID_OI_PROIECTE'
      ListField = 'DENUMIRE'
      OnPopupCloseUp = RPFunctional1PopupCloseUp
      OnPopupInitPopup = RPFunctional1PopupInitPopup
      OnEditChange = RPTipBuget3EditChange
      OnEditValidate = RPTipBuget3EditValidate
      OnButtonClick = RPFunctional1ButtonClick
      OnValidate = RPFunctional1Validate
      Height = 31
      Width = 330
    end
  end
  object gr10: TcxGroupBox
    Left = 4
    Top = 308
    Caption = 'Functional'
    TabOrder = 14
    DesignSize = (
      500
      49)
    Height = 49
    Width = 500
    object cxLabel17: TcxLabel
      Left = 8
      Top = 18
      Caption = 'Clasificatie Functionala:'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
    end
    object RPFunctional10: TcxRepartitorPanel
      Tag = -1
      Left = 162
      Top = 11
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
      PopupEdit.HideEditCursor = False
      PopupEdit.HideSelection = True
      PopupEdit.PopupAutoSize = False
      PopupEdit.PopupAlignment = taLeftJustify
      PopupEdit.PopupClientEdge = False
      PopupEdit.PopupControl = cxTreeFunctionalComplet
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 210
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
      OnPopupCloseUp = RPFunctional1PopupCloseUp
      OnPopupInitPopup = RPFunctional1PopupInitPopup
      OnEditChange = RPTipBuget3EditChange
      OnEditValidate = RPTipBuget3EditValidate
      OnButtonClick = RPFunctional1ButtonClick
      OnValidate = RPFunctional1Validate
      Height = 31
      Width = 330
    end
  end
  object cxTreeProiecte: TcxDBTreeList
    Left = 521
    Top = 444
    Width = 250
    Height = 150
    Bands = <
      item
      end>
    DataController.DataSource = frmData.DTOIProiecte
    DataController.ParentField = 'id_parinte'
    DataController.KeyField = 'id_oi_proiecte'
    LookAndFeel.Kind = lfOffice11
    Navigator.Buttons.CustomButtons = <>
    OptionsBehavior.ExpandOnIncSearch = True
    OptionsBehavior.IncSearch = True
    OptionsBehavior.IncSearchItem = cxTreeProiecteDESCRIERE
    OptionsCustomizing.ColumnsQuickCustomization = True
    OptionsData.Editing = False
    OptionsData.Deleting = False
    OptionsView.ColumnAutoWidth = True
    RootValue = -1
    ScrollbarAnnotations.CustomAnnotations = <>
    TabOrder = 15
    OnDblClick = cxTreeFunctionalDblClick
    OnKeyDown = cxTreeFunctionalKeyDown
    object cxTreeProiecteID_OI_PROIECTE: TcxDBTreeListColumn
      Tag = -1
      Visible = False
      DataBinding.FieldName = 'id_oi_proiecte'
      Width = 100
      Position.ColIndex = 3
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeProiecteID_OI_TIPURI_PROIECTE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_OI_TIPURI_PROIECTE'
      Width = 100
      Position.ColIndex = 4
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeProiecteID_PARINTE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'id_parinte'
      Width = 100
      Position.ColIndex = 2
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeProiecteDENUMIRE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'Denumire'
      Width = 100
      Position.ColIndex = 0
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeProiecteDESCRIERE: TcxDBTreeListColumn
      Caption.AlignHorz = taCenter
      Caption.Text = 'Descriere'
      DataBinding.FieldName = 'DESCRIERE'
      Width = 100
      Position.ColIndex = 1
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
      OnGetDisplayText = cxTreeProiecteDESCRIEREGetDisplayText
    end
    object cxTreeProiecteSTARE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'STARE'
      Width = 100
      Position.ColIndex = 6
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeProiecteCOD_FUNCTIONAL: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'cod_functional'
      Width = 100
      Position.ColIndex = 5
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
  end
  object gr11: TcxGroupBox
    Left = 265
    Top = 308
    Caption = 'Economic Unitate'
    TabOrder = 16
    DesignSize = (
      500
      85)
    Height = 85
    Width = 500
    object cxLabel18: TcxLabel
      Left = 8
      Top = 17
      Caption = 'Clasificatie Economica'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
    end
    object RPEconomic11: TcxRepartitorPanel
      Tag = -1
      Left = 163
      Top = 10
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
      PopupEdit.HideEditCursor = False
      PopupEdit.HideSelection = True
      PopupEdit.PopupAutoSize = False
      PopupEdit.PopupAlignment = taLeftJustify
      PopupEdit.PopupClientEdge = False
      PopupEdit.PopupControl = cxTreeEconomic
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 210
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
      OnPopupCloseUp = RPFunctional1PopupCloseUp
      OnPopupInitPopup = RPFunctional1PopupInitPopup
      OnEditChange = RPTipBuget3EditChange
      OnEditValidate = RPTipBuget3EditValidate
      OnButtonClick = RPFunctional1ButtonClick
      OnValidate = RPFunctional1Validate
      Height = 31
      Width = 330
    end
    object cxLabel19: TcxLabel
      Left = 8
      Top = 53
      Caption = 'Unitate:'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
    end
    object RBUnitate11: TcxRepartitorPanel
      Tag = -1
      Left = 163
      Top = 46
      Anchors = [akLeft, akTop, akRight]
      TabOrder = 3
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
      PopupEdit.PopupControl = cxTreeUnitati
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 210
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
      CodField = 'ID_OI_UNITATI'
      KeyField = 'ID_OI_UNITATI'
      ListField = 'DENUMIRE'
      OnPopupCloseUp = RBUnitate12PopupCloseUp
      OnPopupInitPopup = RBUnitate12PopupInitPopup
      OnEditChange = RPTipBuget3EditChange
      OnEditValidate = RPTipBuget3EditValidate
      OnButtonClick = RPFunctional1ButtonClick
      OnValidate = RBUnitate12Validate
      Height = 31
      Width = 330
    end
  end
  object gr2: TcxGroupBox
    Left = 176
    Top = 9
    Caption = 'Unitate Functional'
    Ctl3D = True
    ParentCtl3D = False
    TabOrder = 1
    DesignSize = (
      500
      84)
    Height = 84
    Width = 500
    object cxLabel2: TcxLabel
      Left = 8
      Top = 52
      Caption = 'Clasificatie Functionala:'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
    end
    object RPFunctional2: TcxRepartitorPanel
      Tag = -1
      Left = 163
      Top = 45
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
      PopupEdit.HideEditCursor = False
      PopupEdit.HideSelection = True
      PopupEdit.PopupAutoSize = False
      PopupEdit.PopupAlignment = taLeftJustify
      PopupEdit.PopupClientEdge = False
      PopupEdit.PopupControl = cxTreeFunctional
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 210
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
      OnPopupCloseUp = RPFunctional1PopupCloseUp
      OnPopupInitPopup = RPFunctional1PopupInitPopup
      OnEditChange = RPTipBuget3EditChange
      OnEditValidate = RPTipBuget3EditValidate
      OnButtonClick = RPFunctional1ButtonClick
      OnValidate = RPFunctional1Validate
      Height = 31
      Width = 330
    end
    object cxLabel3: TcxLabel
      Left = 8
      Top = 19
      Caption = 'Unitate:'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
    end
    object RBUnitate12: TcxRepartitorPanel
      Tag = -1
      Left = 163
      Top = 12
      Anchors = [akLeft, akTop, akRight]
      TabOrder = 3
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
      PopupEdit.PopupControl = cxTreeUnitati
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 210
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
      CodField = 'ID_OI_UNITATI'
      KeyField = 'ID_OI_UNITATI'
      ListField = 'DENUMIRE'
      OnPopupCloseUp = RBUnitate12PopupCloseUp
      OnPopupInitPopup = RBUnitate12PopupInitPopup
      OnEditChange = RPTipBuget3EditChange
      OnEditValidate = RPTipBuget3EditValidate
      OnButtonClick = RPFunctional1ButtonClick
      OnValidate = RBUnitate12Validate
      Height = 31
      Width = 330
    end
  end
  object gr3: TcxGroupBox
    Left = 292
    Top = 8
    Caption = 'Tip Buget Functional'
    TabOrder = 2
    DesignSize = (
      500
      82)
    Height = 82
    Width = 500
    object cxLabel4: TcxLabel
      Left = 3
      Top = 51
      Caption = 'Clasificatie Functionala:'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
    end
    object RPFunctional3: TcxRepartitorPanel
      Tag = -1
      Left = 163
      Top = 42
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
      PopupEdit.HideEditCursor = False
      PopupEdit.HideSelection = True
      PopupEdit.PopupAutoSize = False
      PopupEdit.PopupAlignment = taLeftJustify
      PopupEdit.PopupClientEdge = False
      PopupEdit.PopupControl = cxTreeFunctional
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 210
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
      OnPopupCloseUp = RPFunctional3PopupCloseUp
      OnPopupInitPopup = RPFunctional3PopupInitPopup
      OnEditChange = RPTipBuget3EditChange
      OnEditValidate = RPTipBuget3EditValidate
      OnButtonClick = RPFunctional1ButtonClick
      OnValidate = RPFunctional1Validate
      Height = 31
      Width = 330
    end
    object cxLabel5: TcxLabel
      Left = 8
      Top = 20
      Caption = 'Tip Buget :'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
    end
    object RPTipBuget3: TcxRepartitorPanel
      Tag = -1
      Left = 163
      Top = 13
      Anchors = [akLeft, akTop, akRight]
      TabOrder = 3
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
      PopupEdit.PopupControl = cxListTipBuget
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 210
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
      CodField = 'ID_BG_TIPURI_BUGET'
      OnPopupCloseUp = RPFunctional1PopupCloseUp
      OnPopupInitPopup = RPFunctional1PopupInitPopup
      OnEditChange = RPTipBuget3EditChange
      OnEditValidate = RPTipBuget3EditValidate
      OnButtonClick = RPFunctional1ButtonClick
      OnValidate = RPTipBuget3Validate
      Height = 31
      Width = 330
    end
  end
  object gr5: TcxGroupBox
    Left = 142
    Top = 88
    Caption = 'Proiect - Tip buget'
    TabOrder = 4
    DesignSize = (
      500
      84)
    Height = 84
    Width = 500
    object cxLabel7: TcxLabel
      Left = 8
      Top = 18
      Caption = 'Proiect:'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
    end
    object RPProiect5: TcxRepartitorPanel
      Tag = -1
      Left = 163
      Top = 11
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
      PopupEdit.HideEditCursor = False
      PopupEdit.HideSelection = True
      PopupEdit.PopupAutoSize = False
      PopupEdit.PopupAlignment = taLeftJustify
      PopupEdit.PopupClientEdge = False
      PopupEdit.PopupControl = cxTreeProiecte
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 210
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
      KeyField = 'ID_OI_PROIECTE'
      ListField = 'DENUMIRE'
      OnPopupCloseUp = RPFunctional1PopupCloseUp
      OnPopupInitPopup = RPFunctional1PopupInitPopup
      OnEditChange = RPTipBuget3EditChange
      OnEditValidate = RPTipBuget3EditValidate
      OnButtonClick = RPFunctional1ButtonClick
      OnValidate = RPFunctional1Validate
      Height = 31
      Width = 330
    end
    object cxLabel8: TcxLabel
      Left = 8
      Top = 50
      Caption = 'Tip Buget :'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
    end
    object RPTipBuget5: TcxRepartitorPanel
      Tag = -1
      Left = 163
      Top = 43
      Anchors = [akLeft, akTop, akRight]
      TabOrder = 3
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
      PopupEdit.PopupControl = cxListTipBuget
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 210
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
      CodField = 'ID_BG_TIPURI_BUGET'
      OnPopupCloseUp = RPFunctional1PopupCloseUp
      OnPopupInitPopup = RPFunctional1PopupInitPopup
      OnEditChange = RPTipBuget3EditChange
      OnEditValidate = RPTipBuget3EditValidate
      OnButtonClick = RPFunctional1ButtonClick
      OnValidate = RPFunctional1Validate
      Height = 31
      Width = 330
    end
  end
  object gr6: TcxGroupBox
    Left = 276
    Top = 96
    Caption = 'Unitate Tip Buget Functional'
    TabOrder = 5
    DesignSize = (
      500
      113)
    Height = 113
    Width = 500
    object cxLabel9: TcxLabel
      Left = 8
      Top = 78
      Caption = 'Clasificatie Functionala:'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
    end
    object RPFunctional6: TcxRepartitorPanel
      Tag = -1
      Left = 163
      Top = 71
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
      PopupEdit.HideEditCursor = False
      PopupEdit.HideSelection = True
      PopupEdit.PopupAutoSize = False
      PopupEdit.PopupAlignment = taLeftJustify
      PopupEdit.PopupClientEdge = False
      PopupEdit.PopupControl = cxTreeFunctional
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 210
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
      OnPopupCloseUp = RPFunctional1PopupCloseUp
      OnPopupInitPopup = RPFunctional1PopupInitPopup
      OnEditChange = RPTipBuget3EditChange
      OnEditValidate = RPTipBuget3EditValidate
      OnButtonClick = RPFunctional1ButtonClick
      OnValidate = RPFunctional1Validate
      Height = 31
      Width = 330
    end
    object cxLabel10: TcxLabel
      Left = 8
      Top = 18
      Caption = 'Unitate:'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
    end
    object cxLabel11: TcxLabel
      Left = 8
      Top = 49
      Caption = 'Tip Buget :'
      ParentFont = False
      Style.Font.Charset = DEFAULT_CHARSET
      Style.Font.Color = clWindowText
      Style.Font.Height = -11
      Style.Font.Name = 'MS Sans Serif'
      Style.Font.Style = [fsBold]
      Style.IsFontAssigned = True
    end
    object RPTipBuget6: TcxRepartitorPanel
      Tag = -1
      Left = 163
      Top = 42
      Anchors = [akLeft, akTop, akRight]
      TabOrder = 5
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
      PopupEdit.PopupControl = cxListTipBuget
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 210
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
      CodField = 'ID_BG_TIPURI_BUGET'
      OnPopupCloseUp = RPFunctional1PopupCloseUp
      OnPopupInitPopup = RPTipBuget6PopupInitPopup
      OnEditChange = RPTipBuget3EditChange
      OnEditValidate = RPTipBuget3EditValidate
      OnButtonClick = RPFunctional1ButtonClick
      OnValidate = RPTipBuget6Validate
      Height = 31
      Width = 330
    end
    object RPUnitate6: TcxRepartitorPanel
      Tag = -1
      Left = 163
      Top = 11
      Anchors = [akLeft, akTop, akRight]
      TabOrder = 3
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
      PopupEdit.PopupControl = cxTreeUnitati
      PopupEdit.PopupFlatBorder = False
      PopupEdit.PopupFormBorderStyle = True
      PopupEdit.PopupHeight = 200
      PopupEdit.PopupMinHeight = 100
      PopupEdit.PopupMinWidth = 300
      PopupEdit.PopupSizeable = True
      PopupEdit.PopupWidth = 300
      PopupEdit.Style.Color = clWindow
      PopupEdit.Visible = True
      PopupEdit.Width = 210
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
      CodField = 'ID_OI_UNITATI'
      OnPopupCloseUp = RBUnitate12PopupCloseUp
      OnPopupInitPopup = RBUnitate12PopupInitPopup
      OnEditChange = RPTipBuget3EditChange
      OnEditValidate = RPTipBuget3EditValidate
      OnButtonClick = RPFunctional1ButtonClick
      OnValidate = RBUnitate12Validate
      Height = 31
      Width = 330
    end
  end
end
