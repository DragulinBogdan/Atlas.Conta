object frmAlopDisponibil: TfrmAlopDisponibil
  Left = 335
  Top = 254
  Caption = 'Disponibil Bugetar'
  ClientHeight = 458
  ClientWidth = 960
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  DesignSize = (
    960
    458)
  PixelsPerInch = 96
  TextHeight = 13
  object pageBuget: TcxPageControl
    Left = 0
    Top = 0
    Width = 960
    Height = 385
    Align = alTop
    Anchors = [akLeft, akTop, akRight, akBottom]
    TabOrder = 0
    Properties.ActivePage = tabGlobal
    Properties.CustomButtons.Buttons = <>
    Properties.Style = 9
    Properties.TabPosition = tpBottom
    Properties.TabSlants.Positions = [spLeft, spRight]
    LookAndFeel.Kind = lfOffice11
    OnChange = pageBugetChange
    ClientRectBottom = 365
    ClientRectRight = 960
    ClientRectTop = 0
    object tabLegal: TcxTabSheet
      Caption = 'Angajamente Legale'
      DesignSize = (
        960
        365)
      object lbFurnizor: TLabel
        Left = 16
        Top = 8
        Width = 240
        Height = 13
        Caption = 'Furnizorul asociata angajamentului legal : '
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object lbListaPozitiiAngajamente: TLabel
        Left = 16
        Top = 60
        Width = 277
        Height = 13
        Caption = 'Lista pozitiilor angajate pentru repartitorul curent'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Bevel2: TBevel
        Left = 295
        Top = 66
        Width = 656
        Height = 9
        Anchors = [akLeft, akTop, akRight]
        Shape = bsTopLine
        ExplicitWidth = 648
      end
      object lbSumaDeFacturat: TLabel
        Left = 256
        Top = 8
        Width = 102
        Height = 13
        Caption = 'Suma Facturata : '
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
        Visible = False
      end
      object edFurnizor: TcxRepartitorPanel
        Left = 22
        Top = 24
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
        PopupEdit.PopupControl = TreeRepartitori
        PopupEdit.PopupFlatBorder = False
        PopupEdit.PopupFormBorderStyle = True
        PopupEdit.PopupHeight = 200
        PopupEdit.PopupMinHeight = 100
        PopupEdit.PopupMinWidth = 100
        PopupEdit.PopupSizeable = True
        PopupEdit.PopupWidth = 250
        PopupEdit.Style.Color = clWindow
        PopupEdit.Visible = True
        PopupEdit.Width = 815
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
        DataSource = frmData.DTRepartitori
        CodField = 'COD_FISCAL'
        KeyField = 'ID_REPARTITORI'
        ListField = 'NUME'
        OnPopupInitPopup = edClasaFunctionalaPopupInitPopup
        OnPopupPopup = edFurnizorPopupPopup
        OnValidate = edFurnizorValidate
        Height = 30
        Width = 935
      end
      object GridAngajate: TcxGrid
        Left = 16
        Top = 76
        Width = 935
        Height = 268
        Anchors = [akLeft, akTop, akRight, akBottom]
        TabOrder = 2
        LookAndFeel.Kind = lfOffice11
        object GridAngajateDBTableView1: TcxGridDBTableView
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          DataController.Summary.DefaultGroupSummaryItems = <>
          DataController.Summary.FooterSummaryItems = <>
          DataController.Summary.SummaryGroups = <>
        end
        object GridAngajateV: TcxGridDBTableView
          OnDblClick = GridAngajateVDblClick
          OnKeyDown = GridAngajateVKeyDown
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          OnFocusedItemChanged = GridAngajateVFocusedItemChanged
          OnFocusedRecordChanged = GridAngajateVFocusedRecordChanged
          DataController.DataSource = DTAngajamente
          DataController.Filter.MaxValueListCount = 1000
          DataController.KeyFieldNames = 'ID_ANGAJAMENTE_DEFALCARE'
          DataController.Summary.DefaultGroupSummaryItems = <>
          DataController.Summary.FooterSummaryItems = <
            item
              Kind = skSum
              Column = GridAngajateVvalFacturare
            end>
          DataController.Summary.SummaryGroups = <>
          Filtering.ColumnPopup.MaxDropDownItemCount = 12
          OptionsBehavior.AlwaysShowEditor = True
          OptionsBehavior.FocusCellOnCycle = True
          OptionsCustomize.ColumnsQuickCustomization = True
          OptionsData.CancelOnExit = False
          OptionsData.Deleting = False
          OptionsData.DeletingConfirmation = False
          OptionsData.Inserting = False
          OptionsSelection.MultiSelect = True
          OptionsSelection.CheckBoxPosition = cbpIndicator
          OptionsSelection.CheckBoxVisibility = [cbvDataRow, cbvGroupRow]
          OptionsSelection.HideFocusRectOnExit = False
          OptionsSelection.MultiSelectMode = msmPersistent
          OptionsView.ColumnAutoWidth = True
          OptionsView.Footer = True
          OptionsView.GridLines = glNone
          OptionsView.GroupByBox = False
          OptionsView.GroupFooters = gfVisibleWhenExpanded
          OptionsView.Indicator = True
          Preview.AutoHeight = False
          Preview.MaxLineCount = 2
          object GridAngajateVSel: TcxGridDBColumn
            Caption = 'Sel'
            DataBinding.FieldName = 'SEL'
            PropertiesClassName = 'TcxCheckBoxProperties'
            Properties.ImmediatePost = True
            Visible = False
            HeaderAlignmentHorz = taCenter
            Width = 41
          end
          object GridAngajateVID: TcxGridDBColumn
            Caption = 'Id'
            DataBinding.FieldName = 'ID_ANGAJAMENTE_DEFALCARE'
            Visible = False
            Options.Editing = False
          end
          object GridAngajateVeste_procentual: TcxGridDBColumn
            Caption = 'Este %'
            DataBinding.FieldName = 'este_procentual'
            PropertiesClassName = 'TcxCheckBoxProperties'
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 58
          end
          object GridAngajateVprocProcent: TcxGridDBColumn
            Caption = '% Proiect'
            DataBinding.FieldName = 'procProcent'
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 61
          end
          object GridAngajateVvalFacturare: TcxGridDBColumn
            Caption = 'Valoare'
            DataBinding.FieldName = 'valFacturare'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.DisplayFormat = ',0.00;-,0.00'
            Properties.UseDisplayFormatWhenEditing = True
            Properties.UseLeftAlignmentOnEditing = False
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 96
          end
          object GridAngajateVNUMAR: TcxGridDBColumn
            Caption = 'Nr'
            DataBinding.FieldName = 'NUMAR'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 49
          end
          object GridAngajateVDATA_EMITERE: TcxGridDBColumn
            Caption = 'Data'
            DataBinding.FieldName = 'DATA_EMITERE'
            PropertiesClassName = 'TcxDateEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.DateButtons = [btnClear, btnToday]
            Properties.DateOnError = deToday
            Properties.InputKind = ikRegExpr
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 54
          end
          object GridAngajateVCLASA_FUNCTIONALA: TcxGridDBColumn
            Caption = 'Cap.'
            DataBinding.FieldName = 'COD_FUNCTIONAL'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 59
          end
          object GridAngajateVCOD_ECONOMIC: TcxGridDBColumn
            Caption = 'Titlu/Art'
            DataBinding.FieldName = 'COD_ECONOMIC'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 53
          end
          object GridAngajateVSCOPUL: TcxGridDBColumn
            Caption = 'Scopul'
            DataBinding.FieldName = 'SCOPUL'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 182
          end
          object GridAngajateVDESCRIERE: TcxGridDBColumn
            Caption = 'Descriere'
            DataBinding.FieldName = 'DESCRIERE'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 70
          end
          object GridAngajateVANGAJAT: TcxGridDBColumn
            Caption = 'Angajat'
            DataBinding.FieldName = 'ANGAJAT'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Properties.Nullable = False
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 64
          end
          object GridAngajateVFACTURAT: TcxGridDBColumn
            Caption = 'Facturat'
            DataBinding.FieldName = 'FACTURAT'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 61
          end
          object GridAngajateVRAMAS_DE_ANGAJAT: TcxGridDBColumn
            Caption = 'De facturat'
            DataBinding.FieldName = 'RAMAS_DE_ANGAJAT'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00 ;-,0.00 '
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 77
          end
          object GridAngajateVPROC_FACTURAT: TcxGridDBColumn
            Caption = 'Proc. %'
            DataBinding.FieldName = 'PROC_FACTURAT'
            PropertiesClassName = 'TcxProgressBarProperties'
            Options.Editing = False
            Width = 50
          end
          object GridAngajateVID_ANGAJAMENTE_DEFALCARE: TcxGridDBColumn
            DataBinding.FieldName = 'ID_ANGAJAMENTE_DEFALCARE'
            Visible = False
            Options.Editing = False
          end
          object GridAngajateVID_ANALITIC: TcxGridDBColumn
            DataBinding.FieldName = 'ID_ANALITIC'
            Visible = False
            Options.Editing = False
          end
          object GridAngajateVid_oi_unitati: TcxGridDBColumn
            Caption = 'Id Unitate'
            DataBinding.FieldName = 'id_oi_unitati'
            Visible = False
            Options.Editing = False
          end
          object GridAngajateVid_oi_proiecte: TcxGridDBColumn
            Caption = 'Id Proiect'
            DataBinding.FieldName = 'id_oi_proiecte'
            Visible = False
            Options.Editing = False
          end
          object GridAngajateVContract_NR: TcxGridDBColumn
            Caption = 'Nr Contract'
            DataBinding.FieldName = 'nr_contract'
            Width = 60
          end
          object GridAngajateVContract_Data: TcxGridDBColumn
            Caption = 'Data Contract'
            DataBinding.FieldName = 'data_contract'
            Width = 73
          end
        end
        object GridAngajateL: TcxGridLevel
          GridView = GridAngajateV
        end
      end
      object ChkRepNeFacturat: TcxCheckBox
        Left = 16
        Top = 343
        Anchors = [akLeft, akBottom]
        Caption = 'Arata doar pozitiile nefacturate'
        ParentFont = False
        State = cbsChecked
        Style.Font.Charset = DEFAULT_CHARSET
        Style.Font.Color = clWindowText
        Style.Font.Height = -11
        Style.Font.Name = 'MS Sans Serif'
        Style.Font.Style = [fsBold]
        Style.LookAndFeel.Kind = lfOffice11
        Style.IsFontAssigned = True
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 1
        OnClick = ChkRepNeFacturatClick
      end
      object TreeRepartitori: TcxDBTreeList
        Left = 80
        Top = 112
        Width = 465
        Height = 113
        Bands = <
          item
            Caption.AlignHorz = taCenter
          end>
        DataController.ParentField = 'ID_PARINTE'
        DataController.KeyField = 'ID_REPARTITORI'
        LookAndFeel.Kind = lfOffice11
        Navigator.Buttons.CustomButtons = <>
        OptionsBehavior.AutoDragCopy = True
        OptionsBehavior.ConfirmDelete = False
        OptionsBehavior.DragCollapse = False
        OptionsBehavior.DragExpand = False
        OptionsBehavior.ExpandOnIncSearch = True
        OptionsBehavior.IncSearch = True
        OptionsBehavior.IncSearchItem = TreeRepartitoriNUME
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
        TabOrder = 3
        Visible = False
        OnDblClick = TreeBugeteDblClick
        OnKeyDown = TreeBugeteKeyDown
        object TreeRepartitoriNUME: TcxDBTreeListColumn
          Caption.Text = 'Denumire'
          DataBinding.FieldName = 'NUME'
          Width = 279
          Position.ColIndex = 0
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeRepartitoriADRESA: TcxDBTreeListColumn
          Caption.Text = 'Adresa'
          DataBinding.FieldName = 'ADRESA'
          Width = 58
          Position.ColIndex = 2
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeRepartitoriCONT: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'Cont'
          DataBinding.FieldName = 'CONT'
          Width = 55
          Position.ColIndex = 3
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeRepartitoriCODFISC: TcxDBTreeListColumn
          Caption.Text = 'CIF'
          DataBinding.FieldName = 'CODFISC'
          Width = 68
          Position.ColIndex = 1
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeRepartitoriGESTINT: TcxDBTreeListColumn
          Caption.Text = 'Este Interna'
          DataBinding.FieldName = 'GESTINT'
          Width = 58
          Position.ColIndex = 4
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
      end
      object edSumaDeFacturat: TcxCurrencyEdit
        Left = 357
        Top = 4
        EditValue = 0c
        Properties.Alignment.Horz = taRightJustify
        Properties.DisplayFormat = ',0.00;(-,0.00)'
        Properties.UseDisplayFormatWhenEditing = True
        Properties.UseLeftAlignmentOnEditing = False
        Properties.UseThousandSeparator = True
        Style.Color = clBtnFace
        Style.TextStyle = [fsBold]
        TabOrder = 4
        Visible = False
        Width = 121
      end
      object edTipProiect: TcxImageComboBox
        Left = 485
        Top = 4
        Anchors = [akLeft, akTop, akRight]
        Properties.Items = <>
        TabOrder = 5
        Visible = False
        Width = 467
      end
    end
    object tabGlobal: TcxTabSheet
      Caption = 'Angajamente Globale'
      DesignSize = (
        960
        365)
      object LbClasaFunctionala: TLabel
        Left = 16
        Top = 8
        Width = 133
        Height = 13
        Caption = 'Clasificatia Functionala'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label1: TLabel
        Left = 16
        Top = 60
        Width = 339
        Height = 13
        Caption = 'Clasificatia economica desfasurata pentru clasa functionala'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Bevel1: TBevel
        Left = 360
        Top = 66
        Width = 591
        Height = 9
        Anchors = [akLeft, akTop, akRight]
        Shape = bsTopLine
        ExplicitWidth = 583
      end
      object edClasaFunctionala: TcxRepartitorPanel
        Left = 22
        Top = 24
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
        PopupEdit.PopupFormBorderStyle = True
        PopupEdit.PopupHeight = 200
        PopupEdit.PopupMinHeight = 100
        PopupEdit.PopupMinWidth = 100
        PopupEdit.PopupSizeable = True
        PopupEdit.PopupWidth = 250
        PopupEdit.Style.Color = clWindow
        PopupEdit.Visible = True
        PopupEdit.Width = 815
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
        OnPopupInitPopup = edClasaFunctionalaPopupInitPopup
        OnValidate = edClasaFunctionalaValidate
        Height = 30
        Width = 935
      end
      object TreeChild: TcxDBTreeList
        Left = 16
        Top = 76
        Width = 935
        Height = 248
        Anchors = [akLeft, akTop, akRight, akBottom]
        Bands = <
          item
            Caption.AlignHorz = taCenter
            Options.Moving = False
          end>
        DataController.DataSource = DTEconomic
        DataController.ParentField = 'ID_PARINTE'
        DataController.KeyField = 'ID'
        LookAndFeel.Kind = lfOffice11
        Navigator.Buttons.CustomButtons = <>
        OptionsBehavior.ImmediateEditor = False
        OptionsBehavior.ConfirmDelete = False
        OptionsBehavior.DragCollapse = False
        OptionsBehavior.DragExpand = False
        OptionsBehavior.ExpandOnIncSearch = True
        OptionsBehavior.IncSearch = True
        OptionsBehavior.IncSearchItem = TreeChildClasa
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
        OptionsView.CheckGroups = True
        OptionsView.DynamicIndent = True
        OptionsView.Footer = True
        ParentColor = False
        Preview.AutoHeight = False
        Preview.MaxLineCount = 2
        RootValue = -1
        ScrollbarAnnotations.CustomAnnotations = <>
        TabOrder = 3
        OnCustomDrawDataCell = TreeChildCustomDrawDataCell
        OnDblClick = TreeChildDblClick
        OnFocusedNodeChanged = TreeChildFocusedNodeChanged
        OnGetNodeImageIndex = TreeChildGetNodeImageIndex
        OnKeyDown = TreeChildKeyDown
        OnMouseUp = TreeChildMouseUp
        object TreeChildClasa: TcxDBTreeListColumn
          PropertiesClassName = 'TcxTextEditProperties'
          Properties.Alignment.Horz = taLeftJustify
          Caption.Text = 'Clasa'
          DataBinding.FieldName = 'COD_ECONOMIC_ECRAN'
          Width = 219
          Position.ColIndex = 0
          Position.RowIndex = 0
          Position.BandIndex = 0
          SortOrder = soAscending
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
          OnGetDisplayText = TreeChildClasaGetDisplayText
        end
        object TreeChildCOD_ECONOMIC: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'Cod Econ.'
          DataBinding.FieldName = 'COD_ECONOMIC'
          Width = 100
          Position.ColIndex = 7
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeChildDENUMIRE: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'Clasa'
          DataBinding.FieldName = 'DENUMIRE'
          Width = 100
          Position.ColIndex = 8
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeChildASIGNAT: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'Asignat'
          DataBinding.FieldName = 'ASIGNAT'
          Width = 100
          Position.ColIndex = 6
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeChildPLANIFICAT: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DecimalPlaces = 0
          Properties.DisplayFormat = ',0.00 ;-,0.00 '
          Caption.Text = 'Planificat'
          DataBinding.FieldName = 'PLANIFICAT'
          Width = 75
          Position.ColIndex = 1
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeChildANGAJAT: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DecimalPlaces = 0
          Properties.DisplayFormat = ',0.00 ;-,0.00 '
          Caption.Text = 'Angajat'
          DataBinding.FieldName = 'ANGAJAT'
          Width = 76
          Position.ColIndex = 2
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeChildREALIZAT: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DecimalPlaces = 0
          Properties.DisplayFormat = ',0.00 ;-,0.00 '
          Caption.Text = 'Realizat'
          DataBinding.FieldName = 'REALIZAT'
          Width = 76
          Position.ColIndex = 3
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeChildPROC_ANGAJAT: TcxDBTreeListColumn
          Caption.Text = 'Angajat %'
          DataBinding.FieldName = 'PROC_ANGAJAT'
          Width = 76
          Position.ColIndex = 4
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeChildPROC_REALIZAT: TcxDBTreeListColumn
          Caption.Text = 'Realizat %'
          DataBinding.FieldName = 'PROC_REALIZAT'
          Width = 75
          Position.ColIndex = 5
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeChildCOD_FUNCTIONAL: TcxDBTreeListColumn
          Caption.Text = 'Cod Functional'
          DataBinding.FieldName = 'COD_FUNCTIONAL'
          Width = 100
          Position.ColIndex = 9
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeChildID_OI_UNITATI: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'IdUnitate'
          DataBinding.FieldName = 'ID_OI_UNITATI'
          Width = 100
          Position.ColIndex = 10
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeChildID_OI_PROIECTE: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'Id Proiect'
          DataBinding.FieldName = 'ID_OI_PROIECTE'
          Width = 100
          Position.ColIndex = 11
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeChildID: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'Id'
          DataBinding.FieldName = 'ID'
          Width = 100
          Position.ColIndex = 12
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeChildCOD_ECONOMIC_ECRAN: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'COD_ECONOMIC_ECRAN'
          Width = 100
          Position.ColIndex = 13
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
      end
      object ChkArataDoarPlanificat: TcxCheckBox
        Left = 16
        Top = 343
        Anchors = [akLeft, akBottom]
        Caption = 'Arata doar planificat'
        ParentFont = False
        Style.Font.Charset = DEFAULT_CHARSET
        Style.Font.Color = clWindowText
        Style.Font.Height = -11
        Style.Font.Name = 'MS Sans Serif'
        Style.Font.Style = [fsBold]
        Style.LookAndFeel.Kind = lfOffice11
        Style.IsFontAssigned = True
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 1
        OnClick = ChkArataDoarPlanificatClick
      end
      object ChkArataNerealizat: TcxCheckBox
        Left = 164
        Top = 343
        Anchors = [akLeft, akBottom]
        Caption = 'Arata doar nerealizat'
        ParentFont = False
        Style.Font.Charset = DEFAULT_CHARSET
        Style.Font.Color = clWindowText
        Style.Font.Height = -11
        Style.Font.Name = 'MS Sans Serif'
        Style.Font.Style = [fsBold]
        Style.LookAndFeel.Kind = lfOffice11
        Style.IsFontAssigned = True
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 2
        OnClick = ChkArataNerealizatClick
      end
      object TreeBugete: TcxDBTreeList
        Left = 184
        Top = 104
        Width = 369
        Height = 150
        Bands = <
          item
            Caption.AlignHorz = taCenter
          end>
        DataController.ParentField = 'ID_PARINTE'
        DataController.KeyField = 'ID_BG_PLAN_FUNCTIONAL'
        LookAndFeel.Kind = lfOffice11
        Navigator.Buttons.CustomButtons = <>
        OptionsBehavior.ImmediateEditor = False
        OptionsBehavior.ConfirmDelete = False
        OptionsBehavior.DragCollapse = False
        OptionsBehavior.ExpandOnIncSearch = True
        OptionsBehavior.IncSearch = True
        OptionsBehavior.IncSearchItem = TreeBugeteNUMAR_RAND
        OptionsBehavior.ShowHourGlass = False
        OptionsCustomizing.BandCustomizing = False
        OptionsCustomizing.BandVertSizing = False
        OptionsCustomizing.ColumnVertSizing = False
        OptionsData.CancelOnExit = False
        OptionsData.Editing = False
        OptionsData.Deleting = False
        OptionsSelection.CellSelect = False
        OptionsSelection.HideFocusRect = False
        OptionsView.CellTextMaxLineCount = -1
        OptionsView.ShowEditButtons = ecsbFocused
        OptionsView.ColumnAutoWidth = True
        ParentColor = False
        Preview.AutoHeight = False
        Preview.MaxLineCount = 2
        RootValue = -1
        ScrollbarAnnotations.CustomAnnotations = <>
        TabOrder = 4
        Visible = False
        OnDblClick = TreeBugeteDblClick
        OnKeyDown = TreeBugeteKeyDown
        object TreeBugeteNUMAR_RAND: TcxDBTreeListColumn
          Caption.AlignHorz = taCenter
          Caption.Text = 'Buget'
          DataBinding.FieldName = 'COD_ECRAN'
          Width = 100
          Position.ColIndex = 2
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
          OnGetDisplayText = TreeBugeteNUMAR_RANDGetDisplayText
        end
        object TreeBugeteDENUMIRE: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'Denumire'
          DataBinding.FieldName = 'DENUMIRE'
          Width = 100
          Position.ColIndex = 1
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeBugeteCOD_BUGET: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'COD_FUNCTIONAL'
          Width = 100
          Position.ColIndex = 0
          Position.RowIndex = 0
          Position.BandIndex = 0
          SortOrder = soAscending
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeBugeteCOD_ECRAN: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'Cod Ecran'
          DataBinding.FieldName = 'cod_ecran'
          Options.Editing = False
          Width = 100
          Position.ColIndex = 3
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeBugeteID_ANALITIC: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'Analitic'
          DataBinding.FieldName = 'ID_ANALITIC'
          Options.Editing = False
          Width = 100
          Position.ColIndex = 4
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeBugetecxDBTreeListColumn1: TcxDBTreeListColumn
          Visible = False
          Caption.Text = 'Id'
          DataBinding.FieldName = 'ID_BG_PLAN_FUNCTIONAL'
          Options.Editing = False
          Width = 100
          Position.ColIndex = 5
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
      end
    end
    object tabOrd: TcxTabSheet
      Caption = 'Ordonantari'
      ImageIndex = 2
      DesignSize = (
        960
        365)
      object Label4: TLabel
        Left = 16
        Top = 5
        Width = 198
        Height = 13
        Caption = 'Furnizorul asociat ordonantariilor : '
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label5: TLabel
        Left = 16
        Top = 56
        Width = 296
        Height = 13
        Caption = 'Lista pozitiilor ordonantate pentru repartitorul curent'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Bevel3: TBevel
        Left = 318
        Top = 62
        Width = 633
        Height = 9
        Anchors = [akLeft, akTop, akRight]
        Shape = bsTopLine
        ExplicitWidth = 625
      end
      object edFurnizorOrd: TcxRepartitorPanel
        Left = 22
        Top = 20
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
        PopupEdit.PopupControl = TreeRepartitori
        PopupEdit.PopupFlatBorder = False
        PopupEdit.PopupFormBorderStyle = True
        PopupEdit.PopupHeight = 200
        PopupEdit.PopupMinHeight = 100
        PopupEdit.PopupMinWidth = 100
        PopupEdit.PopupSizeable = True
        PopupEdit.PopupWidth = 250
        PopupEdit.Style.Color = clWindow
        PopupEdit.Visible = True
        PopupEdit.Width = 815
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
        DataSource = frmData.DTRepartitori
        CodField = 'COD_FISCAL'
        KeyField = 'ID_REPARTITORI'
        ListField = 'NUME'
        OnPopupInitPopup = edClasaFunctionalaPopupInitPopup
        OnPopupPopup = edFurnizorOrdPopupPopup
        OnValidate = edFurnizorOrdValidate
        Height = 30
        Width = 935
      end
      object cxGridOrd: TcxGrid
        Left = 16
        Top = 76
        Width = 935
        Height = 268
        Anchors = [akLeft, akTop, akRight, akBottom]
        TabOrder = 1
        LookAndFeel.Kind = lfOffice11
        object cxGridDBTableView1: TcxGridDBTableView
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          DataController.Summary.DefaultGroupSummaryItems = <>
          DataController.Summary.FooterSummaryItems = <>
          DataController.Summary.SummaryGroups = <>
        end
        object GridOrd: TcxGridDBTableView
          OnDblClick = GridOrdDblClick
          OnKeyUp = GridOrdKeyUp
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          OnFocusedRecordChanged = GridOrdFocusedRecordChanged
          DataController.DataSource = DTOrdonantari
          DataController.Filter.MaxValueListCount = 1000
          DataController.KeyFieldNames = 'ID_ORDONANTARE_DEFALCARE'
          DataController.Summary.DefaultGroupSummaryItems = <>
          DataController.Summary.FooterSummaryItems = <>
          DataController.Summary.SummaryGroups = <>
          Filtering.ColumnPopup.MaxDropDownItemCount = 12
          OptionsBehavior.IncSearch = True
          OptionsBehavior.IncSearchItem = GridOrdNr
          OptionsBehavior.FocusCellOnCycle = True
          OptionsCustomize.ColumnsQuickCustomization = True
          OptionsData.CancelOnExit = False
          OptionsData.Deleting = False
          OptionsData.DeletingConfirmation = False
          OptionsData.Inserting = False
          OptionsSelection.HideFocusRectOnExit = False
          OptionsView.ColumnAutoWidth = True
          OptionsView.Footer = True
          OptionsView.GridLines = glNone
          OptionsView.GroupByBox = False
          OptionsView.GroupFooters = gfVisibleWhenExpanded
          OptionsView.Indicator = True
          Preview.AutoHeight = False
          Preview.MaxLineCount = 2
          object GridOrdSel: TcxGridDBColumn
            Caption = 'Sel'
            DataBinding.FieldName = 'SEL'
            PropertiesClassName = 'TcxCheckBoxProperties'
            Properties.ImmediatePost = True
            Visible = False
            Options.ShowEditButtons = isebAlways
            Width = 43
          end
          object GridOrdIdAng: TcxGridDBColumn
            Caption = 'IdAng'
            DataBinding.FieldName = 'ID_ANGAJAMENTE_DEFALCARE'
            Visible = False
            Options.Editing = False
          end
          object GridOrdNr: TcxGridDBColumn
            Caption = 'Nr'
            DataBinding.FieldName = 'NUMAR'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 30
          end
          object GridOrdData: TcxGridDBColumn
            Caption = 'Data'
            DataBinding.FieldName = 'DATA_EMITERE'
            PropertiesClassName = 'TcxDateEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.DateButtons = [btnClear, btnToday]
            Properties.DateOnError = deToday
            Properties.InputKind = ikRegExpr
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 61
          end
          object GridOrdCOD_FUNCTIONAL: TcxGridDBColumn
            Caption = 'Cap.'
            DataBinding.FieldName = 'COD_FUNCTIONAL'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 64
          end
          object GridOrdCOD_ECONOMIC: TcxGridDBColumn
            Caption = 'Titlu/Art'
            DataBinding.FieldName = 'COD_ECONOMIC'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 67
          end
          object GridOrdDOCUMENTE: TcxGridDBColumn
            Caption = 'Descriere'
            DataBinding.FieldName = 'DOCUMENTE'
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 151
          end
          object GridOrdAngajat: TcxGridDBColumn
            Caption = 'Ordonatat'
            DataBinding.FieldName = 'ANGAJAT'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Properties.Nullable = False
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 65
          end
          object GridOrdOrdonantat: TcxGridDBColumn
            Caption = 'Platit'
            DataBinding.FieldName = 'ordonantat'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 59
          end
          object GridOrdRamas: TcxGridDBColumn
            Caption = 'De platit'
            DataBinding.FieldName = 'RAMAS_DE_ORDONANTAT'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00 ;-,0.00 '
            Properties.ReadOnly = True
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 61
          end
          object GridOrdProcent: TcxGridDBColumn
            Caption = 'Proc. %'
            DataBinding.FieldName = 'PROC_ORDONANTAT'
            PropertiesClassName = 'TcxProgressBarProperties'
            Properties.PeakValue = 94.020000000000000000
            Options.Editing = False
            Width = 49
          end
          object GridOrdIdOrd: TcxGridDBColumn
            DataBinding.FieldName = 'ID_ORDONANTARE_DEFALCARE'
            Visible = False
            Options.Editing = False
          end
          object GridOrdIdProiect: TcxGridDBColumn
            Caption = 'IdProiect'
            DataBinding.FieldName = 'id_oi_proiecte'
            Visible = False
            Options.Editing = False
          end
          object GridOrdIdUnitate: TcxGridDBColumn
            Caption = 'IdUnitate'
            DataBinding.FieldName = 'id_oi_unitati'
            Visible = False
            Options.Editing = False
          end
          object GridOrdContract_NR: TcxGridDBColumn
            Caption = 'Nr Contract'
            DataBinding.FieldName = 'nr_contract'
            Width = 36
          end
          object GridOrdContract_DATA: TcxGridDBColumn
            Caption = 'Data Contract'
            DataBinding.FieldName = 'data_contract'
          end
        end
        object GridOrdL: TcxGridLevel
          GridView = GridOrd
        end
      end
      object chkOrdNeplatit: TcxCheckBox
        Left = 16
        Top = 343
        Anchors = [akLeft, akBottom]
        Caption = 'Arata doar pozitiile neplatite'
        ParentFont = False
        State = cbsChecked
        Style.Font.Charset = DEFAULT_CHARSET
        Style.Font.Color = clWindowText
        Style.Font.Height = -11
        Style.Font.Name = 'MS Sans Serif'
        Style.Font.Style = [fsBold]
        Style.LookAndFeel.Kind = lfOffice11
        Style.IsFontAssigned = True
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 2
        OnClick = chkOrdNeplatitClick
      end
    end
  end
  object BtnOk: TcxButton
    Left = 752
    Top = 399
    Width = 80
    Height = 29
    Anchors = [akRight, akBottom]
    Caption = 'Ok'
    LookAndFeel.Kind = lfOffice11
    OptionsImage.Glyph.SourceDPI = 96
    OptionsImage.Glyph.Data = {
      424D360900000000000036000000280000001800000018000000010020000000
      000000000000C40E0000C40E0000000000000000000000000000000000000000
      000000000000000000000000000000000000000000010000000E000000270001
      004400010053000100550000004B000000300000001600000005000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000001000000120105015806300BB70D6015E80F77
      19FA107C1CFD107C1CFD0F7119F70B5012E1052208B7000200740000002B0000
      0004000000000000000000000000000000000000000000000000000000000000
      000000000000000000030104014009420FC3107D1BFD21932CFF25A732FF28B3
      35FF23B630FF26B634FF2CB038FF29A235FF1C8A27FF0F7719FA07320BC90000
      0064000000110000000000000000000000000000000000000000000000000000
      000000000004010B025B0D6A17EC269331FF3DB949FF36C344FF2AC038FF20BF
      2FFF1DBE2DFF22BF32FF2BC03AFF38C245FF46C552FF42B14DFF1D8A28FF0C5B
      15E90005017F0000001600000000000000000000000000000000000000000000
      0002020C0358117C1CFB42AB4CFF51C65CFF42C34EFF34C242FF24A62FFF418B
      47FF1DBF2DFF20BF30FF2AC039FF36C244FF43C450FF52C55DFF60C66AFF3298
      3CFF0F7219F60005017E0000000F000000000000000000000000000000000105
      01360E7319F24DAE57FF5FC76AFF51C55DFF43C44FFF2FA73AFF7C927EFFB9B9
      B8FF2FA539FF26BF35FF2DC03CFF38C246FF44C450FF51C65CFF5FC76AFF6ECA
      77FF37993FFF0D5C14E900010060000000030000000000000000000000030A47
      10BD389A41FF6FCA79FF61C76BFF54C65FFF3DA947FF7C927EFFD7D6D6FFF4F5
      F5FF95AC97FF2AB237FF34C242FF3DC34AFF47C453FF53C65EFF5FC769FF6BC8
      74FF77C97FFF238B2DFF07330CC60000002600000000000000000108023F1985
      24FF7CCA83FF71C97AFF64C86EFF46984DFF8D978EFFDADADAFFFFFFFFFFFFFF
      FFFFE8E8E9FF799C7BFF3CC049FF44C450FF4CC558FF57C661FF61C76BFF6BC8
      74FF76CA7EFF62B76AFF0F781AFB0003016D000000030000000109420FB149A3
      52FF7DCC86FF6BBE75FF558258FFA5A8A5FFE2E2E2FFFFFFFFFFE8ECE9FFDCE9
      DDFFFFFFFFFFDCDCDCFF5F9564FF4CC458FF52C55DFF59C664FF62C76CFF6BC8
      74FF74CA7DFF7FCC86FF288E32FF062709B7000000130001000B0E7419F16EBB
      75FF71B277FF7F8C7FFFCDCECEFFEEEEEEFFFFFFFFFFFDFDFEFF96C99AFF78C4
      7FFFFEFDFEFFFDFDFDFFCCCDCCFF5B9C61FF59C764FF5DC668FF63C76DFF6BC8
      74FF73CA7BFF7ACA82FF4FAA59FF0C5814E40000002C020F033D10801CFF81CB
      89FF79CB81FF7DBF84FFD5DBD6FFFDFDFDFFFFFFFFFFDBE1DBFF57C461FF55C5
      61FFD1DFD2FFFFFFFFFFFDFDFDFFC4C5C4FF5C9560FF62C86EFF66C870FF6AC8
      74FF71C97AFF76CA7FFF65BB6DFF0F781AFB0001004803170557148220FF7CCC
      85FF73CA7BFF6AC873FF65C66EFF90C696FFE8E7E8FF8FC795FF61C76BFF64C7
      6EFF81C488FFFCFAFCFFFFFFFFFFF9F9F9FFC3C4C3FF619165FF67C671FF6AC8
      74FF6EC977FF71CA7AFF6EC577FF0F7F1AFF0104015803170559158220FF75CB
      7EFF6CC876FF68C871FF64C76DFF62C76CFF6EC476FF6AC773FF6CC875FF71C9
      79FF74C97CFFB9D2BBFFFFFFFFFFFFFFFFFFFAFAFAFFCACACAFF729374FF5AAB
      63FF6AC874FF6CC876FF6CC575FF0F7F1AFF010401580313044C13811EFF6FCA
      78FF68C871FF65C76EFF63C76EFF67C870FF6CC975FF74CA7CFF7CCA83FF82CB
      88FF86CC8CFF87C88DFFE6E7E5FFFFFFFFFFFFFFFFFFFEFEFEFFDADBDAFFA8AC
      A8FF65B86DFF6AC874FF66C26FFF0E7E1AFE000301450108021B0F7C1BFB62C0
      6BFF64C76EFF62C76CFF65C76FFF6CC975FF76CA7EFF82CB88FF8ACC91FF92CE
      98FF96CE9CFF97CE9DFF9DC8A1FFF4F3F4FFFFFFFFFFFFFFFFFFFFFFFFFFE0E4
      E0FF68C771FF68C871FF56B460FF0E6D18F000000020000000010D6115D049AB
      52FF61C76BFF62C76CFF69C872FF73CA7CFF81CB87FF8DCD94FF99CE9FFFA1CF
      A6FFA6D0AAFFA6D0ABFFA3D0A8FFB4CCB7FFFAF9F9FFFFFFFFFFFFFFFFFFCDDD
      CFFF6BC974FF6AC873FF3FA149FF083E0EC00000000B0000000005260867258D
      30FF68CA72FF63C76DFF6DC876FF7BCA82FF8BCD91FF9BCEA0FFA8D0ACFFB2D2
      B5FFB6D2B8FFB5D2B8FFB1D2B4FFA8D1ACFFB3C9B6FFFBFAFBFFFFFFFFFFCCDD
      CDFF6DC976FF6BC675FF14811EFF020C03610000000100000000000301090E70
      18E44CAD55FF65C86EFF71C979FF81CB88FF93CE99FFA5D0A9FFB4D2B7FFBFD3
      C1FFC3D4C4FFC1D4C2FFBBD2BDFFB1D2B4FFA5D0A9FFACC9AEFFF2F1F1FFD9E3
      DAFF6DC877FF3EA148FF0C5713D50000000F0000000000000000000000000526
      0962168321FF63C06CFF73C97BFF84CC8BFF98CE9DFFABD1AFFFBBD3BEFFC9D4
      C9FFCED5CEFFC9D4CAFFC0D3C2FFB5D2B8FFA8D0ACFF99CF9FFF99C79EFFC5D1
      C6FF5AB864FF11791BF7010A024D000000010000000000000000000000000000
      000109430F94168221FF62C16CFF7FCB87FF99CE9FFFADD1B0FFBED3C0FFCCD5
      CCFFD2D6D3FFCBD5CCFFC1D4C2FFB4D2B7FFA7D0ABFF99CE9FFF7DCB85FF5EBD
      68FF13801EFF041C067900000003000000000000000000000000000000000000
      00000001000409440F9A178321FF4CAF55FF78CB80FF97CE9CFFB9D2BDFFC6D4
      C7FFC9D4CAFFC5D4C7FFBDD3BFFFADD1B1FF8CCD93FF73CB7CFF46AA50FF1381
      1EFE0420077C0000000300000000000000000000000000000000000000000000
      00000000000000010002062D0A700F7619EF299132FF4CB056FF69C771FF7DCD
      85FF86CD8DFF84CD8CFF79CC82FF64C46EFF49AE53FF248E2FFF0D6A17E00212
      0455000000010000000000000000000000000000000000000000000000000000
      000000000000000000000000000001080213083D0D880F7119E9107E1CFF1B86
      25FF208B2BFF208B2BFF198624FF107E1BFD0E6716DA052709700000000A0000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000030108041D07470839
      0D9209410EA009420EA00526097B020C022D0000000100000000000000000000
      00000000000000000000000000000000000000000000}
    TabOrder = 1
    OnClick = BtnOkClick
  end
  object BtnCancel: TcxButton
    Left = 838
    Top = 399
    Width = 80
    Height = 29
    Anchors = [akRight, akBottom]
    Caption = 'Abandon'
    LookAndFeel.Kind = lfOffice11
    ModalResult = 2
    OptionsImage.Glyph.SourceDPI = 96
    OptionsImage.Glyph.Data = {
      424D360900000000000036000000280000001800000018000000010020000000
      000000000000C40E0000C40E0000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000707193610103D821717
      56B91E1E6EEC1E1E71F21B1B64D61313489A0C0C2E620101050A000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000202070F0D0D316819195ECA1F1F7AFE18188DFF1212
      A4FF0F0FADFF0E0EB0FF1010A8FF141499FF1C1C82FF1E1E70F01212428D0505
      1125000000000000000000000000000000000000000000000000000000000000
      000000000000070719361B1B66DB1F1F79FF1212A0FF0303D2FF0000E4FF0000
      EAFF0000EDFF0000EDFF0000ECFF0000E8FF0000E0FF0B0BB7FF1B1B86FF2020
      76FC11113F880101020400000000000000000000000000000000000000000000
      0000090922491F1F72F513139AFF0202D2FF0000E3FF0000EEFF0000F5FF0000
      F8FF0000FAFF0000FAFF0000F9FF0000F7FF0000F2FF0000E9FF0000DAFF0B0B
      B1FF1D1D7DFF161654B301010205000000000000000000000000000000000707
      1A371E1E71F31010A1FF0000DAFF0000E7FF0000F0FF0000F7FF0000FBFF0000
      FDFF0000FDFF0000FDFF0000FDFF0000FCFF0000F9FF0000F3FF0000EAFF0000
      DFFF0404C6FF1C1C7FFF15154DA4000000000000000000000000000000001818
      5AC0191989FF0000D0FF0000E3FF0000EDFF0000F4FF0000F9FF0000FDFF0000
      FEFF0000FFFF0000FFFF0000FEFF0000FEFF0000FBFF0000F7FF0000EFFF0000
      E8FF0000DAFF0909B2FF202077FF07071A37000000000000000008081D3E2020
      77FE0505B8FF0000DAFF0000E8FF0000F0FF0000F5FF0000FAFF0000FDFF0000
      FEFF0000FFFF0000FFFF0000FFFF0000FEFF0000FCFF0000F8FF0000F2FF0000
      EBFF0000E2FF0000CFFF17178CFF171756B9000000000000000018185AC01313
      93FF0000CBFF0000DEFF0000E9FF0000F0FF0000F6FF0000FAFF0000FDFF0000
      FFFF0000FFFF0000FFFF0000FFFF0000FEFF0000FCFF0000F8FF0000F2FF0000
      ECFF0000E4FF0000D6FF0505B6FF202078FF07071B3904041022202076FD0606
      AEFF0000CFFF0000DEFF0000E8FF0000EFFF0000F5FF0000F9FF0000FDFF0000
      FEFF0000FFFF0000FFFF0000FFFF0000FEFF0000FBFF0000F7FF0000F1FF0000
      EBFF0000E4FF0000D8FF0000C4FF181887FF1111408A10103A7D1E1E7AFF0000
      BAFF0000D0FF4040C1FF5858BBFF5757BDFF5757C0FF5757C2FF5757C4FF5757
      C5FF5757C5FF5757C5FF5757C5FF5757C4FF5757C3FF5757C1FF5757BEFF5757
      BCFF5555B8FF0000CFFF0000C5FF0F0F96FF19195CC61A1A61CF191983FF0000
      BAFF0000CDFFA7A7DCFFEFEFEFFFEDEDEDFFEDEDEDFFEDEDEDFFEDEDEDFFEDED
      EDFFEDEDEDFFEDEDEDFFEDEDEDFFEDEDEDFFEDEDEDFFEDEDEDFFEDEDEDFFECEC
      ECFFCECECEFF0101C2FF0000C4FF0A0A9FFF1E1E6EEC1E1E70F0171785FF0000
      B9FF0000CAFFAAAADEFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
      FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFDFD
      FDFFD7D7D7FF0101BFFF0000C2FF0707A2FF1F1F74F91E1E6EEC171784FF0000
      B4FF0000C5FFAAAADDFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
      FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFDFD
      FDFFD7D7D7FF0101BAFF0000BDFF07079FFF1F1F74F8171756B81A1A80FF0000
      AFFF0000BFFFAAAADBFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
      FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
      FFFFE6E6E6FF0101B4FF0000B7FF0B0B96FF1C1C69E20D0D2F641F1F78FF0101
      A7FF0000B8FF3E3EC1FF6363CCFF5E5ECEFF5959D0FF5858D3FF5858D6FF5858
      D7FF5858D8FF5858D8FF5858D8FF5858D7FF5858D4FF5959D1FF5C5CCFFF6262
      CDFF6060CAFF0101B8FF0000B1FF12128BFF171755B70202070E1F1F73F60A0A
      96FF0000B0FF0C0CBDFF2525CAFF1818CBFF0909CCFF0202CFFF0000D3FF0000
      D6FF0000D7FF0000D8FF0000D7FF0000D4FF0101D1FF0505CEFF1313CBFF2323
      CBFF1B1BC4FF0000B6FF0000A8FF1B1B7DFF0F0F3775000000001414499C1919
      80FF0000A7FF0B0BB5FF3B3BC9FF3737CBFF1F1FC8FF0C0CC7FF0303C9FF0101
      CBFF0000CCFF0000CCFF0000CBFF0101CAFF0707C8FF1616C8FF3030CBFF3F3F
      CCFF2020BFFF0000ADFF090996FF1F1F75FA04040E1D000000000606152C2020
      77FF080896FF0303ACFF4242C7FF5858D0FF4141CCFF2323C7FF1010C4FF0606
      C3FF0303C4FF0202C4FF0404C4FF0B0BC4FF1919C5FF3535CAFF5353D0FF5151
      CDFF1515B6FF0000A5FF191980FF15154DA40000000000000000000000001515
      50AB1A1A7DFF0202A0FF2121B7FF6B6BD2FF7575D7FF5C5CD1FF3F3FCBFF2A2A
      C6FF2020C4FF2020C4FF2525C5FF3535C8FF4F4FCEFF6D6DD5FF7171D5FF3939
      C0FF0202A8FF0D0D8EFF1F1F74F9050513280000000000000000000000000303
      09141A1A62D2171781FF03039FFF2A2AB6FF7F7FD6FF9F9FE2FF9999E0FF8989
      DCFF7F7FD9FF7E7ED9FF8484DAFF9292DFFF9F9FE2FF9595DDFF4545C1FF0505
      A5FF0B0B91FF1F1F77FD0C0C2E62000000000000000000000000000000000000
      000004040D1C1A1A62D11B1B7DFF0A0A91FF1C1CABFF7777CFFFB6B6E7FFCCCC
      EFFFD2D2F1FFD2D2F1FFCFCFF0FFC2C2EBFF9898DCFF3E3EBAFF080899FF1515
      84FF1F1F75FB0E0E326C00000000000000000000000000000000000000000000
      0000000000000202060C14144A9E202077FE1B1B7CFF0F0F8EFF2626A8FF5757
      BFFF7777CDFF7C7CCEFF6767C6FF3A3AB3FF111199FF151583FF202077FF1B1B
      63D408081E400000000000000000000000000000000000000000000000000000
      000000000000000000000000000003030D1B10103A7C1C1C6AE3202077FF1B1B
      7CFF18187EFF18187FFF19197DFF1E1E79FF202076FC161652AF08081D3F0000
      0001000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000010104080A0A254F1414
      4BA01D1D6BE61E1E6FED19195ECA0E0E36730505112400000000000000000000
      00000000000000000000000000000000000000000000}
    TabOrder = 2
    OnClick = BtnCancelClick
  end
  object cxButton1: TcxButton
    Left = 656
    Top = 168
    Width = 75
    Height = 25
    Caption = 'cxButton1'
    TabOrder = 3
    Visible = False
    OnClick = cxButton1Click
  end
  object DTAngajamente: TDataSource
    DataSet = QryAngajamente
    Left = 24
    Top = 105
  end
  object QryAngajamente: TZReadOnlyQuery
    Connection = frmData.dbContabilitate
    AfterOpen = QryAngajamenteAfterOpen
    SQL.Strings = (
      'EXEC spAlopExecutieAng :ID_REPARTITOR, :DATA, :ID_CULGEST_ITEMSI')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_REPARTITOR'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftInteger
        Name = 'ID_CULGEST_ITEMSI'
        ParamType = ptInput
      end>
    Left = 112
    Top = 105
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_REPARTITOR'
        ParamType = ptUnknown
        Size = 4
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftInteger
        Name = 'ID_CULGEST_ITEMSI'
        ParamType = ptInput
      end>
  end
  object DTEconomic: TDataSource
    DataSet = QryEconomic
    Left = 24
    Top = 161
  end
  object QryEconomic: TZReadOnlyQuery
    Connection = frmData.dbContabilitate
    AfterOpen = QryEconomicAfterOpen
    SQL.Strings = (
      
        'EXEC spAlopExecutieGlobal :COD_BUGET, '#39#39', :DATA, 1, 1,NULL, :ID_' +
        'ANALITIC')
    Params = <
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'COD_BUGET'
        ParamType = ptUnknown
        Size = 128
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_ANALITIC'
        ParamType = ptUnknown
        Size = 4
      end>
    Left = 112
    Top = 161
    ParamData = <
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'COD_BUGET'
        ParamType = ptUnknown
        Size = 128
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA'
        ParamType = ptUnknown
        Size = 16
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_ANALITIC'
        ParamType = ptUnknown
        Size = 4
      end>
  end
  object DTOrdonantari: TDataSource
    DataSet = qryOrdonantari
    Left = 24
    Top = 225
  end
  object qryOrdonantari: TZReadOnlyQuery
    Connection = frmData.dbContabilitate
    AfterOpen = qryOrdonantariAfterOpen
    SQL.Strings = (
      'EXEC spAlopExecutieOrd :ID_REPARTITOR, :DATA')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_REPARTITOR'
        ParamType = ptInput
        Size = 4
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA'
        ParamType = ptInput
        Size = 16
      end>
    Left = 112
    Top = 217
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_REPARTITOR'
        ParamType = ptInput
        Size = 4
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA'
        ParamType = ptInput
        Size = 16
      end>
  end
  object dtRepartitori: TDataSource
    DataSet = qryRepartitori
    Left = 24
    Top = 280
  end
  object qryRepartitori: TZReadOnlyQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      #39'exec [spGetUserRepartitori] :userID, 4, '#39'1'#39)
    Params = <
      item
        DataType = ftInteger
        Name = 'userID'
        ParamType = ptInput
      end>
    Left = 112
    Top = 280
    ParamData = <
      item
        DataType = ftInteger
        Name = 'userID'
        ParamType = ptInput
      end>
  end
end
