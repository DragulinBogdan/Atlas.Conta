object frmRepo: TfrmRepo
  Left = 313
  Top = 130
  ClientHeight = 505
  ClientWidth = 681
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  PixelsPerInch = 96
  TextHeight = 13
  object PageControl1: TPageControl
    Left = 0
    Top = 0
    Width = 681
    Height = 505
    ActivePage = TabSheet3
    Align = alClient
    TabOrder = 0
    object TabSheet2: TTabSheet
      Caption = 'Teste'
      ImageIndex = 1
      ExplicitLeft = 0
      ExplicitTop = 0
      ExplicitWidth = 0
      ExplicitHeight = 0
    end
    object TabSheet1: TTabSheet
      Caption = 'Plan Cont'
      ExplicitLeft = 0
      ExplicitTop = 0
      ExplicitWidth = 0
      ExplicitHeight = 0
      object TreePlan: TcxDBTreeList
        Tag = 1
        Left = 99
        Top = 18
        Width = 401
        Height = 150
        Bands = <
          item
            Caption.AlignHorz = taCenter
          end>
        DataController.DataSource = frmData.DTPlanCont
        DataController.ParentField = 'PARINTE'
        DataController.KeyField = 'CONT'
        LookAndFeel.Kind = lfOffice11
        Navigator.Buttons.CustomButtons = <>
        OptionsBehavior.GoToNextCellOnTab = True
        OptionsBehavior.ImmediateEditor = False
        OptionsBehavior.ExpandOnIncSearch = True
        OptionsBehavior.IncSearch = True
        OptionsBehavior.IncSearchItem = TreePlanDescriere
        OptionsData.Editing = False
        OptionsData.Deleting = False
        OptionsView.CellTextMaxLineCount = -1
        OptionsView.ColumnAutoWidth = True
        ParentColor = False
        Preview.AutoHeight = False
        Preview.MaxLineCount = 2
        RootValue = -1
        ScrollbarAnnotations.CustomAnnotations = <>
        TabOrder = 0
        Visible = False
        OnCustomDrawDataCell = TreePlanCustomDrawDataCell
        OnDblClick = TreePlanDblClick
        OnKeyDown = TreePlanKeyDown
        object TreePlanDescriere: TcxDBTreeListColumn
          Caption.Text = 'Plan Cont'
          Width = 273
          Position.ColIndex = 1
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
          OnGetDisplayText = TreePlanDescriereGetDisplayText
        end
        object TreePlanCONT: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'Cont'
          DataBinding.FieldName = 'CONT'
          Width = 100
          Position.ColIndex = 5
          Position.RowIndex = 0
          Position.BandIndex = 0
          SortOrder = soAscending
          SortIndex = 1
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreePlanROMANA: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'Plan Cont'
          DataBinding.FieldName = 'ROMANA'
          Width = 247
          Position.ColIndex = 0
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreePlanFCTCONT: TcxDBTreeListColumn
          Caption.Text = 'Funct.'
          DataBinding.FieldName = 'FCTCONT'
          Width = 34
          Position.ColIndex = 2
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreePlanSID: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'SID'
          Width = 45
          Position.ColIndex = 3
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreePlanSIC: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'SIC'
          Width = 47
          Position.ColIndex = 4
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
      end
    end
    object TabSheet3: TTabSheet
      Caption = 'Clasa Functionala'
      ImageIndex = 2
      ExplicitLeft = 0
      ExplicitTop = 0
      ExplicitWidth = 0
      ExplicitHeight = 0
      object cxTreeFunctional: TcxDBTreeList
        Tag = 1
        Left = 120
        Top = 32
        Width = 249
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
        TabOrder = 0
        OnDblClick = TreePlanDblClick
        OnKeyDown = TreePlanKeyDown
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
          Tag = -2
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
      object cxTreeEconomic: TcxDBTreeList
        Tag = 1
        Left = 135
        Top = 64
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
        TabOrder = 3
        OnDblClick = TreePlanDblClick
        OnKeyDown = TreePlanKeyDown
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
          Tag = -2
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
      object cxTreeProiecte: TcxDBTreeList
        Left = 151
        Top = 117
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
        TabOrder = 1
        OnDblClick = TreePlanDblClick
        OnKeyDown = TreePlanKeyDown
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
          Tag = -2
          Caption.AlignHorz = taCenter
          Caption.Text = 'Proiect'
          DataBinding.FieldName = 'DESCRIERE'
          Width = 100
          Position.ColIndex = 1
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
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
      object cxTreeUnitati: TcxDBTreeList
        Left = 183
        Top = 177
        Width = 250
        Height = 150
        Bands = <
          item
          end>
        DataController.DataSource = frmData.DTOIUnitati
        DataController.ParentField = 'id_parinte'
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
        TabOrder = 2
        OnDblClick = TreePlanDblClick
        OnKeyDown = TreePlanKeyDown
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
        end
        object cxTreeUnitatiDESCRIERE: TcxDBTreeListColumn
          Tag = -2
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
    end
    object TabSheet4: TTabSheet
      Caption = 'Repartitori'
      ImageIndex = 3
      ExplicitLeft = 0
      ExplicitTop = 0
      ExplicitWidth = 0
      ExplicitHeight = 0
      object TreeRepartitori: TcxDBTreeList
        Left = 3
        Top = 8
        Width = 486
        Height = 137
        Bands = <
          item
            Caption.AlignHorz = taCenter
          end>
        DataController.DataSource = frmData.DTRepartitori
        DataController.ParentField = 'ID_REPARTITORI'
        DataController.KeyField = 'ID_REPARTITORI'
        LookAndFeel.Kind = lfOffice11
        Navigator.Buttons.CustomButtons = <>
        OptionsBehavior.ImmediateEditor = False
        OptionsBehavior.DragCollapse = False
        OptionsBehavior.ExpandOnIncSearch = True
        OptionsBehavior.IncSearch = True
        OptionsBehavior.IncSearchItem = TreeRepartitoriNUME
        OptionsBehavior.ShowHourGlass = False
        OptionsCustomizing.BandCustomizing = False
        OptionsCustomizing.BandVertSizing = False
        OptionsCustomizing.ColumnsQuickCustomization = True
        OptionsCustomizing.ColumnVertSizing = False
        OptionsData.CancelOnExit = False
        OptionsData.Editing = False
        OptionsData.Deleting = False
        OptionsSelection.HideFocusRect = False
        OptionsView.CellTextMaxLineCount = -1
        OptionsView.ShowEditButtons = ecsbFocused
        OptionsView.ColumnAutoWidth = True
        OptionsView.Indicator = True
        ParentColor = False
        Preview.AutoHeight = False
        Preview.MaxLineCount = 2
        RootValue = -1
        ScrollbarAnnotations.CustomAnnotations = <>
        TabOrder = 0
        Visible = False
        OnCustomDrawDataCell = TreeRepartitoriCustomDrawDataCell
        OnDblClick = TreePlanDblClick
        OnKeyDown = TreePlanKeyDown
        object TreeRepartitoriNUME: TcxDBTreeListColumn
          Tag = -2
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Caption.Text = 'Denumire'
          DataBinding.FieldName = 'NUME'
          Width = 245
          Position.ColIndex = 0
          Position.RowIndex = 0
          Position.BandIndex = 0
          SortOrder = soAscending
          SortIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeRepartitoriADRESA: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Caption.Text = 'Adresa'
          DataBinding.FieldName = 'ADRESA'
          Width = 57
          Position.ColIndex = 1
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeRepartitoriCONT_CRSP: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          Caption.Text = 'Cont'
          DataBinding.FieldName = 'CONT_CRSP'
          Width = 100
          Position.ColIndex = 5
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeRepartitoriCODFISC: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Caption.Text = 'Cod Fiscal'
          DataBinding.FieldName = 'CODFISC'
          Width = 70
          Position.ColIndex = 2
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeRepartitoriGESTINT: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCheckBoxProperties'
          Properties.Alignment = taLeftJustify
          Properties.NullStyle = nssUnchecked
          Properties.ReadOnly = True
          Properties.ValueChecked = 'True'
          Properties.ValueGrayed = ''
          Properties.ValueUnchecked = 'False'
          Caption.Text = 'Interna'
          DataBinding.FieldName = 'GESTINT'
          Width = 42
          Position.ColIndex = 3
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeRepartitoriTIP_GESTIUNE: TcxDBTreeListColumn
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.DropDownRows = 7
          Properties.Items = <
            item
              Description = 'Magazin'
              ImageIndex = 0
              Value = '1'
            end
            item
              Description = 'Depozit'
              ImageIndex = 1
              Value = '2'
            end
            item
              Description = 'Custodie'
              ImageIndex = 2
              Value = '3'
            end
            item
              Description = 'Externa'
              ImageIndex = 3
              Value = '4'
            end
            item
              ImageIndex = 4
              Value = '5'
            end>
          Properties.ReadOnly = True
          Caption.Text = 'Tip'
          DataBinding.FieldName = 'TIP_GESTIUNE'
          Width = 58
          Position.ColIndex = 4
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
      end
      object cxGrid2: TcxGrid
        Left = 0
        Top = 287
        Width = 673
        Height = 190
        Align = alBottom
        Anchors = [akLeft, akTop, akRight, akBottom]
        TabOrder = 1
        object cxGrid2DBTableView1: TcxGridDBTableView
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          DataController.Filter.Options = [fcoCaseInsensitive]
          DataController.KeyFieldNames = 'ID_REPARTITORI'
          DataController.Summary.DefaultGroupSummaryItems = <>
          DataController.Summary.FooterSummaryItems = <>
          DataController.Summary.SummaryGroups = <>
          FilterRow.InfoText = 'Definire filtru'
          FilterRow.Visible = True
          OptionsData.Editing = False
          OptionsView.ColumnAutoWidth = True
          object cxGrid2DBTableView1ID_REPARTITORI: TcxGridDBColumn
            DataBinding.FieldName = 'ID_REPARTITORI'
            Width = 58
          end
          object cxGrid2DBTableView1CODSECTIE: TcxGridDBColumn
            DataBinding.FieldName = 'CODSECTIE'
            Width = 99
          end
          object cxGrid2DBTableView1NUME: TcxGridDBColumn
            DataBinding.FieldName = 'NUME'
            Width = 281
          end
          object cxGrid2DBTableView1ADRESA: TcxGridDBColumn
            DataBinding.FieldName = 'ADRESA'
            Width = 139
          end
          object cxGrid2DBTableView1CONT: TcxGridDBColumn
            DataBinding.FieldName = 'CONT'
            Width = 53
          end
          object cxGrid2DBTableView1CONT_CEC: TcxGridDBColumn
            DataBinding.FieldName = 'CONT_CEC'
            Width = 55
          end
          object cxGrid2DBTableView1BANCA: TcxGridDBColumn
            DataBinding.FieldName = 'BANCA'
            Width = 63
          end
          object cxGrid2DBTableView1CODCLASM: TcxGridDBColumn
            DataBinding.FieldName = 'CODCLASM'
            Width = 28
          end
          object cxGrid2DBTableView1COD_FISCAL: TcxGridDBColumn
            DataBinding.FieldName = 'COD_FISCAL'
            Width = 58
          end
        end
        object cxGrid2Level1: TcxGridLevel
          GridView = cxGrid2DBTableView1
        end
      end
    end
    object TabSheet5: TTabSheet
      Caption = 'Valuta'
      ImageIndex = 4
      ExplicitLeft = 0
      ExplicitTop = 0
      ExplicitWidth = 0
      ExplicitHeight = 0
    end
    object TabSheet6: TTabSheet
      Caption = 'TipuriMateriale'
      ImageIndex = 5
      ExplicitLeft = 0
      ExplicitTop = 0
      ExplicitWidth = 0
      ExplicitHeight = 0
      object TreeTipMaterial: TcxDBTreeList
        Left = 120
        Top = 32
        Width = 289
        Height = 393
        Bands = <
          item
          end>
        DataController.ParentField = 'ID_PARINTE'
        DataController.KeyField = 'ID_GEST_TIP_MATERIAL'
        LookAndFeel.Kind = lfOffice11
        Navigator.Buttons.CustomButtons = <>
        OptionsBehavior.CopyCaptionsToClipboard = False
        OptionsData.Editing = False
        OptionsView.ColumnAutoWidth = True
        OptionsView.Headers = False
        RootValue = -1
        ScrollbarAnnotations.CustomAnnotations = <>
        TabOrder = 0
        OnDblClick = TreePlanDblClick
        OnKeyDown = TreePlanKeyDown
        object TreeTipMaterialDESCRIERE: TcxDBTreeListColumn
          Tag = -2
          Caption.Text = 'Descriere'
          DataBinding.FieldName = 'DESCRIERE'
          Width = 100
          Position.ColIndex = 0
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeTipMaterialID_GEST_TIP_MATERIAL: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'ID_GEST_TIP_MATERIAL'
          Width = 100
          Position.ColIndex = 1
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeTipMaterialID_PARINTE: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'ID_PARINTE'
          Width = 100
          Position.ColIndex = 2
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
      end
    end
    object TabSheet7: TTabSheet
      Caption = 'CBT structuri'
      ImageIndex = 6
      ExplicitLeft = 0
      ExplicitTop = 0
      ExplicitWidth = 0
      ExplicitHeight = 0
      object cxTreeStructura: TcxDBTreeList
        Left = 157
        Top = 80
        Width = 513
        Height = 177
        Bands = <
          item
            Caption.AlignHorz = taCenter
          end>
        DataController.DataSource = DTStructure
        DataController.ParentField = 'COD_PARINTE'
        DataController.KeyField = 'COD_CB'
        LookAndFeel.Kind = lfOffice11
        Navigator.Buttons.CustomButtons = <>
        OptionsBehavior.GoToNextCellOnTab = True
        OptionsBehavior.ImmediateEditor = False
        OptionsBehavior.ConfirmDelete = False
        OptionsBehavior.ExpandOnIncSearch = True
        OptionsBehavior.IncSearch = True
        OptionsBehavior.ShowHourGlass = False
        OptionsCustomizing.BandCustomizing = False
        OptionsCustomizing.BandVertSizing = False
        OptionsData.Editing = False
        OptionsData.Deleting = False
        OptionsView.CellTextMaxLineCount = -1
        OptionsView.ShowEditButtons = ecsbFocused
        OptionsView.BandLineHeight = 19
        OptionsView.ColumnAutoWidth = True
        Preview.AutoHeight = False
        Preview.MaxLineCount = 2
        RootValue = -1
        ScrollbarAnnotations.CustomAnnotations = <>
        TabOrder = 0
        OnDblClick = TreePlanDblClick
        object cxTreeStructuraCOD_CB: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          DataBinding.FieldName = 'COD_CB'
          Width = 100
          Position.ColIndex = 15
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeStructuraCOD_PARINTE: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          DataBinding.FieldName = 'COD_PARINTE'
          Width = 100
          Position.ColIndex = 16
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeStructuraDENUMIRE: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Caption.Text = 'Denumire Casa'
          DataBinding.FieldName = 'DENUMIRE'
          Width = 349
          Position.ColIndex = 0
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeStructuraDENV: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          DataBinding.FieldName = 'DENV'
          Width = 100
          Position.ColIndex = 14
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeStructuraC_O: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          DataBinding.FieldName = 'C_O'
          Width = 100
          Position.ColIndex = 13
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeStructuraDATA_SOLD: TcxDBTreeListColumn
          PropertiesClassName = 'TcxDateEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.DateButtons = [btnClear, btnToday]
          Properties.DateOnError = deToday
          Properties.InputKind = ikRegExpr
          Visible = False
          DataBinding.FieldName = 'DATA_SOLD'
          Width = 100
          Position.ColIndex = 12
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeStructuraCASIER: TcxDBTreeListColumn
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.DropDownRows = 7
          Properties.Items = <
            item
              ImageIndex = 0
              Value = 'False'
            end
            item
              Description = 'Casier'
              ImageIndex = 1
              Value = 'True'
            end>
          Properties.ReadOnly = True
          Visible = False
          Caption.Text = 'Este Casier'
          DataBinding.FieldName = 'CASIER'
          Width = 100
          Position.ColIndex = 11
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeStructuraVALIDATOR: TcxDBTreeListColumn
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.DropDownRows = 7
          Properties.Items = <
            item
              ImageIndex = 0
              Value = 'False'
            end
            item
              Description = 'Contabil'
              ImageIndex = 1
              Value = 'True'
            end>
          Properties.ReadOnly = True
          Visible = False
          Caption.Text = 'Este Contabil'
          DataBinding.FieldName = 'VALIDATOR'
          Width = 100
          Position.ColIndex = 10
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeStructuraADMIN: TcxDBTreeListColumn
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.DropDownRows = 7
          Properties.Items = <
            item
              ImageIndex = 0
              Value = 'False'
            end
            item
              Description = 'Administrator'
              ImageIndex = 1
              Value = 'True'
            end>
          Properties.ReadOnly = True
          Visible = False
          Caption.Text = 'Este Administrator'
          DataBinding.FieldName = 'ADMIN'
          Width = 100
          Position.ColIndex = 9
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeStructuraIS_BANCA: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCheckBoxProperties'
          Properties.Alignment = taLeftJustify
          Properties.NullStyle = nssUnchecked
          Properties.ReadOnly = True
          Properties.ValueChecked = 'True'
          Properties.ValueGrayed = ''
          Properties.ValueUnchecked = 'False'
          Visible = False
          DataBinding.FieldName = 'IS_BANCA'
          Width = 100
          Position.ColIndex = 8
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeStructuraIS_AVANS: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCheckBoxProperties'
          Properties.Alignment = taLeftJustify
          Properties.NullStyle = nssUnchecked
          Properties.ReadOnly = True
          Properties.ValueChecked = 'True'
          Properties.ValueGrayed = ''
          Properties.ValueUnchecked = 'False'
          Visible = False
          DataBinding.FieldName = 'IS_AVANS'
          Width = 100
          Position.ColIndex = 7
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeStructuraIS_TEMPOR: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCheckBoxProperties'
          Properties.Alignment = taLeftJustify
          Properties.NullStyle = nssUnchecked
          Properties.ReadOnly = True
          Properties.ValueChecked = 'True'
          Properties.ValueGrayed = ''
          Properties.ValueUnchecked = 'False'
          Visible = False
          DataBinding.FieldName = 'IS_TEMPOR'
          Width = 100
          Position.ColIndex = 6
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeStructuraID_REPARTITORI: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          DataBinding.FieldName = 'ID_REPARTITORI'
          Width = 100
          Position.ColIndex = 5
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeStructuraICON: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          DataBinding.FieldName = 'ICON'
          Width = 100
          Position.ColIndex = 4
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeStructuraID_VALUTA: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          DataBinding.FieldName = 'ID_VALUTA'
          Width = 100
          Position.ColIndex = 3
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeStructuraDESCRIERE: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Visible = False
          Caption.Text = 'Descriere'
          DataBinding.FieldName = 'DESCRIERE'
          Width = 100
          Position.ColIndex = 2
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeStructuraCRSP_LEI: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.MaxLength = 0
          Properties.ReadOnly = True
          Caption.Text = 'Cont Contabil'
          DataBinding.FieldName = 'CRSP_LEI'
          Width = 162
          Position.ColIndex = 1
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
      end
    end
  end
  object EditRepo: TcxEditRepository
    Left = 24
    Top = 72
    PixelsPerInch = 96
    object EditRepoCont: TcxEditRepositoryPopupItem
      Properties.Alignment.Horz = taLeftJustify
      Properties.HideSelection = False
      Properties.PopupControl = TreePlan
      Properties.PopupSysPanelStyle = True
      Properties.OnCloseUp = EditRepoContPropertiesCloseUp
      Properties.OnPopup = EditRepoContPropertiesPopup
    end
    object EditRepoRepartitor: TcxEditRepositoryPopupItem
      Properties.Alignment.Horz = taLeftJustify
      Properties.PopupControl = TreeRepartitori
      Properties.PopupSysPanelStyle = True
      Properties.OnCloseUp = EditRepoContPropertiesCloseUp
      Properties.OnPopup = EditRepoContPropertiesPopup
    end
    object EditRepoTipValuta: TcxEditRepositoryImageComboBoxItem
      Properties.Items = <>
      Properties.OnInitPopup = EditRepoTipValutaPropertiesInitPopup
    end
    object EditRepoUtilizator: TcxEditRepositoryImageComboBoxItem
      Properties.Items = <>
      Properties.OnInitPopup = EditRepoUtilizatorPropertiesInitPopup
    end
    object EditRepoLookupRepartitor: TcxEditRepositoryLookupComboBoxItem
      Properties.DropDownSizeable = True
      Properties.DropDownWidth = 300
      Properties.ImmediatePost = True
      Properties.KeyFieldNames = 'ID_REPARTITORI'
      Properties.ListColumns = <
        item
          FieldName = 'NUME'
        end>
      Properties.ListOptions.AnsiSort = True
      Properties.ListOptions.CaseInsensitive = True
      Properties.ListSource = frmData.DTRepartitori
    end
    object EditRepoLookupVama: TcxEditRepositoryLookupComboBoxItem
      Properties.DropDownAutoSize = True
      Properties.ImmediatePost = True
      Properties.KeyFieldNames = 'ID_IE_VAMI'
      Properties.ListColumns = <
        item
          Caption = 'Cod'
          MinWidth = 10
          Width = 10
          FieldName = 'COD'
        end
        item
          Caption = 'Denumire'
          FieldName = 'DENUMIRE'
        end>
      Properties.ListFieldIndex = 1
    end
    object EditRepoTipMaterial: TcxEditRepositoryDisplayPopupItem
      Properties.Alignment.Horz = taLeftJustify
      Properties.PopupControl = TreeTipMaterial
      Properties.PopupSysPanelStyle = True
      Properties.OnCloseUp = EditRepoContPropertiesCloseUp
      Properties.OnPopup = EditRepoContPropertiesPopup
      Properties.OnCustomDisplayValue = EditRepoTipMaterialPropertiesCustomDisplayValue
    end
    object EditRepoCBTStructura: TcxEditRepositoryPopupItem
      Properties.HideSelection = False
      Properties.PopupControl = cxTreeStructura
      Properties.PopupSysPanelStyle = True
      Properties.OnCloseUp = EditRepoContPropertiesCloseUp
      Properties.OnPopup = EditRepoContPropertiesPopup
    end
    object EditRepoTreeFunctional: TcxEditRepositoryPopupItem
      Properties.Alignment.Horz = taLeftJustify
      Properties.HideSelection = False
      Properties.PopupControl = cxTreeFunctional
      Properties.PopupSysPanelStyle = True
      Properties.OnCloseUp = EditRepoContPropertiesCloseUp
      Properties.OnPopup = EditRepoContPropertiesPopup
    end
    object EditRepoTreeEconomic: TcxEditRepositoryPopupItem
      Properties.Alignment.Horz = taLeftJustify
      Properties.HideSelection = False
      Properties.PopupControl = cxTreeEconomic
      Properties.PopupSysPanelStyle = True
      Properties.OnCloseUp = EditRepoContPropertiesCloseUp
      Properties.OnPopup = EditRepoContPropertiesPopup
    end
    object EditRepoTreeProiecte: TcxEditRepositoryDisplayPopupItem
      Properties.Alignment.Horz = taLeftJustify
      Properties.HideSelection = False
      Properties.PopupControl = cxTreeProiecte
      Properties.PopupSysPanelStyle = True
      Properties.OnCloseUp = EditRepoContPropertiesCloseUp
      Properties.OnPopup = EditRepoContPropertiesPopup
      Properties.OnCustomDisplayValue = EditRepoTipMaterialPropertiesCustomDisplayValue
    end
    object EditRepoTreeUnitati: TcxEditRepositoryDisplayPopupItem
      Properties.Alignment.Horz = taLeftJustify
      Properties.HideSelection = False
      Properties.PopupControl = cxTreeUnitati
      Properties.PopupSysPanelStyle = True
      Properties.OnCloseUp = EditRepoContPropertiesCloseUp
      Properties.OnPopup = EditRepoContPropertiesPopup
      Properties.OnCustomDisplayValue = EditRepoTipMaterialPropertiesCustomDisplayValue
    end
    object EditRepoLookupProiecte: TcxEditRepositoryLookupComboBoxItem
      Properties.DropDownSizeable = True
      Properties.DropDownWidth = 300
      Properties.ImmediatePost = True
      Properties.KeyFieldNames = 'id_oi_proiecte'
      Properties.ListColumns = <
        item
          FieldName = 'Denumire'
        end>
      Properties.ListOptions.AnsiSort = True
      Properties.ListOptions.CaseInsensitive = True
      Properties.ListSource = frmData.DTOIProiecte
    end
    object EditRepoLookupUnitati: TcxEditRepositoryLookupComboBoxItem
      Properties.DropDownSizeable = True
      Properties.DropDownWidth = 300
      Properties.ImmediatePost = True
      Properties.KeyFieldNames = 'id_oi_unitati'
      Properties.ListColumns = <
        item
          FieldName = 'Denumire'
        end>
      Properties.ListOptions.AnsiSort = True
      Properties.ListOptions.CaseInsensitive = True
      Properties.ListSource = frmData.DTOIUnitati
    end
  end
  object dxMemData1: TdxMemData
    Indexes = <>
    SortOptions = []
    Left = 72
    Top = 112
    object dxMemData1CONT: TStringField
      FieldName = 'CONT'
      Size = 255
    end
  end
  object DTStructure: TDataSource
    DataSet = QryStructure
    Left = 552
    Top = 144
  end
  object QryStructure: TZQuery
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
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'IS_ADMIN'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'DISP_WAY'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 584
    Top = 144
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'COD_UTILIZATOR'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'IS_ADMIN'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'DISP_WAY'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object ImagesStructura: TImageList
    Left = 616
    Top = 144
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
end
