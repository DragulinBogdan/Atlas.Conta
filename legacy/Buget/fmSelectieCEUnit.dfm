object fmSelectieCE: TfmSelectieCE
  Left = 523
  Top = 267
  BorderStyle = bsNone
  Caption = 'Selectie Clasificatie Economica'
  ClientHeight = 479
  ClientWidth = 830
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
  object pageEconomic: TcxPageControl
    Left = 0
    Top = 0
    Width = 830
    Height = 400
    Align = alClient
    TabOrder = 0
    TabStop = False
    Properties.ActivePage = tabEconomic
    Properties.CustomButtons.Buttons = <>
    Properties.Style = 9
    Properties.TabPosition = tpBottom
    Properties.TabSlants.Kind = skCutCorner
    Properties.TabSlants.Positions = [spLeft, spRight]
    LookAndFeel.Kind = lfOffice11
    OnChange = pageEconomicChange
    ClientRectBottom = 380
    ClientRectRight = 830
    ClientRectTop = 0
    object tabEconomic: TcxTabSheet
      Caption = 'Plan General'
      ImageIndex = 0
      object pnBugete: TPanel
        Left = 0
        Top = 0
        Width = 830
        Height = 380
        Align = alClient
        BevelOuter = bvNone
        TabOrder = 0
        object Panel7: TPanel
          Left = 0
          Top = 0
          Width = 830
          Height = 32
          Align = alTop
          BevelOuter = bvNone
          TabOrder = 1
          DesignSize = (
            830
            32)
          object chkDoarFolosite: TcxCheckBox
            Left = 689
            Top = 5
            Anchors = [akTop, akRight]
            Caption = 'Arata clasificatii folosite'
            Properties.OnEditValueChanged = chkDoarFolositePropertiesEditValueChanged
            State = cbsChecked
            TabOrder = 0
          end
        end
        object cxTreeEconomic: TcxDBTreeList
          Left = 0
          Top = 32
          Width = 830
          Height = 348
          Align = alClient
          Bands = <
            item
              Caption.AlignHorz = taCenter
              MinWidth = 350
            end
            item
              FixedKind = tlbfRight
              Visible = False
            end
            item
              FixedKind = tlbfRight
              Visible = False
            end>
          DataController.DataSource = dtCEDeAngajat
          DataController.ParentField = 'id_parinte'
          DataController.KeyField = 'id_bg_plan_economic'
          LookAndFeel.Kind = lfOffice11
          Navigator.Buttons.CustomButtons = <>
          OptionsBehavior.ImmediateEditor = False
          OptionsBehavior.ConfirmDelete = False
          OptionsBehavior.DragCollapse = False
          OptionsBehavior.ExpandOnIncSearch = True
          OptionsBehavior.IncSearch = True
          OptionsBehavior.ShowHourGlass = False
          OptionsCustomizing.BandsQuickCustomization = True
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
          ParentColor = False
          Preview.AutoHeight = False
          Preview.MaxLineCount = 2
          RootValue = Null
          ScrollbarAnnotations.CustomAnnotations = <>
          TabOrder = 0
          OnDblClick = cxTreeEconomicDblClick
          OnKeyUp = cxTreeEconomicKeyUp
          object cxTreeEconomicid_bg_plan_economic: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'id_bg_plan_economic'
            Width = 100
            Position.ColIndex = 0
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicid_parinte: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'id_parinte'
            Width = 100
            Position.ColIndex = 1
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomiccod_economic: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'cod_economic'
            Width = 100
            Position.ColIndex = 2
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomiccod_ecran: TcxDBTreeListColumn
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Cod Buget'
            DataBinding.FieldName = 'cod_ecran'
            Width = 100
            Position.ColIndex = 3
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomiccod_functional: TcxDBTreeListColumn
            Visible = False
            Caption.Text = 'Cod Functional'
            DataBinding.FieldName = 'cod_functional'
            Width = 100
            Position.ColIndex = 14
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicid_oi_proiecte: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'id_oi_proiecte'
            Width = 100
            Position.ColIndex = 4
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicdenumire: TcxDBTreeListColumn
            Visible = False
            Caption.Text = 'Denumire Buget'
            DataBinding.FieldName = 'denumire'
            Width = 250
            Position.ColIndex = 5
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicdescriere: TcxDBTreeListColumn
            Caption.AlignHorz = taCenter
            Caption.Text = 'Titlu/Articol'
            DataBinding.FieldName = 'cod_ecran'
            Width = 505
            Position.ColIndex = 6
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
            OnGetDisplayText = cxTreeEconomicdescriereGetDisplayText
          end
          object cxTreeEconomictip: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'tip'
            Width = 100
            Position.ColIndex = 7
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicfolosit: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'folosit'
            Width = 100
            Position.ColIndex = 8
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicid_bg_tipuri_buget: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'id_bg_tipuri_buget'
            Width = 100
            Position.ColIndex = 9
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicid_analitic: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'id_analitic'
            Width = 100
            Position.ColIndex = 10
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicCLASA: TcxDBTreeListColumn
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Clasa'
            DataBinding.FieldName = 'clasa'
            Width = 100
            Position.ColIndex = 11
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicid_oi_unitati: TcxDBTreeListColumn
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Unitate'
            DataBinding.FieldName = 'id_oi_unitati'
            Width = 100
            Position.ColIndex = 12
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomiceste_procentual: TcxDBTreeListColumn
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Procentual'
            DataBinding.FieldName = 'este_procentual'
            Width = 100
            Position.ColIndex = 13
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicprevederi: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.DisplayFormat = ',0.00;(-,0.00)'
            Caption.AlignHorz = taCenter
            Caption.Text = 'Prevederi'
            DataBinding.FieldName = 'prevederi'
            Width = 230
            Position.ColIndex = 15
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicanterior: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.DisplayFormat = ',0.00;(-,0.00)'
            Caption.AlignHorz = taCenter
            Caption.Text = 'Anterior'
            DataBinding.FieldName = 'anterior'
            Width = 185
            Position.ColIndex = 16
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeEconomicprocAngajat: TcxDBTreeListColumn
            PropertiesClassName = 'TcxProgressBarProperties'
            Caption.AlignHorz = taCenter
            Caption.Text = '%'
            DataBinding.FieldName = 'procAngajat'
            Width = 100
            Position.ColIndex = 17
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
        end
      end
    end
    object tabEconomicProiect: TcxTabSheet
      Caption = 'Conform Proiect'
      ImageIndex = 1
      object gridProiecte: TcxGrid
        Left = 0
        Top = 32
        Width = 830
        Height = 348
        Align = alClient
        TabOrder = 0
        object viewProiecte: TcxGridDBBandedTableView
          OnDblClick = viewProiecteDblClick
          OnKeyUp = viewProiecteKeyUp
          Navigator.Buttons.CustomButtons = <>
          ScrollbarAnnotations.CustomAnnotations = <>
          DataController.DataSource = dtProiectList
          DataController.Summary.DefaultGroupSummaryItems = <>
          DataController.Summary.FooterSummaryItems = <>
          DataController.Summary.SummaryGroups = <>
          OptionsBehavior.IncSearch = True
          OptionsBehavior.IncSearchItem = viewProiectenumeProiect
          OptionsData.CancelOnExit = False
          OptionsData.Deleting = False
          OptionsData.DeletingConfirmation = False
          OptionsData.Inserting = False
          OptionsView.ColumnAutoWidth = True
          OptionsView.GroupByBox = False
          Preview.Column = viewProiectedescProiect
          Bands = <
            item
              Caption = 'Informatii Proiect'
              Width = 308
            end
            item
              Caption = 'Credite de angajamente'
              Width = 309
            end
            item
              Caption = 'Credite bugetare'
              Width = 315
            end>
          object viewProiectenumeProiect: TcxGridDBBandedColumn
            Caption = 'Proiect'
            DataBinding.FieldName = 'numeProiect'
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            SortIndex = 0
            SortOrder = soAscending
            Width = 228
            Position.BandIndex = 0
            Position.ColIndex = 0
            Position.RowIndex = 0
          end
          object viewProiectedescProiect: TcxGridDBBandedColumn
            DataBinding.FieldName = 'descProiect'
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 150
            Position.BandIndex = 0
            Position.ColIndex = 1
            Position.RowIndex = 0
          end
          object viewProiectenumeUnitate: TcxGridDBBandedColumn
            DataBinding.FieldName = 'numeUnitate'
            Visible = False
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 150
            Position.BandIndex = 0
            Position.ColIndex = 2
            Position.RowIndex = 0
          end
          object viewProiectedescUnitate: TcxGridDBBandedColumn
            DataBinding.FieldName = 'descUnitate'
            Visible = False
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 150
            Position.BandIndex = 0
            Position.ColIndex = 3
            Position.RowIndex = 0
          end
          object viewProiecteesteProcentual: TcxGridDBBandedColumn
            Caption = 'Are %'
            DataBinding.FieldName = 'esteProcentual'
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 41
            Position.BandIndex = 0
            Position.ColIndex = 2
            Position.RowIndex = 2
          end
          object viewProiectecod_functional: TcxGridDBBandedColumn
            Caption = 'Cod Functional'
            DataBinding.FieldName = 'cod_functional'
            Visible = False
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 148
            Position.BandIndex = 0
            Position.ColIndex = 0
            Position.RowIndex = 1
          end
          object viewProiectecod_economic: TcxGridDBBandedColumn
            Caption = 'Cod Economic'
            DataBinding.FieldName = 'cod_economic'
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 164
            Position.BandIndex = 0
            Position.ColIndex = 0
            Position.RowIndex = 2
          end
          object viewProiecteid_oi_unitati: TcxGridDBBandedColumn
            DataBinding.FieldName = 'id_oi_unitati'
            Visible = False
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Position.BandIndex = 0
            Position.ColIndex = 4
            Position.RowIndex = 0
          end
          object viewProiecteid_oi_proiecte: TcxGridDBBandedColumn
            DataBinding.FieldName = 'id_oi_proiecte'
            Visible = False
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Position.BandIndex = 0
            Position.ColIndex = 5
            Position.RowIndex = 0
          end
          object viewProiecteangajat: TcxGridDBBandedColumn
            Caption = 'Se Angajeaza'
            DataBinding.FieldName = 'angajat'
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Visible = False
            HeaderAlignmentHorz = taCenter
            Width = 94
            Position.BandIndex = 0
            Position.ColIndex = 1
            Position.RowIndex = 2
          end
          object viewProiecteplanificat: TcxGridDBBandedColumn
            Caption = 'Planificat'
            DataBinding.FieldName = 'planificat'
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 126
            Position.BandIndex = 2
            Position.ColIndex = 0
            Position.RowIndex = 0
          end
          object viewProiecteca: TcxGridDBBandedColumn
            Caption = 'Credite Ang'
            DataBinding.FieldName = 'ca'
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Position.BandIndex = 1
            Position.ColIndex = 0
            Position.RowIndex = 0
          end
          object viewProiecteang_legal_anual: TcxGridDBBandedColumn
            Caption = 'Ang Leg Anual'
            DataBinding.FieldName = 'ang_legal_anual'
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 88
            Position.BandIndex = 1
            Position.ColIndex = 0
            Position.RowIndex = 1
          end
          object viewProiecteang_legal_multi: TcxGridDBBandedColumn
            Caption = 'Ang Legal Multi'
            DataBinding.FieldName = 'ang_legal_multi'
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 96
            Position.BandIndex = 1
            Position.ColIndex = 1
            Position.RowIndex = 1
          end
          object viewProiecteang_bug_anual: TcxGridDBBandedColumn
            Caption = 'Ang Bug Anual'
            DataBinding.FieldName = 'ang_bug_anual'
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Position.BandIndex = 2
            Position.ColIndex = 0
            Position.RowIndex = 1
          end
          object viewProiecteang_bug_multi: TcxGridDBBandedColumn
            Caption = 'Ang Bug Multi'
            DataBinding.FieldName = 'ang_bug_multi'
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Position.BandIndex = 2
            Position.ColIndex = 1
            Position.RowIndex = 1
          end
          object viewProiecteang_legal: TcxGridDBBandedColumn
            Caption = 'Ang Legal'
            DataBinding.FieldName = 'ang_legal'
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Position.BandIndex = 1
            Position.ColIndex = 1
            Position.RowIndex = 0
          end
          object viewProiecteang_bugetar: TcxGridDBBandedColumn
            Caption = 'Ang Bugetar'
            DataBinding.FieldName = 'ang_bugetar'
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 110
            Position.BandIndex = 2
            Position.ColIndex = 1
            Position.RowIndex = 0
          end
          object viewProiecteprocCA: TcxGridDBBandedColumn
            Caption = '% CA'
            DataBinding.FieldName = 'procCA'
            PropertiesClassName = 'TcxProgressBarProperties'
            Properties.PeakValue = 0.094000000000000000
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Width = 80
            Position.BandIndex = 1
            Position.ColIndex = 2
            Position.RowIndex = 1
          end
          object viewProiecteprocBuget: TcxGridDBBandedColumn
            Caption = '% Buget'
            DataBinding.FieldName = 'procBuget'
            PropertiesClassName = 'TcxProgressBarProperties'
            Properties.PeakValue = 100.000000000000000000
            HeaderAlignmentHorz = taCenter
            Options.Editing = False
            Position.BandIndex = 2
            Position.ColIndex = 2
            Position.RowIndex = 1
          end
        end
        object nivelProiecte: TcxGridLevel
          GridView = viewProiecte
        end
      end
      object Panel1: TPanel
        Left = 0
        Top = 0
        Width = 830
        Height = 32
        Align = alTop
        BevelOuter = bvNone
        TabOrder = 1
        object lbSumaProiect: TcxLabel
          Left = 16
          Top = 8
          Caption = 'Suma in cadrul proiectului '
        end
        object edSumaProiect: TcxCurrencyEdit
          Left = 152
          Top = 5
          Properties.Alignment.Horz = taRightJustify
          Properties.DisplayFormat = ',0.00;-,0.00'
          Properties.UseDisplayFormatWhenEditing = True
          Properties.UseLeftAlignmentOnEditing = False
          Properties.UseThousandSeparator = True
          TabOrder = 1
          Width = 121
        end
      end
    end
  end
  object pnBottom: TPanel
    Left = 0
    Top = 400
    Width = 830
    Height = 79
    Align = alBottom
    TabOrder = 1
    DesignSize = (
      830
      79)
    object btnCancel: TcxButton
      Left = 745
      Top = 8
      Width = 75
      Height = 25
      Anchors = [akTop, akRight]
      Caption = 'Abandon'
      ModalResult = 2
      TabOrder = 0
    end
    object btnOk: TcxButton
      Left = 657
      Top = 8
      Width = 75
      Height = 25
      Anchors = [akTop, akRight]
      Caption = 'Ok'
      TabOrder = 1
      OnClick = btnOkClick
    end
  end
  object stiluriCF: TcxStyleRepository
    Left = 120
    Top = 152
    PixelsPerInch = 96
    object stilProcent: TcxStyle
      AssignedValues = [svColor]
      Color = clAqua
    end
    object stilUnitate: TcxStyle
      AssignedValues = [svFont]
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
    end
  end
  object dtCEDeAngajat: TDataSource
    DataSet = qryCEDeAngajat
    Left = 88
    Top = 113
  end
  object qryCEDeAngajat: TZReadOnlyQuery
    SQL.Strings = (
      
        'exec spAlopBugetGetCEAngajat :cod_functional, :dataAng, :id_oi_u' +
        'nitati, :id_oi_proiecte, :decatFolosit, :re' +
        'fUser')
    Params = <
      item
        DataType = ftUnknown
        Name = 'cod_functional'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'dataAng'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'id_oi_unitati'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'id_oi_proiecte'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'id_alop_angajament'
        ParamType = ptUnknown
      end
      item
        DataType = ftBoolean
        Name = 'decatFolosit'
        ParamType = ptInput
        Value = True
      end
      item
        DataType = ftUnknown
        Name = 'refUser'
        ParamType = ptUnknown
      end>
    Left = 120
    Top = 113
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'cod_functional'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'dataAng'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'id_oi_unitati'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'id_oi_proiecte'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'id_alop_angajament'
        ParamType = ptUnknown
      end
      item
        DataType = ftBoolean
        Name = 'decatFolosit'
        ParamType = ptInput
        Value = True
      end
      item
        DataType = ftUnknown
        Name = 'refUser'
        ParamType = ptUnknown
      end>
  end
  object qryProiectList: TZQuery
    AfterOpen = qryProiectListAfterOpen
    CachedUpdates = True
    SQL.Strings = (
      
        'exec spAlopBugetGetProiectAngajat :cod_functional, :dataAng, :id' +
        '_oi_unitati, :id_oi_proiecte, :decatFolosit')
    Params = <
      item
        DataType = ftString
        Name = 'cod_functional'
        ParamType = ptInput
        Value = '51.01.01.03'
      end
      item
        DataType = ftUnknown
        Name = 'dataAng'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'id_oi_unitati'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'id_oi_proiecte'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'decatFolosit'
        ParamType = ptUnknown
      end>
    Left = 120
    Top = 81
    ParamData = <
      item
        DataType = ftString
        Name = 'cod_functional'
        ParamType = ptInput
        Value = '51.01.01.03'
      end
      item
        DataType = ftUnknown
        Name = 'dataAng'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'id_oi_unitati'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'id_oi_proiecte'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'decatFolosit'
        ParamType = ptUnknown
      end>
  end
  object dtProiectList: TDataSource
    DataSet = qryProiectList
    Left = 88
    Top = 81
  end
end
