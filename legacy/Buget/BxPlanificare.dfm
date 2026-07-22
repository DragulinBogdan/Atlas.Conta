object frmBxPlanificare: TfrmBxPlanificare
  Left = 276
  Top = 143
  Caption = 'Planificare buget'
  ClientHeight = 687
  ClientWidth = 1030
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poDefault
  OnClose = FormClose
  OnCloseQuery = FormCloseQuery
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  OnShow = FormShow
  PixelsPerInch = 96
  TextHeight = 13
  object pnAllForm: TPanel
    Left = 0
    Top = 0
    Width = 1030
    Height = 687
    Align = alClient
    BevelOuter = bvNone
    TabOrder = 0
    object Panel2: TPanel
      Left = 0
      Top = 0
      Width = 1030
      Height = 595
      Align = alClient
      TabOrder = 1
      ExplicitHeight = 609
      object Panel4: TPanel
        Left = 1
        Top = 1
        Width = 703
        Height = 593
        Align = alClient
        Color = clWhite
        TabOrder = 0
        ExplicitHeight = 607
        object Panel5: TPanel
          Left = 1
          Top = 1
          Width = 701
          Height = 24
          Align = alTop
          AutoSize = True
          BevelOuter = bvLowered
          TabOrder = 0
          object SelectDefalcare: TcxTabControl
            Left = 1
            Top = 1
            Width = 699
            Height = 22
            Align = alTop
            TabOrder = 0
            Properties.CustomButtons.Buttons = <>
            Properties.Style = 9
            Properties.TabIndex = 0
            Properties.Tabs.Strings = (
              'Buget General'
              'Buget Propriu'
              'Institutii/Directii/Subunitati'
              'Proiecte/Investitii/Achizitii'
              'Economic')
            LookAndFeel.Kind = lfOffice11
            OnChange = SelectDefalcareChange
            ClientRectRight = 0
            ClientRectTop = 0
          end
        end
        object cxTreeClasificEco: TcxDBTreeList
          Left = 1
          Top = 25
          Width = 701
          Height = 567
          Align = alClient
          Bands = <
            item
              Caption.AlignHorz = taCenter
              Caption.Text = 'Clasificatie'
              Width = 276
            end
            item
              Caption.AlignHorz = taCenter
              Caption.Text = 'Bugetar'
              Width = 120
            end
            item
              Caption.AlignHorz = taCenter
              Caption.Text = 'Credit de angajament'
              Width = 100
            end
            item
              Caption.AlignHorz = taCenter
              Caption.Text = 'Estimari'
              Width = 111
            end
            item
              Caption.AlignHorz = taCenter
              Caption.Text = 'Buget Luna'
              Visible = False
            end>
          DataController.DataSource = DTClasEconomica
          DataController.ParentField = 'ID_PARINTE'
          DataController.KeyField = 'ID'
          LookAndFeel.Kind = lfOffice11
          Navigator.Buttons.CustomButtons = <>
          OptionsBehavior.GoToNextCellOnEnter = True
          OptionsBehavior.GoToNextCellOnTab = True
          OptionsBehavior.ConfirmDelete = False
          OptionsBehavior.ExpandOnIncSearch = True
          OptionsBehavior.IncSearch = True
          OptionsBehavior.IncSearchItem = cxTreeClasificEcoCOD_BUGET
          OptionsBehavior.ShowHourGlass = False
          OptionsCustomizing.BandCustomizing = False
          OptionsCustomizing.ColumnsQuickCustomization = True
          OptionsCustomizing.StackedColumns = False
          OptionsData.Deleting = False
          OptionsSelection.InvertSelect = False
          OptionsView.Bands = True
          OptionsView.ColumnAutoWidth = True
          OptionsView.GridLines = tlglBoth
          OptionsView.Indicator = True
          ParentColor = False
          PopupMenus.ColumnHeaderMenu.UseBuiltInMenu = True
          PopupMenus.FooterMenu.UseBuiltInMenu = True
          PopupMenus.GroupFooterMenu.UseBuiltInMenu = True
          Preview.MaxLineCount = 0
          RootValue = -1
          ScrollbarAnnotations.CustomAnnotations = <>
          TabOrder = 1
          OnCustomDrawDataCell = cxTreeClasificEcoCustomDrawDataCell
          OnEnter = cxTreeClasificEcoEnter
          OnFocusedColumnChanged = cxTreeClasificEcoFocusedColumnChanged
          OnFocusedNodeChanged = cxTreeClasificEcoFocusedNodeChanged
          OnMouseMove = cxTreeClasificEcoMouseMove
          ExplicitHeight = 581
          object cxTreeClasificEcoCOD_BUGET: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Caption.Text = 'Art./Alin.'
            DataBinding.FieldName = 'COD_BUGET'
            Options.Editing = False
            Width = 52
            Position.ColIndex = 0
            Position.RowIndex = 0
            Position.BandIndex = 0
            SortOrder = soAscending
            SortIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
            OnGetDisplayText = cxTreeClasificEcoCOD_BUGETGetDisplayText
          end
          object cxTreeClasificEcoDENUMIRE: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Caption.Text = 'Denumire'
            DataBinding.FieldName = 'DENUMIRE'
            Options.Editing = False
            Width = 100
            Position.ColIndex = 3
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoREALIZAT: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.Text = 'Realizat'
            DataBinding.FieldName = 'REALIZAT'
            Width = 49
            Position.ColIndex = 0
            Position.RowIndex = 0
            Position.BandIndex = 3
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLANIFICAT: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.Text = 'Planificat'
            DataBinding.FieldName = 'PLANIFICAT'
            Options.Editing = False
            Width = 42
            Position.ColIndex = 0
            Position.RowIndex = 0
            Position.BandIndex = 1
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLANIFICAT_REST: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.Text = 'Restante'
            DataBinding.FieldName = 'PLANIFICAT_REST'
            Width = 38
            Position.ColIndex = 1
            Position.RowIndex = 0
            Position.BandIndex = 1
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLANIFICAT1: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.Text = 'Plan. Trim I'
            DataBinding.FieldName = 'PLANIFICAT1'
            Width = 33
            Position.ColIndex = 2
            Position.RowIndex = 0
            Position.BandIndex = 1
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLANIFICAT2: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.Text = 'Plan. Trim II'
            DataBinding.FieldName = 'PLANIFICAT2'
            Width = 28
            Position.ColIndex = 4
            Position.RowIndex = 0
            Position.BandIndex = 1
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLANIFICAT3: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.Text = 'Plan. Trim III'
            DataBinding.FieldName = 'PLANIFICAT3'
            Width = 44
            Position.ColIndex = 5
            Position.RowIndex = 0
            Position.BandIndex = 1
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLANIFICAT4: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.Text = 'Plan. Trim IV'
            DataBinding.FieldName = 'PLANIFICAT4'
            Width = 22
            Position.ColIndex = 6
            Position.RowIndex = 0
            Position.BandIndex = 1
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLUS1AN: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.Text = 'An .'
            DataBinding.FieldName = 'PLUS1AN'
            Width = 25
            Position.ColIndex = 1
            Position.RowIndex = 0
            Position.BandIndex = 3
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLUS2AN: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.Text = 'An .'
            DataBinding.FieldName = 'PLUS2AN'
            Width = 62
            Position.ColIndex = 2
            Position.RowIndex = 0
            Position.BandIndex = 3
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLUS3AN: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.Text = 'An .'
            DataBinding.FieldName = 'PLUS3AN'
            Width = 55
            Position.ColIndex = 3
            Position.RowIndex = 0
            Position.BandIndex = 3
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLUS4AN: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.Text = 'An .'
            DataBinding.FieldName = 'PLUS4AN'
            Width = 54
            Position.ColIndex = 4
            Position.RowIndex = 0
            Position.BandIndex = 3
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoCA: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.Text = 'CA An'
            DataBinding.FieldName = 'CA'
            Options.Editing = False
            Width = 21
            Position.ColIndex = 0
            Position.RowIndex = 0
            Position.BandIndex = 2
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoCA1: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.Text = 'CA. Trim I'
            DataBinding.FieldName = 'CA1'
            Width = 20
            Position.ColIndex = 1
            Position.RowIndex = 0
            Position.BandIndex = 2
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoCA2: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.Text = 'CA. Trim II'
            DataBinding.FieldName = 'CA2'
            Width = 21
            Position.ColIndex = 2
            Position.RowIndex = 0
            Position.BandIndex = 2
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoCA3: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.Text = 'CA. Trim III'
            DataBinding.FieldName = 'CA3'
            Width = 20
            Position.ColIndex = 3
            Position.RowIndex = 0
            Position.BandIndex = 2
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoCA4: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.Text = 'CA. Trim IV'
            DataBinding.FieldName = 'CA4'
            Width = 21
            Position.ColIndex = 4
            Position.RowIndex = 0
            Position.BandIndex = 2
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoAN_FISCAL: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Caption.Text = 'An Fiscal'
            DataBinding.FieldName = 'AN_FISCAL'
            Options.Editing = False
            Width = 100
            Position.ColIndex = 3
            Position.RowIndex = 0
            Position.BandIndex = 1
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoREVIZIE: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Caption.Text = 'Revizie'
            DataBinding.FieldName = 'REVIZIE'
            Options.Editing = False
            Width = 100
            Position.ColIndex = 2
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoINTRODUS: TcxDBTreeListColumn
            PropertiesClassName = 'TcxMaskEditProperties'
            Properties.Alignment.Horz = taLeftJustify
            Properties.MaxLength = 0
            Properties.ReadOnly = True
            Visible = False
            Caption.Text = 'Introdus'
            DataBinding.FieldName = 'INTRODUS'
            Options.Editing = False
            Width = 100
            Position.ColIndex = 1
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoINTRODUCERE_ESTIMARE: TcxDBTreeListColumn
            Visible = False
            Caption.Text = 'Introducere Estimat'
            DataBinding.FieldName = 'INTRODUCERE_ESTIMARE'
            Width = 100
            Position.ColIndex = 5
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoINTRODUCERE_CA: TcxDBTreeListColumn
            Visible = False
            Caption.Text = 'Introducere CA'
            DataBinding.FieldName = 'INTRODUCERE_CA'
            Width = 100
            Position.ColIndex = 4
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLAN1: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.AlignHorz = taCenter
            Caption.Text = 'Ian'
            DataBinding.FieldName = 'PLAN1'
            Width = 33
            Position.ColIndex = 0
            Position.RowIndex = 0
            Position.BandIndex = 4
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLAN2: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.AlignHorz = taCenter
            Caption.Text = 'Feb'
            DataBinding.FieldName = 'PLAN2'
            Width = 33
            Position.ColIndex = 1
            Position.RowIndex = 0
            Position.BandIndex = 4
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLAN3: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.AlignHorz = taCenter
            Caption.Text = 'Mar'
            DataBinding.FieldName = 'PLAN3'
            Width = 33
            Position.ColIndex = 2
            Position.RowIndex = 0
            Position.BandIndex = 4
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLAN4: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.AlignHorz = taCenter
            Caption.Text = 'Apr'
            DataBinding.FieldName = 'PLAN4'
            Width = 33
            Position.ColIndex = 3
            Position.RowIndex = 0
            Position.BandIndex = 4
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLAN5: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.AlignHorz = taCenter
            Caption.Text = 'Mai'
            DataBinding.FieldName = 'PLAN5'
            Width = 33
            Position.ColIndex = 4
            Position.RowIndex = 0
            Position.BandIndex = 4
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLAN6: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.AlignHorz = taCenter
            Caption.Text = 'Iun'
            DataBinding.FieldName = 'PLAN6'
            Width = 33
            Position.ColIndex = 5
            Position.RowIndex = 0
            Position.BandIndex = 4
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLAN7: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.AlignHorz = taCenter
            Caption.Text = 'Iul'
            DataBinding.FieldName = 'PLAN7'
            Width = 33
            Position.ColIndex = 6
            Position.RowIndex = 0
            Position.BandIndex = 4
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLAN8: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.AlignHorz = taCenter
            Caption.Text = 'Aug'
            DataBinding.FieldName = 'PLAN8'
            Width = 33
            Position.ColIndex = 7
            Position.RowIndex = 0
            Position.BandIndex = 4
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLAN9: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.AlignHorz = taCenter
            Caption.Text = 'Sep'
            DataBinding.FieldName = 'PLAN9'
            Width = 33
            Position.ColIndex = 8
            Position.RowIndex = 0
            Position.BandIndex = 4
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLAN10: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.AlignHorz = taCenter
            Caption.Text = 'Oct'
            DataBinding.FieldName = 'PLAN10'
            Width = 33
            Position.ColIndex = 9
            Position.RowIndex = 0
            Position.BandIndex = 4
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLAN11: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.AlignHorz = taCenter
            Caption.Text = 'Noi'
            DataBinding.FieldName = 'PLAN11'
            Width = 33
            Position.ColIndex = 10
            Position.RowIndex = 0
            Position.BandIndex = 4
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeClasificEcoPLAN12: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.Alignment.Horz = taRightJustify
            Properties.AssignedValues.MaxValue = True
            Properties.AssignedValues.MinValue = True
            Properties.DecimalPlaces = 2
            Properties.DisplayFormat = ',0.00;-,0.00'
            Caption.AlignHorz = taCenter
            Caption.Text = 'Dec'
            DataBinding.FieldName = 'PLAN12'
            Width = 33
            Position.ColIndex = 11
            Position.RowIndex = 0
            Position.BandIndex = 4
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
        end
      end
      object cxSplitter1: TcxSplitter
        Left = 704
        Top = 1
        Width = 8
        Height = 593
        HotZoneClassName = 'TcxXPTaskBarStyle'
        AlignSplitter = salRight
        Control = pnNavPanel
        ExplicitHeight = 607
      end
      object pnNavPanel: TPanel
        Left = 712
        Top = 1
        Width = 317
        Height = 593
        Align = alRight
        BevelOuter = bvNone
        TabOrder = 2
        ExplicitHeight = 607
        object NavPanel: TdxNavBar
          Left = 0
          Top = 20
          Width = 317
          Height = 573
          Align = alClient
          ActiveGroupIndex = 0
          TabOrder = 0
          View = 14
          ExplicitHeight = 587
          object NavPanelGroup1: TdxNavBarGroup
            Caption = 'Detalii Completare'
            SelectedLinkIndex = -1
            TopVisibleLinkIndex = 0
            OptionsGroupControl.AllowControlResizing = True
            OptionsGroupControl.ShowControl = True
            OptionsGroupControl.UseControl = True
            Links = <>
          end
          object NavPanelGroup2: TdxNavBarGroup
            Caption = 'Detalii Versiune Curenta'
            SelectedLinkIndex = -1
            TopVisibleLinkIndex = 0
            OptionsGroupControl.AllowControlResizing = True
            OptionsGroupControl.ShowControl = True
            OptionsGroupControl.UseControl = True
            Links = <>
          end
          object NavPanelGroup3: TdxNavBarGroup
            Caption = 'Optiuni Vizualizare'
            SelectedLinkIndex = -1
            TopVisibleLinkIndex = 0
            OptionsGroupControl.AllowControlResizing = True
            OptionsGroupControl.ShowControl = True
            OptionsGroupControl.UseControl = True
            Links = <>
          end
          object NavPanelGroup1Control: TdxNavBarGroupControl
            Left = 13
            Top = 37
            Width = 274
            Height = 349
            TabOrder = 0
            DesignSize = (
              274
              349)
            GroupIndex = 0
            OriginalHeight = 349
            object Label2: TLabel
              Left = 7
              Top = 1
              Width = 124
              Height = 13
              Caption = 'Perioada de raportare'
              Font.Charset = DEFAULT_CHARSET
              Font.Color = clWindowText
              Font.Height = -11
              Font.Name = 'MS Sans Serif'
              Font.Style = [fsBold]
              ParentFont = False
            end
            object Bevel1: TBevel
              Left = 135
              Top = 8
              Width = 107
              Height = 9
              Anchors = [akLeft, akTop, akRight]
              Shape = bsTopLine
              ExplicitWidth = 110
            end
            object Label11: TLabel
              Left = 4
              Top = 20
              Width = 43
              Height = 13
              Caption = 'Unitate : '
            end
            object Label3: TLabel
              Left = 4
              Top = 117
              Width = 57
              Height = 13
              Caption = 'Anul fiscal : '
            end
            object Label4: TLabel
              Left = 140
              Top = 117
              Width = 41
              Height = 13
              Anchors = [akTop, akRight]
              Caption = 'Versiune'
            end
            object Label1: TLabel
              Left = 4
              Top = 44
              Width = 42
              Height = 13
              Caption = 'Proiect : '
            end
            object Label15: TLabel
              Left = 4
              Top = 68
              Width = 58
              Height = 13
              Caption = 'Functional : '
            end
            object Label16: TLabel
              Left = 4
              Top = 92
              Width = 56
              Height = 13
              Caption = 'Economic : '
            end
            object pnControls: TPanel
              Left = 0
              Top = 133
              Width = 274
              Height = 216
              Align = alBottom
              Anchors = [akLeft, akTop, akRight, akBottom]
              BevelOuter = bvNone
              TabOrder = 7
              DesignSize = (
                274
                216)
              object lbNumarZecimale: TLabel
                Left = 15
                Top = 124
                Width = 77
                Height = 13
                Caption = 'Numar Zecimale'
              end
              object lbModIntroducere: TLabel
                Left = 15
                Top = 100
                Width = 78
                Height = 13
                Caption = 'Mod Introducere'
              end
              object lbHeadModIntroducere: TLabel
                Left = 7
                Top = 85
                Width = 111
                Height = 13
                Caption = 'Mod de introducere'
                Font.Charset = DEFAULT_CHARSET
                Font.Color = clWindowText
                Font.Height = -11
                Font.Name = 'MS Sans Serif'
                Font.Style = [fsBold]
                ParentFont = False
              end
              object lbDataAprobare: TLabel
                Left = 7
                Top = 58
                Width = 78
                Height = 13
                Caption = 'Data Aprobare : '
              end
              object Bevel4: TBevel
                Left = 124
                Top = 81
                Width = 116
                Height = 9
                Anchors = [akLeft, akTop, akRight]
                Shape = bsTopLine
                ExplicitWidth = 119
              end
              object ChkArataDoarPlanificat: TcxCheckBox
                Left = 5
                Top = 173
                Hint = 'Arata doar elementele planificate'
                Caption = 'Doar Planificat'
                Style.LookAndFeel.Kind = lfOffice11
                Style.Shadow = False
                StyleDisabled.LookAndFeel.Kind = lfOffice11
                StyleFocused.LookAndFeel.Kind = lfOffice11
                StyleHot.LookAndFeel.Kind = lfOffice11
                TabOrder = 0
                OnClick = ChkArataDoarPlanificatClick
              end
              object BtnDelVer: TcxButton
                Left = 11
                Top = 7
                Width = 120
                Height = 20
                Caption = 'Anuleaza Versiune'
                LookAndFeel.Kind = lfOffice11
                OptionsImage.Glyph.SourceDPI = 96
                OptionsImage.Glyph.Data = {
                  424D360400000000000036000000280000001000000010000000010020000000
                  000000000000C40E0000C40E00000000000000000000FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF000000
                  FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF000000FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF000000
                  FFFF0000FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF000000FFFF0000FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF000000FFFF0000FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF000000
                  FFFF0000FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF000000FFFF0000FFFF0000FFFFFF00FF00FF00FF000000FFFF0000
                  FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF000000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000
                  FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF000000FFFF0000FFFF0000FFFF0000FFFFFF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF000000FFFF0000FFFF0000FFFF0000FFFFFF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF000000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000
                  FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF000000FFFF0000FFFF0000FFFFFF00FF00FF00FF000000FFFF0000
                  FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF000000FFFF0000FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF000000
                  FFFF0000FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF000000
                  FFFF0000FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF000000FFFF0000FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF000000
                  FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF000000FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00}
                TabOrder = 1
                OnClick = BtnDelVerClick
              end
              object btnAnuleazaVersiune: TcxButton
                Left = 147
                Top = 7
                Width = 120
                Height = 20
                Anchors = [akTop, akRight]
                Caption = 'Anuleaza Ecran'
                LookAndFeel.Kind = lfOffice11
                OptionsImage.Glyph.SourceDPI = 96
                OptionsImage.Glyph.Data = {
                  424D360400000000000036000000280000001000000010000000010020000000
                  000000000000C40E0000C40E00000000000000000000FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF000000
                  FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF000000FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF000000
                  FFFF0000FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF000000FFFF0000FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF000000FFFF0000FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF000000
                  FFFF0000FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF000000FFFF0000FFFF0000FFFFFF00FF00FF00FF000000FFFF0000
                  FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF000000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000
                  FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF000000FFFF0000FFFF0000FFFF0000FFFFFF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF000000FFFF0000FFFF0000FFFF0000FFFFFF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF000000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000
                  FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF000000FFFF0000FFFF0000FFFFFF00FF00FF00FF000000FFFF0000
                  FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF000000FFFF0000FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF000000
                  FFFF0000FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF000000
                  FFFF0000FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF000000FFFF0000FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF000000
                  FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF000000FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00}
                TabOrder = 2
                OnClick = btnAnuleazaVersiuneClick
              end
              object btnLastPlan: TcxButton
                Left = 11
                Top = 30
                Width = 120
                Height = 20
                Caption = 'Ultima Planificare'
                LookAndFeel.Kind = lfOffice11
                OptionsImage.Glyph.SourceDPI = 96
                OptionsImage.Glyph.Data = {
                  424D760600000000000036000000280000001400000014000000010020000000
                  000000000000C40E0000C40E0000000000000000000000000000000000000000
                  0000000000000000000000000000000000000000000000000000000000000000
                  0000000000000000000000000000000000000000000000000000000000000000
                  0000000000000000000000000000000000000000000000000000000000000000
                  0000000000000000000000000000000000000000000000000000000000000000
                  0000000000000000000000000000000000000000000000000000000000000000
                  00000000000000000000000000000000000000000000090605FF4C3130FF4C31
                  31FF090605FF0000000000000000000000000000000000000000000000000000
                  0000000000000000000000000000000000000000000000000000000000000000
                  0000090606FF372524FFAD7877FFAA7473FF372524FF090606FF000000000000
                  0000000000000000000000000000000000000000000000000000000000000000
                  000000000000000000000000000000000000402626FFA6706FFFCCA9A8FFBD98
                  97FFA5706FFF422829FF00000000000000000000000000000000000000000000
                  0000000000000000000000000000000000000000000000000000000000007449
                  48FFC0908FFFDEB8B8FFCEA5A5FFC09696FFC09696FFB17F7FFF784D4CFF0000
                  0000000000000000000000000000000000000000000000000000000000000000
                  00000000000000000000744948FFB78B8AFFD3A6A5FFD7AAAAFFD4A5A6FFCA9C
                  9CFFC29393FFB9898AFFA37574FF784D4CFF0000000000000000000000000000
                  000000000000000000000000000000000000090605FF3E2626FFC18988FFD2A0
                  A0FFD8A7A7FFD6A3A4FFD5A2A3FFD09E9DFFC89595FFC28F8FFFB88685FFB079
                  79FF412A29FF090606FF00000000000000000000000000000000000000000905
                  06FF382624FFAB7473FFE3ADADFFD7A1A1FFD69E9FFFD59C9DFFD59B9CFFD29B
                  9AFFCE9696FFC89090FFC08989FFBF898AFFA66D6CFF372423FF090606FF0000
                  00000000000000000000000000003E2525FFAA716FFFEAB6B6FFDAA3A3FFD89D
                  9DFFD69797FFD49494FFD59595FFD29494FFD29393FFCE9191FFC98B8BFFC086
                  86FFBE8484FFA76E6EFF412A2AFF0000000000000000000000007B504EFFD09E
                  9EFFFAD1D2FFEFC9C9FFECC0C0FFE2B1AFFFD99B9AFFD38B89FFD28A8AFFD389
                  89FFD28B8BFFD18A8AFFD18989FFD38A8AFFC88381FFC68381FFB77777FF7E54
                  52FF000000000000000079504EFFA46F6EFFA97675FFA87675FF996A69FFD097
                  97FFD99595FFD38785FFD28383FFD18281FFD28283FFD38484FFC67C7DFF9563
                  62FFA16A69FFA06968FFA06968FF79514FFF0000000000000000392625FF482F
                  2EFF442C2BFF3B2625FF8D5A59FFCF9797FFDC9899FFD38585FFD17F7FFFD17E
                  7DFFD17D7DFFD37E7EFFC47677FF8F5F5EFF3C2827FF462E2DFF49302FFF3926
                  25FF000000000000000000000000000000000000000000000000996261FFD39E
                  9FFFDFA0A0FFD58989FFD38080FFD37E7EFFD17D7CFFD37B7AFFC57473FF9A67
                  65FF000000000000000000000000000000000000000000000000000000000000
                  00000000000000000000986260FFD4A1A1FFE3A6A7FFDB9393FFD88B8AFFD685
                  85FFD58080FFD57B7BFFC47270FF9A6766FF0000000000000000000000000000
                  0000000000000000000000000000000000000000000000000000986160FFDBA8
                  A8FFF0BCBCFFECB5B5FFECB5B5FFE9AFAFFFE8A6A7FFE39696FFCD7E7EFF9A66
                  65FF000000000000000000000000000000000000000000000000000000000000
                  00000000000000000000A76D6CFFA57170FFA87373FFA77271FFA77271FFA771
                  70FFA76F6FFFA66D6CFFA26968FFA86E6DFF0000000000000000000000000000
                  0000000000000000000000000000000000000000000000000000503434FF452D
                  2CFF442C2BFF442C2CFF442C2CFF442C2CFF452D2CFF452D2CFF452E2DFF5034
                  34FF000000000000000000000000000000000000000000000000000000000000
                  0000000000000000000000000000000000000000000000000000000000000000
                  0000000000000000000000000000000000000000000000000000000000000000
                  0000000000000000000000000000000000000000000000000000000000000000
                  0000000000000000000000000000000000000000000000000000000000000000
                  00000000000000000000000000000000000000000000}
                TabOrder = 3
                OnClick = btnLastPlanClick
              end
              object btnMuta: TcxButton
                Left = 147
                Top = 30
                Width = 120
                Height = 20
                Anchors = [akTop, akRight]
                Caption = 'Preia buget'
                DropDownMenu = pmPreiaPlanificare
                Kind = cxbkDropDown
                LookAndFeel.Kind = lfOffice11
                OptionsImage.Glyph.SourceDPI = 96
                OptionsImage.Glyph.Data = {
                  424D360400000000000036000000280000001000000010000000010020000000
                  000000000000C40E0000C40E00000000000000000000FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00808080FF000000FF000000FF000000FF000000FF000000FF0000
                  00FF000000FF000000FF000000FFFF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00808080FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
                  FFFFFFFFFFFFFFFFFFFF000000FFFF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00808080FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
                  FFFFFFFFFFFFFFFFFFFF000000FFFF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00808080FFFFFFFFFFFFFFFFFF008000FFFFFFFFFFFFFFFFFFFFFF
                  FFFFFFFFFFFFFFFFFFFF000000FFFF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00808080FFFFFFFFFFFFFFFFFF008000FF008000FFFFFFFFFFFFFF
                  FFFFFFFFFFFFFFFFFFFF000000FFFF00FF00FF00FF00FF00FF00008000FF0080
                  00FF008000FF008000FF008000FF008000FF008000FF008000FF008000FFFFFF
                  FFFFFFFFFFFFFFFFFFFF000000FFFF00FF00FF00FF00FF00FF00008000FF0080
                  00FF008000FF008000FF008000FF008000FF008000FF008000FF008000FF0080
                  00FFFFFFFFFFFFFFFFFF000000FFFF00FF00FF00FF00FF00FF00008000FF0080
                  00FF008000FF008000FF008000FF008000FF008000FF008000FF008000FFFFFF
                  FFFFFFFFFFFFFFFFFFFF000000FFFF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00808080FFFFFFFFFFFFFFFFFF008000FF008000FFFFFFFFFFFFFF
                  FFFFFFFFFFFFFFFFFFFF000000FFFF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00808080FFFFFFFFFFFFFFFFFF008000FFFFFFFFFFFFFFFFFFFFFF
                  FFFFFFFFFFFFFFFFFFFF000000FFFF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00808080FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF0000
                  00FF000000FF000000FF000000FFFF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00808080FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF0000
                  00FFC0C0C0FF000000FFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00808080FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF0000
                  00FF000000FFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00808080FF808080FF808080FF808080FF808080FF808080FF0000
                  00FFFF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
                  FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00}
                TabOrder = 4
              end
              object edDataAprobare: TcxDBDateEdit
                Left = 96
                Top = 54
                DataBinding.DataField = 'DATA_APROBARE'
                DataBinding.DataSource = DTVersiune
                ParentFont = False
                Properties.ImmediatePost = True
                Properties.InputKind = ikMask
                Properties.SaveTime = False
                Properties.ShowTime = False
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
                TabOrder = 5
                OnExit = edDataAprobareExit
                Width = 159
              end
              object edNrZerouri: TcxImageComboBox
                Left = 96
                Top = 98
                EditValue = '1'
                Properties.Items = <
                  item
                    Description = 'Leu'
                    ImageIndex = 0
                    Value = '1'
                  end
                  item
                    Description = 'Zeci de lei'
                    ImageIndex = 1
                    Value = '10'
                  end
                  item
                    Description = 'Sute de lei'
                    ImageIndex = 2
                    Value = '100'
                  end
                  item
                    Description = 'Mii lei'
                    ImageIndex = 3
                    Value = '1000'
                  end
                  item
                    Description = 'Zeci de mii'
                    ImageIndex = 4
                    Value = '10000'
                  end
                  item
                    Description = 'Sute de mii'
                    ImageIndex = 5
                    Value = '100000'
                  end
                  item
                    Description = 'Milioane'
                    ImageIndex = 6
                    Value = '1000000'
                  end>
                Properties.OnChange = edNrZerouriPropertiesChange
                Style.LookAndFeel.Kind = lfOffice11
                StyleDisabled.LookAndFeel.Kind = lfOffice11
                StyleFocused.LookAndFeel.Kind = lfOffice11
                StyleHot.LookAndFeel.Kind = lfOffice11
                TabOrder = 6
                Width = 97
              end
              object edZerouri: TcxSpinEdit
                Left = 196
                Top = 98
                Properties.OnChange = edZerouriChange
                Style.LookAndFeel.Kind = lfOffice11
                StyleDisabled.LookAndFeel.Kind = lfOffice11
                StyleFocused.LookAndFeel.Kind = lfOffice11
                StyleHot.LookAndFeel.Kind = lfOffice11
                TabOrder = 7
                Value = 1
                Width = 59
              end
              object edZecimale: TcxSpinEdit
                Left = 98
                Top = 121
                Properties.AssignedValues.MinValue = True
                Properties.MaxValue = 20.000000000000000000
                Properties.OnChange = edZecimaleChange
                Style.LookAndFeel.Kind = lfOffice11
                StyleDisabled.LookAndFeel.Kind = lfOffice11
                StyleFocused.LookAndFeel.Kind = lfOffice11
                StyleHot.LookAndFeel.Kind = lfOffice11
                TabOrder = 8
                Width = 49
              end
              object edIDVersiune: TcxTextEdit
                Left = 212
                Top = 144
                Properties.ReadOnly = True
                Style.Color = clSilver
                Style.LookAndFeel.Kind = lfOffice11
                StyleDisabled.LookAndFeel.Kind = lfOffice11
                StyleFocused.LookAndFeel.Kind = lfOffice11
                StyleHot.LookAndFeel.Kind = lfOffice11
                TabOrder = 9
                Width = 34
              end
              object btnBlocheazaVers: TcxButton
                Left = 67
                Top = 145
                Width = 139
                Height = 20
                Caption = 'Blocheaza Versiune'
                LookAndFeel.Kind = lfOffice11
                TabOrder = 10
                OnClick = btnBlocheazaVersClick
              end
              object chkColoaneEstimari: TcxCheckBox
                Left = 151
                Top = 194
                Hint = 'Estimari pentru urmatorii ani'
                Anchors = [akTop, akRight]
                Caption = 'Coloane Estimari'
                Style.LookAndFeel.Kind = lfOffice11
                Style.Shadow = False
                StyleDisabled.LookAndFeel.Kind = lfOffice11
                StyleFocused.LookAndFeel.Kind = lfOffice11
                StyleHot.LookAndFeel.Kind = lfOffice11
                TabOrder = 11
                OnClick = chkColoaneEstimariClick
              end
              object chkColoaneCA: TcxCheckBox
                Left = 5
                Top = 195
                Hint = 'Estimari pentru urmatorii ani'
                Caption = 'Coloane CA'
                Style.LookAndFeel.Kind = lfOffice11
                Style.Shadow = False
                StyleDisabled.LookAndFeel.Kind = lfOffice11
                StyleFocused.LookAndFeel.Kind = lfOffice11
                StyleHot.LookAndFeel.Kind = lfOffice11
                TabOrder = 12
                OnClick = chkColoaneCAClick
              end
              object chkPlanLuna: TcxCheckBox
                Left = 151
                Top = 173
                Hint = 'Arata doar elementele planificate'
                Anchors = [akTop, akRight]
                Caption = 'Planificare Pe Luna'
                Style.LookAndFeel.Kind = lfOffice11
                Style.Shadow = False
                StyleDisabled.LookAndFeel.Kind = lfOffice11
                StyleFocused.LookAndFeel.Kind = lfOffice11
                StyleHot.LookAndFeel.Kind = lfOffice11
                TabOrder = 13
                OnClick = chkPlanLunaClick
              end
            end
            object edUnitate: TcxTextEdit
              Left = 63
              Top = 15
              Anchors = [akLeft, akTop, akRight]
              Properties.ReadOnly = True
              Style.Color = clSilver
              Style.LookAndFeel.Kind = lfOffice11
              StyleDisabled.LookAndFeel.Kind = lfOffice11
              StyleFocused.LookAndFeel.Kind = lfOffice11
              StyleHot.LookAndFeel.Kind = lfOffice11
              TabOrder = 0
              Width = 204
            end
            object edAnFiscal: TcxSpinEdit
              Left = 64
              Top = 114
              Properties.OnChange = edAnFiscalPropertiesChange
              Style.LookAndFeel.Kind = lfOffice11
              StyleDisabled.LookAndFeel.Kind = lfOffice11
              StyleFocused.LookAndFeel.Kind = lfOffice11
              StyleHot.LookAndFeel.Kind = lfOffice11
              TabOrder = 1
              Value = 2007
              Width = 57
            end
            object edProiect: TcxTextEdit
              Left = 63
              Top = 39
              Anchors = [akLeft, akTop, akRight]
              Properties.ReadOnly = True
              Style.Color = clSilver
              Style.LookAndFeel.Kind = lfOffice11
              StyleDisabled.LookAndFeel.Kind = lfOffice11
              StyleFocused.LookAndFeel.Kind = lfOffice11
              StyleHot.LookAndFeel.Kind = lfOffice11
              TabOrder = 2
              Width = 204
            end
            object edFunctional: TcxTextEdit
              Left = 63
              Top = 63
              Anchors = [akLeft, akTop, akRight]
              Properties.ReadOnly = True
              Style.Color = clSilver
              Style.LookAndFeel.Kind = lfOffice11
              StyleDisabled.LookAndFeel.Kind = lfOffice11
              StyleFocused.LookAndFeel.Kind = lfOffice11
              StyleHot.LookAndFeel.Kind = lfOffice11
              TabOrder = 3
              Width = 204
            end
            object edEconomic: TcxTextEdit
              Left = 63
              Top = 87
              Anchors = [akLeft, akTop, akRight]
              Properties.ReadOnly = True
              Style.Color = clSilver
              Style.LookAndFeel.Kind = lfOffice11
              StyleDisabled.LookAndFeel.Kind = lfOffice11
              StyleFocused.LookAndFeel.Kind = lfOffice11
              StyleHot.LookAndFeel.Kind = lfOffice11
              TabOrder = 4
              Width = 204
            end
            object btnAddVersiune: TcxButton
              Left = 249
              Top = 115
              Width = 21
              Height = 21
              Anchors = [akTop, akRight]
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
              TabOrder = 5
              OnClick = btnAddVersiuneClick
            end
            object edVersiune: TcxLookupComboBox
              Left = 184
              Top = 114
              Anchors = [akTop, akRight]
              Properties.Alignment.Horz = taLeftJustify
              Properties.DropDownAutoSize = True
              Properties.DropDownListStyle = lsFixedList
              Properties.DropDownSizeable = True
              Properties.IncrementalFiltering = False
              Properties.KeyFieldNames = 'ID_BG_VERSIUNE'
              Properties.ListColumns = <
                item
                  Caption = 'Versiune'
                  FieldName = 'descriere'
                end
                item
                  Caption = 'Aprobata'
                  SortOrder = soDescending
                  FieldName = 'DATA_APROBARE'
                end
                item
                  Caption = 'Revizie'
                  SortOrder = soDescending
                  FieldName = 'Revizie'
                end>
              Properties.ListOptions.CaseInsensitive = True
              Properties.ListSource = DTVersiune
              Properties.OnEditValueChanged = edVersiunePropertiesEditValueChanged
              Properties.OnInitPopup = edVersiunePropertiesInitPopup
              Style.LookAndFeel.Kind = lfOffice11
              StyleDisabled.LookAndFeel.Kind = lfOffice11
              StyleFocused.LookAndFeel.Kind = lfOffice11
              StyleHot.LookAndFeel.Kind = lfOffice11
              TabOrder = 6
              Width = 63
            end
          end
          object NavPanelGroup2Control: TdxNavBarGroupControl
            Left = 13
            Top = 427
            Width = 274
            Height = 340
            TabOrder = 2
            GroupIndex = 1
            OriginalHeight = 340
            object cxIspDetalii: TcxDBVerticalGrid
              Left = 0
              Top = 0
              Width = 274
              Height = 340
              BorderStyle = cxcbsNone
              Align = alClient
              OptionsView.CellEndEllipsis = True
              OptionsView.CategoryExplorerStyle = True
              OptionsView.RowHeaderWidth = 149
              OptionsBehavior.AllowChangeRecord = False
              Navigator.Buttons.CustomButtons = <>
              ScrollbarAnnotations.CustomAnnotations = <>
              TabOrder = 0
              OnExit = ispDetaliiExit
              DataController.DataSource = DTVersiune
              Version = 1
              object cxIspDetaliiCategoryRow1: TcxCategoryRow
                Properties.Caption = 'Detalii ale variantei de buget'
                ID = 0
                ParentID = -1
                Index = 0
                Version = 1
              end
              object cxIspDetaliiID_BG_VERSIUNE: TcxDBEditorRow
                Properties.Caption = 'Identificator Versiune'
                Properties.EditPropertiesClassName = 'TcxTextEditProperties'
                Properties.EditProperties.ReadOnly = False
                Properties.DataBinding.FieldName = 'ID_BG_VERSIUNE'
                Properties.Options.ShowEditButtons = eisbNever
                Styles.Header = cxStyle1
                Styles.Content = cxStyle1
                ID = 1
                ParentID = 0
                Index = 0
                Version = 1
              end
              object cxIspDetaliiDATA_CREARE: TcxDBEditorRow
                Properties.Caption = 'Data Crearii'
                Properties.EditPropertiesClassName = 'TcxDateEditProperties'
                Properties.EditProperties.InputKind = ikMask
                Properties.EditProperties.SaveTime = False
                Properties.EditProperties.ShowTime = False
                Properties.DataBinding.FieldName = 'DATA_CREARE'
                ID = 2
                ParentID = 0
                Index = 1
                Version = 1
              end
              object cxIspDetaliiID_UTILIZATORI_CREAT: TcxDBEditorRow
                Properties.Caption = 'Creat de '
                Properties.EditPropertiesClassName = 'TcxImageComboBoxProperties'
                Properties.EditProperties.Items = <>
                Properties.DataBinding.FieldName = 'ID_UTILIZATORI_CREAT'
                ID = 3
                ParentID = 0
                Index = 2
                Version = 1
              end
              object cxIspDetaliiDEPARTAMENTE_CREAT: TcxDBEditorRow
                Properties.Caption = 'Departament'
                Properties.EditPropertiesClassName = 'TcxImageComboBoxProperties'
                Properties.EditProperties.Items = <>
                Properties.DataBinding.FieldName = 'DEPARTAMENTE_CREAT'
                ID = 4
                ParentID = 0
                Index = 3
                Version = 1
              end
              object cxIspDetaliiCategoryRow2: TcxCategoryRow
                Properties.Caption = 'Aprobare'
                ID = 5
                ParentID = -1
                Index = 1
                Version = 1
              end
              object cxIspDetaliiDATA_APROBARE: TcxDBEditorRow
                Properties.Caption = 'Data Aprobarii'
                Properties.EditPropertiesClassName = 'TcxDateEditProperties'
                Properties.EditProperties.InputKind = ikMask
                Properties.EditProperties.SaveTime = False
                Properties.EditProperties.ShowTime = False
                Properties.DataBinding.FieldName = 'DATA_APROBARE'
                Styles.Header = frmData.cxStyle16
                ID = 6
                ParentID = 5
                Index = 0
                Version = 1
              end
              object cxIspDetaliiID_UTILIZATORI_APROBAT: TcxDBEditorRow
                Properties.Caption = 'Aprobat de'
                Properties.EditPropertiesClassName = 'TcxImageComboBoxProperties'
                Properties.EditProperties.Items = <>
                Properties.DataBinding.FieldName = 'ID_UTILIZATORI_APROBAT'
                ID = 7
                ParentID = 5
                Index = 1
                Version = 1
              end
              object cxIspDetaliiFUNCTIE: TcxDBEditorRow
                Properties.Caption = 'Functie'
                Properties.DataBinding.FieldName = 'FUNCTIE'
                ID = 8
                ParentID = 5
                Index = 2
                Version = 1
              end
              object cxIspDetaliiDEPARTAMENTE_APROBAT: TcxDBEditorRow
                Properties.Caption = 'Departament'
                Properties.DataBinding.FieldName = 'DEPARTAMENTE_APROBAT'
                ID = 9
                ParentID = 5
                Index = 3
                Version = 1
              end
              object cxIspDetaliiCategoryRow3: TcxCategoryRow
                Properties.Caption = 'Detalii varianta curenta'
                ID = 10
                ParentID = -1
                Index = 2
                Version = 1
              end
              object cxIspDetaliiDBMultiEditorRow1: TcxDBMultiEditorRow
                Properties.Editors = <
                  item
                    Caption = 'Versiune'
                    EditPropertiesClassName = 'TcxTextEditProperties'
                    EditProperties.ReadOnly = True
                    DataBinding.FieldName = 'REVIZIE'
                  end
                  item
                    Caption = 'An Fiscal'
                    EditPropertiesClassName = 'TcxTextEditProperties'
                    EditProperties.ReadOnly = True
                    DataBinding.FieldName = 'AN_FISCAL'
                  end>
                Styles.Header = cxStyle1
                Styles.Content = cxStyle1
                ID = 11
                ParentID = 10
                Index = 0
                Version = 1
              end
              object cxIspDetaliiEXPLICATIE: TcxDBEditorRow
                Height = 63
                Properties.Caption = 'Explicatie'
                Properties.EditPropertiesClassName = 'TcxMemoProperties'
                Properties.DataBinding.FieldName = 'EXPLICATIE'
                ID = 12
                ParentID = 10
                Index = 1
                Version = 1
              end
              object cxIspDetaliiACT_APROBARE: TcxDBEditorRow
                Height = 46
                Properties.Caption = 'Act Aprobare'
                Properties.EditPropertiesClassName = 'TcxMemoProperties'
                Properties.DataBinding.FieldName = 'ACT_APROBARE'
                ID = 13
                ParentID = 10
                Index = 2
                Version = 1
              end
              object cxIspDetaliiCLASA_FUNCTIONALA: TcxDBEditorRow
                Properties.DataBinding.FieldName = 'CLASA_FUNCTIONALA'
                Visible = False
                ID = 14
                ParentID = -1
                Index = 3
                Version = 1
              end
              object cxIspDetaliiTIP_BUGET: TcxDBEditorRow
                Properties.DataBinding.FieldName = 'TIP_BUGET'
                Visible = False
                ID = 15
                ParentID = -1
                Index = 4
                Version = 1
              end
              object cxIspDetaliiVERSIUNE: TcxDBEditorRow
                Properties.Caption = 'Versiune'
                Properties.DataBinding.FieldName = 'VERSIUNE'
                Visible = False
                ID = 16
                ParentID = -1
                Index = 5
                Version = 1
              end
              object cxIspDetaliiAN_FISCAL: TcxDBEditorRow
                Properties.Caption = 'An Fiscal'
                Properties.DataBinding.FieldName = 'AN_FISCAL'
                Visible = False
                ID = 17
                ParentID = -1
                Index = 6
                Version = 1
              end
              object cxIspDetaliiREVIZIE: TcxDBEditorRow
                Properties.Caption = 'Versiune'
                Properties.DataBinding.FieldName = 'REVIZIE'
                Visible = False
                ID = 18
                ParentID = -1
                Index = 7
                Version = 1
              end
            end
          end
          object NavPanelGroup3Control: TdxNavBarGroupControl
            Left = 13
            Top = 808
            Width = 274
            Height = 251
            TabOrder = 1
            DesignSize = (
              274
              251)
            GroupIndex = 2
            OriginalHeight = 251
            object Label12: TLabel
              Left = 8
              Top = 11
              Width = 95
              Height = 13
              Caption = 'Marcatori Vizuali'
              Font.Charset = DEFAULT_CHARSET
              Font.Color = clWindowText
              Font.Height = -11
              Font.Name = 'MS Sans Serif'
              Font.Style = [fsBold]
              ParentFont = False
            end
            object Bevel5: TBevel
              Left = 107
              Top = 15
              Width = 132
              Height = 9
              Anchors = [akLeft, akTop, akRight]
              Shape = bsTopLine
              ExplicitWidth = 138
            end
            object Label13: TLabel
              Left = 8
              Top = 27
              Width = 96
              Height = 13
              Caption = 'Nivel de Introducere'
            end
            object Label14: TLabel
              Left = 8
              Top = 51
              Width = 93
              Height = 13
              Caption = 'Culoare Introducere'
            end
            object Label5: TLabel
              Left = 7
              Top = 130
              Width = 80
              Height = 13
              Caption = 'Balanta buget'
              Font.Charset = DEFAULT_CHARSET
              Font.Color = clBlack
              Font.Height = -11
              Font.Name = 'MS Sans Serif'
              Font.Style = [fsBold]
              ParentFont = False
            end
            object Bevel2: TBevel
              Left = 89
              Top = 137
              Width = 151
              Height = 9
              Anchors = [akLeft, akTop, akRight]
              Shape = bsTopLine
              ExplicitWidth = 157
            end
            object Label6: TLabel
              Left = 15
              Top = 145
              Width = 69
              Height = 13
              Caption = 'Total Cheltuieli'
            end
            object Label7: TLabel
              Left = 15
              Top = 160
              Width = 62
              Height = 13
              Caption = 'Total Venituri'
            end
            object Bevel3: TBevel
              Left = 63
              Top = 177
              Width = 177
              Height = 5
              Anchors = [akLeft, akTop, akRight]
              Shape = bsTopLine
              ExplicitWidth = 183
            end
            object LbExcedent: TLabel
              Left = 15
              Top = 179
              Width = 45
              Height = 13
              Caption = 'Excedent'
            end
            object btnColorInside: TPanel
              Left = 128
              Top = 19
              Width = 113
              Height = 21
              Color = clMoneyGreen
              TabOrder = 1
              OnClick = btnColorInsideClick
            end
            object btnFontColor: TPanel
              Left = 128
              Top = 43
              Width = 113
              Height = 21
              Color = clMaroon
              TabOrder = 2
              OnClick = btnColorInsideClick
            end
            object chkVisualMark: TcxCheckBox
              Left = 16
              Top = 78
              Caption = 'Marcheaza Vizual posibilitatea de editare'
              Style.LookAndFeel.Kind = lfOffice11
              StyleDisabled.LookAndFeel.Kind = lfOffice11
              StyleFocused.LookAndFeel.Kind = lfOffice11
              StyleHot.LookAndFeel.Kind = lfOffice11
              TabOrder = 0
              OnClick = chkVisualMarkClick
            end
            object chkFitScreen: TcxCheckBox
              Left = 13
              Top = 210
              Caption = 'Incadreaza coloane in fereastra'
              State = cbsChecked
              TabOrder = 3
              OnClick = chkFitScreenClick
            end
          end
        end
        object tabBuget: TcxTabControl
          Left = 0
          Top = 0
          Width = 317
          Height = 20
          Align = alTop
          TabOrder = 1
          Properties.CustomButtons.Buttons = <>
          Properties.Style = 9
          Properties.TabIndex = 0
          Properties.Tabs.Strings = (
            'Toate'
            'Buget'
            'Deschideri')
          LookAndFeel.Kind = lfOffice11
          OnChange = tabBugetChange
          ClientRectRight = 0
          ClientRectTop = 0
        end
      end
    end
    object pnInfo: TPanel
      Left = 0
      Top = 632
      Width = 1030
      Height = 55
      Align = alBottom
      BevelOuter = bvNone
      TabOrder = 2
      DesignSize = (
        1030
        55)
      object btnRaportare: TcxButton
        Left = 23
        Top = 6
        Width = 88
        Height = 27
        Anchors = [akLeft, akBottom]
        Caption = 'Rapoarte'
        Kind = cxbkDropDown
        LookAndFeel.Kind = lfOffice11
        LookAndFeel.NativeStyle = False
        LookAndFeel.SkinName = ''
        OptionsImage.Glyph.SourceDPI = 96
        OptionsImage.Glyph.Data = {
          424D360400000000000036000000280000001000000010000000010020000000
          000000000000C40E0000C40E00000000000000000000F8F8F800F8F8F800F8F8
          F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800EFF1EFFFD0D9D0FFF8F8
          F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800DEE5DEFF1A8318FFABBE
          ABFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800DDE6DDFF0C9D07FF0A89
          06FF84A884FFF6F6F6FFF8F8F800F8F8F800F8F8F800F8F8F800569E55FF2D8B
          2BFF2D8B2BFF2D8B2BFF2D8B2BFF2D8B2BFF2D8B2BFF2D8F2AFF0AA103FF08A4
          00FF079401FF649663FFF0F0F0FFF8F8F800F8F8F800F8F8F80036AD32FF08A8
          00FF08A800FF08A800FF08A800FF08A800FF08A800FF08A800FF08A800FF08A8
          00FF08A800FF069F00FF448D41FFE0E4E0FFF8F8F800F8F8F8003BAE38FF08AD
          00FF08AD00FF08AD00FF08AD00FF08AD00FF08AD00FF08AD00FF08AD00FF08AD
          00FF08AD00FF08AD00FF07A900FF308F2DFFE7EAE7FFF8F8F8003AB036FF08B0
          00FF08B000FF08B000FF08B000FF08B000FF08B000FF08B000FF08B000FF08B0
          00FF08B000FF08B000FF1AAF14FFB0D5AFFFF8F8F800F8F8F80039B435FF08B5
          00FF08B500FF08B500FF08B500FF08B500FF08B500FF08B500FF08B500FF08B5
          00FF08B500FF27B422FFC6E0C6FFF8F8F800F8F8F800F8F8F800BFDCBDFFB3DA
          B0FFB3DAB0FFB3DAB0FFB3DAB1FFB3DAB3FFB3DAB3FFA1CFA0FF0BB505FF08B8
          00FF3EBB3BFFDCE9DCFFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800DEE6DEFF0CB807FF58C0
          55FFE8EEE8FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800E5EBE4FF84CA84FFF4F5
          F4FFF8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8
          F800F8F8F800F8F8F800F8F8F800F8F8F800F8F8F800}
        OptionsImage.Layout = blGlyphRight
        TabOrder = 0
        Visible = False
      end
    end
    object splitV: TcxSplitter
      Left = 0
      Top = 595
      Width = 1030
      Height = 8
      HotZoneClassName = 'TcxXPTaskBarStyle'
      AlignSplitter = salBottom
      Control = pnBottom
      OnAfterOpen = splitVAfterOpen
      OnAfterClose = splitVAfterClose
      ExplicitTop = 609
    end
    object pnBottom: TPanel
      Left = 0
      Top = 603
      Width = 1030
      Height = 29
      Align = alBottom
      BevelOuter = bvNone
      TabOrder = 0
      ExplicitTop = 617
    end
  end
  object QryClasaEconomica: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      
        'EXEC spBugetIntroducere_BG_APROBAT :ID_VERSIUNE, :DIVIZOR, :COD_' +
        'FUNCTIONAL, :COD_ECONOMIC, :ID_OI_UNITATI, :ID_OI_PROIECTE')
    Params = <
      item
        DataType = ftUnknown
        Name = 'ID_VERSIUNE'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'DIVIZOR'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'COD_FUNCTIONAL'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'COD_ECONOMIC'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'ID_OI_UNITATI'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'ID_OI_PROIECTE'
        ParamType = ptUnknown
      end>
    Left = 81
    Top = 225
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'ID_VERSIUNE'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'DIVIZOR'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'COD_FUNCTIONAL'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'COD_ECONOMIC'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'ID_OI_UNITATI'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'ID_OI_PROIECTE'
        ParamType = ptUnknown
      end>
  end
  object DTClasEconomica: TDataSource
    DataSet = ClasaEconomica
    Left = 137
    Top = 177
  end
  object ClasaEconomica: TdxMemData
    Indexes = <>
    SortOptions = []
    Left = 225
    Top = 177
  end
  object qryVersiune: TZQuery
    Connection = frmData.dbContabilitate
    OnNewRecord = qryVersiuneNewRecord
    SQL.Strings = (
      'SELECT * FROM BG_VERSIUNE'
      'where '
      '  an_fiscal = :an_fiscal'
      ' and '
      '(tip_versiune=:tip_versiune or :tip_versiune is null )'
      'order by '
      '  tip_versiune desc, revizie')
    Params = <
      item
        DataType = ftInteger
        Name = 'an_fiscal'
        ParamType = ptInput
      end
      item
        DataType = ftString
        Name = 'tip_versiune'
        ParamType = ptInput
      end>
    Left = 145
    Top = 289
    ParamData = <
      item
        DataType = ftInteger
        Name = 'an_fiscal'
        ParamType = ptInput
      end
      item
        DataType = ftString
        Name = 'tip_versiune'
        ParamType = ptInput
      end>
  end
  object DTVersiune: TDataSource
    DataSet = qryVersiune
    Left = 89
    Top = 289
  end
  object ImageList1: TImageList
    Left = 56
    Top = 160
    Bitmap = {
      494C010102000500040010001000FFFFFFFFFF10FFFFFFFFFFFFFFFF424D3600
      0000000000003600000028000000400000001000000001002000000000000010
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000F7F7F700DEDEDE00D6D6
      D600D6D6D600D6D6D600D6D6D600D6D6D600D6D6D600D6D6D600D6D6D600D6D6
      D600D6D6D600DEDEDE00F7F7F7000000000000000000F7F7F700DEDEDE00D6D6
      D600D6D6D600D6D6D600D6D6D600D6D6D600D6D6D600D6D6D600D6D6D600D6D6
      D600D6D6D600D6D6D600DEDEDE00F7F7F7000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000F7F7F700C6C6C6008C8C8C007373
      7300737373007373730073737300737373007373730073737300737373007373
      7300737373008C8C8C00C6C6C600F7F7F700F7F7F700CECECE00848484006B6B
      6B006B6B6B006B6B6B006B6B6B006B6B6B006B6B6B006B6B6B006B6B6B006B6B
      6B006B6B6B006B6B6B0084848400CECECE000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000DEDEDE000873A5000873A5000873
      A5000873A5000873A5000873A5000873A5000873A5000873A5000873A5000873
      A5000873A500636363008C8C8C00DEDEDE00DEDEDE001884B5001884B500187B
      B500107BAD00107BAD001073AD000873A5000873A500086BA500006B9C00006B
      9C00006B9C0000639C004A4A4A00848484000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000189CC600189CC6009CFFFF006BD6
      FF006BD6FF006BD6FF006BD6FF006BD6FF006BD6FF006BD6FF006BD6FF006BD6
      FF00299CBD000873A50073737300D6D6D6002184BD0063CEFF002184BD009CFF
      FF006BD6FF006BD6FF006BD6FF006BD6FF006BD6FF006BD6FF006BD6FF006BD6
      FF0039A5D6009CFFFF0000639C006B6B6B000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000189CC600189CC6007BE7F7009CFF
      FF007BE7FF007BE7FF007BE7FF007BE7FF007BE7FF007BE7FF007BE7FF007BDE
      FF0042B5DE00187B9C0063636300BDBDBD00218CBD0063CEFF00218CBD009CFF
      FF007BE7FF007BE7FF007BE7FF007BE7FF007BE7FF007BE7FF007BE7FF007BE7
      FF0042ADDE009CFFFF00006B9C006B6B6B000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000189CC60021A5CE0042BDD6009CFF
      FF0084EFFF0084EFFF0084EFFF0084EFFF0084EFFF0084EFFF0084EFFF0084E7
      FF0042BDEF00189CC600636363008C8C8C00298CC60063CEFF002994C6009CFF
      FF0084EFFF0084EFFF0084EFFF0084EFFF0084EFFF0084EFFF0084EFFF0084EF
      FF004AB5E7009CFFFF00006B9C006B6B6B000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000189CC60042B5E70021A5CE00A5FF
      FF0094F7FF0094F7FF0094F7FF0094F7FF0094F7FF0094F7FF0094F7FF0094F7
      FF0052BDE7005ABDCE000873A50073737300298CC60063CEFF00319CCE009CFF
      FF0094F7FF0094F7FF0094F7FF0094F7FF0094F7FF0094F7FF0094F7FF0094F7
      FF0052BDEF009CFFFF00006B9C006B6B6B000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000189CC60073D6FF00189CC6008CF7
      F7009CFFFF009CFFFF009CFFFF009CFFFF009CFFFF009CFFFF009CFFFF009CFF
      FF005AC6FF0094FFFF00187B9C00737373002994C6006BD6FF00319CCE009CFF
      FF009CFFFF009CFFFF009CFFFF009CFFFF009CFFFF009CFFFF009CFFFF009CFF
      FF0063C6FF009CFFFF00086BA5006B6B6B000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000189CC60084D6FF00189CC6006BBD
      DE000000000000000000F7FFFF00000000000000000000000000000000000000
      000084E7FF0000000000187BA5008C8C8C002994C6007BE7FF002994C6000000
      0000000000000000000000000000000000000000000000000000000000000000
      000084E7FF00000000000873A5008C8C8C000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000189CC60084EFFF0052C6E700189C
      C600189CC600189CC600189CC600189CC600189CC600189CC600189CC600189C
      C600189CC600189CC600188CB500C6C6C6003194CE0084EFFF0084E7FF002994
      C6002994C6002994C6002994C6002994C6002994C600298CC600218CBD002184
      BD001884B5001884B5001884B500DEDEDE000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000189CC6009CF7FF008CF7FF008CF7
      FF008CF7FF008CF7FF008CF7FF00000000000000000000000000000000000000
      0000189CC600187B9C00C6C6C600F7F7F700319CCE0094F7FF008CF7FF008CF7
      FF008CF7FF008CF7FF008CF7FF00000000000000000000000000000000000000
      0000107BAD008C8C8C00DEDEDE00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000189CC600000000009CFFFF009CFF
      FF009CFFFF009CFFFF0000000000189CC600189CC600189CC600189CC600189C
      C600189CC600DEDEDE00F7F7F70000000000319CCE00000000009CFFFF009CFF
      FF009CFFFF009CFFFF0000000000218CBD002184BD001884B5001884B5001884
      B500187BB500DEDEDE00F7F7F700000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000021A5CE00000000000000
      00000000000000000000189CC600C6C6C600F7F7F70000000000000000000000
      00000000000000000000000000000000000000000000319CCE00000000000000
      00000000000000000000298CC600CECECE00F7F7F70000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000021A5CE0021A5
      CE0021A5CE0021A5CE00DEDEDE00F7F7F7000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000319CCE00319C
      CE003194CE002994C600DEDEDE00F7F7F7000000000000000000000000000000
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
      000000000000000000000000000000000000424D3E000000000000003E000000
      2800000040000000100000000100010000000000800000000000000000000000
      000000000000000000000000FFFFFF00FFFFFFFF000000008001800000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000DF41FF400000000
      000000000000000001F001F1000000004201420100000000BC7FBC7F00000000
      C0FFC0FF00000000FFFFFFFF0000000000000000000000000000000000000000
      000000000000}
  end
  object ImageList2: TImageList
    Left = 280
    Top = 102
    Bitmap = {
      494C010103000500040010001000FFFFFFFFFF10FFFFFFFFFFFFFFFF424D3600
      0000000000003600000028000000400000001000000001002000000000000010
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000004A9CC6008484840084A5AD000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000006B8C940073CEE7005A737B000000
      000000000000000000000000000000000000000000000000000000000000527B
      C600000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000073849C0000428C000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000004A9CC6008484840084A5
      AD0000000000000000000000000000000000738C94005AB5E700427B9C000000
      000000000000000000000000000000000000000000000000000000000000317B
      EF00527BC600296BC60000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000002173AD001873AD0029528400297BB50029A5D600295A
      8C00005294000863A50000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000006BADBD0073C6D6004A63
      6B0000000000000000000000000000000000636B6B00297B9C0029739C007B63
      63009C6B6B00000000000000000000000000000000000000000000000000397B
      E700007BFF000073F700527BC600000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000004AADDE0042B5E7000863A5002994CE0031ADDE00086B
      AD001094C6001094CE0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000528494005ABDF700426B
      8C0073636300000000009C737300AD737300AD6B6B0052848C0073CEE7008C73
      7B00D68484008C63630000000000000000000000000000000000000000000000
      0000009CFF00008CFF00008CFF00527BC6000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000021529400298CC6004ABDEF0042BDEF0042B5E70031ADDE0029A5
      DE0018A5D6001094C60000428C00526384000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000000000002184B5002973
      94006B4A4A009C6B6B00D6848400DE8C8C00C67B7B006B737B0084DEEF009484
      8C00DE8C8C00AD7373008C8C8C00000000000000000000000000000000000000
      00000000000000B5FF00008CFF000094FF00527BC600527BC600000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000214A940042ADE70052C6F7004ABDEF0063BDE7004A849C001873
      A5001094CE001094C600006BA500216394000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000AD737300528CA50073D6
      FF00635A5A00A56B6B00D6848400D6848400D68484007B636300C6CECE00B58C
      8C00D6848400BD7B7B00947B7B00000000000000000000000000000000000000
      0000000000000000000000B5FF0008C6FF00009CFF00009CFF00527BC6000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000006BC6E70052C6EF004ABDEF006BC6EF00DEDEDE006B6B6B00295A
      73001094C6000894CE00109CCE0063ADBD000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000BD7B7B008C84840084DE
      EF0073949C009C6B6B00C67B7B00C67B7B009C6B6B00A56B6B00CE8C8C00CE94
      9400CE949400C68C8C0094848400000000000000000000000000000000000000
      000000000000000000000000000000B5FF0008BDFF0000ADFF00009CFF00527B
      C600000000000000000000000000000000000000000000000000000000000000
      000000000000218C390039A5B5004ABDEF006BC6EF00DEDEDE006B6B6B00316B
      7B0018A5D600189CCE00189CCE00000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000AD7373009C6B6B00949C
      9C00A5A5A5009C6B6B009C6B6B00CE8C8C00DEA5A500E7ADAD00DEA5A500DEA5
      A500DEA5A500C68C8C009C848400000000000000000000000000000000000000
      0000527BC600527BC600527BC60000C6FF0008FFFF0031F7FF0010BDFF0000AD
      FF00527BC600527BC60000000000000000000000000063A55A00088C1000007B
      00003994290039CE520031AD9C0042B5DE0063C6D600D6D6D60063636300426B
      7B0029A5D60029A5D60000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000A56B6B00A56B6B00AD73
      7300CE9C9C00DEB5B500EFBDBD00EFB5B500DEA5A500E7A5A500EFADAD00EFAD
      AD00DEADAD00B58484008C8C8C00000000000000000000000000000000000000
      000029ADFF0000C6FF0000EFFF0000F7FF0000F7FF0000FFFF004AEFFF0018CE
      FF0000A5FF00527BC600000000000000000000000000299C29004ADE6B0021B5
      310018AD290039CE520018A5390018A55A0042B54200C6B59C00525252008C73
      630042A5C600189CCE0000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000AD7B7B00EFD6D600FFEF
      EF00FFE7E700EFCECE00E7C6C600EFC6C600F7C6C600EFBDBD00E7ADAD00EFAD
      AD00CE9C9C009C84840000000000000000000000000000000000000000000000
      000039A5FF0000C6FF0000EFFF0000F7FF0000EFFF0000DEFF0000FFFF0000FF
      FF0039EFFF0008C6FF00527BC6000000000000000000318C18004ADE6B004AE7
      730039D65A0039CE520029C6420021BD310018A51800219418006B8442000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000B5848400DECECE00FFF7
      F700FFF7F700FFE7E700EFCECE00EFD6D600F7CECE00F7C6C600F7C6C600DEAD
      AD009C8484000000000000000000000000000000000000000000000000000000
      00000000000008C6FF0039E7FF004AEFFF0042F7FF0018FFFF0000FFFF0000FF
      FF0008FFFF0021FFFF00527BC6000000000039A5390021B531004AE773004AE7
      730073D6840073B584001894210010A5180010A51800089C0800529429000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000B58C8C00CEAD
      AD00EFDEDE00FFEFEF00FFEFEF00FFE7E700EFC6C600EFB5B5009C8484009C84
      8400000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000031D6FF0008F7FF0000FFFF0000F7FF0000D6
      FF0000B5FF00527BC600000000000000000094E7A5006BF7940052EF7B004ADE
      6B00D6D6D600A5A5A500217B29000894080008A5100010A5180073C663000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000000000000000000000000000AD9C
      9C00C6ADAD00CEB5B500C6ADAD00BDA5A500BDA5A5009C8484008C8484007373
      7300000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000031D6FF0000F7FF0000EF
      FF0000ADFF0000A5FF00527BC600000000000000000042D663004ADE6B004ADE
      6B00D6D6D600A5A5A500399C420021BD310018A518004AA52900000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000A5A5A500FFFFFF008C8C8C007373
      7300000000000000000000000000000000000000000000000000000000000000
      00000000000000000000000000000000000000000000000000000000000031D6
      FF0042DEFF0010D6FF005AA5FF00527BC60000000000000000005AE77B004ADE
      6B00CECECE008C8C8C005A944A0052BD4A0063C6630000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000A5A5A500FFFFFF008C8C8C007373
      7300000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      00000000000031D6FF0052A5FF00527BC60000000000000000000000000042D6
      6300B5A58C00736B63008CB54A0021B529000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      0000000000000000000000000000000000000000000000000000000000000000
      000000000000000000000000000000000000424D3E000000000000003E000000
      2800000040000000100000000100010000000000800000000000000000000000
      000000000000000000000000FFFFFF00FF1FFFFFFFFF0000FF1FEFFFFF9F0000
      8F1FE3FFFC0300008F07E1FFFC0300008403F0FFF8000000C001F83FF8000000
      8001FC1FF80000008001FE0FF80100008001F003800300008001F00380030000
      8003F001801F00008007F801001F0000C00FFE03001F0000E00FFF81803F0000
      FF0FFFE0C07F0000FF0FFFF8E0FF000000000000000000000000000000000000
      000000000000}
  end
  object cxStyleRepository1: TcxStyleRepository
    Left = 38
    Top = 66
    PixelsPerInch = 96
    object cxStyle1: TcxStyle
      AssignedValues = [svColor]
      Color = clSilver
    end
  end
  object ColorDialog: TColorDialog
    Options = [cdAnyColor]
    Left = 908
    Top = 4
  end
  object usVersiune: TZUpdateSQL
    DeleteSQL.Strings = (
      'DELETE FROM BG_VERSIUNE'
      'WHERE'
      
        '  ((BG_VERSIUNE.ID_BG_VERSIUNE IS NULL AND :OLD_ID_BG_VERSIUNE I' +
        'S NULL) OR (BG_VERSIUNE.ID_BG_VERSIUNE = :OLD_ID_BG_VERSIUNE))')
    InsertSQL.Strings = (
      'INSERT INTO BG_VERSIUNE'
      '  (ID_OI_UNITATI, ID_OI_PROIECTE, ID_UTILIZATORI_CREAT, '
      'DEPARTAMENTE_CREAT, '
      '   DEPARTAMENTE_APROBAT, ID_UTILIZATORI_APROBAT, '
      'CLASA_FUNCTIONALA, AN_FISCAL, '
      '   REVIZIE, DATA_CREARE, DATA_APROBARE, FUNCTIE, VERSIUNE, '
      'EXPLICATIE, '
      '   ACT_APROBARE, TIP_BUGET, isBlocked)'
      'VALUES'
      '  (:ID_OI_UNITATI, :ID_OI_PROIECTE, :ID_UTILIZATORI_CREAT, '
      ':DEPARTAMENTE_CREAT, '
      '   :DEPARTAMENTE_APROBAT, :ID_UTILIZATORI_APROBAT, '
      ':CLASA_FUNCTIONALA, :AN_FISCAL, '
      '   :REVIZIE, :DATA_CREARE, :DATA_APROBARE, :FUNCTIE, '
      ':VERSIUNE, :EXPLICATIE, '
      '   :ACT_APROBARE, :TIP_BUGET, :isBlocked)')
    ModifySQL.Strings = (
      'UPDATE BG_VERSIUNE SET'
      '  ID_OI_UNITATI = :ID_OI_UNITATI,'
      '  ID_OI_PROIECTE = :ID_OI_PROIECTE,'
      '  ID_UTILIZATORI_CREAT = :ID_UTILIZATORI_CREAT,'
      '  DEPARTAMENTE_CREAT = :DEPARTAMENTE_CREAT,'
      '  DEPARTAMENTE_APROBAT = :DEPARTAMENTE_APROBAT,'
      '  ID_UTILIZATORI_APROBAT = :ID_UTILIZATORI_APROBAT,'
      '  CLASA_FUNCTIONALA = :CLASA_FUNCTIONALA,'
      '  AN_FISCAL = :AN_FISCAL,'
      '  REVIZIE = :REVIZIE,'
      '  DATA_CREARE = :DATA_CREARE,'
      '  DATA_APROBARE = :DATA_APROBARE,'
      '  FUNCTIE = :FUNCTIE,'
      '  VERSIUNE = :VERSIUNE,'
      '  EXPLICATIE = :EXPLICATIE,'
      '  ACT_APROBARE = :ACT_APROBARE,'
      '  TIP_BUGET = :TIP_BUGET,'
      '  isBlocked = :isBlocked'
      'WHERE'
      '  ((BG_VERSIUNE.ID_BG_VERSIUNE IS NULL AND '
      ':OLD_ID_BG_VERSIUNE IS NULL) OR '
      '(BG_VERSIUNE.ID_BG_VERSIUNE = :OLD_ID_BG_VERSIUNE))')
    UseSequenceFieldForRefreshSQL = False
    Left = 217
    Top = 282
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'ID_OI_UNITATI'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'ID_OI_PROIECTE'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'ID_UTILIZATORI_CREAT'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'DEPARTAMENTE_CREAT'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'DEPARTAMENTE_APROBAT'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'ID_UTILIZATORI_APROBAT'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'CLASA_FUNCTIONALA'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'AN_FISCAL'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'REVIZIE'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'DATA_CREARE'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'DATA_APROBARE'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'FUNCTIE'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'VERSIUNE'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'EXPLICATIE'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'ACT_APROBARE'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'TIP_BUGET'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'isBlocked'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'OLD_ID_BG_VERSIUNE'
        ParamType = ptUnknown
      end>
  end
  object qryTemp: TZQuery
    Connection = frmData.dbContabilitate
    UpdateObject = usVersiune
    SQL.Strings = (
      '')
    Params = <>
    Left = 147
    Top = 335
  end
  object TimerEnableControls: TTimer
    Enabled = False
    Interval = 500
    OnTimer = TimerEnableControlsTimer
    Left = 592
    Top = 200
  end
  object pmPreiaPlanificare: TPopupMenu
    OnPopup = pmPreiaPlanificarePopup
    Left = 592
    Top = 136
    object CmdPreia: TMenuItem
      OnClick = CmdPreiaClick
    end
  end
end
