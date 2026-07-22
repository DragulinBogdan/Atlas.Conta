object frmPreluareExcel: TfrmPreluareExcel
  Left = 427
  Top = 153
  Caption = 'Preluare planificari'
  ClientHeight = 472
  ClientWidth = 636
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  OnCreate = FormCreate
  DesignSize = (
    636
    472)
  PixelsPerInch = 96
  TextHeight = 13
  object GridPreluare: TcxGrid
    Left = 0
    Top = 96
    Width = 636
    Height = 329
    Align = alTop
    Anchors = [akLeft, akTop, akRight, akBottom]
    TabOrder = 6
    LookAndFeel.Kind = lfOffice11
    object GridPreluareV: TcxGridDBTableView
      Navigator.Buttons.CustomButtons = <>
      DataController.DataSource = DTGenerate
      DataController.Filter.MaxValueListCount = 1000
      DataController.Filter.Active = True
      DataController.KeyFieldNames = 'RecId'
      DataController.Summary.DefaultGroupSummaryItems = <>
      DataController.Summary.FooterSummaryItems = <>
      DataController.Summary.SummaryGroups = <>
      Filtering.ColumnPopup.MaxDropDownItemCount = 12
      OptionsSelection.HideFocusRectOnExit = False
      OptionsSelection.InvertSelect = False
      OptionsView.ColumnAutoWidth = True
      OptionsView.GroupByBox = False
      OptionsView.GroupFooters = gfVisibleWhenExpanded
      Preview.AutoHeight = False
      Preview.MaxLineCount = 2
      object GridPreluareVEROARE: TcxGridDBColumn
        Caption = 'Eroare'
        DataBinding.FieldName = 'EROARE'
        PropertiesClassName = 'TcxImageComboBoxProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.DropDownRows = 7
        Properties.Items = <
          item
            Description = 'Preluat OK'
            ImageIndex = 0
            Value = '0'
          end
          item
            Description = 'Cod Fara Punct'
            ImageIndex = 1
            Value = '1'
          end
          item
            Description = 'Cod Partial'
            ImageIndex = 2
            Value = '2'
          end
          item
            Description = 'Eroare - Cod negasit'
            ImageIndex = 3
            Value = '3'
          end
          item
            Description = 'Eroare - Este parinte'
            ImageIndex = 4
            Value = '4'
          end
          item
            Description = 'Gasit - Cod Partial'
            ImageIndex = 5
            Value = '5'
          end
          item
            Description = 'Corectat - se preia'
            Value = '6'
          end
          item
            Description = 'Corectat - nu se preia'
            Value = '7'
          end
          item
            Description = 'Buget - Parinte Estimat se preia'
            Value = '8'
          end>
        HeaderAlignmentHorz = taCenter
        MinWidth = 16
        Width = 46
      end
      object GridPreluareVCOD_FUNCTIONAL: TcxGridDBColumn
        Caption = 'Cls. Func.'
        DataBinding.FieldName = 'COD_FUNCTIONAL'
        PropertiesClassName = 'TcxPopupEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 128
        Properties.PopupAutoSize = False
        Properties.PopupControl = cxTreeFunctional
        Properties.PopupHeight = 240
        Properties.PopupSysPanelStyle = True
        Properties.PopupWidth = 320
        Properties.ReadOnly = True
        Properties.OnCloseQuery = GridPreluareVCOD_FUNCTIONALPropertiesCloseQuery
        Properties.OnPopup = GridPreluareVCOD_FUNCTIONALPropertiesPopup
        Visible = False
        HeaderAlignmentHorz = taCenter
        Width = 67
      end
      object GridPreluareVCOD_ECONOMIC_ECRAN: TcxGridDBColumn
        Caption = 'Cls. Ec. Excel.'
        DataBinding.FieldName = 'COD_ECONOMIC_ECRAN'
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 128
        Properties.ReadOnly = True
        Options.Editing = False
        Width = 74
      end
      object GridPreluareVCOD_ECONOMIC: TcxGridDBColumn
        Caption = 'Cls. Ec.'
        DataBinding.FieldName = 'COD_ECONOMIC'
        PropertiesClassName = 'TcxPopupEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 128
        Properties.PopupAutoSize = False
        Properties.PopupControl = cxTreeEconomic
        Properties.PopupHeight = 240
        Properties.PopupSysPanelStyle = True
        Properties.PopupWidth = 320
        Properties.ReadOnly = True
        Properties.OnCloseQuery = GridPreluareVCOD_ECONOMICPropertiesCloseQuery
        Properties.OnPopup = GridPreluareVCOD_ECONOMICPropertiesPopup
        HeaderAlignmentHorz = taCenter
        Width = 74
      end
      object GridPreluareVPLANIFICAT_REST: TcxGridDBColumn
        Caption = 'Restante'
        DataBinding.FieldName = 'PLANIFICAT_REST'
        PropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.Alignment.Horz = taRightJustify
        Properties.DecimalPlaces = 2
        Properties.DisplayFormat = ',0.00;-,0.00'
        Properties.Nullable = False
        Properties.ReadOnly = True
        HeaderAlignmentHorz = taCenter
        Width = 49
      end
      object GridPreluareVPLANIFICAT1: TcxGridDBColumn
        Caption = 'Trim. 1'
        DataBinding.FieldName = 'PLANIFICAT1'
        PropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.Alignment.Horz = taRightJustify
        Properties.AssignedValues.MaxValue = True
        Properties.AssignedValues.MinValue = True
        Properties.DecimalPlaces = 2
        Properties.DisplayFormat = ',0.00;-,0.00'
        Properties.Nullable = False
        Properties.ReadOnly = True
        HeaderAlignmentHorz = taCenter
        Width = 65
      end
      object GridPreluareVPLANIFICAT2: TcxGridDBColumn
        Caption = 'Trim. 2'
        DataBinding.FieldName = 'PLANIFICAT2'
        PropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.Alignment.Horz = taRightJustify
        Properties.AssignedValues.MaxValue = True
        Properties.AssignedValues.MinValue = True
        Properties.DecimalPlaces = 2
        Properties.DisplayFormat = ',0.00;-,0.00'
        Properties.Nullable = False
        Properties.ReadOnly = True
        HeaderAlignmentHorz = taCenter
        Width = 61
      end
      object GridPreluareVPLANIFICAT3: TcxGridDBColumn
        Caption = 'Trim. 3'
        DataBinding.FieldName = 'PLANIFICAT3'
        PropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.Alignment.Horz = taRightJustify
        Properties.AssignedValues.MaxValue = True
        Properties.AssignedValues.MinValue = True
        Properties.DecimalPlaces = 2
        Properties.DisplayFormat = ',0.00;-,0.00'
        Properties.Nullable = False
        Properties.ReadOnly = True
        HeaderAlignmentHorz = taCenter
        Width = 56
      end
      object GridPreluareVPLANIFICAT4: TcxGridDBColumn
        Caption = 'Trim. 4'
        DataBinding.FieldName = 'PLANIFICAT4'
        PropertiesClassName = 'TcxCurrencyEditProperties'
        Properties.Alignment.Horz = taRightJustify
        Properties.AssignedValues.MaxValue = True
        Properties.AssignedValues.MinValue = True
        Properties.DecimalPlaces = 2
        Properties.DisplayFormat = ',0.00;-,0.00'
        Properties.Nullable = False
        Properties.ReadOnly = True
        HeaderAlignmentHorz = taCenter
        Width = 56
      end
      object GridPreluareVPLUS1AN: TcxGridDBColumn
        Caption = 'Estimat 1 AN'
        DataBinding.FieldName = 'PLUS1AN'
        Width = 49
      end
      object GridPreluareVPLUS2AN: TcxGridDBColumn
        Caption = 'Estimat 2 AN'
        DataBinding.FieldName = 'PLUS2AN'
        Width = 48
      end
      object GridPreluareVPLUS3AN: TcxGridDBColumn
        Caption = 'Estimat 3 An'
        DataBinding.FieldName = 'PLUS3AN'
        Width = 48
      end
    end
    object GridPreluareL: TcxGridLevel
      GridView = GridPreluareV
    end
  end
  object cxTreeEconomic: TcxDBTreeList
    Left = 60
    Top = 230
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
    OptionsData.Editing = False
    OptionsData.Deleting = False
    OptionsView.ColumnAutoWidth = True
    RootValue = -1
    TabOrder = 7
    Visible = False
    OnDblClick = cxTreeEconomicDblClick
    OnKeyDown = cxTreeEconomicKeyDown
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
      DataBinding.FieldName = 'COD_ECONOMIC'
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
  object cxTreeFunctional: TcxDBTreeList
    Left = 235
    Top = 177
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
    OptionsData.Editing = False
    OptionsData.Deleting = False
    OptionsView.ColumnAutoWidth = True
    RootValue = -1
    TabOrder = 5
    Visible = False
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
  object cxTreeProiecte: TcxDBTreeList
    Left = 59
    Top = 121
    Width = 250
    Height = 150
    Bands = <
      item
      end>
    DataController.DataSource = frmData.DTOIProiecte
    DataController.ParentField = 'ID_PARINTE'
    DataController.KeyField = 'ID_OI_PROIECTE'
    LookAndFeel.Kind = lfOffice11
    Navigator.Buttons.CustomButtons = <>
    OptionsBehavior.ExpandOnIncSearch = True
    OptionsBehavior.IncSearch = True
    OptionsBehavior.IncSearchItem = cxTreeProiecteDESCRIERE
    OptionsData.Editing = False
    OptionsData.Deleting = False
    OptionsView.ColumnAutoWidth = True
    RootValue = -1
    TabOrder = 4
    Visible = False
    object cxTreeProiecteID_OI_PROIECTE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'ID_OI_PROIECTE'
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
      DataBinding.FieldName = 'ID_PARINTE'
      Width = 100
      Position.ColIndex = 2
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
    object cxTreeProiecteDENUMIRE: TcxDBTreeListColumn
      Visible = False
      DataBinding.FieldName = 'DENUMIRE'
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
      DataBinding.FieldName = 'cod_functional'
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
      DataBinding.FieldName = 'COD_FUNCTIONAL'
      Width = 100
      Position.ColIndex = 5
      Position.RowIndex = 0
      Position.BandIndex = 0
      Summary.FooterSummaryItems = <>
      Summary.GroupFooterSummaryItems = <>
    end
  end
  object cxTreeUnitati: TcxDBTreeList
    Left = 372
    Top = 129
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
    OptionsData.Editing = False
    OptionsData.Deleting = False
    OptionsView.ColumnAutoWidth = True
    RootValue = -1
    TabOrder = 3
    Visible = False
    OnDblClick = cxTreeUnitatiDblClick
    OnKeyDown = cxTreeUnitatiKeyDown
    object cxTreeUnitatiID_OI_UNITATI: TcxDBTreeListColumn
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
  object pnTop: TPanel
    Left = 0
    Top = 0
    Width = 636
    Height = 96
    Align = alTop
    TabOrder = 0
    DesignSize = (
      636
      96)
    object LbAnFiscal: TLabel
      Left = 14
      Top = 9
      Width = 52
      Height = 13
      Caption = 'An &Fiscal : '
      FocusControl = edAnFiscal
    end
    object LbRevizie: TLabel
      Left = 198
      Top = 9
      Width = 44
      Height = 13
      Caption = '&Revizie : '
      FocusControl = edRevizie
    end
    object LbMultiplicator: TLabel
      Left = 382
      Top = 9
      Width = 65
      Height = 13
      Caption = '&Multiplicator : '
      FocusControl = edMultiplicator
    end
    object cxLabel1: TcxLabel
      Left = 8
      Top = 37
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
      Top = 26
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
      PopupEdit.Width = 326
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
      OnPopupInitPopup = RPFunctional1PopupInitPopup
      Height = 31
      Width = 446
    end
    object RPProiect: TcxRepartitorPanel
      Tag = -1
      Left = 235
      Top = 57
      Anchors = [akLeft, akTop, akRight]
      TabOrder = 5
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
      PopupEdit.Width = 254
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
      OnPopupInitPopup = RPFunctional1PopupInitPopup
      Height = 31
      Width = 374
    end
    object lbDefalcare: TcxLabel
      Left = 148
      Top = 66
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
      Width = 81
      AnchorX = 229
    end
    object edCategorie: TcxImageComboBox
      Left = 8
      Top = 64
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
      TabOrder = 7
      Width = 145
    end
    object edAnFiscal: TcxImageComboBox
      Left = 68
      Top = 5
      Properties.Items = <>
      TabOrder = 0
      Width = 121
    end
    object edMultiplicator: TcxImageComboBox
      Left = 452
      Top = 5
      EditValue = '1000'
      Properties.Items = <
        item
          Description = 'Leu'
          ImageIndex = 0
          Value = 1
        end
        item
          Description = 'Zeci de lei'
          ImageIndex = 0
          Value = 10
        end
        item
          Description = 'Sute de lei'
          ImageIndex = 0
          Value = 100
        end
        item
          Description = 'Mii lei'
          ImageIndex = 0
          Value = 1000
        end
        item
          Description = 'Zeci de mii'
          ImageIndex = 0
          Value = 10000
        end
        item
          Description = 'Sute de mii'
          ImageIndex = 0
          Value = 100000
        end
        item
          Description = 'Milioane'
          ImageIndex = 0
          Value = 1000000
        end>
      TabOrder = 2
      Width = 121
    end
    object edRevizie: TcxImageComboBox
      Left = 252
      Top = 5
      Properties.Items = <>
      TabOrder = 1
      Width = 121
    end
  end
  object BtnPreia: TcxButton
    Left = 456
    Top = 432
    Width = 89
    Height = 25
    Anchors = [akRight, akBottom]
    Caption = 'Preia Planificare'
    LookAndFeel.Kind = lfOffice11
    TabOrder = 1
    OnClick = BtnPreiaClick
  end
  object BtnCancel: TcxButton
    Left = 552
    Top = 432
    Width = 73
    Height = 25
    Anchors = [akRight, akBottom]
    Caption = 'Abandon'
    LookAndFeel.Kind = lfOffice11
    TabOrder = 2
    OnClick = BtnCancelClick
  end
  object PreluareProgress: TcxProgressBar
    Left = 8
    Top = 434
    Anchors = [akLeft, akRight, akBottom]
    Properties.PeakValue = 20.000000000000000000
    TabOrder = 8
    Visible = False
    Width = 441
  end
  object DTGenerate: TDataSource
    DataSet = Preluate
    Left = 8
    Top = 128
  end
  object Preluate: TdxMemData
    Indexes = <>
    SortOptions = []
    Left = 40
    Top = 128
    object PreluateCOD_FUNCTIONAL: TStringField
      FieldName = 'COD_FUNCTIONAL'
      Size = 128
    end
    object PreluateCOD_ECONOMIC_ECRAN: TStringField
      FieldName = 'COD_ECONOMIC_ECRAN'
      Size = 128
    end
    object PreluateCOD_ECONOMIC: TStringField
      FieldName = 'COD_ECONOMIC'
      Size = 128
    end
    object PreluatePLANIFICAT_REST: TCurrencyField
      FieldName = 'PLANIFICAT_REST'
    end
    object PreluatePLANIFICAT1: TCurrencyField
      FieldName = 'PLANIFICAT1'
    end
    object PreluatePLANIFICAT2: TCurrencyField
      FieldName = 'PLANIFICAT2'
    end
    object PreluatePLANIFICAT3: TCurrencyField
      FieldName = 'PLANIFICAT3'
    end
    object PreluatePLANIFICAT4: TCurrencyField
      FieldName = 'PLANIFICAT4'
    end
    object PreluateEROARE: TIntegerField
      FieldName = 'EROARE'
    end
    object PreluatePLUS1AN: TCurrencyField
      FieldName = 'PLUS1AN'
    end
    object PreluatePLUS2AN: TCurrencyField
      FieldName = 'PLUS2AN'
    end
    object PreluatePLUS3AN: TCurrencyField
      FieldName = 'PLUS3AN'
    end
  end
  object cxGridPopupMenu: TcxGridPopupMenu
    Grid = GridPreluare
    PopupMenus = <>
    Left = 560
    Top = 288
  end
end
