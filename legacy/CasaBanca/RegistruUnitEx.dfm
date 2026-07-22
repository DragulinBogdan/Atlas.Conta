object FrmRegistruEx: TFrmRegistruEx
  Left = 270
  Top = 140
  Caption = 'Registru Casa / Banca'
  ClientHeight = 652
  ClientWidth = 1025
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  ShowHint = True
  WindowState = wsMaximized
  OnClose = FormClose
  PixelsPerInch = 96
  TextHeight = 13
  object Splitter2: TSplitter
    Left = 0
    Top = 589
    Width = 1025
    Height = 1
    Cursor = crVSplit
    Align = alBottom
    ExplicitTop = 609
  end
  object pnRest: TPanel
    Left = 0
    Top = 0
    Width = 1025
    Height = 589
    Align = alClient
    BevelOuter = bvNone
    TabOrder = 0
    object treeRegistru: TcxDBTreeList
      Left = 0
      Top = 47
      Width = 1025
      Height = 542
      Align = alClient
      Bands = <
        item
          Caption.AlignHorz = taCenter
          Caption.Text = 'Stare'
          FixedKind = tlbfLeft
          Options.Customizing = False
          Options.Moving = False
          Options.Sizing = False
          Width = 180
        end
        item
          Caption.AlignHorz = taCenter
          Caption.Text = 'Valori'
          Width = 879
        end
        item
          Caption.AlignHorz = taCenter
          Caption.Text = 'Cont'
          Visible = False
        end
        item
          Caption.AlignHorz = taCenter
          Caption.Text = 'Proiect'
          Options.Customizing = False
          Options.Moving = False
          Visible = False
        end
        item
          Caption.AlignHorz = taCenter
          Caption.Text = 'HiddenProj'
          Visible = False
        end
        item
          Caption.AlignHorz = taCenter
          Caption.Text = 'Facturi'
          Visible = False
        end>
      DataController.DataSource = dtRegistru
      DataController.ParentField = 'refParinte'
      DataController.KeyField = 'idRegistru'
      Navigator.Buttons.CustomButtons = <>
      OptionsCustomizing.ColumnsQuickCustomization = True
      OptionsView.Bands = True
      OptionsView.ColumnAutoWidth = True
      OptionsView.Footer = True
      PopupMenus.ColumnHeaderMenu.UseBuiltInMenu = True
      PopupMenus.FooterMenu.UseBuiltInMenu = True
      PopupMenus.GroupFooterMenu.UseBuiltInMenu = True
      RootValue = -1
      ScrollbarAnnotations.CustomAnnotations = <>
      Styles.Content = stilPrimulNivel
      TabOrder = 1
      object treeRegistruidRegistru: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'idRegistru'
        Width = 100
        Position.ColIndex = 0
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrurefInitial: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'refInitial'
        Width = 100
        Position.ColIndex = 1
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrurefParinte: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'refParinte'
        Width = 100
        Position.ColIndex = 2
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrunumeTipDoc: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'numeTipDoc'
        Width = 88
        Position.ColIndex = 0
        Position.RowIndex = 0
        Position.BandIndex = 1
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrunumarDoc: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'numarDoc'
        Width = 88
        Position.ColIndex = 1
        Position.RowIndex = 0
        Position.BandIndex = 1
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrudescScurta: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'descScurta'
        Width = 88
        Position.ColIndex = 2
        Position.RowIndex = 0
        Position.BandIndex = 1
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrudescLunga: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'descLunga'
        Width = 100
        Position.ColIndex = 3
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrucursSchimb: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'cursSchimb'
        Width = 87
        Position.ColIndex = 3
        Position.RowIndex = 0
        Position.BandIndex = 1
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistruvalIncasare: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'valIncasare'
        Width = 89
        Position.ColIndex = 4
        Position.RowIndex = 0
        Position.BandIndex = 1
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistruvalPlata: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'valPlata'
        Width = 88
        Position.ColIndex = 5
        Position.RowIndex = 0
        Position.BandIndex = 1
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistruisOnCredit: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'isOnCredit'
        Width = 87
        Position.ColIndex = 6
        Position.RowIndex = 0
        Position.BandIndex = 1
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrusemnSuma: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'semnSuma'
        Width = 88
        Position.ColIndex = 7
        Position.RowIndex = 0
        Position.BandIndex = 1
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrusold: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'sold'
        Width = 88
        Position.ColIndex = 8
        Position.RowIndex = 0
        Position.BandIndex = 1
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrusoldRON: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'soldRON'
        Width = 88
        Position.ColIndex = 9
        Position.RowIndex = 0
        Position.BandIndex = 1
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistruechilibrata: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        Caption.Text = 'Ecl'
        DataBinding.FieldName = 'echilibrata'
        Width = 34
        Position.ColIndex = 4
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistruvalidata: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        Caption.Text = 'Val'
        DataBinding.FieldName = 'validata'
        Width = 29
        Position.ColIndex = 5
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrupozRegistru: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        Caption.Text = 'Poz'
        DataBinding.FieldName = 'pozRegistru'
        Width = 48
        Position.ColIndex = 19
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrudataRegistru: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        Caption.Text = 'Data'
        DataBinding.FieldName = 'dataRegistru'
        Width = 110
        Position.ColIndex = 20
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrurefTipTransfer: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'refTipTransfer'
        Width = 100
        Position.ColIndex = 6
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrurefTransfer: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'refTransfer'
        Width = 100
        Position.ColIndex = 7
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrurefCasaTransfer: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'refCasaTransfer'
        Width = 100
        Position.ColIndex = 8
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrucontCorespondent: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'contCorespondent'
        Width = 100
        Position.ColIndex = 0
        Position.RowIndex = 0
        Position.BandIndex = 2
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrurefRepartitor: TcxDBTreeListColumn
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'refRepartitor'
        Width = 100
        Position.ColIndex = 1
        Position.RowIndex = 0
        Position.BandIndex = 2
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrudataEmitere: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'dataEmitere'
        Width = 100
        Position.ColIndex = 9
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrunrDecont: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'nrDecont'
        Width = 100
        Position.ColIndex = 10
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrudataDecont: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'dataDecont'
        Width = 100
        Position.ColIndex = 11
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrurefUserAdaugare: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'refUserAdaugare'
        Width = 100
        Position.ColIndex = 12
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrurefUserValidare: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'refUserValidare'
        Width = 100
        Position.ColIndex = 13
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistruhashValidare: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'hashValidare'
        Width = 100
        Position.ColIndex = 14
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrunumarExtras: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'numarExtras'
        Width = 100
        Position.ColIndex = 15
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrudataExtras: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'dataExtras'
        Width = 100
        Position.ColIndex = 16
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistruversiuneRand: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'versiuneRand'
        Width = 100
        Position.ColIndex = 17
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeRegistrurefCasierie: TcxDBTreeListColumn
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'refCasierie'
        Width = 100
        Position.ColIndex = 18
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
    end
    object pnTop: TcxGroupBox
      Left = 0
      Top = 0
      Align = alTop
      Caption = 'Casa/ Banca / Trezorerie : '
      TabOrder = 0
      DesignSize = (
        1025
        47)
      Height = 47
      Width = 1025
      object edCurentHouse: TcxPopupEdit
        Left = 16
        Top = 15
        Anchors = [akLeft, akTop, akRight]
        Properties.PopupControl = treeCasierii
        Properties.PopupSysPanelStyle = True
        TabOrder = 0
        Width = 361
      end
      object edListaData: TcxImageComboBox
        Left = 383
        Top = 15
        Anchors = [akTop, akRight]
        Enabled = False
        Properties.Items = <>
        TabOrder = 1
        Width = 86
      end
    end
    object treeCasierii: TcxDBTreeList
      Left = 264
      Top = 308
      Width = 457
      Height = 201
      Bands = <
        item
          Caption.AlignHorz = taCenter
        end>
      DataController.DataSource = dtListaBanci
      DataController.ImageIndexField = 'ICON'
      DataController.ParentField = 'COD_PARINTE'
      DataController.KeyField = 'COD_CB'
      LookAndFeel.Kind = lfUltraFlat
      Navigator.Buttons.CustomButtons = <>
      OptionsBehavior.CellHints = True
      OptionsBehavior.GoToNextCellOnTab = True
      OptionsBehavior.ImmediateEditor = False
      OptionsBehavior.AutoDragCopy = True
      OptionsBehavior.ConfirmDelete = False
      OptionsBehavior.DragCollapse = False
      OptionsBehavior.ExpandOnIncSearch = True
      OptionsBehavior.IncSearch = True
      OptionsBehavior.IncSearchItem = treeCasieriiDENUMIRE
      OptionsBehavior.ShowHourGlass = False
      OptionsCustomizing.BandCustomizing = False
      OptionsCustomizing.BandVertSizing = False
      OptionsCustomizing.ColumnVertSizing = False
      OptionsData.Editing = False
      OptionsData.Deleting = False
      OptionsSelection.CellSelect = False
      OptionsSelection.HideFocusRect = False
      OptionsView.CellTextMaxLineCount = -1
      OptionsView.ShowEditButtons = ecsbFocused
      OptionsView.BandLineHeight = 19
      OptionsView.ColumnAutoWidth = True
      ParentColor = False
      Preview.MaxLineCount = 2
      RootValue = -1
      ScrollbarAnnotations.CustomAnnotations = <>
      TabOrder = 2
      Visible = False
      object treeCasieriiCOD_CB: TcxDBTreeListColumn
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'COD_CB'
        Width = 100
        Position.ColIndex = 3
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeCasieriiCOD_PARINTE: TcxDBTreeListColumn
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'COD_PARINTE'
        Width = 100
        Position.ColIndex = 17
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeCasieriiDENUMIRE: TcxDBTreeListColumn
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Caption.AlignHorz = taCenter
        Caption.Text = 'Denumire Casa'
        DataBinding.FieldName = 'DENUMIRE'
        Width = 210
        Position.ColIndex = 0
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeCasieriiDENV: TcxDBTreeListColumn
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'DENV'
        Width = 100
        Position.ColIndex = 16
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeCasieriiC_O: TcxDBTreeListColumn
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'C_O'
        Width = 100
        Position.ColIndex = 15
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeCasieriiDATA_SOLD: TcxDBTreeListColumn
        PropertiesClassName = 'TcxDateEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.DateButtons = [btnClear, btnToday]
        Properties.DateOnError = deToday
        Properties.InputKind = ikRegExpr
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'DATA_SOLD'
        Width = 100
        Position.ColIndex = 14
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeCasieriiCASIER: TcxDBTreeListColumn
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
        Caption.AlignHorz = taCenter
        Caption.Text = 'Este Casier'
        DataBinding.FieldName = 'CASIER'
        Width = 100
        Position.ColIndex = 13
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeCasieriiVALIDATOR: TcxDBTreeListColumn
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
        Caption.AlignHorz = taCenter
        Caption.Text = 'Este Contabil'
        DataBinding.FieldName = 'VALIDATOR'
        Width = 100
        Position.ColIndex = 12
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeCasieriiADMIN: TcxDBTreeListColumn
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
        Caption.AlignHorz = taCenter
        Caption.Text = 'Este Administrator'
        DataBinding.FieldName = 'ADMIN'
        Width = 100
        Position.ColIndex = 11
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeCasieriiIS_BANCA: TcxDBTreeListColumn
        PropertiesClassName = 'TcxCheckBoxProperties'
        Properties.Alignment = taLeftJustify
        Properties.NullStyle = nssUnchecked
        Properties.ReadOnly = True
        Properties.ValueChecked = 'True'
        Properties.ValueGrayed = ''
        Properties.ValueUnchecked = 'False'
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'IS_BANCA'
        Width = 100
        Position.ColIndex = 10
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeCasieriiIS_AVANS: TcxDBTreeListColumn
        PropertiesClassName = 'TcxCheckBoxProperties'
        Properties.Alignment = taLeftJustify
        Properties.NullStyle = nssUnchecked
        Properties.ReadOnly = True
        Properties.ValueChecked = 'True'
        Properties.ValueGrayed = ''
        Properties.ValueUnchecked = 'False'
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'IS_AVANS'
        Width = 100
        Position.ColIndex = 9
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeCasieriiIS_TEMPOR: TcxDBTreeListColumn
        PropertiesClassName = 'TcxCheckBoxProperties'
        Properties.Alignment = taLeftJustify
        Properties.NullStyle = nssUnchecked
        Properties.ReadOnly = True
        Properties.ValueChecked = 'True'
        Properties.ValueGrayed = ''
        Properties.ValueUnchecked = 'False'
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'IS_TEMPOR'
        Width = 100
        Position.ColIndex = 8
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeCasieriiID_REPARTITORI: TcxDBTreeListColumn
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'ID_REPARTITORI'
        Width = 100
        Position.ColIndex = 7
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeCasieriiICON: TcxDBTreeListColumn
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'ICON'
        Width = 100
        Position.ColIndex = 6
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeCasieriiID_VALUTA: TcxDBTreeListColumn
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Visible = False
        Caption.AlignHorz = taCenter
        DataBinding.FieldName = 'ID_VALUTA'
        Width = 100
        Position.ColIndex = 5
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeCasieriiDESCRIERE: TcxDBTreeListColumn
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Visible = False
        Caption.AlignHorz = taCenter
        Caption.Text = 'Descriere'
        DataBinding.FieldName = 'DESCRIERE'
        Width = 100
        Position.ColIndex = 4
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeCasieriiCRSP_LEI: TcxDBTreeListColumn
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Caption.AlignHorz = taCenter
        Caption.Text = 'Cont Contabil'
        DataBinding.FieldName = 'CRSP_LEI'
        Options.Sizing = False
        Width = 129
        Position.ColIndex = 1
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
      object treeCasieriicodFunctional: TcxDBTreeListColumn
        PropertiesClassName = 'TcxMaskEditProperties'
        Properties.Alignment.Horz = taLeftJustify
        Properties.MaxLength = 0
        Properties.ReadOnly = True
        Caption.AlignHorz = taCenter
        Caption.Text = 'Cod Functional'
        DataBinding.FieldName = 'codFunctional'
        Options.Sizing = False
        Width = 116
        Position.ColIndex = 2
        Position.RowIndex = 0
        Position.BandIndex = 0
        Summary.FooterSummaryItems = <>
        Summary.GroupFooterSummaryItems = <>
      end
    end
  end
  object pnDetail: TcxCollapsedGroup
    Left = 0
    Top = 590
    Align = alBottom
    Caption = 'Detalii Suplimentare'
    ParentFont = False
    TabOrder = 1
    Visible = False
    Height = 20
    Width = 1025
    object Splitter1: TSplitter
      Left = 127
      Top = 18
      Width = -9
      Height = 0
      ExplicitLeft = 126
      ExplicitTop = 21
    end
    object Splitter3: TSplitter
      Left = 835
      Top = 18
      Height = 0
      Align = alRight
      ExplicitLeft = 836
      ExplicitTop = 21
    end
    object DBExplicCont: TdxDBMemo
      Left = 2
      Top = 18
      Width = 125
      Align = alLeft
      TabOrder = 0
      DataField = 'descLunga'
      DataSource = dtRegistru
      MaxLength = 800
      Height = 0
      StoredValues = 2
    end
    object DBExplicProj: TdxDBMemo
      Left = 118
      Top = 18
      Width = 717
      Align = alClient
      Enabled = False
      TabOrder = 2
      DataField = 'descLunga'
      DataSource = dtRegistru
      MaxLength = 800
      ReadOnly = False
      Height = 0
      StoredValues = 66
    end
    object pnFilter: TPanel
      Left = 838
      Top = 18
      Width = 185
      Height = 0
      Align = alRight
      BevelInner = bvRaised
      BevelOuter = bvLowered
      Color = clWindow
      TabOrder = 1
      DesignSize = (
        185
        0)
      object lblFilter: TLabel
        Left = 8
        Top = 24
        Width = 5
        Height = 13
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object chkFilter: TcxCheckBox
        Left = 5
        Top = 3
        Caption = 'Filtru Activ / Inactiv'
        TabOrder = 0
        Transparent = True
      end
      object edtTextFiltru: TdxEdit
        Left = 8
        Top = 48
        Width = 169
        Hint = 'explicatie like '#39'%<|>%'#39
        TabOrder = 1
        Anchors = [akLeft, akTop, akRight]
      end
    end
  end
  object pnSummary: TPanel
    Left = 0
    Top = 610
    Width = 1025
    Height = 42
    Align = alBottom
    AutoSize = True
    BevelInner = bvLowered
    TabOrder = 2
    Visible = False
    object SummStatus: TdxStatusBar
      Left = 2
      Top = 21
      Width = 1021
      Height = 19
      Panels = <
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 50
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Text = 'Total Incasari'
          Width = 90
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 150
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Text = 'Total Plati'
          Width = 70
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 150
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 20
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Text = 'Filtrat Incasari'
          Width = 90
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 150
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Text = 'Filtrat Plati'
          Width = 80
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 150
        end>
      Font.Charset = DEFAULT_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      Color = 16776176
    end
    object SelectedSumm: TdxStatusBar
      Left = 2
      Top = 2
      Width = 1021
      Height = 19
      Panels = <
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 50
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Text = 'Total Incasari'
          Width = 90
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 150
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Text = 'Total Plati'
          Width = 70
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 150
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 20
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Text = 'Nivel2 Incasari'
          Width = 90
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 150
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Text = 'Nivel2 Plati'
          Width = 80
        end
        item
          PanelStyleClassName = 'TdxStatusBarTextPanelStyle'
          Width = 150
        end>
      Font.Charset = ANSI_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'MS Sans Serif'
      Font.Style = [fsBold]
      Color = 16776176
    end
  end
  object dtRegistru: TDataSource
    DataSet = qryRegistru
    Left = 202
    Top = 200
  end
  object Cmd_RegistruCasa: TActionList
    Left = 352
    Top = 145
    object Cmd_JustificareAvans: TAction
      Caption = 'Justificare Avans'
    end
    object Cmd_EchilibrarePlata: TAction
      Caption = 'Echilibreaza Plata-Incasare'
      ShortCut = 16416
    end
    object Cmd_AdaugaPlata: TAction
      Caption = 'Adauga Plata Incasare'
      ShortCut = 45
    end
    object Cmd_DeletePlata: TAction
      Caption = 'Stergere Plata'
      ShortCut = 16430
    end
    object Cmd_SalveazaPlata: TAction
      Caption = 'Salveaza Plata'
      ShortCut = 16467
    end
    object Cmd_TransferaPlata: TAction
      Caption = 'Transfer in alta casa'
      ShortCut = 16468
    end
    object Cmd_AcceptaTransfer: TAction
      Caption = 'Accepta Transfer'
      ShortCut = 16449
    end
    object Cmd_Validate: TAction
      Caption = 'Validare'
      ShortCut = 116
    end
    object Cmd_UnValidate: TAction
      Caption = 'Scoate aTributul de validare'
      ShortCut = 117
    end
    object Cmd_AnuleazaTransfer: TAction
      Caption = 'Anuleaza Transfer'
    end
    object Cmd_GenereazaDiferenta: TAction
      Caption = 'Genereaza Diferenta'
    end
    object Cmd_Renumeroteaza: TAction
      Caption = 'Renumeroteaza'
    end
    object Cmd_RenumeroteazaAll: TAction
      Caption = 'Renumeroteaza Ecran'
    end
    object Cmd_Import: TAction
      Caption = 'Import din alta Casa'
    end
    object CmdErrors: TAction
      Category = 'Utils'
      Caption = 'Lista de Verificari'
    end
    object Cmd_SaveLocal: TAction
      Category = 'Utils'
      Caption = 'Salveaza datele local'
    end
    object Cmd_RecalculateSold: TAction
      Category = 'Utils'
      Caption = 'Recalcul Sold'
      Hint = 'Recalcul Sold Incepand cu pozitia curenta'
    end
    object Cmd_ShowDetail: TAction
      Category = 'Utils'
      Caption = 'Afiseaza Detalii'
    end
    object Cmd_ShowSummary: TAction
      Category = 'Utils'
      Caption = 'Afisarea Banda Sumatoare'
    end
    object Cmd_ShowLegend: TAction
      Category = 'Utils'
      Caption = 'Afiseaza Legenda'
      Hint = 'Afiseaza Legenda'
    end
    object Cmd_ValideazaIesire: TAction
      Caption = 'Valideaza Iesire'
    end
    object Cmd_Flag: TAction
      Caption = 'Flag Inregistrarea Curenta'
    end
    object Cmd_GotoRecord: TAction
      Caption = 'Pozitionare Inregistrare'
      Hint = 'Pozitionare pe Inregistrarea din casa de destinatie'
    end
    object Cmd_VenitCasa: TAction
      Caption = 'Import Casa/Banca'
    end
    object Cmd_SetBandSize: TAction
      Category = 'Utils'
      Caption = 'Setare Marime Banda'
    end
    object Cmd_DispozitiePlata: TAction
      Caption = 'Tipareste Dispozitie Plata'
    end
    object CmdDecont: TAction
      Category = 'Decontari'
      Caption = 'Decontare Document Furnizor'
    end
    object Cmd_TransferaPozitie: TAction
      Caption = 'Transfera pozitie'
    end
    object CmdCopyColumn: TAction
      Caption = 'Copiaza coloana curenta'
      ShortCut = 123
    end
  end
  object GridRegistruPopup: TPopupMenu
    Left = 350
    Top = 200
    object AdaugaPlataIncasare: TMenuItem
      Action = Cmd_AdaugaPlata
    end
    object StergerePlata: TMenuItem
      Action = Cmd_DeletePlata
    end
    object EchilibreazaPlataIncasare: TMenuItem
      Action = Cmd_EchilibrarePlata
    end
    object Renumeroteaza: TMenuItem
      Action = Cmd_Renumeroteaza
    end
    object RenumeroteazaEcran1: TMenuItem
      Action = Cmd_RenumeroteazaAll
    end
    object SetareMarimeBanda1: TMenuItem
      Action = Cmd_SetBandSize
      Caption = 'Setare Marime Banda "Stare"'
    end
    object N1: TMenuItem
      Caption = '-'
    end
    object PozitionareInregistrare1: TMenuItem
      Action = Cmd_GotoRecord
      Caption = 'Pozitionare Inregistrare ->'
    end
    object Transferinaltacasa: TMenuItem
      Action = Cmd_TransferaPozitie
    end
    object AcceptaTransfer: TMenuItem
      Action = Cmd_AcceptaTransfer
    end
    object AnuleazaTransfer: TMenuItem
      Action = Cmd_AnuleazaTransfer
    end
    object ImportCasaBanca1: TMenuItem
      Action = Cmd_VenitCasa
    end
    object N4: TMenuItem
      Caption = '-'
    end
    object GenereazaDiferenta: TMenuItem
      Action = Cmd_GenereazaDiferenta
    end
    object JustificareAvans: TMenuItem
      Action = Cmd_JustificareAvans
    end
    object TiparesteDispozitiePlata1: TMenuItem
      Action = Cmd_DispozitiePlata
    end
    object N2: TMenuItem
      Caption = '-'
    end
    object Validare: TMenuItem
      Tag = 2
      Action = Cmd_Validate
    end
    object DeValidare1: TMenuItem
      Tag = 2
      Action = Cmd_UnValidate
      Caption = 'DeValidare'
    end
    object FlagInregistrareaCurenta1: TMenuItem
      Action = Cmd_Flag
    end
    object N3: TMenuItem
      Caption = '-'
    end
    object ImportdinaltaCasa1: TMenuItem
      Action = Cmd_Import
    end
    object DecontareDocumentFurnizor1: TMenuItem
      Action = CmdDecont
    end
    object Copiazacoloanacurenta1: TMenuItem
      Action = CmdCopyColumn
    end
  end
  object qryListaBanci: TZQuery
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
    Left = 264
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
  object dtListaBanci: TDataSource
    DataSet = qryListaBanci
    Left = 200
    Top = 144
  end
  object qryRegistru: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'select * from vwRegistruCasa'
      ''
      'order by refCasierie, dataRegistru, pozRegistru, idRegistru')
    Params = <>
    Properties.Strings = (
      'KeyFields=idRegistru')
    Left = 267
    Top = 200
  end
  object stiluriRegistru: TcxStyleRepository
    Left = 452
    Top = 144
    PixelsPerInch = 96
    object stilPrimulNivel: TcxStyle
    end
    object stilNivelulDoi: TcxStyle
    end
    object stilNivelDoiSters: TcxStyle
    end
    object stilCurentNivelUnu: TcxStyle
    end
    object stilCurentNivelDoi: TcxStyle
    end
    object stilValidat: TcxStyle
    end
    object stilDataCurenta: TcxStyle
    end
    object stilAreFocus: TcxStyle
    end
  end
end
