object frmBGPlan: TfrmBGPlan
  Left = 564
  Top = 128
  Caption = 'Intretinere Plan De Bugete'
  ClientHeight = 650
  ClientWidth = 1042
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  Scaled = False
  OnClose = FormClose
  OnCloseQuery = FormCloseQuery
  OnCreate = FormCreate
  DesignSize = (
    1042
    650)
  PixelsPerInch = 96
  TextHeight = 13
  object PaginaClasificatii: TcxPageControl
    Left = 0
    Top = 0
    Width = 1042
    Height = 574
    Align = alTop
    Anchors = [akLeft, akTop, akRight, akBottom]
    TabOrder = 2
    Properties.ActivePage = cxTabEconomic
    Properties.CustomButtons.Buttons = <>
    Properties.Style = 9
    Properties.TabSlants.Positions = [spLeft, spRight]
    LookAndFeel.Kind = lfOffice11
    ClientRectBottom = 574
    ClientRectRight = 1042
    ClientRectTop = 20
    object cxTabFunctional: TcxTabSheet
      Caption = 'Clasificatie Functionala'
      ImageIndex = 2
      object pnFunctDetalii: TPanel
        Left = 694
        Top = 0
        Width = 348
        Height = 554
        Align = alRight
        BevelOuter = bvNone
        TabOrder = 0
        object cxFunctParams: TcxDBVerticalGrid
          Left = 0
          Top = 0
          Width = 348
          Height = 554
          BorderStyle = cxcbsNone
          Align = alClient
          LookAndFeel.Kind = lfOffice11
          OptionsView.CellTextMaxLineCount = 3
          OptionsView.ScrollBars = ssVertical
          OptionsView.AutoScaleBands = False
          OptionsView.GridLineColor = clBtnShadow
          OptionsView.RowHeaderMinWidth = 30
          OptionsView.RowHeaderWidth = 184
          OptionsView.RowHeight = 20
          OptionsView.ValueWidth = 62
          OptionsData.CancelOnExit = False
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          Styles.StyleSheet = frmData.cxVerticalGridStyleSheetHighContrastWhite
          TabOrder = 0
          DataController.DataSource = frmData.DTBGPlanFunctional
          DataController.GridMode = True
          Version = 1
          object cxFunctParamsDESCRIERE: TcxDBEditorRow
            Expanded = False
            Height = 17
            Properties.Caption = 'Descriere'
            Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.MaxLength = 0
            Properties.EditProperties.ReadOnly = False
            Properties.DataBinding.FieldName = 'DESCRIERE'
            Visible = False
            ID = 0
            ParentID = -1
            Index = 0
            Version = 1
          end
          object cxFunctParamsCategoryRow1: TcxCategoryRow
            Properties.Caption = 'Detalii Capitol/Subcapitol/Paragraf'
            ID = 1
            ParentID = -1
            Index = 1
            Version = 1
          end
          object cxFunctParamsCOD_BUGET: TcxDBEditorRow
            Expanded = False
            Height = 17
            Properties.Caption = 'Capitol'
            Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.MaxLength = 0
            Properties.EditProperties.ReadOnly = False
            Properties.DataBinding.FieldName = 'COD_FUNCTIONAL'
            ID = 2
            ParentID = 1
            Index = 0
            Version = 1
          end
          object cxFunctParamsDENUMIRE: TcxDBEditorRow
            Expanded = False
            Height = 73
            Properties.Caption = 'Denumire'
            Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.MaxLength = 0
            Properties.EditProperties.ReadOnly = False
            Properties.DataBinding.FieldName = 'DENUMIRE'
            ID = 3
            ParentID = 1
            Index = 1
            Version = 1
          end
          object cxFunctParamsCAPITOL: TcxDBEditorRow
            Expanded = False
            Height = 17
            Properties.Caption = 'Capitol Parinte'
            Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.MaxLength = 0
            Properties.EditProperties.ReadOnly = False
            Properties.DataBinding.FieldName = 'CAPITOL'
            ID = 4
            ParentID = 1
            Index = 2
            Version = 1
          end
          object cxFunctParamsESTE_LUCRARE: TcxDBEditorRow
            Height = 17
            Properties.Caption = 'Lucrare'
            Properties.EditPropertiesClassName = 'TcxCheckBoxProperties'
            Properties.EditProperties.Alignment = taLeftJustify
            Properties.EditProperties.NullStyle = nssUnchecked
            Properties.EditProperties.ReadOnly = False
            Properties.EditProperties.ValueChecked = 'True'
            Properties.EditProperties.ValueGrayed = ''
            Properties.EditProperties.ValueUnchecked = 'False'
            Properties.DataBinding.FieldName = 'ESTE_LUCRARE'
            ID = 5
            ParentID = 1
            Index = 3
            Version = 1
          end
          object cxFunctParamsTIP_REFLECTARE: TcxDBEditorRow
            Properties.Caption = 'Clasa Bugetara (Chet/Ven)'
            Properties.EditPropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.Items = <
              item
                Description = 'Cheltuieli'
                ImageIndex = 0
                Value = 0
              end
              item
                Description = 'Venituri'
                Value = 1
              end
              item
                Description = 'Nivel Grupare'
                Value = 2
              end>
            Properties.DataBinding.FieldName = 'TIP_REFLECTARE'
            ID = 6
            ParentID = 1
            Index = 4
            Version = 1
          end
          object cxFunctParamsTIP_SECTIUNE: TcxDBEditorRow
            Properties.Caption = 'Sectiune'
            Properties.EditPropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.Items = <
              item
                Description = 'Toate'
                ImageIndex = 0
                Value = 0
              end
              item
                Description = 'Sectiune Functionare'
                Value = 1
              end
              item
                Description = 'Sectiune Dezvoltare'
                Value = 2
              end>
            Properties.DataBinding.FieldName = 'TIP_SECTIUNE'
            ID = 7
            ParentID = 1
            Index = 5
            Version = 1
          end
          object cxFunctParamsESTE_NIVEL_RAPORTARE: TcxDBEditorRow
            Height = 17
            Properties.Caption = 'Nivel Raportare'
            Properties.EditPropertiesClassName = 'TcxCheckBoxProperties'
            Properties.EditProperties.Alignment = taLeftJustify
            Properties.EditProperties.NullStyle = nssUnchecked
            Properties.EditProperties.ReadOnly = False
            Properties.EditProperties.ValueChecked = 'True'
            Properties.EditProperties.ValueGrayed = ''
            Properties.EditProperties.ValueUnchecked = 'False'
            Properties.DataBinding.FieldName = 'ESTE_NIVEL_RAPORTARE'
            ID = 8
            ParentID = 1
            Index = 6
            Version = 1
          end
          object cxFunctParamsALIAS_CONT: TcxDBEditorRow
            Properties.Caption = 'Alias balanta forexe'
            Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
            Properties.DataBinding.FieldName = 'alias_cont'
            ID = 9
            ParentID = 1
            Index = 7
            Version = 1
          end
          object cxFunctParamsESTE_STANDARD: TcxDBEditorRow
            Properties.Caption = 'Standard in legislatie'
            Properties.DataBinding.FieldName = 'ESTE_STANDARD'
            ID = 10
            ParentID = 1
            Index = 8
            Version = 1
          end
          object cxFunctParamsID_BG_TIPURI_BUGET: TcxDBEditorRow
            Properties.Caption = 'Tipul de Buget asociat'
            Properties.EditPropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.Items = <>
            Properties.DataBinding.FieldName = 'ID_BG_TIPURI_BUGET'
            ID = 11
            ParentID = 1
            Index = 9
            Version = 1
          end
          object cxFunctParamsTIP_BUGET: TcxDBEditorRow
            Properties.Caption = 'Codul tipului de buget'
            Properties.EditPropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.EditProperties.Items = <>
            Properties.DataBinding.FieldName = 'TIP_BUGET'
            ID = 12
            ParentID = 1
            Index = 10
            Version = 1
          end
          object cxFunctParamsCategoryRow2: TcxCategoryRow
            Properties.Caption = 'Detalii Identificare'
            ID = 13
            ParentID = -1
            Index = 2
            Version = 1
          end
          object cxFunctParamsID_BG_PLAN_FUNCTIONAL: TcxDBEditorRow
            Properties.Caption = 'Identificator'
            Properties.EditPropertiesClassName = 'TcxTextEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.DataBinding.FieldName = 'ID_BG_PLAN_FUNCTIONAL'
            Properties.Options.Editing = False
            Styles.Content = frmData.cxStyle5
            ID = 14
            ParentID = 13
            Index = 0
            Version = 1
          end
          object cxFunctParamsCLASA: TcxDBEditorRow
            Properties.Caption = 'Clasa Identificare'
            Properties.EditPropertiesClassName = 'TcxTextEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.DataBinding.FieldName = 'CLASA'
            Properties.Options.Editing = False
            Styles.Content = frmData.cxStyle5
            ID = 15
            ParentID = 13
            Index = 1
            Version = 1
          end
        end
      end
      object SplitterFunct: TcxSplitter
        Left = 686
        Top = 0
        Width = 8
        Height = 554
        HotZoneClassName = 'TcxMediaPlayer9Style'
        AlignSplitter = salRight
        Control = pnFunctDetalii
      end
      object pnFunctClient: TPanel
        Left = 0
        Top = 0
        Width = 686
        Height = 554
        Align = alClient
        BevelOuter = bvNone
        TabOrder = 2
        object Panel4: TPanel
          Left = 0
          Top = 0
          Width = 686
          Height = 33
          Align = alTop
          BevelOuter = bvNone
          TabOrder = 1
          DesignSize = (
            686
            33)
          object Label2: TLabel
            Left = 8
            Top = 8
            Width = 71
            Height = 13
            Caption = 'Filtru Tip Buget'
          end
          object edtFiltruBuget: TcxImageComboBox
            Left = 88
            Top = 5
            Anchors = [akLeft, akTop, akRight]
            Properties.Items = <>
            Properties.OnChange = edtFiltruBugetPropertiesChange
            Style.LookAndFeel.Kind = lfOffice11
            StyleDisabled.LookAndFeel.Kind = lfOffice11
            StyleFocused.LookAndFeel.Kind = lfOffice11
            StyleHot.LookAndFeel.Kind = lfOffice11
            TabOrder = 0
            Width = 590
          end
        end
        object cxTreeFunctional: TcxDBTreeList
          Left = 0
          Top = 33
          Width = 686
          Height = 521
          Align = alClient
          Bands = <
            item
              Caption.AlignHorz = taCenter
            end>
          DataController.DataSource = frmData.DTBGPlanFunctional
          DataController.ParentField = 'ID_PARINTE'
          DataController.KeyField = 'ID_BG_PLAN_FUNCTIONAL'
          DragMode = dmAutomatic
          FindPanel.Behavior = fcbSearch
          Images = ImaginiConturi
          LookAndFeel.Kind = lfOffice11
          Navigator.Buttons.CustomButtons = <>
          OptionsBehavior.GoToNextCellOnEnter = True
          OptionsBehavior.GoToNextCellOnTab = True
          OptionsBehavior.ImmediateEditor = False
          OptionsBehavior.DragCollapse = False
          OptionsBehavior.ExpandOnIncSearch = True
          OptionsBehavior.IncSearch = True
          OptionsBehavior.IncSearchItem = cxTreeFunctionalDESCRIERE
          OptionsBehavior.ShowHourGlass = False
          OptionsCustomizing.BandCustomizing = False
          OptionsCustomizing.BandVertSizing = False
          OptionsCustomizing.ColumnVertSizing = False
          OptionsData.CancelOnExit = False
          OptionsData.Editing = False
          OptionsData.Appending = True
          OptionsData.Deleting = False
          OptionsData.Inserting = True
          OptionsSelection.HideFocusRect = False
          OptionsSelection.InvertSelect = False
          OptionsSelection.MultiSelect = True
          OptionsView.CellTextMaxLineCount = -1
          OptionsView.ShowEditButtons = ecsbFocused
          OptionsView.ColumnAutoWidth = True
          OptionsView.ExtPaintStyle = True
          ParentColor = False
          PopupMenu = ppCF
          Preview.AutoHeight = False
          Preview.MaxLineCount = 0
          RootValue = -1
          ScrollbarAnnotations.CustomAnnotations = <>
          Styles.StyleSheet = frmData.TreeListStyleSheetHighContrastWhite
          Styles.IncSearch = frmData.cxStyle12
          TabOrder = 0
          OnCustomDrawDataCell = cxTreeFunctionalCustomDrawDataCell
          OnDragDrop = cxTreeFunctionalDragDrop
          OnDragOver = cxTreeFunctionalDragOver
          OnEndDrag = cxTreeFunctionalEndDrag
          OnGetNodeImageIndex = cxTreeFunctionalGetNodeImageIndex
          OnKeyDown = cxTreeFunctionalKeyDown
          object cxTreeFunctionalCOD_FUNCTIONAL: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Capitol'
            DataBinding.FieldName = 'COD_FUNCTIONAL'
            Width = 94
            Position.ColIndex = 0
            Position.RowIndex = 0
            Position.BandIndex = 0
            SortOrder = soAscending
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalDENUMIRE: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.CharCase = ecUpperCase
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Denumire'
            DataBinding.FieldName = 'DENUMIRE'
            Width = 107
            Position.ColIndex = 1
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalDESCRIERE: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Caption.AlignHorz = taCenter
            Caption.Text = 'Descriere'
            DataBinding.FieldName = 'DESCRIERE'
            Width = 230
            Position.ColIndex = 2
            Position.RowIndex = 0
            Position.BandIndex = 0
            SortOrder = soAscending
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
            OnGetDisplayText = cxTreeFunctionalDESCRIEREGetDisplayText
          end
          object cxTreeFunctionalPLANIFICAT1: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.;-,0.'
            Properties.Nullable = False
            Properties.ReadOnly = True
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Plan. Trim. I'
            DataBinding.FieldName = 'PLANIFICAT1'
            Options.Editing = False
            Width = 495
            Position.ColIndex = 5
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalPLANIFICAT2: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.;-,0.'
            Properties.Nullable = False
            Properties.ReadOnly = True
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Plan. Trim. II'
            DataBinding.FieldName = 'PLANIFICAT2'
            Options.Editing = False
            Width = 495
            Position.ColIndex = 8
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalPLANIFICAT3: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.;-,0.'
            Properties.Nullable = False
            Properties.ReadOnly = True
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Plan. Trim. III'
            DataBinding.FieldName = 'PLANIFICAT3'
            Options.Editing = False
            Width = 495
            Position.ColIndex = 7
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalPLANIFICAT4: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.;-,0.'
            Properties.Nullable = False
            Properties.ReadOnly = True
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Plan. Trim. IV'
            DataBinding.FieldName = 'PLANIFICAT4'
            Options.Editing = False
            Width = 495
            Position.ColIndex = 3
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalCLASA: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Clasa'
            DataBinding.FieldName = 'CLASA'
            Options.Editing = False
            Width = 80
            Position.ColIndex = 4
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalCAPITOL: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Capitol'
            DataBinding.FieldName = 'CAPITOL'
            Width = 80
            Position.ColIndex = 6
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalESTE_LUCRARE: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCheckBoxProperties'
            Properties.Alignment = taLeftJustify
            Properties.NullStyle = nssUnchecked
            Properties.ReadOnly = True
            Properties.ValueChecked = 'True'
            Properties.ValueGrayed = ''
            Properties.ValueUnchecked = 'False'
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Lucrare'
            DataBinding.FieldName = 'ESTE_LUCRARE'
            MinWidth = 16
            Width = 50
            Position.ColIndex = 9
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeFunctionalESTE_NIVEL_RAPORTARE: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCheckBoxProperties'
            Properties.Alignment = taLeftJustify
            Properties.NullStyle = nssUnchecked
            Properties.ReadOnly = True
            Properties.ValueChecked = 'True'
            Properties.ValueGrayed = ''
            Properties.ValueUnchecked = 'False'
            Visible = False
            Caption.Text = 'Nivel Raportare'
            DataBinding.FieldName = 'ESTE_NIVEL_RAPORTARE'
            MinWidth = 16
            Width = 46
            Position.ColIndex = 10
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
        end
      end
    end
    object cxTabEconomic: TcxTabSheet
      Caption = 'Clasificatie Economica'
      ImageIndex = 3
      object pnEcoDetalii: TPanel
        Left = 654
        Top = 0
        Width = 388
        Height = 554
        Align = alRight
        BevelOuter = bvNone
        TabOrder = 0
        object cxEcoDetalii: TcxDBVerticalGrid
          Left = 0
          Top = 0
          Width = 388
          Height = 554
          BorderStyle = cxcbsNone
          Align = alClient
          LookAndFeel.Kind = lfOffice11
          LookAndFeel.NativeStyle = False
          LookAndFeel.SkinName = ''
          OptionsView.CellTextMaxLineCount = 3
          OptionsView.ScrollBars = ssVertical
          OptionsView.AutoScaleBands = False
          OptionsView.GridLineColor = clBtnShadow
          OptionsView.RowHeaderMinWidth = 30
          OptionsView.RowHeaderWidth = 193
          OptionsView.RowHeight = 20
          OptionsView.ValueWidth = 62
          OptionsBehavior.GoToNextCellOnEnter = True
          OptionsBehavior.GoToNextCellOnTab = True
          OptionsData.CancelOnExit = False
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          Styles.StyleSheet = frmData.cxVerticalGridStyleSheetHighContrastWhite
          TabOrder = 0
          DataController.DataSource = frmData.DTBGPlanEconomic
          Version = 1
          object cxEcoDetaliiCategoryRow1: TcxCategoryRow
            Properties.Caption = 'Detalii Titlu/Articol/Aliniat'
            ID = 0
            ParentID = -1
            Index = 0
            Version = 1
          end
          object cxEcoDetaliiCOD_BUGET: TcxDBEditorRow
            Height = 20
            Properties.Caption = 'Titlu'
            Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.MaxLength = 0
            Properties.EditProperties.ReadOnly = False
            Properties.DataBinding.FieldName = 'COD_ECONOMIC'
            ID = 1
            ParentID = 0
            Index = 0
            Version = 1
          end
          object cxEcoDetaliiDENUMIRE: TcxDBEditorRow
            Expanded = False
            Height = 50
            Properties.Caption = 'Denumire'
            Properties.EditPropertiesClassName = 'TcxMaskEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.MaxLength = 0
            Properties.EditProperties.ReadOnly = False
            Properties.DataBinding.FieldName = 'DENUMIRE'
            ID = 2
            ParentID = 0
            Index = 1
            Version = 1
          end
          object cxEcoDetaliiTIP_REFLECTARE: TcxDBEditorRow
            Properties.Caption = 'Clasa Bugetara (Chet/Ven)'
            Properties.EditPropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.Items = <
              item
                Description = 'Cheltuieli'
                ImageIndex = 0
                Value = 0
              end
              item
                Description = 'Venituri'
                Value = 1
              end
              item
                Description = 'Nivel Grupare'
                Value = 2
              end>
            Properties.DataBinding.FieldName = 'TIP_REFLECTARE'
            ID = 3
            ParentID = 0
            Index = 2
            Version = 1
          end
          object cxEcoDetaliiTIP_SECTIUNE: TcxDBEditorRow
            Properties.Caption = 'Sectiune'
            Properties.EditPropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.Items = <
              item
                Description = 'Toate'
                ImageIndex = 0
                Value = 0
              end
              item
                Description = 'Sectiune Functionare'
                Value = 1
              end
              item
                Description = 'Sectiune Dezvoltare'
                Value = 2
              end>
            Properties.DataBinding.FieldName = 'TIP_SECTIUNE'
            ID = 4
            ParentID = 0
            Index = 3
            Version = 1
          end
          object cxEcoDetaliiBOLD: TcxDBEditorRow
            Properties.Caption = 'Ingrosat'
            Properties.EditPropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.EditProperties.Items = <
              item
                Description = 'Apare Ingrosat'
                ImageIndex = 0
                Value = 1
              end
              item
                Description = 'Nu apare ingrosat'
                Value = 0
              end>
            Properties.DataBinding.FieldName = 'BOLD'
            ID = 5
            ParentID = 0
            Index = 4
            Version = 1
          end
          object cxEcoDetaliiESTE_STANDARD: TcxDBEditorRow
            Properties.Caption = 'Standard in legislatie'
            Properties.DataBinding.FieldName = 'ESTE_STANDARD'
            ID = 6
            ParentID = 0
            Index = 5
            Version = 1
          end
          object cxEcoDetaliiINTRODUCERE_CA: TcxDBEditorRow
            Properties.Caption = 'Introducere credit angajament'
            Properties.EditPropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.EditProperties.Items = <
              item
                Description = 'Se permite introducere'
                ImageIndex = 0
                Value = 1
              end
              item
                Description = 'Nu se permite'
                Value = 0
              end>
            Properties.DataBinding.FieldName = 'INTRODUCERE_CA'
            ID = 7
            ParentID = 0
            Index = 6
            Version = 1
          end
          object cxEcoDetaliiINTRODUCERE_ESTIMARE: TcxDBEditorRow
            Properties.Caption = 'Introducere estimare'
            Properties.EditPropertiesClassName = 'TcxImageComboBoxProperties'
            Properties.EditProperties.Items = <
              item
                Description = 'Se permite introducere'
                ImageIndex = 0
                Value = 1
              end
              item
                Description = 'Nu se permite'
                Value = 0
              end>
            Properties.DataBinding.FieldName = 'INTRODUCERE_ESTIMARE'
            ID = 8
            ParentID = 0
            Index = 7
            Version = 1
          end
          object cxEcoDetaliiCategoryRow2: TcxCategoryRow
            Properties.Caption = 'Detalii Indentificare'
            ID = 9
            ParentID = -1
            Index = 1
            Version = 1
          end
          object cxEcoDetaliiID_BG_PLAN_ECONOMIC: TcxDBEditorRow
            Properties.Caption = 'Identificator'
            Properties.EditPropertiesClassName = 'TcxTextEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.DataBinding.FieldName = 'ID_BG_PLAN_ECONOMIC'
            Properties.Options.Editing = False
            Styles.Content = frmData.cxStyle5
            ID = 10
            ParentID = 9
            Index = 0
            Version = 1
          end
          object cxEcoDetaliiID_PARINTE: TcxDBEditorRow
            Properties.Caption = 'Id Parinte'
            Properties.EditPropertiesClassName = 'TcxTextEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.DataBinding.FieldName = 'ID_PARINTE'
            Properties.Options.Editing = False
            Styles.Content = frmData.cxStyle5
            ID = 11
            ParentID = 9
            Index = 1
            Version = 1
          end
          object cxEcoDetaliiCLASA: TcxDBEditorRow
            Properties.Caption = 'Clasa Indentificare'
            Properties.EditPropertiesClassName = 'TcxTextEditProperties'
            Properties.EditProperties.Alignment.Horz = taLeftJustify
            Properties.DataBinding.FieldName = 'CLASA'
            Properties.Options.Editing = False
            Styles.Content = frmData.cxStyle5
            ID = 12
            ParentID = 9
            Index = 2
            Version = 1
          end
          object cxEcoDetaliiCategoryRow3: TcxCategoryRow
            Properties.Caption = 'Coduri CPV'
            ID = 13
            ParentID = -1
            Index = 2
            Version = 1
          end
          object cxEcoDetaliiDBEditorRow1: TcxDBEditorRow
            Height = 26
            Properties.Caption = '    Denumire'
            ID = 14
            ParentID = -1
            Index = 3
            Version = 1
          end
        end
        object cxDBTreeList1: TcxDBTreeList
          Left = -264
          Top = 240
          Width = 250
          Height = 150
          Bands = <>
          Navigator.Buttons.CustomButtons = <>
          RootValue = -1
          ScrollbarAnnotations.CustomAnnotations = <>
          TabOrder = 1
        end
      end
      object cxSplitter1: TcxSplitter
        Left = 646
        Top = 0
        Width = 8
        Height = 554
        HotZoneClassName = 'TcxMediaPlayer9Style'
        AlignSplitter = salRight
        Control = pnEcoDetalii
      end
      object pnEcoClient: TPanel
        Left = 0
        Top = 0
        Width = 646
        Height = 554
        Align = alClient
        BevelOuter = bvNone
        TabOrder = 2
        object cxTreeEconomic: TcxDBTreeList
          Left = 0
          Top = 0
          Width = 646
          Height = 554
          Align = alClient
          Bands = <
            item
              Caption.AlignHorz = taCenter
            end>
          DataController.DataSource = frmData.DTBGPlanEconomic
          DataController.ParentField = 'ID_PARINTE'
          DataController.KeyField = 'ID_BG_PLAN_ECONOMIC'
          DragMode = dmAutomatic
          Images = ImaginiConturi
          LookAndFeel.Kind = lfOffice11
          Navigator.Buttons.CustomButtons = <>
          OptionsBehavior.GoToNextCellOnEnter = True
          OptionsBehavior.GoToNextCellOnTab = True
          OptionsBehavior.AutoDragCopy = True
          OptionsBehavior.DragCollapse = False
          OptionsBehavior.ExpandOnIncSearch = True
          OptionsBehavior.IncSearch = True
          OptionsBehavior.IncSearchItem = cxTreeEconomicDESCRIERE
          OptionsBehavior.ShowHourGlass = False
          OptionsCustomizing.BandCustomizing = False
          OptionsCustomizing.BandVertSizing = False
          OptionsCustomizing.ColumnVertSizing = False
          OptionsData.CancelOnExit = False
          OptionsData.Editing = False
          OptionsData.Appending = True
          OptionsData.Deleting = False
          OptionsData.Inserting = True
          OptionsSelection.HideFocusRect = False
          OptionsSelection.InvertSelect = False
          OptionsSelection.MultiSelect = True
          OptionsView.CellTextMaxLineCount = -1
          OptionsView.ShowEditButtons = ecsbFocused
          OptionsView.ColumnAutoWidth = True
          ParentColor = False
          PopupMenu = ppCE
          Preview.AutoHeight = False
          Preview.MaxLineCount = 0
          RootValue = -1
          ScrollbarAnnotations.CustomAnnotations = <>
          Styles.StyleSheet = frmData.TreeListStyleSheetHighContrastWhite
          Styles.IncSearch = frmData.cxStyle12
          TabOrder = 0
          OnDragDrop = cxTreeEconomicDragDrop
          OnDragOver = cxTreeEconomicDragOver
          OnGetNodeImageIndex = cxTreeEconomicGetNodeImageIndex
          object cxTreeEconomicCOD_ECONOMIC: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Titlu'
            DataBinding.FieldName = 'COD_ECONOMIC'
            Width = 76
            Position.ColIndex = 0
            Position.RowIndex = 0
            Position.BandIndex = 0
            SortOrder = soAscending
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicDENUMIRE: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.CharCase = ecUpperCase
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Denumire'
            DataBinding.FieldName = 'DENUMIRE'
            Width = 209
            Position.ColIndex = 1
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicDESCRIERE: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Caption.AlignHorz = taCenter
            Caption.Text = 'Descriere'
            DataBinding.FieldName = 'DESCRIERE'
            Width = 96
            Position.ColIndex = 2
            Position.RowIndex = 0
            Position.BandIndex = 0
            SortOrder = soAscending
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
            OnGetDisplayText = cxTreeEconomicDESCRIEREGetDisplayText
          end
          object cxTreeEconomicNUMAR_RAND: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taCenter
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Nr. Rand'
            DataBinding.FieldName = 'NUMAR_RAND'
            Width = 55
            Position.ColIndex = 3
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicCLASA: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.;-,0.'
            Properties.Nullable = False
            Properties.ReadOnly = True
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Clasa'
            DataBinding.FieldName = 'CLASA'
            Options.Editing = False
            Width = 81
            Position.ColIndex = 4
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
        end
      end
      object cxLookupComboBox1: TcxLookupComboBox
        Left = 855
        Top = 328
        Align = alCustom
        Anchors = [akTop, akRight]
        Properties.Alignment.Horz = taLeftJustify
        Properties.DropDownListStyle = lsFixedList
        Properties.KeyFieldNames = 'codCPV'
        Properties.ListColumns = <
          item
            FieldName = 'CPV_AFISARE'
          end>
        Properties.ListOptions.ShowHeader = False
        Properties.ListSource = dtCPV
        TabOrder = 3
        Width = 177
      end
    end
  end
  object BtnOk: TcxButton
    Left = 875
    Top = 580
    Width = 75
    Height = 25
    Anchors = [akRight, akBottom]
    Caption = 'Ok'
    LookAndFeel.Kind = lfOffice11
    TabOrder = 0
    OnClick = BtnOkClick
  end
  object BtnCancel: TcxButton
    Left = 956
    Top = 580
    Width = 75
    Height = 25
    Anchors = [akRight, akBottom]
    Caption = 'Cancel'
    LookAndFeel.Kind = lfOffice11
    TabOrder = 1
    OnClick = BtnCancelClick
  end
  object ImaginiConturi: TImageList
    Left = 80
    Top = 208
    Bitmap = {
      494C010103000500040010001000FFFFFFFFFF10FFFFFFFFFFFFFFFF424D3600
      0000000000003600000028000000400000001000000001002000000000000010
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000FFFF00FFFF
      FF0000FFFF0000FFFF0000848400000000000000000000000000000000000000
      00000000000000000000000000000000000084848400C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600C6C6C600C6C6C6000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000FFFF00FFFF
      FF0000FFFF0000FFFF008484840084848400FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000848400FFFF
      FF0000FFFF000084840000FFFF00008484000000000000000000000000000000
      0000000000000000000000000000000000008484840000FFFF00FFFFFF00FFFF
      FF00FFFFFF0000FFFF00FFFFFF00FFFFFF000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000848400FFFF
      FF0000FFFF000084840084848400FFFFFF00FFFFFF00FF000000FF000000FFFF
      FF00FFFFFF00FFFFFF0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000FFFF00FFFF
      FF0000FFFF0000FFFF00008484000084840000000000C6C6C600C6C6C600C6C6
      C600C6C6C600C6C6C600C6C6C6000000000084848400FFFFFF00000000000000
      0000000000000000000000000000FFFFFF000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000FFFF00FFFF
      FF0000FFFF0084848400FFFFFF00FFFFFF00FF000000C6C6C600FFFFFF00FFFF
      FF00FFFFFF00FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000848400FFFF
      FF0000FFFF000084840000FFFF000084840000000000FFFFFF00FFFFFF0000FF
      FF00FFFFFF00FFFFFF00C6C6C600000000008484840000FFFF00FFFFFF00FFFF
      FF00FFFFFF0000FFFF00FFFFFF00FFFFFF000000000000FFFF00FFFFFF0000FF
      FF0000FFFF00008484000000000000000000848484000084840000848400FFFF
      FF0000FFFF0084848400FFFFFF00FF000000FF000000FFFFFF00FFFFFF00FF00
      0000FFFFFF00FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000FFFF00FFFF
      FF0000FFFF0000FFFF0000848400008484000000000000000000000000000000
      00000000000000FFFF00C6C6C6000000000084848400FFFFFF00000000000000
      0000000000000000000000000000000000000000000000848400FFFFFF0000FF
      FF000084840000FFFF000084840000000000848484000084840000FFFF00FFFF
      FF0000FFFF0084848400FFFFFF00FF000000FF000000C6C6C600C6C6C600FF00
      0000FF000000FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000848400FFFF
      FF0000FFFF000084840000FFFF000084840000000000FFFFFF00FFFFFF0000FF
      FF00FFFFFF00FFFFFF00C6C6C600000000008484840000FFFF00FFFFFF00FFFF
      FF000000000000FF000000FF000000FF00000000000000FFFF00FFFFFF000000
      000000000000008484000084840000000000848484000084840000848400FFFF
      FF0000FFFF0084848400FFFFFF00C6C6C600FF000000FF000000FF000000FF00
      0000FF000000FF000000FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000FFFF00FFFF
      FF0000FFFF0000FFFF0000848400008484000000000000000000000000000000
      00000000000000FFFF00C6C6C6000000000084848400FFFFFF00000000000000
      0000000000000000000000FF000000FF000000FF000000000000FFFFFF000000
      00000000000000FFFF000084840000000000848484000084840000FFFF00FFFF
      FF0000FFFF0084848400FFFFFF00FFFFFF00C6C6C600FF000000FF000000FF00
      0000FF000000FFFFFF00FFFFFF00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000848484000084840000848400FFFF
      FF0000FFFF0000848400FFFFFF000084840000000000FFFFFF00FFFFFF0000FF
      FF00FFFFFF00FFFFFF00C6C6C600000000008484840000FFFF00FFFFFF00FFFF
      FF00FFFFFF0000FFFF000000000000FF000000FF000000FF00000000000000FF
      000000000000008484000084840000000000848484000084840000848400FFFF
      FF0000FFFF000084840084848400FFFFFF00FFFFFF00FFFFFF00FFFFFF00FF00
      0000FFFFFF00FFFFFF0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000008484840000848400FFFFFF0000FF
      FF000000000000FFFF00FFFFFF0000FFFF000000000000000000000000000000
      00000000000000FFFF00C6C6C6000000000084848400FFFFFF00000000000000
      000000000000FFFFFF00FFFFFF000000000000FF000000FF000000FF000000FF
      00000000000000FFFF0000848400000000008484840000848400FFFFFF0000FF
      FF000000000000FFFF008484840084848400FFFFFF00FFFFFF00FFFFFF00FFFF
      FF00FFFFFF000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000084848400FFFFFF0000FFFF000000
      000000FF00000000000000FFFF00FFFFFF0000000000FFFFFF00FFFFFF0000FF
      FF00FFFFFF00FFFFFF00C6C6C600000000008484840000FFFF00FFFFFF00FFFF
      FF00FFFFFF0000FFFF0084840000848400000000000000FF000000FF000000FF
      00000000000000848400008484000000000084848400FFFFFF0000FFFF000000
      000000FF00000000000000FFFF00FFFFFF008484840084848400848484008484
      840084848400FFFFFF00C6C6C600000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000848484000000000000FF
      000000FF000000FF00000000000084848400000000000000000000000000FFFF
      FF00FFFFFF00C6C6C600C6C6C6000000000084848400FFFFFF00FFFFFF0000FF
      FF00FFFFFF00FFFFFF00848400000000000000FF000000FF000000FF000000FF
      000000000000FFFFFF00008484000000000000000000848484000000000000FF
      000000FF000000FF00000000000084848400000000000000000000000000FFFF
      FF00FFFFFF00C6C6C600C6C6C600000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000FF000000FF
      000000FF000000FF000000FF000000000000FFFFFF00FFFFFF00FFFFFF0000FF
      FF00848400008484000084840000000000008484840084848400848484008484
      8400848484008484840000000000000000000000000000000000000000000000
      000000000000FFFFFF0000FFFF0000000000000000000000000000FF000000FF
      000000FF000000FF000000FF000000000000FFFFFF00FFFFFF00FFFFFF0000FF
      FF00848400008484000084840000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000000000000000000000FF
      000000FF000000FF000000000000FFFFFF00FFFFFF0000FFFF00FFFFFF00FFFF
      FF0084840000FFFF000000000000000000000000000000000000000000000000
      000000000000000000000000000084848400FFFFFF0000FFFF00FFFFFF0000FF
      FF00FFFFFF0000FFFF00FFFFFF000000000000000000000000000000000000FF
      000000FF000000FF000000000000FFFFFF00FFFFFF0000FFFF00FFFFFF00FFFF
      FF0084840000FFFF000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000FF000000FF000000FF0000008400000084000000000000848484008484
      8400848400000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000084848400FFFFFF0000FFFF00FFFF
      FF0000FFFF00FFFFFF0084848400000000000000000000000000000000000000
      000000FF000000FF000000FF0000008400000084000000000000848484008484
      8400848400000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000084848400848484008484
      8400848484008484840000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000424D3E000000000000003E000000
      2800000040000000100000000100010000000000800000000000000000000000
      000000000000000000000000FFFFFF00C1FF007FC107000080FF007F80010000
      0000007F00000000000000030000000000000001000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000080000000800000008000000080000000C001FE00C0010000
      E003FF01E0030000F07FFF83F07F000000000000000000000000000000000000
      000000000000}
  end
  object ppCF: TPopupMenu
    Left = 184
    Top = 143
    object AdaugaCapitol1: TMenuItem
      Caption = 'Adauga Capitol'
      ShortCut = 45
      OnClick = CmdAdaugaCapitolExecute
    end
    object Adaugasubcapitol1: TMenuItem
      Caption = 'Adauga subcapitol'
      ShortCut = 16429
      OnClick = CmdAdaugaSubCapitolExecute
    end
    object MutaCapitolulpeSintetic1: TMenuItem
      Caption = 'Muta Capitolul pe Sintetic'
      OnClick = CmdCapitolPeSinteticExecute
    end
    object N2: TMenuItem
      Caption = '-'
    end
    object Stergeclasificatiecurenta1: TMenuItem
      Action = CmdStergeCapitol
    end
  end
  object ppCE: TPopupMenu
    Left = 184
    Top = 207
    object AdaugaSubtitlu1: TMenuItem
      Caption = 'Adauga Titlu'
      ShortCut = 45
      OnClick = CmdAdaugaTitluExecute
    end
    object AdaugaSubtitlu2: TMenuItem
      Caption = 'Adauga Subtitlu'
      ShortCut = 16429
      OnClick = CmdAdaugaSubtitluExecute
    end
    object MutaTitlupeSintetic1: TMenuItem
      Caption = 'Muta Titlu pe Sintetic'
      OnClick = CmdTitluPeSinteticExecute
    end
    object N3: TMenuItem
      Caption = '-'
    end
    object StergeTitlu1: TMenuItem
      Action = CmdStergeTitlu
    end
    object N1: TMenuItem
      Caption = '-'
    end
  end
  object Actuni: TActionList
    Left = 80
    Top = 143
    object CmdAdaugaCapitol: TAction
      Caption = 'Adauga Capitol'
      ShortCut = 45
      OnExecute = CmdAdaugaCapitolExecute
    end
    object CmdAdaugaSubCapitol: TAction
      Caption = 'Adauga subcapitol'
      ShortCut = 16429
      OnExecute = CmdAdaugaSubCapitolExecute
    end
    object CmdCapitolPeSintetic: TAction
      Caption = 'Muta Capitolul pe Sintetic'
      OnExecute = CmdCapitolPeSinteticExecute
    end
    object CmdAdaugaTitlu: TAction
      Caption = 'Adauga Titlu'
      ShortCut = 45
      OnExecute = CmdAdaugaTitluExecute
    end
    object CmdAdaugaSubtitlu: TAction
      Caption = 'Adauga Subtitlu'
      ShortCut = 16429
      OnExecute = CmdAdaugaSubtitluExecute
    end
    object CmdTitluPeSintetic: TAction
      Caption = 'Muta Titlu pe Sintetic'
      OnExecute = CmdTitluPeSinteticExecute
    end
    object CmdStergeCapitol: TAction
      Caption = 'Sterge clasificatie curenta'
      ShortCut = 16430
      OnExecute = CmdStergeCapitolExecute
    end
    object CmdStergeTitlu: TAction
      Caption = 'Sterge Titlu'
      ShortCut = 16430
      OnExecute = CmdStergeTitluExecute
    end
  end
  object ZQuery1: TZQuery
    Params = <>
    Left = 64920
    Top = 304
  end
  object qryCPV: TZQuery
    Connection = frmData.dbContabilitate
    OnCalcFields = qryCPVCalcFields
    SQL.Strings = (
      'select * from coduriCPV;')
    Params = <>
    Left = 160
    Top = 312
    object qryCPVCPV_AFISARE: TStringField
      FieldKind = fkCalculated
      FieldName = 'CPV_AFISARE'
      Size = 80
      Calculated = True
    end
    object qryCPVcodCPV: TStringField
      FieldName = 'codCPV'
      Size = 25
    end
    object qryCPVDENUMIRE: TStringField
      FieldName = 'DENUMIRE'
      Size = 255
    end
  end
  object dtCPV: TDataSource
    DataSet = qryCPV
    Left = 80
    Top = 312
  end
end
