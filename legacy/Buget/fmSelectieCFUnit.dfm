object fmSelectieCF: TfmSelectieCF
  Left = 316
  Top = 244
  BorderStyle = bsNone
  Caption = 'Selectie Clasificatie Functionala'
  ClientHeight = 423
  ClientWidth = 757
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
  object PageFunctional: TcxPageControl
    Left = 0
    Top = 0
    Width = 757
    Height = 423
    Align = alClient
    TabOrder = 0
    TabStop = False
    Properties.ActivePage = tabFunctional
    Properties.CustomButtons.Buttons = <>
    Properties.Style = 9
    Properties.TabPosition = tpBottom
    Properties.TabSlants.Kind = skCutCorner
    Properties.TabSlants.Positions = [spLeft, spRight]
    OnChange = PageFunctionalChange
    ClientRectBottom = 403
    ClientRectRight = 757
    ClientRectTop = 0
    object tabFunctional: TcxTabSheet
      Caption = 'Plan General'
      ImageIndex = 0
      object pnBugete: TPanel
        Left = 0
        Top = 0
        Width = 757
        Height = 403
        Align = alClient
        BevelOuter = bvNone
        TabOrder = 0
        object Panel7: TPanel
          Left = 0
          Top = 0
          Width = 757
          Height = 32
          Align = alTop
          BevelOuter = bvNone
          TabOrder = 1
          DesignSize = (
            757
            32)
          object lbTipBuget: TLabel
            Left = 3
            Top = 9
            Width = 71
            Height = 13
            Caption = 'Filtru Tip Buget'
          end
          object edtFiltruBuget: TcxImageComboBox
            Left = 80
            Top = 5
            TabStop = False
            Anchors = [akLeft, akTop, akRight]
            Properties.Items = <>
            Properties.OnChange = edtFiltruBugetPropertiesChange
            TabOrder = 0
            Width = 529
          end
          object chkDoarFolosite: TcxCheckBox
            Left = 616
            Top = 5
            Anchors = [akTop, akRight]
            Caption = 'Arata clasificatii folosite'
            Properties.OnEditValueChanged = chkDoarFolositePropertiesEditValueChanged
            State = cbsChecked
            TabOrder = 1
          end
        end
        object cxTreeBugete: TcxDBTreeList
          Left = 0
          Top = 32
          Width = 757
          Height = 371
          Align = alClient
          Bands = <
            item
              Caption.AlignHorz = taCenter
              FixedKind = tlbfLeft
              MinWidth = 300
            end
            item
              FixedKind = tlbfRight
            end
            item
              FixedKind = tlbfRight
            end>
          DataController.DataSource = dtCFDeAngajat
          DataController.ParentField = 'ID_PARINTE'
          DataController.KeyField = 'ID_BG_PLAN_FUNCTIONAL'
          FindPanel.DisplayMode = fpdmManual
          Navigator.Buttons.CustomButtons = <>
          OptionsBehavior.ImmediateEditor = False
          OptionsBehavior.ConfirmDelete = False
          OptionsBehavior.DragCollapse = False
          OptionsBehavior.ExpandOnIncSearch = True
          OptionsBehavior.IncSearch = True
          OptionsBehavior.ShowHourGlass = False
          OptionsCustomizing.BandCustomizing = False
          OptionsCustomizing.BandVertSizing = False
          OptionsCustomizing.ColumnsQuickCustomization = True
          OptionsCustomizing.ColumnVertSizing = False
          OptionsData.CancelOnExit = False
          OptionsData.Editing = False
          OptionsData.Deleting = False
          OptionsView.CellTextMaxLineCount = -1
          OptionsView.ColumnAutoWidth = True
          OptionsView.FixedSeparatorWidth = 1
          Preview.AutoHeight = False
          Preview.MaxLineCount = 2
          RootValue = -1
          ScrollbarAnnotations.CustomAnnotations = <>
          Styles.OnGetContentStyle = cxTreeBugeteStylesGetContentStyle
          TabOrder = 0
          OnDblClick = cxTreeBugeteDblClick
          OnKeyUp = cxTreeBugeteKeyUp
          object cxTreeBugeteid_bg_plan_functional: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'id_bg_plan_functional'
            Width = 100
            Position.ColIndex = 0
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeBugeteid_parinte: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'id_parinte'
            Width = 100
            Position.ColIndex = 1
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeBugetecod_functional: TcxDBTreeListColumn
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Cod'
            DataBinding.FieldName = 'cod_functional'
            Width = 100
            Position.ColIndex = 2
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeBugetecod_ecran: TcxDBTreeListColumn
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
          object cxTreeBugeteid_oi_unitati: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'id_oi_unitati'
            Width = 100
            Position.ColIndex = 4
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeBugetedenumire: TcxDBTreeListColumn
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Denumire Buget'
            DataBinding.FieldName = 'denumire'
            Width = 250
            Position.ColIndex = 5
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeBugetedescriere: TcxDBTreeListColumn
            Caption.AlignHorz = taCenter
            Caption.Text = 'Buget'
            Width = 447
            Position.ColIndex = 6
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
            OnGetDisplayText = cxTreeBugeteDESCRIEREGetDisplayText
          end
          object cxTreeBugetetip: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'tip'
            Width = 100
            Position.ColIndex = 7
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeBugeteca: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.DisplayFormat = ',0.00;(,0.00)'
            Caption.AlignHorz = taCenter
            Caption.Text = 'Credit Ang'
            DataBinding.FieldName = 'ca'
            Width = 55
            Position.ColIndex = 0
            Position.RowIndex = 0
            Position.BandIndex = 1
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeBugeteang_legal: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.DisplayFormat = ',0.00;(,0.00)'
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Ang Legal'
            DataBinding.FieldName = 'ang_legal'
            Width = 100
            Position.ColIndex = 2
            Position.RowIndex = 0
            Position.BandIndex = 1
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeBugeteang_legal_anual: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.DisplayFormat = ',0.00;(,0.00)'
            Visible = False
            Caption.AlignHorz = taCenter
            Caption.Text = 'Ang Legal An'
            DataBinding.FieldName = 'ang_legal_anual'
            Width = 100
            Position.ColIndex = 3
            Position.RowIndex = 0
            Position.BandIndex = 1
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeBugeteang_legal_multi: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.DisplayFormat = ',0.00;(,0.00)'
            Caption.AlignHorz = taCenter
            Caption.Text = 'Ang Legal Multi'
            DataBinding.FieldName = 'ang_legal_multi'
            Width = 56
            Position.ColIndex = 1
            Position.RowIndex = 0
            Position.BandIndex = 1
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeBugeteplanificat: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.DisplayFormat = ',0.00;(,0.00)'
            Caption.AlignHorz = taCenter
            Caption.Text = 'Credit Bug.'
            DataBinding.FieldName = 'planificat'
            Width = 55
            Position.ColIndex = 0
            Position.RowIndex = 0
            Position.BandIndex = 2
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeBugeteang_bugetar: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.DisplayFormat = ',0.00;(,0.00)'
            Caption.AlignHorz = taCenter
            Caption.Text = 'Ang Bugetar'
            DataBinding.FieldName = 'ang_bugetar'
            Width = 56
            Position.ColIndex = 1
            Position.RowIndex = 0
            Position.BandIndex = 2
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeBugeteang_bug_anual: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.DisplayFormat = ',0.00;(,0.00)'
            Caption.AlignHorz = taCenter
            Caption.Text = 'Ang Bug An'
            DataBinding.FieldName = 'ang_bug_anual'
            Width = 56
            Position.ColIndex = 3
            Position.RowIndex = 0
            Position.BandIndex = 2
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeBugeteang_bug_multi: TcxDBTreeListColumn
            PropertiesClassName = 'TcxCurrencyEditProperties'
            Properties.DisplayFormat = ',0.00;(,0.00)'
            Caption.AlignHorz = taCenter
            Caption.Text = 'Ang Bug Multi'
            DataBinding.FieldName = 'ang_bug_multi'
            Width = 55
            Position.ColIndex = 4
            Position.RowIndex = 0
            Position.BandIndex = 2
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeBugetenerepartizat: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'nerepartizat'
            Width = 100
            Position.ColIndex = 8
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeBugetefolosit: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'folosit'
            Width = 100
            Position.ColIndex = 9
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeBugeteid_bg_tipuri_buget: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'id_bg_tipuri_buget'
            Width = 100
            Position.ColIndex = 10
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeBugeteid_analitic: TcxDBTreeListColumn
            Visible = False
            DataBinding.FieldName = 'id_analitic'
            Width = 100
            Position.ColIndex = 11
            Position.RowIndex = 0
            Position.BandIndex = 0
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeBugeteprocCA: TcxDBTreeListColumn
            PropertiesClassName = 'TcxProgressBarProperties'
            Properties.ShowOverload = True
            Styles.Content = stilProcent
            Caption.AlignHorz = taCenter
            Caption.Text = '% CA'
            DataBinding.FieldName = 'procCA'
            Width = 55
            Position.ColIndex = 4
            Position.RowIndex = 0
            Position.BandIndex = 1
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
          object cxTreeBugeteprocBuget: TcxDBTreeListColumn
            PropertiesClassName = 'TcxProgressBarProperties'
            Properties.ShowOverload = True
            Caption.AlignHorz = taCenter
            Caption.Text = '% Bug'
            DataBinding.FieldName = 'procBuget'
            Width = 54
            Position.ColIndex = 2
            Position.RowIndex = 0
            Position.BandIndex = 2
            Summary.FooterSummaryItems = <>
            Summary.GroupFooterSummaryItems = <>
          end
        end
      end
    end
    object tabFunctionalContract: TcxTabSheet
      Caption = 'Conform contract'
      ImageIndex = 1
      object TreeDetaliiContract: TcxDBTreeList
        Left = 0
        Top = 0
        Width = 757
        Height = 403
        Align = alClient
        Bands = <
          item
            Caption.AlignHorz = taCenter
          end>
        DataController.DataSource = DTContracteInfo
        DataController.ParentField = 'id_parinte'
        DataController.KeyField = 'id'
        Navigator.Buttons.CustomButtons = <>
        OptionsBehavior.ImmediateEditor = False
        OptionsBehavior.AutoDragCopy = True
        OptionsBehavior.ConfirmDelete = False
        OptionsBehavior.DragCollapse = False
        OptionsBehavior.DragExpand = False
        OptionsBehavior.ExpandOnIncSearch = True
        OptionsBehavior.IncSearch = True
        OptionsBehavior.ShowHourGlass = False
        OptionsCustomizing.BandCustomizing = False
        OptionsCustomizing.BandVertSizing = False
        OptionsCustomizing.ColumnsQuickCustomization = True
        OptionsCustomizing.ColumnVertSizing = False
        OptionsData.CancelOnExit = False
        OptionsData.Editing = False
        OptionsData.Appending = True
        OptionsData.Deleting = False
        OptionsData.Inserting = True
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
        OnDblClick = cxTreeBugeteDblClick
        OnKeyUp = cxTreeBugeteKeyUp
        object TreeDetaliiContractid: TcxDBTreeListColumn
          Visible = False
          Caption.AlignHorz = taCenter
          DataBinding.FieldName = 'id'
          Width = 100
          Position.ColIndex = 0
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDetaliiContractid_parinte: TcxDBTreeListColumn
          Visible = False
          Caption.AlignHorz = taCenter
          DataBinding.FieldName = 'id_parinte'
          Width = 100
          Position.ColIndex = 1
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDetaliiContractcod_functional: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taRightJustify
          Caption.AlignHorz = taCenter
          Caption.Text = 'Cod Functional'
          DataBinding.FieldName = 'cod_functional'
          Width = 100
          Position.ColIndex = 7
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDetaliiContractid_oi_unitati: TcxDBTreeListColumn
          Visible = False
          Caption.AlignHorz = taCenter
          DataBinding.FieldName = 'id_oi_unitati'
          Width = 100
          Position.ColIndex = 2
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDetaliiContractcod_economic: TcxDBTreeListColumn
          Visible = False
          Caption.AlignHorz = taCenter
          DataBinding.FieldName = 'cod_economic'
          Width = 100
          Position.ColIndex = 3
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDetaliiContractid_oi_proiecte: TcxDBTreeListColumn
          Visible = False
          Caption.AlignHorz = taCenter
          Caption.Text = 'Id Proiect'
          DataBinding.FieldName = 'id_oi_proiecte'
          Width = 100
          Position.ColIndex = 4
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDetaliiContractcod_ecran: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taRightJustify
          Caption.AlignHorz = taCenter
          Caption.Text = 'Cod Ecran'
          DataBinding.FieldName = 'cod_ecran'
          Width = 74
          Position.ColIndex = 8
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDetaliiContractcod_proiect: TcxDBTreeListColumn
          PropertiesClassName = 'TcxMaskEditProperties'
          Properties.Alignment.Horz = taRightJustify
          Caption.AlignHorz = taCenter
          Caption.Text = 'Cod Proiect'
          DataBinding.FieldName = 'cod_proiect'
          Width = 74
          Position.ColIndex = 9
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDetaliiContractNume: TcxDBTreeListColumn
          Caption.AlignHorz = taCenter
          DataBinding.FieldName = 'Nume'
          Width = 332
          Position.ColIndex = 5
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDetaliiContractValoare: TcxDBTreeListColumn
          Caption.AlignHorz = taCenter
          DataBinding.FieldName = 'Valoare'
          Width = 87
          Position.ColIndex = 6
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeDetaliiContractidFurnizor: TcxDBTreeListColumn
          Visible = False
          Caption.AlignHorz = taCenter
          DataBinding.FieldName = 'idFurnizor'
          Width = 100
          Position.ColIndex = 10
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
      end
    end
  end
  object qryContracteInfo: TZReadOnlyQuery
    SQL.Strings = (
      'exec spAlopAngInfoContract :idContract, :dataAng')
    Params = <
      item
        DataType = ftUnknown
        Name = 'idContract'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'dataAng'
        ParamType = ptUnknown
      end>
    Left = 137
    Top = 245
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'idContract'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'dataAng'
        ParamType = ptUnknown
      end>
  end
  object DTContracteInfo: TDataSource
    DataSet = qryContracteInfo
    Left = 48
    Top = 245
  end
  object dtCFDeAngajat: TDataSource
    DataSet = qryCFDeAngajat
    Left = 48
    Top = 297
  end
  object qryCFDeAngajat: TZReadOnlyQuery
    SQL.Strings = (
      'exec spAlopBugetGetCFAngajat :dataAng, :doarFolosit, :refUser')
    Params = <
      item
        DataType = ftUnknown
        Name = 'dataAng'
        ParamType = ptUnknown
      end
      item
        DataType = ftBoolean
        Name = 'doarFolosit'
        ParamType = ptInput
        Value = 'True'
      end
      item
        DataType = ftUnknown
        Name = 'refUser'
        ParamType = ptUnknown
      end>
    Left = 137
    Top = 297
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'dataAng'
        ParamType = ptUnknown
      end
      item
        DataType = ftBoolean
        Name = 'doarFolosit'
        ParamType = ptInput
        Value = 'True'
      end
      item
        DataType = ftUnknown
        Name = 'refUser'
        ParamType = ptUnknown
      end>
  end
  object stiluriCF: TcxStyleRepository
    Left = 304
    Top = 280
    PixelsPerInch = 96
    object stilProcent: TcxStyle
      AssignedValues = [svColor]
      Color = clAqua
    end
    object stilNormal: TcxStyle
      AssignedValues = [svFont]
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = []
    end
  end
end
