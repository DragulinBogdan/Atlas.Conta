object FrmBalanta: TFrmBalanta
  Left = 328
  Top = 191
  Caption = 'Balanta Contabila'
  ClientHeight = 663
  ClientWidth = 1086
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  OnClose = FormClose
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  DesignSize = (
    1086
    663)
  PixelsPerInch = 96
  TextHeight = 13
  object pnAll: TPanel
    Left = 0
    Top = 0
    Width = 1086
    Height = 593
    Align = alTop
    Anchors = [akLeft, akTop, akRight, akBottom]
    BevelOuter = bvNone
    TabOrder = 0
    object Splitter1: TSplitter
      Left = 186
      Top = 0
      Height = 593
      ExplicitLeft = 169
      ExplicitHeight = 664
    end
    object PnClient: TPanel
      Left = 189
      Top = 0
      Width = 897
      Height = 593
      Align = alClient
      BevelOuter = bvNone
      TabOrder = 0
      ExplicitHeight = 631
      object pnTools: TPanel
        Left = 0
        Top = 0
        Width = 897
        Height = 25
        Align = alTop
        BevelOuter = bvNone
        TabOrder = 0
        object ExpandLevels: TToolBar
          Left = 4
          Top = 2
          Width = 277
          Height = 21
          Align = alNone
          ButtonHeight = 21
          ButtonWidth = 65
          Caption = 'Nivele de sinteza'
          EdgeInner = esNone
          EdgeOuter = esNone
          Font.Charset = DEFAULT_CHARSET
          Font.Color = clWindowText
          Font.Height = -11
          Font.Name = 'MS Sans Serif'
          Font.Style = []
          ParentFont = False
          ShowCaptions = True
          TabOrder = 0
        end
      end
      object cxTreeBalanta: TcxDBTreeList
        Left = 0
        Top = 25
        Width = 897
        Height = 568
        Align = alClient
        Bands = <
          item
            FixedKind = tlbfLeft
            MinWidth = 30
            Width = 304
          end
          item
            Width = 1000
          end>
        DataController.DataSource = DTBalanta
        DataController.ParentField = 'PARINTE'
        DataController.KeyField = 'CONT'
        Navigator.Buttons.CustomButtons = <>
        OptionsBehavior.CellHints = True
        OptionsBehavior.ImmediateEditor = False
        OptionsBehavior.ExpandOnIncSearch = True
        OptionsBehavior.IncSearch = True
        OptionsCustomizing.BandMoving = False
        OptionsCustomizing.BandVertSizing = False
        OptionsCustomizing.ColumnsQuickCustomization = True
        OptionsCustomizing.ColumnVertSizing = False
        OptionsCustomizing.StackedColumns = False
        OptionsData.Editing = False
        OptionsData.Deleting = False
        OptionsView.BandLineHeight = 10
        OptionsView.Bands = True
        OptionsView.HeaderAutoHeight = True
        OptionsView.Indicator = True
        PopupMenu = ppMeniu
        PopupMenus.ColumnHeaderMenu.UseBuiltInMenu = True
        PopupMenus.FooterMenu.UseBuiltInMenu = True
        PopupMenus.GroupFooterMenu.UseBuiltInMenu = True
        RootValue = -1
        ScrollbarAnnotations.CustomAnnotations = <>
        TabOrder = 1
        OnCustomDrawDataCell = cxTreeBalantaCustomDrawDataCell
        ExplicitTop = 68
        ExplicitHeight = 563
        object cxTreeBalantaCONT: TcxDBTreeListColumn
          Visible = False
          Caption.AlignHorz = taCenter
          Caption.Text = 'Cont'
          DataBinding.FieldName = 'CONT'
          Width = 100
          Position.ColIndex = 8
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaCONT_PLAN: TcxDBTreeListColumn
          Visible = False
          Caption.AlignHorz = taCenter
          Caption.Text = 'Cont Plan'
          DataBinding.FieldName = 'CONT_PLAN'
          Width = 100
          Position.ColIndex = 2
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaCOD_FUNCTIONAL: TcxDBTreeListColumn
          Visible = False
          Caption.AlignHorz = taCenter
          Caption.Text = 'Cod Functional'
          DataBinding.FieldName = 'COD_FUNCTIONAL'
          Width = 100
          Position.ColIndex = 3
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaCOD_ECONOMIC: TcxDBTreeListColumn
          Visible = False
          Caption.AlignHorz = taCenter
          Caption.Text = 'Cod Economic'
          DataBinding.FieldName = 'COD_ECONOMIC'
          Width = 100
          Position.ColIndex = 4
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaEXPLICATIE: TcxDBTreeListColumn
          Caption.AlignHorz = taCenter
          Caption.Text = 'Explicatie'
          DataBinding.FieldName = 'CONT'
          Width = 199
          Position.ColIndex = 0
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
          OnGetDisplayText = cxTreeBalantaEXPLICATIEGetDisplayText
        end
        object cxTreeBalantaROMANA: TcxDBTreeListColumn
          Visible = False
          Caption.AlignHorz = taCenter
          Caption.Text = 'Denumire'
          DataBinding.FieldName = 'ROMANA'
          Width = 125
          Position.ColIndex = 1
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaCODREP: TcxDBTreeListColumn
          Visible = False
          Caption.AlignHorz = taCenter
          Caption.Text = 'Id Rep'
          DataBinding.FieldName = 'CODREP'
          Width = 100
          Position.ColIndex = 5
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaFCTCONT: TcxDBTreeListColumn
          Visible = False
          Caption.AlignHorz = taCenter
          Caption.Text = 'Functionalitate'
          DataBinding.FieldName = 'FCTCONT'
          Width = 100
          Position.ColIndex = 6
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaNIVEL: TcxDBTreeListColumn
          Visible = False
          Caption.AlignHorz = taCenter
          Caption.Text = 'Nivel'
          DataBinding.FieldName = 'NIVEL'
          Width = 100
          Position.ColIndex = 7
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaSID: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Caption.AlignHorz = taCenter
          Caption.MultiLine = True
          Caption.Text = 'Sold Initial Debitor'
          DataBinding.FieldName = 'SID'
          Width = 73
          Position.ColIndex = 0
          Position.RowIndex = 0
          Position.BandIndex = 1
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaSIC: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Caption.AlignHorz = taCenter
          Caption.MultiLine = True
          Caption.Text = 'Sold Inital Creditor'
          DataBinding.FieldName = 'SIC'
          Width = 83
          Position.ColIndex = 1
          Position.RowIndex = 0
          Position.BandIndex = 1
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaS_P_D: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Visible = False
          Caption.AlignHorz = taCenter
          Caption.MultiLine = True
          Caption.Text = 'Sold Precedent Debitor'
          DataBinding.FieldName = 'S_P_D'
          Width = 54
          Position.ColIndex = 2
          Position.RowIndex = 0
          Position.BandIndex = 1
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaS_P_C: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Visible = False
          Caption.AlignHorz = taCenter
          Caption.MultiLine = True
          Caption.Text = 'Sold Precedent Creditor'
          DataBinding.FieldName = 'S_P_C'
          Width = 54
          Position.ColIndex = 3
          Position.RowIndex = 0
          Position.BandIndex = 1
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaR_P_D: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Caption.AlignHorz = taCenter
          Caption.MultiLine = True
          Caption.Text = 'Rulaj Precedent Debitor'
          DataBinding.FieldName = 'R_P_D'
          Width = 86
          Position.ColIndex = 4
          Position.RowIndex = 0
          Position.BandIndex = 1
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaR_P_C: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Caption.AlignHorz = taCenter
          Caption.MultiLine = True
          Caption.Text = 'Rulaj Precedent Creditor'
          DataBinding.FieldName = 'R_P_C'
          Width = 81
          Position.ColIndex = 5
          Position.RowIndex = 0
          Position.BandIndex = 1
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaT_P_D: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Visible = False
          Caption.AlignHorz = taCenter
          Caption.MultiLine = True
          Caption.Text = 'Total Precedent Debitor'
          DataBinding.FieldName = 'T_P_D'
          Width = 54
          Position.ColIndex = 6
          Position.RowIndex = 0
          Position.BandIndex = 1
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaT_P_C: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Visible = False
          Caption.AlignHorz = taCenter
          Caption.MultiLine = True
          Caption.Text = 'Total Precedent Creditor'
          DataBinding.FieldName = 'T_P_C'
          Width = 54
          Position.ColIndex = 7
          Position.RowIndex = 0
          Position.BandIndex = 1
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaR_L_D: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Caption.AlignHorz = taCenter
          Caption.MultiLine = True
          Caption.Text = 'Rulaj Debitor'
          DataBinding.FieldName = 'R_L_D'
          Width = 84
          Position.ColIndex = 8
          Position.RowIndex = 0
          Position.BandIndex = 1
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaR_L_C: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Caption.AlignHorz = taCenter
          Caption.MultiLine = True
          Caption.Text = 'Rulaj Creditor'
          DataBinding.FieldName = 'R_L_C'
          Width = 85
          Position.ColIndex = 9
          Position.RowIndex = 0
          Position.BandIndex = 1
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaR_T_D: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Caption.AlignHorz = taCenter
          Caption.MultiLine = True
          Caption.Text = 'Rulaj Total Debitor'
          DataBinding.FieldName = 'R_T_D'
          Width = 84
          Position.ColIndex = 10
          Position.RowIndex = 0
          Position.BandIndex = 1
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaR_T_C: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Caption.AlignHorz = taCenter
          Caption.MultiLine = True
          Caption.Text = 'Rulaj Total Creditor'
          DataBinding.FieldName = 'R_T_C'
          Width = 86
          Position.ColIndex = 11
          Position.RowIndex = 0
          Position.BandIndex = 1
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaT_L_D: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Visible = False
          Caption.AlignHorz = taCenter
          Caption.MultiLine = True
          Caption.Text = 'Total Lunar Debitor'
          DataBinding.FieldName = 'T_L_D'
          Width = 53
          Position.ColIndex = 12
          Position.RowIndex = 0
          Position.BandIndex = 1
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaT_L_C: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Visible = False
          Caption.AlignHorz = taCenter
          Caption.MultiLine = True
          Caption.Text = 'Total Lunar Creditor'
          DataBinding.FieldName = 'T_L_C'
          Width = 53
          Position.ColIndex = 13
          Position.RowIndex = 0
          Position.BandIndex = 1
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaS_T_D: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Caption.AlignHorz = taCenter
          Caption.MultiLine = True
          Caption.Text = 'Sume Totale Debitate'
          DataBinding.FieldName = 'S_T_D'
          Width = 83
          Position.ColIndex = 14
          Position.RowIndex = 0
          Position.BandIndex = 1
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaS_T_C: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Caption.AlignHorz = taCenter
          Caption.MultiLine = True
          Caption.Text = 'Sume Totale Creditate'
          DataBinding.FieldName = 'S_T_C'
          Width = 86
          Position.ColIndex = 15
          Position.RowIndex = 0
          Position.BandIndex = 1
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaS_D: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Caption.AlignHorz = taCenter
          Caption.MultiLine = True
          Caption.Text = 'Sold Debitor'
          DataBinding.FieldName = 'S_D'
          Width = 84
          Position.ColIndex = 16
          Position.RowIndex = 0
          Position.BandIndex = 1
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object cxTreeBalantaS_C: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCurrencyEditProperties'
          Properties.DecimalPlaces = 2
          Properties.DisplayFormat = ',0.00;-,0.00'
          Caption.AlignHorz = taCenter
          Caption.MultiLine = True
          Caption.Text = 'Sold Creditor'
          DataBinding.FieldName = 'S_C'
          Width = 85
          Position.ColIndex = 17
          Position.RowIndex = 0
          Position.BandIndex = 1
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
      end
    end
    object pnLeft: TScrollBox
      Left = 0
      Top = 0
      Width = 186
      Height = 593
      VertScrollBar.Position = 50
      Align = alLeft
      BevelOuter = bvNone
      Color = clGray
      ParentColor = False
      TabOrder = 1
      ExplicitHeight = 631
      DesignSize = (
        165
        589)
      object Label1: TLabel
        Left = 6
        Top = -49
        Width = 84
        Height = 13
        Caption = 'Luna de raportare'
      end
      object Label2: TLabel
        Left = 6
        Top = -4
        Width = 82
        Height = 13
        Caption = 'An de raportare : '
      end
      object Label3: TLabel
        Left = 6
        Top = 16
        Width = 128
        Height = 13
        Caption = 'Data Inceput / Data Sfarsit'
      end
      object btnGo: TSpeedButton
        Left = 8
        Top = 480
        Width = 155
        Height = 22
        Caption = 'Aplica Filtre Balanta'
        OnClick = btnGoClick
      end
      object btnGenerareNote: TSpeedButton
        Left = 8
        Top = 502
        Width = 155
        Height = 22
        Caption = 'Generare Note Contabile'
        OnClick = btnGenerareNoteClick
      end
      object btnGenNoteInchidere: TSpeedButton
        Left = 8
        Top = 524
        Width = 155
        Height = 22
        Caption = 'Generare Nota Inchidere'
        OnClick = btnGenNoteInchidereClick
      end
      object Label4: TLabel
        Left = 2
        Top = 288
        Width = 65
        Height = 13
        Caption = 'Grup Proiecte'
      end
      object Label5: TLabel
        Left = 2
        Top = 325
        Width = 33
        Height = 13
        Caption = 'Proiect'
      end
      object lbValuta: TLabel
        Left = 2
        Top = 399
        Width = 48
        Height = 13
        Caption = 'Tip Valuta'
      end
      object Label6: TLabel
        Left = 2
        Top = 362
        Width = 54
        Height = 13
        Caption = 'Tip Balanta'
      end
      object Label7: TLabel
        Left = 2
        Top = 251
        Width = 34
        Height = 13
        Caption = 'Unitate'
      end
      object Label8: TLabel
        Left = 2
        Top = 214
        Width = 104
        Height = 13
        Caption = 'Clasificatii Functionale'
      end
      object Label9: TLabel
        Left = 2
        Top = 177
        Width = 22
        Height = 13
        Caption = 'Cont'
      end
      object lbTipDefalcare: TLabel
        Left = 2
        Top = 436
        Width = 64
        Height = 13
        Caption = 'Tip Defalcare'
      end
      object edAnRaportare: TcxSpinEdit
        Left = 96
        Top = -8
        Properties.OnChange = edAnRaportarePropertiesChange
        TabOrder = 1
        Value = 2007
        Width = 65
      end
      object edDataStart: TcxDateEdit
        Left = 2
        Top = 32
        Properties.InputKind = ikMask
        Properties.SaveTime = False
        Properties.ShowTime = False
        TabOrder = 2
        Width = 80
      end
      object edDataEnd: TcxDateEdit
        Left = 85
        Top = 32
        Properties.InputKind = ikMask
        Properties.SaveTime = False
        Properties.ShowTime = False
        TabOrder = 3
        Width = 80
      end
      object edListaLuni: TcxImageComboBox
        Left = 32
        Top = -33
        Properties.Items = <>
        Properties.OnChange = edListaLuniPropertiesChange
        TabOrder = 0
        Width = 129
      end
      object edCont: TcxImageComboBox
        Left = 8
        Top = 190
        Properties.Items = <>
        TabOrder = 8
        Width = 155
      end
      object edUnitate: TcxCheckComboBox
        Left = 8
        Top = 264
        Properties.Delimiter = ','
        Properties.EmptySelectionText = 'Toate Unitatile'
        Properties.EditValueFormat = cvfCaptions
        Properties.Items = <>
        TabOrder = 12
        Width = 155
      end
      object edTipBalanta: TcxImageComboBox
        Left = 8
        Top = 375
        EditValue = 0
        Properties.Items = <
          item
            Description = 'Bilantier'
            ImageIndex = 0
            Value = 0
          end
          item
            Description = 'Extrabilantier'
            Value = 1
          end
          item
            Description = 'Integral'
            Value = 2
          end>
        TabOrder = 14
        Width = 153
      end
      object chkPreluareNote: TcxCheckBox
        Left = 2
        Top = 584
        Anchors = [akLeft, akTop, akBottom]
        Caption = 'Preluare note din module'
        TabOrder = 10
        Transparent = True
      end
      object ChkAplicaCulori: TcxCheckBox
        Left = 2
        Top = 554
        Caption = 'Aplica Culori Pentru Nivele'
        TabOrder = 4
        Transparent = True
        OnClick = ChkAplicaCuloriClick
      end
      object ChkDoarMiscari: TcxCheckBox
        Left = 2
        Top = 52
        Caption = 'Elimina Conturile fara tranz.'
        State = cbsChecked
        TabOrder = 5
        Transparent = True
      end
      object ChkPeRepartitori: TcxCheckBox
        Left = 2
        Top = 69
        Caption = 'Defalcat pe repartitori'
        State = cbsChecked
        TabOrder = 6
        Transparent = True
      end
      object ChkPeMateriale: TcxCheckBox
        Left = 2
        Top = 86
        Caption = 'Defalcat pe materiale'
        TabOrder = 7
        Transparent = True
      end
      object chkConsolidata: TcxCheckBox
        Left = 2
        Top = 137
        Caption = 'Balanta Consolidata'
        ParentFont = False
        Style.Font.Charset = DEFAULT_CHARSET
        Style.Font.Color = clWindowText
        Style.Font.Height = -12
        Style.Font.Name = 'MS Sans Serif'
        Style.Font.Style = [fsBold]
        Style.IsFontAssigned = True
        TabOrder = 9
        Transparent = True
      end
      object ChkArataFunctionalitate: TcxCheckBox
        Left = 2
        Top = 568
        Caption = 'Afiseaza culori functionalitate'
        TabOrder = 11
        Transparent = True
        OnClick = ChkArataFunctionalitateClick
      end
      object chkSintetic: TcxCheckBox
        Left = 2
        Top = 103
        Caption = 'Balanta Sintetica'
        TabOrder = 13
        Transparent = True
      end
      object chkExecutie: TcxCheckBox
        Left = 2
        Top = 120
        Caption = 'Defalcat pe executie'
        ParentFont = False
        State = cbsChecked
        TabOrder = 15
        Transparent = True
      end
      object edGrup: TcxImageComboBox
        Left = 8
        Top = 301
        Properties.ClearKey = 27
        Properties.Items = <>
        Properties.OnChange = edGrupPropertiesChange
        TabOrder = 16
        Width = 155
      end
      object edProiect: TcxImageComboBox
        Left = 8
        Top = 338
        Properties.Items = <>
        TabOrder = 17
        Width = 155
      end
      object edClasaFunctionala: TcxCheckComboBox
        Left = 8
        Top = 227
        Properties.Delimiter = ','
        Properties.EmptySelectionText = 'Toate Clasificatiile'
        Properties.EditValueFormat = cvfCaptions
        Properties.Items = <>
        TabOrder = 18
        Width = 155
      end
      object edTipValuta: TcxImageComboBox
        Left = 8
        Top = 412
        Properties.ClearKey = 27
        Properties.Items = <>
        TabOrder = 19
        Width = 155
      end
      object edTipDefalcare: TcxImageComboBox
        Left = 8
        Top = 452
        Properties.ClearKey = 27
        Properties.Items = <>
        TabOrder = 20
        Width = 155
      end
      object edTipInchidere: TcxComboBox
        Left = 8
        Top = 156
        Properties.DropDownListStyle = lsFixedList
        Properties.Items.Strings = (
          'Fara Note de Inchidere'
          'Cu Note Inchidere'
          'Trimestre < Inchise'
          'Luni < Inchise')
        TabOrder = 21
        Text = 'Cu Note Inchidere'
        Width = 155
      end
    end
  end
  object BtnOk: TcxButton
    Left = 974
    Top = 599
    Width = 88
    Height = 26
    Anchors = [akRight, akBottom]
    Caption = 'Ok'
    LookAndFeel.Kind = lfOffice11
    ModalResult = 1
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
  object btnRaportare: TcxButton
    Left = 8
    Top = 599
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
    TabOrder = 2
    Visible = False
  end
  object QryBalanta: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'EXEC SP_GET_BALANTA'
      '  @DATA_MIN       = :DATA_MIN,'
      '  @DATA_MAX       = :DATA_MAX,'
      '  @DOAR_MISCARI   = :DOAR_MISCARI,'
      '  @PE_REPARTITORI = :PE_REPARTITORI,'
      '  @PE_MATERIALE   = :PE_MATERIALE,'
      '  @CONT           = :CONT,'
      '  @CU_RECALCUL    = :CU_RECALCUL,'
      '  @param          = :param,'
      '  @CU_INCHIDERE   = :CU_INCHIDERE,'
      '  @CONSOLIDATA    = :CONSOLIDATA,'
      '  @COD_FUNCTIONAL = :COD_FUNCTIONAL,'
      '  @IS_SINTETIC    = :IS_SINTETIC,'
      '  @ID_UNITATE     = :ID_UNITATE,'
      '  @extrabilantier = :TIP_BALANTA,'
      '  @PE_EXECUTIE    = :CU_EXECUTIE,'
      '  @ID_PROIECT     = :ID_PROIECT,'
      '  @id_oi_grupe    = :ID_OI_GRUPE,'
      '  @tipDefalcare = :tipDefalcare')
    Params = <
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA_MIN'
        ParamType = ptInput
        Size = 16
        Value = 40179d
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA_MAX'
        ParamType = ptInput
        Size = 16
        Value = 40543d
      end
      item
        DataType = ftBoolean
        Precision = 255
        NumericScale = 255
        Name = 'DOAR_MISCARI'
        ParamType = ptInput
        Size = 2
      end
      item
        DataType = ftBoolean
        Precision = 255
        NumericScale = 255
        Name = 'PE_REPARTITORI'
        ParamType = ptInput
        Size = 2
      end
      item
        DataType = ftBoolean
        Precision = 255
        NumericScale = 255
        Name = 'PE_MATERIALE'
        ParamType = ptInput
        Size = 2
      end
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'CONT'
        ParamType = ptInput
        Size = 50
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'CU_RECALCUL'
        ParamType = ptInput
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'PARAM'
        ParamType = ptInput
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 255
        NumericScale = 255
        Name = 'CU_INCHIDERE'
        ParamType = ptInput
        Size = 2
      end
      item
        DataType = ftBoolean
        Precision = 255
        NumericScale = 255
        Name = 'CONSOLIDATA'
        ParamType = ptInput
        Size = 2
      end
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'COD_FUNCTIONAL'
        ParamType = ptInput
        Size = 128
      end
      item
        DataType = ftBoolean
        Precision = 10
        Name = 'IS_SINTETIC'
        ParamType = ptInput
        Size = 4
      end
      item
        DataType = ftString
        Precision = 10
        Name = 'ID_UNITATE'
        ParamType = ptInput
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'TIP_BALANTA'
        ParamType = ptInput
        Size = 4
      end
      item
        DataType = ftBoolean
        Precision = 255
        NumericScale = 255
        Name = 'CU_EXECUTIE'
        ParamType = ptInput
        Size = 2
      end
      item
        DataType = ftInteger
        Name = 'ID_PROIECT'
        ParamType = ptInput
      end
      item
        DataType = ftInteger
        Name = 'ID_OI_GRUPE'
        ParamType = ptInput
      end
      item
        DataType = ftString
        Name = 'tipDefalcare'
        ParamType = ptInput
      end>
    Left = 221
    Top = 160
    ParamData = <
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA_MIN'
        ParamType = ptInput
        Size = 16
        Value = 40179d
      end
      item
        DataType = ftDateTime
        Precision = 23
        NumericScale = 3
        Name = 'DATA_MAX'
        ParamType = ptInput
        Size = 16
        Value = 40543d
      end
      item
        DataType = ftBoolean
        Precision = 255
        NumericScale = 255
        Name = 'DOAR_MISCARI'
        ParamType = ptInput
        Size = 2
      end
      item
        DataType = ftBoolean
        Precision = 255
        NumericScale = 255
        Name = 'PE_REPARTITORI'
        ParamType = ptInput
        Size = 2
      end
      item
        DataType = ftBoolean
        Precision = 255
        NumericScale = 255
        Name = 'PE_MATERIALE'
        ParamType = ptInput
        Size = 2
      end
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'CONT'
        ParamType = ptInput
        Size = 50
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'CU_RECALCUL'
        ParamType = ptInput
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'PARAM'
        ParamType = ptInput
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 255
        NumericScale = 255
        Name = 'CU_INCHIDERE'
        ParamType = ptInput
        Size = 2
      end
      item
        DataType = ftBoolean
        Precision = 255
        NumericScale = 255
        Name = 'CONSOLIDATA'
        ParamType = ptInput
        Size = 2
      end
      item
        DataType = ftString
        Precision = 255
        NumericScale = 255
        Name = 'COD_FUNCTIONAL'
        ParamType = ptInput
        Size = 128
      end
      item
        DataType = ftBoolean
        Precision = 10
        Name = 'IS_SINTETIC'
        ParamType = ptInput
        Size = 4
      end
      item
        DataType = ftString
        Precision = 10
        Name = 'ID_UNITATE'
        ParamType = ptInput
        Size = 4
      end
      item
        DataType = ftInteger
        Precision = 10
        Name = 'TIP_BALANTA'
        ParamType = ptInput
        Size = 4
      end
      item
        DataType = ftBoolean
        Precision = 255
        NumericScale = 255
        Name = 'CU_EXECUTIE'
        ParamType = ptInput
        Size = 2
      end
      item
        DataType = ftInteger
        Name = 'ID_PROIECT'
        ParamType = ptInput
      end
      item
        DataType = ftInteger
        Name = 'ID_OI_GRUPE'
        ParamType = ptInput
      end
      item
        DataType = ftString
        Name = 'tipDefalcare'
        ParamType = ptInput
      end>
  end
  object DTBalanta: TDataSource
    DataSet = QryBalanta
    Left = 188
    Top = 161
  end
  object ppMeniu: TPopupMenu
    Left = 252
    Top = 161
    object FisaCont1: TMenuItem
      Action = Cmd_FisaCont
    end
    object N1: TMenuItem
      Caption = '-'
    end
    object ppTiparireBalanta: TMenuItem
      Caption = 'Tiparire Balanta'
      OnClick = ppTiparireBalantaClick
    end
    object N3: TMenuItem
      Caption = '-'
    end
    object ppExportBalanta: TMenuItem
      Caption = 'Exporta Balanta'
      object ppExportExcel: TMenuItem
        Caption = 'Export In Excel'
        OnClick = ppExportExcelClick
      end
      object ppExportXML: TMenuItem
        Caption = 'Export in XML'
        OnClick = ppExportXMLClick
      end
      object ppExportCSV: TMenuItem
        Caption = 'Export in CSV'
        OnClick = ppExportCSVClick
      end
      object ppExportHTML: TMenuItem
        Caption = 'Export in HTML'
        OnClick = ppExportHTMLClick
      end
    end
    object ppSaveTip: TMenuItem
      Action = Cmd_SalveazaTipBalanta
    end
    object ppStergeTipBalanta: TMenuItem
      Action = Cmd_StergeTipBalanta
    end
    object ppListaTipBalanta: TMenuItem
      Caption = 'Tipuri de Balanta'
    end
    object N2: TMenuItem
      Caption = '-'
    end
  end
  object CmdNote: TActionList
    Left = 246
    Top = 223
    object Cmd_FisaCont: TAction
      Caption = 'Fisa Cont'
      ShortCut = 16416
      OnExecute = Cmd_FisaContExecute
    end
    object Cmd_SalveazaTipBalanta: TAction
      Caption = 'Salveaza Tip Balanta'
      OnExecute = Cmd_SalveazaTipBalantaExecute
    end
    object Cmd_StergeTipBalanta: TAction
      Caption = 'Sterge Tip Balanta'
    end
    object Cmd_ListaTipBalanta: TAction
      Caption = 'Tipuri de Balanta'
    end
    object Cmd_ExporXLS: TAction
      Category = 'Export'
      Caption = 'Export excel'
      OnExecute = Cmd_ExporXLSExecute
    end
  end
  object tiparireBalanta: TdxComponentPrinter
    CurrentLink = linkBalanta
    Version = 0
    Left = 292
    Top = 161
    PixelsPerInch = 96
    object linkBalanta: TcxDBTreeListReportLink
      Component = cxTreeBalanta
      DesignerCaption = 'Personalizare Raport'
      PrinterPage.DMPaper = 9
      PrinterPage.Footer = 200
      PrinterPage.GrayShading = True
      PrinterPage.Header = 200
      PrinterPage.Margins.Bottom = 500
      PrinterPage.Margins.Left = 500
      PrinterPage.Margins.Right = 500
      PrinterPage.Margins.Top = 500
      PrinterPage.Orientation = poLandscape
      PrinterPage.PageSize.X = 8300
      PrinterPage.PageSize.Y = 11700
      PrinterPage._dxMeasurementUnits_ = 0
      PrinterPage._dxLastMU_ = 1
      ReportTitle.Text = 'Balanta'
      OptionsExpanding.AutoExpandNodes = True
      OptionsExpanding.ExplicitlyExpandNodes = True
      OptionsSize.AutoWidth = True
      OptionsView.BandHeaders = False
      OptionsView.Borders = False
      OptionsView.ExpandButtons = False
      PixelsPerInch = 96
      BuiltInReportLink = True
    end
  end
  object saveDialogExcel: TSaveDialog
    DefaultExt = '*xls'
    Filter = 'Fisiere Excel|*.xls|Toate Fisierele|*.*'
    Title = 'Alegeti Fisiereul Excel in care se face exportul'
    Left = 292
    Top = 224
  end
  object saveDialogCSV: TSaveDialog
    DefaultExt = '*.csv'
    Filter = 'Fisiere CSV|*.csv|Toate Fisierele|*.*'
    Title = 'Alegeti Fisierul CSV in care se face exportul'
    Left = 356
    Top = 224
  end
  object saveDialogXML: TSaveDialog
    DefaultExt = '*.xml'
    Filter = 'Fisiere XML|*.xml|Toate Fisierele|*.*'
    Title = 'Alegeti Fisierul XML in care se face exportul'
    Left = 324
    Top = 224
  end
  object saveDialogHTML: TSaveDialog
    DefaultExt = '*.html'
    Filter = 'Fisiere HMTL|*.html|Toate Fisierele|*.*'
    Title = 'Alegeti Fisierul HTML in care se face exportul'
    Left = 388
    Top = 224
  end
end
