object frmOIProiecteInvest: TfrmOIProiecteInvest
  Left = 315
  Top = 89
  Caption = 'Intretinere Proiecte'
  ClientHeight = 590
  ClientWidth = 807
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
    Width = 807
    Height = 590
    Align = alClient
    BevelOuter = bvNone
    Color = clWhite
    TabOrder = 0
    object cxGroupBox: TcxGroupBox
      Left = 518
      Top = 0
      Align = alRight
      ParentBackground = False
      ParentColor = False
      Style.BorderColor = clMenuHighlight
      Style.BorderStyle = ebsThick
      Style.Color = clWindow
      Style.Shadow = False
      TabOrder = 0
      Height = 590
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
      end
      object edDataProiect: TcxDBDateEdit
        Left = 22
        Top = 275
        DataBinding.DataField = 'data_proiect'
        DataBinding.DataSource = DTOIProiecte
        Properties.InputKind = ikMask
        Properties.ReadOnly = True
        Properties.SaveTime = False
        Properties.ShowTime = False
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
      Width = 518
      Height = 590
      Align = alClient
      BevelOuter = bvNone
      TabOrder = 1
      object TreeProiecte: TcxDBTreeList
        Left = 0
        Top = 49
        Width = 518
        Height = 178
        Align = alClient
        Bands = <
          item
          end>
        DataController.DataSource = DTOIProiecte
        DataController.ParentField = 'id_parinte'
        DataController.KeyField = 'id_oi_proiecte'
        DragMode = dmAutomatic
        LookAndFeel.Kind = lfOffice11
        Navigator.Buttons.CustomButtons = <>
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
        ScrollbarAnnotations.CustomAnnotations = <>
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
          Width = 100
          Position.ColIndex = 4
          Position.RowIndex = 0
          Position.BandIndex = 0
          Summary.FooterSummaryItems = <>
          Summary.GroupFooterSummaryItems = <>
        end
      end
      object pnControl: TPanel
        Left = 0
        Top = 412
        Width = 518
        Height = 178
        Align = alBottom
        BevelOuter = bvNone
        TabOrder = 0
        object cxPageControl: TcxPageControl
          Left = 0
          Top = 6
          Width = 518
          Height = 172
          Align = alBottom
          TabOrder = 0
          Properties.ActivePage = tabImplicit
          Properties.CustomButtons.Buttons = <>
          Properties.Style = 9
          Properties.TabSlants.Kind = skCutCorner
          Properties.TabSlants.Positions = [spLeft, spRight]
          LookAndFeel.Kind = lfOffice11
          ClientRectBottom = 172
          ClientRectRight = 518
          ClientRectTop = 20
          object tabImplicit: TcxTabSheet
            Caption = 'Informatii Proiect'
            ImageIndex = 2
            object GridCF: TcxGrid
              Left = 0
              Top = 0
              Width = 518
              Height = 152
              Align = alClient
              TabOrder = 0
              LookAndFeel.Kind = lfOffice11
              object GridCFV: TcxGridDBTableView
                Navigator.Buttons.CustomButtons = <>
                ScrollbarAnnotations.CustomAnnotations = <>
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
            ExplicitTop = 0
            ExplicitWidth = 0
            ExplicitHeight = 0
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
                Navigator.Buttons.CustomButtons = <>
                ScrollbarAnnotations.CustomAnnotations = <>
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
            ExplicitTop = 0
            ExplicitWidth = 0
            ExplicitHeight = 0
            object GridConta: TcxGrid
              Left = 0
              Top = 0
              Width = 481
              Height = 152
              Align = alClient
              TabOrder = 0
              LookAndFeel.Kind = lfOffice11
              object GridContaV: TcxGridDBTableView
                Navigator.Buttons.CustomButtons = <>
                ScrollbarAnnotations.CustomAnnotations = <>
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
        Width = 518
        Height = 49
        Align = alTop
        BevelOuter = bvNone
        Color = clWindow
        TabOrder = 1
        OnResize = pnTopResize
        DesignSize = (
          518
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
          LookAndFeel.Kind = lfOffice11
          OptionsImage.Glyph.SourceDPI = 96
          OptionsImage.Glyph.Data = {
            424D360400000000000036000000280000001000000010000000010020000000
            000000000000C40E0000C40E0000000000000000000000000000000000000000
            0000272017FF080704FF382D1DFF58462EFF685438FF675438FF503F29FF2A22
            16FF020201FF0000000000000000000000000000000000000000000000000000
            00004C3E2CFF9F8763FFD0BA8EFFE9D5A6FFEDDAA8FFEDD8A5FFE4CE9AFFC5AC
            7DFF89704CFF231C12FF0000000000000000000000001B1712FF16130FFF5C4D
            3AFFBAA687FFECDEBDFFB1A18DFFA49584FFC4B59EFFF0E4C5FFF1E1BAFFEDD9
            A8FFE6D09CFF9E855DFF1F1911FF00000000000000002A241DFF4B4033FFB19F
            86FFF4ECD6FFB3A69AFF83726BFF83726BFF82716AFF978880FFE6DFD2FFF6EC
            D4FFF0DFB6FFE4CE9AFF8E744FFF0E0B08FF000000004A4036FF9A8978FFF5EF
            E4FFFCF9F1FF9E8F88FF8E7C75FFD8D2D0FFF7F6F6FFE1DDDBFFC3BAB6FFF1ED
            E8FFF9F3E3FFF0E0B9FFDAC391FF624E33FF00000000958373FFD6CEC6FFFEFE
            FDFFFFFEFEFFA5958EFFA4948EFFFBFAFAFFFEFDFDFFFFFFFFFFFFFFFFFFF6F5
            F4FFFDFCF8FFF6EED9FFEEDBAFFFB1976BFF0D0A07FFA39182FFF3F1EFFFFFFF
            FFFFFDFDFDFFB5A6A0FFA5938CFFC9BEBAFFFFFFFFFFFFFFFFFFFFFFFFFFDDD6
            D3FFFBFAF9FFFAF5E9FFF2E4C3FFCBB588FF2C2317FFAC9D8FFFFEFEFEFFFFFF
            FFFFD2C8C4FFA6938AFFA6928AFFD8CFCBFFFFFFFFFFFFFFFFFFFFFFFFFFC6B9
            B4FFC3B5B0FFFBF7F0FFF5EAD0FFD8C49EFF3B2F20FFB7A99DFFFEFDFDFFFFFF
            FFFFFFFFFFFFE7E1DFFFAC9A91FFEDE9E7FFFFFFFFFFFFFFFFFFFDFDFCFFAE9C
            93FFA69289FFC1B3A9FFEEE4D0FFD9C9A8FF3B3022FFC2B6ABFFF4F2F1FFFFFF
            FFFFFFFFFFFFFFFFFFFFE2DCD8FFFBFAF9FFFFFFFFFFFFFFFFFFE2DCD8FFBCAD
            A4FFA38F83FFE2DAD2FFF9F2E1FFCEBEA2FF272018FFC9BFB6FFE6E2DDFFFFFF
            FFFFFFFFFFFFFFFFFFFFFFFFFFFFF7F6F5FFFFFFFFFFFFFFFFFFFEFDFDFFDCD5
            D0FF9D897BFFD8CFC6FFFAF4E6FFAE9B83FF070604FFD0C7BFFFD5CEC7FFFCFC
            FBFFFFFFFFFFFFFFFFFFFFFFFFFFD1C8C1FFE0DBD6FFFDFCFCFFFDFDFDFFB8AA
            9FFF978373FFD7CFC7FFE4DDD1FF605243FF00000000D4CCC6FFD5CDC6FFE0DB
            D7FFFDFDFCFFFFFFFFFFFFFFFFFFFCFCFBFFC0B4AAFF958271FF94806FFF907B
            6AFF917D6CFFDFD9D4FF8E7F71FF0C0B09FF00000000D5CDC7FFD5CDC7FFC1BC
            B7FFE0DCD8FFFCFCFBFFFFFFFFFFFFFFFFFFFFFFFFFFF4F2F0FFBFB4A9FFB1A4
            97FFCFC6BEFF988B80FF0E0C0BFF0000000000000000D5CDC7FFD5CDC7FFB9B4
            AFFFB9B5B1FFDAD5D1FFEBE7E4FFF7F5F3FFFBFAF9FFFAF9F8FFEEEBE7FFD0C8
            C0FF7C726AFF100F0DFF000000000000000000000000D5CDC7FFD5CDC7FFB9B4
            AFFFB4B0ACFFB2AEAAFFA6A29EFF9E9A95FF8D8883FF837D78FF595450FF1B19
            18FF0000000000000000000000000000000000000000}
          TabOrder = 0
          OnClick = btnRefreshClick
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
        Top = 235
        Width = 518
        Height = 177
        Align = alBottom
        TabOrder = 3
        object gridObiecte: TcxGrid
          Left = 1
          Top = 23
          Width = 516
          Height = 131
          Align = alClient
          TabOrder = 0
          object vwObiecte: TcxGridDBTableView
            Navigator.Buttons.CustomButtons = <>
            ScrollbarAnnotations.CustomAnnotations = <>
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
          Width = 516
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
            OptionsImage.Glyph.SourceDPI = 96
            OptionsImage.Glyph.Data = {
              424D360C00000000000036000000280000003000000010000000010020000000
              000000000000C40E0000C40E00000000000000000000FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00008000FF008000FF008000FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00808080FF808080FF808080FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FF008000FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00C0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FF008000FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00C0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FF008000FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00C0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FF008000FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00C0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF000080
              00FF008000FF008000FF008000FF00FF00FF00FF00FF00FF00FF008000FF0080
              00FF008000FF008000FF008000FF008000FFFF00FF00FF00FF00FF00FF008080
              80FF808080FF808080FF808080FFC0C0C0FFC0C0C0FFC0C0C0FF808080FF8080
              80FF808080FF808080FF808080FF808080FFFF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FF008000FFFF00FF00FF00FF00C0C0C0FFC0C0
              C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0
              C0FFC0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00FF00FF00FF0000FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FFFF00FF00FF00FF00FF00FF0000FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FF008000FFFF00FF00FF00FF00C0C0C0FFC0C0
              C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0
              C0FFC0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00FF00FF00FF0000FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FFFF00FF00FF00FF00FF00FF0000FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FFFF00FF00FF00FF00FF00FF00C0C0C0FFC0C0
              C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0
              C0FFC0C0C0FFC0C0C0FFC0C0C0FFFF00FF00FF00FF00FF00FF0000FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FFFF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FF008000FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00C0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FF008000FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00C0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FF008000FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00C0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FF008000FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00C0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00C0C0C0FFC0C0C0FFC0C0C0FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00}
            OptionsImage.NumGlyphs = 3
            TabOrder = 0
            OnClick = btn2Click
          end
        end
        object pnl2: TPanel
          Left = 1
          Top = 1
          Width = 516
          Height = 22
          Align = alTop
          BevelOuter = bvLowered
          TabOrder = 2
          object btn3: TcxButton
            Left = 3
            Top = 2
            Width = 62
            Height = 18
            OptionsImage.Glyph.SourceDPI = 96
            OptionsImage.Glyph.Data = {
              424D360C00000000000036000000280000003000000010000000010020000000
              000000000000C40E0000C40E00000000000000000000FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00008000FF008000FF008000FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00808080FF808080FF808080FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FF008000FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00C0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FF008000FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00C0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FF008000FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00C0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FF008000FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00C0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF000080
              00FF008000FF008000FF008000FF00FF00FF00FF00FF00FF00FF008000FF0080
              00FF008000FF008000FF008000FF008000FFFF00FF00FF00FF00FF00FF008080
              80FF808080FF808080FF808080FFC0C0C0FFC0C0C0FFC0C0C0FF808080FF8080
              80FF808080FF808080FF808080FF808080FFFF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FF008000FFFF00FF00FF00FF00C0C0C0FFC0C0
              C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0
              C0FFC0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00FF00FF00FF0000FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FFFF00FF00FF00FF00FF00FF0000FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FF008000FFFF00FF00FF00FF00C0C0C0FFC0C0
              C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0
              C0FFC0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00FF00FF00FF0000FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FFFF00FF00FF00FF00FF00FF0000FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FFFF00FF00FF00FF00FF00FF00C0C0C0FFC0C0
              C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0
              C0FFC0C0C0FFC0C0C0FFC0C0C0FFFF00FF00FF00FF00FF00FF0000FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF
              00FF00FF00FF00FF00FF00FF00FFFF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FF008000FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00C0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FF008000FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00C0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FF008000FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00C0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FF008000FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00C0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00C0C0C0FFC0C0C0FFC0C0C0FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF0000FF00FF00FF00FF00FF00FFFF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00}
            OptionsImage.NumGlyphs = 3
            TabOrder = 0
            OnClick = btn3Click
          end
          object btnDelete: TcxButton
            Left = 68
            Top = 2
            Width = 62
            Height = 18
            OptionsImage.Glyph.SourceDPI = 96
            OptionsImage.Glyph.Data = {
              424D360C00000000000036000000280000003000000010000000010020000000
              000000000000C40E0000C40E00000000000000000000FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF000000
              80FF000080FF000080FF000080FF000080FF000080FF000080FF000080FF0000
              80FF000080FF000080FF000080FF000080FFFF00FF00FF00FF00FF00FF008080
              80FF808080FF808080FF808080FF808080FF808080FF808080FF808080FF8080
              80FF808080FF808080FF808080FF808080FFFF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF000000FFFF0000
              FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000
              FFFF0000FFFF0000FFFF0000FFFF000080FFFF00FF00FF00FF00C0C0C0FFC0C0
              C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0
              C0FFC0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00FF00FF00FF000000FFFF0000
              FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000
              FFFF0000FFFF0000FFFF0000FFFFFF00FF00FF00FF00FF00FF000000FFFF0000
              FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000
              FFFF0000FFFF0000FFFF0000FFFF000080FFFF00FF00FF00FF00C0C0C0FFC0C0
              C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0
              C0FFC0C0C0FFC0C0C0FFC0C0C0FF808080FFFF00FF00FF00FF000000FFFF0000
              FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000
              FFFF0000FFFF0000FFFF0000FFFFFF00FF00FF00FF00FF00FF000000FFFF0000
              FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000
              FFFF0000FFFF0000FFFF0000FFFFFF00FF00FF00FF00FF00FF00C0C0C0FFC0C0
              C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0C0FFC0C0
              C0FFC0C0C0FFC0C0C0FFC0C0C0FFFF00FF00FF00FF00FF00FF000000FFFF0000
              FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000FFFF0000
              FFFF0000FFFF0000FFFF0000FFFFFF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00
              FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00FF00}
            OptionsImage.NumGlyphs = 3
            TabOrder = 1
            OnClick = btnDeleteClick
          end
        end
      end
      object split1: TcxSplitter
        Left = 0
        Top = 227
        Width = 518
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
    DataSet = QrySolduri
    Left = 201
    Top = 505
  end
  object DTCF: TDataSource
    DataSet = QryCF
    Left = 9
    Top = 505
  end
  object cxGridPopupMenu: TcxGridPopupMenu
    PopupMenus = <>
    Left = 32
    Top = 112
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
  object QryCF: TZQuery
    Connection = frmData.dbContabilitate
    AutoCalcFields = False
    SQL.Strings = (
      
        'SELECT * FROM REPARTITORI_BUGET WHERE ID_REPARTITORI =  :ID_PROI' +
        'ECTE')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_PROIECTE'
        ParamType = ptUnknown
        Size = 4
        Value = 1
      end>
    Left = 41
    Top = 505
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_PROIECTE'
        ParamType = ptUnknown
        Size = 4
        Value = 1
      end>
  end
  object qryBuget: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'EXEC spProiectInfoBuget :ID_PROIECTE')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_PROIECTE'
        ParamType = ptUnknown
        Size = 4
        Value = 1
      end>
    Left = 137
    Top = 505
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_PROIECTE'
        ParamType = ptUnknown
        Size = 4
        Value = 1
      end>
  end
  object QrySolduri: TZQuery
    Connection = frmData.dbContabilitate
    AutoCalcFields = False
    SQL.Strings = (
      
        'SELECT * FROM SOLDURI_REPARTITORI WHERE ID_REPARTITORI =  :ID_PR' +
        'OIECTE')
    Params = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_PROIECTE'
        ParamType = ptUnknown
        Size = 4
        Value = 1
      end>
    Left = 233
    Top = 505
    ParamData = <
      item
        DataType = ftInteger
        Precision = 10
        Name = 'ID_PROIECTE'
        ParamType = ptUnknown
        Size = 4
        Value = 1
      end>
  end
  object qryOITipuriProiecte: TZQuery
    Connection = frmData.dbContabilitate
    SQL.Strings = (
      'exec  SP_OI_GET_TIPURI_PROIECTE')
    Params = <>
    Left = 344
    Top = 224
  end
  object qryOIProiecte: TZQuery
    Connection = frmData.dbContabilitate
    UpdateObject = usOIProiecte
    SQL.Strings = (
      'SELECT * FROM OI_PROIECTE'
      '')
    Params = <>
    Left = 369
    Top = 103
  end
  object usOIProiecte: TZUpdateSQL
    DeleteSQL.Strings = (
      'DELETE FROM OI_PROIECTE'
      'WHERE'
      
        '  ((OI_PROIECTE.id_oi_proiecte IS NULL AND :OLD_id_oi_proiecte I' +
        'S '
      'NULL) OR (OI_PROIECTE.id_oi_proiecte = :OLD_id_oi_proiecte))')
    InsertSQL.Strings = (
      'exec spOIProiecteAdd '#39'<Proiect Nou>'#39', NULL')
    ModifySQL.Strings = (
      'UPDATE OI_PROIECTE SET'
      '  Denumire = :Denumire,'
      '  id_parinte = :id_parinte,'
      '  cod_functional = :cod_functional,'
      '  ID_OI_TIPURI_PROIECTE = :ID_OI_TIPURI_PROIECTE,'
      '  DESCRIERE = :DESCRIERE,'
      '  STARE = :STARE,'
      '  SHAPE_TYPE = :SHAPE_TYPE,'
      '  SHAPE_COLOR = :SHAPE_COLOR,'
      '  SHAPE_LEFT_TOP = :SHAPE_LEFT_TOP,'
      '  SHAPE_RIGHT_BOTTOM = :SHAPE_RIGHT_BOTTOM,'
      '  SHAPE_POS_ID = :SHAPE_POS_ID,'
      '  SHAPE_FONT_COLOR = :SHAPE_FONT_COLOR,'
      '  SHAPE_FONT_NAME = :SHAPE_FONT_NAME,'
      '  SUMA_SOLD = :SUMA_SOLD,'
      '  ARE_CONTABILITATE = :ARE_CONTABILITATE,'
      '  cod_proiect = :cod_proiect,'
      '  id_oi_unitati = :id_oi_unitati,'
      '  ESTE_CREDIT_ANGAJAMENT = :ESTE_CREDIT_ANGAJAMENT,'
      '  id_oi_proiecte_detalii = :id_oi_proiecte_detalii,'
      '  data_proiect = :data_proiect'
      'WHERE'
      
        '  ((OI_PROIECTE.id_oi_proiecte IS NULL AND :OLD_id_oi_proiecte I' +
        'S '
      'NULL) OR (OI_PROIECTE.id_oi_proiecte = :OLD_id_oi_proiecte))')
    UseSequenceFieldForRefreshSQL = False
    Left = 402
    Top = 105
    ParamData = <
      item
        DataType = ftUnknown
        Name = 'Denumire'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'id_parinte'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'cod_functional'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'ID_OI_TIPURI_PROIECTE'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'DESCRIERE'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'STARE'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'SHAPE_TYPE'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'SHAPE_COLOR'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'SHAPE_LEFT_TOP'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'SHAPE_RIGHT_BOTTOM'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'SHAPE_POS_ID'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'SHAPE_FONT_COLOR'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'SHAPE_FONT_NAME'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'SUMA_SOLD'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'ARE_CONTABILITATE'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'cod_proiect'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'ID_OI_UNITATI'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'ESTE_CREDIT_ANGAJAMENT'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'id_oi_proiecte_detalii'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'data_proiect'
        ParamType = ptUnknown
      end
      item
        DataType = ftUnknown
        Name = 'OLD_id_oi_proiecte'
        ParamType = ptUnknown
      end>
  end
end
