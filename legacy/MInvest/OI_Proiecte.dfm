object frmOIProiecte: TfrmOIProiecte
  Left = 294
  Top = 83
  Width = 815
  Height = 617
  Caption = 'Intretinere Proiecte'
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'MS Sans Serif'
  Font.Style = []
  OldCreateOrder = False
  Position = poMainFormCenter
  OnClose = FormClose
  OnCloseQuery = FormCloseQuery
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object pnContent: TPanel
    Left = 0
    Top = 0
    Width = 799
    Height = 579
    Align = alClient
    BevelOuter = bvNone
    Color = clWhite
    TabOrder = 0
    object cxGroupBox: TcxGroupBox
      Left = 510
      Top = 0
      Align = alRight
      ParentBackground = False
      ParentColor = False
      Style.BorderColor = clMenuHighlight
      Style.BorderStyle = ebsThick
      Style.Color = clWindow
      Style.Shadow = False
      TabOrder = 0
      Height = 579
      Width = 289
      object Label1: TLabel
        Left = 12
        Top = 107
        Width = 54
        Height = 13
        Hint = 'Denumirea tipului de material'
        Caption = 'Denumire'
        FocusControl = edtDenumire
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label2: TLabel
        Left = 12
        Top = 148
        Width = 55
        Height = 13
        Hint = 'Descrierea tipului de material'
        Caption = 'Descriere'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label3: TLabel
        Left = 12
        Top = 15
        Width = 69
        Height = 13
        Hint = 'Identificatorul din nomenclator'
        Caption = 'Identificator'
        FocusControl = edtIdGestTipMaterial
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Bevel1: TBevel
        Left = 8
        Top = 354
        Width = 273
        Height = 6
        Shape = bsBottomLine
      end
      object Label11: TLabel
        Left = 10
        Top = 60
        Width = 23
        Height = 13
        Hint = 'Denumirea tipului de material'
        Caption = 'Cod'
        FocusControl = cxDBTextEdit1
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object Label4: TLabel
        Left = 14
        Top = 260
        Width = 121
        Height = 13
        Hint = 'Tipul de produs asociat'
        Caption = 'Data activare proiect'
        Font.Charset = DEFAULT_CHARSET
        Font.Color = clWindowText
        Font.Height = -11
        Font.Name = 'MS Sans Serif'
        Font.Style = [fsBold]
        ParentFont = False
      end
      object edtDenumire: TcxDBTextEdit
        Left = 22
        Top = 123
        Hint = 'Denumirea Proiectului'
        DataBinding.DataField = 'DENUMIRE'
        DataBinding.DataSource = DTOIProiecte
        Properties.ReadOnly = True
        Style.Color = 12910591
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 1
        Width = 250
      end
      object edtIdGestTipMaterial: TcxDBTextEdit
        Left = 22
        Top = 31
        Hint = 'Identificatorul din nomenclator'
        DataBinding.DataField = 'ID_OI_PROIECTE'
        DataBinding.DataSource = DTOIProiecte
        Properties.Alignment.Horz = taLeftJustify
        Properties.ReadOnly = True
        Style.Color = clSilver
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 0
        Width = 250
      end
      object edtSeAfiseaza: TcxDBCheckBox
        Left = 12
        Top = 234
        Hint = 
          'Starea proiectului  : activa(casuta bifata) sau incactiva(casuta' +
          ' nebifata)'
        Caption = 'Este Activ(X) sau Inactiv ()'
        DataBinding.DataField = 'STARE'
        DataBinding.DataSource = DTOIProiecte
        ParentFont = False
        Properties.NullStyle = nssUnchecked
        Properties.ReadOnly = True
        Properties.ValueGrayed = 'False'
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
        TabOrder = 3
        Width = 189
      end
      object edtDescriere: TcxDBMemo
        Left = 22
        Top = 160
        Hint = 'Descrierea tipului de material'
        DataBinding.DataField = 'DESCRIERE'
        DataBinding.DataSource = DTOIProiecte
        Properties.ReadOnly = True
        Style.Color = 12910591
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 2
        Height = 67
        Width = 248
      end
      object edtAreContabilitateProprie: TcxDBCheckBox
        Left = 12
        Top = 300
        Caption = 'Se tine contabilitate separata (x) Da ( ) Nu'
        DataBinding.DataField = 'ARE_CONTABILITATE'
        DataBinding.DataSource = DTOIProiecte
        ParentFont = False
        Properties.NullStyle = nssUnchecked
        Properties.ReadOnly = True
        Properties.ValueGrayed = 'False'
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
        TabOrder = 4
        Width = 269
      end
      object cxDBTextEdit1: TcxDBTextEdit
        Left = 22
        Top = 76
        Hint = 'Denumirea Proiectului'
        DataBinding.DataField = 'cod_proiect'
        DataBinding.DataSource = DTOIProiecte
        Properties.ReadOnly = True
        Style.Color = 12910591
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 5
        Width = 250
      end
      object cxDBCheckBox1: TcxDBCheckBox
        Left = 12
        Top = 324
        Caption = 'Credit Angajament (x) Da ( ) Nu'
        DataBinding.DataField = 'ESTE_CREDIT_ANGAJAMENT'
        DataBinding.DataSource = DTOIProiecte
        ParentFont = False
        Properties.NullStyle = nssUnchecked
        Properties.ReadOnly = True
        Properties.ValueGrayed = 'False'
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
        TabOrder = 6
        Width = 269
      end
      object edDataProiect: TcxDBDateEdit
        Left = 22
        Top = 275
        DataBinding.DataField = 'data_proiect'
        DataBinding.DataSource = DTOIProiecte
        Properties.InputKind = ikMask
        Properties.ReadOnly = True
        Style.Color = 12910591
        Style.LookAndFeel.Kind = lfOffice11
        StyleDisabled.LookAndFeel.Kind = lfOffice11
        StyleFocused.LookAndFeel.Kind = lfOffice11
        StyleHot.LookAndFeel.Kind = lfOffice11
        TabOrder = 7
        Width = 176
      end
    end
    object Panel1: TPanel
      Left = 0
      Top = 0
      Width = 510
      Height = 579
      Align = alClient
      BevelOuter = bvNone
      TabOrder = 1
      object TreeProiecte: TcxDBTreeList
        Left = 0
        Top = 49
        Width = 510
        Height = 167
        Align = alClient
        Bands = <
          item
          end>
        DataController.DataSource = DTOIProiecte
        DataController.ParentField = 'id_parinte'
        DataController.KeyField = 'id_oi_proiecte'
        DragMode = dmAutomatic
        LookAndFeel.Kind = lfOffice11
        OptionsBehavior.IncSearch = True
        OptionsCustomizing.ColumnsQuickCustomization = True
        OptionsData.Editing = False
        OptionsData.Deleting = False
        OptionsSelection.MultiSelect = True
        OptionsView.ColumnAutoWidth = True
        OptionsView.GridLineColor = clSilver
        OptionsView.GridLines = tlglBoth
        OptionsView.Indicator = True
        RootValue = -1
        TabOrder = 2
        OnCustomDrawDataCell = TreeProiecteCustomDrawDataCell
        OnDblClick = TreeProiecteDblClick
        OnFocusedNodeChanged = TreeProiecteFocusedNodeChanged
        object TreeProiecteid_oi_proiecte: TcxDBTreeListColumn
          Caption.Text = 'Id'
          DataBinding.FieldName = 'id_oi_proiecte'
          Width = 31
          Position.ColIndex = 5
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeProiectecod_proiect: TcxDBTreeListColumn
          Caption.Text = 'Cod'
          DataBinding.FieldName = 'cod_proiect'
          Width = 78
          Position.ColIndex = 0
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeProiecteDenumire: TcxDBTreeListColumn
          DataBinding.FieldName = 'Denumire'
          Width = 222
          Position.ColIndex = 1
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeProiecteid_parinte: TcxDBTreeListColumn
          Visible = False
          DataBinding.FieldName = 'id_parinte'
          Width = 20
          Position.ColIndex = 2
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeProiecteID_OI_TIPURI_PROIECTE: TcxDBTreeListColumn
          PropertiesClassName = 'TcxImageComboBoxProperties'
          Properties.Alignment.Horz = taLeftJustify
          Properties.Items = <>
          Caption.Text = 'Tip Proiect'
          DataBinding.FieldName = 'ID_OI_TIPURI_PROIECTE'
          Width = 123
          Position.ColIndex = 3
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
        object TreeProiecteSTARE: TcxDBTreeListColumn
          PropertiesClassName = 'TcxCheckBoxProperties'
          Visible = False
          Caption.Text = 'Stare'
          DataBinding.FieldName = 'STARE'
          Position.ColIndex = 4
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
      end
      object pnControl: TPanel
        Left = 0
        Top = 401
        Width = 510
        Height = 178
        Align = alBottom
        BevelOuter = bvNone
        TabOrder = 0
        object cxPageControl: TcxPageControl
          Left = 0
          Top = 6
          Width = 510
          Height = 172
          ActivePage = tabImplicit
          Align = alBottom
          LookAndFeel.Kind = lfOffice11
          Style = 9
          TabOrder = 0
          TabSlants.Kind = skCutCorner
          TabSlants.Positions = [spLeft, spRight]
          ClientRectBottom = 172
          ClientRectRight = 510
          ClientRectTop = 20
          object tabImplicit: TcxTabSheet
            Caption = 'Informatii Proiect'
            ImageIndex = 2
            object GridCF: TcxGrid
              Left = 0
              Top = 0
              Width = 510
              Height = 152
              Align = alClient
              TabOrder = 0
              LookAndFeel.Kind = lfOffice11
              object GridCFV: TcxGridDBTableView
                NavigatorButtons.ConfirmDelete = False
                DataController.DataSource = DTCF
                DataController.KeyFieldNames = 'ID_REPARTITORI_BUGET'
                DataController.Summary.DefaultGroupSummaryItems = <>
                DataController.Summary.FooterSummaryItems = <>
                DataController.Summary.SummaryGroups = <>
                OptionsData.CancelOnExit = False
                OptionsData.Deleting = False
                OptionsData.DeletingConfirmation = False
                OptionsData.Editing = False
                OptionsData.Inserting = False
                OptionsView.ColumnAutoWidth = True
                OptionsView.GroupByBox = False
                object GridCFVID_REPARTITORI_BUGET: TcxGridDBColumn
                  DataBinding.FieldName = 'ID_REPARTITORI_BUGET'
                  Visible = False
                end
                object GridCFVID_REPARTITORI: TcxGridDBColumn
                  DataBinding.FieldName = 'ID_REPARTITORI'
                  Visible = False
                end
                object GridCFVCOD_FUNCTIONAL: TcxGridDBColumn
                  Caption = 'Cod functional'
                  DataBinding.FieldName = 'COD_FUNCTIONAL'
                  Width = 90
                end
                object GridCFVID_OI_UNITATI: TcxGridDBColumn
                  Caption = 'Unitate'
                  DataBinding.FieldName = 'ID_OI_UNITATI'
                  PropertiesClassName = 'TcxLookupComboBoxProperties'
                  Properties.KeyFieldNames = 'id_oi_unitati'
                  Properties.ListColumns = <
                    item
                      FieldName = 'Denumire'
                    end>
                  Width = 68
                end
                object GridCFVCOD_ECONOMIC: TcxGridDBColumn
                  Caption = 'Cod economic'
                  DataBinding.FieldName = 'COD_ECONOMIC'
                  Width = 90
                end
                object GridCFVID_OI_PROIECTE: TcxGridDBColumn
                  DataBinding.FieldName = 'ID_OI_PROIECTE'
                  Visible = False
                end
              end
              object GridCFL: TcxGridLevel
                GridView = GridCFV
              end
            end
          end
          object tabBuget: TcxTabSheet
            Caption = 'Situatie Bugetara'
            ImageIndex = 0
            object GridBuget: TcxGrid
              Left = 0
              Top = 0
              Width = 481
              Height = 152
              Align = alLeft
              Anchors = [akLeft, akTop, akRight, akBottom]
              BevelInner = bvNone
              BevelOuter = bvNone
              TabOrder = 0
              LookAndFeel.Kind = lfOffice11
              object GridBugetV: TcxGridDBTableView
                NavigatorButtons.ConfirmDelete = False
                DataController.DataSource = DTBuget
                DataController.KeyFieldNames = 'id'
                DataController.Summary.DefaultGroupSummaryItems = <>
                DataController.Summary.FooterSummaryItems = <>
                DataController.Summary.SummaryGroups = <>
                OptionsData.Deleting = False
                OptionsData.DeletingConfirmation = False
                OptionsData.Editing = False
                OptionsData.Inserting = False
                OptionsView.ColumnAutoWidth = True
                OptionsView.GroupByBox = False
                object GridBugetVid: TcxGridDBColumn
                  Caption = 'Id'
                  DataBinding.FieldName = 'id'
                  Visible = False
                end
                object GridBugetVcod_functional: TcxGridDBColumn
                  Caption = 'Cod Functional'
                  DataBinding.FieldName = 'cod_functional'
                  OnGetCellHint = GridBugetVcod_functionalGetCellHint
                  Width = 76
                end
                object GridBugetVcod_economic: TcxGridDBColumn
                  Caption = 'Cod Economic'
                  DataBinding.FieldName = 'cod_economic'
                  OnGetCellHint = GridBugetVcod_economicGetCellHint
                  Width = 82
                end
                object GridBugetVid_bg_versiune: TcxGridDBColumn
                  DataBinding.FieldName = 'id_bg_versiune'
                  Visible = False
                end
                object GridBugetVid_oi_proiecte: TcxGridDBColumn
                  DataBinding.FieldName = 'id_oi_proiecte'
                  Visible = False
                end
                object GridBugetVan_fiscal: TcxGridDBColumn
                  Caption = 'An'
                  DataBinding.FieldName = 'an_fiscal'
                  Visible = False
                end
                object GridBugetVrevizie: TcxGridDBColumn
                  Caption = 'Revizie'
                  DataBinding.FieldName = 'revizie'
                  Visible = False
                end
                object GridBugetVplanificat1: TcxGridDBColumn
                  Caption = 'Planificat1'
                  DataBinding.FieldName = 'planificat1'
                  Width = 55
                end
                object GridBugetVplanificat2: TcxGridDBColumn
                  Caption = 'Planificat2'
                  DataBinding.FieldName = 'planificat2'
                  Width = 49
                end
                object GridBugetVplanificat3: TcxGridDBColumn
                  Caption = 'Planificat3'
                  DataBinding.FieldName = 'planificat3'
                  Width = 48
                end
                object GridBugetVplanificat4: TcxGridDBColumn
                  Caption = 'Planificat4'
                  DataBinding.FieldName = 'planificat4'
                  Width = 49
                end
                object GridBugetVplanificat: TcxGridDBColumn
                  Caption = 'Planificat'
                  DataBinding.FieldName = 'planificat'
                  Width = 48
                end
                object GridBugetVden_functional: TcxGridDBColumn
                  Caption = 'Functional'
                  DataBinding.FieldName = 'den_functional'
                  Visible = False
                end
                object GridBugetVden_economic: TcxGridDBColumn
                  Caption = 'Economic'
                  DataBinding.FieldName = 'den_economic'
                  Visible = False
                end
              end
              object GridBugetLevel: TcxGridLevel
                GridView = GridBugetV
              end
            end
          end
          object tabContabilitate: TcxTabSheet
            Caption = 'Situatie Contabila'
            ImageIndex = 1
            object GridConta: TcxGrid
              Left = 0
              Top = 0
              Width = 481
              Height = 152
              Align = alClient
              TabOrder = 0
              LookAndFeel.Kind = lfOffice11
              object GridContaV: TcxGridDBTableView
                NavigatorButtons.ConfirmDelete = False
                DataController.DataSource = DTSolduri
                DataController.Filter.MaxValueListCount = 1000
                DataController.KeyFieldNames = 'ID_SOLDURI_REPARTITORI'
                DataController.Summary.DefaultGroupSummaryItems = <>
                DataController.Summary.FooterSummaryItems = <>
                DataController.Summary.SummaryGroups = <>
                Filtering.ColumnPopup.MaxDropDownItemCount = 12
                OptionsData.CancelOnExit = False
                OptionsData.Deleting = False
                OptionsData.DeletingConfirmation = False
                OptionsData.Editing = False
                OptionsData.Inserting = False
                OptionsSelection.HideFocusRectOnExit = False
                OptionsSelection.InvertSelect = False
                OptionsView.ColumnAutoWidth = True
                OptionsView.GroupByBox = False
                OptionsView.GroupFooters = gfVisibleWhenExpanded
                Preview.AutoHeight = False
                Preview.MaxLineCount = 2
                object GridContaVCONT: TcxGridDBColumn
                  Caption = 'Cont Contabil'
                  DataBinding.FieldName = 'CONT'
                  HeaderAlignmentHorz = taCenter
                  MinWidth = 16
                  Options.Filtering = False
                  Width = 194
                end
                object GridContaVSOLD: TcxGridDBColumn
                  Caption = 'Sold'
                  DataBinding.FieldName = 'SOLD'
                  PropertiesClassName = 'TcxCurrencyEditProperties'
                  Properties.Alignment.Horz = taLeftJustify
                  Properties.AssignedValues.MaxValue = True
                  Properties.AssignedValues.MinValue = True
                  Properties.DecimalPlaces = 2
                  Properties.DisplayFormat = ',0.00;-,0.00'
                  Properties.Nullable = False
                  Properties.ReadOnly = True
                  HeaderAlignmentHorz = taCenter
                  Options.Filtering = False
                  Width = 124
                end
                object GridContaVSOLD_DEBITOR: TcxGridDBColumn
                  Caption = 'Sold Debit'
                  DataBinding.FieldName = 'SOLD_DEBITOR'
                  PropertiesClassName = 'TcxCurrencyEditProperties'
                  Properties.Alignment.Horz = taLeftJustify
                  Properties.AssignedValues.MaxValue = True
                  Properties.AssignedValues.MinValue = True
                  Properties.DecimalPlaces = 2
                  Properties.DisplayFormat = ',0.00;-,0.00'
                  Properties.Nullable = False
                  Properties.ReadOnly = True
                  Visible = False
                  Options.Filtering = False
                  Width = 55
                end
                object GridContaVSOLD_CREDITOR: TcxGridDBColumn
                  Caption = 'Sold Credit'
                  DataBinding.FieldName = 'SOLD_CREDITOR'
                  PropertiesClassName = 'TcxCurrencyEditProperties'
                  Properties.Alignment.Horz = taLeftJustify
                  Properties.AssignedValues.MaxValue = True
                  Properties.AssignedValues.MinValue = True
                  Properties.DecimalPlaces = 2
                  Properties.DisplayFormat = ',0.00;-,0.00'
                  Properties.Nullable = False
                  Properties.ReadOnly = True
                  Visible = False
                  Options.Filtering = False
                  Width = 57
                end
              end
              object GridContaL: TcxGridLevel
                GridView = GridContaV
              end
            end
          end
        end
      end
      object pnTop: TPanel
        Left = 0
        Top = 0
        Width = 510
        Height = 49
        Align = alTop
        BevelOuter = bvNone
        Color = clWindow
        TabOrder = 1
        OnResize = pnTopResize
        DesignSize = (
          510
          49)
        object Label13: TLabel
          Left = 9
          Top = 7
          Width = 77
          Height = 13
          Caption = '&Cod Functional: '
        end
        object lbl1: TLabel
          Left = 9
          Top = 29
          Width = 28
          Height = 13
          Caption = 'Filtru: '
        end
        object btn1: TSpeedButton
          Left = 367
          Top = 25
          Width = 22
          Height = 20
          Caption = 'X'
          OnClick = btn1Click
        end
        object btnRefresh: TcxButton
          Left = 400
          Top = 3
          Width = 70
          Height = 23
          Anchors = [akTop, akRight]
          Caption = 'Refresh'
          TabOrder = 0
          OnClick = btnRefreshClick
          Glyph.Data = {
            36030000424D3603000000000000360000002800000010000000100000000100
            1800000000000003000000000000000000000000000000000000000000000000
            000000272017080704382D1D58462E685438675438503F292A22160202010000
            000000000000000000000000000000000000004C3E2C9F8763D0BA8EE9D5A6ED
            DAA8EDD8A5E4CE9AC5AC7D89704C231C120000000000000000001B171216130F
            5C4D3ABAA687ECDEBDB1A18DA49584C4B59EF0E4C5F1E1BAEDD9A8E6D09C9E85
            5D1F19110000000000002A241D4B4033B19F86F4ECD6B3A69A83726B83726B82
            716A978880E6DFD2F6ECD4F0DFB6E4CE9A8E744F0E0B080000004A40369A8978
            F5EFE4FCF9F19E8F888E7C75D8D2D0F7F6F6E1DDDBC3BAB6F1EDE8F9F3E3F0E0
            B9DAC391624E33000000958373D6CEC6FEFEFDFFFEFEA5958EA4948EFBFAFAFE
            FDFDFFFFFFFFFFFFF6F5F4FDFCF8F6EED9EEDBAFB1976B0D0A07A39182F3F1EF
            FFFFFFFDFDFDB5A6A0A5938CC9BEBAFFFFFFFFFFFFFFFFFFDDD6D3FBFAF9FAF5
            E9F2E4C3CBB5882C2317AC9D8FFEFEFEFFFFFFD2C8C4A6938AA6928AD8CFCBFF
            FFFFFFFFFFFFFFFFC6B9B4C3B5B0FBF7F0F5EAD0D8C49E3B2F20B7A99DFEFDFD
            FFFFFFFFFFFFE7E1DFAC9A91EDE9E7FFFFFFFFFFFFFDFDFCAE9C93A69289C1B3
            A9EEE4D0D9C9A83B3022C2B6ABF4F2F1FFFFFFFFFFFFFFFFFFE2DCD8FBFAF9FF
            FFFFFFFFFFE2DCD8BCADA4A38F83E2DAD2F9F2E1CEBEA2272018C9BFB6E6E2DD
            FFFFFFFFFFFFFFFFFFFFFFFFF7F6F5FFFFFFFFFFFFFEFDFDDCD5D09D897BD8CF
            C6FAF4E6AE9B83070604D0C7BFD5CEC7FCFCFBFFFFFFFFFFFFFFFFFFD1C8C1E0
            DBD6FDFCFCFDFDFDB8AA9F978373D7CFC7E4DDD1605243000000D4CCC6D5CDC6
            E0DBD7FDFDFCFFFFFFFFFFFFFCFCFBC0B4AA95827194806F907B6A917D6CDFD9
            D48E7F710C0B09000000D5CDC7D5CDC7C1BCB7E0DCD8FCFCFBFFFFFFFFFFFFFF
            FFFFF4F2F0BFB4A9B1A497CFC6BE988B800E0C0B000000000000D5CDC7D5CDC7
            B9B4AFB9B5B1DAD5D1EBE7E4F7F5F3FBFAF9FAF9F8EEEBE7D0C8C07C726A100F
            0D000000000000000000D5CDC7D5CDC7B9B4AFB4B0ACB2AEAAA6A29E9E9A958D
            8883837D785954501B1918000000000000000000000000000000}
          LookAndFeel.Kind = lfOffice11
        end
        object edtFiltCodFunctional: TcxButtonEdit
          Left = 90
          Top = 3
          Hint = 'Codul Functional asociat proiectului.'
          Anchors = [akLeft, akTop, akRight]
          Properties.Buttons = <
            item
              Default = True
              Kind = bkEllipsis
            end
            item
              Caption = 'X'
              Kind = bkText
            end>
          Properties.OnButtonClick = edtFiltCodFunctionalPropertiesButtonClick
          Style.Color = 12910591
          Style.LookAndFeel.Kind = lfOffice11
          StyleDisabled.LookAndFeel.Kind = lfOffice11
          StyleFocused.LookAndFeel.Kind = lfOffice11
          StyleHot.LookAndFeel.Kind = lfOffice11
          TabOrder = 1
          Width = 303
        end
        object txtFiltrare: TcxTextEdit
          Left = 90
          Top = 25
          Properties.OnChange = txtFiltrarePropertiesChange
          TabOrder = 2
          Width = 277
        end
      end
      object pnlObiecte: TPanel
        Left = 0
        Top = 224
        Width = 510
        Height = 177
        Align = alBottom
        TabOrder = 3
        object gridObiecte: TcxGrid
          Left = 1
          Top = 23
          Width = 508
          Height = 131
          Align = alClient
          TabOrder = 0
          object vwObiecte: TcxGridDBTableView
            NavigatorButtons.ConfirmDelete = False
            DataController.DataSource = dsObiecte
            DataController.KeyFieldNames = 'idObiect'
            DataController.Summary.DefaultGroupSummaryItems = <>
            DataController.Summary.FooterSummaryItems = <>
            DataController.Summary.SummaryGroups = <>
            OptionsCustomize.ColumnFiltering = False
            OptionsCustomize.ColumnGrouping = False
            OptionsCustomize.ColumnMoving = False
            OptionsCustomize.ColumnSorting = False
            OptionsData.CancelOnExit = False
            OptionsData.Deleting = False
            OptionsData.DeletingConfirmation = False
            OptionsData.Editing = False
            OptionsData.Inserting = False
            OptionsView.ColumnAutoWidth = True
            OptionsView.GroupByBox = False
            object vwObiecteidObiect: TcxGridDBColumn
              DataBinding.FieldName = 'idObiect'
              Visible = False
              Width = 202
            end
            object vwObiecteDenumire: TcxGridDBColumn
              Caption = 'Obiecte'
              DataBinding.FieldName = 'Denumire'
              HeaderAlignmentHorz = taCenter
              Width = 408
            end
          end
          object lvObiecte: TcxGridLevel
            GridView = vwObiecte
          end
        end
        object pnl1: TPanel
          Left = 1
          Top = 154
          Width = 508
          Height = 22
          Align = alBottom
          BevelOuter = bvLowered
          TabOrder = 1
          object btn2: TcxButton
            Left = 4
            Top = 2
            Width = 113
            Height = 18
            Caption = 'Adaugati obiecte'
            TabOrder = 0
            OnClick = btn2Click
            Glyph.Data = {
              36090000424D3609000000000000360000002800000030000000100000000100
              1800000000000009000000000000000000000000000000000000FF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF00
              8000008000008000FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FF808080808080808080FF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00008000FF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFC0C0C0C0
              C0C0C0C0C0808080FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00FF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF00FF0000
              FF0000FF00008000FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFC0C0C0C0C0C0C0C0C0808080FF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF00FF0000
              FF0000FF00FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00008000FF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFC0C0C0C0
              C0C0C0C0C0808080FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00FF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF00FF0000
              FF0000FF00008000FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFC0C0C0C0C0C0C0C0C0808080FF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF00FF0000
              FF0000FF00FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              00800000800000800000800000FF0000FF0000FF000080000080000080000080
              00008000008000FF00FFFF00FFFF00FF808080808080808080808080C0C0C0C0
              C0C0C0C0C0808080808080808080808080808080808080FF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00FF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF0000FF0000FF0000FF0000
              FF0000FF0000FF0000FF0000FF0000FF0000FF00008000FF00FFFF00FFC0C0C0
              C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0
              C0C0C0C0808080FF00FFFF00FF00FF0000FF0000FF0000FF0000FF0000FF0000
              FF0000FF0000FF0000FF0000FF0000FF0000FF00FF00FFFF00FFFF00FF00FF00
              00FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF
              0000FF00008000FF00FFFF00FFC0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0
              C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0808080FF00FFFF00FF00FF00
              00FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF
              0000FF00FF00FFFF00FFFF00FF00FF0000FF0000FF0000FF0000FF0000FF0000
              FF0000FF0000FF0000FF0000FF0000FF0000FF00FF00FFFF00FFFF00FFC0C0C0
              C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0
              C0C0C0C0FF00FFFF00FFFF00FF00FF0000FF0000FF0000FF0000FF0000FF0000
              FF0000FF0000FF0000FF0000FF0000FF0000FF00FF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00008000FF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFC0C0C0C0
              C0C0C0C0C0808080FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00FF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF00FF0000
              FF0000FF00008000FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFC0C0C0C0C0C0C0C0C0808080FF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF00FF0000
              FF0000FF00FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00008000FF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFC0C0C0C0
              C0C0C0C0C0808080FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00FF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF00FF0000
              FF0000FF00008000FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFC0C0C0C0C0C0C0C0C0808080FF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF00FF0000
              FF0000FF00FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00FF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFC0C0C0C0
              C0C0C0C0C0FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00FF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF}
            NumGlyphs = 3
          end
        end
        object pnl2: TPanel
          Left = 1
          Top = 1
          Width = 508
          Height = 22
          Align = alTop
          BevelOuter = bvLowered
          TabOrder = 2
          object btn3: TcxButton
            Left = 3
            Top = 2
            Width = 62
            Height = 18
            TabOrder = 0
            OnClick = btn3Click
            Glyph.Data = {
              36090000424D3609000000000000360000002800000030000000100000000100
              1800000000000009000000000000000000000000000000000000FF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF00
              8000008000008000FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FF808080808080808080FF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00008000FF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFC0C0C0C0
              C0C0C0C0C0808080FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00FF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF00FF0000
              FF0000FF00008000FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFC0C0C0C0C0C0C0C0C0808080FF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF00FF0000
              FF0000FF00FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00008000FF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFC0C0C0C0
              C0C0C0C0C0808080FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00FF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF00FF0000
              FF0000FF00008000FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFC0C0C0C0C0C0C0C0C0808080FF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF00FF0000
              FF0000FF00FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              00800000800000800000800000FF0000FF0000FF000080000080000080000080
              00008000008000FF00FFFF00FFFF00FF808080808080808080808080C0C0C0C0
              C0C0C0C0C0808080808080808080808080808080808080FF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00FF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF0000FF0000FF0000FF0000
              FF0000FF0000FF0000FF0000FF0000FF0000FF00008000FF00FFFF00FFC0C0C0
              C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0
              C0C0C0C0808080FF00FFFF00FF00FF0000FF0000FF0000FF0000FF0000FF0000
              FF0000FF0000FF0000FF0000FF0000FF0000FF00FF00FFFF00FFFF00FF00FF00
              00FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF
              0000FF00008000FF00FFFF00FFC0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0
              C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0808080FF00FFFF00FF00FF00
              00FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF
              0000FF00FF00FFFF00FFFF00FF00FF0000FF0000FF0000FF0000FF0000FF0000
              FF0000FF0000FF0000FF0000FF0000FF0000FF00FF00FFFF00FFFF00FFC0C0C0
              C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0
              C0C0C0C0FF00FFFF00FFFF00FF00FF0000FF0000FF0000FF0000FF0000FF0000
              FF0000FF0000FF0000FF0000FF0000FF0000FF00FF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00008000FF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFC0C0C0C0
              C0C0C0C0C0808080FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00FF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF00FF0000
              FF0000FF00008000FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFC0C0C0C0C0C0C0C0C0808080FF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF00FF0000
              FF0000FF00FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00008000FF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFC0C0C0C0
              C0C0C0C0C0808080FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00FF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF00FF0000
              FF0000FF00008000FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFC0C0C0C0C0C0C0C0C0808080FF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF00FF0000
              FF0000FF00FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00FF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFC0C0C0C0
              C0C0C0C0C0FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FF00FF0000FF0000FF00FF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF}
            NumGlyphs = 3
          end
          object btnDelete: TcxButton
            Left = 68
            Top = 2
            Width = 62
            Height = 18
            TabOrder = 1
            OnClick = btnDeleteClick
            Glyph.Data = {
              36090000424D3609000000000000360000002800000030000000100000000100
              1800000000000009000000000000000000000000000000000000FF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              0000800000800000800000800000800000800000800000800000800000800000
              80000080000080FF00FFFF00FFFF00FF80808080808080808080808080808080
              8080808080808080808080808080808080808080808080FF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FF0000FF0000FF0000FF0000FF0000FF0000FF00
              00FF0000FF0000FF0000FF0000FF0000FF0000FF000080FF00FFFF00FFC0C0C0
              C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0
              C0C0C0C0808080FF00FFFF00FF0000FF0000FF0000FF0000FF0000FF0000FF00
              00FF0000FF0000FF0000FF0000FF0000FF0000FFFF00FFFF00FFFF00FF0000FF
              0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000
              FF0000FF000080FF00FFFF00FFC0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0
              C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0808080FF00FFFF00FF0000FF
              0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000FF0000
              FF0000FFFF00FFFF00FFFF00FF0000FF0000FF0000FF0000FF0000FF0000FF00
              00FF0000FF0000FF0000FF0000FF0000FF0000FFFF00FFFF00FFFF00FFC0C0C0
              C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0C0
              C0C0C0C0FF00FFFF00FFFF00FF0000FF0000FF0000FF0000FF0000FF0000FF00
              00FF0000FF0000FF0000FF0000FF0000FF0000FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF
              FF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00
              FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF
              00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FFFF00FF}
            NumGlyphs = 3
          end
        end
      end
      object split1: TcxSplitter
        Left = 0
        Top = 216
        Width = 510
        Height = 8
        HotZoneClassName = 'TcxSimpleStyle'
        AlignSplitter = salBottom
        Control = pnlObiecte
      end
    end
  end
  object DTOIProiecte: TDataSource
    DataSet = qryOIProiecte
    Left = 336
    Top = 104
  end
  object DTOITipuriProiecte: TDataSource
    DataSet = qryOITipuriProiecte
    Left = 312
    Top = 224
  end
  object DTBuget: TDataSource
    DataSet = qryBuget
    Left = 105
    Top = 505
  end
  object DTSolduri: TDataSource
    DataSet = qrySolduri
    Left = 201
    Top = 505
  end
  object DTCF: TDataSource
    DataSet = qryCF
    Left = 9
    Top = 505
  end
  object cxGridPopupMenu: TcxGridPopupMenu
    PopupMenus = <>
    Left = 32
    Top = 112
  end
  object qryOIProiecte: TADOQuery
    Connection = dtmMain.dbContaConnection
    OnFilterRecord = qryOIProiecteFilterRecord
    Parameters = <>
    SQL.Strings = (
      ' SELECT * FROM OI_PROIECTE'
      ''
      ''
      '')
    Left = 368
    Top = 104
  end
  object qryOITipuriProiecte: TADOQuery
    Connection = dtmMain.dbContaConnection
    Parameters = <>
    SQL.Strings = (
      'exec  SP_OI_GET_TIPURI_PROIECTE')
    Left = 344
    Top = 224
  end
  object qryCF: TADOQuery
    Connection = dtmMain.dbContaConnection
    Parameters = <
      item
        Name = 'ID_PROIECTE'
        Attributes = [paSigned, paNullable]
        DataType = ftInteger
        Precision = 10
        Size = 4
        Value = Null
      end>
    SQL.Strings = (
      
        'SELECT * FROM Contabilitate_2013..REPARTITORI_BUGET WHERE ID_REP' +
        'ARTITORI =  :ID_PROIECTE')
    Left = 40
    Top = 504
  end
  object qryBuget: TADOQuery
    Connection = dtmMain.dbContaConnection
    Parameters = <
      item
        Name = 'ID_PROIECTE'
        Attributes = [paSigned, paNullable]
        DataType = ftInteger
        Precision = 10
        Size = 4
        Value = Null
      end>
    SQL.Strings = (
      'EXEC Contabilitate_2013..spProiectInfoBuget :ID_PROIECTE')
    Left = 136
    Top = 504
  end
  object qrySolduri: TADOQuery
    Connection = dtmMain.dbContaConnection
    Parameters = <
      item
        Name = 'ID_PROIECTE'
        Attributes = [paSigned, paNullable]
        DataType = ftInteger
        Precision = 10
        Size = 4
        Value = Null
      end>
    SQL.Strings = (
      
        'SELECT * FROM Contabilitate_2013..SOLDURI_REPARTITORI WHERE ID_R' +
        'EPARTITORI =  :ID_PROIECTE')
    Left = 240
    Top = 504
  end
  object mdtObiecte: TdxMemData
    Active = True
    Indexes = <>
    SortOptions = []
    Left = 32
    Top = 208
    object intgrfldObiecteidObiect: TIntegerField
      FieldName = 'idObiect'
    end
    object strngfldObiecteDenumire: TStringField
      FieldName = 'Denumire'
      Size = 200
    end
  end
  object dsObiecte: TDataSource
    DataSet = mdtObiecte
    Left = 72
    Top = 208
  end
end
